
"use strict";
var upanelApp = angular
    .module("upanelApp", ["ngRoute", "ngCookies", "angularUtils.directives.dirPagination", "toaster", "angucomplete-alt", "angularjs-dropdown-multiselect", "ejangular"]);
upanelApp.controller("ProductLibraryController", ProductLibraryController);
upanelApp.controller("ManualOTUploadController", ManualOTUploadController);
upanelApp.controller("OTManualController", OTManualController);
upanelApp.controller("PackingConfirmationController", PackingConfirmationController);
upanelApp.controller("PackingContentController", PackingContentController);
upanelApp.controller("MachineMapController", MachineMapController);
upanelApp.controller("SkillMapController", SkillMapController);
upanelApp.controller("specialUnlockController", specialUnlockController);
upanelApp.controller("SalaryIntegrationWithThirdPartyController", SalaryIntegrationWithThirdPartyController);
upanelApp.controller("accessControllerEmployeeTagController", accessControllerEmployeeTagController);
upanelApp.controller("AdminAttendanceControlController", AdminAttendanceControlController);
upanelApp.controller("accountDashboardController", accountDashboardController);
upanelApp.controller("advanceJournalController", advanceJournalController);
upanelApp.controller("pfesiDisbursementController", pfesiDisbursementController);
upanelApp.controller("NewAttendanceProcessController", NewAttendanceProcessController);
upanelApp.controller("NewAttendanceProcessAuditReportController", NewAttendanceProcessAuditReportController);
upanelApp.controller("NewAttendanceProcessPlantWiseController", NewAttendanceProcessPlantWiseController);
upanelApp.controller("advanceJournalOpeningBalanceController", advanceJournalOpeningBalanceController);
upanelApp.controller("allowanceDailyController", allowanceDailyController);
upanelApp.controller("annualBudgetController", annualBudgetController);
upanelApp.controller("approvalConfigurationController", approvalConfigurationController);
upanelApp.controller("assetInventoryIssueController", assetInventoryIssueController);
upanelApp.controller("assetItemArticleController", assetItemArticleController);
upanelApp.controller("assetItemController", AssetItemController);
upanelApp.controller("CurrencyExchangeController", CurrencyExchangeController);
upanelApp.controller("BOMReportsController", BOMReportsController);
upanelApp.controller("BalanceOTReportController", BalanceOTReportController);
upanelApp.controller("attendanceManagementController", attendanceManagementController);
upanelApp.controller("attendanceProcessDataController", attendanceProcessDataController);
upanelApp.controller("attendanceProcessDataNewController", attendanceProcessDataNewController);
upanelApp.controller("attendanceProcessDataEntityWiseController", attendanceProcessDataEntityWiseController);
upanelApp.controller("attendanceProcessDataEntityWiseNewController", attendanceProcessDataEntityWiseNewController);
upanelApp.controller("attendanceReportController", attendanceReportController);
upanelApp.controller("attendanceSlipController", attendanceSlipController);
upanelApp.controller("balanceSheetDetailsReportController", BalanceSheetDetailsReportController);
upanelApp.controller("balanceSheetOpeningBalanceReportController", BalanceSheetOpeningBalanceReportController);
upanelApp.controller("balanceSheetReportController", balanceSheetReportController);
upanelApp.controller("balanceSheetReportTreeViewController", balanceSheetReportTreeViewController);
upanelApp.controller("bankBaseController", bankBaseController);
upanelApp.controller("bankBookReportController", bankBookReportController);
upanelApp.controller("bankJournalController", bankJournalController);
upanelApp.controller("bankLedgerReportController", bankLedgerReportController);
upanelApp.controller("bankOpeningBalanceController", bankOpeningBalanceController);
upanelApp.controller("bankOpeningBalanceLedgerController", bankOpeningBalanceLedgerController);
upanelApp.controller("bankReconcileReportController", bankReconcileReportController);
upanelApp.controller("bankReconciliationController", bankReconciliationController);
upanelApp.controller("bankReconciliationClosingController", bankReconciliationClosingController);
upanelApp.controller("bankReconciliationDataUploadController", bankReconciliationDataUploadController);
upanelApp.controller("bankReconciliationDataUploadReconciledController", bankReconciliationDataUploadReconciledController);
upanelApp.controller("bankSettlementReconciliationController", bankSettlementReconciliationController);
upanelApp.controller("BankSettlementCustomerAdvanceController", BankSettlementCustomerAdvanceController);
upanelApp.controller("bankSettlementCustomerReceiptController", bankSettlementCustomerReceiptController);
upanelApp.controller("bankSettlementJournalController", bankSettlementJournalController);
upanelApp.controller("baseAttributeAndCharacteristicsValueController", baseAttributeAndCharacteristicsValueController);
upanelApp.controller("baseInvoiceController", baseInvoiceController);
upanelApp.controller("incentiveController", incentiveController);
upanelApp.controller("incentiveReceivableController", incentiveReceivableController);
upanelApp.controller("balanceSheetSchedulingController", balanceSheetSchedulingController);
upanelApp.controller("baseInvoiceWriteOffController", baseInvoiceWriteOffController);
upanelApp.controller("baseMaterialAndArticleController", baseMaterialAndArticleController);
upanelApp.controller("baseOpeningBalanceController", baseOpeningBalanceController);
upanelApp.controller("BonusPolicyMonthlyRetainEligibleEmployeeController", BonusPolicyMonthlyRetainEligibleEmployeeController);
upanelApp.controller("budgetCodeChangeController", budgetCodeChangeController);
upanelApp.controller("budgetMasterController", budgetMasterController);
upanelApp.controller("budgetMasterFARegisterController", budgetMasterFARegisterController);
upanelApp.controller("bulletinController", BulletinController);
upanelApp.controller("bulletinTemplateController", bulletinTemplateController);
upanelApp.controller("candidateAdministrationController", candidateAdministrationController);
upanelApp.controller("candidatedocumentAddRemoveController", candidatedocumentAddRemoveController);
upanelApp.controller("candidateDocumentAssignmentController", candidateDocumentAssignmentController);
upanelApp.controller("capitalizedFixedAssetRegisterController", capitalizedFixedAssetRegisterController);
upanelApp.controller("fixedAssetDepreciationProcessController", fixedAssetDepreciationProcessController);
upanelApp.controller('fixedAssetDepreciationPostController', fixedAssetDepreciationPostController);
upanelApp.controller("cashBaseController", cashBaseController);
upanelApp.controller("cashBookReportController", cashBookReportController);
upanelApp.controller("cashJournalController", cashJournalController);
upanelApp.controller("cashLedgerReportController", cashLedgerReportController);
upanelApp.controller("cashOpeningBalanceController", cashOpeningBalanceController);
upanelApp.controller("cashOpeningBalanceLedgerController", cashOpeningBalanceLedgerController);
upanelApp.controller("characteristicsValueController", CharacteristicsValueController);
upanelApp.controller("characteristicsWisePropertiesController", CharacteristicsWisePropertiesController);
upanelApp.controller("checkLotController", checkLotController);
upanelApp.controller("commitmentController", commitmentController);
upanelApp.controller("companyDepartmentController", CompanyDepartmentController);
upanelApp.controller("companyDesignationController", CompanyDesignationController);
upanelApp.controller("companyDivisionController", CompanyDivisionController);
upanelApp.controller("companyLineController", CompanyLineController);
upanelApp.controller("companySectionController", CompanySectionController);
upanelApp.controller("companySubDivisionController", CompanySubDivisionController);
upanelApp.controller("companySubSectionController", CompanySubSectionController);
upanelApp.controller("companyTaxContributionController", companyTaxContributionController);
upanelApp.controller("compliancejobCardReportController", compliancejobCardReportController);
upanelApp.controller("complianceShiftRotationController", complianceShiftRotationController);
upanelApp.controller("compliedShiftAssignmentController", compliedShiftAssignmentController);
upanelApp.controller("compliedshiftController", CompliedShiftController);
upanelApp.controller("compliedShiftGroupingController", CompliedShiftGroupingController);
upanelApp.controller("creditNoteController", creditNoteController);
upanelApp.controller("creditNoteSetOffController", creditNoteSetOffController);
upanelApp.controller("currencyBaseController", currencyBaseController);
upanelApp.controller("customerAdvanceController", customerAdvanceController);
upanelApp.controller("customerAdvanceOpeningBalanceController", customerAdvanceOpeningBalanceController);
upanelApp.controller("customerAdvanceWriteOffController", customerAdvanceWriteOffController);
upanelApp.controller("customerInterPlantCompanyReceiptController", CustomerInterPlantCompanyReceiptController);
upanelApp.controller("customerInterTransactionPendingController", customerInterTransactionPendingController);
upanelApp.controller("customerInvoiceController", customerInvoiceController);
upanelApp.controller("customerInvoiceOpeningBalanceController", customerInvoiceOpeningBalanceController);
upanelApp.controller("customerInvoiceReceiptController", customerInvoiceReceiptController);
upanelApp.controller("customerInvoiceSettlementController", customerInvoiceSettlementController);
upanelApp.controller("customerInvoiceWriteOffController", customerInvoiceWriteOffController);
upanelApp.controller("customerPaymentController", customerPaymentController);
upanelApp.controller("customerSuspenseController", customerSuspenseController);
upanelApp.controller("customerSuspenseWriteOffController", customerSuspenseWriteOffController);
upanelApp.controller("dailyAttendanceStatusReportController", dailyAttendanceStatusReportController);
upanelApp.controller("dailyComplianceReportController", dailyComplianceReportController);
upanelApp.controller("dashBoardController", dashBoardController);
upanelApp.controller("debitNoteController", debitNoteController);
upanelApp.controller("debitNoteSetOffController", debitNoteSetOffController);
upanelApp.controller("departmentController", DepartmentController);
upanelApp.controller("designationMasterController", DesignationMasterController);
upanelApp.controller("destinationController", DestinationController);
upanelApp.controller("divisionController", DivisionController);
upanelApp.controller("documentDashboardController", documentDashboardController);
upanelApp.controller("documentExcelReportController", documentExcelReportController);
upanelApp.controller("dynamicSalaryTopSheetController", dynamicSalaryTopSheetController);
upanelApp.controller("employeeAdvanceController", employeeAdvanceController);
upanelApp.controller("employeeAdvanceOpeningBalanceController", employeeAdvanceOpeningBalanceController);
//upanelApp.controller("employeeAdvanceRequisitionController", employeeAdvanceRequisitionController);
upanelApp.controller("employeeAdvanceRequisitionHRController", employeeAdvanceRequisitionHRController);
upanelApp.controller("employeeAdvanceRequisitionPostController", employeeAdvanceRequisitionPostController);
upanelApp.controller("employeeAdvanceWriteOffController", employeeAdvanceWriteOffController);
upanelApp.controller("employeeTotalAdvanceWriteOffController", employeeTotalAdvanceWriteOffController);
upanelApp.controller("EmployeeAndPlantWiseAttendanceUnLockController", EmployeeAndPlantWiseAttendanceUnLockController);
upanelApp.controller("employeeAttendanceGroupController", employeeAttendanceGroupController);
upanelApp.controller("employeeBankInformationController", employeeBankInformationController);
upanelApp.controller("employeeBaseController", employeeBaseController);
upanelApp.controller("employeeBaseMultipleController", employeeBaseMultipleController);
upanelApp.controller("employeeDeviceController", employeeDeviceController);
upanelApp.controller("employeedocumentAddRemoveController", employeedocumentAddRemoveController);
upanelApp.controller("employeeDocumentAssignmentController", employeeDocumentAssignmentController);
upanelApp.controller("employeeExpenseBookingReportController", employeeExpenseBookingReportController);
upanelApp.controller("employeeIdCardController", employeeIdCardController);
upanelApp.controller("employeeInformationController", employeeInformationController);
upanelApp.controller("SectionemployeeLeaveApplicationController", SectionemployeeLeaveApplicationController);
upanelApp.controller("employeeLeaveApplicationController", employeeLeaveApplicationController);
upanelApp.controller("employeeLeaveBalanceController", employeeLeaveBalanceController);
upanelApp.controller("employeeLeaveCarryForwardController", employeeLeaveCarryForwardController);
upanelApp.controller("employeeLedgerReportController", employeeLedgerReportController);
upanelApp.controller("EmployeeLockAndUnLockController", EmployeeLockAndUnLockController);
upanelApp.controller("employeePayableController", employeePayableController);
upanelApp.controller("employeePayableOpeningBalanceController", employeePayableOpeningBalanceController);
upanelApp.controller("employeePaymentController", employeePaymentController);
upanelApp.controller("multipleEmployeePaymentController", multipleEmployeePaymentController);
upanelApp.controller("employeeProbationalPeriodController", employeeProbationalPeriodController);
upanelApp.controller("EmployeeProfileApprovalController", EmployeeProfileApprovalController);
upanelApp.controller("EmployeeProfileUnApprovalController", EmployeeProfileUnApprovalController);
upanelApp.controller("employeePurchaseController", employeePurchaseController);
upanelApp.controller("employeeRegisterController", employeeRegisterController);
upanelApp.controller("employeeReportInfoController", employeeReportInfoController);
upanelApp.controller("employeeShiftAssignController", employeeShiftAssignController);
upanelApp.controller("entityExpenseBookingApprovalController", entityExpenseBookingApprovalController);
upanelApp.controller("entityExpenseBookingController", entityExpenseBookingController);
upanelApp.controller("entityOperationSettingsController", entityOperationSettingsController);
upanelApp.controller("equityController", equityController);
upanelApp.controller("ExceptionForHolidayController", ExceptionForHolidayController);
upanelApp.controller("exchangeVoucherController", exchangeVoucherController);
upanelApp.controller("expenseBookingApprovalController", expenseBookingApprovalController);
upanelApp.controller("expenseBookingApprovedController", expenseBookingApprovedController);
upanelApp.controller("expenseBookingApprovedListController", expenseBookingApprovedListController);
upanelApp.controller("expenseBookingController", expenseBookingController);
upanelApp.controller("expenseDashboardController", expenseDashboardController);
upanelApp.controller("ExtraOTController", ExtraOTController);
upanelApp.controller("ExtraOTDeleteController", ExtraOTDeleteController);
upanelApp.controller("fabricRollManagementSettingsController", fabricRollManagementSettingsController);
upanelApp.controller("fabricRollMasterController", fabricRollMasterController);
upanelApp.controller("fgcomponentController", FGComponentController);
upanelApp.controller("fgzoneController", FGZoneController);
upanelApp.controller("fiscalYearBaseController", fiscalYearBaseController);
upanelApp.controller("fiscalYearClosePostController", fiscalYearClosePostController);
upanelApp.controller("fixedAssetExpenseReportController", fixedAssetExpenseReportController);
upanelApp.controller("fixedAssetMasterOpeningBalanceController", fixedAssetMasterOpeningBalanceController);
upanelApp.controller("fixedAssetObReportController", FixedAssetObReportController);
upanelApp.controller("fixedAssetRegisterAUCJVController", fixedAssetRegisterAUCJVController);
upanelApp.controller("fixedAssetRegisterController", fixedAssetRegisterController);
upanelApp.controller("fixedAssetRegisterJVController", fixedAssetRegisterJVController);
upanelApp.controller("fixedAssetRegisterJVOBController", fixedAssetRegisterJVOBController);
upanelApp.controller("GateentryTokenController", GateentryTokenController);
upanelApp.controller("GatePassController", GatePassController);
upanelApp.controller("InOutGatePassController", InOutGatePassController);
upanelApp.controller("GatePassEmployeeController", GatePassEmployeeController);
upanelApp.controller("generalLedgerOpeningBalanceReportController", generalLedgerOpeningBalanceReportController);
upanelApp.controller("generalLedgerReportController", generalLedgerReportController);
upanelApp.controller("lcLedgerReportController", lcLedgerReportController);
upanelApp.controller("paymentPendingforSetOffReportController", paymentPendingforSetOffReportController);
upanelApp.controller("generalLedgerGSTReportController", generalLedgerGSTReportController);
upanelApp.controller("glMappingController", glMappingController);
upanelApp.controller("grnApprovalController", grnApprovalController);
upanelApp.controller("grnApprovedController", grnApprovedController);
upanelApp.controller("grnPaymentHoldController", grnPaymentHoldController);
upanelApp.controller("hrDashboardController", hrDashboardController);
upanelApp.controller("incomeStatementReportController", IncomeStatementReportController);
upanelApp.controller("individualComplianceReportController", individualComplianceReportController);
upanelApp.controller("inquiryController", inquiryController);
upanelApp.controller("inquiryMasterController", inquiryMasterController);
upanelApp.controller("interCompanyInvestmentTakenOpeningBalanceController", interCompanyInvestmentTakenOpeningBalanceController);
upanelApp.controller("interCompanyLoanTakenOpeningBalanceController", interCompanyLoanTakenOpeningBalanceController);
upanelApp.controller("interCompanyPartyController", InterCompanyPartyController);
upanelApp.controller("interCompanyTransactionTakenOpeningBalanceController", interCompanyTransactionTakenOpeningBalanceController);
upanelApp.controller("interInvestmentGivenOpeningBalanceController", interInvestmentGivenOpeningBalanceController);
upanelApp.controller("interLoanGivenOpeningBalanceController", interLoanGivenOpeningBalanceController);
upanelApp.controller("interLoanPendingController", interLoanPendingController);
upanelApp.controller("interPlantInvestmentTakenOpeningBalanceController", interPlantInvestmentTakenOpeningBalanceController);
upanelApp.controller("interPlantLoanTakenOpeningBalanceController", interPlantLoanTakenOpeningBalanceController);
upanelApp.controller("interPlantTransactionTakenOpeningBalanceController", interPlantTransactionTakenOpeningBalanceController);
upanelApp.controller("interTransactionController", interTransactionController);
upanelApp.controller("interTransactionGivenOpeningBalanceController", interTransactionGivenOpeningBalanceController);
upanelApp.controller("intSalesOrderInvoiceController", intSalesOrderInvoiceController);
upanelApp.controller("intSalesOrderInvoiceEditController", intSalesOrderInvoiceEditController);
upanelApp.controller("intSalesOrderInvoicePostController", intSalesOrderInvoicePostController);
upanelApp.controller("AssetIssueSlipBaseController", AssetIssueSlipBaseController);
upanelApp.controller("inventoryIssueController", inventoryIssueController);
upanelApp.controller("inventoryIssueSlipBaseController", inventoryIssueSlipBaseController);
upanelApp.controller("inventoryIssueJournalController", inventoryIssueJournalController);
upanelApp.controller("inventoryIssueReturnJournalController", inventoryIssueReturnJournalController);
upanelApp.controller("inventoryPayableController", inventoryPayableController);
upanelApp.controller("inventoryReceiveController", inventoryReceiveController);
upanelApp.controller("inventoryRejectPayableController", inventoryRejectPayableController);
upanelApp.controller("inventoryReportController", inventoryReportController);
upanelApp.controller("inventoryShortagePayableController", inventoryShortagePayableController);
upanelApp.controller("investmentController", investmentController);
upanelApp.controller("investmentGivenOpeningBalanceController", investmentGivenOpeningBalanceController);
upanelApp.controller("investmentTakenOpeningBalanceController", investmentTakenOpeningBalanceController);
upanelApp.controller("invoiceChargeWriteOffController", invoiceChargeWriteOffController);
upanelApp.controller("invoiceController", InvoiceController);
upanelApp.controller("issueRegisterController", issueRegisterController);
upanelApp.controller("IssueReturnRegisterController", IssueReturnRegisterController);
upanelApp.controller("IssueSlipController", IssueSlipController);
upanelApp.controller("jobCardReportController", jobCardReportController);
upanelApp.controller("jobCardReportNewController", jobCardReportNewController);
upanelApp.controller("journalController", journalController);
upanelApp.controller("journalOpeningBalanceController", journalOpeningBalanceController);
upanelApp.controller("LayOffController", LayOffController);
upanelApp.controller("LCReportsController", LCReportsController);
upanelApp.controller("leaveEncashmentController", leaveEncashmentController);
upanelApp.controller("LeaveEncashmentEntryController", LeaveEncashmentEntryController);
upanelApp.controller("leaveInformationController", LeaveInformationController);
upanelApp.controller("lineController", LineController);
upanelApp.controller("LineDesignerController", LineDesignerController);
upanelApp.controller("lineEmployeeAssignController", lineEmployeeAssignController);
upanelApp.controller("lineEmployeeAssignEditController", lineEmployeeAssignEditController);
upanelApp.controller("lineProductionBookingController", lineProductionBookingController);
upanelApp.controller("lineProductionExcelController", lineProductionExcelController);
upanelApp.controller("loanAdvanceMasterController", loanAdvanceMasterController);
upanelApp.controller("loanController", loanController);
upanelApp.controller("loanGivenOpeningBalanceController", loanGivenOpeningBalanceController);
upanelApp.controller("loanLedgerReportController", loanLedgerReportController);
upanelApp.controller("loanPaymentController", loanPaymentController);
upanelApp.controller("loanCloseController", loanCloseController);
upanelApp.controller("loanTakenController", loanTakenController);
upanelApp.controller("loanTakenOpeningBalanceController", loanTakenOpeningBalanceController);
upanelApp.controller("lsdController", LSDController);
upanelApp.controller("machineAttributeController", machineAttributeController);
upanelApp.controller("machineController", machineController);
upanelApp.controller("machineMasterUIController", machineMasterUIController);
upanelApp.controller("MachineMasterTransactionController", MachineMasterTransactionController);
upanelApp.controller("MachineMasterTransactionReportController", MachineMasterTransactionReportController);
upanelApp.controller("mainProcessPlanningController", MainProcessPlanningController);
upanelApp.controller("manpowerAttendanceGroupSummaryController", manpowerAttendanceGroupSummaryController);
upanelApp.controller("manpowerBudgetDashboardController", manpowerBudgetDashboardController);
upanelApp.controller("manualOutTimeController", manualOutTimeController);
upanelApp.controller("masterOrderController", masterOrderController);
upanelApp.controller("masterOrderSalesController", masterOrderSalesController);
upanelApp.controller("masterOrderSalesPostController", masterOrderSalesPostController);
upanelApp.controller("salesIncentiveController", salesIncentiveController);
upanelApp.controller("materialAttributeMasterController", MaterialAttributeMasterController);
upanelApp.controller("materialAttributeValueController", MaterialAttributeValueController);
upanelApp.controller("MaterialBudgetController", MaterialBudgetController);
upanelApp.controller("materialGroupMasterController", MaterialGroupMasterController);
upanelApp.controller("MaterialIssueSlipController", MaterialIssueSlipController);
upanelApp.controller("materialledgerController", materialledgerController);
upanelApp.controller("purchaseorderRegisterController", purchaseorderRegisterController);
upanelApp.controller("materialMasterArticleController", materialMasterArticleController);
upanelApp.controller("materialMasterController", MaterialMasterController);
upanelApp.controller("materialMasterOpeningBalanceController", materialMasterOpeningBalanceController);
upanelApp.controller("materialMasterReportController", MaterialMasterReportController);
upanelApp.controller("materialStockController", materialStockController);
upanelApp.controller("MaternityLeaveTransactionController", MaternityLeaveTransactionController);
upanelApp.controller("misAccountDashboardController", misAccountDashboardController);
upanelApp.controller("LeaveYearEndProcessEncashmentApprovalController", LeaveYearEndProcessEncashmentApprovalController);

//.controller("mpanelDashboardController", mpanelDashboardController)                                                              ;
//.controller("mpanelLoginController", mpanelLoginController)                                                                      ;
//.controller("mpanelLogoutController", mpanelLogoutController)                                                                    ;
upanelApp.controller("multipleResignationApprovalController", multipleResignationApprovalController);
upanelApp.controller("multipleVendorPaymentApprovedController", multipleVendorPaymentApprovedController);
upanelApp.controller("multipleVendorPaymentController", multipleVendorPaymentController);
upanelApp.controller("nonAssetRegisterController", nonAssetRegisterController);
upanelApp.controller("nonFinancialMaterialOpeningBalancePostController", nonFinancialMaterialOpeningBalancePostController);
upanelApp.controller("normalJournalController", normalJournalController);
upanelApp.controller("oDDeleteController", oDDeleteController);
upanelApp.controller("oDDeleteNewController", oDDeleteNewController);
upanelApp.controller("onDutyApprovalController", onDutyApprovalController);
upanelApp.controller("onDutyApprovalNewController", onDutyApprovalNewController);
upanelApp.controller("onDutyTransactionController", onDutyTransactionController);
upanelApp.controller("openingBalanceReportController", openingBalanceReportController);
upanelApp.controller("operationController", OperationController);
upanelApp.controller("OperationMasterController", OperationMasterController);
upanelApp.controller("operationMotionController", operationMotionController);
upanelApp.controller("operationVariationController", operationVariationController);
upanelApp.controller("operationVideoUploadController", OperationVideoUploadController);
upanelApp.controller("OrderCostingApprovalController", OrderCostingApprovalController);
upanelApp.controller("OrderCostingUnApprovalController", OrderCostingUnApprovalController);
upanelApp.controller('costingCategoryController', costingCategoryController);
upanelApp.controller('costingSubCategoryController', costingSubCategoryController);
upanelApp.controller("OTAdjustmentController", OTAdjustmentController);
upanelApp.controller("otFinalController", otFinalController);
upanelApp.controller("otFinalInformationController", otFinalInformationController);
upanelApp.controller("otFinalInformationNewController", otFinalInformationNewController);
upanelApp.controller("OTManagementController", OTManagementController);
upanelApp.controller("ourStyleController", OurStyleController);
upanelApp.controller("packingListMasterController", packingListMasterController);
upanelApp.controller("paidHoursEmployeeAssignController", paidHoursEmployeeAssignController);
upanelApp.controller("partyBaseController", partyBaseController);
upanelApp.controller("partyController", PartyController);
upanelApp.controller("buyerController", BuyerController);
upanelApp.controller("buyerDivisionController", BuyerDivisionController);
upanelApp.controller("buyerDepartmentController", BuyerDepartmentController);
upanelApp.controller("buyerBrandController", buyerBrandController);
upanelApp.controller("buyerProgramController", buyerProgramController);
upanelApp.controller("partyLedgerOutstandingReportController", partyLedgerOutstandingReportController);
upanelApp.controller("partyLedgerReportController", partyLedgerReportController);
upanelApp.controller("partyOpeningBalanceLedgerController", partyOpeningBalanceLedgerController);
upanelApp.controller("partyOutstandingReportController", partyOutstandingReportController);
upanelApp.controller("partyReconciliationController", partyReconciliationController);
upanelApp.controller("partyReportController", partyReportController);
upanelApp.controller("paymentByBankController", paymentByBankController);
upanelApp.controller("paymentByCashController", paymentByCashController);
upanelApp.controller("paymentTermController", PaymentTermController);
upanelApp.controller("payRegisterBDReportWithStructureController", payRegisterBDReportWithStructureController);
upanelApp.controller("PhysicalStockAdjustmentMasterController", PhysicalStockAdjustmentMasterController);
upanelApp.controller("plantCalendarController", PlantCalendarController);
upanelApp.controller("plantSelectionController", plantSelectionController);
upanelApp.controller("PlantWiseAttendanceLockController", PlantWiseAttendanceLockController);
upanelApp.controller("PlantWiseAttendanceUnLockController", PlantWiseAttendanceUnLockController);
upanelApp.controller("plantWiseLetterTemplateController", plantWiseLetterTemplateController);
upanelApp.controller("plantWiseTermsAndConditionsController", plantWiseTermsAndConditionsController);
upanelApp.controller("portController", PortController);
upanelApp.controller("postRecruitmentDocumentByDepartmentController", postRecruitmentDocumentByDepartmentController);
upanelApp.controller("preCostingController", PreCostingController);
upanelApp.controller("preRecruitmentDocumentApprovalController", preRecruitmentDocumentApprovalController);
upanelApp.controller("preRecruitmentDocumentByDepartmentController", preRecruitmentDocumentByDepartmentController);
upanelApp.controller("printCashCheckController", printCashCheckController);
upanelApp.controller("printNonCashCheckController", printNonCashCheckController);
upanelApp.controller("processSetReportController", ProcessSetReportController);
upanelApp.controller("ProcurementController", ProcurementController);
upanelApp.controller("productCategoryController", ProductCategoryController);
upanelApp.controller("productController", ProductController);
upanelApp.controller("productDefinitionController", productDefinitionController);
upanelApp.controller("productionCalendarController", ProductionCalendarController);
upanelApp.controller("productionOrderController", ProductionOrderController);
upanelApp.controller("productionOrderReportsController", productionOrderReportsController);
upanelApp.controller("productionOrderSchedulingParametersType1Controller", ProductionOrderSchedulingParametersType1Controller);
upanelApp.controller("productionOrderSubprocessController", ProductionOrderSubprocessController);
upanelApp.controller("productionResourcesController", productionResourcesController);
upanelApp.controller("productionStatusController", ProductionStatusController);
upanelApp.controller("ProductionSummaryController", ProductionSummaryController);
upanelApp.controller("ProductionSummaryInOutController", ProductionSummaryInOutController);
upanelApp.controller("productionSystemController", productionSystemController);
upanelApp.controller("productMasterController", ProductMasterController);
upanelApp.controller("productSubCategoryAttributeController", ProductSubCategoryAttributeController);
upanelApp.controller("productSubCategoryController", ProductSubCategoryController);
upanelApp.controller("ProfileFromExcelController", ProfileFromExcelController);
upanelApp.controller("projectPlanningController", ProjectPlanningController);
upanelApp.controller("projectPlanningPurchaseOrderController", ProjectPlanningPurchaseOrderController);
upanelApp.controller("projectPlanningRequisitionController", ProjectPlanningRequisitionController);
upanelApp.controller("PurchaseOrderByRequisitionController", PurchaseOrderByRequisitionController);
upanelApp.controller("purchaseOrderBOQController", purchaseOrderBOQController);
upanelApp.controller("purchaseOrderGroupController", purchaseOrderGroupController);
upanelApp.controller("PurchaseReturnController", PurchaseReturnController);
upanelApp.controller("PurchaseReturnRegisterController", PurchaseReturnRegisterController);
upanelApp.controller("QRCodeGenerationEmployeeController", QRCodeGenerationController);
upanelApp.controller("QRCodeGenerationOperationController", QRCodeGenerationController);
upanelApp.controller("rawDataSetInOutController", rawDataSetInOutController);
upanelApp.controller("receiptByBankController", receiptByBankController);
upanelApp.controller("receiptByCashController", receiptByCashController);
upanelApp.controller("recipeGlobalMasterController", recipeGlobalMasterController);
upanelApp.controller("recipeMaterialController", recipeMaterialController);
upanelApp.controller("recipeMaterialGroupingMasterController", recipeMaterialGroupingMasterController);
upanelApp.controller("recruitmentAppDataEditController", recruitmentAppDataEditController);
upanelApp.controller("recruitmentApprovalController", recruitmentApprovalController);
upanelApp.controller("recruitmentController", recruitmentController);
upanelApp.controller("recruitmentPlanningController", recruitmentPlanningController);
upanelApp.controller("recruitmentSelectionController", recruitmentSelectionController);
upanelApp.controller("RequisitionController", RequisitionController);
upanelApp.controller("resignationApprovalController", resignationApprovalController);
upanelApp.controller("resignationController", resignationController);
upanelApp.controller("resignationRecruitmentPlanningController", resignationRecruitmentPlanningController);
upanelApp.controller("restController", restController);
upanelApp.controller("routeController", routeController);
upanelApp.controller("routeEmployeeController", routeEmployeeController);
upanelApp.controller("runningOrderParametersController", runningOrderParametersController);
upanelApp.controller("salaryAdvanceApprovalController", salaryAdvanceApprovalController);
upanelApp.controller("salaryAdvanceOpeningBalanceController", salaryAdvanceOpeningBalanceController);
upanelApp.controller("salaryFixationController", salaryFixationController);
upanelApp.controller("salaryLockController", salaryLockController);
upanelApp.controller("salaryPaymentStatementsBankCSVController", salaryPaymentStatementsBankCSVController);
upanelApp.controller("SalaryProcessController", SalaryProcessController);
upanelApp.controller("SalaryProcessNewController", SalaryProcessNewController);
upanelApp.controller("salaryProcessDeleteController", salaryProcessDeleteController);
upanelApp.controller("salaryReportController", salaryReportController);
upanelApp.controller("SalaryStructureApprovalController", SalaryStructureApprovalController);
upanelApp.controller("SalaryStructureUnApprovalController", SalaryStructureUnApprovalController);
upanelApp.controller("salaryTopSheetController", salaryTopSheetController);
upanelApp.controller("salesController", salesController);
upanelApp.controller("salesInvoiceController", salesInvoiceController);
upanelApp.controller("salesInvoicePendingController", salesInvoicePendingController);
upanelApp.controller("salesOrderInvoiceController", SalesOrderInvoiceController);
upanelApp.controller("salesOrderPackingListController", SalesOrderPackingListController);
upanelApp.controller("salesOrderPendingController", SalesOrderPendingController);
upanelApp.controller("sampleOrderController", SampleOrderController);
upanelApp.controller("sampleOrderPendingController", SampleOrderPendingController);
upanelApp.controller("samplePackingListController", SamplePackingListController);
upanelApp.controller("sampleRequisitionController", SampleRequisitionController);
upanelApp.controller("sectionController", SectionController);
upanelApp.controller("securityDepositController", securityDepositController);
upanelApp.controller("securityDepositGivenOpeningBalanceController", securityDepositGivenOpeningBalanceController);
upanelApp.controller("securityDepositTakenOpeningBalanceController", securityDepositTakenOpeningBalanceController);
upanelApp.controller("securityDepositWriteOffController", securityDepositWriteOffController);
upanelApp.controller("separationtypeController", separationtypeController);
upanelApp.controller("ServicePoAcknowledgementController", ServicePoAcknowledgementController);
upanelApp.controller("ServicePOByRequisitionController", ServicePOByRequisitionController);
upanelApp.controller("ServiceRequisitionCheckApprovedController", ServiceRequisitionCheckApprovedController);
upanelApp.controller("ServiceRequisitionController", ServiceRequisitionController);
upanelApp.controller("shiftAssignmentController", shiftAssignmentController);
upanelApp.controller("shiftTimeChangeController", shiftTimeChangeController);
upanelApp.controller("shipModeController", ShipModeController);
upanelApp.controller("skillController", SkillController);
upanelApp.controller("SpecificDateLeaveEncashmentController", SpecificDateLeaveEncashmentController);
upanelApp.controller("stitchCodeController", stitchCodeController);
upanelApp.controller("stoppageController", stoppageController);
upanelApp.controller("subDivisionController", SubDivisionController);
upanelApp.controller("subSectionController", SubSectionController);
upanelApp.controller("subsectionStructureController", SubsectionStructureController);
upanelApp.controller("taskDetailController", taskDetailController);
upanelApp.controller("taxCodeController", TaxCodeController);
upanelApp.controller("taxPayableReportController", taxPayableReportController);
upanelApp.controller("taxPaymentController", taxPaymentController);
upanelApp.controller("testingController", TestingController);
upanelApp.controller("testingStandardController", TestingStandardController);
upanelApp.controller("testingStandardReportController", TestingStandardReportController);
upanelApp.controller("thirdPartyOperationController", ThirdPartyOperationController);
upanelApp.controller("timeCaptureController", TimeCaptureController);
upanelApp.controller("TNAReportsController", TNAReportsController);
upanelApp.controller("TNAStatusReportsController", TNAStatusReportsController);
upanelApp.controller("trialBalanceReportController", trialBalanceReportController);
upanelApp.controller("TrimInTimeController", TrimInTimeController);
upanelApp.controller("unitController", UnitController);
upanelApp.controller("upanelDashboardController", upanelDashboardController);
upanelApp.controller("upanelLoginController", upanelLoginController);
upanelApp.controller("upanelLogoutController", upanelLogoutController);
upanelApp.controller("userPasswordChangeController", UserPasswordChangeController);
upanelApp.controller("vendorAdvanceController", vendorAdvanceController);
upanelApp.controller("vendorAdvanceOpeningBalanceController", vendorAdvanceOpeningBalanceController);
upanelApp.controller("vendorAdvanceWriteOffController", vendorAdvanceWriteOffController);
upanelApp.controller("vendorInvoiceController", vendorInvoiceController);
upanelApp.controller("vendorInvoiceOpeningBalanceController", vendorInvoiceOpeningBalanceController);
upanelApp.controller("vendorPaymentController", vendorPaymentController);
upanelApp.controller("weeklyAbsentismAssignmentController", weeklyAbsentismAssignmentController);
upanelApp.controller("WeekOffChangeController", WeekOffChangeController);
upanelApp.controller("workCenterBuyerTagController", WorkCenterBuyerTagController);
upanelApp.controller("workCenterMasterController", WorkCenterMasterController);
upanelApp.controller("workStationDailyController", workStationDailyController);
upanelApp.controller('ActivityMasterController', ActivityMasterController);
upanelApp.controller('actualOTAndPlantController', actualOTAndPlantController);
upanelApp.controller('advanceAndTDSController', advanceAndTDSController);
upanelApp.controller('allowanceDailyController', allowanceDailyController);
upanelApp.controller('ApprovalController', ApprovalController);
upanelApp.controller('AttendanceDeviceZoneController', AttendanceDeviceZoneController);
upanelApp.controller('attendanceEntryController', attendanceEntryController);
upanelApp.controller('attendanceOnDayStatusController', attendanceOnDayStatusController);
upanelApp.controller('attendanceProcessDataManualStatusController', attendanceProcessDataManualStatusController);
upanelApp.controller('attendanceProcessDataManualStatusNewController', attendanceProcessDataManualStatusNewController);
upanelApp.controller('attendanceProcessUIController', attendanceProcessUIController);
upanelApp.controller('attendanceRawController', attendanceRawController);
upanelApp.controller('AttendanceRawDataDeleteController', AttendanceRawDataDeleteController);
upanelApp.controller('AttendanceRawDataDeleteNewController', AttendanceRawDataDeleteNewController);
upanelApp.controller('AttendanceRawDataUploadController', AttendanceRawDataUploadController);
upanelApp.controller('attendanceSummaryStatusController', attendanceSummaryStatusController);
upanelApp.controller('authorizationConfigController', authorizationConfigController);
upanelApp.controller('biometricDeviceAsAccessListController', biometricDeviceAsAccessListController);
upanelApp.controller('biometricDeviceAsShortLeaveController', biometricDeviceAsShortLeaveController);
upanelApp.controller('BOMMasterAttachmentController', BOMMasterAttachmentController);
upanelApp.controller('BOMMasterController', BOMMasterController);
upanelApp.controller('bonusRegisterController', bonusRegisterController);
upanelApp.controller('bonusRegisterReportController', bonusRegisterReportController);
upanelApp.controller('BonusRetainedDisbursementController', BonusRetainedDisbursementController);
upanelApp.controller('bonusSheetController', bonusSheetController);
upanelApp.controller('BulkIncrementController', BulkIncrementController);
upanelApp.controller('BulkLeaveEntryController', BulkLeaveEntryController);
upanelApp.controller('buyerMasterController', BuyerMasterController);
upanelApp.controller('cashReceiptPaymentReportController', cashReceiptPaymentReportController);
upanelApp.controller('CNFExpenseBockingController', CNFExpenseBockingController);
upanelApp.controller('CompensatoryOffController', CompensatoryOffController);
upanelApp.controller('CompensatoryOffNewController', CompensatoryOffNewController);
upanelApp.controller('complianceAttendanceSettingController', complianceAttendanceSettingController);
upanelApp.controller('complianceRawDataDownloadController', complianceRawDataDownloadController);
upanelApp.controller('ConfirmationController', ConfirmationController);
upanelApp.controller('contractController', contractController);
//upanelApp.controller('contractNewController', contractNewController);
upanelApp.controller('costingGroupFormulaController', costingGroupFormulaController);
upanelApp.controller('costingItemController', costingItemController);
upanelApp.controller('CropMasterController', CropMasterController);

upanelApp.controller('customerInvoiceBanksReceiptController', customerInvoiceBanksReceiptController);
upanelApp.controller('DailyAllowanceConfirmationController', DailyAllowanceConfirmationController);
upanelApp.controller('dailyAllowanceController', dailyAllowanceController);
upanelApp.controller('DailyAllowanceRateEmpWiseController', DailyAllowanceRateEmpWiseController);
upanelApp.controller('DailyAllowanceSettingController', DailyAllowanceSettingController);
upanelApp.controller('dailyAllowanceTransactionController', dailyAllowanceTransactionController);
upanelApp.controller('dailyAttendanceSummaryController', dailyAttendanceSummaryController);
upanelApp.controller('dailyAttendanceSummaryNoLineController', dailyAttendanceSummaryNoLineController);
upanelApp.controller('dailyDayStatusController', dailyDayStatusController);
upanelApp.controller('dailyTransactionReportController', dailyTransactionReportController);
upanelApp.controller('DateRangeWiseAttendanceUnLockController', DateRangeWiseAttendanceUnLockController);
upanelApp.controller('DepartmentGroupController', DepartmentGroupController);
upanelApp.controller('DeviceRawDataDownloadController', DeviceRawDataDownloadController);
upanelApp.controller('disciplinaryActionCategoryController', disciplinaryActionCategoryController);
upanelApp.controller('disciplinaryActionController', disciplinaryActionController);
upanelApp.controller('disciplinaryActionCriticalityController', disciplinaryActionCriticalityController);
upanelApp.controller('EarnLeavePaySlipController', EarnLeavePaySlipController);
upanelApp.controller('empActiveInActiveController', empActiveInActiveController);
upanelApp.controller('empActiveInActiveNewController', empActiveInActiveNewController);
upanelApp.controller('EmployeeBankInfoInformationController', EmployeeBankInfoInformationController);
upanelApp.controller('employeeBankInformationController', employeeBankInformationController);
upanelApp.controller('employeeDeleteController', employeeDeleteController);
upanelApp.controller('employeeDisciplinaryActionController', employeeDisciplinaryActionController);
upanelApp.controller('employeeDisciplinaryActionTransactionController', employeeDisciplinaryActionTransactionController);
upanelApp.controller('employeedocumentAddRemoveController', employeedocumentAddRemoveController);
upanelApp.controller('EmployeeDOJChangeController', EmployeeDOJChangeController);
upanelApp.controller('EmployeeFixedServicTransactionController', EmployeeFixedServicTransactionController);
upanelApp.controller('employeeInFoReportController', employeeInFoReportController);
upanelApp.controller('employeeInformationNewController', employeeInformationNewController);
upanelApp.controller('EmployeeLeaveApprovalController', EmployeeLeaveApprovalController);
upanelApp.controller('employeeLeaveDeleteApplicationController', employeeLeaveDeleteApplicationController);
upanelApp.controller('EmployeeProfileUploadController', EmployeeProfileUploadController);
upanelApp.controller('EmployeePromotionAndIncrementController', EmployeePromotionAndIncrementController);
upanelApp.controller('employeePromotionController', employeePromotionController);
upanelApp.controller('employeePromotionNewController', employeePromotionNewController);
upanelApp.controller('employeeSalaryAdvanceLedgerController', employeeSalaryAdvanceLedgerController);
upanelApp.controller('employeeSalaryPayableController', employeeSalaryPayableController);
upanelApp.controller('employeeSalaryRuleEditableController', EmployeeSalaryRuleEditableController);
upanelApp.controller('EmployeeServiceBookingController', EmployeeServiceBookingController);
upanelApp.controller('EmployeeServicesRateController', EmployeeServicesRateController);
upanelApp.controller('employeeWiseFixedOTSettingController', employeeWiseFixedOTSettingController);
upanelApp.controller('EncashmentController', EncashmentController);
upanelApp.controller('entityTaskController', entityTaskController);
upanelApp.controller('esicStatementsController', esicStatementsController);
upanelApp.controller('esicSummaryController', esicSummaryController);
upanelApp.controller('exceptionEmployeeController', exceptionEmployeeController);
upanelApp.controller('expensesCapitalizedController', expensesCapitalizedController);
upanelApp.controller('FarmerMasterController', FarmerMasterController);
upanelApp.controller('FgPoFormasterOrderController', FgPoFormasterOrderController);
upanelApp.controller('finalSettlementController', finalSettlementController);
upanelApp.controller('finalSettlementNewController', finalSettlementNewController);
upanelApp.controller('finalSettlementReportController', finalSettlementReportController);
upanelApp.controller('finalSettlementVoucherController', finalSettlementVoucherController);
upanelApp.controller('fiscalYearBudgetController', fiscalYearBudgetController);
upanelApp.controller('fixedAssetAUCCapitalizeGRNBassController', fixedAssetAUCCapitalizeGRNBassController);
upanelApp.controller('assetDisposeController', assetDisposeController);
upanelApp.controller('assetDisposePostController', assetDisposePostController);
upanelApp.controller('fixedAssetDisposeController', fixedAssetDisposeController);
upanelApp.controller('fixedAssetDisposePostController', fixedAssetDisposePostController);
upanelApp.controller('FixedAssetsRegisterReportController', FixedAssetsRegisterReportController);
upanelApp.controller('FixedAssetsRegisterDisposedReportController', FixedAssetsRegisterDisposedReportController);
upanelApp.controller('goodsReceiveNoteController', goodsReceiveNoteController);
upanelApp.controller('AllBinWiseGRNController', AllBinWiseGRNController);
upanelApp.controller('gratuityReportController', gratuityReportController);
upanelApp.controller('gratuityPolicyController', gratuityPolicyController);
upanelApp.controller('GRNByPOController', GRNByPOController);
upanelApp.controller('GRNBOQPOController', GRNBOQPOController);
upanelApp.controller('holidayAbsentismAssignmentController', holidayAbsentismAssignmentController);
upanelApp.controller('hourlyOffDutyTagController', hourlyOffDutyTagController);
upanelApp.controller('hourlyOTController', hourlyOTController);
upanelApp.controller('hourlyOTNewController', hourlyOTNewController);
upanelApp.controller('ICSMasterController', ICSMasterController);
upanelApp.controller('incrementGroupController', incrementGroupController);
upanelApp.controller('independentOrderController', independentOrderController);
upanelApp.controller('IndividualAttendanceLockController', IndividualAttendanceLockController);
upanelApp.controller('IndividualAttendanceUnLockController', IndividualAttendanceUnLockController);
upanelApp.controller('individualFixedOTController', individualFixedOTController);
upanelApp.controller('interpartyLedgerReportController', interpartyLedgerReportController);
upanelApp.controller('InventoryDashboardController', InventoryDashboardController);
upanelApp.controller('inventoryIssueDeleteController', inventoryIssueDeleteController);
upanelApp.controller('inventoryReceivableController', inventoryReceivableController);
upanelApp.controller('inventorySalesRegisterController', inventorySalesRegisterController);
upanelApp.controller('inventorySalesController', inventorySalesController);
upanelApp.controller('inventoryScrapController', inventoryScrapController);
upanelApp.controller('InventoryStatusDashboardController', InventoryStatusDashboardController);
upanelApp.controller('InventoryDashboardStatusController', InventoryDashboardStatusController);
upanelApp.controller('invoiceOverheadController', invoiceOverheadController);
upanelApp.controller('invoiceOverheadPostController', invoiceOverheadPostController);
upanelApp.controller('issueAUCCapitalizeController', issueAUCCapitalizeController);
upanelApp.controller('issueGroupController', issueGroupController);
upanelApp.controller('issueImportanceController', issueImportanceController);
upanelApp.controller('issueReportController', issueReportController);
upanelApp.controller('IssueReturnController', IssueReturnController);
upanelApp.controller('issueStandardController', issueStandardController);
upanelApp.controller('IssueStatusReportsController', IssueStatusReportsController);
upanelApp.controller('jobCardcomplianceReportController', jobCardcomplianceReportController);
upanelApp.controller('lateAttendancePostingController', lateAttendancePostingController);
upanelApp.controller('leaveOpeningBalanceController', LeaveOpeningBalanceController);
upanelApp.controller('leavesChecklistReportController', leavesChecklistReportController);
upanelApp.controller('leaveWithWagesRegistersController', leaveWithWagesRegistersController);
upanelApp.controller('leaveWithWagesRegistersForm18Controller', leaveWithWagesRegistersForm18Controller);
upanelApp.controller('LeaveYearEndProcessController', LeaveYearEndProcessController);
upanelApp.controller('LeaveYearEndProcessNewController', LeaveYearEndProcessNewController);
upanelApp.controller('loanInterestPayableController', loanInterestPayableController);
upanelApp.controller('loanInterestPayableReverseController', loanInterestPayableReverseController);
upanelApp.controller('longAbsenteeismAssignController', longAbsenteeismAssignController);
upanelApp.controller('manpowerAttendanceSummaryController', manpowerAttendanceSummaryController);
upanelApp.controller('manpowerAttendanceSummaryControllerNew', manpowerAttendanceSummaryControllerNew);
upanelApp.controller('manpowerBudgetController', manpowerBudgetController);
upanelApp.controller('ManualAttendanceConfirmationController', ManualAttendanceConfirmationController);
upanelApp.controller('masterLCController', masterLCController);
upanelApp.controller('materialAgeingDashboardController', materialAgeingDashboardController);
upanelApp.controller('materialMasterWithProductMasterController', materialMasterWithProductMasterController);
upanelApp.controller('MaterialTransferController', MaterialTransferController);
upanelApp.controller('maternityBenefitAfterController', maternityBenefitAfterController);
upanelApp.controller('maternityBenefitController', maternityBenefitController);
upanelApp.controller('maternityLeaveReportController', maternityLeaveReportController);
upanelApp.controller('monthlyAttendanceInformationController', monthlyAttendanceInformationController);
upanelApp.controller('MonthlyAttendanceInformationNewController', MonthlyAttendanceInformationNewController);
upanelApp.controller('multipleIdCardController', multipleIdCardController);
upanelApp.controller('MultipleLeaveEncashmentController', MultipleLeaveEncashmentController);
upanelApp.controller('nationalFestivalController', nationalFestivalController);
upanelApp.controller('offDutyApproveController', offDutyApproveController);
upanelApp.controller('offDutyHoursController', offDutyHoursController);
upanelApp.controller('openingBalanceReportController', openingBalanceReportController);
upanelApp.controller('OrderControlController', OrderControlController);
upanelApp.controller('OrderCostingController', OrderCostingController);
upanelApp.controller('OrderReportController', OrderReportController);
upanelApp.controller('OTLimitTransactionController', OTLimitTransactionController);
upanelApp.controller('OTLimitTransactionFromAppController', OTLimitTransactionFromAppController);
upanelApp.controller('otSlabController', otSlabController);
upanelApp.controller('ParollsReportController', ParollsReportController);
upanelApp.controller('partyGroupCategoryController', PartyGroupCategoryController);
upanelApp.controller('partyGroupClassController', PartyGroupClassController);
upanelApp.controller('partyGroupController', PartyGroupController);
upanelApp.controller('partyGroupSubCategoryController', PartyGroupSubCategoryController);
upanelApp.controller('partyMappingController', partyMappingController);
upanelApp.controller('PaymentController', PaymentController);
upanelApp.controller('paymentModeChangeController', paymentModeChangeController);
upanelApp.controller('payRegisterBDReportComController', payRegisterBDReportComController);
upanelApp.controller('payRegisterBDReportController', payRegisterBDReportController);
upanelApp.controller('payRegisterBDReportNewController', payRegisterBDReportNewController);
upanelApp.controller('payrollGroupMasterController', payrollGroupMasterController);
upanelApp.controller('paySlipsController', paySlipsController);
upanelApp.controller('paySlipsNewController', paySlipsNewController);
upanelApp.controller('pFEmployeeAppliedController', PFEmployeeAppliedController);
upanelApp.controller('pFEmployeeVoluntaryValueController', PFEmployeeVoluntaryValueController);
upanelApp.controller('PFPolicyController', PFPolicyController);
upanelApp.controller('PhysicalStockAdjustmentMasterController', PhysicalStockAdjustmentMasterController);
upanelApp.controller('plantWiseGateController', plantWiseGateController);
upanelApp.controller('preallocatedOTController', preallocatedOTController);
upanelApp.controller('preallocatedOTReportController', preallocatedOTReportController);
upanelApp.controller('PrePurchaseInvoiceController', PrePurchaseInvoiceController);
upanelApp.controller('ProductionReportsController', ProductionReportsController);
upanelApp.controller('ProductionSummarySFGController', ProductionSummarySFGController);
upanelApp.controller('PromotionIncrementApprovalController', PromotionIncrementApprovalController);
upanelApp.controller('providentFundStatementReportandCSVController', providentFundStatementReportandCSVController);
upanelApp.controller('PurchaseBookingSodaController', PurchaseBookingSodaController);
upanelApp.controller('PurchaseDocumentAcceptanceController', PurchaseDocumentAcceptanceController);
upanelApp.controller('PurchaseDocumentAcceptancePostController', PurchaseDocumentAcceptancePostController);
upanelApp.controller('purchaseLCAmendmentController', purchaseLCAmendmentController);
upanelApp.controller('purchaseLCChargesPostController', purchaseLCChargesPostController);
upanelApp.controller('purchaseLCController', purchaseLCController);
upanelApp.controller('PurchaseLCWithPOController', PurchaseLCWithPOController);
upanelApp.controller('PurchaseOrderController', PurchaseOrderController);
upanelApp.controller('autoLoanController', autoLoanController);
upanelApp.controller('autoLoanPostController', autoLoanPostController);
upanelApp.controller('QMSDefectMasterController', QMSDefectMasterController);
upanelApp.controller('QMSInspectionController', QMSInspectionController);
upanelApp.controller('QMSMasterController', QMSMasterController);
upanelApp.controller('QMSRejectionController', QMSRejectionController);
upanelApp.controller('qualityStdSetController', qualityStdSetController);
upanelApp.controller('quickCostingMasterController', quickCostingMasterController);
upanelApp.controller('rawDataDownloadController', rawDataDownloadController);
upanelApp.controller('RestTypeController', RestTypeController);
upanelApp.controller('salaryCertificateReportController', salaryCertificateReportController);
upanelApp.controller('salaryDisbursementController', salaryDisbursementController);
upanelApp.controller('bonusDisbursementController', bonusDisbursementController);
upanelApp.controller('salaryHeadWiseAmountSettingController', salaryHeadWiseAmountSettingController);
upanelApp.controller('SalaryHeadWiseAmountTransactionController', SalaryHeadWiseAmountTransactionController);
upanelApp.controller('salaryJournalController', salaryJournalController);
upanelApp.controller('salaryPayableController', salaryPayableController);
upanelApp.controller('salaryPayableDisbursementController', salaryPayableDisbursementController);
upanelApp.controller('finalSettlementPostController', finalSettlementPostController);
upanelApp.controller('salaryPaymentStatementsController', salaryPaymentStatementsController);
upanelApp.controller('salaryProcessedReportComplianceController', salaryProcessedReportComplianceController);
upanelApp.controller('salaryProcessedReportController', salaryProcessedReportController);
upanelApp.controller('ArrearProcessedReportController', ArrearProcessedReportController);
upanelApp.controller('ArrearProcessedTotalReportController', ArrearProcessedTotalReportController);
upanelApp.controller('salaryProcessedReportSummaryController', salaryProcessedReportSummaryController);
upanelApp.controller('SalaryProcessOtherStatusController', SalaryProcessOtherStatusController);
upanelApp.controller('SalaryProcessOtherStatusNewController', SalaryProcessOtherStatusNewController);
upanelApp.controller('salaryRuleController', salaryRuleController);
upanelApp.controller('salarySlabWiseValueController', salarySlabWiseValueController);
upanelApp.controller('salaryStructureAndProcessedReportController', salaryStructureAndProcessedReportController);
upanelApp.controller('SalaryStructureDataUploadController', SalaryStructureDataUploadController);
upanelApp.controller('salaryStructureSheetController', salaryStructureSheetController);
upanelApp.controller('SandwichAbsentController', SandwichAbsentController);
upanelApp.controller('SandWichLeaveOnHolidayController', SandWichLeaveOnHolidayController);
upanelApp.controller('SecretarialDocumentCategoryController', SecretarialDocumentCategoryController);
upanelApp.controller('SecretarialDocumentSubCategoryController', SecretarialDocumentSubCategoryController);
upanelApp.controller('separatedsalaryStructureController', separatedsalaryStructureController);
upanelApp.controller('servicePayableController', servicePayableController);
upanelApp.controller('SFBonusSheetGridReportController', SFBonusSheetGridReportController);
upanelApp.controller('SFBonusSheetReportController', SFBonusSheetReportController);
upanelApp.controller('shiftAssignmentController', shiftAssignmentController);
upanelApp.controller('shiftAssignmentDeleteController', shiftAssignmentDeleteController);
upanelApp.controller('shiftCreationController', shiftCreationController);
upanelApp.controller('ShiftRosterCreationController', ShiftRosterCreationController);
upanelApp.controller('shiftSummaryController', shiftSummaryController);
upanelApp.controller('skillGroupingController', skillGroupingController);
upanelApp.controller('skillMatrixController', skillMatrixController);
upanelApp.controller('SpecialFollowUpReportController', SpecialFollowUpReportController);
upanelApp.controller('suspensePayableController', suspensePayableController);
upanelApp.controller('TaskCategoryController', TaskCategoryController);
upanelApp.controller('TaskCategoryIssueController', TaskCategoryIssueController);
upanelApp.controller('TaskCategoryToDoController', TaskCategoryToDoController);
upanelApp.controller('taskManagerDashboardController', taskManagerDashboardController);
upanelApp.controller('TaskMasterCreationController', TaskMasterCreationController);
upanelApp.controller('TaskReplacementController', TaskReplacementController);
upanelApp.controller('TaskScheduleController', TaskScheduleController);
upanelApp.controller('TaskSubCategoryController', TaskSubCategoryController);
upanelApp.controller('TaskSubCategoryIssueController', TaskSubCategoryIssueController);
upanelApp.controller('TaskSubCategoryToDoController', TaskSubCategoryToDoController);
upanelApp.controller('TaskTemplateController', TaskTemplateController);
upanelApp.controller('tbsAssignController', tbsAssignController);
upanelApp.controller('tBSController', tBSController);
upanelApp.controller('tiffinBillReportController', tiffinBillReportController);
upanelApp.controller('tiffinBillReportSummaryController', tiffinBillReportSummaryController);
upanelApp.controller('userPasswordChangeController', UserPasswordChangeController);
upanelApp.controller('vendorChargeWriteOffController', vendorChargeWriteOffController);
upanelApp.controller('welfareReportsController', welfareReportsController);
upanelApp.controller('welfareReturnController', welfareReturnController);
upanelApp.controller('WithinYearLeaveEncashmentController', WithinYearLeaveEncashmentController);
upanelApp.controller('workersLateStatusController', workersLateStatusController);
upanelApp.controller('POLCMapController', POLCMapController);
upanelApp.controller("elementCodeController", ElementCodeController);
upanelApp.controller("sewingCodeController", SewingCodeController);
upanelApp.controller("productionSystemAllowanceController", ProductionSystemAllowanceController);
upanelApp.controller("vASElementTypeController", VASElementTypeController);
upanelApp.controller("timeCaptureController", TimeCaptureController);
upanelApp.controller("bartackCodeController", BartackCodeController);
upanelApp.controller("vasReportController", VASReportController);
upanelApp.controller("vasSAMCompareController", VASSAMCompareController);
upanelApp.controller("vasApprovalController", VASApprovalController);
upanelApp.controller('budgetMasterReportController', budgetMasterReportController);
upanelApp.controller('MasterOrderTaskTemplateController', MasterOrderTaskTemplateController);
upanelApp.controller('ExternalDataUploadFromExcelController', ExternalDataUploadFromExcelController);
upanelApp.controller('CropTypeController', CropTypeController);
upanelApp.controller('CropCategoryController', CropCategoryController);
upanelApp.controller('CropSubCategoryController', CropSubCategoryController);
upanelApp.controller('FarmingProcessController', FarmingProcessController);
upanelApp.controller('LandCategoryController', LandCategoryController);
upanelApp.controller('CropMasterController', CropMasterController);
upanelApp.controller('FarmerMasterController', FarmerMasterController);
upanelApp.controller('ICSMasterController', ICSMasterController);
upanelApp.controller('TalukController', TalukController);
upanelApp.controller('VillageController', VillageController);
upanelApp.controller('CropPlanningController', CropPlanningController);
upanelApp.controller('PurchaseBookingSodaController', PurchaseBookingSodaController);
upanelApp.controller('PaymentController', PaymentController);
upanelApp.controller('ConfirmationController', ConfirmationController);
upanelApp.controller('ApprovalController', ApprovalController);
upanelApp.controller('CropRateLocationController', CropRateLocationController);
upanelApp.controller('machineBudgetController', machineBudgetController);
upanelApp.controller('machineTransferController', machineTransferController);
upanelApp.controller('DailyCropRateController', DailyCropRateController);
upanelApp.controller('FarmingCategoryController', FarmingCategoryController);
upanelApp.controller('TransactionTypeController', TransactionTypeController);
//.controller('EmployeeServiceTypeController', EmployeeServiceTypeController)                                                      ;
upanelApp.controller('EmployeeBankAccountInfoController', EmployeeBankAccountInfoController);
upanelApp.controller('JobWorkItemController', JobWorkItemController);
upanelApp.controller('rePrintNonCashCheckController', rePrintNonCashCheckController);
upanelApp.controller('rePrintCashCheckController', rePrintCashCheckController);
upanelApp.controller('MissedPunchReportController', MissedPunchReportController);
upanelApp.controller('ArrearController', ArrearController);
upanelApp.controller('ArrearApprovalController', ArrearApprovalController);
upanelApp.controller('BulkIncrementSalaryStructureDataUploadController', BulkIncrementSalaryStructureDataUploadController);
upanelApp.controller('chourlyOTReportController', chourlyOTReportController);
upanelApp.controller('ProcessAndResourcesConstraintController', ProcessAndResourcesConstraintController);
upanelApp.controller("interCompanyPartyController", InterCompanyPartyController);
upanelApp.controller('EmployeePlantTransferController', EmployeePlantTransferController);
upanelApp.controller('EmployeePlantTransferNewController', EmployeePlantTransferNewController);
upanelApp.controller('CompanyWiseEmployeePlantTransferController', CompanyWiseEmployeePlantTransferController);
upanelApp.controller("inventoryTransferJournalController", inventoryTransferJournalController);
//.controller("jobWorkItemController", JobWorkItemController)                                                                      ;
upanelApp.controller("checkVoidController", checkVoidController);
upanelApp.controller("checkManagementReportController", checkManagementReportController);
upanelApp.controller("wipReportController", wipReportController);
upanelApp.controller("recipeOperationController", recipeOperationController);
upanelApp.controller("utilityController", UtilityController);
upanelApp.controller("UtilityTransactionController", UtilityTransactionController);
upanelApp.controller("UtilityTransactionReportController", UtilityTransactionReportController);
upanelApp.controller("DesignationBudgetController", DesignationBudgetController);
upanelApp.controller("manpowerBudgetDesignationReportController", manpowerBudgetDesignationReportController);
upanelApp.controller("cahourlyOTReportController", cahourlyOTReportController);
upanelApp.controller("balanceSheetReportGroupWiseController", balanceSheetReportGroupWiseController);
upanelApp.controller("bulletinReportController", bulletinReportController);
upanelApp.controller("trialBalanceReportGroupWiseController", trialBalanceReportGroupWiseController);
upanelApp.controller("jwActivityController", jwActivityController);
upanelApp.controller("jwLocationController", jwLocationController);
upanelApp.controller("jwTransformationMasterController", jwTransformationMasterController);
upanelApp.controller("jwItemController", jwItemController);
upanelApp.controller("partyPaymentStatusController", partyPaymentStatusController);
upanelApp.controller("RCMTaxPayableReportController", RCMTaxPayableReportController);
upanelApp.controller("RCMTaxPayableSalesReportController", RCMTaxPayableSalesReportController);
upanelApp.controller("RCMTaxReceivableSalesReportController", RCMTaxReceivableSalesReportController);
upanelApp.controller("TDSDeductionReportController", TDSDeductionReportController);
upanelApp.controller("GSTReceivableReportController", GSTReceivableReportController);
upanelApp.controller("debitNoteCreditNoteTaxReportController", debitNoteCreditNoteTaxReportController);
upanelApp.controller("parkedReportController", parkedReportController);
upanelApp.controller("GSTPayableSalesReportController", GSTPayableSalesReportController);
upanelApp.controller("BonusProcessController", BonusProcessController);
upanelApp.controller("elementCodeController", ElementCodeController);
upanelApp.controller("jobWorkItemController", JobWorkItemController);
upanelApp.controller("jobWorkActivityController", jobWorkActivityController);
upanelApp.controller("jobWorkLocationController", jobWorkLocationController);
upanelApp.controller("jobWorkValueAddedMasterController", jobWorkValueAddedMasterController);
upanelApp.controller("jobWorkTransformationMasterController", jobWorkTransformationMasterController);
upanelApp.controller("JobWorkValueAddedContractController", JobWorkValueAddedContractController);
upanelApp.controller("OSIssueReturnController", OSIssueReturnController);
upanelApp.controller("JobWorkIssueReturnConfirmationController", JobWorkIssueReturnConfirmationController);
upanelApp.controller("JobWorkRegisterController", JobWorkRegisterController);
upanelApp.controller("DailyAttendanceInformationController", DailyAttendanceInformationController);
upanelApp.controller("RCMTaxReceivableReportController", RCMTaxReceivableReportController);
upanelApp.controller("dayBooksReportController", dayBooksReportController);
upanelApp.controller("DailyAttendanceSummeryReportController", DailyAttendanceSummeryReportController);
upanelApp.controller("purchaseReturnPostController", purchaseReturnPostController);
upanelApp.controller("EmployeeServiceVariableController", EmployeeServiceVariableController);
upanelApp.controller("gstR2ReportController", gstR2ReportController);
upanelApp.controller("LeaveDeleteSingleDayController", LeaveDeleteSingleDayController);
upanelApp.controller("LeaveDeleteSingleDayNewController", LeaveDeleteSingleDayNewController);
upanelApp.controller("MonthlyAttendanceSummeryReportController", MonthlyAttendanceSummeryReportController);
upanelApp.controller("OSTransformationPOController", OSTransformationPOController);
upanelApp.controller("ProfessionalTaxOBController", ProfessionalTaxOBController);
upanelApp.controller("TaxOBController", TaxOBController);
upanelApp.controller("EmployeeIncomeTaxController", EmployeeIncomeTaxController);
upanelApp.controller("EmployeeIncomeTaxProcessController", EmployeeIncomeTaxProcessController);
upanelApp.controller("LateDeductionController", LateDeductionController);
upanelApp.controller("EmployeeDayStatusReportController", EmployeeDayStatusReportController);
upanelApp.controller("VoucherController", VoucherController);
upanelApp.controller("BOQGenerationController", BOQGenerationController);
upanelApp.controller("BOQController", BOQController);
upanelApp.controller("SalarySheetBudgetaryOTController", SalarySheetBudgetaryOTController);
upanelApp.controller("IncrementReportController", IncrementReportController);
upanelApp.controller("IncrementReportSummaryController", IncrementReportSummaryController);
upanelApp.controller("partyPaymentStatusDetailController", partyPaymentStatusDetailController);
upanelApp.controller("salaryProcessedReportBudgetaryController", salaryProcessedReportBudgetaryController);
upanelApp.controller('IndividualGratuityPolicyController', IndividualGratuityPolicyController);
upanelApp.controller("LcNavigationController", LcNavigationController);
upanelApp.controller("QuickBOQReportController", QuickBOQReportController);
upanelApp.controller("AttendanceManualDataUploadController", AttendanceManualDataUploadController);
upanelApp.controller('GraruityInsuranceReportController', GraruityInsuranceReportController);
upanelApp.controller('FinalAttendanceProcessController', FinalAttendanceProcessController);
upanelApp.controller('ShiftChangeSectionWiseController', ShiftChangeSectionWiseController);
upanelApp.controller('LeaveBalanceReportController', LeaveBalanceReportController);
upanelApp.controller('LeaveBalanceToDateReportController', LeaveBalanceToDateReportController);
upanelApp.controller('welfareSummaryReportController', welfareSummaryReportController);
upanelApp.controller('salaryStructureReportPlantWiseController', salaryStructureReportPlantWiseController);
upanelApp.controller('GRNUncheckedAndUnApprovedController', GRNUncheckedAndUnApprovedController);
upanelApp.controller('POUncheckedAndUnApprovedController', POUncheckedAndUnApprovedController);
upanelApp.controller('expenseRegisterReportController', expenseRegisterReportController);
upanelApp.controller('ManualAttendanceFileUploadController', ManualAttendanceFileUploadController);
upanelApp.controller('ManualAttendanceWithShiftController', ManualAttendanceWithShiftController);
upanelApp.controller('ProductionDashboardController', ProductionDashboardController);
upanelApp.controller('BOQUploadController', BOQUploadController);
upanelApp.controller('PostSalesInvoiceController', PostSalesInvoiceController);
upanelApp.controller('DailyDayStatusReportController', DailyDayStatusReportController);
upanelApp.controller('InGatePassController', InGatePassController);
upanelApp.controller('MonthlyLunchOutReportController', MonthlyLunchOutReportController);
upanelApp.controller('InGatePassEntryController', InGatePassEntryController);
upanelApp.controller('ServicePOIndividualController', ServicePOIndividualController);
upanelApp.controller('LunchOutDashboardController', LunchOutDashboardController);
upanelApp.controller('hrDashboardtrController', hrDashboardtrController);
upanelApp.controller('OTPlanningController', OTPlanningController);
upanelApp.controller('ManualOTReportController', ManualOTReportController);
upanelApp.controller('DailyAttendanceReportController', DailyAttendanceReportController);
upanelApp.controller('mixingController', mixingController);
upanelApp.controller('RequisitionRegisterController', RequisitionRegisterController);
upanelApp.controller('monthlyAttendanceInformationDateRangeController', monthlyAttendanceInformationDateRangeController);
upanelApp.controller('MonthlyAttendanceInformationDateRangeNewController', MonthlyAttendanceInformationDateRangeNewController);
upanelApp.controller('AttendanceFromAppReportController', AttendanceFromAppReportController);
upanelApp.controller('MultipleEmployeeIndividualLockController', MultipleEmployeeIndividualLockController);
upanelApp.controller('NewProcessAttendanceReProcessController', NewProcessAttendanceReProcessController);
upanelApp.controller('EmployeeLastPunchReportController', EmployeeLastPunchReportController);
upanelApp.controller('EntireYearPresentDaysSummaryController', EntireYearPresentDaysSummaryController);
upanelApp.controller('professionalTaxReportsController', professionalTaxReportsController);
upanelApp.controller("monthlyGoodWorkReportController", monthlyGoodWorkReportController);
upanelApp.controller("monthlyGoodWorkReportNewController", monthlyGoodWorkReportNewController);
upanelApp.controller("weekOffOTReportController", weekOffOTReportController);
upanelApp.controller("weekOffOTReportOriginalController", weekOffOTReportOriginalController);
upanelApp.controller("DispatchMasterController", DispatchMasterController);
upanelApp.controller("FinancialStatusCustomerReceivableInvoiceDetailController", FinancialStatusCustomerReceivableInvoiceDetailController);
upanelApp.controller("holidayOTReportController", holidayOTReportController);
upanelApp.controller("holidayOTReportOriginalController", holidayOTReportOriginalController);
upanelApp.controller("MovementItemsController", MovementItemsController);
upanelApp.controller("MovementMaterialMasterController", MovementMaterialMasterController);
upanelApp.controller("salaryProcessedReportExtraOTCTCController", salaryProcessedReportExtraOTCTCController);
upanelApp.controller("salaryProcessedReportExtraOTCTCOriginalController", salaryProcessedReportExtraOTCTCOriginalController);
upanelApp.controller("FarmingDashboardController", FarmingDashboardController);
upanelApp.controller("BOQPurchaseOrderController", BOQPurchaseOrderController);
upanelApp.controller("PhysicalVerificationReportController", PhysicalVerificationReportController);
upanelApp.controller("VisitorListReportController", VisitorListReportController);
upanelApp.controller("payRegisterBDReportContractorController", payRegisterBDReportContractorController);
upanelApp.controller("yearlySalaryProcessedReportController", yearlySalaryProcessedReportController);
upanelApp.controller("MovementScanDataReportController", MovementScanDataReportController);
upanelApp.controller("OS3DashboardController", OS3DashboardController);
upanelApp.controller("WeighingScaleReportController", WeighingScaleReportController);
upanelApp.controller("ProductionPlanningReportController", ProductionPlanningReportController);
upanelApp.controller("BlackListController", BlackListController);
upanelApp.controller("JobEvaluationAttributeController", JobEvaluationAttributeController);
upanelApp.controller("JobEvaluationMasterController", JobEvaluationMasterController);
upanelApp.controller("JobEvaluationController", JobEvaluationController);
upanelApp.controller("JobEvaluationReportController", JobEvaluationReportController);
upanelApp.controller("consecutiveAttendaceController", consecutiveAttendaceController);
upanelApp.controller("consecutiveOTHoursController", consecutiveOTHoursController);
upanelApp.controller("FGValuationController", FGValuationController);
upanelApp.controller("bonusProvisionReportController", bonusProvisionReportController);
upanelApp.controller("bonusReportCController", bonusReportCController);
upanelApp.controller("paySlipsContractorController", paySlipsContractorController);
upanelApp.controller("PackingController", PackingController);
upanelApp.controller("GatePassRegisterController", GatePassRegisterController);
upanelApp.controller("bankSheetGenerationController", bankSheetGenerationController);
upanelApp.controller("salaryStructureSheetDailyController", salaryStructureSheetDailyController);
upanelApp.controller("MaterialReconcilationReportController", MaterialReconcilationReportController);
upanelApp.controller("OSReceiptValueAddedController", OSReceiptValueAddedController);
upanelApp.controller("ExceptionOTProcessController", ExceptionOTProcessController);
upanelApp.controller("FinishGoodsBookingController", FinishGoodsBookingController);
upanelApp.controller("ConsumptionBookingController", ConsumptionBookingController);
upanelApp.controller("AuditReportSummeryController", AuditReportSummeryController);
upanelApp.controller("AuditReportSummaryNewController", AuditReportSummaryNewController);
upanelApp.controller("CompanyProvidentFundStatementReportController", CompanyProvidentFundStatementReportController);
upanelApp.controller("ESICStatementsCompanyController", ESICStatementsCompanyController);
upanelApp.controller("GratuityReportCompanyController", GratuityReportCompanyController);
upanelApp.controller("EmployeeAdvanceDeductionController", EmployeeAdvanceDeductionController);
upanelApp.controller("RackController", RackController);
upanelApp.controller("PORollBackController", PORollBackController);
upanelApp.controller("DailyTargetController", DailyTargetController);
upanelApp.controller("jwPOIssueController", jwPOIssueController);
upanelApp.controller("EmployeeAdditionDeductionController", EmployeeAdditionDeductionController);
upanelApp.controller("generalLedgerVSfixedAssetsController", generalLedgerVSfixedAssetsController);
upanelApp.controller("ProductionRelayController", ProductionRelayController);
upanelApp.controller("inventoryOutSourceReceivePostController", inventoryOutSourceReceivePostController);
upanelApp.controller("ManualShiftController", ManualShiftController);
upanelApp.controller("ManualShiftNewController", ManualShiftNewController);
upanelApp.controller("OSReceiveBillingController", OSReceiveBillingController);
upanelApp.controller("entityFixedAssetsRegisterController", entityFixedAssetsRegisterController);
upanelApp.controller("voucherParkController", voucherParkController);
upanelApp.controller("voucherPrintController", voucherPrintController);
upanelApp.controller("salaryProcessedReportExtraOTCTCCompanyController", salaryProcessedReportExtraOTCTCCompanyController);

upanelApp.controller("EmployeeAdditionDeductionProcessController", EmployeeAdditionDeductionProcessController);
upanelApp.controller("MarkerController", MarkerController);
upanelApp.controller("partyPaymentStatusReportController", partyPaymentStatusReportController);
upanelApp.controller("OTManualNewController", OTManualNewController);
upanelApp.controller("ManualOTUploadNewController", ManualOTUploadNewController);
upanelApp.controller("ManualOTReportNewController", ManualOTReportNewController);
upanelApp.controller("MasterPlanController", MasterPlanController);
upanelApp.controller("FinishGoodsBookingPostController", FinishGoodsBookingPostController);
upanelApp.controller("PackingInvoiceController", PackingInvoiceController);
upanelApp.controller("CompanyWiseExternalDataUploadFromExcelController", CompanyWiseExternalDataUploadFromExcelController);
upanelApp.controller("CompanyWiseBankSheetController", CompanyWiseBankSheetController);
upanelApp.controller("PayrollManagementDashboardController", PayrollManagementDashboardController);
upanelApp.controller("InventorySalesReturnController", InventorySalesReturnController);
upanelApp.controller("ProductionConversionParameterController", ProductionConversionParameterController);
upanelApp.controller("ProductionTransformationBookingController", ProductionTransformationBookingController);
upanelApp.controller("EmployeeJobLocationController", EmployeeJobLocationController);
upanelApp.controller("salesPackingPostController", salesPackingPostController);
upanelApp.controller("AttendanceRawDataFromAppController", AttendanceRawDataFromAppController);
upanelApp.controller("WeekOffUpdatesController", WeekOffUpdatesController);
upanelApp.controller("RosterUpdatesController", RosterUpdatesController);
upanelApp.controller("OrderController", OrderController);
upanelApp.controller("SalesOrderUpdateController", SalesOrderUpdateController);
upanelApp.controller("POParameterChangeController", POParameterChangeController);

upanelApp.controller("NewAttdnDashboardController", NewAttdnDashboardController);
upanelApp.controller("EmployeeBudgetUpdateController", EmployeeBudgetUpdateController);
upanelApp.controller("AttendanceDashboardController", AttendanceDashboardController);
upanelApp.controller("NewAttdnProcessLockController", NewAttdnProcessLockController);
upanelApp.controller("NewHRDashboardController", NewHRDashboardController);
upanelApp.controller("OutsourceBillingPostController", OutsourceBillingPostController);
upanelApp.controller("ProductionOrderProcessWithRateController", ProductionOrderProcessWithRateController);
upanelApp.controller("entityWiseExpenseAndEarningController", entityWiseExpenseAndEarningController);
upanelApp.controller("ProductionOrderRateReportController", ProductionOrderRateReportController);
upanelApp.controller("EmployeeLeaveApplicationNewController", EmployeeLeaveApplicationNewController);
upanelApp.controller('EmployeeLeaveApprovalNewController', EmployeeLeaveApprovalNewController);
upanelApp.controller('employeeLeaveDeleteApplicationNewController', employeeLeaveDeleteApplicationNewController);
upanelApp.controller('ProductionTargetReportController', ProductionTargetReportController);
upanelApp.controller('FabricRollController', FabricRollController);
upanelApp.controller('FabricRollsController', FabricRollsController);
upanelApp.controller('FinalDeductionReportController', FinalDeductionReportController);

upanelApp.controller("PostInvoiceController", PostInvoiceController);
upanelApp.controller('GRNRequisitionSOAllocationController', GRNRequisitionSOAllocationController);
upanelApp.controller('salaryProcessedReportControllerNew', salaryProcessedReportControllerNew);
upanelApp.controller('salaryStructureAndProcessedReportNewController', salaryStructureAndProcessedReportNewController);
upanelApp.controller('finishGoodsInventoryRegisterController', finishGoodsInventoryRegisterController);
upanelApp.controller('LineLayoutForProductionBulletinController', LineLayoutForProductionBulletinController);
upanelApp.controller('EmployeeWeekOffUpdatesController', EmployeeWeekOffUpdatesController);
upanelApp.controller('SalaryDisbursementReportController', SalaryDisbursementReportController);
upanelApp.controller('OSissueRegisterController', OSissueRegisterController);
upanelApp.controller('SandwichProcessController', SandwichProcessController);
upanelApp.controller('SandwichProcessPlantWiseController', SandwichProcessPlantWiseController);
upanelApp.controller('JobWorkTransformationPOController', JobWorkTransformationPOController);
upanelApp.controller('AssetWIPStatusController', AssetWIPStatusController);
upanelApp.controller('OTConfirmationProcessController', OTConfirmationProcessController);
upanelApp.controller("multipleResignationApprovalNewController", multipleResignationApprovalNewController);
upanelApp.controller('JWIssueReturnController', JWIssueReturnController);
upanelApp.controller('MachineLayoutReportController', MachineLayoutReportController);
upanelApp.controller('JWReceiptController', JWReceiptController);
upanelApp.controller('LeavesChecklistReportNewController', LeavesChecklistReportNewController);
upanelApp.controller('WasteTransactionReportController', WasteTransactionReportController);
upanelApp.controller('ProformaInvoiceController', ProformaInvoiceController);

upanelApp.controller('GeneralDataMasterController', GeneralDataMasterController);
upanelApp.controller('GeneralDataOperationsController', GeneralDataOperationsController);
upanelApp.controller('InvoiceTaggedWithLCController', InvoiceTaggedWithLCController);
upanelApp.controller('invoiceToAcceptancePostController', invoiceToAcceptancePostController);
upanelApp.controller('PIInvoiceController', PIInvoiceController);
upanelApp.controller('PIPackingListController', PIPackingListController);
upanelApp.controller('FOCController', FOCController);

upanelApp.controller('POMappingWithPIController', POMappingWithPIController);
upanelApp.controller('GeneralWasteController', GeneralWasteController);
upanelApp.controller("issueTransactionController", issueTransactionController);
upanelApp.controller("EmployeeShiftUpdatesController", EmployeeShiftUpdatesController);
upanelApp.controller("ProductionGeneralReportController", ProductionGeneralReportController);
upanelApp.controller("inventorySalesReturnPost", inventorySalesReturnPost);
upanelApp.controller("ServicePORegisterController", ServicePORegisterController);
upanelApp.controller("StocksAgeingReportController", StocksAgeingReportController);
upanelApp.controller("NewEarnLeaveReportController", NewEarnLeaveReportController);
upanelApp.controller("NewSystemEarnLeaveReportController", NewSystemEarnLeaveReportController);
upanelApp.controller("FinishedStockReportController", FinishedStockReportController);
upanelApp.controller("FGInventoryStockReportController", FGInventoryStockReportController);
upanelApp.controller("MeetingTypeController", MeetingTypeController);
upanelApp.controller("MeetingAgendaController", MeetingAgendaController);
upanelApp.controller("StocksAdjustmentController", StocksAdjustmentController);
upanelApp.controller("FinalPackDefinitionController", FinalPackDefinitionController);
upanelApp.controller("PrePackDefinitionController", PrePackDefinitionController);
upanelApp.controller("MeetingReportsController", MeetingReportsController);
upanelApp.controller("EmployeeOperationsController", EmployeeOperationsController);
upanelApp.controller("GroupBalanceReportController", GroupBalanceReportController);
upanelApp.controller("WasteIssueController", WasteIssueController);
upanelApp.controller("inventoryIssueBOQController", inventoryIssueBOQController);
upanelApp.controller("WasteMasterController", WasteMasterController);
upanelApp.controller("WasteLocationController", WasteLocationController);
upanelApp.controller("BOQApprovalController", BOQApprovalController);
upanelApp.controller("VoucherGlUpdateController", VoucherGlUpdateController);
upanelApp.controller("CustomerConfirmationController", CustomerConfirmationController);
upanelApp.controller("DetentionMasterController", DetentionMasterController);
upanelApp.controller("UtilityMasterController", UtilityMasterController);
upanelApp.controller("AbsentismReasoningMasterController", AbsentismReasoningMasterController);
upanelApp.controller("InventorySalesReportMarketingController", InventorySalesReportMarketingController);
upanelApp.controller("CurrentFundPositionController", CurrentFundPositionController);

upanelApp.controller("EmployeeTimeOutController", EmployeeTimeOutController);
upanelApp.controller("postDateChequeController", postDateChequeController);
upanelApp.controller("vendorPaymentApprovalController", vendorPaymentApprovalController);
upanelApp.controller("ManpowerControlReportsController", ManpowerControlReportsController);
upanelApp.controller("SalesOrderStatusReportController", SalesOrderStatusReportController);

upanelApp.controller("BOQStatusReportController", BOQStatusReportController);
upanelApp.controller("FuguaiTransactionController", FuguaiTransactionController);
upanelApp.controller("FuguaiReportController", FuguaiReportController);
upanelApp.controller("TaskManagementReportController", TaskManagementReportController);
upanelApp.controller("salesRegisterController", salesRegisterController);
upanelApp.controller("EmployeeSkillMatrixController", EmployeeSkillMatrixController);
upanelApp.controller("OTControlLimitController", OTControlLimitController);
upanelApp.controller("OTControlLimitReportController", OTControlLimitReportController);
upanelApp.controller("BuyerjobCardcomplianceReportController", BuyerjobCardcomplianceReportController);
upanelApp.controller("EOTSheetController", EOTSheetController);
upanelApp.controller("StorageBinAllocationController", StorageBinAllocationController);
upanelApp.controller("ResidenceStatusAllocationController", ResidenceStatusAllocationController);
upanelApp.controller("ResidenceStatusAllocationReportController", ResidenceStatusAllocationReportController);
upanelApp.controller("PurchaseConfirmationController", PurchaseConfirmationController);
upanelApp.controller("OTCompensatoryAllocationController", OTCompensatoryAllocationController);
upanelApp.controller("FurniturePolicyController", FurniturePolicyController);
upanelApp.controller("FurnitureMasterController", FurnitureMasterController);
upanelApp.controller("ResidenceMasterController", ResidenceMasterController);
upanelApp.controller("ProcessWiseProductionBookingController", ProcessWiseProductionBookingController);
upanelApp.controller("FuguaiZoneMasterController", FuguaiZoneMasterController);
upanelApp.controller("ProductionBookingProcessparameterController", ProductionBookingProcessparameterController)
upanelApp.controller("EOTController", EOTController)
upanelApp.controller("QuaityProcessBookingController", QuaityProcessBookingController)
upanelApp.controller("FurniturePolicyReportController", FurniturePolicyReportController)
upanelApp.controller("FiveSZoneMasterController", FiveSZoneMasterController)
upanelApp.controller("ProductionSummaryWCController", ProductionSummaryWCController)
upanelApp.controller("StockRegisterController", StockRegisterController)
upanelApp.controller("ProductionReportWithParameterController", ProductionReportWithParameterController)
upanelApp.controller("ProcessWiseMaterialAllocationController", ProcessWiseMaterialAllocationController)
upanelApp.controller("RequisitionStatusController", RequisitionStatusController)
upanelApp.controller("EInvoiceController", EInvoiceController)
upanelApp.controller("RouteEmployeeReportController", RouteEmployeeReportController)
upanelApp.controller("SurveyandFeedbackController", SurveyandFeedbackController)
upanelApp.controller("ProductionSummaryReportController", ProductionSummaryReportController)
upanelApp.controller("maintenanceSchedulingController", maintenanceSchedulingController)
upanelApp.controller("DetentionTypeController", DetentionTypeController)
upanelApp.controller("maintenanceStatusDetailsController", maintenanceStatusDetailsController)
upanelApp.controller("PositionWiseMPStatusController", PositionWiseMPStatusController)
upanelApp.controller("MedicineMasterController", MedicineMasterController)
upanelApp.controller("SicknessTypeController", SicknessTypeController)
upanelApp.controller("SalesOrderApprovalController", SalesOrderApprovalController)
upanelApp.controller("MedicinePurposeController", MedicinePurposeController)
upanelApp.controller("MedicineReceiptController", MedicineReceiptController)
upanelApp.controller("MedicalLogController", MedicalLogController)
upanelApp.controller("MedicalLogReportController", MedicalLogReportController)
upanelApp.controller("BOMDetailMasterController", BOMDetailMasterController)
upanelApp.controller("pendingMaintenanceScheduleController", pendingMaintenanceScheduleController)
upanelApp.controller("specialIssueControlController", specialIssueControlController)
upanelApp.controller("specialIssueControlUpdateController", specialIssueControlUpdateController)
upanelApp.controller("IssueControlController", IssueControlController)
upanelApp.controller("MedicineCategoryController", MedicineCategoryController)
upanelApp.controller("MaterialIssueControlController", MaterialIssueControlController)
upanelApp.controller("specialIssueControlRegisterController", specialIssueControlRegisterController)
upanelApp.controller("MaterialIssueControlApprovalController", MaterialIssueControlApprovalController)
upanelApp.controller("specialIssueControlReportController", specialIssueControlReportController)
upanelApp.controller("FinishedGoodsPackingReportController", FinishedGoodsPackingReportController)
upanelApp.controller("MaterialIssueController", MaterialIssueController)
upanelApp.controller("incedentCategoryController", incedentCategoryController)
upanelApp.controller("incedentCategoryUpdateController", incedentCategoryUpdateController)
upanelApp.controller("incedentUpdateController", incedentUpdateController)
upanelApp.controller("maintenanceSummaryReportController", maintenanceSummaryReportController)
upanelApp.controller("POWiseProductionStatusReportController", POWiseProductionStatusReportController)
upanelApp.controller("teamDefinitionController", teamDefinitionController)
upanelApp.controller("ParameterMasterController", ParameterMasterController)
upanelApp.controller("ParameterController", ParameterController)
upanelApp.controller("GeneralContractItemMasterController", GeneralContractItemMasterController)
upanelApp.controller("GeneralContractController", GeneralContractController)
upanelApp.controller("maintenancePlanningReportController", maintenancePlanningReportController)
upanelApp.controller("teamPlanReportController", teamPlanReportController)
upanelApp.controller("GeneralContractEntryController", GeneralContractEntryController)
upanelApp.controller("GeneralContractReportController", GeneralContractReportController)
upanelApp.controller("GeneralContractApprovedController", GeneralContractApprovedController)
upanelApp.controller("GeneralContractCheckedController", GeneralContractCheckedController)
upanelApp.controller("skillManagementController", skillManagementController)
upanelApp.controller("skillManagementDetailsController", skillManagementDetailsController)
upanelApp.controller("pendingSkillManagementController", pendingSkillManagementController)
upanelApp.controller("LandedcostreportController", LandedcostreportController)
upanelApp.controller("OutPassRegisterController", OutPassRegisterController)
upanelApp.controller("masterOrderSalesAdditionalController", masterOrderSalesAdditionalController)
upanelApp.controller("ScanDataController", ScanDataController)
upanelApp.controller("ProductionReportController", ProductionReportController)
upanelApp.controller("SalaryNotDisbursedController", SalaryNotDisbursedController)
upanelApp.controller("PackingScanDataController", PackingScanDataController)
upanelApp.controller("positionWiseDesignationController", positionWiseDesignationController)
upanelApp.controller("ResignationTypeController", ResignationTypeController)
upanelApp.controller("DailyAttendanceStatusReportController", DailyAttendanceStatusReportController)
upanelApp.controller("MaterialIssueReportController", MaterialIssueReportController)
upanelApp.controller("RawMaterialPlanningController", RawMaterialPlanningController)
upanelApp.controller("ProductIntegrityAnalysisMasterController", ProductIntegrityAnalysisMasterController)
upanelApp.controller("LeaveRegistersFormController", LeaveRegistersFormController)
upanelApp.controller("ProductIntegrityAnalysisController", ProductIntegrityAnalysisController)
upanelApp.controller("SalesOrderWiseProductionCompletionReportController", SalesOrderWiseProductionCompletionReportController)
upanelApp.controller("InWardMaterialController", InWardMaterialController)
upanelApp.controller("ProductivityRecoveryMasterController", ProductivityRecoveryMasterController)
upanelApp.controller("SalesReturnController", SalesReturnController)
upanelApp.controller("QualityManagementMasterController", QualityManagementMasterController)
upanelApp.controller("ProductionControlController", ProductionControlController)
upanelApp.controller("POWiseMaterialIssueController", POWiseMaterialIssueController)
upanelApp.controller("InputConfirmationController", InputConfirmationController)
upanelApp.controller("SalesReturnPostController", SalesReturnPostController)
upanelApp.controller("EditControlController", EditControlController)
upanelApp.controller("RunningMachineSetUpTargetController", RunningMachineSetUpTargetController)
upanelApp.controller("DailyPlanningAndProductionReportController", DailyPlanningAndProductionReportController)
upanelApp.controller("EmployeeAttendanceReportController", EmployeeAttendanceReportController)
upanelApp.controller("HRReportMasterController", HRReportMasterController)
upanelApp.controller("WCWorkStationsControlMasterController", WCWorkStationsControlMasterController)
upanelApp.controller("WCWorkStationsControlController", WCWorkStationsControlController)
upanelApp.controller("BudgetCodeWiseHRReportController", BudgetCodeWiseHRReportController)
upanelApp.controller("WCWorkStationsControlReportController", WCWorkStationsControlReportController)
upanelApp.controller("BudgetReportMasterController", BudgetReportMasterController)
upanelApp.controller("WorkcenterWiseDetentionController", WorkcenterWiseDetentionController)
upanelApp.controller("TaskCloserMasterController", TaskCloserMasterController)
upanelApp.controller("WebBasedPackingController", WebBasedPackingController)
upanelApp.controller("GoodWorkController", GoodWorkController)
upanelApp.controller("GoodWorkDateChangeController", GoodWorkDateChangeController)
upanelApp.controller("salaryProcessedReportComController", salaryProcessedReportComController)
upanelApp.controller("VehicleMovementMasterController", VehicleMovementMasterController)
/*upanelApp.controller("VehicleMovementRequisitionController", VehicleMovementRequisitionController)*/
upanelApp.controller("VehicleReqForApproveController", VehicleReqForApproveController)
upanelApp.controller("VehicleInOutController", VehicleInOutController)
upanelApp.controller("VehicleMovementController", VehicleMovementController)
upanelApp.controller("ProductionIssueControlController", ProductionIssueControlController)
upanelApp.controller("ProcessQualityControlController", ProcessQualityControlController)
upanelApp.controller("QualityControlController", QualityControlController)
upanelApp.controller("GLControlController", GLControlController)
upanelApp.controller("WorkCenterQualityControlMasterController", WorkCenterQualityControlMasterController)
upanelApp.controller("QRCodeGeneratorController", QRCodeGeneratorController)
upanelApp.controller("PlantInOutControllReportController", PlantInOutControllReportController)
upanelApp.controller("ComplaintController", ComplaintController)
upanelApp.controller("CustomerQualityAndTechnicalSupportController", CustomerQualityAndTechnicalSupportController)
upanelApp.controller("PaymentAdviseReportController", PaymentAdviseReportController)
upanelApp.controller("faRegisterController", faRegisterController)
upanelApp.controller("WeighingScaleMasterController", WeighingScaleMasterController)
upanelApp.controller("LOTCreationController", LOTCreationController)
upanelApp.controller("CapitalizeAssetRegisterPostingController", CapitalizeAssetRegisterPostingController)
upanelApp.controller("assetDepreciationProcessController", assetDepreciationProcessController);
upanelApp.controller("VehicleTripController", VehicleTripController)
upanelApp.controller("ProcessManagementController", ProcessManagementController)
upanelApp.controller("masterLCAmendmentController", masterLCAmendmentController)
upanelApp.controller("AdditionalInfoUpdateController", AdditionalInfoUpdateController)
upanelApp.controller("ProductParameterMasterController", ProductParameterMasterController)
upanelApp.controller("ProcessTemplateController", ProcessTemplateController)
upanelApp.controller("PayableCreationAndWorkerAdvanceController", PayableCreationAndWorkerAdvanceController)
upanelApp.controller("EmployeeMultipleAdvanceController", EmployeeMultipleAdvanceController)
upanelApp.controller("QualityActionUpdateController", QualityActionUpdateController)
upanelApp.controller("QualityActionConfirmationController", QualityActionConfirmationController)
upanelApp.controller('AssetsRegisterReportController', AssetsRegisterReportController);
upanelApp.controller('assetDepreciationPostController', assetDepreciationPostController);
upanelApp.controller('assetsDepreciationReportController', assetsDepreciationReportController);
upanelApp.controller('documentationController', documentationController);
upanelApp.controller("QualityActionUpdateReportController", QualityActionUpdateReportController)
upanelApp.controller("ProcessParameterMasterController", ProcessParameterMasterController)
upanelApp.controller("BtbPerformanceController", BtbPerformanceController)
upanelApp.controller("CustomerRequirementControlController", CustomerRequirementControlController)
upanelApp.controller("CustomerConfirmationParameterController", CustomerConfirmationParameterController)
upanelApp.controller("CustomerCompletedParameterController", CustomerCompletedParameterController)
upanelApp.controller("partyApprovalController", partyApprovalController)
upanelApp.controller("DefineProcessParameterController", DefineProcessParameterController)
upanelApp.controller("ParameterSettingControlController", ParameterSettingControlController)
upanelApp.controller("OrderWiseQualityReportController", OrderWiseQualityReportController)
upanelApp.controller("GoodWorkSetupController", GoodWorkSetupController)
upanelApp.controller("ServiceAcknowledgementController", ServiceAcknowledgementController)
upanelApp.controller("InvestmentSettelmentController", InvestmentSettelmentController)
upanelApp.controller("LotControlController", LotControlController)
upanelApp.controller("VehicleReportController", VehicleReportController)
upanelApp.controller("DailyQualityStatusReportController", DailyQualityStatusReportController)
upanelApp.controller("SalesChalanController", SalesChalanController)
upanelApp.controller("ExpenseDistributionReportController", ExpenseDistributionReportController)
upanelApp.controller("SubsequentInvestmentController", SubsequentInvestmentController)
upanelApp.controller("SalesChanlanCheckedController", SalesChanlanCheckedController)
upanelApp.controller("SalesChanlanDispatchConfirmationController", SalesChanlanDispatchConfirmationController)
upanelApp.controller("SQCMasterController", SQCMasterController)
upanelApp.controller("PaymentAdviceController", PaymentAdviceController)
upanelApp.controller("DefineSQCIssueController", DefineSQCIssueController)
upanelApp.controller("InvoiceStatusController", InvoiceStatusController)
upanelApp.controller("MasterPlanDetailsController", MasterPlanDetailsController)
upanelApp.controller("CutPlanController", CutPlanController)
upanelApp.controller("LotWiseQualityReportController", LotWiseQualityReportController)
upanelApp.controller("LWQSummaryReportController", LWQSummaryReportController)
upanelApp.controller("LWQRUpdateController", LWQRUpdateController)
upanelApp.controller("LCPendingReportController", LCPendingReportController)
upanelApp.controller("GoodWorkCheckedController", GoodWorkCheckedController)
upanelApp.controller("GoodWorkApproveController", GoodWorkApproveController)
upanelApp.controller("PettyCashMasterController", PettyCashMasterController)
upanelApp.controller("CutPlanEditController", CutPlanEditController)
upanelApp.controller("GoodWorkPaymentDisburseController", GoodWorkPaymentDisburseController)
upanelApp.controller("masterOrderCheckByController", masterOrderCheckByController)
upanelApp.controller("masterOrderApproveByController", masterOrderApproveByController)
upanelApp.controller("BalanceSheetSchedulingReportController", BalanceSheetSchedulingReportController)
upanelApp.controller("ContractSummaryController", ContractSummaryController)
upanelApp.controller("roundOffJournalController", roundOffJournalController)

upanelApp.controller("BarcodeGeneratorSettingController", BarcodeGeneratorSettingController)
upanelApp.controller("GLManagementController", GLManagementController)
upanelApp.controller("BudgetControlController", BudgetControlController)
upanelApp.controller("EmployeeSeperationSetupController", EmployeeSeperationSetupController)
upanelApp.controller("fullandfinalSettlementController", fullandfinalSettlementController)
upanelApp.controller("LeaveTransectionController", LeaveTransectionController)
upanelApp.controller("otApproveController", otApproveController)
upanelApp.controller("GoodWorkReportController", GoodWorkReportController)
upanelApp.controller("productionOrderSchedulingParametersType1NewController", ProductionOrderSchedulingParametersType1NewController);
upanelApp.controller("productionOrderSchedulingParametersType2Controller", ProductionOrderSchedulingParametersType2Controller);
upanelApp.controller("fullandfinalSettlementApproveController", fullandfinalSettlementApproveController);
upanelApp.controller("fullandfinalSettlementPaymentController", fullandfinalSettlementPaymentController);
upanelApp.controller("OrderLineCostingItemServiceMasterMappingController", OrderLineCostingItemServiceMasterMappingController);
upanelApp.controller("serviceControlController", serviceControlController);
upanelApp.controller("AssetManagementController", AssetManagementController);
upanelApp.controller("EmpDocAssetTransectionController", EmpDocAssetTransectionController);
upanelApp.controller("InputCreditController", InputCreditController);
upanelApp.controller("InputCreditApproveController", InputCreditApproveController);
upanelApp.controller("InputCreditCheckController", InputCreditCheckController);
upanelApp.controller("IndependentServiceGRNController", IndependentServiceGRNController);
upanelApp.controller("SalesProcessController", SalesProcessController);
upanelApp.controller("FNFReportController", FNFReportController);
upanelApp.controller("GeneralApprovedApplicationController", GeneralApprovedApplicationController);
upanelApp.controller("productionOrderType2Controller", productionOrderType2Controller);
upanelApp.controller("DebitCreditNoteProcessControlController", DebitCreditNoteProcessControlController)
upanelApp.controller("SalaryAdviceController", SalaryAdviceController)
upanelApp.controller("AssignControlSetupController", AssignControlSetupController)
upanelApp.controller("JobWorkEntryController", JobWorkEntryController)
upanelApp.controller("SpecialDutyController", SpecialDutyController)
upanelApp.controller("SpecialDutyReportController", SpecialDutyReportController)
upanelApp.controller("ProcessConstraintController", ProcessConstraintController)
upanelApp.controller("invoiceReviseMaturedateController", invoiceReviseMaturedateController)
upanelApp.controller("ProductionCuttingBookingController", ProductionCuttingBookingController)
upanelApp.controller("salesOrderRevisedateController", salesOrderRevisedateController)
upanelApp.controller("atAGlanceBIController", atAGlanceBIController)
upanelApp.controller("manpowerStatusBIController", manpowerStatusBIController)
upanelApp.controller("orderStatusBIController", orderStatusBIController)
upanelApp.controller("accountStatusBIController", accountStatusBIController)
upanelApp.controller("atAGlanceBIController", atAGlanceBIController)
upanelApp.controller("FabricGroupingController", FabricGroupingController)
upanelApp.controller("MarkerCheckController", MarkerCheckController)
upanelApp.controller("MarkerApproveController", MarkerApproveController)
upanelApp.controller("ComplianceController", ComplianceController)
upanelApp.controller("ExportDBController", ExportDBController)
upanelApp.controller("ComplianceTransactionController", ComplianceTransactionController)
upanelApp.controller("employeeOperationVariationUploadController", employeeOperationVariationUploadController)
upanelApp.controller("employeeOperationUploadController", employeeOperationUploadController)
upanelApp.controller("ComplianceAuditController", ComplianceAuditController)
upanelApp.controller("manpowerBudgetReportController", manpowerBudgetReportController)
upanelApp.controller("QualityProcessNewController", QualityProcessNewController)
upanelApp.controller("masterOrderUploadController", masterOrderUploadController)
upanelApp.controller("DefectController", DefectController)




upanelApp.config(AccessControllerConfig);
upanelApp.config(AdministrationConfig);
upanelApp.config(accountConfig);
upanelApp.config(bankConfig);
upanelApp.config(BiometricConfig);
upanelApp.config(CommercialConfig);
upanelApp.config(CostingsConfig);
upanelApp.config(employeeConfig);
upanelApp.config(EmployeeServicesConfig);
upanelApp.config(FarmingConfig);
upanelApp.config(fixedAssetConfig);
upanelApp.config(HumanResourceConfig);
upanelApp.config(IEConfig);
upanelApp.config(IssueTrackerConfig);
upanelApp.config(leaveConfig);
upanelApp.config(MachineConfig);
upanelApp.config(MaterialConfig);
upanelApp.config(OrderManagementConfig);
upanelApp.config(OrganizationConfig);
upanelApp.config(PartyConfig);
upanelApp.config(PayrollsConfig);
upanelApp.config(ProcessConfig);
upanelApp.config(ProductConfig);
upanelApp.config(ProductionsConfig);
upanelApp.config(ProjectConfig);
upanelApp.config(qmsConfig);
upanelApp.config(salesManagementConfig);
upanelApp.config(SecurityConfig);
upanelApp.config(SetupConfig);
upanelApp.config(SkillConfig);
upanelApp.config(TaskManagementConfig);
upanelApp.config(WorkCenterConfig);
upanelApp.config(JobWorkConfig);
upanelApp.config(OutsourcingConfig);
upanelApp.config(PerformanceManagementConfig);
upanelApp.config(MeetingManagementConfig);
upanelApp.config(misConfig);

upanelApp.config(["$routeProvider", "$locationProvider", "$httpProvider", function apanelConfig($routeProvider, $locationProvider, $httpProvider) {
    $httpProvider.interceptors.push("errorInterceptor");
    $httpProvider.defaults.headers.common["X-Requested-With"] = "XMLHttpRequest";
    $routeProvider
        .when("/", {
            templateUrl: "UPanel/PlantSelection",
            controller: "plantSelectionController"
        })
        .when("/plant-selection", {
            templateUrl: "UPanel/PlantSelection",
            controller: "plantSelectionController"
        })
        .when("/dashboard", {
            templateUrl: "upanel/dashboard",
            controller: "upanelDashboardController"
        })
        .when("/login", {
            templateUrl: "aPanel/login",
            controller: "upanelLoginController"
        })
        .when("/logout", {
            template: " ",
            controller: "upanelLogoutController"
        })
        .when("/404/:msg", {
            templateUrl: function (params) {
                return "/error/httperror404?message=" + params.msg;
            }
        })
        .when("/405/:msg", {
            templateUrl: function (params) {
                return "/error/httperror405?message=" + params.msg;
            }
        })
        .otherwise({
            redirectTo: "/portal"
        });
}]);
upanelApp.run(["$rootScope", "$cookies", "$window", "$location", "$filter", "baseService", "$http", '$sce', 'SignalRInit',
    function ($rootScope, $cookies, $window, $location, $filter, baseService, $http, $sce, SignalRInit) {
        function getCookie(cname) {
            var name = cname + "=";
            var decodedCookie = decodeURIComponent(document.cookie);
            var ca = decodedCookie.split(';');
            for (var i = 0; i < ca.length; i++) {
                var c = ca[i];
                while (c.charAt(0) === ' ') {
                    c = c.substring(1);
                }
                if (c.indexOf(name) === 0)
                    return c.substring(name.length, c.length);
            }
            return "";
        }

        function isImage(src) {

            var deferred = $q.defer();

            var image = new Image();
            image.onerror = function () {
                deferred.resolve(false);
            };
            image.onload = function () {
                deferred.resolve(true);
            };
            image.src = src;

            return deferred.promise;
        }
        $rootScope.ShowHomeButton = true;
        $rootScope.FormTitle = "Application";
        $rootScope.FormIcon = null;
        $rootScope.NotificationMessage = '';
        $rootScope.plantName = $cookies.get("plantName");
        $rootScope.companyName = $cookies.get("CompanyFullName");
        $rootScope.bootPoint = "#!/";
        $window.companyGroupId = $cookies.get("groupId");
        $window.authenticationToken = $cookies.get("authToken");
        $window.companyId = $cookies.get("companyId");
        $window.plantId = $cookies.get("plantId");
        $window.employeeId = $cookies.get("employeeId");
        $window.employeeName = $cookies.get("FullName");
        $rootScope.CompanyLogo = null;
        $rootScope.CompanyFullName = null;
        $rootScope.companyGroupLogo = virtualPath.LogoOrImage + $cookies.get("gImage");
        $rootScope.userImage = virtualPath.EmployeeImage + $cookies.get("userImage");
        $rootScope.showMenu = "Module";
        $rootScope.menuModuleId = null;
        $rootScope.isLeftMenuHide = $rootScope.plantName === null || $rootScope.plantName === undefined ? true : false;
        $rootScope.ShowFavouriteMenu = $rootScope.isLeftMenuHide;
        SignalRInit.connect();

        $rootScope.moduleShowHide = function () {

            $rootScope.menuModuleName = null;
            if ($rootScope.showMenu === "Menu")
                $rootScope.showMenu = "Module";
        };
        $rootScope.getPageTitle = function (name) {
            $rootScope.FormTitle = name;
        };

        $rootScope.CurrentMenuMasterId = null;
        $rootScope.CurrentHref = '';
        $rootScope.$on('$routeChangeStart', function ($event, next, current) {
            try {
                var href = next.$$route.originalPath;
                $rootScope.FormIcon = null;
                $rootScope.ChangeHref(href);
            } catch (e) {

            }

        });

        $rootScope.$on('$viewContentLoaded', function () {
            //Here, our content is fully loaded !!
            if ($rootScope.ListMenuSearch.length > 0)
                if ('/' + $window.location.href.substr(window.location.href.lastIndexOf('/') + 1).toLowerCase() != $rootScope.CurrentHref.toLowerCase())
                    $rootScope.ChangeHref('/' + $window.location.href.substr(window.location.href.lastIndexOf('/') + 1));


            if ($("h3 .glyphicon").hasClass('glyphicon')) {
                $("h3 .glyphicon").removeAttr('class');
                $("h3").css("padding-left", "4px");
            }
        });

        setInterval(function () {
            if ($rootScope.ListMenuSearch.length > 0)
                if ('/' + $window.location.href.substr(window.location.href.lastIndexOf('/') + 1).toLowerCase() != $rootScope.CurrentHref.toLowerCase())
                    $rootScope.ChangeHref('/' + $window.location.href.substr(window.location.href.lastIndexOf('/') + 1));
            //if ($("h3 .glyphicon").hasClass('glyphicon')) {
            //    $("h3 .glyphicon").removeAttr('class');
            //    $("h3").css("padding-left", "4px");
            //}


        }, 200);
        $rootScope.SelectedHref = null;
        $rootScope.ChangeHref = function (href) {
            try {


                if (!$rootScope.ListMenuSearch || $rootScope.ListMenuSearch.length == 0) {
                    try {
                        $rootScope.SelectedHref = $cookies.get("upanelMenuHelpDocInternalName");
                        $rootScope.CurrentMenuMasterId = $cookies.get("MenuMasterId");
                    } catch (e) {

                    }

                }
                else {
                    $rootScope.FormTitle = $rootScope.title;
                    $rootScope.SelectedHref = null;
                    for (var i = 0; i < $rootScope.ListMenuSearch.length; i++) {
                        if ('/' + $rootScope.ListMenuSearch[i].Href == href) {
                            $rootScope.SelectedHref = $rootScope.ListMenuSearch[i].MenuHelpDocInternalName;
                            $rootScope.CurrentMenuMasterId = $rootScope.ListMenuSearch[i].Href;
                            $cookies.put("upanelMenuHelpDocInternalName", $rootScope.ListMenuSearch[i].MenuHelpDocInternalName);
                            $cookies.put("MenuMasterId", $rootScope.ListMenuSearch[i].Href);


                            $rootScope.CurrentHref = href;
                            $rootScope.FormTitle = $rootScope.ListMenuSearch[i].Remarks;

                            var svg = $($rootScope.ListMenuSearch[i].Image)[0];
                            svg.setAttribute('viewBox', '0 0 10 10');
                            svg.setAttribute('height', '24');
                            svg.setAttribute('width', '24');
                            svg.setAttribute('style', 'background-color:black;-webkit-filter: invert(100%);filter: invert(100%);');


                            $rootScope.FormIcon = $sce.trustAsHtml(svg.outerHTML);

                            break;
                        }
                    }
                }
            } catch (e) {

            }
        }

        $rootScope.DownloadDocumentationFile = function () {
            if (!$rootScope.SelectedHref)
                return;

            try {
                var file_src = 'OrderManagements/productionOrderReports/LoadPdfDocumentation?href=' + $rootScope.SelectedHref
                $rootScope.report(file_src);

            } catch (e) {

            }

        }



        /////////////////favorite menu///////////////////// 
        var BlankMenuIconForFavorite = '<svg xmlns="http://www.w3.org/2000/svg" xmlns:xlink="http://www.w3.org/1999/xlink" width="10pt" height="10pt" viewBox="0 0 10 10" version="1.1"><g id="surface1"><path style=" stroke:none;fill-rule:nonzero;fill:rgb(100%,100%,100%);fill-opacity:1;" d="M 0.9375 1.617188 L 0.9375 8.386719 C 0.9375 8.757812 1.242188 9.0625 1.617188 9.0625 L 8.386719 9.0625 C 8.757812 9.0625 9.0625 8.761719 9.0625 8.386719 L 9.0625 1.617188 C 9.0625 1.242188 8.761719 0.9375 8.386719 0.9375 L 1.617188 0.9375 C 1.242188 0.9375 0.9375 1.242188 0.9375 1.617188 Z M 2.679688 7.417969 C 2.425781 7.453125 2.210938 7.234375 2.242188 6.980469 C 2.265625 6.808594 2.40625 6.664062 2.578125 6.644531 C 2.835938 6.613281 3.050781 6.828125 3.015625 7.082031 C 2.996094 7.257812 2.855469 7.398438 2.679688 7.417969 Z M 2.679688 5.386719 C 2.425781 5.421875 2.210938 5.203125 2.242188 4.949219 C 2.265625 4.773438 2.40625 4.632812 2.578125 4.613281 C 2.835938 4.582031 3.050781 4.796875 3.015625 5.050781 C 2.996094 5.226562 2.855469 5.367188 2.679688 5.386719 Z M 2.679688 3.355469 C 2.425781 3.390625 2.210938 3.171875 2.242188 2.917969 C 2.265625 2.742188 2.40625 2.601562 2.578125 2.582031 C 2.835938 2.550781 3.050781 2.765625 3.015625 3.019531 C 2.996094 3.195312 2.855469 3.335938 2.679688 3.355469 Z M 7.515625 7.304688 L 4 7.304688 C 3.847656 7.304688 3.726562 7.179688 3.726562 7.03125 C 3.726562 6.882812 3.847656 6.757812 4 6.757812 L 7.515625 6.757812 C 7.664062 6.757812 7.789062 6.882812 7.789062 7.03125 C 7.789062 7.179688 7.664062 7.304688 7.515625 7.304688 Z M 7.515625 5.273438 L 4 5.273438 C 3.847656 5.273438 3.726562 5.148438 3.726562 5 C 3.726562 4.851562 3.847656 4.726562 4 4.726562 L 7.515625 4.726562 C 7.664062 4.726562 7.789062 4.851562 7.789062 5 C 7.789062 5.148438 7.664062 5.273438 7.515625 5.273438 Z M 7.515625 3.242188 L 4 3.242188 C 3.847656 3.242188 3.726562 3.117188 3.726562 2.96875 C 3.726562 2.820312 3.847656 2.695312 4 2.695312 L 7.515625 2.695312 C 7.664062 2.695312 7.789062 2.820312 7.789062 2.96875 C 7.789062 3.117188 7.664062 3.242188 7.515625 3.242188 Z M 7.515625 3.242188 "/></g></svg>';

        $rootScope.ShowFavouriteMenu = false;

        $rootScope.ShowHideFavouriteMenu = function () {
            if ($rootScope.ShowFavouriteMenu == true)
                $rootScope.ShowFavouriteMenu = false;
            else
                $rootScope.ShowFavouriteMenu = true;

            $http({
                method: 'GET',
                url: 'Securities/User/SaveShowHideFavouriteMenu?ShowFavouriteMenu=' + $rootScope.ShowFavouriteMenu
            }).then(function successCallback(response) {
                if ($rootScope.ShowFavouriteMenu)
                    $rootScope.GetFavouriteMenu();
            });

        }

        $rootScope.GetShowHideFavouriteMenu = function () {
            if ($rootScope.plantName) {
                $http({
                    method: 'GET',
                    url: 'Securities/User/ShowHideFavouriteMenu'
                }).then(function successCallback(response) {
                    if (response.data)
                        $rootScope.ShowFavouriteMenu = response.data.ShowFavoriteMenu;

                    if ($rootScope.ShowFavouriteMenu)
                        $rootScope.GetFavouriteMenu();
                });
            }
        }
        $rootScope.FavouriteModuleData = [];

        $rootScope.SaveFavouriteMenu = function () {
            $http({
                method: 'GET',
                url: 'Securities/User/SaveFavorite?MenuMasterId=' + $rootScope.CurrentMenuMasterId
            }).then(function successCallback(response) {
                $rootScope.GetFavouriteMenu();
            });
        }
        $rootScope.DeleteFavouriteMenu = function (href) {
            $http({
                method: 'GET',
                url: 'Securities/User/DeleteFavorite?MenuMasterId=' + href
            }).then(function successCallback(response) {
                $rootScope.GetFavouriteMenu();
            });
        }
        $rootScope.GetFavouriteMenu = function () {
            $http({
                method: 'GET',
                url: 'Securities/User/UserFavoriteMenu'
            }).then(function successCallback(response) {
                for (var i = 0; i < response.data.length; i++) {
                    for (var k = 0; k < response.data[i].MenuList.length; k++) {
                        try {
                            if (response.data[i].MenuList[k].Image)
                                response.data[i].MenuList[k].Image = $sce.trustAsHtml(response.data[i].MenuList[k].Image);
                            else {
                                response.data[i].MenuList[k].Image = $sce.trustAsHtml(BlankMenuIconForFavorite);
                            }

                        } catch (e) {

                        }

                    }
                }
                $rootScope.FavouriteModuleData = response.data;
            });
        }
        $rootScope.GetShowHideFavouriteMenu();



        $rootScope.isCompanyImageFound = function () {
            var img = new Image();
            var imgUrl = "POPResources/Organizations/" + $cookies.get("CompanyImage");
            img.src = imgUrl;
            img.onload = function () {
                $rootScope.CompanyLogo = $cookies.get("CompanyImage");
                $rootScope.CompanyFullName = null;
            }
            img.onerror = function () {
                $rootScope.CompanyFullName = $cookies.get("CompanyFullName");
                $rootScope.CompanyLogo = null;

            }
        }
        $rootScope.isCompanyImageFound();


        $rootScope.ResetMenu = function () {
            $rootScope.menuModuleName = '';
            $rootScope.showMenu = 'Module';
            $rootScope.menuFrames = [];
        }



        $rootScope.mpanelMenu = function (id, name) {
            $rootScope.showMenu = "Menu";
            $rootScope.menuModuleId = id;
            $rootScope.menuModuleName = name;
            $rootScope.menuFrames = $filter("filter")($rootScope.menuFrameList, { ModuleId: id }, true);
            setTimeout(function () {
                $rootScope.$apply(function () {
                    angular.element(".main-nav").vmenuModule({
                        Speed: 400,
                        autostart: false,
                        autohide: true
                    });
                });
            }, 100);
        };
        angular.isUndefinedOrNull = function (val) {
            return angular.isUndefined(val) || val === null || val === "";
        };

        
        $rootScope.template =
            '<div class="row" style="display:inline-box;">'
            + '    <div style="float:left;padding-left:10px;" class="glyphicon glyphicon-list"> '
            + '    </div>                                                                              '
            + '    <div style="float:left;padding-left:10px;">                                          '
            + '        ${Item}                                                                        '
            + '        </div>                                                                          '
            + '</div>                                                                                  ';
        $rootScope.tocode = function (args) {
            location.href = $rootScope.bootPoint + args.item.Href;
            $("#AutoCompleteMenuSearch").ejAutocomplete("clearText");
        }
        //$rootScope.alertHello = function (args) {
        //    console.log('Hello', args.item.Href)
        //}
        $rootScope.InsertMenuAccessLog = function (data) {
            $http({
                method: 'Get',
                url: 'menus/MenuMaster/PostMenuAccessLog?href=' + data.Href + '&menuItemName=' + data.MenuItemName + '&panel=' + 'Application',
            }).then(function successCallback(response) {
            });
        }
        $rootScope.report = function (file_src) {
            $("#iframe_div_for_report").empty();
            var frame = $('<iframe id="report">')
                .attr('height', '0px')
                .attr('visibility', 'hidden')
                .attr('width', '0px');
            frame.on('load', function () {

                try {
                    var text = angular.fromJson($('#report')[0].contentDocument.body.innerText);

                    if (text.hasOwnProperty('Message')) {
                        if (angular.isUndefinedOrNull(text.Message) === false) {
                            $('<div id="message">').attr('height', '0px')
                                .attr('visibility', 'hidden')
                                .attr('width', '0px').appendTo('#iframe_div_for_report');
                            $("#message").ejDialog({
                                title: "Error"
                            });
                            $("#message").ejDialog("setContent", text.Message);

                        }
                    }
                    else {
                        var text1 = $('#report')[0].contentDocument.body.innerText;

                        $('<div id="message">').attr('height', '0px')
                            .attr('visibility', 'hidden')
                            .attr('width', '0px').appendTo('#iframe_div_for_report');
                        $("#message").ejDialog({
                            title: "Error"
                        });
                        $("#message").ejDialog("setContent", text1);
                    }

                } catch (e) {


                }

            });


            frame.attr('src', file_src);
            frame.appendTo('#iframe_div_for_report');
        };
    }])
    .filter("unique", unique)
    .filter("dateFilter", dateFilter)
    .filter("dateFiltering", dateFiltering)
    .filter("trustUrl", trustUrl)
    .filter("safecontent", safecontent)
    .filter("sumByKey", sumByKey)
    .filter("setDecimal", setDecimal)
    .filter("groupBy", groupBy)
    .filter("makePositive", makePositive)
    .filter("searchFilter", searchFilter)
    .directive("panelBody", panelBody)
    .directive("panelMenu", panelMenu)
    .directive("nDecimals", nDecimals)
    .directive("datepicker", datepicker)
    .directive("monthpicker", monthpicker)
    .directive("togglable", togglable)
    .directive("showErrors", showErrors)
    .directive("compile", compile)
    .directive("archiveRow", archiveRow)
    .directive("confirmModal", confirmModal)
    .directive("confirmArchive", confirmArchive)
    .directive("confirmArchiveGeneric", confirmArchiveGeneric)
    .directive("loader", loader)
    .directive("tooltip", tooltip)
    .directive("input", inputFocus)
    .directive("textarea", inputFocus)
    .directive("select", inputFocus)
    .directive("input", CodeChecker)
    .directive("ngEnter", ngEnter)
    .directive("stringToNumber", stringToNumber)
    .directive("inputMaxLengthNumber", inputMaxLengthNumber)
    .directive("confirmCancel", confirmCancel)
    .directive("onlyNumbers", onlyNumbers)
    .directive("ngFileSelect", ngFileSelect)
    .directive("modalTable", modalTable)
    .directive("manualValidation", manualValidation)
    .directive("popover", popover)
    .directive("capitalize", capitalize)
    .directive("expand", expand)
    .directive("childExpand", childExpand)
    .factory("errorInterceptor", errorInterceptor)
    .factory("baseService", baseService)
    .factory("cboService", cboService)
    .factory("factoryService", factoryService)
    .factory("fileReader", fileReader)
    .factory("bankService", bankService)
    .factory("accountService", accountService)
    .factory("addressService", addressService)
    .factory("salesManagementService", salesManagementService)
    .factory('signalR', signalR)
    .factory("SignalRInit", SignalRInit)
    .constant("commonMessage", commonMessage)

    ;

