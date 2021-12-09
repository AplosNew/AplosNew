using Library.Core;
using Library.Data;
using Library.Data.Sql;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.ViewModel.Accounts;
using Library.ViewModel.Organizations;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Library.Service.Expenses
{
    public class ExpenseDashboardService : IExpenseDashboardService
    {
        private readonly ISqlRepository _sqlRepository;

        public ExpenseDashboardService(ISqlRepository sqlRepository)
        {
            _sqlRepository = sqlRepository;
        }
        public IEnumerable<object> GetCompanyInformation(string companyGroupId, string companyId)
        {
            try
            {
                var sql = @"SELECT CURR.Code BaseCurrencyCode,* FROM Org.Company CMP
                            INNER JOIN SCS.Currency CURR ON CURR.Id = CMP.BaseCurrencyId
                        WHERE CompanyGroupId = '" + companyGroupId + @"' AND CMP.Id = '" + companyId + @"'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetVoucherLatestDate(string compnayGroupId, string companyId, string plantId, string dateType, string itemType)
        {
            try
            {
                var sql = @"SELECT TOP(1) Replace(CONVERT(VARCHAR(11), V." + dateType + @", 106), ' ', '-') AS PostingDate

                        FROM TRN.VoucherDetail AS VD
                        JOIN TRN.Voucher AS V ON V.Id = VD.VoucherId
                        LEFT JOIN HKP.GLGeneralInfo AS GL ON GL.Id = VD.GLGeneralInfoId

                        LEFT JOIN HKP.AccountGroup AS AG ON AG.Id = GL.AccountGroupId

                        LEFT JOIN HKP.AccountType AS ACNT ON ACNT.Id = AG.AccountTypeId
                        WHERE ACNT.IsBalanceSheet = " + itemType + " AND v.IsPark = 0 ORDER BY V." + dateType + " DESC";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public IEnumerable<object> GetFiscalYearForBarChart(string fromDate, string toDate)
        {
            try
            {
                DateTime firstdate = Convert.ToDateTime(fromDate);
                DateTime lastdate = Convert.ToDateTime(toDate);

                var firstDateOfMonth = new DateTime(firstdate.Year, firstdate.Month, 1);
                var lastDateOfMonth = new DateTime(lastdate.Year, lastdate.Month, 1).AddMonths(1).AddDays(-1);
                string sqlText = @"SELECT Id AS FiscalYearPeriodId, PeriodName, StartDate, EndDate FROM SCS.FiscalYearPeriod where StartDate Between '" + firstDateOfMonth.ToString("dd-MMM-yyyy") + @"' and '" + lastDateOfMonth.ToString("dd-MMM-yyyy") + @"' 
                                and EndDate Between '" + firstDateOfMonth.ToString("dd-MMM-yyyy") + @"' and '" + lastDateOfMonth.ToString("dd-MMM-yyyy") + @"' ";

                return _sqlRepository.GetDataCollection(sqlText);
            }
            catch (Exception ex)
            {
                throw ex;
                throw;
            }
        }

        public IEnumerable<object> OrgStructureList(string companyGroupId, string companyId)
        {
            try
            {
                var sql = @"SELECT  DISTINCT StandardName,UserName ColumnName,ISNULL(RType,'position') AS RType,Sequence from [ORG].[StructureRelationship]  where CompanyGroupId='" + companyGroupId + @"'
							AND ( CompanyId Is null or CompanyId='" + companyId + @"') and  RType = 'Entity'  order by Sequence";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public IEnumerable<OrgStructureListViewModel> OrgStructureListM(string companyGroupId, string companyId)
        {
            try
            {
                var sql = @"SELECT  DISTINCT StandardName,UserName ColumnName,ISNULL(RType,'position') AS RType,Sequence from [ORG].[StructureRelationship]  where CompanyGroupId='" + companyGroupId + @"'
							and ( CompanyId Is null or CompanyId='" + companyId + @"') and  RType = 'Entity'  order by Sequence";

                return _sqlRepository.GetModelCollection<OrgStructureListViewModel>(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public IEnumerable<object> ExpenseList(string companyGroupId, string companyId, string factDate, string fromDate, string toDate)
        {
            try
            {
                var expFactDate = string.Empty;
                var condition = string.Empty;
                var delayPosting = string.Empty;
                var fortheDaycondition = string.Empty;
                var forthePeriodCondition = string.Empty;
                expFactDate = "AND CONVERT(DATE,V.PostingDate) <= CONVERT(DATE,GETDATE())";



                if (factDate == "postingDate")
                {
                    condition = "AND V.PostingDate BETWEEN CONVERT(DATE,'" + fromDate + @"') AND CONVERT(DATE,'" + toDate + @"')";
                    fortheDaycondition = "AND V.PostingDate BETWEEN CONVERT(DATE,'" + toDate + @"') AND CONVERT(DATE,'" + toDate + @"')";
                    forthePeriodCondition = @" AND V.PostingDate BETWEEN (SELECT  StartDate FROM SCS.FiscalYearPeriod where '" + toDate + @"' between StartDate and EndDate)
                                              AND(SELECT  EndDate FROM SCS.FiscalYearPeriod where '" + toDate + @"' between StartDate and EndDate)";
                    delayPosting = @"AND Convert(date,V.AddedDate) = CONVERT(DATE,'" + toDate + @"') AND MONTH(V.PostingDate) < MONTH('" + toDate + @"') AND YEAR(V.PostingDate) <= YEAR('" + toDate + @"')";

                }
                if (factDate == "AddedDate")
                {
                    condition = "AND V.AddedDate BETWEEN CONVERT(DATE,'" + fromDate + @"') AND CONVERT(DATE,'" + toDate + @"')";
                    fortheDaycondition = "AND V.AddedDate BETWEEN CONVERT(DATE, '" + toDate + @"') AND CONVERT(DATE,'" + toDate + @"')";
                    forthePeriodCondition = @" AND V.AddedDate BETWEEN (SELECT  StartDate FROM SCS.FiscalYearPeriod where '" + toDate + @"' between StartDate and EndDate)
                                              AND(SELECT  EndDate FROM SCS.FiscalYearPeriod where '" + toDate + @"' between StartDate and EndDate)";
                    //delayPosting = "@AND V.AddedDate = CONVERT(DATE,'" + toDate + @"') AND MONTH(V.AddedDate) < MONTH('" + toDate + @"') AND YEAR(V.AddedDate) <= YEAR('" + toDate + @"')";
                    delayPosting = @"AND Convert(date,V.AddedDate) = CONVERT(DATE,'" + toDate + @"') AND MONTH(V.PostingDate) < MONTH('" + toDate + @"') AND YEAR(V.PostingDate) <= YEAR('" + toDate + @"')";

                }
                
                var sql = @"SELECT * FROM (SELECT CMP.CompanyGroupId,CMPGR.UserName GroupName,CMP.Id CompanyId,CMP.UserName ColumnName
                                    
								   ,SUM(CASE WHEN ISNULL(Expense.DRcumulative, 0) = 0 THEN Expense.CRcumulative ELSE Expense.DRcumulative END) ExpenseAmount
	                               ,SUM(CASE WHEN ISNULL(Revenue.DRcumulative, 0) = 0 THEN Revenue.CRcumulative ELSE Revenue.DRcumulative END) RevenueAmount
	                               ,SUM(CASE WHEN ISNULL(ForTheDayRevenue.DRcumulative, 0) = 0 THEN ForTheDayRevenue.CRcumulative ELSE ForTheDayRevenue.DRcumulative END) ForTheDayRevenueAmount
	                               ,SUM(CASE WHEN ISNULL(ForTheDayExpense.DRcumulative, 0) = 0 THEN ForTheDayExpense.CRcumulative ELSE ForTheDayExpense.DRcumulative END) ForTheDayExpenseAmount
	                               ,SUM(CASE WHEN ISNULL(ForThePeriodRevenue.DRcumulative, 0) = 0 THEN ForThePeriodRevenue.CRcumulative ELSE ForThePeriodRevenue.DRcumulative END) ForThePeriodRevenueAmount
	                               ,SUM(CASE WHEN ISNULL(ForThePeriodExpense.DRcumulative, 0) = 0 THEN ForThePeriodExpense.CRcumulative ELSE ForThePeriodExpense.DRcumulative END) ForThePeriodExpenseAmount
	                               ,SUM(CASE WHEN ISNULL(ForTheDelayRevenue.DRcumulative, 0) = 0 THEN ForTheDelayRevenue.CRcumulative ELSE ForTheDelayRevenue.DRcumulative END) ForTheDelayRevenueAmount
	                               ,SUM(CASE WHEN ISNULL(ForTheDelayExpense.DRcumulative, 0) = 0 THEN ForTheDelayExpense.CRcumulative ELSE ForTheDelayExpense.DRcumulative END) ForTheDelayExpenseAmount
                                   ,cmp.Active comActive,CMPGR.Active GroupActive
												
									FROM 
									 ORG.CompanyGroup CMPGR
									 join  org.company CMP ON CMP.CompanyGroupId = CMPGR.Id 
                                       
                                        LEFT JOIN
			                             (  
										 SELECT
								             V.CompanyGroupId,V.CompanyId
                                            , DRcumulative = CASE WHEN ACT.BalanceType = 'Debit' THEN SUM(VDC.DrAmount)-SUM(VDC.CrAmount) ELSE 0 END
					                        , CRcumulative = CASE WHEN ACT.BalanceType = 'Credit' THEN SUM(VDC.crAmount)-SUM(VDC.DrAmount) ELSE 0 END
								            FROM TRN.VoucherDetailCurrency AS VDC
								            JOIN TRN.VoucherDetail AS VD ON VD.Id=VDC.VoucherDetailId
								            JOIN TRN.Voucher AS V ON V.Id =VD.VoucherId
								             
							                 JOIN HKP.GLGeneralInfo AS GL ON GL.Id=VD.GLGeneralInfoId
					                         JOIN HKP.AccountGroup AS AG ON AG.Id=GL.AccountGroupId
					                         JOIN HKP.AccountType AS ACT ON ACT.Id=AG.AccountTypeId
								            --LEFT JOIN (SELECT * FROM SCS.CompanyParallelCurrency WHERE ParallelCurrencyType='CompanyCurrency') as CPC ON
								             --CPC.CurrencyId=VDC.ParallelCurrencyId
								            WHERE VD.BudgetMasterId   IS NOT NULL AND V.IsPark = 0 
								             " + condition + @"
								            --AND CPC.ParallelCurrencyType='CompanyCurrency'
                                            AND ACT.IsBalanceSheet =  0  AND ACT.Id IN ('Expense')
								            GROUP BY
								            V.CompanyGroupId,V.CompanyId,ACT.BalanceType) AS Expense ON Expense.CompanyGroupId = CMP.CompanyGroupId and Expense.CompanyId = CMP.Id
                                             
											 LEFT JOIN
                                              ( SELECT
								             V.CompanyGroupId,V.CompanyId
                                            , DRcumulative = CASE WHEN ACT.BalanceType = 'Debit' THEN SUM(VDC.DrAmount)-SUM(VDC.CrAmount) ELSE 0 END
					                        , CRcumulative = CASE WHEN ACT.BalanceType = 'Credit' THEN SUM(VDC.crAmount)-SUM(VDC.DrAmount) ELSE 0 END
								            FROM TRN.VoucherDetailCurrency AS VDC
								            JOIN TRN.VoucherDetail AS VD ON VD.Id=VDC.VoucherDetailId
								            JOIN TRN.Voucher AS V ON V.Id =VD.VoucherId
								            
							                 JOIN HKP.GLGeneralInfo AS GL ON GL.Id=VD.GLGeneralInfoId
					                         JOIN HKP.AccountGroup AS AG ON AG.Id=GL.AccountGroupId
					                         JOIN HKP.AccountType AS ACT ON ACT.Id=AG.AccountTypeId
								            --LEFT JOIN (SELECT * FROM SCS.CompanyParallelCurrency WHERE ParallelCurrencyType='CompanyCurrency') as CPC ON
								             --CPC.CurrencyId=VDC.ParallelCurrencyId
								            WHERE VD.BudgetMasterId IS NOT NULL and V.IsPark = 0 
													" + condition + @"								            
                                            AND ACT.IsBalanceSheet =  0  AND ACT.Id IN ('Revenue')
								            GROUP BY
								            V.CompanyGroupId,V.CompanyId,ACT.BalanceType) AS Revenue  ON  CMP.CompanyGroupId = Revenue.CompanyGroupId and CMP.Id = Revenue.CompanyId
                                           
										   LEFT JOIN (
                                            		SELECT V.CompanyGroupId,V.CompanyId
                                            			,DRcumulative = CASE WHEN ACT.BalanceType = 'Debit' THEN SUM(VDC.DrAmount) - SUM(VDC.CrAmount) ELSE 0 END
                                            			,CRcumulative = CASE WHEN ACT.BalanceType = 'Credit' THEN SUM(VDC.crAmount) - SUM(VDC.DrAmount) ELSE 0 END 
														FROM TRN.VoucherDetailCurrency AS VDC JOIN TRN.VoucherDetail AS VD ON VD.Id = VDC.VoucherDetailId 
														JOIN TRN.Voucher AS V ON V.Id = VD.VoucherId
														
														 JOIN HKP.GLGeneralInfo AS GL ON GL.Id = VD.GLGeneralInfoId 
														LEFT JOIN HKP.AccountGroup AS AG ON AG.Id = GL.AccountGroupId 
														LEFT JOIN HKP.AccountType AS ACT ON ACT.Id = AG.AccountTypeId
                                            		--LEFT JOIN (SELECT * FROM SCS.CompanyParallelCurrency WHERE ParallelCurrencyType ='CompanyCurrency') as CPC ON
                                            		--CPC.CurrencyId=VDC.ParallelCurrencyId
                                            		WHERE VD.BudgetMasterId IS NOT NULL
                                            			AND V.IsPark = 0
                                            			" + fortheDaycondition + @"
                                            					--AND CPC.ParallelCurrencyType='CompanyCurrency'
                                            			AND ACT.IsBalanceSheet = 0
                                            			AND ACT.Id IN ('Expense') GROUP BY V.CompanyGroupId,V.CompanyId
                                            			,ACT.BalanceType
                                            		) AS ForTheDayExpense ON ForTheDayExpense.CompanyGroupId = CMP.CompanyGroupId
                                            		AND ForTheDayExpense.CompanyId = CMP.Id
                                            LEFT JOIN (
                                            		SELECT V.CompanyGroupId,V.CompanyId
                                            			,DRcumulative = CASE WHEN ACT.BalanceType = 'Debit' THEN SUM(VDC.DrAmount) - SUM(VDC.CrAmount) ELSE 0 END
                                            			,CRcumulative = CASE WHEN ACT.BalanceType = 'Credit' THEN SUM(VDC.crAmount) - SUM(VDC.DrAmount) ELSE 0 END 
														FROM TRN.VoucherDetailCurrency AS VDC 
														JOIN TRN.VoucherDetail AS VD ON VD.Id = VDC.VoucherDetailId 
														JOIN TRN.Voucher AS V ON V.Id = VD.VoucherId 
														
														LEFT JOIN HKP.GLGeneralInfo AS GL ON GL.Id = VD.GLGeneralInfoId 
														LEFT JOIN HKP.AccountGroup AS AG ON AG.Id = GL.AccountGroupId 
														LEFT JOIN HKP.AccountType AS ACT ON ACT.Id = AG.AccountTypeId
                                            		--LEFT JOIN (SELECT * FROM SCS.CompanyParallelCurrency WHERE ParallelCurrencyType='CompanyCurrency') as CPC ON
                                            		--CPC.CurrencyId=VDC.ParallelCurrencyId
                                            		WHERE VD.BudgetMasterId IS NOT NULL
                                            			AND V.IsPark = 0
                                            			" + fortheDaycondition + @"
                                            					--AND CPC.ParallelCurrencyType='CompanyCurrency'
                                            			AND ACT.IsBalanceSheet = 0
                                            			AND ACT.Id IN ('Revenue') GROUP BY V.CompanyGroupId,V.CompanyId
                                            			,ACT.BalanceType
                                            		) AS ForTheDayRevenue ON ForTheDayRevenue.CompanyGroupId = CMP.CompanyGroupId
                                            		AND ForTheDayRevenue.CompanyId = CMP.Id LEFT JOIN (
                                            		SELECT V.CompanyGroupId,V.CompanyId
                                            			,DRcumulative = CASE WHEN ACT.BalanceType = 'Debit' THEN SUM(VDC.DrAmount) - SUM(VDC.CrAmount) ELSE 0 END
                                            			,CRcumulative = CASE WHEN ACT.BalanceType = 'Credit' THEN SUM(VDC.crAmount) - SUM(VDC.DrAmount) ELSE 0 END 
														FROM TRN.VoucherDetailCurrency AS VDC 
														JOIN TRN.VoucherDetail AS VD ON VD.Id = VDC.VoucherDetailId 
														JOIN TRN.Voucher AS V ON V.Id = VD.VoucherId 
														 JOIN ORG.Company AS CMP ON CMP.Id = V.CompanyId 
														 JOIN ORG.CompanyGroup AS CMPGR ON CMPGR.Id = V.CompanyGroupId 
														LEFT JOIN HKP.GLGeneralInfo AS GL ON GL.Id = VD.GLGeneralInfoId 
														LEFT JOIN HKP.AccountGroup AS AG ON AG.Id = GL.AccountGroupId 
														LEFT JOIN HKP.AccountType AS ACT ON ACT.Id = AG.AccountTypeId
                                            		--LEFT JOIN (SELECT * FROM SCS.CompanyParallelCurrency WHERE ParallelCurrencyType='CompanyCurrency') as CPC ON
                                            		--CPC.CurrencyId=VDC.ParallelCurrencyId
                                            		WHERE VD.BudgetMasterId IS NOT NULL
                                            			AND V.IsPark = 0
                                            			 " + forthePeriodCondition + @"
                                              
                                            					--AND CPC.ParallelCurrencyType='CompanyCurrency'
                                            			AND ACT.IsBalanceSheet = 0
                                            			AND ACT.Id IN ('Expense') GROUP BY V.CompanyGroupId,V.CompanyId
                                            			,ACT.BalanceType
                                            		) AS ForThePeriodExpense ON CMP.CompanyGroupId = ForThePeriodExpense.CompanyGroupId
                                            		AND CMP.Id = ForThePeriodExpense.CompanyId
                                             LEFT JOIN (
                                            		SELECT V.CompanyGroupId
                                            			,V.CompanyId
                                            			,DRcumulative = CASE WHEN ACT.BalanceType = 'Debit' THEN SUM(VDC.DrAmount) - SUM(VDC.CrAmount) ELSE 0 END
                                            			,CRcumulative = CASE WHEN ACT.BalanceType = 'Credit' THEN SUM(VDC.crAmount) - SUM(VDC.DrAmount) ELSE 0 END 
														FROM TRN.VoucherDetailCurrency AS VDC 
														JOIN TRN.VoucherDetail AS VD ON VD.Id = VDC.VoucherDetailId 
														JOIN TRN.Voucher AS V ON V.Id = VD.VoucherId 
														LEFT JOIN HKP.GLGeneralInfo AS GL ON GL.Id = VD.GLGeneralInfoId 
														LEFT JOIN HKP.AccountGroup AS AG ON AG.Id = GL.AccountGroupId 
														LEFT JOIN HKP.AccountType AS ACT ON ACT.Id = AG.AccountTypeId
                                            		--LEFT JOIN (SELECT * FROM SCS.CompanyParallelCurrency WHERE ParallelCurrencyType='CompanyCurrency') as CPC ON
                                            		--CPC.CurrencyId=VDC.ParallelCurrencyId
                                            		WHERE VD.BudgetMasterId IS NOT NULL
                                            			AND V.IsPark = 0
                                            			 " + forthePeriodCondition + @"
                                             
                                            					--AND CPC.ParallelCurrencyType='CompanyCurrency'
                                            			AND ACT.IsBalanceSheet = 0
                                            			AND ACT.Id IN ('Revenue') GROUP BY V.CompanyGroupId,V.CompanyId
                                            			,ACT.BalanceType
                                            		) AS ForThePeriodRevenue ON ForThePeriodRevenue.CompanyGroupId = CMP.CompanyGroupId
                                            		AND ForThePeriodRevenue.CompanyId = CMP.Id
                                                    LEFT JOIN (
                                            		SELECT V.CompanyGroupId,V.CompanyId
														,DRcumulative = CASE WHEN ACT.BalanceType = 'Debit' THEN SUM(VDC.DrAmount) - SUM(VDC.CrAmount) ELSE 0 END
                                            			,CRcumulative = CASE WHEN ACT.BalanceType = 'Credit' THEN SUM(VDC.crAmount) - SUM(VDC.DrAmount) ELSE 0 END 
														FROM TRN.VoucherDetailCurrency AS VDC 
														JOIN TRN.VoucherDetail AS VD ON VD.Id = VDC.VoucherDetailId 
														JOIN TRN.Voucher AS V ON V.Id = VD.VoucherId 
														LEFT JOIN HKP.GLGeneralInfo AS GL ON GL.Id = VD.GLGeneralInfoId 
														LEFT JOIN HKP.AccountGroup AS AG ON AG.Id = GL.AccountGroupId 
														LEFT JOIN HKP.AccountType AS ACT ON ACT.Id = AG.AccountTypeId
                                            		--LEFT JOIN (SELECT * FROM SCS.CompanyParallelCurrency WHERE ParallelCurrencyType='CompanyCurrency') as CPC ON
                                            		--CPC.CurrencyId=VDC.ParallelCurrencyId
                                            		WHERE VD.BudgetMasterId IS NOT NULL
                                            			AND V.IsPark = 0
                                            		" + delayPosting + @"
                                            					--AND CPC.ParallelCurrencyType='CompanyCurrency'
                                            			AND ACT.IsBalanceSheet = 0
                                            			AND ACT.Id IN ('Expense') GROUP BY V.CompanyGroupId,V.CompanyId,ACT.BalanceType
                                            		) AS ForTheDelayExpense ON ForTheDelayExpense.CompanyGroupId = CMP.CompanyGroupId
                                            		AND ForTheDelayExpense.CompanyId = CMP.Id
                                             LEFT JOIN (
                                            		SELECT  V.CompanyGroupId,V.CompanyId
														,DRcumulative = CASE WHEN ACT.BalanceType = 'Debit' THEN SUM(VDC.DrAmount) - SUM(VDC.CrAmount) ELSE 0 END
                                            			,CRcumulative = CASE WHEN ACT.BalanceType = 'Credit' THEN SUM(VDC.crAmount) - SUM(VDC.DrAmount) ELSE 0 END 
														FROM TRN.VoucherDetailCurrency AS VDC 
														JOIN TRN.VoucherDetail AS VD ON VD.Id = VDC.VoucherDetailId 
														JOIN TRN.Voucher AS V ON V.Id = VD.VoucherId 
														LEFT JOIN HKP.GLGeneralInfo AS GL ON GL.Id = VD.GLGeneralInfoId 
														LEFT JOIN HKP.AccountGroup AS AG ON AG.Id = GL.AccountGroupId 
														LEFT JOIN HKP.AccountType AS ACT ON ACT.Id = AG.AccountTypeId
                                            		--LEFT JOIN (SELECT * FROM SCS.CompanyParallelCurrency WHERE ParallelCurrencyType='CompanyCurrency') as CPC ON
                                            		--CPC.CurrencyId=VDC.ParallelCurrencyId
                                            		WHERE VD.BudgetMasterId IS NOT NULL
                                            			AND V.IsPark = 0
                                            		" + delayPosting + @"
                                            					--AND CPC.ParallelCurrencyType='CompanyCurrency'
                                            			AND ACT.IsBalanceSheet = 0
                                            			AND ACT.Id IN ('Revenue') GROUP BY  V.CompanyGroupId,V.CompanyId,ACT.BalanceType
                                            		) AS ForTheDelayRevenue ON CMP.CompanyGroupId = Revenue.CompanyGroupId
                                            		AND CMP.Id = Revenue.CompanyId
												GROUP BY CMP.CompanyGroupId
                                            	,CMP.UserName,CMP.Id,CMPGR.UserName,Revenue.CompanyGroupId,Revenue.CompanyId
                                                ,cmp.Active ,CMPGR.Active 
												) OverAll 
												where OverAll.comActive=1 and OverAll.GroupActive=1
												and isnull(OverAll.ExpenseAmount,0) > 0 OR isnull(OverAll.ForTheDayExpenseAmount,0) > 0 OR isnull(OverAll.ForThePeriodExpenseAmount,0) > 0 OR ISNULL(OverAll.ForTheDelayExpenseAmount,0) >0
												OR isnull(OverAll.RevenueAmount,0) > 0 OR isnull(OverAll.ForTheDayRevenueAmount,0) > 0 OR isnull(OverAll.ForThePeriodRevenueAmount,0) > 0 OR ISNULL(OverAll.ForTheDelayRevenueAmount,0) >0";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public IEnumerable<object> DymnamicExpenseList(IEnumerable<ChartColumnList> ChartColumnList, int seq, string factDate, string fromDate, string toDate, string companyGroupId, string companyId)
        {
            var cList = string.Empty;
            var cListId = string.Empty;
            var join = string.Empty;
            var wc = string.Empty;
            var Wcm = string.Empty;

            var cListextG = string.Empty;
            var cListextIdG = string.Empty;
            var grpFactDate = string.Empty;
            var fDate = string.Empty;

            try
            {
                var expFactDate = string.Empty;
                var condition = string.Empty;
                var fortheDaycondition = string.Empty;
                var forthePeriodCondition = string.Empty;
                var delayPosting = string.Empty;
                expFactDate = "AND CONVERT(DATE,V.PostingDate) <= CONVERT(DATE,GETDATE())";

                if (factDate == "postingDate")
                {
                    expFactDate = "Replace(CONVERT(VARCHAR(11),V.PostingDate,6),' ','-') factDate,";
                    condition = "AND V.PostingDate BETWEEN CONVERT(DATE,'" + fromDate + @"') AND CONVERT(DATE,'" + toDate + @"')";
                    grpFactDate = "V.PostingDate";
                    fortheDaycondition = "AND V.PostingDate BETWEEN CONVERT(DATE,'" + toDate + @"') AND CONVERT(DATE,'" + toDate + @"')";
                    forthePeriodCondition = @" AND V.PostingDate BETWEEN (SELECT  StartDate FROM SCS.FiscalYearPeriod where '" + toDate + @"' between StartDate and EndDate)
                                              AND(SELECT  EndDate FROM SCS.FiscalYearPeriod where '" + toDate + @"' between StartDate and EndDate)";
                    delayPosting = @"AND CONVERT(DATE,V.AddedDate) = CONVERT(DATE,'" + toDate + @"') AND MONTH(V.PostingDate) < MONTH('" + toDate + @"') AND YEAR(V.PostingDate) <= YEAR('" + toDate + @"')";
                }
                if (factDate == "AddedDate")
                {
                    expFactDate = "Replace(CONVERT(VARCHAR(11),V.AddedDate,6),' ','-') factDate,";
                    condition = "AND V.AddedDate BETWEEN CONVERT(DATE,'" + fromDate + @"') AND CONVERT(DATE,'" + toDate + @"')";
                    grpFactDate = "V.AddedDate";
                    fortheDaycondition = "AND V.AddedDate BETWEEN CONVERT(DATE, '" + toDate + @"') AND CONVERT(DATE,'" + toDate + @"')";
                    forthePeriodCondition = @" AND V.AddedDate BETWEEN (SELECT  StartDate FROM SCS.FiscalYearPeriod where '" + toDate + @"' between StartDate and EndDate)
                                              AND(SELECT  EndDate FROM SCS.FiscalYearPeriod where '" + toDate + @"' between StartDate and EndDate)";

                    delayPosting = @"AND CONVERT(DATE,V.AddedDate) = CONVERT(DATE,'" + toDate + @"') AND MONTH(V.PostingDate) < MONTH('" + toDate + @"') AND YEAR(V.PostingDate) <= YEAR('" + toDate + @"')";
                }

                seq += 1;
                foreach (var item in ChartColumnList)
                {
                    if (item.Sequence != -2 && item.Sequence != -1)
                    {
                        if (item.Sequence == seq)
                        {
                            if (item.RType == "Entity")
                            {
                                cList = "," + item.StandardName + ".UserName";
                                cListId = "," + item.StandardName + ".Id";
                                //cListextG = "," + item.ColumnName + "Name";
                                //cListextIdG = "," + item.ColumnName + "Id";
                                if (item.StandardName == "EmployeeGroup")
                                {
                                    join += "LEFT JOIN [HKP].[" + item.StandardName + "] ON " + item.StandardName + ".Id = E." + item.StandardName + "Id\n";
                                }
                                else
                                {
                                    join += "LEFT JOIN [ORG].[" + item.StandardName + "] ON " + item.StandardName + ".Id = E." + item.StandardName + "Id\n";
                                }
                            }
                            else
                            {
                                cList = "," + item.StandardName + ".UserName"; cListId = "," + item.StandardName + ".Id";
                                join += "LEFT JOIN [ORG].[" + item.StandardName + "] ON " + item.StandardName + ".Id = Po." + item.StandardName + "Id\n";
                            }
                        }
                    }
                    if (item.Sequence != -2)
                    {
                        if (item.Sequence == -1)
                        {
                            wc = "  AND  V.CompanyId ='" + item.Id + "'";
                            Wcm = "  AND  CMP.Id ='" + item.Id + "'";
                        }
                        else
                        {
                            if (item.Sequence < seq)
                            {
                                wc += " and " + item.StandardName + ".Id='" + item.Text + "'";
                                Wcm += " and " + item.StandardName + ".Id='" + item.Text + "'";
                            }
                        }
                    }
                }
                var sql = @"  SELECT * FROM ( SELECT   OrgStructure.CompanyId,OrgStructure.ColumnName, OrgStructure.UId
		                           ,SUM(CASE WHEN ISNULL(Expense.DRcumulative, 0) = 0 THEN Expense.CRcumulative ELSE Expense.DRcumulative END) ExpenseAmount
	                               ,SUM(CASE WHEN ISNULL(Revenue.DRcumulative, 0) = 0 THEN Revenue.CRcumulative ELSE Revenue.DRcumulative END) RevenueAmount
	                               ,SUM(CASE WHEN ISNULL(ForTheDayRevenue.DRcumulative, 0) = 0 THEN ForTheDayRevenue.CRcumulative ELSE ForTheDayRevenue.DRcumulative END) ForTheDayRevenueAmount
	                               ,SUM(CASE WHEN ISNULL(ForTheDayExpense.DRcumulative, 0) = 0 THEN ForTheDayExpense.CRcumulative ELSE ForTheDayExpense.DRcumulative END) ForTheDayExpenseAmount
                                   ,SUM(CASE WHEN ISNULL(ForThePeriodRevenue.DRcumulative, 0) = 0 THEN ForThePeriodRevenue.CRcumulative ELSE ForThePeriodRevenue.DRcumulative END) ForThePeriodRevenueAmount
	                               ,SUM(CASE WHEN ISNULL(ForThePeriodExpense.DRcumulative, 0) = 0 THEN ForThePeriodExpense.CRcumulative ELSE ForThePeriodExpense.DRcumulative END) ForThePeriodExpenseAmount
                          ,SUM(CASE WHEN ISNULL(ForTheDelayRevenue.DRcumulative, 0) = 0 THEN ForTheDelayRevenue.CRcumulative ELSE ForTheDelayRevenue.DRcumulative END) ForTheDelayRevenueAmount
	                               ,SUM(CASE WHEN ISNULL(ForTheDelayExpense.DRcumulative, 0) = 0 THEN ForTheDelayExpense.CRcumulative ELSE ForTheDelayExpense.DRcumulative END) ForTheDelayExpenseAmount
                                    FROM
                                    (SELECT
								             DISTINCT cmp.Id AS CompanyId
								           	" + cList + @" AS ColumnName
									         " + cListId + @" AS UId
											from ORG.CompanyGroup CMPGR
											 JOIN  org.company CMP ON CMP.CompanyGroupId = CMPGR.Id 
											left join org.Entity E ON E.CompanyGroupId = CMPGR.Id
 JOIN [ORG].[Plant] ON Plant.Id = E.PlantId
 JOIN [ORG].Division ON Division.Id = E.DivisionId
 JOIN [ORG].SubDivision ON SubDivision.Id = E.SubDivisionId

                                            where cmp.Active =1 and CMPGR.Active = 1  
                                           " + Wcm + @"
											) OrgStructure
                                    LEFT JOIN
		                         (
		                            SELECT
								             V.CompanyId
								           		,V.PlantId AS UId
                                            , DRcumulative = CASE WHEN ACT.BalanceType = 'Debit' THEN SUM(VDC.DrAmount)-SUM(VDC.CrAmount) ELSE 0 END
					                        , CRcumulative = CASE WHEN ACT.BalanceType = 'Credit' THEN SUM(VDC.crAmount)-SUM(VDC.DrAmount) ELSE 0 END
								            FROM TRN.VoucherDetailCurrency AS VDC
								            JOIN TRN.VoucherDetail AS VD ON VD.Id=VDC.VoucherDetailId
								            JOIN TRN.Voucher AS V ON V.Id =VD.VoucherId
							                LEFT JOIN HKP.GLGeneralInfo AS GL ON GL.Id=VD.GLGeneralInfoId
					                        LEFT JOIN HKP.AccountGroup AS AG ON AG.Id=GL.AccountGroupId
					                        LEFT JOIN HKP.AccountType AS ACT ON ACT.Id=AG.AccountTypeId
 JOIN [ORG].[Plant] ON Plant.Id = V.PlantId

											
								            WHERE VD.BudgetMasterId   IS NOT NULL
                                            " + condition + @"
								            --AND CPC.ParallelCurrencyType='CompanyCurrency'
											" + wc + @"
											AND ACT.IsBalanceSheet =  0  AND ACT.Id IN ('Expense') 
                                            and V.IsPark = 0 
								            GROUP BY ACT.BalanceType, V.CompanyId,V.PlantId
								               ) AS Expense ON   OrgStructure.CompanyId = Expense.CompanyId AND Expense.UId = OrgStructure.UId
                                        Left Join 
                                    (
		                            SELECT
								             V.CompanyId
								           		,V.PlantId AS UId
                                            , DRcumulative = CASE WHEN ACT.BalanceType = 'Debit' THEN SUM(VDC.DrAmount)-SUM(VDC.CrAmount) ELSE 0 END
					                        , CRcumulative = CASE WHEN ACT.BalanceType = 'Credit' THEN SUM(VDC.crAmount)-SUM(VDC.DrAmount) ELSE 0 END
								            FROM TRN.VoucherDetailCurrency AS VDC
								            JOIN TRN.VoucherDetail AS VD ON VD.Id=VDC.VoucherDetailId
								            JOIN TRN.Voucher AS V ON V.Id =VD.VoucherId
							                LEFT JOIN HKP.GLGeneralInfo AS GL ON GL.Id=VD.GLGeneralInfoId
					                        LEFT JOIN HKP.AccountGroup AS AG ON AG.Id=GL.AccountGroupId
					                        LEFT JOIN HKP.AccountType AS ACT ON ACT.Id=AG.AccountTypeId

 JOIN [ORG].[Plant] ON Plant.Id = V.PlantId
											

								            WHERE VD.BudgetMasterId   IS NOT NULL
								           " + condition + @"
								            --AND CPC.ParallelCurrencyType='CompanyCurrency' 
                                            and VD.IsPark = 0
											" + wc + @"
											AND ACT.IsBalanceSheet =  0  AND ACT.Id IN ('Revenue')
								            GROUP BY ACT.BalanceType, V.CompanyId
								              ,V.PlantId) AS Revenue ON 
                                     OrgStructure.CompanyId = Revenue.CompanyId and OrgStructure.UId = Revenue.UId
                                        LEFT JOIN
                                    (
		                            SELECT
								             V.CompanyId
								           	,V.PlantId AS UId
                                            , DRcumulative = CASE WHEN ACT.BalanceType = 'Debit' THEN SUM(VDC.DrAmount)-SUM(VDC.CrAmount) ELSE 0 END
					                        , CRcumulative = CASE WHEN ACT.BalanceType = 'Credit' THEN SUM(VDC.crAmount)-SUM(VDC.DrAmount) ELSE 0 END
								            FROM TRN.VoucherDetailCurrency AS VDC
								            JOIN TRN.VoucherDetail AS VD ON VD.Id=VDC.VoucherDetailId
								            JOIN TRN.Voucher AS V ON V.Id =VD.VoucherId
							                LEFT JOIN HKP.GLGeneralInfo AS GL ON GL.Id=VD.GLGeneralInfoId
					                        LEFT JOIN HKP.AccountGroup AS AG ON AG.Id=GL.AccountGroupId
					                        LEFT JOIN HKP.AccountType AS ACT ON ACT.Id=AG.AccountTypeId
 JOIN [ORG].[Plant] ON Plant.Id = V.PlantId

												
								            WHERE VD.BudgetMasterId   IS NOT NULL
								               " + fortheDaycondition + @"
								            --AND CPC.ParallelCurrencyType='CompanyCurrency'
                                        and VD.IsPark = 0
											  " + wc + @"
											AND ACT.IsBalanceSheet =  0  AND ACT.Id IN ('Revenue')
								            GROUP BY ACT.BalanceType, V.CompanyId
								              ,V.PlantId ) AS ForTheDayRevenue ON 
												 ForTheDayRevenue.CompanyId = OrgStructure.CompanyId AND ForTheDayRevenue.UId = OrgStructure.UId
                                        LEFT JOIN
                                        (
		                                    SELECT
								             V.CompanyId
								           		,V.PlantId AS UId
                                            , DRcumulative = CASE WHEN ACT.BalanceType = 'Debit' THEN SUM(VDC.DrAmount)-SUM(VDC.CrAmount) ELSE 0 END
					                        , CRcumulative = CASE WHEN ACT.BalanceType = 'Credit' THEN SUM(VDC.crAmount)-SUM(VDC.DrAmount) ELSE 0 END
								            FROM TRN.VoucherDetailCurrency AS VDC
								            JOIN TRN.VoucherDetail AS VD ON VD.Id=VDC.VoucherDetailId
								            JOIN TRN.Voucher AS V ON V.Id =VD.VoucherId
							                LEFT JOIN HKP.GLGeneralInfo AS GL ON GL.Id=VD.GLGeneralInfoId
					                        LEFT JOIN HKP.AccountGroup AS AG ON AG.Id=GL.AccountGroupId
					                        LEFT JOIN HKP.AccountType AS ACT ON ACT.Id=AG.AccountTypeId
 JOIN [ORG].[Plant] ON Plant.Id = V.PlantId

										
								            WHERE VD.BudgetMasterId   IS NOT NULL
								             " + fortheDaycondition + @"
								            --AND CPC.ParallelCurrencyType='CompanyCurrency' 
                                            and VD.IsPark = 0
											  	" + wc + @"
											AND ACT.IsBalanceSheet =  0  AND ACT.Id IN ('Expense')
								            GROUP BY ACT.BalanceType, V.CompanyId
								            ,V.PlantId ) AS ForTheDayExpense ON 
												 ForTheDayExpense.CompanyId = OrgStructure.CompanyId AND ForTheDayExpense.UId = OrgStructure.UId
                                                                    
                                            ------------------------------For the Period -----------------------------------
                                    LEFT JOIN
                                    (
		                            SELECT
								             V.CompanyId
								           		, V.PlantId AS UId
                                            , DRcumulative = CASE WHEN ACT.BalanceType = 'Debit' THEN SUM(VDC.DrAmount)-SUM(VDC.CrAmount) ELSE 0 END
					                        , CRcumulative = CASE WHEN ACT.BalanceType = 'Credit' THEN SUM(VDC.crAmount)-SUM(VDC.DrAmount) ELSE 0 END
								            FROM TRN.VoucherDetailCurrency AS VDC
								            JOIN TRN.VoucherDetail AS VD ON VD.Id=VDC.VoucherDetailId
								            JOIN TRN.Voucher AS V ON V.Id =VD.VoucherId
							                LEFT JOIN HKP.GLGeneralInfo AS GL ON GL.Id=VD.GLGeneralInfoId
					                        LEFT JOIN HKP.AccountGroup AS AG ON AG.Id=GL.AccountGroupId
					                        LEFT JOIN HKP.AccountType AS ACT ON ACT.Id=AG.AccountTypeId
 JOIN [ORG].[Plant] ON Plant.Id = V.PlantId

									
								            WHERE VD.BudgetMasterId   IS NOT NULL
								              " + forthePeriodCondition + @"
                                             
								            --AND CPC.ParallelCurrencyType='CompanyCurrency'
                                        and VD.IsPark = 0
											  	" + wc + @"
											AND ACT.IsBalanceSheet =  0  AND ACT.Id IN ('Revenue')
								            GROUP BY ACT.BalanceType, V.CompanyId
								             ,V.PlantId ) AS ForThePeriodRevenue ON 
												 ForThePeriodRevenue.CompanyId = OrgStructure.CompanyId AND ForThePeriodRevenue.UId = OrgStructure.UId
                                        LEFT JOIN
                                        (
		                                    SELECT
								             V.CompanyId
								            	,V.PlantId AS UId
                                            , DRcumulative = CASE WHEN ACT.BalanceType = 'Debit' THEN SUM(VDC.DrAmount)-SUM(VDC.CrAmount) ELSE 0 END
					                        , CRcumulative = CASE WHEN ACT.BalanceType = 'Credit' THEN SUM(VDC.crAmount)-SUM(VDC.DrAmount) ELSE 0 END
								            FROM TRN.VoucherDetailCurrency AS VDC
								            JOIN TRN.VoucherDetail AS VD ON VD.Id=VDC.VoucherDetailId
								            JOIN TRN.Voucher AS V ON V.Id =VD.VoucherId
							                LEFT JOIN HKP.GLGeneralInfo AS GL ON GL.Id=VD.GLGeneralInfoId
					                        LEFT JOIN HKP.AccountGroup AS AG ON AG.Id=GL.AccountGroupId
					                        LEFT JOIN HKP.AccountType AS ACT ON ACT.Id=AG.AccountTypeId
 JOIN [ORG].[Plant] ON Plant.Id = V.PlantId


								            WHERE VD.BudgetMasterId   IS NOT NULL
								               " + forthePeriodCondition + @"
                                            and VD.IsPark = 0
											" + wc + @"
											AND ACT.IsBalanceSheet =  0  AND ACT.Id IN ('Expense')
								            GROUP BY ACT.BalanceType, V.CompanyId
								            ,V.PlantId) AS ForThePeriodExpense ON 
												 ForThePeriodExpense.CompanyId = OrgStructure.CompanyId AND ForThePeriodExpense.UId = OrgStructure.UId
                                            --------------------------------------------------------------------------------
								            -------------------------------------Delay Posting------------------------------
                                    LEFT JOIN
                                    (
		                            SELECT
								             V.CompanyId
								           		,V.PlantId AS UId
                                            , DRcumulative = CASE WHEN ACT.BalanceType = 'Debit' THEN SUM(VDC.DrAmount)-SUM(VDC.CrAmount) ELSE 0 END
					                        , CRcumulative = CASE WHEN ACT.BalanceType = 'Credit' THEN SUM(VDC.crAmount)-SUM(VDC.DrAmount) ELSE 0 END
								            FROM TRN.VoucherDetailCurrency AS VDC
								            JOIN TRN.VoucherDetail AS VD ON VD.Id=VDC.VoucherDetailId
								            JOIN TRN.Voucher AS V ON V.Id =VD.VoucherId
							                LEFT JOIN HKP.GLGeneralInfo AS GL ON GL.Id=vd.GLGeneralInfoId
					                        LEFT JOIN HKP.AccountGroup AS AG ON AG.Id=GL.AccountGroupId
					                        LEFT JOIN HKP.AccountType AS ACT ON ACT.Id=AG.AccountTypeId
 JOIN [ORG].[Plant] ON Plant.Id = V.PlantId
											

								            WHERE VD.BudgetMasterId   IS NOT NULL
								            " + delayPosting + @"
                                        and VD.IsPark = 0
											  	" + wc + @"
											AND ACT.IsBalanceSheet =  0  AND ACT.Id IN ('Revenue')
								            GROUP BY ACT.BalanceType, V.CompanyId
								             ,V.PlantId ) AS ForTheDelayRevenue ON 
												 ForTheDelayRevenue.CompanyId = OrgStructure.CompanyId AND ForTheDelayRevenue.UId = OrgStructure.UId
                                        LEFT JOIN
                                        (
		                                    SELECT
								             V.CompanyId
								           	  	,V.PlantId AS UId
                                            , DRcumulative = CASE WHEN ACT.BalanceType = 'Debit' THEN SUM(VDC.DrAmount)-SUM(VDC.CrAmount) ELSE 0 END
					                        , CRcumulative = CASE WHEN ACT.BalanceType = 'Credit' THEN SUM(VDC.crAmount)-SUM(VDC.DrAmount) ELSE 0 END
								            FROM TRN.VoucherDetailCurrency AS VDC
								            JOIN TRN.VoucherDetail AS VD ON VD.Id=VDC.VoucherDetailId
								            JOIN TRN.Voucher AS V ON V.Id =VD.VoucherId
							                LEFT JOIN HKP.GLGeneralInfo AS GL ON GL.Id=VD.GLGeneralInfoId
					                        LEFT JOIN HKP.AccountGroup AS AG ON AG.Id=GL.AccountGroupId
					                        LEFT JOIN HKP.AccountType AS ACT ON ACT.Id=AG.AccountTypeId
 JOIN [ORG].[Plant] ON Plant.Id = V.PlantId
											
								            WHERE VD.BudgetMasterId   IS NOT NULL
								             " + delayPosting + @"
                                            and VD.IsPark = 0
											 " + wc + @"
											AND ACT.IsBalanceSheet =  0  AND ACT.Id IN ('Expense')
								            GROUP BY ACT.BalanceType, V.CompanyId
								          ,V.PlantId ) AS ForTheDelayExpense ON 
												 ForTheDelayExpense.CompanyId = OrgStructure.CompanyId AND ForTheDelayExpense.UId = OrgStructure.UId
                                            --------------------------------------------------------------------------------
                                                                                                                                                                  
                                          GROUP BY OrgStructure.CompanyId,OrgStructure.ColumnName,OrgStructure.UId,Revenue.CompanyId,Revenue.UId
							                    	 ,Expense.CompanyId,Expense.UId) OverAll WHERE isnull(OverAll.ExpenseAmount,0) > 0 OR isnull(OverAll.ForTheDayExpenseAmount,0) > 0 OR isnull(OverAll.ForThePeriodExpenseAmount,0) > 0 OR ISNULL(OverAll.ForTheDelayExpenseAmount,0) >0
												OR isnull(OverAll.RevenueAmount,0) > 0 OR isnull(OverAll.ForTheDayRevenueAmount,0) > 0 OR isnull(OverAll.ForThePeriodRevenueAmount,0) > 0 OR ISNULL(OverAll.ForTheDelayRevenueAmount,0) >0";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        #region Expense and Revenue bar Chart Data(Actual an Delay) 

        public IEnumerable<object> ExpenseListLineChart(string companyGroupId, string companyId, string factDate, string fromDate, string toDate)
        {
            try
            {
                var expFactDate = string.Empty;
                var condition = string.Empty;
                var grpFactDate = string.Empty;
                var fDate = string.Empty;

                if (factDate == "postingDate")
                {
                    expFactDate = "Replace(CONVERT(VARCHAR(11),V.PostingDate,6),' ','-') factDate,";
                    fDate = "V.PostingDate fDate,";
                    condition = "AND V.PostingDate BETWEEN CONVERT(DATE,'" + fromDate + @"') AND CONVERT(DATE,'" + toDate + @"')";
                    grpFactDate = "V.PostingDate";
                }
                else if (factDate == "AddedDate")
                {
                    expFactDate = "Replace(CONVERT(VARCHAR(11),V.AddedDate,6),' ','-') factDate,";
                    fDate = "V.AddedDate fDate,";
                    condition = "AND V.AddedDate BETWEEN CONVERT(DATE,'" + fromDate + @"') AND CONVERT(DATE,'" + toDate + @"')";
                    grpFactDate = "V.AddedDate";
                }


                var sql = @"SELECT SUM(CASE WHEN ISNULL(periodWiseExp.DRcumulative,0)=0 THEN periodWiseExp.CRcumulative ELSE periodWiseExp.DRcumulative END) Amount, periodWiseExp.PostingPeriod,
                                periodWiseExp.PostingPeriodId, periodWiseExp.EntryPeriod,periodWiseExp.EntryPeriodId, periodWiseExp.EntryPeriodEndDate
								,periodWiseExp.PostingPeriodEndDate,periodWiseExp.EntryPeriodStartDate,periodWiseExp.PostingPeriodStartDate FROM
                                  (SELECT
                                          DRcumulative = CASE WHEN ACT.BalanceType = 'Debit' THEN SUM(VDC.DrAmount)-SUM(VDC.CrAmount) ELSE 0 END
								        , CRcumulative = CASE WHEN ACT.BalanceType = 'Credit' THEN SUM(VDC.crAmount)-SUM(VDC.DrAmount) ELSE 0 END
								        , EFYP.PeriodName PostingPeriod,
                                        EFYP.Id PostingPeriodId, FYPA.PeriodName AS EntryPeriod,FYPA.EntryPeriodId, FYPA.EndDate EntryPeriodEndDate
                                        , EFYP.EndDate PostingPeriodEndDate
                                             , FYPA.StartDate  EntryPeriodStartDate,EFYP.StartDate PostingPeriodStartDate
                                        FROM TRN.VoucherDetailCurrency AS VDC
                                        LEFT JOIN TRN.VoucherDetail AS EVD ON EVD.Id = VDC.VoucherDetailId
                                        LEFT JOIN TRN.Voucher AS V ON V.Id = EVD.VoucherId

                                        LEFT JOIN SCS.FiscalYearPeriod AS EFYP ON EFYP.Id = EVD.FiscalYearPeriodId

                                        LEFT JOIN MST.BudgetMaster AS BM ON BM.Id = EVD.BudgetMasterId

                                        LEFT JOIN HKP.BudgetCategory AS BC ON BC.Id = BM.BudgetCategoryId

                                        LEFT JOIN HKP.BudgetSubCategory AS BSC ON BSC.Id = BM.BudgetSubCategoryId

                                        LEFT JOIN HKP.Budget AS B ON B.Id = BM.BudgetId

                                        LEFT JOIN HKP.GLGeneralInfo AS GL ON GL.Id = BM.GLGeneralInfoId

                                        LEFT JOIN HKP.AccountGroup AS AG ON AG.Id = GL.AccountGroupId

                                        LEFT JOIN HKP.AccountType AS ACT ON ACT.Id = AG.AccountTypeId

                                        LEFT JOIN ORG.Entity AS ENT ON ENT.Id = V.EntityId

                                        LEFT JOIN(SELECT Id AS EntryPeriodId, PeriodName, StartDate, EndDate FROM SCS.FiscalYearPeriod)

                                                    AS FYPA ON MONTH(CONVERT(DATE, FYPA.EndDate)) = MONTH(CONVERT(DATE, evd.AddedDate))

                                            AND YEAR(CONVERT(DATE, FYPA.EndDate))= YEAR(CONVERT(DATE, evd.AddedDate))

                                        WHERE V.CompanyGroupId = '" + companyGroupId + @"'  " + condition + @" and V.IsPark = 0 

                                        AND ACT.IsBalanceSheet = 0  AND ACT.Id IN ('Expense')

                                          GROUP BY EFYP.Id , EFYP.PeriodName,  FYPA.PeriodName, FYPA.EndDate,EFYP.EndDate,FYPA.EntryPeriodId ,ACT.BalanceType
                                        ,FYPA.StartDate ,EFYP.StartDate 
								    ) periodWiseExp

                            GROUP by periodWiseExp.PostingPeriod,periodWiseExp.PostingPeriodId, periodWiseExp.EntryPeriod,periodWiseExp.EntryPeriodId,
                            periodWiseExp.EntryPeriodEndDate, periodWiseExp.PostingPeriodEndDate,periodWiseExp.EntryPeriodStartDate,periodWiseExp.PostingPeriodStartDate 
                           ORDER BY periodWiseExp.PostingPeriodEndDate,periodWiseExp.EntryPeriodEndDate DESC ";

                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public IEnumerable<object> RevenueListLineChart(string companyGroupId, string companyId, string factDate, string fromDate, string toDate)
        {
            try
            {
                var expFactDate = string.Empty;
                var condition = string.Empty;
                var grpFactDate = string.Empty;
                var fDate = string.Empty;

                if (factDate == "postingDate")
                {
                    expFactDate = "Replace(CONVERT(VARCHAR(11),V.PostingDate,6),' ','-') factDate,";
                    fDate = "V.PostingDate fDate,";
                    condition = "AND V.PostingDate BETWEEN CONVERT(DATE,'" + fromDate + @"') AND CONVERT(DATE,'" + toDate + @"')";
                    grpFactDate = "V.PostingDate";
                }
                else if (factDate == "AddedDate")
                {
                    expFactDate = "Replace(CONVERT(VARCHAR(11),V.AddedDate,6),' ','-') factDate,";
                    fDate = "V.AddedDate fDate,";
                    condition = "AND V.AddedDate BETWEEN CONVERT(DATE,'" + fromDate + @"') AND CONVERT(DATE,'" + toDate + @"')";
                    grpFactDate = "V.AddedDate";
                }
                var sql = @"SELECT SUM(CASE WHEN ISNULL(periodWiseExp.DRcumulative,0)=0 THEN periodWiseExp.CRcumulative ELSE periodWiseExp.DRcumulative END) Amount, periodWiseExp.PostingPeriod,
                                periodWiseExp.PostingPeriodId, periodWiseExp.EntryPeriod,periodWiseExp.EntryPeriodId, periodWiseExp.EntryPeriodEndDate
								,periodWiseExp.PostingPeriodEndDate,periodWiseExp.EntryPeriodStartDate,periodWiseExp.PostingPeriodStartDate FROM
                                  (SELECT
                                          DRcumulative = CASE WHEN ACT.BalanceType = 'Debit' THEN SUM(VDC.DrAmount)-SUM(VDC.CrAmount) ELSE 0 END
								        , CRcumulative = CASE WHEN ACT.BalanceType = 'Credit' THEN SUM(VDC.crAmount)-SUM(VDC.DrAmount) ELSE 0 END
								        , EFYP.PeriodName PostingPeriod, EFYP.Id PostingPeriodId, FYPA.PeriodName AS EntryPeriod,FYPA.EntryPeriodId, FYPA.EndDate EntryPeriodEndDate
                                        , EFYP.EndDate PostingPeriodEndDate
                                        , FYPA.StartDate  EntryPeriodStartDate,EFYP.StartDate PostingPeriodStartDate
                                        FROM TRN.VoucherDetailCurrency AS VDC
                                        LEFT JOIN TRN.VoucherDetail AS EVD ON EVD.Id = VDC.VoucherDetailId
                                        LEFT JOIN TRN.Voucher AS V ON V.Id = EVD.VoucherId

                                        LEFT JOIN SCS.FiscalYearPeriod AS EFYP ON EFYP.Id = EVD.FiscalYearPeriodId

                                        LEFT JOIN MST.BudgetMaster AS BM ON BM.Id = EVD.BudgetMasterId

                                        LEFT JOIN HKP.BudgetCategory AS BC ON BC.Id = BM.BudgetCategoryId

                                        LEFT JOIN HKP.BudgetSubCategory AS BSC ON BSC.Id = BM.BudgetSubCategoryId

                                        LEFT JOIN HKP.Budget AS B ON B.Id = BM.BudgetId

                                        LEFT JOIN HKP.GLGeneralInfo AS GL ON GL.Id = BM.GLGeneralInfoId

                                        LEFT JOIN HKP.AccountGroup AS AG ON AG.Id = GL.AccountGroupId

                                        LEFT JOIN HKP.AccountType AS ACT ON ACT.Id = AG.AccountTypeId

                                        LEFT JOIN ORG.Entity AS ENT ON ENT.Id = V.EntityId

                                        LEFT JOIN(SELECT Id AS EntryPeriodId, PeriodName, StartDate, EndDate FROM SCS.FiscalYearPeriod)

                                                    AS FYPA ON MONTH(CONVERT(DATE, FYPA.EndDate)) = MONTH(CONVERT(DATE, evd.AddedDate))

                                            AND YEAR(CONVERT(DATE, FYPA.EndDate))= YEAR(CONVERT(DATE, evd.AddedDate))

                                        WHERE V.CompanyGroupId = '" + companyGroupId + @"'  " + condition + @" and V.IsPark = 0 

                                        AND ACT.IsBalanceSheet = 0  AND ACT.Id IN ('Revenue')

                                          GROUP BY EFYP.Id , EFYP.PeriodName,  FYPA.PeriodName, FYPA.EndDate,EFYP.EndDate,FYPA.EntryPeriodId ,ACT.BalanceType,EFYP.StartDate,FYPA.StartDate
								    ) periodWiseExp

                            GROUP by periodWiseExp.PostingPeriod,periodWiseExp.PostingPeriodId, periodWiseExp.EntryPeriod,periodWiseExp.EntryPeriodId,
                            periodWiseExp.EntryPeriodEndDate, periodWiseExp.PostingPeriodEndDate,periodWiseExp.EntryPeriodStartDate,periodWiseExp.PostingPeriodStartDate
                           ORDER BY periodWiseExp.PostingPeriodEndDate,periodWiseExp.EntryPeriodEndDate DESC ";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public IEnumerable<object> DymnamicExpenseListLineChart(IEnumerable<ChartColumnList> ChartColumnList, int seq, string factDate, string fromDate, string toDate, string companyGroupId, string companyId)
        {
            var cList = string.Empty;
            var cListId = string.Empty;
            var join = string.Empty;
            var wc = string.Empty;
            var cListextG = string.Empty;
            var cListextIdG = string.Empty;
            var fDate = string.Empty;
            //string expFactDate = string.Empty;
            try
            {
                var expFactDate = string.Empty;
                var condition = string.Empty;
                var grpFactDate = string.Empty;

                if (factDate == "postingDate")
                {
                    expFactDate = "Replace(CONVERT(VARCHAR(11),V.PostingDate,6),' ','-') factDate,";
                    fDate = "V.PostingDate fDate,";
                    condition = "AND V.PostingDate BETWEEN CONVERT(DATE,'" + fromDate + @"') AND CONVERT(DATE,'" + toDate + @"')";
                    grpFactDate = "V.PostingDate";
                }
                else if (factDate == "AddedDate")
                {
                    expFactDate = "Replace(CONVERT(VARCHAR(11),V.AddedDate,6),' ','-') factDate,";
                    condition = "AND V.AddedDate BETWEEN CONVERT(DATE,'" + fromDate + @"') AND CONVERT(DATE,'" + toDate + @"')";
                    fDate = "V.AddedDate fDate,";
                    grpFactDate = "V.AddedDate";
                }
                seq += 1;
                foreach (var item in ChartColumnList)
                {
                    if (item.Sequence != -2 && item.Sequence != -1)
                    {
                        if (item.Sequence <= seq)
                        {
                            if (item.RType == "Entity")
                            {
                                cList = "," + item.StandardName + ".UserName";
                                cListId = "," + item.StandardName + ".Id";
                                //cListextG = "," + item.ColumnName + "Name";
                                //cListextIdG = "," + item.ColumnName + "Id";
                                if (item.StandardName == "EmployeeGroup")
                                {
                                    join += "LEFT JOIN [HKP].[" + item.StandardName + "] ON " + item.StandardName + ".Id = ENT." + item.StandardName + "Id\n";
                                }
                                else
                                {
                                    join += "LEFT JOIN [ORG].[" + item.StandardName + "] ON " + item.StandardName + ".Id = ENT." + item.StandardName + "Id\n";
                                }
                            }
                            else
                            {
                                cList = "," + item.StandardName + ".UserName";
                                join += "LEFT JOIN [ORG].[" + item.StandardName + "] ON " + item.StandardName + ".Id = Po." + item.StandardName + "Id\n";
                            }
                        }
                    }
                    if (item.Sequence != -2)
                    {
                        if (item.Sequence == -1)
                        {
                            wc = "  AND CMP.Id ='" + item.Id + "'";
                        }
                        else
                        {
                            if (item.Sequence < seq)
                            {
                                wc += " and " + item.StandardName + ".Id='" + item.Text + "'";
                            }
                        }
                    }
                }

                var sql = @"SELECT SUM(CASE WHEN ISNULL(periodWiseExp.DRcumulative,0)=0 THEN periodWiseExp.CRcumulative ELSE periodWiseExp.DRcumulative END) Amount, periodWiseExp.PostingPeriod,
                                periodWiseExp.PostingPeriodId, periodWiseExp.EntryPeriod,periodWiseExp.EntryPeriodId, periodWiseExp.EntryPeriodEndDate
								,periodWiseExp.PostingPeriodEndDate,periodWiseExp.EntryPeriodStartDate,periodWiseExp.PostingPeriodStartDate FROM
                                  (SELECT
                                          DRcumulative = CASE WHEN ACT.BalanceType = 'Debit' THEN SUM(VDC.DrAmount)-SUM(VDC.CrAmount) ELSE 0 END
								        , CRcumulative = CASE WHEN ACT.BalanceType = 'Credit' THEN SUM(VDC.crAmount)-SUM(VDC.DrAmount) ELSE 0 END
								        , EFYP.PeriodName PostingPeriod,
                                        EFYP.Id PostingPeriodId, FYPA.PeriodName AS EntryPeriod,FYPA.EntryPeriodId, FYPA.EndDate EntryPeriodEndDate
                                        , EFYP.EndDate PostingPeriodEndDate
                                        ,FYPA.StartDate  EntryPeriodStartDate,EFYP.StartDate PostingPeriodStartDate
                                        FROM TRN.VoucherDetailCurrency AS VDC
                                        LEFT JOIN TRN.VoucherDetail AS EVD ON EVD.Id = VDC.VoucherDetailId
                                        LEFT JOIN TRN.Voucher AS V ON V.Id = EVD.VoucherId

                                        LEFT JOIN SCS.FiscalYearPeriod AS EFYP ON EFYP.Id = EVD.FiscalYearPeriodId

                                        LEFT JOIN MST.BudgetMaster AS BM ON BM.Id = EVD.BudgetMasterId

                                        LEFT JOIN HKP.BudgetCategory AS BC ON BC.Id = BM.BudgetCategoryId

                                        LEFT JOIN HKP.BudgetSubCategory AS BSC ON BSC.Id = BM.BudgetSubCategoryId

                                        LEFT JOIN HKP.Budget AS B ON B.Id = BM.BudgetId

                                        LEFT JOIN HKP.GLGeneralInfo AS GL ON GL.Id = BM.GLGeneralInfoId

                                        LEFT JOIN HKP.AccountGroup AS AG ON AG.Id = GL.AccountGroupId

                                        LEFT JOIN HKP.AccountType AS ACT ON ACT.Id = AG.AccountTypeId

                                        LEFT JOIN ORG.Entity AS ENT ON ENT.Id = V.EntityId
                                        LEFT JOIN ORG.Company AS cmp ON cmp.Id = V.CompanyId
									" + join + @"
                                        LEFT JOIN(SELECT Id AS EntryPeriodId, PeriodName, StartDate, EndDate FROM SCS.FiscalYearPeriod)

                                                    AS FYPA ON MONTH(CONVERT(DATE, FYPA.EndDate)) = MONTH(CONVERT(DATE, evd.AddedDate))

                                            AND YEAR(CONVERT(DATE, FYPA.EndDate))= YEAR(CONVERT(DATE, evd.AddedDate))

                                        WHERE 

                                         

                                          ACT.IsBalanceSheet = 0  AND ACT.Id IN ('Expense') " + wc + @" AND EVD.IsPark = 0 " + condition + @"

                                          GROUP BY EFYP.Id , EFYP.PeriodName,  FYPA.PeriodName, FYPA.EndDate,EFYP.EndDate,FYPA.EntryPeriodId ,ACT.BalanceType,EFYP.StartDate ,FYPA.StartDate
								    ) periodWiseExp

                            GROUP by periodWiseExp.PostingPeriod,periodWiseExp.PostingPeriodId, periodWiseExp.EntryPeriod,periodWiseExp.EntryPeriodId,
                            periodWiseExp.EntryPeriodEndDate, periodWiseExp.PostingPeriodEndDate,periodWiseExp.EntryPeriodStartDate,periodWiseExp.PostingPeriodStartDate
                           ORDER BY  periodWiseExp.PostingPeriodEndDate,periodWiseExp.EntryPeriodEndDate DESC";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public IEnumerable<object> DymnamicRevenueListLineChart(IEnumerable<ChartColumnList> ChartColumnList, int seq, string factDate, string fromDate, string toDate, string companyGroupId, string companyId)
        {
            var cList = string.Empty;
            var cListId = string.Empty;
            var join = string.Empty;
            var wc = string.Empty;
            var cListextG = string.Empty;
            var cListextIdG = string.Empty;
            var fDate = string.Empty;
            //string expFactDate = string.Empty;
            try
            {
                var expFactDate = string.Empty;
                var condition = string.Empty;
                var grpFactDate = string.Empty;

                if (factDate == "postingDate")
                {
                    expFactDate = "Replace(CONVERT(VARCHAR(11),V.PostingDate,6),' ','-') factDate,";
                    fDate = "V.PostingDate fDate,";
                    condition = " AND V.PostingDate BETWEEN CONVERT(DATE,'" + fromDate + @"') AND CONVERT(DATE,'" + toDate + @"')";
                    grpFactDate = "V.PostingDate";
                }
                else if (factDate == "AddedDate")
                {
                    expFactDate = "Replace(CONVERT(VARCHAR(11),V.AddedDate,6),' ','-') factDate,";
                    condition = "AND V.AddedDate BETWEEN CONVERT(DATE,'" + fromDate + @"') AND CONVERT(DATE,'" + toDate + @"')";
                    fDate = "V.AddedDate fDate,";
                    grpFactDate = "V.AddedDate";
                }
                seq += 1;
                foreach (var item in ChartColumnList)
                {
                    if (item.Sequence != -2 && item.Sequence != -1)
                    {
                        if (item.Sequence <= seq)
                        {
                            if (item.RType == "Entity")
                            {
                                cList = "," + item.StandardName + ".UserName";
                                cListId = "," + item.StandardName + ".Id";

                                if (item.StandardName == "EmployeeGroup")
                                {
                                    join += "LEFT JOIN [HKP].[" + item.StandardName + "] ON " + item.StandardName + ".Id = ENT." + item.StandardName + "Id\n";
                                }
                                else
                                {
                                    join += "LEFT JOIN [ORG].[" + item.StandardName + "] ON " + item.StandardName + ".Id = ENT." + item.StandardName + "Id\n";
                                }
                            }
                            else
                            {
                                cList = "," + item.StandardName + ".UserName";
                                join += "LEFT JOIN [ORG].[" + item.StandardName + "] ON " + item.StandardName + ".Id = Po." + item.StandardName + "Id\n";
                            }
                        }
                    }
                    if (item.Sequence != -2)
                    {
                        if (item.Sequence == -1)
                        {
                            wc = "  AND CMP.Id ='" + item.Id + "'";
                        }
                        else
                        {
                            if (item.Sequence < seq)
                            {
                                wc += " and " + item.StandardName + ".Id='" + item.Text + "'";
                            }
                        }
                    }
                }

                var sql = @"SELECT SUM(CASE WHEN ISNULL(periodWiseExp.DRcumulative,0)=0 THEN periodWiseExp.CRcumulative ELSE periodWiseExp.DRcumulative END) Amount, periodWiseExp.PostingPeriod,
                                periodWiseExp.PostingPeriodId, periodWiseExp.EntryPeriod,periodWiseExp.EntryPeriodId, periodWiseExp.EntryPeriodEndDate
								,periodWiseExp.PostingPeriodEndDate,periodWiseExp.EntryPeriodStartDate,periodWiseExp.PostingPeriodStartDate FROM
                                  (SELECT
                                          DRcumulative = CASE WHEN ACT.BalanceType = 'Debit' THEN SUM(VDC.DrAmount)-SUM(VDC.CrAmount) ELSE 0 END
								        , CRcumulative = CASE WHEN ACT.BalanceType = 'Credit' THEN SUM(VDC.crAmount)-SUM(VDC.DrAmount) ELSE 0 END
								        , EFYP.PeriodName PostingPeriod,
                                        EFYP.Id PostingPeriodId, FYPA.PeriodName AS EntryPeriod,FYPA.EntryPeriodId, FYPA.EndDate EntryPeriodEndDate
                                        , EFYP.EndDate PostingPeriodEndDate,FYPA.StartDate  EntryPeriodStartDate,EFYP.StartDate PostingPeriodStartDate

                                        FROM TRN.VoucherDetailCurrency AS VDC
                                        LEFT JOIN TRN.VoucherDetail AS EVD ON EVD.Id = VDC.VoucherDetailId
                                        LEFT JOIN TRN.Voucher AS V ON V.Id = EVD.VoucherId

                                        LEFT JOIN SCS.FiscalYearPeriod AS EFYP ON EFYP.Id = EVD.FiscalYearPeriodId

                                        LEFT JOIN MST.BudgetMaster AS BM ON BM.Id = EVD.BudgetMasterId

                                        LEFT JOIN HKP.BudgetCategory AS BC ON BC.Id = BM.BudgetCategoryId

                                        LEFT JOIN HKP.BudgetSubCategory AS BSC ON BSC.Id = BM.BudgetSubCategoryId

                                        LEFT JOIN HKP.Budget AS B ON B.Id = BM.BudgetId

                                        LEFT JOIN HKP.GLGeneralInfo AS GL ON GL.Id = BM.GLGeneralInfoId

                                        LEFT JOIN HKP.AccountGroup AS AG ON AG.Id = GL.AccountGroupId

                                        LEFT JOIN HKP.AccountType AS ACT ON ACT.Id = AG.AccountTypeId

                                        LEFT JOIN ORG.Entity AS ENT ON ENT.Id = V.EntityId
                                        LEFT JOIN ORG.Company AS cmp ON cmp.Id = V.CompanyId
									" + join + @"
                                        LEFT JOIN(SELECT Id AS EntryPeriodId, PeriodName, StartDate, EndDate FROM SCS.FiscalYearPeriod)

                                                    AS FYPA ON MONTH(CONVERT(DATE, FYPA.EndDate)) = MONTH(CONVERT(DATE, evd.AddedDate))

                                            AND YEAR(CONVERT(DATE, FYPA.EndDate))= YEAR(CONVERT(DATE, evd.AddedDate))

                                        WHERE 

                                         

                                          ACT.IsBalanceSheet = 0  AND ACT.Id IN ('Revenue') " + wc + @" AND EVD.IsPark = 0 " + condition + @"

                                          GROUP BY EFYP.Id , EFYP.PeriodName,  FYPA.PeriodName, FYPA.EndDate,EFYP.EndDate,FYPA.EntryPeriodId ,ACT.BalanceType,EFYP.StartDate ,FYPA.StartDate
								    ) periodWiseExp

                            GROUP by periodWiseExp.PostingPeriod,periodWiseExp.PostingPeriodId, periodWiseExp.EntryPeriod,periodWiseExp.EntryPeriodId,
                            periodWiseExp.EntryPeriodEndDate, periodWiseExp.PostingPeriodEndDate,periodWiseExp.EntryPeriodStartDate,periodWiseExp.PostingPeriodStartDate
                           ORDER BY  periodWiseExp.PostingPeriodEndDate,periodWiseExp.EntryPeriodEndDate DESC";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        #endregion

        #region Monthly Expense Vs Budget 
        public IEnumerable<object> MonthlyExpenseVSBudgetBarChart(string factDate, string fromDate, string toDate, string companyGroupId, string companyId)
        {
            var cList = string.Empty;
            var cListId = string.Empty;
            var join = string.Empty;
            var wc = string.Empty;
            var cListextG = string.Empty;
            var cListextIdG = string.Empty;
            var daysOfMonth = DateTime.DaysInMonth(Convert.ToDateTime(toDate).Year, Convert.ToDateTime(toDate).Month);

            DateTime calculatedToDate = new DateTime(Convert.ToDateTime(toDate).Year, Convert.ToDateTime(toDate).Month, daysOfMonth);

            DateTime calculatedFromDate = calculatedToDate.AddMonths(-1);
            calculatedFromDate = calculatedFromDate.AddDays(1);

            try
            {
                var expFactDate = string.Empty;
                var condition = string.Empty;
                var periodId = "";

                if (factDate == "postingDate")
                {
                    expFactDate = "Replace(CONVERT(VARCHAR(11),V.PostingDate,6),' ','-') factDate,";
                    condition = "AND V.PostingDate BETWEEN CONVERT(DATE,'" + calculatedFromDate.ToString("dd-MMM-yyyy") + @"') AND CONVERT(DATE,'" + calculatedToDate.ToString("dd-MMM-yyyy") + @"')";
                    periodId = "PostingPeriodId";
                }
                else if (factDate == "AddedDate")
                {
                    expFactDate = "Replace(CONVERT(VARCHAR(11),V.AddedDate,6),' ','-') factDate,";
                    condition = "AND V.AddedDate BETWEEN CONVERT(DATE,'" + calculatedFromDate.ToString("dd-MMM-yyyy") + @"') AND CONVERT(DATE,'" + calculatedToDate.ToString("dd-MMM-yyyy") + @"')";
                    periodId = "EntryPeriodId";

                }
                else
                {
                    condition = "";
                }

                var sql = @"SELECT ISNULL(Amount,0) Amount, ISNULL(BudgetAmount,0) BudgetAmount
									,FiscalYear.PeriodName PostingPeriod,
                                FiscalYear.Id PostingPeriodId				
								 FROM				
									  (
                                       (SELECT * FROM SCS.FiscalYearPeriod WHERE
									   StartDate BETWEEN (SELECT  StartDate FROM SCS.FiscalYearPeriod where '" + calculatedToDate.ToString("dd-MMM-yyyy") + @"' between StartDate and EndDate)
                                              AND(SELECT  EndDate FROM SCS.FiscalYearPeriod where '" + calculatedToDate.ToString("dd-MMM-yyyy") + @"' between StartDate and EndDate)
									  ) FiscalYear 
                                    Left join
                                    (SELECT
                                         SUM(AnnualBudgetDetail.StandardAmount) BudgetAmount
								        , EFYP.PeriodName PostingPeriod,
                                        EFYP.Id PostingPeriodId--, FYPA.PeriodName AS EntryPeriod,FYPA.EntryPeriodId, FYPA.EndDate EntryPeriodEndDate
                                        , EFYP.EndDate PostingPeriodEndDate
										--,BM.id
									     FROM
										 MST.AnnualBudgetDetail AS AnnualBudgetDetail	
										 Left join SCS.FiscalYearPeriod AS EFYP  ON EFYP.Id = AnnualBudgetDetail.FiscalYearPeriodId
										
										LEFT JOIN  MST.AnnualBudget AS AnnualBudget ON AnnualBudget.Id = AnnualBudgetDetail.AnnualBudgetId
                                        Left JOIN MST.BudgetMaster AS BM ON BM.Id = AnnualBudgetDetail.BudgetMasterId


                                        LEFT JOIN HKP.BudgetCategory AS BC ON BC.Id = BM.BudgetCategoryId

                                        LEFT JOIN HKP.BudgetSubCategory AS BSC ON BSC.Id = BM.BudgetSubCategoryId

                                        LEFT JOIN HKP.GLGeneralInfo AS GL ON GL.Id = BM.GLGeneralInfoId

                                        LEFT JOIN HKP.AccountGroup AS AG ON AG.Id = GL.AccountGroupId

                                        LEFT JOIN HKP.AccountType AS ACT ON ACT.Id = AG.AccountTypeId

                                        LEFT JOIN ORG.Entity AS ENT ON ENT.Id = AnnualBudgetDetail.EntityId                       

                                        WHERE AnnualBudgetDetail.CompanyGroupId = '" + companyGroupId + @"'  AND EFYP.StartDate BETWEEN (SELECT  StartDate FROM SCS.FiscalYearPeriod where '" + calculatedToDate.ToString("dd-MMM-yyyy") + @"' between StartDate and EndDate)
                                              AND(SELECT  EndDate FROM SCS.FiscalYearPeriod where '" + calculatedToDate.ToString("dd-MMM-yyyy") + @"' between StartDate and EndDate)

                                        AND ACT.IsBalanceSheet = 0  AND ACT.Id IN ('Expense')

                                          GROUP BY EFYP.Id , EFYP.PeriodName,EFYP.Id,EFYP.EndDate
										
								    ) periodWiseExpBudget ON FiscalYear.Id = periodWiseExpBudget.PostingPeriodId
									LEFT JOIN
									(SELECT SUM(CASE WHEN ISNULL(DRcumulative,0)=0 THEN periodWiseExp.CRcumulative ELSE periodWiseExp.DRcumulative END) Amount, periodWiseExp.PostingPeriod,
                                periodWiseExp.PostingPeriodId, periodWiseExp.EntryPeriod,periodWiseExp.EntryPeriodId, periodWiseExp.EntryPeriodEndDate
								,periodWiseExp.PostingPeriodEndDate FROM
                                  (SELECT
                                          DRcumulative = CASE WHEN ACT.BalanceType = 'Debit' THEN SUM(VDC.DrAmount)-SUM(VDC.CrAmount) ELSE 0 END
								        , CRcumulative = CASE WHEN ACT.BalanceType = 'Credit' THEN SUM(VDC.crAmount)-SUM(VDC.DrAmount) ELSE 0 END
								        , EFYP.PeriodName PostingPeriod,
                                        EFYP.Id PostingPeriodId, FYPA.PeriodName AS EntryPeriod,FYPA.EntryPeriodId, FYPA.EndDate EntryPeriodEndDate
                                        , EFYP.EndDate PostingPeriodEndDate

                                        FROM TRN.VoucherDetailCurrency AS VDC
                                        LEFT JOIN TRN.VoucherDetail AS EVD ON EVD.Id = VDC.VoucherDetailId
                                        LEFT JOIN TRN.Voucher AS V ON V.Id = EVD.VoucherId

                                        LEFT JOIN SCS.FiscalYearPeriod AS EFYP ON EFYP.Id = EVD.FiscalYearPeriodId

                                        LEFT JOIN MST.BudgetMaster AS BM ON BM.Id = EVD.BudgetMasterId

                                        LEFT JOIN HKP.BudgetCategory AS BC ON BC.Id = BM.BudgetCategoryId

                                        LEFT JOIN HKP.BudgetSubCategory AS BSC ON BSC.Id = BM.BudgetSubCategoryId

                                        LEFT JOIN HKP.Budget AS B ON B.Id = BM.BudgetId

                                        LEFT JOIN HKP.GLGeneralInfo AS GL ON GL.Id = BM.GLGeneralInfoId

                                        LEFT JOIN HKP.AccountGroup AS AG ON AG.Id = GL.AccountGroupId

                                        LEFT JOIN HKP.AccountType AS ACT ON ACT.Id = AG.AccountTypeId

                                        LEFT JOIN ORG.Entity AS ENT ON ENT.Id = V.EntityId

                                        LEFT JOIN(SELECT Id AS EntryPeriodId, PeriodName, StartDate, EndDate FROM SCS.FiscalYearPeriod)

                                                    AS FYPA ON MONTH(CONVERT(DATE, FYPA.EndDate)) = MONTH(CONVERT(DATE, evd.AddedDate))

                                            AND YEAR(CONVERT(DATE, FYPA.EndDate))= YEAR(CONVERT(DATE, evd.AddedDate))

                                        WHERE V.CompanyGroupId = '" + companyGroupId + @"' " + condition + @" AND V.IsPark = 0 

                                        AND ACT.IsBalanceSheet = 0  AND ACT.Id IN ('Expense')

                                          GROUP BY EFYP.Id , EFYP.PeriodName,  FYPA.PeriodName, FYPA.EndDate,EFYP.EndDate,FYPA.EntryPeriodId ,ACT.BalanceType
								    ) periodWiseExp

                            GROUP by periodWiseExp.PostingPeriod,periodWiseExp.PostingPeriodId, periodWiseExp.EntryPeriod,periodWiseExp.EntryPeriodId,
                            periodWiseExp.EntryPeriodEndDate, periodWiseExp.PostingPeriodEndDate	
                       ) periodWiseExp
									ON periodWiseExp." + periodId + @" = FiscalYear.Id  
									)";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public IEnumerable<object> MonthlyRevenueVSBudgetBarChart(string factDate, string fromDate, string toDate, string companyGroupId, string companyId)
        {
            var cList = string.Empty;
            var cListId = string.Empty;
            var join = string.Empty;
            var wc = string.Empty;
            var cListextG = string.Empty;
            var cListextIdG = string.Empty;
            var daysOfMonth = DateTime.DaysInMonth(Convert.ToDateTime(toDate).Year, Convert.ToDateTime(toDate).Month);
            var periodId = "";

            DateTime calculatedToDate = new DateTime(Convert.ToDateTime(toDate).Year, Convert.ToDateTime(toDate).Month, daysOfMonth);

            DateTime calculatedFromDate = calculatedToDate.AddMonths(-1);
            calculatedFromDate = calculatedFromDate.AddDays(1);
            try
            {
                var expFactDate = string.Empty;
                var condition = string.Empty;

                if (factDate == "postingDate")
                {
                    expFactDate = "Replace(CONVERT(VARCHAR(11),V.PostingDate,6),' ','-') factDate,";
                    condition = "AND V.PostingDate BETWEEN CONVERT(DATE,'" + calculatedFromDate.ToString("dd-MMM-yyyy") + @"') AND CONVERT(DATE,'" + calculatedToDate.ToString("dd-MMM-yyyy") + @"')";
                    // condition = "AND V.PostingDate BETWEEN CONVERT(DATE,'" + calculatedFromDate.ToString("dd-MMM-yyyy") + @"') AND CONVERT(DATE,'" + calculatedToDate.ToString("dd-MMM-yyyy") + @"')";
                    periodId = "PostingPeriodId";

                }
                else if (factDate == "AddedDate")
                {
                    expFactDate = "Replace(CONVERT(VARCHAR(11),V.AddedDate,6),' ','-') factDate,";
                    condition = "AND V.AddedDate BETWEEN CONVERT(DATE,'" + calculatedFromDate.ToString("dd-MMM-yyyy") + @"') AND CONVERT(DATE,'" + calculatedToDate.ToString("dd-MMM-yyyyy") + @"')";
                    periodId = "EntryPeriodId";
                }
                else
                {
                    condition = "";
                }

                var sql = @"SELECT ISNULL(Amount,0) Amount, ISNULL(BudgetAmount,0) BudgetAmount
									,FiscalYear.PeriodName PostingPeriod,
                                FiscalYear.Id PostingPeriodId				
								 FROM				
									  (
                                       (SELECT * FROM SCS.FiscalYearPeriod WHERE
									   StartDate BETWEEN (SELECT  StartDate FROM SCS.FiscalYearPeriod where '" + calculatedToDate.ToString("dd-MMM-yyyy") + @"' between StartDate and EndDate)
                                              AND(SELECT  EndDate FROM SCS.FiscalYearPeriod where '" + calculatedToDate.ToString("dd-MMM-yyyy") + @"' between StartDate and EndDate)
									  ) FiscalYear 
                                    Left join
                                    (SELECT
                                         SUM(AnnualBudgetDetail.StandardAmount) BudgetAmount
								        , EFYP.PeriodName PostingPeriod,
                                        EFYP.Id PostingPeriodId--, FYPA.PeriodName AS EntryPeriod,FYPA.EntryPeriodId, FYPA.EndDate EntryPeriodEndDate
                                        , EFYP.EndDate PostingPeriodEndDate
										--,BM.id
									     FROM
										 MST.AnnualBudgetDetail AS AnnualBudgetDetail	
										 Left join SCS.FiscalYearPeriod AS EFYP  ON EFYP.Id = AnnualBudgetDetail.FiscalYearPeriodId
										
										LEFT JOIN  MST.AnnualBudget AS AnnualBudget ON AnnualBudget.Id = AnnualBudgetDetail.AnnualBudgetId
                                        Left JOIN MST.BudgetMaster AS BM ON BM.Id = AnnualBudgetDetail.BudgetMasterId


                                        LEFT JOIN HKP.BudgetCategory AS BC ON BC.Id = BM.BudgetCategoryId

                                        LEFT JOIN HKP.BudgetSubCategory AS BSC ON BSC.Id = BM.BudgetSubCategoryId

                                        LEFT JOIN HKP.GLGeneralInfo AS GL ON GL.Id = BM.GLGeneralInfoId

                                        LEFT JOIN HKP.AccountGroup AS AG ON AG.Id = GL.AccountGroupId

                                        LEFT JOIN HKP.AccountType AS ACT ON ACT.Id = AG.AccountTypeId

                                        LEFT JOIN ORG.Entity AS ENT ON ENT.Id = AnnualBudgetDetail.EntityId                       

                                        WHERE AnnualBudgetDetail.CompanyGroupId = '" + companyGroupId + @"'  AND EFYP.StartDate BETWEEN (SELECT  StartDate FROM SCS.FiscalYearPeriod where '" + calculatedToDate.ToString("dd-MMM-yyyy") + @"' between StartDate and EndDate)
                                              AND(SELECT  EndDate FROM SCS.FiscalYearPeriod where '" + calculatedToDate.ToString("dd-MMM-yyyy") + @"' between StartDate and EndDate)

                                        AND ACT.IsBalanceSheet = 0  AND ACT.Id IN ('Revenue')

                                          GROUP BY EFYP.Id , EFYP.PeriodName,EFYP.Id,EFYP.EndDate
										
								    ) periodWiseExpBudget ON FiscalYear.Id = periodWiseExpBudget.PostingPeriodId
									LEFT JOIN
									(SELECT SUM(CASE WHEN ISNULL(DRcumulative,0)=0 THEN periodWiseExp.CRcumulative ELSE periodWiseExp.DRcumulative END) Amount, periodWiseExp.PostingPeriod,
                                periodWiseExp.PostingPeriodId, periodWiseExp.EntryPeriod,periodWiseExp.EntryPeriodId, periodWiseExp.EntryPeriodEndDate
								,periodWiseExp.PostingPeriodEndDate FROM
                                  (SELECT
                                          DRcumulative = CASE WHEN ACT.BalanceType = 'Debit' THEN SUM(VDC.DrAmount)-SUM(VDC.CrAmount) ELSE 0 END
								        , CRcumulative = CASE WHEN ACT.BalanceType = 'Credit' THEN SUM(VDC.crAmount)-SUM(VDC.DrAmount) ELSE 0 END
								        , EFYP.PeriodName PostingPeriod,
                                        EFYP.Id PostingPeriodId, FYPA.PeriodName AS EntryPeriod,FYPA.EntryPeriodId, FYPA.EndDate EntryPeriodEndDate
                                        , EFYP.EndDate PostingPeriodEndDate

                                        FROM TRN.VoucherDetailCurrency AS VDC
                                        LEFT JOIN TRN.VoucherDetail AS EVD ON EVD.Id = VDC.VoucherDetailId
                                        LEFT JOIN TRN.Voucher AS V ON V.Id = EVD.VoucherId

                                        LEFT JOIN SCS.FiscalYearPeriod AS EFYP ON EFYP.Id = EVD.FiscalYearPeriodId

                                        LEFT JOIN MST.BudgetMaster AS BM ON BM.Id = EVD.BudgetMasterId

                                        LEFT JOIN HKP.BudgetCategory AS BC ON BC.Id = BM.BudgetCategoryId

                                        LEFT JOIN HKP.BudgetSubCategory AS BSC ON BSC.Id = BM.BudgetSubCategoryId

                                        LEFT JOIN HKP.Budget AS B ON B.Id = BM.BudgetId

                                        LEFT JOIN HKP.GLGeneralInfo AS GL ON GL.Id = BM.GLGeneralInfoId

                                        LEFT JOIN HKP.AccountGroup AS AG ON AG.Id = GL.AccountGroupId

                                        LEFT JOIN HKP.AccountType AS ACT ON ACT.Id = AG.AccountTypeId

                                        LEFT JOIN ORG.Entity AS ENT ON ENT.Id = V.EntityId

                                        LEFT JOIN(SELECT Id AS EntryPeriodId, PeriodName, StartDate, EndDate FROM SCS.FiscalYearPeriod)

                                                    AS FYPA ON MONTH(CONVERT(DATE, FYPA.EndDate)) = MONTH(CONVERT(DATE, evd.AddedDate))

                                            AND YEAR(CONVERT(DATE, FYPA.EndDate))= YEAR(CONVERT(DATE, evd.AddedDate))

                                        WHERE V.CompanyGroupId = '" + companyGroupId + @"' " + condition + @" AND V.IsPark = 0 

                                        AND ACT.IsBalanceSheet = 0  AND ACT.Id IN ('Revenue')

                                          GROUP BY EFYP.Id , EFYP.PeriodName,  FYPA.PeriodName, FYPA.EndDate,EFYP.EndDate,FYPA.EntryPeriodId ,ACT.BalanceType
								    ) periodWiseExp

                            GROUP by periodWiseExp.PostingPeriod,periodWiseExp.PostingPeriodId, periodWiseExp.EntryPeriod,periodWiseExp.EntryPeriodId,
                            periodWiseExp.EntryPeriodEndDate, periodWiseExp.PostingPeriodEndDate	
                       ) periodWiseExp
									ON periodWiseExp." + periodId + @" = FiscalYear.Id  
									)";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public IEnumerable<object> MonthlyDynamicExpenseVSBudgetBarChart(IEnumerable<ChartColumnList> ChartColumnList, int seq, string factDate, string fromDate, string toDate, string companyGroupId, string companyId)
        {
            var cList = string.Empty;
            var cListId = string.Empty;
            var join = string.Empty;
            var wc = string.Empty;
            var cListextG = string.Empty;
            var cListextIdG = string.Empty;
            var daysOfMonth = DateTime.DaysInMonth(Convert.ToDateTime(toDate).Year, Convert.ToDateTime(toDate).Month);
            var periodId = "";
            DateTime calculatedToDate = new DateTime(Convert.ToDateTime(toDate).Year, Convert.ToDateTime(toDate).Month, daysOfMonth);

            DateTime calculatedFromDate = calculatedToDate.AddMonths(-1);
            calculatedFromDate = calculatedFromDate.AddDays(1);
            try
            {
                var expFactDate = string.Empty;
                var condition = string.Empty;

                if (factDate == "postingDate")
                {
                    expFactDate = "Replace(CONVERT(VARCHAR(11),V.PostingDate,6),' ','-') factDate,";
                    condition = "AND V.PostingDate BETWEEN CONVERT(DATE,'" + calculatedFromDate.ToString("dd-MMM-yyyy") + @"') AND CONVERT(DATE,'" + calculatedToDate.ToString("dd-MMM-yyyy") + @"')";
                    periodId = "PostingPeriodId";
                }
                else if (factDate == "AddedDate")
                {
                    expFactDate = "Replace(CONVERT(VARCHAR(11),V.AddedDate,6),' ','-') factDate,";
                    condition = "AND V.AddedDate BETWEEN CONVERT(DATE,'" + calculatedFromDate.ToString("dd-MMM-yyyy") + @"') AND CONVERT(DATE,'" + calculatedToDate.ToString("dd-MMM-yyyy") + @"')";
                    periodId = "EntryPeriodId";

                }
                else
                {
                    condition = "";
                }
                seq += 1;
                foreach (var item in ChartColumnList)
                {
                    if (item.Sequence != -2 && item.Sequence != -1)
                    {
                        if (item.Sequence <= seq)
                        {
                            if (item.RType == "Entity")
                            {
                                cList = "," + item.StandardName + ".UserName";
                                cListId = "," + item.StandardName + ".Id";
                                //cListextG = "," + item.ColumnName + "Name";
                                //cListextIdG = "," + item.ColumnName + "Id";
                                if (item.ColumnName == "EmployeeGroup")
                                {
                                    join += "LEFT JOIN [HKP].[" + item.StandardName + "] ON " + item.StandardName + ".Id = ENT." + item.StandardName + "Id\n";
                                }
                                else
                                {
                                    join += "LEFT JOIN [ORG].[" + item.StandardName + "] ON " + item.StandardName + ".Id = ENT." + item.StandardName + "Id\n";
                                }
                            }
                            else
                            {
                                cList = "," + item.StandardName + ".UserName";
                                join += "LEFT JOIN [ORG].[" + item.StandardName + "] ON " + item.StandardName + ".Id = Po." + item.StandardName + "Id\n";
                            }
                        }
                    }
                    if (item.Sequence != -2)
                    {
                        if (item.Sequence == -1)
                        {
                            wc = "  AND CMP.Id ='" + item.Id + "'";
                        }
                        else
                        {
                            if (item.Sequence < seq)
                            {
                                wc += " and " + item.StandardName + ".Id='" + item.Text + "'";
                            }
                        }
                    }
                }

                var sql = @"SELECT ISNULL(Amount,0) Amount, ISNULL(BudgetAmount,0) BudgetAmount
									,FiscalYear.PeriodName PostingPeriod,
                                FiscalYear.Id PostingPeriodId				
								 FROM				
									  (
                                       (SELECT * FROM SCS.FiscalYearPeriod WHERE
									   StartDate BETWEEN (SELECT  StartDate FROM SCS.FiscalYearPeriod where '" + calculatedToDate.ToString("dd-MMM-yyyy") + @"' between StartDate and EndDate)
                                              AND(SELECT  EndDate FROM SCS.FiscalYearPeriod where '" + calculatedToDate.ToString("dd-MMM-yyyy") + @"' between StartDate and EndDate)
									  ) FiscalYear 
                                    Left join
                                    (SELECT
                                         SUM(AnnualBudgetDetail.StandardAmount) BudgetAmount
								        , EFYP.PeriodName PostingPeriod,
                                        EFYP.Id PostingPeriodId--, FYPA.PeriodName AS EntryPeriod,FYPA.EntryPeriodId, FYPA.EndDate EntryPeriodEndDate
                                        , EFYP.EndDate PostingPeriodEndDate
										     ,    cmp.Id AS CompanyId
								           	" + cList + @" AS ColumnName
									         " + cListId + @" AS UId
									     FROM
										 MST.AnnualBudgetDetail AS AnnualBudgetDetail	
										 Left join SCS.FiscalYearPeriod AS EFYP  ON EFYP.Id = AnnualBudgetDetail.FiscalYearPeriodId
										
										LEFT JOIN  MST.AnnualBudget AS AnnualBudget ON AnnualBudget.Id = AnnualBudgetDetail.AnnualBudgetId
                                        Left JOIN MST.BudgetMaster AS BM ON BM.Id = AnnualBudgetDetail.BudgetMasterId
                                        LEFT JOIN ORG.Company AS CMP ON CMP.Id = AnnualBudgetDetail.CompanyId


                                        LEFT JOIN HKP.BudgetCategory AS BC ON BC.Id = BM.BudgetCategoryId

                                        LEFT JOIN HKP.BudgetSubCategory AS BSC ON BSC.Id = BM.BudgetSubCategoryId

                                        LEFT JOIN HKP.GLGeneralInfo AS GL ON GL.Id = BM.GLGeneralInfoId

                                        LEFT JOIN HKP.AccountGroup AS AG ON AG.Id = GL.AccountGroupId

                                        LEFT JOIN HKP.AccountType AS ACT ON ACT.Id = AG.AccountTypeId

                                        LEFT JOIN ORG.Entity AS ENT ON ENT.Id = AnnualBudgetDetail.EntityId                       
                                       
                                        " + join + @"

                                        WHERE AnnualBudgetDetail.CompanyGroupId = '" + companyGroupId + @"'  AND EFYP.StartDate BETWEEN (SELECT  StartDate FROM SCS.FiscalYearPeriod where '" + calculatedToDate.ToString("dd-MMM-yyyy") + @"' between StartDate and EndDate)
                                              AND(SELECT  EndDate FROM SCS.FiscalYearPeriod where '" + calculatedToDate.ToString("dd-MMM-yyyy") + @"' between StartDate and EndDate)

                                        AND ACT.IsBalanceSheet = 0  AND ACT.Id IN ('Expense')" + wc + @"

                                          GROUP BY EFYP.Id , EFYP.PeriodName,EFYP.Id,EFYP.EndDate " + cList + @"  " + cListId + @", cmp.Id
										
								    ) periodWiseExpBudget ON FiscalYear.Id = periodWiseExpBudget.PostingPeriodId
									LEFT JOIN
									(SELECT SUM(CASE WHEN ISNULL(DRcumulative,0)=0 THEN periodWiseExp.CRcumulative ELSE periodWiseExp.DRcumulative END) Amount, periodWiseExp.PostingPeriod,
                                periodWiseExp.PostingPeriodId, periodWiseExp.EntryPeriod,periodWiseExp.EntryPeriodId, periodWiseExp.EntryPeriodEndDate
								,periodWiseExp.PostingPeriodEndDate FROM
                                  (SELECT
                                          DRcumulative = CASE WHEN ACT.BalanceType = 'Debit' THEN SUM(VDC.DrAmount)-SUM(VDC.CrAmount) ELSE 0 END
								        , CRcumulative = CASE WHEN ACT.BalanceType = 'Credit' THEN SUM(VDC.crAmount)-SUM(VDC.DrAmount) ELSE 0 END
								        , EFYP.PeriodName PostingPeriod,
                                        EFYP.Id PostingPeriodId, FYPA.PeriodName AS EntryPeriod,FYPA.EntryPeriodId, FYPA.EndDate EntryPeriodEndDate
                                        , EFYP.EndDate PostingPeriodEndDate
                                         , cmp.Id AS CompanyId

                                               " + cList + @" AS ColumnName

                                             " + cListId + @" AS UId

                                        FROM TRN.VoucherDetailCurrency AS VDC
                                        LEFT JOIN TRN.VoucherDetail AS EVD ON EVD.Id = VDC.VoucherDetailId
                                        LEFT JOIN TRN.Voucher AS V ON V.Id = EVD.VoucherId
                                        LEFT JOIN ORG.Company AS CMP ON CMP.Id = V.CompanyId

                                        LEFT JOIN SCS.FiscalYearPeriod AS EFYP ON EFYP.Id = EVD.FiscalYearPeriodId

                                        LEFT JOIN MST.BudgetMaster AS BM ON BM.Id = EVD.BudgetMasterId

                                        LEFT JOIN HKP.BudgetCategory AS BC ON BC.Id = BM.BudgetCategoryId

                                        LEFT JOIN HKP.BudgetSubCategory AS BSC ON BSC.Id = BM.BudgetSubCategoryId

                                        LEFT JOIN HKP.Budget AS B ON B.Id = BM.BudgetId

                                        LEFT JOIN HKP.GLGeneralInfo AS GL ON GL.Id = BM.GLGeneralInfoId

                                        LEFT JOIN HKP.AccountGroup AS AG ON AG.Id = GL.AccountGroupId

                                        LEFT JOIN HKP.AccountType AS ACT ON ACT.Id = AG.AccountTypeId

                                        LEFT JOIN ORG.Entity AS ENT ON ENT.Id = V.EntityId
                                            
                                        " + join + @"                                                                                                    

                                        LEFT JOIN(SELECT Id AS EntryPeriodId, PeriodName, StartDate, EndDate FROM SCS.FiscalYearPeriod)

                                                    AS FYPA ON MONTH(CONVERT(DATE, FYPA.EndDate)) = MONTH(CONVERT(DATE, evd.AddedDate))

                                            AND YEAR(CONVERT(DATE, FYPA.EndDate))= YEAR(CONVERT(DATE, evd.AddedDate))

                                        WHERE V.CompanyGroupId = '" + companyGroupId + @"' " + condition + @" AND V.IsPark = 0 

                                        AND ACT.IsBalanceSheet = 0  AND ACT.Id IN ('Expense')

                                    " + wc + @"

                                          GROUP BY EFYP.Id , EFYP.PeriodName,  FYPA.PeriodName, FYPA.EndDate,EFYP.EndDate,FYPA.EntryPeriodId ,ACT.BalanceType
                                            " + cList + @"  " + cListId + @", cmp.Id
			                    ) periodWiseExp

                            GROUP by periodWiseExp.PostingPeriod,periodWiseExp.PostingPeriodId, periodWiseExp.EntryPeriod,periodWiseExp.EntryPeriodId,
                            periodWiseExp.EntryPeriodEndDate, periodWiseExp.PostingPeriodEndDate

                       ) periodWiseExp
									ON periodWiseExp." + periodId + @" = FiscalYear.Id  
									)";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public IEnumerable<object> MonthlyDynamicRevenueVSBudgetBarChart(IEnumerable<ChartColumnList> ChartColumnList, int seq, string factDate, string fromDate, string toDate, string companyGroupId, string companyId)
        {
            var cList = string.Empty;
            var cListId = string.Empty;
            var join = string.Empty;
            var wc = string.Empty;
            var cListextG = string.Empty;
            var cListextIdG = string.Empty;
            var daysOfMonth = DateTime.DaysInMonth(Convert.ToDateTime(toDate).Year, Convert.ToDateTime(toDate).Month);

            DateTime calculatedToDate = new DateTime(Convert.ToDateTime(toDate).Year, Convert.ToDateTime(toDate).Month, daysOfMonth);
            var periodId = "";
            DateTime calculatedFromDate = calculatedToDate.AddMonths(-1);
            calculatedFromDate = calculatedFromDate.AddDays(1);
            try
            {
                var expFactDate = string.Empty;
                var condition = string.Empty;

                if (factDate == "postingDate")
                {
                    expFactDate = "Replace(CONVERT(VARCHAR(11),V.PostingDate,6),' ','-') factDate,";
                    condition = "AND V.PostingDate BETWEEN CONVERT(DATE,'" + calculatedFromDate.ToString("dd-MMM-yyyy") + @"') AND CONVERT(DATE,'" + calculatedToDate.ToString("dd-MMM-yyyy") + @"')";
                    periodId = "PostingPeriodId";
                }
                else if (factDate == "AddedDate")
                {
                    expFactDate = "Replace(CONVERT(VARCHAR(11),V.AddedDate,6),' ','-') factDate,";
                    condition = "AND V.AddedDate BETWEEN CONVERT(DATE,'" + calculatedFromDate.ToString("dd-MMM-yyyy") + @"') AND CONVERT(DATE,'" + calculatedToDate.ToString("dd-MMM-yyyy") + @"')";
                    periodId = "EntryPeriodId";

                }
                else
                {
                    condition = "";
                }
                seq += 1;
                foreach (var item in ChartColumnList)
                {
                    if (item.Sequence != -2 && item.Sequence != -1)
                    {
                        if (item.Sequence <= seq)
                        {
                            if (item.RType == "Entity")
                            {
                                cList = "," + item.StandardName + ".UserName";
                                cListId = "," + item.StandardName + ".Id";
                                //cListextG = "," + item.ColumnName + "Name";
                                //cListextIdG = "," + item.ColumnName + "Id";
                                if (item.ColumnName == "EmployeeGroup")
                                {
                                    join += "LEFT JOIN [HKP].[" + item.StandardName + "] ON " + item.StandardName + ".Id = ENT." + item.StandardName + "Id\n";
                                }
                                else
                                {
                                    join += "LEFT JOIN [ORG].[" + item.StandardName + "] ON " + item.StandardName + ".Id = ENT." + item.StandardName + "Id\n";
                                }
                            }
                            else
                            {
                                cList = "," + item.StandardName + ".UserName";
                                join += "LEFT JOIN [ORG].[" + item.StandardName + "] ON " + item.StandardName + ".Id = Po." + item.StandardName + "Id\n";
                            }
                        }
                    }
                    if (item.Sequence != -2)
                    {
                        if (item.Sequence == -1)
                        {
                            wc = "  AND CMP.Id ='" + item.Id + "'";
                        }
                        else
                        {
                            if (item.Sequence < seq)
                            {
                                wc += " and " + item.StandardName + ".Id='" + item.Text + "'";
                            }
                        }
                    }
                }

                var sql = @"SELECT ISNULL(Amount,0) Amount, ISNULL(BudgetAmount,0) BudgetAmount
									,FiscalYear.PeriodName PostingPeriod,
                                FiscalYear.Id PostingPeriodId				
								 FROM				
									  (
                                       (SELECT * FROM SCS.FiscalYearPeriod WHERE
									   StartDate BETWEEN (SELECT  StartDate FROM SCS.FiscalYearPeriod where '" + calculatedToDate.ToString("dd-MMM-yyyy") + @"' between StartDate and EndDate)
                                              AND(SELECT  EndDate FROM SCS.FiscalYearPeriod where '" + calculatedToDate.ToString("dd-MMM-yyyy") + @"' between StartDate and EndDate)
									  ) FiscalYear 
                                    Left join
                                    (SELECT
                                         SUM(AnnualBudgetDetail.StandardAmount) BudgetAmount
								        , EFYP.PeriodName PostingPeriod,
                                        EFYP.Id PostingPeriodId--, FYPA.PeriodName AS EntryPeriod,FYPA.EntryPeriodId, FYPA.EndDate EntryPeriodEndDate
                                        , EFYP.EndDate PostingPeriodEndDate
										     ,    cmp.Id AS CompanyId
								           	" + cList + @" AS ColumnName
									         " + cListId + @" AS UId
									     FROM
										 MST.AnnualBudgetDetail AS AnnualBudgetDetail	
										 Left join SCS.FiscalYearPeriod AS EFYP  ON EFYP.Id = AnnualBudgetDetail.FiscalYearPeriodId
										
										LEFT JOIN  MST.AnnualBudget AS AnnualBudget ON AnnualBudget.Id = AnnualBudgetDetail.AnnualBudgetId
                                        Left JOIN MST.BudgetMaster AS BM ON BM.Id = AnnualBudgetDetail.BudgetMasterId

                                        LEFT JOIN ORG.Company AS CMP ON CMP.Id = AnnualBudgetDetail.CompanyId

                                        LEFT JOIN HKP.BudgetCategory AS BC ON BC.Id = BM.BudgetCategoryId

                                        LEFT JOIN HKP.BudgetSubCategory AS BSC ON BSC.Id = BM.BudgetSubCategoryId

                                        LEFT JOIN HKP.GLGeneralInfo AS GL ON GL.Id = BM.GLGeneralInfoId

                                        LEFT JOIN HKP.AccountGroup AS AG ON AG.Id = GL.AccountGroupId

                                        LEFT JOIN HKP.AccountType AS ACT ON ACT.Id = AG.AccountTypeId

                                        LEFT JOIN ORG.Entity AS ENT ON ENT.Id = AnnualBudgetDetail.EntityId                       
                                       
                                        " + join + @"

                                        WHERE AnnualBudgetDetail.CompanyGroupId = '" + companyGroupId + @"'  AND EFYP.StartDate BETWEEN (SELECT  StartDate FROM SCS.FiscalYearPeriod where '" + calculatedToDate.ToString("dd-MMM-yyyy") + @"' between StartDate and EndDate)
                                              AND(SELECT  EndDate FROM SCS.FiscalYearPeriod where '" + calculatedToDate.ToString("dd-MMM-yyyy") + @"' between StartDate and EndDate)

                                        AND ACT.IsBalanceSheet = 0  AND ACT.Id IN ('Revenue')" + wc + @"

                                          GROUP BY EFYP.Id , EFYP.PeriodName,EFYP.Id,EFYP.EndDate " + cList + @"  " + cListId + @", cmp.Id
										
								    ) periodWiseExpBudget ON FiscalYear.Id = periodWiseExpBudget.PostingPeriodId
									LEFT JOIN
									(SELECT SUM(CASE WHEN ISNULL(DRcumulative,0)=0 THEN periodWiseExp.CRcumulative ELSE periodWiseExp.DRcumulative END) Amount, periodWiseExp.PostingPeriod,
                                periodWiseExp.PostingPeriodId, periodWiseExp.EntryPeriod,periodWiseExp.EntryPeriodId, periodWiseExp.EntryPeriodEndDate
								,periodWiseExp.PostingPeriodEndDate FROM
                                  (SELECT
                                          DRcumulative = CASE WHEN ACT.BalanceType = 'Debit' THEN SUM(VDC.DrAmount)-SUM(VDC.CrAmount) ELSE 0 END
								        , CRcumulative = CASE WHEN ACT.BalanceType = 'Credit' THEN SUM(VDC.crAmount)-SUM(VDC.DrAmount) ELSE 0 END
								        , EFYP.PeriodName PostingPeriod,
                                        EFYP.Id PostingPeriodId, FYPA.PeriodName AS EntryPeriod,FYPA.EntryPeriodId, FYPA.EndDate EntryPeriodEndDate
                                        , EFYP.EndDate PostingPeriodEndDate
                                         , cmp.Id AS CompanyId

                                               " + cList + @" AS ColumnName

                                             " + cListId + @" AS UId

                                        FROM TRN.VoucherDetailCurrency AS VDC
                                        LEFT JOIN TRN.VoucherDetail AS EVD ON EVD.Id = VDC.VoucherDetailId
                                        LEFT JOIN TRN.Voucher AS V ON V.Id = EVD.VoucherId
                                        LEFT JOIN ORG.Company AS CMP ON CMP.Id = V.CompanyId

                                        LEFT JOIN SCS.FiscalYearPeriod AS EFYP ON EFYP.Id = EVD.FiscalYearPeriodId

                                        LEFT JOIN MST.BudgetMaster AS BM ON BM.Id = EVD.BudgetMasterId

                                        LEFT JOIN HKP.BudgetCategory AS BC ON BC.Id = BM.BudgetCategoryId

                                        LEFT JOIN HKP.BudgetSubCategory AS BSC ON BSC.Id = BM.BudgetSubCategoryId

                                        LEFT JOIN HKP.Budget AS B ON B.Id = BM.BudgetId

                                        LEFT JOIN HKP.GLGeneralInfo AS GL ON GL.Id = BM.GLGeneralInfoId

                                        LEFT JOIN HKP.AccountGroup AS AG ON AG.Id = GL.AccountGroupId

                                        LEFT JOIN HKP.AccountType AS ACT ON ACT.Id = AG.AccountTypeId

                                        LEFT JOIN ORG.Entity AS ENT ON ENT.Id = V.EntityId
                                            
                                        " + join + @"                                                                                                    

                                        LEFT JOIN(SELECT Id AS EntryPeriodId, PeriodName, StartDate, EndDate FROM SCS.FiscalYearPeriod)

                                                    AS FYPA ON MONTH(CONVERT(DATE, FYPA.EndDate)) = MONTH(CONVERT(DATE, evd.AddedDate))

                                            AND YEAR(CONVERT(DATE, FYPA.EndDate))= YEAR(CONVERT(DATE, evd.AddedDate))

                                        WHERE V.CompanyGroupId = '" + companyGroupId + @"' " + condition + @" AND V.IsPark = 0 

                                        AND ACT.IsBalanceSheet = 0  AND ACT.Id IN ('Revenue')

                                    " + wc + @"

                                          GROUP BY EFYP.Id , EFYP.PeriodName,  FYPA.PeriodName, FYPA.EndDate,EFYP.EndDate,FYPA.EntryPeriodId ,ACT.BalanceType
                                            " + cList + @"  " + cListId + @", cmp.Id
			                    ) periodWiseExp

                            GROUP by periodWiseExp.PostingPeriod,periodWiseExp.PostingPeriodId, periodWiseExp.EntryPeriod,periodWiseExp.EntryPeriodId,
                            periodWiseExp.EntryPeriodEndDate, periodWiseExp.PostingPeriodEndDate

                       ) periodWiseExp
									ON periodWiseExp." + periodId + @" = FiscalYear.Id )";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        #endregion
        #region Budget VS Actual (From Date - To Date) -> PERIOD WISE
        /// <summary>
        /// Expense Budget VS Actual
        /// </summary>
        /// <param name="factDate"></param>
        /// <param name="fromDate"></param>
        /// <param name="toDate"></param>
        /// <param name="companyGroupId"></param>
        /// <param name="companyId"></param>
        /// <returns></returns>
        public IEnumerable<object> PeriodExpenseVSBudgetBarChart(string factDate, string fromDate, string toDate, string companyGroupId, string companyId)
        {
            var cList = string.Empty;
            var cListId = string.Empty;
            var join = string.Empty;
            var wc = string.Empty;
            var cListextG = string.Empty;
            var cListextIdG = string.Empty;
            var daysOfMonth = DateTime.DaysInMonth(Convert.ToDateTime(toDate).Year, Convert.ToDateTime(toDate).Month);
            var periodId = "";
            DateTime calculatedToDate = new DateTime(Convert.ToDateTime(toDate).Year, Convert.ToDateTime(toDate).Month, daysOfMonth);

            DateTime calculatedFromDate = calculatedToDate.AddMonths(-12);
            try
            {
                var expFactDate = string.Empty;
                var condition = string.Empty;

                if (factDate == "postingDate")
                {
                    expFactDate = "Replace(CONVERT(VARCHAR(11),V.PostingDate,6),' ','-') factDate,";
                    condition = "AND V.PostingDate BETWEEN CONVERT(DATE,'" + fromDate + @"') AND CONVERT(DATE,'" + toDate + @"')";
                    // condition = "AND V.PostingDate BETWEEN CONVERT(DATE,'" + calculatedFromDate.ToString("dd-MMM-yyyy") + @"') AND CONVERT(DATE,'" + calculatedToDate.ToString("dd-MMM-yyyy") + @"')";
                    periodId = "PostingPeriodId";
                }
                else if (factDate == "AddedDate")
                {
                    expFactDate = "Replace(CONVERT(VARCHAR(11),V.AddedDate,6),' ','-') factDate,";
                    condition = "AND V.AddedDate BETWEEN CONVERT(DATE,'" + fromDate + @"') AND CONVERT(DATE,'" + toDate + @"')";
                    periodId = "EntryPeriodId";

                }
                else
                {
                    condition = "";
                }

                var sql = @"SELECT ISNULL(Amount,0) Amount, ISNULL(BudgetAmount,0) BudgetAmount
									,FiscalYear.PeriodName PostingPeriod,
                                FiscalYear.Id PostingPeriodId				
								 FROM				
									  (
                                       (SELECT * FROM SCS.FiscalYearPeriod WHERE
									   StartDate BETWEEN (SELECT  StartDate FROM SCS.FiscalYearPeriod where '" + calculatedToDate.ToString("dd-MMM-yyyy") + @"' between StartDate and EndDate)
                                              AND(SELECT  EndDate FROM SCS.FiscalYearPeriod where '" + calculatedToDate.ToString("dd-MMM-yyyy") + @"' between StartDate and EndDate)
									  ) FiscalYear 
                                    Left join
                                    (SELECT
                                         SUM(AnnualBudgetDetail.StandardAmount) BudgetAmount
								        , EFYP.PeriodName PostingPeriod,
                                        EFYP.Id PostingPeriodId--, FYPA.PeriodName AS EntryPeriod,FYPA.EntryPeriodId, FYPA.EndDate EntryPeriodEndDate
                                        , EFYP.EndDate PostingPeriodEndDate
										--,BM.id
									     FROM
										 MST.AnnualBudgetDetail AS AnnualBudgetDetail	
										 Left join SCS.FiscalYearPeriod AS EFYP  ON EFYP.Id = AnnualBudgetDetail.FiscalYearPeriodId
										
										LEFT JOIN  MST.AnnualBudget AS AnnualBudget ON AnnualBudget.Id = AnnualBudgetDetail.AnnualBudgetId
                                        Left JOIN MST.BudgetMaster AS BM ON BM.Id = AnnualBudgetDetail.BudgetMasterId


                                        LEFT JOIN HKP.BudgetCategory AS BC ON BC.Id = BM.BudgetCategoryId

                                        LEFT JOIN HKP.BudgetSubCategory AS BSC ON BSC.Id = BM.BudgetSubCategoryId

                                        LEFT JOIN HKP.GLGeneralInfo AS GL ON GL.Id = BM.GLGeneralInfoId

                                        LEFT JOIN HKP.AccountGroup AS AG ON AG.Id = GL.AccountGroupId

                                        LEFT JOIN HKP.AccountType AS ACT ON ACT.Id = AG.AccountTypeId

                                        LEFT JOIN ORG.Entity AS ENT ON ENT.Id = AnnualBudgetDetail.EntityId                       

                                        WHERE AnnualBudgetDetail.CompanyGroupId = '" + companyGroupId + @"'  AND EFYP.StartDate BETWEEN (SELECT  StartDate FROM SCS.FiscalYearPeriod where '" + calculatedToDate.ToString("dd-MMM-yyyy") + @"' between StartDate and EndDate)
                                              AND(SELECT  EndDate FROM SCS.FiscalYearPeriod where '" + calculatedToDate.ToString("dd-MMM-yyyy") + @"' between StartDate and EndDate)

                                        AND ACT.IsBalanceSheet = 0  AND ACT.Id IN ('Expense')

                                          GROUP BY EFYP.Id , EFYP.PeriodName,EFYP.Id,EFYP.EndDate
										
								    ) periodWiseExpBudget ON FiscalYear.Id = periodWiseExpBudget.PostingPeriodId
									LEFT JOIN
									(SELECT SUM(CASE WHEN ISNULL(DRcumulative,0)=0 THEN periodWiseExp.CRcumulative ELSE periodWiseExp.DRcumulative END) Amount, periodWiseExp.PostingPeriod,
                                periodWiseExp.PostingPeriodId, periodWiseExp.EntryPeriod,periodWiseExp.EntryPeriodId, periodWiseExp.EntryPeriodEndDate
								,periodWiseExp.PostingPeriodEndDate FROM
                                  (SELECT
                                          DRcumulative = CASE WHEN ACT.BalanceType = 'Debit' THEN SUM(VDC.DrAmount)-SUM(VDC.CrAmount) ELSE 0 END
								        , CRcumulative = CASE WHEN ACT.BalanceType = 'Credit' THEN SUM(VDC.crAmount)-SUM(VDC.DrAmount) ELSE 0 END
								        , EFYP.PeriodName PostingPeriod,
                                        EFYP.Id PostingPeriodId, FYPA.PeriodName AS EntryPeriod,FYPA.EntryPeriodId, FYPA.EndDate EntryPeriodEndDate
                                        , EFYP.EndDate PostingPeriodEndDate

                                        FROM TRN.VoucherDetailCurrency AS VDC
                                        LEFT JOIN TRN.VoucherDetail AS EVD ON EVD.Id = VDC.VoucherDetailId
                                        LEFT JOIN TRN.Voucher AS V ON V.Id = EVD.VoucherId

                                        LEFT JOIN SCS.FiscalYearPeriod AS EFYP ON EFYP.Id = EVD.FiscalYearPeriodId

                                        LEFT JOIN MST.BudgetMaster AS BM ON BM.Id = EVD.BudgetMasterId

                                        LEFT JOIN HKP.BudgetCategory AS BC ON BC.Id = BM.BudgetCategoryId

                                        LEFT JOIN HKP.BudgetSubCategory AS BSC ON BSC.Id = BM.BudgetSubCategoryId

                                        LEFT JOIN HKP.Budget AS B ON B.Id = BM.BudgetId

                                        LEFT JOIN HKP.GLGeneralInfo AS GL ON GL.Id = BM.GLGeneralInfoId

                                        LEFT JOIN HKP.AccountGroup AS AG ON AG.Id = GL.AccountGroupId

                                        LEFT JOIN HKP.AccountType AS ACT ON ACT.Id = AG.AccountTypeId

                                        LEFT JOIN ORG.Entity AS ENT ON ENT.Id = V.EntityId

                                        LEFT JOIN(SELECT Id AS EntryPeriodId, PeriodName, StartDate, EndDate FROM SCS.FiscalYearPeriod)

                                                    AS FYPA ON MONTH(CONVERT(DATE, FYPA.EndDate)) = MONTH(CONVERT(DATE, evd.AddedDate))

                                            AND YEAR(CONVERT(DATE, FYPA.EndDate))= YEAR(CONVERT(DATE, evd.AddedDate))

                                        WHERE V.CompanyGroupId = '" + companyGroupId + @"' " + condition + @" AND V.IsPark = 0 

                                        AND ACT.IsBalanceSheet = 0  AND ACT.Id IN ('Expense')

                                          GROUP BY EFYP.Id , EFYP.PeriodName,  FYPA.PeriodName, FYPA.EndDate,EFYP.EndDate,FYPA.EntryPeriodId ,ACT.BalanceType
								    ) periodWiseExp

                            GROUP by periodWiseExp.PostingPeriod,periodWiseExp.PostingPeriodId, periodWiseExp.EntryPeriod,periodWiseExp.EntryPeriodId,
                            periodWiseExp.EntryPeriodEndDate, periodWiseExp.PostingPeriodEndDate	
                       ) periodWiseExp
									ON periodWiseExp." + periodId + @" = FiscalYear.Id  
									)";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        /// <summary>
        /// Revenue Budget VS Actual
        /// </summary>
        /// <param name="factDate"></param>
        /// <param name="fromDate"></param>
        /// <param name="toDate"></param>
        /// <param name="companyGroupId"></param>
        /// <param name="companyId"></param>
        /// <returns></returns>
        public IEnumerable<object> PeriodRevenueVSBudgetBarChart(string factDate, string fromDate, string toDate, string companyGroupId, string companyId)
        {
            var cList = string.Empty;
            var cListId = string.Empty;
            var join = string.Empty;
            var wc = string.Empty;
            var cListextG = string.Empty;
            var cListextIdG = string.Empty;
            var daysOfMonth = DateTime.DaysInMonth(Convert.ToDateTime(toDate).Year, Convert.ToDateTime(toDate).Month);
            var periodId = "";
            DateTime calculatedToDate = new DateTime(Convert.ToDateTime(toDate).Year, Convert.ToDateTime(toDate).Month, daysOfMonth);

            DateTime calculatedFromDate = calculatedToDate.AddMonths(-12);
            try
            {
                var expFactDate = string.Empty;
                var condition = string.Empty;

                if (factDate == "postingDate")
                {
                    expFactDate = "Replace(CONVERT(VARCHAR(11),V.PostingDate,6),' ','-') factDate,";
                    condition = "AND V.PostingDate BETWEEN CONVERT(DATE,'" + fromDate + @"') AND CONVERT(DATE,'" + toDate + @"')";
                    periodId = "PostingPeriodId";
                }
                else if (factDate == "AddedDate")
                {
                    expFactDate = "Replace(CONVERT(VARCHAR(11),V.AddedDate,6),' ','-') factDate,";
                    condition = "AND V.AddedDate BETWEEN CONVERT(DATE,'" + fromDate + @"') AND CONVERT(DATE,'" + toDate + @"')";
                    periodId = "EntryPeriodId";

                }
                else
                {
                    condition = "";
                }

                var sql = @"SELECT ISNULL(Amount,0) Amount, ISNULL(BudgetAmount,0) BudgetAmount
									,FiscalYear.PeriodName PostingPeriod,
                                FiscalYear.Id PostingPeriodId				
								 FROM				
									  (
                                       (SELECT * FROM SCS.FiscalYearPeriod WHERE
									   StartDate BETWEEN (SELECT  StartDate FROM SCS.FiscalYearPeriod where '" + calculatedToDate.ToString("dd-MMM-yyyy") + @"' between StartDate and EndDate)
                                              AND(SELECT  EndDate FROM SCS.FiscalYearPeriod where '" + calculatedToDate.ToString("dd-MMM-yyyy") + @"' between StartDate and EndDate)
									  ) FiscalYear 
                                    Left join
                                    (SELECT
                                         SUM(AnnualBudgetDetail.StandardAmount) BudgetAmount
								        , EFYP.PeriodName PostingPeriod,
                                        EFYP.Id PostingPeriodId--, FYPA.PeriodName AS EntryPeriod,FYPA.EntryPeriodId, FYPA.EndDate EntryPeriodEndDate
                                        , EFYP.EndDate PostingPeriodEndDate
										--,BM.id
									     FROM
										 MST.AnnualBudgetDetail AS AnnualBudgetDetail	
										 Left join SCS.FiscalYearPeriod AS EFYP  ON EFYP.Id = AnnualBudgetDetail.FiscalYearPeriodId
										
										LEFT JOIN  MST.AnnualBudget AS AnnualBudget ON AnnualBudget.Id = AnnualBudgetDetail.AnnualBudgetId
                                        Left JOIN MST.BudgetMaster AS BM ON BM.Id = AnnualBudgetDetail.BudgetMasterId


                                        LEFT JOIN HKP.BudgetCategory AS BC ON BC.Id = BM.BudgetCategoryId

                                        LEFT JOIN HKP.BudgetSubCategory AS BSC ON BSC.Id = BM.BudgetSubCategoryId

                                        LEFT JOIN HKP.GLGeneralInfo AS GL ON GL.Id = BM.GLGeneralInfoId

                                        LEFT JOIN HKP.AccountGroup AS AG ON AG.Id = GL.AccountGroupId

                                        LEFT JOIN HKP.AccountType AS ACT ON ACT.Id = AG.AccountTypeId

                                        LEFT JOIN ORG.Entity AS ENT ON ENT.Id = AnnualBudgetDetail.EntityId                       

                                        WHERE AnnualBudgetDetail.CompanyGroupId = '" + companyGroupId + @"'  AND EFYP.StartDate BETWEEN (SELECT  StartDate FROM SCS.FiscalYearPeriod where '" + calculatedToDate.ToString("dd-MMM-yyyy") + @"' between StartDate and EndDate)
                                              AND(SELECT  EndDate FROM SCS.FiscalYearPeriod where '" + calculatedToDate.ToString("dd-MMM-yyyy") + @"' between StartDate and EndDate)

                                        AND ACT.IsBalanceSheet = 0  AND ACT.Id IN ('Revenue')

                                          GROUP BY EFYP.Id , EFYP.PeriodName,EFYP.Id,EFYP.EndDate
										
								    ) periodWiseExpBudget ON FiscalYear.Id = periodWiseExpBudget.PostingPeriodId
									LEFT JOIN
									(SELECT SUM(CASE WHEN ISNULL(DRcumulative,0)=0 THEN periodWiseExp.CRcumulative ELSE periodWiseExp.DRcumulative END) Amount, periodWiseExp.PostingPeriod,
                                periodWiseExp.PostingPeriodId, periodWiseExp.EntryPeriod,periodWiseExp.EntryPeriodId, periodWiseExp.EntryPeriodEndDate
								,periodWiseExp.PostingPeriodEndDate FROM
                                  (SELECT
                                          DRcumulative = CASE WHEN ACT.BalanceType = 'Debit' THEN SUM(VDC.DrAmount)-SUM(VDC.CrAmount) ELSE 0 END
								        , CRcumulative = CASE WHEN ACT.BalanceType = 'Credit' THEN SUM(VDC.crAmount)-SUM(VDC.DrAmount) ELSE 0 END
								        , EFYP.PeriodName PostingPeriod,
                                        EFYP.Id PostingPeriodId, FYPA.PeriodName AS EntryPeriod,FYPA.EntryPeriodId, FYPA.EndDate EntryPeriodEndDate
                                        , EFYP.EndDate PostingPeriodEndDate

                                        FROM TRN.VoucherDetailCurrency AS VDC
                                        LEFT JOIN TRN.VoucherDetail AS EVD ON EVD.Id = VDC.VoucherDetailId
                                        LEFT JOIN TRN.Voucher AS V ON V.Id = EVD.VoucherId

                                        LEFT JOIN SCS.FiscalYearPeriod AS EFYP ON EFYP.Id = EVD.FiscalYearPeriodId

                                        LEFT JOIN MST.BudgetMaster AS BM ON BM.Id = EVD.BudgetMasterId

                                        LEFT JOIN HKP.BudgetCategory AS BC ON BC.Id = BM.BudgetCategoryId

                                        LEFT JOIN HKP.BudgetSubCategory AS BSC ON BSC.Id = BM.BudgetSubCategoryId

                                        LEFT JOIN HKP.Budget AS B ON B.Id = BM.BudgetId

                                        LEFT JOIN HKP.GLGeneralInfo AS GL ON GL.Id = BM.GLGeneralInfoId

                                        LEFT JOIN HKP.AccountGroup AS AG ON AG.Id = GL.AccountGroupId

                                        LEFT JOIN HKP.AccountType AS ACT ON ACT.Id = AG.AccountTypeId

                                        LEFT JOIN ORG.Entity AS ENT ON ENT.Id = V.EntityId

                                        LEFT JOIN(SELECT Id AS EntryPeriodId, PeriodName, StartDate, EndDate FROM SCS.FiscalYearPeriod)

                                                    AS FYPA ON MONTH(CONVERT(DATE, FYPA.EndDate)) = MONTH(CONVERT(DATE, evd.AddedDate))

                                            AND YEAR(CONVERT(DATE, FYPA.EndDate))= YEAR(CONVERT(DATE, evd.AddedDate))

                                        WHERE V.CompanyGroupId = '" + companyGroupId + @"' " + condition + @" AND V.IsPark = 0 

                                        AND ACT.IsBalanceSheet = 0  AND ACT.Id IN ('Revenue')

                                          GROUP BY EFYP.Id , EFYP.PeriodName,  FYPA.PeriodName, FYPA.EndDate,EFYP.EndDate,FYPA.EntryPeriodId ,ACT.BalanceType
								    ) periodWiseExp

                            GROUP by periodWiseExp.PostingPeriod,periodWiseExp.PostingPeriodId, periodWiseExp.EntryPeriod,periodWiseExp.EntryPeriodId,
                            periodWiseExp.EntryPeriodEndDate, periodWiseExp.PostingPeriodEndDate	
                       ) periodWiseExp
									ON periodWiseExp." + periodId + @" = FiscalYear.Id  
									)";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public IEnumerable<object> PeriodDynamicExpenseVSBudgetBarChart(IEnumerable<ChartColumnList> ChartColumnList, int seq, string factDate, string fromDate, string toDate, string companyGroupId, string companyId)
        {
            var cList = string.Empty;
            var cListId = string.Empty;
            var join = string.Empty;
            var wc = string.Empty;
            var cListextG = string.Empty;
            var cListextIdG = string.Empty;
            var daysOfMonth = DateTime.DaysInMonth(Convert.ToDateTime(toDate).Year, Convert.ToDateTime(toDate).Month);
            var periodId = "";
            DateTime calculatedToDate = new DateTime(Convert.ToDateTime(toDate).Year, Convert.ToDateTime(toDate).Month, daysOfMonth);

            DateTime calculatedFromDate = calculatedToDate.AddMonths(-1);
            try
            {
                var expFactDate = string.Empty;
                var condition = string.Empty;


                if (factDate == "postingDate")
                {
                    expFactDate = "Replace(CONVERT(VARCHAR(11),V.PostingDate,6),' ','-') factDate,";
                    condition = "AND V.PostingDate BETWEEN CONVERT(DATE,'" + fromDate + @"') AND CONVERT(DATE,'" + toDate + @"')";
                    periodId = "PostingPeriodId";
                }
                else if (factDate == "AddedDate")
                {
                    expFactDate = "Replace(CONVERT(VARCHAR(11),V.AddedDate,6),' ','-') factDate,";
                    condition = "AND V.AddedDate BETWEEN CONVERT(DATE,'" + fromDate + @"') AND CONVERT(DATE,'" + toDate + @"')";
                    periodId = "EntryPeriodId";
                }
                else
                {
                    condition = "";
                }
                seq += 1;
                foreach (var item in ChartColumnList)
                {
                    if (item.Sequence != -2 && item.Sequence != -1)
                    {
                        if (item.Sequence <= seq)
                        {
                            if (item.RType == "Entity")
                            {
                                cList = "," + item.StandardName + ".UserName";
                                cListId = "," + item.StandardName + ".Id";
                                //cListextG = "," + item.ColumnName + "Name";
                                //cListextIdG = "," + item.ColumnName + "Id";
                                if (item.ColumnName == "EmployeeGroup")
                                {
                                    join += "LEFT JOIN [HKP].[" + item.StandardName + "] ON " + item.StandardName + ".Id = ENT." + item.StandardName + "Id\n";
                                }
                                else
                                {
                                    join += "LEFT JOIN [ORG].[" + item.StandardName + "] ON " + item.StandardName + ".Id = ENT." + item.StandardName + "Id\n";
                                }
                            }
                            else
                            {
                                cList = "," + item.StandardName + ".UserName";
                                join += "LEFT JOIN [ORG].[" + item.StandardName + "] ON " + item.StandardName + ".Id = Po." + item.StandardName + "Id\n";
                            }
                        }
                    }
                    if (item.Sequence != -2)
                    {
                        if (item.Sequence == -1)
                        {
                            wc = "  AND CMP.Id ='" + item.Id + "'";
                        }
                        else
                        {
                            if (item.Sequence < seq)
                            {
                                wc += " and " + item.StandardName + ".Id='" + item.Text + "'";
                            }
                        }
                    }
                }

                var sql = @"SELECT ISNULL(Amount,0) Amount, ISNULL(BudgetAmount,0) BudgetAmount
									,FiscalYear.PeriodName PostingPeriod,
                                FiscalYear.Id PostingPeriodId				
								 FROM				
									  (
                                       (SELECT * FROM SCS.FiscalYearPeriod WHERE
									   StartDate BETWEEN (SELECT  StartDate FROM SCS.FiscalYearPeriod where '" + calculatedToDate.ToString("dd-MMM-yyyy") + @"' between StartDate and EndDate)
                                              AND(SELECT  EndDate FROM SCS.FiscalYearPeriod where '" + calculatedToDate.ToString("dd-MMM-yyyy") + @"' between StartDate and EndDate)
									  ) FiscalYear 
                                    Left join
                                    (SELECT
                                         SUM(AnnualBudgetDetail.StandardAmount) BudgetAmount
								        , EFYP.PeriodName PostingPeriod,
                                        EFYP.Id PostingPeriodId--, FYPA.PeriodName AS EntryPeriod,FYPA.EntryPeriodId, FYPA.EndDate EntryPeriodEndDate
                                        , EFYP.EndDate PostingPeriodEndDate
										     ,    cmp.Id AS CompanyId
								           	" + cList + @" AS ColumnName
									         " + cListId + @" AS UId
									     FROM
										 MST.AnnualBudgetDetail AS AnnualBudgetDetail	
										 Left join SCS.FiscalYearPeriod AS EFYP  ON EFYP.Id = AnnualBudgetDetail.FiscalYearPeriodId
										
										LEFT JOIN  MST.AnnualBudget AS AnnualBudget ON AnnualBudget.Id = AnnualBudgetDetail.AnnualBudgetId
                                        Left JOIN MST.BudgetMaster AS BM ON BM.Id = AnnualBudgetDetail.BudgetMasterId
                                        LEFT JOIN ORG.Company AS CMP ON CMP.Id = AnnualBudgetDetail.CompanyId

                                        LEFT JOIN HKP.BudgetCategory AS BC ON BC.Id = BM.BudgetCategoryId

                                        LEFT JOIN HKP.BudgetSubCategory AS BSC ON BSC.Id = BM.BudgetSubCategoryId

                                        LEFT JOIN HKP.GLGeneralInfo AS GL ON GL.Id = BM.GLGeneralInfoId

                                        LEFT JOIN HKP.AccountGroup AS AG ON AG.Id = GL.AccountGroupId

                                        LEFT JOIN HKP.AccountType AS ACT ON ACT.Id = AG.AccountTypeId

                                        LEFT JOIN ORG.Entity AS ENT ON ENT.Id = AnnualBudgetDetail.EntityId                       
                                       
                                        " + join + @"

                                        WHERE AnnualBudgetDetail.CompanyGroupId = '" + companyGroupId + @"'  AND EFYP.StartDate BETWEEN (SELECT  StartDate FROM SCS.FiscalYearPeriod where '" + calculatedToDate.ToString("dd-MMM-yyyy") + @"' between StartDate and EndDate)
                                              AND(SELECT  EndDate FROM SCS.FiscalYearPeriod where '" + calculatedToDate.ToString("dd-MMM-yyyy") + @"' between StartDate and EndDate)

                                        AND ACT.IsBalanceSheet = 0  AND ACT.Id IN ('Expense')" + wc + @"

                                          GROUP BY EFYP.Id , EFYP.PeriodName,EFYP.Id,EFYP.EndDate " + cList + @"  " + cListId + @", cmp.Id
										
								    ) periodWiseExpBudget ON FiscalYear.Id = periodWiseExpBudget.PostingPeriodId
									LEFT JOIN
									(SELECT SUM(CASE WHEN ISNULL(DRcumulative,0)=0 THEN periodWiseExp.CRcumulative ELSE periodWiseExp.DRcumulative END) Amount, periodWiseExp.PostingPeriod,
                                periodWiseExp.PostingPeriodId, periodWiseExp.EntryPeriod,periodWiseExp.EntryPeriodId, periodWiseExp.EntryPeriodEndDate
								,periodWiseExp.PostingPeriodEndDate FROM
                                  (SELECT
                                          DRcumulative = CASE WHEN ACT.BalanceType = 'Debit' THEN SUM(VDC.DrAmount)-SUM(VDC.CrAmount) ELSE 0 END
								        , CRcumulative = CASE WHEN ACT.BalanceType = 'Credit' THEN SUM(VDC.crAmount)-SUM(VDC.DrAmount) ELSE 0 END
								        , EFYP.PeriodName PostingPeriod,
                                        EFYP.Id PostingPeriodId, FYPA.PeriodName AS EntryPeriod,FYPA.EntryPeriodId, FYPA.EndDate EntryPeriodEndDate
                                        , EFYP.EndDate PostingPeriodEndDate
                                         , cmp.Id AS CompanyId

                                               " + cList + @" AS ColumnName

                                             " + cListId + @" AS UId

                                        FROM TRN.VoucherDetailCurrency AS VDC
                                        LEFT JOIN TRN.VoucherDetail AS EVD ON EVD.Id = VDC.VoucherDetailId
                                        LEFT JOIN TRN.Voucher AS V ON V.Id = EVD.VoucherId
                                        LEFT JOIN ORG.Company AS CMP ON CMP.Id = V.CompanyId
                                        LEFT JOIN SCS.FiscalYearPeriod AS EFYP ON EFYP.Id = EVD.FiscalYearPeriodId

                                        LEFT JOIN MST.BudgetMaster AS BM ON BM.Id = EVD.BudgetMasterId

                                        LEFT JOIN HKP.BudgetCategory AS BC ON BC.Id = BM.BudgetCategoryId

                                        LEFT JOIN HKP.BudgetSubCategory AS BSC ON BSC.Id = BM.BudgetSubCategoryId

                                        LEFT JOIN HKP.Budget AS B ON B.Id = BM.BudgetId

                                        LEFT JOIN HKP.GLGeneralInfo AS GL ON GL.Id = BM.GLGeneralInfoId

                                        LEFT JOIN HKP.AccountGroup AS AG ON AG.Id = GL.AccountGroupId

                                        LEFT JOIN HKP.AccountType AS ACT ON ACT.Id = AG.AccountTypeId

                                        LEFT JOIN ORG.Entity AS ENT ON ENT.Id = V.EntityId
                                            
                                        " + join + @"                                                                                                    

                                        LEFT JOIN(SELECT Id AS EntryPeriodId, PeriodName, StartDate, EndDate FROM SCS.FiscalYearPeriod)

                                                    AS FYPA ON MONTH(CONVERT(DATE, FYPA.EndDate)) = MONTH(CONVERT(DATE, evd.AddedDate))

                                            AND YEAR(CONVERT(DATE, FYPA.EndDate))= YEAR(CONVERT(DATE, evd.AddedDate))

                                        WHERE V.CompanyGroupId = '" + companyGroupId + @"' " + condition + @" AND V.IsPark = 0 

                                        AND ACT.IsBalanceSheet = 0  AND ACT.Id IN ('Expense')

                                    " + wc + @"

                                          GROUP BY EFYP.Id , EFYP.PeriodName,  FYPA.PeriodName, FYPA.EndDate,EFYP.EndDate,FYPA.EntryPeriodId ,ACT.BalanceType
                                            " + cList + @"  " + cListId + @", cmp.Id
			                    ) periodWiseExp

                            GROUP by periodWiseExp.PostingPeriod,periodWiseExp.PostingPeriodId, periodWiseExp.EntryPeriod,periodWiseExp.EntryPeriodId,
                            periodWiseExp.EntryPeriodEndDate, periodWiseExp.PostingPeriodEndDate

                       ) periodWiseExp
									ON periodWiseExp." + periodId + @" = FiscalYear.Id  
									)";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public IEnumerable<object> PeriodDynamicRevenueVSBudgetBarChart(IEnumerable<ChartColumnList> ChartColumnList, int seq, string factDate, string fromDate, string toDate, string companyGroupId, string companyId)
        {
            var cList = string.Empty;
            var cListId = string.Empty;
            var join = string.Empty;
            var wc = string.Empty;
            var cListextG = string.Empty;
            var cListextIdG = string.Empty;
            var daysOfMonth = DateTime.DaysInMonth(Convert.ToDateTime(toDate).Year, Convert.ToDateTime(toDate).Month);
            var periodId = "";
            DateTime calculatedToDate = new DateTime(Convert.ToDateTime(toDate).Year, Convert.ToDateTime(toDate).Month, daysOfMonth);

            DateTime calculatedFromDate = calculatedToDate.AddMonths(-1);
            try
            {
                var expFactDate = string.Empty;
                var condition = string.Empty;

                if (factDate == "postingDate")
                {
                    expFactDate = "Replace(CONVERT(VARCHAR(11),V.PostingDate,6),' ','-') factDate,";
                    condition = "AND V.PostingDate BETWEEN CONVERT(DATE,'" + fromDate + @"') AND CONVERT(DATE,'" + toDate + @"')";
                    periodId = "PostingPeriodId";
                }
                else if (factDate == "AddedDate")
                {
                    expFactDate = "Replace(CONVERT(VARCHAR(11),V.AddedDate,6),' ','-') factDate,";
                    condition = "AND V.AddedDate BETWEEN CONVERT(DATE,'" + fromDate + @"') AND CONVERT(DATE,'" + toDate + @"')";
                    periodId = "EntryPeriodId";

                }
                else
                {
                    condition = "";
                }
                seq += 1;
                foreach (var item in ChartColumnList)
                {
                    if (item.Sequence != -2 && item.Sequence != -1)
                    {
                        if (item.Sequence <= seq)
                        {
                            if (item.RType == "Entity")
                            {
                                cList = "," + item.StandardName + ".UserName";
                                cListId = "," + item.StandardName + ".Id";
                                //cListextG = "," + item.ColumnName + "Name";
                                //cListextIdG = "," + item.ColumnName + "Id";
                                if (item.ColumnName == "EmployeeGroup")
                                {
                                    join += "LEFT JOIN [HKP].[" + item.StandardName + "] ON " + item.StandardName + ".Id = ENT." + item.StandardName + "Id\n";
                                }
                                else
                                {
                                    join += "LEFT JOIN [ORG].[" + item.StandardName + "] ON " + item.StandardName + ".Id = ENT." + item.StandardName + "Id\n";
                                }
                            }
                            else
                            {
                                cList = "," + item.StandardName + ".UserName";
                                join += "LEFT JOIN [ORG].[" + item.StandardName + "] ON " + item.StandardName + ".Id = Po." + item.StandardName + "Id\n";
                            }
                        }
                    }
                    if (item.Sequence != -2)
                    {
                        if (item.Sequence == -1)
                        {
                            wc = "  AND CMP.Id ='" + item.Id + "'";
                        }
                        else
                        {
                            if (item.Sequence < seq)
                            {
                                wc += " and " + item.StandardName + ".Id='" + item.Text + "'";
                            }
                        }
                    }
                }

                var sql = @"SELECT ISNULL(Amount,0) Amount, ISNULL(BudgetAmount,0) BudgetAmount
									,FiscalYear.PeriodName PostingPeriod,
                                FiscalYear.Id PostingPeriodId				
								 FROM				
									  (
                                       (SELECT * FROM SCS.FiscalYearPeriod WHERE
									   StartDate BETWEEN (SELECT  StartDate FROM SCS.FiscalYearPeriod where '" + calculatedToDate.ToString("dd-MMM-yyyy") + @"' between StartDate and EndDate)
                                              AND(SELECT  EndDate FROM SCS.FiscalYearPeriod where '" + calculatedToDate.ToString("dd-MMM-yyyy") + @"' between StartDate and EndDate)
									  ) FiscalYear 
                                    Left join
                                    (SELECT
                                         SUM(AnnualBudgetDetail.StandardAmount) BudgetAmount
								        , EFYP.PeriodName PostingPeriod,
                                        EFYP.Id PostingPeriodId--, FYPA.PeriodName AS EntryPeriod,FYPA.EntryPeriodId, FYPA.EndDate EntryPeriodEndDate
                                        , EFYP.EndDate PostingPeriodEndDate
										     ,    cmp.Id AS CompanyId
								           	" + cList + @" AS ColumnName
									         " + cListId + @" AS UId
									     FROM
										 MST.AnnualBudgetDetail AS AnnualBudgetDetail	
										 Left join SCS.FiscalYearPeriod AS EFYP  ON EFYP.Id = AnnualBudgetDetail.FiscalYearPeriodId
										
										LEFT JOIN  MST.AnnualBudget AS AnnualBudget ON AnnualBudget.Id = AnnualBudgetDetail.AnnualBudgetId
                                        Left JOIN MST.BudgetMaster AS BM ON BM.Id = AnnualBudgetDetail.BudgetMasterId
                                        LEFT JOIN ORG.Company AS CMP ON CMP.Id = AnnualBudgetDetail.CompanyId


                                        LEFT JOIN HKP.BudgetCategory AS BC ON BC.Id = BM.BudgetCategoryId

                                        LEFT JOIN HKP.BudgetSubCategory AS BSC ON BSC.Id = BM.BudgetSubCategoryId

                                        LEFT JOIN HKP.GLGeneralInfo AS GL ON GL.Id = BM.GLGeneralInfoId

                                        LEFT JOIN HKP.AccountGroup AS AG ON AG.Id = GL.AccountGroupId

                                        LEFT JOIN HKP.AccountType AS ACT ON ACT.Id = AG.AccountTypeId

                                        LEFT JOIN ORG.Entity AS ENT ON ENT.Id = AnnualBudgetDetail.EntityId                       
                                       
                                        " + join + @"

                                        WHERE AnnualBudgetDetail.CompanyGroupId = '" + companyGroupId + @"'  AND EFYP.StartDate BETWEEN (SELECT  StartDate FROM SCS.FiscalYearPeriod where '" + calculatedToDate.ToString("dd-MMM-yyyy") + @"' between StartDate and EndDate)
                                              AND(SELECT  EndDate FROM SCS.FiscalYearPeriod where '" + calculatedToDate.ToString("dd-MMM-yyyy") + @"' between StartDate and EndDate)

                                        AND ACT.IsBalanceSheet = 0  AND ACT.Id IN ('Revenue')" + wc + @"

                                          GROUP BY EFYP.Id , EFYP.PeriodName,EFYP.Id,EFYP.EndDate " + cList + @"  " + cListId + @", cmp.Id
										
								    ) periodWiseExpBudget ON FiscalYear.Id = periodWiseExpBudget.PostingPeriodId
									LEFT JOIN
									(SELECT SUM(CASE WHEN ISNULL(DRcumulative,0)=0 THEN periodWiseExp.CRcumulative ELSE periodWiseExp.DRcumulative END) Amount, periodWiseExp.PostingPeriod,
                                periodWiseExp.PostingPeriodId, periodWiseExp.EntryPeriod,periodWiseExp.EntryPeriodId, periodWiseExp.EntryPeriodEndDate
								,periodWiseExp.PostingPeriodEndDate FROM
                                  (SELECT
                                          DRcumulative = CASE WHEN ACT.BalanceType = 'Debit' THEN SUM(VDC.DrAmount)-SUM(VDC.CrAmount) ELSE 0 END
								        , CRcumulative = CASE WHEN ACT.BalanceType = 'Credit' THEN SUM(VDC.crAmount)-SUM(VDC.DrAmount) ELSE 0 END
								        , EFYP.PeriodName PostingPeriod,
                                        EFYP.Id PostingPeriodId, FYPA.PeriodName AS EntryPeriod,FYPA.EntryPeriodId, FYPA.EndDate EntryPeriodEndDate
                                        , EFYP.EndDate PostingPeriodEndDate
                                         , cmp.Id AS CompanyId

                                               " + cList + @" AS ColumnName

                                             " + cListId + @" AS UId

                                        FROM TRN.VoucherDetailCurrency AS VDC
                                        LEFT JOIN TRN.VoucherDetail AS EVD ON EVD.Id = VDC.VoucherDetailId
                                        LEFT JOIN TRN.Voucher AS V ON V.Id = EVD.VoucherId
                                        LEFT JOIN ORG.Company AS CMP ON CMP.Id = V.CompanyId

                                        LEFT JOIN SCS.FiscalYearPeriod AS EFYP ON EFYP.Id = EVD.FiscalYearPeriodId

                                        LEFT JOIN MST.BudgetMaster AS BM ON BM.Id = EVD.BudgetMasterId

                                        LEFT JOIN HKP.BudgetCategory AS BC ON BC.Id = BM.BudgetCategoryId

                                        LEFT JOIN HKP.BudgetSubCategory AS BSC ON BSC.Id = BM.BudgetSubCategoryId

                                        LEFT JOIN HKP.Budget AS B ON B.Id = BM.BudgetId

                                        LEFT JOIN HKP.GLGeneralInfo AS GL ON GL.Id = BM.GLGeneralInfoId

                                        LEFT JOIN HKP.AccountGroup AS AG ON AG.Id = GL.AccountGroupId

                                        LEFT JOIN HKP.AccountType AS ACT ON ACT.Id = AG.AccountTypeId

                                        LEFT JOIN ORG.Entity AS ENT ON ENT.Id = V.EntityId
                                            
                                        " + join + @"                                                                                                    

                                        LEFT JOIN(SELECT Id AS EntryPeriodId, PeriodName, StartDate, EndDate FROM SCS.FiscalYearPeriod)

                                                    AS FYPA ON MONTH(CONVERT(DATE, FYPA.EndDate)) = MONTH(CONVERT(DATE, evd.AddedDate))

                                            AND YEAR(CONVERT(DATE, FYPA.EndDate))= YEAR(CONVERT(DATE, evd.AddedDate))

                                        WHERE V.CompanyGroupId = '" + companyGroupId + @"' " + condition + @" AND V.IsPark = 0 

                                        AND ACT.IsBalanceSheet = 0  AND ACT.Id IN ('Revenue')

                                    " + wc + @"

                                          GROUP BY EFYP.Id , EFYP.PeriodName,  FYPA.PeriodName, FYPA.EndDate,EFYP.EndDate,FYPA.EntryPeriodId ,ACT.BalanceType
                                            " + cList + @"  " + cListId + @", cmp.Id
			                    ) periodWiseExp

                            GROUP by periodWiseExp.PostingPeriod,periodWiseExp.PostingPeriodId, periodWiseExp.EntryPeriod,periodWiseExp.EntryPeriodId,
                            periodWiseExp.EntryPeriodEndDate, periodWiseExp.PostingPeriodEndDate

                       ) periodWiseExp
									ON periodWiseExp." + periodId + @" = FiscalYear.Id  
									)";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }
        #endregion

        #region Period Wise Expense Bar Chart

        public IEnumerable<object> PeriodWiseExpenseBarChart(string factDate, string fromDate, string toDate, string companyGroupId, string companyId)
        {
            var cList = string.Empty;
            var cListId = string.Empty;
            var join = string.Empty;
            var wc = string.Empty;
            var cListextG = string.Empty;
            var cListextIdG = string.Empty;
            var daysOfMonth = DateTime.DaysInMonth(Convert.ToDateTime(toDate).Year, Convert.ToDateTime(toDate).Month);

            DateTime calculatedToDate = new DateTime(Convert.ToDateTime(toDate).Year, Convert.ToDateTime(toDate).Month, daysOfMonth);
            var periodId = "";
            DateTime calculatedFromDate = calculatedToDate.AddMonths(-12);
            try
            {
                var expFactDate = string.Empty;
                var condition = string.Empty;

                if (factDate == "postingDate")
                {
                    expFactDate = "Replace(CONVERT(VARCHAR(11),V.PostingDate,6),' ','-') factDate,";
                    condition = "AND V.PostingDate BETWEEN CONVERT(DATE,'" + calculatedFromDate.ToString("dd-MMM-yyyy") + @"') AND CONVERT(DATE,'" + calculatedToDate.ToString("dd-MMM-yyyy") + @"')";
                    periodId = "PostingPeriodId";
                }
                else if (factDate == "AddedDate")
                {
                    expFactDate = "Replace(CONVERT(VARCHAR(11),V.AddedDate,6),' ','-') factDate,";
                    condition = "AND V.AddedDate BETWEEN CONVERT(DATE,'" + calculatedFromDate.ToString("dd-MMM-yyyy") + @"') AND CONVERT(DATE,'" + calculatedToDate.ToString("dd-MMM-yyyy") + @"')";
                    periodId = "EntryPeriodId";

                }
                else
                {
                    condition = "";
                }

                var sql = @"SELECT ISNULL(Amount,0) Amount, ISNULL(BudgetAmount,0) BudgetAmount
									,FiscalYear.PeriodName PostingPeriod,
                                FiscalYear.Id PostingPeriodId				
								 FROM				
									  (
                                       (SELECT * FROM SCS.FiscalYearPeriod WHERE
									   StartDate BETWEEN (SELECT  StartDate FROM SCS.FiscalYearPeriod where '" + calculatedFromDate.ToString("dd-MMM-yyyy") + @"' between StartDate and EndDate)
                                              AND(SELECT  EndDate FROM SCS.FiscalYearPeriod where '" + calculatedToDate.ToString("dd-MMM-yyyy") + @"' between StartDate and EndDate)
									  ) FiscalYear 
                                    Left join
                                    (SELECT
                                         SUM(AnnualBudgetDetail.StandardAmount) BudgetAmount
								        , EFYP.PeriodName PostingPeriod,
                                        EFYP.Id PostingPeriodId--, FYPA.PeriodName AS EntryPeriod,FYPA.EntryPeriodId, FYPA.EndDate EntryPeriodEndDate
                                        , EFYP.EndDate PostingPeriodEndDate
										--,BM.id
									     FROM
										 MST.AnnualBudgetDetail AS AnnualBudgetDetail	
										 Left join SCS.FiscalYearPeriod AS EFYP  ON EFYP.Id = AnnualBudgetDetail.FiscalYearPeriodId
										
										LEFT JOIN  MST.AnnualBudget AS AnnualBudget ON AnnualBudget.Id = AnnualBudgetDetail.AnnualBudgetId
                                        Left JOIN MST.BudgetMaster AS BM ON BM.Id = AnnualBudgetDetail.BudgetMasterId


                                        LEFT JOIN HKP.BudgetCategory AS BC ON BC.Id = BM.BudgetCategoryId

                                        LEFT JOIN HKP.BudgetSubCategory AS BSC ON BSC.Id = BM.BudgetSubCategoryId

                                        LEFT JOIN HKP.GLGeneralInfo AS GL ON GL.Id = BM.GLGeneralInfoId

                                        LEFT JOIN HKP.AccountGroup AS AG ON AG.Id = GL.AccountGroupId

                                        LEFT JOIN HKP.AccountType AS ACT ON ACT.Id = AG.AccountTypeId

                                        LEFT JOIN ORG.Entity AS ENT ON ENT.Id = AnnualBudgetDetail.EntityId                       

                                        WHERE AnnualBudgetDetail.CompanyGroupId = '" + companyGroupId + @"'  AND EFYP.StartDate BETWEEN (SELECT  StartDate FROM SCS.FiscalYearPeriod where '" + calculatedFromDate.ToString("dd-MMM-yyyy") + @"' between StartDate and EndDate)
                                              AND(SELECT  EndDate FROM SCS.FiscalYearPeriod where '" + calculatedToDate.ToString("dd-MMM-yyyy") + @"' between StartDate and EndDate)

                                        AND ACT.IsBalanceSheet = 0  AND ACT.Id IN ('Expense')

                                          GROUP BY EFYP.Id , EFYP.PeriodName,EFYP.Id,EFYP.EndDate
										
								    ) periodWiseExpBudget ON FiscalYear.Id = periodWiseExpBudget.PostingPeriodId
									LEFT JOIN
									(SELECT SUM(CASE WHEN ISNULL(DRcumulative,0)=0 THEN periodWiseExp.CRcumulative ELSE periodWiseExp.DRcumulative END) Amount, periodWiseExp.PostingPeriod,
                                periodWiseExp.PostingPeriodId, periodWiseExp.EntryPeriod,periodWiseExp.EntryPeriodId, periodWiseExp.EntryPeriodEndDate
								,periodWiseExp.PostingPeriodEndDate FROM
                                  (SELECT
                                          DRcumulative = CASE WHEN ACT.BalanceType = 'Debit' THEN SUM(VDC.DrAmount)-SUM(VDC.CrAmount) ELSE 0 END
								        , CRcumulative = CASE WHEN ACT.BalanceType = 'Credit' THEN SUM(VDC.crAmount)-SUM(VDC.DrAmount) ELSE 0 END
								        , EFYP.PeriodName PostingPeriod,
                                        EFYP.Id PostingPeriodId, FYPA.PeriodName AS EntryPeriod,FYPA.EntryPeriodId, FYPA.EndDate EntryPeriodEndDate
                                        , EFYP.EndDate PostingPeriodEndDate

                                        FROM TRN.VoucherDetailCurrency AS VDC
                                        LEFT JOIN TRN.VoucherDetail AS EVD ON EVD.Id = VDC.VoucherDetailId
                                        LEFT JOIN TRN.Voucher AS V ON V.Id = EVD.VoucherId

                                        LEFT JOIN SCS.FiscalYearPeriod AS EFYP ON EFYP.Id = EVD.FiscalYearPeriodId

                                        LEFT JOIN MST.BudgetMaster AS BM ON BM.Id = EVD.BudgetMasterId

                                        LEFT JOIN HKP.BudgetCategory AS BC ON BC.Id = BM.BudgetCategoryId

                                        LEFT JOIN HKP.BudgetSubCategory AS BSC ON BSC.Id = BM.BudgetSubCategoryId

                                        LEFT JOIN HKP.Budget AS B ON B.Id = BM.BudgetId

                                        LEFT JOIN HKP.GLGeneralInfo AS GL ON GL.Id = BM.GLGeneralInfoId

                                        LEFT JOIN HKP.AccountGroup AS AG ON AG.Id = GL.AccountGroupId

                                        LEFT JOIN HKP.AccountType AS ACT ON ACT.Id = AG.AccountTypeId

                                        LEFT JOIN ORG.Entity AS ENT ON ENT.Id = V.EntityId

                                        LEFT JOIN(SELECT Id AS EntryPeriodId, PeriodName, StartDate, EndDate FROM SCS.FiscalYearPeriod)

                                                    AS FYPA ON MONTH(CONVERT(DATE, FYPA.EndDate)) = MONTH(CONVERT(DATE, evd.AddedDate))

                                            AND YEAR(CONVERT(DATE, FYPA.EndDate))= YEAR(CONVERT(DATE, evd.AddedDate))

                                        WHERE V.CompanyGroupId = '" + companyGroupId + @"'  AND V.PostingDate BETWEEN (SELECT  StartDate FROM SCS.FiscalYearPeriod where '" + calculatedFromDate.ToString("dd-MMM-yyyy") + @"' between StartDate and EndDate)
                                              AND(SELECT  EndDate FROM SCS.FiscalYearPeriod where '" + calculatedToDate.ToString("dd-MMM-yyyy") + @"' between StartDate and EndDate) AND V.IsPark = 0 

                                        AND ACT.IsBalanceSheet = 0  AND ACT.Id IN ('Expense')

                                          GROUP BY EFYP.Id , EFYP.PeriodName,  FYPA.PeriodName, FYPA.EndDate,EFYP.EndDate,FYPA.EntryPeriodId ,ACT.BalanceType
								    ) periodWiseExp

                            GROUP by periodWiseExp.PostingPeriod,periodWiseExp.PostingPeriodId, periodWiseExp.EntryPeriod,periodWiseExp.EntryPeriodId,
                            periodWiseExp.EntryPeriodEndDate, periodWiseExp.PostingPeriodEndDate	
                       ) periodWiseExp
									ON periodWiseExp." + periodId + @" = FiscalYear.Id  
									)";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public IEnumerable<object> PeriodWiseRevenueBarChart(string factDate, string fromDate, string toDate, string companyGroupId, string companyId)
        {
            var cList = string.Empty;
            var cListId = string.Empty;
            var join = string.Empty;
            var wc = string.Empty;
            var cListextG = string.Empty;
            var cListextIdG = string.Empty;
            var daysOfMonth = DateTime.DaysInMonth(Convert.ToDateTime(toDate).Year, Convert.ToDateTime(toDate).Month);

            DateTime calculatedToDate = new DateTime(Convert.ToDateTime(toDate).Year, Convert.ToDateTime(toDate).Month, daysOfMonth);

            DateTime calculatedFromDate = calculatedToDate.AddMonths(-12);
            try
            {
                var expFactDate = string.Empty;
                var condition = string.Empty;
                var periodId = "";
                if (factDate == "postingDate")
                {
                    expFactDate = "Replace(CONVERT(VARCHAR(11),V.PostingDate,6),' ','-') factDate,";
                    condition = "AND V.PostingDate BETWEEN CONVERT(DATE,'" + calculatedFromDate.ToString("dd-MMM-yyyy") + @"') AND CONVERT(DATE,'" + calculatedToDate.ToString("dd-MMM-yyyy") + @"')";
                    periodId = "PostingPeriodId";
                }
                else if (factDate == "AddedDate")
                {
                    expFactDate = "Replace(CONVERT(VARCHAR(11),V.AddedDate,6),' ','-') factDate,";
                    condition = "AND V.AddedDate BETWEEN CONVERT(DATE,'" + calculatedFromDate.ToString("dd-MMM-yyyy") + @"') AND CONVERT(DATE,'" + calculatedToDate.ToString("dd-MMM-yyyy") + @"')";
                    periodId = "EntryPeriodId";
                }
                else
                {
                    condition = "";
                }

                var sql = @"SELECT ISNULL(Amount,0) Amount, ISNULL(BudgetAmount,0) BudgetAmount
									,FiscalYear.PeriodName PostingPeriod,
                                FiscalYear.Id PostingPeriodId				
								 FROM				
									  (
                                     (SELECT * FROM SCS.FiscalYearPeriod WHERE
									   StartDate BETWEEN (SELECT  StartDate FROM SCS.FiscalYearPeriod where '" + calculatedFromDate.ToString("dd-MMM-yyyy") + @"' between StartDate and EndDate)
                                              AND(SELECT  EndDate FROM SCS.FiscalYearPeriod where '" + calculatedToDate.ToString("dd-MMM-yyyy") + @"' between StartDate and EndDate)
									  ) FiscalYear
									  Left join 
                                    (SELECT
                                         SUM(AnnualBudgetDetail.StandardAmount) BudgetAmount
								        , EFYP.PeriodName PostingPeriod,
                                        EFYP.Id PostingPeriodId--, FYPA.PeriodName AS EntryPeriod,FYPA.EntryPeriodId, FYPA.EndDate EntryPeriodEndDate
                                        , EFYP.EndDate PostingPeriodEndDate
										--,BM.id
									     FROM
										 MST.AnnualBudgetDetail AS AnnualBudgetDetail	
										 Left join SCS.FiscalYearPeriod AS EFYP  ON EFYP.Id = AnnualBudgetDetail.FiscalYearPeriodId
										
										LEFT JOIN  MST.AnnualBudget AS AnnualBudget ON AnnualBudget.Id = AnnualBudgetDetail.AnnualBudgetId
                                        Left JOIN MST.BudgetMaster AS BM ON BM.Id = AnnualBudgetDetail.BudgetMasterId


                                        LEFT JOIN HKP.BudgetCategory AS BC ON BC.Id = BM.BudgetCategoryId

                                        LEFT JOIN HKP.BudgetSubCategory AS BSC ON BSC.Id = BM.BudgetSubCategoryId

                                        LEFT JOIN HKP.GLGeneralInfo AS GL ON GL.Id = BM.GLGeneralInfoId

                                        LEFT JOIN HKP.AccountGroup AS AG ON AG.Id = GL.AccountGroupId

                                        LEFT JOIN HKP.AccountType AS ACT ON ACT.Id = AG.AccountTypeId

                                        LEFT JOIN ORG.Entity AS ENT ON ENT.Id = AnnualBudgetDetail.EntityId                       

                                        WHERE AnnualBudgetDetail.CompanyGroupId = '" + companyGroupId + @"'  AND EFYP.StartDate BETWEEN (SELECT  StartDate FROM SCS.FiscalYearPeriod where '" + calculatedFromDate.ToString("dd-MMM-yyyy") + @"' between StartDate and EndDate)
                                              AND(SELECT  EndDate FROM SCS.FiscalYearPeriod where '" + calculatedToDate.ToString("dd-MMM-yyyy") + @"' between StartDate and EndDate)

                                        AND ACT.IsBalanceSheet = 0  AND ACT.Id IN ('Revenue')

                                          GROUP BY EFYP.Id , EFYP.PeriodName,EFYP.Id,EFYP.EndDate
										
								    ) periodWiseExpBudget  ON FiscalYear.Id = periodWiseExpBudget.PostingPeriodId
									LEFT JOIN
									(SELECT SUM(CASE WHEN ISNULL(DRcumulative,0)=0 THEN periodWiseExp.CRcumulative ELSE periodWiseExp.DRcumulative END) Amount, periodWiseExp.PostingPeriod,
                                periodWiseExp.PostingPeriodId, periodWiseExp.EntryPeriod,periodWiseExp.EntryPeriodId, periodWiseExp.EntryPeriodEndDate
								,periodWiseExp.PostingPeriodEndDate FROM
                                  (SELECT
                                          DRcumulative = CASE WHEN ACT.BalanceType = 'Debit' THEN SUM(VDC.DrAmount)-SUM(VDC.CrAmount) ELSE 0 END
								        , CRcumulative = CASE WHEN ACT.BalanceType = 'Credit' THEN SUM(VDC.crAmount)-SUM(VDC.DrAmount) ELSE 0 END
								        , EFYP.PeriodName PostingPeriod,
                                        EFYP.Id PostingPeriodId, FYPA.PeriodName AS EntryPeriod,FYPA.EntryPeriodId, FYPA.EndDate EntryPeriodEndDate
                                        , EFYP.EndDate PostingPeriodEndDate

                                        FROM TRN.VoucherDetailCurrency AS VDC
                                        LEFT JOIN TRN.VoucherDetail AS EVD ON EVD.Id = VDC.VoucherDetailId
                                        LEFT JOIN TRN.Voucher AS V ON V.Id = EVD.VoucherId

                                        LEFT JOIN SCS.FiscalYearPeriod AS EFYP ON EFYP.Id = EVD.FiscalYearPeriodId

                                        LEFT JOIN MST.BudgetMaster AS BM ON BM.Id = EVD.BudgetMasterId

                                        LEFT JOIN HKP.BudgetCategory AS BC ON BC.Id = BM.BudgetCategoryId

                                        LEFT JOIN HKP.BudgetSubCategory AS BSC ON BSC.Id = BM.BudgetSubCategoryId

                                        LEFT JOIN HKP.Budget AS B ON B.Id = BM.BudgetId

                                        LEFT JOIN HKP.GLGeneralInfo AS GL ON GL.Id = BM.GLGeneralInfoId

                                        LEFT JOIN HKP.AccountGroup AS AG ON AG.Id = GL.AccountGroupId

                                        LEFT JOIN HKP.AccountType AS ACT ON ACT.Id = AG.AccountTypeId

                                        LEFT JOIN ORG.Entity AS ENT ON ENT.Id = V.EntityId

                                        LEFT JOIN(SELECT Id AS EntryPeriodId, PeriodName, StartDate, EndDate FROM SCS.FiscalYearPeriod)

                                                    AS FYPA ON MONTH(CONVERT(DATE, FYPA.EndDate)) = MONTH(CONVERT(DATE, evd.AddedDate))

                                            AND YEAR(CONVERT(DATE, FYPA.EndDate))= YEAR(CONVERT(DATE, evd.AddedDate))

                                        WHERE V.CompanyGroupId = '" + companyGroupId + @"'  AND V.PostingDate BETWEEN (SELECT  StartDate FROM SCS.FiscalYearPeriod where '" + calculatedFromDate.ToString("dd-MMM-yyyy") + @"' between StartDate and EndDate)
                                              AND(SELECT  EndDate FROM SCS.FiscalYearPeriod where '" + calculatedToDate.ToString("dd-MMM-yyyy") + @"' between StartDate and EndDate) AND V.IsPark = 0 

                                        AND ACT.IsBalanceSheet = 0  AND ACT.Id IN ('Revenue')

                                          GROUP BY EFYP.Id , EFYP.PeriodName,  FYPA.PeriodName, FYPA.EndDate,EFYP.EndDate,FYPA.EntryPeriodId ,ACT.BalanceType
								    ) periodWiseExp

                            GROUP by periodWiseExp.PostingPeriod,periodWiseExp.PostingPeriodId, periodWiseExp.EntryPeriod,periodWiseExp.EntryPeriodId,
                            periodWiseExp.EntryPeriodEndDate, periodWiseExp.PostingPeriodEndDate
                       ) periodWiseExp
									ON periodWiseExp." + periodId + @" = FiscalYear.Id )";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public IEnumerable<object> DynamicPeriodWiseRevenueBarChart(IEnumerable<ChartColumnList> ChartColumnList, int seq, string factDate, string fromDate, string toDate, string companyGroupId, string companyId)
        {
            var cList = string.Empty;
            var cListId = string.Empty;
            var join = string.Empty;
            var wc = string.Empty;
            var cListextG = string.Empty;
            var cListextIdG = string.Empty;
            //string expFactDate = string.Empty;
            try
            {
                var expFactDate = string.Empty;
                var condition = string.Empty;
                var grpFactDate = string.Empty;
                var daysOfMonth = DateTime.DaysInMonth(Convert.ToDateTime(toDate).Year, Convert.ToDateTime(toDate).Month);

                DateTime calculatedToDate = new DateTime(Convert.ToDateTime(toDate).Year, Convert.ToDateTime(toDate).Month, daysOfMonth);

                DateTime calculatedFromDate = calculatedToDate.AddMonths(-12);
                var periodId = "";

                if (factDate == "postingDate")
                {
                    expFactDate = "Replace(CONVERT(VARCHAR(11),V.PostingDate,6),' ','-') factDate,";
                    condition = "AND V.PostingDate BETWEEN CONVERT(DATE,'" + fromDate + @"') AND CONVERT(DATE,'" + toDate + @"')";
                    grpFactDate = "V.PostingDate";
                    periodId = "PostingPeriodId";
                }
                else if (factDate == "AddedDate")
                {
                    expFactDate = "Replace(CONVERT(VARCHAR(11),V.AddedDate,6),' ','-') factDate,";
                    condition = "AND V.AddedDate BETWEEN CONVERT(DATE,'" + fromDate + @"') AND CONVERT(DATE,'" + toDate + @"')";
                    grpFactDate = "V.AddedDate";
                    periodId = "EntryPeriodId";

                }
                seq += 1;
                foreach (var item in ChartColumnList)
                {
                    if (item.Sequence != -2 && item.Sequence != -1)
                    {
                        if (item.Sequence <= seq)
                        {
                            if (item.RType == "Entity")
                            {
                                cList = "," + item.StandardName + ".UserName";
                                cListId = "," + item.StandardName + ".Id";
                                //cListextG = "," + item.ColumnName + "Name";
                                //cListextIdG = "," + item.ColumnName + "Id";
                                if (item.ColumnName == "EmployeeGroup")
                                {
                                    join += "LEFT JOIN [HKP].[" + item.StandardName + "] ON " + item.StandardName + ".Id = ENT." + item.StandardName + "Id\n";
                                }
                                else
                                {
                                    join += "LEFT JOIN [ORG].[" + item.StandardName + "] ON " + item.StandardName + ".Id = ENT." + item.StandardName + "Id\n";
                                }
                            }
                            else
                            {
                                cList = "," + item.StandardName + ".UserName";
                                join += "LEFT JOIN [ORG].[" + item.StandardName + "] ON " + item.StandardName + ".Id = Po." + item.StandardName + "Id\n";
                            }
                        }
                    }
                    if (item.Sequence != -2)
                    {
                        if (item.Sequence == -1)
                        {
                            wc = "  AND CMP.Id ='" + item.Id + "'";
                        }
                        else
                        {
                            if (item.Sequence < seq)
                            {
                                wc += " and " + item.StandardName + ".Id='" + item.Text + "'";
                            }
                        }
                    }
                }
                var sql = @"SELECT ISNULL(Amount,0) Amount, ISNULL(BudgetAmount,0) BudgetAmount
								,FiscalYear.PeriodName PostingPeriod,
                                FiscalYear.Id PostingPeriodId			
								 FROM				
									  (
                                     (SELECT * FROM SCS.FiscalYearPeriod WHERE
									   StartDate BETWEEN (SELECT  StartDate FROM SCS.FiscalYearPeriod where '" + calculatedFromDate.ToString("dd-MMM-yyyy") + @"' between StartDate and EndDate)
                                              AND(SELECT  EndDate FROM SCS.FiscalYearPeriod where '" + calculatedToDate.ToString("dd-MMM-yyyy") + @"' between StartDate and EndDate)
									  ) FiscalYear
									  Left join 
                                    (SELECT
                                         SUM(AnnualBudgetDetail.StandardAmount) BudgetAmount
								        , EFYP.PeriodName PostingPeriod,
                                        EFYP.Id PostingPeriodId
                                        , EFYP.EndDate PostingPeriodEndDate
									     ,    cmp.Id AS CompanyId
								           	" + cList + @" AS ColumnName
									         " + cListId + @" AS UId
									     FROM
										 MST.AnnualBudgetDetail AS AnnualBudgetDetail	
										 Left join SCS.FiscalYearPeriod AS EFYP  ON EFYP.Id = AnnualBudgetDetail.FiscalYearPeriodId
										
										LEFT JOIN  MST.AnnualBudget AS AnnualBudget ON AnnualBudget.Id = AnnualBudgetDetail.AnnualBudgetId
                                        Left JOIN MST.BudgetMaster AS BM ON BM.Id = AnnualBudgetDetail.BudgetMasterId
                                        LEFT JOIN ORG.Company AS CMP ON CMP.Id = AnnualBudget.CompanyId

                                        LEFT JOIN HKP.BudgetCategory AS BC ON BC.Id = BM.BudgetCategoryId

                                        LEFT JOIN HKP.BudgetSubCategory AS BSC ON BSC.Id = BM.BudgetSubCategoryId

                                        LEFT JOIN HKP.GLGeneralInfo AS GL ON GL.Id = BM.GLGeneralInfoId

                                        LEFT JOIN HKP.AccountGroup AS AG ON AG.Id = GL.AccountGroupId

                                        LEFT JOIN HKP.AccountType AS ACT ON ACT.Id = AG.AccountTypeId

                                        LEFT JOIN ORG.Entity AS ENT ON ENT.Id = AnnualBudgetDetail.EntityId       
                                        " + join + @"

                                        WHERE AnnualBudgetDetail.CompanyGroupId = '" + companyGroupId + @"'  AND EFYP.StartDate BETWEEN (SELECT  StartDate FROM SCS.FiscalYearPeriod where '" + calculatedFromDate.ToString("dd-MMM-yyyy") + @"' between StartDate and EndDate)
                                              AND(SELECT  EndDate FROM SCS.FiscalYearPeriod where '" + calculatedToDate.ToString("dd-MMM-yyyy") + @"' between StartDate and EndDate)

                                        AND ACT.IsBalanceSheet = 0  AND ACT.Id IN ('Revenue') " + wc + @"

                                          GROUP BY EFYP.Id , EFYP.PeriodName,EFYP.Id,EFYP.EndDate 	" + cList + @"  " + cListId + @", cmp.Id
								    ) periodWiseExpBudget ON FiscalYear.Id = periodWiseExpBudget.PostingPeriodId

                                    LEFT JOIN
                                    (SELECT SUM(CASE WHEN ISNULL(DRcumulative,0)= 0 THEN periodWiseExp.CRcumulative ELSE periodWiseExp.DRcumulative END) Amount, periodWiseExp.PostingPeriod,
                                periodWiseExp.PostingPeriodId, periodWiseExp.EntryPeriod,periodWiseExp.EntryPeriodId, periodWiseExp.EntryPeriodEndDate
								,periodWiseExp.PostingPeriodEndDate FROM
                                  (SELECT
                                        
                                          DRcumulative = CASE WHEN ACT.BalanceType = 'Debit' THEN SUM(VDC.DrAmount)-SUM(VDC.CrAmount) ELSE 0 END
								        , CRcumulative = CASE WHEN ACT.BalanceType = 'Credit' THEN SUM(VDC.crAmount)-SUM(VDC.DrAmount) ELSE 0 END
								        , EFYP.PeriodName PostingPeriod,
                                        EFYP.Id PostingPeriodId, FYPA.PeriodName AS EntryPeriod,FYPA.EntryPeriodId, FYPA.EndDate EntryPeriodEndDate
                                        , EFYP.EndDate PostingPeriodEndDate
                                            , cmp.Id AS CompanyId

                                               " + cList + @" AS ColumnName

                                             " + cListId + @" AS UId

                                        FROM TRN.VoucherDetailCurrency AS VDC
                                        LEFT JOIN TRN.VoucherDetail AS EVD ON EVD.Id = VDC.VoucherDetailId
                                        LEFT JOIN TRN.Voucher AS V ON V.Id = EVD.VoucherId
                                        LEFT JOIN ORG.Company AS CMP ON CMP.Id = V.CompanyId
                                        LEFT JOIN SCS.FiscalYearPeriod AS EFYP ON EFYP.Id = EVD.FiscalYearPeriodId

                                        LEFT JOIN MST.BudgetMaster AS BM ON BM.Id = EVD.BudgetMasterId

                                        LEFT JOIN HKP.BudgetCategory AS BC ON BC.Id = BM.BudgetCategoryId

                                        LEFT JOIN HKP.BudgetSubCategory AS BSC ON BSC.Id = BM.BudgetSubCategoryId

                                        LEFT JOIN HKP.Budget AS B ON B.Id = BM.BudgetId

                                        LEFT JOIN HKP.GLGeneralInfo AS GL ON GL.Id = BM.GLGeneralInfoId

                                        LEFT JOIN HKP.AccountGroup AS AG ON AG.Id = GL.AccountGroupId

                                        LEFT JOIN HKP.AccountType AS ACT ON ACT.Id = AG.AccountTypeId

                                        LEFT JOIN ORG.Entity AS ENT ON ENT.Id = V.EntityId
                                        " + join + @"

                                        LEFT JOIN(SELECT Id AS EntryPeriodId, PeriodName, StartDate, EndDate FROM SCS.FiscalYearPeriod)

                                                    AS FYPA ON MONTH(CONVERT(DATE, FYPA.EndDate)) = MONTH(CONVERT(DATE, evd.AddedDate))

                                            AND YEAR(CONVERT(DATE, FYPA.EndDate))= YEAR(CONVERT(DATE, evd.AddedDate))

                                        WHERE V.CompanyGroupId = '" + companyGroupId + @"'  AND V.PostingDate BETWEEN(SELECT StartDate FROM SCS.FiscalYearPeriod where '" + calculatedFromDate.ToString("dd-MMM-yyyy") + @"' between StartDate and EndDate)
                                              AND(SELECT  EndDate FROM SCS.FiscalYearPeriod where '" + calculatedToDate.ToString("dd-MMM-yyyy") + @"' between StartDate and EndDate) AND V.IsPark = 0

                                        AND ACT.IsBalanceSheet = 0  AND ACT.Id IN('Revenue') " + wc + @"

                                          GROUP BY EFYP.Id , EFYP.PeriodName,  FYPA.PeriodName, FYPA.EndDate,EFYP.EndDate,FYPA.EntryPeriodId ,ACT.BalanceType, cmp.Id
                                            " + cList + @"  " + cListId + @"
								    ) periodWiseExp

                            GROUP by periodWiseExp.PostingPeriod,periodWiseExp.PostingPeriodId, periodWiseExp.EntryPeriod,periodWiseExp.EntryPeriodId,
                            periodWiseExp.EntryPeriodEndDate, periodWiseExp.PostingPeriodEndDate
                       ) periodWiseExp
                                    ON periodWiseExp." + periodId + @" = FiscalYear.Id )";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public IEnumerable<object> DynamicPeriodWiseExpenseBarChart(IEnumerable<ChartColumnList> ChartColumnList, int seq, string factDate, string fromDate, string toDate, string companyGroupId, string companyId)
        {
            var cList = string.Empty;
            var cListId = string.Empty;
            var join = string.Empty;
            var wc = string.Empty;
            var cListextG = string.Empty;
            var cListextIdG = string.Empty;
            //string expFactDate = string.Empty;
            try
            {
                var expFactDate = string.Empty;
                var condition = string.Empty;
                var grpFactDate = string.Empty;
                var daysOfMonth = DateTime.DaysInMonth(Convert.ToDateTime(toDate).Year, Convert.ToDateTime(toDate).Month);

                DateTime calculatedToDate = new DateTime(Convert.ToDateTime(toDate).Year, Convert.ToDateTime(toDate).Month, daysOfMonth);

                DateTime calculatedFromDate = calculatedToDate.AddMonths(-12);
                var periodId = "";
                if (factDate == "postingDate")
                {
                    expFactDate = "Replace(CONVERT(VARCHAR(11),V.PostingDate,6),' ','-') factDate,";
                    condition = "AND V.PostingDate BETWEEN CONVERT(DATE,'" + fromDate + @"') AND CONVERT(DATE,'" + toDate + @"')";
                    grpFactDate = "V.PostingDate";
                    periodId = "PostingPeriodId";
                }
                else if (factDate == "AddedDate")
                {
                    expFactDate = "Replace(CONVERT(VARCHAR(11),V.AddedDate,6),' ','-') factDate,";
                    condition = "AND V.AddedDate BETWEEN CONVERT(DATE,'" + fromDate + @"') AND CONVERT(DATE,'" + toDate + @"')";
                    grpFactDate = "V.AddedDate";
                    periodId = "EntryPeriodId";

                }
                seq += 1;
                foreach (var item in ChartColumnList)
                {
                    if (item.Sequence != -2 && item.Sequence != -1)
                    {
                        if (item.Sequence <= seq)
                        {
                            if (item.RType == "Entity")
                            {
                                cList = "," + item.StandardName + ".UserName";
                                cListId = "," + item.StandardName + ".Id";
                                //cListextG = "," + item.ColumnName + "Name";
                                //cListextIdG = "," + item.ColumnName + "Id";
                                if (item.ColumnName == "EmployeeGroup")
                                {
                                    join += "LEFT JOIN [HKP].[" + item.StandardName + "] ON " + item.StandardName + ".Id = ENT." + item.StandardName + "Id\n";
                                }
                                else
                                {
                                    join += "LEFT JOIN [ORG].[" + item.StandardName + "] ON " + item.StandardName + ".Id = ENT." + item.StandardName + "Id\n";
                                }
                            }
                            else
                            {
                                cList = "," + item.StandardName + ".UserName";
                                join += "LEFT JOIN [ORG].[" + item.StandardName + "] ON " + item.StandardName + ".Id = Po." + item.StandardName + "Id\n";
                            }
                        }
                    }
                    if (item.Sequence != -2)
                    {
                        if (item.Sequence == -1)
                        {
                            wc = "  AND CMP.Id ='" + item.Id + "'";
                        }
                        else
                        {
                            if (item.Sequence < seq)
                            {
                                wc += " and " + item.StandardName + ".Id='" + item.Text + "'";
                            }
                        }
                    }
                }
                var sql = @"SELECT ISNULL(Amount,0) Amount, ISNULL(BudgetAmount,0) BudgetAmount
										,FiscalYear.PeriodName PostingPeriod,
                                FiscalYear.Id PostingPeriodId				
								 FROM				
									  (
                                     (SELECT * FROM SCS.FiscalYearPeriod WHERE
									   StartDate BETWEEN (SELECT  StartDate FROM SCS.FiscalYearPeriod where '" + calculatedFromDate.ToString("dd-MMM-yyyy") + @"' between StartDate and EndDate)
                                              AND(SELECT  EndDate FROM SCS.FiscalYearPeriod where '" + calculatedToDate.ToString("dd-MMM-yyyy") + @"' between StartDate and EndDate)
									  ) FiscalYear
									  Left join 
                                    (SELECT
                                         SUM(AnnualBudgetDetail.StandardAmount) BudgetAmount
								        , EFYP.PeriodName PostingPeriod,
                                        EFYP.Id PostingPeriodId
                                        , EFYP.EndDate PostingPeriodEndDate
									     ,    cmp.Id AS CompanyId
								           	" + cList + @" AS ColumnName
									         " + cListId + @" AS UId
									     FROM
										 MST.AnnualBudgetDetail AS AnnualBudgetDetail	
										 Left join SCS.FiscalYearPeriod AS EFYP  ON EFYP.Id = AnnualBudgetDetail.FiscalYearPeriodId
										
										LEFT JOIN  MST.AnnualBudget AS AnnualBudget ON AnnualBudget.Id = AnnualBudgetDetail.AnnualBudgetId
                                        Left JOIN MST.BudgetMaster AS BM ON BM.Id = AnnualBudgetDetail.BudgetMasterId

                                        LEFT JOIN ORG.Company AS CMP ON CMP.Id = AnnualBudget.CompanyId

                                        LEFT JOIN HKP.BudgetCategory AS BC ON BC.Id = BM.BudgetCategoryId

                                        LEFT JOIN HKP.BudgetSubCategory AS BSC ON BSC.Id = BM.BudgetSubCategoryId

                                        LEFT JOIN HKP.GLGeneralInfo AS GL ON GL.Id = BM.GLGeneralInfoId

                                        LEFT JOIN HKP.AccountGroup AS AG ON AG.Id = GL.AccountGroupId

                                        LEFT JOIN HKP.AccountType AS ACT ON ACT.Id = AG.AccountTypeId

                                        LEFT JOIN ORG.Entity AS ENT ON ENT.Id = AnnualBudgetDetail.EntityId   
                                        --LEFT JOIN ORG.Position AS PO ON PO.Id = AnnualBudgetDetail.       

                                        " + join + @"

                                        WHERE AnnualBudgetDetail.CompanyGroupId = '" + companyGroupId + @"'  AND EFYP.StartDate BETWEEN (SELECT  StartDate FROM SCS.FiscalYearPeriod where '" + calculatedFromDate.ToString("dd-MMM-yyyy") + @"' between StartDate and EndDate)
                                              AND(SELECT  EndDate FROM SCS.FiscalYearPeriod where '" + calculatedToDate.ToString("dd-MMM-yyyy") + @"' between StartDate and EndDate)

                                        AND ACT.IsBalanceSheet = 0  AND ACT.Id IN ('Expense') " + wc + @"

                                          GROUP BY EFYP.Id , EFYP.PeriodName,EFYP.Id,EFYP.EndDate " + cList + @"  " + cListId + @", cmp.Id
								    ) periodWiseExpBudget ON FiscalYear.Id = periodWiseExpBudget.PostingPeriodId

                                    left join
                                    (SELECT SUM(CASE WHEN ISNULL(DRcumulative,0)= 0 THEN periodWiseExp.CRcumulative ELSE periodWiseExp.DRcumulative END) Amount, periodWiseExp.PostingPeriod,
                                periodWiseExp.PostingPeriodId, periodWiseExp.EntryPeriod,periodWiseExp.EntryPeriodId, periodWiseExp.EntryPeriodEndDate
								,periodWiseExp.PostingPeriodEndDate FROM
                                  (SELECT
                                        
                                          DRcumulative = CASE WHEN ACT.BalanceType = 'Debit' THEN SUM(VDC.DrAmount)-SUM(VDC.CrAmount) ELSE 0 END
								        , CRcumulative = CASE WHEN ACT.BalanceType = 'Credit' THEN SUM(VDC.crAmount)-SUM(VDC.DrAmount) ELSE 0 END
								        , EFYP.PeriodName PostingPeriod,
                                        EFYP.Id PostingPeriodId, FYPA.PeriodName AS EntryPeriod,FYPA.EntryPeriodId, FYPA.EndDate EntryPeriodEndDate
                                        , EFYP.EndDate PostingPeriodEndDate
                                        , cmp.Id AS CompanyId

                                               " + cList + @" AS ColumnName

                                             " + cListId + @" AS UId

                                        FROM TRN.VoucherDetailCurrency AS VDC
                                        LEFT JOIN TRN.VoucherDetail AS EVD ON EVD.Id = VDC.VoucherDetailId
                                        LEFT JOIN TRN.Voucher AS V ON V.Id = EVD.VoucherId
                                        LEFT JOIN ORG.Company AS CMP ON CMP.Id = V.CompanyId


                                        LEFT JOIN SCS.FiscalYearPeriod AS EFYP ON EFYP.Id = EVD.FiscalYearPeriodId

                                        LEFT JOIN MST.BudgetMaster AS BM ON BM.Id = EVD.BudgetMasterId

                                        LEFT JOIN HKP.BudgetCategory AS BC ON BC.Id = BM.BudgetCategoryId

                                        LEFT JOIN HKP.BudgetSubCategory AS BSC ON BSC.Id = BM.BudgetSubCategoryId

                                        LEFT JOIN HKP.Budget AS B ON B.Id = BM.BudgetId

                                        LEFT JOIN HKP.GLGeneralInfo AS GL ON GL.Id = BM.GLGeneralInfoId

                                        LEFT JOIN HKP.AccountGroup AS AG ON AG.Id = GL.AccountGroupId

                                        LEFT JOIN HKP.AccountType AS ACT ON ACT.Id = AG.AccountTypeId

                                        LEFT JOIN ORG.Entity AS ENT ON ENT.Id = V.EntityId
                                        " + join + @"

                                        LEFT JOIN(SELECT Id AS EntryPeriodId, PeriodName, StartDate, EndDate FROM SCS.FiscalYearPeriod)

                                                    AS FYPA ON MONTH(CONVERT(DATE, FYPA.EndDate)) = MONTH(CONVERT(DATE, evd.AddedDate))

                                            AND YEAR(CONVERT(DATE, FYPA.EndDate))= YEAR(CONVERT(DATE, evd.AddedDate))

                                        WHERE V.CompanyGroupId = '" + companyGroupId + @"'  AND V.PostingDate BETWEEN(SELECT StartDate FROM SCS.FiscalYearPeriod where '" + calculatedFromDate.ToString("dd-MMM-yyyy") + @"' between StartDate and EndDate)
                                              AND(SELECT  EndDate FROM SCS.FiscalYearPeriod where '" + calculatedToDate.ToString("dd-MMM-yyyy") + @"' between StartDate and EndDate) AND V.IsPark = 0

                                        AND ACT.IsBalanceSheet = 0  AND ACT.Id IN('Expense') " + wc + @"

                                          GROUP BY EFYP.Id , EFYP.PeriodName,  FYPA.PeriodName, FYPA.EndDate,EFYP.EndDate,FYPA.EntryPeriodId ,ACT.BalanceType
                                            " + cList + @"  " + cListId + @", cmp.Id
								    ) periodWiseExp

                            GROUP by periodWiseExp.PostingPeriod,periodWiseExp.PostingPeriodId, periodWiseExp.EntryPeriod,periodWiseExp.EntryPeriodId,
                            periodWiseExp.EntryPeriodEndDate, periodWiseExp.PostingPeriodEndDate
                       ) periodWiseExp
                                    ON periodWiseExp." + periodId + @" = FiscalYear.Id )";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        #endregion Budget Wise Expense Bar Chart

        //------------------------------------------Modals-----------------------------------------------------//
        public IEnumerable<object> ModalBudgetWiseExpense(IEnumerable<ChartColumnList> ChartColumnList, int seq, string factDate, string fromDate, string toDate, string companyGroupId, string companyId, string expenseRevenue, string periodType, string postingPeriodId, string entryPeriodId)
        {
            var cList = string.Empty;
            var cListId = string.Empty;
            var join = string.Empty;
            var wc = string.Empty;
            var cListextG = string.Empty;
            var cListextIdG = string.Empty;

            var id = string.Empty;
            var columnName = string.Empty;
            var rType = string.Empty;
            var sequence = string.Empty;
            var text = string.Empty;

            try
            {
                var expFactDate = string.Empty;
                var condition = string.Empty;

                expFactDate = "AND CONVERT(DATE,V.PostingDate) <= CONVERT(DATE,GETDATE())";
                if (factDate == "postingDate")
                {
                    if (periodType == "")
                    {
                        condition = "AND FYPA.EntryPeriodId = '" + entryPeriodId + @"' AND EFYP.Id = '" + postingPeriodId + @"' AND V.PostingDate BETWEEN CONVERT(DATE,'" + fromDate + @"') AND CONVERT(DATE,'" + toDate + @"')";
                    }
                    if (periodType == "ALL")
                    {
                        condition = "AND V.PostingDate BETWEEN CONVERT(DATE,'" + fromDate + @"') AND CONVERT(DATE,'" + toDate + @"')";
                    }
                    if (periodType == "FORTHEDAY")
                    {
                        condition = "AND V.PostingDate BETWEEN CONVERT(DATE,'" + toDate + @"') AND CONVERT(DATE,'" + toDate + @"')";
                    }
                    if (periodType == "FORTHEPERIOD")
                    {
                        condition = @" AND V.PostingDate BETWEEN (SELECT  StartDate FROM SCS.FiscalYearPeriod where '" + toDate + @"' between StartDate and EndDate)
                                              AND(SELECT  EndDate FROM SCS.FiscalYearPeriod where '" + toDate + @"' between StartDate and EndDate)";
                    }
                    if (periodType == "DELAY")
                    {
                        condition = @"AND CONVERT(DATE,V.AddedDate) = CONVERT(DATE, '" + toDate + @"') AND MONTH(V.PostingDate) < MONTH('" + toDate + @"') AND YEAR(V.PostingDate) <= YEAR('" + toDate + @"')";
                    }

                }
                else if (factDate == "AddedDate")
                {
                    if (periodType == "")
                    {
                        condition = "AND FYPA.EntryPeriodId = '" + entryPeriodId + @"' AND EFYP.Id = '" + postingPeriodId + @"' AND V.AddedDate BETWEEN CONVERT(DATE,'" + fromDate + @"') AND CONVERT(DATE,'" + toDate + @"')";
                    }
                    if (periodType == "ALL")
                    {
                        condition = "AND V.AddedDate BETWEEN CONVERT(DATE,'" + fromDate + @"') AND CONVERT(DATE,'" + toDate + @"')";
                    }
                    if (periodType == "FORTHEDAY")
                    {
                        condition = "AND V.AddedDate BETWEEN CONVERT(DATE,'" + toDate + @"') AND CONVERT(DATE,'" + toDate + @"')";
                    }
                    if (periodType == "FORTHEPERIOD")
                    {
                        condition = @" AND V.AddedDate BETWEEN (SELECT  StartDate FROM SCS.FiscalYearPeriod where '" + toDate + @"' between StartDate and EndDate)
                                              AND(SELECT  EndDate FROM SCS.FiscalYearPeriod where '" + toDate + @"' between StartDate and EndDate)";
                    }
                    if (periodType == "DELAY")
                    {
                        condition = @"AND CONVERT(DATE,V.AddedDate) = CONVERT(DATE, '" + toDate + @"') AND MONTH(V.PostingDate) < MONTH('" + toDate + @"') AND YEAR(V.PostingDate) <= YEAR('" + toDate + @"')";
                    }
                }
                else
                {
                    condition = "";
                }
                if (seq == -2)
                {
                    foreach (var item in ChartColumnList)
                    {
                        if (item.Sequence != -2 && item.Sequence != -1)
                        {
                            if (item.RType == "Entity")
                            {
                                cList += "," + item.StandardName + ".UserName " + item.StandardName + " ";
                                cListId += "," + item.StandardName + ".Id " + item.StandardName + " ";

                                cListextG += "," + item.StandardName + ".UserName ";
                                cListextIdG += "," + item.StandardName + ".Id";

                                if (item.StandardName == "EmployeeGroup")
                                {
                                    join += "LEFT JOIN [HKP].[" + item.StandardName + "] ON " + item.StandardName + ".Id = E." + item.StandardName + "Id\n";
                                }
                                else
                                {
                                    join += "LEFT JOIN [ORG].[" + item.StandardName + "] ON " + item.StandardName + ".Id = E." + item.StandardName + "Id\n";
                                }
                            }

                        }
                    }
                    var sql = @"Select  ItemWExpense.BudgetName,ItemWExpense.BudgetId,ItemWExpense.BudgetCategoryName,ItemWExpense.BudgetSubCategoryName
		                        , SUM(CASE WHEN ISNULL(ItemWExpense.DRcumulative,0)=0 THEN ItemWExpense.CRcumulative ELSE ItemWExpense.DRcumulative END) Amount
		                        , CategorySequence,SubcategorySequence,ItemSequence,PostingPeriod,EntryPeriod,PostingPeriodId,EntryPeriodId
		                          FROM(
		                                 SELECT	B.UserName AS BudgetName,B.Id AS BudgetId,
							    BC.UserName  AS BudgetCategoryName,
							    BSC.UserName AS BudgetSubCategoryName                           
                          

								   , DRcumulative = CASE WHEN ACT.BalanceType = 'Debit' THEN SUM(VDC.DrAmount)-SUM(VDC.CrAmount) ELSE 0 END
					            , CRcumulative = CASE WHEN ACT.BalanceType = 'Credit' THEN SUM(VDC.crAmount)-SUM(VDC.DrAmount) ELSE 0 END
                                , BC.Sequence CategorySequence,BSC.Sequence SubcategorySequence,B.Sequence ItemSequence
							    --,SUM(VDC.DrAmount) Amount
                                ,EFYP.PeriodName PostingPeriod,FYPA.PeriodName EntryPeriod,EFYP.Id PostingPeriodId,FYPA.EntryPeriodId 
								FROM TRN.VoucherDetailCurrency AS VDC
								JOIN TRN.VoucherDetail AS VD ON VD.Id=VDC.VoucherDetailId
								JOIN TRN.Voucher AS V ON V.Id =VD.VoucherId
								LEFT JOIN ORG.Company AS CMP on CMP.Id = V.CompanyId
								LEFT JOIN ORG.CompanyGroup AS CMPGR on CMPGR.Id = V.CompanyGroupId
								LEFT JOIN MST.BudgetMaster AS BM ON BM.Id =VD.BudgetMasterId
								LEFT JOIN HKP.Budget AS B ON B.Id=BM.BudgetId
								LEFT JOIN HKP.BudgetSubCategory AS BSC ON BSC.Id=BM.BudgetSubCategoryId
								LEFT JOIN HKP.BudgetCategory AS BC ON BC.Id=BM.BudgetCategoryId
								LEFT JOIN SCS.FiscalYearPeriod AS EFYP ON EFYP.Id=VD.FiscalYearPeriodId

								LEFT JOIN ORG.Entity AS E ON E.Id = V.EntityId
								LEFT JOIN ORG.Company AS C ON C.Id = V.CompanyId
								LEFT JOIN [ORG].[Plant] ON Plant.Id = E.PlantId
							    LEFT JOIN [ORG].[Division] ON Division.Id = E.DivisionId
							    LEFT JOIN [ORG].[SubDivision] ON SubDivision.Id = E.SubDivisionId
							    LEFT JOIN [ORG].[Unit] ON Unit.Id = E.UnitId
							    LEFT JOIN HKP.GLGeneralInfo AS GL ON GL.Id=BM.GLGeneralInfoId
					            LEFT JOIN HKP.AccountGroup AS AG ON AG.Id=GL.AccountGroupId
					            LEFT JOIN HKP.AccountType AS ACT ON ACT.Id=AG.AccountTypeId
                                LEFT OUTER JOIN (SELECT Id AS EntryPeriodId, PeriodName,StartDate,EndDate FROM SCS.FiscalYearPeriod  )AS
								    FYPA ON MONTH(CONVERT(DATE,FYPA.EndDate))=MONTH(CONVERT(DATE,VD.AddedDate))
								    AND  YEAR(CONVERT(DATE,FYPA.EndDate))=YEAR(CONVERT(DATE,VD.AddedDate))
								LEFT JOIN (SELECT * FROM SCS.CompanyParallelCurrency WHERE ParallelCurrencyType='CompanyCurrency') as
								 CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId AND CPC.CompanyId = C.Id
								WHERE   ACT.IsBalanceSheet =  0  AND ACT.Id  = '" + expenseRevenue + @"' AND V.IsPark = 0	AND  VD.BudgetMasterId  IS NOT NULL
								" + condition + @"
								" + wc + @"GROUP BY B.Id,B.UserName,BC.UserName,BSC.UserName,ACT.BalanceType
                                , BC.Sequence ,BSC.Sequence ,B.Sequence,EFYP.PeriodName,FYPA.PeriodName,EFYP.Id,FYPA.EntryPeriodId ) ItemWExpense
								 GROUP BY ItemWExpense.BudgetName,ItemWExpense.BudgetId,ItemWExpense.BudgetCategoryName,ItemWExpense.BudgetSubCategoryName
                                  ,CategorySequence,SubcategorySequence,ItemSequence,EntryPeriod,PostingPeriod,PostingPeriodId,EntryPeriodId";

                    return _sqlRepository.GetDataCollection(sql);
                }
                else
                {

                    seq += 1;
                    foreach (var item in ChartColumnList)
                    {
                        if (item.Sequence != -2 && item.Sequence != -1)
                        {
                            if (item.RType == "Entity")
                            {
                                cList += "," + item.StandardName + ".UserName " + item.StandardName + " ";

                                if (item.StandardName == "EmployeeGroup")
                                {
                                    join += "LEFT JOIN [HKP].[" + item.StandardName + "] ON " + item.StandardName + ".Id = E." + item.StandardName + "Id\n";
                                }
                                else
                                {
                                    join += "LEFT JOIN [ORG].[" + item.StandardName + "] ON " + item.StandardName + ".Id = E." + item.StandardName + "Id\n";
                                }
                            }
                        }

                        if (item.Sequence != -2)
                        {
                            if (item.Sequence == -1)
                            {
                                wc = " and  C.Id ='" + item.Id + "'";
                            }
                            else
                            {
                                if (item.Sequence < seq)
                                {
                                    wc += " AND " + item.StandardName + ".Id='" + item.Text + "'";
                                }
                            }
                        }
                    }

                    var sql = @"Select  ItemWExpense.BudgetName,ItemWExpense.BudgetId,ItemWExpense.BudgetCategoryName,ItemWExpense.BudgetSubCategoryName
		                        , SUM(CASE WHEN ISNULL(ItemWExpense.DRcumulative,0)=0 THEN ItemWExpense.CRcumulative ELSE ItemWExpense.DRcumulative END) Amount
		                        , CategorySequence,SubcategorySequence,ItemSequence,PostingPeriod,EntryPeriod,PostingPeriodId,EntryPeriodId
		                          FROM(
		                                 SELECT	B.UserName AS BudgetName,B.Id AS BudgetId,
							    BC.UserName  AS BudgetCategoryName,
							    BSC.UserName AS BudgetSubCategoryName
                               
                            
								   , DRcumulative = CASE WHEN ACT.BalanceType = 'Debit' THEN SUM(VDC.DrAmount)-SUM(VDC.CrAmount) ELSE 0 END
					            , CRcumulative = CASE WHEN ACT.BalanceType = 'Credit' THEN SUM(VDC.crAmount)-SUM(VDC.DrAmount) ELSE 0 END
                                , BC.Sequence CategorySequence,BSC.Sequence SubcategorySequence,B.Sequence ItemSequence
							    --,SUM(VDC.DrAmount) Amount
                                ,EFYP.PeriodName PostingPeriod,FYPA.PeriodName EntryPeriod,EFYP.Id PostingPeriodId,FYPA.EntryPeriodId 
								FROM TRN.VoucherDetailCurrency AS VDC
								JOIN TRN.VoucherDetail AS VD ON VD.Id=VDC.VoucherDetailId
								JOIN TRN.Voucher AS V ON V.Id =VD.VoucherId
								LEFT JOIN ORG.Company AS CMP on CMP.Id = V.CompanyId
								LEFT JOIN ORG.CompanyGroup AS CMPGR on CMPGR.Id = V.CompanyGroupId
								LEFT JOIN MST.BudgetMaster AS BM ON BM.Id =VD.BudgetMasterId
								LEFT JOIN HKP.Budget AS B ON B.Id=BM.BudgetId
								LEFT JOIN HKP.BudgetSubCategory AS BSC ON BSC.Id=BM.BudgetSubCategoryId
								LEFT JOIN HKP.BudgetCategory AS BC ON BC.Id=BM.BudgetCategoryId
								LEFT JOIN SCS.FiscalYearPeriod AS EFYP ON EFYP.Id=VD.FiscalYearPeriodId

								LEFT JOIN ORG.Entity AS E ON E.Id = V.EntityId
								LEFT JOIN ORG.Company AS C ON C.Id = V.CompanyId
								LEFT JOIN [ORG].[Plant] ON Plant.Id = E.PlantId
							    LEFT JOIN [ORG].[Division] ON Division.Id = E.DivisionId
							    LEFT JOIN [ORG].[SubDivision] ON SubDivision.Id = E.SubDivisionId
							    LEFT JOIN [ORG].[Unit] ON Unit.Id = E.UnitId
							    LEFT JOIN HKP.GLGeneralInfo AS GL ON GL.Id=BM.GLGeneralInfoId
					            LEFT JOIN HKP.AccountGroup AS AG ON AG.Id=GL.AccountGroupId
					            LEFT JOIN HKP.AccountType AS ACT ON ACT.Id=AG.AccountTypeId
                                LEFT OUTER JOIN (SELECT Id AS EntryPeriodId, PeriodName,StartDate,EndDate FROM SCS.FiscalYearPeriod  )AS
								    FYPA ON MONTH(CONVERT(DATE,FYPA.EndDate))=MONTH(CONVERT(DATE,VD.AddedDate))
								    AND  YEAR(CONVERT(DATE,FYPA.EndDate))=YEAR(CONVERT(DATE,VD.AddedDate))
								LEFT JOIN (SELECT * FROM SCS.CompanyParallelCurrency WHERE ParallelCurrencyType='CompanyCurrency') as
								 CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId AND CPC.CompanyId = C.Id
								WHERE   ACT.IsBalanceSheet =  0  AND ACT.Id  = '" + expenseRevenue + @"' AND V.IsPark = 0	AND  VD.BudgetMasterId  IS NOT NULL
								" + condition + @"
								" + wc + @"GROUP BY B.Id,B.UserName,BC.UserName,BSC.UserName,ACT.BalanceType
                                , BC.Sequence ,BSC.Sequence ,B.Sequence,EFYP.PeriodName,FYPA.PeriodName,EFYP.Id,FYPA.EntryPeriodId ) ItemWExpense
								 GROUP BY ItemWExpense.BudgetName,ItemWExpense.BudgetId,ItemWExpense.BudgetCategoryName,ItemWExpense.BudgetSubCategoryName
                                  ,CategorySequence,SubcategorySequence,ItemSequence,EntryPeriod,PostingPeriod,PostingPeriodId,EntryPeriodId";

                    return _sqlRepository.GetDataCollection(sql);
                }
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }

        public IEnumerable<object> ModalExpenseDetail(IEnumerable<ChartColumnList> ChartColumnList, int seq
            , string budgetId, string factDate, string fromDate, string toDate, string companyGroupId, string companyId, string entryPeriodId, string postingPeriodId, string expenseORRevenue, string periodType)
        {
            var cList = string.Empty;
            var cListId = string.Empty;
            var join = string.Empty;
            var wc = string.Empty;
            var cListextG = string.Empty;
            var cListextIdG = string.Empty;
            //Array[] ChartColumnListM2 = ChartColumnListM;

            var id = string.Empty;
            var columnName = string.Empty;
            var rType = string.Empty;
            var sequence = string.Empty;
            var text = string.Empty;

            try
            {
                var expFactDate = string.Empty;
                var condition = string.Empty;

                expFactDate = "AND CONVERT(DATE,V.PostingDate) <= CONVERT(DATE,GETDATE())";
                if (factDate == "postingDate")
                {
                    if (periodType == "ALL")
                    {
                        condition = " AND V.PostingDate BETWEEN CONVERT(DATE,'" + fromDate + @"') AND CONVERT(DATE,'" + toDate + @"')";
                    }
                    if (periodType == "FORTHEDAY")
                    {
                        condition = "AND V.PostingDate BETWEEN CONVERT(DATE,'" + toDate + @"') AND CONVERT(DATE,'" + toDate + @"')";
                    }
                    if (periodType == "FORTHEPERIOD")
                    {
                        condition = @" AND V.PostingDate BETWEEN (SELECT  StartDate FROM SCS.FiscalYearPeriod where '" + toDate + @"' between StartDate and EndDate)
                                              AND(SELECT  EndDate FROM SCS.FiscalYearPeriod where '" + toDate + @"' between StartDate and EndDate)";
                    }
                    if (periodType == "")

                    {
                        condition = "AND FYPA.EntryPeriodId = '" + entryPeriodId + @"' AND EFYP.Id = '" + postingPeriodId + @"' AND V.PostingDate BETWEEN CONVERT(DATE,'" + fromDate + @"') AND CONVERT(DATE,'" + toDate + @"')";
                    }
                    if (periodType == "DELAY")
                    {
                        condition = @"AND CONVERT(DATE,V.AddedDate) = CONVERT(DATE, '" + toDate + @"') AND MONTH(V.PostingDate) < MONTH('" + toDate + @"') AND YEAR(V.PostingDate) <= YEAR('" + toDate + @"')";
                    }
                }
                else if (factDate == "AddedDate")
                {
                    if (periodType == "ALL")
                    {
                        condition = "AND V.AddedDate BETWEEN CONVERT(DATE,'" + fromDate + @"') AND CONVERT(DATE,'" + toDate + @"')";
                    }
                    if (periodType == "FORTHEDAY")
                    {
                        condition = "AND V.AddedDate BETWEEN CONVERT(DATE,'" + toDate + @"') AND CONVERT(DATE,'" + toDate + @"')";
                    }
                    if (periodType == "FORTHEPERIOD")
                    {
                        condition = @" AND V.AddedDate BETWEEN (SELECT  StartDate FROM SCS.FiscalYearPeriod where '" + toDate + @"' between StartDate and EndDate)
                                              AND(SELECT  EndDate FROM SCS.FiscalYearPeriod where '" + toDate + @"' between StartDate and EndDate)";
                    }
                    if (periodType == "")

                    {
                        condition = "AND FYPA.EntryPeriodId = '" + entryPeriodId + @"' AND EFYP.Id = '" + postingPeriodId + @"' AND V.AddedDate BETWEEN CONVERT(DATE,'" + fromDate + @"') AND CONVERT(DATE,'" + toDate + @"')";
                    }
                    if (periodType == "DELAY")
                    {
                        condition = @"AND CONVERT(DATE,V.AddedDate) = CONVERT(DATE, '" + toDate + @"') AND MONTH(V.PostingDate) < MONTH('" + toDate + @"') AND YEAR(V.PostingDate) <= YEAR('" + toDate + @"')";
                    }
                }
                else
                {
                    condition = "";
                }
                seq += 1;
                foreach (var item in ChartColumnList)
                {
                    if (item.Sequence != -2 && item.Sequence != -1)
                    {
                        if (item.RType == "Entity")
                        {
                            cList += "," + item.StandardName + ".UserName " + item.StandardName + " ";

                            if (item.ColumnName == "EmployeeGroup")
                            {
                                join += "LEFT JOIN [HKP].[" + item.StandardName + "] ON " + item.StandardName + ".Id = E." + item.StandardName + "Id\n";
                            }
                            else
                            {
                                join += "LEFT JOIN [ORG].[" + item.StandardName + "] ON " + item.StandardName + ".Id = E." + item.StandardName + "Id\n";
                            }
                        }
                        //else
                        //{
                        //	cList += "," + item.ColumnName + ".UserName " + item.ColumnName + " ";
                        //	join += "LEFT JOIN [ORG].[" + item.ColumnName + "] ON " + item.ColumnName + ".Id = Po." + item.ColumnName + "Id\n";

                        //}
                    }

                    if (item.Sequence != -2)
                    {
                        if (item.Sequence == -1)
                        {
                            wc = " and  C.Id ='" + item.Id + "'";
                        }
                        else
                        {
                            if (item.Sequence < seq)
                            {
                                wc += " AND " + item.StandardName + ".Id='" + item.Text + "'";
                            }
                        }
                    }
                }

                var sql = @"SELECT	BC.UserName AS BudgetCategoryName,B.Id BudgetId
							    " + cList + @"
								,cmp.Id AS CompanyId
                                ,CMPGR.Id CompanyGroupId
                                ,V.PlantId,II.Id InventoryIssueId
								,ISNULL(BSC.UserName,'') AS BudgetSubCategoryName
								,ISNULL(B.UserName,'') AS BudgetName
								,ISNULL(V.SourceType,'') SourceType
                                ,Amount=CASE WHEN (CASE WHEN ACT.BalanceType = 'Debit' THEN VDC.DrAmount-VDC.CrAmount ELSE 0 END)=0 then 
								CASE WHEN ACT.BalanceType = 'Credit' THEN VDC.crAmount-VDC.DrAmount ELSE 0 END ELSE (CASE WHEN ACT.BalanceType = 'Debit' THEN VDC.DrAmount-VDC.CrAmount ELSE 0 END) END
								 ,A.UserName ActivityName
								,V.Id VoucherId,V.VoucherNo,v.DocRefNo,Replace(CONVERT(VARCHAR(11), V.DocDate, 106), ' ', '-') DocDate,Replace(CONVERT(VARCHAR(11), V.PostingDate, 106), ' ', '-') PostingDate
								,Beneficiary =concat( STUFF((select distinct ','+xp.UserName from
														TRN.VoucherDetail XVD JOIN [HKP].[Party] AS XP ON XP.Id=XVD.PartyId
													    where	XVD.VoucherId=V.Id AND XVD.PartyId<>'' AND VD.ActivityId!=XVD.ActivityId  for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
												,STUFF((select distinct ','+xp.AccountTitle from
														TRN.VoucherDetail XVD JOIN MST.BankMaster AS XP ON XP.Id=XVD.BankMasterId
													where	XVD.VoucherId=V.Id AND XVD.BankMasterId<>'' AND VD.ActivityId!=XVD.ActivityId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
												 , STUFF((select distinct ','+xp.UserName from
														TRN.VoucherDetail XVD JOIN MST.CashMaster AS XP ON XP.Id=XVD.CashMasterId
													where	XVD.VoucherId=V.Id AND XVD.CashMasterId<>'' AND VD.ActivityId!=XVD.ActivityId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
												 ,STUFF((select distinct ','+xp.EmployeeName from
														TRN.VoucherDetail XVD JOIN [dbo].[EmployeeInformation] AS XP ON XP.SystemId=XVD.EmployeeId
													where	XVD.VoucherId=V.Id AND XVD.EmployeeId<>'' AND VD.ActivityId!=XVD.ActivityId  for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                                               )
                                FROM TRN.VoucherDetailCurrency AS VDC
								JOIN TRN.VoucherDetail AS VD ON VD.Id=VDC.VoucherDetailId
								JOIN TRN.Voucher AS V ON V.Id =VD.VoucherId
	                            left join trn.InventoryIssue II on II.VoucherId = v.Id
								LEFT JOIN ORG.Company AS CMP on CMP.Id = V.CompanyId
								LEFT JOIN ORG.CompanyGroup AS CMPGR on CMPGR.Id = V.CompanyGroupId
								LEFT JOIN MST.BudgetMaster AS BM ON BM.Id =VD.BudgetMasterId
								LEFT JOIN HKP.Budget AS B ON B.Id=BM.BudgetId
								LEFT JOIN HKP.BudgetSubCategory AS BSC ON BSC.Id=BM.BudgetSubCategoryId
								LEFT JOIN HKP.BudgetCategory AS BC ON BC.Id=BM.BudgetCategoryId
								LEFT JOIN ORG.Entity AS E ON E.Id = VD.EntityId
                                LEFT JOIN HKP.GLGeneralInfo AS GL ON GL.Id=BM.GLGeneralInfoId
					            LEFT JOIN HKP.AccountGroup AS AG ON AG.Id=GL.AccountGroupId
					            LEFT JOIN HKP.AccountType AS ACT ON ACT.Id=AG.AccountTypeId
                                LEFT JOIN HKP.Activity A ON A.Id=VD.ActivityId
                                LEFT JOIN SCS.FiscalYearPeriod AS EFYP ON EFYP.Id=VD.FiscalYearPeriodId
                                LEFT OUTER JOIN (SELECT Id AS EntryPeriodId, PeriodName,StartDate,EndDate FROM SCS.FiscalYearPeriod  )AS
								    FYPA ON MONTH(CONVERT(DATE,FYPA.EndDate))=MONTH(CONVERT(DATE,VD.AddedDate))
								    AND  YEAR(CONVERT(DATE,FYPA.EndDate))=YEAR(CONVERT(DATE,VD.AddedDate))
								LEFT JOIN ORG.Company AS C ON C.Id = V.CompanyId
											" + join + @"
								LEFT JOIN (SELECT * FROM SCS.CompanyParallelCurrency WHERE ParallelCurrencyType='CompanyCurrency') as CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId AND CPC.CompanyId = C.Id
								WHERE  VD.BudgetMasterId  IS NOT NULL " + condition + @" AND  CPC.ParallelCurrencyType='CompanyCurrency' AND V.IsPark = 0 " + wc + @"
								 AND	BM.BudgetId = " + budgetId + @" and FYPA.EntryPeriodId = '" + entryPeriodId + @"' and EFYP.Id = '" + postingPeriodId + @"'  AND ACT.IsBalanceSheet =  0  AND ACT.Id = '" + expenseORRevenue + @"' ";

                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }

        public GridModel ModalVoucharDetail(GridParameter parameter, string voucharNo, string budgetId, string factDate, string fromDate, string toDate, string companyGroupId, string companyId, string expenseORRevenue, string periodType)
        {
            string cList = string.Empty;
            string cListId = string.Empty;
            string join = string.Empty;
            string wc = string.Empty;
            string cListextG = string.Empty;
            string cListextIdG = string.Empty;

            try
            {
                string expFactDate = string.Empty;
                string condition = string.Empty;

                IEnumerable<OrgStructureListViewModel> OrgStrList = OrgStructureListM(companyGroupId, companyId);
                expFactDate = "AND CONVERT(DATE,V.PostingDate) <= CONVERT(DATE,GETDATE())";
                if (factDate == "postingDate")
                {
                    if (periodType == "ALL")
                    {
                        condition = "AND V.PostingDate BETWEEN CONVERT(DATE,'" + fromDate + @"') AND CONVERT(DATE,'" + toDate + @"')";
                    }
                    if (periodType == "FORTHEDAY")
                    {
                        condition = "AND V.PostingDate BETWEEN CONVERT(DATE,'" + toDate + @"') AND CONVERT(DATE,'" + toDate + @"')";
                    }
                    if (periodType == "FORTHEPERIOD")
                    {
                        condition = @" AND V.PostingDate BETWEEN (SELECT  StartDate FROM SCS.FiscalYearPeriod where '" + toDate + @"' between StartDate and EndDate)
                                              AND(SELECT  EndDate FROM SCS.FiscalYearPeriod where '" + toDate + @"' between StartDate and EndDate)";
                    }

                }
                else if (factDate == "AddedDate")
                {
                    if (periodType == "ALL")
                    {
                        condition = "AND V.AddedDate BETWEEN CONVERT(DATE,'" + fromDate + @"') AND CONVERT(DATE,'" + toDate + @"')";
                    }
                    if (periodType == "FORTHEDAY")
                    {
                        condition = "AND V.AddedDate BETWEEN CONVERT(DATE,'" + toDate + @"') AND CONVERT(DATE,'" + toDate + @"')";
                    }
                    if (periodType == "FORTHEPERIOD")
                    {
                        condition = @" AND V.AddedDate BETWEEN (SELECT  StartDate FROM SCS.FiscalYearPeriod where '" + toDate + @"' between StartDate and EndDate)
                                              AND(SELECT  EndDate FROM SCS.FiscalYearPeriod where '" + toDate + @"' between StartDate and EndDate)";
                    }
                }
                else
                {
                    condition = "";
                }
                foreach (var item in OrgStrList)
                {
                    if (item.RType == "Entity")
                    {
                        cList += "," + item.StandardName + ".UserName " + item.StandardName + " ";

                        if (item.StandardName == "EmployeeGroup")
                        {
                            join += "LEFT JOIN [HKP].[" + item.StandardName + "] AS " + item.StandardName + " ON " + item.StandardName + ".Id = E." + item.StandardName + "Id\n";
                        }
                        else
                        {
                            join += "LEFT JOIN [ORG].[" + item.StandardName + "] AS " + item.StandardName + " ON " + item.StandardName + ".Id = E." + item.StandardName + "Id\n";
                        }
                    }
                    else
                    {
                        //cGList += "," + item.ColumnName + ".UserName";
                        cList += "," + item.StandardName + ".UserName " + item.StandardName + " ";
                        join += "LEFT JOIN [ORG].[" + item.StandardName + "] AS " + item.StandardName + " ON " + item.StandardName + ".Id = Po." + item.StandardName + "Id\n";
                    }
                }

                parameter.CmdText = @"SELECT  GLGI.AccountCode+' - '+GLGI.UserName AS GLGeneralInfoName, PT.Code+' - '+PT.UserName As PartyName, VD.BudgetMasterId,B.Id BudgetId, B.UserName AS BudgetName, VD.ActivityId, A.UserName AS ActivityName,
                                        V.VoucherNo, Replace(CONVERT(VARCHAR(11), V.DocDate, 106), ' ', '-') DocDate ,Replace(CONVERT(VARCHAR(11), V.PostingDate, 106), ' ', '-') PostingDate, VD.DocRefNo, V.Narration, VD.EntityId,VD.PlantId,  VD.VoucherId,
                                        VD.Id AS VoucherDetailId, V.CurrencyId, C.Code AS CurrencyCode, VD.PartyId,
										CC.CompanyCurrencyId, CC.CompanyFromCurrencyId, CC.ToCurrencyId, CC.CompanyCurrencyRate,CC.CompanyCurrencyDrAmount,CC.CompanyCurrencyCrAmount, CC.CompanyCurrencyConversion,
										GC.CompanyGroupCurrencyId, GC.CompanyGroupFromCurrencyId, GC.CompanyGroupCurrencyAmount,GC.CompanyGroupCurrencyRate, GC.CompanyGroupCurrencyConversion,
										HC.HardCurrencyId, HC.HardFromCurrencyId, HC.HardCurrencyRate, HC.HardCurrencyConversion
                                               FROM
											   [TRN].[VoucherDetail] AS VD
                                               LEFT JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                                               LEFT JOIN [HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id=VD.GLGeneralInfoId
                							   LEFT JOIN [MST].[BudgetMaster] AS BM ON BM.Id=VD.BudgetMasterId
                							   LEFT JOIN [HKP].[Budget] AS B ON B.Id=BM.BudgetId
                							   LEFT JOIN [HKP].[Activity] AS A ON A.Id=VD.ActivityId
                                               LEFT JOIN [SCS].[Currency] AS C ON C.Id=V.CurrencyId
                                               LEFT JOIN [HKP].[Party] AS PT ON PT.Id=VD.PartyId
	                                           LEFT JOIN HKP.GLGeneralInfo AS GL ON GL.Id=BM.GLGeneralInfoId
					                           LEFT JOIN HKP.AccountGroup AS AG ON AG.Id=GL.AccountGroupId
					                           LEFT JOIN HKP.AccountType AS ACT ON ACT.Id=AG.AccountTypeId
                						LEFT JOIN (
                						SELECT VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.FromCurrencyId AS CompanyFromCurrencyId, VDC.ToCurrencyId,
                						VDC.ToCurrencyRate AS CompanyCurrencyRate, VDC.ToCurrencyConversion AS CompanyCurrencyConversion, VDC.DrAmount AS CompanyCurrencyDrAmount,VDC.CrAmount AS CompanyCurrencyCrAmount, VDC.VoucherDetailId
                						FROM [TRN].[VoucherDetailCurrency] AS VDC
                						JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
                						WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId='" + companyId + @"'
										) AS CC ON CC.VoucherDetailId=VD.Id
										LEFT JOIN (
										SELECT VDC.ParallelCurrencyId AS CompanyGroupCurrencyId, VDC.FromCurrencyId AS CompanyGroupFromCurrencyId, VDC.ToCurrencyId,
											VDC.ToCurrencyRate AS CompanyGroupCurrencyRate, VDC.ToCurrencyConversion AS CompanyGroupCurrencyConversion, VDC.DrAmount AS CompanyGroupCurrencyAmount, VDC.VoucherDetailId
											FROM [TRN].[VoucherDetailCurrency] AS VDC
											JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
											WHERE CPC.ParallelCurrencyType='CompanyGroupCurrency' AND CPC.CompanyId='" + companyId + @"'
										) AS GC ON GC.VoucherDetailId=VD.Id
										LEFT JOIN (
											SELECT VDC.ParallelCurrencyId AS HardCurrencyId, VDC.FromCurrencyId AS HardFromCurrencyId, VDC.ToCurrencyId,
											VDC.ToCurrencyRate AS HardCurrencyRate, VDC.ToCurrencyConversion AS HardCurrencyConversion, VDC.DrAmount AS HardCurrencyAmount, VDC.VoucherDetailId
											FROM [TRN].[VoucherDetailCurrency] AS VDC
											JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
											WHERE CPC.ParallelCurrencyType='HardCurrency' AND CPC.CompanyId= '" + companyId + @"'
										) AS HC ON HC.VoucherDetailId=VD.Id
										WHERE  V.VoucherNo='" + voucharNo + @"' AND V.IsPark = 0 ";

                return _sqlRepository.GetGridData(parameter);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }

        public IEnumerable<object> GetBudgetMasterWiseAmount(string companyGroupId, string companyId, string plantId, string divisionId, string subDivisionId, string unitId, string budgetCategory, string budgetSubCategory, string budget, string Activity, string budgetMasterId, string fromDate, string toDate, string periodName, string dateType, string dayOrPeriod, string PostingPeriodId, string EntryPeriodId)
        {
            var bCId = string.Empty; var bCSId = string.Empty; var bId = string.Empty;
            var company = string.Empty;
            var plant = string.Empty;
            var division = string.Empty;
            var subDivision = string.Empty;
            var unit = string.Empty;
            var periodChange = string.Empty;

            if (dayOrPeriod == "day")
                fromDate = toDate;

            try
            {
                var cmdText = @"SELECT  BC.UserName AS [BudgetCategory],BSC.UserName AS [BudgetSubCategory],B.UserName AS [Budget]
									,Amount=CASE WHEN ISNULL(VDC.DrAmount,0)=0 THEN VDC.CrAmount ELSE VDC.DrAmount END
									,EFYP.PeriodName PostingPeriod,FYPA.PeriodName EntryPeriod
									,EVD.BudgetMasterId,EVD.Id,EVD.VoucherId,FYPA.PeriodName as EntryPeriod
									,V.VoucherNo,REPLACE(CONVERT(VARCHAR(11),V.VoucherDate, 106), ' ', '-') VoucherDate
                                    ,V.VoucherNo,REPLACE(CONVERT(VARCHAR(11),V.PostingDate, 106), ' ', '-') PostingDate
									FROM TRN.VoucherDetailCurrency AS VDC
									LEFT JOIN TRN.VoucherDetail AS EVD ON EVD.Id=VDC.VoucherDetailId
									LEFT JOIN TRN.Voucher AS V ON V.Id=EVD.VoucherId
									LEFT JOIN SCS.FiscalYearPeriod AS EFYP ON EFYP.Id=EVD.FiscalYearPeriodId
									LEFT JOIN MST.BudgetMaster AS BM ON BM.Id=EVD.BudgetMasterId
									LEFT JOIN HKP.BudgetCategory AS BC ON BC.Id=BM.BudgetCategoryId
									LEFT JOIN HKP.BudgetSubCategory AS BSC ON BSC.Id=BM.BudgetSubCategoryId
									LEFT JOIN HKP.Budget AS B ON B.Id=BM.BudgetId
									LEFT JOIN ORG.Entity AS ENT ON ENT.Id=V.EntityId
								    LEFT OUTER JOIN (SELECT Id AS EntryPeriodId, PeriodName,StartDate,EndDate FROM SCS.FiscalYearPeriod  )AS
								    FYPA ON MONTH(CONVERT(DATE,FYPA.EndDate))=MONTH(CONVERT(DATE,EVD.AddedDate))
								    AND  YEAR(CONVERT(DATE,FYPA.EndDate))=YEAR(CONVERT(DATE,EVD.AddedDate))

									WHERE  CONVERT(DATE, V." + dateType + ") BETWEEN CONVERT(DATE, '" + fromDate + "') AND   CONVERT(DATE, '" + toDate + @"')
                                    AND EFYP.Id ='" + PostingPeriodId + @"' AND FYPA.EntryPeriodId = '" + EntryPeriodId + @"' and AND EVD.IsPark = 0

                                  AND EVD.BudgetMasterId = '" + budgetMasterId + @"'";

                if (!string.IsNullOrEmpty(companyId))
                    cmdText += "AND V.CompanyId='" + companyId + "'";
                if (!string.IsNullOrEmpty(plantId))
                    cmdText += "AND ENT.PlantId='" + plantId + "' ";
                if (!string.IsNullOrEmpty(divisionId))
                    cmdText += "AND ENT.DivisionId ='" + divisionId + "' ";
                if (!string.IsNullOrEmpty(subDivisionId))
                    cmdText += "AND ENT.SubDivisionId='" + subDivisionId + "' ";
                if (!string.IsNullOrEmpty(unitId))
                    cmdText += "AND ENT.UnitId ='" + unitId + "' ";
                if (!string.IsNullOrEmpty(budgetCategory))
                    cmdText += "AND  BBM.budgetCategoryId = '" + budgetCategory + @"'";
                if (!string.IsNullOrEmpty(budgetSubCategory))
                    cmdText += "and BBM.budgetSubCategoryId = '" + budgetSubCategory + "'";
                if (!string.IsNullOrEmpty(budget))
                    cmdText += "and BBM.budgetId = '" + budget + "'";
                if (!string.IsNullOrEmpty(Activity))
                    cmdText += "AND EVD.ActivityId='" + Activity + @"'";

                cmdText += " ORDER BY  V." + dateType + "";

                return _sqlRepository.GetDataCollection(cmdText);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }
    }
}