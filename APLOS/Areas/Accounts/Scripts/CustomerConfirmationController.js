'use strict';
CustomerConfirmationController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$window'];
function CustomerConfirmationController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window) {
    $rootScope.title = 'Voucher GL Update';
    $scope.Action = 'Save';
    $scope.path = 'Accounts/VoucherGlUpdate/';
    $scope.url = "Accounts/VoucherGlUpdate";
    $scope.parkUrl = $scope.url + "/parkModeVoucher";
    $scope.saveUrl = $scope.path + 'UpdateInvoice';

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
    $scope.VoucherDataList = [];
    $scope.getVoucherData = function () {
        try {
            $http({
                method: 'POST',
                url: $scope.path + "getVoucherDataList",
                data: { voucherNo: $scope.voucher.VoucherNo},
                dataType: 'JSON'

            }).then(function successCallback(response) {
                $scope.VoucherDataList = response.data.DATA;

            }),
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
        }

        catch (e) {

        }
    }
  

    $scope.Clear = function () {
        $scope.Action = "Update";
        $scope.voucher = {};
        $scope.voucher.VoucherDate = $filter("date")(Date.now(), "dd-MMM-yyyy");
        $scope.voucherDetailList = [];
    };
    $scope.Get = function (data) {
        if (data.Capitalize === "Yes") {
            ShowResult(data.VoucherNo + " Voucher Already Capitalized, update not allowed!", "failure");
            return;
        }
        $scope.voucher.Id = data.Id;
        $scope.voucher.PostingDate = data.PostingDate;
        $scope.voucher.DocDate = data.DocDate;
        $scope.voucher.DocRefNo = data.DocRefNo;
        $scope.voucher.Narration = data.Narration;
        $scope.voucher.CurrencyId = data.CurrencyId;
        $scope.voucher.EntityId = data.EntityId;
        $scope.voucher.Entity = data.Entity;
        $scope.voucher.CurrencyCode = data.CurrencyCode;
        $scope.voucher.VoucherType = data.VoucherType;
        $scope.voucher.SourceType = data.SourceType;
        $scope.voucher.Capitalize = data.Capitalize;
        $scope.GetCurrencyExchangeRateList();
        $scope.currencyDisable = true;
        $scope.Action = "Update";
        
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }

        $scope.getJournalVoucherDetailList($scope.voucher.Id);
    };
    $scope.getJournalVoucherDetailList = function (id) {
        $http({
            method: "get",
            url: "accounts/VoucherGlUpdate/Data?voucherId=" + id
        }).then(function successCallback(response) {
            $scope.voucherDetailList = response.data;
        });
    };

   

    $scope.tempList = [];
    $scope.paymentSelectedList = [];
    $scope.multiplevendorInvoiceSearchList = [
        //{
        //    "name": "VoucherNo",
        //    "value": "VoucherNo"
        //},
        {
            "name": "Customer Code",
            "value": "PartyCode"
        },
        {
            "name": "Customer Name",
            "value": "PartyName"
        }
        //,{
        //    "name": "Entity",
        //    "value": "EntityName"
        //}
        //,{
        //    "name": "Posting Date",
        //    "value": "PostingDate"
        //},
        //{
        //    "name": "Doc Date",
        //    "value": "DocDate"
        //},
        //{
        //    "name": "Doc Ref",
        //    "value": "DocRefNo"
        //}
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

   
    $scope.closePopUp = function () {
        if (baseService.arrayLength($scope.tempList) > 0) {
            angular.forEach($scope.tempList, function (item) {
                $scope.paymentSelectedList.push({
                    PartyId: item.PartyId
                    , PartyCode: item.PartyCode
                    , PartyName: item.PartyName
                    
                });
            });
        }
        angular.element(document.querySelector('#CustomerListPopUP')).modal('hide');
    };
   
    $scope.Save = function () {
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
                    $scope.Clear();
                }
            }), function (response) {
                ShowResult(response.data.Message, 'failure');
            };
      

    };

    

};






