#region Using

using Library.Core;
using Library.Model.Machines;
using Library.Service.Core;
using System.Collections.Generic;
using System.Data;

#endregion Using

namespace Library.Service.Machines
{
    /// <summary>
    /// </summary>
    public interface IOperationService : IService<Operation>
    {
        IEnumerable<object> GetCbo(string companyGroupId);

        decimal GetAutoSequence();

        IEnumerable<object> GetOperationCbo(string subprocessid);

        GridModel Query(GridParameter parameters, string companyGroupId, string[] ids);

        GridModel GetSearchData(GridParameter parameters, string groupId, string processid, string fgComponentId);

        GridModel GetOperationListByProcess(GridParameter parameters, string processId);

        GridModel GetArticleListByMaterialMaster(GridParameter parameters, string materialMasterId);

        object GetOperationUtilityData(string operationId);

        void InsertGraph(Operation entity
            , IEnumerable<OperationProcess> processList
            , IEnumerable<OperationFgComponent> operationFgComponent
            , IEnumerable<OperationAttribute> attributeList
            , IEnumerable<OperationAttributeValue> valueList);

        void UpdateGraph(Operation entity
            , IEnumerable<OperationProcess> processList
            , IEnumerable<OperationFgComponent> operationFgComponent
            , IEnumerable<OperationAttribute> attributeList
            , IEnumerable<OperationAttributeValue> valueList);

        void DeleteGraph(string id);

        decimal GetAttributeSequence(string operationId);

        IEnumerable<object> GetOperationAttributeList(string operationId);

        IEnumerable<object> GetOperationAttributeListForSubOperation(string operationId);

        decimal GetValueSequence(string operationAttributeId);
        IEnumerable<object> GetOperationAttributeValueList(string operationId);

        GridModel GetValueListByAttributeId(GridParameter parameters, string attributeId);

       
    }
}