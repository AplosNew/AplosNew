#region Using

using Library.Core;
using Library.Model.Setups;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Setups
{
    public interface IPlantSettingService : IService<PlantSetting>
    {
        decimal GetAutoSequence();
        GridModel Query(GridParameter parameters);
        Dictionary<string, object> GetGetAuthorizedSignatureFile(string id);
    }
}