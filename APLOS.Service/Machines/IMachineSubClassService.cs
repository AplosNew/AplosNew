using Library.Model.Machines;
using Library.Service.Core;
using System.Collections.Generic;

namespace Library.Service.Machines
{
    /// <summary>
    ///  Author:Belayet, Date:23-Dec-2015
    /// </summary>
    public interface IMachineSubClassService : IService<MachineSubClass>
    {
        /// <summary>
        /// GetMachineSubClassList
        /// </summary>
        /// <param name="machineClassId"></param>
        /// <returns>IEnumerable<object></returns>
        IEnumerable<object> GetMachineSubClassList();

        /// <summary>
        /// GetAutoSequence
        /// </summary>
        /// <returns>decimal</returns>
        decimal GetAutoSequence();
    }
}