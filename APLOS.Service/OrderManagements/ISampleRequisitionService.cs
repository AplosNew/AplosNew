using Library.Core;
using Library.Model.OrderManagements;
using Library.Service.Core;

namespace Library.Service.OrderManagements
{
    public interface ISampleRequisitionService : IService<SampleRequisition>
    {
        void DeleteGraph(string key);

        GridModel Query(GridParameter parameters);
    }
}