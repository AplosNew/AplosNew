#region Using

using Library.Core;
using Library.Model.Inventory;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.MaterialManagement.Inventory
{
    public interface IEntitySFGInventoryService : IService<EntitySFGInventory>
	{
		GridModel Query(GridParameter parameters, string entityId);

		GridModel GetProcessListByEntity(GridParameter parameters, string entityId);

		void InsertUpdateOrDelete(IEnumerable<EntitySFGInventory> entities);

		void DeleteGraph(string entityId);

		void Delete(string id);

        GridModel GetEntityProcessCbo(bool cadmin, bool sadmin, string userId, string entityId);
	}
}