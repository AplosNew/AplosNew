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
        .when("/einvoice", {
            templateUrl: "SalesManagements/Sales/EInvoice",
            controller: "EInvoiceController"
        })
        ;
}