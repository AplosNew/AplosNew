#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Accounting.Accounts;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.MaterialManagement.Material;
using Library.Model.Commercial;
using Library.Model.Enums;
using Library.Model.Parties;
using Library.Security.Core;
using Library.Service.Finances;
using Library.Service.Helpers;
using Library.ViewModel.Accounts;
using Library.ViewModel.Vouchers;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Web.Mvc;

#endregion

namespace Aplos.Areas.Commercial.Controllers
{
    public class InvoiceTaggedWithLCController : BaseController
    {
        #region Constructor
        private readonly SqlRepository _sqlRepository;
        private readonly IAutoLoanService _autoLoanService;
        clsInvoiceTagWithLc ep = new clsInvoiceTagWithLc();
		public InvoiceTaggedWithLCController()
		{
			_sqlRepository = new SqlRepository();
		}
		#endregion

		#region -- Pages

		public ActionResult Aplos()
        {
            return View();
		}

		#endregion

		#region Operation

		[HttpGet, Authorize]
		public ActionResult GetVendorAvailableInvoiceList(string FromDate,string ToDate,bool DateRange)
		{
			try
			{
				var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var jsondata = Json(ep.VendorAvailableInvoiceList(identity.CompanyGroupId,identity.CompanyId, FromDate,ToDate,DateRange), JsonRequestBehavior.AllowGet);
                jsondata.MaxJsonLength = int.MaxValue;
                return jsondata;
            }
			catch (Exception ex)
			{
				return Json(new { Error = true, Message = ex.Message });
			}
        }

      

        [HttpGet, Authorize]
        public ActionResult InvoiceTaggedWithLCReportExcelFormat(ReportFormat reportFormat, string FromDate, string ToDate, bool DateRange)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var reportFileName = "Invoice Tagged With LC";
            var workbook = GetInvoiceTaggedWithLCReportWorkSheet(identity.CompanyGroupId, identity.CompanyId, FromDate, ToDate, DateRange);
            switch (reportFormat)
            {
                case ReportFormat.Pdf:
                    return RenderReportAsPdf(workbook, reportFileName);
                case ReportFormat.Excel:
                    return RenderReportAsExcel(workbook, reportFileName);
                default:
                    return RenderReportAsExcel(workbook, reportFileName);
            }
        }

        private IWorkbook GetInvoiceTaggedWithLCReportWorkSheet(string companyGroupId, string companyId, string FromDate, string ToDate, bool DateRange)
        {

            var excelEngine = new ExcelEngine();
            var report = new ReportUtility();
            var workbook = report.GetWorkbook(ref excelEngine, 1);
            workbook.Version = ExcelVersion.Excel2016;

            var sheet = workbook.Worksheets[0];

            sheet.Name = "InvoiceTaggedWithLC";


            int ROW = 5;
            int endCol = 1;
            int COL = 1;


            DataTable data = InvoiceWithTaggedLCList(companyGroupId, companyId, FromDate, ToDate, DateRange);

            #region Headers
            report.SetHeaderText(ref sheet, ROW, COL, "Voucher No", 12, ExcelHAlign.HAlignLeft);
            int ColVoucherNo = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Posting Date", 12, ExcelHAlign.HAlignLeft);
            int ColPostingDate = COL;
            COL++;


            report.SetHeaderText(ref sheet, ROW, COL, "DocRefNo", 15, ExcelHAlign.HAlignLeft);
            int ColDocRefNo = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Vendor", 25, ExcelHAlign.HAlignLeft);
            int ColPartyPlantName = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "DueDate BaseOn", 10, ExcelHAlign.HAlignLeft);
            int ColBaseOnDueDate = COL;
            COL++;


            report.SetHeaderText(ref sheet, ROW, COL, "DueDate", 10, ExcelHAlign.HAlignLeft);
            int ColActualDueDate = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "NoOfDays", 8, ExcelHAlign.HAlignLeft);
            int ColBaseNoOfDays = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Currency", 8, ExcelHAlign.HAlignLeft);
            int ColCurrencyCode = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Receivable", 15, ExcelHAlign.HAlignRight);
            int ColReceivable = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Received", 15, ExcelHAlign.HAlignRight);
            int ColReceived = COL;
            COL++;
            report.SetHeaderText(ref sheet, ROW, COL, "Balance", 15, ExcelHAlign.HAlignRight);
            int ColBalance = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Source Type", 12, ExcelHAlign.HAlignLeft);
            int ColSourceType = COL;

            endCol = COL;
            #endregion Headers

            var startRow = 0;

            int RowIndex = ROW;
            startRow = ROW;
            ROW++;
            for (int i = 0; i < data.Rows.Count; i++)
            {

                sheet[ROW, ColVoucherNo].Text = data.Rows[i]["VoucherNo"].ToString();
                sheet[ROW, ColPostingDate].Text = data.Rows[i]["PostingDate"].ToString();
                sheet[ROW, ColDocRefNo].Text = data.Rows[i]["DocRefNo"].ToString();
                sheet[ROW, ColPartyPlantName].Text = data.Rows[i]["PartyPlantName"].ToString();
                sheet[ROW, ColBaseOnDueDate].Text = data.Rows[i]["BaseOnDueDate"].ToString();

                sheet[ROW, ColActualDueDate].Text = data.Rows[i]["ActualDueDate"].ToString();
                sheet[ROW, ColBaseNoOfDays].Text = data.Rows[i]["BaseNoOfDays"].ToString();

                sheet[ROW, ColCurrencyCode].Text = data.Rows[i]["CurrencyCode"].ToString();

                sheet[ROW, ColReceivable].Number = Convert.ToDouble(data.Rows[i]["Receivable"].ToString());
				sheet[ROW, ColReceivable].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);
				sheet[ROW, ColReceivable].VerticalAlignment = ExcelVAlign.VAlignCenter;
				sheet[ROW, ColReceivable].HorizontalAlignment = ExcelHAlign.HAlignRight;

				sheet[ROW, ColReceived].Number = Convert.ToDouble(data.Rows[i]["Received"].ToString());
				sheet[ROW, ColReceived].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);
				sheet[ROW, ColReceived].VerticalAlignment = ExcelVAlign.VAlignCenter;
				sheet[ROW, ColReceived].HorizontalAlignment = ExcelHAlign.HAlignRight;

				sheet[ROW, ColBalance].Number = Convert.ToDouble(data.Rows[i]["Balance"].ToString());
                sheet[ROW, ColBalance].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);
                sheet[ROW, ColBalance].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet[ROW, ColBalance].HorizontalAlignment = ExcelHAlign.HAlignRight;


                sheet[ROW, ColSourceType].Text = data.Rows[i]["SourceType"].ToString();

                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);

                ROW++;
            }

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            sheet.UsedRange.NumberFormat = "#,##0.00";
            sheet.UsedRange.WrapText = true;
            sheet.UsedRange.CellStyle.Font.Size = 8;
            report.CompanyHeader(ref sheet, endCol, "Invoice Tagged With LC", identity.CompanyId);
            report.PageSetup(ref sheet, 5, ExcelPageOrientation.Landscape);
            return workbook;
        }

		public DataTable InvoiceWithTaggedLCList(string companyGroupId, string companyId, string FromDate, string ToDate, bool DateRange)
		{
			try
			{
				string DatewiseData = "";
				if (DateRange)
				{
					DatewiseData = "AND IV.ActualDueDate between '" + FromDate + @"' And '" + ToDate + @"'";

				}
				else
				{
					DatewiseData = "AND IV.ActualDueDate <= '" + FromDate + @"'";
				}
				string strSQL = string.Empty;
				strSQL = @" SELECT IVD.GLGeneralInfoId AS GLGeneralInfoId
									,GLGI.AccountCode AS GLGeneralInfoCode
									,GLGI.UserName AS GLGeneralInfoName
									,IVD.BudgetMasterId
									,B.UserName AS BudgetName
									,IVD.ActivityId
									,EN.UserName AS EntityName
									,A.UserName AS ActivityName
									,V.VoucherNo
									,Replace(CONVERT(VARCHAR(11), IV.DocDate, 106), ' ', '-') DocDate
									,Replace(CONVERT(VARCHAR(11), IV.PostingDate, 106), ' ', '-') PostingDate
									,IV.DocRefNo
									,IV.Narration
									,IV.Id AS InvoiceId
									,EN.Id EntityId
									,VD.PlantId
									,IVD.Id AS InvoiceDetailId
									,IV.VoucherId
									,Replace(CONVERT(VARCHAR(11),IV.ActualDueDate, 106), ' ', '-') ActualDueDate
									,Replace(CONVERT(VARCHAR(11),IV.BaseOnDueDate, 106), ' ', '-') BaseOnDueDate
									,IV.BaseNoOfDays, CASE WHEN  IV.SourceType = 'VendorInvoice' THEN 'Inbound Invoice'  
															WHEN  IV.SourceType = 'InventoryPayable' THEN  'GRN' 
															WHEN  IV.SourceType = 'PostInvoice' THEN  'Post Invoice' 
														END SourceType
									,VD.Id AS VoucherDetailId
									,IV.CurrencyId
									,C.Code AS CurrencyCode
									,IV.PartyId
									,IVD.Amount AS Receivable
									,V.ExchangeType
									,0 ExchangeAmount
									,IVD.WrittenOffAmount AS Received
									,IVD.Amount - IVD.WrittenOffAmount AS Balance
									,IV.PartyPlantId
									,PP.UserName AS PartyPlantName
									,CC.CompanyCurrencyId
									,CC.CompanyFromCurrencyId
									,CC.ToCurrencyId
									,CC.CompanyCurrencyRate
									,CC.CompanyCurrencyConversion
									,GC.CompanyGroupCurrencyId
									,GC.CompanyGroupFromCurrencyId
									,GC.CompanyGroupCurrencyRate
									,GC.CompanyGroupCurrencyConversion
									,HC.HardCurrencyId
									,HC.HardFromCurrencyId
									,HC.HardCurrencyRate
									,HC.HardCurrencyConversion
									,Particular = REPLACE(REPLACE(STUFF((
													SELECT DISTINCT ',' + xpo.UserName
													FROM hkp.Activity xpo
													INNER JOIN TRN.VoucherDetail xPDAMAP ON xpo.id = xPDAMAP.ActivityId
													WHERE VD.ActivityId != xPDAMAP.ActivityId
														AND xPDAMAP.VoucherId = V.Id
													FOR XML path('')
														,TYPE
													).value('.', 'VARCHAR(MAX)'), 1, 1, ''), '&amp;', '&'), 'amp;', '')
									,AcceptanceNo = STUFF((
											SELECT DISTINCT ',' + XPDA.AcceptanceNo
											FROM TRN.PurchaseDocAcceptance XPDA
											LEFT JOIN TRN.Voucher XV ON XV.Id = XPDA.VoucherId
											WHERE XV.Id = V.Id
											FOR XML path('')
												,TYPE
											).value('.', 'VARCHAR(MAX)'), 1, 1, '')
									,LCRef = STUFF((
											SELECT DISTINCT ',' + XLC.LCRef
											FROM dbo.PurchaseLC XLC
											LEFT JOIN TRN.PurchaseDocAcceptance XPDA ON XPDA.PurchaseLCId = XLC.Id
											LEFT JOIN TRN.Voucher XV ON XV.Id = XPDA.VoucherId
											WHERE XV.Id = V.Id
											FOR XML path('')
												,TYPE
											).value('.', 'VARCHAR(MAX)'), 1, 1, '')
									,ContractNo = STUFF((
											SELECT DISTINCT ',' + XC.ContractNo
											FROM dbo.PurchaseLC XLC
											JOIN TRN.PurchaseDocAcceptance XPDA ON XPDA.PurchaseLCId = XLC.Id
											LEFT JOIN dbo.Contract XC ON XC.Id = XLC.ContractId
											LEFT JOIN TRN.Voucher XV ON XV.Id = XPDA.VoucherId
											WHERE XV.Id = V.Id
											FOR XML path('')
												,TYPE
											).value('.', 'VARCHAR(MAX)'), 1, 1, '')
									,Customer = STUFF((
											SELECT DISTINCT ',' + XP.UserName
											FROM dbo.PurchaseLC XLC
											JOIN TRN.PurchaseDocAcceptance XPDA ON XPDA.PurchaseLCId = XLC.Id
											LEFT JOIN dbo.Contract XC ON XC.Id = XLC.ContractId
											LEFT JOIN HKP.Party XP ON XP.Id = XC.CustomerId
											LEFT JOIN TRN.Voucher XV ON XV.Id = XPDA.VoucherId
											WHERE XV.Id = V.Id
											FOR XML path('')
												,TYPE
											).value('.', 'VARCHAR(MAX)'), 1, 1, '')
									,MasterLCNo = STUFF((
											SELECT DISTINCT ',' + MLC.LCRef
											FROM dbo.PurchaseLC XLC
											JOIN TRN.PurchaseDocAcceptance XPDA ON XPDA.PurchaseLCId = XLC.Id
											LEFT JOIN dbo.Contract XC ON XC.Id = XLC.ContractId
											LEFT JOIN dbo.MasterLC MLC ON MLC.Id = XC.MasterLCId
											LEFT JOIN TRN.Voucher XV ON XV.Id = XPDA.VoucherId
											WHERE XV.Id = V.Id
											FOR XML path('')
												,TYPE
											).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								FROM [TRN].[InvoiceDetail] AS IVD
								LEFT JOIN [TRN].[Invoice] AS IV ON IVD.InvoiceId = IV.Id
								LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id = IV.PartyPlantId
								LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.InvoiceDetailId = IVD.Id
								LEFT JOIN [TRN].[Voucher] AS V ON V.Id = IV.VoucherId
								LEFT JOIN [HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id = IVD.GLGeneralInfoId
								LEFT JOIN [MST].[BudgetMaster] AS BM ON BM.Id = IVD.BudgetMasterId
								LEFT JOIN [HKP].[Budget] AS B ON B.Id = BM.BudgetId
								LEFT JOIN [HKP].[Activity] AS A ON A.Id = IVD.ActivityId
								LEFT JOIN [SCS].[Currency] AS C ON C.Id = IV.CurrencyId
								LEFT JOIN [ORG].[Entity] AS EN ON EN.Id = IV.EntityId
								LEFT JOIN (
									SELECT VDC.ParallelCurrencyId AS CompanyCurrencyId
										,VDC.FromCurrencyId AS CompanyFromCurrencyId
										,VDC.ToCurrencyId
										,VDC.ToCurrencyRate AS CompanyCurrencyRate
										,VDC.ToCurrencyConversion AS CompanyCurrencyConversion
										,VDC.DrAmount AS CompanyCurrencyAmount
										,VDC.VoucherDetailId
									FROM [TRN].[VoucherDetailCurrency] AS VDC
									JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId = VDC.ParallelCurrencyId
									WHERE CPC.ParallelCurrencyType = 'CompanyCurrency'
										AND CPC.CompanyId = '" + companyId + @"'
									) AS CC ON CC.VoucherDetailId = VD.Id
								LEFT JOIN (
									SELECT VDC.ParallelCurrencyId AS CompanyGroupCurrencyId
										,VDC.FromCurrencyId AS CompanyGroupFromCurrencyId
										,VDC.ToCurrencyId
										,VDC.ToCurrencyRate AS CompanyGroupCurrencyRate
										,VDC.ToCurrencyConversion AS CompanyGroupCurrencyConversion
										,VDC.DrAmount AS CompanyGroupCurrencyAmount
										,VDC.VoucherDetailId
									FROM [TRN].[VoucherDetailCurrency] AS VDC
									JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId = VDC.ParallelCurrencyId
									WHERE CPC.ParallelCurrencyType = 'CompanyGroupCurrency'
										AND CPC.CompanyId = '" + companyId + @"'
									) AS GC ON GC.VoucherDetailId = VD.Id
								LEFT JOIN (
									SELECT VDC.ParallelCurrencyId AS HardCurrencyId
										,VDC.FromCurrencyId AS HardFromCurrencyId
										,VDC.ToCurrencyId
										,VDC.ToCurrencyRate AS HardCurrencyRate
										,VDC.ToCurrencyConversion AS HardCurrencyConversion
										,VDC.DrAmount AS HardCurrencyAmount
										,VDC.VoucherDetailId
									FROM [TRN].[VoucherDetailCurrency] AS VDC
									JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId = VDC.ParallelCurrencyId
									WHERE CPC.ParallelCurrencyType = 'HardCurrency'
										AND CPC.CompanyId = '" + companyId + @"'
									) AS HC ON HC.VoucherDetailId = VD.Id
								WHERE IV.Archive = 0
									AND IV.IsWrittenOff = 0
									AND IVD.IsWrittenOff = 0
									AND V.IsPark = 0
									AND IVD.IsBlock = 0
									AND IV.SourceType IN (
										'" + SourceType.VendorInvoice + @"'
										,'" + SourceType.SuspensePayable + @"'
										,'" + SourceType.ServicePayable + @"'
										,'" + SourceType.EmployeePayable + @"'
										,'" + SourceType.PostInvoice + @"'
										)
									AND IV.CompanyGroupId = '" + companyGroupId + @"'
									AND IV.CompanyId = '" + companyId + @"'
									" + DatewiseData + @"
								AND IV.Id NOT IN (SELECT InvoiceId FROM InvoiceTaggingWithLCDetail)
								UNION ALL
								
								SELECT IVD.GLGeneralInfoId AS GLGeneralInfoId
									,GLGI.AccountCode AS GLGeneralInfoCode
									,GLGI.UserName AS GLGeneralInfoName
									,IVD.BudgetMasterId
									,B.UserName AS BudgetName
									,IVD.ActivityId
									,EN.UserName AS EntityName
									,A.UserName AS ActivityName
									,V.VoucherNo
									,Replace(CONVERT(VARCHAR(11), IV.DocDate, 106), ' ', '-') DocDate
									,Replace(CONVERT(VARCHAR(11), IV.PostingDate, 106), ' ', '-') PostingDate
									,IV.DocRefNo
									,IV.Narration
									,IV.Id AS InvoiceId
									,EN.Id EntityId
									,VD.PlantId
									,IVD.Id AS InvoiceDetailId
									,IV.VoucherId
									,Replace(CONVERT(VARCHAR(11),IV.ActualDueDate, 106), ' ', '-') ActualDueDate
									,Replace(CONVERT(VARCHAR(11),IV.BaseOnDueDate, 106), ' ', '-') BaseOnDueDate
									,IV.BaseNoOfDays, CASE WHEN  IV.SourceType = 'VendorInvoice' THEN 'Inbound Invoice'  WHEN  IV.SourceType = 'InventoryPayable' THEN  'GRN' END SourceType
									,VD.Id AS VoucherDetailId
									,IV.CurrencyId
									,C.Code AS CurrencyCode
									,IV.PartyId
									,IVD.NetAmount AS Receivable
									,V.ExchangeType
									,0 ExchangeAmount
									,IVD.WrittenOffAmount AS Received
									,IVD.NetAmount - IVD.WrittenOffAmount AS Balance
									, IV.PartyPlantId
									,PP.UserName AS PartyPlantName
									,CC.CompanyCurrencyId
									,CC.CompanyFromCurrencyId
									,CC.ToCurrencyId
									,CC.CompanyCurrencyRate
									,CC.CompanyCurrencyConversion
									,GC.CompanyGroupCurrencyId
									,GC.CompanyGroupFromCurrencyId
									,GC.CompanyGroupCurrencyRate
									,GC.CompanyGroupCurrencyConversion
									,HC.HardCurrencyId
									,HC.HardFromCurrencyId
									,HC.HardCurrencyRate
									,HC.HardCurrencyConversion
									,Particular = REPLACE(REPLACE(STUFF((
													SELECT DISTINCT ',' + xpo.UserName
													FROM hkp.Activity xpo
													INNER JOIN TRN.VoucherDetail xPDAMAP ON xpo.id = xPDAMAP.ActivityId
													WHERE VD.ActivityId != xPDAMAP.ActivityId
														AND xPDAMAP.VoucherId = V.Id
													FOR XML path('')
														, TYPE
													).value('.', 'VARCHAR(MAX)'), 1, 1, ''), '&amp;', '&'), 'amp;', '')
									,NULL AcceptanceNo
									, NULL LCRef
									,NULL ContractNo
									, NULL Customer
									,NULL MasterLCNo
								FROM[TRN].[InvoiceDetail] AS IVD
								LEFT JOIN[TRN].[Invoice] AS IV ON IVD.InvoiceId = IV.Id
								LEFT JOIN[HKP].[PartyPlant] AS PP ON PP.Id = IV.PartyPlantId
								LEFT JOIN[TRN].[VoucherDetail] AS VD ON VD.InvoiceDetailId = IVD.Id
								LEFT JOIN[TRN].[Voucher] AS V ON V.Id = VD.VoucherId
								LEFT JOIN[HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id = IVD.GLGeneralInfoId
								LEFT JOIN[MST].[BudgetMaster] AS BM ON BM.Id = IVD.BudgetMasterId
								LEFT JOIN[HKP].[Budget] AS B ON B.Id = BM.BudgetId
								LEFT JOIN[HKP].[Activity] AS A ON A.Id = IVD.ActivityId
								LEFT JOIN[SCS].[Currency] AS C ON C.Id = IV.CurrencyId
								LEFT JOIN[ORG].[Entity] AS EN ON EN.Id = IV.EntityId
								LEFT JOIN TRN.InventoryReceive IR ON IR.Id = IV.InventoryReceiveId
								LEFT JOIN(
									SELECT VDC.ParallelCurrencyId AS CompanyCurrencyId
										, VDC.FromCurrencyId AS CompanyFromCurrencyId
										, VDC.ToCurrencyId
										, VDC.ToCurrencyRate AS CompanyCurrencyRate
										, VDC.ToCurrencyConversion AS CompanyCurrencyConversion
										, VDC.DrAmount AS CompanyCurrencyAmount
										, VDC.VoucherDetailId
									FROM [TRN].[VoucherDetailCurrency] AS VDC
									JOIN[SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId = VDC.ParallelCurrencyId
									WHERE CPC.ParallelCurrencyType = 'CompanyCurrency'
										AND CPC.CompanyId = '" + companyId + @"'
									) AS CC ON CC.VoucherDetailId = VD.Id
								LEFT JOIN(
									SELECT VDC.ParallelCurrencyId AS CompanyGroupCurrencyId
										, VDC.FromCurrencyId AS CompanyGroupFromCurrencyId
										, VDC.ToCurrencyId
										, VDC.ToCurrencyRate AS CompanyGroupCurrencyRate
										, VDC.ToCurrencyConversion AS CompanyGroupCurrencyConversion
										, VDC.DrAmount AS CompanyGroupCurrencyAmount
										, VDC.VoucherDetailId
									FROM [TRN].[VoucherDetailCurrency] AS VDC
									JOIN[SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId = VDC.ParallelCurrencyId
									WHERE CPC.ParallelCurrencyType = 'CompanyGroupCurrency'
										AND CPC.CompanyId = '" + companyId + @"'
									) AS GC ON GC.VoucherDetailId = VD.Id
								LEFT JOIN(
									SELECT VDC.ParallelCurrencyId AS HardCurrencyId
										, VDC.FromCurrencyId AS HardFromCurrencyId
										, VDC.ToCurrencyId
										, VDC.ToCurrencyRate AS HardCurrencyRate
										, VDC.ToCurrencyConversion AS HardCurrencyConversion
										, VDC.DrAmount AS HardCurrencyAmount
										, VDC.VoucherDetailId
									FROM [TRN].[VoucherDetailCurrency] AS VDC
									JOIN[SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId = VDC.ParallelCurrencyId
									WHERE CPC.ParallelCurrencyType = 'HardCurrency'
										AND CPC.CompanyId = '" + companyId + @"'
									) AS HC ON HC.VoucherDetailId = VD.Id
								WHERE IV.Archive = 0
									AND IV.IsWrittenOff = 0
									AND IVD.IsWrittenOff = 0
									AND V.IsPark = 0
									AND IVD.IsBlock = 0
									AND IV.SourceType IN('" + SourceType.InventoryPayable + @"')
									AND IV.CompanyGroupId = '" + companyGroupId + @"'
									AND IV.CompanyId = '" + companyId + @"'
									AND IR.PurchaseDocumentAcceptanceId IS NULL
									" + DatewiseData + @"
								AND IV.Id NOT IN (SELECT InvoiceId FROM InvoiceTaggingWithLCDetail)";
				return _sqlRepository.GetDataTable(strSQL);
			}
			catch (Exception ex)
			{
				throw (ex);
			}

		}


		[HttpGet, Authorize]
		public ActionResult GetpurchaseLCList()
		{
			try
			{
				var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

				string sql = @"
                        SELECT 
                         PLCV.[Version] PreVersion, PLCV.Amount AmendmentAmount, FORMAT(PLC.AmendmentDate,'dd-MMM-yyyy') AmendmentDate, 
						 PLC.Id,PLC.Version, PLC.ContractId, PLC.VendorId, PLC.BenificiaryBank, PLC.OpeningBankMasterId, PLC.BenificiaryBankDescription, 
                         PLC.LeinBank, PLC.LeinBankDescription, PLC.OrderSpecific, PLC.LCRef, FORMAT(PLC.LCDate,'dd-MMM-yyyy') LCDate,
                         FORMAT(PLC.ExpiryDate,'dd-MMM-yyyy') ExpiryDate, PLC.Amount ,ISNULL(ITLC.LoanAmount,0) LoanAmount,BalanceLCAmount=PLC.Amount-ISNULL(ITLC.LoanAmount,0), PLC.[Type], PLC.Tenure, PLC.CurrencyId, PLC.Rate, PLC.FinalDestination, 
                         PLC.PortOfLandingId, PLC.[Status], PLC.AddedBy, FORMAT(PLC.AddedDate,'dd-MMM-yyyy') AddedDate, PLC.AddedFromIP, PLC.UpdatedBy, FORMAT(PLC.UpdatedDate,'dd-MMM-yyyy') UpdatedDate, PLC.UpdatedFromIP
						,P.UserName PartyName, OB.AccountTitle OpeningBank,CN.Code Currency,PLC.LCANo,PLC.LIBOUR,PLC.InsuranceCoverNoteNo,PLC.InsuranceAttachment,PLC.PaymentBasedOn,C.ContractNo , PLC.InsuranceValue,PLC.IsAccepptanceFirst,PLC.PortOfLoading,PT.UserName CustomerName
						,FORMAT(PLC.ShipmentDate,'dd-MMM-yyyy') ShipmentDate,PLC.PINo,OB.CurrencyId BankCurrency,MLC.LCRef MasterLCNo,C.Remarks ContractRemarks ,PLC.Remarks 
						 FROM [dbo].[PurchaseLC] PLC
                        LEFT JOIN dbo.[Contract] C ON C.Id=PLC.ContractId
                        LEFT JOIN dbo.MasterLC MLC ON MLC.Id=C.MasterLCId
						LEFT JOIN HKP.Party PT ON PT.Id=C.CustomerId
                        LEFT JOIN HKP.Party P  ON P.Id=PLC.VendorId
                        LEFT JOIN MST.BankMaster OB  ON OB.Id=PLC.OpeningBankMasterId
						LEFT JOIN SCS.Currency CN ON CN.Id=PLC.CurrencyId
						LEFT JOIN [dbo].[PurchaseLCVersion] PLCV ON PLCV.PurchaseLCId=PLC.Id   AND PLCV.Id=(SELECT TOP 1 Id FROM [dbo].[PurchaseLCVersion] WHERE PurchaseLCId=PLC.Id  ORDER BY [Version] ASC) 
						LEFT JOIN (SELECT PurchaseLCId,SUM(ISNULL(Amount,0)) LoanAmount FROM InvoiceTaggingWithLCDetail where PurchaseLCId<>'' Group By PurchaseLCId) ITLC ON ITLC.PurchaseLcId=PLC.Id
						Where PLC.PlantId='" + identity.PlantId + "'   ORDER BY PLC.AddedDate DESC";
				var jsondata = Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
				jsondata.MaxJsonLength = int.MaxValue;
				return jsondata;

			}
			catch (Exception ex)
			{
				throw ex;
			}
		}


		[HttpGet, Authorize]
		public ActionResult GetLCSetOffDetailByInvoiceList( string purchaseLCId)
		{
			try
			{
				var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

				string sql = @"
                        SELECT V.VoucherNo,IV.DocRefNo,IV.InventoryReceiveId GRNNo,IRD.POId PONo,IRD.POAmount
						,Replace(CONVERT(VARCHAR(11), IV.PostingDate, 106), ' ', '-') PostingDate,PLC.Amount LCAmount,ITLC.Amount LoanAmount
						,ITLC.AddedBy LoanCreatedBy
						,Replace(CONVERT(VARCHAR(11), ITLC.AddedDate, 106), ' ', '-') LoanCreatedDate,PLC.LCREF LC,OBM.AccountTitle OpeningBank
						FROM InvoiceTaggingWithLCDetail ITLC
						LEFT JOIN [dbo].[PurchaseLC] PLC ON PLC.Id=ITLC.PurchaseLCId
						LEFT JOIN [TRN].[Invoice] IV ON IV.Id=ITLC.InvoiceId
						LEFT JOIN [TRN].[Voucher] V ON V.Id=IV.VoucherId
						LEFT JOIN [MST].[BankMaster] OBM ON OBM.Id=ITLC.OpeningBankMasterId
						LEFT JOIN (SELECT DISTINCT RD.POId,pod.POAmount,RD.InventoryReceiveId FROM TRN.InventoryReceiveDetail RD 
									LEFT JOIN (SELECT InventoryReceiveId,SUM(TransactionAmount) POAmount FROM  TRN.PurchaseOrderDetail group by InventoryReceiveId) pod on pod.InventoryReceiveId=RD.POId
						
						) IRD ON IRD.InventoryReceiveId=IV.InventoryReceiveId
						WHERE ITLC.PurchaseLCId='" + purchaseLCId + @"' ";
				var jsondata = Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
				jsondata.MaxJsonLength = int.MaxValue;
				return jsondata;

			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

		public JsonResult Create(List<Dictionary<string,object>> DataList, Dictionary<string, object> LcData)
        {
            try
            {
                #region Validation
                if (DataList==null)
                {
                    throw new Exception("Select from Invoice list ");
                }
				if (DataList.Count == 0)
				{
					throw new Exception("Select from Invoice list ");
				}
				for (int i = 0; i < DataList.Count; i++)
                {
                    if (DataList[i]["PartyId"].ToString() != LcData["VendorId"].ToString())
                    {
                        throw new Exception("Vendor should be matched with Purchase LC for [" + DataList[i]["PartyPlantName"].ToString() + "]");
                    }
                    if (DataList[i]["CurrencyId"].ToString() != LcData["CurrencyId"].ToString())
                    {
                        throw new Exception("Currency should be matched with Purchase LC for [" + DataList[i]["PartyPlantName"].ToString() + "]");
                    }
                }
                if (Convert.ToBoolean(LcData["IsLoan"]))
                {
                    if (string.IsNullOrEmpty(LcData["LoanNo"].ToString()))
                    {
						throw new Exception("Enter Loan No");
                    }
					if (string.IsNullOrEmpty(LcData["LoanDate"].ToString()))
					{
						throw new Exception("Enter Loan Date");
					}
					if (string.IsNullOrEmpty(LcData["LoanAmount"].ToString()) || Convert.ToDecimal(LcData["LoanAmount"])==0)
					{
						throw new Exception("Enter Loan Amount");
					}
				}
				#endregion
				if (Convert.ToBoolean(LcData["IsLoan"]))
				{
					ep.Save(DataList, LcData);
				}
				else
				{
					ep.SaveWithoutLoan(DataList, LcData);
				}
				
                return Json(new { Error = false, Message = AplosMessage.Updated });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetSaveData()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                return Json(ep.GetMaster(identity.CompanyGroupId,identity.CompanyId,identity.PlantId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

		[HttpGet, Authorize]
		public ActionResult GetInvoiceTaggedWithLCReport(ReportFormat reportFormat, string LCId)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			var workbook = GetInvoiceTaggedWithLCReportFormat(out string reportFileName, LCId);
			switch (reportFormat)
			{
				case ReportFormat.Pdf:
					return RenderReportAsPdf(workbook, reportFileName);

				case ReportFormat.Excel:
					return RenderReportAsExcelx(workbook, reportFileName);

				default:
					return RenderReportAsExcel(workbook, reportFileName); ;
			}
		}

		public IWorkbook GetInvoiceTaggedWithLCReportFormat(out string reportFileName, string LCId)
		{
			var reportUtility = new ReportUtility();
			var excelEngine = new ExcelEngine();
			var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
			workbook.Version = ExcelVersion.Excel2016;
			var sheet = workbook.Worksheets[0];
			sheet.Name = "InvoiceTaggedWithLC";

			var header = GetInvoiceTaggedWithLCHeader(LCId);

			reportFileName = "Invoice Tagged With LC Report";

			var data = GetInvoiceTaggedWithLCQuery(LCId);


			int ROW = 5;
			int xlsCol = 1;
			int colLast = 6;

			reportUtility.SetMasterHeaderText(ref sheet, ROW, 1, "Loan No.");
			sheet.Range[ROW, 1].VerticalAlignment = ExcelVAlign.VAlignTop;
			reportUtility.SetText(ref sheet, ROW, 2, header["LoanNo"].ToString());
			sheet[reportUtility.GetColumnNameForXls(2) + ROW + ":" + reportUtility.GetColumnNameForXls(4) + ROW].Merge();
			sheet.Range[ROW, 2].VerticalAlignment = ExcelVAlign.VAlignTop;
			//sheet.Range[ROW, 1, ROW, colLast].BorderAround(ExcelLineStyle.Hair);
			//sheet.Range[ROW, 1, ROW, colLast].BorderInside(ExcelLineStyle.Hair);

			reportUtility.SetMasterHeaderText(ref sheet, ROW, 5, "Loan Date");
			sheet[ROW, 5].ColumnWidth = 25;
			sheet.Range[ROW, 5].VerticalAlignment = ExcelVAlign.VAlignTop;
			reportUtility.SetText(ref sheet, ROW, 6, header["NewLoanDate"].ToString());
			sheet[ROW, 6].ColumnWidth = 25;
			sheet.Range[ROW, 6].VerticalAlignment = ExcelVAlign.VAlignTop;
			ROW++;

			reportUtility.SetMasterHeaderText(ref sheet, ROW, 1, "Source Type");
			reportUtility.SetText(ref sheet, ROW, 2, header["SourceType"].ToString());
			sheet[reportUtility.GetColumnNameForXls(2) + ROW + ":" + reportUtility.GetColumnNameForXls(4) + ROW].Merge();
			sheet.Range[ROW, 1].VerticalAlignment = ExcelVAlign.VAlignTop;
			sheet.Range[ROW, 2].VerticalAlignment = ExcelVAlign.VAlignTop;
			//sheet.Range[ROW, 1, ROW, colLast].BorderAround(ExcelLineStyle.Hair);
			//sheet.Range[ROW, 1, ROW, colLast].BorderInside(ExcelLineStyle.Hair);

			reportUtility.SetMasterHeaderText(ref sheet, ROW, 5, "LC No.");
			reportUtility.SetText(ref sheet, ROW, 6, header["LCRef"].ToString());
			sheet.Range[ROW, 5].VerticalAlignment = ExcelVAlign.VAlignTop;
			sheet.Range[ROW, 6].VerticalAlignment = ExcelVAlign.VAlignTop;
			ROW++;

			reportUtility.SetMasterHeaderText(ref sheet, ROW, 1, "Bank Master");
			reportUtility.SetText(ref sheet, ROW, 2, header["AccountTitle"].ToString());
			sheet[reportUtility.GetColumnNameForXls(2) + ROW + ":" + reportUtility.GetColumnNameForXls(4) + ROW].Merge();
			sheet.Range[ROW, 1].VerticalAlignment = ExcelVAlign.VAlignTop;
			sheet.Range[ROW, 2].VerticalAlignment = ExcelVAlign.VAlignTop;
			//sheet.Range[ROW, 1, ROW, colLast].BorderAround(ExcelLineStyle.Hair);
			//sheet.Range[ROW, 1, ROW, colLast].BorderInside(ExcelLineStyle.Hair);

			reportUtility.SetMasterHeaderText(ref sheet, ROW, 5, "Currency");
			reportUtility.SetText(ref sheet, ROW, 6, header["CurrencyCode"].ToString());
			sheet.Range[ROW, 5].VerticalAlignment = ExcelVAlign.VAlignTop;
			sheet.Range[ROW, 6].VerticalAlignment = ExcelVAlign.VAlignTop;
			ROW++;


			reportUtility.SetMasterHeaderText(ref sheet, ROW, 1, "Amount");
			reportUtility.SetText(ref sheet, ROW, 2,clsStaticInfo.dbl( header["Amount"].ToString()));
			sheet.Range[ROW, 2].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);
			sheet.Range[ROW, 2].HorizontalAlignment = ExcelHAlign.HAlignLeft;
			sheet[reportUtility.GetColumnNameForXls(2) + ROW + ":" + reportUtility.GetColumnNameForXls(4) + ROW].Merge();
			sheet.Range[ROW, 1].VerticalAlignment = ExcelVAlign.VAlignTop;
			sheet.Range[ROW, 2].VerticalAlignment = ExcelVAlign.VAlignTop;
			//sheet.Range[ROW, 1, ROW, colLast].BorderAround(ExcelLineStyle.Hair);
			//sheet.Range[ROW, 1, ROW, colLast].BorderInside(ExcelLineStyle.Hair);

			reportUtility.SetMasterHeaderText(ref sheet, ROW, 5, "Added By");
			reportUtility.SetText(ref sheet, ROW, 6, header["AddedBy"].ToString());
			sheet.Range[ROW, 5].VerticalAlignment = ExcelVAlign.VAlignTop;
			sheet.Range[ROW, 6].VerticalAlignment = ExcelVAlign.VAlignTop;
			ROW++;
			ROW++;

			int endcolHeader = 8;

			sheet[ROW, xlsCol].Text = "Invoice No.";
			sheet[ROW, xlsCol].ColumnWidth = 25;
			int colAcceptanceNo = xlsCol;
			xlsCol++;

			sheet[ROW, xlsCol].Text = "Invoice Date";
			sheet[ROW, xlsCol].ColumnWidth = 25;
			int colAcceptanceDate = xlsCol;
			xlsCol++;

			sheet[ROW, xlsCol].Text = "Voucher No.";
			sheet[ROW, xlsCol].ColumnWidth = 25;
			int colVoucherNo = xlsCol;
			xlsCol++;

			sheet[ROW, xlsCol].Text = "Posting Date";
			sheet[ROW, xlsCol].ColumnWidth = 25;
			int colPostingDate = xlsCol;
			xlsCol++;

			sheet[ROW, xlsCol].Text = "Vendor";
			sheet[ROW, xlsCol].ColumnWidth = 25;
			int colVendor = xlsCol;
			xlsCol++;

			sheet[ROW, xlsCol].Text = "Amount";
			sheet[ROW, xlsCol].ColumnWidth = 25;
			sheet.Range[ROW, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignRight;
			int colAmount = xlsCol;

			int endCols = xlsCol;
			sheet.Range[ROW, 1, ROW, endCols].CellStyle.Font.Bold = true;
			sheet.Range[ROW, 1, ROW, endCols].CellStyle.Interior.ColorIndex = ExcelKnownColors.Grey_40_percent;
			sheet.Range[ROW, 1, ROW, endCols].BorderAround(ExcelLineStyle.Hair);
			sheet.Range[ROW, 1, ROW, endCols].BorderInside(ExcelLineStyle.Hair);


			var startRow = 0;
			int RowIndex = ROW;
			startRow = ROW;
			ROW++;

			for (int i = 0; i < data.Rows.Count; i++)
			{
				sheet[ROW, colAcceptanceNo].Text = data.Rows[i]["AcceptanceNo"].ToString();
				sheet[ROW, colAcceptanceDate].Text = data.Rows[i]["AcceptanceDate"].ToString();
				sheet[ROW, colVoucherNo].Text = data.Rows[i]["VoucherNo"].ToString();
				sheet[ROW, colPostingDate].Text = data.Rows[i]["PostingDate"].ToString();
				sheet[ROW, colVendor].Text = data.Rows[i]["PartyName"].ToString();
				
				sheet[ROW, colAmount].Number = clsStaticInfo.dbl(data.Rows[i]["Amount"].ToString());
				sheet[ROW, colAmount].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);

				sheet.Range[ROW, 1, ROW, endCols].BorderAround(ExcelLineStyle.Hair);
				sheet.Range[ROW, 1, ROW, endCols].BorderInside(ExcelLineStyle.Hair);

				ROW++;

			}
			reportUtility.SetText(ref sheet, ROW, 5, "Total: ", true);
			sheet[ROW, 5].HorizontalAlignment = ExcelHAlign.HAlignRight;

			sheet[ROW, 6].Formula = "SUM(" + OTSBD.clsStaticInfo.GetxlsCol(colAmount) + startRow.ToString() + ":" + OTSBD.clsStaticInfo.GetxlsCol(colAmount) + (ROW - 1).ToString() + ")";
			sheet[ROW, 6].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);
			sheet[ROW, 6].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet[ROW, 6].HorizontalAlignment = ExcelHAlign.HAlignRight;
			sheet[ROW, 6].CellStyle.Font.Bold = true;

			sheet.Range[ROW, 5, ROW, 6].BorderAround(ExcelLineStyle.Hair);
			sheet.Range[ROW, 5, ROW, 6].BorderInside(ExcelLineStyle.Hair);



			sheet.IsGridLinesVisible = false;
			sheet.UsedRange.WrapText = true;
			sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
			sheet.Range[startRow, 1, ROW, endcolHeader].CellStyle.Font.Size = 8f;

			sheet["A" + startRow.ToString()].FreezePanes();


			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			reportUtility = new ReportUtility();
			//reportUtility.PlantHeader(ref sheet, endcolHeader, "Invoice Tagged With LC", identity.PlantId);
			reportUtility.CompanyPlantHeader(ref sheet, endcolHeader, "Invoice Tagged With LC", identity.CompanyId, identity.PlantId, identity.PlantName, null);
			reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
			//sheet[ROW, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
			sheet.Range[1, 5, 4, endcolHeader].HorizontalAlignment = ExcelHAlign.HAlignLeft;


			return workbook;
		}

		private Dictionary<string, object> GetInvoiceTaggedWithLCHeader(string Id)
		{
			try
			{

				var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

				string strSQL = string.Empty;
				strSQL = @"SELECT 'Invoice' SourceType, LAA.Id LoanAgainstAcceptanceId,LAA.Id, LAA.CompanyGroupId, LAA.CompanyId, 
						LAA.PlantId, LAA.EntityId, LAA.CurrencyId, LAA.VoucherId, 'Vendor' PartyType,LAA.PartyId, 
						LAA.PartyPlantId,'LoanTaken' TransactionType,'Bank' PaymentSource , LAA.LoanDate, LAA.LoanNo, 
						ITLC.Amount, format(LAA.LoanDate,'dd-MMM-yyyy') NewLoanDate,P.UserName PartyName,PP.UserName PartyPlantName ,
						CU.Code CurrencyCode,U.FullName UserName
						,LAA.BankMasterId, BM.AccountTitle, XVD.LCRef,XVD.PINo,LAA.AddedBy
						FROM InvoiceTaggingWithLCMaster LAA 
						LEFT JOIN (SELECT InvoiceTaggingWithLCMasterId,SUM(Amount)Amount FROM InvoiceTaggingWithLCDetail Group By InvoiceTaggingWithLCMasterId) ITLC ON LAA.Id=ITLC.InvoiceTaggingWithLCMasterId
						LEFT JOIN HKP.Party P ON P.Id=LAA.PartyId 
						LEFT JOIN HKP.PartyPlant PP ON PP.Id=LAA.PartyPlantId
						LEFT JOIN SCS.Currency CU ON CU.Id=LAA.CurrencyId
						LEFT JOIN SEC.[USER] U ON U.UserId=LAA.AddedBy
						LEFT JOIN MST.BankMaster BM ON BM.Id=LAA.BankMasterId
						LEFT JOIN dbo.PurchaseLC XVD ON XVD.Id=LAA.PurchaseLCId
						WHERE LAA.IsLoan=1 AND  
						--LAA.VoucherId IS NULL and
						LAA.PlantId='" + identity.PlantId + @"' 
						and LAA.Id='" + Id + "'";
				return _sqlRepository.GetData(strSQL);
			}
			catch (Exception ex)
			{
				throw (ex);
			}
		}

		private DataTable GetInvoiceTaggedWithLCQuery(string LoanAgainstAcceptanceMasterId)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;


			var sql = @"SELECT LAA.Id LoanAgainstAcceptanceId,LAA.CurrencyId, format(LAA.LoanDate,'dd-MMM-yyyy') NewLoanDate,P.UserName PartyName,PP.UserName PartyPlantName ,CU.Code CurrencyCode,U.FullName UserName
						,IVD.GLGeneralInfoId,IVD.BudgetMasterId,IVD.ActivityId,IVD.InvoiceId,IVD.Id InvoiceDetailId,LAAD.Amount
						,IV.CompanyCurrencyRate,BM.AccountTitle,IV.DocRefNo  AcceptanceNo,FORMAT(V.PostingDate,'dd-MMM-yyyy') AcceptanceDate
						,V.VoucherNo,LAA.BankMasterId,isnull( Format( V.PostingDate,'dd-MMM-yyyy'),'') as PostingDate
						FROM InvoiceTaggingWithLCMaster LAA 
						LEFT JOIN InvoiceTaggingWithLCDetail LAAD ON LAA.Id=LAAD.InvoiceTaggingWithLCMasterId
						LEFT JOIN HKP.Party P ON P.Id=LAA.PartyId 
						LEFT JOIN HKP.PartyPlant PP ON PP.Id=LAA.PartyPlantId
						LEFT JOIN MST.BankMaster BM ON BM.Id=LAA.BankMasterId
						LEFT JOIN SCS.Currency CU ON CU.Id=LAA.CurrencyId
						LEFT JOIN TRN.Invoice IV ON IV.Id=LAAD.InvoiceId
						LEFT JOIN TRN.Voucher V on V.Id=IV.VoucherId
						LEFT JOIN TRN.InvoiceDetail IVD ON IVD.InvoiceId=IV.Id
						LEFT JOIN SEC.[USER] U ON U.UserId=LAA.AddedBy
						WHERE LAA.PlantId='" + identity.PlantId + @"' AND LAAD.InvoiceTaggingWithLCMasterId='" + LoanAgainstAcceptanceMasterId + @"'  
						--AND LAA.VoucherId IS NULL
						";

			return _sqlRepository.GetDataTable(sql);
		}
		#endregion
		[HttpPost]
		public ActionResult UntagInvoiceWithLC(string untageId,string voucherId,string VoucherNo)
		{
			try
			{
				var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                if (voucherId == null)
                {
					var rdBuilder = new System.Text.StringBuilder();
					var builderSqlDetail = @"DELETE InvoiceTaggingWithLCDetail where InvoiceTaggingWithLCMasterId='" + untageId + "'";
					var builderSql = @"DELETE InvoiceTaggingWithLCMaster where Id='" + untageId + "'";
					rdBuilder.Append(builderSqlDetail);
					rdBuilder.Append(builderSql);
					_sqlRepository.ExecuteSqlCommand(rdBuilder.ToString());
					return Json(new { Message = "Please Delete VoucherNo " + VoucherNo + " first !" });
				}
                else
                {
					return Json(new { Error = true, Message = "Please Delete VoucherNo " + VoucherNo + " first !" });
					 
				}
				
			}
			catch (CustomException)
			{
				throw;
			}
			
		}
	}
}