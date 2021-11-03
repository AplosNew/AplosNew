using Library.Model.Inventory;
using Library.Service.Core;
using Library.ViewModel.Materials;
using Library.ViewModel.OrderManagements;
using System.Collections.Generic;

namespace Library.Service.Inventory
{
    public interface IProcurementMasterDetailService : IService<ProcurementMasterDetail>
    {
        IEnumerable<object> GetProcurementMasterDetailsByMasterId(string procurementMasterId);
        void DetailDeleteReq(string id);

    }

}