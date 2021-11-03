using Library.Core;
using Library.Model.Materials;
using Library.Service.Core;
using System.Collections.Generic;

namespace Library.Service.Materials
{
    public interface IOurStyleService : IService<OurStyle>
    {
        /// <summary>
        /// GetOurStyleCbo dropdown list.
        /// </summary>
        /// <returns></returns>
        IEnumerable<object> GetCbo();

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        decimal GetAutoSequence();

        GridModel Query(GridParameter parameters);
    }
}