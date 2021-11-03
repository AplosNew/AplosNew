#region Using

using Library.Core;
using Library.Model.Employees;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Employees
{
    public interface ISOPAttachmentDetailService : IService<SOPAttachmentDetail>
    {
        void DeleteGraphBySOPItem(string sopItemId);

        void InsertGraph(IEnumerable<SOPAttachmentDetail> entities, string sopItemId);

        void UpdateGraph(IEnumerable<SOPAttachmentDetail> entities, string sopItemId);

        GridModel Query(GridParameter parameters, string sopItemId);
    }
}