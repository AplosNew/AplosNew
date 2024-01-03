#region Using

using Library.Core;
using Library.Model.Employees;
using Library.Service.Core;
using Library.ViewModel.Setup;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Employees
{
    public interface IRecruitmentSelectionService : IService<PreRecruitmentEmployee>
    {
        IEnumerable<object> GetCbo(string companyGroupId);
        GridModel GetAppData(GridParameter parameters, string plantId, string fd, string td);

        GridModel GetData(GridParameter parameters, string plantId);

        void GetPKList(IEnumerable<PreRecruitmentEmployee> list, out string masterid);

        void InsertORUpdateMaster(EmailSetup emailSetup, IEnumerable<PreRecruitmentEmployee> entities, string companyId);

        IEnumerable<PreRecruitmentEmployee> GetMasterlist(string PKs);

        GridModel GetBudgetCodeList(GridParameter parameters, string plantId);
        GridModel GetManpowerBudgetListByEntitySql(GridParameter parameters, string plantId, string entityids);
        GridModel GetCbo();
    }
}