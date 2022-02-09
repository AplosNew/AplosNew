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
                                            WHERE v.PostingDate <= '" + toDate + @"' and v.CompanyId ='" + companyId + @"' 
                                            AND  v.IsPark=0
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
                                            where v.PostingDate <= '" + toDate + @"' and v.CompanyId ='" + companyId + @"' 
                                            and  v.IsPark=0
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
                                            WHERE v.PostingDate <= '" + toDate + @"' and v.CompanyId ='" + companyId + @"' 
                                            AND  v.IsPark=0
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
                                            where v.PostingDate <= '" + toDate + @"' and v.CompanyId ='" + companyId + @"' 
                                            and  v.IsPark=0
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

    }
}
