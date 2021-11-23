"use strict";
var upanelApp = angular
    .module("upanelApp", ["ngRoute", "ngCookies", "angularUtils.directives.dirPagination", "toaster", "angucomplete-alt", "angularjs-dropdown-multiselect", "ejangular"])
    .controller("ProductLibraryController", ProductLibraryController)
    .controller("ManualOTUploadController", ManualOTUploadController)
    .controller("OTManualController", OTManualController)
    .controller("PackingConfirmationController", PackingConfirmationController)
    .controller("PackingContentController", PackingContentController)
    .controller("MachineMapController", MachineMapController)
    .controller("SkillMapController", SkillMapController)
    .controller("specialUnlockController", specialUnlockController)
    .controller("SalaryIntegrationWithThirdPartyController", SalaryIntegrationWithThirdPartyController)
    .controller("accessControllerEmployeeTagController", accessControllerEmployeeTagController)
    .controller("AdminAttendanceControlController", AdminAttendanceControlController)
    .controller("accountDashboardController", accountDashboardController)
    .controller("advanceJournalController", advanceJournalController)
    .controller("NewAttendanceProcessController", NewAttendanceProcessController)
    .controller("NewAttendanceProcessAuditReportController", NewAttendanceProcessAuditReportController)
    .controller("NewAttendanceProcessPlantWiseController", NewAttendanceProcessPlantWiseController)
    .controller("advanceJournalOpeningBalanceController", advanceJournalOpeningBalanceController)
    .controller("allowanceDailyController", allowanceDailyController)
    .controller("annualBudgetController", annualBudgetController)
    .controller("approvalConfigurationController", approvalConfigurationController)
    .controller("assetInventoryIssueController", assetInventoryIssueController)
    .controller("assetItemArticleController", assetItemArticleController)
    .controller("assetItemController", AssetItemController)
    .controller("CurrencyExchangeController", CurrencyExchangeController)
    .controller("BOMReportsController", BOMReportsController)
    .controller("BalanceOTReportController", BalanceOTReportController)
    .controller("attendanceManagementController", attendanceManagementController)
    .controller("attendanceProcessDataController", attendanceProcessDataController)
    .controller("attendanceProcessDataNewController", attendanceProcessDataNewController)
    .controller("attendanceProcessDataEntityWiseController", attendanceProcessDataEntityWiseController)
    .controller("attendanceProcessDataEntityWiseNewController", attendanceProcessDataEntityWiseNewController)
    .controller("attendanceReportController", attendanceReportController)
    .controller("attendanceSlipController", attendanceSlipController)
    .controller("balanceSheetDetailsReportController", BalanceSheetDetailsReportController)
    .controller("balanceSheetOpeningBalanceReportController", BalanceSheetOpeningBalanceReportController)
    .controller("balanceSheetReportController", balanceSheetReportController)
    .controller("bankBaseController", bankBaseController)
    .controller("bankBookReportController", bankBookReportController)
    .controller("bankJournalController", bankJournalController)
    .controller("bankLedgerReportController", bankLedgerReportController)
    .controller("bankOpeningBalanceController", bankOpeningBalanceController)
    .controller("bankOpeningBalanceLedgerController", bankOpeningBalanceLedgerController)
    .controller("bankReconcileReportController", bankReconcileReportController)
    .controller("bankReconciliationController", bankReconciliationController)
    .controller("baseAttributeAndCharacteristicsValueController", baseAttributeAndCharacteristicsValueController)
    .controller("baseInvoiceController", baseInvoiceController)
    .controller("baseInvoiceWriteOffController", baseInvoiceWriteOffController)
    .controller("baseMaterialAndArticleController", baseMaterialAndArticleController)
    .controller("baseOpeningBalanceController", baseOpeningBalanceController)
    .controller("BonusPolicyMonthlyRetainEligibleEmployeeController", BonusPolicyMonthlyRetainEligibleEmployeeController)
    .controller("budgetCodeChangeController", budgetCodeChangeController)
    .controller("budgetMasterController", budgetMasterController)
    .controller("budgetMasterFARegisterController", budgetMasterFARegisterController)
    .controller("bulletinController", BulletinController)
    .controller("bulletinTemplateController", bulletinTemplateController)
    .controller("candidateAdministrationController", candidateAdministrationController)
    .controller("candidatedocumentAddRemoveController", candidatedocumentAddRemoveController)
    .controller("candidateDocumentAssignmentController", candidateDocumentAssignmentController)
    .controller("capitalizedFixedAssetRegisterController", capitalizedFixedAssetRegisterController)
    .controller("cashBaseController", cashBaseController)
    .controller("cashBookReportController", cashBookReportController)
    .controller("cashJournalController", cashJournalController)
    .controller("cashLedgerReportController", cashLedgerReportController)
    .controller("cashOpeningBalanceController", cashOpeningBalanceController)
    .controller("cashOpeningBalanceLedgerController", cashOpeningBalanceLedgerController)
    .controller("characteristicsValueController", CharacteristicsValueController)
    .controller("characteristicsWisePropertiesController", CharacteristicsWisePropertiesController)
    .controller("checkLotController", checkLotController)
    .controller("commitmentController", commitmentController)
    .controller("companyDepartmentController", CompanyDepartmentController)
    .controller("companyDesignationController", CompanyDesignationController)
    .controller("companyDivisionController", CompanyDivisionController)
    .controller("companyLineController", CompanyLineController)
    .controller("companySectionController", CompanySectionController)
    .controller("companySubDivisionController", CompanySubDivisionController)
    .controller("companySubSectionController", CompanySubSectionController)
    .controller("companyTaxContributionController", companyTaxContributionController)
    .controller("compliancejobCardReportController", compliancejobCardReportController)
    .controller("complianceShiftRotationController", complianceShiftRotationController)
    .controller("compliedShiftAssignmentController", compliedShiftAssignmentController)
    .controller("compliedshiftController", CompliedShiftController)
    .controller("compliedShiftGroupingController", CompliedShiftGroupingController)
    .controller("creditNoteController", creditNoteController)
    .controller("creditNoteSetOffController", creditNoteSetOffController)
    .controller("currencyBaseController", currencyBaseController)
    .controller("customerAdvanceController", customerAdvanceController)
    .controller("customerAdvanceOpeningBalanceController", customerAdvanceOpeningBalanceController)
    .controller("customerAdvanceWriteOffController", customerAdvanceWriteOffController)
    .controller("customerInterPlantCompanyReceiptController", CustomerInterPlantCompanyReceiptController)
    .controller("customerInterTransactionPendingController", customerInterTransactionPendingController)
    .controller("customerInvoiceController", customerInvoiceController)
    .controller("customerInvoiceOpeningBalanceController", customerInvoiceOpeningBalanceController)
    .controller("customerInvoiceReceiptController", customerInvoiceReceiptController)
    .controller("customerInvoiceSettlementController", customerInvoiceSettlementController)
    .controller("customerInvoiceWriteOffController", customerInvoiceWriteOffController)
    .controller("customerPaymentController", customerPaymentController)
    .controller("customerSuspenseController", customerSuspenseController)
    .controller("customerSuspenseWriteOffController", customerSuspenseWriteOffController)
    .controller("dailyAttendanceStatusReportController", dailyAttendanceStatusReportController)
    .controller("dailyComplianceReportController", dailyComplianceReportController)
    .controller("dashBoardController", dashBoardController)
    .controller("debitNoteController", debitNoteController)
    .controller("debitNoteSetOffController", debitNoteSetOffController)
    .controller("departmentController", DepartmentController)
    .controller("designationMasterController", DesignationMasterController)
    .controller("destinationController", DestinationController)
    .controller("divisionController", DivisionController)
    .controller("documentDashboardController", documentDashboardController)
    .controller("documentExcelReportController", documentExcelReportController)
    .controller("dynamicSalaryTopSheetController", dynamicSalaryTopSheetController)
    .controller("employeeAdvanceController", employeeAdvanceController)
    .controller("employeeAdvanceOpeningBalanceController", employeeAdvanceOpeningBalanceController)
    .controller("employeeAdvanceRequisitionController", employeeAdvanceRequisitionController)
    .controller("employeeAdvanceRequisitionPostController", employeeAdvanceRequisitionPostController)
    .controller("employeeAdvanceWriteOffController", employeeAdvanceWriteOffController)
    .controller("EmployeeAndPlantWiseAttendanceUnLockController", EmployeeAndPlantWiseAttendanceUnLockController)
    .controller("employeeAttendanceGroupController", employeeAttendanceGroupController)
    .controller("employeeBankInformationController", employeeBankInformationController)
    .controller("employeeBaseController", employeeBaseController)
    .controller("employeeBaseMultipleController", employeeBaseMultipleController)
    .controller("employeeDeviceController", employeeDeviceController)
    .controller("employeedocumentAddRemoveController", employeedocumentAddRemoveController)
    .controller("employeeDocumentAssignmentController", employeeDocumentAssignmentController)
    .controller("employeeExpenseBookingReportController", employeeExpenseBookingReportController)
    .controller("employeeIdCardController", employeeIdCardController)
    .controller("employeeInformationController", employeeInformationController)
    .controller("employeeLeaveApplicationController", employeeLeaveApplicationController)
    .controller("employeeLeaveBalanceController", employeeLeaveBalanceController)
    .controller("employeeLeaveCarryForwardController", employeeLeaveCarryForwardController)
    .controller("employeeLedgerReportController", employeeLedgerReportController)
    .controller("EmployeeLockAndUnLockController", EmployeeLockAndUnLockController)
    .controller("employeePayableController", employeePayableController)
    .controller("employeePayableOpeningBalanceController", employeePayableOpeningBalanceController)
    .controller("employeePaymentController", employeePaymentController)
    .controller("employeeProbationalPeriodController", employeeProbationalPeriodController)
    .controller("EmployeeProfileApprovalController", EmployeeProfileApprovalController)
    .controller("EmployeeProfileUnApprovalController", EmployeeProfileUnApprovalController)
    .controller("employeePurchaseController", employeePurchaseController)
    .controller("employeeRegisterController", employeeRegisterController)
    .controller("employeeReportInfoController", employeeReportInfoController)
    .controller("employeeShiftAssignController", employeeShiftAssignController)
    .controller("entityExpenseBookingApprovalController", entityExpenseBookingApprovalController)
    .controller("entityExpenseBookingController", entityExpenseBookingController)
    .controller("entityOperationSettingsController", entityOperationSettingsController)
    .controller("equityController", equityController)
    .controller("ExceptionForHolidayController", ExceptionForHolidayController)
    .controller("exchangeVoucherController", exchangeVoucherController)
    .controller("expenseBookingApprovalController", expenseBookingApprovalController)
    .controller("expenseBookingApprovedController", expenseBookingApprovedController)
    .controller("expenseBookingApprovedListController", expenseBookingApprovedListController)
    .controller("expenseBookingController", expenseBookingController)
    .controller("expenseDashboardController", expenseDashboardController)
    .controller("ExtraOTController", ExtraOTController)
    .controller("ExtraOTDeleteController", ExtraOTDeleteController)
    .controller("fabricRollManagementSettingsController", fabricRollManagementSettingsController)
    .controller("fabricRollMasterController", fabricRollMasterController)
    .controller("fgcomponentController", FGComponentController)
    .controller("fgzoneController", FGZoneController)
    .controller("fiscalYearBaseController", fiscalYearBaseController)
    .controller("fixedAssetExpenseReportController", fixedAssetExpenseReportController)
    .controller("fixedAssetMasterOpeningBalanceController", fixedAssetMasterOpeningBalanceController)
    .controller("fixedAssetObReportController", FixedAssetObReportController)
    .controller("fixedAssetRegisterAUCJVController", fixedAssetRegisterAUCJVController)
    .controller("fixedAssetRegisterController", fixedAssetRegisterController)
    .controller("fixedAssetRegisterJVController", fixedAssetRegisterJVController)
    .controller("fixedAssetRegisterJVOBController", fixedAssetRegisterJVOBController)
    .controller("GateentryTokenController", GateentryTokenController)
    .controller("GatePassController", GatePassController)
    .controller("InOutGatePassController", InOutGatePassController)
    .controller("GatePassEmployeeController", GatePassEmployeeController)
    .controller("generalLedgerOpeningBalanceReportController", generalLedgerOpeningBalanceReportController)
    .controller("generalLedgerReportController", generalLedgerReportController)
    .controller("glMappingController", glMappingController)
    .controller("grnApprovalController", grnApprovalController)
    .controller("grnApprovedController", grnApprovedController)
    .controller("grnPaymentHoldController", grnPaymentHoldController)
    .controller("hrDashboardController", hrDashboardController)
    .controller("incomeStatementReportController", IncomeStatementReportController)
    .controller("individualComplianceReportController", individualComplianceReportController)
    .controller("inquiryController", inquiryController)
    .controller("inquiryMasterController", inquiryMasterController)
    .controller("interCompanyInvestmentTakenOpeningBalanceController", interCompanyInvestmentTakenOpeningBalanceController)
    .controller("interCompanyLoanTakenOpeningBalanceController", interCompanyLoanTakenOpeningBalanceController)
    .controller("interCompanyPartyController", InterCompanyPartyController)
    .controller("interCompanyTransactionTakenOpeningBalanceController", interCompanyTransactionTakenOpeningBalanceController)
    .controller("interInvestmentGivenOpeningBalanceController", interInvestmentGivenOpeningBalanceController)
    .controller("interLoanGivenOpeningBalanceController", interLoanGivenOpeningBalanceController)
    .controller("interLoanPendingController", interLoanPendingController)
    .controller("interPlantInvestmentTakenOpeningBalanceController", interPlantInvestmentTakenOpeningBalanceController)
    .controller("interPlantLoanTakenOpeningBalanceController", interPlantLoanTakenOpeningBalanceController)
    .controller("interPlantTransactionTakenOpeningBalanceController", interPlantTransactionTakenOpeningBalanceController)
    .controller("interTransactionController", interTransactionController)
    .controller("interTransactionGivenOpeningBalanceController", interTransactionGivenOpeningBalanceController)
    .controller("intSalesOrderInvoiceController", intSalesOrderInvoiceController)
    .controller("intSalesOrderInvoiceEditController", intSalesOrderInvoiceEditController)
    .controller("intSalesOrderInvoicePostController", intSalesOrderInvoicePostController)
    .controller("InventoryCheckApprovedController", InventoryCheckApprovedController)
    .controller("inventoryIssueController", inventoryIssueController)
    .controller("inventoryIssueJournalController", inventoryIssueJournalController)
    .controller("inventoryPayableController", inventoryPayableController)
    .controller("inventoryReceiveController", inventoryReceiveController)
    .controller("inventoryRejectPayableController", inventoryRejectPayableController)
    .controller("inventoryReportController", inventoryReportController)
    .controller("inventoryShortagePayableController", inventoryShortagePayableController)
    .controller("investmentController", investmentController)
    .controller("investmentGivenOpeningBalanceController", investmentGivenOpeningBalanceController)
    .controller("investmentTakenOpeningBalanceController", investmentTakenOpeningBalanceController)
    .controller("invoiceChargeWriteOffController", invoiceChargeWriteOffController)
    .controller("invoiceController", InvoiceController)
    .controller("issueRegisterController", issueRegisterController)
    .controller("IssueReturnRegisterController", IssueReturnRegisterController)
    .controller("IssueSlipController", IssueSlipController)
    .controller("jobCardReportController", jobCardReportController)
    .controller("jobCardReportNewController", jobCardReportNewController)
    .controller("journalController", journalController)
    .controller("journalOpeningBalanceController", journalOpeningBalanceController)
    .controller("LayOffController", LayOffController)
    .controller("LCReportsController", LCReportsController)
    .controller("leaveEncashmentController", leaveEncashmentController)
    .controller("LeaveEncashmentEntryController", LeaveEncashmentEntryController)
    .controller("leaveInformationController", LeaveInformationController)
    .controller("lineController", LineController)
    .controller("LineDesignerController", LineDesignerController)
    .controller("lineEmployeeAssignController", lineEmployeeAssignController)
    .controller("lineEmployeeAssignEditController", lineEmployeeAssignEditController)
    .controller("lineProductionBookingController", lineProductionBookingController)
    .controller("lineProductionExcelController", lineProductionExcelController)
    .controller("loanAdvanceMasterController", loanAdvanceMasterController)
    .controller("loanController", loanController)
    .controller("loanGivenOpeningBalanceController", loanGivenOpeningBalanceController)
    .controller("loanLedgerReportController", loanLedgerReportController)
    .controller("loanPaymentController", loanPaymentController)
    .controller("loanCloseController", loanCloseController)
    .controller("loanTakenController", loanTakenController)
    .controller("loanTakenOpeningBalanceController", loanTakenOpeningBalanceController)
    .controller("lsdController", LSDController)
    .controller("machineAttributeController", machineAttributeController)
    .controller("machineController", machineController)
    .controller("machineMasterUIController", machineMasterUIController)
    .controller("mainProcessPlanningController", MainProcessPlanningController)
    .controller("manpowerAttendanceGroupSummaryController", manpowerAttendanceGroupSummaryController)
    .controller("manpowerBudgetDashboardController", manpowerBudgetDashboardController)
    .controller("manualOutTimeController", manualOutTimeController)
    .controller("masterOrderController", masterOrderController)
    .controller("masterOrderSalesController", masterOrderSalesController)
    .controller("masterOrderSalesPostController", masterOrderSalesPostController)
    .controller("materialAttributeMasterController", MaterialAttributeMasterController)
    .controller("materialAttributeValueController", MaterialAttributeValueController)
    .controller("MaterialBudgetController", MaterialBudgetController)
    .controller("materialGroupMasterController", MaterialGroupMasterController)
    .controller("MaterialIssueSlipController", MaterialIssueSlipController)
    .controller("materialledgerController", materialledgerController)
    .controller("purchaseorderRegisterController", purchaseorderRegisterController)
    .controller("materialMasterArticleController", materialMasterArticleController)
    .controller("materialMasterController", MaterialMasterController)
    .controller("materialMasterOpeningBalanceController", materialMasterOpeningBalanceController)
    .controller("materialMasterReportController", MaterialMasterReportController)
    .controller("materialStockController", materialStockController)
    .controller("MaternityLeaveTransactionController", MaternityLeaveTransactionController)
    .controller("misAccountDashboardController", misAccountDashboardController)

    //.controller("mpanelDashboardController", mpanelDashboardController)
    //.controller("mpanelLoginController", mpanelLoginController)
    //.controller("mpanelLogoutController", mpanelLogoutController)
    .controller("multipleResignationApprovalController", multipleResignationApprovalController)
    .controller("multipleVendorPaymentApprovedController", multipleVendorPaymentApprovedController)
    .controller("multipleVendorPaymentController", multipleVendorPaymentController)
    .controller("nonAssetRegisterController", nonAssetRegisterController)
    .controller("nonFinancialMaterialOpeningBalancePostController", nonFinancialMaterialOpeningBalancePostController)
    .controller("normalJournalController", normalJournalController)
    .controller("oDDeleteController", oDDeleteController)
    .controller("oDDeleteNewController", oDDeleteNewController)
    .controller("onDutyApprovalController", onDutyApprovalController)
    .controller("onDutyApprovalNewController", onDutyApprovalNewController)
    .controller("onDutyTransactionController", onDutyTransactionController)
    .controller("openingBalanceReportController", openingBalanceReportController)
    .controller("operationController", OperationController)
    .controller("OperationMasterController", OperationMasterController)
    .controller("operationMotionController", operationMotionController)
    .controller("operationVariationController", operationVariationController)
    .controller("operationVideoUploadController", OperationVideoUploadController)
    .controller("OrderCostingApprovalController", OrderCostingApprovalController)
    .controller("OrderCostingUnApprovalController", OrderCostingUnApprovalController)
    .controller('costingCategoryController', costingCategoryController)
    .controller('costingSubCategoryController', costingSubCategoryController)
    .controller("OTAdjustmentController", OTAdjustmentController)
    .controller("otFinalController", otFinalController)
    .controller("otFinalInformationController", otFinalInformationController)
    .controller("OTManagementController", OTManagementController)
    .controller("ourStyleController", OurStyleController)
    .controller("packingListMasterController", packingListMasterController)
    .controller("paidHoursEmployeeAssignController", paidHoursEmployeeAssignController)
    .controller("partyBaseController", partyBaseController)
    .controller("partyController", PartyController)
    .controller("buyerController", BuyerController)
    .controller("buyerDivisionController", BuyerDivisionController)
    .controller("buyerDepartmentController", BuyerDepartmentController)
    .controller("buyerBrandController", buyerBrandController)
    .controller("buyerProgramController", buyerProgramController)
    .controller("partyLedgerOutstandingReportController", partyLedgerOutstandingReportController)
    .controller("partyLedgerReportController", partyLedgerReportController)
    .controller("partyOpeningBalanceLedgerController", partyOpeningBalanceLedgerController)
    .controller("partyOutstandingReportController", partyOutstandingReportController)
    .controller("partyReconciliationController", partyReconciliationController)
    .controller("partyReportController", partyReportController)
    .controller("paymentByBankController", paymentByBankController)
    .controller("paymentByCashController", paymentByCashController)
    .controller("paymentTermController", PaymentTermController)
    .controller("payRegisterBDReportWithStructureController", payRegisterBDReportWithStructureController)
    .controller("PhysicalStockAdjustmentMasterController", PhysicalStockAdjustmentMasterController)
    .controller("plantCalendarController", PlantCalendarController)
    .controller("plantSelectionController", plantSelectionController)
    .controller("PlantWiseAttendanceLockController", PlantWiseAttendanceLockController)
    .controller("PlantWiseAttendanceUnLockController", PlantWiseAttendanceUnLockController)
    .controller("plantWiseLetterTemplateController", plantWiseLetterTemplateController)
    .controller("plantWiseTermsAndConditionsController", plantWiseTermsAndConditionsController)
    .controller("portController", PortController)
    .controller("postRecruitmentDocumentByDepartmentController", postRecruitmentDocumentByDepartmentController)
    .controller("preCostingController", PreCostingController)
    .controller("preRecruitmentDocumentApprovalController", preRecruitmentDocumentApprovalController)
    .controller("preRecruitmentDocumentByDepartmentController", preRecruitmentDocumentByDepartmentController)
    .controller("printCashCheckController", printCashCheckController)
    .controller("printNonCashCheckController", printNonCashCheckController)
    .controller("processSetReportController", ProcessSetReportController)
    .controller("ProcurementController", ProcurementController)
    .controller("productCategoryController", ProductCategoryController)
    .controller("productController", ProductController)
    .controller("productDefinitionController", productDefinitionController)
    .controller("productionCalendarController", ProductionCalendarController)
    .controller("productionOrderController", ProductionOrderController)
    .controller("productionOrderReportsController", productionOrderReportsController)
    .controller("productionOrderSchedulingParametersType1Controller", ProductionOrderSchedulingParametersType1Controller)
    .controller("productionOrderSubprocessController", ProductionOrderSubprocessController)
    .controller("productionResourcesController", productionResourcesController)
    .controller("productionStatusController", ProductionStatusController)
    .controller("ProductionSummaryController", ProductionSummaryController)
    .controller("ProductionSummaryInOutController", ProductionSummaryInOutController)
    .controller("productionSystemController", productionSystemController)
    .controller("productMasterController", ProductMasterController)
    .controller("productSubCategoryAttributeController", ProductSubCategoryAttributeController)
    .controller("productSubCategoryController", ProductSubCategoryController)
    .controller("ProfileFromExcelController", ProfileFromExcelController)
    .controller("projectPlanningController", ProjectPlanningController)
    .controller("projectPlanningPurchaseOrderController", ProjectPlanningPurchaseOrderController)
    .controller("projectPlanningRequisitionController", ProjectPlanningRequisitionController)
    .controller("PurchaseOrderByRequisitionController", PurchaseOrderByRequisitionController)
    .controller("purchaseOrderGroupController", purchaseOrderGroupController)
    .controller("PurchaseReturnController", PurchaseReturnController)
    .controller("PurchaseReturnRegisterController", PurchaseReturnRegisterController)
    .controller("QRCodeGenerationEmployeeController", QRCodeGenerationController)
    .controller("QRCodeGenerationOperationController", QRCodeGenerationController)
    .controller("rawDataSetInOutController", rawDataSetInOutController)
    .controller("receiptByBankController", receiptByBankController)
    .controller("receiptByCashController", receiptByCashController)
    .controller("recipeGlobalMasterController", recipeGlobalMasterController)
    .controller("recipeMaterialController", recipeMaterialController)
    .controller("recipeMaterialGroupingMasterController", recipeMaterialGroupingMasterController)
    .controller("recruitmentAppDataEditController", recruitmentAppDataEditController)
    .controller("recruitmentApprovalController", recruitmentApprovalController)
    .controller("recruitmentController", recruitmentController)
    .controller("recruitmentPlanningController", recruitmentPlanningController)
    .controller("recruitmentSelectionController", recruitmentSelectionController)
    .controller("RequisitionController", RequisitionController)
    .controller("resignationApprovalController", resignationApprovalController)
    .controller("resignationController", resignationController)
    .controller("resignationRecruitmentPlanningController", resignationRecruitmentPlanningController)
    .controller("restController", restController)
    .controller("routeController", routeController)
    .controller("routeEmployeeController", routeEmployeeController)
    .controller("runningOrderParametersController", runningOrderParametersController)
    .controller("salaryAdvanceApprovalController", salaryAdvanceApprovalController)
    .controller("salaryAdvanceOpeningBalanceController", salaryAdvanceOpeningBalanceController)
    .controller("salaryFixationController", salaryFixationController)
    .controller("salaryLockController", salaryLockController)
    .controller("salaryPaymentStatementsBankCSVController", salaryPaymentStatementsBankCSVController)
    .controller("SalaryProcessController", SalaryProcessController)
    .controller("SalaryProcessNewController", SalaryProcessNewController)
    .controller("salaryProcessDeleteController", salaryProcessDeleteController)
    .controller("salaryReportController", salaryReportController)
    .controller("SalaryStructureApprovalController", SalaryStructureApprovalController)
    .controller("SalaryStructureUnApprovalController", SalaryStructureUnApprovalController)
    .controller("salaryTopSheetController", salaryTopSheetController)
    .controller("salesController", salesController)
    .controller("salesInvoiceController", salesInvoiceController)
    .controller("salesInvoicePendingController", salesInvoicePendingController)
    .controller("salesOrderInvoiceController", SalesOrderInvoiceController)
    .controller("salesOrderPackingListController", SalesOrderPackingListController)
    .controller("salesOrderPendingController", SalesOrderPendingController)
    .controller("sampleOrderController", SampleOrderController)
    .controller("sampleOrderPendingController", SampleOrderPendingController)
    .controller("samplePackingListController", SamplePackingListController)
    .controller("sampleRequisitionController", SampleRequisitionController)
    .controller("sectionController", SectionController)
    .controller("securityDepositController", securityDepositController)
    .controller("securityDepositGivenOpeningBalanceController", securityDepositGivenOpeningBalanceController)
    .controller("securityDepositTakenOpeningBalanceController", securityDepositTakenOpeningBalanceController)
    .controller("securityDepositWriteOffController", securityDepositWriteOffController)
    .controller("separationtypeController", separationtypeController)
    .controller("ServicePoAcknowledgementController", ServicePoAcknowledgementController)
    .controller("ServicePOByRequisitionController", ServicePOByRequisitionController)
    .controller("ServiceRequisitionCheckApprovedController", ServiceRequisitionCheckApprovedController)
    .controller("ServiceRequisitionController", ServiceRequisitionController)
    .controller("shiftAssignmentController", shiftAssignmentController)
    .controller("shiftTimeChangeController", shiftTimeChangeController)
    .controller("shipModeController", ShipModeController)
    .controller("skillController", SkillController)
    .controller("SpecificDateLeaveEncashmentController", SpecificDateLeaveEncashmentController)
    .controller("stitchCodeController", stitchCodeController)
    .controller("stoppageController", stoppageController)
    .controller("subDivisionController", SubDivisionController)
    .controller("subSectionController", SubSectionController)
    .controller("subsectionStructureController", SubsectionStructureController)
    .controller("taskDetailController", taskDetailController)
    .controller("taxCodeController", TaxCodeController)
    .controller("taxPayableReportController", taxPayableReportController)
    .controller("taxPaymentController", taxPaymentController)
    .controller("testingController", TestingController)
    .controller("testingStandardController", TestingStandardController)
    .controller("testingStandardReportController", TestingStandardReportController)
    .controller("thirdPartyOperationController", ThirdPartyOperationController)
    .controller("timeCaptureController", TimeCaptureController)
    .controller("TNAReportsController", TNAReportsController)
    .controller("TNAStatusReportsController", TNAStatusReportsController)
    .controller("trialBalanceReportController", trialBalanceReportController)
    .controller("TrimInTimeController", TrimInTimeController)
    .controller("unitController", UnitController)
    .controller("upanelDashboardController", upanelDashboardController)
    .controller("upanelLoginController", upanelLoginController)
    .controller("upanelLogoutController", upanelLogoutController)
    .controller("userPasswordChangeController", UserPasswordChangeController)
    .controller("vendorAdvanceController", vendorAdvanceController)
    .controller("vendorAdvanceOpeningBalanceController", vendorAdvanceOpeningBalanceController)
    .controller("vendorAdvanceWriteOffController", vendorAdvanceWriteOffController)
    .controller("vendorInvoiceController", vendorInvoiceController)
    .controller("vendorInvoiceOpeningBalanceController", vendorInvoiceOpeningBalanceController)
    .controller("vendorPaymentController", vendorPaymentController)
    .controller("weeklyAbsentismAssignmentController", weeklyAbsentismAssignmentController)
    .controller("WeekOffChangeController", WeekOffChangeController)
    .controller("workCenterBuyerTagController", WorkCenterBuyerTagController)
    .controller("workCenterMasterController", WorkCenterMasterController)
    .controller("workStationDailyController", workStationDailyController)
    .controller('ActivityMasterController', ActivityMasterController)
    .controller('actualOTAndPlantController', actualOTAndPlantController)
    .controller('advanceAndTDSController', advanceAndTDSController)
    .controller('allowanceDailyController', allowanceDailyController)
    .controller('ApprovalController', ApprovalController)
    .controller('AttendanceDeviceZoneController', AttendanceDeviceZoneController)
    .controller('attendanceEntryController', attendanceEntryController)
    .controller('attendanceOnDayStatusController', attendanceOnDayStatusController)
    .controller('attendanceProcessDataManualStatusController', attendanceProcessDataManualStatusController)
    .controller('attendanceProcessDataManualStatusNewController', attendanceProcessDataManualStatusNewController)
    .controller('attendanceProcessUIController', attendanceProcessUIController)
    .controller('attendanceRawController', attendanceRawController)
    .controller('AttendanceRawDataDeleteController', AttendanceRawDataDeleteController)
    .controller('AttendanceRawDataDeleteNewController', AttendanceRawDataDeleteNewController)
    .controller('AttendanceRawDataUploadController', AttendanceRawDataUploadController)
    .controller('attendanceSummaryStatusController', attendanceSummaryStatusController)
    .controller('authorizationConfigController', authorizationConfigController)
    .controller('biometricDeviceAsAccessListController', biometricDeviceAsAccessListController)
    .controller('biometricDeviceAsShortLeaveController', biometricDeviceAsShortLeaveController)
    .controller('BOMMasterAttachmentController', BOMMasterAttachmentController)
    .controller('BOMMasterController', BOMMasterController)
    .controller('bonusRegisterController', bonusRegisterController)
    .controller('bonusRegisterReportController', bonusRegisterReportController)
    .controller('BonusRetainedDisbursementController', BonusRetainedDisbursementController)
    .controller('bonusSheetController', bonusSheetController)
    .controller('BulkIncrementController', BulkIncrementController)
    .controller('BulkLeaveEntryController', BulkLeaveEntryController)
    .controller('buyerMasterController', BuyerMasterController)
    .controller('cashReceiptPaymentReportController', cashReceiptPaymentReportController)
    .controller('CNFExpenseBockingController', CNFExpenseBockingController)
    .controller('CompensatoryOffController', CompensatoryOffController)
    .controller('CompensatoryOffNewController', CompensatoryOffNewController)
    .controller('complianceAttendanceSettingController', complianceAttendanceSettingController)
    .controller('complianceRawDataDownloadController', complianceRawDataDownloadController)
    .controller('ConfirmationController', ConfirmationController)
    .controller('contractController', contractController)
    .controller('costingGroupFormulaController', costingGroupFormulaController)
    .controller('costingItemController', costingItemController)
    .controller('CropMasterController', CropMasterController)

    .controller('customerInvoiceBanksReceiptController', customerInvoiceBanksReceiptController)
    .controller('DailyAllowanceConfirmationController', DailyAllowanceConfirmationController)
    .controller('dailyAllowanceController', dailyAllowanceController)
    .controller('DailyAllowanceRateEmpWiseController', DailyAllowanceRateEmpWiseController)
    .controller('DailyAllowanceSettingController', DailyAllowanceSettingController)
    .controller('dailyAllowanceTransactionController', dailyAllowanceTransactionController)
    .controller('dailyAttendanceSummaryController', dailyAttendanceSummaryController)
    .controller('dailyAttendanceSummaryNoLineController', dailyAttendanceSummaryNoLineController)
    .controller('dailyDayStatusController', dailyDayStatusController)
    .controller('dailyTransactionReportController', dailyTransactionReportController)
    .controller('DateRangeWiseAttendanceUnLockController', DateRangeWiseAttendanceUnLockController)
    .controller('DepartmentGroupController', DepartmentGroupController)
    .controller('DeviceRawDataDownloadController', DeviceRawDataDownloadController)
    .controller('disciplinaryActionCategoryController', disciplinaryActionCategoryController)
    .controller('disciplinaryActionController', disciplinaryActionController)
    .controller('disciplinaryActionCriticalityController', disciplinaryActionCriticalityController)
    .controller('EarnLeavePaySlipController', EarnLeavePaySlipController)
    .controller('empActiveInActiveController', empActiveInActiveController)
    .controller('empActiveInActiveNewController', empActiveInActiveNewController)
    .controller('EmployeeBankInfoInformationController', EmployeeBankInfoInformationController)
    .controller('employeeBankInformationController', employeeBankInformationController)
    .controller('employeeDeleteController', employeeDeleteController)
    .controller('employeeDisciplinaryActionController', employeeDisciplinaryActionController)
    .controller('employeeDisciplinaryActionTransactionController', employeeDisciplinaryActionTransactionController)
    .controller('employeedocumentAddRemoveController', employeedocumentAddRemoveController)
    .controller('EmployeeDOJChangeController', EmployeeDOJChangeController)
    .controller('EmployeeFixedServicTransactionController', EmployeeFixedServicTransactionController)
    .controller('employeeInFoReportController', employeeInFoReportController)
    .controller('employeeInformationNewController', employeeInformationNewController)
    .controller('EmployeeLeaveApprovalController', EmployeeLeaveApprovalController)
    .controller('employeeLeaveDeleteApplicationController', employeeLeaveDeleteApplicationController)
    .controller('EmployeeProfileUploadController', EmployeeProfileUploadController)
    .controller('EmployeePromotionAndIncrementController', EmployeePromotionAndIncrementController)
    .controller('employeePromotionController', employeePromotionController)
    .controller('employeePromotionNewController', employeePromotionNewController)
    .controller('employeeSalaryAdvanceLedgerController', employeeSalaryAdvanceLedgerController)
    .controller('employeeSalaryPayableController', employeeSalaryPayableController)
    .controller('employeeSalaryRuleEditableController', EmployeeSalaryRuleEditableController)
    .controller('EmployeeServiceBookingController', EmployeeServiceBookingController)
    .controller('EmployeeServicesRateController', EmployeeServicesRateController)
    .controller('employeeWiseFixedOTSettingController', employeeWiseFixedOTSettingController)
    .controller('EncashmentController', EncashmentController)
    .controller('entityTaskController', entityTaskController)
    .controller('esicStatementsController', esicStatementsController)
    .controller('esicSummaryController', esicSummaryController)
    .controller('exceptionEmployeeController', exceptionEmployeeController)
    .controller('expensesCapitalizedController', expensesCapitalizedController)
    .controller('FarmerMasterController', FarmerMasterController)
    .controller('FgPoFormasterOrderController', FgPoFormasterOrderController)
    .controller('finalSettlementController', finalSettlementController)

    .controller('finalSettlementReportController', finalSettlementReportController)
    .controller('finalSettlementVoucherController', finalSettlementVoucherController)
    .controller('fiscalYearBudgetController', fiscalYearBudgetController)
    .controller('fixedAssetAUCCapitalizeGRNBassController', fixedAssetAUCCapitalizeGRNBassController)
    .controller('fixedAssetDisposeController', fixedAssetDisposeController)
    .controller('fixedAssetDisposePostController', fixedAssetDisposePostController)
    .controller('FixedAssetsRegisterReportController', FixedAssetsRegisterReportController)
    .controller('FixedAssetsRegisterDisposedReportController', FixedAssetsRegisterDisposedReportController)
    .controller('goodsReceiveNoteController', goodsReceiveNoteController)
    .controller('gratuityReportController', gratuityReportController)
    .controller('gratuityPolicyController', gratuityPolicyController)
    .controller('GRNByPOController', GRNByPOController)
    .controller('holidayAbsentismAssignmentController', holidayAbsentismAssignmentController)

    .controller('hourlyOffDutyTagController', hourlyOffDutyTagController)
    .controller('hourlyOTController', hourlyOTController)
    .controller('ICSMasterController', ICSMasterController)
    .controller('incrementGroupController', incrementGroupController)
    .controller('independentOrderController', independentOrderController)
    .controller('IndividualAttendanceLockController', IndividualAttendanceLockController)
    .controller('IndividualAttendanceUnLockController', IndividualAttendanceUnLockController)
    .controller('individualFixedOTController', individualFixedOTController)
    .controller('interpartyLedgerReportController', interpartyLedgerReportController)
    .controller('InventoryDashboardController', InventoryDashboardController)
    .controller('inventoryIssueDeleteController', inventoryIssueDeleteController)
    .controller('inventoryReceivableController', inventoryReceivableController)
    .controller('inventorySalesRegisterController', inventorySalesRegisterController)
    .controller('inventorySalesController', inventorySalesController)

    .controller('inventoryScrapController', inventoryScrapController)
    .controller('InventoryStatusDashboardController', InventoryStatusDashboardController)
    .controller('InventoryDashboardStatusController', InventoryDashboardStatusController)
    .controller('invoiceOverheadController', invoiceOverheadController)
    .controller('invoiceOverheadPostController', invoiceOverheadPostController)
    .controller('issueAUCCapitalizeController', issueAUCCapitalizeController)
    .controller('issueGroupController', issueGroupController)
    .controller('issueImportanceController', issueImportanceController)
    .controller('issueReportController', issueReportController)
    .controller('IssueReturnController', IssueReturnController)
    .controller('issueStandardController', issueStandardController)
    .controller('IssueStatusReportsController', IssueStatusReportsController)
    .controller('jobCardcomplianceReportController', jobCardcomplianceReportController)

    .controller('lateAttendancePostingController', lateAttendancePostingController)
    .controller('leaveOpeningBalanceController', LeaveOpeningBalanceController)

    .controller('leavesChecklistReportController', leavesChecklistReportController)

    .controller('leaveWithWagesRegistersController', leaveWithWagesRegistersController)
    .controller('leaveWithWagesRegistersForm18Controller', leaveWithWagesRegistersForm18Controller)

    .controller('LeaveYearEndProcessController', LeaveYearEndProcessController)
    .controller('loanInterestPayableController', loanInterestPayableController)
    .controller('loanInterestPayableReverseController', loanInterestPayableReverseController)
    .controller('longAbsenteeismAssignController', longAbsenteeismAssignController)
    .controller('manpowerAttendanceSummaryController', manpowerAttendanceSummaryController)
    .controller('manpowerAttendanceSummaryControllerNew', manpowerAttendanceSummaryControllerNew)
    .controller('manpowerBudgetController', manpowerBudgetController)
    .controller('ManualAttendanceConfirmationController', ManualAttendanceConfirmationController)
    .controller('masterLCController', masterLCController)
    .controller('materialAgeingDashboardController', materialAgeingDashboardController)
    .controller('materialMasterWithProductMasterController', materialMasterWithProductMasterController)
    .controller('MaterialTransferController', MaterialTransferController)
    .controller('maternityBenefitAfterController', maternityBenefitAfterController)
    .controller('maternityBenefitController', maternityBenefitController)
    .controller('maternityLeaveReportController', maternityLeaveReportController)
    .controller('monthlyAttendanceInformationController', monthlyAttendanceInformationController)
    .controller('MonthlyAttendanceInformationNewController', MonthlyAttendanceInformationNewController)
    .controller('multipleIdCardController', multipleIdCardController)
    .controller('MultipleLeaveEncashmentController', MultipleLeaveEncashmentController)
    .controller('nationalFestivalController', nationalFestivalController)
    .controller('offDutyApproveController', offDutyApproveController)
    .controller('offDutyHoursController', offDutyHoursController)
    .controller('openingBalanceReportController', openingBalanceReportController)
    .controller('OrderControlController', OrderControlController)
    .controller('OrderCostingController', OrderCostingController)
    .controller('OrderReportController', OrderReportController)
    .controller('OTLimitTransactionController', OTLimitTransactionController)
    .controller('OTLimitTransactionFromAppController', OTLimitTransactionFromAppController)
    .controller('otSlabController', otSlabController)
    .controller('ParollsReportController', ParollsReportController)
    .controller('partyGroupCategoryController', PartyGroupCategoryController)
    .controller('partyGroupClassController', PartyGroupClassController)
    .controller('partyGroupController', PartyGroupController)
    .controller('partyGroupSubCategoryController', PartyGroupSubCategoryController)
    .controller('partyMappingController', partyMappingController)
    .controller('PaymentController', PaymentController)
    .controller('paymentModeChangeController', paymentModeChangeController)
    .controller('payRegisterBDReportComController', payRegisterBDReportComController)
    .controller('payRegisterBDReportController', payRegisterBDReportController)
    .controller('payRegisterBDReportNewController', payRegisterBDReportNewController)
    .controller('payrollGroupMasterController', payrollGroupMasterController)
    .controller('paySlipsController', paySlipsController)
    .controller('paySlipsNewController', paySlipsNewController)
    .controller('pFEmployeeAppliedController', PFEmployeeAppliedController)
    .controller('pFEmployeeVoluntaryValueController', PFEmployeeVoluntaryValueController)
    .controller('PFPolicyController', PFPolicyController)
    .controller('PhysicalStockAdjustmentMasterController', PhysicalStockAdjustmentMasterController)
    .controller('plantWiseGateController', plantWiseGateController)
    .controller('preallocatedOTController', preallocatedOTController)
    .controller('preallocatedOTReportController', preallocatedOTReportController)
    .controller('PrePurchaseInvoiceController', PrePurchaseInvoiceController)
    .controller('ProductionReportsController', ProductionReportsController)
    .controller('ProductionSummarySFGController', ProductionSummarySFGController)
    .controller('PromotionIncrementApprovalController', PromotionIncrementApprovalController)
    .controller('providentFundStatementReportandCSVController', providentFundStatementReportandCSVController)
    .controller('PurchaseBookingSodaController', PurchaseBookingSodaController)
    .controller('PurchaseDocumentAcceptanceController', PurchaseDocumentAcceptanceController)
    .controller('PurchaseDocumentAcceptancePostController', PurchaseDocumentAcceptancePostController)
    .controller('purchaseLCAmendmentController', purchaseLCAmendmentController)
    .controller('purchaseLCChargesPostController', purchaseLCChargesPostController)
    .controller('purchaseLCController', purchaseLCController)
    .controller('PurchaseLCWithPOController', PurchaseLCWithPOController)
    .controller('PurchaseOrderController', PurchaseOrderController)
    .controller('autoLoanController', autoLoanController)
    .controller('autoLoanPostController', autoLoanPostController)
    .controller('QMSDefectMasterController', QMSDefectMasterController)
    .controller('QMSInspectionController', QMSInspectionController)
    .controller('QMSMasterController', QMSMasterController)
    .controller('QMSRejectionController', QMSRejectionController)
    .controller('qualityStdSetController', qualityStdSetController)
    .controller('quickCostingMasterController', quickCostingMasterController)
    .controller('rawDataDownloadController', rawDataDownloadController)
    .controller('RestTypeController', RestTypeController)
    .controller('salaryCertificateReportController', salaryCertificateReportController)
    .controller('salaryDisbursementController', salaryDisbursementController)
    .controller('salaryHeadWiseAmountSettingController', salaryHeadWiseAmountSettingController)
    .controller('SalaryHeadWiseAmountTransactionController', SalaryHeadWiseAmountTransactionController)
    .controller('salaryJournalController', salaryJournalController)
    .controller('salaryPayableController', salaryPayableController)
    .controller('salaryPayableDisbursementController', salaryPayableDisbursementController)
    .controller('salaryPaymentStatementsController', salaryPaymentStatementsController)
    .controller('salaryProcessedReportComplianceController', salaryProcessedReportComplianceController)
    .controller('salaryProcessedReportController', salaryProcessedReportController)
    .controller('ArrearProcessedReportController', ArrearProcessedReportController)
    .controller('ArrearProcessedTotalReportController', ArrearProcessedTotalReportController)
    .controller('salaryProcessedReportSummaryController', salaryProcessedReportSummaryController)
    .controller('SalaryProcessOtherStatusController', SalaryProcessOtherStatusController)
    .controller('SalaryProcessOtherStatusNewController', SalaryProcessOtherStatusNewController)
    .controller('salaryRuleController', salaryRuleController)
    .controller('salarySlabWiseValueController', salarySlabWiseValueController)
    .controller('salaryStructureAndProcessedReportController', salaryStructureAndProcessedReportController)
    .controller('SalaryStructureDataUploadController', SalaryStructureDataUploadController)
    .controller('salaryStructureSheetController', salaryStructureSheetController)
    .controller('SandwichAbsentController', SandwichAbsentController)
    .controller('SandWichLeaveOnHolidayController', SandWichLeaveOnHolidayController)
    .controller('SecretarialDocumentCategoryController', SecretarialDocumentCategoryController)
    .controller('SecretarialDocumentSubCategoryController', SecretarialDocumentSubCategoryController)
    .controller('separatedsalaryStructureController', separatedsalaryStructureController)
    .controller('servicePayableController', servicePayableController)
    .controller('SFBonusSheetGridReportController', SFBonusSheetGridReportController)
    .controller('SFBonusSheetReportController', SFBonusSheetReportController)
    .controller('shiftAssignmentController', shiftAssignmentController)
    .controller('shiftAssignmentDeleteController', shiftAssignmentDeleteController)
    .controller('shiftCreationController', shiftCreationController)
    .controller('ShiftRosterCreationController', ShiftRosterCreationController)
    .controller('shiftSummaryController', shiftSummaryController)
    .controller('skillGroupingController', skillGroupingController)
    .controller('skillMatrixController', skillMatrixController)
    .controller('SpecialFollowUpReportController', SpecialFollowUpReportController)
    .controller('suspensePayableController', suspensePayableController)
    .controller('TaskCategoryController', TaskCategoryController)
    .controller('TaskCategoryIssueController', TaskCategoryIssueController)
    .controller('TaskCategoryToDoController', TaskCategoryToDoController)
    .controller('taskManagerDashboardController', taskManagerDashboardController)
    .controller('TaskMasterCreationController', TaskMasterCreationController)
    .controller('TaskReplacementController', TaskReplacementController)
    .controller('TaskScheduleController', TaskScheduleController)
    .controller('TaskSubCategoryController', TaskSubCategoryController)
    .controller('TaskSubCategoryIssueController', TaskSubCategoryIssueController)
    .controller('TaskSubCategoryToDoController', TaskSubCategoryToDoController)
    .controller('TaskTemplateController', TaskTemplateController)
    .controller('tbsAssignController', tbsAssignController)
    .controller('tBSController', tBSController)
    .controller('tiffinBillReportController', tiffinBillReportController)
    .controller('tiffinBillReportSummaryController', tiffinBillReportSummaryController)
    .controller('userPasswordChangeController', UserPasswordChangeController)
    .controller('vendorChargeWriteOffController', vendorChargeWriteOffController)
    .controller('welfareReportsController', welfareReportsController)
    .controller('welfareReturnController', welfareReturnController)
    .controller('WithinYearLeaveEncashmentController', WithinYearLeaveEncashmentController)
    .controller('workersLateStatusController', workersLateStatusController)
    .controller('POLCMapController', POLCMapController)
    .controller("elementCodeController", ElementCodeController)
    .controller("sewingCodeController", SewingCodeController)
    .controller("productionSystemAllowanceController", ProductionSystemAllowanceController)
    .controller("vASElementTypeController", VASElementTypeController)
    .controller("timeCaptureController", TimeCaptureController)
    .controller("bartackCodeController", BartackCodeController)
    .controller("vasReportController", VASReportController)
    .controller("vasSAMCompareController", VASSAMCompareController)
    .controller("vasApprovalController", VASApprovalController)
    .controller('budgetMasterReportController', budgetMasterReportController)
    .controller('MasterOrderTaskTemplateController', MasterOrderTaskTemplateController)
    .controller('ExternalDataUploadFromExcelController', ExternalDataUploadFromExcelController)
    .controller('CropTypeController', CropTypeController)
    .controller('CropCategoryController', CropCategoryController)
    .controller('CropSubCategoryController', CropSubCategoryController)
    .controller('FarmingProcessController', FarmingProcessController)
    .controller('LandCategoryController', LandCategoryController)
    .controller('CropMasterController', CropMasterController)
    .controller('FarmerMasterController', FarmerMasterController)
    .controller('ICSMasterController', ICSMasterController)
    .controller('TalukController', TalukController)
    .controller('VillageController', VillageController)
    .controller('CropPlanningController', CropPlanningController)
    .controller('PurchaseBookingSodaController', PurchaseBookingSodaController)
    .controller('PaymentController', PaymentController)
    .controller('ConfirmationController', ConfirmationController)
    .controller('ApprovalController', ApprovalController)
    .controller('CropRateLocationController', CropRateLocationController)
    .controller('machineBudgetController', machineBudgetController)
    .controller('DailyCropRateController', DailyCropRateController)
    .controller('FarmingCategoryController', FarmingCategoryController)
    .controller('TransactionTypeController', TransactionTypeController)
    //.controller('EmployeeServiceTypeController', EmployeeServiceTypeController)
    .controller('EmployeeBankAccountInfoController', EmployeeBankAccountInfoController)
    .controller('JobWorkItemController', JobWorkItemController)
    .controller('rePrintNonCashCheckController', rePrintNonCashCheckController)
    .controller('rePrintCashCheckController', rePrintCashCheckController)
    .controller('MissedPunchReportController', MissedPunchReportController)
    .controller('ArrearController', ArrearController)
    .controller('ArrearApprovalController', ArrearApprovalController)
    .controller('BulkIncrementSalaryStructureDataUploadController', BulkIncrementSalaryStructureDataUploadController)
    .controller('chourlyOTReportController', chourlyOTReportController)
    .controller('ProcessAndResourcesConstraintController', ProcessAndResourcesConstraintController)
    .controller("interCompanyPartyController", InterCompanyPartyController)
    .controller('EmployeePlantTransferController', EmployeePlantTransferController)
    .controller('EmployeePlantTransferNewController', EmployeePlantTransferNewController)
   .controller('CompanyWiseEmployeePlantTransferController', CompanyWiseEmployeePlantTransferController)
    .controller("inventoryTransferJournalController", inventoryTransferJournalController)
    //.controller("jobWorkItemController", JobWorkItemController)
    .controller("checkVoidController", checkVoidController)
    .controller("checkManagementReportController", checkManagementReportController)
    .controller("wipReportController", wipReportController)
    .controller("recipeOperationController", recipeOperationController)
    .controller("utilityController", UtilityController)
    .controller("DesignationBudgetController", DesignationBudgetController)
    .controller("manpowerBudgetDesignationReportController", manpowerBudgetDesignationReportController)
    .controller("cahourlyOTReportController", cahourlyOTReportController)
    .controller("balanceSheetReportGroupWiseController", balanceSheetReportGroupWiseController)
    .controller("bulletinReportController", bulletinReportController)
    .controller("trialBalanceReportGroupWiseController", trialBalanceReportGroupWiseController)
    .controller("jwActivityController", jwActivityController)
    .controller("jwLocationController", jwLocationController)
    .controller("jwTransformationMasterController", jwTransformationMasterController)
    .controller("jwItemController", jwItemController)
    .controller("partyPaymentStatusController", partyPaymentStatusController)
    .controller("RCMTaxPayableReportController", RCMTaxPayableReportController)
    .controller("RCMTaxPayableSalesReportController", RCMTaxPayableSalesReportController)
    .controller("RCMTaxReceivableSalesReportController", RCMTaxReceivableSalesReportController)
    .controller("TDSDeductionReportController", TDSDeductionReportController)
    .controller("GSTReceivableReportController", GSTReceivableReportController)
    .controller("GSTPayableSalesReportController", GSTPayableSalesReportController)
    .controller("BonusProcessController", BonusProcessController)
    .controller("elementCodeController", ElementCodeController)
    .controller("jobWorkItemController", JobWorkItemController)
    .controller("jobWorkActivityController", jobWorkActivityController)
    .controller("jobWorkLocationController", jobWorkLocationController)
    .controller("jobWorkValueAddedMasterController", jobWorkValueAddedMasterController)
    .controller("jobWorkTransformationMasterController", jobWorkTransformationMasterController)
    .controller("JobWorkValueAddedContractController", JobWorkValueAddedContractController)
    .controller("OSIssueReturnController", OSIssueReturnController)
    .controller("JobWorkIssueReturnConfirmationController", JobWorkIssueReturnConfirmationController)
    .controller("JobWorkRegisterController", JobWorkRegisterController)
    .controller("DailyAttendanceInformationController", DailyAttendanceInformationController)
    .controller("RCMTaxReceivableReportController", RCMTaxReceivableReportController)
    .controller("dayBooksReportController", dayBooksReportController)
    .controller("DailyAttendanceSummeryReportController", DailyAttendanceSummeryReportController)
    .controller("purchaseReturnPostController", purchaseReturnPostController)
    .controller("EmployeeServiceVariableController", EmployeeServiceVariableController)
    .controller("gstR2ReportController", gstR2ReportController)
    .controller("LeaveDeleteSingleDayController", LeaveDeleteSingleDayController)
    .controller("LeaveDeleteSingleDayNewController", LeaveDeleteSingleDayNewController)
    .controller("MonthlyAttendanceSummeryReportController", MonthlyAttendanceSummeryReportController)
    .controller("OSTransformationPOController", OSTransformationPOController)
    .controller("ProfessionalTaxOBController", ProfessionalTaxOBController)
    .controller("TaxOBController", TaxOBController)
    .controller("LateDeductionController", LateDeductionController)
    .controller("EmployeeDayStatusReportController", EmployeeDayStatusReportController)
    .controller("VoucherController", VoucherController)
    .controller("BOQGenerationController", BOQGenerationController)
    .controller("BOQController", BOQController)
    .controller("SalarySheetBudgetaryOTController", SalarySheetBudgetaryOTController)
    .controller("IncrementReportController", IncrementReportController)
    .controller("IncrementReportSummaryController", IncrementReportSummaryController)
    .controller("partyPaymentStatusDetailController", partyPaymentStatusDetailController)
    .controller("salaryProcessedReportBudgetaryController", salaryProcessedReportBudgetaryController)
    .controller('IndividualGratuityPolicyController', IndividualGratuityPolicyController)
    .controller("LcNavigationController", LcNavigationController)
    .controller("QuickBOQReportController", QuickBOQReportController)
    .controller("AttendanceManualDataUploadController", AttendanceManualDataUploadController)
    .controller('GraruityInsuranceReportController', GraruityInsuranceReportController)
    .controller('FinalAttendanceProcessController', FinalAttendanceProcessController)
    .controller('ShiftChangeSectionWiseController', ShiftChangeSectionWiseController)
    .controller('LeaveBalanceReportController', LeaveBalanceReportController)
    .controller('LeaveBalanceToDateReportController', LeaveBalanceToDateReportController)
    .controller('welfareSummaryReportController', welfareSummaryReportController)
    .controller('salaryStructureReportPlantWiseController', salaryStructureReportPlantWiseController)
    .controller('GRNUncheckedAndUnApprovedController', GRNUncheckedAndUnApprovedController)
    .controller('POUncheckedAndUnApprovedController', POUncheckedAndUnApprovedController)
    .controller('expenseRegisterReportController', expenseRegisterReportController)
    .controller('ManualAttendanceFileUploadController', ManualAttendanceFileUploadController)
    .controller('ManualAttendanceWithShiftController', ManualAttendanceWithShiftController)
    .controller('ProductionDashboardController', ProductionDashboardController)
    .controller('BOQUploadController', BOQUploadController)
    .controller('PostSalesInvoiceController', PostSalesInvoiceController)
    .controller('DailyDayStatusReportController', DailyDayStatusReportController)
    .controller('InGatePassController', InGatePassController)
    .controller('MonthlyLunchOutReportController', MonthlyLunchOutReportController)
    .controller('InGatePassEntryController', InGatePassEntryController)
    .controller('ServicePOIndividualController', ServicePOIndividualController)
    .controller('LunchOutDashboardController', LunchOutDashboardController)
    .controller('hrDashboardtrController', hrDashboardtrController)
    .controller('OTPlanningController', OTPlanningController)
    .controller('ManualOTReportController', ManualOTReportController)
    .controller('DailyAttendanceReportController', DailyAttendanceReportController)
    .controller('mixingController', mixingController)
    .controller('RequisitionRegisterController', RequisitionRegisterController)
    .controller('monthlyAttendanceInformationDateRangeController', monthlyAttendanceInformationDateRangeController)
    .controller('MonthlyAttendanceInformationDateRangeNewController', MonthlyAttendanceInformationDateRangeNewController)
    .controller('AttendanceFromAppReportController', AttendanceFromAppReportController)
    .controller('EmployeeLastPunchReportController', EmployeeLastPunchReportController)
    .controller('EntireYearPresentDaysSummaryController', EntireYearPresentDaysSummaryController)
    .controller('professionalTaxReportsController', professionalTaxReportsController)
    .controller("monthlyGoodWorkReportController", monthlyGoodWorkReportController)
    .controller("monthlyGoodWorkReportNewController", monthlyGoodWorkReportNewController)
    .controller("weekOffOTReportController", weekOffOTReportController)
    .controller("weekOffOTReportOriginalController", weekOffOTReportOriginalController)
    .controller("DispatchMasterController", DispatchMasterController)
    .controller("FinancialStatusCustomerReceivableInvoiceDetailController", FinancialStatusCustomerReceivableInvoiceDetailController)
    .controller("holidayOTReportController", holidayOTReportController)
    .controller("holidayOTReportOriginalController", holidayOTReportOriginalController)
    .controller("MovementItemsController", MovementItemsController)
    .controller("MovementMaterialMasterController", MovementMaterialMasterController)
    .controller("salaryProcessedReportExtraOTCTCController", salaryProcessedReportExtraOTCTCController)
    .controller("salaryProcessedReportExtraOTCTCOriginalController", salaryProcessedReportExtraOTCTCOriginalController)
    .controller("FarmingDashboardController", FarmingDashboardController)
    .controller("BOQPurchaseOrderController", BOQPurchaseOrderController)
    .controller("PhysicalVerificationReportController", PhysicalVerificationReportController)
    .controller("payRegisterBDReportContractorController", payRegisterBDReportContractorController)
    .controller("yearlySalaryProcessedReportController", yearlySalaryProcessedReportController)
    .controller("MovementScanDataReportController", MovementScanDataReportController)
    .controller("OS3DashboardController", OS3DashboardController)
    .controller("WeighingScaleReportController", WeighingScaleReportController)

    .controller("BlackListController", BlackListController)
    .controller("JobEvaluationAttributeController", JobEvaluationAttributeController)
    .controller("JobEvaluationMasterController", JobEvaluationMasterController)
    .controller("JobEvaluationController", JobEvaluationController)
    .controller("JobEvaluationReportController", JobEvaluationReportController)
    .controller("consecutiveAttendaceController", consecutiveAttendaceController)
    .controller("consecutiveOTHoursController", consecutiveOTHoursController)
    .controller("FGValuationController", FGValuationController)
    .controller("bonusProvisionReportController", bonusProvisionReportController)
    .controller("bonusReportCController", bonusReportCController)
    .controller("paySlipsContractorController", paySlipsContractorController)
    .controller("PackingController", PackingController)
    .controller("GatePassRegisterController", GatePassRegisterController)
    .controller("bankSheetGenerationController", bankSheetGenerationController)
    .controller("salaryStructureSheetDailyController", salaryStructureSheetDailyController)
    .controller("MaterialReconcilationReportController", MaterialReconcilationReportController)
    .controller("OSReceiptValueAddedController", OSReceiptValueAddedController)
    .controller("ExceptionOTProcessController", ExceptionOTProcessController)
    .controller("FinishGoodsBookingController", FinishGoodsBookingController)
    .controller("ConsumptionBookingController", ConsumptionBookingController)
    .controller("AuditReportSummeryController", AuditReportSummeryController)
    .controller("AuditReportSummaryNewController", AuditReportSummaryNewController)
    .controller("CompanyProvidentFundStatementReportController", CompanyProvidentFundStatementReportController)
    .controller("ESICStatementsCompanyController", ESICStatementsCompanyController)
    .controller("GratuityReportCompanyController", GratuityReportCompanyController)
    .controller("EmployeeAdvanceDeductionController", EmployeeAdvanceDeductionController)
    .controller("RackController", RackController)
    .controller("PORollBackController", PORollBackController)
    .controller("DailyTargetController", DailyTargetController)
    .controller("jwPOIssueController", jwPOIssueController)
    .controller("EmployeeAdditionDeductionController", EmployeeAdditionDeductionController)
    .controller("generalLedgerVSfixedAssetsController", generalLedgerVSfixedAssetsController)
    .controller("ProductionRelayController", ProductionRelayController)
    .controller("inventoryOutSourceReceivePostController", inventoryOutSourceReceivePostController)
    .controller("ManualShiftController", ManualShiftController)
    .controller("ManualShiftNewController", ManualShiftNewController)
    .controller("OSReceiveBillingController", OSReceiveBillingController)
    .controller("entityFixedAssetsRegisterController", entityFixedAssetsRegisterController)
    .controller("voucherParkController", voucherParkController)
    .controller("salaryProcessedReportExtraOTCTCCompanyController", salaryProcessedReportExtraOTCTCCompanyController)

    .controller("EmployeeAdditionDeductionProcessController", EmployeeAdditionDeductionProcessController)
    .controller("MarkerController", MarkerController)
    .controller("partyPaymentStatusReportController", partyPaymentStatusReportController)
    .controller("OTManualNewController", OTManualNewController)
    .controller("ManualOTUploadNewController", ManualOTUploadNewController)
    .controller("ManualOTReportNewController", ManualOTReportNewController)
    .controller("CutPlanController", CutPlanController)
    .controller("FinishGoodsBookingPostController", FinishGoodsBookingPostController)
    .controller("PackingInvoiceController", PackingInvoiceController)
    .controller("CompanyWiseExternalDataUploadFromExcelController", CompanyWiseExternalDataUploadFromExcelController)
    .controller("CompanyWiseBankSheetController", CompanyWiseBankSheetController)
    .controller("PayrollManagementDashboardController", PayrollManagementDashboardController)
    .controller("InventorySalesReturnController", InventorySalesReturnController)
    .controller("ProductionConversionParameterController", ProductionConversionParameterController)
    .controller("ProductionTransformationBookingController", ProductionTransformationBookingController)
    .controller("EmployeeJobLocationController", EmployeeJobLocationController)
    .controller("salesPackingPostController", salesPackingPostController)
    .controller("AttendanceRawDataFromAppController", AttendanceRawDataFromAppController)
    .controller("WeekOffUpdatesController", WeekOffUpdatesController)
    .controller("RosterUpdatesController", RosterUpdatesController)
    .controller("OrderController", OrderController)
    .controller("SalesOrderUpdateController", SalesOrderUpdateController)
    .controller("POParameterChangeController", POParameterChangeController)

    .controller("NewAttdnDashboardController", NewAttdnDashboardController)
    .controller("EmployeeBudgetUpdateController", EmployeeBudgetUpdateController)
    .controller("AttendanceDashboardController", AttendanceDashboardController)
    .controller("NewAttdnProcessLockController", NewAttdnProcessLockController)
    .controller("NewHRDashboardController", NewHRDashboardController)
    .controller("OutsourceBillingPostController", OutsourceBillingPostController)
    .controller("ProductionOrderProcessWithRateController", ProductionOrderProcessWithRateController)
    .controller("entityWiseExpenseAndEarningController", entityWiseExpenseAndEarningController)
    .controller("ProductionOrderRateReportController", ProductionOrderRateReportController)
    .controller("EmployeeLeaveApplicationNewController", EmployeeLeaveApplicationNewController)
    .controller('EmployeeLeaveApprovalNewController', EmployeeLeaveApprovalNewController)
    .controller('employeeLeaveDeleteApplicationNewController', employeeLeaveDeleteApplicationNewController)
    .controller('ProductionTargetReportController', ProductionTargetReportController)
    .controller('FabricRollController', FabricRollController)
    .controller('FinalDeductionReportController', FinalDeductionReportController)
   

    .controller("PostInvoiceController", PostInvoiceController)

    .controller('GRNRequisitionSOAllocationController', GRNRequisitionSOAllocationController)
    .controller('salaryProcessedReportControllerNew', salaryProcessedReportControllerNew)
    .controller('salaryStructureAndProcessedReportNewController', salaryStructureAndProcessedReportNewController)
    .controller('finishGoodsInventoryRegisterController', finishGoodsInventoryRegisterController)
    .controller('LineLayoutForProductionBulletinController', LineLayoutForProductionBulletinController)
    .controller('EmployeeWeekOffUpdatesController', EmployeeWeekOffUpdatesController)
    .controller('SalaryDisbursementReportController', SalaryDisbursementReportController)
    .controller('OSissueRegisterController', OSissueRegisterController)
    .controller('SandwichProcessController', SandwichProcessController)
    .controller('JobWorkTransformationPOController', JobWorkTransformationPOController)
    .controller('AssetWIPStatusController', AssetWIPStatusController)
    .controller('OTConfirmationProcessController' , OTConfirmationProcessController)
    .controller("multipleResignationApprovalNewController", multipleResignationApprovalNewController)
    .controller('JWIssueReturnController', JWIssueReturnController)
    .controller('MachineLayoutReportController', MachineLayoutReportController)


    .config(AccessControllerConfig)
    .config(accountConfig)
    .config(bankConfig)
    .config(BiometricConfig)
    .config(CommercialConfig)
    .config(CostingsConfig)
    .config(employeeConfig)
    .config(EmployeeServicesConfig)
    .config(FarmingConfig)
    .config(fixedAssetConfig)
    .config(HumanResourceConfig)
    .config(IEConfig)
    .config(IssueTrackerConfig)
    .config(leaveConfig)
    .config(MachineConfig)
    .config(MaterialConfig)
    .config(OrderManagementConfig)
    .config(OrganizationConfig)
    .config(PartyConfig)
    .config(PayrollsConfig)
    .config(ProcessConfig)
    .config(ProductConfig)
    .config(ProductionsConfig)
    .config(ProjectConfig)
    .config(qmsConfig)
    .config(salesManagementConfig)
    .config(SecurityConfig)
    .config(SetupConfig)
    .config(SkillConfig)
    .config(TaskManagementConfig)
    .config(WorkCenterConfig)
    .config(JobWorkConfig)
    .config(OutsourcingConfig)
    .config(PerformanceManagementConfig)


    .config(["$routeProvider", "$locationProvider", "$httpProvider", function apanelConfig($routeProvider, $locationProvider, $httpProvider) {
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
    }])
    .run(["$rootScope", "$cookies", "$window", "$location", "$filter", "baseService", "$http", '$sce', 'SignalRInit',
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
        $rootScope.bootPoint = "#!/";
        $window.companyGroupId = $cookies.get("groupId");
        $window.authenticationToken = $cookies.get("authToken");
        $window.companyId = $cookies.get("companyId");
        $window.plantId = $cookies.get("plantId");
        $window.employeeId = $cookies.get("employeeId");
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