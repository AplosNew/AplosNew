using Library.Core;
using Library.Model.Payrolls;
using Library.Service.Core;
using System.Collections.Generic;

namespace Library.Service.Payrolls
{
    public interface ICompanyGroupPayrollGroupService : IService<CompanyGroupPayrollGroup>
    {
        IEnumerable<object> GetCbo(string companyGroupId);

        void UpdateGraph(string payrollGroupId, bool active);

        void DeleteGraph(string payrollGroupId);

        GridModel Query(GridParameter parameters);
    }
}