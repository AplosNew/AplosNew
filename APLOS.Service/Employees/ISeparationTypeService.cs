#region Using

using Library.Model.Employees;

using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Employees
{
    /// <summary>
    /// </summary>
    public interface ISeparationTypeService : IService<SeparationType>
    {
        /// <summary>
        /// Query Item list for dropdown.
        /// </summary>
        /// <returns>IEnumerable<object></returns>
       

        /// <summary>
        /// Query auto sequence number.
        /// </summary>
        /// <returns>decimal</returns>
        decimal GetAutoSequence();
    }
}