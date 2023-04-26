'use strict';
BudgetCodeWiseHRReportController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function BudgetCodeWiseHRReportController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'HR Report Master';
    $scope.Action = 'Save';

    $scope.ModelList = [];
    $scope.path = 'HumanResource/BudgetCodeWiseHRReport/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'GetSequence';
    $scope.saveUrl = $scope.path + 'Save';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl);

    $scope.HRReportMasterList = [];
    $scope.GetHRReportMasterList = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetHRReportMasterList",
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.HRReportMasterList = response.data;            
        });
    }
    $scope.GetHRReportMasterList();

    $scope.BudgetList = [];
    $scope.GetDataOnFavouriteFilter = function () {
        // $scope.CheckedEntity = [];
        var DropDownEntityListObj = $("#favourite").data("ejDropDownList");
        var favouriteId = DropDownEntityListObj.getSelectedValue();

        if (angular.isUndefinedOrNull(favouriteId)) {
            for (var i = 0; i < DropDownEntityListObj.popupListItems.length; i++) {
                if (angular.isUndefinedOrNull(favouriteId)) {
                    favouriteId = + DropDownEntityListObj.popupListItems[i].Id;
                } else {
                    favouriteId += ',' + DropDownEntityListObj.popupListItems[i].Id;
                }
            }
        }

        $http({
            method: 'POST',
            url: $scope.path + "GetDataOnFavouriteFilter",
            data: { 'filterId': favouriteId },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.BudgetList = response.data;
           
        });
    }

    $scope.popupToExportExcel = function () {

        angular.element(document.querySelector('#popupToExportExcel')).modal('show');
    }

    $scope.closepopupToExportExcel = function () {

        angular.element(document.querySelector('#popupToExportExcel')).modal('hide');
    }

    $scope.EmpCategoryList = [];
    $scope.GetEmployeeCategory = function () {
        $http({
            method: 'GET',
            url: 'humanresource/DailyAttendanceStatusReport/GetEmployeeCategory',
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.EmpCategoryList = resp.data;
        });
    }
    $scope.GetEmployeeCategory();

    $scope.ProcessList = [];
    $scope.GetProcess = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'GetProcess',
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.ProcessList = resp.data;
        });
    }
    $scope.GetProcess();

    $scope.EntityList = [];
    $scope.GetEntity = function () {
        $http({
            method: 'POST',
            url: 'HumanResource/HRReportMaster/GetEntity',
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.EntityList = response.data;

        });
    }
    $scope.GetEntity();

    $scope.DayStatusList = [
        {
            Text: "A",
            Value: "A"
        },
        {
            Text: "AH",
            Value: "AH"
        },
        {
            Text: "AW",
            Value: "AW"
        },
        {
            Text: "CH",
            Value: "CH"
        },
        {
            Text: "CL",
            Value: "CL"
        },
        {
            Text: "CW",
            Value: "CW"
        },
        {
            Text: "EM",
            Value: "EM"
        },
        {
            Text: "H",
            Value: "H"
        },
        {
            Text: "HDCL",
            Value: "HDCL"
        },
        {
            Text: "HDP",
            Value: "HDP"
        },
        {
            Text: "HDPL",
            Value: "HDPL"
        },
        {
            Text: "HDSL",
            Value: "HDSL"
        },
        {
            Text: "HL",
            Value: "HL"
        },
        {
            Text: "HP",
            Value: "HP"
        },
        {
            Text: "L",
            Value: "L"
        },
        {
            Text: "LWP",
            Value: "LWP"
        },
        {
            Text: "ML",
            Value: "ML"
        },
        {
            Text: "OD",
            Value: "OD"
        },
        {
            Text: "P",
            Value: "P"
        },
        {
            Text: "PL",
            Value: "PL"
        },
        {
            Text: "PW",
            Value: "PW"
        },
        {
            Text: "SL",
            Value: "SL"
        },
        {
            Text: "W",
            Value: "W"
        },
        {
            Text: "WAH",
            Value: "WAH"
        },
        {
            Text: "WAW",
            Value: "WAW"
        },
        {
            Text: "WP",
            Value: "WP"
        },
    ]

    $scope.POWiseReportExcel = function () {
        var dataList = [];
        var g = $("#GridPoWise").data("ejGrid");
        dataList = g.getFilteredRecords();

        if (dataList.length == 0) {
            dataList = $scope.POWiseList;
        }
        $scope.fileName = 'HRReportMaaster.xlsx';

        $http({
            method: 'POST',
            url: $scope.path + "HRReportMasterDataReport",
            data: {
                'data': dataList,
                'reportFileName': $scope.fileName
            },
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
    }

};