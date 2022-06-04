'use strict';
LICController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function LICController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'LIC';
    $scope.Action = 'Save';
    $scope.LICModelList = [];
    $scope.path = 'Securities/LICController/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'create';
    $scope.deleteUrl = $scope.path + 'delete/';
    $scope.Action = 'Save';

    $scope.getLICData = function () {
        $http({
            method: 'GET',
            url: $scope.path + "GetList",
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.LICModelList = response.data;
        });
    }
    $scope.getLICData();

    
    $scope.ModelTemp = {
        Id: null,
        LicKey1: null,
        LicKey2: null
    };
    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);

    //$scope.GetSequence = function () {
    //    cboService.getSequence($scope.getSeqUrl, function (data) {
    //        $scope.ModelTemp.Sequence = data;
    //        $scope.ModelNew.Sequence = data;
    //    });
    //};
    //$scope.GetSequence();

    $scope.Get = function (args) {

        $scope.ModelNew = Object.assign({}, args.data);
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
                data: { 'data': $scope.ModelNew },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    //ClearFields(response.data.Sequence);
                    $scope.getLICData();

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        }
    };

    //$scope.Delete = function () {
    //    if (!baseService.isUndefinedOrNull($scope.ModelNew.Id)) {
    //        $http({
    //            method: 'POST',
    //            url: $scope.deleteUrl + $scope.ModelNew.Id,
    //            dataType: 'JSON'
    //        }).then(function successCallback(response) {
    //            if (response.data.Error === true) {
    //                ShowResult(response.data.Message, 'failure');
    //            }
    //            else {
    //                ShowResult(response.data.Message, 'success');
    //                ClearFields(response.data.Sequence);
    //                $scope.getData();
    //            }
    //            function errorCallBack(response) {
    //                ShowResult(response.data.Message, 'failure'); 
    //            }
    //        });
    //    } 
    //};

    //$scope.Clear = function () {
    //    ClearFields($scope.GetSequence());
    //    return true;
    //};

    //function ClearFields(seq) {
    //    $scope.Action = 'Save';
    //    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);
    //    $scope.ModelNew.Sequence = seq;
    //}
}