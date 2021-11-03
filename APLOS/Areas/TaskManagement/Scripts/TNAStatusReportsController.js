'use strict';
TNAStatusReportsController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$controller'];
function TNAStatusReportsController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $controller) {
    $scope.title = 'Task Status Report';
    $scope.ModelList = [];
    $scope.path = 'TaskManagement/TNAStatusReports/';
    $scope.ModelList = [];
    $controller('taskDetailController', { $scope: $scope, $http: $http });

    $scope.getData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetList",
            data: { column: $scope.searchBy, value: $scope.search },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ModelList = response.data;
        });
    }
    $scope.showLegends = function () {
        $("#dialogLegends").data("ejDialog").open();
    }
    $scope.taskcolorchange = function (args) {
        try {

            //today's task
            var DueDate = new Date(args.data.DueDate);
            if (DueDate.getDate() == new Date().getDate()
                && DueDate.getMonth() == new Date().getMonth()
                && DueDate.getFullYear() == new Date().getFullYear()) {
                args.cell.bgColor = "#E6F0FF";
            }

            //overdue
            if (new Date(DueDate.getFullYear(), DueDate.getMonth(), DueDate.getDate()) < new Date(new Date().getFullYear(), new Date().getMonth(), new Date().getDate())) {
                args.cell.bgColor = "#FFF4E6";
            }

            //future
            if (new Date(DueDate.getFullYear(), DueDate.getMonth(), DueDate.getDate()) > new Date(new Date().getFullYear(), new Date().getMonth(), new Date().getDate())) {
                args.cell.bgColor = "#F5FFE6";
            }

            if (args.data.CurrentStatus == "Closed") {
                var ClosingDate = args.data.ClosingDate;
                //late closed
                if (new Date(DueDate.getFullYear(), DueDate.getMonth(), DueDate.getDate()) < new Date(ClosingDate.getFullYear(), ClosingDate.getMonth(), ClosingDate.getDate())) {
                    args.cell.bgColor = "#52b3d9";
                }

                //early closed
                if (new Date(DueDate.getFullYear(), DueDate.getMonth(), DueDate.getDate()) >= new Date(ClosingDate.getFullYear(), ClosingDate.getMonth(), ClosingDate.getDate())) {
                    args.cell.bgColor = "#2ecc71";
                }
            }

        } catch (e) {

        }
    }
    $scope.Durration = "";
    $scope.Status = "";
    $scope.isValid = true;
    $scope.ActiveStatus = "Active";

    $scope.exportgriddataUrl = 'GridReports/ExcelExport';
    $scope.downloadgriddataUrl = 'GridReports/Download';
    $scope.downloadgriddataPDFUrl = 'GridReports/DownloadPdf';

    $scope.Today = new Date();
    $scope.PreviousMonth = new Date().setDate(new Date().getDate() - 31);
    $scope.NextMonth = new Date().setDate(new Date().getDate() + 31);
    $scope.FromDate = $filter("dateFiltering")($scope.PreviousMonth);
    $scope.ToDate = $filter("dateFiltering")($scope.NextMonth);
    $scope.filterString = {
        Status: 'All', FromDate: $scope.FromDate, ToDate: $scope.ToDate, ActiveStatus: 'All'
    };

    $scope.GetTaskList = function () {

        if (baseService.isUndefinedOrNull($scope.FromDate)) {
            ShowResult('FromDate is required', 'failure');
            $scope.isValid = false;
            return;
        }
        if (baseService.isUndefinedOrNull($scope.ToDate)) {
            ShowResult('ToDate is required', 'failure');
            $scope.isValid = false;
            return;
        }

        if ($scope.isValid == true) {
            var gridObj = $("#GridEdit").data("ejGrid");
            gridObj.clearFiltering();

            $scope.ModelList = [];
            $http({
                method: 'POST',
                url: $scope.path + 'GetTaskList',
                data: { filterString: $scope.filterString },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    for (var i = 0; i < response.data.length; i++) {
                        try {
                            if (response.data[i].DueDate != null)
                                response.data[i].DueDate = new Date(response.data[i].DueDate);

                        } catch (e) {

                        }

                        try {
                            if (response.data[i].CommitmentDate != null)
                                response.data[i].CommitmentDate = new Date(response.data[i].CommitmentDate);
                        } catch (e) {

                        }
                        try {
                            if (response.data[i].ClosingDate != null)
                                response.data[i].ClosingDate = new Date(response.data[i].ClosingDate);
                        } catch (e) {

                        }

                    }
                    $scope.ModelList = response.data;
                }
            }, function errorCallback(response) {
                ShowResult(response.data.Message, 'failure');
            });
        }
    }

    $scope.downloadgriddataUrl = 'GridReports/Download';
    $scope.exportgriddataUrl = 'GridReports/ExcelExportJson';
    $scope.ExportToExcel = function () {
        var gridObj = $("#GridEdit").ejGrid("instance");
        var data = gridObj.model.dataSource();
        $http({
            method: 'POST',
            url: $scope.exportgriddataUrl,
            data: { 'obj': JSON.stringify(data) }
        }).then(function successCallback(response) {
            if (response.data.Error == true) {

            }
            else {

                location.href = $scope.downloadgriddataUrl + "?FileName=" + response.data.FileName;
            }
        });
    }
    $scope.reportFormat = "Excel";
    $scope.PrintExcel = function () {
        $http({
            method: 'POST',
            url: 'TaskManagement/TNAStatusReports/GetTNAStatusReports',
            data: { reportFormat: $scope.reportFormat, filterString: $scope.filterString },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                $rootScope.report($scope.downloadgriddataUrl + "?FileName=" + response.data.FileName);
            }
        }, function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
        });


    }


}