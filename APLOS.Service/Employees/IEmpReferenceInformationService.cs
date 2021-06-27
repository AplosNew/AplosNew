#region Using

using Library.Model.Employees;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Employees
{
    public interface IEmpReferenceInformationService : IService<EmpReferenceInformation>
    {
        void SaveList(string empid, string empidOld);

        IEnumerable<PreRecruitmentEmpReference> GetPreReferenceList(string PKs);

        IEnumerable<object> GetData(string empSystemID);

        void InsertOrUpdate(EmpReferenceInformation entity);
    }
}