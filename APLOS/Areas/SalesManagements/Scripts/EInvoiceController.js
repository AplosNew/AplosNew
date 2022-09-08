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
            , url: 'SalesManagements/Sales/GetPostedSalesList'
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
        $scope.selectedpostedSalesList = [];
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
        GetSalesMaterialList($scope.sqlInStatement);
        angular.element(document.querySelector('#PostedSalespopUp')).modal('hide');
    }

    function removeDuplicates(myArr, prop) {
        return myArr.filter((obj, pos, arr) => {
            return arr.map(mapObj => mapObj[prop]).indexOf(obj[prop]) === pos;
        });
    }


    function checkExistInvoice(list, SalesId) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].SalesId !== SalesId) {
                return false;
            }
        }
        return true;
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

    $scope.salesMaterialList = [];
    $scope.refreshTemplateMaterial = function (args) {
        $("#headchk").ejCheckBox({ "change": CheckBoxSelectAllMaterial });
    };
    function CheckBoxSelectAllMaterial(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        var filtered = $("#SMGrid").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.salesMaterialList.length; i++) {
                $scope.salesMaterialList[i].Active = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].Active = ChkOrUnchk;
            }
        }
        //var gridObj = $("#PSGrid").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
    };


    $scope.onShowReportMOS = function () {
        if (baseService.isUndefinedOrNull($scope.modelNew.Id)) return ShowResult('No Id found', 'failure');
        $window.open('SalesManagements/Sales/SalesReport?reportFormat=' + 'Pdf' + '&&salesId=' + $scope.modelNew.Id, '_blank');
    };

}