OrderManagementConfig.$inject = ['$routeProvider', '$locationProvider'];
function OrderManagementConfig($routeProvider, $locationProvider)
{
    $routeProvider
        .when('/seasons', {
            templateUrl: 'OrderManagements/seasons',
            controller: 'seasonsController'
        })
        .when('/order-category', {
            templateUrl: 'OrderManagements/ordercategory',
            controller: 'orderCategoryController'
        })
        .when('/order-control-stage', {
            templateUrl: 'OrderManagements/ordercontrolstage',
            controller: 'orderControlStageController'
        })
        .when('/order-status', {
            templateUrl: 'OrderManagements/orderstatus',
            controller: 'orderStatusController'
        })
        .when('/ship-mode', {
            templateUrl: 'OrderManagements/shipmode',
            controller: 'shipModeController'
        })
        .when('/port', {
            templateUrl: 'OrderManagements/port',
            controller: 'portController'
        })
        .when('/destination', {
            templateUrl: 'OrderManagements/destination',
            controller: 'destinationController'
        })
        .when('/lsd', {
            templateUrl: 'OrderManagements/lsd',
            controller: 'lsdController'
        })
        .when('/dmm', {
            templateUrl: 'Productions/dmm',
            controller: 'dMMController'
        })
        .when('/master-order', {
            templateUrl: 'OrderManagements/masterOrder',
            controller: 'masterOrderController'
        })
        .when('/sales-order-update', {
            templateUrl: 'OrderManagements/SalesOrderUpdate',
            controller: 'SalesOrderUpdateController'
        })
        .when('/sample-order', {
            templateUrl: 'OrderManagements/SampleOrder',
            controller: 'sampleOrderController'
        })
        .when('/sample-order-pending', {
            templateUrl: 'OrderManagements/sampleOrderPending',
            controller: 'sampleOrderPendingController'
        })
        .when('/sample-packing-list', {
            templateUrl: 'OrderManagements/samplepackinglist',
            controller: 'samplePackingListController'
        })
        .when('/commitment', {
            templateUrl: 'OrderManagements/commitment',
            controller: 'commitmentController'
        })
        .when('/inquiry', {
            templateUrl: 'OrderManagements/inquiry',
            controller: 'inquiryController'
        })
        .when('/sample-requisition', {
            templateUrl: 'OrderManagements/SampleRequisition',
            controller: 'sampleRequisitionController'
        })
        .when('/critical', {
            templateUrl: 'OrderManagements/Critical',
            controller: 'criticalController'
        })
        .when('/lineday-criticality', {
            templateUrl: 'OrderManagements/LineDayCriticality',
            controller: 'lineDayCriticalityController'
        })
        .when('/line-production-booking', {
            templateUrl: 'OrderManagements/lineproductionbooking',
            controller: 'lineProductionBookingController'
        })
        .when('/line-employee-assign', {
            templateUrl: 'OrderManagements/LineEmployeeAssign',
            controller: 'lineEmployeeAssignController'
        })
        .when('/line-production-excel', {
            templateUrl: 'OrderManagements/LineProductionBooking/LineProductionExcel',
            controller: 'lineProductionExcelController'
        })
        .when('/line-employee-assign-edit', {
            templateUrl: 'OrderManagements/LineEmployeeAssign/LineEmployeeEdit',
            controller: 'lineEmployeeAssignEditController'
        })
        .when('/line-employee-date-report', {
            templateUrl: 'OrderManagements/LineEmployeeAssign/LineEmployeeDateReport',
            controller: 'lineEmployeeAssignController'
        })
        .when('/sales-order-packing-list', {
            templateUrl: 'OrderManagements/salesorderpackinglist',
            controller: 'salesOrderPackingListController'
        })
        .when('/sales-order-invoice', {
            templateUrl: 'OrderManagements/salesorderinvoice',
            controller: 'salesOrderInvoiceController'
        })
        .when('/sales-order-pending', {
            templateUrl: 'OrderManagements/salesorderpending',
            controller: 'salesOrderPendingController'
        })
        .when('/customer-division', {
            templateUrl: 'OrderManagements/CustomerDivision',
            controller: 'customerDivisionController'
        })
        .when('/production-order', {
            templateUrl: 'OrderManagements/productionorder',
            controller: 'productionOrderController'
        })

        .when('/production-order-subprocess', {
            templateUrl: 'OrderManagements/productionOrderSubprocess',
            controller: 'productionOrderSubprocessController'
        })
        .when('/packing-list', {
            templateUrl: 'OrderManagements/packingListMaster',
            controller: 'packingListMasterController'
        })
        //.when('/order-report', {
        //    templateUrl: 'ordermanagements/masterorder/report',
        //    controller: 'masterOrderReportController'
        //})
        .when('/product-planning', {
            templateUrl: 'OrderManagements/productPlanning',
            controller: 'productPlanningController'
        })
        .when('/productionOrderSchedulingParametersType1', {
            templateUrl: 'OrderManagements/ProductionOrderSchedulingParametersType1',
            controller: 'productionOrderSchedulingParametersType1Controller'
        })
        .when('/production-calendar', {
            templateUrl: 'OrderManagements/productionCalendar',
            controller: 'productionCalendarController'
        })
        .when('/plant-calendar', {
            templateUrl: 'OrderManagements/plantCalendar',
            controller: 'plantCalendarController'
        })
        .when('/production-order-reports', {
            templateUrl: 'OrderManagements/ProductionOrderReports',
            controller: 'productionOrderReportsController'
        })
        .when('/production-resources', {
            templateUrl: 'OrderManagements/ProductionResources',
            controller: 'productionResourcesController'
        })
        .when('/running-order-parameters', {
            templateUrl: 'OrderManagements/RunningOrderParameters',
            controller: 'runningOrderParametersController'
        })
        .when('/inquiry-master', {
            templateUrl: 'OrderManagements/InquiryMaster',
            controller: 'inquiryMasterController'
        })
        .when('/independent-order', {
            templateUrl: 'OrderManagements/MasterOrder/IndependentOrder',
            controller: 'independentOrderController'
        })
        .when('/bom', {
            templateUrl: 'OrderManagements/BOMMaster/Aplos',
            controller: 'BOMMasterController'
        })
        .when('/bom-tag', {
            templateUrl: 'OrderManagements/BOMMasterAttachment/Aplos',
            controller: 'BOMMasterAttachmentController'
        }).when('/order-control-types', {
            templateUrl: 'OrderManagements/OrderControlTypes/Aplos',
            controller: 'OrderControlTypesController'
        })
        .when('/order-control', {
            templateUrl: 'OrderManagements/OrderControl/Aplos',
            controller: 'OrderControlController'
        })

        .when('/production-report', {
            templateUrl: 'OrderManagements/ProductionReports/Aplos',
            controller: 'ProductionReportsController'
        })

        .when('/order-report', {
            templateUrl: 'OrderManagements/OrderReport/Aplos',
            controller: 'OrderReportController'
        })    
       
        .when('/boq-upload', {
            templateUrl: 'OrderManagements/BOQUpload/Aplos',
            controller: 'BOQUploadController'
        })    
        .when('/bom-reports', {
            templateUrl: 'OrderManagements/BOMReports/Aplos',
            controller: 'BOMReportsController'
        })   
        .when('/mixing', {
            templateUrl: 'OrderManagements/Mixing/Aplos',
            controller: 'mixingController'
        })   
        .when('/product-library', {
            templateUrl: 'OrderManagements/ProductLibrary/Aplos',
            controller: 'ProductLibraryController'
        })
        .when('/scan-item', {
            templateUrl: 'OrderManagements/ScanItem/Aplos',
            controller: 'ScanItemController'
        })
        .when('/report-currency-exchange-rates', {
            templateUrl: 'OrderManagements/ReportCurrencyExchange/Aplos',
            controller: 'ReportCurrencyExchangeController'
        })
        .when('/production-planning-report', {
            templateUrl: 'OrderManagements/ProductionPlanningReport/Aplos',
            controller: 'ProductionPlanningReportController'
        })
       
        .when('/os3-dashboard', {
            templateUrl: 'OrderManagements/OS3Dashboard/Aplos',
            controller: 'OS3DashboardController'
        })
        .when('/order', {
            templateUrl: 'OrderManagements/Order/Aplos',
            controller: 'OrderController'
        })
        .when('/terms-and-conditions', {
            templateUrl: 'OrderManagements/TermsAndConditions/Aplos',
            controller: 'TermsAndConditionsController'
        })

        //packing & Dispatch Ather sir
        .when('/packing-content', {
            templateUrl: 'OrderManagements/PackingContent/Aplos',
            controller: 'PackingContentController'
        })
        .when('/packing-confirmation', {
            templateUrl: 'OrderManagements/PackingConfirmation/Aplos',
            controller: 'PackingConfirmationController'
        }) 
        .when('/dispatch', {
            templateUrl: 'OrderManagements/DispatchMaster/Aplos',
            controller: 'DispatchMasterController'
        })
        .when('/packing-type', {
            templateUrl: 'OrderManagements/PackingType/Aplos',
            controller: 'PackingTypeController'
        })
        .when('/sales-order-app', {
            templateUrl: 'OrderManagements/SalesOrderApproval/Aplos',
            controller: 'SalesOrderApprovalController'
        }) 

        .when('/product-integrity-analysis-master', {
            templateUrl: 'OrderManagements/ProductIntegrityAnalysisMaster/Aplos',
            controller: 'ProductIntegrityAnalysisMasterController'
        })

        .when('/product-integrity-analysis', {
            templateUrl: 'OrderManagements/ProductIntegrityAnalysis/Aplos',
            controller: 'ProductIntegrityAnalysisController'
        })

        .when('/so-pro-com', {
            templateUrl: 'OrderManagements/SalesOrderWiseProductionCompletionReport/Aplos',
            controller: 'SalesOrderWiseProductionCompletionReportController'
        })

        .when('/productivity-recovery-master', {
            templateUrl: 'OrderManagements/ProductivityRecoveryMaster/Aplos',
            controller: 'ProductivityRecoveryMasterController'
        })
        .when('/documentation', {
            templateUrl: 'OrderManagements/documentation/Aplos',
            controller: 'documentationController'
        })
        .when('/tcgroup', {
            templateUrl: 'OrderManagements/TermsAndConditions/Group',
            controller: 'TermsandConditionGroupController'
        })
        .when('/mo-checkby', {
            templateUrl: 'OrderManagements/masterOrder/CheckBy',
            controller: 'masterOrderCheckByController'
        })
        .when('/mo-approveby', {
            templateUrl: 'OrderManagements/masterOrder/ApproveBy',
            controller: 'masterOrderApproveByController'
        })
        .when('/productionOrderSchedulingParametersType1New', {
            templateUrl: 'OrderManagements/ProductionOrderSchedulingParametersType1/AplosNew',
            controller: 'productionOrderSchedulingParametersType1NewController'
        })
        ;
}
