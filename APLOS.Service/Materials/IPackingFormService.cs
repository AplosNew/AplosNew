#region Using

using Library.Core;
using Library.Model.Materials;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Materials
{
    public interface IPackingFormService : IService<PackingForm>
    {
        IEnumerable<ComboModel> GetCbo(string companyGroupId);

        decimal GetAutoSequence(string companyGroupId);

        GridModel Query(GridParameter parameters, string companyGroupId);
    }
}