#region Using

using Library.Core;
using Syncfusion.XlsIO;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Employees
{
    public interface IDocumentDashboardService
    {
        //IEnumerable<object> PreRecStatus(string companyGroupId, string companyId, string Docby, string DocCategory, string DocName);
        //IEnumerable<object> PreDocNotSubmitted(string companyGroupId, string companyId, string Docby, string DocCategory, string DocName);
        //IEnumerable<object> PreDocSubmitted(string companyGroupId, string companyId, string Docby, string DocCategory, string DocName);
        IEnumerable<object> GetComplianceDocumentDetail(string compnayGroupId, string complianceDocumentId);

        IEnumerable<object> DailyOverDueStatus(string companyGroupId, string DocumentCategoryId, string DocumentSubCategoryId, string EmplyeeTypeOrCategoryId, string ComplianceDocumentId, string DocumentationBy, string ResponsiblePersonId, string Importance, string OptionalOrMandatory, string DocumentType);

        IEnumerable<object> PendingDocuments(string companyGroupId, string DocumentCategoryId, string DocumentSubCategoryId, string EmplyeeTypeOrCategoryId, string ComplianceDocumentId, string DocumentationBy, string ResponsiblePersonId, string Importance, string OptionalOrMandatory, string DocumentType);

        //GridModel GetCbo(string compnayGroupId);

        GridModel PreEmp(GridParameter parameter, string employmentstage, string DocumentCategoryId, string DocumentSubCategoryId, string EmplyeeTypeOrCategoryId, string ComplianceDocumentId, string DocumentationBy, string ResponsiblePersonId, string Importance, string OptionalOrMandatory, string DocumentType);

        GridModel PreEmp1(GridParameter parameter, string employmentstage, string DocumentCategoryId, string DocumentSubCategoryId, string EmplyeeTypeOrCategoryId, string ComplianceDocumentId, string DocumentationBy, string ResponsiblePersonId, string Importance, string OptionalOrMandatory, string DocumentType);

        GridModel PreEmp2(GridParameter parameter, string employmentstage, string DocumentCategoryId, string DocumentSubCategoryId, string EmplyeeTypeOrCategoryId, string ComplianceDocumentId, string DocumentationBy, string ResponsiblePersonId, string Importance, string OptionalOrMandatory, string DocumentType);

        GridModel PreEmp3(GridParameter parameter, string employmentstage, string DocumentCategoryId, string DocumentSubCategoryId, string EmplyeeTypeOrCategoryId, string ComplianceDocumentId, string DocumentationBy, string ResponsiblePersonId, string Importance, string OptionalOrMandatory, string DocumentType);

        IEnumerable<object> Doc(string employmentstage, string DocumentCategoryId, string DocumentSubCategoryId, string EmplyeeTypeOrCategoryId, string ComplianceDocumentId, string DocumentationBy, string ResponsiblePersonId, string Importance, string OptionalOrMandatory, string DocumentType);

        IEnumerable<object> Doc1(string employmentstage, string DocumentCategoryId, string DocumentSubCategoryId, string EmplyeeTypeOrCategoryId, string ComplianceDocumentId, string DocumentationBy, string ResponsiblePersonId, string Importance, string OptionalOrMandatory, string DocumentType);

        IEnumerable<object> Doc2(string employmentstage, string DocumentCategoryId, string DocumentSubCategoryId, string EmplyeeTypeOrCategoryId, string ComplianceDocumentId, string DocumentationBy, string ResponsiblePersonId, string Importance, string OptionalOrMandatory, string DocumentType);

        IEnumerable<object> Doc3(string employmentstage, string DocumentCategoryId, string DocumentSubCategoryId, string EmplyeeTypeOrCategoryId, string ComplianceDocumentId, string DocumentationBy, string ResponsiblePersonId, string Importance, string OptionalOrMandatory, string DocumentType);

        IEnumerable<object> EmpWiseDocOpt(string employmentStage, string segment, string preRecEmployeeId, string employeeId, string companyGroupId, string DocumentCategoryId, string DocumentSubCategoryId, string ComplianceDocumentId, string DocumentationBy, string ResponsiblePersonId, string Importance, string OptionalOrMandatory, string DocumentType);

        IEnumerable<object> EmpWiseDocMandt(string employmentStage, string segment, string preRecEmployeeId, string employeeId, string companyGroupId, string DocumentCategoryId, string DocumentSubCategoryId, string ComplianceDocumentId, string DocumentationBy, string ResponsiblePersonId, string Importance, string OptionalOrMandatory, string DocumentType);

        GridModel DocWiseEmp(GridParameter parameter, string employmentStage, string segment, string CompDocumentId, string EmplyeeTypeOrCategoryId, string companyGroupId, string DocumentCategoryId, string DocumentSubCategoryId, string ComplianceDocumentId, string DocumentationBy, string ResponsiblePersonId, string Importance, string OptionalOrMandatory, string DocumentType);

        GridModel CompletdDocWiseEmp(GridParameter parameter, string CompDocumentId, string companyGroupId, string DocumentCategoryId, string DocumentSubCategoryId, string EmplyeeTypeOrCategoryId, string ComplianceDocumentId, string DocumentationBy, string ResponsiblePersonId, string Importance, string OptionalOrMandatory, string DocumentType);

        GridModel OthersDocWiseEmp(GridParameter parameter, string CompDocumentId, string companyGroupId, string DocumentCategoryId, string DocumentSubCategoryId, string EmplyeeTypeOrCategoryId, string ComplianceDocumentId, string DocumentationBy, string ResponsiblePersonId, string Importance, string OptionalOrMandatory, string DocumentType);

        GridModel OverDueWiseEmp(GridParameter parameter, string CompDocumentId, string companyGroupId, string DocumentCategoryId, string DocumentSubCategoryId, string EmplyeeTypeOrCategoryId, string ComplianceDocumentId, string DocumentationBy, string ResponsiblePersonId, string Importance, string OptionalOrMandatory, string DocumentType);

        GridModel DueWiseEmp(GridParameter parameter, string CompDocumentId, string companyGroupId, string DocumentCategoryId, string DocumentSubCategoryId, string EmplyeeTypeOrCategoryId, string ComplianceDocumentId, string DocumentationBy, string ResponsiblePersonId, string Importance, string OptionalOrMandatory, string DocumentType);

        IEnumerable<object> PieCompletedDoc(string companyGroupId, string DocumentCategoryId, string DocumentSubCategoryId, string EmplyeeTypeOrCategoryId, string ComplianceDocumentId, string DocumentationBy, string ResponsiblePersonId, string Importance, string OptionalOrMandatory, string DocumentType);

        IEnumerable<object> PieOthersDoc(string companyGroupId, string DocumentCategoryId, string DocumentSubCategoryId, string EmplyeeTypeOrCategoryId, string ComplianceDocumentId, string DocumentationBy, string ResponsiblePersonId, string Importance, string OptionalOrMandatory, string DocumentType);

        IEnumerable<object> PieDueDoc(string companyGroupId, string DocumentCategoryId, string DocumentSubCategoryId, string EmplyeeTypeOrCategoryId, string ComplianceDocumentId, string DocumentationBy, string ResponsiblePersonId, string Importance, string OptionalOrMandatory, string DocumentType);

        IEnumerable<object> PieOverDueDoc(string companyGroupId, string DocumentCategoryId, string DocumentSubCategoryId, string EmplyeeTypeOrCategoryId, string ComplianceDocumentId, string DocumentationBy, string ResponsiblePersonId, string Importance, string OptionalOrMandatory, string DocumentType);

        IEnumerable<object> OverDueStatus(string DocumentCategoryId, string DocumentSubCategoryId, string EmplyeeTypeOrCategoryId, string ComplianceDocumentId, string DocumentationBy, string ResponsiblePersonId, string Importance, string OptionalOrMandatory, string DocumentationType);

        IEnumerable<ComboModel> GetComplianceDocumentCbo(string compnayGroupId, string ComplianceDocumentCategoryId, string ComplianceDocumentSubCategoryId);

        IEnumerable<ComboModel> GetCascadingComplianceDocumentCategoryCbo(string compnayGroupId);

        IEnumerable<ComboModel> GetCascadingComplianceDocumentSubCategoryCbo(string compnayGroupId, string documentCategoryId);

        GridModel GetResponsiblePersonCbo(string compnayGroupId);

        IEnumerable<object> PieChart(string companyGroupId, string DocumentCategoryId, string DocumentSubCategoryId, string EmplyeeTypeOrCategoryId, string ComplianceDocumentId, string DocumentationBy, string ResponsiblePersonId, string Importance, string OptionalOrMandatory, string DocumentationType);

        //---------------------------------------Report-------------------------------------//
        string GetEmployeeDueDocumentList(string employeeId, string companyGroupId);

        IWorkbook GetEmployeeDocumentReport();
    }
}