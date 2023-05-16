using Library.Core;
using Library.Data;
using Library.Data.Sql;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.ViewModel.Organizations;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

namespace Library.Service.Accounts
{
    public class MISAccountDashboardService : IMISAccountDashboardService
    {
        private readonly ISqlRepository _sqlRepository;

        public MISAccountDashboardService(ISqlRepository sqlRepository)
        {
            _sqlRepository = sqlRepository;
        }

        /// <summary>
        /// Function for get TODATE and FROMDATE Onload
        /// </summary>
        /// <param name="compnayGroupId"></param>
        /// <param name="companyId"></param>
        /// <param name="plantId"></param>
        /// <returns></returns>
        public IEnumerable<object> GetVoucherLatestDate(string compnayGroupId, string companyId, string plantId, string dateType, string itemType)
        {
            var sql = @"SELECT TOP(1) Replace(CONVERT(VARCHAR(11), V." + dateType + @", 106), ' ', '-') AS PostingDate FROM TRN.VoucherDetailCurrency AS VDC
					    LEFT JOIN TRN.VoucherDetail AS VD ON VD.Id=VDC.VoucherDetailId
						LEFT JOIN TRN.Voucher AS V ON V.Id=VDC.VoucherId
						LEFT JOIN MST.BudgetMaster  AS BM ON VD.BudgetMasterId = BM.Id
						LEFT JOIN ORG.Entity AS ENT ON v.EntityId = ENT.Id
					    LEFT JOIN HKP.Budget AS B ON B.Id=BM.BudgetId
                        LEFT JOIN HKP.GLGeneralInfo AS GL ON GL.Id=VD.GLGeneralInfoId
						LEFT JOIN HKP.Activity AS ACT ON ACT.Id = VD.ActivityId
						LEFT JOIN HKP.AccountGroup AS AG ON AG.Id=GL.AccountGroupId
						LEFT JOIN HKP.AccountType AS ACNT ON ACNT.Id=AG.AccountTypeId
                        WHERE ACNT.IsBalanceSheet = " + itemType + " ORDER BY V." + dateType + " DESC";
            return _sqlRepository.GetDataCollection(sql);
        }

        public IEnumerable<object> OrgStructureList(string companyGroupId, string companyId)
        {
            try
            {
                var sql = @"SELECT  DISTINCT StandardName UserName,ISNULL(RType,'position') AS RType,Sequence from [ORG].[StructureRelationship]  where CompanyGroupId='" + companyGroupId + @"'
							and ( CompanyId Is null or CompanyId='" + companyId + @"') and  RType = 'Entity'  order by Sequence";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }

        public IEnumerable<OrgStructureListViewModel> OrgStructureListM(string companyGroupId, string companyId)
        {
            try
            {
                var sql = @"SELECT  DISTINCT StandardName ColumnName,ISNULL(RType,'position') AS RType,Sequence from [ORG].[StructureRelationship]  where CompanyGroupId='" + companyGroupId + @"'
							and ( CompanyId Is null or CompanyId='" + companyId + @"') and  RType = 'Entity'  order by Sequence";
                return _sqlRepository.GetModelCollection<OrgStructureListViewModel>(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }

        public IEnumerable<ComboModel> MISBudgetCategoryCbo(string companyGroupId, string companyId, string plantId, string divisionId, string subDivisionId, string unitId, string fromDate, string toDate)
        {
            var bCId = string.Empty; var bCSId = string.Empty; var bId = string.Empty;
            var company = string.Empty;
            var plant = string.Empty;
            var division = string.Empty;
            var subDivision = string.Empty;
            var unit = string.Empty;

            if (companyId != null && companyId != "" && companyId != "null")
            {
                company = "AND V.CompanyId='" + companyId + "' ";
            }
            else
            {
                company = "";
            }
            if (plantId != null && plantId != "" && plantId != "null")
            {
                plant = "AND V.PlantId='" + plantId + "' ";
            }
            else
            {
                plant = "";
            }
            if (divisionId != null && divisionId != "" && divisionId != "null")
            {
                division = "AND D.Id ='" + divisionId + "' ";
            }
            else
            {
                division = "";
            }
            if (subDivisionId != null && subDivisionId != "" && subDivisionId != "null")
            {
                subDivision = "AND D.Id ='" + subDivisionId + "' ";
            }
            else
            {
                subDivision = "";
            }
            if (unitId != null && unitId != "" && unitId != "null")
            {
                unit = "AND D.Id ='" + subDivisionId + "' ";
            }
            else
            {
                unit = "";
            }

            try
            {
                var _sql = @"SELECT Distinct BC.Id,BC.UserName FROM HKP.BudgetCategory BC
								LEFT JOIN  MST.BudgetMaster BM ON BC.Id = BM.BudgetCategoryId
								LEFT JOIN TRN.VoucherDetail VD ON VD.BudgetMasterId = BM.Id
								LEFT JOIN TRN.Voucher V ON V.Id = VD.VoucherId
								LEFT JOIN ORG.Entity AS E ON E.Id=VD.EntityId
								LEFT JOIN ORG.Division AS D ON D.Id=E.DivisionId
								LEFT JOIN ORG.SubDivision AS SD ON SD.Id=E.SubDivisionId
								LEFT JOIN ORG.Unit AS U ON U.Id=E.UnitId
                        WHERE V.CompanyGroupId='" + companyGroupId + @"' " + company + @" " + plant + @" " + division + @" " + subDivision + @" " + unit + @" " + bCId + @" " + bCSId + @" " + bId + @" and
						CONVERT(DATE,v.PostingDate )  between  CONVERT(DATE,'" + fromDate + @"') and  CONVERT(DATE,'" + toDate + @"')";
                return _sqlRepository.GetCombo(_sql, "Id", "UserName");
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }

        public IEnumerable<object> GetBudgetWiseAmountListElastic(string companyGroupId, string companyId, string plantId, string divisionId, string subDivisionId, string unitId, string budgetMasterId, string Activity, string fromDate, string toDate, string voucherId)
        {
            var bCId = string.Empty; var bCSId = string.Empty; var bId = string.Empty;
            var company = string.Empty;
            var plant = string.Empty;
            var division = string.Empty;
            var subDivision = string.Empty;
            var unit = string.Empty;

            if (companyId != null && companyId != "")
            {
                company = "AND V.CompanyId='" + companyId + "' ";
            }
            else
            {
                company = "";
            }
            if (plantId != null && plantId != "")
            {
                plant = "AND V.PlantId='" + plantId + "' ";
            }
            else
            {
                plant = "";
            }
            if (divisionId != null && divisionId != "")
            {
                division = "AND D.Id ='" + divisionId + "' ";
            }
            else
            {
                division = "";
            }
            if (subDivisionId != null && subDivisionId != "")
            {
                subDivision = "AND D.Id ='" + subDivisionId + "' ";
            }
            else
            {
                subDivision = "";
            }
            if (unitId != null && unitId != "")
            {
                unit = "AND D.Id ='" + subDivisionId + "' ";
            }
            else
            {
                unit = "";
            }

            try
            {
                var cmdText = @"SELECT V.CompanyGroupId,V.CompanyId, V.PlantId, V.Id VoucherId, IVS.Id InventoryIssueId, V.SourceType,  BBM.BudgetCategoryName AS [BudgetCategory],BBM.BudgetSubCategoryName AS [BudgetSubCategory]
						,BBM.BudgetName AS [Budget],vd.DrAmount AS DR,VD.CrAmount AS CR ,IVR.Id InventoryReceiveId ,s.SourceType SalesSourceType   FROM TRN.Voucher AS V
						LEFT JOIN TRN.VoucherDetail AS VD ON VD.VoucherId=V.Id
						LEFT OUTER JOIN(SELECT * FROM TRN.VoucherDetailCurrency )AS VDC ON VDC.VoucherDetailId=VD.Id
						LEFT JOIN (SELECT BM.Id, BC.Id budgetCategoryId,BSC.Id budgetSubCategoryId,B.Id budgetId,BC.UserName AS BudgetCategoryName,BSC.UserName AS BudgetSubCategoryName,B.UserName AS BudgetName FROM MST.BudgetMaster AS BM
                        LEFT JOIN HKP.BudgetCategory AS BC ON BC.Id=BM.BudgetCategoryId
                        LEFT JOIN HKP.BudgetSubCategory AS BSC ON BSC.Id=BM.BudgetSubCategoryId
                        LEFT JOIN HKP.Budget AS B ON B.Id=BM.BudgetId) AS BBM ON BBM.Id=VD.BudgetMasterId
						LEFT JOIN ORG.Entity AS E ON E.Id=VD.EntityId
						LEFT JOIN ORG.Division AS D ON D.Id=E.DivisionId
						LEFT JOIN ORG.SubDivision AS SD ON SD.Id=E.SubDivisionId
						LEFT JOIN ORG.Unit AS U ON U.Id=E.UnitId
						left join trn.InventoryIssue  IVS on IVS.VoucherId = V.Id
						left join trn.InventoryReceive IVR ON IVR.VoucherId = V.Id
						left join TRN.Sales s on s.VoucherId=V.Id

						WHERE VD.VoucherId='" + voucherId + "'";

                cmdText += "ORDER BY vd.DrAmount DESC, vd.CrAmount DESC";
                return _sqlRepository.GetDataCollection(cmdText);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }

        public IEnumerable<object> GetBudgetWisevarianceElastic(string companyGroupId, string companyId, string plantId, string divisionId, string subDivisionId, string unitId, string budgetCategory, string budgetSubCategory, string budget, string Activity, string fromDate, string toDate, string bType, string dateType)
        {
            var bCId = string.Empty; var bCSId = string.Empty; var bId = string.Empty;
            var companyGroup = string.Empty;
            var company = string.Empty;
            var plant = string.Empty;
            var division = string.Empty;
            var subDivision = string.Empty;
            var unit = string.Empty;
            var entCompanyGroup = string.Empty;
            var entCompany = string.Empty;
            var entPlant = string.Empty;
            var entDivision = string.Empty;
            var entSubDivision = string.Empty;
            var entUnit = string.Empty;
            var budgetType = string.Empty;

            if (companyGroupId != null && companyGroupId != "")
            {
                companyGroup = "AND V.CompanyGroupId='" + companyGroupId + "' ";
                entCompanyGroup = "AND ENT.CompanyGroupId='" + companyGroupId + "' ";
            }
            else
            {
                companyGroup = "";
                entCompanyGroup = "";
            }
            if (companyId != null && companyId != "")
            {
                company = "AND V.CompanyId='" + companyId + "' ";
                company = "AND ENT.CompanyId='" + companyId + "' ";
            }
            else
            {
                company = "";
            }
            if (plantId != null && plantId != "")
            {
                plant = "AND ENT.PlantId='" + plantId + "' ";
            }
            else
            {
                plant = "";
            }
            if (divisionId != null && divisionId != "")
            {
                division = "AND ENT.DivisionId ='" + divisionId + "' ";
            }
            else
            {
                division = "";
            }
            if (subDivisionId != null && subDivisionId != "")
            {
                subDivision = "AND ENT.SubDivisionId='" + subDivisionId + "' ";
            }
            else
            {
                subDivision = "";
            }
            if (unitId != null && unitId != "")
            {
                unit = "AND ENT.UnitId ='" + unitId + "' ";
            }
            else
            {
                unit = "";
            }
            if (budgetCategory != null && budgetCategory != "")
            {
                bCId = "AND  BBM.budgetCategoryId = '" + budgetCategory + @"'";
            }
            else
            {
                bCId = "";
            }
            if (budgetSubCategory != null)
            {
                bCSId = "and BBM.budgetSubCategoryId = '" + budgetSubCategory + @"'";
            }
            else
            {
                bCSId = "";
            }
            if (budget != null)
            {
                bId = "and BBM.budgetId = '" + budget + @"'";
            }
            else
            {
                bId = "";
            }
            if (bType == null || bType == "")
            {
                budgetType = "";
            }
            else
            {
                budgetType = "AND IsBalanceSheet =  " + bType + @"";
            }

            try
            {
                var month = Convert.ToDateTime(toDate).Month;
                var year = Convert.ToDateTime(toDate).Year;
                var fromMonth = Convert.ToDateTime(fromDate).Month;
                var fromYear = Convert.ToDateTime(fromDate).Year;
                var fromMonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(Convert.ToInt32(fromMonth));//Month Name from Month No
                var monthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(Convert.ToInt32(month));//Month Name from Month No
                var daysInMonth = DateTime.DaysInMonth(Convert.ToInt32(year), Convert.ToInt32(month));//Number of Days in a month

                var lastDateOfToMonth = daysInMonth + "-" + monthName + "-" + year;
                var firsDateOfFromMonth = 1 + "-" + fromMonthName + "-" + fromYear;

                var cmdText = @"SELECT DISTINCT ABUD.EmployeeId,ABUD.EmployeeName,BBM.IsBalanceSheet,BBM.ReportType, BBM.Id BudgetMasterId
                   ,BBM.BcategorySequence,BBM.budgetCategoryId, BBM.BSubCategorySequence,BBM.budgetSubcategoryId, BBM.ItemSeq,BBM.budgetId ,BBM.BudgetCategoryName,BBM.BudgetSubCategoryName,BBM.BudgetName
                    ,ISNULL(AMT.companyGrpName,'') companyGrpName,ISNULL(AMT.cmpGroupId,'') cmpGroupId,ISNULL(AMT.companyId,'') companyId,ISNULL(AMT.companyName,'') companyName,ISNULL(AMT.plantId,'') plantId,ISNULL(AMT.PlantName,'') PlantName,ISNULL(AMT.divisionId,'') divisionId,ISNULL(AMT.divisionName,'') divisionName
                    ,ISNULL(AMT.subDivId,'') subDivId,ISNULL(AMT.subDivisionName,'') subDivisionName,ISNULL(AMT.unitId,'') unitId,ISNULL(AMT.UnitName,'') UnitName,ISNULL(AMT.EntityId,'') EntityId,ISNULL(AMT.Entity,'') Entity

					--DrAmount and CrAmount should adjust in one column.
					 ,Amount=ISNULL(ABS(CASE WHEN ISNULL(AMT.DRcumulative,0)=0 THEN AMT.CRcumulative ELSE AMT.DRcumulative END),0)
					 ,ExAmount=ISNULL(ABS(CASE WHEN ISNULL((EBUD.ExDrAmount),0)=0 THEN (EBUD.ExCrAmount) ELSE (EBUD.ExDrAmount) END),0)
						,ISNULL(ABUD.ActualAmount,0) AS BudgetAmount

				   --*** WHEN ExpenseAmount Less than BudgetAmount
                        --,ExcessAmount=ISNULL(ABS((CASE WHEN ISNULL((AMT.DRcumulative),0)=0 THEN ISNULL(AMT.CRcumulative ,0) ELSE ISNULL(AMT.DRcumulative,0) END))-ISNULL(ABUD.ActualAmount,0)

					 ,ExcessAmount=ISNULL(ABS(CASE WHEN(CASE WHEN ISNULL((AMT.DRcumulative),0)=0 THEN ISNULL(AMT.CRcumulative ,0) ELSE ISNULL(AMT.DRcumulative,0) END)>ISNULL(ABUD.ActualAmount,0) THEN
									 (CASE WHEN ISNULL((AMT.DRcumulative),0)=0 THEN ISNULL(AMT.CRcumulative ,0) ELSE ISNULL(AMT.DRcumulative,0) END)-ISNULL(ABUD.ActualAmount,0) END),0)

					--*** WHEN ExpenseAmount more than BudgetAmount
					,ShortAmount= ISNULL(ABS(CASE WHEN(CASE WHEN ISNULL((AMT.DRcumulative),0)=0 THEN (AMT.CRcumulative) ELSE (AMT.DRcumulative) END)<(ABUD.ActualAmount) THEN
									 (ABUD.ActualAmount)-(CASE WHEN ISNULL((AMT.DRcumulative),0)=0 THEN (AMT.CRcumulative) ELSE (AMT.DRcumulative) END) END),0)
						,ExceptionPosting = ISNULL(ABS(Case when EXPO.EXPOCrAmount = 0 then EXPO.EXPODrAmount Else Expo.EXPOCrAmount end),0)
					--,EXPO.PeriodName
					FROM   TRN.VoucherDetailCurrency AS VDC
					      LEFT JOIN TRN.VoucherDetail AS VD ON VD.Id=VDC.VoucherDetailId
						  LEFT JOIN TRN.Voucher AS V  ON V.Id=VDC.VoucherId
						  LEFT JOIN ORG.Entity AS ENT ON V.EntityId=ENT.Id
						  LEFT JOIN ORG.CompanyGroup AS cmpGrp ON cmpGrp.Id=V.CompanyGroupId
						  LEFT JOIN ORG.Company AS cmp ON Cmp.Id=ENT.CompanyId
						  LEFT JOIN ORG.Plant AS plant ON plant.Id=ENT.PlantId
						  LEFT JOIN ORG.Division AS div ON div.Id=ENT.DivisionId
						  LEFT JOIN ORG.SubDivision AS subDiv ON subDiv.Id=ENT.SubDivisionId
						  LEFT JOIN ORG.Unit AS Unit ON Unit.Id=ENT.UnitId
					      LEFT JOIN (SELECT BM.Id, BC.Id budgetCategoryId,BSC.Id budgetSubCategoryId,B.Id budgetId,ACT.IsBalanceSheet,BM.ReportType
										,BC.UserName AS BudgetCategoryName,BSC.UserName AS BudgetSubCategoryName
											,B.UserName AS BudgetName
                                              ,Bc.Sequence BcategorySequence, Bsc.Sequence BSubCategorySequence, B.Sequence ItemSeq
                                           FROM MST.BudgetMaster AS BM
					                        LEFT JOIN HKP.BudgetCategory AS BC ON BC.Id=BM.BudgetCategoryId
					                        LEFT JOIN HKP.BudgetSubCategory AS BSC ON BSC.Id=BM.BudgetSubCategoryId
		        	                       LEFT JOIN HKP.Budget AS B ON B.Id=BM.BudgetId
								 LEFT JOIN HKP.GLGeneralInfo AS GL ON GL.Id=BM.GLGeneralInfoId
					   LEFT JOIN HKP.AccountGroup AS AG ON AG.Id=GL.AccountGroupId
					   LEFT JOIN HKP.AccountType AS ACT ON ACT.Id=AG.AccountTypeId
							)  AS BBM ON BBM.Id = VD.BudgetMasterId
					      LEFT JOIN ORG.Entity AS E ON E.Id=VD.EntityId
					      LEFT JOIN ORG.Division AS D ON D.Id=E.DivisionId
					      LEFT JOIN ORG.SubDivision AS SD ON SD.Id=E.SubDivisionId
					      LEFT JOIN ORG.Unit AS U ON U.Id=E.UnitId

						  LEFT outer JOIN (
						  SELECT SUM(VDC.DrAmount) DrAmount,SUM(VDC.CrAmount) CrAmount, BM.Id BudgetMasterId,V.EntityId,ENT.UserName Entity,
                         cmp.Id companyId,cmp.UserName companyName,cmpGrp.Id cmpGroupId,cmpGrp.UserName companyGrpName
                    ,plant.Id plantId,plant.UserName PlantName,div.Id divisionId,div.UserName divisionName,subDiv.Id subDivId, subDiv.UserName subDivisionName
                    ,unit.Id unitId, unit.UserName UnitName,
					sum(CASE WHEN ACT.BalanceType = 'Debit' THEN (sum(VDC.DrAmount)-sum(VDC.CrAmount)) ELSE 0 END) over (partition by GL.Id, VD.BudgetMasterId, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) as DRcumulative,
					sum(CASE WHEN ACT.BalanceType = 'Credit' THEN (sum(VDC.CrAmount)-sum(VDC.DrAmount)) ELSE 0 END) over (partition by GL.Id, VD.BudgetMasterId, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) as CRcumulative
				           FROM
					      TRN.VoucherDetailCurrency AS VDC
					      LEFT JOIN TRN.VoucherDetail AS VD ON VD.Id=VDC.VoucherDetailId
						  LEFT JOIN  TRN.Voucher AS V ON V.Id=VDC.VoucherId
						  LEFT JOIN  MST.BudgetMaster  AS BM ON VD.BudgetMasterId = BM.Id
						  LEFT JOIN ORG.Entity AS ENT ON V.EntityId=ENT.Id
						  LEFT JOIN ORG.CompanyGroup AS cmpGrp ON cmpGrp.Id=V.CompanyGroupId
						  LEFT JOIN ORG.Company AS cmp ON Cmp.Id=ENT.CompanyId
						  LEFT JOIN ORG.Plant AS plant ON plant.Id=ENT.PlantId
						  LEFT JOIN ORG.Division AS div ON div.Id=ENT.DivisionId
						  LEFT JOIN ORG.SubDivision AS subDiv ON subDiv.Id=ENT.SubDivisionId
						  LEFT JOIN ORG.Unit AS Unit ON Unit.Id=ENT.UnitId

						  LEFT JOIN HKP.GLGeneralInfo AS GL ON GL.Id=BM.GLGeneralInfoId
					      LEFT JOIN HKP.AccountGroup AS AG ON AG.Id=GL.AccountGroupId
					      LEFT JOIN HKP.AccountType AS ACT ON ACT.Id=AG.AccountTypeId
							WHERE
							CONVERT(DATE, V." + dateType + @") BETWEEN CONVERT(DATE, '" + fromDate + @"') AND   CONVERT(DATE, '" + toDate + @"') " + companyGroup + @" " + company + @" " + plant + @" " + division + @" " + subDivision + @" " + unit + @" " + bCId + @" " + bCSId + @" " + bId + @" " + budgetType + @"
					        GROUP BY BM.Id,V.EntityId,ENT.UserName,GL.Id,VD.BudgetMasterId,VDC.ParallelCurrencyId,ACT.BalanceType,cmp.Id, cmp.UserName, cmpGrp.Id, cmpGrp.UserName, plant.Id, plant.UserName ,div.Id ,div.UserName
						  ,subDiv.Id , subDiv.UserName,unit.Id, unit.UserName
                           ) AS AMT ON AMT.BudgetMasterId = BBM.Id

							--ExpensesForThePeriod
							LEFT   JOIN (
						 SELECT
					sum(CASE WHEN EACT.BalanceType = 'Debit' THEN (sum(EVDC.DrAmount)-sum(EVDC.CrAmount)) ELSE 0 END)
					over (partition by EGL.Id, EVD.BudgetMasterId, EVDC.ParallelCurrencyId order by EVDC.ParallelCurrencyId) as ExDrAmount,

							sum(CASE WHEN EACT.BalanceType = 'Credit' THEN (sum(EVDC.CrAmount)-sum(EVDC.DrAmount)) ELSE 0 END)
							over (partition by EGL.Id, EVD.BudgetMasterId, EVDC.ParallelCurrencyId order by EVDC.ParallelCurrencyId) as ExCrAmount

							,EVD.BudgetMasterId
							 FROM TRN.VoucherDetailCurrency AS EVDC
							  INNER JOIN TRN.VoucherDetail AS EVD ON EVDC.VoucherDetailId=EVD.Id
					          Inner JOIN TRN.Voucher AS V ON EVD.VoucherId=V.Id
					          LEFT JOIN  MST.BudgetMaster  AS EBM ON EVD.BudgetMasterId = EBM.Id
							  LEFT JOIN HKP.GLGeneralInfo AS EGL ON EGL.Id=EBM.GLGeneralInfoId
							  LEFT JOIN HKP.AccountGroup AS EAG ON EAG.Id=EGL.AccountGroupId
							  LEFT JOIN HKP.AccountType AS EACT ON EACT.Id=EAG.AccountTypeId
						      where CONVERT(DATE, V." + dateType + @")  = CONVERT(DATE,  '" + toDate + @"')

							  GROUP BY EVD.BudgetMasterId,EGL.Id, EVD.BudgetMasterId, EVDC.ParallelCurrencyId,EACT.BalanceType

							--FROM TRN.VoucherDetail AS EVD
							--LEFT JOIN SCS.FiscalYearPeriod AS EFYP ON EFYP.Id=EVD.FiscalYearPeriodId
			                  ) AS EBUD ON EBUD.BudgetMasterId=BBM.Id

							--BudgetForThePeriod--------------
							LEft  JOIN(
                                SELECT  AB.EmployeeId,EI.EmployeeName,ABD.FiscalYearId
	                                ,sum(ABD.StandardAmount)/DATEDIFF(DAY, CONVERT(DATE, '" + firsDateOfFromMonth + @"'), CONVERT(DATE, '" + lastDateOfToMonth + @"')) *DATEDIFF(DAY, CONVERT(DATE, '" + fromDate + @"'), CONVERT(DATE, '" + toDate + @"'))  StandardAmount
	                                ,sum(ABD.ActualAmount)/DATEDIFF(DAY, CONVERT(DATE, '" + firsDateOfFromMonth + @"'), CONVERT(DATE, '" + lastDateOfToMonth + @"')) *DATEDIFF(DAY, CONVERT(DATE, '" + fromDate + @"'), CONVERT(DATE, '" + toDate + @"')) ActualAmount,ABD.BudgetMasterId FROM SCS.FiscalYearPeriod AS FYP
							            LEFT JOIN MST.AnnualBudgetDetail AS ABD ON ABD.FiscalYearPeriodId=FYP.Id
							LEFT JOIN MST.AnnualBudget AS AB ON AB.Id=ABD.AnnualBudgetId
							INNER JOIN EmployeeInformation AS EI ON EI.SystemId=AB.EmployeeId
							WHERE
							  CONVERT(DATE,FYP.StartDate) >= CONVERT(DATE, '" + fromDate + @"') AND  CONVERT(DATE,FYP.EndDate) <= CONVERT(DATE,  '" + toDate + @"')

							  GROUP BY ABD.FiscalYearId,ABD.BudgetMasterId,AB.EmployeeId,EI.EmployeeName
							--SELECT  AB.EmployeeId,EI.EmployeeName,FYP.PeriodName,ABD.FiscalYearId,sum(ABD.StandardAmount)/" + daysInMonth + @"*DAY(CONVERT(DATE, '" + toDate + @"')) StandardAmount,sum(ABD.ActualAmount) ActualAmount,ABD.BudgetMasterId FROM SCS.FiscalYearPeriod AS FYP
							--LEFT JOIN MST.AnnualBudgetDetail AS ABD ON ABD.FiscalYearPeriodId=FYP.Id
							--LEFT JOIN MST.AnnualBudget AS AB ON AB.Id=ABD.AnnualBudgetId
							--INNER JOIN EmployeeInformation AS EI ON EI.SystemId=AB.EmployeeId
							--where CONVERT(DATE, '" + toDate + @"')
							--BETWEEN  CONVERT(DATE,FYP.StartDate) AND  CONVERT(DATE,FYP.EndDate)  GROUP BY ABD.FiscalYearId,ABD.BudgetMasterId,FYP.PeriodName,AB.EmployeeId,EI.EmployeeName
							) AS ABUD ON ABUD.BudgetMasterId=BBM.Id

							--Exception Posting----------
							LEFT OUTER JOIN (
						 SELECT
					      EXPODrAmount= sum(EVDC.DrAmount),   EXPOCrAmount= sum(EVDC.CrAmount),BudgetMasterId
							FROM TRN.VoucherDetailCurrency AS EVDC
							LEFT JOIN TRN.VoucherDetail AS EVD ON EVD.Id=EVDC.VoucherDetailId
							 LEFT JOIN SCS.FiscalYearPeriod AS EFYP ON EFYP.Id=EVD.FiscalYearPeriodId
							WHERE  MONTH(CONVERT(DATE, '" + toDate + @"'))
							=  MONTH(CONVERT(DATE,EVD.AddedDate))
                        AND MONTH(EFYP.EndDate) < MONTH(CONVERT(DATE, '" + toDate + @"')) AND YEAR(EFYP.EndDate) =  YEAR( CONVERT(DATE, '" + toDate + @"'))
							GROUP BY BudgetMasterId

							) AS EXPO ON EXPO.BudgetMasterId=BBM.Id --AND EBUD.PeriodName = EXPO.PeriodName

						WHERE CONVERT(DATE, V." + dateType + @") BETWEEN CONVERT(DATE, '" + fromDate + @"') AND   CONVERT(DATE, '" + toDate + @"')
                         --" + companyGroup + @"
                          -- " + company + @" " + plant + @" " + division + @" " + subDivision + @" " + unit + @"
                            " + bCId + @" " + bCSId + @" " + bId + @" " + budgetType + @" ";

                cmdText += @"GROUP BY BBM.Id ,BBM.BudgetCategoryName,BBM.BudgetSubCategoryName,BBM.BudgetName, AMT.DrAmount,AMT.CrAmount,AMT.CRcumulative,AMT.DRcumulative,	ABUD.ActualAmount,EBUD.ExDrAmount,EBUD.ExCrAmount
								,EXPO.EXPOCrAmount,EXPO.EXPODrAmount,BBM.ReportType,BBM.IsBalanceSheet,AMT.EntityId,AMT.Entity,ABUD.EmployeeId,ABUD.EmployeeName
								,cmp.Id,cmp.UserName,cmpGrp.Id,cmpGrp.UserName,plant.Id ,plant.UserName ,div.Id ,div.UserName ,subDiv.Id , subDiv.UserName
                                ,unit.Id,unit.UserName,BBM.BcategorySequence, BBM.BSubCategorySequence, BBM.ItemSeq,AMT.companyGrpName,AMT.cmpGroupId,AMT.companyId
                                ,BBM.budgetSubCategoryId,BBM.budgetCategoryId,BBM.budgetId,AMT.companyName,AMT.plantId,AMT.PlantName,AMT.divisionId,AMT.divisionName, AMT.subDivId,AMT.subDivisionName,AMT.unitId,AMT.UnitName
                                ORDER BY BBM.BcategorySequence, BBM.BSubCategorySequence, BBM.ItemSeq";

                return _sqlRepository.GetDataCollection(cmdText);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }

        public IEnumerable<object> GetBudgetCategoryWisevarianceElastic(string parameterString, string companyGroupId, string companyId, string plantId, string divisionId, string subDivisionId, string unitId, string budgetCategory, string budgetSubCategory, string budget, string Activity, string fromDate, string toDate, string bType, string dateType)
        {
            var bCId = string.Empty; var bCSId = string.Empty; var bId = string.Empty;
            //var companyGroup = string.Empty;
            //var company = string.Empty;
            //var plant = string.Empty;
            //var division = string.Empty;
            //var subDivision = string.Empty;
            //var unit = string.Empty;
            //var entCompanyGroup = string.Empty;
            //var entCompany = string.Empty;
            //var entPlant = string.Empty;
            //var entDivision = string.Empty;
            //var entSubDivision = string.Empty;
            //var entUnit = string.Empty;
            var budgetType = string.Empty;

            //if (companyGroupId != null && companyGroupId != "")
            //{
            //    companyGroup = "AND V.CompanyGroupId='" + companyGroupId + "' ";
            //    entCompanyGroup = "AND ENT.CompanyGroupId='" + companyGroupId + "' ";
            //}
            //else
            //{
            //    companyGroup = "";
            //    entCompanyGroup = "";
            //}
            //if (companyId != null && companyId != "")
            //{
            //    company = "AND V.CompanyId='" + companyId + "' ";
            //    company = "AND ENT.CompanyId='" + companyId + "' ";
            //}
            //else
            //{
            //    company = "";
            //}
            //if (plantId != null && plantId != "")
            //{
            //    plant = "AND ENT.PlantId='" + plantId + "' ";
            //}
            //else
            //{
            //    plant = "";
            //}
            //if (divisionId != null && divisionId != "")
            //{
            //    division = "AND ENT.DivisionId ='" + divisionId + "' ";
            //}
            //else
            //{
            //    division = "";
            //}
            //if (subDivisionId != null && subDivisionId != "")
            //{
            //    subDivision = "AND ENT.SubDivisionId='" + subDivisionId + "' ";
            //}
            //else
            //{
            //    subDivision = "";
            //}
            //if (unitId != null && unitId != "")
            //{
            //    unit = "AND ENT.UnitId ='" + unitId + "' ";
            //}
            //else
            //{
            //    unit = "";
            //}
            if (budgetCategory != null && budgetCategory != "")
            {
                bCId = "AND  BBM.budgetCategoryId = '" + budgetCategory + @"'";
            }
            else
            {
                bCId = "";
            }
            if (budgetSubCategory != null)
            {
                bCSId = "and BBM.budgetSubCategoryId = '" + budgetSubCategory + @"'";
            }
            else
            {
                bCSId = "";
            }
            if (budget != null)
            {
                bId = "and BBM.budgetId = '" + budget + @"'";
            }
            else
            {
                bId = "";
            }
            if (bType == null || bType == "")
            {
                budgetType = "";
            }
            else
            {
                budgetType = "AND IsBalanceSheet =  " + bType + @"";
            }

            try
            {
                var month = Convert.ToDateTime(toDate).Month;
                var year = Convert.ToDateTime(toDate).Year;
                var fromMonth = Convert.ToDateTime(fromDate).Month;
                var fromYear = Convert.ToDateTime(fromDate).Year;
                var fromMonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(Convert.ToInt32(fromMonth));//Month Name from Month No
                var monthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(Convert.ToInt32(month));//Month Name from Month No
                var daysInMonth = DateTime.DaysInMonth(Convert.ToInt32(year), Convert.ToInt32(month));//Number of Days in a month

                var lastDateOfToMonth = daysInMonth + "-" + monthName + "-" + year;
                var firsDateOfFromMonth = 1 + "-" + fromMonthName + "-" + fromYear;

                var cmdText = @"select distinct budgetCategoryId,BudgetCategoryName,BcategorySequence,sum(Amount) Amount,Sum(ExAmount) ExAmount,Sum(ExcessAmount) ExcessAmount, SUM(ShortAmount) ShortAmount,SUM(ExceptionPosting) ExceptionPosting  from (

                SELECT DISTINCT ABUD.EmployeeId,ABUD.EmployeeName,BBM.IsBalanceSheet,BBM.ReportType, BBM.Id BudgetMasterId
                   ,BBM.BcategorySequence,BBM.budgetCategoryId, BBM.BSubCategorySequence,BBM.budgetSubcategoryId, BBM.ItemSeq,BBM.budgetId ,BBM.BudgetCategoryName,BBM.BudgetSubCategoryName,BBM.BudgetName
                    ,ISNULL(AMT.companyGrpName,'') companyGrpName,ISNULL(AMT.cmpGroupId,'') cmpGroupId,ISNULL(AMT.companyId,'') companyId,ISNULL(AMT.companyName,'') companyName,ISNULL(AMT.plantId,'') plantId,ISNULL(AMT.PlantName,'') PlantName,ISNULL(AMT.divisionId,'') divisionId,ISNULL(AMT.divisionName,'') divisionName
                    ,ISNULL(AMT.subDivId,'') subDivId,ISNULL(AMT.subDivisionName,'') subDivisionName,ISNULL(AMT.unitId,'') unitId,ISNULL(AMT.UnitName,'') UnitName,ISNULL(AMT.EntityId,'') EntityId,ISNULL(AMT.Entity,'') Entity

					--DrAmount and CrAmount should adjust in one column.
					 ,Amount=ISNULL(ABS(CASE WHEN ISNULL(AMT.DRcumulative,0)=0 THEN AMT.CRcumulative ELSE AMT.DRcumulative END),0)
					 ,ExAmount=ISNULL(ABS(CASE WHEN ISNULL((EBUD.ExDrAmount),0)=0 THEN (EBUD.ExCrAmount) ELSE (EBUD.ExDrAmount) END),0)
						,ISNULL(ABUD.ActualAmount,0) AS BudgetAmount

				   --*** WHEN ExpenseAmount Less than BudgetAmount
                        --,ExcessAmount=ISNULL(ABS((CASE WHEN ISNULL((AMT.DRcumulative),0)=0 THEN ISNULL(AMT.CRcumulative ,0) ELSE ISNULL(AMT.DRcumulative,0) END))-ISNULL(ABUD.ActualAmount,0)

					 ,ExcessAmount=ISNULL(ABS(CASE WHEN(CASE WHEN ISNULL((AMT.DRcumulative),0)=0 THEN ISNULL(AMT.CRcumulative ,0) ELSE ISNULL(AMT.DRcumulative,0) END)>ISNULL(ABUD.ActualAmount,0) THEN
									 (CASE WHEN ISNULL((AMT.DRcumulative),0)=0 THEN ISNULL(AMT.CRcumulative ,0) ELSE ISNULL(AMT.DRcumulative,0) END)-ISNULL(ABUD.ActualAmount,0) END),0)

					--*** WHEN ExpenseAmount more than BudgetAmount
					,ShortAmount= ISNULL(ABS(CASE WHEN(CASE WHEN ISNULL((AMT.DRcumulative),0)=0 THEN (AMT.CRcumulative) ELSE (AMT.DRcumulative) END)<(ABUD.ActualAmount) THEN
									 (ABUD.ActualAmount)-(CASE WHEN ISNULL((AMT.DRcumulative),0)=0 THEN (AMT.CRcumulative) ELSE (AMT.DRcumulative) END) END),0)
						,ExceptionPosting = ISNULL(ABS(Case when EXPO.EXPOCrAmount = 0 then EXPO.EXPODrAmount Else Expo.EXPOCrAmount end),0)
					--,EXPO.PeriodName
					FROM   TRN.VoucherDetailCurrency AS VDC
					      LEFT JOIN TRN.VoucherDetail AS VD ON VD.Id=VDC.VoucherDetailId
						  LEFT JOIN TRN.Voucher AS V  ON V.Id=VDC.VoucherId
						  LEFT JOIN ORG.Entity AS ENT ON V.EntityId=ENT.Id
						  LEFT JOIN ORG.CompanyGroup AS cmpGrp ON cmpGrp.Id=V.CompanyGroupId
						  LEFT JOIN ORG.Company AS cmp ON Cmp.Id=ENT.CompanyId
						  LEFT JOIN ORG.Plant AS plant ON plant.Id=ENT.PlantId
						  LEFT JOIN ORG.Division AS div ON div.Id=ENT.DivisionId
						  LEFT JOIN ORG.SubDivision AS subDiv ON subDiv.Id=ENT.SubDivisionId
						  LEFT JOIN ORG.Unit AS Unit ON Unit.Id=ENT.UnitId
					      LEFT JOIN (SELECT BM.Id, BC.Id budgetCategoryId,BSC.Id budgetSubCategoryId,B.Id budgetId,ACT.IsBalanceSheet,BM.ReportType
										,BC.UserName AS BudgetCategoryName,BSC.UserName AS BudgetSubCategoryName
											,B.UserName AS BudgetName
                                              ,Bc.Sequence BcategorySequence, Bsc.Sequence BSubCategorySequence, B.Sequence ItemSeq
                                           FROM MST.BudgetMaster AS BM
					                        LEFT JOIN HKP.BudgetCategory AS BC ON BC.Id=BM.BudgetCategoryId
					                        LEFT JOIN HKP.BudgetSubCategory AS BSC ON BSC.Id=BM.BudgetSubCategoryId
		        	                       LEFT JOIN HKP.Budget AS B ON B.Id=BM.BudgetId
								 LEFT JOIN HKP.GLGeneralInfo AS GL ON GL.Id=BM.GLGeneralInfoId
					   LEFT JOIN HKP.AccountGroup AS AG ON AG.Id=GL.AccountGroupId
					   LEFT JOIN HKP.AccountType AS ACT ON ACT.Id=AG.AccountTypeId
							)  AS BBM ON BBM.Id = VD.BudgetMasterId
					      LEFT JOIN ORG.Entity AS E ON E.Id=VD.EntityId
					      LEFT JOIN ORG.Division AS D ON D.Id=E.DivisionId
					      LEFT JOIN ORG.SubDivision AS SD ON SD.Id=E.SubDivisionId
					      LEFT JOIN ORG.Unit AS U ON U.Id=E.UnitId

						  LEFT outer JOIN (
						  SELECT SUM(VDC.DrAmount) DrAmount,SUM(VDC.CrAmount) CrAmount, BM.Id BudgetMasterId,V.EntityId,ENT.UserName Entity,
                         cmp.Id companyId,cmp.UserName companyName,cmpGrp.Id cmpGroupId,cmpGrp.UserName companyGrpName
                    ,plant.Id plantId,plant.UserName PlantName,div.Id divisionId,div.UserName divisionName,subDiv.Id subDivId, subDiv.UserName subDivisionName
                    ,unit.Id unitId, unit.UserName UnitName,
					sum(CASE WHEN ACT.BalanceType = 'Debit' THEN (sum(VDC.DrAmount)-sum(VDC.CrAmount)) ELSE 0 END) over (partition by GL.Id, VD.BudgetMasterId, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) as DRcumulative,
					sum(CASE WHEN ACT.BalanceType = 'Credit' THEN (sum(VDC.CrAmount)-sum(VDC.DrAmount)) ELSE 0 END) over (partition by GL.Id, VD.BudgetMasterId, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) as CRcumulative
				           FROM
					      TRN.VoucherDetailCurrency AS VDC
					      LEFT JOIN TRN.VoucherDetail AS VD ON VD.Id=VDC.VoucherDetailId
						  LEFT JOIN  TRN.Voucher AS V ON V.Id=VDC.VoucherId
						  LEFT JOIN  MST.BudgetMaster  AS BM ON VD.BudgetMasterId = BM.Id
						  LEFT JOIN ORG.Entity AS ENT ON V.EntityId=ENT.Id
						  LEFT JOIN ORG.CompanyGroup AS cmpGrp ON cmpGrp.Id=V.CompanyGroupId
						  LEFT JOIN ORG.Company AS cmp ON Cmp.Id=ENT.CompanyId
						  LEFT JOIN ORG.Plant AS plant ON plant.Id=ENT.PlantId
						  LEFT JOIN ORG.Division AS div ON div.Id=ENT.DivisionId
						  LEFT JOIN ORG.SubDivision AS subDiv ON subDiv.Id=ENT.SubDivisionId
						  LEFT JOIN ORG.Unit AS Unit ON Unit.Id=ENT.UnitId

						  LEFT JOIN HKP.GLGeneralInfo AS GL ON GL.Id=BM.GLGeneralInfoId
					      LEFT JOIN HKP.AccountGroup AS AG ON AG.Id=GL.AccountGroupId
					      LEFT JOIN HKP.AccountType AS ACT ON ACT.Id=AG.AccountTypeId
							WHERE
							CONVERT(DATE, V." + dateType + @") BETWEEN CONVERT(DATE, '" + fromDate + @"') AND   CONVERT(DATE, '" + toDate + @"')  
					        GROUP BY BM.Id,V.EntityId,ENT.UserName,GL.Id,VD.BudgetMasterId,VDC.ParallelCurrencyId,ACT.BalanceType,cmp.Id, cmp.UserName, cmpGrp.Id, cmpGrp.UserName, plant.Id, plant.UserName ,div.Id ,div.UserName
						  ,subDiv.Id , subDiv.UserName,unit.Id, unit.UserName
                           ) AS AMT ON AMT.BudgetMasterId = BBM.Id AND AMT.EntityId = ENT.Id

							--ExpensesForThePeriod
							LEFT   JOIN (
						 SELECT
					sum(CASE WHEN EACT.BalanceType = 'Debit' THEN (sum(EVDC.DrAmount)-sum(EVDC.CrAmount)) ELSE 0 END)
					over (partition by EGL.Id, EVD.BudgetMasterId, EVDC.ParallelCurrencyId order by EVDC.ParallelCurrencyId) as ExDrAmount,

							sum(CASE WHEN EACT.BalanceType = 'Credit' THEN (sum(EVDC.CrAmount)-sum(EVDC.DrAmount)) ELSE 0 END)
							over (partition by EGL.Id, EVD.BudgetMasterId, EVDC.ParallelCurrencyId order by EVDC.ParallelCurrencyId) as ExCrAmount

							,EVD.BudgetMasterId,V.EntityId
							 FROM TRN.VoucherDetailCurrency AS EVDC
							  INNER JOIN TRN.VoucherDetail AS EVD ON EVDC.VoucherDetailId=EVD.Id
					          Inner JOIN TRN.Voucher AS V ON EVD.VoucherId=V.Id
					          LEFT JOIN  MST.BudgetMaster  AS EBM ON EVD.BudgetMasterId = EBM.Id
							  LEFT JOIN HKP.GLGeneralInfo AS EGL ON EGL.Id=EBM.GLGeneralInfoId
							  LEFT JOIN HKP.AccountGroup AS EAG ON EAG.Id=EGL.AccountGroupId
							  LEFT JOIN HKP.AccountType AS EACT ON EACT.Id=EAG.AccountTypeId
						      where CONVERT(DATE, V." + dateType + @")  = CONVERT(DATE,  '" + toDate + @"')

							  GROUP BY EVD.BudgetMasterId,EGL.Id, EVD.BudgetMasterId, EVDC.ParallelCurrencyId,EACT.BalanceType,V.EntityId

			                  ) AS EBUD ON EBUD.BudgetMasterId=BBM.Id AND EBUD.EntityId = ENT.Id

							--BudgetForThePeriod--------------
							LEft  JOIN(
                                SELECT  AB.EmployeeId,EI.EmployeeName,ABD.FiscalYearId,ABD.EntityId
	                                ,sum(ABD.StandardAmount)/DATEDIFF(DAY, CONVERT(DATE, '" + firsDateOfFromMonth + @"'), CONVERT(DATE, '" + lastDateOfToMonth + @"')) *DATEDIFF(DAY, CONVERT(DATE, '" + fromDate + @"'), CONVERT(DATE, '" + toDate + @"'))  StandardAmount
	                                ,sum(ABD.ActualAmount)/DATEDIFF(DAY, CONVERT(DATE, '" + firsDateOfFromMonth + @"'), CONVERT(DATE, '" + lastDateOfToMonth + @"')) *DATEDIFF(DAY, CONVERT(DATE, '" + fromDate + @"'), CONVERT(DATE, '" + toDate + @"')) ActualAmount,ABD.BudgetMasterId FROM SCS.FiscalYearPeriod AS FYP
							            LEFT JOIN MST.AnnualBudgetDetail AS ABD ON ABD.FiscalYearPeriodId=FYP.Id
							LEFT JOIN MST.AnnualBudget AS AB ON AB.Id=ABD.AnnualBudgetId
							INNER JOIN EmployeeInformation AS EI ON EI.SystemId=AB.EmployeeId
							WHERE
							  CONVERT(DATE,FYP.StartDate) >= CONVERT(DATE, '" + fromDate + @"') AND  CONVERT(DATE,FYP.EndDate) <= CONVERT(DATE,  '" + toDate + @"')

							  GROUP BY ABD.FiscalYearId,ABD.BudgetMasterId,AB.EmployeeId,EI.EmployeeName,ABD.EntityId
							) AS ABUD ON ABUD.BudgetMasterId=BBM.Id AND ABUD.EntityId = ENT.Id

							--Exception Posting----------
							LEFT OUTER JOIN (
						 SELECT
					      EXPODrAmount= sum(EVDC.DrAmount),   EXPOCrAmount= sum(EVDC.CrAmount),BudgetMasterId,EVD.EntityId
							FROM TRN.VoucherDetailCurrency AS EVDC
							LEFT JOIN TRN.VoucherDetail AS EVD ON EVD.Id=EVDC.VoucherDetailId
							 LEFT JOIN SCS.FiscalYearPeriod AS EFYP ON EFYP.Id=EVD.FiscalYearPeriodId
							WHERE  MONTH(CONVERT(DATE, '" + toDate + @"'))
							=  MONTH(CONVERT(DATE,EVD.AddedDate))
                        AND MONTH(EFYP.EndDate) < MONTH(CONVERT(DATE, '" + toDate + @"')) AND YEAR(EFYP.EndDate) =  YEAR( CONVERT(DATE, '" + toDate + @"'))
							GROUP BY BudgetMasterId,EVD.EntityId

							) AS EXPO ON EXPO.BudgetMasterId=BBM.Id AND EXPO.EntityId = ENT.Id

						WHERE CONVERT(DATE, V." + dateType + @") BETWEEN CONVERT(DATE, '" + fromDate + @"') AND   CONVERT(DATE, '" + toDate + @"')
                          " + parameterString + @"
                            " + bCId + @" " + bCSId + @" " + bId + @" " + budgetType + @" ";

                cmdText += @"GROUP BY BBM.Id ,BBM.BudgetCategoryName,BBM.BudgetSubCategoryName,BBM.BudgetName, AMT.DrAmount,AMT.CrAmount,AMT.CRcumulative,AMT.DRcumulative,	ABUD.ActualAmount,EBUD.ExDrAmount,EBUD.ExCrAmount
								,EXPO.EXPOCrAmount,EXPO.EXPODrAmount,BBM.ReportType,BBM.IsBalanceSheet,AMT.EntityId,AMT.Entity,ABUD.EmployeeId,ABUD.EmployeeName
								,cmp.Id,cmp.UserName,cmpGrp.Id,cmpGrp.UserName,plant.Id ,plant.UserName ,div.Id ,div.UserName ,subDiv.Id , subDiv.UserName
                                ,unit.Id,unit.UserName,BBM.BcategorySequence, BBM.BSubCategorySequence, BBM.ItemSeq,AMT.companyGrpName,AMT.cmpGroupId,AMT.companyId
                                ,BBM.budgetSubCategoryId,BBM.budgetCategoryId,BBM.budgetId,AMT.companyName,AMT.plantId,AMT.PlantName,AMT.divisionId,AMT.divisionName, AMT.subDivId,AMT.subDivisionName,AMT.unitId,AMT.UnitName
                               ) BCategory GROUP BY budgetCategoryId,BudgetCategoryName,BcategorySequence
								ORDER BY BcategorySequence";

                return _sqlRepository.GetDataCollection(cmdText);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }

        public IEnumerable<object> GetBudgetSubCategoryWisevarianceElastic(string parameterString, string companyGroupId, string companyId, string plantId, string divisionId, string subDivisionId, string unitId, string budgetCategory, string budgetSubCategory, string budget, string Activity, string fromDate, string toDate, string bType, string dateType)
        {
            var bCId = string.Empty; var bCSId = string.Empty; var bId = string.Empty;
            //var companyGroup = string.Empty;
            //var company = string.Empty;
            //var plant = string.Empty;
            //var division = string.Empty;
            //var subDivision = string.Empty;
            //var unit = string.Empty;
            //var entCompanyGroup = string.Empty;
            //var entCompany = string.Empty;
            //var entPlant = string.Empty;
            //var entDivision = string.Empty;
            //var entSubDivision = string.Empty;
            //var entUnit = string.Empty;
            var budgetType = string.Empty;

            //if (companyGroupId != null && companyGroupId != "")
            //{
            //    companyGroup = "AND V.CompanyGroupId='" + companyGroupId + "' ";
            //    entCompanyGroup = "AND ENT.CompanyGroupId='" + companyGroupId + "' ";
            //}
            //else
            //{
            //    companyGroup = "";
            //    entCompanyGroup = "";
            //}
            //if (companyId != null && companyId != "")
            //{
            //    company = "AND V.CompanyId='" + companyId + "' ";
            //    company = "AND ENT.CompanyId='" + companyId + "' ";
            //}
            //else
            //{
            //    company = "";
            //}
            //if (plantId != null && plantId != "")
            //{
            //    plant = "AND ENT.PlantId='" + plantId + "' ";
            //}
            //else
            //{
            //    plant = "";
            //}
            //if (divisionId != null && divisionId != "")
            //{
            //    division = "AND ENT.DivisionId ='" + divisionId + "' ";
            //}
            //else
            //{
            //    division = "";
            //}
            //if (subDivisionId != null && subDivisionId != "")
            //{
            //    subDivision = "AND ENT.SubDivisionId='" + subDivisionId + "' ";
            //}
            //else
            //{
            //    subDivision = "";
            //}
            //if (unitId != null && unitId != "")
            //{
            //    unit = "AND ENT.UnitId ='" + unitId + "' ";
            //}
            //else
            //{
            //    unit = "";
            //}
            if (budgetCategory != null && budgetCategory != "")
            {
                bCId = "AND  BBM.budgetCategoryId = '" + budgetCategory + @"'";
            }
            else
            {
                bCId = "";
            }
            if (budgetSubCategory != null)
            {
                bCSId = "and BBM.budgetSubCategoryId = '" + budgetSubCategory + @"'";
            }
            else
            {
                bCSId = "";
            }
            if (budget != null)
            {
                bId = "and BBM.budgetId = '" + budget + @"'";
            }
            else
            {
                bId = "";
            }
            if (bType == null || bType == "")
            {
                budgetType = "";
            }
            else
            {
                budgetType = "AND IsBalanceSheet =  " + bType + @"";
            }

            try
            {
                var month = Convert.ToDateTime(toDate).Month;
                var year = Convert.ToDateTime(toDate).Year;
                var fromMonth = Convert.ToDateTime(fromDate).Month;
                var fromYear = Convert.ToDateTime(fromDate).Year;
                var fromMonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(Convert.ToInt32(fromMonth));//Month Name from Month No
                var monthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(Convert.ToInt32(month));//Month Name from Month No
                var daysInMonth = DateTime.DaysInMonth(Convert.ToInt32(year), Convert.ToInt32(month));//Number of Days in a month

                var lastDateOfToMonth = daysInMonth + "-" + monthName + "-" + year;
                var firsDateOfFromMonth = 1 + "-" + fromMonthName + "-" + fromYear;

                var cmdText = @"SELECT budgetCategoryId,budgetSubcategoryId,BudgetSubCategoryName,BSubCategorySequence,SUM(Amount) Amount,Sum(ExAmount) ExAmount,Sum(ExcessAmount) ExcessAmount,SUM(ShortAmount) ShortAmount, sum(ExceptionPosting) ExceptionPosting  from (

               SELECT DISTINCT ABUD.EmployeeId,ABUD.EmployeeName,BBM.IsBalanceSheet,BBM.ReportType, BBM.Id BudgetMasterId
                   ,BBM.BcategorySequence,BBM.budgetCategoryId, BBM.BSubCategorySequence,BBM.budgetSubcategoryId, BBM.ItemSeq,BBM.budgetId ,BBM.BudgetCategoryName,BBM.BudgetSubCategoryName,BBM.BudgetName
                    ,ISNULL(AMT.companyGrpName,'') companyGrpName,ISNULL(AMT.cmpGroupId,'') cmpGroupId,ISNULL(AMT.companyId,'') companyId,ISNULL(AMT.companyName,'') companyName,ISNULL(AMT.plantId,'') plantId,ISNULL(AMT.PlantName,'') PlantName,ISNULL(AMT.divisionId,'') divisionId,ISNULL(AMT.divisionName,'') divisionName
                    ,ISNULL(AMT.subDivId,'') subDivId,ISNULL(AMT.subDivisionName,'') subDivisionName,ISNULL(AMT.unitId,'') unitId,ISNULL(AMT.UnitName,'') UnitName,ISNULL(AMT.EntityId,'') EntityId,ISNULL(AMT.Entity,'') Entity

					--DrAmount and CrAmount should adjust in one column.
					 ,Amount=ISNULL(ABS(CASE WHEN ISNULL(AMT.DRcumulative,0)=0 THEN AMT.CRcumulative ELSE AMT.DRcumulative END),0)
					 ,ExAmount=ISNULL(ABS(CASE WHEN ISNULL((EBUD.ExDrAmount),0)=0 THEN (EBUD.ExCrAmount) ELSE (EBUD.ExDrAmount) END),0)
						,ISNULL(ABUD.ActualAmount,0) AS BudgetAmount

				   --*** WHEN ExpenseAmount Less than BudgetAmount
                        --,ExcessAmount=ISNULL(ABS((CASE WHEN ISNULL((AMT.DRcumulative),0)=0 THEN ISNULL(AMT.CRcumulative ,0) ELSE ISNULL(AMT.DRcumulative,0) END))-ISNULL(ABUD.ActualAmount,0)

					 ,ExcessAmount=ISNULL(ABS(CASE WHEN(CASE WHEN ISNULL((AMT.DRcumulative),0)=0 THEN ISNULL(AMT.CRcumulative ,0) ELSE ISNULL(AMT.DRcumulative,0) END)>ISNULL(ABUD.ActualAmount,0) THEN
									 (CASE WHEN ISNULL((AMT.DRcumulative),0)=0 THEN ISNULL(AMT.CRcumulative ,0) ELSE ISNULL(AMT.DRcumulative,0) END)-ISNULL(ABUD.ActualAmount,0) END),0)

					--*** WHEN ExpenseAmount more than BudgetAmount
					,ShortAmount= ISNULL(ABS(CASE WHEN(CASE WHEN ISNULL((AMT.DRcumulative),0)=0 THEN (AMT.CRcumulative) ELSE (AMT.DRcumulative) END)<(ABUD.ActualAmount) THEN
									 (ABUD.ActualAmount)-(CASE WHEN ISNULL((AMT.DRcumulative),0)=0 THEN (AMT.CRcumulative) ELSE (AMT.DRcumulative) END) END),0)
						,ExceptionPosting = ISNULL(ABS(Case when EXPO.EXPOCrAmount = 0 then EXPO.EXPODrAmount Else Expo.EXPOCrAmount end),0)
					--,EXPO.PeriodName
					FROM   TRN.VoucherDetailCurrency AS VDC
					      LEFT JOIN TRN.VoucherDetail AS VD ON VD.Id=VDC.VoucherDetailId
						  LEFT JOIN TRN.Voucher AS V  ON V.Id=VDC.VoucherId
						  LEFT JOIN ORG.Entity AS ENT ON V.EntityId=ENT.Id
						  LEFT JOIN ORG.CompanyGroup AS cmpGrp ON cmpGrp.Id=V.CompanyGroupId
						  LEFT JOIN ORG.Company AS cmp ON Cmp.Id=ENT.CompanyId
						  LEFT JOIN ORG.Plant AS plant ON plant.Id=ENT.PlantId
						  LEFT JOIN ORG.Division AS div ON div.Id=ENT.DivisionId
						  LEFT JOIN ORG.SubDivision AS subDiv ON subDiv.Id=ENT.SubDivisionId
						  LEFT JOIN ORG.Unit AS Unit ON Unit.Id=ENT.UnitId
					      LEFT JOIN (SELECT BM.Id, BC.Id budgetCategoryId,BSC.Id budgetSubCategoryId,B.Id budgetId,ACT.IsBalanceSheet,BM.ReportType
										,BC.UserName AS BudgetCategoryName,BSC.UserName AS BudgetSubCategoryName
											,B.UserName AS BudgetName
                                              ,Bc.Sequence BcategorySequence, Bsc.Sequence BSubCategorySequence, B.Sequence ItemSeq
                                           FROM MST.BudgetMaster AS BM
					                        LEFT JOIN HKP.BudgetCategory AS BC ON BC.Id=BM.BudgetCategoryId
					                        LEFT JOIN HKP.BudgetSubCategory AS BSC ON BSC.Id=BM.BudgetSubCategoryId
		        	                       LEFT JOIN HKP.Budget AS B ON B.Id=BM.BudgetId
								 LEFT JOIN HKP.GLGeneralInfo AS GL ON GL.Id=BM.GLGeneralInfoId
					   LEFT JOIN HKP.AccountGroup AS AG ON AG.Id=GL.AccountGroupId
					   LEFT JOIN HKP.AccountType AS ACT ON ACT.Id=AG.AccountTypeId
							)  AS BBM ON BBM.Id = VD.BudgetMasterId
					      LEFT JOIN ORG.Entity AS E ON E.Id=VD.EntityId
					      LEFT JOIN ORG.Division AS D ON D.Id=E.DivisionId
					      LEFT JOIN ORG.SubDivision AS SD ON SD.Id=E.SubDivisionId
					      LEFT JOIN ORG.Unit AS U ON U.Id=E.UnitId

						  LEFT outer JOIN (
						  SELECT SUM(VDC.DrAmount) DrAmount,SUM(VDC.CrAmount) CrAmount, BM.Id BudgetMasterId,V.EntityId,ENT.UserName Entity,
                         cmp.Id companyId,cmp.UserName companyName,cmpGrp.Id cmpGroupId,cmpGrp.UserName companyGrpName
                    ,plant.Id plantId,plant.UserName PlantName,div.Id divisionId,div.UserName divisionName,subDiv.Id subDivId, subDiv.UserName subDivisionName
                    ,unit.Id unitId, unit.UserName UnitName,
					sum(CASE WHEN ACT.BalanceType = 'Debit' THEN (sum(VDC.DrAmount)-sum(VDC.CrAmount)) ELSE 0 END) over (partition by GL.Id, VD.BudgetMasterId, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) as DRcumulative,
					sum(CASE WHEN ACT.BalanceType = 'Credit' THEN (sum(VDC.CrAmount)-sum(VDC.DrAmount)) ELSE 0 END) over (partition by GL.Id, VD.BudgetMasterId, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) as CRcumulative
				           FROM
					      TRN.VoucherDetailCurrency AS VDC
					      LEFT JOIN TRN.VoucherDetail AS VD ON VD.Id=VDC.VoucherDetailId
						  LEFT JOIN  TRN.Voucher AS V ON V.Id=VDC.VoucherId
						  LEFT JOIN  MST.BudgetMaster  AS BM ON VD.BudgetMasterId = BM.Id
						  LEFT JOIN ORG.Entity AS ENT ON V.EntityId=ENT.Id
						  LEFT JOIN ORG.CompanyGroup AS cmpGrp ON cmpGrp.Id=V.CompanyGroupId
						  LEFT JOIN ORG.Company AS cmp ON Cmp.Id=ENT.CompanyId
						  LEFT JOIN ORG.Plant AS plant ON plant.Id=ENT.PlantId
						  LEFT JOIN ORG.Division AS div ON div.Id=ENT.DivisionId
						  LEFT JOIN ORG.SubDivision AS subDiv ON subDiv.Id=ENT.SubDivisionId
						  LEFT JOIN ORG.Unit AS Unit ON Unit.Id=ENT.UnitId

						  LEFT JOIN HKP.GLGeneralInfo AS GL ON GL.Id=BM.GLGeneralInfoId
					      LEFT JOIN HKP.AccountGroup AS AG ON AG.Id=GL.AccountGroupId
					      LEFT JOIN HKP.AccountType AS ACT ON ACT.Id=AG.AccountTypeId
							WHERE
							CONVERT(DATE, V." + dateType + @") BETWEEN CONVERT(DATE, '" + fromDate + @"') AND   CONVERT(DATE, '" + toDate + @"') 
					        GROUP BY BM.Id,V.EntityId,ENT.UserName,GL.Id,VD.BudgetMasterId,VDC.ParallelCurrencyId,ACT.BalanceType,cmp.Id, cmp.UserName, cmpGrp.Id, cmpGrp.UserName, plant.Id, plant.UserName ,div.Id ,div.UserName
						  ,subDiv.Id , subDiv.UserName,unit.Id, unit.UserName
                           ) AS AMT ON AMT.BudgetMasterId = BBM.Id AND AMT.EntityId = ENT.Id

							--ExpensesForThePeriod
							LEFT   JOIN (
						 SELECT
					sum(CASE WHEN EACT.BalanceType = 'Debit' THEN (sum(EVDC.DrAmount)-sum(EVDC.CrAmount)) ELSE 0 END)
					over (partition by EGL.Id, EVD.BudgetMasterId, EVDC.ParallelCurrencyId order by EVDC.ParallelCurrencyId) as ExDrAmount,

							sum(CASE WHEN EACT.BalanceType = 'Credit' THEN (sum(EVDC.CrAmount)-sum(EVDC.DrAmount)) ELSE 0 END)
							over (partition by EGL.Id, EVD.BudgetMasterId, EVDC.ParallelCurrencyId order by EVDC.ParallelCurrencyId) as ExCrAmount

							,EVD.BudgetMasterId,V.EntityId
							 FROM TRN.VoucherDetailCurrency AS EVDC
							  INNER JOIN TRN.VoucherDetail AS EVD ON EVDC.VoucherDetailId=EVD.Id
					          Inner JOIN TRN.Voucher AS V ON EVD.VoucherId=V.Id
					          LEFT JOIN  MST.BudgetMaster  AS EBM ON EVD.BudgetMasterId = EBM.Id
							  LEFT JOIN HKP.GLGeneralInfo AS EGL ON EGL.Id=EBM.GLGeneralInfoId
							  LEFT JOIN HKP.AccountGroup AS EAG ON EAG.Id=EGL.AccountGroupId
							  LEFT JOIN HKP.AccountType AS EACT ON EACT.Id=EAG.AccountTypeId
						      where CONVERT(DATE, V." + dateType + @")  = CONVERT(DATE,  '" + toDate + @"')

							  GROUP BY EVD.BudgetMasterId,EGL.Id, EVD.BudgetMasterId, EVDC.ParallelCurrencyId,EACT.BalanceType,V.EntityId

			                  ) AS EBUD ON EBUD.BudgetMasterId=BBM.Id AND EBUD.EntityId = ENT.Id

							--BudgetForThePeriod--------------
							LEft  JOIN(
                                SELECT  AB.EmployeeId,EI.EmployeeName,ABD.FiscalYearId,ABD.EntityId
	                                ,sum(ABD.StandardAmount)/DATEDIFF(DAY, CONVERT(DATE, '" + firsDateOfFromMonth + @"'), CONVERT(DATE, '" + lastDateOfToMonth + @"')) *DATEDIFF(DAY, CONVERT(DATE, '" + fromDate + @"'), CONVERT(DATE, '" + toDate + @"'))  StandardAmount
	                                ,sum(ABD.ActualAmount)/DATEDIFF(DAY, CONVERT(DATE, '" + firsDateOfFromMonth + @"'), CONVERT(DATE, '" + lastDateOfToMonth + @"')) *DATEDIFF(DAY, CONVERT(DATE, '" + fromDate + @"'), CONVERT(DATE, '" + toDate + @"')) ActualAmount,ABD.BudgetMasterId FROM SCS.FiscalYearPeriod AS FYP
							            LEFT JOIN MST.AnnualBudgetDetail AS ABD ON ABD.FiscalYearPeriodId=FYP.Id
							LEFT JOIN MST.AnnualBudget AS AB ON AB.Id=ABD.AnnualBudgetId
							INNER JOIN EmployeeInformation AS EI ON EI.SystemId=AB.EmployeeId
							WHERE
							  CONVERT(DATE,FYP.StartDate) >= CONVERT(DATE, '" + fromDate + @"') AND  CONVERT(DATE,FYP.EndDate) <= CONVERT(DATE,  '" + toDate + @"')

							  GROUP BY ABD.FiscalYearId,ABD.BudgetMasterId,AB.EmployeeId,EI.EmployeeName,ABD.EntityId
							) AS ABUD ON ABUD.BudgetMasterId=BBM.Id AND ABUD.EntityId = ENT.Id

							--Exception Posting----------
							LEFT OUTER JOIN (
						 SELECT
					      EXPODrAmount= sum(EVDC.DrAmount),   EXPOCrAmount= sum(EVDC.CrAmount),BudgetMasterId,EVD.EntityId
							FROM TRN.VoucherDetailCurrency AS EVDC
							LEFT JOIN TRN.VoucherDetail AS EVD ON EVD.Id=EVDC.VoucherDetailId
							 LEFT JOIN SCS.FiscalYearPeriod AS EFYP ON EFYP.Id=EVD.FiscalYearPeriodId
							WHERE  MONTH(CONVERT(DATE, '" + toDate + @"'))
							=  MONTH(CONVERT(DATE,EVD.AddedDate))
                        AND MONTH(EFYP.EndDate) < MONTH(CONVERT(DATE, '" + toDate + @"')) AND YEAR(EFYP.EndDate) =  YEAR( CONVERT(DATE, '" + toDate + @"'))
							GROUP BY BudgetMasterId,EVD.EntityId

							) AS EXPO ON EXPO.BudgetMasterId=BBM.Id AND EXPO.EntityId = ENT.Id

						WHERE CONVERT(DATE, V." + dateType + @") BETWEEN CONVERT(DATE, '" + fromDate + @"') AND   CONVERT(DATE, '" + toDate + @"')
                    
                             " + parameterString + @"
                            " + bCId + @" " + bCSId + @" " + bId + @" " + budgetType + @" ";

                cmdText += @"GROUP BY BBM.Id ,BBM.BudgetCategoryName,BBM.BudgetSubCategoryName,BBM.BudgetName, AMT.DrAmount,AMT.CrAmount,AMT.CRcumulative,AMT.DRcumulative,	ABUD.ActualAmount,EBUD.ExDrAmount,EBUD.ExCrAmount
								,EXPO.EXPOCrAmount,EXPO.EXPODrAmount,BBM.ReportType,BBM.IsBalanceSheet,AMT.EntityId,AMT.Entity,ABUD.EmployeeId,ABUD.EmployeeName
								,cmp.Id,cmp.UserName,cmpGrp.Id,cmpGrp.UserName,plant.Id ,plant.UserName ,div.Id ,div.UserName ,subDiv.Id , subDiv.UserName
                                ,unit.Id,unit.UserName,BBM.BcategorySequence, BBM.BSubCategorySequence, BBM.ItemSeq,AMT.companyGrpName,AMT.cmpGroupId,AMT.companyId
                                ,BBM.budgetSubCategoryId,BBM.budgetCategoryId,BBM.budgetId,AMT.companyName,AMT.plantId,AMT.PlantName,AMT.divisionId,AMT.divisionName, AMT.subDivId,AMT.subDivisionName,AMT.unitId,AMT.UnitName
                               ) BSubCategory	GROUP BY  budgetCategoryId,budgetSubcategoryId,BudgetSubCategoryName,BSubCategorySequence
								ORDER BY BSubCategorySequence";

                return _sqlRepository.GetDataCollection(cmdText);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }


        public IEnumerable<object> GetBudgetItemWisevarianceElastic(string parameterString, string companyGroupId, string companyId, string plantId, string divisionId, string subDivisionId, string unitId, string budgetCategory, string budgetSubCategory, string budget, string Activity, string fromDate, string toDate, string bType, string dateType)
        {
            var bCId = string.Empty; var bCSId = string.Empty; var bId = string.Empty;
            //var companyGroup = string.Empty;
            //var company = string.Empty;
            //var plant = string.Empty;
            //var division = string.Empty;
            //var subDivision = string.Empty;
            //var unit = string.Empty;
            //var entCompanyGroup = string.Empty;
            //var entCompany = string.Empty;
            //var entPlant = string.Empty;
            //var entDivision = string.Empty;
            //var entSubDivision = string.Empty;
            //var entUnit = string.Empty;
            var budgetType = string.Empty;

            //if (companyGroupId != null && companyGroupId != "")
            //{
            //    companyGroup = "AND V.CompanyGroupId='" + companyGroupId + "' ";
            //    entCompanyGroup = "AND ENT.CompanyGroupId='" + companyGroupId + "' ";
            //}
            //else
            //{
            //    companyGroup = "";
            //    entCompanyGroup = "";
            //}
            //if (companyId != null && companyId != "")
            //{
            //    company = "AND V.CompanyId='" + companyId + "' ";
            //    company = "AND ENT.CompanyId='" + companyId + "' ";
            //}
            //else
            //{
            //    company = "";
            //}
            //if (plantId != null && plantId != "")
            //{
            //    plant = "AND ENT.PlantId='" + plantId + "' ";
            //}
            //else
            //{
            //    plant = "";
            //}
            //if (divisionId != null && divisionId != "")
            //{
            //    division = "AND ENT.DivisionId ='" + divisionId + "' ";
            //}
            //else
            //{
            //    division = "";
            //}
            //if (subDivisionId != null && subDivisionId != "")
            //{
            //    subDivision = "AND ENT.SubDivisionId='" + subDivisionId + "' ";
            //}
            //else
            //{
            //    subDivision = "";
            //}
            //if (unitId != null && unitId != "")
            //{
            //    unit = "AND ENT.UnitId ='" + unitId + "' ";
            //}
            //else
            //{
            //    unit = "";
            //}
            if (budgetCategory != null && budgetCategory != "")
            {
                bCId = "AND  BBM.budgetCategoryId = '" + budgetCategory + @"'";
            }
            else
            {
                bCId = "";
            }
            if (budgetSubCategory != null)
            {
                bCSId = "and BBM.budgetSubCategoryId = '" + budgetSubCategory + @"'";
            }
            else
            {
                bCSId = "";
            }
            if (budget != null)
            {
                bId = "and BBM.budgetId = '" + budget + @"'";
            }
            else
            {
                bId = "";
            }
            if (bType == null || bType == "")
            {
                budgetType = "";
            }
            else
            {
                budgetType = "AND IsBalanceSheet =  " + bType + @"";
            }

            try
            {
                var month = Convert.ToDateTime(toDate).Month;
                var year = Convert.ToDateTime(toDate).Year;
                var fromMonth = Convert.ToDateTime(fromDate).Month;
                var fromYear = Convert.ToDateTime(fromDate).Year;
                var fromMonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(Convert.ToInt32(fromMonth));//Month Name from Month No
                var monthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(Convert.ToInt32(month));//Month Name from Month No
                var daysInMonth = DateTime.DaysInMonth(Convert.ToInt32(year), Convert.ToInt32(month));//Number of Days in a month

                var lastDateOfToMonth = daysInMonth + "-" + monthName + "-" + year;
                var firsDateOfFromMonth = 1 + "-" + fromMonthName + "-" + fromYear;

                var cmdText = @"SELECT budgetCategoryId,budgetSubcategoryId,BudgetSubCategoryName,BSubCategorySequence,budgetId ,BudgetName,ItemSeq,SUM(Amount) Amount,Sum(ExAmount) ExAmount,Sum(ExcessAmount) ExcessAmount,SUM(ShortAmount) ShortAmount, sum(ExceptionPosting) ExceptionPosting  from (

                SELECT DISTINCT ABUD.EmployeeId,ABUD.EmployeeName,BBM.IsBalanceSheet,BBM.ReportType, BBM.Id BudgetMasterId
                   ,BBM.BcategorySequence,BBM.budgetCategoryId, BBM.BSubCategorySequence,BBM.budgetSubcategoryId, BBM.ItemSeq,BBM.budgetId ,BBM.BudgetCategoryName,BBM.BudgetSubCategoryName,BBM.BudgetName
                    ,ISNULL(AMT.companyGrpName,'') companyGrpName,ISNULL(AMT.cmpGroupId,'') cmpGroupId,ISNULL(AMT.companyId,'') companyId,ISNULL(AMT.companyName,'') companyName,ISNULL(AMT.plantId,'') plantId,ISNULL(AMT.PlantName,'') PlantName,ISNULL(AMT.divisionId,'') divisionId,ISNULL(AMT.divisionName,'') divisionName
                    ,ISNULL(AMT.subDivId,'') subDivId,ISNULL(AMT.subDivisionName,'') subDivisionName,ISNULL(AMT.unitId,'') unitId,ISNULL(AMT.UnitName,'') UnitName,ISNULL(AMT.EntityId,'') EntityId,ISNULL(AMT.Entity,'') Entity

					--DrAmount and CrAmount should adjust in one column.
					 ,Amount=ISNULL(ABS(CASE WHEN ISNULL(AMT.DRcumulative,0)=0 THEN AMT.CRcumulative ELSE AMT.DRcumulative END),0)
					 ,ExAmount=ISNULL(ABS(CASE WHEN ISNULL((EBUD.ExDrAmount),0)=0 THEN (EBUD.ExCrAmount) ELSE (EBUD.ExDrAmount) END),0)
						,ISNULL(ABUD.ActualAmount,0) AS BudgetAmount

				   --*** WHEN ExpenseAmount Less than BudgetAmount
                        --,ExcessAmount=ISNULL(ABS((CASE WHEN ISNULL((AMT.DRcumulative),0)=0 THEN ISNULL(AMT.CRcumulative ,0) ELSE ISNULL(AMT.DRcumulative,0) END))-ISNULL(ABUD.ActualAmount,0)

					 ,ExcessAmount=ISNULL(ABS(CASE WHEN(CASE WHEN ISNULL((AMT.DRcumulative),0)=0 THEN ISNULL(AMT.CRcumulative ,0) ELSE ISNULL(AMT.DRcumulative,0) END)>ISNULL(ABUD.ActualAmount,0) THEN
									 (CASE WHEN ISNULL((AMT.DRcumulative),0)=0 THEN ISNULL(AMT.CRcumulative ,0) ELSE ISNULL(AMT.DRcumulative,0) END)-ISNULL(ABUD.ActualAmount,0) END),0)

					--*** WHEN ExpenseAmount more than BudgetAmount
					,ShortAmount= ISNULL(ABS(CASE WHEN(CASE WHEN ISNULL((AMT.DRcumulative),0)=0 THEN (AMT.CRcumulative) ELSE (AMT.DRcumulative) END)<(ABUD.ActualAmount) THEN
									 (ABUD.ActualAmount)-(CASE WHEN ISNULL((AMT.DRcumulative),0)=0 THEN (AMT.CRcumulative) ELSE (AMT.DRcumulative) END) END),0)
						,ExceptionPosting = ISNULL(ABS(Case when EXPO.EXPOCrAmount = 0 then EXPO.EXPODrAmount Else Expo.EXPOCrAmount end),0)
					--,EXPO.PeriodName
					FROM   TRN.VoucherDetailCurrency AS VDC
					      LEFT JOIN TRN.VoucherDetail AS VD ON VD.Id=VDC.VoucherDetailId
						  LEFT JOIN TRN.Voucher AS V  ON V.Id=VDC.VoucherId
						  LEFT JOIN ORG.Entity AS ENT ON V.EntityId=ENT.Id
						  LEFT JOIN ORG.CompanyGroup AS cmpGrp ON cmpGrp.Id=V.CompanyGroupId
						  LEFT JOIN ORG.Company AS cmp ON Cmp.Id=ENT.CompanyId
						  LEFT JOIN ORG.Plant AS plant ON plant.Id=ENT.PlantId
						  LEFT JOIN ORG.Division AS div ON div.Id=ENT.DivisionId
						  LEFT JOIN ORG.SubDivision AS subDiv ON subDiv.Id=ENT.SubDivisionId
						  LEFT JOIN ORG.Unit AS Unit ON Unit.Id=ENT.UnitId
					      LEFT JOIN (SELECT BM.Id, BC.Id budgetCategoryId,BSC.Id budgetSubCategoryId,B.Id budgetId,ACT.IsBalanceSheet,BM.ReportType
										,BC.UserName AS BudgetCategoryName,BSC.UserName AS BudgetSubCategoryName
											,B.UserName AS BudgetName
                                              ,Bc.Sequence BcategorySequence, Bsc.Sequence BSubCategorySequence, B.Sequence ItemSeq
                                           FROM MST.BudgetMaster AS BM
					                        LEFT JOIN HKP.BudgetCategory AS BC ON BC.Id=BM.BudgetCategoryId
					                        LEFT JOIN HKP.BudgetSubCategory AS BSC ON BSC.Id=BM.BudgetSubCategoryId
		        	                       LEFT JOIN HKP.Budget AS B ON B.Id=BM.BudgetId
								 LEFT JOIN HKP.GLGeneralInfo AS GL ON GL.Id=BM.GLGeneralInfoId
					   LEFT JOIN HKP.AccountGroup AS AG ON AG.Id=GL.AccountGroupId
					   LEFT JOIN HKP.AccountType AS ACT ON ACT.Id=AG.AccountTypeId
							)  AS BBM ON BBM.Id = VD.BudgetMasterId
					      LEFT JOIN ORG.Entity AS E ON E.Id=VD.EntityId
					      LEFT JOIN ORG.Division AS D ON D.Id=E.DivisionId
					      LEFT JOIN ORG.SubDivision AS SD ON SD.Id=E.SubDivisionId
					      LEFT JOIN ORG.Unit AS U ON U.Id=E.UnitId

						  LEFT outer JOIN (
						  SELECT SUM(VDC.DrAmount) DrAmount,SUM(VDC.CrAmount) CrAmount, BM.Id BudgetMasterId,V.EntityId,ENT.UserName Entity,
                         cmp.Id companyId,cmp.UserName companyName,cmpGrp.Id cmpGroupId,cmpGrp.UserName companyGrpName
                    ,plant.Id plantId,plant.UserName PlantName,div.Id divisionId,div.UserName divisionName,subDiv.Id subDivId, subDiv.UserName subDivisionName
                    ,unit.Id unitId, unit.UserName UnitName,
					sum(CASE WHEN ACT.BalanceType = 'Debit' THEN (sum(VDC.DrAmount)-sum(VDC.CrAmount)) ELSE 0 END) over (partition by GL.Id, VD.BudgetMasterId, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) as DRcumulative,
					sum(CASE WHEN ACT.BalanceType = 'Credit' THEN (sum(VDC.CrAmount)-sum(VDC.DrAmount)) ELSE 0 END) over (partition by GL.Id, VD.BudgetMasterId, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) as CRcumulative
				           FROM
					      TRN.VoucherDetailCurrency AS VDC
					      LEFT JOIN TRN.VoucherDetail AS VD ON VD.Id=VDC.VoucherDetailId
						  LEFT JOIN  TRN.Voucher AS V ON V.Id=VDC.VoucherId
						  LEFT JOIN  MST.BudgetMaster  AS BM ON VD.BudgetMasterId = BM.Id
						  LEFT JOIN ORG.Entity AS ENT ON V.EntityId=ENT.Id
						  LEFT JOIN ORG.CompanyGroup AS cmpGrp ON cmpGrp.Id=V.CompanyGroupId
						  LEFT JOIN ORG.Company AS cmp ON Cmp.Id=ENT.CompanyId
						  LEFT JOIN ORG.Plant AS plant ON plant.Id=ENT.PlantId
						  LEFT JOIN ORG.Division AS div ON div.Id=ENT.DivisionId
						  LEFT JOIN ORG.SubDivision AS subDiv ON subDiv.Id=ENT.SubDivisionId
						  LEFT JOIN ORG.Unit AS Unit ON Unit.Id=ENT.UnitId

						  LEFT JOIN HKP.GLGeneralInfo AS GL ON GL.Id=BM.GLGeneralInfoId
					      LEFT JOIN HKP.AccountGroup AS AG ON AG.Id=GL.AccountGroupId
					      LEFT JOIN HKP.AccountType AS ACT ON ACT.Id=AG.AccountTypeId
							WHERE
							CONVERT(DATE, V." + dateType + @") BETWEEN CONVERT(DATE, '" + fromDate + @"') AND   CONVERT(DATE, '" + toDate + @"') 
					        GROUP BY BM.Id,V.EntityId,ENT.UserName,GL.Id,VD.BudgetMasterId,VDC.ParallelCurrencyId,ACT.BalanceType,cmp.Id, cmp.UserName, cmpGrp.Id, cmpGrp.UserName, plant.Id, plant.UserName ,div.Id ,div.UserName
						  ,subDiv.Id , subDiv.UserName,unit.Id, unit.UserName
                           ) AS AMT ON AMT.BudgetMasterId = BBM.Id AND AMT.EntityId = ENT.Id

							--ExpensesForThePeriod
							LEFT   JOIN (
						 SELECT
					sum(CASE WHEN EACT.BalanceType = 'Debit' THEN (sum(EVDC.DrAmount)-sum(EVDC.CrAmount)) ELSE 0 END)
					over (partition by EGL.Id, EVD.BudgetMasterId, EVDC.ParallelCurrencyId order by EVDC.ParallelCurrencyId) as ExDrAmount,

							sum(CASE WHEN EACT.BalanceType = 'Credit' THEN (sum(EVDC.CrAmount)-sum(EVDC.DrAmount)) ELSE 0 END)
							over (partition by EGL.Id, EVD.BudgetMasterId, EVDC.ParallelCurrencyId order by EVDC.ParallelCurrencyId) as ExCrAmount

							,EVD.BudgetMasterId,V.EntityId
							 FROM TRN.VoucherDetailCurrency AS EVDC
							  INNER JOIN TRN.VoucherDetail AS EVD ON EVDC.VoucherDetailId=EVD.Id
					          Inner JOIN TRN.Voucher AS V ON EVD.VoucherId=V.Id
					          LEFT JOIN  MST.BudgetMaster  AS EBM ON EVD.BudgetMasterId = EBM.Id
							  LEFT JOIN HKP.GLGeneralInfo AS EGL ON EGL.Id=EBM.GLGeneralInfoId
							  LEFT JOIN HKP.AccountGroup AS EAG ON EAG.Id=EGL.AccountGroupId
							  LEFT JOIN HKP.AccountType AS EACT ON EACT.Id=EAG.AccountTypeId
						      where CONVERT(DATE, V." + dateType + @")  = CONVERT(DATE,  '" + toDate + @"')

							  GROUP BY EVD.BudgetMasterId,EGL.Id, EVD.BudgetMasterId, EVDC.ParallelCurrencyId,EACT.BalanceType,V.EntityId

			                  ) AS EBUD ON EBUD.BudgetMasterId=BBM.Id AND EBUD.EntityId = ENT.Id

							--BudgetForThePeriod--------------
							LEft  JOIN(
                                SELECT  AB.EmployeeId,EI.EmployeeName,ABD.FiscalYearId,ABD.EntityId
	                                ,sum(ABD.StandardAmount)/DATEDIFF(DAY, CONVERT(DATE, '" + firsDateOfFromMonth + @"'), CONVERT(DATE, '" + lastDateOfToMonth + @"')) *DATEDIFF(DAY, CONVERT(DATE, '" + fromDate + @"'), CONVERT(DATE, '" + toDate + @"'))  StandardAmount
	                                ,sum(ABD.ActualAmount)/DATEDIFF(DAY, CONVERT(DATE, '" + firsDateOfFromMonth + @"'), CONVERT(DATE, '" + lastDateOfToMonth + @"')) *DATEDIFF(DAY, CONVERT(DATE, '" + fromDate + @"'), CONVERT(DATE, '" + toDate + @"')) ActualAmount,ABD.BudgetMasterId FROM SCS.FiscalYearPeriod AS FYP
							            LEFT JOIN MST.AnnualBudgetDetail AS ABD ON ABD.FiscalYearPeriodId=FYP.Id
							LEFT JOIN MST.AnnualBudget AS AB ON AB.Id=ABD.AnnualBudgetId
							INNER JOIN EmployeeInformation AS EI ON EI.SystemId=AB.EmployeeId
							WHERE
							  CONVERT(DATE,FYP.StartDate) >= CONVERT(DATE, '" + fromDate + @"') AND  CONVERT(DATE,FYP.EndDate) <= CONVERT(DATE,  '" + toDate + @"')

							  GROUP BY ABD.FiscalYearId,ABD.BudgetMasterId,AB.EmployeeId,EI.EmployeeName,ABD.EntityId
							) AS ABUD ON ABUD.BudgetMasterId=BBM.Id AND ABUD.EntityId = ENT.Id

							--Exception Posting----------
							LEFT OUTER JOIN (
						 SELECT
					      EXPODrAmount= sum(EVDC.DrAmount),   EXPOCrAmount= sum(EVDC.CrAmount),BudgetMasterId,EVD.EntityId
							FROM TRN.VoucherDetailCurrency AS EVDC
							LEFT JOIN TRN.VoucherDetail AS EVD ON EVD.Id=EVDC.VoucherDetailId
							 LEFT JOIN SCS.FiscalYearPeriod AS EFYP ON EFYP.Id=EVD.FiscalYearPeriodId
							WHERE  MONTH(CONVERT(DATE, '" + toDate + @"'))
							=  MONTH(CONVERT(DATE,EVD.AddedDate))
                        AND MONTH(EFYP.EndDate) < MONTH(CONVERT(DATE, '" + toDate + @"')) AND YEAR(EFYP.EndDate) =  YEAR( CONVERT(DATE, '" + toDate + @"'))
							GROUP BY BudgetMasterId,EVD.EntityId

							) AS EXPO ON EXPO.BudgetMasterId=BBM.Id AND EXPO.EntityId = ENT.Id

						WHERE CONVERT(DATE, V." + dateType + @") BETWEEN CONVERT(DATE, '" + fromDate + @"') AND   CONVERT(DATE, '" + toDate + @"')
                    " + parameterString + @" " + bCId + @" " + bCSId + @" " + bId + @" " + budgetType + @"";

                cmdText += @"GROUP BY BBM.Id ,BBM.BudgetCategoryName,BBM.BudgetSubCategoryName,BBM.BudgetName, AMT.DrAmount,AMT.CrAmount,AMT.CRcumulative,AMT.DRcumulative,	ABUD.ActualAmount,EBUD.ExDrAmount,EBUD.ExCrAmount
								,EXPO.EXPOCrAmount,EXPO.EXPODrAmount,BBM.ReportType,BBM.IsBalanceSheet,AMT.EntityId,AMT.Entity,ABUD.EmployeeId,ABUD.EmployeeName
								,cmp.Id,cmp.UserName,cmpGrp.Id,cmpGrp.UserName,plant.Id ,plant.UserName ,div.Id ,div.UserName ,subDiv.Id , subDiv.UserName
                                ,unit.Id,unit.UserName,BBM.BcategorySequence, BBM.BSubCategorySequence, BBM.ItemSeq,AMT.companyGrpName,AMT.cmpGroupId,AMT.companyId
                                ,BBM.budgetSubCategoryId,BBM.budgetCategoryId,BBM.budgetId,AMT.companyName,AMT.plantId,AMT.PlantName,AMT.divisionId,AMT.divisionName, AMT.subDivId,AMT.subDivisionName,AMT.unitId,AMT.UnitName
                               ) BudgetItem	GROUP BY  budgetCategoryId,budgetSubcategoryId,BudgetSubCategoryName,BSubCategorySequence,budgetId ,BudgetName,ItemSeq
								ORDER BY ItemSeq";

                return _sqlRepository.GetDataCollection(cmdText);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }

        public IEnumerable<object> GetActivityWisevarianceElastic(string companyGroupId, string companyId, string plantId, string divisionId, string subDivisionId, string unitId, string fromDate, string toDate, string bType, string[] budgetMasterId, string budgetCategoryId, string dateType)
        {
            var bCId = string.Empty; var bCSId = string.Empty; var bId = string.Empty;
            var companyGroup = string.Empty;
            var company = string.Empty;
            var plant = string.Empty;
            var division = string.Empty;
            var subDivision = string.Empty;
            var unit = string.Empty;
            var budgetType = string.Empty;
            var budgetMaster = string.Empty;
            var budgetMasterAmt = string.Empty;

            if (budgetMasterId.Length > 0)
            {
                for (int i = budgetMasterId.Length - 1; i >= 0; i--)
                {
                    //if (entityList[i] == null || entityList[i] == "null")
                    if (budgetMasterId[i] == null)
                    {
                        budgetMaster += "";
                        budgetMasterAmt += "";
                    }
                    else
                    {
                        if (budgetMasterAmt.Length == 0 && budgetMaster.Length == 0)
                        {
                            budgetMaster = "'" + budgetMasterId[i] + "'";
                            budgetMasterAmt = "'" + budgetMasterId[i] + "'";
                        }
                        else
                        {
                            budgetMaster += ",'" + budgetMasterId[i] + "'";
                            budgetMasterAmt += ",'" + budgetMasterId[i] + "'";
                        }
                    }
                }
                if (budgetMaster != "" && budgetMasterAmt != "")
                {
                    budgetMaster = "And BM.Id In(" + budgetMaster + ")";
                    budgetMasterAmt = "And BBM.Id In(" + budgetMasterAmt + ")";
                }
            }

            if (companyGroupId != null && companyGroupId != "")
            {
                companyGroup = "V.CompanyGroupId='" + companyGroupId + "' ";
            }
            else
            {
                companyGroupId = "";
            }

            if (companyId != null && companyId != "")
            {
                company = "AND V.CompanyId='" + companyId + "' ";
            }
            else
            {
                company = "";
            }
            if (plantId != null && plantId != "")
            {
                plant = "AND V.PlantId='" + plantId + "' ";
            }
            else
            {
                plant = "";
            }
            if (divisionId != null && divisionId != "")
            {
                division = "AND ENT.DivisionId ='" + divisionId + "' ";
            }
            else
            {
                division = "";
            }
            if (subDivisionId != null && subDivisionId != "")
            {
                subDivision = "AND ENT.SubDivisionId ='" + subDivisionId + "' ";
            }
            else
            {
                subDivision = "";
            }
            if (unitId != null && unitId != "")
            {
                unit = "AND ENT.UnitId ='" + unitId + "' ";
            }
            else
            {
                unit = "";
            }

            if (bType == null || bType == "")
            {
                budgetType = "";
            }
            else
            {
                budgetType = "AND IsBalanceSheet =  " + bType + @"";
            }

            try
            {
                // DrAmount and CrAmount should adjust in one column.
                var cmdText = @"SELECT DISTINCT ABUD.EmployeeId, ABUD.EmployeeName, BBM.IsBalanceSheet, BBM.ReportType, BBM.Id BudgetMasterId, BBM.BudgetCategoryName
                                , BBM.BudgetSubCategoryName, BBM.BudgetName, AMT.EntityId, AMT.Entity, AMT.Activity, AMT.ActivityId
                                , BBM.CategorySequence,BBM.SubCategorySequence, BBM.BudgetItemSequence
					            , Amount=ABS(CASE WHEN ISNULL(AMT.DRcumulative,0)=0 THEN AMT.CRcumulative ELSE AMT.DRcumulative END)
					            	 ,ExAmount=ABS(CASE WHEN ISNULL((EBUD.ExDrAmount),0)=0 THEN (EBUD.ExCrAmount) ELSE (EBUD.ExDrAmount) END)
						,ISNULL(ABUD.ActualAmount,0) AS BudgetAmount
								--,ExcessAmount=ABS((CASE WHEN ISNULL((AMT.DRcumulative),0)=0 THEN ISNULL(AMT.CRcumulative ,0) ELSE ISNULL(AMT.DRcumulative,0) END))-ISNULL(ABUD.ActualAmount,0)

								--, ExAmount=ABS(CASE WHEN ISNULL((EBUD.ExDrAmount),0)=0 THEN (EBUD.ExCrAmount) ELSE (EBUD.ExDrAmount) END)
						        , ISNULL(ABUD.ActualAmount,0) AS BudgetAmount

					--*** WHEN ExpenseAmount Less than BudgetAmount
					,ExcessAmount=ABS((CASE WHEN ISNULL((AMT.DRcumulative),0)=0 THEN ISNULL(AMT.CRcumulative ,0) ELSE ISNULL(AMT.DRcumulative,0) END))-ISNULL(ABUD.ActualAmount,0)
					--*** WHEN ExpenseAmount more than BudgetAmount
					,ShortAmount=CASE WHEN(CASE WHEN ISNULL((EBUD.ExDrAmount),0)=0 THEN (EBUD.ExCrAmount) ELSE (EBUD.ExDrAmount) END)<(ABUD.ActualAmount) THEN
									 (ABUD.ActualAmount)-(CASE WHEN ISNULL((EBUD.ExDrAmount),0)=0 THEN (EBUD.ExCrAmount) ELSE (EBUD.ExDrAmount) END) END
						,ExceptionPosting = Case when EXPO.EXPOCrAmount = 0 then EXPO.EXPODrAmount Else Expo.EXPOCrAmount END
					--,EXPO.PeriodName
					FROM   TRN.VoucherDetailCurrency AS VDC
					      LEFT JOIN TRN.VoucherDetail AS VD ON VD.Id=VDC.VoucherDetailId
						 LEFT JOIN TRN.Voucher AS V  ON V.Id=VDC.VoucherId
						  LEFT JOIN ORG.Entity AS ENT ON V.EntityId=ENT.Id

					      LEFT JOIN (
						  SELECT BM.Id, BC.Id budgetCategoryId,BSC.Id budgetSubCategoryId,B.Id budgetId,BM.ReportType,ACNT.IsBalanceSheet
									   , BC.UserName AS BudgetCategoryName,BSC.UserName AS BudgetSubCategoryName
                                       , BC.Sequence CategorySequence,BSC.Sequence SubCategorySequence, B.Sequence BudgetItemSequence
									   ,B.UserName AS BudgetName
											FROM MST.BudgetMaster AS BM
					                          LEFT JOIN HKP.BudgetCategory AS BC ON BC.Id=BM.BudgetCategoryId
					                          LEFT JOIN HKP.BudgetSubCategory AS BSC ON BSC.Id=BM.BudgetSubCategoryId
					                          LEFT JOIN HKP.Budget AS B ON B.Id=BM.BudgetId
                                              LEFT JOIN HKP.GLGeneralInfo AS GL ON GL.Id=BM.GLGeneralInfoId
										      LEFT JOIN HKP.AccountGroup AS AG ON AG.Id=GL.AccountGroupId
										      LEFT JOIN HKP.AccountType AS ACNT ON ACNT.Id=AG.AccountTypeId
							)  AS BBM ON BBM.Id = VD.BudgetMasterId
					      LEFT JOIN ORG.Entity AS E ON E.Id=VD.EntityId
					      LEFT JOIN ORG.Division AS D ON D.Id=E.DivisionId
					      LEFT JOIN ORG.SubDivision AS SD ON SD.Id=E.SubDivisionId
					      LEFT JOIN ORG.Unit AS U ON U.Id=E.UnitId

						  LEFT outer JOIN (
						  SELECT SUM(VDC.DrAmount) DrAmount,SUM(VDC.CrAmount) CrAmount, BM.Id BudgetMasterId,V.EntityId,ENT.UserName Entity,ACT.Id ActivityId,Act.UserName Activity,
					SUM(CASE WHEN ACNT.BalanceType = 'Debit' THEN (SUM(VDC.DrAmount)-SUM(VDC.CrAmount)) ELSE 0 END) OVER (PARTITION BY VD.ActivityId, GL.Id, VD.BudgetMasterId, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) as DRcumulative,
					SUM(CASE WHEN ACNT.BalanceType = 'Credit' THEN (SUM(VDC.CrAmount)-SUM(VDC.DrAmount)) ELSE 0 END) OVER (PARTITION BY VD.ActivityId, GL.Id, VD.BudgetMasterId, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) as CRcumulative
				           FROM
					      TRN.VoucherDetailCurrency AS VDC
					      LEFT JOIN TRN.VoucherDetail AS VD ON VD.Id=VDC.VoucherDetailId
						  LEFT JOIN  TRN.Voucher AS V ON V.Id=VDC.VoucherId
						  LEFT JOIN  MST.BudgetMaster  AS BM ON VD.BudgetMasterId = BM.Id
						  LEFT JOIN ORG.Entity AS ENT ON v.EntityId = ENT.Id
					                    LEFT JOIN HKP.Budget AS B ON B.Id=BM.BudgetId
                                        LEFT JOIN HKP.GLGeneralInfo AS GL ON GL.Id=VD.GLGeneralInfoId
										LEFT JOIN HKP.Activity AS ACT ON ACT.Id = VD.ActivityId
										LEFT JOIN HKP.AccountGroup AS AG ON AG.Id=GL.AccountGroupId
										LEFT JOIN HKP.AccountType AS ACNT ON ACNT.Id=AG.AccountTypeId
							WHERE CONVERT(DATE, v." + dateType + @") BETWEEN CONVERT(DATE, '" + fromDate + @"') AND   CONVERT(DATE, '" + toDate + @"') " + budgetType + @"  " + budgetMaster + @"
						  GROUP BY BM.Id,V.EntityId,ENT.UserName,GL.Id,VD.BudgetMasterId,VDC.ParallelCurrencyId,ACNT.BalanceType,VD.ActivityId,ACT.Id,Act.UserName

						  ) AS AMT ON AMT.BudgetMasterId = BBM.Id

							--ExpensesForThePeriod
							LEFT   JOIN (
						 SELECT
					sum(CASE WHEN EACT.BalanceType = 'Debit' THEN (sum(EVDC.DrAmount)-sum(EVDC.CrAmount)) ELSE 0 END)
					over (partition by EVD.ActivityId, EGL.Id, EVD.BudgetMasterId, EVDC.ParallelCurrencyId order by EVDC.ParallelCurrencyId) as ExDrAmount,
							sum(CASE WHEN EACT.BalanceType = 'Credit' THEN (sum(EVDC.CrAmount)-sum(EVDC.DrAmount)) ELSE 0 END)
							over (partition by EVD.ActivityId, EGL.Id, EVD.BudgetMasterId, EVDC.ParallelCurrencyId order by EVDC.ParallelCurrencyId) as ExCrAmount
							,EVD.BudgetMasterId
							 FROM TRN.VoucherDetailCurrency AS EVDC
							  INNER JOIN TRN.VoucherDetail AS EVD ON EVDC.VoucherDetailId=EVD.Id
					          Inner JOIN TRN.Voucher AS V ON EVD.VoucherId=V.Id
					          LEFT JOIN  MST.BudgetMaster  AS EBM ON EVD.BudgetMasterId = EBM.Id
							  LEFT JOIN HKP.GLGeneralInfo AS EGL ON EGL.Id=EBM.GLGeneralInfoId
										LEFT JOIN HKP.Activity AS ACT ON ACT.Id = EVD.ActivityId
							  LEFT JOIN HKP.AccountGroup AS EAG ON EAG.Id=EGL.AccountGroupId
							  LEFT JOIN HKP.AccountType AS EACT ON EACT.Id=EAG.AccountTypeId
						      where CONVERT(DATE,V." + dateType + @") =  CONVERT(DATE, '" + toDate + @"')
							  GROUP BY EVD.BudgetMasterId,EGL.Id, EVD.BudgetMasterId, EVDC.ParallelCurrencyId,EACT.BalanceType ,EVD.ActivityId
							--FROM TRN.VoucherDetail AS EVD
							--LEFT JOIN SCS.FiscalYearPeriod AS EFYP ON EFYP.Id=EVD.FiscalYearPeriodId
			                  ) AS EBUD ON EBUD.BudgetMasterId=BBM.Id
							--BudgetForThePeriod--------------
							LEft  JOIN(
							SELECT  AB.EmployeeId,EI.EmployeeName,FYP.PeriodName,ABD.FiscalYearId,sum(ABD.StandardAmount) StandardAmount,sum(ABD.ActualAmount) ActualAmount,ABD.BudgetMasterId FROM SCS.FiscalYearPeriod AS FYP
							LEFT JOIN MST.AnnualBudgetDetail AS ABD ON ABD.FiscalYearPeriodId=FYP.Id
							LEFT JOIN MST.AnnualBudget AS AB ON AB.Id=ABD.AnnualBudgetId
							INNER JOIN EmployeeInformation AS EI ON EI.SystemId=AB.EmployeeId
							where  CONVERT(DATE, '" + toDate + @"')
							BETWEEN  CONVERT(DATE,FYP.StartDate) AND  CONVERT(DATE,FYP.EndDate)  GROUP BY ABD.FiscalYearId,ABD.BudgetMasterId,FYP.PeriodName,AB.EmployeeId,EI.EmployeeName
							) AS ABUD ON ABUD.BudgetMasterId=BBM.Id
							--Exception Posting----------
							LEFT OUTER JOIN (
						 SELECT
					      EXPODrAmount= sum(EVDC.DrAmount),   EXPOCrAmount= sum(EVDC.CrAmount),BudgetMasterId,EVD.ActivityId
							FROM TRN.VoucherDetailCurrency AS EVDC
							LEFT JOIN TRN.VoucherDetail AS EVD ON EVD.Id=EVDC.VoucherDetailId
							 LEFT JOIN SCS.FiscalYearPeriod AS EFYP ON EFYP.Id=EVD.FiscalYearPeriodId
							WHERE  MONTH( CONVERT(DATE, '" + toDate + @"') )
							=  MONTH(CONVERT(DATE,EVD.AddedDate))
                        AND MONTH(EFYP.EndDate) < MONTH( CONVERT(DATE, '" + toDate + @"') ) AND YEAR(EFYP.EndDate) =  YEAR( CONVERT(DATE, '" + toDate + @"') )
							GROUP BY BudgetMasterId,EVD.ActivityId
							) AS EXPO ON EXPO.BudgetMasterId=BBM.Id AND EXPO.ActivityId = AMT.ActivityId--AND EBUD.PeriodName = EXPO.PeriodName
						WHERE " + companyGroup + @" " + company + @" " + plant + @" " + division + @" " + subDivision + @" " + unit + @" " + bCId + @" " + bCSId + @" " + bId + @" " + budgetType + @"  AND BBM.budgetCategoryId = '" + budgetCategoryId + @"'    " + budgetMasterAmt + @"
							AND CONVERT(DATE, v." + dateType + @") BETWEEN CONVERT(DATE, '" + fromDate + @"') AND   CONVERT(DATE, '" + toDate + @"')
							GROUP BY BBM.Id ,BBM.BudgetCategoryName,BBM.BudgetSubCategoryName,BBM.BudgetName,
						    ABUD.ActualAmount,EBUD.ExDrAmount,EBUD.ExCrAmount
								,AMT.DrAmount,AMT.CrAmount,AMT.CRcumulative,AMT.DRcumulative,EXPO.EXPOCrAmount,EXPO.EXPODrAmount
								,BBM.ReportType,BBM.IsBalanceSheet,AMT.EntityId,AMT.Entity,ABUD.EmployeeId,ABUD.EmployeeName
								,AMT.Activity,AMT.ActivityId,BBM.CategorySequence,BBM.SubCategorySequence, BBM.BudgetItemSequence";
                return _sqlRepository.GetDataCollection(cmdText);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }

        public IEnumerable<object> GetBudgetMasterWiseAmountElastic(string companyGroupId, string companyId, string plantId, string divisionId, string subDivisionId, string unitId, string budgetCategory, string budgetSubCategory, string budget, string activity, string budgetMasterId, string fromDate, string toDate, string dayOrPeriod, string dateType)
        {
            var bCId = string.Empty; var bCSId = string.Empty; var bId = string.Empty;
            var plant = string.Empty;
            var division = string.Empty;
            var subDivision = string.Empty;
            var unit = string.Empty;
            if (dayOrPeriod == "day")
                fromDate = toDate;
            try
            {
                var cmdText = @"SELECT BC.UserName AS [BudgetCategory], BSC.UserName AS [BudgetSubCategory], B.UserName AS [Budget],BC.Id BudgetCategoryId, BSC.Id BudgetSubCategoryId,B.Id ItemId
                                , Amount=SUM(CASE WHEN ISNULL(VDC.DrAmount,0)=0 THEN VDC.CrAmount ELSE VDC.DrAmount END), EFYP.PeriodName PostingPeriod
                                , EFYP.Id PostingPeriodId, EVD.BudgetMasterId, FYPA.PeriodName AS EntryPeriod,FYPA.EntryPeriodId, FYPA.EndDate, FYPA.EndDate
								FROM TRN.VoucherDetailCurrency AS VDC
								LEFT JOIN TRN.VoucherDetail AS EVD ON EVD.Id=VDC.VoucherDetailId
                                LEFT JOIN TRN.Voucher AS V ON V.Id = EVD.VoucherId
								LEFT JOIN SCS.FiscalYearPeriod AS EFYP ON EFYP.Id=EVD.FiscalYearPeriodId
								LEFT JOIN MST.BudgetMaster AS BM ON BM.Id=EVD.BudgetMasterId
								LEFT JOIN HKP.BudgetCategory AS BC ON BC.Id=BM.BudgetCategoryId
								LEFT JOIN HKP.BudgetSubCategory AS BSC ON BSC.Id=BM.BudgetSubCategoryId
								LEFT JOIN HKP.Budget AS B ON B.Id=BM.BudgetId
								LEFT JOIN ORG.Entity AS ENT ON ENT.Id=V.EntityId
								LEFT JOIN (SELECT Id AS EntryPeriodId,PeriodName,StartDate,EndDate FROM SCS.FiscalYearPeriod) AS FYPA ON month(convert(date,FYPA.EndDate))=month(convert(date, evd.AddedDate))
								    AND  YEAR(CONVERT(date, FYPA.EndDate))=year(convert(date, evd.AddedDate))
								WHERE  CONVERT(DATE, V." + dateType + @") BETWEEN CONVERT(DATE, '" + fromDate + @"') AND   CONVERT(DATE, '" + toDate + @"')  ";
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
                    cmdText += "AND  BC.Id = '" + budgetCategory + @"'";
                if (!string.IsNullOrEmpty(budgetSubCategory))
                    cmdText += "and BSC.Id = '" + budgetSubCategory + "'";
                if (!string.IsNullOrEmpty(budget))
                    cmdText += "and B.Id = '" + budget + "'";
                if (!string.IsNullOrEmpty(budgetMasterId))
                {
                    cmdText += "AND BM.Id = '" + budgetMasterId + @"'";
                }
                if (!string.IsNullOrEmpty(activity))
                    cmdText += "AND EVD.ActivityId='" + activity + @"'";

                cmdText += "  GROUP BY EFYP.Id , EFYP.PeriodName, EVD.BudgetMasterId, BC.UserName, BSC.UserName, B.UserName, FYPA.PeriodName, FYPA.EndDate,FYPA.EntryPeriodId, BC.Id, BSC.Id ,B.Id ORDER BY FYPA.EndDate ASC  ";
                return _sqlRepository.GetDataCollection(cmdText);
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
                                    AND EFYP.Id ='" + PostingPeriodId + @"' AND FYPA.EntryPeriodId = '" + EntryPeriodId + @"'

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

        public IEnumerable<object> GetBudgetMasterWiseExceptionAmount(string companyGroupId, string companyId, string plantId, string divisionId, string subDivisionId, string unitId, string budgetCategory, string budgetSubCategory, string budget, string Activity, string[] budgetMasterId, string fromDate, string toDate, string periodName)
        {
            var bCId = string.Empty; var bCSId = string.Empty; var bId = string.Empty;
            var company = string.Empty;
            var plant = string.Empty;
            var division = string.Empty;
            var subDivision = string.Empty;
            var unit = string.Empty;
            var budgetMasterString = string.Empty;
            try
            {
                var cmdText = @"SELECT ExceptionPostingAmount = CASE WHEN EXPO.EXPOCrAmount = 0 THEN EXPO.EXPODrAmount ELSE Expo.EXPOCrAmount END, BudgetMasterId,EntryPeriod,PostingPeriod, [BudgetCategory],[BudgetSubCategory],[Budget],EndDate FROM
								 (SELECT  EXPODrAmount= sum(EVD.DrAmount),   EXPOCrAmount= sum(EVD.CrAmount),BudgetMasterId,EFYP.PeriodName PostingPeriod,FYPA.PeriodName EntryPeriod,BC.UserName AS [BudgetCategory],BC.Id BudgetCategoryId
                                 ,BSC.UserName AS [BudgetSubCategory],BSC.Id budgetubcategoryId,B.UserName AS [Budget],B.Id budgetId,EFYP.EndDate
                                    ,BC.Sequence budgetCategorySequence, BSC.Sequence budgetSubcategorySequence,B.Sequence budgetSequence
									FROM TRN.VoucherDetail AS EVD
										LEFT JOIN TRN.Voucher AS V ON V.Id=EVD.VoucherId
										LEFT JOIN ORG.Entity AS ENT ON V.EntityId = ENT.Id
										LEFT JOIN SCS.FiscalYearPeriod AS EFYP ON EFYP.Id=EVD.FiscalYearPeriodId
										LEFT JOIN MST.BudgetMaster AS BM ON BM.Id=EVD.BudgetMasterId
										LEFT JOIN HKP.BudgetCategory AS BC ON BC.Id=BM.BudgetCategoryId
										LEFT JOIN HKP.BudgetSubCategory AS BSC ON BSC.Id=BM.BudgetSubCategoryId
										LEFT JOIN HKP.Budget AS B ON B.Id=BM.BudgetId
										LEFT OUTER JOIN (SELECT PeriodName,StartDate,EndDate FROM SCS.FiscalYearPeriod  )AS
									    FYPA ON MONTH(CONVERT(DATE,FYPA.EndDate))=MONTH(CONVERT(DATE,evd.AddedDate))
										  AND  YEAR(CONVERT(DATE,FYPA.EndDate))=YEAR(CONVERT(DATE,evd.AddedDate))
							WHERE  MONTH( CONVERT(DATE,'" + toDate + @"'))= MONTH(CONVERT(DATE,EVD.AddedDate))
                        AND MONTH(EFYP.EndDate) < MONTH( CONVERT(DATE,'" + toDate + @"')) AND YEAR(EFYP.EndDate) =  YEAR(CONVERT(DATE,'" + toDate + @"')) AND
								 V.CompanyGroupId = '" + companyGroupId + @"'";

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
                    cmdText += "AND  BC.Id = '" + budgetCategory + @"'";
                if (!string.IsNullOrEmpty(budgetSubCategory))
                    cmdText += "AND BSC.Id = '" + budgetSubCategory + "'";
                if (!string.IsNullOrEmpty(budget))
                    cmdText += "AND B.Id = '" + budget + "'";
                if (!string.IsNullOrEmpty(Activity))
                    cmdText += "AND EVD.ActivityId='" + Activity + @"'";

                if (budgetMasterId.Length > 0)
                {
                    for (int i = budgetMasterId.Length - 1; i >= 0; i--)
                    {
                        //if (entityList[i] == null || entityList[i] == "null")
                        if (budgetMasterId[i] == null)
                        {
                            budgetMasterString += "";
                        }
                        else
                        {
                            if (budgetMasterString.Length == 0)
                            {
                                budgetMasterString = "'" + budgetMasterId[i] + "'";
                            }
                            else
                            {
                                budgetMasterString += ",'" + budgetMasterId[i] + "'";
                            }
                        }
                    }
                    if (budgetMasterString != "")
                    {
                        budgetMasterString = "And BM.Id In(" + budgetMasterString + ")";
                    }
                    cmdText += budgetMasterString;
                }

                cmdText += @"GROUP BY BudgetMasterId,EFYP.PeriodName,FYPA.PeriodName, BC.UserName,BC.Id,BSC.UserName,BSC.Id,B.UserName,B.Id,EFYP.EndDate
                                ,BC.Id,BC.Sequence , BSC.Sequence ,B.Sequence) EXPO ORDER BY budgetCategorySequence,budgetSubcategorySequence,budgetSequence, EndDate";
                return _sqlRepository.GetDataCollection(cmdText);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }

        public IEnumerable<object> GetBudgetMasterWiseExceptionAmountDetail(string companyGroupId, string companyId, string plantId, string divisionId, string subDivisionId, string unitId, string budgetCategory, string budgetSubCategory, string budget, string Activity, string budgetMasterId, string fromDate, string toDate, string periodName)
        {
            var bCId = string.Empty; var bCSId = string.Empty; var bId = string.Empty;
            var company = string.Empty;
            var plant = string.Empty;
            var division = string.Empty;
            var subDivision = string.Empty;
            var unit = string.Empty;

            if (companyId != null && companyId != "")
            {
                company = "AND V.CompanyId='" + companyId + "' ";
            }
            else
            {
                company = "";
            }
            if (plantId != null && plantId != "")
            {
                plant = "AND V.PlantId='" + plantId + "' ";
            }
            else
            {
                plant = "";
            }
            if (divisionId != null && divisionId != "")
            {
                division = "AND D.Id ='" + divisionId + "' ";
            }
            else
            {
                division = "";
            }
            if (subDivisionId != null && subDivisionId != "")
            {
                subDivision = "AND D.Id ='" + subDivisionId + "' ";
            }
            else
            {
                subDivision = "";
            }
            if (unitId != null && unitId != "")
            {
                unit = "AND D.Id ='" + subDivisionId + "' ";
            }
            else
            {
                unit = "";
            }
            if (budgetCategory != null && budgetCategory != "")
            {
                bCId = "AND  BBM.budgetCategoryId = '" + budgetCategory + @"'";
            }
            else
            {
                bCId = "";
            }
            if (budgetSubCategory != null)
            {
                bCSId = "and BBM.budgetSubCategoryId = '" + budgetSubCategory + @"'";
            }
            else
            {
                bCSId = "";
            }
            if (budget != null)
            {
                bId = "and BBM.budgetId = '" + budget + @"'";
            }
            else
            {
                bId = "";
            }
            try
            {
                var CmdText = @"  SELECT EVD.VoucherId,V.VoucherNo,REPLACE(CONVERT(VARCHAR(11),V.VoucherDate, 106), ' ', '-') VoucherDate,REPLACE(CONVERT(VARCHAR(11),V.AddedDate, 106), ' ', '-') EntryDate,REPLACE(CONVERT(VARCHAR(11),V.PostingDate, 106), ' ', '-') PostingDate
								 ,DATEDIFF(DAY, V.AddedDate, V.PostingDate) AS GAP
                                 ,V.Narration ReasonForDelay
                                 ,V.AddedBy
                                 ,EI.EmployeeName budgetResponsiblePerson
								,ExceptionPostingAmount = CASE WHEN EVD.DrAmount = 0 THEN EVD.CrAmount ELSE EVD.DrAmount END, EVD.BudgetMasterId,EFYP.PeriodName PostingPeriod,FYPA.PeriodName EntryPeriod,BC.UserName AS [BudgetCategory],BSC.UserName AS [BudgetSubCategory],B.UserName AS [Budget],EFYP.EndDate
									FROM TRN.VoucherDetail AS EVD
										LEFT JOIN TRN.Voucher AS V ON V.Id=EVD.VoucherId
										LEFT JOIN SCS.FiscalYearPeriod AS EFYP ON EFYP.Id=EVD.FiscalYearPeriodId
										LEFT JOIN MST.BudgetMaster AS BM ON BM.Id=EVD.BudgetMasterId
										LEFT JOIN HKP.BudgetCategory AS BC ON BC.Id=BM.BudgetCategoryId
										LEFT JOIN HKP.BudgetSubCategory AS BSC ON BSC.Id=BM.BudgetSubCategoryId
										LEFT JOIN HKP.Budget AS B ON B.Id=BM.BudgetId
										LEFT JOIN MST.AnnualBudget AS AB ON AB.BudgetMasterId = BM.Id
										LEFT JOIN EmployeeInformation AS EI ON AB.EmployeeId = EI.SystemId
										LEFT OUTER JOIN (SELECT PeriodName,StartDate,EndDate FROM SCS.FiscalYearPeriod  )AS
									    FYPA ON MONTH(CONVERT(DATE,FYPA.EndDate))=MONTH(CONVERT(DATE,evd.AddedDate))
										  AND  YEAR(CONVERT(DATE,FYPA.EndDate))=YEAR(CONVERT(DATE,evd.AddedDate))
							WHERE  MONTH( CONVERT(DATE,'" + toDate + @"'))= MONTH(CONVERT(DATE, EVD.AddedDate))
						AND MONTH(EFYP.EndDate) < MONTH(CONVERT(DATE, '" + toDate + @"')) AND YEAR(EFYP.EndDate) = YEAR(CONVERT(DATE, '" + toDate + @"')) AND
								 V.CompanyGroupId = '" + companyGroupId + @"' AND EVD.BudgetMasterId = '" + budgetMasterId + @"' AND EFYP.PeriodName = '" + periodName + @"' ";

                return _sqlRepository.GetDataCollection(CmdText);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }

        #region Independent Entity Combo  and List

        public IEnumerable<object> GetEntityDetailFromCompanySelection(string companyId)
        {
            var company = string.Empty;
            company = (companyId == null || companyId == "null" || companyId == "") ? "" : "AND CMP.Id = '" + companyId + @"' ";
            var sql = @"SELECT cmp.CompanyGroupId,CG.UserName CompanyGroup, Cmp.UserName Company FROM ORG.Company CMP
						INNER JOIN ORG.CompanyGroup CG ON CMP.CompanyGroupId = CG.Id
							WHERE CMP.Active = 1
							 " + company + @"";
            return _sqlRepository.GetDataCollection(sql);
        }

        public IEnumerable<ComboModel> GetEntityWisePlantCbo(string compnayGroupId, string companyId, string plantId)
        {
            var compnayGroup = string.Empty; var company = string.Empty; var plant = string.Empty;

            compnayGroup = (compnayGroupId == null || compnayGroupId == "null" || compnayGroupId == "") ? "" : "AND ENT.CompanyGroupId = '" + compnayGroupId + @"' ";
            company = (companyId == null || companyId == "null" || companyId == "") ? "" : "AND ENT.CompanyId = '" + companyId + @"' ";
            plant = (plantId == null || plantId == "null" || plantId == "") ? "" : "AND ENT.PlantId = '" + plantId + @"' ";

            var _sql = @" SELECT DISTINCT Plant.UserName, Plant.Id
								FROM ORG.Plant Plant
								INNER JOIN ORG.Entity ENT ON ENT.PlantId = Plant.Id
								WHERE Plant.Active = 1
								" + compnayGroup + @" " + company + @"";
            return _sqlRepository.GetCombo(_sql, "Id", "UserName");
        }

        public IEnumerable<object> GetEntityDetailFromPlantSelection(string plantId)
        {
            var plant = string.Empty;

            plant = (plantId == null || plantId == "null" || plantId == "") ? "" : "AND ENT.PlantId = '" + plantId + @"' ";

            var sql = @"SELECT ENT.CompanyGroupId,CG.UserName CompanyGroup, ENT.CompanyId,cmp.UserName CompanyName,ENT.PlantId,Plant.UserName Plant
							FROM ORG.Plant Plant
							INNER JOIN ORG.Entity ENT ON ENT.PlantId = Plant.Id

								INNER JOIN ORG.CompanyGroup CG ON ENT.CompanyGroupId = CG.Id
								INNER JOIN ORG.Company cmp ON ENT.CompanyId = cmp.Id

							WHERE Plant.Active = 1
							 " + plant + @"";
            return _sqlRepository.GetDataCollection(sql);
        }

        public IEnumerable<object> GetEntityWiseEntityCbo(string[] entityList, string compnayGroupId, string companyId, string plantId, string divisionId, string subDivisionId, string unitId)
        {
            var compnayGroup = string.Empty; var company = string.Empty; var plant = string.Empty; var entity = string.Empty;
            var divison = string.Empty; var subDivision = string.Empty; var unit = string.Empty;

            compnayGroup = (compnayGroupId == null || compnayGroupId == "null" || compnayGroupId == "") ? "" : "  WHERE EN.CompanyGroupId = '" + compnayGroupId + @"' ";
            company = (companyId == null || companyId == "null" || companyId == "") ? "" : "AND EN.CompanyId = '" + companyId + @"' ";
            plant = (plantId == null || plantId == "null" || plantId == "") ? "" : "AND EN.PlantId = '" + plantId + @"' ";
            divison = (divisionId == null || divisionId == "null" || divisionId == "") ? "" : "AND EN.DivisionId = '" + divisionId + @"' ";
            subDivision = (subDivisionId == null || subDivisionId == "null" || subDivisionId == "") ? "" : "AND EN.subDivisionId = '" + subDivisionId + @"' ";
            unit = (unitId == null || unitId == "null" || unitId == "") ? "" : "AND EN.UnitId = '" + unitId + @"' ";

            var entityStr = string.Empty;

            if (entityList != null)
            {
                for (int i = entityList.Length - 1; i >= 0; i--)
                {
                    //if (entityList[i] == null || entityList[i] == "null")
                    if (entityList[i] == null)
                    {
                        entityStr = "";
                    }
                    else
                    {
                        if (entityStr.Length == 0)
                        {
                            entityStr = "'" + entityList[i] + "'";
                        }
                        else
                        {
                            entityStr += ",'" + entityList[i] + "'";
                        }
                    }
                }
                if (entityStr != "")
                {
                    entityStr = "And EN.Id In(" + entityStr + ")";
                }
            }
            var _sql = @"SELECT EN.CompanyGroupId, CG.UserName AS [CompanyGroup], EN.CompanyId, CO.UserName AS [Company]
						,PT.Id plantId,PT.UserName Plant,DV.UserName Division,DV.Id DivisionId,SDV.Id subDivisionId, SDV.UserName subDivision,UN.UserName Unit,UN.Id UnitId,EN.Id entityId,EN.UserName Entity
						FROM ORG.Entity AS EN
						 LEFT OUTER JOIN ORG.CompanyGroup AS CG ON CG.Id=EN.CompanyGroupId
						 LEFT OUTER JOIN ORG.Company AS CO ON CO.Id=EN.CompanyId
						 LEFT OUTER JOIN ORG.Plant AS PT ON PT.Id=EN.PlantId
						 LEFT OUTER JOIN ORG.Division AS DV ON DV.Id=EN.DivisionId
						 LEFT OUTER JOIN ORG.SubDivision AS SDV ON SDV.Id=EN.SubDivisionId
						 LEFT OUTER JOIN ORG.Unit AS UN ON UN.Id=EN.UnitId
					    " + compnayGroup + @"  " + company + @" " + plant + @" " + entityStr + @"";
            return _sqlRepository.GetDataCollection(_sql);
        }

        public IEnumerable<object> GetEntityDetailFromEntitySelection(string entityId)
        {
            var entity = string.Empty;
            entity = (entityId == null || entityId == "null" || entityId == "") ? "" : "AND ENT.Id = '" + entityId + @"' ";

            var sql = @"SELECT ENT.CompanyGroupId,CG.UserName CompanyGroup, ENT.CompanyId,cmp.UserName CompanyName
					,ENT.PlantId,plant.UserName plant,ENT.Id EntityId, ENT.UserName Entity,DIV.Id DivisionId,Div.UserName Division,SubDiv.Id SubDivisionId
					,SubDiv.UserName SubDivision,Unit.Id UnitId, Unit.UserName Unit
							FROM ORG.Entity ENT
							    INNER JOIN ORG.CompanyGroup CG ON ENT.CompanyGroupId = CG.Id
								INNER JOIN ORG.Company cmp ON ENT.CompanyId = cmp.Id
								INNER JOIN ORG.Plant Plant ON ENT.PlantId = Plant.Id
								INNER JOIN ORG.Division DIV ON ENT.DivisionId = DIV.Id
								INNER JOIN ORG.SubDivision SubDiv ON ENT.SubDivisionId = SubDiv.Id
								INNER JOIN ORG.Unit Unit ON ENT.UnitId = Unit.Id
							WHERE ENT.Active = 1
							 " + entity + @"";
            return _sqlRepository.GetDataCollection(sql);
        }

        public IEnumerable<ComboModel> GetEntityWiseDivisionCbo(string compnayGroupId, string companyId, string plantId)
        {
            var compnayGroup = string.Empty; var company = string.Empty; var plant = string.Empty;
            var divison = string.Empty; var subDivision = string.Empty; var unit = string.Empty;

            compnayGroup = (compnayGroupId == null || compnayGroupId == "null" || compnayGroupId == "") ? "" : "AND ENT.CompanyGroupId = '" + compnayGroupId + @"' ";
            company = (companyId == null || companyId == "null" || companyId == "") ? "" : "AND ENT.CompanyId = '" + companyId + @"' ";
            plant = (plantId == null || plantId == "null" || plantId == "") ? "" : "AND ENT.PlantId = '" + plantId + @"' ";

            var _sql = @"SELECT DISTINCT DIV.UserName, Div.Id
								FROM ORG.Division DIV
								INNER JOIN ORG.Entity ENT ON ENT.DivisionId = DIV.Id
								WHERE DIV.Active = 1
								" + compnayGroup + @" " + company + @" " + plant + @"";

            return _sqlRepository.GetCombo(_sql, "Id", "UserName");
        }

        public IEnumerable<object> GetEntityDetailFromDivisionCbo(string compnayGroupId, string companyId, string plantId, string entityId, string divisionId)
        {
            var compnayGroup = string.Empty; var company = string.Empty; var plant = string.Empty; var entity = string.Empty;
            var divison = string.Empty; var subDivision = string.Empty; var unit = string.Empty;
            compnayGroup = (compnayGroupId == null || compnayGroupId == "null" || compnayGroupId == "") ? "" : "AND ENT.CompanyGroupId = '" + compnayGroupId + @"' ";
            company = (companyId == null || companyId == "null" || companyId == "") ? "" : "AND ENT.CompanyId = '" + companyId + @"' ";
            plant = (plantId == null || plantId == "null" || plantId == "") ? "" : "AND ENT.PlantId = '" + plantId + @"' ";
            divison = (divisionId == null || divisionId == "null" || divisionId == "") ? "" : "AND ENT.DivisionId = '" + divisionId + @"' ";
            entity = (entityId == null || entityId == "null" || entityId == "") ? "" : "AND ENT.Id = '" + entityId + @"' ";

            var sql = @"SELECT ENT.CompanyGroupId,CG.UserName CompanyGroup, ENT.CompanyId,cmp.UserName CompanyName,ENT.Id entityId,ENT.UserName Entity,ENT.PlantId,plant.UserName Plant, Div.UserName Division
							FROM ORG.Division DIV
							    INNER JOIN ORG.Entity ENT ON ENT.DivisionId = DIV.Id
								INNER JOIN ORG.CompanyGroup CG ON ENT.CompanyGroupId = CG.Id
								INNER JOIN ORG.Company cmp ON ENT.CompanyId = cmp.Id
								INNER JOIN ORG.plant plant ON ENT.PlantId = plant.Id
							WHERE DIV.Active = 1
							 " + divison + @" " + entity + @"";
            return _sqlRepository.GetDataCollection(sql);
        }

        public IEnumerable<ComboModel> GetEntityWiseSubDivisionCbo(string compnayGroupId, string companyId, string plantId, string divisionId)
        {
            var compnayGroup = string.Empty; var company = string.Empty; var plant = string.Empty;
            var divison = string.Empty; var subDivision = string.Empty; var unit = string.Empty;
            compnayGroup = (compnayGroupId == null || compnayGroupId == "null" || compnayGroupId == "") ? "" : "AND ENT.CompanyGroupId = '" + compnayGroupId + @"' ";
            company = (companyId == null || companyId == "null" || companyId == "") ? "" : "AND ENT.CompanyId = '" + companyId + @"' ";
            plant = (plantId == null || plantId == "null" || plantId == "") ? "" : "AND ENT.PlantId = '" + plantId + @"' ";
            divison = (divisionId == null || divisionId == "null" || divisionId == "") ? "" : "AND ENT.DivisionId = '" + divisionId + @"' ";
            var _sql = @"SELECT DISTINCT SubDIV.UserName, SubDIV.Id
								FROM ORG.SubDivision SubDIV
								INNER JOIN ORG.Entity ENT ON ENT.SubDivisionId = SubDIV.Id
								WHERE SubDIV.Active = 1
								" + compnayGroup + @" " + company + @" " + plant + @" " + divison + @"";
            return _sqlRepository.GetCombo(_sql, "Id", "UserName");
        }

        public IEnumerable<object> GetEntityDetailFromSubDivisionCbo(string compnayGroupId, string companyId, string plantId, string entityId, string divisionId, string subDivisionId)
        {
            var compnayGroup = string.Empty; var company = string.Empty; var plant = string.Empty; var entity = string.Empty;
            var divison = string.Empty; var subDivision = string.Empty; var unit = string.Empty;
            compnayGroup = (compnayGroupId == null || compnayGroupId == "null" || compnayGroupId == "") ? "" : "AND ENT.CompanyGroupId = '" + compnayGroupId + @"' ";
            company = (companyId == null || companyId == "null" || companyId == "") ? "" : "AND ENT.CompanyId = '" + companyId + @"' ";
            plant = (plantId == null || plantId == "null" || plantId == "") ? "" : "AND ENT.PlantId = '" + plantId + @"' ";
            divison = (divisionId == null || divisionId == "null" || divisionId == "") ? "" : "AND ENT.DivisionId = '" + divisionId + @"' ";
            entity = (entityId == null || entityId == "null" || entityId == "") ? "" : "AND ENT.Id = '" + entityId + @"' ";
            subDivision = (subDivisionId == null || subDivisionId == "null" || subDivisionId == "") ? "" : "AND ENT.subDivisionId = '" + subDivisionId + @"' ";
            var sql = @"SELECT ENT.CompanyGroupId,CG.UserName CompanyGroup, ENT.CompanyId,cmp.UserName Company,ENT.Id entityId,ENT.UserName Entity,ENT.PlantId,plant.UserName Plant,ENT.DivisionId,division.UserName Division,
						SubDIV.UserName SubDivision
							FROM ORG.SubDivision SubDIV
								INNER JOIN ORG.Entity ENT ON ENT.SubDivisionId = SubDIV.Id
								INNER JOIN ORG.Unit Unit ON ENT.UnitId = Unit.Id
								INNER JOIN ORG.CompanyGroup CG ON ENT.CompanyGroupId = CG.Id
								INNER JOIN ORG.Company cmp ON ENT.CompanyId = cmp.Id
								INNER JOIN ORG.plant plant ON ENT.PlantId = plant.Id
								INNER JOIN ORG.Division division ON ENT.DivisionId = division.Id
                                WHERE SubDIV.Active = 1
							    " + compnayGroup + @" " + company + @" " + plant + @" " + entity + @" " + divison + @" " + subDivision + @"";
            return _sqlRepository.GetDataCollection(sql);
        }

        public IEnumerable<ComboModel> GetEntityWiseUnitCbo(string compnayGroupId, string companyId, string plantId, string entityId, string divisionId, string subDivisionId)
        {
            var compnayGroup = string.Empty; var company = string.Empty; var plant = string.Empty;
            var divison = string.Empty; var subDivision = string.Empty; var unit = string.Empty;
            var entity = string.Empty;

            compnayGroup = (compnayGroupId == null || compnayGroupId == "null" || compnayGroupId == "") ? "" : "AND ENT.CompanyGroupId = '" + compnayGroupId + @"' ";
            company = (companyId == null || companyId == "null" || companyId == "") ? "" : "AND ENT.CompanyId = '" + companyId + @"' ";
            plant = (plantId == null || plantId == "null" || plantId == "") ? "" : "AND ENT.PlantId = '" + plantId + @"' ";
            divison = (divisionId == null || divisionId == "null" || divisionId == "") ? "" : "AND ENT.DivisionId = '" + divisionId + @"' ";
            subDivision = (subDivisionId == null || subDivisionId == "null" || subDivisionId == "") ? "" : "AND ENT.subDivisionId = '" + subDivisionId + @"' ";
            entity = (entityId == null || entityId == "null" || entityId == "") ? "" : "AND ENT.Id = '" + entityId + @"' ";
            var _sql = @"SELECT DISTINCT Unit.UserName, Unit.Id
								FROM ORG.Unit Unit
								INNER JOIN ORG.Entity ENT ON ENT.UnitId = Unit.Id
								WHERE Unit.Active = 1
								" + compnayGroup + @" " + company + @" " + plant + @" " + entityId + @" " + divison + @" " + subDivision + @"";

            return _sqlRepository.GetCombo(_sql, "Id", "UserName");
        }

        public IEnumerable<object> GetEntityDetailFromUnitCbo(string compnayGroupId, string companyId, string plantId, string entityId, string divisionId, string subDivisionId, string unitId)
        {
            var compnayGroup = string.Empty; var company = string.Empty; var plant = string.Empty; var entity = string.Empty;
            var divison = string.Empty; var subDivision = string.Empty; var unit = string.Empty;

            compnayGroup = (compnayGroupId == null || compnayGroupId == "null" || compnayGroupId == "") ? "" : "AND ENT.CompanyGroupId = '" + compnayGroupId + @"' ";
            company = (companyId == null || companyId == "null" || companyId == "") ? "" : "AND ENT.CompanyId = '" + companyId + @"' ";
            plant = (plantId == null || plantId == "null" || plantId == "") ? "" : "AND ENT.PlantId = '" + plantId + @"' ";
            divison = (divisionId == null || divisionId == "null" || divisionId == "") ? "" : "AND ENT.DivisionId = '" + divisionId + @"' ";
            entity = (entityId == null || entityId == "null" || entityId == "") ? "" : "AND ENT.Id = '" + entityId + @"' ";
            subDivision = (subDivisionId == null || subDivisionId == "null" || subDivisionId == "") ? "" : "AND ENT.subDivisionId = '" + subDivisionId + @"' ";
            unit = (unitId == null || unitId == "null" || unitId == "") ? "" : "AND ENT.UnitId = '" + unitId + @"' ";

            var sql = @"SELECT ENT.CompanyGroupId,CG.UserName CompanyGroup, ENT.CompanyId,cmp.UserName Company,ENT.PlantId,ENT.Id entityId,ENT.UserName Entity,plant.UserName Plant,ENT.DivisionId,division.UserName Division
								,ENT.SubDivisionId,subDivision.UserName subDivision,  Unit.UserName Unit
							FROM ORG.Unit Unit
								INNER JOIN ORG.Entity ENT ON ENT.UnitId = Unit.Id
								INNER JOIN ORG.CompanyGroup CG ON ENT.CompanyGroupId = CG.Id
								INNER JOIN ORG.Company cmp ON ENT.CompanyId = cmp.Id
								INNER JOIN ORG.plant plant ON ENT.PlantId = plant.Id
								INNER JOIN ORG.Division division ON ENT.DivisionId = division.Id
								INNER JOIN ORG.SubDivision subDivision ON ENT.SubDivisionId = subDivision.Id   WHERE Unit.Active = 1
							 " + compnayGroup + @" " + company + @" " + plant + @" " + divison + @" " + subDivision + @" " + unit + @"";
            return _sqlRepository.GetDataCollection(sql);
        }

        #endregion Independent Entity Combo  and List

        #region Item/Budget Responsible person

        public IEnumerable<object> GetItemResponsiblePersonCbo(string compnayGroupId, string companyId, string plantId, string entityId, string divisionId, string subDivisionId, string unitId)
        {
            var compnayGroup = string.Empty; var company = string.Empty; var plant = string.Empty; var entity = string.Empty;
            var divison = string.Empty; var subDivision = string.Empty; var unit = string.Empty;

            compnayGroup = (compnayGroupId == null || compnayGroupId == "null" || compnayGroupId == "") ? "" : "AND ENT.CompanyGroupId = '" + compnayGroupId + @"' ";
            company = (companyId == null || companyId == "null" || companyId == "") ? "" : "AND ENT.CompanyId = '" + companyId + @"' ";
            plant = (plantId == null || plantId == "null" || plantId == "") ? "" : "AND ENT.PlantId = '" + plantId + @"' ";
            divison = (divisionId == null || divisionId == "null" || divisionId == "") ? "" : "AND ENT.DivisionId = '" + divisionId + @"' ";
            entity = (entityId == null || entityId == "null" || entityId == "") ? "" : "AND ENT.Id = '" + entityId + @"' ";
            subDivision = (subDivisionId == null || subDivisionId == "null" || subDivisionId == "") ? "" : "AND ENT.subDivisionId = '" + subDivisionId + @"' ";
            unit = (unitId == null || unitId == "null" || unitId == "") ? "" : "AND ENT.UnitId = '" + unitId + @"' ";

            var sql = @"Select EI.SystemId, EI.EmployeeName FROM EmployeeInformation EI
								INNER JOIN MST.AnnualBudget AB ON EI.SystemId = AB.EmployeeId
								INNER JOIN ORG.CompanyGroup companyGroup ON companyGroup.Id = AB.CompanyGroupId
								INNER JOIN ORG.Company company ON company.Id = AB.CompanyId
								INNER JOIN ORG.Entity ENT ON ENT.Id = AB.EntityId
								INNER JOIN ORG.Plant plant ON plant.Id = ENT.PlantId
								INNER JOIN ORG.Division division ON division.Id = ENT.DivisionId
								INNER JOIN ORG.SubDivision subDivision ON subDivision.Id = ENT.SubDivisionId
								INNER JOIN ORG.Unit unit ON unit.Id = ENT.UnitId
							 " + compnayGroup + @" " + company + @" " + plant + @" " + divison + @" " + subDivision + @" " + unit + @"";
            return _sqlRepository.GetDataCollection(sql);
        }

        #endregion Item/Budget Responsible person

        #region Balance Sheet Tree View
        public IEnumerable<object> GetBalanceSheetInfoGLLevel(string parameterString, string companyGroupId, string companyId, string plantId, string date, string GLGeneralInfoId, string BudgetMasterId, string ActivityId)
        {
            try
            {
                var cmdText = @"select *,CASE WHEN DRcumulative=0 THEN CRcumulative ELSE DRcumulative END Amount
                                FROM (SELECT distinct GL.Id AS AccountCodeId, VDC.ParallelCurrencyId,CU.Code AS CurrencyCode,
								sum(CASE WHEN ACT.BalanceType = 'Debit' THEN (sum(VDC.DrAmount)-sum(VDC.CrAmount)) ELSE 0 END) over (partition by GL.Id,  VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) as DRcumulative
                                , sum(CASE WHEN ACT.BalanceType = 'Credit' THEN (sum(VDC.CrAmount)-sum(VDC.DrAmount)) ELSE 0 END) over (partition by GL.Id, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) as CRcumulative
                                , ACT.BalanceType, ACT.Id AS [MainHead], AG.UserName AS [Level], VD.GLGeneralInfoId,GL.UserName AS GL,GL.AccountCode
	                            FROM TRN.VoucherDetailCurrency AS VDC
		                        INNER JOIN TRN.VoucherDetail AS VD ON VD.Id =VDC.VoucherDetailId
		                        INNER JOIN TRN.Voucher AS V ON V.Id=VD.VoucherId
		                        LEFT OUTER JOIN HKP.GLGeneralInfo AS GL ON GL.Id=VD.GLGeneralInfoId
                                LEFT OUTER JOIN HKP.AccountGroup AS AG ON AG.Id=GL.AccountGroupId
                                LEFT OUTER JOIN [HKP].[AccountType] act on act.Id =AG.AccountTypeId
		                        LEFT OUTER JOIN SCS.Currency AS CU ON CU.Id=VDC.ParallelCurrencyId
                                WHERE act.IsBalanceSheet=1 AND v.PostingDate <='" + date + @"' AND V.CompanyGroupId='" + companyGroupId + @"'
                                AND V.CompanyId='" + companyId + @"' AND V.PlantId='" + plantId + @"'
                                AND V.IsPark=0
                                GROUP BY GL.Id, GL.AccountCode, VDC.ParallelCurrencyId, CU.Code, VD.GLGeneralInfoId, GL.UserName, GL.AccountCode, V.PostingDate, ACT.BalanceType, AG.UserName, ACT.Id

			) AS K where k.DRcumulative<>0  OR 	k.CRcumulative<>0";
                return _sqlRepository.GetDataCollection(cmdText);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }

        public IEnumerable<object> GetBalanceSheetInfoBudgetLevel(string parameterString, string companyGroupId, string companyId, string plantId, string date, string GLGeneralInfoId, string BudgetMasterId, string ActivityId)
        {
            try
            {
                var cmdText = @"select *,CASE WHEN DRcumulative=0 THEN CRcumulative ELSE DRcumulative END Amount
                                FROM (SELECT distinct GL.Id AS AccountCodeId, VDC.ParallelCurrencyId,CU.Code AS CurrencyCode
                                , sum(CASE WHEN ACT.BalanceType = 'Debit' THEN (sum(VDC.DrAmount)-sum(VDC.CrAmount)) ELSE 0 END) over (partition by GL.Id, VD.BudgetMasterId, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) as DRcumulative
                                , sum(CASE WHEN ACT.BalanceType = 'Credit' THEN (sum(VDC.CrAmount)-sum(VDC.DrAmount)) ELSE 0 END) over (partition by GL.Id, VD.BudgetMasterId, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) as CRcumulative
                                , ACT.BalanceType, ACT.Id AS [MainHead], AG.UserName AS [Level], VD.GLGeneralInfoId,GL.UserName AS GL,GL.AccountCode, VD.BudgetMasterId, BM.RefNo+' - '+BUD.UserName AS Budget,VD.GLGeneralInfoId+VD.BudgetMasterId GLGeneralInfoIdBudgetMasterId
	                            FROM TRN.VoucherDetailCurrency AS VDC
		                        INNER JOIN TRN.VoucherDetail AS VD ON VD.Id =VDC.VoucherDetailId
		                        INNER JOIN TRN.Voucher AS V ON V.Id=VD.VoucherId
		                        LEFT OUTER JOIN HKP.GLGeneralInfo AS GL ON GL.Id=VD.GLGeneralInfoId
                                LEFT OUTER JOIN HKP.AccountGroup AS AG ON AG.Id=GL.AccountGroupId
                                left outer join [HKP].[AccountType] act on act.Id =AG.AccountTypeId
		                        LEFT OUTER JOIN SCS.Currency AS CU ON CU.Id=VDC.ParallelCurrencyId
                                LEFT JOIN MST.BudgetMaster BM ON BM.Id=VD.BudgetMasterId
                                LEFT JOIN [HKP].[Budget] AS BUD ON BUD.Id = BM.BudgetId
                                WHERE act.IsBalanceSheet=1 AND v.PostingDate <= '" + date + @"' AND V.CompanyGroupId='" + companyGroupId + @"'
                                AND V.CompanyId='" + companyId + @"' AND V.PlantId='" + plantId + @"'
                                AND V.IsPark=0
                                GROUP BY  GL.Id, GL.AccountCode, VDC.ParallelCurrencyId, CU.Code, VD.GLGeneralInfoId, GL.UserName, GL.AccountCode, V.PostingDate, ACT.BalanceType, AG.UserName, ACT.Id, VD.BudgetMasterId, BM.RefNo, BUD.UserName
) AS K where k.DRcumulative<>0  OR 	k.CRcumulative<>0";
                return _sqlRepository.GetDataCollection(cmdText);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }


        public IEnumerable<object> GetBalanceSheetInfoActivityLevel(string parameterString, string companyGroupId, string companyId, string plantId, string date, string GLGeneralInfoId, string BudgetMasterId, string ActivityId)
        {
            try
            {
                var cmdText = @"select *,CASE WHEN DRcumulative=0 THEN CRcumulative ELSE DRcumulative END Amount
                                FROM (SELECT distinct GL.Id AS AccountCodeId, VDC.ParallelCurrencyId,CU.Code AS CurrencyCode
                              , sum(CASE WHEN ACT.BalanceType = 'Debit' THEN (sum(VDC.DrAmount)-sum(VDC.CrAmount)) ELSE 0 END) over (partition by GL.Id, VD.BudgetMasterId, A.Id, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) as DRcumulative
                                , sum(CASE WHEN ACT.BalanceType = 'Credit' THEN (sum(VDC.CrAmount)-sum(VDC.DrAmount)) ELSE 0 END) over (partition by GL.Id, VD.BudgetMasterId,A.Id, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) as CRcumulative
                                , ACT.BalanceType, ACT.Id AS [MainHead], AG.UserName AS [Level], VD.GLGeneralInfoId,GL.UserName AS GL,GL.AccountCode, VD.BudgetMasterId, BM.RefNo+' - '+BUD.UserName AS Budget
                                , A.UserName AS Activity, A.Id as ActivityId,VD.GLGeneralInfoId+VD.BudgetMasterId GLGeneralInfoIdBudgetMasterId,VD.GLGeneralInfoId+VD.BudgetMasterId+A.Id GLGeneralInfoIdBudgetMasterIdActivityId
	                            FROM TRN.VoucherDetailCurrency AS VDC
		                        INNER JOIN TRN.VoucherDetail AS VD ON VD.Id =VDC.VoucherDetailId
		                        INNER JOIN TRN.Voucher AS V ON V.Id=VD.VoucherId
		                        LEFT OUTER JOIN HKP.GLGeneralInfo AS GL ON GL.Id=VD.GLGeneralInfoId
                                LEFT OUTER JOIN HKP.AccountGroup AS AG ON AG.Id=GL.AccountGroupId
                                left outer join [HKP].[AccountType] act on act.Id =AG.AccountTypeId
		                        LEFT OUTER JOIN SCS.Currency AS CU ON CU.Id=VDC.ParallelCurrencyId
                                LEFT JOIN MST.BudgetMaster BM ON BM.Id=VD.BudgetMasterId
                                LEFT JOIN [HKP].[Budget] AS BUD ON BUD.Id = BM.BudgetId
                                LEFT JOIN HKP.Activity A on VD.ActivityId=A.Id
                                WHERE act.IsBalanceSheet=1 AND v.PostingDate <= '" + date + @"' AND V.CompanyGroupId='" + companyGroupId + @"'
                                AND V.CompanyId='" + companyId + @"' AND V.PlantId='" + plantId + @"'
                                AND V.IsPark=0
                                GROUP BY GL.Id, GL.AccountCode, VDC.ParallelCurrencyId, CU.Code,
VD.GLGeneralInfoId, GL.UserName, GL.AccountCode, V.PostingDate, ACT.BalanceType, AG.UserName, ACT.Id, VD.BudgetMasterId, BM.RefNo, BUD.UserName, A.UserName, A.Id
) AS K where k.DRcumulative<>0  OR 	k.CRcumulative<>0";
                return _sqlRepository.GetDataCollection(cmdText);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }
        public IEnumerable<object> GetBalanceSheetInfoVoucherLevel(string companyGroupId, string companyId, string plantId, string date, string GLGeneralInfoId, string BudgetMasterId, string ActivityId)
        {
            try
            {
                var cmdText = @"select *,CASE WHEN DRcumulative=0 THEN CRcumulative ELSE DRcumulative END Amount
                                FROM (SELECT distinct GL.Id AS AccountCodeId, VDC.ParallelCurrencyId,CU.Code AS CurrencyCode
                                , sum(CASE WHEN ACT.BalanceType = 'Debit' THEN (sum(VDC.DrAmount)-sum(VDC.CrAmount)) ELSE 0 END) over (partition by GL.Id, VD.BudgetMasterId, A.Id, VDC.ParallelCurrencyId,VD.VoucherId order by VDC.ParallelCurrencyId) as DRcumulative
                                , sum(CASE WHEN ACT.BalanceType = 'Credit' THEN (sum(VDC.CrAmount)-sum(VDC.DrAmount)) ELSE 0 END) over (partition by GL.Id, VD.BudgetMasterId,A.Id, VDC.ParallelCurrencyId,VD.VoucherId order by VDC.ParallelCurrencyId) as CRcumulative
                                , ACT.BalanceType, ACT.Id AS [MainHead], AG.UserName AS [Level], VD.GLGeneralInfoId,GL.UserName AS GL,GL.AccountCode, VD.BudgetMasterId, BM.RefNo+' - '+BUD.UserName AS Budget
                                , A.UserName AS Activity, A.Id as ActivityId,VD.GLGeneralInfoId+VD.BudgetMasterId GLGeneralInfoIdBudgetMasterId,VD.GLGeneralInfoId+VD.BudgetMasterId+A.Id GLGeneralInfoIdBudgetMasterIdActivityId
                                ,v.CompanyGroupId,v.CompanyId,v.PlantId,V.SourceType,VD.VoucherId,V.VoucherNo,REPLACE(CONVERT(VARCHAR(11),V.VoucherDate, 106), ' ', '-') VoucherDate
                                ,REPLACE(CONVERT(VARCHAR(11),V.PostingDate, 106), ' ', '-') PostingDate
	                            FROM TRN.VoucherDetailCurrency AS VDC
		                        INNER JOIN TRN.VoucherDetail AS VD ON VD.Id =VDC.VoucherDetailId
		                        INNER JOIN TRN.Voucher AS V ON V.Id=VD.VoucherId
		                        LEFT OUTER JOIN HKP.GLGeneralInfo AS GL ON GL.Id=VD.GLGeneralInfoId
                                LEFT OUTER JOIN HKP.AccountGroup AS AG ON AG.Id=GL.AccountGroupId
                                left outer join [HKP].[AccountType] act on act.Id =AG.AccountTypeId
		                        LEFT OUTER JOIN SCS.Currency AS CU ON CU.Id=VDC.ParallelCurrencyId
                                LEFT JOIN MST.BudgetMaster BM ON BM.Id=VD.BudgetMasterId
                                LEFT JOIN [HKP].[Budget] AS BUD ON BUD.Id = BM.BudgetId
                                LEFT JOIN HKP.Activity A on VD.ActivityId=A.Id
                                WHERE act.IsBalanceSheet=1 AND v.PostingDate <= '" + date + @"' AND V.CompanyGroupId='" + companyGroupId + @"'
                                AND V.CompanyId='" + companyId + @"' AND V.PlantId='" + plantId + @"' AND VD.GLGeneralInfoId='" + GLGeneralInfoId + @"' AND VD.BudgetMasterId='" + BudgetMasterId + @"' AND VD.ActivityId='" + ActivityId + @"'
                                AND V.IsPark=0
                                GROUP BY GL.Id, GL.AccountCode, VDC.ParallelCurrencyId, CU.Code,
                                VD.GLGeneralInfoId, GL.UserName, GL.AccountCode, V.PostingDate, ACT.BalanceType, AG.UserName, ACT.Id, VD.BudgetMasterId, BM.RefNo, BUD.UserName, A.UserName, A.Id
                                ,v.CompanyGroupId,v.CompanyId,v.PlantId,V.SourceType,VD.VoucherId,V.VoucherNo,V.VoucherDate,V.PostingDate
                                ) AS K where k.DRcumulative<>0  OR 	k.CRcumulative<>0";
                return _sqlRepository.GetDataCollection(cmdText);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }

        #endregion
    }
}