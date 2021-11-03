using Library.Model.Materials;
using Library.Service.Core;
using System.Collections.Generic;

namespace Library.Service.Materials
{
    public interface IMaterialGroupAlternativeUoMService : IService<MaterialGroupAlternativeUoM>
    {
        void InsertOrUpdateGraph(IEnumerable<MaterialGroupAlternativeUoM> entities, string masterId);

        IEnumerable<object> GetAltUomListMasterId(string masterId);

        IEnumerable<object> GetAltUomList();

        void DeleteGraph(string masterId);
    }
}