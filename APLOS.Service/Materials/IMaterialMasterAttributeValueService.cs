using Library.Core;
using Library.Model.Materials;
using Library.Service.Core;
using System.Collections.Generic;

namespace Library.Service.Materials
{
    public interface IMaterialMasterAttributeValueService : IService<MaterialMasterAttributeValue>
    {
        decimal GetAutoSequence();

        void InsertOrUpdateGraph(MaterialMasterAttribute attribute, IEnumerable<MaterialMasterAttributeValue> entity, IEnumerable<MaterialMasterAttributeValue> dbList);

        void DeleteGraph(IEnumerable<MaterialMasterAttributeValue> attributeValueList);

        IEnumerable<object> Query(string masterId);
     
        GridModel GetAttributeValueList(GridParameter parameters, string assignment, string materialMasterId, string attributeId);
    }
}