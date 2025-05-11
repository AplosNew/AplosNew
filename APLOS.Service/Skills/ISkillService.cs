#region Using

using Library.Core;
using Library.Model.Employees;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Skills
{
    public interface ISkillService : IService<Skill>
    {
        /// <summary>
        /// goes for operation.which skill not declare for machine type.
        /// </summary>
        /// <param name="processId"></param>
        /// <returns></returns>
        IEnumerable<object> GetCboWithoutMachineType(string processId);

        /// <summary>
        /// goes for machine. which skill declare for machine type.
        /// </summary>
        /// <param name="processId"></param>
        /// <returns></returns>
        IEnumerable<object> GetCboByProcess(string companyGroupId,string[] processIds);

        //IEnumerable<object> GetCboForMachineType(string processId);
        /// <summary>
        /// Get cbo by machine type id.
        /// </summary>
        /// <param name="processId"></param>
        IEnumerable<ComboModel> GetCboByMachineTypeId(string processId, string matchineTypeId);

        GridModel GetIsMachineSkillList(GridParameter parameters, string companyGroupId, string[] skillProcessIds);

        GridModel GetCommonSkillListByProcess(GridParameter parameters, string companyGroupId, string[] processIds, bool MachineRequired);

        decimal GetAutoSequence();

        void InsertGraph(Skill entity, IEnumerable<SkillProcess> skillProcess);

        void UpdateGraph(Skill entity, IEnumerable<SkillProcess> skillProcess);

        void DeleteGraph(string id);

        GridModel Query(GridParameter parameters);
    }
}