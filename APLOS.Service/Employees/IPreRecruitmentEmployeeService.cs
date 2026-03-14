#region Using

using Library.Core;
using Library.Model.Employees;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Employees
{
    public interface IPreRecruitmentEmployeeService : IService<PreRecruitmentEmployee>
    {
        IEnumerable<object> inactiveEmps(string col, string cGroupId, string val);
        GridModel GetCandidateDataWithAssignNonAssignDoc(GridParameter parameters, string assign, string plantId);

        GridModel GetCandidateData(GridParameter parameters, string companyGroupId, string companyId, string plantId);

        IEnumerable<object> GetDocumentDataList(string companyGroupId, string budgetId, string pId, string plantId);

        void UpdateApprove(PreRecruitmentEmployee entity);

        bool Login(string id, string pin);

        void UpdatePinAndLoginFlag(string id, string pin);

        IEnumerable<object> GetData(string empid);

        IEnumerable<object> GetJobData(string Id);

        IEnumerable<object> GetEntityByEmployee(string tableName, string fieldName, string employeeId);

        void UpdateMaster(PreRecruitmentEmployee entity);

        void UpdatePersonal(PreRecruitmentEmployee entity);

        void UpdateFinal(PreRecruitmentEmployee entity);

        void UpdateAddress(PreRecruitmentEmployee entity);

        void UpdateCandidate(PreRecruitmentEmployee entity);

        void UpdateSubmitByDepartment(IEnumerable<PreRecruitmentEmployee> entity);

        IEnumerable<object> GetDocumentData(string companyGroupId, string budgetId, string plantId, string empType, string pId);

        GridModel SearchPostOfficeName(GridParameter parameters, string sCountry, string sDistrict);

        void Insert(EmployeeInformation entity);

        GridModel Query(GridParameter parameters, string companyGroupId, string plantId);

        IEnumerable<object> CboList();

        GridModel GetPoliceStationName(GridParameter parameters, string districtId);

        GridModel SearchCountryName(GridParameter parameters);

        GridModel SearchCityName(GridParameter parameters, string countryId);

        GridModel SearchDistrictName(GridParameter parameters, string countryId);

        GridModel GetAllCandidate(GridParameter parameters, string plantId);

        GridModel GetEmployeeWithPlant(GridParameter parameters, string plantId);
        GridModel GetActiveAndInActiveEmployeeList(GridParameter parameters, string companyGroupId, string plantId, string EmployeeStatus);
    }
}