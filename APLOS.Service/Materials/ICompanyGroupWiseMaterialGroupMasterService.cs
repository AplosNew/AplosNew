using Library.Model.Materials;
using Library.Service.Core;

namespace Library.Service.Materials
{
    public interface ICompanyGroupWiseMaterialGroupMasterService : IService<CompanyGroupWiseMaterialGroupMaster>
    {
        void InsertGraph(string operationId);

        void DeleteGraph(string masterId);
    }
}