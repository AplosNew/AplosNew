'use strict';
DetentionLogReportController.$inject = ["cboService", "commonMessage", "$scope", "$rootScope", "baseService", "$routeParams", "$location", "$http", "$filter", "$window"];
function DetentionLogReportController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window) {
    $rootScope.title = "Detention Logout Report";
    $scope.Action = 'Save';
    $scope.path = 'Materials/DetentionLogReport/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getUrl = $scope.path + 'get';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.getStorage = $scope.path + 'StorageSql';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'Delete';
    //$scope.downloadgriddataUrl = 'GridReports/Download';

    $scope.downloadgriddataUrlPath = 'GridReports/DownloadUsingFullPath';//DownloadUsingPath

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;

    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.DepartmentList = [];
    $scope.GetDepartment = function () {
        $http.get('Materials/DetentionLogReport/GetDepartment')
            .then(
                function successCallback(response) {
                    $scope.DepartmentList = response.data;

                }
            )
    }
    $scope.GetDepartment();

    $scope.DetentionTypeList = [];
    $scope.getDetentionType = function () {
        $http({
            method: 'POST',
            url: 'Materials/DetentionLog/getDetentionTypeListByDepartment'
        }).then(function successCallback(response) {
            $scope.DetentionTypeList = response.data;
            $scope.GetDepartment();
        });
    }
    $scope.getDetentionType();

    // #region Reports
    $scope.ModalTempPendingDetention = {
        From: null,
        To: null,
        DepartmentId: null,
        DetentionTypeId: null
    };
    $scope.ModalNewPendingDetention = Object.assign({}, $scope.ModalTempPendingDetention);

    $scope.ModalTempClosedDetention = {
        From: null,
        To: null,
        DepartmentId: null,
        DetentionTypeId: null
    };
    $scope.ModalNewClosedDetention = Object.assign({}, $scope.ModalTempClosedDetention);

    $scope.ClosedDetentionList = [];
    $scope.GetClosedDetentionGridReport = function () {
        $http.get('Materials/DetentionLogout/GetClosedDetentionGridReport?from=' + $scope.ModalNewClosedDetention.From + '&to=' + $scope.ModalNewClosedDetention.To + '&departmentId=' + $scope.ModalNewClosedDetention.DepartmentId + '&detentiontypeId=' + $scope.ModalNewClosedDetention.DetentionTypeId)
            .then(function successCallback(response) {
                $scope.ClosedDetentionList = response.data;
            })
    }

    $scope.PendingDetentionList = [];
    $scope.GetPendingDetentionGridView = function () {
        $http.get('Materials/DetentionLogout/GetPendingDetentionGridView?from=' + $scope.ModalNewPendingDetention.From + '&to=' + $scope.ModalNewPendingDetention.To + '&departmentId=' + $scope.ModalNewPendingDetention.DepartmentId + '&detentiontypeId=' + $scope.ModalNewPendingDetention.DetentionTypeId)
            .then(function successCallback(response) {
                $scope.PendingDetentionList = response.data;
            })
    }

    $scope.fileName = "ClosedDetentionReport.xlsx";
    $scope.XlsGetClosedDetentionReport = function () {

        //$http.get('Materials/DetentionLogout/XlsGetClosedDetentionReport?from=' + $scope.ModalNewClosedDetention.From + '&to=' + $scope.ModalNewClosedDetention.To + '&departmentId=' + $scope.ModalNewClosedDetention.DepartmentId + '&detentiontypeId=' + $scope.ModalNewClosedDetention.DetentionTypeId)
        $http({
            method: 'POST',
            url: 'Materials/DetentionLogout/XlsGetClosedDetentionReport?parameters=' + $scope.ModalNewClosedDetention.From + '&to=' + $scope.ModalNewClosedDetention.To + '&departmentId=' + $scope.ModalNewClosedDetention.DepartmentId + '&detentiontypeId=' + $scope.ModalNewClosedDetention.DetentionTypeId,
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

    $scope.fileName = "PendingDetentionReport.xlsx";
    $scope.XlsGetPendingDetentionView = function () {

        $http.get('Materials/DetentionLogout/XlsGetPendingDetentionView?from=' + $scope.ModalNewPendingDetention.From + '&to=' + $scope.ModalNewPendingDetention.To + '&departmentId=' + $scope.ModalNewPendingDetention.DepartmentId + '&detentiontypeId=' + $scope.ModalNewPendingDetention.DetentionTypeId)
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
    // #endregion Reports
}
