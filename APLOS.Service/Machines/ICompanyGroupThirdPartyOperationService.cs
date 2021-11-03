using Library.Model.Machines;
using Library.Service.Core;

namespace Library.Service.Machines
{
    public interface ICompanyGroupThirdPartyOperationService : IService<CompanyGroupThirdPartyOperation>
    {
        void DeleteGraph(string key);
    }
}