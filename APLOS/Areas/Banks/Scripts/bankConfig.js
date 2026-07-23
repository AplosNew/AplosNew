bankConfig.$inject = ["$routeProvider", "$locationProvider"];
function bankConfig($routeProvider, $locationProvider) {
    $routeProvider
        .when("/bank", {
            templateUrl: "Banks/Bank/Bank",
            controller: "bankController"
        })
        .when("/bank-account-type", {
            templateUrl: "Banks/BankAccountType/BankAccountType",
            controller: "bankAccountTypeController"
        })
        .when("/bank-branch", {
            templateUrl: "Banks/BankBranch/BankBranch",
            controller: "bankBranchController"
        })
        .when("/bank-category", {
            templateUrl: "Banks/BankCategory/BankCategory",
            controller: "bankCategoryController"
        })
        .when("/bank-sub-category", {
            templateUrl: "Banks/BankSubCategory/BankSubCategory",
            controller: "bankSubCategoryController"
        })
        .when("/bank-master", {
            templateUrl: "Banks/BankMaster/BankMaster",
            controller: "bankMasterController"
        })
        .when("/bank-ledger", {
            templateUrl: "Banks/BankReport/BankLedgerReport",
            controller: "bankLedgerReportController"
        })
        .when("/bank-reconcile-ledger", {
            templateUrl: "Banks/BankReport/BankReconcileReport",
            controller: "bankReconcileReportController"
        })
        .when("/bank-book", {
            templateUrl: "Banks/BankReport/BankBookReport",
            controller: "bankBookReportController"
        })
        .when("/bank-ob-ledger", {
            templateUrl: "Banks/BankReport/BankOpeningBalanceLedger",
            controller: "bankOpeningBalanceLedgerController"
        })
        .when("/bank-reconciliation", {
            templateUrl: "Banks/bankreconciliation/BankReconciliation",
            controller: "bankReconciliationController"
        })
        .when("/bank-reconciliation-closing", {
            templateUrl: "Banks/bankreconciliation/BankReconciliationClosing",
            controller: "bankReconciliationClosingController"
        })
        .when("/bank-reconciliation-data-upload", {
            templateUrl: "Banks/bankreconciliation/BankReconciliationDataUpload",
            controller: "bankReconciliationDataUploadController"
        })
        .when("/bank-reconciliation-data-upload-reconciled", {
            templateUrl: "Banks/bankreconciliation/BankReconciliationDataUploadReconciled",
            controller: "bankReconciliationDataUploadReconciledController"
        })
        .when("/bank-journal", {
            templateUrl: "Banks/BankJournal/BankJournal",
            controller: "bankJournalController"
        })
        .when("/cash-master", {
            templateUrl: "Banks/CashMaster/CashMaster",
            controller: "cashMasterController"
        })
        .when("/cash-opening-balance", {
            templateUrl: "Banks/CashOpeningBalance/CashOpeningBalance",
            controller: "cashOpeningBalanceController"
        })
        .when("/cash-ledger", {
            templateUrl: "Banks/CashReport/CashLedgerReport",
            controller: "cashLedgerReportController"
        })
        .when("/cash-book", {
            templateUrl: "Banks/CashReport/CashBookReport",
            controller: "cashBookReportController"
        })
        .when("/cash-ob-ledger", {
            templateUrl: "Banks/CashReport/CashOpeningBalanceLedger",
            controller: "cashOpeningBalanceLedgerController"
        })
        .when("/cash-journal", {
            templateUrl: "Banks/CashJournal/CashJournal",
            controller: "cashJournalController"
        })
        .when("/entity-expense-booking", {
            templateUrl: "Banks/CashJournal/EntityExpenseBooking",
            controller: "entityExpenseBookingController"
        })
        .when("/entity-expense-booking-approval", {
            templateUrl: "Banks/CashJournal/EntityExpenseBookingApproval",
            controller: "entityExpenseBookingApprovalController"
        })
        .when("/check-lot", {
            templateUrl: "Banks/CheckManagement/CheckLot",
            controller: "checkLotController"
        })
        .when("/print-non-cash-check", {
            templateUrl: "Banks/CheckManagement/PrintNonCashCheck",
            controller: "printNonCashCheckController"
        })
        .when("/print-cash-check", {
            templateUrl: "Banks/CheckManagement/PrintCashCheck",
            controller: "printCashCheckController"
        })
        .when("/bank-opening-balance", {
            templateUrl: "Banks/BankOpeningBalance/BankOpeningBalance",
            controller: "bankOpeningBalanceController"
        })
        .when("/bank-charge-type", {
            templateUrl: "Banks/BankChargeType/BankChargeType",
            controller: "bankChargeTypeController"
        })
        .when("/bank-charge-type-gl", {
            templateUrl: "Banks/BankChargeType/BankChargeTypeGL",
            controller: "bankChargeTypeGLController"
        })
        .when("/payment-by-bank", {
            templateUrl: "Banks/BankJournal/PaymentByBank",
            controller: "paymentByBankController"
        })
        .when("/payment-by-cash", {
            templateUrl: "Banks/CashJournal/PaymentByCash",
            controller: "paymentByCashController"
        })
        .when("/receipt-by-bank", {
            templateUrl: "Banks/BankJournal/ReceiptByBank",
            controller: "receiptByBankController"
        })
        .when("/receipt-by-cash", {
            templateUrl: "Banks/CashJournal/ReceiptByCash",
            controller: "receiptByCashController"
        })

        .when("/cash-receipt-payment-report", {
            templateUrl: "Banks/CashReport/CashReceiptPaymentReport",
            controller: "cashReceiptPaymentReportController"
        })
        .when("/reprint-non-cash-check", {
            templateUrl: "Banks/CheckManagement/RePrintNonCashCheck",
            controller: "rePrintNonCashCheckController"
        })

        .when("/reprint-cash-check", {
            templateUrl: "Banks/CheckManagement/RePrintCashCheck",
            controller: "rePrintCashCheckController"
        })


       .when("/check-void", {
           templateUrl: "Banks/CheckManagement/CheckVoid",
           controller: "checkVoidController"
       })

        .when("/check-management-report", {
            templateUrl: "Banks/CheckManagement/CheckManagementReport",
            controller: "checkManagementReportController"
        })

        .when("/bank-sheet-generation", {
            templateUrl: "Banks/BankReport/BankSheetGeneration",
            controller: "bankSheetGenerationController"
        })
        .when("/pdc", {
            templateUrl: "Banks/CheckManagement/PostDateCheque",
            controller: "postDateChequeController"
        })
        .when("/current-fund-position-report", {
            templateUrl: "Banks/BankJournal/CurrentFundPosition",
            controller: "CurrentFundPositionController"
        })
        .when("/bank-settlement-reco", {
            templateUrl: "Banks/bankreconciliation/BankSettlementReconciliation",
            controller: "bankSettlementReconciliationController"
        })
        .when("/bank-settle-customerAdvance/:id", {
            templateUrl: "Banks/bankreconciliation/BankSettlementCustomerAdvance",
            controller: "BankSettlementCustomerAdvanceController"
        })
        .when("/bank-settle-customerReceipt/:id", {
            templateUrl: "Banks/bankreconciliation/BankSettlementCustomerReceipt",
            controller: "bankSettlementCustomerReceiptController"
        })
        .when("/bank-settle-journal/:id", {
            templateUrl: "Banks/bankreconciliation/BankSettlementJournal",
            controller: "bankSettlementJournalController"
        })
        .when("/monthly-expenses-asset", {
            templateUrl: "Banks/CashReport/MonthlyExpenseAndAssetStatement",
            controller: "monthlyExpensesAndAssetStatementController"
        })
        ;
}