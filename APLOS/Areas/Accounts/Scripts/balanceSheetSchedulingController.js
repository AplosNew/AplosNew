'use strict';
balanceSheetSchedulingController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function balanceSheetSchedulingController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {

    // #region TAB CHANGE
    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };

    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };
        // #endregion TAB CHANGE

    //  #region BalanceSheetScheduling
    $scope.ActionBalanceSheetScheduling = 'Save';
    $scope.indexBalanceSheetScheduling = -1;
    $scope.balanceSheetSchedulings = [];
    $scope.pathBalanceSheetScheduling = 'accounts/BalanceSheetScheduling/';
    $scope.getListUrlBalanceSheetScheduling = $scope.pathBalanceSheetScheduling + 'GetList';
    $scope.saveUrlBalanceSheetScheduling = $scope.pathBalanceSheetScheduling + 'create';
    $scope.deleteUrlBalanceSheetScheduling = $scope.pathBalanceSheetScheduling + 'delete/';
    baseService.init($scope.getListUrlBalanceSheetScheduling);

    $scope.searchBy = "OptionNo"; $scope.search = "";
    $scope.searchByList = [{ value: 'Id', name: "Id" }, { value: 'OptionNo', name: "OptionNo" }, { value: 'Type', name: "Type" }, { value: 'Group', name: "Group" }, { value: 'SubGroup', name: "Sub Group" }, { value: 'UserGroup', name: "User Group" }, { value: 'UserSubGroup', name: "User Sub Group" }];

    $scope.getDataBalanceSheetScheduling = function () {
        $http({
            method: 'POST',
            url: $scope.pathBalanceSheetScheduling + "GetList",
            data: {},
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.balanceSheetSchedulings = response.data;
        });
    }
    $scope.getDataBalanceSheetScheduling();

    $scope.balanceSheetScheduling = {
        Id: null,
        OptionNo: null,
        Type: null,
        Group: null,
        SubGroup: null,
        UserGroup: null,
        UserSubGroup: null,
        Item: null,
        ScheduleNo: null,
        ScheduleName: null,
        UserItem: null,
        UserScheduleName: null,
        AddedBy: null,
        AddedDate: new Date(),
        AddedFromIP: null,
        UpdatedDate: null
    };
    $scope.Get = function (args) {
        $scope.balanceSheetScheduling = Object.assign({}, args.data);
        $scope.ActionBalanceSheetScheduling = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.SaveBalanceSheetScheduling = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.balanceSheetSchedulingForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.saveUrlBalanceSheetScheduling,
                data: { 'data': $scope.balanceSheetScheduling },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.ClearFieldsBalanceSheetScheduling();
                    $scope.getDataBalanceSheetScheduling();

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        }
       
    };

    $scope.DeleteBalanceSheetScheduling = function () {
        if (!baseService.isUndefinedOrNull($scope.balanceSheetScheduling.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrlBalanceSheetScheduling + $scope.balanceSheetScheduling.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.ClearFieldsBalanceSheetScheduling();
                    $scope.getDataBalanceSheetScheduling();
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, 'failure');
            });
        }
        else {
            ShowResult(commonMessage.primaryKeyNullMessage, 'failure');
        }
        return true;
    };
    $scope.ClearFieldsBalanceSheetScheduling = function () {
        $scope.ActionBalanceSheetScheduling = 'Save';
        $scope.balanceSheetScheduling = {
            Id: null,
            OptionNo: null,
            Type: null,
            Group: null,
            SubGroup: null,
            UserGroup: null,
            UserSubGroup: null,
            Item: null,
            ScheduleNo: null,
            ScheduleName: null,
            UserItem: null,
            UserScheduleName: null,
            AddedBy: null,
            AddedDate: new Date(),
            AddedFromIP: null,
            UpdatedDate: null
        };
    }
    $scope.message_Detailconfirmation = null;
    $scope.RemoveBalanceSheetScheduling = function () {
        if (!baseService.isUndefinedOrNull($scope.balanceSheetScheduling.Id))
            $scope.message_Detailconfirmation = 'Are you sure want to delete permanently';
        angular.element(document.querySelector('#confirmDetailPopUpLevel1')).modal('show');
    }
    //  #endregion BalanceSheetScheduling
}