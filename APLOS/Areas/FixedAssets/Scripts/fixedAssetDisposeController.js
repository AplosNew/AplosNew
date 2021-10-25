"use strict";
fixedAssetDisposeController.$inject = ["commonMessage", "$scope", "$rootScope", "baseService", "$http", "$filter", "$controller"];
function fixedAssetDisposeController(commonMessage, $scope, $rootScope, baseService, $http, $filter, $controller) {
    $rootScope.title = "Fixed Asset Dispose";
    $scope.Action = "Save";
    $scope.index = -1;
    $scope.voucherDetailList = [];
    $scope.voucherDetailCurrencyList = [];
    $scope.voucherList = [];
    $scope.isCrBankAmount = false;
    $scope.isDrBankAmount = false;
    $scope.currencyDisable = false;
    $scope.isAdvance = true;
    $scope.partyType = "Customer";
    $scope.postUrl = "accounts/OpeningBalance/PostOBAdvanceJournal";
    $controller('employeeBaseController', { $scope: $scope, $http: $http });
    $controller("partyBaseController", { $scope: $scope, $http: $http });

    $scope.voucherDetailList = [];
    $scope.searchBy = "FARDisposeNo"; $scope.search = "";
    $scope.searchByList = [{ value: 'FARDisposeNo', name: "FARDispose No" }, { value: 'EmployeeName', name: "Employee" }, { value: 'Status', name: "Status" }];

    //baseService.init("Accounts/OpeningBalance/GetOBAdvanceJournalList", null, null, "DESC", "PostingDate DESC, DocRefNo", "PostingDate");
    //$scope.getData = function (pageno) {
    //    baseService.pagination(pageno)
    //        .then(function (result) {
    //            $scope.voucherList = result.Rows;
    //            $scope.voucherListParameters.total_count = result.Total;
    //        }, function () {
    //            ShowResult(commonMessage.NetworkError, "failure");
    //        }).finally(function () {
    //        });
    //};


    $scope.voucherList = [];
    $scope.getData = function () {
        $http({
            method: 'Post'
            , url: 'FixedAssets/FixedAssetRegister/GetFixedAssetLostList'
            , data: { column: $scope.searchBy, value: $scope.search }
            , dataType: 'JSON'
        }).then(function (response) {
            $scope.voucherList = response.data;
        }), function (response) {
            ShowResult(response.data.Message, 'failure');
        };
    };
    $scope.getData();
    $scope.voucher = {
        Id: null,
        DocDate: null,
        DocRefNo: null,
        Amount: 0,
        Narration: null,
        Remarks: null,
        IsPark: false,
        AddedBy: null,
        AddedDate: null,
        AddedFromIP: null,
        EmployeeId: null,
        Designation: null,
        DOJ: null,
        GivenDesignation: null,
        Department: null,
        LegalDesignation: null
    };

    $scope.voucherDetail = {
        Id: null,
        GLGeneralInfoId: null,
        BudgetMasterId: null,
        ActivityId: null,
        COAICode: null,
        AccountTypeId: null,
        CurrencyId: null,
        DocRefNo: null,
        DrAmount: null,
        CrAmount: null,
        Narration: null,
        BankMasterId: null,
        CashMasterId: null,
        PartyId: null,
        PartyPlantId: null,
        TransactionTypeId: null,
        FAType: null,
        DrDisable: false,
        CrDisable: false,
        CashCurrencyId: null,
        BankCurrencyId: null,
        BankAmount: null
    };
    $scope.Get = function (data) {
        $scope.voucher.Id = data.Id;
        $scope.Action = "Update";
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }

    };


    $scope.removeRow = function (index) {
        $scope.voucherDetailList.splice(index, 1);
    };

    $scope.fixedAssetDisposeStatusList = [];
    $http({
        method: 'GET',
        url: 'Enum/GetFixedAssetDisposeStatusEnumCbo/'
    }).then(function successCallback(response) {
        $scope.fixedAssetDisposeStatusList = response.data;
    });

    $scope.Clear = function () {
        $scope.Action = "Save";
        $scope.voucher.Active = true;
        $scope.voucher.Amount = 0;
        $scope.voucher.DocRefNo = null;
        $scope.voucher.Narration = null;
        $scope.voucher.VoucherDate = $filter("date")(Date.now(), "dd-MMM-yyyy");
        $scope.voucherDetailList = [];
    };

    $scope.showEmployeeListPopUp = function () {
        baseService.setCurrentPage('employeeList');
        $scope.getEmployeeData = function (pageno) {
            var url = null;
            if (baseService.isUndefinedOrNull($scope.employeeUrl)) {
                url = 'accounts/EmployeePayable/GetEmployeeListAllPlant';
            }
            else {
                url = $scope.employeeUrl;
            }
            baseService.paginationBase(url, pageno, $scope.employeeParameters)
                .then(function (result) {
                    $scope.employeeList = result.Rows;
                    $scope.employeeParameters.total_count = result.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#employeePopUp')).modal('show');
        $scope.getEmployeeData();
    };

    $scope.closeEmployeePopUp = function () {
        if ($scope.employeeIndex !== -1) {
            var employee = $scope.employeeList[$scope.employeeIndex];
            $scope.voucher.EmployeeId = employee.SystemId;
            $scope.voucher.EmployeeName = employee.EmployeeCode + ' - ' + employee.EmployeeName;
            $scope.voucher.DOJ = employee.DOJ;
            $scope.voucher.Department = employee.Department;
            $scope.voucher.Designation = employee.Designation;
            $scope.voucher.GivenDesignation = employee.GivenDesignation;
            $scope.voucher.LegalDesignation = employee.LegalDesignation;
        }
        $scope.hideEmployeePopUp();
    };

    $scope.validation = function () {
        if ($scope.voucher.Status == 'CompensateByEmployee' && baseService.isUndefinedOrNull($scope.voucher.EmployeeId)) {
            ShowResult("Please select Employee!", "failure");
            return true;
        }
        if ($scope.voucher.Status == 'Sales' && baseService.isUndefinedOrNull($scope.voucher.PartyId)) {
            ShowResult("Please select Customer!", "failure");
            return true;
        }
        if ($scope.voucherDetailList.length == 0) {
            ShowResult("Please select Fixed Asset Register!", "failure");
            return true;
        }
        else {
            return false;
        }

    };


    $scope.Save = function () {
        $scope.$broadcast("show-errors-check-validity");
        if ($scope.form0.$valid && !$scope.validation()) {
            if ($scope.Action === "Save" && $scope.voucher.Status == 'CompensateByEmployee') {
                $http({
                    method: "POST",
                    url: "fixedassets/fixedassetregister/createfixedassetlost",
                    data: {
                        "status": $scope.voucher.Status,
                        "fixedAssetRegister": $scope.voucherDetailList,
                        "employeeId": $scope.voucher.EmployeeId,
                        "remarks": $scope.voucher.Remarks
                    },
                    dataType: "JSON"
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, "failure");
                    }
                    else {
                        ShowResult(response.data.Message, "success");
                        $scope.getData();
                        $scope.Clear();
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, "failure");
                });
                return true;
            }
            if ($scope.Action === "Save" && $scope.voucher.Status == 'Sales') {
                $http({
                    method: "POST",
                    url: "fixedassets/fixedassetregister/CreateFixedAssetSales",
                    data: {
                        "status": $scope.voucher.Status,
                        "fixedAssetRegister": $scope.voucherDetailList,
                        "partyId": $scope.voucher.PartyId,
                        "partyPlantId": $scope.voucher.InvoicingPartyPlantId,
                        "remarks": $scope.voucher.Remarks
                    },
                    dataType: "JSON"
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, "failure");
                    }
                    else {
                        ShowResult(response.data.Message, "success");
                        $scope.getData();
                        $scope.Clear();
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, "failure");
                });
                return true;
            }
            if ($scope.Action === "Save" && $scope.voucher.Status == 'Scrap') {
                $http({
                    method: "POST",
                    url: "fixedassets/fixedassetregister/CreateFixedAssetScrap",
                    data: {
                        "status": $scope.voucher.Status,
                        "fixedAssetRegister": $scope.voucherDetailList,
                        "remarks": $scope.voucher.Remarks
                    },
                    dataType: "JSON"
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, "failure");
                    }
                    else {
                        ShowResult(response.data.Message, "success");
                        $scope.getData();
                        $scope.Clear();
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, "failure");
                });
                return true;
            }
            else if ($scope.Action === "Update") {
                $http({
                    method: "POST",
                    url: "accounts/OpeningBalance/UpdateOBAdvanceJournal",
                    data: {
                        "voucherVM": $scope.voucher,
                        "voucherDetailVMList": JSON.stringify($scope.voucherDetailList)
                    },
                    dataType: 'JSON'
                    , contentType: "application/json charset=utf-8"
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, "failure");
                    }
                    else {
                        ShowResult(response.data.Message, "success");
                        $scope.getData();
                        $scope.Clear();
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, "failure");
                });
            }
            return true;
        }
    };

    $scope.voucherId = null;
    $scope.confirmPost = function (voucherId) {
        $scope.voucherId = voucherId;
        $scope.message_confirmation = "Are you sure to Post?";
        angular.element(document.querySelector("#confirmPostPopUp")).modal("show");
    };




    $scope.searchByFARegister = "FixedAssetMasterName"; $scope.searchFARegister = "";
    $scope.searchByFARegisterList = [{ value: 'SerialNo', name: "SerialNo" }, { value: 'AssetNo', name: "AssetNo" }, { value: 'InvoiceNo', name: "Invoice No" }, { value: 'MaterialMasterName', name: "MaterialMaster" }, { value: 'Article', name: "Article" }, { value: 'FixedAssetMasterName', name: "FixedAssetMaster" }];

    $scope.assetRegisterPopUpList = [];
    $scope.getFixedAssetRegisterPopUpList = function () {
        $http({
            method: 'Post'
            , url: 'FixedAssets/FixedAssetRegister/GetFixedAssetRegisterPopUpList'
            , data: { column: $scope.searchByFARegister, value: $scope.searchFARegister }
            , dataType: 'JSON'
        }).then(function (response) {
            $scope.assetRegisterPopUpList = response.data;
        }), function (response) {
            ShowResult(response.data.Message, 'failure');
        };
        angular.element(document.querySelector("#assetRegisterPopUpmodal")).modal("show");
    };
    $scope.closeFARegisterPopUp = function () {
        angular.element(document.querySelector("#assetRegisterPopUpmodal")).modal("hide");
    }
    $scope.selectFARegisterPopUp = function (x) {
        if (!baseService.isUndefinedOrNull($scope.voucher.Status)) {

            var data = x.data;

            var getRow = $filter("filter")($scope.voucherDetailList, { "GLGeneralInfoId": data.GLGeneralInfoId, "BudgetMasterId": data.BudgetMasterId, "ActivityId": data.ActivityId, "FixedAssetRegisterId": data.FixedAssetRegisterId });
            if (!baseService.isUndefinedOrNull(getRow) && getRow.length == 0) {
                $scope.voucherDetail.BudgetMasterId = data.BudgetMasterId;
                $scope.voucherDetail.BudgetName = data.BudgetName;
                $scope.voucherDetail.ActivityId = data.ActivityId;
                $scope.voucherDetail.ActivityName = data.ActivityName;
                $scope.voucherDetail.GLGeneralInfoId = data.GLGeneralInfoId;
                $scope.voucherDetail.GLGeneralInfoName = data.GLGeneralInfoCode + '-' + data.GLGeneralInfoName;
                $scope.voucherDetail.FixedAssetMasterId = data.FixedAssetMasterId;
                $scope.voucherDetail.FixedAssetRegisterId = data.FixedAssetRegisterId;
                $scope.voucherDetail.ParticularName = data.FixedAssetMasterName;

                $scope.voucherDetail.Material = data.MaterialMasterName;
                $scope.voucherDetail.Article = data.Article;
                $scope.voucherDetail.CapitalizationDate = data.CapitalizationDate;
                $scope.voucherDetail.PurchaseDate = data.PurchaseDate;
                $scope.voucherDetail.IssueDate = data.IssueDate;
                $scope.voucherDetail.TrnCurrency = data.TrnCurrency;
                $scope.voucherDetail.isOB = data.IsOBBalance;
                $scope.voucherDetail.vendor = data.Vendor;
                
                $scope.voucherDetail.FAType = $scope.voucher.FAType;
                $scope.voucherDetail.DocDate = $filter("dateFiltering")($scope.voucher.DocDate);
                $scope.voucherDetail.DocRefNo = $scope.voucher.DocRefNo;
                $scope.voucherDetail.Narration = $scope.voucher.Narration;
                $scope.voucherDetail.EntityId = $scope.voucher.EntityId;
                $scope.voucherDetail.PlantId = $scope.voucher.PlantId;
                $scope.voucherDetail.Price = data.Price;
                $scope.voucherDetail.SubAssetAmount = data.SubAssetAmount;
                $scope.voucherDetail.PurchasePrice = data.PurchasePrice;
                $scope.voucherDetail.ADBaseAmount = data.ADBaseAmount;
                $scope.voucherDetail.NetBookValue = data.NetBookValue;
                if ($scope.voucher.Status == 'Scrap') {
                    $scope.voucherDetail.NegotiationValue = data.NetBookValue;
                }
                $scope.voucherDetail.SerialNo = data.SerialNo;
                $scope.voucherDetail.AssetNo = data.AssetNo;
                $scope.voucherDetail.Id = data.Id;
                $scope.voucherDetail.PartyType = 'Fixed Asset';
                $scope.voucherDetailList.splice(0, 0, $scope.voucherDetail);
                $scope.voucherDetail = {};
                $scope.closeFARegisterPopUp();

            }
            else {
                ShowResult('Asset No ' + data.FixedAssetRegisterId+' already exist !!', 'failure', 'assetRegisterPopUpmodal');

            }

        }
        else {
            ShowResult('Please select Type !!', 'failure', 'assetRegisterPopUpmodal');
        }

    };
    $scope.showPopup = function () {
        angular.element(document.querySelector('#employeeSelectionPopUp')).modal('show');
    }
    $scope.hidePopup = function () {
        angular.element(document.querySelector('#employeeSelectionPopUp')).modal('hide');
    }

    $scope.invoicingPartyPopUp = function () {
        //debugger;
        angular.element(document.querySelector('#invoicingPartyPopUp')).modal('show');
    };
    $scope.closePartyPopUp = function (x) {
        //if ($scope.partyIndex !== -1) {


        var party = x.data;// $scope.partyList[$scope.partyIndex];
        $scope.voucher.PartyName = party.Code + " - " + party.UserName;
        $scope.voucher.PartyId = party.Id;
        $scope.voucher.PaymentTermId = party.PaymentTermId;
        $scope.voucher.CurrencyId = party.CurrencyId;
        // $scope.GetCurrencyExchangeRateList();
        //  $scope.changePaymentTerm($scope.salesVM.PaymentTermId);
        $scope.partyPlantList = [];
        $scope.getCboPartyPlantList(party.Id, function (result) {
            $scope.partyPlantList = result;
            angular.forEach($scope.partyPlantList, function (item, i) {
                if (item.IsDefault) {
                    $scope.partyPlantId = item.Value;
                    $scope.voucher.InvoicingPartyPlantId = item.Value;
                    $scope.voucher.DeliveryPartyPlantId = item.Value;
                    $scope.voucher.InvoicingByAddress = item.Address1;
                    $scope.voucher.DeliveryByAddress = item.Address1;
                    $scope.voucher.InvoicingState = item.StateName;
                    $scope.voucher.InvoicingGSTIN = item.GSTIN;
                    $scope.voucher.DeliveryState = item.StateName;
                    $scope.voucher.DeliveryGSTIN = item.GSTIN;
                    $scope.voucher.InvoicingStateId = item.StateId;
                }
            });
        });
        //}
        $scope.hidePartyPopUp();
    };
    $scope.closeInvoicingPartyPopUp = function () {
        //if ($scope.salesMaterialList.length || $scope.chargesList.length) {

        if (!baseService.isUndefinedOrNull($scope.voucher.ChangeInvoicingStateId)) {
            if ($scope.voucher.PlantStateId == $scope.voucher.InvoicingStateId == $scope.voucher.ChangeInvoicingStateId)
                angular.element(document.querySelector('#invoicingPartyPopUp')).modal('hide');
            else if ($scope.voucher.InvoicingStateId == $scope.voucher.ChangeInvoicingStateId)
                angular.element(document.querySelector('#invoicingPartyPopUp')).modal('hide');
            else if ($scope.voucher.PlantStateId != $scope.voucher.InvoicingStateId && $scope.voucher.PlantStateId != $scope.voucher.ChangeInvoicingStateId)
                angular.element(document.querySelector('#invoicingPartyPopUp')).modal('hide');
            else
                ShowResult('Change is not allowed', 'failure', 'invoicingPartyPopUp');
        }
        else
            angular.element(document.querySelector('#invoicingPartyPopUp')).modal('hide');
        //}
        //else
        // angular.element(document.querySelector('#invoicingPartyPopUp')).modal('hide');

    };

    $scope.billShippAddress = function (id, flag) {
        if (!baseService.isUndefinedOrNull(id)) {
            var address = $.grep($scope.partyPlantList, function (item) { return item.Value === id; })[0].Address1;
            var state = $.grep($scope.partyPlantList, function (item) { return item.Value === id; })[0].StateName;
            var stateId = $.grep($scope.partyPlantList, function (item) { return item.Value === id; })[0].StateId;
            if (flag === 'billTo') {
                $scope.voucher.InvoicingState = state;
                $scope.voucher.ChangeInvoicingStateId = stateId;
                $scope.voucher.InvoicingGSTIN = $.grep($scope.partyPlantList, function (item) { return item.Value === id; })[0].GSTIN;
                return $scope.voucher.InvoicingByAddress = address;
            }
            else if (flag === 'shipTo') {
                $scope.voucher.DeliveryState = state;
                $scope.voucher.DeliveryGSTIN = $.grep($scope.partyPlantList, function (item) { return item.Value === id; })[0].GSTIN;
                return $scope.voucher.DeliveryByAddress = address;
            }
        }
        else {
            if (flag === 'billTo') {
                $scope.voucher.InvoicingState = null;
                $scope.voucher.InvoicingGSTIN = null;
                return $scope.voucher.InvoicingByAddress = null;
            }
            else if (flag === 'shipTo') {
                $scope.voucher.DeliveryState = null;
                $scope.voucher.DeliveryGSTIN = null;
                return $scope.voucher.DeliveryByAddress = null;
            }
        }
    };

}