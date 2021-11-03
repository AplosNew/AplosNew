'use strict';
SandWichLeaveOnHolidayController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function SandWichLeaveOnHolidayController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
   // $rootScope.title = 'Sand Wich Leave On Holiday';
    $scope.Action = 'Save'; 
    $scope.path = 'Leave/SandWichLeaveOnHoliday/';   
    $scope.saveUrl = $scope.path + 'ProcessSandwich';

    $scope.ModelNew = {
        FromDate: $filter('dateFiltering')(Date.now()),
        ToDate: $filter('dateFiltering')(Date.now())

    };




    $scope.getReport = function () {
        if (baseService.isUndefinedOrNull($scope.ModelNew.FromDate)) {
            manualValidation("div_FromDate", true, "From Date is required.");
        }
        else if (baseService.isUndefinedOrNull($scope.ModelNew.ToDate)) {
            manualValidation("div_ToDate", true, "To Date is required.");
        }

        //else if (baseService.isUndefinedOrNull($scope.report.FromDate)) {
        //    manualValidation('div_FromDate', true, "From Date is required.");
        //}
        //else if (baseService.isUndefinedOrNull($scope.report.ToDate)) {
        //    manualValidation('div_ToDate', true, "To Date is required.");
        //}

        else if (new Date($scope.ModelNew.FromDate) > new Date($scope.ModelNew.ToDate)) {
            manualValidation('div_FromDate', true, "From date must be below or equal to To Date");
        }
        else if (new Date($scope.ModelNew.ToDate) < new Date($scope.ModelNew.FromDate)) {
            manualValidation('div_ToDate', true, "To date must be above or equal to From Date.");
        }
        else {
            return false;
        }
    };


    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        //$scope.dateValidation()
        if ($scope.SandwichLeaveOnHoliday.$valid && !$scope.getReport()) {
            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: { 'sFromDate': $scope.ModelNew.FromDate, 'sTodate': $scope.ModelNew.ToDate },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');                   
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        }
    };

   
}