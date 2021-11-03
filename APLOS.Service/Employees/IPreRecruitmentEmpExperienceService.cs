#region Using

using Library.Model.Employees;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Employees
{
    public interface IPreRecruitmentEmpExperienceService : IService<PreRecruitmentEmpExperience>
    {
        IEnumerable<object> GetData(string empSystemID);

        void InsertORUpdateMaster(PreRecruitmentEmpExperience entity);

        Dictionary<string, object> GetExperienceFile(string systemId);
    }
}