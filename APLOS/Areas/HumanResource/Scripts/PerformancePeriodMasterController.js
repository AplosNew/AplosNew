'use strict';
PerformancePeriodMasterController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function PerformancePeriodMasterController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Performance Period';
    //$scope.Action = 'Save';
    //$scope.ModelList = [];
    $scope.path = 'HumanResource/PerformancePeriodMaster/';
    $scope.getListUrl = $scope.path + 'getlist';
//    $scope.saveUrl = $scope.path + 'create';
//    $scope.deleteUrl = $scope.path + 'delete/';
//    baseService.init($scope.getListUrl);


//    $scope.getData = function () {
//        $http({
//            method: 'POST',
//            url: $scope.path + "GetList",
//            data: {},
//            dataType: 'JSON'
//        }).then(function successCallback(response) {
//            $scope.ModelList = response.data;
         
//        });
//    }
//    $scope.getData();

//    $scope.ModelTemp = {
//        SystemId: null,
//        PerformanceYearName: null,
//        StartDate=null,
//        EndDate=null,
//        Remarks: null,
//        Active: true
//    };
//    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);
//    $scope.Get = function (args) {

//        $scope.ModelNew = Object.assign({}, args.data);
//        $scope.Action = 'Update';
//        if (!$rootScope.isCollapsed) {
//            $rootScope.toggle();
//        }
//    };

//    $scope.Save = function () {
//        $scope.$broadcast('show-errors-check-validity');
//        if ($scope.ModelNewForm.$valid) {
//            $http({
//                method: 'POST',
//                url: $scope.saveUrl,
//                data: { 'data': $scope.ModelNew },
//                dataType: 'JSON'
//            }).then(function successCallback(response) {
//                if (response.data.Error === true) {
//                    ShowResult(response.data.Message, 'failure');
//                }
//                else {
//                    ShowResult(response.data.Message, 'success');
                
//                    $scope.getData();

//                }
//            }), function errorCallBack(response) {
//                ShowResult(response.data.Message, 'failure');
//            }

//        }
//    };

//    $scope.Delete = function () {
//        if (!baseService.isUndefinedOrNull($scope.ModelNew.Id)) {
//            $http({
//                method: 'POST',
//                url: $scope.deleteUrl + $scope.ModelNew.Id,
//                dataType: 'JSON'
//            }).then(function successCallback(response) {
//                if (response.data.Error === true) {
//                    ShowResult(response.data.Message, 'failure');
//                }
//                else {
//                    ShowResult(response.data.Message, 'success');
                  
//                    $scope.getData();
//                }
//                function errorCallBack(response) {
//                    ShowResult(response.data.Message, 'failure');
//                }
//            });
//        }
//    };

//    $scope.Clear = function () {
      
//        return true;
//    };

//    function ClearFields(seq) {
//        $scope.Action = 'Save';
//        $scope.ModelNew = {
//            SystemId: null,
//            PerformanceYearName: null,
//            StartDate=null,
//            EndDate=null,
//            Remarks: null,
//            Active: true
//        };
//    }

 

}