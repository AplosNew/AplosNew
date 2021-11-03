using Library.Data.Repositories;
using Library.Model.Inventory;
using Library.Service.Core;

namespace Library.Service.Invoices
{
    public class InventoryIssueJournalService : IInventoryIssueJournalService
    {
        #region Constructor

        private readonly IRepositoryAsync<InventoryIssue> _inventoryIssueRepository;
        private readonly IRepositoryAsync<InventoryIssueDetail> _inventoryIssueDetailRepository;
        private readonly IRepositoryAsync<InventoryIssueHistory> _inventoryIssueHistoryRepository;
        public InventoryIssueJournalService(
             IRepositoryAsync<InventoryIssue> inventoryIssueRepository
        , IRepositoryAsync<InventoryIssueDetail> inventoryIssueDetailRepository
        , IRepositoryAsync<InventoryIssueHistory> inventoryIssueHistoryRepository
            ) 
        {
            _inventoryIssueRepository = inventoryIssueRepository;
            _inventoryIssueDetailRepository = inventoryIssueDetailRepository;
            _inventoryIssueHistoryRepository = inventoryIssueHistoryRepository;
        }

        #endregion Constructor

        public InventoryIssue FindInventoryIssue(string issueId)
        {
            return _inventoryIssueRepository.Find(issueId);
        }

        public InventoryIssueDetail FindInventoryIssueDetail(string issueDetailId)
        {
            return _inventoryIssueDetailRepository.Find(issueDetailId);
        }
        public InventoryIssueHistory FindInventoryIssueHistory(string issueDetailHistoryId)
        {
            return _inventoryIssueHistoryRepository.Find(issueDetailHistoryId);
        }
        public IQueryFluent<InventoryIssueDetail> QueryInventoryIssueDetail(string issueId)
        {
            return _inventoryIssueDetailRepository.Query(r => r.InventoryIssueId == issueId);
        }
        public IQueryFluent<InventoryIssueHistory> QueryInventoryIssueHistory(string issueDetailId)
        {
            return _inventoryIssueHistoryRepository.Query(r => r.InventoryIssueDetailId == issueDetailId);
        }

        public void UpdateInventoryIssue(InventoryIssue inventoryIssue)
        {
            _inventoryIssueRepository.Update(inventoryIssue);
        }
        public void UpdateInventoryIssueDetail(InventoryIssueDetail inventoryIssueDetail)
        {
            _inventoryIssueDetailRepository.Update(inventoryIssueDetail);
        }
        public void UpdateInventoryIssueHistory(InventoryIssueHistory inventoryIssueHistory)
        {
            _inventoryIssueHistoryRepository.Update(inventoryIssueHistory);
        }
    }
}