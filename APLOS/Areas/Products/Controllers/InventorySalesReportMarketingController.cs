using System.Web.Mvc;
using Aplos.Controllers;
using Aplos.Properties;
using Library.MaterialManagement.Inventory;
using Library.Model.Inventory;
using Library.Core;
using Library.Crosscutting.Security;
using System.Threading;
using Library.ViewModel.Materials;
using System.Collections.Generic;
using System.Linq;
using Library.Model.Enums;
using Library.MaterialManagement.Reports;
using System;
using Library.Data;
using Library.Service.Enums;
using Library.Service.Logs;
using System.Reflection;
using Library.Data.Sql;
using Library.Model.Parties;
using Library.Data.Repositories;
using Syncfusion.XlsIO;
using Library.Service.Helpers;
using System.Data;
using Library.Service.Currencies;
using Library.Service.Organizations;
using OTSBD;
using Syncfusion.ExcelToPdfConverter;
using Syncfusion.Pdf;
using Library.MaterialManagement.InventoryManagements;
using Library.Accounting.Accounts;
using Newtonsoft.Json;
using Aplos.MaterialManagement.MaterialQuery;
using Library.OrderManagement.Sales;

namespace Aplos.Areas.Products.Controllers
{
    public class InventorySalesReportMarketingController : BaseController
    {
        #region Constructor

        private readonly IInventoryIssueService _inventoryIssueService;
        private readonly IInventoryIssueDetailService _inventoryDetailService;
        private readonly IInventoryMaterialService _inventoryMaterialService;
        private readonly IInventoryReceiveService _inventoryReveiveService;
        private readonly ISqlRepository _sqlRepository;
        private readonly IRepositoryAsync<CompanyParty> _companyPartyRepository;
        private readonly CompanyParallelCurrencyService _companyParallelCurrencyService;
        private readonly IPlantService _plantService;

        clsSales clsSales = new clsSales();

        public InventorySalesReportMarketingController(IInventoryIssueService inventoryIssueService
            , IInventoryIssueDetailService inventoryDetailService
            , IInventoryMaterialService inventoryMaterialService
            , IInventoryReceiveService inventoryReveiveService
            , IRepositoryAsync<CompanyParty> companyPartyRepository
            , CompanyParallelCurrencyService companyParallelCurrencyService
            , IPlantService plantService
            , ISqlRepository sqlRepository)
        {
            _inventoryIssueService = inventoryIssueService;
            _inventoryDetailService = inventoryDetailService;
            _inventoryMaterialService = inventoryMaterialService;
            _inventoryReveiveService = inventoryReveiveService;
            _sqlRepository = sqlRepository;
            _companyPartyRepository = companyPartyRepository;
            _companyParallelCurrencyService = companyParallelCurrencyService;
            _plantService = plantService;
        }

        #endregion Constructor

        #region Aplos

        public ActionResult Aplos()
        {
            return View();
        }
       
        #endregion Aplos

        [Authorize, HttpGet]
        public ActionResult InventorySalesReportExcel(ReportFormat reportFormat, string plantId, string fromDate, string toDate, string Qty, string Amount, string RcptIssue, string Asset, string Inventory, string Summary, bool WithTax, string Type, string partyId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            plantId = identity.PlantId;
            var reportFileName = "Sales Register.xls" + fromDate + "To" + toDate + "";
            ExcelEngine excelEngine = new ExcelEngine();

            IWorkbook workbook = InventorySalesReportList(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, fromDate, toDate, Qty, Amount, Summary, WithTax, Type, partyId);
            switch (reportFormat)
            {
                case ReportFormat.Pdf:
                    return RenderReportAsPdf(workbook, reportFileName);

                case ReportFormat.Excel:
                    return RenderReportAsExcel(workbook, reportFileName);

                default:
                    return View();
            }
        }

		[HttpGet, Authorize]
		public DataTable GetInventorySalesReportData(string CompanyGroupId, string CompanyId, string PlantId, string fromDate, string toDate, string Qty, string Amount, string Summary, string Type, string partyId)
		{
            //return Json(clsSales.GetInventorySalesReportData(CompanyGroupId, CompanyId, PlantId, fromDate, toDate, Qty, Amount, Summary, Type, partyId), JsonRequestBehavior.AllowGet);
            return clsSales.GetInventorySalesReportData(CompanyGroupId, CompanyId, PlantId, fromDate, toDate, Qty, Amount, Summary, Type, partyId);

        }
		

        public string NumberFormatZeroDecimal = "#,##0.00;(#,##0)";
        public string NumberFormatTwoDecimal = "#,##0.00;(#,##0.00)";
        public string NumberFormatFourDecimal = "#,####0.0000;(#,####0.0000)";
        [Authorize, HttpGet]
        private IWorkbook InventorySalesReportList(string companyGroupId, string companyId, string plantId, string fromDate, string toDate, string Qty, string Amount, string Summary, bool WithTax, string Type, string partyId)
        {

            //Start EmployeeAdvanceDueList
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                ExcelEngine excelEngine = new ExcelEngine();
                //Instantiate the Excel application object
                IApplication application = excelEngine.Excel;

                //Set the default application version
                application.DefaultVersion = ExcelVersion.Excel2013;

                //Load the existing Excel workbook into IWorkbook
                IWorkbook workbook = application.Workbooks.Create(1);

                //Get the first worksheet in the workbook into IWorksheet
                IWorksheet worksheet = workbook.Worksheets[0];
                DataTable dtInventorySalesReportList = GetInventorySalesReportData(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, fromDate, toDate, Qty, Amount, Summary, Type, partyId);

                if (dtInventorySalesReportList.Rows.Count == 0)
                    throw new Exception("No data found");
                // throw new Exception("To date must be above or equal to From Date.");



                worksheet.Name = Summary;

                var _rowd = 4;
                if (fromDate != "" && toDate != "")
                {

                    worksheet.Range[_rowd, 3, _rowd, 6].Text = fromDate + " " + "To" + " " + toDate;
                    worksheet.Range[_rowd, 3, _rowd, 6].CellStyle.Font.Size = 8;
                    worksheet.Range[_rowd, 3, _rowd, 6].CellStyle.Font.Bold = false;
                    worksheet.Range[_rowd, 3, _rowd, 6].Merge();
                }

                else
                {

                    worksheet[_rowd, 4].Text = toDate;
                    worksheet[_rowd, 4].CellStyle.Font.Size = 8;
                    worksheet.Range[_rowd, 3, _rowd, 4].CellStyle.Font.Bold = false;
                    worksheet.Range[_rowd, 3, _rowd, 4].Merge();
                    //sheet1.Range[_rowd, 3, _rowd , 3].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                }

                var _rows = 5;
                worksheet.Range[_rows, 3, _rows, 6].Text = "Report Ref No: ";
                worksheet.Range[_rows, 3, _rows, 6].CellStyle.Font.Size = 8;
                worksheet.Range[_rowd, 3, _rowd, 4].CellStyle.Font.Bold = false;
                worksheet.Range[_rows, 3, _rows, 6].Merge();
                worksheet.Range[_rows, 3, _rows, 6].CellStyle.Font.Bold = false;
                _rows++;



                int COL = 1; int ROW = 7;
                int startCol = COL;

                if (Summary == "Details")
                {
                    worksheet[ROW, COL].Text = "SL"; //1
                    int colSL = COL;
                    worksheet[ROW, COL].ColumnWidth = 5;
                    worksheet[ROW, COL].CellStyle.Font.Bold = true;
                    worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    COL++;
					worksheet[ROW, COL].Text = "Sales Invoice No.";//2
					int colSalesId = COL;
					worksheet[ROW, COL].ColumnWidth = 15;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Invoice Date";//3
					int colInvoiceDate = COL;
					worksheet[ROW, COL].ColumnWidth = 20;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;

					worksheet[ROW, COL].Text = "Master Order Id";//4
					int colMasterOrderId = COL;
					worksheet[ROW, COL].ColumnWidth = 20;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;

					worksheet[ROW, COL].Text = "Sales Order Id";//5
					int colSalesOrderId = COL;
					worksheet[ROW, COL].ColumnWidth = 20;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;

					worksheet[ROW, COL].Text = "SourceType";//6
					int colSourceType = COL;
					worksheet[ROW, COL].ColumnWidth = 15;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;

					worksheet[ROW, COL].Text = "Customer Name";//7
					int colPartyName = COL;
					worksheet[ROW, COL].ColumnWidth = 30;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++; 
					
					worksheet[ROW, COL].Text = "Destination Name";//8
					int colDestinationName = COL;
					worksheet[ROW, COL].ColumnWidth = 12;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;

					worksheet[ROW, COL].Text = "Material Group";//9
					int colMaterialGroupMasterName = COL;
					worksheet[ROW, COL].ColumnWidth = 30;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Material Master";//10
					int colMaterialMasterName = COL;
					worksheet[ROW, COL].ColumnWidth = 20;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Article";//11
					int colArticleName = COL;
					worksheet[ROW, COL].ColumnWidth = 20;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Product Details";//12
					int colProdDetail = COL;
					worksheet[ROW, COL].ColumnWidth = 50;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "LotNo";//13
					int colLotNo = COL;
					worksheet[ROW, COL].ColumnWidth = 20;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;

					worksheet[ROW, COL].Text = "Buyer Ref. No.";//14
					int colBuyRef = COL;
					worksheet[ROW, COL].ColumnWidth = 20;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;

					worksheet[ROW, COL].Text = "Cartons/Bags";//15
					int colBags = COL;
					worksheet[ROW, COL].ColumnWidth = 20;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Transaction Qty";//16
					int colTransactionQty = COL;
					worksheet[ROW, COL].ColumnWidth = 20;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Gross Weight";//17
					int colGrossWeight = COL;
					worksheet[ROW, COL].ColumnWidth = 15;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					//worksheet[ROW, COL].Text = "Currency";//18
					//int colCurrency = COL;
					//worksheet[ROW, COL].ColumnWidth = 12;
					//worksheet[ROW, COL].CellStyle.Font.Bold = true;
					//worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					//worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					//COL++;
					worksheet[ROW, COL].Text = "Transaction Rate";//18
					int colTransactionRate = COL;
					worksheet[ROW, COL].ColumnWidth = 20;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;

					worksheet[ROW, COL].Text = "Transaction Amount";//19
					int colTransactionAmount = COL;
					worksheet[ROW, COL].ColumnWidth = 20;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Tax Amount";//20
					int colTaxAmount = COL;
					worksheet[ROW, COL].ColumnWidth = 20;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;

					worksheet[ROW, COL].Text = "Net Amount";//21
					int colNetAmount = COL;
					worksheet[ROW, COL].ColumnWidth = 20;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;

					worksheet[ROW, COL].Text = "Service Charge";//22
					int colServiceCharge = COL;
					worksheet[ROW, COL].ColumnWidth = 12;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Service Tax";//23
					int colServiceTax = COL;
					worksheet[ROW, COL].ColumnWidth = 12;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Exchange Rate";//24
					int colToCurrencyRate = COL;
					worksheet[ROW, COL].ColumnWidth = 20;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;

                  

                    int colCGST = 0;
                    int colCGSTTax = 0;
                    int colSGST = 0;
                    int colSGSTTax = 0;
                    int colIGST = 0;
                    int colIGSTTax = 0;

                    

					worksheet[ROW, COL].Text = "Transporter Name";//25
					int colTransporterName = COL;
					worksheet[ROW, COL].ColumnWidth = 20;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Vehicle No.";//26
					int colVehicleNo = COL;
					worksheet[ROW, COL].ColumnWidth = 20;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Transpoter Doc Ref No.";//27
					int colTranspoterDocRefNo = COL;
					worksheet[ROW, COL].ColumnWidth = 25;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Transporter Doc Ref No. Date";//28
					int colTransporterDocRefDate = COL;
					worksheet[ROW, COL].ColumnWidth = 30;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Driver no";//29
					int colDriverNo = COL;
					worksheet[ROW, COL].ColumnWidth = 30;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;

					worksheet[ROW, COL].Text = "Container No.";//30
					int colContainer = COL;
					worksheet[ROW, COL].ColumnWidth = 15;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;


					worksheet[ROW, COL].Text = "Bill To";//31
					int colBillTo = COL;
					worksheet[ROW, COL].ColumnWidth = 30;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;


					worksheet[ROW, COL].Text = "Bill To Address";//32
					int colBillToAddress = COL;
					worksheet[ROW, COL].ColumnWidth = 30;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;


					worksheet[ROW, COL].Text = "Bill To State";//33
					int colBillToState = COL;
					worksheet[ROW, COL].ColumnWidth = 30;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;

					worksheet[ROW, COL].Text = "Bill To GST No.";//34
					int colBillToGstNo = COL;
					worksheet[ROW, COL].ColumnWidth = 30;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;

					worksheet[ROW, COL].Text = "Ship To";//35
					int colShipTo = COL;
					worksheet[ROW, COL].ColumnWidth = 30;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;

					worksheet[ROW, COL].Text = "Ship To Address";//36
					int colShipToAddress = COL;
					worksheet[ROW, COL].ColumnWidth = 20;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Ship To State";//37
					int colShipToState = COL;
					worksheet[ROW, COL].ColumnWidth = 15;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Ship To GST No.";//38
					int colShipToGSTNo = COL;
					worksheet[ROW, COL].ColumnWidth = 15;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Agent Name";//39
					int colAgentName = COL;
					worksheet[ROW, COL].ColumnWidth = 20;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Agent Commission %";//40
					int colAgentCommission = COL;
					worksheet[ROW, COL].ColumnWidth = 25;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Payment Term";//41
					int colPaymentTerm = COL;
					worksheet[ROW, COL].ColumnWidth = 30;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Base on Due Date";//42
					int colBaseOnDueDate = COL;
					worksheet[ROW, COL].ColumnWidth = 30;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;

					worksheet[ROW, COL].Text = "Customer PONo";//Last
					int colPONo = COL;
					worksheet[ROW, COL].ColumnWidth = 20;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;

                    

                    int endCol = COL;
                    worksheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 10f;
                    worksheet.Range[ROW, 1, ROW, endCol].RowHeight = 22;
                    worksheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                    worksheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                    worksheet.Range[ROW, startCol, ROW, COL].CellStyle.ColorIndex = ExcelKnownColors.Grey_40_percent;
                    ROW++;
                    try
                    {
                        if (Summary == "Details")
                        {
                            for (int i = 0; i < dtInventorySalesReportList.Rows.Count; i++)
                            {

                                // int i = 0; i < dtMasterOrderItem.Rows.Count; i++
                                worksheet[ROW, colSL].Text = dtInventorySalesReportList.Rows[i]["S.N"].ToString();
                                
                                worksheet[ROW, colSourceType].Text = dtInventorySalesReportList.Rows[i]["SourceType"].ToString();

                                worksheet[ROW, colSalesId].Text = dtInventorySalesReportList.Rows[i]["SalesId"].ToString();
                                
                                worksheet[ROW, colInvoiceDate].Text = dtInventorySalesReportList.Rows[i]["InvoiceDate"].ToString();

                                worksheet[ROW, colSalesOrderId].Text = dtInventorySalesReportList.Rows[i]["SalesOrderId"].ToString();
                                worksheet[ROW, colMasterOrderId].Text = dtInventorySalesReportList.Rows[i]["MasterOrderId"].ToString();
                                
                                worksheet[ROW, colPONo].Text = dtInventorySalesReportList.Rows[i]["PONumber"].ToString();
                                worksheet[ROW, colBillTo].Text = dtInventorySalesReportList.Rows[i]["BillTo"].ToString();
                                worksheet[ROW, colBillToAddress].Text = dtInventorySalesReportList.Rows[i]["BillToAddress"].ToString();
                                worksheet[ROW, colBillToState].Text = dtInventorySalesReportList.Rows[i]["BillToState"].ToString();
                                worksheet[ROW, colBillToGstNo].Text = dtInventorySalesReportList.Rows[i]["BillToGSTNo"].ToString();
                                worksheet[ROW, colShipTo].Text = dtInventorySalesReportList.Rows[i]["ShipTo"].ToString();
                                worksheet[ROW, colShipToAddress].Text = dtInventorySalesReportList.Rows[i]["ShipToAddress"].ToString();
                                worksheet[ROW, colShipToState].Text = dtInventorySalesReportList.Rows[i]["ShipToState"].ToString();
                                worksheet[ROW, colShipToGSTNo].Text = dtInventorySalesReportList.Rows[i]["ShipToGSTNo"].ToString();

                                worksheet[ROW, colToCurrencyRate].Number = clsStaticInfo.dbl(dtInventorySalesReportList.Rows[i]["ToCurrencyRate"].ToString());
                                worksheet.Range[ROW, colToCurrencyRate].NumberFormat = NumberFormatFourDecimal;
                               
                                worksheet[ROW, colPartyName].Text = dtInventorySalesReportList.Rows[i]["PartyName"].ToString();
                                
                                worksheet[ROW, colMaterialGroupMasterName].Text = dtInventorySalesReportList.Rows[i]["MaterialGroupMasterName"].ToString();
                                worksheet[ROW, colMaterialMasterName].Text = dtInventorySalesReportList.Rows[i]["MaterialMasterName"].ToString();
                                
                                worksheet[ROW, colArticleName].Text = dtInventorySalesReportList.Rows[i]["MaterialMasterArticleName"].ToString();
                               
                                worksheet[ROW, colTransactionRate].Number = clsStaticInfo.dbl(dtInventorySalesReportList.Rows[i]["TransactionRate"].ToString());
                                worksheet.Range[ROW, colTransactionRate].NumberFormat = NumberFormatFourDecimal;
                                worksheet[ROW, colTransactionQty].Number = clsStaticInfo.dbl(dtInventorySalesReportList.Rows[i]["TransactionQty"].ToString());
                                worksheet.Range[ROW, colTransactionQty].NumberFormat = NumberFormatTwoDecimal;
                                worksheet[ROW, colTransactionAmount].Number = clsStaticInfo.dbl(dtInventorySalesReportList.Rows[i]["TransactionAmount"].ToString());
                                worksheet.Range[ROW, colTransactionAmount].NumberFormat = NumberFormatTwoDecimal;
                                worksheet[ROW, colTaxAmount].Number = clsStaticInfo.dbl(dtInventorySalesReportList.Rows[i]["TaxAmount"].ToString());
                                worksheet.Range[ROW, colTaxAmount].NumberFormat = NumberFormatTwoDecimal;
								worksheet[ROW, colContainer].Text = dtInventorySalesReportList.Rows[i]["ContainerNo"].ToString();
								/// In The Query The Transpoert Name is actually the Agent Name 
								worksheet[ROW, colTransporterName].Text = dtInventorySalesReportList.Rows[i]["AgentName"].ToString();
								worksheet[ROW, colTranspoterDocRefNo].Text = dtInventorySalesReportList.Rows[i]["TransportDocRefNo"].ToString();
								worksheet[ROW, colTransporterDocRefDate].Text = dtInventorySalesReportList.Rows[i]["TransportDocDate"].ToString();
								worksheet[ROW, colAgentName].Text = dtInventorySalesReportList.Rows[i]["TransporterName"].ToString();
								worksheet[ROW, colAgentCommission].Text = dtInventorySalesReportList.Rows[i]["AgentCommission"].ToString();
								worksheet[ROW, colGrossWeight].Text = dtInventorySalesReportList.Rows[i]["GrossWeights"].ToString();
								worksheet[ROW, colLotNo].Text = dtInventorySalesReportList.Rows[i]["LOT"].ToString();
								worksheet[ROW, colPaymentTerm].Text = dtInventorySalesReportList.Rows[i]["PaymentTerm"].ToString();
								worksheet[ROW, colBaseOnDueDate].Text = dtInventorySalesReportList.Rows[i]["BaseOnDueDate"].ToString();
								worksheet[ROW, colServiceCharge].Number = clsStaticInfo.dbl(dtInventorySalesReportList.Rows[i]["ServiceCharge"].ToString());
								worksheet.Range[ROW, colServiceCharge].NumberFormat = NumberFormatTwoDecimal;
								worksheet[ROW, colServiceTax].Number = clsStaticInfo.dbl(dtInventorySalesReportList.Rows[i]["ServiceTax"].ToString());
								worksheet.Range[ROW, colServiceTax].NumberFormat = NumberFormatTwoDecimal;

								worksheet[ROW, colNetAmount].Number = clsStaticInfo.dbl(dtInventorySalesReportList.Rows[i]["NetAmount"].ToString());
								worksheet.Range[ROW, colNetAmount].NumberFormat = NumberFormatTwoDecimal;
								//worksheet[ROW, colCurrency].Text = dtInventorySalesReportList.Rows[i]["Currency"].ToString();
								worksheet[ROW, colDestinationName].Text = dtInventorySalesReportList.Rows[i]["DestinationName"].ToString();

								worksheet[ROW, colBuyRef].Text = dtInventorySalesReportList.Rows[i]["BuyerRefNo"].ToString();
								worksheet[ROW, colProdDetail].Text = dtInventorySalesReportList.Rows[i]["PordDertails"].ToString();

								worksheet[ROW, colBags].Number = clsStaticInfo.dbl(dtInventorySalesReportList.Rows[i]["Bags"].ToString());
								worksheet.Range[ROW, colBags].NumberFormat = NumberFormatTwoDecimal;
								worksheet[ROW, colVehicleNo].Number = clsStaticInfo.dbl(dtInventorySalesReportList.Rows[i]["TransportVehicleNo"].ToString());
								worksheet.Range[ROW, colVehicleNo].NumberFormat = NumberFormatTwoDecimal;
								worksheet[ROW, colDriverNo].Number = clsStaticInfo.dbl(dtInventorySalesReportList.Rows[i]["TransportDriverNo"].ToString());
								worksheet.Range[ROW, colDriverNo].NumberFormat = NumberFormatTwoDecimal;

								

								worksheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                                worksheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                                worksheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                                ROW++;
                            }
                            worksheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                            //worksheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                            worksheet["A" + 7].FreezePanes();
                            ReportUtility reportUtility = new ReportUtility();
                            reportUtility.PlantHeader(ref worksheet, endCol, " Sales Register", identity.PlantId);
                            reportUtility.PageSetup(ref worksheet, 5, ExcelPageOrientation.Landscape);
                            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                            // worksheet.Range[1, 1, 4, endCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            worksheet.Range[1, 1, 3, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                        }
                    }
                    catch (Exception ex)
                    {

                        throw ex;
                    }
                }
                else
                {
                    worksheet[ROW, COL].Text = "SL";
                    int colSL = COL;
                    worksheet[ROW, COL].ColumnWidth = 5;
                    worksheet[ROW, COL].CellStyle.Font.Bold = true;
                    worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    COL++;
                    worksheet[ROW, COL].Text = "Id";
                    int colId = COL;
                    worksheet[ROW, COL].ColumnWidth = 10;
                    worksheet[ROW, COL].CellStyle.Font.Bold = true;
                    worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    COL++;
                    worksheet[ROW, COL].Text = "SourceType";
                    int colSourceType = COL;
                    worksheet[ROW, COL].ColumnWidth = 15;
                    worksheet[ROW, COL].CellStyle.Font.Bold = true;
                    worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    COL++;
                    worksheet[ROW, COL].Text = "Entry Date";
                    int colSalesDate = COL;
                    worksheet[ROW, COL].ColumnWidth = 15;
                    worksheet[ROW, COL].CellStyle.Font.Bold = true;
                    worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    COL++;
                    worksheet[ROW, COL].Text = "Invloice Date";
                    int colInvoiceDate = COL;
                    worksheet[ROW, COL].ColumnWidth = 15;
                    worksheet[ROW, COL].CellStyle.Font.Bold = true;
                    worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    COL++;

                    worksheet[ROW, COL].Text = "Bill To";
                    int colBillTo = COL;
                    worksheet[ROW, COL].ColumnWidth = 30;
                    worksheet[ROW, COL].CellStyle.Font.Bold = true;
                    worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    COL++;

                    worksheet[ROW, COL].Text = "Ship To";
                    int colShipTo = COL;
                    worksheet[ROW, COL].ColumnWidth = 30;
                    worksheet[ROW, COL].CellStyle.Font.Bold = true;
                    worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    COL++;
                    worksheet[ROW, COL].Text = "Doc Ref No";
                    int colDocRefNo = COL;
                    worksheet[ROW, COL].ColumnWidth = 30;
                    worksheet[ROW, COL].CellStyle.Font.Bold = true;
                    worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    COL++;

                    worksheet[ROW, COL].Text = "Doc Date";
                    int colDocDate = COL;
                    worksheet[ROW, COL].ColumnWidth = 30;
                    worksheet[ROW, COL].CellStyle.Font.Bold = true;
                    worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    COL++;

                    worksheet[ROW, COL].Text = "Customer Name";
                    int colPartyName = COL;
                    worksheet[ROW, COL].ColumnWidth = 30;
                    worksheet[ROW, COL].CellStyle.Font.Bold = true;
                    worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    COL++;

                    worksheet[ROW, COL].Text = "Customer Code";
                    int colPartyCode = COL;
                    worksheet[ROW, COL].ColumnWidth = 30;
                    worksheet[ROW, COL].CellStyle.Font.Bold = true;
                    worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    COL++;
                    worksheet[ROW, COL].Text = "Customer PO Number";
                    int colCustomerPONumber = COL;
                    worksheet[ROW, COL].ColumnWidth = 30;
                    worksheet[ROW, COL].CellStyle.Font.Bold = true;
                    worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    COL++;
                    worksheet[ROW, COL].Text = "Master Order Number";
                    int colMasterOrderNumber = COL;
                    worksheet[ROW, COL].ColumnWidth = 30;
                    worksheet[ROW, COL].CellStyle.Font.Bold = true;
                    worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    COL++;
                    worksheet[ROW, COL].Text = "Sales Order Number";
                    int colSalesOrderNumber = COL;
                    worksheet[ROW, COL].ColumnWidth = 30;
                    worksheet[ROW, COL].CellStyle.Font.Bold = true;
                    worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    COL++;
                    worksheet[ROW, COL].Text = "Tran. Currency";
                    int colCurrency = COL;
                    worksheet[ROW, COL].ColumnWidth = 12;
                    worksheet[ROW, COL].CellStyle.Font.Bold = true;
                    worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    COL++;
                    worksheet[ROW, COL].Text = "Exchange Rate";
                    int colToCurrencyRate = COL;
                    worksheet[ROW, COL].ColumnWidth = 20;
                    worksheet[ROW, COL].CellStyle.Font.Bold = true;
                    worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    COL++;
                    worksheet[ROW, COL].Text = "Mat.Amt";
                    int colMatAmt = COL;
                    worksheet[ROW, COL].ColumnWidth = 12;
                    worksheet[ROW, COL].CellStyle.Font.Bold = true;
                    worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    COL++;
                    worksheet[ROW, COL].Text = "Serv. Amt";
                    int colServAmt = COL;
                    worksheet[ROW, COL].ColumnWidth = 12;
                    worksheet[ROW, COL].CellStyle.Font.Bold = true;
                    worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    COL++;
                    worksheet[ROW, COL].Text = "Ttl. Taxable Amt.";
                    int colTransactionAmount = COL;
                    worksheet[ROW, COL].ColumnWidth = 20;
                    worksheet[ROW, COL].CellStyle.Font.Bold = true;
                    worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    COL++;
                    int colCGST = 0;
                    int colSGST = 0;
                    int colIGST = 0;
                    int colTCS = 0;
                    int colBooksCGST = 0;
                    int colBooksSGST = 0;
                    int colBooksIGST = 0;
                    int colBooksTCS = 0;



                    if (WithTax == true)
                    {
                        worksheet[ROW, COL].Text = "CGST";
                        colCGST = COL;
                        worksheet[ROW, COL].ColumnWidth = 20;
                        worksheet[ROW, COL].CellStyle.Font.Bold = true;
                        worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                        worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        COL++;

                        worksheet[ROW, COL].Text = "SGST";
                        colSGST = COL;
                        worksheet[ROW, COL].ColumnWidth = 20;
                        worksheet[ROW, COL].CellStyle.Font.Bold = true;
                        worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                        worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        COL++;
                        worksheet[ROW, COL].Text = "IGST";
                        colIGST = COL;
                        worksheet[ROW, COL].ColumnWidth = 20;
                        worksheet[ROW, COL].CellStyle.Font.Bold = true;
                        worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                        worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        COL++;
                        worksheet[ROW, COL].Text = "TCS";
                        colTCS = COL;
                        worksheet[ROW, COL].ColumnWidth = 20;
                        worksheet[ROW, COL].CellStyle.Font.Bold = true;
                        worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                        worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        COL++;
                    }

                    worksheet[ROW, COL].Text = "Books Mat.Amt";
                    int colBooksMatAmt = COL;
                    worksheet[ROW, COL].ColumnWidth = 12;
                    worksheet[ROW, COL].CellStyle.Font.Bold = true;
                    worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    COL++;

                    worksheet[ROW, COL].Text = "Books Serv. Amt";
                    int colBooksServAmt = COL;
                    worksheet[ROW, COL].ColumnWidth = 12;
                    worksheet[ROW, COL].CellStyle.Font.Bold = true;
                    worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    COL++;

                    worksheet[ROW, COL].Text = "Books Ttl. Taxable Amt.";
                    int colBooksTtlTaxableAmt = COL;
                    worksheet[ROW, COL].ColumnWidth = 20;
                    worksheet[ROW, COL].CellStyle.Font.Bold = true;
                    worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    COL++;

                    if (WithTax == true)
                    {
                        worksheet[ROW, COL].Text = "Books CGST";
                        colBooksCGST = COL;
                        worksheet[ROW, COL].ColumnWidth = 20;
                        worksheet[ROW, COL].CellStyle.Font.Bold = true;
                        worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                        worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        COL++;


                        worksheet[ROW, COL].Text = "Books SGST";
                        colBooksSGST = COL;
                        worksheet[ROW, COL].ColumnWidth = 20;
                        worksheet[ROW, COL].CellStyle.Font.Bold = true;
                        worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                        worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        COL++;


                        worksheet[ROW, COL].Text = "Books IGST";
                        colBooksIGST = COL;
                        worksheet[ROW, COL].ColumnWidth = 20;
                        worksheet[ROW, COL].CellStyle.Font.Bold = true;
                        worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                        worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        COL++;


                        worksheet[ROW, COL].Text = "Books TCS";
                        colBooksTCS = COL;
                        worksheet[ROW, COL].ColumnWidth = 20;
                        worksheet[ROW, COL].CellStyle.Font.Bold = true;
                        worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                        worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        COL++;
                    }

                    worksheet[ROW, COL].Text = "VoucherNo";
                    int colVoucherDetailId = COL;
                    worksheet[ROW, COL].ColumnWidth = 12;
                    worksheet[ROW, COL].CellStyle.Font.Bold = true;
                    worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    COL++;


                    worksheet[ROW, COL].Text = "Entity";
                    int colEntity = COL;
                    worksheet[ROW, COL].ColumnWidth = 30;
                    worksheet[ROW, COL].CellStyle.Font.Bold = true;
                    worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    COL++;
                    worksheet[ROW, COL].Text = "Checked By Name";
                    int colCheckedByName = COL;
                    worksheet[ROW, COL].ColumnWidth = 20;
                    worksheet[ROW, COL].CellStyle.Font.Bold = true;
                    worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    COL++;

                    worksheet[ROW, COL].Text = "Approved By Name";
                    int colApprovedByName = COL;
                    worksheet[ROW, COL].ColumnWidth = 20;
                    worksheet[ROW, COL].CellStyle.Font.Bold = true;
                    worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    COL++;


                    worksheet[ROW, COL].Text = "Is Posted";
                    int colPosted = COL;
                    worksheet[ROW, COL].ColumnWidth = 12;
                    worksheet[ROW, COL].CellStyle.Font.Bold = true;
                    worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    COL++;

                    worksheet[ROW, COL].Text = "Note For Accounts";
                    int colNoteForAccounts = COL;
                    worksheet[ROW, COL].ColumnWidth = 30;
                    worksheet[ROW, COL].CellStyle.Font.Bold = true;
                    worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    COL++;

                    worksheet[ROW, COL].Text = "Contract";
                    int colContract = COL;
                    worksheet[ROW, COL].ColumnWidth = 30;
                    worksheet[ROW, COL].CellStyle.Font.Bold = true;
                    worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    COL++;
                    worksheet[ROW, COL].Text = "MastrerLC Ref No";
                    int colMastrerLCRefNo = COL;
                    worksheet[ROW, COL].ColumnWidth = 30;
                    worksheet[ROW, COL].CellStyle.Font.Bold = true;
                    worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    COL++;
                    worksheet[ROW, COL].Text = "Commercial Invoice No";
                    int colComercialInvoiceNo = COL;
                    worksheet[ROW, COL].ColumnWidth = 30;
                    worksheet[ROW, COL].CellStyle.Font.Bold = true;
                    worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    COL++;
                    worksheet[ROW, COL].Text = "Expiry Date";
                    int colExpiryDatet = COL;
                    worksheet[ROW, COL].ColumnWidth = 30;
                    worksheet[ROW, COL].CellStyle.Font.Bold = true;
                    worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    COL++;
                    worksheet[ROW, COL].Text = "BL/AWB No.";
                    int colBLAWBNo = COL;
                    worksheet[ROW, COL].ColumnWidth = 30;
                    worksheet[ROW, COL].CellStyle.Font.Bold = true;
                    worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    COL++;
                    worksheet[ROW, COL].Text = "BL/AWB Date";
                    int colBLAWBDate = COL;
                    worksheet[ROW, COL].ColumnWidth = 30;
                    worksheet[ROW, COL].CellStyle.Font.Bold = true;
                    worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    COL++;
                    worksheet[ROW, COL].Text = "Payment Term";
                    int colPaymentTerm = COL;
                    worksheet[ROW, COL].ColumnWidth = 30;
                    worksheet[ROW, COL].CellStyle.Font.Bold = true;
                    worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    COL++;
                    worksheet[ROW, COL].Text = "Base on Due Date";
                    int colBaseOnDueDate = COL;
                    worksheet[ROW, COL].ColumnWidth = 30;
                    worksheet[ROW, COL].CellStyle.Font.Bold = true;
                    worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    COL++;
                    worksheet[ROW, COL].Text = "No Of Days";
                    int colNoOfDays = COL;
                    worksheet[ROW, COL].ColumnWidth = 30;
                    worksheet[ROW, COL].CellStyle.Font.Bold = true;
                    worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    COL++;
                    worksheet[ROW, COL].Text = "Mature Date";
                    int colMatureDate = COL;
                    worksheet[ROW, COL].ColumnWidth = 30;
                    worksheet[ROW, COL].CellStyle.Font.Bold = true;
                    worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    COL++;
                    worksheet[ROW, COL].Text = "LC Amount";
                    int colLCAmount = COL;
                    worksheet[ROW, COL].ColumnWidth = 30;
                    worksheet[ROW, COL].CellStyle.Font.Bold = true;
                    worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    COL++;
                    worksheet[ROW, COL].Text = "ExFactory Date";
                    int colExFactoryDate = COL;
                    worksheet[ROW, COL].ColumnWidth = 30;
                    worksheet[ROW, COL].CellStyle.Font.Bold = true;
                    worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    COL++;
                    worksheet[ROW, COL].Text = "Transport Agent";
                    int colTransportAgent = COL;
                    worksheet[ROW, COL].ColumnWidth = 30;
                    worksheet[ROW, COL].CellStyle.Font.Bold = true;
                    worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    COL++;
                    worksheet[ROW, COL].Text = "Transport Doc Date";
                    int colTransportDocDate = COL;
                    worksheet[ROW, COL].ColumnWidth = 30;
                    worksheet[ROW, COL].CellStyle.Font.Bold = true;
                    worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    COL++;
                    worksheet[ROW, COL].Text = "CNF Agent";
                    int colCNFAgent = COL;
                    worksheet[ROW, COL].ColumnWidth = 30;
                    worksheet[ROW, COL].CellStyle.Font.Bold = true;
                    worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    COL++;
                    worksheet[ROW, COL].Text = "Container No.";
                    int colContainerNo = COL;
                    worksheet[ROW, COL].ColumnWidth = 30;
                    worksheet[ROW, COL].CellStyle.Font.Bold = true;
                    worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    COL++;
                    worksheet[ROW, COL].Text = "Vessel Tracking No.";
                    int colVesselTrackingNo = COL;
                    worksheet[ROW, COL].ColumnWidth = 30;
                    worksheet[ROW, COL].CellStyle.Font.Bold = true;
                    worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    COL++;

                    worksheet[ROW, COL].Text = "Own Order Ref.";
                    int colOwnOrderRef = COL;
                    worksheet[ROW, COL].ColumnWidth = 30;
                    worksheet[ROW, COL].CellStyle.Font.Bold = true;
                    worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    COL++;

                    worksheet[ROW, COL].Text = "Realize date";
                    int colRealizeDate = COL;
                    worksheet[ROW, COL].ColumnWidth = 30;
                    worksheet[ROW, COL].CellStyle.Font.Bold = true;
                    worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    COL++;

                    worksheet[ROW, COL].Text = "Realize amount";
                    int colRealizeAmount = COL;
                    worksheet[ROW, COL].ColumnWidth = 30;
                    worksheet[ROW, COL].CellStyle.Font.Bold = true;
                    worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    COL++;

                    worksheet[ROW, COL].Text = "Balance";
                    int colBalance = COL;
                    worksheet[ROW, COL].ColumnWidth = 30;
                    worksheet[ROW, COL].CellStyle.Font.Bold = true;
                    worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    //COL++;

                    int endCol = COL;
                    worksheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 10f;
                    worksheet.Range[ROW, 1, ROW, endCol].RowHeight = 22;
                    worksheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                    worksheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                    worksheet.Range[ROW, startCol, ROW, COL].CellStyle.ColorIndex = ExcelKnownColors.Grey_40_percent;
                    ROW++;
                    try
                    {
                        if (Summary == "Summary")
                        {
                            for (int i = 0; i < dtInventorySalesReportList.Rows.Count; i++)
                            {

                                // int i = 0; i < dtMasterOrderItem.Rows.Count; i++
                                worksheet[ROW, colSL].Text = dtInventorySalesReportList.Rows[i]["S.N"].ToString();
                                worksheet[ROW, colId].Text = dtInventorySalesReportList.Rows[i]["SalesId"].ToString();
                                worksheet[ROW, colSourceType].Text = dtInventorySalesReportList.Rows[i]["SourceType"].ToString();

                                worksheet[ROW, colSalesDate].Text = dtInventorySalesReportList.Rows[i]["SalesDate"].ToString();
                                worksheet[ROW, colInvoiceDate].Text = dtInventorySalesReportList.Rows[i]["InvoiceDate"].ToString();
                                worksheet[ROW, colBillTo].Text = dtInventorySalesReportList.Rows[i]["BillTo"].ToString();
                                worksheet[ROW, colShipTo].Text = dtInventorySalesReportList.Rows[i]["ShipTo"].ToString();
                                worksheet[ROW, colToCurrencyRate].Number = clsStaticInfo.dbl(dtInventorySalesReportList.Rows[i]["ToCurrencyRate"].ToString());
                                worksheet[ROW, colToCurrencyRate].NumberFormat = NumberFormatFourDecimal;

                                worksheet[ROW, colDocRefNo].Text = dtInventorySalesReportList.Rows[i]["DocRefNo"].ToString();
                                worksheet[ROW, colDocDate].Text = dtInventorySalesReportList.Rows[i]["DocDate"].ToString();
                                worksheet[ROW, colCustomerPONumber].Text = dtInventorySalesReportList.Rows[i]["PONumber"].ToString();
                                worksheet[ROW, colMasterOrderNumber].Text = dtInventorySalesReportList.Rows[i]["MasterOrder"].ToString();
                                worksheet[ROW, colSalesOrderNumber].Text = dtInventorySalesReportList.Rows[i]["SONumber"].ToString();
                                worksheet[ROW, colPartyName].Text = dtInventorySalesReportList.Rows[i]["PartyName"].ToString();

                                worksheet[ROW, colPartyCode].Text = dtInventorySalesReportList.Rows[i]["Code"].ToString();

                                worksheet[ROW, colCurrency].Text = dtInventorySalesReportList.Rows[i]["Currency"].ToString();


                               
                               
                                worksheet[ROW, colMatAmt].Number = clsStaticInfo.dbl(dtInventorySalesReportList.Rows[i]["TransactionAmount"].ToString());
                                worksheet.Range[ROW, colMatAmt].NumberFormat = NumberFormatTwoDecimal;
                               

                                worksheet[ROW, colServAmt].Number = clsStaticInfo.dbl(dtInventorySalesReportList.Rows[i]["ServiceCharge"].ToString());
                                worksheet.Range[ROW, colServAmt].NumberFormat = NumberFormatTwoDecimal;

                                worksheet[ROW, colTransactionAmount].Number = clsStaticInfo.dbl(dtInventorySalesReportList.Rows[i]["TotalTaxableAmt"].ToString());
                                worksheet.Range[ROW, colTransactionAmount].NumberFormat = NumberFormatTwoDecimal;
                                if (WithTax == true)
                                {
                                    worksheet[ROW, colCGST].Number = clsStaticInfo.dbl(dtInventorySalesReportList.Rows[i]["CGST"].ToString());
                                    worksheet.Range[ROW, colCGST].NumberFormat = NumberFormatTwoDecimal;
                                    worksheet[ROW, colSGST].Number = clsStaticInfo.dbl(dtInventorySalesReportList.Rows[i]["SGST"].ToString());
                                    worksheet.Range[ROW, colSGST].NumberFormat = NumberFormatTwoDecimal;
                                    worksheet[ROW, colIGST].Number = clsStaticInfo.dbl(dtInventorySalesReportList.Rows[i]["IGST"].ToString());
                                    worksheet.Range[ROW, colIGST].NumberFormat = NumberFormatTwoDecimal;
                                    worksheet[ROW, colTCS].Number = clsStaticInfo.dbl(dtInventorySalesReportList.Rows[i]["TCS"].ToString());
                                    worksheet.Range[ROW, colTCS].NumberFormat = NumberFormatTwoDecimal;

                                    worksheet[ROW, colBooksCGST].Number = clsStaticInfo.dbl(dtInventorySalesReportList.Rows[i]["BooksCGST"].ToString());
                                    worksheet.Range[ROW, colBooksCGST].NumberFormat = NumberFormatTwoDecimal;
                                    worksheet[ROW, colBooksSGST].Number = clsStaticInfo.dbl(dtInventorySalesReportList.Rows[i]["BooksSGST"].ToString());
                                    worksheet.Range[ROW, colBooksSGST].NumberFormat = NumberFormatTwoDecimal;
                                    worksheet[ROW, colBooksIGST].Number = clsStaticInfo.dbl(dtInventorySalesReportList.Rows[i]["BooksIGST"].ToString());
                                    worksheet.Range[ROW, colBooksIGST].NumberFormat = NumberFormatTwoDecimal;
                                    worksheet[ROW, colBooksTCS].Number = clsStaticInfo.dbl(dtInventorySalesReportList.Rows[i]["BooksTCS"].ToString());
                                    worksheet.Range[ROW, colBooksTCS].NumberFormat = NumberFormatTwoDecimal;
                                }
                                worksheet[ROW, colBooksMatAmt].Number = clsStaticInfo.dbl(dtInventorySalesReportList.Rows[i]["BooksCurrencyTransactionAmount"].ToString());
                                worksheet.Range[ROW, colBooksMatAmt].NumberFormat = NumberFormatTwoDecimal;
                                worksheet[ROW, colBooksServAmt].Number = clsStaticInfo.dbl(dtInventorySalesReportList.Rows[i]["BooksServiceCharge"].ToString());
                                worksheet.Range[ROW, colBooksServAmt].NumberFormat = NumberFormatTwoDecimal;
                                worksheet[ROW, colBooksTtlTaxableAmt].Number = clsStaticInfo.dbl(dtInventorySalesReportList.Rows[i]["BooksTotalTaxableAmt"].ToString());
                                worksheet.Range[ROW, colBooksTtlTaxableAmt].NumberFormat = NumberFormatTwoDecimal;

                               
                                worksheet[ROW, colVoucherDetailId].Text = dtInventorySalesReportList.Rows[i]["VoucherId"].ToString();



                                worksheet[ROW, colEntity].Text = dtInventorySalesReportList.Rows[i]["Entity"].ToString();
                                worksheet[ROW, colCheckedByName].Text = dtInventorySalesReportList.Rows[i]["CheckedByName"].ToString();
                                worksheet[ROW, colApprovedByName].Text = dtInventorySalesReportList.Rows[i]["ApprovedByName"].ToString();
                                worksheet[ROW, colPosted].Text = dtInventorySalesReportList.Rows[i]["Posted"].ToString();
                                worksheet[ROW, colNoteForAccounts].Text = dtInventorySalesReportList.Rows[i]["NoteForAccounts"].ToString();
                               
                                worksheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                                worksheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                                worksheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                                ROW++;
                            }

                            worksheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                            //worksheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                            worksheet["A" + 7].FreezePanes();
                            ReportUtility reportUtility = new ReportUtility();
                            reportUtility.PlantHeader(ref worksheet, endCol, " Sales Register", identity.PlantId);
                            reportUtility.PageSetup(ref worksheet, 5, ExcelPageOrientation.Landscape);
                            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                            // worksheet.Range[1, 1, 4, endCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            worksheet.Range[1, 1, 3, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                        }
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }

                //}

                worksheet.UsedRange.CellStyle.Font.FontName = "Tahoma";
                //worksheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                worksheet.IsGridLinesVisible = false;
                //worksheet.UsedRange.CellStyle.Font.Size = 8;
                #region Freeze Panes

                worksheet.IsDisplayZeros = false;
                worksheet.UsedRange["A8"].FreezePanes();
                worksheet.FirstVisibleColumn = 1;
                //worksheet.FirstVisibleRow = 8;

                #endregion Freeze Panes


                return workbook;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
       
    }
}