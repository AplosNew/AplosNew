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
        //.when('/contracts', {
        //    templateUrl: 'Commercial/contract/Aplos1',
        //    controller: 'contractNewController'
        //})
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
        .when('/masterlc-amendment', {
            templateUrl: 'Commercial/contract/MasterLCAmendment',
            controller: 'masterLCAmendmentController'
        })
        .when('/fund', {
            templateUrl: 'Commercial/ContractFundUtilization/Aplos',
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
        .when('/proforma-invoice', {
            templateUrl: 'Commercial/ProformaInvoice/Aplos',
            controller: 'ProformaInvoiceController'
        })
        .when('/invoice-tagged-with-lc', {
            templateUrl: 'Commercial/InvoiceTaggedWithLC/Aplos',
            controller: 'InvoiceTaggedWithLCController'
        })
        .when('/invoice-to-acceptance-post', {
            templateUrl: 'Commercial/InvoiceToAcceptancePost/InvoiceToAcceptancePost',
            controller: 'invoiceToAcceptancePostController'
        })

        .when('/pi-invoice', {
            templateUrl: 'Commercial/PIInvoice/Aplos',
            controller: 'PIInvoiceController'
        })
        .when('/pi-packing-list', {
            templateUrl: 'Commercial/PIPackingList/Aplos',
            controller: 'PIPackingListController'
        })
        .when('/po-mapping-with-pi', {
            templateUrl: 'Commercial/POMappingWithPI/Aplos',
            controller: 'POMappingWithPIController'
        })
        .when('/commercial-add-info', {
            templateUrl: 'Commercial/CommercialAdditionalInfo/Aplos',
            controller: 'CommercialAdditionalInfoController'
        })
        .when('/btb-performance', {
            templateUrl: 'Commercial/LCReports/btb',
            controller: 'BtbPerformanceController'
        })

        .when("/invoice-status", {
            templateUrl: "Commercial/PostSalesInvoice/InvoiceStatus",
            controller: "InvoiceStatusController"
        })
        .when("/lcpendingreport", {
            templateUrl: "Commercial/Contract/LCPendingReport",
            controller: "LCPendingReportController"
        })
        .when('/contract-summary', {
            templateUrl: 'Commercial/contract/ContractSummary',
            controller: 'ContractSummaryController'
        })
        .when('/compliance', {
            templateUrl: 'Commercial/Compliance/Aplos',
            controller: 'ComplianceController'
        })
        .when('/compliance-transaction', {
            templateUrl: 'Commercial/Compliance/Transaction',
            controller: 'ComplianceTransactionController'
        })

        .when('/compliance-audit', {
            templateUrl: 'Commercial/Compliance/Audit',
            controller: 'ComplianceAuditController'
        });
}