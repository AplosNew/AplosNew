#region Using

using Library.Core;
using Library.Model.Processes;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Processes
{
	public interface IEntityProcessTagService : IService<EntityProcessTag>
	{
		GridModel Query(GridParameter parameters, string entityId);

		GridModel GetProcessListByEntity(GridParameter parameters, string entityId);

		void InsertUpdateOrDelete(IEnumerable<EntityProcessTag> entities);

		void DeleteGraph(string entityId, string productionProcessGroupId);

		void Delete(string id);

        GridModel GetEntityProcessCbo(bool cadmin, bool sadmin, string userId, string entityId);
		GridModel GetEntityCuttingProcessCbo(bool cadmin, bool sadmin, string userId, string entityId);
	}
}