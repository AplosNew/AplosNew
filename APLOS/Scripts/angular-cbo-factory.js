cboService.$inject = ['$http', '$window', '$rootScope', 'baseService'];
function cboService($http, $window, $rootScope, baseService) {
    var service = {
        getEnumCbo: getEnumCbo
        , getSequence: getSequence
        , getCboPositionByEntityId: getCboPositionByEntityId
        , getCboRoleByCompanyGroup: getCboRoleByCompanyGroup
        , getCboPositionByCompanyGroup: getCboPositionByCompanyGroup
        , getCboVendorByCompany: getCboVendorByCompany
        , getCboDesignationByCompanyGroup: getCboDesignationByCompanyGroup
        , getCboRecruitmentProcess: getCboRecruitmentProcess
        , getCboLanguage: getCboLanguage
        , getCboShiftDefinationByPlant: getCboShiftDefinationByPlant
        , getCboLineByCompanyGroup: getCboLineByCompanyGroup
        , getCboLineByCompany: getCboLineByCompany
        , getCboDesignationGroupByCompanyGroup: getCboDesignationGroupByCompanyGroup
        , getCboSubSectionByCompanyGroup: getCboSubSectionByCompanyGroup
        , getCboSubSectionByCompany: getCboSubSectionByCompany
        , getCboSectionByCompanyGroup: getCboSectionByCompanyGroup
        , getCboSectionByCompany: getCboSectionByCompany
        , getCboDepartmentByCompanyGroup: getCboDepartmentByCompanyGroup
        , getCboDepartmentByCompany: getCboDepartmentByCompany
        , getCboSubDivisionByCompanyGroup: getCboSubDivisionByCompanyGroup
        , getCboSubDivisionByCompany: getCboSubDivisionByCompany
        , getCboDivisionByCompanyGroup: getCboDivisionByCompanyGroup
        , getCboDivisionByCompany: getCboDivisionByCompany
        , getCboEmployeeGroupByCompanyGroup: getCboEmployeeGroupByCompanyGroup
        , getCboBudgetMasterForSetup: getCboBudgetMasterForSetup
        , getCboFiscalYear: getCboFiscalYear
        , getCboActivity: getCboActivity
        , getCboActivityPhone: getCboActivityPhone
        , getCboActivityCompanyGroup: getCboActivityCompanyGroup
        , getCboActivityByEmployee: getCboActivityByEmployee
        , getCboActivityPhoneByEmployeeActivity: getCboActivityPhoneByEmployeeActivity
        , getCboBudgetByEmployeeActivity: getCboBudgetByEmployeeActivity
        , getBudgetMasterCboByCompanyAndGLId: getBudgetMasterCboByCompanyAndGLId
        , getBudgetMasterActivityCbo: getBudgetMasterActivityCbo
        , GetBudgetMasterActivityLevelEmployeeCbo: GetBudgetMasterActivityLevelEmployeeCbo
        , GetBudgetMasterActivityLevelPotalCbo: GetBudgetMasterActivityLevelPotalCbo
        , GetBudgetMasterActivityLevelCbo: GetBudgetMasterActivityLevelCbo
        , getBudgetMasterCboByCOAAndGLId: getBudgetMasterCboByCOAAndGLId
        , getCboBudgetForSetup: getCboBudgetForSetup
        , getCboActivityForSetup: getCboActivityForSetup
        , getCboBudgetGroupByCompanyGroup: getCboBudgetGroupByCompanyGroup
        , getCboRoutineBudgetMasterByEntityAndFY: getCboRoutineBudgetMasterByEntityAndFY
        , getCboModule: getCboModule
        , getCboModuleByCompanyGroup: getCboModuleByCompanyGroup
        , getCboSubModule: getCboSubModule
        , getCboSubModuleByModule: getCboSubModuleByModule
        , getPlantShiftCbo: getPlantShiftCbo
        , getEntityPlantShiftCbo: getEntityPlantShiftCbo
        , getCboEmployeeBudgetList: getCboEmployeeBudgetList
        , getCboEmployeeBudgetActivityList: getCboEmployeeBudgetActivityList
        , getBudgetCboByGL: getBudgetCboByGL
        , getCboTransactionCurrencyByCompany: getCboTransactionCurrencyByCompany
        , getCboCurrencyTransactionForPotal: getCboCurrencyTransactionForPotal
        , getCompanyCurrency: getCompanyCurrency
        , getParallelCurrency: getParallelCurrency
        , getCboParallelCurrency: getCboParallelCurrency
        , getCompanyGroupCurrencyCbo: getCompanyGroupCurrencyCbo
        , getCurrencyCboForPotal: getCurrencyCboForPotal
        , getCboUnit: getCboUnit
        , getCboUnitByCompanyGroup: getCboUnitByCompanyGroup
        , getCboUnitByCompany: getCboUnitByCompany
        , getCboCompanyGroup: getCboCompanyGroup
        , getCboCompanyByCompanyGroup: getCboCompanyByCompanyGroup
        , getCboInterCompany: getCboInterCompany
        , getCboCompanyByCOA: getCboCompanyByCOA
        , getCboCompanyGroupPayrollGroup: getCboCompanyGroupPayrollGroup
        , getCboPlant: getCboPlant
        , getCboPlantByCompanyGroup: getCboPlantByCompanyGroup
        , getCboPlantByCompany: getCboPlantByCompany
        , getCompanyGroupCompanyCbo: getCompanyGroupCompanyCbo
        , getCompanyLineCbo: getCompanyLineCbo
        , getEntityCompanyLineCbo: getEntityCompanyLineCbo
        , getCboEntityLineById: getCboEntityLineById
        , getCboEntityWithPlant: getCboEntityWithPlant
        , getCboInterEntityWithPlant: getCboInterEntityWithPlant
        , getCboInterPlant: getCboInterPlant
        , getCboEntityPlantWise: getCboEntityPlantWise
        , getCboEntityCompanyWise: getCboEntityCompanyWise
        , getCboWithEmployee: getCboWithEmployee
        , getCboEntityByCompanyWise: getCboEntityByCompanyWise
        , getCboEntityByPlant: getCboEntityByPlant
        , getEntityCboByPlant: getEntityCboByPlant
        , getCboProductionEntitiesByPlant: getCboProductionEntitiesByPlant
        , getEntityByUser: getEntityByUser
        , getEntityByGeneralUser: getEntityByGeneralUser
        , getCboEntityExceptionByCompany: getCboEntityExceptionByCompany
        , getCboEntityCostCenter: getCboEntityCostCenter
        , getCboEntityByCostCenter: getCboEntityByCostCenter
        , getCboEntityType: getCboEntityType
        , getCboEntityByCompanyGroup: getCboEntityByCompanyGroup
        , getCboEntityAndPositionRelationshipByCompanyGroupAndCompany: getCboEntityAndPositionRelationshipByCompanyGroupAndCompany
        , GetEntityProcessCbo: GetEntityProcessCbo
        , GetEntityProductionProcessCbo: GetEntityProductionProcessCbo
        , GetWCProcessCbo: GetWCProcessCbo
        , GetToWCProcessCbo: GetToWCProcessCbo
        , GetProductionShiftCbo: GetProductionShiftCbo
        , getCboProductionEntityByCompanyGroup: getCboProductionEntityByCompanyGroup
        , getCboProductionEntityByCompany: getCboProductionEntityByCompany
        , getCboProductionEntityByPlant: getCboProductionEntityByPlant
        , getShipModeCbo: getShipModeCbo
        , getFixedAssetList: getFixedAssetList
        , getFixedAssetClassList: getFixedAssetClassList
        , getFixedAssetSubClassList: getFixedAssetSubClassList
        , getFixedAssetCategoryList: getFixedAssetCategoryList
        , getFixedAssetSubCategoryList: getFixedAssetSubCategoryList
        , getFixedAssetItemList: getFixedAssetItemList
        , getFixedAssetMasterList: getFixedAssetMasterList
        , getSubAssetTypeList: getSubAssetTypeList
        , getCboBuyer: getCboBuyer
        , getBuyerStyleCboByBuyer: getBuyerStyleCboByBuyer
        , getBuyerDepartmentCboByBuyer: getBuyerDepartmentCboByBuyer
        , getBuyerDivisionCboByBuyer: getBuyerDivisionCboByBuyer
        , getBuyerBrandCboByBuyer: getBuyerBrandCboByBuyer
        , getWashOperationCbo: getWashOperationCbo
        , jobDescriptionCategoryList: jobDescriptionCategoryList
        , jobDescriptionSubCategoryList: jobDescriptionSubCategoryList
        , jobDescriptionItemList: jobDescriptionItemList
        , loadUtilityCbo: loadUtilityCbo
        , loadUomUtilityCbo: loadUomUtilityCbo
        , loadSubprocessCbo: loadSubprocessCbo
        , loadProcessWithCompanyCbo: loadProcessWithCompanyCbo
        , getProcessCbo: getProcessCbo
        , getCboProcessTypeByProcess: getCboProcessTypeByProcess
        , loadOperationCbo: loadOperationCbo
        , getCboRecruitmentProcessSetByCompanyGroup: getCboRecruitmentProcessSetByCompanyGroup
        , productionProcessGroupCbo: productionProcessGroupCbo
        , getProductionProcessCbo: getProductionProcessCbo
        , getCompanyProductionProcessCbo: getCompanyProductionProcessCbo
        , getCboRecruitmentGroupByPlant: getCboRecruitmentGroupByPlant
        , getCboManpowerBudgetByCompanyAndPlant: getCboManpowerBudgetByCompanyAndPlant
        , getCboBrand: getCboBrand
        , getCboReligion: getCboReligion
        , getCboBloodGroup: getCboBloodGroup
        , getCboQualificationLevel: getCboQualificationLevel
        , getCboQualificationStream: getCboQualificationStream
        , getCboChartOfAccount: getCboChartOfAccount
        , getCboDepreciationRule: getCboDepreciationRule
        , getCboChartOfAccountLevel1: getCboChartOfAccountLevel1
        , getCboChartOfAccountLevel2: getCboChartOfAccountLevel2
        , getCboChartOfAccountLevel3: getCboChartOfAccountLevel3
        , getCboChartOfAccountLevel4: getCboChartOfAccountLevel4
        , getCboChartOfAccountLevel5: getCboChartOfAccountLevel5
        , getCboChartOfAccountLevel6: getCboChartOfAccountLevel6
        , getCboGivenDesignation: getCboGivenDesignation
        , getCboLegalDesignation: getCboLegalDesignation
        , getTaxCategoryCboByCountry: getTaxCategoryCboByCountry
        , getTaxCodeCbo: getTaxCodeCbo
        , getCboWorkCenterMaster: getCboWorkCenterMaster
        , getCboWorkCenterMasterByEntity: getCboWorkCenterMasterByEntity
        , getCboProjectPlanningCategory: getCboProjectPlanningCategory
        , getCboProjectPlanningSubCategory: getCboProjectPlanningSubCategory
        , getCboProjectPlanning: getCboProjectPlanning
        , getCivilStatus: getCivilStatus
        , getCboCostCenterCategory: getCboCostCenterCategory
        , getCboCostCenterSubCategory: getCboCostCenterSubCategory
        , getCboServiceCategory: getCboServiceCategory
        , getCboServiceSubCategory: getCboServiceSubCategory
        , getCboServicePartyGroupCategory: getCboServicePartyGroupCategory
        , getCboServicePartyGroupSubCategory: getCboServicePartyGroupSubCategory
        , getCboServicePartyGroupClass: getCboServicePartyGroupClass
        , getCboSalutaion: getCboSalutaion
        , getTestingCategoryCbo: getTestingCategoryCbo
        , getPaymentModeCbo: getPaymentModeCbo
        , getUoMCbo: getUoMCbo
        , getToUoMFactor: getToUoMFactor
        , getHNSCbo: getHNSCbo
        , getTestinStdCbo: getTestinStdCbo
        , getCboSalesType: getCboSalesType
        , getUomCboByMaterialMaster: getUomCboByMaterialMaster
        , getUoMCboByMaterialGroup: getUoMCboByMaterialGroup
        , getCboSalesOrganisationByPlant: getCboSalesOrganisationByPlant
        , getPackingFromCboByCompanyGroup: getPackingFromCboByCompanyGroup
        , getCboRegister: getCboRegister
        , getCboSecurityTypeTaken: getCboSecurityTypeTaken
        , getCboSecurityTypeGiven: getCboSecurityTypeGiven
        , getCboOtherFinancingType: getCboOtherFinancingType
        , getCboInterCompanyFinancingType: getCboInterCompanyFinancingType
        , getInterCompanyAssetLiabilityType: getInterCompanyAssetLiabilityType
        , getCboInterPlantFinancingType: getCboInterPlantFinancingType
        , getCboEmployeeTransactionType: getCboEmployeeTransactionType
        , getCboEmployeeAdvanceSalaryTransactionType: getCboEmployeeAdvanceSalaryTransactionType
        , getEmpTrnTypeByAdvanceType: getEmpTrnTypeByAdvanceType
        , getCboAdvPayTranType: getCboAdvPayTranType
        , GetCboAssetLiabilityTranType: GetCboAssetLiabilityTranType
        , getCboFinanceTypeForAdvanceJournal: getCboFinanceTypeForAdvanceJournal
        , getCboVendorTranTypeList: getCboVendorTranTypeList
        , getCboCustomerTranTypeList: getCboCustomerTranTypeList
        , GetCboExpensesBookingTransactionType: GetCboExpensesBookingTransactionType
        , getCboComplianceDocumentCategory: getCboComplianceDocumentCategory
        , getCboComplianceDocumentSubCategory: getCboComplianceDocumentSubCategory
        , getCboEmployeeCategoryGroupByCompanyGroup: getCboEmployeeCategoryGroupByCompanyGroup
        , getCboRank: getCboRank
        , getCboLowerGivenDesignation: getCboLowerGivenDesignation
        , getCboAssetItemMachine: getCboAssetItemMachine
        , getCboSalutationByCompanyGroup: getCboSalutationByCompanyGroup
        , getCboDepartment: getCboDepartment
        , getCboDesignation: getCboDesignation
        , getCboParty: getCboParty
        , getCboUpperGivenDesignation: getCboUpperGivenDesignation
        , getCboCompanyPartyReconAdditionalGLList: getCboCompanyPartyReconAdditionalGLList
        , getPartyCbobyPartyTypeAccountGroup: getPartyCbobyPartyTypeAccountGroup
        , getCboAssetItemCharacteristics: getCboAssetItemCharacteristics
        , getCboVoucherType: getCboVoucherType
        , getCboVoucherTypeEmployeePayableList: getCboVoucherTypeEmployeePayableList
        , getCboVoucherTypeSalaryPayableList: getCboVoucherTypeSalaryPayableList
        , getCboVoucherTypeSalaryDisbursementList: getCboVoucherTypeSalaryDisbursementList
        , getCboVoucherTypeGoodWorkDisbursementList: getCboVoucherTypeGoodWorkDisbursementList
        , getCboVoucherTypeFinalSettlementDisbursementList: getCboVoucherTypeFinalSettlementDisbursementList
        , getCboVoucherTypeBonusDisbursementList: getCboVoucherTypeBonusDisbursementList
        , getCboVoucherTypeAccountReceivableList: getCboVoucherTypeAccountReceivableList
        , getCboVoucherTypeReceiptList: getCboVoucherTypeReceiptList
        , getCboVoucherTypeBanksReceiptList: getCboVoucherTypeBanksReceiptList
        , getCboVoucherTypeSuspensePayableList: getCboVoucherTypeSuspensePayableList
        , getCboVoucherTypeAccountPayableList: getCboVoucherTypeAccountPayableList
        , getCboVoucherTypeFGInventoryList: getCboVoucherTypeFGInventoryList
        , getCboVoucherTypePostInvoiceList: getCboVoucherTypePostInvoiceList
        , getCboVoucherTypeReceivableFromOthersList: getCboVoucherTypeReceivableFromOthersList
        , getCboVoucherTypeOutSourceBillingList: getCboVoucherTypeOutSourceBillingList
        , getCboVoucherTypePackingJournalList: getCboVoucherTypePackingJournalList
        , getCboVoucherTypePuechaseDocumentAcceptanceList: getCboVoucherTypePuechaseDocumentAcceptanceList
        , getCboVoucherTypePuechaseLCOpeningChargesList: getCboVoucherTypePuechaseLCOpeningChargesList
        , getCboVoucherTypeIssueJournalList: getCboVoucherTypeIssueJournalList
        , getCboVoucherTypeIssueReturnJournalList: getCboVoucherTypeIssueReturnJournalList
        , getCboVoucherTypeFixedAssetCapitalizeJournalList: getCboVoucherTypeFixedAssetCapitalizeJournalList
        , getCboVoucherTypeFiscalYearCloseJournalList: getCboVoucherTypeFiscalYearCloseJournalList
        , getCboVoucherTypeFixedAssetDepreciationJournalList: getCboVoucherTypeFixedAssetDepreciationJournalList
        , getCboVoucherTypeFixedAssetDisposeJournalList: getCboVoucherTypeFixedAssetDisposeJournalList
        , getCboVoucherTypePaymentList: getCboVoucherTypePaymentList
        , getCboVoucherTypePartyReconcilliationList: getCboVoucherTypePartyReconcilliationList
        , getCboVoucherTypeAdvanceTakenList: getCboVoucherTypeAdvanceTakenList
        , getCboVoucherTypeInterTransactionList: getCboVoucherTypeInterTransactionList
        , getCboVoucherTypeAdvanceTakenWriteOffList: getCboVoucherTypeAdvanceTakenWriteOffList
        , getCboVoucherTypeAdvanceGivenList: getCboVoucherTypeAdvanceGivenList
        , getCboVoucherTypeAdvanceGivenWriteOffList: getCboVoucherTypeAdvanceGivenWriteOffList
        , getCboVoucherTypeEmployeeAdvanceList: getCboVoucherTypeEmployeeAdvanceList
        , getCboVoucherTypeEmployeeAdvanceWriteOffList: getCboVoucherTypeEmployeeAdvanceWriteOffList
        , getCboVoucherTypeEmployeePaymentList: getCboVoucherTypeEmployeePaymentList
        , getCboVoucherTypeSecurityTakenList: getCboVoucherTypeSecurityTakenList
        , getCboVoucherTypeSecurityTakenWriteOffList: getCboVoucherTypeSecurityTakenWriteOffList
        , getCboVoucherTypeSecurityGivenList: getCboVoucherTypeSecurityGivenList
        , getCboVoucherTypeSecurityGivenWriteOffList: getCboVoucherTypeSecurityGivenWriteOffList
        , getCboVoucherTypeOpeningBalanceList: getCboVoucherTypeOpeningBalanceList
        , getCboVoucherTypeCustomerSuspense: getCboVoucherTypeCustomerSuspense
        , getCboVoucherTypeCreditNoteList: getCboVoucherTypeCreditNoteList
        , getCboVoucherTypeDebitNoteList: getCboVoucherTypeDebitNoteList
        , getCboVoucherTypeTaxPaymentList: getCboVoucherTypeTaxPaymentList
        , getCboVoucherTypeInventoryReturnPayableList: getCboVoucherTypeInventoryReturnPayableList
        , getCboVoucherTypeSalesReturnList: getCboVoucherTypeSalesReturnList
        , getCboDocumnetCategoryList: getCboDocumnetCategoryList
        , getCboCascadingComplianceDocumentSubCategory: getCboCascadingComplianceDocumentSubCategory
        , getCboComplianceDocumnetList: getCboComplianceDocumnetList
        , getMailReceiverCbo: getMailReceiverCbo
        , getStoppageCbo: getStoppageCbo
        , getRouteCbo: getRouteCbo
        , getHolidayCategoryCbo: getHolidayCategoryCbo
        , getCboCityByCompany: getCboCityByCompany
        , getBudgetMasterById: getBudgetMasterById
        , getBudgetClassCbo: getBudgetClassCbo
        , getBudgetCategoryCbo: getBudgetCategoryCbo
        , getBudgetGroupCbo: getBudgetGroupCbo
        , getBudgetTypeCbo: getBudgetTypeCbo
        , getBudgetSubCategoryCbo: getBudgetSubCategoryCbo
        , getBudgetItemCbo: getBudgetItemCbo
        , getBudgetActivityCbo: getBudgetActivityCbo
        , getBudgetCbo: getBudgetCbo
        , getBudgetCategoryCboByMaster: getBudgetCategoryCboByMaster
        , getBudgetSubCategoryCboByCategory: getBudgetSubCategoryCboByCategory
        , getBudgetCboBySubCategory: getBudgetCboBySubCategory
        , getLeaveTypeCbo: getLeaveTypeCbo
        , getLeaveTypeCumulativeCbo: getLeaveTypeCumulativeCbo
        , getYearCbo: getYearCbo
        , getResponsiblePersonCbo: getResponsiblePersonCbo
        , getTaxYearCbo: getTaxYearCbo
        , getSalaryHeadCbo: getSalaryHeadCbo
        , getSalaryFixationCbo: getSalaryFixationCbo
        , getCriticalityCbo: getCriticalityCbo
        , getActionCbo: getActionCbo
        , partyAccountGroupCbo: partyAccountGroupCbo
        , getRoasterCboByPlant: getRoasterCboByPlant
        , getRosterWiseShiftCbo: getRosterWiseShiftCbo
        , getCboReportingPerson: getCboReportingPerson
        , getCboMaterialStorageByCompanyAndPlant: getCboMaterialStorageByCompanyAndPlant
        , getCboLegalSalaryGrade: getCboLegalSalaryGrade
        , getCboEntityDivisionList: getCboEntityDivisionList
        , getCboEntitySubDivisionList: getCboEntitySubDivisionList
        , getCboEntityUnitList: getCboEntityUnitList
        , getCboTaxVariantByCompanyGroup: getCboTaxVariantByCompanyGroup
        , getCboAllCompanyTransactionList: getCboAllCompanyTransactionList
        , getCboWorkGroupListWithPlant: getCboWorkGroupListWithPlant
        , getCboYearlyCaledar: getCboYearlyCaledar
        , getCboBuyerActivity: getCboBuyerActivity
        , getCboEntityWiseDivision: getCboEntityWiseDivision
        , getCboEntityWisePlant: getCboEntityWisePlant
        , getCboEntityWiseSubDivision: getCboEntityWiseSubDivision
        , getCboEntityWiseUnit: getCboEntityWiseUnit
        , getCboSeasons: getCboSeasons
        , getCboBuyerProgram: getCboBuyerProgram
        , getEmployeeLocationCbo: getEmployeeLocationCbo
        , getCboEntityWiseEntity: getCboEntityWiseEntity
        , getEmpLeaveTypeCbo: getEmpLeaveTypeCbo
        , getCboLeaveType: getCboLeaveType
        , getCboPayRollGroupCbo: getCboPayRollGroupCbo
        , getActivityWithBuyerMasterCbo: getActivityWithBuyerMasterCbo
        , getOperationCbo: getOperationCbo
        , getLineCbo: getLineCbo
        , getCostCenterCbo: getCostCenterCbo
        , getCboLeaveYear: getCboLeaveYear
        , getCboFixedShift: getCboFixedShift
        , getCboCompliedRosterShift: getCboCompliedRosterShift
        , getCboRosterMaster: getCboRosterMaster
        , getSalesOrderCbo: getSalesOrderCbo
        , getShiftCbo: getShiftCbo
        , getWeekCbo: getWeekCbo
        , getJobLocationCbo: getJobLocationCbo
        , getShiftGroupCbo: getShiftGroupCbo
        , getSectionCbo: getSectionCbo
        , getCompliedShiftCbo: getCompliedShiftCbo
        , getActualShiftCbo: getActualShiftCbo
        , getCompliedShiftGroupingCbo: getCompliedShiftGroupingCbo
        , getOperationsCbo: getOperationsCbo
        , getEmployeeCbo: getEmployeeCbo
        , gettaskTypeCbo: gettaskTypeCbo
        , gettaskClassCbo: gettaskClassCbo
        , gettaskCategoryCbo: gettaskCategoryCbo
        , gettaskOrgCategoryCbo: gettaskOrgCategoryCbo
        , gettaskFrequencyCbo: gettaskFrequencyCbo
        , gettaskStatusCbo: gettaskStatusCbo
        , getCboMachine: getCboMachine
        , getCboEmployeeRosterShift: getCboEmployeeRosterShift
        , getCboRosterShift: getCboRosterShift
        , getconfirmationTemplateCbo: getconfirmationTemplateCbo
        , getappointmentTemplateCbo: getappointmentTemplateCbo
        , getYearCboList: getYearCboList
        , getrecipeCbo: getrecipeCbo
        , getUnitOfMeasurementCbo: getUnitOfMeasurementCbo
        , getRecipeOperationCbo: getRecipeOperationCbo
        , getMaterialMasterCbo: getMaterialMasterCbo
        , getMaterialAttributeCbo: getMaterialAttributeCbo
        , getCharacteristicsCbo: getCharacteristicsCbo
        , getProductionStatusCboByGroup: getProductionStatusCboByGroup
        , processCriteriaCbo: processCriteriaCbo
        , subprocessCbo: subprocessCbo
        , getMeasurementCbo: getMeasurementCbo
        , getCboWithBuyer: getCboWithBuyer
        , getRecipeMaterialGroupingMasterMeasurementCbo: getRecipeMaterialGroupingMasterMeasurementCbo
        , getTestingStandardCboByBuyer: getTestingStandardCboByBuyer
        , getSalaryProcessIdCboByYearMonth: getSalaryProcessIdCboByYearMonth
        , workcenterCboByProcessId: workcenterCboByProcessId
        , getPayGroupCbo: getPayGroupCbo
        , getAttendanceGroupCbo: getAttendanceGroupCbo
        , getAccountsGroupCbo: getAccountsGroupCbo
        , getHolidayCbo: getHolidayCbo
        , getIdCardTemplateCbo: getIdCardTemplateCbo
        , GetSeparationType: GetSeparationType
        , getCboSpecialTaxByPlant: getCboSpecialTaxByPlant
        , getLanguageIdCbo: getLanguageIdCbo
        , getTemplateCbo: getTemplateCbo
        , getDisciplinaryCategotyCbo: getDisciplinaryCategotyCbo
        , getCommitmentCbo: getCommitmentCbo
        , getProductMasterCbo: getProductMasterCbo
        , getLegalDesignationCbobyGivenDesignation: getLegalDesignationCbobyGivenDesignation
        , getCboOperationMasterByCompanyGroup: getCboOperationMasterByCompanyGroup
        , getSectionCboByDepartment: getSectionCboByDepartment
        , getSubSectionCboBySection: getSubSectionCboBySection
        , getLineCboBySubSection: getLineCboBySubSection
        , getCharacteristicsValueCbo: getCharacteristicsValueCbo
        , getCharacteristicsValueCboByCharacteristicsId: getCharacteristicsValueCboByCharacteristicsId
        , getCharacteristicsValueByPrCbo: getCharacteristicsValueByPrCbo
        , getProductionBookingPeriodCbo: getProductionBookingPeriodCbo
        , getSizeGroupCbo: getSizeGroupCbo
        , getAttachmentCbo: getAttachmentCbo
        , getGaugeFolderCbo: getGaugeFolderCbo
        , getFGComponentCbo: getFGComponentCbo
        , getFGZoneCbo: getFGZoneCbo
        , getOperationConsumptionCbo: getOperationConsumptionCbo
        , getMachineVariantCbo: getMachineVariantCbo
        , getOperationTypeCbo: getOperationTypeCbo
        , getOperationVariationCbo: getOperationVariationCbo
        , getMachineCbo: getMachineCbo
        , getAttendanceDayStatus: getAttendanceDayStatus
        , getSalaryHeadCategoryCbo: getSalaryHeadCategoryCbo
        , getCurrencyRuleCbo: getCurrencyRuleCbo
        , getRelationCbo: getRelationCbo
        , getProfessionCbo: getProfessionCbo
        , getSHCbo: getSHCbo
        , getBabyNoCbo: getBabyNoCbo
        , getPayRollGroupCbo: getPayRollGroupCbo
        , getSFGInventoryCbo: getSFGInventoryCbo
        , getOperationCategoryCbo: getOperationCategoryCbo
        , getEmployeeBankCbo: getEmployeeBankCbo
        , getTaskTemplateMasterCbo: getTaskTemplateMasterCbo
        , getAuthorizationConfigCbo: getAuthorizationConfigCbo
        , getIssueCategoryCbo: getIssueCategoryCbo
        , getIssueStandardCbo: getIssueStandardCbo
        , getIssueSubCategoryCbo: getIssueSubCategoryCbo
        , getIssueImportanceCbo: getIssueImportanceCbo
        , getEmployeeWorkTypeCbo: getEmployeeWorkTypeCbo
        , getSlrHeadCbo: getSlrHeadCbo
        , getCurrencyCbo: getCurrencyCbo
        , getDailyAllowanceCbo: getDailyAllowanceCbo
        , getCboOperationVariationByCompanyGroup: getCboOperationVariationByCompanyGroup
        , getCostingCategoryCbo: getCostingCategoryCbo
        , getCostingSubCategoryCbo: getCostingSubCategoryCbo
        , getEmployeeStatusWithMLVCbo: getEmployeeStatusWithMLVCbo
        , getPortCbo: getPortCbo
        , getCboCompanyByCompanyGroupWithAddressMaster: getCboCompanyByCompanyGroupWithAddressMaster
        , getSectionCboByDepartmentId: getSectionCboByDepartmentId
        , getSubSectionCboBySectionId: getSubSectionCboBySectionId
        , getPortByPlantCbo: getPortByPlantCbo
        , getPlanningTypesCbo: getPlanningTypesCbo
        , getCostingTypesCbo: getCostingTypesCbo
        , getMaterialMasterTypeCbo: getMaterialMasterTypeCbo
        , getCboRestType: getCboRestType
        , GetAdditionalPayDayCbo: GetAdditionalPayDayCbo
        , getCboVoucherTypeAutoLoanList: getCboVoucherTypeAutoLoanList
        , getCbomeetingType: getCbomeetingType
        , getContractFundCbo: getContractFundCbo
        , getPerformanceGroupListCbo: getPerformanceGroupListCbo
        , getbyDesignationMasterCbo: getbyDesignationMasterCbo
        , getUtilityGroupCbo: getUtilityGroupCbo

    };


    function getUtilityGroupCbo(callback) {
        base('Materials/UtilityGroup/GetCbo', callback);
    }

    function getbyDesignationMasterCbo(callback) {
        base('Organizations/Designation/GetbyDesignationMasterCbo', callback);
    }

    function getPerformanceGroupListCbo(callback) {
        base('HumanResource/PerformanceGroup/GetCbo', callback);
    }

    function getContractFundCbo(callback) {
        base('Commercial/ContractFundUtilization/GetCbo', callback);
    }
    function GetAdditionalPayDayCbo(plantId, callback) {
        base('Attendances/AdditionalPayDay/GetCbo?plantId=' + plantId, callback);
    }
    function getCboRestType(callback) {
        base('HumanResource/RestType/GetCbo', callback);
    }
    function getCbomeetingType(callback) {
        base('MeetingManagement/MeetingType/GetCbo', callback);
    }
    function getMaterialMasterTypeCbo(callback) {
        base('Materials/MaterialMasterType/GetCbo', callback);
    }
    function getCostingTypesCbo(callback) {
        base('Productions/CostingTypes/GetCbo', callback);
    }
    function getPlanningTypesCbo(callback) {
        base('Productions/PlanningTypes/GetCbo', callback);
    }

    function getSectionCboByDepartmentId(deptID, callback) {
        base('Employees/GuestUser/GetSectionCboByDepartment?deptID=' + deptID, callback);
    }
    function getSubSectionCboBySectionId(secID, callback) {
        base('Employees/GuestUser/GetSubSectionCboBySection?secID=' + secID, callback);
    }

    function getPortCbo(callback) {
        base('OrderManagements/Port/GetCbo', callback);
    }
    function getPortByPlantCbo(callback) {
        base('Commercial/PrePurchaseInvoice/GetPortByPlantCbo', callback);
    }

    function getCboOperationVariationByCompanyGroup(companyGroupId, callback) {
        if (baseService.isUndefinedOrNull(companyGroupId)) {
            if (!baseService.isUndefinedOrNull($window.companyGroupId)) {
                companyGroupId = $window.companyGroupId;
            }
            else
                companyGroupId = null;
        }
        base('employees/EmployeeInformation/GetOperationVariationCbo?companyGroupId=' + companyGroupId, callback);
    }
    function getCostingCategoryCbo(callback) {
        base('costings/costingCategory/getcbo', callback);
    }
    function getCostingSubCategoryCbo(callback) {
        base('costings/costingComponent/getcbo', callback);
    }
    function getSlrHeadCbo(callback) {
        base('Employees/AllowanceDaily/GetSalaryHeadCbo', callback);
    }
    function getCurrencyCbo(plantId, callback) {
        base('Payrolls/CurrencyRule/GetCurrencyCbo?plantId=' + plantId, callback);
    }
    function getDailyAllowanceCbo(callback) {
        base('humanresource/AttendanceManagement/GetDailyAllowanceCbo', callback);
    }
    function getEmployeeWorkTypeCbo(callback) {
        base('Employees/EmployeeWorkType/GetCbo', callback);
    }
    function getIssueImportanceCbo(callback) {
        base('IssueTracker/IssueImportance/GetCbo', callback);
    }
    function getIssueSubCategoryCbo(callback) {
        base('IssueTracker/IssueSubCategory/GetCbo', callback);
    }
    function getIssueStandardCbo(callback) {
        base('IssueTracker/IssueStandard/GetCbo', callback);
    }
    function getIssueCategoryCbo(callback) {
        base('IssueTracker/IssueCategory/GetCbo', callback);
    }
    function getTaskTemplateMasterCbo(callback) {
        base('Parties/BuyerMaster/GetTaskTemplateMasterCbo', callback)
    }
    function getSFGInventoryCbo(callback) {
        base('Products/SFGInventory/GetCbo', callback);
    }
    function getCurrencyRuleCbo(callback) {
        base('payrolls/salaryrule/GetCurrencyRuleCbo', callback);
    }
    function getRelationCbo(callback) {
        base('Employees/employeeinformation/GetRelationCbo', callback);
    }
    function getProfessionCbo(callback) {
        base('employees/employeeinformation/getprofessioncbo', callback);
    }

    function getSalaryHeadCategoryCbo(callback) {
        base('payrolls/salaryhead/getsalaryheadcategorycbo', callback);
    }
    function getMachineCbo(processId, callback) {
        base('IE/BulletinTemplate/getcbo?processId=' + processId, callback);
    }
    function getOperationVariationCbo(callback) {
        base('machines/operationvariation/getcbo', callback);
    }
    function getOperationCategoryCbo(callback) {
        base('machines/operationcategory/getcbo', callback);
    }
    function getOperationTypeCbo(callback) {
        base('machines/operationtype/getcbo', callback);
    }
    function getMachineVariantCbo(callback) {
        base('machines/machinevariant/getcbo', callback);
    }
    function getOperationConsumptionCbo(callback) {
        base('ie/operationconsumption/getcbo', callback);
    }
    function getFGZoneCbo(callback) {
        base('materials/fgzone/getcbo', callback);
    }
    function getFGComponentCbo(callback) {
        base('materials/fgcomponent/getcbo', callback);
    }
    function getGaugeFolderCbo(callback) {
        base('ie/gaugefolder/getcbo', callback);
    }
    function getAttachmentCbo(callback) {
        base('ie/attachment/getcbo', callback);
    }
    function getSizeGroupCbo(callback) {
        base('ie/sizegroup/getcbo', callback);
    }
    function getProductionBookingPeriodCbo(callback) {
        base('productions/productionbookingperiod/getcbo', callback);
    }
    function getCharacteristicsValueCbo(soid, callback) {
        base('productions/productionsummary/getcharacteristicsvaluecbo?soid=' + soid, callback);
    }
    function getCharacteristicsValueByPrCbo(soid, callback) {
        base('productions/productionsummary/getcharacteristicsvaluebyprocbo?soid=' + soid, callback);
    }
    function getCharacteristicsValueCboByCharacteristicsId(materialMasterId, characteristicsId, valueAssignmentLevel, callback) {
        base('Materials/CharacteristicsValue/GetCharacteristicsValueCboByCharacteristicsId?materialMasterId=' + materialMasterId + '&characteristicsId=' + characteristicsId + '&valueAssignmentLevel=' + valueAssignmentLevel, callback);

    }
    function getSectionCboByDepartment(deptID, callback) {
        base('humanresource/dailydaystatus/getsectioncbobydepartment?deptID=' + deptID, callback);
    }
    function getSubSectionCboBySection(secID, callback) {
        base('humanresource/dailydaystatus/getsubsectioncbobysection?secID=' + secID, callback);
    }
    function getLineCboBySubSection(subsecID, callback) {
        base('humanresource/dailydaystatus/getlinecbobysubsection?subsecID=' + subsecID, callback);
    }

    function getCboOperationMasterByCompanyGroup(companyGroupId, callback) {
        if (baseService.isUndefinedOrNull(companyGroupId)) {
            if (!baseService.isUndefinedOrNull($window.companyGroupId)) {
                companyGroupId = $window.companyGroupId;
            }
            else
                companyGroupId = null;
        }
        base('employees/recruitment/getoperationmastercbo?companyGroupId=' + companyGroupId, callback);
    }

    function getProductMasterCbo(callback) {
        base('Products/ProductMaster/getcbo', callback);
    }
    function getCommitmentCbo(callback) {
        base('OrderManagements/Commitment/getcbo', callback);
    }

    function getCboSpecialTaxByPlant(plantId, callback) {
        base('setups/specialtax/getcbo?plantId=' + plantId, callback);
    }

    function getDisciplinaryCategotyCbo(callback) {
        base('humanresource/disciplinaryActionCategory/getcbo', callback);
    }

    function getHolidayCbo(yearId, month, callback) {
        base('employees/holidayabsentismassignment/getholidaycbo?yearId=' + yearId + '&month=' + month, callback);
    }

    function getTestingStandardCboByBuyer(companyGroupId, buyerId, callback) {
        if (baseService.isUndefinedOrNull(companyGroupId)) {
            if (!baseService.isUndefinedOrNull($window.companyGroupId)) {
                companyGroupId = $window.companyGroupId;
            }
            else
                companyGroupId = null;
        }
        base('Setups/TestingStandard/gettestingstandardcbobybuyer?companyGroupId=' + companyGroupId + '&buyerId=' + buyerId, callback);
    }

    function getMeasurementCbo(materialMasterId, callback) {
        base('Productions/RecipeGlobalMaster/GetMeasurementCbo?materialMasterId=' + materialMasterId, callback);
    }

    function getRecipeMaterialGroupingMasterMeasurementCbo(recipeMaterialGroupingMasterId, callback) {
        base('Productions/RecipeGlobalMaster/GetRecipeMaterialGroupingMasterMeasurementCbo?recipeMaterialGroupingMasterId=' + recipeMaterialGroupingMasterId, callback);
    }

    function subprocessCbo(processid, callback) {
        base('productions/recipeglobalmaster/getsubprocesscbo?processid=' + processid, callback);
    }
    function workcenterCboByProcessId(processid, callback) {
        base('productions/ProductionSummary/GetCbo?processid=' + processid, callback);
    }
    function processCriteriaCbo(companyGroupId, callback) {
        if (baseService.isUndefinedOrNull(companyGroupId)) {
            if (!baseService.isUndefinedOrNull($window.companyGroupId)) {
                companyGroupId = $window.companyGroupId;
            }
            else
                companyGroupId = null;
        }
        base('productions/recipeglobalmaster/getcbo?companyGroupId=' + companyGroupId, callback);
    }

    function getProductionStatusCboByGroup(callback) {
        base('Productions/ProductionStatus/GetCbo', callback);
    }

    function getMaterialAttributeCbo(callback) {
        base('Productions/RecipeGlobalMaster/GetMaterialAttributeCbo', callback);
    }

    function getCharacteristicsCbo(callback) {
        base('Productions/RecipeGlobalMaster/GetCharacteristicsCbo', callback);
    }

    function getMaterialMasterCbo(callback) {
        base('Productions/RecipeGlobalMaster/GetMaterialMasterCbo', callback);
    }

    function getRecipeOperationCbo(processId, callback) {
        base('Productions/RecipeGlobalMaster/GetRecipeOperationCbo?processId=' + processId, callback);
    }

    function getUnitOfMeasurementCbo(callback) {
        base('Productions/RecipeGlobalMaster/GetUnitOfMeasurementCbo', callback);
    }

    function getYearCboList(plantId, callback) {
        base('employees/employeeleavecarryforward/getyearcbolist?plantId=' + plantId, callback);
    }
    function getrecipeCbo(entityId, callback) {
        base('productions/recipematerial/getrecipecbo?entityId=' + entityId, callback);
    }

    function getTemplateCbo(type, callback) {
        base('employees/employeeinformation/gettemplatecbo?type=' + type, callback);
    }

    function getIdCardTemplateCbo(callback) {
        base('employees/employeeinformation/getidcardcbo', callback);
    }

    function getappointmentTemplateCbo(callback) {
        base('employees/employeeinformation/getcbo', callback);
    }

    function getconfirmationTemplateCbo(callback) {
        base('employees/employeeprobationalperiod/getcbo', callback);
    }

    function getCboEmployeeRosterShift(empId, callback) {
        base('humanresource/compliedshiftassignment/getcbo?empId=' + empId, callback);
    }

    function getCboCompliedRosterShift(callback) {
        base('humanresource/compliedshiftassignment/getcompliedrostercbo', callback);
    }


    function getCboMachine(callback) {
        base('attendances/accesscontrollerlist/getcbo', callback);
    }

    function gettaskTypeCbo(callback) {
        base('taskmanagement/tasktype/getcbo', callback);
    }
    function gettaskClassCbo(callback) {
        base('taskmanagement/taskCategory/getcbo', callback);
    }
    function gettaskCategoryCbo(callback) {
        base('taskmanagement/taskclass/getcbo', callback);
    }
    function gettaskOrgCategoryCbo(callback) {
        base('taskmanagement/taskOrgCategory/getcbo', callback);
    }
    function gettaskFrequencyCbo(callback) {
        base('taskmanagement/taskFrequency/getcbo', callback);
    }
    function gettaskStatusCbo(callback) {
        base('taskmanagement/taskStatus/getcbo', callback);
    }
    function getEmployeeCbo(callback) {
        base('Employees/EmployeeInformation/GetEmployeeCbo', callback);
    }
    function getOperationsCbo(callback) {
        base('Machines/Operation/GetCbo', callback);
    }
    function getCompliedShiftGroupingCbo(callback) {
        base('HumanResource/CompliedShiftAssignment/GetCompliedShiftGroupingCbo', callback);
    }
    function getActualShiftCbo(callback) {
        base('HumanResource/CompliedShiftAssignment/GetActualShiftCbo', callback);
    }
    function getCompliedShiftCbo(callback) {
        base('HumanResource/CompliedShiftAssignment/GetCompliedShiftCbo', callback);
    }
    function getSectionCbo(callback) {
        base('HumanResource/CompliedShiftAssignment/GetSectionCbo', callback);
    }

    function getShiftGroupCbo(plantId, joblocationId, callback) {
        base('Setups/ShiftGroup/GetCbo?plantId=' + plantId + '&joblocationId=' + joblocationId, callback);
    }
    function getJobLocationCbo(plant, callback) {
        base('Setups/ShiftGroup/GetJobLocationCbo?plantId=' + plant, callback);
    }

    function getWeekCbo(callback) {
        base('Employees/WeeklyAbsentismAssignment/getWeekCbo', callback);
    }

    function getShiftCbo(date, linetext, salesorder, callback) {
        base('OrderManagements/LineEmployeeAssign/GetShiftCbo?date=' + date + '&linetext=' + linetext + '&salesorder=' + salesorder, callback);
    }

    function getSalesOrderCbo(date, linetext, callback) {
        base('OrderManagements/LineEmployeeAssign/GetSalesOrderCbo?date=' + date + '&linetext=' + linetext, callback);
    }

    function getLineCbo(date, callback) {
        base('OrderManagements/LineEmployeeAssign/GetLineCbo?date=' + date, callback);
    }
    function getCostCenterCbo(callback) {
        base('Organizations/CostCenter/GetCbo', callback);
    }
    function getOperationCbo(date, linetext, callback) {
        base('OrderManagements/LineEmployeeAssign/GetOperationCbo?date=' + date + '&linetext=' + linetext, callback);
    }

    function getActivityWithBuyerMasterCbo(buyermasterId, callback) {
        base('OrderManagements/Inquiry/GetActivityWithBuyerMasterCbo?buyermasterId=' + buyermasterId, callback);
    }


    function getLanguageIdCbo(callback) {
        base('HumanResource/PayRegisterBDReport/GetLanguageIdCbo', callback);
    }


    function getCboPayRollGroupCbo(companyGroupId, callback) {
        if (baseService.isUndefinedOrNull(companyGroupId)) {
            if (!baseService.isUndefinedOrNull($window.companyGroupId)) {
                companyGroupId = $window.companyGroupId;
            }
            else
                companyGroupId = null;
        }
        base('Payrolls/PayrollGroup/getcbo?companyGroupId=' + companyGroupId, callback);
    }



    function getEmployeeLocationCbo(callback) {
        base('Setups/EmployeeLocation/GetCbo', callback);
    }

    function getAttendanceGroupCbo(callback) {
        base('Setups/AttendanceGroup/GetCbo', callback);
    }

    function getAccountsGroupCbo(callback) {
        base('Employees/AccountsGroup/GetCbo', callback);
    }

    function getCboBuyerProgram(buyerId, callback) {
        base('Parties/BuyerProgram/getcbo?buyerId=' + buyerId, callback);
    }

    function getCboSeasons(callback) {
        base('OrderManagements/Seasons/getcbo', callback);
    }

    function getCboLeaveType(callback) {
        base('Employees/LeaveApplication/getcbo', callback);
    }

    function getEmpLeaveTypeCbo(empid, callback) {
        base('Employees/LeaveApplication/GetLeaveTypeCbo?EmpsystemId=' + empid, callback);
    }
    function getCboLeaveYear(callback) {
        base('Employees/LeaveApplication/GetYearCbo', callback);
    }
    function getCboFixedShift(callback) {
        base('Attendances/ShiftAssignment/GetFixedShift', callback);
    }
    function getCboRosterShift(rosterid, callback) {
        base('Attendances/ShiftAssignment/GetRosterShift?rosterid=' + rosterid, callback);
    }
    function getCboRosterMaster(callback) {
        base('Attendances/ShiftAssignment/GetRosterMaster', callback);
    }

    function getCboYearlyCaledar(callback) {
        base('Setups/yearlyCalendar/getcbo', callback);
    }

    function getCboBuyerActivity(activityType, callback) {
        base('Setups/OrderActivity/getcbo?activityType=' + activityType, callback);
    }

    function getCboWorkGroupListWithPlant(plantId, callback) {
        base('HumanResource/WorkGroup/getcbo?plantId=' + plantId, callback);
    }

    function getCboAllCompanyTransactionList(callback) {
        base('currencies/transactioncurrency/GetCboAllCompanyTransactionList', callback);
    }

    function getCboTransactionCurrencyByCompany(companyId, callback) {
        if (baseService.isUndefinedOrNull(companyId)) {
            if (!baseService.isUndefinedOrNull($window.companyId)) {
                companyId = $window.companyId;
            }
            else
                companyId = null;
        }
        base('currencies/transactioncurrency/GetCboCurrencyTransaction?companyId=' + companyId, callback);
    }

    function getCboCurrencyTransactionForPotal(companyId, callback) {
        if (baseService.isUndefinedOrNull(companyId)) {
            if (!baseService.isUndefinedOrNull($window.companyId)) {
                companyId = $window.companyId;
            }
            else
                companyId = null;
        }
        base('currencies/transactioncurrency/GetCboCurrencyTransactionForPotal?companyId=' + companyId, callback);
    }

    function getCboTaxVariantByCompanyGroup(callback) {
        base('Accounts/TaxCategory/GetTaxVariantCbo', callback);
    }

    function partyAccountGroupCbo(callback) {
        base('parties/partyaccountgroup/getcbo', callback);
    }

    function productionProcessGroupCbo(companyGroupId, callback) {
        if (baseService.isUndefinedOrNull(companyGroupId)) {
            if (!baseService.isUndefinedOrNull($window.companyGroupId)) {
                companyGroupId = $window.companyGroupId;
            }
            else
                companyGroupId = null;
        }
        base('Processes/ProductionProcessGroup/getcbo?companyGroupId=' + companyGroupId, callback);
    }

    function getCompanyProductionProcessCbo(companyId, callback) {
        if (baseService.isUndefinedOrNull(companyId)) {
            if (!baseService.isUndefinedOrNull($window.companyId))
                companyId = $window.companyId;
            else
                companyId = null;
        }
        base('Processes/CompanyProcess/GetCompanyProductionProcessCbo?companyId=' + companyId, callback);
    }
    function getProductionProcessCbo(callback) {
        base('Processes/Process/GetProductionProcessCbo', callback);
    }
    function getCriticalityCbo(callback) {
        base('employees/disciplinaryactioncriticality/getcbo', callback);
    }

    function getActionCbo(callback) {
        base('employees/disciplinaryaction/getcbo', callback);
    }

    function getSalaryFixationCbo(callback) {
        base('HumanResource/SalaryFixation/getcbo', callback);
    }

    function getSalaryHeadCbo(currencyRuleSystemID, callback) {
        base('payrolls/loanadvancemaster/getcbo?currencyRuleSystemID=' + currencyRuleSystemID, callback);
    }

    function getSHCbo(currencyRuleSystemID, callback) {
        base('payrolls/loanadvancemaster/GetSalaryHeadCbo?currencyRuleSystemID=' + currencyRuleSystemID, callback);
    }

    function getTaxYearCbo(companyGroupId, callback) {
        if (baseService.isUndefinedOrNull(companyGroupId)) {
            if (!baseService.isUndefinedOrNull($window.companyGroupId)) {
                companyGroupId = $window.companyGroupId;
            }
            else
                companyGroupId = null;
        }
        base('accounts/taxyear/getcbo?companyGroupId=' + companyGroupId, callback);
    }

    function getBudgetMasterById(id, callback) {
        base('accounts/budgetmaster/getbudgetmasterbyid/' + id, callback);
    }

    function getBudgetClassCbo(callback) {
        base('accounts/budgetclass/getcbo', callback);
    }

    function getLeaveTypeCbo(companyGroupId, callback) {
        //IF companyId Is null then it will get companyId from Identity
        if (baseService.isUndefinedOrNull(companyGroupId)) {
            if (!baseService.isUndefinedOrNull($window.companyGroupId)) {
                companyGroupId = $window.companyGroupId;
            }
            else
                companyGroupId = null;
        }
        base('employees/employeeleavecarryforward/leavetypelist?companyGroupId=' + companyGroupId, callback);
    }

    function getLeaveTypeCumulativeCbo(companyGroupId, callback) {
        //IF companyId Is null then it will get companyId from Identity
        if (baseService.isUndefinedOrNull(companyGroupId)) {
            if (!baseService.isUndefinedOrNull($window.companyGroupId)) {
                companyGroupId = $window.companyGroupId;
            }
            else
                companyGroupId = null;
        }
        base('employees/employeeleavecarryforward/leaveTypeCumulativeList?companyGroupId=' + companyGroupId, callback);
    }

    function getYearCbo(companyGroupId, callback) {
        //IF companyId Is null then it will get companyId from Identity
        if (baseService.isUndefinedOrNull(companyGroupId)) {
            if (!baseService.isUndefinedOrNull($window.companyGroupId)) {
                companyGroupId = $window.companyGroupId;
            }
            else
                companyGroupId = null;
        }
        base('employees/employeeleavecarryforward/getYear?companyGroupId=' + companyGroupId, callback);
    }

    function getBudgetCategoryCbo(callback) {
        base('accounts/budgetcategory/getcbo', callback);
    }

    function getBudgetGroupCbo(callback) {
        base('accounts/budgetgroup/getcbo', callback);
    }

    function getBudgetTypeCbo(callback) {
        base('accounts/budgettype/getcbo', callback);
    }

    function getBudgetSubCategoryCbo(callback) {
        base('accounts/budgetsubcategory/getcbo', callback);
    }

    function getBudgetItemCbo(callback) {
        base('accounts/budgetitem/getcbo', callback);
    }

    function getBudgetActivityCbo(callback) {
        base('accounts/budgetactivity/getcbo', callback);
    }

    function getBudgetCbo(callback) {
        base('accounts/budget/getcbo', callback);
    }

    function getBudgetCategoryCboByMaster(callback) {
        base('Accounts/BudgetMaster/GetBudgetCategoryCbo', callback);
    }

    function getBudgetSubCategoryCboByCategory(categoryId, callback) {
        base('Accounts/BudgetMaster/GetBudgetSubCategoryCboByCategory?categoryId=' + categoryId, callback);
    }

    function getBudgetCboBySubCategory(subCategoryId, callback) {
        base('Accounts/BudgetMaster/GetBudgetCboBySubCategory?subCategoryId=' + subCategoryId, callback);
    }

    function getHolidayCategoryCbo(companyGroupId, callback) {
        if (baseService.isUndefinedOrNull(companyGroupId)) {
            if (!baseService.isUndefinedOrNull($window.companyGroupId)) {
                companyGroupId = $window.companyGroupId;
            }
            else
                companyGroupId = null;
        }
        base('Setups/holidayCategory/getcbo?companyGroupId=' + companyGroupId, callback);
    }

    function getCboEntityDivisionList(companyGroupId, companyId, plantId, callback) {
        if (baseService.isUndefinedOrNull(companyGroupId)) {
            if (!baseService.isUndefinedOrNull($window.companyGroupId)) {
                companyGroupId = $window.companyGroupId;
            }
            else
                companyGroupId = null;
        }

        if (baseService.isUndefinedOrNull(companyId)) {
            if (!baseService.isUndefinedOrNull($window.companyId)) {
                companyId = $window.companyId;
            }
            else
                companyId = null;
        }
        base('Organizations/Entity/GetCboEntityDivisionList?companyGroupId=' + companyGroupId + '&companyId=' + companyId + '&plantId=' + plantId, callback);
    }

    function getCboEntitySubDivisionList(companyGroupId, companyId, plantId, divisionId, callback) {
        if (baseService.isUndefinedOrNull(companyGroupId)) {
            if (!baseService.isUndefinedOrNull($window.companyGroupId)) {
                companyGroupId = $window.companyGroupId;
            }
            else
                companyGroupId = null;
        }

        if (baseService.isUndefinedOrNull(companyId)) {
            if (!baseService.isUndefinedOrNull($window.companyId)) {
                companyId = $window.companyId;
            }
            else
                companyId = null;
        }
        base('Organizations/Entity/GetCboEntitySubDivisionList?companyGroupId=' + companyGroupId + '&companyId=' + companyId + '&plantId=' + plantId + '&divisionId=' + divisionId, callback);
    }

    function getCboEntityUnitList(companyGroupId, companyId, plantId, divisionId, subDivisionId, callback) {
        if (baseService.isUndefinedOrNull(companyGroupId)) {
            if (!baseService.isUndefinedOrNull($window.companyGroupId)) {
                companyGroupId = $window.companyGroupId;
            }
            else
                companyGroupId = null;
        }

        if (baseService.isUndefinedOrNull(companyId)) {
            if (!baseService.isUndefinedOrNull($window.companyId)) {
                companyId = $window.companyId;
            }
            else
                companyId = null;
        }
        base('Organizations/Entity/GetCboEntityUnitList?companyGroupId=' + companyGroupId + '&companyId=' + companyId + '&plantId=' + plantId + '&divisionId=' + divisionId + '&subDivisionId=' + subDivisionId, callback);
    }

    function getStoppageCbo(routeId, callback) {
        base('employees/stoppage/getcbo?routeId=' + routeId, callback);
    }

    function getRouteCbo(callback) {
        base('employees/Route/getcbo', callback);
    }

    function getCboComplianceDocumnetList(complianceDocumentCategoryId, complianceDocumentSubCategoryId, callback) {
        //IF companyId Is null then it will get companyId from Identity
        base('employees/DocumentDashboard/GetComplianceDocumentCbo?ComplianceDocumentCategoryId=' + complianceDocumentCategoryId + '&ComplianceDocumentSubCategoryId=' + complianceDocumentSubCategoryId, callback);
    }

    function getResponsiblePersonCbo(companyGroupId, callback) {
        //IF companyId Is null then it will get companyId from Identity
        if (baseService.isUndefinedOrNull(companyGroupId)) {
            if (!baseService.isUndefinedOrNull($window.companyGroupId)) {
                companyGroupId = $window.companyGroupId;
            }
            else
                companyGroupId = null;
        }
        base('employees/DocumentDashboard/GetResponsiblePersonCbo?companyGroupId=' + companyGroupId, callback);
    }

    function getCboDocumnetCategoryList(callback) {
        base('employees/DocumentDashboard/GetCascadingComplianceDocumentCategoryCbo', callback);
    }

    function getCboCascadingComplianceDocumentSubCategory(documentCategoryId, callback) {
        base('employees/DocumentDashboard/GetCascadingComplianceDocumentSubCategoryCbo?documentCategoryId=' + documentCategoryId, callback);
    }

    function getPartyCbobyPartyTypeAccountGroup(partyType, accountGroupId, callback) {
        base('Parties/party/getPartyCbobyPartyTypeAccountGroup?partyType=' + partyType + '&accountGroupId=' + accountGroupId, callback);
    }

    function getCboCompanyPartyReconAdditionalGLList(partyId, partyType, callback) {
        base('Parties/party/GetCompanyPartyReconAdditionalGLList?partyId=' + partyId + '&partyType=' + partyType, callback);
    }

    function getCboParty(callback) {
        base('Parties/party/getpartycbo', callback);
    }

    function getCboCompanyGroupPayrollGroup(callback) {
        base('Payrolls/payrollgroup/getcbo', callback);
    }

    function getCboRank(callback) {
        base('employees/recruitmentappdataedit/getrankcbolist', callback);
    }

    function getCboEmployeeTransactionType(callback) {
        base('accounts/EmployeeTransaction/GetCboEmployeeTransactionType', callback);
    }
    function getCboEmployeeAdvanceSalaryTransactionType(callback) {
        base('accounts/EmployeeTransaction/GetCboEmployeeAdvanceSalaryTransactionType', callback);
    }

    function getEmpTrnTypeByAdvanceType(advanceType, callback) {
        base('accounts/EmployeeTransaction/GetEmpTrnTypeByAdvanceType?advanceType=' + advanceType, callback);
    }

    function getCboAdvPayTranType(callback) {
        base('accounts/EmployeeTransaction/GetCboAdvPayTranType', callback);
    }
    function GetCboAssetLiabilityTranType(callback) {
        base('accounts/FinancingType/GetCboAssetLiabilityTranType', callback);
    }
    function getCboFinanceTypeForAdvanceJournal(callback) {
        base('accounts/FinancingType/GetCboFinanceTypeForAdvanceJournal', callback);
    }

    function getCboCustomerTranTypeList(callback) {
        base('accounts/FinancingType/GetCboCustomerTranTypeList', callback);
    }
    function getCboVendorTranTypeList(callback) {
        base('accounts/FinancingType/GetCboVendorTranTypeList', callback);
    }
    function GetCboExpensesBookingTransactionType(callback) {
        base('accounts/EmployeeTransaction/GetCboExpensesBookingTransactionType', callback);
    }

    function getCboInterCompanyFinancingType(sourceType, callback) {
        base('Accounts/FinancingType/GetCboInterCompanyFinancingType?sourceType=' + sourceType, callback);
    }
    function getInterCompanyAssetLiabilityType(callback) {
        base('Accounts/FinancingType/GetInterCompanyAssetLiabilityType', callback);
    }

    function getCboInterPlantFinancingType(sourceType, callback) {
        base('accounts/FinancingType/GetCboInterPlantFinancingType?sourceType=' + sourceType, callback);
    }

    function getCboOtherFinancingType(sourceType, callback) {
        base('accounts/FinancingType/GetCboOtherFinancingType?sourceType=' + sourceType, callback);
    }

    function getCboVoucherTypeEmployeeAdvanceList(callback) {
        base('accounts/VoucherTypeMatrix/GetCboVoucherTypeEmployeeAdvanceList', callback);
    }

    function getCboVoucherTypeEmployeeAdvanceWriteOffList(callback) {
        base('accounts/VoucherTypeMatrix/GetCboVoucherTypeEmployeeAdvanceWriteOffList', callback);
    }

    function getCboVoucherTypeAdvanceGivenList(callback) {
        base('accounts/VoucherTypeMatrix/GetCboVoucherTypeAdvanceGivenList', callback);
    }

    function getCboVoucherTypeAdvanceGivenWriteOffList(callback) {
        base('accounts/VoucherTypeMatrix/GetCboVoucherTypeAdvanceGivenWriteOffList', callback);
    }

    function getCboVoucherTypeAdvanceTakenWriteOffList(callback) {
        base('accounts/VoucherTypeMatrix/GetCboVoucherTypeAdvanceTakenWriteOffList', callback);
    }

    function getCboSecurityTypeGiven(callback) {
        base('accounts/SecurityDeposit/GetCboSecurityTypeGiven', callback);
    }

    function getCboSecurityTypeTaken(callback) {
        base('accounts/SecurityDeposit/GetCboSecurityTypeTaken', callback);
    }

    function getCboChartOfAccount(companyGroupId, callback) {
        if (baseService.isUndefinedOrNull(companyGroupId)) {
            if (!baseService.isUndefinedOrNull($window.companyGroupId)) {
                companyGroupId = $window.companyGroupId;
            }
            else
                companyGroupId = null;
        }
        base('Accounts/COA/GetCOACbo?companyGroupId=' + companyGroupId, callback);
    }

    function getCboRegister(callback) {
        base('accounts/Register/GetCbo', callback);
    }

    function getCboSalesOrganisationByPlant(plantId, callback) {
        base('Organizations/SalesOrganisation/GetCboByPlant?plantId=' + plantId, callback);
    }

    function getCboSalesType(callback) {
        base('Setups/SalesType/GetCbo', callback);
    }

    function getCboSalutaion(callback) {
        base('employees/salutation/getcbo', callback);
    }

    function getBudgetMasterCboByCompanyAndGLId(companyId, glId, callback) {
        base('accounts/BudgetMaster/GetBudgetMasterCboByCompanyAndGLId?companyId=' + companyId + '&glId=' + glId, callback);
    }

    function getBudgetMasterCboByCOAAndGLId(coaId, glId, callback) {
        base('accounts/BudgetMaster/GetBudgetMasterCboByCOAAndGLId?coaId=' + coaId + '&glId=' + glId, callback);
    }

    function getBudgetMasterActivityCbo(budgetMasterId, callback) {
        base('accounts/BudgetMaster/GetBudgetMasterActivityCbo?budgetMasterId=' + budgetMasterId, callback);
    }

    function GetBudgetMasterActivityLevelEmployeeCbo(budgetMasterId, level, employeeId, callback) {
        base('accounts/BudgetMaster/GetBudgetMasterActivityLevelEmployeeCbo?budgetMasterId=' + budgetMasterId + '&level=' + level + '&employeeId=' + employeeId, callback);
    }
    function GetBudgetMasterActivityLevelPotalCbo(budgetMasterId, level, employeeId, callback) {
        base('accounts/BudgetMaster/GetBudgetMasterActivityLevelPotalCbo?budgetMasterId=' + budgetMasterId + '&level=' + level + '&employeeId=' + employeeId, callback);
    }
    function GetBudgetMasterActivityLevelCbo(budgetMasterId, level, callback) {
        base('accounts/BudgetMaster/GetBudgetMasterActivityLevelCbo?budgetMasterId=' + budgetMasterId + '&level=' + level, callback);
    }

    function getCboBudgetForSetup(coaId, glId, callback) {
        base('accounts/BudgetMaster/GetCboBudgetForSetup?coaId=' + coaId + '&glId=' + glId, callback);
    }

    function getCboActivityForSetup(coaId, glId, budgetId, callback) {
        base('accounts/BudgetMaster/GetCboActivityForSetup?coaId=' + coaId + '&glId=' + glId + '&budgetId=' + budgetId, callback);
    }

    function getCboServicePartyGroupCategory(callback) {
        base('Parties/partygroupcategory/getpartygroupcategorycbo', callback);
    }

    function getCboServicePartyGroupSubCategory(callback) {
        base('Parties/partygroupsubcategory/getpartygroupsubcategorycbo', callback);
    }

    function getCboServicePartyGroupClass(callback) {
        base('Parties/partygroupclass/getpartygroupclasscbo', callback);
    }

    function getCompanyCurrency(companyId, callback) {
        base('Organizations/Company/GetCompanyCurrency?param1=' + companyId, callback);
    }

    function getCboParallelCurrency(callback) {
        base('currencies/CompanyParallelCurrency/CboParallelCurrency', callback);
    }

    function getParallelCurrency(companyId, callback) {
        base('currencies/CompanyParallelCurrency/CurrencyParallel?companyId=' + companyId, callback);
    }

    function getCompanyGroupCurrencyCbo(companyGroupId, callback) {
        if (baseService.isUndefinedOrNull(companyGroupId)) {
            if (!baseService.isUndefinedOrNull($window.companyGroupId))
                companyGroupId = $window.companyGroupId;
            else
                companyGroupId = null;
        }
        base('currencies/companygroupcurrency/getcbo?companyGroupId=' + companyGroupId, callback);
    }

    function getCurrencyCboForPotal(companyId, callback) {
        if (baseService.isUndefinedOrNull(companyId)) {
            if (!baseService.isUndefinedOrNull($window.companyId))
                companyId = $window.companyId;
            else
                companyId = null;
        }
        base('currencies/TransactionCurrency/GetCboCurrencyTransactionForPotal?companyId=' + companyId, callback);
    }

    function getCboWorkCenterMaster(callback) {
        base('WorkCenters/WorkCenterMaster/GetCbo', callback);
    }

    function getCboWorkCenterMasterByEntity(entityId, callback) {
        base('WorkCenters/WorkCenterMaster/GetCboList?entityId=' + entityId, callback);
    }

    function getCivilStatus(callback) {
        base('employees/civilstatus/getcbo', callback);
    }

    function getCboFiscalYear(entityId, callback) {
        if (baseService.isUndefinedOrNull(entityId)) {
            if (!baseService.isUndefinedOrNull($window.entityId)) {
                entityId = $window.entityId;
            }
            else
                entityId = null;
        }
        base('accounts/companyfiscalyear/getfiscalyearbyentity?entityId=' + entityId, callback);
    }

    function getCboLegalDesignation(companyGroupId, callback) {
        //IF companyId Is null then it will get companyId from Identity
        if (baseService.isUndefinedOrNull(companyGroupId)) {
            if (!baseService.isUndefinedOrNull($window.companyGroupId)) {
                companyGroupId = $window.companyGroupId;
            }
            else
                companyGroupId = null;
        }
        base('employees/RecruitmentApproval/GetLegalDesignationCbo?companyGroupId=' + companyGroupId, callback);
    }

    function getLegalDesignationCbobyGivenDesignation(givenDesignationpId, callback) {
        base('employees/RecruitmentApproval/GetLegalDesignationCbobyGivenDesignation?givenDesignationpId=' + givenDesignationpId, callback);
    }

    function getCboGivenDesignation(callback) {
        base('employees/RecruitmentApproval/GetGivenDesignationCbo', callback);
    }
    function GetSeparationType(callback) {
        base('employees/ResignationApprovalMultiple/GetCboSeparationType', callback);
    }
    function getCboLowerGivenDesignation(id, callback) {
        base('Organizations/Designation/GetLowerDesignationCbo?id=' + id, callback);
    }

    function getCboUpperGivenDesignation(id, callback) {
        base('Organizations/Designation/GetUpperDesignationCbo?id=' + id, callback);
    }

    function getCboChartOfAccountLevel6(callback) {
        base('accounts/ChartOfAccountLevel5/GetCbo', callback);
    }

    function getCboChartOfAccountLevel5(callback) {
        base('accounts/ChartOfAccountLevel5/GetCbo', callback);
    }

    function getCboChartOfAccountLevel4(callback) {
        base('accounts/ChartOfAccountLevel4/GetCbo', callback);
    }

    function getCboChartOfAccountLevel3(callback) {
        base('accounts/ChartOfAccountLevel3/GetCbo', callback);
    }

    function getCboChartOfAccountLevel2(callback) {
        base('accounts/ChartOfAccountLevel2/GetCbo', callback);
    }

    function getCboChartOfAccountLevel1(callback) {
        base('accounts/ChartOfAccountLevel1/GetCbo', callback);
    }

    function getCboBudgetGroupByCompanyGroup(companyGroupId, callback) {
        if (baseService.isUndefinedOrNull(companyGroupId)) {
            if (!baseService.isUndefinedOrNull($window.companyGroupId)) {
                companyGroupId = $window.companyGroupId;
            }
            else
                companyGroupId = null;
        }
        base('accounts/CompanyGroupBudgetGroup/GetCbo?companyGroupId=' + companyGroupId, callback);
    }

    function getCboRoutineBudgetMasterByEntityAndFY(entityId, fiscalYearId, callback) {
        base('accounts/AnnualBudget/GetCboRoutineBudget?entityId=' + entityId + '&&fiscalYearId=' + fiscalYearId, callback);
    }

    function getCboBudgetMasterForSetup(callback) {
        base('accounts/BudgetMaster/GetCboBudgetMasterForSetup', callback);
    }

    // Get position manpower budget by company and plant id.
    function getCboManpowerBudgetByCompanyAndPlant(companyId, plantId, callback) {
        if (baseService.isUndefinedOrNull(companyId)) {
            if (!baseService.isUndefinedOrNull($window.companyId)) {
                companyId = $window.companyId;
            }
            else
                companyId = null;
        }
        base('Organizations/manpowerbudget/getcbobycompanyandplant?companyId=' + companyId + '&plantid=' + plantId, callback);
    }

    function getCboRecruitmentGroupByPlant(plantId, callback) {
        base('employees/recruitmentgroup/getcbo?plantId=' + plantId, callback);
    }

    function getCboRecruitmentProcessSetByCompanyGroup(companyGroupId, callback) {
        if (baseService.isUndefinedOrNull(companyGroupId)) {
            if (!baseService.isUndefinedOrNull($window.companyGroupId)) {
                companyGroupId = $window.companyGroupId;
            }
            else
                companyGroupId = null;
        }
        base('employees/recruitmentprocessset/getcbo?companyGroupId=' + companyGroupId, callback);
    }

    // Get voucher type Matrix cbo list.

    function getCboVoucherTypeEmployeePayableList(callback) {
        base('accounts/VoucherTypeMatrix/GetCboVoucherTypeEmployeePayableList', callback);
    }

    function getCboVoucherTypeSalaryPayableList(callback) {
        base('accounts/VoucherTypeMatrix/GetCboVoucherTypeSalaryPayableList', callback);
    }
    function getCboVoucherTypeFinalSettlementDisbursementList(callback) {
        base('accounts/VoucherTypeMatrix/GetCboVoucherTypeFinalSettlementDisbursementList', callback);
    }
    function getCboVoucherTypeSalaryDisbursementList(callback) {
        base('accounts/VoucherTypeMatrix/GetCboVoucherTypeSalaryDisbursementList', callback);
    }
    function getCboVoucherTypeGoodWorkDisbursementList(callback) {
        base('accounts/VoucherTypeMatrix/GetCboVoucherTypeGoodWorkDisbursementList', callback);
    }
    function getCboVoucherTypeBonusDisbursementList(callback) {
        base('accounts/VoucherTypeMatrix/GetCboVoucherTypeBonusDisbursementList', callback);
    }

    function getCboVoucherTypeAccountReceivableList(callback) {
        base('accounts/VoucherTypeMatrix/GetCboVoucherTypeAccountReceivableList', callback);
    }

    function getCboVoucherTypeReceiptList(callback) {
        base('accounts/VoucherTypeMatrix/GetCboVoucherTypeReceiptList', callback);
    }

    function getCboVoucherTypeBanksReceiptList(callback) {
        base('accounts/VoucherTypeMatrix/GetCboVoucherTypeBanksReceiptList', callback);
    }
    function getCboVoucherTypeSuspensePayableList(callback) {
        base('accounts/VoucherTypeMatrix/GetCboVoucherTypeSuspensePayableList', callback);
    }

    function getCboVoucherTypeAccountPayableList(callback) {
        base('accounts/VoucherTypeMatrix/GetCboVoucherTypeAccountPayableList', callback);
    }
    function getCboVoucherTypeFGInventoryList(callback) {
        base('accounts/VoucherTypeMatrix/GetCboVoucherTypeFGInventoryList', callback);
    }
    function getCboVoucherTypePostInvoiceList(callback) {
        base('accounts/VoucherTypeMatrix/GetCboVoucherTypePostInvoiceList', callback);
    }

    function getCboVoucherTypeReceivableFromOthersList(callback) {
        base('accounts/VoucherTypeMatrix/GetCboVoucherTypeReceivableFromOthersList', callback);
    }
    function getCboVoucherTypeOutSourceBillingList(callback) {
        base('accounts/VoucherTypeMatrix/GetCboVoucherTypeOutSourceBillingList', callback);
    }

    function getCboVoucherTypePackingJournalList(callback) {
        base('accounts/VoucherTypeMatrix/GetCboVoucherTypePackingJournalList', callback);
    }
    function getCboVoucherTypePuechaseDocumentAcceptanceList(callback) {
        base('accounts/VoucherTypeMatrix/GetCboVoucherTypePuechaseDocumentAcceptanceList', callback);
    }
    function getCboVoucherTypePuechaseLCOpeningChargesList(callback) {
        base('accounts/VoucherTypeMatrix/GetCboVoucherTypePuechaseLCOpeningChargesList', callback);
    }
    function getCboVoucherTypeIssueJournalList(callback) {
        base('accounts/VoucherTypeMatrix/GetCboVoucherTypeIssueJournalList', callback);
    }
    function getCboVoucherTypeIssueReturnJournalList(callback) {
        base('accounts/VoucherTypeMatrix/GetCboVoucherTypeIssueReturnJournalList', callback);
    }

    function getCboVoucherTypeFixedAssetCapitalizeJournalList(callback) {
        base('accounts/VoucherTypeMatrix/GetCboVoucherTypeFixedAssetCapitalizeJournalList', callback);
    }
    function getCboVoucherTypeFixedAssetDepreciationJournalList(callback) {
        base('accounts/VoucherTypeMatrix/GetCboVoucherTypeFixedAssetDepreciationJournalList', callback);
    }
    function getCboVoucherTypeFixedAssetDisposeJournalList(callback) {
        base('accounts/VoucherTypeMatrix/GetCboVoucherTypeFixedAssetDisposeJournalList', callback);
    }
    function getCboVoucherTypePaymentList(callback) {
        base('accounts/VoucherTypeMatrix/GetCboVoucherTypePaymentList', callback);
    }
    function getCboVoucherTypeFiscalYearCloseJournalList(callback) {
        base('accounts/VoucherTypeMatrix/GetCboVoucherTypeFiscalYearCloseJournalList', callback);
    }

    function getCboVoucherTypePartyReconcilliationList(callback) {
        base('accounts/VoucherTypeMatrix/GetCboVoucherTypePartyReconcilliationList', callback);
    }


    function getCboVoucherTypeEmployeePaymentList(callback) {
        base('accounts/VoucherTypeMatrix/GetCboVoucherTypeEmployeePaymentList', callback);
    }

    function getCboVoucherTypeAdvanceTakenList(callback) {
        base('accounts/VoucherTypeMatrix/GetCboVoucherTypeAdvanceTakenList', callback);
    }
    function getCboVoucherTypeInterTransactionList(callback) {
        base('accounts/VoucherTypeMatrix/GetCboVoucherTypeInterTransactionList', callback);
    }

    function getCboVoucherTypeCustomerSuspense(callback) {
        base('accounts/VoucherTypeMatrix/GetCboVoucherTypeCustomerSuspense', callback);
    }

    function getCboVoucherTypeSecurityTakenList(callback) {
        base('accounts/VoucherTypeMatrix/GetCboVoucherTypeSecurityTakenList', callback);
    }

    function getCboVoucherTypeSecurityTakenWriteOffList(callback) {
        base('accounts/VoucherTypeMatrix/GetCboVoucherTypeSecurityTakenWriteOffList', callback);
    }

    function getCboVoucherTypeSecurityGivenList(callback) {
        base('accounts/VoucherTypeMatrix/GetCboVoucherTypeSecurityGivenList', callback);
    }

    function getCboVoucherTypeSecurityGivenWriteOffList(callback) {
        base('accounts/VoucherTypeMatrix/GetCboVoucherTypeSecurityGivenWriteOffList', callback);
    }

    function getCboVoucherTypeOpeningBalanceList(callback) {
        base('accounts/VoucherTypeMatrix/GetCboVoucherTypeOpeningBalanceList', callback);
    }

    function getCboVoucherTypeCreditNoteList(callback) {
        base('accounts/VoucherTypeMatrix/GetCboVoucherTypeCreditNoteList', callback);
    }

    function getCboVoucherTypeInventoryReturnPayableList(callback) {
        base('accounts/VoucherTypeMatrix/GetCboVoucherTypeInventoryReturnPayableList', callback);
    }

    function getCboVoucherTypeSalesReturnList(callback) {
        base('accounts/VoucherTypeMatrix/GetCboVoucherTypeSalesReturnList', callback);
    }

    function getCboVoucherTypeDebitNoteList(callback) {
        base('accounts/VoucherTypeMatrix/GetCboVoucherTypeDebitNoteList', callback);
    }

    function getCboVoucherTypeTaxPaymentList(callback) {
        base('accounts/VoucherTypeMatrix/GetCboVoucherTypeTaxPaymentList', callback);
    }
    function getCboVoucherType(callback) {
        base('accounts/VoucherType/getvouchertypecbo', callback);
    }

    function getCboPositionByEntityId(entityId, callback) {
        base('Organizations/Position/getcbobyentity?entityid=' + entityId, callback);
    }

    // Get role by company group Id.
    function getCboRoleByCompanyGroup(companyGroupId, callback) {
        if (baseService.isUndefinedOrNull(companyGroupId)) {
            if (!baseService.isUndefinedOrNull($window.companyGroupId)) {
                companyGroupId = $window.companyGroupId;
            }
            else
                companyGroupId = null;
        }
        base('Securities/role/getrolebycompanygroup?companyGroupId=' + companyGroupId, callback);
    }

    // Get position realation data by company group Id.
    function getCboPositionByCompanyGroup(companyGroupId, callback) {
        if (baseService.isUndefinedOrNull(companyGroupId)) {
            if (!baseService.isUndefinedOrNull($window.companyGroupId)) {
                companyGroupId = $window.companyGroupId;
            }
            else
                companyGroupId = null;
        }
        base('Organizations/Position/getcbobycompanygroup?companyGroupId=' + companyGroupId, callback);
    }

    // Get vendor by company Id.
    function getCboVendorByCompany(companyId, callback) {
        if (baseService.isUndefinedOrNull(companyId)) {
            if (!baseService.isUndefinedOrNull($window.companyId)) {
                companyId = $window.companyId;
            }
            else
                companyId = null;
        }
        base('Parties/party/getvendorcbobycompany?companyId=' + companyId, callback);
    }

    // Get designation by companyGroupId.
    function getCboDesignationByCompanyGroup(companyGroupId, callback) {
        if (baseService.isUndefinedOrNull(companyGroupId)) {
            if (!baseService.isUndefinedOrNull($window.companyGroupId)) {
                companyGroupId = $window.companyGroupId;
            }
            else
                companyGroupId = null;
        }
        base('Organizations/designation/getcbobycompanygroup?companyGroupId=' + companyGroupId, callback);
    }

    // Get Salutation by companyGroupId.
    function getCboSalutationByCompanyGroup(companyGroupId, callback) {
        if (baseService.isUndefinedOrNull(companyGroupId)) {
            if (!baseService.isUndefinedOrNull($window.companyGroupId)) {
                companyGroupId = $window.companyGroupId;
            }
            else
                companyGroupId = null;
        }
        base('employees/salutation/getcbo?companyGroupId=' + companyGroupId, callback);
    }

    // Get RecruitmentProcessSet cbo list.
    function getCboRecruitmentProcess(callback) {
        base('employees/recruitmentprocess/getcbo', callback);
    }

    // Get Language cbo list.
    function getCboLanguage(callback) {
        base('Setups/language/getcbo', callback);
    }

    // Get ShiftDefination by plantId.
    function getCboShiftDefinationByPlant(plantId, callback) {
        base('attendances/shiftdefination/getcbobyplant?plantid=' + plantId, callback);
    }

    function getRoasterCboByPlant(plantId, callback) {
        base('humanresource/employeeshiftassign/getroastercbobyplant?plantid=' + plantId, callback);
    }

    function getRosterWiseShiftCbo(plantId, roasterId, callback) {
        base('humanresource/employeeshiftassign/getrosterwiseshiftname?plantId=' + plantId + '&roasterId=' + roasterId, callback);
    }

    function getCboLineByCompanyGroup(companyGroupId, callback) {
        if (baseService.isUndefinedOrNull(companyGroupId)) {
            if (!baseService.isUndefinedOrNull($window.companyGroupId)) {
                companyGroupId = $window.companyGroupId;
            }
            else
                companyGroupId = null;
        }
        base('Organizations/Line/GetCboByCompanyGroup?companyGroupId=' + companyGroupId, callback);
    }

    // Get line by companyId.
    function getCboLineByCompany(companyId, callback) {
        if (baseService.isUndefinedOrNull(companyId)) {
            if (!baseService.isUndefinedOrNull($window.companyId)) {
                companyId = $window.companyId;
            }
            else
                companyId = null;
        }
        base('Organizations/line/getcbobycompany?companyId=' + companyId, callback);
    }

    // Get designation group by companyGroupId.
    function getCboDesignationGroupByCompanyGroup(companyGroupId, callback) {
        if (baseService.isUndefinedOrNull(companyGroupId)) {
            if (!baseService.isUndefinedOrNull($window.companyGroupId)) {
                companyGroupId = $window.companyGroupId;
            }
            else
                companyGroupId = null;
        }
        base('Organizations/designationgroup/getcbobycompanygroup?companyGroupId=' + companyGroupId, callback);
    }

    // Get sub section by companyGroupId.
    function getCboSubSectionByCompanyGroup(companyGroupId, callback) {
        if (baseService.isUndefinedOrNull(companyGroupId)) {
            if (!baseService.isUndefinedOrNull($window.companyGroupId)) {
                companyGroupId = $window.companyGroupId;
            }
            else
                companyGroupId = null;
        }
        base('Organizations/subsection/getcbobycompanygroup?companyGroupId=' + companyGroupId, callback);
    }

    // Get sub section by companyId.
    function getCboSubSectionByCompany(companyId, callback) {
        if (baseService.isUndefinedOrNull(companyId)) {
            if (!baseService.isUndefinedOrNull($window.companyId)) {
                companyId = $window.companyId;
            }
            else
                companyId = null;
        }
        base('Organizations/subsection/getcbobycompany?companyId=' + companyId, callback);
    }

    // Get section by companyGroupId.
    function getCboSectionByCompanyGroup(companyGroupId, callback) {
        if (baseService.isUndefinedOrNull(companyGroupId)) {
            if (!baseService.isUndefinedOrNull($window.companyGroupId)) {
                companyGroupId = $window.companyGroupId;
            }
            else
                companyGroupId = null;
        }
        base('Organizations/section/getcbobycompanygroup?companyGroupId=' + companyGroupId, callback);
    }

    // Get section by companyId.
    function getCboSectionByCompany(companyId, callback) {
        if (baseService.isUndefinedOrNull(companyId)) {
            if (!baseService.isUndefinedOrNull($window.companyId)) {
                companyId = $window.companyId;
            }
            else
                companyId = null;
        }
        base('Organizations/section/getcbobycompany?companyId=' + companyId, callback);
    }

    // Get department by companyGroupId.
    function getCboDepartmentByCompanyGroup(companyGroupId, callback) {
        if (baseService.isUndefinedOrNull(companyGroupId)) {
            if (!baseService.isUndefinedOrNull($window.companyGroupId)) {
                companyGroupId = $window.companyGroupId;
            }
            else
                companyGroupId = null;
        }
        base('Organizations/department/getcbobycompanygroup?companyGroupId=' + companyGroupId, callback);
    }

    // Get all department cbo list.
    function getCboDepartment(callback) {
        base('Organizations/department/getcbo', callback);
    }

    // Get all designation cbo list.
    function getCboDesignation(callback) {
        base('employees/employeeinformation/cbolist', callback);
    }

    // Get department by companyId.
    function getCboDepartmentByCompany(companyId, callback) {
        if (baseService.isUndefinedOrNull(companyId)) {
            if (!baseService.isUndefinedOrNull($window.companyId)) {
                companyId = $window.companyId;
            }
            else
                companyId = null;
        }
        base('Organizations/department/getcbobycompany?companyId=' + companyId, callback);
    }

    function getCboSubDivisionByCompanyGroup(companyGroupId, callback) {
        if (baseService.isUndefinedOrNull(companyGroupId)) {
            if (!baseService.isUndefinedOrNull($window.companyGroupId)) {
                companyGroupId = $window.companyGroupId;
            }
            else
                companyGroupId = null;
        }
        base('Organizations/subdivision/getcbobycompanygroup?companyGroupId=' + companyGroupId, callback);
    }

    // Get sub division by companyId.
    function getCboSubDivisionByCompany(companyId, callback) {
        if (baseService.isUndefinedOrNull(companyId)) {
            if (!baseService.isUndefinedOrNull($window.companyId)) {
                companyId = $window.companyId;
            }
            else
                companyId = null;
        }
        base('Organizations/subdivision/getcbobycompany?companyId=' + companyId, callback);
    }

    function getCboDivisionByCompanyGroup(companyGroupId, callback) {
        if (baseService.isUndefinedOrNull(companyGroupId)) {
            if (!baseService.isUndefinedOrNull($window.companyGroupId)) {
                companyGroupId = $window.companyGroupId;
            }
            else
                companyGroupId = null;
        }
        base('Organizations/division/getcbobycompanygroup?companyGroupId=' + companyGroupId, callback);
    }

    // Get division by companyId.
    function getCboDivisionByCompany(companyId, callback) {
        if (baseService.isUndefinedOrNull(companyId)) {
            if (!baseService.isUndefinedOrNull($window.companyId)) {
                companyId = $window.companyId;
            }
            else
                companyId = null;
        }
        base('Organizations/division/getcbobycompany?companyId=' + companyId, callback);
    }

    // Get employee group by companyGroupId.
    function getCboEmployeeGroupByCompanyGroup(companyGroupId, callback) {
        if (baseService.isUndefinedOrNull(companyGroupId)) {
            if (!baseService.isUndefinedOrNull($window.companyGroupId)) {
                companyGroupId = $window.companyGroupId;
            }
            else
                companyGroupId = null;
        }
        base('employees/employeegroup/getcbobycompanygroup?companyGroupId=' + companyGroupId, callback);
    }

    // Get employee group by companyGroupId.
    function getCboEmployeeCategoryGroupByCompanyGroup(companyGroupId, callback) {
        if (baseService.isUndefinedOrNull(companyGroupId)) {
            if (!baseService.isUndefinedOrNull($window.companyGroupId)) {
                companyGroupId = $window.companyGroupId;
            }
            else
                companyGroupId = null;
        }
        base('Organizations/CompanyGroupEmployeeCategory/GetCbo?companyGroupId=' + companyGroupId, callback);
    }

    // Get all enums list.
    function getEnumCbo(url, callback) {
        base(url, callback);
    }

    // Get all sequence list.
    function getSequence(url, callback) {
        base(url, callback);
    }

    // Get all activity cbo list.
    function getCboActivity(callback) {
        base('accounts/activity/getcbo', callback);
    }

    function getCboActivityPhone(callback) {
        base('accounts/activity/GetCboActivityPhone', callback);
    }

    // Get activity list by company group id
    function getCboActivityCompanyGroup(companyGroupId, callback) {
        if (baseService.isUndefinedOrNull(companyGroupId)) {
            if (!baseService.isUndefinedOrNull($window.companyGroupId)) {
                companyGroupId = $window.companyGroupId;
            }
            else
                companyGroupId = null;
        }
        base('accounts/companygroupactivity/getcbo?companyGroupId=' + companyGroupId, callback);
    }

    // Get all module cbo list.
    function getCboModule(callback) {
        base('Modules/module/getcbo', callback);
    }

    // Get module list by company group id.
    function getCboModuleByCompanyGroup(companyGroupId, callback) {
        if (baseService.isUndefinedOrNull(companyGroupId)) {
            if (!baseService.isUndefinedOrNull($window.companyGroupId)) {
                companyGroupId = $window.companyGroupId;
            }
            else
                companyGroupId = null;
        }
        base('Modules/module/getcbobycompanygroup?companyGroupId=' + companyGroupId, callback);
    }

    // Get all sub module cbo list.
    function getCboSubModule(callback) {
        base('Modules/submodule/getcbo', callback);
    }

    // Get sub module cbo list by module.
    function getCboSubModuleByModule(moduleId, callback) {
        base('Modules/submodule/getcbobymodule?moduleId=' + moduleId, callback);
    }

    // Get employee activity list.
    function getCboActivityByEmployee(employeeId, callback) {
        base('accounts/BudgetMaster/getactivitycbobyemployee?employeeId=' + employeeId, callback);
    }

    // Get employee activity phone list.
    function getCboActivityPhoneByEmployeeActivity(employeeId, budgetId, activityId, callback) {
        base('accounts/BudgetMaster/GetCboEmployeeBudgetActivityPhoneList?employeeId=' + employeeId + '&budgetId=' + budgetId + '&activityId=' + activityId, callback);
    }

    function getPlantShiftCbo(plantId, callback) {
        base('attendances/shiftdefination/getcbo?plantid=' + plantId, callback);
    }

    // Shift by entity structure data id inside Plant id.
    function getEntityPlantShiftCbo(entityId, callback) {
        base('attendances/shiftdefination/getentityplantshiftcbo?entityId=' + entityId, callback);
    }

    function getCboEmployeeBudgetList(employeeId, callback) {
        base('accounts/budgetmaster/getCboEmployeeBudgetList?employeeId=' + employeeId, callback);
    }

    function getCboEmployeeBudgetActivityList(employeeId, budgetMasterId, callback) {
        base('accounts/budgetmaster/GetCboEmployeeBudgetActivityList?employeeId=' + employeeId + '&budgetMasterId=' + budgetMasterId, callback);
    }

    function base(url, callback) {
        $http.get(url)
            .then(function successCallback(response) {
                callback(response.data);
            }, function errorCallback(response) {
                ShowResult(response, 'failure');
            });
    }

    function getBudgetCboByGL(glgeneralInfoId, callback) {
        base('accounts/budgetmaster/getbudgetcbobygl?glgeneralInfoId=' + glgeneralInfoId, callback);
    }

    function getCboBudgetByEmployeeActivity(employeeId, activityId, callback) {
        base('accounts/budgetmaster/getbudgetcbobyemployeeactivity?employeeId=' + employeeId + '&activityId=' + activityId, callback);
    }

    // Get all unit.
    function getCboUnit(callback) {
        base('Organizations/unit/getcbo', callback);
    }

    // Get unit by companyGroupId.
    function getCboUnitByCompanyGroup(companyGroupId, callback) {
        if (baseService.isUndefinedOrNull(companyGroupId)) {
            if (!baseService.isUndefinedOrNull($window.companyGroupId)) {
                companyGroupId = $window.companyGroupId;
            }
            else
                companyGroupId = null;
        }
        base('Organizations/unit/getcbobycompanygroup?companyGroupId=' + companyGroupId, callback);
    }

    // Get unit by companyId.
    function getCboUnitByCompany(companyId, callback) {
        if (baseService.isUndefinedOrNull(companyId)) {
            if (!baseService.isUndefinedOrNull($window.companyId)) {
                companyId = $window.companyId;
            }
            else
                companyId = null;
        }
        base('Organizations/unit/getcbobycompany?companyId=' + companyId, callback);
    }

    // Get company by companyGroupId.
    function getCboCompanyByCompanyGroup(companyGroupId, callback) {
        if (baseService.isUndefinedOrNull(companyGroupId)) {
            if (!baseService.isUndefinedOrNull($window.companyGroupId)) {
                companyGroupId = $window.companyGroupId;
            }
            else
                companyGroupId = null;
        }
        base('Organizations/Company/GetCboByCompanyGroup?companyGroupId=' + companyGroupId, callback);
    }

    function getCboCompanyByCompanyGroupWithAddressMaster(companyGroupId, callback) {
        if (baseService.isUndefinedOrNull(companyGroupId)) {
            if (!baseService.isUndefinedOrNull($window.companyGroupId)) {
                companyGroupId = $window.companyGroupId;
            }
            else
                companyGroupId = null;
        }
        base('Parties/InterCompanyParty/GetCboCompanyByCompanyGroupWithAddressMaster?companyGroupId=' + companyGroupId, callback);
    }

    // Get company by companyGroupId.
    function getCboInterCompany(companyGroupId, callback) {
        if (baseService.isUndefinedOrNull(companyGroupId)) {
            if (!baseService.isUndefinedOrNull($window.companyGroupId)) {
                companyGroupId = $window.companyGroupId;
            }
            else
                companyGroupId = null;
        }
        base('Organizations/Company/GetCboInterCompany?companyGroupId=' + companyGroupId, callback);
    }

    // Get company by coaId.
    function getCboCompanyByCOA(coaId, callback) {
        base('Organizations/company/getcbobycoa?coaId=' + coaId, callback);
    }

    // Get all plant.
    function getCboPlant(callback) {
        base('Organizations/plant/getcbo', callback);
    }

    //Get all Brand List
    function getCboBrand(callback) {
        base('Setups/brand/getcbo', callback);
    }

    // Get plant by companyGroupId.
    function getCboPlantByCompanyGroup(companyGroupId, callback) {
        if (baseService.isUndefinedOrNull(companyGroupId)) {
            if (!baseService.isUndefinedOrNull($window.companyGroupId)) {
                companyGroupId = $window.companyGroupId;
            }
            else
                companyGroupId = null;
        }
        base('Organizations/plant/getcbobycompanygroup?companyGroupId=' + companyGroupId, callback);
    }

    // Get plant by companyId.
    function getCboPlantByCompany(companyId, callback) {
        if (baseService.isUndefinedOrNull(companyId)) {
            if (!baseService.isUndefinedOrNull($window.companyId)) {
                companyId = $window.companyId;
            }
            else
                companyId = null;
        }
        base('Organizations/plant/GetCboByCompany?companyId=' + companyId, callback);
    }

    function getCboCompanyGroup(callback) {
        base('Organizations/companygroup/getcbo', callback);
    }

    function getCompanyGroupCompanyCbo(companyGroupId, callback) {
        if (baseService.isUndefinedOrNull(companyGroupId)) {
            if (!baseService.isUndefinedOrNull($window.companyGroupId)) {
                companyGroupId = $window.companyGroupId;
            }
            else
                companyGroupId = null;
        }
        base('Organizations/company/getcbobycompanygroup?companyGroupId=' + companyGroupId, callback);
    }

    // Line by company id
    function getCompanyLineCbo(companyId, callback) {
        base('Organizations/companyline/getcbo?companyId=' + companyId, callback);
    }

    // #region Entity
    function getEntityCompanyLineCbo(entityId, callback) {
        base('Organizations/companyline/getentitycompanylinecbo?entityId=' + entityId, callback);
    }

    function getCboEntityLineById(entityId, callback) {
        base('Organizations/entityline/getcboentitylinebyid?entityId=' + entityId, callback);
    }

    function getCboEntityWithPlant(companyGroupId, companyId, callback) {
        if (baseService.isUndefinedOrNull(companyGroupId)) {
            if (!baseService.isUndefinedOrNull($window.companyGroupId)) {
                companyGroupId = $window.companyGroupId;
            }
            else
                companyGroupId = null;
        }
        if (baseService.isUndefinedOrNull(companyId)) {
            if (!baseService.isUndefinedOrNull($window.companyId)) {
                companyId = $window.companyId;
            }
            else
                companyId = null;
        }
        base('Organizations/entity/GetCboByCompany?companyGroupId=' + companyGroupId + '&companyId=' + companyId, callback);
    }

    function getCboInterEntityWithPlant(companyGroupId, companyId, id, callback) {
        if (baseService.isUndefinedOrNull(companyGroupId)) {
            if (!baseService.isUndefinedOrNull($window.companyGroupId)) {
                companyGroupId = $window.companyGroupId;
            }
            else
                companyGroupId = null;
        }
        if (baseService.isUndefinedOrNull(companyId)) {
            if (!baseService.isUndefinedOrNull($window.companyId)) {
                companyId = $window.companyId;
            }
            else
                companyId = null;
        }
        base('Organizations/entity/GetCboInterEntity?companyGroupId=' + companyGroupId + '&companyId=' + companyId + '&Id=' + id, callback);
    }

    function getCboInterPlant(companyGroupId, companyId, plantId, callback) {
        if (baseService.isUndefinedOrNull(companyGroupId)) {
            if (!baseService.isUndefinedOrNull($window.companyGroupId)) {
                companyGroupId = $window.companyGroupId;
            }
            else
                companyGroupId = null;
        }
        if (baseService.isUndefinedOrNull(companyId)) {
            if (!baseService.isUndefinedOrNull($window.companyId)) {
                companyId = $window.companyId;
            }
            else
                companyId = null;
        }
        base('Organizations/Plant/GetCboInterPlant?companyGroupId=' + companyGroupId + '&companyId=' + companyId + '&plantId=' + plantId, callback);
    }

    function getCboEntityPlantWise(companyGroupId, companyId, plantId, callback) {
        if (baseService.isUndefinedOrNull(companyGroupId)) {
            if (!baseService.isUndefinedOrNull($window.companyGroupId)) {
                companyGroupId = $window.companyGroupId;
            }
            else
                companyGroupId = null;
        }
        if (baseService.isUndefinedOrNull(companyId)) {
            if (!baseService.isUndefinedOrNull($window.companyId)) {
                companyId = $window.companyId;
            }
            else
                companyId = null;
        }
        if (baseService.isUndefinedOrNull(plantId)) {
            if (!baseService.isUndefinedOrNull($window.plantId)) {
                plantId = $window.plantId;
            }
            else
                companyId = null;
        }
        base('Organizations/Entity/GetCboByPlant?companyGroupId=' + companyGroupId + '&companyId=' + companyId + '&plantId=' + plantId, callback);
    }

    function getCboEntityCompanyWise(companyGroupId, companyId, callback) {
        if (baseService.isUndefinedOrNull(companyGroupId)) {
            if (!baseService.isUndefinedOrNull($window.companyGroupId)) {
                companyGroupId = $window.companyGroupId;
            }
            else
                companyGroupId = null;
        }
        base('Organizations/Entity/GetCboByCompany?companyGroupId=' + companyGroupId + '&companyId=' + companyId, callback);
    }

    function getCboWithEmployee(companyGroupId, companyId, callback) {
        if (baseService.isUndefinedOrNull(companyGroupId)) {
            if (!baseService.isUndefinedOrNull($window.companyGroupId)) {
                companyGroupId = $window.companyGroupId;
            }
            else
                companyGroupId = null;
        }
        if (baseService.isUndefinedOrNull(companyId)) {
            if (!baseService.isUndefinedOrNull($window.companyId)) {
                companyId = $window.companyId;
            }
            else
                companyId = null;
        }
        base('Organizations/entity/GetCboWithEmployee?companyGroupId=' + companyGroupId + '&companyId=' + companyId, callback);
    }

    function getCboEntityByCompanyWise(companyGroupId, companyId, callback) {
        if (baseService.isUndefinedOrNull(companyGroupId)) {
            if (!baseService.isUndefinedOrNull($window.companyGroupId))
                companyGroupId = $window.companyGroupId;
            else
                companyGroupId = null;
        }
        if (baseService.isUndefinedOrNull(companyId)) {
            if (!baseService.isUndefinedOrNull($window.companyId)) {
                companyId = $window.companyId;
            }
            else
                companyId = null;
        }
        base('Organizations/entity/GetCboByCompany?companyGroupId=' + companyGroupId + '&companyId=' + companyId, callback);
    }

    function getCboEntityByPlant(companyGroupId, companyId, plantId, callback) {
        if (baseService.isUndefinedOrNull(companyGroupId)) {
            if (!baseService.isUndefinedOrNull($window.companyGroupId))
                companyGroupId = $window.companyGroupId;
            else
                companyGroupId = null;
        }
        if (baseService.isUndefinedOrNull(companyId)) {
            if (!baseService.isUndefinedOrNull($window.companyId)) {
                companyId = $window.companyId;
            }
            else
                companyId = null;
        }
        base('Organizations/entity/GetCboByPlant?companyGroupId=' + companyGroupId + '&companyId=' + companyId + '&plantId=' + plantId, callback);
    }

    function getEntityCboByPlant(companyGroupId, companyId, plantId, callback) {
        if (baseService.isUndefinedOrNull(companyGroupId)) {
            if (!baseService.isUndefinedOrNull($window.companyGroupId))
                companyGroupId = $window.companyGroupId;
            else
                companyGroupId = null;
        }
        if (baseService.isUndefinedOrNull(companyId)) {
            if (!baseService.isUndefinedOrNull($window.companyId)) {
                companyId = $window.companyId;
            }
            else
                companyId = null;
        }
        base('Organizations/entity/GetEntityCboByPlant?companyGroupId=' + companyGroupId + '&companyId=' + companyId + '&plantId=' + plantId, callback);
    }

    function getCboProductionEntitiesByPlant(plantId, callback) {
        base('Materials/MachineBudget/GetProductionEntityCbo?plantId=' + plantId, callback);
    }

    function getEntityByUser(callback) {

        base('Organizations/entity/GetEntityByUser', callback);
    }
    function getEntityByGeneralUser(callback) {

        base('Organizations/entity/GetEntityByGeneralUser', callback);
    }

    function getCboEntityExceptionByCompany(companyId, callback) {
        base('Organizations/entity/getexceptioncbobycompany?companyId=' + companyId, callback);
    }

    function getCboEntityCostCenter(callback) {
        base('Organizations/CompanyCostCenter/GetCboList', callback);
    }

    function getCboEntityByCostCenter(costCenterId, callback) {
        base('Organizations/EntityCostCenter/GetEntityById?costCenterId=' + costCenterId, callback);
    }

    function getCboEntityType(callback) {
        base('Enum/GetCboEntityType', callback);
    }

    function getCboEntityByCompanyGroup(companyGroupId, callback) {
        if (baseService.isUndefinedOrNull(companyGroupId)) {
            if (!baseService.isUndefinedOrNull($window.companyGroupId)) {
                companyGroupId = $window.companyGroupId;
            }
            else
                companyGroupId = null;
        }
        base('Organizations/entity/getcbobycompanygroup?companyGroupId=' + companyGroupId, callback);
    }

    function getCboEntityAndPositionRelationshipByCompanyGroupAndCompany(companyGroupId, companyId, callback) {
        if (baseService.isUndefinedOrNull(companyGroupId)) {
            if (!baseService.isUndefinedOrNull($window.companyGroupId)) {
                companyGroupId = $window.companyGroupId;
            }
            else
                companyGroupId = null;
        }
        if (baseService.isUndefinedOrNull(companyId)) {
            if (!baseService.isUndefinedOrNull($window.companyId)) {
                companyId = $window.companyId;
            }
            else
                companyId = null;
        }
        base('Organizations/positionrelationship/getentityandpositionrelationship?companyGroupId=' + companyGroupId + '&companyId=' + companyId, callback);
    }

    function GetEntityProcessCbo(entityId, callback) {
        base('Processes/EntityProcessTag/GetEntityProcessCbo?entityid=' + entityId, callback);
    }
    function GetEntityProductionProcessCbo(entityId, callback) {
        base('Productions/RecipeGlobalMaster/GetEntityProductionProcessCbo?entityid=' + entityId, callback);
    }

    function GetWCProcessCbo(processid, entityId, shiftId, callback) {
        base('Productions/ProductionSummary/GetWCProcessCbo?processid=' + processid + '&entityId=' + entityId + '&shiftId=' + shiftId, callback);
    }
    function GetToWCProcessCbo(processid, entityId, callback) {
        base('Productions/ProductionSummary/GetToWCProcessCbo?processid=' + processid + '&entityId=' + entityId, callback);
    }

    function GetProductionShiftCbo(callback) {
        base('Productions/ProductionSummary/GetShiftGroupCbo', callback);
    }

    function getCboProductionEntityByCompany(companyGroupId, companyId, callback) {
        if (baseService.isUndefinedOrNull(companyGroupId)) {
            if (!baseService.isUndefinedOrNull($window.companyGroupId)) {
                companyGroupId = $window.companyGroupId;
            }
            else
                companyGroupId = null;
        }
        base('Organizations/entitycomponentcosting/getcboproduction?companyGroupId=' + companyGroupId + '&companyId=' + companyId, callback);
    }

    function getCboProductionEntityByPlant(companyGroupId, companyId, plantId, callback) {
        if (baseService.isUndefinedOrNull(companyGroupId)) {
            if (!baseService.isUndefinedOrNull($window.companyGroupId)) companyGroupId = $window.companyGroupId;
            else companyGroupId = null;
        }
        if (baseService.isUndefinedOrNull(companyId)) {
            if (!baseService.isUndefinedOrNull($window.companyId)) companyId = $window.companyId;
            else companyId = null;
        }
        base('Organizations/EntityComponentCosting/GetCboProductionByPlant?companyGroupId=' + companyGroupId + '&companyId=' + companyId + '&plantId=' + plantId, callback);
    }

    function getCboProductionEntityByCompanyGroup(companyGroupId, callback) {
        if (baseService.isUndefinedOrNull(companyGroupId)) {
            if (!baseService.isUndefinedOrNull($window.companyGroupId)) {
                companyGroupId = $window.companyGroupId;
            }
            else
                companyGroupId = null;
        }
        base('setups/entityconfig/getcboproductionbycompanygroup?companyGroupId=' + companyGroupId, callback);
    }

    // #endregion Entity

    function getShipModeCbo(callback) {
        base('OrderManagements/shipmode/getcbo', callback);
    }

    function getFixedAssetList(callback) {
        base('fixedassets/fixedasset/getfixedassetlist', callback);
    }

    function getFixedAssetClassList(callback) {
        base('fixedassets/fixedassetclass/getcbo', callback);
    }

    function getFixedAssetSubClassList(callback) {
        base('fixedassets/fixedassetsubclass/getcbo', callback);
    }

    function getFixedAssetCategoryList(callback) {
        base('fixedassets/fixedassetcategory/getfixedassetcategorylist', callback);
    }

    function getFixedAssetSubCategoryList(callback) {
        base('fixedassets/fixedassetsubcategory/getfixedassetsubcategorylist', callback);
    }

    function getFixedAssetItemList(callback) {
        base('fixedassets/fixedassetregister/getcbo', callback);
    }

    function getFixedAssetMasterList(callback) {
        base('fixedassets/fixedassetmaster/getcbo', callback);
    }

    function getSubAssetTypeList(callback) {
        base('fixedassets/SubAssetType/getcbo', callback);
    }

    function jobDescriptionCategoryList(callback) {
        base('employees/jobdescriptioncategory/getcbo', callback);
    }

    function jobDescriptionSubCategoryList(callback) {
        base('employees/jobdescriptionsubcategory/getcbo', callback);
    }

    function jobDescriptionItemList(callback) {
        base('employees/jobdescriptionitem/getcbo', callback);
    }

    function loadUtilityCbo(callback) {
        base('Processes/utility/getcbo', callback);
    }

    function getUoMCbo(callback) {
        base('Setups/unitofmeasurement/getcbo/', callback);
    }

    function getToUoMFactor(firstUoMId, secondUoMId, callback) {
        base('Setups/uomconversion/gettouomfactor?fromUOMId=' + firstUoMId + '&toUOMId=' + secondUoMId, callback);
    }

    function loadUomUtilityCbo(callback) {
        base('Setups/unitofmeasurement/getunitofmeasurementcbo', callback);
    }

    function loadSubprocessCbo(processid, callback) {
        base('Processes/CompanySubProcess/getcbo?processid=' + processid, callback);
    }

    function loadProcessWithCompanyCbo(companyId, callback) {
        base('Processes/Process/getcbo?companyId=' + companyId, callback);
    }

    function getProcessCbo(callback) {
        base('Processes/Process/getcbo', callback);
    }
    function getCboProcessTypeByProcess(processId, callback) {
        base('Processes/ProcessType/GetCbobyProcess?processId=' + processId, callback);
    }
    function loadOperationCbo(subprocessid, callback) {
        base('Machines/operation/getoperationcbo?subprocessid=' + subprocessid, callback);
    }

    function getWashOperationCbo(recipewashsubprocessid, callback) {
        base('Productions/recipewashmaster/getWashOperationCbo?recipewashsubprocessid=' + recipewashsubprocessid, callback);
    }

    function getCboBuyer(callback) {
        base('Parties/buyer/getcbo', callback);
    }

    function getBuyerStyleCboByBuyer(buyerid, callback) {
        base('materials/buyerstyle/getcbo?buyerid=' + buyerid, callback);
    }

    function getBuyerDepartmentCboByBuyer(buyerid, callback) {
        base('Parties/BuyerDepartment/getcbo?buyerid=' + buyerid, callback);
    }

    function getBuyerDivisionCboByBuyer(buyerid, callback) {
        base('Parties/BuyerDivision/getcbo?buyerid=' + buyerid, callback);
    }

    function getBuyerBrandCboByBuyer(buyerid, callback) {
        base('Parties/BuyerBrand/getcbo?buyerid=' + buyerid, callback);
    }

    function getBuyerBrandCbo(callback) {
        base('Parties/BuyerBrand/GetCboAll', callback);
    }

    // Get Religion cbo list.
    function getCboReligion(callback) {
        base('Setups/religion/getcbo', callback);
    }

    // Get BloodGroup cbo list.
    function getCboBloodGroup(callback) {
        base('employees/bloodgroup/getcbo', callback);
    }

    // Get City cbo list by Company.

    function getCboCityByCompany(companyId, callback) {
        base('employees/stoppage/getcitybycompanycbo?companyId=' + companyId, callback);
    }

    // Get QualificationLevel cbo list.
    function getCboQualificationLevel(callback) {
        base('employees/qualificationlevel/getcbo', callback);
    }

    // Get QualificationStream cbo list.
    function getCboQualificationStream(callback) {
        base('employees/qualificationstream/getcbo', callback);
    }

    // Get DepreciationRule cbo list.
    function getCboDepreciationRule(callback) {
        base('fixedassets/fixedAssetdepreciationrule/GetCbo/', callback);
    }

    //Project
    function getCboProjectPlanningCategory(callback) {
        base('Projects/projectplanningcategory/GetCbo/', callback);
    }

    function getCboProjectPlanningSubCategory(callback) {
        base('Projects/projectplanningsubcategory/GetCbo/', callback);
    }

    function getCboProjectPlanning(callback) {
        base('Projects/projectplanning/GetCbo/', callback);
    }

    //HNS Code
    function getHNSCbo(callback) {
        base('Setups/hsncode/getcbo/', callback);
    }

    //UoM by Material Master
    function getUomCboByMaterialMaster(materilaMasterId, callback) {
        base('Materials/materialmaster/getuomcbobymaterialmaster?id=' + materilaMasterId, callback);
    }

    //UoM by Material Group
    function getUoMCboByMaterialGroup(materilaGroupId, callback) {
        base('Ordermanagements/salesorderlinear/getmguomlist?mgid=' + materilaGroupId, callback);
    }

    //Packing From
    function getPackingFromCboByCompanyGroup(companyGroupId, callback) {
        if (baseService.isUndefinedOrNull(companyGroupId)) {
            if (!baseService.isUndefinedOrNull($window.companyGroupId))
                companyGroupId = $window.companyGroupId;
            else
                companyGroupId = null;
        }
        base('Materials/packingform/getcbo?companyGroupId=' + companyGroupId, callback);
    }

    //Testing Std
    function getTestinStdCbo(companyGroupId, callback) {
        if (baseService.isUndefinedOrNull(companyGroupId)) {
            if (!baseService.isUndefinedOrNull($window.companyGroupId)) {
                companyGroupId = $window.companyGroupId;
            }
            else
                companyGroupId = null;
        }
        base('Setups/testingstandard/getcbo?companyGroupId=' + companyGroupId, callback);
    }

    //CostCenter
    function getCboCostCenterCategory(callback) {
        base('Organizations/CostCenterCategory/GetCbo/', callback);
    }

    function getCboCostCenterSubCategory(callback) {
        base('Organizations/CostCenterSubCategory/GetCbo/', callback);
    }

    function getCboServiceCategory(callback) {
        base('Setups/ServiceCategory/GetCbo/', callback);
    }

    function getCboServiceSubCategory(callback) {
        base('Setups/ServiceSubCategory/GetCbo/', callback);
    }

    function getTaxCodeCbo(callback) {
        base('accounts/taxcode/getcbo/', callback);
    }

    function getTaxCategoryCboByCountry(countryId, callback) {
        base('accounts/taxcategory/getcbo?countryId=' + countryId, callback);
    }

    function getTestingCategoryCbo(callback) {
        base('Setups/testingcategory/getcbo', callback);
    }

    function getPaymentModeCbo(callback) {
        base('Setups/PaymentMode/getcbo', callback);
    }

    function getCboComplianceDocumentCategory(callback) {
        base('employees/ComplianceDocumentCategory/getcbo', callback);
    }

    function getCboComplianceDocumentSubCategory(callback) {
        base('employees/ComplianceDocumentSubCategory/getcbo', callback);
    }

    function getCboAssetItemMachine(callback) {
        base('Machines/AssetItem/getcbo', callback);
    }

    function getCboAssetItemCharacteristics(callback) {
        base('fixedassets/AssetItemCharacteristics/getcbo', callback);
    }

    // Get all receiver by companyGroup.
    function getMailReceiverCbo(callback) {
        base('Setups/MailReceiver/getcbo?companyGroupId=' + $window.companyGroupId, callback);
    }

    function getCboReportingPerson(companyId, plantId, callback) {
        base('employees/HRDashboard/GetReportingPersonCbo?companyId=' + companyId + "&plantId=" + plantId, callback);
    }

    function getCboMaterialStorageByCompanyAndPlant(companyId, plantId, callback) {
        if (baseService.isUndefinedOrNull(companyId)) {
            if (!baseService.isUndefinedOrNull($window.companyId)) {
                companyId = $window.companyId;
            }
            else
                companyId = null;
        }
        base('Materials/MaterialStorage/GetCbo?companyId=' + companyId + '&plantid=' + plantId, callback);
    }

    function getCboLegalSalaryGrade(plantId, callback) {
        base('HumanResource/LegalSalaryGrade/GetCbo?plantId=' + plantId, callback);
    }

    function getCboMISBudgetCategory(companyId, plantId, divisionId, subDivisionId, unitId, fromDate, toDate, callback) {
        base('Accounts/MISAccountDashboard/MISBudgetCategoryCbo?companyId=' + companyId + '&plantid=' + plantId + '&divisionId=' + divisionId + '&subDivisionId=' + subDivisionId + '&unitId=' + unitId + '&fromDate=' + fromDate + '&toDate=' + toDate, callback);
    }

    function getCboEntityWisePlant(compnayGroupId, companyId, callback) {
        base('Accounts/MISAccountDashboard/GetEntityWisePlantCbo?compnayGroupId=' + compnayGroupId + '&companyId=' + companyId, callback);
    }

    function getCboEntityWiseDivision(compnayGroupId, companyId, plantId, callback) {
        base('Accounts/MISAccountDashboard/GetEntityWiseDivisionCbo?compnayGroupId=' + compnayGroupId + '&companyId=' + companyId + '&plantId=' + plantId, callback);
    }

    function getCboEntityWiseSubDivision(compnayGroupId, companyId, plantId, divisionId, callback) {
        base('Accounts/MISAccountDashboard/GetEntityWiseSubDivisionCbo?compnayGroupId=' + compnayGroupId + '&companyId=' + companyId + '&plantId=' + plantId + '&divisionId=' + divisionId, callback);
    }

    function getCboEntityWiseUnit(compnayGroupId, companyId, plantId, divisionId, subDivisionId, callback) {
        base('Accounts/MISAccountDashboard/GetEntityWiseUnitCbo?compnayGroupId=' + compnayGroupId + '&companyId=' + companyId + '&plantId=' + plantId + '&divisionId=' + divisionId + '&subDivisionId=' + subDivisionId, callback);
    }

    function getCboEntityWiseEntity(entityList, divisionId, subDivisionId, unitId, callback) {
        base('Accounts/MISAccountDashboard/GetEntityWiseEntityCbo?entityList=' + entityList + '&divisionId=' + divisionId + '&subDivisionId=' + subDivisionId + '&unitId' + unitId, callback);
    }

    function getCboWithBuyer(companyGroupId, callback) {
        if (baseService.isUndefinedOrNull(companyGroupId)) {
            if (!baseService.isUndefinedOrNull($window.companyGroupId)) {
                companyGroupId = $window.companyGroupId;
            }
            else
                companyGroupId = null;
        }
        base('Setups/TestingStandard/GetCboWithBuyer?companyGroupId=' + companyGroupId, callback);
    }
    function getSalaryProcessIdCboByYearMonth(month, year, isCompletedMonth, callback) {
        if (baseService.isUndefinedOrNull(companyGroupId)) {
            if (!baseService.isUndefinedOrNull($window.companyGroupId)) {
                companyGroupId = $window.companyGroupId;
            }
            else
                companyGroupId = null;
        }
        base('PayRegisterBDReport/GetSalaryprocessIdCbo?month=' + month + '&year=' + year + '&IsCompletedMonth=' + isCompletedMonth, callback);
    }

    function getPayGroupCbo(callback) {
        base('PayRegisterBDReport/GetPayGroupCbo', callback);
    }

    function getAttendanceDayStatus(callback) {
        base('DailyDayStatus/GetAttendanceDayStatus', callback);
    }

    function getBabyNoCbo(callback) {
        base('MaternityLeaveTransaction/GetBabyNoCbo', callback);
    }

    function getPayRollGroupCbo(callback) {
        base('PayrollReports/GetPayRollGroupCbo', callback);
    }
    function getEmployeeBankCbo(callback) {
        base('SalaryPaymentStatements/GetEmployeeBankCbo', callback);
    }

    function getAuthorizationConfigCbo(status, callback) {
        base('Employees/AuthorizationConfig/getcbo?status=' + status, callback);
    }

    function getEmployeeStatusWithMLVCbo(callback) {
        base('SalaryTopSheet/GetEmployeeStatusWithMLVCbo', callback);
    }
    function getCboVoucherTypeAutoLoanList(callback) {
        base('Accounts/VoucherTypeMatrix/GetCboVoucherTypeAutoLoanList', callback);
    }
    return service;
}