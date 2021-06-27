#region Using

using Library.Core;
using Library.Model.Materials;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Materials
{
    public interface IFabricRollManagementSettingsService : IService<FabricRollManagementSettings>
    {
        GridModel Query(GridParameter parameters, string[] searchParam);

        GridModel GetMaterialMasterList(GridParameter parameters, string companyGroupId, string[] paramList);

        IEnumerable<object> GetCharacteristicsList(string materialMasterId);

        void Delete(string id);
    }
}