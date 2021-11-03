#region Using

using Library.Core;
using Library.Model.Employees;
using Library.Model.IE;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Employees
{
    /// <summary>   Interface for SOP Item service. </summary>
    public interface ISkillGroupService : IService<OperationMaster> 
    {
        //GridModel Query(GridParameter parameters, string companyGroupId);

        //void InsertGraph(SOPItem entity, IEnumerable<SOPAttachmentDetail> sopAttachmentDetail);

        //void UpdateGraph(SOPItem entity, IEnumerable<SOPAttachmentDetail> sopAttachmentDetail);

        //GridModel Query(GridParameter parameters, string companyGroupId, string[] sopItemIds);

        IEnumerable<object> GetSkillMaster();
        IEnumerable<object> GetSkillMasterDetail(string queryString, string queryStringProcess, string queryStringSkill, string queryStringOperationCode, string queryStringGrouping, string queryStringMachineCategory, string queryStringMachineSubCategoryCode, string queryStringCaption, string queryStringOperationCategoryId, string queryStringOnRoll, string queryStringTotalPresent, string queryStringOnRollShort, string queryStringOnRollExcess, string queryStringPresentShort, string queryStringPresentExcess);
        IEnumerable<object> GetGraphDetails(string queryString, string queryStringProcess, string queryStringSkill, string queryStringOperationCode, string queryStringGrouping, string queryStringMachineCategory, string queryStringMachineSubCategoryCode, string queryStringCaption, string queryStringOperationCategoryId, string queryStringOnRoll, string queryStringTotalPresent, string queryStringOnRollShort, string queryStringOnRollExcess, string queryStringPresentShort, string queryStringPresentExcess);
        IEnumerable<object> GetGraphDetails1();

        IEnumerable<object> GetProcess();    
        IEnumerable<object> GetEntity();

        //decimal GetAutoSequence();
        ////
    }
}