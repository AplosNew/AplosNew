#region Using

using Library.Core;
using Library.Model.HumanResources;
using Library.Service.Core;
using Syncfusion.XlsIO;
using System.Collections.Generic;
using System.Data;
using System.Web;
using static Library.Service.HumanResources.PayRegisterBDReportService;
//using static Library.Service.HumanResources.PayrollReportsService;

#endregion Using

namespace Library.Service.HumanResources
{
    public interface IPayrollReportsService
    {
        IWorkbook GetEmployeeSalaryStructure(string companyGroupId, string companyId, string plantId, string userId, string effectiveDate, string payRollGroup, Dictionary<string, string> parameters);
        IWorkbook GetSeparatedEmployeeStructure(string companyGroupId, string companyId, string plantId, string userId, string effectiveDate, string FromDate, string ToDate, string payRollGroup, Dictionary<string, string> parameters);
        IWorkbook GetEmployeeSalaryStructurePlantWise(string companyGroupId, string companyId, string plantIdList, string userId, string effectiveDate, Dictionary<string, string> parameters);

        IWorkbook GetEmployeeSalaryStructureWithProcessed(string companyGroupId, string companyId, string plantId, string userId, string month, string year, string payRollGroup, Dictionary<string, string> parameters, bool isActive, bool isSeperated, bool isMaternity);
        //IWorkbook GetEmployeeSalaryProcessedReport(string companyGroupId, string companyId, string plantId, string userId, string month, string year, string salaryProcessId, string payRollGroup, Dictionary<string, string> parameters);
        //IWorkbook GetEmployeeSalaryProcessedReport(string companyGroupId, string companyId, string plantId, string userId, string month, string year, string salaryProcessId, string payRollGroup, Dictionary<string, string> parameters, bool isActive, bool isSeperated, bool isMaternity, bool isTopSheet);
        IWorkbook GetEmployeeSalaryProcessedOTQtyAmountReport(string companyGroupId, string companyId, string plantId, string userId, string month, string year, string salaryProcessId, string payRollGroup, Dictionary<string, string> parameters, bool isActive, bool isSeperated, bool isMaternity, bool isTopSheet, double budgetedOT);
        IWorkbook GetEmployeeSalaryProcessedReport(string companyGroupId, string companyId, string plantId, string userId, string month, string year, string salaryProcessId, string payRollGroup, Dictionary<string, string> parameters, bool isActive, bool isSeperated, bool isMaternity, bool sa, bool ca, bool isTopSheet);
        IWorkbook GetEmployeeSalaryProcessedReportSalaryLogWise(string companyGroupId, string companyId, string plantId, string userId, string month, string year, string salaryProcessId, string payRollGroup, Dictionary<string, string> parameters, bool isActive, bool isSeperated, bool isMaternity, bool isTopSheet);
        IWorkbook GetEmployeeSalaryProcessedReportSalaryLogWiseCompliance(string companyGroupId, string companyId, string plantId, string userId, string month, string year, string salaryProcessId, string payRollGroup, Dictionary<string, string> parameters, bool isActive, bool isSeperated, bool isMaternity, bool isTopSheet);
        IEnumerable<object> GetEmpInfo(string companyGroupId, string plantId, string effectiveDate, string salaryProcessId, bool sa, bool ca, string userId, bool isActive, bool isSeperated, bool isMaternity);
        IEnumerable<object> GetSeparatedEmpInfo(string companyGroupId, string plantId, string effectiveDate, string FromDate, string ToDate, string salaryProcessId, bool sa, bool ca, string userId);
        IEnumerable<ComboModel> GetPayRollGroupCbo(bool sa, bool ca, string plantId, string userId);
        IWorkbook GetEmployeePaySlip(string companyGroupId, string companyId, string plantId, string userId, string month, string year, string salaryProcessId, Dictionary<string, string> parameters, string languageId, bool isActive, bool isSeperated, bool isMaternity);
        IEnumerable<object> GetEmpInfoSalaryPorcessed(string companyGroupId, string plantId, string effectiveDate, string salaryProcessId, bool sa, bool ca, string userId, bool isActive, bool isSeperated, bool isMaternity);

        IWorkbook GetEmployeeSalaryProcessedReportSalLogWiseDirectInDirectSalaryPayable(string companyGroupId, string companyId, string plantId, string userId, string month, string year, string salaryProcessId, string payRollGroup, Dictionary<string, string> parameters, bool isActive, bool isSeperated, bool isMaternity, bool isTopSheet, bool IsDirectInDirect);

    }
}