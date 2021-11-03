#region Using

using Library.Core;
using Library.ViewModel.Accounts;
using Library.ViewModel.Organizations;
using Syncfusion.XlsIO;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Employees
{
    public interface IHRDashboardService
    {
        IEnumerable<ComboModel> GetReportingPersonCbo(string compnayGroupId, string companyId, string plantId);

        IEnumerable<OrgStructureListViewModel> OrgStructureList(string CompanyGroupId);

        IEnumerable<object> OrgStructureListColList(string CompanyGroupId, string CompanyId);

        IEnumerable<object> HROverAllStatusDefault(string companyGroupId, string hrDate, string EmplyeeTypeOrCategoryId);

        IEnumerable<object> HROverAllStatusDynamic(IEnumerable<ChartColumnList> ChartColumnList, int seq, string companyGroupId, string hrDate, string EmplyeeTypeOrCategoryId);
        IEnumerable<object> HRLongAbsentismDefault(string companyGroupId, string hrDate, string EmplyeeTypeOrCategoryId);

        IEnumerable<object> ConsecutiveLateStatsDynamic(IEnumerable<ChartColumnList> ChartColumnList, int seq, string companyGroupId, string hrDate, string EmplyeeTypeOrCategoryId);

        IEnumerable<object> ConsecutiveAbsentStatsDynamic(IEnumerable<ChartColumnList> ChartColumnList, int seq, string companyGroupId, string hrDate, string EmplyeeTypeOrCategoryId);

        IEnumerable<object> JoiningStatusDaily(string companyGroupId, string companyId, string hrDate, string EmplyeeTypeOrCategoryId);

        IEnumerable<object> DynamicJoiningOrSeparationStatusDaily(IEnumerable<ChartColumnList> ChartColumnList, int seq, string companyGroupId, string hrDate, string EmplyeeTypeOrCategoryId);

        IEnumerable<object> AbsentismStatusDaily(string companyGroupId, string companyId, string hrDate, string EmplyeeTypeOrCategoryId);

        IEnumerable<object> DynamicAbsentismStatusDaily(IEnumerable<ChartColumnList> ChartColumnList, int seq, string companyGroupId, string hrDate, string EmplyeeTypeOrCategoryId);
        //IEnumerable<object> GruopWiseAttnStatus(string companyGroupId, string companyId, string hrDate);

        //List<OrgStructureListViewModel> GetRelationChain(string companyGroupId, string companyId, string hrDate);

        //GridModel ListProbationOverDue(GridParameter parameter, string condition, string companyGroupId, string plantId);

        IEnumerable<object> ListSeparationStatus(IEnumerable<ChartColumnList> ChartColumnList, int seq, string condition, string companyGroupId, string hrDate, string EmplyeeTypeOrCategoryId);

        //IEnumerable<object> LeaveStatus(string companyGroupId, string hrDate);
        IEnumerable<object> LeaveStatus(string companyGroupId, string hrDate, string EmplyeeTypeOrCategoryId);

        IEnumerable<object> DynamicLeaveStatus(IEnumerable<ChartColumnList> ChartColumnList, int seq, string companyGroupId, string hrDate, string EmplyeeTypeOrCategoryId);
        IEnumerable<object> ModalOnRoleEmployeeList(IEnumerable<ChartColumnList> chartColumnList, string companyGroupId, int seq, string hrDate, string EmplyeeTypeOrCategoryId);
        IEnumerable<object> ModalHRDailyPresentStatusList(IEnumerable<ChartColumnList> chartColumnList, string companyGroupId, int seq, string hrDate, string EmplyeeTypeOrCategoryId);
        IEnumerable<object> ModalHRDailyAbsentStatusList(IEnumerable<ChartColumnList> chartColumnList, string companyGroupId, int seq, string hrDate, string EmplyeeTypeOrCategoryId);
        IEnumerable<object> ModalHRDailyLeaveStatusList(IEnumerable<ChartColumnList> chartColumnList, string companyGroupId, int seq, string hrDate, string EmplyeeTypeOrCategoryId);
        IEnumerable<object> ModalHRDailyLateStatusList(IEnumerable<ChartColumnList> chartColumnList, string companyGroupId, int seq, string hrDate, string EmplyeeTypeOrCategoryId);
        IEnumerable<object> ModalHRDailyShiftNotAssignedStatusList(IEnumerable<ChartColumnList> chartColumnList, string companyGroupId, int seq, string hrDate, string EmplyeeTypeOrCategoryId);
        IEnumerable<object> ModalLongAbsenteismStatusList(IEnumerable<ChartColumnList> chartColumnList, string companyGroupId, int seq, string hrDate, string EmplyeeTypeOrCategoryId);
        #region DynamicDashboard

        IEnumerable<object> DefaultAttnStatus(string companyGroupId, string hrDate, string EmplyeeTypeOrCategoryId);

        IEnumerable<object> DrillDownAttnStatus(IEnumerable<ChartColumnList> ChartColumnList, int seq, string companyGroupId, string hrDate, string EmplyeeTypeOrCategoryId);

        #endregion DynamicDashboard

        #region Modal
        //GridModel ModalHRDailyLateStatusList(IEnumerable<ChartColumnList> chartColumnList, string companyGroupId, int seq, string hrDate, string EmplyeeTypeOrCategoryId, GridParameter parameters);

        // GridModel ModalHRDailyShiftNotAssignedStatusList(IEnumerable<ChartColumnList> chartColumnList, string companyGroupId, int seq, string hrDate, string EmplyeeTypeOrCategoryId, GridParameter parameters);

        IEnumerable<object> ModalHRDailyAttdnNotProcessedStatusList(IEnumerable<ChartColumnList> chartColumnList, string companyGroupId, int seq, string hrDate, string EmplyeeTypeOrCategoryId);

        IEnumerable<object> ModalHRDailyOffDayStatusList(IEnumerable<ChartColumnList> chartColumnList, string companyGroupId, int seq, string hrDate, string EmplyeeTypeOrCategoryId);

        IEnumerable<object> ModalOthersDetail(IEnumerable<ChartColumnList> chartColumnList, string companyGroupId, int seq, string hrDate, string EmplyeeTypeOrCategoryId);

        IEnumerable<object> ConsecutiveAbsentStats(string hrDate, string EmplyeeTypeOrCategoryId);

        IEnumerable<object> ConsecutiveLateStats(string hrDate, string EmplyeeTypeOrCategoryId);

        IEnumerable<object> ModalConsecutiveLateStats(IEnumerable<ChartColumnList> ChartColumnList, int seq, string companyGroupId, string hrDate, string EmplyeeTypeOrCategoryId);

        IEnumerable<object> ModalConsecutiveAbsentStats(IEnumerable<ChartColumnList> ChartColumnList, int seq, string companyGroupId, string hrDate, string EmplyeeTypeOrCategoryId);

        IEnumerable<object> ListOfIncrementDue(IEnumerable<ChartColumnList> ChartColumnList, int seq, string condition, string companyGroupId, string hrDate, string EmplyeeTypeOrCategoryId);

        IEnumerable<object> ListProbationOverDue(IEnumerable<ChartColumnList> ChartColumnList, int seq, string condition, string companyGroupId, string hrDate, string EmplyeeTypeOrCategoryId);

        #endregion Modal

        #region 2nd Part

        GridModel DateWiseAbsentList(string companyGroupId, string companyId, string plantId, string hrFromDate, string hrToDate, string dayCount, string comparator, GridParameter parameters);

        IEnumerable<object> ConsecutiveDateWiseAbsentList(string companyGroupId, string companyId, string plantId, string hrFromDate, string hrToDate, string dayCount);

        GridModel DateWiseLatetListStatus(string companyGroupId, string companyId, string plantId, string hrFromDate, string hrToDate, string dayCount, string comparator, GridParameter parameters);

        GridModel DateJoiningStatus(string companyGroupId, string companyId, string plantId, string hrFromDate, string hrToDate, string dayCount, string comparator, GridParameter parameters);
     
        IWorkbook DateWiseJoiningListInExcel(string companyGroupId, string companyId, string plantId, string hrFromDate, string hrToDate, string dayCount, string comparator);//For excel Report

        GridModel DateSepartaionStatus(string companyGroupId, string companyId, string plantId, string hrFromDate, string hrToDate, string dayCount, string comparator, GridParameter parameters);

        IEnumerable<object> ROPersonWiseAttnStatus(string companyGroupId, string companyId, string plantId, string hrFromDate, string hrToDate, string reportingPersonId);

        GridModel ROPersonWisePresentStatusList(string companyGroupId, string companyId, string plantId, string hrFromDate, string hrToDate, string reportingPersonId, string EmplyeeTypeOrCategoryId, GridParameter parameters);

        GridModel ROPersonWiseAbsentStatusList(string companyGroupId, string companyId, string plantId, string hrFromDate, string hrToDate, string reportingPersonId, string EmplyeeTypeOrCategoryId, GridParameter parameters);

        GridModel ROPersonWiseLatetStatusList(string companyGroupId, string companyId, string plantId, string hrFromDate, string hrToDate, string reportingPersonId, string EmplyeeTypeOrCategoryId, GridParameter parameters);

        GridModel ROPersonWiseLeavetStatusList(string companyGroupId, string companyId, string plantId, string hrFromDate, string hrToDate, string reportingPersonId, string EmplyeeTypeOrCategoryId, GridParameter parameters);

        GridModel ROPersonWiseWeekOffHolidayStatusList(string companyGroupId, string companyId, string plantId, string hrFromDate, string hrToDate, string reportingPersonId, string EmplyeeTypeOrCategoryId, GridParameter parameters);

        GridModel ModalROPHRDailyShiftNotAssignedStatusList(string companyGroupId, string companyId, string plantId, string hrFromDate, string hrToDate, string reportingPersonId, string EmplyeeTypeOrCategoryId, GridParameter parameters);

        GridModel ModalROHRDailyAttdnNotProcessedStatusList(string companyGroupId, string companyId, string plantId, string hrFromDate, string hrToDate, string reportingPersonId, string EmplyeeTypeOrCategoryId, GridParameter parameters);

        IEnumerable<object> ModalEmployeeWiseAbsentDateList(string companyGroupId, string companyId, string plantId, string hrFromDate, string hrToDate, string employeeCode, string dayCount, string comparator);

        IEnumerable<object> ModalEmployeeWiseLateDateList(string companyGroupId, string companyId, string plantId, string hrFromDate, string hrToDate, string employeeCode, string dayCount, string comparator);

        IEnumerable<object> ModalConsecutiveAbsentDateList(string companyGroupId, string companyId, string plantId, string empSystemID, string hrDate);

        IEnumerable<object> ModalConsecutiveLateDateList(string companyGroupId, string companyId, string plantId, string empSystemID, string hrDate);

        #endregion 2nd Part


        #region Excel Reports
        IEnumerable<ChartColumnList> GetChartColumnList(IEnumerable<ChartColumnList> chartColumnList);
        IWorkbook HRDailyAbsentStatusListForExcel(IEnumerable<ChartColumnList> chartColumnList, string CompanyGroupId, int seq, string hrDate, string EmplyeeTypeOrCategoryId);
        IWorkbook DateWiseAbsentListInExcel(string companyGroupId, string companyId, string plantId, string hrFromDate, string hrToDate, string dayCount, string comparator);
        #endregion
    }
}