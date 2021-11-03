namespace Library.Service.Enums
{
    public enum TaskAppliedOnEnum
    {
        MasterOrder,
        Style,
        SalesOrder,
        ProductionOrder
    }
    public enum TaskNotificationTypesEnum
    {
        //info
        Attendance,
        Salary,
        SalaryDisbursement,
        SalaryApproval,
        SalaryApprovalRollback,
        Promotion,
        PromotionRollback,
        Increment,
        IncrementRollback,
        GeneralAnnouncement,
        Holiday,
        Birthday,

        //tasks
        ExpenseBookingCheck = 101,
        ExpenseBookingApprove,
        AdvanceBookingCheck,
        AdvanceBookingApprove,
        MaterialBookingCheck,
        MaterialBookingApprove,
        PurchaseOrderCheck,
        PurchaseOrderApprove,
        GoodsReceiveNotesCheck,
        GoodsReceiveNotesApprove,
        ToDoToCheck,
        ToDoToCrossCheck,
        ToDoToApprove,
        ToDoAssignTo,
        ToDoToReview,
        TNAAssignTo,
        ToReview,
        Issue,

    }
    public enum DependentDatesEnum
    {

        MasterOrderCreationDate,
        SOShipmentDate,
        SOCreationDate,
        FirstSOShipmentDate,
        LastSOShipmentDate,
        ProductionOrderCreationDate,
        ProductionOrderFirstOutputDate,
        ProductionOrderLastoutputdate,
        LatestStartDate,
        MainRawmaterialinhouseDate,
        OtherRMinhouseDate,
        MaterialCreationDate

    }
}
