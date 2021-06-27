#region Using

using Library.Core;
using Library.Model.Setups;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Setups
{
    public interface ISpecialTaxService : IService<SpecialTax>
    {
        IEnumerable<object> GetCbo(string plantId);

        decimal GetAutoSequence();

        GridModel Query(GridParameter parameters, string countryId);
    }
}