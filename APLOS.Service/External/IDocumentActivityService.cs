#region Using

using Library.Model.External;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.External
{
    public interface IDocumentActivityService : IService<DocumentActivity>
    {
        IEnumerable<object> GetDocumentList(string activityId);

        Dictionary<string, object> GetDocFile(string id);
    }
}