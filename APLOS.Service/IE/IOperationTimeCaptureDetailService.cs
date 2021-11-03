using Library.Model.IE;
using Library.Service.Core;
using System.Collections.Generic;

namespace Library.Service.IEnumerable
{
    public interface IOperationTimeCaptureDetailService : IService<OperationTimeCaptureDetail>
    {
        /// <summary>
        /// GetAreaList
        /// </summary>
        /// <returns>IEnumerable<object></returns>
        IEnumerable<object> GetOperationTimeCaptureDetailList();

        int GetVasVersion(string operationId);

        IEnumerable<object> GetAllVersion(string operationId);

        void InsertOrUpdateGraph(string masterId, IEnumerable<OperationTimeCaptureDetail> from_ui, out List<OperationTimeCaptureDetail> from_db);

    }
}