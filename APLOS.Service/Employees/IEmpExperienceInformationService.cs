#region Using

using Library.Model.Employees;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Employees
{
    public interface IEmpExperienceInformationService : IService<EmpExperienceInformation>
    {
        void SaveList(string empid, string empidOld);

        IEnumerable<PreRecruitmentEmpExperience> GetPreRecruitmentEmpExperienceList(string PKs);

        IEnumerable<object> GetData(string empSystemID);

        Dictionary<string, object> GetExperienceFile(string systemId);

        void InsertORUpdateMaster(EmpExperienceInformation entity);
    }
}