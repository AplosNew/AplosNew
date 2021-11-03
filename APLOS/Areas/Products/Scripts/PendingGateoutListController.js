'use strict';

PendingGateoutListController.$inject = ['accountService', 'addressService', '$location', '$window', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter', '$controller'];
function PendingGateoutListController(accountService, addressService, $location, $window, cboService, commonMessage, $scope, $rootScope, baseService, $http, $filter, $controller) {

    $rootScope.title = "In out Get pass Checked";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.products = [];
    $scope.path = 'Products/GateentryToken/';


    // #region  Get UI Name to set default status
    $scope.tabType = 'pendingGateOutList';
    $scope.uiType = function () {
        $scope.url = $location.absUrl().split('!/')[1]
        if ($scope.url === 'Pending-Gate-out-List') {
            $scope.tabType = 'pendingGateOutList';
        }
    }
   
    $scope.uiType();
    $scope.tab = 1;
    $scope.setTabServicePOUnChecked = function (newTab) {

        $scope.tab = newTab;
        $scope.tabType = 'pendingGateOutList';
        $scope.GetGetCheckedApprovedList();

    };
    $scope.isSetServicePOUnChecked = function (tabNum) {
        return $scope.tab === tabNum;
    };
    $scope.setTabServicePOHold = function (newTab) {
        //debugger;
        $scope.tabType = 'GateOutList';
        $scope.tab = newTab;

        $scope.GetGetCheckedApprovedList();

    };
    $scope.isSetServicePOHold = function (tabNum) {
        return $scope.tab === tabNum;
    };
    //$scope.setTabServicePOChecked = function (newTab) {
    //    $scope.tab = newTab;
    //    $scope.tabType = 'CheckedList';
    //    $scope.GetGetCheckedApprovedList();
    //};
    //$scope.isSetServicePOChecked = function (tabNum) {
    //    return $scope.tab === tabNum;
    //};

    //$scope.setTabServicePOUnApproved = function (newTab) {
    //    $scope.tab = newTab;
    //    $scope.tabType = 'UnApprovedList';
    //    $scope.GetGetCheckedApprovedList();

    //};
    //$scope.isSetServicePOUnApproved = function (tabNum) {
    //    return $scope.tab === tabNum;
    //};

    //$scope.setTabServicePOhold = function (newTab) {
    //    $scope.tab = newTab;
    //    $scope.tabType = 'HoldRejectApprovedList';

    //    $scope.GetGetCheckedApprovedList();

    //};
    //$scope.isSetServicePOhold = function (tabNum) {
    //    return $scope.tab === tabNum;
    //};

    //$scope.setTabServicePOApproved = function (newTab) {
    //    $scope.tab = newTab;
    //    $scope.tabType = 'ApprovedList';
    //    $scope.GetGetCheckedApprovedList();

    //};
    //$scope.isSetServicePOApproved = function (tabNum) {
    //    return $scope.tab === tabNum;
    //};
    // #endregion
    //#region 6 tab check and approved tab data load
    $scope.CheckedApproved = [];
    $scope.GetGetCheckedApprovedList = function () {
        //debugger;
        $http({
            method: 'GET',
            url: 'Products/GateentryToken/GetPendingGateOutList?tabType=' + $scope.tabType
        }).then(function successCallback(response) {
            $scope.CheckedApproved = response.data;
            for (var i = 0; i < $scope.CheckedApproved.length; i++) {
                response.data[i].PODate = new Date($scope.CheckedApproved[i].PODate);
            }
        });
    }
    $scope.GetGetCheckedApprovedList();


    $scope.AllTabPrint = function (z) {
        //debugger;
        var x = "#" + z;
        var gridObj = $(x).data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        location.href = " GateentryToken/InOutGatePassTeamplateReport?GatePassId=" + data.Id;
    };


    $scope.onClickChecked = function (z) {
        debugger;
        var x = "#" + z;
        var gridObj = $(x).data("ejGrid");
        $scope.ChkedAppDataInfo = gridObj.getSelectedRecords()[0];
        $scope.url = $location.absUrl().split('!/')[1]
        $scope.message = 'Are you sure to check?';
        angular.element(document.querySelector('#CheckApprovedAlert')).modal('show');
    };
    $scope.null = null;
    $scope.InOutGatePassUpdatePOP = function () {
        try {
         

                $http({
                    method: 'POST',
                    url: 'Products/GateentryToken/PendingInOutGatePassUpdate',
                    data:
                    {
                        'ComId': $scope.ChkedAppDataInfo.Id,
                        //'CheckedApprovedStataus': $scope.ChkedAppDataInfo.CheckedStatus,
                        //'CheckedHoldRejectReason': $scope.ChkedAppDataInfo.CheckedHoldRejectReason,
                        'UserSendData': $scope.ChkedAppDataInfo
                    },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.GetGetCheckedApprovedList();
                    }
                }, function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                });
            


        }
        catch (e) {
            ShowResult(e, 'failure');
        }
    };


    //#region Checked And Approval Status

    $scope.CheckedStatusList = function () {
        cboService.getEnumCbo("enum/GetPOApprovalStatusCbo", function (result) {
            $scope.checkedstatusList = result;
        });
    }
    $scope.CheckedStatusList();
    $scope.LoadapprovalStatus = function () {
        cboService.getEnumCbo("enum/GetCheckedStatusCbo", function (result) {
            $scope.approvalStatusList = result;
        });

    }
    $scope.LoadapprovalStatus();
    //#endregion
    //#region CheckBY Approve BY 

    $scope.checkedByList = [];
    $scope.GetToBeCheckedByList = function () {
        //debugger;
        $http({
            method: 'GET',
            url: 'Products/GateentryToken/GetGatePassCheckedBy'
        }).then(function successCallback(response) {
            $scope.checkedByList = response.data;
        });
    }
    $scope.GetToBeCheckedByList();

     //#endregion

    //#region In out details

    $scope.PurchaserReturn1st = [];
    $scope.PurchaserReturnListDetails = function () {
        //debugger;
        $http({
            method: 'GET',
            //url: 'Products/Requisition/GetAllReqdataDetails?ReqDetailId=' + $scope.filteredData
            url: 'Products/GoodsReceiveNote/PurchaseReturnDetailsData'
        }).then(function successCallback(response) {
            $scope.lst = response.data;
            //$scope.detailgrid($scope.lst);
            window.PurchaseReturnDetailsList = response.data;

        });
    }
    $scope.PurchaserReturnListDetails();




    $scope.data1 = $scope.PurchaserReturn1st;
    $scope.detailTemppurchaseReturn = "#tabGridpurchaseContents";
    //$scope.detailgrid = "detailGridData(e)";
    $scope.detailgridpurchaseReturn = function detailGridData(e) {

        debugger;
        var filteredData = e.data["PurchaseReturnId"];
        if (e.data.GatePassType === 'PurchaseReturn') {
            var data = ej.DataManager(window.PurchaseReturnDetailsList).executeLocal(ej.Query().where("PurchaseReturnId", "equal", parseInt(filteredData), true).take(500));
            e.detailsElement.find("#detailGrid1st").ejGrid({
                dataSource: data,
                columns: ["MaterialName", "Article", "SKU1", "SKU2", "SKU3", "TransactionQty", "TransactionUoMId", "TransactionUoM", "TransactionRate", "TotalMaterialTranAmount"]
            });
            e.detailsElement.find(".tabcontrol").ejTab();
        }
        //if (e.data.GatePassFor === 'InventorySales') {
        //    var data = ej.DataManager(window.PurchaseReturnDetailsList).executeLocal(ej.Query().where("PurchaseReturnId", "equal", parseInt(filteredData), true).take(500));
        //    e.detailsElement.find("#detailGrid1st").ejGrid({
        //        dataSource: data,
        //        columns: ["MaterialName", "Article", "SKU1", "SKU2", "SKU3", "TransactionQty", "TransactionUoMId", "TransactionUoM", "TransactionRate", "TotalMaterialTranAmount"]
        //    });
        //    e.detailsElement.find(".tabcontrol").ejTab();
        //}

        //if (e.data.GatePassFor === 'InventoryScrap') {
        //    var data = ej.DataManager(window.PurchaseReturnDetailsList).executeLocal(ej.Query().where("PurchaseReturnId", "equal", parseInt(filteredData), true).take(500));
        //    e.detailsElement.find("#detailGrid1st").ejGrid({
        //        dataSource: data,
        //        columns: ["MaterialName", "Article", "SKU1", "SKU2", "SKU3", "TransactionQty", "TransactionUoMId", "TransactionUoM", "TransactionRate", "TotalMaterialTranAmount"]
        //    });
        //    e.detailsElement.find(".tabcontrol").ejTab();
        //}

        //if (e.data.GatePassFor === 'InventoryTransfer') {
        //    var data = ej.DataManager(window.PurchaseReturnDetailsList).executeLocal(ej.Query().where("PurchaseReturnId", "equal", parseInt(filteredData), true).take(500));
        //    e.detailsElement.find("#detailGrid1st").ejGrid({
        //        dataSource: data,
        //        columns: ["MaterialName", "Article", "SKU1", "SKU2", "SKU3", "TransactionQty", "TransactionUoMId", "TransactionUoM", "TransactionRate", "TotalMaterialTranAmount"]
        //    });
        //    e.detailsElement.find(".tabcontrol").ejTab();
        //}
    }

}