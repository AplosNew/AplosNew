'use strict';
GeneralContractReportController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$window'];
function GeneralContractReportController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window) {
    $rootScope.title = 'Contract Report';
    $scope.ModelList = [];
    $scope.path = 'Administration/GeneralContractReport/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.Action = 'Save';
    $scope.saveUrl = $scope.path + 'Save';
    $scope.updateUrl = $scope.path + 'Update';
    $scope.deleteUrl = 'Administration/GeneralContractItemMaster/Delete'
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

    var CurrentDate = new Date();
    $scope.ModelTemp = {
        FromDate: null,
        ToDate: null,
        GeneralContractId: null,
        EntityId:null
    };
    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);

    $scope.ContractList = [];
    $scope.GetContractName = function () {
        $http.get('Administration/GeneralContractEntry/GetContract')
            .then(function successCallback(response) {
                $scope.ContractList = response.data;
            })
    }
    $scope.GetContractName();

    $scope.EntityList = [];
    $scope.GetEntity = function () {
        $http.get('Administration/GeneralContractEntry/GetEntity')
            .then(function successCallback(response) {
                $scope.EntityList = response.data;
            })
    }
    $scope.GetEntity();

    $scope.FilteredTransactionList = []
    $scope.GetAllTransactionData = function () {
        $http.get('Administration/GeneralContractReport/GetAllTransactionData?from=' + $scope.ModelNew.FromDate + '&to=' + $scope.ModelNew.ToDate + '&contractid=' + $scope.ModelNew.GeneralContractId + '&entityid=' + $scope.ModelNew.EntityId)
            .then(function successCallback(response) {
                $scope.FilteredTransactionList = response.data;
            })
    }

    $scope.SummaryList = [];
    $scope.GetSummaryData = function () {
        $http.get('Administration/GeneralContractReport/GetSummaryData?from=' + $scope.ModelNew.FromDate + '&to=' + $scope.ModelNew.ToDate + '&contractid=' + $scope.ModelNew.GeneralContractId + '&entityid=' + $scope.ModelNew.EntityId)
            .then(function successCallback(response) {
                $scope.SummaryList = response.data;
            })
    }

    $scope.fileName = "Contract Transaction.xlsx";
    $scope.XlsDownloadContractTransactionReport = function () {

        //$http.get('Materials/DetentionLogout/XlsGetClosedDetentionReport?from=' + $scope.ModalNewClosedDetention.From + '&to=' + $scope.ModalNewClosedDetention.To + '&departmentId=' + $scope.ModalNewClosedDetention.DepartmentId + '&detentiontypeId=' + $scope.ModalNewClosedDetention.DetentionTypeId)
        $http({
            method: 'POST',
            url: 'Administration/GeneralContractReport/XlsDownloadContractTransactionReport?from=' + $scope.ModelNew.FromDate + '&to=' + $scope.ModelNew.ToDate + '&contractid=' + $scope.ModelNew.GeneralContractId + '&entityid=' + $scope.ModelNew.EntityId,
            dataType: 'JSON',
        })
            .then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {

                    //$rootScope.report($scope.downloadgriddataUrl + "?FileName=" + response.data.FileName);

                    $window.open($scope.downloadgriddataUrlPath + "?FullPath=" + response.data.FileName + "&fileName=" + $scope.fileName);
                }
            }, function errorCallback(response) {
                ShowResult(response.data.Message, 'failure');
            });

    };

    $scope.summaryfileName = "Contract Transaction Summary.xlsx"
    $scope.XlsDownloadSummaryReport = function () {

        //$http.get('Materials/DetentionLogout/XlsGetClosedDetentionReport?from=' + $scope.ModalNewClosedDetention.From + '&to=' + $scope.ModalNewClosedDetention.To + '&departmentId=' + $scope.ModalNewClosedDetention.DepartmentId + '&detentiontypeId=' + $scope.ModalNewClosedDetention.DetentionTypeId)
        $http({
            method: 'POST',
            url: 'Administration/GeneralContractReport/XlsDownloadSummaryReport?from=' + $scope.ModelNew.FromDate + '&to=' + $scope.ModelNew.ToDate + '&contractid=' + $scope.ModelNew.GeneralContractId + '&entityid=' + $scope.ModelNew.EntityId,
            dataType: 'JSON',
        })
            .then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {

                    //$rootScope.report($scope.downloadgriddataUrl + "?FileName=" + response.data.FileName);

                    $window.open($scope.downloadgriddataUrlPath + "?FullPath=" + response.data.FileName + "&fileName=" + $scope.summaryfileName);
                }
            }, function errorCallback(response) {
                ShowResult(response.data.Message, 'failure');
            });

    };
}