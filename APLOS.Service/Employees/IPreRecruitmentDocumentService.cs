#region Using

using Library.Core;
using Library.Model.Employees;
using Library.Service.Core;
using System;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Employees
{
    public interface IPreRecruitmentDocumentService : IService<PreRecruitmentDocument>
    {
        IEnumerable<object> GetDocumentList(string plantId, string empType, string butgedCode, string givenDesignationId);

        IEnumerable<object> GetEmpDocumentDataList(string companyGroupId, string pId, string plantId);

        void DeleteCandidateDocument(string id);

        void CreateNewDOcument(IEnumerable<PreRecruitmentDocument> entities, string empId);

        IEnumerable<object> GetDocumentDataList(string empId);

        void CreateCandidateDocument(IEnumerable<PreRecruitmentEmployee> entities);

        void DueDocumentProcess();

        void DueDocumentMailSendingProcess(string by);

        void ProcessDocumentDailyOverDue(DateTime processDate, string by, string ip);

        void InsertGraph(IEnumerable<PreRecruitmentDocument> entities, string PreRecruitmentEmployeeId);

        IEnumerable<PreRecruitmentDocument> GetDocumentFile(string id);

        Dictionary<string, object> GetDocFile(string id);

        void SaveDocumentList(string plantId, string empType, List<PreRecruitmentDocument> budgetIdList);

        IEnumerable<object> GetDocumentData(string companyGroupId, string budgetId, string plantId, string empType, string pId);

        void UpdatePreRecruitmentDocument(string id);

        GridModel GetSubmittedEmployee(GridParameter parameter, bool isControlAdmin, bool isSysAdmin, string companyGroupId, string companyId, string employeeId);

        void InsertORUpdateMaster(PreRecruitmentDocument entity);

        IEnumerable<object> GetDocumentDataList(string companyGroupId, string budgetId, string pId, string plantId);

        void InsertORUpdate(PreRecruitmentDocument entity);

        void SalaryFixationMail();
    }
}