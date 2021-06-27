#region Using

using Library.Model.Employees;
using Library.Service.Core;
using System;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Employees
{
    public interface IEmployeeDocumentService : IService<EmployeeDocument>
    {
        void CreateNewDOcument(IEnumerable<EmployeeDocument> entities, string empId);

        IEnumerable<object> GetDocumentList(string plantId, string empType, string budgetCode, string givenDesignationId);

        void DeleteEmployeeDocument(string id);

        void DueDocumentProcess();

        void DueDocumentMailSendingProcess(string by);

        void ProcessDocumentDailyOverDue(DateTime processDate, string by, string ip);

        void SaveList(string empid, string empidOld);

        IEnumerable<PreRecruitmentDocument> GetPreRecruitmentDocumentList(string PKs);

        void InitPostDocument(IEnumerable<EmployeeInformation> empList, string plantId, string empType);

        void EmployeeBirthdayWish(string by);

        void EmployeeBirthDateList(string addedBy, string ip, string appVersion);

        Dictionary<string, object> GetDocFile(string id);

        void UpdateEmployeeDocument(string id);

        void InsertORUpdateMaster(EmployeeDocument entity);
    }
}