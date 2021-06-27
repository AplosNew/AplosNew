#region Using

using Library.Core;
using Library.Model.Setups;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Setups
{
    public interface ICompanyServiceMasterService : IService<CompanyServiceMaster>
    {
        IEnumerable<object> GetCboByCompany(string companyId);

        IEnumerable<object> Query(string companyId);

        void InsertOrUpdate(IEnumerable<CompanyServiceMaster> entities, string companyId);

        void Delete(string Id);

        IEnumerable<object> GetCboService();
    }
}