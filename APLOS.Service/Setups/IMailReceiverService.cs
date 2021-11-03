#region Using

using Library.Core;
using Library.Model.Setups;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Setups
{
    public interface IMailReceiverService : IService<MailReceiver>
    {
        void DeleteDetail(int Id);

        void Insert(MailReceiver entity, IEnumerable<MailReceiverDetail> mailReceiverDetailList);

        void InsertMailReceiverMapping(MailReceiverServiceMapping entity);

        void Update(MailReceiver entity, IEnumerable<MailReceiverDetail> mailReceiverDetailList);

        void UpdateMailReceiverMapping(MailReceiverServiceMapping entity);

        void DeleteGraph(string masterId);

        void DeleteMapping(string id);

        IEnumerable<object> GetTaggingUser(string mailReceiverId);

        IEnumerable<object> GetCbo(string companyGroupId);

        GridModel QueryMailReceiverMapping(GridParameter parameters);

        GridModel Query(GridParameter parameters);

        GridModel AdminQuery(GridParameter parameters);

        IEnumerable<object> GetAdminCcUser();

        IEnumerable<object> GetAdminBccUser();
    }
}