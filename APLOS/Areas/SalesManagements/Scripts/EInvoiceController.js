"use strict";
EInvoiceController.$inject = ["cboService", "commonMessage", '$window', "$scope", "$rootScope", "baseService", "$http", "$filter", "$controller", "accountService"];
function EInvoiceController(cboService, commonMessage, $window, $scope, $rootScope, baseService, $http, $filter, $controller, accountService) {
    $rootScope.title = "EInvoice";
    $scope.Action = "Save";
    $scope.invoiceList = [];
    $scope.postedSalesList = [];
    $scope.SearchInvoiceData = function () {
        $http({
            method: 'GET'
            , url: 'SalesManagements/Sales/GetParkedSalesList'
            , dataType: 'JSON'
        }).then(function (response) {
            $scope.postedSalesList = response.data;

        }), function (response) {
            ShowResult(response.data.Message, 'failure');
        };
        angular.element(document.querySelector('#PostedSalespopUp')).modal('show');
    }

    $scope.refreshTemplatepostedSales = function (args) {
        $("#headchk").ejCheckBox({ "change": CheckBoxSelectAllpostedSales });
    };
    function CheckBoxSelectAllpostedSales(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        var filtered = $("#PSGrid").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.postedSalesList.length; i++) {
                $scope.postedSalesList[i].Flag = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].Flag = ChkOrUnchk;
            }
        }
        //var gridObj = $("#PSGrid").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
    };

    $scope.sqlInStatement = [];
    $scope.selectedpostedSalesList = [];
    $scope.closePSPopUp = function () {
        for (var i = 0; i < $scope.postedSalesList.length; i++) {
            if ($scope.postedSalesList[i].Flag) {
                if (checkExistInvoice($scope.selectedpostedSalesList, $scope.postedSalesList[i].SalesId)) {
                    $scope.selectedpostedSalesList.push($scope.postedSalesList[i]);
                }
            }
        }

        if ($scope.selectedpostedSalesList.length > 0) {
            var uniqueId = removeDuplicates($scope.selectedpostedSalesList, 'SalesId');
            var wcEmpCode = "";
            if (uniqueId.length > 0) {
                wcEmpCode = "IN(";
                wcEmpCode += Array.prototype.map.call(uniqueId, function (item) { return "'" + item.SalesId + "'"; }).join(",") + ")";
            }
            $scope.sqlInStatement = wcEmpCode;
        }
        if ($scope.sqlInStatement.length > 0) {
            GetSalesMaterialList($scope.sqlInStatement);
        }
        angular.element(document.querySelector('#PostedSalespopUp')).modal('hide');
    }

    function removeDuplicates(myArr, prop) {
        return myArr.filter((obj, pos, arr) => {
            return arr.map(mapObj => mapObj[prop]).indexOf(obj[prop]) === pos;
        });
    }


    function checkExistInvoice(list, SalesId) {
        if (list.length > 0) {
            for (var i = 0; i < list.length; i++) {
                if (list[i].SalesId !== SalesId) {
                    return true;
                }
            }
            return false;
        }
        else {
            return true;
        }
    }

    $scope.salesMaterialList = [];
    function GetSalesMaterialList(Ids) {
        $scope.masterOrderItemList = [];
        $http({
            method: 'GET',
            url: "SalesManagements/Sales/GetSalesMaterialList?Ids=" + Ids
        }).then(function (response) {
            $scope.salesMaterialList = response.data;
        });
    }

    $scope.downloadgriddataUrl = 'GridReports/Download';
    $scope.reportFormat = "Excel";
    $scope.PrintExcel = function () {
        var dataList = [];
        var g = $("#SMGrid").data("ejGrid");
        dataList = g.getFilteredRecords();
        var ids = "";
        if (baseService.arrayLength(dataList) > 0) {
            for (var i = 0; i < dataList.length; i++) {
                if (ids == "") {
                    ids = "'','" + dataList[i].SalesMaterialId + "'";
                }
                else {
                    ids += ",'" + dataList[i].SalesMaterialId + "'";
                }
            }
        }
        else {
            for (var i = 0; i < $scope.salesMaterialList.length; i++) {
                if (ids == "") {
                    ids = "'','" + $scope.salesMaterialList[i].SalesMaterialId + "'";
                }
                else {
                    ids += ",'" + $scope.salesMaterialList[i].SalesMaterialId + "'";
                }
            }
        }


        $http({
            method: 'POST',
            url: 'SalesManagements/Sales/GetEInvoiceSaveReports',
            data: { reportFormat: $scope.reportFormat, issueIds: ids, 'data': $scope.selectedpostedSalesList},
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                $rootScope.report($scope.downloadgriddataUrl + "?FileName=" + response.data.FileName);
            }
        }, function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
        });


    }


    $scope.Clear = function () {
        $scope.masterOrderItemList = [];
        $scope.salesMaterialList = [];
        $scope.invoiceList = [];
        $scope.postedSalesList = [];
        $scope.selectedpostedSalesList = [];
    }
}