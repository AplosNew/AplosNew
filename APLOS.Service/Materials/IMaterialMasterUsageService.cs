#region Using

using Library.Model.Materials;
using Library.Service.Core;

#endregion Using

namespace Library.Service.Materials
{
    public interface IMaterialMasterUsageService : IService<MaterialMasterUsage>
    {
        void InsertOrUpdate(MaterialMasterUsage entity, string materialMasterId);

        MaterialMasterUsage Get(string materialMasterId);

        void DeleteGraph(string materialMasterId);
    }
}