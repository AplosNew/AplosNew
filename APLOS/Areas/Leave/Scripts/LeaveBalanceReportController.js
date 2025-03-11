'use strict';
LeaveBalanceReportController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'cboService','$window'];
function LeaveBalanceReportController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, cboService, $window) {
    $rootScope.title = 'Leave Register';
    $scope.Action = 'Save';
    $scope.path = 'Leave/LeaveBalanceReport/';

    $scope.LBR = {
        RadioValue: 'General',
    }

    //#region Get year 
    $scope.YearList = [];
    $scope.getYear = function () {
        $http({
            method: 'GET',
            url: 'Attendances/MonthlyAttendanceSummeryReport/GetYear',
        }).then(function successCallback(response) {
            $scope.YearList = response.data;
        });
    }
    $scope.getYear();
    //#endregion

    //#region Get Function
    $scope.YearId = null;
    $scope.downloadgriddataUrlPath = 'GridReports/DownloadUsingFullPath';//DownloadUsingPath
    $scope.Report = function () {
        try {
            var dataList = [];
            var g = $("#Grid").data("ejGrid");
            dataList = g.getFilteredRecords();

            if (dataList.length == 0) {
                dataList = $scope.EmpData;
            }

            if (dataList.length == 0) {
                throw "First click on  Load Data button.";
            }

            $scope.fileName = "LeaveRegisterReport.xlsx";

            $http({
                method: 'POST',
                url: "Leave/LeaveBalanceReport/GetReport",
                data: { 'reportFileName': $scope.fileName, 'data': dataList, 'Year' : $scope.YearId},
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    $window.open($scope.downloadgriddataUrlPath + "?FullPath=" + response.data.FileName + "&fileName=" + $scope.fileName);
                }
            }, function errorCallback(response) {
                ShowResult(response.data.Message, 'failure');
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope._Report = function () {
        var reportFormat = "Excel";
        try {
            if ($scope.YearId == "" || $scope.YearId == null) {
                throw "Select Year";
            }
            var url = 'Leave/LeaveBalanceReport/GetReport?reportFormat=' + reportFormat + "&Year=" + $scope.YearId;

            $rootScope.report(url);
        } catch (e) {
            ShowResult(e, 'info');
        }
    };

    $scope.EmpData = [];
    $scope.LoadData = function () {
        try {
            if ($scope.YearId == "" || $scope.YearId == null) {
                throw "Select Year";
            }
            $http({
                method: 'GET',
                url: $scope.path + 'GetEmp?YearId=' + $scope.YearId,
            }).then(function successCallback(response) {
                $scope.EmpData = response.data;
                for (var i = 0; i < $scope.EmpData.length; i++) {
                    try {
                        if (angular.isUndefinedOrNull($scope.EmpData[i].DOJ) == false)
                            $scope.EmpData[i].DOJ = new Date($scope.EmpData[i].DOJ);
                    } catch (e) {

                    }

                }
            });
        } catch (e) {
            ShowResult(e, 'info');
        }
    }


    $scope.LeaveBalanceList = [];
    $scope.LeaveTypes = function () {
        $http.get('Leave/LeaveBalanceReport/GetLeaveBalance?YearId=' + $scope.YearNo)
            .then(function (response) {
                $scope.LeaveBalanceList = response.data;
            });
    };

    //#endregion

    //#region TAB
    $scope.ShowDiv = false;
    $scope.AddLineItemT = function (obj) {
        try {
            $scope.ShowDiv = true;
            //$scope.PlantId = obj.data.SystemID;
            var eDialog = $("#policyID").data("ejDialog");
            eDialog.open();
            $scope.LeaveTypes(obj.data.SystemID);
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.LeaveTypes = function (empId) {
        $http.get('Leave/LeaveBalanceReport/GetLeaveBalance?year=' + $scope.YearId + '&empId=' + empId)
            .then(function (response) {
                $scope.LeaveBalanceList = response.data;
            });
    };

    //#endregion

}