#region Using

using Library.Core;
using Library.Model.Inventory;
using Library.Model.TaskManagement;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.TaskManagement
{
    public interface IEntityTaskService : IService<EntityTask>
	{
		GridModel Query(GridParameter parameters, string entityId);

        GridModel GetTaskMasterData(GridParameter parameters);

        void InsertUpdateOrDelete(IEnumerable<EntityTask> entities);

		void DeleteGraph(string entityId);

		void Delete(string id);
        
	}
}