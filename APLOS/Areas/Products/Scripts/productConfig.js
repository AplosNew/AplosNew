ProductConfig.$inject = ['$routeProvider', '$locationProvider'];
function ProductConfig($routeProvider, $locationProvider) {
    $routeProvider
        .when('/product-category', {
            templateUrl: 'Products/productcategory/',
            controller: 'productCategoryController'
        })
        .when('/product-sub-category', {
            templateUrl: 'Products/productsubcategory/',
            controller: 'productSubCategoryController'
        })
        .when('/product', {
            templateUrl: 'Products/product/',
            controller: 'productController'
        })
        .when('/product-subcategory-attribute', {
            templateUrl: 'Products/productsubcategoryattribute/',
            controller: 'productSubCategoryAttributeController'
        })
        .when('/product-master', {
            templateUrl: 'Products/productmaster/',
            controller: 'productMasterController'
        })
        .when('/product-definition', {
            templateUrl: 'Products/productdefinition/',
            controller: 'productDefinitionController'
        })
        //.when('/GRN-without-PO', {
        //    templateUrl: 'Products/inventoryReceive/',
        //    controller: 'inventoryReceiveController'
        //})

        .when('/GRN-without-PO', {
            templateUrl: 'Products/inventoryReceive/VendorGRN',
            controller: 'inventoryReceiveController'
        })
		.when('/Employee-GRN', {
			templateUrl: 'Products/inventoryReceive/EmployeePurchaseGRN',
			controller: 'inventoryReceiveController'
		})
		.when('/GRN-By-PO', {
			templateUrl: 'Products/GoodsReceiveNote/GRNByPO',
			controller: 'GRNByPOController'
        })
        .when('/bin-wise-GRN', {
            templateUrl: 'Products/GoodsReceiveNote/AllBinWiseGRN',
            controller: 'AllBinWiseGRNController'
        })
        .when('/GRN-boq-PO', {
            templateUrl: 'Products/GoodsReceiveNote/GRNBOQPO',
            controller: 'GRNBOQPOController'
        })

        .when('/employee-purchase', {
            templateUrl: 'Products/inventoryReceive/employeepurchase',
            controller: 'employeePurchaseController'
        })
        .when('/grn-approved', {
            templateUrl: 'Products/GoodsReceiveNote/GRNApproved',
            controller: 'grnApprovedController'
        })
        .when('/grn-approval', {
            templateUrl: 'Products/GoodsReceiveNote/GRNApproval',
            controller: 'grnApprovalController'
        })
        .when('/grn-payment-hold', {
            templateUrl: 'Products/inventoryReceive/PaymentHold',
            controller: 'grnPaymentHoldController'
        })
        .when('/gate-pass', {
            templateUrl: 'Products/GateentryToken/GatePass',
            controller: 'GatePassController'
        })  
        .when('/in-gate-pass', {
            templateUrl: 'Products/GateentryToken/InGatePass',
            controller: 'InGatePassController'
        })   
        .when('/in-gate-pass-entry', {
            templateUrl: 'Products/GateentryToken/InGatePassNoGeneration',
            controller: 'InGatePassEntryController'
        }) 
        .when('/gate-pass-register', {
            templateUrl: 'Products/GatePassRegister/Aplos',
            controller: 'GatePassRegisterController'
        }) 
        //.when('/gate-pass-checked', {
        //    templateUrl: 'Products/GateentryToken/GatePassChecked',
        //    controller: 'GatePassController'
        //})
        //.when('/gate-pass-approved', {
        //    templateUrl: 'Products/GateentryToken/GatePassApproved',
        //    controller: 'GatePassController'
        //})
        .when('/gate-pass-dispatch', {
            templateUrl: 'Products/GateentryToken/GatePassApprovedBySecurity',
            controller: 'GatePassController'
        })
        .when('/gate-pass-employee', {
            templateUrl: 'Products/GateentryToken/GatePassEmployee',
            controller: 'GatePassEmployeeController'
        })
        .when('/In-out-gate-pass', {
            templateUrl: 'Products/GateentryToken/InOutGatePass',
            controller: 'InOutGatePassController'
        })
    
        .when('/PO-without-requisition', {
            templateUrl: 'Products/PurchaseOrder/Aplos',
            controller: 'PurchaseOrderController'
        })
        .when('/po-boq', {
            templateUrl: 'Products/PurchaseOrder/POBOQ',
            controller: 'purchaseOrderBOQController'
        })
        
        .when('/GRN', {
            templateUrl: 'Products/GoodsReceiveNote/Aplos',
            controller: 'goodsReceiveNoteController'
        })
        .when('/Grn-Check', {
            templateUrl: 'Products/GoodsReceiveNote/GRNCheck',
            controller: 'grnApprovalController'
        })
		.when('/purchaseOrder-Checked-By', {
            templateUrl: 'Products/PurchaseOrder/POChecke',
            controller: 'PurchaseOrderCheckController'
        })
 
        .when('/poclosed', {
            templateUrl: 'Products/PurchaseOrder/POClosed',
            controller: 'PurchaseOrderController'
        })
        .when('/product-material', {
            templateUrl: 'Products/ProductDefinition/MaterialMasterWithProductMaster',
            controller: 'materialMasterWithProductMasterController'
        })
        .when('/FGForMasterOrderPurchaseOrder', {
            templateUrl: 'Products/PurchaseOrder/FGForMasterOrder',
            controller: 'PurchaseOrderController'
        })
        .when('/FGPOForMasterOrder', {
            templateUrl: 'Products/PurchaseOrder/FGForMasterOrder',
            controller: 'FgPoFormasterOrderController'
        })
        .when('/purchaseOrder-unapproval', {
            templateUrl: 'Products/PurchaseOrder/POUnApproval',
            controller: 'PurchaseOrderController'
        })
        .when('/requisition', {
            templateUrl:'Products/Requisition/Aplos',
            controller: 'RequisitionController'
        })
        

        .when('/asset-issue-slip', {
            templateUrl: 'Products/GoodsReceiveNote/AssetIssueSlip',
            controller: 'AssetIssueSlipController'
        })
	
        .when('/issue-ui', {
            templateUrl: 'Products/GoodsReceiveNote/IssueUI',
            controller: 'IssueSlipController'
        })

        .when('/issueslip-check', {
            templateUrl: 'Products/GoodsReceiveNote/IssueSlipCheck',
            controller: 'IssueSlipCheckedByController'
        })

        .when('/approving-issue-slip', {
            templateUrl: 'Products/GoodsReceiveNote/ApprovingIssueSlip',
            controller: 'IssueSlipApprovedByController'
        })

        .when('/Material-Wise-issue-slip', {
            templateUrl: 'Products/GoodsReceiveNote/MaterialIssueSlip',
            controller: 'MaterialIssueSlipController'
        })

        .when('/purchaseOrder-Authorized', {
            templateUrl: 'Products/PurchaseOrder/POApprove',
            controller: 'PurchaseOrderApproveController'
        })
        .when('/material-budget', {
            templateUrl: 'Products/MaterialBudget/Aplos',
            controller: 'MaterialBudgetController'
        })
        .when('/requisition-checkby', {
            templateUrl: 'Products/InventoryCheckApproved/Aplos',
            controller: 'InventoryrequisitionCheckbyController'
        })
        .when('/gate-entry', {
            templateUrl: 'Products/GateentryToken/Aplos',
            controller: 'GateentryTokenController'
        })
        .when('/requisition-approvedby', {
            templateUrl: 'Products/InventoryCheckApproved/ReqAuthorized',
            controller: 'InventoryrequisitionapprovedbyController'
        })
        .when('/procurement-master', {
            templateUrl: 'Products/Procurement/Aplos',
            controller: 'ProcurementController'
        })
         
        .when('/Purchase-Order-By-Requisition', {
            templateUrl: 'Products/PurchaseOrder/PurchaseOrderByRequisition',
            controller: 'PurchaseOrderByRequisitionController'
        })
        .when('/Service-PO-By-Requisition', {
            templateUrl: 'Products/PurchaseOrder/ServicePOByRequisition',
            controller: 'ServicePOByRequisitionController'
        })
        .when('/Service-PO-Independent', {
            templateUrl: 'Products/PurchaseOrder/ServicePOIndependent',
            controller: 'ServicePOIndividualController'
        })
        .when('/Service-GRN', {
            templateUrl: 'Products/GoodsReceiveNote/IndependentServiceGRN',
            controller: 'IndependentServiceGRNController'
        })
        .when('/purchase-order-group', {
            templateUrl: 'Products/PurchaseOrderGroup',
            controller: 'purchaseOrderGroupController'
        })
        .when('/quality-std-set', {
            templateUrl: 'Products/QualityStdSet',
            controller: 'qualityStdSetController'
        })
        .when('/sfginventory', {
            templateUrl: 'Products/SFGInventory',
            controller: 'SFGInventoryController'
        })
        .when('/sfgmovement', {
            templateUrl: 'Products/SFGMovement',
            controller: 'SFGMovementController'
        })
        .when('/entity-sfginventory', {
            templateUrl: 'Products/EntitySFGInventory',
            controller: 'EntitySFGInventoryController'
        })
        .when('/plant-gate', {
            templateUrl: 'products/plantwisegate',
            controller: 'plantWiseGateController'
        })

        //#region Inventory Issue
        .when('/inventory-issue', {
            templateUrl: 'Products/inventoryIssue/Aplos',
            controller: 'inventoryIssueController'
        })
        
        .when('/asset-inventory-issue', {
            templateUrl: 'Products/inventoryIssue/AssetIssue',
            controller: 'assetInventoryIssueController'
        })
        .when('/slip-issue', {
            templateUrl: 'Products/inventoryIssue/SlipIssue',
            controller: 'inventoryIssueSlipBaseController'
        })
        .when('/issue-return', {
            templateUrl: 'Products/inventoryIssue/IssueReturn',
            controller: 'IssueReturnController'
        })
        .when('/slip-asset-Issue', {
            templateUrl: 'Products/inventoryIssue/SlipAssetIssue',
            controller: 'AssetIssueSlipBaseController'
        })
        
       
        .when('/inventory-issue-delete', {
            templateUrl: 'Products/inventoryIssue/IssueDelete',
            controller: 'inventoryIssueDeleteController'
        })

      
        .when('/PO-LC-Map', {
            templateUrl: 'Products/PurchaseOrder/POLCMap',
            controller: 'POLCMapController'
        })
    
                     
        .when('/purchase-document-acceptance', {
            templateUrl: 'Products/PurchaseDocumentsAcceptance/PurchaseDocAcceptance',
            controller: 'PurchaseDocumentAcceptanceController'
        })


        .when('/inventory-dashboard-delay-status', {
            templateUrl: 'Products/InventoryDashboard/aplos',
            controller: 'InventoryDashboardController'
        })

        .when('/inventory-dashboard-inventory-status', {
            templateUrl: 'Products/InventoryDashboard/InventoryStatus',
            controller: 'InventoryStatusDashboardController'
        })


        .when('/inventory-dashboard-status', {
            templateUrl: 'Products/InventoryDashboard/InventoryDashboardStatus',
            controller: 'InventoryDashboardStatusController'
        })

        .when('/material-ageing-dashboard', {
            templateUrl: 'Products/InventoryDashboard/MaterialAgeing',
            controller: 'materialAgeingDashboardController'
            
        })


        .when('/purchase-document-acceptance-post', {
            templateUrl: 'Products/PurchaseDocumentsAcceptance/PurchaseDocAcceptancePost',
            controller: 'PurchaseDocumentAcceptancePostController'
        })



        .when('/service-requisition-creation', {
            templateUrl: 'Products/ServiceRequisition/ServiceReqCreation',
            controller: 'ServiceRequisitionController'
        })
        .when('/service-po-acknowledgement', {
            templateUrl: 'Products/PurchaseOrder/ServicePoAcknowledgement',
            controller: 'ServicePoAcknowledgementController'
        })
        .when('/purchase-return', {
            templateUrl: 'Products/GoodsReceiveNote/PurchaseReturn',
            controller: 'PurchaseReturnController'
        })


        .when('/Physical-Stock-Adjustment', {
            templateUrl: 'Products/inventoryIssue/PhysicalStockAdjustment',
            controller: 'PhysicalStockAdjustmentMasterController'
        })

        .when('/gate-entry-register', {
            templateUrl: 'Products/GateentryToken/GateentryRegister',
            controller: 'GateentryTokenController'
        })

      
        .when('/inventory-sales', {
            templateUrl: 'Products/InventoryIssue/InventorySales',
            controller: 'inventorySalesController'
        })
        .when('/inventory-sales-Rnd', {
            templateUrl: 'Products/InventoryIssue/InventorySalesRnd',
            controller: 'inventorySalesController'
        })
          .when('/inventory-sales-Report', {
                templateUrl: 'Products/InventoryIssue/InventorySalesReport',
              controller: 'inventorySalesRegisterController'
          })
        .when('/sales-register', {
            templateUrl: 'Products/SalesRegister/SalesRegister',
            controller: 'salesRegisterController'
        })
        .when('/inventory-scrap', {
            templateUrl: 'Products/InventoryIssue/InventoryScrap',
            controller: 'inventoryScrapController'
        })

        .when('/inventory-scrap-report', {
            templateUrl: 'Products/InventoryIssue/InventoryScrapReport',
            controller: 'inventoryScrapController'
        })
        .when('/material-transfer', {
            templateUrl: 'Products/InventoryIssue/MaterialTransfer',
            controller: 'MaterialTransferController'
        })


        .when('/material-transfer-report', {
            templateUrl: 'Products/InventoryIssue/MaterialTransferRpt',
            controller: 'MaterialTransferController'
        })

        .when('/purchase-return-post', {
            templateUrl: 'Products/InventoryPurchaseReturn/InventoryPurchaseReturnPost',
            controller: 'purchaseReturnPostController'
        })

        .when('/GRN-Uncheck-Unapproved', {
            templateUrl: 'Products/GoodsReceiveNote/GRNUncheckedAndUnApproved',
            controller: 'GRNUncheckedAndUnApprovedController'
        })

        .when('/PO-Uncheck-Unapproved', {
            templateUrl: 'Products/PurchaseOrder/POUncheckedAndUnApproved',
            controller: 'POUncheckedAndUnApprovedController'
        })

        .when('/po-roll-back', {
            templateUrl: 'Products/PurchaseOrder/PORollBack',
            controller: 'PORollBackController'
        })
        .when('/inventory-sales-return', {
            templateUrl: 'Products/InventorySalesReturn/Aplos',
            controller: 'InventorySalesReturnController'
        })
        .when('/po-parameter', {
            templateUrl: 'Products/POParameterChange/Aplos',
            controller: 'POParameterChangeController'
        })
        .when('/grn-so-allocation', {
            templateUrl: 'Products/GoodsReceiveNote/GRNRequitionSOAllocation',
            controller: 'GRNRequisitionSOAllocationController'
        })
        .when('/foc', {
            templateUrl: 'Products/InventoryReceive/FOC',
            controller: 'FOCController'
        })

        .when('/inventory-issue-boq', {
            templateUrl: 'Products/inventoryIssue/inventoryIssueBOQ',
            controller: 'inventoryIssueBOQController'
        })

        .when('/inventory-sales-report-marketing', {
            templateUrl: 'Products/InventorySalesReportMarketing/Aplos',
            controller: 'InventorySalesReportMarketingController'
        })

        .when('/purchase-confirmation', {
            templateUrl: 'Products/inventoryReceiveAddition/PurchaseConfirmation',
            controller: 'PurchaseConfirmationController'
        })
        .when('/landed-cost-report', {
            templateUrl: 'Products/Landedcostreport/Report',
            controller: 'LandedcostreportController'
        })
        .when('/out-pass-register', {
            templateUrl: 'Products/OutPassRegister/Report',
            controller: 'OutPassRegisterController'
        })
        .when('/po-wise-material-issue', {
            templateUrl: 'Products/inventoryIssue/POWiseMaterialIssue',
            controller: 'POWiseMaterialIssueController'
        })
        .when('/service-acknowledgement', {
            templateUrl: 'Products/PurchaseOrder/ServiceAcknowledgement',
            controller: 'ServiceAcknowledgementController'
        })
        ;
}