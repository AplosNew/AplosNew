using Library.Core;
using Library.Model.HumanResources;
using Library.Service.Core;
using Syncfusion.XlsIO;
using System.Collections.Generic;

namespace Library.Service.HumanResources
{
    public interface IMaternityLeavePolicyService : IService<MaternityLeavePolicy>
    {
        IEnumerable<object> Query(string plantId);
       
    }
}