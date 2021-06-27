using Library.Core;
using Library.Model.Materials;
using Library.Service.Core;
using System.Collections.Generic;

namespace Library.Service.Materials
{
    public interface IDefectCodeService : IService<DefectCode>
    {
        GridModel Query(GridParameter parameters, string processId);

        void Insert(DefectCode entity, IEnumerable<DefectCodeDetail> defectCodeDetail);

        void Update(DefectCode entity, IEnumerable<DefectCodeDetail> defectCodeDetail, string[] deletedItems);

        void DeleteGraph(string id);

        IEnumerable<object> GetDefectCodeList();
    }
}