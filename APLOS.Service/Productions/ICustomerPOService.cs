using Library.Core;
using Library.Model.Productions;
using Library.Service.Core;
using System;
using System.Collections.Generic;

namespace Library.Service.Productions
{
    public interface ICustomerPOService : IService<CustomerPO>
    {
        GridModel GetListByMasterOrder(string companyId, string masterOrderId);

        Tuple<string, string> InsertGraphPo(CustomerPO entity);
    }
}