'use strict';
SKUUploadController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$window'];
function SKUUploadController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window) {
    $rootScope.title = "SKU Upload";

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;

    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };


    $scope.moentityList = [];
    $http({
        method: 'GET',
        url: 'OrderManagements/ProductionOrder/GetMasterOrderEntityCbo'
    }).then(function successCallback(response) {
        $scope.moentityList = response.data;
    });


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
        angular.element(document.querySelector('#recipeMaterialPopUp')).modal('show');
        $scope.serachSoMaterial();

    };
    $scope.SelectSalesOrder = function (obj) {
        $scope.SalesOrder = obj.data.SalesOrderId
        angular.element(document.querySelector('#recipeMaterialPopUp')).modal('hide');
    }

    $scope.summaryRows = [{
        title: "Total Qty", summaryColumns: [{ summaryType: ej.Grid.SummaryType.Sum, displayColumn: "Qty", dataMember: "Qty", format: "{0:N0}" }],
        showCaptionSummary: true

    }];
    $scope.serachSoMaterial = function serachSoMaterial() {
        var DropDownEntityListObj = $("#moentityDropdown").data("ejDropDownList");
        $scope.MOEntityId = DropDownEntityListObj.getSelectedValue();

        if (angular.isUndefinedOrNull($scope.MOEntityId)) {
            for (var i = 0; i < DropDownEntityListObj.popupListItems.length; i++) {
                if (angular.isUndefinedOrNull($scope.MOEntityId)) {
                    $scope.MOEntityId = + DropDownEntityListObj.popupListItems[i].Id;
                } else {
                    $scope.MOEntityId += ',' + DropDownEntityListObj.popupListItems[i].Id;
                }
            }
        }
        $http({
            method: 'GET',
            url: 'OrderManagements/MasterOrder/GetSalesOrderListSearch?column=' + $scope.recipeMaterialParameters.searchBy + '&value=' + $scope.recipeMaterialParameters.search +  "&moentity=" + $scope.MOEntityId
        }).then(function successCallback(response) {

            for (var i = 0; i < response.data.length; i++) {
                for (var J = 0; J < $scope.recipeMaterialListSelected.length; J++) {
                    if (response.data[i].SalesOrderId == $scope.recipeMaterialListSelected[J].SalesOrderId)
                        response.data[i].Checked = true;
                }
            }
            $scope.MaterialID = "";//important for changing color
            $scope.recipeMaterialList = response.data;

        });


    }

    $scope.recipeMaterialListSelected = [];
    $scope.addRecipeMaterial = function () {

        try {
            var id = "";
            var productid = "";
            var groupid = "";
            for (var i = 0; i < $scope.recipeMaterialList.length; i++) {
                if ($scope.recipeMaterialList[i].Checked == true) {
                    //if (baseService.isUndefinedOrNull($scope.recipeMaterialList[i].ProductionGrouping)
                    //    || $scope.recipeMaterialList[i].ProductionGrouping == "")
                    //{
                    //    throw "Sales orders without product group are not allowed";
                    //}
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



                    if (!baseService.isUndefinedOrNull($scope.recipeMaterialList[i].ProductionGrouping)) {
                        if ($scope.recipeMaterialList[i].ProductionGrouping != groupid) {
                            throw "Selecting different group materials are not allowed";
                        }
                        else {
                            if ($scope.recipeMaterialList[i].ArticleId != id) {
                                $scope.message_DiffArticleconfirmation = 'You are going to add different articles. Are you sure?';
                                angular.element(document.querySelector('#confirmDiffArticlePopUp')).modal('show');
                            }
                        }

                    } else {
                        if ($scope.recipeMaterialList[i].ArticleId != id)
                            throw "Selecting different articles are not allowed";

                    }
                    //if ($scope.recipeMaterialList[i].ProductID != productid)
                    //    throw "Selecting different products are not allowed";




                    //if ($scope.recipeMaterialList[i].MaterialMasterId != id)
                    //    throw "Selecting different material are not allowed";

                }
            }

            $scope.recipeMaterialListSelected = [];
            for (var i = 0; i < $scope.recipeMaterialList.length; i++) {
                if ($scope.recipeMaterialList[i].Checked == true) {
                    $scope.recipeMaterialListSelected.push($scope.recipeMaterialList[i]);
                }
            }

            if (baseService.isUndefinedOrNull($scope.message_DiffArticleconfirmation)) {
                $scope.CloseRecipeMaterialPopUp();
            }
        } catch (e) {
            ShowResult(e, 'failure', 'recipeMaterialPopUp');
        }
    };



    $scope.message_DiffArticleconfirmation = null;
    $scope.message_DiffArticle1confirmation = null;

    $scope.ConDiffArticle = function () {
        $scope.message_DiffArticle1confirmation = 'You are going to add different articles. Are you sure?';
        angular.element(document.querySelector('#confirmDiffArticle1PopUp')).modal('show');
    }

    $scope.OverConDiffArticle = function () {
        $scope.CloseRecipeMaterialPopUp();
    }


    $scope.checkSameRecipe = function (data, index, event) {
        $rootScope.genericPushInTempList(data, event, $scope.productionMaterialList, 'SalesOrderId', 'SalesOrderId');
    };

    $scope.CloseRecipeMaterialPopUp = function () {
        angular.element(document.querySelector('#recipeMaterialPopUp')).modal('hide');
    };

    // #endregion Recipe Material and SO
    $("#uploadSOImage").change(function () {
        $scope.picdata = this.files[0];
    });
    $scope.getFile = function () {
        $scope.progress = 0;
        fileReader.readAsDataUrl($scope.file, $scope)
            .then(function (result) {
                $scope.imageSrc = result;
            });
    };

    $scope.GetSKUSampleFile = function () {
        var ReportFormat = 'Excel';
        location.href = 'OrderManagements/MasterOrder/GetSKUSampleFile?reportFormat=' + ReportFormat + '&salesOrderId=' + $scope.SalesOrder;
    };

    

    $scope.soData = [];
    $scope.ImportSOData = function () {
        try {
            $scope.$broadcast('show-errors-check-validity');
            if ($scope.ModelNew3Form.$valid) {
                var picData = new FormData();
                $http({
                    method: 'POST',
                    url: 'OrderManagements/MasterOrder/ImportSKUData',
                    headers: { 'Content-Type': undefined },
                    transformRequest: function (data) {
                        picData.append("fileNew", angular.toJson(data.fileNew));
                        if (baseService.isUndefinedOrNull($scope.picdata) === false) {
                            picData.append('file', data.file);
                        }
                        return picData;
                    },
                    data: {
                        'file': $scope.picdata

                    }
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        $scope.ShowSaveBtn = false;
                        ShowResult(response.data.Message, "failure");

                    }
                    else {
                        $scope.soData = [];
                        $scope.soData = response.data;
                        $scope.ShowSaveBtn = true;
                    }
                }, function errorCallback(response) {

                });
                return true;

            }
        } catch (e) {

            ShowResult(e, "failure");
        }
    };

    
    $scope.SKU1List = [];
    function groupSoBySku1(data) {
        var map = {};
        var result = [];

        angular.forEach(data, function (row) {
            var key = row.SalesOrderId + '|' + row.SKU1Id;

            if (!map[key]) {
                map[key] = { SalesOrderId: row.SalesOrderId, SKU1Id: row.SKU1Id, Qty: 0 };
                result.push(map[key]);
            }

            map[key].Qty += Number(row.Qty) || 0;
        });

        return result;
    }

    // Usage
   
    $scope.SaveSOData = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.SalesOrder)) {
                throw "SalesOrder is required.";
            }
            $scope.SKU1List = [];
            $scope.SKU1List = groupSoBySku1($scope.soData);
            $scope.ShowSaveBtn = true;
            $http({
                method: 'POST',
                url: 'OrderManagements/MasterOrder/SaveUploadedData',
                data: { 'sku1List': $scope.SKU1List,'dataList': $scope.soData, 'salesOrderId': $scope.SalesOrder },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    $scope.ShowSaveBtn = true;
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.soData = [];
                    $("#uploadSOImage").val(null);
                    $scope.ShowSaveBtn = false;
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };

        } catch (e) {
            ShowResult(e, 'failure');
            $scope.ShowSaveBtn = false;
        }
    };

}