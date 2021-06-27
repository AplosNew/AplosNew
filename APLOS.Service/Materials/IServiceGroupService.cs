using Library.Core;
using Library.Model.Materials;
using Library.Service.Core;
using System.Collections.Generic;

namespace Library.Service.Materials
{
    public interface IServiceGroupService : IService<ServiceGroup>
    {
        IEnumerable<object> GetCbo();

        decimal GetAutoSequence(string serviceTypeId);

        void Delete(string Id);

        GridModel Query(GridParameter parameters, string serviceTypeId);
    }
}