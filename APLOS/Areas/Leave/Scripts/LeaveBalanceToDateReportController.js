'use strict';
LeaveBalanceToDateReportController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'cboService', '$window'];
function LeaveBalanceToDateReportController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, cboService, $window) {
    $rootScope.title = 'Leave Register';
    $scope.Action = 'Save';
    $scope.path = 'Leave/LeaveBalanceToDateReport/';

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

    $scope.selectedValues = {
        ToDate: null,
    };




    //#region Get Function
    $scope.sqlInStatement = null;
    $scope.YearId = null;
    $scope.XReport = function () {
        var dataList = [];
        var reportFormat = "Excel";
        try {

            var g = $("#Grid").data("ejGrid");
            dataList = g.getFilteredRecords();
            if (dataList.length == 0) {
                dataList = $scope.EmpData;
            }

            if (dataList.length > 0) {
                var wcId = "";
                if (dataList.length > 0) {
                    wcId = "IN(";
                    wcId += Array.prototype.map.call(dataList, function (item) { return "'" + item.SystemID + "'"; }).join(",") + ")";
                }
                $scope.sqlInStatement = wcId;

            }



            var DropDownListObj = $("#ddlPlantList").data("ejDropDownList");
            var PlantId = DropDownListObj.getSelectedValue();
            if ($scope.selectedValues.ToDate == "" || $scope.selectedValues.ToDate == null) {
                throw "Select Date";
            }
            var url = $scope.path + '/GetReport?reportFormat=' + reportFormat + "&Year=" + $scope.YearId + "&ToDate=" + $scope.selectedValues.ToDate + '&PlantId=' + PlantId + '&empIds=' + $scope.sqlInStatement;

            $rootScope.report(url);
        } catch (e) {
            ShowResult(e, 'info');
        }
    };

    $scope.downloadgriddataUrl = 'GridReports/Download';
    $scope.exportgriddataUrl = 'GridReports/ExcelExportUpd';
    $scope.downloadgriddataUrlPath = 'GridReports/DownloadUsingFullPath';

    $scope.Report = function () {
        var reportFormat = "Excel";
        var dataList = [];
        var g = $("#Grid").data("ejGrid");
        dataList = g.getFilteredRecords();
        if (dataList.length == 0) {
            dataList = $scope.EmpData;
        }

        if (dataList.length > 0) {
            var wcId = "";
            if (dataList.length > 0) {
                wcId = "IN(";
                wcId += Array.prototype.map.call(dataList, function (item) { return "'" + item.SystemID + "'"; }).join(",") + ")";
            }
            $scope.sqlInStatement = wcId;
        }

        var DropDownListObj = $("#ddlPlantList").data("ejDropDownList");
        var PlantId = DropDownListObj.getSelectedValue();
        if ($scope.selectedValues.ToDate == "" || $scope.selectedValues.ToDate == null) {
            throw "Select Date";
        }

        $scope.fileName = 'Leave Register Report.xls';
        $http({
            method: "POST",
            url: 'Leave/LeaveBalanceToDateReport/GetReport',
            data: {
                'reportFormat': reportFormat,
                'Year': $scope.YearId,
                'ToDate': $scope.selectedValues.ToDate,
                'PlantId': PlantId,
                'empIds': $scope.sqlInStatement,
            },
            dataType: 'JSON',
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                $window.open($scope.downloadgriddataUrlPath + "?FullPath=" + response.data.FileName + "&fileName=" + $scope.fileName);
            }
        }, function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
        });

    };





    $scope.EmpData = [];
    $scope.LoadData = function () {
        try {
            //if ($scope.YearId == "" || $scope.YearId == null) {
            //    throw "Select Year";
            //}

            if ($scope.selectedValues.ToDate == "" || $scope.selectedValues.ToDate == null) {
                throw "Select Date";
            }

            var DropDownListObj = $("#ddlPlantList").data("ejDropDownList");
            var PlantId = DropDownListObj.getSelectedValue();

            $http({
                method: 'GET',
                url: $scope.path + 'GetEmp?YearId=' + $scope.YearId + '&ToDate=' + $scope.selectedValues.ToDate + '&PlantId=' + PlantId,
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
        var DropDownListObj = $("#ddlPlantList").data("ejDropDownList");
        var PlantId = DropDownListObj.getSelectedValue();
        $http.get($scope.path + '/GetLeaveBalance?YearId=' + $scope.YearNo + "&ToDate=" + $scope.selectedValues.ToDate + '&PlantId=' + PlantId)
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
        var DropDownListObj = $("#ddlPlantList").data("ejDropDownList");
        var PlantId = DropDownListObj.getSelectedValue();
        $http.get($scope.path + '/GetLeaveBalance?year=' + $scope.YearId + '&empId=' + empId + "&ToDate=" + $scope.selectedValues.ToDate + '&PlantId=' + PlantId)
            .then(function (response) {
                $scope.LeaveBalanceList = response.data;
            });
    };

    //#endregion

    $scope.PlantList = [];
    $scope.getPlant = function () {
        $http({
            method: 'GET',
            url: "humanresource/payrollReports/GetPlantList",
        }).then(function successCallback(response) {
            $scope.PlantList = response.data;
            var index = 0;
            for (var i = 0; i < $scope.PlantList.length; i++) {
                if ($scope.PlantList[i].PlantId == $window.plantId) {
                    index = i;
                }
            }

            $('#ddlPlantList').ejDropDownList(
                {
                    dataSource: $scope.PlantList,
                    fields: { text: "PlantName", value: "PlantId" },
                    selectedIndex: index, showCheckBox: true, multiSelectMode: ej.MultiSelectMode.VisualMode
                    , width: 340
                });

        });
    }
    $scope.getPlant();

}