using Library.Model.Materials;
using Library.Service.Core;

namespace Library.Service.Materials
{
    public interface ICompanyGroupWiseMaterialGroup1Service : IService<CompanyGroupWiseMaterialGroup1>
    {
        void Insert(string operationId);

        void DeleteGraph(string masterId);
    }
}