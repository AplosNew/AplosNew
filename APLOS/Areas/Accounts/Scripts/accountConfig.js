accountConfig.$inject = ["$routeProvider"];
function accountConfig($routeProvider) {
    $routeProvider
        .when("/account-dashboard", {
            templateUrl: "Accounts/AccountDashboard/aplos",
            controller: "accountDashboardController"
        })
        .when("/chart-of-account-level1", {
            templateUrl: "Accounts/chartofaccountlevel1",
            controller: "chartOfAccountLevel1Controller"
        })
        .when("/chart-of-account-level2", {
            templateUrl: "Accounts/chartofaccountlevel2",
            controller: "chartOfAccountLevel2Controller"
        })
        .when("/chart-of-account-level3", {
            templateUrl: "Accounts/chartofaccountlevel3/aplos",
            controller: "chartOfAccountLevel3Controller"
        })
        .when("/chart-of-account-level4", {
            templateUrl: "Accounts/chartofaccountlevel4/aplos",
            controller: "chartOfAccountLevel4Controller"
        })
        .when("/chart-of-account-level5", {
            templateUrl: "Accounts/chartofaccountlevel5/aplos",
            controller: "chartOfAccountLevel5Controller"
        })
        .when("/chart-of-account-level6", {
            templateUrl: "Accounts/chartofaccountlevel6/aplos",
            controller: "chartOfAccountLevel6Controller"
        })
        .when("/coa-relationship", {
            templateUrl: "Accounts/chartOfAccountRelationship/aplos",
            controller: "chartOfAccountRelationshipController"
        })
        .when("/company-tax-year", {
            templateUrl: "Accounts/companyTaxYear/aplos",
            controller: "companyTaxYearController"
        })
        .when("/country-tax-year", {
            templateUrl: "Accounts/countryTaxYear/aplos",
            controller: "countryTaxYearController"
        })
        .when("/company-tax-year-period", {
            templateUrl: "Accounts/companyTaxYearPeriod/aplos",
            controller: "companyTaxYearPeriodController"
        })
        .when("/country-tax-year-period", {
            templateUrl: "Accounts/countryTaxYearPeriod/aplos",
            controller: "countryTaxYearPeriodController"
        })
        .when("/fiscal-year", {
            templateUrl: "Accounts/FiscalYear/Aplos",
            controller: "fiscalYearController"
        })
        .when("/fiscal-year-period", {
            templateUrl: "Accounts/FiscalYear/FiscalYearPeriod",
            controller: "fiscalYearPeriodController"
        })
        .when("/company-fiscal-year", {
            templateUrl: "Accounts/FiscalYear/CompanyFiscalYear",
            controller: "companyFiscalYearController"
        })
        .when("/company-fiscal-year-period", {
            templateUrl: "Accounts/FiscalYear/CompanyFiscalYearPeriod",
            controller: "companyFiscalYearPeriodController"
        })
        .when("/tax-fiscal-year", {
            templateUrl: "Accounts/taxyear/aplos",
            controller: "taxYearController"
        })
        .when("/tax-fiscal-year-period", {
            templateUrl: "Accounts/taxyearperiod/aplos",
            controller: "taxYearPeriodController"
        })
        .when("/fiscal-year-close", {
            templateUrl: "Accounts/fiscalYearClose/FiscalYearClose",
            controller: "fiscalYearCloseController"
        })
        .when("/fiscal-year-close-post", {
            templateUrl: "Accounts/fiscalYearClose/FiscalYearClosePost",
            controller: "fiscalYearClosePostController"
        })
        .when("/voucher-type", {
            templateUrl: "Accounts/voucherType/VoucherType",
            controller: "voucherTypeController"
        })
        .when("/voucher-type-matrix", {
            templateUrl: "Accounts/voucherType/VoucherTypeMatrix",
            controller: "voucherTypeMatrixController"
        })
        .when("/voucher-type-config", {
            templateUrl: "Accounts/voucherType/VoucherTypeConfig",
            controller: "voucherTypeConfigController"
        })
        .when("/payment-term", {
            templateUrl: "Accounts/paymentterm/aplos",
            controller: "paymentTermController"
        })
        .when("/tax-code", {
            templateUrl: "Accounts/taxcode/aplos",
            controller: "taxCodeController"
        })
        .when("/tax-code-year", {
            templateUrl: "Accounts/taxcode/taxcodeyear",
            controller: "taxCodeYearController"
        })
        .when("/exchange-gainloss", {
            templateUrl: "Accounts/ExchangeGainLoss/ExchangeGainLoss",
            controller: "exchangeGainLossController"
        })
        .when("/rounding-gl", {
            templateUrl: "Accounts/FinancingType/RoundingGL",
            controller: "roundingGLController"
        })
        .when("/coa", {
            templateUrl: "Accounts/coa/aplos",
            controller: "coaController"
        })
        .when("/alternative-coa", {
            templateUrl: "Accounts/alternativecoa/AlternativeCOA",
            controller: "alternativeCOAController"
        })
        .when("/account-group", {
            templateUrl: "Accounts/accountgroup/aplos",
            controller: "accountGroupController"
        })
        .when("/account-type", {
            templateUrl: "Accounts/accounttype/aplos",
            controller: "accountTypeController"
        })
        .when("/gl-item", {
            templateUrl: "Accounts/glitem/aplos",
            controller: "glItemController"
        })
        .when("/gl-mapping", {
            templateUrl: "Accounts/GLItem/GLMapping",
            controller: "glMappingController"
        })
        .when("/gl-companyinfo", {
            templateUrl: "Accounts/glitem/glCompanyInfo",
            controller: "glCompanyInfoController"
        })
        .when("/gl-report", {
            templateUrl: "Accounts/glitem/GeneralLedgerListReport",
            controller: "generalLedgerListReportController"
        })
        .when("/alternativegl", {
            templateUrl: "Accounts/alternativegl/AlternativeGL",
            controller: "alternativeGLController"
        })
        .when("/journal", {
            templateUrl: "Accounts/voucher/journal",
            controller: "journalController"
        })
        .when("/advance-journal", {
            templateUrl: "Accounts/voucher/AdvanceJournal",
            controller: "advanceJournalController"
        })
        .when("/normal-journal", {
            templateUrl: "Accounts/voucher/NormalJournal",
            controller: "normalJournalController"
        })
        .when("/pfesic-disbursement", {
            templateUrl: "Accounts/voucher/PFESICDisbursement",
            controller: "pfesiDisbursementController"
        })
        .when("/exchange-voucher", {
            templateUrl: "Accounts/voucher/exchangevoucher",
            controller: "exchangeVoucherController"
        })
        .when("/customer-invoice", {
            templateUrl: "Accounts/Invoice/CustomerInvoice",
            controller: "customerInvoiceController"
        })
        .when("/customer-invoice-write-off", {
            templateUrl: "Accounts/Advance/CustomerInvoiceWriteOff",
            controller: "customerInvoiceWriteOffController"
        })
        .when("/customer-invoice-settlement", {
            templateUrl: "Accounts/Invoice/CustomerInvoiceSettlement",
            controller: "customerInvoiceSettlementController"
        })
       
        .when("/invoice-charge-write-off", {
            templateUrl: "Accounts/Advance/InvoiceChargeWriteOff",
            controller: "invoiceChargeWriteOffController"
        })
        .when("/debit-note", {
            templateUrl: "Accounts/AdjustmentNote/DebitNote",
            controller: "debitNoteController"
        })
        .when("/debit-note-type", {
            templateUrl: "Accounts/FinancingType/DebitNoteType",
            controller: "debitNoteTypeController"
        })
        .when("/debit-note-type-gl", {
            templateUrl: "Accounts/FinancingType/DebitNoteTypeGL",
            controller: "debitNoteTypeGLController"
        })
        .when("/debit-note-setoff", {
            templateUrl: "Accounts/AdjustmentNote/DebitNoteSetOff",
            controller: "debitNoteSetOffController"
        })
        .when("/credit-note-type", {
            templateUrl: "Accounts/FinancingType/CreditNoteType",
            controller: "creditNoteTypeController"
        })
        .when("/credit-note-type-gl", {
            templateUrl: "Accounts/FinancingType/CreditNoteTypeGL",
            controller: "creditNoteTypeGLController"
        })
        .when("/credit-note", {
            templateUrl: "Accounts/AdjustmentNote/CreditNote",
            controller: "creditNoteController"
        })
        .when("/credit-note-setoff", {
            templateUrl: "Accounts/AdjustmentNote/CreditNoteSetOff",
            controller: "creditNoteSetOffController"
        })
        .when("/customer-trantype", {
            templateUrl: "Accounts/FinancingType/CustomerTranType",
            controller: "customerTranTypeController"
        })
        .when("/customer-trantype-gl", {
            templateUrl: "Accounts/FinancingType/CustomerTranTypeGL",
            controller: "customerTranTypeGLController"
        })
        .when("/vendor-trantype", {
            templateUrl: "Accounts/FinancingType/VendorTranType",
            controller: "vendorTranTypeController"
        })
        .when("/vendor-trantype-gl", {
            templateUrl: "Accounts/FinancingType/VendorTranTypeGL",
            controller: "vendorTranTypeGLController"
        })
        //.when("/customer-advance/:advanceId?", {
        //    templateUrl: "Accounts/Advance/CustomerAdvance",
        //    controller: "customerAdvanceController"
        //})
        .when("/customer-advance", {
            templateUrl: "Accounts/Advance/CustomerAdvance",
            controller: "customerAdvanceController"
        })
        .when("/customer-invoice-receipt", {
            templateUrl: "Accounts/Invoice/CustomerInvoiceReceipt",
            controller: "customerInvoiceReceiptController"
        })
        .when("/customer-invoice-banksreceipt", {
            templateUrl: "Accounts/Invoice/CustomerInvoiceBanksReceipt",
            controller: "customerInvoiceBanksReceiptController"
        })
        .when("/customer-payment", {
            templateUrl: "Accounts/Advance/CustomerPayment",
            controller: "customerPaymentController"
        })
        .when("/customer-advance-write-off", {
            templateUrl: "Accounts/Advance/CustomerAdvanceWriteOff",
            controller: "customerAdvanceWriteOffController"
        })
        //.when("/customer-advance-write-off/:advanceId?", {
        //    templateUrl: "Accounts/Advance/CustomerAdvanceWriteOff",
        //    controller: "customerAdvanceWriteOffController"
        //})
        //.when("/customer-suspense/:advanceId?", {
        //    templateUrl: "Accounts/Advance/CustomerSuspense",
        //    controller: "customerSuspenseController"
        //})
        //.when("/customer-suspense-write-off/:advanceId?", {
        //    templateUrl: "Accounts/Advance/CustomerSuspenseWriteOff",
        //    controller: "customerSuspenseWriteOffController"
        //})
        .when("/customer-suspense", {
            templateUrl: "Accounts/Advance/CustomerSuspense",
            controller: "customerSuspenseController"
        })
        .when("/customer-suspense-write-off", {
            templateUrl: "Accounts/Advance/CustomerSuspenseWriteOff",
            controller: "customerSuspenseWriteOffController"
        })
        .when("/int-sales-order-invoice", {
            templateUrl: "Accounts/voucher/IntSalesOrderInvoice",
            controller: "intSalesOrderInvoiceController"
        })
        .when("/sales-order-edit-invoice", {
            templateUrl: "Accounts/voucher/IntSalesOrderInvoiceEdit",
            controller: "intSalesOrderInvoiceEditController"
        })
        .when("/sales-order-invoice-post/:id", {
            templateUrl: "Accounts/voucher/IntSalesOrderInvoicePost",
            controller: "intSalesOrderInvoicePostController"
        })
        .when("/general-ledger", {
            templateUrl: "Accounts/Voucher/GeneralLedgerReport",
            controller: "generalLedgerReportController"
        })
        .when("/lc-ledger-report", {
            templateUrl: "Accounts/Voucher/LCLedgerReport",
            controller: "lcLedgerReportController"
        })
        .when("/gst-ledger", {
            templateUrl: "Accounts/Voucher/GSTLedgerReport",
            controller: "generalLedgerGSTReportController"
        })
        .when("/group-balance-report", {
            templateUrl: "Accounts/GroupBalanceReport/Aplos",
            controller: "GroupBalanceReportController"
        })

        .when("/general-ob-ledger", {
            templateUrl: "Accounts/Voucher/GeneralLedgerOpeningBalanceReport",
            controller: "generalLedgerOpeningBalanceReportController"
        })
        .when("/trialbalance-report", {
            templateUrl: "Accounts/Voucher/TrialBalanceReportPage",
            controller: "trialBalanceReportController"
        })
        .when("/incomestatement-report", {
            templateUrl: "Accounts/voucher/incomestatementreportpage",
            controller: "incomeStatementReportController"
        })
        .when("/entity-wise-expense-earning-report", {
            templateUrl: "Accounts/voucher/EntityWiseExpenseAndEarning",
            controller: "entityWiseExpenseAndEarningController"
        })
    

        .when("/balancesheet-report", {
            templateUrl: "Accounts/voucher/BalanceSheetReportPage",
            controller: "balanceSheetReportController"
        })
        .when("/balancesheet-report-treeview/:FromDate", {
            templateUrl: "Accounts/voucher/BalanceSheetReportTreeView",
            controller: "balanceSheetReportTreeViewController"
        })
        .when("/ob-balance-sheet", {
            templateUrl: "Accounts/voucher/balancesheetopeningbalancereport",
            controller: "balanceSheetOpeningBalanceReportController"
        })
        .when("/balancesheet-details-report", {
            templateUrl: "Accounts/voucher/balancesheetdetailsreportpage",
            controller: "balanceSheetDetailsReportController"
        })
        .when("/ob-fixed-asset-report", {
            templateUrl: "Accounts/voucher/fixedassetobreport",
            controller: "fixedAssetObReportController"
        })
        .when("/management-chart-of-account-ex", {
            templateUrl: "Accounts/managementchartofaccount/mcaexpense",
            controller: "managementChartofAccountEXController"
        })
        .when("/management-chart-of-account-fa", {
            templateUrl: "Accounts/managementchartofaccount/mcafixedasset",
            controller: "managementChartofAccountFAController"
        })
        
        .when("/vendor-payment", {
            templateUrl: "Accounts/Invoice/VendorPayment",
            controller: "vendorPaymentController"
        })
        .when("/vendor-pay-app", {
            templateUrl: "Accounts/Invoice/VendorPaymentApproval",
            controller: "vendorPaymentApprovalController"
        })
        .when("/tax-payment", {
            templateUrl: "Accounts/InvoiceTax/TaxPayment",
            controller: "taxPaymentController"
        })
        .when("/tax-payable-report", {
            templateUrl: "Accounts/InvoiceTax/TaxPayableReport",
            controller: "taxPayableReportController"
        })
        .when("/vendor-advance", {
            templateUrl: "Accounts/Advance/VendorAdvance",
            controller: "vendorAdvanceController"
        })
        .when("/vendor-advance-write-off", {
            templateUrl: "Accounts/Advance/VendorAdvanceWriteOff",
            controller: "vendorAdvanceWriteOffController"
        })
        .when("/acc-cutoff-date-opening-balance", {
            templateUrl: "Accounts/OpeningBalance/ACCCutOffDate",
            controller: "accCutOffDateOpeningBalanceController"
        })
        .when("/hr-cutoff-date-opening-balance", {
            templateUrl: "Accounts/OpeningBalance/HRCutOffDate",
            controller: "hrCutOffDateOpeningBalanceController"
        })
        .when("/vendor-invoice-opening-balance", {
            templateUrl: "Accounts/OpeningBalance/VendorInvoice",
            controller: "vendorInvoiceOpeningBalanceController"
        })
        .when("/vendor-advance-opening-balance", {
            templateUrl: "Accounts/OpeningBalance/VendorAdvance",
            controller: "vendorAdvanceOpeningBalanceController"
        })
        .when("/customer-advance-opening-balance", {
            templateUrl: "Accounts/OpeningBalance/CustomerAdvance",
            controller: "customerAdvanceOpeningBalanceController"
        })
        .when("/customer-invoice-opening-balance", {
            templateUrl: "Accounts/OpeningBalance/CustomerInvoice",
            controller: "customerInvoiceOpeningBalanceController"
        })
        .when("/loan-type", {
            templateUrl: "Accounts/FinancingType/LoanType",
            controller: "loanTypeController"
        })
        .when("/loan-type-gl", {
            templateUrl: "Accounts/FinancingType/LoanTypeGL",
            controller: "loanTypeGLController"
        })
        .when("/loan", {
            templateUrl: "Accounts/loan/loan",
            controller: "loanController"
        })
        .when("/loan-repayment", {
            templateUrl: "Accounts/loan/LoanPayment",
            controller: "loanPaymentController"
        })
        .when("/loan-close", {
            templateUrl: "Accounts/loan/LoanClose",
            controller: "loanCloseController"
        })
        .when("/loan-interest-payable", {
            templateUrl: "Accounts/loan/LoanInterestPayable",
            controller: "loanInterestPayableController"
        })
        .when("/loaninterest-payable-reverse", {
            templateUrl: "Accounts/loan/LoanInterestPayableReverse",
            controller: "loanInterestPayableReverseController"
        })
        .when("/loan-taken-opening-balance", {
            templateUrl: "Accounts/OpeningBalance/loanTaken",
            controller: "loanTakenOpeningBalanceController"
        })
        .when("/loan-given-opening-balance", {
            templateUrl: "Accounts/OpeningBalance/loanGiven",
            controller: "loanGivenOpeningBalanceController"
        })
        .when("/inter-loan-given-opening-balance", {
            templateUrl: "Accounts/OpeningBalance/InterLoanGiven",
            controller: "interLoanGivenOpeningBalanceController"
        })
        .when("/inter-plant-loan-taken-opening-balance", {
            templateUrl: "Accounts/OpeningBalance/InterPlantLoanTaken",
            controller: "interPlantLoanTakenOpeningBalanceController"
        })
        .when("/inter-company-loan-taken-opening-balance", {
            templateUrl: "Accounts/OpeningBalance/InterCompanyLoanTaken",
            controller: "interCompanyLoanTakenOpeningBalanceController"
        })
        .when("/investment-type", {
            templateUrl: "Accounts/FinancingType/InvestmentType",
            controller: "investmentTypeController"
        })
        .when("/investment-type-gl", {
            templateUrl: "Accounts/FinancingType/InvestmentTypeGL",
            controller: "investmentTypeGLController"
        })
        .when("/investment", {
            templateUrl: "Accounts/Investment/Investment",
            controller: "investmentController"
        })
        .when("/investment-settelment", {
            templateUrl: "Accounts/Investment/InvestmentSettelment",
            controller: "InvestmentSettelmentController"
        })
        .when("/inter-investment-given-opening-balance", {
            templateUrl: "Accounts/OpeningBalance/interInvestmentGiven",
            controller: "interInvestmentGivenOpeningBalanceController"
        })
        .when("/inter-plant-investment-opening-balance", {
            templateUrl: "Accounts/openingbalance/InterPlantInvestmentTaken",
            controller: "interPlantInvestmentTakenOpeningBalanceController"
        })
        .when("/inter-company-investment-opening-balance", {
            templateUrl: "Accounts/openingbalance/InterCompanyInvestmentTaken",
            controller: "interCompanyInvestmentTakenOpeningBalanceController"
        })
        .when("/investment-taken-opening-balance", {
            templateUrl: "Accounts/OpeningBalance/investmentTaken",
            controller: "investmentTakenOpeningBalanceController"
        })
        .when("/investment-given-opening-balance", {
            templateUrl: "Accounts/OpeningBalance/investmentGiven",
            controller: "investmentGivenOpeningBalanceController"
        })
        .when("/inter-transaction-type", {
            templateUrl: "Accounts/FinancingType/InterTransactionType",
            controller: "interTransactionTypeController"
        })
        .when("/inter-transaction-type-gl", {
            templateUrl: "Accounts/FinancingType/InterTransactionTypeGL",
            controller: "interTransactionTypeGLController"
        })
        .when("/inter-transaction-given-opening-balance", {
            templateUrl: "Accounts/OpeningBalance/InterTransactionGiven",
            controller: "interTransactionGivenOpeningBalanceController"
        })
        .when("/inter-plant-transaction-opening-balance", {
            templateUrl: "Accounts/openingbalance/InterPlantTransactionTaken",
            controller: "interPlantTransactionTakenOpeningBalanceController"
        })
        .when("/inter-company-transaction-opening-balance", {
            templateUrl: "Accounts/openingbalance/InterCompanyTransactionTaken",
            controller: "interCompanyTransactionTakenOpeningBalanceController"
        })
        .when("/customer-interPlantCompany-receipt", {
            templateUrl: "Accounts/Invoice/CustomerInterPlantCompanyReceipt",
            controller: "customerInterPlantCompanyReceiptController"
        })
        .when("/inter-transaction", {
            templateUrl: "Accounts/Advance/InterTransaction",
            controller: "interTransactionController"
        })
        .when("/inter-advance-suspense-pending-transaction", {
            templateUrl: "Accounts/Advance/CustomerInterTransactionPending",
            controller: "customerInterTransactionPendingController"
        })
        .when("/employee-advance-opening-balance", {
            templateUrl: "Accounts/OpeningBalance/EmployeeAdvance",
            controller: "employeeAdvanceOpeningBalanceController"
        })
        .when("/employee-payable-opening-balance", {
            templateUrl: "Accounts/OpeningBalance/EmployeePayable",
            controller: "employeePayableOpeningBalanceController"
        })
        .when("/employee-advance", {
            templateUrl: "Accounts/advance/EmployeeAdvance",
            controller: "employeeAdvanceController"
        })
        .when("/employee-advance-reqPost", {
            templateUrl: "Accounts/advance/EmployeeAdvanceRequisitionPost",
            controller: "employeeAdvanceRequisitionPostController"
        })
        .when("/employee-advance-write-off", {
            templateUrl: "Accounts/advance/EmployeeAdvanceWriteOff",
            controller: "employeeAdvanceWriteOffController"
        })
        .when("/employee-total-advance-write-off", {
            templateUrl: "Accounts/advance/EmployeeTotalAdvanceWriteOff",
            controller: "employeeTotalAdvanceWriteOffController"
        })
        .when("/employee-transactiontype", {
            templateUrl: "Accounts/EmployeeTransaction/EmployeeTransactionType",
            controller: "employeeTransactionTypeController"
        })
        .when("/employee-transactiontype-gl", {
            templateUrl: "Accounts/EmployeeTransaction/EmployeeTransactionTypeGL",
            controller: "employeeTransactionTypeGLController"
        })
        .when("/employee-expenses-booking", {
            templateUrl: "Accounts/EmployeeTransaction/employeeExpensesBooking",
            controller: "employeeExpensesBookingController"
        })
        .when("/employee-payable", {
            templateUrl: "Accounts/EmployeePayable/EmployeePayable",
            controller: "employeePayableController"
        })
        .when("/employee-payment", {
            templateUrl: "Accounts/EmployeePayable/EmployeePayment",
            controller: "employeePaymentController"
        })
        .when("/fixed-asset-master-opening-balance", {
            templateUrl: "Accounts/OpeningBalance/FixedAssetMaster",
            controller: "fixedAssetMasterOpeningBalanceController"
        })
        .when("/material-master-opening-balance", {
            templateUrl: "Accounts/OpeningBalance/MaterialMaster",
            controller: "materialMasterOpeningBalanceController"
        })
        .when("/journal-opening-balance", {
            templateUrl: "Accounts/OpeningBalance/Journal",
            controller: "journalOpeningBalanceController"
        })
        .when("/advancejournal-opening-balance", {
            templateUrl: "Accounts/OpeningBalance/AdvanceJournal",
            controller: "advanceJournalOpeningBalanceController"
        })
        .when("/opening-balance-report", {
            templateUrl: "Accounts/OpeningBalance/Report",
            controller: "openingBalanceReportController"
        })
        .when("/tax-code-category", {
            templateUrl: "Accounts/TaxCategory/aplos",
            controller: "taxCategoryController"
        })
        .when("/tax-category-gl", {
            templateUrl: "Accounts/TaxCategoryGL/aplos",
            controller: "taxCategoryGLController"
        })
        .when("/tax-category-gl-output", {
            templateUrl: "Accounts/TaxCategoryGLOutput/aplos",
            controller: "taxCategoryGLOutputController"
        })
        .when("/tax-category-rcm", {
            templateUrl: "Accounts/TaxCategoryGL/TaxCategoryRCM",
            controller: "taxCategoryRCMController"
        })
        .when("/output-excluded-tax", {
            templateUrl: "Accounts/TaxCategoryGL/TaxCategoryRCMOutput",
            controller: "taxCategoryRCMOutputController"
        })
        .when("/invoice-deduction-type", {
            templateUrl: "Accounts/FinancingType/PaymentDeduction",
            controller: "paymentDeductionController"
        })
        .when("/payment-deduction-gl", {
            templateUrl: "Accounts/FinancingType/PaymentDeductionGL",
            controller: "paymentDeductionGLController"
        })
        .when("/security-given-opening-balance", {
            templateUrl: "Accounts/OpeningBalance/SecurityGiven",
            controller: "securityDepositGivenOpeningBalanceController"
        })
        .when("/security-taken-opening-balance", {
            templateUrl: "Accounts/OpeningBalance/SecurityTaken",
            controller: "securityDepositTakenOpeningBalanceController"
        })
        .when("/security-type", {
            templateUrl: "Accounts/FinancingType/SecurityType",
            controller: "securityTypeController"
        })
        .when("/security-type-gl", {
            templateUrl: "Accounts/FinancingType/SecurityTypeGL",
            controller: "securityTypeGLController"
        })
        .when("/security-deposit", {
            templateUrl: "Accounts/SecurityDeposit/SecurityDeposit",
            controller: "securityDepositController"
        })
        .when("/security-deposit-write-off", {
            templateUrl: "Accounts/SecurityDeposit/SecurityDepositWriteOff",
            controller: "securityDepositWriteOffController"
        })
        .when("/tax-code-gl", {
            templateUrl: "Accounts/TaxCodeGL/Aplos",
            controller: "taxCodeGLController"
        })
        .when("/tax-category-gl-output", {
            templateUrl: "Accounts/TaxCategoryGLOutput/aplos",
            controller: "taxCategoryGLOutputController"
        })
        .when("/register", {
            templateUrl: "Accounts/register/aplos",
            controller: "registerController"
        })
        .when("/budget", {
            templateUrl: "Accounts/BudgetMaster/Budget",
            controller: "budgetController"
        })
        .when("/budget-class", {
            templateUrl: "Accounts/BudgetMaster/BudgetClass",
            controller: "budgetClassController"
        })
        .when("/budget-group", {
            templateUrl: "Accounts/BudgetMaster/BudgetGroup",
            controller: "budgetGroupController"
        })
        .when("/budget-category", {
            templateUrl: "Accounts/BudgetMaster/BudgetCategory",
            controller: "budgetCategoryController"
        })
        .when("/budget-sub-category", {
            templateUrl: "Accounts/BudgetMaster/BudgetSubCategory",
            controller: "budgetSubCategoryController"
        })
        .when("/budget-activity", {
            templateUrl: "Accounts/budgetactivity/aplos",
            controller: "budgetActivityController"
        })
        .when("/budget-master", {
            templateUrl: "Accounts/BudgetMaster/aplos",
            controller: "budgetMasterController"
        })
        .when("/budget-master-register", {
            templateUrl: "Accounts/BudgetMaster/FARegister",
            controller: "budgetMasterFARegisterController"
        })
        .when("/budget-master-report", {
            templateUrl: "Accounts/BudgetMaster/BudgetMasterReport",
            controller: "budgetMasterReportController"
        })
        .when("/annual-budget", {
            templateUrl: "Accounts/AnnualBudget/aplos",
            controller: "annualBudgetController"
        })
        .when("/expense-booking", {
            templateUrl: "Accounts/ExpenseBooking/Aplos",
            controller: "expenseBookingController"
        })
        .when("/expense-booking-potal", {
            templateUrl: "Accounts/ExpenseBooking/ExpenseBookingPotal",
            controller: "expenseBookingPotalController"
        })
        .when("/expense-booking-approval-potal", {
            templateUrl: "Accounts/ExpenseBooking/ExpenseBookingApprovalPotal",
            controller: "expenseBookingApprovalPotalController"
        })
        .when("/expense-booking-approval", {
            templateUrl: "Accounts/ExpenseBooking/Approval",
            controller: "expenseBookingApprovalController"
        })
        .when("/expense-booking-approved/:id", {
            templateUrl: "Accounts/ExpenseBooking/Approved",
            controller: "expenseBookingApprovedController"
        })
        .when("/expense-booking-approvedList", {
            templateUrl: "Accounts/ExpenseBooking/ApprovedList",
            controller: "expenseBookingApprovedListController"
        })
        .when("/multiple-vendor-payment", {
            templateUrl: "Accounts/Invoice/MultipleVendorPayment",
            controller: "multipleVendorPaymentController"
        })
        .when("/multiple-vendor-payment-approved", {
            templateUrl: "Accounts/Invoice/MultipleVendorPaymentApproved",
            controller: "multipleVendorPaymentApprovedController"
        })
        .when("/activity", {
            templateUrl: "Accounts/activity/aplos",
            controller: "activityController"
        })
        .when("/activity-phone", {
            templateUrl: "Accounts/activityphone/aplos",
            controller: "activityPhoneController"
        })
        .when("/activity-phone-tagging", {
            templateUrl: "Accounts/activityphonetagging/aplos",
            controller: "activityPhoneTaggingController"
        })
        .when("/activity-responsible-rerson", {
            templateUrl: "Accounts/RoutineBudget/activity",
            controller: "activityResponsiblePersonController"
        })
        .when("/responsible-person/:entityId/:routineBudgetId/:activityId/:activityName", {
            templateUrl: "Accounts/RoutineBudget/ResponsiblePerson",
            controller: "responsiblePersonController"
        })
        .when("/expense-dashboard", {
            templateUrl: "Accounts/ExpenseDashboard/aplos",
            controller: "expenseDashboardController"
        })
        .when("/mis-account-dashboard", {
            templateUrl: "Accounts/MISAccountDashboard/Aplos",
            controller: "misAccountDashboardController"
        })
        .when("/tax-variant", {
            templateUrl: "Accounts/TaxCategory/TaxVariant",
            controller: "taxVariantController"
        })
        .when("/inventory-payable", {
            templateUrl: "Accounts/inventoryPayable/InventoryPayable",
            controller: "inventoryPayableController"
        })
        .when("/inventory-transfer-journal", {
            templateUrl: "Accounts/inventoryTransferPost/InventoryTransferJournal",
            controller: "inventoryTransferJournalController"
        })
        .when("/inventory-sale-post", {
            templateUrl: "Accounts/InventorySale/InventoryReceivable",
            controller: "inventoryReceivableController"
        })
        .when("/inventory-shortage-payable", {
            templateUrl: "Accounts/inventoryPayable/InventoryShortagePayable",
            controller: "inventoryShortagePayableController"
        })
        .when("/inventory-reject-payable", {
            templateUrl: "Accounts/inventoryPayable/InventoryRejectPayable",
            controller: "inventoryRejectPayableController"
        })
        .when("/inventory-issue-journal", {
            templateUrl: "Accounts/inventoryPayable/InventoryIssueJournal",
            controller: "inventoryIssueJournalController"
        })
        .when("/inventory-issue-return-journal", {
            templateUrl: "Accounts/inventoryPayable/InventoryIssueReturnJournal",
            controller: "inventoryIssueReturnJournalController"
        })
        .when("/daily-transaction-report", {
            templateUrl: "Accounts/Voucher/DailyTransactionReportPage",
            controller: "dailyTransactionReportController"
        })
        //.when("/employee-advance-requisition", {
        //    templateUrl: "Accounts/Views/EmployeeAdvanceRequisition",
        //    controller: "employeeAdvanceRequisitionController"
        //})
        .when("/hr-employee-advance-requisition", {
            templateUrl: "Accounts/Advance/HREmployeeAdvanceRequisition/",
            controller: "employeeAdvanceRequisitionHRController"
        })
        .when("/standard-actual-budget", {
            templateUrl: "Accounts/BudgetMaster/FiscalYearBudget",
            controller: "fiscalYearBudgetController"
        })
        .when("/vendor-invoice", {
            templateUrl: "Accounts/Invoice/VendorInvoice",
            controller: "vendorInvoiceController"
        })
        .when("/invoice-overhead", {
            templateUrl: "Accounts/Invoice/InvoiceOverhead",
            controller: "invoiceOverheadController"
        })

        .when("/invoice-overheadpost", {
            templateUrl: "Accounts/Invoice/InvoiceOverheadPost",
            controller: "invoiceOverheadPostController"
        })

        .when("/discounttype-gl", {
            templateUrl: "Accounts/FinancingType/DiscountTypeGL",
            controller: "discountTypeGLController"
        })

        .when("/employee-salary-payable", {
            templateUrl: "Accounts/EmployeePayable/EmployeeSalaryPayable",
            controller: "employeeSalaryPayableController"
        })
        .when("/nonFinancial-material-OB", {
            templateUrl: "Accounts/OpeningBalance/NonFinancialMaterialOBPost",
            controller: "nonFinancialMaterialOpeningBalancePostController"
        })
        .when("/delete-acccutoff-backdata", {
            templateUrl: "Accounts/OpeningBalance/DeleteAccCutOffDateBackData",
            controller: "deleteAccCutOffDateBackDataController"
        })
        .when("/loan-register", {
            templateUrl: "Accounts/Loan/LoanLedgerReport",
            controller: "loanLedgerReportController"
        })


        .when("/material-master-opening-balance-report", {
            templateUrl: "Accounts/OpeningBalance/MaterialMasterOpeningBalanceReport",
            controller: "openingBalanceReportController"
        })
        .when("/salary-payable", {
            templateUrl: "Accounts/SalaryDisbursement/salaryPayable",
            controller: "salaryPayableController"
        })
        .when("/salary-disbursement", {
            templateUrl: "Accounts/SalaryDisbursement/Aplos",
            controller: "salaryDisbursementController"
        })
        .when("/salary-payable-disbursement", {
            templateUrl: "Accounts/SalaryDisbursement/salaryPayableDisbursement",
            controller: "salaryPayableDisbursementController"
        })
        .when("/finalSettlement-posting", {
            templateUrl: "Accounts/SalaryDisbursement/FinalSettlementPost",
            controller: "finalSettlementPostController"
        })
        .when("/bonus-disbursement", {
            templateUrl: "Accounts/SalaryDisbursement/BonusDisbursement",
            controller: "bonusDisbursementController"
        })
        .when("/service-payable", {
            templateUrl: "Accounts/inventoryPayable/ServicePayable",
            controller: "servicePayableController"
        })
        .when("/general-account-determinate", {
            templateUrl: "Accounts/GeneralAccountDeterminate/Aplos",
            controller: "GeneralAccountDeterminateController"
        })

        .when("/salary-journal", {
            templateUrl: "Accounts/voucher/SalaryJournal",
            controller: "salaryJournalController"
        })



        .when('/opening-balance-register', {
            templateUrl: 'Accounts/OpeningBalance/OpeningBalanceRegister',
            controller: 'openingBalanceReportController'
        })
        .when('/employee-salary-advance-ledger', {
            templateUrl: 'Accounts/Advance/EmployeeSalaryAdvanceLedger',
            controller: 'employeeSalaryAdvanceLedgerController'
        })

        .when("/vendor-charge-writeoff", {
            templateUrl: "Accounts/Advance/VendorChargeWriteOff",
            controller: "vendorChargeWriteOffController"
        })

        .when("/suspense-payable", {
            templateUrl: "Accounts/Invoice/SuspensePayable",
            controller: "suspensePayableController"
        })

        .when("/balancesheet-report-groupwise", {
            templateUrl: "Accounts/voucher/BalanceSheetReportGroupWise",
            controller: "balanceSheetReportGroupWiseController"
        })


        .when("/trial-balance-report-groupwise", {
            templateUrl: "Accounts/voucher/TrialBalanceReportGroupWise",
            controller: "trialBalanceReportGroupWiseController"
        })
        .when("/party-payment-status", {
            templateUrl: "Accounts/AccountStatusDashboard/PartyPaymentStatus",
            controller: "partyPaymentStatusController"
        })

        .when("/party-payment-status-detail/:id", {
            templateUrl: "Accounts/voucher/PartyPaymentStatusDetail",
            controller: "partyPaymentStatusDetailController"
        })

        .when("/rcm-tax-payable", {
            templateUrl: "Accounts/TaxReport/RCMTaxPayable",
            controller: "RCMTaxPayableReportController"
        })
        .when("/rcm-payable-sales", {
            templateUrl: "Accounts/TaxReport/RCMTaxPayableSales",
            controller: "RCMTaxPayableSalesReportController"
        })
    


        .when("/rcm-tax-receivable", {
            templateUrl: "Accounts/TaxReport/RCMTaxReceivable",
            controller: "RCMTaxReceivableReportController"
        })
        .when("/rcm-receivable-sales", {
            templateUrl: "Accounts/TaxReport/RCMTaxReceivableSales",
            controller: "RCMTaxReceivableSalesReportController"
        })


        .when("/tds-tax-deductionReport", {
            templateUrl: "Accounts/TaxReport/TDSDeductionReport",
            controller: "TDSDeductionReportController"
        })

        .when("/gst-receivable", {
            templateUrl: "Accounts/TaxReport/GSTReceivableReport",
            controller: "GSTReceivableReportController"
        })

        .when("/debitNote-creditNote-status", {
            templateUrl: "Accounts/TaxReport/DebitNoteCreditNoteTaxReport",
            controller: "debitNoteCreditNoteTaxReportController"
        })
        .when("/payment-pending-setOff", {
            templateUrl: "Accounts/TaxReport/PaymentPendingforSetOffReport",
            controller: "paymentPendingforSetOffReportController"
        })
    
        .when("/gst-payable-sales", {
            templateUrl: "Accounts/TaxReport/GSTPayableSalesReport",
            controller: "GSTPayableSalesReportController"
        })
        .when("/day-books", {
            templateUrl: "Accounts/VoucherReport/DayBookReport",
            controller: "dayBooksReportController"
        })
        .when("/parked-pendingPosting", {
            templateUrl: "Accounts/VoucherReport/ParkedReport",
            controller: "parkedReportController"
        })
        .when("/gst-r2", {
            templateUrl: "Accounts/TaxReport/GSTR2",
            controller: "gstR2ReportController"
        })
        .when("/expense-register-report", {
            templateUrl: "Accounts/VoucherReport/ExpenseRegisterReport",
            controller: "expenseRegisterReportController"
        })
        .when("/financial-status-customer-receivable-invoice-detail/:id", {
            templateUrl: "Accounts/AccountStatusDashboard/CustomerReceivableInvoiceDetail",
            controller: "FinancialStatusCustomerReceivableInvoiceDetailController"
        })
        .when("/party-reconciliation", {
            templateUrl: "Accounts/PartyReconciliation/PartyReconciliation",
            controller: "partyReconciliationController"
        })

        .when("/party-reconciliation-detail/:id", {
            templateUrl: "Accounts/PartyReconciliation/PartyReconciliationDetail",
            controller: "partyReconsilationDetailController"
        })
        .when("/os-receive-post", {
            templateUrl: "Accounts/inventoryPayable/InventoryOutSourceReceivePost",
            controller: "inventoryOutSourceReceivePostController"
        })

        .when("/voucher-park", {
            templateUrl: "Accounts/VoucherPark/Aplos",
            controller: "voucherParkController"
        })
        .when('/incentive-master', {
            templateUrl: 'Accounts/Incentive/Aplos',
            controller: 'incentiveController'
        })
        .when('/incentive-receivable', {
            templateUrl: 'Accounts/Incentive/IncentiveReceivable',
            controller: 'incentiveReceivableController'
        })
        .when("/voucher-gl-update", {
            templateUrl: "Accounts/VoucherGlUpdate/Aplos",
            controller: "VoucherGlUpdateController"
        })
        .when("/customer-confirmation", {
            templateUrl: "Accounts/VoucherGlUpdate/CustomerConfirmation",
            controller: "CustomerConfirmationController"
        })
        .when("/post-invoice", {
            templateUrl: "Accounts/PostInvoice/Aplos",
            controller: "PostInvoiceController"
        })

        .when("/asset-wip-status", {
            templateUrl: "Accounts/VoucherReport/AssetWIPStatusReport",
            controller: "AssetWIPStatusController"
        })

        .when("/inventory-sales-returnpost", {
            templateUrl: "Accounts/InventorySale/InventorySalesReturnPost",
            controller: "inventorySalesReturnPost"
        })

        .when("/balance-sheet-scheduling", {
            templateUrl: "Accounts/BalanceSheetScheduling/Aplos",
            controller: "balanceSheetSchedulingController"
        })

        .when("/chart-account-setup", {
            templateUrl: "Accounts/ChartAccountSetup/Aplos",
            controller: "ChartAccountSetupController"
        })

        .when("/mnc-account-setup", {
            templateUrl: "Accounts/ManagementChartAccountSetup/Aplos",
            controller: "ManagementChartAccountSetupController"
        })
        .when("/gl-control", {
            templateUrl: "Accounts/GeneralAccountDeterminate/GlControl",
            controller: "GLControlController"
        })

        .when("/payment-advise", {
            templateUrl: "Accounts/SalaryDisbursement/PaymentAdviseReport",
            controller: "PaymentAdviseReportController"
        })
        .when("/expense-distribution-report", {
            templateUrl: "Accounts/VoucherReport/EDReport",
            controller: "ExpenseDistributionReportController"
        })
        .when("/subsequent-investment", {
            templateUrl: "Accounts/Investment/SubsequentInvestment",
            controller: "SubsequentInvestmentController"
        })
        .when("/bonus-entry", {
            templateUrl: "Accounts/Bonus/Aplos",
            controller: "BonusController"
        })
        .when("/payment-advice", {
            templateUrl: "Accounts/Invoice/paymentadvice",
            controller: "PaymentAdviceController"
        })
        .when("/petty-cash", {
            templateUrl: "Accounts/PettyCashMaster/Aplos",
            controller: "PettyCashMasterController"
        })
        .when("/bss-report", {
            templateUrl: "Accounts/BalanceSheetScheduling/Report",
            controller: "BalanceSheetSchedulingReportController"
        })
        .when("/roundoff-journal", {
            templateUrl: "Accounts/Voucher/RoundOffJournal",
            controller: "roundOffJournalController"
        })
        .when("/gl-management", {
            templateUrl: "Accounts/GLManagement/GLManagement",
            controller: "GLManagementController"
        })
        ;
} 