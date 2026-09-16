'use strict';
DispatchController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$controller', 'cboService', '$window'];
function DispatchController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $controller, cboService, $window) {
    $rootScope.title = "Dispatch";
    $scope.Action = 'Save';
    $scope.ModelList = [];
    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.PRMCboList = [];
    $scope.GetPRMCbo = function () {
        $http({
            method: "GET",
            url: "OrderManagements/ProductionOrder/GetPRMCbo"
        }).then(function (response) {
            $scope.PRMCboList = response.data;
        });
    };
    $scope.GetPRMCbo();

    $scope.searchBy = "ParentPO"; $scope.search = "";
    $scope.searchByList = [{ value: 'Id', name: "Id" }, { value: 'ParentPO', name: "ParentPO" }, { value: 'DestinationPO', name: "DestinationPO" }, { value: 'Remarks', name: "Remarks" }];


    $scope.getData = function () {
        $http({
            method: 'POST',
            url: "OrderManagements/ProductionOrder/GetDispatch",
            data: { column: $scope.searchBy, value: $scope.search },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ModelList = response.data;
        });
    }
    $scope.getData();

    $scope.ModelTemp = {
        Id: null,
        PackingId: null,
        ParentPO: null,
        DestinationPO: null,
        BillingInvoicing: null,
        StyleNumber: null,
        Gender: null,
        DispatchSO: null,
        InvoiceNO: null,
        Remarks: null,
        AddedBy: null,
        AddedDate: null,
        AddedFromIP: null,
        UpdatedBy: null,
        UpdatedDate: null,
        UpdatedFromIP: null
    };
    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);

    $scope.Get = function (args) {
        $scope.ModelNew = Object.assign({}, args.data);
        $scope.GetSKUDetailList();
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.ModelNewForm.$valid) {
            $http({
                method: 'POST',
                url: 'OrderManagements/ProductionOrder/CreateDispatch',
                data: { 'data': $scope.ModelNew },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    ClearFields();
                    $scope.getData();

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.ModelNew.Id)) {
            $http({
                method: 'POST',
                url: "OrderManagements/ProductionOrder/DeleteDispatch/" + $scope.ModelNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    ClearFields();
                    $scope.getData();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };

    $scope.Clear = function () {
        ClearFields();
        return true;
    };

    function ClearFields() {
        $scope.Action = 'Save';
        $scope.ModelNew = Object.assign({}, $scope.ModelTemp);
    }

    $scope.SKUDetailList = [];
    $scope.GetSKUDetailList = function () {
        $http({
            method: 'GET',
            url: 'OrderManagements/ProductionOrder/GetSKUDetailList?masterId=' + $scope.ModelNew.Id + '&packId=' + $scope.ModelNew.PackingId
        }).then(function successCallback(response) {
            $scope.SKUDetailList = response.data;
        });
    }

    $scope.SaveDispatchChild = function () {
        try {
            if ($scope.SKUDetailList.length > 0) {
                var tempList = [];

                for (var i = 0; i < $scope.SKUDetailList.length; i++) {
                    if (!baseService.isUndefinedOrNull($scope.SKUDetailList[i].Qty) || $scope.SKUDetailList[i].Qty > 0) {
                        tempList.push($scope.SKUDetailList[i]);
                    }

                }
                $http({
                    method: 'POST',
                    url: "OrderManagements/ProductionOrder/SaveDispatchChild",
                    data: { 'skulist': tempList, 'masterId': $scope.ModelNew.Id },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.GetSKUDetailList();
                    }
                }, function errorCallback(response) {
                    ShowResult(response.data.Message, 'failure');
                });

            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }


}

