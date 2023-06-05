using Library.Core;
using Library.Model.Biometrics;
using Library.Service.Core;
using Syncfusion.XlsIO;
using System.Collections.Generic;
using System.Data;

namespace Library.Service.HumanResources
{
    public interface IMaternityLeaveTransactionService : IService<LeaveTransaction>
    {
        void DeleteGraph(string id);
        void Save(LeaveTransaction entity);
        IEnumerable<object> GetFemaleEmployee(string plantId);
        IEnumerable<object> getemployeeDelete(string plantId,string CompanyId);
        IEnumerable<object> getFixedOTemployee(string YearNo, string MonthNo,string plantId,string CompanyId);
        IEnumerable<object> GetPolicyData(string EffectiveDate,string plantId);

        void CreateMaternityLeaveReportSheet(string companyId,string SystemId, string LanguageId,string plantId ,string UserName,string LeaveTransactionId, string fromDate);
        IWorkbook EmpEncashReportOld(string fromDate, string toDate, string plantId, string companyGroupId);
        IEnumerable<object>Query(string empId);

        //IEnumerable<object> GetleaveByEmpId(string empId);
        IEnumerable<object> getChildNo(string Id, string PlantId);
        IWorkbook LeaveReport(string fromDate, string toDate, string plantId, string employeeCodeString, string companyGroupId);
        IWorkbook EmpEncashReport(string year, string plantId, string companyGroupId);
        IWorkbook ShortLeaveReport(string date,  string companyGroupId, string plantId, string employeeCodeString);
        IEnumerable<object> GetClanderYear(string plantId);
        //DataTable GetEMPLeaveRegisterData(string plantId, string WorkDate, string CalanderYearId);
        //IWorkbook GetEMPLeaveRegisterReport(object companyGroupId, object companyId, string plantId, object languageId, object paymentMode, object payGroup, object bonusPointId);
        //IWorkbook GetEMPLeaveRegisterReport(object companyGroupId, string companyId, string plantId);

        //IEnumerable<ComboModel> GetBabyNoCbo(string plantId);
    }
}