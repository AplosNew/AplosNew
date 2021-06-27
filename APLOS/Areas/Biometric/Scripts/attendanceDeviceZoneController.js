'use strict';
AttendanceDeviceZoneController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'cboService'];
function AttendanceDeviceZoneController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, cboService) {
    $rootScope.title = "Biometric Zone";
    $scope.Action = 'Save';
    $scope.path = 'Biometric/AttendanceDeviceZone/';
    $scope.saveUrl = $scope.path + 'Save';
    $scope.deleteUrl = $scope.path + 'Delete/';

    $scope.zoneNew = {};//for screen rendering data

    //#region search functionalities
    $scope.GetAllZoneUrl = $scope.path + 'GetAllZone';
    $scope.GetSpecificZoneUrl = $scope.path + 'GetSpecificZone?id=';
    $scope.SearchSpecificZoneurl = $scope.path + 'SearchSpecificZone';

    $scope.searchtext = '';
    $scope.searchfield = '';
    $scope.searchFields = [];
    $scope.searchdata = [];

    $scope.GetAllData = function () {
        $http({
            method: "GET",
            dataType: 'JSON',
            url: $scope.GetAllZoneUrl,
        }).then(function successCallback(response) {
            $scope.searchdata = response.data;
            if (baseService.arrayLength($scope.searchFields) === 0)
                baseService.getDDLSearchColumn(response.data, $scope.searchFields);
        });
    };
    $scope.SearchData = function () {
        $http({
            url: $scope.SearchSpecificZoneurl,
            method: "GET",
            dataType: 'JSON',
            params: {
                column: $scope.searchfield,
                value: $scope.searchtext
            },

        }).then(function successCallback(response) {
            $scope.searchdata = response.data;

        });
    };
    $scope.refreshTemplate = function (args) {
        if (args.rowIndex === 0) {
            $("#headchk").ejCheckBox({ "change": headCheckChange });
        }

        var valobj = $($("#Grid .rowCheckbox")[args.rowIndex]).ejCheckBox()[0];
        var val = $($("#Grid .rowCheckbox")[args.rowIndex]).ejCheckBox()[0].defaultValue;

        $($("#Grid .rowCheckbox")[args.rowIndex]).ejCheckBox({ "change": null });
        var row = $filter('filter')($scope.searchdata, { 'Id': val });
        if (!baseService.isUndefinedOrNull(row) && row.length > 0) {
            if (row[0].Active === true)
                $($("#Grid .rowCheckbox")[args.rowIndex]).ejCheckBox({ "checked": true });
            else
                $($("#Grid .rowCheckbox")[args.rowIndex]).ejCheckBox({ "checked": false });

        }
        $($("#Grid .rowCheckbox")[args.rowIndex]).ejCheckBox({ "change": checkChange });
    }
    $scope.GetAllData();

    $scope.recorddoubleclick = function (args) {
        var gridObj = $("#Grid").data("ejGrid");
        //getting corresponding record             
        $scope.zoneNew = gridObj.getSelectedRecords()[0];
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    }

    //#endregion search functionalities


    $scope.Save = function () {

        $scope.$broadcast('show-errors-check-validity');
        if ($scope.zoneNewForm.$valid) {

            $http({
                method: "POST",
                dataType: 'JSON',
                data: $scope.zoneNew,
                url: $scope.saveUrl,
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.Clear();
                    $scope.GetAllData();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        }

    }
    $scope.Delete = function () {

        $http({
            method: "POST",
            dataType: 'JSON',
            url: $scope.deleteUrl + $scope.zoneNew.Id
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.Clear();
                $scope.GetAllData();
                if ($rootScope.isCollapsed) {
                    $rootScope.toggle();
                }
            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        }

    }
    $scope.Clear = function () {

        $scope.Action = 'Save';
        $scope.zoneNew = {};
    }

}