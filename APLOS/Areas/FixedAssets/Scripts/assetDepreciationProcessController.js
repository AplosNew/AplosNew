'use strict';
assetDepreciationProcessController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$window'];
function assetDepreciationProcessController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window) {
    $rootScope.title = 'Depreciation Process';
    $scope.Action = 'Save';
    $scope.path = 'FixedAssets/FixedAssetRegister/';
    $scope.url = "FixedAssets/FixedAssetRegister";
    $scope.saveUrl = $scope.path + 'SaveAssetDepreciationProcess';

    $scope.depreciationProcess = {
        FiscalYearId: null,
        ToDate: $filter("dateFiltering")(Date.now()),
        StartDate: null,
        EndDate: null,
        ProcessName: null
    };

    $http({
        method: 'GET',
        url: 'accounts/fiscalyear/getcbo'
    }).then(function successCallback(response) {
        $scope.fiscalYearList = response.data;
    });

    $scope.FiscalYearDataList = [];
    $scope.getFiscalYearData = function () {
        try {
            $http({
                method: 'GET',
                url: "accounts/fiscalyear/GetFiscalYearDataByFiscalYear?fiscalYearId=" + $scope.depreciationProcess.FiscalYearId
            }).then(function successCallback(response) {
                $scope.FiscalYearDataList = response.data;
                $scope.depreciationProcess.StartDate = response.data.StartDate;
                $scope.depreciationProcess.EndDate =response.data.EndDate;
            }),
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
        }

        catch (e) {

        }
    }

    $scope.fixedAssetMastersList = [];
    $scope.getFixedAssetMastersData = function () {
        $scope.ToDatevalidation();
        if ($scope.invalidDocDate == false) {
            try {
                $http({
                    method: 'POST',
                    url: $scope.path + "GetAssetMastersListForProcess",
                    data: {
                        fiscalYearId: $scope.depreciationProcess.FiscalYearId,
                        toDate: $scope.depreciationProcess.ToDate,
                        startDate: $scope.depreciationProcess.StartDate
                    },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    $scope.fixedAssetMastersList = response.data.DATA;
                }),
                    function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    }
            }

            catch (e) {

            }
        }
    }
    $scope.voucherDetailList = [];
    $scope.pushInTempListforProcess = function (event, data) {
        try {
            if (event.currentTarget.checked) {
                if (checkExistTempListforConfirm($scope.voucherDetailList, data.Id) === false) {
                    $scope.voucherDetailList.push(data);
                }
                else {
                    for (var i = 0; i < baseService.arrayLength($scope.voucherDetailList); i++) {
                        if ($scope.voucherDetailList[i].Id === data.Id) {
                            $scope.voucherDetailList.splice(i, 1);
                            break;
                        }
                    }

                    $scope.voucherDetailList.push(data);
                }
            }
            else {
                for (var t = 0; t < baseService.arrayLength($scope.voucherDetailList); t++) {
                    if ($scope.voucherDetailList[t].Id === data.Id) {
                        $scope.voucherDetailList.splice(t, 1);
                        break;
                    }
                }
            }
        } catch (e) {
            event.currentTarget.checked = false;
            ShowResult(e, "failure");
        }
    }

    function checkExistTempListforConfirm(list, id) {
        for (var i = 0; i < baseService.arrayLength(list); i++) {
            if (list[i].Id === id) {
                return true;
            }
        }
        return false;
    }
    $scope.refreshTemplateAssetMaster = function (args) {
        $("#headchk").ejCheckBox({ "change": CheckBoxSelectAllAssetMaster });
    };

    function CheckBoxSelectAllAssetMaster(e) {

        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;

        }

        var filtered = $("#GridFixedAssetMastersList").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.fixedAssetMastersList.length; i++) {
                $scope.fixedAssetMastersList[i].isSelected = ChkOrUnchk;
            }
        }
        else {

            for (var j = 0; j < filtered.length; j++) {

                filtered[j].isSelected = ChkOrUnchk;
            }


        }
        var gridObj = $("#GridFixedAssetMastersList").data("ejGrid");
        gridObj.refreshContent();
    };
    $scope.invalidDocDate = false;
    $scope.ToDatevalidation = function () {
        var msg = "";
        $scope.invalidDocDate = false;
        if (baseService.isUndefinedOrNull($scope.depreciationProcess.ToDate)) {
            ShowResult('Please select To Date!', 'failure');
            $scope.invalidDocDate = true;
            return;
        }
        else if (baseService.isUndefinedOrNull($scope.depreciationProcess.FiscalYearId)) {
            ShowResult('Please select Fiscal Year!', 'failure');
            $scope.invalidDocDate = true;
            return;
        }
        else if (baseService.isUndefinedOrNull($scope.depreciationProcess.ProcessName) || $scope.depreciationProcess.ProcessName==="") {
            ShowResult('Please Insert User Name!', 'failure');
            $scope.invalidDocDate = true;
            return;
        }
        else if (new Date($scope.depreciationProcess.ToDate) > new Date()) {
            ShowResult('ToDate must be below or equal to current Date!', 'failure');
            $scope.invalidDocDate = true;
            return;
        }
        else if (new Date($scope.depreciationProcess.StartDate) > new Date($scope.depreciationProcess.ToDate)) {
            ShowResult('To Date must be greater or equal to Fiscal Year Start Date!', 'failure');
            $scope.invalidDocDate = true;
            return;
        }
        else if (new Date($scope.depreciationProcess.EndDate) < new Date($scope.depreciationProcess.ToDate)) {
            ShowResult('To Date must be less or equal to Fiscal Year End Date!', 'failure');
            $scope.invalidDocDate = true;
            return;
        }
        
    }
    $scope.Save = function () {
        $scope.ToDatevalidation();
        if ($scope.invalidDocDate == false) {
            var selectedAssetMastersList = [];
            for (var i = 0; i < $scope.fixedAssetMastersList.length; i++) {
                if ($scope.fixedAssetMastersList[i].isSelected == true) {
                    if ($scope.fixedAssetMastersList[i].PreviousYearAsset > 0 && $scope.fixedAssetMastersList[i].PreviousYearAssetProcess == 0) {
                        ShowResult('Please process previous fiscal year fixed Asset First!', 'failure');
                        return;
                    }
                    if ($scope.fixedAssetMastersList[i].PreviousYearAsset > 0 && $scope.fixedAssetMastersList[i].PreviousYearAssetProcess > 0 && $scope.fixedAssetMastersList[i].PreviousYearAssetFullProcess == "No") {
                        ShowResult('Please process previous fiscal year fixed Asset First!', 'failure');
                        return;
                    }
                    if (selectedAssetMastersList, $scope.fixedAssetMastersList[i].Id) {
                        selectedAssetMastersList.push($scope.fixedAssetMastersList[i].Id);
                    }
                }
                
            }
            if (selectedAssetMastersList.length == 0) {
                ShowResult('Please select at least one Asset master', 'failure');
                return;
            }
            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: {
                    selectedAssetMastersList: selectedAssetMastersList,
                    fiscalYearId: $scope.depreciationProcess.FiscalYearId,
                    toDate: $scope.depreciationProcess.ToDate,
                    processName: $scope.depreciationProcess.ProcessName
                },
                dataType: 'JSON'
            }).then(function (response) {
                if (response.data.Error === true)
                    ShowResult(response.data.Message, 'failure');
                else {
                    $scope.getFixedAssetMastersData();
                    ShowResult(response.data.Message, 'success');

                }
            }), function (response) {
                ShowResult(response.data.Message, 'failure');
            };
        }

    };



};






