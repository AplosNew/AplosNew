using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Service.Enums;
using Library.Service.Helpers;
using Library.Service.Logs;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Threading;

namespace Library.Accounting.Accounts
{
    public class TaxReportService
    {
        private readonly ISqlRepository _sqlRepository;

        public TaxReportService(ISqlRepository sqlRepository)
        {
            _sqlRepository = sqlRepository;
        }
        public GridModel GetListWithCombineExcludedOutputTax(GridParameter parameters, string coaId, string countryId, string inputOutput)
        {
            try
            {
                var coaStr = "WHERE ISNULL(C.Id,'') =''";
                if (coaId != "null")
                    coaStr = "WHERE ISNULL(C.Id,'') ='" + coaId + @"'";
                parameters.CmdText = @"SELECT F.Id, F.CountryId, F.COAId, F.UserName 'COAName'
                            , ST.Id AS TaxCategoryId, ST.UserName AS TaxCategoryName, ST.Code, ST.[Description]
                            , F.GLGeneralInfoId, F.AssetGLCode, F.AssetGLText
	                        , F.BudgetMasterId, F.BudgetName
	                        , F.ActivityId, F.ActivityName
	                        , F.LiabilityGLId, F.LiabilityGLCode, F.LiabilityGLText
	                        , F.LiabilityBudgetMasterId, F.LiabilityBudgetName
	                        , F.LiabilityActivityId, F.LiabilityActivityName
	                        , F.InputTaxOutPutTax
                        FROM (SELECT * FROM [MST].[TaxCategory] WHERE CountryId='" + countryId + @"') AS ST
                        LEFT JOIN (SELECT STAD.Id, STAD.TaxCategoryId, C.Id AS COAId, C.UserName, STAD.CountryId, STAD.InputTaxOutPutTax
        	                        , STAD.GLGeneralInfoId, GLR.UserName AS  AssetGLText, GLR.AccountCode AS AssetGLCode
			                        , STAD.BudgetMasterId, B.UserName AS BudgetName
			                        , STAD.ActivityId, A.UserName AS ActivityName

			                        , STAD.LiabilityGLId, LG.UserName AS  LiabilityGLText, LG.AccountCode AS LiabilityGLCode
			                        , STAD.LiabilityBudgetMasterId, LB.UserName AS LiabilityBudgetName
			                        , STAD.LiabilityActivityId, LA.UserName AS LiabilityActivityName
                            FROM [MST].[TaxCategoryGL] AS STAD
                            LEFT JOIN [HKP].[COA] AS C ON STAD.COAId=C.Id
                            LEFT JOIN [HKP].[GLGeneralInfo] AS GLR ON GLR.Id=STAD.GLGeneralInfoId
	                        LEFT JOIN [MST].[BudgetMaster] AS BM ON STAD.BudgetMasterId = BM.Id
	                        LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId = B.Id
	                        LEFT JOIN [HKP].[Activity] AS A ON STAD.ActivityId = A.Id

	                        LEFT JOIN [HKP].[GLGeneralInfo] AS LG ON STAD.LiabilityGLId=LG.Id
	                        LEFT JOIN [MST].[BudgetMaster] AS LBM ON STAD.LiabilityBudgetMasterId = LBM.Id
	                        LEFT JOIN [HKP].[Budget] AS LB ON LBM.BudgetId = LB.Id
	                        LEFT JOIN [HKP].[Activity] AS LA ON STAD.LiabilityActivityId = LA.Id
                       " + coaStr + " AND STAD.InputTaxOutPutTax='" + inputOutput + "' AND STAD.TaxType='Excluded')AS F ON F.TaxCategoryId = ST.Id";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }
        public GridModel GetListWithCombineAssingExcludedOutput(GridParameter parameters, string coaId, string countryId, string inputOutput)
        {
            try
            {
                var coaStr = " ";
                if (coaId != "null")
                    coaStr += "WHERE ISNULL(C.Id,'') ='" + coaId + @"'";
                parameters.CmdText = @"SELECT F.Id, F.CountryId, F.COAId, F.UserName 'COAName'
                        , ST.Id AS TaxCategoryId, ST.UserName AS TaxCategoryName, ST.Code, ST.[Description]
                        , F.GLGeneralInfoId, F.AssetGLCode, F.AssetGLText
	                    , F.BudgetMasterId, F.BudgetName
	                    , F.ActivityId, F.ActivityName
	                    , F.LiabilityGLId, F.LiabilityGLCode, F.LiabilityGLText
	                    , F.LiabilityBudgetMasterId, F.LiabilityBudgetName
	                    , F.LiabilityActivityId, F.LiabilityActivityName
	                    , F.InputTaxOutPutTax
                    FROM (SELECT * FROM [MST].[TaxCategory] WHERE CountryId='" + countryId + @"') AS ST
                    LEFT JOIN (SELECT STAD.Id, STAD.TaxCategoryId, C.Id AS COAId, C.UserName, STAD.CountryId, STAD.InputTaxOutPutTax
        	                    , STAD.GLGeneralInfoId, GLR.UserName AS  AssetGLText, GLR.AccountCode AS AssetGLCode
			                    , STAD.BudgetMasterId, B.UserName AS BudgetName
			                    , STAD.ActivityId, A.UserName AS ActivityName

			                    , STAD.LiabilityGLId, LG.UserName AS  LiabilityGLText, LG.AccountCode AS LiabilityGLCode
			                    , STAD.LiabilityBudgetMasterId, LB.UserName AS LiabilityBudgetName
			                    , STAD.LiabilityActivityId, LA.UserName AS LiabilityActivityName
                        FROM [MST].[TaxCategoryGL] AS STAD
                        LEFT JOIN [HKP].[COA] AS C ON STAD.COAId=C.Id
                        LEFT JOIN [HKP].[GLGeneralInfo] AS GLR ON GLR.Id=STAD.GLGeneralInfoId
	                    LEFT JOIN [MST].[BudgetMaster] AS BM ON STAD.BudgetMasterId = BM.Id
	                    LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId = B.Id
	                    LEFT JOIN [HKP].[Activity] AS A ON STAD.ActivityId = A.Id

	                    LEFT JOIN [HKP].[GLGeneralInfo] AS LG ON STAD.LiabilityGLId=LG.Id
	                    LEFT JOIN [MST].[BudgetMaster] AS LBM ON STAD.LiabilityBudgetMasterId = LBM.Id
	                    LEFT JOIN [HKP].[Budget] AS LB ON LBM.BudgetId = LB.Id
	                    LEFT JOIN [HKP].[Activity] AS LA ON STAD.LiabilityActivityId = LA.Id
                   " + coaStr + " AND STAD.InputTaxOutPutTax='" + inputOutput + "' AND STAD.TaxType='Excluded')AS F ON F.TaxCategoryId = ST.Id WHERE  F.GLGeneralInfoId <> ''";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }
        public GridModel GetListWithCombineNotAssingExcludedOutput(GridParameter parameters, string coaId, string countryId, string inputOutput)
        {
            try
            {
                var coaStr = " ";
                if (coaId != "null")
                    coaStr += "WHERE ISNULL(C.Id,'') ='" + coaId + @"'";
                parameters.CmdText = @"SELECT F.Id, F.CountryId, F.COAId, F.UserName 'COAName', ST.Id AS TaxCategoryId, ST.UserName AS TaxCategoryName, ST.Code, ST.[Description]
                                    , F.GLGeneralInfoId, F.AssetGLCode, F.AssetGLText, F.BudgetMasterId, F.BudgetName
	                                , F.ActivityId, F.ActivityName, F.LiabilityGLId, F.LiabilityGLCode, F.LiabilityGLText
	                                , F.LiabilityBudgetMasterId, F.LiabilityBudgetName, F.LiabilityActivityId, F.LiabilityActivityName, F.InputTaxOutPutTax
                                    FROM (SELECT * FROM [MST].[TaxCategory] WHERE CountryId='" + countryId + @"') AS ST
                                    LEFT JOIN (SELECT STAD.Id, STAD.TaxCategoryId, C.Id AS COAId, C.UserName, STAD.CountryId, STAD.InputTaxOutPutTax
        	                        , STAD.GLGeneralInfoId, GLR.UserName AS  AssetGLText, GLR.AccountCode AS AssetGLCode, STAD.BudgetMasterId, B.UserName AS BudgetName, STAD.ActivityId, A.UserName AS ActivityName
			                        , STAD.LiabilityGLId, LG.UserName AS  LiabilityGLText, LG.AccountCode AS LiabilityGLCode, STAD.LiabilityBudgetMasterId, LB.UserName AS LiabilityBudgetName
			                        , STAD.LiabilityActivityId, LA.UserName AS LiabilityActivityName
                                    FROM [MST].[TaxCategoryGL] AS STAD
                                    LEFT JOIN [HKP].[COA] AS C ON STAD.COAId=C.Id
                                    LEFT JOIN [HKP].[GLGeneralInfo] AS GLR ON GLR.Id=STAD.GLGeneralInfoId
	                                LEFT JOIN [MST].[BudgetMaster] AS BM ON STAD.BudgetMasterId = BM.Id
	                                LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId = B.Id
	                                LEFT JOIN [HKP].[Activity] AS A ON STAD.ActivityId = A.Id
	                                LEFT JOIN [HKP].[GLGeneralInfo] AS LG ON STAD.LiabilityGLId=LG.Id
	                                LEFT JOIN [MST].[BudgetMaster] AS LBM ON STAD.LiabilityBudgetMasterId = LBM.Id
	                                LEFT JOIN [HKP].[Budget] AS LB ON LBM.BudgetId = LB.Id
	                                LEFT JOIN [HKP].[Activity] AS LA ON STAD.LiabilityActivityId = LA.Id
                                   " + coaStr + " AND STAD.InputTaxOutPutTax='" + inputOutput + "' AND STAD.TaxType='Excluded') AS F ON F.TaxCategoryId = ST.Id WHERE (ISNULL(F.GLGeneralInfoId,'')= '')";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }
        public string GetTaxYearId(string fromDate, string toDate, string companyId)
        {
            try
            {
                string taxYearId = "";
                var sql = @"SELECT DISTINCT TY.Id TaxCodeYearId
                        FROM [MST].[TaxCodeYear] AS TCY
                        LEFT JOIN [SCS].[TaxYear] AS TY ON TY.Id=TCY.TaxYearId
                        LEFT JOIN [SCS].[TaxYearPeriod] AS TYP ON TYP.TaxYearId=TY.Id
                        WHERE (Year(TYP.StartDate) = Year('" + fromDate + @"')  and Month(TYP.StartDate) = Month('" + fromDate + @"')) or
                        (Year(TYP.EndDate) = Year('" + toDate + @"')  and Month(TYP.EndDate) = Month('" + toDate + @"'))";
                DataTable dtTax = _sqlRepository.GetDataTable(sql);
                taxYearId = "''";
                if (dtTax.Rows.Count > 0)
                {

                    for (int i = 0; i < dtTax.Rows.Count; i++)
                    {
                        taxYearId += ",'" + dtTax.Rows[i]["TaxCodeYearId"].ToString() + "'";
                    }
                }

                return taxYearId;




            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }

        #region RCM Payable & Receivable
        public IWorkbook GetRCMPayableReport(string companyGroupId, string companyId, string plantId, string plantName, string fromDate, string toDate, string name)
        {
            clsReport objRpt = null;
            clsReport objRptSR = null;
            try
            {

                ExcelEngine excelEngine = null;
                IApplication application = null;
                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;
                excelEngine.Excel.DefaultVersion = ExcelVersion.Excel2013;
                var reportUtility = new ReportUtility();
                var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
                workbook.Version = ExcelVersion.Excel2013;
                var sheet1 = workbook.Worksheets[0];

                #region Logo
                string strPath = "";
                Image companyLogo = null;
                try
                {
                    DataTable dtCompanyImage = _sqlRepository.GetDataTable("SELECT * FROM ORG.COMPANY WHERE ID = '" + companyId + @"'");

                    strPath = Path.Combine(ResourcesPathReader.GetLogoOrImagePath(), dtCompanyImage.Rows[0]["Image"].ToString());  // IDCardEng.xlsx
                    companyLogo = Image.FromFile(strPath);
                }
                catch (Exception)
                {
                }
                #endregion
                objRpt = new clsReport();

                objRptSR = new clsReport(_sqlRepository);

                DataTable dtRCMPayable = null;
                string taxyearId = GetTaxYearId(fromDate, toDate, companyId);
                dtRCMPayable = GetRCMPayable(companyGroupId, companyId, plantId, plantName, fromDate, toDate, taxyearId);
                if (dtRCMPayable.Rows.Count == 0)
                {
                    throw new Exception("No Data Found....");
                }

                DataTable dtCmp = objRptSR.SelectedCompanyDT(plantId);

                DataTable dtFactory = objRptSR.SelectedPlantDT(plantId);

                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;

                int xlsRow = 1, xlsCol = 1;
                int endXlsCol = 1;
                string FactoryName = "";
                string CmpName = "";
                xlsRow = 6;
                sheet1.Range[xlsRow - 1, 1].Text = "Report Ref No:";
                sheet1.Range[xlsRow - 1, 1].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow - 1, 1].RowHeight = 20;
                sheet1.Range[xlsRow - 1, 1].CellStyle.Font.Bold = true;



                int iPostingDate = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Date";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;

                xlsCol++;
                int iTaxPercentage = xlsCol; // Party
                sheet1.Range[xlsRow, xlsCol].Text = "Percentage";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignRight;


                xlsCol++;
                int iParticulars = xlsCol; // Party
                sheet1.Range[xlsRow, xlsCol].Text = "Particulars";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 40;
                xlsCol++;

                int iPartyPlant = xlsCol; // Party
                sheet1.Range[xlsRow, xlsCol].Text = "Party Plant";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 40;
                xlsCol++;
                int iGSTIN = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "GSTIN";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;

                xlsCol++;
                int iVoucherType = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Voucher Type";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                xlsCol++;
                int iVoucherNo = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Voucher No";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                xlsCol++;
                int iVoucherRef = xlsCol; // Doc Ref
                sheet1.Range[xlsRow, xlsCol].Text = "Voucher Ref";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;

                xlsCol++;
                int iTaxableAmount = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Taxable Amount";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                DataTable dtTaxCode = null;
                dtRCMPayable.DefaultView.Sort = "TCSequence";
                dtTaxCode = dtRCMPayable.DefaultView.ToTable(true, "TaxCode");
                dtTaxCode.Columns.Add("ColumnNumber", typeof(String));
                dtTaxCode.Columns.Add("ColumnFormula", typeof(String));

                if (dtTaxCode.Rows.Count > 0)
                {
                    for (int i = 0; i < dtTaxCode.Rows.Count; i++)
                    {
                        xlsCol++;
                        sheet1.Range[xlsRow, xlsCol].Text = dtTaxCode.Rows[i]["TaxCode"].ToString();
                        sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                        dtTaxCode.Rows[i]["ColumnNumber"] = xlsCol.ToString();
                    }
                }
                endXlsCol = xlsCol;

                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].WrapText = true;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 23;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;

                string voucherNo = "";
                string TC = "";
                string Particulars = "";
                string voucherNocomp = "";
                string taxFitler = "";
                string Percentage = "";
                int startRow = 0;
                int perStartRow = 0;
                string formula = "";
                string formula2 = "";
                string totalFormula = "";

                string lineItemPercentageType = "";
                xlsRow++;
                startRow = xlsRow;
                perStartRow = xlsRow;
                bool isFirst = true;

                string Particularstemp = "";
                for (int i = 0; i < dtRCMPayable.Rows.Count; i++)
                {
                    if (dtRCMPayable.Rows[i]["LineItemType"].ToString().ToUpper() == "GL")
                    {
                        Particularstemp = dtRCMPayable.Rows[i]["Particular"].ToString();
                        voucherNocomp = dtRCMPayable.Rows[i]["VoucherNo"].ToString() + "-" + dtRCMPayable.Rows[i]["LineItemType"].ToString().ToUpper() + Particularstemp;
                        taxFitler = " and VoucherNo = '" + dtRCMPayable.Rows[i]["VoucherNo"].ToString() + "' and LineItemType = '" + dtRCMPayable.Rows[i]["LineItemType"].ToString() + "'";
                    }
                    if (dtRCMPayable.Rows[i]["LineItemType"].ToString().ToUpper() == "MATERIAL")
                    {

                        Particularstemp = dtRCMPayable.Rows[i]["Particular"].ToString();
                        voucherNocomp = dtRCMPayable.Rows[i]["VoucherNo"].ToString() + "-" + dtRCMPayable.Rows[i]["LineItemType"].ToString().ToUpper() + "-" + dtRCMPayable.Rows[i]["InventoryReceiveDetailId"].ToString().ToUpper() + Particularstemp;

                        taxFitler = " and VoucherNo = '" + dtRCMPayable.Rows[i]["VoucherNo"].ToString() + "' and LineItemType = '" + dtRCMPayable.Rows[i]["LineItemType"].ToString() + "' and InventoryReceiveDetailId = '" + dtRCMPayable.Rows[i]["InventoryReceiveDetailId"].ToString().ToUpper() + "'";

                    }
                    if (dtRCMPayable.Rows[i]["LineItemType"].ToString().ToUpper() == "SERVICE")
                    {

                        Particularstemp = dtRCMPayable.Rows[i]["Particular"].ToString();
                        voucherNocomp = dtRCMPayable.Rows[i]["VoucherNo"].ToString() + "-" + dtRCMPayable.Rows[i]["LineItemType"].ToString().ToUpper() + "-" + dtRCMPayable.Rows[i]["InventoryServiceId"].ToString().ToUpper() + Particularstemp;
                        taxFitler = " and VoucherNo = '" + dtRCMPayable.Rows[i]["VoucherNo"].ToString() + "' and LineItemType = '" + dtRCMPayable.Rows[i]["LineItemType"].ToString() + "' and InventoryServiceId = '" + dtRCMPayable.Rows[i]["InventoryServiceId"].ToString().ToUpper() + "'";

                    }

                    if (dtRCMPayable.Rows[i]["LineItemType"].ToString().ToUpper() == "SALES")
                    {
                        Particularstemp = dtRCMPayable.Rows[i]["Particular"].ToString();
                        voucherNocomp = dtRCMPayable.Rows[i]["VoucherNo"].ToString() + "-" + dtRCMPayable.Rows[i]["LineItemType"].ToString().ToUpper() + "-" + dtRCMPayable.Rows[i]["InventoryServiceId"].ToString().ToUpper() + Particularstemp;
                        taxFitler = " and VoucherNo = '" + dtRCMPayable.Rows[i]["VoucherNo"].ToString() + "' and LineItemType = '" + dtRCMPayable.Rows[i]["LineItemType"].ToString() + "' and InventoryReceiveDetailId = '" + dtRCMPayable.Rows[i]["InventoryReceiveDetailId"].ToString().ToUpper() + "'";

                    }


                    if (voucherNo != voucherNocomp)
                    {

                        if (dtRCMPayable.Rows[i]["LineItemType"].ToString().ToUpper() == "GL")
                        {
                            lineItemPercentageType = "ValueOfFixed";
                        }
                        if (dtRCMPayable.Rows[i]["LineItemType"].ToString().ToUpper() == "MATERIAL" || dtRCMPayable.Rows[i]["LineItemType"].ToString().ToUpper() == "SALES")
                        {
                            lineItemPercentageType = "Percentage";
                        }
                        if (Percentage != dtRCMPayable.Rows[i][lineItemPercentageType].ToString())
                        {
                            if (isFirst == false)
                            {

                                sheet1[perStartRow, iPostingDate, xlsRow - 1, iPostingDate].BorderAround(ExcelLineStyle.Hair);
                                sheet1[perStartRow, iTaxPercentage, xlsRow - 1, iTaxPercentage].BorderAround(ExcelLineStyle.Hair);
                                sheet1[perStartRow, iParticulars, xlsRow - 1, iParticulars].BorderAround(ExcelLineStyle.Hair);
                                sheet1[perStartRow, iPartyPlant, xlsRow - 1, iPartyPlant].BorderAround(ExcelLineStyle.Hair);
                                sheet1[perStartRow, iGSTIN, xlsRow - 1, iGSTIN].BorderAround(ExcelLineStyle.Hair);
                                sheet1[perStartRow, iVoucherType, xlsRow - 1, iVoucherType].BorderAround(ExcelLineStyle.Hair);
                                sheet1[perStartRow, iVoucherNo, xlsRow - 1, iVoucherNo].BorderAround(ExcelLineStyle.Hair);
                                sheet1[perStartRow, iVoucherRef, xlsRow - 1, iVoucherRef].BorderAround(ExcelLineStyle.Hair);
                                sheet1[perStartRow, iTaxableAmount, xlsRow - 1, iTaxableAmount].BorderAround(ExcelLineStyle.Hair);
                                formula = "SUM(" + clsStaticInfo.GetxlsCol(iTaxableAmount) + perStartRow + ":" + clsStaticInfo.GetxlsCol(iTaxableAmount) + (xlsRow - 1) + ")";
                                formula2 = "";
                                if (dtTaxCode.Rows.Count > 0)
                                {
                                    for (int j = 0; j < dtTaxCode.Rows.Count; j++)
                                    {
                                        sheet1[perStartRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"]), xlsRow - 1, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])].BorderAround(ExcelLineStyle.Hair);
                                        formula2 = "SUM(" + clsStaticInfo.GetxlsCol(Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])) + perStartRow + ":" + clsStaticInfo.GetxlsCol(Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])) + (xlsRow - 1) + ")";
                                        sheet1[xlsRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"]), xlsRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])].Formula = formula2;

                                        dtTaxCode.Rows[j]["ColumnFormula"] += (clsStaticInfo.GetxlsCol(Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])) + xlsRow).ToString() + " + ";
                                    }
                                }
                                sheet1.Range[xlsRow, 1, xlsRow, 1].Text = "Total";

                                sheet1[xlsRow, iTaxableAmount, xlsRow, iTaxableAmount].Formula = formula;
                                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Font.Bold = true;
                                totalFormula += (clsStaticInfo.GetxlsCol(iTaxableAmount) + xlsRow).ToString() + "+";

                                xlsRow++;


                            }
                            xlsRow++;
                            sheet1.Range[xlsRow - 1, 1].Number = clsStaticInfo.dbl(dtRCMPayable.Rows[i][lineItemPercentageType].ToString());
                            sheet1.Range[xlsRow - 1, 1].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                            perStartRow = xlsRow;
                            isFirst = false;

                        }

                        sheet1.Range[xlsRow, iPostingDate].Text = clsStaticInfo.GetDateTaxFormate(dtRCMPayable.Rows[i]["PostingDate"].ToString());
                        sheet1.Range[xlsRow, iTaxPercentage].Number = clsStaticInfo.dbl(dtRCMPayable.Rows[i]["ValueOfFixed"].ToString());
                        sheet1.Range[xlsRow, iTaxPercentage].NumberFormat = reportUtility.NumberFormatDecimalTwo();

                        sheet1.Range[xlsRow, iParticulars].Text = dtRCMPayable.Rows[i]["PartyName"].ToString();
                        sheet1.Range[xlsRow, iPartyPlant].Text = dtRCMPayable.Rows[i]["PartyPlant"].ToString();
                        sheet1.Range[xlsRow, iGSTIN].Text = dtRCMPayable.Rows[i]["GSTIN"].ToString();
                        sheet1.Range[xlsRow, iVoucherType].Text = dtRCMPayable.Rows[i]["SourceType"].ToString();
                        sheet1.Range[xlsRow, iVoucherNo].Text = dtRCMPayable.Rows[i]["VoucherNo"].ToString();
                        sheet1.Range[xlsRow, iVoucherRef].Text = dtRCMPayable.Rows[i]["DocRefNo"].ToString();//TaxableAmount
                        sheet1.Range[xlsRow, iTaxableAmount].Number = clsStaticInfo.dbl(dtRCMPayable.Rows[i]["TaxableAmount"].ToString());//TaxableAmount
                        sheet1.Range[xlsRow, iTaxableAmount].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                        // dtRCMPayable.DefaultView.RowFilter = "VoucherNo = '" + dtRCMPayable.Rows[i]["VoucherNo"].ToString() + "'";
                        if (dtTaxCode.Rows.Count > 0)
                        {
                            for (int j = 0; j < dtTaxCode.Rows.Count; j++)
                            {
                                dtRCMPayable.DefaultView.RowFilter = "VoucherNo = '" + dtRCMPayable.Rows[i]["VoucherNo"].ToString() + @"' AND Seq=" + dtRCMPayable.Rows[i]["Seq"].ToString() + @" AND  TaxCode = '" + dtTaxCode.Rows[j]["TaxCode"].ToString() + "'" + taxFitler;

                                for (int AKA = 0; AKA < dtRCMPayable.DefaultView.Count; AKA++)
                                {
                                    sheet1.Range[xlsRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])].Number = clsStaticInfo.dbl(dtRCMPayable.DefaultView[AKA]["CrAmount"].ToString());
                                    sheet1.Range[xlsRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                                    // xlsRow++;
                                }
                                if (dtRCMPayable.DefaultView.Count > 0)
                                {
                                    sheet1.Range[xlsRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])].Number = clsStaticInfo.dbl(dtRCMPayable.DefaultView[0]["CrAmount"].ToString());
                                    sheet1.Range[xlsRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])].NumberFormat = reportUtility.NumberFormatDecimalTwo();

                                }
                                else
                                {
                                    sheet1.Range[xlsRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])].Text = "-";
                                    sheet1.Range[xlsRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])].HorizontalAlignment = ExcelHAlign.HAlignRight;


                                }
                            }
                        }
                        Percentage = dtRCMPayable.Rows[i][lineItemPercentageType].ToString();

                        xlsRow++;
                    }

                    if (dtRCMPayable.Rows[i]["LineItemType"].ToString().ToUpper() == "GL")
                    {
                        voucherNo = dtRCMPayable.Rows[i]["VoucherNo"].ToString() + "-" + dtRCMPayable.Rows[i]["LineItemType"].ToString().ToUpper() + Particularstemp;
                    }
                    if (dtRCMPayable.Rows[i]["LineItemType"].ToString().ToUpper() == "MATERIAL")
                    {
                        voucherNo = dtRCMPayable.Rows[i]["VoucherNo"].ToString() + "-" + dtRCMPayable.Rows[i]["LineItemType"].ToString().ToUpper() + "-" + dtRCMPayable.Rows[i]["InventoryReceiveDetailId"].ToString().ToUpper() + Particularstemp;
                    }
                    if (dtRCMPayable.Rows[i]["LineItemType"].ToString().ToUpper() == "SERVICE")
                    {
                        voucherNo = dtRCMPayable.Rows[i]["VoucherNo"].ToString() + "-" + dtRCMPayable.Rows[i]["LineItemType"].ToString().ToUpper() + "-" + dtRCMPayable.Rows[i]["InventoryServiceId"].ToString().ToUpper() + Particularstemp;
                    }
                    if (dtRCMPayable.Rows[i]["LineItemType"].ToString().ToUpper() == "SALES")
                    {
                        voucherNo = dtRCMPayable.Rows[i]["VoucherNo"].ToString() + "-" + dtRCMPayable.Rows[i]["LineItemType"].ToString().ToUpper() + "-" + dtRCMPayable.Rows[i]["InventoryReceiveDetailId"].ToString().ToUpper() + Particularstemp;
                    }


                }
                sheet1[perStartRow, iPostingDate, xlsRow - 1, iPostingDate].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iTaxPercentage, xlsRow - 1, iTaxPercentage].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iParticulars, xlsRow - 1, iParticulars].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iPartyPlant, xlsRow - 1, iPartyPlant].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iGSTIN, xlsRow - 1, iGSTIN].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iVoucherType, xlsRow - 1, iVoucherType].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iVoucherNo, xlsRow - 1, iVoucherNo].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iVoucherRef, xlsRow - 1, iVoucherRef].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iTaxableAmount, xlsRow - 1, iTaxableAmount].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iTaxableAmount, xlsRow - 1, iTaxableAmount].BorderAround(ExcelLineStyle.Hair);


                if (dtTaxCode.Rows.Count > 0)
                {
                    for (int j = 0; j < dtTaxCode.Rows.Count; j++)
                    {
                        sheet1[perStartRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"]), xlsRow - 1, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])].BorderAround(ExcelLineStyle.Hair);

                    }
                }



                if (dtTaxCode.Rows.Count > 0)
                {
                    for (int j = 0; j < dtTaxCode.Rows.Count; j++)
                    {
                        sheet1[perStartRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"]), xlsRow - 1, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])].BorderAround(ExcelLineStyle.Hair);
                        formula2 = "SUM(" + clsStaticInfo.GetxlsCol(Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])) + perStartRow + ":" + clsStaticInfo.GetxlsCol(Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])) + (xlsRow - 1) + ")";
                        sheet1[xlsRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"]), xlsRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])].Formula = formula2;
                        dtTaxCode.Rows[j]["ColumnFormula"] += (clsStaticInfo.GetxlsCol(Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])) + xlsRow).ToString() + " + ";

                    }
                }
                sheet1.Range[xlsRow, 1, xlsRow, 1].Text = "Total";
                formula = "SUM(" + clsStaticInfo.GetxlsCol(iTaxableAmount) + perStartRow + ":" + clsStaticInfo.GetxlsCol(iTaxableAmount) + (xlsRow - 1) + ")";

                sheet1[xlsRow, iTaxableAmount, xlsRow, iTaxableAmount].Formula = formula;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Font.Bold = true;
                totalFormula += (clsStaticInfo.GetxlsCol(iTaxableAmount) + xlsRow).ToString() + "+";



                xlsRow++;
                xlsRow++;


                if (dtTaxCode.Rows.Count > 0)
                {
                    for (int j = 0; j < dtTaxCode.Rows.Count; j++)
                    {
                        //sheet1[perStartRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"]), xlsRow - 1, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])].BorderAround(ExcelLineStyle.Hair);
                        //formula2 = "SUM(" + clsStaticInfo.GetxlsCol(Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])) + perStartRow + ":" + clsStaticInfo.GetxlsCol(Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])) + (xlsRow - 1) + ")";
                        string fm = dtTaxCode.Rows[j]["ColumnFormula"].ToString().Trim();
                        sheet1[xlsRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"]), xlsRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])].Formula = fm.Remove(fm.Length - 1); //dtTaxCode.Rows[j]["ColumnFormula"].ToString().Remove(dtTaxCode.Rows[j]["ColumnFormula"].ToString().Length - 1);
                    }
                }
                sheet1.Range[xlsRow, 1, xlsRow, 1].Text = "Grand Total";

                sheet1[xlsRow, iTaxableAmount, xlsRow, iTaxableAmount].Formula = totalFormula.Remove(totalFormula.Length - 1);
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Font.Bold = true;




                #region ******************Report Header******************



                xlsRow = 1;
                xlsCol = 3;
                try
                {
                    if (companyLogo != null)
                    {

                        double totalWidth = sheet1.GetColumnWidth(1) + sheet1.GetColumnWidth(iGSTIN);
                        int totalWidthPixel = (int)(totalWidth * 7.5);
                        int totalheight = (int)((sheet1.GetRowHeight(1) + sheet1.GetRowHeight(2) + sheet1.GetRowHeight(3) + sheet1.GetRowHeight(3)) * 1.50);

                        companyLogo = ReportUtility.FixedSize(companyLogo, totalWidthPixel, totalheight);
                        IPictureShape pic = null;

                        pic = sheet1.Pictures.AddPicture(1, 1, companyLogo);
                        //pic.Height = 80;
                        //pic.Width = 220;
                    }
                }
                catch (Exception ex)
                {
                }

                FactoryName = string.Empty;

                string FactoryAddress = string.Empty;

                if (dtCmp.Rows.Count > 0)
                {
                    CmpName = dtCmp.Rows[0]["CompanyName"].ToString();
                }
                else
                {
                    CmpName = "";
                }
                sheet1.Range[xlsRow, 3].Text = CmpName;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 12;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 17;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                if (dtFactory.Rows.Count > 0)
                {
                    //FactoryName = dsFactory.Tables[0].Rows[0]["PlantName"].ToString();
                    FactoryName = dtFactory.Rows[0]["UserName"].ToString();
                }
                else
                {
                    FactoryName = "";
                }
                sheet1.Range[xlsRow, 3].Text = FactoryName;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 14;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 18;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                if (dtFactory.Rows.Count > 0)
                {
                    FactoryAddress = dtFactory.Rows[0]["Address1"].ToString();
                }
                else
                {
                    FactoryAddress = "";
                }
                sheet1.Range[xlsRow, 3].Text = FactoryAddress;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                //sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 22;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                sheet1.Range[xlsRow, 3].Text = "RCM Tax Payable Report From " + fromDate + " To " + toDate;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;


                #endregion ******************Report Header******************

                #region Freeze Panes

                sheet1.IsDisplayZeros = false;
                sheet1.UsedRange["A7"].FreezePanes();
                sheet1.FirstVisibleColumn = 1;
                sheet1.FirstVisibleRow = 6;

                #endregion Freeze Panes

                #region UsedRange Alignment

                sheet1.UsedRange.WrapText = false;
                sheet1.UsedRange.CellStyle.Font.Size = 10;
                sheet1.Range["A1"].CellStyle.Font.Size = 14;
                sheet1.Range["A2"].CellStyle.Font.Size = 10;
                sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;
                sheet1.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                #endregion UsedRange Alignment

                #region Page Setup
                sheet1.PageSetup.TopMargin = 0.5;
                sheet1.PageSetup.BottomMargin = 0.7;
                sheet1.PageSetup.PrintTitleRows = "$1:$5";
                sheet1.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                sheet1.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + name + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:MM tt").ToString();
                sheet1.PageSetup.LeftMargin = 0.5;
                sheet1.PageSetup.RightMargin = 0.2;
                sheet1.PageSetup.Orientation = ExcelPageOrientation.Portrait;
                sheet1.PageSetup.FitToPagesTall = 0;
                sheet1.PageSetup.FitToPagesWide = 1;
                sheet1.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                sheet1.IsDisplayZeros = false;
                #endregion Page Setup


                sheet1.Name = "RCM Payable";
                return workbook;
            }
            catch (System.Exception ex)
            {

                throw ex;
            }
        }
        public IWorkbook GetRCMPayableSalesReport(string companyGroupId, string companyId, string plantId, string plantName, string fromDate, string toDate, string name)
        {
            clsReport objRpt = null;
            clsReport objRptSR = null;
            try
            {

                ExcelEngine excelEngine = null;
                IApplication application = null;
                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;
                excelEngine.Excel.DefaultVersion = ExcelVersion.Excel2013;
                var reportUtility = new ReportUtility();
                var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
                workbook.Version = ExcelVersion.Excel2013;
                var sheet1 = workbook.Worksheets[0];

                #region Logo
                string strPath = "";
                Image companyLogo = null;
                try
                {
                    DataTable dtCompanyImage = _sqlRepository.GetDataTable("SELECT * FROM ORG.COMPANY WHERE ID = '" + companyId + @"'");

                    strPath = Path.Combine(ResourcesPathReader.GetLogoOrImagePath(), dtCompanyImage.Rows[0]["Image"].ToString());  // IDCardEng.xlsx
                    companyLogo = Image.FromFile(strPath);
                }
                catch (Exception)
                {
                }
                #endregion
                objRpt = new clsReport();

                objRptSR = new clsReport(_sqlRepository);

                DataTable dtRCMPayableSales = null;
                string taxyearId = GetTaxYearId(fromDate, toDate, companyId);
                dtRCMPayableSales = GetRCMPayableSales(companyGroupId, companyId, plantId, plantName, fromDate, toDate, taxyearId);
                if (dtRCMPayableSales.Rows.Count == 0)
                {
                    throw new Exception("No Data Found....");
                }

                DataTable dtCmp = objRptSR.SelectedCompanyDT(plantId);

                DataTable dtFactory = objRptSR.SelectedPlantDT(plantId);

                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;

                int xlsRow = 1, xlsCol = 1;
                int endXlsCol = 1;
                string FactoryName = "";
                string CmpName = "";
                xlsRow = 6;
                sheet1.Range[xlsRow - 1, 1].Text = "Report Ref No:";
                sheet1.Range[xlsRow - 1, 1].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow - 1, 1].RowHeight = 20;
                sheet1.Range[xlsRow - 1, 1].CellStyle.Font.Bold = true;



                int iPostingDate = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Date";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;

                xlsCol++;
                int iTaxPercentage = xlsCol; // Party
                sheet1.Range[xlsRow, xlsCol].Text = "Percentage";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignRight;


                xlsCol++;
                int iParticulars = xlsCol; // Party
                sheet1.Range[xlsRow, xlsCol].Text = "Particulars";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 40;
                xlsCol++;

                int iPartyPlant = xlsCol; // Party
                sheet1.Range[xlsRow, xlsCol].Text = "Party Plant";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 40;
                xlsCol++;
                int iGSTIN = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "GSTIN";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;

                xlsCol++;
                int iVoucherType = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Voucher Type";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                xlsCol++;
                int iVoucherNo = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Voucher No";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                xlsCol++;
                int iVoucherRef = xlsCol; // Doc Ref
                sheet1.Range[xlsRow, xlsCol].Text = "Voucher Ref";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;

                xlsCol++;
                int iTaxableAmount = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Taxable Amount";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                DataTable dtTaxCode = null;
                dtRCMPayableSales.DefaultView.Sort = "TCSequence";
                dtTaxCode = dtRCMPayableSales.DefaultView.ToTable(true, "TaxCode");
                dtTaxCode.Columns.Add("ColumnNumber", typeof(String));
                dtTaxCode.Columns.Add("ColumnFormula", typeof(String));

                if (dtTaxCode.Rows.Count > 0)
                {
                    for (int i = 0; i < dtTaxCode.Rows.Count; i++)
                    {
                        xlsCol++;
                        sheet1.Range[xlsRow, xlsCol].Text = dtTaxCode.Rows[i]["TaxCode"].ToString();
                        sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                        dtTaxCode.Rows[i]["ColumnNumber"] = xlsCol.ToString();
                    }
                }
                endXlsCol = xlsCol;

                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].WrapText = true;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 23;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;

                string voucherNo = "";
                string voucherNocomp = "";
                string taxFitler = "";
                string Percentage = "";
                int startRow = 0;
                int perStartRow = 0;
                string formula = "";
                string formula2 = "";
                string totalFormula = "";

                string lineItemPercentageType = "";
                xlsRow++;
                startRow = xlsRow;
                perStartRow = xlsRow;
                bool isFirst = true;


                for (int i = 0; i < dtRCMPayableSales.Rows.Count; i++)
                {
                    if (dtRCMPayableSales.Rows[i]["LineItemType"].ToString().ToUpper() == "GL")
                    {
                        voucherNocomp = dtRCMPayableSales.Rows[i]["VoucherNo"].ToString() + "-" + dtRCMPayableSales.Rows[i]["LineItemType"].ToString().ToUpper();
                        taxFitler = " and VoucherNo = '" + dtRCMPayableSales.Rows[i]["VoucherNo"].ToString() + "' and LineItemType = '" + dtRCMPayableSales.Rows[i]["LineItemType"].ToString() + "'";
                    }
                    if (dtRCMPayableSales.Rows[i]["LineItemType"].ToString().ToUpper() == "MATERIAL")
                    {
                        voucherNocomp = dtRCMPayableSales.Rows[i]["VoucherNo"].ToString() + "-" + dtRCMPayableSales.Rows[i]["LineItemType"].ToString().ToUpper() + "-" + dtRCMPayableSales.Rows[i]["InventoryReceiveDetailId"].ToString().ToUpper();

                        taxFitler = " and VoucherNo = '" + dtRCMPayableSales.Rows[i]["VoucherNo"].ToString() + "' and LineItemType = '" + dtRCMPayableSales.Rows[i]["LineItemType"].ToString() + "' and InventoryReceiveDetailId = '" + dtRCMPayableSales.Rows[i]["InventoryReceiveDetailId"].ToString().ToUpper() + "'";

                    }
                    if (dtRCMPayableSales.Rows[i]["LineItemType"].ToString().ToUpper() == "SERVICE")
                    {
                        voucherNocomp = dtRCMPayableSales.Rows[i]["VoucherNo"].ToString() + "-" + dtRCMPayableSales.Rows[i]["LineItemType"].ToString().ToUpper() + "-" + dtRCMPayableSales.Rows[i]["InventoryServiceId"].ToString().ToUpper();
                        taxFitler = " and VoucherNo = '" + dtRCMPayableSales.Rows[i]["VoucherNo"].ToString() + "' and LineItemType = '" + dtRCMPayableSales.Rows[i]["LineItemType"].ToString() + "' and InventoryServiceId = '" + dtRCMPayableSales.Rows[i]["InventoryServiceId"].ToString().ToUpper() + "'";

                    }

                    if (dtRCMPayableSales.Rows[i]["LineItemType"].ToString().ToUpper() == "SALES")
                    {
                        voucherNocomp = dtRCMPayableSales.Rows[i]["VoucherNo"].ToString() + "-" + dtRCMPayableSales.Rows[i]["LineItemType"].ToString().ToUpper() + "-" + dtRCMPayableSales.Rows[i]["InventoryServiceId"].ToString().ToUpper();
                        taxFitler = " and VoucherNo = '" + dtRCMPayableSales.Rows[i]["VoucherNo"].ToString() + "' and LineItemType = '" + dtRCMPayableSales.Rows[i]["LineItemType"].ToString() + "' and InventoryReceiveDetailId = '" + dtRCMPayableSales.Rows[i]["InventoryReceiveDetailId"].ToString().ToUpper() + "'";

                    }

                    if (voucherNo != voucherNocomp)
                    {

                        if (dtRCMPayableSales.Rows[i]["LineItemType"].ToString().ToUpper() == "GL")
                        {
                            lineItemPercentageType = "ValueOfFixed";
                        }
                        if (dtRCMPayableSales.Rows[i]["LineItemType"].ToString().ToUpper() == "MATERIAL" || dtRCMPayableSales.Rows[i]["LineItemType"].ToString().ToUpper() == "SALES")
                        {
                            lineItemPercentageType = "Percentage";
                        }
                        if (Percentage != dtRCMPayableSales.Rows[i][lineItemPercentageType].ToString())
                        {
                            if (isFirst == false)
                            {
                                sheet1[perStartRow, iPostingDate, xlsRow - 1, iPostingDate].BorderAround(ExcelLineStyle.Hair);
                                sheet1[perStartRow, iTaxPercentage, xlsRow - 1, iTaxPercentage].BorderAround(ExcelLineStyle.Hair);
                                sheet1[perStartRow, iParticulars, xlsRow - 1, iParticulars].BorderAround(ExcelLineStyle.Hair);
                                sheet1[perStartRow, iPartyPlant, xlsRow - 1, iPartyPlant].BorderAround(ExcelLineStyle.Hair);
                                sheet1[perStartRow, iGSTIN, xlsRow - 1, iGSTIN].BorderAround(ExcelLineStyle.Hair);
                                sheet1[perStartRow, iVoucherType, xlsRow - 1, iVoucherType].BorderAround(ExcelLineStyle.Hair);
                                sheet1[perStartRow, iVoucherNo, xlsRow - 1, iVoucherNo].BorderAround(ExcelLineStyle.Hair);
                                sheet1[perStartRow, iVoucherRef, xlsRow - 1, iVoucherRef].BorderAround(ExcelLineStyle.Hair);
                                sheet1[perStartRow, iTaxableAmount, xlsRow - 1, iTaxableAmount].BorderAround(ExcelLineStyle.Hair);
                                formula = "SUM(" + clsStaticInfo.GetxlsCol(iTaxableAmount) + perStartRow + ":" + clsStaticInfo.GetxlsCol(iTaxableAmount) + (xlsRow - 1) + ")";
                                formula2 = "";
                                if (dtTaxCode.Rows.Count > 0)
                                {
                                    for (int j = 0; j < dtTaxCode.Rows.Count; j++)
                                    {
                                        sheet1[perStartRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"]), xlsRow - 1, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])].BorderAround(ExcelLineStyle.Hair);
                                        formula2 = "SUM(" + clsStaticInfo.GetxlsCol(Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])) + perStartRow + ":" + clsStaticInfo.GetxlsCol(Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])) + (xlsRow - 1) + ")";
                                        sheet1[xlsRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"]), xlsRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])].Formula = formula2;

                                        dtTaxCode.Rows[j]["ColumnFormula"] += (clsStaticInfo.GetxlsCol(Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])) + xlsRow).ToString() + " + ";
                                    }
                                }
                                sheet1.Range[xlsRow, 1, xlsRow, 1].Text = "Total";

                                sheet1[xlsRow, iTaxableAmount, xlsRow, iTaxableAmount].Formula = formula;
                                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Font.Bold = true;
                                totalFormula += (clsStaticInfo.GetxlsCol(iTaxableAmount) + xlsRow).ToString() + "+";
                                //sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].NumberFormat = clsStaticInfo.NumberFormat(2);
                                sheet1[xlsRow, 1, xlsRow, endXlsCol].NumberFormat = "#,##0.00;(#,##0.00)";
                                xlsRow++;

                            }
                            xlsRow++;
                            sheet1.Range[xlsRow - 1, 1].Number = clsStaticInfo.dbl(dtRCMPayableSales.Rows[i][lineItemPercentageType].ToString());
                            //sheet1.Range[xlsRow - 1, 1].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                            sheet1[xlsRow - 1, 1].NumberFormat = "#,##0.00;(#,##0.00)";

                            perStartRow = xlsRow;
                            isFirst = false;

                        }

                        sheet1.Range[xlsRow, iPostingDate].Text = dtRCMPayableSales.Rows[i]["PostingDate"].ToString();
                        sheet1.Range[xlsRow, iTaxPercentage].Number = clsStaticInfo.dbl(dtRCMPayableSales.Rows[i]["Percentage"].ToString());
                        //sheet1.Range[xlsRow, iTaxPercentage].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                        sheet1.Range[xlsRow, iTaxPercentage].NumberFormat = "#,##0.00;(#,##0.00)";


                        sheet1.Range[xlsRow, iParticulars].Text = dtRCMPayableSales.Rows[i]["PartyName"].ToString();
                        sheet1.Range[xlsRow, iPartyPlant].Text = dtRCMPayableSales.Rows[i]["PartyPlant"].ToString();
                        sheet1.Range[xlsRow, iGSTIN].Text = dtRCMPayableSales.Rows[i]["GSTIN"].ToString();
                        sheet1.Range[xlsRow, iVoucherType].Text = dtRCMPayableSales.Rows[i]["SourceType"].ToString();
                        sheet1.Range[xlsRow, iVoucherNo].Text = dtRCMPayableSales.Rows[i]["VoucherNo"].ToString();
                        sheet1.Range[xlsRow, iVoucherRef].Text = dtRCMPayableSales.Rows[i]["DocRefNo"].ToString();//TaxableAmount
                        sheet1.Range[xlsRow, iTaxableAmount].Number = clsStaticInfo.dbl(dtRCMPayableSales.Rows[i]["TaxableAmount"].ToString());//TaxableAmount
                        //sheet1.Range[xlsRow, iTaxableAmount].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                        sheet1.Range[xlsRow, iTaxableAmount].NumberFormat = "#,##0.00;(#,##0.00)";

                        dtRCMPayableSales.DefaultView.RowFilter = "VoucherNo = '" + dtRCMPayableSales.Rows[i]["VoucherNo"].ToString() + "'";

                        if (dtTaxCode.Rows.Count > 0)
                        {
                            for (int j = 0; j < dtTaxCode.Rows.Count; j++)
                            {
                                dtRCMPayableSales.DefaultView.RowFilter = "TaxCode = '" + dtTaxCode.Rows[j]["TaxCode"].ToString() + "'" + taxFitler;

                                if (dtRCMPayableSales.DefaultView.Count > 0)
                                {
                                    sheet1.Range[xlsRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])].Number = clsStaticInfo.dbl(dtRCMPayableSales.DefaultView[0]["CrAmount"].ToString());
                                    //sheet1.Range[xlsRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                                    sheet1.Range[xlsRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                                }
                                else
                                {
                                    sheet1.Range[xlsRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])].Text = "-";
                                    sheet1.Range[xlsRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])].HorizontalAlignment = ExcelHAlign.HAlignRight;

                                }
                            }
                        }


                        Percentage = dtRCMPayableSales.Rows[i][lineItemPercentageType].ToString();

                        xlsRow++;
                    }

                    if (dtRCMPayableSales.Rows[i]["LineItemType"].ToString().ToUpper() == "GL")
                    {
                        voucherNo = dtRCMPayableSales.Rows[i]["VoucherNo"].ToString() + "-" + dtRCMPayableSales.Rows[i]["LineItemType"].ToString().ToUpper();
                    }
                    if (dtRCMPayableSales.Rows[i]["LineItemType"].ToString().ToUpper() == "MATERIAL")
                    {
                        voucherNo = dtRCMPayableSales.Rows[i]["VoucherNo"].ToString() + "-" + dtRCMPayableSales.Rows[i]["LineItemType"].ToString().ToUpper() + "-" + dtRCMPayableSales.Rows[i]["InventoryReceiveDetailId"].ToString().ToUpper();
                    }
                    if (dtRCMPayableSales.Rows[i]["LineItemType"].ToString().ToUpper() == "SERVICE")
                    {
                        voucherNo = dtRCMPayableSales.Rows[i]["VoucherNo"].ToString() + "-" + dtRCMPayableSales.Rows[i]["LineItemType"].ToString().ToUpper() + "-" + dtRCMPayableSales.Rows[i]["InventoryServiceId"].ToString().ToUpper();
                    }
                    if (dtRCMPayableSales.Rows[i]["LineItemType"].ToString().ToUpper() == "SALES")
                    {
                        voucherNo = dtRCMPayableSales.Rows[i]["VoucherNo"].ToString() + "-" + dtRCMPayableSales.Rows[i]["LineItemType"].ToString().ToUpper() + "-" + dtRCMPayableSales.Rows[i]["InventoryReceiveDetailId"].ToString().ToUpper();
                    }
                }

                sheet1[perStartRow, iPostingDate, xlsRow - 1, iPostingDate].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iTaxPercentage, xlsRow - 1, iTaxPercentage].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iParticulars, xlsRow - 1, iParticulars].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iPartyPlant, xlsRow - 1, iPartyPlant].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iGSTIN, xlsRow - 1, iGSTIN].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iVoucherType, xlsRow - 1, iVoucherType].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iVoucherNo, xlsRow - 1, iVoucherNo].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iVoucherRef, xlsRow - 1, iVoucherRef].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iTaxableAmount, xlsRow - 1, iTaxableAmount].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iTaxableAmount, xlsRow - 1, iTaxableAmount].BorderAround(ExcelLineStyle.Hair);

                if (dtTaxCode.Rows.Count > 0)
                {
                    for (int j = 0; j < dtTaxCode.Rows.Count; j++)
                    {
                        sheet1[perStartRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"]), xlsRow - 1, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])].BorderAround(ExcelLineStyle.Hair);

                    }
                }


                if (dtTaxCode.Rows.Count > 0)
                {
                    for (int j = 0; j < dtTaxCode.Rows.Count; j++)
                    {
                        sheet1[perStartRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"]), xlsRow - 1, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])].BorderAround(ExcelLineStyle.Hair);
                        formula2 = "SUM(" + clsStaticInfo.GetxlsCol(Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])) + perStartRow + ":" + clsStaticInfo.GetxlsCol(Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])) + (xlsRow - 1) + ")";
                        sheet1[xlsRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"]), xlsRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])].Formula = formula2;
                        dtTaxCode.Rows[j]["ColumnFormula"] += (clsStaticInfo.GetxlsCol(Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])) + xlsRow).ToString() + " + ";

                    }
                }
                sheet1.Range[xlsRow, 1, xlsRow, 1].Text = "Total";
                formula = "SUM(" + clsStaticInfo.GetxlsCol(iTaxableAmount) + perStartRow + ":" + clsStaticInfo.GetxlsCol(iTaxableAmount) + (xlsRow - 1) + ")";

                sheet1[xlsRow, iTaxableAmount, xlsRow, iTaxableAmount].Formula = formula;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].NumberFormat = "#,##0.00;(#,##0.00)";
                totalFormula += (clsStaticInfo.GetxlsCol(iTaxableAmount) + xlsRow).ToString() + "+";


                xlsRow++;
                xlsRow++;


                if (dtTaxCode.Rows.Count > 0)
                {
                    for (int j = 0; j < dtTaxCode.Rows.Count; j++)
                    {
                        //sheet1[perStartRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"]), xlsRow - 1, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])].BorderAround(ExcelLineStyle.Hair);
                        //formula2 = "SUM(" + clsStaticInfo.GetxlsCol(Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])) + perStartRow + ":" + clsStaticInfo.GetxlsCol(Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])) + (xlsRow - 1) + ")";
                        string fm = dtTaxCode.Rows[j]["ColumnFormula"].ToString().Trim();
                        sheet1[xlsRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"]), xlsRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])].Formula = fm.Remove(fm.Length - 1); //dtTaxCode.Rows[j]["ColumnFormula"].ToString().Remove(dtTaxCode.Rows[j]["ColumnFormula"].ToString().Length - 1);
                    }
                }
                sheet1.Range[xlsRow, 1, xlsRow, 1].Text = "Grand Total";

                sheet1[xlsRow, iTaxableAmount, xlsRow, iTaxableAmount].Formula = totalFormula.Remove(totalFormula.Length - 1);
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].NumberFormat = "#,##0.00;(#,##0.00)";



                #region ******************Report Header******************



                xlsRow = 1;
                xlsCol = 3;
                try
                {
                    if (companyLogo != null)
                    {

                        double totalWidth = sheet1.GetColumnWidth(1) + sheet1.GetColumnWidth(iGSTIN);
                        int totalWidthPixel = (int)(totalWidth * 7.5);
                        int totalheight = (int)((sheet1.GetRowHeight(1) + sheet1.GetRowHeight(2) + sheet1.GetRowHeight(3) + sheet1.GetRowHeight(3)) * 1.50);

                        companyLogo = ReportUtility.FixedSize(companyLogo, totalWidthPixel, totalheight);
                        IPictureShape pic = null;

                        pic = sheet1.Pictures.AddPicture(1, 1, companyLogo);
                        //pic.Height = 80;
                        //pic.Width = 220;
                    }
                }
                catch (Exception ex)
                {
                }

                FactoryName = string.Empty;

                string FactoryAddress = string.Empty;

                if (dtCmp.Rows.Count > 0)
                {
                    CmpName = dtCmp.Rows[0]["CompanyName"].ToString();
                }
                else
                {
                    CmpName = "";
                }
                sheet1.Range[xlsRow, 3].Text = CmpName;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 12;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 17;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                if (dtFactory.Rows.Count > 0)
                {
                    //FactoryName = dsFactory.Tables[0].Rows[0]["PlantName"].ToString();
                    FactoryName = dtFactory.Rows[0]["UserName"].ToString();
                }
                else
                {
                    FactoryName = "";
                }
                sheet1.Range[xlsRow, 3].Text = FactoryName;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 14;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 18;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                if (dtFactory.Rows.Count > 0)
                {
                    FactoryAddress = dtFactory.Rows[0]["Address1"].ToString();
                }
                else
                {
                    FactoryAddress = "";
                }
                sheet1.Range[xlsRow, 3].Text = FactoryAddress;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                //sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 22;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                sheet1.Range[xlsRow, 3].Text = "RCM Tax Payable Sales Report From " + fromDate + " To " + toDate;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;


                #endregion ******************Report Header******************

                #region Freeze Panes

                sheet1.IsDisplayZeros = false;
                sheet1.UsedRange["A7"].FreezePanes();
                sheet1.FirstVisibleColumn = 1;
                sheet1.FirstVisibleRow = 6;

                #endregion Freeze Panes

                #region UsedRange Alignment

                sheet1.UsedRange.WrapText = false;
                sheet1.UsedRange.CellStyle.Font.Size = 10;
                sheet1.Range["A1"].CellStyle.Font.Size = 14;
                sheet1.Range["A2"].CellStyle.Font.Size = 10;
                sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;
                sheet1.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                #endregion UsedRange Alignment

                #region Page Setup
                sheet1.PageSetup.TopMargin = 0.5;
                sheet1.PageSetup.BottomMargin = 0.7;
                sheet1.PageSetup.PrintTitleRows = "$1:$5";
                sheet1.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                sheet1.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + name + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:MM tt").ToString();
                sheet1.PageSetup.LeftMargin = 0.5;
                sheet1.PageSetup.RightMargin = 0.2;
                sheet1.PageSetup.Orientation = ExcelPageOrientation.Portrait;
                sheet1.PageSetup.FitToPagesTall = 0;
                sheet1.PageSetup.FitToPagesWide = 1;
                sheet1.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                sheet1.IsDisplayZeros = false;
                #endregion Page Setup


                sheet1.Name = "RCM Payable Sales";
                return workbook;
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }

        private DataTable GetRCMPayable(string companyGroupId, string companyId, string plantId, string plantName, string fromDate, string toDate, string taxyearId)
        {
            string strSql = "";
            strSql = @"
select DENSE_RANK() over(partition by VoucherNo,TaxCode order by Id) AS Seq,* from (
                 
                select SourceType= case when V.SourceType='VendorInvoice' then 'Inbound Invoice'
						                when V.SourceType='VendorPayment' then 'Vendor Payment'
						                when V.SourceType='InventoryPayable' then 'Purchase' else '' end
                ,V.VoucherNo,Format(V.PostingDate,'dd-MMM-yyyy') PostingDate,V.DocRefNo,V.DocDate,P.UserName PartyName,P.TINNO GSTIN 
                ,LineItemType=case when v.SourceType='InventoryPayable' then 'Material' 
				                   when v.SourceType='VendorInvoice' then 'GL'
				                   when v.SourceType='VendorPayment' then 'GL'
				                   else '' end
				                   ,Particular=case when v.SourceType='InventoryPayable' then MM.UserName 
									                WHEN v.SourceType='VendorInvoice' THEN A.UserName
									                WHEN v.SourceType='VendorPayment' THEN AP.UserName
				                   else '' end
								   ,PP.UserName as PartyPlant
                ,TaxableAmount=case when v.SourceType='InventoryPayable' then IRD.TotalMaterialTranAmount
					                when v.SourceType='VendorInvoice' then VD.DrAmount	
					                when v.SourceType='VendorPayment' then IWD.Amount	else 0 end
                ,IT.Id,0 DrAmount ,CrAmount=case when ITD.AType='Cr' then IT.TaxAmount else 0 end
                ,TC.Code +' '+ 'RCM' TaxCode, TC.Sequence TCSequence,TC.TaxCategoryType,TC.UserName+'-'+TC.Code TaxCategory,IsNULL(TAXC.IsRCM,0) IsRCM,TAXC.UserName TaxCodeName
                ,IsNULL(IV.IsExcludingTax,0) IsExcludingTax,IsNULL(IR.IsTaxApplicable,0) IsTaxApplicable,TAXC.[Type],TAXC.ValueOfFixed
                ,HSNP.[Percentage],MM.HSNCodeId,MM.UserName Material,NULL InventoryReceiveDetailId, NULL InventoryServiceId

                FROM  TRN.InvoiceTaxDetail ITD   
				LEFT JOIN TRN.InvoiceTax IT ON IT.Id=ITD.InvoiceTaxId
                LEFT JOIN TRN.Voucher V ON V.Id=IT.VoucherId
                LEFT JOIN TRN.Invoice IV ON IV.Id=IT.InvoiceId
                LEFT JOIN TRN.InvoiceWriteOff IW ON IW.Id=IT.InvoiceWriteOffId
                LEFT JOIN HKP.Party P ON P.Id=IT.PartyId
                LEFT JOIN HKP.PartyPlant PP ON PP.Id=IT.PartyPlantId
				
                LEFT JOIN MST.TaxCategory TC ON TC.Id=IT.TaxCategoryId
                LEFT JOIN( select distinct TAC.Id,TAC.UserName,TAC.IsRCM,TAY.[Type],TACD.ValueOfFixed from MST.TaxCode TAC 
	                LEFT JOIN MST.TaxCodeYear TAY ON TAY.TaxCodeId=TAC.Id
	                LEFT JOIN MST.TaxCodeDetail TACD ON TACD.TaxCodeId=TAC.Id WHERE TAY.TaxYearId IN (" + taxyearId + @")) TAXC ON TAXC.Id=IT.TaxCodeId
                --LEFT JOIN SCS.TaxYear TY ON TY.Id=TAY.TaxYearId
                LEFT JOIN TRN.InventoryReceive IR ON IR.VoucherId=V.Id
                LEFT JOIN TRN.InventoryReceiveTax IRT ON IRT.InventoryReceiveId=IR.Id --AND IRT.TaxCategoryId=IT.TaxCategoryId
                LEFT JOIN MST.HSNTaxPercentage HSNP ON  IRT.HSNCodeId=HSNP.HSNCodeId AND HSNP.TaxCategoryId=IT.TaxCategoryId 
                LEFT JOIN TRN.InventoryReceiveDetail IRD ON IRD.InventoryReceiveId=IR.Id
                LEFT JOIN TRN.InventoryMaterial IM ON IM.Id=IRD.InventoryMaterialId
                LEFT JOIN MST.MaterialMaster MM ON MM.Id=IM.MaterialMasterId
                LEFT JOIN TRN.VoucherDetail VD ON VD.Id=IT.VoucherDetailId 
                LEFT JOIN HKP.Activity A ON A.Id=VD.ActivityId 
                LEFT JOIN (SELECT IW.InvoiceWriteOffId,IW.ActivityId,SUM(I.Amount) Amount FROM TRN.InvoiceWriteOffDetail IW 
			                JOIN TRN.Invoice I ON I.Id=IW.InvoiceId
		                GROUP BY InvoiceWriteOffId,ActivityId) IWD ON IWD.InvoiceWriteOffId=IT.InvoiceWriteOffId
                LEFT JOIN HKP.Activity AP ON AP.Id=IWD.ActivityId
                where TAXC.IsRCM=1 AND V.PostingDate between '" + fromDate + "' AND '" + toDate + @"' and V.PlantId = '" + plantId + @"' and V.IsPark=0
                AND ITD.AType='Cr'
                AND v.SourceType IN ('VendorInvoice','VendorPayment')
                UNION ALL
				select 'Purchase' SourceType
                ,V.VoucherNo,Format(V.PostingDate,'dd-MMM-yyyy') PostingDate,V.DocRefNo,V.DocDate,P.UserName PartyName,P.TINNO GSTIN 
                ,LineItemType=case when v.SourceType='InventoryPayable' then 'Material' 
				                   else '' end
				                   ,Particular=case when v.SourceType='InventoryPayable' then MM.UserName 
				                   else '' end
								   ,PP.UserName as PartyPlant
                ,TaxableAmount=case when v.SourceType='InventoryPayable' then IRD.MaterialTranAmount
					                	else 0 end
                ,IT.Id,0 DrAmount ,CrAmount=case when ITD.AType='Cr' then IRT.TaxAmount else 0 end
                ,TC.Code +' '+ 'RCM' TaxCode, TC.Sequence TCSequence,TC.TaxCategoryType,TC.UserName+'-'+TC.Code TaxCategory,IsNULL(TAXC.IsRCM,0) IsRCM,TAXC.UserName TaxCodeName
                ,IsNULL(IV.IsExcludingTax,0) IsExcludingTax,IsNULL(IR.IsTaxApplicable,0) IsTaxApplicable,TAXC.[Type],TAXC.ValueOfFixed
                ,IRT.[Percentage],MM.HSNCodeId,MM.UserName Material,IRT.InventoryReceiveDetailId, IRT.InventoryServiceId

                FROM  TRN.InvoiceTaxDetail ITD   
				LEFT JOIN TRN.InvoiceTax IT ON IT.Id=ITD.InvoiceTaxId
                LEFT JOIN TRN.Voucher V ON V.Id=IT.VoucherId
                LEFT JOIN TRN.Invoice IV ON IV.Id=IT.InvoiceId
                LEFT JOIN TRN.InvoiceWriteOff IW ON IW.Id=IT.InvoiceWriteOffId
                LEFT JOIN HKP.Party P ON P.Id=IT.PartyId
                LEFT JOIN HKP.PartyPlant PP ON PP.Id=IT.PartyPlantId
                LEFT JOIN MST.TaxCategory TC ON TC.Id=IT.TaxCategoryId
                LEFT JOIN( select distinct TAC.Id,TAC.UserName,TAC.IsRCM,TAY.[Type],TACD.ValueOfFixed from MST.TaxCode TAC 
	                LEFT JOIN MST.TaxCodeYear TAY ON TAY.TaxCodeId=TAC.Id
	                LEFT JOIN MST.TaxCodeDetail TACD ON TACD.TaxCodeId=TAC.Id WHERE TAY.TaxYearId IN (" + taxyearId + @")) TAXC ON TAXC.Id=IT.TaxCodeId
                --LEFT JOIN SCS.TaxYear TY ON TY.Id=TAY.TaxYearId
                LEFT JOIN TRN.InventoryReceive IR ON IR.VoucherId=V.Id
                LEFT JOIN TRN.InventoryReceiveTax IRT ON IRT.InventoryReceiveId=IR.Id AND IRT.TaxCategoryId=IT.TaxCategoryId
                
                LEFT JOIN TRN.InventoryReceiveDetail IRD ON IRD.Id=IRT.InventoryReceiveDetailId
                LEFT JOIN TRN.InventoryMaterial IM ON IM.Id=IRD.InventoryMaterialId
                LEFT JOIN MST.MaterialMaster MM ON MM.Id=IM.MaterialMasterId
                LEFT JOIN TRN.VoucherDetail VD ON VD.Id=IT.VoucherDetailId 
                LEFT JOIN HKP.Activity A ON A.Id=VD.ActivityId 
                LEFT JOIN (SELECT IW.InvoiceWriteOffId,IW.ActivityId,SUM(I.Amount) Amount FROM TRN.InvoiceWriteOffDetail IW 
			                JOIN TRN.Invoice I ON I.Id=IW.InvoiceId
		                GROUP BY InvoiceWriteOffId,ActivityId) IWD ON IWD.InvoiceWriteOffId=IT.InvoiceWriteOffId
                LEFT JOIN HKP.Activity AP ON AP.Id=IWD.ActivityId
                where IR.IsTaxApplicable=1 AND V.PostingDate between '" + fromDate + "' AND '" + toDate + @"'
                and ITD.AType='Cr'
				and V.PlantId = '" + plantId + @"' and V.IsPark=0 and IRT.InventoryServiceId IS NULL and v.SourceType='InventoryPayable'
                union all
				select 'Purchase' SourceType
                ,V.VoucherNo,Format(V.PostingDate,'dd-MMM-yyyy') PostingDate,V.DocRefNo,V.DocDate,P.UserName PartyName,P.TINNO GSTIN 
                ,LineItemType=case when v.SourceType='InventoryPayable' then 'Service' 
				                   else '' end
				                   ,Particular=case when v.SourceType='InventoryPayable' then SM.UserName 
				                   else '' end
								   ,PP.UserName as PartyPlant
                ,TaxableAmount=case when v.SourceType='InventoryPayable' then IRD.Amount
					                	else 0 end
                ,IT.Id,0 DrAmount ,CrAmount=case when ITD.AType='Dr' then IRT.TaxAmount else 0 end
                ,TC.Code +' '+ 'RCM' TaxCode, TC.Sequence TCSequence,TC.TaxCategoryType,TC.UserName+'-'+TC.Code TaxCategory,IsNULL(TAXC.IsRCM,0) IsRCM,TAXC.UserName TaxCodeName
                ,IsNULL(IV.IsExcludingTax,0) IsExcludingTax,IsNULL(IR.IsTaxApplicable,0) IsTaxApplicable,TAXC.[Type],TAXC.ValueOfFixed
                ,IRT.[Percentage],IRT.HSNCodeId,SM.UserName Material,IRT.InventoryReceiveDetailId, IRT.InventoryServiceId

                FROM  TRN.InvoiceTaxDetail ITD   
				LEFT JOIN TRN.InvoiceTax IT ON IT.Id=ITD.InvoiceTaxId
                LEFT JOIN TRN.Voucher V ON V.Id=IT.VoucherId
                LEFT JOIN TRN.Invoice IV ON IV.Id=IT.InvoiceId
                LEFT JOIN TRN.InvoiceWriteOff IW ON IW.Id=IT.InvoiceWriteOffId
                LEFT JOIN HKP.Party P ON P.Id=IT.PartyId
				LEFT JOIN HKP.PartyPlant PP ON PP.Id=IT.PartyPlantId
                LEFT JOIN MST.TaxCategory TC ON TC.Id=IT.TaxCategoryId
                LEFT JOIN( select distinct TAC.Id,TAC.UserName,TAC.IsRCM,TAY.[Type],TACD.ValueOfFixed from MST.TaxCode TAC 
	                LEFT JOIN MST.TaxCodeYear TAY ON TAY.TaxCodeId=TAC.Id
	                LEFT JOIN MST.TaxCodeDetail TACD ON TACD.TaxCodeId=TAC.Id WHERE TAY.TaxYearId IN (" + taxyearId + @")) TAXC ON TAXC.Id=IT.TaxCodeId
                LEFT JOIN TRN.InventoryReceive IR ON IR.VoucherId = V.Id
                            LEFT JOIN TRN.InventoryReceiveTax IRT ON IRT.InventoryReceiveId = IR.Id AND IRT.TaxCategoryId = IT.TaxCategoryId
                            LEFT JOIN TRN.InventoryService IRD ON IRD.Id = IRT.InventoryServiceId
							LEFT JOIN hkp.ServiceMaster SM ON SM.Id = IRD.ServiceMasterId
                LEFT JOIN TRN.VoucherDetail VD ON VD.Id=IT.VoucherDetailId 
                LEFT JOIN HKP.Activity A ON A.Id=VD.ActivityId 
                LEFT JOIN (SELECT IW.InvoiceWriteOffId,IW.ActivityId,SUM(I.Amount) Amount FROM TRN.InvoiceWriteOffDetail IW 
			                JOIN TRN.Invoice I ON I.Id=IW.InvoiceId
		                GROUP BY InvoiceWriteOffId,ActivityId) IWD ON IWD.InvoiceWriteOffId=IT.InvoiceWriteOffId
                LEFT JOIN HKP.Activity AP ON AP.Id=IWD.ActivityId
                WHERE IR.IsTaxApplicable=1 AND V.PostingDate between '" + fromDate + "' AND '" + toDate + @"'
                AND ITD.AType='Cr'
				AND V.PlantId = '" + plantId + @"'  AND V.IsPark=0 and IRT.InventoryReceiveDetailId IS NULL AND v.SourceType='InventoryPayable'
) AS K
                ORDER BY LineItemType,ValueOfFixed,Percentage";

            return _sqlRepository.GetDataTable(strSql);

        }

        private DataTable GetRCMPayableSales(string companyGroupId, string companyId, string plantId, string plantName, string fromDate, string toDate, string taxyearId)
        {
            string strSql = "";
            strSql = @" 
     select SourceType= case when V.SourceType='CustomerInvoice' then 'Outbound Invoice'
						                when V.SourceType='CustomerReceipt' then 'Customer Receipt'
						                 else '' end
                ,V.VoucherNo,Format(V.PostingDate,'dd-MMM-yyyy') PostingDate,V.DocRefNo,V.DocDate,P.UserName PartyName,P.TINNO GSTIN 
                ,LineItemType=case when v.SourceType='CustomerInvoice' then 'GL'
				                   when v.SourceType='CustomerReceipt' then 'GL'
				                   else '' end
				                   ,Particular=case WHEN v.SourceType='CustomerInvoice' THEN A.UserName
									                WHEN v.SourceType='CustomerReceipt' THEN AP.UserName
				                   else '' end
								   ,PP.UserName as PartyPlant
                ,TaxableAmount=case when v.SourceType='CustomerInvoice' then VD.DrAmount	
					                when v.SourceType='CustomerReceipt' then IWD.Amount	else 0 end
                ,IT.Id,0 DrAmount ,CrAmount=case when ITD.AType='Cr' then IT.TaxAmount else 0 end
                ,TC.Code +' '+ 'RCM' TaxCode, TC.Sequence TCSequence,TC.TaxCategoryType,TC.UserName+'-'+TC.Code TaxCategory,IsNULL(TAXC.IsRCM,0) IsRCM,TAXC.UserName TaxCodeName
                ,IsNULL(IV.IsExcludingTax,0) IsExcludingTax,IsNULL(IR.IsTaxApplicable,0) IsTaxApplicable,TAXC.[Type],TAXC.ValueOfFixed
                ,HSNP.[Percentage],MM.HSNCodeId,MM.UserName Material,NULL InventoryReceiveDetailId, NULL InventoryServiceId

                from TRN.InvoiceTax IT 
                left join TRN.InvoiceTaxDetail ITD  ON IT.Id=ITD.InvoiceTaxId AND ITD.AType='Cr'
                LEFT JOIN TRN.Voucher V ON V.Id=IT.VoucherId
                LEFT JOIN TRN.Invoice IV ON IV.Id=IT.InvoiceId
                LEFT JOIN TRN.InvoiceWriteOff IW ON IW.Id=IT.InvoiceWriteOffId
                LEFT JOIN HKP.Party P ON P.Id=IT.PartyId
                LEFT JOIN HKP.PartyPlant PP ON PP.Id=IT.PartyPlantId
				
                LEFT JOIN MST.TaxCategory TC ON TC.Id=IT.TaxCategoryId
                LEFT JOIN( select TAC.Id,TAC.UserName,TAC.IsRCM,TAY.[Type],TACD.ValueOfFixed from MST.TaxCode TAC 
	                LEFT JOIN MST.TaxCodeYear TAY ON TAY.TaxCodeId=TAC.Id
	                LEFT JOIN MST.TaxCodeDetail TACD ON TACD.TaxCodeId=TAC.Id WHERE TAY.TaxYearId IN ('','3')) TAXC ON TAXC.Id=IT.TaxCodeId
                --LEFT JOIN SCS.TaxYear TY ON TY.Id=TAY.TaxYearId
                LEFT JOIN TRN.InventoryReceive IR ON IR.VoucherId=V.Id
                LEFT JOIN TRN.InventoryReceiveTax IRT ON IRT.InventoryReceiveId=IR.Id --AND IRT.TaxCategoryId=IT.TaxCategoryId
                LEFT JOIN MST.HSNTaxPercentage HSNP ON  IRT.HSNCodeId=HSNP.HSNCodeId AND HSNP.TaxCategoryId=IT.TaxCategoryId 
                LEFT JOIN TRN.InventoryReceiveDetail IRD ON IRD.InventoryReceiveId=IR.Id
                LEFT JOIN TRN.InventoryMaterial IM ON IM.Id=IRD.InventoryMaterialId
                LEFT JOIN MST.MaterialMaster MM ON MM.Id=IM.MaterialMasterId
                LEFT JOIN TRN.VoucherDetail VD ON VD.Id=IT.VoucherDetailId 
                LEFT JOIN HKP.Activity A ON A.Id=VD.ActivityId 
                LEFT JOIN (SELECT IW.InvoiceWriteOffId,IW.ActivityId,SUM(I.Amount) Amount FROM TRN.InvoiceWriteOffDetail IW 
			                JOIN TRN.Invoice I ON I.Id=IW.InvoiceId
		                GROUP BY InvoiceWriteOffId,ActivityId) IWD ON IWD.InvoiceWriteOffId=IT.InvoiceWriteOffId
                LEFT JOIN HKP.Activity AP ON AP.Id=IWD.ActivityId
                where TAXC.IsRCM=1 AND V.PostingDate between '" + fromDate + "' AND '" + toDate + @"' and V.PlantId = '" + plantId + @"' and V.IsPark=0
                AND v.SourceType IN ('CustomerInvoice','CustomerReceipt') 

				UNION ALL

select 'Sales' SourceType
                ,V.VoucherNo,Format(V.PostingDate,'dd-MMM-yyyy') PostingDate,V.DocRefNo,V.DocDate,P.UserName PartyName,PP.GSTIN 
                ,LineItemType=case when v.SourceType='SalesInvoice' then 'Sales' 
				                   else '' end
				                   ,Particular=case when v.SourceType='SalesInvoice' then MM.UserName 
				                   else '' end
								   ,PP.UserName as PartyPlant 
                ,TaxableAmount=case when v.SourceType='SaleSInvoice' then ISNULL( SM.BaseAmount ,0)
					                	else 0 end
                ,IT.Id,0 DrAmount ,CrAmount=case when ITD.AType='Cr' then IRT.Amount else 0 end
                ,TC.Code +' '+ 'RCM' TaxCode, TC.Sequence TCSequence,TC.TaxCategoryType,TC.UserName+'-'+TC.Code TaxCategory
				,IsRCM=CASE WHEN IsNULL(CP.TaxApplicable,'')='Mandatory' THEN 1 ELSE 0 END,NULL TaxCodeName
                ,IsNULL(IV.IsExcludingTax,0) IsExcludingTax,IsTaxApplicable=CASE WHEN IsNULL(CP.TaxApplicable,'')='Mandatory' THEN 1 ELSE 0 END,NULL [Type],0 ValueOfFixed
                ,IRT.[Percentage],IRT.HSNCodeId,MM.UserName Material,IRT.SalesMaterialId InventoryReceiveDetailId, IRT.SalesServiceId InventoryServiceId

                from TRN.InvoiceTax IT 
                left join TRN.InvoiceTaxDetail ITD  ON IT.Id=ITD.InvoiceTaxId AND ITD.AType='Cr'
                LEFT JOIN TRN.Voucher V ON V.Id=IT.VoucherId
                LEFT JOIN TRN.Invoice IV ON IV.Id=IT.InvoiceId
                LEFT JOIN HKP.Party P ON P.Id=IT.PartyId
                LEFT JOIN HKP.PartyPlant PP ON PP.Id=IT.PartyPlantId

				LEFT JOIN HKP.CompanyParty CP ON CP.PartyId=P.Id AND CP.PartyType='Customer' AND CP.PlantId = '" + plantId + @"'
                LEFT JOIN MST.TaxCategory TC ON TC.Id=IT.TaxCategoryId
                
                LEFT JOIN TRN.Sales IR ON IR.VoucherId = V.Id
                            LEFT JOIN TRN.SalesTax IRT ON IRT.SalesId = IR.Id AND IRT.TaxCategoryId = IT.TaxCategoryId
							LEFT JOIN TRN.SalesMaterial SM ON SM.Id = IRT.SalesMaterialId
							LEFT JOIN MST.MaterialMaster MM ON MM.Id = SM.MaterialMasterId
                LEFT JOIN TRN.VoucherDetail VD ON VD.Id=IT.VoucherDetailId 
                LEFT JOIN HKP.Activity A ON A.Id=VD.ActivityId 
                
                where CP.TaxApplicable='Mandatory' AND V.PostingDate between '" + fromDate + "' AND '" + toDate + @"'
				and V.PlantId = '" + plantId + @"'  and V.IsPark=0 and IRT.SalesServiceId IS NULL and v.SourceType='SalesInvoice' AND ITD.AType='Cr'

Union ALL

                select 'SalesService' SourceType
                ,V.VoucherNo,Format(V.PostingDate,'dd-MMM-yyyy') PostingDate,V.DocRefNo,V.DocDate,P.UserName PartyName,PP.GSTIN 
                ,LineItemType=case when v.SourceType='SalesInvoice' then 'SalesService' 
				                   else '' end
				                   ,Particular=case when v.SourceType='SalesInvoice' then MM.UserName 
				                   else '' end
								   ,PP.UserName as PartyPlant
                ,TaxableAmount=case when v.SourceType='SaleSInvoice' then ISNULL( SM.BaseAmount ,0)
					                	else 0 end
                ,IT.Id,0 DrAmount ,CrAmount=case when ITD.AType='Cr' then IRT.Amount else 0 end
                ,TC.Code +' '+ 'RCM' TaxCode, TC.Sequence TCSequence,TC.TaxCategoryType,TC.UserName+'-'+TC.Code TaxCategory
				,IsRCM=CASE WHEN IsNULL(CP.TaxApplicable,'')='Mandatory' THEN 1 ELSE 0 END,NULL TaxCodeName
                ,IsNULL(IV.IsExcludingTax,0) IsExcludingTax,IsTaxApplicable=CASE WHEN IsNULL(CP.TaxApplicable,'')='Mandatory' THEN 1 ELSE 0 END,NULL [Type],0 ValueOfFixed
                ,IRT.[Percentage],IRT.HSNCodeId,MM.UserName Material,IRT.SalesMaterialId InventoryReceiveDetailId, IRT.SalesServiceId InventoryServiceId

                from TRN.InvoiceTax IT 
                left join TRN.InvoiceTaxDetail ITD  ON IT.Id=ITD.InvoiceTaxId AND ITD.AType='Cr'
                LEFT JOIN TRN.Voucher V ON V.Id=IT.VoucherId
                LEFT JOIN TRN.Invoice IV ON IV.Id=IT.InvoiceId
                LEFT JOIN HKP.Party P ON P.Id=IT.PartyId
                LEFT JOIN HKP.PartyPlant PP ON PP.Id=IT.PartyPlantId

				LEFT JOIN HKP.CompanyParty CP ON CP.PartyId=P.Id AND CP.PartyType='Customer' AND CP.PlantId = '" + plantId + @"'
                LEFT JOIN MST.TaxCategory TC ON TC.Id=IT.TaxCategoryId
                
                LEFT JOIN TRN.Sales IR ON IR.VoucherId = V.Id
                            LEFT JOIN TRN.SalesTax IRT ON IRT.SalesId = IR.Id AND IRT.TaxCategoryId = IT.TaxCategoryId
							LEFT JOIN TRN.SalesMaterial SM ON SM.Id = IRT.SalesMaterialId
							LEFT JOIN MST.MaterialMaster MM ON MM.Id = SM.MaterialMasterId
                LEFT JOIN TRN.VoucherDetail VD ON VD.Id=IT.VoucherDetailId 
                LEFT JOIN HKP.Activity A ON A.Id=VD.ActivityId 
                
                where CP.TaxApplicable='Mandatory' AND V.PostingDate between '" + fromDate + "' AND '" + toDate + @"'

                and V.PlantId = '" + plantId + @"'  and V.IsPark=0 and IRT.SalesServiceId<>'' and v.SourceType='SalesInvoice' AND ITD.AType='Cr'
UNION ALL

select 'InventorySales' SourceType
                ,V.VoucherNo,Format(V.PostingDate,'dd-MMM-yyyy') PostingDate,V.DocRefNo,V.DocDate,P.UserName PartyName,PP.GSTIN
               ,LineItemType=case when v.SourceType='SalesInvoice' then 'InventorySales' 
				                   else '' end
			   ,Particular=case when v.SourceType='SalesInvoice' then MM.UserName 
				                   else '' end
								   ,PP.UserName as PartyPlant
               ,TaxableAmount=case when v.SourceType='SalesInvoice' then ISNULL(ISD.TotalSalesAmount,0)
					                	else 0 end
										--,ISNULL(IST.[Percentage],0) TaxPercentage
										,IT.Id
                ,0 DrAmount ,CrAmount=case when ITD.AType='Dr' then ISNULL( IST.TaxAmount,0) else 0 end
                ,TC.Code +' '+ 'RCM' TaxCode, TC.Sequence TCSequence,TC.TaxCategoryType,TC.UserName+'-'+TC.Code TaxCategory
				,IsRCM=CASE WHEN IsNULL(CP.TaxApplicable,'')='Mandatory' THEN 1 ELSE 0 END,NULL TaxCodeName
                ,IsNULL(IV.IsExcludingTax,0) IsExcludingTax,IsTaxApplicable=CASE WHEN IsNULL(CP.TaxApplicable,'')='Mandatory' THEN 1 ELSE 0 END,NULL [Type],0 ValueOfFixed
                ,IST.[Percentage],IST.HSNCodeId,MM.UserName Material,IST.InventoryReceiveDetailId , IST.InventorySalesServiceId InventoryServiceId
				
                from TRN.InvoiceTax IT 
                left join TRN.InvoiceTaxDetail ITD  ON IT.Id=ITD.InvoiceTaxId AND ITD.AType='Dr'
                LEFT JOIN TRN.Voucher V ON V.Id=IT.VoucherId
                LEFT JOIN TRN.Invoice IV ON IV.Id=IT.InvoiceId
                LEFT JOIN HKP.Party P ON P.Id=IT.PartyId
                 LEFT JOIN HKP.PartyPlant PP ON PP.Id=IT.PartyPlantId
              
				LEFT JOIN HKP.CompanyParty CP ON CP.PartyId=P.Id AND CP.PartyType='Customer' AND
				CP.PlantId = '" + plantId + @"'
                LEFT JOIN MST.TaxCategory TC ON TC.Id=IT.TaxCategoryId
                LEFT JOIN TRN.InventorySales INS on INS.InventoryVoucherId=V.Id
			    LEFT JOIN TRN.InventorySalesTax  IST ON IST.InventorySalesId = INS.Id AND IST.TaxCategoryId = IT.TaxCategoryId
				LEFT JOIN TRN.InventorySalesDetail ISD ON ISD.InventorySalesId = INS.Id
				lEFT JOIN TRN.InventoryMaterial IM ON IM.Id=ISD.InventoryMaterialId
				LEFT JOIN MST.MaterialMaster MM ON MM.Id = IM.MaterialMasterId
                LEFT JOIN TRN.VoucherDetail VD ON VD.Id=IT.VoucherDetailId 
                LEFT JOIN HKP.Activity A ON A.Id=VD.ActivityId 
                
                where --CP.TaxApplicable='Mandatory' AND 
				V.PostingDate between  '" + fromDate + "' AND '" + toDate + @"'

                and V.PlantId = '" + plantId + @"'  and V.IsPark=0 and IST.InventorySalesServiceId<>'' and v.SourceType='SalesInvoice' AND ITD.AType='Dr'

                ORDER BY LineItemType,ValueOfFixed,Percentage";

            return _sqlRepository.GetDataTable(strSql);

        }


        public IWorkbook GetRCMReceivableReport(string companyGroupId, string companyId, string plantId, string plantName, string fromDate, string toDate, string name)
        {
            clsReport objRpt = null;
            clsReport objRptSR = null;
            try
            {

                ExcelEngine excelEngine = null;
                IApplication application = null;
                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;
                excelEngine.Excel.DefaultVersion = ExcelVersion.Excel2013;
                var reportUtility = new ReportUtility();
                var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
                workbook.Version = ExcelVersion.Excel2013;
                var sheet1 = workbook.Worksheets[0];

                #region Logo
                string strPath = "";
                Image companyLogo = null;
                try
                {
                    DataTable dtCompanyImage = _sqlRepository.GetDataTable("SELECT * FROM ORG.COMPANY WHERE ID = '" + companyId + @"'");

                    //strPath = Path.Combine(ResourcesPathReader.GetLogoOrImagePath(), dtCompanyImage.Rows[0]["Image"].ToString());  // IDCardEng.xlsx
                    //companyLogo = Image.FromFile(strPath);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                #endregion
                objRpt = new clsReport();

                objRptSR = new clsReport(_sqlRepository);

                DataTable dtRCMReceiviable = null;
                string taxyearId = GetTaxYearId(fromDate, toDate, companyId);
                dtRCMReceiviable = GetRCMReceviable(companyGroupId, companyId, plantId, plantName, fromDate, toDate, taxyearId);
                if (dtRCMReceiviable.Rows.Count == 0)
                {
                    throw new Exception("No Data Found....");
                }

                DataTable dtCmp = objRptSR.SelectedCompanyDT(plantId);

                DataTable dtFactory = objRptSR.SelectedPlantDT(plantId);

                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;

                int xlsRow = 1, xlsCol = 1;
                int endXlsCol = 1;
                string FactoryName = "";
                string CmpName = "";
                xlsRow = 6;
                sheet1.Range[xlsRow - 1, 1].Text = "Report Ref No:";
                sheet1.Range[xlsRow - 1, 1].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow - 1, 1].RowHeight = 20;
                sheet1.Range[xlsRow - 1, 1].CellStyle.Font.Bold = true;



                int iPostingDate = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Date";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;


                xlsCol++;
                int iTaxPercentage = xlsCol; // Party
                sheet1.Range[xlsRow, xlsCol].Text = "Percentage";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignRight;

                xlsCol++;
                int iParticulars = xlsCol; // Party
                sheet1.Range[xlsRow, xlsCol].Text = "Particulars";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 40;

                xlsCol++;
                int iPartyPlant = xlsCol; // Party
                sheet1.Range[xlsRow, xlsCol].Text = "Party Plant";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 40;

                xlsCol++;
                int iGSTIN = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "GSTIN";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;

                xlsCol++;
                int iVoucherType = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Voucher Type";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                xlsCol++;
                int iVoucherNo = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Voucher No";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                xlsCol++;
                int iVoucherRef = xlsCol; // Doc Ref
                sheet1.Range[xlsRow, xlsCol].Text = "Voucher Ref";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;

                xlsCol++;
                int iTaxableAmount = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Taxable Amount";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;

                sheet1[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignRight;

                DataTable dtTaxCode = null;
                dtRCMReceiviable.DefaultView.Sort = "TCSequence";
                dtTaxCode = dtRCMReceiviable.DefaultView.ToTable(true, "TaxCode");
                dtTaxCode.Columns.Add("ColumnNumber", typeof(String));
                dtTaxCode.Columns.Add("ColumnFormula", typeof(String));

                if (dtTaxCode.Rows.Count > 0)
                {
                    for (int i = 0; i < dtTaxCode.Rows.Count; i++)
                    {
                        xlsCol++;
                        sheet1.Range[xlsRow, xlsCol].Text = dtTaxCode.Rows[i]["TaxCode"].ToString();
                        sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                        dtTaxCode.Rows[i]["ColumnNumber"] = xlsCol.ToString();
                    }
                }
                endXlsCol = xlsCol;

                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].WrapText = true;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 23;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;

                string voucherNo = "";
                string Percentage = "";
                int startRow = 0;
                int perStartRow = 0;
                string formula = "";
                string formula2 = "";
                string totalFormula = "";

                string lineItemPercentageType = "";
                xlsRow++;
                startRow = xlsRow;
                perStartRow = xlsRow;
                bool isFirst = true;
                string voucherNocomp = "";
                string taxFitler = "";

                string Particularstemp = "";
                for (int i = 0; i < dtRCMReceiviable.Rows.Count; i++)
                {
                    //voucherNo = dtRCMPayable.Rows[i]["VoucherNo"].ToString();

                    if (voucherNo != dtRCMReceiviable.Rows[i]["VoucherNo"].ToString() + "-" + dtRCMReceiviable.Rows[i]["LineItemType"].ToString().ToUpper())
                    {
                        if (dtRCMReceiviable.Rows[i]["LineItemType"].ToString().ToUpper() == "GL")
                        {
                            Particularstemp = dtRCMReceiviable.Rows[i]["Particular"].ToString();

                            voucherNocomp = dtRCMReceiviable.Rows[i]["VoucherNo"].ToString() + "-" + dtRCMReceiviable.Rows[i]["LineItemType"].ToString().ToUpper() + Particularstemp;
                            taxFitler = " and VoucherNo = '" + dtRCMReceiviable.Rows[i]["VoucherNo"].ToString() + "' and LineItemType = '" + dtRCMReceiviable.Rows[i]["LineItemType"].ToString() + "'";
                        }
                        if (dtRCMReceiviable.Rows[i]["LineItemType"].ToString().ToUpper() == "MATERIAL")
                        {
                            Particularstemp = dtRCMReceiviable.Rows[i]["Particular"].ToString();

                            voucherNocomp = dtRCMReceiviable.Rows[i]["VoucherNo"].ToString() + "-" + dtRCMReceiviable.Rows[i]["LineItemType"].ToString().ToUpper() + "-" + dtRCMReceiviable.Rows[i]["InventoryReceiveDetailId"].ToString().ToUpper() + Particularstemp;

                            taxFitler = " and VoucherNo = '" + dtRCMReceiviable.Rows[i]["VoucherNo"].ToString() + "' and LineItemType = '" + dtRCMReceiviable.Rows[i]["LineItemType"].ToString() + "' and InventoryReceiveDetailId = '" + dtRCMReceiviable.Rows[i]["InventoryReceiveDetailId"].ToString().ToUpper() + "'";

                        }
                        if (dtRCMReceiviable.Rows[i]["LineItemType"].ToString().ToUpper() == "SERVICE")
                        {
                            Particularstemp = dtRCMReceiviable.Rows[i]["Particular"].ToString();

                            voucherNocomp = dtRCMReceiviable.Rows[i]["VoucherNo"].ToString() + "-" + dtRCMReceiviable.Rows[i]["LineItemType"].ToString().ToUpper() + "-" + dtRCMReceiviable.Rows[i]["InventoryServiceId"].ToString().ToUpper() + Particularstemp;
                            taxFitler = " and VoucherNo = '" + dtRCMReceiviable.Rows[i]["VoucherNo"].ToString() + "' and LineItemType = '" + dtRCMReceiviable.Rows[i]["LineItemType"].ToString() + "' and InventoryServiceId = '" + dtRCMReceiviable.Rows[i]["InventoryServiceId"].ToString().ToUpper() + "'";
                        }
                        if (dtRCMReceiviable.Rows[i]["LineItemType"].ToString().ToUpper() == "SALES")
                        {
                            Particularstemp = dtRCMReceiviable.Rows[i]["Particular"].ToString();

                            voucherNocomp = dtRCMReceiviable.Rows[i]["VoucherNo"].ToString() + "-" + dtRCMReceiviable.Rows[i]["LineItemType"].ToString().ToUpper() + "-" + dtRCMReceiviable.Rows[i]["InventoryReceiveDetailId"].ToString().ToUpper() + Particularstemp;

                            taxFitler = " and VoucherNo = '" + dtRCMReceiviable.Rows[i]["VoucherNo"].ToString() + "' and LineItemType = '" + dtRCMReceiviable.Rows[i]["LineItemType"].ToString() + "' and InventoryReceiveDetailId = '" + dtRCMReceiviable.Rows[i]["InventoryReceiveDetailId"].ToString().ToUpper() + "'";

                        }

                    }

                    if (voucherNo != voucherNocomp)
                    {
                        if (Percentage != dtRCMReceiviable.Rows[i]["TaxPercentage"].ToString())
                        {
                            if (isFirst == false)
                            {

                                sheet1[perStartRow, iPostingDate, xlsRow - 1, iPostingDate].BorderAround(ExcelLineStyle.Hair);
                                sheet1[perStartRow, iTaxPercentage, xlsRow - 1, iTaxPercentage].BorderAround(ExcelLineStyle.Hair);
                                sheet1[perStartRow, iParticulars, xlsRow - 1, iParticulars].BorderAround(ExcelLineStyle.Hair);
                                sheet1[perStartRow, iPartyPlant, xlsRow - 1, iPartyPlant].BorderAround(ExcelLineStyle.Hair);

                                sheet1[perStartRow, iGSTIN, xlsRow - 1, iGSTIN].BorderAround(ExcelLineStyle.Hair);
                                sheet1[perStartRow, iVoucherType, xlsRow - 1, iVoucherType].BorderAround(ExcelLineStyle.Hair);
                                sheet1[perStartRow, iVoucherNo, xlsRow - 1, iVoucherNo].BorderAround(ExcelLineStyle.Hair);
                                sheet1[perStartRow, iVoucherRef, xlsRow - 1, iVoucherRef].BorderAround(ExcelLineStyle.Hair);
                                sheet1[perStartRow, iTaxableAmount, xlsRow - 1, iTaxableAmount].BorderAround(ExcelLineStyle.Hair);
                                formula = "SUM(" + clsStaticInfo.GetxlsCol(iTaxableAmount) + perStartRow + ":" + clsStaticInfo.GetxlsCol(iTaxableAmount) + (xlsRow - 1) + ")";
                                formula2 = "";
                                if (dtTaxCode.Rows.Count > 0)
                                {
                                    for (int j = 0; j < dtTaxCode.Rows.Count; j++)
                                    {
                                        sheet1[perStartRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"]), xlsRow - 1, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])].BorderAround(ExcelLineStyle.Hair);
                                        formula2 = "SUM(" + clsStaticInfo.GetxlsCol(Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])) + perStartRow + ":" + clsStaticInfo.GetxlsCol(Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])) + (xlsRow - 1) + ")";
                                        sheet1[xlsRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"]), xlsRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])].Formula = formula2;

                                        dtTaxCode.Rows[j]["ColumnFormula"] += (clsStaticInfo.GetxlsCol(Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])) + xlsRow).ToString() + " + ";
                                    }
                                }
                                sheet1.Range[xlsRow, 1, xlsRow, 1].Text = "Total";

                                sheet1[xlsRow, iTaxableAmount, xlsRow, iTaxableAmount].Formula = formula;
                                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Font.Bold = true;
                                totalFormula += (clsStaticInfo.GetxlsCol(iTaxableAmount) + xlsRow).ToString() + "+";

                                xlsRow++;
                            }
                            xlsRow++;
                            sheet1.Range[xlsRow - 1, 1].Number = clsStaticInfo.dbl(dtRCMReceiviable.Rows[i]["TaxPercentage"].ToString());
                            sheet1.Range[xlsRow - 1, 1].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                            perStartRow = xlsRow;
                            isFirst = false;

                        }

                        sheet1.Range[xlsRow, iPostingDate].Text = clsStaticInfo.GetDateTaxFormate(dtRCMReceiviable.Rows[i]["PostingDate"].ToString());
                        sheet1.Range[xlsRow, iTaxPercentage].Number = clsStaticInfo.dbl(dtRCMReceiviable.Rows[i]["TaxPercentage"].ToString());
                        sheet1.Range[xlsRow, iTaxPercentage].NumberFormat = reportUtility.NumberFormatDecimalTwo();

                        sheet1.Range[xlsRow, iParticulars].Text = dtRCMReceiviable.Rows[i]["PartyName"].ToString();
                        sheet1.Range[xlsRow, iPartyPlant].Text = dtRCMReceiviable.Rows[i]["PartyPlant"].ToString();
                        sheet1.Range[xlsRow, iGSTIN].Text = dtRCMReceiviable.Rows[i]["GSTIN"].ToString();
                        sheet1.Range[xlsRow, iVoucherType].Text = dtRCMReceiviable.Rows[i]["SourceType"].ToString();
                        sheet1.Range[xlsRow, iVoucherNo].Text = dtRCMReceiviable.Rows[i]["VoucherNo"].ToString();
                        sheet1.Range[xlsRow, iVoucherRef].Text = dtRCMReceiviable.Rows[i]["DocRefNo"].ToString();//TaxableAmount
                        sheet1.Range[xlsRow, iTaxableAmount].Number = clsStaticInfo.dbl(dtRCMReceiviable.Rows[i]["TaxableAmount"].ToString());//TaxableAmount
                        sheet1.Range[xlsRow, iTaxableAmount].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                        dtRCMReceiviable.DefaultView.RowFilter = "VoucherNo = '" + dtRCMReceiviable.Rows[i]["VoucherNo"].ToString() + "'";

                        if (dtTaxCode.Rows.Count > 0)
                        {
                            for (int j = 0; j < dtTaxCode.Rows.Count; j++)
                            {
                                dtRCMReceiviable.DefaultView.RowFilter = "VoucherNo = '" + dtRCMReceiviable.Rows[i]["VoucherNo"].ToString() + @"' AND Seq=" + dtRCMReceiviable.Rows[i]["Seq"].ToString() + @" AND  TaxCode = '" + dtTaxCode.Rows[j]["TaxCode"].ToString() + "'" + taxFitler;

                                //dtRCMReceiviable.DefaultView.RowFilter = "TaxCode = '" + dtTaxCode.Rows[j]["TaxCode"].ToString() + "'" + taxFitler;

                                for (int AKA = 0; AKA < dtRCMReceiviable.DefaultView.Count; AKA++)
                                {
                                    sheet1.Range[xlsRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])].Number = clsStaticInfo.dbl(dtRCMReceiviable.DefaultView[AKA]["CrAmount"].ToString());
                                    sheet1.Range[xlsRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                                    // xlsRow++;
                                }

                                if (dtRCMReceiviable.DefaultView.Count > 0)
                                {

                                    sheet1.Range[xlsRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])].Number = clsStaticInfo.dbl(dtRCMReceiviable.DefaultView[0]["CrAmount"].ToString());
                                    sheet1.Range[xlsRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                                }
                                else
                                {
                                    sheet1.Range[xlsRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])].Text = "-";
                                    sheet1.Range[xlsRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])].HorizontalAlignment = ExcelHAlign.HAlignRight;


                                }
                            }
                        }
                        Percentage = dtRCMReceiviable.Rows[i]["TaxPercentage"].ToString();
                        xlsRow++;
                    }


                    if (dtRCMReceiviable.Rows[i]["LineItemType"].ToString().ToUpper() == "GL")
                    {
                        voucherNo = dtRCMReceiviable.Rows[i]["VoucherNo"].ToString() + "-" + dtRCMReceiviable.Rows[i]["LineItemType"].ToString().ToUpper() + Particularstemp;

                    }
                    if (dtRCMReceiviable.Rows[i]["LineItemType"].ToString().ToUpper() == "MATERIAL")
                    {
                        voucherNo = dtRCMReceiviable.Rows[i]["VoucherNo"].ToString() + "-" + dtRCMReceiviable.Rows[i]["LineItemType"].ToString().ToUpper() + "-" + dtRCMReceiviable.Rows[i]["InventoryReceiveDetailId"].ToString().ToUpper() + Particularstemp;

                    }
                    if (dtRCMReceiviable.Rows[i]["LineItemType"].ToString().ToUpper() == "SERVICE")
                    {
                        voucherNo = dtRCMReceiviable.Rows[i]["VoucherNo"].ToString() + "-" + dtRCMReceiviable.Rows[i]["LineItemType"].ToString().ToUpper() + "-" + dtRCMReceiviable.Rows[i]["InventoryServiceId"].ToString().ToUpper() + Particularstemp;

                    }
                    if (dtRCMReceiviable.Rows[i]["LineItemType"].ToString().ToUpper() == "SALES")
                    {
                        voucherNo = dtRCMReceiviable.Rows[i]["VoucherNo"].ToString() + "-" + dtRCMReceiviable.Rows[i]["LineItemType"].ToString().ToUpper() + "-" + dtRCMReceiviable.Rows[i]["InventoryReceiveDetailId"].ToString().ToUpper() + Particularstemp;

                    }



                }
                sheet1[perStartRow, iPostingDate, xlsRow - 1, iPostingDate].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iTaxPercentage, xlsRow - 1, iTaxPercentage].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iParticulars, xlsRow - 1, iParticulars].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iPartyPlant, xlsRow - 1, iPartyPlant].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iGSTIN, xlsRow - 1, iGSTIN].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iVoucherType, xlsRow - 1, iVoucherType].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iVoucherNo, xlsRow - 1, iVoucherNo].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iVoucherRef, xlsRow - 1, iVoucherRef].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iTaxableAmount, xlsRow - 1, iTaxableAmount].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iTaxableAmount, xlsRow - 1, iTaxableAmount].BorderAround(ExcelLineStyle.Hair);


                if (dtTaxCode.Rows.Count > 0)
                {
                    for (int j = 0; j < dtTaxCode.Rows.Count; j++)
                    {
                        sheet1[perStartRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"]), xlsRow - 1, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])].BorderAround(ExcelLineStyle.Hair);

                    }
                }



                if (dtTaxCode.Rows.Count > 0)
                {
                    for (int j = 0; j < dtTaxCode.Rows.Count; j++)
                    {
                        sheet1[perStartRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"]), xlsRow - 1, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])].BorderAround(ExcelLineStyle.Hair);
                        formula2 = "SUM(" + clsStaticInfo.GetxlsCol(Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])) + perStartRow + ":" + clsStaticInfo.GetxlsCol(Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])) + (xlsRow - 1) + ")";
                        sheet1[xlsRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"]), xlsRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])].Formula = formula2;
                        dtTaxCode.Rows[j]["ColumnFormula"] += (clsStaticInfo.GetxlsCol(Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])) + xlsRow).ToString() + " + ";

                    }
                }
                sheet1.Range[xlsRow, 1, xlsRow, 1].Text = "Total";
                formula = "SUM(" + clsStaticInfo.GetxlsCol(iTaxableAmount) + perStartRow + ":" + clsStaticInfo.GetxlsCol(iTaxableAmount) + (xlsRow - 1) + ")";

                sheet1[xlsRow, iTaxableAmount, xlsRow, iTaxableAmount].Formula = formula;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Font.Bold = true;
                totalFormula += (clsStaticInfo.GetxlsCol(iTaxableAmount) + xlsRow).ToString() + "+";



                xlsRow++;
                xlsRow++;


                if (dtTaxCode.Rows.Count > 0)
                {
                    for (int j = 0; j < dtTaxCode.Rows.Count; j++)
                    {
                        //sheet1[perStartRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"]), xlsRow - 1, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])].BorderAround(ExcelLineStyle.Hair);
                        //formula2 = "SUM(" + clsStaticInfo.GetxlsCol(Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])) + perStartRow + ":" + clsStaticInfo.GetxlsCol(Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])) + (xlsRow - 1) + ")";
                        string fm = dtTaxCode.Rows[j]["ColumnFormula"].ToString().Trim();
                        sheet1[xlsRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"]), xlsRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])].Formula = fm.Remove(fm.Length - 1); //dtTaxCode.Rows[j]["ColumnFormula"].ToString().Remove(dtTaxCode.Rows[j]["ColumnFormula"].ToString().Length - 1);
                    }
                }
                sheet1.Range[xlsRow, 1, xlsRow, 1].Text = "Grand Total";

                sheet1[xlsRow, iTaxableAmount, xlsRow, iTaxableAmount].Formula = totalFormula.Remove(totalFormula.Length - 1);
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Font.Bold = true;




                #region ******************Report Header******************



                xlsRow = 1;
                xlsCol = 3;
                try
                {
                    if (companyLogo != null)
                    {

                        double totalWidth = sheet1.GetColumnWidth(1) + sheet1.GetColumnWidth(iGSTIN);
                        int totalWidthPixel = (int)(totalWidth * 7.5);
                        int totalheight = (int)((sheet1.GetRowHeight(1) + sheet1.GetRowHeight(2) + sheet1.GetRowHeight(3) + sheet1.GetRowHeight(3)) * 1.50);

                        companyLogo = ReportUtility.FixedSize(companyLogo, totalWidthPixel, totalheight);
                        IPictureShape pic = null;

                        pic = sheet1.Pictures.AddPicture(1, 1, companyLogo);
                        //pic.Height = 80;
                        //pic.Width = 220;
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }

                FactoryName = string.Empty;

                string FactoryAddress = string.Empty;

                if (dtCmp.Rows.Count > 0)
                {
                    CmpName = dtCmp.Rows[0]["CompanyName"].ToString();
                }
                else
                {
                    CmpName = "";
                }
                sheet1.Range[xlsRow, 3].Text = CmpName;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 12;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 17;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                if (dtFactory.Rows.Count > 0)
                {
                    //FactoryName = dsFactory.Tables[0].Rows[0]["PlantName"].ToString();
                    FactoryName = dtFactory.Rows[0]["UserName"].ToString();
                }
                else
                {
                    FactoryName = "";
                }
                sheet1.Range[xlsRow, 3].Text = FactoryName;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 14;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 18;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                if (dtFactory.Rows.Count > 0)
                {
                    FactoryAddress = dtFactory.Rows[0]["Address1"].ToString();
                }
                else
                {
                    FactoryAddress = "";
                }
                sheet1.Range[xlsRow, 3].Text = FactoryAddress;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                //sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 22;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                sheet1.Range[xlsRow, 3].Text = "RCM Tax Receivable Report From " + fromDate + " To " + toDate;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;


                #endregion ******************Report Header******************

                #region Freeze Panes

                sheet1.IsDisplayZeros = false;
                sheet1.UsedRange["A7"].FreezePanes();
                sheet1.FirstVisibleColumn = 1;
                sheet1.FirstVisibleRow = 6;

                #endregion Freeze Panes

                #region UsedRange Alignment

                sheet1.UsedRange.WrapText = false;
                sheet1.UsedRange.CellStyle.Font.Size = 10;
                sheet1.Range["A1"].CellStyle.Font.Size = 14;
                sheet1.Range["A2"].CellStyle.Font.Size = 10;
                sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;
                sheet1.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                #endregion UsedRange Alignment

                #region Page Setup
                sheet1.PageSetup.TopMargin = 0.5;
                sheet1.PageSetup.BottomMargin = 0.7;
                sheet1.PageSetup.PrintTitleRows = "$1:$5";
                sheet1.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                sheet1.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + name + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:MM tt").ToString();
                sheet1.PageSetup.LeftMargin = 0.5;
                sheet1.PageSetup.RightMargin = 0.2;
                sheet1.PageSetup.Orientation = ExcelPageOrientation.Portrait;
                sheet1.PageSetup.FitToPagesTall = 0;
                sheet1.PageSetup.FitToPagesWide = 1;
                sheet1.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                sheet1.IsDisplayZeros = false;
                #endregion Page Setup


                sheet1.Name = "RCM Receivable";
                return workbook;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public IWorkbook GetRCMReceivableSalesReport(string companyGroupId, string companyId, string plantId, string plantName, string fromDate, string toDate, string name)
        {
            clsReport objRpt = null;
            clsReport objRptSR = null;
            try
            {

                ExcelEngine excelEngine = null;
                IApplication application = null;
                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;
                excelEngine.Excel.DefaultVersion = ExcelVersion.Excel2013;
                var reportUtility = new ReportUtility();
                var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
                workbook.Version = ExcelVersion.Excel2013;
                var sheet1 = workbook.Worksheets[0];

                #region Logo
                string strPath = "";
                Image companyLogo = null;
                try
                {
                    DataTable dtCompanyImage = _sqlRepository.GetDataTable("SELECT * FROM ORG.COMPANY WHERE ID = '" + companyId + @"'");

                    strPath = Path.Combine(ResourcesPathReader.GetLogoOrImagePath(), dtCompanyImage.Rows[0]["Image"].ToString());  // IDCardEng.xlsx
                    companyLogo = Image.FromFile(strPath);
                }
                catch (Exception)
                {
                }

                #endregion
                objRpt = new clsReport();

                objRptSR = new clsReport(_sqlRepository);

                DataTable dtRCMReceiviableSales = null;
                string taxyearId = GetTaxYearId(fromDate, toDate, companyId);
                dtRCMReceiviableSales = GetRCMReceviableSales(companyGroupId, companyId, plantId, plantName, fromDate, toDate, taxyearId);
                if (dtRCMReceiviableSales.Rows.Count == 0)
                {
                    throw new Exception("No Data Found....");
                }

                DataTable dtCmp = objRptSR.SelectedCompanyDT(plantId);
                DataTable dtFactory = objRptSR.SelectedPlantDT(plantId);

                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;

                int xlsRow = 1, xlsCol = 1;
                int endXlsCol = 1;
                string FactoryName = "";
                string CmpName = "";
                xlsRow = 6;
                sheet1.Range[xlsRow - 1, 1].Text = "Report Ref No:";
                sheet1.Range[xlsRow - 1, 1].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow - 1, 1].RowHeight = 20;
                sheet1.Range[xlsRow - 1, 1].CellStyle.Font.Bold = true;



                int iPostingDate = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Date";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;


                xlsCol++;
                int iTaxPercentage = xlsCol; // Party
                sheet1.Range[xlsRow, xlsCol].Text = "Percentage";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignRight;

                xlsCol++;
                int iParticulars = xlsCol; // Party
                sheet1.Range[xlsRow, xlsCol].Text = "Particulars";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 40;

                xlsCol++;
                int iPartyPlant = xlsCol; // Party
                sheet1.Range[xlsRow, xlsCol].Text = "Party Plant";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 40;

                xlsCol++;
                int iGSTIN = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "GSTIN";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;

                xlsCol++;
                int iVoucherType = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Voucher Type";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                xlsCol++;
                int iVoucherNo = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Voucher No";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                xlsCol++;
                int iVoucherRef = xlsCol; // Doc Ref
                sheet1.Range[xlsRow, xlsCol].Text = "Voucher Ref";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;

                xlsCol++;
                int iTaxableAmount = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Taxable Amount";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;

                sheet1[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignRight;

                DataTable dtTaxCode = null;
                dtRCMReceiviableSales.DefaultView.Sort = "TCSequence";
                dtTaxCode = dtRCMReceiviableSales.DefaultView.ToTable(true, "TaxCode");
                dtTaxCode.Columns.Add("ColumnNumber", typeof(String));
                dtTaxCode.Columns.Add("ColumnFormula", typeof(String));

                if (dtTaxCode.Rows.Count > 0)
                {
                    for (int i = 0; i < dtTaxCode.Rows.Count; i++)
                    {
                        xlsCol++;
                        sheet1.Range[xlsRow, xlsCol].Text = dtTaxCode.Rows[i]["TaxCode"].ToString();
                        sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                        dtTaxCode.Rows[i]["ColumnNumber"] = xlsCol.ToString();
                    }
                }
                endXlsCol = xlsCol;

                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].WrapText = true;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 23;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;

                string voucherNo = "";
                string Percentage = "";
                int startRow = 0;
                int perStartRow = 0;
                string formula = "";
                string formula2 = "";
                string totalFormula = "";

                string lineItemPercentageType = "";
                xlsRow++;
                startRow = xlsRow;
                perStartRow = xlsRow;
                bool isFirst = true;
                string voucherNocomp = "";
                string taxFitler = "";


                for (int i = 0; i < dtRCMReceiviableSales.Rows.Count; i++)
                {
                    if (voucherNo != dtRCMReceiviableSales.Rows[i]["VoucherNo"].ToString() + "-" + dtRCMReceiviableSales.Rows[i]["LineItemType"].ToString().ToUpper())
                    {


                        if (dtRCMReceiviableSales.Rows[i]["LineItemType"].ToString().ToUpper() == "GL")
                        {
                            voucherNocomp = dtRCMReceiviableSales.Rows[i]["VoucherNo"].ToString() + "-" + dtRCMReceiviableSales.Rows[i]["LineItemType"].ToString().ToUpper();
                            taxFitler = " and VoucherNo = '" + dtRCMReceiviableSales.Rows[i]["VoucherNo"].ToString() + "' and LineItemType = '" + dtRCMReceiviableSales.Rows[i]["LineItemType"].ToString() + "'";
                        }
                        if (dtRCMReceiviableSales.Rows[i]["LineItemType"].ToString().ToUpper() == "MATERIAL")
                        {
                            voucherNocomp = dtRCMReceiviableSales.Rows[i]["VoucherNo"].ToString() + "-" + dtRCMReceiviableSales.Rows[i]["LineItemType"].ToString().ToUpper() + "-" + dtRCMReceiviableSales.Rows[i]["InventoryReceiveDetailId"].ToString().ToUpper();

                            taxFitler = " and VoucherNo = '" + dtRCMReceiviableSales.Rows[i]["VoucherNo"].ToString() + "' and LineItemType = '" + dtRCMReceiviableSales.Rows[i]["LineItemType"].ToString() + "' and InventoryReceiveDetailId = '" + dtRCMReceiviableSales.Rows[i]["InventoryReceiveDetailId"].ToString().ToUpper() + "'";

                        }
                        if (dtRCMReceiviableSales.Rows[i]["LineItemType"].ToString().ToUpper() == "SERVICE")
                        {
                            voucherNocomp = dtRCMReceiviableSales.Rows[i]["VoucherNo"].ToString() + "-" + dtRCMReceiviableSales.Rows[i]["LineItemType"].ToString().ToUpper() + "-" + dtRCMReceiviableSales.Rows[i]["InventoryServiceId"].ToString().ToUpper();
                            taxFitler = " and VoucherNo = '" + dtRCMReceiviableSales.Rows[i]["VoucherNo"].ToString() + "' and LineItemType = '" + dtRCMReceiviableSales.Rows[i]["LineItemType"].ToString() + "' and InventoryServiceId = '" + dtRCMReceiviableSales.Rows[i]["InventoryServiceId"].ToString().ToUpper() + "'";
                        }
                        if (dtRCMReceiviableSales.Rows[i]["LineItemType"].ToString().ToUpper() == "SALES")
                        {
                            voucherNocomp = dtRCMReceiviableSales.Rows[i]["VoucherNo"].ToString() + "-" + dtRCMReceiviableSales.Rows[i]["LineItemType"].ToString().ToUpper() + "-" + dtRCMReceiviableSales.Rows[i]["InventoryReceiveDetailId"].ToString().ToUpper();

                            taxFitler = " and VoucherNo = '" + dtRCMReceiviableSales.Rows[i]["VoucherNo"].ToString() + "' and LineItemType = '" + dtRCMReceiviableSales.Rows[i]["LineItemType"].ToString() + "' and InventoryReceiveDetailId = '" + dtRCMReceiviableSales.Rows[i]["InventoryReceiveDetailId"].ToString().ToUpper() + "'";

                        }
                        if (Percentage != dtRCMReceiviableSales.Rows[i]["TaxPercentage"].ToString())
                        {
                            if (isFirst == false)
                            {

                                sheet1[perStartRow, iPostingDate, xlsRow - 1, iPostingDate].BorderAround(ExcelLineStyle.Hair);
                                sheet1[perStartRow, iTaxPercentage, xlsRow - 1, iTaxPercentage].BorderAround(ExcelLineStyle.Hair);
                                sheet1[perStartRow, iParticulars, xlsRow - 1, iParticulars].BorderAround(ExcelLineStyle.Hair);
                                sheet1[perStartRow, iPartyPlant, xlsRow - 1, iPartyPlant].BorderAround(ExcelLineStyle.Hair);

                                sheet1[perStartRow, iGSTIN, xlsRow - 1, iGSTIN].BorderAround(ExcelLineStyle.Hair);
                                sheet1[perStartRow, iVoucherType, xlsRow - 1, iVoucherType].BorderAround(ExcelLineStyle.Hair);
                                sheet1[perStartRow, iVoucherNo, xlsRow - 1, iVoucherNo].BorderAround(ExcelLineStyle.Hair);
                                sheet1[perStartRow, iVoucherRef, xlsRow - 1, iVoucherRef].BorderAround(ExcelLineStyle.Hair);
                                sheet1[perStartRow, iTaxableAmount, xlsRow - 1, iTaxableAmount].BorderAround(ExcelLineStyle.Hair);
                                formula = "SUM(" + clsStaticInfo.GetxlsCol(iTaxableAmount) + perStartRow + ":" + clsStaticInfo.GetxlsCol(iTaxableAmount) + (xlsRow - 1) + ")";
                                formula2 = "";
                                if (dtTaxCode.Rows.Count > 0)
                                {
                                    for (int j = 0; j < dtTaxCode.Rows.Count; j++)
                                    {
                                        sheet1[perStartRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"]), xlsRow - 1, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])].BorderAround(ExcelLineStyle.Hair);
                                        formula2 = "SUM(" + clsStaticInfo.GetxlsCol(Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])) + perStartRow + ":" + clsStaticInfo.GetxlsCol(Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])) + (xlsRow - 1) + ")";
                                        sheet1[xlsRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"]), xlsRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])].Formula = formula2;

                                        dtTaxCode.Rows[j]["ColumnFormula"] += (clsStaticInfo.GetxlsCol(Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])) + xlsRow).ToString() + " + ";
                                    }
                                }
                                sheet1.Range[xlsRow, 1, xlsRow, 1].Text = "Total";
                                sheet1[xlsRow, iTaxableAmount, xlsRow, iTaxableAmount].Formula = formula;
                                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Font.Bold = true;
                                totalFormula += (clsStaticInfo.GetxlsCol(iTaxableAmount) + xlsRow).ToString() + "+";
                                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].NumberFormat = "#,##0.00;(#,##0.00)";

                                xlsRow++;


                            }
                            xlsRow++;
                            sheet1.Range[xlsRow - 1, 1].Number = clsStaticInfo.dbl(dtRCMReceiviableSales.Rows[i]["TaxPercentage"].ToString());
                            //sheet1.Range[xlsRow - 1, 1].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                            sheet1.Range[xlsRow - 1, 1].NumberFormat = "#,##0.00;(#,##0.00)";
                            perStartRow = xlsRow;
                            isFirst = false;

                        }

                        sheet1.Range[xlsRow, iPostingDate].Text = dtRCMReceiviableSales.Rows[i]["PostingDate"].ToString();
                        sheet1.Range[xlsRow, iTaxPercentage].Number = clsStaticInfo.dbl(dtRCMReceiviableSales.Rows[i]["TaxPercentage"].ToString());
                        sheet1.Range[xlsRow, iTaxPercentage].NumberFormat = reportUtility.NumberFormatDecimalTwo();

                        sheet1.Range[xlsRow, iParticulars].Text = dtRCMReceiviableSales.Rows[i]["PartyName"].ToString();
                        sheet1.Range[xlsRow, iPartyPlant].Text = dtRCMReceiviableSales.Rows[i]["PartyPlant"].ToString();
                        sheet1.Range[xlsRow, iGSTIN].Text = dtRCMReceiviableSales.Rows[i]["GSTIN"].ToString();
                        sheet1.Range[xlsRow, iVoucherType].Text = dtRCMReceiviableSales.Rows[i]["SourceType"].ToString();
                        sheet1.Range[xlsRow, iVoucherNo].Text = dtRCMReceiviableSales.Rows[i]["VoucherNo"].ToString();
                        sheet1.Range[xlsRow, iVoucherRef].Text = dtRCMReceiviableSales.Rows[i]["DocRefNo"].ToString();//TaxableAmount
                        sheet1.Range[xlsRow, iTaxableAmount].Number = clsStaticInfo.dbl(dtRCMReceiviableSales.Rows[i]["TaxableAmount"].ToString());//TaxableAmount
                        sheet1.Range[xlsRow, iTaxableAmount].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                        dtRCMReceiviableSales.DefaultView.RowFilter = "VoucherNo = '" + dtRCMReceiviableSales.Rows[i]["VoucherNo"].ToString() + "'";

                        if (dtTaxCode.Rows.Count > 0)
                        {
                            for (int j = 0; j < dtTaxCode.Rows.Count; j++)
                            {
                                dtRCMReceiviableSales.DefaultView.RowFilter = "TaxCode = '" + dtTaxCode.Rows[j]["TaxCode"].ToString() + "'" + taxFitler;
                                if (dtRCMReceiviableSales.DefaultView.Count > 0)
                                {

                                    sheet1.Range[xlsRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])].Number = clsStaticInfo.dbl(dtRCMReceiviableSales.DefaultView[0]["CrAmount"].ToString());
                                    sheet1.Range[xlsRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                                }
                                else
                                {
                                    sheet1.Range[xlsRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])].Text = "-";
                                    sheet1.Range[xlsRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])].HorizontalAlignment = ExcelHAlign.HAlignRight;


                                }
                            }
                        }
                        Percentage = dtRCMReceiviableSales.Rows[i]["TaxPercentage"].ToString();
                        xlsRow++;
                    }

                    //voucherNo = dtRCMPayable.Rows[i]["VoucherNo"].ToString();
                    if (dtRCMReceiviableSales.Rows[i]["LineItemType"].ToString().ToUpper() == "GL")
                    {
                        voucherNo = dtRCMReceiviableSales.Rows[i]["VoucherNo"].ToString() + "-" + dtRCMReceiviableSales.Rows[i]["LineItemType"].ToString().ToUpper();

                    }
                    if (dtRCMReceiviableSales.Rows[i]["LineItemType"].ToString().ToUpper() == "MATERIAL")
                    {
                        voucherNo = dtRCMReceiviableSales.Rows[i]["VoucherNo"].ToString() + "-" + dtRCMReceiviableSales.Rows[i]["LineItemType"].ToString().ToUpper() + "-" + dtRCMReceiviableSales.Rows[i]["InventoryReceiveDetailId"].ToString().ToUpper();

                    }
                    if (dtRCMReceiviableSales.Rows[i]["LineItemType"].ToString().ToUpper() == "SERVICE")
                    {
                        voucherNo = dtRCMReceiviableSales.Rows[i]["VoucherNo"].ToString() + "-" + dtRCMReceiviableSales.Rows[i]["LineItemType"].ToString().ToUpper() + "-" + dtRCMReceiviableSales.Rows[i]["InventoryServiceId"].ToString().ToUpper();

                    }
                    if (dtRCMReceiviableSales.Rows[i]["LineItemType"].ToString().ToUpper() == "SALES")
                    {
                        voucherNo = dtRCMReceiviableSales.Rows[i]["VoucherNo"].ToString() + "-" + dtRCMReceiviableSales.Rows[i]["LineItemType"].ToString().ToUpper() + "-" + dtRCMReceiviableSales.Rows[i]["InventoryReceiveDetailId"].ToString().ToUpper();

                    }


                }
                sheet1[perStartRow, iPostingDate, xlsRow - 1, iPostingDate].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iTaxPercentage, xlsRow - 1, iTaxPercentage].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iParticulars, xlsRow - 1, iParticulars].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iPartyPlant, xlsRow - 1, iPartyPlant].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iGSTIN, xlsRow - 1, iGSTIN].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iVoucherType, xlsRow - 1, iVoucherType].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iVoucherNo, xlsRow - 1, iVoucherNo].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iVoucherRef, xlsRow - 1, iVoucherRef].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iTaxableAmount, xlsRow - 1, iTaxableAmount].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iTaxableAmount, xlsRow - 1, iTaxableAmount].BorderAround(ExcelLineStyle.Hair);


                if (dtTaxCode.Rows.Count > 0)
                {
                    for (int j = 0; j < dtTaxCode.Rows.Count; j++)
                    {
                        sheet1[perStartRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"]), xlsRow - 1, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])].BorderAround(ExcelLineStyle.Hair);

                    }
                }



                if (dtTaxCode.Rows.Count > 0)
                {
                    for (int j = 0; j < dtTaxCode.Rows.Count; j++)
                    {
                        sheet1[perStartRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"]), xlsRow - 1, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])].BorderAround(ExcelLineStyle.Hair);
                        formula2 = "SUM(" + clsStaticInfo.GetxlsCol(Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])) + perStartRow + ":" + clsStaticInfo.GetxlsCol(Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])) + (xlsRow - 1) + ")";
                        sheet1[xlsRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"]), xlsRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])].Formula = formula2;
                        dtTaxCode.Rows[j]["ColumnFormula"] += (clsStaticInfo.GetxlsCol(Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])) + xlsRow).ToString() + " + ";

                    }
                }
                sheet1.Range[xlsRow, 1, xlsRow, 1].Text = "Total";
                formula = "SUM(" + clsStaticInfo.GetxlsCol(iTaxableAmount) + perStartRow + ":" + clsStaticInfo.GetxlsCol(iTaxableAmount) + (xlsRow - 1) + ")";

                sheet1[xlsRow, iTaxableAmount, xlsRow, iTaxableAmount].Formula = formula;
                sheet1[xlsRow, iTaxableAmount, xlsRow, iTaxableAmount].NumberFormat = "#,##0.00;(#,##0.00)";

                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Font.Bold = true;
                totalFormula += (clsStaticInfo.GetxlsCol(iTaxableAmount) + xlsRow).ToString() + "+";
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].NumberFormat = "#,##0.00;(#,##0.00)";




                xlsRow++;
                xlsRow++;


                if (dtTaxCode.Rows.Count > 0)
                {
                    for (int j = 0; j < dtTaxCode.Rows.Count; j++)
                    {
                        //sheet1[perStartRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"]), xlsRow - 1, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])].BorderAround(ExcelLineStyle.Hair);
                        //formula2 = "SUM(" + clsStaticInfo.GetxlsCol(Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])) + perStartRow + ":" + clsStaticInfo.GetxlsCol(Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])) + (xlsRow - 1) + ")";
                        string fm = dtTaxCode.Rows[j]["ColumnFormula"].ToString().Trim();
                        sheet1[xlsRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"]), xlsRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])].Formula = fm.Remove(fm.Length - 1); //dtTaxCode.Rows[j]["ColumnFormula"].ToString().Remove(dtTaxCode.Rows[j]["ColumnFormula"].ToString().Length - 1);
                    }
                }
                sheet1.Range[xlsRow, 1, xlsRow, 1].Text = "Grand Total";

                sheet1[xlsRow, iTaxableAmount, xlsRow, iTaxableAmount].Formula = totalFormula.Remove(totalFormula.Length - 1);
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].NumberFormat = "#,##0.00;(#,##0.00)";




                #region ******************Report Header******************



                xlsRow = 1;
                xlsCol = 3;
                try
                {
                    if (companyLogo != null)
                    {

                        double totalWidth = sheet1.GetColumnWidth(1) + sheet1.GetColumnWidth(iGSTIN);
                        int totalWidthPixel = (int)(totalWidth * 7.5);
                        int totalheight = (int)((sheet1.GetRowHeight(1) + sheet1.GetRowHeight(2) + sheet1.GetRowHeight(3) + sheet1.GetRowHeight(3)) * 1.50);

                        companyLogo = ReportUtility.FixedSize(companyLogo, totalWidthPixel, totalheight);
                        IPictureShape pic = null;

                        pic = sheet1.Pictures.AddPicture(1, 1, companyLogo);
                        //pic.Height = 80;
                        //pic.Width = 220;
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }

                FactoryName = string.Empty;

                string FactoryAddress = string.Empty;

                if (dtCmp.Rows.Count > 0)
                {
                    CmpName = dtCmp.Rows[0]["CompanyName"].ToString();
                }
                else
                {
                    CmpName = "";
                }
                sheet1.Range[xlsRow, 3].Text = CmpName;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 12;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 17;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                if (dtFactory.Rows.Count > 0)
                {
                    //FactoryName = dsFactory.Tables[0].Rows[0]["PlantName"].ToString();
                    FactoryName = dtFactory.Rows[0]["UserName"].ToString();
                }
                else
                {
                    FactoryName = "";
                }
                sheet1.Range[xlsRow, 3].Text = FactoryName;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 14;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 18;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                if (dtFactory.Rows.Count > 0)
                {
                    FactoryAddress = dtFactory.Rows[0]["Address1"].ToString();
                }
                else
                {
                    FactoryAddress = "";
                }
                sheet1.Range[xlsRow, 3].Text = FactoryAddress;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                //sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 22;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                sheet1.Range[xlsRow, 3].Text = "RCM Tax Receivable Sales Report From " + fromDate + " To " + toDate;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;


                #endregion ******************Report Header******************

                #region Freeze Panes

                sheet1.IsDisplayZeros = false;
                sheet1.UsedRange["A7"].FreezePanes();
                sheet1.FirstVisibleColumn = 1;
                sheet1.FirstVisibleRow = 6;

                #endregion Freeze Panes

                #region UsedRange Alignment

                sheet1.UsedRange.WrapText = false;
                sheet1.UsedRange.CellStyle.Font.Size = 10;
                sheet1.Range["A1"].CellStyle.Font.Size = 14;
                sheet1.Range["A2"].CellStyle.Font.Size = 10;
                sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;
                sheet1.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                #endregion UsedRange Alignment

                #region Page Setup
                sheet1.PageSetup.TopMargin = 0.5;
                sheet1.PageSetup.BottomMargin = 0.7;
                sheet1.PageSetup.PrintTitleRows = "$1:$5";
                sheet1.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                sheet1.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + name + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:MM tt").ToString();
                sheet1.PageSetup.LeftMargin = 0.5;
                sheet1.PageSetup.RightMargin = 0.2;
                sheet1.PageSetup.Orientation = ExcelPageOrientation.Portrait;
                sheet1.PageSetup.FitToPagesTall = 0;
                sheet1.PageSetup.FitToPagesWide = 1;
                sheet1.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                sheet1.IsDisplayZeros = false;
                #endregion Page Setup


                sheet1.Name = "RCM Receivable Sales";
                return workbook;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private DataTable GetRCMReceviable(string companyGroupId, string companyId, string plantId, string plantName, string fromDate, string toDate, string taxyearId)
        {
            string strSql = "";
            strSql = @"select DENSE_RANK() over(partition by VoucherNo,TaxCode order by Id) AS Seq,* from
                       (SELECT SourceType= CASE WHEN V.SourceType='VendorInvoice' THEN 'Inbound Invoice'
						                WHEN V.SourceType='VendorPayment' THEN 'Vendor Payment'
						                WHEN V.SourceType='InventoryPayable' THEN 'Purchase' ELSE '' END
                ,V.VoucherNo,Format(V.PostingDate,'dd-MMM-yyyy') PostingDate,V.DocRefNo,V.DocDate,P.UserName PartyName,P.TINNO GSTIN 
                ,LineItemType=case when v.SourceType='InventoryPayable' then 'Material' 
				                   when v.SourceType='VendorInvoice' then 'GL'
				                   when v.SourceType='VendorPayment' then 'GL'
				                   ELSE '' END
				                   ,Particular=case when v.SourceType='InventoryPayable' then MM.UserName 
									                WHEN v.SourceType='VendorInvoice' THEN A.UserName
									                WHEN v.SourceType='VendorPayment' THEN AP.UserName
				                   else '' end
								   ,PP.UserName as  PartyPlant
                ,TaxableAmount= CASE WHEN v.SourceType='InventoryPayable' THEN IRD.TotalMaterialTranAmount
					                WHEN v.SourceType='VendorInvoice' THEN VD.DrAmount	
					                WHEN v.SourceType='VendorPayment' THEN IWD.Amount	ELSE 0 END
                        ,TaxPercentage= CASE WHEN v.SourceType='VendorInvoice' THEN taxc.ValueOfFixed
												  ELSE '' END
                ,IT.Id,0 DrAmount ,CrAmount=CASE WHEN ITD.AType='Dr' THEN IT.TaxAmount ELSE 0 END
                ,TC.Code +' ' + 'RCM' TaxCode ,TC.Sequence TCSequence,TC.TaxCategoryType,TC.UserName+'-'+TC.Code TaxCategory,IsNULL(TAXC.IsRCM,0) IsRCM,TAXC.UserName TaxCodeName
                ,IsNULL(IV.IsExcludingTax,0) IsExcludingTax,IsNULL(IR.IsTaxApplicable,0) IsTaxApplicable,TAXC.[Type],TAXC.ValueOfFixed
                ,HSNP.[Percentage],MM.HSNCodeId,MM.UserName Material,NULL InventoryReceiveDetailId, NULL InventoryServiceId

                FROM  TRN.InvoiceTaxDetail ITD   
				LEFT JOIN TRN.InvoiceTax IT ON IT.Id=ITD.InvoiceTaxId
                LEFT JOIN TRN.Voucher V ON V.Id=IT.VoucherId
                LEFT JOIN TRN.Invoice IV ON IV.Id=IT.InvoiceId
                LEFT JOIN TRN.InvoiceWriteOff IW ON IW.Id=IT.InvoiceWriteOffId
                LEFT JOIN HKP.Party P ON P.Id=IT.PartyId
				LEFT JOIN HKP.PartyPlant as PP on PP.Id=IT.PartyPlantId

                LEFT JOIN MST.TaxCategory TC ON TC.Id=IT.TaxCategoryId
                LEFT JOIN( select distinct TAC.Id,TAC.UserName,TAC.IsRCM,TAY.[Type],TACD.ValueOfFixed from MST.TaxCode TAC 
	            LEFT JOIN MST.TaxCodeYear TAY ON TAY.TaxCodeId=TAC.Id
	            LEFT JOIN MST.TaxCodeDetail TACD ON TACD.TaxCodeId=TAC.Id WHERE TAY.TaxYearId IN ('','3')) TAXC ON TAXC.Id=IT.TaxCodeId
                --LEFT JOIN SCS.TaxYear TY ON TY.Id=TAY.TaxYearId
                LEFT JOIN TRN.InventoryReceive IR ON IR.VoucherId=V.Id
                LEFT JOIN TRN.InventoryReceiveTax IRT ON IRT.InventoryReceiveId=IR.Id AND IRT.TaxCategoryId=IT.TaxCategoryId
                LEFT JOIN MST.HSNTaxPercentage HSNP ON  IRT.HSNCodeId=HSNP.HSNCodeId AND HSNP.TaxCategoryId=IT.TaxCategoryId 
                LEFT JOIN TRN.InventoryReceiveDetail IRD ON IRD.InventoryReceiveId=IR.Id
                LEFT JOIN TRN.InventoryMaterial IM ON IM.Id=IRD.InventoryMaterialId
                LEFT JOIN MST.MaterialMaster MM ON MM.Id=IM.MaterialMasterId
                LEFT JOIN TRN.VoucherDetail VD ON VD.Id=IT.VoucherDetailId 
                LEFT JOIN HKP.Activity A ON A.Id=VD.ActivityId 
                LEFT JOIN (SELECT IW.InvoiceWriteOffId,IW.ActivityId,SUM(I.Amount) Amount FROM TRN.InvoiceWriteOffDetail IW 
			                JOIN TRN.Invoice I ON I.Id=IW.InvoiceId
		                GROUP BY InvoiceWriteOffId,ActivityId) IWD ON IWD.InvoiceWriteOffId=IT.InvoiceWriteOffId
                LEFT JOIN HKP.Activity AP ON AP.Id=IWD.ActivityId
                where TAXC.IsRCM=1 AND V.PostingDate between '" + fromDate + "' AND '" + toDate + @"' and V.PlantId = '" + plantId + @"' and V.IsPark=0
                AND ITD.AType='Dr'
                and V.SourceType in ('VendorInvoice','VendorPayment') 
                 UNION ALL
				select 'Purchase' SourceType
                ,V.VoucherNo,Format(V.PostingDate,'dd-MMM-yyyy') PostingDate,V.DocRefNo,V.DocDate,P.UserName PartyName,P.TINNO GSTIN 
                ,LineItemType=case when v.SourceType='InventoryPayable' then 'Material' 
				                   else '' end
				                   ,Particular=case when v.SourceType='InventoryPayable' then MM.UserName 
				                   else '' end
								   ,PP.UserName as PartyPlant
                ,TaxableAmount=case when v.SourceType='InventoryPayable' then IRD.MaterialTranAmount
					                	else 0 end
                ,TaxPercentage= case when v.SourceType='InventoryPayable' then IRT.Percentage
												  else '' end
                ,IT.Id,0 DrAmount ,CrAmount=case when ITD.AType='Dr' then IRT.TaxAmount else 0 end
                ,TC.Code +' '+ 'RCM' TaxCode, TC.Sequence TCSequence,TC.TaxCategoryType,TC.UserName+'-'+TC.Code TaxCategory,IsNULL(TAXC.IsRCM,0) IsRCM,TAXC.UserName TaxCodeName
                ,IsNULL(IV.IsExcludingTax,0) IsExcludingTax,IsNULL(IR.IsTaxApplicable,0) IsTaxApplicable,TAXC.[Type],TAXC.ValueOfFixed
                ,IRT.[Percentage],MM.HSNCodeId,MM.UserName Material,IRT.InventoryReceiveDetailId, IRT.InventoryServiceId

                FROM  TRN.InvoiceTaxDetail ITD   
				LEFT JOIN TRN.InvoiceTax IT ON IT.Id=ITD.InvoiceTaxId
                LEFT JOIN TRN.Voucher V ON V.Id=IT.VoucherId
                LEFT JOIN TRN.Invoice IV ON IV.Id=IT.InvoiceId
                LEFT JOIN TRN.InvoiceWriteOff IW ON IW.Id=IT.InvoiceWriteOffId
                LEFT JOIN HKP.Party P ON P.Id=IT.PartyId
                LEFT JOIN HKP.PartyPlant  PP ON PP.Id=IT.PartyPlantId

                LEFT JOIN MST.TaxCategory TC ON TC.Id=IT.TaxCategoryId
                LEFT JOIN( select distinct TAC.Id,TAC.UserName,TAC.IsRCM,TAY.[Type],TACD.ValueOfFixed from MST.TaxCode TAC 
	                LEFT JOIN MST.TaxCodeYear TAY ON TAY.TaxCodeId=TAC.Id
	                LEFT JOIN MST.TaxCodeDetail TACD ON TACD.TaxCodeId=TAC.Id WHERE TAY.TaxYearId IN ('','3')) TAXC ON TAXC.Id=IT.TaxCodeId
                --LEFT JOIN SCS.TaxYear TY ON TY.Id=TAY.TaxYearId
                LEFT JOIN TRN.InventoryReceive IR ON IR.VoucherId=V.Id
                LEFT JOIN TRN.InventoryReceiveTax IRT ON IRT.InventoryReceiveId=IR.Id AND IRT.TaxCategoryId=IT.TaxCategoryId
                
                LEFT JOIN TRN.InventoryReceiveDetail IRD ON IRD.Id=IRT.InventoryReceiveDetailId
                LEFT JOIN TRN.InventoryMaterial IM ON IM.Id=IRD.InventoryMaterialId
                LEFT JOIN MST.MaterialMaster MM ON MM.Id=IM.MaterialMasterId
                LEFT JOIN TRN.VoucherDetail VD ON VD.Id=IT.VoucherDetailId 
                LEFT JOIN HKP.Activity A ON A.Id=VD.ActivityId 
                LEFT JOIN (SELECT IW.InvoiceWriteOffId,IW.ActivityId,SUM(I.Amount) Amount FROM TRN.InvoiceWriteOffDetail IW 
			                JOIN TRN.Invoice I ON I.Id=IW.InvoiceId
		                   GROUP BY InvoiceWriteOffId,ActivityId) IWD ON IWD.InvoiceWriteOffId=IT.InvoiceWriteOffId
                LEFT JOIN HKP.Activity AP ON AP.Id=IWD.ActivityId
                where IR.IsTaxApplicable=1 AND V.PostingDate between '" + fromDate + "' AND '" + toDate + @"'
				and V.PlantId = '" + plantId + @"' and V.IsPark=0 and IRT.InventoryServiceId IS NULL and v.SourceType='InventoryPayable' AND ITD.AType='Dr'
                UNION ALL

				select 'Purchase' SourceType
                ,V.VoucherNo,Format(V.PostingDate,'dd-MMM-yyyy') PostingDate,V.DocRefNo,V.DocDate,P.UserName PartyName,P.TINNO GSTIN 
                ,LineItemType=case when v.SourceType='InventoryPayable' then 'Service' 
				                   else '' end
				                   ,Particular=case when v.SourceType='InventoryPayable' then SM.UserName 
				                   else '' end
								   ,PP.UserName as PartyPlant
                ,TaxableAmount=case when v.SourceType='InventoryPayable' then IRD.Amount
					                	else 0 end
                ,TaxPercentage= case when v.SourceType='InventoryPayable' then IRT.Percentage
												  else '' end
                ,IT.Id,0 DrAmount ,CrAmount=case when ITD.AType='Dr' then IRT.TaxAmount else 0 end
                ,TC.Code +' '+ 'RCM' TaxCode, TC.Sequence TCSequence,TC.TaxCategoryType,TC.UserName+'-'+TC.Code TaxCategory,IsNULL(TAXC.IsRCM,0) IsRCM,TAXC.UserName TaxCodeName
                ,IsNULL(IV.IsExcludingTax,0) IsExcludingTax,IsNULL(IR.IsTaxApplicable,0) IsTaxApplicable,TAXC.[Type],TAXC.ValueOfFixed
                ,IRT.[Percentage],IRT.HSNCodeId,SM.UserName Material,IRT.InventoryReceiveDetailId, IRT.InventoryServiceId

                FROM  TRN.InvoiceTaxDetail ITD   
				LEFT JOIN TRN.InvoiceTax IT ON IT.Id=ITD.InvoiceTaxId
                LEFT JOIN TRN.Voucher V ON V.Id=IT.VoucherId
                LEFT JOIN TRN.Invoice IV ON IV.Id=IT.InvoiceId
                LEFT JOIN TRN.InvoiceWriteOff IW ON IW.Id=IT.InvoiceWriteOffId
                LEFT JOIN HKP.Party P ON P.Id=IT.PartyId
                LEFT JOIN HKP.PartyPlant PP ON PP.Id=IT.PartyPlantId
                LEFT JOIN MST.TaxCategory TC ON TC.Id=IT.TaxCategoryId
                LEFT JOIN( select distinct TAC.Id,TAC.UserName,TAC.IsRCM,TAY.[Type],TACD.ValueOfFixed from MST.TaxCode TAC 
	                LEFT JOIN MST.TaxCodeYear TAY ON TAY.TaxCodeId=TAC.Id
	                LEFT JOIN MST.TaxCodeDetail TACD ON TACD.TaxCodeId=TAC.Id WHERE TAY.TaxYearId IN ('','3')) TAXC ON TAXC.Id=IT.TaxCodeId
                LEFT JOIN TRN.InventoryReceive IR ON IR.VoucherId = V.Id
                            LEFT JOIN TRN.InventoryReceiveTax IRT ON IRT.InventoryReceiveId = IR.Id AND IRT.TaxCategoryId = IT.TaxCategoryId
                            LEFT JOIN TRN.InventoryService IRD ON IRD.Id = IRT.InventoryServiceId
							LEFT JOIN hkp.ServiceMaster SM ON SM.Id = IRD.ServiceMasterId
                LEFT JOIN TRN.VoucherDetail VD ON VD.Id=IT.VoucherDetailId 
                LEFT JOIN HKP.Activity A ON A.Id=VD.ActivityId 
                LEFT JOIN (SELECT IW.InvoiceWriteOffId,IW.ActivityId,SUM(I.Amount) Amount FROM TRN.InvoiceWriteOffDetail IW 
			                JOIN TRN.Invoice I ON I.Id=IW.InvoiceId
		                GROUP BY InvoiceWriteOffId,ActivityId) IWD ON IWD.InvoiceWriteOffId=IT.InvoiceWriteOffId
                LEFT JOIN HKP.Activity AP ON AP.Id=IWD.ActivityId
                WHERE IR.IsTaxApplicable=1 AND V.PostingDate  between '" + fromDate + "' AND '" + toDate + @"'
				AND V.PlantId = '" + plantId + @"'  AND V.IsPark=0 AND IRT.InventoryReceiveDetailId IS NULL AND v.SourceType='InventoryPayable' AND ITD.AType='Dr'    
				
) K
                ORDER BY TaxPercentage,LineItemType";

            return _sqlRepository.GetDataTable(strSql);

        }

        private DataTable GetRCMReceviableSales(string companyGroupId, string companyId, string plantId, string plantName, string fromDate, string toDate, string taxyearId)
        {
            string strSql = "";
            strSql = @"SELECT * FROM 
                       (SELECT SourceType= CASE WHEN V.SourceType='CustomerInvoice' THEN 'Outbound Invoice'
						                WHEN V.SourceType='CustomerReceipt ' THEN 'Customer Receipt'
						                ELSE '' END
                ,V.VoucherNo,Format(V.PostingDate,'dd-MMM-yyyy') PostingDate,V.DocRefNo,V.DocDate,P.UserName PartyName,P.TINNO GSTIN 
                ,LineItemType=case 
				                   when v.SourceType='CustomerInvoice' then 'GL'
				                   when v.SourceType='CustomerReceipt' then 'GL'
				                   ELSE '' END
				                   ,Particular=case 
									                WHEN v.SourceType='CustomerInvoice' THEN A.UserName
									                WHEN v.SourceType='CustomerReceipt' THEN AP.UserName
				                   else '' end
								   ,PP.UserName as  PartyPlant
                ,TaxableAmount= CASE
					                WHEN v.SourceType='CustomerInvoice' THEN VD.DrAmount	
					                WHEN v.SourceType='CustomerReceipt' THEN IWD.Amount	ELSE 0 END
                        ,TaxPercentage= CASE WHEN v.SourceType='CustomerInvoice' THEN taxc.ValueOfFixed
												  ELSE '' END
                ,IT.Id,0 DrAmount ,CrAmount=CASE WHEN ITD.AType='Dr' THEN IT.TaxAmount ELSE 0 END
                ,TC.Code +' ' + 'RCM' TaxCode ,TC.Sequence TCSequence,TC.TaxCategoryType,TC.UserName+'-'+TC.Code TaxCategory,IsNULL(TAXC.IsRCM,0) IsRCM,TAXC.UserName TaxCodeName
                ,IsNULL(IV.IsExcludingTax,0) IsExcludingTax,IsNULL(IR.IsTaxApplicable,0) IsTaxApplicable,TAXC.[Type],TAXC.ValueOfFixed
                ,HSNP.[Percentage],MM.HSNCodeId,MM.UserName Material,NULL InventoryReceiveDetailId, NULL InventoryServiceId

                from TRN.InvoiceTax IT 
                left join TRN.InvoiceTaxDetail ITD  ON IT.Id=ITD.InvoiceTaxId AND ITD.AType='Dr'
                LEFT JOIN TRN.Voucher V ON V.Id=IT.VoucherId
                LEFT JOIN TRN.Invoice IV ON IV.Id=IT.InvoiceId
                LEFT JOIN TRN.InvoiceWriteOff IW ON IW.Id=IT.InvoiceWriteOffId
                LEFT JOIN HKP.Party P ON P.Id=IT.PartyId
				LEFT JOIN HKP.PartyPlant as PP on PP.Id=IT.PartyPlantId

                LEFT JOIN MST.TaxCategory TC ON TC.Id=IT.TaxCategoryId
                LEFT JOIN( select TAC.Id,TAC.UserName,TAC.IsRCM,TAY.[Type],TACD.ValueOfFixed from MST.TaxCode TAC 
	            LEFT JOIN MST.TaxCodeYear TAY ON TAY.TaxCodeId=TAC.Id
	            LEFT JOIN MST.TaxCodeDetail TACD ON TACD.TaxCodeId=TAC.Id WHERE TAY.TaxYearId IN (" + taxyearId + @")) TAXC ON TAXC.Id=IT.TaxCodeId
                --LEFT JOIN SCS.TaxYear TY ON TY.Id=TAY.TaxYearId
                LEFT JOIN TRN.InventoryReceive IR ON IR.VoucherId=V.Id
                LEFT JOIN TRN.InventoryReceiveTax IRT ON IRT.InventoryReceiveId=IR.Id AND IRT.TaxCategoryId=IT.TaxCategoryId
                LEFT JOIN MST.HSNTaxPercentage HSNP ON  IRT.HSNCodeId=HSNP.HSNCodeId AND HSNP.TaxCategoryId=IT.TaxCategoryId 
                LEFT JOIN TRN.InventoryReceiveDetail IRD ON IRD.InventoryReceiveId=IR.Id
                LEFT JOIN TRN.InventoryMaterial IM ON IM.Id=IRD.InventoryMaterialId
                LEFT JOIN MST.MaterialMaster MM ON MM.Id=IM.MaterialMasterId
                LEFT JOIN TRN.VoucherDetail VD ON VD.Id=IT.VoucherDetailId 
                LEFT JOIN HKP.Activity A ON A.Id=VD.ActivityId 
                LEFT JOIN (SELECT IW.InvoiceWriteOffId,IW.ActivityId,SUM(I.Amount) Amount FROM TRN.InvoiceWriteOffDetail IW 
			                JOIN TRN.Invoice I ON I.Id=IW.InvoiceId
		                GROUP BY InvoiceWriteOffId,ActivityId) IWD ON IWD.InvoiceWriteOffId=IT.InvoiceWriteOffId
                LEFT JOIN HKP.Activity AP ON AP.Id=IWD.ActivityId
                where TAXC.IsRCM=1 AND V.PostingDate between '" + fromDate + "' AND '" + toDate + @"' and V.PlantId = '" + plantId + @"' and V.IsPark=0
                and V.SourceType in ('CustomerInvoice','CustomerReceipt') AND ITD.AType='Dr'

        union all
				
                select 'Sales' SourceType
                ,V.VoucherNo,Format(V.PostingDate,'dd-MMM-yyyy') PostingDate,V.DocRefNo,V.DocDate,P.UserName PartyName,PP.GSTIN
                ,LineItemType=case when v.SourceType='SalesInvoice' then 'Sales' 
				                   else '' end
				  ,Particular=case when v.SourceType='SalesInvoice' then MM.UserName 
				                   else '' end
								   ,PP.UserName as PartyPlant
               ,TaxableAmount=case when v.SourceType='SaleSInvoice' then ISNULL(SM.BaseAmount,0)
					                	else 0 end
										,ISNULL(IRT.[Percentage],0) TaxPercentage
										,IT.Id
                ,0 DrAmount ,CrAmount=case when ITD.AType='Dr' then ISNULL( IRT.Amount,0) else 0 end
                ,TC.Code +' '+ 'RCM' TaxCode, TC.Sequence TCSequence,TC.TaxCategoryType,TC.UserName+'-'+TC.Code TaxCategory
				,IsRCM=CASE WHEN IsNULL(CP.TaxApplicable,'')='Mandatory' THEN 1 ELSE 0 END,NULL TaxCodeName
                ,IsNULL(IV.IsExcludingTax,0) IsExcludingTax,IsTaxApplicable=CASE WHEN IsNULL(CP.TaxApplicable,'')='Mandatory' THEN 1 ELSE 0 END,NULL [Type],0 ValueOfFixed
                ,IRT.[Percentage],IRT.HSNCodeId,MM.UserName Material,IRT.SalesMaterialId InventoryReceiveDetailId, IRT.SalesServiceId InventoryServiceId

                from TRN.InvoiceTax IT 
                left join TRN.InvoiceTaxDetail ITD  ON IT.Id=ITD.InvoiceTaxId AND ITD.AType='Dr'
                LEFT JOIN TRN.Voucher V ON V.Id=IT.VoucherId
                LEFT JOIN TRN.Invoice IV ON IV.Id=IT.InvoiceId
                LEFT JOIN HKP.Party P ON P.Id=IT.PartyId
                 LEFT JOIN HKP.PartyPlant PP ON PP.Id=IT.PartyPlantId
              
				LEFT JOIN HKP.CompanyParty CP ON CP.PartyId=P.Id AND CP.PartyType='Customer' AND CP.PlantId = '" + plantId + @"'
                LEFT JOIN MST.TaxCategory TC ON TC.Id=IT.TaxCategoryId
                
                LEFT JOIN TRN.Sales IR ON IR.VoucherId = V.Id
                            LEFT JOIN TRN.SalesTax IRT ON IRT.SalesId = IR.Id AND IRT.TaxCategoryId = IT.TaxCategoryId
							LEFT JOIN TRN.SalesMaterial SM ON SM.Id = IRT.SalesMaterialId
							LEFT JOIN MST.MaterialMaster MM ON MM.Id = SM.MaterialMasterId
                LEFT JOIN TRN.VoucherDetail VD ON VD.Id=IT.VoucherDetailId 
                LEFT JOIN HKP.Activity A ON A.Id=VD.ActivityId 
                
                where CP.TaxApplicable='Mandatory' AND V.PostingDate between '" + fromDate + "' AND '" + toDate + @"'
				and V.PlantId = '" + plantId + @"'  and V.IsPark=0 and IRT.SalesServiceId IS NULL and v.SourceType='SalesInvoice' AND ITD.AType='Dr'
                
 union ALL
				select 'SalesService' SourceType
                ,V.VoucherNo,Format(V.PostingDate,'dd-MMM-yyyy') PostingDate,V.DocRefNo,V.DocDate,P.UserName PartyName
				,PP.GSTIN
                ,LineItemType=case when v.SourceType='SalesInvoice' then 'SalesService' 
				                   else '' end
				  ,Particular=case when v.SourceType='SalesInvoice' then SM.UserName 
				                   else '' end
								  , PP.username as PartyPlant
               ,TaxableAmount=case when v.SourceType='SaleSInvoice' then ISNULL(SS.Amount,0)
					                	else 0 end
										,ISNULL(IRT.[Percentage],0) TaxPercentage
										,IT.Id
                ,0 DrAmount ,CrAmount=case when ITD.AType='Dr' then ISNULL( IRT.Amount,0) else 0 end
                ,TC.Code +' '+ 'RCM' TaxCode, TC.Sequence TCSequence,TC.TaxCategoryType,TC.UserName+'-'+TC.Code TaxCategory
				,IsRCM=CASE WHEN IsNULL(CP.TaxApplicable,'')='Mandatory' THEN 1 ELSE 0 END,NULL TaxCodeName
                ,IsNULL(IV.IsExcludingTax,0) IsExcludingTax,IsTaxApplicable=CASE WHEN IsNULL(CP.TaxApplicable,'')='Mandatory' THEN 1 ELSE 0 END,NULL [Type],0 ValueOfFixed
                ,IRT.[Percentage],IRT.HSNCodeId,SM.UserName Material,IRT.SalesMaterialId InventoryReceiveDetailId, IRT.SalesServiceId InventoryServiceId

                from TRN.InvoiceTax IT 
                left join TRN.InvoiceTaxDetail ITD  ON IT.Id=ITD.InvoiceTaxId AND ITD.AType='Dr'
                LEFT JOIN TRN.Voucher V ON V.Id=IT.VoucherId
                LEFT JOIN TRN.Invoice IV ON IV.Id=IT.InvoiceId
                LEFT JOIN HKP.Party P ON P.Id=IT.PartyId
                LEFT JOIN HKP.PartyPlant PP ON PP.Id=IT.PartyPlantId

				LEFT JOIN HKP.CompanyParty CP ON CP.PartyId=P.Id AND CP.PartyType='Customer' AND CP.PlantId = '" + plantId + @"'
                LEFT JOIN MST.TaxCategory TC ON TC.Id=IT.TaxCategoryId
                
                LEFT JOIN TRN.Sales IR ON IR.VoucherId = V.Id
                           LEFT JOIN TRN.SalesTax IRT ON IRT.SalesId = IR.Id AND IRT.TaxCategoryId = IT.TaxCategoryId
						--LEFT JOIN TRN.SalesMaterial SM ON SM.Id = IRT.SalesMaterialId
						LEFT JOIN TRN.SalesService SS ON SS.Id = IRT.SalesServiceId

						LEFT JOIN HKP.ServiceMaster  SM on SM.Id = SS.ServiceMasterId
                LEFT JOIN TRN.VoucherDetail VD ON VD.Id=IT.VoucherDetailId 
                LEFT JOIN HKP.Activity A ON A.Id=VD.ActivityId 
                where CP.TaxApplicable='Mandatory' AND V.PostingDate between '" + fromDate + "' AND '" + toDate + @"'
                and V.PlantId = '" + plantId + @"'  and V.IsPark=0 and  IRT.SalesServiceId<>''  and v.SourceType='SalesInvoice' AND ITD.AType='Dr'
UNION ALL
				select 'InventorySales' SourceType
                ,V.VoucherNo,Format(V.PostingDate,'dd-MMM-yyyy') PostingDate,V.DocRefNo,V.DocDate,P.UserName PartyName,PP.GSTIN
               ,LineItemType=case when v.SourceType='SalesInvoice' then 'InventorySales' 
				                   else '' end
			   ,Particular=case when v.SourceType='SalesInvoice' then MM.UserName 
				                   else '' end
								   ,PP.UserName as PartyPlant
               ,TaxableAmount=case when v.SourceType='SalesInvoice' then ISNULL(ISD.TotalSalesAmount,0)
					                	else 0 end
										,ISNULL(IST.[Percentage],0) TaxPercentage
										,IT.Id
                ,0 DrAmount ,CrAmount=case when ITD.AType='Dr' then ISNULL( IST.TaxAmount,0) else 0 end
                ,TC.Code +' '+ 'RCM' TaxCode, TC.Sequence TCSequence,TC.TaxCategoryType,TC.UserName+'-'+TC.Code TaxCategory
				,IsRCM=CASE WHEN IsNULL(CP.TaxApplicable,'')='Mandatory' THEN 1 ELSE 0 END,NULL TaxCodeName
                ,IsNULL(IV.IsExcludingTax,0) IsExcludingTax,IsTaxApplicable=CASE WHEN IsNULL(CP.TaxApplicable,'')='Mandatory' THEN 1 ELSE 0 END,NULL [Type],0 ValueOfFixed
                ,IST.[Percentage],IST.HSNCodeId,MM.UserName Material,IST.InventoryReceiveDetailId , IST.InventorySalesServiceId InventoryServiceId
				
                from TRN.InvoiceTax IT 
                left join TRN.InvoiceTaxDetail ITD  ON IT.Id=ITD.InvoiceTaxId AND ITD.AType='Dr'
                LEFT JOIN TRN.Voucher V ON V.Id=IT.VoucherId
                LEFT JOIN TRN.Invoice IV ON IV.Id=IT.InvoiceId
                LEFT JOIN HKP.Party P ON P.Id=IT.PartyId
                 LEFT JOIN HKP.PartyPlant PP ON PP.Id=IT.PartyPlantId
              
				LEFT JOIN HKP.CompanyParty CP ON CP.PartyId=P.Id AND CP.PartyType='Customer' AND
				CP.PlantId = '" + plantId + @"'
                LEFT JOIN MST.TaxCategory TC ON TC.Id=IT.TaxCategoryId
                LEFT JOIN TRN.InventorySales INS on INS.InventoryVoucherId=V.Id
			    LEFT JOIN TRN.InventorySalesTax  IST ON IST.InventorySalesId = INS.Id AND IST.TaxCategoryId = IT.TaxCategoryId
				LEFT JOIN TRN.InventorySalesDetail ISD ON ISD.InventorySalesId = INS.Id
				lEFT JOIN TRN.InventoryMaterial IM ON IM.Id=ISD.InventoryMaterialId
				LEFT JOIN MST.MaterialMaster MM ON MM.Id = IM.MaterialMasterId
                LEFT JOIN TRN.VoucherDetail VD ON VD.Id=IT.VoucherDetailId 
                LEFT JOIN HKP.Activity A ON A.Id=VD.ActivityId 
                
                where --CP.TaxApplicable='Mandatory' AND 
				V.PostingDate between '" + fromDate + "' AND '" + toDate + @"'

                and V.PlantId = '" + plantId + @"'  and V.IsPark=0 and IST.InventorySalesServiceId<>'' and v.SourceType='SalesInvoice' AND ITD.AType='Dr'
				
) DD
                ORDER BY TaxPercentage,LineItemType
";

            return _sqlRepository.GetDataTable(strSql);

        }
        #endregion

        #region GST For Month

        public IWorkbook GetGSTReceivableReport(string companyGroupId, string companyId, string plantId, string plantName, string fromDate, string toDate, string name)
        {
            clsReport objRpt = null;
            clsReport objRptSR = null;
            try
            {

                ExcelEngine excelEngine = null;
                IApplication application = null;
                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;
                excelEngine.Excel.DefaultVersion = ExcelVersion.Excel2013;
                var reportUtility = new ReportUtility();
                var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
                workbook.Version = ExcelVersion.Excel2013;
                var sheet1 = workbook.Worksheets[0];

                #region Logo
                string strPath = "";
                Image companyLogo = null;
                try
                {
                    DataTable dtCompanyImage = _sqlRepository.GetDataTable("SELECT * FROM ORG.COMPANY WHERE ID = '" + companyId + @"'");

                    strPath = Path.Combine(ResourcesPathReader.GetLogoOrImagePath(), dtCompanyImage.Rows[0]["Image"].ToString());  // IDCardEng.xlsx
                    companyLogo = Image.FromFile(strPath);
                }
                catch (Exception)
                {
                }
                #endregion
                objRpt = new clsReport();

                objRptSR = new clsReport(_sqlRepository);

                DataTable dtRCMPayable = null;
                string taxyearId = GetTaxYearId(fromDate, toDate, companyId);
                dtRCMPayable = GetGSTReceivableSQL(companyGroupId, companyId, plantId, plantName, fromDate, toDate, taxyearId);
                if (dtRCMPayable.Rows.Count == 0)
                {
                    throw new Exception("No Data Found....");
                }

                DataTable dtCmp = objRptSR.SelectedCompanyDT(plantId);

                DataTable dtFactory = objRptSR.SelectedPlantDT(plantId);

                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;

                int xlsRow = 1, xlsCol = 1;
                int endXlsCol = 1;
                string FactoryName = "";
                string CmpName = "";
                xlsRow = 6;
                sheet1.Range[xlsRow - 1, 1].Text = "Report Ref No:";
                sheet1.Range[xlsRow - 1, 1].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow - 1, 1].RowHeight = 20;
                sheet1.Range[xlsRow - 1, 1].CellStyle.Font.Bold = true;

                int iSourceType = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Category";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                xlsCol++;

                int iTaxPercentage = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Percentage";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                xlsCol++;

                int iPartyPlantName = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Party Plant";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 35;
                xlsCol++;

                int iParticulars = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Particulars";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 30;
                xlsCol++;

                int iGSTIN = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "GSTIN";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 25;

                //xlsCol++;
                //int iParticulars = xlsCol; // Party
                //sheet1.Range[xlsRow, xlsCol].Text = "Particulars";
                //sheet1.Range[xlsRow, xlsCol].ColumnWidth = 40;

                xlsCol++;
                int iVoucherNo = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Voucher No";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;

                xlsCol++;
                int iVoucherDate = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Entry Date";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;

                xlsCol++;
                int iPostingDate = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Posting Date";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                xlsCol++;
                int iDocRefNo = xlsCol; // Doc Ref
                sheet1.Range[xlsRow, xlsCol].Text = "DocRef No";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;

                xlsCol++;
                int iDocDate = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Doc Date";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;

                xlsCol++;
                int iGRNNo = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "GRN No";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;

                xlsCol++;
                int iTaxableAmount = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Taxable Amount";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignRight;
                DataTable dtTaxCode = null;
                dtRCMPayable.DefaultView.Sort = "TCSequence";
                dtTaxCode = dtRCMPayable.DefaultView.ToTable(true, "TaxCode");
                dtTaxCode.Columns.Add("ColumnNumber", typeof(String));
                dtTaxCode.Columns.Add("ColumnFormula", typeof(String));

                if (dtTaxCode.Rows.Count > 0)
                {
                    for (int i = 0; i < dtTaxCode.Rows.Count; i++)
                    {
                        xlsCol++;
                        sheet1.Range[xlsRow, xlsCol].Text = dtTaxCode.Rows[i]["TaxCode"].ToString();
                        sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                        dtTaxCode.Rows[i]["ColumnNumber"] = xlsCol.ToString();
                    }
                }
                xlsCol++;
                int iTotalTax = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Total Tax";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                endXlsCol = xlsCol;

                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].WrapText = true;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 23;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;

                string voucherNo = "";
                string Percentage = "";
                int startRow = 0;
                int perStartRow = 0;
                string formula = "";
                string formula2 = "";
                string totalFormula = "";

                string lineItemPercentageType = "";
                xlsRow++;
                startRow = xlsRow;
                perStartRow = xlsRow;
                bool isFirst = true;
                string totalTaxformula = "";
                string voucherNocomp = "";
                string taxFitler = "";
                for (int i = 0; i < dtRCMPayable.Rows.Count; i++)
                {


                    if (dtRCMPayable.Rows[i]["LineItemType"].ToString().ToUpper() == "GL")
                    {
                        voucherNocomp = dtRCMPayable.Rows[i]["VoucherNo"].ToString() + "-" + dtRCMPayable.Rows[i]["LineItemType"].ToString().ToUpper();
                        taxFitler = " and VoucherNo = '" + dtRCMPayable.Rows[i]["VoucherNo"].ToString() + "' and LineItemType = '" + dtRCMPayable.Rows[i]["LineItemType"].ToString() + "'";
                    }
                    if (dtRCMPayable.Rows[i]["LineItemType"].ToString().ToUpper() == "MATERIAL")
                    {
                        voucherNocomp = dtRCMPayable.Rows[i]["VoucherNo"].ToString() + "-" + dtRCMPayable.Rows[i]["LineItemType"].ToString().ToUpper() + "-" + dtRCMPayable.Rows[i]["InventoryReceiveDetailId"].ToString().ToUpper();

                        taxFitler = " and VoucherNo = '" + dtRCMPayable.Rows[i]["VoucherNo"].ToString() + "' and LineItemType = '" + dtRCMPayable.Rows[i]["LineItemType"].ToString() + "' and InventoryReceiveDetailId = '" + dtRCMPayable.Rows[i]["InventoryReceiveDetailId"].ToString().ToUpper() + "'";

                    }
                    if (dtRCMPayable.Rows[i]["LineItemType"].ToString().ToUpper() == "SERVICE")
                    {
                        voucherNocomp = dtRCMPayable.Rows[i]["VoucherNo"].ToString() + "-" + dtRCMPayable.Rows[i]["LineItemType"].ToString().ToUpper() + "-" + dtRCMPayable.Rows[i]["InventoryServiceId"].ToString().ToUpper();
                        taxFitler = " and VoucherNo = '" + dtRCMPayable.Rows[i]["VoucherNo"].ToString() + "' and LineItemType = '" + dtRCMPayable.Rows[i]["LineItemType"].ToString() + "' and InventoryServiceId = '" + dtRCMPayable.Rows[i]["InventoryServiceId"].ToString().ToUpper() + "'";
                    }



                    if (voucherNo != voucherNocomp)
                    {

                        if (dtRCMPayable.Rows[i]["LineItemType"].ToString().ToUpper() == "GL")
                        {
                            lineItemPercentageType = "ValueOfFixed";
                        }
                        if (dtRCMPayable.Rows[i]["LineItemType"].ToString().ToUpper() == "MATERIAL")
                        {
                            lineItemPercentageType = "Percentage";
                        }
                        if (dtRCMPayable.Rows[i]["LineItemType"].ToString().ToUpper() == "SERVICE")
                        {
                            lineItemPercentageType = "Percentage";
                        }
                        if (Percentage != dtRCMPayable.Rows[i]["TaxPercentage"].ToString())
                        {
                            if (isFirst == false)
                            {

                                sheet1[perStartRow, iPostingDate, xlsRow - 1, iPostingDate].BorderAround(ExcelLineStyle.Hair);
                                sheet1[perStartRow, iParticulars, xlsRow - 1, iParticulars].BorderAround(ExcelLineStyle.Hair);
                                sheet1[perStartRow, iSourceType, xlsRow - 1, iSourceType].BorderAround(ExcelLineStyle.Hair);
                                sheet1[perStartRow, iTaxPercentage, xlsRow - 1, iTaxPercentage].BorderAround(ExcelLineStyle.Hair);
                                sheet1[perStartRow, iVoucherNo, xlsRow - 1, iVoucherNo].BorderAround(ExcelLineStyle.Hair);
                                sheet1[perStartRow, iVoucherDate, xlsRow - 1, iVoucherDate].BorderAround(ExcelLineStyle.Hair);
                                sheet1[perStartRow, iDocDate, xlsRow - 1, iDocDate].BorderAround(ExcelLineStyle.Hair);
                                sheet1[perStartRow, iDocRefNo, xlsRow - 1, iDocRefNo].BorderAround(ExcelLineStyle.Hair);
                                sheet1[perStartRow, iGRNNo, xlsRow - 1, iGRNNo].BorderAround(ExcelLineStyle.Hair);
                                sheet1[perStartRow, iTaxableAmount, xlsRow - 1, iTaxableAmount].BorderAround(ExcelLineStyle.Hair);
                                sheet1[perStartRow, iTotalTax, xlsRow - 1, iTotalTax].BorderAround(ExcelLineStyle.Hair);

                                formula = "SUM(" + clsStaticInfo.GetxlsCol(iTaxableAmount) + perStartRow + ":" + clsStaticInfo.GetxlsCol(iTaxableAmount) + (xlsRow - 1) + ")";
                                formula2 = "";

                                if (dtTaxCode.Rows.Count > 0)
                                {
                                    totalTaxformula = "SUM(";
                                    for (int j = 0; j < dtTaxCode.Rows.Count; j++)
                                    {
                                        sheet1[perStartRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"]), xlsRow - 1, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])].BorderAround(ExcelLineStyle.Hair);
                                        formula2 = "SUM(" + clsStaticInfo.GetxlsCol(Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])) + perStartRow + ":" + clsStaticInfo.GetxlsCol(Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])) + (xlsRow - 1) + ")";
                                        sheet1[xlsRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"]), xlsRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])].Formula = formula2;

                                        dtTaxCode.Rows[j]["ColumnFormula"] += (clsStaticInfo.GetxlsCol(Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])) + xlsRow).ToString() + " + ";
                                        totalTaxformula += clsStaticInfo.GetxlsCol(Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])) + xlsRow + ":";
                                    }
                                }
                                sheet1.Range[xlsRow, 1, xlsRow, 1].Text = "Total";

                                sheet1[xlsRow, iTaxableAmount, xlsRow, iTaxableAmount].Formula = formula;
                                sheet1[xlsRow, iTotalTax, xlsRow, iTotalTax].Formula = totalTaxformula.Remove(totalTaxformula.Length - 1) + ")";

                                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Font.Bold = true;
                                totalFormula += (clsStaticInfo.GetxlsCol(iTaxableAmount) + xlsRow).ToString() + "+";

                                xlsRow++;


                            }
                            xlsRow++;
                            sheet1.Range[xlsRow - 1, 1].Number = clsStaticInfo.dbl(dtRCMPayable.Rows[i]["TaxPercentage"].ToString());
                            sheet1.Range[xlsRow - 1, 1].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                            perStartRow = xlsRow;
                            isFirst = false;

                        }

                        sheet1.Range[xlsRow, iPartyPlantName].Text = dtRCMPayable.Rows[i]["PartyPlantName"].ToString();
                        sheet1.Range[xlsRow, iParticulars].Text = dtRCMPayable.Rows[i]["Particular"].ToString();
                        sheet1.Range[xlsRow, iGSTIN].Text = dtRCMPayable.Rows[i]["GSTIN"].ToString();

                        sheet1.Range[xlsRow, iDocRefNo].Text = dtRCMPayable.Rows[i]["DocRefNo"].ToString();
                        sheet1.Range[xlsRow, iSourceType].Text = dtRCMPayable.Rows[i]["SourceType"].ToString();
                        //sheet1.Range[xlsRow, iTaxPercentage].Text = dtRCMPayable.Rows[i]["TaxPercentage"].ToString();
                        sheet1.Range[xlsRow, iTaxPercentage].Number = clsStaticInfo.dbl(dtRCMPayable.Rows[i]["TaxPercentage"].ToString());
                        sheet1.Range[xlsRow, iTaxPercentage].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                        sheet1.Range[xlsRow, iVoucherNo].Text = dtRCMPayable.Rows[i]["VoucherNo"].ToString();
                        sheet1.Range[xlsRow, iVoucherDate].Text = dtRCMPayable.Rows[i]["VoucherDate"].ToString();
                        sheet1.Range[xlsRow, iPostingDate].Text = clsStaticInfo.GetDateTaxFormate(dtRCMPayable.Rows[i]["PostingDate"].ToString());
                        sheet1.Range[xlsRow, iDocDate].Text = dtRCMPayable.Rows[i]["DocDate"].ToString();
                        sheet1.Range[xlsRow, iGRNNo].Text = dtRCMPayable.Rows[i]["GRNNo"].ToString();


                        sheet1.Range[xlsRow, iTaxableAmount].Number = clsStaticInfo.dbl(dtRCMPayable.Rows[i]["TaxableAmount"].ToString());//TaxableAmount
                        sheet1.Range[xlsRow, iTaxableAmount].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                        //dtRCMPayable.DefaultView.RowFilter = "VoucherNo = '" + dtRCMPayable.Rows[i]["VoucherNo"].ToString() + "'";

                        if (dtTaxCode.Rows.Count > 0)
                        {
                            totalTaxformula = "=SUM(";
                            for (int j = 0; j < dtTaxCode.Rows.Count; j++)
                            {
                                dtRCMPayable.DefaultView.RowFilter = "TaxCode = '" + dtTaxCode.Rows[j]["TaxCode"].ToString() + "'" + taxFitler;
                                if (dtRCMPayable.DefaultView.Count > 0)
                                {

                                    sheet1.Range[xlsRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])].Number = clsStaticInfo.dbl(dtRCMPayable.DefaultView[0]["DrAmount"].ToString());
                                    sheet1.Range[xlsRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                                }
                                else
                                {
                                    sheet1.Range[xlsRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])].Text = "-";
                                    sheet1.Range[xlsRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])].HorizontalAlignment = ExcelHAlign.HAlignRight;


                                }
                                totalTaxformula += clsStaticInfo.GetxlsCol(Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])) + xlsRow + ":";
                            }
                            sheet1.Range[xlsRow, iTotalTax].Formula = totalTaxformula.Remove(totalTaxformula.Length - 1) + ")";

                        }


                        Percentage = dtRCMPayable.Rows[i]["TaxPercentage"].ToString();



                        xlsRow++;
                    }


                    if (dtRCMPayable.Rows[i]["LineItemType"].ToString().ToUpper() == "GL")
                    {
                        voucherNo = dtRCMPayable.Rows[i]["VoucherNo"].ToString() + "-" + dtRCMPayable.Rows[i]["LineItemType"].ToString().ToUpper();

                    }
                    if (dtRCMPayable.Rows[i]["LineItemType"].ToString().ToUpper() == "MATERIAL")
                    {
                        voucherNo = dtRCMPayable.Rows[i]["VoucherNo"].ToString() + "-" + dtRCMPayable.Rows[i]["LineItemType"].ToString().ToUpper() + "-" + dtRCMPayable.Rows[i]["InventoryReceiveDetailId"].ToString().ToUpper();


                    }
                    if (dtRCMPayable.Rows[i]["LineItemType"].ToString().ToUpper() == "SERVICE")
                    {
                        voucherNo = dtRCMPayable.Rows[i]["VoucherNo"].ToString() + "-" + dtRCMPayable.Rows[i]["LineItemType"].ToString().ToUpper() + "-" + dtRCMPayable.Rows[i]["InventoryServiceId"].ToString().ToUpper();


                    }


                }
                sheet1[perStartRow, iPartyPlantName, xlsRow - 1, iPartyPlantName].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iPostingDate, xlsRow - 1, iPostingDate].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iParticulars, xlsRow - 1, iParticulars].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iTaxPercentage, xlsRow - 1, iTaxPercentage].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iVoucherNo, xlsRow - 1, iVoucherNo].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iVoucherDate, xlsRow - 1, iVoucherDate].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iDocDate, xlsRow - 1, iDocDate].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iGRNNo, xlsRow - 1, iGRNNo].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iDocRefNo, xlsRow - 1, iDocRefNo].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iSourceType, xlsRow - 1, iSourceType].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iTaxableAmount, xlsRow - 1, iTaxableAmount].BorderAround(ExcelLineStyle.Hair);
                //sheet1[perStartRow, iTaxableAmount, xlsRow - 1, iTaxableAmount].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iTotalTax, xlsRow - 1, iTotalTax].BorderAround(ExcelLineStyle.Hair);


                if (dtTaxCode.Rows.Count > 0)
                {
                    for (int j = 0; j < dtTaxCode.Rows.Count; j++)
                    {
                        sheet1[perStartRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"]), xlsRow - 1, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])].BorderAround(ExcelLineStyle.Hair);

                    }
                }



                if (dtTaxCode.Rows.Count > 0)
                {
                    totalTaxformula = "=SUM(";
                    for (int j = 0; j < dtTaxCode.Rows.Count; j++)
                    {
                        sheet1[perStartRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"]), xlsRow - 1, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])].BorderAround(ExcelLineStyle.Hair);
                        formula2 = "SUM(" + clsStaticInfo.GetxlsCol(Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])) + perStartRow + ":" + clsStaticInfo.GetxlsCol(Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])) + (xlsRow - 1) + ")";
                        sheet1[xlsRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"]), xlsRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])].Formula = formula2;

                        dtTaxCode.Rows[j]["ColumnFormula"] += (clsStaticInfo.GetxlsCol(Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])) + xlsRow).ToString() + " + ";
                        totalTaxformula += clsStaticInfo.GetxlsCol(Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])) + xlsRow + ":";

                    }
                }
                sheet1.Range[xlsRow, 1, xlsRow, 1].Text = "Total";
                formula = "SUM(" + clsStaticInfo.GetxlsCol(iTaxableAmount) + perStartRow + ":" + clsStaticInfo.GetxlsCol(iTaxableAmount) + (xlsRow - 1) + ")";

                sheet1[xlsRow, iTaxableAmount, xlsRow, iTaxableAmount].Formula = formula;
                sheet1[xlsRow, iTotalTax, xlsRow, iTotalTax].Formula = totalTaxformula.Remove(totalTaxformula.Length - 1) + ")";

                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Font.Bold = true;
                totalFormula += (clsStaticInfo.GetxlsCol(iTaxableAmount) + xlsRow).ToString() + "+";



                xlsRow++;
                xlsRow++;


                if (dtTaxCode.Rows.Count > 0)
                {
                    totalTaxformula = "=SUM(";
                    for (int j = 0; j < dtTaxCode.Rows.Count; j++)
                    {
                        string fm = dtTaxCode.Rows[j]["ColumnFormula"].ToString().Trim();
                        sheet1[xlsRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"]), xlsRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])].Formula = fm.Remove(fm.Length - 1); //dtTaxCode.Rows[j]["ColumnFormula"].ToString().Remove(dtTaxCode.Rows[j]["ColumnFormula"].ToString().Length - 1);
                        totalTaxformula += clsStaticInfo.GetxlsCol(Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])) + xlsRow + ":";

                    }
                }
                sheet1.Range[xlsRow, 1, xlsRow, 1].Text = "Grand Total";

                sheet1[xlsRow, iTaxableAmount, xlsRow, iTaxableAmount].Formula = totalFormula.Remove(totalFormula.Length - 1);
                sheet1[xlsRow, iTotalTax, xlsRow, iTotalTax].Formula = totalTaxformula.Remove(totalTaxformula.Length - 1) + ")";

                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Font.Bold = true;




                #region ******************Report Header******************



                xlsRow = 1;
                xlsCol = 3;
                try
                {
                    if (companyLogo != null)
                    {

                        double totalWidth = sheet1.GetColumnWidth(1) + sheet1.GetColumnWidth(3);
                        int totalWidthPixel = (int)(totalWidth * 7.5);
                        int totalheight = (int)((sheet1.GetRowHeight(1) + sheet1.GetRowHeight(2) + sheet1.GetRowHeight(3) + sheet1.GetRowHeight(3)) * 1.50);

                        companyLogo = ReportUtility.FixedSize(companyLogo, totalWidthPixel, totalheight);
                        IPictureShape pic = null;

                        pic = sheet1.Pictures.AddPicture(1, 1, companyLogo);
                        //pic.Height = 80;
                        //pic.Width = 220;
                    }
                }
                catch (Exception ex)
                {
                }

                FactoryName = string.Empty;

                string FactoryAddress = string.Empty;

                if (dtCmp.Rows.Count > 0)
                {
                    CmpName = dtCmp.Rows[0]["CompanyName"].ToString();
                }
                else
                {
                    CmpName = "";
                }
                sheet1.Range[xlsRow, 3].Text = CmpName;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 12;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 17;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                if (dtFactory.Rows.Count > 0)
                {
                    //FactoryName = dsFactory.Tables[0].Rows[0]["PlantName"].ToString();
                    FactoryName = dtFactory.Rows[0]["UserName"].ToString();
                }
                else
                {
                    FactoryName = "";
                }
                sheet1.Range[xlsRow, 3].Text = FactoryName;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 14;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 18;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                if (dtFactory.Rows.Count > 0)
                {
                    FactoryAddress = dtFactory.Rows[0]["Address1"].ToString();
                }
                else
                {
                    FactoryAddress = "";
                }
                sheet1.Range[xlsRow, 3].Text = FactoryAddress;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                //sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 22;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                sheet1.Range[xlsRow, 3].Text = "GST Recievable Report (Format 1) From " + fromDate + " To " + toDate;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;


                #endregion ******************Report Header******************

                #region Freeze Panes

                sheet1.IsDisplayZeros = false;
                sheet1.UsedRange["A7"].FreezePanes();
                sheet1.FirstVisibleColumn = 1;
                sheet1.FirstVisibleRow = 6;

                #endregion Freeze Panes

                #region UsedRange Alignment

                sheet1.UsedRange.WrapText = false;
                sheet1.UsedRange.CellStyle.Font.Size = 10;
                sheet1.Range["A1"].CellStyle.Font.Size = 14;
                sheet1.Range["A2"].CellStyle.Font.Size = 10;
                sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;
                sheet1.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                #endregion UsedRange Alignment

                #region Page Setup
                sheet1.PageSetup.TopMargin = 0.5;
                sheet1.PageSetup.BottomMargin = 0.7;
                sheet1.PageSetup.PrintTitleRows = "$1:$5";
                sheet1.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                sheet1.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + name + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:MM tt").ToString();
                sheet1.PageSetup.LeftMargin = 0.5;
                sheet1.PageSetup.RightMargin = 0.2;
                sheet1.PageSetup.Orientation = ExcelPageOrientation.Portrait;
                sheet1.PageSetup.FitToPagesTall = 0;
                sheet1.PageSetup.FitToPagesWide = 1;
                sheet1.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                sheet1.IsDisplayZeros = false;
                #endregion Page Setup


                sheet1.Name = "GST Receivable";
                return workbook;
            }
            catch (System.Exception ex)
            {

                throw ex;
            }
        }
        public IWorkbook GetGSTReceivableReport2(string companyGroupId, string companyId, string plantId, string plantName, string fromDate, string toDate, string name)
        {
            clsReport objRpt = null;
            clsReport objRptSR = null;
            try
            {

                ExcelEngine excelEngine = null;
                IApplication application = null;
                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;
                excelEngine.Excel.DefaultVersion = ExcelVersion.Excel2013;
                var reportUtility = new ReportUtility();
                var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
                workbook.Version = ExcelVersion.Excel2013;
                var sheet1 = workbook.Worksheets[0];

                #region Logo
                string strPath = "";
                Image companyLogo = null;
                try
                {
                    DataTable dtCompanyImage = _sqlRepository.GetDataTable("SELECT * FROM ORG.COMPANY WHERE ID = '" + companyId + @"'");

                    strPath = Path.Combine(ResourcesPathReader.GetLogoOrImagePath(), dtCompanyImage.Rows[0]["Image"].ToString());  // IDCardEng.xlsx
                    companyLogo = Image.FromFile(strPath);
                }
                catch (Exception)
                {
                }
                #endregion
                objRpt = new clsReport();

                objRptSR = new clsReport(_sqlRepository);

                DataTable dtRCMPayable = null;
                string taxyearId = GetTaxYearId(fromDate, toDate, companyId);
                dtRCMPayable = GetGSTReceivableSQL(companyGroupId, companyId, plantId, plantName, fromDate, toDate, taxyearId);
                if (dtRCMPayable.Rows.Count == 0)
                {
                    throw new Exception("No Data Found....");
                }

                DataTable dtCmp = objRptSR.SelectedCompanyDT(plantId);

                DataTable dtFactory = objRptSR.SelectedPlantDT(plantId);

                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;

                int xlsRow = 1, xlsCol = 1;
                int endXlsCol = 1;
                string FactoryName = "";
                string CmpName = "";
                xlsRow = 6;
                sheet1.Range[xlsRow - 1, 1].Text = "Report Ref No:";
                sheet1.Range[xlsRow - 1, 1].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow - 1, 1].RowHeight = 20;
                sheet1.Range[xlsRow - 1, 1].CellStyle.Font.Bold = true;

                int iPartyPlantName = xlsCol; // Party
                sheet1.Range[xlsRow, xlsCol].Text = "Party Plant";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 35;
                xlsCol++;

                int iParticulars = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Particulars";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 30;
                xlsCol++;

                int iGSTIN = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "GSTIN";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 25;

                xlsCol++;
                int iVoucherNo = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Voucher No";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;

                xlsCol++;
                int iTaxPercentage = xlsCol; // Party
                sheet1.Range[xlsRow, xlsCol].Text = "Percentage";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;


                xlsCol++;
                int iTaxableAmount = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Taxable Amount";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignRight;
                DataTable dtTaxCode = null;
                dtRCMPayable.DefaultView.Sort = "TCSequence";
                dtTaxCode = dtRCMPayable.DefaultView.ToTable(true, "TaxCode");
                dtTaxCode.Columns.Add("ColumnNumber", typeof(String));
                dtTaxCode.Columns.Add("ColumnFormula", typeof(String));

                if (dtTaxCode.Rows.Count > 0)
                {
                    for (int i = 0; i < dtTaxCode.Rows.Count; i++)
                    {
                        xlsCol++;
                        sheet1.Range[xlsRow, xlsCol].Text = dtTaxCode.Rows[i]["TaxCode"].ToString();
                        sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                        dtTaxCode.Rows[i]["ColumnNumber"] = xlsCol.ToString();
                    }
                }
                xlsCol++;
                int iTotalTax = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Total Tax";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                endXlsCol = xlsCol;

                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].WrapText = true;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 23;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;

                string voucherNo = "";
                string Percentage = "";
                int startRow = 0;
                int perStartRow = 0;
                string formula = "";
                string formula2 = "";
                string totalFormula = "";

                string lineItemPercentageType = "";
                xlsRow++;
                startRow = xlsRow;
                perStartRow = xlsRow;
                bool isFirst = true;
                string totalTaxformula = "";
                string taxFitler = "";
                string voucherNocomp = "";

                for (int i = 0; i < dtRCMPayable.Rows.Count; i++)
                {

                    if (dtRCMPayable.Rows[i]["LineItemType"].ToString().ToUpper() == "GL")
                    {
                        voucherNocomp = dtRCMPayable.Rows[i]["VoucherNo"].ToString() + "-" + dtRCMPayable.Rows[i]["LineItemType"].ToString().ToUpper();
                        taxFitler = " and VoucherNo = '" + dtRCMPayable.Rows[i]["VoucherNo"].ToString() + "' and LineItemType = '" + dtRCMPayable.Rows[i]["LineItemType"].ToString() + "'";
                    }
                    if (dtRCMPayable.Rows[i]["LineItemType"].ToString().ToUpper() == "MATERIAL")
                    {
                        voucherNocomp = dtRCMPayable.Rows[i]["VoucherNo"].ToString() + "-" + dtRCMPayable.Rows[i]["LineItemType"].ToString().ToUpper() + "-" + dtRCMPayable.Rows[i]["InventoryReceiveDetailId"].ToString().ToUpper();

                        taxFitler = " and VoucherNo = '" + dtRCMPayable.Rows[i]["VoucherNo"].ToString() + "' and LineItemType = '" + dtRCMPayable.Rows[i]["LineItemType"].ToString() + "' and InventoryReceiveDetailId = '" + dtRCMPayable.Rows[i]["InventoryReceiveDetailId"].ToString().ToUpper() + "'";

                    }
                    if (dtRCMPayable.Rows[i]["LineItemType"].ToString().ToUpper() == "SERVICE")
                    {
                        voucherNocomp = dtRCMPayable.Rows[i]["VoucherNo"].ToString() + "-" + dtRCMPayable.Rows[i]["LineItemType"].ToString().ToUpper() + "-" + dtRCMPayable.Rows[i]["InventoryServiceId"].ToString().ToUpper();
                        taxFitler = " and VoucherNo = '" + dtRCMPayable.Rows[i]["VoucherNo"].ToString() + "' and LineItemType = '" + dtRCMPayable.Rows[i]["LineItemType"].ToString() + "' and InventoryServiceId = '" + dtRCMPayable.Rows[i]["InventoryServiceId"].ToString().ToUpper() + "'";



                    }
                    if (voucherNo != voucherNocomp)
                    {

                        if (dtRCMPayable.Rows[i]["LineItemType"].ToString().ToUpper() == "GL")
                        {
                            lineItemPercentageType = "ValueOfFixed";
                        }
                        if (dtRCMPayable.Rows[i]["LineItemType"].ToString().ToUpper() == "MATERIAL")
                        {
                            lineItemPercentageType = "Percentage";
                        }
                        if (Percentage != dtRCMPayable.Rows[i]["TaxPercentage"].ToString())
                        {
                            if (isFirst == false)
                            {

                                sheet1[perStartRow, iParticulars, xlsRow - 1, iParticulars].BorderAround(ExcelLineStyle.Hair);
                                sheet1[perStartRow, iTaxPercentage, xlsRow - 1, iTaxPercentage].BorderAround(ExcelLineStyle.Hair);
                                sheet1[perStartRow, iTaxableAmount, xlsRow - 1, iTaxableAmount].BorderAround(ExcelLineStyle.Hair);
                                sheet1[perStartRow, iTotalTax, xlsRow - 1, iTotalTax].BorderAround(ExcelLineStyle.Hair);

                                formula = "SUM(" + clsStaticInfo.GetxlsCol(iTaxableAmount) + perStartRow + ":" + clsStaticInfo.GetxlsCol(iTaxableAmount) + (xlsRow - 1) + ")";
                                formula2 = "";

                                if (dtTaxCode.Rows.Count > 0)
                                {
                                    totalTaxformula = "SUM(";
                                    for (int j = 0; j < dtTaxCode.Rows.Count; j++)
                                    {
                                        sheet1[perStartRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"]), xlsRow - 1, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])].BorderAround(ExcelLineStyle.Hair);
                                        formula2 = "SUM(" + clsStaticInfo.GetxlsCol(Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])) + perStartRow + ":" + clsStaticInfo.GetxlsCol(Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])) + (xlsRow - 1) + ")";
                                        sheet1[xlsRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"]), xlsRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])].Formula = formula2;

                                        dtTaxCode.Rows[j]["ColumnFormula"] += (clsStaticInfo.GetxlsCol(Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])) + xlsRow).ToString() + " + ";
                                        totalTaxformula += clsStaticInfo.GetxlsCol(Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])) + xlsRow + ":";

                                    }
                                }
                                sheet1.Range[xlsRow, 1, xlsRow, 1].Text = "Total";

                                sheet1[xlsRow, iTaxableAmount, xlsRow, iTaxableAmount].Formula = formula;
                                sheet1[xlsRow, iTotalTax, xlsRow, iTotalTax].Formula = totalTaxformula.Remove(totalTaxformula.Length - 1) + ")";

                                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Font.Bold = true;
                                totalFormula += (clsStaticInfo.GetxlsCol(iTaxableAmount) + xlsRow).ToString() + "+";

                                xlsRow++;


                            }
                            xlsRow++;
                            sheet1.Range[xlsRow - 1, 1].Number = clsStaticInfo.dbl(dtRCMPayable.Rows[i]["TaxPercentage"].ToString());
                            sheet1.Range[xlsRow - 1, 1].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                            perStartRow = xlsRow;
                            isFirst = false;

                        }


                        sheet1.Range[xlsRow, iPartyPlantName].Text = dtRCMPayable.Rows[i]["PartyPlantName"].ToString();
                        sheet1.Range[xlsRow, iParticulars].Text = dtRCMPayable.Rows[i]["Particular"].ToString();
                        sheet1.Range[xlsRow, iGSTIN].Text = dtRCMPayable.Rows[i]["GSTIN"].ToString();
                        sheet1.Range[xlsRow, iVoucherNo].Text = dtRCMPayable.Rows[i]["VoucherNo"].ToString();

                        sheet1.Range[xlsRow, iTaxPercentage].Number = clsStaticInfo.dbl(dtRCMPayable.Rows[i]["TaxPercentage"].ToString());
                        sheet1.Range[xlsRow, iTaxPercentage].NumberFormat = reportUtility.NumberFormatDecimalTwo();


                        sheet1.Range[xlsRow, iTaxableAmount].Number = clsStaticInfo.dbl(dtRCMPayable.Rows[i]["TaxableAmount"].ToString());//TaxableAmount
                        sheet1.Range[xlsRow, iTaxableAmount].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                        dtRCMPayable.DefaultView.RowFilter = "VoucherNo = '" + dtRCMPayable.Rows[i]["VoucherNo"].ToString() + "'";

                        if (dtTaxCode.Rows.Count > 0)
                        {
                            totalTaxformula = "=SUM(";
                            for (int j = 0; j < dtTaxCode.Rows.Count; j++)
                            {
                                dtRCMPayable.DefaultView.RowFilter = "TaxCode = '" + dtTaxCode.Rows[j]["TaxCode"].ToString() + "'" + taxFitler;
                                //dtRCMPayable.DefaultView.RowFilter = "TaxCode = '" + dtTaxCode.Rows[j]["TaxCode"].ToString() + "' and VoucherNo = '" + dtRCMPayable.Rows[i]["VoucherNo"].ToString() + "' and LineItemType = '" + dtRCMPayable.Rows[i]["LineItemType"].ToString() + "'";
                                if (dtRCMPayable.DefaultView.Count > 0)
                                {

                                    sheet1.Range[xlsRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])].Number = clsStaticInfo.dbl(dtRCMPayable.DefaultView[0]["DrAmount"].ToString());
                                    sheet1.Range[xlsRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                                }
                                else
                                {
                                    sheet1.Range[xlsRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])].Text = "-";
                                    sheet1.Range[xlsRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])].HorizontalAlignment = ExcelHAlign.HAlignRight;


                                }
                                totalTaxformula += clsStaticInfo.GetxlsCol(Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])) + xlsRow + ":";
                            }
                            sheet1.Range[xlsRow, iTotalTax].Formula = totalTaxformula.Remove(totalTaxformula.Length - 1) + ")";

                        }

                        Percentage = dtRCMPayable.Rows[i]["TaxPercentage"].ToString();



                        xlsRow++;
                    }

                    if (dtRCMPayable.Rows[i]["LineItemType"].ToString().ToUpper() == "GL")
                    {
                        voucherNo = dtRCMPayable.Rows[i]["VoucherNo"].ToString() + "-" + dtRCMPayable.Rows[i]["LineItemType"].ToString().ToUpper();

                    }
                    if (dtRCMPayable.Rows[i]["LineItemType"].ToString().ToUpper() == "MATERIAL")
                    {
                        voucherNo = dtRCMPayable.Rows[i]["VoucherNo"].ToString() + "-" + dtRCMPayable.Rows[i]["LineItemType"].ToString().ToUpper() + "-" + dtRCMPayable.Rows[i]["InventoryReceiveDetailId"].ToString().ToUpper();


                    }
                    if (dtRCMPayable.Rows[i]["LineItemType"].ToString().ToUpper() == "SERVICE")
                    {
                        voucherNo = dtRCMPayable.Rows[i]["VoucherNo"].ToString() + "-" + dtRCMPayable.Rows[i]["LineItemType"].ToString().ToUpper() + "-" + dtRCMPayable.Rows[i]["InventoryServiceId"].ToString().ToUpper();


                    }

                }
                sheet1[perStartRow, iPartyPlantName, xlsRow - 1, iParticulars].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iParticulars, xlsRow - 1, iParticulars].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iTaxPercentage, xlsRow - 1, iTaxPercentage].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iTaxableAmount, xlsRow - 1, iTaxableAmount].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iVoucherNo, xlsRow - 1, iVoucherNo].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iTotalTax, xlsRow - 1, iTotalTax].BorderAround(ExcelLineStyle.Hair);


                if (dtTaxCode.Rows.Count > 0)
                {
                    for (int j = 0; j < dtTaxCode.Rows.Count; j++)
                    {
                        sheet1[perStartRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"]), xlsRow - 1, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])].BorderAround(ExcelLineStyle.Hair);

                    }
                }



                if (dtTaxCode.Rows.Count > 0)
                {
                    totalTaxformula = "=SUM(";
                    for (int j = 0; j < dtTaxCode.Rows.Count; j++)
                    {
                        sheet1[perStartRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"]), xlsRow - 1, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])].BorderAround(ExcelLineStyle.Hair);
                        formula2 = "SUM(" + clsStaticInfo.GetxlsCol(Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])) + perStartRow + ":" + clsStaticInfo.GetxlsCol(Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])) + (xlsRow - 1) + ")";
                        sheet1[xlsRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"]), xlsRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])].Formula = formula2;

                        dtTaxCode.Rows[j]["ColumnFormula"] += (clsStaticInfo.GetxlsCol(Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])) + xlsRow).ToString() + " + ";
                        totalTaxformula += clsStaticInfo.GetxlsCol(Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])) + xlsRow + ":";

                    }
                }
                sheet1.Range[xlsRow, 1, xlsRow, 1].Text = "Total";
                formula = "SUM(" + clsStaticInfo.GetxlsCol(iTaxableAmount) + perStartRow + ":" + clsStaticInfo.GetxlsCol(iTaxableAmount) + (xlsRow - 1) + ")";

                sheet1[xlsRow, iTaxableAmount, xlsRow, iTaxableAmount].Formula = formula;
                sheet1[xlsRow, iTotalTax, xlsRow, iTotalTax].Formula = totalTaxformula.Remove(totalTaxformula.Length - 1) + ")";

                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Font.Bold = true;
                totalFormula += (clsStaticInfo.GetxlsCol(iTaxableAmount) + xlsRow).ToString() + "+";



                xlsRow++;
                xlsRow++;


                if (dtTaxCode.Rows.Count > 0)
                {
                    totalTaxformula = "=SUM(";
                    for (int j = 0; j < dtTaxCode.Rows.Count; j++)
                    {
                        string fm = dtTaxCode.Rows[j]["ColumnFormula"].ToString().Trim();
                        sheet1[xlsRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"]), xlsRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])].Formula = fm.Remove(fm.Length - 1); //dtTaxCode.Rows[j]["ColumnFormula"].ToString().Remove(dtTaxCode.Rows[j]["ColumnFormula"].ToString().Length - 1);
                        totalTaxformula += clsStaticInfo.GetxlsCol(Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])) + xlsRow + ":";

                    }
                }
                sheet1.Range[xlsRow, 1, xlsRow, 1].Text = "Grand Total";

                sheet1[xlsRow, iTaxableAmount, xlsRow, iTaxableAmount].Formula = totalFormula.Remove(totalFormula.Length - 1);
                sheet1[xlsRow, iTotalTax, xlsRow, iTotalTax].Formula = totalTaxformula.Remove(totalTaxformula.Length - 1) + ")";

                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Font.Bold = true;




                #region ******************Report Header******************



                xlsRow = 1;
                xlsCol = 3;
                try
                {
                    if (companyLogo != null)
                    {

                        double totalWidth = sheet1.GetColumnWidth(1) + sheet1.GetColumnWidth(3);
                        int totalWidthPixel = (int)(totalWidth * 7.5);
                        int totalheight = (int)((sheet1.GetRowHeight(1) + sheet1.GetRowHeight(2) + sheet1.GetRowHeight(3) + sheet1.GetRowHeight(3)) * 1.50);

                        companyLogo = ReportUtility.FixedSize(companyLogo, totalWidthPixel, totalheight);
                        IPictureShape pic = null;

                        pic = sheet1.Pictures.AddPicture(1, 1, companyLogo);
                        //pic.Height = 80;
                        //pic.Width = 220;
                    }
                }
                catch (Exception ex)
                {
                }

                FactoryName = string.Empty;

                string FactoryAddress = string.Empty;

                if (dtCmp.Rows.Count > 0)
                {
                    CmpName = dtCmp.Rows[0]["CompanyName"].ToString();
                }
                else
                {
                    CmpName = "";
                }
                sheet1.Range[xlsRow, 3].Text = CmpName;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 12;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 17;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                if (dtFactory.Rows.Count > 0)
                {
                    //FactoryName = dsFactory.Tables[0].Rows[0]["PlantName"].ToString();
                    FactoryName = dtFactory.Rows[0]["UserName"].ToString();
                }
                else
                {
                    FactoryName = "";
                }
                sheet1.Range[xlsRow, 3].Text = FactoryName;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 14;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 18;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                if (dtFactory.Rows.Count > 0)
                {
                    FactoryAddress = dtFactory.Rows[0]["Address1"].ToString();
                }
                else
                {
                    FactoryAddress = "";
                }
                sheet1.Range[xlsRow, 3].Text = FactoryAddress;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                //sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 22;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                sheet1.Range[xlsRow, 3].Text = "GST Recievable Report (Format 2) From " + fromDate + " To " + toDate;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;


                #endregion ******************Report Header******************

                #region Freeze Panes

                sheet1.IsDisplayZeros = false;
                sheet1.UsedRange["A7"].FreezePanes();
                sheet1.FirstVisibleColumn = 1;
                sheet1.FirstVisibleRow = 6;

                #endregion Freeze Panes

                #region UsedRange Alignment

                sheet1.UsedRange.WrapText = false;
                sheet1.UsedRange.CellStyle.Font.Size = 10;
                sheet1.Range["A1"].CellStyle.Font.Size = 14;
                sheet1.Range["A2"].CellStyle.Font.Size = 10;
                sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;
                sheet1.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                #endregion UsedRange Alignment

                #region Page Setup
                sheet1.PageSetup.TopMargin = 0.5;
                sheet1.PageSetup.BottomMargin = 0.7;
                sheet1.PageSetup.PrintTitleRows = "$1:$5";
                sheet1.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                sheet1.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + name + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:MM tt").ToString();
                sheet1.PageSetup.LeftMargin = 0.5;
                sheet1.PageSetup.RightMargin = 0.2;
                sheet1.PageSetup.Orientation = ExcelPageOrientation.Portrait;
                sheet1.PageSetup.FitToPagesTall = 0;
                sheet1.PageSetup.FitToPagesWide = 1;
                sheet1.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                sheet1.IsDisplayZeros = false;
                #endregion Page Setup


                sheet1.Name = "GST Receivable";
                return workbook;
            }
            catch (System.Exception ex)
            {

                throw ex;
            }
        }
        public IWorkbook GetGSTReceivableReport3(string companyGroupId, string companyId, string plantId, string plantName, string fromDate, string toDate, string name)
        {
            clsReport objRpt = null;
            clsReport objRptSR = null;
            try
            {

                ExcelEngine excelEngine = null;
                IApplication application = null;
                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;
                excelEngine.Excel.DefaultVersion = ExcelVersion.Excel2013;
                var reportUtility = new ReportUtility();
                var workbook = reportUtility.GetWorkbook(ref excelEngine, 4);
                workbook.Version = ExcelVersion.Excel2013;
                var sheet1 = workbook.Worksheets[0];

                #region Logo
                string strPath = "";
                Image companyLogo = null;
                try
                {
                    DataTable dtCompanyImage = _sqlRepository.GetDataTable("SELECT * FROM ORG.COMPANY WHERE ID = '" + companyId + @"'");

                    strPath = Path.Combine(ResourcesPathReader.GetLogoOrImagePath(), dtCompanyImage.Rows[0]["Image"].ToString());  // IDCardEng.xlsx
                    companyLogo = Image.FromFile(strPath);
                }
                catch (Exception)
                {
                }
                #endregion
                objRpt = new clsReport();

                objRptSR = new clsReport(_sqlRepository);

                DataTable dtGStReceivableF3 = null;
                string taxyearId = GetTaxYearId(fromDate, toDate, companyId);
                dtGStReceivableF3 = GetGSTReceivableSQL3(companyGroupId, companyId, plantId, plantName, fromDate, toDate, taxyearId);
                if (dtGStReceivableF3.Rows.Count == 0)
                {
                    throw new Exception("No Data Found....");
                }

                DataTable dtCmp = objRptSR.SelectedCompanyDT(plantId);

                DataTable dtFactory = objRptSR.SelectedPlantDT(plantId);

                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;

                int xlsRow = 1, xlsCol = 1;
                int endXlsCol = 1;
                string FactoryName = "";
                string CmpName = "";
                xlsRow = 6;
                sheet1.Range[xlsRow - 1, 1].Text = "Report Ref No:";
                sheet1.Range[xlsRow - 1, 1].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow - 1, 1].RowHeight = 20;
                sheet1.Range[xlsRow - 1, 1].CellStyle.Font.Bold = true;

                int iVoucherType = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Voucher Type";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                xlsCol++;


                int iPartyName = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Party Name";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 30;
                xlsCol++;

                //int iPartyPlantName = xlsCol;
                //sheet1.Range[xlsRow, xlsCol].Text = "Party Plant";
                //sheet1.Range[xlsRow, xlsCol].ColumnWidth = 25;
                //xlsCol++;

                int iGSTIN = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "GSTIN(Party Plant)";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 25;
                xlsCol++;

                int iPlaceofSupply = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Place of Supply";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 20;
                xlsCol++;

                int iReverseCharge = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Reverse Charge";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 25;
                xlsCol++;

                int iSuppliesundersection7ofIGSTAct = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Supplies under section 7 of IGST Act";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 30;
                xlsCol++;

                int iInvoiceType = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Invoice Type";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 20;
                xlsCol++;

                int iECommerceGSTIN = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "E-Commerce GSTIN";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 20;
                xlsCol++;

                int iItemName = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Item Name";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 35;
                xlsCol++;

                int iHSNSAC = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "HSN/SAC";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 25;
                xlsCol++;

                int iTaxableAmount = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Taxable Amount";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                xlsCol++;

                int iRate = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Rate";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                xlsCol++;

                int iCessAmount = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Cess Amount";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                xlsCol++;

                int iApplicableofTaxRate = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Applicable % of Tax Rate";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                xlsCol++;

                int iVoucherNo = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Voucher No";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                xlsCol++;

                int iEntryDate = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Entry Date";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                xlsCol++;

                int iPostingDate = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Posting Date";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                xlsCol++;

                int iDocRefNo = xlsCol; // Doc Ref
                sheet1.Range[xlsRow, xlsCol].Text = "DocRef No";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;

                xlsCol++;
                int iDocDate = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Doc Date";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;

                xlsCol++;
                int iGRNNo = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Invoice No";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;

                //xlsCol++;
                //int iTaxableAmount = xlsCol;
                //sheet1.Range[xlsRow, xlsCol].Text = "Taxable Amount";
                //sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                //sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignRight;
                DataTable dtTaxCode = null;
                dtGStReceivableF3.DefaultView.Sort = "TCSequence";
                dtTaxCode = dtGStReceivableF3.DefaultView.ToTable(true, "TaxCode");
                dtTaxCode.Columns.Add("ColumnNumber", typeof(String));
                dtTaxCode.Columns.Add("ColumnFormula", typeof(String));

                if (dtTaxCode.Rows.Count > 0)
                {
                    for (int i = 0; i < dtTaxCode.Rows.Count; i++)
                    {
                        xlsCol++;
                        sheet1.Range[xlsRow, xlsCol].Text = dtTaxCode.Rows[i]["TaxCode"].ToString();
                        sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                        dtTaxCode.Rows[i]["ColumnNumber"] = xlsCol.ToString();
                    }
                }
                xlsCol++;
                int iTotalTax = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Total Tax";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                xlsCol++;
                int iGrossAmount = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Gross Amount";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 18;
                sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignRight;
                endXlsCol = xlsCol;
                endXlsCol = xlsCol;

                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].WrapText = true;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 23;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;

                string voucherNo = "";
                string Percentage = "";
                int startRow = 0;
                int perStartRow = 0;
                string formula = "";
                string formula2 = "";
                string totalFormula = "";

                string lineItemPercentageType = "";
                xlsRow++;
                startRow = xlsRow;
                perStartRow = xlsRow;
                bool isFirst = true;
                string totalTaxformula = "";
                string voucherNocomp = "";
                string taxFitler = "";
                for (int i = 0; i < dtGStReceivableF3.Rows.Count; i++)
                {
                    voucherNocomp = dtGStReceivableF3.Rows[i]["VoucherNo"].ToString().ToUpper();
                    taxFitler = " and VoucherNo = '" + dtGStReceivableF3.Rows[i]["VoucherNo"].ToString() + "'";
                    if (voucherNo != voucherNocomp)
                    {
                        if (isFirst == false)
                        {

                            //sheet1[perStartRow, iCategory, xlsRow - 1, iCategory].BorderAround(ExcelLineStyle.Hair);
                            sheet1[perStartRow, iVoucherType, xlsRow - 1, iVoucherType].BorderAround(ExcelLineStyle.Hair);
                            sheet1[perStartRow, iPartyName, xlsRow - 1, iPartyName].BorderAround(ExcelLineStyle.Hair);
                            sheet1[perStartRow, iPostingDate, xlsRow - 1, iPostingDate].BorderAround(ExcelLineStyle.Hair);
                            sheet1[perStartRow, iGSTIN, xlsRow - 1, iGSTIN].BorderAround(ExcelLineStyle.Hair);
                            sheet1[perStartRow, iVoucherNo, xlsRow - 1, iVoucherNo].BorderAround(ExcelLineStyle.Hair);
                            sheet1[perStartRow, iEntryDate, xlsRow - 1, iEntryDate].BorderAround(ExcelLineStyle.Hair);
                            sheet1[perStartRow, iPostingDate, xlsRow - 1, iPostingDate].BorderAround(ExcelLineStyle.Hair);
                            sheet1[perStartRow, iDocRefNo, xlsRow - 1, iDocRefNo].BorderAround(ExcelLineStyle.Hair);
                            sheet1[perStartRow, iDocDate, xlsRow - 1, iDocDate].BorderAround(ExcelLineStyle.Hair);
                            sheet1[perStartRow, iGRNNo, xlsRow - 1, iGRNNo].BorderAround(ExcelLineStyle.Hair);
                            sheet1[perStartRow, iTaxableAmount, xlsRow - 1, iTaxableAmount].BorderAround(ExcelLineStyle.Hair);
                            sheet1[perStartRow, iTotalTax, xlsRow - 1, iTotalTax].BorderAround(ExcelLineStyle.Hair);
                            sheet1[perStartRow, iGrossAmount, xlsRow - 1, iGrossAmount].BorderAround(ExcelLineStyle.Hair);

                            formula = "SUM(" + clsStaticInfo.GetxlsCol(iTaxableAmount) + perStartRow + ":" + clsStaticInfo.GetxlsCol(iTaxableAmount) + (xlsRow - 1) + ")";
                            formula2 = "";

                            if (dtTaxCode.Rows.Count > 0)
                            {
                                totalTaxformula = "SUM(";
                                for (int j = 0; j < dtTaxCode.Rows.Count; j++)
                                {
                                    sheet1[perStartRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"]), xlsRow - 1, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])].BorderAround(ExcelLineStyle.Hair);
                                    formula2 = "SUM(" + clsStaticInfo.GetxlsCol(Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])) + perStartRow + ":" + clsStaticInfo.GetxlsCol(Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])) + (xlsRow - 1) + ")";
                                    sheet1[xlsRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"]), xlsRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])].Formula = formula2;

                                    dtTaxCode.Rows[j]["ColumnFormula"] += (clsStaticInfo.GetxlsCol(Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])) + xlsRow).ToString() + " + ";
                                    totalTaxformula += clsStaticInfo.GetxlsCol(Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])) + xlsRow + ":";
                                }
                            }
                        }
                        isFirst = false;


                        sheet1.Range[xlsRow, iVoucherType].Text = dtGStReceivableF3.Rows[i]["SourceType"].ToString();
                        sheet1.Range[xlsRow, iPartyName].Text = dtGStReceivableF3.Rows[i]["PartyName"].ToString();
                        //sheet1.Range[xlsRow, iPartyPlantName].Text = dtGStReceivableF3.Rows[i]["PartyPlantName"].ToString();
                        sheet1.Range[xlsRow, iGSTIN].Text = dtGStReceivableF3.Rows[i]["GSTIN"].ToString();
                        sheet1.Range[xlsRow, iPlaceofSupply].Text = dtGStReceivableF3.Rows[i]["PlaceofSupply"].ToString();
                        sheet1.Range[xlsRow, iReverseCharge].Text = dtGStReceivableF3.Rows[i]["ReverseCharge"].ToString();
                        sheet1.Range[xlsRow, iSuppliesundersection7ofIGSTAct].Text = dtGStReceivableF3.Rows[i]["Suppliesundersection7ofIGSTAct"].ToString();
                        sheet1.Range[xlsRow, iInvoiceType].Text = dtGStReceivableF3.Rows[i]["InvoiceType"].ToString();
                        sheet1.Range[xlsRow, iECommerceGSTIN].Text = dtGStReceivableF3.Rows[i]["ECommerceGSTIN"].ToString();
                        sheet1.Range[xlsRow, iItemName].Text = dtGStReceivableF3.Rows[i]["ItemName"].ToString();
                        sheet1.Range[xlsRow, iHSNSAC].Text = dtGStReceivableF3.Rows[i]["HSNSAC"].ToString();

                        sheet1.Range[xlsRow, iRate].Text = dtGStReceivableF3.Rows[i]["Rate"].ToString();
                        sheet1.Range[xlsRow, iRate].NumberFormat = "#,##0.00;(#,##0.00)";

                        sheet1.Range[xlsRow, iCessAmount].Text = dtGStReceivableF3.Rows[i]["CessAmount"].ToString();
                        sheet1.Range[xlsRow, iCessAmount].NumberFormat = "#,##0.00;(#,##0.00)";

                        sheet1.Range[xlsRow, iApplicableofTaxRate].Text = dtGStReceivableF3.Rows[i]["ApplicableofTaxRate"].ToString();
                        sheet1.Range[xlsRow, iApplicableofTaxRate].NumberFormat = "#,##0.00;(#,##0.00)";

                        sheet1.Range[xlsRow, iVoucherNo].Text = dtGStReceivableF3.Rows[i]["VoucherNo"].ToString();

                        sheet1.Range[xlsRow, iEntryDate].Text = dtGStReceivableF3.Rows[i]["EntryDate"].ToString();
                        sheet1.Range[xlsRow, iPostingDate].Text = clsStaticInfo.GetDateTaxFormate(dtGStReceivableF3.Rows[i]["PostingDate"].ToString());
                        sheet1.Range[xlsRow, iDocRefNo].Text = dtGStReceivableF3.Rows[i]["DocRefNo"].ToString();
                        sheet1.Range[xlsRow, iDocDate].Text = dtGStReceivableF3.Rows[i]["DocDate"].ToString();
                        sheet1.Range[xlsRow, iGRNNo].Text = dtGStReceivableF3.Rows[i]["GRNNo"].ToString();
                        //sheet1.Range[xlsRow, iTaxPercentage].Text = dtGStReceivableF3.Rows[i]["TaxPercentage"].ToString();
                        //sheet1.Range[xlsRow, iTotalTax].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                        sheet1.Range[xlsRow, iTotalTax].NumberFormat = "#,##0.00;(#,##0.00)";
                        //sheet1.Range[xlsRow, iGrossAmount].Number =clsStaticInfo.dbl( dtGStReceivableF3.Rows[i]["TaxableAmount"].ToString());


                        sheet1.Range[xlsRow, iTaxableAmount].Number = clsStaticInfo.dbl(dtGStReceivableF3.Rows[i]["TaxableAmount"].ToString());//TaxableAmount
                        //sheet1.Range[xlsRow, iTaxableAmount].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                        sheet1.Range[xlsRow, iTaxableAmount].NumberFormat = "#,##0.00;(#,##0.00)";
                        //dtRCMPayable.DefaultView.RowFilter = "VoucherNo = '" + dtRCMPayable.Rows[i]["VoucherNo"].ToString() + "'";

                        if (dtTaxCode.Rows.Count > 0)
                        {
                            totalTaxformula = "=SUM(";
                            for (int j = 0; j < dtTaxCode.Rows.Count; j++)
                            {
                                dtGStReceivableF3.DefaultView.RowFilter = "TaxCode = '" + dtTaxCode.Rows[j]["TaxCode"].ToString() + "'" + taxFitler;
                                if (dtGStReceivableF3.DefaultView.Count > 0)
                                {

                                    sheet1.Range[xlsRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])].Number = clsStaticInfo.dbl(dtGStReceivableF3.DefaultView[0]["CrAmount"].ToString());
                                    //sheet1.Range[xlsRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                                    sheet1.Range[xlsRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])].NumberFormat = "#,##0.00;(#,##0.00)";
                                }
                                else
                                {
                                    sheet1.Range[xlsRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])].Text = "-";
                                    sheet1.Range[xlsRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])].HorizontalAlignment = ExcelHAlign.HAlignRight;

                                }
                                totalTaxformula += clsStaticInfo.GetxlsCol(Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])) + xlsRow + ":";
                            }
                            sheet1.Range[xlsRow, iTotalTax].Formula = totalTaxformula.Remove(totalTaxformula.Length - 1) + ")";

                        }
                        sheet1.Range[xlsRow, iGrossAmount].Formula = clsStaticInfo.GetxlsCol(iTaxableAmount) + xlsRow + "+" + clsStaticInfo.GetxlsCol(iTotalTax) + xlsRow;
                        //sheet1.Range[xlsRow, iGrossAmount].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                        sheet1.Range[xlsRow, iGrossAmount].NumberFormat = "#,##0.00;(#,##0.00)";
                        //Percentage = dtGStReceivableF3.Rows[i]["TaxPercentage"].ToString();

                        xlsRow++;
                    }

                    voucherNo = dtGStReceivableF3.Rows[i]["VoucherNo"].ToString().ToUpper();


                }
                //sheet1[perStartRow, iCategory, xlsRow - 1, iCategory].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iVoucherType, xlsRow - 1, iVoucherType].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iPartyName, xlsRow - 1, iPartyName].BorderAround(ExcelLineStyle.Hair);
                //sheet1[perStartRow, iPartyPlantName, xlsRow - 1, iPartyPlantName].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iGSTIN, xlsRow - 1, iGSTIN].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iVoucherNo, xlsRow - 1, iVoucherNo].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iPostingDate, xlsRow - 1, iPostingDate].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iEntryDate, xlsRow - 1, iEntryDate].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iDocRefNo, xlsRow - 1, iDocRefNo].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iDocDate, xlsRow - 1, iDocDate].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iGRNNo, xlsRow - 1, iGRNNo].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iTaxableAmount, xlsRow - 1, iTaxableAmount].BorderAround(ExcelLineStyle.Hair);
                //sheet1[perStartRow, iTaxableAmount, xlsRow - 1, iTaxableAmount].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iTotalTax, xlsRow - 1, iTotalTax].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iGrossAmount, xlsRow - 1, iGrossAmount].BorderAround(ExcelLineStyle.Hair);


                if (dtTaxCode.Rows.Count > 0)
                {
                    for (int j = 0; j < dtTaxCode.Rows.Count; j++)
                    {
                        sheet1[perStartRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"]), xlsRow - 1, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])].BorderAround(ExcelLineStyle.Hair);

                    }
                }

                if (dtTaxCode.Rows.Count > 0)
                {
                    xlsRow++;
                    totalTaxformula = "=SUM(";
                    for (int j = 0; j < dtTaxCode.Rows.Count; j++)
                    {
                        sheet1[perStartRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"]), xlsRow - 1, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])].BorderAround(ExcelLineStyle.Hair);
                        formula2 = "SUM(" + clsStaticInfo.GetxlsCol(Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])) + perStartRow + ":" + clsStaticInfo.GetxlsCol(Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])) + (xlsRow - 1) + ")";
                        sheet1[xlsRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"]), xlsRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])].Formula = formula2;

                        dtTaxCode.Rows[j]["ColumnFormula"] += (clsStaticInfo.GetxlsCol(Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])) + xlsRow).ToString() + " + ";
                        totalTaxformula += clsStaticInfo.GetxlsCol(Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])) + xlsRow + ":";

                    }
                }


                sheet1.Range[xlsRow, 1, xlsRow, 1].Text = "Grand Total";
                formula = "SUM(" + clsStaticInfo.GetxlsCol(iTaxableAmount) + perStartRow + ":" + clsStaticInfo.GetxlsCol(iTaxableAmount) + (xlsRow - 1) + ")";



                sheet1[xlsRow, iTaxableAmount, xlsRow, iTaxableAmount].Formula = formula;
                sheet1[xlsRow, iTaxableAmount, xlsRow, iTaxableAmount].NumberFormat = "#,##0.00;(#,##0.00)";
                sheet1[xlsRow, iGrossAmount, xlsRow, iGrossAmount].Formula = clsStaticInfo.GetxlsCol(iTaxableAmount) + xlsRow + "+" + clsStaticInfo.GetxlsCol(iTotalTax) + xlsRow;
                sheet1[xlsRow, iGrossAmount, xlsRow, iGrossAmount].NumberFormat = "#,##0.00;(#,##0.00)";

                sheet1[xlsRow, iTotalTax, xlsRow, iTotalTax].Formula = totalTaxformula.Remove(totalTaxformula.Length - 1) + ")";
                sheet1[xlsRow, iTotalTax, xlsRow, iTotalTax].NumberFormat = "#,##0.00;(#,##0.00)";

                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Font.Bold = true;
                totalFormula += (clsStaticInfo.GetxlsCol(iTaxableAmount) + xlsRow).ToString() + "+";

                #region ******************Import******************
                var sheet2 = workbook.Worksheets[1];
                DataTable dtExport = null;
                dtExport = GetImportSQL();

                if (dtExport.Rows.Count == 0)
                {
                    throw new Exception("No Data Found....");
                }


                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;

                int xlsRow2 = 1, xlsCol2 = 1;
                int endXlsCol2 = 1;

                int iExportType = xlsCol2;
                sheet2.Range[xlsRow2, xlsCol2].Text = "Export Type";
                sheet2.Range[xlsRow2, xlsCol2].ColumnWidth = 15;
                xlsCol2++;


                int iInvoiceNumber = xlsCol2;
                sheet2.Range[xlsRow2, xlsCol2].Text = "Invoice Number";
                sheet2.Range[xlsRow2, xlsCol2].ColumnWidth = 25;
                xlsCol2++;

                int iInvoiceDate = xlsCol2;
                sheet2.Range[xlsRow2, xlsCol2].Text = "Invoice Date";
                sheet2.Range[xlsRow2, xlsCol2].ColumnWidth = 25;
                xlsCol2++;

                int iInvoiceValue = xlsCol2;
                sheet2.Range[xlsRow2, xlsCol2].Text = "Invoice Value";
                sheet2.Range[xlsRow2, xlsCol2].ColumnWidth = 20;
                xlsCol2++;

                int iHSNSAC2 = xlsCol2;
                sheet2.Range[xlsRow2, xlsCol2].Text = "HSN/SAC";
                sheet2.Range[xlsRow2, xlsCol2].ColumnWidth = 25;
                xlsCol2++;

                int iPortCode = xlsCol2;
                sheet2.Range[xlsRow2, xlsCol2].Text = "Port Code";
                sheet2.Range[xlsRow2, xlsCol2].ColumnWidth = 30;
                xlsCol2++;

                int iShippingBillNumber = xlsCol2;
                sheet2.Range[xlsRow2, xlsCol2].Text = "Shipping Bill Number";
                sheet2.Range[xlsRow2, xlsCol2].ColumnWidth = 20;
                xlsCol2++;

                int iShippingBillDate = xlsCol2;
                sheet2.Range[xlsRow2, xlsCol2].Text = "Shipping Bill Date";
                sheet2.Range[xlsRow2, xlsCol2].ColumnWidth = 20;
                xlsCol2++;

                int iRate2 = xlsCol2;
                sheet2.Range[xlsRow2, xlsCol2].Text = "Rate";
                sheet2.Range[xlsRow2, xlsCol2].ColumnWidth = 35;
                xlsCol2++;

                int iTaxableValue = xlsCol2;
                sheet2.Range[xlsRow2, xlsCol2].Text = "Taxable Value";
                sheet2.Range[xlsRow2, xlsCol2].ColumnWidth = 25;
                xlsCol2++;

                int iCessAmount2 = xlsCol2;
                sheet2.Range[xlsRow2, xlsCol2].Text = "Cess Amount";
                sheet2.Range[xlsRow2, xlsCol2].ColumnWidth = 15;
                xlsCol2++;

                int iApplicableOfTaxRatePur = xlsCol2;
                sheet2.Range[xlsRow2, xlsCol2].Text = "Applicable % Of Tax Rate";
                sheet2.Range[xlsRow2, xlsCol2].ColumnWidth = 25;
                xlsCol2++;

                int iIGSTPur = xlsCol2;
                sheet2.Range[xlsRow2, xlsCol2].Text = "IGST";
                sheet2.Range[xlsRow2, xlsCol2].ColumnWidth = 18;
                sheet2.Range[xlsRow2, xlsCol2].HorizontalAlignment = ExcelHAlign.HAlignRight;
                endXlsCol2 = xlsCol2;

                sheet2.Range[xlsRow2, 1, xlsRow2, endXlsCol2].BorderInside(ExcelLineStyle.Hair);
                sheet2.Range[xlsRow2, 1, xlsRow2, endXlsCol2].BorderAround(ExcelLineStyle.Hair);
                sheet2.Range[xlsRow2, 1, xlsRow2, endXlsCol2].WrapText = true;
                sheet2.Range[xlsRow2, 1, xlsRow2, endXlsCol2].CellStyle.Font.Bold = true;
                sheet2.Range[xlsRow2, 1, xlsRow2, endXlsCol2].RowHeight = 23;
                sheet2.Range[xlsRow2, 1, xlsRow2, endXlsCol2].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;

                //string voucherNo = "";
                //string Percentage = "";
                int startRow2 = 0;
                int perStartRow2 = 0;
                //string formula = "";
                //string formula2 = "";
                //string totalFormula = "";

                //string lineItemPercentageType = "";
                xlsRow2++;
                startRow2 = xlsRow2;
                perStartRow2 = xlsRow2;
                bool isSecond = true;
                //string totalTaxformula = "";
                //string voucherNocomp = "";
                //string taxFitler = "";
                for (int i = 0; i < dtExport.Rows.Count; i++)
                {

                    //voucherNocomp = dtExport.Rows[i]["VoucherNo"].ToString().ToUpper();
                    //taxFitler = " and VoucherNo = '" + dtExport.Rows[i]["VoucherNo"].ToString() + "'";
                    //if (voucherNo != voucherNocomp)
                    //{
                    if (isSecond == false)
                    {
                        sheet2[perStartRow2, iExportType, xlsRow2 - 1, iExportType].BorderAround(ExcelLineStyle.Hair);
                        sheet2[perStartRow2, iInvoiceNumber, xlsRow2 - 1, iInvoiceNumber].BorderAround(ExcelLineStyle.Hair);
                        sheet2[perStartRow2, iInvoiceDate, xlsRow2 - 1, iInvoiceDate].BorderAround(ExcelLineStyle.Hair);
                        sheet2[perStartRow2, iInvoiceValue, xlsRow2 - 1, iInvoiceValue].BorderAround(ExcelLineStyle.Hair);
                        sheet2[perStartRow2, iHSNSAC2, xlsRow2 - 1, iHSNSAC2].BorderAround(ExcelLineStyle.Hair);
                        sheet2[perStartRow2, iPortCode, xlsRow2 - 1, iPortCode].BorderAround(ExcelLineStyle.Hair);
                        sheet2[perStartRow2, iShippingBillNumber, xlsRow2 - 1, iShippingBillNumber].BorderAround(ExcelLineStyle.Hair);
                        sheet2[perStartRow2, iShippingBillDate, xlsRow2 - 1, iShippingBillDate].BorderAround(ExcelLineStyle.Hair);
                        sheet2[perStartRow2, iRate2, xlsRow2 - 1, iRate2].BorderAround(ExcelLineStyle.Hair);
                        sheet2[perStartRow2, iTaxableValue, xlsRow2 - 1, iTaxableValue].BorderAround(ExcelLineStyle.Hair);
                        sheet2[perStartRow2, iCessAmount2, xlsRow2 - 1, iCessAmount2].BorderAround(ExcelLineStyle.Hair);
                        sheet2[perStartRow2, iApplicableOfTaxRatePur, xlsRow2 - 1, iApplicableOfTaxRatePur].BorderAround(ExcelLineStyle.Hair);
                        sheet2[perStartRow2, iIGSTPur, xlsRow2 - 1, iIGSTPur].BorderAround(ExcelLineStyle.Hair);

                    }
                    isSecond = false;


                    sheet2.Range[xlsRow2, iExportType].Text = dtExport.Rows[i]["ExportType"].ToString();
                    sheet2.Range[xlsRow2, iInvoiceNumber].Text = dtExport.Rows[i]["InvoiceNumber"].ToString();

                    sheet2.Range[xlsRow2, iInvoiceDate].Text = dtExport.Rows[i]["InvoiceDate"].ToString();
                    sheet2.Range[xlsRow2, iInvoiceValue].Text = dtExport.Rows[i]["InvoiceValue"].ToString();
                    sheet2.Range[xlsRow2, iHSNSAC2].Text = dtExport.Rows[i]["HSNSAC"].ToString();
                    sheet2.Range[xlsRow2, iPortCode].Text = dtExport.Rows[i]["PortCode"].ToString();
                    sheet2.Range[xlsRow2, iShippingBillNumber].Text = dtExport.Rows[i]["ShippingBillNumber"].ToString();
                    sheet2.Range[xlsRow2, iShippingBillDate].Text = dtExport.Rows[i]["ShippingBillDate"].ToString();

                    sheet2.Range[xlsRow2, iRate2].Number = clsStaticInfo.dbl(dtExport.Rows[i]["Rate"].ToString());
                    sheet2.Range[xlsRow2, iRate2].NumberFormat = "#,##0.00;(#,##0.00)";

                    sheet2.Range[xlsRow2, iTaxableValue].Number = clsStaticInfo.dbl(dtExport.Rows[i]["TaxableValue"].ToString());
                    sheet2.Range[xlsRow2, iTaxableValue].NumberFormat = "#,##0.00;(#,##0.00)";

                    sheet2.Range[xlsRow2, iCessAmount2].Number = clsStaticInfo.dbl(dtExport.Rows[i]["CessAmount"].ToString());
                    sheet2.Range[xlsRow2, iCessAmount2].NumberFormat = "#,##0.00;(#,##0.00)";

                    sheet2.Range[xlsRow2, iApplicableOfTaxRatePur].Number = clsStaticInfo.dbl(dtExport.Rows[i]["ApplicableOfTaxRate"].ToString());
                    sheet2.Range[xlsRow2, iApplicableOfTaxRatePur].NumberFormat = "#,##0.00;(#,##0.00)";

                    sheet2.Range[xlsRow2, iIGSTPur].Number = clsStaticInfo.dbl(dtExport.Rows[i]["IGST"].ToString());
                    sheet2.Range[xlsRow2, iIGSTPur].NumberFormat = "#,##0.00;(#,##0.00)";

                    xlsRow2++;

                }

                sheet2[perStartRow2, iExportType, xlsRow2 - 1, iExportType].BorderAround(ExcelLineStyle.Hair);
                sheet2[perStartRow2, iInvoiceNumber, xlsRow2 - 1, iInvoiceNumber].BorderAround(ExcelLineStyle.Hair);

                sheet2[perStartRow2, iInvoiceDate, xlsRow2 - 1, iInvoiceDate].BorderAround(ExcelLineStyle.Hair);
                sheet2[perStartRow2, iInvoiceValue, xlsRow2 - 1, iInvoiceValue].BorderAround(ExcelLineStyle.Hair);
                sheet2[perStartRow2, iHSNSAC2, xlsRow2 - 1, iHSNSAC2].BorderAround(ExcelLineStyle.Hair);
                sheet2[perStartRow2, iPortCode, xlsRow2 - 1, iPortCode].BorderAround(ExcelLineStyle.Hair);
                sheet2[perStartRow2, iShippingBillNumber, xlsRow2 - 1, iShippingBillNumber].BorderAround(ExcelLineStyle.Hair);
                sheet2[perStartRow2, iShippingBillDate, xlsRow2 - 1, iShippingBillDate].BorderAround(ExcelLineStyle.Hair);
                sheet2[perStartRow2, iRate2, xlsRow2 - 1, iRate2].BorderAround(ExcelLineStyle.Hair);
                sheet2[perStartRow2, iTaxableValue, xlsRow2 - 1, iTaxableValue].BorderAround(ExcelLineStyle.Hair);

                sheet2[perStartRow2, iCessAmount2, xlsRow2 - 1, iCessAmount2].BorderAround(ExcelLineStyle.Hair);
                sheet2[perStartRow2, iApplicableOfTaxRatePur, xlsRow2 - 1, iApplicableOfTaxRatePur].BorderAround(ExcelLineStyle.Hair);
                sheet2[perStartRow2, iIGSTPur, xlsRow2 - 1, iIGSTPur].BorderAround(ExcelLineStyle.Hair);


                #endregion ******************Import******************


                #region ******************CDNR******************
                var sheet3 = workbook.Worksheets[2];
                DataTable dtCDNR = null;
                dtCDNR = GetCDNRPurchase();

                //string taxyearId = GetTaxYearId(fromDate, toDate, companyId);
                //dtRCMPayable = GetGSTPayableSQL(companyGroupId, companyId, plantId, plantName, fromDate, toDate, taxyearId);

                if (dtCDNR.Rows.Count == 0)
                {
                    throw new Exception("No Data Found....");
                }


                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;

                int xlsRow3 = 1, xlsCol3 = 1;
                int endXlsCol3 = 1;

                int iGSTINUINofRecipient = xlsCol3;
                sheet3.Range[xlsRow3, xlsCol3].Text = "GSTIN/UIN of Recipient";
                sheet3.Range[xlsRow3, xlsCol3].ColumnWidth = 22;
                xlsCol3++;


                int iNoteRefundVoucherNumber = xlsCol3;
                sheet3.Range[xlsRow3, xlsCol3].Text = "Note Refund Voucher Number";
                sheet3.Range[xlsRow3, xlsCol3].ColumnWidth = 30;
                xlsCol3++;

                int iNoteRefundVoucherDate = xlsCol3;
                sheet3.Range[xlsRow3, xlsCol3].Text = "Note Refund Voucher Date";
                sheet3.Range[xlsRow3, xlsCol3].ColumnWidth = 25;
                xlsCol3++;

                int iHSNSAC3 = xlsCol3;
                sheet3.Range[xlsRow3, xlsCol3].Text = "HSN/SAC";
                sheet3.Range[xlsRow3, xlsCol3].ColumnWidth = 20;
                xlsCol3++;

                int iNoteType = xlsCol3;
                sheet3.Range[xlsRow3, xlsCol3].Text = "Note Type";
                sheet3.Range[xlsRow3, xlsCol3].ColumnWidth = 30;
                xlsCol3++;

                int iPlaceOfSupply = xlsCol3;
                sheet3.Range[xlsRow3, xlsCol3].Text = "Place Of Supply";
                sheet3.Range[xlsRow3, xlsCol3].ColumnWidth = 20;
                xlsCol3++;

                int iNoteSupplyType = xlsCol3;
                sheet3.Range[xlsRow3, xlsCol3].Text = "Note Supply Type";
                sheet3.Range[xlsRow3, xlsCol3].ColumnWidth = 20;
                xlsCol3++;

                int iReverseCharge3 = xlsCol3;
                sheet3.Range[xlsRow3, xlsCol3].Text = "Reverse Charge";
                sheet3.Range[xlsRow3, xlsCol3].ColumnWidth = 35;
                xlsCol3++;

                int iNoteRefundVoucherValue = xlsCol3;
                sheet3.Range[xlsRow3, xlsCol3].Text = "Note Refund Voucher Value";
                sheet3.Range[xlsRow3, xlsCol3].ColumnWidth = 28;
                xlsCol3++;

                int iRate3 = xlsCol3;
                sheet3.Range[xlsRow3, xlsCol3].Text = "Rate";
                sheet3.Range[xlsRow3, xlsCol3].ColumnWidth = 15;
                xlsCol3++;

                int iTaxableValue3 = xlsCol3;
                sheet3.Range[xlsRow3, xlsCol3].Text = "Taxable Value";
                sheet3.Range[xlsRow3, xlsCol3].ColumnWidth = 15;
                xlsCol3++;

                int iCessAmount3 = xlsCol3;
                sheet3.Range[xlsRow3, xlsCol3].Text = "Cess Amount";
                sheet3.Range[xlsRow3, xlsCol3].ColumnWidth = 18;
                sheet3.Range[xlsRow3, xlsCol3].HorizontalAlignment = ExcelHAlign.HAlignRight;
                endXlsCol3 = xlsCol3;

                int iApplicableofTaxRate3 = xlsCol3;
                sheet3.Range[xlsRow3, xlsCol3].Text = "Applicable of Tax Rate";
                sheet3.Range[xlsRow3, xlsCol3].ColumnWidth = 18;
                sheet3.Range[xlsRow3, xlsCol3].HorizontalAlignment = ExcelHAlign.HAlignRight;
                endXlsCol3 = xlsCol3;

                int iIGST3 = xlsCol3;
                sheet3.Range[xlsRow3, xlsCol3].Text = "IGST";
                sheet3.Range[xlsRow3, xlsCol3].ColumnWidth = 18;
                sheet3.Range[xlsRow3, xlsCol3].HorizontalAlignment = ExcelHAlign.HAlignRight;
                endXlsCol3 = xlsCol3;

                int iCGST = xlsCol3;
                sheet3.Range[xlsRow3, xlsCol3].Text = "CGST";
                sheet3.Range[xlsRow3, xlsCol3].ColumnWidth = 18;
                sheet3.Range[xlsRow3, xlsCol3].HorizontalAlignment = ExcelHAlign.HAlignRight;
                endXlsCol3 = xlsCol3;

                int iSGST = xlsCol3;
                sheet3.Range[xlsRow3, xlsCol3].Text = "SGST";
                sheet3.Range[xlsRow3, xlsCol3].ColumnWidth = 18;
                sheet3.Range[xlsRow3, xlsCol3].HorizontalAlignment = ExcelHAlign.HAlignRight;
                endXlsCol3 = xlsCol3;

                int iTotalTax3 = xlsCol3;
                sheet3.Range[xlsRow3, xlsCol3].Text = "Total Tax";
                sheet3.Range[xlsRow3, xlsCol3].ColumnWidth = 18;
                sheet3.Range[xlsRow3, xlsCol3].HorizontalAlignment = ExcelHAlign.HAlignRight;
                endXlsCol3 = xlsCol3;

                sheet3.Range[xlsRow3, 1, xlsRow3, endXlsCol3].BorderInside(ExcelLineStyle.Hair);
                sheet3.Range[xlsRow3, 1, xlsRow3, endXlsCol3].BorderAround(ExcelLineStyle.Hair);
                sheet3.Range[xlsRow3, 1, xlsRow3, endXlsCol3].WrapText = true;
                sheet3.Range[xlsRow3, 1, xlsRow3, endXlsCol3].CellStyle.Font.Bold = true;
                sheet3.Range[xlsRow3, 1, xlsRow3, endXlsCol3].RowHeight = 23;
                sheet3.Range[xlsRow3, 1, xlsRow3, endXlsCol3].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;

                int startRow3 = 0;
                int perStartRow3 = 0;

                xlsRow3++;
                startRow3 = xlsRow3;
                perStartRow3 = xlsRow3;
                bool isThird = true;

                for (int i = 0; i < dtCDNR.Rows.Count; i++)
                {
                    if (isThird == false)
                    {
                        sheet3[perStartRow3, iGSTINUINofRecipient, xlsRow3 - 1, iGSTINUINofRecipient].BorderAround(ExcelLineStyle.Hair);
                        sheet3[perStartRow3, iNoteRefundVoucherNumber, xlsRow3 - 1, iNoteRefundVoucherNumber].BorderAround(ExcelLineStyle.Hair);
                        sheet3[perStartRow3, iNoteRefundVoucherDate, xlsRow3 - 1, iNoteRefundVoucherDate].BorderAround(ExcelLineStyle.Hair);
                        sheet3[perStartRow3, iHSNSAC3, xlsRow3 - 1, iHSNSAC3].BorderAround(ExcelLineStyle.Hair);
                        sheet3[perStartRow3, iNoteType, xlsRow3 - 1, iNoteType].BorderAround(ExcelLineStyle.Hair);
                        sheet3[perStartRow3, iPlaceOfSupply, xlsRow3 - 1, iPlaceOfSupply].BorderAround(ExcelLineStyle.Hair);
                        sheet3[perStartRow3, iNoteSupplyType, xlsRow3 - 1, iNoteSupplyType].BorderAround(ExcelLineStyle.Hair);
                        sheet3[perStartRow3, iReverseCharge3, xlsRow3 - 1, iReverseCharge3].BorderAround(ExcelLineStyle.Hair);
                        sheet3[perStartRow3, iNoteRefundVoucherValue, xlsRow3 - 1, iNoteRefundVoucherValue].BorderAround(ExcelLineStyle.Hair);
                        sheet3[perStartRow3, iRate3, xlsRow3 - 1, iRate3].BorderAround(ExcelLineStyle.Hair);
                        sheet3[perStartRow3, iTaxableValue3, xlsRow3 - 1, iTaxableValue3].BorderAround(ExcelLineStyle.Hair);
                        sheet3[perStartRow3, iCessAmount3, xlsRow3 - 1, iCessAmount].BorderAround(ExcelLineStyle.Hair);
                        sheet3[perStartRow3, iApplicableofTaxRate3, xlsRow3 - 1, iApplicableofTaxRate3].BorderAround(ExcelLineStyle.Hair);
                        sheet3[perStartRow3, iIGST3, xlsRow3 - 1, iIGST3].BorderAround(ExcelLineStyle.Hair);
                        sheet3[perStartRow3, iCGST, xlsRow3 - 1, iCGST].BorderAround(ExcelLineStyle.Hair);
                        sheet3[perStartRow3, iSGST, xlsRow3 - 1, iSGST].BorderAround(ExcelLineStyle.Hair);
                        sheet3[perStartRow3, iTotalTax3, xlsRow3 - 1, iTotalTax3].BorderAround(ExcelLineStyle.Hair);

                    }
                    isThird = false;


                    sheet3.Range[xlsRow3, iGSTINUINofRecipient].Text = dtCDNR.Rows[i]["GSTINUINofRecipient"].ToString();
                    sheet3.Range[xlsRow3, iNoteRefundVoucherNumber].Text = dtCDNR.Rows[i]["NoteRefundVoucherNumber"].ToString();

                    sheet3.Range[xlsRow3, iNoteRefundVoucherDate].Text = dtCDNR.Rows[i]["NoteRefundVoucherDate"].ToString();
                    sheet3.Range[xlsRow3, iHSNSAC3].Text = dtCDNR.Rows[i]["HSNSAC"].ToString();
                    sheet3.Range[xlsRow3, iNoteType].Text = dtCDNR.Rows[i]["NoteType"].ToString();
                    sheet3.Range[xlsRow3, iPlaceOfSupply].Text = dtCDNR.Rows[i]["PlaceOfSupply"].ToString();
                    sheet3.Range[xlsRow3, iNoteSupplyType].Text = dtCDNR.Rows[i]["NoteSupplyType"].ToString();

                    sheet3.Range[xlsRow3, iReverseCharge3].Text = dtCDNR.Rows[i]["ReverseCharge"].ToString();
                    sheet3.Range[xlsRow3, iReverseCharge3].NumberFormat = "#,##0.00;(#,##0.00)";

                    sheet3.Range[xlsRow3, iRate3].Text = dtCDNR.Rows[i]["Rate"].ToString();
                    sheet3.Range[xlsRow3, iRate3].NumberFormat = "#,##0.00;(#,##0.00)";

                    sheet3.Range[xlsRow3, iTaxableValue3].Text = dtCDNR.Rows[i]["TaxableValue"].ToString();
                    sheet3.Range[xlsRow3, iTaxableValue3].NumberFormat = "#,##0.00;(#,##0.00)";

                    sheet3.Range[xlsRow3, iCessAmount3].Text = dtCDNR.Rows[i]["CessAmount"].ToString();
                    sheet3.Range[xlsRow3, iCessAmount3].NumberFormat = "#,##0.00;(#,##0.00)";

                    sheet3.Range[xlsRow3, iApplicableofTaxRate3].Number = clsStaticInfo.dbl(dtCDNR.Rows[i]["ApplicableofTaxRate"].ToString());
                    sheet3.Range[xlsRow3, iApplicableofTaxRate3].NumberFormat = "#,##0.00;(#,##0.00)";

                    sheet3.Range[xlsRow3, iIGST3].Number = clsStaticInfo.dbl(dtCDNR.Rows[i]["IGST"].ToString());
                    sheet3.Range[xlsRow3, iIGST3].NumberFormat = "#,##0.00;(#,##0.00)";

                    sheet3.Range[xlsRow3, iCGST].Number = clsStaticInfo.dbl(dtCDNR.Rows[i]["CGST"].ToString());
                    sheet3.Range[xlsRow3, iCGST].NumberFormat = "#,##0.00;(#,##0.00)";

                    sheet3.Range[xlsRow3, iSGST].Number = clsStaticInfo.dbl(dtCDNR.Rows[i]["SGST"].ToString());
                    sheet3.Range[xlsRow3, iSGST].NumberFormat = "#,##0.00;(#,##0.00)";

                    sheet3.Range[xlsRow3, iTotalTax3].Number = clsStaticInfo.dbl(dtCDNR.Rows[i]["TotalTax"].ToString());
                    sheet3.Range[xlsRow3, iTotalTax3].NumberFormat = "#,##0.00;(#,##0.00)";

                    xlsRow3++;
                }

                sheet3[perStartRow3, iGSTINUINofRecipient, xlsRow3 - 1, iGSTINUINofRecipient].BorderAround(ExcelLineStyle.Hair);
                sheet3[perStartRow3, iNoteRefundVoucherNumber, xlsRow3 - 1, iNoteRefundVoucherNumber].BorderAround(ExcelLineStyle.Hair);

                sheet3[perStartRow3, iNoteRefundVoucherDate, xlsRow3 - 1, iNoteRefundVoucherDate].BorderAround(ExcelLineStyle.Hair);
                sheet3[perStartRow3, iHSNSAC3, xlsRow3 - 1, iHSNSAC3].BorderAround(ExcelLineStyle.Hair);
                sheet3[perStartRow3, iNoteType, xlsRow3 - 1, iNoteType].BorderAround(ExcelLineStyle.Hair);
                sheet3[perStartRow3, iPlaceOfSupply, xlsRow3 - 1, iPlaceOfSupply].BorderAround(ExcelLineStyle.Hair);
                sheet3[perStartRow3, iNoteSupplyType, xlsRow3 - 1, iNoteSupplyType].BorderAround(ExcelLineStyle.Hair);
                sheet3[perStartRow3, iReverseCharge3, xlsRow3 - 1, iReverseCharge3].BorderAround(ExcelLineStyle.Hair);
                sheet3[perStartRow3, iRate3, xlsRow3 - 1, iRate3].BorderAround(ExcelLineStyle.Hair);
                sheet3[perStartRow3, iTaxableValue3, xlsRow3 - 1, iTaxableValue3].BorderAround(ExcelLineStyle.Hair);

                sheet3[perStartRow3, iCessAmount3, xlsRow3 - 1, iCessAmount3].BorderAround(ExcelLineStyle.Hair);
                sheet3[perStartRow3, iApplicableofTaxRate3, xlsRow3 - 1, iApplicableofTaxRate3].BorderAround(ExcelLineStyle.Hair);
                sheet3[perStartRow3, iIGST3, xlsRow3 - 1, iIGST3].BorderAround(ExcelLineStyle.Hair);
                sheet3[perStartRow3, iCGST, xlsRow3 - 1, iCGST].BorderAround(ExcelLineStyle.Hair);
                sheet3[perStartRow3, iSGST, xlsRow3 - 1, iSGST].BorderAround(ExcelLineStyle.Hair);
                sheet3[perStartRow3, iTotalTax3, xlsRow3 - 1, iTotalTax3].BorderAround(ExcelLineStyle.Hair);


                #endregion ******************CDNR******************

                #region ******************HSN******************

                var sheet4 = workbook.Worksheets[3];
                DataTable dtHSN = null;
                dtHSN = GetHSNPurchase();

                //string taxyearId = GetTaxYearId(fromDate, toDate, companyId);
                //dtRCMPayable = GetGSTPayableSQL(companyGroupId, companyId, plantId, plantName, fromDate, toDate, taxyearId);

                if (dtHSN.Rows.Count == 0)
                {
                    throw new Exception("No Data Found....");
                }


                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;

                int xlsRow4 = 1, xlsCol4 = 1;
                int endXlsCol4 = 1;


                int iHSN = xlsCol4;
                sheet4.Range[xlsRow4, xlsCol4].Text = "HSN";
                sheet4.Range[xlsRow4, xlsCol4].ColumnWidth = 15;
                xlsCol4++;


                int iDescription = xlsCol4;
                sheet4.Range[xlsRow4, xlsCol4].Text = "Description";
                sheet4.Range[xlsRow4, xlsCol4].ColumnWidth = 25;
                xlsCol4++;

                int iUQC = xlsCol4;
                sheet4.Range[xlsRow4, xlsCol4].Text = "UQC";
                sheet4.Range[xlsRow4, xlsCol4].ColumnWidth = 25;
                xlsCol4++;

                int iTotalQuantity = xlsCol4;
                sheet4.Range[xlsRow4, xlsCol4].Text = "Total Quantity";
                sheet4.Range[xlsRow4, xlsCol4].ColumnWidth = 20;
                xlsCol4++;

                int iTotalValue = xlsCol4;
                sheet4.Range[xlsRow4, xlsCol4].Text = "Total Value";
                sheet4.Range[xlsRow4, xlsCol4].ColumnWidth = 25;
                xlsCol4++;

                int iRate4 = xlsCol4;
                sheet4.Range[xlsRow4, xlsCol4].Text = "Rate";
                sheet4.Range[xlsRow4, xlsCol4].ColumnWidth = 30;
                xlsCol4++;

                int iTaxableValue4 = xlsCol4;
                sheet4.Range[xlsRow4, xlsCol4].Text = "Taxable Value";
                sheet4.Range[xlsRow4, xlsCol4].ColumnWidth = 20;
                xlsCol4++;

                int iIntegratedTaxAmount = xlsCol4;
                sheet4.Range[xlsRow4, xlsCol4].Text = "Integrated Tax Amount";
                sheet4.Range[xlsRow4, xlsCol4].ColumnWidth = 25;
                sheet4.Range[xlsRow4, xlsCol4].HorizontalAlignment = ExcelHAlign.HAlignRight;
                xlsCol4++;

                int iCentralTaxAmount = xlsCol4;
                sheet4.Range[xlsRow4, xlsCol4].Text = "Central Tax Amount";
                sheet4.Range[xlsRow4, xlsCol4].ColumnWidth = 35;
                sheet4.Range[xlsRow4, xlsCol4].HorizontalAlignment = ExcelHAlign.HAlignRight;
                xlsCol4++;

                int iStateUTTaxAmount = xlsCol4;
                sheet4.Range[xlsRow4, xlsCol4].Text = "State UT Tax Amount";
                sheet4.Range[xlsRow4, xlsCol4].ColumnWidth = 25;
                xlsCol4++;

                int iCessAmount4 = xlsCol4;
                sheet4.Range[xlsRow4, xlsCol4].Text = "Cess Amount";
                sheet4.Range[xlsRow4, xlsCol4].ColumnWidth = 15;
                sheet4.Range[xlsRow4, xlsCol4].HorizontalAlignment = ExcelHAlign.HAlignRight;
                endXlsCol4 = xlsCol4;

                sheet4.Range[xlsRow4, 1, xlsRow4, endXlsCol4].BorderInside(ExcelLineStyle.Hair);
                sheet4.Range[xlsRow4, 1, xlsRow4, endXlsCol4].BorderAround(ExcelLineStyle.Hair);
                sheet4.Range[xlsRow4, 1, xlsRow4, endXlsCol4].WrapText = true;
                sheet4.Range[xlsRow4, 1, xlsRow4, endXlsCol4].CellStyle.Font.Bold = true;
                sheet4.Range[xlsRow4, 1, xlsRow4, endXlsCol4].RowHeight = 23;
                sheet4.Range[xlsRow4, 1, xlsRow4, endXlsCol4].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;

                //string voucherNo = "";
                //string Percentage = "";
                int startRow4 = 0;
                int perStartRow4 = 0;
                //string formula = "";
                //string formula2 = "";
                //string totalFormula = "";

                //string lineItemPercentageType = "";
                xlsRow4++;
                startRow4 = xlsRow4;
                perStartRow4 = xlsRow4;
                bool isFourth = true;
                //string totalTaxformula = "";
                //string voucherNocomp = "";
                //string taxFitler = "";
                for (int i = 0; i < dtHSN.Rows.Count; i++)
                {
                    if (isFourth == false)
                    {
                        sheet4[perStartRow4, iHSN, xlsRow4 - 1, iHSN].BorderAround(ExcelLineStyle.Hair);
                        sheet4[perStartRow4, iDescription, xlsRow4 - 1, iDescription].BorderAround(ExcelLineStyle.Hair);
                        sheet4[perStartRow4, iUQC, xlsRow4 - 1, iUQC].BorderAround(ExcelLineStyle.Hair);
                        sheet4[perStartRow4, iTotalQuantity, xlsRow4 - 1, iTotalQuantity].BorderAround(ExcelLineStyle.Hair);
                        sheet4[perStartRow4, iTotalValue, xlsRow4 - 1, iTotalValue].BorderAround(ExcelLineStyle.Hair);
                        sheet4[perStartRow4, iRate4, xlsRow4 - 1, iRate4].BorderAround(ExcelLineStyle.Hair);
                        sheet4[perStartRow4, iTaxableValue4, xlsRow4 - 1, iTaxableValue4].BorderAround(ExcelLineStyle.Hair);
                        sheet4[perStartRow4, iIntegratedTaxAmount, xlsRow4 - 1, iIntegratedTaxAmount].BorderAround(ExcelLineStyle.Hair);
                        sheet4[perStartRow4, iCentralTaxAmount, xlsRow4 - 1, iCentralTaxAmount].BorderAround(ExcelLineStyle.Hair);
                        sheet4[perStartRow4, iStateUTTaxAmount, xlsRow4 - 1, iStateUTTaxAmount].BorderAround(ExcelLineStyle.Hair);
                        sheet4[perStartRow4, iCessAmount4, xlsRow4 - 1, iCessAmount4].BorderAround(ExcelLineStyle.Hair);

                    }
                    isFourth = false;


                    sheet4.Range[xlsRow4, iHSN].Text = dtHSN.Rows[i]["HSN"].ToString();
                    sheet4.Range[xlsRow4, iDescription].Text = dtHSN.Rows[i]["Description"].ToString();

                    sheet4.Range[xlsRow4, iUQC].Text = dtHSN.Rows[i]["UQC"].ToString();

                    sheet4.Range[xlsRow4, iTotalQuantity].Text = dtHSN.Rows[i]["TotalQuantity"].ToString();
                    sheet4.Range[xlsRow4, iTotalQuantity].NumberFormat = "#,##0.00;(#,##0.00)";

                    sheet4.Range[xlsRow4, iTotalValue].Text = dtHSN.Rows[i]["TotalValue"].ToString();
                    sheet4.Range[xlsRow4, iTotalValue].NumberFormat = "#,##0.00;(#,##0.00)";

                    sheet4.Range[xlsRow4, iRate4].Text = dtHSN.Rows[i]["Rate"].ToString();
                    sheet4.Range[xlsRow4, iRate4].NumberFormat = "#,##0.00;(#,##0.00)";

                    sheet4.Range[xlsRow4, iTaxableValue].Text = dtHSN.Rows[i]["TaxableValue"].ToString();
                    sheet4.Range[xlsRow4, iTaxableValue].NumberFormat = "#,##0.00;(#,##0.00)";

                    sheet4.Range[xlsRow4, iIntegratedTaxAmount].Text = dtHSN.Rows[i]["IntegratedTaxAmount"].ToString();
                    sheet4.Range[xlsRow4, iIntegratedTaxAmount].NumberFormat = "#,##0.00;(#,##0.00)";

                    sheet4.Range[xlsRow4, iCentralTaxAmount].Text = dtHSN.Rows[i]["CentralTaxAmount"].ToString();
                    sheet4.Range[xlsRow4, iCentralTaxAmount].NumberFormat = "#,##0.00;(#,##0.00)";

                    sheet4.Range[xlsRow4, iStateUTTaxAmount].Number = clsStaticInfo.dbl(dtHSN.Rows[i]["StateUTTaxAmount"].ToString());
                    sheet4.Range[xlsRow4, iStateUTTaxAmount].NumberFormat = "#,##0.00;(#,##0.00)";


                    sheet4.Range[xlsRow4, iCessAmount4].Text = dtHSN.Rows[i]["CessAmount"].ToString();
                    sheet4.Range[xlsRow4, iCessAmount4].NumberFormat = "#,##0.00;(#,##0.00)";

                    xlsRow4++;
                    //}

                    //voucherNo = dtHSN.Rows[i]["VoucherNo"].ToString().ToUpper();


                }

                sheet4[perStartRow4, iHSN, xlsRow4 - 1, iHSN].BorderAround(ExcelLineStyle.Hair);
                sheet4[perStartRow4, iDescription, xlsRow4 - 1, iDescription].BorderAround(ExcelLineStyle.Hair);

                sheet4[perStartRow4, iUQC, xlsRow4 - 1, iUQC].BorderAround(ExcelLineStyle.Hair);
                sheet4[perStartRow4, iTotalValue, xlsRow4 - 1, iTotalValue].BorderAround(ExcelLineStyle.Hair);
                sheet4[perStartRow4, iRate4, xlsRow4 - 1, iRate4].BorderAround(ExcelLineStyle.Hair);
                sheet4[perStartRow4, iTaxableValue4, xlsRow4 - 1, iTaxableValue4].BorderAround(ExcelLineStyle.Hair);
                sheet4[perStartRow4, iIntegratedTaxAmount, xlsRow4 - 1, iIntegratedTaxAmount].BorderAround(ExcelLineStyle.Hair);
                sheet4[perStartRow4, iCentralTaxAmount, xlsRow4 - 1, iCentralTaxAmount].BorderAround(ExcelLineStyle.Hair);
                sheet4[perStartRow4, iStateUTTaxAmount, xlsRow4 - 1, iStateUTTaxAmount].BorderAround(ExcelLineStyle.Hair);
                sheet4[perStartRow4, iCessAmount4, xlsRow4 - 1, iCessAmount4].BorderAround(ExcelLineStyle.Hair);

                #endregion ******************Export******************

                #region ******************Report Header******************


                xlsRow = 1;
                xlsCol = 3;
                try
                {
                    if (companyLogo != null)
                    {

                        double totalWidth = sheet1.GetColumnWidth(1) + sheet1.GetColumnWidth(3);
                        int totalWidthPixel = (int)(totalWidth * 7.5);
                        int totalheight = (int)((sheet1.GetRowHeight(1) + sheet1.GetRowHeight(2) + sheet1.GetRowHeight(3) + sheet1.GetRowHeight(3)) * 1.50);

                        companyLogo = ReportUtility.FixedSize(companyLogo, totalWidthPixel, totalheight);
                        IPictureShape pic = null;

                        pic = sheet1.Pictures.AddPicture(1, 1, companyLogo);
                        //pic.Height = 80;
                        //pic.Width = 220;
                    }
                }
                catch (Exception ex)
                {
                }

                FactoryName = string.Empty;

                string FactoryAddress = string.Empty;

                if (dtCmp.Rows.Count > 0)
                {
                    CmpName = dtCmp.Rows[0]["CompanyName"].ToString();
                }
                else
                {
                    CmpName = "";
                }
                sheet1.Range[xlsRow, 3].Text = CmpName;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 12;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 17;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                if (dtFactory.Rows.Count > 0)
                {
                    //FactoryName = dsFactory.Tables[0].Rows[0]["PlantName"].ToString();
                    FactoryName = dtFactory.Rows[0]["UserName"].ToString();
                }
                else
                {
                    FactoryName = "";
                }
                sheet1.Range[xlsRow, 3].Text = FactoryName;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 14;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 18;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                if (dtFactory.Rows.Count > 0)
                {
                    FactoryAddress = dtFactory.Rows[0]["Address1"].ToString();
                }
                else
                {
                    FactoryAddress = "";
                }
                sheet1.Range[xlsRow, 3].Text = FactoryAddress;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                //sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 22;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                sheet1.Range[xlsRow, 3].Text = "GST Payable Sales Report (Format 3) From " + fromDate + " To " + toDate;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;


                #endregion ******************Report Header******************

                #region Freeze Panes

                sheet1.IsDisplayZeros = false;
                sheet1.UsedRange["A7"].FreezePanes();
                sheet1.FirstVisibleColumn = 1;
                sheet1.FirstVisibleRow = 6;

                #endregion Freeze Panes

                #region UsedRange Alignment

                sheet1.UsedRange.WrapText = false;
                sheet1.UsedRange.CellStyle.Font.Size = 10;
                sheet1.Range["A1"].CellStyle.Font.Size = 14;
                sheet1.Range["A2"].CellStyle.Font.Size = 10;
                sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;
                sheet1.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                #endregion UsedRange Alignment

                #region Page Setup
                sheet1.PageSetup.TopMargin = 0.5;
                sheet1.PageSetup.BottomMargin = 0.7;
                sheet1.PageSetup.PrintTitleRows = "$1:$5";
                sheet1.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                sheet1.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + name + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:MM tt").ToString();
                sheet1.PageSetup.LeftMargin = 0.5;
                sheet1.PageSetup.RightMargin = 0.2;
                sheet1.PageSetup.Orientation = ExcelPageOrientation.Portrait;
                sheet1.PageSetup.FitToPagesTall = 0;
                sheet1.PageSetup.FitToPagesWide = 1;
                sheet1.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                sheet1.IsDisplayZeros = false;
                #endregion Page SetupLineItemType


                sheet1.Name = "B2B";
                sheet2.Name = "Import";
                sheet3.Name = "CDNR";
                sheet4.Name = "HSN";

                //sheet1.Range[6, 1, 6, endXlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                return workbook;
            }
            catch (System.Exception ex)
            {

                throw ex;
            }
        }
        private DataTable GetGSTReceivableSQL4(string companyGroupId, string companyId, string plantId, string plantName, string fromDate, string toDate, string taxyearId)
        {
            string strSql = "";
            strSql = @"SELECT	x.SourceType,x.VoucherNo,x.VoucherDate,x.PostingDate,x.DocRefNo,x.DocDate,x.PartyName,x.PartyPlantName,x.GSTIN
		,x.TaxCategoryType,x.TaxCode--,x.TaxPercentage
		,SUM(x.TaxableAmount) TaxableAmount,SUM(x.DrAmount) DrAmount,SUM(x.CrAmount) CrAmount
		,x.TCSequence,x.EntryDate,x.GRNNo
		FROM 

(
SELECT 
						'Expenses' SourceType
                            ,V.VoucherNo,format( V.PostingDate,'dd-MMM-yyyy')PostingDate, V.DocRefNo,format (V.DocDate,'dd-MMM-yyyy')DocDate
							,P.UserName PartyName,PP.GSTIN
							,NULL GRNNo,pp.UserName PartyPlantName
                            ,LineItemType=case when v.SourceType='InventoryPayable' then 'Material'
                            WHEN v.SourceType='VendorInvoice' THEN 'GL'
                            WHEN v.SourceType='VendorPayment' THEN 'GL'
                            ELSE '' END
                            --,Particular=CASE WHEN v.SourceType='VendorInvoice' THEN A.UserName
                            --WHEN v.SourceType='VendorPayment' THEN AP.UserName
                            --ELSE '' END
                            ,TaxableAmount=case when v.SourceType='InventoryPayable' then 0
                            when v.SourceType='VendorInvoice' then ISNULL(VD.DrAmount,0)
                            when v.SourceType='VendorPayment' then ISNULL(IWD.Amount,0) else 0 end
                            ,DrAmount=case when ITD.AType='Dr' then ISNULL(IT.TaxAmount,0) else 0 end
							,0 CrAmount
	                        ,format( v.VoucherDate,'dd-MMM-yyyy')VoucherDate
                            ,TC.TaxCategoryType,TC.Code TaxCode,TC.Sequence TCSequence,TC.UserName+'-'+TC.Code TaxCategory,IsNULL(TAXC.IsRCM,0) IsRCM
							
                            ,IsNULL(IV.IsExcludingTax,0) IsExcludingTax,0 IsTaxApplicable,TAXC.[Type],TAXC.ValueOfFixed
                            ,0 [Percentage],NULL HSNCodeId,NULL Material
							,TaxPercentage= case when v.SourceType='VendorInvoice' then taxc.ValueOfFixed
												  else 0 end
												 
												 , Format (IT.AddedDate,'dd-MMM-yyyy')EntryDate
                            from TRN.InvoiceTax IT
                            left join TRN.InvoiceTaxDetail ITD ON IT.Id=ITD.InvoiceTaxId AND ITD.AType='Dr'
                            LEFT JOIN TRN.Voucher V ON V.Id=IT.VoucherId
                            LEFT JOIN TRN.Invoice IV ON IV.Id=IT.InvoiceId
                            LEFT JOIN HKP.Activity TA ON TA.Id=ITD.ActivityId
                            LEFT JOIN HKP.Party P ON P.Id=IT.PartyId
							Left join hkp.PartyPlant PP on PP.Id=IT.PartyPlantId
                            LEFT JOIN MST.TaxCategory TC ON TC.Id=IT.TaxCategoryId
                            LEFT JOIN( select distinct TAC.Id,TAC.UserName,TAC.IsRCM,TAY.[Type],TACD.ValueOfFixed from MST.TaxCode TAC
                            LEFT JOIN MST.TaxCodeYear TAY ON TAY.TaxCodeId=TAC.Id
                            LEFT JOIN MST.TaxCodeDetail TACD ON TACD.TaxCodeId=TAC.Id WHERE TAY.TaxYearId IN (" + taxyearId + @") ) TAXC ON TAXC.Id=IT.TaxCodeId
                            LEFT JOIN TRN.VoucherDetail VD ON VD.Id=IT.VoucherDetailId
                            LEFT JOIN HKP.Activity A ON A.Id=VD.ActivityId
                            LEFT JOIN (SELECT IW.InvoiceWriteOffId,IW.ActivityId,SUM(I.Amount) Amount FROM TRN.InvoiceWriteOffDetail IW
                            JOIN TRN.Invoice I ON I.Id=IW.InvoiceId
                            GROUP BY InvoiceWriteOffId,ActivityId) IWD ON IWD.InvoiceWriteOffId=IT.InvoiceWriteOffId
                            LEFT JOIN HKP.Activity AP ON AP.Id=IWD.ActivityId
                            where TC.TaxCategoryType='GST' AND TAXC.IsRCM=0 AND  V.IsPark=0 AND V.PlantId='" + plantId + @"'
							and V.PostingDate between '" + fromDate + "' AND '" + toDate + @"'
                            AND v.SourceType IN ('VendorInvoice','VendorPayment')
                            
                            UNION all

							SELECT 'GRN' SourceType
                            ,V.VoucherNo,format( V.PostingDate,'dd-MMM-yyyy')PostingDate, V.DocRefNo,format (V.DocDate,'dd-MMM-yyyy')DocDate,P.UserName PartyName,PP.GSTIN
							, IRD.InventoryReceiveId GRNNo,pp.UserName PartyPlantName
                            ,LineItemType=case when v.SourceType='InventoryPayable' then 'Material'
                            WHEN v.SourceType='VendorInvoice' THEN 'GL'
                            WHEN v.SourceType='VendorPayment' THEN 'GL'
                            ELSE '' END
                            
                            ,TaxableAmount=case when v.SourceType='InventoryPayable' then sum(ISNULL(IRD.TotalMaterialTranAmount,0))
                            else 0 end
                            ,DrAmount=case when ITD.AType='Dr' then sum(ISNULL(IRT.TaxAmount,0)) else 0 end,0 CrAmount
	                        ,format( v.VoucherDate,'dd-MMM-yyyy')VoucherDate
                            ,TC.TaxCategoryType,TC.Code TaxCode,TC.Sequence TCSequence,TC.UserName+'-'+TC.Code TaxCategory
							,IsNULL(TAXC.IsRCM,0) IsRCM
                            ,IsNULL(IV.IsExcludingTax,0) IsExcludingTax,IsNULL(IR.IsTaxApplicable,0) IsTaxApplicable
							,TAXC.[Type],TAXC.ValueOfFixed
                            ,IRT.[Percentage],NULL HSNCodeId,null Material
							,TaxPercentage= case  when v.SourceType='InventoryPayable' AND IRT.[Percentage]>0 THEN IRT.[Percentage]
												 else 0 end
												 ,it.AddedDate EntryDate
                            from TRN.InvoiceTax IT
                            left join TRN.InvoiceTaxDetail ITD ON IT.Id=ITD.InvoiceTaxId AND ITD.AType='Dr'
                            LEFT JOIN TRN.Voucher V ON V.Id=IT.VoucherId
                            LEFT JOIN TRN.Invoice IV ON IV.Id=IT.InvoiceId
                            --LEFT JOIN TRN.InvoiceWriteOff IW ON IW.Id=IT.InvoiceWriteOffId
                            LEFT JOIN HKP.Activity TA ON TA.Id=ITD.ActivityId
                            LEFT JOIN HKP.Party P ON P.Id=IT.PartyId
							
                            LEFT JOIN MST.TaxCategory TC ON TC.Id=IT.TaxCategoryId
                            LEFT JOIN( select TAC.Id,TAC.UserName,TAC.IsRCM,TAY.[Type],TACD.ValueOfFixed from MST.TaxCode TAC
                            LEFT JOIN MST.TaxCodeYear TAY ON TAY.TaxCodeId=TAC.Id
                            LEFT JOIN MST.TaxCodeDetail TACD ON TACD.TaxCodeId=TAC.Id WHERE TAY.TaxYearId IN (" + taxyearId + @") 
							--and tac.IsRCM=0
							) TAXC ON TAXC.Id=IT.TaxCodeId
                            --LEFT JOIN SCS.TaxYear TY ON TY.Id=TAY.TaxYearId
                            LEFT JOIN TRN.InventoryReceive IR ON IR.VoucherId=V.Id
                            LEFT JOIN TRN.InventoryReceiveDetail IRD ON IRD.InventoryReceiveId=IR.Id
                            LEFT JOIN TRN.InventoryReceiveTax IRT ON IRD.Id=IRT.InventoryReceiveDetailId AND IRT.TaxCategoryId=IT.TaxCategoryId
                            --LEFT JOIN MST.HSNTaxPercentage HSNP ON IRT.HSNCodeId=HSNP.HSNCodeId AND HSNP.TaxCategoryId=IT.TaxCategoryId
                            LEFT JOIN TRN.InventoryMaterial IM ON IM.Id=IRD.InventoryMaterialId
                            LEFT JOIN MST.MaterialMaster MM ON MM.Id=IM.MaterialMasterId
                            LEFT JOIN TRN.VoucherDetail VD ON VD.Id=IT.VoucherDetailId
                            LEFT JOIN HKP.Activity A ON A.Id=VD.ActivityId
                            Left join hkp.PartyPlant pp on pp.Id=IR.InvoicingPartyPlantId
                            where TC.TaxCategoryType='GST' AND IR.IsTaxApplicable=0 AND V.IsPark=0
							AND V.PlantId = '" + plantId + @"' and V.PostingDate between '" + fromDate + "' AND '" + toDate + @"'
                            AND v.SourceType='InventoryPayable' and IRT.InventoryServiceId IS NULL
                            GROUP BY 
							V.VoucherNo,V.PostingDate, V.DocRefNo,V.DocDate,P.UserName ,PP.GSTIN
							, IRD.InventoryReceiveId ,pp.UserName 
                            , v.SourceType
                            ,v.VoucherDate
                            ,TC.TaxCategoryType,TC.Code ,TC.Sequence ,TC.UserName,TC.Code
							,IsNULL(TAXC.IsRCM,0) 
                            ,IsNULL(IV.IsExcludingTax,0) ,IsNULL(IR.IsTaxApplicable,0) 
							,TAXC.[Type],TAXC.ValueOfFixed,ITD.AType
                            ,IRT.[Percentage],IRT.[Percentage] ,it.AddedDate 
                            

                             UNION all
                            SELECT 'GRN' SourceType
                            ,V.VoucherNo,format( V.PostingDate,'dd-MMM-yyyy')PostingDate, V.DocRefNo,format (V.DocDate,'dd-MMM-yyyy')DocDate,P.UserName PartyName,PP.GSTIN
							, IRD.InventoryReceiveId GRNNo,pp.UserName PartyPlantName
                            ,LineItemType=case when v.SourceType='InventoryPayable' then 'Material'
                            WHEN v.SourceType='VendorInvoice' THEN 'GL'
                            WHEN v.SourceType='VendorPayment' THEN 'GL'
                            ELSE '' END
                            
                            ,TaxableAmount=case when v.SourceType='InventoryPayable' then 0
                            else 0 end
                            ,DrAmount=case when ITD.AType='Dr' then sum(ISNULL(IRT.TaxAmount,0)) else 0 end,0 CrAmount
	                        ,format( v.VoucherDate,'dd-MMM-yyyy')VoucherDate
                            ,TC.TaxCategoryType,TC.Code TaxCode,TC.Sequence TCSequence,TC.UserName+'-'+TC.Code TaxCategory
							,IsNULL(TAXC.IsRCM,0) IsRCM
                            ,IsNULL(IV.IsExcludingTax,0) IsExcludingTax,IsNULL(IR.IsTaxApplicable,0) IsTaxApplicable
							,TAXC.[Type],TAXC.ValueOfFixed
                            ,IRT.[Percentage],NULL HSNCodeId,null Material
							,TaxPercentage= case  when v.SourceType='InventoryPayable' AND IRT.[Percentage]>0 THEN IRT.[Percentage]
												 else 0 end
												 ,it.AddedDate EntryDate
                            from TRN.InvoiceTax IT
                            left join TRN.InvoiceTaxDetail ITD ON IT.Id=ITD.InvoiceTaxId AND ITD.AType='Dr'
                            LEFT JOIN TRN.Voucher V ON V.Id=IT.VoucherId
                            LEFT JOIN TRN.Invoice IV ON IV.Id=IT.InvoiceId
                            --LEFT JOIN TRN.InvoiceWriteOff IW ON IW.Id=IT.InvoiceWriteOffId
                            LEFT JOIN HKP.Activity TA ON TA.Id=ITD.ActivityId
                            LEFT JOIN HKP.Party P ON P.Id=IT.PartyId
							
                            LEFT JOIN MST.TaxCategory TC ON TC.Id=IT.TaxCategoryId
                            LEFT JOIN( select TAC.Id,TAC.UserName,TAC.IsRCM,TAY.[Type],TACD.ValueOfFixed from MST.TaxCode TAC
                            LEFT JOIN MST.TaxCodeYear TAY ON TAY.TaxCodeId=TAC.Id
                            LEFT JOIN MST.TaxCodeDetail TACD ON TACD.TaxCodeId=TAC.Id WHERE TAY.TaxYearId IN (" + taxyearId + @")
							--and tac.IsRCM=0
							) TAXC ON TAXC.Id=IT.TaxCodeId
                            --LEFT JOIN SCS.TaxYear TY ON TY.Id=TAY.TaxYearId
                            LEFT JOIN TRN.InventoryReceive IR ON IR.VoucherId=V.Id
                            LEFT JOIN TRN.InventoryService IRD ON IRD.InventoryReceiveId=IR.Id
                            LEFT JOIN TRN.InventoryReceiveTax IRT ON IRD.Id=IRT.InventoryServiceId AND IRT.TaxCategoryId=IT.TaxCategoryId
                            LEFT JOIN TRN.VoucherDetail VD ON VD.Id=IT.VoucherDetailId
                            LEFT JOIN HKP.Activity A ON A.Id=VD.ActivityId
                            Left join hkp.PartyPlant pp on pp.Id=IR.InvoicingPartyPlantId
                            where TC.TaxCategoryType='GST' AND IR.IsTaxApplicable=0 AND V.IsPark=0
							AND V.PlantId = '" + plantId + @"' and V.PostingDate between '" + fromDate + "' AND '" + toDate + @"'


                            AND v.SourceType='InventoryPayable'  and IRT.InventoryServiceId<>''


                            GROUP BY 
							V.VoucherNo,V.PostingDate, V.DocRefNo,V.DocDate,P.UserName ,PP.GSTIN
							, IRD.InventoryReceiveId ,pp.UserName 
                            , v.SourceType
                            ,v.VoucherDate
                            ,TC.TaxCategoryType,TC.Code ,TC.Sequence ,TC.UserName,TC.Code
							,IsNULL(TAXC.IsRCM,0) 
                            ,IsNULL(IV.IsExcludingTax,0) ,IsNULL(IR.IsTaxApplicable,0) 
							,TAXC.[Type],TAXC.ValueOfFixed,ITD.AType
                            ,IRT.[Percentage],IRT.[Percentage] ,it.AddedDate

UNION ALL

							--****************TCS*********************************
                            SELECT 'GRN' SourceType
                            ,V.VoucherNo,format( V.PostingDate,'dd-MMM-yyyy')PostingDate, V.DocRefNo,format (V.DocDate,'dd-MMM-yyyy')DocDate,P.UserName PartyName,PP.GSTIN
							, IRD.InventoryReceiveId GRNNo,pp.UserName PartyPlantName
                            ,LineItemType=case when v.SourceType='InventoryPayable' then 'Material'
                            WHEN v.SourceType='VendorInvoice' THEN 'GL'
                            WHEN v.SourceType='VendorPayment' THEN 'GL'
                            ELSE '' END
                            
                            ,TaxableAmount=case when v.SourceType='InventoryPayable' then 0
                            else 0 end
                            ,DrAmount=case when ITD.AType='Dr' then sum(ISNULL(ITD.Amount,0)) else 0 end,0 CrAmount
	                        ,format( v.VoucherDate,'dd-MMM-yyyy')VoucherDate
                            ,TC.TaxCategoryType,TC.Code TaxCode,TC.Sequence TCSequence,TC.UserName+'-'+TC.Code TaxCategory
							,IsNULL(TAXC.IsRCM,0) IsRCM
                            ,IsNULL(IV.IsExcludingTax,0) IsExcludingTax,IsNULL(IR.IsTaxApplicable,0) IsTaxApplicable
							,TAXC.[Type],TAXC.ValueOfFixed
                            ,NULL [Percentage],NULL HSNCodeId,null Material
							,NULL TaxPercentage
												 ,it.AddedDate EntryDate
                            from TRN.InvoiceTax IT
                            left join TRN.InvoiceTaxDetail ITD ON IT.Id=ITD.InvoiceTaxId AND ITD.AType='Dr'
                            LEFT JOIN TRN.Voucher V ON V.Id=IT.VoucherId
                            LEFT JOIN TRN.Invoice IV ON IV.Id=IT.InvoiceId
                            --LEFT JOIN TRN.InvoiceWriteOff IW ON IW.Id=IT.InvoiceWriteOffId
                            LEFT JOIN HKP.Activity TA ON TA.Id=ITD.ActivityId
                            LEFT JOIN HKP.Party P ON P.Id=IT.PartyId
							
                            LEFT JOIN MST.TaxCategory TC ON TC.Id=IT.TaxCategoryId
                            LEFT JOIN( select TAC.Id,TAC.UserName,TAC.IsRCM,TAY.[Type],TACD.ValueOfFixed from MST.TaxCode TAC
                            LEFT JOIN MST.TaxCodeYear TAY ON TAY.TaxCodeId=TAC.Id
                            LEFT JOIN MST.TaxCodeDetail TACD ON TACD.TaxCodeId=TAC.Id WHERE TAY.TaxYearId IN (" + taxyearId + @")
							--and tac.IsRCM=0
							) TAXC ON TAXC.Id=IT.TaxCodeId
                            --LEFT JOIN SCS.TaxYear TY ON TY.Id=TAY.TaxYearId
                            LEFT JOIN TRN.InventoryReceive IR ON IR.VoucherId=V.Id
                            LEFT JOIN TRN.InventoryReceiveDetail IRD ON IRD.InventoryReceiveId=IR.Id
                            LEFT JOIN TRN.VoucherDetail VD ON VD.Id=IT.VoucherDetailId
                            LEFT JOIN HKP.Activity A ON A.Id=VD.ActivityId
                            Left join hkp.PartyPlant pp on pp.Id=IR.InvoicingPartyPlantId
                            where TC.TaxCategoryType='TCS' AND IR.IsTaxApplicable=0 AND V.IsPark=0
							AND V.PlantId = '" + plantId + @"' and V.PostingDate between '" + fromDate + "' AND '" + toDate + @"'



                            AND v.SourceType='InventoryPayable'  


                            GROUP BY 
							V.VoucherNo,V.PostingDate, V.DocRefNo,V.DocDate,P.UserName ,PP.GSTIN
							, IRD.InventoryReceiveId ,pp.UserName 
                            , v.SourceType
                            ,v.VoucherDate
                            ,TC.TaxCategoryType,TC.Code ,TC.Sequence ,TC.UserName,TC.Code
							,IsNULL(TAXC.IsRCM,0) 
                            ,IsNULL(IV.IsExcludingTax,0) ,IsNULL(IR.IsTaxApplicable,0) 
							,TAXC.[Type],TAXC.ValueOfFixed,ITD.AType
                           ,it.AddedDate 

			                UNION	ALL			
                            SELECT 'Service' SourceType
                            ,V.VoucherNo,format(V.PostingDate, 'dd-MMM-yyyy')PostingDate, V.DocRefNo,format(V.DocDate, 'dd-MMM-yyyy')DocDate,P.UserName PartyName, PP.GSTIN
							, IRD.ServiceAcknowledgementMasterId GRNNo,pp.UserName PartyPlantName
                              , LineItemType =case when v.SourceType = 'ServicePayable' then 'Service' ELSE '' END
                            
                            ,TaxableAmount =case when v.SourceType = 'ServicePayable' then ISNULL(IRD.Amount,0)

                             else 0 end
                            ,DrAmount =case when ITD.AType = 'Dr' then ISNULL(IRT.TaxAmount,0) else 0 end,0 CrAmount
	                        ,format(v.VoucherDate, 'dd-MMM-yyyy')VoucherDate
                            ,TC.TaxCategoryType,TC.Code TaxCode, TC.Sequence TCSequence, TC.UserName + '-' + TC.Code TaxCategory,IsNULL(TAXC.IsRCM, 0) IsRCM
                                   , IsNULL(IV.IsExcludingTax, 0) IsExcludingTax,IsNULL(IR.IsTaxApplicable, 0) IsTaxApplicable,TAXC.[Type],TAXC.ValueOfFixed
                            ,IRT.[Percentage],NULL HSNCodeId,null Material
							,TaxPercentage = case  when v.SourceType = 'InventoryPayable'  THEN IRT.[Percentage]

                                                 else 0 end
												 ,IT.AddedDate EntryDate
                            from TRN.InvoiceTax IT
                            left
                            join TRN.InvoiceTaxDetail ITD ON IT.Id = ITD.InvoiceTaxId AND ITD.AType = 'Dr'
                            LEFT JOIN TRN.Voucher V ON V.Id = IT.VoucherId
                            LEFT JOIN TRN.Invoice IV ON IV.Id = IT.InvoiceId
                            LEFT JOIN HKP.Activity TA ON TA.Id = ITD.ActivityId
                            LEFT JOIN HKP.Party P ON P.Id = IT.PartyId
							
                            LEFT JOIN MST.TaxCategory TC ON TC.Id = IT.TaxCategoryId
                            LEFT JOIN(select TAC.Id, TAC.UserName, TAC.IsRCM, TAY.[Type], TACD.ValueOfFixed from MST.TaxCode TAC
                            LEFT JOIN MST.TaxCodeYear TAY ON TAY.TaxCodeId= TAC.Id
                            LEFT JOIN MST.TaxCodeDetail TACD ON TACD.TaxCodeId= TAC.Id WHERE TAY.TaxYearId IN (" + taxyearId + @") 
							) TAXC ON TAXC.Id = IT.TaxCodeId
                            LEFT JOIN TRN.ServiceAcknowledgementMaster IR ON IR.VoucherId = V.Id
                            LEFT JOIN TRN.ServicePOAckTax IRT ON IRT.ServiceAcknowledgementMasterId = IR.Id AND IRT.TaxCategoryId = IT.TaxCategoryId
                            --LEFT JOIN MST.HSNTaxPercentage HSNP ON IRT.HSNCodeId = HSNP.HSNCodeId AND HSNP.TaxCategoryId = IT.TaxCategoryId
                            LEFT JOIN TRN.ServiceAcknowledgementDetail IRD ON IRD.Id = IRT.ServiceAcknowledgementDetailId
                            LEFT JOIN hkp.ServiceMaster SM ON SM.Id = IRD.ServiceMasterId
                             Left join hkp.PartyPlant pp on pp.Id=IR.InvoicingPartyPlantId

                            LEFT JOIN TRN.VoucherDetail VD ON VD.Id = IT.VoucherDetailId
                            LEFT JOIN HKP.Activity A ON A.Id = VD.ActivityId
                            LEFT JOIN(SELECT IW.InvoiceWriteOffId, IW.ActivityId, SUM(I.Amount) Amount FROM TRN.InvoiceWriteOffDetail IW
                            JOIN TRN.Invoice I ON I.Id= IW.InvoiceId
                            GROUP BY InvoiceWriteOffId, ActivityId) IWD ON IWD.InvoiceWriteOffId = IT.InvoiceWriteOffId
                            LEFT JOIN HKP.Activity AP ON AP.Id = IWD.ActivityId
                            where TC.TaxCategoryType = 'GST' AND IR.IsTaxApplicable = 0 AND V.IsPark = 0
							 AND V.PlantId = '" + plantId + @"' and V.PostingDate between '" + fromDate + "' AND '" + toDate + @"'
                            AND v.SourceType = 'ServicePayable' 
                            ) x
							group by x.VoucherNo,x.VoucherDate,x.PostingDate,x.DocRefNo,x.DocDate,x.PartyName
							,x.TCSequence,x.PartyPlantName,x.GSTIN,x.SourceType
							,x.TaxCategoryType,x.EntryDate,x.TaxCode,x.GRNNo --,x.TaxPercentage
							ORDER BY 1,2,4-- TaxPercentage,VoucherNo, DocDate ";
            return _sqlRepository.GetDataTable(strSql);
        }
        public IWorkbook GetGSTReceivableReport4(string companyGroupId, string companyId, string plantId, string plantName, string fromDate, string toDate, string name)
        {
            clsReport objRpt = null;
            clsReport objRptSR = null;
            try
            {

                ExcelEngine excelEngine = null;
                IApplication application = null;
                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;
                excelEngine.Excel.DefaultVersion = ExcelVersion.Excel2013;
                var reportUtility = new ReportUtility();
                var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
                workbook.Version = ExcelVersion.Excel2013;
                var sheet1 = workbook.Worksheets[0];

                #region Logo
                string strPath = "";
                Image companyLogo = null;
                try
                {
                    DataTable dtCompanyImage = _sqlRepository.GetDataTable("SELECT * FROM ORG.COMPANY WHERE ID = '" + companyId + @"'");

                    strPath = Path.Combine(ResourcesPathReader.GetLogoOrImagePath(), dtCompanyImage.Rows[0]["Image"].ToString());  // IDCardEng.xlsx
                    companyLogo = Image.FromFile(strPath);
                }
                catch (Exception)
                {
                }
                #endregion
                objRpt = new clsReport();

                objRptSR = new clsReport(_sqlRepository);

                DataTable dtGStReceivableF3 = null;
                string taxyearId = GetTaxYearId(fromDate, toDate, companyId);
                dtGStReceivableF3 = GetGSTReceivableSQL4(companyGroupId, companyId, plantId, plantName, fromDate, toDate, taxyearId);
                if (dtGStReceivableF3.Rows.Count == 0)
                {
                    throw new Exception("No Data Found....");
                }

                DataTable dtCmp = objRptSR.SelectedCompanyDT(plantId);

                DataTable dtFactory = objRptSR.SelectedPlantDT(plantId);

                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;

                int xlsRow = 1, xlsCol = 1;
                int endXlsCol = 1;
                string FactoryName = "";
                string CmpName = "";
                xlsRow = 6;
                sheet1.Range[xlsRow - 1, 1].Text = "Report Ref No:";
                sheet1.Range[xlsRow - 1, 1].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow - 1, 1].RowHeight = 20;
                sheet1.Range[xlsRow - 1, 1].CellStyle.Font.Bold = true;

                int iVoucherType = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Voucher Type";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                xlsCol++;


                int iPartyName = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Party Name";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 25;
                xlsCol++;

                int iPartyPlantName = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Party Plant";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 25;
                xlsCol++;

                int iGSTIN = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "GSTIN(Party Plant)";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 25;

                //xlsCol++;
                //int iParticulars = xlsCol; // Party
                //sheet1.Range[xlsRow, xlsCol].Text = "Particulars";
                //sheet1.Range[xlsRow, xlsCol].ColumnWidth = 40;

                xlsCol++;
                int iVoucherNo = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Voucher No";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;

                xlsCol++;
                int iEntryDate = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Entry Date";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;

                xlsCol++;
                int iPostingDate = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Posting Date";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                xlsCol++;
                int iDocRefNo = xlsCol; // Doc Ref
                sheet1.Range[xlsRow, xlsCol].Text = "DocRef No";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;

                xlsCol++;
                int iDocDate = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Doc Date";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;

                xlsCol++;
                int iGRNNo = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "GRN No";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;

                xlsCol++;
                int iTaxableAmount = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Taxable Amount";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignRight;
                DataTable dtTaxCode = null;
                dtGStReceivableF3.DefaultView.Sort = "TCSequence";
                dtTaxCode = dtGStReceivableF3.DefaultView.ToTable(true, "TaxCode");
                dtTaxCode.Columns.Add("ColumnNumber", typeof(String));
                dtTaxCode.Columns.Add("ColumnFormula", typeof(String));

                if (dtTaxCode.Rows.Count > 0)
                {
                    for (int i = 0; i < dtTaxCode.Rows.Count; i++)
                    {
                        xlsCol++;
                        sheet1.Range[xlsRow, xlsCol].Text = dtTaxCode.Rows[i]["TaxCode"].ToString();
                        sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                        dtTaxCode.Rows[i]["ColumnNumber"] = xlsCol.ToString();
                    }
                }
                xlsCol++;
                int iTotalTax = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Total Tax";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                xlsCol++;
                int iGrossAmount = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Gross Amount";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 18;
                sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignRight;
                endXlsCol = xlsCol;
                endXlsCol = xlsCol;

                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].WrapText = true;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 23;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;

                string voucherNo = "";
                string Percentage = "";
                int startRow = 0;
                int perStartRow = 0;
                string formula = "";
                string formula2 = "";
                string totalFormula = "";

                string lineItemPercentageType = "";
                xlsRow++;
                startRow = xlsRow;
                perStartRow = xlsRow;
                bool isFirst = true;
                string totalTaxformula = "";
                string voucherNocomp = "";
                string taxFitler = "";
                for (int i = 0; i < dtGStReceivableF3.Rows.Count; i++)
                {
                    voucherNocomp = dtGStReceivableF3.Rows[i]["VoucherNo"].ToString().ToUpper();
                    taxFitler = " and VoucherNo = '" + dtGStReceivableF3.Rows[i]["VoucherNo"].ToString() + "'";
                    if (voucherNo != voucherNocomp)
                    {
                        if (isFirst == false)
                        {

                            //sheet1[perStartRow, iCategory, xlsRow - 1, iCategory].BorderAround(ExcelLineStyle.Hair);
                            sheet1[perStartRow, iVoucherType, xlsRow - 1, iVoucherType].BorderAround(ExcelLineStyle.Hair);
                            sheet1[perStartRow, iPartyName, xlsRow - 1, iPartyName].BorderAround(ExcelLineStyle.Hair);
                            sheet1[perStartRow, iPostingDate, xlsRow - 1, iPostingDate].BorderAround(ExcelLineStyle.Hair);
                            sheet1[perStartRow, iGSTIN, xlsRow - 1, iGSTIN].BorderAround(ExcelLineStyle.Hair);
                            sheet1[perStartRow, iVoucherNo, xlsRow - 1, iVoucherNo].BorderAround(ExcelLineStyle.Hair);
                            sheet1[perStartRow, iEntryDate, xlsRow - 1, iEntryDate].BorderAround(ExcelLineStyle.Hair);
                            sheet1[perStartRow, iPostingDate, xlsRow - 1, iPostingDate].BorderAround(ExcelLineStyle.Hair);
                            sheet1[perStartRow, iDocRefNo, xlsRow - 1, iDocRefNo].BorderAround(ExcelLineStyle.Hair);
                            sheet1[perStartRow, iDocDate, xlsRow - 1, iDocDate].BorderAround(ExcelLineStyle.Hair);
                            sheet1[perStartRow, iGRNNo, xlsRow - 1, iGRNNo].BorderAround(ExcelLineStyle.Hair);
                            sheet1[perStartRow, iTaxableAmount, xlsRow - 1, iTaxableAmount].BorderAround(ExcelLineStyle.Hair);
                            sheet1[perStartRow, iTotalTax, xlsRow - 1, iTotalTax].BorderAround(ExcelLineStyle.Hair);
                            sheet1[perStartRow, iGrossAmount, xlsRow - 1, iGrossAmount].BorderAround(ExcelLineStyle.Hair);

                            formula = "SUM(" + clsStaticInfo.GetxlsCol(iTaxableAmount) + perStartRow + ":" + clsStaticInfo.GetxlsCol(iTaxableAmount) + (xlsRow - 1) + ")";
                            formula2 = "";

                            if (dtTaxCode.Rows.Count > 0)
                            {
                                totalTaxformula = "SUM(";
                                for (int j = 0; j < dtTaxCode.Rows.Count; j++)
                                {
                                    sheet1[perStartRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"]), xlsRow - 1, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])].BorderAround(ExcelLineStyle.Hair);
                                    formula2 = "SUM(" + clsStaticInfo.GetxlsCol(Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])) + perStartRow + ":" + clsStaticInfo.GetxlsCol(Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])) + (xlsRow - 1) + ")";
                                    sheet1[xlsRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"]), xlsRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])].Formula = formula2;

                                    dtTaxCode.Rows[j]["ColumnFormula"] += (clsStaticInfo.GetxlsCol(Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])) + xlsRow).ToString() + " + ";
                                    totalTaxformula += clsStaticInfo.GetxlsCol(Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])) + xlsRow + ":";
                                }
                            }
                        }
                        isFirst = false;


                        sheet1.Range[xlsRow, iVoucherType].Text = dtGStReceivableF3.Rows[i]["SourceType"].ToString();
                        sheet1.Range[xlsRow, iPartyName].Text = dtGStReceivableF3.Rows[i]["PartyName"].ToString();
                        sheet1.Range[xlsRow, iPartyPlantName].Text = dtGStReceivableF3.Rows[i]["PartyPlantName"].ToString();
                        sheet1.Range[xlsRow, iGSTIN].Text = dtGStReceivableF3.Rows[i]["GSTIN"].ToString();
                        sheet1.Range[xlsRow, iVoucherNo].Text = dtGStReceivableF3.Rows[i]["VoucherNo"].ToString();

                        sheet1.Range[xlsRow, iEntryDate].Text = dtGStReceivableF3.Rows[i]["EntryDate"].ToString();
                        sheet1.Range[xlsRow, iPostingDate].Text = clsStaticInfo.GetDateTaxFormate(dtGStReceivableF3.Rows[i]["PostingDate"].ToString());
                        sheet1.Range[xlsRow, iDocRefNo].Text = dtGStReceivableF3.Rows[i]["DocRefNo"].ToString();
                        sheet1.Range[xlsRow, iDocDate].Text = dtGStReceivableF3.Rows[i]["DocDate"].ToString();
                        sheet1.Range[xlsRow, iGRNNo].Text = dtGStReceivableF3.Rows[i]["GRNNo"].ToString();
                        //sheet1.Range[xlsRow, iTaxPercentage].Text = dtGStReceivableF3.Rows[i]["TaxPercentage"].ToString();
                        sheet1.Range[xlsRow, iTotalTax].NumberFormat = "#,##0.00;(#,##0.00)";
                        //sheet1.Range[xlsRow, iGrossAmount].Number =clsStaticInfo.dbl( dtGStReceivableF3.Rows[i]["TaxableAmount"].ToString());


                        sheet1.Range[xlsRow, iTaxableAmount].Number = clsStaticInfo.dbl(dtGStReceivableF3.Rows[i]["TaxableAmount"].ToString());//TaxableAmount
                        sheet1.Range[xlsRow, iTaxableAmount].NumberFormat = "#,##0.00;(#,##0.00)";
                        //dtRCMPayable.DefaultView.RowFilter = "VoucherNo = '" + dtRCMPayable.Rows[i]["VoucherNo"].ToString() + "'";

                        if (dtTaxCode.Rows.Count > 0)
                        {
                            totalTaxformula = "=SUM(";
                            for (int j = 0; j < dtTaxCode.Rows.Count; j++)
                            {
                                dtGStReceivableF3.DefaultView.RowFilter = "TaxCode = '" + dtTaxCode.Rows[j]["TaxCode"].ToString() + "'" + taxFitler;
                                if (dtGStReceivableF3.DefaultView.Count > 0)
                                {

                                    sheet1.Range[xlsRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])].Number = clsStaticInfo.dbl(dtGStReceivableF3.DefaultView[0]["DrAmount"].ToString());
                                    sheet1.Range[xlsRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])].NumberFormat = "#,##0.00;(#,##0.00)";
                                }
                                else
                                {
                                    sheet1.Range[xlsRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])].Text = "-";
                                    sheet1.Range[xlsRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])].HorizontalAlignment = ExcelHAlign.HAlignRight;

                                }
                                totalTaxformula += clsStaticInfo.GetxlsCol(Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])) + xlsRow + ":";
                            }
                            sheet1.Range[xlsRow, iTotalTax].Formula = totalTaxformula.Remove(totalTaxformula.Length - 1) + ")";

                        }
                        sheet1.Range[xlsRow, iGrossAmount].Formula = clsStaticInfo.GetxlsCol(iTaxableAmount) + xlsRow + "+" + clsStaticInfo.GetxlsCol(iTotalTax) + xlsRow;
                        sheet1.Range[xlsRow, iGrossAmount].NumberFormat = "#,##0.00;(#,##0.00)";
                        //Percentage = dtGStReceivableF3.Rows[i]["TaxPercentage"].ToString();

                        xlsRow++;
                    }
                    voucherNo = dtGStReceivableF3.Rows[i]["VoucherNo"].ToString().ToUpper();


                }
                //sheet1[perStartRow, iCategory, xlsRow - 1, iCategory].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iVoucherType, xlsRow - 1, iVoucherType].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iPartyName, xlsRow - 1, iPartyName].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iPartyPlantName, xlsRow - 1, iPartyPlantName].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iGSTIN, xlsRow - 1, iGSTIN].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iVoucherNo, xlsRow - 1, iVoucherNo].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iPostingDate, xlsRow - 1, iPostingDate].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iEntryDate, xlsRow - 1, iEntryDate].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iDocRefNo, xlsRow - 1, iDocRefNo].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iDocDate, xlsRow - 1, iDocDate].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iGRNNo, xlsRow - 1, iGRNNo].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iTaxableAmount, xlsRow - 1, iTaxableAmount].BorderAround(ExcelLineStyle.Hair);
                //sheet1[perStartRow, iTaxableAmount, xlsRow - 1, iTaxableAmount].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iTotalTax, xlsRow - 1, iTotalTax].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iGrossAmount, xlsRow - 1, iGrossAmount].BorderAround(ExcelLineStyle.Hair);


                if (dtTaxCode.Rows.Count > 0)
                {
                    for (int j = 0; j < dtTaxCode.Rows.Count; j++)
                    {
                        sheet1[perStartRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"]), xlsRow - 1, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])].BorderAround(ExcelLineStyle.Hair);

                    }
                }

                if (dtTaxCode.Rows.Count > 0)
                {
                    xlsRow++;
                    totalTaxformula = "=SUM(";
                    for (int j = 0; j < dtTaxCode.Rows.Count; j++)
                    {
                        sheet1[perStartRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"]), xlsRow - 1, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])].BorderAround(ExcelLineStyle.Hair);
                        formula2 = "SUM(" + clsStaticInfo.GetxlsCol(Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])) + perStartRow + ":" + clsStaticInfo.GetxlsCol(Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])) + (xlsRow - 1) + ")";
                        sheet1[xlsRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"]), xlsRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])].Formula = formula2;

                        dtTaxCode.Rows[j]["ColumnFormula"] += (clsStaticInfo.GetxlsCol(Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])) + xlsRow).ToString() + " + ";
                        totalTaxformula += clsStaticInfo.GetxlsCol(Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])) + xlsRow + ":";

                    }
                }


                sheet1.Range[xlsRow, 1, xlsRow, 1].Text = "Grand Total";
                formula = "SUM(" + clsStaticInfo.GetxlsCol(iTaxableAmount) + perStartRow + ":" + clsStaticInfo.GetxlsCol(iTaxableAmount) + (xlsRow - 1) + ")";



                sheet1[xlsRow, iTaxableAmount, xlsRow, iTaxableAmount].Formula = formula;
                sheet1[xlsRow, iTaxableAmount, xlsRow, iTaxableAmount].NumberFormat = "#,##0.00;(#,##0.00)";
                sheet1[xlsRow, iGrossAmount, xlsRow, iGrossAmount].Formula = clsStaticInfo.GetxlsCol(iTaxableAmount) + xlsRow + "+" + clsStaticInfo.GetxlsCol(iTotalTax) + xlsRow;
                sheet1[xlsRow, iGrossAmount, xlsRow, iGrossAmount].NumberFormat = "#,##0.00;(#,##0.00)";

                sheet1[xlsRow, iTotalTax, xlsRow, iTotalTax].Formula = totalTaxformula.Remove(totalTaxformula.Length - 1) + ")";
                sheet1[xlsRow, iTotalTax, xlsRow, iTotalTax].NumberFormat = "#,##0.00;(#,##0.00)";

                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Font.Bold = true;
                totalFormula += (clsStaticInfo.GetxlsCol(iTaxableAmount) + xlsRow).ToString() + "+";


                #region ******************Report Header******************


                xlsRow = 1;
                xlsCol = 3;
                try
                {
                    if (companyLogo != null)
                    {

                        double totalWidth = sheet1.GetColumnWidth(1) + sheet1.GetColumnWidth(3);
                        int totalWidthPixel = (int)(totalWidth * 7.5);
                        int totalheight = (int)((sheet1.GetRowHeight(1) + sheet1.GetRowHeight(2) + sheet1.GetRowHeight(3) + sheet1.GetRowHeight(3)) * 1.50);

                        companyLogo = ReportUtility.FixedSize(companyLogo, totalWidthPixel, totalheight);
                        IPictureShape pic = null;

                        pic = sheet1.Pictures.AddPicture(1, 1, companyLogo);
                        //pic.Height = 80;
                        //pic.Width = 220;
                    }
                }
                catch (Exception ex)
                {
                }

                FactoryName = string.Empty;

                string FactoryAddress = string.Empty;

                if (dtCmp.Rows.Count > 0)
                {
                    CmpName = dtCmp.Rows[0]["CompanyName"].ToString();
                }
                else
                {
                    CmpName = "";
                }
                sheet1.Range[xlsRow, 3].Text = CmpName;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 12;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 17;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                if (dtFactory.Rows.Count > 0)
                {
                    //FactoryName = dsFactory.Tables[0].Rows[0]["PlantName"].ToString();
                    FactoryName = dtFactory.Rows[0]["UserName"].ToString();
                }
                else
                {
                    FactoryName = "";
                }
                sheet1.Range[xlsRow, 3].Text = FactoryName;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 14;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 18;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                if (dtFactory.Rows.Count > 0)
                {
                    FactoryAddress = dtFactory.Rows[0]["Address1"].ToString();
                }
                else
                {
                    FactoryAddress = "";
                }
                sheet1.Range[xlsRow, 3].Text = FactoryAddress;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                //sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 22;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                sheet1.Range[xlsRow, 3].Text = "GST Recievable Report (Format 3) From " + fromDate + " To " + toDate;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;


                #endregion ******************Report Header******************

                #region Freeze Panes

                sheet1.IsDisplayZeros = false;
                sheet1.UsedRange["A7"].FreezePanes();
                sheet1.FirstVisibleColumn = 1;
                sheet1.FirstVisibleRow = 6;

                #endregion Freeze Panes

                #region UsedRange Alignment

                sheet1.UsedRange.WrapText = false;
                sheet1.UsedRange.CellStyle.Font.Size = 10;
                sheet1.Range["A1"].CellStyle.Font.Size = 14;
                sheet1.Range["A2"].CellStyle.Font.Size = 10;
                sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;
                sheet1.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                #endregion UsedRange Alignment

                #region Page Setup
                sheet1.PageSetup.TopMargin = 0.5;
                sheet1.PageSetup.BottomMargin = 0.7;
                sheet1.PageSetup.PrintTitleRows = "$1:$5";
                sheet1.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                sheet1.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + name + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:MM tt").ToString();
                sheet1.PageSetup.LeftMargin = 0.5;
                sheet1.PageSetup.RightMargin = 0.2;
                sheet1.PageSetup.Orientation = ExcelPageOrientation.Portrait;
                sheet1.PageSetup.FitToPagesTall = 0;
                sheet1.PageSetup.FitToPagesWide = 1;
                sheet1.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                sheet1.IsDisplayZeros = false;
                #endregion Page SetupLineItemType
                sheet1.Range[6, 1, 6, endXlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;


                sheet1.Name = "GST Receivable";
                return workbook;
            }
            catch (System.Exception ex)
            {

                throw ex;
            }
        }
        #region GST Payable

        #endregion GST Payable

        public IWorkbook GetDebitNoteCreditNoteTaxReport(string companyGroupId, string companyId, string plantId, string plantName, string fromDate, string toDate, string name)
        {
            clsReport objRpt = null;
            clsReport objRptSR = null;
            try
            {

                ExcelEngine excelEngine = null;
                IApplication application = null;
                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;
                excelEngine.Excel.DefaultVersion = ExcelVersion.Excel2013;
                var reportUtility = new ReportUtility();
                var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
                workbook.Version = ExcelVersion.Excel2013;
                var sheet1 = workbook.Worksheets[0];

                #region Logo
                string strPath = "";
                Image companyLogo = null;
                try
                {
                    DataTable dtCompanyImage = _sqlRepository.GetDataTable("SELECT * FROM ORG.COMPANY WHERE ID = '" + companyId + @"'");

                    strPath = Path.Combine(ResourcesPathReader.GetLogoOrImagePath(), dtCompanyImage.Rows[0]["Image"].ToString());  // IDCardEng.xlsx
                    companyLogo = Image.FromFile(strPath);
                }
                catch (Exception)
                {
                }
                #endregion
                objRpt = new clsReport();

                objRptSR = new clsReport(_sqlRepository);

                DataTable dtGStReceivableF3 = null;
                string taxyearId = GetTaxYearId(fromDate, toDate, companyId);
                dtGStReceivableF3 = GetDebitNoteCreditNoteTaxSQL(companyGroupId, companyId, plantId, plantName, fromDate, toDate, taxyearId);
                if (dtGStReceivableF3.Rows.Count == 0)
                {
                    throw new Exception("No Data Found....");
                }

                DataTable dtCmp = objRptSR.SelectedCompanyDT(plantId);

                DataTable dtFactory = objRptSR.SelectedPlantDT(plantId);

                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;

                int xlsRow = 1, xlsCol = 1;
                int endXlsCol = 1;
                string FactoryName = "";
                string CmpName = "";
                xlsRow = 6;
                sheet1.Range[xlsRow - 1, 1].Text = "Report Ref No:";
                sheet1.Range[xlsRow - 1, 1].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow - 1, 1].RowHeight = 20;
                sheet1.Range[xlsRow - 1, 1].CellStyle.Font.Bold = true;

                int iSourceType = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Source Type";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                xlsCol++;

                int iVoucherNo = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Voucher No";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                xlsCol++;

                int iPostingDate = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Posting Date";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                xlsCol++;

                int iDocRefNo = xlsCol; // Doc Ref
                sheet1.Range[xlsRow, xlsCol].Text = "DocRef No";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                xlsCol++;

                int iDocDate = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Doc Date";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                xlsCol++;

                int iPartyName = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Party Name";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 25;
                xlsCol++;

                int iGSTIN = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "GSTIN(Party Plant)";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 25;
                xlsCol++;

                int iGRNNo = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "GRN No";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 25;
                xlsCol++;

                int iPartyPlantName = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Party Plant";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 25;
                xlsCol++;

                int iTaxableAmount = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Taxable Amount";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignRight;
                xlsCol++;

                int iTotalAmount = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Total Amount";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignRight;
                xlsCol++;

                int iIGSTAmount = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "IGST Amount";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignRight;
                xlsCol++;

                int iCGSTAmount = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "CGST Amount";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignRight;
                xlsCol++;

                int iSGSTAmount = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "SGST Amount";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignRight;
                xlsCol++;

                int iVoucherDate = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Voucher Date";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                xlsCol++;

                int iIsRCM = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "IsRCM";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 18;
                sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignRight;
                xlsCol++;

                int iIsTaxApplicable = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Tax Applicable";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 18;
                sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignRight;
                xlsCol++;

                int iType = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Type";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignRight;
                xlsCol++;

                int iPercentage = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Percentage";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignRight;
                xlsCol++;

                int iEntryDate = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Entry Date";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;

                xlsCol++;
                int iPartyType = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Party Type";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;

                xlsCol++;
                int iParkStatus = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Park Status";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                xlsCol++;

                int iWrittenOffAmount = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Written Off Amount";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;

                endXlsCol = xlsCol;
                endXlsCol = xlsCol;

                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].WrapText = true;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 23;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;

                string voucherNo = "";
                int startRow = 0;
                int perStartRow = 0;
                string formula = "";
                string formula2 = "";
                string totalFormula = "";

                string lineItemPercentageType = "";
                xlsRow++;
                startRow = xlsRow;
                perStartRow = xlsRow;
                bool isFirst = true;
                string totalTaxformula = "";
                string voucherNocomp = "";
                string taxFitler = "";
                for (int i = 0; i < dtGStReceivableF3.Rows.Count; i++)
                {
                    voucherNocomp = dtGStReceivableF3.Rows[i]["VoucherNo"].ToString().ToUpper();
                    taxFitler = " and VoucherNo = '" + dtGStReceivableF3.Rows[i]["VoucherNo"].ToString() + "'";
                    if (voucherNo != voucherNocomp)
                    {
                        if (isFirst == false)
                        {

                            sheet1[perStartRow, iSourceType, xlsRow - 1, iSourceType].BorderAround(ExcelLineStyle.Hair);
                            sheet1[perStartRow, iVoucherNo, xlsRow - 1, iVoucherNo].BorderAround(ExcelLineStyle.Hair);
                            sheet1[perStartRow, iPostingDate, xlsRow - 1, iPostingDate].BorderAround(ExcelLineStyle.Hair);
                            sheet1[perStartRow, iDocRefNo, xlsRow - 1, iDocRefNo].BorderAround(ExcelLineStyle.Hair);
                            sheet1[perStartRow, iDocDate, xlsRow - 1, iDocDate].BorderAround(ExcelLineStyle.Hair);
                            sheet1[perStartRow, iPartyName, xlsRow - 1, iPartyName].BorderAround(ExcelLineStyle.Hair);
                            sheet1[perStartRow, iGSTIN, xlsRow - 1, iGSTIN].BorderAround(ExcelLineStyle.Hair);
                            sheet1[perStartRow, iGRNNo, xlsRow - 1, iGRNNo].BorderAround(ExcelLineStyle.Hair);
                            sheet1[perStartRow, iPartyPlantName, xlsRow - 1, iPartyPlantName].BorderAround(ExcelLineStyle.Hair);
                            sheet1[perStartRow, iTaxableAmount, xlsRow - 1, iTaxableAmount].BorderAround(ExcelLineStyle.Hair);
                            sheet1[perStartRow, iTotalAmount, xlsRow - 1, iTotalAmount].BorderAround(ExcelLineStyle.Hair);
                            sheet1[perStartRow, iIGSTAmount, xlsRow - 1, iIGSTAmount].BorderAround(ExcelLineStyle.Hair);
                            sheet1[perStartRow, iCGSTAmount, xlsRow - 1, iCGSTAmount].BorderAround(ExcelLineStyle.Hair);
                            sheet1[perStartRow, iSGSTAmount, xlsRow - 1, iSGSTAmount].BorderAround(ExcelLineStyle.Hair);
                            sheet1[perStartRow, iVoucherDate, xlsRow - 1, iVoucherDate].BorderAround(ExcelLineStyle.Hair);
                            sheet1[perStartRow, iIsRCM, xlsRow - 1, iIsRCM].BorderAround(ExcelLineStyle.Hair);
                            sheet1[perStartRow, iIsTaxApplicable, xlsRow - 1, iIsTaxApplicable].BorderAround(ExcelLineStyle.Hair);
                            sheet1[perStartRow, iType, xlsRow - 1, iType].BorderAround(ExcelLineStyle.Hair);
                            sheet1[perStartRow, iPercentage, xlsRow - 1, iPercentage].BorderAround(ExcelLineStyle.Hair);
                            sheet1[perStartRow, iEntryDate, xlsRow - 1, iEntryDate].BorderAround(ExcelLineStyle.Hair);
                            sheet1[perStartRow, iPartyType, xlsRow - 1, iPartyType].BorderAround(ExcelLineStyle.Hair);
                            sheet1[perStartRow, iParkStatus, xlsRow - 1, iParkStatus].BorderAround(ExcelLineStyle.Hair);
                            sheet1[perStartRow, iWrittenOffAmount, xlsRow - 1, iWrittenOffAmount].BorderAround(ExcelLineStyle.Hair);


                            formula = "SUM(" + clsStaticInfo.GetxlsCol(iTaxableAmount) + perStartRow + ":" + clsStaticInfo.GetxlsCol(iTaxableAmount) + (xlsRow - 1) + ")";
                            formula2 = "";

                            //if (dtTaxCode.Rows.Count > 0)
                            //{
                            //    totalTaxformula = "SUM(";
                            //    for (int j = 0; j < dtTaxCode.Rows.Count; j++)
                            //    {
                            //        sheet1[perStartRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"]), xlsRow - 1, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])].BorderAround(ExcelLineStyle.Hair);
                            //        formula2 = "SUM(" + clsStaticInfo.GetxlsCol(Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])) + perStartRow + ":" + clsStaticInfo.GetxlsCol(Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])) + (xlsRow - 1) + ")";
                            //        sheet1[xlsRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"]), xlsRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])].Formula = formula2;

                            //        dtTaxCode.Rows[j]["ColumnFormula"] += (clsStaticInfo.GetxlsCol(Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])) + xlsRow).ToString() + " + ";
                            //        totalTaxformula += clsStaticInfo.GetxlsCol(Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])) + xlsRow + ":";
                            //    }
                            //}
                        }
                        isFirst = false;


                        sheet1.Range[xlsRow, iSourceType].Text = dtGStReceivableF3.Rows[i]["SourceType"].ToString();
                        sheet1.Range[xlsRow, iVoucherNo].Text = dtGStReceivableF3.Rows[i]["VoucherNo"].ToString();
                        sheet1.Range[xlsRow, iPostingDate].Text = clsStaticInfo.GetDateTaxFormate(dtGStReceivableF3.Rows[i]["PostingDate"].ToString());
                        sheet1.Range[xlsRow, iDocRefNo].Text = dtGStReceivableF3.Rows[i]["DocRefNo"].ToString();
                        sheet1.Range[xlsRow, iDocDate].Text = dtGStReceivableF3.Rows[i]["DocDate"].ToString();
                        sheet1.Range[xlsRow, iPartyName].Text = dtGStReceivableF3.Rows[i]["PartyName"].ToString();
                        sheet1.Range[xlsRow, iGSTIN].Text = dtGStReceivableF3.Rows[i]["GSTIN"].ToString();
                        sheet1.Range[xlsRow, iGRNNo].Text = dtGStReceivableF3.Rows[i]["GRNNo"].ToString();
                        sheet1.Range[xlsRow, iPartyPlantName].Text = dtGStReceivableF3.Rows[i]["PartyPlantName"].ToString();
                        sheet1.Range[xlsRow, iTaxableAmount].Number = clsStaticInfo.dbl(dtGStReceivableF3.Rows[i]["TaxableAmount"].ToString());
                        sheet1.Range[xlsRow, iTaxableAmount].NumberFormat = "#,##0.00;(#,##0.00)";

                        sheet1.Range[xlsRow, iTotalAmount].Number = clsStaticInfo.dbl(dtGStReceivableF3.Rows[i]["TotalAmount"].ToString());
                        sheet1.Range[xlsRow, iTotalAmount].NumberFormat = "#,##0.00;(#,##0.00)";
                        sheet1.Range[xlsRow, iIGSTAmount].Number = clsStaticInfo.dbl(dtGStReceivableF3.Rows[i]["IGSTAmount"].ToString());
                        sheet1.Range[xlsRow, iIGSTAmount].NumberFormat = "#,##0.00;(#,##0.00)";
                        sheet1.Range[xlsRow, iCGSTAmount].Number = clsStaticInfo.dbl(dtGStReceivableF3.Rows[i]["CGSTAmount"].ToString());
                        sheet1.Range[xlsRow, iCGSTAmount].NumberFormat = "#,##0.00;(#,##0.00)";
                        sheet1.Range[xlsRow, iSGSTAmount].Number = clsStaticInfo.dbl(dtGStReceivableF3.Rows[i]["SGSTAmount"].ToString());
                        sheet1.Range[xlsRow, iSGSTAmount].NumberFormat = "#,##0.00;(#,##0.00)";

                        sheet1.Range[xlsRow, iVoucherDate].Text = dtGStReceivableF3.Rows[i]["VoucherDate"].ToString();
                        sheet1.Range[xlsRow, iIsRCM].Text = dtGStReceivableF3.Rows[i]["IsRCM"].ToString();
                        sheet1.Range[xlsRow, iIsTaxApplicable].Text = dtGStReceivableF3.Rows[i]["IsTaxApplicable"].ToString();
                        sheet1.Range[xlsRow, iType].Text = dtGStReceivableF3.Rows[i]["Type"].ToString();
                        sheet1.Range[xlsRow, iPercentage].Number = clsStaticInfo.dbl(dtGStReceivableF3.Rows[i]["Percentage"].ToString());
                        sheet1.Range[xlsRow, iPercentage].NumberFormat = "#,##0.00;(#,##0.00)";
                        sheet1.Range[xlsRow, iEntryDate].Text = dtGStReceivableF3.Rows[i]["EntryDate"].ToString();
                        sheet1.Range[xlsRow, iPartyType].Text = dtGStReceivableF3.Rows[i]["PartyType"].ToString();
                        sheet1.Range[xlsRow, iParkStatus].Text = dtGStReceivableF3.Rows[i]["ParkStatus"].ToString();

                        sheet1.Range[xlsRow, iWrittenOffAmount].Number = clsStaticInfo.dbl(dtGStReceivableF3.Rows[i]["WrittenOffAmount"].ToString());
                        sheet1.Range[xlsRow, iWrittenOffAmount].NumberFormat = "#,##0.00;(#,##0.00)";

                        xlsRow++;
                    }
                    //voucherNo = dtGStReceivableF3.Rows[i]["VoucherNo"].ToString().ToUpper();


                }
                sheet1[perStartRow, iSourceType, xlsRow - 1, iSourceType].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iVoucherNo, xlsRow - 1, iVoucherNo].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iPostingDate, xlsRow - 1, iPostingDate].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iDocRefNo, xlsRow - 1, iDocRefNo].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iDocDate, xlsRow - 1, iDocDate].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iPartyName, xlsRow - 1, iPartyName].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iGSTIN, xlsRow - 1, iGSTIN].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iGRNNo, xlsRow - 1, iGRNNo].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iPartyPlantName, xlsRow - 1, iPartyPlantName].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iTaxableAmount, xlsRow - 1, iTaxableAmount].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iTotalAmount, xlsRow - 1, iTotalAmount].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iIGSTAmount, xlsRow - 1, iIGSTAmount].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iCGSTAmount, xlsRow - 1, iCGSTAmount].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iSGSTAmount, xlsRow - 1, iSGSTAmount].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iVoucherDate, xlsRow - 1, iVoucherDate].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iIsRCM, xlsRow - 1, iIsRCM].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iIsTaxApplicable, xlsRow - 1, iIsTaxApplicable].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iType, xlsRow - 1, iType].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iPercentage, xlsRow - 1, iPercentage].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iEntryDate, xlsRow - 1, iEntryDate].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iPartyType, xlsRow - 1, iPartyType].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iParkStatus, xlsRow - 1, iParkStatus].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iWrittenOffAmount, xlsRow - 1, iWrittenOffAmount].BorderAround(ExcelLineStyle.Hair);


                //if (dtTaxCode.Rows.Count > 0)
                //{
                //    for (int j = 0; j < dtTaxCode.Rows.Count; j++)
                //    {
                //        sheet1[perStartRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"]), xlsRow - 1, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])].BorderAround(ExcelLineStyle.Hair);

                //    }
                //}

                //if (dtTaxCode.Rows.Count > 0)
                //{
                //    xlsRow++;
                //    totalTaxformula = "=SUM(";
                //    for (int j = 0; j < dtTaxCode.Rows.Count; j++)
                //    {
                //        sheet1[perStartRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"]), xlsRow - 1, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])].BorderAround(ExcelLineStyle.Hair);
                //        formula2 = "SUM(" + clsStaticInfo.GetxlsCol(Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])) + perStartRow + ":" + clsStaticInfo.GetxlsCol(Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])) + (xlsRow - 1) + ")";
                //        sheet1[xlsRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"]), xlsRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])].Formula = formula2;

                //        dtTaxCode.Rows[j]["ColumnFormula"] += (clsStaticInfo.GetxlsCol(Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])) + xlsRow).ToString() + " + ";
                //        totalTaxformula += clsStaticInfo.GetxlsCol(Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])) + xlsRow + ":";

                //    }
                //}


                //sheet1.Range[xlsRow, 1, xlsRow, 1].Text = "Grand Total";
                //formula = "SUM(" + clsStaticInfo.GetxlsCol(iTaxableAmount) + perStartRow + ":" + clsStaticInfo.GetxlsCol(iTaxableAmount) + (xlsRow - 1) + ")";



                //sheet1[xlsRow, iTaxableAmount, xlsRow, iTaxableAmount].Formula = formula;
                //sheet1[xlsRow, iTaxableAmount, xlsRow, iTaxableAmount].NumberFormat = "#,##0.00;(#,##0.00)";
                //sheet1[xlsRow, iGrossAmount, xlsRow, iGrossAmount].Formula = clsStaticInfo.GetxlsCol(iTaxableAmount) + xlsRow + "+" + clsStaticInfo.GetxlsCol(iTotalTax) + xlsRow;
                //sheet1[xlsRow, iGrossAmount, xlsRow, iGrossAmount].NumberFormat = "#,##0.00;(#,##0.00)";

                //sheet1[xlsRow, iTotalTax, xlsRow, iTotalTax].Formula = totalTaxformula.Remove(totalTaxformula.Length - 1) + ")";
                //sheet1[xlsRow, iTotalTax, xlsRow, iTotalTax].NumberFormat = "#,##0.00;(#,##0.00)";

                //sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Font.Bold = true;
                //totalFormula += (clsStaticInfo.GetxlsCol(iTaxableAmount) + xlsRow).ToString() + "+";


                #region ******************Report Header******************


                xlsRow = 1;
                xlsCol = 3;
                try
                {
                    if (companyLogo != null)
                    {

                        double totalWidth = sheet1.GetColumnWidth(1) + sheet1.GetColumnWidth(3);
                        int totalWidthPixel = (int)(totalWidth * 7.5);
                        int totalheight = (int)((sheet1.GetRowHeight(1) + sheet1.GetRowHeight(2) + sheet1.GetRowHeight(3) + sheet1.GetRowHeight(3)) * 1.50);

                        companyLogo = ReportUtility.FixedSize(companyLogo, totalWidthPixel, totalheight);
                        IPictureShape pic = null;

                        pic = sheet1.Pictures.AddPicture(1, 1, companyLogo);
                        //pic.Height = 80;
                        //pic.Width = 220;
                    }
                }
                catch (Exception ex)
                {
                }

                FactoryName = string.Empty;

                string FactoryAddress = string.Empty;

                if (dtCmp.Rows.Count > 0)
                {
                    CmpName = dtCmp.Rows[0]["CompanyName"].ToString();
                }
                else
                {
                    CmpName = "";
                }
                sheet1.Range[xlsRow, 3].Text = CmpName;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 12;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 17;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                if (dtFactory.Rows.Count > 0)
                {
                    //FactoryName = dsFactory.Tables[0].Rows[0]["PlantName"].ToString();
                    FactoryName = dtFactory.Rows[0]["UserName"].ToString();
                }
                else
                {
                    FactoryName = "";
                }
                sheet1.Range[xlsRow, 3].Text = FactoryName;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 14;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 18;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                if (dtFactory.Rows.Count > 0)
                {
                    FactoryAddress = dtFactory.Rows[0]["Address1"].ToString();
                }
                else
                {
                    FactoryAddress = "";
                }
                sheet1.Range[xlsRow, 3].Text = FactoryAddress;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                //sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 22;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                sheet1.Range[xlsRow, 3].Text = "Debit Note Credit Note Status Report From " + fromDate + " To " + toDate;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;


                #endregion ******************Report Header******************

                #region Freeze Panes

                sheet1.IsDisplayZeros = false;
                sheet1.UsedRange["A7"].FreezePanes();
                sheet1.FirstVisibleColumn = 1;
                sheet1.FirstVisibleRow = 6;

                #endregion Freeze Panes

                #region UsedRange Alignment

                sheet1.UsedRange.WrapText = false;
                sheet1.UsedRange.CellStyle.Font.Size = 10;
                sheet1.Range["A1"].CellStyle.Font.Size = 14;
                sheet1.Range["A2"].CellStyle.Font.Size = 10;
                sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;
                sheet1.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                #endregion UsedRange Alignment

                #region Page Setup
                sheet1.PageSetup.TopMargin = 0.5;
                sheet1.PageSetup.BottomMargin = 0.7;
                sheet1.PageSetup.PrintTitleRows = "$1:$5";
                sheet1.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                sheet1.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + name + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:MM tt").ToString();
                sheet1.PageSetup.LeftMargin = 0.5;
                sheet1.PageSetup.RightMargin = 0.2;
                sheet1.PageSetup.Orientation = ExcelPageOrientation.Portrait;
                sheet1.PageSetup.FitToPagesTall = 0;
                sheet1.PageSetup.FitToPagesWide = 1;
                sheet1.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                sheet1.IsDisplayZeros = false;
                #endregion Page SetupLineItemType
                sheet1.Range[6, 1, 6, endXlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;


                sheet1.Name = "Debit Note Credit Note Status Report";
                return workbook;
            }
            catch (System.Exception ex)
            {

                throw ex;
            }
        }
        public IWorkbook GetAdvancePaymentPendingforSetOffReport(string companyGroupId, string companyId, string plantId, string plantName, string fromDate, string toDate, string name)
        {
            clsReport objRpt = null;
            clsReport objRptSR = null;
            try
            {

                ExcelEngine excelEngine = null;
                IApplication application = null;
                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;
                excelEngine.Excel.DefaultVersion = ExcelVersion.Excel2013;
                var reportUtility = new ReportUtility();
                var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
                workbook.Version = ExcelVersion.Excel2013;
                var sheet1 = workbook.Worksheets[0];

                #region Logo
                string strPath = "";
                Image companyLogo = null;
                try
                {
                    DataTable dtCompanyImage = _sqlRepository.GetDataTable("SELECT * FROM ORG.COMPANY WHERE ID = '" + companyId + @"'");

                    strPath = Path.Combine(ResourcesPathReader.GetLogoOrImagePath(), dtCompanyImage.Rows[0]["Image"].ToString());  // IDCardEng.xlsx
                    companyLogo = Image.FromFile(strPath);
                }
                catch (Exception)
                {
                }
                #endregion
                objRpt = new clsReport();

                objRptSR = new clsReport(_sqlRepository);

                DataTable dtGStReceivableF3 = null;
                dtGStReceivableF3 = GetAdvancePaymentPendingforSetOffReportSQL(companyGroupId, companyId, plantId);
                if (dtGStReceivableF3.Rows.Count == 0)
                {
                    throw new Exception("No Data Found....");
                }

                DataTable dtCmp = objRptSR.SelectedCompanyDT(plantId);

                DataTable dtFactory = objRptSR.SelectedPlantDT(plantId);

                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;

                int xlsRow = 1, xlsCol = 1;
                int endXlsCol = 1;
                string FactoryName = "";
                string CmpName = "";
                xlsRow = 5;

                int iType = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Party Type";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                xlsCol++;

                int iVoucherRowId = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Voucher Row Id";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                xlsCol++;

                int iVoucherNo = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Voucher No";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                xlsCol++;

                int iPartyName = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Party Name";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 25;
                xlsCol++;

                int iPostingDate = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Posting Date";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                xlsCol++;

                int iDocDate = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Doc Date";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                xlsCol++;

                int iReviewDate = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Review Date";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                xlsCol++;

                int iDocRefNo = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "DocRef No";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 20;
                xlsCol++;

                int iNarration = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Narration";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 25;
                xlsCol++;

                int iGL = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "GL";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 25;
                xlsCol++;

                int iBudget = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Budget";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 25;
                xlsCol++;

                int iActivity = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Activity";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 25;
                xlsCol++;

                int iResponsiblePerson = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Responsible Person";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 18;
                xlsCol++;

                int iPaymentSource = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Payment Source";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 18;
                xlsCol++;

                int iBankName = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Bank Name";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                xlsCol++;

                int iCashName = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Cash Name";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                xlsCol++;

                int iCurrencyCode = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Currency";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                xlsCol++;

                int iReceivable = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Receivable";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignRight;
                xlsCol++;

                int iReceived = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Received";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignRight;
                xlsCol++;

                int iBalance = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Balance";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignRight;
                xlsCol++;

                int iBookReceivable = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Book Receivable";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignRight;
                xlsCol++;

                int iBookReceived = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Book Received";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignRight;
                xlsCol++;

                int iBookBalance = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Book Balance";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignRight;

                endXlsCol = xlsCol;
                endXlsCol = xlsCol;

                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].WrapText = true;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 23;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;


                int startRow = 0;
                int perStartRow = 0;

                xlsRow++;
                startRow = xlsRow;
                perStartRow = xlsRow;

                for (int i = 0; i < dtGStReceivableF3.Rows.Count; i++)
                {
                    sheet1.Range[xlsRow, iType].Text = dtGStReceivableF3.Rows[i]["PartyType"].ToString();
                    sheet1.Range[xlsRow, iVoucherRowId].Text = dtGStReceivableF3.Rows[i]["VoucherRowId"].ToString();
                    sheet1.Range[xlsRow, iVoucherNo].Text = dtGStReceivableF3.Rows[i]["VoucherNo"].ToString();
                    sheet1.Range[xlsRow, iPartyName].Text = dtGStReceivableF3.Rows[i]["PartyName"].ToString();
                    sheet1.Range[xlsRow, iPostingDate].Text = dtGStReceivableF3.Rows[i]["PostingDate"].ToString();
                    sheet1.Range[xlsRow, iDocDate].Text = dtGStReceivableF3.Rows[i]["DocDate"].ToString();
                    sheet1.Range[xlsRow, iReviewDate].Text = dtGStReceivableF3.Rows[i]["ReviewDate"].ToString();
                    sheet1.Range[xlsRow, iDocRefNo].Text = dtGStReceivableF3.Rows[i]["DocRefNo"].ToString();


                    sheet1.Range[xlsRow, iNarration].Text = dtGStReceivableF3.Rows[i]["Narration"].ToString();
                    sheet1.Range[xlsRow, iGL].Text = dtGStReceivableF3.Rows[i]["GL"].ToString();
                    sheet1.Range[xlsRow, iBudget].Text = dtGStReceivableF3.Rows[i]["Budget"].ToString();
                    sheet1.Range[xlsRow, iActivity].Text = dtGStReceivableF3.Rows[i]["Activity"].ToString();
                    sheet1.Range[xlsRow, iResponsiblePerson].Text = dtGStReceivableF3.Rows[i]["ResponsiblePerson"].ToString();
                    sheet1.Range[xlsRow, iPaymentSource].Text = dtGStReceivableF3.Rows[i]["PaymentSource"].ToString();
                    sheet1.Range[xlsRow, iBankName].Text = dtGStReceivableF3.Rows[i]["BankName"].ToString();
                    sheet1.Range[xlsRow, iCashName].Text = dtGStReceivableF3.Rows[i]["CashName"].ToString();
                    sheet1.Range[xlsRow, iCurrencyCode].Text = dtGStReceivableF3.Rows[i]["CurrencyCode"].ToString();


                    sheet1.Range[xlsRow, iReceivable].Number = clsStaticInfo.dbl(dtGStReceivableF3.Rows[i]["Receivable"].ToString());
                    sheet1.Range[xlsRow, iReceivable].NumberFormat = "#,##0.00;(#,##0.00)";
                    sheet1.Range[xlsRow, iReceived].Number = clsStaticInfo.dbl(dtGStReceivableF3.Rows[i]["Received"].ToString());
                    sheet1.Range[xlsRow, iReceived].NumberFormat = "#,##0.00;(#,##0.00)";
                    sheet1.Range[xlsRow, iBalance].Number = clsStaticInfo.dbl(dtGStReceivableF3.Rows[i]["Balance"].ToString());
                    sheet1.Range[xlsRow, iBalance].NumberFormat = "#,##0.00;(#,##0.00)";
                    sheet1.Range[xlsRow, iBookReceivable].Number = clsStaticInfo.dbl(dtGStReceivableF3.Rows[i]["BookReceivable"].ToString());
                    sheet1.Range[xlsRow, iBookReceivable].NumberFormat = "#,##0.00;(#,##0.00)";
                    sheet1.Range[xlsRow, iBookReceived].Number = clsStaticInfo.dbl(dtGStReceivableF3.Rows[i]["BookReceived"].ToString());
                    sheet1.Range[xlsRow, iBookReceived].NumberFormat = "#,##0.00;(#,##0.00)";
                    sheet1.Range[xlsRow, iBookBalance].Number = clsStaticInfo.dbl(dtGStReceivableF3.Rows[i]["BookBalance"].ToString());
                    sheet1.Range[xlsRow, iBookBalance].NumberFormat = "#,##0.00;(#,##0.00)";

                    xlsRow++;

                }
                sheet1[perStartRow, iType, xlsRow - 1, iType].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iVoucherRowId, xlsRow - 1, iVoucherRowId].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iVoucherNo, xlsRow - 1, iVoucherNo].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iPartyName, xlsRow - 1, iPartyName].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iPostingDate, xlsRow - 1, iPostingDate].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iDocDate, xlsRow - 1, iDocDate].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iReviewDate, xlsRow - 1, iReviewDate].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iDocRefNo, xlsRow - 1, iDocRefNo].BorderAround(ExcelLineStyle.Hair);

                sheet1[perStartRow, iNarration, xlsRow - 1, iNarration].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iGL, xlsRow - 1, iGL].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iBudget, xlsRow - 1, iBudget].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iActivity, xlsRow - 1, iActivity].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iResponsiblePerson, xlsRow - 1, iResponsiblePerson].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iPaymentSource, xlsRow - 1, iPaymentSource].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iBankName, xlsRow - 1, iBankName].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iCashName, xlsRow - 1, iCashName].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iCurrencyCode, xlsRow - 1, iCurrencyCode].BorderAround(ExcelLineStyle.Hair);

                sheet1[perStartRow, iReceivable, xlsRow - 1, iReceivable].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iReceived, xlsRow - 1, iReceived].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iBalance, xlsRow - 1, iBalance].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iBookReceivable, xlsRow - 1, iBookReceivable].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iBookReceived, xlsRow - 1, iBookReceived].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iBookBalance, xlsRow - 1, iBookBalance].BorderAround(ExcelLineStyle.Hair);

                #region ******************Report Header******************


                xlsRow = 1;
                xlsCol = 3;
                try
                {
                    if (companyLogo != null)
                    {

                        double totalWidth = sheet1.GetColumnWidth(1) + sheet1.GetColumnWidth(3);
                        int totalWidthPixel = (int)(totalWidth * 7.5);
                        int totalheight = (int)((sheet1.GetRowHeight(1) + sheet1.GetRowHeight(2) + sheet1.GetRowHeight(3) + sheet1.GetRowHeight(3)) * 1.50);

                        companyLogo = ReportUtility.FixedSize(companyLogo, totalWidthPixel, totalheight);
                        IPictureShape pic = null;

                        pic = sheet1.Pictures.AddPicture(1, 1, companyLogo);

                    }
                }
                catch (Exception ex)
                {
                }

                FactoryName = string.Empty;

                string FactoryAddress = string.Empty;

                if (dtCmp.Rows.Count > 0)
                {
                    CmpName = dtCmp.Rows[0]["CompanyName"].ToString();
                }
                else
                {
                    CmpName = "";
                }
                sheet1.Range[xlsRow, 3].Text = CmpName;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 12;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 17;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                if (dtFactory.Rows.Count > 0)
                {
                    FactoryName = dtFactory.Rows[0]["UserName"].ToString();
                }
                else
                {
                    FactoryName = "";
                }
                sheet1.Range[xlsRow, 3].Text = FactoryName;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 14;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 18;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                if (dtFactory.Rows.Count > 0)
                {
                    FactoryAddress = dtFactory.Rows[0]["Address1"].ToString();
                }
                else
                {
                    FactoryAddress = "";
                }
                sheet1.Range[xlsRow, 3].Text = FactoryAddress;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                //sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 22;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                sheet1.Range[xlsRow, 3].Text = "Advance Payment Pending for Set Off Report ";
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;


                #endregion ******************Report Header******************

                #region Freeze Panes

                sheet1.IsDisplayZeros = false;
                sheet1.UsedRange["A7"].FreezePanes();
                sheet1.FirstVisibleColumn = 1;
                sheet1.FirstVisibleRow = 6;

                #endregion Freeze Panes

                #region UsedRange Alignment

                sheet1.UsedRange.WrapText = false;
                sheet1.UsedRange.CellStyle.Font.Size = 10;
                sheet1.Range["A1"].CellStyle.Font.Size = 14;
                sheet1.Range["A2"].CellStyle.Font.Size = 10;
                sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;
                sheet1.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                #endregion UsedRange Alignment

                #region Page Setup
                sheet1.PageSetup.TopMargin = 0.5;
                sheet1.PageSetup.BottomMargin = 0.7;
                sheet1.PageSetup.PrintTitleRows = "$1:$5";
                sheet1.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                sheet1.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + name + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:MM tt").ToString();
                sheet1.PageSetup.LeftMargin = 0.5;
                sheet1.PageSetup.RightMargin = 0.2;
                sheet1.PageSetup.Orientation = ExcelPageOrientation.Portrait;
                sheet1.PageSetup.FitToPagesTall = 0;
                sheet1.PageSetup.FitToPagesWide = 1;
                sheet1.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                sheet1.IsDisplayZeros = false;
                #endregion Page SetupLineItemType
                sheet1.Range[6, 1, 6, endXlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;


                sheet1.Name = "Advance Payment Pending for Set Off Report";
                return workbook;
            }
            catch (System.Exception ex)
            {

                throw ex;
            }
        }
        public IWorkbook GetDebitNotePaymentPendingforSetOffReport(string companyGroupId, string companyId, string plantId, string plantName, string fromDate, string toDate, string name)
        {
            clsReport objRpt = null;
            clsReport objRptSR = null;
            try
            {

                ExcelEngine excelEngine = null;
                IApplication application = null;
                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;
                excelEngine.Excel.DefaultVersion = ExcelVersion.Excel2013;
                var reportUtility = new ReportUtility();
                var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
                workbook.Version = ExcelVersion.Excel2013;
                var sheet1 = workbook.Worksheets[0];

                #region Logo
                string strPath = "";
                Image companyLogo = null;
                try
                {
                    DataTable dtCompanyImage = _sqlRepository.GetDataTable("SELECT * FROM ORG.COMPANY WHERE ID = '" + companyId + @"'");

                    strPath = Path.Combine(ResourcesPathReader.GetLogoOrImagePath(), dtCompanyImage.Rows[0]["Image"].ToString());  // IDCardEng.xlsx
                    companyLogo = Image.FromFile(strPath);
                }
                catch (Exception)
                {
                }
                #endregion
                objRpt = new clsReport();

                objRptSR = new clsReport(_sqlRepository);

                DataTable dtGStReceivableF3 = null;
                dtGStReceivableF3 = GetDebitNotePaymentPendingforSetOffReportSQL(companyGroupId, companyId, plantId);
                if (dtGStReceivableF3.Rows.Count == 0)
                {
                    throw new Exception("No Data Found....");
                }

                DataTable dtCmp = objRptSR.SelectedCompanyDT(plantId);

                DataTable dtFactory = objRptSR.SelectedPlantDT(plantId);

                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;

                int xlsRow = 1, xlsCol = 1;
                int endXlsCol = 1;
                string FactoryName = "";
                string CmpName = "";
                xlsRow = 5;

                int iType = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Party Type";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                xlsCol++;

                int iVoucherRowId = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Voucher Row Id";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                xlsCol++;

                int iVoucherNo = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Voucher No";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                xlsCol++;

                int iPartyName = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Party Name";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 25;
                xlsCol++;

                int iPostingDate = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Posting Date";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                xlsCol++;

                int iDocDate = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Doc Date";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                xlsCol++;

                int iDocRefNo = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "DocRef No";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 20;
                xlsCol++;

                int iNarration = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Narration";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 25;
                xlsCol++;

                int iGL = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "GL";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 25;
                xlsCol++;

                int iBudget = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Budget";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 25;
                xlsCol++;

                int iActivity = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Activity";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 25;
                xlsCol++;

                int iCurrencyCode = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Currency";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                xlsCol++;

                int iReceivable = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Receivable";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignRight;
                xlsCol++;

                int iReceived = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Received";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignRight;
                xlsCol++;

                int iBalance = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Balance";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignRight;
                xlsCol++;

                int iBookReceivable = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Book Receivable";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignRight;
                xlsCol++;

                int iBookReceived = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Book Received";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignRight;
                xlsCol++;

                int iBookBalance = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Book Balance";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignRight;

                endXlsCol = xlsCol;
                endXlsCol = xlsCol;

                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].WrapText = true;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 23;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;


                int startRow = 0;
                int perStartRow = 0;

                xlsRow++;
                startRow = xlsRow;
                perStartRow = xlsRow;

                for (int i = 0; i < dtGStReceivableF3.Rows.Count; i++)
                {
                    sheet1.Range[xlsRow, iType].Text = dtGStReceivableF3.Rows[i]["PartyType"].ToString();
                    sheet1.Range[xlsRow, iVoucherRowId].Text = dtGStReceivableF3.Rows[i]["VoucherRowId"].ToString();
                    sheet1.Range[xlsRow, iVoucherNo].Text = dtGStReceivableF3.Rows[i]["VoucherNo"].ToString();
                    sheet1.Range[xlsRow, iPartyName].Text = dtGStReceivableF3.Rows[i]["PartyName"].ToString();
                    sheet1.Range[xlsRow, iPostingDate].Text = dtGStReceivableF3.Rows[i]["PostingDate"].ToString();
                    sheet1.Range[xlsRow, iDocDate].Text = dtGStReceivableF3.Rows[i]["DocDate"].ToString();
                    sheet1.Range[xlsRow, iDocRefNo].Text = dtGStReceivableF3.Rows[i]["DocRefNo"].ToString();


                    sheet1.Range[xlsRow, iNarration].Text = dtGStReceivableF3.Rows[i]["Narration"].ToString();
                    sheet1.Range[xlsRow, iGL].Text = dtGStReceivableF3.Rows[i]["GL"].ToString();
                    sheet1.Range[xlsRow, iBudget].Text = dtGStReceivableF3.Rows[i]["Budget"].ToString();
                    sheet1.Range[xlsRow, iActivity].Text = dtGStReceivableF3.Rows[i]["Activity"].ToString();
                    sheet1.Range[xlsRow, iCurrencyCode].Text = dtGStReceivableF3.Rows[i]["CurrencyCode"].ToString();


                    sheet1.Range[xlsRow, iReceivable].Number = clsStaticInfo.dbl(dtGStReceivableF3.Rows[i]["Receivable"].ToString());
                    sheet1.Range[xlsRow, iReceivable].NumberFormat = "#,##0.00;(#,##0.00)";
                    sheet1.Range[xlsRow, iReceived].Number = clsStaticInfo.dbl(dtGStReceivableF3.Rows[i]["Received"].ToString());
                    sheet1.Range[xlsRow, iReceived].NumberFormat = "#,##0.00;(#,##0.00)";
                    sheet1.Range[xlsRow, iBalance].Number = clsStaticInfo.dbl(dtGStReceivableF3.Rows[i]["Balance"].ToString());
                    sheet1.Range[xlsRow, iBalance].NumberFormat = "#,##0.00;(#,##0.00)";
                    sheet1.Range[xlsRow, iBookReceivable].Number = clsStaticInfo.dbl(dtGStReceivableF3.Rows[i]["BookReceivable"].ToString());
                    sheet1.Range[xlsRow, iBookReceivable].NumberFormat = "#,##0.00;(#,##0.00)";
                    sheet1.Range[xlsRow, iBookReceived].Number = clsStaticInfo.dbl(dtGStReceivableF3.Rows[i]["BookReceived"].ToString());
                    sheet1.Range[xlsRow, iBookReceived].NumberFormat = "#,##0.00;(#,##0.00)";
                    sheet1.Range[xlsRow, iBookBalance].Number = clsStaticInfo.dbl(dtGStReceivableF3.Rows[i]["BookBalance"].ToString());
                    sheet1.Range[xlsRow, iBookBalance].NumberFormat = "#,##0.00;(#,##0.00)";

                    xlsRow++;

                }
                sheet1[perStartRow, iType, xlsRow - 1, iType].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iVoucherRowId, xlsRow - 1, iVoucherRowId].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iVoucherNo, xlsRow - 1, iVoucherNo].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iPartyName, xlsRow - 1, iPartyName].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iPostingDate, xlsRow - 1, iPostingDate].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iDocDate, xlsRow - 1, iDocDate].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iDocRefNo, xlsRow - 1, iDocRefNo].BorderAround(ExcelLineStyle.Hair);

                sheet1[perStartRow, iNarration, xlsRow - 1, iNarration].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iGL, xlsRow - 1, iGL].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iBudget, xlsRow - 1, iBudget].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iActivity, xlsRow - 1, iActivity].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iCurrencyCode, xlsRow - 1, iCurrencyCode].BorderAround(ExcelLineStyle.Hair);

                sheet1[perStartRow, iReceivable, xlsRow - 1, iReceivable].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iReceived, xlsRow - 1, iReceived].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iBalance, xlsRow - 1, iBalance].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iBookReceivable, xlsRow - 1, iBookReceivable].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iBookReceived, xlsRow - 1, iBookReceived].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iBookBalance, xlsRow - 1, iBookBalance].BorderAround(ExcelLineStyle.Hair);

                #region ******************Report Header******************


                xlsRow = 1;
                xlsCol = 3;
                try
                {
                    if (companyLogo != null)
                    {

                        double totalWidth = sheet1.GetColumnWidth(1) + sheet1.GetColumnWidth(3);
                        int totalWidthPixel = (int)(totalWidth * 7.5);
                        int totalheight = (int)((sheet1.GetRowHeight(1) + sheet1.GetRowHeight(2) + sheet1.GetRowHeight(3) + sheet1.GetRowHeight(3)) * 1.50);

                        companyLogo = ReportUtility.FixedSize(companyLogo, totalWidthPixel, totalheight);
                        IPictureShape pic = null;

                        pic = sheet1.Pictures.AddPicture(1, 1, companyLogo);

                    }
                }
                catch (Exception ex)
                {
                }

                FactoryName = string.Empty;

                string FactoryAddress = string.Empty;

                if (dtCmp.Rows.Count > 0)
                {
                    CmpName = dtCmp.Rows[0]["CompanyName"].ToString();
                }
                else
                {
                    CmpName = "";
                }
                sheet1.Range[xlsRow, 3].Text = CmpName;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 12;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 17;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                if (dtFactory.Rows.Count > 0)
                {
                    FactoryName = dtFactory.Rows[0]["UserName"].ToString();
                }
                else
                {
                    FactoryName = "";
                }
                sheet1.Range[xlsRow, 3].Text = FactoryName;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 14;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 18;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                if (dtFactory.Rows.Count > 0)
                {
                    FactoryAddress = dtFactory.Rows[0]["Address1"].ToString();
                }
                else
                {
                    FactoryAddress = "";
                }
                sheet1.Range[xlsRow, 3].Text = FactoryAddress;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                //sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 22;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                sheet1.Range[xlsRow, 3].Text = "Debit Note Payment Pending for Set Off Report ";
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;


                #endregion ******************Report Header******************

                #region Freeze Panes

                sheet1.IsDisplayZeros = false;
                sheet1.UsedRange["A7"].FreezePanes();
                sheet1.FirstVisibleColumn = 1;
                sheet1.FirstVisibleRow = 6;

                #endregion Freeze Panes

                #region UsedRange Alignment

                sheet1.UsedRange.WrapText = false;
                sheet1.UsedRange.CellStyle.Font.Size = 10;
                sheet1.Range["A1"].CellStyle.Font.Size = 14;
                sheet1.Range["A2"].CellStyle.Font.Size = 10;
                sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;
                sheet1.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                #endregion UsedRange Alignment

                #region Page Setup
                sheet1.PageSetup.TopMargin = 0.5;
                sheet1.PageSetup.BottomMargin = 0.7;
                sheet1.PageSetup.PrintTitleRows = "$1:$5";
                sheet1.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                sheet1.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + name + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:MM tt").ToString();
                sheet1.PageSetup.LeftMargin = 0.5;
                sheet1.PageSetup.RightMargin = 0.2;
                sheet1.PageSetup.Orientation = ExcelPageOrientation.Portrait;
                sheet1.PageSetup.FitToPagesTall = 0;
                sheet1.PageSetup.FitToPagesWide = 1;
                sheet1.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                sheet1.IsDisplayZeros = false;
                #endregion Page SetupLineItemType
                sheet1.Range[6, 1, 6, endXlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;


                sheet1.Name = "Debit Note Payment Pending for Set Off Report";
                return workbook;
            }
            catch (System.Exception ex)
            {

                throw ex;
            }
        }
        public IWorkbook GetCreditNotePaymentPendingforSetOffReport(string companyGroupId, string companyId, string plantId, string plantName, string fromDate, string toDate, string name)
        {
            clsReport objRpt = null;
            clsReport objRptSR = null;
            try
            {

                ExcelEngine excelEngine = null;
                IApplication application = null;
                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;
                excelEngine.Excel.DefaultVersion = ExcelVersion.Excel2013;
                var reportUtility = new ReportUtility();
                var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
                workbook.Version = ExcelVersion.Excel2013;
                var sheet1 = workbook.Worksheets[0];

                #region Logo
                string strPath = "";
                Image companyLogo = null;
                try
                {
                    DataTable dtCompanyImage = _sqlRepository.GetDataTable("SELECT * FROM ORG.COMPANY WHERE ID = '" + companyId + @"'");

                    strPath = Path.Combine(ResourcesPathReader.GetLogoOrImagePath(), dtCompanyImage.Rows[0]["Image"].ToString());  // IDCardEng.xlsx
                    companyLogo = Image.FromFile(strPath);
                }
                catch (Exception)
                {
                }
                #endregion
                objRpt = new clsReport();

                objRptSR = new clsReport(_sqlRepository);

                DataTable dtGStReceivableF3 = null;
                dtGStReceivableF3 = GetCreditNotePaymentPendingforSetOffReportSQL(companyGroupId, companyId, plantId);
                if (dtGStReceivableF3.Rows.Count == 0)
                {
                    throw new Exception("No Data Found....");
                }

                DataTable dtCmp = objRptSR.SelectedCompanyDT(plantId);

                DataTable dtFactory = objRptSR.SelectedPlantDT(plantId);

                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;

                int xlsRow = 1, xlsCol = 1;
                int endXlsCol = 1;
                string FactoryName = "";
                string CmpName = "";
                xlsRow = 5;

                int iType = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Party Type";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                xlsCol++;

                int iVoucherRowId = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Voucher Row Id";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                xlsCol++;

                int iVoucherNo = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Voucher No";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                xlsCol++;

                int iPartyName = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Party Name";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 25;
                xlsCol++;

                int iPostingDate = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Posting Date";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                xlsCol++;

                int iDocDate = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Doc Date";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                xlsCol++;

                int iDocRefNo = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "DocRef No";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 20;
                xlsCol++;

                int iNarration = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Narration";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 25;
                xlsCol++;

                int iGL = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "GL";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 25;
                xlsCol++;

                int iBudget = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Budget";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 25;
                xlsCol++;

                int iActivity = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Activity";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 25;
                xlsCol++;

                int iCurrencyCode = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Currency";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                xlsCol++;

                int iReceivable = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Receivable";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignRight;
                xlsCol++;

                int iReceived = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Received";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignRight;
                xlsCol++;

                int iBalance = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Balance";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignRight;
                xlsCol++;

                int iBookReceivable = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Book Receivable";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignRight;
                xlsCol++;

                int iBookReceived = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Book Received";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignRight;
                xlsCol++;

                int iBookBalance = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Book Balance";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignRight;

                endXlsCol = xlsCol;
                endXlsCol = xlsCol;

                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].WrapText = true;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 23;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;


                int startRow = 0;
                int perStartRow = 0;

                xlsRow++;
                startRow = xlsRow;
                perStartRow = xlsRow;

                for (int i = 0; i < dtGStReceivableF3.Rows.Count; i++)
                {
                    sheet1.Range[xlsRow, iType].Text = dtGStReceivableF3.Rows[i]["PartyType"].ToString();
                    sheet1.Range[xlsRow, iVoucherRowId].Text = dtGStReceivableF3.Rows[i]["VoucherRowId"].ToString();
                    sheet1.Range[xlsRow, iVoucherNo].Text = dtGStReceivableF3.Rows[i]["VoucherNo"].ToString();
                    sheet1.Range[xlsRow, iPartyName].Text = dtGStReceivableF3.Rows[i]["PartyName"].ToString();
                    sheet1.Range[xlsRow, iPostingDate].Text = dtGStReceivableF3.Rows[i]["PostingDate"].ToString();
                    sheet1.Range[xlsRow, iDocDate].Text = dtGStReceivableF3.Rows[i]["DocDate"].ToString();
                    sheet1.Range[xlsRow, iDocRefNo].Text = dtGStReceivableF3.Rows[i]["DocRefNo"].ToString();


                    sheet1.Range[xlsRow, iNarration].Text = dtGStReceivableF3.Rows[i]["Narration"].ToString();
                    sheet1.Range[xlsRow, iGL].Text = dtGStReceivableF3.Rows[i]["GL"].ToString();
                    sheet1.Range[xlsRow, iBudget].Text = dtGStReceivableF3.Rows[i]["Budget"].ToString();
                    sheet1.Range[xlsRow, iActivity].Text = dtGStReceivableF3.Rows[i]["Activity"].ToString();
                    sheet1.Range[xlsRow, iCurrencyCode].Text = dtGStReceivableF3.Rows[i]["CurrencyCode"].ToString();


                    sheet1.Range[xlsRow, iReceivable].Number = clsStaticInfo.dbl(dtGStReceivableF3.Rows[i]["Receivable"].ToString());
                    sheet1.Range[xlsRow, iReceivable].NumberFormat = "#,##0.00;(#,##0.00)";
                    sheet1.Range[xlsRow, iReceived].Number = clsStaticInfo.dbl(dtGStReceivableF3.Rows[i]["Received"].ToString());
                    sheet1.Range[xlsRow, iReceived].NumberFormat = "#,##0.00;(#,##0.00)";
                    sheet1.Range[xlsRow, iBalance].Number = clsStaticInfo.dbl(dtGStReceivableF3.Rows[i]["Balance"].ToString());
                    sheet1.Range[xlsRow, iBalance].NumberFormat = "#,##0.00;(#,##0.00)";
                    sheet1.Range[xlsRow, iBookReceivable].Number = clsStaticInfo.dbl(dtGStReceivableF3.Rows[i]["BookReceivable"].ToString());
                    sheet1.Range[xlsRow, iBookReceivable].NumberFormat = "#,##0.00;(#,##0.00)";
                    sheet1.Range[xlsRow, iBookReceived].Number = clsStaticInfo.dbl(dtGStReceivableF3.Rows[i]["BookReceived"].ToString());
                    sheet1.Range[xlsRow, iBookReceived].NumberFormat = "#,##0.00;(#,##0.00)";
                    sheet1.Range[xlsRow, iBookBalance].Number = clsStaticInfo.dbl(dtGStReceivableF3.Rows[i]["BookBalance"].ToString());
                    sheet1.Range[xlsRow, iBookBalance].NumberFormat = "#,##0.00;(#,##0.00)";

                    xlsRow++;

                }
                sheet1[perStartRow, iType, xlsRow - 1, iType].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iVoucherRowId, xlsRow - 1, iVoucherRowId].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iVoucherNo, xlsRow - 1, iVoucherNo].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iPartyName, xlsRow - 1, iPartyName].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iPostingDate, xlsRow - 1, iPostingDate].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iDocDate, xlsRow - 1, iDocDate].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iDocRefNo, xlsRow - 1, iDocRefNo].BorderAround(ExcelLineStyle.Hair);

                sheet1[perStartRow, iNarration, xlsRow - 1, iNarration].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iGL, xlsRow - 1, iGL].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iBudget, xlsRow - 1, iBudget].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iActivity, xlsRow - 1, iActivity].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iCurrencyCode, xlsRow - 1, iCurrencyCode].BorderAround(ExcelLineStyle.Hair);

                sheet1[perStartRow, iReceivable, xlsRow - 1, iReceivable].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iReceived, xlsRow - 1, iReceived].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iBalance, xlsRow - 1, iBalance].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iBookReceivable, xlsRow - 1, iBookReceivable].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iBookReceived, xlsRow - 1, iBookReceived].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iBookBalance, xlsRow - 1, iBookBalance].BorderAround(ExcelLineStyle.Hair);

                #region ******************Report Header******************


                xlsRow = 1;
                xlsCol = 3;
                try
                {
                    if (companyLogo != null)
                    {

                        double totalWidth = sheet1.GetColumnWidth(1) + sheet1.GetColumnWidth(3);
                        int totalWidthPixel = (int)(totalWidth * 7.5);
                        int totalheight = (int)((sheet1.GetRowHeight(1) + sheet1.GetRowHeight(2) + sheet1.GetRowHeight(3) + sheet1.GetRowHeight(3)) * 1.50);

                        companyLogo = ReportUtility.FixedSize(companyLogo, totalWidthPixel, totalheight);
                        IPictureShape pic = null;

                        pic = sheet1.Pictures.AddPicture(1, 1, companyLogo);

                    }
                }
                catch (Exception ex)
                {
                }

                FactoryName = string.Empty;

                string FactoryAddress = string.Empty;

                if (dtCmp.Rows.Count > 0)
                {
                    CmpName = dtCmp.Rows[0]["CompanyName"].ToString();
                }
                else
                {
                    CmpName = "";
                }
                sheet1.Range[xlsRow, 3].Text = CmpName;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 12;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 17;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                if (dtFactory.Rows.Count > 0)
                {
                    FactoryName = dtFactory.Rows[0]["UserName"].ToString();
                }
                else
                {
                    FactoryName = "";
                }
                sheet1.Range[xlsRow, 3].Text = FactoryName;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 14;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 18;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                if (dtFactory.Rows.Count > 0)
                {
                    FactoryAddress = dtFactory.Rows[0]["Address1"].ToString();
                }
                else
                {
                    FactoryAddress = "";
                }
                sheet1.Range[xlsRow, 3].Text = FactoryAddress;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                //sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 22;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                sheet1.Range[xlsRow, 3].Text = "Credit Note Payment Pending for Set Off Report ";
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;


                #endregion ******************Report Header******************

                #region Freeze Panes

                sheet1.IsDisplayZeros = false;
                sheet1.UsedRange["A7"].FreezePanes();
                sheet1.FirstVisibleColumn = 1;
                sheet1.FirstVisibleRow = 6;

                #endregion Freeze Panes

                #region UsedRange Alignment

                sheet1.UsedRange.WrapText = false;
                sheet1.UsedRange.CellStyle.Font.Size = 10;
                sheet1.Range["A1"].CellStyle.Font.Size = 14;
                sheet1.Range["A2"].CellStyle.Font.Size = 10;
                sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;
                sheet1.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                #endregion UsedRange Alignment

                #region Page Setup
                sheet1.PageSetup.TopMargin = 0.5;
                sheet1.PageSetup.BottomMargin = 0.7;
                sheet1.PageSetup.PrintTitleRows = "$1:$5";
                sheet1.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                sheet1.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + name + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:MM tt").ToString();
                sheet1.PageSetup.LeftMargin = 0.5;
                sheet1.PageSetup.RightMargin = 0.2;
                sheet1.PageSetup.Orientation = ExcelPageOrientation.Portrait;
                sheet1.PageSetup.FitToPagesTall = 0;
                sheet1.PageSetup.FitToPagesWide = 1;
                sheet1.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                sheet1.IsDisplayZeros = false;
                #endregion Page SetupLineItemType
                sheet1.Range[6, 1, 6, endXlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;


                sheet1.Name = "Credit Note Payment Pending for Set Off Report";
                return workbook;
            }
            catch (System.Exception ex)
            {

                throw ex;
            }
        }


        public IWorkbook GetGSTPayableSalesReport(string companyGroupId, string companyId, string plantId, string plantName, string fromDate, string toDate, string name)
        {
            clsReport objRpt = null;
            clsReport objRptSR = null;
            try
            {

                ExcelEngine excelEngine = null;
                IApplication application = null;
                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;
                excelEngine.Excel.DefaultVersion = ExcelVersion.Excel2013;
                var reportUtility = new ReportUtility();
                var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
                workbook.Version = ExcelVersion.Excel2013;
                var sheet1 = workbook.Worksheets[0];

                #region Logo
                string strPath = "";
                Image companyLogo = null;
                try
                {
                    DataTable dtCompanyImage = _sqlRepository.GetDataTable("SELECT * FROM ORG.COMPANY WHERE ID = '" + companyId + @"'");

                    strPath = Path.Combine(ResourcesPathReader.GetLogoOrImagePath(), dtCompanyImage.Rows[0]["Image"].ToString());  // IDCardEng.xlsx
                    companyLogo = Image.FromFile(strPath);
                }
                catch (Exception)
                {
                }
                #endregion
                objRpt = new clsReport();

                objRptSR = new clsReport(_sqlRepository);

                DataTable dtGSTPayable = null;
                string taxyearId = GetTaxYearId(fromDate, toDate, companyId);
                dtGSTPayable = GetGSTPayableSalesSQL(companyGroupId, companyId, plantId, plantName, fromDate, toDate, taxyearId);
                if (dtGSTPayable.Rows.Count == 0)
                {
                    throw new Exception("No Data Found....");
                }

                DataTable dtCmp = objRptSR.SelectedCompanyDT(plantId);

                DataTable dtFactory = objRptSR.SelectedPlantDT(plantId);

                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;

                int xlsRow = 1, xlsCol = 1;
                int endXlsCol = 1;
                string FactoryName = "";
                string CmpName = "";
                xlsRow = 6;
                sheet1.Range[xlsRow - 1, 1].Text = "Report Ref No:";
                sheet1.Range[xlsRow - 1, 1].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow - 1, 1].RowHeight = 20;
                sheet1.Range[xlsRow - 1, 1].CellStyle.Font.Bold = true;

                int iSourceType = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Category";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                xlsCol++;

                int iTaxPercentage = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Percentage";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                xlsCol++;

                int iPartyPlantName = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Party Plant";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 35;
                xlsCol++;

                int iParticulars = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Particulars";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 30;
                xlsCol++;

                int iGSTIN = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "GSTIN";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 25;

                //xlsCol++;
                //int iParticulars = xlsCol; // Party
                //sheet1.Range[xlsRow, xlsCol].Text = "Particulars";
                //sheet1.Range[xlsRow, xlsCol].ColumnWidth = 40;

                xlsCol++;
                int iVoucherNo = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Voucher No";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;

                xlsCol++;
                int iVoucherDate = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Entry Date";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;

                xlsCol++;
                int iPostingDate = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Posting Date";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                xlsCol++;
                int iDocRefNo = xlsCol; // Doc Ref
                sheet1.Range[xlsRow, xlsCol].Text = "DocRef No";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;

                xlsCol++;
                int iDocDate = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Doc Date";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;

                xlsCol++;
                int iSalesNO = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Sales No";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;

                xlsCol++;
                int iTaxableAmount = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Taxable Amount";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignRight;
                DataTable dtTaxCode = null;
                dtGSTPayable.DefaultView.Sort = "TCSequence";
                dtTaxCode = dtGSTPayable.DefaultView.ToTable(true, "TaxCode");
                dtTaxCode.Columns.Add("ColumnNumber", typeof(String));
                dtTaxCode.Columns.Add("ColumnFormula", typeof(String));

                if (dtTaxCode.Rows.Count > 0)
                {
                    for (int i = 0; i < dtTaxCode.Rows.Count; i++)
                    {
                        xlsCol++;
                        sheet1.Range[xlsRow, xlsCol].Text = dtTaxCode.Rows[i]["TaxCode"].ToString();
                        sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                        dtTaxCode.Rows[i]["ColumnNumber"] = xlsCol.ToString();
                    }
                }
                xlsCol++;
                int iTotalTax = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Total Tax";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                endXlsCol = xlsCol;

                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].WrapText = true;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 23;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;

                string voucherNo = "";
                string Percentage = "";
                int startRow = 0;
                int perStartRow = 0;
                string formula = "";
                string formula2 = "";
                string totalFormula = "";

                string lineItemPercentageType = "";
                xlsRow++;
                startRow = xlsRow;
                perStartRow = xlsRow;
                bool isFirst = true;
                string totalTaxformula = "";
                string voucherNocomp = "";
                string taxFitler = "";
                for (int i = 0; i < dtGSTPayable.Rows.Count; i++)
                {
                    //if (dtGSTPayable.Rows[i]["VoucherNo"].ToString() == "SVI-20-21-01005")
                    //{

                    //}

                    if (dtGSTPayable.Rows[i]["LineItemType"].ToString().ToUpper() == "GL")
                    {
                        voucherNocomp = dtGSTPayable.Rows[i]["VoucherNo"].ToString() + "-" + dtGSTPayable.Rows[i]["LineItemType"].ToString().ToUpper();
                        taxFitler = " and VoucherNo = '" + dtGSTPayable.Rows[i]["VoucherNo"].ToString() + "' and LineItemType = '" + dtGSTPayable.Rows[i]["LineItemType"].ToString() + "'";
                    }
                    if (dtGSTPayable.Rows[i]["LineItemType"].ToString().ToUpper() == "SALES")
                    {
                        voucherNocomp = dtGSTPayable.Rows[i]["VoucherNo"].ToString() + "-" + dtGSTPayable.Rows[i]["LineItemType"].ToString().ToUpper() + "-" + dtGSTPayable.Rows[i]["InventoryReceiveDetailId"].ToString().ToUpper();

                        taxFitler = " and VoucherNo = '" + dtGSTPayable.Rows[i]["VoucherNo"].ToString() + "' and LineItemType = '" + dtGSTPayable.Rows[i]["LineItemType"].ToString() + "' and InventoryReceiveDetailId = '" + dtGSTPayable.Rows[i]["InventoryReceiveDetailId"].ToString().ToUpper() + "'";

                    }
                    if (dtGSTPayable.Rows[i]["LineItemType"].ToString().ToUpper() == "SALESSERVICE")
                    {
                        voucherNocomp = dtGSTPayable.Rows[i]["VoucherNo"].ToString() + "-" + dtGSTPayable.Rows[i]["LineItemType"].ToString().ToUpper() + "-" + dtGSTPayable.Rows[i]["InventoryServiceId"].ToString().ToUpper();
                        taxFitler = " and VoucherNo = '" + dtGSTPayable.Rows[i]["VoucherNo"].ToString() + "' and LineItemType = '" + dtGSTPayable.Rows[i]["LineItemType"].ToString() + "' and InventoryServiceId = '" + dtGSTPayable.Rows[i]["InventoryServiceId"].ToString().ToUpper() + "'";
                    }



                    if (voucherNo != voucherNocomp)
                    {

                        if (dtGSTPayable.Rows[i]["LineItemType"].ToString().ToUpper() == "GL")
                        {
                            lineItemPercentageType = "ValueOfFixed";
                        }
                        if (dtGSTPayable.Rows[i]["LineItemType"].ToString().ToUpper() == "SALES")
                        {
                            lineItemPercentageType = "Percentage";
                        }
                        if (dtGSTPayable.Rows[i]["LineItemType"].ToString().ToUpper() == "SALESSERVICE")
                        {
                            lineItemPercentageType = "Percentage";
                        }
                        if (Percentage != dtGSTPayable.Rows[i]["Percentage"].ToString())
                        {
                            if (isFirst == false)
                            {

                                sheet1[perStartRow, iPostingDate, xlsRow - 1, iPostingDate].BorderAround(ExcelLineStyle.Hair);
                                sheet1[perStartRow, iParticulars, xlsRow - 1, iParticulars].BorderAround(ExcelLineStyle.Hair);
                                sheet1[perStartRow, iSourceType, xlsRow - 1, iSourceType].BorderAround(ExcelLineStyle.Hair);
                                sheet1[perStartRow, iTaxPercentage, xlsRow - 1, iTaxPercentage].BorderAround(ExcelLineStyle.Hair);
                                sheet1[perStartRow, iVoucherNo, xlsRow - 1, iVoucherNo].BorderAround(ExcelLineStyle.Hair);
                                sheet1[perStartRow, iVoucherDate, xlsRow - 1, iVoucherDate].BorderAround(ExcelLineStyle.Hair);
                                sheet1[perStartRow, iDocDate, xlsRow - 1, iDocDate].BorderAround(ExcelLineStyle.Hair);
                                sheet1[perStartRow, iDocRefNo, xlsRow - 1, iDocRefNo].BorderAround(ExcelLineStyle.Hair);
                                sheet1[perStartRow, iSalesNO, xlsRow - 1, iSalesNO].BorderAround(ExcelLineStyle.Hair);
                                sheet1[perStartRow, iTaxableAmount, xlsRow - 1, iTaxableAmount].BorderAround(ExcelLineStyle.Hair);
                                sheet1[perStartRow, iTotalTax, xlsRow - 1, iTotalTax].BorderAround(ExcelLineStyle.Hair);

                                formula = "SUM(" + clsStaticInfo.GetxlsCol(iTaxableAmount) + perStartRow + ":" + clsStaticInfo.GetxlsCol(iTaxableAmount) + (xlsRow - 1) + ")";
                                formula2 = "";

                                if (dtTaxCode.Rows.Count > 0)
                                {
                                    totalTaxformula = "SUM(";
                                    for (int j = 0; j < dtTaxCode.Rows.Count; j++)
                                    {
                                        sheet1[perStartRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"]), xlsRow - 1, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])].BorderAround(ExcelLineStyle.Hair);
                                        formula2 = "SUM(" + clsStaticInfo.GetxlsCol(Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])) + perStartRow + ":" + clsStaticInfo.GetxlsCol(Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])) + (xlsRow - 1) + ")";
                                        sheet1[xlsRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"]), xlsRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])].Formula = formula2;

                                        dtTaxCode.Rows[j]["ColumnFormula"] += (clsStaticInfo.GetxlsCol(Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])) + xlsRow).ToString() + " + ";
                                        totalTaxformula += clsStaticInfo.GetxlsCol(Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])) + xlsRow + ":";

                                    }
                                }
                                sheet1.Range[xlsRow, 1, xlsRow, 1].Text = "Total";

                                sheet1[xlsRow, iTaxableAmount, xlsRow, iTaxableAmount].Formula = formula;
                                sheet1[xlsRow, iTotalTax, xlsRow, iTotalTax].Formula = totalTaxformula.Remove(totalTaxformula.Length - 1) + ")";

                                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Font.Bold = true;
                                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].NumberFormat = "#,##0.00;(#,##0.00)";
                                totalFormula += (clsStaticInfo.GetxlsCol(iTaxableAmount) + xlsRow).ToString() + "+";

                                xlsRow++;


                            }
                            xlsRow++;
                            sheet1.Range[xlsRow - 1, 1].Number = clsStaticInfo.dbl(dtGSTPayable.Rows[i]["Percentage"].ToString());
                            sheet1.Range[xlsRow - 1, 1].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                            perStartRow = xlsRow;
                            isFirst = false;

                        }

                        sheet1.Range[xlsRow, iPartyPlantName].Text = dtGSTPayable.Rows[i]["PartyPlantName"].ToString();
                        sheet1.Range[xlsRow, iParticulars].Text = dtGSTPayable.Rows[i]["Particular"].ToString();
                        sheet1.Range[xlsRow, iGSTIN].Text = dtGSTPayable.Rows[i]["GSTIN"].ToString();

                        sheet1.Range[xlsRow, iDocRefNo].Text = dtGSTPayable.Rows[i]["DocRefNo"].ToString();
                        sheet1.Range[xlsRow, iSourceType].Text = dtGSTPayable.Rows[i]["SourceType"].ToString();
                        //sheet1.Range[xlsRow, iTaxPercentage].Text = dtGSTPayable.Rows[i]["TaxPercentage"].ToString();
                        sheet1.Range[xlsRow, iTaxPercentage].Number = clsStaticInfo.dbl(dtGSTPayable.Rows[i]["Percentage"].ToString());
                        //sheet1.Range[xlsRow, iTaxPercentage].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                        sheet1.Range[xlsRow, iTaxPercentage].NumberFormat = "#,##0.00;(#,##0.00)";
                        sheet1.Range[xlsRow, iVoucherNo].Text = dtGSTPayable.Rows[i]["VoucherNo"].ToString();
                        sheet1.Range[xlsRow, iVoucherDate].Text = dtGSTPayable.Rows[i]["VoucherDate"].ToString();
                        sheet1.Range[xlsRow, iPostingDate].Text = clsStaticInfo.GetDateTaxFormate(dtGSTPayable.Rows[i]["PostingDate"].ToString());
                        //sheet1.Range[xlsRow, iPostingDate].DateTime = "dd/MMM/yyyy";

                        sheet1.Range[xlsRow, iDocDate].Text = dtGSTPayable.Rows[i]["DocDate"].ToString();
                        sheet1.Range[xlsRow, iSalesNO].Text = dtGSTPayable.Rows[i]["GRNNo"].ToString();


                        sheet1.Range[xlsRow, iTaxableAmount].Number = clsStaticInfo.dbl(dtGSTPayable.Rows[i]["TaxableAmount"].ToString());//TaxableAmount
                        //sheet1.Range[xlsRow, iTaxableAmount].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                        sheet1.Range[xlsRow, iTaxableAmount].NumberFormat = "#,##0.00;(#,##0.00)";
                        //dtGSTPayable.DefaultView.RowFilter = "VoucherNo = '" + dtGSTPayable.Rows[i]["VoucherNo"].ToString() + "'";

                        if (dtTaxCode.Rows.Count > 0)
                        {
                            totalTaxformula = "=SUM(";
                            for (int j = 0; j < dtTaxCode.Rows.Count; j++)
                            {
                                dtGSTPayable.DefaultView.RowFilter = "TaxCode = '" + dtTaxCode.Rows[j]["TaxCode"].ToString() + "'" + taxFitler;
                                if (dtGSTPayable.DefaultView.Count > 0)
                                {

                                    sheet1.Range[xlsRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])].Number = clsStaticInfo.dbl(dtGSTPayable.DefaultView[0]["CrAmount"].ToString());
                                    sheet1.Range[xlsRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                                    sheet1.Range[xlsRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])].NumberFormat = "#,##0.00;(#,##0.00)";
                                }
                                else
                                {
                                    sheet1.Range[xlsRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])].Text = "-";
                                    sheet1.Range[xlsRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])].HorizontalAlignment = ExcelHAlign.HAlignRight;


                                }
                                totalTaxformula += clsStaticInfo.GetxlsCol(Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])) + xlsRow + ":";
                            }
                            sheet1.Range[xlsRow, iTotalTax].Formula = totalTaxformula.Remove(totalTaxformula.Length - 1) + ")";

                        }

                        Percentage = dtGSTPayable.Rows[i]["Percentage"].ToString();

                        xlsRow++;
                    }


                    if (dtGSTPayable.Rows[i]["LineItemType"].ToString().ToUpper() == "GL")
                    {
                        voucherNo = dtGSTPayable.Rows[i]["VoucherNo"].ToString() + "-" + dtGSTPayable.Rows[i]["LineItemType"].ToString().ToUpper();

                    }
                    if (dtGSTPayable.Rows[i]["LineItemType"].ToString().ToUpper() == "SALES")
                    {
                        voucherNo = dtGSTPayable.Rows[i]["VoucherNo"].ToString() + "-" + dtGSTPayable.Rows[i]["LineItemType"].ToString().ToUpper() + "-" + dtGSTPayable.Rows[i]["InventoryReceiveDetailId"].ToString().ToUpper();

                    }
                    if (dtGSTPayable.Rows[i]["LineItemType"].ToString().ToUpper() == "SALESSERVICE")
                    {
                        voucherNo = dtGSTPayable.Rows[i]["VoucherNo"].ToString() + "-" + dtGSTPayable.Rows[i]["LineItemType"].ToString().ToUpper() + "-" + dtGSTPayable.Rows[i]["InventoryServiceId"].ToString().ToUpper();

                    }


                }
                sheet1[perStartRow, iPartyPlantName, xlsRow - 1, iPartyPlantName].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iPostingDate, xlsRow - 1, iPostingDate].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iParticulars, xlsRow - 1, iParticulars].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iTaxPercentage, xlsRow - 1, iTaxPercentage].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iVoucherNo, xlsRow - 1, iVoucherNo].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iVoucherDate, xlsRow - 1, iVoucherDate].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iDocDate, xlsRow - 1, iDocDate].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iSalesNO, xlsRow - 1, iSalesNO].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iDocRefNo, xlsRow - 1, iDocRefNo].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iSourceType, xlsRow - 1, iSourceType].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iTaxableAmount, xlsRow - 1, iTaxableAmount].BorderAround(ExcelLineStyle.Hair);
                //sheet1[perStartRow, iTaxableAmount, xlsRow - 1, iTaxableAmount].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iTotalTax, xlsRow - 1, iTotalTax].BorderAround(ExcelLineStyle.Hair);


                if (dtTaxCode.Rows.Count > 0)
                {
                    for (int j = 0; j < dtTaxCode.Rows.Count; j++)
                    {
                        sheet1[perStartRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"]), xlsRow - 1, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])].BorderAround(ExcelLineStyle.Hair);

                    }
                }



                if (dtTaxCode.Rows.Count > 0)
                {
                    totalTaxformula = "=SUM(";
                    for (int j = 0; j < dtTaxCode.Rows.Count; j++)
                    {
                        sheet1[perStartRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"]), xlsRow - 1, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])].BorderAround(ExcelLineStyle.Hair);
                        formula2 = "SUM(" + clsStaticInfo.GetxlsCol(Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])) + perStartRow + ":" + clsStaticInfo.GetxlsCol(Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])) + (xlsRow - 1) + ")";
                        sheet1[xlsRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"]), xlsRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])].Formula = formula2;

                        dtTaxCode.Rows[j]["ColumnFormula"] += (clsStaticInfo.GetxlsCol(Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])) + xlsRow).ToString() + " + ";
                        totalTaxformula += clsStaticInfo.GetxlsCol(Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])) + xlsRow + ":";

                    }
                }
                sheet1.Range[xlsRow, 1, xlsRow, 1].Text = "Total";
                formula = "SUM(" + clsStaticInfo.GetxlsCol(iTaxableAmount) + perStartRow + ":" + clsStaticInfo.GetxlsCol(iTaxableAmount) + (xlsRow - 1) + ")";

                sheet1[xlsRow, iTaxableAmount, xlsRow, iTaxableAmount].Formula = formula;
                sheet1[xlsRow, iTotalTax, xlsRow, iTotalTax].Formula = totalTaxformula.Remove(totalTaxformula.Length - 1) + ")";

                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].NumberFormat = "#,##0.00;(#,##0.00)";
                totalFormula += (clsStaticInfo.GetxlsCol(iTaxableAmount) + xlsRow).ToString() + "+";



                xlsRow++;
                xlsRow++;


                if (dtTaxCode.Rows.Count > 0)
                {
                    totalTaxformula = "=SUM(";
                    for (int j = 0; j < dtTaxCode.Rows.Count; j++)
                    {
                        string fm = dtTaxCode.Rows[j]["ColumnFormula"].ToString().Trim();
                        sheet1[xlsRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"]), xlsRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])].Formula = fm.Remove(fm.Length - 1); //dtTaxCode.Rows[j]["ColumnFormula"].ToString().Remove(dtTaxCode.Rows[j]["ColumnFormula"].ToString().Length - 1);
                        totalTaxformula += clsStaticInfo.GetxlsCol(Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])) + xlsRow + ":";

                    }
                }
                sheet1.Range[xlsRow, 1, xlsRow, 1].Text = "Grand Total";

                sheet1[xlsRow, iTaxableAmount, xlsRow, iTaxableAmount].Formula = totalFormula.Remove(totalFormula.Length - 1);
                sheet1[xlsRow, iTotalTax, xlsRow, iTotalTax].Formula = totalTaxformula.Remove(totalTaxformula.Length - 1) + ")";

                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].NumberFormat = "#,##0.00;(#,##0.00)";

                #region ******************Report Header******************

                xlsRow = 1;
                xlsCol = 3;
                try
                {
                    if (companyLogo != null)
                    {

                        double totalWidth = sheet1.GetColumnWidth(1) + sheet1.GetColumnWidth(3);
                        int totalWidthPixel = (int)(totalWidth * 7.5);
                        int totalheight = (int)((sheet1.GetRowHeight(1) + sheet1.GetRowHeight(2) + sheet1.GetRowHeight(3) + sheet1.GetRowHeight(3)) * 1.50);

                        companyLogo = ReportUtility.FixedSize(companyLogo, totalWidthPixel, totalheight);
                        IPictureShape pic = null;

                        pic = sheet1.Pictures.AddPicture(1, 1, companyLogo);
                        //pic.Height = 80;
                        //pic.Width = 220;
                    }
                }
                catch (Exception ex)
                {
                }

                FactoryName = string.Empty;

                string FactoryAddress = string.Empty;

                if (dtCmp.Rows.Count > 0)
                {
                    CmpName = dtCmp.Rows[0]["CompanyName"].ToString();
                }
                else
                {
                    CmpName = "";
                }
                sheet1.Range[xlsRow, 3].Text = CmpName;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 12;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 17;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                if (dtFactory.Rows.Count > 0)
                {
                    //FactoryName = dsFactory.Tables[0].Rows[0]["PlantName"].ToString();
                    FactoryName = dtFactory.Rows[0]["UserName"].ToString();
                }
                else
                {
                    FactoryName = "";
                }
                sheet1.Range[xlsRow, 3].Text = FactoryName;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 14;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 18;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                if (dtFactory.Rows.Count > 0)
                {
                    FactoryAddress = dtFactory.Rows[0]["Address1"].ToString();
                }
                else
                {
                    FactoryAddress = "";
                }
                sheet1.Range[xlsRow, 3].Text = FactoryAddress;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                //sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 22;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                sheet1.Range[xlsRow, 3].Text = "GST Payable Sales Report (Format 1) From " + fromDate + " To " + toDate;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;


                #endregion ******************Report Header******************

                #region Freeze Panes

                sheet1.IsDisplayZeros = false;
                sheet1.UsedRange["A7"].FreezePanes();
                sheet1.FirstVisibleColumn = 1;
                sheet1.FirstVisibleRow = 6;

                #endregion Freeze Panes

                #region UsedRange Alignment

                sheet1.UsedRange.WrapText = false;
                sheet1.UsedRange.CellStyle.Font.Size = 10;
                sheet1.Range["A1"].CellStyle.Font.Size = 14;
                sheet1.Range["A2"].CellStyle.Font.Size = 10;
                sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;
                sheet1.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                #endregion UsedRange Alignment

                #region Page Setup
                sheet1.PageSetup.TopMargin = 0.5;
                sheet1.PageSetup.BottomMargin = 0.7;
                sheet1.PageSetup.PrintTitleRows = "$1:$5";
                sheet1.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                sheet1.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + name + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:MM tt").ToString();
                sheet1.PageSetup.LeftMargin = 0.5;
                sheet1.PageSetup.RightMargin = 0.2;
                sheet1.PageSetup.Orientation = ExcelPageOrientation.Portrait;
                sheet1.PageSetup.FitToPagesTall = 0;
                sheet1.PageSetup.FitToPagesWide = 1;
                sheet1.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                sheet1.IsDisplayZeros = false;
                #endregion Page Setup


                sheet1.Name = "GST Payable Sales";
                return workbook;
            }
            catch (System.Exception ex)
            {

                throw ex;
            }
        }
        public IWorkbook GetGSTPayableSalesReport2(string companyGroupId, string companyId, string plantId, string plantName, string fromDate, string toDate, string name)
        {
            clsReport objRpt = null;
            clsReport objRptSR = null;
            try
            {

                ExcelEngine excelEngine = null;
                IApplication application = null;
                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;
                excelEngine.Excel.DefaultVersion = ExcelVersion.Excel2013;
                var reportUtility = new ReportUtility();
                var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
                workbook.Version = ExcelVersion.Excel2013;
                var sheet1 = workbook.Worksheets[0];

                #region Logo
                string strPath = "";
                Image companyLogo = null;
                try
                {
                    DataTable dtCompanyImage = _sqlRepository.GetDataTable("SELECT * FROM ORG.COMPANY WHERE ID = '" + companyId + @"'");

                    strPath = Path.Combine(ResourcesPathReader.GetLogoOrImagePath(), dtCompanyImage.Rows[0]["Image"].ToString());  // IDCardEng.xlsx
                    companyLogo = Image.FromFile(strPath);
                }
                catch (Exception)
                {
                }
                #endregion
                objRpt = new clsReport();

                objRptSR = new clsReport(_sqlRepository);

                DataTable dtRCMPayable = null;
                string taxyearId = GetTaxYearId(fromDate, toDate, companyId);
                dtRCMPayable = GetGSTPayableSalesSQL(companyGroupId, companyId, plantId, plantName, fromDate, toDate, taxyearId);
                if (dtRCMPayable.Rows.Count == 0)
                {
                    throw new Exception("No Data Found....");
                }

                DataTable dtCmp = objRptSR.SelectedCompanyDT(plantId);

                DataTable dtFactory = objRptSR.SelectedPlantDT(plantId);

                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;

                int xlsRow = 1, xlsCol = 1;
                int endXlsCol = 1;
                string FactoryName = "";
                string CmpName = "";
                xlsRow = 6;
                sheet1.Range[xlsRow - 1, 1].Text = "Report Ref No:";
                sheet1.Range[xlsRow - 1, 1].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow - 1, 1].RowHeight = 20;
                sheet1.Range[xlsRow - 1, 1].CellStyle.Font.Bold = true;

                int iPartyPlantName = xlsCol; // Party
                sheet1.Range[xlsRow, xlsCol].Text = "Party Plant";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 35;
                xlsCol++;

                int iParticulars = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Particulars";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 30;
                xlsCol++;

                int iGSTIN = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "GSTIN";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 25;

                xlsCol++;
                int iVoucherNo = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Voucher No";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;

                xlsCol++;
                int iTaxPercentage = xlsCol; // Party
                sheet1.Range[xlsRow, xlsCol].Text = "Percentage";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                //xlsCol++; 
                //int iParticulars = xlsCol; // Party
                //sheet1.Range[xlsRow, xlsCol].Text = "Particulars";
                //sheet1.Range[xlsRow, xlsCol].ColumnWidth = 40;



                //xlsCol++;
                //int iVoucherDate = xlsCol;
                //sheet1.Range[xlsRow, xlsCol].Text = "Entry Date";
                //sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;

                //xlsCol++;
                //int iPostingDate = xlsCol;
                //sheet1.Range[xlsRow, xlsCol].Text = "Posting Date";
                //sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;

                //xlsCol++;
                //int iDocDate = xlsCol;
                //sheet1.Range[xlsRow, xlsCol].Text = "Doc Date";
                //sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;

                //xlsCol++;
                //int iDocRefNo = xlsCol; // Doc Ref
                //sheet1.Range[xlsRow, xlsCol].Text = "DocRef No";
                //sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;

                //xlsCol++;
                //int iGRNNo = xlsCol;
                //sheet1.Range[xlsRow, xlsCol].Text = "GRN No";
                //sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;

                xlsCol++;
                int iTaxableAmount = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Taxable Amount";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignRight;
                DataTable dtTaxCode = null;
                dtRCMPayable.DefaultView.Sort = "TCSequence";
                dtTaxCode = dtRCMPayable.DefaultView.ToTable(true, "TaxCode");
                dtTaxCode.Columns.Add("ColumnNumber", typeof(String));
                dtTaxCode.Columns.Add("ColumnFormula", typeof(String));

                if (dtTaxCode.Rows.Count > 0)
                {
                    for (int i = 0; i < dtTaxCode.Rows.Count; i++)
                    {
                        xlsCol++;
                        sheet1.Range[xlsRow, xlsCol].Text = dtTaxCode.Rows[i]["TaxCode"].ToString();
                        sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                        dtTaxCode.Rows[i]["ColumnNumber"] = xlsCol.ToString();
                    }
                }
                xlsCol++;
                int iTotalTax = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Total Tax";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                endXlsCol = xlsCol;

                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].WrapText = true;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 23;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;

                string voucherNo = "";
                string Percentage = "";
                int startRow = 0;
                int perStartRow = 0;
                string formula = "";
                string formula2 = "";
                string totalFormula = "";

                string lineItemPercentageType = "";
                xlsRow++;
                startRow = xlsRow;
                perStartRow = xlsRow;
                bool isFirst = true;
                string totalTaxformula = "";
                string taxFitler = "";
                string voucherNocomp = "";

                for (int i = 0; i < dtRCMPayable.Rows.Count; i++)
                {

                    if (dtRCMPayable.Rows[i]["LineItemType"].ToString().ToUpper() == "GL")
                    {
                        voucherNocomp = dtRCMPayable.Rows[i]["VoucherNo"].ToString() + "-" + dtRCMPayable.Rows[i]["LineItemType"].ToString().ToUpper();
                        taxFitler = " and VoucherNo = '" + dtRCMPayable.Rows[i]["VoucherNo"].ToString() + "' and LineItemType = '" + dtRCMPayable.Rows[i]["LineItemType"].ToString() + "'";
                    }
                    if (dtRCMPayable.Rows[i]["LineItemType"].ToString().ToUpper() == "SALES")
                    {
                        voucherNocomp = dtRCMPayable.Rows[i]["VoucherNo"].ToString() + "-" + dtRCMPayable.Rows[i]["LineItemType"].ToString().ToUpper() + "-" + dtRCMPayable.Rows[i]["InventoryReceiveDetailId"].ToString().ToUpper();

                        taxFitler = " and VoucherNo = '" + dtRCMPayable.Rows[i]["VoucherNo"].ToString() + "' and LineItemType = '" + dtRCMPayable.Rows[i]["LineItemType"].ToString() + "' and InventoryReceiveDetailId = '" + dtRCMPayable.Rows[i]["InventoryReceiveDetailId"].ToString().ToUpper() + "'";

                    }
                    if (dtRCMPayable.Rows[i]["LineItemType"].ToString().ToUpper() == "SALESSERVICE")
                    {
                        voucherNocomp = dtRCMPayable.Rows[i]["VoucherNo"].ToString() + "-" + dtRCMPayable.Rows[i]["LineItemType"].ToString().ToUpper() + "-" + dtRCMPayable.Rows[i]["InventoryServiceId"].ToString().ToUpper();
                        taxFitler = " and VoucherNo = '" + dtRCMPayable.Rows[i]["VoucherNo"].ToString() + "' and LineItemType = '" + dtRCMPayable.Rows[i]["LineItemType"].ToString() + "' and InventoryServiceId = '" + dtRCMPayable.Rows[i]["InventoryServiceId"].ToString().ToUpper() + "'";

                    }
                    if (voucherNo != voucherNocomp)
                    {

                        if (dtRCMPayable.Rows[i]["LineItemType"].ToString().ToUpper() == "GL")
                        {
                            lineItemPercentageType = "ValueOfFixed";
                        }
                        if (dtRCMPayable.Rows[i]["LineItemType"].ToString().ToUpper() == "SALES")
                        {
                            lineItemPercentageType = "Percentage";
                        }
                        if (Percentage != dtRCMPayable.Rows[i]["Percentage"].ToString())
                        {
                            if (isFirst == false)
                            {

                                sheet1[perStartRow, iParticulars, xlsRow - 1, iParticulars].BorderAround(ExcelLineStyle.Hair);
                                sheet1[perStartRow, iTaxPercentage, xlsRow - 1, iTaxPercentage].BorderAround(ExcelLineStyle.Hair);
                                sheet1[perStartRow, iTaxableAmount, xlsRow - 1, iTaxableAmount].BorderAround(ExcelLineStyle.Hair);
                                sheet1[perStartRow, iTotalTax, xlsRow - 1, iTotalTax].BorderAround(ExcelLineStyle.Hair);

                                formula = "SUM(" + clsStaticInfo.GetxlsCol(iTaxableAmount) + perStartRow + ":" + clsStaticInfo.GetxlsCol(iTaxableAmount) + (xlsRow - 1) + ")";
                                formula2 = "";

                                if (dtTaxCode.Rows.Count > 0)
                                {
                                    totalTaxformula = "SUM(";
                                    for (int j = 0; j < dtTaxCode.Rows.Count; j++)
                                    {
                                        sheet1[perStartRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"]), xlsRow - 1, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])].BorderAround(ExcelLineStyle.Hair);
                                        formula2 = "SUM(" + clsStaticInfo.GetxlsCol(Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])) + perStartRow + ":" + clsStaticInfo.GetxlsCol(Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])) + (xlsRow - 1) + ")";
                                        sheet1[xlsRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"]), xlsRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])].Formula = formula2;

                                        dtTaxCode.Rows[j]["ColumnFormula"] += (clsStaticInfo.GetxlsCol(Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])) + xlsRow).ToString() + " + ";
                                        totalTaxformula += clsStaticInfo.GetxlsCol(Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])) + xlsRow + ":";

                                    }
                                }
                                sheet1.Range[xlsRow, 1, xlsRow, 1].Text = "Total";

                                sheet1[xlsRow, iTaxableAmount, xlsRow, iTaxableAmount].Formula = formula;
                                sheet1[xlsRow, iTotalTax, xlsRow, iTotalTax].Formula = totalTaxformula.Remove(totalTaxformula.Length - 1) + ")";

                                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Font.Bold = true;
                                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].NumberFormat = "#,##0.00;(#,##0.00)";
                                totalFormula += (clsStaticInfo.GetxlsCol(iTaxableAmount) + xlsRow).ToString() + "+";

                                xlsRow++;


                            }
                            xlsRow++;
                            sheet1.Range[xlsRow - 1, 1].Number = clsStaticInfo.dbl(dtRCMPayable.Rows[i]["Percentage"].ToString());
                            //sheet1.Range[xlsRow - 1, 1].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                            sheet1.Range[xlsRow - 1, 1].NumberFormat = "#,##0.00;(#,##0.00)";
                            perStartRow = xlsRow;
                            isFirst = false;

                        }


                        sheet1.Range[xlsRow, iPartyPlantName].Text = dtRCMPayable.Rows[i]["PartyPlantName"].ToString();
                        sheet1.Range[xlsRow, iParticulars].Text = dtRCMPayable.Rows[i]["Particular"].ToString();
                        sheet1.Range[xlsRow, iGSTIN].Text = dtRCMPayable.Rows[i]["GSTIN"].ToString();
                        sheet1.Range[xlsRow, iVoucherNo].Text = dtRCMPayable.Rows[i]["VoucherNo"].ToString();

                        sheet1.Range[xlsRow, iTaxPercentage].Number = clsStaticInfo.dbl(dtRCMPayable.Rows[i]["Percentage"].ToString());
                        //sheet1.Range[xlsRow, iTaxPercentage].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                        sheet1.Range[xlsRow, iTaxPercentage].NumberFormat = "#,##0.00;(#,##0.00)";


                        sheet1.Range[xlsRow, iTaxableAmount].Number = clsStaticInfo.dbl(dtRCMPayable.Rows[i]["TaxableAmount"].ToString());//TaxableAmount
                        //sheet1.Range[xlsRow, iTaxableAmount].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                        sheet1.Range[xlsRow, iTaxableAmount].NumberFormat = "#,##0.00;(#,##0.00)";
                        dtRCMPayable.DefaultView.RowFilter = "VoucherNo = '" + dtRCMPayable.Rows[i]["VoucherNo"].ToString() + "'";

                        if (dtTaxCode.Rows.Count > 0)
                        {
                            totalTaxformula = "=SUM(";
                            for (int j = 0; j < dtTaxCode.Rows.Count; j++)
                            {
                                dtRCMPayable.DefaultView.RowFilter = "TaxCode = '" + dtTaxCode.Rows[j]["TaxCode"].ToString() + "'" + taxFitler;
                                //dtRCMPayable.DefaultView.RowFilter = "TaxCode = '" + dtTaxCode.Rows[j]["TaxCode"].ToString() + "' and VoucherNo = '" + dtRCMPayable.Rows[i]["VoucherNo"].ToString() + "' and LineItemType = '" + dtRCMPayable.Rows[i]["LineItemType"].ToString() + "'";
                                if (dtRCMPayable.DefaultView.Count > 0)
                                {

                                    sheet1.Range[xlsRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])].Number = clsStaticInfo.dbl(dtRCMPayable.DefaultView[0]["CrAmount"].ToString());
                                    //sheet1.Range[xlsRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                                    sheet1.Range[xlsRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])].NumberFormat = "#,##0.00;(#,##0.00)";
                                }
                                else
                                {
                                    sheet1.Range[xlsRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])].Text = "-";
                                    sheet1.Range[xlsRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])].HorizontalAlignment = ExcelHAlign.HAlignRight;


                                }
                                totalTaxformula += clsStaticInfo.GetxlsCol(Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])) + xlsRow + ":";
                            }
                            sheet1.Range[xlsRow, iTotalTax].Formula = totalTaxformula.Remove(totalTaxformula.Length - 1) + ")";

                        }

                        Percentage = dtRCMPayable.Rows[i]["Percentage"].ToString();



                        xlsRow++;
                    }

                    if (dtRCMPayable.Rows[i]["LineItemType"].ToString().ToUpper() == "GL")
                    {
                        voucherNo = dtRCMPayable.Rows[i]["VoucherNo"].ToString() + "-" + dtRCMPayable.Rows[i]["LineItemType"].ToString().ToUpper();

                    }
                    if (dtRCMPayable.Rows[i]["LineItemType"].ToString().ToUpper() == "SALES")
                    {
                        voucherNo = dtRCMPayable.Rows[i]["VoucherNo"].ToString() + "-" + dtRCMPayable.Rows[i]["LineItemType"].ToString().ToUpper() + "-" + dtRCMPayable.Rows[i]["InventoryReceiveDetailId"].ToString().ToUpper();


                    }
                    if (dtRCMPayable.Rows[i]["LineItemType"].ToString().ToUpper() == "SALESSERVICE")
                    {
                        voucherNo = dtRCMPayable.Rows[i]["VoucherNo"].ToString() + "-" + dtRCMPayable.Rows[i]["LineItemType"].ToString().ToUpper() + "-" + dtRCMPayable.Rows[i]["InventoryServiceId"].ToString().ToUpper();


                    }

                }
                sheet1[perStartRow, iPartyPlantName, xlsRow - 1, iParticulars].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iParticulars, xlsRow - 1, iParticulars].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iTaxPercentage, xlsRow - 1, iTaxPercentage].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iTaxableAmount, xlsRow - 1, iTaxableAmount].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iVoucherNo, xlsRow - 1, iVoucherNo].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iTotalTax, xlsRow - 1, iTotalTax].BorderAround(ExcelLineStyle.Hair);


                if (dtTaxCode.Rows.Count > 0)
                {
                    for (int j = 0; j < dtTaxCode.Rows.Count; j++)
                    {
                        sheet1[perStartRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"]), xlsRow - 1, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])].BorderAround(ExcelLineStyle.Hair);

                    }
                }



                if (dtTaxCode.Rows.Count > 0)
                {
                    totalTaxformula = "=SUM(";
                    for (int j = 0; j < dtTaxCode.Rows.Count; j++)
                    {
                        sheet1[perStartRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"]), xlsRow - 1, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])].BorderAround(ExcelLineStyle.Hair);
                        formula2 = "SUM(" + clsStaticInfo.GetxlsCol(Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])) + perStartRow + ":" + clsStaticInfo.GetxlsCol(Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])) + (xlsRow - 1) + ")";
                        sheet1[xlsRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"]), xlsRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])].Formula = formula2;

                        dtTaxCode.Rows[j]["ColumnFormula"] += (clsStaticInfo.GetxlsCol(Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])) + xlsRow).ToString() + " + ";
                        totalTaxformula += clsStaticInfo.GetxlsCol(Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])) + xlsRow + ":";

                    }
                }
                sheet1.Range[xlsRow, 1, xlsRow, 1].Text = "Total";
                formula = "SUM(" + clsStaticInfo.GetxlsCol(iTaxableAmount) + perStartRow + ":" + clsStaticInfo.GetxlsCol(iTaxableAmount) + (xlsRow - 1) + ")";

                sheet1[xlsRow, iTaxableAmount, xlsRow, iTaxableAmount].Formula = formula;
                sheet1[xlsRow, iTotalTax, xlsRow, iTotalTax].Formula = totalTaxformula.Remove(totalTaxformula.Length - 1) + ")";

                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].NumberFormat = "#,##0.00;(#,##0.00)";
                totalFormula += (clsStaticInfo.GetxlsCol(iTaxableAmount) + xlsRow).ToString() + "+";



                xlsRow++;
                xlsRow++;


                if (dtTaxCode.Rows.Count > 0)
                {
                    totalTaxformula = "=SUM(";
                    for (int j = 0; j < dtTaxCode.Rows.Count; j++)
                    {
                        string fm = dtTaxCode.Rows[j]["ColumnFormula"].ToString().Trim();
                        sheet1[xlsRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"]), xlsRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])].Formula = fm.Remove(fm.Length - 1); //dtTaxCode.Rows[j]["ColumnFormula"].ToString().Remove(dtTaxCode.Rows[j]["ColumnFormula"].ToString().Length - 1);
                        totalTaxformula += clsStaticInfo.GetxlsCol(Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])) + xlsRow + ":";

                    }
                }
                sheet1.Range[xlsRow, 1, xlsRow, 1].Text = "Grand Total";

                sheet1[xlsRow, iTaxableAmount, xlsRow, iTaxableAmount].Formula = totalFormula.Remove(totalFormula.Length - 1);
                sheet1[xlsRow, iTotalTax, xlsRow, iTotalTax].Formula = totalTaxformula.Remove(totalTaxformula.Length - 1) + ")";
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].NumberFormat = "#,##0.00;(#,##0.00)";


                #region ******************Report Header******************



                xlsRow = 1;
                xlsCol = 3;
                try
                {
                    if (companyLogo != null)
                    {

                        double totalWidth = sheet1.GetColumnWidth(1) + sheet1.GetColumnWidth(3);
                        int totalWidthPixel = (int)(totalWidth * 7.5);
                        int totalheight = (int)((sheet1.GetRowHeight(1) + sheet1.GetRowHeight(2) + sheet1.GetRowHeight(3) + sheet1.GetRowHeight(3)) * 1.50);

                        companyLogo = ReportUtility.FixedSize(companyLogo, totalWidthPixel, totalheight);
                        IPictureShape pic = null;

                        pic = sheet1.Pictures.AddPicture(1, 1, companyLogo);
                        //pic.Height = 80;
                        //pic.Width = 220;
                    }
                }
                catch (Exception ex)
                {
                }

                FactoryName = string.Empty;

                string FactoryAddress = string.Empty;

                if (dtCmp.Rows.Count > 0)
                {
                    CmpName = dtCmp.Rows[0]["CompanyName"].ToString();
                }
                else
                {
                    CmpName = "";
                }
                sheet1.Range[xlsRow, 3].Text = CmpName;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 12;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 17;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                if (dtFactory.Rows.Count > 0)
                {
                    //FactoryName = dsFactory.Tables[0].Rows[0]["PlantName"].ToString();
                    FactoryName = dtFactory.Rows[0]["UserName"].ToString();
                }
                else
                {
                    FactoryName = "";
                }
                sheet1.Range[xlsRow, 3].Text = FactoryName;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 14;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 18;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                if (dtFactory.Rows.Count > 0)
                {
                    FactoryAddress = dtFactory.Rows[0]["Address1"].ToString();
                }
                else
                {
                    FactoryAddress = "";
                }
                sheet1.Range[xlsRow, 3].Text = FactoryAddress;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                //sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 22;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                sheet1.Range[xlsRow, 3].Text = "GST Payable Sales Report (Format 2) From " + fromDate + " To " + toDate;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;


                #endregion ******************Report Header******************

                #region Freeze Panes

                sheet1.IsDisplayZeros = false;
                sheet1.UsedRange["A7"].FreezePanes();
                sheet1.FirstVisibleColumn = 1;
                sheet1.FirstVisibleRow = 6;

                #endregion Freeze Panes

                #region UsedRange Alignment

                sheet1.UsedRange.WrapText = false;
                sheet1.UsedRange.CellStyle.Font.Size = 10;
                sheet1.Range["A1"].CellStyle.Font.Size = 14;
                sheet1.Range["A2"].CellStyle.Font.Size = 10;
                sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;
                sheet1.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                #endregion UsedRange Alignment

                #region Page Setup
                sheet1.PageSetup.TopMargin = 0.5;
                sheet1.PageSetup.BottomMargin = 0.7;
                sheet1.PageSetup.PrintTitleRows = "$1:$5";
                sheet1.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                sheet1.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + name + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:MM tt").ToString();
                sheet1.PageSetup.LeftMargin = 0.5;
                sheet1.PageSetup.RightMargin = 0.2;
                sheet1.PageSetup.Orientation = ExcelPageOrientation.Portrait;
                sheet1.PageSetup.FitToPagesTall = 0;
                sheet1.PageSetup.FitToPagesWide = 1;
                sheet1.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                sheet1.IsDisplayZeros = false;
                #endregion Page Setup


                sheet1.Name = "GST Payable Sales";
                return workbook;
            }
            catch (System.Exception ex)
            {

                throw ex;
            }
        }

        public string GetTaxYearId3(string fromDate, string toDate, string companyId)
        {
            try
            {
                string taxYearId = "";
                var sql = @"SELECT DISTINCT TY.Id TaxCodeYearId
                        FROM [MST].[TaxCodeYear] AS TCY
					    LEFT JOIN [SCS].[TaxYear] AS TY ON TY.Id=TCY.TaxYearId
						LEFT JOIN [SCS].[TaxYearPeriod] AS TYP ON TYP.TaxYearId=TY.Id  
                        WHERE TYP.StartDate between  '" + fromDate.ToDbDate() + "' and '" + toDate.ToDbDate() + @"'
                        --WHERE (Month(TYP.StartDate) >= Month('" + fromDate.ToDbDate() + "') AND Year(TYP.StartDate) >= Year('" + fromDate.ToDbDate() + "')) AND (Month(TYP.EndDate) <= Month('" + toDate.ToDbDate() + "') and Year(TYP.EndDate) <= Year('" + toDate.ToDbDate() + @"'))";
                DataTable dtTax = _sqlRepository.GetDataTable(sql);
                taxYearId = "''";
                if (dtTax.Rows.Count > 0)
                {

                    for (int i = 0; i < dtTax.Rows.Count; i++)
                    {
                        taxYearId += ",'" + dtTax.Rows[i]["TaxCodeYearId"].ToString() + "'";
                    }
                }

                return taxYearId;




            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }
        private DataTable GetGSTPayableSalesSQL3(string companyGroupId, string companyId, string plantId, string plantName, string fromDate, string toDate, string taxyearId)
        {
            string strSql = "";
            strSql = @"SELECT	x.SourceType,x.VoucherNo,x.VoucherDate,x.PostingDate,x.DocRefNo,x.DocDate,x.PartyName,x.PartyPlantName,x.GSTIN
		            ,x.TaxCategoryType,x.TaxCode--,x.TaxPercentage
		            ,SUM(x.TaxableAmount) TaxableAmount,SUM(x.DrAmount) DrAmount,SUM(x.CrAmount) CrAmount
		            ,x.TCSequence,x.EntryDate,x.GRNNo
                    ,x.PlaceofSupply,x.ReverseCharge,x.Suppliesundersection7ofIGSTAct,x.InvoiceType,x.ECommerceGSTIN
					,x.ItemName,x.HSNSAC,x.Rate,x.CessAmount,x.ApplicableofTaxRate

		            FROM 
                      (
						            SELECT  'Sales' SourceType
                            ,V.VoucherNo,format( V.PostingDate,'dd-MMM-yyyy')PostingDate, V.DocRefNo,format (V.DocDate,'dd-MMM-yyyy')DocDate
							,P.UserName PartyName,PP.GSTIN
							,NULL GRNNo,pp.UserName PartyPlantName
                            ,LineItemType=case   WHEN v.SourceType='CustomerInvoice' THEN 'GL'
                            ELSE '' END
                            --,Particular=CASE WHEN v.SourceType='VendorInvoice' THEN A.UserName
                            --WHEN v.SourceType='VendorPayment' THEN AP.UserName
                            --ELSE '' END
                            ,TaxableAmount=case when v.SourceType='CustomerInvoice' then ISNULL(VD.CrAmount,0) else 0 end
                            ,0 DrAmount
                            ,CrAmount=case when ITD.AType='Cr' then ISNULL(IT.TaxAmount,0) else 0 end
							
	                        ,format( v.VoucherDate,'dd-MMM-yyyy')VoucherDate
                            ,TC.TaxCategoryType,TC.Code TaxCode,TC.Sequence TCSequence,TC.UserName+'-'+TC.Code TaxCategory,IsNULL(TAXC.IsRCM,0) IsRCM
							
                            ,IsNULL(IV.IsExcludingTax,0) IsExcludingTax,0 IsTaxApplicable,TAXC.[Type],TAXC.ValueOfFixed
                            ,0 [Percentage],NULL HSNCodeId,NULL Material
							,TaxPercentage= case when v.SourceType='CustomerInvoice' then taxc.ValueOfFixed
												  else 0 end
							, Format (IT.AddedDate,'dd-MMM-yyyy')EntryDate
                            ,NULL PlaceofSupply,0 ReverseCharge,NULL Suppliesundersection7ofIGSTAct,NULL InvoiceType,NULL ECommerceGSTIN
                            ,NULL ItemName,NULL HSNSAC,0 Rate,0 CessAmount,0 ApplicableofTaxRate

                            from TRN.InvoiceTax IT
                            left join TRN.InvoiceTaxDetail ITD ON IT.Id=ITD.InvoiceTaxId AND ITD.AType='Cr'
                            LEFT JOIN TRN.Voucher V ON V.Id=IT.VoucherId
                            LEFT JOIN TRN.Invoice IV ON IV.Id=IT.InvoiceId
                            LEFT JOIN HKP.Activity TA ON TA.Id=ITD.ActivityId
                            LEFT JOIN HKP.Party P ON P.Id=IT.PartyId
							Left join hkp.PartyPlant PP on PP.Id=IT.PartyPlantId
                            LEFT JOIN MST.TaxCategory TC ON TC.Id=IT.TaxCategoryId
                            LEFT JOIN( select TAC.Id,TAC.UserName,TAC.IsRCM,TAY.[Type],TACD.ValueOfFixed from MST.TaxCode TAC
                            LEFT JOIN MST.TaxCodeYear TAY ON TAY.TaxCodeId=TAC.Id
                            LEFT JOIN MST.TaxCodeDetail TACD ON TACD.TaxCodeId=TAC.Id WHERE TAY.TaxYearId IN (" + taxyearId + @")) TAXC ON TAXC.Id=IT.TaxCodeId
                            LEFT JOIN TRN.VoucherDetail VD ON VD.Id=IT.VoucherDetailId
                            LEFT JOIN HKP.Activity A ON A.Id=VD.ActivityId
                            LEFT JOIN (SELECT IW.InvoiceWriteOffId,IW.ActivityId,SUM(I.Amount) Amount FROM TRN.InvoiceWriteOffDetail IW
                            JOIN TRN.Invoice I ON I.Id=IW.InvoiceId
                            GROUP BY InvoiceWriteOffId,ActivityId) IWD ON IWD.InvoiceWriteOffId=IT.InvoiceWriteOffId
                            LEFT JOIN HKP.Activity AP ON AP.Id=IWD.ActivityId
                            where TC.TaxCategoryType='GST' AND TAXC.IsRCM=0 AND  V.IsPark=0 AND V.PlantId='" + plantId + @"'
							and V.PostingDate between '" + fromDate + "' AND '" + toDate + @"'
                            AND v.SourceType IN ('CustomerInvoice')
                            
                            UNION all

						SELECT 'Sales' SourceType
                            ,V.VoucherNo,format( V.PostingDate,'dd-MMM-yyyy')PostingDate, V.DocRefNo,format (V.DocDate,'dd-MMM-yyyy')DocDate,P.UserName PartyName,PP.GSTIN
							, IRD.InventorySalesId SalesNo,pp.UserName PartyPlantName
                            ,LineItemType=case when v.SourceType='SalesInvoice' then 'Sales'
                            ELSE '' END
                            
                            ,TaxableAmount=case when v.SourceType='SalesInvoice' then ISNULL(IRD.PolicyAmount,0)
                            else 0 end
                            ,0 DrAmount
                            ,CrAmount=case when ITD.AType='Cr' then ISNULL(IRT.TaxAmount,0) else 0 end
	                        ,format( v.VoucherDate,'dd-MMM-yyyy')VoucherDate
                            ,TC.TaxCategoryType,TC.Code TaxCode,TC.Sequence TCSequence,TC.UserName+'-'+TC.Code TaxCategory
							,IsNULL(TAXC.IsRCM,0) IsRCM
                            ,IsNULL(IV.IsExcludingTax,0) IsExcludingTax,IsTaxApplicable=CASE WHEN IsNULL(CP.TaxApplicable,'')='Mandatory' THEN 1 ELSE 0 END 
							,TAXC.[Type],TAXC.ValueOfFixed
                            ,IRT.[Percentage],NULL HSNCodeId,null Material
							,TaxPercentage= case  when v.SourceType='SalesInvoice' AND IRT.[Percentage]>0 THEN IRT.[Percentage]
												 else 0 end
							,it.AddedDate EntryDate
                            ,NULL PlaceofSupply,0 ReverseCharge,NULL Suppliesundersection7ofIGSTAct,NULL InvoiceType,NULL ECommerceGSTIN
                            ,NULL ItemName,NULL HSNSAC,0 Rate,0 CessAmount,0 ApplicableofTaxRate

                            from TRN.InvoiceTax IT
                            left join TRN.InvoiceTaxDetail ITD ON IT.Id=ITD.InvoiceTaxId AND ITD.AType='Cr'
                            LEFT JOIN TRN.Voucher V ON V.Id=IT.VoucherId
                            LEFT JOIN TRN.Invoice IV ON IV.Id=IT.InvoiceId
                            --LEFT JOIN TRN.InvoiceWriteOff IW ON IW.Id=IT.InvoiceWriteOffId
                            LEFT JOIN HKP.Activity TA ON TA.Id=ITD.ActivityId
                            LEFT JOIN HKP.Party P ON P.Id=IT.PartyId
							
                            LEFT JOIN MST.TaxCategory TC ON TC.Id=IT.TaxCategoryId
                            LEFT JOIN( select TAC.Id,TAC.UserName,TAC.IsRCM,TAY.[Type],TACD.ValueOfFixed from MST.TaxCode TAC
                            LEFT JOIN MST.TaxCodeYear TAY ON TAY.TaxCodeId=TAC.Id
                            LEFT JOIN MST.TaxCodeDetail TACD ON TACD.TaxCodeId=TAC.Id WHERE TAY.TaxYearId IN (" + taxyearId + @")
							--and tac.IsRCM=0
							) TAXC ON TAXC.Id=IT.TaxCodeId
                            --LEFT JOIN SCS.TaxYear TY ON TY.Id=TAY.TaxYearId
                            LEFT JOIN TRN.InventorySales IR ON IR.VoucherId=V.Id
                            LEFT JOIN TRN.InventorySalesDetail IRD ON IRD.InventorySalesId=IR.Id
                            LEFT JOIN TRN.InventorySalesTax IRT ON IRD.Id=IRT.InventorySalesDetailId AND IRT.TaxCategoryId=IT.TaxCategoryId
                            --LEFT JOIN MST.HSNTaxPercentage HSNP ON IRT.HSNCodeId=HSNP.HSNCodeId AND HSNP.TaxCategoryId=IT.TaxCategoryId
                            LEFT JOIN TRN.InventoryMaterial IM ON IM.Id=IRD.InventoryMaterialId
                            LEFT JOIN MST.MaterialMaster MM ON MM.Id=IM.MaterialMasterId
                            LEFT JOIN TRN.VoucherDetail VD ON VD.Id=IT.VoucherDetailId
                            LEFT JOIN HKP.Activity A ON A.Id=VD.ActivityId
                            Left join hkp.PartyPlant pp on pp.Id=IR.InvoicingPartyPlantId
							LEFT JOIN HKP.CompanyParty CP ON CP.PartyId=P.Id AND CP.PartyType='Customer' AND CP.PlantId = '" + plantId + @"'
                            where TC.TaxCategoryType='GST' AND (CP.TaxApplicable IS NULL OR CP.TaxApplicable ='Optional') AND V.IsPark=0
							AND IR.PlantId = '" + plantId + @"' and V.PostingDate between '" + fromDate + "' AND '" + toDate + @"'
                            AND v.SourceType='SalesInvoice' and IRT.InventorySalesServiceId IS NULL


                            
                            
							union all
							SELECT 'Sales' SourceType
                            ,V.VoucherNo,format( V.PostingDate,'dd-MMM-yyyy')PostingDate, V.DocRefNo,format (V.DocDate,'dd-MMM-yyyy')DocDate,P.UserName PartyName,PP.GSTIN
							, IRD.InventorySalesId GRNNo,pp.UserName PartyPlantName
                            ,LineItemType=case when v.SourceType='SalesInvoice' then 'Sales'
                            ELSE '' END
                            
                            ,TaxableAmount=case when v.SourceType='SalesInvoice' then ISNULL(IRD.TotalSalesAmount,0)
                            else 0 end
                            ,0 DrAmount
                            ,CrAmount=case when ITD.AType='Cr' then ISNULL(IRT.TaxAmount,0) else 0 end
	                        ,format( v.VoucherDate,'dd-MMM-yyyy')VoucherDate
                            ,TC.TaxCategoryType,TC.Code TaxCode,TC.Sequence TCSequence,TC.UserName+'-'+TC.Code TaxCategory
							,IsNULL(TAXC.IsRCM,0) IsRCM
                            ,IsNULL(IV.IsExcludingTax,0) IsExcludingTax,IsTaxApplicable=CASE WHEN IsNULL(CP.TaxApplicable,'')='Mandatory' THEN 1 ELSE 0 END 
							,TAXC.[Type],TAXC.ValueOfFixed
                            ,IRT.[Percentage],NULL HSNCodeId,null Material
							,TaxPercentage= case  when v.SourceType='SalesInvoice' AND IRT.[Percentage]>0 THEN IRT.[Percentage]
												 else 0 end
							,it.AddedDate EntryDate
                            ,NULL PlaceofSupply,0 ReverseCharge,NULL Suppliesundersection7ofIGSTAct,NULL InvoiceType,NULL ECommerceGSTIN
                            ,NULL ItemName,NULL HSNSAC,0 Rate,0 CessAmount,0 ApplicableofTaxRate

                            FROM TRN.InvoiceTax IT
                            left join TRN.InvoiceTaxDetail ITD ON IT.Id=ITD.InvoiceTaxId AND ITD.AType='Cr'
                            LEFT JOIN TRN.Voucher V ON V.Id=IT.VoucherId
                            LEFT JOIN TRN.Invoice IV ON IV.Id=IT.InvoiceId
                            LEFT JOIN HKP.Activity TA ON TA.Id=ITD.ActivityId
                            LEFT JOIN HKP.Party P ON P.Id=IT.PartyId
							
                            LEFT JOIN MST.TaxCategory TC ON TC.Id=IT.TaxCategoryId
                            LEFT JOIN( select distinct TAC.Id,TAC.UserName,TAC.IsRCM,TAY.[Type],TACD.ValueOfFixed from MST.TaxCode TAC
                            LEFT JOIN MST.TaxCodeYear TAY ON TAY.TaxCodeId=TAC.Id
                            LEFT JOIN MST.TaxCodeDetail TACD ON TACD.TaxCodeId=TAC.Id WHERE TAY.TaxYearId IN (" + taxyearId + @") 
							) TAXC ON TAXC.Id=IT.TaxCodeId
                            LEFT JOIN TRN.InventorySales IR ON IR.VoucherId=V.Id
                            LEFT JOIN TRN.InventorySalesDetail IRD ON IRD.InventorySalesId=IR.Id
                            LEFT JOIN TRN.InventorySalesTax IRT ON IRD.Id=IRT.InventorySalesServiceId AND IRT.TaxCategoryId=IT.TaxCategoryId
                            LEFT JOIN TRN.InventoryMaterial IM ON IM.Id=IRD.InventoryMaterialId
                            LEFT JOIN MST.MaterialMaster MM ON MM.Id=IM.MaterialMasterId
                            LEFT JOIN TRN.VoucherDetail VD ON VD.Id=IT.VoucherDetailId
                            LEFT JOIN HKP.Activity A ON A.Id=VD.ActivityId
                            Left join hkp.PartyPlant pp on pp.Id=IR.InvoicingPartyPlantId
							LEFT JOIN HKP.CompanyParty CP ON CP.PartyId=P.Id AND CP.PartyType='Customer' AND CP.PlantId = '" + plantId + @"'
                            where TC.TaxCategoryType='GST' AND (CP.TaxApplicable IS NULL OR CP.TaxApplicable ='Optional') AND V.IsPark=0
							AND IR.PlantId = '" + plantId + @"' and V.PostingDate between '" + fromDate + "' AND '" + toDate + @"'
                            AND v.SourceType='SalesInvoice' and IRT.InventorySalesDetailId IS NULL

							UNION All

								SELECT 'SalesService' SourceType
                            ,V.VoucherNo,format( V.PostingDate,'dd-MMM-yyyy')PostingDate, V.DocRefNo,format (V.DocDate,'dd-MMM-yyyy')DocDate,P.UserName PartyName,PP.GSTIN
							, IRD.InventorySalesId SalesNo,pp.UserName PartyPlantName
                            ,LineItemType=case when v.SourceType='SalesInvoice' then 'Material'
                            ELSE '' END
                            
                            ,TaxableAmount=case when v.SourceType='SalesInvoice' then ISNULL(IRD.Amount,0)
                            else 0 end
                            ,0 DrAmount
                            ,CrAmount=case when ITD.AType='Cr' then ISNULL(IRT.TaxAmount,0) else 0 end
	                        ,format( v.VoucherDate,'dd-MMM-yyyy')VoucherDate
                            ,TC.TaxCategoryType,TC.Code TaxCode,TC.Sequence TCSequence,TC.UserName+'-'+TC.Code TaxCategory
							,IsNULL(TAXC.IsRCM,0) IsRCM
                            ,IsNULL(IV.IsExcludingTax,0) IsExcludingTax,IsTaxApplicable=CASE WHEN IsNULL(CP.TaxApplicable,'')='Mandatory' THEN 1 ELSE 0 END 
							,TAXC.[Type],TAXC.ValueOfFixed
                            ,IRT.[Percentage],NULL HSNCodeId,null Material
							,TaxPercentage= case  when v.SourceType='SalesInvoice' AND IRT.[Percentage]>0 THEN IRT.[Percentage]
												 else 0 end
							,it.AddedDate EntryDate
                            ,NULL PlaceofSupply,0 ReverseCharge,NULL Suppliesundersection7ofIGSTAct,NULL InvoiceType,NULL ECommerceGSTIN
                            ,NULL ItemName,NULL HSNSAC,0 Rate,0 CessAmount,0 ApplicableofTaxRate

                            from TRN.InvoiceTax IT
                            left join TRN.InvoiceTaxDetail ITD ON IT.Id=ITD.InvoiceTaxId AND ITD.AType='Cr'
                            LEFT JOIN TRN.Voucher V ON V.Id=IT.VoucherId
                            LEFT JOIN TRN.Invoice IV ON IV.Id=IT.InvoiceId
                            --LEFT JOIN TRN.InvoiceWriteOff IW ON IW.Id=IT.InvoiceWriteOffId
                            LEFT JOIN HKP.Activity TA ON TA.Id=ITD.ActivityId
                            LEFT JOIN HKP.Party P ON P.Id=IT.PartyId
							
                            LEFT JOIN MST.TaxCategory TC ON TC.Id=IT.TaxCategoryId
                            LEFT JOIN( select TAC.Id,TAC.UserName,TAC.IsRCM,TAY.[Type],TACD.ValueOfFixed from MST.TaxCode TAC
                            LEFT JOIN MST.TaxCodeYear TAY ON TAY.TaxCodeId=TAC.Id
                            LEFT JOIN MST.TaxCodeDetail TACD ON TACD.TaxCodeId=TAC.Id WHERE TAY.TaxYearId IN (" + taxyearId + @") 
							--and tac.IsRCM=0
							) TAXC ON TAXC.Id=IT.TaxCodeId
                            --LEFT JOIN SCS.TaxYear TY ON TY.Id=TAY.TaxYearId
                            LEFT JOIN TRN.InventorySales IR ON IR.InventoryVoucherId=V.Id
                            LEFT JOIN TRN.InventorySalesService IRD ON IRD.InventorySalesId=IR.Id
                            LEFT JOIN TRN.InventorySalesTax IRT ON IRD.Id=IRT.InventorySalesServiceId AND IRT.TaxCategoryId=IT.TaxCategoryId
                            --LEFT JOIN MST.HSNTaxPercentage HSNP ON IRT.HSNCodeId=HSNP.HSNCodeId AND HSNP.TaxCategoryId=IT.TaxCategoryId
                            LEFT JOIN HKP.ServiceMaster MM ON MM.Id=IRD.ServiceMasterId
                            LEFT JOIN TRN.VoucherDetail VD ON VD.Id=IT.VoucherDetailId
                            LEFT JOIN HKP.Activity A ON A.Id=VD.ActivityId
                            Left join hkp.PartyPlant pp on pp.Id=IR.InvoicingPartyPlantId
							LEFT JOIN HKP.CompanyParty CP ON CP.PartyId=P.Id AND CP.PartyType='Customer' AND CP.PlantId = '" + plantId + @"'
                            where TC.TaxCategoryType='GST' AND (CP.TaxApplicable IS NULL OR CP.TaxApplicable ='Optional') AND V.IsPark=0
							AND IR.PlantId = '" + plantId + @"' and V.PostingDate between '" + fromDate + "' AND '" + toDate + @"'
                            AND v.SourceType='SalesInvoice' and isnull(IRT.InventorySalesDetailId,'') IS NULL
							
					

							UNION ALL

						
				SELECT 'Sales' SourceType
                            ,V.VoucherNo,format( V.PostingDate,'dd-MMM-yyyy')PostingDate, V.DocRefNo,format (V.DocDate,'dd-MMM-yyyy')DocDate,P.UserName PartyName,PP.GSTIN
							, IRD.SalesId GRNNo,pp.UserName PartyPlantName
                            ,LineItemType=case when v.SourceType='SalesInvoice' then 'Material'
                            ELSE '' END
                            
                            ,TaxableAmount=case when v.SourceType='SalesInvoice' then ISNULL(IRD.TransactionAmount,0)
                            else 0 end
                            ,0 DrAmount,CrAmount=case when ITD.AType='Cr' then ISNULL(IRT.Amount,0) else 0 end
	                        ,format( v.VoucherDate,'dd-MMM-yyyy')VoucherDate
                            ,TC.TaxCategoryType,TC.Code TaxCode,TC.Sequence TCSequence,TC.UserName+'-'+TC.Code TaxCategory
							,IsNULL(TAXC.IsRCM,0) IsRCM
                            ,IsNULL(IV.IsExcludingTax,0) IsExcludingTax,IsTaxApplicable=CASE WHEN IsNULL(CP.TaxApplicable,'')='Mandatory' THEN 1 ELSE 0 END 
							,TAXC.[Type],TAXC.ValueOfFixed
                            ,IRT.[Percentage],NULL HSNCodeId,null Material
							,TaxPercentage= case  when v.SourceType='SalesInvoice' AND IRT.[Percentage]>0 THEN IRT.[Percentage]
												 else 0 end
							,it.AddedDate EntryDate
                            ,NULL PlaceofSupply,0 ReverseCharge,NULL Suppliesundersection7ofIGSTAct,NULL InvoiceType,NULL ECommerceGSTIN
                            ,NULL ItemName,NULL HSNSAC,0 Rate,0 CessAmount,0 ApplicableofTaxRate

                            from TRN.InvoiceTax IT
                            left join TRN.InvoiceTaxDetail ITD ON IT.Id=ITD.InvoiceTaxId AND ITD.AType='Cr'
                            LEFT JOIN TRN.Voucher V ON V.Id=IT.VoucherId
                            LEFT JOIN TRN.Invoice IV ON IV.Id=IT.InvoiceId
                            --LEFT JOIN TRN.InvoiceWriteOff IW ON IW.Id=IT.InvoiceWriteOffId
                            LEFT JOIN HKP.Activity TA ON TA.Id=ITD.ActivityId
                            LEFT JOIN HKP.Party P ON P.Id=IT.PartyId
							
                            LEFT JOIN MST.TaxCategory TC ON TC.Id=IT.TaxCategoryId
                            LEFT JOIN( select TAC.Id,TAC.UserName,TAC.IsRCM,TAY.[Type],TACD.ValueOfFixed from MST.TaxCode TAC
                            LEFT JOIN MST.TaxCodeYear TAY ON TAY.TaxCodeId=TAC.Id
                            LEFT JOIN MST.TaxCodeDetail TACD ON TACD.TaxCodeId=TAC.Id WHERE TAY.TaxYearId IN (" + taxyearId + @") 
							--and tac.IsRCM=0
							) TAXC ON TAXC.Id=IT.TaxCodeId
                            --LEFT JOIN SCS.TaxYear TY ON TY.Id=TAY.TaxYearId
                            LEFT JOIN TRN.Sales IR ON IR.VoucherId=V.Id
                           LEFT JOIN TRN.SalesMaterial IRD ON IRD.SalesId=IR.Id
							  -- LEFT JOIN TRN.InventorySalesDetail IRD ON IRD.InventorySalesId=IR.Id
                            LEFT JOIN TRN.SalesTax IRT ON IRD.Id=IRT.SalesMaterialId AND IRT.TaxCategoryId=IT.TaxCategoryId
                            --LEFT JOIN MST.HSNTaxPercentage HSNP ON IRT.HSNCodeId=HSNP.HSNCodeId AND HSNP.TaxCategoryId=IT.TaxCategoryId
                           -- LEFT JOIN TRN.InventoryMaterial IM ON IM.Id=IRD.InventoryMaterialId
                            LEFT JOIN MST.MaterialMaster MM ON MM.Id=IRD.MaterialMasterId
                            LEFT JOIN TRN.VoucherDetail VD ON VD.Id=IT.VoucherDetailId
                            LEFT JOIN HKP.Activity A ON A.Id=VD.ActivityId
                            Left join hkp.PartyPlant pp on pp.Id=IR.InvoicingPartyPlantId
							LEFT JOIN HKP.CompanyParty CP ON CP.PartyId=P.Id AND CP.PartyType='Customer' AND CP.PlantId = '" + plantId + @"'
                            where TC.TaxCategoryType='GST' AND (CP.TaxApplicable IS NULL OR CP.TaxApplicable ='Optional') AND V.IsPark=0
							AND IR.PlantId = '" + plantId + @"' and V.PostingDate between '" + fromDate + "' AND '" + toDate + @"'
                            AND v.SourceType='SalesInvoice' and IRT.SalesServiceId IS NULL
  ) x
							group by x.VoucherNo,x.VoucherDate,x.PostingDate,x.DocRefNo,x.DocDate,x.PartyName
							,x.TCSequence,x.PartyPlantName,x.GSTIN,x.SourceType
							,x.TaxCategoryType,x.EntryDate,x.TaxCode,x.GRNNo
                            ,x.PlaceofSupply,x.ReverseCharge,x.Suppliesundersection7ofIGSTAct,x.InvoiceType,x.ECommerceGSTIN
					        ,x.ItemName,x.HSNSAC,x.Rate,x.CessAmount,x.ApplicableofTaxRate";

            return _sqlRepository.GetDataTable(strSql);

        }


        public IWorkbook GetGSTPayableSalesReport3(string companyGroupId, string companyId, string plantId, string plantName, string fromDate, string toDate, string name)
        {
            clsReport objRpt = null;
            clsReport objRptSR = null;
            string voucherNo = "";
            try
            {

                ExcelEngine excelEngine = null;
                IApplication application = null;
                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;
                excelEngine.Excel.DefaultVersion = ExcelVersion.Excel2013;
                var reportUtility = new ReportUtility();
                var workbook = reportUtility.GetWorkbook(ref excelEngine, 4);
                workbook.Version = ExcelVersion.Excel2013;
                var sheet1 = workbook.Worksheets[0];

                #region Logo
                string strPath = "";
                Image companyLogo = null;
                try
                {
                    DataTable dtCompanyImage = _sqlRepository.GetDataTable("SELECT * FROM ORG.COMPANY WHERE ID = '" + companyId + @"'");

                    strPath = Path.Combine(ResourcesPathReader.GetLogoOrImagePath(), dtCompanyImage.Rows[0]["Image"].ToString());  // IDCardEng.xlsx
                    companyLogo = Image.FromFile(strPath);
                }
                catch (Exception)
                {
                }
                #endregion
                objRpt = new clsReport();

                objRptSR = new clsReport(_sqlRepository);

                DataTable dtGStReceivableF3 = null;
                string taxyearId = GetTaxYearId3(fromDate, toDate, companyId);
                dtGStReceivableF3 = GetGSTPayableSalesSQL3(companyGroupId, companyId, plantId, plantName, fromDate, toDate, taxyearId);

                //string taxyearId = GetTaxYearId(fromDate, toDate, companyId);
                //dtRCMPayable = GetGSTPayableSQL(companyGroupId, companyId, plantId, plantName, fromDate, toDate, taxyearId);

                if (dtGStReceivableF3.Rows.Count == 0)
                {
                    throw new Exception("No Data Found....");
                }

                DataTable dtCmp = objRptSR.SelectedCompanyDT(plantId);

                DataTable dtFactory = objRptSR.SelectedPlantDT(plantId);

                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;

                int xlsRow = 1, xlsCol = 1;
                int endXlsCol = 1;
                string FactoryName = "";
                string CmpName = "";
                xlsRow = 6;
                sheet1.Range[xlsRow - 1, 1].Text = "Report Ref No:";
                sheet1.Range[xlsRow - 1, 1].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow - 1, 1].RowHeight = 20;
                sheet1.Range[xlsRow - 1, 1].CellStyle.Font.Bold = true;

                int iVoucherType = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Voucher Type";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                xlsCol++;


                int iPartyName = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Party Name";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 30;
                xlsCol++;

                //int iPartyPlantName = xlsCol;
                //sheet1.Range[xlsRow, xlsCol].Text = "Party Plant";
                //sheet1.Range[xlsRow, xlsCol].ColumnWidth = 25;
                //xlsCol++;

                int iGSTIN = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "GSTIN(Party Plant)";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 25;
                xlsCol++;

                int iPlaceofSupply = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Place of Supply";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 20;
                xlsCol++;

                int iReverseCharge = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Reverse Charge";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 25;
                xlsCol++;

                int iSuppliesundersection7ofIGSTAct = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Supplies under section 7 of IGST Act";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 30;
                xlsCol++;

                int iInvoiceType = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Invoice Type";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 20;
                xlsCol++;

                int iECommerceGSTIN = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "E-Commerce GSTIN";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 20;
                xlsCol++;

                int iItemName = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Item Name";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 35;
                xlsCol++;

                int iHSNSAC = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "HSN/SAC";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 25;
                xlsCol++;

                int iTaxableAmount = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Taxable Amount";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                xlsCol++;

                int iRate = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Rate";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                xlsCol++;

                int iCessAmount = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Cess Amount";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                xlsCol++;

                int iApplicableofTaxRate = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Applicable % of Tax Rate";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                xlsCol++;

                int iVoucherNo = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Voucher No";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                xlsCol++;

                int iEntryDate = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Entry Date";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                xlsCol++;

                int iPostingDate = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Posting Date";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                xlsCol++;

                int iDocRefNo = xlsCol; // Doc Ref
                sheet1.Range[xlsRow, xlsCol].Text = "DocRef No";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;

                xlsCol++;
                int iDocDate = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Doc Date";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;

                xlsCol++;
                int iGRNNo = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Invoice No";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;

                //xlsCol++;
                //int iTaxableAmount = xlsCol;
                //sheet1.Range[xlsRow, xlsCol].Text = "Taxable Amount";
                //sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                //sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignRight;
                DataTable dtTaxCode = null;
                dtGStReceivableF3.DefaultView.Sort = "TCSequence";
                dtTaxCode = dtGStReceivableF3.DefaultView.ToTable(true, "TaxCode");
                dtTaxCode.Columns.Add("ColumnNumber", typeof(String));
                dtTaxCode.Columns.Add("ColumnFormula", typeof(String));

                if (dtTaxCode.Rows.Count > 0)
                {
                    for (int i = 0; i < dtTaxCode.Rows.Count; i++)
                    {
                        xlsCol++;
                        sheet1.Range[xlsRow, xlsCol].Text = dtTaxCode.Rows[i]["TaxCode"].ToString();
                        sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                        dtTaxCode.Rows[i]["ColumnNumber"] = xlsCol.ToString();
                    }
                }
                xlsCol++;
                int iTotalTax = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Total Tax";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                xlsCol++;
                int iGrossAmount = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Gross Amount";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 18;
                sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignRight;
                endXlsCol = xlsCol;
                endXlsCol = xlsCol;

                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].WrapText = true;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 23;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;

                //string voucherNo = "";
                string Percentage = "";
                int startRow = 0;
                int perStartRow = 0;
                string formula = "";
                string formula2 = "";
                string totalFormula = "";

                string lineItemPercentageType = "";
                xlsRow++;
                startRow = xlsRow;
                perStartRow = xlsRow;
                bool isFirst = true;
                string totalTaxformula = "";
                string voucherNocomp = "";
                string taxFitler = "";
                for (int i = 0; i < dtGStReceivableF3.Rows.Count; i++)
                {

                    //if (dtGStReceivableF3.Rows[i]["LineItemType"].ToString().ToUpper() == "GL")
                    //{
                    //    voucherNocomp = dtGStReceivableF3.Rows[i]["VoucherNo"].ToString() + "-" + dtGStReceivableF3.Rows[i]["LineItemType"].ToString().ToUpper();
                    //    taxFitler = " and VoucherNo = '" + dtGStReceivableF3.Rows[i]["VoucherNo"].ToString() + "' and LineItemType = '" + dtGStReceivableF3.Rows[i]["LineItemType"].ToString() + "'";
                    //}
                    //if (dtGStReceivableF3.Rows[i]["LineItemType"].ToString().ToUpper() == "MATERIAL")
                    //{
                    //    voucherNocomp = dtGStReceivableF3.Rows[i]["VoucherNo"].ToString() + "-" + dtGStReceivableF3.Rows[i]["LineItemType"].ToString().ToUpper();

                    //    taxFitler = " and VoucherNo = '" + dtGStReceivableF3.Rows[i]["VoucherNo"].ToString() + "' and LineItemType = '" + dtGStReceivableF3.Rows[i]["LineItemType"].ToString() + "'";
                    //}
                    //if (dtGStReceivableF3.Rows[i]["LineItemType"].ToString().ToUpper() == "SERVICE")
                    //{
                    //    voucherNocomp = dtGStReceivableF3.Rows[i]["VoucherNo"].ToString() + "-" + dtGStReceivableF3.Rows[i]["LineItemType"].ToString().ToUpper();
                    //    taxFitler = " and VoucherNo = '" + dtGStReceivableF3.Rows[i]["VoucherNo"].ToString() + "' and LineItemType = '" + dtGStReceivableF3.Rows[i]["LineItemType"].ToString() + "'";
                    //}

                    voucherNocomp = dtGStReceivableF3.Rows[i]["VoucherNo"].ToString().ToUpper();
                    taxFitler = " and VoucherNo = '" + dtGStReceivableF3.Rows[i]["VoucherNo"].ToString() + "'";
                    if (voucherNo != voucherNocomp)
                    {

                        //if (dtGStReceivableF3.Rows[i]["LineItemType"].ToString().ToUpper() == "GL")
                        //{
                        //    lineItemPercentageType = "ValueOfFixed";
                        //}
                        //if (dtGStReceivableF3.Rows[i]["LineItemType"].ToString().ToUpper() == "MATERIAL")
                        //{
                        //    lineItemPercentageType = "Percentage";
                        //}
                        //if (dtGStReceivableF3.Rows[i]["LineItemType"].ToString().ToUpper() == "SERVICE")
                        //{
                        //    lineItemPercentageType = "Percentage";
                        //}
                        //if (Percentage != dtGStReceivableF3.Rows[i]["TaxPercentage"].ToString())
                        //{}
                        if (isFirst == false)
                        {

                            //sheet1[perStartRow, iCategory, xlsRow - 1, iCategory].BorderAround(ExcelLineStyle.Hair);
                            sheet1[perStartRow, iVoucherType, xlsRow - 1, iVoucherType].BorderAround(ExcelLineStyle.Hair);
                            sheet1[perStartRow, iPartyName, xlsRow - 1, iPartyName].BorderAround(ExcelLineStyle.Hair);
                            sheet1[perStartRow, iPostingDate, xlsRow - 1, iPostingDate].BorderAround(ExcelLineStyle.Hair);
                            sheet1[perStartRow, iGSTIN, xlsRow - 1, iGSTIN].BorderAround(ExcelLineStyle.Hair);
                            sheet1[perStartRow, iVoucherNo, xlsRow - 1, iVoucherNo].BorderAround(ExcelLineStyle.Hair);
                            sheet1[perStartRow, iEntryDate, xlsRow - 1, iEntryDate].BorderAround(ExcelLineStyle.Hair);
                            sheet1[perStartRow, iPostingDate, xlsRow - 1, iPostingDate].BorderAround(ExcelLineStyle.Hair);
                            sheet1[perStartRow, iDocRefNo, xlsRow - 1, iDocRefNo].BorderAround(ExcelLineStyle.Hair);
                            sheet1[perStartRow, iDocDate, xlsRow - 1, iDocDate].BorderAround(ExcelLineStyle.Hair);
                            sheet1[perStartRow, iGRNNo, xlsRow - 1, iGRNNo].BorderAround(ExcelLineStyle.Hair);
                            sheet1[perStartRow, iTaxableAmount, xlsRow - 1, iTaxableAmount].BorderAround(ExcelLineStyle.Hair);
                            sheet1[perStartRow, iTotalTax, xlsRow - 1, iTotalTax].BorderAround(ExcelLineStyle.Hair);
                            sheet1[perStartRow, iGrossAmount, xlsRow - 1, iGrossAmount].BorderAround(ExcelLineStyle.Hair);

                            formula = "SUM(" + clsStaticInfo.GetxlsCol(iTaxableAmount) + perStartRow + ":" + clsStaticInfo.GetxlsCol(iTaxableAmount) + (xlsRow - 1) + ")";
                            formula2 = "";

                            if (dtTaxCode.Rows.Count > 0)
                            {
                                totalTaxformula = "SUM(";
                                for (int j = 0; j < dtTaxCode.Rows.Count; j++)
                                {
                                    sheet1[perStartRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"]), xlsRow - 1, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])].BorderAround(ExcelLineStyle.Hair);
                                    formula2 = "SUM(" + clsStaticInfo.GetxlsCol(Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])) + perStartRow + ":" + clsStaticInfo.GetxlsCol(Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])) + (xlsRow - 1) + ")";
                                    sheet1[xlsRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"]), xlsRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])].Formula = formula2;

                                    dtTaxCode.Rows[j]["ColumnFormula"] += (clsStaticInfo.GetxlsCol(Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])) + xlsRow).ToString() + " + ";
                                    totalTaxformula += clsStaticInfo.GetxlsCol(Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])) + xlsRow + ":";
                                }
                            }
                            //sheet1.Range[xlsRow, 1, xlsRow, 1].Text = "Total";

                            //sheet1[xlsRow, iTaxableAmount, xlsRow, iTaxableAmount].Formula = formula;
                            //sheet1[xlsRow, iTotalTax, xlsRow, iTotalTax].Formula = totalTaxformula.Remove(totalTaxformula.Length - 1) + ")";

                            //sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Font.Bold = true;
                            //totalFormula += (clsStaticInfo.GetxlsCol(iTaxableAmount) + xlsRow).ToString() + "+";

                            //xlsRow++;
                        }
                        //xlsRow++;
                        //sheet1.Range[xlsRow - 1, 1].Number = clsStaticInfo.dbl(dtGStReceivableF3.Rows[i]["TaxPercentage"].ToString());
                        //sheet1.Range[xlsRow - 1, 1].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                        //perStartRow = xlsRow;
                        isFirst = false;


                        sheet1.Range[xlsRow, iVoucherType].Text = dtGStReceivableF3.Rows[i]["SourceType"].ToString();
                        sheet1.Range[xlsRow, iPartyName].Text = dtGStReceivableF3.Rows[i]["PartyName"].ToString();
                        //sheet1.Range[xlsRow, iPartyPlantName].Text = dtGStReceivableF3.Rows[i]["PartyPlantName"].ToString();
                        sheet1.Range[xlsRow, iGSTIN].Text = dtGStReceivableF3.Rows[i]["GSTIN"].ToString();
                        sheet1.Range[xlsRow, iPlaceofSupply].Text = dtGStReceivableF3.Rows[i]["PlaceofSupply"].ToString();
                        sheet1.Range[xlsRow, iReverseCharge].Text = dtGStReceivableF3.Rows[i]["ReverseCharge"].ToString();
                        sheet1.Range[xlsRow, iSuppliesundersection7ofIGSTAct].Text = dtGStReceivableF3.Rows[i]["Suppliesundersection7ofIGSTAct"].ToString();
                        sheet1.Range[xlsRow, iInvoiceType].Text = dtGStReceivableF3.Rows[i]["InvoiceType"].ToString();
                        sheet1.Range[xlsRow, iECommerceGSTIN].Text = dtGStReceivableF3.Rows[i]["ECommerceGSTIN"].ToString();
                        sheet1.Range[xlsRow, iItemName].Text = dtGStReceivableF3.Rows[i]["ItemName"].ToString();
                        sheet1.Range[xlsRow, iHSNSAC].Text = dtGStReceivableF3.Rows[i]["HSNSAC"].ToString();

                        sheet1.Range[xlsRow, iRate].Text = dtGStReceivableF3.Rows[i]["Rate"].ToString();
                        sheet1.Range[xlsRow, iRate].NumberFormat = "#,##0.00;(#,##0.00)";

                        sheet1.Range[xlsRow, iCessAmount].Text = dtGStReceivableF3.Rows[i]["CessAmount"].ToString();
                        sheet1.Range[xlsRow, iCessAmount].NumberFormat = "#,##0.00;(#,##0.00)";

                        sheet1.Range[xlsRow, iApplicableofTaxRate].Text = dtGStReceivableF3.Rows[i]["ApplicableofTaxRate"].ToString();
                        sheet1.Range[xlsRow, iApplicableofTaxRate].NumberFormat = "#,##0.00;(#,##0.00)";

                        sheet1.Range[xlsRow, iVoucherNo].Text = dtGStReceivableF3.Rows[i]["VoucherNo"].ToString();

                        sheet1.Range[xlsRow, iEntryDate].Text = dtGStReceivableF3.Rows[i]["EntryDate"].ToString();
                        sheet1.Range[xlsRow, iPostingDate].Text = clsStaticInfo.GetDateTaxFormate(dtGStReceivableF3.Rows[i]["PostingDate"].ToString());
                        sheet1.Range[xlsRow, iDocRefNo].Text = dtGStReceivableF3.Rows[i]["DocRefNo"].ToString();
                        sheet1.Range[xlsRow, iDocDate].Text = dtGStReceivableF3.Rows[i]["DocDate"].ToString();
                        sheet1.Range[xlsRow, iGRNNo].Text = dtGStReceivableF3.Rows[i]["GRNNo"].ToString();
                        //sheet1.Range[xlsRow, iTaxPercentage].Text = dtGStReceivableF3.Rows[i]["TaxPercentage"].ToString();
                        //sheet1.Range[xlsRow, iTotalTax].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                        sheet1.Range[xlsRow, iTotalTax].NumberFormat = "#,##0.00;(#,##0.00)";
                        //sheet1.Range[xlsRow, iGrossAmount].Number =clsStaticInfo.dbl( dtGStReceivableF3.Rows[i]["TaxableAmount"].ToString());


                        sheet1.Range[xlsRow, iTaxableAmount].Number = clsStaticInfo.dbl(dtGStReceivableF3.Rows[i]["TaxableAmount"].ToString());//TaxableAmount
                        //sheet1.Range[xlsRow, iTaxableAmount].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                        sheet1.Range[xlsRow, iTaxableAmount].NumberFormat = "#,##0.00;(#,##0.00)";
                        //dtRCMPayable.DefaultView.RowFilter = "VoucherNo = '" + dtRCMPayable.Rows[i]["VoucherNo"].ToString() + "'";

                        if (dtTaxCode.Rows.Count > 0)
                        {
                            totalTaxformula = "=SUM(";
                            for (int j = 0; j < dtTaxCode.Rows.Count; j++)
                            {
                                dtGStReceivableF3.DefaultView.RowFilter = "TaxCode = '" + dtTaxCode.Rows[j]["TaxCode"].ToString() + "'" + taxFitler;
                                if (dtGStReceivableF3.DefaultView.Count > 0)
                                {

                                    sheet1.Range[xlsRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])].Number = clsStaticInfo.dbl(dtGStReceivableF3.DefaultView[0]["CrAmount"].ToString());
                                    //sheet1.Range[xlsRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                                    sheet1.Range[xlsRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])].NumberFormat = "#,##0.00;(#,##0.00)";
                                }
                                else
                                {
                                    sheet1.Range[xlsRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])].Text = "-";
                                    sheet1.Range[xlsRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])].HorizontalAlignment = ExcelHAlign.HAlignRight;

                                }
                                totalTaxformula += clsStaticInfo.GetxlsCol(Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])) + xlsRow + ":";
                            }
                            sheet1.Range[xlsRow, iTotalTax].Formula = totalTaxformula.Remove(totalTaxformula.Length - 1) + ")";

                        }
                        sheet1.Range[xlsRow, iGrossAmount].Formula = clsStaticInfo.GetxlsCol(iTaxableAmount) + xlsRow + "+" + clsStaticInfo.GetxlsCol(iTotalTax) + xlsRow;
                        //sheet1.Range[xlsRow, iGrossAmount].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                        sheet1.Range[xlsRow, iGrossAmount].NumberFormat = "#,##0.00;(#,##0.00)";
                        //Percentage = dtGStReceivableF3.Rows[i]["TaxPercentage"].ToString();

                        xlsRow++;
                    }


                    //if (dtGStReceivableF3.Rows[i]["LineItemType"].ToString().ToUpper() == "GL")
                    //{
                    //    voucherNo = dtGStReceivableF3.Rows[i]["VoucherNo"].ToString() + "-" + dtGStReceivableF3.Rows[i]["LineItemType"].ToString().ToUpper();

                    //}
                    //if (dtGStReceivableF3.Rows[i]["LineItemType"].ToString().ToUpper() == "MATERIAL")
                    //{
                    //    voucherNo = dtGStReceivableF3.Rows[i]["VoucherNo"].ToString() + "-" + dtGStReceivableF3.Rows[i]["LineItemType"].ToString().ToUpper(); 

                    //}
                    //if (dtGStReceivableF3.Rows[i]["LineItemType"].ToString().ToUpper() == "SERVICE")
                    //{
                    //    voucherNo = dtGStReceivableF3.Rows[i]["VoucherNo"].ToString() + "-" + dtGStReceivableF3.Rows[i]["LineItemType"].ToString().ToUpper();

                    //}
                    voucherNo = dtGStReceivableF3.Rows[i]["VoucherNo"].ToString().ToUpper();


                }
                //sheet1[perStartRow, iCategory, xlsRow - 1, iCategory].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iVoucherType, xlsRow - 1, iVoucherType].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iPartyName, xlsRow - 1, iPartyName].BorderAround(ExcelLineStyle.Hair);
                //sheet1[perStartRow, iPartyPlantName, xlsRow - 1, iPartyPlantName].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iGSTIN, xlsRow - 1, iGSTIN].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iVoucherNo, xlsRow - 1, iVoucherNo].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iPostingDate, xlsRow - 1, iPostingDate].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iEntryDate, xlsRow - 1, iEntryDate].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iDocRefNo, xlsRow - 1, iDocRefNo].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iDocDate, xlsRow - 1, iDocDate].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iGRNNo, xlsRow - 1, iGRNNo].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iTaxableAmount, xlsRow - 1, iTaxableAmount].BorderAround(ExcelLineStyle.Hair);
                //sheet1[perStartRow, iTaxableAmount, xlsRow - 1, iTaxableAmount].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iTotalTax, xlsRow - 1, iTotalTax].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iGrossAmount, xlsRow - 1, iGrossAmount].BorderAround(ExcelLineStyle.Hair);


                if (dtTaxCode.Rows.Count > 0)
                {
                    for (int j = 0; j < dtTaxCode.Rows.Count; j++)
                    {
                        sheet1[perStartRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"]), xlsRow - 1, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])].BorderAround(ExcelLineStyle.Hair);

                    }
                }

                if (dtTaxCode.Rows.Count > 0)
                {
                    xlsRow++;
                    totalTaxformula = "=SUM(";
                    for (int j = 0; j < dtTaxCode.Rows.Count; j++)
                    {
                        sheet1[perStartRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"]), xlsRow - 1, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])].BorderAround(ExcelLineStyle.Hair);
                        formula2 = "SUM(" + clsStaticInfo.GetxlsCol(Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])) + perStartRow + ":" + clsStaticInfo.GetxlsCol(Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])) + (xlsRow - 1) + ")";
                        sheet1[xlsRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"]), xlsRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])].Formula = formula2;

                        dtTaxCode.Rows[j]["ColumnFormula"] += (clsStaticInfo.GetxlsCol(Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])) + xlsRow).ToString() + " + ";
                        totalTaxformula += clsStaticInfo.GetxlsCol(Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])) + xlsRow + ":";

                    }
                }


                sheet1.Range[xlsRow, 1, xlsRow, 1].Text = "Grand Total";
                formula = "SUM(" + clsStaticInfo.GetxlsCol(iTaxableAmount) + perStartRow + ":" + clsStaticInfo.GetxlsCol(iTaxableAmount) + (xlsRow - 1) + ")";



                sheet1[xlsRow, iTaxableAmount, xlsRow, iTaxableAmount].Formula = formula;
                sheet1[xlsRow, iGrossAmount, xlsRow, iGrossAmount].Formula = clsStaticInfo.GetxlsCol(iTaxableAmount) + xlsRow + "+" + clsStaticInfo.GetxlsCol(iTotalTax) + xlsRow;


                sheet1[xlsRow, iTotalTax, xlsRow, iTotalTax].Formula = totalTaxformula.Remove(totalTaxformula.Length - 1) + ")";

                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].NumberFormat = "#,##0.00;(#,##0.00)";
                totalFormula += (clsStaticInfo.GetxlsCol(iTaxableAmount) + xlsRow).ToString() + "+";


                //xlsRow++;
                //xlsRow++;


                //if (dtTaxCode.Rows.Count > 0)
                //{
                //    totalTaxformula = "=SUM(";
                //    for (int j = 0; j < dtTaxCode.Rows.Count; j++)
                //    {
                //        string fm = dtTaxCode.Rows[j]["ColumnFormula"].ToString().Trim();
                //        sheet1[xlsRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"]), xlsRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])].Formula = fm.Remove(fm.Length - 1); //dtTaxCode.Rows[j]["ColumnFormula"].ToString().Remove(dtTaxCode.Rows[j]["ColumnFormula"].ToString().Length - 1);
                //        totalTaxformula += clsStaticInfo.GetxlsCol(Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])) + xlsRow + ":";

                //    }
                //}
                //sheet1.Range[xlsRow, 1, xlsRow, 1].Text = "Grand Total";

                //sheet1[xlsRow, iTaxableAmount, xlsRow, iTaxableAmount].Formula = totalFormula.Remove(totalFormula.Length - 1);
                //sheet1[xlsRow, iTotalTax, xlsRow, iTotalTax].Formula = totalTaxformula.Remove(totalTaxformula.Length - 1) + ")";

                //sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Font.Bold = true;


                #region ******************Export******************
                var sheet2 = workbook.Worksheets[1];
                DataTable dtExport = null;
                dtExport = GetExportSQL(companyGroupId, companyId, plantId, plantName, fromDate, toDate, taxyearId);

                //string taxyearId = GetTaxYearId(fromDate, toDate, companyId);
                //dtRCMPayable = GetGSTPayableSQL(companyGroupId, companyId, plantId, plantName, fromDate, toDate, taxyearId);

                if (dtExport.Rows.Count == 0)
                {
                    // throw new Exception("No Data Found....");
                }


                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;

                int xlsRow2 = 1, xlsCol2 = 1;
                int endXlsCol2 = 1;
                //string FactoryName = "";
                //string CmpName = "";
                //xlsRow2 = 6;
                //sheet2.Range[xlsRow2 - 1, 1].Text = "Report Ref No:";
                //sheet2.Range[xlsRow2 - 1, 1].CellStyle.Font.Size = 10;
                //sheet2.Range[xlsRow2 - 1, 1].RowHeight = 20;
                //sheet2.Range[xlsRow2 - 1, 1].CellStyle.Font.Bold = true;

                int iExportType = xlsCol2;
                sheet2.Range[xlsRow2, xlsCol2].Text = "Export Type";
                sheet2.Range[xlsRow2, xlsCol2].ColumnWidth = 15;
                xlsCol2++;


                int iInvoiceNumber = xlsCol2;
                sheet2.Range[xlsRow2, xlsCol2].Text = "Invoice Number";
                sheet2.Range[xlsRow2, xlsCol2].ColumnWidth = 25;
                xlsCol2++;

                int iInvoiceDate = xlsCol2;
                sheet2.Range[xlsRow2, xlsCol2].Text = "Invoice Date";
                sheet2.Range[xlsRow2, xlsCol2].ColumnWidth = 25;
                xlsCol2++;

                int iInvoiceValue = xlsCol2;
                sheet2.Range[xlsRow2, xlsCol2].Text = "Invoice Value";
                sheet2.Range[xlsRow2, xlsCol2].ColumnWidth = 20;
                xlsCol2++;

                int iHSNSAC2 = xlsCol2;
                sheet2.Range[xlsRow2, xlsCol2].Text = "HSN/SAC";
                sheet2.Range[xlsRow2, xlsCol2].ColumnWidth = 25;
                xlsCol2++;

                int iPortCode = xlsCol2;
                sheet2.Range[xlsRow2, xlsCol2].Text = "Port Code";
                sheet2.Range[xlsRow2, xlsCol2].ColumnWidth = 30;
                xlsCol2++;

                int iShippingBillNumber = xlsCol2;
                sheet2.Range[xlsRow2, xlsCol2].Text = "Shipping Bill Number";
                sheet2.Range[xlsRow2, xlsCol2].ColumnWidth = 20;
                xlsCol2++;

                int iShippingBillDate = xlsCol2;
                sheet2.Range[xlsRow2, xlsCol2].Text = "Shipping Bill Date";
                sheet2.Range[xlsRow2, xlsCol2].ColumnWidth = 20;
                xlsCol2++;

                int iRate2 = xlsCol2;
                sheet2.Range[xlsRow2, xlsCol2].Text = "Rate";
                sheet2.Range[xlsRow2, xlsCol2].ColumnWidth = 35;
                xlsCol2++;

                int iTaxableValue = xlsCol2;
                sheet2.Range[xlsRow2, xlsCol2].Text = "Taxable Value";
                sheet2.Range[xlsRow2, xlsCol2].ColumnWidth = 25;
                xlsCol2++;

                int iCessAmount2 = xlsCol2;
                sheet2.Range[xlsRow2, xlsCol2].Text = "Cess Amount";
                sheet2.Range[xlsRow2, xlsCol2].ColumnWidth = 15;
                xlsCol2++;

                int iApplicableOfTaxRate = xlsCol2;
                sheet2.Range[xlsRow2, xlsCol2].Text = "Applicable % Of Tax Rate";
                sheet2.Range[xlsRow2, xlsCol2].ColumnWidth = 25;
                xlsCol2++;

                int iIGST = xlsCol2;
                sheet2.Range[xlsRow2, xlsCol2].Text = "IGST";
                sheet2.Range[xlsRow2, xlsCol2].ColumnWidth = 18;
                sheet2.Range[xlsRow2, xlsCol2].HorizontalAlignment = ExcelHAlign.HAlignRight;
                endXlsCol2 = xlsCol2;

                sheet2.Range[xlsRow2, 1, xlsRow2, endXlsCol2].BorderInside(ExcelLineStyle.Hair);
                sheet2.Range[xlsRow2, 1, xlsRow2, endXlsCol2].BorderAround(ExcelLineStyle.Hair);
                sheet2.Range[xlsRow2, 1, xlsRow2, endXlsCol2].WrapText = true;
                sheet2.Range[xlsRow2, 1, xlsRow2, endXlsCol2].CellStyle.Font.Bold = true;
                sheet2.Range[xlsRow2, 1, xlsRow2, endXlsCol2].RowHeight = 23;
                sheet2.Range[xlsRow2, 1, xlsRow2, endXlsCol2].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;

                //string voucherNo = "";
                //string Percentage = "";
                int startRow2 = 0;
                int perStartRow2 = 0;
                //string formula = "";
                //string formula2 = "";
                //string totalFormula = "";

                //string lineItemPercentageType = "";
                xlsRow2++;
                startRow2 = xlsRow2;
                perStartRow2 = xlsRow2;
                bool isSecond = true;
                //string totalTaxformula = "";
                //string voucherNocomp = "";
                //string taxFitler = "";
                for (int i = 0; i < dtExport.Rows.Count; i++)
                {

                    //voucherNocomp = dtExport.Rows[i]["VoucherNo"].ToString().ToUpper();
                    //taxFitler = " and VoucherNo = '" + dtExport.Rows[i]["VoucherNo"].ToString() + "'";
                    //if (voucherNo != voucherNocomp)
                    //{
                    if (isSecond == false)
                    {
                        sheet2[perStartRow2, iExportType, xlsRow2 - 1, iExportType].BorderAround(ExcelLineStyle.Hair);
                        sheet2[perStartRow2, iInvoiceNumber, xlsRow2 - 1, iInvoiceNumber].BorderAround(ExcelLineStyle.Hair);
                        sheet2[perStartRow2, iInvoiceDate, xlsRow2 - 1, iInvoiceDate].BorderAround(ExcelLineStyle.Hair);
                        sheet2[perStartRow2, iInvoiceValue, xlsRow2 - 1, iInvoiceValue].BorderAround(ExcelLineStyle.Hair);
                        sheet2[perStartRow2, iHSNSAC2, xlsRow2 - 1, iHSNSAC2].BorderAround(ExcelLineStyle.Hair);
                        sheet2[perStartRow2, iPortCode, xlsRow2 - 1, iPortCode].BorderAround(ExcelLineStyle.Hair);
                        sheet2[perStartRow2, iShippingBillNumber, xlsRow2 - 1, iShippingBillNumber].BorderAround(ExcelLineStyle.Hair);
                        sheet2[perStartRow2, iShippingBillDate, xlsRow2 - 1, iShippingBillDate].BorderAround(ExcelLineStyle.Hair);
                        sheet2[perStartRow2, iRate2, xlsRow2 - 1, iRate2].BorderAround(ExcelLineStyle.Hair);
                        sheet2[perStartRow2, iTaxableValue, xlsRow2 - 1, iTaxableValue].BorderAround(ExcelLineStyle.Hair);
                        sheet2[perStartRow2, iCessAmount2, xlsRow2 - 1, iCessAmount2].BorderAround(ExcelLineStyle.Hair);
                        sheet2[perStartRow2, iApplicableOfTaxRate, xlsRow2 - 1, iApplicableOfTaxRate].BorderAround(ExcelLineStyle.Hair);
                        sheet2[perStartRow2, iIGST, xlsRow2 - 1, iIGST].BorderAround(ExcelLineStyle.Hair);

                    }
                    isSecond = false;


                    sheet2.Range[xlsRow2, iExportType].Text = dtExport.Rows[i]["ExportType"].ToString();
                    sheet2.Range[xlsRow2, iInvoiceNumber].Text = dtExport.Rows[i]["InvoiceNumber"].ToString();

                    sheet2.Range[xlsRow2, iInvoiceDate].Text = dtExport.Rows[i]["InvoiceDate"].ToString();
                    sheet2.Range[xlsRow2, iInvoiceValue].Text = dtExport.Rows[i]["InvoiceValue"].ToString();
                    sheet2.Range[xlsRow2, iHSNSAC2].Text = dtExport.Rows[i]["HSNSAC"].ToString();
                    sheet2.Range[xlsRow2, iPortCode].Text = dtExport.Rows[i]["PostCode"].ToString();
                    sheet2.Range[xlsRow2, iShippingBillNumber].Text = dtExport.Rows[i]["ShippingBill"].ToString();
                    sheet2.Range[xlsRow2, iShippingBillDate].Text = dtExport.Rows[i]["ShippingDate"].ToString();

                    sheet2.Range[xlsRow2, iRate2].Text = dtExport.Rows[i]["Rate"].ToString();
                    sheet2.Range[xlsRow2, iRate2].NumberFormat = "#,##0.00;(#,##0.00)";

                    sheet2.Range[xlsRow2, iTaxableValue].Text = dtExport.Rows[i]["TaxableValue"].ToString();
                    sheet2.Range[xlsRow2, iTaxableValue].NumberFormat = "#,##0.00;(#,##0.00)";

                    sheet2.Range[xlsRow2, iCessAmount2].Text = dtExport.Rows[i]["CessAmount"].ToString();
                    sheet2.Range[xlsRow2, iCessAmount2].NumberFormat = "#,##0.00;(#,##0.00)";

                    sheet2.Range[xlsRow2, iApplicableofTaxRate].Text = dtExport.Rows[i]["ApplicableOfTaxRate"].ToString();
                    sheet2.Range[xlsRow2, iApplicableofTaxRate].NumberFormat = "#,##0.00;(#,##0.00)";

                    sheet2.Range[xlsRow2, iIGST].Number = clsStaticInfo.dbl(dtExport.Rows[i]["IGST"].ToString());
                    sheet2.Range[xlsRow2, iIGST].NumberFormat = "#,##0.00;(#,##0.00)";


                    xlsRow2++;
                    //}

                    //voucherNo = dtExport.Rows[i]["VoucherNo"].ToString().ToUpper();


                }

                if (dtExport.Rows.Count > 0)
                {
                    sheet2[perStartRow2, iExportType, xlsRow2 - 1, iExportType].BorderAround(ExcelLineStyle.Hair);
                    sheet2[perStartRow2, iInvoiceNumber, xlsRow2 - 1, iInvoiceNumber].BorderAround(ExcelLineStyle.Hair);

                    sheet2[perStartRow2, iInvoiceDate, xlsRow2 - 1, iInvoiceDate].BorderAround(ExcelLineStyle.Hair);
                    sheet2[perStartRow2, iInvoiceValue, xlsRow2 - 1, iInvoiceValue].BorderAround(ExcelLineStyle.Hair);
                    sheet2[perStartRow2, iHSNSAC2, xlsRow2 - 1, iHSNSAC2].BorderAround(ExcelLineStyle.Hair);
                    sheet2[perStartRow2, iPortCode, xlsRow2 - 1, iPortCode].BorderAround(ExcelLineStyle.Hair);
                    sheet2[perStartRow2, iShippingBillNumber, xlsRow2 - 1, iShippingBillNumber].BorderAround(ExcelLineStyle.Hair);
                    sheet2[perStartRow2, iShippingBillDate, xlsRow2 - 1, iShippingBillDate].BorderAround(ExcelLineStyle.Hair);
                    sheet2[perStartRow2, iRate2, xlsRow2 - 1, iRate2].BorderAround(ExcelLineStyle.Hair);
                    sheet2[perStartRow2, iTaxableValue, xlsRow2 - 1, iTaxableValue].BorderAround(ExcelLineStyle.Hair);

                    sheet2[perStartRow2, iCessAmount2, xlsRow2 - 1, iCessAmount2].BorderAround(ExcelLineStyle.Hair);
                    sheet2[perStartRow2, iApplicableOfTaxRate, xlsRow2 - 1, iApplicableOfTaxRate].BorderAround(ExcelLineStyle.Hair);
                    sheet2[perStartRow2, iIGST, xlsRow2 - 1, iIGST].BorderAround(ExcelLineStyle.Hair);
                }


                #endregion ******************Export******************


                #region ******************CDNR******************
                var sheet3 = workbook.Worksheets[2];
                DataTable dtCDNR = null;
                dtCDNR = GetCDNR();

                //string taxyearId = GetTaxYearId(fromDate, toDate, companyId);
                //dtRCMPayable = GetGSTPayableSQL(companyGroupId, companyId, plantId, plantName, fromDate, toDate, taxyearId);

                if (dtCDNR.Rows.Count == 0)
                {
                    throw new Exception("No Data Found....");
                }


                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;

                int xlsRow3 = 1, xlsCol3 = 1;
                int endXlsCol3 = 1;

                int iGSTINUINofRecipient = xlsCol3;
                sheet3.Range[xlsRow3, xlsCol3].Text = "GSTIN/UIN of Recipient";
                sheet3.Range[xlsRow3, xlsCol3].ColumnWidth = 22;
                xlsCol3++;


                int iNoteRefundVoucherNumber = xlsCol3;
                sheet3.Range[xlsRow3, xlsCol3].Text = "Note Refund Voucher Number";
                sheet3.Range[xlsRow3, xlsCol3].ColumnWidth = 30;
                xlsCol3++;

                int iNoteRefundVoucherDate = xlsCol3;
                sheet3.Range[xlsRow3, xlsCol3].Text = "Note Refund Voucher Date";
                sheet3.Range[xlsRow3, xlsCol3].ColumnWidth = 25;
                xlsCol3++;

                int iHSNSAC3 = xlsCol3;
                sheet3.Range[xlsRow3, xlsCol3].Text = "HSN/SAC";
                sheet3.Range[xlsRow3, xlsCol3].ColumnWidth = 20;
                xlsCol3++;

                int iNoteType = xlsCol3;
                sheet3.Range[xlsRow3, xlsCol3].Text = "Note Type";
                sheet3.Range[xlsRow3, xlsCol3].ColumnWidth = 30;
                xlsCol3++;

                int iPlaceOfSupply = xlsCol3;
                sheet3.Range[xlsRow3, xlsCol3].Text = "Place Of Supply";
                sheet3.Range[xlsRow3, xlsCol3].ColumnWidth = 20;
                xlsCol3++;

                int iNoteSupplyType = xlsCol3;
                sheet3.Range[xlsRow3, xlsCol3].Text = "Note Supply Type";
                sheet3.Range[xlsRow3, xlsCol3].ColumnWidth = 20;
                xlsCol3++;

                int iReverseCharge3 = xlsCol3;
                sheet3.Range[xlsRow3, xlsCol3].Text = "Reverse Charge";
                sheet3.Range[xlsRow3, xlsCol3].ColumnWidth = 35;
                xlsCol3++;

                int iNoteRefundVoucherValue = xlsCol3;
                sheet3.Range[xlsRow3, xlsCol3].Text = "Note Refund Voucher Value";
                sheet3.Range[xlsRow3, xlsCol3].ColumnWidth = 28;
                xlsCol3++;

                int iRate3 = xlsCol3;
                sheet3.Range[xlsRow3, xlsCol3].Text = "Rate";
                sheet3.Range[xlsRow3, xlsCol3].ColumnWidth = 15;
                xlsCol3++;

                int iTaxableValue3 = xlsCol3;
                sheet3.Range[xlsRow3, xlsCol3].Text = "Taxable Value";
                sheet3.Range[xlsRow3, xlsCol3].ColumnWidth = 15;
                xlsCol3++;

                int iCessAmount3 = xlsCol3;
                sheet3.Range[xlsRow3, xlsCol3].Text = "Cess Amount";
                sheet3.Range[xlsRow3, xlsCol3].ColumnWidth = 18;
                sheet3.Range[xlsRow3, xlsCol3].HorizontalAlignment = ExcelHAlign.HAlignRight;
                endXlsCol3 = xlsCol3;

                int iApplicableofTaxRate3 = xlsCol3;
                sheet3.Range[xlsRow3, xlsCol3].Text = "Applicable of Tax Rate";
                sheet3.Range[xlsRow3, xlsCol3].ColumnWidth = 18;
                sheet3.Range[xlsRow3, xlsCol3].HorizontalAlignment = ExcelHAlign.HAlignRight;
                endXlsCol3 = xlsCol3;

                int iIGST3 = xlsCol3;
                sheet3.Range[xlsRow3, xlsCol3].Text = "IGST";
                sheet3.Range[xlsRow3, xlsCol3].ColumnWidth = 18;
                sheet3.Range[xlsRow3, xlsCol3].HorizontalAlignment = ExcelHAlign.HAlignRight;
                endXlsCol3 = xlsCol3;

                int iCGST = xlsCol3;
                sheet3.Range[xlsRow3, xlsCol3].Text = "CGST";
                sheet3.Range[xlsRow3, xlsCol3].ColumnWidth = 18;
                sheet3.Range[xlsRow3, xlsCol3].HorizontalAlignment = ExcelHAlign.HAlignRight;
                endXlsCol3 = xlsCol3;

                int iSGST = xlsCol3;
                sheet3.Range[xlsRow3, xlsCol3].Text = "SGST";
                sheet3.Range[xlsRow3, xlsCol3].ColumnWidth = 18;
                sheet3.Range[xlsRow3, xlsCol3].HorizontalAlignment = ExcelHAlign.HAlignRight;
                endXlsCol3 = xlsCol3;

                int iTotalTax3 = xlsCol3;
                sheet3.Range[xlsRow3, xlsCol3].Text = "Total Tax";
                sheet3.Range[xlsRow3, xlsCol3].ColumnWidth = 18;
                sheet3.Range[xlsRow3, xlsCol3].HorizontalAlignment = ExcelHAlign.HAlignRight;
                endXlsCol3 = xlsCol3;

                sheet3.Range[xlsRow3, 1, xlsRow3, endXlsCol3].BorderInside(ExcelLineStyle.Hair);
                sheet3.Range[xlsRow3, 1, xlsRow3, endXlsCol3].BorderAround(ExcelLineStyle.Hair);
                sheet3.Range[xlsRow3, 1, xlsRow3, endXlsCol3].WrapText = true;
                sheet3.Range[xlsRow3, 1, xlsRow3, endXlsCol3].CellStyle.Font.Bold = true;
                sheet3.Range[xlsRow3, 1, xlsRow3, endXlsCol3].RowHeight = 23;
                sheet3.Range[xlsRow3, 1, xlsRow3, endXlsCol3].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;

                int startRow3 = 0;
                int perStartRow3 = 0;

                xlsRow3++;
                startRow3 = xlsRow3;
                perStartRow3 = xlsRow3;
                bool isThird = true;

                for (int i = 0; i < dtCDNR.Rows.Count; i++)
                {
                    if (isThird == false)
                    {
                        sheet3[perStartRow3, iGSTINUINofRecipient, xlsRow3 - 1, iGSTINUINofRecipient].BorderAround(ExcelLineStyle.Hair);
                        sheet3[perStartRow3, iNoteRefundVoucherNumber, xlsRow3 - 1, iNoteRefundVoucherNumber].BorderAround(ExcelLineStyle.Hair);
                        sheet3[perStartRow3, iNoteRefundVoucherDate, xlsRow3 - 1, iNoteRefundVoucherDate].BorderAround(ExcelLineStyle.Hair);
                        sheet3[perStartRow3, iHSNSAC3, xlsRow3 - 1, iHSNSAC3].BorderAround(ExcelLineStyle.Hair);
                        sheet3[perStartRow3, iNoteType, xlsRow3 - 1, iNoteType].BorderAround(ExcelLineStyle.Hair);
                        sheet3[perStartRow3, iPlaceOfSupply, xlsRow3 - 1, iPlaceOfSupply].BorderAround(ExcelLineStyle.Hair);
                        sheet3[perStartRow3, iNoteSupplyType, xlsRow3 - 1, iNoteSupplyType].BorderAround(ExcelLineStyle.Hair);
                        sheet3[perStartRow3, iReverseCharge3, xlsRow3 - 1, iReverseCharge3].BorderAround(ExcelLineStyle.Hair);
                        sheet3[perStartRow3, iNoteRefundVoucherValue, xlsRow3 - 1, iNoteRefundVoucherValue].BorderAround(ExcelLineStyle.Hair);
                        sheet3[perStartRow3, iRate3, xlsRow3 - 1, iRate3].BorderAround(ExcelLineStyle.Hair);
                        sheet3[perStartRow3, iTaxableValue3, xlsRow3 - 1, iTaxableValue3].BorderAround(ExcelLineStyle.Hair);
                        sheet3[perStartRow3, iCessAmount3, xlsRow3 - 1, iCessAmount].BorderAround(ExcelLineStyle.Hair);
                        sheet3[perStartRow3, iApplicableofTaxRate3, xlsRow3 - 1, iApplicableofTaxRate3].BorderAround(ExcelLineStyle.Hair);
                        sheet3[perStartRow3, iIGST3, xlsRow3 - 1, iIGST3].BorderAround(ExcelLineStyle.Hair);
                        sheet3[perStartRow3, iCGST, xlsRow3 - 1, iCGST].BorderAround(ExcelLineStyle.Hair);
                        sheet3[perStartRow3, iSGST, xlsRow3 - 1, iSGST].BorderAround(ExcelLineStyle.Hair);
                        sheet3[perStartRow3, iTotalTax3, xlsRow3 - 1, iTotalTax3].BorderAround(ExcelLineStyle.Hair);

                    }
                    isThird = false;


                    sheet3.Range[xlsRow3, iGSTINUINofRecipient].Text = dtCDNR.Rows[i]["GSTINUINofRecipient"].ToString();
                    sheet3.Range[xlsRow3, iNoteRefundVoucherNumber].Text = dtCDNR.Rows[i]["NoteRefundVoucherNumber"].ToString();

                    sheet3.Range[xlsRow3, iNoteRefundVoucherDate].Text = dtCDNR.Rows[i]["NoteRefundVoucherDate"].ToString();
                    sheet3.Range[xlsRow3, iHSNSAC3].Text = dtCDNR.Rows[i]["HSNSAC"].ToString();
                    sheet3.Range[xlsRow3, iNoteType].Text = dtCDNR.Rows[i]["NoteType"].ToString();
                    sheet3.Range[xlsRow3, iPlaceOfSupply].Text = dtCDNR.Rows[i]["PlaceOfSupply"].ToString();
                    sheet3.Range[xlsRow3, iNoteSupplyType].Text = dtCDNR.Rows[i]["NoteSupplyType"].ToString();

                    sheet3.Range[xlsRow3, iReverseCharge3].Text = dtCDNR.Rows[i]["ReverseCharge"].ToString();
                    sheet3.Range[xlsRow3, iReverseCharge3].NumberFormat = "#,##0.00;(#,##0.00)";

                    sheet3.Range[xlsRow3, iRate3].Text = dtCDNR.Rows[i]["Rate"].ToString();
                    sheet3.Range[xlsRow3, iRate3].NumberFormat = "#,##0.00;(#,##0.00)";

                    sheet3.Range[xlsRow3, iTaxableValue3].Text = dtCDNR.Rows[i]["TaxableValue"].ToString();
                    sheet3.Range[xlsRow3, iTaxableValue3].NumberFormat = "#,##0.00;(#,##0.00)";

                    sheet3.Range[xlsRow3, iCessAmount3].Text = dtCDNR.Rows[i]["CessAmount"].ToString();
                    sheet3.Range[xlsRow3, iCessAmount3].NumberFormat = "#,##0.00;(#,##0.00)";

                    sheet3.Range[xlsRow3, iApplicableofTaxRate3].Number = clsStaticInfo.dbl(dtCDNR.Rows[i]["ApplicableofTaxRate"].ToString());
                    sheet3.Range[xlsRow3, iApplicableofTaxRate3].NumberFormat = "#,##0.00;(#,##0.00)";

                    sheet3.Range[xlsRow3, iIGST3].Number = clsStaticInfo.dbl(dtCDNR.Rows[i]["IGST"].ToString());
                    sheet3.Range[xlsRow3, iIGST3].NumberFormat = "#,##0.00;(#,##0.00)";

                    sheet3.Range[xlsRow3, iCGST].Number = clsStaticInfo.dbl(dtCDNR.Rows[i]["CGST"].ToString());
                    sheet3.Range[xlsRow3, iCGST].NumberFormat = "#,##0.00;(#,##0.00)";

                    sheet3.Range[xlsRow3, iSGST].Number = clsStaticInfo.dbl(dtCDNR.Rows[i]["SGST"].ToString());
                    sheet3.Range[xlsRow3, iSGST].NumberFormat = "#,##0.00;(#,##0.00)";

                    sheet3.Range[xlsRow3, iTotalTax3].Number = clsStaticInfo.dbl(dtCDNR.Rows[i]["TotalTax"].ToString());
                    sheet3.Range[xlsRow3, iTotalTax3].NumberFormat = "#,##0.00;(#,##0.00)";

                    xlsRow3++;
                }

                sheet3[perStartRow3, iGSTINUINofRecipient, xlsRow3 - 1, iGSTINUINofRecipient].BorderAround(ExcelLineStyle.Hair);
                sheet3[perStartRow3, iNoteRefundVoucherNumber, xlsRow3 - 1, iNoteRefundVoucherNumber].BorderAround(ExcelLineStyle.Hair);

                sheet3[perStartRow3, iNoteRefundVoucherDate, xlsRow3 - 1, iNoteRefundVoucherDate].BorderAround(ExcelLineStyle.Hair);
                sheet3[perStartRow3, iHSNSAC3, xlsRow3 - 1, iHSNSAC3].BorderAround(ExcelLineStyle.Hair);
                sheet3[perStartRow3, iNoteType, xlsRow3 - 1, iNoteType].BorderAround(ExcelLineStyle.Hair);
                sheet3[perStartRow3, iPlaceOfSupply, xlsRow3 - 1, iPlaceOfSupply].BorderAround(ExcelLineStyle.Hair);
                sheet3[perStartRow3, iNoteSupplyType, xlsRow3 - 1, iNoteSupplyType].BorderAround(ExcelLineStyle.Hair);
                sheet3[perStartRow3, iReverseCharge3, xlsRow3 - 1, iReverseCharge3].BorderAround(ExcelLineStyle.Hair);
                sheet3[perStartRow3, iRate3, xlsRow3 - 1, iRate3].BorderAround(ExcelLineStyle.Hair);
                sheet3[perStartRow3, iTaxableValue3, xlsRow3 - 1, iTaxableValue3].BorderAround(ExcelLineStyle.Hair);

                sheet3[perStartRow3, iCessAmount3, xlsRow3 - 1, iCessAmount3].BorderAround(ExcelLineStyle.Hair);
                sheet3[perStartRow3, iApplicableofTaxRate3, xlsRow3 - 1, iApplicableofTaxRate3].BorderAround(ExcelLineStyle.Hair);
                sheet3[perStartRow3, iIGST3, xlsRow3 - 1, iIGST3].BorderAround(ExcelLineStyle.Hair);
                sheet3[perStartRow3, iCGST, xlsRow3 - 1, iCGST].BorderAround(ExcelLineStyle.Hair);
                sheet3[perStartRow3, iSGST, xlsRow3 - 1, iSGST].BorderAround(ExcelLineStyle.Hair);
                sheet3[perStartRow3, iTotalTax3, xlsRow3 - 1, iTotalTax3].BorderAround(ExcelLineStyle.Hair);


                #endregion ******************CDNR******************

                #region ******************HSN******************

                var sheet4 = workbook.Worksheets[3];
                DataTable dtHSN = null;
                dtHSN = GetHSN(fromDate, toDate, plantId);

                //string taxyearId = GetTaxYearId(fromDate, toDate, companyId);
                //dtRCMPayable = GetGSTPayableSQL(companyGroupId, companyId, plantId, plantName, fromDate, toDate, taxyearId);

                if (dtHSN.Rows.Count == 0)
                {
                    throw new Exception("No Data Found....");
                }


                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;

                int xlsRow4 = 1, xlsCol4 = 1;
                int endXlsCol4 = 1;


                int iHSN = xlsCol4;
                sheet4.Range[xlsRow4, xlsCol4].Text = "HSN";
                sheet4.Range[xlsRow4, xlsCol4].ColumnWidth = 15;
                xlsCol4++;


                int iDescription = xlsCol4;
                sheet4.Range[xlsRow4, xlsCol4].Text = "Description";
                sheet4.Range[xlsRow4, xlsCol4].ColumnWidth = 60;
                xlsCol4++;

                int iUQC = xlsCol4;
                sheet4.Range[xlsRow4, xlsCol4].Text = "UQC";
                sheet4.Range[xlsRow4, xlsCol4].ColumnWidth = 8;
                xlsCol4++;

                int iTotalQuantity = xlsCol4;
                sheet4.Range[xlsRow4, xlsCol4].Text = "Total Quantity";
                sheet4.Range[xlsRow4, xlsCol4].ColumnWidth = 15;
                xlsCol4++;

                int iTotalValue = xlsCol4;
                sheet4.Range[xlsRow4, xlsCol4].Text = "Total Value";
                sheet4.Range[xlsRow4, xlsCol4].ColumnWidth = 15;
                xlsCol4++;

                int iRate4 = xlsCol4;
                sheet4.Range[xlsRow4, xlsCol4].Text = "Rate";
                sheet4.Range[xlsRow4, xlsCol4].ColumnWidth = 10;
                xlsCol4++;

                int iTaxableValue4 = xlsCol4;
                sheet4.Range[xlsRow4, xlsCol4].Text = "Taxable Value";
                sheet4.Range[xlsRow4, xlsCol4].ColumnWidth = 16;
                xlsCol4++;

                int iIntegratedTaxAmount = xlsCol4;
                sheet4.Range[xlsRow4, xlsCol4].Text = "Integrated Tax Amount";
                sheet4.Range[xlsRow4, xlsCol4].ColumnWidth = 22;
                xlsCol4++;

                int iCentralTaxAmount = xlsCol4;
                sheet4.Range[xlsRow4, xlsCol4].Text = "Central Tax Amount";
                sheet4.Range[xlsRow4, xlsCol4].ColumnWidth = 20;
                xlsCol4++;

                int iStateUTTaxAmount = xlsCol4;
                sheet4.Range[xlsRow4, xlsCol4].Text = "State UT Tax Amount";
                sheet4.Range[xlsRow4, xlsCol4].ColumnWidth = 22;
                xlsCol4++;

                int iCessAmount4 = xlsCol4;
                sheet4.Range[xlsRow4, xlsCol4].Text = "Cess Amount";
                sheet4.Range[xlsRow4, xlsCol4].ColumnWidth = 15;
                endXlsCol4 = xlsCol4;

                sheet4.Range[xlsRow4, 1, xlsRow4, endXlsCol4].BorderInside(ExcelLineStyle.Hair);
                sheet4.Range[xlsRow4, 1, xlsRow4, endXlsCol4].BorderAround(ExcelLineStyle.Hair);
                sheet4.Range[xlsRow4, 1, xlsRow4, endXlsCol4].WrapText = true;
                sheet4.Range[xlsRow4, 1, xlsRow4, endXlsCol4].CellStyle.Font.Bold = true;
                sheet4.Range[xlsRow4, 1, xlsRow4, endXlsCol4].RowHeight = 23;
                sheet4.Range[xlsRow4, 1, xlsRow4, endXlsCol4].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;

                //string voucherNo = "";
                //string Percentage = "";
                int startRow4 = 0;
                int perStartRow4 = 0;
                //string formula = "";
                //string formula2 = "";
                //string totalFormula = "";

                //string lineItemPercentageType = "";
                xlsRow4++;
                startRow4 = xlsRow4;
                perStartRow4 = xlsRow4;
                bool isFourth = true;
                //string totalTaxformula = "";
                //string voucherNocomp = "";
                //string taxFitler = "";
                for (int i = 0; i < dtHSN.Rows.Count; i++)
                {
                    if (isFourth == false)
                    {
                        sheet4[perStartRow4, iHSN, xlsRow4 - 1, iHSN].BorderAround(ExcelLineStyle.Hair);
                        sheet4[perStartRow4, iDescription, xlsRow4 - 1, iDescription].BorderAround(ExcelLineStyle.Hair);
                        sheet4[perStartRow4, iUQC, xlsRow4 - 1, iUQC].BorderAround(ExcelLineStyle.Hair);
                        sheet4[perStartRow4, iTotalQuantity, xlsRow4 - 1, iTotalQuantity].BorderAround(ExcelLineStyle.Hair);
                        sheet4[perStartRow4, iTotalValue, xlsRow4 - 1, iTotalValue].BorderAround(ExcelLineStyle.Hair);
                        sheet4[perStartRow4, iRate4, xlsRow4 - 1, iRate4].BorderAround(ExcelLineStyle.Hair);
                        sheet4[perStartRow4, iTaxableValue4, xlsRow4 - 1, iTaxableValue4].BorderAround(ExcelLineStyle.Hair);
                        sheet4[perStartRow4, iIntegratedTaxAmount, xlsRow4 - 1, iIntegratedTaxAmount].BorderAround(ExcelLineStyle.Hair);
                        sheet4[perStartRow4, iCentralTaxAmount, xlsRow4 - 1, iCentralTaxAmount].BorderAround(ExcelLineStyle.Hair);
                        sheet4[perStartRow4, iStateUTTaxAmount, xlsRow4 - 1, iStateUTTaxAmount].BorderAround(ExcelLineStyle.Hair);
                        sheet4[perStartRow4, iCessAmount4, xlsRow4 - 1, iCessAmount4].BorderAround(ExcelLineStyle.Hair);

                    }
                    isFourth = false;


                    sheet4.Range[xlsRow4, iHSN].Text = dtHSN.Rows[i]["HSN"].ToString();
                    sheet4.Range[xlsRow4, iDescription].Text = dtHSN.Rows[i]["Description"].ToString();

                    sheet4.Range[xlsRow4, iUQC].Text = dtHSN.Rows[i]["UoM"].ToString();

                    sheet4.Range[xlsRow4, iTotalQuantity].Number = clsStaticInfo.dbl(dtHSN.Rows[i]["TotalQuantity"].ToString());
                    sheet4.Range[xlsRow4, iTotalQuantity].NumberFormat = "#,##0.00;(#,##0.00)";

                    sheet4.Range[xlsRow4, iTotalValue].Number = clsStaticInfo.dbl(dtHSN.Rows[i]["TotalValue"].ToString());
                    sheet4.Range[xlsRow4, iTotalValue].NumberFormat = "#,##0.00;(#,##0.00)";

                    sheet4.Range[xlsRow4, iRate4].Number = clsStaticInfo.dbl(dtHSN.Rows[i]["Rate"].ToString());
                    sheet4.Range[xlsRow4, iRate4].NumberFormat = "#,##0.00;(#,##0.00)";

                    sheet4.Range[xlsRow4, iTaxableValue4].Number = clsStaticInfo.dbl(dtHSN.Rows[i]["TaxableValue"].ToString());
                    sheet4.Range[xlsRow4, iTaxableValue4].NumberFormat = "#,##0.00;(#,##0.00)";

                    sheet4.Range[xlsRow4, iIntegratedTaxAmount].Number = clsStaticInfo.dbl(dtHSN.Rows[i]["IntegratedTaxAmount"].ToString());
                    sheet4.Range[xlsRow4, iIntegratedTaxAmount].NumberFormat = "#,##0.00;(#,##0.00)";

                    sheet4.Range[xlsRow4, iCentralTaxAmount].Number = clsStaticInfo.dbl(dtHSN.Rows[i]["CentralTaxAmount"].ToString());
                    sheet4.Range[xlsRow4, iCentralTaxAmount].NumberFormat = "#,##0.00;(#,##0.00)";

                    sheet4.Range[xlsRow4, iStateUTTaxAmount].Number = clsStaticInfo.dbl(dtHSN.Rows[i]["StateUTTaxAmount"].ToString());
                    sheet4.Range[xlsRow4, iStateUTTaxAmount].NumberFormat = "#,##0.00;(#,##0.00)";


                    sheet4.Range[xlsRow4, iCessAmount4].Number = clsStaticInfo.dbl(dtHSN.Rows[i]["CessAmount"].ToString());
                    sheet4.Range[xlsRow4, iCessAmount4].NumberFormat = "#,##0.00;(#,##0.00)";

                    xlsRow4++;
                    //}

                    //voucherNo = dtHSN.Rows[i]["VoucherNo"].ToString().ToUpper();


                }

                sheet4[perStartRow4, iHSN, xlsRow4 - 1, iHSN].BorderAround(ExcelLineStyle.Hair);
                sheet4[perStartRow4, iDescription, xlsRow4 - 1, iDescription].BorderAround(ExcelLineStyle.Hair);

                sheet4[perStartRow4, iUQC, xlsRow4 - 1, iUQC].BorderAround(ExcelLineStyle.Hair);
                sheet4[perStartRow4, iTotalValue, xlsRow4 - 1, iTotalValue].BorderAround(ExcelLineStyle.Hair);
                sheet4[perStartRow4, iRate4, xlsRow4 - 1, iRate4].BorderAround(ExcelLineStyle.Hair);
                sheet4[perStartRow4, iTaxableValue4, xlsRow4 - 1, iTaxableValue4].BorderAround(ExcelLineStyle.Hair);
                sheet4[perStartRow4, iIntegratedTaxAmount, xlsRow4 - 1, iIntegratedTaxAmount].BorderAround(ExcelLineStyle.Hair);
                sheet4[perStartRow4, iCentralTaxAmount, xlsRow4 - 1, iCentralTaxAmount].BorderAround(ExcelLineStyle.Hair);
                sheet4[perStartRow4, iStateUTTaxAmount, xlsRow4 - 1, iStateUTTaxAmount].BorderAround(ExcelLineStyle.Hair);
                sheet4[perStartRow4, iCessAmount4, xlsRow4 - 1, iCessAmount4].BorderAround(ExcelLineStyle.Hair);

                #endregion ******************Export******************


                #region ******************Report Header******************


                xlsRow = 1;
                xlsCol = 3;
                try
                {
                    if (companyLogo != null)
                    {

                        double totalWidth = sheet1.GetColumnWidth(1) + sheet1.GetColumnWidth(3);
                        int totalWidthPixel = (int)(totalWidth * 7.5);
                        int totalheight = (int)((sheet1.GetRowHeight(1) + sheet1.GetRowHeight(2) + sheet1.GetRowHeight(3) + sheet1.GetRowHeight(3)) * 1.50);

                        companyLogo = ReportUtility.FixedSize(companyLogo, totalWidthPixel, totalheight);
                        IPictureShape pic = null;

                        pic = sheet1.Pictures.AddPicture(1, 1, companyLogo);
                        //pic.Height = 80;
                        //pic.Width = 220;
                    }
                }
                catch (Exception ex)
                {
                }

                FactoryName = string.Empty;

                string FactoryAddress = string.Empty;

                if (dtCmp.Rows.Count > 0)
                {
                    CmpName = dtCmp.Rows[0]["CompanyName"].ToString();
                }
                else
                {
                    CmpName = "";
                }
                sheet1.Range[xlsRow, 3].Text = CmpName;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 12;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 17;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                if (dtFactory.Rows.Count > 0)
                {
                    //FactoryName = dsFactory.Tables[0].Rows[0]["PlantName"].ToString();
                    FactoryName = dtFactory.Rows[0]["UserName"].ToString();
                }
                else
                {
                    FactoryName = "";
                }
                sheet1.Range[xlsRow, 3].Text = FactoryName;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 14;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 18;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                if (dtFactory.Rows.Count > 0)
                {
                    FactoryAddress = dtFactory.Rows[0]["Address1"].ToString();
                }
                else
                {
                    FactoryAddress = "";
                }
                sheet1.Range[xlsRow, 3].Text = FactoryAddress;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                //sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 22;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                sheet1.Range[xlsRow, 3].Text = "GST Payable Sales Report (Format 3) From " + fromDate + " To " + toDate;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;


                #endregion ******************Report Header******************

                #region Freeze Panes

                sheet1.IsDisplayZeros = false;
                sheet1.UsedRange["A7"].FreezePanes();
                sheet1.FirstVisibleColumn = 1;
                sheet1.FirstVisibleRow = 6;

                #endregion Freeze Panes

                #region UsedRange Alignment

                sheet1.UsedRange.WrapText = false;
                sheet1.UsedRange.CellStyle.Font.Size = 10;
                sheet1.Range["A1"].CellStyle.Font.Size = 14;
                sheet1.Range["A2"].CellStyle.Font.Size = 10;
                sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;
                sheet1.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                #endregion UsedRange Alignment

                #region Page Setup
                sheet1.PageSetup.TopMargin = 0.5;
                sheet1.PageSetup.BottomMargin = 0.7;
                sheet1.PageSetup.PrintTitleRows = "$1:$5";
                sheet1.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                sheet1.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + name + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:MM tt").ToString();
                sheet1.PageSetup.LeftMargin = 0.5;
                sheet1.PageSetup.RightMargin = 0.2;
                sheet1.PageSetup.Orientation = ExcelPageOrientation.Portrait;
                sheet1.PageSetup.FitToPagesTall = 0;
                sheet1.PageSetup.FitToPagesWide = 1;
                sheet1.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                sheet1.IsDisplayZeros = false;
                #endregion Page SetupLineItemType


                sheet1.Name = "B2B";
                sheet2.Name = "Export";
                sheet3.Name = "CDNR";
                sheet4.Name = "HSN";
                return workbook;
            }
            catch (System.Exception ex)
            {

                throw ex;
            }
        }

        private DataTable GetExportSQL(string companyGroupId, string companyId, string plantId, string plantName, string fromDate, string toDate, string taxyearId)
        {
            string strSql = "";
            strSql = @"SELECT 'WPAY' ExportType,x.InvoiceNumber,x.DocDate InvoiceDate,X.Amount InvoiceValue,x.HSNSAC,X.PostCode,X.ShippingBill,X.ShippingDate,x.Rate,SUM(x.TaxableAmount) TaxableValue,x.CessAmount,x.ApplicableofTaxRate,0 IGST,X.RodTepAmount,0 DutyDrawBack

					
		            FROM 
                      (
						            SELECT  SourceType=case when v.SourceType='SalesInvoice' THEN 'Sales' ELSE 'Invoice' END
                            ,V.VoucherNo,format( V.PostingDate,'dd-MMM-yyyy')PostingDate, V.DocRefNo,format (V.DocDate,'dd-MMM-yyyy')DocDate
							,P.UserName PartyName,PP.GSTIN
							,NULL InvoiceNumber,pp.UserName PartyPlantName
                            ,LineItemType=case   WHEN v.SourceType='CustomerInvoice' THEN 'GL'
                            ELSE '' END
                            --,Particular=CASE WHEN v.SourceType='VendorInvoice' THEN A.UserName
                            --WHEN v.SourceType='VendorPayment' THEN AP.UserName
                            --ELSE '' END
                            ,TaxableAmount=case when v.SourceType='CustomerInvoice' then ISNULL(VD.CrAmount,0) else 0 end
                            ,0 DrAmount
                            ,CrAmount=case when ITD.AType='Cr' then ISNULL(IT.TaxAmount,0) else 0 end
							
	                        ,format( v.VoucherDate,'dd-MMM-yyyy')VoucherDate
                            ,TC.TaxCategoryType,TC.Code TaxCode,TC.Sequence TCSequence,TC.UserName+'-'+TC.Code TaxCategory,IsNULL(TAXC.IsRCM,0) IsRCM
							
                            ,IsNULL(IV.IsExcludingTax,0) IsExcludingTax,0 IsTaxApplicable,TAXC.[Type],TAXC.ValueOfFixed
                            ,0 Rate,NULL HSNCodeId,NULL Material
							,TaxPercentage= case when v.SourceType='CustomerInvoice' then taxc.ValueOfFixed
												  else 0 end
							, Format (IT.AddedDate,'dd-MMM-yyyy')EntryDate
                            ,NULL PlaceofSupply,0 ReverseCharge,NULL Suppliesundersection7ofIGSTAct,NULL InvoiceType,NULL ECommerceGSTIN
                            ,NULL ItemName,NULL HSNSAC,0 CessAmount,0 ApplicableofTaxRate,CAM.CountryId Country,PAM.CountryId PCountry,CRN.Code,IV.Amount
							 ,NULL PostCode,NULL ShippingBill,NULL ShippingDate, NULL RodTepAmount
                            from TRN.InvoiceTax IT
                            left join TRN.InvoiceTaxDetail ITD ON IT.Id=ITD.InvoiceTaxId AND ITD.AType='Cr'
                            LEFT JOIN TRN.Voucher V ON V.Id=IT.VoucherId
                            LEFT JOIN TRN.Invoice IV ON IV.Id=IT.InvoiceId
                            LEFT JOIN HKP.Activity TA ON TA.Id=ITD.ActivityId
                            LEFT JOIN HKP.Party P ON P.Id=IT.PartyId
							Left join hkp.PartyPlant PP on PP.Id=IT.PartyPlantId
                            LEFT JOIN MST.TaxCategory TC ON TC.Id=IT.TaxCategoryId
                            LEFT JOIN( select TAC.Id,TAC.UserName,TAC.IsRCM,TAY.[Type],TACD.ValueOfFixed from MST.TaxCode TAC
                            LEFT JOIN MST.TaxCodeYear TAY ON TAY.TaxCodeId=TAC.Id
                            LEFT JOIN MST.TaxCodeDetail TACD ON TACD.TaxCodeId=TAC.Id WHERE TAY.TaxYearId IN (" + taxyearId + @")) TAXC ON TAXC.Id=IT.TaxCodeId
                            LEFT JOIN TRN.VoucherDetail VD ON VD.Id=IT.VoucherDetailId
                            LEFT JOIN HKP.Activity A ON A.Id=VD.ActivityId
                            LEFT JOIN (SELECT IW.InvoiceWriteOffId,IW.ActivityId,SUM(I.Amount) Amount FROM TRN.InvoiceWriteOffDetail IW
                            JOIN TRN.Invoice I ON I.Id=IW.InvoiceId
                            GROUP BY InvoiceWriteOffId,ActivityId) IWD ON IWD.InvoiceWriteOffId=IT.InvoiceWriteOffId
                            LEFT JOIN HKP.Activity AP ON AP.Id=IWD.ActivityId
							LEFT JOIN ORG.Company C ON C.Id=IV.CompanyId
							LEFT JOIN MST.AddressMaster CAM ON CAM.Id=C.AddressMasterId
							LEFT JOIN MST.AddressMaster PAM ON PAM.Id=P.AddressMasterId
							LEFT JOIN SCS.Currency CRN ON CRN.Id=IV.CurrencyId
                            where TC.TaxCategoryType='GST' AND TAXC.IsRCM=0 AND  V.IsPark=0 AND V.PlantId='" + plantId + @"'
							and V.PostingDate between '" + fromDate + "' AND '" + toDate + @"'
                            AND v.SourceType IN ('CustomerInvoice')
                            
                            UNION all

						SELECT SourceType=case when v.SourceType='SalesInvoice' THEN 'Sales' ELSE 'Invoice' END
                            ,V.VoucherNo,format( V.PostingDate,'dd-MMM-yyyy')PostingDate, V.DocRefNo,format (V.DocDate,'dd-MMM-yyyy')DocDate,P.UserName PartyName,PP.GSTIN
							, IRD.InventorySalesId InvoiceNumber,pp.UserName PartyPlantName
                            ,LineItemType=case when v.SourceType='SalesInvoice' then 'Sales'
                            ELSE '' END
                            
                            ,TaxableAmount=case when v.SourceType='SalesInvoice' then ISNULL(IRD.PolicyAmount,0)
                            else 0 end
                            ,0 DrAmount
                            ,CrAmount=case when ITD.AType='Cr' then ISNULL(IRT.TaxAmount,0) else 0 end
	                        ,format( v.VoucherDate,'dd-MMM-yyyy')VoucherDate
                            ,TC.TaxCategoryType,TC.Code TaxCode,TC.Sequence TCSequence,TC.UserName+'-'+TC.Code TaxCategory
							,IsNULL(TAXC.IsRCM,0) IsRCM
                            ,IsNULL(IV.IsExcludingTax,0) IsExcludingTax,IsTaxApplicable=CASE WHEN IsNULL(CP.TaxApplicable,'')='Mandatory' THEN 1 ELSE 0 END 
							,TAXC.[Type],TAXC.ValueOfFixed
                            ,IRT.[Percentage]Rate,NULL HSNCodeId,null Material
							,TaxPercentage= case  when v.SourceType='SalesInvoice' AND IRT.[Percentage]>0 THEN IRT.[Percentage]
												 else 0 end
							,it.AddedDate EntryDate
                            ,NULL PlaceofSupply,0 ReverseCharge,NULL Suppliesundersection7ofIGSTAct,NULL InvoiceType,NULL ECommerceGSTIN
                            ,NULL ItemName,H.Code HSNSAC,0 CessAmount,0 ApplicableofTaxRate,CAM.CountryId Country,PAM.CountryId PCountry,CRN.Code,IV.Amount
							,NULL PostCode,NULL ShippingBill,NULL ShippingDate, NULL RodTepAmount
                            from TRN.InvoiceTax IT
                            left join TRN.InvoiceTaxDetail ITD ON IT.Id=ITD.InvoiceTaxId AND ITD.AType='Cr'
                            LEFT JOIN TRN.Voucher V ON V.Id=IT.VoucherId
                            LEFT JOIN TRN.Invoice IV ON IV.Id=IT.InvoiceId
                            --LEFT JOIN TRN.InvoiceWriteOff IW ON IW.Id=IT.InvoiceWriteOffId
                            LEFT JOIN HKP.Activity TA ON TA.Id=ITD.ActivityId
                            LEFT JOIN HKP.Party P ON P.Id=IT.PartyId
							
                            LEFT JOIN MST.TaxCategory TC ON TC.Id=IT.TaxCategoryId
                            LEFT JOIN( select TAC.Id,TAC.UserName,TAC.IsRCM,TAY.[Type],TACD.ValueOfFixed from MST.TaxCode TAC
                            LEFT JOIN MST.TaxCodeYear TAY ON TAY.TaxCodeId=TAC.Id
                            LEFT JOIN MST.TaxCodeDetail TACD ON TACD.TaxCodeId=TAC.Id WHERE TAY.TaxYearId IN (" + taxyearId + @")
							--and tac.IsRCM=0
							) TAXC ON TAXC.Id=IT.TaxCodeId
                            --LEFT JOIN SCS.TaxYear TY ON TY.Id=TAY.TaxYearId
                            LEFT JOIN TRN.InventorySales IR ON IR.VoucherId=V.Id
                            LEFT JOIN TRN.InventorySalesDetail IRD ON IRD.InventorySalesId=IR.Id
                            LEFT JOIN TRN.InventorySalesTax IRT ON IRD.Id=IRT.InventorySalesDetailId AND IRT.TaxCategoryId=IT.TaxCategoryId
                            --LEFT JOIN MST.HSNTaxPercentage HSNP ON IRT.HSNCodeId=HSNP.HSNCodeId AND HSNP.TaxCategoryId=IT.TaxCategoryId
							LEFT JOIN HKP.HSNCode AS h ON h.Id = IRT.HSNCodeId
                            LEFT JOIN TRN.InventoryMaterial IM ON IM.Id=IRD.InventoryMaterialId
                            LEFT JOIN MST.MaterialMaster MM ON MM.Id=IM.MaterialMasterId
                            LEFT JOIN TRN.VoucherDetail VD ON VD.Id=IT.VoucherDetailId
                            LEFT JOIN HKP.Activity A ON A.Id=VD.ActivityId
                            Left join hkp.PartyPlant pp on pp.Id=IR.InvoicingPartyPlantId
							LEFT JOIN HKP.CompanyParty CP ON CP.PartyId=P.Id AND CP.PartyType='Customer' AND CP.PlantId = '" + plantId + @"'
							LEFT JOIN ORG.Company C ON C.Id=IV.CompanyId
							LEFT JOIN MST.AddressMaster CAM ON CAM.Id=C.AddressMasterId
							LEFT JOIN MST.AddressMaster PAM ON PAM.Id=P.AddressMasterId
							LEFT JOIN SCS.Currency CRN ON CRN.Id=IV.CurrencyId
							
                            where TC.TaxCategoryType='GST' AND (CP.TaxApplicable IS NULL OR CP.TaxApplicable ='Optional') AND V.IsPark=0
							AND IR.PlantId = '" + plantId + @"' and V.PostingDate between '" + fromDate + "' AND '" + toDate + @"'
                            AND v.SourceType='SalesInvoice' and IRT.InventorySalesServiceId IS NULL


                            
                            
							union all
							SELECT SourceType=case when v.SourceType='SalesInvoice' THEN 'Sales' ELSE 'Invoice' END
                            ,V.VoucherNo,format( V.PostingDate,'dd-MMM-yyyy')PostingDate, V.DocRefNo,format (V.DocDate,'dd-MMM-yyyy')DocDate,P.UserName PartyName,PP.GSTIN
							, IRD.InventorySalesId InvoiceNumber,pp.UserName PartyPlantName
                            ,LineItemType=case when v.SourceType='SalesInvoice' then 'Sales'
                            ELSE '' END
                            
                            ,TaxableAmount=case when v.SourceType='SalesInvoice' then ISNULL(IRD.TotalSalesAmount,0)
                            else 0 end
                            ,0 DrAmount
                            ,CrAmount=case when ITD.AType='Cr' then ISNULL(IRT.TaxAmount,0) else 0 end
	                        ,format( v.VoucherDate,'dd-MMM-yyyy')VoucherDate
                            ,TC.TaxCategoryType,TC.Code TaxCode,TC.Sequence TCSequence,TC.UserName+'-'+TC.Code TaxCategory
							,IsNULL(TAXC.IsRCM,0) IsRCM
                            ,IsNULL(IV.IsExcludingTax,0) IsExcludingTax,IsTaxApplicable=CASE WHEN IsNULL(CP.TaxApplicable,'')='Mandatory' THEN 1 ELSE 0 END 
							,TAXC.[Type],TAXC.ValueOfFixed
                            ,IRT.[Percentage]Rate,NULL HSNCodeId,null Material
							,TaxPercentage= case  when v.SourceType='SalesInvoice' AND IRT.[Percentage]>0 THEN IRT.[Percentage]
												 else 0 end
							,it.AddedDate EntryDate
                            ,NULL PlaceofSupply,0 ReverseCharge,NULL Suppliesundersection7ofIGSTAct,NULL InvoiceType,NULL ECommerceGSTIN
                            ,NULL ItemName,h.Code HSNSAC,0 CessAmount,0 ApplicableofTaxRate,CAM.CountryId Country,PAM.CountryId PCountry,CRN.Code,IV.Amount
							,NULL PostCode,NULL ShippingBill,NULL ShippingDate, NULL RodTepAmount
                            FROM TRN.InvoiceTax IT
                            left join TRN.InvoiceTaxDetail ITD ON IT.Id=ITD.InvoiceTaxId AND ITD.AType='Cr'
                            LEFT JOIN TRN.Voucher V ON V.Id=IT.VoucherId
                            LEFT JOIN TRN.Invoice IV ON IV.Id=IT.InvoiceId
                            LEFT JOIN HKP.Activity TA ON TA.Id=ITD.ActivityId
                            LEFT JOIN HKP.Party P ON P.Id=IT.PartyId
							
                            LEFT JOIN MST.TaxCategory TC ON TC.Id=IT.TaxCategoryId
                            LEFT JOIN( select distinct TAC.Id,TAC.UserName,TAC.IsRCM,TAY.[Type],TACD.ValueOfFixed from MST.TaxCode TAC
                            LEFT JOIN MST.TaxCodeYear TAY ON TAY.TaxCodeId=TAC.Id
                            LEFT JOIN MST.TaxCodeDetail TACD ON TACD.TaxCodeId=TAC.Id WHERE TAY.TaxYearId IN (" + taxyearId + @") 
							) TAXC ON TAXC.Id=IT.TaxCodeId
                            LEFT JOIN TRN.InventorySales IR ON IR.VoucherId=V.Id
                            LEFT JOIN TRN.InventorySalesDetail IRD ON IRD.InventorySalesId=IR.Id
                            LEFT JOIN TRN.InventorySalesTax IRT ON IRD.Id=IRT.InventorySalesServiceId AND IRT.TaxCategoryId=IT.TaxCategoryId
							LEFT JOIN HKP.HSNCode AS h ON h.Id = IRT.HSNCodeId
                            LEFT JOIN TRN.InventoryMaterial IM ON IM.Id=IRD.InventoryMaterialId
                            LEFT JOIN MST.MaterialMaster MM ON MM.Id=IM.MaterialMasterId
                            LEFT JOIN TRN.VoucherDetail VD ON VD.Id=IT.VoucherDetailId
                            LEFT JOIN HKP.Activity A ON A.Id=VD.ActivityId
                            Left join hkp.PartyPlant pp on pp.Id=IR.InvoicingPartyPlantId
							LEFT JOIN HKP.CompanyParty CP ON CP.PartyId=P.Id AND CP.PartyType='Customer' AND CP.PlantId = '" + plantId + @"'
							LEFT JOIN ORG.Company C ON C.Id=IV.CompanyId
							LEFT JOIN MST.AddressMaster CAM ON CAM.Id=C.AddressMasterId
							LEFT JOIN MST.AddressMaster PAM ON PAM.Id=P.AddressMasterId
							LEFT JOIN SCS.Currency CRN ON CRN.Id=IV.CurrencyId
                            where TC.TaxCategoryType='GST' AND (CP.TaxApplicable IS NULL OR CP.TaxApplicable ='Optional') AND V.IsPark=0
							AND IR.PlantId = '" + plantId + @"' and V.PostingDate between '" + fromDate + "' AND '" + toDate + @"'
                            AND v.SourceType='SalesInvoice' and IRT.InventorySalesDetailId IS NULL  

							UNION All

								SELECT SourceType=case when v.SourceType='SalesInvoice' THEN 'Sales' ELSE 'Invoice' END
                            ,V.VoucherNo,format( V.PostingDate,'dd-MMM-yyyy')PostingDate, V.DocRefNo,format (V.DocDate,'dd-MMM-yyyy')DocDate,P.UserName PartyName,PP.GSTIN
							, IRD.InventorySalesId InvoiceNumber,pp.UserName PartyPlantName
                            ,LineItemType=case when v.SourceType='SalesInvoice' then 'Material'
                            ELSE '' END
                            
                            ,TaxableAmount=case when v.SourceType='SalesInvoice' then ISNULL(IRD.Amount,0)
                            else 0 end
                            ,0 DrAmount
                            ,CrAmount=case when ITD.AType='Cr' then ISNULL(IRT.TaxAmount,0) else 0 end
	                        ,format( v.VoucherDate,'dd-MMM-yyyy')VoucherDate
                            ,TC.TaxCategoryType,TC.Code TaxCode,TC.Sequence TCSequence,TC.UserName+'-'+TC.Code TaxCategory
							,IsNULL(TAXC.IsRCM,0) IsRCM
                            ,IsNULL(IV.IsExcludingTax,0) IsExcludingTax,IsTaxApplicable=CASE WHEN IsNULL(CP.TaxApplicable,'')='Mandatory' THEN 1 ELSE 0 END 
							,TAXC.[Type],TAXC.ValueOfFixed
                            ,IRT.[Percentage]Rate,NULL HSNCodeId,null Material
							,TaxPercentage= case  when v.SourceType='SalesInvoice' AND IRT.[Percentage]>0 THEN IRT.[Percentage]
												 else 0 end
							,it.AddedDate EntryDate
                            ,NULL PlaceofSupply,0 ReverseCharge,NULL Suppliesundersection7ofIGSTAct,NULL InvoiceType,NULL ECommerceGSTIN
                            ,NULL ItemName,h.Code HSNSAC,0 CessAmount,0 ApplicableofTaxRate,CAM.CountryId Country,PAM.CountryId PCountry,CRN.Code,IV.Amount
							,NULL PostCode,NULL ShippingBill,NULL ShippingDate, NULL RodTepAmount
                            from TRN.InvoiceTax IT
                            left join TRN.InvoiceTaxDetail ITD ON IT.Id=ITD.InvoiceTaxId AND ITD.AType='Cr'
                            LEFT JOIN TRN.Voucher V ON V.Id=IT.VoucherId
                            LEFT JOIN TRN.Invoice IV ON IV.Id=IT.InvoiceId
                            --LEFT JOIN TRN.InvoiceWriteOff IW ON IW.Id=IT.InvoiceWriteOffId
                            LEFT JOIN HKP.Activity TA ON TA.Id=ITD.ActivityId
                            LEFT JOIN HKP.Party P ON P.Id=IT.PartyId
							
                            LEFT JOIN MST.TaxCategory TC ON TC.Id=IT.TaxCategoryId
                            LEFT JOIN( select TAC.Id,TAC.UserName,TAC.IsRCM,TAY.[Type],TACD.ValueOfFixed from MST.TaxCode TAC
                            LEFT JOIN MST.TaxCodeYear TAY ON TAY.TaxCodeId=TAC.Id
                            LEFT JOIN MST.TaxCodeDetail TACD ON TACD.TaxCodeId=TAC.Id WHERE TAY.TaxYearId IN (" + taxyearId + @") 
							--and tac.IsRCM=0
							) TAXC ON TAXC.Id=IT.TaxCodeId
                            --LEFT JOIN SCS.TaxYear TY ON TY.Id=TAY.TaxYearId
                            LEFT JOIN TRN.InventorySales IR ON IR.InventoryVoucherId=V.Id
                            LEFT JOIN TRN.InventorySalesService IRD ON IRD.InventorySalesId=IR.Id
                            LEFT JOIN TRN.InventorySalesTax IRT ON IRD.Id=IRT.InventorySalesServiceId AND IRT.TaxCategoryId=IT.TaxCategoryId
							LEFT JOIN HKP.HSNCode AS h ON h.Id = IRT.HSNCodeId
                            --LEFT JOIN MST.HSNTaxPercentage HSNP ON IRT.HSNCodeId=HSNP.HSNCodeId AND HSNP.TaxCategoryId=IT.TaxCategoryId
                            LEFT JOIN HKP.ServiceMaster MM ON MM.Id=IRD.ServiceMasterId
                            LEFT JOIN TRN.VoucherDetail VD ON VD.Id=IT.VoucherDetailId
                            LEFT JOIN HKP.Activity A ON A.Id=VD.ActivityId
                            Left join hkp.PartyPlant pp on pp.Id=IR.InvoicingPartyPlantId
							LEFT JOIN HKP.CompanyParty CP ON CP.PartyId=P.Id AND CP.PartyType='Customer' AND CP.PlantId = '" + plantId + @"'
							LEFT JOIN ORG.Company C ON C.Id=IV.CompanyId
							LEFT JOIN MST.AddressMaster CAM ON CAM.Id=C.AddressMasterId
							LEFT JOIN MST.AddressMaster PAM ON PAM.Id=P.AddressMasterId
							LEFT JOIN SCS.Currency CRN ON CRN.Id=IV.CurrencyId
                            where TC.TaxCategoryType='GST' AND (CP.TaxApplicable IS NULL OR CP.TaxApplicable ='Optional') AND V.IsPark=0
							AND IR.PlantId = '" + plantId + @"' and V.PostingDate between '" + fromDate + "' AND '" + toDate + @"'
                            AND v.SourceType='SalesInvoice' and isnull(IRT.InventorySalesDetailId,'') IS NULL
							
					

							UNION ALL

						
				SELECT  SourceType=case when v.SourceType='SalesInvoice' THEN 'Sales' ELSE 'Invoice' END
                            ,V.VoucherNo,format( V.PostingDate,'dd-MMM-yyyy')PostingDate, V.DocRefNo,format (V.DocDate,'dd-MMM-yyyy')DocDate,P.UserName PartyName,PP.GSTIN
							, IRD.SalesId InvoiceNumber,pp.UserName PartyPlantName
                            ,LineItemType=case when v.SourceType='SalesInvoice' then 'Material'
                            ELSE '' END
                            
                            ,TaxableAmount=case when v.SourceType='SalesInvoice' then ISNULL(IRD.TransactionAmount,0)
                            else 0 end
                            ,0 DrAmount,CrAmount=case when ITD.AType='Cr' then ISNULL(IRT.Amount,0) else 0 end
	                        ,format( v.VoucherDate,'dd-MMM-yyyy')VoucherDate
                            ,TC.TaxCategoryType,TC.Code TaxCode,TC.Sequence TCSequence,TC.UserName+'-'+TC.Code TaxCategory
							,IsNULL(TAXC.IsRCM,0) IsRCM
                            ,IsNULL(IV.IsExcludingTax,0) IsExcludingTax,IsTaxApplicable=CASE WHEN IsNULL(CP.TaxApplicable,'')='Mandatory' THEN 1 ELSE 0 END 
							,TAXC.[Type],TAXC.ValueOfFixed
                            ,IRT.[Percentage] Rate,NULL HSNCodeId,null Material
							,TaxPercentage= case  when v.SourceType='SalesInvoice' AND IRT.[Percentage]>0 THEN IRT.[Percentage]
												 else 0 end
							,it.AddedDate EntryDate
                            ,NULL PlaceofSupply,0 ReverseCharge,NULL Suppliesundersection7ofIGSTAct,NULL InvoiceType,NULL ECommerceGSTIN
                            ,NULL ItemName,h.Code HSNSAC,0 CessAmount,0 ApplicableofTaxRate,CAM.CountryId Country,PAM.CountryId PCountry,CRN.Code,IV.Amount
							,SAI.PostCode,SAI.ShippingBill,FORMAT(SAI.ShippingDate,'dd-MMM-yyyy')ShippingDate, SAI.RodTepAmount
                            from TRN.InvoiceTax IT
                            left join TRN.InvoiceTaxDetail ITD ON IT.Id=ITD.InvoiceTaxId AND ITD.AType='Cr'
                            LEFT JOIN TRN.Voucher V ON V.Id=IT.VoucherId
                            LEFT JOIN TRN.Invoice IV ON IV.Id=IT.InvoiceId
                            --LEFT JOIN TRN.InvoiceWriteOff IW ON IW.Id=IT.InvoiceWriteOffId
                            LEFT JOIN HKP.Activity TA ON TA.Id=ITD.ActivityId
                            LEFT JOIN HKP.Party P ON P.Id=IT.PartyId
							
                            LEFT JOIN MST.TaxCategory TC ON TC.Id=IT.TaxCategoryId
                            LEFT JOIN( select TAC.Id,TAC.UserName,TAC.IsRCM,TAY.[Type],TACD.ValueOfFixed from MST.TaxCode TAC
                            LEFT JOIN MST.TaxCodeYear TAY ON TAY.TaxCodeId=TAC.Id
                            LEFT JOIN MST.TaxCodeDetail TACD ON TACD.TaxCodeId=TAC.Id WHERE TAY.TaxYearId IN (" + taxyearId + @") 
							--and tac.IsRCM=0
							) TAXC ON TAXC.Id=IT.TaxCodeId
                            --LEFT JOIN SCS.TaxYear TY ON TY.Id=TAY.TaxYearId
                            LEFT JOIN TRN.Sales IR ON IR.VoucherId=V.Id
                           LEFT JOIN TRN.SalesMaterial IRD ON IRD.SalesId=IR.Id
							  -- LEFT JOIN TRN.InventorySalesDetail IRD ON IRD.InventorySalesId=IR.Id
                            LEFT JOIN TRN.SalesTax IRT ON IRD.Id=IRT.SalesMaterialId AND IRT.TaxCategoryId=IT.TaxCategoryId
							LEFT JOIN HKP.HSNCode AS h ON h.Id = IRT.HSNCodeId
							LEFT JOIN(Select top(1)* from [dbo].[SalesAdditionalInfo] Order By AddedDate DESC) SAI ON SAI.SalesId=IR.Id
                            --LEFT JOIN MST.HSNTaxPercentage HSNP ON IRT.HSNCodeId=HSNP.HSNCodeId AND HSNP.TaxCategoryId=IT.TaxCategoryId
                           -- LEFT JOIN TRN.InventoryMaterial IM ON IM.Id=IRD.InventoryMaterialId
                            LEFT JOIN MST.MaterialMaster MM ON MM.Id=IRD.MaterialMasterId
                            LEFT JOIN TRN.VoucherDetail VD ON VD.Id=IT.VoucherDetailId
                            LEFT JOIN HKP.Activity A ON A.Id=VD.ActivityId
                            Left join hkp.PartyPlant pp on pp.Id=IR.InvoicingPartyPlantId
							LEFT JOIN HKP.CompanyParty CP ON CP.PartyId=P.Id AND CP.PartyType='Customer' AND CP.PlantId = '" + plantId + @"'
							LEFT JOIN ORG.Company C ON C.Id=IV.CompanyId
							LEFT JOIN MST.AddressMaster CAM ON CAM.Id=C.AddressMasterId
							LEFT JOIN MST.AddressMaster PAM ON PAM.Id=P.AddressMasterId
							LEFT JOIN SCS.Currency CRN ON CRN.Id=IV.CurrencyId
                            where TC.TaxCategoryType='GST' AND (CP.TaxApplicable IS NULL OR CP.TaxApplicable ='Optional') AND V.IsPark=0
							AND IR.PlantId = '" + plantId + @"' and V.PostingDate between '" + fromDate + "' AND '" + toDate + @"'
                            AND v.SourceType='SalesInvoice' and IRT.SalesServiceId IS NULL

  ) x
  WHERE X.Country<>X.PCountry
GROUP BY x.InvoiceNumber,x.DocDate,X.Amount,x.HSNSAC,X.PostCode,X.ShippingBill,X.ShippingDate,x.Rate,x.CessAmount,x.ApplicableofTaxRate,X.RodTepAmount";

            return _sqlRepository.GetDataTable(strSql);

        }

        private DataTable GetCDNR()
        {
            string strSql = "";
            strSql = @"select '07ADZPA5596R1ZC' GSTINUINofRecipient,68 NoteRefundVoucherNumber,'3/31/2022' NoteRefundVoucherDate,NULL HSNSAC,'C' NoteType,'Delhi' PlaceOfSupply,'Regular' NoteSupplyType,'N' ReverseCharge,1338.75 NoteRefundVoucherValue,5 Rate,1275 TaxableValue,0 CessAmount,0 ApplicableofTaxRate,0 IGST,0 CGST,0 SGST,0 TotalTax";

            return _sqlRepository.GetDataTable(strSql);
        }
        private DataTable GetHSN(string fromDate, string toDate, string plantId)
        {
            string strSql = "";

            try
            {
                strSql = @"Select * from (Select IRD.Id,HSN =case when IGST.HSN<>'' then IGST.HSN when CGST.HSN<>'' then CGST.HSN else SGST.HSN end
,[Description]=case when IGST.[Description]<>'' then IGST.[Description] when CGST.[Description]<>'' then CGST.[Description] else SGST.[Description] end
,UoM.UserName UoM,IRD.TransactionQty TotalQuantity
,IRD.BooksCurrencyTransactionAmount+IRD.BooksCurrencyTaxAmount TotalValue
,Rate=case when IGST.Rate<>0 then IGST.Rate when CGST.Rate<>0 then CGST.Rate else SGST.Rate end
,IRD.BooksCurrencyTransactionAmount TaxableValue
,IntegratedTaxAmount=CASE WHEN IGST.IGSTAmount<>0 THEN  IGST.IGSTAmount ELSE 0 END
, CentralTaxAmount=CASE WHEN CGST.CGSTAmount<>0 THEN  CGST.CGSTAmount ELSE 0 END
, StateUTTaxAmount=CASE WHEN SGST.SGSTAmount<>0 THEN  SGST.SGSTAmount ELSE 0 END
,CONVERT(varchar(50), I.PostingDate ,103)PostingDate,ir.SourceType [Category],0 CessAmount,IR.PlantId
from TRN.SalesMaterial IRD
LEFT JOIN TRN.Sales IR ON IR.Id=IRD.SalesId
LEFT JOIN TRN.Invoice I ON I.VoucherId=IR.VoucherId
LEFT JOIN SCS.UnitOfMeasurement UoM ON UoM.Id=IRD.TransactionUoMId
LEFT JOIN (SELECT IRT.SalesMaterialId,IRT.BooksCurrencyTransactionAmount IGSTAmount,H.Code HSN,H.[Description],IRT.[Percentage] Rate 
        FROM TRN.SalesTax IRT
        LEFT JOIN HKP.HSNCode AS H ON H.Id = IRT.HSNCodeId
        LEFT JOIN MST.TaxCategory TC ON TC.Id=IRT.TaxCategoryId
        WHERE TC.Code='IGST' and IRT.SalesServiceId is null
    )IGST ON IRD.Id=IGST.SalesMaterialId
    LEFT JOIN (SELECT IRT.SalesMaterialId,IRT.BooksCurrencyTransactionAmount CGSTAmount,H.Code HSN,H.[Description],IRT.[Percentage] Rate 
        FROM TRN.SalesTax IRT
        LEFT JOIN HKP.HSNCode AS H ON H.Id = IRT.HSNCodeId
        LEFT JOIN MST.TaxCategory TC ON TC.Id=IRT.TaxCategoryId
        WHERE TC.Code='CGST' and IRT.SalesServiceId is null
    )CGST ON IRD.Id=CGST.SalesMaterialId
    LEFT JOIN (SELECT IRT.SalesMaterialId,IRT.BooksCurrencyTransactionAmount SGSTAmount,H.Code HSN,H.[Description],IRT.[Percentage] Rate 
        FROM TRN.SalesTax IRT
        LEFT JOIN HKP.HSNCode AS H ON H.Id = IRT.HSNCodeId
        LEFT JOIN MST.TaxCategory TC ON TC.Id=IRT.TaxCategoryId
        WHERE TC.Code='SGST' and IRT.SalesServiceId is null
    )SGST ON IRD.Id=SGST.SalesMaterialId
 Where I.PostingDate between '" + fromDate + "' AND '" + toDate + @"' AND IR.PlantId='" + plantId + @"'
	)A";
                return _sqlRepository.GetDataTable(strSql);
            }
            catch (Exception ex)
            {
                throw ex;
            }


        }

        private DataTable GetImportSQL()
        {
            string strSql = "";
            strSql = @"select 'WPAY' ExportType,'MS2022366' InvoiceNumber,'04-Mar-2022' InvoiceDate,'7983933.98656' InvoiceValue,'55096200' HSNSAC,'INCCU1' PortCode,'8782549' ShippingBillNumber,'3/8/2022' ShippingBillDate,'12' Rate,'7128512.488' TaxableValue,0 CessAmount,0 ApplicableOfTaxRate,0 IGST";

            return _sqlRepository.GetDataTable(strSql);

        }

        private DataTable GetCDNRPurchase()
        {
            string strSql = "";
            strSql = @"select '07ADZPA5596R1ZC' GSTINUINofRecipient,68 NoteRefundVoucherNumber,'3/31/2022' NoteRefundVoucherDate,NULL HSNSAC,'C' NoteType,'Delhi' PlaceOfSupply,'Regular' NoteSupplyType,'N' ReverseCharge,1338.75 NoteRefundVoucherValue,5 Rate,1275 TaxableValue,0 CessAmount,0 ApplicableofTaxRate,0 IGST,0 CGST,0 SGST,0 TotalTax";

            return _sqlRepository.GetDataTable(strSql);
        }

        private DataTable GetHSNPurchase()
        {
            string strSql = "";
            strSql = @"select '7404' HSN,'Note- NA in service' Description,'KGS-KILOGRAMS' UQC,420 TotalQuantity,2478 TotalValue,18 Rate,2100 TaxableValue,8826.6 IntegratedTaxAmount,189 CentralTaxAmount,189 StateUTTaxAmount,0 CessAmount";

            return _sqlRepository.GetDataTable(strSql);
        }

        private DataTable GetGSTReceivableSQL(string companyGroupId, string companyId, string plantId, string plantName, string fromDate, string toDate, string taxyearId)
        {
            string strSql = "";
            strSql = @"SELECT * FROM (SELECT SourceType= CASE WHEN V.SourceType='VendorInvoice' THEN 'Expense'
                            WHEN V.SourceType='VendorPayment' THEN 'Vendor Payment'
                            WHEN V.SourceType='InventoryPayable' THEN 'Material' ELSE '' END
                            ,V.VoucherNo,format( V.PostingDate,'dd-MMM-yyyy')PostingDate, V.DocRefNo,format (V.DocDate,'dd-MMM-yyyy')DocDate
							,P.UserName PartyName,PP.GSTIN
							,NULL GRNNo,pp.UserName PartyPlantName
                            ,LineItemType=case when v.SourceType='InventoryPayable' then 'Material'
                            WHEN v.SourceType='VendorInvoice' THEN 'GL'
                            WHEN v.SourceType='VendorPayment' THEN 'GL'
                            ELSE '' END
                            ,Particular=CASE WHEN v.SourceType='VendorInvoice' THEN A.UserName
                            WHEN v.SourceType='VendorPayment' THEN AP.UserName
                            ELSE '' END
                            ,TaxableAmount=case when v.SourceType='InventoryPayable' then 0
                            when v.SourceType='VendorInvoice' then VD.DrAmount
                            when v.SourceType='VendorPayment' then IWD.Amount else 0 end
                            ,IT.Id,DrAmount=case when ITD.AType='Dr' then IT.TaxAmount else 0 end,0 CrAmount
	                        ,format( v.VoucherDate,'dd-MMM-yyyy')VoucherDate
                            ,TC.TaxCategoryType,TC.Code TaxCode,TC.Sequence TCSequence,TC.UserName+'-'+TC.Code TaxCategory,IsNULL(TAXC.IsRCM,0) IsRCM,TAXC.UserName TaxCodeName
                            ,IsNULL(IV.IsExcludingTax,0) IsExcludingTax,0 IsTaxApplicable,TAXC.[Type],TAXC.ValueOfFixed
                            ,0 [Percentage],NULL HSNCodeId,NULL Material
							,TaxPercentage= case when v.SourceType='VendorInvoice' then taxc.ValueOfFixed
												  else 0 end
												 ,TA.UserName ActivityName,NULL InventoryReceiveDetailId,NULL InventoryServiceId
                            from TRN.InvoiceTax IT
                            left join TRN.InvoiceTaxDetail ITD ON IT.Id=ITD.InvoiceTaxId AND ITD.AType='Dr'
                            LEFT JOIN TRN.Voucher V ON V.Id=IT.VoucherId
                            LEFT JOIN TRN.Invoice IV ON IV.Id=IT.InvoiceId
                            LEFT JOIN HKP.Activity TA ON TA.Id=ITD.ActivityId
                            LEFT JOIN HKP.Party P ON P.Id=IT.PartyId
							Left join hkp.PartyPlant PP on PP.Id=IT.PartyPlantId
                            LEFT JOIN MST.TaxCategory TC ON TC.Id=IT.TaxCategoryId
                            LEFT JOIN( select distinct TAC.Id,TAC.UserName,TAC.IsRCM,TAY.[Type],TACD.ValueOfFixed from MST.TaxCode TAC
                            LEFT JOIN MST.TaxCodeYear TAY ON TAY.TaxCodeId=TAC.Id
                            LEFT JOIN MST.TaxCodeDetail TACD ON TACD.TaxCodeId=TAC.Id WHERE TAY.TaxYearId IN (" + taxyearId + @")) TAXC ON TAXC.Id=IT.TaxCodeId
                            LEFT JOIN TRN.VoucherDetail VD ON VD.Id=IT.VoucherDetailId
                            LEFT JOIN HKP.Activity A ON A.Id=VD.ActivityId
                            LEFT JOIN (SELECT IW.InvoiceWriteOffId,IW.ActivityId,SUM(I.Amount) Amount FROM TRN.InvoiceWriteOffDetail IW
                            JOIN TRN.Invoice I ON I.Id=IW.InvoiceId
                            GROUP BY InvoiceWriteOffId,ActivityId) IWD ON IWD.InvoiceWriteOffId=IT.InvoiceWriteOffId
                            LEFT JOIN HKP.Activity AP ON AP.Id=IWD.ActivityId
                            where TC.TaxCategoryType='GST' AND TAXC.IsRCM=0 AND  V.IsPark=0 AND V.PlantId='" + plantId + "' and V.PostingDate between '" + fromDate + "' AND '" + toDate + @"'
                            AND v.SourceType IN ('VendorInvoice','VendorPayment','CustomerInvoice')
                            
                            UNION all

							SELECT SourceType= CASE WHEN V.SourceType='VendorInvoice' THEN 'Expense'
                            WHEN V.SourceType='VendorPayment' THEN 'Vendor Payment'
                            WHEN V.SourceType='InventoryPayable' THEN 'Material' ELSE '' END
                            ,V.VoucherNo,format( V.PostingDate,'dd-MMM-yyyy')PostingDate, V.DocRefNo,format (V.DocDate,'dd-MMM-yyyy')DocDate,P.UserName PartyName,PP.GSTIN
							,IRD.InventoryReceiveId GRNNo,pp.UserName PartyPlantName
                            ,LineItemType=case when v.SourceType='InventoryPayable' then 'Material'
                            WHEN v.SourceType='VendorInvoice' THEN 'GL'
                            WHEN v.SourceType='VendorPayment' THEN 'GL'
                            ELSE '' END
                            ,Particular=CASE WHEN v.SourceType='InventoryPayable' THEN MM.UserName
                            ELSE '' END
                            ,TaxableAmount=case when v.SourceType='InventoryPayable' then IRD.MaterialTranAmount
                            else 0 end
                            ,IT.Id,DrAmount=case when ITD.AType='Dr' then IRT.TaxAmount else 0 end,0 CrAmount
	                        ,format( v.VoucherDate,'dd-MMM-yyyy')VoucherDate
                            ,TC.TaxCategoryType,TC.Code TaxCode,TC.Sequence TCSequence,TC.UserName+'-'+TC.Code TaxCategory,IsNULL(TAXC.IsRCM,0) IsRCM,TAXC.UserName TaxCodeName
                            ,IsNULL(IV.IsExcludingTax,0) IsExcludingTax,IsNULL(IR.IsTaxApplicable,0) IsTaxApplicable,TAXC.[Type],TAXC.ValueOfFixed
                            ,IRT.[Percentage],NULL HSNCodeId,null Material
							,TaxPercentage= case  when v.SourceType='InventoryPayable' AND IRT.[Percentage]>0 THEN IRT.[Percentage]
												 else 0 end
												 ,TA.UserName ActivityName,IRT.InventoryReceiveDetailId,IRT.InventoryServiceId
                            from TRN.InvoiceTax IT
                            left join TRN.InvoiceTaxDetail ITD ON IT.Id=ITD.InvoiceTaxId AND ITD.AType='Dr'
                            LEFT JOIN TRN.Voucher V ON V.Id=IT.VoucherId
                            LEFT JOIN TRN.Invoice IV ON IV.Id=IT.InvoiceId
                            --LEFT JOIN TRN.InvoiceWriteOff IW ON IW.Id=IT.InvoiceWriteOffId
                            LEFT JOIN HKP.Activity TA ON TA.Id=ITD.ActivityId
                            LEFT JOIN HKP.Party P ON P.Id=IT.PartyId
							
                            LEFT JOIN MST.TaxCategory TC ON TC.Id=IT.TaxCategoryId
                            LEFT JOIN( select TAC.Id,TAC.UserName,TAC.IsRCM,TAY.[Type],TACD.ValueOfFixed from MST.TaxCode TAC
                            LEFT JOIN MST.TaxCodeYear TAY ON TAY.TaxCodeId=TAC.Id
                            LEFT JOIN MST.TaxCodeDetail TACD ON TACD.TaxCodeId=TAC.Id WHERE TAY.TaxYearId IN (" + taxyearId + @") 
							--and tac.IsRCM=0
							) TAXC ON TAXC.Id=IT.TaxCodeId
                            --LEFT JOIN SCS.TaxYear TY ON TY.Id=TAY.TaxYearId
                            LEFT JOIN TRN.InventoryReceive IR ON IR.VoucherId=V.Id
                            LEFT JOIN TRN.InventoryReceiveDetail IRD ON IRD.InventoryReceiveId=IR.Id
                            LEFT JOIN TRN.InventoryReceiveTax IRT ON IRD.Id=IRT.InventoryReceiveDetailId AND IRT.TaxCategoryId=IT.TaxCategoryId
                            --LEFT JOIN MST.HSNTaxPercentage HSNP ON IRT.HSNCodeId=HSNP.HSNCodeId AND HSNP.TaxCategoryId=IT.TaxCategoryId
                            LEFT JOIN TRN.InventoryMaterial IM ON IM.Id=IRD.InventoryMaterialId
                            LEFT JOIN MST.MaterialMaster MM ON MM.Id=IM.MaterialMasterId
                            LEFT JOIN TRN.VoucherDetail VD ON VD.Id=IT.VoucherDetailId
                            LEFT JOIN HKP.Activity A ON A.Id=VD.ActivityId
                            Left join hkp.PartyPlant pp on pp.Id=IR.InvoicingPartyPlantId
                            where TC.TaxCategoryType='GST' AND IR.IsTaxApplicable=0 AND V.IsPark=0
							AND V.PlantId = '" + plantId + "' and V.PostingDate between '" + fromDate + "' AND '" + toDate + @"'
                            AND v.SourceType='InventoryPayable' and IRT.InventoryServiceId IS NULL
                            
                            UNION all

                            SELECT SourceType = CASE WHEN V.SourceType = 'VendorInvoice' THEN 'Expense'
                            WHEN V.SourceType = 'VendorPayment' THEN 'Vendor Payment'
                            WHEN V.SourceType = 'InventoryPayable' THEN 'Service' ELSE '' END
                            ,V.VoucherNo,format(V.PostingDate, 'dd-MMM-yyyy')PostingDate, V.DocRefNo,format(V.DocDate, 'dd-MMM-yyyy')DocDate,P.UserName PartyName, PP.GSTIN
							, IRD.InventoryReceiveId GRNNo,pp.UserName PartyPlantName
                              , LineItemType =case when v.SourceType = 'InventoryPayable' then 'Service'
                            
                            ELSE '' END
                            ,Particular = CASE WHEN v.SourceType = 'InventoryPayable' THEN SM.UserName
                                ELSE '' END
                            ,TaxableAmount =case when v.SourceType = 'InventoryPayable' then IRD.Amount

                             else 0 end
                            ,IT.Id,DrAmount =case when ITD.AType = 'Dr' then IRT.TaxAmount else 0 end,0 CrAmount
	                        ,format(v.VoucherDate, 'dd-MMM-yyyy')VoucherDate
                            ,TC.TaxCategoryType,TC.Code TaxCode, TC.Sequence TCSequence, TC.UserName + '-' + TC.Code TaxCategory,IsNULL(TAXC.IsRCM, 0) IsRCM,TAXC.UserName TaxCodeName
                                   , IsNULL(IV.IsExcludingTax, 0) IsExcludingTax,IsNULL(IR.IsTaxApplicable, 0) IsTaxApplicable,TAXC.[Type],TAXC.ValueOfFixed
                            ,IRT.[Percentage],NULL HSNCodeId,null Material
							,TaxPercentage = case  when v.SourceType = 'InventoryPayable'  THEN IRT.[Percentage]

                                                 else 0 end
												 ,TA.UserName ActivityName,IRT.InventoryReceiveDetailId,IRT.InventoryServiceId
                            from TRN.InvoiceTax IT
                            left
                            join TRN.InvoiceTaxDetail ITD ON IT.Id = ITD.InvoiceTaxId AND ITD.AType = 'Dr'
                            LEFT JOIN TRN.Voucher V ON V.Id = IT.VoucherId
                            LEFT JOIN TRN.Invoice IV ON IV.Id = IT.InvoiceId
                            LEFT JOIN HKP.Activity TA ON TA.Id = ITD.ActivityId
                            LEFT JOIN HKP.Party P ON P.Id = IT.PartyId
							
                            LEFT JOIN MST.TaxCategory TC ON TC.Id = IT.TaxCategoryId
                            LEFT JOIN(select TAC.Id, TAC.UserName, TAC.IsRCM, TAY.[Type], TACD.ValueOfFixed from MST.TaxCode TAC
                            LEFT JOIN MST.TaxCodeYear TAY ON TAY.TaxCodeId= TAC.Id
                            LEFT JOIN MST.TaxCodeDetail TACD ON TACD.TaxCodeId= TAC.Id WHERE TAY.TaxYearId IN (" + taxyearId + @") 
							) TAXC ON TAXC.Id = IT.TaxCodeId
                            LEFT JOIN TRN.InventoryReceive IR ON IR.VoucherId = V.Id
                            LEFT JOIN TRN.InventoryReceiveTax IRT ON IRT.InventoryReceiveId = IR.Id AND IRT.TaxCategoryId = IT.TaxCategoryId
                            --LEFT JOIN MST.HSNTaxPercentage HSNP ON IRT.HSNCodeId = HSNP.HSNCodeId AND HSNP.TaxCategoryId = IT.TaxCategoryId
                            LEFT JOIN TRN.InventoryService IRD ON IRD.Id = IRT.InventoryServiceId
                            LEFT JOIN hkp.ServiceMaster SM ON SM.Id = IRD.ServiceMasterId
                             Left join hkp.PartyPlant pp on pp.Id=IR.InvoicingPartyPlantId

                            LEFT JOIN TRN.VoucherDetail VD ON VD.Id = IT.VoucherDetailId
                            LEFT JOIN HKP.Activity A ON A.Id = VD.ActivityId
                            LEFT JOIN(SELECT IW.InvoiceWriteOffId, IW.ActivityId, SUM(I.Amount) Amount FROM TRN.InvoiceWriteOffDetail IW
                            JOIN TRN.Invoice I ON I.Id= IW.InvoiceId
                            GROUP BY InvoiceWriteOffId, ActivityId) IWD ON IWD.InvoiceWriteOffId = IT.InvoiceWriteOffId
                            LEFT JOIN HKP.Activity AP ON AP.Id = IWD.ActivityId
                            where TC.TaxCategoryType = 'GST' AND IR.IsTaxApplicable = 0 AND V.IsPark = 0
							 AND V.PlantId = '" + plantId + "' and V.PostingDate between '" + fromDate + "' AND '" + toDate + @"'
                            AND v.SourceType = 'InventoryPayable'  and IRT.InventoryReceiveDetailId IS NULL
                            ) x
							ORDER BY TaxPercentage,VoucherNo, DocDate, ISNULL(InventoryReceiveDetailId,''),ISNULL(InventoryServiceId,'')  ";
            return _sqlRepository.GetDataTable(strSql);

        }
        private DataTable GetGSTReceivableSQL3(string companyGroupId, string companyId, string plantId, string plantName, string fromDate, string toDate, string taxyearId)
        {
            string strSql = "";
            strSql = @"SELECT	x.SourceType,x.VoucherNo,x.VoucherDate,x.PostingDate,x.DocRefNo,x.DocDate,x.PartyName,x.PartyPlantName,x.GSTIN
		,x.TaxCategoryType,x.TaxCode--,x.TaxPercentage
		,SUM(x.TaxableAmount) TaxableAmount,SUM(x.DrAmount) DrAmount,SUM(x.CrAmount) CrAmount
		,x.TCSequence,x.EntryDate,x.GRNNo
        ,x.PlaceofSupply,x.ReverseCharge,x.Suppliesundersection7ofIGSTAct,x.InvoiceType,x.ECommerceGSTIN
		,x.ItemName,x.HSNSAC,x.Rate,x.CessAmount,x.ApplicableofTaxRate
		FROM(SELECT	'Expenses' SourceType
                            ,V.VoucherNo,format( V.PostingDate,'dd-MMM-yyyy')PostingDate, V.DocRefNo,format (V.DocDate,'dd-MMM-yyyy')DocDate
							,P.UserName PartyName,PP.GSTIN
							,NULL GRNNo,pp.UserName PartyPlantName
                            ,LineItemType=case when v.SourceType='InventoryPayable' then 'Material'
                            WHEN v.SourceType='VendorInvoice' THEN 'GL'
                            WHEN v.SourceType='VendorPayment' THEN 'GL'
                            ELSE '' END
                            --,Particular=CASE WHEN v.SourceType='VendorInvoice' THEN A.UserName
                            --WHEN v.SourceType='VendorPayment' THEN AP.UserName
                            --ELSE '' END
                            ,TaxableAmount=case when v.SourceType='InventoryPayable' then 0
                            when v.SourceType='VendorInvoice' then ISNULL(VD.DrAmount,0)
                            when v.SourceType='VendorPayment' then ISNULL(IWD.Amount,0) else 0 end
                            ,DrAmount=case when ITD.AType='Dr' then ISNULL(IT.TaxAmount,0) else 0 end
							,0 CrAmount
	                        ,format( v.VoucherDate,'dd-MMM-yyyy')VoucherDate
                            ,TC.TaxCategoryType,TC.Code TaxCode,TC.Sequence TCSequence,TC.UserName+'-'+TC.Code TaxCategory,IsNULL(TAXC.IsRCM,0) IsRCM
							
                            ,IsNULL(IV.IsExcludingTax,0) IsExcludingTax,0 IsTaxApplicable,TAXC.[Type],TAXC.ValueOfFixed
                            ,0 [Percentage],NULL HSNCodeId,NULL Material
							,TaxPercentage= case when v.SourceType='VendorInvoice' then taxc.ValueOfFixed else 0 end, Format (IT.AddedDate,'dd-MMM-yyyy')EntryDate
                            ,NULL PlaceofSupply,0 ReverseCharge,NULL Suppliesundersection7ofIGSTAct,NULL InvoiceType,NULL ECommerceGSTIN
                            ,NULL ItemName,NULL HSNSAC,0 Rate,0 CessAmount,0 ApplicableofTaxRate

                            from TRN.InvoiceTax IT
                            left join TRN.InvoiceTaxDetail ITD ON IT.Id=ITD.InvoiceTaxId AND ITD.AType='Dr'
                            LEFT JOIN TRN.Voucher V ON V.Id=IT.VoucherId
                            LEFT JOIN TRN.Invoice IV ON IV.Id=IT.InvoiceId
                            LEFT JOIN HKP.Activity TA ON TA.Id=ITD.ActivityId
                            LEFT JOIN HKP.Party P ON P.Id=IT.PartyId
							Left join hkp.PartyPlant PP on PP.Id=IT.PartyPlantId
                            LEFT JOIN MST.TaxCategory TC ON TC.Id=IT.TaxCategoryId
                            LEFT JOIN( select distinct TAC.Id,TAC.UserName,TAC.IsRCM,TAY.[Type],TACD.ValueOfFixed from MST.TaxCode TAC
                            LEFT JOIN MST.TaxCodeYear TAY ON TAY.TaxCodeId=TAC.Id
                            LEFT JOIN MST.TaxCodeDetail TACD ON TACD.TaxCodeId=TAC.Id WHERE TAY.TaxYearId IN (" + taxyearId + @") ) TAXC ON TAXC.Id=IT.TaxCodeId
                            LEFT JOIN TRN.VoucherDetail VD ON VD.Id=IT.VoucherDetailId
                            LEFT JOIN HKP.Activity A ON A.Id=VD.ActivityId
                            LEFT JOIN (SELECT IW.InvoiceWriteOffId,IW.ActivityId,SUM(I.Amount) Amount FROM TRN.InvoiceWriteOffDetail IW
                            JOIN TRN.Invoice I ON I.Id=IW.InvoiceId
                            GROUP BY InvoiceWriteOffId,ActivityId) IWD ON IWD.InvoiceWriteOffId=IT.InvoiceWriteOffId
                            LEFT JOIN HKP.Activity AP ON AP.Id=IWD.ActivityId
                            where TC.TaxCategoryType='GST' AND TAXC.IsRCM=0 AND  V.IsPark=0 AND V.PlantId='" + plantId + @"'
							and V.PostingDate between '" + fromDate + "' AND '" + toDate + @"'
                            AND v.SourceType IN ('VendorInvoice','VendorPayment')
                            
                            UNION all

							SELECT 'GRN' SourceType
                            ,V.VoucherNo,format( V.PostingDate,'dd-MMM-yyyy')PostingDate, V.DocRefNo,format (V.DocDate,'dd-MMM-yyyy')DocDate,P.UserName PartyName,PP.GSTIN
							, IRD.InventoryReceiveId GRNNo,pp.UserName PartyPlantName
                            ,LineItemType=case when v.SourceType='InventoryPayable' then 'Material'
                            WHEN v.SourceType='VendorInvoice' THEN 'GL'
                            WHEN v.SourceType='VendorPayment' THEN 'GL'
                            ELSE '' END
                            
                            ,TaxableAmount=case when v.SourceType='InventoryPayable' then sum(ISNULL(IRD.TotalMaterialTranAmount,0))
                            else 0 end
                            ,DrAmount=case when ITD.AType='Dr' then sum(ISNULL(IRT.TaxAmount,0)) else 0 end,0 CrAmount
	                        ,format( v.VoucherDate,'dd-MMM-yyyy')VoucherDate
                            ,TC.TaxCategoryType,TC.Code TaxCode,TC.Sequence TCSequence,TC.UserName+'-'+TC.Code TaxCategory
							,IsNULL(TAXC.IsRCM,0) IsRCM
                            ,IsNULL(IV.IsExcludingTax,0) IsExcludingTax,IsNULL(IR.IsTaxApplicable,0) IsTaxApplicable
							,TAXC.[Type],TAXC.ValueOfFixed
                            ,IRT.[Percentage],NULL HSNCodeId,null Material
							,TaxPercentage= case  when v.SourceType='InventoryPayable' AND IRT.[Percentage]>0 THEN IRT.[Percentage]
							else 0 end,it.AddedDate EntryDate
                            ,NULL PlaceofSupply,0 ReverseCharge,NULL Suppliesundersection7ofIGSTAct,NULL InvoiceType,NULL ECommerceGSTIN
                            ,NULL ItemName,NULL HSNSAC,0 Rate,0 CessAmount,0 ApplicableofTaxRate

                            from TRN.InvoiceTax IT
                            left join TRN.InvoiceTaxDetail ITD ON IT.Id=ITD.InvoiceTaxId AND ITD.AType='Dr'
                            LEFT JOIN TRN.Voucher V ON V.Id=IT.VoucherId
                            LEFT JOIN TRN.Invoice IV ON IV.Id=IT.InvoiceId
                            --LEFT JOIN TRN.InvoiceWriteOff IW ON IW.Id=IT.InvoiceWriteOffId
                            LEFT JOIN HKP.Activity TA ON TA.Id=ITD.ActivityId
                            LEFT JOIN HKP.Party P ON P.Id=IT.PartyId
							
                            LEFT JOIN MST.TaxCategory TC ON TC.Id=IT.TaxCategoryId
                            LEFT JOIN( select TAC.Id,TAC.UserName,TAC.IsRCM,TAY.[Type],TACD.ValueOfFixed from MST.TaxCode TAC
                            LEFT JOIN MST.TaxCodeYear TAY ON TAY.TaxCodeId=TAC.Id
                            LEFT JOIN MST.TaxCodeDetail TACD ON TACD.TaxCodeId=TAC.Id WHERE TAY.TaxYearId IN (" + taxyearId + @") 
							--and tac.IsRCM=0
							) TAXC ON TAXC.Id=IT.TaxCodeId
                            --LEFT JOIN SCS.TaxYear TY ON TY.Id=TAY.TaxYearId
                            LEFT JOIN TRN.InventoryReceive IR ON IR.VoucherId=V.Id
                            LEFT JOIN TRN.InventoryReceiveDetail IRD ON IRD.InventoryReceiveId=IR.Id
                            LEFT JOIN TRN.InventoryReceiveTax IRT ON IRD.Id=IRT.InventoryReceiveDetailId AND IRT.TaxCategoryId=IT.TaxCategoryId
                            --LEFT JOIN MST.HSNTaxPercentage HSNP ON IRT.HSNCodeId=HSNP.HSNCodeId AND HSNP.TaxCategoryId=IT.TaxCategoryId
                            LEFT JOIN TRN.InventoryMaterial IM ON IM.Id=IRD.InventoryMaterialId
                            LEFT JOIN MST.MaterialMaster MM ON MM.Id=IM.MaterialMasterId
                            LEFT JOIN TRN.VoucherDetail VD ON VD.Id=IT.VoucherDetailId
                            LEFT JOIN HKP.Activity A ON A.Id=VD.ActivityId
                            Left join hkp.PartyPlant pp on pp.Id=IR.InvoicingPartyPlantId
                            where TC.TaxCategoryType='GST' AND IR.IsTaxApplicable=0 AND V.IsPark=0
							AND V.PlantId = '" + plantId + @"' and V.PostingDate between '" + fromDate + "' AND '" + toDate + @"'
                            AND v.SourceType='InventoryPayable' and IRT.InventoryServiceId IS NULL
                            GROUP BY 
							V.VoucherNo,V.PostingDate, V.DocRefNo,V.DocDate,P.UserName ,PP.GSTIN
							, IRD.InventoryReceiveId ,pp.UserName 
                            , v.SourceType
                            ,v.VoucherDate
                            ,TC.TaxCategoryType,TC.Code ,TC.Sequence ,TC.UserName,TC.Code
							,IsNULL(TAXC.IsRCM,0) 
                            ,IsNULL(IV.IsExcludingTax,0) ,IsNULL(IR.IsTaxApplicable,0) 
							,TAXC.[Type],TAXC.ValueOfFixed,ITD.AType
                            ,IRT.[Percentage],IRT.[Percentage] ,it.AddedDate 
                            

                             UNION all
                            SELECT 'GRN' SourceType
                            ,V.VoucherNo,format( V.PostingDate,'dd-MMM-yyyy')PostingDate, V.DocRefNo,format (V.DocDate,'dd-MMM-yyyy')DocDate,P.UserName PartyName,PP.GSTIN
							, IRD.InventoryReceiveId GRNNo,pp.UserName PartyPlantName
                            ,LineItemType=case when v.SourceType='InventoryPayable' then 'Material'
                            WHEN v.SourceType='VendorInvoice' THEN 'GL'
                            WHEN v.SourceType='VendorPayment' THEN 'GL'
                            ELSE '' END
                            
                            ,TaxableAmount=case when v.SourceType='InventoryPayable' then 0
                            else 0 end
                            ,DrAmount=case when ITD.AType='Dr' then sum(ISNULL(IRT.TaxAmount,0)) else 0 end,0 CrAmount
	                        ,format( v.VoucherDate,'dd-MMM-yyyy')VoucherDate
                            ,TC.TaxCategoryType,TC.Code TaxCode,TC.Sequence TCSequence,TC.UserName+'-'+TC.Code TaxCategory
							,IsNULL(TAXC.IsRCM,0) IsRCM
                            ,IsNULL(IV.IsExcludingTax,0) IsExcludingTax,IsNULL(IR.IsTaxApplicable,0) IsTaxApplicable
							,TAXC.[Type],TAXC.ValueOfFixed
                            ,IRT.[Percentage],NULL HSNCodeId,null Material
							,TaxPercentage= case  when v.SourceType='InventoryPayable' AND IRT.[Percentage]>0 THEN IRT.[Percentage]
							else 0 end,it.AddedDate EntryDate
                            ,NULL PlaceofSupply,0 ReverseCharge,NULL Suppliesundersection7ofIGSTAct,NULL InvoiceType,NULL ECommerceGSTIN
                            ,NULL ItemName,NULL HSNSAC,0 Rate,0 CessAmount,0 ApplicableofTaxRate

                            from TRN.InvoiceTax IT
                            left join TRN.InvoiceTaxDetail ITD ON IT.Id=ITD.InvoiceTaxId AND ITD.AType='Dr'
                            LEFT JOIN TRN.Voucher V ON V.Id=IT.VoucherId
                            LEFT JOIN TRN.Invoice IV ON IV.Id=IT.InvoiceId
                            --LEFT JOIN TRN.InvoiceWriteOff IW ON IW.Id=IT.InvoiceWriteOffId
                            LEFT JOIN HKP.Activity TA ON TA.Id=ITD.ActivityId
                            LEFT JOIN HKP.Party P ON P.Id=IT.PartyId
							
                            LEFT JOIN MST.TaxCategory TC ON TC.Id=IT.TaxCategoryId
                            LEFT JOIN( select TAC.Id,TAC.UserName,TAC.IsRCM,TAY.[Type],TACD.ValueOfFixed from MST.TaxCode TAC
                            LEFT JOIN MST.TaxCodeYear TAY ON TAY.TaxCodeId=TAC.Id
                            LEFT JOIN MST.TaxCodeDetail TACD ON TACD.TaxCodeId=TAC.Id WHERE TAY.TaxYearId IN (" + taxyearId + @")
							--and tac.IsRCM=0
							) TAXC ON TAXC.Id=IT.TaxCodeId
                            --LEFT JOIN SCS.TaxYear TY ON TY.Id=TAY.TaxYearId
                            LEFT JOIN TRN.InventoryReceive IR ON IR.VoucherId=V.Id
                            LEFT JOIN TRN.InventoryService IRD ON IRD.InventoryReceiveId=IR.Id
                            LEFT JOIN TRN.InventoryReceiveTax IRT ON IRD.Id=IRT.InventoryServiceId AND IRT.TaxCategoryId=IT.TaxCategoryId
                            LEFT JOIN TRN.VoucherDetail VD ON VD.Id=IT.VoucherDetailId
                            LEFT JOIN HKP.Activity A ON A.Id=VD.ActivityId
                            Left join hkp.PartyPlant pp on pp.Id=IR.InvoicingPartyPlantId
                            where TC.TaxCategoryType='GST' AND IR.IsTaxApplicable=0 AND V.IsPark=0
							AND V.PlantId = '" + plantId + @"' and V.PostingDate between '" + fromDate + "' AND '" + toDate + @"'


                            AND v.SourceType='InventoryPayable'  and IRT.InventoryServiceId<>''


                            GROUP BY 
							V.VoucherNo,V.PostingDate, V.DocRefNo,V.DocDate,P.UserName ,PP.GSTIN
							, IRD.InventoryReceiveId ,pp.UserName 
                            , v.SourceType
                            ,v.VoucherDate
                            ,TC.TaxCategoryType,TC.Code ,TC.Sequence ,TC.UserName,TC.Code
							,IsNULL(TAXC.IsRCM,0) 
                            ,IsNULL(IV.IsExcludingTax,0) ,IsNULL(IR.IsTaxApplicable,0) 
							,TAXC.[Type],TAXC.ValueOfFixed,ITD.AType
                            ,IRT.[Percentage],IRT.[Percentage] ,it.AddedDate

UNION ALL

							--****************TCS*********************************
                            SELECT 'GRN' SourceType
                            ,V.VoucherNo,format( V.PostingDate,'dd-MMM-yyyy')PostingDate, V.DocRefNo,format (V.DocDate,'dd-MMM-yyyy')DocDate,P.UserName PartyName,PP.GSTIN
							, IRD.InventoryReceiveId GRNNo,pp.UserName PartyPlantName
                            ,LineItemType=case when v.SourceType='InventoryPayable' then 'Material'
                            WHEN v.SourceType='VendorInvoice' THEN 'GL'
                            WHEN v.SourceType='VendorPayment' THEN 'GL'
                            ELSE '' END
                            
                            ,TaxableAmount=case when v.SourceType='InventoryPayable' then 0
                            else 0 end
                            ,DrAmount=case when ITD.AType='Dr' then sum(ISNULL(ITD.Amount,0)) else 0 end,0 CrAmount
	                        ,format( v.VoucherDate,'dd-MMM-yyyy')VoucherDate
                            ,TC.TaxCategoryType,TC.Code TaxCode,TC.Sequence TCSequence,TC.UserName+'-'+TC.Code TaxCategory
							,IsNULL(TAXC.IsRCM,0) IsRCM
                            ,IsNULL(IV.IsExcludingTax,0) IsExcludingTax,IsNULL(IR.IsTaxApplicable,0) IsTaxApplicable
							,TAXC.[Type],TAXC.ValueOfFixed
                            ,NULL [Percentage],NULL HSNCodeId,null Material
							,NULL TaxPercentage,it.AddedDate EntryDate
                            ,NULL PlaceofSupply,0 ReverseCharge,NULL Suppliesundersection7ofIGSTAct,NULL InvoiceType,NULL ECommerceGSTIN
                            ,NULL ItemName,NULL HSNSAC,0 Rate,0 CessAmount,0 ApplicableofTaxRate

                            from TRN.InvoiceTax IT
                            left join TRN.InvoiceTaxDetail ITD ON IT.Id=ITD.InvoiceTaxId AND ITD.AType='Dr'
                            LEFT JOIN TRN.Voucher V ON V.Id=IT.VoucherId
                            LEFT JOIN TRN.Invoice IV ON IV.Id=IT.InvoiceId
                            --LEFT JOIN TRN.InvoiceWriteOff IW ON IW.Id=IT.InvoiceWriteOffId
                            LEFT JOIN HKP.Activity TA ON TA.Id=ITD.ActivityId
                            LEFT JOIN HKP.Party P ON P.Id=IT.PartyId
							
                            LEFT JOIN MST.TaxCategory TC ON TC.Id=IT.TaxCategoryId
                            LEFT JOIN( select TAC.Id,TAC.UserName,TAC.IsRCM,TAY.[Type],TACD.ValueOfFixed from MST.TaxCode TAC
                            LEFT JOIN MST.TaxCodeYear TAY ON TAY.TaxCodeId=TAC.Id
                            LEFT JOIN MST.TaxCodeDetail TACD ON TACD.TaxCodeId=TAC.Id WHERE TAY.TaxYearId IN (" + taxyearId + @")
							--and tac.IsRCM=0
							) TAXC ON TAXC.Id=IT.TaxCodeId
                            --LEFT JOIN SCS.TaxYear TY ON TY.Id=TAY.TaxYearId
                            LEFT JOIN TRN.InventoryReceive IR ON IR.VoucherId=V.Id
                            LEFT JOIN TRN.InventoryReceiveDetail IRD ON IRD.InventoryReceiveId=IR.Id
                            LEFT JOIN TRN.VoucherDetail VD ON VD.Id=IT.VoucherDetailId
                            LEFT JOIN HKP.Activity A ON A.Id=VD.ActivityId
                            Left join hkp.PartyPlant pp on pp.Id=IR.InvoicingPartyPlantId
                            where TC.TaxCategoryType='TCS' AND IR.IsTaxApplicable=0 AND V.IsPark=0
							AND V.PlantId = '" + plantId + @"' and V.PostingDate between '" + fromDate + "' AND '" + toDate + @"'



                            AND v.SourceType='InventoryPayable'  


                            GROUP BY 
							V.VoucherNo,V.PostingDate, V.DocRefNo,V.DocDate,P.UserName ,PP.GSTIN
							, IRD.InventoryReceiveId ,pp.UserName 
                            , v.SourceType
                            ,v.VoucherDate
                            ,TC.TaxCategoryType,TC.Code ,TC.Sequence ,TC.UserName,TC.Code
							,IsNULL(TAXC.IsRCM,0) 
                            ,IsNULL(IV.IsExcludingTax,0) ,IsNULL(IR.IsTaxApplicable,0) 
							,TAXC.[Type],TAXC.ValueOfFixed,ITD.AType
                           ,it.AddedDate 

			                UNION	ALL			
                            SELECT 'Service' SourceType
                            ,V.VoucherNo,format(V.PostingDate, 'dd-MMM-yyyy')PostingDate, V.DocRefNo,format(V.DocDate, 'dd-MMM-yyyy')DocDate,P.UserName PartyName, PP.GSTIN
							, IRD.ServiceAcknowledgementMasterId GRNNo,pp.UserName PartyPlantName
                              , LineItemType =case when v.SourceType = 'ServicePayable' then 'Service' ELSE '' END
                            
                            ,TaxableAmount =case when v.SourceType = 'ServicePayable' then ISNULL(IRD.Amount,0)

                             else 0 end
                            ,DrAmount =case when ITD.AType = 'Dr' then ISNULL(IRT.TaxAmount,0) else 0 end,0 CrAmount
	                        ,format(v.VoucherDate, 'dd-MMM-yyyy')VoucherDate
                            ,TC.TaxCategoryType,TC.Code TaxCode, TC.Sequence TCSequence, TC.UserName + '-' + TC.Code TaxCategory,IsNULL(TAXC.IsRCM, 0) IsRCM
                                   , IsNULL(IV.IsExcludingTax, 0) IsExcludingTax,IsNULL(IR.IsTaxApplicable, 0) IsTaxApplicable,TAXC.[Type],TAXC.ValueOfFixed
                            ,IRT.[Percentage],NULL HSNCodeId,null Material
							,TaxPercentage = case  when v.SourceType = 'InventoryPayable'  THEN IRT.[Percentage]else 0 end,IT.AddedDate EntryDate
                            ,NULL PlaceofSupply,0 ReverseCharge,NULL Suppliesundersection7ofIGSTAct,NULL InvoiceType,NULL ECommerceGSTIN
                            ,NULL ItemName,NULL HSNSAC,0 Rate,0 CessAmount,0 ApplicableofTaxRate

                            from TRN.InvoiceTax IT
                            left
                            join TRN.InvoiceTaxDetail ITD ON IT.Id = ITD.InvoiceTaxId AND ITD.AType = 'Dr'
                            LEFT JOIN TRN.Voucher V ON V.Id = IT.VoucherId
                            LEFT JOIN TRN.Invoice IV ON IV.Id = IT.InvoiceId
                            LEFT JOIN HKP.Activity TA ON TA.Id = ITD.ActivityId
                            LEFT JOIN HKP.Party P ON P.Id = IT.PartyId
							
                            LEFT JOIN MST.TaxCategory TC ON TC.Id = IT.TaxCategoryId
                            LEFT JOIN(select TAC.Id, TAC.UserName, TAC.IsRCM, TAY.[Type], TACD.ValueOfFixed from MST.TaxCode TAC
                            LEFT JOIN MST.TaxCodeYear TAY ON TAY.TaxCodeId= TAC.Id
                            LEFT JOIN MST.TaxCodeDetail TACD ON TACD.TaxCodeId= TAC.Id WHERE TAY.TaxYearId IN (" + taxyearId + @") 
							) TAXC ON TAXC.Id = IT.TaxCodeId
                            LEFT JOIN TRN.ServiceAcknowledgementMaster IR ON IR.VoucherId = V.Id
                            LEFT JOIN TRN.ServicePOAckTax IRT ON IRT.ServiceAcknowledgementMasterId = IR.Id AND IRT.TaxCategoryId = IT.TaxCategoryId
                            --LEFT JOIN MST.HSNTaxPercentage HSNP ON IRT.HSNCodeId = HSNP.HSNCodeId AND HSNP.TaxCategoryId = IT.TaxCategoryId
                            LEFT JOIN TRN.ServiceAcknowledgementDetail IRD ON IRD.Id = IRT.ServiceAcknowledgementDetailId
                            LEFT JOIN hkp.ServiceMaster SM ON SM.Id = IRD.ServiceMasterId
                             Left join hkp.PartyPlant pp on pp.Id=IR.InvoicingPartyPlantId

                            LEFT JOIN TRN.VoucherDetail VD ON VD.Id = IT.VoucherDetailId
                            LEFT JOIN HKP.Activity A ON A.Id = VD.ActivityId
                            LEFT JOIN(SELECT IW.InvoiceWriteOffId, IW.ActivityId, SUM(I.Amount) Amount FROM TRN.InvoiceWriteOffDetail IW
                            JOIN TRN.Invoice I ON I.Id= IW.InvoiceId
                            GROUP BY InvoiceWriteOffId, ActivityId) IWD ON IWD.InvoiceWriteOffId = IT.InvoiceWriteOffId
                            LEFT JOIN HKP.Activity AP ON AP.Id = IWD.ActivityId
                            where TC.TaxCategoryType = 'GST' AND IR.IsTaxApplicable = 0 AND V.IsPark = 0
							 AND V.PlantId = '" + plantId + @"' and V.PostingDate between '" + fromDate + "' AND '" + toDate + @"'
                            AND v.SourceType = 'ServicePayable' 
                            ) x
							group by x.VoucherNo,x.VoucherDate,x.PostingDate,x.DocRefNo,x.DocDate,x.PartyName
							,x.TCSequence,x.PartyPlantName,x.GSTIN,x.SourceType
							,x.TaxCategoryType,x.EntryDate,x.TaxCode,x.GRNNo 
                            ,x.PlaceofSupply,x.ReverseCharge,x.Suppliesundersection7ofIGSTAct,x.InvoiceType,x.ECommerceGSTIN
					        ,x.ItemName,x.HSNSAC,x.Rate,x.CessAmount,x.ApplicableofTaxRate
							ORDER BY 1,2,4-- TaxPercentage,VoucherNo, DocDate ";
            return _sqlRepository.GetDataTable(strSql);

        }

        private DataTable GetDebitNoteCreditNoteTaxSQL(string companyGroupId, string companyId, string plantId, string plantName, string fromDate, string toDate, string taxyearId)
        {
            string strSql = "";
            strSql = @"SELECT  V.SourceType ,V.VoucherNo,format( V.PostingDate,'dd-MMM-yyyy')PostingDate, V.DocRefNo,format (V.DocDate,'dd-MMM-yyyy')DocDate
							,P.UserName PartyName,PP.GSTIN,NULL GRNNo,pp.UserName PartyPlantName
                            ,TaxableAmount=case 
							when v.SourceType='DebitNote' then (select sum(CrAmount) from trn.VoucherDetail where VoucherId=V.Id and CrAmount>0 and  InvoiceTaxDetailId is null)
							when v.SourceType='CreditNote' then (select sum(DrAmount) from trn.VoucherDetail where VoucherId=V.Id and DrAmount>0 and  InvoiceTaxDetailId is null)
                            when v.SourceType='InventoryReturnPayable'  OR v.SourceType='VendorPayment' then (select sum(CrAmount) from trn.VoucherDetail where VoucherId=V.Id and CrAmount>0 and  Id NOT IN(select VoucherDetailId from TRN.InvoiceTax where VoucherId=V.Id))
							else 0 end
							  ,TotalAmount=case 
							when v.SourceType='DebitNote' then (select sum(CrAmount) from trn.VoucherDetail where VoucherId=V.Id and CrAmount>0)
							when v.SourceType='CreditNote' then (select sum(DrAmount) from trn.VoucherDetail where VoucherId=V.Id and DrAmount>0)
							when v.SourceType='InventoryReturnPayable' OR v.SourceType='VendorPayment' then (select sum(CrAmount) from trn.VoucherDetail where VoucherId=V.Id and CrAmount>0)
                            else 0 end
                            ,IsNULL(IGST.TaxAmount,0) IGSTAmount,IsNULL(CGST.TaxAmount,0) CGSTAmount,IsNULL(SGST.TaxAmount,0) SGSTAmount
							
	                        ,format( v.VoucherDate,'dd-MMM-yyyy')VoucherDate
							,IsNULL(IGST.IsRCM,0) IsRCM
                            ,0 IsTaxApplicable,IGST.[Type]
                            ,[Percentage]=CASE WHEN IGST.ValueOfFixed<>0 THEN IGST.ValueOfFixed WHEN SGST.ValueOfFixed<>0 THEN SGST.ValueOfFixed ELSE CGST.ValueOfFixed END
							, Format (V.AddedDate,'dd-MMM-yyyy')EntryDate,ADT.PartyType
							,ParkStatus = case when V.IsPark=1 then 'Parked' else 'Posted' end,ADT.WrittenOffAmount
                            FROM   TRN.AdjustmentNote ADT
							LEFT JOIN TRN.Voucher V  ON ADT.VoucherId=V.Id
                            LEFT JOIN HKP.Party P ON P.Id=ADT.PartyId
							LEFT JOIN hkp.PartyPlant PP on PP.Id=ADT.PartyPlantId
							LEFT JOIN(select IT.VoucherId,TCD.ValueOfFixed [Percentage],TAXC.[Type],TAXC.ValueOfFixed,TC.TaxCategoryType
							,TC.Code,TC.[Sequence],TC.UserName,IsNULL(TAXC.IsRCM,0) IsRCM,SUM(ISNULL(IT.TaxAmount,0)) TaxAmount
							from TRN.InvoiceTax IT
							LEFT JOIN TRN.InvoiceTaxDetail ITD ON IT.Id=ITD.InvoiceTaxId
							LEFT JOIN MST.TaxCategory TC ON TC.Id=IT.TaxCategoryId
                            LEFT JOIN MST.TaxCodeYear TY ON TY.TaxCodeId=IT.TaxCodeId AND TY.TaxYearId=IT.TaxYearId AND TY.Active=1
                            LEFT JOIN MST.TaxCodeDetail TCD ON TCD.TaxCodeYearId=TY.Id AND TCD.TaxCodeId=TY.TaxCodeId
                            LEFT JOIN( select distinct TAC.Id,TAC.UserName,TAC.IsRCM,TAY.[Type],TACD.ValueOfFixed from MST.TaxCode TAC
                            LEFT JOIN MST.TaxCodeYear TAY ON TAY.TaxCodeId=TAC.Id
                            LEFT JOIN MST.TaxCodeDetail TACD ON TACD.TaxCodeId=TAC.Id WHERE TAY.TaxYearId IN (" + taxyearId + @") ) TAXC ON TAXC.Id=IT.TaxCodeId
							where TC.Code='IGST'
							GROUP BY IT.VoucherId,TCD.ValueOfFixed,TAXC.[Type],TAXC.ValueOfFixed,TC.TaxCategoryType
							,TC.Code,TC.[Sequence],TC.UserName,TAXC.IsRCM
							)IGST ON ADT.VoucherId=IGST.VoucherId
                            LEFT JOIN(select IT.VoucherId,TCD.ValueOfFixed [Percentage],TAXC.[Type],TAXC.ValueOfFixed,TC.TaxCategoryType
							,TC.Code,TC.[Sequence],TC.UserName,IsNULL(TAXC.IsRCM,0) IsRCM,SUM(ISNULL(IT.TaxAmount,0)) TaxAmount
							from TRN.InvoiceTax IT
							LEFT JOIN TRN.InvoiceTaxDetail ITD ON IT.Id=ITD.InvoiceTaxId
							LEFT JOIN MST.TaxCategory TC ON TC.Id=IT.TaxCategoryId
                            LEFT JOIN MST.TaxCodeYear TY ON TY.TaxCodeId=IT.TaxCodeId AND TY.TaxYearId=IT.TaxYearId AND TY.Active=1
                            LEFT JOIN MST.TaxCodeDetail TCD ON TCD.TaxCodeYearId=TY.Id AND TCD.TaxCodeId=TY.TaxCodeId
                            LEFT JOIN( select distinct TAC.Id,TAC.UserName,TAC.IsRCM,TAY.[Type],TACD.ValueOfFixed from MST.TaxCode TAC
                            LEFT JOIN MST.TaxCodeYear TAY ON TAY.TaxCodeId=TAC.Id
                            LEFT JOIN MST.TaxCodeDetail TACD ON TACD.TaxCodeId=TAC.Id WHERE TAY.TaxYearId IN (" + taxyearId + @") ) TAXC ON TAXC.Id=IT.TaxCodeId
							where TC.Code='CGST'
							GROUP BY IT.VoucherId,TCD.ValueOfFixed,TAXC.[Type],TAXC.ValueOfFixed,TC.TaxCategoryType
							,TC.Code,TC.[Sequence],TC.UserName,TAXC.IsRCM
							)CGST ON ADT.VoucherId=CGST.VoucherId
							LEFT JOIN(select IT.VoucherId,TCD.ValueOfFixed [Percentage],TAXC.[Type],TAXC.ValueOfFixed,TC.TaxCategoryType
							,TC.Code,TC.[Sequence],TC.UserName,IsNULL(TAXC.IsRCM,0) IsRCM,SUM(ISNULL(IT.TaxAmount,0)) TaxAmount
							from TRN.InvoiceTax IT
							LEFT JOIN TRN.InvoiceTaxDetail ITD ON IT.Id=ITD.InvoiceTaxId
							LEFT JOIN MST.TaxCategory TC ON TC.Id=IT.TaxCategoryId
                            LEFT JOIN MST.TaxCodeYear TY ON TY.TaxCodeId=IT.TaxCodeId AND TY.TaxYearId=IT.TaxYearId AND TY.Active=1
                            LEFT JOIN MST.TaxCodeDetail TCD ON TCD.TaxCodeYearId=TY.Id AND TCD.TaxCodeId=TY.TaxCodeId
                            LEFT JOIN( select distinct TAC.Id,TAC.UserName,TAC.IsRCM,TAY.[Type],TACD.ValueOfFixed from MST.TaxCode TAC
                            LEFT JOIN MST.TaxCodeYear TAY ON TAY.TaxCodeId=TAC.Id
                            LEFT JOIN MST.TaxCodeDetail TACD ON TACD.TaxCodeId=TAC.Id WHERE TAY.TaxYearId IN (" + taxyearId + @") ) TAXC ON TAXC.Id=IT.TaxCodeId
							where TC.Code='SGST'
							GROUP BY IT.VoucherId,TCD.ValueOfFixed,TAXC.[Type],TAXC.ValueOfFixed,TC.TaxCategoryType
							,TC.Code,TC.[Sequence],TC.UserName,TAXC.IsRCM
							)SGST ON ADT.VoucherId=SGST.VoucherId
							
                            where V.PlantId='" + plantId + @"'
							and V.PostingDate BETWEEN '" + fromDate + "' AND '" + toDate + @"'
                            AND v.SourceType IN ('DebitNote','CreditNote','InventoryReturnPayable','VendorPayment') ";
            return _sqlRepository.GetDataTable(strSql);
        }
        private DataTable GetAdvancePaymentPendingforSetOffReportSQL(string companyGroupId, string companyId, string plantId)
        {
            string strSql = "";
            strSql = @"Declare @CompanyGroupId nvarchar(50)='" + companyGroupId + @"',@CompanyId nvarchar(50)='" + companyId + @"',@PlantId nvarchar(50)='" + plantId + @"'

SELECT  X.PartyType,X.VoucherRowId,X.VoucherNo,X.EmployeeName PartyName,X.PostingDate,X.DocDate,X.ReviewDate,X.DocRefNo,X.Narration
,X.GL ,X.Budget ,X.Activity ,X.ResponsiblePerson,X.PaymentSource,x.BankName,x.CashName,X.CurrencyCode
,X.Receivable,X.Received,X.Balance,X.Receivable BookReceivable,X.Received BookReceived,X.Balance BookBalance
FROM (SELECT AD.AdvanceId, AD.Id AS AdvanceDetailId, AD.PartyType, AD.CompanyId, AD.PlantId, AM.PartyId, AM.PartyPlantId, PP.UserName AS PartyPlantName, AM.AdvanceNo, AM.VoucherId, VD.Id AS VoucherRowId, VD.EntityId
								, EN.UserName AS EntityName, AM.CurrencyId, C.Code AS CurrencyCode, AD.GLGeneralInfoId AS GLGeneralInfoId, GLGI.AccountCode AS GLGeneralInfoCode, GLGI.UserName AS GL , AM.EmployeeId, EI.EmployeeCode, EI.EmployeeName
								, AD.BudgetMasterId, B.Code AS BudgetCode, B.UserName AS Budget , AD.ActivityId, A.Code AS ActivityCode, A.UserName AS Activity , V.VoucherNo, Replace(CONVERT(VARCHAR(11), AM.DocDate, 106), ' ', '-') AS DocDate
                                , Replace(CONVERT(VARCHAR(11), AM.PostingDate, 106), ' ', '-') AS PostingDate, AM.DocRefNo, AM.Narration, AD.Amount AS Receivable, AD.WrittenOffAmount+ISNULL(SAVW.SalaryWrittenOffAmount,0) AS Received, 0 DrAmount, 0 CrAmount
                                , AD.Amount-AD.WrittenOffAmount-ISNULL(SAVW.SalaryWrittenOffAmount,0)AS Balance,ETT.UserName EmployeeTransactionTypeName
                                ,CASE WHEN ETT.UserName='Employee Salary' THEN 'Salary' ELSE 'General' END JournalType,AM.PaymentSource,EIR.EmployeeName ResponsiblePerson , Replace(CONVERT(VARCHAR(11), AM.ReviewDate, 106), ' ', '-') AS ReviewDate
								,BKM.AccountTitle BankName,CKM.UserName CashName
                                FROM [TRN].[AdvanceDetail] AS AD
                                LEFT JOIN [TRN].[Advance] AS AM ON AD.AdvanceId=AM.Id
                                LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.AdvanceDetailId=AD.Id
                                LEFT JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
								LEFT JOIN [MST].[BankMaster] AS BKM ON BKM.Id=AM.BankMasterId
								LEFT JOIN [MST].[CashMaster] AS CKM ON CKM.Id=AM.CashMasterId
                                LEFT JOIN [dbo].[EmployeeInformation] AS EI ON EI.SystemId=AM.EmployeeId
                                LEFT JOIN [dbo].[EmployeeInformation] AS EIR ON EIR.SystemId=AM.ResponsiblePersonId
                                LEFT JOIN [HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id=AD.GLGeneralInfoId
                                LEFT JOIN [MST].[BudgetMaster] AS BM ON BM.Id=AD.BudgetMasterId
                                LEFT JOIN [HKP].[Budget] AS B ON B.Id=BM.BudgetId
                                LEFT JOIN [HKP].[Activity] AS A ON A.Id=AD.ActivityId
                                LEFT JOIN [SCS].[Currency] AS C ON C.Id=AM.CurrencyId
                                LEFT JOIN [ORG].[Entity] AS EN ON EN.Id=AM.EntityId
                                LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=AM.PartyPlantId
                                LEFT JOIN [HKP].[EmployeeTransactionType] AS ETT ON ETT.Id=AM.EmployeeTransactionTypeId
								LEFT JOIN (SELECT SUM(ARS.InstallmentAmount)SalaryWrittenOffAmount,ADV.Id AdvanceId
                                    FROM  [TRN].EmployeeAdvanceDeduction EAD 
                                    LEFT JOIN dbo.AdvanceReqSchedule  ARS ON EAD.AdvanceReqScheduleId=ARS.Id
                                    INNER JOIN [TRN].EmployeeSalaryAdvance ESA ON ESA.Id=ARS.EmployeeSalaryAdvanceId
                                    INNER JOIN [TRN].[Advance] ADV ON ADV.VoucherId=ESA.VoucherId
                                    LEFT JOIN DBO.SalaryLock SL ON SL.YearNo=ARS.YearNo AND SL.MonthNo=ARS.MonthNo AND SL.EmpSystemId=ESA.EmployeeId AND SL.PayableVoucherId IS NULL
									GROUP BY ADV.Id) SAVW ON SAVW.AdvanceId=AD.AdvanceId
								
                                WHERE AM.Archive=0 AND AM.IsPosted=1 AND AM.IsWrittenOff=0 AND AD.IsWrittenOff=0 
								AND AM.SourceType in ('EmployeeAdvance','InterTransaction')
                                AND AM.CompanyGroupId=@CompanyGroupId AND AM.CompanyId=@CompanyId AND AM.PlantId=@PlantId AND AM.EmployeeId<>'' )X

UNION ALL
SELECT  X.PartyType,X.VoucherRowId,X.VoucherNo,X.PartyName,X.PostingDate,X.DocDate,X.ReviewDate,X.DocRefNo,X.Narration
,X.GL ,X.Budget ,X.Activity ,X.ResponsiblePerson,X.PaymentSource,x.BankName,x.CashName,X.CurrencyCode
,X.Receivable,X.Received,X.Balance,X.BookReceivable,X.BookReceived,X.BookBalance
FROM (SELECT AD.AdvanceId, AD.Id AS AdvanceDetailId, AD.PartyType, AD.CompanyId, AD.PlantId, AM.PartyId, AM.PartyPlantId,P.Code AS  PartyCode, P.UserName As PartyName, PP.UserName AS PartyPlantName, AM.AdvanceNo, AM.VoucherId, VD.Id AS VoucherRowId, VD.EntityId
								, EN.UserName AS EntityName, AM.CurrencyId, C.Code AS CurrencyCode, AD.GLGeneralInfoId AS GLGeneralInfoId, GLGI.AccountCode AS GLGeneralInfoCode, GLGI.UserName AS GL
								, AD.BudgetMasterId, B.Code AS BudgetCode, B.UserName AS Budget, AD.ActivityId, A.Code AS ActivityCode, A.UserName AS Activity, V.VoucherNo, Replace(CONVERT(VARCHAR(11), AM.DocDate, 106), ' ', '-') AS DocDate
                                , Replace(CONVERT(VARCHAR(11), AM.PostingDate, 106), ' ', '-') AS PostingDate, AM.DocRefNo, AM.Narration,AM.PaymentSource,EIR.EmployeeName ResponsiblePerson , Replace(CONVERT(VARCHAR(11), AM.ReviewDate, 106), ' ', '-') AS ReviewDate
								,BKM.AccountTitle BankName,CKM.UserName CashName, AD.Amount AS Receivable, AD.WrittenOffAmount AS Received  , AD.Amount-AD.WrittenOffAmount AS Balance, CC.CompanyCurrencyRate,CC.CompanyCurrencyAmount BookReceivable ,ISNULL(AW.AdvanceWriteOffBooksAmount,0)BookReceived
								,ISNULL(CC.CompanyCurrencyAmount,0)- ISNULL(AW.AdvanceWriteOffBooksAmount,0)BookBalance
                                FROM [TRN].[AdvanceDetail] AS AD
                                LEFT JOIN [TRN].[Advance] AS AM ON AD.AdvanceId=AM.Id
                                LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.AdvanceDetailId=AD.Id
                                LEFT JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
								LEFT JOIN [MST].[BankMaster] AS BKM ON BKM.Id=AM.BankMasterId
								LEFT JOIN [MST].[CashMaster] AS CKM ON CKM.Id=AM.CashMasterId
								LEFT JOIN [dbo].[EmployeeInformation] AS EIR ON EIR.SystemId=AM.ResponsiblePersonId
                                LEFT JOIN [HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id=AD.GLGeneralInfoId
                                LEFT JOIN [MST].[BudgetMaster] AS BM ON BM.Id=AD.BudgetMasterId
                                LEFT JOIN [HKP].[Budget] AS B ON B.Id=BM.BudgetId
                                LEFT JOIN [HKP].[Activity] AS A ON A.Id=AD.ActivityId
                                LEFT JOIN [SCS].[Currency] AS C ON C.Id=AM.CurrencyId
                                LEFT JOIN [ORG].[Entity] AS EN ON EN.Id=AM.EntityId
                                LEFT JOIN [HKP].[Party] AS P ON P.Id=AM.PartyId
                                LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=AM.PartyPlantId
								LEFT JOIN (
								    SELECT VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.FromCurrencyId AS CompanyFromCurrencyId, VDC.ToCurrencyId,
								    VDC.ToCurrencyRate AS CompanyCurrencyRate, VDC.ToCurrencyConversion AS CompanyCurrencyConversion, VDC.DrAmount AS CompanyCurrencyAmount, VDC.VoucherDetailId
								    FROM [TRN].[VoucherDetailCurrency] AS VDC
								    JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
								    WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId=@CompanyId
							    ) AS CC ON CC.VoucherDetailId=VD.Id
								 LEFT JOIN (select SUM(VDCW.CrAmount)AdvanceWriteOffBooksAmount,AdvanceId from [TRN].[AdvanceWriteOffDetail] AWD
														INNER JOIN  [TRN].[VoucherDetail] VDW ON VDW.AdvanceWriteOffDetailId=AWD.Id
														INNER JOIN  [TRN].[VoucherDetailCurrency] AS VDCW ON VDCW.VoucherDetailId=VDW.Id
														LEFT JOIN [TRN].[AdvanceWriteOff] AW ON AW.Id=AWD.AdvanceWriteOffId WHERE AW.IsPark=0 AND AW.Archive=0 GROUP BY AdvanceId)AW ON AW.AdvanceId=AD.AdvanceId
							  
                                WHERE AM.Archive=0 AND AM.IsPosted=1 AND AM.IsWrittenOff=0 AND AD.IsWrittenOff=0 AND AM.SourceType='VendorAdvance' AND AM.PartyType='Vendor' 
                                AND AM.CompanyGroupId=@CompanyGroupId AND AM.CompanyId=@CompanyId AND AM.PlantId=@PlantId  )X
								
UNION ALL
SELECT  X.PartyType,X.VoucherRowId,X.VoucherNo,X.PartyName,X.PostingDate,X.DocDate,X.ReviewDate,X.DocRefNo,X.Narration
,X.GL ,X.Budget ,X.Activity ,X.ResponsiblePerson,X.PaymentSource,x.BankName,x.CashName,X.CurrencyCode
,X.Receivable,X.Received,X.Balance,X.BookReceivable,X.BookReceived,X.BookBalance
FROM (SELECT AD.AdvanceId, AD.Id AS AdvanceDetailId, AD.PartyType, AD.CompanyId, AD.PlantId, AM.PartyId, AM.PartyPlantId,P.Code AS  PartyCode, P.UserName As PartyName, PP.UserName AS PartyPlantName, AM.AdvanceNo, AM.VoucherId, VD.Id AS VoucherRowId, VD.EntityId
								, EN.UserName AS EntityName, AM.CurrencyId, C.Code AS CurrencyCode, AD.GLGeneralInfoId AS GLGeneralInfoId, GLGI.AccountCode AS GLGeneralInfoCode, GLGI.UserName AS GL
								, AD.BudgetMasterId, B.Code AS BudgetCode, B.UserName AS Budget, AD.ActivityId, A.Code AS ActivityCode, A.UserName AS Activity, V.VoucherNo, Replace(CONVERT(VARCHAR(11), AM.DocDate, 106), ' ', '-') AS DocDate
                                , Replace(CONVERT(VARCHAR(11), AM.PostingDate, 106), ' ', '-') AS PostingDate, AM.DocRefNo, AM.Narration,AM.PaymentSource,EIR.EmployeeName ResponsiblePerson , Replace(CONVERT(VARCHAR(11), AM.ReviewDate, 106), ' ', '-') AS ReviewDate
								,BKM.AccountTitle BankName,CKM.UserName CashName, AD.Amount AS Receivable, AD.WrittenOffAmount AS Received  , AD.Amount-AD.WrittenOffAmount AS Balance, CC.CompanyCurrencyRate,CC.CompanyCurrencyAmount BookReceivable ,ISNULL(AW.AdvanceWriteOffBooksAmount,0)BookReceived
								,ISNULL(CC.CompanyCurrencyAmount,0)- ISNULL(AW.AdvanceWriteOffBooksAmount,0)BookBalance
                                FROM [TRN].[AdvanceDetail] AS AD
                                LEFT JOIN [TRN].[Advance] AS AM ON AD.AdvanceId=AM.Id
                                LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.AdvanceDetailId=AD.Id
                                LEFT JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
								LEFT JOIN [MST].[BankMaster] AS BKM ON BKM.Id=AM.BankMasterId
								LEFT JOIN [MST].[CashMaster] AS CKM ON CKM.Id=AM.CashMasterId
								LEFT JOIN [dbo].[EmployeeInformation] AS EIR ON EIR.SystemId=AM.ResponsiblePersonId
                                LEFT JOIN [HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id=AD.GLGeneralInfoId
                                LEFT JOIN [MST].[BudgetMaster] AS BM ON BM.Id=AD.BudgetMasterId
                                LEFT JOIN [HKP].[Budget] AS B ON B.Id=BM.BudgetId
                                LEFT JOIN [HKP].[Activity] AS A ON A.Id=AD.ActivityId
                                LEFT JOIN [SCS].[Currency] AS C ON C.Id=AM.CurrencyId
                                LEFT JOIN [ORG].[Entity] AS EN ON EN.Id=AM.EntityId
                                LEFT JOIN [HKP].[Party] AS P ON P.Id=AM.PartyId
                                LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=AM.PartyPlantId
								LEFT JOIN (
								    SELECT VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.FromCurrencyId AS CompanyFromCurrencyId, VDC.ToCurrencyId,
								    VDC.ToCurrencyRate AS CompanyCurrencyRate, VDC.ToCurrencyConversion AS CompanyCurrencyConversion, VDC.CrAmount AS CompanyCurrencyAmount, VDC.VoucherDetailId
								    FROM [TRN].[VoucherDetailCurrency] AS VDC
								    JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
								    WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId=@CompanyId
							    ) AS CC ON CC.VoucherDetailId=VD.Id
								 LEFT JOIN (select SUM(VDCW.DrAmount)AdvanceWriteOffBooksAmount,AdvanceId from [TRN].[AdvanceWriteOffDetail] AWD
														INNER JOIN  [TRN].[VoucherDetail] VDW ON VDW.AdvanceWriteOffDetailId=AWD.Id
														INNER JOIN  [TRN].[VoucherDetailCurrency] AS VDCW ON VDCW.VoucherDetailId=VDW.Id
														LEFT JOIN [TRN].[AdvanceWriteOff] AW ON AW.Id=AWD.AdvanceWriteOffId WHERE AW.IsPark=0 AND AW.Archive=0 GROUP BY AdvanceId)AW ON AW.AdvanceId=AD.AdvanceId
							  
                                WHERE AM.Archive=0 AND AM.IsPosted=1 AND AM.IsWrittenOff=0 AND AD.IsWrittenOff=0 AND AM.SourceType='CustomerAdvance' AND AM.PartyType='Customer' 
                                AND AM.CompanyGroupId=@CompanyGroupId AND AM.CompanyId=@CompanyId AND AM.PlantId=@PlantId  )X ";
            return _sqlRepository.GetDataTable(strSql);
        }
        private DataTable GetDebitNotePaymentPendingforSetOffReportSQL(string companyGroupId, string companyId, string plantId)
        {
            string strSql = "";
            strSql = @"Declare @CompanyGroupId nvarchar(50)='" + companyGroupId + @"',@CompanyId nvarchar(50)='" + companyId + @"',@PlantId nvarchar(50)='" + plantId + @"'

SELECT  X.PartyType,X.VoucherRowId,X.VoucherNo,X.PartyName,X.PostingDate,X.DocDate,X.DocRefNo,X.Narration,X.GL ,X.Budget ,X.Activity ,X.CurrencyCode
,X.Receivable,X.Received,X.Balance,X.BookReceivable,X.BookReceived,X.BookBalance
FROM (SELECT I.CompanyId, I.PlantId, I.PartyPlantId, I.PartyType, I.Id AS AdjustmentNoteId, ID.Id AS AdjustmentNoteDetailId, I.VoucherId, V.VoucherNo, VD.EntityId, EN.UserName AS EntityName,   I.PartyId, P.UserName As PartyName, VD.Id AS VoucherRowId, I.CurrencyId
                                    , C.Code AS CurrencyCode, ID.GLGeneralInfoId AS GLGeneralInfoId, GLGI.AccountCode AS GLGeneralInfoCode, GLGI.UserName AS GL, ID.BudgetMasterId, B.Code AS BudgetCode
                                    , B.UserName AS Budget, ID.ActivityId, A.Code AS ActivityCode, A.UserName AS Activity, Replace(CONVERT(VARCHAR(11), I.DocDate, 106), ' ', '-') AS DocDate, Replace(CONVERT(VARCHAR(11)
                                    , I.PostingDate, 106), ' ', '-') AS PostingDate, I.DocRefNo, I.Narration, ISNULL(ID.Amount,0) AS Receivable, (ISNULL(ID.WrittenOffAmount,0)) AS Received, (ISNULL(ID.Amount,0)- (ISNULL(ID.WrittenOffAmount,0))) AS Balance
									, CC.CompanyCurrencyRate,CC.CompanyCurrencyAmount BookReceivable ,ISNULL(W.AdjustmentNoteWriteOffBooksAmount,0)BookReceived
									,ISNULL(CC.CompanyCurrencyAmount,0)- ISNULL(W.AdjustmentNoteWriteOffBooksAmount,0)BookBalance
                                    FROM [TRN].[AdjustmentNoteDetail] AS ID
                                    LEFT JOIN [TRN].[AdjustmentNote] AS I ON I.Id=ID.AdjustmentNoteId
									 LEFT JOIN [HKP].[Party] AS P ON P.Id=I.PartyId
                                    LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.AdjustmentNoteDetailId=ID.Id
                                    LEFT JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                                    LEFT JOIN [HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id=ID.GLGeneralInfoId
                                    LEFT JOIN [MST].[BudgetMaster] AS BM ON BM.Id=ID.BudgetMasterId
                                    LEFT JOIN [HKP].[Budget] AS B ON B.Id=BM.BudgetId
                                    LEFT JOIN [HKP].[Activity] AS A ON A.Id=ID.ActivityId
                                    LEFT JOIN [SCS].[Currency] AS C ON C.Id=I.CurrencyId
                                    LEFT JOIN [ORG].[Entity] AS EN ON EN.Id=I.EntityId
										LEFT JOIN (
										SELECT VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.FromCurrencyId AS CompanyFromCurrencyId, VDC.ToCurrencyId,
										VDC.ToCurrencyRate AS CompanyCurrencyRate, VDC.ToCurrencyConversion AS CompanyCurrencyConversion, VDC.DrAmount AS CompanyCurrencyAmount, VDC.VoucherDetailId
										FROM [TRN].[VoucherDetailCurrency] AS VDC
										JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
										WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId=@CompanyId
									) AS CC ON CC.VoucherDetailId=VD.Id
									LEFT JOIN (select SUM(ISNULL(VDCW.CrAmount,0))AdjustmentNoteWriteOffBooksAmount,AdjustmentNoteId from [TRN].[InvoiceWriteOffDetail] IWD
														INNER JOIN [TRN].[InvoiceWriteOff] IW ON IW.Id=IWD.InvoiceWriteOffId
														INNER JOIN  [TRN].[VoucherDetail] VDW ON VDW.InvoiceWriteOffDetailId=IWD.Id
														INNER JOIN  [TRN].[VoucherDetailCurrency] AS VDCW ON VDCW.VoucherDetailId=VDW.Id
														where IW.IsPark=0 AND IWD.AdjustmentNoteId is not null
														GROUP BY  IWD.AdjustmentNoteId)W ON W.AdjustmentNoteId=ID.AdjustmentNoteId
                                    WHERE I.Archive=0 AND I.IsPark=0  AND I.IsWrittenOff=0 AND ID.IsWrittenOff=0  AND I.SourceType in ('DebitNote','InventoryReturnPayable')
                                    AND I.CompanyGroupId=@CompanyGroupId AND I.CompanyId=@CompanyId AND I.PlantId=@PlantId  )X ";
            return _sqlRepository.GetDataTable(strSql);
        }
        private DataTable GetCreditNotePaymentPendingforSetOffReportSQL(string companyGroupId, string companyId, string plantId)
        {
            string strSql = "";
            strSql = @"Declare @CompanyGroupId nvarchar(50)='" + companyGroupId + @"',@CompanyId nvarchar(50)='" + companyId + @"',@PlantId nvarchar(50)='" + plantId + @"'

SELECT  X.PartyType,X.VoucherRowId,X.VoucherNo,X.PartyName,X.PostingDate,X.DocDate,X.DocRefNo,X.Narration,X.GL ,X.Budget ,X.Activity ,X.CurrencyCode
,X.Receivable,X.Received,X.Balance,X.BookReceivable,X.BookReceived,X.BookBalance
FROM (SELECT I.CompanyId, I.PlantId, I.PartyPlantId, I.PartyType, I.Id AS AdjustmentNoteId, ID.Id AS AdjustmentNoteDetailId, I.VoucherId, V.VoucherNo, VD.EntityId, EN.UserName AS EntityName,   I.PartyId, P.UserName As PartyName, VD.Id AS VoucherRowId, I.CurrencyId
                                    , C.Code AS CurrencyCode, ID.GLGeneralInfoId AS GLGeneralInfoId, GLGI.AccountCode AS GLGeneralInfoCode, GLGI.UserName AS GL, ID.BudgetMasterId, B.Code AS BudgetCode
                                    , B.UserName AS Budget, ID.ActivityId, A.Code AS ActivityCode, A.UserName AS Activity, Replace(CONVERT(VARCHAR(11), I.DocDate, 106), ' ', '-') AS DocDate, Replace(CONVERT(VARCHAR(11)
                                    , I.PostingDate, 106), ' ', '-') AS PostingDate, I.DocRefNo, I.Narration, ISNULL(ID.Amount,0) AS Receivable, (ISNULL(ID.WrittenOffAmount,0)) AS Received, (ISNULL(ID.Amount,0)- (ISNULL(ID.WrittenOffAmount,0))) AS Balance
									, CC.CompanyCurrencyRate,CC.CompanyCurrencyAmount BookReceivable ,ISNULL(W.AdjustmentNoteWriteOffBooksAmount,0)BookReceived
									,ISNULL(CC.CompanyCurrencyAmount,0)- ISNULL(W.AdjustmentNoteWriteOffBooksAmount,0)BookBalance
                                    FROM [TRN].[AdjustmentNoteDetail] AS ID
                                    LEFT JOIN [TRN].[AdjustmentNote] AS I ON I.Id=ID.AdjustmentNoteId
									 LEFT JOIN [HKP].[Party] AS P ON P.Id=I.PartyId
                                    LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.AdjustmentNoteDetailId=ID.Id
                                    LEFT JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                                    LEFT JOIN [HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id=ID.GLGeneralInfoId
                                    LEFT JOIN [MST].[BudgetMaster] AS BM ON BM.Id=ID.BudgetMasterId
                                    LEFT JOIN [HKP].[Budget] AS B ON B.Id=BM.BudgetId
                                    LEFT JOIN [HKP].[Activity] AS A ON A.Id=ID.ActivityId
                                    LEFT JOIN [SCS].[Currency] AS C ON C.Id=I.CurrencyId
                                    LEFT JOIN [ORG].[Entity] AS EN ON EN.Id=I.EntityId
										LEFT JOIN (
										SELECT VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.FromCurrencyId AS CompanyFromCurrencyId, VDC.ToCurrencyId,
										VDC.ToCurrencyRate AS CompanyCurrencyRate, VDC.ToCurrencyConversion AS CompanyCurrencyConversion, VDC.CrAmount AS CompanyCurrencyAmount, VDC.VoucherDetailId
										FROM [TRN].[VoucherDetailCurrency] AS VDC
										JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
										WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId=@CompanyId
									) AS CC ON CC.VoucherDetailId=VD.Id
									LEFT JOIN (select SUM(ISNULL(VDCW.DrAmount,0))AdjustmentNoteWriteOffBooksAmount,AdjustmentNoteId from [TRN].[InvoiceWriteOffDetail] IWD
														INNER JOIN [TRN].[InvoiceWriteOff] IW ON IW.Id=IWD.InvoiceWriteOffId
														INNER JOIN  [TRN].[VoucherDetail] VDW ON VDW.InvoiceWriteOffDetailId=IWD.Id
														INNER JOIN  [TRN].[VoucherDetailCurrency] AS VDCW ON VDCW.VoucherDetailId=VDW.Id
														where IW.IsPark=0 AND IWD.AdjustmentNoteId is not null
														GROUP BY  IWD.AdjustmentNoteId)W ON W.AdjustmentNoteId=ID.AdjustmentNoteId
                                    WHERE I.Archive=0 AND I.IsPark=0  AND I.IsWrittenOff=0 AND ID.IsWrittenOff=0  AND I.SourceType in ('CreditNote','VendorPayment')
                                    AND I.CompanyGroupId=@CompanyGroupId AND I.CompanyId=@CompanyId AND I.PlantId=@PlantId  )X ";
            return _sqlRepository.GetDataTable(strSql);
        }

        private DataTable GetGSTPayableSalesSQL(string companyGroupId, string companyId, string plantId, string plantName, string fromDate, string toDate, string taxyearId)
        {
            string strSql = "";
            strSql = @"SELECT * FROM (
				SELECT SourceType= CASE WHEN V.SourceType='CustomerInvoice' THEN 'GL' ELSE '' END
                            ,V.VoucherNo,format( V.PostingDate,'dd-MMM-yyyy')PostingDate, V.DocRefNo,format (V.DocDate,'dd-MMM-yyyy')DocDate
							,P.UserName PartyName,PP.GSTIN
							,NULL GRNNo,pp.UserName PartyPlantName
                            ,LineItemType=case  WHEN v.SourceType='CustomerInvoice' THEN 'GL'  ELSE '' END
                            ,Particular=CASE WHEN v.SourceType='CustomerInvoice' THEN A.UserName ELSE '' END
                            ,TaxableAmount=case when v.SourceType='CustomerInvoice' then IV.Amount else 0 end
                            ,IT.Id,0 DrAmount
							,CrAmount=case when ITD.AType='Cr' then IT.TaxAmount else 0 end
	                        ,format( v.VoucherDate,'dd-MMM-yyyy')VoucherDate
                            ,TC.TaxCategoryType,TC.Code TaxCode,TC.Sequence TCSequence,TC.UserName+'-'+TC.Code TaxCategory,IsNULL(TAXC.IsRCM,0) IsRCM,TAXC.UserName TaxCodeName
                            ,IsNULL(IV.IsExcludingTax,0) IsExcludingTax,0 IsTaxApplicable,TAXC.[Type],TAXC.ValueOfFixed
                            ,[Percentage]= case when v.SourceType='CustomerInvoice' then taxc.ValueOfFixed else 0 end,NULL HSNCodeId,NULL Material
							,TaxPercentage= case when v.SourceType='CustomerInvoice' then taxc.ValueOfFixed else 0 end
												 ,TA.UserName ActivityName,NULL InventoryReceiveDetailId,NULL InventoryServiceId,EN.UserName Entity
                            from TRN.InvoiceTax IT
                            left join TRN.InvoiceTaxDetail ITD ON IT.Id=ITD.InvoiceTaxId AND ITD.AType='Cr'
                            LEFT JOIN TRN.Voucher V ON V.Id=IT.VoucherId
                            LEFT JOIN TRN.Invoice IV ON IV.Id=IT.InvoiceId
                            LEFT JOIN HKP.Activity TA ON TA.Id=ITD.ActivityId
                            LEFT JOIN HKP.Party P ON P.Id=IT.PartyId
							Left join hkp.PartyPlant PP on PP.Id=IT.PartyPlantId
                            LEFT JOIN MST.TaxCategory TC ON TC.Id=IT.TaxCategoryId
                            LEFT JOIN ORG.Entity EN ON EN.Id=V.EntityId
                            LEFT JOIN( select TAC.Id,TAC.UserName,TAC.IsRCM,TAY.[Type],TACD.ValueOfFixed from MST.TaxCode TAC
                            LEFT JOIN MST.TaxCodeYear TAY ON TAY.TaxCodeId=TAC.Id
                            LEFT JOIN MST.TaxCodeDetail TACD ON TACD.TaxCodeId=TAC.Id WHERE TAY.TaxYearId IN (" + taxyearId + @")) TAXC ON TAXC.Id=IT.TaxCodeId
                            LEFT JOIN TRN.VoucherDetail VD ON VD.Id=IT.VoucherDetailId
                            LEFT JOIN HKP.Activity A ON A.Id=VD.ActivityId
                            LEFT JOIN (SELECT IW.InvoiceWriteOffId,IW.ActivityId,SUM(I.Amount) Amount FROM TRN.InvoiceWriteOffDetail IW
                            JOIN TRN.Invoice I ON I.Id=IW.InvoiceId
                            GROUP BY InvoiceWriteOffId,ActivityId) IWD ON IWD.InvoiceWriteOffId=IT.InvoiceWriteOffId
                            LEFT JOIN HKP.Activity AP ON AP.Id=IWD.ActivityId
                            where TC.TaxCategoryType='GST' AND TAXC.IsRCM=0 AND   V.IsPark=0 AND 
							V.PlantId='" + plantId + @"' and V.PostingDate between '" + fromDate + "' AND '" + toDate + @"'
                            AND v.SourceType IN ('CustomerInvoice')
                            
                            UNION all

							SELECT SourceType= CASE  WHEN V.SourceType='SalesInvoice' THEN 'Sales' ELSE '' END
                            , V.VoucherNo,format( V.PostingDate,'dd-MMM-yyyy')PostingDate, V.DocRefNo,format (V.DocDate,'dd-MMM-yyyy')DocDate,P.UserName PartyName,PP.GSTIN
							, IRD.InventorySalesId GRNNo,pp.UserName PartyPlantName
                            , LineItemType=case when v.SourceType='SalesInvoice' then 'Sales' ELSE '' END
                            , Particular=CASE WHEN v.SourceType='SalesInvoice' THEN MM.UserName ELSE '' END
                            ,TaxableAmount=case when v.SourceType='SalesInvoice' then IRD.PolicyAmount else 0 end

                            ,IT.Id,0 DrAmount,CrAmount=case when ITD.AType='Cr' then IRT.TaxAmount else 0 end
	                        ,format( v.VoucherDate,'dd-MMM-yyyy')VoucherDate
                            ,TC.TaxCategoryType,TC.Code TaxCode,TC.Sequence TCSequence,TC.UserName+'-'+TC.Code TaxCategory,IsNULL(TAXC.IsRCM,0) IsRCM,TAXC.UserName TaxCodeName
                            ,IsNULL(IV.IsExcludingTax,0) IsExcludingTax
							,IsTaxApplicable=CASE WHEN IsNULL(CP.TaxApplicable,'')='Mandatory' THEN 1 ELSE 0 END 
							,TAXC.[Type],TAXC.ValueOfFixed
                            ,IRT.[Percentage],NULL HSNCodeId,null Material
							,TaxPercentage= case  when v.SourceType='InventorySales' AND IRT.[Percentage]>0 THEN IRT.[Percentage]
												 else 0 end
												 ,TA.UserName ActivityName,IRT.InventoryReceiveDetailId
                            ,IRT.InventorySalesServiceId,EN.UserName Entity
                            from TRN.InvoiceTax IT
                            left join TRN.InvoiceTaxDetail ITD ON IT.Id=ITD.InvoiceTaxId AND ITD.AType='Cr'
                            LEFT JOIN TRN.Voucher V ON V.Id=IT.VoucherId
                            LEFT JOIN TRN.Invoice IV ON IV.Id=IT.InvoiceId
                            --LEFT JOIN TRN.InvoiceWriteOff IW ON IW.Id=IT.InvoiceWriteOffId
                            LEFT JOIN HKP.Activity TA ON TA.Id=ITD.ActivityId
                            LEFT JOIN HKP.Party P ON P.Id=IT.PartyId
							LEFT JOIN ORG.Entity EN ON EN.Id=V.EntityId
                            LEFT JOIN MST.TaxCategory TC ON TC.Id=IT.TaxCategoryId
                            LEFT JOIN( select TAC.Id,TAC.UserName,TAC.IsRCM,TAY.[Type],TACD.ValueOfFixed from MST.TaxCode TAC
                            LEFT JOIN MST.TaxCodeYear TAY ON TAY.TaxCodeId=TAC.Id
                            LEFT JOIN MST.TaxCodeDetail TACD ON TACD.TaxCodeId=TAC.Id WHERE TAY.TaxYearId IN (" + taxyearId + @") 
							--and tac.IsRCM=0
							) TAXC ON TAXC.Id=IT.TaxCodeId
                            --LEFT JOIN SCS.TaxYear TY ON TY.Id=TAY.TaxYearId
                            LEFT JOIN TRN.InventorySales IR ON IR.VoucherId=V.Id
                            LEFT JOIN TRN.InventorySalesDetail IRD ON IRD.InventorySalesId=IR.Id
                            LEFT JOIN TRN.InventorySalesTax IRT ON IRD.Id=IRT.InventoryReceiveDetailId AND IRT.TaxCategoryId=IT.TaxCategoryId
                            LEFT JOIN TRN.InventoryMaterial IM ON IM.Id=IRD.InventoryMaterialId
                            LEFT JOIN MST.MaterialMaster MM ON MM.Id=IM.MaterialMasterId
                            LEFT JOIN TRN.VoucherDetail VD ON VD.Id=IT.VoucherDetailId
                            LEFT JOIN HKP.Activity A ON A.Id=VD.ActivityId
                            Left join hkp.PartyPlant pp on pp.Id=IR.InvoicingPartyPlantId
							LEFT JOIN HKP.CompanyParty CP ON CP.PartyId=P.Id AND CP.PartyType='Customer' AND CP.PlantId = '" + plantId + @"'
                            where TC.TaxCategoryType='GST' AND (CP.TaxApplicable IS NULL OR CP.TaxApplicable ='Optional')  --AND V.IsPark=0
							AND IR.PlantId = '" + plantId + @"'   and V.PostingDate between '" + fromDate + "' AND '" + toDate + @"'
                            AND v.SourceType='SalesInvoice' and IRT.InventorySalesServiceId IS NULL
                            UNION all

                            SELECT SourceType=IR.SourceType
                            , V.VoucherNo,format( V.PostingDate,'dd-MMM-yyyy')PostingDate, V.DocRefNo,format (V.DocDate,'dd-MMM-yyyy')DocDate,P.UserName PartyName,PP.GSTIN
							, IRD.SalesId GRNNo,pp.UserName PartyPlantName
                            , LineItemType=case when v.SourceType='SalesInvoice' then 'Sales' ELSE '' END
                            , Particular=CASE WHEN v.SourceType='SalesInvoice' THEN MM.UserName ELSE '' END
                            ,TaxableAmount=case when v.SourceType='SalesInvoice' then IRD.BaseAmount else 0 end

                            ,IT.Id,0 DrAmount,CrAmount=case when ITD.AType='Cr' then IRT.Amount else 0 end
	                        ,format( v.VoucherDate,'dd-MMM-yyyy')VoucherDate
                            ,TC.TaxCategoryType,TC.Code TaxCode,TC.Sequence TCSequence,TC.UserName+'-'+TC.Code TaxCategory,IsNULL(TAXC.IsRCM,0) IsRCM,TAXC.UserName TaxCodeName
                            ,IsNULL(IV.IsExcludingTax,0) IsExcludingTax
							,IsTaxApplicable=CASE WHEN IsNULL(CP.TaxApplicable,'')='Mandatory' THEN 1 ELSE 0 END 
							,TAXC.[Type],TAXC.ValueOfFixed
                            ,IRT.[Percentage],NULL HSNCodeId,null Material
							,TaxPercentage= case  when v.SourceType='Sales' AND IRT.[Percentage]>0 THEN IRT.[Percentage]
												 else 0 end
												 ,TA.UserName ActivityName,IRT.SalesMaterialId InventoryReceiveDetailId
                            ,IRT.SalesServiceId InventorySalesServiceId,EN.UserName Entity
                            from TRN.InvoiceTax IT
                            left join TRN.InvoiceTaxDetail ITD ON IT.Id=ITD.InvoiceTaxId AND ITD.AType='Cr'
                            LEFT JOIN TRN.Voucher V ON V.Id=IT.VoucherId
                            LEFT JOIN TRN.Invoice IV ON IV.Id=IT.InvoiceId
                            --LEFT JOIN TRN.InvoiceWriteOff IW ON IW.Id=IT.InvoiceWriteOffId
                            LEFT JOIN HKP.Activity TA ON TA.Id=ITD.ActivityId
                            LEFT JOIN HKP.Party P ON P.Id=IT.PartyId
							LEFT JOIN ORG.Entity EN ON EN.Id=V.EntityId
                            LEFT JOIN MST.TaxCategory TC ON TC.Id=IT.TaxCategoryId
                            LEFT JOIN( select TAC.Id,TAC.UserName,TAC.IsRCM,TAY.[Type],TACD.ValueOfFixed from MST.TaxCode TAC
                            LEFT JOIN MST.TaxCodeYear TAY ON TAY.TaxCodeId=TAC.Id
                            LEFT JOIN MST.TaxCodeDetail TACD ON TACD.TaxCodeId=TAC.Id WHERE TAY.TaxYearId IN (" + taxyearId + @") 
							--and tac.IsRCM=0
							) TAXC ON TAXC.Id=IT.TaxCodeId
                            --LEFT JOIN SCS.TaxYear TY ON TY.Id=TAY.TaxYearId
                            LEFT JOIN TRN.Sales IR ON IR.VoucherId=V.Id
                            LEFT JOIN TRN.SalesMaterial IRD ON IRD.SalesId=IR.Id
                            LEFT JOIN TRN.SalesTax IRT ON IRD.Id=IRT.SalesMaterialId AND IRT.TaxCategoryId=IT.TaxCategoryId
                            LEFT JOIN MST.MaterialMaster MM ON MM.Id=IRD.MaterialMasterId
                            LEFT JOIN TRN.VoucherDetail VD ON VD.Id=IT.VoucherDetailId
                            LEFT JOIN HKP.Activity A ON A.Id=VD.ActivityId
                            Left join hkp.PartyPlant pp on pp.Id=IR.InvoicingPartyPlantId
							LEFT JOIN HKP.CompanyParty CP ON CP.PartyId=P.Id AND CP.PartyType='Customer' AND CP.PlantId = '" + plantId + @"'
                            where TC.TaxCategoryType='GST' AND (CP.TaxApplicable IS NULL OR CP.TaxApplicable ='Optional')  --AND V.IsPark=0
							AND IR.PlantId = '" + plantId + @"'   and V.PostingDate between '" + fromDate + "' AND '" + toDate + @"'
                            AND v.SourceType='SalesInvoice' and IRT.SalesMaterialId<>''

							UNION all

                            SELECT SourceType=IR.SourceType
                            , V.VoucherNo,format( V.PostingDate,'dd-MMM-yyyy')PostingDate, V.DocRefNo,format (V.DocDate,'dd-MMM-yyyy')DocDate,P.UserName PartyName,PP.GSTIN
							, IRD.SalesId GRNNo,pp.UserName PartyPlantName
                            , LineItemType=case when v.SourceType='SalesInvoice' then 'SalesService' ELSE '' END
                            , Particular=CASE WHEN v.SourceType='SalesInvoice' THEN MM.UserName ELSE '' END
                            ,TaxableAmount=case when v.SourceType='SalesInvoice' then IRD.NetAmount else 0 end

                            ,IT.Id,0 DrAmount,CrAmount=case when ITD.AType='Cr' then IRT.Amount else 0 end
	                        ,format( v.VoucherDate,'dd-MMM-yyyy')VoucherDate
                            ,TC.TaxCategoryType,TC.Code TaxCode,TC.Sequence TCSequence,TC.UserName+'-'+TC.Code TaxCategory,IsNULL(TAXC.IsRCM,0) IsRCM,TAXC.UserName TaxCodeName
                            ,IsNULL(IV.IsExcludingTax,0) IsExcludingTax
							,IsTaxApplicable=CASE WHEN IsNULL(CP.TaxApplicable,'')='Mandatory' THEN 1 ELSE 0 END 
							,TAXC.[Type],TAXC.ValueOfFixed
                            ,IRT.[Percentage],NULL HSNCodeId,null Material
							,TaxPercentage= case  when v.SourceType='Sales' AND IRT.[Percentage]>0 THEN IRT.[Percentage]
												 else 0 end
												 ,TA.UserName ActivityName,IRT.SalesMaterialId InventoryReceiveDetailId
                            ,IRT.SalesServiceId InventorySalesServiceId,EN.UserName Entity
                            from TRN.InvoiceTax IT
                            left join TRN.InvoiceTaxDetail ITD ON IT.Id=ITD.InvoiceTaxId AND ITD.AType='Cr'
                            LEFT JOIN TRN.Voucher V ON V.Id=IT.VoucherId
                            LEFT JOIN TRN.Invoice IV ON IV.Id=IT.InvoiceId
                            --LEFT JOIN TRN.InvoiceWriteOff IW ON IW.Id=IT.InvoiceWriteOffId
                            LEFT JOIN HKP.Activity TA ON TA.Id=ITD.ActivityId
                            LEFT JOIN HKP.Party P ON P.Id=IT.PartyId
							LEFT JOIN ORG.Entity EN ON EN.Id=V.EntityId
                            LEFT JOIN MST.TaxCategory TC ON TC.Id=IT.TaxCategoryId
                            LEFT JOIN( select TAC.Id,TAC.UserName,TAC.IsRCM,TAY.[Type],TACD.ValueOfFixed from MST.TaxCode TAC
                            LEFT JOIN MST.TaxCodeYear TAY ON TAY.TaxCodeId=TAC.Id
                            LEFT JOIN MST.TaxCodeDetail TACD ON TACD.TaxCodeId=TAC.Id WHERE TAY.TaxYearId IN (" + taxyearId + @") 
							--and tac.IsRCM=0
							) TAXC ON TAXC.Id=IT.TaxCodeId
                            --LEFT JOIN SCS.TaxYear TY ON TY.Id=TAY.TaxYearId
                            LEFT JOIN TRN.Sales IR ON IR.VoucherId=V.Id
                            LEFT JOIN TRN.SalesService IRD ON IRD.SalesId=IR.Id
                            LEFT JOIN TRN.SalesTax IRT ON IRD.Id=IRT.SalesServiceId AND IRT.TaxCategoryId=IT.TaxCategoryId  
                            LEFT JOIN hkp.ServiceMaster MM ON MM.Id=IRD.ServiceMasterId
                            LEFT JOIN TRN.VoucherDetail VD ON VD.Id=IT.VoucherDetailId
                            LEFT JOIN HKP.Activity A ON A.Id=VD.ActivityId
                            Left join hkp.PartyPlant pp on pp.Id=IR.InvoicingPartyPlantId
							LEFT JOIN HKP.CompanyParty CP ON CP.PartyId=P.Id AND CP.PartyType='Customer' AND CP.PlantId = '" + plantId + @"'
                            where TC.TaxCategoryType='GST' AND (CP.TaxApplicable IS NULL OR CP.TaxApplicable ='Optional')  --AND V.IsPark=0
							AND IR.PlantId = '" + plantId + @"'   and V.PostingDate between '" + fromDate + "' AND '" + toDate + @"'
                            AND v.SourceType='SalesInvoice' and IRT.SalesServiceId<>''

                            ) x
							ORDER BY [Percentage],VoucherNo, DocDate, ISNULL(InventoryReceiveDetailId,''),ISNULL(InventoryServiceId,'')  ";
            return _sqlRepository.GetDataTable(strSql);

        }

        #endregion
        #region GST R 2 Report
        public IWorkbook GetGSTR2Report(string companyGroupId, string companyId, string plantId, string plantName, string fromDate, string toDate, string name)
        {
            clsReport objRpt = null;
            clsReport objRptSR = null;
            try
            {

                ExcelEngine excelEngine = null;
                IApplication application = null;
                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;
                excelEngine.Excel.DefaultVersion = ExcelVersion.Excel2013;
                var reportUtility = new ReportUtility();
                var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
                workbook.Version = ExcelVersion.Excel2013;
                var sheet1 = workbook.Worksheets[0];

                #region Logo
                string strPath = "";
                Image companyLogo = null;
                try
                {
                    DataTable dtCompanyImage = _sqlRepository.GetDataTable("SELECT * FROM ORG.COMPANY WHERE ID = '" + companyId + @"'");

                    strPath = Path.Combine(ResourcesPathReader.GetLogoOrImagePath(), dtCompanyImage.Rows[0]["Image"].ToString());  // IDCardEng.xlsx
                    companyLogo = Image.FromFile(strPath);
                }
                catch (Exception)
                {
                }
                #endregion
                objRpt = new clsReport();

                objRptSR = new clsReport(_sqlRepository);

                DataTable dtRCMPayable = null;
                string taxyearId = GetTaxYearId(fromDate, toDate, companyId);
                dtRCMPayable = GetGSTR2SQL(companyGroupId, companyId, plantId, plantName, fromDate, toDate, taxyearId);
                if (dtRCMPayable.Rows.Count == 0)
                {
                    throw new Exception("No Data Found....");
                }

                DataTable dtCmp = objRptSR.SelectedCompanyDT(plantId);

                DataTable dtFactory = objRptSR.SelectedPlantDT(plantId);

                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;

                int xlsRow = 1, xlsCol = 1;
                int endXlsCol = 1;
                string FactoryName = "";
                string CmpName = "";
                xlsRow = 6;
                sheet1.Range[xlsRow - 1, 1].Text = "Report Ref No:";
                sheet1.Range[xlsRow - 1, 1].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow - 1, 1].RowHeight = 20;
                sheet1.Range[xlsRow - 1, 1].CellStyle.Font.Bold = true;

                int iSuppliersName = xlsCol; // Party
                sheet1.Range[xlsRow, xlsCol].Text = "Suppliers Name";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 35;
                sheet1.Range[xlsRow, xlsCol, xlsRow + 1, xlsCol].Merge();

                xlsCol++;


                int iGSTINSuppliers = xlsCol; // Party
                sheet1.Range[xlsRow, xlsCol].Text = "GSTIN of Suppliers";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 20;
                sheet1.Range[xlsRow, xlsCol, xlsRow + 1, xlsCol].Merge();



                xlsCol++;
                int iVoucherNo = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Voucher No";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                sheet1.Range[xlsRow, xlsCol, xlsRow + 1, xlsCol].Merge();



                xlsCol++;
                int iPostingDate = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Voucher Date";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                sheet1.Range[xlsRow, xlsCol, xlsRow + 1, xlsCol].Merge();


                int iParticulars = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Particulars";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 45;
                sheet1.Range[xlsRow, xlsCol, xlsRow + 1, xlsCol].Merge();



                xlsCol++;
                int iHSNCode = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "HSN Code";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                sheet1.Range[xlsRow, xlsCol, xlsRow + 1, xlsCol].Merge();

                xlsCol++;
                int iInvoiceDetailsNo = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Invoice Details";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;

                iInvoiceDetailsNo = xlsCol;
                sheet1.Range[xlsRow + 1, xlsCol].Text = "No";
                sheet1.Range[xlsRow + 1, xlsCol].ColumnWidth = 15;

                xlsCol++;
                int iInvoiceDetailsDate = xlsCol;
                sheet1.Range[xlsRow + 1, xlsCol].Text = "Date";
                sheet1.Range[xlsRow + 1, xlsCol].ColumnWidth = 15;

                xlsCol++;
                int iInvoiceDetailsAmount = xlsCol;
                sheet1.Range[xlsRow + 1, xlsCol].Text = "Amount";
                sheet1.Range[xlsRow + 1, xlsCol].ColumnWidth = 15;
                sheet1.Range[xlsRow, iInvoiceDetailsNo, xlsRow, iInvoiceDetailsAmount].Merge();
                sheet1.Range[xlsRow, iInvoiceDetailsNo, xlsRow, iInvoiceDetailsAmount].HorizontalAlignment = ExcelHAlign.HAlignCenter;


                xlsCol++;
                int iRate = xlsCol; // Doc Ref
                sheet1.Range[xlsRow, xlsCol].Text = "Rate";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                sheet1.Range[xlsRow + 1, xlsCol].Text = "%";
                sheet1.Range[xlsRow + 1, xlsCol].ColumnWidth = 15;
                xlsCol++;
                int iTaxableAmount = xlsCol; // Doc Ref
                sheet1.Range[xlsRow, xlsCol].Text = "Taxable Amount";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                // Doc Ref
                sheet1.Range[xlsRow, iTaxableAmount + 1].Text = "Amount of Tax";
                sheet1.Range[xlsRow, iTaxableAmount + 1].ColumnWidth = 15;


                DataTable dtTaxCode = null;
                dtRCMPayable.DefaultView.Sort = "TCSequence";
                dtTaxCode = dtRCMPayable.DefaultView.ToTable(true, "TaxCode");
                dtTaxCode.Columns.Add("ColumnNumber", typeof(String));
                dtTaxCode.Columns.Add("ColumnFormula", typeof(String));

                dtTaxCode.Columns.Add("ColumnNumber2", typeof(String));
                dtTaxCode.Columns.Add("ColumnFormula2", typeof(String));

                DataRow dtRow = dtTaxCode.NewRow();
                dtRow["TaxCode"] = "CES";


                dtTaxCode.Rows.Add(dtRow);

                if (dtTaxCode.Rows.Count > 0)
                {
                    for (int i = 0; i < dtTaxCode.Rows.Count; i++)
                    {
                        xlsCol++;
                        sheet1.Range[xlsRow + 1, xlsCol].Text = dtTaxCode.Rows[i]["TaxCode"].ToString();
                        sheet1.Range[xlsRow + 1, xlsCol].ColumnWidth = 15;
                        dtTaxCode.Rows[i]["ColumnNumber"] = xlsCol.ToString();
                    }
                }
                sheet1.Range[xlsRow, iTaxableAmount + 1, xlsRow, xlsCol].Merge();
                sheet1.Range[xlsRow, iTaxableAmount + 1, xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                xlsCol++;

                int iNameOfState = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Name Of State";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                sheet1.Range[xlsRow, xlsCol, xlsRow + 1, xlsCol].Merge();

                xlsCol++;
                int iInputGoods = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Weather Input/Input Service/Capital Godds (Incl Plant and Machinery)";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                sheet1.Range[xlsRow + 1, xlsCol].Text = "Ineligible for ITC";
                sheet1.Range[xlsRow + 1, xlsCol].ColumnWidth = 15;

                sheet1.Range[xlsRow, iInputGoods + 1].Text = "Amount of ITC Available";
                sheet1.Range[xlsRow, iInputGoods + 1].ColumnWidth = 15;


                if (dtTaxCode.Rows.Count > 0)
                {
                    for (int i = 0; i < dtTaxCode.Rows.Count; i++)
                    {
                        xlsCol++;
                        sheet1.Range[xlsRow + 1, xlsCol].Text = dtTaxCode.Rows[i]["TaxCode"].ToString();
                        sheet1.Range[xlsRow + 1, xlsCol].ColumnWidth = 15;
                        dtTaxCode.Rows[i]["ColumnNumber2"] = xlsCol.ToString();
                    }
                }


                sheet1.Range[xlsRow, iInputGoods + 1, xlsRow, xlsCol].Merge();
                sheet1.Range[xlsRow, iInputGoods + 1, xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                endXlsCol = xlsCol;

                sheet1.Range[xlsRow, 1, xlsRow + 1, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, 1, xlsRow + 1, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, 1, xlsRow + 1, endXlsCol].WrapText = true;
                sheet1.Range[xlsRow, 1, xlsRow + 1, endXlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 1, xlsRow + 1, endXlsCol].RowHeight = 23;
                sheet1.Range[xlsRow, 1, xlsRow + 1, endXlsCol].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;
                sheet1.Range[xlsRow, 1, xlsRow + 1, endXlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;


                string voucherNoLineItem = "";
                string Percentage = "";
                int startRow = 0;
                int perStartRow = 0;
                string formula = "";
                string formula2 = "";
                string totalFormula = "";

                string lineItemPercentageType = "";
                xlsRow++;
                xlsRow++;

                startRow = xlsRow;
                perStartRow = xlsRow;
                bool isFirst = true;
                string totalTaxformula = "";
                string totalTaxformula2 = "";
                //string voucherNo = "";
                for (int i = 0; i < dtRCMPayable.Rows.Count; i++)
                {

                    if (voucherNoLineItem != dtRCMPayable.Rows[i]["VoucherNo"].ToString() + "-" + dtRCMPayable.Rows[i]["VoucherNo"].ToString())
                    {
                        sheet1.Range[xlsRow, iSuppliersName].Text = dtRCMPayable.Rows[i]["PartyName"].ToString();
                        sheet1.Range[xlsRow, iGSTINSuppliers].Text = dtRCMPayable.Rows[i]["GSTIN"].ToString();

                        sheet1.Range[xlsRow, iVoucherNo].Text = dtRCMPayable.Rows[i]["VoucherNo"].ToString();
                        sheet1.Range[xlsRow, iPostingDate].Text = dtRCMPayable.Rows[i]["PostingDate"].ToString();
                        sheet1.Range[xlsRow, iPostingDate].Text = dtRCMPayable.Rows[i]["PostingDate"].ToString();
                        sheet1.Range[xlsRow, iParticulars].Text = dtRCMPayable.Rows[i]["Particular"].ToString();//Description of Goods
                        sheet1.Range[xlsRow, iHSNCode].Text = dtRCMPayable.Rows[i]["HSNCode"].ToString();
                        sheet1.Range[xlsRow, iInvoiceDetailsNo].Text = dtRCMPayable.Rows[i]["DocRefNo"].ToString();
                        sheet1.Range[xlsRow, iInvoiceDetailsDate].Text = clsStaticInfo.GetDateTaxFormate(dtRCMPayable.Rows[i]["DocDate"].ToString());

                        sheet1.Range[xlsRow, iInputGoods].Text = dtRCMPayable.Rows[i]["IsNonCreditable"].ToString();
                        sheet1.Range[xlsRow, iNameOfState].Text = dtRCMPayable.Rows[i]["StateName"].ToString();

                        sheet1.Range[xlsRow, iRate].Number = clsStaticInfo.dbl(dtRCMPayable.Rows[i]["TaxPercentage"].ToString());

                        sheet1.Range[xlsRow, iRate].NumberFormat = reportUtility.NumberFormatDecimalTwo();






                        sheet1.Range[xlsRow, iTaxableAmount].Number = clsStaticInfo.dbl(dtRCMPayable.Rows[i]["TaxableAmount"].ToString());
                        sheet1.Range[xlsRow, iTaxableAmount].NumberFormat = reportUtility.NumberFormatDecimalTwo();


                        //sheet1.Range[xlsRow, iTaxableAmount].Number = clsStaticInfo.dbl(dtRCMPayable.Rows[i]["TaxableAmount"].ToString());//TaxableAmount
                        //sheet1.Range[xlsRow, iTaxableAmount].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                        //dtRCMPayable.DefaultView.RowFilter = "VoucherNo = '" + dtRCMPayable.Rows[i]["VoucherNo"].ToString() + "'";

                        if (dtTaxCode.Rows.Count > 0)
                        {
                            totalTaxformula = "=SUM(";
                            for (int j = 0; j < dtTaxCode.Rows.Count; j++)
                            {
                                dtRCMPayable.DefaultView.RowFilter = "TaxCode = '" + dtTaxCode.Rows[j]["TaxCode"].ToString() + "' and VoucherNo = '" + dtRCMPayable.Rows[i]["VoucherNo"].ToString() + "' and LineItemType = '" + dtRCMPayable.Rows[i]["LineItemType"].ToString() + "'";
                                if (dtRCMPayable.DefaultView.Count > 0)
                                {

                                    sheet1.Range[xlsRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])].Number = clsStaticInfo.dbl(dtRCMPayable.DefaultView[0]["DrAmount"].ToString());
                                    sheet1.Range[xlsRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                                }
                                else
                                {
                                    sheet1.Range[xlsRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])].Text = "-";
                                    sheet1.Range[xlsRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])].HorizontalAlignment = ExcelHAlign.HAlignRight;


                                }
                                totalTaxformula += clsStaticInfo.GetxlsCol(Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])) + xlsRow + ":";
                            }
                            sheet1.Range[xlsRow, iInvoiceDetailsAmount].Formula = totalTaxformula.Remove(totalTaxformula.Length - 1) + ")+" + clsStaticInfo.GetxlsCol(iTaxableAmount) + xlsRow;
                        }

                        if (dtTaxCode.Rows.Count > 0)
                        {
                            totalTaxformula2 = "=SUM(";
                            for (int j = 0; j < dtTaxCode.Rows.Count; j++)
                            {


                                dtRCMPayable.DefaultView.RowFilter = "TaxCode = '" + dtTaxCode.Rows[j]["TaxCode"].ToString() + "' and VoucherNo = '" + dtRCMPayable.Rows[i]["VoucherNo"].ToString() + "' and LineItemType = '" + dtRCMPayable.Rows[i]["LineItemType"].ToString() + "'";

                                if (dtRCMPayable.Rows[i]["IsNonCreditable"].ToString() != "Yes")
                                {
                                    if (dtRCMPayable.DefaultView.Count > 0)
                                    {

                                        sheet1.Range[xlsRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber2"])].Number = clsStaticInfo.dbl(dtRCMPayable.DefaultView[0]["DrAmount"].ToString());
                                        sheet1.Range[xlsRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber2"])].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                                    }
                                    else
                                    {
                                        sheet1.Range[xlsRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber2"])].Text = "-";
                                        sheet1.Range[xlsRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber2"])].HorizontalAlignment = ExcelHAlign.HAlignRight;


                                    }
                                }
                                else
                                {
                                    sheet1.Range[xlsRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber2"])].Text = "-";
                                    sheet1.Range[xlsRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber2"])].HorizontalAlignment = ExcelHAlign.HAlignRight;


                                }



                                totalTaxformula2 += clsStaticInfo.GetxlsCol(Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber2"])) + xlsRow + ":";
                            }


                            //sheet1.Range[xlsRow, iInvoiceDetailsAmount].Formula = totalTaxformula2.Remove(totalTaxformula2.Length - 1) + ")";

                        }

                        //Percentage = dtRCMPayable.Rows[i][lineItemPercentageType].ToString();



                        xlsRow++;
                    }

                    voucherNoLineItem = dtRCMPayable.Rows[i]["VoucherNo"].ToString() + "-" + dtRCMPayable.Rows[i]["VoucherNo"].ToString();


                }
                sheet1[perStartRow, iPostingDate, xlsRow - 1, iPostingDate].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iParticulars, xlsRow - 1, iParticulars].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iRate, xlsRow - 1, iRate].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iVoucherNo, xlsRow - 1, iVoucherNo].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iPostingDate, xlsRow - 1, iPostingDate].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iInvoiceDetailsNo, xlsRow - 1, iInvoiceDetailsNo].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iInvoiceDetailsDate, xlsRow - 1, iInvoiceDetailsDate].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iInvoiceDetailsAmount, xlsRow - 1, iInvoiceDetailsAmount].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iTaxableAmount, xlsRow - 1, iTaxableAmount].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iSuppliersName, xlsRow - 1, iSuppliersName].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iGSTINSuppliers, xlsRow - 1, iGSTINSuppliers].BorderAround(ExcelLineStyle.Hair);

                sheet1[perStartRow, iNameOfState, xlsRow - 1, iNameOfState].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iInputGoods, xlsRow - 1, iInputGoods].BorderAround(ExcelLineStyle.Hair);


                if (dtTaxCode.Rows.Count > 0)
                {
                    for (int j = 0; j < dtTaxCode.Rows.Count; j++)
                    {
                        sheet1[perStartRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"]), xlsRow - 1, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])].BorderAround(ExcelLineStyle.Hair);
                        sheet1[perStartRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber2"]), xlsRow - 1, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber2"])].BorderAround(ExcelLineStyle.Hair);

                    }
                }



                if (dtTaxCode.Rows.Count > 0)
                {
                    totalTaxformula = "=SUM(";
                    for (int j = 0; j < dtTaxCode.Rows.Count; j++)
                    {
                        sheet1[perStartRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"]), xlsRow - 1, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])].BorderAround(ExcelLineStyle.Hair);
                        formula2 = "SUM(" + clsStaticInfo.GetxlsCol(Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])) + perStartRow + ":" + clsStaticInfo.GetxlsCol(Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])) + (xlsRow - 1) + ")";
                        sheet1[xlsRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"]), xlsRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])].Formula = formula2;

                        dtTaxCode.Rows[j]["ColumnFormula"] += (clsStaticInfo.GetxlsCol(Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])) + xlsRow).ToString() + " + ";
                        totalTaxformula += clsStaticInfo.GetxlsCol(Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])) + xlsRow + ":";

                    }
                }
                sheet1.Range[xlsRow, 1, xlsRow, 1].Text = "Total";
                formula = "SUM(" + clsStaticInfo.GetxlsCol(iTaxableAmount) + perStartRow + ":" + clsStaticInfo.GetxlsCol(iTaxableAmount) + (xlsRow - 1) + ")";

                sheet1[xlsRow, iTaxableAmount, xlsRow, iTaxableAmount].Formula = formula;
                formula = "SUM(" + clsStaticInfo.GetxlsCol(iInvoiceDetailsAmount) + perStartRow + ":" + clsStaticInfo.GetxlsCol(iInvoiceDetailsAmount) + (xlsRow - 1) + ")";

                sheet1[xlsRow, iInvoiceDetailsAmount, xlsRow, iInvoiceDetailsAmount].Formula = formula;

                //sheet1[xlsRow, iTotalTax, xlsRow, iTotalTax].Formula = totalTaxformula.Remove(totalTaxformula.Length - 1) + ")";

                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Font.Bold = true;
                //totalFormula += (clsStaticInfo.GetxlsCol(iTaxableAmount) + xlsRow).ToString() + "+";





                if (dtTaxCode.Rows.Count > 0)
                {
                    totalTaxformula = "=SUM(";
                    for (int j = 0; j < dtTaxCode.Rows.Count; j++)
                    {
                        string fm = dtTaxCode.Rows[j]["ColumnFormula"].ToString().Trim();
                        sheet1[xlsRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber2"]), xlsRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber2"])].Formula = fm.Remove(fm.Length - 1); //dtTaxCode.Rows[j]["ColumnFormula"].ToString().Remove(dtTaxCode.Rows[j]["ColumnFormula"].ToString().Length - 1);
                        totalTaxformula += clsStaticInfo.GetxlsCol(Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber2"])) + xlsRow + ":";

                    }
                }


                // sheet1[xlsRow, iTaxableAmount, xlsRow, iTaxableAmount].Formula = totalFormula.Remove(totalFormula.Length - 1);
                //sheet1[xlsRow, iTotalTax, xlsRow, iTotalTax].Formula = totalTaxformula.Remove(totalTaxformula.Length - 1) + ")";

                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Font.Bold = true;




                #region ******************Report Header******************



                xlsRow = 1;
                xlsCol = 3;
                try
                {
                    if (companyLogo != null)
                    {

                        double totalWidth = sheet1.GetColumnWidth(1) + sheet1.GetColumnWidth(3);
                        int totalWidthPixel = (int)(totalWidth * 7.5);
                        int totalheight = (int)((sheet1.GetRowHeight(1) + sheet1.GetRowHeight(2) + sheet1.GetRowHeight(3) + sheet1.GetRowHeight(3)) * 1.50);

                        companyLogo = ReportUtility.FixedSize(companyLogo, totalWidthPixel, totalheight);
                        IPictureShape pic = null;

                        pic = sheet1.Pictures.AddPicture(1, 1, companyLogo);
                        //pic.Height = 80;
                        //pic.Width = 220;
                    }
                }
                catch (Exception ex)
                {
                }

                FactoryName = string.Empty;

                string FactoryAddress = string.Empty;

                if (dtCmp.Rows.Count > 0)
                {
                    CmpName = dtCmp.Rows[0]["CompanyName"].ToString();
                }
                else
                {
                    CmpName = "";
                }
                sheet1.Range[xlsRow, 3].Text = CmpName;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 12;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 17;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                if (dtFactory.Rows.Count > 0)
                {
                    //FactoryName = dsFactory.Tables[0].Rows[0]["PlantName"].ToString();
                    FactoryName = dtFactory.Rows[0]["UserName"].ToString();
                }
                else
                {
                    FactoryName = "";
                }
                sheet1.Range[xlsRow, 3].Text = FactoryName;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 14;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 18;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                if (dtFactory.Rows.Count > 0)
                {
                    FactoryAddress = dtFactory.Rows[0]["Address1"].ToString();
                }
                else
                {
                    FactoryAddress = "";
                }
                sheet1.Range[xlsRow, 3].Text = FactoryAddress;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                //sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 22;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                sheet1.Range[xlsRow, 3].Text = "GST Recievable Report From " + fromDate + " To " + toDate;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;


                #endregion ******************Report Header******************

                #region Freeze Panes

                sheet1.IsDisplayZeros = false;
                sheet1.UsedRange["A7"].FreezePanes();
                sheet1.FirstVisibleColumn = 1;
                sheet1.FirstVisibleRow = 6;

                #endregion Freeze Panes

                #region UsedRange Alignment

                sheet1.UsedRange.WrapText = false;
                sheet1.UsedRange.CellStyle.Font.Size = 10;
                sheet1.Range["A1"].CellStyle.Font.Size = 14;
                sheet1.Range["A2"].CellStyle.Font.Size = 10;
                sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;
                sheet1.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                #endregion UsedRange Alignment

                #region Page Setup
                sheet1.PageSetup.TopMargin = 0.5;
                sheet1.PageSetup.BottomMargin = 0.7;
                sheet1.PageSetup.PrintTitleRows = "$1:$5";
                sheet1.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                sheet1.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + name + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:MM tt").ToString();
                sheet1.PageSetup.LeftMargin = 0.5;
                sheet1.PageSetup.RightMargin = 0.2;
                sheet1.PageSetup.Orientation = ExcelPageOrientation.Portrait;
                sheet1.PageSetup.FitToPagesTall = 0;
                sheet1.PageSetup.FitToPagesWide = 1;
                sheet1.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                sheet1.IsDisplayZeros = false;
                #endregion Page Setup


                sheet1.Name = "GST Receivable";
                return workbook;
            }
            catch (System.Exception ex)
            {

                throw ex;
            }
        }

        private DataTable GetGSTR2SQL(string companyGroupId, string companyId, string plantId, string plantName, string fromDate, string toDate, string taxyearId)
        {
            string strSql = "";
            strSql = @"SELECT SourceType= CASE WHEN V.SourceType='VendorInvoice' THEN 'Expense'
                            WHEN V.SourceType='VendorPayment' THEN 'Vendor Payment'
                            WHEN V.SourceType='InventoryPayable' THEN 'Material' ELSE '' END
							,P.UserName PartyName,P.TINNO GSTIN,V.VoucherNo,format( V.PostingDate,'dd-MMM-yyyy')PostingDate,format( v.VoucherDate,'dd-MMM-yyyy')VoucherDate
							,Particular=CASE WHEN v.SourceType='VendorInvoice' THEN A.UserName
											 WHEN v.SourceType='VendorPayment' THEN AP.UserName
											 ELSE '' END
							,NULL HSNCode
                            , V.DocRefNo,format (V.DocDate,'dd-MMM-yyyy')DocDate
							,NULL GRNNo
							,TaxableAmount=case   when v.SourceType='VendorInvoice' then VD.DrAmount
                            when v.SourceType='VendorPayment' then IWD.Amount else 0 end
							,TaxPercentage= case when v.SourceType='VendorInvoice' then taxc.ValueOfFixed
												  else '' end
							
							,Amount=CASE   WHEN v.SourceType='VendorInvoice' THEN VD.DrAmount
                            WHEN v.SourceType='VendorPayment' THEN IWD.Amount ELSE 0 END
							

                            ,LineItemType=case WHEN v.SourceType='VendorInvoice' THEN 'GL'
                            WHEN v.SourceType='VendorPayment' THEN 'GL'
                            ELSE '' END
                            
                            
                            ,IT.Id,DrAmount=CASE WHEN ITD.AType='Dr' THEN IT.TaxAmount ELSE 0 END,0 CrAmount
	                        
                            ,TC.TaxCategoryType,TC.Code TaxCode,TC.Sequence TCSequence,TC.UserName+'-'+TC.Code TaxCategory,IsNULL(TAXC.IsRCM,0) IsRCM,TAXC.UserName TaxCodeName
                            ,Case when IsNULL(IV.IsExcludingTax,0) = 0 then '-' else 'Yes' end IsNonCreditable ,TAXC.[Type],TAXC.ValueOfFixed
							 ,TA.UserName ActivityName,TAXC.ValueOfFixed [Percentage],S.UserName StateName

                            FROM TRN.InvoiceTax IT
                            LEFT JOIN TRN.InvoiceTaxDetail ITD ON IT.Id=ITD.InvoiceTaxId AND ITD.AType='Dr'
                            LEFT JOIN TRN.Voucher V ON V.Id=IT.VoucherId
                            LEFT JOIN TRN.Invoice IV ON IV.Id=IT.InvoiceId
                            LEFT JOIN HKP.Activity TA ON TA.Id=ITD.ActivityId
                            LEFT JOIN HKP.Party P ON P.Id=IT.PartyId
                            LEFT JOIN HKP.PartyPlant PP ON PP.Id=IV.PartyPlantId
							LEFT JOIN MST.AddressMaster AM ON AM.Id=PP.AddressMasterId
							LEFT JOIN SCS.[State] S ON S.Id=AM.StateId
                            LEFT JOIN MST.TaxCategory TC ON TC.Id=IT.TaxCategoryId
                            LEFT JOIN( select TAC.Id,TAC.UserName,TAC.IsRCM,TAY.[Type],TACD.ValueOfFixed from MST.TaxCode TAC
                            LEFT JOIN MST.TaxCodeYear TAY ON TAY.TaxCodeId=TAC.Id
                            LEFT JOIN MST.TaxCodeDetail TACD ON TACD.TaxCodeId=TAC.Id WHERE TAY.TaxYearId IN ('' ,'3')) TAXC ON TAXC.Id=IT.TaxCodeId
                            LEFT JOIN TRN.VoucherDetail VD ON VD.Id=IT.VoucherDetailId
                            LEFT JOIN HKP.Activity A ON A.Id=VD.ActivityId
                            LEFT JOIN (SELECT IW.InvoiceWriteOffId,IW.ActivityId,SUM(I.Amount) Amount FROM TRN.InvoiceWriteOffDetail IW
                            JOIN TRN.Invoice I ON I.Id=IW.InvoiceId
                            GROUP BY InvoiceWriteOffId,ActivityId) IWD ON IWD.InvoiceWriteOffId=IT.InvoiceWriteOffId
                            LEFT JOIN HKP.Activity AP ON AP.Id=IWD.ActivityId
                            WHERE TC.TaxCategoryType='GST' AND TAXC.IsRCM=0 AND  V.IsPark=0 AND V.PlantId='" + plantId + @"' and V.PostingDate between '" + fromDate + @"' AND '" + toDate + @"'
                            AND v.SourceType IN ('VendorInvoice','VendorPayment','CustomerInvoice') 
                            
                            UNION ALL

							SELECT SourceType= CASE  WHEN V.SourceType='InventoryPayable' THEN 'Material' ELSE '' END
							,P.UserName PartyName,P.TINNO GSTIN
                            ,V.VoucherNo,format( V.PostingDate,'dd-MMM-yyyy')PostingDate,format( v.VoucherDate,'dd-MMM-yyyy')VoucherDate
							,Particular=CASE WHEN v.SourceType='InventoryPayable' THEN MM.UserName ELSE '' END
							,HC.Code HSNCode
							
							, V.DocRefNo,format (V.DocDate,'dd-MMM-yyyy')DocDate,IRD.InventoryReceiveId GRNNo
							, TaxableAmount=CASE WHEN v.SourceType='InventoryPayable' then IRD.MaterialTranAmount
                              ELSE 0 END
							,TaxPercentage= CASE  WHEN v.SourceType='InventoryPayable'  THEN IRT.[Percentage]
												 ELSE 0 END
							, Amount=CASE WHEN v.SourceType='InventoryPayable' THEN IRD.MaterialTranAmount
                              ELSE 0 END

                            ,LineItemType=CASE WHEN v.SourceType='InventoryPayable' THEN 'Material'
                            WHEN v.SourceType='VendorInvoice' THEN 'GL'
                            WHEN v.SourceType='VendorPayment' THEN 'GL'
                            ELSE '' END
                            
                            
                            ,IT.Id,DrAmount=case when ITD.AType='Dr' then IRT.TaxAmount else 0 end,0 CrAmount
	                        
                            ,TC.TaxCategoryType,TC.Code TaxCode,TC.Sequence TCSequence,TC.UserName+'-'+TC.Code TaxCategory,IsNULL(TAXC.IsRCM,0) IsRCM,TAXC.UserName TaxCodeName
                            ,Case when IsNULL(IR.IsNonCreditable,0) = 0 then '-' else 'Yes' end IsNonCreditable,TAXC.[Type],TAXC.ValueOfFixed
                            ,TA.UserName ActivityName,IRT.[Percentage],S.UserName StateName
                            from TRN.InvoiceTax IT
                            left join TRN.InvoiceTaxDetail ITD ON IT.Id=ITD.InvoiceTaxId AND ITD.AType='Dr'
                            LEFT JOIN TRN.Voucher V ON V.Id=IT.VoucherId
                            LEFT JOIN TRN.Invoice IV ON IV.Id=IT.InvoiceId
                            --LEFT JOIN TRN.InvoiceWriteOff IW ON IW.Id=IT.InvoiceWriteOffId
                            LEFT JOIN HKP.Activity TA ON TA.Id=ITD.ActivityId
                            LEFT JOIN HKP.Party P ON P.Id=IT.PartyId
                            LEFT JOIN HKP.PartyPlant PP ON PP.Id=IV.PartyPlantId
							LEFT JOIN MST.AddressMaster AM ON AM.Id=PP.AddressMasterId
							LEFT JOIN SCS.[State] S ON S.Id=AM.StateId
                            LEFT JOIN MST.TaxCategory TC ON TC.Id=IT.TaxCategoryId
                            LEFT JOIN( select TAC.Id,TAC.UserName,TAC.IsRCM,TAY.[Type],TACD.ValueOfFixed from MST.TaxCode TAC
                            LEFT JOIN MST.TaxCodeYear TAY ON TAY.TaxCodeId=TAC.Id
                            LEFT JOIN MST.TaxCodeDetail TACD ON TACD.TaxCodeId=TAC.Id WHERE TAY.TaxYearId IN (" + taxyearId + @") 
							--and tac.IsRCM=0
							) TAXC ON TAXC.Id=IT.TaxCodeId
                            --LEFT JOIN SCS.TaxYear TY ON TY.Id=TAY.TaxYearId
                            LEFT JOIN TRN.InventoryReceive IR ON IR.VoucherId=V.Id
                            LEFT JOIN TRN.InventoryReceiveDetail IRD ON IRD.InventoryReceiveId=IR.Id
                            LEFT JOIN TRN.InventoryReceiveTax IRT ON IRD.Id=IRT.InventoryReceiveDetailId AND IRT.TaxCategoryId=IT.TaxCategoryId
                            LEFT JOIN MST.HSNTaxPercentage HSNP ON IRT.HSNCodeId=HSNP.HSNCodeId AND HSNP.TaxCategoryId=IT.TaxCategoryId
                            LEFT JOIN TRN.InventoryMaterial IM ON IM.Id=IRD.InventoryMaterialId
                            LEFT JOIN MST.MaterialMaster MM ON MM.Id=IM.MaterialMasterId
							LEFT JOIN HKP.HSNCode HC ON HC.Id=MM.HSNCodeId
                            LEFT JOIN TRN.VoucherDetail VD ON VD.Id=IT.VoucherDetailId
                            LEFT JOIN HKP.Activity A ON A.Id=VD.ActivityId
                            LEFT JOIN (SELECT IW.InvoiceWriteOffId,IW.ActivityId,SUM(I.Amount) Amount FROM TRN.InvoiceWriteOffDetail IW
                            JOIN TRN.Invoice I ON I.Id=IW.InvoiceId
                            GROUP BY InvoiceWriteOffId,ActivityId) IWD ON IWD.InvoiceWriteOffId=IT.InvoiceWriteOffId
                            LEFT JOIN HKP.Activity AP ON AP.Id=IWD.ActivityId
                            
                            where TC.TaxCategoryType='GST' AND IR.IsTaxApplicable=0 AND V.IsPark=0
							AND V.PlantId='" + plantId + @"' and V.PostingDate between '" + fromDate + @"' AND '" + toDate + @"'
                            AND v.SourceType='InventoryPayable' and IRT.InventoryServiceId IS NULL 
                            
                            UNION all

                            SELECT SourceType = CASE  WHEN V.SourceType = 'InventoryPayable' THEN 'Service' ELSE '' END
							,P.UserName PartyName, P.TINNO GSTIN
                            ,V.VoucherNo,format(V.PostingDate, 'dd-MMM-yyyy')PostingDate,format(v.VoucherDate, 'dd-MMM-yyyy')VoucherDate
                             ,Particular = CASE WHEN v.SourceType = 'InventoryPayable' THEN SM.UserName
                                ELSE '' END 
								,HC.Code HSNCode
								, V.DocRefNo,format(V.DocDate, 'dd-MMM-yyyy')DocDate, IRD.InventoryReceiveId GRNNo
								,TaxableAmount =case when v.SourceType = 'InventoryPayable' then IRD.Amount else 0 end
								,TaxPercentage = case  when v.SourceType = 'InventoryPayable'  THEN IRT.[Percentage]
                                                 else 0 end
							,Amount =case when v.SourceType = 'InventoryPayable' then IRD.Amount else 0 end
							  , LineItemType =case when v.SourceType = 'InventoryPayable' then 'Service'
                            WHEN v.SourceType = 'VendorInvoice' THEN 'GL'
                            WHEN v.SourceType = 'VendorPayment' THEN 'GL'
                            ELSE '' END
                            
                             
                            ,IT.Id,DrAmount =case when ITD.AType = 'Dr' then IRT.TaxAmount else 0 end,0 CrAmount
	                        
                            ,TC.TaxCategoryType,TC.Code TaxCode, TC.Sequence TCSequence, TC.UserName + '-' + TC.Code TaxCategory,IsNULL(TAXC.IsRCM, 0) IsRCM,TAXC.UserName TaxCodeName
                                   ,Case when IsNULL(IR.IsNonCreditable,0) = 0 then '-' else 'Yes' end IsNonCreditable,TAXC.[Type],TAXC.ValueOfFixed
												 ,TA.UserName ActivityName,IRT.[Percentage],S.UserName StateName
                            from TRN.InvoiceTax IT
                            left join TRN.InvoiceTaxDetail ITD ON IT.Id = ITD.InvoiceTaxId AND ITD.AType = 'Dr'
                            LEFT JOIN TRN.Voucher V ON V.Id = IT.VoucherId
                            LEFT JOIN TRN.Invoice IV ON IV.Id = IT.InvoiceId
                            LEFT JOIN HKP.Activity TA ON TA.Id = ITD.ActivityId
                            LEFT JOIN HKP.Party P ON P.Id=IT.PartyId
                            LEFT JOIN HKP.PartyPlant PP ON PP.Id=IV.PartyPlantId
							LEFT JOIN MST.AddressMaster AM ON AM.Id=PP.AddressMasterId
							LEFT JOIN SCS.[State] S ON S.Id=AM.StateId
                            LEFT JOIN MST.TaxCategory TC ON TC.Id = IT.TaxCategoryId
                            LEFT JOIN(select TAC.Id, TAC.UserName, TAC.IsRCM, TAY.[Type], TACD.ValueOfFixed from MST.TaxCode TAC
                            LEFT JOIN MST.TaxCodeYear TAY ON TAY.TaxCodeId= TAC.Id
                            LEFT JOIN MST.TaxCodeDetail TACD ON TACD.TaxCodeId= TAC.Id WHERE TAY.TaxYearId IN (" + taxyearId + @") 
							) TAXC ON TAXC.Id = IT.TaxCodeId
                            LEFT JOIN TRN.InventoryReceive IR ON IR.VoucherId = V.Id
                            LEFT JOIN TRN.InventoryReceiveTax IRT ON IRT.InventoryReceiveId = IR.Id AND IRT.TaxCategoryId = IT.TaxCategoryId
                            LEFT JOIN MST.HSNTaxPercentage HSNP ON IRT.HSNCodeId = HSNP.HSNCodeId AND HSNP.TaxCategoryId = IT.TaxCategoryId
                            LEFT JOIN TRN.InventoryService IRD ON IRD.Id = IRT.InventoryServiceId
                            LEFT JOIN hkp.ServiceMaster SM ON SM.Id = IRD.ServiceMasterId
							LEFT JOIN HKP.ServiceGroup SG ON SG.Id=SM.ServiceGroupId
							LEFT JOIN HKP.HSNCode HC ON HC.Id=SG.HSNCodeId
                            LEFT JOIN TRN.VoucherDetail VD ON VD.Id = IT.VoucherDetailId
                            LEFT JOIN HKP.Activity A ON A.Id = VD.ActivityId
                            LEFT JOIN(SELECT IW.InvoiceWriteOffId, IW.ActivityId, SUM(I.Amount) Amount FROM TRN.InvoiceWriteOffDetail IW
                            JOIN TRN.Invoice I ON I.Id= IW.InvoiceId
                            GROUP BY InvoiceWriteOffId, ActivityId) IWD ON IWD.InvoiceWriteOffId = IT.InvoiceWriteOffId
                            LEFT JOIN HKP.Activity AP ON AP.Id = IWD.ActivityId
                            where TC.TaxCategoryType = 'GST' AND IR.IsTaxApplicable = 0 AND V.IsPark = 0
                            AND V.PlantId = '" + plantId + @"' AND V.PostingDate BETWEEN '" + fromDate + @"' AND '" + toDate + @"'
                            AND v.SourceType = 'InventoryPayable'  AND IRT.InventoryReceiveDetailId IS NULL 
                            ORDER BY LineItemType,ValueOfFixed,Percentage ";

            return _sqlRepository.GetDataTable(strSql);

        }

        #endregion

        #region TDS Deduction
        public IWorkbook GetTdsDeductionReport(string companyGroupId, string companyId, string plantId, string plantName, string fromDate, string toDate, string name)
        {
            clsReport objRpt = null;
            clsReport objRptSR = null;
            try
            {

                ExcelEngine excelEngine = null;
                IApplication application = null;
                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;
                excelEngine.Excel.DefaultVersion = ExcelVersion.Excel2013;
                var reportUtility = new ReportUtility();
                var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
                workbook.Version = ExcelVersion.Excel2013;
                var sheet1 = workbook.Worksheets[0];

                #region Logo
                string strPath = "";
                Image companyLogo = null;
                try
                {
                    DataTable dtCompanyImage = _sqlRepository.GetDataTable("SELECT * FROM ORG.COMPANY WHERE ID = '" + companyId + @"'");

                    strPath = Path.Combine(ResourcesPathReader.GetLogoOrImagePath(), dtCompanyImage.Rows[0]["Image"].ToString());  // IDCardEng.xlsx
                    companyLogo = Image.FromFile(strPath);
                }
                catch (Exception)
                {
                }
                #endregion
                objRpt = new clsReport();

                objRptSR = new clsReport(_sqlRepository);

                DataTable dtRCMPayable = null;
                string taxyearId = GetTaxYearId(fromDate, toDate, companyId);


                dtRCMPayable = GetTdsDedutionData(companyGroupId, companyId, plantId, plantName, fromDate, toDate, taxyearId);
                if (dtRCMPayable.Rows.Count == 0)
                {
                    throw new Exception("No Data Found....");
                }

                DataTable dtCmp = objRptSR.SelectedCompanyDT(plantId);

                DataTable dtFactory = objRptSR.SelectedPlantDT(plantId);

                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;

                int xlsRow = 1, xlsCol = 1;
                int endXlsCol = 1;
                string FactoryName = "";
                string CmpName = "";
                xlsRow = 6;
                sheet1.Range[xlsRow - 1, 1].Text = "Report Ref No:";
                sheet1.Range[xlsRow - 1, 1].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow - 1, 1].RowHeight = 20;
                sheet1.Range[xlsRow - 1, 1].CellStyle.Font.Bold = true;

                sheet1.Range[xlsRow - 1, 5].Text = "Payable";
                sheet1.Range[xlsRow - 1, 5].CellStyle.Font.Size = 10;
                // sheet1.Range[xlsRow - 1, 4,xlsRow-1,7].RowHeight = 30;
                sheet1.Range[xlsRow - 1, 5].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow - 1, 4, xlsRow - 1, 7].BorderAround(ExcelLineStyle.Thin);

                sheet1.Range[xlsRow - 1, 5].CellStyle.Font.Bold = true;

                sheet1.Range[xlsRow - 1, 9].Text = "Tax";
                sheet1.Range[xlsRow - 1, 9].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow - 1, 9].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                //sheet1.Range[xlsRow - 1, 7].BorderInside(ExcelLineStyle.Thin);
                sheet1.Range[xlsRow - 1, 4, xlsRow - 1, 11].BorderAround(ExcelLineStyle.Thin);
                sheet1.Range[xlsRow - 1, 9].CellStyle.Font.Bold = true;

                int colSLNO = xlsCol;
                sheet1[xlsRow, xlsCol].Text = "SL. No";
                sheet1[xlsRow, xlsCol].ColumnWidth = 7;
                sheet1[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignRight;
                xlsCol++;

                //COL++;


                //COL++;

                int PartyName = xlsCol; // Party
                sheet1.Range[xlsRow, xlsCol].Text = "Suppliers Name";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 40;
                xlsCol++;

                sheet1[xlsRow, xlsCol].Text = "PAN No";
                int colPenNO = xlsCol;
                sheet1[xlsRow, xlsCol].ColumnWidth = 7;
                sheet1[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignRight;
                xlsCol++;


                int GSTIN = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "GSTIN";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                // xlsCol++;

                xlsCol++;
                int iInvoiceVoucherNo = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Invoice Voucher No";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;

                xlsCol++;
                int iInvoicePostingDate = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Posting Date";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;

                xlsCol++;
                int iInvoiceDocDate = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Doc Date";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;

                xlsCol++;
                int iInvoiceDocRefNo = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "DocRef No";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;

                //VoucherNo
                xlsCol++;
                int iVoucherNo = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "TDS Voucher No";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;

                xlsCol++;
                int iPostingDate = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Posting Date";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;

                xlsCol++;
                int iDocDate = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Doc Date";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;

                xlsCol++;
                int iDocRefNo = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "DocRef No";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                xlsCol++;

                int iTDSPer = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "TDS Name";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 30;
                xlsCol++;

                int iPercentage = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Percentage";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 10;
                xlsCol++;

                int iSection = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Section";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 10;
                xlsCol++;

                int iInvoiceAmount = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Invoice Amount";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                sheet1[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignRight;



                xlsCol++;
                int iTaxableAmount = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Taxable Amount";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                sheet1[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignRight;
                xlsCol++;

                int CrAmount = xlsCol; // Doc Ref
                sheet1.Range[xlsRow, xlsCol].Text = "TDS Amount";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                sheet1[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignRight;
                // xlsCol++;

                DataTable dtTaxCode = null;


                dtRCMPayable.DefaultView.Sort = "TCSequence";
               
                endXlsCol = xlsCol;

                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].WrapText = true;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 23;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;




                string voucherNo = "";
                string Percentage = "";
                int startRow = 0;
                int perStartRow = 0;
                string formula = "";
                string formula2 = "";
                string totalFormula = "";
                string totalFormula2 = "";

                string lineItemPercentageType = "";
                string ValueOfFixedNew = "";
                xlsRow++;
                startRow = xlsRow;
                perStartRow = xlsRow;
                bool isFirst = true;
                int sl = 0;

                //int SerialNumber = 0;
                for (int i = 0; i < dtRCMPayable.Rows.Count; i++)
                {

                    if (voucherNo != dtRCMPayable.Rows[i]["VoucherNo"].ToString())
                    {
                        sheet1[xlsRow, colSLNO].Number = (i + 1);
                        sl++;

                        if (dtRCMPayable.Rows[i]["LineItemType"].ToString().ToUpper() == "GL")
                        {
                            lineItemPercentageType = "ValueOfFixed";
                        }
                        if (dtRCMPayable.Rows[i]["LineItemType"].ToString().ToUpper() == "MATERIAL")
                        {
                            lineItemPercentageType = "Percentage";
                        }
                        if (Percentage != dtRCMPayable.Rows[i][lineItemPercentageType].ToString())
                        {
                            if (isFirst == false)
                            {

                                //sheet1[perStartRow, iPostingDate, xlsRow - 1, iPostingDate].BorderAround(ExcelLineStyle.Hair);
                                sheet1[perStartRow, PartyName, xlsRow - 1, PartyName].BorderAround(ExcelLineStyle.Hair);
                                sheet1[perStartRow, colPenNO, xlsRow - 1, colPenNO].BorderAround(ExcelLineStyle.Hair);

                                sheet1[perStartRow, GSTIN, xlsRow - 1, GSTIN].BorderAround(ExcelLineStyle.Hair);
                                sheet1[perStartRow, iDocRefNo, xlsRow - 1, iDocRefNo].BorderAround(ExcelLineStyle.Hair);
                                sheet1[perStartRow, iVoucherNo, xlsRow - 1, iVoucherNo].BorderAround(ExcelLineStyle.Hair);
                                sheet1[perStartRow, iPostingDate, xlsRow - 1, iPostingDate].BorderAround(ExcelLineStyle.Hair);
                                sheet1[perStartRow, iDocDate, xlsRow - 1, iDocDate].BorderAround(ExcelLineStyle.Hair);

                                sheet1[perStartRow, CrAmount, xlsRow - 1, CrAmount].BorderAround(ExcelLineStyle.Hair);
                                sheet1[perStartRow, iTaxableAmount, xlsRow - 1, iTaxableAmount].BorderAround(ExcelLineStyle.Hair);
                                formula = "SUM(" + clsStaticInfo.GetxlsCol(iTaxableAmount) + perStartRow + ":" + clsStaticInfo.GetxlsCol(iTaxableAmount) + (xlsRow - 1) + ")";

                                formula2 = "SUM(" + clsStaticInfo.GetxlsCol(CrAmount) + perStartRow + ":" + clsStaticInfo.GetxlsCol(CrAmount) + (xlsRow - 1) + ")";
                               
                                sheet1.Range[xlsRow, 1, xlsRow, 1].Text = "Total";

                                sheet1[xlsRow, iTaxableAmount, xlsRow, iTaxableAmount].Formula = formula;
                                sheet1[xlsRow, CrAmount, xlsRow, CrAmount].Formula = formula2;
                                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Font.Bold = true;
                                totalFormula += (clsStaticInfo.GetxlsCol(iTaxableAmount) + xlsRow).ToString() + "+";
                                totalFormula2 += (clsStaticInfo.GetxlsCol(CrAmount) + xlsRow).ToString() + "+";


                                //SerialNumber++;
                                //sheet1[xlsRow, colSLNO].Text = (SerialNumber).ToString();
                                xlsRow++;


                            }


                            xlsRow++;
                            sheet1.Range[xlsRow - 1, 1].Text = dtRCMPayable.Rows[i]["ValueOfFixedNew"].ToString();
                            sheet1.Range[xlsRow - 1, 1].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                            perStartRow = xlsRow;
                            isFirst = false;

                        }

                        //sheet1.Range[xlsRow, iPostingDate].Text = dtRCMPayable.Rows[i]["PostingDate"].ToString();
                        sheet1[xlsRow, colSLNO].Number = sl;
                        sheet1.Range[xlsRow, PartyName].Text = dtRCMPayable.Rows[i]["PartyName"].ToString();

                        sheet1.Range[xlsRow, colPenNO].Text = dtRCMPayable.Rows[i]["PanNo"].ToString();
                        sheet1.Range[xlsRow, iInvoiceVoucherNo].Text = dtRCMPayable.Rows[i]["InvoiceVoucherNo"].ToString();
                        sheet1.Range[xlsRow, iInvoicePostingDate].DateTime = Convert.ToDateTime(dtRCMPayable.Rows[i]["InvoicePostingDate"].ToString());
                        //sheet1.Range[xlsRow, iInvoicePostingDate].Text = clsStaticInfo.GetDateTaxFormate(dtRCMPayable.Rows[i]["InvoicePostingDate"].ToString());
                        sheet1.Range[xlsRow, iInvoiceDocRefNo].Text = dtRCMPayable.Rows[i]["InvoieDocRefNo"].ToString();
                        sheet1.Range[xlsRow, iInvoiceDocDate].Text = dtRCMPayable.Rows[i]["InvoiceDocDate"].ToString();
                        sheet1.Range[xlsRow, GSTIN].Text = dtRCMPayable.Rows[i]["GSTIN"].ToString();

                        sheet1.Range[xlsRow, iTDSPer].Text = dtRCMPayable.Rows[i]["TDSPer"].ToString();//TaxableAmount
                                                                                                       //sheet1.Range[xlsRow, iTDSPer].NumberFormat = reportUtility.NumberFormatDecimalTwo();

                        sheet1.Range[xlsRow, iPercentage].Number = clsStaticInfo.dbl(dtRCMPayable.Rows[i]["ValueOfFixed"].ToString());//TaxableAmount
                        sheet1.Range[xlsRow, iPercentage].NumberFormat = reportUtility.NumberFormatDecimalTwo();

                        sheet1.Range[xlsRow, iSection].Text = dtRCMPayable.Rows[i]["Section"].ToString();//TaxableAmount

                        sheet1.Range[xlsRow, iInvoiceAmount].Number = clsStaticInfo.dbl(dtRCMPayable.Rows[i]["InvoiceAmount"].ToString());//TaxableAmount
                        sheet1.Range[xlsRow, iInvoiceAmount].NumberFormat = reportUtility.NumberFormatDecimalTwo();

                        sheet1.Range[xlsRow, iDocRefNo].Text = dtRCMPayable.Rows[i]["DocRefNo"].ToString();
                        sheet1.Range[xlsRow, iVoucherNo].Text = dtRCMPayable.Rows[i]["VoucherNo"].ToString();
                        sheet1.Range[xlsRow, iPostingDate].Text = dtRCMPayable.Rows[i]["PostingDate"].ToString();
                        sheet1.Range[xlsRow, iDocDate].Text = dtRCMPayable.Rows[i]["DocDate"].ToString();

                        //sheet1.Range[xlsRow, CrAmount].Text = dtRCMPayable.Rows[i]["CrAmount"].ToString();//TaxableAmount
                        sheet1.Range[xlsRow, CrAmount].Number = clsStaticInfo.dbl(dtRCMPayable.Rows[i]["CrAmount"].ToString());//TaxableAmount
                        sheet1.Range[xlsRow, CrAmount].NumberFormat = reportUtility.NumberFormatDecimalTwo();

                        sheet1.Range[xlsRow, iTaxableAmount].Number = clsStaticInfo.dbl(dtRCMPayable.Rows[i]["TaxableAmount"].ToString());//TaxableAmount
                        sheet1.Range[xlsRow, iTaxableAmount].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                        dtRCMPayable.DefaultView.RowFilter = "VoucherNo = '" + dtRCMPayable.Rows[i]["VoucherNo"].ToString() + "'";

                       

                        Percentage = dtRCMPayable.Rows[i][lineItemPercentageType].ToString();
                        xlsRow++;
                    }

                    voucherNo = dtRCMPayable.Rows[i]["VoucherNo"].ToString();


                }

                //sheet1[perStartRow, iPostingDate, xlsRow - 1, iPostingDate].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, PartyName, xlsRow - 1, PartyName].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, GSTIN, xlsRow - 1, GSTIN].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iDocRefNo, xlsRow - 1, iDocRefNo].BorderAround(ExcelLineStyle.Hair);
                // sheet1[perStartRow, iVoucherNo, xlsRow - 1, iVoucherNo].BorderAround(ExcelLineStyle.Hair);

                sheet1[perStartRow, CrAmount, xlsRow - 1, CrAmount].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iTaxableAmount, xlsRow - 1, iTaxableAmount].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iTaxableAmount, xlsRow - 1, iTaxableAmount].BorderAround(ExcelLineStyle.Hair);



                sheet1.Range[xlsRow, 1, xlsRow, 1].Text = "Total";
                formula = "SUM(" + clsStaticInfo.GetxlsCol(iTaxableAmount) + perStartRow + ":" + clsStaticInfo.GetxlsCol(iTaxableAmount) + (xlsRow - 1) + ")";
                formula2 = "SUM(" + clsStaticInfo.GetxlsCol(CrAmount) + perStartRow + ":" + clsStaticInfo.GetxlsCol(CrAmount) + (xlsRow - 1) + ")";

                sheet1[xlsRow, iTaxableAmount, xlsRow, iTaxableAmount].Formula = formula;
                sheet1[xlsRow, iTaxableAmount, xlsRow, iTaxableAmount].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);
                sheet1[xlsRow, CrAmount, xlsRow, CrAmount].Formula = formula2;
                sheet1[xlsRow, CrAmount, xlsRow, CrAmount].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Font.Bold = true;
                totalFormula += (clsStaticInfo.GetxlsCol(iTaxableAmount) + xlsRow).ToString() + "+";
                totalFormula2 += (clsStaticInfo.GetxlsCol(CrAmount) + xlsRow).ToString() + "+";





                xlsRow++;
                xlsRow++;

                sheet1.Range[xlsRow, 1, xlsRow, 1].Text = "Grand Total";

                sheet1[xlsRow, iTaxableAmount, xlsRow, iTaxableAmount].Formula = totalFormula.Remove(totalFormula.Length - 1);
                sheet1[xlsRow, iTaxableAmount, xlsRow, iTaxableAmount].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);
                sheet1[xlsRow, CrAmount, xlsRow, CrAmount].Formula = totalFormula2.Remove(totalFormula2.Length - 1);
                sheet1[xlsRow, CrAmount, xlsRow, CrAmount].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Font.Bold = true;




                #region ******************Report Header******************



                xlsRow = 1;
                xlsCol = 3;
                try
                {
                    if (companyLogo != null)
                    {

                        double totalWidth = sheet1.GetColumnWidth(1) + sheet1.GetColumnWidth(GSTIN);
                        int totalWidthPixel = (int)(totalWidth * 7.5);
                        int totalheight = (int)((sheet1.GetRowHeight(1) + sheet1.GetRowHeight(2) + sheet1.GetRowHeight(3) + sheet1.GetRowHeight(3)) * 1.50);

                        companyLogo = ReportUtility.FixedSize(companyLogo, totalWidthPixel, totalheight);
                        IPictureShape pic = null;

                        pic = sheet1.Pictures.AddPicture(1, 1, companyLogo);
                        //pic.Height = 80;
                        //pic.Width = 220;
                    }
                }
                catch (Exception ex)
                {
                }

                FactoryName = string.Empty;

                string FactoryAddress = string.Empty;

                if (dtCmp.Rows.Count > 0)
                {
                    CmpName = dtCmp.Rows[0]["CompanyName"].ToString();
                }
                else
                {
                    CmpName = "";
                }
                sheet1.Range[xlsRow, 3].Text = CmpName;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 12;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 17;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                if (dtFactory.Rows.Count > 0)
                {
                    //FactoryName = dsFactory.Tables[0].Rows[0]["PlantName"].ToString();
                    FactoryName = dtFactory.Rows[0]["UserName"].ToString();
                }
                else
                {
                    FactoryName = "";
                }
                sheet1.Range[xlsRow, 3].Text = FactoryName;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 14;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 18;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                if (dtFactory.Rows.Count > 0)
                {
                    FactoryAddress = dtFactory.Rows[0]["Address1"].ToString();
                }
                else
                {
                    FactoryAddress = "";
                }
                sheet1.Range[xlsRow, 3].Text = FactoryAddress;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                //sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 22;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                sheet1.Range[xlsRow, 3].Text = "TDS Deduction Report From " + fromDate + " To " + toDate;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;


                #endregion ******************Report Header******************

                #region Freeze Panes

                sheet1.IsDisplayZeros = false;
                sheet1.UsedRange["A7"].FreezePanes();
                sheet1.FirstVisibleColumn = 1;
                sheet1.FirstVisibleRow = 6;

                #endregion Freeze Panes

                #region UsedRange Alignment

                sheet1.UsedRange.WrapText = false;
                sheet1.UsedRange.CellStyle.Font.Size = 10;
                sheet1.Range["A1"].CellStyle.Font.Size = 14;
                sheet1.Range["A2"].CellStyle.Font.Size = 10;
                sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;
                sheet1.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                #endregion UsedRange Alignment

                #region Page Setup
                sheet1.PageSetup.TopMargin = 0.5;
                sheet1.PageSetup.BottomMargin = 0.7;
                sheet1.PageSetup.PrintTitleRows = "$1:$5";
                sheet1.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                sheet1.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + name + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:MM tt").ToString();
                sheet1.PageSetup.LeftMargin = 0.5;
                sheet1.PageSetup.RightMargin = 0.2;
                sheet1.PageSetup.Orientation = ExcelPageOrientation.Portrait;
                sheet1.PageSetup.FitToPagesTall = 0;
                sheet1.PageSetup.FitToPagesWide = 1;
                sheet1.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                sheet1.IsDisplayZeros = false;
                #endregion Page Setup


                sheet1.Name = "TDS Deduction";
                return workbook;
            }
            catch (System.Exception ex)
            {

                throw ex;
            }
        }

        private DataTable GetTdsDedutionData(string companyGroupId, string companyId, string plantId, string plantName, string fromDate, string toDate, string taxyearId)
        {
            string strSql = "";
            strSql = @"Select distinct * from (
                 SELECT SourceType= case when V.SourceType='VendorInvoice' then 'Inbound Invoice'
						                when V.SourceType='VendorPayment' then 'Vendor Payment'
						                when V.SourceType='CreditNoteSetOff' then 'Credit Note SetOff'
						                when V.SourceType='InventoryPayable' then 'Purchase' else '' end
                ,InvoiceVoucherNo=case when IWD.VoucherNo<>'' then IWD.VoucherNo else V.VoucherNo end ,IWD.InventoryReceiveId
				,InvoicePostingDate=case when IWD.PostingDate<>'' then  IWD.PostingDate else v.PostingDate end 
				,InvoieDocRefNo=case when iwd.DocRefNo<>'' then iwd.DocRefNo else v.DocRefNo end
				,InvoiceDocDate=case when IWD.DocDate<>'' then format( IWD.DocDate, 'dd-MMM-yyyy') else format( V.DocDate, 'dd-MMM-yyyy') end
				,V.VoucherNo,Format(V.PostingDate,'dd-MMM-yyyy') PostingDate,V.DocRefNo,format( V.DocDate, 'dd-MMM-yyyy')DocDate, P.UserName PartyName,P.TINNO GSTIN 
                ,LineItemType=case when v.SourceType='InventoryPayable' then 'Material' 
				                   when v.SourceType='VendorInvoice' then 'GL'
				                   when v.SourceType='VendorPayment' then 'GL'
				                   when v.SourceType='CreditNoteSetOff' then 'GL'
				                   else '' end
				                   ,Particular=case when v.SourceType='InventoryPayable' then TXC.UserName 
									                WHEN v.SourceType='VendorInvoice' THEN A.UserName
									                WHEN v.SourceType='VendorPayment' THEN AP.UserName
									                WHEN v.SourceType='CreditNoteSetOff' THEN AP.UserName
				                   else '' end
				  ,TaxableAmount=case when IWD.InventoryReceiveId<>'' then IRD.TotalMaterialTranAmount
									when SAM.ServiceAcknowledgementMasterId<>'' then SAM.TotalMaterialTranAmount
					                when v.SourceType='VendorInvoice' then VD.DrAmount
					                when v.SourceType='VendorPayment' then IWD.TaxableAmount
					                when v.SourceType='CreditNoteSetOff' then IWD.Amount-IT.TaxAmount	else 0 end
                ,InvoiceAmount=case when v.SourceType='InventoryPayable' then IRD.TotalMaterialTranAmount
					                when v.SourceType='VendorInvoice' then VD.DrAmount	
					                when v.SourceType='CreditNoteSetOff' then VD.DrAmount	
					                when v.SourceType='VendorPayment' then IWD.Amount	else 0 end
                --,IT.Id
                ,0 DrAmount ,CrAmount=case when ITD.AType='Cr' then IT.TaxAmount else 0 end
                ,TC.Code TaxCode ,TC.Sequence TCSequence,TC.TaxCategoryType,TC.UserName+'-'+TC.Code TaxCategory,IsNULL(TAXC.IsRCM,0) IsRCM,TAXC.UserName TaxCodeName
                ,IsNULL(IV.IsExcludingTax,0) IsExcludingTax,IsNULL(IR.IsTaxApplicable,0) IsTaxApplicable,TAXC.[Type],ValueOfFixedNew = TAXC.UserName +' - '+ convert(varchar,TAXC.ValueOfFixed),TAXC.ValueOfFixed
                ,IsNULL(HSNP.[Percentage],0) Percentage--,MM.HSNCodeId,MM.UserName Material
                ,P.VATResistrationNo PanNo,TXC.UserName TDSPer,TXC.Code Section

                from TRN.InvoiceTax IT 
                left join TRN.InvoiceTaxDetail ITD  ON IT.Id=ITD.InvoiceTaxId AND ITD.AType='Cr'
                LEFT JOIN TRN.Voucher V ON V.Id=IT.VoucherId
				LEFT JOIN MST.TaxCode TXC ON TXC.Id=IT.TaxCodeId
                LEFT JOIN TRN.Invoice IV ON IV.Id=IT.InvoiceId
                LEFT JOIN TRN.InvoiceWriteOff IW ON IW.Id=IT.InvoiceWriteOffId
                LEFT JOIN HKP.Party P ON P.Id=IT.PartyId
				LEFT JOIN MST.TaxCategory TC ON TC.Id=IT.TaxCategoryId AND TC.TaxCategoryType='TDS'
                LEFT JOIN( select distinct TAC.Id,TAC.UserName,TAC.IsRCM,TAY.[Type],TACD.ValueOfFixed from MST.TaxCode TAC 
	                LEFT JOIN MST.TaxCodeYear TAY ON TAY.TaxCodeId=TAC.Id
	               LEFT JOIN MST.TaxCodeDetail TACD ON TACD.TaxCodeId=TAC.Id WHERE TAY.TaxYearId IN (" + taxyearId + @") AND TACD.TaxCodeYearId=TAY.Id) TAXC ON TAXC.Id=IT.TaxCodeId
                LEFT JOIN TRN.InventoryReceive IR ON IR.VoucherId=V.Id
                LEFT JOIN TRN.InventoryReceiveTax IRT ON IRT.InventoryReceiveId=IR.Id --AND IRT.TaxCategoryId=IT.TaxCategoryId
                LEFT JOIN MST.HSNTaxPercentage HSNP ON  IRT.HSNCodeId=HSNP.HSNCodeId AND HSNP.TaxCategoryId=IT.TaxCategoryId 
                LEFT JOIN (SELECT SUM(D.DrAmount) DrAmount,D.VoucherId,D.ActivityId,D.InvoiceWriteOffDetailId 
				FROM  TRN.VoucherDetail D LEFT JOIN TRN.Voucher VV ON VV.Id=D.VoucherId 
				WHERE D.InvoiceTaxDetailId IS NULL AND D.DrAmount<> 0 
				GROUP BY D.VoucherId,D.ActivityId,D.InvoiceWriteOffDetailId
				) VD ON VD.VoucherId=IT.VoucherId 
                LEFT JOIN HKP.Activity A ON A.Id=VD.ActivityId 
                LEFT JOIN (SELECT IT.InvoiceWriteOffId,I.InventoryReceiveId,IW.ActivityId
				, I.Amount Amount,SUM(VD.DrAmount) TaxableAmount
				,V.VoucherNo,V.PostingDate,V.DocRefNo,V.DocDate
				FROM TRN.InvoiceWriteOffDetail IW 
							JOIN TRN.InvoiceTax IT ON IT.InvoiceWriteOffId=iw.InvoiceWriteOffId
			                JOIN TRN.Invoice I ON I.Id=IW.InvoiceId
							JOIN TRN.Voucher V ON V.Id=I.VoucherId
							JOIN TRN.VoucherDetail VD ON VD.VoucherId=I.VoucherId AND VD.DrAmount>0 AND VD.InvoiceTaxDetailId IS NULL
		                GROUP BY IT.InvoiceWriteOffId,IW.ActivityId,V.VoucherNo,I.Amount,V.PostingDate,V.DocRefNo,V.DocDate,I.InventoryReceiveId
						) IWD ON IWD.InvoiceWriteOffId=IT.InvoiceWriteOffId
                LEFT JOIN HKP.Activity AP ON AP.Id=IWD.ActivityId
				LEFT JOIN (select InventoryReceiveId,sum(TotalMaterialTranAmount) TotalMaterialTranAmount from TRN.InventoryReceiveDetail group by InventoryReceiveId)IRD ON IWD.InventoryReceiveId=IRD.InventoryReceiveId
                LEFT JOIN TRN.Invoice SIV ON SIV.VoucherId=V.Id
				LEFT JOIN (select sad.ServiceAcknowledgementMasterId,IWD.InvoiceWriteOffId,sum(sad.Amount) TotalMaterialTranAmount from TRN.ServiceAcknowledgementDetail sad
				join TRN.ServiceAcknowledgementMaster sam on sam.Id=sad.ServiceAcknowledgementMasterId
				join trn.Invoice I on I.ServiceAcknowledgementMasterId =sam.Id
				join trn.InvoiceWriteOffDetail IWD ON IWD.InvoiceId=I.Id
				group by sad.ServiceAcknowledgementMasterId,IWD.InvoiceWriteOffId)SAM ON IT.InvoiceWriteOffId=SAM.InvoiceWriteOffId
                WHERE TC.TaxCategoryType='TDS' AND ITD.AType='Cr' 
				AND V.PostingDate between '" + fromDate + "' AND '" + toDate + "' and V.PlantId = '" + plantId + @"' and V.IsPark=0
                
                UNION ALL
                SELECT SourceType=V.SourceType ,IWD.VoucherNo InvoiceVoucherNo,NULL InventoryReceiveId
				,IWD.PostingDate InvoicePostingDate,iwd.DocRefNo InvoieDocRefNo,format( IWD.DocDate, 'dd-MMM-yyyy') InvoiceDocDate
				,V.VoucherNo,Format(V.PostingDate,'dd-MMM-yyyy') PostingDate,V.DocRefNo,format( V.DocDate, 'dd-MMM-yyyy')DocDate, P.UserName PartyName,P.TINNO GSTIN 
                ,LineItemType='GL' ,Particular=TXC.UserName
				  ,TaxableAmount=IWD.TaxableAmount ,InvoiceAmount=vd.DrAmount
                --,IT.Id
                ,0 DrAmount ,CrAmount=case when ITD.AType='Cr' then IT.TaxAmount else 0 end
                ,TC.Code TaxCode ,TC.Sequence TCSequence,TC.TaxCategoryType,TC.UserName+'-'+TC.Code TaxCategory,IsNULL(TAXC.IsRCM,0) IsRCM,TAXC.UserName TaxCodeName
                ,0 IsExcludingTax,IsNULL(IR.IsTaxApplicable,0) IsTaxApplicable,TAXC.[Type],ValueOfFixedNew = TAXC.UserName +' - '+ convert(varchar,TAXC.ValueOfFixed),TAXC.ValueOfFixed
                ,IsNULL(HSNP.[Percentage],0) Percentage  ,P.VATResistrationNo PanNo,TXC.UserName TDSPer,TXC.Code Section
                FROM TRN.AdditionalTax IT 
                 LEFT JOIN TRN.AdditionalTaxDetail ITD  ON IT.Id=ITD.AdditionalTaxId AND ITD.AType='Cr'
                 LEFT JOIN TRN.Voucher V ON V.Id=IT.VoucherId
				LEFT JOIN MST.TaxCode TXC ON TXC.Id=ITD.TaxCodeId
                  JOIN TRN.adjustmentnote IV ON IV.Id=IT.Adjustmentnoteid
                LEFT JOIN TRN.InvoiceWriteOff IW ON IW.Id=IT.InvoiceWriteOffId
                LEFT JOIN HKP.Party P ON P.Id=IT.PartyId
				LEFT JOIN MST.TaxCategory TC ON TC.Id=ITD.TaxCategoryId AND TC.TaxCategoryType='TDS'
                LEFT JOIN( select distinct TAC.Id,TAC.UserName,TAC.IsRCM,TAY.[Type],TACD.ValueOfFixed from MST.TaxCode TAC 
	                LEFT JOIN MST.TaxCodeYear TAY ON TAY.TaxCodeId=TAC.Id
	               LEFT JOIN MST.TaxCodeDetail TACD ON TACD.TaxCodeId=TAC.Id WHERE TAY.TaxYearId IN (" + taxyearId + @") AND TACD.TaxCodeYearId=TAY.Id) TAXC ON TAXC.Id=ITD.TaxCodeId
                LEFT JOIN TRN.InventoryReceive IR ON IR.VoucherId=V.Id
                LEFT JOIN TRN.InventoryReceiveTax IRT ON IRT.InventoryReceiveId=IR.Id --AND IRT.TaxCategoryId=IT.TaxCategoryId
                LEFT JOIN MST.HSNTaxPercentage HSNP ON  IRT.HSNCodeId=HSNP.HSNCodeId AND HSNP.TaxCategoryId=ITD.TaxCategoryId 
                LEFT JOIN (SELECT SUM(D.DrAmount) DrAmount,D.VoucherId 
				FROM  TRN.VoucherDetail D LEFT JOIN TRN.Voucher VV ON VV.Id=D.VoucherId 
				WHERE  D.DrAmount<> 0 
				GROUP BY D.VoucherId 
				) VD ON VD.VoucherId=IT.VoucherId 
                LEFT JOIN (SELECT I.Id AdjustmentNoteId
				, I.Amount Amount,SUM(vd.DrAmount) TaxableAmount
				,V.VoucherNo,V.PostingDate,V.DocRefNo,V.DocDate
				FROM   TRN.Adjustmentnote I  
							JOIN TRN.Voucher V ON V.Id=I.VoucherId
							JOIN TRN.VoucherDetail VD ON VD.VoucherId=I.VoucherId AND VD.DrAmount>0 AND VD.AdjustmentnoteDetailId IS NULL 
							  and VD.InvoiceTaxdetailId IS NULL
							  GROUP BY I.Id ,V.VoucherNo,I.Amount,V.PostingDate,V.DocRefNo,V.DocDate
						) IWD ON IWD.Adjustmentnoteid=IT.Adjustmentnoteid
				WHERE TC.TaxCategoryType='TDS' AND ITD.AType='Cr' 
				AND V.PostingDate between '" + fromDate + "' AND '" + toDate + "' and V.PlantId = '" + plantId + @"' and V.IsPark=0

                UNION ALL
                SELECT SourceType= case when V.SourceType='VendorInvoice' then 'Inbound Invoice'
						                when V.SourceType='VendorPayment' then 'Vendor Payment'
						                when V.SourceType='CreditNoteSetOff' then 'Credit Note SetOff'
						                when V.SourceType='InventoryPayable' then 'Purchase' else V.SourceType end
                ,IWD.VoucherNo InvoiceVoucherNo,IWD.InventoryReceiveId
				,IWD.PostingDate InvoicePostingDate,iwd.DocRefNo InvoieDocRefNo,format( IWD.DocDate, 'dd-MMM-yyyy') InvoiceDocDate
				,V.VoucherNo,Format(V.PostingDate,'dd-MMM-yyyy') PostingDate,V.DocRefNo,format( V.DocDate, 'dd-MMM-yyyy')DocDate, P.UserName PartyName,P.TINNO GSTIN 
                ,LineItemType=case when v.SourceType='InventoryPayable' then 'Material' 
				                   when v.SourceType='VendorInvoice' then 'GL'
				                   when v.SourceType='VendorPayment' then 'GL'
				                   when v.SourceType='CreditNoteSetOff' then 'GL'
				                   else 'GL' end
				 ,Particular=case when v.SourceType='InventoryPayable' then TXC.UserName 
								  WHEN v.SourceType='VendorInvoice' THEN A.UserName
                                  WHEN v.SourceType='VendorPayment' THEN AP.UserName
				                  else '' end
				  ,TaxableAmount=case when IWD.InventoryReceiveId<>'' then IRD.TotalMaterialTranAmount
									when SAM.ServiceAcknowledgementMasterId<>'' then SAM.TotalMaterialTranAmount
					                when v.SourceType='VendorInvoice' then VD.DrAmount
					                when v.SourceType='VendorPayment' then IWD.TaxableAmount
					                when v.SourceType='CreditNoteSetOff' then IWD.Amount-IT.TaxAmount	else 0 end
                ,InvoiceAmount=case when v.SourceType='InventoryPayable' then IRD.TotalMaterialTranAmount
					                when v.SourceType='VendorInvoice' then VD.DrAmount	
					                when v.SourceType='CreditNoteSetOff' then VD.DrAmount	
					                when v.SourceType='VendorPayment' then IWD.Amount	else 0 end
                --,IT.Id
                ,0 DrAmount ,CrAmount=case when ITD.AType='Cr' then IT.TaxAmount else 0 end
                ,TC.Code TaxCode ,TC.Sequence TCSequence,TC.TaxCategoryType,TC.UserName+'-'+TC.Code TaxCategory,IsNULL(TAXC.IsRCM,0) IsRCM,TAXC.UserName TaxCodeName
                ,0 IsExcludingTax,IsNULL(IR.IsTaxApplicable,0) IsTaxApplicable,TAXC.[Type],ValueOfFixedNew = TAXC.UserName +' - '+ convert(varchar,TAXC.ValueOfFixed),TAXC.ValueOfFixed
                ,IsNULL(HSNP.[Percentage],0) Percentage
                ,P.VATResistrationNo PanNo,TXC.UserName TDSPer,TXC.Code Section

                FROM TRN.AdditionalTax IT 
                left join TRN.AdditionalTaxDetail ITD  ON IT.Id=ITD.AdditionalTaxId AND ITD.AType='Cr'
                LEFT JOIN TRN.Voucher V ON V.Id=IT.VoucherId
				LEFT JOIN MST.TaxCode TXC ON TXC.Id=ITD.TaxCodeId
                  JOIN TRN.Invoice IW ON IW.Id=IT.InvoiceId
                LEFT JOIN HKP.Party P ON P.Id=IT.PartyId
				LEFT JOIN MST.TaxCategory TC ON TC.Id=ITD.TaxCategoryId AND TC.TaxCategoryType='TDS'
                LEFT JOIN ( SELECT DISTINCT TAC.Id,TAC.UserName,TAC.IsRCM,TAY.[Type],TACD.ValueOfFixed from MST.TaxCode TAC 
	            LEFT JOIN MST.TaxCodeYear TAY ON TAY.TaxCodeId=TAC.Id
	            LEFT JOIN MST.TaxCodeDetail TACD ON TACD.TaxCodeId=TAC.Id WHERE TAY.TaxYearId IN (" + taxyearId + @") AND TACD.TaxCodeYearId=TAY.Id) TAXC ON TAXC.Id=ITD.TaxCodeId
                LEFT JOIN TRN.InventoryReceive IR ON IR.VoucherId=V.Id
                LEFT JOIN TRN.InventoryReceiveTax IRT ON IRT.InventoryReceiveId=IR.Id
                LEFT JOIN MST.HSNTaxPercentage HSNP ON  IRT.HSNCodeId=HSNP.HSNCodeId AND HSNP.TaxCategoryId=ITD.TaxCategoryId 
                LEFT JOIN (SELECT SUM(D.DrAmount) DrAmount,D.VoucherId,D.ActivityId,D.InvoiceWriteOffDetailId 
				FROM  TRN.VoucherDetail D LEFT JOIN TRN.Voucher VV ON VV.Id=D.VoucherId 
				WHERE D.InvoiceTaxDetailId IS NULL AND D.DrAmount<> 0 
				GROUP BY D.VoucherId,D.ActivityId,D.InvoiceWriteOffDetailId
				) VD ON VD.VoucherId=IT.VoucherId 
                LEFT JOIN HKP.Activity A ON A.Id=VD.ActivityId 
                LEFT JOIN (SELECT I.Id InvoiceId,IWD.ActivityId,I.InventoryReceiveId
				, I.Amount Amount,SUM(VD.DrAmount) TaxableAmount
				,V.VoucherNo,V.PostingDate,V.DocRefNo,V.DocDate
				FROM  TRN.InvoiceWriteOffDetail IWD 
                JOIN TRN.InvoiceWriteOff IW ON IW.Id=IWD.InvoiceWriteOffId
				JOIN TRN.Invoice I ON I.Id=IWD.InvoiceId
						JOIN TRN.Voucher V ON V.Id=I.VoucherId
						JOIN TRN.VoucherDetail VD ON VD.VoucherId=I.VoucherId AND VD.DrAmount>0 AND VD.InvoiceTaxDetailId IS NULL
                WHERE IW.PaymentSource='Tax'
		                GROUP BY I.Id,V.VoucherNo,I.Amount,V.PostingDate,V.DocRefNo,V.DocDate,I.InventoryReceiveId,IWD.ActivityId
						) IWD ON IWD.InvoiceId=IT.InvoiceId
				LEFT JOIN HKP.Activity AP ON AP.Id=IWD.ActivityId
				LEFT JOIN (select InventoryReceiveId,sum(TotalMaterialTranAmount) TotalMaterialTranAmount from TRN.InventoryReceiveDetail group by InventoryReceiveId)IRD ON IWD.InventoryReceiveId=IRD.InventoryReceiveId
                LEFT JOIN TRN.Invoice SIV ON SIV.VoucherId=V.Id
				LEFT JOIN (select sad.ServiceAcknowledgementMasterId,sum(sad.Amount) TotalMaterialTranAmount 
						FROM  TRN.ServiceAcknowledgementMaster sam 
				JOIN TRN.ServiceAcknowledgementDetail sad ON sad.ServiceAcknowledgementMasterId=sam.Id
				GROUP BY sad.ServiceAcknowledgementMasterId)SAM ON IT.ServiceAcknowledgementMasterId=SAM.ServiceAcknowledgementMasterId
                WHERE TC.TaxCategoryType='TDS' AND ITD.AType='Cr' 
				AND V.PostingDate between  '" + fromDate + "' AND '" + toDate + "' and V.PlantId = '" + plantId + @"' and V.IsPark=0
                
                ) X
                ORDER BY X.LineItemType,X.ValueOfFixed,X.Percentage
				";

            return _sqlRepository.GetDataTable(strSql);

        }

        #endregion

        #region TDS Deduction
        public IWorkbook GetTCSDeductionReport(string companyGroupId, string companyId, string plantId, string plantName, string fromDate, string toDate, string name)
        {
            clsReport objRpt = null;
            clsReport objRptSR = null;
            try
            {

                ExcelEngine excelEngine = null;
                IApplication application = null;
                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;
                excelEngine.Excel.DefaultVersion = ExcelVersion.Excel2013;
                var reportUtility = new ReportUtility();
                var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
                workbook.Version = ExcelVersion.Excel2013;
                var sheet1 = workbook.Worksheets[0];

                #region Logo
                string strPath = "";
                Image companyLogo = null;
                try
                {
                    DataTable dtCompanyImage = _sqlRepository.GetDataTable("SELECT * FROM ORG.COMPANY WHERE ID = '" + companyId + @"'");

                    strPath = Path.Combine(ResourcesPathReader.GetLogoOrImagePath(), dtCompanyImage.Rows[0]["Image"].ToString());  // IDCardEng.xlsx
                    companyLogo = Image.FromFile(strPath);
                }
                catch (Exception)
                {
                }
                #endregion
                objRpt = new clsReport();

                objRptSR = new clsReport(_sqlRepository);

                DataTable dtRCMPayable = null;
                string taxyearId = GetTaxYearId(fromDate, toDate, companyId);


                dtRCMPayable = GetTCSDedutionData(companyGroupId, companyId, plantId, plantName, fromDate, toDate, taxyearId);
                if (dtRCMPayable.Rows.Count == 0)
                {
                    throw new Exception("No Data Found....");
                }

                DataTable dtCmp = objRptSR.SelectedCompanyDT(plantId);

                DataTable dtFactory = objRptSR.SelectedPlantDT(plantId);

                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;

                int xlsRow = 1, xlsCol = 1;
                int endXlsCol = 1;
                string FactoryName = "";
                string CmpName = "";
                xlsRow = 6;
                sheet1.Range[xlsRow - 1, 1].Text = "Report Ref No:";
                sheet1.Range[xlsRow - 1, 1].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow - 1, 1].RowHeight = 20;
                sheet1.Range[xlsRow - 1, 1].CellStyle.Font.Bold = true;

                //sheet1.Range[xlsRow - 1, 5].Text = "Payable";
                //sheet1.Range[xlsRow - 1, 5].CellStyle.Font.Size = 10;
                //// sheet1.Range[xlsRow - 1, 4,xlsRow-1,7].RowHeight = 30;
                //sheet1.Range[xlsRow - 1, 5].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                //sheet1.Range[xlsRow - 1, 4, xlsRow - 1, 7].BorderAround(ExcelLineStyle.Thin);

                //sheet1.Range[xlsRow - 1, 5].CellStyle.Font.Bold = true;

                //sheet1.Range[xlsRow - 1, 9].Text = "Tax";
                //sheet1.Range[xlsRow - 1, 9].CellStyle.Font.Size = 10;
                //sheet1.Range[xlsRow - 1, 9].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                ////sheet1.Range[xlsRow - 1, 7].BorderInside(ExcelLineStyle.Thin);
                //sheet1.Range[xlsRow - 1, 4, xlsRow - 1, 11].BorderAround(ExcelLineStyle.Thin);
                //sheet1.Range[xlsRow - 1, 9].CellStyle.Font.Bold = true;

                //int iPostingDate = xlsCol;
                //sheet1.Range[xlsRow, xlsCol].Text = "Date";
                //sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;

                int colSLNO = xlsCol;
                sheet1[xlsRow, xlsCol].Text = "SL. No";
                sheet1[xlsRow, xlsCol].ColumnWidth = 21;
                sheet1[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignRight;
                xlsCol++;

                //COL++;


                //COL++;

                int PartyName = xlsCol; // Party
                sheet1.Range[xlsRow, xlsCol].Text = "Suppliers Name";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 32;
                xlsCol++;

                sheet1[xlsRow, xlsCol].Text = "PAN No";
                int colPenNO = xlsCol;
                sheet1[xlsRow, xlsCol].ColumnWidth = 12;
                sheet1[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignRight;
                xlsCol++;


                int GSTIN = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "GSTIN";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                // xlsCol++;

                xlsCol++;
                int iInvoiceVoucherNo = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Invoice Voucher No";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;

                xlsCol++;
                int iInvoicePostingDate = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Posting Date";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 12;
                xlsCol++;

                //int iInvoiceDocDate = xlsCol;
                //sheet1.Range[xlsRow, xlsCol].Text = "Doc Date";
                //sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                //xlsCol++;

                //int iInvoiceDocRefNo = xlsCol;
                //sheet1.Range[xlsRow, xlsCol].Text = "DocRef No";
                //sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                ////VoucherNo
                //xlsCol++;

                //int iVoucherNo = xlsCol;
                //sheet1.Range[xlsRow, xlsCol].Text = "TDS Voucher No";
                //sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                //xlsCol++;

                //int iPostingDate = xlsCol;
                //sheet1.Range[xlsRow, xlsCol].Text = "Posting Date";
                //sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                //xlsCol++;

                int iDocDate = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Doc Date";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 12;
                xlsCol++;

                int iDocRefNo = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "DocRef No";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                xlsCol++;

                int iTCSType = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "TCS Type";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                xlsCol++;

                int iTCSPer = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "TCS Name";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 18;
                xlsCol++;

                int iPercentage = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Percentage";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 10;
                xlsCol++;

                int iSection = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Section";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 10;
                xlsCol++;

                int iInvoiceAmount = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Invoice Amount";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                sheet1[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignRight;



                xlsCol++;
                int iTaxableAmount = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Taxable Amount";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                sheet1[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignRight;
                xlsCol++;

                int CrAmount = xlsCol; // Doc Ref
                sheet1.Range[xlsRow, xlsCol].Text = "TCS Amount";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                sheet1[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignRight;
                // xlsCol++;

                DataTable dtTaxCode = null;


                dtRCMPayable.DefaultView.Sort = "TCSequence";
                endXlsCol = xlsCol;

                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].WrapText = true;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 23;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;




                string voucherNo = "";
                string Percentage = "";
                int startRow = 0;
                int perStartRow = 0;
                string formula = "";
                string formula2 = "";
                string totalFormula = "";
                string totalFormula2 = "";

                string lineItemPercentageType = "";
                string ValueOfFixedNew = "";
                xlsRow++;
                startRow = xlsRow;
                perStartRow = xlsRow;
                bool isFirst = true;
                int sl = 0;

                //int SerialNumber = 0;
                for (int i = 0; i < dtRCMPayable.Rows.Count; i++)
                {

                    if (voucherNo != dtRCMPayable.Rows[i]["VoucherNo"].ToString())
                    {
                        sheet1[xlsRow, colSLNO].Number = (i + 1);
                        sl++;

                        if (dtRCMPayable.Rows[i]["LineItemType"].ToString().ToUpper() == "GL")
                        {
                            lineItemPercentageType = "ValueOfFixed";
                        }
                        if (dtRCMPayable.Rows[i]["LineItemType"].ToString().ToUpper() == "MATERIAL")
                        {
                            lineItemPercentageType = "Percentage";
                        }
                        if (Percentage != dtRCMPayable.Rows[i][lineItemPercentageType].ToString())
                        {
                            if (isFirst == false)
                            {

                                //sheet1[perStartRow, iPostingDate, xlsRow - 1, iPostingDate].BorderAround(ExcelLineStyle.Hair);
                                sheet1[perStartRow, PartyName, xlsRow - 1, PartyName].BorderAround(ExcelLineStyle.Hair);
                                sheet1[perStartRow, colPenNO, xlsRow - 1, colPenNO].BorderAround(ExcelLineStyle.Hair);

                                sheet1[perStartRow, GSTIN, xlsRow - 1, GSTIN].BorderAround(ExcelLineStyle.Hair);
                                sheet1[perStartRow, iDocRefNo, xlsRow - 1, iDocRefNo].BorderAround(ExcelLineStyle.Hair);
                                //sheet1[perStartRow, iVoucherNo, xlsRow - 1, iVoucherNo].BorderAround(ExcelLineStyle.Hair);
                                //sheet1[perStartRow, iPostingDate, xlsRow - 1, iPostingDate].BorderAround(ExcelLineStyle.Hair);
                                sheet1[perStartRow, iDocDate, xlsRow - 1, iDocDate].BorderAround(ExcelLineStyle.Hair);

                                sheet1[perStartRow, CrAmount, xlsRow - 1, CrAmount].BorderAround(ExcelLineStyle.Hair);
                                sheet1[perStartRow, iTaxableAmount, xlsRow - 1, iTaxableAmount].BorderAround(ExcelLineStyle.Hair);
                                formula = "SUM(" + clsStaticInfo.GetxlsCol(iTaxableAmount) + perStartRow + ":" + clsStaticInfo.GetxlsCol(iTaxableAmount) + (xlsRow - 1) + ")";

                                formula2 = "SUM(" + clsStaticInfo.GetxlsCol(CrAmount) + perStartRow + ":" + clsStaticInfo.GetxlsCol(CrAmount) + (xlsRow - 1) + ")";
                                //if (dtTaxCode.Rows.Count > 0)
                                //{
                                //    for (int j = 0; j < dtTaxCode.Rows.Count; j++)
                                //    {
                                //       // sheet1[perStartRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"]), xlsRow - 1, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])].BorderAround(ExcelLineStyle.Hair);
                                //        //formula2 = "SUM(" + clsStaticInfo.GetxlsCol(Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])) + perStartRow + ":" + clsStaticInfo.GetxlsCol(Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])) + (xlsRow - 1) + ")";
                                //        //sheet1[xlsRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"]), xlsRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])].Formula = formula2;

                                //       // dtTaxCode.Rows[j]["ColumnFormula"] += (clsStaticInfo.GetxlsCol(Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])) + xlsRow).ToString() + " + ";
                                //    }
                                //}
                                sheet1.Range[xlsRow, 1, xlsRow, 1].Text = "Total";

                                sheet1[xlsRow, iTaxableAmount, xlsRow, iTaxableAmount].Formula = formula;
                                sheet1[xlsRow, CrAmount, xlsRow, CrAmount].Formula = formula2;
                                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Font.Bold = true;
                                totalFormula += (clsStaticInfo.GetxlsCol(iTaxableAmount) + xlsRow).ToString() + "+";
                                totalFormula2 += (clsStaticInfo.GetxlsCol(CrAmount) + xlsRow).ToString() + "+";


                                //SerialNumber++;
                                //sheet1[xlsRow, colSLNO].Text = (SerialNumber).ToString();
                                xlsRow++;


                            }


                            xlsRow++;
                            sheet1.Range[xlsRow - 1, 1].Text = dtRCMPayable.Rows[i]["ValueOfFixedNew"].ToString();
                            sheet1.Range[xlsRow - 1, 1].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                            perStartRow = xlsRow;
                            isFirst = false;

                        }

                        //sheet1.Range[xlsRow, iPostingDate].Text = dtRCMPayable.Rows[i]["PostingDate"].ToString();
                        sheet1[xlsRow, colSLNO].Number = sl;
                        sheet1.Range[xlsRow, PartyName].Text = dtRCMPayable.Rows[i]["PartyName"].ToString();

                        sheet1.Range[xlsRow, colPenNO].Text = dtRCMPayable.Rows[i]["PanNo"].ToString();
                        sheet1.Range[xlsRow, iInvoiceVoucherNo].Text = dtRCMPayable.Rows[i]["InvoiceVoucherNo"].ToString();
                        sheet1.Range[xlsRow, iInvoicePostingDate].DateTime = Convert.ToDateTime(dtRCMPayable.Rows[i]["InvoicePostingDate"].ToString());
                        //sheet1.Range[xlsRow, iInvoicePostingDate].Text = clsStaticInfo.GetDateTaxFormate(dtRCMPayable.Rows[i]["InvoicePostingDate"].ToString());
                        //sheet1.Range[xlsRow, iInvoiceDocRefNo].Text = dtRCMPayable.Rows[i]["InvoieDocRefNo"].ToString();
                        //sheet1.Range[xlsRow, iInvoiceDocDate].Text = dtRCMPayable.Rows[i]["InvoiceDocDate"].ToString();
                        sheet1.Range[xlsRow, GSTIN].Text = dtRCMPayable.Rows[i]["GSTIN"].ToString();

                        sheet1.Range[xlsRow, iTCSType].Text = dtRCMPayable.Rows[i]["SourceType"].ToString();//TaxableAmount
                        sheet1.Range[xlsRow, iTCSPer].Text = dtRCMPayable.Rows[i]["TCSPer"].ToString();//TaxableAmount
                                                                                                       //sheet1.Range[xlsRow, iTDSPer].NumberFormat = reportUtility.NumberFormatDecimalTwo();

                        sheet1.Range[xlsRow, iPercentage].Number = clsStaticInfo.dbl(dtRCMPayable.Rows[i]["ValueOfFixed"].ToString());//TaxableAmount
                        sheet1.Range[xlsRow, iPercentage].NumberFormat = reportUtility.NumberFormatDecimalTwo();

                        sheet1.Range[xlsRow, iSection].Text = dtRCMPayable.Rows[i]["Section"].ToString();//TaxableAmount

                        sheet1.Range[xlsRow, iInvoiceAmount].Number = clsStaticInfo.dbl(dtRCMPayable.Rows[i]["InvoiceAmount"].ToString());//TaxableAmount
                        sheet1.Range[xlsRow, iInvoiceAmount].NumberFormat = reportUtility.NumberFormatDecimalTwo();

                        sheet1.Range[xlsRow, iDocRefNo].Text = dtRCMPayable.Rows[i]["DocRefNo"].ToString();
                        //sheet1.Range[xlsRow, iVoucherNo].Text = dtRCMPayable.Rows[i]["VoucherNo"].ToString();
                        //sheet1.Range[xlsRow, iPostingDate].Text = dtRCMPayable.Rows[i]["PostingDate"].ToString();
                        sheet1.Range[xlsRow, iDocDate].Text = dtRCMPayable.Rows[i]["DocDate"].ToString();

                        //sheet1.Range[xlsRow, CrAmount].Text = dtRCMPayable.Rows[i]["CrAmount"].ToString();//TaxableAmount
                        sheet1.Range[xlsRow, CrAmount].Number = clsStaticInfo.dbl(dtRCMPayable.Rows[i]["CrAmount"].ToString());//TaxableAmount
                        sheet1.Range[xlsRow, CrAmount].NumberFormat = reportUtility.NumberFormatDecimalTwo();

                        sheet1.Range[xlsRow, iTaxableAmount].Number = clsStaticInfo.dbl(dtRCMPayable.Rows[i]["TaxableAmount"].ToString());//TaxableAmount
                        sheet1.Range[xlsRow, iTaxableAmount].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                        dtRCMPayable.DefaultView.RowFilter = "VoucherNo = '" + dtRCMPayable.Rows[i]["VoucherNo"].ToString() + "'";

                        //if (dtTaxCode.Rows.Count > 0)
                        //{
                        //    for (int j = 0; j < dtTaxCode.Rows.Count; j++)
                        //    {
                        //        //dtRCMPayable.DefaultView.RowFilter = "TaxCode = '" + dtTaxCode.Rows[j]["TaxCode"].ToString() + "' and VoucherNo = '" + dtRCMPayable.Rows[i]["VoucherNo"].ToString() + "'";
                        //        if (dtRCMPayable.DefaultView.Count > 0)
                        //        {

                        //            sheet1.Range[xlsRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])].Number = clsStaticInfo.dbl(dtRCMPayable.DefaultView[0]["CrAmount"].ToString());
                        //            sheet1.Range[xlsRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                        //        }
                        //        else
                        //        {
                        //            sheet1.Range[xlsRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])].Text = "-";
                        //            sheet1.Range[xlsRow, Convert.ToInt32(dtTaxCode.Rows[j]["ColumnNumber"])].HorizontalAlignment = ExcelHAlign.HAlignRight;


                        //        }
                        //    }
                        //}


                        Percentage = dtRCMPayable.Rows[i][lineItemPercentageType].ToString();
                        xlsRow++;
                    }

                    voucherNo = dtRCMPayable.Rows[i]["VoucherNo"].ToString();


                }

                //sheet1[perStartRow, iPostingDate, xlsRow - 1, iPostingDate].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, PartyName, xlsRow - 1, PartyName].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, GSTIN, xlsRow - 1, GSTIN].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iDocRefNo, xlsRow - 1, iDocRefNo].BorderAround(ExcelLineStyle.Hair);
                // sheet1[perStartRow, iVoucherNo, xlsRow - 1, iVoucherNo].BorderAround(ExcelLineStyle.Hair);

                sheet1[perStartRow, CrAmount, xlsRow - 1, CrAmount].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iTaxableAmount, xlsRow - 1, iTaxableAmount].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iTaxableAmount, xlsRow - 1, iTaxableAmount].BorderAround(ExcelLineStyle.Hair);



                sheet1.Range[xlsRow, 1, xlsRow, 1].Text = "Total";
                formula = "SUM(" + clsStaticInfo.GetxlsCol(iTaxableAmount) + perStartRow + ":" + clsStaticInfo.GetxlsCol(iTaxableAmount) + (xlsRow - 1) + ")";
                formula2 = "SUM(" + clsStaticInfo.GetxlsCol(CrAmount) + perStartRow + ":" + clsStaticInfo.GetxlsCol(CrAmount) + (xlsRow - 1) + ")";

                sheet1[xlsRow, iTaxableAmount, xlsRow, iTaxableAmount].Formula = formula;
                sheet1[xlsRow, iTaxableAmount, xlsRow, iTaxableAmount].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);
                sheet1[xlsRow, CrAmount, xlsRow, CrAmount].Formula = formula2;
                sheet1[xlsRow, CrAmount, xlsRow, CrAmount].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Font.Bold = true;
                totalFormula += (clsStaticInfo.GetxlsCol(iTaxableAmount) + xlsRow).ToString() + "+";
                totalFormula2 += (clsStaticInfo.GetxlsCol(CrAmount) + xlsRow).ToString() + "+";





                xlsRow++;
                xlsRow++;

                sheet1.Range[xlsRow, 1, xlsRow, 1].Text = "Grand Total";

                sheet1[xlsRow, iTaxableAmount, xlsRow, iTaxableAmount].Formula = totalFormula.Remove(totalFormula.Length - 1);
                sheet1[xlsRow, iTaxableAmount, xlsRow, iTaxableAmount].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);
                sheet1[xlsRow, CrAmount, xlsRow, CrAmount].Formula = totalFormula2.Remove(totalFormula2.Length - 1);
                sheet1[xlsRow, CrAmount, xlsRow, CrAmount].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Font.Bold = true;




                #region ******************Report Header******************



                xlsRow = 1;
                xlsCol = 3;
                try
                {
                    if (companyLogo != null)
                    {

                        double totalWidth = sheet1.GetColumnWidth(1) + sheet1.GetColumnWidth(GSTIN);
                        int totalWidthPixel = (int)(totalWidth * 7.5);
                        int totalheight = (int)((sheet1.GetRowHeight(1) + sheet1.GetRowHeight(2) + sheet1.GetRowHeight(3) + sheet1.GetRowHeight(3)) * 1.50);

                        companyLogo = ReportUtility.FixedSize(companyLogo, totalWidthPixel, totalheight);
                        IPictureShape pic = null;

                        pic = sheet1.Pictures.AddPicture(1, 1, companyLogo);
                        //pic.Height = 80;
                        //pic.Width = 220;
                    }
                }
                catch (Exception ex)
                {
                }

                FactoryName = string.Empty;

                string FactoryAddress = string.Empty;

                if (dtCmp.Rows.Count > 0)
                {
                    CmpName = dtCmp.Rows[0]["CompanyName"].ToString();
                }
                else
                {
                    CmpName = "";
                }
                sheet1.Range[xlsRow, 3].Text = CmpName;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 12;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 17;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                if (dtFactory.Rows.Count > 0)
                {
                    //FactoryName = dsFactory.Tables[0].Rows[0]["PlantName"].ToString();
                    FactoryName = dtFactory.Rows[0]["UserName"].ToString();
                }
                else
                {
                    FactoryName = "";
                }
                sheet1.Range[xlsRow, 3].Text = FactoryName;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 14;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 18;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                if (dtFactory.Rows.Count > 0)
                {
                    FactoryAddress = dtFactory.Rows[0]["Address1"].ToString();
                }
                else
                {
                    FactoryAddress = "";
                }
                sheet1.Range[xlsRow, 3].Text = FactoryAddress;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                //sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 22;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                sheet1.Range[xlsRow, 3].Text = "TCS Deduction Report From " + fromDate + " To " + toDate;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;


                #endregion ******************Report Header******************

                #region Freeze Panes

                sheet1.IsDisplayZeros = false;
                sheet1.UsedRange["A7"].FreezePanes();
                sheet1.FirstVisibleColumn = 1;
                sheet1.FirstVisibleRow = 6;

                #endregion Freeze Panes

                #region UsedRange Alignment

                sheet1.UsedRange.WrapText = false;
                sheet1.UsedRange.CellStyle.Font.Size = 10;
                sheet1.Range["A1"].CellStyle.Font.Size = 14;
                sheet1.Range["A2"].CellStyle.Font.Size = 10;
                sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;
                sheet1.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                #endregion UsedRange Alignment

                #region Page Setup
                sheet1.PageSetup.TopMargin = 0.5;
                sheet1.PageSetup.BottomMargin = 0.7;
                sheet1.PageSetup.PrintTitleRows = "$1:$5";
                sheet1.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                sheet1.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + name + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:MM tt").ToString();
                sheet1.PageSetup.LeftMargin = 0.5;
                sheet1.PageSetup.RightMargin = 0.2;
                sheet1.PageSetup.Orientation = ExcelPageOrientation.Portrait;
                sheet1.PageSetup.FitToPagesTall = 0;
                sheet1.PageSetup.FitToPagesWide = 1;
                sheet1.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                sheet1.IsDisplayZeros = false;
                #endregion Page Setup


                sheet1.Name = "TCS Deduction";
                return workbook;
            }
            catch (System.Exception ex)
            {

                throw ex;
            }
        }

        private DataTable GetTCSDedutionData(string companyGroupId, string companyId, string plantId, string plantName, string fromDate, string toDate, string taxyearId)
        {
            string strSql = "";
            strSql = @"
                 select SourceType= V.SourceType
                ,V.VoucherNo InvoiceVoucherNo,IV.InventoryReceiveId
				,V.PostingDate InvoicePostingDate,V.DocRefNo InvoieDocRefNo,format( V.DocDate, 'dd-MMM-yyyy') InvoiceDocDate
				,V.VoucherNo,Format(V.PostingDate,'dd-MMM-yyyy') PostingDate,V.DocRefNo,format( V.DocDate, 'dd-MMM-yyyy')DocDate, P.UserName PartyName,P.TINNO GSTIN 
                ,LineItemType=case when v.SourceType='InventoryPayable' then 'Material' 
				                   when v.SourceType='SalesInvoice' then 'GL'
				                   when v.SourceType='VendorPayment' then 'GL'
				                   when v.SourceType='CreditNoteSetOff' then 'GL'
				                   else '' end
				                   ,Particular=TAXC.UserName
				  ,TaxableAmount=iv.Amount-it.taxAmount
                ,InvoiceAmount=iv.Amount
                ,IT.Id,0 DrAmount, CrAmount=case when ITD.AType='Dr' then IT.TaxAmount else 0 end

                ,TC.Code TaxCode ,TC.Sequence TCSequence,TC.TaxCategoryType,TC.UserName+'-'+TC.Code TaxCategory,IsNULL(TAXC.IsRCM,0) IsRCM,TAXC.UserName TaxCodeName
                ,IsNULL(IV.IsExcludingTax,0) IsExcludingTax,IsNULL(IR.IsTaxApplicable,0) IsTaxApplicable,TAXC.[Type],ValueOfFixedNew = TAXC.UserName +' - '+ convert(varchar,TAXC.ValueOfFixed),TAXC.ValueOfFixed
                ,0 Percentage
                ,P.VATResistrationNo PanNo,TAXC.UserName TCSPer,TAXC.Section

                from TRN.InvoiceTax IT 
                left join TRN.InvoiceTaxDetail ITD  ON IT.Id=ITD.InvoiceTaxId AND ITD.AType='Dr'
                LEFT JOIN TRN.Voucher V ON V.Id=IT.VoucherId
				LEFT JOIN MST.TaxCode TXC ON TXC.Id=IT.TaxCodeId
                LEFT JOIN TRN.Invoice IV ON IV.Id=IT.InvoiceId
                
                --LEFT JOIN TRN.InvoiceWriteOff IW ON IW.Id=IT.InvoiceWriteOffId
                LEFT JOIN HKP.Party P ON P.Id=IT.PartyId
				 JOIN MST.TaxCategory TC ON TC.Id=IT.TaxCategoryId AND TC.TaxCategoryType='TCS'
				 LEFT JOIN TRN.InventoryReceive IR ON IR.Id=IV.InventoryReceiveId
                LEFT JOIN [TRN].[InventoryReceiveAdditionalTax] ATX ON IR.Id=ATX.InventoryReceiveId
               
                LEFT JOIN( select distinct TAC.Id,TAC.UserName,TAC.IsRCM,TAY.[Type],TACD.ValueOfFixed,TAC.Code Section  from MST.TaxCode TAC 
	                LEFT JOIN MST.TaxCodeYear TAY ON TAY.TaxCodeId=TAC.Id
	               LEFT JOIN MST.TaxCodeDetail TACD ON TACD.TaxCodeId=TAC.Id WHERE TAY.TaxYearId IN (" + taxyearId + @") AND TACD.TaxCodeYearId=TAY.Id) TAXC ON TAXC.Id=ATX.TaxCodeId
				
				where TC.TaxCategoryType='TCS' AND ITD.AType='Dr' 
				AND V.PostingDate between '" + fromDate + "' AND '" + toDate + "' and V.PlantId = '" + plantId + @"' and V.IsPark=0
             

				UNION ALL
				select SourceType= V.SourceType
                ,V.VoucherNo InvoiceVoucherNo,IV.InventoryReceiveId
				,V.PostingDate InvoicePostingDate,V.DocRefNo InvoieDocRefNo,format( V.DocDate, 'dd-MMM-yyyy') InvoiceDocDate
				,V.VoucherNo,Format(V.PostingDate,'dd-MMM-yyyy') PostingDate,V.DocRefNo,format( V.DocDate, 'dd-MMM-yyyy')DocDate, P.UserName PartyName,P.TINNO GSTIN 
                ,LineItemType=case when v.SourceType='InventoryPayable' then 'Material' 
				                   when v.SourceType='SalesInvoice' then 'GL'
				                   when v.SourceType='VendorPayment' then 'GL'
				                   when v.SourceType='CreditNoteSetOff' then 'GL'
				                   else '' end
				                   ,Particular=case when v.SourceType='SalesInvoice' then TXC.UserName 
									                WHEN v.SourceType='VendorInvoice' THEN A.UserName
									                WHEN v.SourceType='VendorPayment' THEN AP.UserName
									                WHEN v.SourceType='CreditNoteSetOff' THEN AP.UserName
				                   else '' end
				  ,TaxableAmount=iv.Amount-it.taxAmount
                ,InvoiceAmount=iv.Amount
                ,IT.Id,0 DrAmount ,CrAmount=case when ITD.AType='Cr' then IT.TaxAmount else 0 end

                ,TC.Code TaxCode ,TC.Sequence TCSequence,TC.TaxCategoryType,TC.UserName+'-'+TC.Code TaxCategory,IsNULL(TAXC.IsRCM,0) IsRCM,TAXC.UserName TaxCodeName
                ,IsNULL(IV.IsExcludingTax,0) IsExcludingTax,IsNULL(IR.IsTaxApplicable,0) IsTaxApplicable,TAXC.[Type],ValueOfFixedNew = TAXC.UserName +' - '+ convert(varchar,TAXC.ValueOfFixed),TAXC.ValueOfFixed
                ,IsNULL(HSNP.[Percentage],0) Percentage--,MM.HSNCodeId,MM.UserName Material
                ,P.VATResistrationNo PanNo,TXC.UserName TDSPer,TXC.Code Section

                FROM TRN.InvoiceTax IT 
                left join TRN.InvoiceTaxDetail ITD  ON IT.Id=ITD.InvoiceTaxId AND ITD.AType='Cr'
                LEFT JOIN TRN.Voucher V ON V.Id=IT.VoucherId
				LEFT JOIN MST.TaxCode TXC ON TXC.Id=IT.TaxCodeId
                LEFT JOIN TRN.Invoice IV ON IV.Id=IT.InvoiceId
                LEFT JOIN TRN.InvoiceWriteOff IW ON IW.Id=IT.InvoiceWriteOffId
                LEFT JOIN HKP.Party P ON P.Id=IT.PartyId
				LEFT JOIN MST.TaxCategory TC ON TC.Id=IT.TaxCategoryId AND TC.TaxCategoryType='TCS'
                LEFT JOIN( select distinct TAC.Id,TAC.UserName,TAC.IsRCM,TAY.[Type],TACD.ValueOfFixed from MST.TaxCode TAC 
	                LEFT JOIN MST.TaxCodeYear TAY ON TAY.TaxCodeId=TAC.Id
	               LEFT JOIN MST.TaxCodeDetail TACD ON TACD.TaxCodeId=TAC.Id WHERE TAY.TaxYearId IN (" + taxyearId + @") AND TACD.TaxCodeYearId=TAY.Id) TAXC ON TAXC.Id=IT.TaxCodeId
                LEFT JOIN TRN.InventoryReceive IR ON IR.VoucherId=V.Id
                LEFT JOIN TRN.InventoryReceiveTax IRT ON IRT.InventoryReceiveId=IR.Id --AND IRT.TaxCategoryId=IT.TaxCategoryId
                LEFT JOIN MST.HSNTaxPercentage HSNP ON  IRT.HSNCodeId=HSNP.HSNCodeId AND HSNP.TaxCategoryId=IT.TaxCategoryId 
                LEFT JOIN (SELECT SUM(D.DrAmount) DrAmount,D.VoucherId,D.ActivityId,D.InvoiceWriteOffDetailId 
				FROM  TRN.VoucherDetail D LEFT JOIN TRN.Voucher VV ON VV.Id=D.VoucherId 
				WHERE D.InvoiceTaxDetailId IS NULL AND D.DrAmount<> 0 
				GROUP BY D.VoucherId,D.ActivityId,D.InvoiceWriteOffDetailId
				) VD ON VD.VoucherId=IT.VoucherId 
                LEFT JOIN HKP.Activity A ON A.Id=VD.ActivityId 
                LEFT JOIN (SELECT IW.InvoiceWriteOffId,I.InventoryReceiveId,IW.ActivityId
				,SUM(I.Amount) Amount,SUM(VD.DrAmount) TaxableAmount
				,V.VoucherNo,V.PostingDate,V.DocRefNo,V.DocDate
				FROM TRN.InvoiceWriteOffDetail IW 
			                JOIN TRN.Invoice I ON I.Id=IW.InvoiceId
							JOIN TRN.Voucher V ON V.Id=I.VoucherId
							JOIN TRN.VoucherDetail VD ON VD.VoucherId=I.VoucherId AND VD.DrAmount>0 AND VD.InvoiceTaxDetailId IS NULL
		                GROUP BY InvoiceWriteOffId,IW.ActivityId,V.VoucherNo,V.PostingDate,V.DocRefNo,V.DocDate,I.InventoryReceiveId
						) IWD ON IWD.InvoiceWriteOffId=IT.InvoiceWriteOffId
                LEFT JOIN HKP.Activity AP ON AP.Id=IWD.ActivityId
				LEFT JOIN (select InventoryReceiveId,sum(TotalMaterialTranAmount) TotalMaterialTranAmount from TRN.InventoryReceiveDetail group by InventoryReceiveId)IRD ON IWD.InventoryReceiveId=IRD.InventoryReceiveId
                LEFT JOIN TRN.Invoice SIV ON SIV.VoucherId=V.Id
				LEFT JOIN (select sad.ServiceAcknowledgementMasterId,IWD.InvoiceWriteOffId,sum(sad.Amount) TotalMaterialTranAmount from TRN.ServiceAcknowledgementDetail sad
				join TRN.ServiceAcknowledgementMaster sam on sam.Id=sad.ServiceAcknowledgementMasterId
				join trn.Invoice I on I.ServiceAcknowledgementMasterId =sam.Id
				join trn.InvoiceWriteOffDetail IWD ON IWD.InvoiceId=I.Id
				group by sad.ServiceAcknowledgementMasterId,IWD.InvoiceWriteOffId)SAM ON IT.InvoiceWriteOffId=SAM.InvoiceWriteOffId
                WHERE TC.TaxCategoryType='TCS' AND ITD.AType='Cr' 
				AND V.PostingDate between '" + fromDate + "' AND '" + toDate + "' and V.PlantId = '" + plantId + @"' and V.IsPark=0
                ORDER BY LineItemType,ValueOfFixed,Percentage
				";

            return _sqlRepository.GetDataTable(strSql);

        }

        public string GSTDetailReport(string FromDate, string ToDate, string SheetName)
        {
            ExcelEngine excelEngine = null;
            IApplication application = null;
            IWorkbook workbook = null;
            IWorksheet sheet = null;
            var filePath = "";
            try
            {
                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;
                workbook = application.Workbooks.Create(1);
                workbook.Worksheets[0].Name = "GSTDetailReport";
                sheet = workbook.Worksheets[0];
                DataTable data;
                GSTDetailReportSQL(FromDate, ToDate, out data);

                int ROW = 5; int COL = 1;
                
                #region columns
                sheet[ROW, COL].Text = "Voucher Type";
                sheet[ROW, COL].ColumnWidth = 10;
                int ColVoucherType = COL;
                COL++;

                sheet[ROW, COL].Text = "Party Id";
                sheet[ROW, COL].ColumnWidth = 10;
                int ColPartyId = COL;
                COL++;

                sheet[ROW, COL].Text = "Party Name";
                sheet[ROW, COL].ColumnWidth = 30;
                int ColPartyName = COL;
                COL++;

                sheet[ROW, COL].Text = "GSTIN(Party Plant)";
                sheet[ROW, COL].ColumnWidth = 15;
                int ColGSTIN = COL;
                COL++;

                sheet[ROW, COL].Text = "Party Type";
                sheet[ROW, COL].ColumnWidth = 10;
                int ColPartyType = COL;
                COL++;

                sheet[ROW, COL].Text = "Party Nature";
                sheet[ROW, COL].ColumnWidth = 12;
                int ColPartyNature = COL;
                COL++;

                sheet[ROW, COL].Text = "Party Category";
                sheet[ROW, COL].ColumnWidth = 15;
                int ColPartyCategory = COL;
                COL++;

                sheet[ROW, COL].Text = "Party Sub Category";
                sheet[ROW, COL].ColumnWidth = 15;
                int ColPartySubCategory = COL;
                COL++;

                sheet[ROW, COL].Text = "Material";
                sheet[ROW, COL].ColumnWidth = 20;
                int ColMaterial = COL;
                COL++;

                sheet[ROW, COL].Text = "Article";
                sheet[ROW, COL].ColumnWidth = 25;
                int ColArticle = COL;
                COL++;

                sheet[ROW, COL].Text = "Particulars";
                sheet[ROW, COL].ColumnWidth = 30;
                int ColParticulars = COL;
                COL++;

                sheet[ROW, COL].Text = "HSN Code";
                sheet[ROW, COL].ColumnWidth = 15;
                int ColHSN = COL;
                COL++;

                sheet[ROW, COL].Text = "Voucher No";
                sheet[ROW, COL].ColumnWidth = 12;
                int ColVoucherNo = COL;
                COL++;

                sheet[ROW, COL].Text = "Entry Date";
                sheet[ROW, COL].ColumnWidth = 12;
                int ColEntryDate = COL;
                COL++;

                sheet[ROW, COL].Text = "Posting Date";
                sheet[ROW, COL].ColumnWidth = 12;
                int ColPostingDate = COL;
                COL++;

                sheet[ROW, COL].Text = "DocRef No";
                sheet[ROW, COL].ColumnWidth = 15;
                int ColDocRefNo = COL;
                COL++;

                sheet[ROW, COL].Text = "Doc Date";
                sheet[ROW, COL].ColumnWidth = 12;
                int ColDocDate = COL;
                COL++;

                sheet[ROW, COL].Text = "GRN No";
                sheet[ROW, COL].ColumnWidth = 12;
                int ColGRNNo = COL;
                COL++;

                sheet[ROW, COL].Text = "Taxable Amount";
                sheet[ROW, COL].ColumnWidth = 12;
                int ColTaxableAmount = COL;
                COL++;

                sheet[ROW, COL].Text = "IGST";
                sheet[ROW, COL].ColumnWidth = 15;
                int ColIGST = COL;
                COL++;

                sheet[ROW, COL].Text = "CGST";
                sheet[ROW, COL].ColumnWidth = 15;
                int ColCGST = COL;
                COL++;

                sheet[ROW, COL].Text = "SGST";
                sheet[ROW, COL].ColumnWidth = 12;
                int ColSGST = COL;
                COL++;
                int ColTCS = 0;
                DataView dv = new DataView(data);
                dv.RowFilter = "TaxCategoryType='" + "TCS" + "'";
                if (dv.Count > 0)
                {
                    sheet[ROW, COL].Text = "TCS";
                    sheet[ROW, COL].ColumnWidth = 12;
                     ColTCS = COL;
                    COL++;
                }
                

                sheet[ROW, COL].Text = "Total Tax";
                sheet[ROW, COL].ColumnWidth = 15;
                int ColTotalTax = COL;
                COL++;

                sheet[ROW, COL].Text = "Gross Amount";
                sheet[ROW, COL].ColumnWidth = 15;
                int ColGrossAmount = COL;

                #endregion columns

                int endCol = COL;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Black;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Color = ExcelKnownColors.White;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Bold = true;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 9f;
                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);

                ROW++;
                int startRow = ROW;
                var TotalTax = 0.00;
                for (int i = 0; i < data.Rows.Count; i++)
                {
                    sheet[ROW, ColVoucherType].Text = data.Rows[i]["SourceType"].ToString();
                    sheet[ROW, ColPartyId].Text = data.Rows[i]["PartyId"].ToString();
                    sheet[ROW, ColPartyName].Text = data.Rows[i]["PartyName"].ToString();
                    sheet[ROW, ColGSTIN].Text = data.Rows[i]["GSTIN"].ToString();
                    sheet[ROW, ColPartyType].Text = data.Rows[i]["PartyType"].ToString();
                    sheet[ROW, ColPartyNature].Text = data.Rows[i]["PartyNature"].ToString();
                    sheet[ROW, ColPartyCategory].Text = data.Rows[i]["PartyCategory"].ToString();
                    sheet[ROW, ColPartySubCategory].Text = data.Rows[i]["PartySubCategory"].ToString();
                    sheet[ROW, ColMaterial].Text = data.Rows[i]["Material"].ToString();
                    sheet[ROW, ColArticle].Text = data.Rows[i]["Article"].ToString();
                    sheet[ROW, ColParticulars].Text = data.Rows[i]["Narration"].ToString();
                    sheet[ROW, ColHSN].Text = data.Rows[i]["HSNCode"].ToString();
                    sheet[ROW, ColVoucherNo].Text = data.Rows[i]["VoucherNo"].ToString();
                    sheet[ROW, ColEntryDate].Text = data.Rows[i]["EntryDate"].ToString();
                    sheet[ROW, ColPostingDate].Text = data.Rows[i]["PostingDate"].ToString();
                    sheet[ROW, ColDocRefNo].Text = data.Rows[i]["DocRefNo"].ToString();
                    sheet[ROW, ColDocDate].Text = data.Rows[i]["DocDate"].ToString();
                    sheet[ROW, ColGRNNo].Text = data.Rows[i]["GRNNo"].ToString();

                    sheet[ROW, ColTaxableAmount].Number = clsStaticInfo.dbl(data.Rows[i]["TaxableAmount"].ToString());
                    sheet[ROW, ColTaxableAmount].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);
                    sheet[ROW, ColIGST].Number = clsStaticInfo.dbl(data.Rows[i]["IGST"].ToString());
                    sheet[ROW, ColIGST].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);

                    sheet[ROW, ColCGST].Number = clsStaticInfo.dbl(data.Rows[i]["CGST"].ToString());
                    sheet[ROW, ColCGST].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);
                    sheet[ROW, ColSGST].Number = clsStaticInfo.dbl(data.Rows[i]["SGST"].ToString());
                    sheet[ROW, ColSGST].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);
                    if (dv.Count > 0)
                    {
                        sheet[ROW, ColTCS].Number = clsStaticInfo.dbl(data.Rows[i]["TCS"].ToString());
                        sheet[ROW, ColTCS].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);
                    }
                   
                    if (dv.Count > 0)
                    {
                         TotalTax = clsStaticInfo.dbl(data.Rows[i]["IGST"].ToString()) + clsStaticInfo.dbl(data.Rows[i]["CGST"].ToString()) + clsStaticInfo.dbl(data.Rows[i]["SGST"].ToString()) + clsStaticInfo.dbl(data.Rows[i]["TCS"].ToString());
                    }
                    else
                    {
                         TotalTax = clsStaticInfo.dbl(data.Rows[i]["IGST"].ToString()) + clsStaticInfo.dbl(data.Rows[i]["CGST"].ToString()) + clsStaticInfo.dbl(data.Rows[i]["SGST"].ToString());

                    }
                    var GrossAmn = TotalTax + clsStaticInfo.dbl(data.Rows[i]["TaxableAmount"].ToString());

                    sheet[ROW, ColTotalTax].Number = TotalTax;
                    sheet[ROW, ColTotalTax].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);
                    sheet[ROW, ColGrossAmount].Number = GrossAmn;
                    sheet[ROW, ColGrossAmount].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);

                    sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                    ROW++;
                }

                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[startRow, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                sheet["A" + startRow.ToString()].FreezePanes();

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ReportUtility reportUtility = new ReportUtility();
                reportUtility.PlantHeader(ref sheet, endCol, "GST Detail Report", identity.PlantId);
                reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.IsGridLinesVisible = false;

                //#endregion ******************Report Header******************

                sheet.PageSetup.TopMargin = 0.2;
                sheet.PageSetup.BottomMargin = 0.8;
                //sheet.PageSetup.PrintTitleRows = "$1:$6";
                sheet.PageSetup.LeftMargin = 0.2;
                sheet.PageSetup.RightMargin = 0.2;
                sheet.PageSetup.Orientation = ExcelPageOrientation.Landscape;
                sheet.PageSetup.FitToPagesTall = 0;
                sheet.PageSetup.FitToPagesWide = 1;
                sheet.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                sheet.PageSetup.CenterHorizontally = true;

                filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SheetName + ".xlsx");
                workbook.SaveAs(filePath);
                workbook.Close();
                excelEngine.Dispose();
                return filePath;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void GSTDetailReportSQL(string FromDate, string ToDate, out DataTable data)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                string strSQL = @"select  SourceType,
                        VoucherNo,VoucherDate,PostingDate,DocRefNo,DocDate,PartyId,PartyName,PartyPlantName,PartyCategory,PartySubCategory
                        ,PartyNature,PartyType,Material,Article,HSNCode,GSTIN , TaxCategoryType
                        , TaxCode , TaxableAmount, ISNULL(DrAmount,0) DrAmount, CrAmount ,EntryDate,GRNNo,Narration
                        into #tempOT from
                        (
                        SELECT	x.SourceType,x.VoucherNo,x.VoucherDate,x.PostingDate,x.DocRefNo,x.DocDate,x.PartyId,x.PartyName,x.PartyPlantName,X.PartyCategory,X.PartySubCategory
                        ,X.PartyNature,X.PartyType,X.Material,X.Article,X.HSNCode,x.GSTIN
		                        ,x.TaxCategoryType,x.TaxCode--,x.TaxPercentage
		                        ,SUM(x.TaxableAmount) TaxableAmount,SUM(x.DrAmount) DrAmount,SUM(x.CrAmount) CrAmount
		                        ,x.TCSequence,x.EntryDate,x.GRNNo,X.Narration
		                        FROM 

                        (
                        SELECT 
						'Expenses' SourceType
                            ,V.VoucherNo,format( V.PostingDate,'dd-MMM-yyyy')PostingDate, V.DocRefNo,format (V.DocDate,'dd-MMM-yyyy')DocDate
							,P.Id PartyId,P.UserName PartyName,PP.GSTIN
							,NULL GRNNo,pp.UserName PartyPlantName,P.PartyNature,IV.PartyType,NULL Material,NULL Article,PC.UserName PartyCategory,PSC.UserName PartySubCategory
                            ,LineItemType=case when v.SourceType='InventoryPayable' then 'Material'
                            WHEN v.SourceType='VendorInvoice' THEN 'GL'
                            WHEN v.SourceType='VendorPayment' THEN 'GL'
                            ELSE '' END
                            ,TaxableAmount=case when v.SourceType='InventoryPayable' then 0
                            when v.SourceType='VendorInvoice' then ISNULL(VD.DrAmount,0)
                            when v.SourceType='VendorPayment' then ISNULL(IWD.Amount,0) else 0 end
                            ,DrAmount=case when ITD.AType='Dr' then ISNULL(IT.TaxAmount,0) else 0 end
							,0 CrAmount
	                        ,format( v.VoucherDate,'dd-MMM-yyyy')VoucherDate
                            ,TC.TaxCategoryType,TC.Code TaxCode,TC.Sequence TCSequence,TC.UserName+'-'+TC.Code TaxCategory,IsNULL(TAXC.IsRCM,0) IsRCM
							
                            ,IsNULL(IV.IsExcludingTax,0) IsExcludingTax,0 IsTaxApplicable,TAXC.[Type],TAXC.ValueOfFixed
                            ,0 [Percentage],NULL HSNCode
							,TaxPercentage= case when v.SourceType='VendorInvoice' then taxc.ValueOfFixed else 0 end
							, Format (IT.AddedDate,'dd-MMM-yyyy')EntryDate,V.Narration
                            from TRN.InvoiceTax IT
                            left join TRN.InvoiceTaxDetail ITD ON IT.Id=ITD.InvoiceTaxId AND ITD.AType='Dr'
                            LEFT JOIN TRN.Voucher V ON V.Id=IT.VoucherId
                            LEFT JOIN TRN.Invoice IV ON IV.Id=IT.InvoiceId
                            LEFT JOIN HKP.Activity TA ON TA.Id=ITD.ActivityId
                            LEFT JOIN HKP.Party P ON P.Id=IT.PartyId
							Left join hkp.PartyPlant PP on PP.Id=IT.PartyPlantId
							Left join hkp.PartyCategory PC on PC.Id=P.PartyCategoryId
							Left join hkp.PartySubCategory PSC on PSC.Id=P.PartySubCategoryId
                            LEFT JOIN MST.TaxCategory TC ON TC.Id=IT.TaxCategoryId
                            LEFT JOIN( select distinct TAC.Id,TAC.UserName,TAC.IsRCM,TAY.[Type],TACD.ValueOfFixed from MST.TaxCode TAC
                            LEFT JOIN MST.TaxCodeYear TAY ON TAY.TaxCodeId=TAC.Id
                            LEFT JOIN MST.TaxCodeDetail TACD ON TACD.TaxCodeId=TAC.Id WHERE TAY.TaxYearId IN ('','7') ) TAXC ON TAXC.Id=IT.TaxCodeId
                            LEFT JOIN TRN.VoucherDetail VD ON VD.Id=IT.VoucherDetailId
                            LEFT JOIN HKP.Activity A ON A.Id=VD.ActivityId
                            LEFT JOIN (SELECT IW.InvoiceWriteOffId,IW.ActivityId,SUM(I.Amount) Amount FROM TRN.InvoiceWriteOffDetail IW
                            JOIN TRN.Invoice I ON I.Id=IW.InvoiceId
                            GROUP BY InvoiceWriteOffId,ActivityId) IWD ON IWD.InvoiceWriteOffId=IT.InvoiceWriteOffId
                            LEFT JOIN HKP.Activity AP ON AP.Id=IWD.ActivityId
                            where TC.TaxCategoryType='GST' AND TAXC.IsRCM=0 AND  V.IsPark=0 AND V.PlantId='" + identity.PlantId + @"'
							and V.PostingDate between '" + FromDate + @"' and '" + ToDate + @"'
                            AND v.SourceType IN ('VendorInvoice','VendorPayment')
                            
                            UNION all

							SELECT 'GRN' SourceType
                            ,V.VoucherNo,format( V.PostingDate,'dd-MMM-yyyy')PostingDate, V.DocRefNo,format (V.DocDate,'dd-MMM-yyyy')DocDate,P.Id PartyId,P.UserName PartyName,PP.GSTIN
							, IRD.InventoryReceiveId GRNNo,pp.UserName PartyPlantName,P.PartyNature,IV.PartyType,MM.UserName Material,MMA.StandardName Article
							,PC.UserName PartyCategory,PSC.UserName PartySubCategory
                            ,LineItemType=case when v.SourceType='InventoryPayable' then 'Material'
                            WHEN v.SourceType='VendorInvoice' THEN 'GL'
                            WHEN v.SourceType='VendorPayment' THEN 'GL'
                            ELSE '' END
                            
                            ,TaxableAmount=IRD.TotalMaterialTranAmount
                            ,DrAmount=case when ITD.AType='Dr' then sum(ISNULL(IRT.TaxAmount,0)) else 0 end,0 CrAmount
	                        ,format( v.VoucherDate,'dd-MMM-yyyy')VoucherDate
                            ,TC.TaxCategoryType,TC.Code TaxCode,TC.Sequence TCSequence,TC.UserName+'-'+TC.Code TaxCategory
							,IsNULL(TAXC.IsRCM,0) IsRCM
                            ,IsNULL(IV.IsExcludingTax,0) IsExcludingTax,IsNULL(IR.IsTaxApplicable,0) IsTaxApplicable
							,TAXC.[Type],TAXC.ValueOfFixed
                            ,IRT.[Percentage],HC.Code HSNCode
							,TaxPercentage= case  when v.SourceType='InventoryPayable' AND IRT.[Percentage]>0 THEN IRT.[Percentage]
												 else 0 end
												 ,Format (it.AddedDate ,'dd-MMM-yyyy') EntryDate,V.Narration
                            from TRN.InvoiceTax IT
                            left join TRN.InvoiceTaxDetail ITD ON IT.Id=ITD.InvoiceTaxId AND ITD.AType='Dr'
                            LEFT JOIN TRN.Voucher V ON V.Id=IT.VoucherId
                            LEFT JOIN TRN.Invoice IV ON IV.Id=IT.InvoiceId
                            LEFT JOIN HKP.Activity TA ON TA.Id=ITD.ActivityId
                            LEFT JOIN HKP.Party P ON P.Id=IT.PartyId
                            Left join hkp.PartyCategory PC on PC.Id=P.PartyCategoryId
							Left join hkp.PartySubCategory PSC on PSC.Id=P.PartySubCategoryId
							
                            LEFT JOIN MST.TaxCategory TC ON TC.Id=IT.TaxCategoryId
                            LEFT JOIN( select TAC.Id,TAC.UserName,TAC.IsRCM,TAY.[Type],TACD.ValueOfFixed from MST.TaxCode TAC
                            LEFT JOIN MST.TaxCodeYear TAY ON TAY.TaxCodeId=TAC.Id
                            LEFT JOIN MST.TaxCodeDetail TACD ON TACD.TaxCodeId=TAC.Id WHERE TAY.TaxYearId IN ('','7') 
							) TAXC ON TAXC.Id=IT.TaxCodeId
                            LEFT JOIN TRN.InventoryReceive IR ON IR.VoucherId=V.Id
                            LEFT JOIN TRN.InventoryReceiveDetail IRD ON IRD.InventoryReceiveId=IR.Id
                            LEFT JOIN TRN.InventoryReceiveTax IRT ON IRD.Id=IRT.InventoryReceiveDetailId AND IRT.TaxCategoryId=IT.TaxCategoryId
                            LEFT JOIN TRN.InventoryMaterial IM ON IM.Id=IRD.InventoryMaterialId
                            LEFT JOIN MST.MaterialMaster MM ON MM.Id=IM.MaterialMasterId
                            LEFT JOIN MST.MaterialMasterArticle MMA ON MMA.Id=IM.ArticleId
                            LEFT JOIN TRN.VoucherDetail VD ON VD.Id=IT.VoucherDetailId
                            LEFT JOIN HKP.Activity A ON A.Id=VD.ActivityId
							LEFT JOIN HKP.HSNCode HC ON HC.Id=IRT.HSNCodeId
                            Left join hkp.PartyPlant pp on pp.Id=IR.InvoicingPartyPlantId
                            where TC.TaxCategoryType='GST' AND IR.IsTaxApplicable=0 AND V.IsPark=0
							AND V.PlantId = '" + identity.PlantId + @"' and V.PostingDate between '" + FromDate + @"' and '" + ToDate + @"'
                            AND v.SourceType='InventoryPayable' and IRT.InventoryServiceId IS NULL
                            GROUP BY 
							V.VoucherNo,V.PostingDate, V.DocRefNo,V.DocDate,P.Id,P.UserName ,PP.GSTIN
							, IRD.InventoryReceiveId ,pp.UserName 
                            , v.SourceType
                            ,v.VoucherDate,IRD.TotalMaterialTranAmount
                            ,TC.TaxCategoryType,TC.Code ,TC.Sequence ,TC.UserName,TC.Code
							,IsNULL(TAXC.IsRCM,0) ,V.Narration
                            ,IsNULL(IV.IsExcludingTax,0) ,IsNULL(IR.IsTaxApplicable,0) 
							,TAXC.[Type],TAXC.ValueOfFixed,ITD.AType,PC.UserName,PSC.UserName
                            ,IRT.[Percentage],IRT.[Percentage] ,it.AddedDate,P.PartyNature,IV.PartyType ,MM.UserName,MMA.StandardName
                            ,HC.Code 

                             UNION all
                            SELECT 'GRN' SourceType
                            ,V.VoucherNo,format( V.PostingDate,'dd-MMM-yyyy')PostingDate, V.DocRefNo,format (V.DocDate,'dd-MMM-yyyy')DocDate,P.Id PartyId,P.UserName PartyName,PP.GSTIN
							, IRD.InventoryReceiveId GRNNo,pp.UserName PartyPlantName,P.PartyNature,IV.PartyType,NULL Material,NULL Article
							,PC.UserName PartyCategory,PSC.UserName PartySubCategory
                            ,LineItemType=case when v.SourceType='InventoryPayable' then 'Material'
                            WHEN v.SourceType='VendorInvoice' THEN 'GL'
                            WHEN v.SourceType='VendorPayment' THEN 'GL'
                            ELSE '' END
                            
                            ,TaxableAmount=case when v.SourceType='InventoryPayable' then 0
                            else 0 end
                            ,DrAmount=case when ITD.AType='Dr' then sum(ISNULL(IRT.TaxAmount,0)) else 0 end,0 CrAmount
	                        ,format( v.VoucherDate,'dd-MMM-yyyy')VoucherDate
                            ,TC.TaxCategoryType,TC.Code TaxCode,TC.Sequence TCSequence,TC.UserName+'-'+TC.Code TaxCategory
							,IsNULL(TAXC.IsRCM,0) IsRCM
                            ,IsNULL(IV.IsExcludingTax,0) IsExcludingTax,IsNULL(IR.IsTaxApplicable,0) IsTaxApplicable
							,TAXC.[Type],TAXC.ValueOfFixed
                            ,IRT.[Percentage],NULL HSNCode
							,TaxPercentage= case  when v.SourceType='InventoryPayable' AND IRT.[Percentage]>0 THEN IRT.[Percentage]
												 else 0 end
												 ,Format (it.AddedDate ,'dd-MMM-yyyy') EntryDate,V.Narration
                            from TRN.InvoiceTax IT
                            left join TRN.InvoiceTaxDetail ITD ON IT.Id=ITD.InvoiceTaxId AND ITD.AType='Dr'
                            LEFT JOIN TRN.Voucher V ON V.Id=IT.VoucherId
                            LEFT JOIN TRN.Invoice IV ON IV.Id=IT.InvoiceId
                            LEFT JOIN HKP.Activity TA ON TA.Id=ITD.ActivityId
                            LEFT JOIN HKP.Party P ON P.Id=IT.PartyId
							Left join hkp.PartyCategory PC on PC.Id=P.PartyCategoryId
							Left join hkp.PartySubCategory PSC on PSC.Id=P.PartySubCategoryId
                            LEFT JOIN MST.TaxCategory TC ON TC.Id=IT.TaxCategoryId
                            LEFT JOIN( select TAC.Id,TAC.UserName,TAC.IsRCM,TAY.[Type],TACD.ValueOfFixed from MST.TaxCode TAC
                            LEFT JOIN MST.TaxCodeYear TAY ON TAY.TaxCodeId=TAC.Id
                            LEFT JOIN MST.TaxCodeDetail TACD ON TACD.TaxCodeId=TAC.Id WHERE TAY.TaxYearId IN ('','7')
							--and tac.IsRCM=0
							) TAXC ON TAXC.Id=IT.TaxCodeId
                            --LEFT JOIN SCS.TaxYear TY ON TY.Id=TAY.TaxYearId
                            LEFT JOIN TRN.InventoryReceive IR ON IR.VoucherId=V.Id
                            LEFT JOIN TRN.InventoryService IRD ON IRD.InventoryReceiveId=IR.Id
                            LEFT JOIN TRN.InventoryReceiveTax IRT ON IRD.Id=IRT.InventoryServiceId AND IRT.TaxCategoryId=IT.TaxCategoryId
                            LEFT JOIN TRN.VoucherDetail VD ON VD.Id=IT.VoucherDetailId
                            LEFT JOIN HKP.Activity A ON A.Id=VD.ActivityId
                            Left join hkp.PartyPlant pp on pp.Id=IR.InvoicingPartyPlantId
                            where TC.TaxCategoryType='GST' AND IR.IsTaxApplicable=0 AND V.IsPark=0
							AND V.PlantId = '" + identity.PlantId + @"' and V.PostingDate between '" + FromDate + @"' and '" + ToDate + @"' 
                            AND v.SourceType='InventoryPayable'  and IRT.InventoryServiceId<>''
							 
                            GROUP BY 
							V.VoucherNo,V.PostingDate, V.DocRefNo,V.DocDate,P.Id,P.UserName ,PP.GSTIN
							, IRD.InventoryReceiveId ,pp.UserName 
                            , v.SourceType
                            ,v.VoucherDate,V.Narration
                            ,TC.TaxCategoryType,TC.Code ,TC.Sequence ,TC.UserName,TC.Code
							,IsNULL(TAXC.IsRCM,0) 
                            ,IsNULL(IV.IsExcludingTax,0) ,IsNULL(IR.IsTaxApplicable,0) 
							,TAXC.[Type],TAXC.ValueOfFixed,ITD.AType,PC.UserName,PSC.UserName
                            ,IRT.[Percentage],IRT.[Percentage] ,it.AddedDate,P.PartyNature,IV.PartyType

							UNION ALL

							--****************TCS*********************************
                            SELECT 'GRN' SourceType
                            ,V.VoucherNo,format( V.PostingDate,'dd-MMM-yyyy')PostingDate, V.DocRefNo,format (V.DocDate,'dd-MMM-yyyy')DocDate,P.Id PartyId,P.UserName PartyName,PP.GSTIN
							, IRD.InventoryReceiveId GRNNo,pp.UserName PartyPlantName,P.PartyNature,IV.PartyType,NULL Material,NULL Article
							,PC.UserName PartyCategory,PSC.UserName PartySubCategory
                            ,LineItemType=case when v.SourceType='InventoryPayable' then 'Material'
                            WHEN v.SourceType='VendorInvoice' THEN 'GL'
                            WHEN v.SourceType='VendorPayment' THEN 'GL'
                            ELSE '' END
                            
                            ,TaxableAmount=case when v.SourceType='InventoryPayable' then 0
                            else 0 end
                            ,DrAmount=case when ITD.AType='Dr' then sum(ISNULL(ITD.Amount,0)) else 0 end,0 CrAmount
	                        ,format( v.VoucherDate,'dd-MMM-yyyy')VoucherDate
                            ,TC.TaxCategoryType,TC.Code TaxCode,TC.Sequence TCSequence,TC.UserName+'-'+TC.Code TaxCategory
							,IsNULL(TAXC.IsRCM,0) IsRCM
                            ,IsNULL(IV.IsExcludingTax,0) IsExcludingTax,IsNULL(IR.IsTaxApplicable,0) IsTaxApplicable
							,TAXC.[Type],TAXC.ValueOfFixed
                            ,NULL [Percentage],NULL HSNCode
							,NULL TaxPercentage
												 ,Format (it.AddedDate ,'dd-MMM-yyyy') EntryDate,V.Narration
                            from TRN.InvoiceTax IT
                            left join TRN.InvoiceTaxDetail ITD ON IT.Id=ITD.InvoiceTaxId AND ITD.AType='Dr'
                            LEFT JOIN TRN.Voucher V ON V.Id=IT.VoucherId
                            LEFT JOIN TRN.Invoice IV ON IV.Id=IT.InvoiceId
                            --LEFT JOIN TRN.InvoiceWriteOff IW ON IW.Id=IT.InvoiceWriteOffId
                            LEFT JOIN HKP.Activity TA ON TA.Id=ITD.ActivityId
                            LEFT JOIN HKP.Party P ON P.Id=IT.PartyId
							Left join hkp.PartyCategory PC on PC.Id=P.PartyCategoryId
							Left join hkp.PartySubCategory PSC on PSC.Id=P.PartySubCategoryId
                            LEFT JOIN MST.TaxCategory TC ON TC.Id=IT.TaxCategoryId
                            LEFT JOIN( select TAC.Id,TAC.UserName,TAC.IsRCM,TAY.[Type],TACD.ValueOfFixed from MST.TaxCode TAC
                            LEFT JOIN MST.TaxCodeYear TAY ON TAY.TaxCodeId=TAC.Id
                            LEFT JOIN MST.TaxCodeDetail TACD ON TACD.TaxCodeId=TAC.Id WHERE TAY.TaxYearId IN ('','7')
							--and tac.IsRCM=0
							) TAXC ON TAXC.Id=IT.TaxCodeId
                            --LEFT JOIN SCS.TaxYear TY ON TY.Id=TAY.TaxYearId
                            LEFT JOIN TRN.InventoryReceive IR ON IR.VoucherId=V.Id
                            LEFT JOIN TRN.InventoryReceiveDetail IRD ON IRD.InventoryReceiveId=IR.Id
                            LEFT JOIN TRN.VoucherDetail VD ON VD.Id=IT.VoucherDetailId
                            LEFT JOIN HKP.Activity A ON A.Id=VD.ActivityId
                            Left join hkp.PartyPlant pp on pp.Id=IR.InvoicingPartyPlantId
                            where TC.TaxCategoryType='TCS' AND IR.IsTaxApplicable=0 AND V.IsPark=0
							AND V.PlantId = '" + identity.PlantId + @"' and V.PostingDate between '" + FromDate + @"' and '" + ToDate + @"'
							AND v.SourceType='InventoryPayable'  
							 
                            GROUP BY 
							V.VoucherNo,V.PostingDate, V.DocRefNo,V.DocDate,P.Id,P.UserName ,PP.GSTIN
							, IRD.InventoryReceiveId ,pp.UserName 
                            , v.SourceType
                            ,v.VoucherDate,V.Narration
                            ,TC.TaxCategoryType,TC.Code ,TC.Sequence ,TC.UserName,TC.Code
							,IsNULL(TAXC.IsRCM,0) 
                            ,IsNULL(IV.IsExcludingTax,0) ,IsNULL(IR.IsTaxApplicable,0) 
							,TAXC.[Type],TAXC.ValueOfFixed,ITD.AType
                           ,it.AddedDate ,P.PartyNature,IV.PartyType,PC.UserName,PSC.UserName

			                UNION	ALL			
                            SELECT 'Service' SourceType
                            ,V.VoucherNo,format(V.PostingDate, 'dd-MMM-yyyy')PostingDate, V.DocRefNo,format(V.DocDate, 'dd-MMM-yyyy')DocDate,P.Id PartyId,P.UserName PartyName, PP.GSTIN
							, IRD.ServiceAcknowledgementMasterId GRNNo,pp.UserName PartyPlantName,P.PartyNature,IV.PartyType,NULL Material,NULL Article
							,PC.UserName PartyCategory,PSC.UserName PartySubCategory
                              , LineItemType =case when v.SourceType = 'ServicePayable' then 'Service' ELSE '' END
                            
                            ,TaxableAmount =case when v.SourceType = 'ServicePayable' then ISNULL(IRD.Amount,0)

                             else 0 end
                            ,DrAmount =case when ITD.AType = 'Dr' then ISNULL(IRT.TaxAmount,0) else 0 end,0 CrAmount
	                        ,format(v.VoucherDate, 'dd-MMM-yyyy')VoucherDate
                            ,TC.TaxCategoryType,TC.Code TaxCode, TC.Sequence TCSequence, TC.UserName + '-' + TC.Code TaxCategory,IsNULL(TAXC.IsRCM, 0) IsRCM
                                   , IsNULL(IV.IsExcludingTax, 0) IsExcludingTax,IsNULL(IR.IsTaxApplicable, 0) IsTaxApplicable,TAXC.[Type],TAXC.ValueOfFixed
                            ,IRT.[Percentage],HC.Code HSNCode
							,TaxPercentage = case  when v.SourceType = 'InventoryPayable'  THEN IRT.[Percentage]

                                                 else 0 end
												 ,Format (it.AddedDate ,'dd-MMM-yyyy') EntryDate,V.Narration
                            from TRN.InvoiceTax IT
                            left
                            join TRN.InvoiceTaxDetail ITD ON IT.Id = ITD.InvoiceTaxId AND ITD.AType = 'Dr'
                            LEFT JOIN TRN.Voucher V ON V.Id = IT.VoucherId
                            LEFT JOIN TRN.Invoice IV ON IV.Id = IT.InvoiceId
                            LEFT JOIN HKP.Activity TA ON TA.Id = ITD.ActivityId
                            LEFT JOIN HKP.Party P ON P.Id = IT.PartyId
							Left join hkp.PartyCategory PC on PC.Id=P.PartyCategoryId
							Left join hkp.PartySubCategory PSC on PSC.Id=P.PartySubCategoryId
                            LEFT JOIN MST.TaxCategory TC ON TC.Id = IT.TaxCategoryId
                            LEFT JOIN(select TAC.Id, TAC.UserName, TAC.IsRCM, TAY.[Type], TACD.ValueOfFixed from MST.TaxCode TAC
                            LEFT JOIN MST.TaxCodeYear TAY ON TAY.TaxCodeId= TAC.Id
                            LEFT JOIN MST.TaxCodeDetail TACD ON TACD.TaxCodeId= TAC.Id WHERE TAY.TaxYearId IN ('','7') 
							) TAXC ON TAXC.Id = IT.TaxCodeId
                            LEFT JOIN TRN.ServiceAcknowledgementMaster IR ON IR.VoucherId = V.Id
                            LEFT JOIN TRN.ServicePOAckTax IRT ON IRT.ServiceAcknowledgementMasterId = IR.Id AND IRT.TaxCategoryId = IT.TaxCategoryId
                            --LEFT JOIN MST.HSNTaxPercentage HSNP ON IRT.HSNCodeId = HSNP.HSNCodeId AND HSNP.TaxCategoryId = IT.TaxCategoryId
                            LEFT JOIN TRN.ServiceAcknowledgementDetail IRD ON IRD.Id = IRT.ServiceAcknowledgementDetailId
                            LEFT JOIN hkp.ServiceMaster SM ON SM.Id = IRD.ServiceMasterId
                             Left join hkp.PartyPlant pp on pp.Id=IR.InvoicingPartyPlantId
							 LEFT JOIN HKP.HSNCode HC ON HC.Id=IRT.HSNCodeId
                            LEFT JOIN TRN.VoucherDetail VD ON VD.Id = IT.VoucherDetailId
                            LEFT JOIN HKP.Activity A ON A.Id = VD.ActivityId
                            LEFT JOIN(SELECT IW.InvoiceWriteOffId, IW.ActivityId, SUM(I.Amount) Amount FROM TRN.InvoiceWriteOffDetail IW
                            JOIN TRN.Invoice I ON I.Id= IW.InvoiceId
                            GROUP BY InvoiceWriteOffId, ActivityId) IWD ON IWD.InvoiceWriteOffId = IT.InvoiceWriteOffId
                            LEFT JOIN HKP.Activity AP ON AP.Id = IWD.ActivityId
                            where TC.TaxCategoryType = 'GST' AND IR.IsTaxApplicable = 0 AND V.IsPark = 0
							 AND V.PlantId = '" + identity.PlantId + @"' and V.PostingDate between '" + FromDate + @"' and '" + ToDate + @"'
                            AND v.SourceType = 'ServicePayable' 
                            ) x

							

							group by x.VoucherNo,x.VoucherDate,x.PostingDate,x.DocRefNo,x.DocDate,x.PartyId,x.PartyName
							,x.TCSequence,x.PartyPlantName,x.GSTIN,x.SourceType,X.HSNCode,X.Narration
							,x.TaxCategoryType,x.EntryDate,x.TaxCode,x.GRNNo,X.PartyNature,X.PartyType,X.Material,X.Article,X.PartyCategory,X.PartySubCategory --,x.TaxPercentage
							--ORDER BY 1,2,4
							)B
DECLARE @sql nvarchar(max), @col nvarchar(max)
                            SELECT @col = (
                                SELECT DISTINCT ','+QUOTENAME(REPLACE(CONVERT(VARCHAR(40), TaxCode, 113), ' ', '-'))    
                                FROM #tempOT 
                                FOR XML PATH ('')
                            ) 
							  
							SELECT @sql = N'
                            (SELECT *
                            FROM #tempOT
                            PIVOT (
                                MAX([DrAmount]) FOR [TaxCode] IN ('+STUFF(@col,1,1,'')+')
                            ) as pvt)' 
                            EXEC sp_executesql @sql
                            drop table #tempOT";

                data = _sqlRepository.GetDataTable(strSQL);
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }

        #endregion
    }
}
