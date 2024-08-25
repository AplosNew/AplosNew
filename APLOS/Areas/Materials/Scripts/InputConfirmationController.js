'use strict';
InputConfirmationController.$inject = ["cboService", "commonMessage", "$scope", "$rootScope", "baseService", "$filter", "$window", "$http", "$controller"];
function InputConfirmationController(cboService, commonMessage, $scope, $rootScope, baseService, $filter, $window, $http, $controller) {
    $rootScope.title = "Input Confirmation";
    $scope.Action = 'Save';
    $scope.index = -1;

    $scope.path = 'Materials/InputConfirmation/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'Create';
    $scope.updateUrl = $scope.path + 'Update';
    $scope.deleteUrl = $scope.path + 'delete/';
    $controller('baseMaterialAndArticleController', { $scope: $scope, $http: $http });

    $scope.modelFilterByList = [
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
        { 'name': 'SO Desc', 'value': 'SODesc' },
        { 'name': 'Buyer', 'value': 'buyer' },
        { 'name': 'Customer', 'value': 'Customer' },
    ];

    $scope.ModelNew = { Id: null, POId: null, EntityId: null, ProcessId: null, ResponsiblePersonId: null, CheckedById: null, WorkCenterMasterId: null, ConfirmationDate: null, Remarks: null, AddedBy: null, AddedDate: null, AddedFromIP: null, UpdatedBy: null, UpdatedDate: null, UpdatedFromIP: null };
    $scope.entityList = [];
    $scope.getAllEntities = function () {
        $http({
            method: 'Get',
            url: "Materials/MaterialIssueControl/EntityList"
        }).then(function successCallback(response) {
            $scope.entityList = response.data;
        });
    }
    $scope.getAllEntities();



    $scope.wcList = [];
    $scope.loadWC = function (processid, entityId) {
        cboService.GetToWCProcessCbo(processid, entityId, function (result) {
            $scope.wcList = result;
        });
    };

    $scope.GetEntityName = function () {
        for (var i = 0; i < $scope.entityList.length; i++) {
            if ($scope.entityList[i].Value == $scope.ModelNew.EntityId) {
                $scope.ModelNew.Entity = $scope.entityList[i].Text;
                break;
            }
        }
    }

    $scope.PRSearchColumn = 'Id';
    $scope.PRSearchValue = null;
    $scope.modelList = [];
    $scope.getData = function () {
        $scope.modelList = [];
        if (!baseService.isUndefinedOrNull($scope.ModelNew.EntityId)) {
            $http({
                method: 'POST',
                data: {
                    'entityid': $scope.ModelNew.EntityId, 'column': $scope.PRSearchColumn, 'value': $scope.PRSearchValue
                },
                url: $scope.getListUrl
            }).then(function successCallback(response) {
                $scope.modelList = response.data;
            });
        }
    };

    $scope.MCFilterByList = [
        { 'name': 'Prod. Order#', 'value': 'POId' },
        { 'name': 'IssueId', 'value': 'IssueId' },
    ];

    $scope.MCSearchColumn = 'POId';
    $scope.MCSearchValue = null;
    $scope.savedList = [];
    $scope.GetSavedData = function () {
        $scope.savedList = [];
        $http({
            method: 'POST',
            data: {
                'column': $scope.MCSearchColumn, 'value': $scope.MCSearchValue
            },
            url: 'Materials/InputConfirmation/GetSavedData'
        }).then(function successCallback(response) {
            $scope.savedList = response.data;
        });
    };
    $scope.GetSavedData();

    $scope.Get = function (obj) {
        $scope.ModelNew.POId = obj.data.Id;
        $scope.GetIssueSlipDataByPOIdList();

        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.IssueSlipDataList = [];
    $scope.GetIssueSlipDataByPOIdList = function () {
        try {
            $http.get('Materials/InputConfirmation/GetIssueSlipDataByPOIdList?ProductionOrderId=' + $scope.ModelNew.POId)
                .then(function (response) {
                    $scope.IssueSlipDataList = response.data;
                    $scope.GetSOItemList();
                    $scope.loadProcessList();
                });
        } catch (ex) {
            ShowResult(ex, 'Info');
        }
    };

    $scope.loadProcessList = function () {
        $http({
            method: 'Get',
            url: "Materials/InputConfirmation/GetFirstProcessCbo?ProductionOrderId=" + $scope.ModelNew.POId
        }).then(function successCallback(result) {
            $scope.processList = result.data;
            if (baseService.arrayLength(result.data) === 1) {
                $scope.ModelNew.ProcessId = $scope.processList[0].Value;
                $scope.loadWC($scope.ModelNew.ProcessId, $scope.ModelNew.EntityId);
            }
        });
    };

    $scope.SOItemList = [];
    $scope.GetSOItemList = function () {
        $scope.SOItemList = [];
        $http.get('Materials/InputConfirmation/GetSOItemList?entityid=' + $scope.ModelNew.EntityId + '&ProductionOrderId=' + $scope.ModelNew.POId + '&masterId=' + $scope.ModelNew.Id)
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.SOItemList = response.data;
                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
    }

    $scope.SearchmaterialList = [];
    $scope.AddMaterial = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.ModelNew.ConfirmationDate)) {
                throw "Confirmation Date is required.";
            }
            $scope.SearchmaterialList = [];
            $http.get('Materials/InputConfirmation/GetInventoryMaterialData?confirmdate=' + $scope.ModelNew.ConfirmationDate)
                .then(
                    function successCallback(response) {
                        if (baseService.arrayLength(response.data) > 0) {
                            $scope.SearchmaterialList = response.data;
                        }

                        angular.element(document.querySelector('#SOpopUp')).modal('show');
                    },
                    function errorCallback(response) {
                        ShowResult(response, 'failure');
                    });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    function checkExistsItem(list, id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].InventoryReceiveDetailId == id) {
                return false;
            }
        }
        return true;
    }

    $scope.itemList = [];
    $scope.closeSOPopUp = function () {
        try {
            for (var i = 0; i < $scope.SearchmaterialList.length; i++) {
                var obj = {};
                if ($scope.SearchmaterialList[i].Flag) {
                    if (checkExistsItem($scope.IssueSlipDataList, $scope.SearchmaterialList[i].InventoryReceiveDetailId)) {
                        if (baseService.isUndefinedOrNull($scope.SearchmaterialList[i].TranQty) || $scope.SearchmaterialList[i].TranQty == 0) {
                            throw "Please input Transaction Qty.";
                        }
                        else if ($scope.SearchmaterialList[i].TranQty > $scope.SearchmaterialList[i].BalanceStock) {
                            throw "Transaction Qty can not greater than Balance Qty.";
                        }
                        else {
                            obj.Id = null;
                            obj.IssueSlipId = null;
                            obj.IssueSlipRowId = null;
                            obj.CostCenter = null;
                            obj.Article = $scope.SearchmaterialList[i].Article;
                            obj.ArticleId = $scope.SearchmaterialList[i].ArticleId;
                            obj.UOM = $scope.SearchmaterialList[i].BUoM;
                            obj.UOMId = $scope.SearchmaterialList[i].TransactionUoMId;
                            obj.InventoryReceiveId = $scope.SearchmaterialList[i].InventoryReceiveId;
                            obj.InventoryReceiveDetailId = $scope.SearchmaterialList[i].InventoryReceiveDetailId;
                            obj.RequestedQty = $scope.SearchmaterialList[i].TranQty;
                            obj.IssueQty = $scope.SearchmaterialList[i].IssueQty;
                            obj.OtherQty = 0;
                            obj.WasteQty = 0;
                            obj.PendingBookedQty = 0;
                            obj.TotalQty = $scope.SearchmaterialList[i].TranQty;

                            $scope.IssueSlipDataList.push(obj);
                            obj = {};
                        }
                    }
                }
            }
            angular.element(document.querySelector('#SOppUp')).modal('hide');
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.GetTotalQty = function (obj) {

        obj.data.TotalQty = parseFloat(obj.data.RequestedQty + obj.data.OtherQty + obj.data.WasteQty).toFixed(2);
        obj.data.PendingBookedQty = parseFloat(obj.data.RequestedQty - obj.data.OtherQty).toFixed(2);

        var gridObj = $("#SOGrid").data("ejGrid");
        gridObj.refreshContent(true);
        gridObj.refreshTemplate();
    }

    // #region checkbox all

    $scope.refreshTemplateemployee = function (args) {
        $("#headchk").ejCheckBox({ "change": CheckBoxSelectAllEmolyeeWise });
    };

    function CheckBoxSelectAllEmolyeeWise(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        var filtered = $("#SOPOPGrid").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.SearchmaterialList.length; i++) {
                $scope.SearchmaterialList[i].Flag = ChkOrUnchk;
            }
        }
        else {

            for (var j = 0; j < filtered.length; j++) {
                filtered[j].CheckBoxSelect = ChkOrUnchk;
            }

        }
        var gridObj = $("#SOPOPGrid").data("ejGrid");
        gridObj.refreshContent();
    };

    // #endregion checkbox all



    $scope.GetSaved = function (obj) {
        $scope.ModelNew = Object.assign({}, obj.data);
        $scope.ModelNew.IssueDate = $filter('dateFiltering')($scope.ModelNew.IssueDate, 'dd-M-yyyy');
        $scope.loadProcessList();
        $scope.Action = 'Update';
        $scope.GetSOItemList();
        $scope.GetSavedChildDetailData();

        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.GetSavedChildDetailData = function () {
        $scope.IssueSlipDataList = [];
        $http.get('Materials/InputConfirmation/GetSavedChildData?masterId=' + $scope.ModelNew.Id)
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.IssueSlipDataList = response.data;
                    }
                    $scope.GetInputConfirmationAdditionalMaterialData();
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });

    };

    $scope.GetInputConfirmationAdditionalMaterialData = function () {
        $scope.AddOtherMaterialList = [];
        $http.get('Materials/InputConfirmation/GetInputConfirmationAdditionalMaterialData?masterId=' + $scope.ModelNew.Id)
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.AddOtherMaterialList = response.data;
                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });

    };
    $scope.disbtn = false;
    $scope.Action = 'Save';
    $scope.Save = function () {

        try {
            var db4day = new Date().setDate(new Date().getDate() - 15);
            $scope.db4day = $filter('dateFiltering')(new Date(db4day), 'dd-MM-yyyy');
            var today = $filter('dateFiltering')(new Date(), 'dd-MM-yyyy')
            if (new Date($scope.ModelNew.ConfirmationDate) < new Date(today)) {
                if (new Date($scope.ModelNew.ConfirmationDate) < new Date($scope.db4day)) {
                    $scope.disbtn = false;
                    //throw "Day Before YesterDay is not allowed.";
                    throw "Only 15 days back date is allowed.";
                }
            }

            if (new Date($scope.ModelNew.ConfirmationDate) > new Date(today)) {
                throw "Future date is not allowed.";
            }

            if (baseService.isUndefinedOrNull($scope.ModelNew.ResponsiblePersonId)) {
                $scope.disbtn = false;
                throw "Responsible Person is required.";
            }
            if (baseService.isUndefinedOrNull($scope.ModelNew.CheckedById)) {
                $scope.disbtn = false;
                throw "Checked By is required.";
            }

            $scope.$broadcast('show-errors-check-validity');
            if ($scope.ModelNewForm.$valid) {
                if ($scope.Action === 'Save') {

                    if (baseService.isUndefinedOrNull($scope.ModelNew.POId)) {
                        $scope.disbtn = false;
                        throw "Select Production Order.";
                    }


                    $scope.disbtn = true;
                    $http({
                        method: 'POST',
                        url: $scope.saveUrl,
                        data: {
                            'model': $scope.ModelNew
                            , 'soList': $scope.SOItemList
                            , 'dataList': $scope.IssueSlipDataList
                            , 'otherMaterialList': $scope.AddOtherMaterialList
                        },
                        dataType: 'JSON'
                        , contentType: "application/json charset=utf-8"
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            $scope.disbtn = false;
                            ShowResult(response.data.Message, 'failure');
                        }
                        else {
                            $scope.disbtn = false;
                            ShowResult(response.data.Message, 'success');
                            $scope.Clear();
                            $scope.GetSavedData();
                        }
                    }), function errorCallBack(response) {
                        $scope.disbtn = false;
                        ShowResult(response.data.Message, 'failure');
                    };
                }
                else {
                    $scope.disbtn = true;
                    $http({
                        method: 'POST',
                        url: $scope.updateUrl,
                        data: {
                            'model': $scope.ModelNew
                            , 'soList': $scope.SOItemList
                            , 'dataList': $scope.IssueSlipDataList
                            , 'otherMaterialList': $scope.AddOtherMaterialList
                        },
                        dataType: 'JSON'
                        , contentType: "application/json charset=utf-8"
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            $scope.disbtn = false;
                            ShowResult(response.data.Message, 'failure');
                        }
                        else {
                            $scope.disbtn = false;
                            ShowResult(response.data.Message, 'success');
                            $scope.Clear();
                            $scope.GetSavedData();
                        }
                    }), function errorCallBack(response) {
                        $scope.disbtn = false;
                        ShowResult(response.data.Message, 'failure');
                    };
                }
            }
        } catch (e) {
            $scope.disbtn = false;
            ShowResult(e, "failure");
        }
    };

    $scope.Clear = function () {
        $scope.disbtn = false;
        $scope.ModelNew = { Id: null, POId: null, EntityId: null, ProcessId: null, ResponsiblePersonId: null, CheckedById: null, WorkCenterMasterId: null, ConfirmationDate: null, Remarks: null, AddedBy: null, AddedDate: null, AddedFromIP: null, UpdatedBy: null, UpdatedDate: null, UpdatedFromIP: null };
        $scope.SOItemList = [];
        $scope.modelList = [];
        $scope.AddOtherMaterialList = [];
        $scope.IssueSlipDataList = [];

        $rootScope.toggle();
        $scope.Action = 'Save';
    }

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.tab2 = 1;
    $scope.setTab2 = function (newTab) {
        $scope.tab2 = newTab;
    };
    $scope.isSet2 = function (tabNum) {
        return $scope.tab2 === tabNum;
    };

    $scope.popUpDataList = [];

    $scope.name = null;
    $scope.showEmployeeListPopUp = function (name) {
        try {
            $scope.name = name;
            $scope.popUpDataList = [];
            $http({
                method: 'GET',
                url: 'OrderManagements/SalesOrderApproval/GetAllActiveEmployeeData'

            }).then(function successCallback(response) {
                $scope.popUpDataList = response.data;
            });

            angular.element(document.querySelector('#popUp')).modal('show');

        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.SelectEmployee = function (arg) {
        if ($scope.name == 'RP') {
            $scope.ModelNew.ResponsiblePersonId = arg.data.SystemId;
            $scope.ModelNew.ResponsiblePersonEmployeeCode = arg.data.EmployeeCode;
            $scope.ModelNew.ResponsiblePerson = arg.data.EmployeeName;
        } else {
            $scope.ModelNew.CheckedById = arg.data.SystemId;
            $scope.ModelNew.CheckedByEmployeeCode = arg.data.EmployeeCode;
            $scope.ModelNew.CheckedBy = arg.data.EmployeeName;
        }
        $scope.closePopUp();
    }

    $scope.clearEmp = function (name) {
        $scope.name = name;
        if ($scope.name == 'RP') {
            $scope.ModelNew.ResponsiblePersonId = null;
            $scope.ModelNew.ResponsiblePersonEmployeeCode = null;
            $scope.ModelNew.ResponsiblePerson = null;
        } else {
            $scope.ModelNew.CheckedById = null;
            $scope.ModelNew.CheckedByEmployeeCode = null;
            $scope.ModelNew.CheckedBy = null;
        }
    }

    $scope.closePopUp = function () {
        angular.element(document.querySelector('#popUp')).modal('hide');
    }

    $scope.summaryRows = [{
        title: "Total Qty", summaryColumns: [{ summaryType: ej.Grid.SummaryType.Sum, displayColumn: "RequestedQty", dataMember: "RequestedQty", format: "{0:N2}" }
            , { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "IssueQty", dataMember: "IssueQty", format: "{0:N2}" }
            , { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "OtherQty", dataMember: "OtherQty", format: "{0:N2}" }
            , { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "WasteQty", dataMember: "WasteQty", format: "{0:N2}" }
            , { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "UptodateOtherQty", dataMember: "UptodateOtherQty", format: "{0:N2}" }
            , { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "UptodateWasteQty", dataMember: "UptodateWasteQty", format: "{0:N2}" }
            , { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "TotalQty", dataMember: "TotalQty", format: "{0:N2}" }
        ]
        , showCaptionSummary: true

    }];

    $scope.AddOtherMaterial = function () {

        $scope.materialType = 'ProductDefinition';
        $scope.getMaterialMasterWithArticle(null);
    };

    $scope.AddOtherMaterialList = [];
    $scope.setInputeMaterialArticleData = function (obj) {
        var data = obj.data;
        var object = {};

        object.Id = null;
        object.ArticleId = data.Id;
        object.ArticleCode = data.Code;
        object.ArticleName = data.StandardName;
        object.Material = data.MaterialMasterName;
        object.MaterialMasterId = data.MaterialMasterId;
        object.Qty = 0;
        $scope.AddOtherMaterialList.push(object);
        object = {};
        angular.element(document.querySelector('#materialarticleNewPopUp')).modal('hide');
    };




}