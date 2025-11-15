'use strict';
IssueReturnController.$inject = ['$window', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$controller'];
function IssueReturnController($window, cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $controller) {
    $rootScope.title = "Issue Return";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.products = [];
    $scope.path = 'Products/InventoryIssue/';
    $scope.getListUrl = $scope.path + 'GetDataByInventoryIssue';
    $scope.saveUrl = $scope.path + 'CreateIssueReturn';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    $scope.currentDate = new Date(Date.now());
    $scope.storageList = [];    $scope.costCenterList = [];

    $controller('baseMaterialAndArticleController', { $scope: $scope, $http: $http });
    $controller("employeeBaseController", { $scope: $scope, $http: $http });
    $scope.product = {
        Id: null
        , ComapnyGroupId: null
        , CompanyId: null
        , PlantId: null
        , PlantName: null
        , EntityId: null
        , EntityName: null
        , MaterialStorageId: null
        , IssueDate: $filter("dateFiltering")(Date.now())
        , Remarks: null
        , EmployeeId: null
        , EmployeeName: null
        , IssueType: 'Revenue'
        , IssueRequestMasterId: null
        , SlipAssetIssueTypeStatus: 'Asset'
        , OrderRefNo: null
        , FromDate1: $filter("dateFiltering")(Date.now())
        , ToDate: $filter("dateFiltering")(Date.now())
    };
    $scope.IssueType = 'Revenue';
    $scope.productNew = Object.assign({}, $scope.product);
    //#region Material Issue icon Detail        $scope.POPopUp = function () {
        if (baseService.isUndefinedOrNull($scope.productNew.CostCenterId)) {
            ShowResult('Select Cost center', 'failure');
            return false;
        }
        $scope.GetApprovedIssueSlipListGrid();
        angular.element(document.querySelector('#POPopUp1')).modal('show');
    };
    $scope.POPopUpClose = function () {
        angular.element(document.querySelector('#POPopUp1')).modal('hide');
    };

    $scope.GetArticleList = [];
    $scope.GetApprovedIssueSlipListGrid = function () {
        //debugger;
        try {            $http({                method: 'GET',                url: 'Products/InventoryIssue/IssueSlipMaterialAndArticleList?fromDate=' + $scope.productNew.FromDate1 + '&toDate=' + $scope.productNew.ToDate + '&CostCenterId=' + $scope.productNew.CostCenterId + '&MaterialStorageId=' + $scope.productNew.MaterialStorageId + '&IssueType=' + $scope.productNew.IssueType,                dataType: 'JSON'            }).then(function successCallback(response) {                if (response.data.Error == true) {                    ShowResult(response.data.Message, 'failure');                }                else {                    $scope.GetArticleList = response.data;                }            }, function errorCallback(response) {                ShowResult(response.status.Message, 'failure');            });        } catch (e) {            ShowResult(e, 'failure');        }

    };    $scope.lst = [];    $scope.POListDetails = function () {        //debugger;        $http({            method: 'GET',            //url: 'Products/Requisition/GetAllReqdataDetails?ReqDetailId=' + $scope.filteredData            url: 'Products/InventoryIssue/MaterialIssueDetailsData'        }).then(function successCallback(response) {            $scope.lst = response.data;            //$scope.detailgrid($scope.lst);            window.lst = response.data;        });    }    $scope.POListDetails();    $scope.data1 = $scope.lst;    $scope.detailTemp = "#tabGridContents";    //$scope.detailgrid = "detailGridData(e)";    $scope.detailgrid = function detailGridData(e) {        //debugger;        var filteredData = e.data["Id"];        var data = ej.DataManager(window.lst).executeLocal(ej.Query().where("IssueNo", "equal", parseInt(filteredData), true).take(200));        e.detailsElement.find("#detailGrid").ejGrid({            dataSource: data,            columns: ["CostCenter", "StorageLocation", "Materials", "Article", "SKU1", "SKU2", "SKU3", "Qty", "UOM"]//"UOM", "TransactionRate", "CurrencyName", "TotalMaterialTranAmount"        });        e.detailsElement.find(".tabcontrol").ejTab();    }    $scope.detailListForArticle = [];    $scope.recorddoubleclickloadArticleDetails = function ($event) {
        //debugger;
        var x = $event;
        //var Id = x.data.Id;
        var MaterialMasterId1 = "''";
        var ArticleId1 = "''";
        var FirstCharacteristicsValueId1 = "''";
        var SecondCharacteristicsValueId1 = "''";
        var ThirdCharacteristicsValueId1 = "''";
        for (var i = 0; i < $scope.GetArticleList.length; i++) {
            if ($scope.GetArticleList[i].Active === true) {
                MaterialMasterId1 += ",'" + $scope.GetArticleList[i].MaterialMasterId + "'";
                ArticleId1 += ",'" + $scope.GetArticleList[i].ArticleId + "'";
                FirstCharacteristicsValueId1 += ",'" + $scope.GetArticleList[i].FirstCharacteristicsValueId + "'";
                SecondCharacteristicsValueId1 += ",'" + $scope.GetArticleList[i].SecondCharacteristicsValueId + "'";
                ThirdCharacteristicsValueId1 += ",'" + $scope.GetArticleList[i].ThirdCharacteristicsValueId + "'";

            }
        }
        $http({
            method: 'GET',
            url: 'Products/InventoryIssue/IssueSlipMaterialAndArticleListForIssued?MaterialMasterId=' + MaterialMasterId1 + '&ArticleId=' + ArticleId1 + '&FirstCharacteristicsValueId=' + FirstCharacteristicsValueId1 + '&SecondCharacteristicsValueId=' + SecondCharacteristicsValueId1 + '&ThirdCharacteristicsValueId=' + ThirdCharacteristicsValueId1 + '&MaterialStorageId=' + $scope.productNew.MaterialStorageId + '&CostCenterId='
                + $scope.productNew.CostCenterId + '&fromDate=' + $scope.productNew.FromDate1 + '&toDate=' + $scope.productNew.ToDate + '&issueType=' + $scope.productNew.IssueType
        }).then(function (response) {
            $scope.detailListForArticle = response.data;
        });
        // $scope.loadArticleData(x.data);

        $scope.POPopUpClose();


    }    $scope.CloseArticlePopUp = function () {
        $scope.POPopUpClose();
    }        $scope.staus = true;
    $scope.enableid = true;
    $scope.Change = function (event, index, x) {
        //debugger;
        if (baseService.isUndefinedOrNull(x.TransactionQty)) {
            ShowResult('Enter the current qty', 'failure');
        }
        else {
            if (event.currentTarget.checked) {
                $scope.index = index;
                //$scope.staus = false;
                x.enableid = false;

                if (x.POQty === (x.GRNRcvQty + x.TransactionQty)) {
                    x.POClosStatus = true;
                }
                else if (x.POQty > (x.GRNRcvQty + x.TransactionQty)) {
                    $scope.PODetailId = x.PODetailId;
                    $scope.message = 'Are you want to close this PO line item?';
                    angular.element(document.querySelector('#ConfirmationForReqClosePopUp')).modal('show');
                }
            }
            else {
                x.enableid = true;
                //$scope.index = index;
                x.POClosStatus = false;
                x.TransactionQty = "";
                x.Balance = x.POQty - x.GRNRcvQty;//parseFloat(x.POQty - x.GRNRcvQty).toFixed(2);
            }
        }

    }    $scope.detailListForArticleNew = [];    $scope.SaveSlipIssue = function () {
        $scope.detailListForArticleNew = [];
        //debugger;
        var gridObj = $("#GridTest1").data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];

        for (var i = 0; i < $scope.detailListForArticle.length; i++) {

            if ($scope.detailListForArticle[i].Active === true) {
                $scope.detailListForArticleNew.push($scope.detailListForArticle[i])
            }
        }
        for (var j = 0; j < $scope.detailListForArticle.length; j++) {
            // if ($scope.detailListForArticle[j].Active === true) {
            if ($scope.detailListForArticle[j].InventoryIssueHistoryId === data.InventoryIssueHistoryId) {
                if ((parseFloat($scope.detailListForArticle[j].TransactionQty) + parseFloat($scope.detailListForArticle[j].IssueReturnQty)) <= $scope.detailListForArticle[j].IssuedQty) {

                }
                else {
                    ShowResult("Return qty can not gaterthen Issued Qty");
                    $scope.detailListForArticle[j].TransactionQty = "";
                    return false;
                }

            }

        }

        $scope.productNew.IssueRequestMasterId = $scope.issueId;
        if ($scope.Action === "Save") {
            $http({
                method: 'POST'
                , url: $scope.saveUrl
                , data: {
                    entities: null
                    , specificStockList: $scope.detailListForArticleNew
                    , inventoryIssue: $scope.productNew
                    , IssueTypeStatus: null

                }
                , dataType: 'JSON'
            }).then(function (response) {
                if (response.data.Error === true)
                    ShowResult(response.data.Message, 'failure');
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.Clear();
                    $scope.getdataInventoryIssue();
                    $scope.POListDetails();
                    //$scope.productNew.Id = response.data.inventoryIssue.Id;
                }
            }), function (response) {
                ShowResult(response.data.Message, 'failure');
            };
        }
        else if ($scope.Action === "Update") {
            $scope.productNew.Id = $scope.productNew.Id;
            $http({
                method: 'POST'
                , url: $scope.saveUrl
                , data: {
                    entities: null
                    , specificStockList: $scope.detailListForArticleNew
                    , inventoryIssue: $scope.productNew
                    , IssueTypeStatus: null

                }
                , dataType: 'JSON'
            }).then(function (response) {
                if (response.data.Error === true)
                    ShowResult(response.data.Message, 'failure');
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.Clear();
                    $scope.getdataInventoryIssue();
                    $scope.POListDetails();
                    $scope.productNew.Id = response.data.inventoryIssue.Id;
                }
            }), function (response) {
                ShowResult(response.data.Message, 'failure');
            };
        }
    };

    $scope.Clear = function () {
        $scope.detailListForArticle = [];
        ClearFields();
        return true;
    };

    function ClearFields() {
        $scope.Action = "Save";
        $scope.product = {};
        $scope.productNew = { FixedAssetOrInventory: 'Inventory', PODepended: false, AlongwithInvoice: false, IssueType: 'Revenue' };
        $scope.detailModel = {};
        $scope.clearCharNames();
        $scope.detailList = [];
        $scope.specificStockList = [];
        $scope.IssueType = 'Revenue';
    }    $scope.GridInventoryIssuedata = [];
    $scope.getdataInventoryIssue = function () {
        //debugger;
        $http({
            method: "GET",
            dataType: 'JSON',
            //url: $scope.getSearchListUrl,
            url: 'Products/InventoryIssue/GetDataByInventoryReturnIssue',
        }).then(function successCallback(response) {
            $scope.GridInventoryIssuedata = response.data;
            //entrydata = copy(searchdata);
        });

    };
    $scope.getdataInventoryIssue(); 
    $scope.recorddoubleclickIssueReturnForUpdate = function ($event) {
        //debugger;
        var x = $event;
        $http({
            method: 'GET',
            url: 'Products/InventoryIssue/IssueReturnForUpdate?Id=' + x.data.Id
        }).then(function (response) {
            $scope.detailListForArticle = response.data;
            $scope.productNew.MaterialStorageId = response.data[0].StorageLocationId;
            $scope.productNew.CostCenterId = response.data[0].CostCenterId;
            $scope.productNew.Id = response.data[0].InventoryIssueReturnId;
            $scope.productNew.Remarks = x.data.Remarks;
        });
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) $rootScope.toggle();
    }    $scope.AllTabPrint = function (z) {
        //debugger;
        var x = "#" + z;
        var gridObj = $(x).data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        location.href = "Products/InventoryIssue/InventoryIssueReturnReport?grnId=" + data.Id;

    };    $http({
    method: 'GET',
    url: 'Materials/MaterialStorage/getcbo'
    }).then(function (response) {
        $scope.storageList = response.data;
    });     $scope.CostCenterLoad = function () {
        cboService.getCostCenterCbo(function (result) {
            $scope.costCenterList = result;
        });
    }
    $scope.CostCenterLoad();    //#endregion

    $scope.IssueBoqList = [];
    $scope.GetIssueBoqList = function (grnRowId) {
        $http({
            method: "POST",
            dataType: 'JSON',
            url: 'Products/InventoryIssue/GetIssueBOQListForIssueReturn?InventoryreceiveDetailId=' + grnRowId,
        }).then(function successCallback(response) {
            $scope.IssueBoqList = response.data;
        });
    };

    $scope.GetIssueBoqPopUp = function (grnRowId, index) {
        $scope.TempIndex = index;
        $scope.TempGrnRowId = grnRowId;
        $scope.GetIssueBoqList(grnRowId);
        angular.element(document.querySelector('#IssueBoqPopUp')).modal('show');

    };
    $scope.IssueBoqPOPopUpClose = function () {
        angular.element(document.querySelector('#IssueBoqPopUp')).modal('hide');
    };
    $scope.SelectedGRNBoqList = [];
    $scope.addToBOQList = function () {
        for (var i = 0; i < $scope.IssueBoqList.length; i++) {
            if ($scope.IssueBoqList[i].ReturnQty > 0) {
                var getRow = $filter("filter")($scope.SelectedGRNBoqList, { "InventoryReceiveDetailId": $scope.IssueBoqList[i].InventoryReceiveDetailId, "BOQDetailId": $scope.IssueBoqList[i].BOQDetailId });
                if (!baseService.isUndefinedOrNull(getRow) && getRow.length > 0 && getRow[0].InventoryReceiveDetailId === $scope.IssueBoqList[i].InventoryReceiveDetailId && getRow[0].BOQDetailId === $scope.IssueBoqList[i].BOQDetailId) {
                    ShowResult("This BOQ Item have already added!", "failure", "IssueBoqPopUp");
                }
                else {
                    $scope.SelectedGRNBoqList.splice(0, 0, $scope.IssueBoqList[i]);
                }
            }
        }
        var tempReturnQty = parseFloat($filter("sumByKey")($filter("filter")($scope.SelectedGRNBoqList, { InventoryReceiveDetailId: $scope.TempGrnRowId }), "ReturnQty")).toFixed(2);
        for (var j = 0; j < $scope.inventoryMaterialList.length; j++) {
            if ($scope.inventoryMaterialList[j].InventoryReceiveDetailId === $scope.TempGrnRowId) {
                $scope.inventoryMaterialList[j].TransactionQty = parseFloat(tempReturnQty).toFixed(2);
                var tempGRNTaxAmount = 0;
                for (var k = 0; k < $scope.inventoryMaterialList[j].POMaterialTaxList.length; k++) {
                    if ($scope.inventoryMaterialList[j].POMaterialTaxList[k].InventoryReceiveDetailId == $scope.inventoryMaterialList[j].InventoryReceiveDetailId) {
                        var tmpTaxAmount = ($scope.inventoryMaterialList[j].POMaterialTaxList[k].TaxAmount / $scope.inventoryMaterialList[j].GRNReceived) * $scope.inventoryMaterialList[j].TransactionQty
                        $scope.inventoryMaterialList[j].POMaterialTaxList[k].TaxAmount = parseFloat(tmpTaxAmount).toFixed(2);
                        tempGRNTaxAmount += Math.round((tmpTaxAmount) * 100 + Number.EPSILON) / 100;
                    }
                    $scope.inventoryMaterialList[j].BaseTaxAmount = Math.round((tempGRNTaxAmount) * 100 + Number.EPSILON) / 100;
                }
            }
        }
        //TODO:taxamount calculation
        angular.element(document.querySelector('#IssueBoqPopUp')).modal('hide');
        $scope.TempIndex = null;
        $scope.TempGrnRowId = null;
        tempReturnQty = 0;
    }

}