#region Using

using Library.Core;
using Library.Model.Employees;
using Library.Model.IE;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.IE
{
    public interface IMachineMasterUIService : IService<MachineMasterUI>
    {
        GridModel Query(GridParameter parameters);

        decimal GetAutoSequence();

        IEnumerable<object> GetCboCompanyGroup();
        //IEnumerable<object> GetOperationMaster();
        IEnumerable<object> GetMachineMaster();
        IEnumerable<object> GetCboOperationType();
        //IEnumerable<object> GetCboOperationCategory();
        IEnumerable<object> GetCboMachineCategory();

        IEnumerable<object> GetCboMachineSubCategory(); 
        IEnumerable<object> GetCboSkill();
        IEnumerable<object> GetCboMachineMaster();

        IEnumerable<object> GetCboProcess();
        IEnumerable<object> GetCbolegalDesignation();
        IEnumerable<object> GetCboSkillGrouping();
        IEnumerable<object> GetDataByMasterOrderId(string id);
        IEnumerable<object> GetDataByMachineMasterId(string id);

        //void Check(OperationMaster entity);
        void Check(MachineMasterUI entity);




    }
}