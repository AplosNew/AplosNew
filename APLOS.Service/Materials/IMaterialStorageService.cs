#region Using

using Library.Core;
using Library.Model.Materials;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Materials
{
    public interface IMaterialStorageService : IService<MaterialStorage>
    {
        IEnumerable<object> GetCbo(string groupId, string companyId, string plantId);
        IEnumerable<object> GetCboForOnlyMaterialTransfer(string groupId, string companyId, string plantId); 
        
        decimal GetAutoSequence(string groupId, string companyId, string plantId);

        GridModel Query(GridParameter parameters, string groupId, string companyId, string plantId);
    }
}