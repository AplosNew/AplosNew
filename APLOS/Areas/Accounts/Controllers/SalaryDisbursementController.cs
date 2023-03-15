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
using Library.Service.SalaryDisbursement;
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

        public ActionResult SalaryPayable()
        {
            return View("~/Areas/Accounts/Views/SalaryDisbursement/SalaryPayable.cshtml");
        }

        
        public ActionResult SalaryPayableDisbursement()
        {
            return View("~/Areas/Accounts/Views/SalaryDisbursement/SalaryPayableDisbursement.cshtml");
        }

        [Authorize, HttpGet]
        public JsonResult GetSalaryLockDataList(string yearNo, string monthNo, string employeeId, bool isActive, bool isSeperated, bool isMaternity)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            AccountsSalaryPayableService accountsSalaryPayableService = new AccountsSalaryPayableService(_sqlRepository);
            return Json(accountsSalaryPayableService.GetSalaryLockDataList(yearNo, monthNo, employeeId, isActive, isSeperated, isMaternity, identity.PlantId), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult GetSalaryLockCTCDataList(string yearNo, string monthNo, string employeeId, bool isActive, bool isSeperated, bool isMaternity)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            AccountsSalaryPayableService accountsSalaryPayableService = new AccountsSalaryPayableService(_sqlRepository);
            return Json(accountsSalaryPayableService.GetSalaryLockCTCDataList(yearNo, monthNo, employeeId, isActive, isSeperated, isMaternity, identity.PlantId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetSalaryLockDataGLList(string yearNo, string monthNo, string employeeId, bool isActive, bool isSeperated, bool isMaternity)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            AccountsSalaryPayableService accountsSalaryPayableService = new AccountsSalaryPayableService(_sqlRepository);
            return Json(accountsSalaryPayableService.GetSalaryLockDataGLList(yearNo, monthNo, employeeId, isActive, isSeperated, isMaternity, identity.PlantId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetSalaryLockInDirectTakeAwayDataList(string yearNo, string monthNo, string employeeId, bool isActive, bool isSeperated, bool isMaternity)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            AccountsSalaryPayableService accountsSalaryPayableService = new AccountsSalaryPayableService(_sqlRepository);
            return Json(accountsSalaryPayableService.GetSalaryLockInDirectTakeAwayDataList(yearNo, monthNo, employeeId, isActive, isSeperated, isMaternity, identity.PlantId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetSalaryLockInDirectCTCDataList(string yearNo, string monthNo, string employeeId, bool isActive, bool isSeperated, bool isMaternity)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            AccountsSalaryPayableService accountsSalaryPayableService = new AccountsSalaryPayableService(_sqlRepository);
            return Json(accountsSalaryPayableService.GetSalaryLockInDirectCTCDataList(yearNo, monthNo, employeeId, isActive, isSeperated, isMaternity, identity.PlantId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetSalaryLockInDirectDataGLList(string yearNo, string monthNo, string employeeId, bool isActive, bool isSeperated, bool isMaternity)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            AccountsSalaryPayableService accountsSalaryPayableService = new AccountsSalaryPayableService(_sqlRepository);
            return Json(accountsSalaryPayableService.GetSalaryLockInDirectDataGLList(yearNo, monthNo, employeeId, isActive, isSeperated, isMaternity, identity.PlantId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetListWithCombine()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var commany = _companyRepository.Find(identity.CompanyId);
            return Json(_salaryHeadGLService.GetSalaryHeadGLCombine(commany.COAId), JsonRequestBehavior.AllowGet);
        }


        [Authorize, HttpPost]
        public JsonResult GetDirectSalaryLockSalarySheetData(string yearNo, string monthNo, bool isActive, bool isSeperated, bool isMaternity)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            AccountsSalaryPayableService accountsSalaryPayableService = new AccountsSalaryPayableService(_sqlRepository);
            return Json(accountsSalaryPayableService.GetDirectSalaryLockSalarySheetData(yearNo, monthNo, isActive, isSeperated, isMaternity, identity.PlantId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public JsonResult GetInDirectSalaryLockSalarySheetData(string yearNo, string monthNo, bool isActive, bool isSeperated, bool isMaternity)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            AccountsSalaryPayableService accountsSalaryPayableService = new AccountsSalaryPayableService(_sqlRepository);
            return Json(accountsSalaryPayableService.GetInDirectSalaryLockSalarySheetData(yearNo, monthNo, isActive, isSeperated, isMaternity, identity.PlantId), JsonRequestBehavior.AllowGet);

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
            voucherVM.IsPark = false;
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



        [Authorize, HttpGet]
        public JsonResult GetDirectSalaryPayableDisbursementDataList(string yearNo, string monthNo, string pMode, bool isActive, bool isSeperated, bool isMaternity, string bankId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            string wcEmpStatus = " AND spm.SalaryProcFlag=''";


            string empStatus = " and (1=0 ";

            if (isActive == true && isSeperated == true && isMaternity == true)
            {
                empStatus = " and (1=1 ";
            }
            else
            {
                if (isActive == true)
                {
                    empStatus += " OR case when  ISNULL(SalaryProcFlag,'Regular') ='' then 'Regular' else ISNULL(SalaryProcFlag,'Regular') end = 'Regular' ";
                }
                if (isSeperated == true)
                {
                    empStatus += " OR ISNULL(SalaryProcFlag,'Regular') ='SEPARATED'";
                }
                if (isMaternity == true)
                {
                    empStatus += " OR ISNULL(SalaryProcFlag,'Regular') ='MLV_PRE'";

                }
            }
            empStatus += ")";

            string sql = null;
            if (!string.IsNullOrEmpty(pMode))
            {
                if (bankId != null)
                {
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
						left join trn.VoucherDetail vd on vd.VoucherId=v.Id and vd.TrnNature ='Net Pay'
							LEFT JOIN HKP.GLGeneralInfo CDGL ON CDGL.Id=vd.GLGeneralInfoId
                            LEFT JOIN MST.BudgetMaster CDBM ON CDBM.Id=vd.BudgetMasterId
                            LEFT JOIN HKP.Budget CDB ON CDB.Id=CDBM.BudgetId
                            LEFT JOIN HKP.Activity CDA ON CDA.Id=vd.ActivityId
                        where sl.MonthNo = '" + monthNo + "' and sl.YearNo = '" + yearNo + @"'  AND sl.PayableVoucherId<>'' AND sl.DisbursementVoucherId IS NULL 
                        and sl.IsDisbursed=1 and spd.PaymentMode='" + pMode + @"'
                        and ISNULL(sh.SalaryHead, '')  in ('Net Pay') and spc.DisbusmentAmount != 0 
                        " + empStatus + @" and spd.BankSystemID='" + bankId + @"'
                       
                        group by sh.SalaryHead, sl.YearNo, sl.MonthNo, sh.HeadType, sh.[Sequence]
                        ,vd.GLGeneralInfoId,vd.BudgetMasterId,vd.ActivityId
                        , CDGL.AccountCode, CDGL.UserName, CDB.UserName, CDA.UserName
                        
                        )X
                        GROUP BY

                        X.GLName,X.BudgetName,X.ActivityName,X.GLGeneralInfoId,X.BudgetMasterId,X.ActivityId
                        ORDER BY 5";
                }
                else
                {
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
						left join trn.VoucherDetail vd on vd.VoucherId=v.Id and vd.TrnNature ='Net Pay'
							LEFT JOIN HKP.GLGeneralInfo CDGL ON CDGL.Id=vd.GLGeneralInfoId
                            LEFT JOIN MST.BudgetMaster CDBM ON CDBM.Id=vd.BudgetMasterId
                            LEFT JOIN HKP.Budget CDB ON CDB.Id=CDBM.BudgetId
                            LEFT JOIN HKP.Activity CDA ON CDA.Id=vd.ActivityId
                        where sl.MonthNo = '" + monthNo + "' and sl.YearNo = '" + yearNo + @"'  AND sl.PayableVoucherId<>'' AND sl.DisbursementVoucherId IS NULL 
                        and sl.IsDisbursed=1 and spd.PaymentMode='" + pMode + @"'
                        and ISNULL(sh.SalaryHead, '')  in ('Net Pay') and spc.DisbusmentAmount != 0 
                        " + empStatus + @"
                        AND sh.PartOfNetPay=1 

                        group by sh.SalaryHead, sl.YearNo, sl.MonthNo, sh.HeadType, sh.[Sequence]
                        ,vd.GLGeneralInfoId,vd.BudgetMasterId,vd.ActivityId
                        , CDGL.AccountCode, CDGL.UserName, CDB.UserName, CDA.UserName
                       
                        )X
                        GROUP BY

                        X.GLName,X.BudgetName,X.ActivityName,X.GLGeneralInfoId,X.BudgetMasterId,X.ActivityId
                        ORDER BY 5";
                }

            }
            else
            {

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
                        left join dbo.SalaryHead sh on sh.SalaryHeadID = spc.SalaryHeadID
                        left join dbo.EmployeeInformation ei on ei.SystemId = sl.EmpSystemId

                        left join MST.ManpowerBudget MPB on MPB.Id = ei.BudgetCode

                        left join ORG.Position PO on PO.Id = MPB.PositionId

                        left join trn.Voucher v on v.Id=sl.PayableVoucherId
						left join trn.VoucherDetail vd on vd.VoucherId=v.Id and vd.TrnNature ='Net Pay'
							LEFT JOIN HKP.GLGeneralInfo CDGL ON CDGL.Id=vd.GLGeneralInfoId
                            LEFT JOIN MST.BudgetMaster CDBM ON CDBM.Id=vd.BudgetMasterId
                            LEFT JOIN HKP.Budget CDB ON CDB.Id=CDBM.BudgetId
                            LEFT JOIN HKP.Activity CDA ON CDA.Id=vd.ActivityId
                        where sl.MonthNo = '" + monthNo + "' and sl.YearNo = '" + yearNo + @"'  AND sl.PayableVoucherId<>'' AND sl.DisbursementVoucherId IS NULL 
                        and sl.IsDisbursed=1
                        and ISNULL(sh.SalaryHead, '')  in ('Net Pay') and spc.DisbusmentAmount != 0 
                        " + empStatus + @"
                        --AND sh.PartOfNetPay=1 

                        group by sh.SalaryHead, sl.YearNo, sl.MonthNo, sh.HeadType, sh.[Sequence]
                        ,vd.GLGeneralInfoId,vd.BudgetMasterId,vd.ActivityId
                        , CDGL.AccountCode, CDGL.UserName, CDB.UserName, CDA.UserName
                       
                        )X
                        GROUP BY

                        X.GLName,X.BudgetName,X.ActivityName,X.GLGeneralInfoId,X.BudgetMasterId,X.ActivityId
                        ORDER BY 5";
            }
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetEmployeeDisbursementDataList(string yearNo, string monthNo, string pMode, string bankId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = null;
            if (string.IsNullOrEmpty(bankId))
            {
                sql = @" select sl.YearNo,sl.MonthNo,ei.EmployeeCode,ei.EmployeeName,d.UserName Designation,spd.PaymentMode,spd.BankAccNo
                        ,DirectManpowerCost=case when po.DirectManpowerCost=0 then 'No' when po.DirectManpowerCost=1 then 'Yes' end ,b.UserName Bank,v.VoucherNo PayableVoucherNo
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
                        LEFT JOIN [ORG].[Division] ON Division.Id = EN.DivisionId
						LEFT JOIN ORG.Line AS L ON L.Id= MPB.LineId
						left join dbo.SalaryHead sh on sh.SalaryHeadID=spc.SalaryHeadID
						left join hkp.Designation d on d.Id=spd.DesignationId
						Left outer join MST.PayrollGroupMaster PGM ON PGM.employeeid = ei.SystemId
						Left outer join HKP.PayrollGroup PG ON PG.id = PGM.PayrollGroupId
						Left Join [dbo].[JobLocation] jl on jl.SystemID = ei.JobLocationID
						left join hkp.Bank b on spd.BankSystemID=b.Id
						left join trn.Voucher v on v.Id=sl.PayableVoucherId
                        LEFT JOIN TRN.Voucher  Vl ON Vl.Id=sl.DisbursementVoucherId 
                        where sl.MonthNo='" + monthNo + "' and sl.YearNo='" + yearNo + "'  AND sl.PayableVoucherId<>'' AND sl.DisbursementVoucherId IS NULL and sl.IsDisbursed=1 and spd.PaymentMode='" + pMode + @"'
                         and spc.DisbusmentAmount!=0  
                        and spd.PlantId='" + identity.PlantId + @"' 
						 and ISNULL(sh.SalaryHead, '')  in ('Net Pay')";
            }
            else
            {
                sql = @" select sl.YearNo,sl.MonthNo,ei.EmployeeCode,ei.EmployeeName,d.UserName Designation,spd.PaymentMode,spd.BankAccNo
                        ,DirectManpowerCost=case when po.DirectManpowerCost=0 then 'No' when po.DirectManpowerCost=1 then 'Yes' end ,b.UserName Bank,v.VoucherNo PayableVoucherNo
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
                        where sl.MonthNo='" + monthNo + "' and sl.YearNo='" + yearNo + @"'  AND sl.PayableVoucherId<>'' AND sl.DisbursementVoucherId IS NULL and sl.IsDisbursed=1 
                        and spd.PaymentMode='" + pMode + "' and spd.BankSystemID='" + bankId + @"'
                         and spc.DisbusmentAmount!=0  
                        and spd.PlantId='" + identity.PlantId + @"' 
						 and ISNULL(sh.SalaryHead, '')  in ('Net Pay')";
            }

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
                                " AND BM.AccountType='" + type + "' AND ISNULL(BM.BankId,'')='" + bankId + "' OR BM.BankId<>''";
            return _sqlRepository.GetGridData(parameters);
        }

        [HttpGet, Authorize]
        public JsonResult GetSalaryPayableDisbursementVoucherList(GridParameter parameters)
        {
            AccountsSalaryPayableService accountsSalaryPayableService = new AccountsSalaryPayableService(_sqlRepository);
            return Json(accountsSalaryPayableService.GetSalaryPayableDisbursementVoucherList(parameters), JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult ParkSalaryPayableDisbursement(VoucherViewModel voucherVM, string yearNo, string monthNo, string monthName, string pMode, IEnumerable<VoucherDetailViewModel> directJVList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            voucherVM.CompanyGroupId = identity.CompanyGroupId;
            voucherVM.CompanyId = identity.CompanyId;
            voucherVM.PlantId = identity.PlantId;
            voucherVM.IsPark = false;
            int year = Int32.Parse(yearNo);
            int month = Int32.Parse(monthNo);

            int monthdays = System.DateTime.DaysInMonth(year, month);
            DateTime dt = new DateTime(year, month, 1);
            dt = dt.AddDays(monthdays - 1);
            if (voucherVM.PostingDate > dt)
                throw new CustomException("Posting Date must in the selected month of " + monthName);
            voucherVM.Amount = directJVList.Sum(r => r.CrAmount);
            voucherVM.SourceType = SourceType.SalaryDisbursement.ToString();

            return Json(new { Message = string.Format(AplosMessage.VoucherSave, _salaryDisbursementService.ParkSalaryPayableDisbursement(voucherVM, yearNo, monthNo, monthName, pMode, directJVList)) });
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
        #region Salary Disbusment ---------------------------------

        [HttpPost, Authorize]
        public ActionResult GetEmpInfo(string effectiveDate, string salaryProcessId, bool isActive, bool isSeperated, bool isMaternity)
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
                                    ,0 NetPayment 
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
                                    LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId			                                       
                                    LEFT JOIN ORG.Line AS eL ON eL.Id= mpb.LineId
                                    Left outer join MST.PayrollGroupMaster PGM ON PGM.employeeid = E.SystemId
                                    Left outer join HKP.PayrollGroup PG ON PG.id = PGM.PayrollGroupId
                                    Left Join [dbo].[JobLocation] jl on jl.SystemID = E.JobLocationID
                                    left join [HKP].[Bank] bb on bb.Id = s.BankSystemID
                                    Left join SalaryLock sl on sl.EmpSystemId=e.SystemId and sl.YearNo=YEAR('" + effectiveDate + @"') AND SL.MonthNo=Month('" + effectiveDate + @"')
                                    LEFT JOIN TRN.Voucher  V ON V.Id=sl.PayableVoucherId 
                                    LEFT JOIN TRN.Voucher  Vl ON Vl.Id=sl.DisbursementVoucherId 
                                    WHERE  s.CompanyGroupId='" + identity.CompanyGroupId + "' AND s.PlantId='" + identity.PlantId + "' and sl.islocked=1  " + wcPayrollGroup + @" 
                                    ) DD " + wcEmpStatus + @" ORDER BY EmployeeCodePreFix,EmployeeCodeNumeric";
            var empdata = _sqlRepository.GetDataCollection(sql);

            var sql2 = @"select SPC.DisbusmentAmount NetPayment, SPC.EmpInfoSystemID from SalaryProcChild SPC
left join dbo.SalaryHead SH on SH.SalaryHeadID = SPC.SalaryHeadID
JOIN SalaryProcMaster SPM ON SPM.SystemID = SPC.SlrProcMstSystemID and spm.MonthNo = Month('" + effectiveDate + @"') and spm.YearNo = Year('" + effectiveDate + @"')
Where HeadCategory='Net Payable' ";

           var empNetPay = _sqlRepository.GetDataCollection(sql2);
            // new { empdata, empNetPay }
            // _sqlRepository.GetDataCollection(sql)

            JsonResult json = Json(new { empdata, empNetPay }, JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;
            return json;
        }

        #region Salary UnDisbursed
        [HttpPost, Authorize]
        public ActionResult GetSalaryUnDisbursed(string effectiveDate, string salaryProcessId, bool isActive, bool isSeperated, bool isMaternity)
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
                                    LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId			                                       
                                    LEFT JOIN ORG.Line AS eL ON eL.Id= mpb.LineId
                                    Left outer join MST.PayrollGroupMaster PGM ON PGM.employeeid = E.SystemId
                                    Left outer join HKP.PayrollGroup PG ON PG.id = PGM.PayrollGroupId
                                    Left Join [dbo].[JobLocation] jl on jl.SystemID = E.JobLocationID
                                    left join [HKP].[Bank] bb on bb.Id = s.BankSystemID
                                    Left join SalaryLock sl on sl.EmpSystemId=e.SystemId and sl.YearNo=YEAR('" + effectiveDate + @"') AND SL.MonthNo=Month('" + effectiveDate + @"')
                                    LEFT JOIN TRN.Voucher  V ON V.Id=sl.PayableVoucherId 
                                    LEFT JOIN TRN.Voucher  Vl ON Vl.Id=sl.DisbursementVoucherId 
                                    WHERE  s.CompanyGroupId='" + identity.CompanyGroupId + "' AND s.PlantId='" + identity.PlantId + "' and sl.islocked=1 and sl.IsDisbursed = 0  " + wcPayrollGroup + @" 
                                    ) DD " + wcEmpStatus + @" ORDER BY EmployeeCodePreFix,EmployeeCodeNumeric";

            JsonResult json = Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;
            return json;
        }

        #region Report
        public void SalaryUndisbursedReportQry(string effectiveDate, string salaryProcessId, bool isActive, bool isSeperated, bool isMaternity, out DataTable data)
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
                                    LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId			                                       
                                    LEFT JOIN ORG.Line AS eL ON eL.Id= mpb.LineId
                                    Left outer join MST.PayrollGroupMaster PGM ON PGM.employeeid = E.SystemId
                                    Left outer join HKP.PayrollGroup PG ON PG.id = PGM.PayrollGroupId
                                    Left Join [dbo].[JobLocation] jl on jl.SystemID = E.JobLocationID
                                    left join [HKP].[Bank] bb on bb.Id = s.BankSystemID
                                    Left join SalaryLock sl on sl.EmpSystemId=e.SystemId and sl.YearNo=YEAR('" + effectiveDate + @"') AND SL.MonthNo=Month('" + effectiveDate + @"')
                                    LEFT JOIN TRN.Voucher  V ON V.Id=sl.PayableVoucherId 
                                    LEFT JOIN TRN.Voucher  Vl ON Vl.Id=sl.DisbursementVoucherId 
                                    WHERE  s.CompanyGroupId='" + identity.CompanyGroupId + "' AND s.PlantId='" + identity.PlantId + "' and sl.islocked=1 and sl.IsDisbursed = 0  " + wcPayrollGroup + @" 
                                    ) DD " + wcEmpStatus + @" ORDER BY EmployeeCodePreFix,EmployeeCodeNumeric";
                             data = _sqlRepository.GetDataTable(sql);
        }
        [HttpPost, Authorize]
        public ActionResult GetEmployeeSalaryUnDisbursed(string effectiveDate, string salaryProcessId, bool isActive, bool isSeperated, bool isMaternity)
        {
            try
            {

                string fileName = "";
                fileName = GeSalaryUndisburseXlsReport(effectiveDate, salaryProcessId, isActive, isSeperated, isMaternity, "Salary UnDisbursed");

                return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);


            }
            catch (Exception ex)
            {
                return Json(new { Message = ex.Message, Error = true }, JsonRequestBehavior.AllowGet);

            }
        }

        public string GeSalaryUndisburseXlsReport(string effectiveDate, string salaryProcessId, bool isActive, bool isSeperated, bool isMaternity, string SheetName)
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
                SalaryUndisbursedReportQry(effectiveDate, salaryProcessId, isActive, isSeperated, isMaternity, out data);

                int ROW = 6; int COL = 1;

                #region Columns


                //sheet[ROW, COL].Text = "Month Name";
                //sheet[ROW, COL].ColumnWidth = 16;
                //int ColMonthName = COL;
                //COL++;

                sheet[ROW, COL].Text = "Employee Code";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColEC = COL;
                COL++;

                sheet[ROW, COL].Text = "Employee Name";
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

                sheet[ROW, COL].Text = "Employee Category";
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

                sheet[ROW, COL].Text = "Sub Section";
                sheet[ROW, COL].ColumnWidth = 16;
                sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int ColSS = COL;
                COL++;

                sheet[ROW, COL].Text = "Designation";
                sheet[ROW, COL].ColumnWidth = 16;
                sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int ColDesg = COL;
                COL++;

                sheet[ROW, COL].Text = "Payable Voucher No";
                sheet[ROW, COL].ColumnWidth = 16;
                sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int ColPblVhrNo = COL;
                COL++;

                sheet[ROW, COL].Text = "Lock";
                sheet[ROW, COL].ColumnWidth = 16;
                sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int ColLock = COL;
                COL++;

                sheet[ROW, COL].Text = "Disbursement Voucher No";
                sheet[ROW, COL].ColumnWidth = 16;
                sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int ColDVNo = COL;
                COL++;

                sheet[ROW, COL].Text = "Disbursed";
                sheet[ROW, COL].ColumnWidth = 16;
                sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int ColDisbursed = COL;
                COL++;

                sheet[ROW, COL].Text = "Pay Roll Group";
                sheet[ROW, COL].ColumnWidth = 16;
                sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int ColPayRollGrp = COL;
                COL++;

                sheet[ROW, COL].Text = "Job Location";
                sheet[ROW, COL].ColumnWidth = 16;
                sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int ColJobLocation = COL;
                COL++;


                sheet[ROW, COL].Text = "Payment Mode";
                sheet[ROW, COL].ColumnWidth = 16;
                sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int ColPM = COL;
                COL++;

                sheet[ROW, COL].Text = "Bank";
                sheet[ROW, COL].ColumnWidth = 16;
                sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int ColBank = COL;
                

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

                //sheet[ROW, COL].Text = "Net Payable";
                //sheet[ROW, COL].ColumnWidth = 16;
                //sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                //int ColNetPay = COL;
                
                // COL++;
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
                    //sheet[ROW, ColNetPay].Text = data.Rows[i]["NetPayable"].ToString();


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
        #endregion Report

        #endregion Salary UnDisbursed

        [HttpPost]
        public ActionResult Save(List<SalaryLock> EmployeeList)
        {
            try
            {
                SaveSalaryLock(EmployeeList);
                return Json(new { Message = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {

                throw;
            }
        }
        public void SaveSalaryLock(List<SalaryLock> EmployeeList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
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

                foreach (var item in EmployeeList)
                {
                    DvMaster.RowFilter = "EmpSystemId='" + item.EmpSystemId + @"'";

                    if (DvMaster.Count == 0)
                    {
                        DataRow dr = dsMaster.Tables[0].NewRow();

                        string sID = string.Empty;
                        bplib.clsGenID objGenID = new bplib.clsGenID();
                        objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "SalaryLock", out sID);

                        dr["Id"] = "SL" + sID;
                        dr["EmpSystemId"] = item.EmpSystemId;
                        dr["YearNo"] = item.YearNo;
                        dr["MonthNo"] = item.MonthNo;
                        dr["IsLocked"] = item.Lock;
                        dr["IsDisbursed"] = item.CheckBoxSelect;
                        dr["PayableVoucherId"] = item.PayableVoucherId;
                        dr["DisbursementVoucherId"] = item.PayableVoucherId;

                        //dr["DisbursedAddedBy"] = identity.Name;
                        dr["AddedBy"] = identity.Name;
                        //dr["DisbursedAddedDate"] = DateTime.Now;
                        dr["AddedDate"] = DateTime.Now;
                        dr["AddedFromIP"] = identity.IPAddress;

                        dsMaster.Tables[0].Rows.Add(dr);
                    }
                    else
                    {
                        DataRow dr = DvMaster[0].Row;
                        dr.BeginEdit();

                        dr["EmpSystemId"] = item.EmpSystemId;
                        dr["YearNo"] = item.YearNo;
                        dr["MonthNo"] = item.MonthNo;
                        dr["IsLocked"] = item.Lock;
                        dr["IsDisbursed"] = item.CheckBoxSelect;
                        dr["PayableVoucherId"] = item.PayableVoucherId;
                        dr["DisbursementVoucherId"] = item.DisbursementVoucherId;

                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = DateTime.Now;
                        dr["UpdatedFromIP"] = identity.IPAddress;

                        dr.EndEdit();
                    }
                    DvMaster.RowFilter = null;
                    clsStaticInfo obj = new clsStaticInfo();
                    obj.SaveDataSets(dsMaster);
                }
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
            ConnectionManager.DAL.ConManager objCon;
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

                foreach (var item in EmployeeList)
                {
                    DvMaster.RowFilter = "EmpSystemId='" + item.EmpSystemId + @"'";

                    if (DvMaster.Count == 0)
                    {
                        DataRow dr = dsMaster.Tables[0].NewRow();

                        string sID = string.Empty;
                        bplib.clsGenID objGenID = new bplib.clsGenID();
                        objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "SalaryLock", out sID);

                        dr["Id"] = "SL" + sID;
                        dr["EmpSystemId"] = item.EmpSystemId;
                        dr["YearNo"] = item.YearNo;
                        dr["MonthNo"] = item.MonthNo;
                        dr["IsLocked"] = item.Lock;
                        dr["IsDisbursed"] = item.CheckBoxSelect;
                        dr["PayableVoucherId"] = item.PayableVoucherId;
                        dr["DisbursementVoucherId"] = item.PayableVoucherId;

                        //dr["DisbursedAddedBy"] = identity.Name;
                        dr["AddedBy"] = identity.Name;
                        //dr["DisbursedAddedDate"] = DateTime.Now;
                        dr["AddedDate"] = DateTime.Now;
                        dr["AddedFromIP"] = identity.IPAddress;

                        dsMaster.Tables[0].Rows.Add(dr);
                    }
                    else
                    {
                        DataRow dr = DvMaster[0].Row;
                        dr.BeginEdit();

                        dr["EmpSystemId"] = item.EmpSystemId;
                        dr["YearNo"] = item.YearNo;
                        dr["MonthNo"] = item.MonthNo;
                        dr["IsLocked"] = item.Lock;
                        dr["IsDisbursed"] = item.CheckBoxSelect;
                        dr["PayableVoucherId"] = item.PayableVoucherId;
                        dr["DisbursementVoucherId"] = item.DisbursementVoucherId;

                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = DateTime.Now;
                        dr["UpdatedFromIP"] = identity.IPAddress;

                        dr.EndEdit();
                    }
                    DvMaster.RowFilter = null;
                    clsStaticInfo obj = new clsStaticInfo();
                    obj.SaveDataSets(dsMaster);
                }
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

        #endregion
    }
}