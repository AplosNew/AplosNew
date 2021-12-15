'use strict';
GeneralDataMasterController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function GeneralDataMasterController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'General Data Master';
    $scope.Action = 'Save';
    $scope.ModelList = [];
    $scope.path = 'Productions/GeneralDataMaster/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'create';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl);
    $scope.searchBy = null; $scope.search = null;
    //$scope.searchBy = "UserName"; $scope.search = "";
    //$scope.searchByList = [{ value: 'Id', name: "Id" }, { value: 'Code', name: "Code" }, { value: 'ShortName', name: "Short Name" }, { value: 'StandardName', name: "Standard Name" }, { value: 'UserName', name: "User Name" }, { value: 'Description', name: "Description" }, { value: 'Remarks', name: "Remarks" }];

    $scope.UOMList = [];

    $scope.getData = function () {
        
        $http({
            method: 'GET',
            url: $scope.path + "getUOM",
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.UOMList = response.data;
        });
    }

    $scope.getData();

   

    $scope.ModelTemp = {
        Id: null,
        UOMId: null,
        Category: null,
        SubCategory: null,
        Item: null,
        UserName: null,
        ValueType:null,
    };
    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);

  

    $scope.Get = function (args) {

        var AllData = [];
        $http({
            method: 'POST',
            url: $scope.path + "Get",
            data: {'Id':args.data.Id},
            dataType: 'JSON'
        }).then(function successCallback(resp) {
            AllData = resp.data.master;
            $scope.ModelNew = Object.assign({}, AllData[0]);
        });

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
                url: $scope.saveUrl,
                data: { 'datas': $scope.ModelNew },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    ClearFields();
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
                    ClearFields();
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
}