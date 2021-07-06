using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Model.Enums;
using Library.Model.Expenses;
using Library.Model.Parties;
using Library.Model.Payments;
using Library.Service.Expenses;
using Library.Service.Helpers;
using Library.ViewModel.Vouchers;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace Aplos.Areas.Accounts.Controllers
{
    public class ExpenseBookingController : BaseController
    {
        private readonly IExpenseBookingService _expenseBookingService;
        private readonly ISqlRepository _sqlRepository;
        public ExpenseBookingController(
            IExpenseBookingService expenseBookingService, ISqlRepository R)
        {
            _expenseBookingService = expenseBookingService;
            _sqlRepository = R;
        }

        
        public ActionResult Aplos()
        {
            return View();
        }

        [Authorize]
        public ActionResult ExpenseBookingPotal()
        {
            return View();
        }

        
        public ActionResult Approval()
        {
            return View();
        }

        [Authorize]
        public ActionResult ExpenseBookingApprovalPotal()
        {
            return View();
        }

        [Authorize]
        public ActionResult ExpenseBookingCheckedByPotal()
        {
            return View();
        }

        [Authorize]
        public ActionResult ExpenseBookingDepartmentApprovalPotal()
        {
            return View();
        }

        [Authorize]
        public ActionResult Approved()
        {
            return View();
        }

        
        public ActionResult ApprovedList()
        {
            return View();
        }

        [HttpGet, Authorize]
        public JsonResult GetPotalEmployeeTransactionNo()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_expenseBookingService.GetEmployeeTransactionNo(identity.EmployeeId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetEmployeeTransactionNo(string employeeId)
        {
            return Json(_expenseBookingService.GetEmployeeTransactionNo(employeeId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetExpenseBookingPendingList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_expenseBookingService.GetExpenseBookingPendingList(parameters, identity.CompanyGroupId, identity.CompanyId, identity.PlantId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetExpenseBookingApprovedList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_expenseBookingService.GetExpenseBookingApprovedList(parameters, identity.CompanyGroupId, identity.CompanyId, identity.PlantId), JsonRequestBehavior.AllowGet);
        }

        //[HttpGet, Authorize]
        //public JsonResult GetList(GridParameter parameters, string status)
        //{
        //    return Json(_expenseBookingService.QueryPoatal(parameters, status), JsonRequestBehavior.AllowGet);
        //}

        [HttpGet, Authorize]
        public JsonResult GetList(string status)
        {
            return Json(_expenseBookingService.QueryPoatal(status), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetListAdmin(GridParameter parameters, string status)
        {
            return Json(_expenseBookingService.QueryAdmin(parameters, status), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetListForApproval(GridParameter parameters)
        {
            return Json(_expenseBookingService.GetListForApproval(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetListForDepartmentApproval(string approvalStatus)
        {
            return Json(_expenseBookingService.GetListForDepartmentApproval(approvalStatus), JsonRequestBehavior.AllowGet);
        }

       

        [HttpGet, Authorize]
        public JsonResult GetListForDepartmentApprovedHoldReject()
        {
            return Json(_expenseBookingService.GetListForDepartmentApprovedHoldReject(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetExpensesBookingById(GridParameter parameters, string Id)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            return Json(_expenseBookingService.GetExpenseBookingApprovedData(parameters, identity.CompanyId, identity.PlantId, Id), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetExpensesBookingDetail(string expenseBookingId)
        {
            return Json(_expenseBookingService.GetExpenseBookingDetail(expenseBookingId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetBudgetTransactionDetail(GridParameter paremeters, string budgetTransactionMasterId)
        {
            return Json(_expenseBookingService.Query(paremeters, budgetTransactionMasterId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetBudgetTransactionMasterById(string id)
        {
            return Json(_expenseBookingService.Find(id), JsonRequestBehavior.AllowGet);
        }

        //public JsonResult CreateDocument(FormCollection form, HttpPostedFileBase[] file)
        //{
        //    var preRecruitmentDocument = new JavaScriptSerializer().Deserialize<PreRecruitmentDocument>(form["preRecruitmentDocument"]);

        //    var directory = ResourcesPathReader.GetDocumentSourcePath();
        //    var path = Path.Combine(directory);
        //    if (file.IsNotNull())
        //    {
        //        for (var i = 0; i < file.Length; i++)
        //        {
        //            ResourcesPathReader.IsValidFileExtention(Path.GetExtension(file[i].FileName));
        //        }
        //    }
        //    var fileId = "";
        //    var fileName = "";
        //    var filedata = _preRecruitmentDocument.GetDocFile(preRecruitmentDocument.Id);
        //    if (filedata.Count > 0)
        //    {
        //        if (!string.IsNullOrEmpty(filedata["FileId"].ToString()) &&
        //            !string.IsNullOrEmpty(filedata["FileName"].ToString()))
        //            fileId = filedata["FileId"].ToString();
        //        fileName = filedata["FileName"].ToString();

        //        if (fileName != preRecruitmentDocument.FileName)
        //            if (System.IO.File.Exists(path + fileId + Path.GetExtension(fileName)))
        //                System.IO.File.Delete(path + fileId + Path.GetExtension(fileName));
        //    }

        //    _preRecruitmentDocument.InsertORUpdateMaster(preRecruitmentDocument);
        //    if (file.IsNotNull())
        //    {
        //        foreach (var item in file)
        //        {
        //            if (item != null)
        //            {
        //                if (System.IO.File.Exists(path + item.FileName))
        //                    System.IO.File.Delete(path + fileId + Path.GetExtension(item.FileName));
        //                item.SaveAs(path + preRecruitmentDocument.Id + Path.GetExtension(item.FileName));
        //            }
        //        }
        //    }
        //    return Json(new { PreRecruitmentDocument = preRecruitmentDocument, Message = AplosMessage.Success });
        //}

        [HttpPost, Authorize]
        public JsonResult PotalBookingCreate(FormCollection form/*ExpenseBooking expenseBooking, IEnumerable<ExpenseBookingDetail> expenseBookingDetails, IEnumerable<ExpenseActivity> expActdetails*/)
        {
            var expenseBooking = new JavaScriptSerializer().Deserialize<ExpenseBooking>(form["expenseBooking"]);
            var expenseBookingDetails = new JavaScriptSerializer().Deserialize<IEnumerable<ExpenseBookingDetail>>(form["expenseBookingDetails"]);
            var expActdetails = new JavaScriptSerializer().Deserialize<IEnumerable<ExpenseActivity>>(form["expActdetails"]);

            var directory = ResourcesPathReader.GetExpensesImagePath();
            var path = Path.Combine(directory);

            if (expenseBooking.FileName.IsNotNull())
            {
                ResourcesPathReader.IsValidFileExtention(Path.GetExtension(expenseBooking.FileName));
            }

            var fileId = "";
            var fileName = "";
            var filedata = _expenseBookingService.GetExpenseBookingFile(expenseBooking.Id);
            if (filedata.Count > 0)
            {
                if (!string.IsNullOrEmpty(filedata["Id"].ToString()) &&
                    !string.IsNullOrEmpty(filedata["FileName"].ToString()))
                    fileId = filedata["Id"].ToString();
                fileName = filedata["FileName"].ToString();

                if (fileName != expenseBooking.FileName)
                    if (System.IO.File.Exists(path + fileId + Path.GetExtension(fileName)))
                        System.IO.File.Delete(path + fileId + Path.GetExtension(fileName));
            }

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            expenseBooking.CompanyGroupId = identity.CompanyGroupId;
            expenseBooking.CompanyId = identity.CompanyId;
            expenseBooking.PlantId = identity.PlantId;
            expenseBooking.AppliedBy = AppliedByBooking.Self.ToString();
            if (expenseBooking.EmployeeId == null)
                expenseBooking.EmployeeId = identity.EmployeeId;
            if(expenseBooking.ResponsiblePersonId==null)
                throw new CustomException("Please select Checked By.");

            foreach (var advanceDetailVM in expenseBookingDetails)
            {
                if (advanceDetailVM.Amount == 0 || advanceDetailVM.Amount.ToString() == null)
                    throw new CustomException("Amount should more than 0.");
            }
            _expenseBookingService.Insert(expenseBooking, expenseBookingDetails, expActdetails);
            var file = Request.Files["file"];

            if (expenseBooking.FileName.IsNotNull())
            {

                if (System.IO.File.Exists(path + expenseBooking.Id))
                    System.IO.File.Delete(path + fileId + Path.GetExtension(expenseBooking.FileName));
                file.SaveAs(path + expenseBooking.Id + Path.GetExtension(expenseBooking.FileName));
            }
            return Json(new { BudgetTransactionMaster = expenseBooking, Message = AplosMessage.Insert });
        }

       
        [HttpPost]
        public JsonResult Create(FormCollection form /*ExpenseBooking expenseBooking, IEnumerable<ExpenseBookingDetail> expenseBookingDetails, IEnumerable<ExpenseActivity> expActdetails,HttpPostedFileBase[] file*/)
        {

            var expenseBooking = new JavaScriptSerializer().Deserialize<ExpenseBooking>(form["expenseBooking"]);
            var expenseBookingDetails = new JavaScriptSerializer().Deserialize<IEnumerable<ExpenseBookingDetail>>(form["expenseBookingDetails"]);
            var expActdetails = new JavaScriptSerializer().Deserialize<IEnumerable<ExpenseActivity>>(form["expActdetails"]);

            var directory = ResourcesPathReader.GetExpensesImagePath();
            var path = Path.Combine(directory);

            if (expenseBooking.FileName.IsNotNull())
            {
                    ResourcesPathReader.IsValidFileExtention(Path.GetExtension(expenseBooking.FileName));
            }

            var fileId = "";
            var fileName = "";
            var filedata = _expenseBookingService.GetExpenseBookingFile(expenseBooking.Id);
            if (filedata.Count > 0)
            {
                if (!string.IsNullOrEmpty(filedata["Id"].ToString()) &&
                    !string.IsNullOrEmpty(filedata["FileName"].ToString()))
                    fileId = filedata["Id"].ToString();
                fileName = filedata["FileName"].ToString();

                if (fileName != expenseBooking.FileName)
                    if (System.IO.File.Exists(path + fileId + Path.GetExtension(fileName)))
                        System.IO.File.Delete(path + fileId + Path.GetExtension(fileName));
            }


            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            expenseBooking.CompanyGroupId = identity.CompanyGroupId;
            expenseBooking.CompanyId = identity.CompanyId;
            expenseBooking.PlantId = identity.PlantId;
            expenseBooking.AppliedBy = AppliedByBooking.Admin.ToString();
            if (expenseBooking.EmployeeId == null)
                throw new CustomException("Please Select Employee");
            if (expenseBooking.ResponsiblePersonId == null)
                throw new CustomException("Please Select Checked By");

            foreach (var advanceDetailVM in expenseBookingDetails)
            {
                if (advanceDetailVM.Amount == 0 || advanceDetailVM.Amount.ToString() == null)
                    throw new CustomException("Amount should more than 0.");
            }
            var file = Request.Files["file"];
            _expenseBookingService.Insert(expenseBooking, expenseBookingDetails, expActdetails);
            if (expenseBooking.FileName.IsNotNull())
            {
              
                        if (System.IO.File.Exists(path + expenseBooking.Id))
                            System.IO.File.Delete(path + fileId + Path.GetExtension(expenseBooking.FileName));
                file.SaveAs(path + expenseBooking.Id + Path.GetExtension(expenseBooking.FileName));
            }
            return Json(new { ExpenseBooking = expenseBooking, Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(FormCollection form /*ExpenseBooking expenseBooking, IEnumerable<ExpenseBookingDetail> expenseBookingDetails, IEnumerable<ExpenseActivity> expActdetails*/)
        {
            var expenseBooking = new JavaScriptSerializer().Deserialize<ExpenseBooking>(form["expenseBooking"]);
            var expenseBookingDetails = new JavaScriptSerializer().Deserialize<IEnumerable<ExpenseBookingDetail>>(form["expenseBookingDetails"]);
            var expActdetails = new JavaScriptSerializer().Deserialize<IEnumerable<ExpenseActivity>>(form["expActdetails"]);

            if(expenseBooking.ApprovalStatus== ApprovalStatus.ToBeApproved.ToString())
                throw new CustomException("Update is not allowed after Checked.");
            if (expenseBooking.ApprovalStatus == ApprovalStatus.CheckedHolded.ToString())
                throw new CustomException("Update is not allowed after Checked Holded.");
            if (expenseBooking.ApprovalStatus == ApprovalStatus.CheckedRejected.ToString())
                throw new CustomException("Update is not allowed after Checked Rejected.");

            var directory = ResourcesPathReader.GetExpensesImagePath();

            var path = Path.Combine(directory);

            if (expenseBooking.FileName.IsNotNull())
            {
                ResourcesPathReader.IsValidFileExtention(Path.GetExtension(expenseBooking.FileName));
            }

            var fileId = "";
            var fileName = "";
            var filedata = _expenseBookingService.GetExpenseBookingFile(expenseBooking.Id);
            if (filedata.Count > 0)
            {
                if (!string.IsNullOrEmpty(filedata["Id"].ToString()) &&
                    !string.IsNullOrEmpty(filedata["FileName"].ToString()))
                    fileId = filedata["Id"].ToString();
                fileName = filedata["FileName"].ToString();

                if (fileName != expenseBooking.FileName)
                    if (System.IO.File.Exists(path + fileId + Path.GetExtension(fileName)))
                        System.IO.File.Delete(path + fileId + Path.GetExtension(fileName));
            }


            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            expenseBooking.CompanyGroupId = identity.CompanyGroupId;
            expenseBooking.CompanyId = identity.CompanyId;
            expenseBooking.PlantId = identity.PlantId;
            if (expenseBooking.EmployeeId == null)
                expenseBooking.EmployeeId = identity.EmployeeId;
            foreach (var advanceDetailVM in expenseBookingDetails)
            {
                if (advanceDetailVM.Amount == 0 || advanceDetailVM.Amount.ToString() == null)
                    throw new CustomException("Amount should more than 0.");
            }
            var file = Request.Files["file"];
            
            _expenseBookingService.Update(expenseBooking, expenseBookingDetails, expActdetails);
            if (expenseBooking.FileName.IsNotNull())
            {
                if (System.IO.File.Exists(path))
                {
                    System.IO.File.Delete(path);
                    file.SaveAs(path + expenseBooking.Id + Path.GetExtension(expenseBooking.FileName));
                }
                else
                {
                file.SaveAs(path + expenseBooking.Id + Path.GetExtension(expenseBooking.FileName));
                }
            }
            return Json(new { Message = AplosMessage.Updated });
        }

        [HttpPost, Authorize]
        public JsonResult ExpenseBookingApprovalPotal(ExpenseBooking expenseBooking, IEnumerable<ExpenseBookingDetail> expenseBookingDetails)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            expenseBooking.CompanyGroupId = identity.CompanyGroupId;
            expenseBooking.CompanyId = identity.CompanyId;
            expenseBooking.PlantId = identity.PlantId;
            expenseBooking.ApprovalStatus = ApprovalStatus.Approved.ToString();
            _expenseBookingService.ExpenseBookingApprovalPotal(expenseBooking, expenseBookingDetails, identity.EmployeeId);
            expenseBooking.EmployeeId = identity.EmployeeId;
            return Json(new { Message = AplosMessage.Updated });
        }

        [HttpPost, Authorize]
        public JsonResult ExpenseBookingHold(ExpenseBooking expenseBooking, IEnumerable<ExpenseBookingDetail> expenseBookingDetailList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            expenseBooking.CompanyGroupId = identity.CompanyGroupId;
            expenseBooking.CompanyId = identity.CompanyId;
            expenseBooking.PlantId = identity.PlantId;
            expenseBooking.ApprovalStatus = ApprovalStatus.ApprovedHolded.ToString();
            _expenseBookingService.ExpenseBookingApprovalPotal(expenseBooking, expenseBookingDetailList, identity.EmployeeId);
            expenseBooking.EmployeeId = identity.EmployeeId;
            return Json(new { Message = AplosMessage.Updated });
        }

        [HttpPost, Authorize]
        public JsonResult ExpenseBookingReject(ExpenseBooking expenseBooking, IEnumerable<ExpenseBookingDetail> expenseBookingDetailList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            expenseBooking.CompanyGroupId = identity.CompanyGroupId;
            expenseBooking.CompanyId = identity.CompanyId;
            expenseBooking.PlantId = identity.PlantId;
            expenseBooking.ApprovalStatus = ApprovalStatus.ApprovedRejected.ToString();
            _expenseBookingService.ExpenseBookingApprovalPotal(expenseBooking, expenseBookingDetailList, identity.EmployeeId);
            expenseBooking.EmployeeId = identity.EmployeeId;
            return Json(new { Message = AplosMessage.Updated });
        }

        [HttpPost, Authorize]
        public JsonResult Delete(string id)
        {
            _expenseBookingService.Archive(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        [HttpPost]
        public JsonResult InsertExpenseBookingApproved(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            voucherVM.CompanyGroupId = identity.CompanyGroupId;
            voucherVM.CompanyId = identity.CompanyId;
            voucherVM.PlantId = identity.PlantId;
            voucherVM.SourceType = SourceType.EmployeePayable.ToString();
            voucherVM.PartyType = PartyType.Employee.ToString();
            if (voucherVM.EmployeeTransactionTypeId == null && voucherVM.BeneficiaryType == BeneficiaryType.Self.ToString())
                throw new CustomException("Please Select Transaction Type.");
            _expenseBookingService.InsertExpenseBookingApproved(voucherVM, voucherDetailList);
            return Json(new { Message = AplosMessage.Insert });
        }
        [HttpPost]
        public ActionResult DeleteApprovedExpenseBooking(string employeeBookingId)
        {
            _expenseBookingService.DeleteApprovedExpenseBooking(employeeBookingId);
            return Json(new { Message = AplosMessage.Deleted });
        }

        #region Checked By
        [HttpGet, Authorize]
        public JsonResult GetCheckedByList(string status)
        {
            return Json(_expenseBookingService.QueryCheckedByPoatal(status), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult CheckedQueryByCheckedBy()
        {
            return Json(_expenseBookingService.CheckedQueryByCheckedBy(), JsonRequestBehavior.AllowGet);
        }


        [HttpPost,Authorize]
        public JsonResult InsertCheckedByChecked(ExpenseBooking expenseBooking, IEnumerable<ExpenseBookingDetail> expenseBookingDetails, string ApprovedById)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            expenseBooking.CompanyGroupId = identity.CompanyGroupId;
            expenseBooking.CompanyId = identity.CompanyId;
            expenseBooking.PlantId = identity.PlantId;
            if (ApprovedById==null)
                throw new CustomException("Please Select Approved By.");
            expenseBooking.ApprovalStatus = ApprovalStatus.ToBeApproved.ToString();
            _expenseBookingService.ExpenseBookingCheckedPotal(expenseBooking, expenseBookingDetails, ApprovedById);
            expenseBooking.EmployeeId = identity.EmployeeId;
            return Json(new { Message = AplosMessage.Updated });
        }
        [HttpPost, Authorize]
        public JsonResult InsertCheckedByHold(ExpenseBooking expenseBooking, IEnumerable<ExpenseBookingDetail> expenseBookingDetailList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            expenseBooking.CompanyGroupId = identity.CompanyGroupId;
            expenseBooking.CompanyId = identity.CompanyId;
            expenseBooking.PlantId = identity.PlantId;
            expenseBooking.ApprovalStatus = ApprovalStatus.CheckedHolded.ToString();
            _expenseBookingService.ExpenseBookingApprovalPotal(expenseBooking, expenseBookingDetailList, null);
            expenseBooking.EmployeeId = identity.EmployeeId;
            return Json(new { Message = AplosMessage.Updated });
        }

        [HttpPost, Authorize]
        public JsonResult InsertCheckedByReject(ExpenseBooking expenseBooking, IEnumerable<ExpenseBookingDetail> expenseBookingDetailList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            expenseBooking.CompanyGroupId = identity.CompanyGroupId;
            expenseBooking.CompanyId = identity.CompanyId;
            expenseBooking.PlantId = identity.PlantId;
            expenseBooking.ApprovalStatus = ApprovalStatus.CheckedRejected.ToString();
            _expenseBookingService.ExpenseBookingApprovalPotal(expenseBooking, expenseBookingDetailList, null);
            expenseBooking.EmployeeId = identity.EmployeeId;
            return Json(new { Message = AplosMessage.Updated });
        }

        #endregion

        //  ExpenseBookingApprovalList
        public ActionResult ExpenseBookingApprovalList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                // if (string.IsNullOrEmpty(MasterLCList))
                //   throw new Exception("Please select at least one master Order");

                ExcelEngine excelEngine = new ExcelEngine();

                IWorkbook workbook = GetExpenseBookingApprovalList(identity.CompanyGroupId, identity.CompanyId, identity.PlantId);

                string strFileName = "Expense Booking Approval.xlsx";
                workbook.SaveAs(strFileName, ExcelSaveType.SaveAsXLS, System.Web.HttpContext.Current.Response, ExcelDownloadType.PromptDialog);
                workbook.Close();
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);

            }
            return null;
        }

        // GetExpenseBookingApprovalList
        private IWorkbook GetExpenseBookingApprovalList(string companyGroupId, string companyId, string plantId)
        {

            //Start EmployeeAdvanceDueList

         
            ExcelEngine excelEngine = new ExcelEngine();
            //Instantiate the Excel application object
            IApplication application = excelEngine.Excel;

            //Set the default application version
            application.DefaultVersion = ExcelVersion.Excel2013;

            //Load the existing Excel workbook into IWorkbook
            IWorkbook workbook = application.Workbooks.Create(1);

            //Get the first worksheet in the workbook into IWorksheet
            IWorksheet worksheet = workbook.Worksheets[0];
            DataTable dtExpenseBookingApprovalList = _sqlRepository.GetDataTable(@"SELECT DISTINCT EB.Id, EB.EmployeeId,EB.PartyId, EI.EmployeeCode, EI.EmployeeName , EB.PartyPlantId, P.UserName AS PartyName,EBD.Amount,EB.BeneficiaryType,
					                EIH.EmployeeCode AS ApproverCode, EIH.EmployeeName AS ApprovedBy, EB.CurrencyId, C.Code AS CurrencyName
									, EB.InvoiceNumber, format(EB.InvoiceDate,'dd-MMM-yyyy') as InvoiceDate, EB.ApprovalStatus, EB.Remarks
                                    ,EIR.EmployeeCode +'-'+EIR.EmployeeName CheckedBy
                                    FROM [TRN].[ExpenseBooking] AS EB
                                    LEFT JOIN [dbo].[EmployeeInformation] AS EI ON EI.SystemId=EB.EmployeeId
                                    LEFT JOIN [SCS].[Currency] C ON EB.CurrencyId=C.Id
							        LEFT JOIN [TRN].[ExpenseBookingApprovalHistory] AS EAH ON EAH.ExpenseBookingId=EB.Id
                                    LEFT JOIN [dbo].[EmployeeInformation] AS EIH ON EIH.SystemId=EAH.EmployeeId
                                    LEFT JOIN [dbo].[EmployeeInformation] AS EIR ON EIR.SystemId=EB.ResponsiblePersonId
									LEFT JOIN [HKP].[Party] AS P ON P.Id=EB.PartyId
									LEFT JOIN (SELECT ExpenseBookingId,sum(Amount) AS Amount  FROM [TRN].[ExpenseBookingDetail] GROUP BY ExpenseBookingId) AS EBD ON EBD.ExpenseBookingId=EB.Id
                                    WHERE EB.Archive=0 AND EB.CompanyGroupId='CG20171' AND EB.CompanyId='C20171' AND EB.PlantId='20171' AND EB.ApprovalStatus='Approved' AND EB.IsPosted=0  ");

            if (dtExpenseBookingApprovalList.Rows.Count == 0)
                throw new Exception("No data found");




            worksheet.Name = "ExpenseBookingApprovalListReport";

            int COL = 1; int ROW = 5;
            int startCol = COL;

            // worksheet[ROW, COL].Text = "Employee Advance Due List Details:";
            // worksheet[ROW, COL].CellStyle.Font.Bold = true;
            //  ROW++;

            worksheet[ROW, COL].Text = "Employee";
            int colEmployee = COL;
            worksheet[ROW, COL].ColumnWidth = 25;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Benificiary";
            int colBenificiary  = COL;
            worksheet[ROW, COL].ColumnWidth = 10;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Invoice Number";
            int colInvoiceNumber = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Invoice Date";
            int colInvoiceDate = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;


            worksheet[ROW, COL].Text = "Checked By";
            int colCheckedBy = COL;
            worksheet[ROW, COL].ColumnWidth = 25;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Approved By";
            int colApprovedBy = COL;
            worksheet[ROW, COL].ColumnWidth = 22;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Currency";
            int colCurrency = COL;
            worksheet[ROW, COL].ColumnWidth = 8;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            //worksheet[ROW, COL].Number = clsStaticInfo.dbl(dtEmployeeAdvanceDueList.Rows[0]["Receivable"].ToString());
            // worksheet[ROW, COL].NumberFormat = clsStaticInfo.NumberFormat();
            // worksheet.Range[MasterOrderDetailsStartRow, leftColumnCaption, ROW, RightColumnValue].CellStyle.Interior.ColorIndex = ExcelKnownColors.Custom44;

            COL++;

            worksheet[ROW, COL].Text = "Amount";
            int colAmount = COL;
            worksheet[ROW, COL].ColumnWidth = 13;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            //worksheet[ROW, COL].Number = clsStaticInfo.dbl(dtEmployeeAdvanceDueList.Rows[0]["Received"].ToString());
            // worksheet[ROW, COL].NumberFormat = clsStaticInfo.NumberFormat();
            //COL++;

            //worksheet[ROW, COL].Text = "Balance";
            //int colBalance = COL;
            //worksheet[ROW, COL].ColumnWidth = 15;
            //worksheet[ROW, COL].CellStyle.Font.Bold = true;
            //worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            //worksheet[ROW, COL].Number = clsStaticInfo.dbl(dtEmployeeAdvanceDueList.Rows[0]["Balance"].ToString());
            //worksheet[ROW, COL].NumberFormat = clsStaticInfo.NumberFormat();
            // COL++;

            // int ROW = 6; int COL = 1;

            //int EmployeeAdvanceDueListStartRow  = ROW;
            //worksheet[ROW, COL].Text = "Employee Advance Due List Details:";
            //worksheet[ROW, COL].CellStyle.Font.Bold = true;
            //ROW++;
            int endCol = COL;
            worksheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
            worksheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
            ROW++;

            for (int i = 0; i < dtExpenseBookingApprovalList.Rows.Count; i++)
            {
                // int i = 0; i < dtMasterOrderItem.Rows.Count; i++
                worksheet[ROW, colEmployee].Text = dtExpenseBookingApprovalList.Rows[i]["EmployeeName"].ToString();
                worksheet[ROW, colBenificiary ].Text = dtExpenseBookingApprovalList.Rows[i]["BeneficiaryType"].ToString();
                worksheet[ROW, colInvoiceNumber].Text = dtExpenseBookingApprovalList.Rows[i]["InvoiceNumber"].ToString();
                worksheet[ROW, colInvoiceDate].Text = dtExpenseBookingApprovalList.Rows[i]["InvoiceDate"].ToString();
                worksheet[ROW, colCheckedBy].Text = dtExpenseBookingApprovalList.Rows[i]["CheckedBy"].ToString();
                worksheet[ROW, colApprovedBy].Text = dtExpenseBookingApprovalList.Rows[i]["ApprovedBy"].ToString();
                worksheet[ROW, colCurrency].Text = dtExpenseBookingApprovalList.Rows[i]["CurrencyName"].ToString();
                
                worksheet[ROW, colAmount].Number = clsStaticInfo.dbl(dtExpenseBookingApprovalList.Rows[i]["Amount"].ToString());
                worksheet[ROW, colAmount].NumberFormat = clsStaticInfo.NumberFormat();
               

               
                //worksheet[ROW, colWriteOff].Number = clsStaticInfo.dbl(dtEmployeeAdvanceDueList.Rows[i]["Received"].ToString());
                //worksheet[ROW, colWriteOff].NumberFormat = clsStaticInfo.NumberFormat();
                //worksheet[ROW, colBalance].Number = clsStaticInfo.dbl(dtEmployeeAdvanceDueList.Rows[i]["Balance"].ToString());
                //worksheet[ROW, colBalance].NumberFormat = clsStaticInfo.NumberFormat();
                //worksheet[ROW, colPurchaseLCCurrencyId].Text = dsData.Tables[0].Rows[i]["PurchasePLCurrency"].ToString();




                // worksheet[startRowGroup1, colSLNO, ROW - 1, colSLNO].Merge();
                //worksheet[StartDataRow, colPurchaseLCAmount, ROW - 1, colPurchaseLCAmount].NumberFormat = "#,##0.00;(#,##0.00)";
                worksheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                worksheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);

                ROW++;

            }

            worksheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
            worksheet.UsedRange.CellStyle.Font.Size = 8f;


            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ReportUtility reportUtility = new ReportUtility();
            reportUtility.PlantHeaderWithOutLogo(ref worksheet, endCol, "Expense Booking Approval", identity.PlantId);

            //reportUtility.PlantHeader(ref worksheet, endCol, "Employee Advance" , identity.PlantId);
            reportUtility.PageSetup(ref worksheet, 5, ExcelPageOrientation.Landscape);
            // worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            worksheet.Range[1, 1, 4, endCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;

            worksheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
            worksheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
            worksheet.IsGridLinesVisible = false;

            return workbook;
        }
    }
}