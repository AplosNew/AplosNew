'use strict';
OrderCostingController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$controller', '$filter', 'cboService', '$window', 'fileReader'];
function OrderCostingController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $controller, $filter, $cboService, $window, fileReader) {
    $rootScope.title = 'Order Costing';
    $scope.ModelList = [];
    $scope.path = 'Costings/OrderCosting/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl);
    $scope.Action = 'Save';
    $scope.searchBy = "UserName"; $scope.searchBySO = "MasterOrderId"; $scope.searchSO = ''; $scope.search = "";

    $scope.partyType = "Vendor";
    $controller('partyBaseController', { $scope: $scope, $http: $http });

    $scope.piemarker = { dataLabel: { visible: true, shape: 'none', connectorLine: { type: 'bezier', color: 'black' }, font: { size: '14px' } } };

    $scope.CostingSummaryDataMain = { BuyerTotal: 0, QuickCostingValue: 0, OrderCostingValue: 0, ProcurementCostingValue: 0, ProfitQuickCosting: 0, ProfitOrderCosting: 0, ProfitProcurementCosting: 0 };
    $scope.CostingSummaryDataNew = Object.assign({}, $scope.CostingSummaryDataMain);

    $scope.CostingStage = '';
    $scope.tranCurrencyList = [];
    cboService.getCboTransactionCurrencyByCompany("", function (result) {
        $scope.tranCurrencyList = result;
    });

    $scope.closePopup = function (popupName) {
        angular.element(document.querySelector("#" + popupName + "")).modal("hide");
        try {
            $("#" + popupName).data("ejDialog").close();
        } catch (e) {

        }
    }
    $scope.openPopup = function (popupName) {

        try {
            $("#" + popupName).data("ejDialog").open();
        } catch (e) {

        }
    }
    $scope.openPopupAngular = function (popupName) {
        try {
            angular.element(document.querySelector("#" + popupName + "")).modal("show");
        } catch (e) {

        }

    }
    $scope.CostingStageList = [{ value: 'QuickCosting', name: 'Quick Costing' }, { value: 'PreCosting', name: 'Pre Costing' }, { value: 'ProcurementCosting', name: 'Procurement Costing' }]

    $scope.searchByList = [{ value: 'Id', name: "Id" }, { value: 'Code', name: "Code" }, { value: 'ShortName', name: "Short Name" }, { value: 'StandardName', name: "Standard Name" }, { value: 'UserName', name: "User Name" }, { value: 'Description', name: "Description" }, { value: 'Remarks', name: "Remarks" }];
    $scope.SOSearchByList = [{ value: 'MasterOrderId', name: "MasterOrderId" }, { value: 'MasterOrderItemId', name: "Master Order Item Id" }, { value: 'Customer', name: "Customer" }, { value: 'Product', name: "Product" }, { value: 'Material', name: "Material" }, { value: 'Article', name: "Article" }];

    $scope.searchInquiryByList = [{ value: 'Id', name: "Id" }, { value: 'InquirySource', name: "Inquiry Source" }, { value: 'Party', name: "Party" }, { value: 'Buyer', name: "Buyer" }, { value: 'BuyerBrand', name: "Buyer Brand" }, { value: 'BuyerDivision', name: "Buyer Division" }, { value: 'BuyerDepartment', name: "Buyer Department" }, { value: 'ResponsiblePerson', name: "Responsible Person" }];
    $scope.searchByInquiryColumn = 'InquirySource';
    $scope.searchInquiryText = '';
    $scope.ModelListInquiryMaster = [];
    $scope.ModelListInquiryItem = [];

    $scope.searchMasterOrderByList = [{ value: 'Id', name: "Id" }, { value: 'Party', name: "Party" }, { value: 'Buyer', name: "Buyer" }, { value: 'BuyerBrand', name: "Buyer Brand" }, { value: 'BuyerDivision', name: "Buyer Division" }, { value: 'BuyerDepartment', name: "Buyer Department" }, { value: 'ResponsiblePerson', name: "Responsible Person" }];
    $scope.searchByMasterOrderColumn = 'Id';
    $scope.searchMasterOrderText = '';
    $scope.ModelListMasterOrderMaster = [];
    $scope.ModelListMasterOrderItem = [];


    $scope.TotalSegmentedCostValue = 0;
    $scope.CostingComponentId = null;
    $scope.PurchaseGroupList = [];
    $scope.BOQCriteriaList = [];
    $scope.getData = function () {

        $http({
            method: 'POST',
            url: $scope.path + "GetList",
            data: { column: $scope.searchBy, value: $scope.search },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ModelList = response.data.DATA;
            for (var i = 0; i < $scope.ModelList.length; i++) {
                $scope.OrderCostingSId = $scope.ModelList[i].Id
            }
            $scope.PurchaseGroupList = response.data.PurchaseGroup;
        });


        $http({
            method: 'POST',
            url: "Costings/BOQCriteria/GetList",
            data: { column: $scope.searchBy, value: $scope.search },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.BOQCriteriaList = response.data;
        });

    }
    $scope.getData();
    $scope.InquiryOrOrder = 'Inquiry';
    $scope.getInquiryData = function () {

        $http({
            method: 'POST',
            url: $scope.path + "GetInquiryList",
            data: { column: $scope.searchByInquiryColumn, value: $scope.searchInquiryText },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ModelListInquiryMaster = response.data.DATA;
        });
    }
    $scope.getInquiryData();
    $scope.getInquiryItemData = function (args) {
        $scope.Clear();
        $scope.openPopup("InquiryItemPopUp");
        $scope.InquiryOrOrder = 'Inquiry';
        $http({
            method: 'POST',
            url: $scope.path + "GetInquiryItemList",
            data: { Id: args.data.Id },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ModelListInquiryItem = response.data.DATA;
        });
    }


    $scope.getMasterOrderData = function () {

        $http({
            method: 'POST',
            url: $scope.path + "GetMasterOrderList",
            data: { column: $scope.searchByMasterOrderColumn, value: $scope.searchMasterOrderText },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ModelListMasterOrderMaster = response.data.DATA;
        });
    }
    $scope.getMasterOrderData();
    $scope.getMasterOrderItemData = function (args) {
        $scope.Clear();
        $scope.InquiryOrOrder = 'Order';
        $scope.openPopup("MasterOrderItemPopUp");
        $http({
            method: 'POST',
            url: $scope.path + "GetMasterOrderItemList",
            data: { Id: args.data.Id },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.SOListForSearch = response.data.DATA;
        });
    }

    $scope.ProductUOM = [];
    //cboService.getUnitOfMeasurementCbo(function (response) {
    //    $scope.ProductUOM = response;

    //});
    $scope.GetUoMOrderCostingByProductMaster = function () {
        $http({
            method: 'GET',
            url: 'Costings/OrderCosting/GetProductUOMOrderCosting?ProductMasterId=' + $scope.ModelNew.ProductMasterId
        }).then(function successCallback(response) {
            $scope.ProductUOM = response.data;
        });
    };


    $scope.IsShowEntryForm = false;
    $scope.SelectedInquiry = {};
    $scope.SelectedMasterOrder = {};
    $scope.CreateNewPopUp = function (args) {
        $scope.tempModel.InquiryItemId = null;
        $scope.SelectedInquiry = {};
        $scope.tempModel.MasterOrderItemId = null;
        $scope.SelectedMasterOrder = {};
        $scope.ModelNew.ProductMasterId = args.ProductMasterId;
        if ($scope.InquiryOrOrder == 'Inquiry') {
            $scope.tempModel.InquiryItemId = args.Id;
            $scope.SelectedInquiry = args;
        }
        else {
            $scope.tempModel.MasterOrderItemId = args.Id;
            $scope.SelectedMasterOrder = args;
        }
        $scope.tempModel.CostingStage = 'QuickCosting';


        $scope.openPopupAngular('SalesOrderTagPopUpAddNew');
        $scope.getSalesOrderData();
    }

    $scope.CreateNewPopUpSO = function () {

        $scope.SOTemplateList = ej.DataManager($scope.SOListForSearch).executeLocal(ej.Query().where("isChecked", "equal", true));
        if ($scope.SOTemplateList.length == 0) {
            ShowResult("Please select at least one master order item to proceed", 'failure');
            return;
        }

        $scope.message_confirmation = "Do you want to create from template ?";
        $("#CreateNewPopUp").data("ejDialog").open();
        $scope.closePopup("SalesOrderTagPopUpAddNew");

    }

    $scope.ModelList1 = [];
    $scope.ModelList2 = [];
    $scope.CopySource = '';//'TEMPLATE/ORDER
    $scope.getCostingPopUp = function (sourceFlag) {
        $scope.CopySource = sourceFlag;
        $scope.ModelList1 = [];
        $scope.ModelList2 = [];
        var _url = $scope.path + 'GetListCostingTemplateForCopy';
        if (sourceFlag == 'ORDER')
            _url = $scope.path + 'GetListOrderCostingForCopy';
        $http({
            method: 'POST',
            url: _url,
            data: { column: $scope.searchBy, value: $scope.search, ProductMasterId: $scope.ModelNew.ProductMasterId },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if ($scope.CopySource == 'ORDER')
                $scope.ModelList2 = response.data.DATA;
            else
                $scope.ModelList1 = response.data.DATA;
        });
        angular.element(document.querySelector('#CostingTemplatePopUp')).modal('show');
    }


    $scope.NewCosting = function () {

        $scope.IsShowEntryForm = true; $scope.ModelNew.CostingStage = $scope.tempModel.CostingStage;
        $scope.ProductMasterDetail($scope.ModelNew.ProductMasterId); $scope.GetCostingComponentByProductMasterId($scope.ModelNew.ProductMasterId)
        $scope.closePopup('CreateNewPopUp');
        $scope.closePopup('InquiryItemPopUp');
        $scope.closePopup('MasterOrderItemPopUp');
        $scope.closePopup('orderCostingUniqueFieldpopUp');
        $scope.Action = 'Create';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    }
    $scope.tempModel = {};
    $scope.toCopyPopup = function (args) {

        var selectedCostingStage = $scope.tempModel.CostingStage;
        $scope.tempModel = args.data;
        $scope.tempModel.CostingMasterTemplateId = $scope.tempModel.Id;
        $scope.tempModel['CostingStage'] = selectedCostingStage;


        $scope.tempModel.InquiryItemId = null;
        $scope.tempModel.MasterOrderItemId = null;
        if ($scope.InquiryOrOrder == 'Inquiry')
            $scope.tempModel.InquiryItemId = $scope.SelectedInquiry.Id;
        else
            $scope.tempModel.MasterOrderItemId = $scope.SelectedMasterOrder.Id;


        $scope.tempModel.Id = null;
        $scope.message_confirmation = "Are you sure ?";

        //
        angular.element(document.querySelector("#costingTemplateCopy")).modal("show");
    };
    $scope.showOrdercostingUniqueFieldFormPopUp = function () {

        angular.element(document.querySelector('#CostingTemplatePopUp')).modal('hide');
        $scope.openPopup('orderCostingUniqueFieldpopUp');

    }
    $scope.closeCostingPopUp = function () {
        //$scope.tempModel = Object.assign({}, $scope.tempModelNew);

        angular.element(document.querySelector('#CostingTemplatePopUp')).modal('hide');
        angular.element(document.querySelector('#orderCostingUniqueFieldpopUp')).modal('hide');
        //$scope.showOrderCostingForm();
        //$scope.GetOrderModelList();
    }
    $scope.CopyCosting = function () {

        var _url = $scope.path + 'CopyCostingTemplate';
        if ($scope.CopySource == 'ORDER')
            _url = $scope.path + 'CopyOrderCosting';

        $http({
            method: 'POST',
            url: _url,
            dataType: 'JSON',
            data: { CopyData: $scope.tempModel, SalesOrderList: $scope.SOTemplateList }
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.Get(response);
                $scope.tempModel = {};
                angular.element(document.querySelector('#orderCostingUniqueFieldpopUp')).modal('hide');

                $scope.getData();
            }
            function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        });

    }


    $scope.SOListForSearch = [];
    $scope.getSalesOrderData = function () {
        $scope.SOListForSearch = [];
        $http({
            method: 'POST',
            url: $scope.path + "GetSOList",
            data: { column: $scope.searchBySO, value: $scope.searchSO, TemplateId: $scope.OrderCostingMasterTemplateId, MasterOrderItemId: $scope.tempModel.MasterOrderItemId },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            for (var i = 0; i < response.data.length; i++) {
                try {
                    response.data.DeliveryDate = new Date(response.data.DeliveryDate);
                } catch (e) {

                }
            }
            $scope.SOListForSearch = response.data;
        });
    }
    $scope.showSalesOrderTagPopUp = function () {

        angular.element(document.querySelector("#SalesOrderTagPopUp")).modal("show");
        $scope.getSalesOrderData();
    }

    $scope.SOTemplateList = [];
    $scope.GetSOListForTemplate = function () {
        $scope.SOTemplateList = [];

        $http({
            method: 'POST',
            url: $scope.path + "GetSOListForTemplate",
            data: { TemplateId: $scope.OrderCostingMasterTemplateId },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            for (var i = 0; i < response.data.length; i++) {
                try {
                    response.data.DeliveryDate = new Date(response.data.DeliveryDate);
                } catch (e) {

                }
            }
            $scope.SOTemplateList = response.data;
        });
    }
    $scope.SaveSalesOrder = function () {

        var saveList = ej.DataManager($scope.SOListForSearch).executeLocal(ej.Query().where("isChecked", "equal", true));

        $http({
            method: 'POST',
            url: $scope.path + "UpdateSOData",
            data: { TemplateId: $scope.OrderCostingMasterTemplateId, SOList: saveList },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.GetSOListForTemplate();
            angular.element(document.querySelector("#SalesOrderTagPopUp")).modal("hide");

        });


    }

    $scope.DeleteSalesOrderData = {};
    $scope.DeleteSalesOrderConfirm = function (args) {
        $scope.DeleteSalesOrderData = args;
        $scope.message_confirmation = "Are you sure to Delete permanently ?";
        angular.element(document.querySelector("#confirmSODelete")).modal("show");

    }
    $scope.DeleteSalesOrder = function () {

        $http({
            method: 'POST',
            url: $scope.path + "DeleteSOData",
            data: { TemplateId: $scope.OrderCostingMasterTemplateId, SOId: $scope.DeleteSalesOrderData.data.MasterOrderItemId },
            dataType: 'JSON'
        }).then(function successCallback(response) {

            $scope.GetSOListForTemplate();
        });
    }


    $scope.GetDirectProcessRateValue = function (args) {
        if (args == null)
            return;

        $http({
            method: 'POST',
            url: $scope.path + "GetDirectProcessRateValue",
            data: { CostingItemId: args.CostingItemId },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            args.Value = response.data[0].ValueLossPercentage;
            if (args.ExecutionType == 'Inside')
                args.Rate = response.data[0].InternalRate;
            else
                args.Rate = response.data[0].ExternalRate;


            $scope.CalculateFinalCosting(args);
        });


    }

    $scope.UpChargeMatrix = [];
    $scope.calculateSPT = function () {
        if ($scope.ModelNew.TargetOrSPT == 'SPT') {
            $scope.ModelNew.MKTTargetPerHour = 0;
            if ($scope.ModelNew.SPT > 0) {
                $scope.ModelNew.MKTTargetPerHour = $scope.ModelNew.NoOfWorkstation * 60 / $scope.ModelNew.SPT * ($scope.ModelNew.EfficiencyPercentage / 100);
                $scope.ModelNew.MKTTargetPerHour = $scope.ModelNew.MKTTargetPerHour.toFixed(2);
            }
        }
        else {
            $scope.ModelNew.SPT = 0;
            if ($scope.ModelNew.MKTTargetPerHour > 0 && $scope.ModelNew.EfficiencyPercentage > 0) {
                $scope.ModelNew.SPT = ($scope.ModelNew.NoOfWorkstation * 60) / ($scope.ModelNew.MKTTargetPerHour / ($scope.ModelNew.EfficiencyPercentage / 100));
                $scope.ModelNew.SPT = $scope.ModelNew.SPT.toFixed(2);
            }

        }

        $scope.ModelNew.WorkCenterTargetPerDay = ($scope.ModelNew.MKTTargetPerHour * $scope.ModelNew.StandardWorkingHours).toFixed(2);

        var costingItem = ej.DataManager($scope.OrderCostingItemList).executeLocal(ej.Query().where("Code", "equal", 'CM'));
        var CMValue = 0;
        if (costingItem.length > 0) {
            costingItem[0].Value = 0;
            costingItem[0].TotalGrossAmount = 0;

            if ($scope.ModelNew.MKTTargetPerHour > 0) {
                var additionalCost = 0;

                if ($scope.ModelNew.StandardWorkingHours > $scope.ModelNew.StandardWorkingHoursForProduct) {
                    additionalCost = ($scope.ModelNew.StandardWorkingHours - $scope.ModelNew.StandardWorkingHoursForProduct) * $scope.ModelNew.AdditionalWorkingHourCostPerHour;
                }


                CMValue = ($scope.ModelNew.StandardWorkingHourCost + additionalCost) /
                    $scope.ModelNew.StandardWorkingHours /
                    $scope.ModelNew.MKTTargetPerHour;
            }
            costingItem[0].Value = CMValue;
            costingItem[0].TotalGrossAmount = CMValue;
        }
        var costingItem = ej.DataManager($scope.OrderCostingItemList).executeLocal(ej.Query().where("Code", "equal", 'UPC'));
        if (costingItem.length > 0) {
            costingItem[0].Value = 0;
            costingItem[0].TotalGrossAmount = 0;

            try {

                var _workdays = $scope.ModelNew.OrderSize /
                    $scope.ModelNew.StandardWorkingHours /
                    $scope.ModelNew.MKTTargetPerHour;

                var WorkDysRequired = Math.ceil(_workdays);

                var UpChargeMatrix = ej.DataManager($scope.UpChargeMatrix).executeLocal(ej.Query().where("WorkCenterDays", "equal", WorkDysRequired));
                if (UpChargeMatrix.length == 0)
                    UpChargeMatrix = ej.DataManager($scope.UpChargeMatrix).executeLocal(ej.Query().where("WorkCenterDays", "lessorequal", WorkDysRequired));

                if (UpChargeMatrix.length > 0) {
                    CMValue = CMValue * (UpChargeMatrix[0][$scope.ModelNew.CriticalLevel] / 100);
                }

                costingItem[0].Value = CMValue;
                costingItem[0].TotalGrossAmount = CMValue;

            } catch (e) {

            }
        }
        $scope.CalculateFinalCosting(null);

        $scope.calculateSPTForProcurementCosting();
    }
    $scope.calculateSPTForProcurementCosting = function () {
        if ($scope.ModelNew.TargetOrSPT == 'SPT') {
            $scope.ModelNew.MKTTargetPerHour = 0;
            if ($scope.ModelNew.SPT > 0) {
                $scope.ModelNew.MKTTargetPerHour = $scope.ModelNew.NoOfWorkstation * 60 / $scope.ModelNew.SPT * ($scope.ModelNew.EfficiencyPercentage / 100);
                $scope.ModelNew.MKTTargetPerHour = $scope.ModelNew.MKTTargetPerHour.toFixed(2);
            }
        }
        else {
            $scope.ModelNew.SPT = 0;
            if ($scope.ModelNew.MKTTargetPerHour > 0 && $scope.ModelNew.EfficiencyPercentage > 0) {
                $scope.ModelNew.SPT = ($scope.ModelNew.NoOfWorkstation * 60) / ($scope.ModelNew.MKTTargetPerHour / ($scope.ModelNew.EfficiencyPercentage / 100));
                $scope.ModelNew.SPT = $scope.ModelNew.SPT.toFixed(2);
            }

        }

        $scope.ModelNew.WorkCenterTargetPerDay = ($scope.ModelNew.MKTTargetPerHour * $scope.ModelNew.StandardWorkingHours).toFixed(2);

        var costingItem = ej.DataManager($scope.OrderCostingItemList).executeLocal(ej.Query().where("Code", "equal", 'CM'));
        var CMValue = 0;
        if (costingItem.length > 0) {
            costingItem[0].Value = 0;
            costingItem[0].TotalProcurementGrossAmount = 0;

            if ($scope.ModelNew.MKTTargetPerHour > 0) {
                var additionalCost = 0;

                if ($scope.ModelNew.StandardWorkingHours > $scope.ModelNew.StandardWorkingHoursForProduct) {
                    additionalCost = ($scope.ModelNew.StandardWorkingHours - $scope.ModelNew.StandardWorkingHoursForProduct) * $scope.ModelNew.AdditionalWorkingHourCostPerHour;
                }


                CMValue = ($scope.ModelNew.StandardWorkingHourCost + additionalCost) /
                    $scope.ModelNew.StandardWorkingHours /
                    $scope.ModelNew.MKTTargetPerHour;
            }
            costingItem[0].Value = CMValue;
            costingItem[0].TotalProcurementGrossAmount = CMValue;
        }
        var costingItem = ej.DataManager($scope.OrderCostingItemList).executeLocal(ej.Query().where("Code", "equal", 'UPC'));
        if (costingItem.length > 0) {
            costingItem[0].Value = 0;
            costingItem[0].TotalProcurementGrossAmount = 0;

            try {

                var _workdays = $scope.ModelNew.OrderSize /
                    $scope.ModelNew.StandardWorkingHours /
                    $scope.ModelNew.MKTTargetPerHour;

                var WorkDysRequired = Math.ceil(_workdays);

                var UpChargeMatrix = ej.DataManager($scope.UpChargeMatrix).executeLocal(ej.Query().where("WorkCenterDays", "equal", WorkDysRequired));
                if (UpChargeMatrix.length == 0)
                    UpChargeMatrix = ej.DataManager($scope.UpChargeMatrix).executeLocal(ej.Query().where("WorkCenterDays", "lessorequal", WorkDysRequired));

                if (UpChargeMatrix.length > 0) {
                    CMValue = CMValue * (UpChargeMatrix[0][$scope.ModelNew.CriticalLevel] / 100);
                }

                costingItem[0].Value = CMValue;
                costingItem[0].TotalProcurementGrossAmount = CMValue;

            } catch (e) {

            }
        }
        $scope.CalculateFinalCostingProcurement(null);

    }
    $scope.CostingItemList = [];
    $scope.AddNewCostingItem = function () {
        try {
            if (angular.isUndefinedOrNull($scope.OrderCostingMasterTemplateId))
                throw 'Please save the costing master first';
            $scope.$broadcast('show-errors-check-validity');
            if ($scope.ModelNewForm.$valid) {
                $http({
                    method: 'POST',
                    url: $scope.path + "GetCostingItemForSelection",
                    data: { CostingStage: $scope.CostingStage, OrderCostingMasterTemplateId: $scope.OrderCostingMasterTemplateId, costingComponentId: $scope.CostingComponentId, Segment: $scope.Segment },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    $scope.CostingItemList = response.data;
                });
            }
        } catch (e) {
            ShowResult(e, "failure");
        }
    }


    $scope.SaveCostingItemsForCostingComponent = function () {
        try {
            if (angular.isUndefinedOrNull($scope.OrderCostingMasterTemplateId))
                throw 'Please save the costing master first';

            $http({
                method: 'POST',
                url: $scope.path + "SaveCostingItemsForCostingComponent",
                data: { CostingStage: $scope.CostingStage, itemList: $scope.CostingItemList, OrderCostingMasterTemplateId: $scope.OrderCostingMasterTemplateId, costingComponentId: $scope.CostingComponentId, Segment: $scope.Segment },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                $scope.AfterAddRemoveCostingItem();

            });
        } catch (e) {
            ShowResult(e, "failure");
        }

    }


    $scope.OrderCostingMasterTemplateId = null;
    $scope.CostingVersionMasterTemplateId = null;
    $scope.ModelMain = {
        Id: null,
        Sequence: 0,
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        Description: null,
        Remarks: null,
        Active: true,
        CustomerId: null,
        Customer: null,
        ProductMasterId: null,
        SpecifyTo: null,
        OrderSize: null,
        ProductionAvailableDays: null,
        MKTTargetPerHour: null,
        TargetSellingPrice: 0,
        PaymentDays: null,
        PackingType: null,
        EstNoOfPackingList: null,
        ExcessShipmentPer: null,
        FileName: null,
        ProductCategory: null,
        ProductSubCategory: null,
        CostingType: null,
        CostingTypeName: null,
        TargetCM: 0,
        TargetProfit: 0,
        IsPercentage: 'true',
        CostingStage: null,
        isDirectApproval: false,

        InquiryItemId: null,
        MasterOrderItemId: null,

        UOM: null,
        TargetOrSPT: 'SPT',
        CriticalLevel: null,
        SPT: 0,
        NoOfWorkstation: 0,
        EfficiencyPercentage: 0,
        StandardWorkingHours: 0,
        StandardWorkingHoursForProduct: 0,
        WorkCenterTargetPerDay: 0,
        StandardWorkingHourCost: 0,
        AdditionalWorkingHourCostPerHour: 0,
        //IsApprovalApplicable: false,
        //ApproveByWhomId: null
    };
    $scope.ModelNew = Object.assign({}, $scope.ModelMain);

    //$scope.checkApprovalApplicable = function () {
    //    if ($scope.ModelNew.IsApprovalApplicable)
    //        $scope.ModelNew.ApproveByWhomId = $scope.ModelNew.ApproveByWhomId;
    //    else
    //        $scope.ModelNew.ApproveByWhomId = null;
    //}

    //$scope.approveByWhomList = [];
    //$scope.GetApproveByWhom = function () {
    //    $http({
    //        method: 'GET',
    //        url: 'Costings/OrderCosting/GetApprovedBY',
    //        dataType: 'JSON'
    //    }).then(function successCallback(response) {
    //        $scope.approveByWhomList = response.data;
    //    });
    //}
    //$scope.GetApproveByWhom();

    $scope.OrderPreCostingDirectMaterial = {
        Id: null,
        CostingItemId: null,
        Consumption: 0,
        UOM: 0,
        Rate: 0,
        ValueLoss: 0,
        GrossConsumption: 0,
        GrossAmount: 0

    };
    $scope.VersionModelNew = {
        Id: null,
        OrderCostingMasterTemplateId: null,
        Version: 1,
        Description: null
    };
    $scope.VersionDetailModelNew = {
        Id: null,
        CostingComponentId: null,
        CostingVersionMasterTemplateId: null,
        Sequence: null,
        CostingValue: null,
        //Code: null,
        CostingComponent: null,
        BuyerTarget: 0.00,


    };

    $scope.SpecifyToList = [];
    $scope.PackingTypeList = [];

    cboService.getEnumCbo("enum/getSpecifyToEnumCbo", function (result) {
        $scope.SpecifyToList = result;
    });
    cboService.getEnumCbo("enum/getPackingTypeEnumCbo", function (result) {
        $scope.PackingTypeList = result;
    });
    $scope.ProductMasterList = [];
    cboService.getProductMasterCbo(function (result) {
        $scope.ProductMasterList = result.Rows;
    });

    $scope.IsCustomer = false;
    $scope.CheckeSpecifyTo = function (SpecifyTo) {
        $scope.IsCustomer = false;
        if ($scope.ModelNew.SpecifyTo == "Customer")
            $scope.IsCustomer = true;
    }

    $scope.CostingStageChangeList = [];
    $scope.editCostingArgs = null;
    $scope.editCosting = function (args) {
        $scope.editCostingArgs = args;

        $scope.CostingStageChangeList = [];
        for (var m = 0; m < $scope.CostingStageList.length; m++) {
            if (args.data.CostingStage == $scope.CostingStageList[m].value) {
                for (var i = m + 1; i < $scope.CostingStageList.length; i++) {
                    $scope.CostingStageChangeList.push(Object.assign({}, $scope.CostingStageList[i]));
                }
            }
        }
        $scope.tempModel.CostingStage = null;
        if ($scope.CostingStageChangeList.length > 0)
            $scope.tempModel.CostingStage = $scope.CostingStageChangeList[0].value;


        $scope.openPopup('EditNewPopUp');
    }
    $scope.CreateNewVersion = function () {
        $http({
            method: 'POST',
            url: $scope.path + "ChanageVersion",
            data: { TemplateId: $scope.editCostingArgs.data.Id },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.Get($scope.editCostingArgs);
        });
    }

    $scope.validateEditCostingStage = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.tempModel.CostingStage)) {
                throw "You might be already at the last stage(Procurement Costing) of the order costing therefore cannot change the costing stage";
            }
            $scope.openPopupAngular('changeCostingStageConfirmation');

        } catch (e) {
            ShowResult(e, "failure");
        }

    }
    $scope.ChangeCostingStageForEdit = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.tempModel.CostingStage)) {
                throw "Plase select costing stage";
            }
            $http({
                method: 'POST',
                url: $scope.path + "ChangeCostingStage",
                data: { TemplateId: $scope.editCostingArgs.data.Id, CostingStage: $scope.tempModel.CostingStage },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                $scope.editCostingArgs.data.CostingStage = $scope.tempModel.CostingStage;
                $scope.Get($scope.editCostingArgs);
            });

        } catch (e) {
            ShowResult(e, "failure");
        }
    }

    $scope.Get = function (args) {
        $scope.IsShowEntryForm = true;

        $http({
            method: 'POST',
            url: $scope.path + "GetListItem",
            data: { Id: args.data.Id },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.BackToOrderCostingComponent();
            $scope.ModelNew = response.data[0];


            if ($scope.ModelNew.SpecifyTo == 'Customer')
                $scope.IsCustomer = true;
            if ($scope.ModelNew.IsPercentage == true) {
                $scope.ModelNew.IsPercentage = 'true';
            }
            else {
                $scope.ModelNew.IsPercentage = 'false';
            }


            $scope.CheckeSpecifyTo();

            var str = $scope.ModelNew.FileName;
            if (!baseService.isUndefinedOrNull(str)) {
                var extention = str.substr(str.indexOf('.'));
                $scope.imageSrc = virtualPath.OrderCostingImagePath + '/' + $scope.ModelNew.Id + extention;

                $scope.filedata = $scope.ModelNew.FileName;
            }

            $scope.OrderCostingMasterTemplateId = $scope.ModelNew.Id;


            $scope.getLatestVersion();
            $scope.SumCostingValue();
            $scope.CalculateProfit();
            $scope.GetUoMOrderCostingByProductMaster();
            $scope.GetSOListForTemplate();
            //$scope.AssignSegmentByeDirectMaterial();
            $scope.closePopup('InquiryItemPopUp'); $scope.closePopup('MasterOrderItemPopUp'); $scope.closePopup('orderCostingUniqueFieldpopUp');
            $scope.Action = 'Update';
            if (!$rootScope.isCollapsed) {
                $rootScope.toggle();
            }
        });
    };



    //$scope.ReportPopUp = function () {


    //    try {
    //        $scope.openPopup('CostingPopUp');

    //    } catch (e) {

    //    }
    //}

    //$scope.OrderPreCostingReport = function (args) {
    //    try {

    //        $scope.OrderCostingId = args.data.Id;
    //        $scope.ProductMasterId = args.data.ProductMasterId;
    //        $scope.PreCosting = 1;
    //        var file_src = $scope.path + 'GetOrderCostingReport?OrderCostingId=' + $scope.OrderCostingId + '&ProductMasterId=' + $scope.ProductMasterId + '&preCosting=' + $scope.PreCosting ;
    //        $rootScope.report(file_src);

    //    } catch (e) {
    //    }
    //}
    //$scope.OrderProcurementCostingReport = function (args) {
    //    try {

    //        $scope.OrderCostingId = args.data.Id;
    //        $scope.ProductMasterId = args.data.ProductMasterId;
    //        $scope.ProcurementCosting = 1;

    //        var file_src = $scope.path + 'GetOrderCostingReport?OrderCostingId=' + $scope.OrderCostingId + '&ProductMasterId=' + $scope.ProductMasterId + '&procurementCosting=' + $scope.ProcurementCosting;
    //        $rootScope.report(file_src);

    //    } catch (e) {
    //    }
    //}



    $scope.picdata = null;
    $("#uploadImage").change(function () {
        $scope.picdata = this.files[0];
    });

    $scope.getFile = function () {
        $scope.progress = 0;
        fileReader.readAsDataUrl($scope.file, $scope)
            .then(function (result) {
                $scope.imageSrc = result;
            });
    };

    $scope.Save = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.ModelNew.ProductMasterId)) {
                throw "Plase select a product";
            }
            if (baseService.isUndefinedOrNull($scope.ModelNew.CostingType)) {
                throw "Costing type is missing for the product";
            }
            if ($scope.ModelNew.SpecifyTo == "Customer") {
                if (baseService.isUndefinedOrNull($scope.ModelNew.CustomerId)) {
                    throw "Customer is required";
                }
            }
            if (baseService.isUndefinedOrNull($scope.ModelNew.CurrencyId)) {
                throw "Currency is required";
            }
            $scope.$broadcast('show-errors-check-validity');
            if ($scope.ModelNewForm.$valid) {
                var picData = new FormData();
                if (!baseService.isUndefinedOrNull($scope.picdata)) {
                    $scope.ModelNew.FileName = $scope.picdata.name;
                }
                $scope.BackToOrderCostingComponent();

                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    headers: { 'Content-Type': undefined },
                    transformRequest: function (data) {
                        picData.append("modelNew", angular.toJson(data.modelNew));
                        picData.append("OrderCostingData", angular.toJson(data.OrderCostingData));
                        picData.append("SalesOrderData", angular.toJson(data.SalesOrderData))
                        if (baseService.isUndefinedOrNull($scope.picdata) === false) {
                            picData.append('file', data.file);
                        }
                        return picData;
                    },
                    data: { 'modelNew': $scope.ModelNew, 'file': $scope.picdata, 'OrderCostingData': $scope.OrderCostingDetailList, 'SalesOrderData': $scope.SOTemplateList }
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, "failure");

                    }
                    else {
                        $scope.Action = 'Update';
                        ShowResult(response.data.Message, 'success');
                        $scope.ModelNew.Id = response.data.data.Id;
                        $scope.OrderCostingMasterTemplateId = response.data.data.Id;
                        // ClearFields(response.data.Sequence);
                        $scope.getData();
                        //$scope.getVersion();
                        $scope.getLatestVersion();
                    }
                }, function errorCallback(response) {

                });
                return true;

            }
        } catch (e) {

            ShowResult(e, "failure");
        }
    };


    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.ModelNew.Id)) {

            $scope.BackToOrderCostingComponent();

            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.ModelNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    ClearFields(response.data.Sequence);
                    $scope.getData();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };

    $scope.Clear = function () {
        $scope.IsShowEntryForm = false;

        ClearFields();

        $scope.buyerList = [];
        return true;
    };

    function ClearFields() {
        $scope.IsShowEntryForm = false;
        $scope.CostingVersionMasterTemplateId = null;
        $scope.Status = 0;
        $scope.Action = 'Save';
        $scope.ModelNew = Object.assign({}, $scope.ModelMain);
        $scope.ModelNew.Id = null;
        $scope.VersionModelNew = {};
        $scope.ModelNew.Active = true;
        $scope.VersionDetailModelNew = {};
        $scope.CostingDetailList = [];
        $scope.SelectedOrderCostingComponent = '';
        $scope.SummaryBySegmentList = [];
        $scope.CostingSummaryDataNew = Object.assign({}, $scope.CostingSummaryDataMain);
        //$("#graphdivComparison").ejChart("redraw");
        //$("#graphdivBuyerTarget").ejChart("redraw");
        //$("#graphdivOrderCosting").ejChart("redraw");
        //$("#graphdivOrderCosting").ejChart("redraw");
        $scope.tabQ = 1;

        var chartObj = $("#graphdivComparison").data("ejChart");
        chartObj.redraw();
        chartObj = $("#graphdivBuyerTarget").data("ejChart");
        chartObj.redraw();
        chartObj = $("#graphdivOrderCosting").data("ejChart");
        chartObj.redraw();
        chartObj = $("#graphdivOrderCosting").data("ejChart");
        chartObj.redraw();

        $scope.Segment = '';
        $scope.CostingComponentId = '';
        $scope.DirectMaterialList = [];
        $scope.OperationList = [];
        $scope.DirectProcessList = [];
        $scope.SalesExpenseList = [];
        $scope.ValueLossList = [];
        $scope.ProfitList = [];


        $scope.DirectProcurementCostingMaterialList = [];
        $scope.OperationProcurementCostingList = [];
        $scope.DirectProcessProcurementCostingList = [];
        $scope.SalesExpenseProcurementCostingList = [];
        $scope.ValueLossProcurementCostingList = [];
        $scope.ProfitProcurementCostingList = [];


        $scope.OrderCostingDetailList = [];
        $scope.VersionList = [];
        $scope.VersionModelNew.Version = 1;
        $scope.ModelNew.IsPercentage = 'true';
        $scope.picData = null;
        $scope.IsCustomer = false;
        $scope.imageSrc = '';
        document.getElementById("uploadImage").value = '';
        document.getElementById("uploadImageSrc").setAttribute('src', null);

        $scope.SumCostingValue();
        $scope.CalculateProfit();
    }

    //#region Customer info
    $scope.partySearchByList = [
        {
            'name': $scope.partyType + ' Code',
            'value': 'Code'
        },
        {
            'name': $scope.partyType + ' Name',
            'value': 'PartyName'
        },
        {
            'name': 'Account Group',
            'value': 'PartyAccountGroupName'
        },
        {
            'name': 'Country',
            'value': 'CountryName'
        },
        {
            'name': 'State',
            'value': 'StateName'
        },
        {
            'name': 'Currency',
            'value': 'CurrencyCode'
        }
    ];
    $scope.partyParameters = {
        limit: 10
        , offset: 0
        , order: 'ASC'
        , sort: 'PartyName, PartyAccountGroupName'
        , searchBy: 'PartyName'
        , pageSize: 10
        , total_count: 0
        , search: null
        , serverPagination: true
    };
    $scope.partyList = [];
    $scope.SelectedMaterialRow = {};
    $scope.MaterialVendorPopUp = function (index, entryStage) {
        $scope.CostingStage = entryStage;
        if ($scope.CostingStage == 'PRE')
            $scope.SelectedMaterialRow = $scope.DirectMaterialList[index];
        else if ($scope.CostingStage == 'PROCUREMENT')
            $scope.SelectedMaterialRow = $scope.DirectProcurementCostingMaterialList[index];
        $scope.showPartyPopUp('Vendor');
    }
    $scope.showPartyPopUp = function (ptype) {
        $scope.partyType = ptype == null ? 'Vendor' : ptype;
        $scope.partyList = [];
        $scope.getPartyList = function () {

            //$scope.partyUrl = 'Parties/party/GetCompanyPartyDataList/' + 'GetCompanyPartyDataList?companyId=' + $window.companyId + '&PlantId=' + $window.plantId + '&partyType=' + $scope.partyType;
            $scope.partyUrl = 'Parties/party/GetCompanyPartyDataListNew?partyType=' + $scope.partyType;
            //baseService.paginationBase($scope.partyUrl, pageno, $scope.partyParameters)
            //    .then(function (result) {
            //        $scope.partyList = result.Rows;
            //        $scope.partyParameters.total_count = result.Total;
            //    }, function () {
            //        ShowResult(commonMessage.NetworkError, 'failure');
            //    }).finally(function () {
            //    });

            $http({
                method: 'POST',
                url: $scope.partyUrl,
                data: { column: $scope.searchByParty, value: $scope.searchParty },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                $scope.partyList = response.data;
            });
        };
        angular.element(document.querySelector('#partyPopUpN')).modal('show');
        $scope.getPartyList();
    };

    $scope.selectPartyPopUpRow = function (index, id) {
        $scope.partyIndex = index;
        $scope.selectedParty = id;
    };

    $scope.selectCustomerPopUp = function (index, id) {
        $scope.partyIndex = index;
        $scope.selectedCustomer = id;
    };

    //$scope.closePartyPopUp = function () {
    //    if ($scope.partyIndex !== -1) {
    //        var party = $scope.partyList[$scope.partyIndex];
    //        if ($scope.partyType == 'Customer') {
    //            $scope.ModelNew.Customer = party.UserName;
    //            $scope.ModelNew.CustomerId = party.Id;
    //        }
    //        else {

    //            $scope.SelectedMaterialRow.Vendor = party.UserName;
    //            $scope.SelectedMaterialRow.VendorId = party.Id;

    //        }

    //        angular.element(document.querySelector('#partyPopUp')).modal('hide');

    //    }

    //};

    $scope.closePartyPopUpN = function (args) {
        var party = args.data;
        $scope.SelectedMaterialRow.Vendor = party.UserName;
        $scope.SelectedMaterialRow.VendorId = party.Id;
        angular.element(document.querySelector('#partyPopUpN')).modal('hide');
    };

    $scope.closePartyPopUp = function () {
        angular.element(document.querySelector('#partyPopUpN')).modal('hide');
    }

    //#endregion End customer info

    $scope.getLatestVersion = function () {
        try {
            $http({
                method: 'GET',
                url: $scope.path + "GetVersion?OrderCostingMasterTemplateId=" + $scope.ModelNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {

                $scope.onChengeProductMaster();

            });
        } catch (e) {
            ShowResult(e.Message, 'failure');
        }
    }








    $scope.CostingSubCategoryList = [];
    $scope.getCostingSubCategory = function () {
        try {
            $http({
                method: 'GET',
                url: $scope.path + "getCostingSubCategory",
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    $scope.CostingSubCategoryList = response.data;
                }
            }, function errorCallback(response) {
                ShowResult(response.data.Message, 'failure');
            });
        } catch (e) {

        }
    }




    $scope.SaveOrderCostingVersion = function () {
        //angular.element(document.querySelector("#confirmPostPopUp")).modal("hide");
        //if (baseService.isUndefinedOrNull($scope.VersionModelNew.Description)) {
        //    ShowResult('Description cannot empty', 'failure');
        //    return;
        //}
        $scope.VersionModelNew.OrderCostingMasterTemplateId = $scope.ModelNew.Id;
        $http({

            method: 'POST',
            url: 'Costings/OrderCosting/CreateCostingDetail',
            data: { "VersionModelNew": $scope.VersionModelNew, "data": $scope.OrderCostingDetailList },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                //$scope.getVersion();
                $scope.getData();
                $scope.getLatestVersion();

            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        }
    };



    $scope.OrderCostingDetailId = null;

    $scope.ShowCostingSubCategoryPopUp = function () {
        $scope.getCostingSubCategory();
        $("#CostingSubCategoryPoUp").ejDialog();
        var eDialog = $("#CostingSubCategoryPoUp").data("ejDialog");
        eDialog.open();

        var gridObj = $("#GridOperation").data("ejGrid");
        gridObj.clearFiltering();  // clears all the filtering

    };
    $scope.CloseCostingSubCategoryPopUp = function (args) {
        $scope.VersionDetailModelNew.CostingSubCategory = args.data.UserName;
        $scope.VersionDetailModelNew.Code = args.data.Code;
        $scope.VersionDetailModelNew.CostingSubCategoryId = args.data.Id;
        $("#CostingSubCategoryPoUp").ejDialog();
        var eDialog = $("#CostingSubCategoryPoUp").data("ejDialog");
        eDialog.close();
    };

    //endregion Version

    //delete costingDetail
    $scope.valuePassInActivityFormDelModal = function (index, data) {
        $scope.buyerMsterActivityId = data.Id;
        $scope.bActivityIndex = index;
        if (baseService.isUndefinedOrNull($scope.buyerMsterActivityId))
            $scope.message_confirmation = 'Are you sure want to delete this data....';
        else
            $scope.message_confirmation = 'Are you sure want to delete [ ' + data.BuyerActivityName + ' ]';
        angular.element(document.querySelector('#confirmActivityPopUp')).modal('show');
    };

    $scope.DeleteActivitySavedItem = function () {
        if (baseService.isUndefinedOrNull($scope.buyerMsterActivityId)) {
            $scope.SelectedActivityList.splice($scope.bActivityIndex, 1);
        }
        else {
            $http({
                method: 'POST',
                url: 'Costings/OrderCosting/DeleteCostingDetail?id=' + $scope.buyerMsterActivityId
            }).then(function successCallback(response) {

            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure', 'buyerActivityPopUp');
            }).finally(function () {
            });
        }
    };


    $scope.confirmDeleteCostingDetail = function (index, obj) {
        $scope.costingDetailId = obj.Id;
        $scope.index = index;
        $scope.message_confirmation = "Do your want delete permanently?";
        angular.element(document.querySelector("#confirmDeletePopUp")).modal("show");

    };




    $scope.confirmDetachPopUp = function (index, obj) {

        $scope.message_confirmation = "Do your want detach this file?";
        angular.element(document.querySelector("#confirfileDetachPopUp")).modal("show");

    };
    $scope.DetachFile = function () {

        $scope.ClearDocument();
        $scope.Save();

    }

    $scope.RemoveCostingDetail = function () {

        angular.element(document.querySelector("#confirmDeletePopUp")).modal("hide");

        if ($scope.OrderCostingDetailList.length > 0) {
            $scope.OrderCostingDetailList.splice($scope.index, 1);

        }
    }
    $scope.RemoveCostingDetailConfirmPopUp = function (index) {
        $scope.indexOfCostingDetail = index;

        $scope.message_confirmation = "Do your want delete?";
        angular.element(document.querySelector("#removeCostingDetailPopUp")).modal("show");

    };
    $scope.RemoveCostingDetailPermanently = function () {
        angular.element(document.querySelector("#removeCostingDetailPopUp")).modal("hide");
        if ($scope.OrderCostingDetailList.length > 0) {
            $scope.OrderCostingDetailList.splice($scope.indexOfCostingDetail, 1);

        }

    };

    $scope.ProductMasterDetail = function (ProductMasterId) {
        $http({
            method: 'GET',
            url: $scope.path + "ProductMasterDetail?ProductMasterId=" + ProductMasterId,
            dataType: 'JSON'
        }).then(function successCallback(response) {


            $scope.ModelNew.ProductSubCategory = response.data.Product[0].ProductSubCategory;
            $scope.ModelNew.CostingType = response.data.Product[0].CostingType;
            $scope.ModelNew.CostingTypeName = response.data.Product[0].CostingTypeName;
            $scope.ModelNew.ProductCategory = response.data.Product[0].ProductCategory;

            $scope.ModelNew.NoOfWorkstation = response.data.Product[0].NoOfWorkstation;
            $scope.ModelNew.EfficiencyPercentage = response.data.Product[0].EfficiencyPercentage;
            $scope.ModelNew.StandardWorkingHours = response.data.Product[0].StandardWorkingHours;
            $scope.ModelNew.StandardWorkingHoursForProduct = response.data.Product[0].StandardWorkingHours;

            $scope.ModelNew.SPT = response.data.Product[0].SPT;

            $scope.ModelNew.StandardWorkingHourCost = response.data.Product[0].StandardWorkingHourCost;
            $scope.ModelNew.AdditionalWorkingHourCostPerHour = response.data.Product[0].AdditionalWorkingHourCostPerHour;

            $scope.calculateSPT();
        });
    }
    $scope.GetCostingComponentByProductMasterId = function (ProductMasterId) {
        $http({
            method: 'GET',
            url: $scope.path + "GetCostingComponentByProductMasterId?ProductMasterId=" + ProductMasterId,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.OrderCostingDetailList = response.data;


        });
    }

    $scope.backColor = 'white';
    $scope.isRemoveFromCostingTypeComponent = false;

    $scope.OrderCostingDetailList = [];
    $scope.OrderCostingItemList = [];
    $scope.Status = 0;
    $scope.onChengeProductMaster = function () {

        $http({
            method: 'GET',
            url: $scope.path + "GetOrderCostingDetailByProductMaster?ProductMasterId=" + $scope.ModelNew.ProductMasterId + "&CostingVersionMasterTemplateId=" + $scope.ModelNew.Id,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.OrderCostingDetailList = response.data.ComponentList;
            $scope.OrderCostingItemList = response.data.ItemList;
            $scope.UpChargeMatrix = response.data.UpChargeMatrix;
            $scope.MakeSummaryBySegment();
        });
    }
    $scope.AfterAddRemoveCostingItem = function () {


        $http({
            method: 'GET',
            url: $scope.path + "GetOrderCostingDetailByProductMaster?ProductMasterId=" + $scope.ModelNew.ProductMasterId + "&CostingVersionMasterTemplateId=" + $scope.ModelNew.Id,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.OrderCostingDetailList = response.data.ComponentList;
            $scope.OrderCostingItemList = response.data.ItemList;
            $scope.MakeSummaryBySegment();

            $scope.NavigateToOrderCosting($scope.SelectedOrderCostingComponent);
        });

    }

    $scope.PiechartData = [];
    $scope.tabQ = 1;
    $scope.setTabQ = function (newTab) {
        $scope.tabQ = newTab;
        if (newTab == 3) {

            //for (var i = 0; i < $scope.OrderCostingDetailList.length; i++) {
            //    if ($scope.OrderCostingDetailList[i].BuyerTarget > 0 ? $scope.OrderCostingDetailList[i].BuyerTarge : $scope.OrderCostingDetailList[i].BuyerTarget = 0);
            //    if ($scope.OrderCostingDetailList[i].CostingValue > 0 ? $scope.OrderCostingDetailList[i].CostingValue : $scope.OrderCostingDetailList[i].CostingValue = 0);
            //    if ($scope.OrderCostingDetailList[i].TotalGrossAmount > 0 ? $scope.OrderCostingDetailList[i].TotalGrossAmount : $scope.OrderCostingDetailList[i].TotalGrossAmount = 0);
            //}


            var chartObj = $("#graphdivComparison").data("ejChart");
            chartObj.redraw();
            chartObj = $("#graphdivBuyerTarget").data("ejChart");
            chartObj.redraw();
            chartObj = $("#graphdivOrderCosting").data("ejChart");
            chartObj.redraw();
            chartObj = $("#graphdivOrderCosting").data("ejChart");
            chartObj.redraw();
        }
    };
    $scope.isSetQ = function (tabNum) {
        return $scope.tabQ === tabNum;

    };

    $scope.tabMain = 1;
    $scope.setTabMain = function (newTab) {
        $scope.tabMain = newTab;

    };
    $scope.isSetMain = function (tabNum) {
        return $scope.tabMain === tabNum;
    };

    $scope.tab1 = 1;
    $scope.setTab1 = function (newTab) {
        $scope.tab1 = newTab;

    };
    $scope.isSet1 = function (tabNum) {
        return $scope.tab1 === tabNum;
    };

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    }
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.tabCosting = 1;
    $scope.setTabCosting = function (newTab) {
        $scope.tabCosting = newTab;
    }
    $scope.isSetCosting = function (tabNum) {
        return $scope.tabCosting === tabNum;
    };


    $scope.CostingItem = {
        Sequence: 0,
        Code: null,
        ShortName: null,
        UserName: null,
        Description: null,
        Remarks: null,
        POIssueDeadLine: null,
        Active: true,
        CostingCategoryId: null,
        CostingComponentId: null,
        UnitOfMeasurementId: null,
        MinimumOfQuantity: 0,
        Wastage: null,
        BudgetMasterId: null,
        ProcessId: null,
        ActivityId: null,
        PurchaseGroupId: null,
        CostingGroupId: null,
        // CostingItemType: null,
    };
    $scope.OrderCostingDetail = {
        Id: null,
        Sequence: 0,
        CostingItemId: null,
        OrderCostingVersionMasterId: null,
        CostingValue: 0,
        BuyerTarget: 0
    };
    $scope.AddCostingItem = function () {

        $scope.CostingItemList.push($scope.CostingItem);
    };
    $scope.SaveCostingItemsIncludingComponent = function () {
        $http({
            method: 'POST',
            url: 'Costings/OrderCosting/SaveCostingItemsIncludingComponent',
            data: { costingItems: $scope.CostingItemList, OrderCostingDetail: $scope.OrderCostingDetail },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');

            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        }
    };



    // #region Costing Items PopUp
    $scope.costingItemParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'UserName',
        searchBy: 'UserName',
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.IssueGroups = [];
    $scope.GetIssueGroup = function (x) {
        $scope.issueGroupNew = x;
        $scope.issueTransactionNew.IssueGroupName = $scope.issueGroupNew.Name;
        $scope.issueTransactionNew.IssueGroupId = $scope.issueGroupNew.Id;
        $scope.hideCostingItemListPopUp();
    }


    // #endregion end Costing Items PopUp



    $scope.detailTemp = "#tabGridContents";


    $scope.getCostingItemByComponentId = function () {
        $scope.data = [];
        $http({
            method: 'GET',
            url: 'Costings/OrderCosting/GetCostingItemByComponentId?costingComponentId=' + $scope.CostingComponentId,

            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.CostingItemList = response.data;


        });
    };

    $scope.showCostingItemListPopUp = function () {
        $scope.getCostingItemByComponentId();
        angular.element(document.querySelector("#costingItemListPopUp")).modal("show");
    };


    $scope.GetDirectCostingMaterialWithItemByComponentId = function () {
        $http({
            method: 'GET',
            url: 'Costings/OrderCosting/GetDirectCostingMaterialWithItemByComponentId?costingComponentId=' + $scope.CostingComponentId + '&OrderCostingMasterTemplateId=' + $scope.ModelNew.Id,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.DirectMaterialList = response.data.Pre;
            $scope.DirectProcurementCostingMaterialList = response.data.Procurement;

            var elmnt = document.getElementById("CostingItemsEntry");
            elmnt.scrollIntoView(false, { behavior: "smooth", block: "end", inline: "nearest" });
        });
    }

    $scope.showPopUp = function (args) {
        $scope.CostingComponentId = args.data.CostingComponentId;
        $scope.TotalSegmentedCostValue = args.data.CostingValue;
        if ($scope.Segment == 'DirectMaterial') {

            $scope.showDirectMaterialWithItemPopUp();
        }
        else if ($scope.Segment == 'Operation') {


            $scope.showOperationPopUp();
        }
        else if ($scope.Segment == 'DirectProcess') {

            $scope.showDirectProcessPopUp();
        }
        else if ($scope.Segment == 'SalesExpense') {

            $scope.showSalesExpensePopUp();
        }
        else if ($scope.Segment == 'ValueLoss') {

            $scope.showValueLossPopUp();
        }
    };
    $scope.showDirectMaterialWithItemPopUp = function () {
        $scope.GetDirectCostingMaterialWithItemByComponentId();
        angular.element(document.querySelector("#DirectMaterialWithItemPopUp")).modal("show");
    }
    $scope.hideDirectMaterialWithItemPopUp = function () {
        angular.element(document.querySelector("#DirectMaterialWithItemPopUp")).modal("hide");
    };

    $scope.totalItemGrossAmount = 0;
    $scope.toatalItemGrossConsumption = 0;
    $scope.CalculateItemValueByPerComponent = function () {


        $scope.totalItemGrossAmount = 0;
        $scope.toatalItemGrossConsumption = 0;
        //if ($scope.DirectMaterialList.length > 0) {
        for (var i = 0; i < $scope.DirectMaterialList.length; i++) {
            $scope.totalItemGrossAmount += $scope.DirectMaterialList[i].GrossAmount;
            $scope.toatalItemGrossConsumption += $scope.DirectMaterialList[i].GrossConsumption;

        }
        //}

        for (var i = 0; i < $scope.OrderCostingDetailList.length; i++) {
            if ($scope.OrderCostingDetailList[i].CostingComponentId == $scope.CostingComponentId) {
                $scope.OrderCostingDetailList[i].TotalGrossAmount = $scope.totalItemGrossAmount;

            }
        }
    };
    $scope.SummaryBySegmentList = [];
    $scope.MakeSummaryBySegment = function () {
        $scope.SummaryBySegmentList = [];
        var DistinctSegments = ej.DataManager($scope.OrderCostingDetailList).executeLocal(ej.Query().group("CostingSegment"));
        for (var s = 0; s < DistinctSegments.length; s++) {
            var ItemsBySegments = DistinctSegments[s].items; //ej.DataManager($scope.OrderCostingDetailList).executeLocal(ej.Query().where("CostingSegment", "equal", DistinctSegments[0].items[s]));
            var BuyerTarget = 0, CostingValue = 0, TotalGrossAmount = 0, TotalProcurementGrossAmount = 0;
            for (var i = 0; i < ItemsBySegments.length; i++) {
                BuyerTarget += ItemsBySegments[i].BuyerTarget;
                CostingValue += ItemsBySegments[i].CostingValue;
                TotalGrossAmount += ItemsBySegments[i].TotalGrossAmount;
                TotalProcurementGrossAmount += ItemsBySegments[i].TotalProcurementGrossAmount;
            }

            var tempData = { Segment: DistinctSegments[s].key, BuyerTarget: BuyerTarget, CostingValue: CostingValue, TotalGrossAmount: TotalGrossAmount, TotalProcurementGrossAmount: TotalProcurementGrossAmount };
            $scope.SummaryBySegmentList.push(tempData);
        }

        $scope.SumCostingValue();
    }
    $scope.displayTextRendering = function (args) {
        var Total = 0;
        for (var i = 0; i < $scope.OrderCostingDetailList.length; i++) {
            Total += $scope.OrderCostingDetailList[i][args.data.series.yName];

        }

        try {
            if (Total > 0)
                args.data.text = (parseFloat(args.data.text) / Total * 100).toFixed(0) + '%';
            else
                args.data.text = "0%";

        } catch (e) {

        }


    }
    $scope.CalculateFinalCosting = function (data) {

        //first try to push the data into main list
        try {
            for (var i = 0; i < $scope.OrderCostingItemList.length; i++) {
                if ($scope.OrderCostingItemList[i].Id == data.CostingItemId) {
                    if ($scope.Segment == "SalesExpense" && data.CostingComponentId == $scope.CostingComponentId) {
                        $scope.OrderCostingItemList[i].ValueType = data.Type;
                        $scope.OrderCostingItemList[i].Value = data.Value;
                    }
                    if ($scope.Segment == "ValueLoss" && data.CostingComponentId == $scope.CostingComponentId) {
                        $scope.OrderCostingItemList[i].ValueType = data.Type;
                        $scope.OrderCostingItemList[i].Value = data.Value;
                    }
                    if ($scope.Segment == "Profit" && data.CostingComponentId == $scope.CostingComponentId) {
                        $scope.OrderCostingItemList[i].ValueType = data.Type;
                        $scope.OrderCostingItemList[i].Value = data.Value;
                    }
                    if ($scope.OrderCostingItemList[i].CostingSegment == 'DirectMaterial') {
                        data.GrossConsumption = parseFloat(data.Consumption / ((100 - data.ValueLoss) / 100)).toFixed(4); // (data.Consumption * data.ValueLoss / 100) + data.Consumption;
                        data.GrossAmount = parseFloat(data.GrossConsumption * data.Rate).toFixed(4);
                        $scope.OrderCostingItemList[i].TotalGrossAmount = parseFloat(data.GrossConsumption * data.Rate).toFixed(4);


                    }
                    else if ($scope.OrderCostingItemList[i].CostingSegment == 'Operation') {
                        $scope.OrderCostingItemList[i].TotalGrossAmount = data.Value;

                    }
                    else if ($scope.OrderCostingItemList[i].CostingSegment == 'DirectProcess') {
                        //first push the 
                        $scope.OrderCostingItemList[i].Rate = data.Rate;
                        $scope.OrderCostingItemList[i].Value = data.Value;


                        var totalPre = getFixedAmountDirectMaterial();

                        $scope.OrderCostingItemList[i].TotalGrossAmount = (totalPre / ((100 - data.Value) / 100)) - totalPre;// totalPre * (data.Value / 100)
                        $scope.OrderCostingItemList[i].TotalGrossAmount += data.Rate;

                        $scope.OrderCostingItemList[i].Rate = data.Rate;
                        $scope.OrderCostingItemList[i].Value = data.Value;

                        data.Amount = $scope.OrderCostingItemList[i].TotalGrossAmount;
                    }
                    else if ($scope.OrderCostingItemList[i].CostingSegment == 'SalesExpense') {


                        if ($scope.OrderCostingItemList[i].ValueType == 'FIXED' || $scope.OrderCostingItemList[i].ValueType == 'Fixed') {
                            $scope.OrderCostingItemList[i].TotalGrossAmount = data.Value;
                        }
                        else {
                            var totalPre = getFixedAmount($scope.OrderCostingItemList[i].ComponentSequence);
                            var totalCurr = getCurrentFixedAmount($scope.OrderCostingItemList[i].ComponentSequence);
                            var totalPercent = getCurrentPercent($scope.OrderCostingItemList[i].ComponentSequence);

                            if (totalPercent >= 100) {
                                data.Value = 0;
                            }
                            if ($scope.OrderCostingItemList[i].CalculationMethod.toUpperCase() == "CUMULATIVE")
                                $scope.OrderCostingItemList[i].TotalGrossAmount = parseFloat(totalPre * (data.Value / 100)).toFixed(4);
                            else
                                $scope.OrderCostingItemList[i].TotalGrossAmount = parseFloat(((totalPre + totalCurr) / ((100 - totalPercent) / 100)) * (data.Value / 100)).toFixed(4);

                        }
                        data.Amount = $scope.OrderCostingItemList[i].TotalGrossAmount;
                    }
                    else if ($scope.OrderCostingItemList[i].CostingSegment == 'ValueLoss') {

                        if ($scope.OrderCostingItemList[i].ValueType == 'FIXED' || $scope.OrderCostingItemList[i].ValueType == 'Fixed') {
                            $scope.OrderCostingItemList[i].TotalGrossAmount = data.Value;
                        }
                        else {

                            var totalPre = getFixedAmount($scope.OrderCostingItemList[i].ComponentSequence);
                            var totalCurr = getCurrentFixedAmount($scope.OrderCostingItemList[i].ComponentSequence);
                            var totalPercent = getCurrentPercent($scope.OrderCostingItemList[i].ComponentSequence);
                            if (totalPercent >= 100) {
                                data.Value = 0;
                            }
                            if ($scope.OrderCostingItemList[i].CalculationMethod.toUpperCase() == "CUMULATIVE")
                                $scope.OrderCostingItemList[i].TotalGrossAmount = parseFloat(totalPre * (data.Value / 100)).toFixed(4);
                            else
                                $scope.OrderCostingItemList[i].TotalGrossAmount = parseFloat(((totalPre + totalCurr) / ((100 - totalPercent) / 100)) * (data.Value / 100)).toFixed(4);


                            //var totalPre = getFixedAmount($scope.OrderCostingItemList[i].ComponentSequence);

                            //$scope.OrderCostingItemList[i].TotalGrossAmount = totalPre * (data.Value / 100);

                        }
                        data.Amount = $scope.OrderCostingItemList[i].TotalGrossAmount;
                    }
                    else if ($scope.OrderCostingItemList[i].CostingSegment == 'Profit') {

                        if ($scope.OrderCostingItemList[i].ValueType == 'FIXED' || $scope.OrderCostingItemList[i].ValueType == 'Fixed') {
                            $scope.OrderCostingItemList[i].TotalGrossAmount = data.Value;
                        }
                        else {

                            var totalPre = getFixedAmount($scope.OrderCostingItemList[i].ComponentSequence);
                            var totalCurr = getCurrentFixedAmount($scope.OrderCostingItemList[i].ComponentSequence);
                            var totalPercent = getCurrentPercent($scope.OrderCostingItemList[i].ComponentSequence);
                            if (totalPercent >= 100) {
                                data.Value = 0;
                            }
                            if ($scope.OrderCostingItemList[i].CalculationMethod.toUpperCase() == "CUMULATIVE")
                                $scope.OrderCostingItemList[i].TotalGrossAmount = parseFloat(totalPre * (data.Value / 100)).toFixed(4);
                            else
                                $scope.OrderCostingItemList[i].TotalGrossAmount = parseFloat(((totalPre + totalCurr) / ((100 - totalPercent) / 100)) * (data.Value / 100)).toFixed(4);

                            //var totalPre = getFixedAmount($scope.OrderCostingItemList[i].ComponentSequence);

                            //$scope.OrderCostingItemList[i].TotalGrossAmount = totalPre * (data.Value / 100);

                        }
                        data.Amount = $scope.OrderCostingItemList[i].TotalGrossAmount;
                    }

                }
            }
        } catch (e) {

        }

        try {
            for (var i = 0; i < $scope.OrderCostingItemList.length; i++) {

                if ($scope.OrderCostingItemList[i].CostingSegment == 'DirectProcess') {

                    var totalPre = getFixedAmountDirectMaterial();

                    $scope.OrderCostingItemList[i].TotalGrossAmount = (totalPre / ((100 - $scope.OrderCostingItemList[i].Value) / 100)) - totalPre;//totalPre * ($scope.OrderCostingItemList[i].Value / 100);
                    $scope.OrderCostingItemList[i].TotalGrossAmount += $scope.OrderCostingItemList[i].Rate;

                }
                else if ($scope.OrderCostingItemList[i].CostingSegment == 'SalesExpense') {

                    if ($scope.OrderCostingItemList[i].ValueType == 'FIXED' || $scope.OrderCostingItemList[i].ValueType == 'Fixed') {
                        $scope.OrderCostingItemList[i].TotalGrossAmount = $scope.OrderCostingItemList[i].Value;
                    }
                    else {

                        var totalPre = getFixedAmount($scope.OrderCostingItemList[i].ComponentSequence);
                        var totalCurr = getCurrentFixedAmount($scope.OrderCostingItemList[i].ComponentSequence);
                        var totalPercent = getCurrentPercent($scope.OrderCostingItemList[i].ComponentSequence);


                        if ($scope.OrderCostingItemList[i].CalculationMethod.toUpperCase() == "CUMULATIVE")
                            $scope.OrderCostingItemList[i].TotalGrossAmount = totalPre * ($scope.OrderCostingItemList[i].Value / 100);
                        else
                            $scope.OrderCostingItemList[i].TotalGrossAmount = parseFloat(((totalPre + totalCurr) / ((100 - totalPercent) / 100)) * ($scope.OrderCostingItemList[i].Value / 100)).toFixed(4);

                        //var totalPre = getFixedAmount($scope.OrderCostingItemList[i].ComponentSequence);

                        //$scope.OrderCostingItemList[i].TotalGrossAmount = totalPre * ($scope.OrderCostingItemList[i].Value / 100);

                    }
                }
                else if ($scope.OrderCostingItemList[i].CostingSegment == 'ValueLoss') {

                    if ($scope.OrderCostingItemList[i].ValueType == 'FIXED' || $scope.OrderCostingItemList[i].ValueType == 'Fixed') {
                        $scope.OrderCostingItemList[i].TotalGrossAmount = $scope.OrderCostingItemList[i].Value;
                    }
                    else {
                        var totalPre = getFixedAmount($scope.OrderCostingItemList[i].ComponentSequence);
                        var totalCurr = getCurrentFixedAmount($scope.OrderCostingItemList[i].ComponentSequence);
                        var totalPercent = getCurrentPercent($scope.OrderCostingItemList[i].ComponentSequence);

                        if ($scope.OrderCostingItemList[i].CalculationMethod.toUpperCase() == "CUMULATIVE")
                            $scope.OrderCostingItemList[i].TotalGrossAmount = totalPre * ($scope.OrderCostingItemList[i].Value / 100);
                        else
                            $scope.OrderCostingItemList[i].TotalGrossAmount = parseFloat(((totalPre + totalCurr) / ((100 - totalPercent) / 100)) * ($scope.OrderCostingItemList[i].Value / 100)).toFixed(4);

                        //var totalPre = getFixedAmount($scope.OrderCostingItemList[i].ComponentSequence);

                        //$scope.OrderCostingItemList[i].TotalGrossAmount = totalPre * ($scope.OrderCostingItemList[i].Value / 100);

                    }
                }
                else if ($scope.OrderCostingItemList[i].CostingSegment == 'Profit') {


                    if ($scope.OrderCostingItemList[i].ValueType == 'FIXED' || $scope.OrderCostingItemList[i].ValueType == 'Fixed') {
                        $scope.OrderCostingItemList[i].TotalGrossAmount = $scope.OrderCostingItemList[i].Value;
                    }
                    else {
                        //var totalPre = getFixedAmount($scope.OrderCostingItemList[i].ComponentSequence);

                        //$scope.OrderCostingItemList[i].TotalGrossAmount = totalPre * ($scope.OrderCostingItemList[i].Value / 100);
                        var totalPre = getFixedAmount($scope.OrderCostingItemList[i].ComponentSequence);
                        var totalCurr = getCurrentFixedAmount($scope.OrderCostingItemList[i].ComponentSequence);
                        var totalPercent = getCurrentPercent($scope.OrderCostingItemList[i].ComponentSequence);

                        if ($scope.OrderCostingItemList[i].CalculationMethod.toUpperCase() == "CUMULATIVE")
                            $scope.OrderCostingItemList[i].TotalGrossAmount = totalPre * ($scope.OrderCostingItemList[i].Value / 100);
                        else
                            $scope.OrderCostingItemList[i].TotalGrossAmount = parseFloat(((totalPre + totalCurr) / ((100 - totalPercent) / 100)) * ($scope.OrderCostingItemList[i].Value / 100)).toFixed(4);

                    }
                }

                //}
            }
        } catch (e) {

        }



        try {
            $scope.totalItemGrossAmount = 0;
            $scope.totalOperationValue = 0;
            $scope.totalDirectProcessAmount = 0;
            $scope.totalSalesExpenseAmount = 0;
            $scope.totalValueLossAmount = 0;
            $scope.TotalSegmentedValueByComponent = 0;


            $scope.CostingSummaryDataNew = Object.assign({}, $scope.CostingSummaryDataMain);

            for (var i = 0; i < $scope.OrderCostingDetailList.length; i++) {


                var TotalValue = 0;
                for (var k = 0; k < $scope.OrderCostingItemList.length; k++) {
                    if ($scope.OrderCostingDetailList[i].CostingComponentId == $scope.OrderCostingItemList[k].CostingComponentId) {
                        TotalValue += $scope.OrderCostingItemList[k].TotalGrossAmount;
                    }
                }
                $scope.OrderCostingDetailList[i].TotalGrossAmount = TotalValue;

                if ($scope.OrderCostingDetailList[i].CostingComponentId == $scope.CostingComponentId) {
                    $scope.TotalSegmentedValueByComponent = TotalValue;
                }

                //$scope.CostingSummaryDataMain = { BuyerTotal: 0, OrderCostingValue: 0, OrderCostingValue, ProfitBuyerCosting: 0, ProfitOrderCosting: 0, ProfitOrderCosting: 0 };

                //calculation
                if ($scope.OrderCostingDetailList[i].CostingSegment == 'Profit') {
                    $scope.CostingSummaryDataNew.ProfitBuyerCosting += $scope.OrderCostingDetailList[i].BuyerTarget;
                    $scope.CostingSummaryDataNew.ProfitQuickCosting += $scope.OrderCostingDetailList[i].CostingValue;
                    $scope.CostingSummaryDataNew.ProfitOrderCosting += $scope.OrderCostingDetailList[i].TotalGrossAmount;
                    $scope.CostingSummaryDataNew.ProfitProcurementCosting += $scope.OrderCostingDetailList[i].TotalProcurementGrossAmount;
                }
                else {
                    $scope.CostingSummaryDataNew.BuyerTotal += $scope.OrderCostingDetailList[i].BuyerTarget;
                    $scope.CostingSummaryDataNew.QuickCostingValue += $scope.OrderCostingDetailList[i].CostingValue;
                    $scope.CostingSummaryDataNew.OrderCostingValue += $scope.OrderCostingDetailList[i].TotalGrossAmount;
                    $scope.CostingSummaryDataNew.ProcurementCostingValue += $scope.OrderCostingDetailList[i].TotalProcurementGrossAmount;

                }


            }

            liveUpdateCostingComponent();

        } catch (e) {

        }



    }

    //$scope.totalItemGrossAmount = 0;
    //$scope.totalOperationValue = 0;
    //$scope.totalDirectProcessAmount = 0;
    //$scope.totalSalesExpenseAmount = 0;
    //$scope.totalValueLossAmount = 0;
    //$scope.TotalSegmentedValueByComponent = 0;

    $scope.totalProcurementItemGrossAmount = 0;
    $scope.totalProcurementOperationValue = 0;
    $scope.totalProcurementDirectProcessAmount = 0;
    $scope.totalProcurementSalesExpenseAmount = 0;
    $scope.totalProcurementValueLossAmount = 0;
    $scope.TotalProcurementSegmentedValueByComponent = 0;
    $scope.CalculateFinalCostingProcurement = function (data) {

        //first try to push the data into main list
        try {
            for (var i = 0; i < $scope.OrderCostingItemList.length; i++) {
                if ($scope.OrderCostingItemList[i].Id == data.CostingItemId) {
                    if ($scope.Segment == "SalesExpense" && data.CostingComponentId == $scope.CostingComponentId) {
                        $scope.OrderCostingItemList[i].ProcurementProcurementValueType = data.Type;
                        $scope.OrderCostingItemList[i].ProcurementValue = data.Value;
                    }
                    if ($scope.Segment == "ValueLoss" && data.CostingComponentId == $scope.CostingComponentId) {
                        $scope.OrderCostingItemList[i].ProcurementProcurementValueType = data.Type;
                        $scope.OrderCostingItemList[i].ProcurementValue = data.Value;
                    }
                    if ($scope.Segment == "Profit" && data.CostingComponentId == $scope.CostingComponentId) {
                        $scope.OrderCostingItemList[i].ProcurementProcurementValueType = data.Type;
                        $scope.OrderCostingItemList[i].ProcurementValue = data.Value;
                    }
                    if ($scope.OrderCostingItemList[i].CostingSegment == 'DirectMaterial') {
                        data.GrossConsumption = parseFloat(data.Consumption / ((100 - data.ValueLoss) / 100)).toFixed(4);//(data.Consumption * data.ValueLoss / 100) + data.Consumption;
                        data.GrossAmount = parseFloat(data.GrossConsumption * data.Rate).toFixed(4);
                        $scope.OrderCostingItemList[i].TotalProcurementGrossAmount = parseFloat(data.GrossConsumption * data.Rate).toFixed(4);


                    }
                    else if ($scope.OrderCostingItemList[i].CostingSegment == 'Operation') {
                        $scope.OrderCostingItemList[i].TotalProcurementGrossAmount = data.Value;

                    }
                    else if ($scope.OrderCostingItemList[i].CostingSegment == 'DirectProcess') {
                        //first push the 
                        $scope.OrderCostingItemList[i].ProcurementRate = data.Rate;
                        $scope.OrderCostingItemList[i].ProcurementValue = data.Value;


                        var totalPre = getProcurementFixedAmountDirectMaterial();

                        $scope.OrderCostingItemList[i].TotalProcurementGrossAmount = parseFloat((totalPre / ((100 - data.Value) / 100)) - totalPre).toFixed(4);// totalPre * (data.Value / 100)
                        $scope.OrderCostingItemList[i].TotalProcurementGrossAmount += data.Rate;

                        $scope.OrderCostingItemList[i].ProcurementRate = data.Rate;
                        $scope.OrderCostingItemList[i].ProcurementValue = data.Value;

                        data.Amount = $scope.OrderCostingItemList[i].TotalProcurementGrossAmount;
                    }
                    else if ($scope.OrderCostingItemList[i].CostingSegment == 'SalesExpense') {


                        if ($scope.OrderCostingItemList[i].ProcurementProcurementValueType == 'FIXED' || $scope.OrderCostingItemList[i].ProcurementProcurementValueType == 'Fixed') {
                            $scope.OrderCostingItemList[i].TotalProcurementGrossAmount = data.Value;
                        }
                        else {
                            var totalPre = getProcurementFixedAmount($scope.OrderCostingItemList[i].ComponentSequence);
                            var totalCurr = getProcurementCurrentFixedAmount($scope.OrderCostingItemList[i].ComponentSequence);
                            var totalPercent = getProcurementCurrentPercent($scope.OrderCostingItemList[i].ComponentSequence);

                            if (totalPercent >= 100) {
                                data.Value = 0;
                            }
                            if ($scope.OrderCostingItemList[i].CalculationMethod.toUpperCase() == "CUMULATIVE")
                                $scope.OrderCostingItemList[i].TotalProcurementGrossAmount = totalPre * (data.Value / 100);
                            else
                                $scope.OrderCostingItemList[i].TotalProcurementGrossAmount = parseFloat(((totalPre + totalCurr) / ((100 - totalPercent) / 100)) * (data.Value / 100)).toFixed(4);

                        }
                        data.Amount = $scope.OrderCostingItemList[i].TotalProcurementGrossAmount;
                    }
                    else if ($scope.OrderCostingItemList[i].CostingSegment == 'ValueLoss') {

                        if ($scope.OrderCostingItemList[i].ProcurementProcurementValueType == 'FIXED' || $scope.OrderCostingItemList[i].ProcurementProcurementValueType == 'Fixed') {
                            $scope.OrderCostingItemList[i].TotalProcurementGrossAmount = data.Value;
                        }
                        else {

                            var totalPre = getProcurementFixedAmount($scope.OrderCostingItemList[i].ComponentSequence);
                            var totalCurr = getProcurementCurrentFixedAmount($scope.OrderCostingItemList[i].ComponentSequence);
                            var totalPercent = getProcurementCurrentPercent($scope.OrderCostingItemList[i].ComponentSequence);
                            if (totalPercent >= 100) {
                                data.Value = 0;
                            }

                            if ($scope.OrderCostingItemList[i].CalculationMethod.toUpperCase() == "CUMULATIVE")
                                $scope.OrderCostingItemList[i].TotalProcurementGrossAmount = totalPre * (data.Value / 100);
                            else
                                $scope.OrderCostingItemList[i].TotalProcurementGrossAmount = parseFloat(((totalPre + totalCurr) / ((100 - totalPercent) / 100)) * (data.Value / 100)).toFixed(4);


                            //var totalPre = getProcurementFixedAmount($scope.OrderCostingItemList[i].ComponentSequence);

                            //$scope.OrderCostingItemList[i].TotalProcurementGrossAmount = totalPre * (data.Value / 100);

                        }
                        data.Amount = $scope.OrderCostingItemList[i].TotalProcurementGrossAmount;
                    }
                    else if ($scope.OrderCostingItemList[i].CostingSegment == 'Profit') {

                        if ($scope.OrderCostingItemList[i].ProcurementProcurementValueType == 'FIXED' || $scope.OrderCostingItemList[i].ProcurementProcurementValueType == 'Fixed') {
                            $scope.OrderCostingItemList[i].TotalProcurementGrossAmount = data.Value;
                        }
                        else {

                            var totalPre = getProcurementFixedAmount($scope.OrderCostingItemList[i].ComponentSequence);
                            var totalCurr = getProcurementCurrentFixedAmount($scope.OrderCostingItemList[i].ComponentSequence);
                            var totalPercent = getProcurementCurrentPercent($scope.OrderCostingItemList[i].ComponentSequence);
                            if (totalPercent >= 100) {
                                data.Value = 0;
                            }

                            if ($scope.OrderCostingItemList[i].CalculationMethod.toUpperCase() == "CUMULATIVE")
                                $scope.OrderCostingItemList[i].TotalProcurementGrossAmount = totalPre * (data.Value / 100);
                            else
                                $scope.OrderCostingItemList[i].TotalProcurementGrossAmount = parseFloat(((totalPre + totalCurr) / ((100 - totalPercent) / 100)) * (data.Value / 100)).toFixed(4);

                            //var totalPre = getProcurementFixedAmount($scope.OrderCostingItemList[i].ComponentSequence);

                            //$scope.OrderCostingItemList[i].TotalProcurementGrossAmount = totalPre * (data.Value / 100);

                        }
                        data.Amount = $scope.OrderCostingItemList[i].TotalProcurementGrossAmount;
                    }

                }
            }
        } catch (e) {

        }

        try {
            for (var i = 0; i < $scope.OrderCostingItemList.length; i++) {

                if ($scope.OrderCostingItemList[i].CostingSegment == 'DirectProcess') {

                    var totalPre = getProcurementFixedAmountDirectMaterial();

                    $scope.OrderCostingItemList[i].TotalProcurementGrossAmount = parseFloat((totalPre / ((100 - $scope.OrderCostingItemList[i].ProcurementValue) / 100)) - totalPre).toFixed(4);//totalPre * ($scope.OrderCostingItemList[i].ProcurementValue / 100);
                    $scope.OrderCostingItemList[i].TotalProcurementGrossAmount += $scope.OrderCostingItemList[i].ProcurementRate;

                }
                else if ($scope.OrderCostingItemList[i].CostingSegment == 'SalesExpense') {

                    if ($scope.OrderCostingItemList[i].ProcurementProcurementValueType == 'FIXED' || $scope.OrderCostingItemList[i].ProcurementProcurementValueType == 'Fixed') {
                        $scope.OrderCostingItemList[i].TotalProcurementGrossAmount = $scope.OrderCostingItemList[i].ProcurementValue;
                    }
                    else {

                        var totalPre = getProcurementFixedAmount($scope.OrderCostingItemList[i].ComponentSequence);
                        var totalCurr = getProcurementCurrentFixedAmount($scope.OrderCostingItemList[i].ComponentSequence);
                        var totalPercent = getProcurementCurrentPercent($scope.OrderCostingItemList[i].ComponentSequence);

                        if ($scope.OrderCostingItemList[i].CalculationMethod.toUpperCase() == "CUMULATIVE")
                            $scope.OrderCostingItemList[i].TotalProcurementGrossAmount = parseFloat(totalPre * ($scope.OrderCostingItemList[i].ProcurementValue / 100)).toFixed(4);
                        else
                            $scope.OrderCostingItemList[i].TotalProcurementGrossAmount = parseFloat(((totalPre + totalCurr) / ((100 - totalPercent) / 100)) * ($scope.OrderCostingItemList[i].ProcurementValue / 100)).toFixed(4);

                        //var totalPre = getProcurementFixedAmount($scope.OrderCostingItemList[i].ComponentSequence);

                        //$scope.OrderCostingItemList[i].TotalProcurementGrossAmount = totalPre * ($scope.OrderCostingItemList[i].ProcurementValue / 100);

                    }
                }
                else if ($scope.OrderCostingItemList[i].CostingSegment == 'ValueLoss') {

                    if ($scope.OrderCostingItemList[i].ProcurementProcurementValueType == 'FIXED' || $scope.OrderCostingItemList[i].ProcurementProcurementValueType == 'Fixed') {
                        $scope.OrderCostingItemList[i].TotalProcurementGrossAmount = $scope.OrderCostingItemList[i].ProcurementValue;
                    }
                    else {
                        var totalPre = getProcurementFixedAmount($scope.OrderCostingItemList[i].ComponentSequence);
                        var totalCurr = getProcurementCurrentFixedAmount($scope.OrderCostingItemList[i].ComponentSequence);
                        var totalPercent = getProcurementCurrentPercent($scope.OrderCostingItemList[i].ComponentSequence);

                        if ($scope.OrderCostingItemList[i].CalculationMethod.toUpperCase() == "CUMULATIVE")
                            $scope.OrderCostingItemList[i].TotalProcurementGrossAmount = totalPre * ($scope.OrderCostingItemList[i].ProcurementValue / 100);
                        else
                            $scope.OrderCostingItemList[i].TotalProcurementGrossAmount = parseFloat(((totalPre + totalCurr) / ((100 - totalPercent) / 100)) * ($scope.OrderCostingItemList[i].ProcurementValue / 100)).toFixed(4);

                        //var totalPre = getProcurementFixedAmount($scope.OrderCostingItemList[i].ComponentSequence);

                        //$scope.OrderCostingItemList[i].TotalProcurementGrossAmount = totalPre * ($scope.OrderCostingItemList[i].ProcurementValue / 100);

                    }
                }
                else if ($scope.OrderCostingItemList[i].CostingSegment == 'Profit') {


                    if ($scope.OrderCostingItemList[i].ProcurementProcurementValueType == 'FIXED' || $scope.OrderCostingItemList[i].ProcurementProcurementValueType == 'Fixed') {
                        $scope.OrderCostingItemList[i].TotalProcurementGrossAmount = $scope.OrderCostingItemList[i].ProcurementValue;
                    }
                    else {
                        //var totalPre = getProcurementFixedAmount($scope.OrderCostingItemList[i].ComponentSequence);

                        //$scope.OrderCostingItemList[i].TotalProcurementGrossAmount = totalPre * ($scope.OrderCostingItemList[i].ProcurementValue / 100);
                        var totalPre = getProcurementFixedAmount($scope.OrderCostingItemList[i].ComponentSequence);
                        var totalCurr = getProcurementCurrentFixedAmount($scope.OrderCostingItemList[i].ComponentSequence);
                        var totalPercent = getProcurementCurrentPercent($scope.OrderCostingItemList[i].ComponentSequence);

                        if ($scope.OrderCostingItemList[i].CalculationMethod.toUpperCase() == "CUMULATIVE")
                            $scope.OrderCostingItemList[i].TotalProcurementGrossAmount = totalPre * ($scope.OrderCostingItemList[i].ProcurementValue / 100);
                        else
                            $scope.OrderCostingItemList[i].TotalProcurementGrossAmount = parseFloat(((totalPre + totalCurr) / ((100 - totalPercent) / 100)) * ($scope.OrderCostingItemList[i].ProcurementValue / 100)).toFixed(4);

                    }
                }

                //}
            }
        } catch (e) {

        }



        try {
            $scope.totalProcurementItemGrossAmount = 0;
            $scope.totalProcurementOperationValue = 0;
            $scope.totalProcurementDirectProcessAmount = 0;
            $scope.totalProcurementSalesExpenseAmount = 0;
            $scope.totalProcurementValueLossAmount = 0;
            $scope.TotalProcurementSegmentedValueByComponent = 0;


            $scope.CostingSummaryDataNew = Object.assign({}, $scope.CostingSummaryDataMain);

            for (var i = 0; i < $scope.OrderCostingDetailList.length; i++) {


                var TotalValue = 0;
                for (var k = 0; k < $scope.OrderCostingItemList.length; k++) {
                    if ($scope.OrderCostingDetailList[i].CostingComponentId == $scope.OrderCostingItemList[k].CostingComponentId) {
                        TotalValue += $scope.OrderCostingItemList[k].TotalProcurementGrossAmount;
                    }
                }
                $scope.OrderCostingDetailList[i].TotalProcurementGrossAmount = TotalValue;

                if ($scope.OrderCostingDetailList[i].CostingComponentId == $scope.CostingComponentId) {
                    $scope.TotalProcurementSegmentedValueByComponent = TotalValue;
                }

                //$scope.CostingSummaryDataMain = { BuyerTotal: 0, OrderCostingValue: 0, OrderCostingValue, ProfitBuyerCosting: 0, ProfitOrderCosting: 0, ProfitOrderCosting: 0 };

                //calculation
                if ($scope.OrderCostingDetailList[i].CostingSegment == 'Profit') {
                    $scope.CostingSummaryDataNew.ProfitBuyerCosting += $scope.OrderCostingDetailList[i].BuyerTarget;
                    $scope.CostingSummaryDataNew.ProfitQuickCosting += $scope.OrderCostingDetailList[i].CostingValue;
                    $scope.CostingSummaryDataNew.ProfitOrderCosting += $scope.OrderCostingDetailList[i].TotalGrossAmount;
                    $scope.CostingSummaryDataNew.ProfitProcurementCosting += $scope.OrderCostingDetailList[i].TotalProcurementGrossAmount;
                }
                else {
                    $scope.CostingSummaryDataNew.BuyerTotal += $scope.OrderCostingDetailList[i].BuyerTarget;
                    $scope.CostingSummaryDataNew.QuickCostingValue += $scope.OrderCostingDetailList[i].CostingValue;
                    $scope.CostingSummaryDataNew.OrderCostingValue += $scope.OrderCostingDetailList[i].TotalGrossAmount;
                    $scope.CostingSummaryDataNew.ProcurementCostingValue += $scope.OrderCostingDetailList[i].TotalProcurementGrossAmount;

                }


            }

            liveUpdateProcurementCostingComponent();

        } catch (e) {

        }



    }

    function liveUpdateCostingComponent() {

        var currentData = [];
        if ($scope.Segment == 'DirectMaterial') {

            return;
        }
        else if ($scope.Segment == 'Operation') {
            //currentData = $scope.DirectMaterialList;
            return;
        }
        else if ($scope.Segment == 'DirectProcess') {
            currentData = $scope.DirectProcessList;

        }
        else if ($scope.Segment == 'SalesExpense') {
            currentData = $scope.SalesExpenseList;

        }
        else if ($scope.Segment == 'ValueLoss') {
            currentData = $scope.ValueLossList;

        }
        else if ($scope.Segment == 'Profit') {
            currentData = $scope.ProfitList;

        }

        var data = ej.DataManager($scope.OrderCostingItemList).executeLocal(ej.Query().where("CostingComponentId", "equal", parseInt($scope.CostingComponentId), true));
        for (var i = 0; i < data.length; i++) {
            var single = ej.DataManager(currentData).executeLocal(ej.Query().where("CostingItemId", "equal", data[i].Id, true));
            if (single != null) {
                if (single.length > 0) {
                    try {
                        single[0].Amount = data[i].TotalGrossAmount;
                    } catch (e) {

                    }
                }

            }
        }
    }
    function liveUpdateProcurementCostingComponent() {

        var currentData = [];
        if ($scope.Segment == 'DirectMaterial') {

            return;
        }
        else if ($scope.Segment == 'Operation') {

            return;
        }
        else if ($scope.Segment == 'DirectProcess') {
            currentData = $scope.DirectProcessProcurementCostingList;

        }
        else if ($scope.Segment == 'SalesExpense') {
            currentData = $scope.SalesExpenseProcurementCostingList;

        }
        else if ($scope.Segment == 'ValueLoss') {
            currentData = $scope.ValueLossProcurementCostingList;

        }
        else if ($scope.Segment == 'Profit') {
            currentData = $scope.ProfitProcurementCostingList;

        }

        var data = ej.DataManager($scope.OrderCostingItemList).executeLocal(ej.Query().where("CostingComponentId", "equal", parseInt($scope.CostingComponentId), true));
        for (var i = 0; i < data.length; i++) {
            var single = ej.DataManager(currentData).executeLocal(ej.Query().where("CostingItemId", "equal", data[i].Id, true));
            if (single != null) {
                if (single.length > 0) {
                    try {
                        single[0].Amount = data[i].TotalProcurementGrossAmount;
                    } catch (e) {

                    }
                }

            }
        }
    }


    $scope.TotalDirectMaterialCost = 0;
    function getFixedAmountDirectMaterial() {
        $scope.TotalDirectMaterialCost = 0;
        var TotalPreviousAmount = 0;
        for (var i = 0; i < $scope.OrderCostingItemList.length; i++) {
            if ($scope.OrderCostingItemList[i].CostingSegment.toUpperCase() == 'DIRECTMATERIAL') {

                TotalPreviousAmount += $scope.OrderCostingItemList[i].TotalGrossAmount;
            }
        }
        $scope.TotalDirectMaterialCost = TotalPreviousAmount;
        return TotalPreviousAmount;
    }


    $scope.TotalProcurementDirectMaterialCost = 0;
    function getProcurementFixedAmountDirectMaterial() {
        $scope.TotalProcurementDirectMaterialCost = 0;
        var TotalPreviousAmount = 0;
        for (var i = 0; i < $scope.OrderCostingItemList.length; i++) {
            if ($scope.OrderCostingItemList[i].CostingSegment.toUpperCase() == 'DIRECTMATERIAL') {

                TotalPreviousAmount += $scope.OrderCostingItemList[i].TotalProcurementGrossAmount;
            }
        }
        $scope.TotalProcurementDirectMaterialCost = TotalPreviousAmount;
        return TotalPreviousAmount;
    }


    function getFixedAmount(sequence) {

        var TotalPreviousAmount = 0;
        for (var i = 0; i < $scope.OrderCostingItemList.length; i++) {
            if ($scope.OrderCostingItemList[i].ComponentSequence < sequence) {

                TotalPreviousAmount += $scope.OrderCostingItemList[i].TotalGrossAmount;
            }
        }

        return TotalPreviousAmount;
    }
    function getCurrentFixedAmount(sequence) {

        var TotalPreviousAmount = 0;
        for (var i = 0; i < $scope.OrderCostingItemList.length; i++) {

            if ($scope.OrderCostingItemList[i].ComponentSequence == sequence
                && ($scope.OrderCostingItemList[i].ValueType.toUpperCase() != 'PERCENTAGE')) {

                TotalPreviousAmount += $scope.OrderCostingItemList[i].TotalGrossAmount;
            }
            if ($scope.OrderCostingItemList[i].ComponentSequence == sequence
                && ($scope.OrderCostingItemList[i].Rate > 0 || $scope.OrderCostingItemList[i].ValueType.toUpperCase() == 'PERCENTAGE')) {

                TotalPreviousAmount += $scope.OrderCostingItemList[i].Rate;
            }
        }

        return TotalPreviousAmount;
    }
    function getCurrentPercent(sequence) {

        var TotalPreviousAmount = 0;
        for (var i = 0; i < $scope.OrderCostingItemList.length; i++) {

            if ($scope.OrderCostingItemList[i].ComponentSequence == sequence && $scope.OrderCostingItemList[i].ValueType.toUpperCase() == 'PERCENTAGE') {

                TotalPreviousAmount += $scope.OrderCostingItemList[i].Value;
            }
        }

        return TotalPreviousAmount;
    }


    function getProcurementFixedAmount(sequence) {

        var TotalPreviousAmount = 0;
        for (var i = 0; i < $scope.OrderCostingItemList.length; i++) {
            if ($scope.OrderCostingItemList[i].ComponentSequence < sequence) {

                TotalPreviousAmount += $scope.OrderCostingItemList[i].TotalProcurementGrossAmount;
            }
        }

        return TotalPreviousAmount;
    }
    function getProcurementCurrentFixedAmount(sequence) {

        var TotalPreviousAmount = 0;
        for (var i = 0; i < $scope.OrderCostingItemList.length; i++) {

            if ($scope.OrderCostingItemList[i].ComponentSequence == sequence
                && ($scope.OrderCostingItemList[i].ProcurementValueType.toUpperCase() != 'PERCENTAGE')) {

                TotalPreviousAmount += $scope.OrderCostingItemList[i].TotalProcurementGrossAmount;
            }
            if ($scope.OrderCostingItemList[i].ComponentSequence == sequence
                && ($scope.OrderCostingItemList[i].Rate > 0 || $scope.OrderCostingItemList[i].ProcurementValueType.toUpperCase() == 'PERCENTAGE')) {

                TotalPreviousAmount += $scope.OrderCostingItemList[i].ProcurementRate;
            }
        }

        return TotalPreviousAmount;
    }
    function getProcurementCurrentPercent(sequence) {

        var TotalPreviousAmount = 0;
        for (var i = 0; i < $scope.OrderCostingItemList.length; i++) {

            if ($scope.OrderCostingItemList[i].ComponentSequence == sequence && $scope.OrderCostingItemList[i].ProcurementValueType.toUpperCase() == 'PERCENTAGE') {

                TotalPreviousAmount += $scope.OrderCostingItemList[i].ProcurementValue;
            }
        }

        return TotalPreviousAmount;
    }



    $scope.TotalSegmentedValueByComponent = 0;


    function calValue(segmentName) {
        var sum = 0;
        for (var i = 0; i < $scope.OrderCostingItemList.length; i++) {
            if ($scope.OrderCostingItemList[i].CostingSegment == segmentName)
                sum += $scope.OrderCostingItemList[i].TotalGrossAmount;
        }
        return sum;
    }




    $scope.DirectMaterialList = [];
    $scope.SaveOrderPreCostingDirectMaterial = function () {
        $scope.hideDirectMaterialWithItemPopUp();

        if ($scope.DirectMaterialList.length > 0) {
            for (var i = 0; i < $scope.DirectMaterialList.length; i++) {
                if ($scope.DirectMaterialList[i].IsGeneric == false) {
                    $scope.DirectMaterialList[i].MaterialMasterId = null;
                    $scope.DirectMaterialList[i].ArticleId = null;
                }
                if (baseService.isUndefinedOrNull($scope.DirectMaterialList[i].Consumption) || $scope.DirectMaterialList[i].Consumption == 'NaN') {
                    ShowResult("Consumption is required for '" + $scope.DirectMaterialList[i].UserName + "'.", 'failure');
                    return false;
                }

                if (baseService.isUndefinedOrNull($scope.DirectMaterialList[i].Rate) || $scope.DirectMaterialList[i].Rate == 'NaN') {
                    ShowResult("Rate is required for '" + $scope.DirectMaterialList[i].UserName + "'.", 'failure');
                    return false;
                }

                if (baseService.isUndefinedOrNull($scope.DirectMaterialList[i].BOQCriteria)) {
                    ShowResult("BOQCriteria is required for '" + $scope.DirectMaterialList[i].UserName + "'.", 'failure');
                    return false;
                }
                if (baseService.isUndefinedOrNull($scope.DirectMaterialList[i].POCriteria)) {
                    ShowResult("POCriteria is required for '" + $scope.DirectMaterialList[i].UserName + "'.", 'failure');
                    return false;
                }
            }
        }

        $http({

            method: 'POST',
            url: 'Costings/OrderCosting/SaveOrderPreCostingDirectMaterial',
            data: { 'data': $scope.DirectMaterialList, OrderCostingMasterTemplateId: $scope.ModelNew.Id, 'cs': $scope.ModelNew.CostingStage },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.onChengeProductMaster();
            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        }
    };
    $scope.DirectProcurementCostingMaterialList = [];
    $scope.SaveOrderProcurementCostingDirectMaterial = function () {
        $scope.hideDirectMaterialWithItemPopUp();

        if ($scope.DirectProcurementCostingMaterialList.length > 0) {
            for (var i = 0; i < $scope.DirectProcurementCostingMaterialList.length; i++) {
                if ($scope.DirectProcurementCostingMaterialList[i].IsGeneric == false) {
                    $scope.DirectProcurementCostingMaterialList[i].MaterialMasterId = null;
                    $scope.DirectProcurementCostingMaterialList[i].ArticleId = null;
                }
                if (baseService.isUndefinedOrNull($scope.DirectProcurementCostingMaterialList[i].BOQCriteria)) {
                    ShowResult("BOQCriteria is required for '" + $scope.DirectProcurementCostingMaterialList[i].UserName + "'.", 'failure');
                    return false;
                }
                if (baseService.isUndefinedOrNull($scope.DirectProcurementCostingMaterialList[i].POCriteria)) {
                    ShowResult("POCriteria is required for '" + $scope.DirectProcurementCostingMaterialList[i].UserName + "'.", 'failure');
                    return false;
                }
            }
        }

        $http({

            method: 'POST',
            url: 'Costings/OrderCosting/SaveOrderProcurementCostingDirectMaterial',
            data: { 'data': $scope.DirectProcurementCostingMaterialList, OrderCostingMasterTemplateId: $scope.ModelNew.Id },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.onChengeProductMaster();
            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        }
    };

    var isExist = false;
    $scope.hideCostingItemListPopUp = function (x) {

        angular.element(document.querySelector("#costingItemListPopUp")).modal("hide");


        if ($scope.DirectMaterialList.length == 0 && x != undefined) {
            $scope.DirectMaterialList.push(x);
            return;
        }

        if ($scope.DirectMaterialList.length > 0) {
            for (var i = 0; i < $scope.DirectMaterialList.length; i++) {
                if (x.CostingItemId == $scope.DirectMaterialList[i].CostingItemId) {
                    isExist = true;
                    break;
                }
                else {
                    isExist = false;
                }
            }

            if (isExist == true) {
                ShowResult(x.UserName + ' already has been taken ', 'failure')
            }
            else {
                $scope.DirectMaterialList.push(x);
            }
        }
    };




    // #region Pre Costing Operation
    $scope.OperationList = [];
    $scope.showOperationPopUp = function () {

        //$scope.CostingComponentId = args.data.CostingComponentId;
        $scope.GetOperationWithItemByComponentId();
        angular.element(document.querySelector("#OperationPopUp")).modal("show");

    };
    $scope.GetOperationWithItemByComponentId = function () {
        $http({
            method: 'GET',
            url: 'Costings/OrderCosting/GetOperationWithItemByComponentId?costingComponentId=' + $scope.CostingComponentId + '&OrderCostingMasterTemplateId=' + $scope.ModelNew.Id,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.OperationList = response.data.Pre;
            $scope.OperationProcurementCostingList = response.data.Procurement;


            var elmnt = document.getElementById("CostingItemsEntry");
            elmnt.scrollIntoView(false, { behavior: "smooth", block: "end", inline: "nearest" });
        });
    }
    $scope.hideOperationPopUp = function () {
        angular.element(document.querySelector("#OperationPopUp")).modal("hide");
    };
    $scope.SaveOperation = function () {
        var flag = false;
        if ($scope.OperationList.length > 0) {

            //for (var i = 0; i < $scope.OperationList.length; i++) {
            //    if ($scope.OperationList[i].Value == 0 || baseService.isUndefinedOrNull($scope.OperationList[i].Value) || $scope.OperationList[i].Value == 'NaN') {
            //        ShowResult("Value is required for '" + $scope.OperationList[i].UserName + "'.", 'failure');
            //        return false;
            //    }
            //}

            if (flag == false) {
                $scope.hideOperationPopUp();
                $http({
                    method: 'POST',
                    url: 'Costings/OrderCosting/SaveOperation',
                    data: { 'data': $scope.OperationList, OrderCostingMasterTemplateId: $scope.ModelNew.Id, 'cs': $scope.ModelNew.CostingStage },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');

                        $scope.onChengeProductMaster();
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            }
        }
        else {
            $scope.hideOperationPopUp();
        }
    };

    $scope.OperationProcurementCostingList = [];
    $scope.SaveProcurementCostingOperation = function () {
        var flag = false;
        if ($scope.OperationProcurementCostingList.length > 0) {

            if (flag == false) {
                $scope.hideOperationPopUp();
                $http({
                    method: 'POST',
                    url: 'Costings/OrderCosting/SaveProcurementCostingOperation',
                    data: { 'data': $scope.OperationProcurementCostingList, OrderCostingMasterTemplateId: $scope.ModelNew.Id },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');

                        $scope.onChengeProductMaster();
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            }
        }
        else {
            $scope.hideOperationPopUp();
        }
    };

    $scope.showCostingItemListWithOperationPopUp = function (costingStage) {
        try {

            if (angular.isUndefinedOrNull($scope.OrderCostingMasterTemplateId))
                throw 'Please save the costing master first';

            $scope.CostingStage = costingStage;

            angular.element(document.querySelector("#costingItemListWithOperationPopUp")).modal("show");
            $scope.AddNewCostingItem();

        } catch (e) {
            ShowResult(e, "failure");
        }

    };
    $scope.hideCostingItemListWithOperationPopUp = function () {
        angular.element(document.querySelector("#costingItemListWithOperationPopUp")).modal("hide");

    }
    $scope.CreateNewPopUpConfirmation = function () {

        if ($scope.CostingStage == 'PRE')
            angular.element(document.querySelector("#addNewItem")).modal("show");
        else if ($scope.CostingStage == 'PROCUREMENT') {
            $scope.SaveCostingItemsForCostingComponent();
            $scope.hideCostingItemListWithOperationPopUp();
        }
    }

    $scope.CostingItemWithOperationList = [];
    $scope.GetCostingItemWithOperationByComponentId = function () {

        $http({
            method: 'GET',
            url: 'Costings/OrderCosting/GetCostingItemWithOperationByComponentId?costingComponentId=' + $scope.CostingComponentId,

            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.CostingItemWithOperationList = response.data;

        });
    };

    $scope.OperationList = [];
    var isExist = false;
    $scope.hideCostingItemListWithOerationPopUp = function (x) {

        angular.element(document.querySelector("#costingItemListWithOperationPopUp")).modal("hide");


        if ($scope.OperationList.length == 0 && x != undefined) {
            $scope.OperationList.push(x);
            return;
        }

        if ($scope.OperationList.length > 0) {
            for (var i = 0; i < $scope.OperationList.length; i++) {
                if (x.CostingItemId == $scope.OperationList[i].CostingItemId) {
                    isExist = true;
                    break;
                }
                else {
                    isExist = false;
                }
            }

            if (isExist == true) {
                ShowResult(x.UserName + ' already has been taken ', 'failure')
            }
            else {
                $scope.OperationList.push(x);
            }
        }
    };
    $scope.DeleteOperationPopUp = function (x) {
        $scope.operationId = x.Id;
        $scope.message_confirmation = "Are you sure to Delete permanently ?";
        angular.element(document.querySelector("#DeleteOperationPopUp")).modal("show");
    };



    $scope.directProcessExecutionTypeList = ['Inside', 'Outside'];
    $scope.DirectProcessList = [];
    $scope.showDirectProcessPopUp = function () {
        $scope.GetDirectProcessWithItemByComponentId();
        angular.element(document.querySelector("#DirectProcessPopUp")).modal("show");
    };
    $scope.GetDirectProcessWithItemByComponentId = function () {
        $http({
            method: 'GET',
            url: 'Costings/OrderCosting/GetDirectProcessWithItemByComponentId?costingComponentId=' + $scope.CostingComponentId + '&OrderCostingMasterTemplateId=' + $scope.ModelNew.Id,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.DirectProcessList = response.data.Pre;
            $scope.DirectProcessProcurementCostingList = response.data.Procurement;


            var elmnt = document.getElementById("CostingItemsEntry");
            elmnt.scrollIntoView(false, { behavior: "smooth", block: "end", inline: "nearest" });
        });
    }
    $scope.hideDirectProcessPopUp = function () {
        angular.element(document.querySelector("#DirectProcessPopUp")).modal("hide");
    };
    $scope.SaveDirectProcess = function () {
        var flag = false;
        if ($scope.DirectProcessList.length > 0) {

            //for (var i = 0; i < $scope.DirectProcessList.length; i++) {
            //    if ($scope.DirectProcessList[i].Rate == 0 || baseService.isUndefinedOrNull($scope.DirectProcessList[i].Rate) || $scope.DirectProcessList[i].Rate == 'NaN') {
            //        ShowResult("Rate is required for '" + $scope.DirectProcessList[i].UserName + "'.", 'failure');
            //        return false;
            //    }
            //}

            if (flag == false) {
                $scope.hideDirectProcessPopUp();
                $http({
                    method: 'POST',
                    url: 'Costings/OrderCosting/SaveDirectProcess',
                    data: { 'data': $scope.DirectProcessList, OrderCostingMasterTemplateId: $scope.ModelNew.Id, 'cs': $scope.ModelNew.CostingStage },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');

                        $scope.onChengeProductMaster();
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            }
        }
        else {
            $scope.hideDirectProcessPopUp();
        }
    };

    $scope.DirectProcessProcurementCostingList = [];
    $scope.SaveProcurementCostingDirectProcess = function () {
        var flag = false;
        if ($scope.DirectProcessProcurementCostingList.length > 0) {

            if (flag == false) {
                $scope.hideDirectProcessPopUp();
                $http({
                    method: 'POST',
                    url: 'Costings/OrderCosting/SaveProcurementCostingDirectProcess',
                    data: { 'data': $scope.DirectProcessProcurementCostingList, OrderCostingMasterTemplateId: $scope.ModelNew.Id },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');

                        $scope.onChengeProductMaster();
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            }
        }
        else {
            $scope.hideDirectProcessPopUp();
        }
    };

    $scope.showCostingItemListWithDirectProcessPopUp = function () {



        angular.element(document.querySelector("#costingItemListWithDirectProcessPopUp")).modal("show");
    };
    $scope.hideCostingItemListWithDirectProcessPopUp = function () {
        angular.element(document.querySelector("#costingItemListWithDirectProcessPopUp")).modal("hide");
    }

    $scope.CostingItemWithDirectProcessList = [];
    $scope.GetCostingItemWithDirectProcessByComponentId = function () {

        $http({
            method: 'GET',
            url: 'Costings/OrderCosting/GetCostingItemWithDirectProcessByComponentId?costingComponentId=' + $scope.CostingComponentId,

            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.CostingItemWithDirectProcessList = response.data;

        });
    };

    var isExist = false;
    $scope.hideCostingItemListWithDirectProcessPopUp = function (x) {

        angular.element(document.querySelector("#costingItemListWithDirectProcessPopUp")).modal("hide");


        if ($scope.DirectProcessList.length == 0 && x != undefined) {
            $scope.DirectProcessList.push(x);
            return;
        }

        if ($scope.DirectProcessList.length > 0) {
            for (var i = 0; i < $scope.DirectProcessList.length; i++) {
                if (x.CostingItemId == $scope.DirectProcessList[i].CostingItemId) {
                    isExist = true;
                    break;
                }
                else {
                    isExist = false;
                }
            }

            if (isExist == true) {
                ShowResult(x.UserName + ' already has been taken ', 'failure')
            }
            else {
                $scope.DirectProcessList.push(x);
            }
        }
    };
    $scope.DeleteDirectProcessPopUp = function (x) {
        $scope.directProcessId = x.Id;
        $scope.message_confirmation = "Are you sure to Delete permanently ?";
        angular.element(document.querySelector("#DeleteDirectProcessPopUp")).modal("show");
    };

    $scope.DeleteDirectProcess = function () {
        $http({
            method: 'POST',
            url: 'Costings/OrderCosting/DeleteDirectProcess',
            data: { 'directProcessId': $scope.directProcessId },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');



                angular.element(document.querySelector("#DeleteDirectProcessPopUp")).modal("hide");

                $scope.GetDirectProcessWithItemByComponentId();
                $scope.GetCostingItemsWithoutFilterForDirectProcess();

                $scope.UpdateGridItems();
                $scope.RefreshComponenctGridItems();
            }
        }, function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
        });
    };
    //$scope.data = [];
    $scope.GetCostingItemsWithoutFilterForDirectProcess = function () {

        $scope.data = [];
        try {
            $http({
                method: 'GET',

                url: $scope.path + 'GetCostingItemsWithoutFilterForDirectProcess?OrderCostingMasterTemplateId=' + $scope.ModelNew.Id + '&costingComponentId=' + $scope.CostingComponentId,
                dataType: 'JSON'
            }).then(function successCallback(response) {

                $scope.data = response.data;
                $scope.UpdateGridItems();
            });
        } catch (e) {

        }
    }

    // #endregion OrderCosting Direct Process

    // #region OrderCosting SalesExpense

    $scope.SalesExpenseList = [];
    $scope.showSalesExpensePopUp = function () {
        $scope.GetSalesExpenseWithItemByComponentId();
        angular.element(document.querySelector("#SalesExpensePopUp")).modal("show");
    };

    $scope.showCostingItemListWithSalesExpensePopUp = function () {
        $scope.GetCostingItemWithSalesExpenseByComponentId();

        angular.element(document.querySelector("#costingItemListWithSalesExpensePopUp")).modal("show");
    };
    $scope.hideCostingItemListWithSalesExpensePopUp = function () {
        angular.element(document.querySelector("#costingItemListWithSalesExpensePopUp")).modal("hide");
    }

    $scope.GetSalesExpenseWithItemByComponentId = function () {
        $http({
            method: 'GET',
            url: 'Costings/OrderCosting/GetSalesExpenseWithItemByComponentId?costingComponentId=' + $scope.CostingComponentId + '&OrderCostingMasterTemplateId=' + $scope.ModelNew.Id,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.SalesExpenseList = response.data.Pre;
            $scope.SalesExpenseProcurementCostingList = response.data.Procurement;


            var elmnt = document.getElementById("CostingItemsEntry");
            elmnt.scrollIntoView(false, { behavior: "smooth", block: "end", inline: "nearest" });
        });
    }
    $scope.hideSalesExpensePopUp = function () {
        angular.element(document.querySelector("#SalesExpensePopUp")).modal("hide");
    };
    $scope.SaveSalesExpense = function () {
        var flag = false;
        if ($scope.SalesExpenseList.length > 0) {

            try {

                for (var i = 0; i < $scope.SalesExpenseList.length; i++) {
                    //if ($scope.SalesExpenseList[i].Value == 0 || baseService.isUndefinedOrNull($scope.SalesExpenseList[i].Value) || $scope.SalesExpenseList[i].Value == 'NaN') {
                    //    throw "Value is required for '" + $scope.SalesExpenseList[i].UserName + "'.";
                    //}

                    if ($scope.SalesExpenseList[i].Value > 0) {
                        if (angular.isUndefinedOrNull($scope.SalesExpenseList[i].Type))
                            throw 'Type is missing';
                    }
                }

                if (flag == false) {
                    $scope.hideSalesExpensePopUp();
                    $http({
                        method: 'POST',
                        url: 'Costings/OrderCosting/SaveSalesExpense',
                        data: { 'data': $scope.SalesExpenseList, OrderCostingMasterTemplateId: $scope.ModelNew.Id, 'cs': $scope.ModelNew.CostingStage },
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure');
                        }
                        else {
                            ShowResult(response.data.Message, 'success');

                            $scope.onChengeProductMaster();
                        }
                    }), function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    }
                }
            } catch (e) {
                ShowResult(e, 'failure');
            }


        }
        else {
            $scope.hideSalesExpensePopUp();
        }
    };

    $scope.SalesExpenseProcurementCostingList = [];
    $scope.SaveProcurementCostingSalesExpense = function () {
        var flag = false;
        if ($scope.SalesExpenseProcurementCostingList.length > 0) {

            try {

                for (var i = 0; i < $scope.SalesExpenseProcurementCostingList.length; i++) {
                    if ($scope.SalesExpenseProcurementCostingList[i].Value > 0) {
                        if (angular.isUndefinedOrNull($scope.SalesExpenseProcurementCostingList[i].Type))
                            throw 'Type is missing';
                    }
                }

                if (flag == false) {
                    $scope.hideSalesExpensePopUp();
                    $http({
                        method: 'POST',
                        url: 'Costings/OrderCosting/SaveProcurementCostingSalesExpense',
                        data: { 'data': $scope.SalesExpenseProcurementCostingList, OrderCostingMasterTemplateId: $scope.ModelNew.Id },
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure');
                        }
                        else {
                            ShowResult(response.data.Message, 'success');

                            $scope.onChengeProductMaster();
                        }
                    }), function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    }
                }
            } catch (e) {
                ShowResult(e, 'failure');
            }


        }
        else {
            $scope.hideSalesExpensePopUp();
        }
    };

    $scope.CostingItemWithSalesExpenseList = [];
    $scope.GetCostingItemWithSalesExpenseByComponentId = function () {

        $http({
            method: 'GET',
            url: 'Costings/OrderCosting/GetCostingItemWithSalesExpenseByComponentId?costingComponentId=' + $scope.CostingComponentId,

            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.CostingItemWithSalesExpenseList = response.data;

        });
    };

    var isExist = false;
    $scope.hideCostingItemListWithSalesExpensePopUp = function (x) {

        angular.element(document.querySelector("#costingItemListWithSalesExpensePopUp")).modal("hide");


        if ($scope.SalesExpenseList.length == 0 && x != undefined) {
            $scope.SalesExpenseList.push(x);
            return;
        }

        if ($scope.SalesExpenseList.length > 0) {
            for (var i = 0; i < $scope.SalesExpenseList.length; i++) {
                if (x.CostingItemId == $scope.SalesExpenseList[i].CostingItemId) {
                    isExist = true;
                    break;
                }
                else {
                    isExist = false;
                }
            }

            if (isExist == true) {
                ShowResult(x.UserName + ' already has been taken ', 'failure')
            }
            else {
                $scope.SalesExpenseList.push(x);
            }
        }
    };
    $scope.DeleteSalesExpensePopUp = function (x) {
        $scope.salesExpenseId = x.Id;
        $scope.message_confirmation = "Are you sure to Delete permanently ?";
        angular.element(document.querySelector("#DeleteSalesExpensePopUp")).modal("show");
    };

    $scope.DeleteSalesExpense = function () {
        $http({
            method: 'POST',
            url: 'Costings/OrderCosting/DeleteSalesExpense',
            data: { 'salesExpenseId': $scope.salesExpenseId },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                angular.element(document.querySelector("#DeleteSalesExpensePopUp")).modal("hide");

                $scope.GetSalesExpenseWithItemByComponentId();
                $scope.GetCostingItemsWithoutFilterForSalesExpense();

                $scope.UpdateGridItems();
                $scope.RefreshComponenctGridItems();
            }
        }, function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
        });
    };
    $scope.GetCostingItemsWithoutFilterForSalesExpense = function () {

        $scope.data = [];
        try {
            $http({
                method: 'GET',

                url: $scope.path + 'GetCostingItemsWithoutFilterForSalesExpense?OrderCostingMasterTemplateId=' + $scope.ModelNew.Id + '&costingComponentId=' + $scope.CostingComponentId,
                dataType: 'JSON'
            }).then(function successCallback(response) {

                $scope.data = response.data;
                $scope.UpdateGridItems();
            });
        } catch (e) {

        }
    }
    // #endregion OrderCosting SalesExpense

    // #region ValueLoss
    $scope.ValueLossList = [];
    $scope.showValueLossPopUp = function () {
        $scope.GetValueLossWithItemByComponentId();
        angular.element(document.querySelector("#ValueLossPopUp")).modal("show");
    };

    $scope.showCostingItemListWithValueLossPopUp = function () {
        $scope.GetCostingItemWithValueLossByComponentId();

        angular.element(document.querySelector("#costingItemListWithValueLossPopUp")).modal("show");
    };
    $scope.hideCostingItemListWithValueLossPopUp = function () {
        angular.element(document.querySelector("#costingItemListWithValueLossPopUp")).modal("hide");
    }

    $scope.GetValueLossWithItemByComponentId = function () {
        $http({
            method: 'GET',
            url: 'Costings/OrderCosting/GetValueLossWithItemByComponentId?costingComponentId=' + $scope.CostingComponentId + '&OrderCostingMasterTemplateId=' + $scope.ModelNew.Id,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ValueLossList = response.data.Pre;
            $scope.ValueLossProcurementCostingList = response.data.Procurement;


            var elmnt = document.getElementById("CostingItemsEntry");
            elmnt.scrollIntoView(false, { behavior: "smooth", block: "end", inline: "nearest" });
        });
    }

    $scope.ProfitList = [];
    $scope.GetProfitWithItemByComponentId = function () {
        $http({
            method: 'GET',
            url: 'Costings/OrderCosting/GetProfitWithItemByComponentId?costingComponentId=' + $scope.CostingComponentId + '&OrderCostingMasterTemplateId=' + $scope.ModelNew.Id,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ProfitList = response.data.Pre;
            $scope.ProfitProcurementCostingList = response.data.Procurement;


            var elmnt = document.getElementById("CostingItemsEntry");
            elmnt.scrollIntoView(false, { behavior: "smooth", block: "end", inline: "nearest" });
        });
    }

    $scope.hideValueLossPopUp = function () {
        angular.element(document.querySelector("#ValueLossPopUp")).modal("hide");
    };
    $scope.SaveValueLoss = function () {

        try {


            var flag = false;
            if ($scope.ValueLossList.length > 0) {
                for (var i = 0; i < $scope.ValueLossList.length; i++) {
                    if ($scope.ValueLossList[i].Value == 0 || baseService.isUndefinedOrNull($scope.ValueLossList[i].Value) || $scope.ValueLossList[i].Value == 'NaN') {
                        throw "Value is required for '" + $scope.ValueLossList[i].UserName + "'.";
                    }

                    if ($scope.ValueLossList[i].Value > 0) {
                        if (angular.isUndefinedOrNull($scope.ValueLossList[i].Type))
                            throw 'Type is missing';
                    }
                }
                if (flag == false) {

                    $scope.hideValueLossPopUp();

                    $http({
                        method: 'POST',
                        url: 'Costings/OrderCosting/SaveValueLoss',
                        data: { 'data': $scope.ValueLossList, OrderCostingMasterTemplateId: $scope.ModelNew.Id, 'cs': $scope.ModelNew.CostingStage },
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure');
                        }
                        else {
                            ShowResult(response.data.Message, 'success');

                            $scope.onChengeProductMaster();
                        }
                    }), function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    }
                }
            }
            else {
                $scope.hideValueLossPopUp();
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.ValueLossProcurementCostingList = [];
    $scope.SaveProcurementCostingValueLoss = function () {

        try {


            var flag = false;
            if ($scope.ValueLossProcurementCostingList.length > 0) {
                for (var i = 0; i < $scope.ValueLossProcurementCostingList.length; i++) {
                    if ($scope.ValueLossProcurementCostingList[i].Value > 0) {
                        if (angular.isUndefinedOrNull($scope.ValueLossProcurementCostingList[i].Type))
                            throw 'Type is missing';
                    }
                }
                if (flag == false) {

                    $scope.hideValueLossPopUp();

                    $http({
                        method: 'POST',
                        url: 'Costings/OrderCosting/SaveProcurementCostingValueLoss',
                        data: { 'data': $scope.ValueLossProcurementCostingList, OrderCostingMasterTemplateId: $scope.ModelNew.Id },
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure');
                        }
                        else {
                            ShowResult(response.data.Message, 'success');

                            $scope.onChengeProductMaster();
                        }
                    }), function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    }
                }
            }
            else {
                $scope.hideValueLossPopUp();
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.SaveProfit = function () {
        try {
            var flag = false;
            if ($scope.ProfitList.length > 0) {

                for (var i = 0; i < $scope.ProfitList.length; i++) {
                    if ($scope.ProfitList[i].Value == 0 || baseService.isUndefinedOrNull($scope.ProfitList[i].Value) || $scope.ProfitList[i].Value == 'NaN') {
                        throw "Value is required for '" + $scope.ProfitList[i].UserName + "'.";
                    }

                    if ($scope.ProfitList[i].Value > 0) {
                        if (angular.isUndefinedOrNull($scope.ProfitList[i].Type))
                            throw 'Type is missing';
                    }
                }

                if (flag == false) {


                    $http({
                        method: 'POST',
                        url: 'Costings/OrderCosting/SaveProfit',
                        data: { 'data': $scope.ProfitList, OrderCostingMasterTemplateId: $scope.ModelNew.Id, 'cs': $scope.ModelNew.CostingStage },
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure');
                        }
                        else {
                            ShowResult(response.data.Message, 'success');

                            $scope.onChengeProductMaster();
                        }
                    }), function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    }
                }
            }

        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.ProfitProcurementCostingList = [];
    $scope.SaveProcurementCostingProfit = function () {
        try {
            var flag = false;
            if ($scope.ProfitProcurementCostingList.length > 0) {

                for (var i = 0; i < $scope.ProfitProcurementCostingList.length; i++) {
                    if ($scope.ProfitProcurementCostingList[i].Value > 0) {
                        if (angular.isUndefinedOrNull($scope.ProfitProcurementCostingList[i].Type))
                            throw 'Type is missing';
                    }
                }

                if (flag == false) {


                    $http({
                        method: 'POST',
                        url: 'Costings/OrderCosting/SaveProcurementCostingProfit',
                        data: { 'data': $scope.ProfitProcurementCostingList, OrderCostingMasterTemplateId: $scope.ModelNew.Id },
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure');
                        }
                        else {
                            ShowResult(response.data.Message, 'success');

                            $scope.onChengeProductMaster();
                        }
                    }), function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    }
                }
            }

        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.CostingItemWithValueLossList = [];
    $scope.GetCostingItemWithValueLossByComponentId = function () {

        $http({
            method: 'GET',
            url: 'Costings/OrderCosting/GetCostingItemWithValueLossByComponentId?costingComponentId=' + $scope.CostingComponentId,

            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.CostingItemWithValueLossList = response.data;

        });
    };

    var isExist = false;
    $scope.hideCostingItemListWithValueLossPopUp = function (x) {

        angular.element(document.querySelector("#costingItemListWithValueLossPopUp")).modal("hide");


        if ($scope.ValueLossList.length == 0 && x != undefined) {
            $scope.ValueLossList.push(x);
            return;
        }

        if ($scope.ValueLossList.length > 0) {
            for (var i = 0; i < $scope.ValueLossList.length; i++) {
                if (x.CostingItemId == $scope.ValueLossList[i].CostingItemId) {
                    isExist = true;
                    break;
                }
                else {
                    isExist = false;
                }
            }

            if (isExist == true) {
                ShowResult(x.UserName + ' already has been taken ', 'failure')
            }
            else {
                $scope.ValueLossList.push(x);
            }
        }
    };
    $scope.DeleteValueLossPopUp = function (x) {
        $scope.ValueLossId = x.Id;
        $scope.message_confirmation = "Are you sure to Delete permanently ?";
        angular.element(document.querySelector("#DeleteValueLossPopUp")).modal("show");
    };

    $scope.DeleteValueLoss = function () {
        $http({
            method: 'POST',
            url: 'Costings/OrderCosting/DeleteValueLoss',
            data: { 'ValueLossId': $scope.ValueLossId },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                angular.element(document.querySelector("#DeleteValueLossPopUp")).modal("hide");

                $scope.GetValueLossWithItemByComponentId();
                $scope.GetCostingItemsWithoutFilterForValueLoss();

                $scope.UpdateGridItems();
                $scope.RefreshComponenctGridItems();
            }
        }, function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
        });
    };
    $scope.GetCostingItemsWithoutFilterForValueLoss = function () {

        $scope.data = [];
        try {
            $http({
                method: 'GET',

                url: $scope.path + 'GetCostingItemsWithoutFilterForValueLoss?OrderCostingMasterTemplateId=' + $scope.ModelNew.Id + '&costingComponentId=' + $scope.CostingComponentId,
                dataType: 'JSON'
            }).then(function successCallback(response) {

                $scope.data = response.data;
                $scope.UpdateGridItems();
            });
        } catch (e) {

        }
    }
    // #endregion 




    $scope.Segmentgrid = function detailGridData(arg) {
        try {
            $scope.tempComponenListBySegment = [];
            $scope.Segment = arg;
            if ($scope.OrderCostingDetailList.length > 0) {
                for (var i = 0; i < $scope.OrderCostingDetailList.length; i++) {
                    if ($scope.OrderCostingDetailList[i].CostingSegment == arg) {
                        $scope.tempComponenListBySegment.push($scope.OrderCostingDetailList[i]);
                    }
                }
            }

        } catch (e) {
            var k = e;
        }

        //$scope.CalculateTotalComponetValueForPerSegment();
        //$scope.loadDataForEdit();

    }
    //$scope.GetCostingItemsWithoutFilter();


    //$scope.Segment == 'DirectMaterial';

    $scope.RefreshComponenctGridItems = function () {
        var _totalGrossAmount = 0;
        if ($scope.Segment == 'DirectMaterial') {

            _totalGrossAmount = $scope.totalItemGrossAmount;
        }
        else if ($scope.Segment == 'Operation') {

            _totalGrossAmount = $scope.totalOperationValue;
        }
        else if ($scope.Segment == 'DirectProcess') {

            _totalGrossAmount = $scope.totalDirectProcessAmount;

        }
        else if ($scope.Segment == 'SalesExpense') {

            _totalGrossAmount = $scope.totalSalesExpenseAmount;

        }
        else if ($scope.Segment == 'ValueLoss') {

            _totalGrossAmount = $scope.totalValueLossAmount;

        }


        for (var i = 0; i < $scope.tempComponenListBySegment.length; i++) {
            if ($scope.tempComponenListBySegment[i].CostingComponentId == $scope.CostingComponentId)
                $scope.tempComponenListBySegment[i].TotalGrossAmount = _totalGrossAmount;
        }
        var gridObj = $("#GridOperation").data("ejGrid");
        gridObj.refreshContent(true);
        gridObj.refreshTemplate();


    };
    $scope.GridOperationSummaryRows = [{
        title: "Total Amount", summaryColumns: [{ summaryType: ej.Grid.SummaryType.Sum, displayColumn: "CostingValue", dataMember: "CostingValue", format: "{0:N2}" },
        { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "BuyerTarget", dataMember: "BuyerTarget", format: "{0:N2}" },
        { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "TotalGrossAmount", dataMember: "TotalGrossAmount", format: "{0:N2}" }],
        showCaptionSummary: true

    }];
    $scope.GridSummaryBySegmentSummaryRows = [{
        title: "Total", summaryColumns: [
            { summaryType: ej.Grid.SummaryType.Sum, textAlign: 'right', displayColumn: "BuyerTarget", dataMember: "BuyerTarget", format: "{0:N2}" },
            { summaryType: ej.Grid.SummaryType.Sum, textAlign: 'right', displayColumn: "CostingValue", dataMember: "CostingValue", format: "{0:N2}" },
            { summaryType: ej.Grid.SummaryType.Sum, textAlign: 'right', displayColumn: "TotalGrossAmount", dataMember: "TotalGrossAmount", format: "{0:N2}" },
            { summaryType: ej.Grid.SummaryType.Sum, textAlign: 'right', displayColumn: "TotalProcurementGrossAmount", dataMember: "TotalProcurementGrossAmount", format: "{0:N2}" }],
        showCaptionSummary: true

    }];

    $scope.selectedComponentRowDetail = null;
    $scope.detailgrid = function detailGridData(e) {
        $scope.selectedComponentRowDetail = e;
        $scope.CostingComponentId = e.data["CostingComponentId"];

        if ($scope.Segment == 'DirectMaterial') {
            $scope.GetCostingItemsWithoutFilter();
        }
        else if ($scope.Segment == 'Operation') {
            $scope.GetCostingItemsWithoutFilterForOperation();
        }
        else if ($scope.Segment == 'DirectProcess') {
            $scope.GetCostingItemsWithoutFilterForDirectProcess();
        }
        else if ($scope.Segment == 'SalesExpense') {
            $scope.GetCostingItemsWithoutFilterForSalesExpense();
        }
        else if ($scope.Segment == 'ValueLoss') {
            $scope.GetCostingItemsWithoutFilterForValueLoss();
        }


        // $scope.getCostingItemByComponentId();
        //$scope.data = ej.DataManager($scope.CostingItemListWithoutFilter).executeLocal(ej.Query().where("CostingComponentId", "equal", parseInt($scope.CostingComponentId), true).take(100));


        //e.detailsElement.find(".tabcontrol").ejTab();
    }

    $scope.expanded = [];
    $scope.flag = true;
    $scope.expand = function (args) {
        if ($scope.flag) {
            $scope.expanded.push(args.masterRow[0].rowIndex);
            $scope.flag = true;
        }
    }


    $scope.tempComponenListBySegment = [];
    $scope.AssignSegmentByeDirectMaterial = function () {
        $scope.tempComponenListBySegment = [];

        var kk = 0;
        try {
            for (var i = 0; baseService.arrayLength($scope.OrderCostingDetailList); i++) {
                kk = i;
                if ($scope.OrderCostingDetailList[i].CostingSegment == 'DirectMaterial') {
                    $scope.tempComponenListBySegment.push($scope.OrderCostingDetailList[i]);
                }
            }

        } catch (e) {

        }

        //var obj = $("#GridOperation").ejGrid("instance");
        ////obj.model.childGrid.dataSource = data1;  //updating the dataSource of childgrid 
        //$("#GridOperation").ejGrid("dataSource", $scope.tempComponenListBySegment); //updating the dataSource of parent grid 
        //// $scope.loadDataForEdit();
        //var gridObj = $("#GridOperation").data("ejGrid");
        //gridObj.refreshContent(true);
        //gridObj.refreshTemplate();

    }




    //#endregion

    //#region Buyer Information
    $scope.CostingBuyer = {
        Id: null,
        OrderCostingMasterTemplateId: null,
        BuyerId: null,
        BuyerStyleRefNo: null,
        OwnStyleRefNo: null
    }
    $scope.costingBuyerNew = Object.assign({}, $scope.CostingBuyer);

    $scope.AddNewBuyer = function () {
        $scope.CostingBuyer = {};
        $scope.costingBuyerNew = {};
        angular.element(document.querySelector('#BuyerPoUp')).modal('show');
    }


    $scope.bulletinTemplate = {
        Id: null,
        CompanyGroupId: null,
        BulletinName: null,
        AlternativeName: null,
        ByWhom: null,
        ProductMasterId: null,
        SizeGroupId: null
    };
    $scope.bulletinTemplateNew = Object.assign({}, $scope.bulletinTemplate);
    $scope.BuyerAction = 'Save'
    $scope.SaveBuyer = function () {
        angular.copy($scope.costingBuyerNew, $scope.CostingBuyer);
        $scope.CostingBuyer.OrderCostingMasterTemplateId = $scope.ModelNew.Id;
        $scope.$broadcast('show-errors-check-validity');
        try {
            if ($scope.BuyerForm.$valid) {
                if ($scope.BuyerAction === 'Save') {
                    $http({
                        method: 'POST',
                        url: 'Costings/OrderCosting/CreateCostingBuyer',
                        data: { data: $scope.CostingBuyer },
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure', 'BuyerPoUp');
                        }
                        else {
                            ShowResult(response.data.Message, 'success', 'BuyerPoUp');
                            angular.element(document.querySelector('#BuyerPoUp')).modal('hide');
                        }
                    }), function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure', 'BuyerPoUp');
                    };
                }
                else if ($scope.BuyerAction === 'Update') {
                    $http({
                        method: 'POST',
                        url: 'Costings/OrderCosting/CreateCostingBuyer',
                        data: $scope.CostingBuyer,
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure', 'BuyerPoUp');
                        }
                        else {
                            ShowResult(response.data.Message, 'success', 'BuyerPoUp');
                            angular.element(document.querySelector('#BuyerPoUp')).modal('hide');
                        }
                    }, function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure', 'BuyerPoUp');
                    });
                }
            }
        } catch (e) {
            ShowResult(e, 'failure', 'BuyerPoUp');
        }
    };
    $scope.CloseBuyer = function () {
        angular.element(document.querySelector('#BuyerPoUp')).modal('hide');
    }

    //#endregion 

    //#region Profit Calculation
    $scope.GrossProfit = 0;
    $scope.MKTGainOrLoss = 0;
    $scope.NetProfit = 0;
    ``
    $scope.CalculateProfit = function () {

        if ($scope.ModelNew.IsPercentage != null && $scope.ModelNew.IsPercentage == 'true') {
            //Percentage
            if ($scope.ModelNew.TargetProfit != NaN)
                $scope.GrossProfit = $scope.ModelNew.TargetSellingPrice - ($scope.ModelNew.TargetSellingPrice * (100 / (100 + $scope.ModelNew.TargetProfit)));
        }
        else {

            //Fixed
            if ($scope.ModelNew.TargetSellingPrice != NaN && $scope.ModelNew.TargetProfit != NaN)
                $scope.GrossProfit = $scope.ModelNew.TargetProfit;
        }
        $scope.SumNetProfitOfGross();
    }
    //#endregion end Profit Calculation

    $scope.CalculateGrossConsumption_Amount = function (x) {
        var GrossConsumption = (x.GrossConsumption * x.Consumption / 100) + x.GrossConsumption;
        x.GrossConsumption = GrossConsumption;

        x.GrossAmount = x.GrossConsumption * x.Rate;

    }

    //delete DirectMaterial
    $scope.DeleteOrderPreCostingDirectMaterialPopUp = function (x) {
        $scope.DirectMaterialId = x.Id;
        $scope.message_confirmation = "Are you sure to Delete permanently ?";
        angular.element(document.querySelector("#DeleteOrderPreCostingDirectMaterialPopUp")).modal("show");
    };

    $scope.DeleteDirectMaterial = function () {
        $http({
            method: 'POST',
            url: 'Costings/OrderCosting/DeleteDirectMaterial',
            data: { 'DirectMaterialId': $scope.DirectMaterialId },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                angular.element(document.querySelector("#DeleteOrderPreCostingDirectMaterialPopUp")).modal("hide");
                //$scope.DirectMaterialList = [];
                $scope.GetDirectCostingMaterialWithItemByComponentId();
            }
        }, function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
        });
    };

    // #region Costing formula 
    $scope.TotalDirectMaterial = 0;
    $scope.TotalOperation = 0;
    $scope.TotalProcess = 0;



    // #endregion end Costing formula 
    // #region Responsible Person
    $scope.employee = [];
    $scope.getPopUpData = function (index, entryStage) {
        $scope.CostingStage = entryStage;
        $scope.index = index;
        $scope.employee = [];
        $http({
            method: 'GET',
            url: 'Costings/OrderCosting/getemployeelist'
        }).then(function successCallback(response) {
            $scope.employee = response.data;
        });
        angular.element(document.querySelector('#employeeNewPopUp')).modal('show');
    }

    $scope.setEmpData = function (obj) {


        var data = obj.data;
        if ($scope.CostingStage == 'PRE') {
            if ($scope.Segment == 'DirectMaterial') {
                $scope.DirectMaterialList[$scope.index].ResponsiblePersonId = data.SystemID;
                $scope.DirectMaterialList[$scope.index].ResponsiblePerson = data.EmployeeName;
            }
            else if ($scope.Segment == 'Operation') {

                $scope.OperationList[$scope.index].ResponsiblePersonId = data.SystemID;
                $scope.OperationList[$scope.index].ResponsiblePerson = data.EmployeeName;
            }
            else if ($scope.Segment == 'DirectProcess') {

                $scope.DirectProcessList[$scope.index].ResponsiblePersonId = data.SystemID;
                $scope.DirectProcessList[$scope.index].ResponsiblePerson = data.EmployeeName;
            }
            else if ($scope.Segment == 'SalesExpense') {

                $scope.SalesExpenseList[$scope.index].ResponsiblePersonId = data.SystemID;
                $scope.SalesExpenseList[$scope.index].ResponsiblePerson = data.EmployeeName;
            }
            else if ($scope.Segment == 'ValueLoss') {

                $scope.ValueLossList[$scope.index].ResponsiblePersonId = data.SystemID;
                $scope.ValueLossList[$scope.index].ResponsiblePerson = data.EmployeeName;
            }
            else if ($scope.Segment == 'Profit') {

                $scope.ProfitList[$scope.index].ResponsiblePersonId = data.SystemID;
                $scope.ProfitList[$scope.index].ResponsiblePerson = data.EmployeeName;
            }
        }
        else if ($scope.CostingStage == 'PROCUREMENT') {
            if ($scope.Segment == 'DirectMaterial') {
                $scope.DirectProcurementCostingMaterialList[$scope.index].ResponsiblePersonId = data.SystemID;
                $scope.DirectProcurementCostingMaterialList[$scope.index].ResponsiblePerson = data.EmployeeName;
            }
            else if ($scope.Segment == 'Operation') {

                $scope.OperationProcurementCostingList[$scope.index].ResponsiblePersonId = data.SystemID;
                $scope.OperationProcurementCostingList[$scope.index].ResponsiblePerson = data.EmployeeName;
            }
            else if ($scope.Segment == 'DirectProcess') {

                $scope.DirectProcessProcurementCostingList[$scope.index].ResponsiblePersonId = data.SystemID;
                $scope.DirectProcessProcurementCostingList[$scope.index].ResponsiblePerson = data.EmployeeName;
            }
            else if ($scope.Segment == 'SalesExpense') {

                $scope.SalesExpenseProcurementCostingList[$scope.index].ResponsiblePersonId = data.SystemID;
                $scope.SalesExpenseProcurementCostingList[$scope.index].ResponsiblePerson = data.EmployeeName;
            }
            else if ($scope.Segment == 'ValueLoss') {

                $scope.ValueLossProcurementCostingList[$scope.index].ResponsiblePersonId = data.SystemID;
                $scope.ValueLossProcurementCostingList[$scope.index].ResponsiblePerson = data.EmployeeName;
            }
            else if ($scope.Segment == 'Profit') {

                $scope.ProfitProcurementCostingList[$scope.index].ResponsiblePersonId = data.SystemID;
                $scope.ProfitProcurementCostingList[$scope.index].ResponsiblePerson = data.EmployeeName;
            }
        }
        angular.element(document.querySelector('#employeeNewPopUp')).modal('hide');

    };

    $scope.closeEmployeePopUp = function () {
        angular.element(document.querySelector('#employeeNewPopUp')).modal('hide');
    };
    // #region Master Order 
    $controller('baseMaterialAndArticleController', { $scope: $scope, $http: $http });
    $scope.materialType = 'BOM';
    //$scope.materialType = 'ProductDefinition';
    $scope.getMaterial = function (index, entryStage) {
        $scope.CostingStage = entryStage;
        $scope.itemIndex = index;
        $scope.getMaterialMasterbyTypePopUp();
    };
    $scope.selectMaterialByType = function (ob) {
        if ($scope.CostingStage == 'PRE') {
            $scope.DirectMaterialList[$scope.itemIndex].MaterialMasterId = ob.Id;
            $scope.DirectMaterialList[$scope.itemIndex].MaterialMasterName = ob.UserName;
            $scope.DirectMaterialList[$scope.itemIndex].ArticleId = null;
            $scope.DirectMaterialList[$scope.itemIndex].ArticleName = null;
            $scope.DirectMaterialList[$scope.itemIndex].InquiryItemId = null;
            $scope.DirectMaterialList[$scope.itemIndex].SampleItemId = null;
            $scope.DirectMaterialList[$scope.itemIndex].HasAttribute = ob.HasAttribute;
            $scope.mmChangeFlag = true;
            if ($scope.DirectMaterialList[$scope.itemIndex].HasAttribute) {
                $scope.getArticleSearchList(ob.Id);
            } else {
                $scope.closeMaterialMasterbyTypePopUp();
                return ShowResult('This material has no attribute', 'failure');
            }
        }
        else if ($scope.CostingStage == 'PROCUREMENT') {
            $scope.DirectProcurementCostingMaterialList[$scope.itemIndex].MaterialMasterId = ob.Id;
            $scope.DirectProcurementCostingMaterialList[$scope.itemIndex].MaterialMasterName = ob.UserName;
            $scope.DirectProcurementCostingMaterialList[$scope.itemIndex].ArticleId = null;
            $scope.DirectProcurementCostingMaterialList[$scope.itemIndex].ArticleName = null;
            $scope.DirectProcurementCostingMaterialList[$scope.itemIndex].InquiryItemId = null;
            $scope.DirectProcurementCostingMaterialList[$scope.itemIndex].SampleItemId = null;
            $scope.DirectProcurementCostingMaterialList[$scope.itemIndex].HasAttribute = ob.HasAttribute;
            $scope.mmChangeFlag = true;
            if ($scope.DirectProcurementCostingMaterialList[$scope.itemIndex].HasAttribute) {
                $scope.getArticleSearchList(ob.Id);
            } else {
                $scope.closeMaterialMasterbyTypePopUp();
                return ShowResult('This material has no attribute', 'failure');
            }
        }

        // getTaxCategoryList(ob.HSNCodeId);
        $scope.HSNCodeId = ob.HSNCodeId;
        $scope.closeMaterialMasterbyTypePopUp();
    };



    $scope.selectarticle = function (ob) {
        try {
            if ($scope.CostingStage == 'PRE') {

                $scope.DirectMaterialList[$scope.itemIndex].MaterialMasterId = ob.MaterialMasterId;
                $scope.DirectMaterialList[$scope.itemIndex].MaterialMasterName = ob.MaterialMasterName;
                $scope.DirectMaterialList[$scope.itemIndex].ArticleId = ob.Id;
                $scope.DirectMaterialList[$scope.itemIndex].ArticleName = ob.StandardName;
                angular.element(document.querySelector('#articleSearchPop')).modal('hide');
                $scope.itemIndex = -1;
                $scope.mmChangeFlag = true;
            }
            else if ($scope.CostingStage == 'PROCUREMENT') {

                $scope.DirectProcurementCostingMaterialList[$scope.itemIndex].MaterialMasterId = ob.MaterialMasterId;
                $scope.DirectProcurementCostingMaterialList[$scope.itemIndex].MaterialMasterName = ob.MaterialMasterName;
                $scope.DirectProcurementCostingMaterialList[$scope.itemIndex].ArticleId = ob.Id;
                $scope.DirectProcurementCostingMaterialList[$scope.itemIndex].ArticleName = ob.StandardName;
                angular.element(document.querySelector('#articleSearchPop')).modal('hide');
                $scope.itemIndex = -1;
                $scope.mmChangeFlag = true;
            }
        } catch (e) {
            ShowResult(e, '', 'articleSearchPop');
        }
    };

    $scope.getArticle = function (index, entryStage) {
        $scope.CostingStage = entryStage;
        $scope.itemIndex = index;
        if (!baseService.isUndefinedOrNull($scope.DirectMaterialList[$scope.itemIndex].MaterialMasterId) && !$scope.DirectMaterialList[$scope.itemIndex].HasAttribute)
            return ShowResult('This material has no attribute', 'failure');
        $scope.getArticleSearchList($scope.DirectMaterialList[$scope.itemIndex].MaterialMasterId);
    };

    $scope.totalCost = 0;
    $scope.SumCostingValue = function () {
        $scope.totalCost = 0;
        $scope.CostingSummaryDataNew = Object.assign({}, $scope.CostingSummaryDataMain);

        if ($scope.OrderCostingDetailList.length > 0) {
            for (let i = 0; i < $scope.OrderCostingDetailList.length; i++) {


                if (!isNaN($scope.OrderCostingDetailList[i].CostingValue)) {
                    $scope.totalCost += $scope.OrderCostingDetailList[i].CostingValue;
                }

                //calculation
                if ($scope.OrderCostingDetailList[i].CostingSegment == 'Profit') {
                    $scope.CostingSummaryDataNew.ProfitBuyerCosting += $scope.OrderCostingDetailList[i].BuyerTarget;
                    $scope.CostingSummaryDataNew.ProfitQuickCosting += $scope.OrderCostingDetailList[i].CostingValue;
                    $scope.CostingSummaryDataNew.ProfitOrderCosting += $scope.OrderCostingDetailList[i].TotalGrossAmount;
                    $scope.CostingSummaryDataNew.ProfitProcurementCosting += $scope.OrderCostingDetailList[i].TotalProcurementGrossAmount;

                }
                else {
                    $scope.CostingSummaryDataNew.BuyerTotal += $scope.OrderCostingDetailList[i].BuyerTarget;
                    $scope.CostingSummaryDataNew.QuickCostingValue += $scope.OrderCostingDetailList[i].CostingValue;
                    $scope.CostingSummaryDataNew.OrderCostingValue += $scope.OrderCostingDetailList[i].TotalGrossAmount;
                    $scope.CostingSummaryDataNew.ProcurementCostingValue += $scope.OrderCostingDetailList[i].TotalProcurementGrossAmount;

                }
            }
        }

        $scope.SumNetProfitOfSelling();
    }

    $scope.netProfit = 0;
    $scope.gainOrLos = 0;
    $scope.SumNetProfitOfSelling = function () {
        $scope.netProfit = 0;
        $scope.gainOrLos = 0;
        $scope.netProfit = $scope.ModelNew.TargetSellingPrice - $scope.totalCost;
        $scope.gainOrLos = $scope.netProfit;
        $scope.SumNetProfitOfGross();
    }
    $scope.NetProfitofGross = 0;
    $scope.SumNetProfitOfGross = function () {
        $scope.NetProfitofGross = $scope.GrossProfit - $scope.gainOrLos;
    }

    $scope.totalItemVale = 0;
    $scope.SumTotalItemsByComponetForPerSegment = function () {
        if ($scope.data.length > 0) {
            for (var i = 0; i < $scope.data.length; i++) {

            }
        }
    }

    $scope.totalComponentOfDirectMaterial = 0;
    $scope.totalComponentOfOperation = 0;
    $scope.totalComponentOfDirectProcess = 0;
    $scope.totalComponentOfSalesExpense = 0;
    $scope.totalComponentOfValueLoss = 0;


    $scope.backColor = 'red';
    $scope.fontColor = 'white';

    $scope.row = null;
    $scope.taskcolorchange = function (args) {
        //$scope.row = null;
        //$scope.row = args;
        //if (args.data.CostingValue < args.data.TotalGrossAmount) {
        //    $scope.row.cell.bgColor = "green";
        //}
        //else {
        //    $scope.row.cell.bgColor = "red";
        //}

    }
    // #endregion Costing Reports
    $scope.SelectedOrderCostingComponent = {};
    $scope.BackToOrderCostingComponent = function () {
        $scope.DirectMaterialList = [];
        $scope.OperationList = [];
        $scope.DirectProcessList = [];
        $scope.SalesExpenseList = [];
        $scope.ValueLossList = [];
        $scope.ProfitList = [];
        $scope.Segment = '';



        var elmnt = document.getElementById("costingMain");
        elmnt.scrollIntoView(false, { behavior: "smooth", block: "end", inline: "nearest" });
    }
    $scope.NavigateToOrderCosting = function (args) {
        if (angular.isUndefinedOrNull($scope.ModelNew.Id)) {
            return ShowResult('Please save the template first', 'failure');
        }


        $scope.DirectMaterialList = [];
        $scope.toatalItemGrossConsumption = 0;
        $scope.totalItemGrossAmount = 0;

        $scope.OperationList = [];
        $scope.totalOperationValue = 0;

        $scope.DirectProcessList = [];
        $scope.totalDirectProcessAmount = 0;

        $scope.SalesExpenseList = [];
        $scope.totalSalesExpenseAmount = 0;

        $scope.ValueLossList = [];
        $scope.totalValueLossAmount = 0;

        $scope.ProfitList = [];


        $scope.DirectProcurementCostingMaterialList = [];
        $scope.OperationProcurementCostingList = [];
        $scope.DirectProcessProcurementCostingList = [];
        $scope.SalesExpenseProcurementCostingList = [];
        $scope.ValueLossProcurementCostingList = [];
        $scope.ProfitProcurementCostingList = [];


        $scope.SelectedOrderCostingComponent = args;
        $scope.CostingComponentId = args.CostingComponentId;
        $scope.Segment = args.CostingSegment;
        if ($scope.Segment == 'DirectMaterial') {

            $scope.GetDirectCostingMaterialWithItemByComponentId();
            $scope.GetOrderBudgetData();
        }
        else if ($scope.Segment == 'Operation') {


            $scope.GetOperationWithItemByComponentId();
            $scope.GetOperationData();
        }
        else if ($scope.Segment == 'DirectProcess') {

            $scope.GetDirectProcessWithItemByComponentId();
            $scope.GetDirectProcessData();
        }
        else if ($scope.Segment == 'SalesExpense') {

            $scope.GetSalesExpenseWithItemByComponentId();
            $scope.GetOrderBudgetSalesExpenseData();
        }
        else if ($scope.Segment == 'ValueLoss') {

            $scope.GetValueLossWithItemByComponentId();
            $scope.GetOrderBudgetValueLossListData();
        }
        else if ($scope.Segment == 'Profit') {

            $scope.GetProfitWithItemByComponentId();
            $scope.GetOrderBudgetProfitData();
        }
        $scope.CalculateFinalCosting(null);
        $scope.CalculateFinalCostingProcurement(null);

    }

    function checkProcurementCostingValueChange(PreCostingList, ProcurementCostingList, RemarksFieldName, ValuesToCheckFields) {
        var PreData = ej.DataManager(PreCostingList);
        var Error = false;
        for (var i = 0; i < ProcurementCostingList.length; i++) {
            ProcurementCostingList[i].Error = false;
            if (angular.isUndefinedOrNull(ProcurementCostingList[i][RemarksFieldName])) {
                var MatchedItem = PreData.executeLocal(ej.Query().where("CostingItemId", "equal", ProcurementCostingList[i].CostingItemId));
                if (MatchedItem.length > 0) {
                    for (var F = 0; F < ValuesToCheckFields.length; F++) {

                        if (ProcurementCostingList[i][ValuesToCheckFields[F]] != MatchedItem[0][ValuesToCheckFields[F]]) {
                            ProcurementCostingList[i].Error = true;
                            Error = true;
                        }
                    }
                }
            }
        }

        if (Error) {
            throw 'Red marked rows do not match with pre-costing values. Please provide remarks for the value change'
        }
    }
    $scope.SaveCostingComponentItems = function (costingStage) {
        if (costingStage == 'PRE') {
            try {
                //var saveList = ej.DataManager($scope.OrderCostingDetailList).executeLocal(ej.Query().where("CostingComponentId", "equal", $scope.CostingComponentId));

                //if (saveList.length > 0) {

                //    if (saveList[0].PreCostingSavingsPercentage > 0) {
                //        var x = saveList[0].TotalGrossAmount / saveList[0].CostingValue ;
                //        x = x * 100;
                //        x = 100 - x;
                //        if (x < saveList[0].PreCostingSavingsPercentage) {
                //            throw 'Pre costing saving percentage is lower than the standard saving percentage :' + saveList[0].PreCostingSavingsPercentage + "%";
                //        }
                //    }
                //}

                if ($scope.Segment == 'DirectMaterial') {

                    $scope.SaveOrderPreCostingDirectMaterial();
                }
                else if ($scope.Segment == 'Operation') {


                    $scope.SaveOperation();
                }
                else if ($scope.Segment == 'DirectProcess') {

                    $scope.SaveDirectProcess();
                }
                else if ($scope.Segment == 'SalesExpense') {

                    $scope.SaveSalesExpense();
                }
                else if ($scope.Segment == 'ValueLoss') {

                    $scope.SaveValueLoss();
                }
                else if ($scope.Segment == 'Profit') {

                    $scope.SaveProfit();
                }
            } catch (e) {
                return ShowResult(e, 'failure');
            }
        }
        else if (costingStage == 'PROCUREMENT') {
            try {
                var saveList = ej.DataManager($scope.OrderCostingDetailList).executeLocal(ej.Query().where("CostingComponentId", "equal", $scope.CostingComponentId));
                if (saveList.length > 0) {
                    var x = saveList[0].TotalProcurementGrossAmount / saveList[0].TotalGrossAmount;
                    x = x * 100;
                    x = 100 - x;
                    if (saveList[0].ProcurementCostingSavingsPercentage > 0) {
                        if (x < saveList[0].ProcurementCostingSavingsPercentage) {
                            ShowResult('Procurement costing saving percentage is lower than the standard saving percentage :' + saveList[0].ProcurementCostingSavingsPercentage + "%");
                        }
                    }
                }
                //$scope.DirectMaterialList = [];
                //$scope.OperationList = [];
                //$scope.DirectProcessList = [];
                //$scope.SalesExpenseList = [];
                //$scope.ValueLossList = [];
                //$scope.ProfitList = [];


                //$scope.DirectProcurementCostingMaterialList = [];
                //$scope.OperationProcurementCostingList = [];
                //$scope.DirectProcessProcurementCostingList = [];
                //$scope.SalesExpenseProcurementCostingList = [];
                //$scope.ValueLossProcurementCostingList = [];
                //$scope.ProfitProcurementCostingList = [];
                if ($scope.Segment == 'DirectMaterial') {
                    checkProcurementCostingValueChange($scope.DirectMaterialList, $scope.DirectProcurementCostingMaterialList, 'Remarks', ['Consumption', 'ValueLoss', 'Rate'])
                    $scope.SaveOrderProcurementCostingDirectMaterial();
                }
                else if ($scope.Segment == 'Operation') {

                    checkProcurementCostingValueChange($scope.OperationList, $scope.OperationProcurementCostingList, 'Description', ['Value'])
                    $scope.SaveProcurementCostingOperation();
                }
                else if ($scope.Segment == 'DirectProcess') {
                    checkProcurementCostingValueChange($scope.DirectProcessList, $scope.DirectProcessProcurementCostingList, 'Description', ['ExecutionType', 'Value', 'Rate'])
                    $scope.SaveProcurementCostingDirectProcess();
                }
                else if ($scope.Segment == 'SalesExpense') {
                    checkProcurementCostingValueChange($scope.SalesExpenseList, $scope.SalesExpenseProcurementCostingList, 'Description', ['Type', 'Value'])
                    $scope.SaveProcurementCostingSalesExpense();
                }
                else if ($scope.Segment == 'ValueLoss') {
                    checkProcurementCostingValueChange($scope.ValueLossList, $scope.ValueLossProcurementCostingList, 'Description', ['Type', 'Value'])

                    $scope.SaveProcurementCostingValueLoss();
                }
                else if ($scope.Segment == 'Profit') {
                    checkProcurementCostingValueChange($scope.ProfitList, $scope.ProfitProcurementCostingList, 'Description', ['Type', 'Value'])

                    $scope.SaveProcurementCostingProfit();
                }
            } catch (e) {
                return ShowResult(e, 'failure');
            }
        }
    }

    $scope.TooltipModel = {};
    $scope.ShowToolTip = function (costingStage, SelectedData) {
        $scope.CostingStage = costingStage;
        $scope.TooltipModel = {};
        if (costingStage == 'PRE') {

            var ShowModel = {};

            if ($scope.Segment == 'DirectMaterial') {
                var saveList = ej.DataManager($scope.DirectProcurementCostingMaterialList).executeLocal(ej.Query().where("CostingItemId", "equal", SelectedData.CostingItemId));

                if (saveList.length > 0)
                    $scope.TooltipModel = Object.assign({}, saveList[0]);

                var purchaseDocument = ej.DataManager($scope.PurchaseGroupList).executeLocal(ej.Query().where("Id", "equal", SelectedData.PurchaseGroupId));
                if (purchaseDocument.length > 0)
                    $scope.TooltipModel.PurchaseGroupId = purchaseDocument[0].UserName;


                angular.element(document.querySelector("#itemDetailPopUp")).modal("show");
                return;
            }
            else if ($scope.Segment == 'Operation') {

                ShowModel = $scope.OperationProcurementCostingList;
            }
            else if ($scope.Segment == 'DirectProcess') {

                ShowModel = $scope.DirectProcessProcurementCostingList;
            }
            else if ($scope.Segment == 'SalesExpense') {

                ShowModel = $scope.SalesExpenseProcurementCostingList;
            }
            else if ($scope.Segment == 'ValueLoss') {

                ShowModel = $scope.ValueLossProcurementCostingList;
            }
            else if ($scope.Segment == 'Profit') {

                ShowModel = $scope.ProfitProcurementCostingList;
            }

            var saveList = ej.DataManager(ShowModel).executeLocal(ej.Query().where("CostingItemId", "equal", SelectedData.CostingItemId));
            if (saveList.length > 0)
                $scope.TooltipModel = Object.assign({}, saveList[0]);

        }
        else if (costingStage == 'PROCUREMENT') {

            var ShowModel = {};

            if ($scope.Segment == 'DirectMaterial') {
                var saveList = ej.DataManager($scope.DirectMaterialList).executeLocal(ej.Query().where("CostingItemId", "equal", SelectedData.CostingItemId));

                if (saveList.length > 0)
                    $scope.TooltipModel = Object.assign({}, saveList[0]);

                var purchaseDocument = ej.DataManager($scope.PurchaseGroupList).executeLocal(ej.Query().where("Id", "equal", SelectedData.PurchaseGroupId));
                if (purchaseDocument.length > 0)
                    $scope.TooltipModel.PurchaseGroupId = purchaseDocument[0].UserName;


                angular.element(document.querySelector("#itemDetailPopUp")).modal("show");
                return;
            }
            else if ($scope.Segment == 'Operation') {

                ShowModel = $scope.OperationList;
            }
            else if ($scope.Segment == 'DirectProcess') {

                ShowModel = $scope.DirectProcessList;
            }
            else if ($scope.Segment == 'SalesExpense') {

                ShowModel = $scope.SalesExpenseList;
            }
            else if ($scope.Segment == 'ValueLoss') {

                ShowModel = $scope.ValueLossList;
            }
            else if ($scope.Segment == 'Profit') {

                ShowModel = $scope.ProfitList;
            }

            var saveList = ej.DataManager(ShowModel).executeLocal(ej.Query().where("CostingItemId", "equal", SelectedData.CostingItemId));
            if (saveList.length > 0)
                $scope.TooltipModel = Object.assign({}, saveList[0]);

        }




        angular.element(document.querySelector("#itemDetailPopUp")).modal("show");
    }

    $scope.UploadFile = function (data, costingStage) {


    }

    $scope.SelectedItemData = {};
    $scope.UploadTableName = '';
    $scope.uploadUrl = $scope.path + "UploadAttachment/";
    $scope.ShowUploadBox = function (data, costingStage, DBTableName) {
        $scope.SelectedItemData = data;
        $scope.CostingStage = costingStage;
        $scope.UploadTableName = DBTableName;

        var _title = "Pre Costing";
        if ($scope.CostingStage == 'PROCUREMENT')
            _title = "Procurement Costing";

        $("#UploadBox").ejDialog("setTitle", "Upload File (" + _title + ")");
        var eDialog = $("#UploadBox").data("ejDialog");
        eDialog.open();

    }
    $scope.confirmFileDelete = function () {
        angular.element(document.querySelector("#confirmFileDelete")).modal("show");
    }
    $scope.onBeginUpload = function (args) {
        try {
            var _data = [{ Id: $scope.SelectedItemData.Id, CostingStage: $scope.CostingStage, TableName: $scope.UploadTableName }];

            args.data = JSON.stringify(_data);

        } catch (e) {

            args.cancel = true;
            ShowResult(e, 'Error');
        }

    }
    $scope.DeleteFile = function () {
        try {
            $http({
                method: 'POST', url: $scope.path + 'DeleteFile', dataType: 'JSON',
                data: { Id: $scope.SelectedItemData.Id, TableName: $scope.UploadTableName }

            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult('error', 'failure');
                }
                else {
                    $scope.SelectedItemData.FileName = '';
                    $scope.SelectedItemData.FileOriginalName = '';
                }
            }, function errorCallback(response) {
                ShowResult('Failed', 'failure');
            });
        } catch (e) {

            args.cancel = true;
            ShowResult(e, 'Error');
        }

    }

    $scope.errorUpload = function (e) {
        ShowResult(e.error, 'failure');

        //    ShowResult("The selected file size is too large. Please select a file less than " + Math.round(e.model.fileSize / (1024 * 1024)) + "MB", 'failure');
    }

    $scope.getFileList = function () {

        $http({
            method: 'POST', url: $scope.path + 'GetFileInfo', dataType: 'JSON',
            data: { Id: $scope.SelectedItemData.Id, TableName: $scope.UploadTableName }

        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult('error', 'failure');
            }
            else {
                $scope.SelectedItemData.FileName = response.data[0].FileName;
                $scope.SelectedItemData.FileOriginalName = response.data[0].FileOriginalName;
            }
        }, function errorCallback(response) {
            ShowResult('Failed', 'failure');
        });
    }

    //#region Update work by saad for preCosting
    $scope.SelectedDirectMaterial = [];
    $scope.getValue = function (args) {
        $scope.SelectedDirectMaterial = args;
        $scope.UpdatePop();
    }
    $scope.ShowDiv = false;
    $scope.UpdatePop = function () {
        try {

            $scope.ShowDiv = true;
            var eDialog = $("#UpdatePOPup").data("ejDialog");
            eDialog.open();
            $scope.GetPreCostingDetail($scope.SelectedDirectMaterial.Id);
            $scope.GetDataFormItem();

        } catch (e) {
            ShowResult(e, "failure");
        }

    };

    $scope.GetPreCostingDetail = function (PreCostingDirectMaterialId) {

        $http({
            method: 'POST',
            url: $scope.path + "GetHeightData",
            data: { PreCostMaterialId: PreCostingDirectMaterialId, ParameterName: 'Height' },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.Height = response.data;
        });

        $http({
            method: 'POST',
            url: $scope.path + "GetHeightData",
            data: { PreCostMaterialId: PreCostingDirectMaterialId, ParameterName: 'Width' },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.Width = response.data;
        });
    }

    $scope.Height = [];
    $scope.Width = [];
    $scope.Height.push(Object.assign({}, $scope.HeightDetail));
    $scope.Width.push(Object.assign({}, $scope.WidthDetail));
    $scope.HeightDetail = {
        Id: null,
        AreaType: '',
        ParameterName: 'Height',
        Parameter: null,
        Actual: 0,
        Allowance: 0,
        WithAllowance: 0,
        NoOfParameter: 0,
        tempId: null,
        Total: 0
    }
    $scope.WidthDetail = {
        Id: null,
        AreaType: '',
        ParameterName: 'Height',
        Parameter: null,
        Actual: 0,
        Allowance: 0,
        WithAllowance: 0,
        NoOfParameter: 0,
        tempId: null,
        Total: 0
    }

    $scope.Calculation = function ($data) {
        $data.WithAllowance = $data.Actual + $data.Allowance;
        $data.Total = $data.WithAllowance * $data.NoOfParameter;
    }

    $scope.ItemList = [];
    $scope.GetDataFormItem = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetDataFromItemCon",
            data: { ProductId: $scope.ModelNew.ProductMasterId, MaterialId: $scope.SelectedDirectMaterial.CostingItemId },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ItemList = response.data;
        });
    }



    $scope.confirmdelete = false;
    $scope.Confirm = function () {
        try {
            if ($scope.ItemConsumption == null || $scope.ItemConsumption == "") {
                throw "Select Item Consumption first..";
            }
            var eDialog = $("#dialogAPI").data("ejDialog");
            eDialog.open();
            $("#dialogAPI_wrapper").css({ 'position': 'fixed' }).css({ 'top': '200px' });
        } catch (e) {
            ShowResult(e, "failure");
        }

    };
    $scope.ConfirmClose = function () {
        var eDialog = $("#dialogAPI").data("ejDialog");
        eDialog.close();
    };

    $scope.ApplyItemConsumption = function () {
        try {
            if ($scope.ItemConsumption == null) {
                throw "Select Item Consumption first..";
            }
            $http({
                method: 'POST',
                url: $scope.path + "SaveNewItemConsumptionData",
                data: { PreCostingDirectMaterialId: $scope.SelectedDirectMaterial.Id, ItemConsumtionId: $scope.ItemConsumption, CostingMasterTemplateId: $scope.SelectedDirectMaterial.OrderCostingMasterTemplateId },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.SelectedDirectMaterial.Consumption = parseFloat(response.data.Consumption.toFixed(3));
                    $scope.GetPreCostingDetail($scope.SelectedDirectMaterial.Id);
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        } catch (e) {
            ShowResult(e, "failure");
        }

    }

    $scope.SaveUpdate = function () {
        try {
            $scope.SaveChildDataList = [];
            for (var i = 0; i < $scope.Height.length; i++) {
                if ($scope.Height[i].Parameter == null && $scope.Height[i].Actual == null) {
                    $scope.Height.splice(i, 1);
                }

            }
            if ($scope.Height.length == 0) {
                $scope.Height.push(Object.assign({}, $scope.HeightDetail));
                throw "Parameter 1 is required..";
            }
            else {
                for (var i = 0; i < $scope.Height.length; i++) {
                    $scope.SaveChildDataList.push($scope.Height[i]);
                }
            }

            for (var i = 0; i < $scope.Width.length; i++) {
                if ($scope.Width[i].Parameter == null && $scope.Width[i].Actual == null) {
                    $scope.Width.splice(i, 1);
                }
            }
            for (var i = 0; i < $scope.Height.length; i++) {
                if ($scope.Height[i].AreaType != 'Circle') {
                    if ($scope.Width.length == 0) {
                        $scope.Width.push(Object.assign({}, $scope.WidthDetail));
                        throw "Parameter 2 is required..";
                    }
                    else {
                        for (var i = 0; i < $scope.Width.length; i++) {
                            $scope.SaveChildDataList.push($scope.Width[i]);
                        }
                    }
                }
            }

            if ($scope.SaveChildDataList.length == 0) {
                throw "Insert Parameter Value";
            }
            $http({
                method: 'POST',
                url: $scope.path + "SaveUpdate",
                data: { 'PreCostingDirectMaterialId': $scope.SelectedDirectMaterial.Id, 'ChildData': $scope.SaveChildDataList },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.SelectedDirectMaterial.Consumption = parseFloat(response.data.Consumption.toFixed(3));
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        } catch (e) {
            ShowResult(e, "failure");
        }
    }

    //#endregion


    //#region Update work by saad for PROCUREMENT
    $scope.SelectedDirectMaterialP = [];
    $scope.getValueP = function (args) {
        $scope.SelectedDirectMaterialP = args;
        $scope.UpdatePopP();
    }
    $scope.ShowDiv = false;
    $scope.UpdatePopP = function () {
        try {

            $scope.ShowDiv = true;
            var eDialog = $("#UpdatePOPupP").data("ejDialog");
            eDialog.open();
            $scope.GetPreCostingDetailP($scope.SelectedDirectMaterialP.Id);
            $scope.GetDataFormItemPro();

        } catch (e) {
            ShowResult(e, "failure");
        }

    };
    $scope.HeightP = [];
    $scope.WidthP = [];
    $scope.GetPreCostingDetailP = function (PreCostingDirectMaterialId) {

        $http({
            method: 'POST',
            url: $scope.path + "GetHeightDataPro",
            data: { PreCostMaterialId: PreCostingDirectMaterialId, ParameterName: 'Height' },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.HeightP = response.data;
        });

        $http({
            method: 'POST',
            url: $scope.path + "GetHeightDataPro",
            data: { PreCostMaterialId: PreCostingDirectMaterialId, ParameterName: 'Width' },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.WidthP = response.data;
        });
    }


    $scope.HeightP.push(Object.assign({}, $scope.HeightDetailP));
    $scope.WidthP.push(Object.assign({}, $scope.WidthDetailP));
    $scope.HeightDetailP = {
        Id: null,
        AreaType: '',
        ParameterName: 'Height',
        Parameter: null,
        Actual: 0,
        Allowance: 0,
        WithAllowance: 0,
        NoOfParameter: 0,
        tempId: null,
        Total: 0
    }
    $scope.WidthDetailP = {
        Id: null,
        AreaType: '',
        ParameterName: 'Height',
        Parameter: null,
        Actual: 0,
        Allowance: 0,
        WithAllowance: 0,
        NoOfParameter: 0,
        tempId: null,
        Total: 0
    }

    $scope.CalculationP = function ($data) {
        $data.WithAllowance = $data.Actual + $data.Allowance;
        $data.Total = $data.WithAllowance * $data.NoOfParameter;
    }

    $scope.ItemListPro = [];
    $scope.GetDataFormItemPro = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetDataFromItemCon",
            data: { ProductId: $scope.ModelNew.ProductMasterId, MaterialId: $scope.SelectedDirectMaterialP.CostingItemId },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ItemListPro = response.data;
        });
    }

    $scope.confirmdelete = false;
    $scope.ConfirmP = function () {
        try {
            if ($scope.ItemConsumptionP == null || $scope.ItemConsumptionP == "") {
                throw "Select Item Consumption first..";
            }
            var eDialog = $("#dialogAPIP").data("ejDialog");
            eDialog.open();
            $("#dialogAPI_wrapper").css({ 'position': 'fixed' }).css({ 'top': '200px' });
        } catch (e) {
            ShowResult(e, "failure");
        }

    };
    $scope.ConfirmCloseP = function () {
        var eDialog = $("#dialogAPIP").data("ejDialog");
        eDialog.close();
    };

    $scope.ApplyItemConsumptionP = function () {
        try {
            if ($scope.ItemConsumptionP == null) {
                throw "Select Item Consumption first..";
            }
            $http({
                method: 'POST',
                url: $scope.path + "SaveNewItemConsumptionDataOrderProcurement",
                data: { PreCostingDirectMaterialId: $scope.SelectedDirectMaterialP.Id, ItemConsumtionId: $scope.ItemConsumptionP, CostingMasterTemplateId: $scope.SelectedDirectMaterialP.OrderCostingMasterTemplateId },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.SelectedDirectMaterialP.Consumption = parseFloat(response.data.Consumption.toFixed(3));
                    $scope.GetPreCostingDetailP($scope.SelectedDirectMaterialP.Id);
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        } catch (e) {
            ShowResult(e, "failure");
        }

    }

    $scope.SaveUpdateP = function () {
        try {
            $scope.SaveChildDataListP = [];
            for (var i = 0; i < $scope.HeightP.length; i++) {
                if ($scope.HeightP[i].Parameter == null && $scope.HeightP[i].Actual == null) {
                    $scope.HeightP.splice(i, 1);
                }

            }
            if ($scope.HeightP.length == 0) {
                $scope.HeightP.push(Object.assign({}, $scope.HeightDetailP));
                throw "Parameter 1 is required..";
            }
            else {
                for (var i = 0; i < $scope.HeightP.length; i++) {
                    $scope.SaveChildDataListP.push($scope.HeightP[i]);
                }
            }

            for (var i = 0; i < $scope.WidthP.length; i++) {
                if ($scope.WidthP[i].Parameter == null && $scope.WidthP[i].Actual == null) {
                    $scope.WidthP.splice(i, 1);
                }
            }
            for (var i = 0; i < $scope.HeightP.length; i++) {
                if ($scope.HeightP[i].AreaType != 'Circle') {
                    if ($scope.WidthP.length == 0) {
                        $scope.WidthP.push(Object.assign({}, $scope.WidthDetailP));
                        throw "Parameter 2 is required..";
                    }
                    else {
                        for (var i = 0; i < $scope.WidthP.length; i++) {
                            $scope.SaveChildDataListP.push($scope.WidthP[i]);
                        }
                    }
                }
            }

            if ($scope.SaveChildDataListP.length == 0) {
                throw "Insert Parameter Value";
            }
            $http({
                method: 'POST',
                url: $scope.path + "SaveUpdateOrderProcurement",
                data: { 'PreCostingDirectMaterialId': $scope.SelectedDirectMaterialP.Id, 'ChildData': $scope.SaveChildDataListP },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.SelectedDirectMaterialP.Consumption = parseFloat(response.data.Consumption.toFixed(3));
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        } catch (e) {
            ShowResult(e, "failure");
        }
    }
    $scope.POCriteriaList = [];
    cboService.getEnumCbo("enum/GetPOInvoiceCriteriaEnumCbo", function (result) {
        $scope.POCriteriaList = result;
    });
    //#endregion

    //#region POPUp For OrderPreCosting-Sub Material

    $scope.SubMaterialList = [];
    $scope.SelectedLine = {};
    $scope.GetSubMaterial = function (item) {
        //try {
        $scope.SelectedLine = {};
        $scope.SelectedLine = item;

        $scope.getSubMaterialData($scope.SelectedLine.Id);
        angular.element(document.querySelector("#SubMaterialPOPupOC")).modal("show");

    }
    $scope.showCostingItemListWithOperationPopUpInSubMaterial = function (costingStage) {
        try {
            if (angular.isUndefinedOrNull($scope.OrderCostingMasterTemplateId))
                throw 'Please save the costing master first';

            $scope.CostingStage = costingStage;

            if (costingStage == 'PROCUREMENT') {
                var eDialog = $("#GeneralSubPro").data("ejDialog");
                eDialog.open();
            }
            else {
                var eDialog = $("#GeneralSub").data("ejDialog");
                eDialog.open();
            }


            $scope.AddNewCostingItemPopUp();

        } catch (e) {
            ShowResult(e, "failure");
        }

    };
    $scope.NewCostingItemList = [];
    $scope.AddNewCostingItemPopUp = function () {
        try {
            if (angular.isUndefinedOrNull($scope.OrderCostingMasterTemplateId))
                throw 'Please save the costing master first';
            $scope.$broadcast('show-errors-check-validity');
            $http({
                method: 'POST',
                url: $scope.path + "GetCostingItemForSubMaterial",
                data: { CostingStage: $scope.CostingStage, OrderCostingMasterTemplateId: $scope.OrderCostingMasterTemplateId, costingComponentId: $scope.CostingComponentId, Segment: $scope.Segment },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                $scope.NewCostingItemList = response.data;
            });
        } catch (e) {
            ShowResult(e, "failure");
        }
    }
    $scope.CloseSubMaterial = function () {
        angular.element(document.querySelector("#SubMaterialPOPupOC")).modal("hide");
    }
    $scope.hideSubMaterialPart = function () {
        var eDialog = $("#GeneralSub").data("ejDialog");
        eDialog.close();
    }
    $scope.SaveSubMaterialPart = function () {
        try {
            //$scope.SelectedLine;
            var selectedList = [];
            for (var i = 0; i < $scope.NewCostingItemList.length; i++) {
                if ($scope.NewCostingItemList[i].Selected) {
                    selectedList.push($scope.NewCostingItemList[i]);
                }
            }
            if (angular.isUndefinedOrNull($scope.OrderCostingMasterTemplateId))
                throw 'Please save the costing master first';
            $http({
                method: 'POST',
                url: $scope.path + "SaveSubMaterial",
                data: { itemList: selectedList, 'PreCDMaterial': $scope.SelectedLine },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.getSubMaterialData($scope.SelectedLine.Id);
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }

            });
        } catch (e) {
            ShowResult(e, "failure");
        }
    }
    $scope.getSubMaterialData = function (OrderPreCostingDirectMaterialId) {
        try {
            $http({
                method: 'POST',
                url: $scope.path + "GetSubMaterialData",
                data: { MasterId: OrderPreCostingDirectMaterialId },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                $scope.SubMaterialList = response.data.data;
            });
        } catch (e) {
            ShowResult(e, 'info');
        }
    }
    $scope.SaveSubMaterial = function () {
        try {
            $http({
                method: 'POST',
                url: $scope.path + "UpdatePreCostingChild",
                data: { subMaterilaList: $scope.SubMaterialList, MasterId: $scope.SelectedLine.Id },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.getSubMaterialData($scope.SelectedLine.Id);
                }
            });
        } catch (e) {
            ShowResult(e, 'info');
        }
    };
    $scope.DeleteSubMaterialId = null;
    $scope.DeleteSubMaterial = function (obj) {
        $scope.DeleteSubMaterialId = obj;
        angular.element(document.querySelector('#confirmDelete')).modal('show');
    }

    $scope.DeleteSubMaterials = function () {
        try {
            $http({
                method: 'POST',
                url: $scope.path + "DeleteSubMaterial",
                data: { SubMaterialId: $scope.DeleteSubMaterialId },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.getSubMaterialData($scope.SelectedLine.Id);
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        } catch (e) {
            ShowResult(e, "failure");
        }
    };
    $scope.CalculationSubMaterial = function (data, index) {
        data.GrossConsumption = parseFloat(data.Consumption) / (100 - (parseFloat(data.ValueLoss) / 100)) / 100;
        data.GrossAmount = parseFloat(data.Rate) * parseFloat(data.GrossConsumption);
        $scope.SubMaterialList[index].GrossConsumption = parseFloat(data.GrossConsumption.toFixed(4));
        $scope.SubMaterialList[index].GrossAmount = parseFloat(data.GrossAmount.toFixed(4));
    };
    //#endregion

    //#region POPUp For OrderProcurementCosting-Sub Material

    $scope.SubMaterialListPro = [];
    $scope.SelectedLinePro = {};
    $scope.GetSubMaterialPro = function (item) {
        //try {
        $scope.SelectedLinePro = {};
        $scope.SelectedLinePro = item;

        $scope.getSubMaterialDataPro($scope.SelectedLinePro.Id);
        angular.element(document.querySelector("#SubMaterialPOPupOCPRo")).modal("show");

    }
    $scope.CloseSubMaterialPro = function () {
        angular.element(document.querySelector("#SubMaterialPOPupOCPRo")).modal("hide");
    }
    $scope.getSubMaterialDataPro = function (OrderProcurementCostingDirectMaterialId) {
        try {
            $http({
                method: 'POST',
                url: $scope.path + "GetSubMaterialDataPro",
                data: { MasterId: OrderProcurementCostingDirectMaterialId },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                $scope.SubMaterialListPro = response.data.data;
            });
        } catch (e) {
            ShowResult(e, 'info');
        }
    }
    $scope.hideSubMaterialPartPro = function () {
        var eDialog = $("#GeneralSubPro").data("ejDialog");
        eDialog.close();
    }
    $scope.SaveSubMaterialPartPro = function () {
        try {
            //$scope.SelectedLine;
            var selectedList = [];
            for (var i = 0; i < $scope.NewCostingItemList.length; i++) {
                if ($scope.NewCostingItemList[i].Selected) {
                    selectedList.push($scope.NewCostingItemList[i]);
                }
            }
            if (angular.isUndefinedOrNull($scope.OrderCostingMasterTemplateId))
                throw 'Please save the costing master first';
            $http({
                method: 'POST',
                url: $scope.path + "SaveSubMaterialPro",
                data: { itemList: selectedList, 'PreCDMaterial': $scope.SelectedLinePro },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.getSubMaterialDataPro($scope.SelectedLinePro.Id);
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }

            });
        } catch (e) {
            ShowResult(e, "failure");
        }
    }
    $scope.CalculationSubMaterialPro = function (data, index) {
        data.GrossConsumption = parseFloat(data.Consumption) / ((100 - (parseFloat(data.ValueLoss)) / 100));
        data.GrossAmount = parseFloat(data.Rate) * parseFloat(data.GrossConsumption);
        $scope.SubMaterialListPro[index].GrossConsumption = parseFloat(data.GrossConsumption.toFixed(4));
        $scope.SubMaterialListPro[index].GrossAmount = parseFloat(data.GrossAmount.toFixed(4));
    };
    $scope.SaveSubMaterialPro = function () {
        try {
            $http({
                method: 'POST',
                url: $scope.path + "UpdatePreCostingChildPro",
                data: { subMaterilaList: $scope.SubMaterialListPro, MasterId: $scope.SelectedLinePro.Id },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.getSubMaterialData($scope.SelectedLinePro.Id);
                }
            });
        } catch (e) {
            ShowResult(e, 'info');
        }
    };
    $scope.DeleteSubMaterialIdPro = null;
    $scope.DeleteSubMaterialPro = function (obj) {
        $scope.DeleteSubMaterialIdPro = obj;
        angular.element(document.querySelector('#confirmDeletePro')).modal('show');
    }

    $scope.DeleteSubMaterialsPro = function () {
        try {
            $http({
                method: 'POST',
                url: $scope.path + "DeleteSubMaterialPro",
                data: { SubMaterialId: $scope.DeleteSubMaterialIdPro },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.getSubMaterialDataPro($scope.SelectedLinePro.Id);
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.OrderBudgetReport = function (x) {
        try {
            var data = x;
            var file_src = 'Costings/OrderCosting/OrderBudgetReport?OrderCostingId=' + data.Id + '&orderBudget=' + 1 + '&MOIId=' + data.MOIId;
            $rootScope.report(file_src);

        } catch (e) {
        }
    }

    $scope.PreOrderCostingReport = function (x) {
        try {
            var data = x;
            var file_src = 'Costings/OrderCosting/GetOrderCostingReport?OrderCostingId=' + data.Id + '&preCosting=' + 1 + '&MOIId=' + data.MOIId;
            $rootScope.report(file_src);

        } catch (e) {
        }
    }
    $scope.ProOrderCostingReport = function (x) {
        try {
            var data = x;
            var file_src = 'Costings/OrderCosting/GetOrderCostingReport?OrderCostingId=' + data.Id + '&procurementCosting=' + 1 + '&MOIId=' + data.MOIId;
            $rootScope.report(file_src);

        } catch (e) {
        }
    }

    $scope.removePreCosting = function (data) {
        try {
            $scope.OPCDMId = data.Id;
            $scope.message_confirmation = "Are you sure want to permanent delete?";
            angular.element(document.querySelector('#confirmPreCostingPopUp')).modal('show');
        }
        catch (e) {
            ShowResult(e, 'Error');
        }
    };

    $scope.DeletePreCosting = function () {
        $http.post('Costings/OrderCosting/DeletePreCosting?OrderPreCostingDirectMaterialId=' + $scope.OPCDMId + '&cs=' + $scope.ModelNew.CostingStage)
            .then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    //$scope.SaveCostingComponentItems('PRE');
                    $scope.GetDirectCostingMaterialWithItemByComponentId();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
    };

    $scope.removeProcurementCosting = function (data) {
        try {
            $scope.OPRCDMId = data.Id;
            $scope.message_confirmation = "Are you sure want to permanent delete?";
            angular.element(document.querySelector('#confirmProcurementCostingPopUp')).modal('show');
        }
        catch (e) {
            ShowResult(e, 'Error');
        }
    };

    $scope.DeleteProcurementCosting = function () {
        $http.post('Costings/OrderCosting/DeleteProcurementCosting?OrderProcurementCostingDirectMaterialId=' + $scope.OPRCDMId)
            .then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    //$scope.SaveCostingComponentItems('PRE');
                    $scope.GetDirectCostingMaterialWithItemByComponentId();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
    };

    $scope.removePreCostingDirectProcess = function (data) {
        try {
            $scope.DPCPCId = data.Id;
            $scope.message_confirmation = "Are you sure want to permanent delete?";
            angular.element(document.querySelector('#confirmPreCostingDirectProcessPopUp')).modal('show');
        }
        catch (e) {
            ShowResult(e, 'Error');
        }
    };

    $scope.DeleteOrderPreCostingDirectProces = function () {
        $http.post('Costings/OrderCosting/DeleteOrderPreCostingDirectProces?OrderPreCostingDirectProcessId=' + $scope.DPCPCId + '&cs=' + $scope.ModelNew.CostingStage)
            .then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    //$scope.SaveCostingComponentItems('PRE');
                    $scope.GetDirectProcessWithItemByComponentId();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
    };

    $scope.removeDirectProcessProcurementCosting = function (data) {
        try {
            $scope.DPPCId = data.Id;
            $scope.message_confirmation = "Are you sure want to permanent delete?";
            angular.element(document.querySelector('#confirmDirectProcessProcurementCostingPopUp')).modal('show');
        }
        catch (e) {
            ShowResult(e, 'Error');
        }
    };

    $scope.DeleteDirectProcessProcurementCosting = function () {
        $http.post('Costings/OrderCosting/DeleteDirectProcessProcurementCosting?DirectProcessProcurementCostingId=' + $scope.DPPCId)
            .then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    //$scope.SaveCostingComponentItems('PRE');
                    $scope.GetDirectProcessWithItemByComponentId();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
    };

    $scope.removeOperationListPreCosting = function (data) {
        try {
            $scope.OLPC = data.Id;
            $scope.message_confirmation = "Are you sure want to permanent delete?";
            angular.element(document.querySelector('#confirmOperationListPreCostingPopUp')).modal('show');
        }
        catch (e) {
            ShowResult(e, 'Error');
        }
    };

    $scope.DeleteOperationListPreCosting = function () {
        $http.post('Costings/OrderCosting/DeleteOperationListPreCosting?OperationListPreCostingId=' + $scope.OLPC + '&cs=' + $scope.ModelNew.CostingStage)
            .then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.GetOperationWithItemByComponentId();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
    };

    $scope.removeOperationListProcurementCosting = function (data) {
        try {
            $scope.OLPRC = data.Id;
            $scope.message_confirmation = "Are you sure want to permanent delete?";
            angular.element(document.querySelector('#confirmOperationListProcurementCostingPopUp')).modal('show');
        }
        catch (e) {
            ShowResult(e, 'Error');
        }
    };

    $scope.DeleteOperationListProcurementCosting = function () {
        $http.post('Costings/OrderCosting/DeleteOperationListProcurementCosting?OperationListProcurementCostingId=' + $scope.OLPRC)
            .then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.GetOperationWithItemByComponentId();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
    };

    $scope.removeValueLossPreCosting = function (data) {
        try {
            $scope.VLPC = data.Id;
            $scope.message_confirmation = "Are you sure want to permanent delete?";
            angular.element(document.querySelector('#confirmValueLossPreCostingPopUp')).modal('show');
        }
        catch (e) {
            ShowResult(e, 'Error');
        }
    };

    $scope.DeleteValueLossPreCosting = function () {
        $http.post('Costings/OrderCosting/DeleteValueLossPreCosting?ValueLossPreCostingId=' + $scope.VLPC + '&cs=' + $scope.ModelNew.CostingStage)
            .then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.GetValueLossWithItemByComponentId();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
    };

    $scope.removeValueLossProcurementCosting = function (data) {
        try {
            $scope.VLPRC = data.Id;
            $scope.message_confirmation = "Are you sure want to permanent delete?";
            angular.element(document.querySelector('#confirmValueLossProcurementCostingPopUp')).modal('show');
        }
        catch (e) {
            ShowResult(e, 'Error');
        }
    };

    $scope.DeleteValueLossProcurementCosting = function () {
        $http.post('Costings/OrderCosting/DeleteValueLossProcurementCosting?ValueLossProcurementCostingId=' + $scope.VLPRC)
            .then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.GetValueLossWithItemByComponentId();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
    };

    $scope.removeOrderPreCostingProfit = function (data) {
        try {
            $scope.OPCP = data.Id;
            $scope.message_confirmation = "Are you sure want to permanent delete?";
            angular.element(document.querySelector('#confirmOrderPreCostingProfitPopUp')).modal('show');
        }
        catch (e) {
            ShowResult(e, 'Error');
        }
    };

    $scope.DeleteOrderPreCostingProfit = function () {
        $http.post('Costings/OrderCosting/DeleteOrderPreCostingProfit?OrderPreCostingProfitId=' + $scope.OPCP + '&cs=' + $scope.ModelNew.CostingStage)
            .then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.GetProfitWithItemByComponentId();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
    };

    $scope.removeOrderProcurementCostingProfit = function (data) {
        try {
            $scope.OPCP = data.Id;
            $scope.message_confirmation = "Are you sure want to permanent delete?";
            angular.element(document.querySelector('#confirmOrderProcurementCostingProfitPopUp')).modal('show');
        }
        catch (e) {
            ShowResult(e, 'Error');
        }
    };

    $scope.DeleteOrderProcurementCostingProfit = function () {
        $http.post('Costings/OrderCosting/DeleteOrderProcurementCostingProfit?OrderProcurementCostingProfitId=' + $scope.OPCP)
            .then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.GetProfitWithItemByComponentId();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
    };

    $scope.removeOrderPreCostingSalesExpense = function (data) {
        try {
            $scope.OPCSE = data.Id;
            $scope.message_confirmation = "Are you sure want to permanent delete?";
            angular.element(document.querySelector('#confirmOrderPreCostingSalesExpensePopUp')).modal('show');
        }
        catch (e) {
            ShowResult(e, 'Error');
        }
    };

    $scope.DeleteOrderPreCostingSalesExpense = function () {
        $http.post('Costings/OrderCosting/DeleteOrderPreCostingSalesExpense?OrderPreCostingSalesExpenseId=' + $scope.OPCSE + '&cs=' + $scope.ModelNew.CostingStage)
            .then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.GetSalesExpenseWithItemByComponentId();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
    };

    $scope.removeOrderProcurementCostingSalesExpense = function (data) {
        try {
            $scope.OPRCSE = data.Id;
            $scope.message_confirmation = "Are you sure want to permanent delete?";
            angular.element(document.querySelector('#confirmOrderProcurementCostingSalesExpensePopUp')).modal('show');
        }
        catch (e) {
            ShowResult(e, 'Error');
        }
    };

    $scope.DeleteOrderProcurementCostingSalesExpense = function () {
        $http.post('Costings/OrderCosting/DeleteOrderProcurementCostingSalesExpense?OrderProcurementCostingSalesExpenseId=' + $scope.OPRCSE)
            .then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.GetSalesExpenseWithItemByComponentId();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
    };

    $scope.OrderBudgetDirectMaterialList = [];
    $scope.GetOrderBudgetData = function () {
        $scope.OrderBudgetDirectMaterialList = [];
        $http({
            method: 'GET',
            url: 'Costings/OrderCosting/GetOrderDirectMaterialBudget?OrderCostingMasterTemplateId=' + $scope.ModelNew.Id,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.OrderBudgetDirectMaterialList = response.data.Pre;
        });
    }

    $scope.TotalOrderCostTotal = [{
        title: "Total", summaryColumns: [
            {
                summaryType: ej.Grid.SummaryType.Sum, displayColumn: "TotalOrderCost", dataMember: "TotalOrderCost", format: "{0:N2}"
            }],
        showCaptionSummary: true
    }];

    $scope.OrderBudgetDirectProcessList = [];
    $scope.GetDirectProcessData = function () {
        $scope.OrderBudgetDirectProcessList = [];
        $http({
            method: 'GET',
            url: 'Costings/OrderCosting/GetOrderBudgetDirectProcess?OrderCostingMasterTemplateId=' + $scope.ModelNew.Id,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.OrderBudgetDirectProcessList = response.data.Pre;
        });
    }

    $scope.TotalDirectProcess = [{
        title: "Total", summaryColumns: [
            {
                summaryType: ej.Grid.SummaryType.Sum, displayColumn: "TotalOrderCost", dataMember: "TotalOrderCost", format: "{0:N2}"
            }],
        showCaptionSummary: true
    }];


    $scope.OrderBudgetOperationList = [];
    $scope.GetOperationData = function () {
        $scope.OrderBudgetOperationList = [];
        $http({
            method: 'GET',
            url: 'Costings/OrderCosting/GetOrderBudgetOperation?OrderCostingMasterTemplateId=' + $scope.ModelNew.Id + '&costingComponentId=' + $scope.CostingComponentId,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.OrderBudgetOperationList = response.data.Pre;
        });
    }

    $scope.OrderBudgetValueLossList = [];
    $scope.GetOrderBudgetValueLossListData = function () {
        $scope.OrderBudgetValueLossList = [];
        $http({
            method: 'GET',
            url: 'Costings/OrderCosting/GetOrderBudgetValueLoss?OrderCostingId=' + $scope.ModelNew.Id,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.OrderBudgetValueLossList = response.data.Pre;
        });
    }

    $scope.TotalValueLossTotal = [{
        title: "Total", summaryColumns: [
            {
                summaryType: ej.Grid.SummaryType.Sum, displayColumn: "TotalOrderCost", dataMember: "TotalOrderCost", format: "{0:N2}"
            }],
        showCaptionSummary: true
    }];

    $scope.OrderBudgetProfitList = [];
    $scope.GetOrderBudgetProfitData = function () {
        $scope.OrderBudgetProfitList = [];
        $http({
            method: 'GET',
            url: 'Costings/OrderCosting/GetOrderBudgetProfit?OrderCostingId=' + $scope.ModelNew.Id,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.OrderBudgetProfitList = response.data.Pre;
        });
    }

    $scope.TotalProfitTotal = [{
        title: "Total", summaryColumns: [
            {
                summaryType: ej.Grid.SummaryType.Sum, displayColumn: "TotalOrderCost", dataMember: "TotalOrderCost", format: "{0:N2}"
            }],
        showCaptionSummary: true
    }];


    $scope.OrderBudgetSalesExpenseList = [];
    $scope.GetOrderBudgetSalesExpenseData = function () {
        $scope.OrderBudgetSalesExpenseList = [];
        $http({
            method: 'GET',
            url: 'Costings/OrderCosting/GetOrderBudgetSalesExpense?OrderCostingId=' + $scope.ModelNew.Id,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.OrderBudgetSalesExpenseList = response.data.Pre;
        });
    }
    $scope.TotalSalesExpenseTotal = [{
        title: "Total", summaryColumns: [
            {
                summaryType: ej.Grid.SummaryType.Sum, displayColumn: "TotalOrderCost", dataMember: "TotalOrderCost", format: "{0:N2}"
            }],
        showCaptionSummary: true
    }];

    //#endregion
}