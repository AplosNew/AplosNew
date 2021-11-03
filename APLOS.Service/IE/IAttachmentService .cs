#region Using

using Library.Core;
using Library.Model.Employees;
using Library.Model.IE;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.IE
{
    public interface IAttachmentService : IService<Attachment>
    {
        GridModel Query(GridParameter parameters);

        decimal GetAutoSequence();

        IEnumerable<object> GetCbo();
    }
}