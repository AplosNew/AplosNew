#region Using

using Library.Core;
using Library.Model.HumanResources;
using Library.Service.Core;
using Syncfusion.XlsIO;
using System.Collections.Generic;
using System.Data;
using System.Web;
using static Library.Service.HumanResources.PayRegisterBDReportService;

#endregion Using

namespace Library.Service.HumanResources
{
    public interface IAttendanceManagementService 
    {
        void EmployeeSattlementReport(string empSystemId, string PlantId);
        IWorkbook GetShiftReport(string companyGroupId, string companyId, string plantId, string plantName, string employeeId, string fromDate, string toDate ,string EmpDoj);

        IWorkbook GetJobCardReport(string username, string companyGroupId, string companyId, string plantId, string plantName, string employeeId, string fromDate, string toDate, bool chkAdditionInfo);
        IWorkbook GetComplianceJobCardReport(string username, string companyGroupId, string companyId, string plantId, string plantName, string EmpIdLoop, string fromDate, string toDate, bool chkAdditionInfo);
        IWorkbook GetMaintenanceJobCardReports(string username, string companyGroupId, string companyId, string plantId, string plantName, string PlannedId);
       
        IWorkbook GetIssueControlJobCardReports(string username, string companyGroupId, string companyId, string plantId, string plantName,string Shift, string IssueId);
        IWorkbook GetNationalFestivalReport(string CalanderYearId,string username, string companyGroupId, string companyId, string plantId, string plantName, string EmpIdLoop, string fromDate, string toDate);
        IWorkbook GetManualOutTimeDateWiseReport(string username, string plantId, string companyId, string companyGroupId,  string plantName, string FromDate, string ToDate);
        IWorkbook GetLateAttendancePostingReport(string username, string plantId, string companyId, string companyGroupId,  string plantName, string EffectiveDate);
        IWorkbook GetPreAllocatedReport(string username, string plantId, string companyId, string companyGroupId,  string plantName, string WorkDate);

        IWorkbook GetAttendanceRawDataReport(string username, string plantId, string companyId, string companyGroupId, string plantName, string WorkDate);

        IWorkbook GetActualOTAndPlanReport(string username, string plantId, string companyId, string companyGroupId,  string plantName, string WorkDate);
        IWorkbook GetTiffinBillFinalReport(string username, string plantId, string companyId, string companyGroupId,  string plantName, string WorkDate, string DailyAllowance,string ReportName);
        IWorkbook GetTiffinBillFinalSummaryReport(string username, string plantId, string companyId, string companyGroupId,  string plantName, string FromDate, string ToDate, string DailyAllowance,string ReportName);

        IWorkbook GetleavesChecklistReport(string username, string plantId, string companyId, string companyGroupId, string plantName, string FromDate, string ToDate);
        IWorkbook GetAttendanceSummaryStatusReport(string username, string plantId, string companyId, string companyGroupId, string plantName, string FromDate, string ToDate);
        IWorkbook GetWorkerLateStatusReport(string username, string plantId, string companyId, string companyGroupId, string plantName, string WorkDate, string EntityId, string EntityUserName);

        //IWorkbook EmployeeSattlementReport(string companyGroupId, string companyId, string plantId, string SystemId, string LanguageId, string UserName);

        void EmployeeSattlementReport(string companyGroupId, string companyId, string plantId, string SystemId, string LanguageId, string UserName);

        void GetSalaryCertificateReport(string companyGroupId, string companyId, string plantId, string SystemId,string EffectiveDate, string LanguageId, string UserName);
        IWorkbook GetOtFinalReport(string companyGroupId, string companyId, string plantId, string EmployeeId, string plantName, string year, string month);
        IWorkbook GetOtFinalReport2(string companyGroupId, string companyId, string plantId,string EmployeeId, string plantName, string year, string month);
        IWorkbook GetHourlyOffDutyTag(string Name , string CompanyGroupId, string PlantId, string CompanyId, string PlantName, string WorkDate);
        IWorkbook GetSampleFile(string Name , string CompanyGroupId, string PlantId, string CompanyId, string PlantName);
        IWorkbook GetHourlyOT(string Name , string CompanyGroupId, string PlantId, string CompanyId, string PlantName, string FromDate,string ToDate);
        IWorkbook GetHourlyOTMonthly(string Name , string CompanyGroupId, string PlantId, string CompanyId, string PlantName, string YearNo, string MonthNo,bool isActive, bool isSeperated);
        IWorkbook GetIndividualDailyOT(string Name , string CompanyGroupId, string PlantId, string CompanyId, string PlantName, string FromDate, string ToDate,string OTDuration, bool CheckBox,string OTfinal, string filePath);
        IWorkbook GetHourlyLeave(string Name , string CompanyGroupId, string PlantId, string CompanyId, string PlantName, string FromDate,string ToDate);

        IEnumerable<object> GetEmpInfo(string companyGroupId, string plantId, string fromDate, string toDate,string criteria);
        IEnumerable<object> GetLeaveEmpInfo(string companyGroupId, string plantId, string fromDate, string toDate, string criteria);
        IEnumerable<object> GetMaternityEmpInfo(string companyGroupId, string plantId,string criteria);

        IEnumerable<object> GetFinalSattlementInformation(string companyGroupId, string plantId, string criteria);
        IEnumerable<object> GetSalaryCertificateInformation(string companyGroupId, string plantId, string criteria ,string EffectiveDate);


        IWorkbook GetTiffinBillReport(string plantId, string FromDate, string ToDate, string ShiftId, string Hr, string Min);
        IWorkbook GetSkillManagementJobCardReports(string name, string companyGroupId, string companyId, string plantId, string plantName, string plannedId);
        IWorkbook GetProductionJobCardReports(string name, string companyGroupId, string companyId, string plantId, string plantName, string workCenterId);
        IWorkbook GetRunningMachineJobCardReports(string name, string companyGroupId, string companyId, string plantId, string plantName, string entityId, string processId, string targetDate, string shiftId);
        IWorkbook GetProductionBookingJobCardReports(string name, string companyGroupId, string companyId, string plantId, string plantName, string entityId, string processId, string productionDate, string shiftId);
        IWorkbook GetProductionBookingJobCardLatestReports(string name, string companyGroupId, string companyId, string plantId, string plantName, string entityId, string processId, string shiftId);
        IWorkbook GetPOIssueJobCardReports(string name, string companyGroupId, string companyId, string plantId, string plantName, string PlannedId, string IssueId);
        IWorkbook GetGeneralIssueJobCardReports(string name, string companyGroupId, string companyId, string plantId, string plantName, string plannedId);
        IWorkbook GetUpdateIssueJobCardReports(string name, string companyGroupId, string companyId, string plantId, string plantName, string plannedId);
    }
}