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
        .when('/recipe-Material-Grouping-Master', {
            templateUrl: 'Productions/RecipeMaterialGroupingMaster/Aplos',
            controller: 'recipeMaterialGroupingMasterController'
        })
        .when('/planning-types', {
            templateUrl: 'Productions/PlanningTypes/Aplos',
            controller: 'planningTypesController'
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
        .when('/cut-plan', {
            templateUrl: 'Productions/CutPlan/Aplos',
            controller: 'CutPlanController'
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

        ;
}
