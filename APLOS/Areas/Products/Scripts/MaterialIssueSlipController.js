'use strict';
MaterialIssueSlipController.$inject = ['addressService', '$window', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter', '$controller', '$location'];
function MaterialIssueSlipController(addressService, $window, cboService, commonMessage, $scope, $rootScope, baseService, $http, $filter, $controller, $location) {

    ///////////////////////////////////////////////Material Issue Slip///////////////////////////

    $scope.Action = 'Save';

    $rootScope.title = 'Issue Slip';
    $scope.recipeMaterialList = [];
    $scope.popUpList = [];
    $scope.valueData = '';
    $scope.filedata = '';
    $scope.message = null;
    $scope.imageSrc = null;
    $scope.Action = 'Save';
    $scope.maxDate = new Date().toDateString();
    $scope.exportgriddataUrl = 'GridReports/ExcelExport';
    $scope.downloadgriddataUrl = 'GridReports/Download';
    $scope.path = 'Products/GoodsReceiveNote/';
    $scope.Action1 = 'Save';
    $scope.loadstatus = false;
    $scope.lstIssueDetailData = [];
    $scope.partyList = [];
    //$scope.path1 = 'OrderManagements/ProductionOrder/';
    $scope.getListUrl = $scope.path + 'GetProductionList';

    $scope.product = {
        OrderSpecific: 'No',
        ProcessId: null,
        CheckedBy: null

    };
    $scope.productNew = Object.assign({}, $scope.product);
    //#region notification setting
    $scope.ClearList = function (data) {
        debugger;
        $scope.inventoryMaterialList = [];
        $scope.OrderSpecific = data;

    };
    $scope.searchCol = "";
    $scope.searchVal = "";
    $scope.PRsearchBy = "Id";
    $scope.PRsearch = "";
    $scope.PRFilterList = [
        { 'name': 'Prod. Order#', 'value': 'Id' },
        { 'name': 'Prod. Status', 'value': 'ProductionStatus' },
        { 'name': 'Material', 'value': 'Material' },
        { 'name': 'Product', 'value': 'Product' },
        { 'name': 'Product Category', 'value': 'ProductCategory' },
        { 'name': 'Master Order No', 'value': 'MasterOrderId' },
        { 'name': 'Buyer Order#', 'value': 'BuyerRefNo' },
        { 'name': 'Own Order#', 'value': 'OwnRefNo' },
        { 'name': 'Buyer Item#', 'value': 'StyleNo' },
        { 'name': 'Own Item#', 'value': 'OwnStyleNo' },
        { 'name': 'SO No', 'value': 'SONo' },
        { 'name': 'Buyer', 'value': 'buyer' },
        { 'name': 'Customer', 'value': 'Customer' },
    ];
    $scope.getDataProductions = function () {
        $scope.modelList = [];
        $http({
            method: 'GET',
            data: { 'parameters': null },
            url: $scope.getListUrl + "?column=" + $scope.PRsearchBy + "&value=" + $scope.PRsearch
        }).then(function successCallback(response) {
            $scope.modelList = response.data;
        });
    };
    $scope.getDataProductions();

    $scope.DisableActionButtons = false;
    $scope.model = {
        Id: null
        , RecipeId: null
        , PlantId: $window.plantid
        , EntityId: null
        , ProductionStatusId: null
        , FirstInputDate: null
        , TargetCommitmentDate: null
        , Lsd: null
        , LsdRemark: null
        , TargetLsd: null
        , CommitmentDate: null
        , CommitmentDateRemarks: null
        , CalculationBasis: null
        , SPT: null
        , NoOfWorkStation: null
        , MinRequiredTargetHourly: null
        , Cm: null
        , CmCurrencyId: null
        , Efficiency: null
        , FirstDayOutPut: null
        , IncrementType: null
        , IncrementValue: null
        , MinAllocatedLine: null
        , Qty: null
        , StandardTime: null
        , MinWorkingDays: null
        , ProductionPriority: null
        , DaysToGetTheTarget: null
        , Remarks: null
        , color: '#ffffff'
    };
    $scope.model = Object.assign({}, $scope.model);
    $scope.SOListSelected = [];
    $scope.Get = function (Row) {
        $scope.TotalSPT = 0;
        $scope.TotalWorkStation = 0;
        $scope.TotalManpower = 0;
        $scope.OrganizationEfficiency = 0;
        $scope.ProductionEfficiencyPerHour = 0;
        $scope.TotalManpower = 0;
        $scope.PitchTime = 0;
        $scope.ProductionEfficiencyPerDay = 0;
        $scope.MaxAllottedTime = 0;
        $scope.LineTargetPerHour = 0;
        $scope.MCtotalspt = 0;
        $scope.NonMCtotalspt = 0;

        $scope.TotalMP = 0;
        $scope.MCtotalMP = 0;
        $scope.NonMCtotalMP = 0;

        $scope.DisableActionButtons = true;
        $scope.operationList = [];
        $scope.model = Row.data;
        //$scope.model = Object.assign({}, $scope.model);
        $scope.model = Object.assign({}, Row.data);

        getProductionRecipeMaterialList();
        GetProcessByProductionOrder();

        $scope.bulletintab = false;

    };


    function getProductionRecipeMaterialList() {
        $http({
            method: 'GET',
            url: $scope.path + 'GetProductionRecipeMaterialList?productionOrderId=' + $scope.model.Id
        }).then(function successCallback(response) {
            $scope.SOListSelected = response.data;
            //getProductionProcessSetList();
        });
    }
    $scope.processList = [];
    function GetProcessByProductionOrder() {

        $http({
            method: 'GET',
            url: $scope.path + 'GetProcessByProductionOrder?productionOrderId=' + $scope.model.Id
        }).then(function successCallback(response) {
            $scope.processList = response.data;
            //getProductionProcessSetList();
        });
    }

    $scope.filterComplete = function () {
        if ($scope.SOListSelected.Active === true) {
            var parameters = [];
            parameters.push({ "Key": "SalesOrderId", "Value": getString(filteredRecords, "SalesOrderId") });
            $scope.GetMaterialWithSKUData(parameters);
        }
    }
    var getString = function (data, column) {
        var string = "''";
        var collection = [];
        for (var i = 0; i < data.length; i++) {
            if (collection.includes(data[i][column]) == false) {
                string += ",'" + data[i][column] + "'";
                collection.push(data[i][column]);
            }
        }
        return string;
    }

    $scope.MaterialColorList = [];
    $scope.GetMaterialWithSKUData = function (parameters) {
        var id1 = "''";
        for (var i = 0; i < $scope.SOListSelected.length; i++) {
            if ($scope.SOListSelected[i].Active === true) {
                id1 += ",'" + $scope.SOListSelected[i].SalesOrderId + "'";
            }
        }

        $http({
            method: 'GET',
            url: $scope.path + 'GetMaterialWithSKU?ProcessId=' + $scope.productNew.ProcessId + '&parameters=' + id1
        }).then(function successCallback(response) {
            $scope.MaterialColorList = response.data;
        });
    }

    $scope.MaterialColorList = [];
    $scope.FilterList123 = [];
    $scope.FilterList1234 = [];
    $scope.GetMaterialListForProductionReq = function () {

        for (var i = 0; i < $scope.MaterialColorList.length; i++) {
            if ($scope.MaterialColorList[i].Active === true && baseService.isUndefinedOrNull($scope.MaterialColorList[i].RequisitionForQty)) {
                ShowResult('Enter the requested qty', 'failure');
                return false;
            }
        }


        if (baseService.isUndefinedOrNull($scope.productNew.ProcessId)) {
            ShowResult('Please select the process', 'failure');
            return false;
        }
        else {
            var SOMATART = "''";
            var Skuvalue1 = "''";
            var Skuvalue2 = "''";
            var Skuvalue3 = "''";
            var Material = "''";
            var Article = "''";
            var id1 = "''";
            var queryString = "";

            var count = 0;
            for (var i = 0; i < $scope.MaterialColorList.length; i++) {
                if ($scope.MaterialColorList[i].Active === true) {
                    var getRowlength = $filter("filter")($scope.MaterialColorList, { "Active": true });
                    count++;
                    var queryString1 = getRowlength.length;
                    Material += ",'" + $scope.MaterialColorList[i].MaterialMasterId + "'";
                    Article += ",'" + $scope.MaterialColorList[i].ArticleId + "'";
                    Skuvalue1 += ",'" + $scope.MaterialColorList[i].FirstCharacteristicsValueId + "'";
                    Skuvalue2 += ",'" + $scope.MaterialColorList[i].SecondCharacteristicsValueId + "'";
                    Skuvalue3 += ",'" + $scope.MaterialColorList[i].ThirdCharacteristicsValueId + "'";
                    id1 += ",'" + $scope.MaterialColorList[i].SalesOrderId + "'";
                    SOMATART += ",'" + $scope.MaterialColorList[i].SOMATART + "'";
                    //queryString += ",'" + SOMATART+"'+" + " " + ID + "," + '" + $scope.MaterialColorList[i].RequisitionForQty + "'" + " " + Qty"";
                    if (queryString1 > count) {
                        queryString += " Select  '" + $scope.MaterialColorList[i].SOMATART + "'" + ' ID ' + '' + ",'" + $scope.MaterialColorList[i].RequisitionForQty + "'" + ' Qty  UNION ALL ';

                    }
                    else {
                        queryString += " Select '" + $scope.MaterialColorList[i].SOMATART + "'" + ' ID ' + '' + ",'" + $scope.MaterialColorList[i].RequisitionForQty + "'" + ' Qty ';

                    }
                }
            }
            $scope.FilterList123 = [];
            $http({
                method: 'GET',
                url: $scope.path + 'GetMaterialListForProductionReq?Material=' + Material + '&Article=' + Article + '&Skuvalue1=' + Skuvalue1 + '&Skuvalue2=' + Skuvalue2 + '&Skuvalue3=' + Skuvalue3 + '&ProcessId=' + $scope.productNew.ProcessId + '&parameters=' + id1 + '&SOMATART=' + SOMATART + '&queryString=' + queryString
            }).then(function successCallback(response) {
                $scope.FilterList123 = response.data;

            });


        }
    }
    $scope.ShowStock = [];
    $scope.GetSOWiseMaterialStock = function (x, $index) {
        $scope.GetDetailGridIndex = $index;
        $http({
            method: 'GET',
            url: $scope.path + 'GetSOWiseMaterialStock?Material=' + x.MaterialMasterId + '&Article=' + x.ArticleId + '&Skuvalue1=' + x.BOQDFirstCharacteristicsValueId + '&Skuvalue2=' + x.BOQDSecondCharacteristicsValueId + '&Skuvalue3=' + x.BOQDThirdCharacteristicsValueId + '&ProcessId=' + $scope.productNew.ProcessId + '&SalesOrderId=' + x.SalesOrderId
        }).then(function successCallback(response) {
            $scope.ShowStock = response.data;

        });

    }

    $scope.SetTheData = function (x, $index) {

        var gridObj = $("#ShowStock1").data("ejGrid");
        //getting corresponding record 
        $scope.data = gridObj.getSelectedRecords()[0];
        if (baseService.isUndefinedOrNull($scope.data.RequestedQty) || $scope.data.RequestedQty === 0) {
            return ShowResult('Enter the Slip Qty', 'failure', 'POPopUp');
        }
        else {
            $scope.FilterList123[$scope.GetDetailGridIndex].RequestedQty = $scope.data.RequestedQty;
            $scope.FilterList123[$scope.GetDetailGridIndex].TransactionUoMName = $scope.data.TransactionUoMName;
            $scope.FilterList123[$scope.GetDetailGridIndex].TransactionUoMId = $scope.data.TransactionUoMId;
            //$scope.FilterList123[$scope.GetDetailGridIndex].RequisitionQty = $scope.data.BaseUOMFactor;

            angular.element(document.querySelector('#POPopUp')).modal('hide');
        }



    }
    $scope.checkedByList = [];
       //**********To Checked By**************
    //$scope.GetSupervisorCboList = function () {
    //    $http({
    //        method: 'GET',
    //        url: 'Products/PurchaseOrder/GetSupervisorCbo'
    //    }).then(function successCallback(response) {
    //        $scope.checkedByList = response.data;
    //    });
    //}
    //$scope.GetSupervisorCboList();
    //********** To Checked By**************
    $scope.GetCheckedByAndApprovedBy1 = function () {
        if (!baseService.isUndefinedOrNull($scope.CheckedByStatusForNoti) && !baseService.isUndefinedOrNull($scope.ApprovedByStatusForNoti)) {
            $http({
                method: 'GET',
                url: 'Products/GoodsReceiveNote/GetCheckedByAndApprovedBY?CheckedBy=' + $scope.CheckedByStatusForNoti + '&ApprovedBy=' + $scope.ApprovedByStatusForNoti,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                $scope.checkedByList = response.data;
            });
        }
    }
    $scope.NotificationSettingStatus = function () {
        //debugger;
        $http({
            method: 'GET',
            url: 'Products/GoodsReceiveNote/NotificationSetting',
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.NotificationSetting = response.data;
            $scope.CheckedByStatusForNoti = $scope.NotificationSetting[0].RequiredChecking;
            $scope.ApprovedByStatusForNoti = $scope.NotificationSetting[0].RequiredApproval;
            $scope.GetCheckedByAndApprovedBy1();
            if ($scope.CheckedByStatusForNoti === true && $scope.ApprovedByStatusForNoti === false) {
                $scope.labelCheckAndApproved = 'To be checked by';
            }
            else if ($scope.CheckedByStatusForNoti === false && $scope.ApprovedByStatusForNoti === true) {
                $scope.labelCheckAndApproved = 'To be approved by';
            }
            else if ($scope.CheckedByStatusForNoti === true && $scope.ApprovedByStatusForNoti === true) {
                $scope.labelCheckAndApproved = 'To be checked by';
            }
            //else {
            //    $scope.productNew.labelCheckAndApproved = 'To be checked/approved by';
            //}

        });
    }
    $scope.NotificationSettingStatus();
    

    $scope.IssueDetailData = function () {
        //debugger;
        $http({
            method: 'GET',
            url: 'Products/GoodsReceiveNote/IssueDetailData'
        }).then(function successCallback(response) {
            $scope.lstIssueDetailData = response.data;
            //$scope.detailgrid($scope.lst);
            window.lstIssueDetailData = response.data;

        });
    }
    $scope.IssueDetailData();

    $scope.FilterList123 = [];

    $scope.data1 = $scope.lst;
    $scope.detailTemp = "#tabGridContents";
    //$scope.detailgrid = "detailGridData(e)";
    $scope.detailgridIssue = function detailGridData(e) {
        //debugger;
        var filteredData = e.data["Id"];
        var data = ej.DataManager(window.lstIssueDetailData).executeLocal(ej.Query().where("IssueMasterId", "equal", parseInt(filteredData), true).take(100));
        e.detailsElement.find("#detailGrid").ejGrid({
            dataSource: data,
            columns: ["EntityName", "CostCenterName", "ExpenceActivityCode", "Activity", "MaterialType", "MaterialMasterGroupName", "MaterialMasterName", "ArticleId", "RequisitionNo", "RequisitionDetailId", "DepartmentName", "AddedBy", "ApprovedQty", "RejectionQty", "TotalQty", "IssuedQty"]
        });
        e.detailsElement.find(".tabcontrol").ejTab();
    }




    $scope.lst = [];
    $scope.IssueSlipDetail = function () {
        //debugger;
        $http({
            method: 'GET',
            url: 'Products/GoodsReceiveNote/IssueSlipDetail'
        }).then(function successCallback(response) {
            $scope.lst = response.data;
            //$scope.detailgrid($scope.lst);
            window.lst = response.data;

        });
    }
    $scope.IssueSlipDetail();



    $scope.data1 = $scope.lst;
    $scope.detailTemp = "#tabGridContents";
    //$scope.detailgrid = "detailGridData(e)";
    $scope.detailgrid = function detailGridData(e) {
        //debugger;

        var filteredData = e.data["Id"];
        var data = ej.DataManager(window.lst).executeLocal(ej.Query().where("IssueRequestMasterId", "equal", parseInt(filteredData), true).take(100));
        e.detailsElement.find("#detailGrid").ejGrid({

            dataSource: data,
            columns: ["CostCenterName", "GLBudgetActivity", "MaterialType", "MaterialGroup", "Material", "ArticleName", "Sku1", "Sku2", "Sku3", "AddedBy", "UOM", "RequestedQty"]
        });
        e.detailsElement.find(".tabcontrol").ejTab();
    }
    //#endregion
    $scope.requisitionIssueDetailList = [];



    $scope.IssueSlipList = [];
    $scope.IssueSlipHoldRejectList=[]
    $scope.IssueSlipCheckedList=[]
    $scope.IssueStatus = 'ForChecked';
    
    $scope.Griddata = function (issueStatus) {
        $scope.IssueSlipList = [];
        $scope.IssueSlipHoldRejectList = []
        $scope.IssueSlipCheckedList = []
        $scope.Status = 'InventorySlip';
        $http({
            method: 'GET',
            url: 'Products/GoodsReceiveNote/IssueListData?IssueStatus=' + issueStatus + '&IssueSlipType=' + $scope.Status
        }).then(function successCallback(response) {
            if (issueStatus == 'ForChecked') {
                $scope.IssueSlipList = response.data;
            }
            else if (issueStatus == 'Checked') {
                $scope.IssueSlipCheckedList = response.data;
            }
            else if (issueStatus == 'HoldReject') {
                $scope.IssueSlipHoldRejectList = response.data;
            }
        });
    }
    $scope.Griddata('ForChecked');



    $scope.requisitionIssueDetailList = [];
    $scope.ApprovedIssueSlipGList = [];
    $scope.LoadIssueSlipApproveData = function () {

        if ($scope.IssueStatusApproval === 'Approval') {
            $scope.IssueStatusApproval = 'Approval';
        }

        //debugger;
        $http({
            method: 'GET',
            url: 'Products/GoodsReceiveNote/ApprovedIssueSlipGridData?IssueStatusApproval=' + $scope.IssueStatusApproval + '&IssueSlipType=' + $scope.Status
        }).then(function successCallback(response) {
            $scope.ApprovedIssueSlipGList = response.data;
        });
    }
    //$scope.LoadIssueSlipApproveData();



    $scope.recorddoubleclick = function ($event) {
        //debugger;
        var x = $event;
        $scope.OMId = x.data.Id;
        $scope.modelNew.OperationMasterIdID = x.data.Id;
        $scope.GetDataByMasterOrderIdfn($scope.OMId);
        $scope.GetDataByMasterOrderIdfnMP1($scope.OMId);
        $scope.GetOperationPositionMPBudget();
        $scope.GetAutoSequenceForManPower();
        $scope.Action = 'Update';
        //$scope.Action1 = 'Update';       
        if (!$rootScope.isCollapsed) $rootScope.toggle();
    };

    $scope.GetDataByMasterOrderIdfn = function (OMId) {
        //debugger;
        $http({
            method: 'GET',
            url: 'Products/GoodsReceiveNote/IssueListById?id=' + OMId
        }).then(function successCallback(response) {

            $scope.modelNew = response.data[0];
            $scope.modelNew.OperationMasterIdID = response.data[0].Id;

        });
    }


    $scope.AllTabPrint = function (z) {
        //debugger;
        var x = "#" + z;
        var gridObj = $(x).data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        location.href = "Products/GoodsReceiveNote/IssueRequestReport?issueId=" + data.Id;

    };
    $scope.IssueRequestReport = [{
        type: "details", buttonOptions: {
            text: "Print",
            width: "50",
            height: "20",

            click: $scope.IssueRequestReportprint
        }
    }];



    $scope.IssueWithReqPOGRNReportprint = function (args) {
        //debugger
        var gridObj = $("#Grid").data("ejGrid");
        //getting corresponding record             
        var data = gridObj.getSelectedRecords()[0];
        //alert('jj' + data.Id);
        // $scope.valuePassInDelModal(data); 
        location.href = "Products/GoodsReceiveNote/IssueWithReqPOGRNReport?Id=" + data.Id;

    };
    $scope.IssueReqPOGRNReport = [{
        type: "details", buttonOptions: {
            text: "Print",
            width: "50",
            height: "20",

            click: $scope.IssueWithReqPOGRNReportprint
        }
    }];

    $scope.loadNewEmployee = function () {
        $scope.excluedEmpColumn = ['Email', 'Reason', 'position', 'ResignationDate', 'AttachLetter', 'ApprovalStatus', 'EffectiveDate', 'Picture', 'IsPastResignationAllowed', 'PastResignationDaysAllowed', 'EmployeeCategory'];
        $scope.popUpParameters = {
            limit: 10,
            offset: 0,
            order: 'asc',
            sort: 'EmployeeCode',
            searchBy: 'EmployeeCode',
            pageSize: 10,
            total_count: 0,
            search: null,
            serverPagination: true
        };
        $scope.popUpUrl = 'employees/resignation/newList?plantId=' + $scope.Resignation.PlantId;
        baseService.setCurrentPage('dataList');
        $scope.getPopUpData = function (pageno) {
            baseService.paginationBase($scope.popUpUrl, pageno, $scope.popUpParameters)
                .then(function (result) {
                    $scope.popUpDataList = result.Rows;
                    $scope.popUpParameters.total_count = result.Total;

                    if (baseService.arrayLength($scope.popUpList) === 0)
                        baseService.getDDLSearchColumn(result.Rows, $scope.popUpList);
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure', 'popUpId1');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#popUpId1')).modal('show');
        $scope.getPopUpData();
    };
    $scope.loadPendingEmployee = function () {
        $scope.excluedEmpColumn = ['Email', 'Reason', 'position', 'ResignationDate', 'AttachLetter', 'ApprovalStatus', 'EffectiveDate', 'Picture', 'IsPastResignationAllowed', 'PastResignationDaysAllowed', 'EmployeeCategory'];
        $scope.popUpParameters = {
            limit: 10,
            offset: 0,
            order: 'asc',
            sort: 'EmployeeCode',
            searchBy: 'EmployeeCode',
            pageSize: 10,
            total_count: 0,
            search: null,
            serverPagination: true
        };
        $scope.popUpUrl = 'employees/resignation/pendingList?plantId=' + $scope.Resignation.PlantId;
        baseService.setCurrentPage('dataList');
        $scope.getPopUpData = function (pageno) {
            baseService.paginationBase($scope.popUpUrl, pageno, $scope.popUpParameters)
                .then(function (result) {
                    $scope.popUpDataList = result.Rows;
                    $scope.popUpParameters.total_count = result.Total;

                    if (baseService.arrayLength($scope.popUpList) === 0)
                        baseService.getDDLSearchColumn(result.Rows, $scope.popUpList);
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure', 'popUpId2');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#popUpId2')).modal('show');
        $scope.getPopUpData();
    };
    $scope.closePopUp = function () {
        $scope.valueData = '';
        angular.element(document.querySelector('#popUpId1')).modal('hide');
    };
    $scope.closePopUp2 = function () {
        $scope.valueData = '';
        angular.element(document.querySelector('#popUpId2')).modal('hide');
    };

    function selectNewEmployee(data) {
        $scope.Resignation.Id = data.Id;
        $scope.imageSrc = virtualPath.EmployeePic + data.Picture;
        $scope.Resignation.EmployeeId = data.EmployeeId;
        $scope.Resignation.EmployeeName = data.EmployeeName;
        $scope.Resignation.EmployeeCode = data.EmployeeCode;
        $scope.Resignation.GivenDesignation = data.GivenDesignation;
        $scope.Resignation.Designation = data.Designation;
        $scope.Resignation.DOJ = data.DOJ;
        $scope.Resignation.DOC = data.DOC;
        $scope.Resignation.EmployeeCategory = data.EmployeeCategory;
        $scope.Resignation.PlantId = data.PlantId;
        $scope.Resignation.EmployeeCategory = data.EmployeeCategory;
        $scope.Resignation.Id = data.Id;
        $scope.Resignation.IsPastResignationAllowed = data.IsPastResignationAllowed;
        $scope.Resignation.PastResignationDaysAllowed = data.PastResignationDaysAllowed;
        $scope.Resignation.Entity = data.Entity;
        $scope.Resignation.AttachLetter = data.AttachLetter;
        $scope.closePopUp();
        $scope.Action = 'Save';
    }
    function selectPendingEmployee(data) {
        $scope.Resignation.Id = data.Id;
        $scope.imageSrc = virtualPath.EmployeePic + data.Picture;
        $scope.Resignation.EmployeeId = data.EmployeeId;
        $scope.Resignation.EmployeeName = data.EmployeeName;
        $scope.Resignation.EmployeeCode = data.EmployeeCode;
        $scope.Resignation.GivenDesignation = data.GivenDesignation;
        $scope.Resignation.Designation = data.Designation;
        $scope.Resignation.DOJ = data.DOJ;
        $scope.Resignation.DOC = data.DOC;
        $scope.Resignation.EmployeeCategory = data.EmployeeCategory;
        $scope.Resignation.PlantId = data.PlantId;
        $scope.Resignation.ResignationDate = data.ResignationDate;
        $scope.Resignation.EffectiveDate = data.EffectiveDate;
        $scope.Resignation.EmployeeCategory = data.EmployeeCategory;
        $scope.Resignation.Reason = data.Reason;
        $scope.Resignation.Id = data.Id;
        $scope.Resignation.IsPastResignationAllowed = data.IsPastResignationAllowed;
        $scope.Resignation.PastResignationDaysAllowed = data.PastResignationDaysAllowed;
        $scope.Resignation.Entity = data.Entity;
        $scope.Resignation.AttachLetter = data.AttachLetter;
        document.getElementById('abc').value = data.AttachLetter;
        $scope.closePopUp2();
        $scope.Action = 'Update';
    }
    $scope.loadResignationHistory = function (Id) {
        $http.get('employees/resignation/getResignationHistoryById?EmployeeId=' + $scope.Resignation.EmployeeId)
            .then(function (response) {
                $scope.entityList = response.data;
            });
        angular.element(document.querySelector('#ResignationHistoryPopUp')).modal('show');
    };

    function CheckField(fieldValue, fieldName) {
        try {
            if (fieldValue === null || fieldValue === '') {
                throw '[' + fieldName + '] is required...';
            }
        } catch (e) {
            throw e;
        }
    }
    function Validate() {
        try {
            CheckField($scope.Resignation.PlantId, 'Plant');
            CheckField($scope.Resignation.EmployeeName, 'Employee Name');
            CheckField($scope.Resignation.ResignationDate, 'Resignation Submission Date');
            CheckField($scope.Resignation.EffectiveDate, 'Applied Effective Date');
            CheckField($scope.Resignation.Reason, 'Reason');
            CheckField($scope.Resignation.AttachLetter, 'Resignation Letter');
            var regDate = new Date($scope.Resignation.ResignationDate);
            var effDate = new Date($scope.Resignation.EffectiveDate);
            var dojDate = new Date($scope.Resignation.DOJ);

            if (dojDate > regDate) {
                throw 'Resignation date must be greater than Date of Join'
            }
            if (regDate > effDate) {
                throw 'Applied Effective date cannot be less than Resignation date'
            }

            var d = new Date();
            var d1 = $filter('date')(d, 'dd-MMM-yy');
            var d3 = $filter('date')(regDate, 'dd-MMM-yy');
            var resignationDate = new Date(d3);
            var today = new Date(d1);
            if (resignationDate > today) {
                throw 'Future Resignation date is not allowed';
            }

            var effDate2 = $filter('date')(effDate, 'dd-MMM-yy')
            var effectiveDate = new Date(effDate2);

            d.setDate(d.getDate() + 90);
            var d1 = $filter('date')(d, 'dd-MMM-yy');
            var d2 = new Date(d1);
            if (effDate > d2) {
                throw 'Applied Effective Date Cannot be Greater then [' + d1 + ']'
            }

            var allowDays = new Date();
            allowDays.setDate(d.getDate() - $scope.Resignation.PastResignationDaysAllowed);
            var d7 = $filter('date')(allowDays, 'dd-MMM-yy');
            var d8 = new Date(d7);
            if ($scope.Resignation.IsPastResignationAllowed === true) {
                if (d8 > regDate) {
                    throw 'Past Resignation date before [' + d7 + '] Days is not allowed';
                }
            }
        } catch (e) {
            throw e;
        }
    }

    $scope.showSearch = function (flag) {
        try {
            $scope.search_flag = flag;
            switch (flag) {
                case 'PendingEMP':
                    CheckField($scope.Resignation.PlantId, 'Plant');
                    $scope.loadPendingEmployee();
                    break;
                case 'NewEMP':
                    CheckField($scope.Resignation.PlantId, 'Plant');
                    $scope.loadNewEmployee();
                    break;
                default:
                    return ShowResult('Search Flag is not defined!!!', 'failure');
            }
            //angular.element(document.querySelector('#popUpId')).modal('show');
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };
    $scope.getSearchObject = function (ob) {
        try {
            switch ($scope.search_flag) {
                case 'PendingEMP':
                    selectPendingEmployee(ob);
                    break;
                case 'NewEMP':
                    selectNewEmployee(ob);
                    break;
                default:
            }
            $scope.search_flag = '';
            //angular.element(document.querySelector('#search_popup')).modal('hide');
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.Clear = function () {
        $scope.FilterList123 = [];
        $scope.MaterialColorList = [];
        $scope.modelList = [];
        $scope.SOListSelected = [];
        $scope.getDataProductions();

        $scope.CheckedBy = "";
        //var gridObj = $("#Grid22").data("ejGrid");
        //gridObj.clearFiltering();

        $scope.productNew.ProcessId = '';
        $scope.productNew.CheckedBy = '';
    };
    function ClearOb(obj) {
        for (var i in obj) {
            obj[i] = null;
        }
    }
    $('#uploadBtn').change(function () {
        $scope.filedata = this.files[0];
        $scope.Resignation.AttachLetter = null;
        $scope.Resignation.AttachLetter = $scope.filedata.name;
        document.getElementById('abc').value = $scope.filedata.name;
    });
    $scope.AttachRemove = function () {
        // $scope.message_confirmation = 'Are you sure to remove this file?';
        // angular.element(document.querySelector('#confirmDelete')).modal('show');
        $scope.filedata = [];
        document.getElementById('uploadBtn').value = null;
        $scope.Resignation.AttachLetter = null;
        document.getElementById('abc').value = '';
    };

    $scope.LeaveTest = function () {
        try {
            $http({
                method: 'POST',
                url: 'employees/resignation/leaveSummary?CompanyGroupId=CG20181',
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                }
            }, function errorCallback(response) {
                $scope.savedisable = false;
                ShowResult(response.status.Message, 'failure');
            });
            return true;
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };


    //$scope.LoadData($scope.user);


    $scope.ClearFilter = function () {
        //debugger;
        var gridObj = $("#Grid2").data("ejGrid");
        gridObj.refreshContent(); // Refreshes the grid contents only 
        gridObj.refreshContent(true); // Refreshes the

        gridObj = $("#DetailGrid").data("ejGrid");
        gridObj.refreshContent(); // Refreshes the grid contents only 
        gridObj.refreshContent(true); // Refreshes the


    }

    $scope.FilterList = [];
    $scope.FilterListData = function () {
        //debugger;
        $http({
            method: 'GET',
            url: 'Products/GoodsReceiveNote/IssueSlipFilter',
        }).then(function successCallback(response) {
            $scope.FilterList = response.data;
            //$scope.detailgrid($scope.lst);
            //  window.lst = response.data;

        });
    }
    //$scope.FilterListData();




    $scope.GetEntity = function () {
        //debugger;
        $.ajax({
            type: "GET",
            contentType: "application/json; charset=utf-8",
            url: 'Products/GoodsReceiveNote/IssueSlipFilter',
            data: {},
            async: false,
            dataType: "json",
            success: function (data) {
                //$scope.FilterList = data;
                $("#Grid2").ejGrid({

                    dataSource: data, // data must be array of json
                    allowPaging: true,
                    //allowSorting: true,
                    allowFiltering: true,
                    isResponsive: true,
                    enableResponsiveRow: true,
                    allowTextWrap: true,
                    textWrapSettings: { wrapMode: "header" },
                    cssClass: "filtered",
                    filterSettings: {
                        filterType: "excel"
                    },
                    // pageSize: 1,
                    allowScrolling: true,
                    scrollSettings: { width: "auto", height: "2" },

                    columns: [
                        { headerText: "Entity Name", field: "EntityName", width: 60 },
                        { headerText: "ActivityName", field: "UserName", width: 80 },
                        { headerText: "Material Type", field: "MaterialType", width: 60 },
                        { headerText: "Group Name", field: "MaterialMasterGroupName", width: 90 },
                        //{ headerText: "Group Name2", field: "MaterialMasterGroupName", width: 90 },
                        { headerText: "MaterialMasterName", field: "MaterialMasterName", width: 90 },

                        { headerText: "Sku1", field: "FirstCharacteristicsValueId", width: 90 },
                        { headerText: "Sku2", field: "SecondCharacteristicsValueId", width: 90 },
                        { headerText: "Sku3", field: "ThirdCharacteristicsValueId", width: 90 },


                        { headerText: "Article", field: "StandardName", width: 80 },
                        { headerText: "RequisitionBy", field: "AddedBy", width: 85 },
                        { headerText: "RequisitionNo", field: "RequisitionNo", width: 95 },
                        { headerText: "Department Name", field: "DepartmentName", width: 80 }


                    ]
                });

                $("#Grid2").children('.e-pager.e-js.e-pager').hide();
                $("#Grid2").children('.e-gridcontent.e-droppable.e-js').hide();
                $("#Grid2").children('.e-gridcontent').hide();
                //$("#Grid2").children('.e-grid .e-headercell {background - color: chocolate;}').add();

                $("#Grid2").children('.e-grid.e-headercell').css('background-color', 'red'); //{background - color: chocolate;}').add();
            }

        });
    }
    $scope.GetEntity();

    $scope.getData = function () {
        //debugger;

        var obj = $("#Grid2").ejGrid("instance");
        var sd = obj.getFilteredRecords();
        if (sd.length == 0) {
            sd = obj.model.dataSource;
            //alert('1' +1);
        }
        $scope.FilterList1 = sd;
    }


    $scope.materialValidation = function () {
        $scope.invalid = true;

    }
    $scope.detailSave = function () {

        //debugger;

        try {

            //$scope.GetListForMasterOrdernew = [];
            for (var i = 0; i < $scope.FilterList1.length; i++) {
                if ($scope.FilterList1[i].RequestedQty === 0) {
                    ShowResult('Enter the Requested Qty', 'failure');
                    return false;
                }
                else if ($scope.FilterList1[i].RejectedQty === 0) {
                    ShowResult('Enter Rejection Qty', 'failure');
                    return false;
                }
                else if ($scope.FilterList1[i].RequestedQty > $scope.FilterList1[i].ApprovedQty) {
                    ShowResult('Requested Qty can not grater than Own Qty', 'failure');
                    return false;
                }
                else if ($scope.FilterList1[i].RejectedQty > $scope.FilterList1[i].RejectionQty1) {
                    ShowResult('Rejection Qty can not grater than Own Rejected Qty', 'failure');
                    return false;
                }
                else if ($scope.FilterList1[$scope.issueSlipDetailIndex].ExpenseActivityId === "") {
                    ShowResult('Please select Expense Activity Code', 'failure');
                    return false;
                }

            }
            // $scope.processgroupList($scope.GetListForMasterOrdernew, $scope.groupList);

            $scope.$broadcast('show-errors-check-validity');
            if ($scope.productNewForm.$valid) {
                $scope.materialValidation();
                if ($scope.invalid) {
                    if ($scope.Action1 === 'Save') {
                        $http({
                            method: 'POST',
                            url: 'Products/GoodsReceiveNote/IssueSlipCreate',
                            data: {
                                entity: $scope.FilterList1
                                , CheckedBy: $scope.CheckedBy

                            },
                            dataType: 'JSON'
                        }).then(function successCallback(response) {
                            if (response.data.Error === true)
                                ShowResult(response.data.Message, 'failure');
                            else {
                                ShowResult(response.data.Message, 'success');
                                $scope.Status = 'InventorySlip';
                                $scope.Griddata();
                                $scope.GriddataAssetIssueSlip();
                                getInventoryMaterialList($scope.productNew.Id);

                            }
                        }), function errorCallBack(response) {
                            ShowResult(response.data.Message, 'failure');
                        };

                    }
                    else if ($scope.Action1 === "Update") {
                        $http({
                            method: 'POST',
                            url: 'Products/GoodsReceiveNote/IssueSlipUpdate',
                            data: {
                                entity: $scope.FilterList1
                                , Id: $scope.Id
                                , CheckedBy: $scope.CheckedBy
                            },
                            dataType: 'JSON'
                        }).then(function successCallback(response) {
                            if (response.data.Error === true)
                                ShowResult(response.data.Message, 'failure');
                            else {
                                ShowResult(response.data.Message, 'success');
                                $scope.Status = 'InventorySlip';
                                $scope.Griddata();
                                $scope.GriddataAssetIssueSlip();
                                getInventoryMaterialList($scope.productNew.Id);

                            }
                        }), function errorCallBack(response) {
                            ShowResult(response.data.Message, 'failure');
                        };

                    }
                }

            }



        } catch (e) {
            //ShowResult(e, 'fail', 'detailPopUp');
        }
    };
    $scope.FilterList1234 = [];
    $scope.detailSaveIssue = function () {
        $scope.FilterList123New = [];
        $scope.FilterList1234 = [];
        //debugger;

        try {
            if ($scope.FilterList123.length == 0) {
                ShowResult('Please Add Issue Slip', 'failure');
                return false;
            }
            //$scope.GetListForMasterOrdernew = [];
            for (var i = 0; i < $scope.FilterList123.length; i++) {
                if ($scope.FilterList123[i].check === true) {
                    if (baseService.isUndefinedOrNull($scope.FilterList123[i].RequestedQty) || $scope.FilterList123[i].RequestedQty === 0) {
                        ShowResult('Enter the Requested Qty', 'failure');
                        return false;
                    }
                    else if (baseService.isUndefinedOrNull($scope.FilterList123[i].CostCenterId) && $scope.FilterList123[i].check === true) {
                        ShowResult('Please select cost center', 'failure');
                        return false;
                    }
                    else if (baseService.isUndefinedOrNull($scope.FilterList123[i].CostCenterId) && $scope.FilterList123[i].check === true) {
                        ShowResult('Please select cost center', 'failure');
                        return false;
                    }
                    else if (baseService.isUndefinedOrNull($scope.FilterList123[i].RequestedQty) && $scope.FilterList123[i].check === true) {
                        ShowResult('Enter the Required Qty', 'failure');
                        return false;
                    }
                    else if (baseService.isUndefinedOrNull($scope.FilterList123[i].ArticleId) && $scope.FilterList123[i].check === true) {
                        ShowResult('No Article found', 'failure');
                        return false;
                    }

                    else {
                        $scope.FilterList123New.push($scope.FilterList123[i]);
                    }
                }


            }
            $scope.SOListSelectedNew = [];
            $scope.MaterialColorListNew = [];
            for (var i = 0; i < $scope.SOListSelected.length; i++) {
                if ($scope.SOListSelected[i].Active === true) {
                    $scope.SOListSelectedNew.push($scope.SOListSelected[i]);
                }
            }
            for (var i = 0; i < $scope.MaterialColorList.length; i++) {
                if ($scope.MaterialColorList[i].Active === true) {
                    $scope.MaterialColorListNew.push($scope.MaterialColorList[i]);
                }
            }
            if (baseService.isUndefinedOrNull($scope.productNew.CheckedBy)) {
                ShowResult('Select the to be checked by/approved by', 'failure');
                return false;
            }
            for (var i2 = 0; i2 < $scope.FilterList123.length; i2++) {
                if ($scope.FilterList123[i2].check === true) {
                    $scope.FilterList123[i2].RequestedQtyNew = Math.round($scope.FilterList123[i2].RequestedQty * 100 + Number.EPSILON) / 100;
                    var getRow1 = $filter("filter")($scope.FilterList1234, { "MaterialMasterId": $scope.FilterList123[i2].MaterialMasterId, "ArticleId": $scope.FilterList123[i2].ArticleId, "BOQDFirstCharacteristicsValueId": $scope.FilterList123[i2].BOQDFirstCharacteristicsValueId, "BOQDSecondCharacteristicsValueId": $scope.FilterList123[i2].BOQDSecondCharacteristicsValueId, "BOQDThirdCharacteristicsValueId": $scope.FilterList123[i2].BOQDThirdCharacteristicsValueId, "TransactionUoMId": $scope.FilterList123[i2].TransactionUoMId, "SalesOrderId": $scope.FilterList123[i2].SalesOrderId, "check": true });


                    if (getRow1.length === 0) {

                        $scope.FilterList1234.push($scope.FilterList123[i2])
                        $scope.FilterList1234.RequestedQtyNew = Math.round($scope.FilterList123[i2].RequestedQtyNew * 100 + Number.EPSILON) / 100;
                    }
                    else {
                        for (var i1 = 0; i1 < $scope.FilterList1234.length; i1++) {

                            if ($scope.FilterList1234[i1].MaterialMasterId === $scope.FilterList123[i2].MaterialMasterId
                                && $scope.FilterList1234[i1].ArticleId === $scope.FilterList123[i2].ArticleId
                                && $scope.FilterList1234[i1].BOQDFirstCharacteristicsValueId === $scope.FilterList123[i2].BOQDFirstCharacteristicsValueId
                                && $scope.FilterList1234[i1].BOQDSecondCharacteristicsValueId === $scope.FilterList123[i2].BOQDSecondCharacteristicsValueId
                                && $scope.FilterList1234[i1].BOQDsThirdCharacteristicsValueId === $scope.FilterList123[i2].BOQDsThirdCharacteristicsValueId
                                && $scope.FilterList1234[i1].TransactionUoMId === $scope.FilterList123[i2].TransactionUoMId) {
                                $scope.FilterList1234[i1].RequestedQtyNew += Math.round($scope.FilterList123[i2].RequestedQtyNew * 100 + Number.EPSILON) / 100;;


                            }

                        }

                    }


                }
            }

            // $scope.FilterList1.IssueSlipType = $scope.IssueSlipType;
            // $scope.processgroupList($scope.GetListForMasterOrdernew, $scope.groupList);
            $scope.$broadcast('show-errors-check-validity');
            if ($scope.productNewForm.$valid) {
                $scope.materialValidation();
                if ($scope.invalid) {
                    if ($scope.Action1 === 'Save') {
                        $http({
                            method: 'POST',
                            url: 'Products/GoodsReceiveNote/IssueSlipCreate',
                            data: {
                                entity: JSON.stringify($scope.FilterList123New)
                                , entityGroupData: JSON.stringify($scope.FilterList1234)
                                , CheckedBy: $scope.productNew.CheckedBy
                                , IssueSlipType: $scope.IssueSlipType
                                , AssetIssueTypeStatus: $scope.AssetIssueTypeStatus
                                , 'CheckedByStatusForNoti': $scope.CheckedByStatusForNoti
                                , 'ApprovedByStatusForNoti': $scope.ApprovedByStatusForNoti
                                , 'SOListSelectedNew': JSON.stringify($scope.SOListSelectedNew)
                                , 'MaterialColorListNew': JSON.stringify($scope.MaterialColorListNew)
                                , 'ProcessId': $scope.productNew.ProcessId
                                , 'OrderSpecific': $scope.productNew.OrderSpecific
                                , 'machinepopUpDataList': $scope.machineQtyList
                            },
                            dataType: 'JSON'
                        }).then(function successCallback(response) {
                            if (response.data.Error === true)
                                ShowResult(response.data.Message, 'failure');
                            else {
                                ShowResult(response.data.Message, 'success');
                                $scope.Id = response.data.Issentity.Id;
                                $scope.Status = 'InventorySlip';
                                $scope.Griddata();
                                $scope.IssueSlipDetail();
                                $scope.Clear();

                                $scope.GriddataAssetIssueSlip();
                                getInventoryMaterialList($scope.productNew.Id);
                                $scope.machineQtyList = [];

                            }
                        }), function errorCallBack(response) {
                            ShowResult(response.data.Message, 'failure');
                        };

                    }
                    else if ($scope.Action1 === "Update") {
                        $http({
                            method: 'POST',
                            url: 'Products/GoodsReceiveNote/IssueSlipUpdate',
                            data: {
                                entity: JSON.stringify($scope.FilterList123New)
                                , Id: $scope.Id
                                , CheckedBy: $scope.productNew.CheckedBy
                                , IssueSlipType: $scope.IssueSlipType
                                , AssetIssueTypeStatus: $scope.AssetIssueTypeStatus
                                , 'CheckedByStatusForNoti': $scope.CheckedByStatusForNoti
                                , 'ApprovedByStatusForNoti': $scope.ApprovedByStatusForNoti
                                , 'SOListSelectedNew': JSON.stringify($scope.SOListSelectedNew)
                                , 'MaterialColorListNew': JSON.stringify($scope.MaterialColorListNew)
                                , 'ProcessId': $scope.productNew.ProcessId
                                , 'OrderSpecific': $scope.productNew.OrderSpecific
                            },
                            dataType: 'JSON'
                        }).then(function successCallback(response) {
                            if (response.data.Error === true)
                                ShowResult(response.data.Message, 'failure');
                            else {
                                ShowResult(response.data.Message, 'success');
                                $scope.Status = 'InventorySlip';
                                $scope.Griddata();
                                $scope.IssueSlipDetail();
                                //$scope.GriddataAssetIssueSlip();
                                getInventoryMaterialList($scope.productNew.Id);
                                //$scope.Clear();
                                $scope.machineQtyList = [];
                            }
                        }), function errorCallBack(response) {
                            ShowResult(response.data.Message, 'failure');
                        };

                    }
                }

            }



        } catch (e) {
            //ShowResult(e, 'fail', 'detailPopUp');
        }
    };




    $scope.CostCenterLoad = function () {
        cboService.getCostCenterCbo(function (result) {
            $scope.costCenterList = result;
        });
    }
    $scope.CostCenterLoad();

    //**********Expenses GL Budget Activity**************
    $scope.searchglByList = [
        {
            "name": "GL",
            "value": "GLGeneralInfoName"
        },
        {
            "name": "Budget",
            "value": "BudgetName"
        },
        {
            "name": "Activity",
            "value": "ActivityName"
        },
        {
            "name": "Ref No",
            "value": "RefNo"
        }
    ];

    $scope.glListParameters = {
        limit: 10,
        offset: 0,
        order: "asc",
        sort: "GLGeneralInfoName",
        searchBy: "ActivityName",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.popUp = function (index) {
        //debugger;
        $scope.customerInvoiceGLList = [];
        //baseService.setCurrentPage("cOAICodeList");
        $scope.GetCOAICodeListData = function (pageno) {
            baseService.paginationBase("Accounts/GLItem/GetAllGLBudgetActivityPostingAutomaticOnly", pageno, $scope.glListParameters)
                .then(function (result) {
                    $scope.cOAICodeList = result.Rows;
                    $scope.glListParameters.total_count = result.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, "failure", "GLPopUp");
                }).finally(function () {
                });
        };
        angular.element(document.querySelector("#GLPopUp")).modal("show");
        $scope.GetCOAICodeListData();
        $scope.issueSlipDetailIndex = index;
    };

    $scope.closeCOAICodeListPopUp = function () {
        angular.element(document.querySelector("#GLPopUp")).modal("hide");
    };

    $scope.closeCOAICodeListPopUpSelected = function (x) {
        if ($scope.rowSelected !== null) {
            angular.element(document.querySelector("#GLPopUp")).modal("hide");
        } else {
            angular.element(document.querySelector("#cancelPopUp")).modal("show");
        }
    };
    $scope.setSelected = function (data) {
        //debugger;
        $scope.FilterList123[$scope.issueSlipDetailIndex].GLGeneralInfoId = data.GLGeneralInfoId;
        $scope.FilterList123[$scope.issueSlipDetailIndex].BudgetMasterId = data.BudgetMasterId;
        $scope.FilterList123[$scope.issueSlipDetailIndex].ExpenseActivityId = data.ActivityId;
        $scope.FilterList123[$scope.issueSlipDetailIndex].GLBudgetActivity = data.GLGeneralInfoCode + '-' + data.ActivityName;
        angular.element(document.querySelector("#GLPopUp")).modal("hide");
    };
    //********** End Expenses GL Budget Activity**************
 

    function getInventoryMaterialList(inveReveiveId) {
        $scope.masterId = inveReveiveId;
        //debugger;
        $scope.inventoryMaterialList = [];
        $http.get($scope.path + 'IssueListById?Id=' + inveReveiveId)
            .then(function (response) {
                $scope.FilterList123 = response.data;

                for (var j = 0; j < $scope.FilterList123.length; j++) {
                    if ($scope.FilterList123[j].TransactionUoMId == null && $scope.FilterList123[j].uoMList.length == 1) {
                        $scope.FilterList123[j].TransactionUoMId = $scope.FilterList123[j].uoMList[0].Value;
                    }
                }

                //$scope.GLBudgetActivity = $scope.FilterList123[0].GLBudgetActivity;
                for (var i = 0; i < $scope.FilterList123.length; i++) {
                    $scope.FilterList123[i].check = true;
                }
            });

    }
    $scope.SOListSelected = [];
    function getSalesOrderInfobyIssueSlipId(inveReveiveId) {
        $scope.masterId = inveReveiveId;
        //debugger;

        $http.get($scope.path + 'GetSalesOrderInfobyIssueSlipId?IssueSlipId=' + inveReveiveId)
            .then(function (response) {

                $scope.ProductionOrderId = response.data[0].ProductionOrderId;

                GetProductionOrderBYSalesOrder($scope.ProductionOrderId);
                $scope.model.Id = $scope.ProductionOrderId;
                GetProcessByProductionOrder();
                $scope.SOListSelected = response.data;
                for (var i = 0; i < $scope.SOListSelected.length; i++) {

                    $scope.SOListSelected[i].Active = true;

                }
            });


    }
    $scope.modelList = [];
    function GetProductionOrderBYSalesOrder(ProductionOrderId) {
        $scope.masterId = ProductionOrderId;
        //debugger;
        $scope.SOListSelected = [];
        $http.get($scope.path + 'GetProductionOrderBYSalesOrder?ProductionOrderId=' + ProductionOrderId)
            .then(function (response) {
                $scope.modelList = response.data;
            });

    }

    $scope.MaterialColorList = [];
    function GetIssueWiseSKU(IssueId) {
        //$scope.masterId = ProductionOrderId;
        //debugger;
        $scope.SOListSelected = [];
        $http.get($scope.path + 'GetIssueWiseSKU?IssueId=' + IssueId)
            .then(function (response) {
                $scope.MaterialColorList = response.data;
                for (var i = 0; i < $scope.MaterialColorList.length; i++) {
                    $scope.MaterialColorList[i].Active = true;
                }
            });

    }

    $scope.recorddoubleclickIssueSlip = function ($event) {//sk
        //debugger;
        var x = $event;
        var Id = x.data.Id;
        $scope.productNew = x.data;
        $scope.Action1 = 'Update';
        //$scope.productNew.CheckedBy = x.data.CheckedBy;
        $scope.productNew.CheckedBy = x.data.CheckedById;

        $scope.IssueSlipType = 'InventorySlip';
        $scope.Id = $scope.productNew.Id;
        if (!baseService.isUndefinedOrNull(x.data.SalesOrderId)) {

            $scope.productNew.OrderSpecific = 'Yes';
            getSalesOrderInfobyIssueSlipId(Id);

            GetIssueWiseSKU(Id);
            /*GetProductionOrderBYSalesOrder($scope.ProductionOrderId);*/
            //$scope.model.Id = $scope.ProductionOrderId;
            //GetProcessByProductionOrder();


        }
        else {
            $scope.productNew.OrderSpecific = 'No';
        }
        $scope.productNew.ProcessId = x.data.ProcessId;
        getInventoryMaterialList($scope.productNew.Id);
        if (baseService.isUndefinedOrNull(x.data.CheckedBy) && !baseService.isUndefinedOrNull(x.data.AuthorizedBy)) {
            $scope.CheckedByStatusForNoti = false;
            $scope.ApprovedByStatusForNoti = true;
            $scope.productNew.CheckedBy = x.data.ApprovedById;
        }
        else if (!baseService.isUndefinedOrNull(x.data.CheckedBy) && !baseService.isUndefinedOrNull(x.data.AuthorizedBy)) {
            $scope.CheckedByStatusForNoti = true;
            $scope.ApprovedByStatusForNoti = true;
            $scope.CheckedBy = x.data.CheckedById;
        }

        $scope.GetCheckedByAndApprovedBy1();


        if (baseService.isUndefinedOrNull(x.data.CheckedById) && !baseService.isUndefinedOrNull(x.data.ApprovedById)) {

            $scope.CheckedBy = x.data.ApprovedById;
            $scope.labelCheckAndApproved = 'To be approved by';
        }
        else if (!baseService.isUndefinedOrNull(x.data.CheckedById) && baseService.isUndefinedOrNull(x.data.ApprovedById)) {

            $scope.CheckedBy = x.data.CheckedById;
            $scope.labelCheckAndApproved = 'To be checked by';
        }
        $scope.Action1 = 'Update';
        if (!$rootScope.isCollapsed) $rootScope.toggle();
    };
    $scope.recorddoubleclickAssetIssueSlip = function ($event) {
        //debugger;
        var x = $event;
        var Id = x.data.Id;
        $scope.productNew = x.data;
        $scope.CheckedBy = x.data.CheckedBy;
        $scope.IssueSlipType = 'AssetSlip';
        $scope.Id = $scope.productNew.Id;
        getInventoryMaterialList($scope.productNew.Id);
        $scope.Action1 = 'Update';
        if (!$rootScope.isCollapsed) $rootScope.toggle();
    };
    //**********Issue**************

    $scope.GetFilterForIssue = function () {
        //debugger;
        $.ajax({
            type: "GET",
            contentType: "application/json; charset=utf-8",
            url: 'Products/GoodsReceiveNote/IssueFilter',
            data: {},
            async: false,
            dataType: "json",
            success: function (data) {
                //$scope.FilterList = data;
                $("#Grid3").ejGrid({

                    dataSource: data, // data must be array of json
                    allowPaging: true,
                    //allowSorting: true,
                    allowFiltering: true,
                    isResponsive: true,
                    enableResponsiveRow: true,
                    allowTextWrap: true,
                    textWrapSettings: { wrapMode: "header" },
                    cssClass: "filtered",
                    filterSettings: {
                        filterType: "excel"
                    },
                    // pageSize: 1,
                    allowScrolling: true,
                    scrollSettings: { width: "auto", height: "2" },

                    columns: [
                        { headerText: "Entity Name", field: "EntityName", width: 60 },
                        { headerText: "ActivityName", field: "ActivityName", width: 80 },
                        { headerText: "Material Type", field: "MaterialType", width: 60 },
                        { headerText: "Group Name", field: "MaterialMasterGroupName", width: 90 },
                        //{ headerText: "Group Name2", field: "MaterialMasterGroupName", width: 90 },
                        { headerText: "MaterialMasterName", field: "MaterialMasterName", width: 90 },
                        { headerText: "Article", field: "StandardName", width: 80 },
                        { headerText: "RequisitionBy", field: "AddedBy", width: 85 },
                        //{ headerText: "RequisitionNo", field: "RequisitionNo", width: 95 },
                        { headerText: "RequisitionDetailId", field: "RequisitionDetailId", width: 95 },

                        { headerText: "Department Name", field: "DepartmentName", width: 80 }


                    ]
                });

                $("#Grid3").children('.e-pager.e-js.e-pager').hide();
                $("#Grid3").children('.e-gridcontent.e-droppable.e-js').hide();
                $("#Grid3").children('.e-gridcontent').hide();
                //$("#Grid2").children('.e-grid .e-headercell {background - color: chocolate;}').add();

                $("#Grid3").children('.e-grid.e-headercell').css('background-color', 'red'); //{background - color: chocolate;}').add();
            }

        });
    }
    $scope.GetFilterForIssue();
    //**********Issue**************

    //**********IssueSlipChecked Approved**************


    $scope.tabIU = 1;
    $scope.IUCsetTabIndex = function (newTab) {
        //debugger;
        $scope.tabIU = newTab;
        $scope.getaldataIssueSlipUnChecked();
    };
    $scope.isIUCSetIndex = function (tabNum) {
        return $scope.tabIU === tabNum;
    };

    $scope.ICsetTabIndex = function (newTab) {
        //debugger;
        $scope.tabIU = newTab;
        $scope.getaldataIssueSlipChecked();
    };
    $scope.isICSetIndex = function (tabNum) {
        return $scope.tabIU === tabNum;
    };

    $scope.GriddataISUnCheckedList = [];
    $scope.getaldataIssueSlipUnChecked = function () {
        //debugger;
        $http({
            method: "GET",
            dataType: 'JSON',
            //url: $scope.getSearchListUrl,
            url: 'Products/GoodsReceiveNote/IssueSlipUnChecked',
        }).then(function successCallback(response) {
            $scope.GriddataISUnCheckedList = response.data;

            //entrydata = copy(searchdata);
        });
    };
    $scope.getaldataIssueSlipUnChecked();


    $scope.GriddataISCkedList = [];
    $scope.getaldataIssueSlipChecked = function () {
        //debugger;
        $http({
            method: "GET",
            dataType: 'JSON',
            //url: $scope.getSearchListUrl,
            url: 'Products/GoodsReceiveNote/IssueSlipChecked',
        }).then(function successCallback(response) {
            $scope.GriddataISCkedList = response.data;

            //entrydata = copy(searchdata);
        });
    };
    $scope.getaldataIssueSlipChecked();



    $scope.IssueSlipChecked = function () {
        var str = $('#combo-default1').val();
        var Status = $('#combo-default').val();
        var Id = str.substring(0, str.indexOf('-'));
        if (str === "") {
            ShowResult("Please Select To be Approved By", 'failure');
            return false;
        }

        else if (Status === null || Status === "") {
            ShowResult("Please Select Checked By Status", 'failure');
            return false;
        }
        else if (Status === "ForChecked" || Status === "Select") {
            ShowResult("Please Select Checked By Status", 'failure');
            return false;
        }

        //debugger;
        $http({
            method: 'POST',
            url: 'Products/GoodsReceiveNote/IssueSlipToChecked',
            data: {
                'PoId': $scope.podata.Id,
                'PoValue': $scope.podata.TotalQty,
                'CheckedStataus': Status,
                'AuthorizedBy': Id

            },

            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.getaldataIssueSlipChecked();
                $scope.getaldataIssueSlipUnChecked();
                $scope.getalldata1();

            }
        }, function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        });
    }


    //**********ApprovingIssueSlipk**************
    $scope.checkedByList1 = [];
    $scope.GetSupervisorCboList1 = function () {
        //debugger;
        $http({
            method: 'GET',
            url: 'Products/InventoryCheckApproved/GetSupervisorCboApproved'
        }).then(function successCallback(response) {
            $scope.checkedByList1 = response.data;
        });
    }
    $scope.GetSupervisorCboList1();

    $scope.poApproved = function () {
        cboService.getEnumCbo("enum/GetPOApprovalStatusCbo", function (result) {
            $scope.POApprovalList = result;
        });
    }
    $scope.poApproved();

    $scope.LoadapprovalStatus = function () {
        cboService.getEnumCbo("enum/GetCheckedStatusCbo", function (result) {
            $scope.approvalStatusList = result;
        });

    }
    $scope.LoadapprovalStatus();
    //#endregion




    $scope.tabIU = 1;
    $scope.IUCsetTabIndex = function (newTab) {
        //debugger;
        $scope.tabIU = newTab;
        $scope.getaldataIssueSlipUnApproved();
    };
    $scope.isIUCSetIndex = function (tabNum) {
        return $scope.tabIU === tabNum;
    };

    $scope.ICsetTabIndex = function (newTab) {
        $scope.tabIU = newTab;

        $scope.getaldataIssueSlipApproved();
    };
    $scope.isICSetIndex = function (tabNum) {
        return $scope.tabIU === tabNum;
    };


    $scope.GridIssueSlipUnApprovedList = [];
    $scope.getaldataIssueSlipUnApproved = function () {
        //debugger;
        $http({
            method: "GET",
            dataType: 'JSON',
            //url: $scope.getSearchListUrl,
            url: 'Products/GoodsReceiveNote/IssueSlipUnApproved',
        }).then(function successCallback(response) {
            $scope.GridIssueSlipUnApprovedList = response.data;

            //entrydata = copy(searchdata);
        });
    };
    $scope.getaldataIssueSlipUnApproved();



    $scope.GridIssueSlipApprovedList = [];
    $scope.getaldataIssueSlipApproved = function () {
        //debugger;
        $http({
            method: "GET",
            dataType: 'JSON',
            //url: $scope.getSearchListUrl,
            url: 'Products/GoodsReceiveNote/IssueSlipApproved',
        }).then(function successCallback(response) {
            $scope.GridIssueSlipApprovedList = response.data;

            //entrydata = copy(searchdata);
        });
    };
    $scope.getaldataIssueSlipApproved();




    $scope.IssueSlipApproved = function () {

        if ($scope.podata.CheckedByStatus === null || $scope.podata.CheckedByStatus === "") {
            ShowResult("Please Select Checked By Status", 'failure');
            return false;
        }
        else if ($scope.podata.CheckedByStatus === "Checked" || $scope.podata.CheckedByStatus === "For Checked" || $scope.podata.CheckedByStatus === "Select") {
            ShowResult("Please Select Approved By Status", 'failure');
            return false;
        }


        //debugger;
        $http({
            method: 'POST',
            url: 'Products/GoodsReceiveNote/IssueSlipToApproved',
            data: {
                'PoId': $scope.podata.Id,
                'PoValue': $scope.podata.TotalQty,
                'CheckedStataus': $scope.podata.CheckedByStatus

            },

            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.getaldataIssueSlipApproved();
                $scope.getaldataIssueSlipUnApproved();
                $scope.getalldata1();

            }
        }, function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        });
    }



    //#region Issue Slip Checked Update
    $scope.onClickPOA = function (args) {
        //debugger;
        var gridObj = $("#GridISUnchecked").data("ejGrid");
        //getting corresponding record 
        $scope.podata = gridObj.getSelectedRecords()[0];

        //alert('Approve=' + data.Id);
        $scope.approvalAlert();
    };
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

    $scope.onClickPOAUTH = function (args) {

        var gridObj = $("#GridIUNApproved").data("ejGrid");
        //getting corresponding record 
        $scope.podata = gridObj.getSelectedRecords()[0];

        //alert('Approve=' + data.Id);
        $scope.approvalAlert();
    };
    $scope.commandpoAuth = [{
        type: "details", buttonOptions: {
            text: "Save",
            width: "100",
            height: "30",
            click: $scope.onClickPOAUTH
        }
    }];
    //#endregion

    $scope.GetRequisitionIssueDetail = function (id) {
        //debugger;
        $http({
            method: 'GET',
            url: 'Products/GoodsReceiveNote/GetRequisitionIssueDetail?issueId=' + id
        }).then(function successCallback(response) {
            $scope.requisitionIssueDetailList = response.data;
        });
    }

    $scope.issuedoubleclick = function ($event) {
        //debugger;
        var x = $event;
        $scope.IssueId = x.data.Id;

        $scope.GetRequisitionIssueDetail($scope.IssueId);
        $scope.Action1 = 'Update';
        if (!$rootScope.isCollapsed) $rootScope.toggle();
    };

    $scope.Issuelist = [];
    $scope.IssueGriddata = function () {
        //debugger;
        $http({
            method: 'GET',
            url: 'Products/GoodsReceiveNote/RequisitionIssueListData'
        }).then(function successCallback(response) {
            $scope.Issuelist = response.data;
        });
    }
    $scope.IssueGriddata();

    $scope.requisitionIssueDetailList = [];
    $scope.GetIssueDetail = function () {
        //debugger;

        var obj = $("#Grid3").ejGrid("instance");
        var sd = obj.getFilteredRecords();
        if (sd.length == 0) {
            sd = obj.model.dataSource;
            //alert('1' +1);
        }
        $scope.requisitionIssueDetailList = sd;
    }

    $scope.issueDetail = function () {
        $scope.issuetemp = [];
        $scope.issuetemp = $scope.requisitionIssueDetailList;
        $scope.issueDetailList = [];
        //var getRow3 = $filter("filter")($scope.inventoryMaterialList, { "MaterialMasterId": $scope.detailModel.MaterialMasterId, "ArticleId": $scope.detailModel.ArticleId, "FirstCharacteristicsValueId": $scope.detailModel.FirstCharacteristicsValueId });
        for (var i = 0; i < $scope.issuetemp.length; i++) {
            var getRow = $filter("filter")($scope.issueDetailList, { "MaterialMasterId": $scope.issuetemp[i].MaterialMasterId, "ArticleId": $scope.issuetemp[i].ArticleId, "FirstCharacteristicsValueId": $scope.issuetemp[i].FirstCharacteristicsValueId });
            if (getRow.length == 0) {
                $scope.issuetemp[i].IssueQty = $filter('sumByKey')($filter('filter')($scope.issuetemp, { MaterialMasterId: $scope.issuetemp[i].MaterialMasterId, ArticleId: $scope.issuetemp[i].ArticleId, FirstCharacteristicsValueId: $scope.issuetemp[i].FirstCharacteristicsValueId }), 'IssueValidQty');
                $scope.issueDetailList.push($scope.issuetemp[i]);
            }
        }
    }
    $scope.IssueSave = function () {
        //debugger;
        try {
            $scope.$broadcast('show-errors-check-validity');
            if ($scope.issueForm.$valid) {
                $scope.issueDetail();
                if ($scope.Action1 === 'Save') {
                    $http({
                        method: 'POST',
                        url: 'Products/GoodsReceiveNote/RequisitionIssueInsert',
                        data: {
                            'entities': $scope.issueDetailList
                            , 'specificStockList': null
                            , 'requisitionIssueDetails': $scope.requisitionIssueDetailList
                        },
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error === true)
                            ShowResult(response.data.Message, 'failure');
                        else {
                            ShowResult(response.data.Message, 'success');
                            $scope.Griddata();
                            $scope.GriddataAssetIssueSlip();
                            getInventoryMaterialList($scope.productNew.Id);

                        }
                    }), function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    };

                }
                else if ($scope.Action1 === "Update") {
                    $http({
                        method: 'POST',
                        url: 'Products/GoodsReceiveNote/RequisitionIssueUpdate',
                        data: {
                            'issueId': $scope.IssueId
                            , 'entities': $scope.issueDetailList
                            , 'specificStockList': null
                            , 'requisitionIssueDetails': $scope.requisitionIssueDetailList
                        },
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error === true)
                            ShowResult(response.data.Message, 'failure');
                        else {
                            ShowResult(response.data.Message, 'success');
                            $scope.Griddata();
                            $scope.GriddataAssetIssueSlip();
                            getInventoryMaterialList($scope.productNew.Id);

                        }
                    }), function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    };

                }
            }

        } catch (e) {
            //ShowResult(e, 'fail', 'detailPopUp');
        }
    };


    $scope.IssueSlipListPopup = [];

    $scope.searchBySlipMaterial = "MaterialMasterName"; $scope.searchSlip = "";
    $scope.searchBySlipList = [{ value: 'MaterialMasterGroupName', name: "MaterialMasterGroupName" }, { value: 'MaterialType', name: "MaterialType" },{ value: 'MaterialMasterName', name: "Material Master" }, { value: 'StandardName', name: "Article" }
        ];

    
    $scope.GetIssueSlipFilterData = function () {
        $http({
            method: 'POST',
            url: 'Products/GoodsReceiveNote/GetIssueSlipFilterData',
            data: { column: $scope.searchBySlipMaterial, value: $scope.searchSlip },
            dataType: 'JSON',
        }).then(function successCallback(response) {
            $scope.IssueSlipListPopup = response.data;
        });
    };

    
    $scope.FilterList123 = [];
    $scope.getDataMaterialWise = function () {

        $scope.IssueSlipType = 'InventorySlip';
        $scope.GetIssueSlipFilterData();
        angular.element(document.querySelector('#ListIssueSlipPopup')).modal('show');
    }
    $scope.IssueSlipTypeHide = function () {
        angular.element(document.querySelector('#ListIssueSlipPopup')).modal('hide');
    };

    $scope.IssueStatus = 'ForChecked';
    $scope.Status = 'InventorySlip';

    $scope.tab1 = 1;
    $scope.setTabIndex = function (newTab) {
        $scope.tab1 = newTab;
        $scope.Status = 'InventorySlip';
        $scope.Griddata('ForChecked');
    };
    $scope.isSetIndex = function (tabNum) {
        return $scope.tab1 === tabNum;
    };

    $scope.setTabIssueCHR = function (newTab) {
        $scope.tab1 = newTab;
        $scope.IssueStatus = 'HoldReject';
        $scope.Griddata('HoldReject');
    };
    $scope.isSetIssueCHR = function (tabNum) {
        return $scope.tab1 === tabNum;
    };


    $scope.setTabIssueChecked = function (newTab) {
        $scope.tab1 = newTab;
        $scope.IssueStatus = 'Checked';
        $scope.Griddata('Checked');
    };
    $scope.isSetIssueChecked = function (tabNum) {
        return $scope.tab1 === tabNum;
    };


    $scope.setTabAHR = function (newTab) {
        $scope.tab1 = newTab;
        $scope.IssueStatusApproval = 'HoldReject';
        $scope.LoadIssueSlipApproveData();
    };
    $scope.isSetAHR = function (tabNum) {
        return $scope.tab1 === tabNum;
    };


    $scope.setTabIssueApprove = function (newTab) {
        $scope.tab1 = newTab;
        $scope.IssueStatusApproval = 'Approval';
        $scope.LoadIssueSlipApproveData();
    };
    $scope.isSetIssueApprove = function (tabNum) {
        return $scope.tab1 === tabNum;
    };


    $scope.removeRowModal = function (ob, index) {

        try {
            if ($scope.Action1 === 'Save') {

                $scope.FilterList123.splice(index, 1);
            }
            else {
                $http({
                    method: 'POST'
                    , url: 'Products/GoodsReceiveNote/IssueSlipDelete?issueslipDetailId=' + ob.Id
                    , dataType: 'JSON'
                }).then(function (response) {
                    if (response.data.Error === true)
                        ShowResult(response.data.Message, 'failure');
                    else
                        ShowResult(response.data.Message, 'success');
                    getInventoryMaterialList($scope.productNew.Id);
                }), function (response) {
                    ShowResult(response.data.Message, 'failure');
                };

            }
        }
        catch (e) {
            ShowResult(e, 'Error');
        }
    };

    $scope.IssueSlipDeleteFn = function (ob, index) {

        $http({
            method: 'POST'
            , url: 'Products/GoodsReceiveNote/IssueSlipDeleteAll?issueslipDetailId=' + $scope.productNew.Id
        }).then(function (response) {
            if (response.data.Error === true)
                ShowResult(response.data.Message, 'failure');
            else
                ShowResult(response.data.Message, 'success');
            $scope.Griddata();
            getInventoryMaterialList($scope.productNew.Id);
        }), function (response) {
            ShowResult(response.data.Message, 'failure');
        };

    }


    $scope.removeRow = function () {
        if (!baseService.isUndefinedOrNull($scope.delData.Id)) {
            $http({
                method: 'POST'
                , url: $scope.deleteUrl + '?issueDetailId=' + $scope.delData.Id
                , dataType: 'JSON'
            }).then(function (response) {
                if (response.data.Error === true)
                    ShowResult(response.data.Message, 'failure');
                else
                    ShowResult(response.data.Message, 'success');
            }), function (response) {
                ShowResult(response.data.Message, 'failure');
            };
        }
        for (var i = 0; i < baseService.arrayLength($scope.specificStockList); i++) {
            if ($scope.specificStockList[i].InventoryMaterialId === $scope.delData.InventoryMaterialId)
                $scope.specificStockList.splice(i, 1);
        }
        $scope.detailList.splice($scope.popUpIndex, 1);
        $scope.delData = null;
        $scope.popUpIndex = -1;
        angular.element(document.querySelector('#confirmProcessPopUp')).modal('hide');
    };

    // #endregion



    //#region Asset Issue Slip Code

    $scope.GetAssetIssueSlipFilterData = function () {
        //debugger;
        $.ajax({
            type: "GET",
            contentType: "application/json; charset=utf-8",
            url: 'Products/GoodsReceiveNote/GetAssetIssueSlipFilterData',
            data: {},
            async: false,
            dataType: "json",
            success: function (data) {
                //$scope.FilterList = data;
                $("#GridAssetFilterData").ejGrid({

                    dataSource: data, // data must be array of json
                    allowPaging: true,
                    //allowSorting: true,
                    allowFiltering: true,
                    isResponsive: true,
                    enableResponsiveRow: true,
                    allowTextWrap: true,
                    textWrapSettings: { wrapMode: "header" },
                    cssClass: "filtered",
                    filterSettings: {
                        filterType: "excel"
                    },
                    // pageSize: 1,
                    allowScrolling: true,
                    scrollSettings: { width: "auto", height: "2" },

                    columns: [
                        { headerText: "Material Type", field: "MaterialType", width: 100 },
                        { headerText: "Group Name", field: "MaterialMasterGroupName", width: 100 },
                        { headerText: "Material Name", field: "MaterialMasterName", width: 100 },
                        { headerText: "Article", field: "StandardName", width: 100 },
                        { headerText: "Sku1", field: "FirstCharacteristicsValue", width: 60 },
                        { headerText: "Sku2", field: "SecondCharacteristicsValue", width: 60 },
                        { headerText: "Sku3", field: "ThirdCharacteristicsValue", width: 60 },
                        { headerText: "Country Name", field: "CountryName", width: 60 }

                    ]
                });

                $("#GridAssetFilterData").children('.e-pager.e-js.e-pager').hide();
                $("#GridAssetFilterData").children('.e-gridcontent.e-droppable.e-js').hide();
                $("#GridAssetFilterData").children('.e-gridcontent').hide();
                //$("#Grid2").children('.e-grid .e-headercell {background - color: chocolate;}').add();

                $("#GridAssetFilterData").children('.e-grid.e-headercell').css('background-color', 'red'); //{background - color: chocolate;}').add();
            }

        });
    }

    $scope.GetAssetIssueSlipFilterData();


    

    $scope.IssueStatus = 'ForChecked';
    $scope.GriddataAssetIssueSlip = function () {

        //debugger;
        $scope.Status1 = 'AssetSlip';
        $scope.IssueSlipType = 'AssetSlip';
        if ($scope.IssueStatus === 'ForChecked') {
            $scope.IssueStatus = 'ForChecked';
        }
        $http({
            method: 'GET',
            url: 'Products/GoodsReceiveNote/AssetIssueListData?IssueStatus=' + $scope.IssueStatus + '&IssueSlipType=' + $scope.Status1
        }).then(function successCallback(response) {
            $scope.AssetIssueSlipList = response.data;
        });
    }
    $scope.GriddataAssetIssueSlip();

    $scope.Status = 'AssetSlip';
    $scope.IssueStatus = 'ForChecked';

    $scope.tab1 = 1;
    $scope.setTabAssetIndex = function (newTab) {
        $scope.tab1 = newTab;
        $scope.IssueStatus = 'ForChecked';
        $scope.Status = 'AssetSlip';
        $scope.GriddataAssetIssueSlip();
    };
    $scope.isSetAssetIndex = function (tabNum) {
        return $scope.tab1 === tabNum;
    };

    $scope.setTabAssetIssueCHR = function (newTab) {
        //alert(2);
        $scope.tab1 = newTab;
        $scope.IssueStatus = 'HoldReject';

        $scope.GriddataAssetIssueSlip();
    };
    $scope.isSetAssetIssueCHR = function (tabNum) {
        return $scope.tab1 === tabNum;
    };


    //   $scope.tab1 = 1;
    $scope.setTabAssetIssueChecked = function (newTab) {
        $scope.tab1 = newTab;
        $scope.IssueStatus = 'Checked';

        $scope.GriddataAssetIssueSlip();
    };
    $scope.isSetAssetIssueChecked = function (tabNum) {
        return $scope.tab1 === tabNum;
    };


    $scope.setTabAssetAHR = function (newTab) {
        // alert(4);
        $scope.tab1 = newTab;
        $scope.IssueStatusApproval = 'HoldReject';
        $scope.Status = $scope.AssetIssueTypeStatus;
        $scope.LoadIssueSlipApproveData();
    };
    $scope.isSetAssetAHR = function (tabNum) {
        return $scope.tab1 === tabNum;
    };


    $scope.setAssetTabIssueApprove = function (newTab) {
        $scope.tab1 = newTab;
        $scope.IssueStatusApproval = 'Approval';
        $scope.Status = $scope.AssetIssueTypeStatus;
        $scope.LoadIssueSlipApproveData();
    };
    $scope.isSetAssetIssueApprove = function (tabNum) {
        return $scope.tab1 === tabNum;
    };

    // #endregion



    $scope.CheckAll1 = function (event) {
        var _isselected = event.target.checked;

        for (var i = 0; i < $scope.FilterList123.length; i++) {

            $scope.FilterList123[i].check = _isselected;
        }
    };


    $scope.Change = function (event, index, x) {

    }
    $scope.IssueSlipType = '';
    $scope.uiType = function () {
        $scope.url = $location.absUrl().split('!/')[1];
        if ($scope.url === 'Material-Wise-issue-slip') {
            $scope.IssueSlipType = 'InventorySlip';
        }
        else if ($scope.url === 'gate-pass-checked') {
            $scope.IssueSlipType = 'AssetSlip';
        }
    }
    $scope.uiType();

    $scope.ClearFilter = function () {
        $scope.MaterialColorList = [];
        $scope.modelList = [];
        $scope.SOListSelected = [];
        $scope.FilterList123 = [];
        $scope.productNew.ProcessId = null;
        $scope.productNew.CheckedBy = null;
    }

    $scope.showMaterialWiseStockModal = function (x, index) {

        $scope.GetSOWiseMaterialStock(x, index);
        angular.element(document.querySelector('#POPopUp')).modal('show');

    };

    $scope.showMaterialWiseStockModalClose = function () {
        //debugger;
        angular.element(document.querySelector('#POPopUp')).modal('hide');

    };
    $scope.ConvertedDataRowList = [];
    $scope.GetListForMasterOrderTemp = [];
    $scope.ConvertedDataRow = function (data) {

        debugger;
        $http({
            method: 'POST',
            url: 'Products/InventoryIssue/ConverttedBOQUOMData',
            data: {
                'data': data
            },
            dataType: 'JSON'
        }).then(function (response) {
            $scope.ConvertedDataRowList = response.data;
            for (var i = 0; i < $scope.FilterList123.length; i++) {
                if ($scope.FilterList123[i].BOQId === $scope.ConvertedDataRowList.data.BOQId) {
                    $scope.FilterList123[i].RequisitionQty = $scope.ConvertedDataRowList.data.RequisitionQty;
                    $scope.FilterList123[i].IssuedQty = $scope.ConvertedDataRowList.data.IssuedQty;
                    $scope.FilterList123[i].TransactionUoMName = $scope.ConvertedDataRowList.data.TransactionUoMName;
                    $scope.FilterList123[i].TransactionUoMId = $scope.ConvertedDataRowList.data.TransactionUoMId;



                }

                var getRow1 = $filter("filter")($scope.FilterList123, { "MaterialMasterId": $scope.FilterList123[i].MaterialMasterId, "ArticleId": $scope.FilterList123[i].ArticleId, "BOQDFirstCharacteristicsValueId": $scope.FilterList123[i].BOQDFirstCharacteristicsValueId, "BOQDSecondCharacteristicsValueId": $scope.FilterList123[i].BOQDSecondCharacteristicsValueId, "BOQDThirdCharacteristicsValueId": $scope.FilterList123[i].BOQDThirdCharacteristicsValueId, 'check': true });
                if (getRow1.length > 1) {
                    for (var i12 = 0; i12 < getRow1.length; i12++) {
                        if (getRow1[i12].TransactionUoMId != $scope.ConvertedDataRowList.data.TransactionUoMId) {
                            ShowResult('For same material UoM can not difference', 'failure');
                            return false;
                        }
                    }

                }

            }

        });

    };

    $scope.refreshIssueSlip = function (args) {
        $("#headchk10").ejCheckBox({ "change": CheckBoxSelectInventoryIssueWise });
    };

    function CheckBoxSelectInventoryIssueWise(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }
        var filtered = $("#GridPopup").data("ejGrid").getFilteredRecords();
        if (baseService.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.IssueSlipListPopup.length; i++) {
                $scope.IssueSlipListPopup[i].check = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].CheckBoxSelect = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridPopup").data("ejGrid");
        gridObj.refreshContent();
    };

    $scope.AddRow = function () {
        if ($scope.Action1 === 'Save') {
            var Id = "''";
            $scope.FilterList123 = [];
            for (var i = 0; i < $scope.IssueSlipListPopup.length; i++) {
                if ($scope.IssueSlipListPopup[i].check == true) {
                    $scope.FilterList123.push($scope.IssueSlipListPopup[i]);
                    Id += ",'" + $scope.IssueSlipListPopup[i].MaterialMasterId + "'";
                }
            }
        }
        else {
            var Id = "''";
            for (var i = 0; i < $scope.IssueSlipListPopup.length; i++) {
                if ($scope.IssueSlipListPopup[i].check == true) {
                    $scope.FilterList123.push($scope.IssueSlipListPopup[i]);
                    Id += ",'" + $scope.IssueSlipListPopup[i].MaterialMasterId + "'";
                }
            }
        }


        angular.element(document.querySelector('#ListIssueSlipPopup')).modal('hide');
        //$scope.getUoM(Id);
    }

    $scope.gridUoMList = [];
    $scope.uom = function () {
        cboService.getUoMCbo(function (response) {
            $scope.gridUoMList = response;
        });
    }
    $scope.uom();


    //$scope.getUoM = function (Id) {
    //    $http({
    //        method: 'GET',
    //        url: $scope.path + "GetUoMList?MaterialMasterId=" + Id,
    //    }).then(function successCallback(response) {
    //        $scope.FilterList123.uoMList = response.data.UOMList;

    //    });
    //}


    $scope.getMachineInventoryIssueStock = function (data) {
        $scope.selectedMaterialRow = data;
        $scope.tempMaterialMasterId = data.MaterialMasterId;
        angular.element(document.querySelector('#ShowMachineInventoryIssue')).modal('show');
    }
    $scope.GetPopUpMachineInventoryIssueClosed = function () {
            $scope.Qty = 0;
        for (var i = 0; i < $scope.machineQtyList.length; i++) {
            $scope.Qty += $scope.machineQtyList[i].Qty;
        }
        if ($scope.selectedMaterialRow.RequestedQty != $scope.Qty) {
            ShowResult('Quantity had must same value as Required Qty!', 'failure');
            return false;
        }


        angular.element(document.querySelector('#ShowMachineInventoryIssue')).modal('hide');
    }

    $scope.showMachinePopUp = function () {
        angular.element(document.querySelector('#machinePopUp')).modal('show');
    };


    $scope.closeMachinePopUp = function () {
        angular.element(document.querySelector('#machinePopUp')).modal('hide');
    };

    $scope.clearMachine = function () {
        $scope.MachineName = null;
        $scope.MachineId = null;
    };
    $scope.machinepopUpList = [];
    $scope.machinevalueData = '';
    $scope.machinepopUpParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'Code',
        searchBy: "UserName",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.machinepopUpDataList = [];
    $scope.showMachinePopUp = function () {
        $scope.machinepopUpDataList = [];
        $scope.popUpUrl = 'materials/materialmastermachineprocess/getmaterialmasterlist';
        baseService.setCurrentPage('machinepopUpDataList');
        $scope.getPopUpData = function (pageno) {
            baseService.paginationBase($scope.popUpUrl, pageno, $scope.machinepopUpParameters)
                .then(function (result) {
                    $scope.machinepopUpDataList = result.Rows;
                    $scope.machinepopUpParameters.total_count = result.Total;
                    if (baseService.arrayLength($scope.machinepopUpList) === 0) {
                        baseService.getDDLSearchColumn(result.Rows, $scope.machinepopUpList);
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure', 'MachinePopUp');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#MachinePopUp')).modal('show');
        $scope.getPopUpData();
    };
    $scope.modelNew = {};

    $scope.selectMichineDoubleClick = function (data) {

        if ($scope.machineQtyList.length>0) {
            for (var i = 0; i < $scope.machineQtyList.length; i++) {
                if ($scope.machineQtyList[i].MachineMasterId == data.MaterialMasterId) {
                    ShowResult('This machine is already exists!', 'failure');
                    return false;
                }
            }
        }
        $scope.modelNew.MachineName = data.UserName;
        $scope.modelNew.MachineMasterId = data.MaterialMasterId;
        $scope.closeMichinePopUp();
    };

    $scope.closeMichinePopUp = function () {
        angular.element(document.querySelector('#MachinePopUp')).modal('hide');
    };
    $scope.machineQtyList = [];
    $scope.addMachine = function () {
        $scope.modelNew.MaterialMasterId = $scope.selectedMaterialRow.MaterialMasterId;
        $scope.modelNew.ArticleId = $scope.selectedMaterialRow.ArticleId;
        $scope.modelNew.Id = null;
        $scope.modelNew.FirstCharacteristicsValueId = $scope.selectedMaterialRow.FirstCharacteristicsValueId;
        $scope.modelNew.CostCenterId = $scope.selectedMaterialRow.CostCenterId;
        $scope.machineQtyList.push($scope.modelNew);
        $scope.modelNew = {};
    }
    $scope.removeMachineRow = function (Id, index) {
        if (baseService.isUndefinedOrNull(Id)) {
            $scope.machineQtyList.splice(index, 1);

        }
        //else {
        //	$scope.DeleteAdditinalTax(Id);
        //	$scope.GetAdvanceTaxInfo($scope.productNew.Id);
        //}
    };

    $scope.workCenterList = [];

    $scope.popUpWorkCenterList = function () {
        try {            
            $http({
                method: 'GET',
                url: $scope.path + 'GetWorkCenterList'
            }).then(function successCallback(res) {
                $scope.workCenterList = res.data;
            });
            var eDialog = $("#workCenterPopUp").data("ejDialog");
            eDialog.open();
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.SetworkCenter = function (data) {
        $scope.WorkCenterMaster = data.data.UserName;
        $scope.WorkCenterMasterId = data.data.WorkCenterMasterId;
        $scope.CloseWorkCenter();
    }

    $scope.CloseWorkCenter = function () {
        var eDialog = $("#workCenterPopUp").data("ejDialog");
        eDialog.close();
    }


}

