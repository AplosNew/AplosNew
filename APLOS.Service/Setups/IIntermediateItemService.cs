#region Using

using Library.Core;
using Library.Model.Setups;
using Library.Service.Core;

#endregion Using

namespace Library.Service.Setups
{
    public interface IIntermediateItemService : IService<IntermediateItem>
    {
        GridModel GetCbo(string companyGroupId);

        GridModel Query(GridParameter parameters, string companyGroupId);

        decimal GetAutoSequence();

        void DeleteGraph(string key);
    }
}