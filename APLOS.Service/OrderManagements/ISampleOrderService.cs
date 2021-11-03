#region Using

using Library.Core;
using Library.Model.OrderManagements;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.OrderManagements
{
    public interface ISampleOrderService : IService<SampleOrder>
    {
        void InsertGraph(SampleOrder entity, IEnumerable<SampleOrderSubMaterial> details, IEnumerable<SampleOrderPartnerFunction> partnerFunctions);

        void UpdateGraph(SampleOrder entity, IEnumerable<SampleOrderSubMaterial> details, IEnumerable<SampleOrderPartnerFunction> partnerFunctions);

        void DeleteGraph(string key);

        GridModel Query(GridParameter parameters, string plantId);
    }
}