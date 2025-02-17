using System.ComponentModel;

namespace Library.Service.Enums
{
    public enum EnumResignationApprovalStatus
    {
        Approved
      , Rejected
      , Hold
    }

    public enum EnumDocumantationBy
    {
        Department
      , Self
    }

    public enum ProfileType
    {
        NID,
        TIN,
        Qualification,
        Training,
        Experience,
        Photo,
        Resignation,

        [Description("Confirmation Resignation")]
        ConfirmationResignation,

        ESIC,
        PF
    }

    public enum RelatedType
    {
        [Description("Employee Related")]
        EmployeeRelated

      , [Description("Company Related")]
        CompanyRelated
    }

    public enum DurationUOM
    {
        Days,
        Months,
        Years
    }

    public enum EnumSalaryApprovalStatus
    {
        Approved
      , Rejected
      , Hold
    }

    public enum LetterType
    {
        [Description("Appointment Letter")]
        AppointmentLetter,
        Acknowledgement,
        [Description("Confirmation Letter")]
        ConfirmationLetter,
        [Description("Exit Interview")]
        ExitInterview,
        [Description("Fixation")]
        Fixation,
        IdCard,
        [Description("Increment History")]
        IncrementHistory,
        [Description("Joining Letter")]
        JoiningLetter,
        [Description("Leave Register")]
        LeaveRegister,
        [Description("Nominee Info")]
        NomineeInfo,
        [Description("Service Book")]
        ServiceBook
    }
    public enum PaymentMode
    {
        Bank,
        Cash,
        Transfer,
        Check
    }

    public enum SupervisorActionStatus
    {
        CheckedBy,
        AuthorizedBy,
        LineSupervisor,

    }
    public enum AdvanceType
    {
        General,
        Salary
    }

    public enum Authorization
    {
        [Description("Employee Advance CheckedBy")]
        EmployeeAdvanceCheckedBy,

        [Description("Employee Advance ApproveBy")]
        EmployeeAdvanceApproveBy,

        [Description("Employee Multiple Advance ApproveBy")]
        EmployeeMultipleAdvanceApproveBy,

        [Description("Expense Booking CheckedBy")]
        ExpenseBookingCheckedBy,

        [Description("Expense Booking ApproveBy")]
        ExpenseBookingApproveBy,

        [Description("Goods Receive Note CheckedBy")]
        GoodsReceiveNoteCheckedBy,

        [Description("Goods Receive Note ApproveBy")]
        GoodsReceiveNoteApproveBy,

        [Description("Kiosk Leave CheckedBy")]
        KioskLeaveCheckedBy,

        [Description("Kiosk Leave ApproveBy")]
        KioskLeaveApproveBy,

        [Description("Purchase Order CheckedBy")]
        PurchaseOrderCheckedBy,

        [Description("Purchase Order ApproveBy")]
        PurchaseOrderApproveBy,

        [Description("Requisition CheckedBy")]
        RequisitionCheckedBy,

        [Description("Requisition ApproveBy")]
        RequisitionApproveBy,

        [Description("Issue Slip CheckedBy")]
        IssueSlipCheckedBy,

        [Description("Issue Slip ApproveBy")]
        IssueSlipApproveBy,


        [Description("Service Acknowledgement CheckedBy")]
        ServiceAcknowledgementCheckedBy,

        [Description("Service Acknowledgement ApproveBy")]
        ServiceAcknowledgementApproveBy,


        [Description("Service Requisition CheckedBy")]
        ServiceRequisitionCheckedBy,

        [Description("Service Requisition ApproveBy")]
        ServiceRequisitionApproveBy,

        [Description("Service PO CheckedBy")]
        ServicePOCheckedBy,

        [Description("Service PO ApproveBy")]
        ServicePOApproveBy,

        [Description("Gate Pass CheckedBy")]
        GatePassCheckedBy,

        [Description("Gate Pass ApproveBy")]
        GatePassApproveBy,
        [Description("Gate Pass ApproveBySecurity")]
        GatePassApproveBySecurity,


        [Description("Service PO Acknowledgement CheckedBy")]
        ServicePOAcknowledgementCheckedBy,

        [Description("Service PO Acknowledgement ApproveBy")]
        ServicePOAcknowledgementApproveBy,

        [Description("Purchase Return CheckedBy")]
        PurchaseReturnCheckedBy,

        [Description("Purchase Return ApproveBy")]
        PurchaseReturnApproveBy,

        [Description("Inventory Sales CheckedBy")]
        InventorySalesCheckedBy,

        [Description("Inventory Sales ApproveBy")]
        InventorySalesApproveBy,

        [Description("Inventory Scrap CheckedBy")]
        InventoryScrapCheckedBy,
        [Description("Inventory Scrap ApproveBy")]
        InventoryScrapApproveBy,


        [Description("Job Work Receipt CheckedBy")]
        JobWorkReceiptCheckedBy,
        [Description("Job Work Receipt ApproveBy")]
        JobWorkReceiptApproveBy,

        [Description("Out Source CheckedBy")]
        OutSourceCheckedBy,
        [Description("Out Source ApproveBy")]
        OutSourceApproveBy,

        [Description("Order Costing CheckedBy")]
        OrderCostingCheckedBy,

        [Description("Order Costing ApproveBy")]
        OrderCostingApproveBy,

        [Description("Employee Approval Authority")]
        EmployeeApprovalAuthority,

        [Description("Capitalize Asset Register ApproveBy")]
        CapitalizeAssetRegisterApproveBy,
        [Description("GoodWork Payment ApproveBy")]
        GoodWorkPaymentApproveBy,

        [Description("Party ApproveBy")]
        PartyApproveBy,

        [Description("Sales Chalan CheckedBy")]
        SalesChalanCheckBy,
        [Description("Sales Chalan ApproveBy")]
        SalesChalanApproveBy,
        [Description("Multiple Vendor Payment")]
        MultipleVendorPayment,

        [Description("Journal ApproveBy")]
        JournalApproveBy,
        [Description("Full And Final ApproveBy")]
        FullAndFinalApproveBy,
        [Description("Input Credit CheckedBy")]
        InputCreditCheckedBy,
        [Description("Input Credit ApproveBy")]
        InputCreditApproveBy,
        [Description("Sales Order CheckedBy")]
        SalesOrderCheckedBy,
        [Description("Sales Order ApproveBy")]
        SalesOrderApproveBy,
        [Description("Marker CheckedBy")]
        MarkerCheckedBy,
        [Description("Marker ApproveBy")]
        MarkerApproveBy
    }
}