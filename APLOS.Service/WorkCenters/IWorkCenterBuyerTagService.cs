using Library.Core;
using Library.Model.WorkCenters;
using Library.Service.Core;

namespace Library.Service.WorkCenters
{
    public interface IWorkCenterBuyerTagService : IService<WorkCenterBuyerTag>
    {
        GridModel Query(GridParameter parameters, string plantId, string unitId);

        void Delete(string id);
    }
}