using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.Service.Enums
{
    public enum CostingItemType
    {
        Material,
        Process,
        NA
    }
    public enum BOQCriteria
    {
        General,
        SO,
        Destination,
        SODestination,
        SKU1,
        SKU2,
        SKU1SKU2
    }
    public enum CostingApprovalStatus
    {
        Hold, Reject, Approved
    }
    public enum CostingCheckedStatus
    {
        Hold, Reject, Approved
    }
    public enum CostingType
    {

        CostingType1,
        CostingType2,
        CostingType3
    }
    public enum ChargesType
    {
        Acceptance,
        [Description("LC Opening")]
        Open,
        [Description("LC Amendment")]
        Amendment,
        Service
    }

    public enum SpecifyTo
    {
        Common,
        Customer
    }
    public enum PackingType
    {
        Solid,
        Assorted
    }

    public enum CostingSegment
    {
        [Description("Direct Meterial")]
        DirectMaterial,
        [Description("Direct Process")]
        DirectProcess,
        [Description("Operation")]
        Operation,
        [Description("Value Loss")]
        ValueLoss,
        [Description("Sales Expense")]
        SalesExpense,
        [Description("Profit")]
        Profit,
        FOB
    }

    public enum AcceptancePaymentSource
    {
        Loan,
        Self
    }
    public enum InquiryProcessEnum
    {    
        [Description("Sample-1")]
        Sample1,
        [Description("Sample-2")]
        Sample2,
        [Description("Sample-3")]
        Sample3,
        [Description("Sample-4")]
        Sample4,
        [Description("Sample-5")]
        Sample5,
        [Description("Sample-6")]
        Sample6,
        [Description("Sample-7")]
        Sample7,
    }

    public enum TransactionTypeEnum
    {
        JobWork,
        Inventory,
        Service
    }
}
