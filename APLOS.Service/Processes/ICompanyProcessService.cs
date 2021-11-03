#region Using

using Library.Core;
using Library.Model.Processes;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Processes
{
    public interface ICompanyProcessService : IService<CompanyProcess>
    {
        void InsertUpdateOrDeleteGraph(IEnumerable<CompanyProcess> entities, string companyGroupId);
		IEnumerable<object> GetCompanyProcessCbo(string companyId);

		IEnumerable<ComboModel> GetCompanyProductionProcessCbo(string companyId);

		GridModel Query(GridParameter parameters, string companyId);

        /// <summary>
        /// Search for without existig process in ui.
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="companyGroupId"></param>
        /// <param name="companyId"></param>
        /// <param name="processIds"></param>
        /// <returns></returns>
		GridModel GetCompanyProcessList(GridParameter parameters, string companyId, string[] processIds);

		GridModel GetCompanyProductionProcessList(GridParameter parameters, string companyGroupId, string companyId, string[] processIds);

    }
}