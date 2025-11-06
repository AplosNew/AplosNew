using Aplos.Controllers;
using Aplos.Properties;
using Library.Accounting.Accounts;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Model.Banks;
using Library.Model.Enums;
using Library.Model.Organizations;
using Library.Service.Advances;
using Library.Service.Employees;
using Library.Service.Helpers;
using Library.Service.HumanResources.Profile;
using Library.Service.SalaryDisbursement;
using Library.Service.Systems;
using Library.ViewModel.Vouchers;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.Accounts.Controllers
{
    public class SalaryDisbursementController : BaseController
    {
        private readonly ISalaryDisbursementService _salaryDisbursementService;
        private readonly IEmployeePayableService _employeePayableService;
        private readonly IEmployeePayableWriteOffService _employeePayableWirteOffService;
        private readonly IAdvanceWriteOffService _advanceWriteOffService;
        private readonly ISqlRepository _sqlRepository;
        private readonly ISalaryHeadGLService _salaryHeadGLService;
        private readonly IRepositoryAsync<Company> _companyRepository;

        public SalaryDisbursementController(
            ISalaryDisbursementService salaryDisbursementService
            , IEmployeePayableService employeePayableService
            , IEmployeePayableWriteOffService employeePayableWirteOffService
            , IAdvanceWriteOffService advanceWriteOffService
            , ISalaryHeadGLService salaryHeadGLService
            , IRepositoryAsync<Company> companyRepository
            , ISqlRepository sqlRepository)
        {
            _salaryDisbursementService = salaryDisbursementService;
            _employeePayableService = employeePayableService;
            _employeePayableWirteOffService = employeePayableWirteOffService;
            _advanceWriteOffService = advanceWriteOffService;
            _salaryHeadGLService = salaryHeadGLService;
            _companyRepository = companyRepository;
            _sqlRepository = sqlRepository;
        }

        #region SalaryPayable

        [Authorize, AllowAnonymous]
        public ActionResult Aplos()
        {
            return View();
        }

      
        public ActionResult PaymentAdviseReport()
        {
            return View();
        }


        public ActionResult SalaryPayable()
        {
            return View("~/Areas/Accounts/Views/SalaryDisbursement/SalaryPayable.cshtml");
        }


        public ActionResult SalaryPayableDisbursement()
        {
            return View("~/Areas/Accounts/Views/SalaryDisbursement/SalaryPayableDisbursement.cshtml");
        }
        public ActionResult BonusDisbursement()
        {
            return View("~/Areas/Accounts/Views/SalaryDisbursement/BonusDisbursement.cshtml");
        }
        public ActionResult BonusDisbursementPost()
        {
            return View("~/Areas/Accounts/Views/SalaryDisbursement/BonusDisbursementPost.cshtml");
        }
        public ActionResult SalaryDisbursementPost()
        {
            return View("~/Areas/Accounts/Views/SalaryDisbursement/SalaryDisbursementPost.cshtml");
        }
        public ActionResult FinalSettlementPost()
        {
            return View("~/Areas/Accounts/Views/SalaryDisbursement/FinalSettlementPost.cshtml");
        }

        [Authorize, HttpGet]
        public JsonResult GetSalaryLockDataList(string yearNo, string monthNo, string employeeId, bool isActive, bool isSeperated, bool isMaternity, string entityId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            AccountsSalaryPayableService accountsSalaryPayableService = new AccountsSalaryPayableService(_sqlRepository);
            return Json(accountsSalaryPayableService.GetSalaryLockDataList(yearNo, monthNo, employeeId, isActive, isSeperated, isMaternity, identity.PlantId, entityId), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult GetSalaryLockCTCDataList(string yearNo, string monthNo, string employeeId, bool isActive, bool isSeperated, bool isMaternity, string entityId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            AccountsSalaryPayableService accountsSalaryPayableService = new AccountsSalaryPayableService(_sqlRepository);
            return Json(accountsSalaryPayableService.GetSalaryLockCTCDataList(yearNo, monthNo, employeeId, isActive, isSeperated, isMaternity, identity.PlantId, entityId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetSalaryLockDataGLList(string yearNo, string monthNo, string employeeId, bool isActive, bool isSeperated, bool isMaternity, string entityId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            AccountsSalaryPayableService accountsSalaryPayableService = new AccountsSalaryPayableService(_sqlRepository);
            return Json(accountsSalaryPayableService.GetSalaryLockDataGLList(yearNo, monthNo, employeeId, isActive, isSeperated, isMaternity, identity.PlantId, entityId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetSalaryLockInDirectTakeAwayDataList(string yearNo, string monthNo, string employeeId, bool isActive, bool isSeperated, bool isMaternity, string entityId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            AccountsSalaryPayableService accountsSalaryPayableService = new AccountsSalaryPayableService(_sqlRepository);
            return Json(accountsSalaryPayableService.GetSalaryLockInDirectTakeAwayDataList(yearNo, monthNo, employeeId, isActive, isSeperated, isMaternity, identity.PlantId, entityId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetSalaryLockInDirectCTCDataList(string yearNo, string monthNo, string employeeId, bool isActive, bool isSeperated, bool isMaternity, string entityId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            AccountsSalaryPayableService accountsSalaryPayableService = new AccountsSalaryPayableService(_sqlRepository);
            return Json(accountsSalaryPayableService.GetSalaryLockInDirectCTCDataList(yearNo, monthNo, employeeId, isActive, isSeperated, isMaternity, identity.PlantId, entityId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetSalaryLockInDirectDataGLList(string yearNo, string monthNo, string employeeId, bool isActive, bool isSeperated, bool isMaternity, string entityId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            AccountsSalaryPayableService accountsSalaryPayableService = new AccountsSalaryPayableService(_sqlRepository);
            return Json(accountsSalaryPayableService.GetSalaryLockInDirectDataGLList(yearNo, monthNo, employeeId, isActive, isSeperated, isMaternity, identity.PlantId, entityId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetListWithCombine()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var commany = _companyRepository.Find(identity.CompanyId);
            return Json(_salaryHeadGLService.GetSalaryHeadGLCombine(commany.COAId), JsonRequestBehavior.AllowGet);
        }


        [Authorize, HttpPost]
        public JsonResult GetDirectSalaryLockSalarySheetData(string yearNo, string monthNo, bool isActive, bool isSeperated, bool isMaternity, string entityId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            AccountsSalaryPayableService accountsSalaryPayableService = new AccountsSalaryPayableService(_sqlRepository);
            return Json(accountsSalaryPayableService.GetDirectSalaryLockSalarySheetData(yearNo, monthNo, isActive, isSeperated, isMaternity, identity.PlantId, entityId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public JsonResult GetInDirectSalaryLockSalarySheetData(string yearNo, string monthNo, bool isActive, bool isSeperated, bool isMaternity, string entityId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            AccountsSalaryPayableService accountsSalaryPayableService = new AccountsSalaryPayableService(_sqlRepository);
            return Json(accountsSalaryPayableService.GetInDirectSalaryLockSalarySheetData(yearNo, monthNo, isActive, isSeperated, isMaternity, identity.PlantId, entityId), JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        public JsonResult ParkSalaryPayable(VoucherViewModel voucherVM, string yearNo, string monthNo, string monthName
            , IEnumerable<VoucherDetailViewModel> directJVList, IEnumerable<VoucherDetailViewModel> inDirectJVList
            , IEnumerable<VoucherDetailViewModel> directSalaryLockList, IEnumerable<VoucherDetailViewModel> indirectSalaryLockList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            voucherVM.CompanyGroupId = identity.CompanyGroupId;
            voucherVM.CompanyId = identity.CompanyId;
            voucherVM.PlantId = identity.PlantId;
            voucherVM.IsPark = true;
            int year = Int32.Parse(yearNo);
            int month = Int32.Parse(monthNo);

            int monthdays = System.DateTime.DaysInMonth(year, month);
            DateTime dt = new DateTime(year, month, 1);
            dt = dt.AddDays(monthdays - 1);
            if (voucherVM.PostingDate > dt)
                throw new CustomException("Posting Date must in the selected month of " + monthName);

            voucherVM.SourceType = SourceType.SalaryPayable.ToString();
            if (directJVList != null && directJVList.Sum(r => r.DrAmount) != directJVList.Sum(r => r.CrAmount))
                throw new CustomException("Direct Salary Dr and Cr Amount not match!");
            if (inDirectJVList != null && inDirectJVList.Sum(r => r.DrAmount) != inDirectJVList.Sum(r => r.CrAmount))
                throw new CustomException("InDirect Salary Dr and Cr Amount not match!");
            return Json(new { Message = string.Format(AplosMessage.VoucherSave, _salaryDisbursementService.ParkSalaryPayable(voucherVM, yearNo, monthNo, monthName, directJVList, inDirectJVList, directSalaryLockList, indirectSalaryLockList)) });
        }

        [HttpGet, Authorize]
        public JsonResult GetSalaryPayableVoucherList(GridParameter parameters)
        {
            return Json(_salaryDisbursementService.GetSalaryPayableVoucherList(parameters), JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public ActionResult DeleteSalaryPayable(string voucherId, string monthNo, string yearNo)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            _salaryDisbursementService.DeleteSalaryPayable(identity.PlantId, voucherId, monthNo, yearNo);
            return Json(new { Message = AplosMessage.Deleted });
        }

        [HttpGet, Authorize]
        public ActionResult GetSalaryPayableVoucherReport(ReportFormat reportFormat, string voucherId)
        {
            AccountsSalaryPayableService accountsSalaryPayableService = new AccountsSalaryPayableService(_sqlRepository);

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var workbook = accountsSalaryPayableService.GetSalaryPayableVoucherReport(out string reportFileName, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, voucherId);
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


        [HttpPost, Authorize]
        public ActionResult GetEmployeeSalaryProcessedReportSalaryLogWiseInVoucher(string month, string year, string salaryProcessId, string payRollGroup, Dictionary<string, string> parameters, bool isActive, bool isSeperated, bool isMaternity, string voucherId)
        {
            try
            {

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                var fileName = month + "-" + year + "SalarySheet" + DateTime.Now.ToString("yyMMdd") + identity.Name + ".xls";
                string fullPath = System.Web.Hosting.HostingEnvironment.MapPath("~/") + fileName;


                //var workbook = _salaryDisbursementService.GetEmployeeSalaryProcessedReportSalaryLogWiseInVoucher(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.UserId, identity.PlantName, voucherId);
                var workbook = _salaryDisbursementService.GetEmployeeSalaryProcessedReportSalaryLogWiseInVoucher(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.UserId, month, year, salaryProcessId, payRollGroup, parameters, isActive, isSeperated, isMaternity, false, voucherId);

                workbook.Version = ExcelVersion.Excel97to2003;
                workbook.SaveAs(fullPath);

                return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Message = ex.Message, Error = true }, JsonRequestBehavior.AllowGet);

            }
        }

        [HttpPost, Authorize]
        public ActionResult GetEmployeeSalaryProcessedReportSalaryLogWiseSalaryPayableInVoucher(string month, string year, string salaryProcessId, string payRollGroup, Dictionary<string, string> parameters, bool isActive, bool isSeperated, bool isMaternity, string voucherId, string Mode, string EmpBank)
        {
            try
            {

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                AccountsSalaryPayableService accountsSalaryPayableService = new AccountsSalaryPayableService(_sqlRepository);

                var fileName = month + "-" + year + "SalarySheet" + DateTime.Now.ToString("yyMMdd") + identity.Name + ".xls";
                string fullPath = System.Web.Hosting.HostingEnvironment.MapPath("~/") + fileName;


                // var workbook = _salaryDisbursementService.GetEmployeeSalaryProcessedReportSalaryLogWiseSalaryPayableInVoucher(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.UserId, month, year, salaryProcessId, payRollGroup, parameters, isActive, isSeperated, isMaternity, false, voucherId);
                var workbook = accountsSalaryPayableService.GetEmployeeSalaryProcessedReportSalaryLogWiseSalaryPayableInVoucher(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.UserId, month, year, salaryProcessId, payRollGroup, parameters, isActive, isSeperated, isMaternity, false, voucherId, Mode, EmpBank);

                workbook.Version = ExcelVersion.Excel97to2003;
                workbook.SaveAs(fullPath);

                return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Message = ex.Message, Error = true }, JsonRequestBehavior.AllowGet);

            }
        }



        [Authorize, HttpPost]
        public JsonResult GetDirectSalaryPayableDisbursementDataList(string yearNo, string monthNo, string disbursementAdviceId, List<SalaryLock> employeeListNew)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string EmpIdLoop = "";
            string EmpSystemIds = "";
            if(employeeListNew!=null)
            {
                foreach (var item in employeeListNew)
                {
                    if (EmpIdLoop == "")
                    {
                        EmpIdLoop = "'" + item.EmpSystemId + "'"; ;
                    }
                    else
                    {
                        EmpIdLoop += ",'" + item.EmpSystemId + "'";

                    }
                }
            }
            
            if(EmpIdLoop != "")
            {
                EmpSystemIds = " and sl.EmpSystemId IN (" + EmpIdLoop + @")";
            }
            string sql = null;
            sql = @"SELECT
                X.GLName,X.BudgetName,X.ActivityName, SUM(X.DrAmount) DrAmount,SUM(X.CrAmount) CrAmount,SUM(X.DisbusmentAmount) DisbusmentAmount,X.GLGeneralInfoId,X.BudgetMasterId,X.ActivityId
                FROM
                (
    select sh.SalaryHead,sh.[Sequence], sl.YearNo, sl.MonthNo, sh.HeadType
                , 0 DrAmount
                , CrAmount =case when SUM(spc.DisbusmentAmount) < 0 then SUM(spc.DisbusmentAmount) * -1 else SUM(spc.DisbusmentAmount) end
                , SUM(spc.DisbusmentAmount) DisbusmentAmount
                    ,vd.GLGeneralInfoId 
				, vd.BudgetMasterId
				,vd.ActivityId
                , CDGL.AccountCode + ' - ' + CDGL.UserName GLName
                    , CDB.UserName BudgetName
                    , CDA.UserName ActivityName
                from[dbo].[SalaryLock] sl
                left join dbo.SalaryProcMaster spm on   spm.MonthNo = sl.MonthNo and spm.YearNo = sl.YearNo
                left join dbo.SalaryProcChild spc on spc.SlrProcMstSystemID = spm.SystemID and sl.EmpSystemId = spc.EmpInfoSystemID
                left join dbo.SalaryProcessLogDetail spd on   spd.EmpSystemId=sl.EmpSystemId and spm.SystemID=spd.SalaryProcessId
                left join dbo.SalaryHead sh on sh.SalaryHeadID = spc.SalaryHeadID
                left join dbo.EmployeeInformation ei on ei.SystemId = sl.EmpSystemId
                left join MST.ManpowerBudget MPB on MPB.Id = ei.BudgetCode
                left join ORG.Position PO on PO.Id = MPB.PositionId
                left join trn.Voucher v on v.Id=sl.PayableVoucherId
				left join trn.VoucherDetail vd on vd.VoucherId=v.Id and vd.TrnNature ='Net Pay' and vd.SalaryHeadId=sh.SalaryHeadID and Vd.AccountsGroupId=sl.AccountsGroupId
					LEFT JOIN HKP.GLGeneralInfo CDGL ON CDGL.Id=vd.GLGeneralInfoId
                    LEFT JOIN MST.BudgetMaster CDBM ON CDBM.Id=vd.BudgetMasterId
                    LEFT JOIN HKP.Budget CDB ON CDB.Id=CDBM.BudgetId
                    LEFT JOIN HKP.Activity CDA ON CDA.Id=vd.ActivityId
                where sl.MonthNo = '" + monthNo + "' and sl.YearNo = '" + yearNo + @"'  AND sl.PayableVoucherId<>'' AND sl.DisbursementVoucherId IS NULL AND sl.PastDisbursed IS NULL
                and sl.IsDisbursed=1 
                and ISNULL(sh.SalaryHead, '')  in ('Net Pay') and spc.DisbusmentAmount != 0 
                and sl.DisbursementAdviceId='" + disbursementAdviceId + @"' " + EmpSystemIds + @"
                       
                group by sh.SalaryHead, sl.YearNo, sl.MonthNo, sh.HeadType, sh.[Sequence]
                ,vd.GLGeneralInfoId,vd.BudgetMasterId,vd.ActivityId
                , CDGL.AccountCode, CDGL.UserName, CDB.UserName, CDA.UserName
                        
                )X
                GROUP BY

                X.GLName,X.BudgetName,X.ActivityName,X.GLGeneralInfoId,X.BudgetMasterId,X.ActivityId
                ORDER BY 5";
                
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetEmployeeDisbursementDataList(string yearNo, string monthNo, string disbursementAdviceId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = null;
                sql = @" select isSelected = Convert(bit, 'True'),sl.EmpSystemId,sl.YearNo,sl.MonthNo,ei.EmployeeCode,ei.EmployeeName,d.UserName Designation,DA.PaymentMode,spd.BankAccNo,spd.IFSCCode
                        ,DirectManpowerCost=case when po.DirectManpowerCost=0 then 'No' when po.DirectManpowerCost=1 then 'Yes' end ,b.UserName BankName,v.VoucherNo PayableVoucherNo
                        ,spc.DisbusmentAmount Amount,spd.Id
						,Department.UserName Department,Department.Id DepartmentId
						,EmpC.UserName EmployeeCategory, EmpC.Id EmpCategoryId
						,Section.UserName Section,Section.Id SectionId
						,SubSection.UserName SubSection,SubSection.Id SubSectionId
						,isnull(L.Id,'') LineId,isnull(L.UserName,'') Line
						,IsLock = case when sl.IsLocked = 1 then 'Locked' else 'Unlocked' end
						,ISNULL(vl.VoucherNo,'') as DisbursementVoucherNo,ISNULL(sl.DisbursementVoucherId,'') DisbursementVoucherId
						,IsDisburse = case when sl.IsDisbursed = 1 then 'Disbursed' else 'Not Disbursed' end
						,ISNULL(REPLACE(CONVERT(VARCHAR(11), ei.DOJ, 106), ' ', '-'),'') DOJ
						,ISNULL(PG.UserName,'') PayRollGroup
						,ISNULL(jl.JobLocation, '') JobLocation
                        ,ISNULL(REPLACE(CONVERT(VARCHAR(11), ei.DOS, 106), ' ', '-'),'') DOS
						,Case when Isnull(SPM.SalaryProcFlag,'') = '' THen 'Regular' else SalaryProcFlag end SalaryProcFlag
                        ,ISNULL(Division.UserName,'') Division ,ISNULL(Division.Id,'') DivisionId
                        ,sl.DisbursementAdviceId,DA.Remarks,SPM.SystemID SalaryProcId,SPM.AddedBy
                        ,CASE WHEN sl.MonthNo=1 THEN 'January'
			                    WHEN sl.MonthNo=2 THEN 'February'
			                    WHEN sl.MonthNo=3 THEN 'March'
			                    WHEN sl.MonthNo=4 THEN 'April'
			                    WHEN sl.MonthNo=5 THEN 'May'
			                    WHEN sl.MonthNo=6 THEN 'June'
			                    WHEN sl.MonthNo=7 THEN 'July'
			                    WHEN sl.MonthNo=8 THEN 'August'
			                    WHEN sl.MonthNo=9 THEN 'September'
			                    WHEN sl.MonthNo=10 THEN 'October'
			                    WHEN sl.MonthNo=11 THEN 'November'
			                    WHEN sl.MonthNo=12 THEN 'December'
			                    ELSE '' END MonthName
                        ,ISNULL(REPLACE(CONVERT(VARCHAR(11), DA.AddedDate, 106), ' ', '-'),'') AdviceDate
                        ,CASE WHEN MONTH(DOS) =  sl.MonthNo  AND YEAR(DOS) = sl.YearNo then 'Separated' else 'Active' end CurrentMonthEmployeeStatus
						,ISNULL(ei.EmployeeStatus,'') EmployeeStatus
                        from [dbo].[SalaryLock] sl 
                        left join dbo.SalaryProcMaster spm on   spm.MonthNo=sl.MonthNo and spm.YearNo=sl.YearNo
                        left join dbo.SalaryProcChild spc on spc.SlrProcMstSystemID=spm.SystemID and sl.EmpSystemId=spc.EmpInfoSystemID
						left join dbo.SalaryProcessLogDetail spd on   spd.EmpSystemId=sl.EmpSystemId and spm.SystemID=spd.SalaryProcessId
                        left join dbo.EmployeeInformation ei on ei.SystemId=sl.EmpSystemId
						left join MST.ManpowerBudget MPB on MPB.Id=ei.BudgetCode
						left join ORG.Position PO on PO.Id=MPB.PositionId
                        LEFT OUTER JOIN ORG.Entity EN ON MPB.EntityId=EN.Id
						LEFT JOIN [ORG].[Department] ON Department.Id = PO.DepartmentId
						LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = ei.GivenDesignationId
                        LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId
						LEFT JOIN [ORG].[Section] ON Section.Id = PO.SectionId
                        LEFT JOIN [ORG].[SubSection] ON SubSection.Id = PO.SubSectionId
						LEFT JOIN ORG.Line AS L ON L.Id= MPB.LineId
                        LEFT JOIN [ORG].[Division] ON Division.Id = EN.DivisionId
						left join dbo.SalaryHead sh on sh.SalaryHeadID=spc.SalaryHeadID
						left join hkp.Designation d on d.Id=spd.DesignationId
						Left outer join MST.PayrollGroupMaster PGM ON PGM.employeeid = ei.SystemId
						Left outer join HKP.PayrollGroup PG ON PG.id = PGM.PayrollGroupId
						Left Join [dbo].[JobLocation] jl on jl.SystemID = ei.JobLocationID
						left join hkp.Bank b on spd.BankSystemID=b.Id
						left join trn.Voucher v on v.Id=sl.PayableVoucherId
                        LEFT JOIN TRN.Voucher  Vl ON Vl.Id=sl.DisbursementVoucherId 
                        LEFT JOIN [dbo].[DisbursementAdvice]  DA ON DA.Id=sl.DisbursementAdviceId
                        where sl.MonthNo='" + monthNo + "' and sl.YearNo='" + yearNo + @"'  AND sl.PayableVoucherId<>'' AND sl.DisbursementVoucherId IS NULL and sl.IsDisbursed=1 AND sl.PastDisbursed IS NULL
                        and sl.DisbursementAdviceId='" + disbursementAdviceId + @"'
                         and spc.DisbusmentAmount!=0  
                        and spd.PlantId='" + identity.PlantId + @"' 
						 and ISNULL(sh.SalaryHead, '')  in ('Net Pay')";
            //return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            var jsondata = Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }

        [Authorize, HttpGet]
        public JsonResult GetEmployeeBonusDisbursementDataList(string disbursementAdviceId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = null;
            sql = @" select isSelected = Convert(bit, 'True'),sl.EmpSystemId,sl.YearNo,sl.MonthNo,ei.EmployeeCode,ei.EmployeeName,d.UserName Designation,DA.PaymentMode,spd.BankAccNo,spd.IFSCCode
                        ,DirectManpowerCost=case when po.DirectManpowerCost=0 then 'No' when po.DirectManpowerCost=1 then 'Yes' end ,b.UserName BankName,v.VoucherNo PayableVoucherNo
                        ,spc.DisbusmentAmount Amount,sl.Id
						,Department.UserName Department,Department.Id DepartmentId
						,EmpC.UserName EmployeeCategory, EmpC.Id EmpCategoryId
						,Section.UserName Section,Section.Id SectionId
						,SubSection.UserName SubSection,SubSection.Id SubSectionId
						,isnull(L.Id,'') LineId,isnull(L.UserName,'') Line
						,IsLock = case when sl.IsLocked = 1 then 'Locked' else 'Unlocked' end
						,ISNULL(vl.VoucherNo,'') as DisbursementVoucherNo,ISNULL(sl.BonusDisbursementVoucherId,'') BonusDisbursementVoucherId
						,IsBonusDisbursed = case when sl.IsBonusDisbursed = 1 then 'Disbursed' else 'Not Disbursed' end
						,ISNULL(REPLACE(CONVERT(VARCHAR(11), ei.DOJ, 106), ' ', '-'),'') DOJ
						,ISNULL(PG.UserName,'') PayRollGroup
						,ISNULL(jl.JobLocation, '') JobLocation
                        ,ISNULL(REPLACE(CONVERT(VARCHAR(11), ei.DOS, 106), ' ', '-'),'') DOS
						,Case when Isnull(SPM.SalaryProcFlag,'') = '' THen 'Regular' else SalaryProcFlag end SalaryProcFlag
                        ,ISNULL(Division.UserName,'') Division ,ISNULL(Division.Id,'') DivisionId
                        ,sl.BonusDisbursementAdviceId,DA.Remarks,SPM.SystemID SalaryProcId,SPM.AddedBy
                        ,CASE WHEN sl.MonthNo=1 THEN 'January'
			                    WHEN sl.MonthNo=2 THEN 'February'
			                    WHEN sl.MonthNo=3 THEN 'March'
			                    WHEN sl.MonthNo=4 THEN 'April'
			                    WHEN sl.MonthNo=5 THEN 'May'
			                    WHEN sl.MonthNo=6 THEN 'June'
			                    WHEN sl.MonthNo=7 THEN 'July'
			                    WHEN sl.MonthNo=8 THEN 'August'
			                    WHEN sl.MonthNo=9 THEN 'September'
			                    WHEN sl.MonthNo=10 THEN 'October'
			                    WHEN sl.MonthNo=11 THEN 'November'
			                    WHEN sl.MonthNo=12 THEN 'December'
			                    ELSE '' END MonthName
                        ,ISNULL(REPLACE(CONVERT(VARCHAR(11), DA.AddedDate, 106), ' ', '-'),'') AdviceDate
                        ,CASE WHEN MONTH(DOS) =  sl.MonthNo  AND YEAR(DOS) = sl.YearNo then 'Separated' else 'Active' end CurrentMonthEmployeeStatus
						,ISNULL(ei.EmployeeStatus,'') EmployeeStatus
                        from [dbo].[SalaryLock] sl 
                        left join dbo.SalaryProcMaster spm on   spm.MonthNo=sl.MonthNo and spm.YearNo=sl.YearNo
                        left join dbo.SalaryProcChild spc on spc.SlrProcMstSystemID=spm.SystemID and sl.EmpSystemId=spc.EmpInfoSystemID
						left join dbo.SalaryProcessLogDetail spd on   spd.EmpSystemId=sl.EmpSystemId and spm.SystemID=spd.SalaryProcessId
                        left join dbo.EmployeeInformation ei on ei.SystemId=sl.EmpSystemId
						left join MST.ManpowerBudget MPB on MPB.Id=ei.BudgetCode
						left join ORG.Position PO on PO.Id=MPB.PositionId
                        LEFT OUTER JOIN ORG.Entity EN ON MPB.EntityId=EN.Id
						LEFT JOIN [ORG].[Department] ON Department.Id = PO.DepartmentId
						LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = ei.GivenDesignationId
                        LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId
						LEFT JOIN [ORG].[Section] ON Section.Id = PO.SectionId
                        LEFT JOIN [ORG].[SubSection] ON SubSection.Id = PO.SubSectionId
						LEFT JOIN ORG.Line AS L ON L.Id= MPB.LineId
                        LEFT JOIN [ORG].[Division] ON Division.Id = EN.DivisionId
						left join dbo.SalaryHead sh on sh.SalaryHeadID=spc.SalaryHeadID
						left join hkp.Designation d on d.Id=spd.DesignationId
						Left outer join MST.PayrollGroupMaster PGM ON PGM.employeeid = ei.SystemId
						Left outer join HKP.PayrollGroup PG ON PG.id = PGM.PayrollGroupId
						Left Join [dbo].[JobLocation] jl on jl.SystemID = ei.JobLocationID
						left join hkp.Bank b on spd.BankSystemID=b.Id
						left join trn.Voucher v on v.Id=sl.PayableVoucherId
                        LEFT JOIN TRN.Voucher  Vl ON Vl.Id=sl.BonusDisbursementVoucherId 
                        LEFT JOIN [dbo].[BonusDisbursementAdvice]  DA ON DA.Id=sl.BonusDisbursementAdviceId
                        where sl.PayableVoucherId<>'' AND sl.BonusDisbursementVoucherId IS NULL and sl.IsBonusDisbursed=1 
                        and sl.BonusDisbursementAdviceId='" + disbursementAdviceId + @"'
                        and spc.DisbusmentAmount!=0  
                        and spd.PlantId='" + identity.PlantId + @"' 
						and ISNULL(SH.HeadCategory, '')  in ('Annual Bonus Retain')
                        ORDER BY ei.EmployeeCode,sl.YearNo,sl.MonthNo ";
            //return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            var jsondata = Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }

        [Authorize, HttpPost]
        public JsonResult GetDirectBonusPayableDisbursementDataList(string disbursementAdviceId, List<SalaryLock> employeeListNew)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string EmpIdLoop = "";
            string EmpSystemIds = "";
            if (employeeListNew != null)
            {
                foreach (var item in employeeListNew)
                {
                    if (EmpIdLoop == "")
                    {
                        EmpIdLoop = "'" + item.Id + "'"; ;
                    }
                    else
                    {
                        EmpIdLoop += ",'" + item.Id + "'";

                    }
                }
            }

            if (EmpIdLoop != "")
            {
                EmpSystemIds = " and sl.Id IN (" + EmpIdLoop + @")";
            }
            string sql = null;
            sql = @"SELECT
                X.GLName,X.BudgetName,X.ActivityName, SUM(X.DrAmount) DrAmount,SUM(X.CrAmount) CrAmount,SUM(X.DisbusmentAmount) DisbusmentAmount,X.GLGeneralInfoId,X.BudgetMasterId,X.ActivityId
                FROM
                (
    -- select sh.SalaryHead,sh.[Sequence], sl.YearNo, sl.MonthNo, sh.HeadType
    --            , 0 DrAmount
    --            , CrAmount =case when SUM(spc.DisbusmentAmount) < 0 then SUM(spc.DisbusmentAmount) * -1 else SUM(spc.DisbusmentAmount) end
    --            , SUM(spc.DisbusmentAmount) DisbusmentAmount
    --                ,vd.GLGeneralInfoId 
	--			  , vd.BudgetMasterId
	--			  ,vd.ActivityId
    --            , CDGL.AccountCode + ' - ' + CDGL.UserName GLName
    --                , CDB.UserName BudgetName
    --                , CDA.UserName ActivityName
    --            from[dbo].[SalaryLock] sl
    --            left join dbo.SalaryProcMaster spm on   spm.MonthNo = sl.MonthNo and spm.YearNo = sl.YearNo
    --            left join dbo.SalaryProcChild spc on spc.SlrProcMstSystemID = spm.SystemID and sl.EmpSystemId = spc.EmpInfoSystemID
    --            left join dbo.SalaryProcessLogDetail spd on   spd.EmpSystemId=sl.EmpSystemId and spm.SystemID=spd.SalaryProcessId
    --            left join dbo.SalaryHead sh on sh.SalaryHeadID = spc.SalaryHeadID
    --            left join dbo.EmployeeInformation ei on ei.SystemId = sl.EmpSystemId
    --            left join MST.ManpowerBudget MPB on MPB.Id = ei.BudgetCode
    --            left join ORG.Position PO on PO.Id = MPB.PositionId
    --            left join trn.Voucher v on v.Id=sl.PayableVoucherId
	--			  left join trn.VoucherDetail vd on vd.VoucherId=v.Id and vd.TrnNature ='Monthly Bonus' and vd.SalaryHeadId=sh.SalaryHeadID and Vd.AccountsGroupId=sl.AccountsGroupId and vd.CrAmount>0
	--			  LEFT JOIN HKP.GLGeneralInfo CDGL ON CDGL.Id=vd.GLGeneralInfoId
    --            LEFT JOIN MST.BudgetMaster CDBM ON CDBM.Id=vd.BudgetMasterId
    --            LEFT JOIN HKP.Budget CDB ON CDB.Id=CDBM.BudgetId
    --            LEFT JOIN HKP.Activity CDA ON CDA.Id=vd.ActivityId
    --            where sl.PayableVoucherId<>'' AND sl.BonusDisbursementVoucherId IS NULL and sl.IsBonusDisbursed=1 
    --            and ISNULL(SH.HeadCategory, '')  in ('Monthly Bonus Retain') and spc.DisbusmentAmount != 0  
    --            and sl.BonusDisbursementAdviceId='" + disbursementAdviceId + @"' " + EmpSystemIds + @"
                       
    --             group by sh.SalaryHead, sl.YearNo, sl.MonthNo, sh.HeadType, sh.[Sequence]
    --             ,vd.GLGeneralInfoId,vd.BudgetMasterId,vd.ActivityId
    --             , CDGL.AccountCode, CDGL.UserName, CDB.UserName, CDA.UserName

    --          UNION ALL
                select sh.SalaryHead,sh.[Sequence], sl.YearNo, sl.MonthNo, sh.HeadType
                , 0 DrAmount
                , CrAmount =case when SUM(spc.DisbusmentAmount) < 0 then SUM(spc.DisbusmentAmount) * -1 else SUM(spc.DisbusmentAmount) end
                , SUM(spc.DisbusmentAmount) DisbusmentAmount
                    ,vd.GLGeneralInfoId 
				, vd.BudgetMasterId
				,vd.ActivityId
                , CDGL.AccountCode + ' - ' + CDGL.UserName GLName
                    , CDB.UserName BudgetName
                    , CDA.UserName ActivityName
                from[dbo].[SalaryLock] sl
                left join dbo.SalaryProcMaster spm on   spm.MonthNo = sl.MonthNo and spm.YearNo = sl.YearNo
                left join dbo.SalaryProcChild spc on spc.SlrProcMstSystemID = spm.SystemID and sl.EmpSystemId = spc.EmpInfoSystemID
                left join dbo.SalaryProcessLogDetail spd on   spd.EmpSystemId=sl.EmpSystemId and spm.SystemID=spd.SalaryProcessId
                left join dbo.SalaryHead sh on sh.SalaryHeadID = spc.SalaryHeadID
                left join dbo.EmployeeInformation ei on ei.SystemId = sl.EmpSystemId
                left join MST.ManpowerBudget MPB on MPB.Id = ei.BudgetCode
                left join ORG.Position PO on PO.Id = MPB.PositionId
                left join trn.Voucher v on v.Id=sl.PayableVoucherId
				left join trn.VoucherDetail vd on vd.VoucherId=v.Id and vd.TrnNature ='Annual Bonus' and vd.SalaryHeadId=sh.SalaryHeadID and Vd.AccountsGroupId=sl.AccountsGroupId and vd.CrAmount>0 
				LEFT JOIN HKP.GLGeneralInfo CDGL ON CDGL.Id=vd.GLGeneralInfoId
                LEFT JOIN MST.BudgetMaster CDBM ON CDBM.Id=vd.BudgetMasterId
                LEFT JOIN HKP.Budget CDB ON CDB.Id=CDBM.BudgetId
                LEFT JOIN HKP.Activity CDA ON CDA.Id=vd.ActivityId
                where sl.PayableVoucherId<>'' AND sl.BonusDisbursementVoucherId IS NULL and sl.IsBonusDisbursed=1 
                and ISNULL(SH.HeadCategory, '')  in ('Annual Bonus Retain') and spc.DisbusmentAmount != 0 
                and sl.BonusDisbursementAdviceId='" + disbursementAdviceId + @"' " + EmpSystemIds + @"
                       
                group by sh.SalaryHead, sl.YearNo, sl.MonthNo, sh.HeadType, sh.[Sequence]
                ,vd.GLGeneralInfoId,vd.BudgetMasterId,vd.ActivityId
                , CDGL.AccountCode, CDGL.UserName, CDB.UserName, CDA.UserName
                        
                )X
                GROUP BY

                X.GLName,X.BudgetName,X.ActivityName,X.GLGeneralInfoId,X.BudgetMasterId,X.ActivityId
                ORDER BY 5";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetBankList(string yearNo, string monthNo)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var sql = @"select Id Value, UserName Text from hkp.bank where Id IN(
                    select distinct spd.BankSystemID
                    from dbo.SalaryLock sl 
                    join dbo.SalaryProcMaster sm  on sl.MonthNo=sm.MonthNo and sl.YearNo=sm.YearNo
                    left join dbo.SalaryProcChild spc on spc.SlrProcMstSystemID=sm.SystemID 
                    left join dbo.SalaryProcessLogDetail spd on spd.EmpSystemId=spc.EmpInfoSystemID
                     WHERE sl.MonthNo='" + monthNo + "' and sl.YearNo='" + yearNo + "' AND sl.PayableVoucherId<>'' AND sl.IsDisbursed=1 and spd.PlantId='" + identity.PlantId + @"' 
                      )   ";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);

        }

        [HttpGet, Authorize]
        public JsonResult GetBankMasterList(GridParameter parameters, BankACType bankACType, string bankId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(GetBankMasterData(parameters, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, bankId, bankACType), JsonRequestBehavior.AllowGet);
        }
        private GridModel GetBankMasterData(GridParameter parameters, string companyGroupId, string companyId, string plantId, string bankId, BankACType type)
        {

            parameters.CmdText = @"SELECT BM.Id AS BankMasterId, BM.AccountTitle, BM.AccountNumber, BM.GLGeneralInfoId, GL.AccountCode AS GLGeneralInfoCode, GL.UserName AS GLGeneralInfoName
                                    , BM.BudgetMasterId, BU.Code AS BudgetCode, BU.UserName AS BudgetName, BM.ActivityId, A.Code AS ActivityCode, A.UserName AS ActivityName
                                    , ACT.UserName AS BankAccountTypeName, BM.BankId, BM.Code AS BankCode, B.UserName AS BankName, BM.BankBranchId, BB.Code AS BankBranchCode, BB.UserName AS BankBranchName
                                    , BM.CurrencyId, C.Code AS CurrencyCode, C.[Name] AS CurrencyName, BM.EntityId
                                    FROM [MST].[BankMaster] AS BM
                                    LEFT JOIN [HKP].[GLGeneralInfo] As GL ON GL.Id=BM.GLGeneralInfoId
                                    LEFT JOIN [MST].[BudgetMaster] AS BUM ON BUM.Id=BM.BudgetMasterId
                                    LEFT JOIN [HKP].[Budget] AS BU ON BU.Id=BUM.BudgetId
                                    LEFT JOIN [HKP].[Activity] AS A ON A.Id=BM.ActivityId
                                    LEFT JOIN [HKP].[BankAccountType] AS ACT ON ACT.Id=BM.BankAccountTypeId
                                    LEFT JOIN [HKP].[Bank] AS B ON B.Id=BM.BankId
                                    LEFT JOIN [HKP].[BankBranch] AS BB ON BB.Id=BM.BankBranchId
                                    LEFT JOIN [SCS].Currency AS C ON C.Id=BM.CurrencyId
                                    WHERE BM.Archive=0 AND BM.Active=1 AND BM.CompanyGroupId='" + companyGroupId + "' AND BM.CompanyId='" + companyId + "' AND BM.PlantId='" + plantId + @"'" +
                                " AND BM.AccountType='" + type + "' ";
            return _sqlRepository.GetGridData(parameters);
        }
        [HttpGet, Authorize]
        public JsonResult GetDisbursementAdviceData()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var sql = @"SELECT  [Id], [YearNo], [MonthNo], [Status], [Remarks], [PaymentMode]
                          ,CASE WHEN [MonthNo]=1 THEN 'January'
			                    WHEN [MonthNo]=2 THEN 'February'
			                    WHEN [MonthNo]=3 THEN 'March'
			                    WHEN [MonthNo]=4 THEN 'April'
			                    WHEN [MonthNo]=5 THEN 'May'
			                    WHEN [MonthNo]=6 THEN 'June'
			                    WHEN [MonthNo]=7 THEN 'July'
			                    WHEN [MonthNo]=8 THEN 'August'
			                    WHEN [MonthNo]=9 THEN 'September'
			                    WHEN [MonthNo]=10 THEN 'October'
			                    WHEN [MonthNo]=11 THEN 'November'
			                    WHEN [MonthNo]=12 THEN 'December'
			                    ELSE '' END MonthName
                         ,(SELECT SUM(spc.DisbusmentAmount)DisbursementAmount from [dbo].[SalaryLock] sl 
                            left join dbo.SalaryProcMaster spm on   spm.MonthNo=sl.MonthNo and spm.YearNo=sl.YearNo
                            left join dbo.SalaryProcChild spc on spc.SlrProcMstSystemID=spm.SystemID and sl.EmpSystemId=spc.EmpInfoSystemID
						    left join dbo.SalaryHead sh on sh.SalaryHeadID = spc.SalaryHeadID
						    WHERE sl.DisbursementAdviceId=DA.Id and ISNULL(sh.SalaryHead, '')  in ('Net Pay') and spc.DisbusmentAmount != 0)DisbursementAmount
                        FROM [dbo].[DisbursementAdvice] DA WHERE DA.Status<>'Close' ";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);

        }
        [HttpGet, Authorize]
        public JsonResult GetFinalSettlementDataForDisbursement()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var sql = @"SELECT  [Id], [EmpSystemId], [SeparationTypeId], FORMAT([FinalSettlementDate],'dd-MMM-yyyy')[FinalSettlementDate], [FormulaDes], [PolicyYearNo], [PolicyDayNo], [SeparationTypeAmount], 
	                    [GratuityAmount], [LvEncashmentAmount], [EarningAmount], [DeductionAmount], [GrossAmount], [BasicAmount], [OTRate], [SalaryRate], [TenureDayNo]
	                    , [TenureMonthNo], [TenureYearNo], [Remarks], [EarnLvDeductionDayNo], [EarnLvDeductionAmount], [TotalRetainedAmount], [NoticePeriodDayNo]
	                    , [NoticePeriodAmount], [NoticePeriodRate], [NoticePeriodType], [PolicyFixedDayNo], [FixedDayAmount], [LvEncashmentDayNo], [LvEncashmentRateAmount]
	                    , [LastMonthProcDay], [LastMonthNetPayAmount], [LastMonthAbsentDay], [LastMonthOTHour], [StampAmount], [LastMonthGrossAmount], [LastMonthAbsenteeismAmount], [LastMonthOTAmount]
	                    , [TotalPayableAmount], [TotalDeductionAmount], [NetPayAmount], [GratuityDayOrYear], [GratuityNoOfDaysOrYear], [GratuityRate], [DisbursementVoucherId]
	                    , ISNULL(E.EmployeeCode,'') EmployeeCode ,ISNULL(E.EmployeeName,'') EmployeeName	
	                    FROM [dbo].[EmployeeFinalSettlement] EFS
	                    LEFT JOIN EmployeeInformation E on E.SystemId= EFS.EmpSystemId
	                    WHERE  DisbursementVoucherId IS NULL ";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);

        }
        [Authorize, HttpPost]
        public JsonResult GetFinalSettlementDisbursementJVDataList(VoucherViewModel voucherVM,string disbursementAdviceId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
           
            string sql = null;
            sql = @"SELECT
                OtherName,TrnType,X.GLName,X.BudgetName,X.ActivityName, SUM(X.DrAmount) DrAmount,SUM(X.CrAmount) CrAmount,SUM(X.Amount) Amount,X.GLGeneralInfoId,X.BudgetMasterId,X.ActivityId
                FROM
                (
                select 'Salary' AS OtherName, 'Dr' AS TrnType
                , DrAmount =case when SUM(spc.DisbusmentAmount) < 0 then SUM(spc.DisbusmentAmount) * -1 else SUM(spc.DisbusmentAmount) end
                , 0 CrAmount 
                , SUM(spc.DisbusmentAmount) Amount
                ,vd.GLGeneralInfoId , vd.BudgetMasterId,vd.ActivityId, CDGL.AccountCode + ' - ' + CDGL.UserName GLName
                , CDB.UserName BudgetName, CDA.UserName ActivityName
                from[dbo].[SalaryLock] sl
                left join dbo.SalaryProcMaster spm on   spm.MonthNo = sl.MonthNo and spm.YearNo = sl.YearNo
                left join dbo.SalaryProcChild spc on spc.SlrProcMstSystemID = spm.SystemID and sl.EmpSystemId = spc.EmpInfoSystemID
                left join dbo.SalaryProcessLogDetail spd on   spd.EmpSystemId=sl.EmpSystemId and spm.SystemID=spd.SalaryProcessId
                left join dbo.SalaryHead sh on sh.SalaryHeadID = spc.SalaryHeadID
                left join trn.Voucher v on v.Id=sl.PayableVoucherId
				left join trn.VoucherDetail vd on vd.VoucherId=v.Id and vd.TrnNature ='Net Pay' and vd.SalaryHeadId=sh.SalaryHeadID and Vd.AccountsGroupId=sl.AccountsGroupId
				LEFT JOIN HKP.GLGeneralInfo CDGL ON CDGL.Id=vd.GLGeneralInfoId
                LEFT JOIN MST.BudgetMaster CDBM ON CDBM.Id=vd.BudgetMasterId
                LEFT JOIN HKP.Budget CDB ON CDB.Id=CDBM.BudgetId
                LEFT JOIN HKP.Activity CDA ON CDA.Id=vd.ActivityId
                where sl.EmpSystemId = '" + voucherVM.EmployeeId + @"'  AND sl.PayableVoucherId<>'' AND sl.DisbursementVoucherId IS NULL 
                and sl.EmployeeFinalSettlementId = '" + disbursementAdviceId + @"'
                and ISNULL(sh.SalaryHead, '')  in ('Net Pay') and spc.DisbusmentAmount != 0     
                group by sh.SalaryHead, sl.YearNo, sl.MonthNo, sh.HeadType, sh.[Sequence]
                ,vd.GLGeneralInfoId,vd.BudgetMasterId,vd.ActivityId
                , CDGL.AccountCode, CDGL.UserName, CDB.UserName, CDA.UserName

                Union All
				SELECT  'FinalSettlementAdjustment' AS OtherName, 'Dr' AS TrnType
                , CASE WHEN (NetPayAmount+AdvanceAmount-LastMonthNetPayAmount)>0 THEN (NetPayAmount+AdvanceAmount-LastMonthNetPayAmount) ELSE 0 END DrAmount 
                , 0 CrAmount 
                , (NetPayAmount+AdvanceAmount-LastMonthNetPayAmount) Amount
                ,GAD.GLGeneralInfoId  ,GAD.BudgetMasterId,GAD.ActivityId, GL.AccountCode + ' - ' + GL.UserName GLName
                , B.UserName BudgetName,A.UserName ActivityName 
				FROM [dbo].[EmployeeFinalSettlement] EFS
				LEFT JOIN HKP.GeneralAccountDeterminate GAD ON  GAD.Id='FinalSettlementAdjustment'
				LEFT JOIN[HKP].[GLGeneralInfo] AS GL ON GAD.GLGeneralInfoId=GL.Id
				LEFT JOIN[MST].[BudgetMaster] AS BM ON GAD.BudgetMasterId= BM.Id
				LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
				LEFT JOIN [HKP].[Activity] AS A ON GAD.ActivityId= A.Id
				WHERE  EFS.Id = '" + disbursementAdviceId + @"'

				Union All
				SELECT  'FinalSettlementAdjustment' AS OtherName, 'Cr' AS TrnType
                , 0 DrAmount 
                , CASE WHEN (LastMonthNetPayAmount-(NetPayAmount+AdvanceAmount))>0 THEN (LastMonthNetPayAmount-(NetPayAmount+AdvanceAmount)) ELSE 0 END CrAmount 
                , (LastMonthNetPayAmount-(NetPayAmount+AdvanceAmount)) Amount
                ,GAD.GLGeneralInfoId  ,GAD.BudgetMasterId,GAD.ActivityId, GL.AccountCode + ' - ' + GL.UserName GLName
                , B.UserName BudgetName,A.UserName ActivityName 
				FROM [dbo].[EmployeeFinalSettlement] EFS
				LEFT JOIN HKP.GeneralAccountDeterminate GAD ON  GAD.Id='FinalSettlementAdjustment'
				LEFT JOIN[HKP].[GLGeneralInfo] AS GL ON GAD.GLGeneralInfoId=GL.Id
				LEFT JOIN[MST].[BudgetMaster] AS BM ON GAD.BudgetMasterId= BM.Id
				LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
				LEFT JOIN [HKP].[Activity] AS A ON GAD.ActivityId= A.Id
				WHERE  EFS.Id = '" + disbursementAdviceId + @"'

                Union All
				SELECT  'Advance' AS OtherName, 'Cr' AS TrnType
                , 0 DrAmount 
                , AdvanceAmount CrAmount 
                , AdvanceAmount Amount
                ,AD.GLGeneralInfoId  ,AD.BudgetMasterId,AD.ActivityId, GL.AccountCode + ' - ' + GL.UserName GLName
                , B.UserName BudgetName,A.UserName ActivityName 
				FROM [dbo].[EmployeeFinalSettlement] EFS
				LEFT JOIN (select EmployeeId,GLGeneralInfoId  ,BudgetMasterId,ActivityId 
									FROM [TRN].[AdvanceDetail] group by EmployeeId,GLGeneralInfoId  ,BudgetMasterId,ActivityId) AD	ON AD.EmployeeId=EFS.EmpSystemId
				LEFT JOIN[HKP].[GLGeneralInfo] AS GL ON AD.GLGeneralInfoId=GL.Id
				LEFT JOIN[MST].[BudgetMaster] AS BM ON AD.BudgetMasterId= BM.Id
				LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
				LEFT JOIN [HKP].[Activity] AS A ON AD.ActivityId= A.Id
				WHERE  EFS.Id = '" + disbursementAdviceId + @"'

                Union All
				SELECT  'Bank/Cash' AS OtherName, 'Cr' AS TrnType
                , 0 DrAmount 
                , NetPayAmount CrAmount 
                , NetPayAmount Amount
                ,'" + voucherVM.GLGeneralInfoId + @"' GLGeneralInfoId ,'" + voucherVM.BudgetMasterId + @"' BudgetMasterId,'" + voucherVM.ActivityId + @"' ActivityId
                , '" + voucherVM.GLGeneralInfoName + @"' GLName , '" + voucherVM.BudgetName + @"' BudgetName,'" + voucherVM.ActivityName + @"' ActivityName 
				FROM [dbo].[EmployeeFinalSettlement] 
				WHERE  Id = '" + disbursementAdviceId + @"'
                        
                )X
                WHERE X.Amount>0
                GROUP BY
                OtherName,TrnType,X.GLName,X.BudgetName,X.ActivityName,X.GLGeneralInfoId,X.BudgetMasterId,X.ActivityId
                ORDER BY TrnType DESC";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetFinalSettlementDisbursementVoucherList(GridParameter parameters)
        {
            AccountsSalaryPayableService accountsSalaryPayableService = new AccountsSalaryPayableService(_sqlRepository);
            return Json(accountsSalaryPayableService.GetFinalSettlementDisbursementVoucherList(parameters), JsonRequestBehavior.AllowGet);
        }

        //[HttpPost]
        //public JsonResult ParkFinalSettlementDisbursement(VoucherViewModel voucherVM, string pMode, IEnumerable<VoucherDetailViewModel> directJVList, string disbursementAdviceId)
        //{
        //    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
        //    voucherVM.CompanyGroupId = identity.CompanyGroupId;
        //    voucherVM.CompanyId = identity.CompanyId;
        //    voucherVM.PlantId = identity.PlantId;
        //    voucherVM.IsPark = true;
        //    voucherVM.Amount = directJVList.Sum(r => r.CrAmount);
        //    voucherVM.SourceType = SourceType.FinalSettlementJournal.ToString();
        //    return Json(new { Message = string.Format(AplosMessage.VoucherSave, _salaryDisbursementService.ParkFinalSettlementDisbursement(voucherVM, directJVList, disbursementAdviceId)) });
        //}
        [HttpPost]
        public JsonResult PostSalaryPayable(string voucherId)
        {
            _salaryDisbursementService.PostSalarydisbursement(voucherId);
            return Json(new { Message = AplosMessage.Posted });
        }
        [HttpPost]
        public JsonResult PostFinalSettlementdisbursement(string voucherId)
        {
            _salaryDisbursementService.PostSalarydisbursement(voucherId);
            return Json(new { Message = AplosMessage.Posted });
        }
        [HttpPost]
        public ActionResult DeleteFinalSettlementDisbursementVoucher(string voucherId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            _salaryDisbursementService.DeleteFinalSettlementDisbursementVoucher(identity.PlantId, voucherId);
            return Json(new { Message = AplosMessage.Deleted });
        }


        [HttpGet, Authorize]
        public JsonResult GetSalaryPayableDisbursementVoucherList(GridParameter parameters)
        {
            AccountsSalaryPayableService accountsSalaryPayableService = new AccountsSalaryPayableService(_sqlRepository);
            return Json(accountsSalaryPayableService.GetSalaryPayableDisbursementVoucherList(parameters), JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult ParkSalaryPayableDisbursement(VoucherViewModel voucherVM, string yearNo, string monthNo, string monthName, string pMode, IEnumerable<VoucherDetailViewModel> directJVList, string disbursementAdviceId, List<SalaryLock> employeeListNew)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            voucherVM.CompanyGroupId = identity.CompanyGroupId;
            voucherVM.CompanyId = identity.CompanyId;
            voucherVM.PlantId = identity.PlantId;
            voucherVM.IsPark = true;
            voucherVM.Amount = directJVList.Sum(r => r.CrAmount);
            voucherVM.SourceType = SourceType.SalaryDisbursement.ToString();
            
            string empSystemIds = "";
            if (employeeListNew != null)
            {
                foreach (var item in employeeListNew)
                {
                    if (empSystemIds == "")
                    {
                        empSystemIds = "'" + item.EmpSystemId + "'"; ;
                    }
                    else
                    {
                        empSystemIds += ",'" + item.EmpSystemId + "'";

                    }
                }
            }

            return Json(new { Message = string.Format(AplosMessage.VoucherSave, _salaryDisbursementService.ParkSalaryPayableDisbursement(voucherVM, yearNo, monthNo, monthName, pMode, directJVList, disbursementAdviceId, empSystemIds)) });
        }
        [HttpPost]
        public JsonResult PostSalarydisbursement(string voucherId)
        {
            _salaryDisbursementService.PostSalarydisbursement(voucherId);
            return Json(new { Message = AplosMessage.Posted });
        }
        [HttpPost]
        public ActionResult DeleteSalaryDisbursementVoucher(string voucherId, string monthNo, string yearNo)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            _salaryDisbursementService.DeleteSalaryDisbursementVoucher(identity.PlantId, voucherId, monthNo, yearNo);
            return Json(new { Message = AplosMessage.Deleted });
        }

        [HttpGet, Authorize]
        public ActionResult GetSalaryDisbursementVoucherReport(ReportFormat reportFormat, string voucherId)
        {
            AccountsSalaryPayableService accountsSalaryPayableService = new AccountsSalaryPayableService(_sqlRepository);

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var workbook = accountsSalaryPayableService.GetSalaryDisbursementVoucherReport(out string reportFileName, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, voucherId);
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
        public ActionResult GetGoodWorkExtraOTDisbursementVoucherReport(ReportFormat reportFormat, string voucherId, string voucherTypeName)
        {
            AccountsSalaryPayableService accountsSalaryPayableService = new AccountsSalaryPayableService(_sqlRepository);

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var workbook = accountsSalaryPayableService.GetGoodWorkExtraOTDisbursementVoucherReport(out string reportFileName, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, voucherId, voucherTypeName);
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
        #region Salary Disbusment ---------------------------------

        [HttpPost, Authorize]
        public ActionResult GetEmpInfo(string effectiveDate, string salaryProcessId, bool isActive, bool isSeperated, bool isMaternity, string paymentMode)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            bool sa = identity.IsSysAdmin;
            bool ca = identity.IsControlAdmin;
            string userId = identity.UserId;
            string plantId = identity.PlantId;
            string companyGroupId = identity.CompanyGroupId;
            var wcPayrollGroup = "";
            string wcEmpStatus = " Where (1=0 ";

            if (sa == true || ca == true)
            {
                wcPayrollGroup = @"";
            }
            //else
            //{
            //    wcPayrollGroup = @"AND E.SystemId  IN (SELECT employeeid from MST.PayrollGroupMaster where PayrollGroupId IN (SELECT PayrollGroupId FROM SEC.UserPayrollGroup where UserId = '" + userId + @"'))";
            //}
            if (salaryProcessId == "STRUCTURE")
            {
                wcEmpStatus = " Where (1=1 ";
            }
            else
            {
                wcEmpStatus = " Where (1=0 ";

                if (isActive == true && isSeperated == true && isMaternity == true)
                {
                    wcEmpStatus = " Where (1=1 ";
                }
                else
                {
                    if (isActive == true)
                    {
                        wcEmpStatus += " OR SalaryProcFlag ='Regular'";
                    }
                    if (isSeperated == true)
                    {
                        wcEmpStatus += " OR SalaryProcFlag ='SEPARATED'";
                    }
                    if (isMaternity == true)
                    {
                        wcEmpStatus += " OR SalaryProcFlag ='MLV_PRE'";

                    }
                }
            }

            wcEmpStatus += ")";

            string sql = @"select [isSelect] = Convert(bit, 'True'),[isToBeSelect] = Convert(bit, 'False'),* FROM (  SELECT   dISTINCT   
                                     isnull(e.SystemId,'') EmpSystemId
									,ISNULL(e.EmployeeId,'')  EmployeeId 
	                                ,sl.Id,CheckBoxSelect=  CONVERT(bit,0) 
									,SPM.MonthNo,SPM.YearNo ,sl.IsLocked AS Lock
                                    ,ISNULL(e.EmployeeCode,'') EmployeeCode
                                    ,ISNULL(e.EmployeeName,'') EmployeeName								
                                    ,ISNULL(mpb.EntityId,'') EntityId
									,ISNULL(mpb.PositionId,'') PositionId                                     
                                    ,isnull(ISNULL(egdsg.UserName,ld.UserName),'') Designation                                       
									,ISNULL(Department.UserName,'') Department 
									,ISNULL(Division.UserName,'') Division 
									,ISNULL(EmpC.UserName,'') EmployeeCategory
									,ISNULL(Plant.UserName,'') Plant 
									,ISNULL(Section.UserName,'') Section 
									,ISNULL(SubSection.UserName,'') SubSection 
									,ISNULL(Unit.UserName,'') Unit 
                                    ,ISNULL(eL.UserName,'') Line
                                    ,ISNULL(REPLACE(CONVERT(VARCHAR(11), e.DOJ, 106), ' ', '-'),'') DOJ
                                    ,ISNULL(REPLACE(CONVERT(VARCHAR(11), e.DOS, 106), ' ', '-'),'') DOS
                                    , CASE WHEN MONTH(DOS) =  MONTH('" + effectiveDate + @"')  AND YEAR(DOS) = YEAR('" + effectiveDate + @"') then 'Separated' else 'Active' end CurrentMonthEmployeeStatus
                                    ,ISNULL(e.EmployeeStatus,'') EmployeeStatus
                                    , Case when Isnull(SPM.SalaryProcFlag,'') = '' THen 'Regular' else SalaryProcFlag end SalaryProcFlag
									,ISNULL(PG.UserName,'') PayRollGroup
                                    ,e.EmployeeCodePreFix,e.EmployeeCodeNumeric
                                    ,ISNULL(jl.JobLocation, '') JobLocation
									,ISNULL(e.PaymentMode,'') PaymentMode
									,ISNULL(bb.UserName,'') BankName
                                    ,ISNULL(v.VoucherNo,'' ) VoucherNo
                                    ,ISNULL(sl.PayableVoucherId,'') PayableVoucherId
                                    ,ISNULL(sl.DisbursementVoucherId,'') DisbursementVoucherId
                                    ,ISNULL(v.VoucherNo,'') as PayableVoucherNo
                                    ,ISNULL(vl.VoucherNo,'') as DisbursementVoucherNo
                                    ,sl.IsDisbursed
                                    ,IsLock = case when sl.IsLocked = 1 then 'Locked' else 'Unlocked' end
                                    ,IsDisburse = case when sl.IsDisbursed = 1 then 'Disbursed' else 'Not Disbursed' end 
                                    ,(select SPC.DisbusmentAmount NetPayment from SalaryProcChild SPC
                                    left join dbo.SalaryHead SH on SH.SalaryHeadID = SPC.SalaryHeadID
                                    JOIN SalaryProcMaster SPM ON SPM.SystemID = SPC.SlrProcMstSystemID and spm.MonthNo = Month('" + effectiveDate + @"') and spm.YearNo = Year('" + effectiveDate + @"')
                                    Where HeadCategory='Net Payable' AND SPC.EmpInfoSystemID=e.SystemId) NetPayment
                                    ,SPM.SystemID SalaryProcId,SPM.AddedBy,AG.UserName AccountsGroup
                                    ,FORMAT(ISNULL(sl.UpdatedDate,sl.AddedDate),'dd-MMM-yyyy') DisbursementDate
                                    ,isnull(sl.DisbursementAdviceId,'')DisbursementAdviceId,isnull(DA.Remarks,'')Remarks
                                    from SalaryProcessLogDetail s
                                    JOIN SalaryProcMaster SPM ON SPM.SystemID = s.SalaryProcessId and spm.MonthNo = Month('" + effectiveDate + @"') and spm.YearNo = Year('" + effectiveDate + @"')
                                    left join EmployeeInformation e on e.SystemId= s.EmpSystemId
                                    LEFT OUTER JOIN HKP.Designation egdsg on egdsg.id=s.DesignationId
                                    LEFT OUTER JOIN HKP.LegalDesignation  ld on ld.Id=s.LegalDesignationId
                                    LEFT OUTER JOIN (select dm.DesignationGroupId,dm.DesignationId,dm.EmployeeCategoryId
                                    ,dg.UserName GivenDesignationGroup
                                    FROM mst.DesignationMaster dm
                                    LEFT OUTER JOIN HKP.DesignationGroup dg on dg.Id=dm.DesignationGroupId
                                    ) egdsgg on egdsgg.DesignationId=e.GivenDesignationId
                                    AND egdsgg.EmployeeCategoryId=s.EmployeeCategoryId
                                    LEFT OUTER JOIN MST.ManpowerBudget mpb on mpb.Id=s.BudgetCode
                                    LEFT OUTER JOIN ORG.Position PO ON mpb.PositionId=PO.Id
                                    LEFT OUTER JOIN ORG.Entity EN ON mpb.EntityId=EN.Id
                                    LEFT JOIN [ORG].[Department] ON Department.Id = PO.DepartmentId
                                    LEFT JOIN [ORG].[Division] ON Division.Id = EN.DivisionId
                                    LEFT JOIN [ORG].[Plant] ON Plant.Id = EN.PlantId
                                    LEFT JOIN [ORG].[Section] ON Section.Id = PO.SectionId
                                    LEFT JOIN [ORG].[SubSection] ON SubSection.Id = PO.SubSectionId
                                    LEFT JOIN [ORG].[Unit] ON Unit.Id = EN.UnitId                                   
                                    LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = E.GivenDesignationId
                                    LEFT JOIN SCS.DesignationMasterConfiguration DMC ON DMC.DesignationMasterId=DesM.Id
									LEFT JOIN dbo.AccountsGroup AG ON AG.Id=DMC.AccountsGroupId
                                    LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId			                                       
                                    LEFT JOIN ORG.Line AS eL ON eL.Id= mpb.LineId
                                    Left outer join MST.PayrollGroupMaster PGM ON PGM.employeeid = E.SystemId
                                    Left outer join HKP.PayrollGroup PG ON PG.id = PGM.PayrollGroupId
                                    Left Join [dbo].[JobLocation] jl on jl.SystemID = E.JobLocationID
                                    left join [HKP].[Bank] bb on bb.Id = s.BankSystemID
                                    Left join SalaryLock sl on sl.EmpSystemId=e.SystemId and sl.YearNo=YEAR('" + effectiveDate + @"') AND SL.MonthNo=Month('" + effectiveDate + @"')
                                    LEFT JOIN TRN.Voucher  V ON V.Id=sl.PayableVoucherId 
                                    LEFT JOIN TRN.Voucher  Vl ON Vl.Id=sl.DisbursementVoucherId 
                                    LEFT JOIN [dbo].[DisbursementAdvice]  DA ON DA.Id=sl.DisbursementAdviceId 
                                    WHERE  s.CompanyGroupId='" + identity.CompanyGroupId + "' AND s.PlantId='" + identity.PlantId + "' AND ISNULL(DA.PaymentMode,'')='" + paymentMode + "' AND ISNULL(sl.PayableVoucherId,'')<>'' and sl.islocked=1 AND sl.IsDisbursed = 1 AND sl.PastDisbursed IS NULL " + wcPayrollGroup + @" 
                                    ) DD " + wcEmpStatus + @" ORDER BY EmployeeCodePreFix,EmployeeCodeNumeric";
            var empdata = _sqlRepository.GetDataCollection(sql);
            JsonResult json = Json(new { empdata}, JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;
            return json;
        }

        [HttpPost, Authorize]
        public ActionResult GetEmployeeInformation(string effectiveDate, string salaryProcessId, bool isActive, bool isSeperated, bool isMaternity,string employeeCategoryId,string PaymentMode)
        {

            string pm = "'" + PaymentMode.Replace(",", "','") + "'";//replaced with ""
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            bool sa = identity.IsSysAdmin;
            bool ca = identity.IsControlAdmin;
            string userId = identity.UserId;
            string plantId = identity.PlantId;
            string companyGroupId = identity.CompanyGroupId;
            var wcPayrollGroup = "";
            string wcEmpStatus = " Where (1=0 ";

            if (sa == true || ca == true)
            {
                wcPayrollGroup = @"";
            }
            //else
            //{
            //    wcPayrollGroup = @"AND E.SystemId  IN (SELECT employeeid from MST.PayrollGroupMaster where PayrollGroupId IN (SELECT PayrollGroupId FROM SEC.UserPayrollGroup where UserId = '" + userId + @"'))";
            //}
            if (salaryProcessId == "STRUCTURE")
            {
                wcEmpStatus = " Where (1=1 ";
            }
            else
            {
                wcEmpStatus = " Where (1=0 ";

                if (isActive == true && isSeperated == true && isMaternity == true)
                {
                    wcEmpStatus = " Where (1=1 ";
                }
                else
                {
                    if (isActive == true)
                    {
                        wcEmpStatus += " OR SalaryProcFlag ='Regular'";
                    }
                    if (isSeperated == true)
                    {
                        wcEmpStatus += " OR SalaryProcFlag ='SEPARATED'";
                    }
                    if (isMaternity == true)
                    {
                        wcEmpStatus += " OR SalaryProcFlag ='MLV_PRE'";

                    }
                }
            }

            wcEmpStatus += ")";

            string sql = @"SELECT * FROM (SELECT  DISTINCT isnull(e.SystemId,'') EmpSystemId   
                                    ,ISNULL(e.EmployeeCode,'') EmployeeCode
                                    ,ISNULL(e.EmployeeName,'') EmployeeName								
                                    ,isnull(ISNULL(egdsg.UserName,ld.UserName),'') Designation                                       
									,ISNULL(Department.UserName,'') Department 
									,ISNULL(Division.UserName,'') Division 
									,ISNULL(EmpC.UserName,'') EmployeeCategory
									,ISNULL(e.PaymentMode,'') PaymentMode
									,ISNULL(bb.UserName,'') BankName
                                    ,0 NetPayment
                                    , Case when Isnull(SPM.SalaryProcFlag,'') = '' THen 'Regular' else SalaryProcFlag end SalaryProcFlag
                                    ,ISNULL(s.BankAccNo,'') BankAccNo
                                    ,ISNULL(s.IFSCCode,'') IFSCCode
                                    ,FORMAT(ISNULL(sl.UpdatedDate,sl.AddedDate),'dd-MMM-yyyy') DisbursementDate
                                    --,FORMAT(VL.PostingDate,'dd-MMM-yyyy') DisbursementDate
                                    ,S.Id,VL.VoucherNo
                                    from SalaryProcessLogDetail s
                                    JOIN SalaryProcMaster SPM ON SPM.SystemID = s.SalaryProcessId and spm.MonthNo = Month('" + effectiveDate + @"') and spm.YearNo = Year('" + effectiveDate + @"')
                                    left join EmployeeInformation e on e.SystemId= s.EmpSystemId
                                    LEFT OUTER JOIN HKP.Designation egdsg on egdsg.id=s.DesignationId
                                    LEFT OUTER JOIN HKP.LegalDesignation  ld on ld.Id=s.LegalDesignationId
                                    LEFT OUTER JOIN (select dm.DesignationGroupId,dm.DesignationId,dm.EmployeeCategoryId
                                    ,dg.UserName GivenDesignationGroup
                                    FROM mst.DesignationMaster dm
                                    LEFT OUTER JOIN HKP.DesignationGroup dg on dg.Id=dm.DesignationGroupId
                                    ) egdsgg on egdsgg.DesignationId=e.GivenDesignationId
                                    AND egdsgg.EmployeeCategoryId=s.EmployeeCategoryId
                                    LEFT OUTER JOIN MST.ManpowerBudget mpb on mpb.Id=s.BudgetCode
                                    LEFT OUTER JOIN ORG.Position PO ON mpb.PositionId=PO.Id
                                    LEFT OUTER JOIN ORG.Entity EN ON mpb.EntityId=EN.Id
                                    LEFT JOIN [ORG].[Department] ON Department.Id = PO.DepartmentId
                                    LEFT JOIN [ORG].[Division] ON Division.Id = EN.DivisionId
                                    LEFT JOIN [ORG].[Plant] ON Plant.Id = EN.PlantId
                                    LEFT JOIN [ORG].[Section] ON Section.Id = PO.SectionId
                                    LEFT JOIN [ORG].[SubSection] ON SubSection.Id = PO.SubSectionId
                                    LEFT JOIN [ORG].[Unit] ON Unit.Id = EN.UnitId                                   
                                    LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = E.GivenDesignationId
                                    LEFT JOIN SCS.DesignationMasterConfiguration DMC ON DMC.DesignationMasterId=DesM.Id
									LEFT JOIN dbo.AccountsGroup AG ON AG.Id=DMC.AccountsGroupId
                                    LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId			                                       
                                    LEFT JOIN ORG.Line AS eL ON eL.Id= mpb.LineId
                                    Left outer join MST.PayrollGroupMaster PGM ON PGM.employeeid = E.SystemId
                                    Left outer join HKP.PayrollGroup PG ON PG.id = PGM.PayrollGroupId
                                    Left Join [dbo].[JobLocation] jl on jl.SystemID = E.JobLocationID
                                    left join [HKP].[Bank] bb on bb.Id = s.BankSystemID
                                    Left join SalaryLock sl on sl.EmpSystemId=e.SystemId and sl.YearNo=YEAR('" + effectiveDate + @"') AND SL.MonthNo=Month('" + effectiveDate + @"')
                                    LEFT JOIN TRN.Voucher  V ON V.Id=sl.PayableVoucherId 
                                    LEFT JOIN TRN.Voucher  Vl ON Vl.Id=sl.DisbursementVoucherId 
                                    WHERE  s.CompanyGroupId='" + identity.CompanyGroupId + "' AND s.PlantId='" + identity.PlantId + "' and sl.islocked=1 AND sl.PastDisbursed IS NULL AND EmpC.Id IN(" + employeeCategoryId+ ")  AND e.PaymentMode IN(" + pm + ")  " + wcPayrollGroup + @" 
                                    ) DD " + wcEmpStatus + @" ORDER BY EmployeeCode";
            var empdata = _sqlRepository.GetDataCollection(sql);

            var sql2 = @"select SPC.DisbusmentAmount NetPayment, SPC.EmpInfoSystemID from SalaryProcChild SPC
left join dbo.SalaryHead SH on SH.SalaryHeadID = SPC.SalaryHeadID
JOIN SalaryProcMaster SPM ON SPM.SystemID = SPC.SlrProcMstSystemID and spm.MonthNo = Month('" + effectiveDate + @"') and spm.YearNo = Year('" + effectiveDate + @"')
Where HeadCategory='Net Payable' ";

            var empNetPay = _sqlRepository.GetDataCollection(sql2);
            
            JsonResult json = Json(new { empdata, empNetPay }, JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;
            return json;
        }

        #region Salary UnDisbursed
        [HttpPost, Authorize]
        public ActionResult GetSalaryUnDisbursed(string effectiveDate, string salaryProcessId, bool isActive, bool isSeperated, bool isMaternity, string paymentMode)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            bool sa = identity.IsSysAdmin;
            bool ca = identity.IsControlAdmin;
            string userId = identity.UserId;
            string plantId = identity.PlantId;
            string companyGroupId = identity.CompanyGroupId;
            var wcPayrollGroup = "";
            string wcEmpStatus = " Where (1=0 ";

            if (sa == true || ca == true)
            {
                wcPayrollGroup = @"";
            }
            //else
            //{
            //    wcPayrollGroup = @"AND E.SystemId  IN (SELECT employeeid from MST.PayrollGroupMaster where PayrollGroupId IN (SELECT PayrollGroupId FROM SEC.UserPayrollGroup where UserId = '" + userId + @"'))";
            //}
            if (salaryProcessId == "STRUCTURE")
            {
                wcEmpStatus = " Where (1=1 ";
            }
            else
            {
                wcEmpStatus = " Where (1=0 ";

                if (isActive == true && isSeperated == true && isMaternity == true)
                {
                    wcEmpStatus = " Where (1=1 ";
                }
                else
                {
                    if (isActive == true)
                    {
                        wcEmpStatus += " OR SalaryProcFlag ='Regular'";
                    }
                    if (isSeperated == true)
                    {
                        wcEmpStatus += " OR SalaryProcFlag ='SEPARATED'";
                    }
                    if (isMaternity == true)
                    {
                        wcEmpStatus += " OR SalaryProcFlag ='MLV_PRE'";

                    }
                }
            }

            wcEmpStatus += ")";

            string sql = @"select [isSelect] = Convert(bit, 'True'),[isToBeSelect] = Convert(bit, 'False'),* FROM (  SELECT   dISTINCT   
                                     isnull(e.SystemId,'') EmpSystemId
									,ISNULL(e.EmployeeId,'')  EmployeeId 
	                                ,sl.Id,CheckBoxSelect=CONVERT(bit,0)   
									,SPM.MonthNo,SPM.YearNo ,sl.IsLocked AS Lock
                                    ,ISNULL(e.EmployeeCode,'') EmployeeCode
                                    ,ISNULL(e.EmployeeName,'') EmployeeName								
                                    ,ISNULL(mpb.EntityId,'') EntityId
									,ISNULL(mpb.PositionId,'') PositionId                                     
                                    ,isnull(ISNULL(egdsg.UserName,ld.UserName),'') Designation                                       
									,ISNULL(Department.UserName,'') Department 
									,ISNULL(Division.UserName,'') Division 
									,ISNULL(EmpC.UserName,'') EmployeeCategory
									,ISNULL(Plant.UserName,'') Plant 
									,ISNULL(Section.UserName,'') Section 
									,ISNULL(SubSection.UserName,'') SubSection 
									,ISNULL(Unit.UserName,'') Unit 
                                    ,ISNULL(eL.UserName,'') Line
                                    ,ISNULL(REPLACE(CONVERT(VARCHAR(11), e.DOJ, 106), ' ', '-'),'') DOJ
                                    ,ISNULL(REPLACE(CONVERT(VARCHAR(11), e.DOS, 106), ' ', '-'),'') DOS
                                    , CASE WHEN MONTH(DOS) =  MONTH('" + effectiveDate + @"')  AND YEAR(DOS) = YEAR('" + effectiveDate + @"') then 'Separated' else 'Active' end CurrentMonthEmployeeStatus
                                    ,ISNULL(e.EmployeeStatus,'') EmployeeStatus
                                    , Case when Isnull(SPM.SalaryProcFlag,'') = '' THen 'Regular' else SalaryProcFlag end SalaryProcFlag
									,ISNULL(PG.UserName,'') PayRollGroup
                                    ,e.EmployeeCodePreFix,e.EmployeeCodeNumeric
                                    ,ISNULL(jl.JobLocation, '') JobLocation
									,ISNULL(e.PaymentMode,'') PaymentMode
									,ISNULL(bb.UserName,'') BankName
                                    ,ISNULL(v.VoucherNo,'' ) VoucherNo
                                    ,ISNULL(sl.PayableVoucherId,'') PayableVoucherId
                                    ,ISNULL(sl.DisbursementVoucherId,'') DisbursementVoucherId
                                    ,ISNULL(v.VoucherNo,'') as PayableVoucherNo
                                    ,ISNULL(vl.VoucherNo,'') as DisbursementVoucherNo
                                    ,sl.IsDisbursed
                                    ,IsLock = case when sl.IsLocked = 1 then 'Locked' else 'Unlocked' end
                                    ,IsDisburse = case when sl.IsDisbursed = 1 then 'Disbursed' else 'Not Disbursed' end 
                                    ,SPCD.NetPayment
                                    ,SPM.SystemID SalaryProcId,SPM.AddedBy,AG.UserName AccountsGroup
                                    ,FORMAT(ISNULL(sl.UpdatedDate,sl.AddedDate),'dd-MMM-yyyy') DisbursementDate
                                    ,sl.DisbursementAdviceId,DA.Remarks
                                    from SalaryProcessLogDetail s
                                    JOIN SalaryProcMaster SPM ON SPM.SystemID = s.SalaryProcessId and spm.MonthNo = Month('" + effectiveDate + @"') and spm.YearNo = Year('" + effectiveDate + @"')
                                    left join EmployeeInformation e on e.SystemId= s.EmpSystemId
                                    INNER JOIN (select SPC.DisbusmentAmount NetPayment,SPC.EmpInfoSystemID from SalaryProcChild SPC
                                    left join dbo.SalaryHead SH on SH.SalaryHeadID = SPC.SalaryHeadID
                                    JOIN SalaryProcMaster SPM ON SPM.SystemID = SPC.SlrProcMstSystemID and spm.MonthNo = Month('" + effectiveDate + @"') and spm.YearNo = Year('" + effectiveDate + @"')
                                    Where HeadCategory='Net Payable' AND ISNULL(SPC.DisbusmentAmount,0)!=0)SPCD ON SPCD.EmpInfoSystemID=s.EmpSystemId
                                    LEFT OUTER JOIN HKP.Designation egdsg on egdsg.id=s.DesignationId
                                    LEFT OUTER JOIN HKP.LegalDesignation  ld on ld.Id=s.LegalDesignationId
                                    LEFT OUTER JOIN (select dm.DesignationGroupId,dm.DesignationId,dm.EmployeeCategoryId
                                    ,dg.UserName GivenDesignationGroup
                                    FROM mst.DesignationMaster dm
                                    LEFT OUTER JOIN HKP.DesignationGroup dg on dg.Id=dm.DesignationGroupId
                                    ) egdsgg on egdsgg.DesignationId=e.GivenDesignationId
                                    AND egdsgg.EmployeeCategoryId=s.EmployeeCategoryId
                                    LEFT OUTER JOIN MST.ManpowerBudget mpb on mpb.Id=s.BudgetCode
                                    LEFT OUTER JOIN ORG.Position PO ON mpb.PositionId=PO.Id
                                    LEFT OUTER JOIN ORG.Entity EN ON mpb.EntityId=EN.Id
                                    LEFT JOIN [ORG].[Department] ON Department.Id = PO.DepartmentId
                                    LEFT JOIN [ORG].[Division] ON Division.Id = EN.DivisionId
                                    LEFT JOIN [ORG].[Plant] ON Plant.Id = EN.PlantId
                                    LEFT JOIN [ORG].[Section] ON Section.Id = PO.SectionId
                                    LEFT JOIN [ORG].[SubSection] ON SubSection.Id = PO.SubSectionId
                                    LEFT JOIN [ORG].[Unit] ON Unit.Id = EN.UnitId                                   
                                    LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = E.GivenDesignationId
                                    LEFT JOIN SCS.DesignationMasterConfiguration DMC ON DMC.DesignationMasterId=DesM.Id
									LEFT JOIN dbo.AccountsGroup AG ON AG.Id=DMC.AccountsGroupId
                                    LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId			                                       
                                    LEFT JOIN ORG.Line AS eL ON eL.Id= mpb.LineId
                                    Left outer join MST.PayrollGroupMaster PGM ON PGM.employeeid = E.SystemId
                                    Left outer join HKP.PayrollGroup PG ON PG.id = PGM.PayrollGroupId
                                    Left Join [dbo].[JobLocation] jl on jl.SystemID = E.JobLocationID
                                    left join [HKP].[Bank] bb on bb.Id = s.BankSystemID
                                    Left join SalaryLock sl on sl.EmpSystemId=e.SystemId and sl.YearNo=YEAR('" + effectiveDate + @"') AND SL.MonthNo=Month('" + effectiveDate + @"')
                                    LEFT JOIN TRN.Voucher  V ON V.Id=sl.PayableVoucherId 
                                    LEFT JOIN TRN.Voucher  Vl ON Vl.Id=sl.DisbursementVoucherId 
                                    LEFT JOIN [dbo].[DisbursementAdvice]  DA ON DA.Id=sl.DisbursementAdviceId 
                                    WHERE  s.CompanyGroupId='" + identity.CompanyGroupId + "' AND s.PlantId='" + identity.PlantId + "' AND ISNULL(e.PaymentMode,'')='" + paymentMode + "' AND ISNULL(sl.PayableVoucherId,'')<>'' and sl.islocked=1 AND ISNULL(sl.IsDisbursed,0) = 0 AND V.IsPark=0 AND sl.PastDisbursed IS NULL  " + wcPayrollGroup + @" 
                                    ) DD " + wcEmpStatus + @" ORDER BY EmployeeCodePreFix,EmployeeCodeNumeric";

            JsonResult json = Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;
            return json;
        }

        [HttpPost, Authorize]
        public ActionResult GetSalaryUnDisbursedDateRange(string fromDate, string toDate, string salaryProcessId, bool isActive, bool isSeperated, bool isMaternity, string paymentMode)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            bool sa = identity.IsSysAdmin;
            bool ca = identity.IsControlAdmin;
            string userId = identity.UserId;
            string plantId = identity.PlantId;
            string companyGroupId = identity.CompanyGroupId;
            var wcPayrollGroup = "";
            string wcEmpStatus = " Where (1=0 ";

            if (sa == true || ca == true)
            {
                wcPayrollGroup = @"";
            }
            //else
            //{
            //    wcPayrollGroup = @"AND E.SystemId  IN (SELECT employeeid from MST.PayrollGroupMaster where PayrollGroupId IN (SELECT PayrollGroupId FROM SEC.UserPayrollGroup where UserId = '" + userId + @"'))";
            //}
            if (salaryProcessId == "STRUCTURE")
            {
                wcEmpStatus = " Where (1=1 ";
            }
            else
            {
                wcEmpStatus = " Where (1=0 ";

                if (isActive == true && isSeperated == true && isMaternity == true)
                {
                    wcEmpStatus = " Where (1=1 ";
                }
                else
                {
                    if (isActive == true)
                    {
                        wcEmpStatus += " OR SalaryProcFlag ='Regular'";
                    }
                    if (isSeperated == true)
                    {
                        wcEmpStatus += " OR SalaryProcFlag ='SEPARATED'";
                    }
                    if (isMaternity == true)
                    {
                        wcEmpStatus += " OR SalaryProcFlag ='MLV_PRE'";

                    }
                }
            }

            wcEmpStatus += ")";

            string sql = @"select [isSelect] = Convert(bit, 'True'),[isToBeSelect] = Convert(bit, 'False'),* FROM (  SELECT   dISTINCT   
                                     isnull(e.SystemId,'') EmpSystemId
									,ISNULL(e.EmployeeId,'')  EmployeeId 
	                                ,sl.Id,CheckBoxSelect=CONVERT(bit,0)   
									,SPM.MonthNo
                                    ,CASE WHEN SPM.MonthNo=1 THEN 'January'
								            WHEN SPM.MonthNo=2 THEN 'February'
								            WHEN SPM.MonthNo=3 THEN 'March'
								            WHEN SPM.MonthNo=4 THEN 'April'
								            WHEN SPM.MonthNo=5 THEN 'May'
								            WHEN SPM.MonthNo=6 THEN 'June'
								            WHEN SPM.MonthNo=7 THEN 'July'
								            WHEN SPM.MonthNo=8 THEN 'August'
								            WHEN SPM.MonthNo=9 THEN 'September'
								            WHEN SPM.MonthNo=10 THEN 'October'
								            WHEN SPM.MonthNo=11 THEN 'November'
								            WHEN SPM.MonthNo=12 THEN 'December'
								            ELSE '' END MonthName
                                    ,SPM.YearNo ,sl.IsLocked AS Lock
                                    ,ISNULL(e.EmployeeCode,'') EmployeeCode
                                    ,ISNULL(e.EmployeeName,'') EmployeeName								
                                    ,ISNULL(mpb.EntityId,'') EntityId
									,ISNULL(mpb.PositionId,'') PositionId                                     
                                    ,isnull(ISNULL(egdsg.UserName,ld.UserName),'') Designation                                       
									,ISNULL(Department.UserName,'') Department 
									,ISNULL(Division.UserName,'') Division 
									,ISNULL(EmpC.UserName,'') EmployeeCategory
									,ISNULL(Plant.UserName,'') Plant 
									,ISNULL(Section.UserName,'') Section 
									,ISNULL(SubSection.UserName,'') SubSection 
									,ISNULL(Unit.UserName,'') Unit 
                                    ,ISNULL(eL.UserName,'') Line
                                    ,ISNULL(REPLACE(CONVERT(VARCHAR(11), e.DOJ, 106), ' ', '-'),'') DOJ
                                    ,ISNULL(REPLACE(CONVERT(VARCHAR(11), e.DOS, 106), ' ', '-'),'') DOS
                                    ,CASE WHEN MONTH(DOS) =  SPM.MonthNo  AND YEAR(DOS) = SPM.YearNo then 'Separated' else 'Active' end CurrentMonthEmployeeStatus
                                    ,ISNULL(e.EmployeeStatus,'') EmployeeStatus
                                    , Case when Isnull(SPM.SalaryProcFlag,'') = '' THen 'Regular' else SalaryProcFlag end SalaryProcFlag
									,ISNULL(PG.UserName,'') PayRollGroup
                                    ,e.EmployeeCodePreFix,e.EmployeeCodeNumeric
                                    ,ISNULL(jl.JobLocation, '') JobLocation
									,ISNULL(e.PaymentMode,'') PaymentMode
									,ISNULL(bb.UserName,'') BankName
                                    ,ISNULL(v.VoucherNo,'' ) VoucherNo
                                    ,ISNULL(sl.PayableVoucherId,'') PayableVoucherId
                                    ,ISNULL(sl.DisbursementVoucherId,'') DisbursementVoucherId
                                    ,ISNULL(v.VoucherNo,'') as PayableVoucherNo
                                    ,ISNULL(vl.VoucherNo,'') as DisbursementVoucherNo
                                    ,sl.IsDisbursed
                                    ,IsLock = case when sl.IsLocked = 1 then 'Locked' else 'Unlocked' end
                                    ,IsDisburse = case when sl.IsDisbursed = 1 then 'Disbursed' else 'Not Disbursed' end 
                                    ,SPCD.NetPayment
                                    ,SPM.SystemID SalaryProcId,SPM.AddedBy,AG.UserName AccountsGroup
                                    ,FORMAT(ISNULL(sl.UpdatedDate,sl.AddedDate),'dd-MMM-yyyy') DisbursementDate
                                    ,sl.DisbursementAdviceId,DA.Remarks
                                    from SalaryProcessLogDetail s
                                    JOIN SalaryProcMaster SPM ON SPM.SystemID = s.SalaryProcessId 
                                    left join EmployeeInformation e on e.SystemId= s.EmpSystemId
                                    INNER JOIN (select SPC.DisbusmentAmount NetPayment,SPC.EmpInfoSystemID,spm.YearNo,spm.MonthNo,SPC.SalaryHeadID from SalaryProcChild SPC
                                        left join dbo.SalaryHead SH on SH.SalaryHeadID = SPC.SalaryHeadID
                                        JOIN SalaryProcMaster SPM ON SPM.SystemID = SPC.SlrProcMstSystemID 
                                        Where HeadCategory='Net Payable' AND ISNULL(SPC.DisbusmentAmount,0)!=0 
                                        AND CONCAT(spm.YearNo,RIGHT('00'+Isnull(Cast(spm.MonthNo AS VARCHAR(max)), ''),2)) 
									    BETWEEN  CONCAT(YEAR('" + fromDate + @"'),RIGHT('00'+Isnull(Cast(Month('" + fromDate + @"') AS VARCHAR(max)), ''),2))
									    AND CONCAT(YEAR('" + toDate + @"'),RIGHT('00'+Isnull(Cast(Month('" + toDate + @"') AS VARCHAR(max)), ''),2)) )SPCD ON SPCD.EmpInfoSystemID=s.EmpSystemId AND SPCD.YearNo=SPM.YearNo AND SPCD.MonthNo=SPM.MonthNo
                                    LEFT OUTER JOIN HKP.Designation egdsg on egdsg.id=s.DesignationId
                                    LEFT OUTER JOIN HKP.LegalDesignation  ld on ld.Id=s.LegalDesignationId
                                    LEFT OUTER JOIN (select dm.DesignationGroupId,dm.DesignationId,dm.EmployeeCategoryId
                                    ,dg.UserName GivenDesignationGroup
                                    FROM mst.DesignationMaster dm
                                    LEFT OUTER JOIN HKP.DesignationGroup dg on dg.Id=dm.DesignationGroupId
                                    ) egdsgg on egdsgg.DesignationId=e.GivenDesignationId
                                    AND egdsgg.EmployeeCategoryId=s.EmployeeCategoryId
                                    LEFT OUTER JOIN MST.ManpowerBudget mpb on mpb.Id=s.BudgetCode
                                    LEFT OUTER JOIN ORG.Position PO ON mpb.PositionId=PO.Id
                                    LEFT OUTER JOIN ORG.Entity EN ON mpb.EntityId=EN.Id
                                    LEFT JOIN [ORG].[Department] ON Department.Id = PO.DepartmentId
                                    LEFT JOIN [ORG].[Division] ON Division.Id = EN.DivisionId
                                    LEFT JOIN [ORG].[Plant] ON Plant.Id = EN.PlantId
                                    LEFT JOIN [ORG].[Section] ON Section.Id = PO.SectionId
                                    LEFT JOIN [ORG].[SubSection] ON SubSection.Id = PO.SubSectionId
                                    LEFT JOIN [ORG].[Unit] ON Unit.Id = EN.UnitId                                   
                                    LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = E.GivenDesignationId
                                    LEFT JOIN SCS.DesignationMasterConfiguration DMC ON DMC.DesignationMasterId=DesM.Id
									LEFT JOIN dbo.AccountsGroup AG ON AG.Id=DMC.AccountsGroupId
                                    LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId			                                       
                                    LEFT JOIN ORG.Line AS eL ON eL.Id= mpb.LineId
                                    Left outer join MST.PayrollGroupMaster PGM ON PGM.employeeid = E.SystemId
                                    Left outer join HKP.PayrollGroup PG ON PG.id = PGM.PayrollGroupId
                                    Left Join [dbo].[JobLocation] jl on jl.SystemID = E.JobLocationID
                                    left join [HKP].[Bank] bb on bb.Id = s.BankSystemID
                                    Left join SalaryLock sl on sl.EmpSystemId=e.SystemId AND sl.YearNo=SPM.YearNo AND sl.MonthNo=SPM.MonthNo
                                    LEFT JOIN TRN.Voucher  V ON V.Id=sl.PayableVoucherId 
                                    LEFT JOIN TRN.Voucher  Vl ON Vl.Id=sl.DisbursementVoucherId 
                                    LEFT JOIN [dbo].[DisbursementAdvice]  DA ON DA.Id=sl.DisbursementAdviceId 
                                    WHERE  s.CompanyGroupId='" + identity.CompanyGroupId + "' AND s.PlantId='" + identity.PlantId + "' AND ISNULL(e.PaymentMode,'')='" + paymentMode + "' AND ISNULL(sl.PayableVoucherId,'')<>'' and sl.islocked=1 AND ISNULL(sl.IsDisbursed,0) = 0 AND sl.PastDisbursed IS NULL  " + wcPayrollGroup + @" 
                                    AND CONCAT(sl.YearNo,RIGHT('00'+Isnull(Cast(SL.MonthNo AS VARCHAR(max)), ''),2)) 
									BETWEEN  CONCAT(YEAR('" + fromDate + @"'),RIGHT('00'+Isnull(Cast(Month('" + fromDate + @"') AS VARCHAR(max)), ''),2))
									AND CONCAT(YEAR('" + toDate + @"'),RIGHT('00'+Isnull(Cast(Month('" + toDate + @"') AS VARCHAR(max)), ''),2))
                                    ) DD " + wcEmpStatus + @" ORDER BY EmployeeCodePreFix,EmployeeCodeNumeric,YearNo,MonthNo";

            JsonResult json = Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;
            return json;
        }

        #region Report
        public void SalaryUndisbursedReportQry(string effectiveDate, string salaryProcessId, bool isActive, bool isSeperated, bool isMaternity, string paymentMode, out DataTable data)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            bool sa = identity.IsSysAdmin;
            bool ca = identity.IsControlAdmin;
            string userId = identity.UserId;
            string plantId = identity.PlantId;
            string companyGroupId = identity.CompanyGroupId;
            var wcPayrollGroup = "";
            string wcEmpStatus = " Where (1=0 ";

            if (sa == true || ca == true)
            {
                wcPayrollGroup = @"";
            }
            //else
            //{
            //    wcPayrollGroup = @"AND E.SystemId  IN (SELECT employeeid from MST.PayrollGroupMaster where PayrollGroupId IN (SELECT PayrollGroupId FROM SEC.UserPayrollGroup where UserId = '" + userId + @"'))";
            //}
            if (salaryProcessId == "STRUCTURE")
            {
                wcEmpStatus = " Where (1=1 ";
            }
            else
            {
                wcEmpStatus = " Where (1=0 ";

                if (isActive == true && isSeperated == true && isMaternity == true)
                {
                    wcEmpStatus = " Where (1=1 ";
                }
                else
                {
                    if (isActive == true)
                    {
                        wcEmpStatus += " OR SalaryProcFlag ='Regular'";
                    }
                    if (isSeperated == true)
                    {
                        wcEmpStatus += " OR SalaryProcFlag ='SEPARATED'";
                    }
                    if (isMaternity == true)
                    {
                        wcEmpStatus += " OR SalaryProcFlag ='MLV_PRE'";

                    }
                }
            }

            wcEmpStatus += ")";

            string sql = @"select [isSelect] = Convert(bit, 'True'),[isToBeSelect] = Convert(bit, 'False'),* FROM (  SELECT   dISTINCT   
                                     isnull(e.SystemId,'') EmpSystemId
									,ISNULL(e.EmployeeId,'')  EmployeeId 
	                                ,sl.Id,CheckBoxSelect=case when  sl.Id is null then  CONVERT(bit,0) when sl.IsDisbursed <> 1  then CONVERT(bit,0) else  CONVERT(bit,1) end   
									,SPM.MonthNo,SPM.YearNo ,sl.IsLocked AS Lock
                                    ,ISNULL(e.EmployeeCode,'') EmployeeCode
                                    ,ISNULL(e.EmployeeName,'') EmployeeName								
                                    ,ISNULL(mpb.EntityId,'') EntityId
									,ISNULL(mpb.PositionId,'') PositionId                                     
                                    ,isnull(ISNULL(egdsg.UserName,ld.UserName),'') Designation                                       
									,ISNULL(Department.UserName,'') Department 
									,ISNULL(Division.UserName,'') Division 
									,ISNULL(EmpC.UserName,'') EmployeeCategory
									,ISNULL(Plant.UserName,'') Plant 
									,ISNULL(Section.UserName,'') Section 
									,ISNULL(SubSection.UserName,'') SubSection 
									,ISNULL(Unit.UserName,'') Unit 
                                    ,ISNULL(eL.UserName,'') Line
                                    ,ISNULL(REPLACE(CONVERT(VARCHAR(11), e.DOJ, 106), ' ', '-'),'') DOJ
                                    ,ISNULL(REPLACE(CONVERT(VARCHAR(11), e.DOS, 106), ' ', '-'),'') DOS
                                    , CASE WHEN MONTH(DOS) =  MONTH('" + effectiveDate + @"')  AND YEAR(DOS) = YEAR('" + effectiveDate + @"') then 'Separated' else 'Active' end CurrentMonthEmployeeStatus
                                    ,ISNULL(e.EmployeeStatus,'') EmployeeStatus
                                    , Case when Isnull(SPM.SalaryProcFlag,'') = '' THen 'Regular' else SalaryProcFlag end SalaryProcFlag
									,ISNULL(PG.UserName,'') PayRollGroup
                                    ,e.EmployeeCodePreFix,e.EmployeeCodeNumeric
                                    ,ISNULL(jl.JobLocation, '') JobLocation
									,ISNULL(e.PaymentMode,'') PaymentMode
									,ISNULL(bb.UserName,'') BankName
                                    ,ISNULL(v.VoucherNo,'' ) VoucherNo
                                    ,ISNULL(sl.PayableVoucherId,'') PayableVoucherId
                                    ,ISNULL(sl.DisbursementVoucherId,'') DisbursementVoucherId
                                    ,ISNULL(v.VoucherNo,'') as PayableVoucherNo
                                    ,ISNULL(vl.VoucherNo,'') as DisbursementVoucherNo
                                    ,sl.IsDisbursed
                                    ,IsLock = case when sl.IsLocked = 1 then 'Locked' else 'Unlocked' end
                                    ,IsDisburse = case when sl.IsDisbursed = 1 then 'Disbursed' else 'Not Disbursed' end 
                                    ,CAST(SPCD.NetPayment AS DECIMAL(18,2))NetPayment
                                    from SalaryProcessLogDetail s
                                    JOIN SalaryProcMaster SPM ON SPM.SystemID = s.SalaryProcessId and spm.MonthNo = Month('" + effectiveDate + @"') and spm.YearNo = Year('" + effectiveDate + @"')
                                    left join EmployeeInformation e on e.SystemId= s.EmpSystemId
                                    INNER JOIN (select SPC.DisbusmentAmount NetPayment,SPC.EmpInfoSystemID from SalaryProcChild SPC
                                    left join dbo.SalaryHead SH on SH.SalaryHeadID = SPC.SalaryHeadID
                                    JOIN SalaryProcMaster SPM ON SPM.SystemID = SPC.SlrProcMstSystemID and spm.MonthNo = Month('" + effectiveDate + @"') and spm.YearNo = Year('" + effectiveDate + @"')
                                    Where HeadCategory='Net Payable' AND ISNULL(SPC.DisbusmentAmount,0)!=0)SPCD ON SPCD.EmpInfoSystemID=s.EmpSystemId
                                    LEFT OUTER JOIN HKP.Designation egdsg on egdsg.id=s.DesignationId
                                    LEFT OUTER JOIN HKP.LegalDesignation  ld on ld.Id=s.LegalDesignationId
                                    LEFT OUTER JOIN (select dm.DesignationGroupId,dm.DesignationId,dm.EmployeeCategoryId
                                    ,dg.UserName GivenDesignationGroup
                                    FROM mst.DesignationMaster dm
                                    LEFT OUTER JOIN HKP.DesignationGroup dg on dg.Id=dm.DesignationGroupId
                                    ) egdsgg on egdsgg.DesignationId=e.GivenDesignationId
                                    AND egdsgg.EmployeeCategoryId=s.EmployeeCategoryId
                                    LEFT OUTER JOIN MST.ManpowerBudget mpb on mpb.Id=s.BudgetCode
                                    LEFT OUTER JOIN ORG.Position PO ON mpb.PositionId=PO.Id
                                    LEFT OUTER JOIN ORG.Entity EN ON mpb.EntityId=EN.Id
                                    LEFT JOIN [ORG].[Department] ON Department.Id = PO.DepartmentId
                                    LEFT JOIN [ORG].[Division] ON Division.Id = EN.DivisionId
                                    LEFT JOIN [ORG].[Plant] ON Plant.Id = EN.PlantId
                                    LEFT JOIN [ORG].[Section] ON Section.Id = PO.SectionId
                                    LEFT JOIN [ORG].[SubSection] ON SubSection.Id = PO.SubSectionId
                                    LEFT JOIN [ORG].[Unit] ON Unit.Id = EN.UnitId                                   
                                    LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = E.GivenDesignationId
                                    LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId			                                       
                                    LEFT JOIN ORG.Line AS eL ON eL.Id= mpb.LineId
                                    Left outer join MST.PayrollGroupMaster PGM ON PGM.employeeid = E.SystemId
                                    Left outer join HKP.PayrollGroup PG ON PG.id = PGM.PayrollGroupId
                                    Left Join [dbo].[JobLocation] jl on jl.SystemID = E.JobLocationID
                                    left join [HKP].[Bank] bb on bb.Id = s.BankSystemID
                                    Left join SalaryLock sl on sl.EmpSystemId=e.SystemId and sl.YearNo=YEAR('" + effectiveDate + @"') AND SL.MonthNo=Month('" + effectiveDate + @"')
                                    LEFT JOIN TRN.Voucher  V ON V.Id=sl.PayableVoucherId 
                                    LEFT JOIN TRN.Voucher  Vl ON Vl.Id=sl.DisbursementVoucherId 
                                    WHERE  s.CompanyGroupId='" + identity.CompanyGroupId + "' AND s.PlantId='" + identity.PlantId + "' AND ISNULL(e.PaymentMode,'')='" + paymentMode + "' and sl.islocked=1 and ISNULL(sl.IsDisbursed,0) = 0 AND sl.PastDisbursed IS NULL  " + wcPayrollGroup + @" 
                                    ) DD " + wcEmpStatus + @" ORDER BY EmployeeCodePreFix,EmployeeCodeNumeric";
            data = _sqlRepository.GetDataTable(sql);
        }
        public void SalaryDisbursementVoucherWiseQry(string voucherId, out DataTable data)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select isSelected = Convert(bit, 'True'),sl.EmpSystemId,sl.YearNo,sl.MonthNo,ei.EmployeeCode,ei.EmployeeName,d.UserName Designation,spd.PaymentMode,spd.BankAccNo,spd.IFSCCode
                        ,DirectManpowerCost=case when po.DirectManpowerCost=0 then 'No' when po.DirectManpowerCost=1 then 'Yes' end ,b.UserName BankName,v.VoucherNo PayableVoucherNo
                        ,spc.DisbusmentAmount Amount,spd.Id
						,Department.UserName Department,Department.Id DepartmentId
						,EmpC.UserName EmployeeCategory, EmpC.Id EmpCategoryId
						,Section.UserName Section,Section.Id SectionId
						,SubSection.UserName SubSection,SubSection.Id SubSectionId
						,isnull(L.Id,'') LineId,isnull(L.UserName,'') Line
						,IsLock = case when sl.IsLocked = 1 then 'Locked' else 'Unlocked' end
						,ISNULL(vl.VoucherNo,'') as DisbursementVoucherNo,ISNULL(sl.DisbursementVoucherId,'') DisbursementVoucherId
						,IsDisburse = case when sl.IsDisbursed = 1 then 'Disbursed' else 'Not Disbursed' end
						,ISNULL(REPLACE(CONVERT(VARCHAR(11), ei.DOJ, 106), ' ', '-'),'') DOJ
						,ISNULL(PG.UserName,'') PayRollGroup
						,ISNULL(jl.JobLocation, '') JobLocation
                        ,ISNULL(REPLACE(CONVERT(VARCHAR(11), ei.DOS, 106), ' ', '-'),'') DOS
						,Case when Isnull(SPM.SalaryProcFlag,'') = '' THen 'Regular' else SalaryProcFlag end SalaryProcFlag
                        ,ISNULL(Division.UserName,'') Division ,ISNULL(Division.Id,'') DivisionId
                        ,sl.DisbursementAdviceId,DA.Remarks,SPM.SystemID SalaryProcId,SPM.AddedBy
                        ,CASE WHEN sl.MonthNo=1 THEN 'January'
			                    WHEN sl.MonthNo=2 THEN 'February'
			                    WHEN sl.MonthNo=3 THEN 'March'
			                    WHEN sl.MonthNo=4 THEN 'April'
			                    WHEN sl.MonthNo=5 THEN 'May'
			                    WHEN sl.MonthNo=6 THEN 'June'
			                    WHEN sl.MonthNo=7 THEN 'July'
			                    WHEN sl.MonthNo=8 THEN 'August'
			                    WHEN sl.MonthNo=9 THEN 'September'
			                    WHEN sl.MonthNo=10 THEN 'October'
			                    WHEN sl.MonthNo=11 THEN 'November'
			                    WHEN sl.MonthNo=12 THEN 'December'
			                    ELSE '' END MonthName
                        ,ISNULL(REPLACE(CONVERT(VARCHAR(11), DA.AddedDate, 106), ' ', '-'),'') AdviceDate
                        ,CASE WHEN MONTH(DOS) =  sl.MonthNo  AND YEAR(DOS) = sl.YearNo then 'Separated' else 'Active' end CurrentMonthEmployeeStatus
						,ISNULL(ei.EmployeeStatus,'') EmployeeStatus
                        from [dbo].[SalaryLock] sl 
                        left join dbo.SalaryProcMaster spm on   spm.MonthNo=sl.MonthNo and spm.YearNo=sl.YearNo
                        left join dbo.SalaryProcChild spc on spc.SlrProcMstSystemID=spm.SystemID and sl.EmpSystemId=spc.EmpInfoSystemID
						left join dbo.SalaryProcessLogDetail spd on   spd.EmpSystemId=sl.EmpSystemId and spm.SystemID=spd.SalaryProcessId
                        left join dbo.EmployeeInformation ei on ei.SystemId=sl.EmpSystemId
						left join MST.ManpowerBudget MPB on MPB.Id=ei.BudgetCode
						left join ORG.Position PO on PO.Id=MPB.PositionId
                        LEFT OUTER JOIN ORG.Entity EN ON MPB.EntityId=EN.Id
						LEFT JOIN [ORG].[Department] ON Department.Id = PO.DepartmentId
						LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = ei.GivenDesignationId
                        LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId
						LEFT JOIN [ORG].[Section] ON Section.Id = PO.SectionId
                        LEFT JOIN [ORG].[SubSection] ON SubSection.Id = PO.SubSectionId
						LEFT JOIN ORG.Line AS L ON L.Id= MPB.LineId
                        LEFT JOIN [ORG].[Division] ON Division.Id = EN.DivisionId
						left join dbo.SalaryHead sh on sh.SalaryHeadID=spc.SalaryHeadID
						left join hkp.Designation d on d.Id=spd.DesignationId
						Left outer join MST.PayrollGroupMaster PGM ON PGM.employeeid = ei.SystemId
						Left outer join HKP.PayrollGroup PG ON PG.id = PGM.PayrollGroupId
						Left Join [dbo].[JobLocation] jl on jl.SystemID = ei.JobLocationID
						left join hkp.Bank b on spd.BankSystemID=b.Id
						left join trn.Voucher v on v.Id=sl.PayableVoucherId
                        LEFT JOIN TRN.Voucher  Vl ON Vl.Id=sl.DisbursementVoucherId 
                        LEFT JOIN [dbo].[DisbursementAdvice]  DA ON DA.Id=sl.DisbursementAdviceId
                        where sl.PayableVoucherId<>'' AND sl.DisbursementVoucherId IS NOT NULL and sl.IsDisbursed=1 AND sl.PastDisbursed IS NULL
                        and sl.DisbursementVoucherId='" + voucherId + @"'
                         and spc.DisbusmentAmount!=0  
                        and spd.PlantId='" + identity.PlantId + @"' 
						 and ISNULL(sh.SalaryHead, '')  in ('Net Pay') ";
            data = _sqlRepository.GetDataTable(sql);
        }
        public void BonusDisbursementVoucherWiseQry(string voucherId, out DataTable data)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select isSelected = Convert(bit, 'True'),sl.EmpSystemId,sl.YearNo,sl.MonthNo,ei.EmployeeCode,ei.EmployeeName,d.UserName Designation,spd.PaymentMode,spd.BankAccNo,spd.IFSCCode
                        ,DirectManpowerCost=case when po.DirectManpowerCost=0 then 'No' when po.DirectManpowerCost=1 then 'Yes' end ,b.UserName BankName,v.VoucherNo PayableVoucherNo
                        ,spc.DisbusmentAmount Amount,spd.Id
						,Department.UserName Department,Department.Id DepartmentId
						,EmpC.UserName EmployeeCategory, EmpC.Id EmpCategoryId
						,Section.UserName Section,Section.Id SectionId
						,SubSection.UserName SubSection,SubSection.Id SubSectionId
						,isnull(L.Id,'') LineId,isnull(L.UserName,'') Line
						,IsLock = case when sl.IsLocked = 1 then 'Locked' else 'Unlocked' end
						,ISNULL(vl.VoucherNo,'') as DisbursementVoucherNo,ISNULL(sl.DisbursementVoucherId,'') DisbursementVoucherId
						,IsDisburse = case when sl.IsDisbursed = 1 then 'Disbursed' else 'Not Disbursed' end
						,ISNULL(REPLACE(CONVERT(VARCHAR(11), ei.DOJ, 106), ' ', '-'),'') DOJ
						,ISNULL(PG.UserName,'') PayRollGroup
						,ISNULL(jl.JobLocation, '') JobLocation
                        ,ISNULL(REPLACE(CONVERT(VARCHAR(11), ei.DOS, 106), ' ', '-'),'') DOS
						,Case when Isnull(SPM.SalaryProcFlag,'') = '' THen 'Regular' else SalaryProcFlag end SalaryProcFlag
                        ,ISNULL(Division.UserName,'') Division ,ISNULL(Division.Id,'') DivisionId
                        ,sl.BonusDisbursementAdviceId,DA.Remarks,SPM.SystemID SalaryProcId,SPM.AddedBy
                        ,CASE WHEN sl.MonthNo=1 THEN 'January'
			                    WHEN sl.MonthNo=2 THEN 'February'
			                    WHEN sl.MonthNo=3 THEN 'March'
			                    WHEN sl.MonthNo=4 THEN 'April'
			                    WHEN sl.MonthNo=5 THEN 'May'
			                    WHEN sl.MonthNo=6 THEN 'June'
			                    WHEN sl.MonthNo=7 THEN 'July'
			                    WHEN sl.MonthNo=8 THEN 'August'
			                    WHEN sl.MonthNo=9 THEN 'September'
			                    WHEN sl.MonthNo=10 THEN 'October'
			                    WHEN sl.MonthNo=11 THEN 'November'
			                    WHEN sl.MonthNo=12 THEN 'December'
			                    ELSE '' END MonthName
                        ,ISNULL(REPLACE(CONVERT(VARCHAR(11), DA.AddedDate, 106), ' ', '-'),'') AdviceDate
                        ,CASE WHEN MONTH(DOS) =  sl.MonthNo  AND YEAR(DOS) = sl.YearNo then 'Separated' else 'Active' end CurrentMonthEmployeeStatus
						,ISNULL(ei.EmployeeStatus,'') EmployeeStatus
                        from [dbo].[SalaryLock] sl 
                        left join dbo.SalaryProcMaster spm on   spm.MonthNo=sl.MonthNo and spm.YearNo=sl.YearNo
                        left join dbo.SalaryProcChild spc on spc.SlrProcMstSystemID=spm.SystemID and sl.EmpSystemId=spc.EmpInfoSystemID
						left join dbo.SalaryProcessLogDetail spd on   spd.EmpSystemId=sl.EmpSystemId and spm.SystemID=spd.SalaryProcessId
                        left join dbo.EmployeeInformation ei on ei.SystemId=sl.EmpSystemId
						left join MST.ManpowerBudget MPB on MPB.Id=ei.BudgetCode
						left join ORG.Position PO on PO.Id=MPB.PositionId
                        LEFT OUTER JOIN ORG.Entity EN ON MPB.EntityId=EN.Id
						LEFT JOIN [ORG].[Department] ON Department.Id = PO.DepartmentId
						LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = ei.GivenDesignationId
                        LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId
						LEFT JOIN [ORG].[Section] ON Section.Id = PO.SectionId
                        LEFT JOIN [ORG].[SubSection] ON SubSection.Id = PO.SubSectionId
						LEFT JOIN ORG.Line AS L ON L.Id= MPB.LineId
                        LEFT JOIN [ORG].[Division] ON Division.Id = EN.DivisionId
						left join dbo.SalaryHead sh on sh.SalaryHeadID=spc.SalaryHeadID
						left join hkp.Designation d on d.Id=spd.DesignationId
						Left outer join MST.PayrollGroupMaster PGM ON PGM.employeeid = ei.SystemId
						Left outer join HKP.PayrollGroup PG ON PG.id = PGM.PayrollGroupId
						Left Join [dbo].[JobLocation] jl on jl.SystemID = ei.JobLocationID
						left join hkp.Bank b on spd.BankSystemID=b.Id
						left join trn.Voucher v on v.Id=sl.PayableVoucherId
                        LEFT JOIN TRN.Voucher  Vl ON Vl.Id=sl.BonusDisbursementVoucherId 
                        LEFT JOIN [dbo].[BonusDisbursementAdvice]  DA ON DA.Id=sl.BonusDisbursementAdviceId
                        where sl.PayableVoucherId<>'' AND sl.BonusDisbursementVoucherId IS NOT NULL and sl.IsBonusDisbursed=1 
                        and sl.BonusDisbursementVoucherId='" + voucherId + @"'
                         and spc.DisbusmentAmount!=0  
                        and spd.PlantId='" + identity.PlantId + @"' 
						and ISNULL(SH.HeadCategory, '')  in ('Monthly Bonus Retain','Annual Bonus Retain') ";
            data = _sqlRepository.GetDataTable(sql);
        }
        public void BonusDisbursementSummaryQry(string fromDate, string toDate, string salaryProcessId, bool isActive, bool isSeperated, bool isMaternity, string paymentMode, out DataTable data)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            bool sa = identity.IsSysAdmin;
            bool ca = identity.IsControlAdmin;
            string userId = identity.UserId;
            string plantId = identity.PlantId;
            string companyGroupId = identity.CompanyGroupId;
            var wcPayrollGroup = "";
            string wcEmpStatus = " Where (1=0 ";

            if (sa == true || ca == true)
            {
                wcPayrollGroup = @"";
            }
            if (salaryProcessId == "STRUCTURE")
            {
                wcEmpStatus = " Where (1=1 ";
            }
            else
            {
                wcEmpStatus = " Where (1=0 ";

                if (isActive == true && isSeperated == true && isMaternity == true)
                {
                    wcEmpStatus = " Where (1=1 ";
                }
                else
                {
                    if (isActive == true)
                    {
                        wcEmpStatus += " OR SalaryProcFlag ='Regular'";
                    }
                    if (isSeperated == true)
                    {
                        wcEmpStatus += " OR SalaryProcFlag ='SEPARATED'";
                    }
                    if (isMaternity == true)
                    {
                        wcEmpStatus += " OR SalaryProcFlag ='MLV_PRE'";

                    }
                }
            }

            wcEmpStatus += ")";

            string sql = @"SELECT EmpSystemId,EmployeeId,EmployeeCode,EmployeeName,EmployeeCategory,DOJ,DOS,CurrentMonthEmployeeStatus
                           ,EmployeeStatus,PaymentMode,SUM(NetPayment)Amount
                           FROM (  SELECT   dISTINCT     
                                     isnull(e.SystemId,'') EmpSystemId
									,ISNULL(e.EmployeeId,'')  EmployeeId 
	                                ,sl.Id,CheckBoxSelect= CONVERT(bit,0)   
									,SPM.MonthNo
									,CASE WHEN SPM.MonthNo=1 THEN 'January'
									        WHEN SPM.MonthNo=2 THEN 'February'
									        WHEN SPM.MonthNo=3 THEN 'March'
									        WHEN SPM.MonthNo=4 THEN 'April'
									        WHEN SPM.MonthNo=5 THEN 'May'
									        WHEN SPM.MonthNo=6 THEN 'June'
									        WHEN SPM.MonthNo=7 THEN 'July'
									        WHEN SPM.MonthNo=8 THEN 'August'
									        WHEN SPM.MonthNo=9 THEN 'September'
									        WHEN SPM.MonthNo=10 THEN 'October'
									        WHEN SPM.MonthNo=11 THEN 'November'
									        WHEN SPM.MonthNo=12 THEN 'December'
									        ELSE '' END MonthName
									,SPM.YearNo ,sl.IsLocked AS Lock,ISNULL(e.EmployeeCode,'') EmployeeCode ,ISNULL(e.EmployeeName,'') EmployeeName								
                                    ,ISNULL(mpb.EntityId,'') EntityId,ISNULL(mpb.PositionId,'') PositionId ,isnull(ISNULL(egdsg.UserName,ld.UserName),'') Designation                                       
									,ISNULL(Department.UserName,'') Department ,ISNULL(Division.UserName,'') Division ,ISNULL(EmpC.UserName,'') EmployeeCategory
									,ISNULL(Plant.UserName,'') Plant ,ISNULL(Section.UserName,'') Section ,ISNULL(SubSection.UserName,'') SubSection 
									,ISNULL(Unit.UserName,'') Unit  ,ISNULL(eL.UserName,'') Line,ISNULL(REPLACE(CONVERT(VARCHAR(11), e.DOJ, 106), ' ', '-'),'') DOJ
                                    ,ISNULL(REPLACE(CONVERT(VARCHAR(11), e.DOS, 106), ' ', '-'),'') DOS
                                    ,CASE WHEN MONTH(DOS) =  SPM.MonthNo  AND YEAR(DOS) = SPM.YearNo then 'Separated' else 'Active' end CurrentMonthEmployeeStatus
                                    ,ISNULL(e.EmployeeStatus,'') EmployeeStatus
                                    , Case when Isnull(SPM.SalaryProcFlag,'') = '' THen 'Regular' else SalaryProcFlag end SalaryProcFlag
									,ISNULL(PG.UserName,'') PayRollGroup
                                    ,e.EmployeeCodePreFix,e.EmployeeCodeNumeric
                                    ,ISNULL(jl.JobLocation, '') JobLocation
									,ISNULL(e.PaymentMode,'') PaymentMode
									,ISNULL(bb.UserName,'') BankName
                                    ,ISNULL(v.VoucherNo,'' ) VoucherNo
                                    ,ISNULL(sl.PayableVoucherId,'') PayableVoucherId
                                    ,ISNULL(sl.BonusDisbursementVoucherId,'') DisbursementVoucherId
                                    ,ISNULL(v.VoucherNo,'') as PayableVoucherNo
                                    ,ISNULL(vl.VoucherNo,'') as DisbursementVoucherNo
                                    ,sl.IsBonusDisbursed
                                    ,IsLock = case when sl.IsLocked = 1 then 'Locked' else 'Unlocked' end
                                    ,IsDisburse = case when sl.IsBonusDisbursed = 1 then 'Disbursed' else 'Not Disbursed' end 
                                    ,SPCD.NetPayment,SPM.SystemID SalaryProcId,SPM.AddedBy,AG.UserName AccountsGroup
                                    ,FORMAT(DA.AddedDate,'dd-MMM-yyyy') DisbursementDate
                                    ,isnull(sl.BonusDisbursementAdviceId,'')BonusDisbursementAdviceId,isnull(DA.Remarks,'')Remarks,s.IFSCCode,s.BankAccNo
                                    from SalaryProcessLogDetail s
                                    JOIN SalaryProcMaster SPM ON SPM.SystemID = s.SalaryProcessId 
                                    INNER JOIN EmployeeInformation e on e.SystemId= s.EmpSystemId
                                    INNER JOIN (select SPC.DisbusmentAmount NetPayment,SPC.EmpInfoSystemID,spm.YearNo,spm.MonthNo from SalaryProcChild SPC
                                                left join dbo.SalaryHead SH on SH.SalaryHeadID = SPC.SalaryHeadID
                                                JOIN SalaryProcMaster SPM ON SPM.SystemID = SPC.SlrProcMstSystemID 
                                                Where HeadCategory IN('Annual Bonus Retain') AND ISNULL(SPC.DisbusmentAmount,0)!=0
									            AND CONCAT(spm.YearNo,RIGHT('00'+Isnull(Cast(spm.MonthNo AS VARCHAR(max)), ''),2)) 
									            BETWEEN  CONCAT(YEAR('" + fromDate + @"'),RIGHT('00'+Isnull(Cast(Month('" + fromDate + @"') AS VARCHAR(max)), ''),2))
									            AND CONCAT(YEAR('" + toDate + @"'),RIGHT('00'+Isnull(Cast(Month('" + toDate + @"') AS VARCHAR(max)), ''),2)) )SPCD ON SPCD.EmpInfoSystemID=s.EmpSystemId AND SPCD.YearNo=SPM.YearNo AND SPCD.MonthNo=SPM.MonthNo
                                    LEFT OUTER JOIN HKP.Designation egdsg on egdsg.id=s.DesignationId
                                    LEFT OUTER JOIN HKP.LegalDesignation  ld on ld.Id=s.LegalDesignationId
                                    LEFT OUTER JOIN (select dm.DesignationGroupId,dm.DesignationId,dm.EmployeeCategoryId
                                                    ,dg.UserName GivenDesignationGroup
                                                    FROM mst.DesignationMaster dm
                                                    LEFT OUTER JOIN HKP.DesignationGroup dg on dg.Id=dm.DesignationGroupId
                                                    ) egdsgg on egdsgg.DesignationId=e.GivenDesignationId AND egdsgg.EmployeeCategoryId=s.EmployeeCategoryId
                                    LEFT OUTER JOIN MST.ManpowerBudget mpb on mpb.Id=s.BudgetCode
                                    LEFT OUTER JOIN ORG.Position PO ON mpb.PositionId=PO.Id
                                    LEFT OUTER JOIN ORG.Entity EN ON mpb.EntityId=EN.Id
                                    LEFT JOIN [ORG].[Department] ON Department.Id = PO.DepartmentId
                                    LEFT JOIN [ORG].[Division] ON Division.Id = EN.DivisionId
                                    LEFT JOIN [ORG].[Plant] ON Plant.Id = EN.PlantId
                                    LEFT JOIN [ORG].[Section] ON Section.Id = PO.SectionId
                                    LEFT JOIN [ORG].[SubSection] ON SubSection.Id = PO.SubSectionId
                                    LEFT JOIN [ORG].[Unit] ON Unit.Id = EN.UnitId                                   
                                    LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = E.GivenDesignationId
                                    LEFT JOIN SCS.DesignationMasterConfiguration DMC ON DMC.DesignationMasterId=DesM.Id
									LEFT JOIN dbo.AccountsGroup AG ON AG.Id=DMC.AccountsGroupId
                                    LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId			                                       
                                    LEFT JOIN ORG.Line AS eL ON eL.Id= mpb.LineId
                                    Left outer join MST.PayrollGroupMaster PGM ON PGM.employeeid = E.SystemId
                                    Left outer join HKP.PayrollGroup PG ON PG.id = PGM.PayrollGroupId
                                    Left Join [dbo].[JobLocation] jl on jl.SystemID = E.JobLocationID
                                    left join [HKP].[Bank] bb on bb.Id = s.BankSystemID
                                    Left join SalaryLock sl on sl.EmpSystemId=e.SystemId AND sl.YearNo=SPM.YearNo AND sl.MonthNo=SPM.MonthNo
                                    LEFT JOIN TRN.Voucher  V ON V.Id=sl.PayableVoucherId 
                                    LEFT JOIN TRN.Voucher  Vl ON Vl.Id=sl.BonusDisbursementVoucherId 
                                    LEFT JOIN [dbo].[BonusDisbursementAdvice]  DA ON DA.Id=sl.BonusDisbursementAdviceId 
                                    WHERE  s.CompanyGroupId='" + identity.CompanyGroupId + "' AND s.PlantId='" + identity.PlantId + "' AND ISNULL(e.PaymentMode,'')='" + paymentMode + "' AND ISNULL(sl.PayableVoucherId,'')<>'' and sl.islocked=1 AND sl.IsBonusDisbursed = 1  " + wcPayrollGroup + @" 
                                    AND CONCAT(sl.YearNo,RIGHT('00'+Isnull(Cast(SL.MonthNo AS VARCHAR(max)), ''),2)) 
									BETWEEN  CONCAT(YEAR('" + fromDate + @"'),RIGHT('00'+Isnull(Cast(Month('" + fromDate + @"') AS VARCHAR(max)), ''),2))
									AND CONCAT(YEAR('" + toDate + @"'),RIGHT('00'+Isnull(Cast(Month('" + toDate + @"') AS VARCHAR(max)), ''),2))
                                    ) DD " + wcEmpStatus + @" GROUP BY EmpSystemId,EmployeeId,EmployeeCode,EmployeeName,EmployeeCategory
									,DOJ,DOS,CurrentMonthEmployeeStatus,EmployeeStatus,PaymentMode";
            data = _sqlRepository.GetDataTable(sql);
        }
        public void BonusUnDisbursementSummaryQry(string fromDate, string toDate, string salaryProcessId, bool isActive, bool isSeperated, bool isMaternity, string paymentMode, out DataTable data)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            bool sa = identity.IsSysAdmin;
            bool ca = identity.IsControlAdmin;
            string userId = identity.UserId;
            string plantId = identity.PlantId;
            string companyGroupId = identity.CompanyGroupId;
            var wcPayrollGroup = "";
            string wcEmpStatus = " Where (1=0 ";

            if (sa == true || ca == true)
            {
                wcPayrollGroup = @"";
            }
            if (salaryProcessId == "STRUCTURE")
            {
                wcEmpStatus = " Where (1=1 ";
            }
            else
            {
                wcEmpStatus = " Where (1=0 ";

                if (isActive == true && isSeperated == true && isMaternity == true)
                {
                    wcEmpStatus = " Where (1=1 ";
                }
                else
                {
                    if (isActive == true)
                    {
                        wcEmpStatus += " OR SalaryProcFlag ='Regular'";
                    }
                    if (isSeperated == true)
                    {
                        wcEmpStatus += " OR SalaryProcFlag ='SEPARATED'";
                    }
                    if (isMaternity == true)
                    {
                        wcEmpStatus += " OR SalaryProcFlag ='MLV_PRE'";

                    }
                }
            }

            wcEmpStatus += ")";

            string sql = @"SELECT EmpSystemId,EmployeeId,EmployeeCode,EmployeeName,EmployeeCategory,DOJ,DOS--,CurrentMonthEmployeeStatus
                           ,EmployeeStatus,PaymentMode,SUM(NetPayment)Amount,SUM(TotalPayDay)TotalPayDay
                           FROM (  SELECT   dISTINCT     
                                     isnull(e.SystemId,'') EmpSystemId
									,ISNULL(e.EmployeeId,'')  EmployeeId 
	                                ,sl.Id,CheckBoxSelect= CONVERT(bit,0)   
									,SPM.MonthNo
									,CASE WHEN SPM.MonthNo=1 THEN 'January'
									        WHEN SPM.MonthNo=2 THEN 'February'
									        WHEN SPM.MonthNo=3 THEN 'March'
									        WHEN SPM.MonthNo=4 THEN 'April'
									        WHEN SPM.MonthNo=5 THEN 'May'
									        WHEN SPM.MonthNo=6 THEN 'June'
									        WHEN SPM.MonthNo=7 THEN 'July'
									        WHEN SPM.MonthNo=8 THEN 'August'
									        WHEN SPM.MonthNo=9 THEN 'September'
									        WHEN SPM.MonthNo=10 THEN 'October'
									        WHEN SPM.MonthNo=11 THEN 'November'
									        WHEN SPM.MonthNo=12 THEN 'December'
									        ELSE '' END MonthName
									,SPM.YearNo ,sl.IsLocked AS Lock,ISNULL(e.EmployeeCode,'') EmployeeCode ,ISNULL(e.EmployeeName,'') EmployeeName								
                                    ,ISNULL(mpb.EntityId,'') EntityId,ISNULL(mpb.PositionId,'') PositionId ,isnull(ISNULL(egdsg.UserName,ld.UserName),'') Designation                                       
									,ISNULL(Department.UserName,'') Department ,ISNULL(Division.UserName,'') Division ,ISNULL(EmpC.UserName,'') EmployeeCategory
									,ISNULL(Plant.UserName,'') Plant ,ISNULL(Section.UserName,'') Section ,ISNULL(SubSection.UserName,'') SubSection 
									,ISNULL(Unit.UserName,'') Unit  ,ISNULL(eL.UserName,'') Line,ISNULL(REPLACE(CONVERT(VARCHAR(11), e.DOJ, 106), ' ', '-'),'') DOJ
                                    ,ISNULL(REPLACE(CONVERT(VARCHAR(11), e.DOS, 106), ' ', '-'),'') DOS
                                    ,CASE WHEN MONTH(DOS) =  SPM.MonthNo  AND YEAR(DOS) = SPM.YearNo then 'Separated' else 'Active' end CurrentMonthEmployeeStatus
                                    ,ISNULL(e.EmployeeStatus,'') EmployeeStatus
                                    , Case when Isnull(SPM.SalaryProcFlag,'') = '' THen 'Regular' else SalaryProcFlag end SalaryProcFlag
									,ISNULL(PG.UserName,'') PayRollGroup
                                    ,e.EmployeeCodePreFix,e.EmployeeCodeNumeric
                                    ,ISNULL(jl.JobLocation, '') JobLocation
									,ISNULL(e.PaymentMode,'') PaymentMode
									,ISNULL(bb.UserName,'') BankName
                                    ,ISNULL(v.VoucherNo,'' ) VoucherNo
                                    ,ISNULL(sl.PayableVoucherId,'') PayableVoucherId
                                    ,ISNULL(sl.BonusDisbursementVoucherId,'') DisbursementVoucherId
                                    ,ISNULL(v.VoucherNo,'') as PayableVoucherNo
                                    ,ISNULL(vl.VoucherNo,'') as DisbursementVoucherNo
                                    ,sl.IsBonusDisbursed
                                    ,IsLock = case when sl.IsLocked = 1 then 'Locked' else 'Unlocked' end
                                    ,IsDisburse = case when sl.IsBonusDisbursed = 1 then 'Disbursed' else 'Not Disbursed' end 
                                    ,SPCD.NetPayment,PD.TotalPayDay ,SPM.SystemID SalaryProcId,SPM.AddedBy,AG.UserName AccountsGroup
                                    ,FORMAT(DA.AddedDate,'dd-MMM-yyyy') DisbursementDate
                                    ,isnull(sl.BonusDisbursementAdviceId,'')BonusDisbursementAdviceId,isnull(DA.Remarks,'')Remarks,s.IFSCCode,s.BankAccNo
                                    from SalaryProcessLogDetail s
                                    JOIN SalaryProcMaster SPM ON SPM.SystemID = s.SalaryProcessId 
                                    INNER JOIN EmployeeInformation e on e.SystemId= s.EmpSystemId
                                    INNER JOIN (select SPC.DisbusmentAmount NetPayment,SPC.EmpInfoSystemID,spm.YearNo,spm.MonthNo from SalaryProcChild SPC
                                                left join dbo.SalaryHead SH on SH.SalaryHeadID = SPC.SalaryHeadID
                                                JOIN SalaryProcMaster SPM ON SPM.SystemID = SPC.SlrProcMstSystemID 
--                                                Where HeadCategory IN('Monthly Bonus Retain','Annual Bonus Retain') AND ISNULL(SPC.DisbusmentAmount,0)!=0
                                                Where HeadCategory IN('Annual Bonus Retain') AND ISNULL(SPC.DisbusmentAmount,0)!=0
									            AND CONCAT(spm.YearNo,RIGHT('00'+Isnull(Cast(spm.MonthNo AS VARCHAR(max)), ''),2)) 
									            BETWEEN  CONCAT(YEAR('" + fromDate + @"'),RIGHT('00'+Isnull(Cast(Month('" + fromDate + @"') AS VARCHAR(max)), ''),2))
									            AND CONCAT(YEAR('" + toDate + @"'),RIGHT('00'+Isnull(Cast(Month('" + toDate + @"') AS VARCHAR(max)), ''),2)) )SPCD ON SPCD.EmpInfoSystemID=s.EmpSystemId AND SPCD.YearNo=SPM.YearNo AND SPCD.MonthNo=SPM.MonthNo
LEFT JOIN(
									Select  ISNULL(TotalPayDay,0)TotalPayDay,EmpSystemID,YearNo,MonthNo from dbo.SalaryProceAttdnData 
where CONCAT(YearNo,RIGHT('00'+Isnull(Cast(MonthNo AS VARCHAR(max)), ''),2)) 
BETWEEN  CONCAT(YEAR('" + fromDate + @"'),RIGHT('00'+Isnull(Cast(Month('" + fromDate + @"') AS VARCHAR(max)), ''),2))
AND CONCAT(YEAR('" + toDate + @"'),RIGHT('00'+Isnull(Cast(Month('" + toDate + @"') AS VARCHAR(max)), ''),2))
									)PD ON PD.EmpSystemID=s.EmpSystemId AND PD.YearNo=SPM.YearNo AND PD.MonthNo=SPM.MonthNo
                                    LEFT OUTER JOIN HKP.Designation egdsg on egdsg.id=s.DesignationId
                                    LEFT OUTER JOIN HKP.LegalDesignation  ld on ld.Id=s.LegalDesignationId
                                    LEFT OUTER JOIN (select dm.DesignationGroupId,dm.DesignationId,dm.EmployeeCategoryId
                                                    ,dg.UserName GivenDesignationGroup
                                                    FROM mst.DesignationMaster dm
                                                    LEFT OUTER JOIN HKP.DesignationGroup dg on dg.Id=dm.DesignationGroupId
                                                    ) egdsgg on egdsgg.DesignationId=e.GivenDesignationId AND egdsgg.EmployeeCategoryId=s.EmployeeCategoryId
                                    LEFT OUTER JOIN MST.ManpowerBudget mpb on mpb.Id=s.BudgetCode
                                    LEFT OUTER JOIN ORG.Position PO ON mpb.PositionId=PO.Id
                                    LEFT OUTER JOIN ORG.Entity EN ON mpb.EntityId=EN.Id
                                    LEFT JOIN [ORG].[Department] ON Department.Id = PO.DepartmentId
                                    LEFT JOIN [ORG].[Division] ON Division.Id = EN.DivisionId
                                    LEFT JOIN [ORG].[Plant] ON Plant.Id = EN.PlantId
                                    LEFT JOIN [ORG].[Section] ON Section.Id = PO.SectionId
                                    LEFT JOIN [ORG].[SubSection] ON SubSection.Id = PO.SubSectionId
                                    LEFT JOIN [ORG].[Unit] ON Unit.Id = EN.UnitId                                   
                                    LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = E.GivenDesignationId
                                    LEFT JOIN SCS.DesignationMasterConfiguration DMC ON DMC.DesignationMasterId=DesM.Id
									LEFT JOIN dbo.AccountsGroup AG ON AG.Id=DMC.AccountsGroupId
                                    LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId			                                       
                                    LEFT JOIN ORG.Line AS eL ON eL.Id= mpb.LineId
                                    Left outer join MST.PayrollGroupMaster PGM ON PGM.employeeid = E.SystemId
                                    Left outer join HKP.PayrollGroup PG ON PG.id = PGM.PayrollGroupId
                                    Left Join [dbo].[JobLocation] jl on jl.SystemID = E.JobLocationID
                                    left join [HKP].[Bank] bb on bb.Id = s.BankSystemID
                                    Left join SalaryLock sl on sl.EmpSystemId=e.SystemId AND sl.YearNo=SPM.YearNo AND sl.MonthNo=SPM.MonthNo
                                    LEFT JOIN TRN.Voucher  V ON V.Id=sl.PayableVoucherId 
                                    LEFT JOIN TRN.Voucher  Vl ON Vl.Id=sl.BonusDisbursementVoucherId 
                                    LEFT JOIN [dbo].[BonusDisbursementAdvice]  DA ON DA.Id=sl.BonusDisbursementAdviceId 
                                    WHERE  s.CompanyGroupId='" + identity.CompanyGroupId + "' AND s.PlantId='" + identity.PlantId + @"' 
                                    AND ISNULL(e.PaymentMode,'')='" + paymentMode + "' AND ISNULL(sl.PayableVoucherId,'')<>'' AND ISNULL(sl.PastBonusDisbursed,0) = 0  and sl.islocked=1 AND ISNULL(sl.IsBonusDisbursed,0) = 0 " + wcPayrollGroup + @" 
                                    AND CONCAT(sl.YearNo,RIGHT('00'+Isnull(Cast(SL.MonthNo AS VARCHAR(max)), ''),2)) 
									BETWEEN  CONCAT(YEAR('" + fromDate + @"'),RIGHT('00'+Isnull(Cast(Month('" + fromDate + @"') AS VARCHAR(max)), ''),2))
									AND CONCAT(YEAR('" + toDate + @"'),RIGHT('00'+Isnull(Cast(Month('" + toDate + @"') AS VARCHAR(max)), ''),2))
                                    ) DD " + wcEmpStatus + @" GROUP BY EmpSystemId,EmployeeId,EmployeeCode,EmployeeName,EmployeeCategory
									,DOJ,DOS--,CurrentMonthEmployeeStatus
,EmployeeStatus,PaymentMode";
            data = _sqlRepository.GetDataTable(sql);
        }

        [HttpPost, Authorize]
        public ActionResult GetEmployeeSalaryUnDisbursed(string effectiveDate, string salaryProcessId, bool isActive, bool isSeperated, bool isMaternity, string paymentMode)
        {
            try
            {

                string fileName = "";
                fileName = GeSalaryUndisburseXlsReport(effectiveDate, salaryProcessId, isActive, isSeperated, isMaternity, paymentMode, "Salary UnDisbursed");

                return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);


            }
            catch (Exception ex)
            {
                return Json(new { Message = ex.Message, Error = true }, JsonRequestBehavior.AllowGet);

            }
        }

        public string GeSalaryUndisburseXlsReport(string effectiveDate, string salaryProcessId, bool isActive, bool isSeperated, bool isMaternity, string paymentMode, string SheetName)
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
                workbook.Worksheets[0].Name = "Salary Not Disbursed";
                sheet = workbook.Worksheets[0];
                DataTable data;
                SalaryUndisbursedReportQry(effectiveDate, salaryProcessId, isActive, isSeperated, isMaternity, paymentMode, out data);

                int ROW = 6; int COL = 1;

                #region Columns


                //sheet[ROW, COL].Text = "Month Name";
                //sheet[ROW, COL].ColumnWidth = 16;
                //int ColMonthName = COL;
                //COL++;

                sheet[ROW, COL].Text = "EmployeeCode";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColEC = COL;
                COL++;

                sheet[ROW, COL].Text = "EmployeeName";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColEN = COL;
                COL++;

                sheet[ROW, COL].Text = "DOJ";
                sheet[ROW, COL].ColumnWidth = 16;
                sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int ColDOJ = COL;
                COL++;

                sheet[ROW, COL].Text = "DOS";
                sheet[ROW, COL].ColumnWidth = 16;
                sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int ColDOS = COL;
                COL++;

                sheet[ROW, COL].Text = "EmployeeCategory";
                sheet[ROW, COL].ColumnWidth = 16;
                sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int ColEcg = COL;
                COL++;

                sheet[ROW, COL].Text = "Department";
                sheet[ROW, COL].ColumnWidth = 16;
                sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int ColDep = COL;
                COL++;

                sheet[ROW, COL].Text = "Section";
                sheet[ROW, COL].ColumnWidth = 16;
                sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int ColSec = COL;
                COL++;

                sheet[ROW, COL].Text = "SubSection";
                sheet[ROW, COL].ColumnWidth = 16;
                sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int ColSS = COL;
                COL++;

                sheet[ROW, COL].Text = "Designation";
                sheet[ROW, COL].ColumnWidth = 16;
                sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int ColDesg = COL;
                COL++;

                sheet[ROW, COL].Text = "PayableVoucherNo";
                sheet[ROW, COL].ColumnWidth = 16;
                sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int ColPblVhrNo = COL;
                COL++;

                sheet[ROW, COL].Text = "Lock";
                sheet[ROW, COL].ColumnWidth = 16;
                sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int ColLock = COL;
                COL++;

                sheet[ROW, COL].Text = "DisbursementVoucherNo";
                sheet[ROW, COL].ColumnWidth = 16;
                sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int ColDVNo = COL;
                COL++;

                sheet[ROW, COL].Text = "Disbursed";
                sheet[ROW, COL].ColumnWidth = 16;
                sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int ColDisbursed = COL;
                COL++;

                sheet[ROW, COL].Text = "PayRollGroup";
                sheet[ROW, COL].ColumnWidth = 16;
                sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int ColPayRollGrp = COL;
                COL++;

                sheet[ROW, COL].Text = "JobLocation";
                sheet[ROW, COL].ColumnWidth = 16;
                sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int ColJobLocation = COL;
                COL++;


                sheet[ROW, COL].Text = "PaymentMode";
                sheet[ROW, COL].ColumnWidth = 16;
                sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int ColPM = COL;
                COL++;

                sheet[ROW, COL].Text = "Bank";
                sheet[ROW, COL].ColumnWidth = 16;
                sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int ColBank = COL;
                COL++;

                sheet[ROW, COL].Text = "NetPayable";
                sheet[ROW, COL].ColumnWidth = 16;
                sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int ColNetPay = COL;


                //sheet[ROW, COL].Text = "Bank Account No";
                //sheet[ROW, COL].ColumnWidth = 16;
                //sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                //int ColBAN = COL;
                //COL++;

                //sheet[ROW, COL].Text = "IFSC Code";
                //sheet[ROW, COL].ColumnWidth = 16;
                //sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                //int ColIFSC = COL;
                //COL++;

                #endregion Columns

                int endCol = COL;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Black;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Color = ExcelKnownColors.White;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Bold = true;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 9f;
                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);

                ROW++;
                int startRow = ROW;
                double[] arr = new double[3];
                for (int i = 0; i < data.Rows.Count; i++)
                {

                    //sheet[ROW, ColMonthName].Text = data.Rows[i]["MonthName"].ToString();
                    sheet[ROW, ColEC].Text = data.Rows[i]["EmployeeCode"].ToString();
                    sheet[ROW, ColEN].Text = data.Rows[i]["EmployeeName"].ToString();
                    sheet[ROW, ColDOJ].DateTime = Convert.ToDateTime(data.Rows[i]["DOJ"].ToString());
                    sheet[ROW, ColDOS].Text = data.Rows[i]["DOS"].ToString();
                    sheet[ROW, ColEcg].Text = data.Rows[i]["EmployeeCategory"].ToString();
                    sheet[ROW, ColDep].Text = data.Rows[i]["Department"].ToString();
                    sheet[ROW, ColSec].Text = data.Rows[i]["Section"].ToString();
                    sheet[ROW, ColSS].Text = data.Rows[i]["SubSection"].ToString();
                    sheet[ROW, ColDesg].Text = data.Rows[i]["Designation"].ToString();
                    sheet[ROW, ColPblVhrNo].Text = data.Rows[i]["PayableVoucherNo"].ToString();
                    sheet[ROW, ColLock].Text = data.Rows[i]["IsLock"].ToString();
                    sheet[ROW, ColDVNo].Text = data.Rows[i]["DisbursementVoucherNo"].ToString();
                    sheet[ROW, ColDisbursed].Text = data.Rows[i]["IsDisburse"].ToString();
                    sheet[ROW, ColPayRollGrp].Text = data.Rows[i]["PayRollGroup"].ToString();
                    sheet[ROW, ColJobLocation].Text = data.Rows[i]["JobLocation"].ToString();

                    sheet[ROW, ColPM].Text = data.Rows[i]["PaymentMode"].ToString();
                    sheet[ROW, ColBank].Text = data.Rows[i]["BankName"].ToString();
                    //sheet[ROW, ColBAN].Text = data.Rows[i]["BankAccNo"].ToString();
                    //sheet[ROW, ColIFSC].Text = data.Rows[i]["IFSCCode"].ToString();
                    sheet[ROW, ColNetPay].Text = data.Rows[i]["NetPayment"].ToString();


                    ROW++;
                }



                sheet.UsedRange.WrapText = false;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[startRow, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                sheet["A" + startRow.ToString()].FreezePanes();

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ReportUtility reportUtility = new ReportUtility();
                reportUtility.PlantHeader(ref sheet, endCol, "Salary Not Disbursed Report", identity.PlantId);
                reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                sheet.UsedRange.WrapText = false;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.IsGridLinesVisible = true;
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

        [HttpPost, Authorize]
        public ActionResult GetEmployeeSalaryDisbursementVoucherWise(string voucherId)
        {
            try
            {
                string fileName = "";
                fileName = GetEmployeeSalaryDisbursementVoucherWiseXlsReport(voucherId, "SalaryDisbursement");
                return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Message = ex.Message, Error = true }, JsonRequestBehavior.AllowGet);
            }
        }

        public string GetEmployeeSalaryDisbursementVoucherWiseXlsReport(string voucherId, string SheetName)
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
                workbook.Worksheets[0].Name = "SalaryDisbursement";
                sheet = workbook.Worksheets[0];
                DataTable data;
                SalaryDisbursementVoucherWiseQry(voucherId, out data);

                int ROW = 6; int COL = 1;

                #region Columns


                sheet[ROW, COL].Text = "DisbursementAdviceId";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColDisbursementAdviceId = COL;
                COL++;

                sheet[ROW, COL].Text = "AdviceDate";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColAdviceDate = COL;
                COL++;

                sheet[ROW, COL].Text = "Remarks";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColRemarks = COL;
                COL++;

                sheet[ROW, COL].Text = "AddedBy";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColAddedBy = COL;
                COL++;

                sheet[ROW, COL].Text = "Year";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColYear = COL;
                COL++;

                sheet[ROW, COL].Text = "Month";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColMonth = COL;
                COL++;

                sheet[ROW, COL].Text = "EmployeeCode";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColEC = COL;
                COL++;

                sheet[ROW, COL].Text = "EmployeeName";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColEN = COL;
                COL++;

                sheet[ROW, COL].Text = "Designation";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColDesg = COL;
                COL++;

                sheet[ROW, COL].Text = "Department";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColDep = COL;
                COL++;

                sheet[ROW, COL].Text = "EmployeeCategory";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColEcg = COL;
                COL++;

                sheet[ROW, COL].Text = "Section";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColSec = COL;
                COL++;

                sheet[ROW, COL].Text = "SubSection";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColSS = COL;
                COL++;
                
                sheet[ROW, COL].Text = "DOJ";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColDOJ = COL;
                COL++;

                sheet[ROW, COL].Text = "DOS";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColDOS = COL;
                COL++;

                sheet[ROW, COL].Text = "CurrentMonthEmployeeStatus";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColCurrentMonthEmployeeStatus = COL;
                COL++;

                sheet[ROW, COL].Text = "EmployeeStatus";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColEmployeeStatus = COL;
                COL++;

                sheet[ROW, COL].Text = "PayableVoucherNo";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColPblVhrNo = COL;
                COL++;

                sheet[ROW, COL].Text = "DisbursementVoucherNo";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColDVNo = COL;
                COL++;

                sheet[ROW, COL].Text = "PaymentMode";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColPM = COL;
                COL++;

                sheet[ROW, COL].Text = "Bank";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColBank = COL;
                COL++;

                sheet[ROW, COL].Text = "Bank Account No";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColBAN = COL;
                COL++;

                sheet[ROW, COL].Text = "IFSC Code";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColIFSC = COL;
                COL++;

                sheet[ROW, COL].Text = "NetPayable";
                sheet[ROW, COL].ColumnWidth = 16;
                sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int ColNetPay = COL;


                

                #endregion Columns

                int endCol = COL;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Black;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Color = ExcelKnownColors.White;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Bold = true;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 9f;
                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);

                ROW++;
                int startRow = ROW;
                double[] arr = new double[3];
                for (int i = 0; i < data.Rows.Count; i++)
                {

                    sheet[ROW, ColDisbursementAdviceId].Text = data.Rows[i]["DisbursementAdviceId"].ToString();
                    sheet[ROW, ColAdviceDate].DateTime = Convert.ToDateTime(data.Rows[i]["AdviceDate"].ToString());
                    sheet[ROW, ColRemarks].Text = data.Rows[i]["Remarks"].ToString();
                    sheet[ROW, ColAddedBy].Text = data.Rows[i]["AddedBy"].ToString();
                    sheet[ROW, ColYear].Text = data.Rows[i]["YearNo"].ToString();
                    sheet[ROW, ColMonth].Text = data.Rows[i]["MonthName"].ToString();
                    sheet[ROW, ColEC].Text = data.Rows[i]["EmployeeCode"].ToString();
                    sheet[ROW, ColEN].Text = data.Rows[i]["EmployeeName"].ToString();
                    sheet[ROW, ColDesg].Text = data.Rows[i]["Designation"].ToString();
                    sheet[ROW, ColDep].Text = data.Rows[i]["Department"].ToString();
                    sheet[ROW, ColEcg].Text = data.Rows[i]["EmployeeCategory"].ToString();
                    sheet[ROW, ColSec].Text = data.Rows[i]["Section"].ToString();
                    sheet[ROW, ColSS].Text = data.Rows[i]["SubSection"].ToString();
                    sheet[ROW, ColDOJ].DateTime = Convert.ToDateTime(data.Rows[i]["DOJ"].ToString());
                    sheet[ROW, ColDOS].Text = data.Rows[i]["DOS"].ToString();
                    sheet[ROW, ColCurrentMonthEmployeeStatus].Text = data.Rows[i]["CurrentMonthEmployeeStatus"].ToString();
                    sheet[ROW, ColEmployeeStatus].Text = data.Rows[i]["EmployeeStatus"].ToString();
                    sheet[ROW, ColPblVhrNo].Text = data.Rows[i]["PayableVoucherNo"].ToString();
                    sheet[ROW, ColDVNo].Text = data.Rows[i]["DisbursementVoucherNo"].ToString();
                    sheet[ROW, ColPM].Text = data.Rows[i]["PaymentMode"].ToString();
                    sheet[ROW, ColBank].Text = data.Rows[i]["BankName"].ToString();
                    sheet[ROW, ColBAN].Text = data.Rows[i]["BankAccNo"].ToString();
                    sheet[ROW, ColIFSC].Text = data.Rows[i]["IFSCCode"].ToString();
                    sheet[ROW, ColNetPay].Number = Convert.ToDouble(data.Rows[i]["Amount"].ToString());

                    ROW++;
                }



                sheet.UsedRange.WrapText = false;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[startRow, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                sheet["A" + startRow.ToString()].FreezePanes();

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ReportUtility reportUtility = new ReportUtility();
                reportUtility.PlantHeader(ref sheet, endCol, "Salary Disbursement Report", identity.PlantId);
                reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                sheet.UsedRange.WrapText = false;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.IsGridLinesVisible = true;
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

        [HttpPost, Authorize]
        public ActionResult GetEmployeeBonusDisbursementVoucherWise(string voucherId)
        {
            try
            {
                string fileName = "";
                fileName = GetEmployeeBonusDisbursementVoucherWiseXlsReport(voucherId, "BonusDisbursement");
                return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Message = ex.Message, Error = true }, JsonRequestBehavior.AllowGet);
            }
        }

        public string GetEmployeeBonusDisbursementVoucherWiseXlsReport(string voucherId, string SheetName)
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
                workbook.Worksheets[0].Name = "BonusDisbursement";
                sheet = workbook.Worksheets[0];
                DataTable data;
                BonusDisbursementVoucherWiseQry(voucherId, out data);

                int ROW = 6; int COL = 1;

                #region Columns


                sheet[ROW, COL].Text = "BonusDisbursementAdviceId";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColDisbursementAdviceId = COL;
                COL++;

                sheet[ROW, COL].Text = "AdviceDate";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColAdviceDate = COL;
                COL++;

                sheet[ROW, COL].Text = "Remarks";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColRemarks = COL;
                COL++;

                sheet[ROW, COL].Text = "AddedBy";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColAddedBy = COL;
                COL++;

                sheet[ROW, COL].Text = "Year";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColYear = COL;
                COL++;

                sheet[ROW, COL].Text = "Month";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColMonth = COL;
                COL++;

                sheet[ROW, COL].Text = "EmployeeCode";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColEC = COL;
                COL++;

                sheet[ROW, COL].Text = "EmployeeName";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColEN = COL;
                COL++;

                sheet[ROW, COL].Text = "Designation";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColDesg = COL;
                COL++;

                sheet[ROW, COL].Text = "Department";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColDep = COL;
                COL++;

                sheet[ROW, COL].Text = "EmployeeCategory";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColEcg = COL;
                COL++;

                sheet[ROW, COL].Text = "Section";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColSec = COL;
                COL++;

                sheet[ROW, COL].Text = "SubSection";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColSS = COL;
                COL++;

                sheet[ROW, COL].Text = "DOJ";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColDOJ = COL;
                COL++;

                sheet[ROW, COL].Text = "DOS";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColDOS = COL;
                COL++;

                sheet[ROW, COL].Text = "CurrentMonthEmployeeStatus";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColCurrentMonthEmployeeStatus = COL;
                COL++;

                sheet[ROW, COL].Text = "EmployeeStatus";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColEmployeeStatus = COL;
                COL++;

                sheet[ROW, COL].Text = "PayableVoucherNo";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColPblVhrNo = COL;
                COL++;

                sheet[ROW, COL].Text = "DisbursementVoucherNo";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColDVNo = COL;
                COL++;

                sheet[ROW, COL].Text = "PaymentMode";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColPM = COL;
                COL++;

                sheet[ROW, COL].Text = "Bank";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColBank = COL;
                COL++;

                sheet[ROW, COL].Text = "Bank Account No";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColBAN = COL;
                COL++;

                sheet[ROW, COL].Text = "IFSC Code";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColIFSC = COL;
                COL++;

                sheet[ROW, COL].Text = "BonusPayment";
                sheet[ROW, COL].ColumnWidth = 16;
                sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int ColNetPay = COL;




                #endregion Columns

                int endCol = COL;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Black;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Color = ExcelKnownColors.White;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Bold = true;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 9f;
                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);

                ROW++;
                int startRow = ROW;
                double[] arr = new double[3];
                for (int i = 0; i < data.Rows.Count; i++)
                {

                    sheet[ROW, ColDisbursementAdviceId].Text = data.Rows[i]["BonusDisbursementAdviceId"].ToString();
                    sheet[ROW, ColAdviceDate].DateTime = Convert.ToDateTime(data.Rows[i]["AdviceDate"].ToString());
                    sheet[ROW, ColRemarks].Text = data.Rows[i]["Remarks"].ToString();
                    sheet[ROW, ColAddedBy].Text = data.Rows[i]["AddedBy"].ToString();
                    sheet[ROW, ColYear].Text = data.Rows[i]["YearNo"].ToString();
                    sheet[ROW, ColMonth].Text = data.Rows[i]["MonthName"].ToString();
                    sheet[ROW, ColEC].Text = data.Rows[i]["EmployeeCode"].ToString();
                    sheet[ROW, ColEN].Text = data.Rows[i]["EmployeeName"].ToString();
                    sheet[ROW, ColDesg].Text = data.Rows[i]["Designation"].ToString();
                    sheet[ROW, ColDep].Text = data.Rows[i]["Department"].ToString();
                    sheet[ROW, ColEcg].Text = data.Rows[i]["EmployeeCategory"].ToString();
                    sheet[ROW, ColSec].Text = data.Rows[i]["Section"].ToString();
                    sheet[ROW, ColSS].Text = data.Rows[i]["SubSection"].ToString();
                    sheet[ROW, ColDOJ].DateTime = Convert.ToDateTime(data.Rows[i]["DOJ"].ToString());
                    sheet[ROW, ColDOS].Text = data.Rows[i]["DOS"].ToString();
                    sheet[ROW, ColCurrentMonthEmployeeStatus].Text = data.Rows[i]["CurrentMonthEmployeeStatus"].ToString();
                    sheet[ROW, ColEmployeeStatus].Text = data.Rows[i]["EmployeeStatus"].ToString();
                    sheet[ROW, ColPblVhrNo].Text = data.Rows[i]["PayableVoucherNo"].ToString();
                    sheet[ROW, ColDVNo].Text = data.Rows[i]["DisbursementVoucherNo"].ToString();
                    sheet[ROW, ColPM].Text = data.Rows[i]["PaymentMode"].ToString();
                    sheet[ROW, ColBank].Text = data.Rows[i]["BankName"].ToString();
                    sheet[ROW, ColBAN].Text = data.Rows[i]["BankAccNo"].ToString();
                    sheet[ROW, ColIFSC].Text = data.Rows[i]["IFSCCode"].ToString();
                    sheet[ROW, ColNetPay].Number = Convert.ToDouble(data.Rows[i]["Amount"].ToString());

                    ROW++;
                }



                sheet.UsedRange.WrapText = false;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[startRow, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                sheet["A" + startRow.ToString()].FreezePanes();

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ReportUtility reportUtility = new ReportUtility();
                reportUtility.PlantHeader(ref sheet, endCol, "Bonus Disbursement Report", identity.PlantId);
                reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                sheet.UsedRange.WrapText = false;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.IsGridLinesVisible = true;
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
        [HttpPost, Authorize]
        public ActionResult GetEmployeeBonusDisbursementSummary(string fromDate, string toDate, string salaryProcessId, bool isActive, bool isSeperated, bool isMaternity, string paymentMode)
        {
            try
            {
                string fileName = "";
                fileName = GetEmployeeBonusDisbursementSummaryXlsReport( fromDate,  toDate,  salaryProcessId,  isActive,  isSeperated,  isMaternity,  paymentMode, "BonusDisbursementSummary");
                return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Message = ex.Message, Error = true }, JsonRequestBehavior.AllowGet);
            }
        }

        public string GetEmployeeBonusDisbursementSummaryXlsReport(string fromDate, string toDate, string salaryProcessId, bool isActive, bool isSeperated, bool isMaternity, string paymentMode, string SheetName)
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
                workbook.Worksheets[0].Name = "BonusDisbursementSummary";
                sheet = workbook.Worksheets[0];
                DataTable data;
                BonusDisbursementSummaryQry(fromDate, toDate, salaryProcessId, isActive, isSeperated, isMaternity, paymentMode, out data);

                int ROW = 6; int COL = 1;

                #region Columns

                sheet[ROW, COL].Text = "EmployeeCode";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColEC = COL;
                COL++;

                sheet[ROW, COL].Text = "EmployeeName";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColEN = COL;
                COL++;

                //sheet[ROW, COL].Text = "Designation";
                //sheet[ROW, COL].ColumnWidth = 16;
                //int ColDesg = COL;
                //COL++;

                //sheet[ROW, COL].Text = "Department";
                //sheet[ROW, COL].ColumnWidth = 16;
                //int ColDep = COL;
                //COL++;

                sheet[ROW, COL].Text = "EmployeeCategory";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColEcg = COL;
                COL++;

                //sheet[ROW, COL].Text = "Section";
                //sheet[ROW, COL].ColumnWidth = 16;
                //int ColSec = COL;
                //COL++;

                //sheet[ROW, COL].Text = "SubSection";
                //sheet[ROW, COL].ColumnWidth = 16;
                //int ColSS = COL;
                //COL++;

                sheet[ROW, COL].Text = "DOJ";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColDOJ = COL;
                COL++;

                sheet[ROW, COL].Text = "DOS";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColDOS = COL;
                COL++;

                sheet[ROW, COL].Text = "CurrentMonthEmployeeStatus";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColCurrentMonthEmployeeStatus = COL;
                COL++;

                sheet[ROW, COL].Text = "EmployeeStatus";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColEmployeeStatus = COL;
                COL++;

                sheet[ROW, COL].Text = "PaymentMode";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColPM = COL;
                COL++;

                //sheet[ROW, COL].Text = "Bank";
                //sheet[ROW, COL].ColumnWidth = 16;
                //int ColBank = COL;
                //COL++;

                //sheet[ROW, COL].Text = "Bank Account No";
                //sheet[ROW, COL].ColumnWidth = 16;
                //int ColBAN = COL;
                //COL++;

                //sheet[ROW, COL].Text = "IFSC Code";
                //sheet[ROW, COL].ColumnWidth = 16;
                //int ColIFSC = COL;
                //COL++;

                sheet[ROW, COL].Text = "BonusPayment";
                sheet[ROW, COL].ColumnWidth = 16;
                sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int ColNetPay = COL;




                #endregion Columns

                int endCol = COL;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Black;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Color = ExcelKnownColors.White;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Bold = true;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 9f;
                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);

                ROW++;
                int startRow = ROW;
                double[] arr = new double[3];
                for (int i = 0; i < data.Rows.Count; i++)
                {
                    sheet[ROW, ColEC].Text = data.Rows[i]["EmployeeCode"].ToString();
                    sheet[ROW, ColEN].Text = data.Rows[i]["EmployeeName"].ToString();
                    //sheet[ROW, ColDesg].Text = data.Rows[i]["Designation"].ToString();
                    //sheet[ROW, ColDep].Text = data.Rows[i]["Department"].ToString();
                    sheet[ROW, ColEcg].Text = data.Rows[i]["EmployeeCategory"].ToString();
                    //sheet[ROW, ColSec].Text = data.Rows[i]["Section"].ToString();
                    //sheet[ROW, ColSS].Text = data.Rows[i]["SubSection"].ToString();
                    sheet[ROW, ColDOJ].DateTime = Convert.ToDateTime(data.Rows[i]["DOJ"].ToString());
                    sheet[ROW, ColDOS].Text = data.Rows[i]["DOS"].ToString();
                    sheet[ROW, ColCurrentMonthEmployeeStatus].Text = data.Rows[i]["CurrentMonthEmployeeStatus"].ToString();
                    sheet[ROW, ColEmployeeStatus].Text = data.Rows[i]["EmployeeStatus"].ToString();
                    sheet[ROW, ColPM].Text = data.Rows[i]["PaymentMode"].ToString();
                    //sheet[ROW, ColBank].Text = data.Rows[i]["BankName"].ToString();
                    //sheet[ROW, ColBAN].Text = data.Rows[i]["BankAccNo"].ToString();
                    //sheet[ROW, ColIFSC].Text = data.Rows[i]["IFSCCode"].ToString();
                    sheet[ROW, ColNetPay].Number = Convert.ToDouble(data.Rows[i]["Amount"].ToString());

                    ROW++;
                }



                sheet.UsedRange.WrapText = false;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[startRow, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                sheet["A" + startRow.ToString()].FreezePanes();

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ReportUtility reportUtility = new ReportUtility();
                reportUtility.PlantHeader(ref sheet, endCol, "Bonus Disbursement Summary Report From " + fromDate + " To " + toDate + "", identity.PlantId);
                reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                sheet.UsedRange.WrapText = false;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.IsGridLinesVisible = true;
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
        [HttpPost, Authorize]
        public ActionResult GetEmployeeBonusUnDisbursementSummary(string fromDate, string toDate, string salaryProcessId, bool isActive, bool isSeperated, bool isMaternity, string paymentMode)
        {
            try
            {
                string fileName = "";
                fileName = GetEmployeeBonusUnDisbursementSummaryXlsReport(fromDate, toDate, salaryProcessId, isActive, isSeperated, isMaternity, paymentMode, "BonusDisbursementSummary");
                return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Message = ex.Message, Error = true }, JsonRequestBehavior.AllowGet);
            }
        }

        public string GetEmployeeBonusUnDisbursementSummaryXlsReport(string fromDate, string toDate, string salaryProcessId, bool isActive, bool isSeperated, bool isMaternity, string paymentMode, string SheetName)
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
                workbook.Worksheets[0].Name = "BonusUnDisbursementSummary";
                sheet = workbook.Worksheets[0];
                DataTable data;
                BonusUnDisbursementSummaryQry(fromDate, toDate, salaryProcessId, isActive, isSeperated, isMaternity, paymentMode, out data);

                int ROW = 6; int COL = 1;

                #region Columns

                sheet[ROW, COL].Text = "EmployeeCode";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColEC = COL;
                COL++;

                sheet[ROW, COL].Text = "EmployeeName";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColEN = COL;
                COL++;

                //sheet[ROW, COL].Text = "Designation";
                //sheet[ROW, COL].ColumnWidth = 16;
                //int ColDesg = COL;
                //COL++;

                //sheet[ROW, COL].Text = "Department";
                //sheet[ROW, COL].ColumnWidth = 16;
                //int ColDep = COL;
                //COL++;

                sheet[ROW, COL].Text = "EmployeeCategory";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColEcg = COL;
                COL++;

                //sheet[ROW, COL].Text = "Section";
                //sheet[ROW, COL].ColumnWidth = 16;
                //int ColSec = COL;
                //COL++;

                //sheet[ROW, COL].Text = "SubSection";
                //sheet[ROW, COL].ColumnWidth = 16;
                //int ColSS = COL;
                //COL++;

                sheet[ROW, COL].Text = "DOJ";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColDOJ = COL;
                COL++;

                sheet[ROW, COL].Text = "DOS";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColDOS = COL;
                COL++;

                //sheet[ROW, COL].Text = "CurrentMonthEmployeeStatus";
                //sheet[ROW, COL].ColumnWidth = 16;
                //int ColCurrentMonthEmployeeStatus = COL;
                //COL++;

                sheet[ROW, COL].Text = "EmployeeStatus";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColEmployeeStatus = COL;
                COL++;

                sheet[ROW, COL].Text = "PaymentMode";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColPM = COL;
                COL++;

                sheet[ROW, COL].Text = "TotalPayDay";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColTPD = COL;
                COL++;

                //sheet[ROW, COL].Text = "Bank Account No";
                //sheet[ROW, COL].ColumnWidth = 16;
                //int ColBAN = COL;
                //COL++;

                //sheet[ROW, COL].Text = "IFSC Code";
                //sheet[ROW, COL].ColumnWidth = 16;
                //int ColIFSC = COL;
                //COL++;

                sheet[ROW, COL].Text = "BonusPayment";
                sheet[ROW, COL].ColumnWidth = 16;
                sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int ColNetPay = COL;




                #endregion Columns

                int endCol = COL;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Black;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Color = ExcelKnownColors.White;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Bold = true;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 9f;
                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);

                ROW++;
                int startRow = ROW;
                double[] arr = new double[3];
                for (int i = 0; i < data.Rows.Count; i++)
                {
                    sheet[ROW, ColEC].Text = data.Rows[i]["EmployeeCode"].ToString();
                    sheet[ROW, ColEN].Text = data.Rows[i]["EmployeeName"].ToString();
                    //sheet[ROW, ColDesg].Text = data.Rows[i]["Designation"].ToString();
                    //sheet[ROW, ColDep].Text = data.Rows[i]["Department"].ToString();
                    sheet[ROW, ColEcg].Text = data.Rows[i]["EmployeeCategory"].ToString();
                    //sheet[ROW, ColSec].Text = data.Rows[i]["Section"].ToString();
                    //sheet[ROW, ColSS].Text = data.Rows[i]["SubSection"].ToString();
                    sheet[ROW, ColDOJ].DateTime = Convert.ToDateTime(data.Rows[i]["DOJ"].ToString());
                    sheet[ROW, ColDOS].Text = data.Rows[i]["DOS"].ToString();
                    //sheet[ROW, ColCurrentMonthEmployeeStatus].Text = data.Rows[i]["CurrentMonthEmployeeStatus"].ToString();
                    sheet[ROW, ColEmployeeStatus].Text = data.Rows[i]["EmployeeStatus"].ToString();
                    sheet[ROW, ColPM].Text = data.Rows[i]["PaymentMode"].ToString();
                    sheet[ROW, ColTPD].Text = data.Rows[i]["TotalPayDay"].ToString();
                    //sheet[ROW, ColBank].Text = data.Rows[i]["BankName"].ToString();
                    //sheet[ROW, ColBAN].Text = data.Rows[i]["BankAccNo"].ToString();
                    //sheet[ROW, ColIFSC].Text = data.Rows[i]["IFSCCode"].ToString();
                    sheet[ROW, ColNetPay].Number = Convert.ToDouble(data.Rows[i]["Amount"].ToString());

                    ROW++;
                }



                sheet.UsedRange.WrapText = false;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[startRow, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                sheet["A" + startRow.ToString()].FreezePanes();

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ReportUtility reportUtility = new ReportUtility();
                reportUtility.PlantHeader(ref sheet, endCol, "Bonus UnDisbursement Summary Report From " + fromDate + " To " + toDate + "", identity.PlantId);
                reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                sheet.UsedRange.WrapText = false;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.IsGridLinesVisible = true;
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

        #endregion Report

        #endregion Salary UnDisbursed

        [HttpPost]
        public ActionResult Save(Dictionary<string, object> DisbursementAdvice, List<SalaryLock> EmployeeList)
        {
            try
            {
                SaveSalaryLock(DisbursementAdvice,EmployeeList);
                return Json(new { Message = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {

                throw;
            }
        }
        public void SaveSalaryLock(Dictionary<string, object> DisbursementAdvice, List<SalaryLock> EmployeeList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            DataSet MasterDS;
            DataSet dsMaster;
            try
            {
                string EmpIdLoop = "";
                foreach (var item in EmployeeList)
                {
                    if (EmpIdLoop == "")
                    {
                        EmpIdLoop = "'" + item.EmpSystemId + "'"; ;
                    }
                    else
                    {
                        EmpIdLoop += ",'" + item.EmpSystemId + "'";

                    }
                }

                string sql = "select * from SalaryLock where  MonthNo='" + EmployeeList[0].MonthNo + @"' and YearNo='" + EmployeeList[0].YearNo + @"' and EmpSystemId IN (" + EmpIdLoop + @")";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");
                DataView DvMaster = new DataView(dsMaster.Tables[0]);

                dsMaster.Tables[0].DefaultView.RowFilter = "DisbursementVoucherId <> '' ";
                while (dsMaster.Tables[0].DefaultView.Count > 0)
                {
                    for (int i = 0; i < EmployeeList.Count; i++)
                    {
                        if (EmployeeList[i].EmpSystemId == dsMaster.Tables[0].DefaultView[0]["EmpSystemId"].ToString() && EmployeeList[i].IsLocked == false)
                        {
                            throw new Exception("Accounting Disbursement already done for this Employee [" + EmployeeList[i].EmployeeCode + "]");
                        }
                    }
                }

                string _Id = string.Empty;
                string _masterId = string.Empty;
                string sqlDA = "SELECT * FROM [dbo].[DisbursementAdvice] WHERE Id='" + DisbursementAdvice["Id"] + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sqlDA, out MasterDS, false, "1");

                if (MasterDS.Tables[0].Rows.Count == 0)
                {
                    DataRow drMS = MasterDS.Tables[0].NewRow();
                    AccountsCommonService _accountsCommonService = new AccountsCommonService(_sqlRepository);
                    _Id = _accountsCommonService.GetAutoNumber(nameof(DisbursementAdvice), PKGeneratorEnum.Yearly, null, DateTime.Now);

                    drMS["Id"] = _Id;
                    drMS["YearNo"] = EmployeeList.FirstOrDefault().YearNo;
                    drMS["MonthNo"] = EmployeeList.FirstOrDefault().MonthNo;
                    drMS["Status"] = "Active";
                    drMS["Remarks"] = DisbursementAdvice["Remarks"];
                    drMS["PaymentMode"] = DisbursementAdvice["PaymentMode"];
                    drMS["AddedBy"] = identity.Name;
                    drMS["AddedDate"] = DateTime.Now;
                    drMS["AddedFromIP"] = identity.IPAddress;
                    MasterDS.Tables[0].Rows.Add(drMS);
                }
                
                _masterId = MasterDS.Tables[0].Rows[0]["Id"].ToString();

                foreach (var item in EmployeeList)
                {
                    DvMaster.RowFilter = "EmpSystemId='" + item.EmpSystemId + @"'";

                    DataRow dr = DvMaster[0].Row;
                    dr.BeginEdit();

                    dr["IsDisbursed"] = true;
                    dr["DisbursementAdviceId"] = _masterId;
                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = DateTime.Now;
                    dr["UpdatedFromIP"] = identity.IPAddress;

                    dr.EndEdit();
                    
                    DvMaster.RowFilter = null;
                }
                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(MasterDS,dsMaster);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        #region Save SalaryUnDisbursed 
        [HttpPost]
        public ActionResult SaveSalaryUnDisbursed(List<SalaryLock> EmployeeList)
        {
            try
            {
                SaveSalaryUnDisbursedLock(EmployeeList);
                return Json(new { Message = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {

                throw;
            }
        }
        public void SaveSalaryUnDisbursedLock(List<SalaryLock> EmployeeList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            
            try
            {
                string IdLoop = "";
                foreach (var item in EmployeeList)
                {
                    if (IdLoop == "")
                    {
                        IdLoop = "'" + item.Id + "'"; ;
                    }
                    else
                    {
                        IdLoop += ",'" + item.Id + "'";

                    }
                }

                var vendorAdWr = new System.Text.StringBuilder();
                var vendorAdWrsql = "";
                vendorAdWrsql = @"update [dbo].[SalaryLock] set IsDisbursed=0, DisbursementAdviceId=null, UpdatedBy='" + identity.Name + "', UpdatedDate='" + DateTime.Now + "', UpdatedFromIP='" + identity.IPAddress + "' where Id IN (" + IdLoop + @") ";
                vendorAdWr.Append(vendorAdWrsql);
                _sqlRepository.ExecuteSqlCommand(vendorAdWr.ToString());
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion Save SalaryUnDisbursed 

        #region Save BonusUnDisbursed 
        [HttpPost]
        public ActionResult SaveBonusUnDisbursed(List<SalaryLock> EmployeeList)
        {
            try
            {
                SaveBonusUnDisbursedLock(EmployeeList);
                return Json(new { Message = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {

                throw;
            }
        }
        public void SaveBonusUnDisbursedLock(List<SalaryLock> EmployeeList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            try
            {
                string IdLoop = "";
                foreach (var item in EmployeeList)
                {
                    if (IdLoop == "")
                    {
                        IdLoop = "'" + item.Id + "'"; ;
                    }
                    else
                    {
                        IdLoop += ",'" + item.Id + "'";

                    }
                }

                var vendorAdWr = new System.Text.StringBuilder();
                var vendorAdWrsql = "";
                vendorAdWrsql = @"update [dbo].[SalaryLock] set IsBonusDisbursed=0, BonusDisbursementAdviceId=null, UpdatedBy='" + identity.Name + "', UpdatedDate='" + DateTime.Now + "', UpdatedFromIP='" + identity.IPAddress + "' where Id IN (" + IdLoop + @") ";
                vendorAdWr.Append(vendorAdWrsql);
                _sqlRepository.ExecuteSqlCommand(vendorAdWr.ToString());
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion Save SalaryUnDisbursed 
        public class SalaryLock : BaseModel
        {
            #region Scalar Properties            
            public string Id { get; set; }
            public string EmpSystemId { get; set; }
            public int YearNo { get; set; }
            public int MonthNo { get; set; }
            public bool IsLocked { get; set; }
            public bool Lock { get; set; }
            public bool IsDisbursed { get; set; }
            public string PayableVoucherId { get; set; }
            public string DisbursementVoucherId { get; set; }
            public bool CheckBoxSelect { get; set; }
            public string EmployeeCode { get; set; }
            #endregion Scalar Properties

            #region Audit Properties
            [NeverUpdate]
            public string AddedBy { get; set; }
            [NeverUpdate]
            public DateTime? AddedDate { get; set; }
            [NeverUpdate]
            public string AddedFromIP { get; set; }

            public string UpdatedBy { get; set; }
            public DateTime? UpdatedDate { get; set; }
            public string UpdatedFromIP { get; set; }

            #endregion Audit Properties
        }


        #endregion

        #region Bonus Disbursement
        [HttpPost, Authorize]
        public ActionResult GetEmpInfoBonusDisbursement(string fromDate, string toDate, string salaryProcessId, bool isActive, bool isSeperated, bool isMaternity, string paymentMode)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            bool sa = identity.IsSysAdmin;
            bool ca = identity.IsControlAdmin;
            string userId = identity.UserId;
            string plantId = identity.PlantId;
            string companyGroupId = identity.CompanyGroupId;
            var wcPayrollGroup = "";
            string wcEmpStatus = " Where (1=0 ";

            if (sa == true || ca == true)
            {
                wcPayrollGroup = @"";
            }
            if (salaryProcessId == "STRUCTURE")
            {
                wcEmpStatus = " Where (1=1 ";
            }
            else
            {
                wcEmpStatus = " Where (1=0 ";

                if (isActive == true && isSeperated == true && isMaternity == true)
                {
                    wcEmpStatus = " Where (1=1 ";
                }
                else
                {
                    if (isActive == true)
                    {
                        wcEmpStatus += " OR SalaryProcFlag ='Regular'";
                    }
                    if (isSeperated == true)
                    {
                        wcEmpStatus += " OR SalaryProcFlag ='SEPARATED'";
                    }
                    if (isMaternity == true)
                    {
                        wcEmpStatus += " OR SalaryProcFlag ='MLV_PRE'";

                    }
                }
            }

            wcEmpStatus += ")";

            string sql = @"select [isSelect] = Convert(bit, 'False'),[isToBeSelect] = Convert(bit, 'False'),* FROM (  SELECT   dISTINCT   
                                     isnull(e.SystemId,'') EmpSystemId
									,ISNULL(e.EmployeeId,'')  EmployeeId 
	                                ,sl.Id,CheckBoxSelect= CONVERT(bit,0)   
									,SPM.MonthNo
									,CASE WHEN SPM.MonthNo=1 THEN 'January'
									        WHEN SPM.MonthNo=2 THEN 'February'
									        WHEN SPM.MonthNo=3 THEN 'March'
									        WHEN SPM.MonthNo=4 THEN 'April'
									        WHEN SPM.MonthNo=5 THEN 'May'
									        WHEN SPM.MonthNo=6 THEN 'June'
									        WHEN SPM.MonthNo=7 THEN 'July'
									        WHEN SPM.MonthNo=8 THEN 'August'
									        WHEN SPM.MonthNo=9 THEN 'September'
									        WHEN SPM.MonthNo=10 THEN 'October'
									        WHEN SPM.MonthNo=11 THEN 'November'
									        WHEN SPM.MonthNo=12 THEN 'December'
									        ELSE '' END MonthName
									,SPM.YearNo ,sl.IsLocked AS Lock,ISNULL(e.EmployeeCode,'') EmployeeCode ,ISNULL(e.EmployeeName,'') EmployeeName								
                                    ,ISNULL(mpb.EntityId,'') EntityId,ISNULL(mpb.PositionId,'') PositionId ,isnull(ISNULL(egdsg.UserName,ld.UserName),'') Designation                                       
									,ISNULL(Department.UserName,'') Department ,ISNULL(Division.UserName,'') Division ,ISNULL(EmpC.UserName,'') EmployeeCategory
									,ISNULL(Plant.UserName,'') Plant ,ISNULL(Section.UserName,'') Section ,ISNULL(SubSection.UserName,'') SubSection 
									,ISNULL(Unit.UserName,'') Unit  ,ISNULL(eL.UserName,'') Line,ISNULL(REPLACE(CONVERT(VARCHAR(11), e.DOJ, 106), ' ', '-'),'') DOJ
                                    ,ISNULL(REPLACE(CONVERT(VARCHAR(11), e.DOS, 106), ' ', '-'),'') DOS
                                    ,CASE WHEN MONTH(DOS) =  SPM.MonthNo  AND YEAR(DOS) = SPM.YearNo then 'Separated' else 'Active' end CurrentMonthEmployeeStatus
                                    ,ISNULL(e.EmployeeStatus,'') EmployeeStatus
                                    , Case when Isnull(SPM.SalaryProcFlag,'') = '' THen 'Regular' else SalaryProcFlag end SalaryProcFlag
									,ISNULL(PG.UserName,'') PayRollGroup
                                    ,e.EmployeeCodePreFix,e.EmployeeCodeNumeric
                                    ,ISNULL(jl.JobLocation, '') JobLocation
									,ISNULL(DA.PaymentMode,'') PaymentMode
									,ISNULL(bb.UserName,'') BankName
                                    ,ISNULL(v.VoucherNo,'' ) VoucherNo
                                    ,ISNULL(sl.PayableVoucherId,'') PayableVoucherId
                                    ,ISNULL(sl.BonusDisbursementVoucherId,'') DisbursementVoucherId
                                    ,ISNULL(v.VoucherNo,'') as PayableVoucherNo
                                    ,ISNULL(vl.VoucherNo,'') as DisbursementVoucherNo
                                    ,sl.IsBonusDisbursed
                                    ,IsLock = case when sl.IsLocked = 1 then 'Locked' else 'Unlocked' end
                                    ,IsDisburse = case when sl.IsBonusDisbursed = 1 then 'Disbursed' else 'Not Disbursed' end 
                                    ,SPCD.NetPayment,SPM.SystemID SalaryProcId,SPM.AddedBy,AG.UserName AccountsGroup
                                    ,FORMAT(DA.AddedDate,'dd-MMM-yyyy') DisbursementDate
                                    ,isnull(sl.BonusDisbursementAdviceId,'')BonusDisbursementAdviceId,isnull(DA.Remarks,'')Remarks
                                    from SalaryProcessLogDetail s
                                    JOIN SalaryProcMaster SPM ON SPM.SystemID = s.SalaryProcessId 
                                    INNER JOIN EmployeeInformation e on e.SystemId= s.EmpSystemId
                                    INNER JOIN (select SPC.DisbusmentAmount NetPayment,SPC.EmpInfoSystemID,spm.YearNo,spm.MonthNo from SalaryProcChild SPC
                                                left join dbo.SalaryHead SH on SH.SalaryHeadID = SPC.SalaryHeadID
                                                JOIN SalaryProcMaster SPM ON SPM.SystemID = SPC.SlrProcMstSystemID 
                                                Where HeadCategory IN('Monthly Bonus Retain','Annual Bonus Retain') AND ISNULL(SPC.DisbusmentAmount,0)!=0
									            AND CONCAT(spm.YearNo,RIGHT('00'+Isnull(Cast(spm.MonthNo AS VARCHAR(max)), ''),2)) 
									            BETWEEN  CONCAT(YEAR('" + fromDate + @"'),RIGHT('00'+Isnull(Cast(Month('" + fromDate + @"') AS VARCHAR(max)), ''),2))
									            AND CONCAT(YEAR('" + toDate + @"'),RIGHT('00'+Isnull(Cast(Month('" + toDate + @"') AS VARCHAR(max)), ''),2)) )SPCD ON SPCD.EmpInfoSystemID=s.EmpSystemId AND SPCD.YearNo=SPM.YearNo AND SPCD.MonthNo=SPM.MonthNo
                                    LEFT OUTER JOIN HKP.Designation egdsg on egdsg.id=s.DesignationId
                                    LEFT OUTER JOIN HKP.LegalDesignation  ld on ld.Id=s.LegalDesignationId
                                    LEFT OUTER JOIN (select dm.DesignationGroupId,dm.DesignationId,dm.EmployeeCategoryId
                                                    ,dg.UserName GivenDesignationGroup
                                                    FROM mst.DesignationMaster dm
                                                    LEFT OUTER JOIN HKP.DesignationGroup dg on dg.Id=dm.DesignationGroupId
                                                    ) egdsgg on egdsgg.DesignationId=e.GivenDesignationId AND egdsgg.EmployeeCategoryId=s.EmployeeCategoryId
                                    LEFT OUTER JOIN MST.ManpowerBudget mpb on mpb.Id=s.BudgetCode
                                    LEFT OUTER JOIN ORG.Position PO ON mpb.PositionId=PO.Id
                                    LEFT OUTER JOIN ORG.Entity EN ON mpb.EntityId=EN.Id
                                    LEFT JOIN [ORG].[Department] ON Department.Id = PO.DepartmentId
                                    LEFT JOIN [ORG].[Division] ON Division.Id = EN.DivisionId
                                    LEFT JOIN [ORG].[Plant] ON Plant.Id = EN.PlantId
                                    LEFT JOIN [ORG].[Section] ON Section.Id = PO.SectionId
                                    LEFT JOIN [ORG].[SubSection] ON SubSection.Id = PO.SubSectionId
                                    LEFT JOIN [ORG].[Unit] ON Unit.Id = EN.UnitId                                   
                                    LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = E.GivenDesignationId
                                    LEFT JOIN SCS.DesignationMasterConfiguration DMC ON DMC.DesignationMasterId=DesM.Id
									LEFT JOIN dbo.AccountsGroup AG ON AG.Id=DMC.AccountsGroupId
                                    LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId			                                       
                                    LEFT JOIN ORG.Line AS eL ON eL.Id= mpb.LineId
                                    Left outer join MST.PayrollGroupMaster PGM ON PGM.employeeid = E.SystemId
                                    Left outer join HKP.PayrollGroup PG ON PG.id = PGM.PayrollGroupId
                                    Left Join [dbo].[JobLocation] jl on jl.SystemID = E.JobLocationID
                                    left join [HKP].[Bank] bb on bb.Id = s.BankSystemID
                                    Left join SalaryLock sl on sl.EmpSystemId=e.SystemId AND sl.YearNo=SPM.YearNo AND sl.MonthNo=SPM.MonthNo
                                    LEFT JOIN TRN.Voucher  V ON V.Id=sl.PayableVoucherId 
                                    LEFT JOIN TRN.Voucher  Vl ON Vl.Id=sl.BonusDisbursementVoucherId 
                                    LEFT JOIN [dbo].[BonusDisbursementAdvice]  DA ON DA.Id=sl.BonusDisbursementAdviceId 
                                    WHERE  s.CompanyGroupId='" + identity.CompanyGroupId + "' AND s.PlantId='" + identity.PlantId + "' AND ISNULL(e.PaymentMode,'')='" + paymentMode + @"' 
                                    AND ISNULL(sl.PayableVoucherId,'')<>'' and sl.islocked=1 AND  ISNULL(sl.IsBonusDisbursed,0) = 0      " + wcPayrollGroup + @" 
                                    AND CONCAT(sl.YearNo,RIGHT('00'+Isnull(Cast(SL.MonthNo AS VARCHAR(max)), ''),2)) 
									BETWEEN  CONCAT(YEAR('" + fromDate + @"'),RIGHT('00'+Isnull(Cast(Month('" + fromDate + @"') AS VARCHAR(max)), ''),2))
									AND CONCAT(YEAR('" + toDate + @"'),RIGHT('00'+Isnull(Cast(Month('" + toDate + @"') AS VARCHAR(max)), ''),2))
                                    ) DD " + wcEmpStatus + @" ORDER BY EmployeeCodePreFix,EmployeeCodeNumeric,YearNo,MonthNo";
            var empdata = _sqlRepository.GetDataCollection(sql);
            JsonResult json = Json(new { empdata }, JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;
            return json;
        }

        [HttpPost, Authorize]
        public ActionResult GetBonusUnDisbursed(string fromDate, string toDate, string salaryProcessId, bool isActive, bool isSeperated, bool isMaternity, string paymentMode)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            bool sa = identity.IsSysAdmin;
            bool ca = identity.IsControlAdmin;
            string userId = identity.UserId;
            string plantId = identity.PlantId;
            string companyGroupId = identity.CompanyGroupId;
            var wcPayrollGroup = "";
            string wcEmpStatus = " Where (1=0 ";

            if (sa == true || ca == true)
            {
                wcPayrollGroup = @"";
            }
            if (salaryProcessId == "STRUCTURE")
            {
                wcEmpStatus = " Where (1=1 ";
            }
            else
            {
                wcEmpStatus = " Where (1=0 ";

                if (isActive == true && isSeperated == true && isMaternity == true)
                {
                    wcEmpStatus = " Where (1=1 ";
                }
                else
                {
                    if (isActive == true)
                    {
                        wcEmpStatus += " OR SalaryProcFlag ='Regular'";
                    }
                    if (isSeperated == true)
                    {
                        wcEmpStatus += " OR SalaryProcFlag ='SEPARATED'";
                    }
                    if (isMaternity == true)
                    {
                        wcEmpStatus += " OR SalaryProcFlag ='MLV_PRE'";

                    }
                }
            }

            wcEmpStatus += ")";

            string sql = @"
--select [isSelect] = Convert(bit, 'False'),[isToBeSelect] = Convert(bit, 'False'),* FROM (  SELECT   dISTINCT   
--                                     isnull(e.SystemId,'') EmpSystemId
--									,ISNULL(e.EmployeeId,'')  EmployeeId 
--	                                ,sl.Id,CheckBoxSelect= CONVERT(bit,0)   
--									,SPM.MonthNo
--									,CASE WHEN SPM.MonthNo=1 THEN 'January'
--								            WHEN SPM.MonthNo=2 THEN 'February'
--								            WHEN SPM.MonthNo=3 THEN 'March'
--								            WHEN SPM.MonthNo=4 THEN 'April'
--								            WHEN SPM.MonthNo=5 THEN 'May'
--								            WHEN SPM.MonthNo=6 THEN 'June'
--								            WHEN SPM.MonthNo=7 THEN 'July'
--								            WHEN SPM.MonthNo=8 THEN 'August'
--								            WHEN SPM.MonthNo=9 THEN 'September'
--								            WHEN SPM.MonthNo=10 THEN 'October'
--								            WHEN SPM.MonthNo=11 THEN 'November'
--								            WHEN SPM.MonthNo=12 THEN 'December'
--								            ELSE '' END MonthName
--									,SPM.YearNo ,sl.IsLocked AS Lock,ISNULL(e.EmployeeCode,'') EmployeeCode ,ISNULL(e.EmployeeName,'') EmployeeName								
--                                    ,ISNULL(mpb.EntityId,'') EntityId,ISNULL(mpb.PositionId,'') PositionId ,isnull(ISNULL(egdsg.UserName,ld.UserName),'') Designation                                     - - 
--									,ISNULL(Department.UserName,'') Department ,ISNULL(Division.UserName,'') Division ,ISNULL(EmpC.UserName,'') EmployeeCategory
--									,ISNULL(Plant.UserName,'') Plant ,ISNULL(Section.UserName,'') Section ,ISNULL(SubSection.UserName,'') SubSection 
--									,ISNULL(Unit.UserName,'') Unit  ,ISNULL(eL.UserName,'') Line,ISNULL(REPLACE(CONVERT(VARCHAR(11), e.DOJ, 106), ' ', '-'),'') DOJ
--                                    ,ISNULL(REPLACE(CONVERT(VARCHAR(11), e.DOS, 106), ' ', '-'),'') DOS
--                                    , CASE WHEN MONTH(DOS) =  SPM.MonthNo  AND YEAR(DOS) = SPM.YearNo then 'Separated' else 'Active' end CurrentMonthEmployeeStatus
--                                    ,ISNULL(e.EmployeeStatus,'') EmployeeStatus
--                                    , Case when Isnull(SPM.SalaryProcFlag,'') = '' THen 'Regular' else SalaryProcFlag end SalaryProcFlag
--									,ISNULL(PG.UserName,'') PayRollGroup
--                                    ,e.EmployeeCodePreFix,e.EmployeeCodeNumeric
--                                    ,ISNULL(jl.JobLocation, '') JobLocation
--									,ISNULL(e.PaymentMode,'') PaymentMode
--									,ISNULL(bb.UserName,'') BankName
--                                    ,ISNULL(v.VoucherNo,'' ) VoucherNo
--                                    ,ISNULL(sl.PayableVoucherId,'') PayableVoucherId
--                                    ,ISNULL(sl.BonusDisbursementVoucherId,'') DisbursementVoucherId
--                                    ,ISNULL(v.VoucherNo,'') as PayableVoucherNo
--                                    ,ISNULL(vl.VoucherNo,'') as DisbursementVoucherNo
--                                    ,sl.IsBonusDisbursed
--                                    ,IsLock = case when sl.IsLocked = 1 then 'Locked' else 'Unlocked' end
--                                    ,IsDisburse = case when sl.IsBonusDisbursed = 1 then 'Disbursed' else 'Not Disbursed' end 
--                                    ,SPCD.NetPayment,SPCD.SalaryHeadID
--                                    ,SPM.SystemID SalaryProcId,SPM.AddedBy,AG.UserName AccountsGroup
--                                    ,FORMAT(DA.AddedDate,'dd-MMM-yyyy') DisbursementDate
--                                    ,sl.BonusDisbursementAdviceId,DA.Remarks
--                                    from SalaryProcessLogDetail s
--                                    JOIN SalaryProcMaster SPM ON SPM.SystemID = s.SalaryProcessId 
--                                    left join EmployeeInformation e on e.SystemId= s.EmpSystemId
--                                    INNER JOIN (select SPC.DisbusmentAmount NetPayment,SPC.EmpInfoSystemID,spm.YearNo,spm.MonthNo,SPC.SalaryHeadID from SalaryProcChild SPC
--                                    left join dbo.SalaryHead SH on SH.SalaryHeadID = SPC.SalaryHeadID
--                                    JOIN SalaryProcMaster SPM ON SPM.SystemID = SPC.SlrProcMstSystemID 
--                                    Where HeadCategory IN('Monthly Bonus Retain') AND ISNULL(SPC.DisbusmentAmount,0)!=0
--									AND CONCAT(spm.YearNo,RIGHT('00'+Isnull(Cast(spm.MonthNo AS VARCHAR(max)), ''),2)) 
--									BETWEEN  CONCAT(YEAR('" + fromDate + @"'),RIGHT('00'+Isnull(Cast(Month('" + fromDate + @"') AS VARCHAR(max)), ''),2))
--									AND CONCAT(YEAR('" + toDate + @"'),RIGHT('00'+Isnull(Cast(Month('" + toDate + @"') AS VARCHAR(max)), ''),2)) )SPCD ON SPCD.EmpInfoSystemID=s.EmpSystemId AND --SPCD.YearNo=SPM.YearNo AND SPCD.MonthNo=SPM.MonthNo
--                                    LEFT OUTER JOIN HKP.Designation egdsg on egdsg.id=s.DesignationId
--                                    LEFT OUTER JOIN HKP.LegalDesignation  ld on ld.Id=s.LegalDesignationId
--                                    LEFT OUTER JOIN (select dm.DesignationGroupId,dm.DesignationId,dm.EmployeeCategoryId
--                                    ,dg.UserName GivenDesignationGroup
--                                    FROM mst.DesignationMaster dm
--                                    LEFT OUTER JOIN HKP.DesignationGroup dg on dg.Id=dm.DesignationGroupId
--                                    ) egdsgg on egdsgg.DesignationId=e.GivenDesignationId
--                                    AND egdsgg.EmployeeCategoryId=s.EmployeeCategoryId
--                                    LEFT OUTER JOIN MST.ManpowerBudget mpb on mpb.Id=s.BudgetCode
--                                    LEFT OUTER JOIN ORG.Position PO ON mpb.PositionId=PO.Id
--                                    LEFT OUTER JOIN ORG.Entity EN ON mpb.EntityId=EN.Id
--                                    LEFT JOIN [ORG].[Department] ON Department.Id = PO.DepartmentId
--                                    LEFT JOIN [ORG].[Division] ON Division.Id = EN.DivisionId
--                                    LEFT JOIN [ORG].[Plant] ON Plant.Id = EN.PlantId
--                                    LEFT JOIN [ORG].[Section] ON Section.Id = PO.SectionId
--                                    LEFT JOIN [ORG].[SubSection] ON SubSection.Id = PO.SubSectionId
--                                    LEFT JOIN [ORG].[Unit] ON Unit.Id = EN.UnitId                                   
--                                    LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = E.GivenDesignationId
--                                    LEFT JOIN SCS.DesignationMasterConfiguration DMC ON DMC.DesignationMasterId=DesM.Id
--									LEFT JOIN dbo.AccountsGroup AG ON AG.Id=DMC.AccountsGroupId
--                                    LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId			                                       
--                                    LEFT JOIN ORG.Line AS eL ON eL.Id= mpb.LineId
--                                    Left outer join MST.PayrollGroupMaster PGM ON PGM.employeeid = E.SystemId
--                                    Left outer join HKP.PayrollGroup PG ON PG.id = PGM.PayrollGroupId
--                                    Left Join [dbo].[JobLocation] jl on jl.SystemID = E.JobLocationID
--                                    left join [HKP].[Bank] bb on bb.Id = s.BankSystemID
--                                    Left join SalaryLock sl on sl.EmpSystemId=e.SystemId AND sl.YearNo=SPM.YearNo AND sl.MonthNo=SPM.MonthNo
--                                    LEFT JOIN TRN.Voucher  V ON V.Id=sl.PayableVoucherId 
--                                    LEFT JOIN TRN.Voucher  Vl ON Vl.Id=sl.BonusDisbursementVoucherId left join trn.VoucherDetail vd on vd.VoucherId=v.Id and vd.TrnNature ='Monthly Bonus' and --vd.SalaryHeadId=SPCD.SalaryHeadID and vd.CrAmount>0 
--                                    LEFT JOIN [dbo].[BonusDisbursementAdvice]  DA ON DA.Id=sl.BonusDisbursementAdviceId 
--                                    WHERE  s.CompanyGroupId='" + identity.CompanyGroupId + "' AND s.PlantId='" + identity.PlantId + "' AND ISNULL(e.PaymentMode,'')='" + paymentMode + "' AND ISNULL(sl.PayableVoucherId,'')<>'' and sl.islocked=1 AND ISNULL(sl.IsBonusDisbursed,0) = 0 " + wcPayrollGroup + @" 
--                                    AND CONCAT(sl.YearNo,RIGHT('00'+Isnull(Cast(SL.MonthNo AS VARCHAR(max)), ''),2)) 
--									BETWEEN  CONCAT(YEAR('" + fromDate + @"'),RIGHT('00'+Isnull(Cast(Month('" + fromDate + @"') AS VARCHAR(max)), ''),2))
--									AND CONCAT(YEAR('" + toDate + @"'),RIGHT('00'+Isnull(Cast(Month('" + toDate + @"') AS VARCHAR(max)), ''),2))
--                                    ) DD " + wcEmpStatus + @" --ORDER BY EmployeeCodePreFix,EmployeeCodeNumeric,YearNo,MonthNo
--UNION
select [isSelect] = Convert(bit, 'False'),[isToBeSelect] = Convert(bit, 'False'),* FROM (  SELECT   dISTINCT   
                                     isnull(e.SystemId,'') EmpSystemId
									,ISNULL(e.EmployeeId,'')  EmployeeId 
	                                ,sl.Id,CheckBoxSelect= CONVERT(bit,0)   
									,SPM.MonthNo
									,CASE WHEN SPM.MonthNo=1 THEN 'January'
								            WHEN SPM.MonthNo=2 THEN 'February'
								            WHEN SPM.MonthNo=3 THEN 'March'
								            WHEN SPM.MonthNo=4 THEN 'April'
								            WHEN SPM.MonthNo=5 THEN 'May'
								            WHEN SPM.MonthNo=6 THEN 'June'
								            WHEN SPM.MonthNo=7 THEN 'July'
								            WHEN SPM.MonthNo=8 THEN 'August'
								            WHEN SPM.MonthNo=9 THEN 'September'
								            WHEN SPM.MonthNo=10 THEN 'October'
								            WHEN SPM.MonthNo=11 THEN 'November'
								            WHEN SPM.MonthNo=12 THEN 'December'
								            ELSE '' END MonthName
									,SPM.YearNo,PD.TotalPayDay ,sl.IsLocked AS Lock,ISNULL(e.EmployeeCode,'') EmployeeCode ,ISNULL(e.EmployeeName,'') EmployeeName
                                    ,ISNULL(mpb.EntityId,'') EntityId,ISNULL(mpb.PositionId,'') PositionId ,isnull(ISNULL(egdsg.UserName,ld.UserName),'') Designation                                       
									,ISNULL(Department.UserName,'') Department ,ISNULL(Division.UserName,'') Division ,ISNULL(EmpC.UserName,'') EmployeeCategory
									,ISNULL(Plant.UserName,'') Plant ,ISNULL(Section.UserName,'') Section ,ISNULL(SubSection.UserName,'') SubSection 
									,ISNULL(Unit.UserName,'') Unit  ,ISNULL(eL.UserName,'') Line,ISNULL(REPLACE(CONVERT(VARCHAR(11), e.DOJ, 106), ' ', '-'),'') DOJ
                                    ,ISNULL(REPLACE(CONVERT(VARCHAR(11), e.DOS, 106), ' ', '-'),'') DOS
                                    , CASE WHEN MONTH(DOS) =  SPM.MonthNo  AND YEAR(DOS) = SPM.YearNo then 'Separated' else 'Active' end CurrentMonthEmployeeStatus
                                    ,ISNULL(e.EmployeeStatus,'') EmployeeStatus
                                    , Case when Isnull(SPM.SalaryProcFlag,'') = '' THen 'Regular' else SalaryProcFlag end SalaryProcFlag
									,ISNULL(PG.UserName,'') PayRollGroup
                                    ,e.EmployeeCodePreFix,e.EmployeeCodeNumeric
                                    ,ISNULL(jl.JobLocation, '') JobLocation
									,ISNULL(e.PaymentMode,'') PaymentMode
									,ISNULL(bb.UserName,'') BankName
                                    ,ISNULL(v.VoucherNo,'' ) VoucherNo
                                    ,ISNULL(sl.PayableVoucherId,'') PayableVoucherId
                                    ,ISNULL(sl.BonusDisbursementVoucherId,'') DisbursementVoucherId
                                    ,ISNULL(v.VoucherNo,'') as PayableVoucherNo
                                    ,ISNULL(vl.VoucherNo,'') as DisbursementVoucherNo
                                    ,sl.IsBonusDisbursed
                                    ,IsLock = case when sl.IsLocked = 1 then 'Locked' else 'Unlocked' end
                                    ,IsDisburse = case when sl.IsBonusDisbursed = 1 then 'Disbursed' else 'Not Disbursed' end 
                                    ,SPCD.NetPayment,SPCD.SalaryHeadID
                                    ,SPM.SystemID SalaryProcId,SPM.AddedBy,AG.UserName AccountsGroup
                                    ,FORMAT(DA.AddedDate,'dd-MMM-yyyy') DisbursementDate
                                    ,sl.BonusDisbursementAdviceId,DA.Remarks
                                    from SalaryProcessLogDetail s
                                    JOIN SalaryProcMaster SPM ON SPM.SystemID = s.SalaryProcessId 
                                    left join EmployeeInformation e on e.SystemId= s.EmpSystemId
                                    INNER JOIN (select SPC.DisbusmentAmount NetPayment,SPC.EmpInfoSystemID,spm.YearNo,spm.MonthNo,SPC.SalaryHeadID from SalaryProcChild SPC
                                    left join dbo.SalaryHead SH on SH.SalaryHeadID = SPC.SalaryHeadID
                                    JOIN SalaryProcMaster SPM ON SPM.SystemID = SPC.SlrProcMstSystemID 
                                    Where HeadCategory IN('Annual Bonus Retain') AND ISNULL(SPC.DisbusmentAmount,0)!=0
									AND CONCAT(spm.YearNo,RIGHT('00'+Isnull(Cast(spm.MonthNo AS VARCHAR(max)), ''),2)) 
									BETWEEN  CONCAT(YEAR('" + fromDate + @"'),RIGHT('00'+Isnull(Cast(Month('" + fromDate + @"') AS VARCHAR(max)), ''),2))
									AND CONCAT(YEAR('" + toDate + @"'),RIGHT('00'+Isnull(Cast(Month('" + toDate + @"') AS VARCHAR(max)), ''),2)) )SPCD ON SPCD.EmpInfoSystemID=s.EmpSystemId AND SPCD.YearNo=SPM.YearNo AND SPCD.MonthNo=SPM.MonthNo
LEFT JOIN(
									Select  ISNULL(TotalPayDay,0)TotalPayDay,EmpSystemID,YearNo,MonthNo from dbo.SalaryProceAttdnData 
where CONCAT(YearNo,RIGHT('00'+Isnull(Cast(MonthNo AS VARCHAR(max)), ''),2)) 
BETWEEN  CONCAT(YEAR('" + fromDate + @"'),RIGHT('00'+Isnull(Cast(Month('" + fromDate + @"') AS VARCHAR(max)), ''),2))
AND CONCAT(YEAR('" + toDate + @"'),RIGHT('00'+Isnull(Cast(Month('" + toDate + @"') AS VARCHAR(max)), ''),2))
									)PD ON PD.EmpSystemID=s.EmpSystemId AND PD.YearNo=SPM.YearNo AND PD.MonthNo=SPM.MonthNo

                                    LEFT OUTER JOIN HKP.Designation egdsg on egdsg.id=s.DesignationId
                                    LEFT OUTER JOIN HKP.LegalDesignation  ld on ld.Id=s.LegalDesignationId
                                    LEFT OUTER JOIN (select dm.DesignationGroupId,dm.DesignationId,dm.EmployeeCategoryId
                                    ,dg.UserName GivenDesignationGroup
                                    FROM mst.DesignationMaster dm
                                    LEFT OUTER JOIN HKP.DesignationGroup dg on dg.Id=dm.DesignationGroupId
                                    ) egdsgg on egdsgg.DesignationId=e.GivenDesignationId
                                    AND egdsgg.EmployeeCategoryId=s.EmployeeCategoryId
                                    LEFT OUTER JOIN MST.ManpowerBudget mpb on mpb.Id=s.BudgetCode
                                    LEFT OUTER JOIN ORG.Position PO ON mpb.PositionId=PO.Id
                                    LEFT OUTER JOIN ORG.Entity EN ON mpb.EntityId=EN.Id
                                    LEFT JOIN [ORG].[Department] ON Department.Id = PO.DepartmentId
                                    LEFT JOIN [ORG].[Division] ON Division.Id = EN.DivisionId
                                    LEFT JOIN [ORG].[Plant] ON Plant.Id = EN.PlantId
                                    LEFT JOIN [ORG].[Section] ON Section.Id = PO.SectionId
                                    LEFT JOIN [ORG].[SubSection] ON SubSection.Id = PO.SubSectionId
                                    LEFT JOIN [ORG].[Unit] ON Unit.Id = EN.UnitId                                   
                                    LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = E.GivenDesignationId
                                    LEFT JOIN SCS.DesignationMasterConfiguration DMC ON DMC.DesignationMasterId=DesM.Id
									LEFT JOIN dbo.AccountsGroup AG ON AG.Id=DMC.AccountsGroupId
                                    LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId			                                       
                                    LEFT JOIN ORG.Line AS eL ON eL.Id= mpb.LineId
                                    Left outer join MST.PayrollGroupMaster PGM ON PGM.employeeid = E.SystemId
                                    Left outer join HKP.PayrollGroup PG ON PG.id = PGM.PayrollGroupId
                                    Left Join [dbo].[JobLocation] jl on jl.SystemID = E.JobLocationID
                                    left join [HKP].[Bank] bb on bb.Id = s.BankSystemID
                                    Left join SalaryLock sl on sl.EmpSystemId=e.SystemId AND sl.YearNo=SPM.YearNo AND sl.MonthNo=SPM.MonthNo
                                    LEFT JOIN TRN.Voucher  V ON V.Id=sl.PayableVoucherId 
                                    LEFT JOIN TRN.Voucher  Vl ON Vl.Id=sl.BonusDisbursementVoucherId
                                    left join trn.VoucherDetail vd on vd.VoucherId=v.Id and vd.TrnNature ='Annual Bonus' and vd.SalaryHeadId=SPCD.SalaryHeadID and vd.CrAmount>0
                                    LEFT JOIN [dbo].[BonusDisbursementAdvice]  DA ON DA.Id=sl.BonusDisbursementAdviceId 
                                    WHERE  s.CompanyGroupId='" + identity.CompanyGroupId + "' AND s.PlantId='" + identity.PlantId + "' AND ISNULL(e.PaymentMode,'')='" + paymentMode + @"' 
                                    AND ISNULL(sl.PayableVoucherId,'')<>''  and sl.islocked=1 AND ISNULL(sl.IsBonusDisbursed,0) = 0 AND V.IsPark=0 AND ISNULL(sl.PastBonusDisbursed,0) = 0 " + wcPayrollGroup + @" 
                                    AND CONCAT(sl.YearNo,RIGHT('00'+Isnull(Cast(SL.MonthNo AS VARCHAR(max)), ''),2)) 
									BETWEEN  CONCAT(YEAR('" + fromDate + @"'),RIGHT('00'+Isnull(Cast(Month('" + fromDate + @"') AS VARCHAR(max)), ''),2))
									AND CONCAT(YEAR('" + toDate + @"'),RIGHT('00'+Isnull(Cast(Month('" + toDate + @"') AS VARCHAR(max)), ''),2))
                                    ) DD " + wcEmpStatus + @" ORDER BY EmployeeCodePreFix,EmployeeCodeNumeric,YearNo,MonthNo
";

            JsonResult json = Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;
            return json;
        }

        [HttpPost]
        public ActionResult SaveBonus(Dictionary<string, object> DisbursementAdvice, List<SalaryLock> EmployeeList)
        {
            try
            {
                SaveBonusDisbursement(DisbursementAdvice, EmployeeList);
                return Json(new { Message = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {

                throw;
            }
        }
        public void SaveBonusDisbursement(Dictionary<string, object> DisbursementAdvice, List<SalaryLock> EmployeeList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            DataSet MasterDS;
            DataSet dsMaster;
            try
            {
                string SalaryLockIds = "";
                foreach (var item in EmployeeList)
                {
                    if (SalaryLockIds == "")
                    {
                        SalaryLockIds = "'" + item.Id + "'"; ;
                    }
                    else
                    {
                        SalaryLockIds += ",'" + item.Id + "'";

                    }
                }

                string sql = "select * from SalaryLock where Id IN (" + SalaryLockIds + @")";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");
                DataView DvMaster = new DataView(dsMaster.Tables[0]);

                dsMaster.Tables[0].DefaultView.RowFilter = "BonusDisbursementVoucherId <> '' ";
                while (dsMaster.Tables[0].DefaultView.Count > 0)
                {
                    for (int i = 0; i < EmployeeList.Count; i++)
                    {
                        if (EmployeeList[i].EmpSystemId == dsMaster.Tables[0].DefaultView[0]["EmpSystemId"].ToString() && EmployeeList[i].IsLocked == false)
                        {
                            throw new Exception("Accounting Disbursement already done for this Employee [" + EmployeeList[i].EmployeeCode + "]");
                        }
                    }
                }

                string _Id = string.Empty;
                string _masterId = string.Empty;
                string sqlDA = "SELECT * FROM [dbo].[BonusDisbursementAdvice] WHERE Id='" + DisbursementAdvice["Id"] + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sqlDA, out MasterDS, false, "1");

                if (MasterDS.Tables[0].Rows.Count == 0)
                {
                    DataRow drMS = MasterDS.Tables[0].NewRow();
                    AccountsCommonService _accountsCommonService = new AccountsCommonService(_sqlRepository);
                    _Id = _accountsCommonService.GetAutoNumber("BonusDisbursementAdvice", PKGeneratorEnum.Yearly, null, DateTime.Now);

                    drMS["Id"] = _Id;
                    drMS["FromDate"] = DisbursementAdvice["FromDate"];
                    drMS["ToDate"] = DisbursementAdvice["ToDate"];
                    drMS["Status"] = "Active";
                    drMS["Remarks"] = DisbursementAdvice["Remarks"];
                    drMS["PaymentMode"] = DisbursementAdvice["PaymentMode"];
                    drMS["AddedBy"] = identity.Name;
                    drMS["AddedDate"] = DateTime.Now;
                    drMS["AddedFromIP"] = identity.IPAddress;
                    MasterDS.Tables[0].Rows.Add(drMS);
                }

                _masterId = MasterDS.Tables[0].Rows[0]["Id"].ToString();

                foreach (var item in EmployeeList)
                {
                    DvMaster.RowFilter = "Id='" + item.Id + @"'";

                    DataRow dr = DvMaster[0].Row;
                    dr.BeginEdit();

                    dr["IsBonusDisbursed"] = true;
                    dr["BonusDisbursementAdviceId"] = _masterId;
                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = DateTime.Now;
                    dr["UpdatedFromIP"] = identity.IPAddress;

                    dr.EndEdit();

                    DvMaster.RowFilter = null;
                }
                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(MasterDS, dsMaster);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet, Authorize]
        public JsonResult GetBonusDisbursementAdviceData()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var sql = @"SELECT  [Id],  [Status], [Remarks], [PaymentMode],CONCAT(DATENAME(mm, DA.FromDate), '-', DATEPART(yy, DA.FromDate))FromDate
						 ,CONCAT(DATENAME(mm, DA.ToDate), '-', DATEPART(yy, DA.ToDate))ToDate
                         ,(SELECT SUM(spc.DisbusmentAmount)DisbursementAmount from [dbo].[SalaryLock] sl 
                            left join dbo.SalaryProcMaster spm on   spm.MonthNo=sl.MonthNo and spm.YearNo=sl.YearNo
                            left join dbo.SalaryProcChild spc on spc.SlrProcMstSystemID=spm.SystemID and sl.EmpSystemId=spc.EmpInfoSystemID
						    left join dbo.SalaryHead sh on sh.SalaryHeadID = spc.SalaryHeadID
						    WHERE sl.BonusDisbursementAdviceId=DA.Id and ISNULL(SH.HeadCategory, '')  in ('Monthly Bonus Retain','Annual Bonus Retain') and spc.DisbusmentAmount != 0)DisbursementAmount
                        FROM [dbo].[BonusDisbursementAdvice] DA WHERE DA.Status<>'Close'  ";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);

        }

        [HttpGet, Authorize]
        public JsonResult GetBonusDisbursementVoucherList(GridParameter parameters)
        {
            AccountsSalaryPayableService accountsSalaryPayableService = new AccountsSalaryPayableService(_sqlRepository);
            return Json(accountsSalaryPayableService.GetBonusDisbursementVoucherList(parameters), JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult SaveBonusDisbursementPosting(VoucherViewModel voucherVM, string fromDate, string toDate, string pMode, IEnumerable<VoucherDetailViewModel> directJVList, string disbursementAdviceId, List<SalaryLock> employeeListNew)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            voucherVM.CompanyGroupId = identity.CompanyGroupId;
            voucherVM.CompanyId = identity.CompanyId;
            voucherVM.PlantId = identity.PlantId;
            voucherVM.IsPark = true;
            voucherVM.Amount = directJVList.Sum(r => r.CrAmount);
            voucherVM.SourceType = SourceType.BonusDisbursement.ToString();

            string empSystemIds = "";
            if (employeeListNew != null)
            {
                foreach (var item in employeeListNew)
                {
                    if (empSystemIds == "")
                    {
                        empSystemIds = "'" + item.Id + "'"; ;
                    }
                    else
                    {
                        empSystemIds += ",'" + item.Id + "'";

                    }
                }
            }

            return Json(new { Message = string.Format(AplosMessage.VoucherSave, _salaryDisbursementService.SaveBonusDisbursementPosting(voucherVM, fromDate, toDate, pMode, directJVList, disbursementAdviceId, empSystemIds)) });
        }
        [HttpPost]
        public JsonResult PostBonusDisbursement(string voucherId)
        {
            _salaryDisbursementService.PostSalarydisbursement(voucherId);
            return Json(new { Message = AplosMessage.Posted });
        }
        [HttpPost]
        public ActionResult DeleteBonusDisbursementVoucher(string voucherId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            _salaryDisbursementService.DeleteBonusDisbursementVoucher(identity.PlantId, voucherId);
            return Json(new { Message = AplosMessage.Deleted });
        }

        #endregion

        #region Import File
        [HttpPost, Authorize]
        public JsonResult ImportData()
        {
            string path;
            clsTemplateReadProfile objR = null;
            try
            {
                objR = new clsTemplateReadProfile();
                var file = Request.Files["file"];
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                SaveFiles(out path);
                var data = ReadData(identity.PlantId, path);
                JsonResult json = Json(data, JsonRequestBehavior.AllowGet);
                json.MaxJsonLength = int.MaxValue;
                return json;
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        [HttpPost, Authorize]
        public JsonResult ImportBonusData()
        {
            string path;
            clsTemplateReadProfile objR = null;
            try
            {
                objR = new clsTemplateReadProfile();
                var file = Request.Files["file"];
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                SaveFiles(out path);
                var data = ReadBonusData(identity.PlantId, path);
                JsonResult json = Json(data, JsonRequestBehavior.AllowGet);
                json.MaxJsonLength = int.MaxValue;
                return json;
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }
        public List<BonusDisburseTemplate> ReadBonusData(string plantid, string path)
        {
            List<BonusDisburseTemplate> data = null;
            //string path = "";
            DataSet dsExcel = null;
            try
            {
                data = new List<BonusDisburseTemplate>();
                ReadBonusFile(path, out dsExcel);
                Validation(dsExcel, plantid);
                data = dsExcel.Tables[0].ToList<BonusDisburseTemplate>();
                return data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void ReadBonusFile(string path, out DataSet dsExcel)
        {
            FileInfo docFile;
            dsExcel = null;
            try
            {
                ExcelEngine excelEngine = null;
                IApplication application = null;
                IWorkbook workbook = null;
                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;
                workbook = excelEngine.Excel.Workbooks.Open(path);
                DataTable dt = workbook.Worksheets[0].ExportDataTable(5, 1, 5000, 28, ExcelExportDataTableOptions.ColumnNames);
                dt.DefaultView.RowFilter = "isnull(EmployeeCode,'')<>''";
                dt = dt.DefaultView.ToTable();

                dsExcel = new DataSet();
                dsExcel.Tables.Add(dt);
                docFile = new FileInfo(path);
                if (docFile.Exists)
                {
                    docFile.Delete();
                }
            }
            catch (Exception ex)
            {
                docFile = new FileInfo(path);
                if (docFile.Exists)
                {
                    docFile.Delete();
                }
                throw (ex);
            }
        }

        public void SaveFiles(out string path)
        {
            path = "";
            try
            {
                var file = Request.Files["file"];
                if (file != null)
                {
                    var extension = Path.GetExtension(file.FileName);
                    if (extension.ToLower() == ".xlsx" || extension.ToLower() == ".xls")
                    {
                    }
                    else
                        throw new CustomException(Resources.ExcelUploadError);
                }
                if (file != null)
                {
                    path = Path.Combine(ResourcesPathReader.GetAttendanceRawData(), file.FileName);
                    if (System.IO.File.Exists(path))
                    {
                        System.IO.File.Delete(path);
                        file.SaveAs(path);
                    }
                    else
                    {
                        file.SaveAs(path);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<SalaryDisburseTemplate> ReadData(string plantid, string path)
        {
            List<SalaryDisburseTemplate> data = null;
            //string path = "";
            DataSet dsExcel = null;
            try
            {
                data = new List<SalaryDisburseTemplate>();
                //SaveFile(out path);
                ReadFile(path, out dsExcel);
                Validation(dsExcel, plantid);
                data = dsExcel.Tables[0].ToList<SalaryDisburseTemplate>();
                return data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void ReadFile(string path, out DataSet dsExcel)
        {
            FileInfo docFile;
            dsExcel = null;
            try
            {
                ExcelEngine excelEngine = null;
                IApplication application = null;
                IWorkbook workbook = null;
                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;
                workbook = excelEngine.Excel.Workbooks.Open(path);
                DataTable dt = workbook.Worksheets[0].ExportDataTable(6, 1, 5000, 28, ExcelExportDataTableOptions.ColumnNames);
                dt.DefaultView.RowFilter = "isnull(EmployeeCode,'')<>''";
                dt = dt.DefaultView.ToTable();

                dsExcel = new DataSet();
                dsExcel.Tables.Add(dt);
                docFile = new FileInfo(path);
                if (docFile.Exists)
                {
                    docFile.Delete();
                }
            }
            catch (Exception ex)
            {
                docFile = new FileInfo(path);
                if (docFile.Exists)
                {
                    docFile.Delete();
                }
                throw (ex);
            }
        }

        public void Validation(DataSet dsExcel, string plantid)
        {

            try
            {

                if (dsExcel.Tables[0].Rows.Count > 0)
                {
                    if (false)
                    {
                        for (int i = 0; i < dsExcel.Tables[0].Rows.Count; i++)
                        {
                            string strTempPDate = "";
                            string strTempPTimee = "";
                            string strTempPType = "";

                            strTempPDate = dsExcel.Tables[0].Rows[i][1].ToString().Trim();
                            strTempPTimee = dsExcel.Tables[0].Rows[i][2].ToString().Trim();
                            strTempPType = dsExcel.Tables[0].Rows[i][3].ToString().Trim().ToUpper();

                        }//for

                    }

                }
                else
                {
                    throw new Exception("Please Select File");
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        #endregion

        [HttpPost, Authorize]
        public ActionResult GetPaymentAdviseReportDataXls(List<Dictionary<string, object>> data, string reportFileName)
        {
            try
            {
                if (data == null)
                {
                    throw new Exception("No Data found.");
                }
                DataTable dt = new DataTable("DD");
                foreach (string item in data[0].Keys)
                {
                    if (item.ToUpper().Contains("PK") || item.ToUpper().Contains("EJVALUE"))
                        continue;

                    dt.Columns.Add(item);
                }


                for (int i = 0; i < data.Count; i++)
                {
                    DataRow dr = dt.NewRow();
                    foreach (string item in data[i].Keys)
                    {
                        if (item.ToUpper().Contains("PK") || item.ToUpper().Contains("EJVALUE"))
                            continue;

                        dr[item] = data[i][item];
                    }

                    dt.Rows.Add(dr);
                }
                string fileName = "";
                fileName = GetPaymentAdviseReport(dt, "", reportFileName);
                return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string GetPaymentAdviseReport(DataTable data, string ReportHeader, string reportFileName)
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
                workbook.Worksheets[0].Name = "Payment Advise Data";
                sheet = workbook.Worksheets[0];

                int ROW = 6; int COL = 1;

                #region columns

                sheet[ROW, COL].Text = "EmpSystemId"; sheet[ROW, COL].ColumnWidth = 16; int colEmpId = COL; COL++;
                sheet[ROW, COL].Text = "Employee Code"; sheet[ROW, COL].ColumnWidth = 16; int colEmpCode = COL; COL++;
                sheet[ROW, COL].Text = "Employee Name"; sheet[ROW, COL].ColumnWidth = 16; int colEmpName = COL; COL++;
                sheet[ROW, COL].Text = "Designation"; sheet[ROW, COL].ColumnWidth = 16; int colDesig = COL; COL++;
                sheet[ROW, COL].Text = "Department"; sheet[ROW, COL].ColumnWidth = 16; int colDept = COL; COL++;
                sheet[ROW, COL].Text = "Division"; sheet[ROW, COL].ColumnWidth = 16; int colDiv = COL; COL++;
                sheet[ROW, COL].Text = "Employee Category"; sheet[ROW, COL].ColumnWidth = 16; int colEmpCat = COL; COL++;
                sheet[ROW, COL].Text = "Payment Mode"; sheet[ROW, COL].ColumnWidth = 16; int colPM = COL; COL++;
                sheet[ROW, COL].Text = "Bank Name"; sheet[ROW, COL].ColumnWidth = 16; int colBM = COL; COL++;
                sheet[ROW, COL].Text = "BankAccNo"; sheet[ROW, COL].ColumnWidth = 16; int colBA = COL; COL++;
                sheet[ROW, COL].Text = "IFSCCode"; sheet[ROW, COL].ColumnWidth = 16; int colIF = COL; COL++;
                sheet[ROW, COL].Text = "DisbursmentId"; sheet[ROW, COL].ColumnWidth = 16; int colDI = COL; COL++;
                sheet[ROW, COL].Text = "Disbursement Date"; sheet[ROW, COL].ColumnWidth = 16; int colDD = COL; COL++;
                sheet[ROW, COL].Text = "SalaryProcFlag"; sheet[ROW, COL].ColumnWidth = 16; int colSP = COL; COL++;
                sheet[ROW, COL].Text = "VoucherNo"; sheet[ROW, COL].ColumnWidth = 16; int colVN = COL; COL++;
                sheet[ROW, COL].Text = "Net Payment"; sheet[ROW, COL].ColumnWidth = 16; int colNP = COL;
                

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
                int LastRow = ROW + (data.Rows.Count - 1);

                for (int i = 0; i < data.Rows.Count; i++)
                {
                    sheet[ROW, colEmpId].Text = data.Rows[i]["EmpSystemId"].ToString();
                    sheet[ROW, colEmpCode].Text = data.Rows[i]["EmployeeCode"].ToString();
                    sheet[ROW, colDesig].Text = data.Rows[i]["EmployeeName"].ToString();
                    sheet[ROW, colDept].Text = data.Rows[i]["Designation"].ToString();
                    sheet[ROW, colDiv].Text = data.Rows[i]["Department"].ToString();
                    sheet[ROW, colEmpName].Text = data.Rows[i]["Division"].ToString();
                    sheet[ROW, colEmpCat].Text = data.Rows[i]["EmployeeCategory"].ToString();
                    sheet[ROW, colPM].Text = data.Rows[i]["PaymentMode"].ToString();
                    sheet[ROW, colBM].Text = data.Rows[i]["BankName"].ToString();
                    sheet[ROW, colBA].Text = data.Rows[i]["BankAccNo"].ToString();
                    sheet[ROW, colIF].Text = data.Rows[i]["IFSCCode"].ToString();
                    sheet[ROW, colDI].Text = data.Rows[i]["Id"].ToString();
                    sheet[ROW, colDD].Text = data.Rows[i]["DisbursementDate"].ToString();
                    sheet[ROW, colSP].Text = data.Rows[i]["SalaryProcFlag"].ToString();
                    sheet[ROW, colVN].Text = data.Rows[i]["VoucherNo"].ToString();
                    sheet[ROW, colNP].Number = clsStaticInfo.dbl(data.Rows[i]["NetPayment"].ToString());

                    sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                    ROW++;
                }

                sheet.AutoFilters.FilterRange = sheet.Range[startRow - 1, 1, ROW, endCol];
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[startRow, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                sheet["A" + startRow.ToString()].FreezePanes();

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ReportUtility reportUtility = new ReportUtility();
                reportUtility.PlantHeader(ref sheet, endCol, "Payment Advise Report", identity.PlantId);
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

                filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, reportFileName + ".xlsx");
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

        #endregion
    }

    public class SalaryDisburseTemplate
    {
        public string EmployeeCode { get; set; }
        public string EmployeeName { get; set; }
        //public string Designation { get; set; }
        //public string Department { get; set; }
        //public string Division { get; set; }
        //public string EmployeeCategory { get; set; }
        //public string Plant { get; set; }
        //public string Section { get; set; }
        //public string SubSection { get; set; }
        //public string Unit { get; set; }
        //public string DOJ { get; set; }
        //public string DOS { get; set; }
        //public string CurrentMonthEmployeeStatus { get; set; }
        //public string EmployeeStatus { get; set; }
        //public string SalaryProcFlag { get; set; }
        //public string PayRollGroup { get; set; }
        //public string JobLocation { get; set; }
        //public string PaymentMode { get; set; }
        //public string BankName { get; set; }
        //public string VoucherNo { get; set; }
        //public string PayableVoucherNo { get; set; }
        //public string DisbursementVoucherNo { get; set; }
        //public string IsLock { get; set; }
        //public string IsDisburse { get; set; }
        //public string NetPayment { get; set; }
        
    }
    public class BonusDisburseTemplate
    {
        public string Id { get; set; }
        public string EmployeeCode { get; set; }
        public string EmployeeName { get; set; }
        

    }
}