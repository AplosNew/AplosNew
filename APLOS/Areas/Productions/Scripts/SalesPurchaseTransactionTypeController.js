'use strict';
SalesPurchaseTransactionTypeController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function SalesPurchaseTransactionTypeController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Sales';
    $scope.Action = 'Save';
    $scope.ModelList = [];
    $scope.path = 'Productions/SalesPurchaseTransactionType/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.getSeqUrl2 = $scope.path + 'GetAutoSequence2';
    $scope.saveUrl = $scope.path + 'create';
    $scope.savePurchaseUrl = $scope.path + 'CreatePurchase';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl);
    $scope.searchBy = "UserName"; $scope.search = "";
    $scope.searchByList = [{ value: 'Id', name: "Id" }, { value: 'Code', name: "Code" }, { value: 'ShortName', name: "Short Name" }, { value: 'StandardName', name: "Standard Name" }, { value: 'UserName', name: "User Name" }, { value: 'Description', name: "Description" }, { value: 'Remarks', name: "Remarks" }];

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };

    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.transactionTypesList = [];
    cboService.getEnumCbo('Enum/GetTransactionTypeEnumCbo', function (result) {
        $scope.transactionTypesList = result;
    });

    $scope.getData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetList",
            data: { column: $scope.searchBy, value: $scope.search },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ModelList = response.data;
            ClearFields(response.data.Sequence);
            $scope.GetSequence();
        });
    }
    $scope.getData();

    $scope.ModelTemp = {
        Id: null,
        Sequence: 1,
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        TransactionType: null,
        SalesPurchaseType: 'Sales',
        Description: null,
        Remarks: null,
        Active: true,
        AddedBy: null,
        AddedDate: null,
        AddedFromIP: null,
        UpdatedBy: null,
        UpdatedDate: null,
        UpdatedFromIP: null

    };
    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);

    $scope.GetSequence = function () {
        cboService.getSequence($scope.getSeqUrl, function (data) {
            $scope.ModelTemp.Sequence = data;
            $scope.ModelNew.Sequence = data;
        });
    };
    $scope.GetSequence();

    $scope.Get = function (args) {

        $scope.ModelNew = Object.assign({}, args.data);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.Save = function () {
        $scope.ModelNew.SalesPurchaseType = 'Sales';
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.ModelNewForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: { 'data': $scope.ModelNew },
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
        if (!baseService.isUndefinedOrNull($scope.ModelNew.Id)) {
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
        ClearFields($scope.GetSequence());
        return true;
    };

    function ClearFields(seq) {
        $scope.Action = 'Save';
        $scope.ModelNew = Object.assign({}, $scope.ModelTemp);
        $scope.ModelNew.Sequence = seq;
    }

    $scope.PurchaseType = 'Purchase';
    $scope.PurchaseList = [];
    $scope.GetPurchaseList = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetPurchaseList",
            data: { column: $scope.searchBy, value: $scope.search },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.PurchaseList = response.data;
            ClearFields(response.data.Sequence);
            $scope.GetSequence2();
        });
    }
    $scope.GetPurchaseList();

    $scope.ModelTemp2 = {
        Id: null,
        Sequence: 1,
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        TransactionType: null,
        SalesPurchaseType: 'Purchase',
        Description: null,
        Remarks: null,
        Active: true,
        AddedBy: null,
        AddedDate: null,
        AddedFromIP: null,
        UpdatedBy: null,
        UpdatedDate: null,
        UpdatedFromIP: null

    };
    $scope.PurchaseModel = Object.assign({}, $scope.ModelTemp2);

    $scope.GetSequence2 = function () {
        cboService.getSequence($scope.getSeqUrl2, function (data) {
            $scope.PurchaseModel.Sequence = data;
        });
    };
    $scope.GetSequence2();

    $scope.Get2 = function (args) {

        $scope.PurchaseModel = Object.assign({}, args.data);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.Save2 = function () {
        $scope.PurchaseModel.SalesPurchaseType= 'Purchase';
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.PurchaseModelForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.savePurchaseUrl,
                data: { 'data': $scope.PurchaseModel },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    ClearFields2(response.data.Sequence);
                    $scope.GetPurchaseList();

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        }
    };

    $scope.removeRow = function () {
        if (baseService.isUndefinedOrNull($scope.PurchaseModel.Id))
            $scope.message_confirmation = 'Are you sure want to delete this data....';
        else
            $scope.message_confirmation = 'Are you sure want to delete [ ' + $scope.PurchaseModel.UserName + ' ]';
        angular.element(document.querySelector('#confirmgenericPopUp')).modal('show');
    };

    $scope.Delete2 = function () {
        if (!baseService.isUndefinedOrNull($scope.PurchaseModel.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.PurchaseModel.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    ClearFields2(response.data.Sequence);
                    $scope.GetPurchaseList();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };

    $scope.Clear2 = function () {
        ClearFields2($scope.GetSequence2());
        return true;
    };

    function ClearFields2(seq) {
        $scope.Action = 'Save';
        $scope.PurchaseModel = Object.assign({}, $scope.ModelTemp);
        $scope.PurchaseModel.Sequence = seq;
    }

}