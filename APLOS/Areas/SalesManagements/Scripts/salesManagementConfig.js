salesManagementConfig.$inject = ["$routeProvider"];
function salesManagementConfig($routeProvider) {
    $routeProvider
        .when("/sales", {
            templateUrl: "SalesManagements/Sales/Sales",
            controller: "salesController"
        })
        .when("/sales-invoice-pending", {
            templateUrl: "SalesManagements/Sales/SalesInvoicePending",
            controller: "salesInvoicePendingController"
        })
        .when("/sales-invoice", {
            templateUrl: "SalesManagements/SalesInvoice/SalesInvoice",
            controller: "salesInvoiceController"
        })
        .when("/master-order-sales", {
            templateUrl: "SalesManagements/Sales/MasterOrderSales",
            controller: "masterOrderSalesController"
        })

        .when("/masterorder-sales-post", {
            templateUrl: "SalesManagements/Sales/MasterOrderSalesPost",
            controller: "masterOrderSalesPostController"
        })
        .when("/sales-packing-post", {
            templateUrl: "SalesManagements/Sales/SalesPackingPost",
            controller: "salesPackingPostController"
        })
        .when("/sales-incentive", {
            templateUrl: "SalesManagements/Sales/SalesIncentive",
            controller: "salesIncentiveController"
        })
        .when("/einvoice", {
            templateUrl: "SalesManagements/Sales/EInvoice",
            controller: "EInvoiceController"
        })
        .when("/invoice-add-info", {
            templateUrl: "SalesManagements/Sales/AdditionalInfo",
            controller: "masterOrderSalesAdditionalController"
        })
        .when("/sales-return", {
            templateUrl: "SalesManagements/Sales/SalesReturn",
            controller: "SalesReturnController"
        })
        .when("/sales-return-post", {
            templateUrl: "SalesManagements/Sales/SalesReturnPost",
            controller: "SalesReturnPostController"
        })
        .when("/addinfo", {
            templateUrl: "SalesManagements/AdditionalInfo/Aplos",
            controller: "AdditionalInfoController"
        })
        .when("/sales-chalan", {
            templateUrl: "SalesManagements/SalesChalan/Aplos",
            controller: "SalesChalanController"
        })
        .when("/sales-chalan-check", {
            templateUrl: "SalesManagements/SalesChalan/SalesChalanCheck",
            controller: "SalesChanlanCheckedController"
        })
        .when("/sales-chalan-dis-con", {
            templateUrl: "SalesManagements/SalesChalan/DispatchConfirmation",
            controller: "SalesChanlanDispatchConfirmationController"
        })
        .when("/input-credit", {
            templateUrl: "SalesManagements/Sales/InputCredit",
            controller: "InputCreditController"
        })
        .when("/input-credit-check", {
            templateUrl: "SalesManagements/Sales/InputCreditCheck",
            controller: "InputCreditCheckController"
        })
        .when("/input-credit-approve", {
            templateUrl: "SalesManagements/Sales/InputCreditApprove",
            controller: "InputCreditApproveController"
        })
        .when("/sales-process", {
            templateUrl: "SalesManagements/Sales/SalesProcess",
            controller: "SalesProcessController"
        })
        ;
}