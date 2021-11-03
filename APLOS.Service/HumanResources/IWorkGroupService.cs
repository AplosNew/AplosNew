using Library.Core;
using Library.Model.HumanResources;
using Library.Service.Core;
using Syncfusion.XlsIO;
using System.Collections.Generic;

namespace Library.Service.HumanResources
{
    public interface IWorkGroupService : IService<WorkGroup>
    {
        
        

        GridModel Query(GridParameter parameters, string plantId);

        decimal GetAutoSequence();

        IEnumerable<object> GetCbo(string plantId);
    }
}