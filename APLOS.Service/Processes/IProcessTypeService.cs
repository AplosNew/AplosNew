#region Using

using Library.Core;
using Library.Model.Processes;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Processes
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Author:Belayet Date:06-09-2015. </summary>
    ///
    /// <remarks>   Modified  By:Mehedi Hasan Tamim;Date:28-12-2015; </remarks>
    /// <remarks>   Modified Last By:Belayet Hossain;Date:12-Jan-2016; </remarks>
    ///-------------------------------------------------------------------------------------------------

    public interface IProcessTypeService : IService<ProcessType>
    {
        GridModel GetProcessTypeList(GridParameter parameters, string processId);

        void DeleteGraph(string id);

        IEnumerable<object> GetCbo();

        IEnumerable<object> GetProcessType();

        IEnumerable<object> GetCbobyProcess(string processId);
    }
}