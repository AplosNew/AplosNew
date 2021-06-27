'use strict';
BonusProcessController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function BonusProcessController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Bonus Process';
    $scope.Action = 'Save';
    $scope.ModelList = [];
    $scope.path = 'Payrolls/BonusProcess/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';
    baseService.init($scope.getListUrl);
    $scope.downloadgriddataUrlPath = 'GridReports/DownloadUsingFullPath';//DownloadUsingPath

    //#region model
    $scope.BonusProcess = {
        SystemID: null,
        Work: null,
        DSalaryHead: null,
        BPolicy: null,
        Remark: null,
        BType: null,
        Rate: 1,
    }
    //#endregion

    //#region Get List
    $scope.SalaryHeadList = [];
    $scope.getHead = function () {
        $http({
            method: 'GET',
            url: $scope.path + "GetHead",
        }).then(function successCallback(response) {
            $scope.SalaryHeadList = response.data;
        });
    }
    $scope.getHead();

    $scope.BonusList = [];
    $scope.getBonus = function () {
        $http({
            method: 'GET',
            url: $scope.path + "GetBonus",
        }).then(function successCallback(response) {
            $scope.BonusList = response.data;
        });
    }
    $scope.getBonus();
    //#endregion

    $scope.Process = function () {
        try {
            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: { 'process': $scope.BonusEmpList, 'MasterID': $scope.BonusProcess.BPolicy, 'Bonus': $scope.BonusProcess },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    //#region -- Modal --
    $scope.MasterBonusList = [];
    $scope.BonusEmpList = [];
    $scope.ShowDiv = false;
    $scope.AddLineIdem = function () {
        try {
            $scope.ShowDiv = true;
            var eDialog = $("#BPMRDetails").data("ejDialog");
            eDialog.open();
            $scope.LoadGrid($scope.BonusProcess.BPolicy);
        } catch (e) {
            ShowResult(e, "failure");
        }

    };
    //#endregion

    //#region -- Get Bonus --
    $scope.MasterId = null;
    $scope.GetBonus = function () {
        $scope.MasterId = $scope.BonusProcess.BPolicy;
        $scope.LoadGrid($scope.MasterId);
        $scope.GetBonusEmp($scope.MasterId, $scope.BonusProcess.Work);
        $scope.GetCurrency();
    }

    $scope.LoadGrid = function (MasterId) {
        $http({
            method: 'GET',
            url: $scope.path + 'GetDetails?MasterId=' + MasterId,
        }).then(function successCallback(response) {
            $scope.MasterBonusList = response.data;
        });
    }

    $scope.GetBonusEmp = function (MasterId, Date) {
        $http({
            method: 'POST',
            url: $scope.path + 'GetEmpBonus?MasterId=' + MasterId + '&CutOffDate=' + Date,
        }).then(function successCallback(response) {
            $scope.BonusEmpList = response.data.data;
        });
    }

    $scope.GetCurrency = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'GetCurrency',
        }).then(function successCallback(response) {
            $scope.BonusProcess.AmtDefCurrency = response.data.data[0].Currency;
            $scope.BonusProcess.LocalCurrency = response.data.data[0].LocalCurrency;
        });
    }

    //#endregion

    //#region Report

    $scope.GetReport = function () {
        try {
            $scope.fileName = "Bonus Process Report.xls";
            
            $http({
                method: 'POST',
                url: $scope.path +'Report',
                data: {
                    'workDate': $scope.BonusProcess.Work
                }
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    $rootScope.report($scope.downloadgriddataUrlPath + "?FullPath=" + response.data.FileName + "&fileName=" + $scope.fileName);//downloadgriddataUrlPath
                }
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };
    //#endregion

}