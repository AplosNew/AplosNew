"use strict";
bankReconciliationDataUploadReconciledController.$inject = ["commonMessage", "$scope", "$rootScope", "baseService", "$http", "$filter", "$window",  "$controller"];
function bankReconciliationDataUploadReconciledController(commonMessage, $scope, $rootScope, baseService, $http, $filter, $window, $controller) {
    $rootScope.title = "Bank Reconciliation Uploaded Data";
    $scope.Action = "Save";
    $scope.index = -1;
    $scope.path = "banks/bankreconciliation/";
    $scope.listUrl = $scope.path + '/GetBankreconciliationList';
    $scope.getBnkReconListUrl = $scope.path + "GetBankReconciledList/";
    $scope.issuedReconUrl = $scope.path + "GetIssuedNotPresentList";
    $scope.receivedReconUrl = $scope.path + "GetReceivedNotPresentList";
    $scope.bankCrReconUrl = $scope.path + "GetBankCrReconList";
    $scope.bankDrReconUrl = $scope.path + "GetBankDrReconList";
    $scope.deleteUrl = $scope.path + "/DeleteBankreconciliation";
    $controller("bankBaseController", { $scope: $scope, $http: $http });

    baseService.init($scope.listUrl, null, null, "DESC", "BankName", "BankName");
    //$scope.getData = function (pageno) {
    //    baseService.pagination(pageno)
    //        .then(function (result) {
    //            $scope.bankreconciliationList = result.Rows;
    //        }, function () {
    //            ShowResult(commonMessage.NetworkError, "failure");
    //        }).finally(function () {
    //        });
    //};
    //$scope.getData();

    $scope.searchByList = [
        {
            "name": "BankName",
            "value": "BankName"
        },
        {
            "name": "Bank Statement No",
            "value": "BankStatementNo"
        }
    ];

    $scope.bankReconciliation = {
        Id: null,
        CompanyGroupId: $window.companyGroupId,
        CompanyId: $window.companyId,
        BankMasterId: null,
        BankName: null,
        BankBranch: null,
        BankAccount: null,
        BankGL: null,
        BankCurrency: null,
        OpeningBlance: null,
        ClosingBalance: null,
        BankStatementNo: null,
        FromDate: $filter("dateFiltering")(Date.now()),
        ToDate: $filter("dateFiltering")(Date.now())
    };
    $scope.bankReconciliationNew = Object.assign({}, $scope.bankReconciliation);

    //$scope.getCutOffDate = function () {
    //    $http.get("Accounts/OpeningBalance/GetACCCutOffDate")
    //        .then(function (response) {
    //            if (response.data !== null) {
    //                $scope.cutOffDate = $filter("dateFiltering")(response.data.CutOffDate);
    //            }
    //            else {
    //                ShowResult("Opening Balance Cut Off date not found!", "failure");
    //            }
    //        });
    //}
    //$scope.getCutOffDate();

    $scope.bankList = [];
    $scope.bankIndex = -1;
    $scope.selectedBank = null;
    

    $scope.selectBankPopUp = function (index, id) {
        $scope.bankIndex = index;
        $scope.selectedBank = id;
    };

    $scope.closeBankPopUp = function () {
        if ($scope.bankIndex !== -1) {
            selectBankRow();
        }
        angular.element(document.querySelector("#bankPopUp")).modal("hide");
        $scope.bankIndex = -1;
    };

    function selectBankRow() {
        var bank = $scope.bankList[$scope.bankIndex];
        if (bank.GLGeneralInfoId === null) {
            ShowResult("Bank GL not found!", "failure");
        }
        else if (bank.CurrencyId === null) {
            ShowResult("Bank Transaction Currency not found!", "failure");
        }
        else {
            $scope.bankReconciliationNew.BankMasterId = bank.BankMasterId;
            $scope.bankReconciliationNew.BankName = bank.BankName;
            $scope.bankReconciliationNew.BankAccount = bank.AccountTitle;
            $scope.bankReconciliationNew.BankGL = bank.GLItem;
            $scope.bankReconciliationNew.BankCurrency = bank.CurrencyCode;
            $scope.bankReconciliationNew.BankBranch = bank.BankBranchName;
            $scope.bankReconciliationNew.GLGeneralInfoId = bank.GLGeneralInfoId;
            $scope.bankReconciliationNew.BankGL = bank.GLGeneralInfoId + ' - ' + bank.GLGeneralInfoName;
            //$scope.getBankReconLastDate($scope.bankReconciliationNew.BankMasterId);
            $scope.clear();
        }
    }
    //$scope.getBankReconLastDate = function (id) {
    //    $http.get("Banks/BankReconciliation/GetBankReconLastDate?bankMasterId=" + id)
    //        .then(function (response) {

    //            $scope.bankReconciliationNew.FromDate = response.data.FromDate;
    //            $scope.bankReconciliationNew.OpeningBlance = response.data.ClosingBalance;
    //            $scope.bankReconciliationNew.ClosingBalance = null;
    //            $scope.bankReconciliationNew.BankStatementNo = null;
    //            if ($scope.bankReconciliationNew.FromDate == null)
    //                $scope.bankReconciliationNew.FromDate = $filter("dateFiltering")($scope.cutOffDate);
    //        });
    //}
    //$scope.getBankReconDrCrTotalAmount = function (id, fromDate, toDate) {
    //    $http.get("Banks/BankReconciliation/GetBankReconDrCrTotalAmount?bankMasterId=" + id + '&fromDate=' + fromDate + '&toDate=' + toDate)
    //        .then(function (response) {
    //            $scope.bankCrAmmount = response.data.bankCrAmmount;
    //            $scope.bankDrAmmount = response.data.bankDrAmmount;
    //        });
    //}

    $scope.tab = 5;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.getBnkReconList = function () {
        try {
            $scope.getBankDrReconList();
            $scope.getBankReconciliationUploadedDrData();                 
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.Save = function () {
        try {
            $scope.$broadcast("show-errors-check-validity");
            if ($scope.bankReconciliationNewForm.$valid && !$scope.postingDateValidate()) {
                checkAfterTotalAmount($scope.bnkReconList[$scope.bnkReconList.length - 1].After, parseFloat($scope.bankReconciliationNew.ClosingBalance));
                //$scope.tempList = [];
                //listMerge($scope.tempList, $scope.issuedTempList);
                //listMerge($scope.tempList, $scope.receivedTempList);
                //listMerge($scope.tempList, $scope.bankCrTempList);
                //listMerge($scope.tempList, $scope.bankDrTempList);
                angular.copy($scope.bankReconciliationNew, $scope.bankReconciliation);
                $http({
                    method: "POST",
                    url: $scope.path + "create",
                    data: {
                        "bankReconciliation": $scope.bankReconciliation
                        , "tempList": $scope.tempList
                    },
                    dataType: "JSON"
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, "failure");
                    }
                    else {
                        $scope.getBankReconLastDate($scope.bankReconciliationNew.BankMasterId);
                        $scope.getBnkReconList()
                        ShowResult(response.data.Message, "success");
                    }
                });
            }
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    function checkAfterTotalAmount(totalAfterAmount, closingBalance) {
        if (parseFloat(totalAfterAmount) !== parseFloat(closingBalance))
            throw "After reconciled total amount must be equal closing balance......!";
    }

    $scope.validDateRange = function (event,data, list, index) {
        try {
            if (new Date($scope.bankReconciliationNew.FromDate) > new Date(data.EncashmentDate) ||
                new Date($scope.bankReconciliationNew.ToDate) < new Date(data.EncashmentDate)) {
                for (var i = 0; i < $scope[list].length; i++) {
                    if ($scope[list][i].VoucherDetailId === data.VoucherDetailId) {
                        $scope[list][i].Flag = false;
                        data.Flag = false;
                    }
                }
                throw "Encashment date is out of date range..............!";
            }
            if (new Date(data.PostingDate) > new Date(data.EncashmentDate)) {
                for (var i = 0; i < $scope[list].length; i++) {
                    if ($scope[list][i].VoucherDetailId === data.VoucherDetailId) {
                        $scope[list][i].Flag = false;
                        data.Flag = false;
                    }
                }
                throw "Encashment date can not be less then posting date [" + data.PostingDate + "]";
            }
        } catch (e) {
            ShowResult(e, "failure");
        }
    };
    $scope.postingDateValidate = function () {
        for (var i = 0; i < $scope.tempList.length; i++) {
            if (baseService.isUndefinedOrNull($scope.tempList[i].ReconcileDate)) {
                ShowResult("Please Inpute Encashment Date ", "failure");
                return true;
                break;
            }
            else {
                return false;
            }
        }
    }
   
    //$scope.issuedReconList = [];
    //$scope.issuedTempList = [];
    //$scope.issuedReconParameters = {
    //    limit: 10,
    //    offset: 0,
    //    order: "asc",
    //    sort: "PostingDate",
    //    searchBy: "PostingDate",
    //    pageSize: 10,
    //    total_count: 0,
    //    search: null,
    //    serverPagination: true
    //};

    //$scope.getIssuedReconList = function () {
    //    $scope.issuedReconParameters.cutOffDate = $scope.cutOffDate;
    //    $scope.issuedReconParameters.bankMasterId = $scope.bankReconciliationNew.BankMasterId;
    //    $scope.issuedReconParameters.fromDate = $scope.bankReconciliationNew.FromDate;
    //    $scope.issuedReconParameters.toDate = $scope.bankReconciliationNew.ToDate;
    //    $scope.getIssuedReconData = function (pageno) {
    //        baseService.paginationBase($scope.issuedReconUrl, pageno, $scope.issuedReconParameters)
    //            .then(function (result) {
    //                $scope.issuedReconDataList = result.Rows;
    //                $scope.issuedReconParameters.total_count = result.Total;
    //                if (baseService.arrayLength($scope.issuedReconList) === 0) {
    //                    baseService.getDDLSearchColumn(result.Rows, $scope.issuedReconList);
    //                    $scope.issuedAmmount = $scope.amountCalculate($scope.issuedReconDataList);
    //                    $scope.issuedReconAmount = $scope.bnkReconList[1].ReconciledValue;
    //                    for (var i = 0; i < baseService.arrayLength($scope.issuedReconDataList); i++) {
    //                        $scope.issuedReconDataList[i].PostingDate = $filter("dateFiltering")($scope.issuedReconDataList[i].PostingDate);
    //                        $scope.issuedReconDataList[i].Flag = isExistInList($scope.issuedTempList, $scope.issuedReconDataList[i].VoucherDetailId);
    //                    }
    //                }
    //            }, function () {
    //                ShowResult(commonMessage.NetworkError, "failure");
    //            }).finally(function () {
    //            });
    //    };
    //    $scope.getIssuedReconData();
    //};

    //$scope.receivedReconList = [];
    //$scope.receivedTempList = [];
    //$scope.receivedReconParameters = {
    //    limit: 10,
    //    offset: 0,
    //    order: "asc",
    //    sort: "PostingDate",
    //    searchBy: "PostingDate",
    //    pageSize: 10,
    //    total_count: 0,
    //    search: null,
    //    serverPagination: true
    //};

    //$scope.getReceivedReconList = function () {
    //    $scope.receivedReconParameters.cutOffDate = $scope.cutOffDate;
    //    $scope.receivedReconParameters.bankMasterId = $scope.bankReconciliationNew.BankMasterId;
    //    $scope.receivedReconParameters.fromDate = $scope.bankReconciliationNew.FromDate;
    //    $scope.receivedReconParameters.toDate = $scope.bankReconciliationNew.ToDate;
    //    $scope.getReceivedReconData = function (pageno) {
    //        baseService.paginationBase($scope.receivedReconUrl, pageno, $scope.receivedReconParameters)
    //            .then(function (result) {
    //                $scope.receivedReconDataList = result.Rows;
    //                $scope.receivedReconParameters.total_count = result.Total;
    //                if (baseService.arrayLength($scope.receivedReconList) === 0) {
    //                    baseService.getDDLSearchColumn(result.Rows, $scope.receivedReconList);
    //                    $scope.receivedAmmount = $scope.amountCalculate($scope.receivedReconDataList);
    //                    $scope.receivedReconAmount = $scope.bnkReconList[3].ReconciledValue;
    //                    for (var i = 0; i < baseService.arrayLength($scope.receivedReconDataList); i++) {
    //                        $scope.receivedReconDataList[i].Flag = isExistInList($scope.receivedTempList, $scope.receivedReconDataList[i].VoucherDetailId);
    //                    }
    //                }
    //            }, function () {
    //                ShowResult(commonMessage.NetworkError, "failure");
    //            }).finally(function () {
    //            });
    //    };
    //    $scope.getReceivedReconData();
    //};

    $scope.bankCrReconList = [];
    $scope.bankCrTempList = [];
    //$scope.bankCrReconParameters = {
    //    limit: 10,
    //    offset: 0,
    //    order: "asc",
    //    sort: "PostingDate",
    //    searchBy: "PostingDate",
    //    pageSize: 10,
    //    total_count: 0,
    //    search: null,
    //    serverPagination: true
    //};
    $scope.bankCrReconDataListSyncfusion = [];
    $scope.getBankCrReconListSyncfusion = function () {
        try {
            $http({
                method: 'POST',
                url: $scope.path + "GetBankCrReconListSyncfusion",
                data: {
                    bankMasterId: $scope.bankReconciliationNew.BankMasterId,
                    fromDate: $scope.bankReconciliationNew.FromDate,
                    toDate: $scope.bankReconciliationNew.ToDate
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                $scope.bankCrReconDataListSyncfusion = response.data.DATA;
            }),
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
        }

        catch (e) {

        }
    }
    //$scope.getBankCrReconList = function () {
    //    $scope.bankCrReconParameters.bankMasterId = $scope.bankReconciliationNew.BankMasterId;
    //    $scope.bankCrReconParameters.fromDate = $scope.bankReconciliationNew.FromDate;
    //    $scope.bankCrReconParameters.toDate = $scope.bankReconciliationNew.ToDate;
    //    $scope.getBankCrReconData = function (pageno) {
    //        baseService.paginationBase($scope.bankCrReconUrl, pageno, $scope.bankCrReconParameters)
    //            .then(function (result) {
    //                $scope.bankCrReconDataList = result.Rows;
    //                $scope.bankCrReconParameters.total_count = result.Total;
    //                if (baseService.arrayLength($scope.bankCrReconList) === 0) {
    //                    baseService.getDDLSearchColumn(result.Rows, $scope.bankCrReconList);
    //                   // $scope.bankCrAmmount = $scope.amountCalculate($scope.bankCrReconDataList);
    //                    $scope.bankCrReconAmount = $scope.bnkReconList[2].ReconciledValue;
                        
    //                }
    //                for (var i = 0; i < baseService.arrayLength($scope.bankCrReconDataList); i++) {
    //                    $scope.bankCrReconDataList[i].Flag = isExistInList($scope.bankCrTempList, $scope.bankCrReconDataList[i].VoucherDetailId);
    //                }
    //            }, function () {
    //                ShowResult(commonMessage.NetworkError, "failure");
    //            }).finally(function () {
    //            });
    //    };
    //    $scope.getBankCrReconData();
    //};

    $scope.bankDrReconList = [];
    $scope.bankDrTempList = [];
    //$scope.bankDrReconParameters = {
    //    limit: 10,
    //    offset: 0,
    //    order: "asc",
    //    sort: "PostingDate",
    //    searchBy: "PostingDate",
    //    pageSize: 10,
    //    total_count: 0,
    //    search: null,
    //    serverPagination: true
    //};
    $scope.bankDrReconDataList = [];
    $scope.getBankDrReconList = function () {
        try {
            $http({
                method: 'POST',
                url: $scope.path + "GetBankDrReconListUploadedData",
                data: {
                    bankMasterId: $scope.bankReconciliationNew.BankMasterId,
                    fromDate: $scope.bankReconciliationNew.FromDate,
                    toDate: $scope.bankReconciliationNew.ToDate
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                $scope.bankDrReconDataList = response.data.DATA;
            }),
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
        }

        catch (e) {

        }
    }
    //$scope.getBankDrReconList = function () {
    //    $scope.bankDrReconParameters.cutOffDate = $scope.cutOffDate;
    //    $scope.bankDrReconParameters.bankMasterId = $scope.bankReconciliationNew.BankMasterId;
    //    $scope.bankDrReconParameters.fromDate = $scope.bankReconciliationNew.FromDate;
    //    $scope.bankDrReconParameters.toDate = $scope.bankReconciliationNew.ToDate;
    //    $scope.getBankDrReconData = function (pageno) {
    //        baseService.paginationBase($scope.bankDrReconUrl, pageno, $scope.bankDrReconParameters)
    //            .then(function (result) {
    //                $scope.bankDrReconDataList = result.Rows;
    //                $scope.bankDrReconParameters.total_count = result.Total;
    //                if (baseService.arrayLength($scope.bankDrReconList) === 0) {
    //                    baseService.getDDLSearchColumn(result.Rows, $scope.bankDrReconList);
    //                    //$scope.bankDrAmmount = $scope.amountCalculate($scope.bankDrReconDataList);
    //                     $scope.bankDrReconAmount = $scope.bnkReconList[4].ReconciledValue;
                        
    //                }
    //                for (var i = 0; i < baseService.arrayLength($scope.bankDrReconDataList); i++) {
    //                    $scope.bankDrReconDataList[i].PostingDate = $filter("dateFiltering")($scope.bankDrReconDataList[i].PostingDate);
    //                    $scope.bankDrReconDataList[i].Flag = isExistInList($scope.bankDrTempList, $scope.bankDrReconDataList[i].VoucherDetailId);
    //                }
    //            }, function () {
    //                ShowResult(commonMessage.NetworkError, "failure");
    //            }).finally(function () {
    //            });
    //    };
    //    $scope.getBankDrReconData();
    //};

    //$scope.amountCalculate = function (list) {
    //    try {
    //        var amount = 0;
    //        for (var i = 0; i < list.length; i++) {
    //            amount = amount + parseInt(list[i].Amount);
    //        }
    //        return amount;
    //    } catch (e) {
    //        ShowResult(e, "failure");
    //    }
    //};
    $scope.bankDrReconAmount = 0;
    $scope.isReconciledBankDrReconAmount = function (event, data, i, variable) {
        try {
            if (event.currentTarget.checked)
                $scope.bankDrReconAmount = Math.round(($scope.bankDrReconAmount + parseFloat(data.Amount)) * 100 + Number.EPSILON) / 100;
            else
                $scope.bankDrReconAmount = Math.round(($scope.bankDrReconAmount - parseFloat(data.Amount)) * 100 + Number.EPSILON) / 100;
            
        } catch (e) {
            ShowResult(e, "failure");
        }
    };
    $scope.bankDrReconUploadedDataAmount = 0;
    $scope.isReconciledBankDrReconUploadedDataAmount = function (event, data, i, variable) {
        try {
            if (event.currentTarget.checked)
                $scope.bankDrReconUploadedDataAmount = Math.round(($scope.bankDrReconUploadedDataAmount + parseFloat(data.DrAmount)) * 100 + Number.EPSILON) / 100;
            else
                $scope.bankDrReconUploadedDataAmount = Math.round(($scope.bankDrReconUploadedDataAmount - parseFloat(data.DrAmount)) * 100 + Number.EPSILON) / 100;

        } catch (e) {
            ShowResult(e, "failure");
        }
    };


    $scope.listMerge= function (event, data) {
        try {
            if (event.currentTarget.checked) {
                $scope.tempList.push({
                    VoucherDetailId: data.VoucherDetailId
                    , ReconcileDate: data.ReconcileDate
                    , DrAmount: data.DrAmount
                    , CrAmount: data.CrAmount
                    , BankMasterId: data.BankMasterId
                    , ReconcileDate: data.EncashmentDate
                })
            }
            else {
                var i = $scope.tempList.length;
                while (i--) {
                    if ($scope.tempList[i]["VoucherDetailId"] === data.VoucherDetailId) {
                        $scope.tempList.splice(i, 1);
                    }
                }
            }


        } catch (e) {
            ShowResult(e, "failure");
        }
    }

    //$scope.encashmentDateUpdate = function (data) {
    //    try {
    //        for (var i = 0; i < $scope.tempList.length; i++) {
    //            if ($scope.tempList[i].VoucherDetailId == data.VoucherDetailId) {
    //                $scope.tempList[i].ReconcileDate = $filter("dateFiltering")(data.EncashmentDate)
    //            }
    //        }
    //    } catch (e) {
    //        ShowResult(e, "failure");
    //    }
    //}


    //function isExistInList(list, id) {
    //    try {
    //        for (var i = 0; i < baseService.arrayLength(list); i++) {
    //            if (list[i].VoucherDetailId === id)
    //                return true;
    //        }
    //        return false;
    //    } catch (e) {
    //        ShowResult(e, "failure");
    //    }
    //}


    $scope.clear = function () {
        //$scope.bnkReconList = [];
        //$scope.issuedReconList = [];
        //$scope.receivedReconList = [];
        $scope.bankCrReconList = [];
        $scope.bankDrReconList = [];
    }
    $scope.invalidDocDate = false;
    $scope.checkDocDate = function () {
        var msg = "";
        //if (new Date($scope.voucher.ToDate) > new Date()) {
        //    $scope.invalidDocDate = true;
        //    msg = "Doc date must be below or equal to current Date!";
        //}
        if (new Date($scope.bankReconciliationNew.ToDate) < new Date($scope.bankReconciliationNew.FromDate)) {
            msg = "To date must be below or equal to From Date!";
            $scope.invalidDocDate = true;
        }
        
        else $scope.invalidDocDate = false;
        
        return manualValidation("div_ToDate", $scope.invalidDocDate, msg);
    };
    //$scope.CRREconcileReport = function () {
    //    try {
            
    //        var file_src = 'banks/bankreconciliation/CRReconcileReport?BankMasterID=' + $scope.bankCrReconParameters.bankMasterId + '&fromDate=' + $scope.bankCrReconParameters.fromDate + '&toDate=' + $scope.bankCrReconParameters.toDate + '&bankReconciliation=' + $scope.bankReconciliationNew
    //        $rootScope.report(file_src);
      
    //    } catch (e) {

    //    }
    //}

  
    //$scope.DRREconcileReport = function () {
    //    try {

    //        var file_src = 'banks/bankreconciliation/DRReconcileReport?BankMasterID=' + $scope.bankCrReconParameters.bankMasterId + '&fromDate=' + $scope.bankCrReconParameters.fromDate + '&toDate=' + $scope.bankCrReconParameters.toDate + '&cutOffDate' + $scope.bankDrReconParameters.cutOffDate
    //        $rootScope.report(file_src);

    //    } catch (e) {

    //    }
    //}
    //$scope.delete = function (bankReconciliationId) {
    //    $http({
    //        method: "POST",
    //        url: $scope.deleteUrl,
    //        data: {
    //            "bankReconciliationId": bankReconciliationId
    //        },
    //        dataType: "JSON"
    //    }).then(function successCallback(response) {
    //        if (response.data.Error === true) {
    //            ShowResult(response.data.Message, "failure");
    //        }
    //        else {
    //            ShowResult(response.data.Message, "success");
    //            $scope.getData();
    //            $scope.bankReconciliationId = null;
               
    //        }
    //    }, function errorCallback(response) {
    //        ShowResult(response.status.Message, "failure");
    //    });
    //    return true;
    //};

    //$scope.bankReconciliationId = null;
    //$scope.confirmDelete = function (bankReconciliationId) {
    //    $scope.bankReconciliationId = bankReconciliationId;
    //    $scope.message_delete_confirmation = "Are you sure to Delete?";
    //    angular.element(document.querySelector("#confirmDeletePopUp")).modal("show");
    //};

    //$scope.invoiceSearchList = [
    //    {
    //        "Text": "BankRefNo",
    //        "Value": "BankRefNo"
    //    },
    //    {
    //        "Text": "BankParticulars",
    //        "Value": "BankParticulars"
    //    },
    //    {
    //        "Text": "OwnRefNo",
    //        "Value": "OwnRefNo"
    //    },
    //    {
    //        "Text": "BankStatementDate",
    //        "Value": "BankStatementDate"
    //    },
    //    {
    //        "Text": "Id",
    //        "Value": "Id"
    //    }
    //];

    //$scope.invoiceParameters = {
    //    limit: 10,
    //    offset: 0,
    //    order: "ASC",
    //    sort: "BankRefNo",
    //    searchBy: "BankRefNo",
    //    pageSize: 10,
    //    total_count: 0,
    //    search: null,
    //    serverPagination: true
    //};
    //var VoucherDetailId = null;
    //$scope.invoiceList = [];
    //$scope.voucherDetailList = [];
    //$scope.getBankReconciliationUploadedDataPopUp = function (data) {
    //    VoucherDetailId = data.VoucherDetailId;
    //    $scope.getBankReconciliationUploadedData = function (pageno) {
    //        $scope.bankReconciliationUploadedDataLUrl1 = "banks/bankreconciliation/GetAvailableBankReconciliationUploadedDataList?bankMasterId=" + $scope.bankReconciliationNew.BankMasterId + '&fromDate=' + $scope.bankReconciliationNew.FromDate + '&toDate=' + $scope.bankReconciliationNew.ToDate  ;
    //        baseService.paginationBase($scope.bankReconciliationUploadedDataLUrl1, pageno, $scope.invoiceParameters)
    //            .then(function (result) {
    //                try {
    //                    $scope.invoiceList = result.Rows;
    //                    $scope.invoiceParameters.total_count = result.Total;
    //                } catch (e) {
    //                    ShowResult(e, "Error");
    //                }
    //            }, function () {
    //                ShowResult(commonMessage.NetworkError, "failure");
    //            }).finally(function () {
    //            });
    //    };
    //    angular.element(document.querySelector("#vendorInvoicePopUp")).modal("show");
    //    $scope.getBankReconciliationUploadedData();
    //};

    //$scope.closePopUp = function () {
    //    angular.element(document.querySelector("#vendorInvoicePopUp")).modal("hide");
    //};

    //$scope.closeInvoicePopUpselected = function () {
    //    angular.forEach($scope.invoiceList, function (data, i) {
    //        if (data.Active === true) {
    //            data.VoucherDetailId = VoucherDetailId;
                
    //            var getRow = null;
    //            getRow = $filter("filter")($scope.voucherDetailList, {"Id": data.Id });
    //            if (getRow.length === 0) {
    //                $scope.voucherDetailList.push(data);
    //                angular.element(document.querySelector("#vendorInvoicePopUp")).modal("hide");
    //            }
    //            else {
    //                ShowResult(data.Id + " already  Exist", "failure", "vendorInvoicePopUp");
    //            }
    //        }
    //    });
    //};
    //$scope.removeRow = function (index, data) {
    //    $scope.voucherDetailList.splice(index, 1);
    //};
    $scope.bankDrReconUploadedDataList = [];
    $scope.getBankReconciliationUploadedDrData = function () {
        try {
            $http({
                method: 'POST',
                url: $scope.path + "GetAvailableBankReconciliationUploadedDrDataList",
                data: {
                    bankMasterId: $scope.bankReconciliationNew.BankMasterId,
                    fromDate: $scope.bankReconciliationNew.FromDate,
                    toDate: $scope.bankReconciliationNew.ToDate
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                $scope.bankDrReconUploadedDataList = response.data.DATA;
            }),
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
        }

        catch (e) {

        }
    }
}