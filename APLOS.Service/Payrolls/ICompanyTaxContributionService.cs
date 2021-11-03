#region Using

using Library.Core;
using Library.Model.Payrolls;
using Library.Service.Core;

#endregion Using

namespace Library.Service.Payrolls
{
    public interface ICompanyTaxContributionService : IService<CompanyTaxContribution>
    {
        GridModel GetAllEmployee(GridParameter parameters, string plantId);

        void Insert(CompanyTaxContribution entity, string companyGroupId);

        GridModel Query(GridParameter parameters, string empId, string plantId, string taxYearId);

        GridModel BasicQuery(GridParameter parameters);

        GridModel Query(GridParameter parameters, string empId, string plantId);
    }
}