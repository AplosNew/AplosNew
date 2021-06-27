#region Using

using Library.Core;
using Library.Model.Materials;
using Library.Service.Core;

#endregion Using

namespace Library.Service.Materials
{
    public interface IMaterialStockService : IService<MaterialStock>
    {
        GridModel Query(GridParameter parameters, string[] searchParam);

        GridModel GetMaterialMasterList(GridParameter parameters, string companyGroupId, string[] paramList);

        void Delete(string id);
    }
}