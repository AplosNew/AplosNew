'use strict';
WebBasedPackingController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'toaster', 'cboService', '$controller', '$window'];
function WebBasedPackingController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, toaster, cboService, $controller, $window) {
    $rootScope.title = 'Web Based Packing';
    $scope.Action = 'Save';
    $scope.path = 'HumanResource/WebBasedPacking/';
    var currentDate = new Date();
    var time = currentDate.getHours() + ":" + currentDate.getMinutes() + ":" + currentDate.getSeconds();
    //var getCurrenttime = () => {}
    $scope.ModelTemp = {
        Id: null,
        EntityId: null,
        PurposeId: null,
        FromLoc: null,
        LocMasterId: null,
        WorkDate: currentDate,
        Time: time,
        ShiftId: null,

    };
    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);

    $scope.EntityList = [];
    $scope.GetEntity = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'GetEntity',
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.EntityList = resp.data;
        });
    }
    $scope.GetEntity();

    $scope.PurposeList = [];
    $scope.GetPurpose = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'GetPurpose',
            data: { 'Entity': $scope.ModelNew.EntityId },
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.PurposeList = resp.data;
        });
    }
    //$scope.GetPurpose();

    $scope.FromLocationList = [];
    $scope.GetFromLocation = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'FromLoc',
            data: {
                'Entity': $scope.ModelNew.EntityId,
                'Purpose': $scope.ModelNew.PurposeId
            },
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.FromLocationList = resp.data;
        });
    }
    //$scope.GetFrom();

    $scope.ToLocationList = [];
    $scope.GetToLocation = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'ToLoc',
            data: {
                'Entity': $scope.ModelNew.EntityId,
                'Purpose': $scope.ModelNew.PurposeId,
                'FromLoc': $scope.ModelNew.FromLoc
            },
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.ToLocationList = resp.data;
        });
    }

    $scope.ShiftList = [];
    $scope.GetShift = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'GetShiftMaster',
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.ShiftList = resp.data;
        });
    }
    $scope.GetShift();

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        $http({
            method: 'POST',
            url: $scope.path + 'Save',
            data: {
                'datas': $scope.ModelNew,
                
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
               
                //$scope.getData();

            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        }

        /*}*/
    };

    $scope.ScannedData = [];
    $scope.BarCodeScan = function () {
      //  console.log(sender);
        $http({
            method: 'POST',
            url: $scope.path + 'Scanner_Clicked',
            data: {
                'datas': $scope.ModelNew,
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.ScannedData = response.data;
                

            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        }

    }
}