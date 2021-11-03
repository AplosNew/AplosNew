#region Using

using Library.Core;
using Library.Model.Payrolls;
using Library.Model.Setups;
using Library.Service.Core;
using Library.ViewModel.HR;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Setups
{
    public interface ISalaryFixationService : IService<SalaryFixation>
    {
        IEnumerable<object> GetTermsAndConditionsByEmployee(string preRecruitmentEmployeeid);

        IEnumerable<object> GetTermsAndConditionsByPlant(string plantId);

        void InsertOrUpdateGraph(IEnumerable<SalaryFixation> entities, string companyGroupId, string plantid);

        void InsertOrUpdateGraphFromFixation(IEnumerable<SalaryFixation> entities, string companyGroupId, string plantid, EmployeeWiseTermsAndConditions employeeWiseTermsAndConditions, bool IsMail);

        GridModel GetEmployees(GridParameter parameters, string companyGroupId, string companyId);

        IEnumerable<object> GetSalaryHeadList(string preRecEmpId);

        IEnumerable<object> GetCbo(string companyGroupId);

        IEnumerable<object> GetSalaryHeadDataList();

        IEnumerable<object> GetGDAndEmpWiseSalaryHeadList(string preRecruitmentEmployeeId, string givenDesignationId);

        IEnumerable<object> GetCalculationInfo(string preRecruitmentEmployeeId, string givenDesignationId, string plantId);

        void GetCalculationInfoFinal(IEnumerable<SalaryFixation> entities, string TotalSalary, string preRecruitmentEmployeeId, string givenDesignationId, string plantId, out IEnumerable<SalaryFixationVM> list);

        //void GetCalculationInfoFinal(IEnumerable<SalaryFixation> entities, string preRecruitmentEmployeeId, string givenDesignationId, out IEnumerable<SalaryFixationVM> list);
        IEnumerable<SalaryFixationVM> GetHeadList(string preRecruitmentEmployeeId, string givenDesignationId, string plantId);
    }
}