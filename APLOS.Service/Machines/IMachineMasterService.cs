using Library.Model.Machines;
using Library.Service.Core;
using System.Collections.Generic;

namespace Library.Service.Machines
{
    public interface IMachineMasterService : IService<MachineMaster>
    {
        /// <summary>
        /// GetMachineMasterList
        /// </summary>
        /// <returns></returns>
        IEnumerable<object> GetMachineMasterList();

        /// <summary>
        /// MachineMasterDetailViewModel
        /// </summary>
        /// <param name="Id">string</param>
        /// <returns></returns>
        IEnumerable<MachineMaster> GetAllById(string Id);

        /// <summary>
        /// Query auto sequence number.
        /// </summary>
        /// <returns>decimal</returns>
        decimal GetAutoSequence();
    }
}