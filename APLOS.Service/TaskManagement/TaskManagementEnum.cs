using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.Service.Enums
{
    public enum CurrentStatusEnum
    {
       ToStart,
       InProgress,
       Done,
       ToClose,
       Closed
    }

    public enum TaskTypeEnum
    {
        ToDo,
        TNA,
        UpdateAudit,
        FollowUpAudit,
        InternalAudit,
        ExternalAudit
    }

    public enum AuthorizationTypeEnum
    {
        CreatedBy,
        AssignTo,
        UpdateAudit,
        FollowUpAudit,
        InternalAudit,
        ExternalAudit
    }
}
