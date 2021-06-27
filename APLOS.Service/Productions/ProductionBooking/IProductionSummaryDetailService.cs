#region Using

using Library.Core;
using Library.Model.Productions;
using Library.Model.Productions.ProductionBooking;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Productions
{
    public interface IProductionSummaryDetailService : IService<ProductionSummaryDetail>
    {
        void Save(string psPK, IEnumerable<ProductionSummaryDetail> psd);
        void DeleteDetail(string psPK);
        void InsertSecondCharacteristic(IEnumerable<ProductionSummaryDetail> entites, ProductionSummary productionSummary);
    }
}