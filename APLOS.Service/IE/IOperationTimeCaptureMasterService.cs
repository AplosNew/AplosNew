using Library.Core;
using Library.Model.IE;
using Library.Service.Core;
using System.Collections.Generic;

namespace Library.Service.IEnumerable
{
    public interface IOperationTimeCaptureMasterService : IService<OperationTimeCaptureMaster>
    {
        /// <summary>
        /// GetAreaList
        /// </summary>
        /// <returns>IEnumerable<object></returns>
        GridModel GetSearchData(GridParameter parameters);

        IEnumerable<object> GetOperationTimeCaptureMasterList();

        IEnumerable<object> GetOperationList();

        void Insert(OperationTimeCaptureMaster operationtimecapturemaster, IEnumerable<OperationTimeCaptureDetail> operationtimecapturedetailList);

        IEnumerable<OperationTimeCaptureDetail> GetOperationTimeCaptureDetailList_tested(string MasterId);

        // IEnumerable<object> GetOperationTimeCaptureDetailList(string MasterId);
        IEnumerable<object> GetMasterData(string MasterId);

        IEnumerable<object> GetDetailList(string MasterId);
    }
}