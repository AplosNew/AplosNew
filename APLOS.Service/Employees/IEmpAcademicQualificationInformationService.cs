#region Using

using Library.Model.Employees;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Employees
{
    public interface IEmpAcademicQualificationInformationService : IService<EmpAcademicQualificationInformation>
    {
        void SaveList(string empid, string empidOld);

        IEnumerable<PreRecruitmentEmpQualification> GetPreRecruitmentEmpQualificationList(string PKs);

        IEnumerable<object> GetData(string empSystemID);

        void InsertORUpdateMaster(EmpAcademicQualificationInformation entity);

        Dictionary<string, object> GetQualificationFile(string systemId);
    }
}