#region Using

using Library.Core;
using Library.Model.Employees;
using Library.Model.IE;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.IE
{
    public interface IOperationPositionMPBudgetService : IService<OperationPositionMPBudget>
    {
        GridModel Query(GridParameter parameters);

        decimal GetAutoSequence(string OMId);

        IEnumerable<object> GetCboCompanyGroup();
        IEnumerable<object> GetOperationMaster();
        IEnumerable<object> GetCboOperationType();
        IEnumerable<object> GetCboOperationCategory();
        IEnumerable<object> GetCboSkill();
        IEnumerable<object> GetCboMachineMaster();

        IEnumerable<object> GetCboProcess();
        IEnumerable<object> GetCbolegalDesignation();
        IEnumerable<object> GetCboSkillGrouping();
        IEnumerable<object> GetDataByMasterOrderId(string id);

        void Check(OperationPositionMPBudget entity);

        IEnumerable<object> GetOperationPositionMPBudgetService(string id);

        IEnumerable<object> GetDataByMasterOrderIdMP(string id);
        IEnumerable<object> GetDataByMasterOrderIdMP1(string id); 

        IEnumerable<object> GetCboPosition();
        IEnumerable<object> GetCboEntity();
        IEnumerable<object> GetCboShift();

        //IEnumerable<object> GetCboLine();
    }
}