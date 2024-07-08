using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Service.Helpers;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Library.Accounting.Accounts
{
    public class AccountsTrialBalanceService
    {
        private readonly ISqlRepository _sqlRepository;
        public AccountsTrialBalanceService(ISqlRepository sqlRepository
            )
        {
            _sqlRepository = sqlRepository;
        }
        public IWorkbook GetTrialBalanceReport(string companyId, string toDate, bool isBudgetLevel, bool isActivityLevel, bool isDetailLevel)
        {
            var excelEngine = new ExcelEngine();
            var oRU = new ReportUtility();
            var dsLocal = GetTrialBalanceInfo(companyId, toDate, isBudgetLevel, isActivityLevel, isDetailLevel);
            var workbook = oRU.GetWorkbook(ref excelEngine, 1);
            workbook.Version = ExcelVersion.Excel2013;
            var sheet = workbook.Worksheets[0];
            var dtLocal = dsLocal.Tables[0];
            if (dtLocal.Rows.Count > 0)
            {
                var dvParallelCurrency = new DataView(dsLocal.Tables[0])
                {
                    Sort = "CurrencyCode ASC"
                };
                var dtParallelCurrency = dvParallelCurrency.ToTable(true, "CurrencyCode", "ParallelCurrencyId");

                var dvMainBody = new DataView(dsLocal.Tables[0])
                {
                    Sort = "GLGeneralInfoCode"
                };
                var dtMainBody = dvMainBody.ToTable();

                var col = 1;
                var shet2EndxlsCol = col;

                var row = 6;
                row++;
                var headreColIndex = 1;
                var mainColIndex = 1;

                oRU.SetHeaderText(ref sheet, row, headreColIndex, "GL", 32);
                headreColIndex++;
                if (isBudgetLevel)
                {
                    oRU.SetHeaderText(ref sheet, row, headreColIndex, "Budget Name", 32);
                    headreColIndex++;
                }
                if (isActivityLevel)
                {
                    oRU.SetHeaderText(ref sheet, row, headreColIndex, "Budget Name", 32);
                    headreColIndex++;

                    oRU.SetHeaderText(ref sheet, row, headreColIndex, "Activity Name", 32);
                    headreColIndex++;


                }
                if (isDetailLevel)
                {
                    oRU.SetHeaderText(ref sheet, row, headreColIndex, "Budget Name", 32);
                    headreColIndex++;

                    oRU.SetHeaderText(ref sheet, row, headreColIndex, "Activity Name", 32);
                    headreColIndex++;
                    oRU.SetHeaderText(ref sheet, row, headreColIndex, "Particulars", 32);
                    headreColIndex++;


                }
                var colSum = headreColIndex - 1;
                int colCurrencyIndex = headreColIndex;
                var plCurrencyId = string.Empty;
                var plCurrencyCode = string.Empty;

                var alParaCurrency = new ArrayList();

                for (int n = 0; n < dtParallelCurrency.Rows.Count; n++)
                {
                    oRU.SetHeaderText(ref sheet, row - 1, headreColIndex, dtParallelCurrency.Rows[n]["CurrencyCode"].ToString(), ExcelHAlign.HAlignCenter);
                    sheet[row - 1, headreColIndex, row - 1, headreColIndex + 1].Merge();
                    var dic = new Dictionary<string, int>
                {
                    { dtParallelCurrency.Rows[n]["ParallelCurrencyId"].ToString(), headreColIndex }
                };
                    alParaCurrency.Add(dic);

                    oRU.SetHeaderText(ref sheet, row, headreColIndex, "Dr", ExcelHAlign.HAlignRight); headreColIndex++;
                    oRU.SetHeaderText(ref sheet, row, headreColIndex, "Cr", ExcelHAlign.HAlignRight); //headreColIndex++;

                    if (n == 0)
                    {
                        plCurrencyCode = dtParallelCurrency.Rows[n]["CurrencyCode"].ToString();
                    }

                    sheet.Range[row - 1, colCurrencyIndex, row - 1, headreColIndex].BorderAround(ExcelLineStyle.Hair);
                }
                shet2EndxlsCol = headreColIndex - 1;

                var drcrCol = 0;
                var Row_Total_Start = row + 1;

                if (isActivityLevel)
                {
                    for (int n = 0; n < dtMainBody.Rows.Count; n++)
                    {
                        row++;
                        var AccountCodeId = dtMainBody.Rows[n]["GLGeneralInfoCode"].ToString();
                        var BudgetMasterId = dtMainBody.Rows[n]["BudgetMasterId"].ToString();
                        var ActivityId = dtMainBody.Rows[n]["ActivityId"].ToString();
                        //var BankMasterId = dtMainBody.Rows[n]["BankMasterId"].ToString();

                        var Balancetype = dtMainBody.Rows[n]["Balancetype"].ToString();

                        mainColIndex = 1;

                        oRU.SetText(ref sheet, row, mainColIndex, AccountCodeId + " - " + dtMainBody.Rows[n]["GL"]); mainColIndex++;
                        oRU.SetText(ref sheet, row, mainColIndex, dtMainBody.Rows[n]["Budget"].ToString()); mainColIndex++;
                        oRU.SetText(ref sheet, row, mainColIndex, dtMainBody.Rows[n]["Activity"].ToString()); mainColIndex++;
                        // oRU.SetText(ref sheet, row, mainColIndex, dtMainBody.Rows[n]["Particulars"].ToString()); mainColIndex++;

                        for (int p = 0; p < dtParallelCurrency.Rows.Count; p++)
                        {
                            var ParallelCurrencyId = dtParallelCurrency.Rows[p]["ParallelCurrencyId"].ToString();



                            var dvDrCr = new DataView(dsLocal.Tables[0])
                            {
                                RowFilter = "ISNULL(ParallelCurrencyId,'')='" + ParallelCurrencyId + "' AND ISNULL(GLGeneralInfoCode,'')='" + AccountCodeId + "' AND ISNULL(BudgetMasterId,'')='" + BudgetMasterId + "' AND ISNULL(ActivityId,'')='" + ActivityId + "'"
                            };
                            var dtDrCr = dvDrCr.ToTable();
                            if (dtDrCr.Rows.Count != 0)
                            {
                                var _drPC = Convert.ToDouble(dtDrCr.Rows[0]["DRcumulative"].ToString());
                                var _crPC = Convert.ToDouble(dtDrCr.Rows[0]["CRcumulative"].ToString());
                                if (_drPC < 0)
                                {
                                    _crPC += _drPC * -1;
                                    _drPC = 0.00;
                                }
                                if (_crPC < 0)
                                {
                                    _drPC += _crPC * -1;
                                    _crPC = 0.00;
                                }
                                oRU.SetText(ref sheet, row, mainColIndex, _drPC); mainColIndex++;
                                oRU.SetText(ref sheet, row, mainColIndex, _crPC);
                            }

                        }
                    }

                }
                else if (isBudgetLevel)
                {
                    for (int n = 0; n < dtMainBody.Rows.Count; n++)
                    {
                        row++;
                        var AccountCodeId = dtMainBody.Rows[n]["GLGeneralInfoCode"].ToString();
                        var BudgetMasterId = dtMainBody.Rows[n]["BudgetMasterId"].ToString();
                        var _Balancetype = dtMainBody.Rows[n]["Balancetype"].ToString();
                        mainColIndex = 1;
                        oRU.SetText(ref sheet, row, mainColIndex, AccountCodeId + " - " + dtMainBody.Rows[n]["GL"]); mainColIndex++;
                        oRU.SetText(ref sheet, row, mainColIndex, dtMainBody.Rows[n]["Budget"].ToString()); mainColIndex++;

                        for (int p = 0; p < dtParallelCurrency.Rows.Count; p++)
                        {
                            var ParallelCurrencyId = dtParallelCurrency.Rows[p]["ParallelCurrencyId"].ToString();

                            var dvDrCr = new DataView(dsLocal.Tables[0])
                            {
                                RowFilter = "ISNULL(ParallelCurrencyId,'')='" + ParallelCurrencyId + "' AND ISNULL(GLGeneralInfoCode,'')='" + AccountCodeId + "' AND ISNULL(BudgetMasterId,'')='" + BudgetMasterId + "'"
                            };

                            var dtDrCr = dvDrCr.ToTable();
                            if (dtDrCr.Rows.Count != 0)
                            {
                                var _drPC = clsStaticInfo.dbl(dtDrCr.Rows[0]["DRcumulative"].ToString());
                                var _crPC = clsStaticInfo.dbl(dtDrCr.Rows[0]["CRcumulative"].ToString());
                                if (_drPC < 0)
                                {
                                    _crPC += _drPC * -1;
                                    _drPC = 0.00;
                                }
                                if (_crPC < 0)
                                {
                                    _drPC += _crPC * -1;
                                    _crPC = 0.00;
                                }
                                oRU.SetText(ref sheet, row, mainColIndex, _drPC); mainColIndex++;
                                oRU.SetText(ref sheet, row, mainColIndex, _crPC);
                            }
                        }
                    }
                }
                else if (isDetailLevel)
                {
                    for (int n = 0; n < dtMainBody.Rows.Count; n++)
                    {
                        //if(dtMainBody.Rows[n]["Activity"].ToString() == "Issued Share Capital")
                        //{

                        //}

                        row++;
                        var AccountCodeId = dtMainBody.Rows[n]["GLGeneralInfoCode"].ToString();
                        var BudgetMasterId = dtMainBody.Rows[n]["BudgetMasterId"].ToString();
                        var ActivityId = dtMainBody.Rows[n]["ActivityId"].ToString();
                        var BankMasterId = dtMainBody.Rows[n]["BankMasterId"].ToString();
                        var CashMasterId = dtMainBody.Rows[n]["CashMasterId"].ToString();
                        var PartyId = dtMainBody.Rows[n]["PartyId"].ToString();
                        var PartyPlantId = dtMainBody.Rows[n]["PartyPlantId"].ToString();
                        var Balancetype = dtMainBody.Rows[n]["Balancetype"].ToString();

                        mainColIndex = 1;

                        oRU.SetText(ref sheet, row, mainColIndex, AccountCodeId + " - " + dtMainBody.Rows[n]["GL"]); mainColIndex++;
                        oRU.SetText(ref sheet, row, mainColIndex, dtMainBody.Rows[n]["Budget"].ToString()); mainColIndex++;
                        oRU.SetText(ref sheet, row, mainColIndex, dtMainBody.Rows[n]["Activity"].ToString()); mainColIndex++;
                        oRU.SetText(ref sheet, row, mainColIndex, dtMainBody.Rows[n]["Particulars"].ToString()); mainColIndex++;

                        for (int p = 0; p < dtParallelCurrency.Rows.Count; p++)
                        {
                            var ParallelCurrencyId = dtParallelCurrency.Rows[p]["ParallelCurrencyId"].ToString();
                            if (!string.IsNullOrEmpty(BankMasterId))
                            {
                                var dvDrCr = new DataView(dsLocal.Tables[0])
                                {
                                    RowFilter = "ISNULL(ParallelCurrencyId,'')='" + ParallelCurrencyId + "' AND ISNULL(GLGeneralInfoCode,'')='" + AccountCodeId + "' AND ISNULL(BudgetMasterId,'')='" + BudgetMasterId + "' AND ISNULL(ActivityId,'')='" + ActivityId + "' AND ISNULL(BankMasterId,'') = '" + BankMasterId + "'"
                                };
                                var dtDrCr = dvDrCr.ToTable();
                                if (dtDrCr.Rows.Count != 0)
                                {
                                    var _drPC = Convert.ToDouble(dtDrCr.Rows[0]["DRcumulative"].ToString());
                                    var _crPC = Convert.ToDouble(dtDrCr.Rows[0]["CRcumulative"].ToString());
                                    if (_drPC < 0)
                                    {
                                        _crPC += _drPC * -1;
                                        _drPC = 0.00;
                                    }
                                    if (_crPC < 0)
                                    {
                                        _drPC += _crPC * -1;
                                        _crPC = 0.00;
                                    }

                                    oRU.SetText(ref sheet, row, mainColIndex, _drPC); mainColIndex++;
                                    oRU.SetText(ref sheet, row, mainColIndex, _crPC);
                                }
                            }
                            else if (!string.IsNullOrEmpty(CashMasterId))
                            {
                                var dvDrCr = new DataView(dsLocal.Tables[0])
                                {
                                    RowFilter = "ISNULL(ParallelCurrencyId,'')='" + ParallelCurrencyId + "' AND ISNULL(GLGeneralInfoCode,'')='" + AccountCodeId + "' AND ISNULL(BudgetMasterId,'')='" + BudgetMasterId + "' AND ISNULL(ActivityId,'')='" + ActivityId + "'  AND ISNULL(CashMasterId,'') = '" + CashMasterId + "'"
                                };
                                var dtDrCr = dvDrCr.ToTable();
                                if (dtDrCr.Rows.Count != 0)
                                {
                                    var _drPC = Convert.ToDouble(dtDrCr.Rows[0]["DRcumulative"].ToString());
                                    var _crPC = Convert.ToDouble(dtDrCr.Rows[0]["CRcumulative"].ToString());
                                    if (_drPC < 0)
                                    {
                                        _crPC += _drPC * -1;
                                        _drPC = 0.00;
                                    }
                                    if (_crPC < 0)
                                    {
                                        _drPC += _crPC * -1;
                                        _crPC = 0.00;
                                    }
                                    oRU.SetText(ref sheet, row, mainColIndex, _drPC); mainColIndex++;
                                    oRU.SetText(ref sheet, row, mainColIndex, _crPC);
                                }
                            }
                            else if (!string.IsNullOrEmpty(PartyId))
                            {
                                var dvDrCr = new DataView(dsLocal.Tables[0])
                                {
                                    RowFilter = "ISNULL(ParallelCurrencyId,'')='" + ParallelCurrencyId + "' AND ISNULL(GLGeneralInfoCode,'')='" + AccountCodeId + "' AND ISNULL(BudgetMasterId,'')='" + BudgetMasterId + "' AND ISNULL(ActivityId,'')='" + ActivityId + "'  AND ISNULL(PartyId,'') = '" + PartyId + "' AND ISNULL(PartyPlantId,'') = '" + PartyPlantId + "'"
                                };
                                var dtDrCr = dvDrCr.ToTable();
                                if (dtDrCr.Rows.Count != 0)
                                {
                                    var _drPC = Convert.ToDouble(dtDrCr.Rows[0]["DRcumulative"].ToString());
                                    var _crPC = Convert.ToDouble(dtDrCr.Rows[0]["CRcumulative"].ToString());
                                    if (_drPC < 0)
                                    {
                                        _crPC += _drPC * -1;
                                        _drPC = 0.00;
                                    }
                                    if (_crPC < 0)
                                    {
                                        _drPC += _crPC * -1;
                                        _crPC = 0.00;
                                    }
                                    oRU.SetText(ref sheet, row, mainColIndex, _drPC); mainColIndex++;
                                    oRU.SetText(ref sheet, row, mainColIndex, _crPC);
                                }
                            }
                            else
                            {
                                var dvDrCr = new DataView(dsLocal.Tables[0])
                                {
                                    RowFilter = "ISNULL(ParallelCurrencyId,'')='" + ParallelCurrencyId + "' AND ISNULL(GLGeneralInfoCode,'')='" + AccountCodeId + "' AND ISNULL(BudgetMasterId,'')='" + BudgetMasterId + "' AND ISNULL(ActivityId,'')='" + ActivityId + "' AND ISNULL(BankMasterId,'') = '' AND ISNULL(CashMasterId,'') = '' AND ISNULL(PartyId,'') = '' AND ISNULL(PartyPlantId,'') = ''"
                                };
                                var dtDrCr = dvDrCr.ToTable();
                                if (dtDrCr.Rows.Count != 0)
                                {
                                    var _drPC = Convert.ToDouble(dtDrCr.Rows[0]["DRcumulative"].ToString());
                                    var _crPC = Convert.ToDouble(dtDrCr.Rows[0]["CRcumulative"].ToString());
                                    if (_drPC < 0)
                                    {
                                        _crPC += _drPC * -1;
                                        _drPC = 0.00;
                                    }
                                    if (_crPC < 0)
                                    {
                                        _drPC += _crPC * -1;
                                        _crPC = 0.00;
                                    }
                                    oRU.SetText(ref sheet, row, mainColIndex, _drPC); mainColIndex++;
                                    oRU.SetText(ref sheet, row, mainColIndex, _crPC);
                                }
                            }
                        }
                    }

                }
                else
                {
                    for (int n = 0; n < dtMainBody.Rows.Count; n++)
                    {
                        if (Convert.ToDouble(dtMainBody.Rows[n]["DRcumulative"].ToString()) + Convert.ToDouble(dtMainBody.Rows[n]["CRcumulative"].ToString()) != 0)
                        {
                            row++;
                            var AccountCodeId = dtMainBody.Rows[n]["GLGeneralInfoCode"].ToString();
                            var _Balancetype = dtMainBody.Rows[n]["Balancetype"].ToString();
                            oRU.SetText(ref sheet, row, mainColIndex, AccountCodeId + " - " + dtMainBody.Rows[n]["GL"]);
                            mainColIndex++;
                            for (int p = 0; p < dtParallelCurrency.Rows.Count; p++)
                            {
                                var ParallelCurrencyId = dtParallelCurrency.Rows[p]["ParallelCurrencyId"].ToString();

                                var dvDrCr = new DataView(dsLocal.Tables[0])
                                {
                                    RowFilter = "ISNULL(ParallelCurrencyId,'')='" + ParallelCurrencyId + "' AND ISNULL(GLGeneralInfoCode,'')='" + AccountCodeId + "'"
                                };
                                var dtDrCr = dvDrCr.ToTable();
                                if (dtDrCr.Rows.Count != 0)
                                {
                                    drcrCol++;
                                    var _drPC = Convert.ToDouble(dtDrCr.Rows[0]["DRcumulative"].ToString());
                                    var _crPC = Convert.ToDouble(dtDrCr.Rows[0]["CRcumulative"].ToString());
                                    if (_drPC < 0)
                                    {
                                        _crPC += _drPC * -1;
                                        _drPC = 0.00;
                                    }
                                    if (_crPC < 0)
                                    {
                                        _drPC += _crPC * -1;
                                        _crPC = 0.00;
                                    }
                                    oRU.SetText(ref sheet, row, mainColIndex, _drPC); mainColIndex++;
                                    oRU.SetText(ref sheet, row, mainColIndex, _crPC);
                                }
                            }
                            mainColIndex = 1;
                        }
                    }
                }

                row++;

                oRU.SetMasterHeaderText(ref sheet, row, colSum, "Total ");
                sheet.Range[oRU.GetColumnNameForXls(1) + row + ": " + oRU.GetColumnNameForXls(colSum) + row].Merge();

                var sumdrcrCol = colSum + 1;
                for (int s = 0; s < dtParallelCurrency.Rows.Count; s++)
                {
                    sheet.Range[row, sumdrcrCol].Formula = "=SUM(" + oRU.GetColumnNameForXls(sumdrcrCol) + Row_Total_Start + ":" + oRU.GetColumnNameForXls(sumdrcrCol) + (row - 1) + ")";
                    sheet.Range[row, sumdrcrCol].NumberFormat = oRU.NumberFormatDecimalTwo();
                    sheet.Range[row, sumdrcrCol].CellStyle.Font.Bold = true;
                    sheet.Range[row, sumdrcrCol].BorderAround(ExcelLineStyle.Hair);

                    sumdrcrCol++;
                    sheet.Range[row, sumdrcrCol].Formula = "=SUM(" + oRU.GetColumnNameForXls(sumdrcrCol) + Row_Total_Start + ":" + oRU.GetColumnNameForXls(sumdrcrCol) + (row - 1) + ")";
                    sheet.Range[row, sumdrcrCol].NumberFormat = oRU.NumberFormatDecimalTwo();
                    sheet.Range[row, sumdrcrCol].CellStyle.Font.Bold = true;
                    sheet.Range[row, sumdrcrCol].BorderAround(ExcelLineStyle.Hair);
                }

                var colLast = sumdrcrCol;
                sheet.Range[8, 1, row, colLast].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[8, 1, row, colLast].BorderAround(ExcelLineStyle.Hair);

                sheet.Name = "Sheet";
                sheet.UsedRange.AutofitColumns();
                sheet.UsedRange.CellStyle.Font.Size = 8;
                oRU.CompanyHeader(ref sheet, colLast, "Trial Balance", companyId);
                //oRU.CompanyPlantHeader(ref sheet, colLast, "Trial Balance", companyId, plantId, plantName, null);
                oRU.SetText(ref sheet, 5, colLast, "As On " + toDate + "", ExcelHAlign.HAlignCenter);
                sheet.Range[oRU.GetColumnNameForXls(1) + 5 + ":" + oRU.GetColumnNameForXls(colLast) + 5].Merge();
                if (isActivityLevel)
                {
                    oRU.PageSetup(ref sheet, 5, ExcelPageOrientation.Landscape);
                }
                else
                {
                    oRU.PageSetup(ref sheet, 5, ExcelPageOrientation.Portrait);
                }
            }
            else
            {
                sheet.Name = "Sheet";
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                oRU.CompanyHeader(ref sheet, 5, "Trial Balance", identity.CompanyId);
                //oRU.CompanyPlantHeader(ref sheet, 5, "Trial Balance", identity.CompanyId, plantId, plantName, null);
                oRU.SetText(ref sheet, 5, 3, "No Data Found", ExcelHAlign.HAlignCenter);
                oRU.PageSetup(ref sheet, 5, ExcelPageOrientation.Portrait);
            }
            return workbook;
        }
        private DataSet GetTrialBalanceInfo(string companyId, string toDate, bool isBudgetLevel, bool isActivityLevel, bool isDetailLevel)
        {
            GridParameter parameters = null;
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                parameters = new GridParameter
                {
                    ExportType = "DATASET"
                };
                if (isActivityLevel)
                {
                    parameters.CmdText = @"SELECT * FROM( SELECT distinct	GL.Id AS AccountCodeId,
		                                    VDC.ParallelCurrencyId,CU.Code AS CurrencyCode,
		                         sum(CASE WHEN ACT.BalanceType = 'Debit' THEN (sum(VDC.DrAmount)-sum(VDC.CrAmount)) ELSE 0 END) over (partition by GL.Id, VD.BudgetMasterId, A.Id, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) as DRcumulative
                                , sum(CASE WHEN ACT.BalanceType = 'Credit' THEN (sum(VDC.CrAmount)-sum(VDC.DrAmount)) ELSE 0 END) over (partition by GL.Id, VD.BudgetMasterId,A.Id, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) as CRcumulative ,
                                            ACT.BalanceType,
                                            ACT.Id AS [MainHead],
		                                    VD.GLGeneralInfoId,GL.UserName AS GL, GL.AccountCode AS GLGeneralInfoCode,
                                            VD.BudgetMasterId,
		                                    BUD.UserName AS Budget,
											A.UserName AS Activity,
									
                                            A.Id AS ActivityId
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
                                            WHERE v.PostingDate <= '" + toDate + @"' and v.CompanyId ='" + companyId + @"' AND  v.IsPark=0
                                            AND VDC.VoucherDetailId NOT IN ( SELECT VD.Id FROM  TRN.VoucherDetail AS VD  
																INNER JOIN TRN.Voucher AS V ON V.Id=VD.VoucherId
																LEFT JOIN HKP.GLGeneralInfo AS GL ON GL.Id=VD.GLGeneralInfoId
																LEFT OUTER JOIN HKP.AccountGroup AS AG ON AG.Id=GL.AccountGroupId
																LEFT OUTER JOIN [HKP].[AccountType] act on act.Id =AG.AccountTypeId
																WHERE ACT.Id IN('Revenue','Expense') AND V.FiscalYearId in(select FiscalYearId from [SCS].[FiscalYearClose] ))
                                              GROUP BY GL.Id, GL.AccountCode, VDC.ParallelCurrencyId, CU.Code, VD.GLGeneralInfoId, GL.UserName, 
											GL.AccountCode, ACT.BalanceType, ACT.Id, VD.BudgetMasterId, A.UserName, BUD.UserName, v.PostingDate, A.Id
											 ) ttd 
                                            WHERE ISNULL(DRcumulative,0.00) <> 0.00 OR ISNULL(CRcumulative,0) <> 0.00";

                    return _sqlRepository.GetGridData(parameters).Source;
                }
                else if (isBudgetLevel)
                {
                    parameters.CmdText = @"  SELECT * FROM (SELECT distinct	GL.Id AS AccountCodeId,
		                                    VDC.ParallelCurrencyId,CU.Code AS CurrencyCode,
		                                    sum(CASE WHEN ACT.BalanceType = 'Debit' THEN (sum(VDC.DrAmount)-sum(VDC.CrAmount)) ELSE 0 END) over (partition by GL.Id, VD.BudgetMasterId, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) as DRcumulative
                                            , sum(CASE WHEN ACT.BalanceType = 'Credit' THEN (sum(VDC.CrAmount)-sum(VDC.DrAmount)) ELSE 0 END) over (partition by GL.Id, VD.BudgetMasterId, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) as CRcumulative ,
                                            ACT.BalanceType,
                                            ACT.Id AS [MainHead],
		                                    VD.GLGeneralInfoId,GL.UserName AS GL, GL.AccountCode AS GLGeneralInfoCode,
                                            VD.BudgetMasterId,
		                                    BUD.UserName AS Budget
	                                        FROM TRN.VoucherDetailCurrency AS VDC
		                                    INNER JOIN TRN.VoucherDetail AS VD ON VD.Id =VDC.VoucherDetailId
		                                    INNER JOIN TRN.Voucher AS V ON V.Id=VD.VoucherId
		                                    LEFT JOIN HKP.GLGeneralInfo AS GL ON GL.Id=VD.GLGeneralInfoId
                                            LEFT OUTER JOIN HKP.AccountGroup AS AG ON AG.Id=GL.AccountGroupId
                                            LEFT OUTER JOIN [HKP].[AccountType] act on act.Id =AG.AccountTypeId
                                            LEFT JOIN SCS.Currency AS CU ON CU.Id=VDC.ParallelCurrencyId
											LEFT JOIN MST.BudgetMaster BM ON VD.BudgetMasterId=BM.Id
                                            LEFT JOIN [HKP].[Budget] AS BUD ON BM.BudgetId=BUD.Id
                                            where v.PostingDate <= '" + toDate + @"' and v.CompanyId ='" + companyId + @"'  and  v.IsPark=0
                                            AND VDC.VoucherDetailId NOT IN ( SELECT VD.Id FROM  TRN.VoucherDetail AS VD  
																INNER JOIN TRN.Voucher AS V ON V.Id=VD.VoucherId
																LEFT JOIN HKP.GLGeneralInfo AS GL ON GL.Id=VD.GLGeneralInfoId
																LEFT OUTER JOIN HKP.AccountGroup AS AG ON AG.Id=GL.AccountGroupId
																LEFT OUTER JOIN [HKP].[AccountType] act on act.Id =AG.AccountTypeId
																WHERE ACT.Id IN('Revenue','Expense') AND V.FiscalYearId in(select FiscalYearId from [SCS].[FiscalYearClose] ))
                                            GROUP BY GL.Id, GL.AccountCode, VDC.ParallelCurrencyId,CU.Code,VD.GLGeneralInfoId,GL.UserName, GL.AccountCode, ACT.BalanceType,ACT.Id,VD.BudgetMasterId,BUD.UserName,v.PostingDate) ttd 
                                            WHERE ISNULL(DRcumulative,0.00) <> 0.00 OR ISNULL(CRcumulative,0) <> 0.00";

                    return _sqlRepository.GetGridData(parameters).Source;
                }
                else if (isDetailLevel)
                {
                    parameters.CmdText = @"SELECT * FROM( SELECT distinct	GL.Id AS AccountCodeId,
		                                    VDC.ParallelCurrencyId,CU.Code AS CurrencyCode,
		                         sum(CASE WHEN ACT.BalanceType = 'Debit' THEN (sum(VDC.DrAmount)-sum(VDC.CrAmount)) ELSE 0 END) over (partition by GL.Id, VD.BudgetMasterId, A.Id,VD.BankMasterId,VD.CashMasterId, VD.PartyId, VD.PartyPlantId, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) as DRcumulative
                                , sum(CASE WHEN ACT.BalanceType = 'Credit' THEN (sum(VDC.CrAmount)-sum(VDC.DrAmount)) ELSE 0 END) over (partition by GL.Id, VD.BudgetMasterId,A.Id,VD.BankMasterId,VD.CashMasterId, VD.PartyId, VD.PartyPlantId, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) as CRcumulative ,
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
                                            WHERE v.PostingDate <= '" + toDate + @"' and v.CompanyId ='" + companyId + @"' AND  v.IsPark=0
                                            AND VDC.VoucherDetailId NOT IN ( SELECT VD.Id FROM  TRN.VoucherDetail AS VD  
																INNER JOIN TRN.Voucher AS V ON V.Id=VD.VoucherId
																LEFT JOIN HKP.GLGeneralInfo AS GL ON GL.Id=VD.GLGeneralInfoId
																LEFT OUTER JOIN HKP.AccountGroup AS AG ON AG.Id=GL.AccountGroupId
																LEFT OUTER JOIN [HKP].[AccountType] act on act.Id =AG.AccountTypeId
																WHERE ACT.Id IN('Revenue','Expense') AND V.FiscalYearId in(select FiscalYearId from [SCS].[FiscalYearClose] ))
                                            GROUP BY GL.Id, GL.AccountCode, VDC.ParallelCurrencyId, CU.Code, VD.GLGeneralInfoId, GL.UserName, 
											GL.AccountCode, ACT.BalanceType, ACT.Id, VD.BudgetMasterId, A.UserName, BUD.UserName, v.PostingDate, A.Id, BA.AccountTitle, CM.UserName
											,VD.BankMasterId, VD.CashMasterId, P.UserName, PP.UserName, VD.PartyId, VD.PartyPlantId ) ttd 
                                            WHERE ISNULL(DRcumulative,0.00) <> 0.00 OR ISNULL(CRcumulative,0) <> 0.00";

                    return _sqlRepository.GetGridData(parameters).Source;


                }

                else
                {
                    parameters.CmdText = @" SELECT * FROM (SELECT  distinct	GL.Id AS AccountCodeId,
		                                    VDC.ParallelCurrencyId,CU.Code AS CurrencyCode,
		                                 sum(CASE WHEN ACT.BalanceType = 'Debit' THEN (sum(VDC.DrAmount)-sum(VDC.CrAmount)) ELSE 0 END) over (partition by GL.Id, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) as DRcumulative
                                         , sum(CASE WHEN ACT.BalanceType = 'Credit' THEN (sum(VDC.CrAmount)-sum(VDC.DrAmount)) ELSE 0 END) over (partition by GL.Id, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) as CRcumulative ,
                                            ACT.BalanceType,
                                            ACT.Id AS [MainHead],
		                                    VD.GLGeneralInfoId,GL.UserName AS GL, GL.AccountCode AS GLGeneralInfoCode
	                                        FROM TRN.VoucherDetailCurrency AS VDC
		                                    INNER JOIN TRN.VoucherDetail AS VD ON VD.Id =VDC.VoucherDetailId
		                                    INNER JOIN TRN.Voucher AS V ON V.Id=VD.VoucherId
		                                    LEFT JOIN HKP.GLGeneralInfo AS GL ON GL.Id=VD.GLGeneralInfoId
                                            LEFT OUTER JOIN HKP.AccountGroup AS AG ON AG.Id=GL.AccountGroupId
                                            LEFT OUTER JOIN [HKP].[AccountType] act on act.Id =AG.AccountTypeId
                                            LEFT JOIN SCS.Currency AS CU ON CU.Id=VDC.ParallelCurrencyId
                                            where v.PostingDate <= '" + toDate + @"' and v.CompanyId ='" + companyId + @"' and  v.IsPark=0
                                            AND VDC.VoucherDetailId NOT IN ( SELECT VD.Id FROM  TRN.VoucherDetail AS VD  
																INNER JOIN TRN.Voucher AS V ON V.Id=VD.VoucherId
																LEFT JOIN HKP.GLGeneralInfo AS GL ON GL.Id=VD.GLGeneralInfoId
																LEFT OUTER JOIN HKP.AccountGroup AS AG ON AG.Id=GL.AccountGroupId
																LEFT OUTER JOIN [HKP].[AccountType] act on act.Id =AG.AccountTypeId
																WHERE ACT.Id IN('Revenue','Expense') AND V.FiscalYearId in(select FiscalYearId from [SCS].[FiscalYearClose] ))
                                            group by GL.Id, GL.AccountCode, VDC.ParallelCurrencyId,CU.Code,VD.GLGeneralInfoId,GL.UserName, GL.AccountCode, ACT.BalanceType,ACT.Id,v.PostingDate) ttd 
                                            WHERE ISNULL(DRcumulative,0.00) <> 0.00 OR ISNULL(CRcumulative,0) <> 0.00";

                    return _sqlRepository.GetGridData(parameters).Source;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IWorkbook GetDateRangeWiseTrialBalanceReport(string companyId, string fromDate, string toDate, bool isBudgetLevel, bool isActivityLevel, bool isDetailLevel)
        {
            var excelEngine = new ExcelEngine();
            var oRU = new ReportUtility();
            var dsLocal = GetDateRangeWiseTrialBalanceInfo(companyId, fromDate, toDate, isBudgetLevel, isActivityLevel, isDetailLevel);
            var workbook = oRU.GetWorkbook(ref excelEngine, 1);
            workbook.Version = ExcelVersion.Excel2013;
            var sheet = workbook.Worksheets[0];
            var dtLocal = dsLocal.Tables[0];
            var obDebit = 0;
            var obCredit = 0;
            var Debit = 0;
            var Credit = 0;
            var cbDebit = 0;
            var cbCredit = 0;

            if (dtLocal.Rows.Count > 0)
            {
                var dvParallelCurrency = new DataView(dsLocal.Tables[0])
                {
                    Sort = "CurrencyCode ASC"
                };
                var dtParallelCurrency = dvParallelCurrency.ToTable(true, "CurrencyCode", "ParallelCurrencyId");

                var dvMainBody = new DataView(dsLocal.Tables[0])
                {
                    Sort = "GLGeneralInfoCode"
                };
                var dtMainBody = dvMainBody.ToTable();

                var col = 1;
                var shet2EndxlsCol = col;
                var colParticulers = 0;
                var row = 6;
                row++;
                var headreColIndex = 1;
                var mainColIndex = 1;

                oRU.SetHeaderText(ref sheet, row, headreColIndex, "GL", 32);
                headreColIndex++;
                if (isBudgetLevel)
                {
                    oRU.SetHeaderText(ref sheet, row, headreColIndex, "Budget Name", 32);
                    headreColIndex++;
                }
                if (isActivityLevel)
                {
                    oRU.SetHeaderText(ref sheet, row, headreColIndex, "Budget Name", 32);
                    headreColIndex++;

                    oRU.SetHeaderText(ref sheet, row, headreColIndex, "Activity Name", 35);

                    headreColIndex++;
                }
                if (isDetailLevel)
                {
                    oRU.SetHeaderText(ref sheet, row, headreColIndex, "Budget Name", 32);
                    headreColIndex++;

                    oRU.SetHeaderText(ref sheet, row, headreColIndex, "Activity Name", 35);
                    headreColIndex++;

                    oRU.SetHeaderText(ref sheet, row, headreColIndex, "Particulars", 35);

                    colParticulers = headreColIndex;
                    headreColIndex++;


                }
                var colSum = headreColIndex - 1;
                var plCurrencyId = string.Empty;
                var plCurrencyCode = string.Empty;

                var alParaCurrency = new ArrayList();

                for (int n = 0; n < dtParallelCurrency.Rows.Count; n++)
                {

                    row++;
                    row++;

                    oRU.SetHeaderText(ref sheet, row, headreColIndex, "Dr", ExcelHAlign.HAlignRight); obDebit = headreColIndex; headreColIndex++;
                    oRU.SetHeaderText(ref sheet, row, headreColIndex, "Cr", ExcelHAlign.HAlignRight); obCredit = headreColIndex; headreColIndex++;
                    oRU.SetHeaderText(ref sheet, row, headreColIndex, "Dr", ExcelHAlign.HAlignRight); Debit = headreColIndex; headreColIndex++;
                    oRU.SetHeaderText(ref sheet, row, headreColIndex, "Cr", ExcelHAlign.HAlignRight); Credit = headreColIndex; headreColIndex++;
                    oRU.SetHeaderText(ref sheet, row, headreColIndex, "Dr", ExcelHAlign.HAlignRight); cbDebit = headreColIndex; headreColIndex++;
                    oRU.SetHeaderText(ref sheet, row, headreColIndex, "Cr", ExcelHAlign.HAlignRight); cbCredit = headreColIndex; headreColIndex++;

                    oRU.SetHeaderText(ref sheet, row - 1, obDebit, "Openning Balance", ExcelHAlign.HAlignCenter);
                    sheet[row - 1, obDebit, row - 1, obCredit].Merge();
                    oRU.SetHeaderText(ref sheet, row - 1, Debit, "For the Period", ExcelHAlign.HAlignCenter);
                    sheet[row - 1, Debit, row - 1, Credit].Merge();
                    oRU.SetHeaderText(ref sheet, row - 1, cbDebit, "Closing Balance", ExcelHAlign.HAlignCenter);
                    sheet[row - 1, cbDebit, row - 1, cbCredit].Merge();


                    oRU.SetHeaderText(ref sheet, row - 2, obDebit, dtParallelCurrency.Rows[n]["CurrencyCode"].ToString(), ExcelHAlign.HAlignCenter);
                    sheet[row - 2, obDebit, row - 2, cbCredit].Merge();
                    var dic = new Dictionary<string, int>
                {
                    { dtParallelCurrency.Rows[n]["ParallelCurrencyId"].ToString(), headreColIndex }
                };
                    alParaCurrency.Add(dic);


                    if (n == 0)
                    {
                        plCurrencyCode = dtParallelCurrency.Rows[n]["CurrencyCode"].ToString();
                    }
                }
                shet2EndxlsCol = headreColIndex - 1;

                var drcrCol = 0;
                var Row_Total_Start = row + 1;
                if (isActivityLevel)
                {
                    for (int n = 0; n < dtMainBody.Rows.Count; n++)
                    {
                        row++;
                        var AccountCodeId = dtMainBody.Rows[n]["GLGeneralInfoCode"].ToString();
                        var BudgetMasterId = dtMainBody.Rows[n]["BudgetMasterId"].ToString();
                        var ActivityId = dtMainBody.Rows[n]["ActivityId"].ToString();

                        var _Balancetype = dtMainBody.Rows[n]["Balancetype"].ToString();
                        mainColIndex = 1;

                        oRU.SetText(ref sheet, row, mainColIndex, AccountCodeId + " - " + dtMainBody.Rows[n]["GL"]); mainColIndex++;
                        oRU.SetText(ref sheet, row, mainColIndex, dtMainBody.Rows[n]["Budget"].ToString()); mainColIndex++;
                        oRU.SetText(ref sheet, row, mainColIndex, dtMainBody.Rows[n]["Activity"].ToString()); mainColIndex++;

                        //oRU.SetText(ref sheet, row, mainColIndex, dtMainBody.Rows[n]["Particulars"].ToString()); mainColIndex++;
                        //if(dtMainBody.Rows[n]["Particulars"].ToString().Length >= 34)
                        //{
                        //    sheet.Range[row, colParticulers].RowHeight = 12.75 * 2;
                        //    sheet.Range[row, colParticulers].WrapText = true;

                        //    if (dtMainBody.Rows[n]["Particulars"].ToString().Length >= 70)
                        //    {
                        //        sheet.Range[row, colParticulers].RowHeight = 12.75 * 3;
                        //    }
                        //}
                        for (int p = 0; p < dtParallelCurrency.Rows.Count; p++)
                        {
                            var ParallelCurrencyId = dtParallelCurrency.Rows[p]["ParallelCurrencyId"].ToString();



                            var dvDrCr = new DataView(dsLocal.Tables[0])
                            {
                                RowFilter = "ParallelCurrencyId='" + ParallelCurrencyId + "' AND GLGeneralInfoCode='" + AccountCodeId + "' AND BudgetMasterId='" + BudgetMasterId + "' AND ActivityId='" + ActivityId + "'"

                                // RowFilter = "ParallelCurrencyId='" + ParallelCurrencyId + "' AND GLGeneralInfoCode='" + AccountCodeId + "' AND BudgetMasterId='" + BudgetMasterId + "' AND ActivityId='" + ActivityId + "'  AND PartyId = '" + PartyId + "' AND PartyPlantId = '" + PartyPlantId + "'"
                            };
                            var dtActDrCr = dvDrCr.ToTable();
                            if (dtActDrCr.Rows.Count != 0)
                            {
                                for (int acp = 0; acp < dtParallelCurrency.Rows.Count; acp++)
                                {
                                    var acpParallelCurrencyId = dtParallelCurrency.Rows[acp]["ParallelCurrencyId"].ToString();

                                    //var dvActDrCr = new DataView(dsLocal.Tables[0])
                                    //{
                                    //    RowFilter = "ParallelCurrencyId='" + ParallelCurrencyId + "' AND GLGeneralInfoCode='" + AccountCodeId + "'"
                                    //};
                                    //var dtActDrCr = dvActDrCr.ToTable();
                                    if (dtActDrCr.Rows.Count != 0)
                                    {
                                        drcrCol++;
                                        var _obDrPC = clsStaticInfo.dbl(dtActDrCr.Rows[0]["OBDRcumulative"].ToString());
                                        var _obCrPC = clsStaticInfo.dbl(dtActDrCr.Rows[0]["OBCRcumulative"].ToString());
                                        if (_obDrPC < 0)
                                        {
                                            _obCrPC += _obDrPC * -1;
                                            _obDrPC = 0.00;
                                        }
                                        if (_obCrPC < 0)
                                        {
                                            _obDrPC += _obCrPC * -1;
                                            _obCrPC = 0.00;
                                        }
                                        var _drPC = clsStaticInfo.dbl(dtActDrCr.Rows[0]["DRcumulative"].ToString());
                                        var _crPC = clsStaticInfo.dbl(dtActDrCr.Rows[0]["CRcumulative"].ToString());
                                        if (_drPC < 0)
                                        {
                                            _crPC += _drPC * -1;
                                            _drPC = 0.00;
                                        }
                                        if (_crPC < 0)
                                        {
                                            _drPC += _crPC * -1;
                                            _crPC = 0.00;
                                        }
                                        var _cbDrPC = clsStaticInfo.dbl(dtActDrCr.Rows[0]["CBDRcumulative"].ToString());
                                        var _cbCrPC = clsStaticInfo.dbl(dtActDrCr.Rows[0]["CBCRcumulative"].ToString());
                                        if (_cbDrPC < 0)
                                        {
                                            _cbCrPC = _cbDrPC * -1;
                                            _cbDrPC = 0.00;
                                        }
                                        if (_cbCrPC < 0)
                                        {
                                            _cbDrPC = _cbCrPC * -1;
                                            _cbCrPC = 0.00;
                                        }
                                        oRU.SetText(ref sheet, row, obDebit, _obDrPC);
                                        oRU.SetText(ref sheet, row, obCredit, _obCrPC);
                                        oRU.SetText(ref sheet, row, Debit, _drPC);
                                        oRU.SetText(ref sheet, row, Credit, _crPC);
                                        oRU.SetText(ref sheet, row, cbDebit, _cbDrPC);
                                        oRU.SetText(ref sheet, row, cbCredit, _cbCrPC);
                                    }
                                }
                                //oRU.SetText(ref sheet, row, mainColIndex, _drPC); mainColIndex++;
                                //oRU.SetText(ref sheet, row, mainColIndex, _crPC);
                            }

                        }
                    }
                }
                else if (isBudgetLevel)
                {
                    for (int n = 0; n < dtMainBody.Rows.Count; n++)
                    {
                        row++;
                        var AccountCodeId = dtMainBody.Rows[n]["GLGeneralInfoCode"].ToString();
                        var BudgetMasterId = dtMainBody.Rows[n]["BudgetMasterId"].ToString();
                        var _Balancetype = dtMainBody.Rows[n]["Balancetype"].ToString();
                        mainColIndex = 1;
                        oRU.SetText(ref sheet, row, mainColIndex, AccountCodeId + " - " + dtMainBody.Rows[n]["GL"]); mainColIndex++;
                        oRU.SetText(ref sheet, row, mainColIndex, dtMainBody.Rows[n]["Budget"].ToString()); mainColIndex++;

                        for (int p = 0; p < dtParallelCurrency.Rows.Count; p++)
                        {
                            var ParallelCurrencyId = dtParallelCurrency.Rows[p]["ParallelCurrencyId"].ToString();

                            var dvDrCr = new DataView(dsLocal.Tables[0])
                            {
                                RowFilter = "ParallelCurrencyId='" + ParallelCurrencyId + "' AND GLGeneralInfoCode='" + AccountCodeId + "' AND BudgetMasterId='" + BudgetMasterId + "'"
                            };

                            var dtDrCr = dvDrCr.ToTable();
                            if (dtDrCr.Rows.Count != 0)
                            {
                                drcrCol++;
                                var _obDrPC = clsStaticInfo.dbl(dtDrCr.Rows[0]["OBDRcumulative"].ToString());
                                var _obCrPC = clsStaticInfo.dbl(dtDrCr.Rows[0]["OBCRcumulative"].ToString());
                                if (_obDrPC < 0)
                                {
                                    _obCrPC += _obDrPC * -1;
                                    _obDrPC = 0.00;
                                }
                                if (_obCrPC < 0)
                                {
                                    _obDrPC += _obCrPC * -1;
                                    _obCrPC = 0.00;
                                }
                                var _drPC = clsStaticInfo.dbl(dtDrCr.Rows[0]["DRcumulative"].ToString());
                                var _crPC = clsStaticInfo.dbl(dtDrCr.Rows[0]["CRcumulative"].ToString());
                                if (_drPC < 0)
                                {
                                    _crPC += _drPC * -1;
                                    _drPC = 0.00;
                                }
                                if (_crPC < 0)
                                {
                                    _drPC += _crPC * -1;
                                    _crPC = 0.00;
                                }
                                var _pDrPC = clsStaticInfo.dbl(dtDrCr.Rows[0]["PDRcumulative"].ToString());
                                var _pCrPC = clsStaticInfo.dbl(dtDrCr.Rows[0]["PCRcumulative"].ToString());
                                if (_pDrPC < 0)
                                {
                                    _pCrPC += _pDrPC * -1;
                                    _pDrPC = 0.00;
                                }
                                if (_pCrPC < 0)
                                {
                                    _pCrPC += _pCrPC * -1;
                                    _pCrPC = 0.00;
                                }
                                var _cbDrPC = clsStaticInfo.dbl(dtDrCr.Rows[0]["CBDRcumulative"].ToString());
                                var _cbCrPC = clsStaticInfo.dbl(dtDrCr.Rows[0]["CBCRcumulative"].ToString());
                                if (_cbDrPC < 0)
                                {
                                    _cbCrPC = _cbDrPC * -1;
                                    _cbDrPC = 0.00;
                                }
                                if (_cbCrPC < 0)
                                {
                                    _cbDrPC = _cbCrPC * -1;
                                    _cbCrPC = 0.00;
                                }
                                oRU.SetText(ref sheet, row, obDebit, _obDrPC);
                                oRU.SetText(ref sheet, row, obCredit, _obCrPC);
                                oRU.SetText(ref sheet, row, Debit, _pDrPC);
                                oRU.SetText(ref sheet, row, Credit, _pCrPC);
                                oRU.SetText(ref sheet, row, cbDebit, _cbDrPC);
                                oRU.SetText(ref sheet, row, cbCredit, _cbCrPC);
                            }
                        }
                    }
                }
                else if (isDetailLevel)
                {
                    for (int n = 0; n < dtMainBody.Rows.Count; n++)
                    {
                        row++;
                        var AccountCodeId = dtMainBody.Rows[n]["GLGeneralInfoCode"].ToString();
                        var BudgetMasterId = dtMainBody.Rows[n]["BudgetMasterId"].ToString();
                        var ActivityId = dtMainBody.Rows[n]["ActivityId"].ToString();
                        var BankMasterId = dtMainBody.Rows[n]["BankMasterId"].ToString();
                        var CashMasterId = dtMainBody.Rows[n]["CashMasterId"].ToString();
                        var PartyId = dtMainBody.Rows[n]["PartyId"].ToString();
                        var PartyPlantId = dtMainBody.Rows[n]["PartyPlantId"].ToString();
                        var _Balancetype = dtMainBody.Rows[n]["Balancetype"].ToString();
                        mainColIndex = 1;

                        oRU.SetText(ref sheet, row, mainColIndex, AccountCodeId + " - " + dtMainBody.Rows[n]["GL"]); mainColIndex++;
                        oRU.SetText(ref sheet, row, mainColIndex, dtMainBody.Rows[n]["Budget"].ToString()); mainColIndex++;
                        oRU.SetText(ref sheet, row, mainColIndex, dtMainBody.Rows[n]["Activity"].ToString()); mainColIndex++;

                        oRU.SetText(ref sheet, row, mainColIndex, dtMainBody.Rows[n]["Particulars"].ToString()); mainColIndex++;
                        //if(dtMainBody.Rows[n]["Particulars"].ToString().Length >= 34)
                        //{
                        //    sheet.Range[row, colParticulers].RowHeight = 12.75 * 2;
                        //    sheet.Range[row, colParticulers].WrapText = true;

                        //    if (dtMainBody.Rows[n]["Particulars"].ToString().Length >= 70)
                        //    {
                        //        sheet.Range[row, colParticulers].RowHeight = 12.75 * 3;
                        //    }
                        //}
                        for (int p = 0; p < dtParallelCurrency.Rows.Count; p++)
                        {
                            var ParallelCurrencyId = dtParallelCurrency.Rows[p]["ParallelCurrencyId"].ToString();
                            if (!string.IsNullOrEmpty(BankMasterId))
                            {
                                var dvDrCr = new DataView(dsLocal.Tables[0])
                                {
                                    RowFilter = "ISNULL(ParallelCurrencyId,'')='" + ParallelCurrencyId + "' AND ISNULL(GLGeneralInfoCode,'')='" + AccountCodeId + "' AND ISNULL(BudgetMasterId,'')='" + BudgetMasterId + "' AND ISNULL(ActivityId,'')='" + ActivityId + "' AND ISNULL(BankMasterId,'') = '" + BankMasterId + "'"
                                };
                                var dtActDrCr = dvDrCr.ToTable();
                                if (dtActDrCr.Rows.Count != 0)
                                {
                                    for (int acp = 0; acp < dtParallelCurrency.Rows.Count; acp++)
                                    {
                                        var acpParallelCurrencyId = dtParallelCurrency.Rows[acp]["ParallelCurrencyId"].ToString();

                                        //var dvActDrCr = new DataView(dsLocal.Tables[0])
                                        //{
                                        //    RowFilter = "ParallelCurrencyId='" + ParallelCurrencyId + "' AND GLGeneralInfoCode='" + AccountCodeId + "'"
                                        //};
                                        //var dtActDrCr = dvActDrCr.ToTable();
                                        if (dtActDrCr.Rows.Count != 0)
                                        {
                                            drcrCol++;
                                            var _obDrPC = clsStaticInfo.dbl(dtActDrCr.Rows[0]["OBDRcumulative"].ToString());
                                            var _obCrPC = clsStaticInfo.dbl(dtActDrCr.Rows[0]["OBCRcumulative"].ToString());
                                            if (_obDrPC < 0)
                                            {
                                                _obCrPC += _obDrPC * -1;
                                                _obDrPC = 0.00;
                                            }
                                            if (_obCrPC < 0)
                                            {
                                                _obDrPC += _obCrPC * -1;
                                                _obCrPC = 0.00;
                                            }
                                            var _drPC = clsStaticInfo.dbl(dtActDrCr.Rows[0]["DRcumulative"].ToString());
                                            var _crPC = clsStaticInfo.dbl(dtActDrCr.Rows[0]["CRcumulative"].ToString());
                                            if (_drPC < 0)
                                            {
                                                _crPC += _drPC * -1;
                                                _drPC = 0.00;
                                            }
                                            if (_crPC < 0)
                                            {
                                                _drPC += _crPC * -1;
                                                _crPC = 0.00;
                                            }
                                            var _pDrPC = clsStaticInfo.dbl(dtActDrCr.Rows[0]["PDRcumulative"].ToString());
                                            var _pCrPC = clsStaticInfo.dbl(dtActDrCr.Rows[0]["PCRcumulative"].ToString());
                                            if (_pDrPC < 0)
                                            {
                                                _pCrPC += _pDrPC * -1;
                                                _pDrPC = 0.00;
                                            }
                                            if (_pCrPC < 0)
                                            {
                                                _pCrPC += _pCrPC * -1;
                                                _pCrPC = 0.00;
                                            }
                                            var _cbDrPC = clsStaticInfo.dbl(dtActDrCr.Rows[0]["CBDRcumulative"].ToString());
                                            var _cbCrPC = clsStaticInfo.dbl(dtActDrCr.Rows[0]["CBCRcumulative"].ToString());
                                            if (_cbDrPC < 0)
                                            {
                                                _cbCrPC = _cbDrPC * -1;
                                                _cbDrPC = 0.00;
                                            }
                                            if (_cbCrPC < 0)
                                            {
                                                _cbDrPC = _cbCrPC * -1;
                                                _cbCrPC = 0.00;
                                            }
                                            oRU.SetText(ref sheet, row, obDebit, _obDrPC);
                                            oRU.SetText(ref sheet, row, obCredit, _obCrPC);
                                            oRU.SetText(ref sheet, row, Debit, _pDrPC);
                                            oRU.SetText(ref sheet, row, Credit, _pCrPC);
                                            oRU.SetText(ref sheet, row, cbDebit, _cbDrPC);
                                            oRU.SetText(ref sheet, row, cbCredit, _cbCrPC);
                                        }
                                    }
                                    //oRU.SetText(ref sheet, row, mainColIndex, _drPC); mainColIndex++;
                                    //oRU.SetText(ref sheet, row, mainColIndex, _crPC);
                                }
                            }
                            else if (!string.IsNullOrEmpty(CashMasterId))
                            {
                                var dvDrCr = new DataView(dsLocal.Tables[0])
                                {
                                    RowFilter = "ParallelCurrencyId='" + ParallelCurrencyId + "' AND GLGeneralInfoCode='" + AccountCodeId + "' AND BudgetMasterId='" + BudgetMasterId + "' AND ActivityId='" + ActivityId + "'  AND CashMasterId = '" + CashMasterId + "'"
                                };
                                var dtActDrCr = dvDrCr.ToTable();
                                if (dtActDrCr.Rows.Count != 0)
                                {
                                    for (int acp = 0; acp < dtParallelCurrency.Rows.Count; acp++)
                                    {
                                        var acpParallelCurrencyId = dtParallelCurrency.Rows[acp]["ParallelCurrencyId"].ToString();

                                        //var dvActDrCr = new DataView(dsLocal.Tables[0])
                                        //{
                                        //    RowFilter = "ParallelCurrencyId='" + ParallelCurrencyId + "' AND GLGeneralInfoCode='" + AccountCodeId + "'"
                                        //};
                                        //var dtActDrCr = dvActDrCr.ToTable();
                                        if (dtActDrCr.Rows.Count != 0)
                                        {
                                            drcrCol++;
                                            var _obDrPC = clsStaticInfo.dbl(dtActDrCr.Rows[0]["OBDRcumulative"].ToString());
                                            var _obCrPC = clsStaticInfo.dbl(dtActDrCr.Rows[0]["OBCRcumulative"].ToString());
                                            if (_obDrPC < 0)
                                            {
                                                _obCrPC += _obDrPC * -1;
                                                _obDrPC = 0.00;
                                            }
                                            if (_obCrPC < 0)
                                            {
                                                _obDrPC += _obCrPC * -1;
                                                _obCrPC = 0.00;
                                            }
                                            var _drPC = clsStaticInfo.dbl(dtActDrCr.Rows[0]["DRcumulative"].ToString());
                                            var _crPC = clsStaticInfo.dbl(dtActDrCr.Rows[0]["CRcumulative"].ToString());
                                            if (_drPC < 0)
                                            {
                                                _crPC += _drPC * -1;
                                                _drPC = 0.00;
                                            }
                                            if (_crPC < 0)
                                            {
                                                _drPC += _crPC * -1;
                                                _crPC = 0.00;
                                            }
                                            var _pDrPC = clsStaticInfo.dbl(dtActDrCr.Rows[0]["PDRcumulative"].ToString());
                                            var _pCrPC = clsStaticInfo.dbl(dtActDrCr.Rows[0]["PCRcumulative"].ToString());
                                            if (_pDrPC < 0)
                                            {
                                                _pCrPC += _pDrPC * -1;
                                                _pDrPC = 0.00;
                                            }
                                            if (_pCrPC < 0)
                                            {
                                                _pCrPC += _pCrPC * -1;
                                                _pCrPC = 0.00;
                                            }
                                            var _cbDrPC = clsStaticInfo.dbl(dtActDrCr.Rows[0]["CBDRcumulative"].ToString());
                                            var _cbCrPC = clsStaticInfo.dbl(dtActDrCr.Rows[0]["CBCRcumulative"].ToString());
                                            if (_cbDrPC < 0)
                                            {
                                                _cbCrPC = _cbDrPC * -1;
                                                _cbDrPC = 0.00;
                                            }
                                            if (_cbCrPC < 0)
                                            {
                                                _cbDrPC = _cbCrPC * -1;
                                                _cbCrPC = 0.00;
                                            }
                                            oRU.SetText(ref sheet, row, obDebit, _obDrPC);
                                            oRU.SetText(ref sheet, row, obCredit, _obCrPC);
                                            oRU.SetText(ref sheet, row, Debit, _pDrPC);
                                            oRU.SetText(ref sheet, row, Credit, _pCrPC);
                                            oRU.SetText(ref sheet, row, cbDebit, _cbDrPC);
                                            oRU.SetText(ref sheet, row, cbCredit, _cbCrPC);
                                        }
                                    }
                                    //oRU.SetText(ref sheet, row, mainColIndex, _drPC); mainColIndex++;
                                    //oRU.SetText(ref sheet, row, mainColIndex, _crPC);
                                }
                            }
                            else if (!string.IsNullOrEmpty(PartyId))
                            {
                                var dvDrCr = new DataView(dsLocal.Tables[0])
                                {
                                    RowFilter = "ParallelCurrencyId='" + ParallelCurrencyId + "' AND GLGeneralInfoCode='" + AccountCodeId + "' AND BudgetMasterId='" + BudgetMasterId + "' AND ActivityId='" + ActivityId + "'  AND PartyId = '" + PartyId + "' AND PartyPlantId = '" + PartyPlantId + "'"
                                };
                                var dtActDrCr = dvDrCr.ToTable();
                                if (dtActDrCr.Rows.Count != 0)
                                {
                                    for (int acp = 0; acp < dtParallelCurrency.Rows.Count; acp++)
                                    {
                                        var acpParallelCurrencyId = dtParallelCurrency.Rows[acp]["ParallelCurrencyId"].ToString();

                                        //var dvActDrCr = new DataView(dsLocal.Tables[0])
                                        //{
                                        //    RowFilter = "ParallelCurrencyId='" + ParallelCurrencyId + "' AND GLGeneralInfoCode='" + AccountCodeId + "'"
                                        //};
                                        //var dtActDrCr = dvActDrCr.ToTable();
                                        if (dtActDrCr.Rows.Count != 0)
                                        {
                                            drcrCol++;
                                            var _obDrPC = clsStaticInfo.dbl(dtActDrCr.Rows[0]["OBDRcumulative"].ToString());
                                            var _obCrPC = clsStaticInfo.dbl(dtActDrCr.Rows[0]["OBCRcumulative"].ToString());
                                            if (_obDrPC < 0)
                                            {
                                                _obCrPC += _obDrPC * -1;
                                                _obDrPC = 0.00;
                                            }
                                            if (_obCrPC < 0)
                                            {
                                                _obDrPC += _obCrPC * -1;
                                                _obCrPC = 0.00;
                                            }
                                            var _drPC = clsStaticInfo.dbl(dtActDrCr.Rows[0]["DRcumulative"].ToString());
                                            var _crPC = clsStaticInfo.dbl(dtActDrCr.Rows[0]["CRcumulative"].ToString());
                                            if (_drPC < 0)
                                            {
                                                _crPC += _drPC * -1;
                                                _drPC = 0.00;
                                            }
                                            if (_crPC < 0)
                                            {
                                                _drPC += _crPC * -1;
                                                _crPC = 0.00;
                                            }
                                            var _pDrPC = clsStaticInfo.dbl(dtActDrCr.Rows[0]["PDRcumulative"].ToString());
                                            var _pCrPC = clsStaticInfo.dbl(dtActDrCr.Rows[0]["PCRcumulative"].ToString());
                                            if (_pDrPC < 0)
                                            {
                                                _pCrPC += _pDrPC * -1;
                                                _pDrPC = 0.00;
                                            }
                                            if (_pCrPC < 0)
                                            {
                                                _pCrPC += _pCrPC * -1;
                                                _pCrPC = 0.00;
                                            }
                                            var _cbDrPC = clsStaticInfo.dbl(dtActDrCr.Rows[0]["CBDRcumulative"].ToString());
                                            var _cbCrPC = clsStaticInfo.dbl(dtActDrCr.Rows[0]["CBCRcumulative"].ToString());
                                            if (_cbDrPC < 0)
                                            {
                                                _cbCrPC = _cbDrPC * -1;
                                                _cbDrPC = 0.00;
                                            }
                                            if (_cbCrPC < 0)
                                            {
                                                _cbDrPC = _cbCrPC * -1;
                                                _cbCrPC = 0.00;
                                            }
                                            oRU.SetText(ref sheet, row, obDebit, _obDrPC);
                                            oRU.SetText(ref sheet, row, obCredit, _obCrPC);
                                            oRU.SetText(ref sheet, row, Debit, _pDrPC);
                                            oRU.SetText(ref sheet, row, Credit, _pCrPC);
                                            oRU.SetText(ref sheet, row, cbDebit, _cbDrPC);
                                            oRU.SetText(ref sheet, row, cbCredit, _cbCrPC);
                                        }
                                    }
                                    //oRU.SetText(ref sheet, row, mainColIndex, _drPC); mainColIndex++;
                                    //oRU.SetText(ref sheet, row, mainColIndex, _crPC);
                                }
                            }
                            else
                            {

                                var dvDrCr = new DataView(dsLocal.Tables[0])
                                {
                                    RowFilter = "ParallelCurrencyId='" + ParallelCurrencyId + "' AND GLGeneralInfoCode='" + AccountCodeId + "' AND BudgetMasterId='" + BudgetMasterId + "' AND ActivityId='" + ActivityId + "' AND ISNULL(BankMasterId,'') = '' AND ISNULL(CashMasterId,'') = '' AND ISNULL(PartyId,'') = '' AND ISNULL(PartyPlantId,'') = ''"

                                    // RowFilter = "ParallelCurrencyId='" + ParallelCurrencyId + "' AND GLGeneralInfoCode='" + AccountCodeId + "' AND BudgetMasterId='" + BudgetMasterId + "' AND ActivityId='" + ActivityId + "'  AND PartyId = '" + PartyId + "' AND PartyPlantId = '" + PartyPlantId + "'"
                                };
                                var dtActDrCr = dvDrCr.ToTable();
                                if (dtActDrCr.Rows.Count != 0)
                                {
                                    for (int acp = 0; acp < dtParallelCurrency.Rows.Count; acp++)
                                    {
                                        var acpParallelCurrencyId = dtParallelCurrency.Rows[acp]["ParallelCurrencyId"].ToString();

                                        //var dvActDrCr = new DataView(dsLocal.Tables[0])
                                        //{
                                        //    RowFilter = "ParallelCurrencyId='" + ParallelCurrencyId + "' AND GLGeneralInfoCode='" + AccountCodeId + "'"
                                        //};
                                        //var dtActDrCr = dvActDrCr.ToTable();
                                        if (dtActDrCr.Rows.Count != 0)
                                        {
                                            drcrCol++;
                                            var _obDrPC = clsStaticInfo.dbl(dtActDrCr.Rows[0]["OBDRcumulative"].ToString());
                                            var _obCrPC = clsStaticInfo.dbl(dtActDrCr.Rows[0]["OBCRcumulative"].ToString());
                                            if (_obDrPC < 0)
                                            {
                                                _obCrPC += _obDrPC * -1;
                                                _obDrPC = 0.00;
                                            }
                                            if (_obCrPC < 0)
                                            {
                                                _obDrPC += _obCrPC * -1;
                                                _obCrPC = 0.00;
                                            }
                                            var _drPC = clsStaticInfo.dbl(dtActDrCr.Rows[0]["DRcumulative"].ToString());
                                            var _crPC = clsStaticInfo.dbl(dtActDrCr.Rows[0]["CRcumulative"].ToString());
                                            if (_drPC < 0)
                                            {
                                                _crPC += _drPC * -1;
                                                _drPC = 0.00;
                                            }
                                            if (_crPC < 0)
                                            {
                                                _drPC += _crPC * -1;
                                                _crPC = 0.00;
                                            }
                                            var _cbDrPC = clsStaticInfo.dbl(dtActDrCr.Rows[0]["CBDRcumulative"].ToString());
                                            var _cbCrPC = clsStaticInfo.dbl(dtActDrCr.Rows[0]["CBCRcumulative"].ToString());
                                            if (_cbDrPC < 0)
                                            {
                                                _cbCrPC = _cbDrPC * -1;
                                                _cbDrPC = 0.00;
                                            }
                                            if (_cbCrPC < 0)
                                            {
                                                _cbDrPC = _cbCrPC * -1;
                                                _cbCrPC = 0.00;
                                            }
                                            oRU.SetText(ref sheet, row, obDebit, _obDrPC);
                                            oRU.SetText(ref sheet, row, obCredit, _obCrPC);
                                            oRU.SetText(ref sheet, row, Debit, _drPC);
                                            oRU.SetText(ref sheet, row, Credit, _crPC);
                                            oRU.SetText(ref sheet, row, cbDebit, _cbDrPC);
                                            oRU.SetText(ref sheet, row, cbCredit, _cbCrPC);
                                        }
                                    }
                                    //oRU.SetText(ref sheet, row, mainColIndex, _drPC); mainColIndex++;
                                    //oRU.SetText(ref sheet, row, mainColIndex, _crPC);
                                }
                            }
                        }
                    }

                }

                else
                {
                    for (int n = 0; n < dtMainBody.Rows.Count; n++)
                    {
                        if (clsStaticInfo.dbl(dtMainBody.Rows[n]["OBDRcumulative"].ToString()) + clsStaticInfo.dbl(dtMainBody.Rows[n]["OBCRcumulative"].ToString()) + clsStaticInfo.dbl(dtMainBody.Rows[n]["DRcumulative"].ToString()) + clsStaticInfo.dbl(dtMainBody.Rows[n]["CRcumulative"].ToString()) + clsStaticInfo.dbl(dtMainBody.Rows[n]["CBDRcumulative"].ToString()) + clsStaticInfo.dbl(dtMainBody.Rows[n]["CBCRcumulative"].ToString()) != 0)
                        {
                            row++;
                            var AccountCodeId = dtMainBody.Rows[n]["GLGeneralInfoCode"].ToString();
                            var _Balancetype = dtMainBody.Rows[n]["Balancetype"].ToString();
                            oRU.SetText(ref sheet, row, mainColIndex, AccountCodeId + " - " + dtMainBody.Rows[n]["GL"]);
                            mainColIndex++;
                            for (int p = 0; p < dtParallelCurrency.Rows.Count; p++)
                            {
                                var ParallelCurrencyId = dtParallelCurrency.Rows[p]["ParallelCurrencyId"].ToString();

                                var dvDrCr = new DataView(dsLocal.Tables[0])
                                {
                                    RowFilter = "ParallelCurrencyId='" + ParallelCurrencyId + "' AND GLGeneralInfoCode='" + AccountCodeId + "'"
                                };
                                var dtDrCr = dvDrCr.ToTable();
                                if (dtDrCr.Rows.Count != 0)
                                {
                                    drcrCol++;
                                    var _obDrPC = clsStaticInfo.dbl(dtDrCr.Rows[0]["OBDRcumulative"].ToString());
                                    var _obCrPC = clsStaticInfo.dbl(dtDrCr.Rows[0]["OBCRcumulative"].ToString());
                                    if (_obDrPC < 0)
                                    {
                                        _obCrPC += _obDrPC * -1;
                                        _obDrPC = 0.00;
                                    }
                                    if (_obCrPC < 0)
                                    {
                                        _obDrPC += _obCrPC * -1;
                                        _obCrPC = 0.00;
                                    }
                                    var _drPC = clsStaticInfo.dbl(dtDrCr.Rows[0]["DRcumulative"].ToString());
                                    var _crPC = clsStaticInfo.dbl(dtDrCr.Rows[0]["CRcumulative"].ToString());
                                    if (_drPC < 0)
                                    {
                                        _crPC += _drPC * -1;
                                        _drPC = 0.00;
                                    }
                                    if (_crPC < 0)
                                    {
                                        _drPC += _crPC * -1;
                                        _crPC = 0.00;
                                    }
                                    var _pDrPC = clsStaticInfo.dbl(dtDrCr.Rows[0]["PDRcumulative"].ToString());
                                    var _pCrPC = clsStaticInfo.dbl(dtDrCr.Rows[0]["PCRcumulative"].ToString());
                                    if (_pDrPC < 0)
                                    {
                                        _pCrPC += _pDrPC * -1;
                                        _pDrPC = 0.00;
                                    }
                                    if (_pCrPC < 0)
                                    {
                                        _pCrPC += _pCrPC * -1;
                                        _pCrPC = 0.00;
                                    }
                                    var _cbDrPC = clsStaticInfo.dbl(dtDrCr.Rows[0]["CBDRcumulative"].ToString());
                                    var _cbCrPC = clsStaticInfo.dbl(dtDrCr.Rows[0]["CBCRcumulative"].ToString());
                                    if (_cbDrPC < 0)
                                    {
                                        _cbCrPC = _cbDrPC * -1;
                                        _cbDrPC = 0.00;
                                    }
                                    if (_cbCrPC < 0)
                                    {
                                        _cbDrPC = _cbCrPC * -1;
                                        _cbCrPC = 0.00;
                                    }
                                    oRU.SetText(ref sheet, row, obDebit, _obDrPC);
                                    oRU.SetText(ref sheet, row, obCredit, _obCrPC);
                                    oRU.SetText(ref sheet, row, Debit, _pDrPC);
                                    oRU.SetText(ref sheet, row, Credit, _pCrPC);
                                    oRU.SetText(ref sheet, row, cbDebit, _cbDrPC);
                                    oRU.SetText(ref sheet, row, cbCredit, _cbCrPC);
                                }
                            }
                            mainColIndex = 1;
                        }
                    }
                }

                row++;

                oRU.SetMasterHeaderText(ref sheet, row, colSum, "Total ");
                sheet.Range[oRU.GetColumnNameForXls(1) + row + ": " + oRU.GetColumnNameForXls(colSum) + row].Merge();

                var sumdrcrCol = colSum + 1;
                for (int s = 0; s < dtParallelCurrency.Rows.Count; s++)
                {
                    sheet.Range[row, obDebit].Formula = "=SUM(" + oRU.GetColumnNameForXls(obDebit) + Row_Total_Start + ":" + oRU.GetColumnNameForXls(obDebit) + (row - 1) + ")";
                    sheet.Range[row, obDebit].NumberFormat = oRU.NumberFormatDecimalTwo();
                    sheet.Range[row, obDebit].CellStyle.Font.Bold = true;
                    sheet.Range[row, obDebit].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, obCredit].Formula = "=SUM(" + oRU.GetColumnNameForXls(obCredit) + Row_Total_Start + ":" + oRU.GetColumnNameForXls(obCredit) + (row - 1) + ")";
                    sheet.Range[row, obCredit].NumberFormat = oRU.NumberFormatDecimalTwo();
                    sheet.Range[row, obCredit].CellStyle.Font.Bold = true;
                    sheet.Range[row, obCredit].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, Credit].Formula = "=SUM(" + oRU.GetColumnNameForXls(Credit) + Row_Total_Start + ":" + oRU.GetColumnNameForXls(Credit) + (row - 1) + ")";
                    sheet.Range[row, Credit].NumberFormat = oRU.NumberFormatDecimalTwo();
                    sheet.Range[row, Credit].CellStyle.Font.Bold = true;
                    sheet.Range[row, Credit].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, Debit].Formula = "=SUM(" + oRU.GetColumnNameForXls(Debit) + Row_Total_Start + ":" + oRU.GetColumnNameForXls(Debit) + (row - 1) + ")";
                    sheet.Range[row, Debit].NumberFormat = oRU.NumberFormatDecimalTwo();
                    sheet.Range[row, Debit].CellStyle.Font.Bold = true;
                    sheet.Range[row, Debit].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, cbCredit].Formula = "=SUM(" + oRU.GetColumnNameForXls(cbCredit) + Row_Total_Start + ":" + oRU.GetColumnNameForXls(cbCredit) + (row - 1) + ")";
                    sheet.Range[row, cbCredit].NumberFormat = oRU.NumberFormatDecimalTwo();
                    sheet.Range[row, cbCredit].CellStyle.Font.Bold = true;
                    sheet.Range[row, cbCredit].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, cbDebit].Formula = "=SUM(" + oRU.GetColumnNameForXls(cbDebit) + Row_Total_Start + ":" + oRU.GetColumnNameForXls(cbDebit) + (row - 1) + ")";
                    sheet.Range[row, cbDebit].NumberFormat = oRU.NumberFormatDecimalTwo();
                    sheet.Range[row, cbDebit].CellStyle.Font.Bold = true;
                    sheet.Range[row, cbDebit].BorderAround(ExcelLineStyle.Hair);
                }

                var colLast = cbCredit;

                sheet.Range[8, 1, row, colLast].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[8, 1, row, colLast].BorderAround(ExcelLineStyle.Hair);

                sheet.Name = "Trial Balance";
                sheet.UsedRange.AutofitColumns();
                sheet.UsedRange.CellStyle.Font.Size = 8;
                //oRU.CompanyPlantHeader(ref sheet, colLast, "Trial Balance", companyId, plantName, null);
                oRU.CompanyHeader(ref sheet, colLast, "Trial Balance", companyId);
                oRU.SetText(ref sheet, 5, colLast, "Between " + fromDate + " AND " + toDate + "", ExcelHAlign.HAlignCenter);
                sheet.Range[oRU.GetColumnNameForXls(1) + 5 + ":" + oRU.GetColumnNameForXls(colLast) + 5].Merge();
                if (isActivityLevel)
                {
                    oRU.PageSetup(ref sheet, 5, ExcelPageOrientation.Landscape);
                    //sheet["A" + colParticulers.ToString()].ColumnWidth = 35;

                }
                else
                {
                    oRU.PageSetup(ref sheet, 5, ExcelPageOrientation.Portrait);

                }

                //sheet.PageSetup.PrintTitleRows = "$A$1:$IV$" + titleRow;
            }
            else
            {
                sheet.Name = "Trial Balance";
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                //oRU.CompanyPlantHeader(ref sheet, 5, "Trial Balance", identity.CompanyId, plantName, null);
                oRU.CompanyHeader(ref sheet, 5, "Trial Balance", companyId);
                oRU.SetText(ref sheet, 5, 3, "No Data Found", ExcelHAlign.HAlignCenter);
                oRU.PageSetup(ref sheet, 5, ExcelPageOrientation.Portrait);
            }

            return workbook;
        }

        private DataSet GetDateRangeWiseTrialBalanceInfo(string companyId, string fromDate, string toDate, bool isBudgetLevel, bool isActivityLevel, bool isDetailLevel)
        {
            GridParameter parameters = null;
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                parameters = new GridParameter
                {
                    ExportType = "DATASET"
                };
                if (isActivityLevel)
                {
                    parameters.CmdText = @"
                                            SELECT * 


                                            FROM(SELECT  AccountCodeId,ParallelCurrencyId,CurrencyCode,
		                                  SuM(OBDRcumulative + FROBDRcumulative) OBDRcumulative, SUM(OBCRcumulative + FROBCRcumulative) OBCRcumulative
										, SUM(DRcumulative) DRcumulative, SUM(CRcumulative) CRcumulative
                                            , SUM(OBDRcumulative + DRcumulative+FROBDRcumulative) CBDRcumulative, SUm(OBCRcumulative + CRcumulative+FROBCRcumulative) CBCRcumulative
                                           , SUM(PDRcumulative) PDRcumulative, SUM(PCRcumulative) PCRcumulative
										   ,BalanceType,[MainHead],GLGeneralInfoId,GL,GLGeneralInfoCode,Budget
										 ,ISNULL(BudgetMasterId,'') BudgetMasterId
										 ,Activity,ISNULL(ActivityId,'') ActivityId
		                                 FROM
		                                ( SELECT distinct	GL.Id AS AccountCodeId,
		                                    VDC.ParallelCurrencyId,CU.Code AS CurrencyCode,
		                                        SUM(CASE WHEN ACT.BalanceType = 'Debit' THEN (sum(VDC.DrAmount) - sum(VDC.CrAmount)) ELSE 0 END) OVER (PARTITION BY GL.Id, VD.BudgetMasterId,A.Id, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId
			                                                ) AS OBDRcumulative, sum(CASE WHEN ACT.BalanceType = 'Credit' THEN (sum(VDC.CrAmount) - sum(VDC.DrAmount)) ELSE 0 END) OVER (PARTITION BY GL.Id, VD.BudgetMasterId,A.Id, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId
			                                                ) AS OBCRcumulative, 0 DRcumulative, 0 CRcumulative, 0 CBDRcumulative, 0 CBCRcumulative,0 FROBDRcumulative, 0 FROBCRcumulative,0 PDRcumulative,0 PCRcumulative,       
										    ACT.BalanceType,
                                            ACT.Id AS [MainHead],
		                                    VD.GLGeneralInfoId,GL.UserName AS GL, GL.AccountCode AS GLGeneralInfoCode,
                                            VD.BudgetMasterId,
		                                    BUD.UserName AS Budget,
											A.UserName AS Activity,
											
                                            A.Id AS ActivityId
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
                                            WHERE v.PostingDate < '" + fromDate + @"' and v.CompanyId ='" + companyId + @"' AND  v.IsPark=0
                                            AND VDC.VoucherDetailId NOT IN ( SELECT VD.Id FROM  TRN.VoucherDetail AS VD  
																INNER JOIN TRN.Voucher AS V ON V.Id=VD.VoucherId
																LEFT JOIN HKP.GLGeneralInfo AS GL ON GL.Id=VD.GLGeneralInfoId
																LEFT OUTER JOIN HKP.AccountGroup AS AG ON AG.Id=GL.AccountGroupId
																LEFT OUTER JOIN [HKP].[AccountType] act on act.Id =AG.AccountTypeId
																WHERE ACT.Id IN('Revenue','Expense') AND V.FiscalYearId in(select FiscalYearId from [SCS].[FiscalYearClose] ))
                                            GROUP BY GL.Id, GL.AccountCode, VDC.ParallelCurrencyId, CU.Code, VD.GLGeneralInfoId, GL.UserName, 
											GL.AccountCode, ACT.BalanceType, ACT.Id, VD.BudgetMasterId, A.UserName, BUD.UserName, v.PostingDate, A.Id
											
											UNION 

										
											   SELECT distinct	GL.Id AS AccountCodeId,
		                                    VDC.ParallelCurrencyId,CU.Code AS CurrencyCode,0 OBDRcumulative,0 OBCRcumulative,
		                                        SUM(CASE WHEN ACT.BalanceType = 'Debit' THEN (sum(VDC.DrAmount) - sum(VDC.CrAmount)) ELSE 0 END) OVER (PARTITION BY GL.Id, VD.BudgetMasterId,A.Id, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId
			                                                ) AS DRcumulative, sum(CASE WHEN ACT.BalanceType = 'Credit' THEN (sum(VDC.CrAmount) - sum(VDC.DrAmount)) ELSE 0 END) OVER (PARTITION BY GL.Id, VD.BudgetMasterId,A.Id, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId
			                                                ) AS CRcumulative
                                 
                                           , 0 CBDRcumulative, 0 CBCRcumulative,0 FROBDRcumulative, 0 FROBCRcumulative   
										    , SUM(CASE WHEN SUM(VDC.DrAmount)<>0 THEN (SUM(VDC.DrAmount)) 
																		 ELSE 0 END
															) OVER (
			                                           PARTITION BY GL.Id, VD.BudgetMasterId,A.Id, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId
			                                                ) AS PDRcumulative
															
															, SUM(CASE WHEN SUM(VDC.CrAmount)<>0 THEN (SUM(VDC.CrAmount)) 
																		 ELSE 0 END
															) OVER (PARTITION BY GL.Id, VD.BudgetMasterId,A.Id, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId
			                                                ) AS PCRcumulative,
										    ACT.BalanceType,
                                            ACT.Id AS [MainHead],
		                                    VD.GLGeneralInfoId,GL.UserName AS GL, GL.AccountCode AS GLGeneralInfoCode,
                                            VD.BudgetMasterId,
		                                    BUD.UserName AS Budget,
											A.UserName AS Activity,
											

                                            A.Id AS ActivityId
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
                                            WHERE CONVERT(DATE, v.PostingDate) BETWEEN CONVERT(DATE, '" + fromDate + "') AND CONVERT(DATE, '" + toDate + @"') AND SourceType!='OpeningBalance' AND v.CompanyId ='" + companyId + @"' 
                                            AND  V.IsPark=0
                                           GROUP BY GL.Id, GL.AccountCode, VDC.ParallelCurrencyId, CU.Code, VD.GLGeneralInfoId, GL.UserName, 
											GL.AccountCode, ACT.BalanceType, ACT.Id, VD.BudgetMasterId, A.UserName, BUD.UserName, v.PostingDate, A.Id
                                            UNION
                                                        SELECT DISTINCT GL.Id AS AccountCodeId, VDC.ParallelCurrencyId, CU.Code AS CurrencyCode, 0 OBDRcumulative,0 OBCRcumulative, 0 DRcumulative, 0 CRcumulative, 0 CBDRcumulative, 0 CBCRcumulative ,
															SUM(CASE WHEN ACT.BalanceType = 'Debit' THEN (sum(VDC.DrAmount) - sum(VDC.CrAmount)) ELSE 0 END) OVER (PARTITION BY GL.Id, VD.BudgetMasterId,A.Id, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId
			                                                ) AS FROBDRcumulative, sum(CASE WHEN ACT.BalanceType = 'Credit' THEN (sum(VDC.CrAmount) - sum(VDC.DrAmount)) ELSE 0 END) OVER (PARTITION BY GL.Id, VD.BudgetMasterId,A.Id, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId
			                                                ) AS FROBCRcumulative,0 PDRcumulative,0 PCRcumulative
															, ACT.BalanceType,
                                            ACT.Id AS [MainHead],
		                                    VD.GLGeneralInfoId,GL.UserName AS GL, GL.AccountCode AS GLGeneralInfoCode,
                                            VD.BudgetMasterId,
		                                    BUD.UserName AS Budget,
											A.UserName AS Activity,
											

                                            A.Id AS ActivityId
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
	                                               
                                                    WHERE V.PostingDate = '" + fromDate + @"' AND V.CompanyId = '" + companyId + @"'  AND v.IsPark = 0 and v.SourceType='OpeningBalance'

                                                
                                                GROUP BY GL.Id, GL.AccountCode, VDC.ParallelCurrencyId, CU.Code, VD.GLGeneralInfoId, GL.UserName, 
											GL.AccountCode, ACT.BalanceType, ACT.Id, VD.BudgetMasterId, A.UserName, BUD.UserName, v.PostingDate, A.Id, BA.AccountTitle, CM.UserName
											
											) TOTAL	
											GROUP BY AccountCodeId,ParallelCurrencyId,CurrencyCode,BalanceType,[MainHead],GLGeneralInfoId,GL,GLGeneralInfoCode,Budget
		                                    ,BudgetMasterId,Activity,ActivityId
                                            )ttd 
                                               WHERE ISNULL(DRcumulative,0.00) <> 0.00 OR ISNULL(CRcumulative,0) <> 0.00 OR
											ISNULL(OBDRcumulative,0.00) <> 0.00 OR ISNULL(OBCRcumulative,0) <> 0.00 OR
											ISNULL(CBDRcumulative,0.00) <> 0.00 OR ISNULL(CBCRcumulative,0) <> 0.00";

                    return _sqlRepository.GetGridData(parameters).Source;
                }
                else if (isBudgetLevel)
                {
                    parameters.CmdText = @" SELECT * FROM (SELECT AccountCodeId, ParallelCurrencyId, CurrencyCode, SuM(OBDRcumulative + FROBDRcumulative) OBDRcumulative, SUM(OBCRcumulative + FROBCRcumulative) OBCRcumulative
                                            , SUM(DRcumulative) DRcumulative, SUM(CRcumulative) CRcumulative
                                            , SUM(OBDRcumulative + DRcumulative+FROBDRcumulative) CBDRcumulative, SUm(OBCRcumulative + CRcumulative+FROBCRcumulative) CBCRcumulative
                                           , SUM(PDRcumulative) PDRcumulative, SUM(PCRcumulative) PCRcumulative
                                           , BalanceType, [MainHead],ISNULL(GLGeneralInfoId,'') GLGeneralInfoId, GL, GLGeneralInfoCode
                                           , ISNULL(BudgetMasterId,'') BudgetMasterId, Budget
                                                    FROM (
                                                    		SELECT DISTINCT GL.Id AS AccountCodeId, VDC.ParallelCurrencyId, CU.Code AS CurrencyCode, SUM(CASE WHEN ACT.BalanceType = 'Debit' THEN (sum(VDC.DrAmount) - sum(VDC.CrAmount)) ELSE 0 END) OVER (
                                                    			PARTITION BY GL.Id, VD.BudgetMasterId, VDC.ParallelCurrencyId ORDER BY VDC.ParallelCurrencyId
                                                    			) AS OBDRcumulative, SUM(CASE WHEN ACT.BalanceType = 'Credit' THEN (sum(VDC.CrAmount) - sum(VDC.DrAmount)) ELSE 0 END) OVER (
                                                    			PARTITION BY GL.Id, VD.BudgetMasterId, VDC.ParallelCurrencyId ORDER BY VDC.ParallelCurrencyId
                                                    			) AS OBCRcumulative, 0 DRcumulative, 0 CRcumulative, 0 CBDRcumulative, 0 CBCRcumulative,0 FROBDRcumulative,0 FROBCRcumulative,0 PDRcumulative,0 PCRcumulative, ACT.BalanceType, ACT.Id AS [MainHead], VD.GLGeneralInfoId, GL.UserName AS GL, GL.AccountCode AS GLGeneralInfoCode, VD.BudgetMasterId, BUD.UserName AS Budget
                                                    	FROM TRN.VoucherDetailCurrency AS VDC
                                                    	INNER JOIN TRN.VoucherDetail AS VD ON VD.Id = VDC.VoucherDetailId
                                                    	INNER JOIN TRN.Voucher AS V ON V.Id = VD.VoucherId
                                                    	LEFT JOIN HKP.GLGeneralInfo AS GL ON GL.Id = VD.GLGeneralInfoId
                                                    	LEFT JOIN HKP.AccountGroup AS AG ON AG.Id = GL.AccountGroupId
                                                    	LEFT JOIN [HKP].[AccountType] act ON act.Id = AG.AccountTypeId
                                                    	LEFT JOIN SCS.Currency AS CU ON CU.Id = VDC.ParallelCurrencyId
                                                    	LEFT JOIN MST.BudgetMaster BM ON VD.BudgetMasterId = BM.Id
                                                    	LEFT JOIN [HKP].[Budget] AS BUD ON BM.BudgetId = BUD.Id
                                                    	WHERE v.PostingDate < '" + fromDate + @"' AND v.CompanyId = '" + companyId + @"'  AND v.IsPark = 0 
                                                        AND VDC.VoucherDetailId NOT IN ( SELECT VD.Id FROM  TRN.VoucherDetail AS VD  
																INNER JOIN TRN.Voucher AS V ON V.Id=VD.VoucherId
																LEFT JOIN HKP.GLGeneralInfo AS GL ON GL.Id=VD.GLGeneralInfoId
																LEFT OUTER JOIN HKP.AccountGroup AS AG ON AG.Id=GL.AccountGroupId
																LEFT OUTER JOIN [HKP].[AccountType] act on act.Id =AG.AccountTypeId
																WHERE ACT.Id IN('Revenue','Expense') AND V.FiscalYearId in(select FiscalYearId from [SCS].[FiscalYearClose] ))
                                                    	GROUP BY GL.Id, GL.AccountCode, VDC.ParallelCurrencyId, CU.Code, VD.GLGeneralInfoId, GL.UserName, GL.AccountCode, ACT.BalanceType, ACT.Id, VD.BudgetMasterId, BUD.UserName, v.PostingDate
                                                    	
                                                    	UNION
                                                    	
                                                    	SELECT DISTINCT GL.Id AS AccountCodeId, VDC.ParallelCurrencyId, CU.Code AS CurrencyCode, 0 OBDRcumulative, 0 OBCRcumulative
                                                              , SUM(CASE WHEN ACT.BalanceType = 'Debit' THEN (sum(VDC.DrAmount) - sum(VDC.CrAmount)) ELSE 0 END) OVER (
			                                                PARTITION BY GL.Id, VD.BudgetMasterId, VDC.ParallelCurrencyId ORDER BY VDC.ParallelCurrencyId
			                                                ) AS DRcumulative, sum(CASE WHEN ACT.BalanceType = 'Credit' THEN (SUM(VDC.CrAmount) - SUM(VDC.DrAmount)) ELSE 0 END) OVER (
			                                               PARTITION BY GL.Id, VD.BudgetMasterId, VDC.ParallelCurrencyId ORDER BY VDC.ParallelCurrencyId
			                                                ) AS CRcumulative, 0 CBDRcumulative, 0 CBCRcumulative,0 FROBDRcumulative, 0 FROBCRcumulative
                                                          , SUM(CASE WHEN SUM(VDC.DrAmount)<>0 THEN (SUM(VDC.DrAmount)) 
																		 ELSE 0 END
															) OVER (
			                                               PARTITION BY GL.Id, VD.BudgetMasterId, VDC.ParallelCurrencyId ORDER BY VDC.ParallelCurrencyId
			                                                ) AS PDRcumulative
															
															, SUM(CASE WHEN SUM(VDC.CrAmount)<>0 THEN (SUM(VDC.CrAmount)) 
																		 ELSE 0 END
															) OVER (
			                                               PARTITION BY GL.Id, VD.BudgetMasterId, VDC.ParallelCurrencyId ORDER BY VDC.ParallelCurrencyId
			                                                ) AS PCRcumulative
														, ACT.BalanceType, ACT.Id AS [MainHead], VD.GLGeneralInfoId, GL.UserName AS GL, GL.AccountCode AS GLGeneralInfoCode, VD.BudgetMasterId, BUD.UserName AS Budget
                                                    	FROM TRN.VoucherDetailCurrency AS VDC
                                                    	INNER JOIN TRN.VoucherDetail AS VD ON VD.Id = VDC.VoucherDetailId
                                                    	INNER JOIN TRN.Voucher AS V ON V.Id = VD.VoucherId
                                                    	LEFT JOIN HKP.GLGeneralInfo AS GL ON GL.Id = VD.GLGeneralInfoId
                                                    	LEFT JOIN HKP.AccountGroup AS AG ON AG.Id = GL.AccountGroupId
                                                    	LEFT JOIN [HKP].[AccountType] act ON act.Id = AG.AccountTypeId
                                                    	LEFT JOIN SCS.Currency AS CU ON CU.Id = VDC.ParallelCurrencyId
                                                    	LEFT JOIN MST.BudgetMaster BM ON VD.BudgetMasterId = BM.Id
                                                    	LEFT JOIN [HKP].[Budget] AS BUD ON BM.BudgetId = BUD.Id
                                                    	WHERE Convert(DATE, v.PostingDate) BETWEEN Convert(DATE, '" + fromDate + @"') AND Convert(DATE, '" + toDate + @"') AND SourceType!='OpeningBalance' AND v.CompanyId = '" + companyId + @"'  AND v.IsPark = 0
                                                    	GROUP BY GL.Id, GL.AccountCode, VDC.ParallelCurrencyId, CU.Code, VD.GLGeneralInfoId, GL.UserName, GL.AccountCode, ACT.BalanceType, ACT.Id, VD.BudgetMasterId, BUD.UserName, v.PostingDate
                                                    	UNION
                                                        SELECT DISTINCT GL.Id AS AccountCodeId, VDC.ParallelCurrencyId, CU.Code AS CurrencyCode, 0 OBDRcumulative,0 OBCRcumulative, 0 DRcumulative, 0 CRcumulative, 0 CBDRcumulative, 0 CBCRcumulative ,
															SUM(CASE WHEN ACT.BalanceType = 'Debit' THEN (sum(VDC.DrAmount) - sum(VDC.CrAmount)) ELSE 0 END) OVER (
                                                    			PARTITION BY GL.Id, VD.BudgetMasterId, VDC.ParallelCurrencyId ORDER BY VDC.ParallelCurrencyId
                                                    			) AS FROBDRcumulative, SUM(CASE WHEN ACT.BalanceType = 'Credit' THEN (sum(VDC.CrAmount) - sum(VDC.DrAmount)) ELSE 0 END) OVER (
                                                    			PARTITION BY GL.Id, VD.BudgetMasterId, VDC.ParallelCurrencyId ORDER BY VDC.ParallelCurrencyId
                                                    			) AS FROBCRcumulative,0 PDRcumulative,0 PCRcumulative
															 ,ACT.BalanceType, ACT.Id AS [MainHead], VD.GLGeneralInfoId, GL.UserName AS GL, GL.AccountCode AS GLGeneralInfoCode, VD.BudgetMasterId, BUD.UserName AS Budget
	                                               FROM TRN.VoucherDetailCurrency AS VDC
                                                    	INNER JOIN TRN.VoucherDetail AS VD ON VD.Id = VDC.VoucherDetailId
                                                    	INNER JOIN TRN.Voucher AS V ON V.Id = VD.VoucherId
                                                    	LEFT JOIN HKP.GLGeneralInfo AS GL ON GL.Id = VD.GLGeneralInfoId
                                                    	LEFT JOIN HKP.AccountGroup AS AG ON AG.Id = GL.AccountGroupId
                                                    	LEFT JOIN [HKP].[AccountType] act ON act.Id = AG.AccountTypeId
                                                    	LEFT JOIN SCS.Currency AS CU ON CU.Id = VDC.ParallelCurrencyId
                                                    	LEFT JOIN MST.BudgetMaster BM ON VD.BudgetMasterId = BM.Id
                                                    	LEFT JOIN [HKP].[Budget] AS BUD ON BM.BudgetId = BUD.Id
                                                    	WHERE Convert(DATE, v.PostingDate) = Convert(DATE, '" + fromDate + @"') AND v.CompanyId = '" + companyId + @"' AND v.IsPark = 0  AND SourceType='OpeningBalance'
                                                    	GROUP BY GL.Id, GL.AccountCode, VDC.ParallelCurrencyId, CU.Code, VD.GLGeneralInfoId, GL.UserName, GL.AccountCode, ACT.BalanceType, ACT.Id, VD.BudgetMasterId, BUD.UserName, v.PostingDate
                                                    	
                                                        ) TOTAL
                                                    GROUP BY AccountCodeId, ParallelCurrencyId, CurrencyCode, BalanceType, [MainHead], GLGeneralInfoId, GL, GLGeneralInfoCode, BudgetMasterId, Budget)ttd 
                                            WHERE ISNULL(DRcumulative,0.00) <> 0.00 OR ISNULL(CRcumulative,0) <> 0.00 OR
											ISNULL(OBDRcumulative,0.00) <> 0.00 OR ISNULL(OBCRcumulative,0) <> 0.00 OR
											ISNULL(CBDRcumulative,0.00) <> 0.00 OR ISNULL(CBCRcumulative,0) <> 0.00";
                    return _sqlRepository.GetGridData(parameters).Source;
                }
                else if (isDetailLevel)
                {

                    parameters.CmdText = @"SELECT * FROM(SELECT  AccountCodeId,ParallelCurrencyId,CurrencyCode,
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
                                            WHERE v.PostingDate < '" + fromDate + @"' and v.CompanyId ='" + companyId + @"' AND  v.IsPark=0
                                            AND VDC.VoucherDetailId NOT IN ( SELECT VD.Id FROM  TRN.VoucherDetail AS VD  
																INNER JOIN TRN.Voucher AS V ON V.Id=VD.VoucherId
																LEFT JOIN HKP.GLGeneralInfo AS GL ON GL.Id=VD.GLGeneralInfoId
																LEFT OUTER JOIN HKP.AccountGroup AS AG ON AG.Id=GL.AccountGroupId
																LEFT OUTER JOIN [HKP].[AccountType] act on act.Id =AG.AccountTypeId
																WHERE ACT.Id IN('Revenue','Expense') AND V.FiscalYearId in(select FiscalYearId from [SCS].[FiscalYearClose] ))
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
                                            WHERE CONVERT(DATE, v.PostingDate) BETWEEN CONVERT(DATE, '" + fromDate + "') AND CONVERT(DATE, '" + toDate + @"') AND SourceType!='OpeningBalance' AND v.CompanyId ='" + companyId + @"' 
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
	                                               
                                                    WHERE V.PostingDate = '" + fromDate + @"' AND V.CompanyId = '" + companyId + @"'  AND v.IsPark = 0 and v.SourceType='OpeningBalance'

                                                GROUP BY GL.Id, GL.AccountCode, VDC.ParallelCurrencyId, CU.Code, VD.GLGeneralInfoId, GL.UserName, 
											GL.AccountCode, ACT.BalanceType, ACT.Id, VD.BudgetMasterId, A.UserName, BUD.UserName, v.PostingDate, A.Id, BA.AccountTitle, CM.UserName
											,VD.BankMasterId, VD.CashMasterId, P.UserName, PP.UserName, VD.PartyId, VD.PartyPlantId
											) TOTAL

											GROUP BY AccountCodeId,ParallelCurrencyId,CurrencyCode,BalanceType,[MainHead],GLGeneralInfoId,GL,GLGeneralInfoCode,Budget
		                                    ,BudgetMasterId,Activity,Particulars,ActivityId,BankMasterId,CashMasterId,PartyId,PartyPlantId
                                           
                                            )ttd 
                                            WHERE ISNULL(DRcumulative,0.00) <> 0.00 OR ISNULL(CRcumulative,0) <> 0.00 OR
											ISNULL(OBDRcumulative,0.00) <> 0.00 OR ISNULL(OBCRcumulative,0) <> 0.00 OR
											ISNULL(CBDRcumulative,0.00) <> 0.00 OR ISNULL(CBCRcumulative,0) <> 0.00";

                    return _sqlRepository.GetGridData(parameters).Source;

                }
                else
                {
                    parameters.CmdText = @" SELECT * FROM(SELECT AccountCodeId, ParallelCurrencyId, CurrencyCode, SuM(OBDRcumulative + FROBDRcumulative) OBDRcumulative, SUM(OBCRcumulative + FROBCRcumulative) OBCRcumulative
                            , SUM(DRcumulative) DRcumulative
                            , SUM(CRcumulative) CRcumulative, SUM(OBDRcumulative + DRcumulative+FROBDRcumulative) CBDRcumulative, SUm(OBCRcumulative + CRcumulative+FROBCRcumulative) CBCRcumulative
                            , SUM(PDRcumulative) PDRcumulative, SUM(PCRcumulative) PCRcumulative
												, BalanceType, [MainHead], ISNULL(GLGeneralInfoId,'') GLGeneralInfoId, ISNULL(GL,'') GL, ISNULL(GLGeneralInfoCode,'') GLGeneralInfoCode
                                                FROM (
	                                                SELECT DISTINCT GL.Id AS AccountCodeId, VDC.ParallelCurrencyId, CU.Code AS CurrencyCode, sum(CASE WHEN ACT.BalanceType = 'Debit' THEN (sum(VDC.DrAmount) - sum(VDC.CrAmount)) ELSE 0 END) OVER (
			                                                PARTITION BY GL.Id, VDC.ParallelCurrencyId ORDER BY VDC.ParallelCurrencyId
			                                                ) AS OBDRcumulative, sum(CASE WHEN ACT.BalanceType = 'Credit' THEN (sum(VDC.CrAmount) - sum(VDC.DrAmount)) ELSE 0 END) OVER (
			                                                PARTITION BY GL.Id, VDC.ParallelCurrencyId ORDER BY VDC.ParallelCurrencyId
			                                                ) AS OBCRcumulative, 0 DRcumulative, 0 CRcumulative, 0 CBDRcumulative, 0 CBCRcumulative,0 FROBDRcumulative, 0 FROBCRcumulative
															, 0 PDRcumulative, 0 PCRcumulative, ACT.BalanceType, ACT.Id AS [MainHead], VD.GLGeneralInfoId, GL.UserName AS GL, GL.AccountCode AS GLGeneralInfoCode
	                                                
													FROM TRN.VoucherDetailCurrency AS VDC
	                                                INNER JOIN TRN.VoucherDetail AS VD ON VD.Id = VDC.VoucherDetailId
	                                                INNER JOIN TRN.Voucher AS V ON V.Id = VD.VoucherId
	                                                LEFT JOIN HKP.GLGeneralInfo AS GL ON GL.Id = VD.GLGeneralInfoId
	                                                LEFT JOIN HKP.AccountGroup AS AG ON AG.Id = GL.AccountGroupId
	                                                LEFT JOIN [HKP].[AccountType] act ON act.Id = AG.AccountTypeId
	                                                LEFT JOIN SCS.Currency AS CU ON CU.Id = VDC.ParallelCurrencyId
	                                               
                                                    WHERE v.PostingDate < '" + fromDate + @"' AND v.CompanyId = '" + companyId + @"'  AND v.IsPark = 0
                                                    AND VDC.VoucherDetailId NOT IN ( SELECT VD.Id FROM  TRN.VoucherDetail AS VD  
																INNER JOIN TRN.Voucher AS V ON V.Id=VD.VoucherId
																LEFT JOIN HKP.GLGeneralInfo AS GL ON GL.Id=VD.GLGeneralInfoId
																LEFT OUTER JOIN HKP.AccountGroup AS AG ON AG.Id=GL.AccountGroupId
																LEFT OUTER JOIN [HKP].[AccountType] act on act.Id =AG.AccountTypeId
																WHERE ACT.Id IN('Revenue','Expense') AND V.FiscalYearId in(select FiscalYearId from [SCS].[FiscalYearClose] ))
	                                                GROUP BY GL.Id, GL.AccountCode, VDC.ParallelCurrencyId, CU.Code, VD.GLGeneralInfoId, GL.UserName, GL.AccountCode, ACT.BalanceType, ACT.Id, v.PostingDate
	
	                                                UNION
	
	                                                 SELECT DISTINCT GL.Id AS AccountCodeId, VDC.ParallelCurrencyId, CU.Code AS CurrencyCode, 0 OBDRcumulative, 0 OBcRcumulative, sum(CASE WHEN ACT.BalanceType = 'Debit' THEN (sum(VDC.DrAmount) - sum(VDC.CrAmount)) ELSE 0 END) OVER (
			                                                PARTITION BY GL.Id, VDC.ParallelCurrencyId ORDER BY VDC.ParallelCurrencyId
			                                                ) AS DRcumulative, sum(CASE WHEN ACT.BalanceType = 'Credit' THEN (SUM(VDC.CrAmount) - SUM(VDC.DrAmount)) ELSE 0 END) OVER (
			                                                PARTITION BY GL.Id, VDC.ParallelCurrencyId ORDER BY VDC.ParallelCurrencyId
			                                                ) AS CRcumulative, 0 CBDRcumulative, 0 CBCRcumulative,0 FROBDRcumulative, 0 FROBCRcumulative
                                                          , sum(CASE WHEN SUM(VDC.DrAmount)<>0 THEN (SUM(VDC.DrAmount)) 
																		 ELSE 0 END
															) OVER (
			                                                PARTITION BY GL.Id, VDC.ParallelCurrencyId ORDER BY VDC.ParallelCurrencyId
			                                                ) AS PDRcumulative
															
															, sum(CASE WHEN SUM(VDC.CrAmount)<>0 THEN (SUM(VDC.CrAmount)) 
																		 ELSE 0 END
															) OVER (
			                                                PARTITION BY GL.Id, VDC.ParallelCurrencyId ORDER BY VDC.ParallelCurrencyId
			                                                ) AS PCRcumulative


													, ACT.BalanceType, ACT.Id AS [MainHead], VD.GLGeneralInfoId, GL.UserName AS GL, GL.AccountCode AS GLGeneralInfoCode
	                                                FROM TRN.VoucherDetailCurrency AS VDC
	                                                INNER JOIN TRN.VoucherDetail AS VD ON VD.Id = VDC.VoucherDetailId
	                                                INNER JOIN TRN.Voucher AS V ON V.Id = VD.VoucherId
	                                                LEFT JOIN HKP.GLGeneralInfo AS GL ON GL.Id = VD.GLGeneralInfoId
	                                                LEFT JOIN HKP.AccountGroup AS AG ON AG.Id = GL.AccountGroupId
	                                                LEFT JOIN [HKP].[AccountType] act ON act.Id = AG.AccountTypeId
	                                                LEFT JOIN SCS.Currency AS CU ON CU.Id = VDC.ParallelCurrencyId
	                                                WHERE CONVERT(DATE, v.PostingDate) BETWEEN CONVERT(DATE, '" + fromDate + @"') AND CONVERT(DATE, '" + toDate + @"') AND SourceType!='OpeningBalance' AND v.CompanyId = '" + companyId + @"'  AND v.IsPark = 0
	                                                GROUP BY GL.Id, GL.AccountCode, VDC.ParallelCurrencyId, CU.Code, VD.GLGeneralInfoId, GL.UserName, GL.AccountCode, ACT.BalanceType, ACT.Id, v.PostingDate	
	                                                 
                                                    UNION

													 SELECT DISTINCT GL.Id AS AccountCodeId, VDC.ParallelCurrencyId, CU.Code AS CurrencyCode, 0 OBDRcumulative,0 OBCRcumulative, 0 DRcumulative, 0 CRcumulative, 0 CBDRcumulative, 0 CBCRcumulative ,
															sum(CASE WHEN ACT.BalanceType = 'Debit' THEN (sum(VDC.DrAmount) - sum(VDC.CrAmount)) ELSE 0 END) OVER (
			                                                PARTITION BY GL.Id, VDC.ParallelCurrencyId ORDER BY VDC.ParallelCurrencyId
			                                                ) AS FROBDRcumulative, sum(CASE WHEN ACT.BalanceType = 'Credit' THEN (sum(VDC.CrAmount) - sum(VDC.DrAmount)) ELSE 0 END) OVER (
			                                                PARTITION BY GL.Id, VDC.ParallelCurrencyId ORDER BY VDC.ParallelCurrencyId
			                                                ) AS FROBCRcumulative
															, 0 PDRcumulative, 0 PCRcumulative
															, ACT.BalanceType, ACT.Id AS [MainHead], VD.GLGeneralInfoId, GL.UserName AS GL, GL.AccountCode AS GLGeneralInfoCode
	                                                FROM TRN.VoucherDetailCurrency AS VDC
	                                                INNER JOIN TRN.VoucherDetail AS VD ON VD.Id = VDC.VoucherDetailId
	                                                INNER JOIN TRN.Voucher AS V ON V.Id = VD.VoucherId
	                                                LEFT JOIN HKP.GLGeneralInfo AS GL ON GL.Id = VD.GLGeneralInfoId
	                                                LEFT JOIN HKP.AccountGroup AS AG ON AG.Id = GL.AccountGroupId
	                                                LEFT JOIN [HKP].[AccountType] act ON act.Id = AG.AccountTypeId
	                                                LEFT JOIN SCS.Currency AS CU ON CU.Id = VDC.ParallelCurrencyId
	                                               
                                                    WHERE V.PostingDate = '" + fromDate + @"' AND V.CompanyId = '" + companyId + @"' AND v.IsPark = 0 and v.SourceType='OpeningBalance'
                                                GROUP BY GL.Id, GL.AccountCode, VDC.ParallelCurrencyId, CU.Code, VD.GLGeneralInfoId, GL.UserName, GL.AccountCode, ACT.BalanceType, ACT.Id, v.PostingDate
	    
                                                ) TOTAL
                                                GROUP BY AccountCodeId, ParallelCurrencyId, CurrencyCode, BalanceType, [MainHead], GLGeneralInfoId, GL, GLGeneralInfoCode )ttd
                                            WHERE ISNULL(DRcumulative, 0.00) <> 0.00 OR ISNULL(CRcumulative,0) <> 0.00 OR

                                            ISNULL(OBDRcumulative, 0.00) <> 0.00 OR ISNULL(OBCRcumulative,0) <> 0.00 OR

                                            ISNULL(CBDRcumulative, 0.00) <> 0.00 OR ISNULL(CBCRcumulative,0) <> 0.00";

                    return _sqlRepository.GetGridData(parameters).Source;
                }



            }
            catch (Exception)
            {
                throw;
            }
        }


    }
}
