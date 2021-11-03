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
    public interface IPayRegisterBDReportService
    {
        //IWorkbook EmployeeSalaryRegister(PayRegisterParamList PayRegisterParam, string paymentDate, string sqlInStatement, string sheetBasedOn, bool isActive, bool isSeperated, bool isMaternity);
        //IWorkbook EmployeeSalaryRegisterWithStructure(PayRegisterParamList PayRegisterParam, string paymentDate, string sqlInStatement, bool isActive, bool isSeperated, bool isMaternity);
        //IWorkbook EmployeeSalaryRegisterWithStructureNew(PayRegisterParamList PayRegisterParam, string paymentDate, string printDate, string sqlInStatement, bool isActive, bool isSeperated, bool isMaternity);
        //IWorkbook NewEmployeeSalaryRegisterWithStructure(string companyGroupId, string companyId, string plantId, string year, string month, string languageId, string paymentDate, string printDate, string fromDate, string toDate, string groupBy, Dictionary<string, string> parameters, string salaryProcessId, string sheetBasedOn, bool withAttendance, string paperSize, string docGrouping, bool sa, bool ca, string userId, bool isActive, bool isSeperated, bool isMaternity, bool onlyEarning);
        //IWorkbook ComEmployeeSalaryRegisterWithStructure(string companyGroupId, string companyId, string plantId, string year, string month, string languageId, string paymentDate, string printDate, string fromDate, string toDate, string groupBy, Dictionary<string, string> parameters, string salaryProcessId, string sheetBasedOn, bool withAttendance, string paperSize, string docGrouping, bool sa, bool ca, string userId, bool isActive, bool isSeperated, bool isMaternity);

        //IEnumerable<ComboModel> GetSalaryprocessIdCbo(string compnayGroupId, string companyId, string plantId, string MonthNo, string YearNo, string IsCompleteMonth);
        //IEnumerable<ComboModel> GetPayGroupCbo(bool sa, bool ca, string userId);

        //IEnumerable<object> GetEmpInfo(string companyGroupId, string companyId, string plantId, string effectiveDate, string monthNo, string YearNo, string salaryProcessId, bool sa, bool ca, string userId, bool isActive, bool isSeperated, bool isMaternity);
        //List<SalaryRegisterSorting> GetPlantWiseSalaryRegisterSortingParameters(string companyGroupId, string companyId, string plantId);
        //#region Bonus Report
        //DataTable GetBonusData(string PayRollGroupId, string BonusPointId);
        //void InsertORUpdate(IEnumerable<PlantWiseSalaryRegisterSortingParameters> entities, string companyGroupId, string companyId, string plantId);

        //#endregion

        //#region Pay Slip
        //void GetEmployeePaySliptRdlcReport(string companyGroupId, string companyId, string plantId, string year, string month, string languageId, string paymentDate, string printDate, string fromDate, string toDate, string groupBy, string user, Dictionary<string, string> parameters);
        //#endregion
    }
}