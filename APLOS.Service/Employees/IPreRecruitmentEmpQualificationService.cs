#region Using

using Library.Model.Employees;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Employees
{
    public interface IPreRecruitmentEmpQualificationService : IService<PreRecruitmentEmpQualification>
    {
        IEnumerable<object> GetData(string empSystemID);

        Dictionary<string, object> GetQualificationFile(string systemId);

        void InsertORUpdateMaster(PreRecruitmentEmpQualification entity);
    }
}