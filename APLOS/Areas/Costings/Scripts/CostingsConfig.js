CostingsConfig.$inject = ['$routeProvider', '$locationProvider'];
function CostingsConfig($routeProvider, $locationProvider) {
    $routeProvider
        .when('/costing-category', {
            templateUrl: 'Costings/CostingCategory/Aplos',
            controller: 'costingCategoryController'
        })
        .when('/costing-component', {
            templateUrl: 'Costings/CostingComponent/Aplos',
            controller: 'CostingComponentController'
        })
        .when('/costing-item', {
            templateUrl: 'Costings/CostingItem/Aplos',
            controller: 'costingItemController'
        })
        .when('/costing-subcategory', {
            templateUrl: 'Costings/CostingSubCategory/Aplos',
            controller: 'costingSubCategoryController'
        })
        .when('/quick-costing', {
            templateUrl: 'Costings/QuickCostingMaster/Aplos',
            controller: 'quickCostingMasterController'
        })
        .when('/costing-type-component', {
            templateUrl: 'Costings/CostingTypeComponent/Aplos',
            controller: 'CostingTypeComponentController'
        })
        .when('/costing-group-formula', {
            templateUrl: 'costings/costinggroupformula/Aplos',
            controller: 'costingGroupFormulaController'
        })
        .when('/order-costing', {
            templateUrl: 'costings/OrderCosting/Aplos',
            controller: 'OrderCostingController'
        })
        .when('/order-costing-approval', {
            templateUrl: 'costings/OrderCostingApproval/Aplos',
            controller: 'OrderCostingApprovalController'
        })
        .when('/order-costing-unapproval', {
            templateUrl: 'costings/OrderCostingUnApproval/Aplos',
            controller: 'OrderCostingUnApprovalController'
        })
        .when('/up-charge-matrix', {
            templateUrl: 'costings/CostingUpCharge/Aplos',
            controller: 'CostingUpChargeController'
        })
        .when('/boq-criteria', {
            templateUrl: 'costings/BOQCriteria/Aplos',
            controller: 'BOQCriteriaController'
        })
        .when('/quick-boq-report', {
            templateUrl: 'costings/QuickBOQReport/Aplos',
            controller: 'QuickBOQReportController'
        })
        .when('/boq-generation', {
            templateUrl: 'costings/BOQGeneration/Aplos',
            controller: 'BOQGenerationController'
        })
        .when('/costing-boq', {
            templateUrl: 'costings/BOQ/Aplos',
            controller: 'BOQController'
        })
        .when('/boq-purchase-order', {
            templateUrl: 'costings/BOQPurchaseOrder/Aplos',
            controller: 'BOQPurchaseOrderController'
        })

        .when('/item-consumption', {
            templateUrl: 'costings/ItemConsumption/Aplos',
            controller: 'ItemConsumptionController'
        })
        .when('/boq-approval', {
            templateUrl: 'costings/BOQ/Approval',
            controller: 'BOQApprovalController'
        })
        .when('/boq-cos-app-setting', {
            templateUrl: 'costings/BOQCostingApprovalSetting/Aplos',
            controller: 'BOQCostingApprovalSettingController'
        })
        .when('/orderlinecostingitem', {
            templateUrl: 'costings/OrderLineCostingItem/Aplos',
            controller: 'OrderLineCostingItemController'
        })
        .when('/boq-status-report', {
            templateUrl: 'costings/BOQStatusReport/Aplos',
            controller: 'BOQStatusReportController'
        })
        .when('/bom-detail', {
            templateUrl: 'costings/BOMDetailMaster/Aplos',
            controller: 'BOMDetailMasterController'
        })
        .when('/olcsmmap', {
            templateUrl: 'costings/OrderLineCostingItem/OLCSMMap',
            controller: 'OrderLineCostingItemServiceMasterMappingController'
        })

        ;
}
