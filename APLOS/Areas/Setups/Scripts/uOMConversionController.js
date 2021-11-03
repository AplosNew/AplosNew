'use strict';
UOMConversionController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function UOMConversionController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "UOM Conversion";
    $scope.Action = 'Save';
    $scope.isDeleted = true;
    $scope.index = -1;
    $scope.uOMConversions = [];
    $scope.path = 'Setups/uomconversion/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl, null, null, null, 'FromUOM', 'FromUOM');
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.uOMConversions = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.getData();

    $scope.uOMConversion = {
        Id: null,
        CompanyGroupId: null,
        FromUOMId: null,
        FromUOMFactor: 1,
        ToUOMId: null,
        ToUOMFactor: null
    };

    $scope.uOMConversionNew = Object.assign({}, $scope.uOMConversion);

    $scope.searchByList = [
        {
            'name': 'From UOM',
            'value': 'FromUOM'
        },
        {
            'name': 'From UOM Factor',
            'value': 'FromUOMFactor'
        },
        {
            'name': 'To UOM',
            'value': 'ToUOM'
        },
        {
            'name': 'To UOM Factor',
            'value': 'ToUOMFactor'
        }
    ];

    $scope.uomList = [];
    $http({
        method: 'GET',
        url: 'Setups/unitofmeasurement/getcbo'
    }).then(function successCallback(response) {
        $scope.uomList = response.data;
    });

    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.uOMConversion = $scope.uOMConversions[$scope.index];
        $scope.uOMConversionNew = Object.assign({}, $scope.uOMConversion);
        $scope.isDeleted = false;
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };
    $scope.Save = function () {
        try {
            Validation("From UOM Factor Uom", $scope.uOMConversionNew.FromUOMId);
            Validation("To UOM Factor", $scope.uOMConversionNew.ToUOMFactor);
            Validation("To UOM Factor Uom", $scope.uOMConversionNew.ToUOMId);

            angular.copy($scope.uOMConversionNew, $scope.uOMConversion);
            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: $scope.uOMConversion,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.getData();
                    ClearFields();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }
    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.uOMConversionNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.uOMConversionNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.getData();
                    ClearFields();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
        else {
            ShowResult(commonMessage.primaryKeyNullMessage, 'failure');
        }
    }
    $scope.Clear = function () {
        ClearFields();
        return true;
    }
    function ClearFields() {
        $scope.isDeleted = true;
        $scope.uOMConversion = {};
        $scope.uOMConversionNew = { FromUOMFactor: 1 };
    }
    function Validation(field, value) {
        if (baseService.isUndefinedOrNull(value)) {
            throw 'Please select ' + field + "........!";
        }
    }
}