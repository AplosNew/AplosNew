#region Using

using Library.Model.Employees;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Employees
{
    public interface IPreRecruitmentEmpReferenceService : IService<PreRecruitmentEmpReference>
    {
        IEnumerable<object> GetData(string empSystemID);

        void InsertOrUpdate(PreRecruitmentEmpReference entity);
    }
}