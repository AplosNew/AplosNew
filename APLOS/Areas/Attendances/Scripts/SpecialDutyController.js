'use strict';
SpecialDutyController.$inject = ['$window','cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function SpecialDutyController($window,cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Special Duty';
    $scope.path = 'Attendances/SpecialDuty/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };

    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.ModelTemp = {
               WorkDate: $filter('dateFiltering')(new Date(), 'dd-M-yyyy')
    };
    $scope.OTManual = Object.assign({}, $scope.ModelTemp);

    $scope.sdApproveList = [];
    $scope.GetSDData = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.OTManual.WorkDate)) {
                throw "Select Work Date.";
            }
            $http({
                method: "GET",
                dataType: 'JSON',
                url: 'Attendances/SpecialDuty/GetList?workDate=' + $scope.OTManual.WorkDate
            }).then(function successCallback(response) {
                $scope.sdApproveList = response.data;
                $scope.GetSDApprovedData();
            });
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.sdApprovedList = [];
    $scope.GetSDApprovedData = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.OTManual.WorkDate)) {
                throw "Select Work Date.";
            }
            $http({
                method: "GET",
                dataType: 'JSON',
                url: 'Attendances/SpecialDuty/GetSDApprovedData?workDate=' + $scope.OTManual.WorkDate
            }).then(function successCallback(response) {
                $scope.sdApprovedList = response.data;

            });
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.refreshTemplateemployee4 = function (args) {
        $("#headchk4").ejCheckBox({ "change": CheckBoxSelectAllEmolyeeWise });
    };

    function CheckBoxSelectAllEmolyeeWise(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        var filtered = $("#GridEmployeeInfoList").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.sdApproveList.length; i++) {
                $scope.sdApproveList[i].IsApproved = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].IsApproved = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridEmployeeInfoList").data("ejGrid");
        gridObj.refreshContent();
    };


    $scope.downloadgriddataUrlPath = 'GridReports/DownloadUsingFullPath';
    $scope.DownloadOTData = function (name) {
        try {
            var dataList = [];
            if (name == 'pending') {
                var g = $("#GridEmployeeInfoList").data("ejGrid");
                dataList = g.getFilteredRecords();

                if (dataList.length == 0) {
                    dataList = $scope.sdApproveList;
                }
            }
            else {
                var g = $("#GridOA").data("ejGrid");
                dataList = g.getFilteredRecords();

                if (dataList.length == 0) {
                    dataList = $scope.otApprovedList;
                }
            }
            $scope.fileName = "OTDataReport.xlsx";

            $http({
                method: 'POST',
                url: "HumanResource/OTConfirmationProcess/GetOTDataXls",
                data: { 'data': dataList, 'reportFileName': $scope.fileName },
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

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.ModelNewForm.$valid) {

            try {


                var dataList = [];
                var tosavedataList = [];
                var g = $("#GridEmployeeInfoList").data("ejGrid");
                dataList = g.getFilteredRecords();

                if (dataList.length == 0) {
                    dataList = $scope.sdApproveList;
                }


                for (var i = 0; i < dataList.length; i++) {
                    if (dataList[i].IsApproved == true) {
                        tosavedataList.push(dataList[i]);
                    }
                }
                if (tosavedataList.length == 0) {
                    throw 'Select Employee.';
                }

                $http({
                    method: 'POST',
                    data: { data: tosavedataList, 'workDate': $scope.OTManual.WorkDate},
                    url: 'Attendances/SpecialDuty/SaveSDData'

                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, "failure");
                    }
                    else {
                        ShowResult(response.data.Message, "success");
                        $scope.GetSDData();
                        ClearFields();
                    }
                });
            }
            catch (e) {
                ShowResult(e, "failure");
            }

        }
    }


    function ClearFields() {
        $scope.OTManual = Object.assign({}, $scope.ModelTemp);
        $scope.sdApproveList = [];
    }
}