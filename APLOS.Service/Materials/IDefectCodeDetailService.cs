using Library.Core;
using Library.Model.Materials;
using Library.Service.Core;
using System.Collections.Generic;

namespace Library.Service.Materials
{
    public interface IDefectCodeDetailService : IService<DefectCodeDetail>
    {
        void Insert(IEnumerable<DefectCodeDetail> entity, string materialGridId, string[] deletedItems);

        GridModel Query(GridParameter parameters, string defectCodeId);

        void DeleteGraph(string defectCodeId);

        void DeleteGraph(string[] deletedItems);
    }
}