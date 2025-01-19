using Aplos.Controllers;
using Aplos.Properties;
using Library.Accounting.Accounts;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Model.Banks;
using Library.Model.Enums;
using Library.Security.Core;
using Library.Service.Banks;
using Library.Service.Helpers;
using Library.ViewModel.Banks;
using Library.ViewModel.Vouchers;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.HumanResource.Controllers
{
    public class EmployeeSkillMatrixController : BaseController
    {
        private readonly IBankJournalService _bankJournalService;
        private readonly IBankReportService _bankReportService;
        private readonly ISqlRepository _sqlRepository;
        private readonly AccountsBankService _accountsBankService;

        public EmployeeSkillMatrixController(
            IBankJournalService bankJournalService
            , IBankReportService bankReportService
            , ISqlRepository sqlRepository
            , AccountsBankService accountsBankService
            )
        {
            _bankJournalService = bankJournalService;
            _bankReportService = bankReportService;
            _sqlRepository = sqlRepository;
            _accountsBankService = accountsBankService;
        }

        //[HttpGet]
        //public ActionResult CurrentFundPosition()
        //{
        //    return View("~/Areas/Banks/Views/CurrentFundPosition.cshtml");
        //}

        public ActionResult Aplos()
        {
            return View();
        }

        [HttpGet, Authorize]
        public ActionResult BankJournal()
        {
            return View("~/Areas/Banks/Views/BankJournal.cshtml");
        }

        [HttpGet]
        public JsonResult GetBankJournalList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_bankJournalService.GetBankJournalList(parameters, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, SourceType.BankJournal), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult InsertBankJournal(VoucherViewModel voucherVM, IEnumerable<BankChargeViewModel> bankChargeDetailVMList, IEnumerable<VoucherDetailViewModel> voucherDetailVMList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            voucherVM.CompanyGroupId = identity.CompanyGroupId;
            voucherVM.CompanyId = identity.CompanyId;
            voucherVM.PlantId = identity.PlantId;
            voucherVM.SourceType = SourceType.BankJournal.ToString();
            voucherVM.IsPark = true;
            if (voucherVM.BankJournalType == BankJournalType.CashExpense.ToString() && voucherDetailVMList == null)
                throw new CustomException("Please select GL!");
            if (voucherVM.BankJournalType == BankJournalType.BankCharge.ToString() && bankChargeDetailVMList == null)
                throw new CustomException("Please select GL!");
            if (voucherVM.BankJournalType == BankJournalType.ProfitEarn.ToString() && voucherVM.FinancingTypeId == null)
                throw new CustomException("Please select Investment Type!");

            return Json(new { Message = string.Format(AplosMessage.VoucherSave, _bankJournalService.InsertBankJournal(voucherVM, voucherDetailVMList, bankChargeDetailVMList)) });
        }

        [HttpPost]
        public JsonResult UpdateBankJournal(VoucherViewModel voucherVM, IEnumerable<BankChargeViewModel> bankChargeDetailVMList, IEnumerable<VoucherDetailViewModel> voucherDetailVMList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            voucherVM.CompanyGroupId = identity.CompanyGroupId;
            voucherVM.CompanyId = identity.CompanyId;
            voucherVM.PlantId = identity.PlantId;
            voucherVM.SourceType = SourceType.BankJournal.ToString();
            voucherVM.IsPark = true;
            return Json(new { Message = string.Format(AplosMessage.VoucherUpdate, _bankJournalService.UpdateBankJournal(voucherVM, voucherDetailVMList, bankChargeDetailVMList)) });
        }

        [HttpPost]
        public JsonResult PostBankJournal(string id)
        {
            _bankJournalService.PostBankJournal(id);
            return Json(new { Message = AplosMessage.Posted });
        }

        [HttpPost]
        public JsonResult DeleteBankJournal(string bankJournalId, string voucherId)
        {
            _bankJournalService.DeleteBankJournal(bankJournalId, voucherId);
            return Json(new { Message = AplosMessage.Posted });
        }

        [HttpGet, Authorize]
        public JsonResult GetBankJournal(string id)
        {
            return Json(_bankJournalService.GetBankJournal(id), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetBankJournalDetailList(string id)
        {
            return Json(_bankJournalService.GetBankChargeList(id), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAdvanceBankChargeList(string bankChargeId)
        {
            return Json(_bankJournalService.GetAdvanceBankChargeList(bankChargeId), JsonRequestBehavior.AllowGet);
        }

        #region Telly


        public ActionResult PaymentByBank()
        {
            return View("~/Areas/Banks/Views/PaymentByBank.cshtml");
        }

        [HttpGet, Authorize]
        public JsonResult GetPaymentByBankList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_bankJournalService.GetBankCashPaymentList(parameters, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, SourceType.PaymentByBank), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetBankCashPaymentDetailList(GridParameter parameters, string bankJournalId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_bankJournalService.GetBankCashPaymentDetailList(parameters, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, SourceType.PaymentByBank, bankJournalId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult InsertPaymentByBank(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            voucherVM.CompanyGroupId = identity.CompanyGroupId;
            voucherVM.CompanyId = identity.CompanyId;
            voucherVM.PlantId = identity.PlantId;
            voucherVM.SourceType = SourceType.PaymentByBank.ToString();
            if (voucherVM.BankJournalType == BankJournalType.BankToBank.ToString() && voucherVM.OtherBankMasterId == null)
                throw new CustomException("Please Select To  Bank!");
            else if (voucherVM.BankJournalType == BankJournalType.BankToCash.ToString() && voucherVM.OtherCashMasterId == null)
                throw new CustomException("Please Select To  Cash!");
            else if (voucherVM.BankJournalType == BankJournalType.BankToVendor.ToString() && voucherVM.PartyId == null)
                throw new CustomException("Please Select Vendor!");
            else if (voucherVM.BankJournalType == BankJournalType.BankToVendor.ToString() && voucherVM.PartyPlantId == null)
                throw new CustomException("Please Select Invoicing Vendor!");
            else if (voucherVM.BankJournalType == BankJournalType.BankToEmployee.ToString() && voucherVM.EmployeeId == null)
                throw new CustomException("Please Select Employee!");
            else if (voucherVM.BankJournalType == BankJournalType.BankToEmployee.ToString() && voucherVM.EmployeeTransactionTypeId == null)
                throw new CustomException("Please Select Transaction Type!");
            else if (voucherVM.Amount == 0 || voucherVM.Amount < 0)
                throw new CustomException("Please Input Amount!");
            else if (voucherVM.BankJournalType == BankJournalType.BankToGL.ToString() && voucherDetailVMList == null)
                throw new CustomException("Please select GL!");
            else if (voucherVM.BankJournalType == BankJournalType.BankToGL.ToString() && voucherVM.Amount != voucherDetailVMList.Sum(r => r.Amount))
                throw new CustomException("Dr Cr Amount not match!");
            voucherVM.IsPark = true;
            var no = _bankJournalService.InsertBankPayment(voucherVM, voucherDetailVMList);
            return Json(new { Message = string.Format(AplosMessage.VoucherSave, no) });
        }

        [HttpPost]
        public JsonResult UpdatePaymentByBank(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            voucherVM.CompanyGroupId = identity.CompanyGroupId;
            voucherVM.CompanyId = identity.CompanyId;
            voucherVM.PlantId = identity.PlantId;
            voucherVM.SourceType = SourceType.PaymentByBank.ToString();
            if (voucherVM.BankJournalType == BankJournalType.BankToBank.ToString() && voucherVM.OtherBankMasterId == null)
                throw new CustomException("Please Select To  Bank!");
            else if (voucherVM.BankJournalType == BankJournalType.BankToCash.ToString() && voucherVM.OtherCashMasterId == null)
                throw new CustomException("Please Select To  Cash!");
            else if (voucherVM.BankJournalType == BankJournalType.BankToVendor.ToString() && voucherVM.PartyId == null)
                throw new CustomException("Please Select Vendor!");
            else if (voucherVM.BankJournalType == BankJournalType.BankToVendor.ToString() && voucherVM.PartyPlantId == null)
                throw new CustomException("Please Select Invoicing Vendor!");
            else if (voucherVM.BankJournalType == BankJournalType.BankToEmployee.ToString() && voucherVM.EmployeeId == null)
                throw new CustomException("Please Select Employee!");
            else if (voucherVM.BankJournalType == BankJournalType.BankToEmployee.ToString() && voucherVM.EmployeeTransactionTypeId == null)
                throw new CustomException("Please Select Transaction Type!");
            else if (voucherVM.Amount == 0 || voucherVM.Amount < 0)
                throw new CustomException("Please Input Amount!");
            else if (voucherVM.BankJournalType == BankJournalType.BankToGL.ToString() && voucherDetailVMList == null)
                throw new CustomException("Please select GL!");
            else if (voucherVM.BankJournalType == BankJournalType.BankToGL.ToString() && voucherVM.Amount != voucherDetailVMList.Sum(r => r.Amount))
                throw new CustomException("Dr Cr Amount not match!");
            voucherVM.IsPark = true;
            return Json(new { Message = string.Format(AplosMessage.VoucherUpdate, _bankJournalService.UpdateBankPayment(voucherVM, voucherDetailVMList)) });
        }

        [HttpPost]
        public JsonResult PostPaymentByBank(string id)
        {
            _bankJournalService.PostBankJournal(id);
            return Json(new { Message = AplosMessage.Posted });
        }

        [HttpGet]
        public ActionResult ReceiptByBank()
        {
            return View("~/Areas/Banks/Views/ReceiptByBank.cshtml");
        }


        [HttpGet, Authorize]
        public JsonResult GetReceiptByBankList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_bankJournalService.GetBankCashPaymentList(parameters, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, SourceType.ReceiptByBank), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetBankCashReceiptDetailList(GridParameter parameters, string bankJournalId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_bankJournalService.GetBankCashPaymentDetailList(parameters, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, SourceType.ReceiptByBank, bankJournalId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult InsertReceiptByBank(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            voucherVM.CompanyGroupId = identity.CompanyGroupId;
            voucherVM.CompanyId = identity.CompanyId;
            voucherVM.PlantId = identity.PlantId;
            voucherVM.SourceType = SourceType.ReceiptByBank.ToString();
            if (voucherVM.BankJournalType == BankJournalType.BankToBank.ToString() && voucherVM.OtherBankMasterId == null)
                throw new CustomException("Please Select To Bank!");
            else if (voucherVM.BankJournalType == BankJournalType.BankToCash.ToString() && voucherVM.OtherCashMasterId == null)
                throw new CustomException("Please Select To Cash!");
            else if (voucherVM.BankJournalType == BankJournalType.BankToCustomer.ToString() && voucherVM.PartyId == null)
                throw new CustomException("Please Select Customer!");
            else if (voucherVM.BankJournalType == BankJournalType.BankToCustomer.ToString() && voucherVM.PartyPlantId == null)
                throw new CustomException("Please Select Invoicing Customer!");
            else if (voucherVM.BankJournalType == BankJournalType.BankToEmployee.ToString() && voucherVM.EmployeeId == null)
                throw new CustomException("Please Select Employee!");
            else if (voucherVM.BankJournalType == BankJournalType.BankToEmployee.ToString() && voucherVM.EmployeeTransactionTypeId == null)
                throw new CustomException("Please Select Transaction Type!");
            else if (voucherVM.Amount == 0 || voucherVM.Amount < 0)
                throw new CustomException("Please Input Amount!");
            else if (voucherVM.BankJournalType == BankJournalType.BankToGL.ToString() && voucherDetailVMList == null)
                throw new CustomException("Please select GL!");
            else if (voucherVM.BankJournalType == BankJournalType.BankToGL.ToString() && voucherVM.Amount != voucherDetailVMList.Sum(r => r.Amount))
                throw new CustomException("Dr Cr Amount not match!");
            voucherVM.IsPark = true;
            var no = _bankJournalService.InsertBankReceipt(voucherVM, voucherDetailVMList);
            return Json(new { Message = string.Format(AplosMessage.VoucherSave, no) });

        }

        [HttpPost]
        public JsonResult UpdateReceiptByBank(VoucherViewModel voucherVM, IEnumerable<BankChargeViewModel> bankChargeDetailVMList, IEnumerable<VoucherDetailViewModel> voucherDetailVMList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            voucherVM.CompanyGroupId = identity.CompanyGroupId;
            voucherVM.CompanyId = identity.CompanyId;
            voucherVM.PlantId = identity.PlantId;
            voucherVM.SourceType = SourceType.ReceiptByBank.ToString();
            if (voucherVM.BankJournalType == BankJournalType.BankToBank.ToString() && voucherVM.OtherBankMasterId == null)
                throw new CustomException("Please Select To Bank!");
            else if (voucherVM.BankJournalType == BankJournalType.BankToCash.ToString() && voucherVM.OtherCashMasterId == null)
                throw new CustomException("Please Select To Cash!");
            else if (voucherVM.BankJournalType == BankJournalType.BankToCustomer.ToString() && voucherVM.PartyId == null)
                throw new CustomException("Please Select Customer!");
            else if (voucherVM.BankJournalType == BankJournalType.BankToCustomer.ToString() && voucherVM.PartyPlantId == null)
                throw new CustomException("Please Select Invoicing Customer!");
            else if (voucherVM.BankJournalType == BankJournalType.BankToEmployee.ToString() && voucherVM.EmployeeId == null)
                throw new CustomException("Please Select Employee!");
            else if (voucherVM.BankJournalType == BankJournalType.BankToEmployee.ToString() && voucherVM.EmployeeTransactionTypeId == null)
                throw new CustomException("Please Select Transaction Type!");
            else if (voucherVM.Amount == 0 || voucherVM.Amount < 0)
                throw new CustomException("Please Input Amount!");
            else if (voucherVM.BankJournalType == BankJournalType.BankToGL.ToString() && voucherDetailVMList == null)
                throw new CustomException("Please select GL!");
            else if (voucherVM.BankJournalType == BankJournalType.BankToGL.ToString() && voucherVM.Amount != voucherDetailVMList.Sum(r => r.Amount))
                throw new CustomException("Dr Cr Amount not match!");
            voucherVM.IsPark = true;
            return Json(new { Message = string.Format(AplosMessage.VoucherUpdate, _bankJournalService.UpdateBankReceipt(voucherVM, voucherDetailVMList)) });
        }

        [HttpPost]
        public JsonResult PostReceiptByBank(string id)
        {
            _bankJournalService.PostBankJournal(id);
            return Json(new { Message = AplosMessage.Posted });
        }

        #endregion Telly

        [Authorize, HttpGet]
        public JsonResult GetAvilabeCustomerPaymentList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_bankJournalService.GetAvilabeCustomerPaymentList(parameters, identity.CompanyGroupId, identity.CompanyId, identity.PlantId), JsonRequestBehavior.AllowGet);
        }


        //Current Fund Position start//

        [HttpGet]
        public ActionResult getEmployeeSkillMatrixList()
        {
            string sql = @"SELECT ISNULL(EI.EmployeeCode,0) EmployeeCode
	                                    ,EI.EmployeeName
	                                    ,EI.EmployeeStatus
	                                    ,D.UserName Department
	                                    ,S.UserName Section
	                                    ,ISNULL(E.UserName,'') Entity
	                                    ,ISNULL(L.UserName,0) BudgetedLine
	                                    ,SC.UserName SkillCategory
	                                    ,ISNULL(MMA.StandardName,'') Machine
	                                    ,ECA.UserName EmployeeCategory
	                                    ,ISNULL(O.Code,0) OperationCode
	                                    ,Process=ISNULL(STUFF((SELECT DISTINCT ',' + P.UserName FROM [MST].[OperationProcess] AS OPMT
					                                                        LEFT JOIN HKP.[Process] AS P ON OPMT.ProcessId=P.Id
					                                                        WHERE OPMT.OperationId=O.Id
					                                                        GROUP BY P.UserName
					                                                        FOR XML PATH ('')
					                                                        ),1,1,''),'')
                                    FROM EmployeeInformation EI
                                    LEFT JOIN MST.ManpowerBudget MB ON MB.Id = EI.BudgetCode
                                    LEFT JOIN ORG.Position PR ON MB.PositionId=PR.Id
                                    LEFT JOIN ORG.Department D ON D.Id = PR.DepartmentId
                                    LEFT JOIN ORG.Section S ON S.Id = PR.SectionId
                                    LEFT JOIN ORG.Line L ON L.Id = MB.LineId
                                    LEFT JOIN ORG.Entity E ON E.Id = MB.EntityId
                                    LEFT JOIN MST.OperationVariation OV ON OV.Id = EI.OperationVariationId
                                    LEFT JOIN MST.Operation O ON O.Id = OV.OperationId
                                    LEFT JOIN HKP.Skill Sk ON Sk.Id = O.SkillId
                                    LEFT JOIN HKP.SkillCategory SC ON SC.Id = Sk.SkillCategoryId
                                    left join MST.MaterialMasterArticle MMA on MMA.Id=OV.ArticleId
                                    LEFT JOIN (select EC.UserName,DM.DesignationId from MST.DesignationMaster DM 
                                    left join HKP.EmployeeCategory EC ON EC.Id = DM.EmployeeCategoryId
                                    ) ECA  on ECA.DesignationId=EI.GivenDesignationId

                                    WHERE EI.EmployeeStatus = 'Active'";

            var data = _sqlRepository.GetDataCollection(sql);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult GetEmployeeSkillMatrixEmployeeWise()
        {
            try
            {
                string fileName = "";
                fileName = EmployeeSkillMatrixEmployeeWiseReport("Employee Skill Matrix Line Wise");
                return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public string EmployeeSkillMatrixEmployeeWiseReport(string SheetName)
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
                workbook.Worksheets[0].Name = "CurrentFundPositionReport";
                sheet = workbook.Worksheets[0];
                DataTable data;
                EmployeeSkillMatrixEmployeeWiseSQL(out data);

                int ROW = 6; int COL = 1;

                #region columns
                sheet[ROW, COL].Text = "Employee Code";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColEmployeeCode = COL;
                COL++;

                sheet[ROW, COL].Text = "Employee Name";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColEmployeeName = COL;
                COL++;

                sheet[ROW, COL].Text = "Employee Status";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColEmployeeStatus = COL;
                COL++;

                sheet[ROW, COL].Text = "Department";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColDepartment = COL;
                COL++;

                sheet[ROW, COL].Text = "Section";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColSection = COL;
                COL++;

                sheet[ROW, COL].Text = "Entity";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColEntity = COL;
                COL++;

                sheet[ROW, COL].Text = "Budgeted Line";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColBudgetedLine = COL;
                COL++;

                sheet[ROW, COL].Text = "Skill Category";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColSkillCategory = COL;
                COL++;

                sheet[ROW, COL].Text = "Machine";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColMachine = COL;
                COL++;

                sheet[ROW, COL].Text = "Employee Category";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColEmployeeCategory = COL;
                COL++;

                sheet[ROW, COL].Text = "Operation Code";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColOperationCode = COL;
                COL++;

                sheet[ROW, COL].Text = "Process";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColProcess = COL;


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

                for (int i = 0; i < data.Rows.Count; i++)
                {
                    sheet[ROW, ColEmployeeCode].Text = data.Rows[i]["EmployeeCode"].ToString();
                    sheet[ROW, ColEmployeeName].Text = data.Rows[i]["EmployeeName"].ToString();
                    sheet[ROW, ColEmployeeStatus].Text = data.Rows[i]["EmployeeStatus"].ToString();
                    sheet[ROW, ColDepartment].Text = data.Rows[i]["Department"].ToString();
                    sheet[ROW, ColSection].Text = data.Rows[i]["Section"].ToString();
                    sheet[ROW, ColEntity].Text = data.Rows[i]["Entity"].ToString();
                    sheet[ROW, ColBudgetedLine].Text = data.Rows[i]["BudgetedLine"].ToString();
                    sheet[ROW, ColSkillCategory].Text = data.Rows[i]["SkillCategory"].ToString();
                    sheet[ROW, ColMachine].Text = data.Rows[i]["Machine"].ToString();
                    sheet[ROW, ColEmployeeCategory].Text = data.Rows[i]["EmployeeCategory"].ToString();
                    sheet[ROW, ColOperationCode].Text = data.Rows[i]["OperationCode"].ToString();
                    sheet[ROW, ColProcess].Number = clsStaticInfo.dbl(data.Rows[i]["Process"].ToString());
                    
                    sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                    ROW++;

                }
                //IListObject table = sheet.ListObjects.Create("Table1", sheet.Range[6, 1, ROW, endCol]);
                //table.BuiltInTableStyle = TableBuiltInStyles.TableStyleMedium7;
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[startRow, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                sheet["A" + startRow.ToString()].FreezePanes();

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ReportUtility reportUtility = new ReportUtility();
                reportUtility.PlantHeader(ref sheet, endCol, "Employee Skill Matrix Report", identity.PlantId);
                reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.IsGridLinesVisible = false;

                //sheet.Range[startRow, 1, ROW, endCol].NumberFormat = Library.Service.Extension.clsStaticInfo.NumberFormat(2);


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

        public void EmployeeSkillMatrixEmployeeWiseSQL(out DataTable data)
        {
            try
            {
                string strSQL = @"SELECT ISNULL(EI.EmployeeCode,'') EmployeeCode
	                                ,EI.EmployeeName
	                                ,EI.EmployeeStatus
	                                ,D.UserName Department
	                                ,S.UserName Section
	                                ,ISNULL(E.UserName,'') Entity
	                                ,ISNULL(L.UserName,'') BudgetedLine
	                                ,L.Id BudgetedLineId
	                                ,SC.UserName SkillCategory
	                                ,ISNULL(MMA.StandardName,'') Machine
	                                ,ECA.UserName EmployeeCategory
	                                ,ISNULL(O.Code,'') OperationCode
	                                ,Process=ISNULL(STUFF((SELECT DISTINCT ',' + P.UserName FROM [MST].[OperationProcess] AS OPMT
					                                                    LEFT JOIN HKP.[Process] AS P ON OPMT.ProcessId=P.Id
					                                                    WHERE OPMT.OperationId=O.Id
					                                                    GROUP BY P.UserName
					                                                    FOR XML PATH ('')
					                                                    ),1,1,''),'')
                                FROM EmployeeInformation EI
                                LEFT JOIN MST.ManpowerBudget MB ON MB.Id = EI.BudgetCode
                                LEFT JOIN ORG.Position PR ON MB.PositionId=PR.Id
                                LEFT JOIN ORG.Department D ON D.Id = PR.DepartmentId
                                LEFT JOIN ORG.Section S ON S.Id = PR.SectionId
                                LEFT JOIN ORG.Line L ON L.Id = MB.LineId
                                LEFT JOIN ORG.Entity E ON E.Id = MB.EntityId
                                LEFT JOIN MST.OperationVariation OV ON OV.Id = EI.OperationVariationId
                                LEFT JOIN MST.Operation O ON O.Id = OV.OperationId
                                LEFT JOIN HKP.Skill Sk ON Sk.Id = O.SkillId
                                LEFT JOIN HKP.SkillCategory SC ON SC.Id = Sk.SkillCategoryId
                                left join MST.MaterialMasterArticle MMA on MMA.Id=OV.ArticleId
                                LEFT JOIN (select EC.UserName,DM.DesignationId from MST.DesignationMaster DM 
                                left join HKP.EmployeeCategory EC ON EC.Id = DM.EmployeeCategoryId
                                ) ECA  on ECA.DesignationId=EI.GivenDesignationId

                                WHERE EI.EmployeeStatus = 'Active'";

                data = _sqlRepository.GetDataTable(strSQL);
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }

        [HttpPost, Authorize]
        public ActionResult GetEmployeeSkillMatrixLineWise()
        {
            try
            {
                string fileName = "";
                fileName = EmployeeSkillMatrixLineWiseReport("Employee Skill Matrix Line Wise");
                return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public string EmployeeSkillMatrixLineWiseReport(string SheetName)
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
                workbook.Worksheets[0].Name = "CurrentFundPositionReport";
                sheet = workbook.Worksheets[0];
                DataTable data;
                EmployeeSkillMatrixLineWiseSQL(out data);

                int ROW = 6; int COL = 1;

                #region columns
                sheet[ROW, COL].Text = "Employee Code";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColEmployeeCode = COL;
                COL++;

                sheet[ROW, COL].Text = "Employee Name";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColEmployeeName = COL;
                COL++;

                sheet[ROW, COL].Text = "Employee Status";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColEmployeeStatus = COL;
                COL++;

                sheet[ROW, COL].Text = "Department";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColDepartment = COL;
                COL++;

                sheet[ROW, COL].Text = "Section";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColSection = COL;
                COL++;

                sheet[ROW, COL].Text = "Entity";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColEntity = COL;
                COL++;

                sheet[ROW, COL].Text = "Budgeted Line";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColBudgetedLine = COL;
                COL++;

                sheet[ROW, COL].Text = "Skill Category";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColSkillCategory = COL;
                COL++;

                sheet[ROW, COL].Text = "Machine";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColMachine = COL;
                COL++;

                sheet[ROW, COL].Text = "Employee Category";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColEmployeeCategory = COL;
                COL++;

                sheet[ROW, COL].Text = "Operation Code";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColOperationCode = COL;
                COL++;

                sheet[ROW, COL].Text = "Process";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColProcess = COL;


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

                for (int i = 0; i < data.Rows.Count; i++)
                {
                    sheet[ROW, ColEmployeeCode].Text = data.Rows[i]["EmployeeCode"].ToString();
                    sheet[ROW, ColEmployeeName].Text = data.Rows[i]["EmployeeName"].ToString();
                    sheet[ROW, ColEmployeeStatus].Text = data.Rows[i]["EmployeeStatus"].ToString();
                    sheet[ROW, ColDepartment].Text = data.Rows[i]["Department"].ToString();
                    sheet[ROW, ColSection].Text = data.Rows[i]["Section"].ToString();
                    sheet[ROW, ColEntity].Text = data.Rows[i]["Entity"].ToString();
                    sheet[ROW, ColBudgetedLine].Text = data.Rows[i]["BudgetedLine"].ToString();
                    sheet[ROW, ColSkillCategory].Text = data.Rows[i]["SkillCategory"].ToString();
                    sheet[ROW, ColMachine].Text = data.Rows[i]["Machine"].ToString();
                    sheet[ROW, ColEmployeeCategory].Text = data.Rows[i]["EmployeeCategory"].ToString();
                    sheet[ROW, ColOperationCode].Text = data.Rows[i]["OperationCode"].ToString();
                    sheet[ROW, ColProcess].Number = clsStaticInfo.dbl(data.Rows[i]["Process"].ToString());

                    sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                    ROW++;

                }
                //IListObject table = sheet.ListObjects.Create("Table1", sheet.Range[6, 1, ROW, endCol]);
                //table.BuiltInTableStyle = TableBuiltInStyles.TableStyleMedium7;
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[startRow, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                sheet["A" + startRow.ToString()].FreezePanes();

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ReportUtility reportUtility = new ReportUtility();
                reportUtility.PlantHeader(ref sheet, endCol, "Employee Skill Matrix Report", identity.PlantId);
                reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.IsGridLinesVisible = false;

                //sheet.Range[startRow, 1, ROW, endCol].NumberFormat = Library.Service.Extension.clsStaticInfo.NumberFormat(2);


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

        public void EmployeeSkillMatrixLineWiseSQL(out DataTable data)
        {
            try
            {
                string strSQL = @"SELECT ISNULL(EI.EmployeeCode,0) EmployeeCode
	                                ,EI.EmployeeName
	                                ,EI.EmployeeStatus
	                                ,D.UserName Department
	                                ,S.UserName Section
	                                ,ISNULL(E.UserName,'') Entity
	                                ,ISNULL(L.UserName,'') BudgetedLine
	                                ,L.Id BudgetedLineId
	                                ,SC.UserName SkillCategory
	                                ,ISNULL(MMA.StandardName,'') Machine
	                                ,ECA.UserName EmployeeCategory
	                                ,ISNULL(O.Code,'') OperationCode
	                                ,Process=ISNULL(STUFF((SELECT DISTINCT ',' + P.UserName FROM [MST].[OperationProcess] AS OPMT
					                                                    LEFT JOIN HKP.[Process] AS P ON OPMT.ProcessId=P.Id
					                                                    WHERE OPMT.OperationId=O.Id
					                                                    GROUP BY P.UserName
					                                                    FOR XML PATH ('')
					                                                    ),1,1,''),'')
                                FROM EmployeeInformation EI
                                LEFT JOIN MST.ManpowerBudget MB ON MB.Id = EI.BudgetCode
LEFT JOIN ORG.Position PR ON MB.PositionId=PR.Id
                                LEFT JOIN ORG.Department D ON D.Id = PR.DepartmentId
                                LEFT JOIN ORG.Section S ON S.Id = PR.SectionId
                                LEFT JOIN ORG.Line L ON L.Id = MB.LineId
                                LEFT JOIN ORG.Entity E ON E.Id = MB.EntityId
                                LEFT JOIN MST.OperationVariation OV ON OV.Id = EI.OperationVariationId
                                LEFT JOIN MST.Operation O ON O.Id = OV.OperationId
                                LEFT JOIN HKP.Skill Sk ON Sk.Id = O.SkillId
                                LEFT JOIN HKP.SkillCategory SC ON SC.Id = Sk.SkillCategoryId
                                LEFT JOIN MST.MaterialMasterArticle MMA on MMA.Id=OV.ArticleId
                                LEFT JOIN (select EC.UserName,DM.DesignationId from MST.DesignationMaster DM 
                                LEFT JOIN HKP.EmployeeCategory EC ON EC.Id = DM.EmployeeCategoryId
                                ) ECA  on ECA.DesignationId=EI.GivenDesignationId

                                WHERE EI.EmployeeStatus = 'Active'
								ORDER BY L.UserName";

                data = _sqlRepository.GetDataTable(strSQL);
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }
    }
}