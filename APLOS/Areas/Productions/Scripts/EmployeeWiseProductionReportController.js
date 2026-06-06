
'use strict';
EmployeeWiseProductionReportController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function EmployeeWiseProductionReportController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "Production Target Report";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.path = 'Productions/ProductionReport/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';

    $scope.fromDate = $filter('dateFiltering')(Date.now());
    $scope.toDate = $filter('dateFiltering')(Date.now());
    $scope.IncentiveType = '';
    $scope.EntityId = '';
    $scope.shiftId = '';
    $scope.workCenterId = '';
    $scope.dayStatus = '';
    $scope.ReportFormat = 'Excel';

    cboService.getCboEntityByPlant(null, null, "", function (result) {
        $scope.entityList = result;
    });
    $scope.ShiftList = [];
    $scope.ProcessList = [];
    $scope.workCenterList = [];
    $scope.dayStatusList = [];
    $scope.getProcess = function () {
        $http({
            method: 'POST',
            url: 'Productions/EmployeeOperations/GetProcess',
            data: { 'EId': $scope.EntityId }
        }).then(function succ(resp) {
            $scope.ProcessList = resp.data;
        });
    }
    $scope.GetShiftList = function () {
        $http.get('Productions/EmployeeOperations/GetShift?processId=' + $scope.ProcessId + '&entityId=' + $scope.EntityId)
            .then(function (response) {
                if (baseService.arrayLength(response.data) > 0) {
                    $scope.ShiftList = response.data;
                    if (baseService.arrayLength(response.data) === 1) {
                        $scope.shiftId = $scope.ShiftList[0].Value;
                    }
                }
            });
    }

    $scope.getWkC = function () {
        $scope.workCenterList = [];
        $http({
            method: 'POST',
            url: 'Productions/EmployeeOperations/GetWorkCenter',
            data: { 'PId': $scope.ProcessId, 'entityId': $scope.EntityId }
        }).then(function succ(resp) {
            $scope.workCenterList = resp.data;
        });
    }

    $scope.getDayStatus = function () {
        $http({
            method: 'POST',
            url: 'Productions/ProductionReport/GetDayStatusCbo',
        }).then(function succ(resp) {
            $scope.dayStatusList = resp.data;
        });
    }
    $scope.getDayStatus();
    $scope.EmployeeWiseProductionReport = function () {
        try {
            if (angular.isUndefinedOrNull($scope.fromDate))
                throw 'Plase select from date.';
            if (angular.isUndefinedOrNull($scope.toDate))
                throw 'Plase select to date.';
            var file_src = $scope.path + 'GetEmployeeWiseProductionReport?reportFormat=' + $scope.ReportFormat + '&fromDate=' + $scope.fromDate + '&toDate=' + $scope.toDate + '&entityId='
                + $scope.EntityId + '&incentiveType=' + $scope.IncentiveType + '&shiftId=' + $scope.shiftId + '&workCenterId=' + $scope.workCenterId + '&dayStatus=' + $scope.dayStatus;
            $rootScope.report(file_src);
        } catch (e) {

        }
    }
    $scope.EmployeeWiseProductionSummary = function () {
        try {
            if (angular.isUndefinedOrNull($scope.fromDate))
                throw 'Plase select from date.';
            if (angular.isUndefinedOrNull($scope.toDate))
                throw 'Plase select to date.';
            var file_src = $scope.path + 'GetEmployeeWiseProductionSummaryReport?reportFormat=' + $scope.ReportFormat + '&fromDate=' + $scope.fromDate + '&toDate=' + $scope.toDate + '&entityId=' + $scope.EntityId
                + '&incentiveType=' + $scope.IncentiveType + '&shiftId=' + $scope.shiftId + '&workCenterId=' + $scope.workCenterId + '&dayStatus=' + $scope.dayStatus;
            $rootScope.report(file_src);
        } catch (e) {

        }
    }

    $scope.EfficencyIncentiveReport = function () {
        try {
            if (angular.isUndefinedOrNull($scope.fromDate))
                throw 'Plase select from date.';
            if (angular.isUndefinedOrNull($scope.toDate))
                throw 'Plase select to date.';
            var file_src = $scope.path + 'GetEfficencyIncentiveReport?reportFormat=' + $scope.ReportFormat + '&fromDate=' + $scope.fromDate + '&toDate=' + $scope.toDate + '&entityId=' + $scope.EntityId
                + '&incentiveType=' + $scope.IncentiveType + '&shiftId=' + $scope.shiftId + '&workCenterId=' + $scope.workCenterId + '&dayStatus=' + $scope.dayStatus;
            $rootScope.report(file_src);
        } catch (e) {

        }
    }

    $scope.IncentiveTypeList = [];
    $scope.GetIncentiveType = function () {
        $http({
            method: 'GET',
            url: 'IE/IncentiveType/GetCbo'
        }).then(function successCallback(response) {
            $scope.IncentiveTypeList = response.data;
            console.log('incentiveType', $scope.IncentiveTypeList)
        });
    }
    $scope.GetIncentiveType();
}