using Library.Core;
using Library.Model.IE;
using Library.Service.Core;
using Library.Service.Extension;
using Syncfusion.XlsIO;
using System.Collections.Generic;

namespace Library.Service.IEnumerable
{
    public interface IBulletinTemplateService : IService<BulletinTemplate>
    {

        void ReplaceOperation(OTSBD.IdentityParameter para, string Code, decimal Sequence,string processId, string bulletinTemplateMasterId);
        void InsertOperation(OTSBD.IdentityParameter para, string Code, string processId, string bulletinTemplateMasterId);
        void DeleteBulletin(string id);
        void Copy(BulletinTemplate entity);
        void UpdateSequence(BulletinTemplateDetail entity);
        void UpdateMachine(BulletinTemplateDetail entity);
        void DeleteOperation(string id);
        GridModel GetCbo(string processId);
        IEnumerable<object> GetProcessQtyAndNoWSData(string processId, string productMasterId);
        IEnumerable<object> GetOperationData(string companyGroupId, string processId, string bulletinTemplateId, string productMasterId);
        IEnumerable<object> GetBulletinOperation(string bulletinTemplateMasterId);
        void InsertOrUpdateOperation(IEnumerable<BulletinTemplateDetail> entities, string bulletinTemplateMasterId, BulletinCalculation bulletinCalculation);
        decimal GetAutoSequence();
        void InsertOrUpdateProcess(BulletinTemplateMaster entity);
        void DeleteProcess(string id);
        void DeleteBuyer(string id);
        void InsertOrUpdateBuyer(BulletinTemplateBuyerInfo entity);
        IEnumerable<object> Query(string companyGroupId);
        IEnumerable<object> GetBulletinProcess(string bulletinTemplateId);
        IEnumerable<object> GetBulletinBuyer(string bulletinTemplateId);
    }
}