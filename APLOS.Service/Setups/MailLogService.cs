using Library.Core;
using Library.Data.Sql;

namespace Library.Service.Setups
{
    public class MailLogService : IMailLogService
    {
        #region Constructor

        private readonly ISqlRepository _sqlRepository;

        public MailLogService(ISqlRepository sqlRepository
            )
        {
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        public GridModel MailLogList(GridParameter parameters, string fromDate, string toDate)
        {
            string condition = string.Empty;
            if (fromDate == "undefined" && toDate == "undefined")
            {
                condition = "";
            }
            else
            {
                condition = "WHERE CONVERT(date,RecordTime) BETWEEN CONVERT(DATE,'" + fromDate + @"') AND CONVERT(DATE,'" + toDate + @"')";
            }
            parameters.CmdText = @"SELECT ROW_NUMBER() OVER (ORDER BY SenderName DESC) AS RowNum, CG.UserName CompanyGroup,MR.Name MailReceiverName,[MailReceiverId],[MailGenerator],[SenderEmail]
											   ,[SenderName],[Subject],[ServiceName],[AttachmentName],[RecordTime],[AppVersion],[IsSuccess]
											   ,[IsServiceActive],[IsReciepientListActive],[HasAttachment],[InactiveUsers],[MissingEMails]
											   ,[ToAddressProblem],[ToList],[CcList],[BccList],ML.[AddedBy],ML.[AddedDate],ML.[AddedFromIP],ML.[Remarks]
											   FROM [ACS].[MailLog] as ML
											   LEFT JOIN ORG.CompanyGroup AS CG ON CG.Id = Ml.CompanyGroupId
											   LEFT JOIN SCS.MailReceiver AS MR ON MR.Id = Ml.MailReceiverId " + condition + @"";

            return _sqlRepository.GetGridData(parameters);
        }
    }
}