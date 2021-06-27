#region Using

using Library.Core;
using Library.Model.Biometrics;
using Library.Model.Productions.Recipe;
using Library.Service.Core;
using System;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Productions
{
    public interface IRecipeMaterialGroupingMasterService : IService<RecipeMaterialGroupingMaster>
    {
        IEnumerable<object> GetRecipeMaterialGroupingDetailList(string masterId);
        GridModel Query(GridParameter parameters);
        void InsertOrUpdate(RecipeMaterialGroupingMaster details);
        decimal GetAutoSequence();
        IEnumerable<object> GetCbo();
        void CreateRecipeMaterialGroupingDetail(RecipeMaterialGroupingDetail entity);
        void DeleteRawMaterial(string rawmaterialid);
        void Delete(string id);
        bool RecipeMaterialGroupingValidation(string RecipeMaterialGroupingMasterId, string articleId, string MaterialMasterId);
    }
}