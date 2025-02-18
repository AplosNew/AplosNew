ProductionsConfig.$inject = ['$routeProvider', '$locationProvider'];
function ProductionsConfig($routeProvider, $locationProvider) {
    $routeProvider
        // sales-order
        .when('/recipe-master', {
            templateUrl: 'Productions/recipemaster',
            controller: 'recipeMasterController'
        })
        .when('/recipe-wash-master', {
            templateUrl: 'Productions/recipewashmaster',
            controller: 'recipeWashMasterController'
        })
        .when('/production-settings-uom', {
            templateUrl: 'Productions/productionsettingswithprocessuom',
            controller: 'productionSettingsWithProcessUOMController'
        })
        .when('/productionstatus', {
            templateUrl: 'Productions/productionstatus',
            controller: 'productionStatusController'
        })
        .when('/dmm', {
            templateUrl: 'Productions/dmm',
            controller: 'dMMController'
        })
        .when('/main-process-planing', {
            templateUrl: 'Productions/mainprocessplanning',
            controller: 'mainProcessPlanningController'
        })
       
        .when('/recipe-config', {
            templateUrl: 'productions/recipeconfig',
            controller: 'recipeConfigController'
        })       
        .when('/inventory-report', {
            templateUrl: 'productions/InventoryReport',
            controller: 'inventoryReportController'
        })
        .when('/recipe-material', {
            templateUrl: 'productions/RecipeMaterial',
            controller: 'recipeMaterialController'
        })

        .when('/recipe-global-master', {
            templateUrl: 'productions/RecipeGlobalMaster',
            controller: 'recipeGlobalMasterController'
        })
        .when('/recipe-operation', {
            templateUrl: 'productions/RecipeOperation',
            controller: 'recipeOperationController'
        })
        .when('/production-summary', {
            templateUrl: 'Productions/ProductionSummary/Aplos',
            controller: 'ProductionSummaryController'
        })
        .when('/production-summary-wc', {
            templateUrl: 'Productions/ProductionSummary/AplosWC',
            controller: 'ProductionSummaryWCController'
        })
        .when('/recipe-Material-Grouping-Master', {
            templateUrl: 'Productions/RecipeMaterialGroupingMaster/Aplos',
            controller: 'recipeMaterialGroupingMasterController'
        })
        .when('/planning-types', {
            templateUrl: 'Productions/PlanningTypes/Aplos',
            controller: 'planningTypesController'
        })
        .when('/planning-typesNew', {
            templateUrl: 'Productions/PlanningTypesNew/Aplos',
            controller: 'planningTypesNewController'
        })
        .when('/production-booking-period', {
            templateUrl: 'Productions/ProductionBookingPeriod/Aplos',
            controller: 'productionBookingPeriodController'
        })
        .when('/production-summary-inout', {
            templateUrl: 'Productions/ProductionSummary/Aplosinout',
            controller: 'ProductionSummaryInOutController'
        })
        .when('/production-summary-sfg', {
            templateUrl: 'Productions/ProductionSummary/AplosSFG',
            controller: 'ProductionSummarySFGController'
        })

        .when('/costing-types', {
            templateUrl: 'Productions/costingTypes/Aplos',
            controller: 'costingTypesController'
        })
    
        //.when('/production-summary-reject', {
        //    templateUrl: 'Productions/ProductionSummary/reject',
        //    controller: 'ProductionSummaryController'
        //})
        .when('/process-resources-constraint', {
            templateUrl: 'Productions/ProcessAndResourcesConstraint/Aplos',
            controller: 'ProcessAndResourcesConstraintController'
        })
        .when('/process-and-inventory-sequence', {
            templateUrl: 'Productions/ProcessAndInventorySequence/Aplos',
            controller: 'ProcessAndInventorySequenceController'
        })
        .when('/wip-report', {
            templateUrl: 'Productions/WIPReport/Aplos',
            controller: 'wipReportController'
        })
        .when('/production-dashboard', {
            templateUrl: 'Productions/ProductionDashboard/Aplos',
            controller: 'ProductionDashboardController'
        })

        .when('/movement-items', {
            templateUrl: 'Productions/MovementItems/Aplos',
            controller: 'MovementItemsController'
        })
        .when('/movement-master', {
            templateUrl: 'Productions/MovementMaterialMaster/Aplos',
            controller: 'MovementMaterialMasterController'
        })
        .when('/movement-scandata-report', {
            templateUrl: 'Productions/MovementScanDataReport/Aplos',
            controller: 'MovementScanDataReportController'
        })
        .when('/weighing-scale-report', {
            templateUrl: 'Productions/WeighingScaleReport/Aplos',
            controller: 'WeighingScaleReportController'
        })
        .when('/efficiency-slab', {
            templateUrl: 'Productions/EfficiencySlab/Aplos',
            controller: 'EfficiencySlabController'
        })
        .when('/material-purpose', {
            templateUrl: 'Productions/MaterialMovementPurpose/Aplos',
            controller: 'MaterialMovementPurposeController'
        })
        .when('/fg-valuation', {
            templateUrl: 'Productions/FGValuation/Aplos',
            controller: 'FGValuationController'
        })
        .when('/packing', {
            templateUrl: 'Productions/Packing/Aplos',
            controller: 'PackingController'
        })
        .when('/finish-goods-book', {
            templateUrl: 'Productions/FinishGoodsBooking/Aplos',
            controller: 'FinishGoodsBookingController'
        })
        .when('/fg-inventory-post', {
            templateUrl: 'Productions/FinishGoodsBooking/FGInventoryPost',
            controller: 'FinishGoodsBookingPostController'
        })
        .when('/consumption-book', {
            templateUrl: 'Productions/FinishGoodsBooking/ConsumptionBook',
            controller: 'ConsumptionBookingController'
        })
        .when('/daily-target', {
            templateUrl: 'Productions/DailyTarget/Aplos',
            controller: 'DailyTargetController'
        })
        .when('/production-control', {
            templateUrl: 'Productions/ProductionControl/Aplos',
            controller: 'ProductionControlController'
        })
        .when('/running-machine-setup-target', {
            templateUrl: 'Productions/RunningMachineSetUpTarget/Aplos',
            controller: 'RunningMachineSetUpTargetController'
        })
        .when('/production-relay', {
            templateUrl: 'Productions/ProductionRelay/Aplos',
            controller: 'ProductionRelayController'
        })
        .when('/shrinkage-group', {
            templateUrl: 'Productions/ShrinkageGroup/Aplos',
            controller: 'ShrinkageGroupController'
        })
        .when('/fabric-width', {
            templateUrl: 'Productions/FabricWidth/Aplos',
            controller: 'FabricWidthController'
        })
        .when('/shade', {
            templateUrl: 'Productions/Shade/Aplos',
            controller: 'ShadeController'
        })
        .when('/marker', {
            templateUrl: 'Productions/Marker/Aplos',
            controller: 'MarkerController'
        })
        .when('/production-target-report', {
            templateUrl: 'Productions/ProductionTargetReport/Aplos',
            controller: 'ProductionTargetReportController'
        })
        .when('/master-plan', {
            templateUrl: 'Productions/MasterPlan/Aplos',
            controller: 'MasterPlanController'
        })
        .when('/packing-invoice', {
            templateUrl: 'Productions/PackingInvoice/Aplos',
            controller: 'PackingInvoiceController'
        })

        .when('/conversion-parameter', {
            templateUrl: 'Productions/ProductionConversionParameter/Aplos',
            controller: 'ProductionConversionParameterController'
        })

        .when('/production-booking', {
            templateUrl: 'Productions/ProductionTransformationBooking/Aplos',
            controller: 'ProductionTransformationBookingController'
        })

        .when('/production-order-process-with-rate', {
            templateUrl: 'Productions/ProductionOrderProcessWithRate/Aplos',
            controller: 'ProductionOrderProcessWithRateController'
        })

        .when('/production-order-rate-report', {
            templateUrl: 'Productions/ProductionOrderRateReport/Aplos',
            controller: 'ProductionOrderRateReportController'
        })

        .when('/finish-goods-inventory-register-report', {
            templateUrl: 'Productions/FinishGoodsBooking/FinishGoodsInventoryRegister',
            controller: 'finishGoodsInventoryRegisterController'
        })
        .when('/machine-layout-report', {
            templateUrl: 'Productions/MachineLayoutReport/Aplos',
            controller: 'MachineLayoutReportController'
        })
        .when('/waste-master', {
            templateUrl: 'Productions/WasteMaster/Aplos',
            controller: 'WasteMasterController'
        })
        .when('/waste-transaction-report', {
            templateUrl: 'Productions/WasteTransactionReport/Aplos',
            controller: 'WasteTransactionReportController'
        })
        .when('/waste-issue', {
            templateUrl: 'Productions/WasteIssue/Aplos',
            controller: 'WasteIssueController'
        })
        .when('/waste-location', {
            templateUrl: 'Productions/WasteLocation/Aplos',
            controller: 'WasteLocationController'
        })
        .when('/general-data-master' , {
            templateUrl: 'Productions/GeneralDataMaster/Aplos',
            controller: 'GeneralDataMasterController'
        })
        .when('/general-data-upload', {
            templateUrl: 'Productions/GeneralDataOperations/Aplos',
            controller: 'GeneralDataOperationsController'
        })
        .when('/waste-transaction', {
            templateUrl: 'Productions/GeneralWaste/Aplos',
            controller: 'GeneralWasteController'
        })
        .when('/production-general-report', {
            templateUrl: 'Productions/ProductionGeneralReport/Aplos',
            controller: 'ProductionGeneralReportController'
        })
        .when('/ageing-stocks-report', {
            templateUrl: 'Productions/StocksAgeingReport/Aplos',
            controller: 'StocksAgeingReportController'
        })
        .when('/finished-stocks-report', {
            templateUrl: 'Productions/FinishedStockReport/Aplos',
            controller: 'FinishedStockReportController'
        })
        .when('/fg-inventory-stocks-report', {
            templateUrl: 'Productions/FGInventoryStockReport/Aplos',
            controller: 'FGInventoryStockReportController'
        }) 
        .when('/stocks-adjustment', {
            templateUrl: 'Productions/StocksAdjustment/Aplos',
            controller: 'StocksAdjustmentController'
        })
        .when('/pre-pack-definition', {
            templateUrl: 'Productions/PrePackDefinition/Aplos',
            controller: 'PrePackDefinitionController'
        })
        .when('/final-pack-definition', {
            templateUrl: 'Productions/FinalPackDefinition/Aplos',
            controller: 'FinalPackDefinitionController'
        })
        .when('/employee-operations', {
            templateUrl: 'Productions/EmployeeOperations/Aplos',
            controller: 'EmployeeOperationsController'
        })

        .when('/productive-allowance-rate-setup', {
            templateUrl: 'Productions/ProductiveAllowanceRateSetup/Aplos',
            controller: 'ProductiveAllowanceRateSetupController'
        })
        .when('/employee-time-out', {
            templateUrl: 'Productions/EmployeeTimeOut/Aplos',
            controller: 'EmployeeTimeOutController'
        })
        .when('/salesorder-status-report', {
            templateUrl: 'Productions/SalesOrderStatusReport/Aplos',
            controller: 'SalesOrderStatusReportController'
        })
        .when('/process-wise-production-booking', {
            templateUrl: 'Productions/ProcessWiseProductionBooking/Aplos',
            controller: 'ProcessWiseProductionBookingController'
        })
        .when('/quaity-process-booking', {
            templateUrl: 'Productions/QuaityProcessBooking/Aplos',
            controller: 'QuaityProcessBookingController'
        })
        .when('/production-report-with-parameter', {
            templateUrl: 'Productions/ProductionReportWithParameter/Aplos',
            controller: 'ProductionReportWithParameterController'
        })
        .when('/pro-sum-rpt', {
            templateUrl: 'Productions/ProductionSummary/Report',
            controller: 'ProductionSummaryReportController'
        })
        .when('/finished-goods-packing-report', {
            templateUrl: 'Productions/FinishedGoodsPackingReport/Aplos',
            controller: 'FinishedGoodsPackingReportController'
        })
        .when('/po-wise-production-status-report', {
            templateUrl: 'Productions/POWiseProductionStatusReport/Aplos',
            controller: 'POWiseProductionStatusReportController'
        })

        .when('/parameter-master', {
            templateUrl: 'Productions/Parameter/Aplos',
            controller: 'ParameterMasterController'
        })
        .when('/parameter', {
            templateUrl: 'Productions/ParameterMaster/Aplos',
            controller: 'ParameterController'
        })
        .when('/productionreport', {
            templateUrl: 'Productions/ProductionReport/Report',
            controller: 'ProductionReportController'
        })
        .when('/packing-scan-data', {
            templateUrl: 'Productions/PackingScanData/Aplos',
            controller: 'PackingScanDataController'
        })
        .when('/daily-planning-production-reports', {
            templateUrl: 'Productions/DailyPlanningAndProductionReport/Report',
            controller: 'DailyPlanningAndProductionReportController'
        })
        .when('/wcwork-stations-control-master', {
            templateUrl: 'Productions/WCWorkStationsControlMaster/Aplos',
            controller: 'WCWorkStationsControlMasterController'
        })
        .when('/wcwork-stations-control', {
            templateUrl: 'Productions/WCWorkStationsControl/AplosWC',
            controller: 'WCWorkStationsControlController'
        })
        .when('/wcwork-stations-control-report', {
            templateUrl: 'Productions/WCWorkStationsControlReport/Aplos',
            controller: 'WCWorkStationsControlReportController'
        })
        .when('/workcenter-wise-issue-control', {
            templateUrl: 'Productions/ProductionIssueControl/AplosWC',
            controller: 'ProductionIssueControlController'
        })
        .when('/workcenter-quality-control-master', {
            templateUrl: 'Productions/WorkCenterQualityControlMaster/AplosWC',
            controller: 'WorkCenterQualityControlMasterController'
        })
        .when('/process-quality-issue-control', {
            templateUrl: 'Productions/ProcessQualityControl/AplosWC',
            controller: 'ProcessQualityControlController'
        })
        .when('/quality-control', {
            templateUrl: 'Productions/QualityControl/AplosWC',
            controller: 'QualityControlController'
        })
        .when('/quality-action-update', {
            templateUrl: 'Productions/QualityActionUpdate/Aplos',
            controller: 'QualityActionUpdateController'
        })
        .when('/quality-action-confirmation', {
            templateUrl: 'Productions/QualityActionConfirmation/Aplos',
            controller: 'QualityActionConfirmationController'
        })
        .when('/quality-action-update-report', {
            templateUrl: 'Productions/QualityActionUpdateReport/Aplos',
            controller: 'QualityActionUpdateReportController'
        })
        .when('/pro-entity-setup', {
            templateUrl: 'Productions/ProductionOrderEntitySetup/Aplos',
            controller: 'ProductionOrderEntitySetupController'
        })
        .when('/lot-control', {
            templateUrl: 'Productions/LotControl/Aplos',
            controller: 'LotControlController'
        })
        .when('/master-plan-setup', {
            templateUrl: 'Productions/MasterPlanSetUp/Aplos',
            controller: 'MasterPlanSetUpController'
        })
        .when('/master-plan-details', {
            templateUrl: 'Productions/MasterPlanDetails/Aplos',
            controller: 'MasterPlanDetailsController'
        })
        .when('/cut-plan', {
            templateUrl: 'Productions/CutPlan/Aplos',
            controller: 'CutPlanController'
        })
        .when('/cut-plan-edit', {
            templateUrl: 'Productions/CutPlanEdit/Aplos',
            controller: 'CutPlanEditController'
        })
        .when('/salespurchase-transactiontype', {
            templateUrl: 'Productions/SalesPurchaseTransactionType/Aplos',
            controller: 'SalesPurchaseTransactionTypeController'
        })
        .when('/cutting-booking', {
            templateUrl: 'Productions/ProductionSummary/CuttingBooking',
            controller: 'ProductionCuttingBookingController'
        })
        .when('/marker-check', {
            templateUrl: 'Productions/Marker/check',
            controller: 'MarkerCheckController'
        })
        .when('/marker-approve', {
            templateUrl: 'Productions/Marker/Approve',
            controller: 'MarkerApproveController'
        })
        ;
}
