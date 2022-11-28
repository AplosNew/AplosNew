'use strict';
AssetIssueSlipController.$inject = ['addressService', '$window', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter', '$controller', '$location'];
function AssetIssueSlipController(addressService, $window, cboService, commonMessage, $scope, $rootScope, baseService, $http, $filter, $controller, $location) {
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
    
    $scope.checkedByList = [];
    $scope.GetSupervisorCboList = function () {
        //debugger;
        $http({
            method: 'GET',
            url: 'Products/PurchaseOrder/GetSupervisorCbo'
        }).then(function successCallback(response) {
            $scope.checkedByList = response.data;
        });
    }
    $scope.GetSupervisorCboList();
    $scope.FilterList123 = [];
    $scope.getDataAssetIssuelWise = function () {
        $scope.IssueSlipType = 'AssetSlip';
        //debugger;
        var obj1 = $("#GridAssetFilterData").ejGrid("instance");
        var sd1 = obj1.getFilteredRecords();
        if (sd1.length == 0) {
            sd1 = obj1.model.dataSource;
            //alert('1' +1);
        }
        for (var i = 0; i < sd1.length; i++) {
            $scope.FilterList123.push(sd1[i]);

        }

    }

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
                if ($scope.FilterList123[i].RequestedQty >0 ) {
                   if (baseService.isUndefinedOrNull($scope.FilterList123[i].CostCenterId) && $scope.FilterList123[i].RequestedQty > 0) {
                        ShowResult('Please select cost center', 'failure');
                        return false;
                    }
                   
                   else if ($scope.FilterList123[i].RequestedQty > $scope.FilterList123[i].TotalQty) {
                        ShowResult('Required Qty can not greater than Total Qty', 'failure');
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
           
            if (baseService.isUndefinedOrNull($scope.CheckedBy)) {
                ShowResult('Select the to be checked by/approved by', 'failure');
                return false;
            }
            for (var i2 = 0; i2 < $scope.FilterList123New.length; i2++) {
                    $scope.FilterList123[i2].RequestedQtyNew = Math.round($scope.FilterList123[i2].RequestedQty * 100 + Number.EPSILON) / 100;
                var getRow1 = $filter("filter")($scope.FilterList1234, { "MaterialMasterId": $scope.FilterList123New[i2].MaterialMasterId, "ArticleId": $scope.FilterList123New[i2].ArticleId, "BOQDFirstCharacteristicsValueId": $scope.FilterList123New[i2].BOQDFirstCharacteristicsValueId, "BOQDSecondCharacteristicsValueId": $scope.FilterList123New[i2].BOQDSecondCharacteristicsValueId, "BOQDThirdCharacteristicsValueId": $scope.FilterList123New[i2].BOQDThirdCharacteristicsValueId, "TransactionUoMId": $scope.FilterList123New[i2].TransactionUoMId, "SalesOrderId": $scope.FilterList123New[i2].SalesOrderId, "RequestedQty":$scope.FilterList123New[i2].RequestedQty>0 });

                    if (getRow1.length === 0) {
                        $scope.FilterList1234.push($scope.FilterList123New[i2])
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

            // $scope.FilterList1.IssueSlipType = $scope.IssueSlipType;
            // $scope.processgroupList($scope.GetListForMasterOrdernew, $scope.groupList);
            $scope.$broadcast('show-errors-check-validity');
            if ($scope.productNewForm.$valid) {
                    if ($scope.Action1 === 'Save') {
                        $http({
                            method: 'POST',
                            url: 'Products/GoodsReceiveNote/IssueSlipCreate',
                            data: {
                                entity: JSON.stringify($scope.FilterList123New)
                                , entityGroupData: JSON.stringify($scope.FilterList1234)
                                , CheckedBy: $scope.CheckedBy
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
                                $scope.Id = response.data.Issentity.Id;
                                $scope.Status = 'InventorySlip';
                                $scope.Griddata();
                                $scope.IssueSlipDetail();
                                $scope.Clear();

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




    $scope.CostCenterLoad = function () {
        cboService.getCostCenterCbo(function (result) {
            $scope.costCenterList = result;
        });
    }
    $scope.CostCenterLoad();

    $scope.AssetIssueSlipList = [];

    $scope.IssueStatus = 'ForChecked';
    $scope.GriddataAssetIssueSlip = function () {

        //debugger;
        $scope.Status1 = 'AssetSlip';
        $scope.IssueSlipType = 'AssetSlip';
        if ($scope.IssueStatus === 'ForChecked') {
            $scope.IssueStatus = 'ForChecked';
        }

        else {

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
        $scope.getUoM(Id);
    }

    $scope.gridUoMList = [];
    $scope.uom = function () {
        cboService.getUoMCbo(function (response) {
            $scope.gridUoMList = response;
        });
    }
    $scope.uom();


    $scope.getUoM = function (Id) {
        $http({
            method: 'GET',
            url: $scope.path + "GetUoMList?MaterialMasterId=" + Id,
        }).then(function successCallback(response) {
            $scope.FilterList123.uoMList = response.data.UOMList;

        });
    }

}

