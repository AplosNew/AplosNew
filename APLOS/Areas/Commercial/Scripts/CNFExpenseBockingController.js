'use strict';
CNFExpenseBockingController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function CNFExpenseBockingController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'CNF Expanse Bocking';
    $scope.Action = 'Save';
    $scope.ModelList = [];
    $scope.path = 'Commercial/ServiceMasterCharges/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'create';
    $scope.deleteUrl = $scope.path + 'delete/';
    $scope.Action = 'Save';
    baseService.init($scope.getListUrl);
    $scope.searchBy = "UserName"; $scope.search = "";
    
    $scope.ModelTemp = {
       
    };
    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);

    $scope.ServiceMasterChargesList = [];
    $scope.getServiceMasterCharges = function () {
        $scope.ServiceMasterChargesList = [];
        $http({
            method: 'POST',
            url: $scope.path + "GetServiceMasterCharges",
            data: { column: $scope.searchBy, value: $scope.search },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ServiceMasterChargesList = response.data;
        });
    }

    //#region Popup
    $scope.showServiceMasterChargesPopUp = function () {
        angular.element(document.querySelector('#ServiceMasterPopUp')).modal('show');
        $scope.getServiceMasterCharges();
    };
   
    $scope.hideServiceMasterChargesPopUp = function () {
        angular.element(document.querySelector("#ServiceMasterPopUp")).modal("hide");
    };

    $scope.ModelCharges = { ServiceMasterChargesId: null, Charges: null, Amount: 0 };
    $scope.ChargesList = [];
    $scope.TotalAmount = 0;
    $scope.CountTotalAmount = function (args) {
        //$scope.TotalAmount = $scope.TotalAmount+
        var x = args;
    }
    $scope.selectServiceMasterCharges = function (args) {
        var temobj = {};
        $scope.ModelCharges =args.data;
        temobj = $scope.ModelCharges;
        
         temobj = $scope.ModelCharges;
        var getRowDr = $filter("filter")($scope.ChargesList, { "Id": temobj.Id });
        if (getRowDr.length == 0 && temobj.Id != null) {
            temobj.Amount = 0;
            $scope.ChargesList.push(temobj);
        }
        else {
            ShowResult("Data already exist", 'failure');
        }
        $scope.hideServiceMasterChargesPopUp();
    };
    //endrrgion PopUp
    
 

    //invoice popup
    $scope.showInvoicePopUp = function () {

        angular.element(document.querySelector('#InvoicePopUp')).modal('show');
        $scope.GetVendorAvailableInvoiceList();
    };
    $scope.hideInvoicePopUp = function () {
        angular.element(document.querySelector("#InvoicePopUp")).modal("hide");
    };


    $scope.VendorAvailableInvoiceList = [];
    $scope.GetVendorAvailableInvoiceList = function () {
        $scope.VendorAvailableInvoiceList = [];
        $http({
            method: 'GET',
            url: 'accounts/Invoice/GetVendorAvailableInvoiceList1',
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.VendorAvailableInvoiceList = response.data;
        });
    }


    $scope.InvoiceList = [];
    $scope.InvoiceModel = {
        InvoiceId: null,
        Amount: 0,
        DistributedAmount: 0
    };

    //checked invoice
    function checkLCExist(list, Id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].OverHeadTypeGLId === Id) {
                return true;
            }
        }
        return false;
    }

    $scope.checkedInvoiceList = [];
    $scope.selectInvoice = function () {
        if (baseService.arrayLength($scope.VendorAvailableInvoiceList) > 0) {
            angular.forEach($scope.VendorAvailableInvoiceList, function (a) {
                if (checkLCExist($scope.checkedInvoiceList, a.InvoiceId) === false) {
                    if (a.Active) {
                       
                        $scope.checkedInvoiceList.push({
                             InvoiceId: a.InvoiceId
                            , Amount: a.Receivable
                            , DistributedAmount: 0
                        });
                    }
                }

            });
        }
        else
            angular.forEach($scope.checkedInvoiceList, function (a) {
                if (!baseService.valueCheckInList($scope.checkedInvoiceList, 'Id', a.InvoiceId))
                    $scope.checkedInvoiceList.splice(a, 1);
        });
       
        $scope.calDistributedAmount();

        $scope.hideInvoicePopUp();
       
    };
    $scope.calDistributedAmount = function myfunction() {
        $scope.TotalChargesAmount = $filter("sumByKey")($filter("filter")($scope.ChargesList), "Amount");
        $scope.TotalInvoiceAmount = $filter("sumByKey")($filter("filter")($scope.checkedInvoiceList), "Amount");
        
        for (var i = 0; i < $scope.checkedInvoiceList.length; i++) {
            $scope.checkedInvoiceList[i].DistributedAmount = parseFloat(($scope.checkedInvoiceList[i].Amount * $scope.TotalChargesAmount) / $scope.TotalInvoiceAmount).toFixed(4);
        }
        $scope.TotalDistributedInvoiceAmount = $filter("sumByKey")($filter("filter")($scope.checkedInvoiceList), "DistributedAmount");
        var gridObj = $("#InvoiceInfo").data("ejGrid");
        gridObj.refreshContent(true);
    }


    $scope.SaveInvoiceDetail = function () {
        $http({
            method: 'POST',
            url: 'Commercial/ServiceMasterCharges/SaveInvoiceDetail',
            data: { 'data': $scope.checkedInvoiceList },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        }
    }

    $scope.GetInvoiceDetailCharges = function () {
        $http({
            method: 'GET',
            url: $scope.path + "GetInvoiceDetailCharges",
            dataType: 'JSON'
        }).then(function successCallback(response) {
            //$scope.checkedInvoiceList = response.data;
        });
    }
    $scope.removeChargesRow = function (index) {
        $scope.ChargesList.splice(index, 1);
    };

    $scope.RemoveChargeRow = function (args) {

    }
}