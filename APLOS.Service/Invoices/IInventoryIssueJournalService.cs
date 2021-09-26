using Library.Data.Repositories;
using Library.Model.Inventory;

namespace Library.Service.Invoices
{
    public interface IInventoryIssueJournalService 
    {
        InventoryIssue FindInventoryIssue(string issueId);
        InventoryIssueDetail FindInventoryIssueDetail(string issueDetailId);
        InventoryIssueHistory FindInventoryIssueHistory(string issueDetailHistoryId);
        IQueryFluent<InventoryIssueDetail> QueryInventoryIssueDetail(string issueId);
        IQueryFluent<InventoryIssueHistory> QueryInventoryIssueHistory(string issueDetailId);
        void UpdateInventoryIssue(InventoryIssue inventoryIssue);
        void UpdateInventoryIssueDetail(InventoryIssueDetail inventoryIssueDetail);
        void UpdateInventoryIssueHistory(InventoryIssueHistory inventoryIssueHistory);
    }
}