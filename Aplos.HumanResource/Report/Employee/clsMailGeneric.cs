using Library.Model.Setups;
using Library.ViewModel.Setups;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.HumanResource.Report.Employee
{
    public class clsMailGeneric
    {
        public string GetMaileList(MailReceiverServiceMapping item)
        {
           return  @"SELECT MRD.Id, MRD.UserId, MRD.MailType, ISNULL(U.FullName, MRD.FullName) AS FullName, ISNULL(U.Email, MRD.Email) AS Email, ISNULL(U.Active, CONVERT(BIT, 1)) AS Active  FROM [SCS].[MailReceiverDetail] AS MRD
						LEFT JOIN [SEC].[User] AS U ON U.Id=MRD.UserId
						JOIN [SCS].[MailReceiver] AS MR ON MR.Id = MRD.MailReceiverId
                        WHERE MRD.MailReceiverId='" + item.MailReceiverId + "' and MR.Active = 1";
            
        }
    }
}
