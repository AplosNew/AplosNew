using Library.Core;
using Library.Model.Processes;
using Library.Service.Core;
using System.Collections.Generic;

namespace Library.Service.Processes
{
	public interface IProcessService : IService<Process>
	{
		decimal GetAutoSequence();

		void DeleteGraph(string id);

		IEnumerable<object> GetCbo(string companyGroupId);

		IEnumerable<object> GetCboByIsValueAdded(string groupId);

		IEnumerable<ComboModel> GetProductionProcessCbo(string companyGroupId);

		GridModel Query(GridParameter parameters, string companyGroupId, string[] processIds);
		GridModel GetProductionProcessList(GridParameter parameters, string companyGroupId, string CompanyId,string productionOrderId);

		GridModel GetLoadProcessWithSubProcess(GridParameter parameters, string companyGroupId);
	}
}