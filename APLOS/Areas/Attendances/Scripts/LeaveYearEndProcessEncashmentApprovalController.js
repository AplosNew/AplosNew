'use strict';
LeaveYearEndProcessEncashmentApprovalController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$window', '$filter'];
function LeaveYearEndProcessEncashmentApprovalController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $window, $filter) {
    $rootScope.title = 'Leave Encanshment Approve';
    $scope.path = 'Attendances/LeaveYearEndProcess/';
    $scope.GetEncashmentForEditurl = $scope.path + 'GetEncashmentForEdit';
    $scope.ApproveYearlyEncashenturl = $scope.path + 'ApproveYearlyEncashent';
    $scope.UnApproveYearlyEncashenturl = $scope.path + 'UnApproveYearlyEncashent';
    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    }

    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };
    $scope.Date = ($filter('dateFiltering')(new Date(), 'dd-MM-yyyy'));


    $scope.PendingCheckboxAll = function (args) {
        $("#headchkPending").ejCheckBox({ "change": CheckBoxSelectAllPending });
    };
    function CheckBoxSelectAllPending(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        var filtered = $("#GridEncashedPendingData").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            filtered = $scope.EncashedPendingData;
        }

        for (var i = 0; i < filtered.length; i++) {
            filtered[i]["Checked"] = ChkOrUnchk;
        }

        var gridObj = $("#GridEncashedPendingData").data("ejGrid");
        gridObj.refreshContent();
    };

    $scope.SavedCheckboxAll = function (args) {
        $("#headchkSaved").ejCheckBox({ "change": CheckBoxSelectAllSaved });
    };
    function CheckBoxSelectAllSaved(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        var filtered = $("#GridEncashedSavedData").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            filtered = $scope.EncashedSavedData;
        }

        for (var i = 0; i < filtered.length; i++) {
            filtered[i]["Checked"] = ChkOrUnchk;
        }

        var gridObj = $("#GridEncashedSavedData").data("ejGrid");
        gridObj.refreshContent();
    };


    $scope.YearlyCalendarId = null;
    $scope.EncashedSavedData = [];
    $scope.EncashedPendingData = [];

    $scope.GetEncashmentForEdit = function () {
        try {


            $http.get($scope.GetEncashmentForEditurl + '?Date=' + $scope.Date)
                .then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {

                        $scope.EncashedSavedData = response.data.EncashedData;
                        $scope.EncashedPendingData = response.data.EncashedPendingData;
                    }
                },

                    function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    });

        } catch (e) {
            ShowResult(e, "failure");
        }
    };


    $scope.ApproveYearlyEncashent = function () {

        var _data = ej.DataManager($scope.EncashedPendingData).executeLocal(ej.Query().where("Checked", "equal", true));
        $http({
            method: "POST",
            dataType: 'JSON',
            url: $scope.ApproveYearlyEncashenturl,
            data: { data: _data }
        }).then(function successCallback(response) {
            if (response.data.Error == false) {
                $scope.GetEncashmentForEdit();
            }
            else {

                ShowResult(response.data.Message, 'failure');

            }
        });
    }
    $scope.UnApproveYearlyEncashent = function () {

        var _data = ej.DataManager($scope.EncashedSavedData).executeLocal(ej.Query().where("Checked", "equal", true));
        $http({
            method: "POST",
            dataType: 'JSON',
            url: $scope.UnApproveYearlyEncashenturl,
            data: { data: _data }
        }).then(function successCallback(response) {
            if (response.data.Error == false) {
                $scope.GetEncashmentForEdit();
            }
            else {

                ShowResult(response.data.Message, 'failure');

            }
        });
    }
}