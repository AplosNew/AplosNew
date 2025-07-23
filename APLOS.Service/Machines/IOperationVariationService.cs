#region Using

using Library.Core;
using Library.Model.Machines;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Machines
{
    /// <summary>
    /// </summary>
    public interface IOperationVariationService : IService<OperationVariation>
    {
        void UpdateOperationVaiationCode(OperationVariation entity);
        void DeleteOperationVariationSizeGroup(string id);
        object GetOperationUtilityData(string operationId, string articleId, string skillId);
        IEnumerable<object> GetCbo(string operationId);

        decimal GetAutoSequence(string companyGroupId, string operationId);

        GridModel Query(GridParameter parameters, string groupId, string operationId);

        GridModel GetMachineListByOperation(GridParameter parameters, string companyGroupId, string operationId);

        void InsertGraph(OperationVariation entity, IEnumerable<OperationVariationAttributeValue> valueList, IEnumerable<OperationVariationSizeGroup> operationVariationSizeGroupDataList, IEnumerable<OperationVariationProductMaster> operationVariationPMDataList);

        void UpdateGraph(OperationVariation entity, IEnumerable<OperationVariationAttributeValue> valueList, IEnumerable<OperationVariationSizeGroup> operationVariationSizeGroupDataList, IEnumerable<OperationVariationProductMaster> operationVariationPMDataList);

        void DeleteGraph(string id);

        IEnumerable<object> GetVairiationValue(string operationId, string masterId);
    }
}