MaterialConfig.$inject = ['$routeProvider', '$locationProvider'];
function MaterialConfig($routeProvider, $locationProvider) {
    $routeProvider
        .when('/characteristics', {
            templateUrl: 'Materials/characteristics',
            controller: 'characteristicsController'
        })
        .when('/characteristics-value', {
            templateUrl: 'Materials/characteristicsvalue',
            controller: 'characteristicsValueController'
        })
        .when('/materialGrid', {
            templateUrl: 'Materials/materialGrid',
            controller: 'materialGridController'
        })
        .when('/materialtype', {
            templateUrl: 'Materials/materialtype',
            controller: 'materialTypeController'
        })
        .when('/materialgroup1', {
            templateUrl: 'Materials/materialgroup1',
            controller: 'materialGroup1Controller'
        })
        .when('/materialgroup2', {
            templateUrl: 'Materials/materialgroup2',
            controller: 'materialGroup2Controller'
        })
        .when('/materialgroup3', {
            templateUrl: 'Materials/materialgroup3',
            controller: 'materialGroup3Controller'
        })
        .when('/materialgroup4', {
            templateUrl: 'Materials/materialgroup4',
            controller: 'materialGroup4Controller'
        })
        .when('/material-attribute', {
            templateUrl: 'Materials/materialattribute',
            controller: 'materialAttributeController'
        })
        .when('/material-attribute-master', {
            templateUrl: 'Materials/materialattributemaster',
            controller: 'materialAttributeMasterController'
        })
        .when('/material-group-master', {
            templateUrl: 'Materials/materialgroupmaster',
            controller: 'materialGroupMasterController'
        })
        .when('/material-group-article', {
            templateUrl: 'Materials/materialgroupmaster/Article',
            controller: 'materialGroupArticleController'
        })
        .when('/material-master', {
            templateUrl: 'Materials/MaterialMaster',
            controller: 'materialMasterController'
        })
        .when('/material-master-article', {
            templateUrl: 'Materials/materialmasterarticle',
            controller: 'materialMasterArticleController'
        })
        .when('/material-master-alternativeuom', {
            templateUrl: 'Materials/materialmasteralternativeuom',
            controller: 'materialMasterAlternativeUOMController'
        })
        .when('/characteristics-wise-properties', {
            templateUrl: 'Materials/characteristicswiseproperties',
            controller: 'characteristicsWisePropertiesController'
        })
        .when('/material-master-report', {
            templateUrl: 'Materials/materialmaster/materialmasterreportpage',
            controller: 'materialMasterReportController'
        })
        .when('/material-group-accountdeterminate', {
            templateUrl: 'Materials/materialGroupGL',
            controller: 'materialGroupGLController'
        })
        .when('/material-master-alternativeuom', {
            templateUrl: 'Materials/materialmasteralternativeuom',
            controller: 'materialMasterAlternativeUOMController'
        })
        .when('/material-attribute-value', {
            templateUrl: 'Materials/materialAttributeValue',
            controller: 'materialAttributeValueController'
        })
        .when('/defect-code', {
            templateUrl: 'Materials/defectcode',
            controller: 'defectCodeController'
        })
        .when('/fg-zone', {
            templateUrl: 'Materials/fgzone',
            controller: 'fgzoneController'
        })
        .when('/fg-component', {
            templateUrl: 'Materials/fgcomponent',
            controller: 'fgcomponentController'
        })
        .when('/our-style', {
            templateUrl: 'Materials/ourstyle',
            controller: 'ourStyleController'
        })
        .when('/buyer-style', {
            templateUrl: 'Materials/buyerstyle',
            controller: 'buyerStyleController'
        })
        .when('/packing-form', {
            templateUrl: 'Materials/packingForm',
            controller: 'packingFormController'
        })

        .when('/material-category', {
            templateUrl: 'Materials/materialcategory',
            controller: 'materialCategoryController'
        })
        .when('/material-subcategory', {
            templateUrl: 'Materials/materialsubcategory',
            controller: 'materialSubCategoryController'
        })
        .when('/fabricroll-management-settings', {
            templateUrl: 'Materials/fabricrollmanagementsettings',
            controller: 'fabricRollManagementSettingsController'
        })
        .when('/material-master-account-determinate', {
            templateUrl: 'Materials/materialMasterGL',
            controller: 'materialMasterGLController'
        })
        .when('/material-stock', {
            templateUrl: 'Materials/materialstock',
            controller: 'materialStockController'
        })
        .when('/machine', {
            templateUrl: 'materials/materialmastermachineprocess/',
            controller: 'machineController'
        })
        .when('/machine-budget', {
            templateUrl: 'materials/MachineBudget/',
            controller: 'machineBudgetController'
        })
        .when('/material-storage', {
            templateUrl: 'materials/materialStorage/',
            controller: 'materialStorageController'
        })
        .when('/fabric-roll-master', {
            templateUrl: 'materials/FabricRollMaster/',
            controller: 'fabricRollMasterController'
        })
        .when('/fabric-roll', {
            templateUrl: 'materials/FabricRoll/',
            controller: 'FabricRollController'
        })

        .when('/fabric-rolls', {
            templateUrl: 'materials/FabricRoll/aplos',
            controller: 'FabricRollsController'
        })

        .when('/material-ledger', {
            templateUrl: 'materials/MaterialLedger/aplos',
            controller: 'materialledgerController'
		})

		.when('/material-stock-balance', {
			templateUrl: 'materials/MaterialLedger/MaterialStockBalance',
			controller: 'materialledgerController'
        })

        .when('/material-stationary-requisition', {
            templateUrl: 'materials/MaterialLedger/Materialstationeryrequest',
            controller: 'materialledgerController'
        })
        .when('/requisition-register', {
            templateUrl: 'materials/RequisitionRegister/Aplos',
            controller: 'RequisitionRegisterController'
        })



        .when('/Physical-Inventory-Report', {
            templateUrl: 'materials/MaterialLedger/PhysicalInventory',
            controller: 'materialledgerController'
        })


        .when('/material-master-stock', {
            templateUrl: 'materials/MaterialLedger/MaterialMasterStock',
            controller: 'materialledgerController'
        })

        .when('/material-store-ledger', {
            templateUrl: 'materials/MaterialLedger/MaterialStoreLedger',
            controller: 'materialledgerController'
        })

        .when('/material-consumption-report', {
            templateUrl: 'materials/MaterialLedger/MaterialConsumption',
            controller: 'materialledgerController'
        })


        .when('/material-receipts-report', {
            templateUrl: 'materials/MaterialLedger/MaterialReceiptsReport',
            controller: 'materialledgerController'
        })


        .when('/material-issue-report', {
            templateUrl: 'materials/MaterialLedger/MaterialIssueReport',
            controller: 'materialledgerController'
        })


		.when('/purchase-register', {
			templateUrl: 'materials/MaterialLedger/PurchaseRegister',
			controller: 'materialledgerController'
        })

        .when('/service-acknowledgement-register', {
            templateUrl: 'materials/MaterialLedger/ServiceAcktRegister',
            controller: 'purchaseorderRegisterController'
        })
        .when('/purchase-order-register', {
            templateUrl: 'materials/MaterialLedger/PurchaseOrderRegister',
            controller: 'purchaseorderRegisterController'
        })


        .when('/purchase-return-register', {
            templateUrl: 'materials/MaterialLedger/PurchaseReturnRegister',
            controller: 'PurchaseReturnRegisterController'
        })

        .when('/issue-register', {
            templateUrl: 'materials/IssueRegister/aplos',
            controller: 'issueRegisterController'
        })

        .when('/issue-return-register', {
            templateUrl: 'materials/IssueRegister/IssueReturnRegister',
            controller: 'IssueReturnRegisterController'
        })
        .when('/material-setting', {
            templateUrl: 'materials/materialsetting/Aplos',
            controller: 'materialSettingController'
        })
        .when('/material-master-type', {
            templateUrl: 'materials/MaterialMasterType/Aplos',
            controller: 'MaterialMasterTypeController'
        })
        .when('/rack', {
            templateUrl: 'materials/Rack/Aplos',
            controller: 'RackController'
        })
        .when('/service-po-register', {
            templateUrl: 'materials/MaterialLedger/ServicePORegister',
            controller: 'ServicePORegisterController'
        })
        .when('/detention-master', {
            templateUrl: 'materials/DetentionMaster/Aplos',
            controller: 'DetentionMasterController'
        })
        .when('/utility-master', {
            templateUrl: 'materials/UtilityMaster/Aplos',
            controller: 'UtilityMasterController'
        })
        .when('/utility-transaction', {
            templateUrl: 'materials/UtilityTransaction/Aplos',
            controller: 'UtilityTransactionController'
        })
        .when('/utility-transactionReport', {
            templateUrl: 'materials/UtilityTransactionReport/Aplos',
            controller: 'UtilityTransactionReportController'
        })
        .when('/storage-bin-master', {
            templateUrl: 'materials/StorageBinMaster/Aplos',
            controller: 'StorageBinMasterController'
        })
        .when('/storage-bin-allocation', {
            templateUrl: 'materials/StorageBinAllocation/Aplos',
            controller: 'StorageBinAllocationController'
        })
        .when('/stock-register', {
            templateUrl: 'materials/StockRegister/StockRegister',
            controller: 'StockRegisterController'
        })
        .when('/requisition-status', {
            templateUrl: 'materials/StockRegister/RequisitionStatus',
            controller: 'RequisitionStatusController'
        })
        .when('/process-wise-material-allocation', {
            templateUrl: 'materials/ProcessWiseMaterialAllocation/Aplos',
            controller: 'ProcessWiseMaterialAllocationController'
        })
        .when('/detention-log', {
            templateUrl: 'materials/DetentionLog/Aplos',
            controller: 'DetentionLogController'
        })

        .when('/detention-logout', {
            templateUrl: 'materials/DetentionLogout/Aplos',
            controller: 'DetentionLogoutController'
        })

        .when('/inventory-issue-control', {
            templateUrl: 'materials/IssueControl/Aplos',
            controller: 'IssueControlController'
        })
        .when('/material-issue-control', {
            templateUrl: 'materials/MaterialIssueControl/Aplos',
            controller: 'MaterialIssueControlController'
        })
        .when('/material-issue-ctrl-approval', {
            templateUrl: 'materials/MaterialIssueControl/Approval',
            controller: 'MaterialIssueControlApprovalController'
        })
        .when('/material-issue', {
            templateUrl: 'materials/MaterialIssueControl/issue',
            controller: 'MaterialIssueController'
        })
        .when('/utility-group', {
            templateUrl: 'Materials/UtilityGroup',
            controller: 'UtilityGroupController'
        })
        .when('/detention-log-report', {
            templateUrl: 'Materials/DetentionLogReport',
            controller: 'DetentionLogReportController'
        })
        .when('/scan-data', {
            templateUrl: 'Materials/ScanData/Aplos',
            controller: 'ScanDataController'
        })
        .when('/waste-type', {
            templateUrl: 'Materials/WasteType/Aplos',
            controller: 'WasteTypeController'
        })
        .when('/material-control-report', {
            templateUrl: 'Materials/MaterialIssueReport/Aplos',
            controller: 'MaterialIssueReportController'
        })
        .when('/material-planning', {
            templateUrl: 'materials/RawMaterialPlanning/Aplos',
            controller: 'RawMaterialPlanningController'
        })
        .when('/in-ward-material', {
            templateUrl: 'materials/MaterialLedger/InWardMaterial',
            controller: 'InWardMaterialController'
        })
        .when('/input-confirmation', {
            templateUrl: 'materials/InputConfirmation/Aplos',
            controller: 'InputConfirmationController'
        })

        .when('/qrcode-generator', {
            templateUrl: 'materials/QRCodeGenerator/Aplos',
            controller: 'QRCodeGeneratorController'
        })

        .when('/weighingmachine', {
            templateUrl: 'materials/WeighingScaleMaster/Aplos',
            controller: 'WeighingScaleMasterController'
        })
        .when('/lot-creation', {
            templateUrl: 'materials/LOTCreation/Aplos',
            controller: 'LOTCreationController'
        })
        .when('/barcode-gen-setting', {
            templateUrl: 'materials/BarcodeGeneratorSetting/Aplos',
            controller: 'BarcodeGeneratorSettingController'
        })
};
