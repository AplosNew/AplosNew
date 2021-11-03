#region Using

using Library.Model.Machines;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Machines
{
    public interface IOperationCategoryService : IService<OperationCategory>
    {
        decimal GetAutoSequence();

        void DeleteGraph(string id);
    }
}