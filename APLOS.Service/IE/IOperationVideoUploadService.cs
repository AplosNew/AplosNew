using Library.Model.IE;
using Library.Service.Core;
using System.Collections.Generic;

namespace Library.Service.IEnumerable
{
    public interface IOperationVideoUploadService : IService<OperationVideoUpload>
    {
        decimal GetAutoSequence();
        IEnumerable<object> GetOperationVideoUploadList();
    }
}