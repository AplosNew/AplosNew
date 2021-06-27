'use strict';
ExtraPaymentVoucherController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'toaster'];
function ExtraPaymentVoucherController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, toaster) {
    $rootScope.title = 'Voucher';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.vendorpayments = [];
    $scope.voucherDetails = [];
    $scope.path = 'accounts/voucher/';
    $scope.saveUrl = $scope.path + 'voucherpaymentcreate';
    $scope.updateUrl = $scope.path + 'voucherpaymentupdate';
    $scope.deleteUrl = $scope.path + 'delete/';
    $scope.getListUrl = $scope.path + 'getvendorinvoicepaymentdata';
    baseService.init($scope.getListUrl, null, null, null, 'VoucherNo', 'VoucherNo');
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.vendorpayments = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.getData();
    $scope.searchByGLList = [

        {
            'name': 'Voucher No',
            'value': 'VoucherNo'
        },
        {
            'name': 'Voucher Date',
            'value': 'VoucherDate'
        }
        ,
        {
            'name': 'DocRefNo',
            'value': 'DocRefNo'
        }
        ,
        {
            'name': 'DocDate',
            'value': 'DocDate'
        }
        ,
        {
            'name': 'Vendor',
            'value': 'Vendor'
        }
    ];
    $scope.voucher = {
        Id: null,
        CurrencyId: null,
        VoucherTypeId: null,
        Type: 'Payment Voucher',
        TransactionRefNo: null,
        CompanyId: null,
        VoucherNo: null,
        VoucherDate: $filter('date')(Date.now(), 'dd-MMM-yyyy'),
        PostingDate: $filter('date')(Date.now(), 'dd-MMM-yyyy'),
        DocRefNo: null,
        DocDate: $filter('date')(Date.now(), 'dd-MMM-yyyy'),
        Narration: null,
        Active: true,
        PartyId: null,
        VendorInvoicePaymentId: null
    };

    $scope.voucherTypeList = [];
    $scope.tranCurrencyList = [];

    cboService.getCboTransactionCurrencyByCompany('', function (result) {
        $scope.tranCurrencyList = result;
    });

    $http({
        method: 'GET',
        url: 'accounts/vouchertype/getvouchertypecbo'
    }).then(function successCallback(response) {
        $scope.voucherTypeList = response.data;
        $scope.voucher.VoucherTypeId = $scope.voucherTypeList[0].Value;
        $scope.getprefix($scope.voucher.VoucherTypeId);
    });
    $scope.prefix = null;
    $scope.getprefix = function (item) {
        $http({
            method: 'get',
            url: 'accounts/VoucherType/GetPrefix?id=' + item
        }).then(function successCallback(response) {
            $scope.prefix = response.data;
        });
    };
    $scope.closebankListPopUp = function () {
        angular.element(document.querySelector('#baknListPopUp')).modal('hide');
    };
    $scope.closeVendorListPopUp = function () {
        angular.element(document.querySelector('#VendorListPopUp')).modal('hide');
    }
    $scope.closeVendorPayableListPopUp = function () {
        angular.element(document.querySelector('#VendorPayableListPopUp')).modal('hide');
    };
    $scope.closeVendorPayableEditListPopUp = function () {
        angular.element(document.querySelector('#VendorPayableEditListPopUp')).modal('hide');
    };
    $scope.closeBankListPopUpSelected = function () {
        if ($scope.bankidSelected !== null) {
            angular.element(document.querySelector('#baknListPopUp')).modal('hide');
        } else {
            angular.element(document.querySelector('#cancelPopUp')).modal('show');
        }
    };
    $scope.closeVendorListPopUpSelected = function () {
        if ($scope.vendorSelected !== null) {
            angular.element(document.querySelector('#VendorListPopUp')).modal('hide');
        } else {
            angular.element(document.querySelector('#cancelPopUp')).modal('show');
        }
    }

    $scope.closeVendorPayableListPopUpSelected = function () {
        $scope.VendorPayablePaymentList = [];
        angular.forEach($scope.vendorpayableList, function (item) {
            if (item.Active) {
                $scope.VendorPayablePaymentList.push(
                    item
                );
            }
        });
        if ($scope.VendorPayablePaymentList.length > 0) {
            angular.element(document.querySelector('#VendorPayableListPopUp')).modal('hide');
            $scope.selectedPayableTblShow = true;
        } else {
            angular.element(document.querySelector('#cancelPopUp')).modal('show');
        }
    };

    $scope.closeVendorPayableEditListPopUpSelected = function () {
        if ($scope.VendorPayablePaymentEditableList.Active) {
            $scope.VendorPayablePaymentList[$scope.editIndex] = $scope.VendorPayablePaymentEditableList;
        }

        angular.element(document.querySelector('#VendorPayableEditListPopUp')).modal('hide');
    };
    $scope.passNarration = function (narration) {
        $scope.voucherDetail.Narration = narration;
    };
    $scope.voucherDetail = {
        Id: null,
        GL: null,
        DocRefNo: null,
        DocDate: null,
        CrAmount: null,
        Narration: null,
        GLGeneralInfoId: null,
        CurrencyId: null,
        Active: true,
        AddedBy: null,
        AddedDate: $filter('date')(Date.now(), 'yyyy-MM-dd'),
        AddedFromIP: null,
        UpdatedBy: null,
        UpdatedDate: $filter('date')(Date.now(), 'yyyy-MM-dd'),
        UpdatedFromIP: null
    };
    $scope.setBankCodeSelected = function (x) {
        $scope.bankidSelected = x.Id;
        $scope.getBankName = x.Bank;
        $scope.getBankBranchName = x.BankBranch;
        $scope.getBankGlCode = x.COAItemCode;
        $scope.getAccountNumber = x.AccountNumber;
        $scope.getBankAccountType = x.BankAccountType;
        $scope.getBankGlItem = x.GLItem;
        $scope.getBankGlIId = x.GLGeneralInfoId;
        $scope.voucherDetail = {
            Id: $scope.voucherDetail.Id,
            GLGeneralInfoId: $scope.getBankGlIId,
            DocRefNo: $scope.voucher.DocRefNo,
            DocDate: $scope.voucher.DocDate,
            CrAmount: $scope.voucherDetail.CrAmount,
            Narration: $scope.voucherDetail.Narration,
            COAICode: $scope.getBankGlCode,
            CurrencyId: $scope.voucher.CurrencyId
        };
    }

    $scope.setVendorDataSelected = function (x) {
        $scope.vendorSelected = '';
        $scope.vendorSelectedId = x.Id;
        $scope.vendorSelected = x.PartyId;
        $scope.getVendorName = x.Party;
        $scope.getVendorCode = x.Code;
        $scope.getVendorGLItem = x.GLItem;
        $scope.getGlCode = x.COAICode;
    }

    // #region *************GET***************
    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.voucher = $scope.vendorpayments[$scope.index];
        $scope.voucher.VoucherDate = $filter('dateFiltering')($scope.voucher.VoucherDate);
        $scope.voucher.PostingDate = $filter('dateFiltering')($scope.voucher.PostingDate);
        $scope.voucher.DocDate = $filter('dateFiltering')($scope.voucher.DocDate);
        $scope.getvendorGL($scope.voucher.VendorInvoicePaymentId);
        $scope.getvoucherdeatilamount($scope.voucher.Id);
        $scope.getbankmasterdetailinvendorpayment($scope.voucher.Id);
        $scope.getvendorpayableGL($scope.voucher.PartyId, $scope.voucher.VendorInvoicePaymentId)
        //  $scope.getbankbyGL()
        $scope.Action = 'Update';
        $scope.editBtnShow = true;
        $scope.selectedPayableTblShow = true;
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.getvendorGL = function (id) {
        $http({
            method: 'GET',
            url: 'accounts/voucher/getvendorinvoicepaymenteditdata?vendorinvocepaymentid=' + id
        }).then(function successCallback(response) {
            $scope.setVendorDataSelected(response.data.Rows[0]);
        });
    }
    $scope.getvoucherdeatilamount = function (id) {
        $http({
            method: 'GET',
            url: 'accounts/voucher/GetVendorDatailPaymentCrAmount?voucherId=' + id
        }).then(function successCallback(response) {
            $scope.voucherDetail = response.data.Rows[0];
        });
    }
    $scope.Bankmasterdetail = [];
    $scope.getbankmasterdetailinvendorpayment = function (id) {
        $http({
            method: 'GET',
            url: 'accounts/voucher/GetBankMasterDetailInVendorPayment?voucherId=' + id
        }).then(function successCallback(response) {
            $scope.setBankCodeSelected(response.data.Rows[0]);
        });
    }
    $scope.getvendorpayableGL = function (id, invoicepaymentid) {
        $http({
            method: 'GET',
            url: 'accounts/voucher/GetVendorInvoiceParty?partyid=' + id + '&&vendorinvoicepamentid=' + invoicepaymentid
        }).then(function successCallback(response) {
            $scope.VendorPayablePaymentList = response.data.Rows;
        });
    }
    // #endregion

    // #region *************Check Date*****************
    $scope.postingDateMessage = '';
    $scope.checkPostingDate = function () {
        if (new Date($scope.voucher.PostingDate) > new Date()) {
            $scope.postingDateMessage = 'Posting date must be below or equal to current Date ';
            return false;
        }
        else if ($scope.voucher.PostingDate > $scope.voucher.DocDate) {
            $scope.postingDateMessage = 'Posting date must be below or equal to Doc Date ';
            return false;
        } else {
            $scope.postingDateMessage = '';
            return true;
        }
    }
    $scope.dateMessage = '';
    $scope.checkDate = function () {
        if (new Date($scope.voucher.DocDate) > new Date()) {
            $scope.dateMessage = 'Doc date must be below or equal to current Date ';
            return false;
        }
        else if ($scope.voucher.DocDate > $scope.voucher.VoucherDate) {
            $scope.dateMessage = 'Doc date must be below or equal to Voucher Date ';
            return false;
        } else {
            $scope.dateMessage = '';
            return true;
        }
    }
    $scope.checkVDockDate = function () {
        if (new Date($scope.voucherDetail.DocDate) > new Date()) {
            $scope.pop('error', 'Doc date must be below or equal to current Date ');
            return false;
        }
        else if ($scope.voucher.DocDate > $scope.voucher.VoucherDate) {
            $scope.pop('error', 'Doc date must be below or equal to Voucher Date');
            return false;
        } else {
            return true;
        }
    }
    $scope.VoucherDateMessage = '';
    $scope.checkVoucherDate = function () {
        if (new Date($scope.voucher.VoucherDate) > new Date()) {
            $scope.VoucherDateMessage = 'Voucher date must be below or equal to current Date ';
            return false
        }
        else if (new Date($scope.voucher.VoucherDate) < new Date()) {
            $scope.VoucherDateMessage = '';
            return true
        }
    }

    // #endregion

    // #region ************Bank*****************
    $scope.getBankList = [];
    $scope.searchBankByList = [
        {
            'name': 'Bank',
            'value': 'Bank'
        },
        {
            'name': 'BankBranch',
            'value': 'BankBranch'
        },
        {
            'name': 'AccountType',
            'value': 'BankAccountType'
        }
        ,
        {
            'name': 'AccountNumber',
            'value': 'AccountNumber'
        },
        {
            'name': 'GL Code',
            'value': 'COAItemCode'
        }
        ,
        {
            'name': 'GL',
            'value': 'GLItem'
        }
    ];
    $scope.bankGLParameters = {
        limit: 2,
        offset: 0,
        order: 'asc',
        sort: 'AccountNumber',
        searchBy: 'AccountNumber',
        pageSize: 2,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.getBankMasterVoucher = function (index) {
        $scope.rowSelectedIndex = index;
        $scope.bankGLUrl = 'banks/bankmaster/GetHouseBankBankMasterList';
        $scope.bankListData = function (pageno) {
            baseService.paginationBase($scope.bankGLUrl, pageno, $scope.bankGLParameters)
                .then(function (result) {
                    $scope.getBankList = result.Rows;
                    $scope.bankGLParameters.total_count = result.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        $scope.bankListData();
        angular.element(document.querySelector('#baknListPopUp')).modal('show');
    }

    // #endregion

    // #region *********** Vendor GL List **********************
    $scope.VendorList = [];
    $scope.searchVendorByList = [
        {
            'name': 'AccountType',
            'value': 'AccountType'
        },
        {
            'name': 'GL Code',
            'value': 'COAICode'
        },
        {
            'name': 'GL',
            'value': 'GLItem'
        },
        {
            'name': 'Vendor',
            'value': 'Party'
        }
    ];
    $scope.vendorGLParameters = {
        limit: 4,
        offset: 0,
        order: 'asc',
        sort: 'Party',
        searchBy: 'Party',
        pageSize: 4,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.getVendorParty = function (index) {
        $scope.rowSelectedIndex = index;
        $scope.vendorGLUrl = 'Parties/party/getinvoicevendordata';
        $scope.getvendorGLData = function (pageno) {
            baseService.paginationBase($scope.vendorGLUrl, pageno, $scope.vendorGLParameters)
                .then(function (result) {
                    $scope.VendorList = result.Rows;
                    $scope.vendorGLParameters.total_count = result.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        $scope.getvendorGLData();
        angular.element(document.querySelector('#VendorListPopUp')).modal('show');
    }
    //#endregion

    // #region ***************VendorPayableList*******************
    $scope.vendorpayableList = [];
    $scope.searchVendorPayableByList = [
        {
            'name': 'GL Code',
            'value': 'AccountType'
        },
        {
            'name': 'GL',
            'value': 'GLItem'
        },
        {
            'name': 'VoucherNo',
            'value': 'VoucherNo'
        },
        {
            'name': 'DocDate',
            'value': 'DocDate'
        },
        {
            'name': 'DocRefNo',
            'value': 'DocRefNo'
        }
    ];
    $scope.vendorPayableGLParameters = {
        limit: 6,
        offset: 0,
        order: 'asc',
        sort: 'VoucherNo',
        searchBy: 'VoucherNo',
        pageSize: 6,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.getPopenItemList = function () {
        $scope.vendorpayableGLData = function (pageno) {
            $scope.vendorpayableGLGLUrl = 'accounts/voucher/GetVendorInvoiceParty?partyid=' + $scope.vendorSelected;
            baseService.paginationBase($scope.vendorpayableGLGLUrl, pageno, $scope.vendorPayableGLParameters)
                .then(function (result) {
                    $scope.vendorpayableList = result.Rows;
                    //for (var i = 0; i <= $scope.vendorpayableList.length; i++) {
                    //    $scope.vendorpayableList[i].DrAmount = $scope.vendorpayableList[i].Balance
                    //}
                    $scope.vendorPayableGLParameters.total_count = result.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#VendorPayableListPopUp')).modal('show');
        $scope.vendorpayableGLData(null, $scope.vendorSelected);
    }

    // #endregion

    // #region ***************VendorPayableEdit****************
    $scope.VendorPayablePaymentEditableList = [];
    $scope.getvendorpayableEdit = function (data, index) {
        $scope.editIndex = index;
        $scope.pandingEditable = angular.copy(data);
        $scope.VendorPayablePaymentEditableList = $scope.pandingEditable;
        //$http({
        //    method: 'GET',
        //    url: 'accounts/voucher/GetVendorInvoiceParty?partyid=' + id + '&&vendorinvoicepamentid=' + invoicepaymentid,
        //}).then(function successCallback(response) {
        //    $scope.VendorPayablePaymentList = data.Rows;
        //});
        angular.element(document.querySelector('#VendorPayableEditListPopUp')).modal('show');
    }

    // #endregion

    // #region *****************Check Bank Amount and payment Amount*****************
    $scope.total = function () {
        $scope.Drtotal = 0;
        $scope.SplitAmount = 0;
        angular.forEach($scope.VendorPayablePaymentList, function (item) {
            $scope.Drtotal += item.DrAmount;
        });
    };
    $scope.checkCrAndDrEquealMsg = '';
    $scope.checkCrAndDrEqueal = function () {
        if ($scope.Drtotal === $scope.voucherDetail.CrAmount) {
            $scope.checkCrAndDrEquealMsg = '';
            return true;
        } else {
            ShowResult('Bank Amount  and Payment amount is not equeal', 'failure');
            return false;
        }
    }

    // #endregion

    // #region *******Bank/Vendor/Payment Check**********
    $scope.checkNullMsg = '';
    $scope.checkNullvendorMsg = '';
    $scope.checkNullpayableMsg = '';
    $scope.checkNullbank = function () {
        if (!$scope.bankidSelected === '') {
            $scope.checkNullMsg = '';
            return true;
        }
        else {
            ShowResult('Bank did not select', 'failure');
            return false
        }
    }

    $scope.checkNullvendor = function () {
        if (!$scope.vendorSelected === '') {
            $scope.checkNullvendorMsg = '';
            return true;
        }
        else {
            ShowResult('Vendor did not select', 'failure');
            return false
        }
    }

    $scope.checkNullvendorpayable = function () {
        if (!$scope.VendorPayablePaymentList === '') {
            $scope.checkNullpayableMsg = '';
            return true;
        }
        else {
            ShowResult('Payment Amount  Can not 0', 'failure');
            return false
        }
    }
    // #endregion

    $scope.Save = function () {
        $scope.vendorinvoicepayment = {
            Id: $scope.vendorSelectedId !== null ? $scope.vendorSelectedId : null,
            CurrencyId: $scope.voucher.CurrencyId,
            VoucherTypeId: $scope.voucher.VoucherTypeId,
            Type: 'Payment Voucher',
            VoucherNo: $scope.voucher.VoucherNo,
            VoucherDate: $scope.voucher.VoucherDate,
            PostingDate: $scope.voucher.PostingDate,
            DocRefNo: $scope.voucher.DocRefNo,
            DocDate: $scope.voucher.DocDate,
            Narration: $scope.voucher.Narration,
            PartyId: $scope.vendorSelected,
            Active: true,
            AddedBy: null,
            AddedDate: $filter('date')(Date.now(), 'yyyy-MM-dd'),
            AddedFromIP: null
        };
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.paymentvoucherForm.$valid && $scope.checkNullbank() && $scope.checkNullvendor() && $scope.checkNullvendorpayable() && $scope.checkCrAndDrEqueal()) {
            if ($scope.Action === 'Save') {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: { 'voucher': $scope.voucher, 'voucdetail': $scope.voucherDetail, 'prefix': $scope.prefix, 'vendorinvoicepayment': $scope.vendorinvoicepayment, 'vendorinvoicepamentdetail': $scope.VendorPayablePaymentList },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.voucher = response.data.Voucher;
                        ClearFields(response.data.Sequence);
                        // Show last voucher no.
                    }
                });
                return true;
            }
            else if ($scope.Action === 'Update') {
                $http({
                    method: 'POST',
                    url: $scope.updateUrl,
                    data: { 'voucher': $scope.voucher, 'voucdetail': $scope.voucherDetail, 'prefix': $scope.prefix, 'vendorinvoicepayment': $scope.vendorinvoicepayment, 'vendorinvoicepamentdetail': $scope.VendorPayablePaymentList },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -1) {
                            $scope.vouchers[$scope.index] = $scope.voucher;
                        }
                        ClearFields(response.data.Sequence);
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, 'failure');
                });
                return true;
            }
        }
    }
    $scope.Clear = function () {
        ClearFields();
        return true;
    }

    function ClearFields(seq) {
        $scope.Action = 'Save';
        $scope.voucher = {};
        $scope.voucherDetailrow = [];
        $scope.voucherDetail = [];
        $scope.Drtotal = 0;
        $scope.Crtotal = 0;
        $scope.voucher.Sequence = seq;
        $scope.voucher.Active = true;
    }

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };
}