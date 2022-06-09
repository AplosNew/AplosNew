'use strict';
LICController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function LICController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'LIC';
    $scope.Action = 'Save';
    $scope.LICModelList = [];
    $scope.path = 'Securities/LIC/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';
    $scope.Action = 'Save';

    $scope.ModelTemp = {
        Id: null,
        LicKey1: null,
        LicKey2: null
    };
    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);

    $scope.getLICData = function () {
        $scope.LICModelList = [];
        $http({
            method: 'GET',
            url: $scope.getListUrl,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.LICModelList = response.data;
            if (baseService.arrayLength($scope.LICModelList)>0) {
                $scope.ModelNew = Object.assign({}, response.data[0]);
                $scope.ModelNew.LicKey1 = $filter('dateFiltering')($scope.ModelNew.LicKey1, 'dd-M-yyyy');
                $scope.ModelNew.LicKey2 = $filter('dateFiltering')($scope.ModelNew.LicKey2, 'dd-M-yyyy');
            }

        });
    }
    $scope.getLICData();

    //$scope.Get = function (args) {
    //    $scope.ModelNew = Object.assign({}, args.data);
    //    $scope.ModelNew.LicKey1 = $filter('dateFiltering')($scope.ModelNew.LicKey1, 'dd-M-yyyy');
    //    $scope.ModelNew.LicKey2 = $filter('dateFiltering')($scope.ModelNew.LicKey2, 'dd-M-yyyy');
    //    $scope.Action = 'Update';
    //    if (!$rootScope.isCollapsed) {
    //        $rootScope.toggle();
    //    }
    //};

    $scope.Save = function () {
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
                    $scope.getLICData();

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        }
    };

    
}