'use strict';
DefineLotController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$window'];
function DefineLotController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window) {
    $rootScope.title = 'Define Lot';
    $scope.path = 'Productions/Packing/';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.Action = "Save";
    $scope.modal = {};
    $scope.modalNew = {};

    $scope.searchBy = "UserName"; $scope.search = "";
    $scope.searchByList = [{ value: 'Id', name: "Id" }, { value: 'Code', name: "Code" }, { value: 'ShortName', name: "Short Name" }, { value: 'StandardName', name: "Standard Name" }, { value: 'UserName', name: "User Name" }, { value: 'Description', name: "Description" }, { value: 'Remarks', name: "Remarks" }];


    $scope.getData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetList",
            data: { column: $scope.searchBy, value: $scope.search },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ModelList = response.data;
        });
    }
    $scope.getData();


    $scope.GetLotNumber = function () {
        $http({
            method: 'GET',
            url: $scope.path + "GetLotNumber",
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.modalNew.LotNumber = response.data;
        });
    };
    $scope.GetLotNumber();

    $scope.GetSequence = function () {
        cboService.getSequence($scope.getSeqUrl, function (data) {
            $scope.modal.Sequence = data;
            $scope.modalNew.Sequence = data;
        });
    };
    $scope.GetSequence();

    $scope.SubProductionOrderList = [];
    $scope.skuList = [];
    $scope.purposeList = [];
    $http({
        method: 'GET',
        url: "Productions/MovementMaterialMaster/getPurposeCategory",
        dataType: 'JSON'
    }).then(function successCallback(response) {
        $scope.purposeList = response.data;
    });

    $scope.getLocation = function () {
        if (!baseService.isUndefinedOrNull($scope.modalNew.PurposeId)) {
            var paymentTerm = $.grep($scope.purposeList, function (item) { return item.Value === $scope.modalNew.PurposeId; })[0];
            $scope.modalNew.Location = paymentTerm.FromToLocation;

        }
    };


    $scope.CustomersList = [];
    $http({
        method: 'GET',
        url: $scope.path + "getCustomers",
        dataType: 'JSON'
    }).then(function successCallback(response) {
        $scope.CustomersList = response.data;
    });

    $scope.selectCustomer = function () {
        angular.element(document.querySelector('#customersModal')).modal('show');
    }

    $scope.doubleCustomer = function (e) {
        $scope.modalNew.Customer = e.data.username;
        $scope.modalNew.CustomerId = e.data.id;
        angular.element(document.querySelector('#customersModal')).modal('hide');
    }

    $scope.ProcessList = [];
    $scope.GetProcessByCompany = function () {
        cboService.getCompanyProductionProcessCbo($window.companyId, function (response) {
            $scope.ProcessList = response;
        });
    }
    $scope.GetProcessByCompany();


    $scope.Get = function (args) {

        $scope.modalNew = Object.assign({}, args.data);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.ProductionOrderList = [];
    $scope.getProductionOrderPopUp = function () {

        $scope.ProductOrderList = [];
        $http.get('Productions/ProductionSummary/GetProductionOrderData?entityid=' + null + '&workCenterMasterId=' + null + '&productionLevel=' + null + '&processId=' + $scope.modalNew.ProcessId + '&status=' + null)
            .then(
                function successCallback(response) {
                    $scope.ProductionOrderList = response.data;
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
        angular.element(document.querySelector('#POItemPopup')).modal('show');

    };

    $scope.SetPrOData = function ($event) {
        $scope.modalNew.ProductionOrderId = $event.data.POId;
        angular.element(document.querySelector('#POItemPopup')).modal('hide');

    }

    $scope.SalesOrderListForProductionOrderId = [];
    $scope.getSalesOrderOfProdOrderList = function (prodOrdId) {
        $scope.openPopup('dialogSOItemsForProductionOrder');
        $http({
            method: 'GET',
            url: 'OrderManagements/ProductionOrder/GetProductionRecipeMaterialList?productionOrderId=' + prodOrdId
        }).then(function successCallback(response) {
            $scope.SalesOrderListForProductionOrderId = response.data;

        });
    }

    $scope.summaryRows = [{
        title: "Total Qty", summaryColumns: [{ summaryType: ej.Grid.SummaryType.Sum, displayColumn: "Qty", dataMember: "Qty", format: "{0:N0}" }],
        showCaptionSummary: true

    }];

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

    $scope.SubProductionOrderList = [];
    $scope.GetSPOata = function () {
        $http({
            method: 'POST',
            data: { 'poId': $scope.modalNew.ProductionOrderId },
            url: 'OrderManagements/ProductionOrder/GetSavedSKUData'
        }).then(function successCallback(response) {
            $scope.SubProductionOrderList = response.data;
            angular.element(document.querySelector('#SPOItemPopup')).modal('show');

        });
    }

    $scope.selectSPOItem = function (e) {
        $scope.modalNew.SubProductionOrderId = e.data.ID;
        $scope.modalNew.SalesOrder = e.data.SONo;
        angular.element(document.querySelector('#SPOItemPopup')).modal('hide');
    }


    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.ModelNewForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: { 'data': $scope.modalNew },
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
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.modalNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.modalNew.Id,
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
        ClearFields($scope.GetSequence());
        $scope.GetLotNumber();
        return true;
    };

    function ClearFields(seq) {
        $scope.Action = 'Save';
        $scope.modalNew = Object.assign({}, $scope.modal);
        $scope.modalNew.Sequence = seq;
    }
















}