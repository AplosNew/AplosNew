#region Using

using Library.Core;
using Library.Model.Employees;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Employees
{
    /// <summary>   Interface for SOP Item service. </summary>
    public interface ISOPItemService : IService<SOPItem>
    {
        GridModel Query(GridParameter parameters, string companyGroupId);

        void InsertGraph(SOPItem entity, IEnumerable<SOPAttachmentDetail> sopAttachmentDetail);

        void UpdateGraph(SOPItem entity, IEnumerable<SOPAttachmentDetail> sopAttachmentDetail);

        GridModel Query(GridParameter parameters, string companyGroupId, string[] sopItemIds);

        IEnumerable<object> GetFileBySOPId(string sopId);

        decimal GetAutoSequence();
    }
}