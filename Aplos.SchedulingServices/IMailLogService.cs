using Library.Core;

namespace Library.Service.Setups
{
    public interface IMailLogService
    {
        GridModel MailLogList(GridParameter parameters, string fromDate, string toDate);
    }
}