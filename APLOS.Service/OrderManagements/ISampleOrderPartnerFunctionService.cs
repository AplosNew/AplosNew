#region Using

using Library.Core;
using Library.Model.OrderManagements;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.OrderManagements
{
    public interface ISampleOrderPartnerFunctionService : IService<SampleOrderPartnerFunction>
    {
        IEnumerable<object> Query(string masterId);

        GridModel GetCustomerBySPF(GridParameter parameters);

        void InsertOrUpdateGraph(string masterId, IEnumerable<SampleOrderPartnerFunction> partnerFunctions);

        void DeleteGraph(string masterId);
    }
}