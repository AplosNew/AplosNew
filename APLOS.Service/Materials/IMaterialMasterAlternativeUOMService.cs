using Library.Model.Materials;
using Library.Service.Core;
using System.Collections.Generic;

namespace Library.Service.Materials
{
    public interface IMaterialMasterAlternativeUOMService : IService<MaterialMasterAlternativeUOM>
    {
        void Insert(IEnumerable<MaterialMasterAlternativeUOM> entity, string materiaMasterId);

        IEnumerable<object> GetMaterialMasterAltUomList(string materialMasterId);

        IEnumerable<object> GetAlternativeUOMByMaterialMasterId(string materialMasterId);

        void DeleteGraph(string materialMasterId);
    }
}