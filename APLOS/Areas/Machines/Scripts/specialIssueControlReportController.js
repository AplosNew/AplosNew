'use strict';
specialIssueControlReportController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'toaster', 'cboService', '$controller', '$window'];
function specialIssueControlReportController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, toaster, cboService, $controller, $window) {

    $rootScope.title = 'Special Issue Control Report';
    $scope.path = 'Machines/SpecialIssueControlReport/';
    $scope.saveUrl = $scope.path + 'create';
    $scope.saveSummary = $scope.path + 'createSummary';
    $scope.downloadgriddataUrl = 'GridReports/Download';

    var date = new Date(), y = date.getFullYear(), m = date.getMonth();
    var firstDay = new Date(y, m, 1);
    $scope.SpecialIssueRegisters = {
        Shift: null,
        FromDate: $filter('dateFiltering')(firstDay),
        ToDate: $filter('dateFiltering')(new Date(), 'dd-MM-yyyy'),
        ReportFormat: 'Excel'
    };
   
    $scope.ShiftList = [];
    $scope.GetShiftList = function () {
        $http({
            method: 'GET',
            url: 'Machines/SpecialIssueControlReport/GetShiftList'
        }).then(function successCallback(response) {
            $scope.ShiftList = response.data;
        });
    }
    $scope.GetShiftList();

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.IssueControlDetailsList = [];
    $scope.LoadSpecialIssueDetailsList = function () {
        $http({
            method: 'Get',
            url: 'Machines/SpecialIssueControlReport/LoadSpecialIssueDetailsList'
        }).then(function successCallback(response) {
            $scope.IssueControlDetailsList = response.data;
            var gridObj = $("#GridSpecialIssueControlDetails").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
        });
    }

    $scope.IssueControlSummaryList = [];
    $scope.LoadSpecialIssueSummaryList = function () {
        $http({
            method: 'Get',
            url: 'Machines/SpecialIssueControlReport/LoadSpecialIssueSummaryList'
        }).then(function successCallback(response) {
            $scope.IssueControlSummaryList = response.data;
            var gridObj = $("#GridSpecialIssueControlSummary").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
        });
    }

    $scope.IssueItemSummaryList = [];
    $scope.GetIssueItemSummary = function () {
        $http({
            method: 'Get',
            url: 'Machines/SpecialIssueControlReport/LoadIssueItemSummaryList?FromDate=' + $scope.SpecialIssueRegisters.FromDate + '&ToDate=' + $scope.SpecialIssueRegisters.ToDate + '&Shift=' + $scope.SpecialIssueRegisters.Shift
        }).then(function successCallback(response) {
            $scope.IssueItemSummaryList = response.data;
        }
        )
    }

    $scope.IssueItemDetailsList = [];
    $scope.GetIssueItemDetails = function () {
        $http({
            method: 'Get',
            url: 'Machines/SpecialIssueControlReport/LoadIssueItemDetailsList?FromDate=' + $scope.SpecialIssueRegisters.FromDate + '&ToDate=' + $scope.SpecialIssueRegisters.ToDate + '&Shift=' + $scope.SpecialIssueRegisters.Shift
        }).then(function successCallback(response) {
            $scope.IssueItemDetailsList = response.data;
            $scope.GetIssueItemSummary();
            $scope.Save();
        }
        )
    }

    $scope.Save = function () {
        try {

            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: {
                    "DataList": $scope.IssueItemDetailsList
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    $scope.SaveSummary();
                    ShowResult(response.data.Message, 'success');
                    $scope.LoadSpecialIssueDetailsList();
                    $scope.LoadSpecialIssueSummaryList();
                    $scope.Action = 'Save';
                }

            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };
        } catch (ex) {
            ShowResult(ex, 'Info');
        }
    };
    $scope.SaveSummary = function () {
        try {

            $http({
                method: 'POST',
                url: $scope.saveSummary,
                data: {
                    "DataList": $scope.IssueItemSummaryList
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.Action = 'Save';
                }

            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };
        } catch (ex) {
            ShowResult(ex, 'Info');
        }
    };

    $scope.SpecialIssueControlDetailsReport = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'XlsSpecialIssueControlDetails?FromDate=' + $scope.SpecialIssueRegisters.FromDate + '&ToDate=' + $scope.SpecialIssueRegisters.ToDate + '&Shift=' + $scope.SpecialIssueRegisters.Shift,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {

                $rootScope.report($scope.downloadgriddataUrl + "?FileName=" + response.data.FileName);
            }
        }, function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
        });

    };

    $scope.SpecialIssueControlSummaryReport = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'XlsSpecialIssueControlSummary?FromDate=' + $scope.SpecialIssueRegisters.FromDate + '&ToDate=' + $scope.SpecialIssueRegisters.ToDate + '&Shift=' + $scope.SpecialIssueRegisters.Shift,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {

                $rootScope.report($scope.downloadgriddataUrl + "?FileName=" + response.data.FileName);
            }
        }, function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
        });

    };
}