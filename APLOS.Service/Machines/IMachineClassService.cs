using Library.Model.Machines;
using Library.Service.Core;
using System.Collections.Generic;

namespace Library.Service.Machines
{
    public interface IMachineClassService : IService<MachineClass>
    {
        IEnumerable<object> GetCbo();

        decimal GetAutoSequence();

        void DeleteGraph(string id);
    }
}