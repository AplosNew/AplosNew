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
    $controller("currencyBaseController", { $scope: $scope, $http: $http });
    $scope.path = 'FixedAssets/FixedAssetRegister/'

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
        DocDate: $filter("dateFiltering")(Date.now()),
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
        LegalDesignation: null,
        CurrencyId: null,
        CompanyCurrencyRate: null,
        ToCurrencyRate: null,
        ToCurrencyRate: null,
        PostingDate: $filter("dateFiltering")(Date.now()),

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
    $scope.Get = function (x) {
        var data = x.rowData;
        $scope.voucher.Status = data.Status;
        $scope.voucher.Id = data.Id;
        $scope.voucher.Remarks = data.Remarks;
        $scope.voucher.TrnCurrency = data.TrnCurrency;
        $scope.voucher.CurrencyId = data.trnCurrencyId;
        $scope.voucher.PartyName = data.CustomerName;
        $scope.voucher.PartyId = data.PartyId;
        $scope.voucher.CompanyCurrencyRate = data.CompanyCurrencyRate;
        $scope.voucher.ToCurrencyRate = data.CompanyCurrencyRate;
        $scope.voucher.BaseNagotiationValue = data.BaseNagotiationValue;
        $scope.voucher.DocDate = data.DocDate;
        $scope.voucher.VoucherNo = data.VoucherNo;

        if ($scope.voucher.Status == 'Sales') {
        $scope.getCboPartyPlantList($scope.voucher.PartyId, function (result) {
            $scope.partyPlantList = result;
            angular.forEach($scope.partyPlantList, function (item, i) {
                if (item.IsDefault) {
                    $scope.voucher.InvoicingPartyPlantId = data.PartyPlantId;
                    $scope.voucher.DeliveryPartyPlantId = data.DeliveryPartyPlantId;
                    $scope.voucher.InvoicingByAddress = data.InvoicingByAddress;
                    $scope.voucher.DeliveryByAddress = data.DeliveryByAddress;

                    $scope.voucher.InvoicingState = item.StateName;
                    $scope.voucher.InvoicingGSTIN = item.GSTIN;
                    $scope.voucher.DeliveryState = item.StateName;
                    $scope.voucher.DeliveryGSTIN = item.GSTIN;
                    $scope.voucher.InvoicingStateId = item.StateId;
                }
            });
        });
        }

        if ($scope.voucher.Status == 'Sales') {
            $scope.DisposeTpye();
            // return true;
        }

        $scope.getFARDisposeDetail(data.Id);
        $scope.Action = "Update";
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }

    };

    $scope.voucherDetailList = [];
    $scope.getFARDisposeDetail = function (fixedAssetRegisterDisposeId) {
        $http({
            method: 'GET',
            url: $scope.path + "GetFixedAssetRegisterDisposeEditList?fixedAssetRegisterDisposeId=" + fixedAssetRegisterDisposeId,
            // , url: 'FixedAssets/FixedAssetRegister/GetFixedAssetRegisterPopUpList'
        }).then(function successCallback(response) {
            $scope.voucherDetailList = response.data;
        });
    }


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

    $scope.invalidDocDate = false;
    $scope.checkDocDate = function () {
        var msg = "";
        if (new Date($scope.voucher.DocDate) > new Date()) {
            $scope.invalidDocDate = true;
            msg = "Doc date must be below or equal to current Date!";
        }
        else $scope.invalidDocDate = false;
        return manualValidation("div_DocDate", $scope.invalidDocDate, msg);
    };

    $scope.Clear = function () {
        $scope.Action = "Save";
        $scope.voucher.Active = true;
        $scope.voucher.Amount = 0;
        $scope.voucher.DocRefNo = null;
        $scope.voucher.Narration = null;
        $scope.voucher.Status = null;
        $scope.voucher.Remarks = null;
        $scope.voucher.PartyId = null;
        $scope.voucher.PartyPlantId = null;
        $scope.voucher.ToCurrencyRate = null;
        $scope.voucher.CurrencyId = null;
        $scope.voucher.PartyName = null;
        $scope.voucher.DocDate = $filter("date")(Date.now(), "dd-MMM-yyyy");
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
        if ($scope.voucher.Status == 'Sales' && baseService.isUndefinedOrNull($scope.voucher.CurrencyId)) {
            ShowResult("Please select Currency!", "failure");
            return true;
        }
        if ($scope.voucher.Status == 'Sales' && baseService.isUndefinedOrNull($scope.voucher.CompanyCurrencyRate)) {
            ShowResult("Please Input Rate!", "failure");
            return true;
        }
        if ($scope.voucherDetailList.length == 0) {
            ShowResult("Please select Fixed Asset Register!", "failure");
            return true;
        }
        if ($scope.voucherDetailList.length > 0) {
            for (var i = 0; i < $scope.voucherDetailList.length; i++) {
                if (new Date($scope.voucherDetailList[i].PurchaseDate) > new Date($scope.voucher.DocDate)) {
                    ShowResult("Doc date must be greater or equal to Invoice Date!", "failure");
                    return true;
                }
            } 
        }
        else {
            return false;
        }

    };


    $scope.Save = function () {
        $scope.voucher.ToCurrencyRate = $scope.voucher.CompanyCurrencyRate;
        $scope.voucher.PartyPlantId = $scope.voucher.InvoicingPartyPlantId;
        $scope.$broadcast("show-errors-check-validity");
        if ($scope.form0.$valid && !$scope.validation()) {
            if ($scope.Action === "Save" && $scope.voucher.Status == 'CompensateByEmployee') {
                $http({
                    method: "POST",
                    url: "fixedassets/fixedassetregister/createfixedassetlost",
                    data: {
                        "fixedAssetDisposed": $scope.voucher,
                        "fixedAssetRegister": $scope.voucherDetailList
                        
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
                        "fixedAssetDisposed": $scope.voucher,
                        "fixedAssetRegister": $scope.voucherDetailList
                       
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
            if ($scope.Action === "Save" && ($scope.voucher.Status == 'Scrap' || $scope.voucher.Status == 'Theft')) {
                $http({
                    method: "POST",
                    url: "fixedassets/fixedassetregister/CreateFixedAssetScrap",
                    data: {
                        "fixedAssetDisposed": $scope.voucher,
                        "fixedAssetRegister": $scope.voucherDetailList
                        //"status": $scope.voucher.Status,
                        //"fixedAssetRegister": $scope.voucherDetailList,
                        //"remarks": $scope.voucher.Remarks
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
            if ($scope.Action === "Update" && $scope.voucher.Status == 'CompensateByEmployee' ) {
                $http({
                    method: "POST",
                    url: "fixedassets/fixedassetregister/UpdateFixedAssetLost",
                    data: {
                        "fixedAssetDisposed": $scope.voucher,
                        "fixedAssetRegister": $scope.voucherDetailList

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
            if ($scope.Action === "Update" && ($scope.voucher.Status == 'Scrap' || $scope.voucher.Status == 'Theft')) {
                $http({
                    method: "POST",
                    url: "fixedassets/fixedassetregister/UpdateFixedAssetScrap",
                    data: {
                        "fixedAssetDisposed": $scope.voucher,
                        "fixedAssetRegister": $scope.voucherDetailList
                        
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
            else if ($scope.Action === "Update" && $scope.voucher.Status == 'Sales') {
                $http({
                    method: "POST",
                    url: "fixedassets/fixedassetregister/UpdateFixedAssetSales",
                    data: {
                        "status": $scope.voucher.Status,
                        "disposeVM": $scope.voucher,
                        "fixedAssetRegister": $scope.voucherDetailList,
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
            if (baseService.arrayLength($scope.voucherDetailList) > 0) {
                for (var i = 0; i < baseService.arrayLength($scope.voucherDetailList); i++) {
                    for (var j = 0; j < baseService.arrayLength($scope.assetRegisterPopUpList); j++) {
                        if ($scope.voucherDetailList[i].FixedAssetRegisterId == $scope.assetRegisterPopUpList[j].FixedAssetRegisterId) {
                            $scope.assetRegisterPopUpList[j].Active = true;
                        }
                    }
                }
            }
        }), function (response) {
            ShowResult(response.data.Message, 'failure');
        };
        angular.element(document.querySelector("#assetRegisterPopUpmodal")).modal("show");
    };
    $scope.closeFARegisterPopUp = function () {
        angular.element(document.querySelector("#assetRegisterPopUpmodal")).modal("hide");
    }
    function checkFAExist(list, FixedAssetRegisterId) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].FixedAssetRegisterId === FixedAssetRegisterId) {

                return true;
            }
        }
        return false;
    }
    $scope.selectFARegisterPopUp = function () {
        if (!baseService.isUndefinedOrNull($scope.voucher.Status)) {

            if (baseService.arrayLength($scope.assetRegisterPopUpList) > 0) {
                $scope.voucherDetailList = [];
                angular.forEach($scope.assetRegisterPopUpList, function (a) {
                        if (a.Active) {
                            $scope.voucherDetail.BudgetMasterId = a.BudgetMasterId;
                            $scope.voucherDetail.BudgetName = a.BudgetName;
                            $scope.voucherDetail.ActivityId = a.ActivityId;
                            $scope.voucherDetail.ActivityName = a.ActivityName;
                            $scope.voucherDetail.GLGeneralInfoId = a.GLGeneralInfoId;
                            $scope.voucherDetail.GLGeneralInfoName = a.GLGeneralInfoCode + '-' + a.GLGeneralInfoName;
                            $scope.voucherDetail.FixedAssetMasterId = a.FixedAssetMasterId;
                            $scope.voucherDetail.FixedAssetRegisterId = a.FixedAssetRegisterId;
                            $scope.voucherDetail.FixedAssetMasterName = a.FixedAssetMasterName;

                            $scope.voucherDetail.MaterialMasterName = a.MaterialMasterName;
                            $scope.voucherDetail.Article = a.Article;
                            $scope.voucherDetail.CapitalizationDate = a.CapitalizationDate;
                            $scope.voucherDetail.PurchaseDate = a.PurchaseDate;
                            $scope.voucherDetail.IssueDate = a.IssueDate;
                            $scope.voucherDetail.TrnCurrency = a.TrnCurrency;
                            $scope.voucherDetail.baseCurrency = a.BaseCurrency;

                            $scope.voucherDetail.IsOpeningBalance = a.IsOpeningBalance;
                            $scope.voucherDetail.vendor = a.Vendor;

                            $scope.voucherDetail.FAType = $scope.voucher.FAType;
                            $scope.voucherDetail.DocDate = $filter("dateFiltering")($scope.voucher.DocDate);
                            $scope.voucherDetail.DocRefNo = $scope.voucher.DocRefNo;
                            $scope.voucherDetail.Narration = $scope.voucher.Narration;
                            $scope.voucherDetail.EntityId = $scope.voucher.EntityId;
                            $scope.voucherDetail.PlantId = $scope.voucher.PlantId;


                            $scope.voucherDetail.FABaseAmount = a.FABaseAmount;
                            $scope.voucherDetail.SubAssetBaseAmount = a.SubAssetBaseAmount;
                            $scope.voucherDetail.PurchaseBaseAmount = a.PurchaseBaseAmount;
                            $scope.voucherDetail.ADBaseAmount = a.ADBaseAmount;
                            $scope.voucherDetail.NetBaseBookValue = a.NetBaseBookValue;

                            $scope.voucherDetail.Price = a.Price;
                            $scope.voucherDetail.SubAssetAmount = a.SubAssetAmount;
                            $scope.voucherDetail.PurchasePrice = a.PurchasePrice;
                            $scope.voucherDetail.ADBaseAmount = a.ADBaseAmount;
                            $scope.voucherDetail.NetBookValue = a.NetBookValue;
                            $scope.voucherDetail.InvoiceNo = a.InvoiceNo;

                            if ($scope.voucher.Status == 'Scrap') {
                                $scope.voucherDetail.NegotiationValue = a.NetBookValue;
                            }
                            $scope.voucherDetail.SerialNo = a.SerialNo;
                            $scope.voucherDetail.AssetNo = a.AssetNo;
                            $scope.voucherDetail.Id = a.Id;
                            $scope.voucherDetail.PartyType = 'Fixed Asset';
                            $scope.voucherDetailList.splice(0, 0, $scope.voucherDetail);
                            $scope.voucherDetail = {};
                        }
                });
                $scope.closeFARegisterPopUp();
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
        $scope.GetCurrencyExchangeRateList();
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

    $scope.currencyExchangeRate = [];
    $scope.GetCurrencyExchangeRateList = function () {
        if (!baseService.isUndefinedOrNull($scope.voucher.PostingDate) && !baseService.isUndefinedOrNull($scope.voucher.CurrencyId)) {
            $http({
                method: "GET",
                url: "currencies/ExchangeRate/GetCompanyCurrencyExchangeRate?fromdate=" + $scope.voucher.PostingDate + "&currencyId=" + $scope.voucher.CurrencyId
            }).then(function successCallback(response) {
                $scope.currencyExchangeRate = response.data;
                $scope.voucher.CompanyCurrencyRate = $scope.currencyExchangeRate.ToCurrencyRate;
                if ($scope.voucherDetailList.length > 0) {
                    $scope.updateBooksNegotiationValue();
                }
            });
        }
        else {
            $scope.currencyExchangeRate = [];
        }
    };

    $scope.DisposeTpye = function () {
        //$scope.removeRow();
        $scope.voucherDetailList = [];
    };

    $scope.calBooksNegotiationValue = function (data) {
        var assetNegotiationValue = parseFloat(data.NegotiationValue);
        if (assetNegotiationValue === 0 || assetNegotiationValue <0) {
            data.NegotiationValue = "";
            ShowResult("Negotiation Amount should be greater than 0(zero).", "failure");
        }
        if ($scope.voucher.Status == 'CompensateByEmployee') {
            data.BaseNagotiationValue = data.NegotiationValue;
        }
        else {
            data.BaseNagotiationValue = data.NegotiationValue * $scope.voucher.CompanyCurrencyRate;
        }
    }
    $scope.calBooksAdjustmentDepreciation = function (data) {
        var assetNetBaseBookValue = parseFloat(data.NetBaseBookValue), assetAdjustmentDepreciation = parseFloat(data.AdjustmentDepreciationAmount);
        if (assetAdjustmentDepreciation > assetNetBaseBookValue) {
            data.AdjustmentDepreciationAmount = "";
            ShowResult("Adjustment Depreciation Amount should not exceed Net Base Amount.", "failure");
        }
    }
    $scope.updateBooksNegotiationValue = function () {
        for (var i = 0; i < $scope.voucherDetailList.length; i++) {
            $scope.voucherDetailList[i].BaseNagotiationValue = $scope.voucherDetailList[i].NegotiationValue * $scope.voucher.CompanyCurrencyRate
        }
    }

    $scope.onClickExcelPrints = function (args) {

        try {
            var data = args.data;
            var reportFormat = "Excel";

            var file_src = 'FixedAssets/FixedAssetRegister/GetBulletinTamplateIndexReport?reportFormat=' + reportFormat + '&fixedAssetRegisterDisposeId=' + data.Id
            $rootScope.report(file_src);

        } catch (e) {

        }
    }

    $scope.onClickPdfPrints = function (args) {

        try {
            var data = args.data;
            var reportFormat = "Pdf";

            var file_src = 'FixedAssets/FixedAssetRegister/GetFixedAssetDisposePdfReport?reportFormat=' + reportFormat + '&fixedAssetRegisterDisposeId=' + data.Id
            $rootScope.report(file_src);

        } catch (e) {

        }
    }


    


}
