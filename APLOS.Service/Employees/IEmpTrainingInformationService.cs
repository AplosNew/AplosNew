#region Using

using Library.Model.Employees;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Employees
{
    public interface IEmpTrainingInformationService : IService<EmpTrainingInformation>
    {
        void SaveList(string empid, string empidOld);

        IEnumerable<PreRecruitmentEmpTraining> GetPreRecruitmentEmpTrainingList(string PKs);

        IEnumerable<object> GetData(string empSystemID);

        void InsertORUpdateMaster(EmpTrainingInformation entity);

        Dictionary<string, object> GetTrainingFile(string systemId);
    }
}