#region Using

using Library.Core;
using Library.Model.HumanResources;
using Library.Service.Core;
using Syncfusion.XlsIO;
using System.Collections.Generic;
using System.Data;
using System.Web;
using static Library.Service.HumanResources.ManpowerAttendanceSummary;

#endregion Using

namespace Library.Service.HumanResources
{
    public interface IManpowerAttendanceSummary
    {
        string ExcelDailyDayStatus3(string PlantId, string PrevWorkDate, string companyId, string TextFromDate, string sDepID, string sSecID, string sSubSecID, string LineId, string dayStatus, string Dep, string Sec, string employeeCategory, string shift, string entity, string designationList);
        IWorkbook ExcelDailyDayStatus(string PlantId, string PrevWorkDate, string companyId, string TextFromDate, string sDepID, string sSecID, string sSubSecID, string sLineID, string dayStatus, string Dep, string Sec, string employeeCategory, string shift, string entity);
        IWorkbook ExcelDailyDayStatusReport(string PlantId, string PrevWorkDate, string companyId, string TextFromDate);
        //IWorkbook ExcelDailyDayStatus(string PlantId, string PrevWorkDate, string companyId, string TextFromDate, string sDepID, string sSecID, string sSubSecID, string sLineID, string dayStatus, string Dep, string Sec,string  employeeCategory,string shift,string entity);
        IWorkbook GetSummaryManpowerAttendanceExcel(string companyGroupId, string companyId, string workDate, bool withLine, bool withDesignation, string PlantIds, string typeLists,bool WithoutTBS,bool WithoutLA);
        IWorkbook GetSummaryManpowerAttendanceExcelNew(string companyGroupId, string companyId, string workDate, bool withLine, bool withDesignation, string PlantIds, string typeLists, bool WithoutTBS, bool WithoutLA);
        string GetSummaryManpowerAttendanceExcelNew1(string companyGroupId, string companyId, string workDate, bool withLine, bool withDesignation,DataTable dtManPBSummary);
        IWorkbook GetSummaryManpowerAttendanceExcelWithLine(string companyGroupId, string companyId, string PlantId, string workDate, bool withLine,string typeLists,bool WithoutTBS,bool WithoutLA);
        IWorkbook GetSummaryManpowerAttendanceExcelWithLineNew(string companyGroupId, string companyId, string PlantId, string workDate, bool withLine);
        IEnumerable<object> GetDailyManpowerAttendanceSummaryData(string companyGroupId, string companyId, string WorkDate, bool withLine, bool withDesignation, string plantId, string typeList, bool WithoutTBS, bool WithoutLA);
        IWorkbook GetSummaryManpowerAttendanceExcelNew(string companyGroupId, string companyId, string PlantId, string workDate, bool withLine);
        IWorkbook GetSummaryManpowerAttendanceGroupWiseExcel(string PlantId, string companyId, string workDate, string sUnitID, string sDivID, string sDepID, string sSecID, string sSubSecID);
        //IWorkbook EmployeeSalaryRegister(PayRegisterParamList PayRegisterParam);
        //IEnumerable<ComboModel> GetSalaryprocessIdCbo(string compnayGroupId, string companyId, string plantId, string MonthNo, string YearNo, string IsCompleteMonth);
        //IEnumerable<ComboModel> GetPayGroupCbo(bool sa, bool ca, string userId);
        IWorkbook GetAttendancFromAppSummaryExcel1(string PlantId, string companyId, string workDate, string sUnitID, string sDivID, string sDepID, string sSecID, string sSubSecID);
        IEnumerable<ComboModel> GetSectionCboByDepartment(string deptID);
        IEnumerable<ComboModel> GetSubSectionCboBySection(string secID);
        IEnumerable<ComboModel> GetLineCboBySubSection(string subsecID);
        IEnumerable<ComboModel> GetAttendanceDayStatus();
        IWorkbook GetCustomizedAttendanceSummaryReport(string companyGroupId, string companyId, string PlantId, string workDate);
        // object ExcelDailyDayStatus(string plantId, string prevWorkDate, string companyId, string workDate, string sDepID, string sSecID, string sSubSecID, string sLineID, string dayStatus, string dep, string sec);
    }
}