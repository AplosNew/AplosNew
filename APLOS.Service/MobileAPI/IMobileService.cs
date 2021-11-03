using Library.Service.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.Service.MobileAPI
{
    public interface IMobileService : IService<OperationMasterData>
    {

        List<OperationMasterData> SearchOperationMasterData(string strKey, string CompanyGroupId);
        List<OperationMasterData> GetOperationMasterData(string id);
    }
}
