'use strict';
CustomerConfirmationController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$window', "$controller"];
function CustomerConfirmationController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window, $controller) {
    $rootScope.title = 'Voucher GL Update';
    $scope.Action = 'Save';
    $scope.path = 'Accounts/VoucherGlUpdate/';
    $scope.url = "Accounts/VoucherGlUpdate";
    $scope.parkUrl = $scope.url + "/parkModeVoucher";
    $scope.saveUrl = $scope.path + 'UpdateInvoice';
    $controller("bankBaseController", { $scope: $scope, $http: $http });


    $scope.report = {
        PaymentStatus: null,
        FromDate: $filter("dateFiltering")(Date.now()),
        ToDate: $filter("dateFiltering")(Date.now())
    };
    $scope.paymentStatusList = [
        {
            "Text": "Pending",
            "Value": "Pending"
        }
        //,{
        //    "Text": "ALL",
        //    "Value": "ALL"
        //}
    ];


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
    $scope.customerInvoiceList = [];
    $scope.getcustomerInvoiceList = function () {
        try {
            $http({
                method: 'POST',
                url: $scope.path + "GetcustomerInvoiceList",
                data: {
                    CustomerSelectedList: NewCustomerSelectedList,
                    fromDate: $scope.report.FromDate,
                    toDate: $scope.report.ToDate,
                    paymentStatus: $scope.report.PaymentStatus
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                $scope.customerInvoiceList = response.data.DATA;
            }),
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
        }

        catch (e) {

        }
    }
    $scope.voucherDetailList = [];
    $scope.pushInTempListforConfirm = function (event, data) {
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

    $scope.Save = function () {
        if ($scope.voucherDetailList.length == 0) {
            ShowResult('Please select at least one Invoice', 'failure');
            return;
        }
        $http({
            method: 'POST',
            url: $scope.saveUrl,
            data: {
                voucherDetailVMList: $scope.voucherDetailList
            },
            dataType: 'JSON'
        }).then(function (response) {
            if (response.data.Error === true)
                ShowResult(response.data.Message, 'failure');
            else {
                ShowResult(response.data.Message, 'success');

            }
        }), function (response) {
            ShowResult(response.data.Message, 'failure');
        };


    };

    $scope.bankSearchByList = [
        {
            "name": "Bank",
            "value": "BankName"
        },
        {
            "name": "Bank Branch",
            "value": "BankBranchName"
        },
        {
            "name": "Account Type",
            "value": "BankAccountTypeName"
        },
        {
            "name": "Account Number",
            "value": "AccountNumber"
        },
        {
            "name": "Currency",
            "value": "CurrencyCode"
        }
    ];

    $scope.bankParameters = {
        limit: 10,
        offset: 0,
        order: "asc",
        sort: "BankName, BankBranchName, AccountTitle",
        searchBy: "AccountNumber",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    if ($scope.bankACType === "HouseBank") {
        $scope.bankSearchByList.push(
            {
                "name": "GL Code",
                "value": "GLGeneralInfoCode"
            },
            {
                "name": "GL Name",
                "value": "GLGeneralInfoName"
            },
            {
                "name": "Budget Code",
                "value": "BudgetCode"
            },
            {
                "name": "Budget Name",
                "value": "BudgetName"
            },
            {
                "name": "Activity Code",
                "value": "ActivityCode"
            },
            {
                "name": "Activity Name",
                "value": "ActivityName"
            }
        );
    }
    $scope.selectBankPopUp = function (index, id) {
        $scope.bankIndex = index;
    };

    $scope.rowData = {};
    $scope.showBankPopUp = function (entityId, data) {
        $scope.rowData = data.data;
        if (entityId === undefined || entityId === "undefined") {
            entityId = null;
        }
        $scope.getBankList = function (pageno) {
            $scope.url = "Banks/BankMaster/GetBankMasterList?bankACType=HouseBank&&entityId=" + entityId;
            baseService.paginationBase($scope.url, pageno, $scope.bankParameters)
                .then(function (result) {
                    $scope.bankList = result.Rows;
                    $scope.bankParameters.total_count = result.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, "failure");
                }).finally(function () {
                });
        };
        $scope.getBankList();
        angular.element(document.querySelector("#bankPopUp")).modal("show");
    };
  
    $scope.closeBankPopUp = function () {
        if ($scope.bankIndex !== -1) {
            var bank = $scope.bankList[$scope.bankIndex];

            $scope.rowData.AccountTitle = bank.AccountTitle;
            $scope.rowData.BankMasterName = bank.AccountTitle;
            $scope.rowData.BankMasterId = bank.BankMasterId;
            var gridObj = $("#GridCustomerInvoiceList").data("ejGrid");
            gridObj.refreshContent();
            gridObj.refreshTemplate();
        }
        $scope.hideBankPopUp();
    }
};








