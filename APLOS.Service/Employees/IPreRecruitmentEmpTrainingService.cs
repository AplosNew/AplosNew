#region Using

using Library.Model.Employees;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Employees
{
    public interface IPreRecruitmentEmpTrainingService : IService<PreRecruitmentEmpTraining>
    {
        IEnumerable<object> GetData(string empSystemID);

        void InsertORUpdateMaster(PreRecruitmentEmpTraining entity);

        Dictionary<string, object> GetTrainingFile(string systemId);
    }
}