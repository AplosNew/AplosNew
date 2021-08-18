'use strict';
PackingContentController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$controller', 'cboService', '$window'];
function PackingContentController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $controller, cboService, $window) {
    $rootScope.title = "Packing Content";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.lsds = [];
    $scope.path = 'OrderManagements/PackingContent/';
    $scope.deleteUrl = $scope.path + 'delete/';

    $scope.model = {
        Id: null, ProductionOrderId: null, Qty: 0, UoMId: null, NetWeight: 0, GrossWeight: 0, WeightUoMId: null, LotNo: null, Remarks: null, AddedBy: null, AddedDate: null, AddedFromIP: null, UpdatedBy: null, UpdatedDate: null, UpdatedFromIP: null, IsPackingSKURequired: false, PackingForm: null, Entity: null
    }
    $scope.packingContenNew = Object.assign({}, $scope.model);

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.masterDataList = [];
    $scope.getmasterData = function () {
        $http.get("OrderManagements/PackingContent/GetList")
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.masterDataList = response.data;
                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
    };
    $scope.getmasterData();

    $scope.TQty = 0;

    $scope.Get = function (obj) {
        $scope.model = obj.data;
        $scope.packingContenNew = Object.assign({}, $scope.model);
        $scope.getDetailData($scope.packingContenNew.Id);
        $scope.getPackingChildData($scope.packingContenNew.Id);

        $scope.TQty = $scope.LineNo * $scope.packingContenNew.Qty;
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    //$scope.uOMList = [];
    //cboService.getUoMCbo(function (response) {
    //    $scope.uOMList = response;
    //});

    $scope.ProductionOrderList = [];
    $scope.getProductionOrderPopUp = function () {
        $scope.ProductionOrderList = [];
        $http.get('OrderManagements/PackingContent/GetProductionOrderDataList')
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.ProductionOrderList = response.data;
                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
        angular.element(document.querySelector('#POItemPopup')).modal('show');
    };

    // #region checkbox all

    $scope.refreshTemplatePO = function (args) {
        $("#headchk").ejCheckBox({ "change": CheckBoxSelectAllPOWise });
    };

    function CheckBoxSelectAllPOWise(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        var filtered = $("#GridPO").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.ProductionOrderList.length; i++) {
                $scope.ProductionOrderList[i].Flag = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].CheckBoxSelect = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridPO").data("ejGrid");
        gridObj.refreshContent();
    };

    // #endregion checkbox all

    $scope.ClosePOPopUp = function () {
        try {
            MakeData();
            angular.element(document.querySelector('#POItemPopup')).modal('hide');
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }
    $scope.SelectedProductionOrderList = [];
    function MakeData() {
        for (var i = 0; i < $scope.ProductionOrderList.length; i++) {
            if ($scope.ProductionOrderList[i].Flag == true) {
                if (checkExists($scope.SelectedProductionOrderList, $scope.ProductionOrderList[i].POId) === false) {
                    var ob = {};
                    ob.Id = null;
                    ob.ProductionOrderId = $scope.ProductionOrderList[i].POId;
                    ob.BuyerOrder = $scope.ProductionOrderList[i].BuyerOrder;
                    ob.OwnOrder = $scope.ProductionOrderList[i].OwnOrder;
                    ob.Description = $scope.ProductionOrderList[i].Description;
                    ob.Qty = $scope.ProductionOrderList[i].Qty;
                    ob.LSD = $scope.ProductionOrderList[i].LSD;
                    ob.CommitmentDate = $scope.ProductionOrderList[i].CommitmentDate;
                    ob.ProductionStatus = $scope.ProductionOrderList[i].ProductionStatus;
                    ob.BuyerItem = $scope.ProductionOrderList[i].BuyerItem;
                    ob.OwnItem = $scope.ProductionOrderList[i].OwnItem;
                    ob.ProductCategory = $scope.ProductionOrderList[i].ProductCategory;
                    ob.Product = $scope.ProductionOrderList[i].Product;
                    ob.Customer = $scope.ProductionOrderList[i].Customer;
                    ob.Buyer = $scope.ProductionOrderList[i].Buyer;
                    ob.PONumber = $scope.ProductionOrderList[i].PONumber;
                    ob.RequiredTimeUnit = $scope.ProductionOrderList[i].RequiredTimeUnit;
                    $scope.SelectedProductionOrderList.push(ob);
                }
                else {
                    throw "This Production Order: " + $scope.ProductionOrderList[i].POId + " is already taken.";
                }
            }
        }
    }

    function checkExists(list, id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].ProductionOrderId === id) {
                return true;
            }
        }
        return false;
    }


    $scope.closePopup = function (popupName) {
        angular.element(document.querySelector("#" + popupName + "")).modal("hide");
        try {
            $("#" + popupName).data("ejDialog").close();
        } catch (e) {
        }
    }
    $scope.openPopup = function (popupName) {

        try {
            $("#" + popupName).data("ejDialog").open();
        } catch (e) {

        }
    }
    $scope.summaryRows = [{
        title: "Total Qty", summaryColumns: [{ summaryType: ej.Grid.SummaryType.Sum, displayColumn: "Qty", dataMember: "Qty", format: "{0:N0}" }],
        showCaptionSummary: true

    }];

    $scope.SalesOrderListForProductionOrderId = [];
    $scope.getSalesOrderOfProdOrderList = function (prodOrdId) {
        $scope.openPopup('dialogSOItemsForProductionOrder');
        $http({
            method: 'GET',
            url: 'OrderManagements/ProductionOrder/GetProductionRecipeMaterialList?productionOrderId=' + prodOrdId
        }).then(function successCallback(response) {
            $scope.SalesOrderListForProductionOrderId = response.data;

        });
    }

    $scope.EntityProcessSettingList = [];
    $scope.GetEntityProcessSettingData = function (EntityId) {

        $http({
            method: 'GET',
            url: 'OrderManagements/PackingContent/GetEntityProcessSettingData?EntityId=' + EntityId
        }).then(function successCallback(response) {
            $scope.EntityProcessSettingList = response.data;

            if (baseService.arrayLength($scope.EntityProcessSettingList) == 0) {
                ShowResult("Invalid configuration in (Entity Process), No packing nature.", 'failure', 'POItemPopup');
            }
            else if ($scope.EntityProcessSettingList.length > 1) {
                ShowResult("Invalid configuration in (Entity Process), more than one packing nature.", 'failure', 'POItemPopup');
            }
            else if (baseService.arrayLength($scope.EntityProcessSettingList) > 0) {
                for (var i = 0; i < $scope.EntityProcessSettingList.length; i++) {
                    $scope.packingContenNew.IsPackingSKURequired = $scope.EntityProcessSettingList[i].IsPackingSKURequired;
                    $scope.packingContenNew.PackingForm = $scope.EntityProcessSettingList[i].PackingForm;
                }
                if ($scope.packingContenNew.IsPackingSKURequired == false) {
                    ShowResult("SKU not applicable for the " + $scope.packingContenNew.PackingForm + " (Entity Process)", 'failure', 'POItemPopup');
                }
                else {
                    angular.element(document.querySelector('#POItemPopup')).modal('hide');
                }
            }
            else {
                angular.element(document.querySelector('#POItemPopup')).modal('hide');
            }
        });

    };

    $scope.selectSOItem = function ($event) {
        try {
            var soitem = $event.data;
            $scope.packingContenNew.EntityId = soitem.EntityId;
            //$scope.GetEntityProcessSettingData($scope.packingContenNew.EntityId);

            $http({
                method: 'GET',
                url: 'OrderManagements/PackingContent/GetEntityProcessSettingData?EntityId=' + $scope.packingContenNew.EntityId
            }).then(function successCallback(response) {
                $scope.EntityProcessSettingList = response.data;

                if (baseService.arrayLength($scope.EntityProcessSettingList) == 0) {
                    ShowResult("Invalid configuration in (Entity Process), No packing nature.", 'failure', 'POItemPopup');
                }
                else if ($scope.EntityProcessSettingList.length > 1) {
                    ShowResult("Invalid configuration in (Entity Process), more than one packing nature.", 'failure', 'POItemPopup');
                }
                else if (baseService.arrayLength($scope.EntityProcessSettingList) > 0) {
                    for (var i = 0; i < $scope.EntityProcessSettingList.length; i++) {
                        $scope.packingContenNew.IsPackingSKURequired = $scope.EntityProcessSettingList[i].IsPackingSKURequired;
                        $scope.packingContenNew.PackingForm = $scope.EntityProcessSettingList[i].PackingForm;
                    }
                    if ($scope.packingContenNew.IsPackingSKURequired == false) {
                        ShowResult("SKU not applicable for the " + $scope.packingContenNew.PackingForm + " (Entity Process)", 'failure', 'POItemPopup');
                    }
                    else {
                        angular.element(document.querySelector('#POItemPopup')).modal('hide');
                        $scope.packingContenNew.ProductionOrderId = soitem.POId;
                    }
                }
                else {
                    angular.element(document.querySelector('#POItemPopup')).modal('hide');
                    $scope.packingContenNew.ProductionOrderId = soitem.POId;
                }
            });

        } catch (ex) {
            ShowResult(ex, 'failure', 'POItemPopup');
        }
    };

    // #region Recipe Material and SO

    $scope.recipeMaterialFilterList = [
        { 'name': 'Master Order No', 'value': 'MasterOrderNo' },
        { 'name': 'Buyer Order#', 'value': 'BuyerOrderNo' },
        { 'name': 'Own Order#', 'value': 'OwnOrderNo' },
        { 'name': 'Buyer Item#', 'value': 'BuyerReferenceNo' },
        { 'name': 'Own Item#', 'value': 'OwnReferenceNo' },
        {
            'name': 'Material',
            'value': 'MaterialMasterName'
        },
        {
            'name': 'Product Name',
            'value': 'ProductName'
        },
        {
            'name': 'Buyer',
            'value': 'Buyer'
        },
        {
            'name': 'Article',
            'value': 'Article'
        },
        {
            'name': 'Customer',
            'value': 'Customer'
        },
        {
            'name': 'Commitment Date',
            'value': 'CommitmentDate'
        },
        {
            'name': 'Destination',
            'value': 'DestinationName'
        },
        {
            'name': 'Shipment Mode',
            'value': 'ShipmentModeName'
        },
        {
            'name': 'PO Number',
            'value': 'PONumber'
        }
    ];

    $scope.recipeMaterialParameters = {
        limit: 10
        , offset: 0
        , order: 'asc'
        , sort: 'MaterialMasterName, ArticleName'
        , searchBy: 'MaterialMasterName'
        , pageSize: 10
        , total_count: 0
        , search: null
        , serverPagination: true
    };
    $scope.recipeMaterialList = [];
    $scope.recipeMaterialParameters.searchBy = "MaterialMasterName";
    $scope.recipeMaterialParameters.search = "";
    $scope.recipeMaterialPopUp = function () {
        //angular.element(document.querySelector('#MaterialPopUp')).modal('show');
        $scope.openPopup('MaterialPopUp');
        $scope.serachSoMaterial();

    };

    $scope.summaryRows = [{
        title: "Total Qty", summaryColumns: [{ summaryType: ej.Grid.SummaryType.Sum, displayColumn: "Qty", dataMember: "Qty", format: "{0:N0}" }],
        showCaptionSummary: true

    }];
    $scope.sqlInStatement = null;
    $scope.serachSoMaterial = function serachSoMaterial() {

        try {
            if (baseService.arrayLength($scope.SelectedProductionOrderList) < 0 || baseService.arrayLength($scope.SelectedProductionOrderList) == 0) {
                throw "Select Production Order.";
            }

            if ($scope.SelectedProductionOrderList.length > 0) {
                var uniqueProductionOrderId = removeDuplicates($scope.SelectedProductionOrderList, 'ProductionOrderId');
                var wcProductionOrderId = "";
                if (uniqueProductionOrderId.length > 0) {
                    wcProductionOrderId = "IN(";
                    wcProductionOrderId += Array.prototype.map.call(uniqueProductionOrderId, function (item) { return "'" + item.ProductionOrderId + "'"; }).join(",") + ")";
                }
                $scope.sqlInStatement = wcProductionOrderId;
            }

            $http({
                method: 'GET',
                url: 'OrderManagements/PackingContent/GetSalesOrderListSearch?column=' + $scope.recipeMaterialParameters.searchBy + '&value=' + $scope.recipeMaterialParameters.search + "&productionorderid=" + $scope.sqlInStatement
            }).then(function successCallback(response) {
                $scope.recipeMaterialList = response.data;

            });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    function removeDuplicates(myArr, prop) {
        return myArr.filter((obj, pos, arr) => {
            return arr.map(mapObj => mapObj[prop]).indexOf(obj[prop]) === pos;
        });
    }

    $scope.GetCheckItemArticleSKUData = function () {
        $http({
            method: "GET",
            url: "SalesManagements/Sales/CheckItemArticleSKUList"
        }).then(function (response) {
            $scope.checkItemArticleSKUList = response.data;
        });
    };
    $scope.GetCheckItemArticleSKUData();

    $scope.CheckMaterialArticleSKU = function (mdata) {
        var getRowParty = $filter("filter")($scope.checkItemArticleSKUList, { "MaterialMasterId": mdata.MaterialMasterId, "MaterialMasterArticleId": mdata.ArticleId });
        if (getRowParty.length == 0) {
            $scope.mValid = false;
            ShowResult(mdata.MaterialMasterName + " have no article", "failure", 'MaterialPopUp');
        }

        if (mdata.WithSKU == 'Yes') {
            if (getRowParty.length > 0) {
                if (getRowParty[0].IsSKU === 1 && mdata.FirstCharacteristicsValueId === null) {
                    ShowResult(mdata.MaterialMasterName + " have no SKU Value", "failure", 'MaterialPopUp');
                    $scope.mValid = false;

                }
                if (getRowParty[0].IsSKU === 2 && mdata.FirstCharacteristicsValueId === null || mdata.SecondCharacteristicsValueId === null) {
                    if (mdata.FirstCharacteristicsValueId === null) {
                        ShowResult(mdata.MaterialMasterName + " have no SKU1 Value", "failure", 'MaterialPopUp');
                    } else {
                        ShowResult(mdata.MaterialMasterName + " have no SKU2 Value", "failure", 'MaterialPopUp');
                    }
                    $scope.mValid = false;
                }
                else
                    $scope.mValid = true;
            };
        } else {
            $scope.mValid = true;
        }
    };

    $scope.recipeMaterialListSelected = [];
    $scope.addRecipeMaterial = function () {
        try {
            $scope.packingContenNew.Qty = 0;
            var id = "";
            var productid = "";
            var groupid = "";
            for (var i = 0; i < $scope.recipeMaterialList.length; i++) {
                if ($scope.recipeMaterialList[i].Checked == true) {

                    if (baseService.isUndefinedOrNull($scope.recipeMaterialList[i].ArticleId)
                        || $scope.recipeMaterialList[i].ArticleId == "") {
                        throw "Sales order items without product are not allowed";
                    }

                    if (id == "")
                        id = $scope.recipeMaterialList[i].ArticleId;

                    if (productid == "")
                        productid = $scope.recipeMaterialList[i].ProductID;

                    if (groupid == "")
                        groupid = $scope.recipeMaterialList[i].ProductionGrouping;

                    if ($scope.recipeMaterialList[i].ProductionGrouping != groupid)
                        throw "Selecting different group materials are not allowed";

                    if ($scope.recipeMaterialList[i].ProductID != productid)
                        throw "Selecting different products are not allowed";

                    if ($scope.recipeMaterialList[i].ArticleId != id)
                        throw "Selecting different articles are not allowed";


                }
            }

            // $scope.recipeMaterialListSelected = [];
            for (var i = 0; i < $scope.recipeMaterialList.length; i++) {
                $scope.CheckMaterialArticleSKU($scope.recipeMaterialList[i]);

                if ($scope.mValid == true) {
                    if ($scope.recipeMaterialList[i].Checked == true) {
                        $scope.recipeMaterialListSelected.push($scope.recipeMaterialList[i]);
                    }
                }
            }


            for (var i = 0; i < $scope.recipeMaterialListSelected.length; i++) {
                $scope.packingContenNew.Qty += parseFloat($scope.recipeMaterialListSelected[i].Qty);
            }

            // $scope.SaveDetail();
            $scope.CloseRecipeMaterialPopUp();
        } catch (e) {
            ShowResult(e, 'failure', 'recipeMaterialPopUp');
        }


    };

    $scope.checkSameRecipe = function (data, index, event) {
        $rootScope.genericPushInTempList(data, event, $scope.productionMaterialList, 'SalesOrderId', 'SalesOrderId');
    };

    $scope.CloseRecipeMaterialPopUp = function () {
        angular.element(document.querySelector('#MaterialPopUp')).modal('hide');
    };

    // #endregion Recipe Material and SO

    $scope.OpenNoRowsPopUp = function () {
        if (baseService.arrayLength($scope.lineItemNo) > 0) {
            $scope.LineNo = $scope.lineItemNo.length;
        }
        // $scope.openPopup('dialogNoRows');
        angular.element(document.querySelector('#dialogNoRows')).modal('show');
    };
    $scope.ConfirmClose = function () {
        //var eDialog = $("#dialogNoRows").data("ejDialog");
        //eDialog.close();
        angular.element(document.querySelector('#dialogNoRows')).modal('hide');
    };
    $scope.recipeMaterialListSelected = [];

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        try {

            if (baseService.arrayLength($scope.SelectedProductionOrderList) < 0 || baseService.arrayLength($scope.SelectedProductionOrderList) == 0) {
                throw "Select Production Order.";
            }

            if ($scope.packingContenNew.NetWeight > $scope.packingContenNew.GrossWeight) {
                throw "Net Weight cann't greater than Gross Weight.";
            }

            if (!baseService.isUndefinedOrNull($scope.packingContenNew.LotNo)) {

                if (/^[ A-Za-z0-9_./-]*$/.test($scope.packingContenNew.LotNo)) {
                    ///
                } else {
                    throw "You have entered an invalid value for Lot No.";
                }
            }


            if (baseService.arrayLength($scope.recipeMaterialListSelected) <= 0) {
                throw "Select Material."
            }


            if ($scope.PackingContentForm.$valid) {
                angular.copy($scope.packingContenNew, $scope.model);
                if ($scope.Action == 'Save') {

                    var obj = angular.copy($scope.PackingChild);
                    obj.Id = null;
                    obj.PackingContentMasterId = null;
                    obj.Sequence = 1;
                    $scope.lineItemNo.push(obj);


                    $http({
                        method: 'POST',
                        url: 'OrderManagements/PackingContent/Create',
                        data: { 'data': $scope.model, 'packingContentDetails': $scope.recipeMaterialListSelected, 'packingChilds': $scope.lineItemNo, 'packingProductionOrderList': $scope.SelectedProductionOrderList},
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure');
                        }
                        else {
                            ShowResult(response.data.Message, 'success');
                            $scope.packingContenNew = response.data.Data;
                            $scope.getmasterData();
                            $scope.getDetailData($scope.packingContenNew.Id);
                            $scope.getPackingChildData($scope.packingContenNew.Id);
                            $scope.OpenNoRowsPopUp();
                            $scope.Action = 'Update';
                        }
                    }), function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    }
                }
                else {
                    $http({
                        method: 'POST',
                        url: 'OrderManagements/PackingContent/Edit',
                        data: { 'data': $scope.model, 'packingContentDetails': $scope.recipeMaterialListSelected, 'packingChilds': $scope.lineItemNo, 'packingProductionOrderList': $scope.SelectedProductionOrderList },
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure');
                        }
                        else {
                            ShowResult(response.data.Message, 'success');

                            $scope.packingContenNew = response.data.Data;
                            $scope.getmasterData();
                            $scope.getDetailData($scope.packingContenNew.Id);
                            $scope.getPackingChildData($scope.packingContenNew.Id);
                            //$scope.OpenNoRowsPopUp();
                            $scope.Action = 'Update';
                        }
                    }), function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    }
                }
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.SaveDetail = function () {
        try {
            $http({
                method: 'POST',
                url: 'OrderManagements/PackingContent/CreateDetail',
                data: { 'entities': $scope.recipeMaterialListSelected, 'MasterId': $scope.packingContenNew.Id },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');

                    $scope.getDetailData($scope.packingContenNew.Id);
                    $scope.closePopup('MaterialPopUp');
                    $scope.OpenNoRowsPopUp();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };

        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.getDetailData = function (MasterId) {
        $scope.packingContenNew.Qty = 0;
        $scope.recipeMaterialListSelected = [];
        $http.get("OrderManagements/PackingContent/GetPackingContentDetailDataList?MasterId=" + MasterId)
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.recipeMaterialListSelected = response.data;

                        for (var i = 0; i < $scope.recipeMaterialListSelected.length; i++) {
                            $scope.packingContenNew.Qty += $scope.recipeMaterialListSelected[i].Qty;
                        }
                        $scope.TQty = $scope.LineNo * $scope.packingContenNew.Qty;
                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
    };

    $scope.getPackingChildData = function (MasterId) {
        $scope.lineItemNo = [];
        $http.get("OrderManagements/PackingContent/GetPackingChildDataList?MasterId=" + MasterId)
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.lineItemNo = response.data;
                        $scope.LineNo = $scope.lineItemNo.length;
                        $scope.TQty = $scope.LineNo * $scope.packingContenNew.Qty;
                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
    };
    $scope.LineNo = 1;
    $scope.lineItemNo = [];
    $scope.PackingChild = { Id: null, PackingContentMasterId: $scope.packingContenNew.Id, Sequence: 1 }
    $scope.GenerateNoRow = function () {
        try {
            $scope.lineItemNo = [];
            if (baseService.isUndefinedOrNull($scope.LineNo) || $scope.LineNo <= 0) {
                throw "" + $scope.packingContenNew.PackingForm + " should greater than 0."
            }

            // if (baseService.arrayLength($scope.lineItemNo) <= 0) {
            for (var i = 1; i < $scope.LineNo + 1; i++) {
                var obj = angular.copy($scope.PackingChild);
                obj.Id = null;
                obj.PackingContentMasterId = $scope.packingContenNew.Id;
                obj.Sequence = i;
                $scope.lineItemNo.push(obj);
            }
            //}
            $scope.ConfirmClose();
            $scope.SavePackingChildData();
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.SavePackingChildData = function () {
        try {
            $http({
                method: 'POST',
                url: 'OrderManagements/PackingContent/CreatePackingChild',
                data: { 'Childs': $scope.lineItemNo, 'PackingContentMasterId': $scope.packingContenNew.Id },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.getPackingChildData($scope.packingContenNew.Id);
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };

        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.PackingContentDataListByPR = [];
    $scope.GetPackingContentDataByPRId = function () {
        $http({
            method: 'GET',
            url: 'OrderManagements/PackingContent/GetPackingContentDataByPRId?PRId=' + $scope.packingContenNew.ProductionOrderId
        }).then(function successCallback(response) {
            $scope.PackingContentDataListByPR = response.data;
        });
        angular.element(document.querySelector('#PackingContentPopUp')).modal('show');
    };

    $scope.ClosePackingContentPopUp = function () {
        angular.element(document.querySelector('#PackingContentPopUp')).modal('hide');
    }


    $scope.Clear = function () {
        $scope.model = {
            Id: null, ProductionOrderId: null, Qty: 0, UoMId: null, NetWeight: 0, GrossWeight: 0, WeightUoMId: null, LotNo: null, Remarks: null, AddedBy: null, AddedDate: null, AddedFromIP: null, UpdatedBy: null, UpdatedDate: null, UpdatedFromIP: null, IsPackingSKURequired: false, PackingForm: null, Entity: null
        }
        $scope.packingContenNew = Object.assign({}, $scope.model);

        $scope.recipeMaterialListSelected = [];
        $scope.lineItemNo = [];
        $scope.Action = 'Save';
    }

}

