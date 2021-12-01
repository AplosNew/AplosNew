'use strict';
LeavesChecklistReportNewController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'toaster', 'cboService', '$controller','$window'];
function LeavesChecklistReportNewController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, toaster, cboService, $controller, $window) {
    $scope.path = 'HumanResource/LeavesChecklistReportNew/';
    $rootScope.title = 'Leaves Check list Report New';
/*    $controller('employeeBaseController', { $scope: $scope, $http: $http });*/

    var date = new Date(), y = date.getFullYear(), m = date.getMonth();
    var firstDay = new Date(y, m, 1);

    $scope.LeavesCheckListReport = {
        FromDate: $filter('dateFiltering')(firstDay),
        ToDate: $filter('dateFiltering')(Date.now()),
        ReportFormat: 'Excel',
    };

    $scope.LeavesCheckListReportData = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.LeavesCheckListReport.FromDate)) {
                manualValidation('div_FromDate', true, "From Date is required.");
                ShowResult("From Date is required.", 'failure');
            }
            else if (baseService.isUndefinedOrNull($scope.LeavesCheckListReport.ToDate)) {
                manualValidation('div_ToDate', true, "To Date is required.");
                ShowResult("To Date is required.", 'failure');
            }
            else if (new Date($scope.LeavesCheckListReport.FromDate) > new Date($scope.LeavesCheckListReport.ToDate)) {
                manualValidation('div_FromDate', true, "From date must be below or equal to To Date");
                ShowResult("From date must be below or equal to To Date", 'failure');
            }
            else if (new Date($scope.LeavesCheckListReport.ToDate) < new Date($scope.LeavesCheckListReport.FromDate)) {
                manualValidation('div_ToDate', true, "To date must be above or equal to From Date.");
                ShowResult("To date must be above or equal to From Date.", 'failure');
            }
            else {
                var url = 'HumanResource/LeavesChecklistReportNew/GetleavesChecklistReport?reportFormat=Excel' + ' &FromDate=' + $scope.LeavesCheckListReport.FromDate + ' &ToDate=' + $scope.LeavesCheckListReport.ToDate;
                $rootScope.report(url);
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };
     $scope.PlantIdFromUI = null;
    $scope.PlantList = [];
    $scope.getPlant = function () {
        $http({
            method: 'GET',
            url: $scope.path + "GetPlantList",
        }).then(function successCallback(response) {
            $scope.PlantList = response.data;
            var index = 0;
            for (var i = 0; i < $scope.PlantList.length; i++) {
                if ($scope.PlantList[i].PlantId == $window.plantId) {
                    index = i;
                }
            }
            $('#CWPlant').ejDropDownList(
                {
                    dataSource: $scope.PlantList,
                    fields: { text: "PlantName", value: "PlantId" },
                    selectedIndex: index, showCheckBox: true, multiSelectMode: ej.MultiSelectMode.VisualMode
                    ,width:250
                });

        });
    }
    $scope.getPlant();

    $scope.LeaveTypeList = [];
    $scope.getLeave = function () {
        $http({
            method: 'GET',
            url: $scope.path + "GetLeaveType",
        }).then(function successCallback(response) {
            $scope.LeaveTypeList = response.data;
        });
    }
    $scope.getLeave();
}