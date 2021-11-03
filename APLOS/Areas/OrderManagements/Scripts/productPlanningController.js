'use strict';
ProductPlanningController.$inject = ["cboService", "commonMessage", "$scope", "$rootScope", "baseService", "$filter", "$window", "$http"];
function ProductPlanningController(cboService, commonMessage, $scope, $rootScope, baseService, $filter, $window, $http) {
    $rootScope.title = "Product Planning";
    $scope.Action = 'Save';
    $scope.index = -1;

    $scope.modelList = [];
    $scope.productionMaterialList = [];
    $scope.prdProcessSetList = [];
    $scope.productionEntityList = [];
    $scope.productionWorkCenterList = [];

    $scope.path = 'OrderManagements/ProductPlanning/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    
    $scope.modelFilterByList = [
        {
            value: 'EntityName'
            , name: 'Entity '
        },
        {
            value: 'ProductionStatusName'
            , name: 'Production Status'
        }
    ];

    baseService.init($scope.getListUrl, null, null, null, 'EntityName', 'EntityName');
    $scope.getData = function () {

        $http({
            method: 'GET',
            data: { 'parameters': null },
            url: $scope.getListUrl
        }).then(function successCallback(response) {
            $scope.modelList = response.data;
        });
    };
    $scope.getData();


    cboService.getCboProductionEntityByPlant(null, null, $window.plantId, function (result) {
        $scope.entityList = result;
    });

    cboService.getProductionStatusCboByGroup(function (result) {
        $scope.productionStatusList = result;
    });

    cboService.getEnumCbo('enum/getenumrequiredtimeunitcbo', function (result) {
        $scope.requiredTimeUnitList = result;
    });

    // #endregion
    //change list: data,Id,Active,GridName
    $scope.MaterialID = "";
    $scope.isAlternative = -1;
    $scope.rowDataBound = function rowDataBound(e) {

        if ($scope.MaterialID != e.data.ArticleId) {
            $scope.isAlternative = $scope.isAlternative * -1;
            $scope.MaterialID = e.data.ArticleId;
        }
        if ($scope.isAlternative > 0)
            e.row.css("background-color", '#fff6b7');
        else
            e.row.css("background-color", '#d1e5ff');


    }
    $scope.rowDataBoundOrder = function rowDataBoundOrder(e) {

        e.row.css("background-color", e.data.color);

    }
    function checkChangeSOItem(e) {

        var val = e.model.value;
        //item level check
        var row = $filter('filter')($scope.recipeMaterialList, { 'SalesOrderId': e.model.value });
        if (!baseService.isUndefinedOrNull(row) && row.length > 0) {
            if (e.model.checkState == "check")
                row[0].Checked = true;
            else
                row[0].Checked = false;
        }
        //$rootScope.genericPushInTempList(data, event, $scope.productionMaterialList, 'SalesOrderId', 'SalesOrderId');
    }
    function headCheckChangeSOItem(e) {
        if (e.model.checkState == "check") {

            // var gridObj = $("#GridSOItem").data("ejGrid");
            var filtered = $("#GridSOItem").data("ejGrid").getFilteredRecords();
            if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
                for (var i = 0; i < $scope.recipeMaterialList.length; i++) {
                    $scope.recipeMaterialList[i].Checked = true;
                }
            }
            else {
                for (var i = 0; i < $scope.recipeMaterialList.length; i++) {
                    for (var j = 0; j < filtered.length; j++) {
                        if ($scope.recipeMaterialList[i].SalesOrderId == filtered[j].SalesOrderId)
                            $scope.recipeMaterialList[i].Checked = true;
                    }

                }
            }

            var checkbox = $("#GridSOItem .rowCheckbox").ejCheckBox();
            for (var i = 0; i < checkbox.length; i++) {
                $($("#GridSOItem .rowCheckbox")[i]).ejCheckBox({ "change": null });
                $($("#GridSOItem .rowCheckbox")[i]).ejCheckBox({ "checked": true });
                $($("#GridSOItem .rowCheckbox")[i]).ejCheckBox({ "change": checkChangeSOItem });
            }
        }
        else {
            var filtered = $("#GridSOItem").data("ejGrid").getFilteredRecords();
            if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
                for (var i = 0; i < $scope.recipeMaterialList.length; i++) {
                    $scope.recipeMaterialList[i].Checked = false;
                }
            }
            else {
                for (var i = 0; i < $scope.recipeMaterialList.length; i++) {
                    for (var j = 0; j < filtered.length; j++) {
                        if ($scope.recipeMaterialList[i].SalesOrderId == filtered[j].SalesOrderId)
                            $scope.recipeMaterialList[i].Checked = false;
                    }

                }
            }
            var checkbox = $("#GridSOItem .rowCheckbox").ejCheckBox();
            for (var i = 0; i < checkbox.length; i++) {
                $($("#GridSOItem .rowCheckbox")[i]).ejCheckBox({ "change": null });
                $($("#GridSOItem .rowCheckbox")[i]).ejCheckBox({ "checked": false });
                $($("#GridSOItem .rowCheckbox")[i]).ejCheckBox({ "change": checkChangeSOItem });
            }
        }
        //header level check
    }
    $scope.dataBoundSOItem = function (args) {
        $("#GridSOItem .rowCheckbox").ejCheckBox({ "change": checkChange });
        $("#headchk").ejCheckBox({ "change": headCheckChangeSOItem });

    }

    //if the grid has scrollbars, you have to use the two functions (window.onresize,actioncomplete)
    $window.onresize = function (event) {
        $scope.actionCompleteSearch();
        $scope.actionCompleteSelected();

    }
    $scope.actionCompleteSearch = function (args) {
        try {
            var gridObj = $("#GridSOItem").ejGrid("instance");
            var scrollerwidth = $("#orderModal").width();//Obtain the width of the container
            //var scrollerheight = ($("#OuterContainer").height()) - ($(".e-gridheader").outerHeight()) - ($(".e-pager").outerHeight());//Obtain the height of the container and subtract it from gridheader and pager
            gridObj.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth - 20 } });//pass the obtainer width and height to gridmodel options
            gridObj.windowonresize();
        } catch (e) {

        }
    }
    $scope.actionCompleteSelected = function (args) {
        try {
            var gridObj = $("#GridSOItemSelected").ejGrid("instance");
            var scrollerwidth = $("#OuterContainer").width();//Obtain the width of the container
            //var scrollerheight = ($("#OuterContainer").height()) - ($(".e-gridheader").outerHeight()) - ($(".e-pager").outerHeight());//Obtain the height of the container and subtract it from gridheader and pager
            gridObj.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth - 20 } });//pass the obtainer width and height to gridmodel options
            gridObj.windowonresize();
        } catch (e) {

        }
    }


    $scope.refreshTemplateSOItem = function (args) {
        if (args.rowIndex == 0) {
            //$("#headchk").ejCheckBox({ "change": headCheckChangeSOItem });



        }

        var valobj = $($("#GridSOItem .rowCheckbox")[args.rowIndex]).ejCheckBox()[0];
        var val = $($("#GridSOItem .rowCheckbox")[args.rowIndex]).ejCheckBox()[0].defaultValue;

        $($("#GridSOItem .rowCheckbox")[args.rowIndex]).ejCheckBox({ "change": null });
        var row = $filter('filter')($scope.recipeMaterialList, { 'SalesOrderId': val });
        if (!baseService.isUndefinedOrNull(row) && row.length > 0) {
            if (row[0].Checked == true)
                $($("#GridSOItem .rowCheckbox")[args.rowIndex]).ejCheckBox({ "checked": true });
            else
                $($("#GridSOItem .rowCheckbox")[args.rowIndex]).ejCheckBox({ "checked": false });

        }
        $($("#GridSOItem .rowCheckbox")[args.rowIndex]).ejCheckBox({ "change": checkChangeSOItem });
    }


    $scope.searchByList = [
        //{
        //    'name': 'Production Status',
        //    'value': 'ProductionStatusName'
        //},
        {
            'name': 'Recipe',
            'value': 'RecipeName'
        },
        {
            'name': 'Remarks',
            'value': 'Remarks'
        }
    ];

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

    $scope.Get = function (Row) {
        //$scope.index = index;
        $scope.model = Row.data;
        //$scope.model = Object.assign({}, $scope.model);
        $scope.model = Object.assign({}, Row.data);
        getProductionRecipeMaterialList();
        getProductionProcessSetList();
        getProductionOrderEntityList();
        getProductionOrderWorkCenterList();
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    function getProductionRecipeMaterialList() {
        $http({
            method: 'GET',
            url: $scope.path + 'GetProductionRecipeMaterialList?productionOrderId=' + $scope.model.Id
        }).then(function successCallback(response) {
            $scope.recipeMaterialListSelected = response.data;
        });
    }

    $scope.menu = [];
    $scope.getMenu = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'Menu'
        }).then(function successCallback(response) {
            $scope.menu = response.data.DATA;
            for (var i = 0; i < response.data.MASTER.length; i++) {
                $scope.menu.push(response.data.MASTER[i]);
            }
            $("#treeView").ejTreeView({
                fields: { dataSource: $scope.menu, id: "id", parentId: "pid", text: "MenuText" },
                allowDragAndDrop: true,
                allowDragAndDropAcrossControl: true
            });

        });
    }
    //$scope.getMenu();


    function getProductionProcessSetList() {
        $http({
            method: 'GET',
            url: $scope.path + 'GetProductionOrderProcessSetList?productionOrderId=' + $scope.model.Id
        }).then(function successCallback(response) {
            $scope.prdProcessSetList = response.data;
        });
    }

    function getProductionOrderEntityList() {
        $http({
            method: 'GET',
            url: $scope.path + 'GetProductionOrderEntityList?productionOrderId=' + $scope.model.Id
        }).then(function successCallback(response) {
            $scope.productionEntityList = response.data;
        });
    }

    function getProductionOrderWorkCenterList() {
        $http({
            method: 'GET',
            url: $scope.path + 'GetProductionOrderWorkCenterList?productionOrderId=' + $scope.model.Id
        }).then(function successCallback(response) {
            $scope.productionWorkCenterList = response.data;
        });
    }


    $scope.Save = function () {
        try {
            daysSortValidation($scope.prdProcessSetList);
            isJobWorkType($scope.prdProcessSetList);

            var isBaseProcess = false;
            for (var i = 0; i < baseService.arrayLength($scope.prdProcessSetList); i++) {
                if ($scope.prdProcessSetList[i].IsBaseProcess) {
                    isBaseProcess = true;
                    break;
                }
                isBaseProcess = false;
            }
            if (!isBaseProcess) throw 'Please select base process';


            $scope.$broadcast('show-errors-check-validity');
            if ($scope.modelNewForm.$valid) {
                $http({
                    method: 'POST'
                    , url: $scope.saveUrl
                    , data: {
                        'master': $scope.model
                        , 'detaillist': $scope.recipeMaterialListSelected
                        , 'processSetlist': $scope.prdProcessSetList
                        , 'entitylist': $scope.productionEntityList
                        , 'workcenterlist': $scope.productionWorkCenterList
                    }
                    , dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.getData();
                        ClearFields();
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                };
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.confirmdelete = false;
    $scope.Confirm = function () {
        var eDialog = $("#dialogAPI").data("ejDialog");
        eDialog.open();
        $("#dialogAPI_wrapper").css({ 'position': 'fixed' }).css({ 'top': '200px' });
    };
    $scope.ConfirmClose = function () {
        var eDialog = $("#dialogAPI").data("ejDialog");
        eDialog.close();
    };
    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.model.Id)) {
            $http({
                method: 'POST'
                , url: $scope.deleteUrl
                , data: { 'masterid': $scope.model.Id }
                , dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.getData();
                    ClearFields();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
        else {
            ShowResult(commonMessage.primaryKeyNullMessage, 'failure');
        }
    };

    $scope.Clear = function () {
        ClearFields();
        return true;
    };

    function ClearFields() {
        $scope.Action = "Save";
        $scope.model = {};
        $scope.model = { PlantId: $window.plantid };
        $scope.productionMaterialList = [];
        $scope.prdProcessSetList = [];
        $scope.productionEntityList = [];
        $scope.productionWorkCenterList = [];
        $scope.recipeMaterialListSelected = [];
        try {
            var gridObj = $("#GridSOItem").ejGrid("instance");
            gridObj.refreshContent(true);
        } catch (e) {

        }
     

        $scope.model.color = "#ffffff";
    }
    $scope.Clear();

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

   
}