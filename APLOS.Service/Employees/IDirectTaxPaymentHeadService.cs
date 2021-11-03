#region Using

using Library.Core;
using Library.Model.Employees;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Employees
{
    public interface IDirectTaxPaymentHeadService : IService<DirectTaxPaymentHead>
    {
        GridModel Query(GridParameter parameters);

        decimal GetAutoSequence();

        IEnumerable<object> GetCbo();
    }
}