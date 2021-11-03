using Library.Model.Employees;
using Library.Data;
using Library.Service.Employees;

using System;
using System.Web.Mvc;
using System.Linq;
using Aplos.Controllers;
using Aplos.Properties;
using Library.Crosscutting.Security;
using System.Threading;
using Library.Data.Sql;
using OTSBD;
using System.Data;
using System.Collections.Generic;
using Library.Service.Payrolls.Setting;
using static Library.Service.Payrolls.Setting.clsCurrencyRule;
using Library.HumanResource.Payroll.Setting;
using Library.HumanResource.Payroll.Tax;
using System.Reflection;
using Library.Service.Logs;
using Library.Service.Enums;
using Library.HumanResource.Payroll;

namespace Aplos.Areas.Payrolls.Controllers
{

    public class EmployeeAdvanceDeductionController : BaseController
    {

        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        private readonly IEmployeeProfileService _employeeProfileService;

        public EmployeeAdvanceDeductionController(ISqlRepository R, IEmployeeProfileService employeeProfileService)
        {
            _sqlRepository = R;
            _employeeProfileService = employeeProfileService;
        }

        #endregion Constructor

        #region View

        public ActionResult Aplos()
        {
            return View();
        }

        #endregion

        #region -- Get --

        [HttpGet, Authorize]
        public ActionResult GetSalaryHeadListeList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select SalaryHeadID as Id,SalaryHead+' ['+HeadType+']' as UserName 
                            from [dbo].[SalaryHead]  WHERE ExtDataUpload=1 and HeadCategory='Advance'
                            ORDER BY HeadType DESC,SalaryHead";
            var data = _sqlRepository.GetDataCollection(sql);
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetSalaryInterest()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select SalaryHeadID as Id,SalaryHead+' ['+HeadType+']' as UserName 
                            from [dbo].[SalaryHead]  WHERE HeadCategory='Interest Deduction'
                            ORDER BY HeadType DESC,SalaryHead";
            var data = _sqlRepository.GetDataCollection(sql);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult GetSalaryAdvance(string Year, string Month)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(GetEmployeeList(identity.PlantId, identity.CompanyId, Year, Month), JsonRequestBehavior.AllowGet);
        }
        public IEnumerable<object> GetEmployeeList(string plantId, string companyId, string Year, string Month)
        {
            try
            {
                string CmdText = @"select IsSelected = case when ead.EmployeeId is null then Convert(bit, 'False') ELSE Convert(bit, 'True') END,ars.YearNo,ars.MonthNo,a.EmployeeId,e.EmployeeCode,e.EmployeeName,
                                    a.Amount SanctionedAmount,a.Id AdvanceId,ars.Id AdvanceReqScheduleId,
                                    a.WrittenOffAmount RecoveredAmount,a.Amount-a.WrittenOffAmount Balance,
                                    ars.InstallmentAmount CurrentInstallment ,ars.PrincipalAmount,ars.ProfitAmount InterestAmount
                                    from trn.Advance a 
                                    left join trn.EmployeeSalaryAdvance esa on esa.VoucherId=a.VoucherId
                                    left join dbo.AdvanceReqSchedule ars on ars.EmployeeSalaryAdvanceId=esa.Id
                                    left join EmployeeInformation e on e.SystemId=a.EmployeeId and e.SystemId=esa.EmployeeId
                                    left join [TRN].[EmployeeAdvanceDeduction] ead on ead.EmployeeId = a.EmployeeId and ead.AdvanceId = a.Id and ead.YearNo='" + Year + @"' and ead.MonthNo='" + Month + @"'
                                    where ars.YearNo='" + Year + "' and ars.MonthNo='" + Month + "' and a.EmployeeId<>'' and a.PlantId='" + plantId + @"'
                                    and a.JournalType = 'Salary' order by a.EmployeeId";
                return _sqlRepository.GetDataCollection(CmdText);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }
        [HttpPost, Authorize]
        public ActionResult GetGeneralAdvance(string Year, string Month)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(GetEmployeeListG(identity.PlantId, identity.CompanyId, Year, Month), JsonRequestBehavior.AllowGet);
        }
        public IEnumerable<object> GetEmployeeListG(string plantId, string companyId, string Year, string Month)
        {
            try
            {
                string CmdText = @"select IsSelected = case when ead.EmployeeId is null then Convert(bit, 'False') ELSE Convert(bit, 'True') END,
                                    ars.YearNo,ars.MonthNo,a.EmployeeId,e.EmployeeCode,e.EmployeeName,
                                    a.Amount SanctionedAmount,a.Id AdvanceId,ars.Id AdvanceReqScheduleId,
                                    a.WrittenOffAmount RecoveredAmount,a.Amount-a.WrittenOffAmount Balance,
                                    ars.InstallmentAmount CurrentInstallment ,ars.PrincipalAmount,ars.ProfitAmount InterestAmount
                                    from trn.Advance a 
                                    left join trn.EmployeeSalaryAdvance esa on esa.VoucherId=a.VoucherId
                                    left join dbo.AdvanceReqSchedule ars on ars.EmployeeSalaryAdvanceId=esa.Id
                                    left join EmployeeInformation e on e.SystemId=a.EmployeeId and e.SystemId=esa.EmployeeId
                                    left join [TRN].[EmployeeAdvanceDeduction] ead on ead.EmployeeId = a.EmployeeId and ead.AdvanceId = a.Id and ead.YearNo='" + Year + @"' and ead.MonthNo='" + Month + @"'
                                    where ars.YearNo='" + Year + "' and ars.MonthNo='" + Month + "' and a.EmployeeId<>'' and a.PlantId='" + plantId + @"'
                                    and not a.JournalType = 'Salary' order by a.EmployeeId";
                return _sqlRepository.GetDataCollection(CmdText);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }
        #endregion

        #region -- Save --
        [HttpPost]
        public JsonResult SaveSalaryAdvance(List<SalaryAdvance> data, string Year, string Month, List<SalaryHeadAD> SalaryHead,string Advance, string Interest,List<SalaryAdvance> DataToBeDelete)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                clsEmpAdvanceDeduction ep = new clsEmpAdvanceDeduction();
                ep.SaveAdvance(data, Year, Month, SalaryHead,Advance,Interest, DataToBeDelete);
                return Json(new { Error = false, Data = data, Message = AplosMessage.Updated });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        #endregion
    }
}