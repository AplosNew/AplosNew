'use strict';
SalesChanlanCheckedController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$window', '$controller', '$route'];
function SalesChanlanCheckedController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window, $controller, $route) {
    $rootScope.title = 'Sales Chalan Check & Approve';
    $scope.ModelList = [];
    $scope.path = 'SalesManagements/SalesChalan/';
    $scope.downloadgriddataUrlPath = 'GridReports/DownloadUsingFullPath';


    // #region TAB CHANGE
    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };

    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    // #endregion TAB CHANGE

    $scope.GriddataSCCUnCheckedList = [];
    $scope.GetUncheckedData = function () {
        $http.get('SalesManagements/SalesChalan/GetUncheckedData')
            .then(function successCallback(response) {
                $scope.GriddataSCCUnCheckedList = response.data;
            })
    }
    $scope.GetUncheckedData();

    $scope.GriddataSCCCheckedList = [];
    $scope.GetcheckedData = function () {
        $http.get('SalesManagements/SalesChalan/GetcheckedData')
            .then(function successCallback(response) {
                $scope.GriddataSCCCheckedList = response.data;
            })
    }
    $scope.GetcheckedData();

    // Employee Who Responsible For Approving
    $scope.ApproveByList = [];
    $scope.GetSalesChalanApproveByCboList = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'GetSalesChalanApproveByCboList'
        }).then(function successCallback(response) {
            $scope.ApproveByList = response.data;
        });
    }
    $scope.GetSalesChalanApproveByCboList();

    $scope.POApprovalList = [
        {
            'Text': 'Checked',
            'Value': 'Checked'
        }
    ];

    $scope.btndisable = false;
    $scope.SaveCheckData = function (args) {
        try {
            if (baseService.isUndefinedOrNull(args.data.ApproveById)) {
                throw "Select Approve By Person.";
            }
            if (baseService.isUndefinedOrNull(args.data.CheckedStatus)) {
                throw "Select Checked Status.";
            }
            $scope.btndisable = true;
            $http({
                method: 'POST',
                url: 'SalesManagements/SalesChalan/CreateCheckBy',
                data: { 'data': args.data },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    $scope.btndisable = false;
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.btndisable = false;
                    $scope.GetcheckedData();
                    $scope.GetUncheckedData();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };


    $scope.approvalStatusList = [
        {
            'Text': 'Approved',
            'Value': 'Approved'
        }
    ];

    $scope.SaveApproveData = function (args) {
        try {
            if (baseService.isUndefinedOrNull(args.data.ApprovedStatus)) {
                throw "Select Approved Status.";
            }
            $scope.btndisable = true;
            $http({
                method: 'POST',
                url: 'SalesManagements/SalesChalan/CreateApproveBy',
                data: { 'data': args.data },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    $scope.btndisable = false;
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.btndisable = false;
                    $scope.GetcheckedData();
                    $scope.GetApproveBycheckedData();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.GriddataApproveList = [];
    $scope.GetApproveBycheckedData = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'GetApproveBycheckedData'
        }).then(function successCallback(response) {
            $scope.GriddataApproveList = response.data;
        });
    }
    $scope.GetApproveBycheckedData();
}