'use strict';

ServicePOCheckAndApprovedController.$inject = ['accountService', 'addressService', '$location', '$window',  'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter', '$controller'];
function ServicePOCheckAndApprovedController(accountService, addressService, $location, $window, cboService, commonMessage, $scope, $rootScope, baseService, $http, $filter, $controller) {

    $rootScope.title = "Service PO Checked And Approved";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.products = [];
    $scope.path = 'Products/PurchaseOrder/';    


    // #region  Get UI Name to set default status
    $scope.tabType = '';
    $scope.uiType = function () {
        $scope.url = $location.absUrl().split('!/')[1]
        if ($scope.url === 'Service-PO-Approval') {
            $scope.tabType = 'UnApprovedList';
        }
        else if ($scope.url === 'Service-PO-Checking') {
            $scope.tabType = 'UnCheckedList';
        }
       

    }
    $scope.uiType();
    $scope.tab = 1;
    $scope.setTabServicePOUnChecked = function (newTab) {

        $scope.tab = newTab;
        $scope.tabType = 'UnCheckedList';
        $scope.GetGetCheckedApprovedList();

    };
    $scope.isSetServicePOUnChecked = function (tabNum) {
        return $scope.tab === tabNum;
    };
    $scope.setTabServicePOHold = function (newTab) {
        //debugger;
        $scope.tabType = 'HoldRejectCheckedList';
        $scope.tab = newTab;

        $scope.GetGetCheckedApprovedList();

    };
    $scope.isSetServicePOHold = function (tabNum) {
        return $scope.tab === tabNum;
    };
    $scope.setTabServicePOChecked = function (newTab) {
        $scope.tab = newTab;
        $scope.tabType = 'CheckedList';
        $scope.GetGetCheckedApprovedList();
    };
    $scope.isSetServicePOChecked = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.setTabServicePOUnApproved = function (newTab) {
        $scope.tab = newTab;
        $scope.tabType = 'UnApprovedList';
        $scope.GetGetCheckedApprovedList();

    };
    $scope.isSetServicePOUnApproved = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.setTabServicePOhold = function (newTab) {
        $scope.tab = newTab;
        $scope.tabType = 'HoldRejectApprovedList';

        $scope.GetGetCheckedApprovedList();

    };
    $scope.isSetServicePOhold = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.setTabServicePOApproved = function (newTab) {
        $scope.tab = newTab;
        $scope.tabType = 'ApprovedList';
        $scope.GetGetCheckedApprovedList();

    };
    $scope.isSetServicePOApproved = function (tabNum) {
        return $scope.tab === tabNum;
    };
    // #endregion
    //#region 6 tab check and approved tab data load
    $scope.CheckedApproved = [];
    $scope.GetGetCheckedApprovedList = function () {
        //debugger;
        $http({
            method: 'GET',
            url: 'Products/purchaseOrder/GetCheckedApprovedList?tabType=' + $scope.tabType
        }).then(function successCallback(response) {
            $scope.CheckedApproved = response.data;
            for (var i = 0; i < $scope.CheckedApproved.length; i++) {
                response.data[i].PODate = new Date($scope.CheckedApproved[i].PODate);
            }
        });
    }
    $scope.GetGetCheckedApprovedList();

   //#endregion
    //#region Button Update function call
    //$scope.onClickChecked = function (z) {
    //    //debugger;
    //    var x = "#" + z;
    //    var gridObj = $(x).data("ejGrid");
    //    $scope.ChkedAppDataInfo = gridObj.getSelectedRecords()[0];
    //    $scope.url = $location.absUrl().split('!/')[1]
    //    if ($scope.url === 'Service-PO-Approval') {
    //        //$scope.tabType = 'UnApprovedList';
    //        if ($scope.ChkedAppDataInfo.CheckedStatus === 'Approval') {
    //            $scope.ChkedAppDataInfo.CheckedStatus = 'Approve';
    //        }
    //        $scope.POPUpStatus = $scope.ChkedAppDataInfo.CheckedStatus;
    //    }
    //    else if ($scope.url === 'Service-PO-Checking') {
    //        // $scope.tabType = 'UnCheckedList';
    //        if ($scope.ChkedAppDataInfo.CheckedStatus === 'Checked') {
    //            $scope.ChkedAppDataInfo.CheckedStatus = 'Check';
    //        }
    //        $scope.POPUpStatus = $scope.ChkedAppDataInfo.CheckedStatus;
    //    }
    //    $scope.message = 'Are you sure to ' + $scope.POPUpStatus + '?';
    //    angular.element(document.querySelector('#CheckApprovedAlert')).modal('show');
    //};

    //#endregion
    //#region Update Checked and Aproved
    $scope.onClickChecked = function (z) {
        //debugger; 
        var x = "#" + z;
        var gridObj = $(x).data("ejGrid");
        $scope.ChkedAppDataInfo = gridObj.getSelectedRecords()[0];
        $scope.url = $location.absUrl().split('!/')[1]
        if ($scope.url === 'Service-PO-Checking') {
            // $scope.tabType = 'UnCheckedList';
            if ($scope.ChkedAppDataInfo.CheckedStatus === 'Checked') {
                $scope.ChkedAppDataInfo.CheckedStatus = 'Checked';
            }
            $scope.POPUpStatus = $scope.ChkedAppDataInfo.CheckedStatus;
        }
        $scope.message = 'Are you sure to ' + $scope.POPUpStatus + '?';
        angular.element(document.querySelector('#CheckApprovedAlert')).modal('show');
    };
    $scope.onClickApproved = function (z) {
        //debugger;
        var x = "#" + z;
        var gridObj = $(x).data("ejGrid");
        $scope.ChkedAppDataInfo = gridObj.getSelectedRecords()[0];
        $scope.url = $location.absUrl().split('!/')[1]
        if ($scope.url === 'Service-PO-Approval') {
            //$scope.tabType = 'UnApprovedList';
            if ($scope.ChkedAppDataInfo.CheckedStatus === 'Approved') {
                $scope.ChkedAppDataInfo.CheckedStatus = 'Approved';
            }
            $scope.POPUpStatus = $scope.ChkedAppDataInfo.CheckedStatus;
        }
        $scope.message = 'Are you sure to ' + $scope.POPUpStatus + '?';
        angular.element(document.querySelector('#CheckApprovedAlert')).modal('show');
    };

    $scope.CheckdServicePO = function () {
        try {
            if ($scope.url === 'Service-PO-Checking') {
                if (baseService.isUndefinedOrNull($scope.ChkedAppDataInfo.CheckedStatus) || $scope.ChkedAppDataInfo.CheckedStatus === "Select") {
                    ShowResult("Please Select Checked By Status", 'failure');
                    return false;
                }
                else if ($scope.ChkedAppDataInfo.CheckedStatus === "Hold" || $scope.ChkedAppDataInfo.CheckedStatus === "Reject") {
                    if (baseService.isUndefinedOrNull($scope.ChkedAppDataInfo.CheckedHoldRejectReason)) {
                        ShowResult("Enter The Reason", 'failure');
                        return false;
                    }

                }
                else if ($scope.ChkedAppDataInfo.CheckedStatus === "Checked") {
                    if (baseService.isUndefinedOrNull($scope.ChkedAppDataInfo.ApprovedBy)) {
                        ShowResult("Please Select Approved By", 'failure');
                        return false;
                    }

                }
                var filteredData = $scope.ChkedAppDataInfo.Id;
                var data = ej.DataManager(window.lst).executeLocal(ej.Query().where("ServicePOMasterId", "equal", parseInt(filteredData), true).take(1000));
                if (data.length == 0) {
                    throw "PO Details is reuired.";
                }

                $http({
                    method: 'POST',
                    url: 'Products/PurchaseOrder/ServicePOCheckedAndApproved',
                    data: {
                        'Id': $scope.ChkedAppDataInfo.Id,
                        'PoValue': $scope.ChkedAppDataInfo.TotalQty,
                        'CheckedApprovedStataus': $scope.ChkedAppDataInfo.CheckedStatus,
                        'RejectReason': $scope.ChkedAppDataInfo.CheckedHoldRejectReason,
                        'CheckedApprovedBy': $scope.ChkedAppDataInfo.ApprovedBy,
                        'UIType': $scope.url
                    },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult("Information Updated Successfully", 'success');
                        $scope.GetGetCheckedApprovedList();
                    }
                }, function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                });
            }
        }
		catch (e) {
                ShowResult(e, 'failure');
            }
        }
        


    $scope.ApprovedServicePO = function () {
         try {
         if ($scope.url === 'Service-PO-Approval') {
            if (baseService.isUndefinedOrNull($scope.ChkedAppDataInfo.CheckedStatus) || $scope.ChkedAppDataInfo.CheckedStatus === "Select") {
                ShowResult("Please Select Approved By Status", 'failure');
                return false;
            }
            else if ($scope.ChkedAppDataInfo.CheckedStatus === "Hold" || $scope.ChkedAppDataInfo.CheckedStatus === "Reject") {
                if (baseService.isUndefinedOrNull($scope.ChkedAppDataInfo.ApprovedHoldRejectReason)) {
                    ShowResult("Enter The Reason", 'failure');
                    return false;
                }
            }

            $http({
                method: 'POST',
                url: 'Products/PurchaseOrder/ServicePOCheckedAndApproved',
                data: {
                    'Id': $scope.ChkedAppDataInfo.Id,
                    'PoValue': $scope.ChkedAppDataInfo.TotalQty,
                    'CheckedApprovedStataus': $scope.ChkedAppDataInfo.CheckedStatus,
                    'RejectReason': $scope.ChkedAppDataInfo.ApprovedHoldRejectReason,
                    'CheckedApprovedBy': $scope.ChkedAppDataInfo.SenderSecurityEmployeeId,
                    'UIType': $scope.url
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult("Information Updated Successfully", 'success');
                    $scope.GetGetCheckedApprovedList();
                }
            }, function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            });
            }
        }
        catch (e) {
            ShowResult(e, 'failure');
        }
    }






    //#endregion   
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


    $scope.ApprovedByList = [];
    $scope.GetToBeApprovedByByList = function () {
        //debugger;
        $http({
            method: 'GET',
            url: 'Products/PurchaseOrder/ServicePOApproveBy'
        }).then(function successCallback(response) {
            $scope.ApprovedByList = response.data;
        });
    }
    $scope.GetToBeApprovedByByList();


    

    //#endregion
    //#region  Grid Detail Load
    $scope.lst = [];
    $scope.POListDetails = function () {

        $http({
            method: 'GET',
            url: 'Products/PurchaseOrder/LoadServicePoDetails'
        }).then(function successCallback(response) {
            $scope.lst = response.data;
            window.lst = response.data;

        });
    }
    $scope.POListDetails();
    $scope.servicePODocumentMapDataAll = function () {
        //debugger;
        $http({
            method: 'GET',
            url: 'Products/PurchaseOrder/ServicePODocumentMapDataAll'
        }).then(function successCallback(response) {
            window.Img = response.data;

        });
    }
    $scope.servicePODocumentMapDataAll();

    $scope.data1 = $scope.lst;
    $scope.detailTemp = "#tabGridContents";
    $scope.detailgrid = function detailGridData(e) {
        var filteredData = e.data["Id"];
        var data = ej.DataManager(window.lst).executeLocal(ej.Query().where("ServicePOMasterId", "equal", parseInt(filteredData), true).take(1000));
        e.detailsElement.find("#detailGrid").ejGrid({

            dataSource: data,
            columns: ["Id", "ServiceName", "Qty", "UoM", "Rate","Amount", "TotalTaxAmount"]
        });
        e.detailsElement.find(".tabcontrol").ejTab();
        var dataImg = ej.DataManager(window.Img).executeLocal(ej.Query().where("ServicePOMasterId", "equal", parseInt(filteredData), true).take(1000));
        e.detailsElement.find("#detailGrid1").ejGrid({
            dataSource: dataImg,
            columns: [{ field: "UserFilename", headerText: "UserFilename", width: 100 },
            { field: "Description", headerText: "Description", width: 100 },
            { field: "Remarks", headerText: "Remarks", width: 100 }
            ]
        });
        e.detailsElement.find(".tabcontrol").ejTab();
    }
    //#endregion
    //#region Print

    $scope.AllTabPrint = function (z) {
        //debugger;
        var x = "#" + z;
        var gridObj = $(x).data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        location.href = "Products/PurchaseOrder/ServicePurchaseOrderReport?purchaseOrderId=" + data.Id;
    };
 
















































   


}