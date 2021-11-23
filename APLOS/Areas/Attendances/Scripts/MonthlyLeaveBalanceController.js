'use strict';
MonthlyLeaveBalanceController.$inject = ['addressService', 'fileReader', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$window'];
function MonthlyLeaveBalanceController(addressService, fileReader, cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window) {
    $rootScope.title = ' Monthly Leave Balance ';
    $scope.Action = 'Save';
    $scope.path = 'Attendances/MonthlyLeaveBalance/';
    $scope.salaryProcessModel = {

        MonthId: null,
        YearId: null

    };
    $scope.yearList = [];
    cboService.getCboLeaveYear(function (result) {
        $scope.yearList = result;
    });

    $scope.Save = function () {

        try {
            if (baseService.isUndefinedOrNull($scope.salaryProcessModel.MonthId))
                throw 'Please select month';

            if (baseService.isUndefinedOrNull($scope.salaryProcessModel.YearId))
                throw 'Please select year';


            $http({
                method: 'POST',
                url: $scope.path + 'ProcessMonthlyLeaveBalance',
                data: { year: $scope.salaryProcessModel.YearId, month: $scope.salaryProcessModel.MonthId },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.getSavedPayRollGroupData();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };
        } catch (e) {
            ShowResult(e, 'failure');
        }

    }


}

