'use strict';
multipleVPController.$inject = ['accountService', 'addressService', '$location', '$window', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter', '$controller'];
function multipleVPController(accountService, addressService, $location, $window, cboService, commonMessage, $scope, $rootScope, baseService, $http, $filter, $controller) {

    $rootScope.title = "Purchase Return Checked Approved";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.products = [];
    $scope.path = 'accounts/invoice/';
     
    // #region  Tab Control for Requisition
    $scope.tabType = '';
    $scope.uiType = function () {
        $scope.url = $location.absUrl().split('!/')[1]
        $scope.tabType = 'UnApprovedList';
    }
    $scope.uiType();
    $scope.tab = 1;
    //UnCheckedList, HoldRejectCheckedList, CheckedList, UnApprovedList, HoldRejectApprovedList, ApprovedList   
    $scope.setTabGatePassUnCheckedList = function (newTab) {
        //debugger;
        $scope.tabType = 'UnCheckedList';
        //$scope.GetGetCheckedApprovedList();
        $scope.tab = newTab;
    };
    $scope.isSetMVPList = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.setTabMultipleVPUnApprovedList = function (newTab) {
        $scope.tab = newTab;
        $scope.tabType = 'UnApprovedList';
        $scope.GetCheckedUnApprovedList();

    };

    $scope.isSetMVPHoldRejectApprovedList = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.setTabMVPHoldRejectList = function (newTab) {
        $scope.tab = newTab;
        $scope.tabType = 'HoldRejectList';
        $scope.GetCheckedHoldRejectList();
    };

    $scope.isSetMVPApprovedList = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.setTabMVPApprovedList = function (newTab) {
        $scope.tab = newTab;
        $scope.tabType = 'ApprovedList';
        $scope.GetCheckedApprovedList();
    };

    // #endregion
    //#endregion


    $scope.CheckedApprovedData = [];
    $scope.GetCheckedUnApprovedList = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'GetMultiplePaymentMyAppData?tabType=' + $scope.tabType
        }).then(function successCallback(response) {
            $scope.CheckedApprovedData = response.data;
        });
    }
    $scope.GetCheckedUnApprovedList();

    $scope.SaveUnApproveData = function (args) {
        try {
            $http({
                method: 'POST',
                url: 'Accounts/Invoice/CreateUnApproveBy',
                data: {'data': args.data},
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, 'failure');
            });
        }
        catch (e) {
            ShowResult(e, "failure");
        }
    };


    $scope.CheckedHoldRejectData = [];
    $scope.GetCheckedHoldRejectList = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'GetMultiplePaymentMyAppData?tabType=' + $scope.tabType
        }).then(function successCallback(response) {
            $scope.CheckedHoldRejectData = response.data;
        });
    }
    $scope.GetCheckedHoldRejectList();

 
    $scope.CheckedApproved = [];
    $scope.GetCheckedApprovedList = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'GetMultiplePaymentMyAppData?tabType=' + $scope.tabType
        }).then(function successCallback(response) {
            $scope.CheckedApproved = response.data;
        });
    }
    $scope.GetCheckedApprovedList();

    //$scope.CheckedApproved = [];
    //$scope.GetGetCheckedApprovedList = function () {
    //    //debugger;
    //    $http({
    //        method: 'GET',
    //        url: 'Products/GoodsReceiveNote/PurchaseReturnApprovedCeackList?tabType=' + $scope.tabType
    //    }).then(function successCallback(response) {
    //        $scope.CheckedApproved = response.data;
    //        for (var i = 0; i < $scope.CheckedApproved.length; i++) {
    //            response.data[i].GatePassEntryDate = new Date($scope.CheckedApproved[i].GatePassEntryDate);
    //        }
    //    });
    //}
    //$scope.GetGetCheckedApprovedList();

    $scope.GatePassCheckApprovedAlertCall = function () {
        $scope.message = 'Are you sure want to Approve?';
        angular.element(document.querySelector('#GatePassCheckApprovedAlert')).modal('show');
    };
    //$scope.onClickGatePassChecked = function (z) {
    //    //debugger;
    //    var x = "#" + z;
    //    var gridObj = $(x).data("ejGrid");
    //    $scope.gatePassChkedAppData = gridObj.getSelectedRecords()[0];
    //    $scope.url = $location.absUrl().split('!/')[1]
    //    if ($scope.url === 'Purchase-Return-Approved') {
    //        //$scope.tabType = 'UnApprovedList';
    //        $scope.POPUpStatus = $scope.gatePassChkedAppData.ApprovedByStatus;
    //    }
    //    else if ($scope.url === 'Purchase-Return-Checked') {
    //        // $scope.tabType = 'UnCheckedList';
    //        $scope.POPUpStatus = $scope.gatePassChkedAppData.CheckedByStatus;
    //    }
    //    $scope.message = 'Are you sure want to ' + $scope.POPUpStatus + '?';
    //    angular.element(document.querySelector('#GatePassCheckApprovedAlert')).modal('show');
    //};

    $scope.GatePassCheckApproved = function () {


        debugger;
        if ($scope.url === 'Purchase-Return-Checked') {
            if (baseService.isUndefinedOrNull($scope.gatePassChkedAppData.CheckedByStatus) || $scope.gatePassChkedAppData.CheckedByStatus === "Select") {
                ShowResult("Please Select Checked By Status", 'failure');
                return false;
            }
            else if ($scope.gatePassChkedAppData.CheckedByStatus === "Hold" || $scope.gatePassChkedAppData.CheckedByStatus === "Reject") {
                if (baseService.isUndefinedOrNull($scope.gatePassChkedAppData.CheckedHoldRejectReason)) {
                    ShowResult("Enter The Reason", 'failure');
                    return false;
                }

            }
            else if ($scope.gatePassChkedAppData.CheckedByStatus === "Checked") {
                if (baseService.isUndefinedOrNull($scope.gatePassChkedAppData.ApprovedBy)) {
                    ShowResult("Please Select Approved By", 'failure');
                    return false;
                }

            }
            $http({
                method: 'POST',
                url: 'Products/GoodsReceiveNote/PurchaseReturnCheckedAndApproved',
                data: {
                    'Id': $scope.gatePassChkedAppData.Id,
                    'PoValue': $scope.gatePassChkedAppData.TotalQty,
                    'CheckedApprovedStataus': $scope.gatePassChkedAppData.CheckedByStatus,
                    'RejectReason': $scope.gatePassChkedAppData.CheckedHoldRejectReason,
                    'CheckedApprovedBy': $scope.gatePassChkedAppData.ApprovedBy,
                    'UIType': $scope.url
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult('Updated successfully', 'failure');
                }
                else {
                    ShowResult('Updated successfully', 'success');
                    $scope.GetGetCheckedApprovedList();
                }
            }, function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            });
        }
        else if ($scope.url === 'Purchase-Return-Approved') {
            if (baseService.isUndefinedOrNull($scope.gatePassChkedAppData.ApprovedByStatus) || $scope.gatePassChkedAppData.ApprovedByStatus === "Select") {
                ShowResult("Please Select Approved By Status", 'failure');
                return false;
            }
            else if ($scope.gatePassChkedAppData.ApprovedByStatus === "Hold" || $scope.gatePassChkedAppData.ApprovedByStatus === "Reject") {
                if (baseService.isUndefinedOrNull($scope.gatePassChkedAppData.ApprovedHoldRejectReason)) {
                    ShowResult("Enter The Reason", 'failure');
                    return false;
                }

            }


            $http({
                method: 'POST',
                url: 'Products/GoodsReceiveNote/PurchaseReturnCheckedAndApproved',
                data: {
                    'Id': $scope.gatePassChkedAppData.Id,
                    'PoValue': $scope.gatePassChkedAppData.TotalQty,
                    'CheckedApprovedStataus': $scope.gatePassChkedAppData.ApprovedByStatus,
                    'RejectReason': $scope.gatePassChkedAppData.ApprovedHoldRejectReason,
                    'CheckedApprovedBy': $scope.gatePassChkedAppData.SenderSecurityEmployeeId,
                    'UIType': $scope.url
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult('Updated successfully', 'failure');
                }
                else {
                    ShowResult('Updated successfully', 'success');
                    $scope.GetGetCheckedApprovedList();
                }
            }, function errorCallBack(response) {
                ShowResult('Updated successfully', 'failure');
            });
        }
    }

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };


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

    //#region CheckBY Approve BY Secusity Approve

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


    $scope.ApprovedByList = [];
    $scope.GetToBeApprovedByByList = function () {
        //debugger;
        $http({
            method: 'GET',
            url: 'Products/PurchaseOrder/ServicePOAcknowledgementApproveBy'
        }).then(function successCallback(response) {
            $scope.ApprovedByList = response.data;
        });
    }
    $scope.GetToBeApprovedByByList();

    $scope.ApprovedBySecurityList = [];
    $scope.GetToBeSecurityByList = function () {
        //debugger;
        $http({
            method: 'GET',
            url: 'Products/GateentryToken/GetGatePassTobeApproveddSecurityBy'
        }).then(function successCallback(response) {
            $scope.ApprovedBySecurityList = response.data;
        });
    }
    $scope.GetToBeSecurityByList();

    //#endregion


    //#region Service Detail
    $scope.lst = [];
    $scope.ServiceListDetails = function () {
        //debugger;
        $http({
            method: 'GET',
            //url: 'Products/Requisition/GetAllReqdataDetails?ReqDetailId=' + $scope.filteredData
            url: 'Products/PurchaseOrder/LoadAllAckServicesData'
        }).then(function successCallback(response) {
            $scope.lst = response.data;
            window.lst = response.data;

        });
    }
    $scope.ServiceListDetails();


    $scope.data1 = $scope.lst;
    $scope.detailTemp = "#tabGridContents";
    //$scope.detailgrid = "detailGridData(e)";
    $scope.detailgrid = function detailGridData(e) {
        //debugger;

        var filteredData = e.data["Id"];
        var data = ej.DataManager(window.lst).executeLocal(ej.Query().where("ServiceAcknowledgementMasterId", "equal", parseInt(filteredData), true).take(20000));
        e.detailsElement.find("#detailGrid").ejGrid({

            dataSource: data,
            columns: ["Id", "ServiceName", "Amount", "Code", "TotalTaxAmount", "TotalAmount"]
        });
        e.detailsElement.find(".tabcontrol").ejTab();
    }
    //#endregion







    $scope.AllTabPrint = function (z) {
        //debugger;
        var x = "#" + z;
        var gridObj = $(x).data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        location.href = " GoodsReceiveNote/PurchaseReturnReport?grnId=" + data.Id;
    };



    $scope.data1 = $scope.PurchaserReturn1st;
    //$scope.detailTemppurchaseReturn = "#tabGridpurchaseContents";
    //$scope.detailgrid = "detailGridData(e)";
    //$scope.detailgridpurchaseReturn = function detailGridData(e) {
    //    //debugger;

    //    var filteredData = e.data["Id"];
    //    var data = ej.DataManager(window.lst).executeLocal(ej.Query().where("PurchaseReturnId", "equal", parseInt(filteredData), true).take(105));
    //    e.detailsElement.find("#detailGridPR").ejGrid({

    //        dataSource: data,
    //        columns: ["MaterialName", "Article", "SKU1", "SKU2", "SKU3", "TransactionQty", "TransactionUoMId", "TransactionUoM", "TransactionRate", "TotalMaterialTranAmount"]
    //    });
    //    e.detailsElement.find(".tabcontrol").ejTab();
    //}




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
            window.lst = response.data;

        });
    }
    $scope.PurchaserReturnListDetails();
     
}