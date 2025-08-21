#region Using

using Library.Core;
using Library.Model.Employees;
using Library.Model.IE;
using Library.Service.Core;
using Syncfusion.XlsIO;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.IE
{
	public interface IOperationMasterService : IService<OperationMaster>
	{
		GridModel Query(GridParameter parameters);

		decimal GetAutoSequence();

		IEnumerable<object> GetCboCompanyGroup();
		IEnumerable<object> GetOperationMaster();
		IEnumerable<object> GetCboOperationType();
		IEnumerable<object> GetCboOperationCategory();
		IEnumerable<object> GetCboSkill();
		IEnumerable<object> GetCboSkillCboByMachine(string Id);
		IEnumerable<object> GetCboMachineMaster();

		IEnumerable<object> GetCboProcess();
		IEnumerable<object> GetCbolegalDesignation();
		IEnumerable<object> GetCboSkillGrouping();
		IEnumerable<object> GetDataByMasterOrderId(string id);
		IEnumerable<object> GetSkillMasterMachineData(string masterId);
		void Check(OperationMaster entity);



		IWorkbook CreateOperationMasterReports(string companyId, string plantId);



	}
}