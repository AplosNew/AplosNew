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
        Issue,
        UpdateAudit,
        FollowUpAudit,
        InternalAudit,
        ExternalAudit
    }
    public enum TaskCategoryFlagEnum
    {
        ToDo,
        TNA,
        Issue
    }

    public enum AuthorizationTypeEnum
    {
        CreatedBy,
        AssignTo,
        CheckBy,
        CrossCheckBy,
        ApproveBy,
        UpdateAudit,
        FollowUpAudit,
        InternalAudit,
        ExternalAudit
    }
}
