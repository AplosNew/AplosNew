#region Using

using Library.Core;
using Library.Model.Processes;
using Library.Service.Core;

#endregion Using

namespace Library.Service.Processes
{
    public interface IProductionProcessGroupService : IService<ProductionProcessGroup>
    {
        GridModel GetCbo(string companyGroupId);

        GridModel Query(GridParameter parameters, string companyGroupId);

        decimal GetAutoSequence();

        void DeleteGraph(string key);
    }
}