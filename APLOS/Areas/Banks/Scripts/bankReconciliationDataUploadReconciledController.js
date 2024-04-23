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
        DrAmount: null,
        CrAmount: null,
        FromDate: $filter("dateFiltering")(Date.now()),
        ToDate: $filter("dateFiltering")(Date.now())
    };
    $scope.bankReconciliationNew = Object.assign({}, $scope.bankReconciliation);

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
            $scope.clear();
        }
    }
    

    $scope.tab = 1;
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
            $scope.getBankDrReconciledList();
            $scope.getBankCrReconList();
            $scope.getBankReconciliationUploadedCrData();
            $scope.getBankCrReconciledList();
            $scope.clear();
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.SaveAdjustmentJournal = function () {
        try {
            $scope.saveBtnDisable = true;
             $http({
                    method: "POST",
                    url: $scope.path + "SaveAdjustmentJournalBankReconciliationMap",
                    data: {
                        "bankReconciliation": $scope.bankReconciliation
                        , "bankReconciliationList": $scope.TempList
                    },
                    dataType: "JSON"
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        $scope.saveBtnDisable = false;
                        ShowResult(response.data.Message, "failure");
                    }
                    else {
                        $scope.getBnkReconList();
                        $scope.saveBtnDisable = false;
                        ShowResult(response.data.Message, "success");
                    }
                });
            

        } catch (e) {
            $scope.saveBtnDisable = false;
            ShowResult(e, "failure");
        }
    };

    $scope.saveBtnDisable = false;
    $scope.Save = function () {
        try {
                checkTotalAmount();
            $scope.listMergeTempList();
            if ($scope.bankDrTempList.length > 1 && $scope.bankDrUploadedDataTempList.length > 1) {
                throw "Both site multiple not allowed,Please check One site one Dr. Reconcile Pending or other site multiple .!";
            }

            if ($scope.bankCrTempList.length > 1 && $scope.bankCrUploadedDataTempList.length > 1) {
                throw "Both site multiple not allowed,Please check One site one Cr. Reconcile Pending or other site multiple .!";
            }
            if ($scope.TempList.length === 0) {
                throw "Please check at least one Reconcile Pending .!";
            }
                

            angular.copy($scope.bankReconciliationNew, $scope.bankReconciliation);
            if (bankDrReconDifferenceAmount > 0 && bankDrReconDifferenceAmount <= 2) {
                $scope.message_confirmation = "Do you want to adjust? adjust amount is " + bankDrReconDifferenceAmount;
                angular.element(document.querySelector("#confirmSavePopUp")).modal("show");
            }
            else {
                $scope.saveBtnDisable = true;
                $http({
                    method: "POST",
                    url: $scope.path + "SaveBankReconciliationMap",
                    data: {
                        "bankReconciliation": $scope.bankReconciliation
                        , "bankReconciliationList": $scope.TempList
                    },
                    dataType: "JSON"
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        $scope.saveBtnDisable = false;
                        ShowResult(response.data.Message, "failure");
                    }
                    else {
                        $scope.getBnkReconList();
                        $scope.saveBtnDisable = false;
                        ShowResult(response.data.Message, "success");
                    }
                });
            }
                
        } catch (e) {
            $scope.saveBtnDisable = false;
            ShowResult(e, "failure");
        }
    };
    var bankDrReconDifferenceAmount = 0;
    function checkTotalAmount() {
        bankDrReconDifferenceAmount = 0;
        if (parseFloat($scope.bankDrReconAmount) > parseFloat($scope.bankDrReconUploadedDataAmount)) {
            bankDrReconDifferenceAmount = (parseFloat($scope.bankDrReconAmount) - parseFloat($scope.bankDrReconUploadedDataAmount)).toFixed(2);
            $scope.bankReconciliationNew.DrAmount = bankDrReconDifferenceAmount;
        }
         if (parseFloat($scope.bankDrReconAmount) < parseFloat($scope.bankDrReconUploadedDataAmount)) {
             bankDrReconDifferenceAmount = (parseFloat($scope.bankDrReconUploadedDataAmount) - parseFloat($scope.bankDrReconAmount)).toFixed(2);
             $scope.bankReconciliationNew.CrAmount = bankDrReconDifferenceAmount;
        }
         if (parseFloat($scope.bankCrReconAmount) < parseFloat($scope.bankCrReconUploadedDataAmount)) {
             bankDrReconDifferenceAmount = (parseFloat($scope.bankCrReconUploadedDataAmount) - parseFloat($scope.bankCrReconAmount)).toFixed(2);
             $scope.bankReconciliationNew.DrAmount = bankDrReconDifferenceAmount;
        }
         if (parseFloat($scope.bankCrReconAmount) > parseFloat($scope.bankCrReconUploadedDataAmount)) {
             bankDrReconDifferenceAmount = (parseFloat($scope.bankCrReconAmount) - parseFloat($scope.bankCrReconUploadedDataAmount)).toFixed(2);
             $scope.bankReconciliationNew.CrAmount = bankDrReconDifferenceAmount;
        }
            
        if ((parseFloat($scope.bankDrReconAmount) !== parseFloat($scope.bankDrReconUploadedDataAmount)) && (bankDrReconDifferenceAmount > 2 || bankDrReconDifferenceAmount<0))
            throw "Bank Dr reconciled total amount must be equal Bank Dr reconciled Uploaded total amount.!";
        if ((parseFloat($scope.bankCrReconAmount) !== parseFloat($scope.bankCrReconUploadedDataAmount)) && (bankDrReconDifferenceAmount > 2 || bankDrReconDifferenceAmount < 0))
            throw "Bank Cr reconciled total amount must be equal Bank Cr reconciled Uploaded total amount.!";
    }

    $scope.bankDrTempList = [];
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


    $scope.listMergebankDrTempList= function (event, data) {
        try {
            if (event.currentTarget.checked) {
                $scope.bankDrTempList.push({
                    BankReconciliationUploadedDataId: ""
                    ,VoucherDetailId: data.VoucherDetailId
                    , GLTransactionDetailId: data.GLTransactionDetailId
                })
            }
            else {
                var i = $scope.bankDrTempList.length;
                while (i--) {
                    if ($scope.bankDrTempList[i]["VoucherDetailId"] === data.VoucherDetailId) {
                        $scope.bankDrTempList.splice(i, 1);
                    }
                }
            }


        } catch (e) {
            ShowResult(e, "failure");
        }
    }
    $scope.bankDrUploadedDataTempList = [];
    $scope.listMergebankDrUploadedDataTempList = function (event, data) {
        try {
            if (event.currentTarget.checked) {
                $scope.bankDrUploadedDataTempList.push({
                    BankReconciliationUploadedDataId: data.Id
                    , VoucherDetailId: ""
                    , GLTransactionDetailId: ""
                })
            }
            else {
                var i = $scope.bankDrUploadedDataTempList.length;
                while (i--) {
                    if ($scope.bankDrUploadedDataTempList[i]["BankReconciliationUploadedDataId"] === data.Id) {
                        $scope.bankDrUploadedDataTempList.splice(i, 1);
                    }
                }
            }


        } catch (e) {
            ShowResult(e, "failure");
        }
    }
    $scope.TempList = [];
    $scope.listMergeTempList = function () {
        try {
            $scope.TempList = [];
            if ($scope.bankDrTempList.length > 1 && $scope.bankDrUploadedDataTempList.length > 1) {
                ShowResult("Both site multiple not allowed,Please check One site one Dr. Reconcile Pending or other site multiple!", "failure");
                return;
            }
                
            if ($scope.bankCrTempList.length > 1 && $scope.bankCrUploadedDataTempList.length > 1) {
                ShowResult("Both site multiple not allowed,Please check One site one Cr. Reconcile Pending or other site multiple!", "failure");
                return;
            }
                
           
            if ($scope.bankDrTempList.length > 1) {
                for (var i = 0; i < $scope.bankDrTempList.length; i++) {
                    $scope.TempList.push({
                        BankReconciliationUploadedDataId: $scope.bankDrUploadedDataTempList[0].BankReconciliationUploadedDataId
                        , VoucherDetailId: $scope.bankDrTempList[i].VoucherDetailId
                        , GLTransactionDetailId: $scope.bankDrTempList[i].GLTransactionDetailId

                    })
                }
            }
            else if ($scope.bankDrUploadedDataTempList.length > 1) {
                for (var i = 0; i < $scope.bankDrUploadedDataTempList.length; i++) {
                    $scope.TempList.push({
                        BankReconciliationUploadedDataId: $scope.bankDrUploadedDataTempList[i].BankReconciliationUploadedDataId
                        , VoucherDetailId: $scope.bankDrTempList[0].VoucherDetailId
                        , GLTransactionDetailId: $scope.bankDrTempList[0].GLTransactionDetailId

                    })
                }
            }
            else if ($scope.bankCrTempList.length > 1) {
                for (var i = 0; i < $scope.bankCrTempList.length; i++) {
                    $scope.TempList.push({
                        BankReconciliationUploadedDataId: $scope.bankCrUploadedDataTempList[0].BankReconciliationUploadedDataId
                        , VoucherDetailId: $scope.bankCrTempList[i].VoucherDetailId
                        , GLTransactionDetailId: $scope.bankCrTempList[i].GLTransactionDetailId

                    })
                }
            }
            else if ($scope.bankCrUploadedDataTempList.length > 1) {
                for (var i = 0; i < $scope.bankCrUploadedDataTempList.length; i++) {
                    $scope.TempList.push({
                        BankReconciliationUploadedDataId: $scope.bankCrUploadedDataTempList[i].BankReconciliationUploadedDataId
                        , VoucherDetailId: $scope.bankCrTempList[0].VoucherDetailId
                        , GLTransactionDetailId: $scope.bankCrTempList[0].GLTransactionDetailId

                    })
                }
            }
            else {
                if ($scope.bankDrUploadedDataTempList.length > 0 && $scope.bankDrTempList.length > 0) {
                    $scope.TempList.push({
                        BankReconciliationUploadedDataId: $scope.bankDrUploadedDataTempList[0].BankReconciliationUploadedDataId
                        , VoucherDetailId: $scope.bankDrTempList[0].VoucherDetailId
                        , GLTransactionDetailId: $scope.bankDrTempList[0].GLTransactionDetailId

                    })
                }
                else if ($scope.bankCrUploadedDataTempList.length > 0 && $scope.bankCrTempList.length > 0) {
                    $scope.TempList.push({
                        BankReconciliationUploadedDataId: $scope.bankCrUploadedDataTempList[0].BankReconciliationUploadedDataId
                        , VoucherDetailId: $scope.bankCrTempList[0].VoucherDetailId
                        , GLTransactionDetailId: $scope.bankCrTempList[0].GLTransactionDetailId

                    })
                }
            }


        } catch (e) {
            ShowResult(e, "failure");
        }
    }

    


    $scope.clear = function () {
        $scope.bankDrReconAmount = 0;
        $scope.bankDrReconUploadedDataAmount = 0;
        $scope.bankDrTempList = [];
        $scope.bankDrUploadedDataTempList = [];
        $scope.bankCrReconAmount = 0;
        $scope.bankCrReconUploadedDataAmount = 0;
        $scope.bankCrTempList = [];
        $scope.bankCrUploadedDataTempList = [];
        $scope.TempList = [];
        $scope.saveBtnDisable = false;
        
        $scope.bankReconciliation.DrAmount = null;
        $scope.bankReconciliation.CrAmount = null;
        $scope.bankReconciliationNew.DrAmount = null;
        $scope.bankReconciliationNew.CrAmount = null;
        
    }
    $scope.invalidDocDate = false;
    $scope.checkDocDate = function () {
        var msg = "";
        if (new Date($scope.bankReconciliationNew.ToDate) > new Date()) {
            $scope.invalidDocDate = true;
            msg = "Doc date must be below or equal to current Date!";
        }
        if (new Date($scope.bankReconciliationNew.ToDate) < new Date($scope.bankReconciliationNew.FromDate)) {
            msg = "To date must be below or equal to From Date!";
            $scope.invalidDocDate = true;
        }
        
        else $scope.invalidDocDate = false;
        
        return manualValidation("div_ToDate", $scope.invalidDocDate, msg);
    };
    
    $scope.bankDrReconUploadedDataList = [];
    $scope.getBankReconciliationUploadedDrData = function () {
        try {
            $http({
                method: 'POST',
                url: $scope.path + "GetAvailableBankReconciliationUploadedCrDataList",
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
    $scope.bankDrReconciledDataList = [];
    $scope.getBankDrReconciledList = function () {
        try {
            $http({
                method: 'POST',
                url: $scope.path + "GetBankDrReconciledList",
                data: {
                    bankMasterId: $scope.bankReconciliationNew.BankMasterId,
                    fromDate: $scope.bankReconciliationNew.FromDate,
                    toDate: $scope.bankReconciliationNew.ToDate
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                $scope.bankDrReconciledDataList = response.data.DATA;
            }),
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
        }

        catch (e) {

        }
    }
    $scope.CRREconcileReport = function () {
        try {

            var file_src = 'banks/bankreconciliation/CRReconcilePendingReport?bankMasterId=' + $scope.bankReconciliationNew.BankMasterId + '&fromDate=' + $scope.bankReconciliationNew.FromDate + '&toDate=' + $scope.bankReconciliationNew.ToDate 
            $rootScope.report(file_src);

        } catch (e) {

        }
    }


    $scope.DRREconcileReport = function () {
        try {

            var file_src = 'banks/bankreconciliation/DRReconcilePendingReport?BankMasterID=' + $scope.bankReconciliationNew.BankMasterId + '&fromDate=' + $scope.bankReconciliationNew.FromDate + '&toDate=' + $scope.bankReconciliationNew.ToDate 
            $rootScope.report(file_src);

        } catch (e) {

        }
    }
    $scope.bankCrTempList = [];
    $scope.bankCrReconDataList = [];
    $scope.getBankCrReconList = function () {
        try {
            $http({
                method: 'POST',
                url: $scope.path + "GetBankCrReconListUploadedData",
                data: {
                    bankMasterId: $scope.bankReconciliationNew.BankMasterId,
                    fromDate: $scope.bankReconciliationNew.FromDate,
                    toDate: $scope.bankReconciliationNew.ToDate
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                $scope.bankCrReconDataList = response.data.DATA;
            }),
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
        }

        catch (e) {

        }
    }

    $scope.bankCrReconAmount = 0;
    $scope.isReconciledBankCrReconAmount = function (event, data, i, variable) {
        try {
            if (event.currentTarget.checked)
                $scope.bankCrReconAmount = Math.round(($scope.bankCrReconAmount + parseFloat(data.Amount)) * 100 + Number.EPSILON) / 100;
            else
                $scope.bankCrReconAmount = Math.round(($scope.bankCrReconAmount - parseFloat(data.Amount)) * 100 + Number.EPSILON) / 100;

        } catch (e) {
            ShowResult(e, "failure");
        }
    };
    $scope.bankCrReconUploadedDataAmount = 0;
    $scope.isReconciledBankCrReconUploadedDataAmount = function (event, data, i, variable) {
        try {
            if (event.currentTarget.checked)
                $scope.bankCrReconUploadedDataAmount = Math.round(($scope.bankCrReconUploadedDataAmount + parseFloat(data.CrAmount)) * 100 + Number.EPSILON) / 100;
            else
                $scope.bankCrReconUploadedDataAmount = Math.round(($scope.bankCrReconUploadedDataAmount - parseFloat(data.CrAmount)) * 100 + Number.EPSILON) / 100;

        } catch (e) {
            ShowResult(e, "failure");
        }
    };


    $scope.listMergebankCrTempList = function (event, data) {
        try {
            if (event.currentTarget.checked) {
                $scope.bankCrTempList.push({
                    BankReconciliationUploadedDataId: ""
                    , VoucherDetailId: data.VoucherDetailId
                    , GLTransactionDetailId: data.GLTransactionDetailId
                })
            }
            else {
                var i = $scope.bankCrTempList.length;
                while (i--) {
                    if ($scope.bankCrTempList[i]["VoucherDetailId"] === data.VoucherDetailId) {
                        $scope.bankCrTempList.splice(i, 1);
                    }
                }
            }


        } catch (e) {
            ShowResult(e, "failure");
        }
    }
    $scope.bankCrUploadedDataTempList = [];
    $scope.listMergebankCrUploadedDataTempList = function (event, data) {
        try {
            if (event.currentTarget.checked) {
                $scope.bankCrUploadedDataTempList.push({
                    BankReconciliationUploadedDataId: data.Id
                    , VoucherDetailId: ""
                    , GLTransactionDetailId: ""
                })
            }
            else {
                var i = $scope.bankCrUploadedDataTempList.length;
                while (i--) {
                    if ($scope.bankCrUploadedDataTempList[i]["BankReconciliationUploadedDataId"] === data.Id) {
                        $scope.bankCrUploadedDataTempList.splice(i, 1);
                    }
                }
            }


        } catch (e) {
            ShowResult(e, "failure");
        }
    }
    $scope.bankCrReconUploadedDataList = [];
    $scope.getBankReconciliationUploadedCrData = function () {
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
                $scope.bankCrReconUploadedDataList = response.data.DATA;
            }),
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
        }

        catch (e) {

        }
    }
    $scope.bankCrReconciledDataList = [];
    $scope.getBankCrReconciledList = function () {
        try {
            $http({
                method: 'POST',
                url: $scope.path + "GetBankCrReconciledList",
                data: {
                    bankMasterId: $scope.bankReconciliationNew.BankMasterId,
                    fromDate: $scope.bankReconciliationNew.FromDate,
                    toDate: $scope.bankReconciliationNew.ToDate
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                $scope.bankCrReconciledDataList = response.data.DATA;
            }),
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
        }

        catch (e) {

        }
    }
    $scope.onClickDeletePopUp = function (x) {
        var data = x;
        $scope.voucherDetailId = data.VoucherDetailId;
        $scope.message_delete_confirmation = "Are you sure to Delete?";
        angular.element(document.querySelector('#confirmDeletePopUp')).modal('show');
    };
    $scope.delete = function (voucherDetailId) {
        $http({
            method: "POST",
            url: $scope.path + 'DeleteBankReconciliationMapData',
            data: {
                "voucherDetailId": voucherDetailId
            },
            dataType: "JSON"
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, "failure");
            }
            else {
                ShowResult(response.data.Message, "success");
                $scope.getBnkReconList();
            }
        }, function errorCallback(response) {
            ShowResult(response.status.Message, "failure");
        });
        return true;
    };
}