using Library.Core;
using Library.Data;
using Library.Data.Sql;
using Library.Model.Currencies;
using Library.Model.Enums;
using Library.Model.Vouchers;
using Library.Service.Currencies;
using Library.Service.Enums;
using Library.Service.Helpers;
using Library.Service.Logs;
using Library.Service.Organizations;
using Library.Service.Properties;
using Library.ViewModel.OrderManagements;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Library.Accounting.Accounts
{
    public class AccountsInventoryPayableReportService
    {
        private readonly ISqlRepository _sqlRepository;

        public AccountsInventoryPayableReportService(ISqlRepository sqlRepository
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
        //testing 
        //private bool GetPlantIsShowFCInWord(string plantId)
        //{
        //   var IsShowFCInWord = @"SELECT IsShowFCInWord FROM ORG.Plant WHERE Id='"+ plantId + "'";
        //    return bool.Parse(IsShowFCInWord);
        //}

        private bool GetPlantIsShowFCInWord(string plantId)
        {
            return bplib.clsWebLib.GetBoolData(_sqlRepository.GetDataCollection(@"SELECT IsShowFCInWord FROM ORG.Plant WHERE Id='" + plantId + "'")[0]["IsShowFCInWord"].ToString());
        }

        private IEnumerable<InventoryReportViewModel> GetInventoryMaterialForImprestPayable(string companyId, string plantId, string inveReveiveId)
        {
            try
            {
                var sql = @"DECLARE @receiveId varchar(10)='" + inveReveiveId + @"', @companyId varchar(10)='" + companyId + @"', @plantId varchar(30)='" + plantId + @"', @countryId varchar(10)
                    SELECT  NULL OtherName, TrnType=Case when VD.DrAmount=0 then 'Cr' else 'Dr' End
							, NULL MaterialGroupMasterId, NULL TaxCategoryId,VD.GLGeneralInfoId
							, GL.AccountCode GLGeneralInfoCode
							,GL.UserName  GLGeneralInfoName
							,VD.BudgetMasterId
							,B.Code  BudgetCode
							,B.UserName  BudgetName
							,VD.ActivityId 
							,A.Code ActivityCode
							,A.UserName  ActivityName
							,VD.DrAmount Dr
							,vd.CrAmount Cr
                        FROM TRN.VoucherDetail VD 
                        LEFT JOIN TRN.VoucherDetailCurrency VDC ON VDC.VoucherDetailId=VD.Id
                        LEFT JOIN TRN.Voucher V ON V.Id=VD.VoucherId
                        LEFT JOIN TRN.EmployeePayable IV ON IV.VoucherId=V.Id
                        LEFT JOIN TRN.InventoryReceive IR ON IR.Id=IV.InventoryReceiveId
                        LEFT JOIN[HKP].[GLGeneralInfo] AS GL ON VD.GLGeneralInfoId=GL.Id
                        LEFT JOIN[MST].[BudgetMaster] AS BM ON VD.BudgetMasterId= BM.Id
                        LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
                        LEFT JOIN [HKP].[Activity] AS A ON VD.ActivityId= A.Id
                        where IV.InventoryReceiveId=@receiveId";
                return _sqlRepository.GetModelCollection<InventoryReportViewModel>(sql);
            }
            catch (CustomException)
            {
                throw;
            }
        }

        private IEnumerable<InventoryReportViewModel> GetInventoryMaterialWithoutReversChargePayable(string companyId, string plantId, string inveReveiveId)
        {
            try
            {
                var sql = @"SELECT  NULL OtherName, TrnType=Case when VD.DrAmount=0 then 'Cr' else 'Dr' End
							, NULL MaterialGroupMasterId, NULL TaxCategoryId,VD.GLGeneralInfoId
							, GL.AccountCode GLGeneralInfoCode
							,GL.UserName  GLGeneralInfoName
							,VD.BudgetMasterId
							,B.Code  BudgetCode
							,B.UserName  BudgetName
							,VD.ActivityId 
							,A.Code ActivityCode
							,A.UserName  ActivityName
							,VD.DrAmount Dr
							,vd.CrAmount Cr
                            ,V.Narration
                        FROM TRN.VoucherDetail VD 
                        LEFT JOIN TRN.VoucherDetailCurrency VDC ON VDC.VoucherDetailId=VD.Id
                        LEFT JOIN TRN.Voucher V ON V.Id=VD.VoucherId
                        LEFT JOIN TRN.InventoryReceive IR ON IR.VoucherId=V.Id
                        LEFT JOIN[HKP].[GLGeneralInfo] AS GL ON VD.GLGeneralInfoId=GL.Id
                        LEFT JOIN[MST].[BudgetMaster] AS BM ON VD.BudgetMasterId= BM.Id
                        LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
                        LEFT JOIN [HKP].[Activity] AS A ON VD.ActivityId= A.Id
                        WHERE IR.Id='" + inveReveiveId + @"'
                    --UNION
					--SELECT 'Svc' AS OtherName, 'Cr' AS TrnType, NULL AS MaterialGroupMasterId, IRTS.TaxCategoryId
					--	, TCGL.LiabilityGLId AS GLGeneralInfoId, GL.AccountCode AS GLGeneralInfoCode, GL.UserName AS GLGeneralInfoName
					--	, TCGL.LiabilityBudgetMasterId AS BudgetMasterId, B.Code AS BudgetCode, B.UserName AS BudgetName
					--	, TCGL.LiabilityActivityId AS ActivityId, A.Code AS ActivityCode, A.UserName AS ActivityName
					--	, NULL AS  Dr, SUM(IRTS.TaxAmount) Cr
					--	, SUM(IRTS.TaxAmount) AS Amount
					--FROM [TRN].[InventoryReceiveTax] AS IRTS
					--JOIN [TRN].[InventoryReceive] AS IR ON IRTS.InventoryReceiveId=IR.Id
					--JOIN [MST].[TaxCategoryGL] AS TCGL ON TCGL.TaxCategoryId=IRTS.TaxCategoryId
					--JOIN [MST].[TaxCategory] AS TC ON TCGL.TaxCategoryId=TC.Id AND IRTS.TaxCategoryId=TC.Id
					--LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON TCGL.GLGeneralInfoId=GL.Id
					--LEFT JOIN [MST].[BudgetMaster] AS BM ON TCGL.BudgetMasterId= BM.Id
					--LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
					--LEFT JOIN [HKP].[Activity] AS A ON TCGL.ActivityId= A.Id
					--WHERE IRTS.InventoryReceiveId=@receiveId AND IR.IsNonCreditable=0 AND IRTS.InventoryServiceId<>'' AND TCGL.InputTaxOutPutTax='Input' AND ISNULL(TCGL.TaxType,'')<>'RCM'
					--GROUP BY IRTS.TaxCategoryId, TCGL.LiabilityGLId, GL.AccountCode, GL.UserName, TCGL.LiabilityBudgetMasterId, B.Code, B.UserName, TCGL.LiabilityActivityId, A.Code, A.UserName";
                return _sqlRepository.GetModelCollection<InventoryReportViewModel>(sql);
            }
            catch (CustomException)
            {
                throw;
            }
        }
        private IEnumerable<InventoryReportViewModel> GetInventoryMaterialWithoutReversChargePayableFOC(string companyId, string plantId, string inveReveiveId)
        {
            try
            {
                var sql = @"SELECT  NULL OtherName, TrnType=Case when VD.PartyId<>'' then 'Cr' else 'Dr' End
							, NULL MaterialGroupMasterId, NULL TaxCategoryId,VD.GLGeneralInfoId
							, GL.AccountCode GLGeneralInfoCode
							,GL.UserName  GLGeneralInfoName
							,VD.BudgetMasterId
							,B.Code  BudgetCode
							,B.UserName  BudgetName
							,VD.ActivityId 
							,A.Code ActivityCode
							,A.UserName  ActivityName
							,VD.DrAmount Dr
							,vd.CrAmount Cr
                            ,V.Narration
                        FROM TRN.VoucherDetail VD 
                        LEFT JOIN TRN.VoucherDetailCurrency VDC ON VDC.VoucherDetailId=VD.Id
                        LEFT JOIN TRN.Voucher V ON V.Id=VD.VoucherId
                        LEFT JOIN TRN.InventoryReceive IR ON IR.VoucherId=V.Id
                        LEFT JOIN[HKP].[GLGeneralInfo] AS GL ON VD.GLGeneralInfoId=GL.Id
                        LEFT JOIN[MST].[BudgetMaster] AS BM ON VD.BudgetMasterId= BM.Id
                        LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
                        LEFT JOIN [HKP].[Activity] AS A ON VD.ActivityId= A.Id
                        WHERE IR.Id='" + inveReveiveId + @"'
                    ";
                return _sqlRepository.GetModelCollection<InventoryReportViewModel>(sql);
            }
            catch (CustomException)
            {
                throw;
            }
        }
        private IEnumerable<InventoryReportViewModel> GetInventoryMaterialReversChargePayable(string companyId, string plantId, string inveReveiveId)
        {
            try
            {
                var sql = @"DECLARE @receiveId varchar(10)='" + inveReveiveId + @"', @companyId varchar(10)='" + companyId + @"', @plantId varchar(30)='" + plantId + @"', @countryId varchar(10)
                    SELECT  NULL OtherName, TrnType=Case when VD.DrAmount=0 then 'Cr' else 'Dr' End
							, NULL MaterialGroupMasterId, NULL TaxCategoryId,VD.GLGeneralInfoId
							, GL.AccountCode GLGeneralInfoCode
							,GL.UserName  GLGeneralInfoName
							,VD.BudgetMasterId
							,B.Code  BudgetCode
							,B.UserName  BudgetName
							,VD.ActivityId 
							,A.Code ActivityCode
							,A.UserName  ActivityName
							,VD.DrAmount Dr
							,vd.CrAmount Cr
                            ,V.Narration
                        FROM TRN.VoucherDetail VD 
                        LEFT JOIN TRN.VoucherDetailCurrency VDC ON VDC.VoucherDetailId=VD.Id
                        LEFT JOIN TRN.Voucher V ON V.Id=VD.VoucherId
                        LEFT JOIN TRN.Invoice IV ON IV.VoucherId=V.Id
                        LEFT JOIN TRN.InventoryReceive IR ON IR.Id=IV.InventoryReceiveId
                        LEFT JOIN[HKP].[GLGeneralInfo] AS GL ON VD.GLGeneralInfoId=GL.Id
                        LEFT JOIN[MST].[BudgetMaster] AS BM ON VD.BudgetMasterId= BM.Id
                        LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
                        LEFT JOIN [HKP].[Activity] AS A ON VD.ActivityId= A.Id
                        WHERE IV.InventoryReceiveId=@receiveId order by VD.CrAmount";
                return _sqlRepository.GetModelCollection<InventoryReportViewModel>(sql);
            }
            catch (CustomException)
            {
                throw;
            }
        }

        private static void AssignSvcInTax(IEnumerable<InventoryReportViewModel> dataList, InventoryReportViewModel item, string trnType)
        {
            for (var i = 0; i < dataList.Count(); i++)
            {
                var row2 = dataList.ElementAt(i);
                if (row2.OtherName == "Tax" && row2.TrnType == trnType && row2.GLGeneralInfoId == item.GLGeneralInfoId
                    && row2.BudgetMasterId == item.BudgetMasterId && row2.ActivityId == item.ActivityId)
                {
                    if (trnType == "Dr")
                        dataList.ElementAt(i).Dr += item.Amount;
                    else
                        dataList.ElementAt(i).Cr += item.Amount;
                    dataList.ElementAt(i).Amount += item.Amount;
                }
            }
        }
        public IWorkbook PabyableJournal(string companyId, string plantId, string inventoryReceiveId, string employeeId, bool isReversCharge, bool isFoc, string sheetHeader)
        {
            try
            {
                var excelEngine = new ExcelEngine();
                var report = new ReportUtility();
                var workbook = report.GetWorkbook(ref excelEngine, 1);
                var sheet1 = workbook.Worksheets[0];
                GetInventoryMaterialPayableReportSheet(ref sheet1, report, sheetHeader, sheetHeader, companyId, plantId, inventoryReceiveId, employeeId, isReversCharge, isFoc);
                workbook.Version = ExcelVersion.Excel2013;
                return workbook;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private void GetInventoryMaterialPayableReportSheet(ref IWorksheet sheet, ReportUtility reportUtility, string sheetHeader, string sheetName
            , string companyId, string plantId, string inventoryReceiveId, string employeeId, bool isReversCharge, bool isFoc)
        {
            IEnumerable<InventoryReportViewModel> dataList;

            if (!string.IsNullOrEmpty(employeeId) && employeeId != "null")
                dataList = GetInventoryMaterialForImprestPayable(companyId, plantId, inventoryReceiveId);
            else
            {
                if (!isFoc)
                {
                    if (isReversCharge)
                        dataList = GetInventoryMaterialReversChargePayable(companyId, plantId, inventoryReceiveId);
                    else
                        dataList = GetInventoryMaterialWithoutReversChargePayable(companyId, plantId, inventoryReceiveId);
                }
                else
                {

                    dataList = GetInventoryMaterialWithoutReversChargePayableFOC(companyId, plantId, inventoryReceiveId);

                }
            }
            if (dataList.Count() == 0) throw new Exception("No Data Found!");

            var plantName = new DataView(_sqlRepository.GetDataTable(@"SELECT UserName from org.Plant WHERE Id='" + plantId + "'")).ToTable(true, "UserName").Rows[0]["UserName"].ToString();

            var sql = @"SELECT IR.PartyId, CONCAT(P.Code,'-', P.UserName) AS Vendor, IR.EmployeeId, EMP.EmployeeCode, EMP.EmployeeName
	                         , IR.DocDate, IR.IsNonCreditable, IR.AlongwithInvoice,IR.DocRefNo
							 ,InvoiceNo=CASE WHEN IR.EmployeeId<>'' THEN EP.EmployeePayableNo ELSE IV.InvoiceNo END
	                         , VoucherNo=CASE WHEN IR.EmployeeId<>'' THEN VEP.VoucherNo ELSE V.VoucherNo END
							 , VoucherDate=CASE WHEN IR.EmployeeId<>'' THEN VEP.VoucherDate ELSE V.VoucherDate END
							 , InvoiceDate=CASE WHEN IR.EmployeeId<>'' THEN REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') ELSE REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') END
							 , PostingDate=CASE WHEN IR.EmployeeId<>'' THEN  REPLACE(CONVERT(CHAR(11), VEP.PostingDate, 106),' ','-') ELSE REPLACE(CONVERT(CHAR(11), V.PostingDate, 106),' ','-') END

	                         , IR.BaseCurrencyId, BCU.Code AS BaseCurrency, IR.CurrencyId, TCU.Code AS  TranscationCurrency, IR.ToCurrencyRate
	                         , FiscalYearName=CASE WHEN IR.EmployeeId<>'' THEN FYEP.FiscalYearName ELSE FY.FiscalYearName END
	                         , PeriodNo=CASE WHEN IR.EmployeeId<>'' THEN FYPEP.PeriodNo ELSE FYP.PeriodNo END
							 , IR.AddedBy, IR.UpdatedBy, IR.Id GRNNo,REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNDate,IR.POId,IR.NoteforAccounts Narration
                        FROM [TRN].[InventoryReceive] AS IR
						LEFT JOIN [TRN].[Invoice] AS IV ON IV.InventoryReceiveId=IR.Id
						LEFT JOIN [TRN].[Voucher] AS V ON IR.VoucherId=V.Id
						LEFT JOIN [SCS].[FiscalYear] AS FY ON V.FiscalYearId=FY.Id
						LEFT  JOIN [SCS].[FiscalYearPeriod] AS FYP ON V.FiscalYearPeriodId=FYP.Id
						LEFT  JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                        LEFT JOIN [dbo].[EmployeeInformation] AS EMP ON IR.EmployeeId=EMP.SystemId
                        LEFT JOIN [SCS].[Currency] AS BCU ON IR.BaseCurrencyId=BCU.Id
                        LEFT JOIN [SCS].[Currency] AS TCU ON IR.CurrencyId=TCU.Id
						LEFT JOIN TRN.EmployeePayable EP ON EP.InventoryReceiveId=IR.Id
						LEFT JOIN [TRN].[Voucher] AS VEP ON EP.VoucherId=VEP.Id
						LEFT JOIN [SCS].[FiscalYear] AS FYEP ON VEP.FiscalYearId=FYEP.Id
						LEFT  JOIN [SCS].[FiscalYearPeriod] AS FYPEP ON VEP.FiscalYearPeriodId=FYPEP.Id
                        WHERE IR.Id='" + inventoryReceiveId + "'";
            var receiveList = _sqlRepository.GetData(sql);
            var isNonCreditable = Convert.ToBoolean(receiveList["IsNonCreditable"].ToString());
            var baseCurrency = receiveList["BaseCurrency"].ToString();
            var transcationCurrency = receiveList["TranscationCurrency"].ToString();
            var toCurrencyRate = Convert.ToDouble(receiveList["ToCurrencyRate"].ToString()) == 0 ? 1 : Convert.ToDouble(receiveList["ToCurrencyRate"].ToString());

            var newList = new List<InventoryReportViewModel>();

            if (!isNonCreditable)
            {
                var svcList = dataList.Where(t => t.OtherName == "Svc").ToList();
                foreach (var item in svcList)
                {
                    if (item.OtherName == "Svc" && item.TrnType == "Dr")
                    {
                        var taxList = dataList.Where(t => t.OtherName == "Tax" && t.TrnType == "Dr" && t.GLGeneralInfoId == item.GLGeneralInfoId
                                                && t.BudgetMasterId == item.BudgetMasterId && t.ActivityId == item.ActivityId).ToList();
                        item.Amount = Convert.ToDecimal(item.Amount) / Convert.ToDecimal(taxList.Count() == 0 ? 1 : taxList.Count());
                        AssignSvcInTax(dataList, item, "Dr");
                    }
                    else if (item.OtherName == "Svc" && item.TrnType == "Cr")
                    {
                        var taxList = dataList.Where(t => t.OtherName == "Tax" && t.TrnType == "Cr" && t.GLGeneralInfoId == item.GLGeneralInfoId && t.BudgetMasterId == item.BudgetMasterId && t.ActivityId == item.ActivityId).ToList();
                        item.Amount = Convert.ToDecimal(item.Amount) / Convert.ToDecimal(taxList.Count() == 0 ? 1 : taxList.Count());
                        AssignSvcInTax(dataList, item, "Cr");
                    }
                }
                foreach (var item in dataList)
                {
                    if (item.OtherName == "Tax" && item.TrnType == "Dr")
                    {
                        var flag = false;
                        for (var t = 0; t < newList.Count(); t++)
                        {
                            if (item.OtherName == newList[t].OtherName && item.TrnType == newList[t].TrnType
                                && item.GLGeneralInfoId == newList[t].GLGeneralInfoId
                                && item.BudgetMasterId == newList[t].BudgetMasterId
                                && item.ActivityId == newList[t].ActivityId)
                            {
                                newList[t].Dr += item.Dr;
                                flag = true;
                                break;
                            }
                        }
                        if (!flag)
                            newList.Add(item);
                    }
                    else if (item.OtherName == "Tax" && item.TrnType == "Cr")
                    {
                        var has = false;
                        for (var a = 0; a < newList.Count(); a++)
                        {
                            if (item.OtherName == newList[a].OtherName && item.TrnType == newList[a].TrnType
                                && item.GLGeneralInfoId == newList[a].GLGeneralInfoId
                                && item.BudgetMasterId == newList[a].BudgetMasterId
                                && item.ActivityId == newList[a].ActivityId)
                            {
                                newList[a].Dr += item.Dr;
                                has = true;
                                break;
                            }
                        }
                        if (!has)
                            newList.Add(item);
                    }
                    else if (item.OtherName != "Svc") newList.Add(item);
                }
            }
            else
            {
                var svcList = dataList.Where(t => t.OtherName == "Svc").ToList();
                foreach (var item in svcList)
                {
                    if (item.OtherName == "Svc" && item.TrnType == "Dr")
                    {
                        var taxList = dataList.Where(t => t.OtherName == "Tax" && t.TrnType == "Dr" && t.GLGeneralInfoId == item.GLGeneralInfoId
                                               && t.BudgetMasterId == item.BudgetMasterId && t.ActivityId == item.ActivityId).ToList();
                        item.Amount = Convert.ToDecimal(item.Amount) / Convert.ToDecimal(taxList.Count() == 0 ? 1 : taxList.Count());
                        AssignSvcInTax(dataList, item, "Dr");
                    }
                    else if (item.OtherName == "Svc" && item.TrnType == "Cr")
                    {
                        var taxList = dataList.Where(t => t.OtherName == "Tax" && t.TrnType == "Cr" && t.GLGeneralInfoId == item.GLGeneralInfoId && t.BudgetMasterId == item.BudgetMasterId && t.ActivityId == item.ActivityId).ToList();
                        item.Amount = Convert.ToDecimal(item.Amount) / Convert.ToDecimal(taxList.Count() == 0 ? 1 : taxList.Count());
                        AssignSvcInTax(dataList, item, "Cr");
                    }
                }
                foreach (var item in dataList)
                {
                    if (item.OtherName == "Material" && item.TrnType == "Dr")
                    {
                        var flag = false;
                        for (var t = 0; t < newList.Count(); t++)
                        {
                            if (newList[t].OtherName == "Material" && newList[t].TrnType == "Dr"
                                && item.MaterialGroupMasterId == newList[t].MaterialGroupMasterId)
                            {
                                newList[t].Dr += item.Dr;
                                flag = true;
                                break;
                            }
                        }
                        if (!flag)
                            newList.Add(item);
                    }
                    else if (item.OtherName != "Svc")
                        if (item.OtherName != "Material")
                            newList.Add(item);
                }
            }

            var shet2EndxlsCol = 1;

            #region Right header

            var _row = 5;

            reportUtility.SetMasterHeaderText(ref sheet, _row, 1, "Voucher No");
            reportUtility.SetText(ref sheet, _row, 2, receiveList["VoucherNo"].ToString());
            sheet.Range[_row, 2, _row, 3].Merge();
            _row++;

            reportUtility.SetMasterHeaderText(ref sheet, _row, 1, "GRN No");
            reportUtility.SetText(ref sheet, _row, 2, receiveList["GRNNo"].ToString());
            sheet.Range[_row, 2, _row, 3].Merge();
            _row++;

            reportUtility.SetMasterHeaderText(ref sheet, _row, 1, "PO No");
            reportUtility.SetText(ref sheet, _row, 2, receiveList["POId"].ToString());
            sheet.Range[_row, 2, _row, 3].Merge();
            _row++;

            reportUtility.SetMasterHeaderText(ref sheet, _row, 1, "Posting Date");
            reportUtility.SetText(ref sheet, _row, 2, receiveList["PostingDate"].ToString());
            sheet.Range[_row, 2, _row, 3].Merge();
            _row++;

            if (!string.IsNullOrEmpty(employeeId) && employeeId != "null")
            {
                reportUtility.SetMasterHeaderText(ref sheet, _row, 1, "Employee");
                reportUtility.SetText(ref sheet, _row, 2, receiveList["EmployeeCode"].ToString() + "-" + receiveList["EmployeeName"].ToString());
                sheet.Range[_row, 2, _row, 3].Merge();
                _row++;
            }
            reportUtility.SetMasterHeaderText(ref sheet, _row, 1, "Vendor");
            reportUtility.SetText(ref sheet, _row, 2, receiveList["Vendor"].ToString());
            sheet.Range[_row, 2, _row, 3].Merge();
            _row++;

            #endregion

            #region Left Header

            var _rowL = _row;
            var row = _row + 1;
            var _rowR = 5;

            reportUtility.SetMasterHeaderText(ref sheet, _rowR, 4, "Entry Date");
            reportUtility.SetText(ref sheet, _rowR, 5, receiveList["VoucherDate"].ToString());
            sheet.Range[_rowR, 5, _rowR, 8].Merge();
            _rowR++;

            reportUtility.SetMasterHeaderText(ref sheet, _rowR, 4, "GRN Date");
            reportUtility.SetText(ref sheet, _rowR, 5, receiveList["GRNDate"].ToString());
            sheet.Range[_rowR, 5, _rowR, 8].Merge();
            _rowR++;

            reportUtility.SetMasterHeaderText(ref sheet, _rowR, 4, "Invoice No");
            reportUtility.SetText(ref sheet, _rowR, 5, receiveList["InvoiceNo"].ToString());
            sheet.Range[_rowR, 5, _rowR, 8].Merge();
            _rowR++;

            reportUtility.SetMasterHeaderText(ref sheet, _rowR, 4, "FiscalYear");
            reportUtility.SetText(ref sheet, _rowR, 5, receiveList["FiscalYearName"].ToString() + "(" + receiveList["PeriodNo"].ToString() + ")");
            sheet.Range[_rowR, 5, _rowR, 8].Merge();
            _rowR++;

            #endregion

            #region Table

            var headreColIndex = 1;

            reportUtility.SetHeaderText(ref sheet, _rowL, headreColIndex, "GL", 24); headreColIndex++;
            reportUtility.SetHeaderText(ref sheet, _rowL, headreColIndex, "Budget", 24); headreColIndex++;
            reportUtility.SetHeaderText(ref sheet, _rowL, headreColIndex, "Activity", 24); headreColIndex++;

            if (baseCurrency != transcationCurrency)
            {
                reportUtility.SetHeaderText(ref sheet, _rowL - 1, headreColIndex, transcationCurrency, 24, ExcelHAlign.HAlignCenter);
                sheet[_rowL - 1, headreColIndex, _rowL - 1, headreColIndex + 1].Merge();
                reportUtility.SetHeaderText(ref sheet, _rowL, headreColIndex, "Debit", 24, ExcelHAlign.HAlignRight);
                headreColIndex++;
                reportUtility.SetHeaderText(ref sheet, _rowL, headreColIndex, "Credit", 24, ExcelHAlign.HAlignRight);
                headreColIndex++;
            }

            reportUtility.SetHeaderText(ref sheet, _rowL - 1, headreColIndex, baseCurrency, ExcelHAlign.HAlignCenter);
            sheet[_rowL - 1, headreColIndex, _rowL - 1, headreColIndex + 1].Merge();

            reportUtility.SetHeaderText(ref sheet, _rowL, headreColIndex, "Debit", 24, ExcelHAlign.HAlignRight);
            headreColIndex++;
            reportUtility.SetHeaderText(ref sheet, _rowL, headreColIndex, "Credit", 24, ExcelHAlign.HAlignRight);
            headreColIndex++;


            shet2EndxlsCol = headreColIndex;
            var Row_Total_Start = _rowL + 1;
            double trnCurrencyAmount = 0;
            double baseCurrencyAmount = 0;

            foreach (var item in newList)
            {
                _rowL++;
                reportUtility.SetText(ref sheet, _rowL, 1, item.GLGeneralInfoCode + '-' + item.GLGeneralInfoName);
                reportUtility.SetText(ref sheet, _rowL, 2, item.BudgetName);
                reportUtility.SetText(ref sheet, _rowL, 3, item.ActivityName);

                if (baseCurrency != transcationCurrency)
                {
                    reportUtility.SetText(ref sheet, _rowL, 4, Convert.ToDouble(item.Dr));
                    reportUtility.SetText(ref sheet, _rowL, 5, Convert.ToDouble(item.Cr));
                    reportUtility.SetText(ref sheet, _rowL, 6, Convert.ToDouble(item.Dr) * toCurrencyRate);
                    reportUtility.SetText(ref sheet, _rowL, 7, Convert.ToDouble(item.Cr) * toCurrencyRate);
                    trnCurrencyAmount += Convert.ToDouble(item.Dr);
                    baseCurrencyAmount += Convert.ToDouble(item.Dr) * toCurrencyRate;
                }
                else
                {
                    reportUtility.SetText(ref sheet, _rowL, 4, Convert.ToDouble(item.Dr));
                    reportUtility.SetText(ref sheet, _rowL, 5, Convert.ToDouble(item.Cr));
                    baseCurrencyAmount += Convert.ToDouble(item.Dr);
                }

            }

            _rowL++;
            sheet.Range[_rowL, 1, _rowL, 3].Merge();
            reportUtility.SetText(ref sheet, _rowL, 1, null, false);

            if (baseCurrency != transcationCurrency)
            {
                sheet.Range[_rowL, 4].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(4) + Row_Total_Start + ":" + reportUtility.GetColumnNameForXls(4) + (_rowL - 1) + ")";
                sheet.Range[_rowL, 4].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                sheet.Range[_rowL, 4].CellStyle.Font.Bold = true;
                sheet.Range[_rowL, 4].BorderAround(ExcelLineStyle.Hair);

                sheet.Range[_rowL, 5].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(5) + Row_Total_Start + ":" + reportUtility.GetColumnNameForXls(5) + (_rowL - 1) + ")";
                sheet.Range[_rowL, 5].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                sheet.Range[_rowL, 5].CellStyle.Font.Bold = true;
                sheet.Range[_rowL, 5].BorderAround(ExcelLineStyle.Hair);

                sheet.Range[_rowL, 6].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(6) + Row_Total_Start + ":" + reportUtility.GetColumnNameForXls(6) + (_rowL - 1) + ")";
                sheet.Range[_rowL, 6].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                sheet.Range[_rowL, 6].CellStyle.Font.Bold = true;
                sheet.Range[_rowL, 6].BorderAround(ExcelLineStyle.Hair);

                sheet.Range[_rowL, 7].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(7) + Row_Total_Start + ":" + reportUtility.GetColumnNameForXls(7) + (_rowL - 1) + ")";
                sheet.Range[_rowL, 7].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                sheet.Range[_rowL, 7].CellStyle.Font.Bold = true;
                sheet.Range[_rowL, 7].BorderAround(ExcelLineStyle.Hair);
            }
            else
            {
                sheet.Range[_rowL, 4].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(4) + Row_Total_Start + ":" + reportUtility.GetColumnNameForXls(4) + (_rowL - 1) + ")";
                sheet.Range[_rowL, 4].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                sheet.Range[_rowL, 4].CellStyle.Font.Bold = true;
                sheet.Range[_rowL, 4].BorderAround(ExcelLineStyle.Hair);

                sheet.Range[_rowL, 5].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(5) + Row_Total_Start + ":" + reportUtility.GetColumnNameForXls(5) + (_rowL - 1) + ")";
                sheet.Range[_rowL, 5].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                sheet.Range[_rowL, 5].CellStyle.Font.Bold = true;
                sheet.Range[_rowL, 5].BorderAround(ExcelLineStyle.Hair);
            }

            #endregion

            sheet.Range[(row), 1, _rowL, shet2EndxlsCol - 1].BorderInside(ExcelLineStyle.Hair);
            sheet.Range[(row), 1, _rowL, shet2EndxlsCol - 1].BorderAround(ExcelLineStyle.Hair);

            _rowL++;
            var _col = 2;
            reportUtility.SetText(ref sheet, _rowL, 1, "In Word:", true);
            if (baseCurrency != transcationCurrency)
            {
                var _amountValue = reportUtility.InWord(trnCurrencyAmount, receiveList["CurrencyId"].ToString());
                sheet.Range[reportUtility.GetColumnNameForXls(_col) + _rowL].Text = _amountValue;
                sheet.Range[reportUtility.GetColumnNameForXls(_col) + _rowL + ":" + reportUtility.GetColumnNameForXls(shet2EndxlsCol) + _rowL].Merge();
                sheet.Range[reportUtility.GetColumnNameForXls(_col) + _rowL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[reportUtility.GetColumnNameForXls(_col) + _rowL].VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[reportUtility.GetColumnNameForXls(_col) + _rowL].CellStyle.Font.Bold = true;
                _rowL++;
            }

            var _amount = reportUtility.InWord(baseCurrencyAmount, receiveList["BaseCurrencyId"].ToString());
            sheet.Range[reportUtility.GetColumnNameForXls(_col) + _rowL].Text = _amount;
            sheet.Range[reportUtility.GetColumnNameForXls(_col) + _rowL + ":" + reportUtility.GetColumnNameForXls(shet2EndxlsCol) + _rowL].Merge();
            sheet.Range[reportUtility.GetColumnNameForXls(_col) + _rowL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            sheet.Range[reportUtility.GetColumnNameForXls(_col) + _rowL].VerticalAlignment = ExcelVAlign.VAlignTop;
            sheet.Range[reportUtility.GetColumnNameForXls(_col) + _rowL].CellStyle.Font.Bold = true;

            #region Signature

            _rowL = _rowL + 4;

            sheet.Range[_rowL, 1].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
            sheet.Range[_rowL, 3].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
            sheet.Range[_rowL, 5].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;

            reportUtility.SetText(ref sheet, _rowL - 1, 1, receiveList["UpdatedBy"].ToString(), false);
            reportUtility.SetText(ref sheet, _rowL, 1, "Prepared By", true);
            reportUtility.SetText(ref sheet, _rowL, 3, "Checked By", true);
            reportUtility.SetText(ref sheet, _rowL, 5, "Authorized By", true);

            #endregion Signature

            sheet.Name = sheetName;
            sheet.UsedRange.WrapText = true;
            sheet.UsedRange.CellStyle.Font.Size = 8;
            reportUtility.CompanyPlantHeader(ref sheet, shet2EndxlsCol, sheetHeader, companyId, plantId, plantName, null);
            reportUtility.PageSetup(ref sheet, 5, ExcelPageOrientation.Landscape);

        }

        public IWorkbook ServicePabyableJournal(string companyGroupId, string companyId, string plantId, string plantName, string serviceAcknowledmentId, string voucherId, bool isReversCharge, string sheetHeader)
        {
            try
            {
                var excelEngine = new ExcelEngine();
                var report = new ReportUtility();
                var workbook = report.GetWorkbook(ref excelEngine, 1);
                var sheet1 = workbook.Worksheets[0];

                GetServicePayableReportSheet(out string reportFileName, companyGroupId, companyId, plantId, plantName, voucherId);
                workbook.Version = ExcelVersion.Excel2013;
                return workbook;
            }
            catch (Exception)
            {
                throw;
            }
        }
        private Dictionary<string, object> GetServicePaybalePostingHeader(string companyGroupId, string companyId, string plantId, string voucherId, SourceType sourceType)
        {
            var cmdText = @"SELECT VT.UserName AS VoucherTypeName, V.VoucherNo, REPLACE(CONVERT(VARCHAR(11), V.VoucherDate, 106), ' ', '-') AS VoucherDate, REPLACE(CONVERT(VARCHAR(11), V.PostingDate, 106), ' ', '-') AS PostingDate
            , REPLACE(CONVERT(VARCHAR(11), V.DocDate, 106), ' ', '-') AS DocDate, V.DocRefNo, V.AddedBy, V.PostedBy, UPPER(V.Narration) AS Narration, CASE WHEN V.IsPark=1 THEN 'Parked' ELSE 'Posted' END AS [Status]
            , P.UserName AS Vendor, PP.UserName AS VendorPlant, IV.CurrencyId, C.Code AS CurrencyCode,IV.ServiceAcknowledgementMasterId ServiceAcknoledgeNo
            FROM TRN.ServiceAcknowledgementMaster SAM
            LEFT JOIN [TRN].[Invoice] AS IV ON SAM.Id=IV.ServiceAcknowledgementMasterId
            LEFT JOIN [TRN].[Voucher] AS V ON V.Id=SAM.VoucherId
            LEFT JOIN [SCS].[VoucherType] AS VT ON VT.Id=V.VoucherTypeId
            LEFT JOIN [HKP].[Party] AS P ON P.Id=SAM.PartyId
            LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=SAM.InvoicingPartyPlantId
            LEFT JOIN [SCS].[Currency] AS C ON C.Id=V.CurrencyId
            WHERE  V.CompanyGroupId='" + companyGroupId + "' AND V.CompanyId='" + companyId + "' AND V.PlantId='" + plantId + @"'
            AND SAM.VoucherId='" + voucherId + @"'
            AND V.SourceType='" + sourceType + "'";
            return _sqlRepository.GetData(cmdText);
        }
        private DataTable GetServicePaybalePostingInvoiceVoucher(string voucherId)
        {
            try
            {
                var sql = @"SELECT V.Id, GL.Id AS AccountCodeId, VDC.VoucherDetailId, FY.FiscalYearName, FYP.PeriodName, FYP.PeriodNo, V.IsPark, REPLACE(CONVERT(VARCHAR(11), V.PostingDate, 106), ' ', '-') AS PostingDate
                        , [Park/Post]=CASE WHEN V.IsPark=1 THEN 'Parked' ELSE 'Posted' END, REPLACE(CONVERT(VARCHAR(11), v.DocDate, 106), ' ', '-') AS DocDate, V.DocRefNo, V.VoucherNo, UPPER(V.Narration) AS Narration
                        , V.CurrencyId, REPLACE(CONVERT(VARCHAR(11), V.VoucherDate, 106), ' ', '-') AS VoucherDate, CU1.Code AS TrnCurrency, V.AddedBy, V.PostedBy, VDC.ParallelCurrencyId, CU.Code AS CurrencyCode
                        , VDC.FromCurrencyId, VDC.ToCurrencyId, VDC.ToCurrencyRate, VD.DrAmount AS DrAmount, VD.CrAmount AS CrAmount, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount, [DRCR]=CASE WHEN VDC.DrAmount>0 THEN '1' ELSE '2' END
                        , VD.GLGeneralInfoId, GL.UserName AS GL, GL.AccountCode AS GLGeneralInfoCode, VD.Narration AS DetailNarration, BUD.UserName AS Budget
                        , ACT.UserName AS Activity
                        FROM [TRN].[VoucherDetailCurrency] AS VDC
                        JOIN [TRN].[VoucherDetail] AS VD ON VD.Id=VDC.VoucherDetailId
                        JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                        LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON GL.Id=VD.GLGeneralInfoId
                        LEFT JOIN [SCS].[Currency] AS CU ON CU.Id=VDC.ParallelCurrencyId
                        LEFT JOIN [SCS].[Currency] AS CU1 ON CU1.Id=V.CurrencyId
                        LEFT JOIN [SCS].[FiscalYear] AS FY ON FY.Id=V.FiscalYearId
                        LEFT JOIN [SCS].[FiscalYearPeriod] AS FYP ON FYP.Id=V.FiscalYearPeriodId
                        LEFT JOIN [MST].[BudgetMaster] BUM ON VD.BudgetMasterId=BUM.Id
                        LEFT JOIN [HKP].[Budget] AS BUD ON BUD.Id=BUM.BudgetId
                        LEFT JOIN [HKP].[Activity] AS ACT ON ACT.Id=VD.ActivityId
                        WHERE V.Archive=0 AND V.Id='" + voucherId + @"' ORDER BY VD.DrAmount DESC";
                return _sqlRepository.GetDataTable(sql);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IWorkbook GetServicePayableReportSheet(out string reportFileName, string companyGroupId, string companyId, string plantId, string plantName, string voucherId)
        {
            var reportUtility = new ReportUtility();
            var excelEngine = new ExcelEngine();
            var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
            workbook.Version = ExcelVersion.Excel2016;
            var sheet = workbook.Worksheets[0];
            sheet.Name = "Voucher";

            var header = GetServicePaybalePostingHeader(companyGroupId, companyId, plantId, voucherId, SourceType.ServicePayable); //

            reportFileName = Convert.ToDateTime(header["PostingDate"]).ToString("yyMMdd") + " " + header["VoucherNo"];

            var dsLocal = GetServicePaybalePostingInvoiceVoucher(voucherId);

            var transcationCurrency = header["CurrencyId"].ToString();
            GetParallelCurrency(companyId, out string companyCurrencyId, out string companyCurrencyCode);

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
            reportUtility.SetMasterHeaderText(ref sheet, row, 3, "Voucher Date");
            reportUtility.SetText(ref sheet, row, 4, header["VoucherDate"].ToString());
            row++;

            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Posting Date");
            reportUtility.SetText(ref sheet, row, 2, header["PostingDate"].ToString());
            reportUtility.SetMasterHeaderText(ref sheet, row, 3, "DocDate");
            reportUtility.SetText(ref sheet, row, 4, header["DocDate"].ToString());
            row++;

            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Vendor");
            reportUtility.SetText(ref sheet, row, 2, header["Vendor"].ToString());
            reportUtility.SetMasterHeaderText(ref sheet, row, 3, "Doc Ref");
            reportUtility.SetText(ref sheet, row, 4, header["DocRefNo"].ToString());
            row++;

            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Vendor Plant");
            reportUtility.SetText(ref sheet, row, 2, header["VendorPlant"].ToString());
            reportUtility.SetMasterHeaderText(ref sheet, row, 3, "Status");
            reportUtility.SetText(ref sheet, row, 4, header["Status"].ToString());

            row++;

            colLast = companyCurrencyId == transcationCurrency ? 5 : 7;
            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Narration");
            reportUtility.SetText(ref sheet, row, 2, header["Narration"].ToString());
            sheet[reportUtility.GetColumnNameForXls(2) + row + ":" + reportUtility.GetColumnNameForXls(colLast) + row].Merge();
            //sheet[1, 2].ColumnWidth = 100;

            row++;

            if (companyCurrencyId == transcationCurrency)
            {
                reportUtility.SetHeaderText(ref sheet, row, 3, companyCurrencyCode, ExcelHAlign.HAlignCenter);
                sheet[row, 3, row, 4].Merge();
            }
            else
            {
                reportUtility.SetHeaderText(ref sheet, row, 3, header["CurrencyCode"].ToString(), ExcelHAlign.HAlignCenter);
                sheet[row, 3, row, 4].Merge();

                reportUtility.SetHeaderText(ref sheet, row, 5, companyCurrencyCode, ExcelHAlign.HAlignCenter);
                sheet[row, 5, row, 6].Merge();
            }

            row++;

            reportUtility.SetHeaderText(ref sheet, row, xlsCol, "GL"); colGl = xlsCol; xlsCol++;
            sheet[reportUtility.GetColumnNameForXls(colGl) + row + ":" + reportUtility.GetColumnNameForXls(2) + row].Merge(); xlsCol++;

            if (companyCurrencyId != transcationCurrency)
            {
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Debit", 13, ExcelHAlign.HAlignRight); colinrDebit = xlsCol; xlsCol++;
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Credit", 13, ExcelHAlign.HAlignRight); colinrCredit = xlsCol; xlsCol++;

                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Debit", 13, ExcelHAlign.HAlignRight); colusdDebit = xlsCol; xlsCol++;
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Credit", 13, ExcelHAlign.HAlignRight); colusdCradit = xlsCol;
                colLast = xlsCol;
            }
            else
            {
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Debit", 14, ExcelHAlign.HAlignRight); colinrDebit = xlsCol; xlsCol++;
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Credit", 14, ExcelHAlign.HAlignRight); colinrCredit = xlsCol;
                colLast = xlsCol;
            }

            if (dsLocal.Rows.Count > 0)
            {
                double totalTranAmount = 0;
                double totalBookCurrencyAmount = 0;
                row++;
                for (int i = 0; i < dsLocal.Rows.Count; i++)
                {
                    var glName = dsLocal.Rows[i]["Budget"].ToString();


                    reportUtility.SetText(ref sheet, row, colGl, dsLocal.Rows[i]["GLGeneralInfoCode"] + " - " + glName + " - " + dsLocal.Rows[i]["Activity"]);

                    sheet[reportUtility.GetColumnNameForXls(colGl) + row + ":" + reportUtility.GetColumnNameForXls(2) + row].Merge();

                    if (companyCurrencyId != transcationCurrency)
                    {
                        reportUtility.SetText(ref sheet, row, colinrDebit, Convert.ToDouble(dsLocal.Rows[i]["DrAmount"].ToString()));
                        reportUtility.SetText(ref sheet, row, colinrCredit, Convert.ToDouble(dsLocal.Rows[i]["CrAmount"].ToString()));
                        reportUtility.SetText(ref sheet, row, colusdDebit, Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyDrAmount"].ToString()));
                        reportUtility.SetText(ref sheet, row, colusdCradit, Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyCrAmount"].ToString()));
                        totalTranAmount += Convert.ToDouble(dsLocal.Rows[i]["DrAmount"].ToString());
                    }
                    else
                    {
                        reportUtility.SetText(ref sheet, row, colinrDebit, Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyDrAmount"].ToString()));
                        reportUtility.SetText(ref sheet, row, colinrCredit, Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyCrAmount"].ToString()));
                    }
                    totalBookCurrencyAmount += Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyDrAmount"].ToString());

                    sheet.Range[row, 1, row, colLast].BorderInside(ExcelLineStyle.Hair);
                    sheet.Range[row, 1, row, colLast].BorderAround(ExcelLineStyle.Hair);
                    row++;

                    glName = string.Empty;
                }

                reportUtility.SetText(ref sheet, row, 2, "Total: ", true);

                if (companyCurrencyId != transcationCurrency)
                {
                    sheet.Range[row, colinrDebit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(3) + 12 + ":" + reportUtility.GetColumnNameForXls(3) + (row - 1) + ")";
                    sheet.Range[row, colinrDebit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colinrDebit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colinrDebit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colinrDebit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colinrDebit].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, colinrCredit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(4) + 12 + ":" + reportUtility.GetColumnNameForXls(4) + (row - 1) + ")";
                    sheet.Range[row, colinrCredit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colinrCredit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colinrCredit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colinrCredit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colinrCredit].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, colusdDebit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(5) + 12 + ":" + reportUtility.GetColumnNameForXls(5) + (row - 1) + ")";
                    sheet.Range[row, colusdDebit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colusdDebit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colusdDebit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colusdDebit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colusdDebit].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, colusdCradit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(6) + 12 + ":" + reportUtility.GetColumnNameForXls(6) + (row - 1) + ")";
                    sheet.Range[row, colusdCradit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colusdCradit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colusdCradit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colusdCradit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colusdCradit].BorderAround(ExcelLineStyle.Hair);
                }
                else
                {
                    sheet.Range[row, colinrDebit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(3) + 12 + ":" + reportUtility.GetColumnNameForXls(3) + (row - 1) + ")";
                    sheet.Range[row, colinrDebit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colinrDebit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colinrDebit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colinrDebit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colinrDebit].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, colinrCredit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(4) + 12 + ":" + reportUtility.GetColumnNameForXls(4) + (row - 1) + ")";
                    sheet.Range[row, colinrCredit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colinrCredit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colinrCredit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colinrCredit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colinrCredit].BorderAround(ExcelLineStyle.Hair);
                }

                sheet.Range[13, 1, row - 1, colLast].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[13, 1, row - 1, colLast].BorderAround(ExcelLineStyle.Hair);

                row += 2;
                reportUtility.SetText(ref sheet, row, 1, "In Word:", true);

                if (companyCurrencyId != transcationCurrency && GetPlantIsShowFCInWord(plantId))
                {
                    sheet.Range[reportUtility.GetColumnNameForXls(2) + row].Text = reportUtility.InWord(totalTranAmount, transcationCurrency);
                    sheet.Range[reportUtility.GetColumnNameForXls(2) + row + ":" + reportUtility.GetColumnNameForXls(colLast) + row].Merge();
                    sheet.Range[reportUtility.GetColumnNameForXls(2) + row].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet.Range[reportUtility.GetColumnNameForXls(2) + row].VerticalAlignment = ExcelVAlign.VAlignTop;
                    sheet.Range[reportUtility.GetColumnNameForXls(2) + row].CellStyle.Font.Bold = true;
                    row++;
                }

                sheet.Range[reportUtility.GetColumnNameForXls(2) + row].Text = reportUtility.InWord(totalBookCurrencyAmount, companyCurrencyId);
                sheet.Range[reportUtility.GetColumnNameForXls(2) + row + ":" + reportUtility.GetColumnNameForXls(colLast) + row].Merge();
                sheet.Range[reportUtility.GetColumnNameForXls(2) + row].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[reportUtility.GetColumnNameForXls(2) + row].VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[reportUtility.GetColumnNameForXls(2) + row].CellStyle.Font.Bold = true;

                sheet.UsedRange.AutofitColumns();
                sheet[1, 2].ColumnWidth = 60;
                sheet.UsedRange.CellStyle.Font.Size = 8;
                row += 4;
                reportUtility.SetSignatureText(ref sheet, row - 1, 1, header["AddedBy"].ToString());
                sheet.Range[row, 1].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                reportUtility.SetTextMiddle(ref sheet, row, 1, "Prepared By", true);

                reportUtility.SetSignatureText(ref sheet, row - 1, 2, header["PostedBy"].ToString());
                sheet.Range[row, 2].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                reportUtility.SetTextMiddle(ref sheet, row, 2, "Checked By", true);

                sheet.Range[row, 4].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                reportUtility.SetTextMiddle(ref sheet, row, 4, "Authorized By", true);

                reportUtility.CompanyPlantHeader(ref sheet, colLast, "Service Payable", companyId, plantId, plantName, null);
                reportUtility.PageSetup(ref sheet, colLast, ExcelPageOrientation.Portrait);
            }
            else
            {
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.CellStyle.Font.Size = 8;
                reportUtility.CompanyPlantHeader(ref sheet, 5, "Service Payable", companyId, plantId, plantName, null);
                reportUtility.PageSetup(ref sheet, 5, ExcelPageOrientation.Portrait);
            }
            return workbook;
        }

        #region IssueJournal

        public IWorkbook IssueJournal(string reportFileName, string companyGroupId, string companyId, string plantId, string plantName, string issueId)
        {
            var reportUtility = new ReportUtility();
            var excelEngine = new ExcelEngine();
            var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
            workbook.Version = ExcelVersion.Excel2016;
            var sheet = workbook.Worksheets[0];
            sheet.Name = "Voucher";

            var header = GetIssueJournalHeader(companyGroupId, companyId, plantId, issueId, SourceType.IssueJournal);

            reportFileName = Convert.ToDateTime(header["PostingDate"]).ToString("yyMMdd") + " " + header["VoucherNo"];

            var dsLocal = InventoryIssueData(companyGroupId, companyId, plantId, issueId, SourceType.IssueJournal);

            var transcationCurrency = header["CurrencyId"].ToString();
            GetParallelCurrency(companyId, out string companyCurrencyId, out string companyCurrencyCode);

            var row = 5;

            var colLast = 1;

            int xlsCol = 1;
            int colGl = 0;
            int colParticulars = 0;
            int colinrDebit = 0;
            int colinrCredit = 0;
            int colusdDebit = 0;
            int colusdCradit = 0;

            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Voucher No");
            reportUtility.SetText(ref sheet, row, 2, header["VoucherNo"].ToString(), ExcelHAlign.HAlignLeft);
            reportUtility.SetMasterHeaderText(ref sheet, row, 4, "Issue No");
            reportUtility.SetText(ref sheet, row, 5, header["IssueNo"].ToString(), ExcelHAlign.HAlignLeft);

            sheet[reportUtility.GetColumnNameForXls(2) + row + ":" + reportUtility.GetColumnNameForXls(3) + row].Merge();

            row++;

            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Posting Date");
            reportUtility.SetText(ref sheet, row, 2, header["PostingDate"].ToString(), ExcelHAlign.HAlignLeft);
            reportUtility.SetMasterHeaderText(ref sheet, row, 4, "Entry Date");
            reportUtility.SetText(ref sheet, row, 5, header["VoucherDate"].ToString(), ExcelHAlign.HAlignLeft);


            sheet[reportUtility.GetColumnNameForXls(2) + row + ":" + reportUtility.GetColumnNameForXls(3) + row].Merge();

            row++;

            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Issue Date");
            reportUtility.SetText(ref sheet, row, 2, header["PostingDate"].ToString(), ExcelHAlign.HAlignLeft);

            reportUtility.SetMasterHeaderText(ref sheet, row, 4, "DocDate");
            reportUtility.SetText(ref sheet, row, 5, header["DocDate"].ToString(), ExcelHAlign.HAlignLeft);

            sheet[reportUtility.GetColumnNameForXls(2) + row + ":" + reportUtility.GetColumnNameForXls(3) + row].Merge();

            row++;

            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Status");
            reportUtility.SetText(ref sheet, row, 2, header["Status"].ToString(), ExcelHAlign.HAlignLeft);
            reportUtility.SetMasterHeaderText(ref sheet, row, 4, "Doc Ref");
            reportUtility.SetText(ref sheet, row, 5, header["DocRefNo"].ToString(), ExcelHAlign.HAlignLeft);

            sheet[reportUtility.GetColumnNameForXls(2) + row + ":" + reportUtility.GetColumnNameForXls(3) + row].Merge();

            row++;

            colLast = companyCurrencyId == transcationCurrency ? 5 : 7;
            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Narration");
            reportUtility.SetText(ref sheet, row, 2, header["Narration"].ToString(), ExcelHAlign.HAlignLeft);
            sheet[reportUtility.GetColumnNameForXls(2) + row + ":" + reportUtility.GetColumnNameForXls(3) + row].Merge();

            row++;

            if (companyCurrencyId == transcationCurrency)
            {
                reportUtility.SetHeaderText(ref sheet, row, 4, companyCurrencyCode, ExcelHAlign.HAlignCenter);
                sheet[row, 4, row, 5].Merge();
                sheet[row, 4, row, 5].BorderAround(ExcelLineStyle.Thin);
            }
            else
            {
                reportUtility.SetHeaderText(ref sheet, row, 4, header["CurrencyCode"].ToString(), ExcelHAlign.HAlignCenter);
                sheet[row, 4, row, 5].Merge();

                reportUtility.SetHeaderText(ref sheet, row, 6, companyCurrencyCode, ExcelHAlign.HAlignCenter);
                sheet[row, 6, row, 7].Merge();
                sheet[row, 6, row, 7].BorderAround(ExcelLineStyle.Thin);
            }

            row++;

            reportUtility.SetHeaderText(ref sheet, row, xlsCol, "GL"); colGl = xlsCol; xlsCol++;
            sheet[reportUtility.GetColumnNameForXls(colGl) + row + ":" + reportUtility.GetColumnNameForXls(2) + row].BorderAround(ExcelLineStyle.Thin); ;
            sheet[reportUtility.GetColumnNameForXls(colGl) + row + ":" + reportUtility.GetColumnNameForXls(2) + row].Merge(); xlsCol++;
            reportUtility.SetHeaderText(ref sheet, row, xlsCol, "GL", 12, ExcelHAlign.HAlignRight);

            reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Particulars", 12); colParticulars = xlsCol; xlsCol++;

            if (companyCurrencyId != transcationCurrency)
            {
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Debit", 12, ExcelHAlign.HAlignRight); colinrDebit = xlsCol; xlsCol++;
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Credit", 12, ExcelHAlign.HAlignRight); colinrCredit = xlsCol; xlsCol++;

                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Debit", 12, ExcelHAlign.HAlignRight); colusdDebit = xlsCol; xlsCol++;
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Credit", 12, ExcelHAlign.HAlignRight); colusdCradit = xlsCol;
                colLast = xlsCol;
            }
            else
            {
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Debit", 13, ExcelHAlign.HAlignRight); colinrDebit = xlsCol; xlsCol++;
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Credit", 13, ExcelHAlign.HAlignRight); colinrCredit = xlsCol;
                colLast = xlsCol;
            }

            if (dsLocal.Rows.Count > 0)
            {
                double totalTranAmount = 0;
                double totalBookCurrencyAmount = 0;
                var xRow = row;
                row++;
                for (int i = 0; i < dsLocal.Rows.Count; i++)
                {
                    var glName = dsLocal.Rows[i]["BudgetName"].ToString();


                    reportUtility.SetText(ref sheet, row, colGl, dsLocal.Rows[i]["GLGeneralInfoCode"] + " - " + glName + " - " + dsLocal.Rows[i]["Activity"]);

                    sheet[reportUtility.GetColumnNameForXls(colGl) + row + ":" + reportUtility.GetColumnNameForXls(2) + row].Merge();


                    reportUtility.SetText(ref sheet, row, colParticulars, dsLocal.Rows[i]["ParticularName"].ToString());

                    if (companyCurrencyId != transcationCurrency)
                    {
                        reportUtility.SetText(ref sheet, row, colinrDebit, Convert.ToDouble(dsLocal.Rows[i]["DrAmount"].ToString()));
                        reportUtility.SetText(ref sheet, row, colinrCredit, Convert.ToDouble(dsLocal.Rows[i]["CrAmount"].ToString()));
                        reportUtility.SetText(ref sheet, row, colusdDebit, Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyDrAmount"].ToString()));
                        reportUtility.SetText(ref sheet, row, colusdCradit, Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyCrAmount"].ToString()));
                        totalTranAmount += Convert.ToDouble(dsLocal.Rows[i]["DrAmount"].ToString());
                    }
                    else
                    {
                        reportUtility.SetText(ref sheet, row, colinrDebit, Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyDrAmount"].ToString()));
                        reportUtility.SetText(ref sheet, row, colinrCredit, Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyCrAmount"].ToString()));
                    }
                    totalBookCurrencyAmount += Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyDrAmount"].ToString());

                    sheet.Range[row, 1, row, colLast].BorderInside(ExcelLineStyle.Hair);
                    sheet.Range[row, 1, row, colLast].BorderAround(ExcelLineStyle.Hair);
                    row++;

                    glName = string.Empty;

                }


                reportUtility.SetText(ref sheet, row, 3, "Total: ", true);
                var lastRow = row - 1;

                if (companyCurrencyId != transcationCurrency)
                {
                    sheet.Range[row, colinrDebit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colinrDebit) + xRow + ":" + reportUtility.GetColumnNameForXls(colinrDebit) + (lastRow) + ")";
                    sheet.Range[row, colinrDebit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colinrDebit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colinrDebit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colinrDebit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colinrDebit].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, colinrCredit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colinrCredit) + xRow + ":" + reportUtility.GetColumnNameForXls(colinrCredit) + (lastRow) + ")";
                    sheet.Range[row, colinrCredit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colinrCredit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colinrCredit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colinrCredit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colinrCredit].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, colusdDebit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colusdDebit) + xRow + ":" + reportUtility.GetColumnNameForXls(colusdDebit) + (lastRow) + ")";
                    sheet.Range[row, colusdDebit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colusdDebit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colusdDebit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colusdDebit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colusdDebit].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, colusdCradit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colusdCradit) + xRow + ":" + reportUtility.GetColumnNameForXls(colusdCradit) + (lastRow) + ")";
                    sheet.Range[row, colusdCradit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colusdCradit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colusdCradit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colusdCradit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colusdCradit].BorderAround(ExcelLineStyle.Hair);
                }
                else
                {
                    sheet.Range[row, colinrDebit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colinrDebit) + xRow + ":" + reportUtility.GetColumnNameForXls(colinrDebit) + (lastRow) + ")";
                    sheet.Range[row, colinrDebit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colinrDebit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colinrDebit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colinrDebit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colinrDebit].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, colinrCredit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colinrCredit) + xRow + ":" + reportUtility.GetColumnNameForXls(colinrCredit) + (lastRow) + ")";
                    sheet.Range[row, colinrCredit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colinrCredit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colinrCredit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colinrCredit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colinrCredit].BorderAround(ExcelLineStyle.Hair);
                }

                row += 2;
                reportUtility.SetText(ref sheet, row, 1, "In Word:", true);

                if (companyCurrencyId != transcationCurrency && GetPlantIsShowFCInWord(plantId))
                {
                    sheet.Range[reportUtility.GetColumnNameForXls(2) + row].Text = reportUtility.InWord(totalTranAmount, transcationCurrency);
                    sheet.Range[reportUtility.GetColumnNameForXls(2) + row + ":" + reportUtility.GetColumnNameForXls(colLast) + row].Merge();
                    sheet.Range[reportUtility.GetColumnNameForXls(2) + row].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet.Range[reportUtility.GetColumnNameForXls(2) + row].VerticalAlignment = ExcelVAlign.VAlignTop;
                    sheet.Range[reportUtility.GetColumnNameForXls(2) + row].CellStyle.Font.Bold = true;
                    row++;
                }

                sheet.Range[reportUtility.GetColumnNameForXls(2) + row].Text = reportUtility.InWord(totalBookCurrencyAmount, companyCurrencyId);
                sheet.Range[reportUtility.GetColumnNameForXls(2) + row + ":" + reportUtility.GetColumnNameForXls(colLast) + row].Merge();
                sheet.Range[reportUtility.GetColumnNameForXls(2) + row].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[reportUtility.GetColumnNameForXls(2) + row].VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[reportUtility.GetColumnNameForXls(2) + row].CellStyle.Font.Bold = true;

                sheet.UsedRange.AutofitColumns();
                sheet[1, 2].ColumnWidth = 40;
                sheet.UsedRange.CellStyle.Font.Size = 8;
                row += 4;
                reportUtility.SetSignatureText(ref sheet, row - 1, 1, header["AddedBy"].ToString());
                sheet.Range[row, 1].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                reportUtility.SetTextMiddle(ref sheet, row, 1, "Prepared By", true);

                reportUtility.SetSignatureText(ref sheet, row - 1, 3, header["PostedBy"].ToString());
                sheet.Range[row, 3].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                reportUtility.SetTextMiddle(ref sheet, row, 3, "Checked By", true);

                sheet.Range[row, 5].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                reportUtility.SetTextMiddle(ref sheet, row, 5, "Authorized By", true);

                reportUtility.CompanyPlantHeader(ref sheet, colLast, header["VoucherTypeName"].ToString(), companyId, plantId, plantName, null);
                reportUtility.PageSetup(ref sheet, colLast, ExcelPageOrientation.Portrait);
            }
            else
            {
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.CellStyle.Font.Size = 8;
                reportUtility.CompanyPlantHeader(ref sheet, 5, header["VoucherTypeName"].ToString(), companyId, plantId, plantName, null);
                reportUtility.PageSetup(ref sheet, 5, ExcelPageOrientation.Portrait);
            }
            return workbook;
        }


        private Dictionary<string, object> GetIssueJournalHeader(string companyGroupId, string companyId, string plantId, string issueId, SourceType sourceType)
        {
            var cmdText = @"SELECT II.Id IssueNo,VT.UserName AS VoucherTypeName, V.VoucherNo, REPLACE(CONVERT(VARCHAR(11), V.VoucherDate, 106), ' ', '-') AS VoucherDate, REPLACE(CONVERT(VARCHAR(11), V.PostingDate, 106), ' ', '-') AS PostingDate
                            , REPLACE(CONVERT(VARCHAR(11), V.DocDate, 106), ' ', '-') AS DocDate, II.Id DocRefNo, V.AddedBy, V.PostedBy, UPPER(V.Narration) AS Narration, CASE WHEN V.IsPark=1 THEN 'Parked' ELSE 'Posted' END AS [Status]
                            , V.CurrencyId, C.Code AS CurrencyCode
                            FROM  [TRN].[Voucher] AS V 
							LEFT JOIN  [TRN].[InventoryIssue] II ON II.VoucherId=V.Id
                            LEFT JOIN [SCS].[VoucherType] AS VT ON VT.Id=V.VoucherTypeId
							LEFT JOIN [SCS].[Currency] AS C ON C.Id=V.CurrencyId
                            WHERE V.Archive=0 AND V.CompanyGroupId='" + companyGroupId + "' AND V.CompanyId='" + companyId + "' AND V.PlantId='" + plantId + "' AND II.Id='" + issueId + "' AND V.SourceType='" + sourceType + "'";
            return _sqlRepository.GetData(cmdText);
        }
        private DataTable InventoryIssueData(string companyGroupId, string companyId, string plantId, string issueId, SourceType sourceType)
        {
            var sql = @"SELECT   IR.Id IssueNo,V.Id, GL.Id AS AccountCodeId, GL.AccountCode, VDC.VoucherDetailId, FY.FiscalYearName, FYP.PeriodName, FYP.PeriodNo, V.IsPark, REPLACE(CONVERT(VARCHAR(11), V.PostingDate, 106), ' ', '-') AS PostingDate
                            , [Park/Post]=CASE WHEN V.IsPark=1 THEN 'Parked' ELSE 'Posted' END, REPLACE(CONVERT(VARCHAR(11), V.DocDate, 106), ' ', '-') AS DocDate, V.DocRefNo, REPLACE(CONVERT(VARCHAR(11), V.VoucherDate, 106), ' ', '-') AS VoucherDate
                            , V.VoucherNo, V.CurrencyId, CU1.Code AS TrnCurrency, V.AddedBy, V.PostedBy, VDC.ParallelCurrencyId, CU.Code AS CurrencyCode, VDC.FromCurrencyId, VDC.ToCurrencyId, VDC.ToCurrencyRate
                            , VD.DrAmount+VD.CrAmount AS Value,VD.DrAmount,VD.CrAmount, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount, [DRCR]=CASE WHEN VDC.DrAmount>0 THEN '1' ELSE '2' END, VD.GLGeneralInfoId, GL.UserName AS GL, GL.AccountCode AS GLGeneralInfoCode
                            , REPLACE(CONVERT(VARCHAR(11), VD.DocDate, 106), ' ', '-') AS InvoiceDate, VD.DocRefNo AS InvoiceNo, UPPER(VD.Narration) AS DetailNarration, ENT.UserName AS Entity
                            , VD.Id AS BudgetMasterId, BUD.UserName AS BudgetName, ACT.UserName AS Activity, UPPER(V.Narration) AS Narration, VD.PartyType, VD.FAType,VD.FixedAssetMasterId
							,null [ParticularName]
                        FROM [TRN].[InventoryIssue] AS IR
						LEFT JOIN [TRN].[Voucher] AS V ON IR.VoucherId=V.Id
						LEFT JOIN [TRN].[VoucherDetail] AS VD ON IR.VoucherId=VD.VoucherId
						LEFT JOIN [TRN].[VoucherDetailCurrency] AS VDC ON VDC.VoucherDetailId=VD.Id
						LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON GL.Id=VD.GLGeneralInfoId
						LEFT JOIN [SCS].[FiscalYear] AS FY ON V.FiscalYearId=FY.Id
						LEFT  JOIN [SCS].[FiscalYearPeriod] AS FYP ON V.FiscalYearPeriodId=FYP.Id
                        LEFT JOIN [dbo].[EmployeeInformation] AS EMP ON IR.EmployeeId=EMP.SystemId
						LEFT JOIN [SCS].[Currency] AS CU ON CU.Id=VDC.ParallelCurrencyId
                            LEFT JOIN [SCS].[Currency] AS CU1 ON CU1.Id=V.CurrencyId
                            LEFT JOIN [MST].[BudgetMaster] BMT ON VD.BudgetMasterId=BMT.Id
                            LEFT JOIN [HKP].[Budget] BUD ON BUD.Id=BMT.BudgetId
                            LEFT JOIN [HKP].[Activity] AS ACT ON ACT.Id = VD.ActivityId
                            LEFT JOIN [ORG].[Entity] AS ENT ON ENT.Id = VD.EntityId
						where IR.Id='" + issueId + "'";
            return _sqlRepository.GetDataTable(sql);
        }


        #endregion

        #region Inventory JobWork Received

        public Dictionary<string, object> GetOutSourcingHeader(string companyGroupId, string companyId, string plantId, string voucherId)
        {
            var cmdText = @"SELECT VT.UserName AS VoucherTypeName, V.VoucherNo

                        , REPLACE(CONVERT(VARCHAR(11), V.VoucherDate, 106), ' ', '-') AS VoucherDate
                        , REPLACE(CONVERT(VARCHAR(11), V.PostingDate, 106), ' ', '-') AS PostingDate
                        , REPLACE(CONVERT(VARCHAR(11), V.DocDate, 106), ' ', '-') AS DocDate
							, V.DocRefNo, V.AddedBy, V.PostedBy
							,[Type]=CASE WHEN IR.EmployeeId<>'' THEN 'Employee' Else 'Vendor' END
							, UPPER(V.Narration) AS Narration
							, CASE WHEN V.IsPark=1 THEN 'Parked' ELSE 'Posted' END AS [Status]
                            , V.CurrencyId, C.Code AS CurrencyCode
						    		,IR.TransformationContractId POId
							
								--,POId=STUFF((select distinct ','+PO.Id from
								--	TRN.InventoryReceiveDetail XVD JOIN TRN.InventoryReceive AS XP ON XP.Id=XVD.InventoryReceiveId AND IR.Id=XVD.InventoryReceiveId
								--	LEFT JOIN TRN.PurchaseOrder PO ON PO.Id=XVD.POId  for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

									,PDA.AcceptanceNo,
									PDA.AcceptanceDate
									,IR.GRNType
									,IR.Id GRNNo, IR.JWGRIRVoucherId

								--,PODate=	STUFF((select distinct ','+REPLACE(CONVERT(CHAR(11), PO.PODate, 106),' ','-') from
								--						TRN.InventoryReceiveDetail XVD JOIN TRN.InventoryReceive AS XP ON XP.Id=XVD.InventoryReceiveId AND IR.Id=XVD.InventoryReceiveId
								--						LEFT JOIN TRN.PurchaseOrder PO ON PO.Id=XVD.POId  for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								--,POVendorRefNo=	STUFF((select distinct ','+PO.DocRefNo from
								--						TRN.InventoryReceiveDetail XVD JOIN TRN.InventoryReceive AS XP ON XP.Id=XVD.InventoryReceiveId AND IR.Id=XVD.InventoryReceiveId
								--						LEFT JOIN TRN.PurchaseOrder PO ON PO.Id=XVD.POId  for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								--,LCNo=	STUFF((select distinct ','+LC.LCRef from
								--						TRN.InventoryReceiveDetail XVD JOIN TRN.InventoryReceive AS XP ON XP.Id=XVD.InventoryReceiveId AND IR.Id=XVD.InventoryReceiveId
								--						LEFT JOIN TRN.PurchaseOrder PO ON PO.Id=XVD.POId
								--						LEFT JOIN DBO.PurchaseLC LC ON LC.Id=PO.PurchaseLCId
								--						for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								-- ,PurchaseLCId=	STUFF((select distinct ','+LC.Id from
								--						TRN.InventoryReceiveDetail XVD JOIN TRN.InventoryReceive AS XP ON XP.Id=XVD.InventoryReceiveId AND IR.Id=XVD.InventoryReceiveId
								--						LEFT JOIN TRN.PurchaseOrder PO ON PO.Id=XVD.POId
								--						LEFT JOIN DBO.PurchaseLC LC ON LC.Id=PO.PurchaseLCId
								--						for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

								,ContractNo=	STUFF((select distinct ','+C.ContractNo from
														TRN.InventoryReceiveDetail XVD JOIN TRN.InventoryReceive AS XP ON XP.Id=XVD.InventoryReceiveId AND IR.Id=XVD.InventoryReceiveId
														LEFT JOIN TRN.PurchaseOrder PO ON PO.Id=XVD.POId
														LEFT JOIN dbo.PurchaseLC LC ON LC.Id=PO.PurchaseLCId
														LEFT JOIN dbo.[Contract] C ON C.Id=LC.ContractId
														for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								 ,CustomerName=	STUFF((select distinct ','+P.UserName from
														TRN.InventoryReceiveDetail XVD JOIN TRN.InventoryReceive AS XP ON XP.Id=XVD.InventoryReceiveId AND IR.Id=XVD.InventoryReceiveId
														LEFT JOIN TRN.PurchaseOrder PO ON PO.Id=XVD.POId
														LEFT JOIN dbo.PurchaseLC LC ON LC.Id=PO.PurchaseLCId
														LEFT JOIN dbo.[Contract] C ON C.Id=LC.ContractId
														LEFT JOIN HKP.Party P ON P.Id=C.CustomerId
														for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')



                            FROM  [TRN].[Voucher] AS V 
                            LEFT JOIN [SCS].[VoucherType] AS VT ON VT.Id=V.VoucherTypeId
							LEFT JOIN [SCS].[Currency] AS C ON C.Id=V.CurrencyId
							left join trn.InventoryReceive IR ON IR.JWGRIRVoucherId =V.Id


					  LEFT JOIN [EmployeeInformation] AS EI ON IR.EmployeeId=EI.SystemId
     --               LEFT JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
     --               LEFT JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
     --               LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
     --               LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
     --               LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
     --               LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
     --               LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
     --               LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
     --               LEFT JOIN [TRN].GateEntry GE ON GE.Id=IR.GateEntryNo
					--LEFT JOIN dbo.PlantWiseGate PG ON PG.Id=GE.PlantWiseGateId
					LEFT JOIN TRN.PurchaseDocAcceptance PDA ON PDA.Id=IR.PurchaseDocumentAcceptanceId
					LEFT JOIN dbo.PurchaseLC PLC ON PLC.Id=PDA.PurchaseLCId

                    WHERE V.Archive=0 AND V.CompanyGroupId='" + companyGroupId+"' AND V.CompanyId='"+companyId+"' AND V.PlantId='"+plantId+@"' 
					AND V.Id='"+voucherId+@"' 
					AND V.SourceType='InventoryJWReceipt'";
            return _sqlRepository.GetData(cmdText);
        }

        public DataTable GetOutSourcingJournalData(string companyGroupId, string companyId, string plantId, string voucherId)
        {
            var cmdText = @"SELECT V.Id, GL.Id AS AccountCodeId, GL.AccountCode, VDC.VoucherDetailId, FY.FiscalYearName, FYP.PeriodName, FYP.PeriodNo, V.IsPark, REPLACE(CONVERT(VARCHAR(11), V.PostingDate, 106), ' ', '-') AS PostingDate
                            , [Park/Post]=CASE WHEN V.IsPark=1 THEN 'Parked' ELSE 'Posted' END, REPLACE(CONVERT(VARCHAR(11), V.DocDate, 106), ' ', '-') AS DocDate, V.DocRefNo, REPLACE(CONVERT(VARCHAR(11), V.VoucherDate, 106), ' ', '-') AS VoucherDate
                            , V.VoucherNo, V.CurrencyId, CU1.Code AS TrnCurrency, V.AddedBy, V.PostedBy, VDC.ParallelCurrencyId, CU.Code AS CurrencyCode, VDC.FromCurrencyId, VDC.ToCurrencyId, VDC.ToCurrencyRate
                            , VD.DrAmount+VD.CrAmount AS Value,VD.DrAmount,VD.CrAmount, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount, [DRCR]=CASE WHEN VDC.DrAmount>0 THEN '1' ELSE '2' END, VD.GLGeneralInfoId, GL.UserName AS GL, GL.AccountCode AS GLGeneralInfoCode
                            , REPLACE(CONVERT(VARCHAR(11), VD.DocDate, 106), ' ', '-') AS InvoiceDate, VD.DocRefNo AS InvoiceNo, UPPER(VD.Narration) AS DetailNarration, ENT.UserName AS Entity
                            , VD.Id AS BudgetMasterId, BUD.UserName AS BudgetName, ACT.UserName AS Activity, UPPER(V.Narration) AS Narration, P.UserName AS PartyName, PP.UserName AS PartyLocation,VD.PartyType, VD.FAType,VD.FixedAssetMasterId
							,[ParticularName]=CASE
								WHEN EI.EmployeeName<>'' THEN EI.EmployeeCode+'-'+EI.EmployeeName
								WHEN BM.AccountTitle<>'' THEN BM.AccountTitle
								WHEN P.UserName<>'' THEN P.UserName 
								WHEN CM.UserName<>'' THEN CM.UserName
                                WHEN FAM.UserName<>'' THEN FAM.UserName
								ELSE ''	END
                            FROM [TRN].[VoucherDetailCurrency] AS VDC
                            INNER JOIN [TRN].[VoucherDetail] AS VD ON VD.Id =VDC.VoucherDetailId
                            INNER JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                            LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON GL.Id=VD.GLGeneralInfoId
                            LEFT JOIN [SCS].[Currency] AS CU ON CU.Id=VDC.ParallelCurrencyId
                            LEFT JOIN [SCS].[Currency] AS CU1 ON CU1.Id=V.CurrencyId
                            LEFT JOIN [SCS].[FiscalYear] AS FY ON FY.Id=V.FiscalYearId
                            LEFT JOIN [SCS].[FiscalYearPeriod] AS FYP ON FYP.Id=V.FiscalYearPeriodId
                            LEFT JOIN [MST].[BudgetMaster] BMT ON VD.BudgetMasterId=BMT.Id
                            LEFT JOIN [HKP].[Budget] BUD ON BUD.Id=BMT.BudgetId
                            LEFT JOIN [HKP].[Activity] AS ACT ON ACT.Id = VD.ActivityId
                            LEFT JOIN [ORG].[Entity] AS ENT ON ENT.Id = VD.EntityId
							LEFT JOIN [HKP].Party AS P ON P.Id=VD.PartyId
							LEFT JOIN [HKP].PartyPlant AS PP ON PP.Id=VD.PartyPlantId
							LEFT JOIN [DBO].EmployeeInformation AS EI ON EI.SystemId=VD.EmployeeId
							LEFT JOIN [MST].BankMaster AS BM ON BM.Id=VD.BankMasterId
							LEFT JOIN [MST].CashMaster AS CM ON CM.Id=VD.CashMasterId
                            LEFT JOIN [MST].[FixedAssetMaster] AS FAM ON FAM.Id=VD.FixedAssetMasterId
                            WHERE V.Archive=0 AND V.CompanyGroupId='" + companyGroupId + "' AND V.CompanyId='" + companyId + "' AND V.PlantId='" + plantId + "' AND V.Id='" + voucherId + "' ORDER BY VD.DrAmount DESC";
            return _sqlRepository.GetDataTable(cmdText);
        }

        public IWorkbook GetOutSourcingVoucherReport(out string reportFileName, string companyGroupId, string companyId, string plantId, string plantName, string voucherId)
        {
            var reportUtility = new ReportUtility();
            var excelEngine = new ExcelEngine();
            var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
            workbook.Version = ExcelVersion.Excel2016;
            var sheet = workbook.Worksheets[0];
            sheet.Name = "Voucher";

            var header = GetOutSourcingHeader(companyGroupId, companyId, plantId, voucherId);

            reportFileName = Convert.ToDateTime(header["PostingDate"]).ToString("yyMMdd") + " " + header["VoucherNo"];

            var dsLocal = GetOutSourcingJournalData(companyGroupId, companyId, plantId, voucherId);

            var transcationCurrency = header["CurrencyId"].ToString();
            GetParallelCurrency(companyId, out string companyCurrencyId, out string companyCurrencyCode);

            var row = 5;
            var colLast = 1;
            int xlsCol = 1;

            int colinrDebit = 0;
            int colinrCredit = 0;
            int colusdDebit = 0;
            int colusdCradit = 0;

            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Voucher No");
            sheet[row, 1].ColumnWidth = 20;
            sheet.Range[row, 1].VerticalAlignment = ExcelVAlign.VAlignTop;
            reportUtility.SetText(ref sheet, row, 2, header["VoucherNo"].ToString());
            sheet[row, 2].ColumnWidth = 15;
            sheet.Range[row, 2].VerticalAlignment = ExcelVAlign.VAlignTop;


            reportUtility.SetMasterHeaderText(ref sheet, row, 6, "Voucher Date");
            reportUtility.SetText(ref sheet, row, 7, header["VoucherDate"].ToString());
            sheet.Range[row, 6].VerticalAlignment = ExcelVAlign.VAlignTop;
            sheet.Range[row, 7].VerticalAlignment = ExcelVAlign.VAlignTop;
            row++;

            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Posting Date");
            reportUtility.SetText(ref sheet, row, 2, header["PostingDate"].ToString());
            sheet.Range[row, 1].VerticalAlignment = ExcelVAlign.VAlignTop;
            sheet.Range[row, 2].VerticalAlignment = ExcelVAlign.VAlignTop;

            reportUtility.SetMasterHeaderText(ref sheet, row, 6, "DocDate");
            reportUtility.SetText(ref sheet, row, 7, header["DocDate"].ToString());
            sheet.Range[row, 6].VerticalAlignment = ExcelVAlign.VAlignTop;
            sheet.Range[row, 7].VerticalAlignment = ExcelVAlign.VAlignTop;
            row++;


            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "PO No");
            reportUtility.SetText(ref sheet, row, 2, header["POId"].ToString());
            sheet[reportUtility.GetColumnNameForXls(2) + row + ":" + reportUtility.GetColumnNameForXls(5) + row].Merge();
            sheet.Range[row, 1].VerticalAlignment = ExcelVAlign.VAlignTop;
            sheet.Range[row, 2].VerticalAlignment = ExcelVAlign.VAlignTop;


            reportUtility.SetMasterHeaderText(ref sheet, row, 6, "GRN No");
            reportUtility.SetText(ref sheet, row, 7, header["GRNNo"].ToString());
            sheet.Range[row, 6].VerticalAlignment = ExcelVAlign.VAlignTop;
            sheet.Range[row, 7].VerticalAlignment = ExcelVAlign.VAlignTop;
            row++;

            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Contract No");
            reportUtility.SetText(ref sheet, row, 2, header["ContractNo"].ToString());
            sheet[reportUtility.GetColumnNameForXls(2) + row + ":" + reportUtility.GetColumnNameForXls(5) + row].Merge();
            sheet.Range[row, 1].VerticalAlignment = ExcelVAlign.VAlignTop;
            sheet.Range[row, 2].VerticalAlignment = ExcelVAlign.VAlignTop;

            reportUtility.SetMasterHeaderText(ref sheet, row, 6, "Doc Ref");
            reportUtility.SetText(ref sheet, row, 7, header["DocRefNo"].ToString());
            sheet.Range[row, 6].VerticalAlignment = ExcelVAlign.VAlignTop;
            sheet.Range[row, 7].VerticalAlignment = ExcelVAlign.VAlignTop;
            row++;

            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Narration");
            reportUtility.SetText(ref sheet, row, 2, header["Narration"].ToString());
            sheet[reportUtility.GetColumnNameForXls(2) + row + ":" + reportUtility.GetColumnNameForXls(5) + row].Merge();
            sheet.Range[row, 1].VerticalAlignment = ExcelVAlign.VAlignTop;
            sheet.Range[row, 2].VerticalAlignment = ExcelVAlign.VAlignTop;

            reportUtility.SetMasterHeaderText(ref sheet, row, 6, "Status");
            reportUtility.SetText(ref sheet, row, 7, header["Status"].ToString());
            sheet.Range[row, 6].VerticalAlignment = ExcelVAlign.VAlignTop;
            sheet.Range[row, 7].VerticalAlignment = ExcelVAlign.VAlignTop;

            //row++;
            row++;  //10
            colLast = companyCurrencyId == transcationCurrency ? 7 : 9;
            if (companyCurrencyId == transcationCurrency)
            {
                reportUtility.SetHeaderText(ref sheet, row, 6, companyCurrencyCode, ExcelHAlign.HAlignCenter);
                sheet[row, 6, row, 7].Merge();
            }
            else
            {
                reportUtility.SetHeaderText(ref sheet, row, 6, header["CurrencyCode"].ToString(), ExcelHAlign.HAlignCenter);
                sheet[row, 6, row, 7].Merge();

                reportUtility.SetHeaderText(ref sheet, row, 8, companyCurrencyCode, ExcelHAlign.HAlignCenter);
                sheet[row, 8, row, 9].Merge();
            }
            //sheet[row, 6].RowHeight = 15;

            sheet.Range[row, 6, row, colLast].BorderAround(ExcelLineStyle.Hair);
            sheet.Range[row, 6, row, colLast].BorderInside(ExcelLineStyle.Hair);
            row++;

            int colGl = 0;
            reportUtility.SetHeaderText(ref sheet, row, xlsCol, "GL"); colGl = xlsCol; xlsCol++;
            sheet[reportUtility.GetColumnNameForXls(colGl) + row + ":" + reportUtility.GetColumnNameForXls(3) + row].Merge();

            xlsCol++; //clo3

            xlsCol++; //cloDNaration
                      // int colDnaration = 0;
                      // reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Detail Narration"); colDnaration = xlsCol;
            sheet[row, 4].ColumnWidth = 15;
            //sheet[reportUtility.GetColumnNameForXls(colGl) + row + ":" + reportUtility.GetColumnNameForXls(3) + row].Merge();
            // sheet.ShowColumn(4, false); 
            //sheet.HideColumn(5);
            //sheet[1, 5].ColumnWidth = 0; 


            xlsCol++; //clo5
            int colParticulars = 0;
            colParticulars = xlsCol;
            reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Particulars");
            sheet[row, colParticulars].ColumnWidth = 20;
            //sheet[reportUtility.GetColumnNameForXls(colGl) + row + ":" + reportUtility.GetColumnNameForXls(2) + row].Merge();
            xlsCol++;

            //xlsCol++;

            if (companyCurrencyId != transcationCurrency)
            {
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Debit", 13, ExcelHAlign.HAlignRight); colinrDebit = xlsCol; xlsCol++;
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Credit", 13, ExcelHAlign.HAlignRight); colinrCredit = xlsCol; xlsCol++;

                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Debit", 13, ExcelHAlign.HAlignRight); colusdDebit = xlsCol; xlsCol++;
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Credit", 13, ExcelHAlign.HAlignRight); colusdCradit = xlsCol;
                colLast = xlsCol; //col9

                sheet.Range[row, colGl, row, colLast].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[row, colGl, row, colLast].BorderInside(ExcelLineStyle.Hair);
                //sheet.Range[row, colGl, row, colLast].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
            }
            else
            {

                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Debit", 13, ExcelHAlign.HAlignRight); colinrDebit = xlsCol; xlsCol++;
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Credit", 13, ExcelHAlign.HAlignRight); colinrCredit = xlsCol;
                colLast = xlsCol;

                //sheet.Range[row, 4, row, colLast].BorderAround(ExcelLineStyle.Thin);
                //sheet.Range[row, 4, row, colLast].BorderInside(ExcelLineStyle.Thin);

                sheet.Range[row, colGl, row, colLast].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[row, colGl, row, colLast].BorderInside(ExcelLineStyle.Hair);
                //sheet.Range[row, 4, row, colLast].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
            }

            sheet[reportUtility.GetColumnNameForXls(colGl) + row + ":" + reportUtility.GetColumnNameForXls(4) + row].Merge();



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

                    var glName = dsLocal.Rows[i]["BudgetName"].ToString();


                    reportUtility.SetText(ref sheet, row, colGl, dsLocal.Rows[i]["GLGeneralInfoCode"] + " - " + glName + " - " + dsLocal.Rows[i]["Activity"]);

                    //sheet[reportUtility.GetColumnNameForXls(colGl) + row + ":" + reportUtility.GetColumnNameForXls(2) + row].Merge();
                    sheet[reportUtility.GetColumnNameForXls(colGl) + row + ":" + reportUtility.GetColumnNameForXls(colGl + 3) + row].Merge();


                    reportUtility.SetText(ref sheet, row, colParticulars, dsLocal.Rows[i]["ParticularName"].ToString());




                    if (companyCurrencyId != transcationCurrency)
                    {
                        reportUtility.SetText(ref sheet, row, colinrDebit, Convert.ToDouble(dsLocal.Rows[i]["DrAmount"].ToString()));
                        reportUtility.SetText(ref sheet, row, colinrCredit, Convert.ToDouble(dsLocal.Rows[i]["CrAmount"].ToString()));
                        reportUtility.SetText(ref sheet, row, colusdDebit, Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyDrAmount"].ToString()));
                        reportUtility.SetText(ref sheet, row, colusdCradit, Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyCrAmount"].ToString()));
                        totalTranAmount += Convert.ToDouble(dsLocal.Rows[i]["DrAmount"].ToString());
                    }
                    else
                    {
                        reportUtility.SetText(ref sheet, row, colinrDebit, Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyDrAmount"].ToString()));
                        reportUtility.SetText(ref sheet, row, colinrCredit, Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyCrAmount"].ToString()));
                    }
                    totalBookCurrencyAmount += Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyDrAmount"].ToString());

                    sheet.Range[row, 1, row, colLast].BorderInside(ExcelLineStyle.Hair);
                    sheet.Range[row, 1, row, colLast].BorderAround(ExcelLineStyle.Hair);

                    // glName = string.Empty;

                    // sheet.AutofitRow(3);



                    row++;
                }

                formulaEndRow = row - 1;
                reportUtility.SetText(ref sheet, row, 5, "Total: ", true);

                if (companyCurrencyId != transcationCurrency)
                {
                    
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

                    sheet.Range[row, colusdDebit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colusdDebit) + formulaStartRow + ":" + reportUtility.GetColumnNameForXls(colusdDebit) + (formulaEndRow) + ")";
                    sheet.Range[row, colusdDebit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colusdDebit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colusdDebit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colusdDebit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colusdDebit].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, colusdCradit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colusdCradit) + formulaStartRow + ":" + reportUtility.GetColumnNameForXls(colusdCradit) + (formulaEndRow) + ")";
                    sheet.Range[row, colusdCradit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colusdCradit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colusdCradit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colusdCradit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colusdCradit].BorderAround(ExcelLineStyle.Hair);
                }
                else
                {
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
                }

                sheet.Range[row, colinrDebit, row, colLast].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[row, colinrDebit, row, colLast].BorderAround(ExcelLineStyle.Hair);

                row += 2;
                reportUtility.SetText(ref sheet, row, 1, "In Word:", true);

                if (companyCurrencyId != transcationCurrency && GetPlantIsShowFCInWord(plantId))
                {
                    sheet.Range[reportUtility.GetColumnNameForXls(2) + row].Text = reportUtility.InWord(totalTranAmount, transcationCurrency);
                    sheet.Range[reportUtility.GetColumnNameForXls(2) + row + ":" + reportUtility.GetColumnNameForXls(colLast) + row].Merge();
                    sheet.Range[reportUtility.GetColumnNameForXls(2) + row].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet.Range[reportUtility.GetColumnNameForXls(2) + row].VerticalAlignment = ExcelVAlign.VAlignTop;
                    // sheet.Range[reportUtility.GetColumnNameForXls(2) + row].CellStyle.Font.Bold = true;

                    sheet.Range[row, 2].VerticalAlignment = ExcelVAlign.VAlignTop;
                    row++;

                }

                sheet.Range[reportUtility.GetColumnNameForXls(2) + row].Text = reportUtility.InWord(totalBookCurrencyAmount, companyCurrencyId);
                sheet.Range[reportUtility.GetColumnNameForXls(2) + row + ":" + reportUtility.GetColumnNameForXls(colLast) + row].Merge();
                sheet.Range[reportUtility.GetColumnNameForXls(2) + row].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                // sheet.Range[reportUtility.GetColumnNameForXls(2) + row].VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[reportUtility.GetColumnNameForXls(2) + row].CellStyle.Font.Bold = true;
                sheet.Range[row, 2].VerticalAlignment = ExcelVAlign.VAlignTop;

                //sheet.UsedRange.AutofitColumns();

                sheet.UsedRange.CellStyle.Font.Size = 8;
                row += 4;

                reportUtility.SetSignatureText(ref sheet, row - 1, 1, header["AddedBy"].ToString());
                sheet.Range[row, 1].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                reportUtility.SetTextMiddle(ref sheet, row, 1, "Prepared By", true);
                sheet[row, 1].ColumnWidth = 21;

                // reportUtility.SetSignatureText(ref sheet, row - 1, 3, header["AddedBy"].ToString());
                sheet.Range[row, 3].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                reportUtility.SetTextMiddle(ref sheet, row, 3, "Received By", true);
                sheet[row, 3].ColumnWidth = 15;



                reportUtility.SetSignatureText(ref sheet, row - 1, 5, header["PostedBy"].ToString());
                sheet.Range[row, 5].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                reportUtility.SetTextMiddle(ref sheet, row, 5, "Checked By", true);
                //sheet[row, 5].ColumnWidth = 15;

                sheet.Range[row, 7].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                reportUtility.SetTextMiddle(ref sheet, row, 7, "Authorized By", true);
                sheet[row, 6].ColumnWidth = 15;
                sheet[row, 7].ColumnWidth = 15;

                sheet[row, 8].ColumnWidth = 15;
                sheet[row, 9].ColumnWidth = 15;


                reportUtility.CompanyPlantHeader(ref sheet, colLast, "Journal", companyId, plantName, null);
                reportUtility.PageSetup(ref sheet, colLast, ExcelPageOrientation.Portrait);

            }
            else
            {
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.CellStyle.Font.Size = 8;
                reportUtility.CompanyPlantHeader(ref sheet, 9, "Journal", companyId, plantName, null);
                reportUtility.PageSetup(ref sheet, 9, ExcelPageOrientation.Portrait);
            }

            return workbook;
        }

        #endregion
    }
}
