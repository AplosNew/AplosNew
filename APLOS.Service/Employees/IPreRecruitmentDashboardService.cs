using Library.Core;
using Library.ViewModel.Organizations;
using System.Collections.Generic;

namespace Library.Service.Employees
{
    public interface IPreRecruitmentDashboardService
    {
        IEnumerable<OrgStructureListViewModel> OrgStructureList(string companyGroupId, string companyId);

        IEnumerable<object> OverAllStatus(string CompanyGroupId, string CompanyId);

        //IEnumerable<xOrgStrunctureList> xOrgStrunctureList(string CompanyGroupId, string CompanyId);

        IEnumerable<object> NotSelDoc(string CompanyGroupId, string CompanyId);

        IEnumerable<object> SelDoc(string CompanyGroupId, string CompanyId);

        IEnumerable<object> SelDocOVD(string CompanyGroupId, string CompanyId);

        IEnumerable<object> NotConfirmedDoc(string CompanyGroupId, string CompanyId);

        IEnumerable<object> LoggedInDoc(string CompanyGroupId, string CompanyId);

        IEnumerable<object> LoggedInDocOVD(string CompanyGroupId, string CompanyId);

        IEnumerable<object> NotLoggedInDoc(string CompanyGroupId, string CompanyId);

        IEnumerable<object> NotLoggedInDocOverDue(string CompanyGroupId, string CompanyId);

        GridModel ListSelTotalInterviewee(GridParameter parameters, string companyGroupId, string companyId);

        //IEnumerable<object> ListNotSelectedEmp(IEnumerable<OrgStrunctureList> OrgStructureList, string companyGroupId, string companyId);
        GridModel ListNotSelectedEmp(GridParameter parameters, string companyGroupId, string companyId);

        GridModel SubmittedButNotConfirmed(GridParameter parameters, string companyGroupId, string companyId);

        GridModel ListOverDueTotalInterviewee(GridParameter parameters, string companyGroupId, string companyId);

        GridModel ListLoggedInInterviewee(GridParameter parameters, string companyGroupId, string companyId);

        GridModel ListODLoggedInInterviewee(GridParameter parameters, string companyGroupId, string companyId);

        GridModel ListNotoggedInInterviewee(GridParameter parametes, string companyGroupId, string companyId);

        GridModel ListODNotoggedInInterviewee(GridParameter parametes, string companyGroupId, string companyId);

        IEnumerable<object> DocumentUploadingStatus(string companyGroupId, string companyId, string status);

        IEnumerable<object> EmployeeWiseDoument(string EmpId, string CompanyGroupId, string CompanyId);

        IEnumerable<object> EmployeeWiseDoumentDept(string EmpId, string CompanyGroupId, string CompanyId);

        IEnumerable<object> EmployeeWiseNotUploadedDoumentSelf(string EmpId, string CompanyGroupId, string CompanyId);

        IEnumerable<object> EmployeeWiseNotUploadedDoumentDept(string EmpId, string CompanyGroupId, string CompanyId);

        IEnumerable<object> PreDocSubmitted(string CompanyGroupId, string CompanyId);

        IEnumerable<object> PreDocNotSubmitted(string CompanyGroupId, string CompanyId);
    }
}