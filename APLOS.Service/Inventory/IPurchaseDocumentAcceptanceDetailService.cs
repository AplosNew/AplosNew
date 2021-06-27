using Library.Core;
using Library.Model.Inventory;
using Library.Service.Core;
using Library.ViewModel.Inventory;
using Library.ViewModel.Materials;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;

namespace Library.Service.Inventory
{
    public interface IPurchaseDocumentAcceptanceDetailService : IService<PurchaseDocAcceptanceDetail>    
    {
        //IEnumerable<object> GetPOWithLCList(string plantId, string PoType);
        //GridModel QueryOnlyPO(GridParameter parameters, string inveReveiveId);

        //IEnumerable<object>  GetAcceptanceCharges();

        //void InsertOrUpdateGraphNew(PurchaseDocAcceptance entity, IEnumerable<PurchaseDocAcceptanceDetailViewModel> PurchaseDocAcceptanceDetail, IEnumerable<PurchaseDocAcceptanceServiceViewModel> AcceptancechargesList);

    }
}
