'use strict';
grnPaymentHoldController.$inject = ['addressService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$http'];
function grnPaymentHoldController(addressService, commonMessage, $scope, $rootScope, baseService, $routeParams, $http) {
    $rootScope.title = "GRN Approved";
    $scope.modelList = [];
    $scope.path = 'Products/InventoryReceive/';
    $scope.updateUrl = $scope.path + 'paymenthold';

    $scope.searchGrnList = [
        {
            value: 'PartyCode'
            , name: 'Vendor Code'
        },
        {
            value: 'PartyName'
            , name: 'Vendor Name'
        },
        {
            value: 'PartyAccountGroupName'
            , name: 'Account Group'
        },
        {
            value: 'Id'
            , name: 'GRN No'
        },
        {
            value: 'GRNDate'
            , name: 'GRN Date'
        },
        {
            value: 'DocRefNo'
            , name: 'Vendor DocRefNo'
        },
        {
            value: 'InvoiceNo'
            , name: 'Invoice No'
        },
        {
            value: 'InvoiceDate'
            , name: 'Invoice Date'
        }
    ];

    $scope.grnPopUp = function () {
        $scope.popUpParameters = {
            limit: 10
            , offset: 0
            , order: 'asc'
            , sort: 'PartyCode'
            , searchBy: "PartyCode"
            , pageSize: 10
            , total_count: 0
            , search: null
            , serverPagination: true
        };
        $scope.grnList = [];
        $rootScope.tempList = [];
        angular.forEach($scope.modelList, function (a) {
            $rootScope.tempList.push({
                Id: a.Id
                , PartyCode: a.PartyCode
                , PartyName: a.PartyName
                , PartyAccountGroupName: a.PartyAccountGroupName
                , GRNDate: a.GRNDate
                , DocRefNo: a.DocRefNo
                , InvoiceNo: a.InvoiceNo
                , InvoiceDate: a.InvoiceDate
                , TransactionQty: a.TransactionQty
                , TransactionAmount: a.TransactionAmount
                , BaseAmount: a.BaseAmount
                , IsPaymentHold: a.IsPaymentHold
            });
        });
        baseService.setCurrentPage('grnList');
        $scope.getGrnData = function (pageno) {
            baseService.paginationBase($scope.path + 'GetListForHold', pageno, $scope.popUpParameters)
                .then(function (result) {
                    $scope.grnList = result.Rows;
                    $scope.popUpParameters.total_count = result.Total;
                    for (var t = 0; t < baseService.arrayLength($scope.grnList); t++) {
                        $scope.grnList[t].Flag = baseService.valueCheckInList($rootScope.tempList, 'Id', $scope.grnList[t].Id);
                    }
                    angular.element(document.querySelector('#grnPopUp')).modal('show');
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        $scope.getGrnData();
    };

    $scope.grnAdd = function () {
        if (baseService.arrayLength($rootScope.tempList) > 0) {
            angular.forEach($rootScope.tempList, function (a) {
                if (!baseService.valueCheckInList($scope.modelList, 'Id', a.Id)) {
                    $scope.modelList.push({
                         Id: a.Id
                        , PartyCode: a.PartyCode
                        , PartyName: a.PartyName
                        , PartyAccountGroupName: a.PartyAccountGroupName
                        , GRNDate: a.GRNDate
                        , DocRefNo: a.DocRefNo
                        , InvoiceNo: a.InvoiceNo
                        , InvoiceDate: a.InvoiceDate
                        , TransactionQty: a.TransactionQty
                        , TransactionAmount: a.TransactionAmount
                        , BaseAmount: a.BaseAmount
                        , IsPaymentHold: a.IsPaymentHold
                    });
                }
            });
        }
        else
            $scope.modelList = [];
        angular.forEach($scope.modelList, function (a) {
            if (!baseService.valueCheckInList($rootScope.tempList, 'Id', a.Id))
                $scope.modelList.splice(a, 1);
        });
        $scope.closeGrnPopUp();
    };

    $scope.closeGrnPopUp = function () {
        angular.element(document.querySelector('#grnPopUp')).modal('hide');
    }

    $scope.removeRowModal = function (name, index, listName, tempId, listId) {
        try {
            $scope.popUpIndex = index;
            $scope.listName = listName;
            $scope.tempId = tempId;
            $scope.listId = listId;
            $scope.message_confirmation = "Are you sure want to remove [" + name + "] ";
            angular.element(document.querySelector('#confirmProcessPopUp')).modal('show');
        }
        catch (e) {
            ShowResult(e, 'Error');
        }
    }
    $scope.removeRow = function () {
        for (var t = 0; t < baseService.arrayLength($rootScope.tempList); t++) {
            if ($rootScope.tempList[t][$scope.tempId] === $scope[$scope.listName][$scope.popUpIndex][$scope.listId])
                $rootScope.tempList.splice(t, 1);
        }
        $scope[$scope.listName].splice($scope.popUpIndex, 1);
        $scope.popUpIndex = -1;
        angular.element(document.querySelector('#confirmProcessPopUp')).modal('hide');
    };

    $scope.detailPopUp = function (inveReveiveId) {
        $http.get($scope.path + 'GetInventoryMaterialList?inveReveiveId=' + inveReveiveId)
            .then(function (response) {
                $scope.inventoryMaterialList = response.data.Rows;
                checkSameValueInColumnList($scope.inventoryMaterialList, 'TransactionUoM');
            });

        $http.get($scope.path + 'GetServiceChargeList?receiveId=' + inveReveiveId)
            .then(function (response) {
                $scope.chargesList = [];
                $scope.chargesList = response.data;
            });
        angular.element(document.querySelector('#detailPopUp')).modal('show');
    }

    $scope.closeDetailPopUp = function () {
        $scope.inventoryMaterialList = [];

        angular.element(document.querySelector('#detailPopUp')).modal('hide');
    }
    function checkSameValueInColumnList(list, fieldName) {
        for (var i = 0; i < baseService.arrayLength(list); i++) {
            if (list[i][fieldName] === (i > 0 ? list[i - 1][fieldName] : list[i][fieldName]))
                $scope.sumORnot = true;
            else return $scope.sumORnot = false;
        }
    }

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.Save = function () {
        try {
            if (baseService.arrayLength($scope.modelList) === 0) return ShowResult('Select GRN', 'failure');
            $http({
                method: 'POST'
                , url: $scope.updateUrl
                , data: $scope.modelList
                , dataType: 'JSON'
            }).then(function (response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.Clear();
                }
            }), function (response) {
                ShowResult(response.data.Message, 'failure');
            };

        } catch (e) {
            throw e;
        }
    };

    $scope.Clear = function () {
        $scope.modelList = [];
    };
}