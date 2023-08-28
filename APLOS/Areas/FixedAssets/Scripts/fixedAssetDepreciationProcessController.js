'use strict';
fixedAssetDepreciationProcessController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$window'];
function fixedAssetDepreciationProcessController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window) {
    $rootScope.title = 'Depreciation Process';
    $scope.Action = 'Save';
    $scope.path = 'FixedAssets/FixedAssetRegister/';
    $scope.url = "FixedAssets/FixedAssetRegister";
    $scope.saveUrl = $scope.path + 'SaveFixedAssetDepreciationProcess';

    $scope.depreciationProcess = {
        FiscalYearId: null,
        //FromDate: $filter("dateFiltering")(Date.now()),
        ToDate: $filter("dateFiltering")(Date.now()),
        StartDate: null,
        EndDate: null
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

    $scope.tempList = [];
    $scope.paymentSelectedList = [];
    $scope.multiplevendorInvoiceSearchList = [
        {
            "name": "Customer Code",
            "value": "PartyCode"
        },
        {
            "name": "Customer Name",
            "value": "PartyName"
        }

    ];
    $scope.popUpParameters = {
        limit: 10,
        offset: 0,
        order: 'ASC',
        sort: 'PartyName',
        searchBy: 'PartyName',
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    function avoidCheckList(id) {
        for (var i = 0; i < $scope.paymentSelectedList.length; i++) {
            if ($scope.paymentSelectedList[i].PartyCode === id) {
                return true;
                break;
            }
        }
        return false;
    }
    $scope.getPopupCustomerList = function () {
        $scope.tempList = [];
        $scope.customerreceivableGLData = function (pageno) {
            $scope.customerReceivableGLUrl1 = 'accounts/AccountStatusDashboard/GetCustomerListForConfirmation?fromDate=' + $scope.report.FromDate + '&toDate=' + $scope.report.ToDate + '&paymentStatus=' + $scope.report.PaymentStatus;
            baseService.paginationBase($scope.customerReceivableGLUrl1, pageno, $scope.popUpParameters)
                .then(function (result) {
                    try {
                        $scope.paymentList = [];
                        angular.forEach(result.DATA.Rows, function (item) {
                            if (avoidCheckList(item.PartyCode) === false) {
                                $scope.paymentList.push(item);
                            }
                        })
                        $scope.popUpParameters.total_count = result.Total;
                        for (var i = 0; i < $scope.paymentList.length; i++) {
                            $scope.paymentList[i].Active = getActive($scope.tempList, $scope.paymentList[i].PartyCode);
                        }
                    } catch (e) {
                        ShowResult(e, 'Error');
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#CustomerListPopUP')).modal('show');
        $scope.customerreceivableGLData();
    };
    $scope.closePopUp = function () {
        angular.element(document.querySelector('#CustomerListPopUP')).modal('hide');
    };
    $scope.changeDateType = function (type) {
        $scope.multiplePayment.DateType = type;
    }
    function getActive(list, id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].PartyCode === id) {
                return true;
            }
        }

        return false;
    }
    $scope.pushInTempList = function (event, data) {
        try {
            if (event.currentTarget.checked) {
                if (checkExistTempList($scope.tempList, data.PartyCode) === false) {
                    $scope.tempList.push(data);
                }
                else {
                    for (var i = 0; i < baseService.arrayLength($scope.tempList); i++) {
                        if ($scope.tempList[i].PartyCode === data.PartyCode) {
                            $scope.tempList.splice(i, 1);
                            break;
                        }
                    }

                    $scope.tempList.push(data);
                }
            }
            else {
                for (var t = 0; t < baseService.arrayLength($scope.tempList); t++) {
                    if ($scope.tempList[t].PartyCode === data.PartyCode) {
                        $scope.tempList.splice(t, 1);
                        break;
                    }
                }
            }
        } catch (e) {
            event.currentTarget.checked = false;
            ShowResult(e, "failure");
        }
    }

    function checkExistTempList(list, id) {
        for (var i = 0; i < baseService.arrayLength(list); i++) {
            if (list[i].PartyCode === id) {
                return true;
            }
        }
        return false;
    }

    var NewCustomerSelectedList = [];
    $scope.closePopUp = function () {
        NewCustomerSelectedList = [];
        for (var i = 0; i < $scope.tempList.length; i++) {

            if (NewCustomerSelectedList, $scope.tempList[i].PartyId) {
                NewCustomerSelectedList.push($scope.tempList[i].PartyId);
            }

        }
        if (NewCustomerSelectedList.length > 0) {
            $scope.getcustomerInvoiceList();
        }

        angular.element(document.querySelector('#CustomerListPopUP')).modal('hide');
    };
    $scope.fixedAssetMastersList = [];
    $scope.getFixedAssetMastersData = function () {
        $scope.ToDatevalidation();
        if ($scope.invalidDocDate == false) {
            try {
                $http({
                    method: 'POST',
                    url: $scope.path + "GetfixedAssetMastersListForProcess",
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
            //msg = "Please select To Date!";
            return;
        }
        else if (baseService.isUndefinedOrNull($scope.depreciationProcess.FiscalYearId)) {
            ShowResult('Please select Fiscal Year!', 'failure');
            $scope.invalidDocDate = true;
            //msg = "Please select To Date!";
            return;
        }
        else if (new Date($scope.depreciationProcess.ToDate) > new Date()) {
            ShowResult('ToDate must be below or equal to current Date!', 'failure');
            $scope.invalidDocDate = true;
            //msg = "ToDate must be below or equal to current Date!";
            return;
        }
        else if (new Date($scope.depreciationProcess.StartDate) > new Date($scope.depreciationProcess.ToDate)) {
            ShowResult('To Date must be greater or equal to Fiscal Year Start Date!', 'failure');
            //msg = "To Date must be greater or equal to Fiscal Year Start Date!";
            $scope.invalidDocDate = true;
            return;
        }
        else if (new Date($scope.depreciationProcess.EndDate) < new Date($scope.depreciationProcess.ToDate)) {
            ShowResult('To Date must be less or equal to Fiscal Year End Date!', 'failure');
            //msg = "To Date must be less or equal to Fiscal Year End Date!";
            $scope.invalidDocDate = true;
            return;
        }
        //else $scope.invalidDocDate = false;
        //return manualValidation("div_ToDate", $scope.invalidDocDate, msg);
    }
    $scope.Save = function () {
        $scope.ToDatevalidation();
        if ($scope.invalidDocDate == false) {
            var selectedAssetMastersList = [];
            for (var i = 0; i < $scope.fixedAssetMastersList.length; i++) {
                if ($scope.fixedAssetMastersList[i].isSelected == true) {

                    if (selectedAssetMastersList, $scope.fixedAssetMastersList[i].Id) {
                        selectedAssetMastersList.push($scope.fixedAssetMastersList[i].Id);
                    }
                }
                if ($scope.fixedAssetMastersList[i].PreviousYearAsset > 0 && $scope.fixedAssetMastersList[i].PreviousYearAssetProcess==0) {
                    ShowResult('Please process previous fiscal year fixed Asset First!', 'failure');
                    return;
                }
                if ($scope.fixedAssetMastersList[i].PreviousYearAsset > 0 && $scope.fixedAssetMastersList[i].PreviousYearAssetProcess > 0 && $scope.fixedAssetMastersList[i].PreviousYearAssetFullProcess == "No") {
                    ShowResult('Please process previous fiscal year fixed Asset First!', 'failure');
                    return;
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
                    toDate: $scope.depreciationProcess.ToDate
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






