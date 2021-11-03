#region Using

using Library.Core;
using Library.Model.Machines;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Machines
{
    public interface IPersonalAllowanceService : IService<PersonalAllowance>
    {
        IEnumerable<object> GetCbo(string companyGroupId);

        GridModel Query(GridParameter parameters, string companyGroupId);
        void InsertAndUpdate(IEnumerable<PersonalAllowance> entity);
    }
}