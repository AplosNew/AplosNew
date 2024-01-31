'use strict';
GoodWorkPaymentDisburseController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', "$controller"];
function GoodWorkPaymentDisburseController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $controller) {
    $rootScope.title = 'Good Work Payment Disburse';
    $rootScope.titleTab1 = 'Undisburse Data';
    $rootScope.titleTab2 = 'Disburse Data';

    $scope.path = 'Attendances/GoodWork/';
    baseService.init($scope.getListUrl);
    $scope.Action = 'Save';
    $controller("employeeBaseController", { $scope: $scope, $http: $http });
    $scope.downloadgriddataUrlPath = 'GridReports/DownloadUsingFullPath';

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };

    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.PCEmployeeList = [];
    $scope.PCOTEmployeeUndisburseList = [];
    $scope.GetLoadEmployeeInformation = function () {
        $scope.TabName = 'PaymentDisburse';
        if ($scope.ToDate === "" || $scope.ToDate === null || $scope.ToDate === undefined) {
            ShowResult('Select To Date', 'failure');
            return false;
        }
        if ($scope.FromDate === "" || $scope.FromDate === null || $scope.FromDate === undefined) {
            ShowResult('Select From Date', 'failure');
            return false;
        }
        $http({
            method: 'POST',
            url: $scope.path + "LoadPCEmployeelist",
            data: { 'fromDate': $scope.FromDate, 'toDate': $scope.ToDate, 'tabName': $scope.TabName },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.PCOTEmployeeUndisburseList = response.data;
            $scope.GetGoodWorkPaymentDisburseOTAdvisedetail();
        });
    }

    $scope.refreshTemplateemployee = function (args) {
        $("#headchk").ejCheckBox({ "change": CheckBoxSelectAllEmolyeeWise });
    };

    function CheckBoxSelectAllEmolyeeWise(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        var filtered = $("#GridChildEdit").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.PCOTEmployeeUndisburseList.length; i++) {
                $scope.PCOTEmployeeUndisburseList[i].CheckBoxSelect = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].CheckBoxSelect = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridChildEdit").data("ejGrid");
        gridObj.refreshContent();
    };

    var getString = function (data) {
        var string = "''";
        var collection = [];
        for (var i = 0; i < data.length; i++) {
            if (collection.includes(data[i]) == false) {
                string += ",'" + data[i] + "'";
                collection.push(data[i]);
            }
        }
        return string;
    }

    $scope.GoodWorkPaymentDisburseSave = function () {
        try {
            $scope.NewDisburseIds = [];
            for (var i = 0; i < $scope.PCOTEmployeeUndisburseList.length; i++) {
                if ($scope.PCOTEmployeeUndisburseList[i].isSelected == true) {
                    $scope.NewDisburseIds.push($scope.PCOTEmployeeUndisburseList[i].EmpSystemId);
                }
            }
            var disburseIds = getString($scope.NewDisburseIds);

            $http({
                method: 'POST',
                url: $scope.path + 'CreateGoodWorkPaymentDisburse',
                data: { 'Id': disburseIds },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.GetGoodWorkPaymentUnDisburseOTAdvisedetail();
                    $scope.GetGoodWorkPaymentDisburseOTAdvisedetail();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.GetGoodWorkPaymentUnDisburseOTAdvisedetail = function () {
        $http({
            method: 'Get',
            url: $scope.path + "GetGoodWorkPaymentAdviseUnDisburseOTDetailList",
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.PCOTEmployeeUndisburseList = response.data;
        });
    }

    $scope.PCOTEmployeedisburseList = [];
    $scope.GetGoodWorkPaymentDisburseOTAdvisedetail = function () {
        if ($scope.ToDate === "" || $scope.ToDate === null || $scope.ToDate === undefined) {
            ShowResult('Select To Date', 'failure');
            return false;
        }
        if ($scope.FromDate === "" || $scope.FromDate === null || $scope.FromDate === undefined) {
            ShowResult('Select From Date', 'failure');
            return false;
        }
        $scope.FD = $filter('dateFiltering')(new Date($scope.FromDate), 'dd-MM-yyyy');
        $scope.TD = $filter('dateFiltering')(new Date($scope.ToDate), 'dd-MM-yyyy');
        $http({
            method: 'Get',
            url: $scope.path + 'GetGoodWorkPaymentAdviseDisburseOTDetailList?fromDate=' + $scope.FD + '&toDate=' + $scope.TD,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.PCOTEmployeedisburseList = response.data;
        });
    }

    $scope.GWPaymentUndisburseReport = function () {
        var dataList = [];
        var g = $("#GridChildEdit").data("ejGrid");
        dataList = g.getFilteredRecords();
        if (dataList.length == 0) {
            dataList = $scope.PCOTEmployeeUndisburseList;
        }
        $scope.fileName = "Good Work Payment Undisburse Report.xlsx";

        $http({
            method: 'POST',
            url: $scope.path + "GetGoodWorkPaymentUndisburseReport",
            data: { 'data': dataList, 'reportFileName': $scope.fileName },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                $rootScope.report($scope.downloadgriddataUrlPath + "?FullPath=" + response.data.FileName + "&fileName=" + $scope.fileName);
            }
        }, function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
        });
    }

    $scope.GWPaymentDisburseReport = function () {
        var dataList = [];
        var g = $("#GridDisburse").data("ejGrid");
        dataList = g.getFilteredRecords();
        if (dataList.length == 0) {
            dataList = $scope.PCOTEmployeedisburseList;
        }
        $scope.fileName = "Good Work Payment Disburse Report.xlsx";

        $http({
            method: 'POST',
            url: $scope.path + "PCOTEmployeeDisburseList",
            data: { 'data': dataList, 'reportFileName': $scope.fileName },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                $rootScope.report($scope.downloadgriddataUrlPath + "?FullPath=" + response.data.FileName + "&fileName=" + $scope.fileName);
            }
        }, function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
        });
    }

}