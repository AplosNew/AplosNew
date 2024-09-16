'use strict';
InventoryrequisitionCheckbyController.$inject = ['$window', 'cboService', '$scope', '$rootScope', '$http', 'baseService', '$filter'];
function InventoryrequisitionCheckbyController($window, cboService, $scope, $rootScope, $http, baseService, $filter) {
    $rootScope.title = "Inventory Approved";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.products = [];
    $scope.path = 'Products/InventoryCheckApproved/';
    $scope.path1 = 'Products/Requisition/';
    $scope.detailUpdate = 'Products/Requisition/UpdateApprovedQty';

    $scope.AllTabPrint = function (z) {
        //debugger;
        var FromCheckedUI = 'FromCheckedUI';
        var x = "#" + z;
        var gridObj = $(x).data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];

        $http({
            method: 'GET',
            url: 'Products/Requisition/GetFiscalYear?formattedDate=' + data.RequisitionDate1,
        }).then(function successCallback(response) {
            $scope.startDate = response.data[0].StartDate;
            $scope.endDate = response.data[0].EndDate;
            location.href = "Products/Requisition/RequisitionReportby?RequisitionId=" + data.Id + '&startDate=' + $scope.startDate + '&endDate=' + $scope.endDate + '&PreparedBy=' + data.PreparedBy + '&FromCheckedUI=' + FromCheckedUI;
        });

    };


    $scope.RequisitionHoldRejectList = [];
    $scope.RequisitionHoldReject = function () {
        //debugger;
        $http({
            method: "GET",
            dataType: 'JSON',
            //url: $scope.getSearchListUrl,
            url: 'Products/InventoryCheckApproved/GetListRequisionHoldReject',
        }).then(function successCallback(response) {
            $scope.RequisitionHoldRejectList = response.data;


            //entrydata = copy(searchdata);
        });
    };
    $scope.RequisitionHoldReject();

    $scope.tab = 1;
    $scope.setTabReqUnchecked = function (newTab) {
        $scope.tab = newTab;
        $scope.RequisitionUnchecked();

    };
    $scope.isSetReqUnchecked = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.setTabReqHoldRreject = function (newTab) {
        $scope.tab = newTab;
        $scope.RequisitionHoldReject();

    };
    $scope.isSetReqHoldReject = function (tabNum) {
        return $scope.tab === tabNum;
    };




    $scope.setTabReqchecked = function (newTab) {
        $scope.tab = newTab;
        $scope.Requisitionchecked();

    };
    $scope.isSetReqchecked = function (tabNum) {
        return $scope.tab === tabNum;
    };

    //#Region Grid data bind 
    $scope.RequisitionUncheckedList = [];
    $scope.RequisitionUnchecked = function () {
        //debugger;
        $http({
            method: "GET",
            dataType: 'JSON',
            //url: $scope.getSearchListUrl,
            url: 'Products/InventoryCheckApproved/GetListRequisionUnchecked',
        }).then(function successCallback(response) {
            $scope.RequisitionUncheckedList = response.data;


            //entrydata = copy(searchdata);
        });
    };
    $scope.RequisitionUnchecked();


    $scope.RequisitioncheckedList = [];
    $scope.Requisitionchecked = function () {
        //debugger;
        $http({
            method: "GET",
            dataType: 'JSON',
            //url: $scope.getSearchListUrl,
            url: 'Products/InventoryCheckApproved/GetListRequisionchecked',
        }).then(function successCallback(response) {
            $scope.RequisitioncheckedList = response.data;

            //entrydata = copy(searchdata);
        });
    };
    $scope.Requisitionchecked();

    $scope.checkedByList = [];
    $scope.GetSupervisorCboList = function () {
        //debugger;
        $http({
            method: 'GET',
            url: 'Products/InventoryCheckApproved/GetSupervisorCboApproved'
        }).then(function successCallback(response) {
            $scope.checkedByList = response.data;
        });
    }
    $scope.GetSupervisorCboList();


    $scope.poApproved = function () {
        cboService.getEnumCbo("enum/GetPOApprovalStatusCbo", function (result) {
            $scope.POApprovalList = result;
        });
    }
    $scope.poApproved();
    $scope.onClickPOA = function (z) {
        var x = "#" + z;
        var gridObj = $(x).data("ejGrid");
        $scope.podata = gridObj.getSelectedRecords()[0];

        $scope.message = 'Are you sure want to ' + $scope.podata.CheckedByStatus + '?';
        angular.element(document.querySelector('#poapprovealert')).modal('show');

    };
    //debugger;
    $scope.commandpo = [{
        type: "details", buttonOptions: {
            text: "Save",
            width: "100",
            height: "30",
            click: $scope.onClickPOA
        }
    }];

    $scope.approvalAlert = function () {
        $scope.message = 'Are you sure want to Approve?';
        angular.element(document.querySelector('#poapprovealert')).modal('show');
    };

    $scope.onClickRequisitionHoldReject = function (z) {
        //debugger;
        var x = "#" + z;
        var gridObj = $(x).data("ejGrid");
        $scope.podata = gridObj.getSelectedRecords()[0];
        if ($scope.podata.CheckedStatus === "For Checked" || $scope.podata.CheckedStatus === "Select" || baseService.isUndefinedOrNull($scope.podata.CheckedStatus)) {
            ShowResult("Please Select Checked By Status", 'failure');
            return false;
        }

        else if ($scope.podata.CheckedStatus === "Checked" && baseService.isUndefinedOrNull($scope.podata.AuthorizedBy)) {
            ShowResult("Please Select To be Approved By", 'failure');
            return false;
        }
        else if (($scope.podata.CheckedStatus === "Hold" || $scope.podata.CheckedStatus === "Reject") && baseService.isUndefinedOrNull($scope.podata.CheckedRejectReason)) {
            ShowResult("Enter The Reason", 'failure');
            return false;
        }
        else {
            $scope.message = 'Are you sure want to ' + $scope.podata.CheckedStatus + '?';
            angular.element(document.querySelector('#Reqcheckalert')).modal('show');
        }

    };
    $scope.commandReqHRSave = [{
        type: "details", buttonOptions: {
            text: "Save",
            width: "100",
            height: "30",
            click: $scope.onClickRequisitionHoldReject
        }
    }];


    $scope.HoldandRejectAlert = function () {
        $scope.message = 'Are you sure want to Checked?';
        angular.element(document.querySelector('#Reqcheckalert')).modal('show');
    };

    $scope.onClickPOAUTH = function (z) {

        var x = "#" + z;
        var gridObj = $(x).data("ejGrid");
        $scope.podata = gridObj.getSelectedRecords()[0];
        $scope.approvalAlert();
    };


    $scope.onClickReqAHR = function (z) {
        //debugger;
        var x = "#" + z;
        var gridObj = $(x).data("ejGrid");
        $scope.podata = gridObj.getSelectedRecords()[0];
        $scope.approvalAlert();
    };


    $scope.poApp = function () {
        try {
            var filteredData = $scope.podata.Id;
           
            if ($scope.podata.CheckedStatus === "For Checked" || $scope.podata.CheckedStatus === "Select" || baseService.isUndefinedOrNull($scope.podata.CheckedStatus)) {
                ShowResult("Please Select Checked By Status", 'failure');
                return false;
            }

            else if ($scope.podata.CheckedStatus === "Checked" && baseService.isUndefinedOrNull($scope.podata.AuthorizedBy)) {
                ShowResult("Please Select To be Approved By", 'failure');
                return false;
            }
            else if (($scope.podata.CheckedStatus === "Hold" || $scope.podata.CheckedStatus === "Reject") && baseService.isUndefinedOrNull($scope.podata.CheckedRejectReason)) {
                ShowResult("Enter The Reason", 'failure');
                return false;
            }


            var data = ej.DataManager($scope.lst).executeLocal(ej.Query().where("MaterialReqqusitionMasterId", "equal", parseInt(filteredData), true).take(100));
            if (data.length == 0) {
                throw "Requisition Details is reuired.";
            }


            $http({
                method: 'POST',
                url: 'Products/InventoryCheckApproved/ReqChecked',
                data: {
                    'PoId': $scope.podata.Id,
                    'PoValue': $scope.podata.TotalQty,
                    'CheckedStataus': $scope.podata.CheckedStatus,
                    'CheckedHoldRejectReason': $scope.podata.CheckedRejectReason,
                    'AuthorizedBy': $scope.podata.AuthorizedBy,
                    'RequisitionType': $scope.podata.RequisitionType,
                    'RequirmentType': $scope.podata.RequirmentType,
                    'CheckedBy': $scope.podata.CheckedBy,
                    'PreparedBY': $scope.podata.AddedBy

                },

                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.RequisitionUnchecked();
                    $scope.GetSupervisorCboList();
                    $scope.RequisitionHoldReject();
                    $scope.Requisitionchecked();
                }
            }, function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.lst = [];
    $scope.ReqListDetails = function () {
        try {
            //debugger;
            $http({
                method: 'GET',
                url: 'Products/Requisition/GetAllReqdataDetailsById?Id=' + $scope.podata.Id
            }).then(function successCallback(response) {
                $scope.lst = response.data;
                if ($scope.lst.length > 0) {
                    $scope.poApp();
                } else {
                  //  throw "Requisition Details is required.";
                    ShowResult("Requisition Details is required.", 'failure');
                }

            });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }
    //$scope.ReqListDetails();


    $scope.data1 = $scope.lst;
    $scope.detailTemp = "#tabGridContents";

    $scope.detailgrid = function detailGridData(e) {

        var filteredData = e.data["Id"];

        $http({
            method: 'GET',
            url: 'Products/Requisition/GetAllReqdataDetailsById?Id=' + filteredData
        }).then(function successCallback(response) {
            $scope.InvoiceNoList = response.data;

            var data = ej.DataManager($scope.InvoiceNoList).executeLocal(ej.Query().where("MaterialReqqusitionMasterId", "equal", parseInt(filteredData), true).take(100));

            e.detailsElement.find("#detailGrid").ejGrid({

                dataSource: data,
                columns: [
                    { field: "BudgetType", headerText: "BudgetType", width: 50 },
                    { field: "ActivityName", headerText: "ActivityName", width: 150 },
                    { field: "MaterialGroupName", headerText: "MaterialGroupName", width: 100 },
                    { field: "MaterialName", headerText: "Material Name", width: 150 },
                    { field: "ArticleName", headerText: "Article Name", width: 150 },
                    { field: "SKU1", headerText: "SKU1", width: 50 },
                    { field: "SKU2", headerText: "SKU2", width: 50 },
                    { field: "SKU3", headerText: "SKU3", width: 50 },
                    { field: "MaterialDetail", headerText: "MaterialDetail", width: 150 },
                    { field: "TransactionQty", headerText: "Qty", width: 70 },
                    { field: "TransactionUoM", headerText: "UoM", width: 50 },
                    { field: "EstimatedRate", headerText: "E.Rate", width: 50 },
                    { field: "CurrencyName", headerText: "Curr", width: 30 },
                    { field: "TotalAmount", headerText: "T.Amount", width: 100 }

                ]
            });
            e.detailsElement.find(".tabcontrol").ejTab();
        });


    }

    //#endregion
    // #endregion Requisition
    $scope.recorddoubleclick = function ($event) {
        //debugger;
        var x = $event;
        var Id = x.data.Id;
        getInventoryMaterialList(Id);
    };
    function getInventoryMaterialList(ReqId) {

        //debugger;
        $scope.requisitionMaterialList = [];
        $http.get($scope.path1 + 'GetAllReqdataDetailsById?Id=' + ReqId)
            .then(function (response) {
                $scope.requisitionMaterialList = response.data;

            });
        angular.element(document.querySelector('#ListOfRequisition')).modal('show');

    }
    $scope.recorddoubleclick1 = function ($event) {
        //debugger;
        var x = $event;
        var Id = x.data.Id;
        getInventoryMaterialList1(Id);
    };

    function getInventoryMaterialList1(ReqId) {

        //debugger;
        $scope.requisitionMaterialList1 = [];
        $http.get($scope.path1 + 'GetAllReqdataDetailsById?Id=' + ReqId)
            .then(function (response) {
                $scope.requisitionMaterialList1 = response.data;

            });
        angular.element(document.querySelector('#ListOfRequisition1')).modal('show');

    }
    $scope.RequisitionListHide = function () {
        $scope.taxCategoryList = [];
        angular.element(document.querySelector('#ListOfRequisition')).modal('hide');
    };
    $scope.RequisitionListHide1 = function () {
        $scope.taxCategoryList = [];
        angular.element(document.querySelector('#ListOfRequisition1')).modal('hide');
    };




    $scope.onClickAReqHR = function (args) {

        var gridObj = $("#GridReqAHR").data("ejGrid");
        //getting corresponding record             
        var data = gridObj.getSelectedRecords()[0];
        //alert('jj' + data.Id);
        // $scope.valuePassInDelModal(data); 
        location.href = "Products/Requisition/RequisitionReportby?RequisitionId=" + data.Id;

    };
    $scope.commandAReqHRPrint = [{
        type: "details", buttonOptions: {
            text: "Print",
            width: "100",
            height: "30",
            click: $scope.onClickAReqHR
        }
    }];


    $scope.onClickReqApp = function (args) {

        var gridObj = $("#GridPOAPp").data("ejGrid");
        //getting corresponding record             
        var data = gridObj.getSelectedRecords()[0];
        //alert('jj' + data.Id);
        // $scope.valuePassInDelModal(data); 
        location.href = "Products/Requisition/RequisitionReportby?RequisitionId=" + data.Id;

    };
    $scope.commandReqApprint = [{
        type: "details", buttonOptions: {
            text: "Print",
            width: "100",
            height: "30",
            click: $scope.onClickReqApp
        }
    }];


    //#region All Print option of Requisition Check


    $scope.onClick11 = function (args) {

        var gridObj = $("#GridPO").data("ejGrid");
        //getting corresponding record             
        var data = gridObj.getSelectedRecords()[0];
        //alert('jj' + data.Id);
        // $scope.valuePassInDelModal(data); 
        location.href = "Products/Requisition/RequisitionReportby?RequisitionId=" + data.Id;

    };
    $scope.command1 = [{
        type: "details", buttonOptions: {
            text: "Print",
            width: "100",
            height: "30",
            click: $scope.onClick11
        }
    }];



    $scope.onClickReqHR = function (args) {

        var gridObj = $("#GridReqHR").data("ejGrid");
        //getting corresponding record             
        var data = gridObj.getSelectedRecords()[0];
        //alert('jj' + data.Id);
        // $scope.valuePassInDelModal(data); 
        location.href = "Products/Requisition/RequisitionReportby?RequisitionId=" + data.Id;

    };
    $scope.commandReqHRPrint = [{
        type: "details", buttonOptions: {
            text: "Print",
            width: "100",
            height: "30",
            click: $scope.onClickReqHR
        }
    }];


    $scope.onClick111 = function (z) {

        var x = "#" + z;
        var gridObj = $(x).data("ejGrid");
        //getting corresponding record             
        var data = gridObj.getSelectedRecords()[0];
        //alert('jj' + data.Id);
        // $scope.valuePassInDelModal(data); 
        location.href = "Products/Requisition/RequisitionReportby?RequisitionId=" + data.Id;

    };
    $scope.command11 = [{
        type: "details", buttonOptions: {
            text: "Print",
            width: "100",
            height: "30",
            click: $scope.onClick111
        }
    }];


    //#endregion

    $window.onresize = function (event) {

        $scope.actionCompleteSelected1();

    };
    $scope.actionCompleteSelected1 = function (args) {
        try {
            if (args.requestType === "refresh") {
                var gridObj = $("#GridPO1").ejGrid("instance");
                var scrollerwidth = $("#checked").width();//Obtain the width of the container

                //   $("#GridReq").children('.e-grid.e-headercell').css('height', '100px');              
                gridObj.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth - 20, height: 400 } });//pass the obtainer width and height to gridmodel options
                gridObj.windowonresize();
            }
        } catch (e) {
            //$scope.ShowResultCustom(e, 'failure');
        }
    };
    $window.onresize = function (event) {

        $scope.actionCompleteSelected();

    };
    $scope.actionCompleteSelected = function (args) {
        try {
            if (args.requestType === "refresh") {
                var gridObj = $("#GridPO").ejGrid("instance");
                var scrollerwidth = $("#id1").width();//Obtain the width of the container

                //   $("#GridReq").children('.e-grid.e-headercell').css('height', '100px');              
                gridObj.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth - 20, height: 400 } });//pass the obtainer width and height to gridmodel options
                gridObj.windowonresize();
            }
        } catch (e) {
            //$scope.ShowResultCustom(e, 'failure');
        }
    };


    $scope.onClick1 = function (args) {

        var gridObj = $("#GridPO12").data("ejGrid");
        //getting corresponding record             
        var data = gridObj.getSelectedRecords()[0];
        //alert('jj' + data.Id);
        // $scope.valuePassInDelModal(data); 
        location.href = "Products/Requisition/RequisitionReportby?RequisitionId=" + data.Id;

    };
    $scope.command = [{
        type: "details", buttonOptions: {
            text: "Print",
            width: "100",
            height: "30",
            click: $scope.onClick1
        }
    }];
}



