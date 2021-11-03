using Library.Model.Materials;
using Library.Service.Core;

namespace Library.Service.Materials
{
    public interface ICompanyGroupWiseMaterialGroup2Service : IService<CompanyGroupWiseMaterialGroup2>
    {
        void Insert(string operationId);

        void DeleteGraph(string masterId);
    }
}