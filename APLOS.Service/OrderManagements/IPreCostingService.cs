#region Using

using Library.Core;
using Library.Model.OrderManagements;
using Library.Service.Core;
using System;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.OrderManagements
{
    public interface IPreCostingService : IService<PreCosting>
    {
        GridModel Query(GridParameter parameters, string companyGroupId);

        IEnumerable<object> GetPreCostingDetailList(string preCostingId);

        GridModel GetProductPreCostingWithCompanyGroup(GridParameter parameters, string companyGroupId);

        GridModel GetFinishGoodsWithCompanyGroup(GridParameter parameters, string companyGroupId);

        void InsertAndUpdate(PreCosting entity);

        void PreCostingDetailInsertAndUpdate(IEnumerable<PreCostingDetail> entities);

        IEnumerable<object> getUomList();

        void DeleteGraph(string id);

        void DeletePreCostingDetail(string id);

        IEnumerable<Object> GetPreCostingCalculation(string plantId, string fgId);

        IEnumerable<Object> GetPreCostingCalculationWithEntity(string plantId, string fgId);

        IEnumerable<Object> GetPlantWithWorkCenter(string companyId);

        IEnumerable<Object> GetFGNoOfWorkStation(string finishGoodId);

        IEnumerable<Object> GetMaterialGroupArticlePrdProcessGroupList(string materialGroupArticleId);

        IEnumerable<Object> GetMaterialGroupProcessCritia(string materialGroupArticleId);
    }
}