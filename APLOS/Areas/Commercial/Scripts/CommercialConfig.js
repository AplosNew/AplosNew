CommercialConfig.$inject = ['$routeProvider'];
function CommercialConfig($routeProvider) {
    $routeProvider

        .when('/overheadtypegl-sales', {
            templateUrl: 'Commercial/OverHeadTypeGL/Aplos',
            controller: 'OverHeadTypeGLSalesController'
        })
        .when('/overheadtypegl-purchases', {
            templateUrl: 'Commercial/OverHeadTypeGL/Aplos',
            controller: 'OverHeadTypeGLParachasController'
        })
        .when('/overhead-type', {
            templateUrl: 'Commercial/OverHeadType/Aplos',
            controller: 'OverHeadTypeController'
        })
        .when('/contract', {
            templateUrl: 'Commercial/contract/Aplos',
            controller: 'contractController'
        })
        .when('/purchaselc', {
            templateUrl: 'Commercial/PurchaseLC/Aplos',
            controller: 'purchaseLCController'
        })

        .when('/CNF-Expense-Bocking', {
            templateUrl: 'Commercial/ServiceMasterCharges/CNFExpenseBocking',
            controller: 'CNFExpenseBockingController'
        })
        .when('/purchaselc-chargesPost', {
            templateUrl: 'Commercial/PurchaseLC/PurchaseLCChargesPost',
            controller: 'purchaseLCChargesPostController'
        })

        .when('/purchaselc-amendment', {
            templateUrl: 'Commercial/PurchaseLCAmendment/Aplos',
            controller: 'purchaseLCAmendmentController'
        })
        .when('/masterlc', {
            templateUrl: 'Commercial/contract/masterlc',
            controller: 'masterLCController'
        })
        .when('/fund', {
            templateUrl: 'Commercial/LCFundUtilization/Aplos',
            controller: 'contractFundUtilizationController'
        })
        .when('/prepurchase-invoice', {
            templateUrl: 'Commercial/PrePurchaseInvoice/Aplos',
            controller: 'PrePurchaseInvoiceController'
        })
        .when('/lcpo', {
            templateUrl: 'Commercial/PurchaseLCWithPO/Aplos',
            controller: 'PurchaseLCWithPOController'
        })

        .when('/lc-reports', {
            templateUrl: 'Commercial/LCReports/Aplos',
            controller: 'LCReportsController'
        })
        .when('/lc-Navigation', {
            templateUrl: 'Commercial/LcNavigation/Aplos',
            controller: 'LcNavigationController'
        })
        .when('/invoice-budget', {
            templateUrl: 'Commercial/InvoiceBudgetChargesSetting/Aplos',
            controller: 'InvoiceBudgetChargesSettingController'
        })
        .when('/post-sales-invoice', {
            templateUrl: 'Commercial/PostSalesInvoice/Aplos',
            controller: 'PostSalesInvoiceController'
        })

        .when('/auto-loan', {
            templateUrl: 'Commercial/AutoLoan/Aplos',
            controller: 'autoLoanController'
        })
        .when('/auto-loan-post', {
            templateUrl: 'Commercial/AutoLoan/AutoLoanPost',
            controller: 'autoLoanPostController'
        })
        ;





}