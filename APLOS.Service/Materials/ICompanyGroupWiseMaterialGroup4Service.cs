using Library.Model.Materials;
using Library.Service.Core;

namespace Library.Service.Materials
{
    public interface ICompanyGroupWiseMaterialGroup4Service : IService<CompanyGroupWiseMaterialGroup4>
    {
        void Insert(string operationId);

        void DeleteGraph(string masterId);
    }
}