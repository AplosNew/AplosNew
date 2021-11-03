'use strict';
BOMMasterAttachmentController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$controller', 'cboService', '$window'];
function BOMMasterAttachmentController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $controller, cboService, $window) {
    $rootScope.title = "BOM Attachment";

    //#region Attachment segment
    $scope.downloadgriddataUrl = 'GridReports/Download';
    $scope.AttachmentAction = 'Save';
    $scope.Attachmentindex = -1;
    $scope.Attachmentlsds = [];
    $scope.Attachmentpath = 'OrderManagements/BOMMasterAttachment/';
    $scope.AttachmentgetListUrl = $scope.Attachmentpath + 'getlist';
    $scope.AttachmentsaveMasterUrl = $scope.Attachmentpath + 'create';
    $scope.AttachmentupdateUrl = $scope.Attachmentpath + 'edit';
    $scope.AttachmentdeleteUrl = $scope.Attachmentpath + 'delete/';

    $scope.Attachmenttab = 1;
    $scope.AttachmentsetTab = function (newTab) {
        $scope.Attachmenttab = newTab;
    };
    $scope.AttachmentisSet = function (tabNum) {
        return $scope.Attachmenttab === tabNum;

    };

    $scope.AttachmentEdittab = 1;
    $scope.AttachmentEditsetTab = function (newTab) {
        $scope.AttachmentEdittab = newTab;
    };
    $scope.AttachmentEditisSet = function (tabNum) {
        return $scope.AttachmentEdittab === tabNum;

    };
    $scope.AttachmentcurrencyList = [];
    $scope.AttachmentBaseCurrencyId = null;
    cboService.getCboTransactionCurrencyByCompany('', function (result) {
        $scope.AttachmentcurrencyList = [];
        $scope.AttachmentcurrencyList = result;
        $scope.AttachmentBaseCurrencyId = $filter("filter")($scope.AttachmentcurrencyList, { IsBaseCurrency: 1 })[0].CurrencyId;
    });

    $scope.AttachmentOrderSearchColumn = 'MasterOrderId'; $scope.AttachmentOrderSearchValue = '';
    $scope.AttachmentmodelFilterByList = [
        { 'name': 'Master Order No', 'value': 'MasterOrderId' },
        { 'name': 'Material', 'value': 'Material' },
        { 'name': 'Article', 'value': 'Article' },

        { 'name': 'Contract No', 'value': 'ContractNo' },
        { 'name': 'LC Ref', 'value': 'LCRef' },

        { 'name': 'Product', 'value': 'Product' },
        { 'name': 'Product Category', 'value': 'ProductCategory' },
        { 'name': 'Buyer Order#', 'value': 'BuyerOrderNo' },
        { 'name': 'Own Order#', 'value': 'OwnOrderNo' },
        { 'name': 'Buyer Item#', 'value': 'BuyerItemNo' },
        { 'name': 'Own Item#', 'value': 'OwnItemNo' },
        { 'name': 'Buyer', 'value': 'Buyer' },
        { 'name': 'Customer', 'value': 'Customer' },
    ];


    $scope.AttachmentBOMSearchColumn = 'BomDesc'; $scope.AttachmentBOMSearchValue = '';
    $scope.AttachmentmodelFilterByListBOM = [
        { 'name': 'Id', 'value': 'Id' },
        { 'name': 'BOM Desc', 'value': 'BomDesc' },
        { 'name': 'Material', 'value': 'BOMMaterial' },
        { 'name': 'Article', 'value': 'BOMArticle' },
    ];

    $scope.AttachmentclosePopup = function (popupName) {
        angular.element(document.querySelector("#" + popupName + "")).modal("hide");
        try {
            $("#" + popupName).data("ejDialog").close();
        } catch (e) {

        }
    }
    $scope.AttachmentopenPopup = function (popupName) {

        try {
            $("#" + popupName).data("ejDialog").open();
        } catch (e) {

        }
    }
    $scope.AttachmentopenPopupAngular = function (popupName) {
        try {
            angular.element(document.querySelector("#" + popupName + "")).modal("show");
        } catch (e) {

        }

    }

    $scope.AttachmentselectedItem = {};
    $scope.AttachmentselectionType = '';

    $scope.AttachmentUnassignedData = [];
    $scope.AttachmentAssignedData = [];
    $scope.AttachmentgetUnassignedData = function () {
        $http({
            method: 'POST',
            data: {
                'column': $scope.AttachmentOrderSearchColumn, 'value': $scope.AttachmentOrderSearchValue, 'Assigned': 'false'
            },
            url: $scope.AttachmentgetListUrl
        }).then(function successCallback(response) {
            for (var i = 0; i < response.data.length; i++) {
                response.data[i].AddedDate = new Date(response.data[i].AddedDate);
            }
            $scope.AttachmentUnassignedData = response.data;
        });

    };
    $scope.AttachmentgetAssignedData = function () {
        $http({
            method: 'POST',
            data: {
                'column': $scope.AttachmentOrderSearchColumn, 'value': $scope.AttachmentOrderSearchValue, 'Assigned': 'true'
            },
            url: $scope.AttachmentgetListUrl
        }).then(function successCallback(response) {
            for (var i = 0; i < response.data.length; i++) {
                response.data[i].AddedDate = new Date(response.data[i].AddedDate);
            }
            $scope.AttachmentAssignedData = response.data;
        });

    };
    $scope.AttachmentgetUnassignedData();
    $scope.AttachmentgetAssignedData();

    $scope.AttachmentPopupBOMMaster = function (data, selectiontype) {
        $scope.AttachmentselectedItem = data;
        $scope.AttachmentselectionType = selectiontype;
        $scope.AttachmentopenPopup('dialogBOMSearch');
        $scope.AttachmentsearchBOM();
    }

    $scope.AttachmentBOMList = [];
    $scope.AttachmentLoadAllBOM = false;
    $scope.AttachmentsearchBOM = function () {
        $http({
            method: 'POST',
            data: {
                'column': $scope.AttachmentBOMSearchColumn, 'value': $scope.AttachmentBOMSearchValue, 'ArticleId': $scope.AttachmentselectedItem.ArticleId, 'loadAll': $scope.AttachmentLoadAllBOM
            },
            url: $scope.Attachmentpath + 'searchBOM'
        }).then(function successCallback(response) {
            for (var i = 0; i < response.data.length; i++) {
                response.data[i].AddedDate = new Date(response.data[i].AddedDate);
            }
            $scope.AttachmentBOMList = response.data;
        });

    };

    $scope.AttachmentTagBOM = function (args) {
        if ($scope.AttachmentselectionType == 'Unassigned') {

        }
        $scope.AttachmentclosePopup('dialogBOMSearch');
        var data = null;
        for (var i = 0; i < $scope.AttachmentUnassignedData.length; i++) {
            if ($scope.AttachmentUnassignedData[i].MasterOrderItemId == $scope.AttachmentselectedItem.MasterOrderItemId) {
                $scope.AttachmentUnassignedData[i].BOMMasterId = args.data.Id;
                data = $scope.AttachmentUnassignedData[i];
                break;
            }

        }
        if (data == null)
            return;

        $http({
            method: 'POST',
            data: {
                'data': data
            },
            url: $scope.Attachmentpath + 'saveAttachment'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.AttachmentgetUnassignedData();
                $scope.AttachmentgetAssignedData();
            }

        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        };

    }
    $scope.AttachmentUnTagAttachmentConfirm = function (args) {
        $scope.AttachmentselectedItem = args;
        $scope.AttachmentopenPopupAngular('confirmDeleteAttachment');

    }
    $scope.AttachmentUnTagAttachment = function () {
        $http({
            method: 'POST',
            data: {
                'ItemId': $scope.AttachmentselectedItem.MasterOrderItemId
            },
            url: $scope.Attachmentpath + 'UnTagAttachment'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.AttachmentgetUnassignedData();
                $scope.AttachmentgetAssignedData();
            }

        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        };
    }
    $scope.AttachmentApprovalRequiredQty = function (data) {

        if (!data.RequiredQtyApproved)
            data.RequiredQtyApproved = false;
        $http({
            method: 'POST',
            data: {
                'Id': data.Id, 'Approve': data.RequiredQtyApproved
            },
            url: $scope.Attachmentpath + 'ApprovalRequiredQty'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                for (var i = 0; i < $scope.AttachmentBOMItemData.length; i++) {
                    if ($scope.AttachmentBOMItemData[i].Id == data.Id) {
                        if ($scope.AttachmentBOMItemData[i].RequiredQtyApproved)
                            $scope.AttachmentBOMItemData[i].RequiredQtyApproved = false;
                        else
                            $scope.AttachmentBOMItemData[i].RequiredQtyApproved = true;

                        var gridObj = $("#GridEditBOQItem").data("ejGrid");
                        gridObj.refreshContent(true);
                        gridObj.refreshTemplate();


                        var TotalItem = 0; var TotalApproved = 0;
                        for (var kk = 0; kk < $scope.AttachmentBOMItemData.length; kk++) {
                            if ($scope.AttachmentBOMItemData[kk].VendorId == $scope.AttachmentBOMItemData[i].VendorId
                                && $scope.AttachmentBOMItemData[kk].MaterialMasterId == $scope.AttachmentBOMItemData[i].MaterialMasterId
                                && $scope.AttachmentBOMItemData[kk].ArticleId == $scope.AttachmentBOMItemData[i].ArticleId) {
                                TotalItem++;
                                if ($scope.AttachmentBOMItemData[kk].RequiredQtyApproved)
                                    TotalApproved++;
                            }
                        }

                        for (var kk = 0; kk < $scope.AttachmentRateModel.length; kk++) {
                            if ($scope.AttachmentRateModel[kk].VendorId == $scope.AttachmentBOMItemData[i].VendorId
                                && $scope.AttachmentRateModel[kk].MaterialMasterId == $scope.AttachmentBOMItemData[i].MaterialMasterId
                                && $scope.AttachmentRateModel[kk].ArticleId == $scope.AttachmentBOMItemData[i].ArticleId) {

                                if (TotalApproved == 0)
                                    $scope.AttachmentRateModel[kk].RequiredQtyApprovedFlag = 'NONE';
                                else if (TotalApproved < TotalItem)
                                    $scope.AttachmentRateModel[kk].RequiredQtyApprovedFlag = 'PARTIAL';
                                else if (TotalApproved = TotalItem)
                                    $scope.AttachmentRateModel[kk].RequiredQtyApprovedFlag = 'FULL';

                                break;
                            }
                        }


                        var gridObj = $("#GridEditBOQItemRateParent").data("ejGrid");
                        gridObj.refreshContent(true);
                        gridObj.refreshTemplate();

                        break;
                    }


                }
                for (var i = 0; i < $scope.AttachmentBOMItemDataChild.length; i++) {
                    if ($scope.AttachmentBOMItemDataChild[i].Id == data.Id) {
                        if ($scope.AttachmentBOMItemDataChild[i].RequiredQtyApproved)
                            $scope.AttachmentBOMItemDataChild[i].RequiredQtyApproved = false;
                        else
                            $scope.AttachmentBOMItemDataChild[i].RequiredQtyApproved = true;

                        var gridObj = $("#GridEditBOQItemChild").data("ejGrid");
                        gridObj.refreshContent(true);
                        gridObj.refreshTemplate();
                        break;
                    }
                }


            }

        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        };
    }

    $scope.AttachmentApprovalRequiredQtyMaterial = function (data) {


        $http({
            method: 'POST',
            data: {
                'MasterOrderItemId': $scope.AttachmentSelectedBOMRow.MasterOrderItemId, 'VendorId': data.VendorId, 'MaterialMasterId': data.MaterialMasterId, 'ArticleId': data.ArticleId, 'Approve': data.RequiredQtyApprovedFlag == 'FULL' ? false : true
            },
            url: $scope.Attachmentpath + 'ApprovalRequiredQtyMaterial'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {

                if (data.RequiredQtyApprovedFlag == 'FULL')
                    data.RequiredQtyApprovedFlag = 'NONE';

                else if (data.RequiredQtyApprovedFlag == 'PARTIAL')
                    data.RequiredQtyApprovedFlag = 'FULL';

                else if (data.RequiredQtyApprovedFlag == 'NONE')
                    data.RequiredQtyApprovedFlag = 'FULL';

                for (var i = 0; i < $scope.AttachmentRateModel.length; i++) {
                    if ($scope.AttachmentRateModel[i].VendorId == data.VendorId
                        && $scope.AttachmentRateModel[i].MaterialMasterId == data.MaterialMasterId
                        && $scope.AttachmentRateModel[i].ArticleId == data.ArticleId) {

                        $scope.AttachmentRateModel[i].RequiredQtyApprovedFlag = data.RequiredQtyApprovedFlag;

                        for (var kk = 0; kk < $scope.AttachmentBOMItemData.length; kk++) {
                            if ($scope.AttachmentBOMItemData[kk].VendorId == data.VendorId
                                && $scope.AttachmentBOMItemData[kk].MaterialMasterId == data.MaterialMasterId
                                && $scope.AttachmentBOMItemData[kk].ArticleId == data.ArticleId) {
                                if (data.RequiredQtyApprovedFlag == "NONE")
                                    $scope.AttachmentBOMItemData[kk].RequiredQtyApproved = false;
                                else
                                    $scope.AttachmentBOMItemData[kk].RequiredQtyApproved = true;


                                var gridObj = $("#GridEditBOQItem").data("ejGrid");
                                gridObj.refreshContent(true);
                                gridObj.refreshTemplate();
                            }
                        }

                        var gridObj = $("#GridEditBOQItemRateParent").data("ejGrid");
                        gridObj.refreshContent(true);
                        gridObj.refreshTemplate();
                        break;
                    }


                }

            }

        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        };
    }

    $scope.AttachmentBOMProcess = function (args) {
        $http({
            method: 'GET',
            url: $scope.Attachmentpath + 'BOMProcess?MasterOrderItemId=' + args
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
                if (response.data.FileName != "") {
                    $rootScope.report($scope.downloadgriddataUrl + "?FileName=" + response.data.FileName);
                }
            }
            else {
                for (var i = 0; i < $scope.AttachmentAssignedData.length; i++) {
                    if ($scope.AttachmentAssignedData[i].MasterOrderItemId == args) {
                        $scope.AttachmentAssignedData[i].HasBOQ = true;
                        break;
                    }
                }

                var gridObj = $("#GridAttachmentAssigned").data("ejGrid");
                gridObj.refreshContent(true);
                gridObj.refreshTemplate();
            }

        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        };
    }

    $scope.AttachmentBOMItemData = [];
    $scope.AttachmentBOMItemDataParent = [];
    $scope.AttachmentBOMItemDataChild = [];
    $scope.SelectedBOMItemData = {};
    $scope.VendorRequestSource = 'BOMITEM';
    $scope.AttachmentOpenParty = function (args) {
        $scope.SelectedBOMItemData = args;
    }
    $scope.AttachmentclearVendor = function (args) {

        for (var i = 0; i < $scope.AttachmentBOMItemData.length; i++) {
            if ($scope.AttachmentRateModel[i].ArticleId == args.ArticleId) {
                $scope.AttachmentRateModel[i].VendorId = null;
                $scope.AttachmentRateModel[i].Vendor = null;

                var gridObj = $("#GridEditBOQItemRateParent").data("ejGrid");
                gridObj.refreshContent(true);
                gridObj.refreshTemplate();
                break;
            }
        }

        for (var i = 0; i < $scope.AttachmentBOMItemDataChild.length; i++) {
            if ($scope.AttachmentBOMItemDataChild[i].Id == args.Id) {
                $scope.AttachmentBOMItemDataChild[i].VendorId = null;
                $scope.AttachmentBOMItemDataChild[i].Vendor = null;

                var gridObj = $("#GridEditBOQItemChild").data("ejGrid");
                gridObj.refreshContent(true);
                gridObj.refreshTemplate();
                break;
            }
        }




    }

    $scope.AttachmentSelectedBOMRow = {};
    $scope.AttachmentRateModel = [];
    $scope.AttachmentLoadBomRequiredQty = function (args) {
        $scope.AttachmentRateModel = [];
        $scope.AttachmentEdittab = 1;
        $scope.AttachmentSelectedBOMRow = args;
        $http({
            method: 'GET',
            url: $scope.Attachmentpath + 'LoadBomRequiredQty?MasterOrderItemId=' + args.MasterOrderItemId
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                $scope.AttachmentBOMItemData = response.data.ChildData;
                $scope.AttachmentBOMItemDataParent = response.data.ParentData;
                $scope.AttachmentRateModel = response.data.RateData;
            }

        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        };

    }
    $scope.MasterOrderBOMReport = function (args) {

        try {
            var file_src = $scope.Attachmentpath + 'MasterOrderBOMReport?MasterOrderItemId=' + args.MasterOrderItemId
            $rootScope.report(file_src);

        } catch (e) {

        }
    }
   
    $scope.ItemList = [];
    $scope.MasterOrderBOMReportByMaterial = function (data) {
        $scope.AttachmentSelectedBOMRow = data;
        $rootScope.openPopup('dialogBOMItemSelectionForReport');
        $http({
            method: 'POST',
            data: { 'MasterOrderItemId': data.MasterOrderItemId },
            url: $scope.Attachmentpath + "GetBOMItemListForReport"
        }).then(function successCallback(response) {
            $scope.ItemList = response.data;
        });
    }
    $scope.refreshTemplateItem = function (args) {
        if (args.rowIndex == 0) {
            $("#headchkItem").ejCheckBox({ "change": CheckAllItem });
        }
    }
    $scope.getBOMReport = function () {

        var MasterOrderItemId = $scope.AttachmentSelectedBOMRow.MasterOrderItemId;
        var _itemIds = ej.DataManager($scope.ItemList).executeLocal(ej.Query().where("Checked", "equal", true));
        var itemids = getString(_itemIds, "ArticleId");


        try {
            var file_src = $scope.Attachmentpath + 'GetBOMReport?ItemIds=' + itemids + '&MasterOrderItemId=' + MasterOrderItemId;
            $rootScope.report(file_src);

        } catch (e) {

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
    function CheckAllItem(e) {
        if (!e.isInteraction)
            return;

        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;

        }

        var filtered = $("#BOQItems").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.ItemList.length; i++) {
                $scope.ItemList[i].Checked = ChkOrUnchk;
            }
        }
        else {

            for (var j = 0; j < filtered.length; j++) {

                filtered[j].Checked = ChkOrUnchk;
            }


        }
        var gridObj = $("#BOQItems").data("ejGrid");
        gridObj.refreshContent();
    }
    $scope.AttachmentLoadBomRequiredQtyChild = function (args, ParentId) {
        $http({
            method: 'GET',
            url: $scope.Attachmentpath + 'LoadBomRequiredQtyChild?MasterOrderItemId=' + args + "&ParentId=" + ParentId
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                $scope.AttachmentBOMItemDataChild = response.data;
            }

        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        };

    }
    $scope.AttachmentUpdateBomRequiredQty = function () {


        var ColumnToUpdate = ["Id", "RequiredQtyPO", "UoMId", "BaseUoMId", "POUoMId", "MaterialMasterId"];
        var selectedData = ej.DataManager($scope.AttachmentBOMItemData).executeLocal(ej.Query().select(ColumnToUpdate));

        $http({
            method: 'POST',
            data: { data: selectedData }, //$scope.AttachmentBOMItemData
            url: $scope.Attachmentpath + 'UpdateBomRequiredQty'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                $scope.AttachmentLoadBomRequiredQty($scope.AttachmentSelectedBOMRow);
                ShowResult(response.data.Message, 'success');
            }

        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        };

    }
    $scope.AttachmentUpdateRate = function () {


        var ColumnToUpdate = ["ArticleId", "VendorId", "Rate", "CurrencyId", "MasterOrderItemId", "POUoMId"];
        var selectedData = ej.DataManager($scope.AttachmentRateModel).executeLocal(ej.Query().select(ColumnToUpdate));
        for (var i = 0; i < selectedData.length; i++) {
            if (selectedData[i].Rate > 0 && angular.isUndefinedOrNull(selectedData[i].CurrencyId)) {
                ShowResult("Please provide curreny", 'failure');
                return;
            }
        }


        $http({
            method: 'POST',
            data: { data: selectedData }, //$scope.AttachmentBOMItemData
            url: $scope.Attachmentpath + 'UpdateBomRequiredQtyRate'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                $scope.AttachmentLoadBomRequiredQty($scope.AttachmentSelectedBOMRow);
                ShowResult(response.data.Message, 'success');
            }

        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        };

    }


    $scope.AttachmentUpdateBomRequiredQtyChild = function () {
        $http({
            method: 'POST',
            data: { data: $scope.AttachmentBOMItemDataChild },
            url: $scope.Attachmentpath + 'UpdateBomRequiredQty'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {


                ShowResult(response.data.Message, 'success');
            }

        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        };

    }
    $scope.rowDataBoundRequisition = function (e) {
        try {
            if (e.data.IncompleteMaterial == true)
                e.row.css("background-color", '#ff0000');
        } catch (e) {

        }


    }

    $scope.changeIsMainMaterial = function (args) {
        if (!args.isInteraction)
            return;

        var isMainMaterial = false;
        if (args.checkState === "check") {
            isMainMaterial = true;
        }

        var data = ej.DataManager($scope.AttachmentRateModel).executeLocal(ej.Query().where("RowId", "equal", args.model.id))[0];

        $http({
            method: 'POST',
            data: {
                'MasterOrderItemId': $scope.AttachmentSelectedBOMRow.MasterOrderItemId, 'VendorId': data.VendorId, 'MaterialMasterId': data.MaterialMasterId, 'ArticleId': data.ArticleId, 'isMainMaterial': isMainMaterial
            },
            url: $scope.Attachmentpath + 'UpdateMainMaterialFlag'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
            }

        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        };
    }
    //#endregion  Attachment segment

    //#region BOM Master segment
    $scope.partyType = "Vendor";
    $controller("partyBaseController", { $scope: $scope, $http: $http });
    $scope.closePartyPopUp = function () {
        if ($scope.partyIndex !== -1) {
            var party = $scope.partyList[$scope.partyIndex];
            if ($scope.VendorRequestSource == 'BOMITEM') {
                for (var i = 0; i < $scope.AttachmentBOMItemData.length; i++) {
                    if ($scope.AttachmentRateModel[i].ArticleId == $scope.SelectedBOMItemData.ArticleId) {
                        $scope.AttachmentRateModel[i].VendorId = party.Id;
                        $scope.AttachmentRateModel[i].Vendor = party.UserName;

                        var gridObj = $("#GridEditBOQItemRateParent").data("ejGrid");
                        gridObj.refreshContent(true);
                        gridObj.refreshTemplate();
                        break;
                    }
                }
                for (var i = 0; i < $scope.AttachmentBOMItemDataChild.length; i++) {
                    if ($scope.AttachmentBOMItemDataChild[i].Id == $scope.SelectedBOMItemData.Id) {
                        $scope.AttachmentBOMItemDataChild[i].VendorId = party.Id;
                        $scope.AttachmentBOMItemDataChild[i].Vendor = party.UserName;

                        var gridObj = $("#GridEditBOQItemChild").data("ejGrid");
                        gridObj.refreshContent(true);
                        gridObj.refreshTemplate();
                        break;
                    }
                }




            }
            else {
                //$scope.bomDetailNew.VendorId = party.Id;
                //$scope.bomDetailNew.PartyCode = party.Code;
                //$scope.bomDetailNew.PartyName = party.UserName;
            }
        }
        $scope.hidePartyPopUp();
    };


    //#endregion
}
