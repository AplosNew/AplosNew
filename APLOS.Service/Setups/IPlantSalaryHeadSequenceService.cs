#region Using

using Library.Core;
using Library.Model.Payrolls;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Setups
{
    public interface IPlantSalaryHeadSequenceService : IService<PlantSalaryHeadSequence>
    {
        void InsertORUpdate(IEnumerable<PlantSalaryHeadSequence> entityList);

        void Delete(string Id);

        IEnumerable<object> QueryGraph(string plantId, string companyGroupId);

        IEnumerable<object> GetSalaryHead(string companyGroupId);

        GridModel Query(GridParameter parameters);
    }
}