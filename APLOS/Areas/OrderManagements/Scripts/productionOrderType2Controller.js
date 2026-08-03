'use strict';
productionOrderType2Controller.$inject = ["cboService", "commonMessage", "$scope", "$rootScope", "baseService", "$filter", "$window", "$http", "$controller"];
function productionOrderType2Controller(cboService, commonMessage, $scope, $rootScope, baseService, $filter, $window, $http, $controller) {
    $rootScope.title = "Production Order";
    $scope.Action = 'Save';
    $scope.index = -1;
    $rootScope.CurrentScope = $scope;
    $scope.processList = [];
    $scope.operationList = [];
    $scope.modelList = [];
    $scope.productionMaterialList = [];
    $scope.prdProcessSetList = [];
    $scope.productionEntityList = [];
    $scope.productionWorkCenterList = [];
    $scope.productionFPWorkCenterList = [];
    $scope.bulletintab = true;

    $scope.path = 'OrderManagements/ProductionOrder/';
    $scope.getListUrl = $scope.path + 'GetType2List';
    //$scope.saveUrl = $scope.path + 'CreateProductionOrderType2';
    $scope.saveUrl = $scope.path + 'Create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'DeleteType2/';

    $controller('baseMaterialAndArticleController', { $scope: $scope, $http: $http });

    $scope.GetProductionHistory = function (Id) {

        try {
            var file_src = $scope.path + 'GetProductionHistory?ProductionOrderId=' + Id
            $rootScope.report(file_src);

        } catch (e) {

        }
    }

    $scope.rowDataBoundOrder = function rowDataBoundOrder(e) {
        try {
            if (e.data.Owner == 'OWN') {
                e.row.css("background-color", "#E6F0FF");
            }
            else if (e.data.Owner == 'IN') {
                e.row.css("background-color", "#FF957F");
            }
            else if (e.data.Owner == 'OUT') {
                e.row.css("background-color", "#FFF97F");
            }
        } catch (e) {

        }

    }

    $scope.onBeginUpload = function (args) {
        try {
            if (angular.isUndefinedOrNull($scope.model.Id))
                throw 'Please select/save the production order first'

            args.data = $scope.model.Id;
        } catch (e) {

            args.cancel = true;
            ShowResult(e, 'Error');
        }

    }
    $scope.uploadUrl = "OrderManagements/ProductionOrder/SaveDefault";
    $scope.fileselect = function (e) {

    }
    $scope.errorPicUpload = function (e) {
        if (angular.isUndefinedOrNull($scope.model.Id))
            ShowResult('Please select/save the production order first', 'Error');
        else
            ShowResult("The selected file size is too large. Please select a file less than " + Math.round(e.model.fileSize / (1024 * 1024)) + "MB", 'failure');
    }


    $scope.modelFilterByList = [
        {
            value: 'Id'
            , name: 'Id '
        },
        {
            value: 'Remarks'
            , name: 'Remarks '
        },
        {
            value: 'EntityName'
            , name: 'Entity '
        },
        {
            value: 'ProductionStatusName'
            , name: 'Production Status'
        }
    ];

    //baseService.init($scope.getListUrl, null, null, null, 'EntityName', 'EntityName');
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

    $scope.getData = function () {
        $scope.modelList = [];
        $http({
            method: 'GET',
            data: { 'parameters': null },
            url: $scope.getListUrl + "?column=" + $scope.PRsearchBy + "&value=" + $scope.PRsearch
        }).then(function successCallback(response) {
            $scope.modelList = response.data;
        });
    };
    $scope.getData();

    $scope.moentityList = [];
    $http({
        method: 'GET',
        url: 'OrderManagements/ProductionOrder/GetMasterOrderEntityCbo'
    }).then(function successCallback(response) {
        $scope.moentityList = response.data;
    });

    $scope.GetPlanningTypeEntiy = function () {
        $http({
            method: 'GET',
            url: 'OrderManagements/ProductionOrder/GetPlanningType2EntityCbo?processId=' + $scope.model.PlanningTypeProcessId
        }).then(function successCallback(response) {
            $scope.entityList = response.data;
        });
    }

    $scope.planningTypeProcessList = [];
    $scope.GetPlanningTypeProcess = function () {
        $http({
            method: 'GET',
            url: 'OrderManagements/ProductionOrder/GetPlanningType2ProcessCbo'
        }).then(function successCallback(response) {
            $scope.planningTypeProcessList = response.data;
        });
    }
    $scope.GetPlanningTypeProcess();

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
    //$scope.rowDataBound = function rowDataBound(e) {

    //    if ($scope.MaterialID != e.data.ProductionGrouping + e.data.MaterialMasterId) {
    //        $scope.isAlternative = $scope.isAlternative * -1;
    //        $scope.MaterialID = e.data.ProductionGrouping + e.data.MaterialMasterId;
    //    }
    //    if ($scope.isAlternative > 0)
    //        e.row.css("background-color", '#fff6b7');
    //    else
    //        e.row.css("background-color", '#d1e5ff');


    //}


    $scope.rowDataBound = function rowDataBound(e) {

        if ($scope.MaterialID != e.data.ProductionGrouping + e.data.MaterialMasterId) {
            $scope.isAlternative = $scope.isAlternative * -1;
            $scope.MaterialID = e.data.ProductionGrouping + e.data.MaterialMasterId;
        }
        if ($scope.isAlternative > 0)
            e.row.css("background-color", "#90EE90");
        else
            e.row.css("background-color", '##013220');


    }

    //$scope.rowDataBound = function rowDataBound(e) {
    //    if (angular.isUndefinedOrNull($scope.recipeMaterialList) == false) {
    //        for (var i = 0; i < $scope.recipeMaterialList.length; i++) {
    //            if ($scope.recipeMaterialList[i].ProductionGrouping == e.data.ProductionGrouping) {
    //                e.row.css("background-color", "#90EE90");
    //            }
    //            else {
    //                e.row.css("background-color", '##013220');
    //            }

    //        }
    //    }
    //}


    //$scope.rowDataBoundOrder = function rowDataBoundOrder(e) {

    //    e.row.css("background-color", e.data.color);

    //}
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
    $window.onload = function (event) {
        $scope.actionCompleteSearch();
        $scope.actionCompleteSelected();

    }
    $window.onresize = function (event) {
        $scope.actionCompleteSearch();
        $scope.actionCompleteSelected();

    }
    $scope.actionCompleteSearch = function (args) {
        try {
            if (args.requestType == "refresh") {
                var gridObj = $("#GridSOItem").ejGrid("instance");
                var scrollerwidth = $("#orderModal").width();//Obtain the width of the container
                //var scrollerheight = ($("#OuterContainer").height()) - ($(".e-gridheader").outerHeight()) - ($(".e-pager").outerHeight());//Obtain the height of the container and subtract it from gridheader and pager
                gridObj.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth - 20 } });//pass the obtainer width and height to gridmodel options
                gridObj.windowonresize();
            }
        } catch (e) {

        }
    }
    $scope.actionCompleteSelected = function (args) {
        try {
            if (args.requestType == "refresh") {
                var gridObj = $("#GridSOItemSelected").ejGrid("instance");
                var scrollerwidth = $("#OuterContainer").width();//Obtain the width of the container

                //args.requestType: "filtering"
                //var filtereddata = gridObj.getFilteredRecords();
                //var scrollerheight = ($("#OuterContainer").height()) - ($(".e-gridheader").outerHeight()) - ($(".e-pager").outerHeight());//Obtain the height of the container and subtract it from gridheader and pager
                gridObj.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth - 20, height: 300 } });//pass the obtainer width and height to gridmodel options
                gridObj.windowonresize();
            }
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
        , PlantId: $window.plantId
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
        , IsPreDefineLotApplicable: false
        , IsWorkCenterValidateApplicable: true
        , UserDefineLotNo: null
        , UsedInPB: false
        , PlanningTypeProcessId: null
        , WCPreferenceType: 'INCLUDE'
    };
    $scope.model = Object.assign({}, $scope.model);

    $scope.DisableActionButtons = false;
    $scope.pbookingmessage = null;
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

        $scope.DisableActionButtons = false;
        $scope.operationList = [];
        $scope.lotControlList = [];
        $scope.model = Row.data;
        //$scope.model = Object.assign({}, $scope.model);
        $scope.model = Object.assign({}, Row.data);
        $scope.GetPlanningTypeEntiy();
        if (baseService.isUndefinedOrNull($scope.model.UserDefineLotNo)) {
            $scope.model.UserDefineLotNo = $scope.model.Id;
        }

        getProductionRecipeMaterialList();

        //$scope.GetBulletinTamplate2ndIndexReport(Row.data.Id);

        $scope.GetProductionOrderPopUp();
        if ($scope.model.UsedInPB) {
            $scope.pbookingmessage = "Lot generation is not possible as Production is booked with this Production Order.";
        }

        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };
    $scope.Load = function (Row) {
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
        $scope.model = Object.assign({}, Row);
        getProductionRecipeMaterialList();
        getProductionProcessSetList();
        getProductionOrderEntityList();
        getProductionOrderWorkCenterList();
        $scope.Action = 'Update';

    };

    function getProductionRecipeMaterialList() {
        $http({
            method: 'GET',
            url: $scope.path + 'GetProductionOrderType2MaterialList?productionOrderId=' + $scope.model.Id
        }).then(function successCallback(response) {
            $scope.recipeMaterialListSelected = response.data;
            getProductionProcessSetList();
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

    $scope.productionBookingLevelList = [];
    cboService.getEnumCbo("enum/GetEnumProductionBookingLevelCbo", function (result) {
        $scope.productionBookingLevelList = result;
    });

    function getProductionProcessSetList() {
        $http({
            method: 'GET',
            url: $scope.path + 'GetProductionOrderProcessSetList?productionOrderId=' + $scope.model.Id
        }).then(function successCallback(response) {
            $scope.prdProcessSetList = response.data;
            for (var i = 0; i < $scope.prdProcessSetList.length; i++) {
                if ($scope.prdProcessSetList[i].Sequence == 1) {
                    $scope.processId = $scope.prdProcessSetList[i].ProcessId;
                    $scope.prdProcessSetList[i].IsProductionVerification = true;
                }
                UomCboByFGMaterialMaster($scope.prdProcessSetList[i].MaterialMasterId);
            }

            getProductionOrderWorkCenterList();

        });
    }

    function getProductionOrderEntityList() {
        $http({
            method: 'GET',
            url: $scope.path + 'GetProductionOrderEntityList?productionOrderId=' + $scope.model.Id
        }).then(function successCallback(response) {
            $scope.productionEntityList = response.data;
            getProductionOrderWorkCenterList();

        });
    }

    function getProductionOrderWorkCenterList() {
        $http({
            method: 'GET',
            url: $scope.path + 'GetProductionOrderType2WorkCenterList?productionOrderId=' + $scope.model.Id
        }).then(function successCallback(response) {
            $scope.productionWorkCenterList = response.data;
            // $scope.getProductionBulletinData($scope.model.Id);
            $scope.GetSavedWorkCenterListByEntityandFirstProcess();
        });
    }

    $scope.btndisable = false;
    $scope.Save = function () {
        try {
            $scope.btndisable = true;

            if (baseService.isUndefinedOrNull($scope.recipeMaterialListSelected) || $scope.recipeMaterialListSelected.length <= 0) {
                $scope.btndisable = false;
                throw 'Please select at least one material';
            }
            var getRow = $filter("filter")($scope.prdProcessSetList, { "IsInventory": true });
            if (getRow.length == 0) {
                $scope.btndisable = false;
                throw "Please select IsInventory for Process Set.";
            }

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
            if (!isBaseProcess) {
                $scope.btndisable = false;
                throw 'Please select base process';
            }

            $scope.$broadcast('show-errors-check-validity');
            if ($scope.modelNewForm.$valid) {
                var totalQty = 0;
                for (var i = 0; i < $scope.recipeMaterialListSelected.length; i++)
                    totalQty += $scope.recipeMaterialListSelected[i].Qty;

                $scope.model.Qty = totalQty;

                $http({
                    method: 'POST'
                    , url: $scope.saveUrl
                    , data: {
                        'data': $scope.model
                        , 'detaillist': $scope.recipeMaterialListSelected
                        , 'processSetlist': $scope.prdProcessSetList
                        //, 'entitylist': $scope.productionEntityList
                        , 'workcenterlist': $scope.productionWorkCenterList
                        , 'fpworkcenterlist': $scope.productionFPWorkCenterList
                        //, 'UploadDefault': push
                    }
                    , dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        $scope.btndisable = false;
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.model.Id = response.data.DATA;
                        $scope.getData();
                        getProductionRecipeMaterialList();
                        $scope.GetProductionOrderPopUp();
                        $scope.btndisable = false;
                        $scope.Action = "Update"
                        //var uploadObj = $("#UploadDefault").data("ejUploadbox");
                        //uploadObj.element.find('.e-uploadinput').click();

                        //ClearFields();
                    }
                }), function errorCallBack(response) {
                    $scope.btndisable = false;
                    ShowResult(response.data.Message, 'failure');
                };
            }
            else {
                $scope.btndisable = false;
            }
        } catch (e) {
            $scope.btndisable = false;
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
        $scope.DisableActionButtons = false;
        $scope.btndisable = false;
        $scope.Action = "Save";
        $scope.model = {
            Id: null
            , RecipeId: null
            , PlantId: $window.plantId
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
            , IsPreDefineLotApplicable: false
            , IsWorkCenterValidateApplicable: true
            , UserDefineLotNo: null
            , UsedInPB: false
            , PlanningTypeProcessId: null
            , WCPreferenceType: 'INCLUDE'
        };
        $scope.model = Object.assign({}, $scope.model);
        $scope.processList = [];
        $scope.productionMaterialList = [];
        $scope.prdProcessSetList = [];
        $scope.productionEntityList = [];
        $scope.productionWorkCenterList = [];
        $scope.recipeMaterialListSelected = [];
        $scope.productionWorkCenterList = [];
        try {
            var gridObj = $("#GridSOItem").ejGrid("instance");
            gridObj.refreshContent(true);


        } catch (e) {

        }
        $scope.bulletintab = true;
        $scope.bulletinTemplate = {};
        $scope.bulletinTemplateNew = {};
        $scope.buyerList = [];
        $scope.operationList = [];
        $scope.processCountList = [];
        $scope.processPitchList = [];
        $scope.TotalSPT = null;
        $scope.TotalWorkStation = null;
        $scope.TotalManpower = null;
        $scope.model.color = "#ffffff";
        $scope.bulletintab = true;
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
        $scope.PicFileName = virtualPath.ProductionBulletinImage + '';
        $scope.productionFPWorkCenterList = [];
        $scope.lotControlList = [];
        $scope.pbookingmessage = null;
    }
    $scope.Clear();

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
        //$("#recipeMaterialPopUp").ejDialog("setTitle", "Sales Order");
        //var eDialog = $("#recipeMaterialPopUp").data("ejDialog");
        //eDialog.open();

        //var gridObj = $("#recipeMaterialPopUp").data("ejGrid");
        //gridObj.clearFiltering(); 
        $scope.serachSoMaterial();

    };

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
            url: $scope.path + 'GetSalesOrderListSearch?column=' + $scope.recipeMaterialParameters.searchBy + '&value=' + $scope.recipeMaterialParameters.search + "&productionorderid=" + $scope.model.Id + "&EntityId=" + $scope.model.EntityId + "&ProcessId=" + $scope.model.PlanningTypeProcessId + "&moentity=" + $scope.MOEntityId
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
        //if (event.currentTarget.checked) {
        //    var flag = true;
        //    for (var i = 0; i < baseService.arrayLength($rootScope.tempList); i++) {
        //        flag = $rootScope.tempList[i].RecipeGlobalMasterId === data.RecipeGlobalMasterId ? true : false;
        //    }
        //    if (!flag) {
        //        $scope.recipeMaterialList[index].Flag = false;
        //        return ShowResult('Recipe not matched.', 'failure', 'recipeMaterialPopUp');
        //    }
        //}
        $rootScope.genericPushInTempList(data, event, $scope.productionMaterialList, 'SalesOrderId', 'SalesOrderId');
    };

    $scope.CloseRecipeMaterialPopUp = function () {
        angular.element(document.querySelector('#recipeMaterialPopUp')).modal('hide');
    };

    // #endregion Recipe Material and SO

    // #region Process Set

    $scope.processPetParameters = {
        limit: 10
        , offset: 0
        , order: 'asc'
        , sort: 'Entity,ProcessCategory,ProcessCriteria,Code,Description'
        , searchBy: "Code"
        , pageSize: 10
        , total_count: 0
        , search: null
        , serverPagination: true
    };
    $scope.processSetPopUp = function () {
        if (baseService.isUndefinedOrNull($scope.model.RequiredTimeUnit))
            return ShowResult('Please at first select required time unit.', 'failure');
        $scope.popUpList = [];
        $scope.popUpUrl = 'Processes/ProcessSet/GetListByCompany';
        baseService.setCurrentPage('dataList');
        $scope.processPetParameters.companyId = $window.companyId;
        $scope.processPetParameters.entityId = $scope.model.EntityId;
        $scope.getProcessSetList = function (pageno) {
            baseService.paginationBase($scope.popUpUrl, pageno, $scope.processPetParameters)
                .then(function (result) {
                    $scope.processSetList = result.Rows;
                    $scope.processPetParameters.total_count = result.Total;
                    if (baseService.arrayLength($scope.popUpList) === 0) {
                        baseService.getDDLSearchColumn(result.Rows, $scope.popUpList);
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure', 'processSetPopUp');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#processSetPopUp')).modal('show');
        $scope.getProcessSetList();
    };

    $scope.selectProcessSet = function (data) {
        getProcessSetList(data.Id);
        angular.element(document.querySelector('#processSetPopUp')).modal('hide');
    };

    function getProcessSetList(id) {
        $scope.prdProcessSetList = [];
        $http({
            method: 'GET',
            url: 'Processes/processset/GetProcessSetList?processSetId=' + id + '&entityId=' + $scope.model.EntityId
        }).then(function successCallback(response) {
            for (var i = 0; i < response.data.length; i++) {
                if (response.data[i].Days < 0)
                    response.data[i].Days = response.data[i].Days * -1;
                response.data[i].Id = null;
            }
            $scope.prdProcessSetList = response.data;
            for (var i = 0; i < $scope.prdProcessSetList.length; i++) {
                UomCboByFGMaterialMaster($scope.prdProcessSetList[i].MaterialMasterId);
                if ($scope.prdProcessSetList[i].Sequence == 1) {
                    $scope.prdProcessSetList[i].IsProductionVerification = true;
                }
            }


        });
    }

    // #endregion

    // #region Process

    $scope.processSearchList = [
        {
            'name': 'Sequence',
            'value': 'Sequence'
        },
        {
            'name': 'Code',
            'value': 'Code'
        },
        {
            'name': 'User Name',
            'value': 'UserName'
        },
        {
            'name': 'Local Name',
            'value': 'LocalName'
        },
        {
            'name': 'Alias',
            'value': 'Alias'
        }
    ];
    $scope.processPopUpParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'Sequence',
        searchBy: "UserName",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.processPopUp = function () {
        if (baseService.isUndefinedOrNull($window.companyId))
            return ShowResult('Please at first select company.', 'failure');
        if (baseService.isUndefinedOrNull($scope.model.RequiredTimeUnit))
            return ShowResult('Please at first select required time unit.', 'failure');


        $scope.popUpProcessUrl = 'Processes/Process/GetProductionProcessList?productionOrderId=' + $scope.model.Id + '&EntityId=' + $scope.model.EntityId;
        $scope.getProcessData = function (pageno) {
            baseService.paginationBase($scope.popUpProcessUrl, pageno, $scope.processPopUpParameters)
                .then(function (result) {
                    $scope.processPopUpDataList = result.Rows;
                    $scope.processPopUpParameters.total_count = result.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure', 'processPopUp');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#processPopUp')).modal('show');
        $scope.getProcessData();
    };

    $scope.closeProcessPopUp = function () {
        angular.element(document.querySelector('#processPopUp')).modal('hide');
    };

    $scope.processDelModal = function (data, index) {
        $scope.processIndex = index;
        $scope.processMessage = 'Are you sure want to permanently delete [ ' + data.ProcessName + ' ]?';
        angular.element(document.querySelector('#confirmProcessPopUp')).modal('show');
    };

    $scope.removeProcessRow = function () {
        $scope.prdProcessSetList.splice($scope.processIndex, 1);
        $scope.processIndex = -1;
    };

    $scope.processAdd = function (data) {

        if (checkExistProcessId($scope.prdProcessSetList, data.Id) === false) {
            $scope.prdProcessSetList.push({
                Id: null
                , CompanyGroupId: $window.companyGroupId
                , CompanyId: $window.companyId
                , ProductionOrderId: $scope.model.Id
                , ProcessId: data.Id
                , ProcessName: data.UserName
                , Sequence: $scope.prdProcessSetList.length + 1
                , IsBaseProcess: false
                , Days: 0
                , Symbol: '+'
                , ProductionCycleTime: 1
                , JobWorkApplicable: false
                , JobWorkType: null
                , EntityOrVendorId: null
                , EntityOrVendorName: null
                , Archive: false
                , class: 'new'
                , setDisable: true
                , MaterialMasterId: null
                , ArticleId: null
                , MaterialMasterName: null
                , ArticleName: null
                , Qty: 100
                , UOMId: null
                , RelaySequence: data.RelaySequence
                , ProductionBookingLevel: data.ProductionBookingLevel
                , IsInventory: data.IsInventory
                , IsProductionVerification: false
            });
            UomCboByFGMaterialMaster(data.MaterialMasterId);
        }
    };

    function checkExistProcessId(list, Id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].ProcessId === Id) {
                return true;
            }
        }
        return false;
    }

    $scope.setPlusOrMinus = function (event, index) {
        for (var i = 0; i <= $scope.prdProcessSetList.length - 1; i++) {
            if (i < index) {
                $scope.prdProcessSetList[i].Symbol = '-';
                $scope.prdProcessSetList[i].IsBaseProcess = false;
            }
            else if (i > index) {
                $scope.prdProcessSetList[i].Symbol = '+';
                $scope.prdProcessSetList[i].IsBaseProcess = false;
            }
            else if (i === index) {
                $scope.prdProcessSetList[i].Symbol = null;
                $scope.prdProcessSetList[i].Days = 0;
                $scope.prdProcessSetList[i].IsBaseProcess = true;
            }
        }
    };

    $scope.setIsInventory = function (event, index) {
        for (var i = 0; i <= $scope.prdProcessSetList.length - 1; i++) {
            if (i < index) {
                $scope.prdProcessSetList[i].IsInventory = false;
            }
            else if (i > index) {
                $scope.prdProcessSetList[i].IsInventory = false;
            }
            else if (i === index) {
                $scope.prdProcessSetList[i].IsInventory = true;
            }
        }
    };

    $scope.setIsProductionVerification = function (event, index) {
        for (var i = 0; i <= $scope.prdProcessSetList.length - 1; i++) {
            if (i < index) {
                $scope.prdProcessSetList[i].IsProductionVerification = false;
            }
            else if (i > index) {
                $scope.prdProcessSetList[i].IsProductionVerification = false;
            }
            else if (i === index) {
                $scope.prdProcessSetList[i].IsProductionVerification = true;

            }
        }
    };

    $scope.clearEntityOrVendor = function (list, index) {
        list[index].EntityIdWithinCompany = null;
        list[index].EntityIdWithinGroup = null;
        list[index].PartyId = null;
        list[index].EntityOrVendorName = null;
    };

    $scope.clearJobType = function (list, index) {
        list[index].JobWorkType = null;
    };

    $scope.SetDisable = function (id) {
        for (var i = 0; i < $scope.prdProcessSetList.length; i++) {
            if ($scope.prdProcessSetList[i].Id === id) {
                if ($scope.prdProcessSetList[i].JobWorkApplicable)
                    return $scope.prdProcessSetList[i].setDisable = false;
                else
                    return $scope.prdProcessSetList[i].setDisable = true;
            }
        }
    };
    angular.isUndefinedOrNull = function (val) {
        return angular.isUndefined(val) || val === null || val === ""
    }
    function daysSortValidation(list) {
        try {
            if (angular.isUndefinedOrNull(list) == true || list.length == 0)
                throw "Please select process";

            var seq = 0;
            var seqNeg = 0;
            var isNeg = true;
            if (list[0].Days === 0) {
                isNeg = false;
            } else {
                seqNeg = parseInt(list[0].Days);
                seqNeg += 1;
            }
            for (var i = 0; i < list.length; i++) {
                if (isNeg === false) {//0,1,2
                    if (list[i].Days >= seq) {
                        seq = list[i].Days;
                    }
                    else//0,1,3,2
                        throw "Lag days sequence is not valid.....!";
                }
                else //2,1,0,1,2 or2,1,0
                {
                    if (list[i].Days <= seqNeg) {//2,1,0
                        seqNeg = list[i].Days;
                        if (list[i].Days === 0) {
                            isNeg = false;
                            seq = 0;
                        }
                    }
                    else {
                        //2,3,1,0,1,2
                        throw "Lag days sequence is not valid.....!";
                    }
                }
            }
        } catch (e) {
            throw e;
        }
    }
    function isJobWorkType(list) {
        try {
            for (var i = 0; i < list.length; i++) {
                if (list[i].JobWorkApplicable && list[i].JobWorkType === null
                ) {
                    throw 'Please select job work type!';
                }
            }
        } catch (e) {
            throw e;
        }
    }

    function getProductionType2ProcessSetList() {
        $http({
            method: 'GET',
            url: $scope.path + 'GetProductionOrderProcessSetList?productionOrderId=' + $scope.model.Id
        }).then(function successCallback(response) {
            $scope.prdProcessSetList = response.data;
        });
    }



    $scope.message_confirmation = null;
    $scope.valuePassInDelModal = function (data, index) {
        $scope.index = index;
        $scope.processobj = data;
        $scope.message_confirmation = 'Are you sure want to delete [ ' + data.ProcessName + ' ]';
        angular.element(document.querySelector('#confirmDelPopUp')).modal('show');
    };
    $scope.processSetRemoveRow = function () {
        if (!baseService.isUndefinedOrNull($scope.processobj.Id)) {
            $http({
                method: 'POST',
                url: 'OrderManagements/ProductionOrder/DeleteType2Process?id=' + $scope.processobj.Id
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    getProductionType2ProcessSetList();
                }
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
        }
        else {
            $scope.prdProcessSetList.splice($scope.index, 1);
            $scope.index = -1;
        }
    };



    $scope.businessProcesses = "BOM";
    //$scope.materialType = 'ProductDefinition';
    $scope.materialType = null;

    $scope.getMaterial = function (index) {
        $scope.itemIndex = index;
        //$scope.getMaterialMasterbyTypePopUp();
        $scope.getMaterialMasterSearchData();
    };

    //$scope.selectMaterialByType = function (ob) {
    $scope.setMaterialMasterData = function (ob) {
        $scope.prdProcessSetList[$scope.itemIndex].MaterialMasterId = ob.Id;
        $scope.prdProcessSetList[$scope.itemIndex].MaterialMasterName = ob.UserName;
        $scope.prdProcessSetList[$scope.itemIndex].ArticleId = null;
        $scope.prdProcessSetList[$scope.itemIndex].ArticleName = null;
        $scope.prdProcessSetList[$scope.itemIndex].HasAttribute = ob.HasAttribute;
        $scope.mmChangeFlag = true;
        if ($scope.prdProcessSetList[$scope.itemIndex].HasAttribute) {
            $scope.getArticleSearchList(ob.Id);
        } else {
            $scope.closeMaterialMasterSearchPopUp();
            return ShowResult('This material has no attribute', 'failure');
        }
        // getTaxCategoryList(ob.HSNCodeId);
        $scope.HSNCodeId = ob.HSNCodeId;
        UomCboByFGMaterialMaster(ob.Id);
        $scope.closeMaterialMasterSearchPopUp();
    };

    $scope.getArticle = function (index) {
        $scope.itemIndex = index;
        if (!baseService.isUndefinedOrNull($scope.prdProcessSetList[$scope.itemIndex].MaterialMasterId) && !$scope.prdProcessSetList[$scope.itemIndex].HasAttribute)
            return ShowResult('This material has no attribute', 'failure');
        $scope.getArticleSearchList($scope.prdProcessSetList[$scope.itemIndex].MaterialMasterId);
    };

    $scope.selectarticle = function (ob) {
        try {
            $scope.prdProcessSetList[$scope.itemIndex].MaterialMasterId = ob.MaterialMasterId;
            $scope.prdProcessSetList[$scope.itemIndex].MaterialMasterName = ob.MaterialMasterName;
            $scope.prdProcessSetList[$scope.itemIndex].ArticleId = ob.Id;
            $scope.prdProcessSetList[$scope.itemIndex].ArticleName = ob.StandardName;
            angular.element(document.querySelector('#articleSearchPop')).modal('hide');
            $scope.itemIndex = -1;
            $scope.mmChangeFlag = true;
        } catch (e) {
            ShowResult(e, '', 'articleSearchPop');
        }
    };

    $scope.clearArticle = function (index) {
        $scope.prdProcessSetList[index].ArticleId = null;
        $scope.prdProcessSetList[index].ArticleName = null;
    };


    //$scope.uOMList = [];
    //cboService.getUoMCbo(function (response) {
    //    $scope.uOMList = response;
    //});

    $scope.uOMList = [];
    function UomCboByFGMaterialMaster(materilaMasterId) {
        var mmId = []; mmId.push(materilaMasterId);
        cboService.getUomCboByMaterialMaster(JSON.stringify(mmId), function (response) {
            if (baseService.arrayLength(response) > 0) {
                angular.forEach(response, function (item, i) {
                    // if (checkExistList($scope.uOMList, item.Value) === false) {
                    $scope.uOMList.push(item);
                    // }
                    if (!baseService.isUndefinedOrNull($scope.itemIndex)) {
                        $scope.prdProcessSetList[$scope.itemIndex].UOMId = item.Value;
                    }
                });
            }
        });
    }
    function checkExistList(list, Id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].Value == Id) {
                return true;
            }
        }
        return false;
    }

    // #endregion

    //#region Job Work Type

    $scope.popUpTitle = '';
    $scope.valueData = '';
    $scope.popUp = function (index) {
        $scope.popUpList = [];
        $scope.popUpDataList = [];
        $scope.popUpParameters = {
            limit: 10
            , offset: 0
            , order: 'asc'
            , sort: 'Name'
            , searchBy: "Name"
            , pageSize: 10
            , total_count: 0
            , search: null
            , serverPagination: true
        };
        if (isJobWorkApplicable($scope.prdProcessSetList, index))
            return ShowResult('Please select at first job work type!', 'failure');
        $scope.popUpUrl = typeCheckAndCreateUrl($scope.prdProcessSetList, index);
        $scope.getPopUpData = function (pageno) {
            baseService.paginationBase($scope.popUpUrl, pageno, $scope.popUpParameters)
                .then(function (result) {
                    $scope.popUpDataList = result.Rows;
                    $scope.popUpParameters.total_count = result.Total;
                    if (baseService.arrayLength($scope.popUpList) === 0) {
                        baseService.getDDLSearchColumn(result.Rows, $scope.popUpList);
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure', 'popUpId');
                }).finally(function () {
                });
        };

        $scope.index = index;

        var jobWorkType = $scope.prdProcessSetList[index].JobWorkType;

        if (jobWorkType == "Party") {
            $scope.ShowCustomerPopUpNew();
        } else {
            angular.element(document.querySelector('#popUpId')).modal('show');
            $scope.getPopUpData();
        }
    };

    $scope.selectDoubleClick = function (data) {
        valueSetInGrid($scope.prdProcessSetList, data, $scope.index);
        $scope.index = -1;
        $scope.closePopUp();
    };

    $scope.selectSingleClick = function (data) {
        $scope.valueData = data;
    };

    $scope.selectByButton = function () {
        if (baseService.isUndefinedOrNull($scope.valueData)) {
            return ShowResult('Please at first select row', 'failure', 'popUpId');
        }
        $scope.selectDoubleClick($scope.valueData);
        $scope.closePopUp();
    };

    $scope.closePopUp = function () {
        $scope.valueData = '';
        angular.element(document.querySelector('#popUpId')).modal('hide');
    };

    $scope.searchByParty = "UserName"; $scope.searchParty = "";
    $scope.partyList = [];
    $scope.ShowCustomerPopUpNew = function () {

        $scope.partyType = "Vendor";
        $scope.searchByPartyList = [{ value: 'Code', name: "Code" }, { value: 'UserName', name: $scope.partyType }, { value: 'PartyAccountGroupName', name: "Account Group" }, { value: 'CurrencyCode', name: "Currency" }, { value: 'CountryName', name: "Country" }, { value: 'StateName', name: "State" }];


        $scope.partyUrl = 'Parties/party/GetCompanyPartyDataSearch?partyType=' + $scope.partyType + '&CompanyId=' + $window.companyId + '&PlantId=' + $window.plantId;

        $http({
            method: 'POST',
            url: $scope.partyUrl,
            data: { column: $scope.searchByParty, value: $scope.searchParty },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.partyList = response.data;
        });
        angular.element(document.querySelector('#CustomerPopUpNew')).modal('show');
    };

    $scope.SetCustomerData = function (obj) {
        var party = obj.data;
        valueSetInGrid($scope.prdProcessSetList, party, $scope.index);
        angular.element(document.querySelector('#CustomerPopUpNew')).modal('hide');
        $scope.searchParty = '';
    }

    $scope.closeCustomerPopUpNew = function () {
        angular.element(document.querySelector('#CustomerPopUpNew')).modal('hide');
        $scope.hidePartyPopUp();
        $scope.partyType = "Vendor";
        $scope.searchParty = '';
    }



    function typeCheckAndCreateUrl(list, index) {
        if (list[index].JobWorkType === 'EntityWithinCompany') {
            $scope.popUpTitle = 'Entity within company';
            return 'Organizations/entity/withincompany?companyId=' + $window.companyId + '&entityId=' + $scope.model.EntityId;
        }
        else if (list[index].JobWorkType === 'EntityWithinGroup') {
            $scope.popUpTitle = 'Entity in group';
            return 'Organizations/entity/withingroup?companyGroupId=' + $window.companyGroupId + '&companyId=' + $window.companyId + '&entityId=' + $scope.model.EntityId;
        }
        else {
            $scope.popUpParameters.sort = 'PartyName';
            $scope.popUpParameters.searchBy = 'PartyName';
            $scope.popUpTitle = 'Vendor';
            //return 'Parties/vendorcompanydata/getpartyfromvendor?companyGroupId=' + $window.companyGroupId + '&companyId=' + $window.companyId;
            return 'Parties/party/GetCompanyPartyDataList?partyType=vendor';
        }
    }

    function isJobWorkApplicable(list, index) {
        if (baseService.isUndefinedOrNull(list[index].JobWorkType))
            return true;
        else
            return false;
    }
    function valueSetInGrid(list, data, index) {
        $scope.clearEntityOrVendor(list, index);
        if (list[index].JobWorkType === 'EntityWithinCompany') {
            list[index].EntityIdWithinCompany = data.Id;
            list[index].EntityOrVendorName = data.Name;
        }
        else if (list[index].JobWorkType === 'EntityWithinGroup') {
            list[index].EntityIdWithinGroup = data.Id;
            list[index].EntityOrVendorName = data.Name;
        }
        else {
            list[index].PartyId = data.Id;
            list[index].EntityOrVendorName = data.PartyName;
        }
    }

    //#endregion Job Work Type

    //#region Job Work PopUp

    cboService.getEnumCbo("enum/GetJobWorkTypeListCbo", function (result) {
        $scope.jobWorkTypeList = result;
    });

    //#endregion

    // #region Entity

    $scope.EntityId = null;

    $scope.addEntityPopUp = function () {
        if (baseService.isUndefinedOrNull($scope.EntityId)) return ShowResult('Production Entity is required.', 'failure');
        for (var i = 0; i < baseService.arrayLength($scope.productionEntityList); i++) {
            if (baseService.valueCheckInList($scope.productionEntityList, 'EntityId', $scope.EntityId))
                return ShowResult('This entity already taken.', 'failure');
        }
        $scope.productionEntityList.push({
            Id: null
            , ProductionOrderId: $scope.model.Id
            , EntityId: $scope.EntityId
            , EntityName: angular.element("#entityId :selected").text()
        });
    };

    // #endregion Entity

    // #region  Work Center

    $scope.workCenterList = [];
    $scope.workcenterfor = '';
    //$scope.workcenterDialog = $("#workCenterPopUp").ejDialog({ target: "#entrycontainer" });
    $scope.workCenterPopUp = function (wcfor) {
        $scope.workcenterfor = wcfor;
        $rootScope.tempList = [];
        $scope.workCenterList = [];

        if (wcfor == 'RUNNING') {
            angular.forEach($scope.runningWorkCenterList, function (a) {
                $rootScope.tempList.push({
                    Id: a.Id
                    , Plant: a.Plant
                    , Entity: a.Entity
                    , WorkCenterMasterId: a.WorkCenterMasterId
                    , ProductionOrderId: a.ProductionOrderId
                    , Code: a.Code
                    , UserName: a.UserName
                    , Flag: true
                });
            });
        }
        else {
            angular.forEach($scope.productionWorkCenterList, function (a) {
                $rootScope.tempList.push({
                    Id: a.Id
                    , Plant: a.Plant
                    , Entity: a.Entity
                    , WorkCenterMasterId: a.WorkCenterMasterId
                    , ProductionOrderId: a.ProductionOrderId
                    , Code: a.Code
                    , UserName: a.UserName
                    , Flag: true
                });
            });
        }

        $http({
            method: 'GET',
            url: 'OrderManagements/ProductionOrder/GetWorkCenterListByEntity?entityId=' + $scope.model.EntityId
        }).then(function successCallback(res) {
            $scope.workCenterList = res.data;

            if (baseService.arrayLength($scope.workCenterList) > 0) {
                for (var i = 0; i < $scope.workCenterList.length; i++) {
                    if (wcfor == 'RUNNING') {
                        for (var j = 0; j < $scope.runningWorkCenterList.length; j++) {
                            if ($scope.runningWorkCenterList[j].WorkCenterMasterId === $scope.workCenterList[i].WorkCenterMasterId) {
                                $scope.workCenterList[i].Flag = true;
                            }
                        }
                    }
                    else {
                        for (var j = 0; j < $scope.productionWorkCenterList.length; j++) {
                            if ($scope.productionWorkCenterList[j].WorkCenterMasterId === $scope.workCenterList[i].WorkCenterMasterId) {
                                $scope.workCenterList[i].Flag = true;
                            }
                        }
                    }

                }
            }
        });


        var eDialog = $("#workCenterPopUp").data("ejDialog");
        eDialog.open();
    }
    $scope.TabActiveIndex = -1;
    $scope.onactivetab = function (args) {
        try {
            $scope.TabActiveIndex = args.activeIndex;
            if (args.activeIndex == 1) {
                $scope.OpenSimulatedData();
                $scope.OpenSimulatedData();
            }
            else if (args.activeIndex == 2) {

                $scope.GetAllWorkcenterWisePlanningSummary();
            }
        } catch (e) {

        }

    }
    $scope.addWorkCenter = function () {
        if ($scope.workcenterfor == 'RUNNING') {
            if (baseService.arrayLength($rootScope.tempList) > 0) {
                angular.forEach($rootScope.tempList, function (a) {
                    if (!baseService.valueCheckInList($scope.runningWorkCenterList, 'WorkCenterMasterId', a.WorkCenterMasterId)) {
                        $scope.runningWorkCenterList.push({
                            Id: null
                            , isResidualApplicable: false
                            , Entity: a.Entity
                            , Plant: a.Plant
                            , WorkCenterMasterId: a.WorkCenterMasterId
                            , ProductionOrderId: $scope.model.Id
                            , Code: a.Code
                            , UserName: a.UserName
                        });
                    }
                });
            }
            else
                $scope.runningWorkCenterList = [];
            angular.forEach($scope.runningWorkCenterList, function (a) {
                if (!baseService.valueCheckInList($rootScope.tempList, 'WorkCenterMasterId', a.WorkCenterMasterId))
                    $scope.runningWorkCenterList.splice(a, 1);
            });
        }
        else {
            if (baseService.arrayLength($rootScope.tempList) > 0) {
                angular.forEach($rootScope.tempList, function (a) {
                    if (!baseService.valueCheckInList($scope.productionWorkCenterList, 'WorkCenterMasterId', a.WorkCenterMasterId)) {
                        $scope.productionWorkCenterList.push({
                            Id: null
                            , Entity: a.Entity
                            , Plant: a.Plant
                            , WorkCenterMasterId: a.WorkCenterMasterId
                            , ProductionOrderId: $scope.model.Id
                            , Code: a.Code
                            , UserName: a.UserName
                        });
                    }
                });
            }
            else
                $scope.productionWorkCenterList = [];
            angular.forEach($scope.productionWorkCenterList, function (a) {
                if (!baseService.valueCheckInList($rootScope.tempList, 'WorkCenterMasterId', a.WorkCenterMasterId))
                    $scope.productionWorkCenterList.splice(a, 1);
            });

        }


        $scope.CloseWorkCenterPopUp();
    };

    $scope.CloseWorkCenterPopUp = function () {

        var eDialog = $("#workCenterPopUp").data("ejDialog");
        var eDialog = $("#type2RunworkCenterPopUp").data("ejDialog");
        var eDialog = $("#type2workCenterPopUp").data("ejDialog");
        eDialog.close();

    };
    $scope.productWorkCenterList = [];
    $scope.rowDataBoundWorkCenter = function rowDataBoundWorkCenter(e) {

        if (angular.isUndefinedOrNull($scope.productWorkCenterList) == false) {
            for (var i = 0; i < $scope.productWorkCenterList.length; i++) {
                if ($scope.productWorkCenterList[i].Code == e.data.Code)
                    e.row.css("background-color", "#00ff00");

            }
        }

    }

    $scope.productionWorkCenterList = [];

    $scope.AddNewWorkCenter = function () {
        if ($scope.workcenterfor == 'RUNNING') {
            for (var i = 0; i < $scope.workCenterList.length; i++) {
                var exists = ej.DataManager($scope.runningWorkCenterList).executeLocal(ej.Query().where("Code", "equal", $scope.workCenterList[i].Code));
                if ($scope.workCenterList[i].Flag == true) {
                    if (exists.length == 0) {
                        $scope.runningWorkCenterList.push({
                            Id: null
                            , isResidualApplicable: false
                            , Plant: $scope.workCenterList[i].Plant
                            , Entity: $scope.workCenterList[i].Entity
                            , WorkCenterMasterId: $scope.workCenterList[i].WorkCenterMasterId
                            , ProductionOrderId: $scope.model.Id
                            , Code: $scope.workCenterList[i].Code
                            , UserName: $scope.workCenterList[i].UserName
                        });
                    }
                }
                else {
                    if (exists.length > 0) {
                        exists.pop();
                    }
                }
            }

        }
        else {

            for (var i = 0; i < $scope.workCenterList.length; i++) {
                var exists = ej.DataManager($scope.productionWorkCenterList).executeLocal(ej.Query().where("Code", "equal", $scope.workCenterList[i].Code));
                if ($scope.workCenterList[i].Flag == true) {
                    if (exists.length == 0) {
                        $scope.productionWorkCenterList.push({
                            Id: null
                            , Plant: $scope.workCenterList[i].Plant
                            , Entity: $scope.workCenterList[i].Entity
                            , WorkCenterMasterId: $scope.workCenterList[i].WorkCenterMasterId
                            , ProductionOrderId: $scope.model.Id
                            , Code: $scope.workCenterList[i].Code
                            , UserName: $scope.workCenterList[i].UserName
                        });
                    }
                }
                else {
                    if (exists.length > 0) {
                        exists.pop();
                    }
                }
            }
        }

        $scope.CloseWorkCenterPopUp();
    }

    $scope.processId = null;
    $scope.workCenterFPList = [];
    $scope.GetWorkCenterListByEntityandFirstProcess = function () {
        $http({
            method: 'GET',
            url: 'OrderManagements/ProductionOrder/GetWorkCenterListByEntityandFirstProcess?entityId=' + $scope.model.EntityId + '&processId=' + $scope.processId + '&productionOrderId=' + $scope.model.Id
        }).then(function successCallback(res) {
            $scope.workCenterFPList = res.data;

            for (var j = 0; j < $scope.productionFPWorkCenterList.length; j++) {
                if ($scope.productionFPWorkCenterList[j].WorkCenterMasterId === $scope.workCenterFPList[i].WorkCenterMasterId) {
                    $scope.workCenterFPList[i].Selection = true;
                }
            }
        });
        var eDialog = $("#workCenterbyFPPopUp").data("ejDialog");
        eDialog.open();
    }

    $scope.GetSavedWorkCenterListByEntityandFirstProcess = function () {
        $http({
            method: 'GET',
            url: 'OrderManagements/ProductionOrder/GetSavedType2WorkCenterListByEntityandFirstProcess?productionOrderId=' + $scope.model.Id
        }).then(function successCallback(res) {
            $scope.productionFPWorkCenterList = res.data;
            if ($scope.model.IsPreDefineLotApplicable) {
                //  $scope.GetPOLotControlSettingsData();
            }
        });
    }


    $scope.refreshTemplateFPWC = function (args) {
        $("#headchk").ejCheckBox({ "change": headCheckChangeFPWC });
    };

    function headCheckChangeFPWC(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        var filtered = $("#GridSelectWCFP").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.workCenterFPList.length; i++) {
                $scope.workCenterFPList[i].Selection = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].CheckBoxSelect = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridSelectWCFP").data("ejGrid");
        gridObj.refreshContent();
    };

    $scope.CloseFPWC = function () {
        try {
            for (var i = 0; i < $scope.workCenterFPList.length; i++) {
                if ($scope.workCenterFPList[i].Selection == true) {
                    if (checkExistsFPWC($scope.productionFPWorkCenterList, $scope.workCenterFPList[i].WorkCenterMasterId) === false) {
                        var ob = {};
                        ob.Id = null;
                        ob.ProcessId = $scope.processId;
                        ob.WorkCenterMasterId = $scope.workCenterFPList[i].WorkCenterMasterId;
                        ob.ProductionOrderId = $scope.model.Id;
                        ob.Plant = $scope.workCenterFPList[i].Plant;
                        ob.Entity = $scope.workCenterFPList[i].Entity;
                        ob.Code = $scope.workCenterFPList[i].Code;
                        ob.UserName = $scope.workCenterFPList[i].UserName;
                        ob.Remark = $scope.workCenterFPList[i].Remark;

                        $scope.productionFPWorkCenterList.push(ob);
                    }
                }
            }


            $scope.SaveFP();
            var eDialog = $("#workCenterbyFPPopUp").data("ejDialog");
            eDialog.close();
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.SaveFP = function () {
        try {
            if (baseService.arrayLength($scope.productionFPWorkCenterList) > 0) {
                $http({
                    method: 'POST',
                    url: 'OrderManagements/ProductionOrder/SaveWCFPData',
                    data: {
                        'data': $scope.productionFPWorkCenterList,
                        'productionOrderId': $scope.model.Id
                    },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.GetSavedWorkCenterListByEntityandFirstProcess();

                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');

                };
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    function checkExistsFPWC(list, id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].WorkCenterMasterId === id) {
                return true;
            }
        }
        return false;
    }

    $scope.message_detailconfirmation = null;
    $scope.removeFPWCDetail = function (obj) {

        $scope.FPWC = obj.data;
        if (!baseService.isUndefinedOrNull($scope.FPWC.Id))
            $scope.message_detailconfirmation = 'Are you sure want to delete permanently [ ' + $scope.FPWC.UserName + ' ]';
        angular.element(document.querySelector('#confirmBoMDetailPopUp')).modal('show');
    }

    $scope.DeleteFPWCDetail = function () {
        $http({
            method: 'POST',
            url: 'OrderManagements/ProductionOrder/DeleteFPWCDetail?id=' + $scope.FPWC.Id
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.GetSavedWorkCenterListByEntityandFirstProcess();
            }
        }, function () {
            ShowResult(commonMessage.NetworkError, 'failure');
        }).finally(function () {
        });

    };

    // #endregion  Work Center

    $scope.removeRowModal = function (name, index, listName, tempId, listId) {
        try {
            $scope.popUpIndex = index;
            $scope.listName = listName;
            $scope.tempId = tempId;
            $scope.listId = listId;
            $scope.message_confirmation = "Are you sure want to delete [" + name + "] ";
            angular.element(document.querySelector('#confirmRecipeMaterialPopUp')).modal('show');
        }
        catch (e) {
            ShowResult(e, 'Error');
        }
    };
    $scope.removeRow = function () {
        for (var t = 0; t < baseService.arrayLength($rootScope.tempList); t++) {
            if ($rootScope.tempList[t][$scope.tempId] === $scope[$scope.listName][$scope.popUpIndex][$scope.listId])
                $rootScope.tempList.splice(t, 1);
        }
        $scope[$scope.listName].splice($scope.popUpIndex, 1);
        $scope.popUpIndex = -1;
        angular.element(document.querySelector('#confirmRecipeMaterialPopUp')).modal('hide');
    };


    $scope.lotControlList = [];
    $scope.GetPOLotControlSettingsData = function () {
        try {
            $http({
                method: 'GET',
                url: 'OrderManagements/ProductionOrder/GetPOLotContSettingsData?poId=' + $scope.model.Id + '&entityId=' + $scope.model.EntityId
            }).then(function (response) {
                $scope.lotControlList = response.data;

            });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }
    $scope.GetPOLotControlData = function () {

        try {
            if (baseService.isUndefinedOrNull($scope.model.UserDefineLotNo)) {
                throw "User Define LotNo is required.";
            }
            $http({
                method: 'GET',
                url: 'OrderManagements/ProductionOrder/GetPOLotControlSettingsData?poId=' + $scope.model.Id + '&entityId=' + $scope.model.EntityId + '&userLotNo=' + $scope.model.UserDefineLotNo
            }).then(function (response) {

                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    if (baseService.arrayLength($scope.lotControlList) > 0) {
                        for (var i = 0; i < $scope.lotControlList.length; i++) {
                            for (var j = 0; j < response.data.length; j++) {
                                if (!baseService.isUndefinedOrNull($scope.lotControlList[i].Id) && $scope.lotControlList[i].ProcessId == response.data[j].ProcessId && $scope.lotControlList[i].SeqNo == response.data[j].SeqNo) {
                                    $scope.lotControlList[i].LotNo = response.data[j].LotNo;
                                    $scope.lotControlList[i].UserLotNo = response.data[j].UserLotNo;
                                }
                            }
                        }
                    } else {
                        $scope.lotControlList = response.data;
                    }
                    var gridObj = $("#GridLC").data("ejGrid");
                    gridObj.refreshContent();
                    gridObj.refreshTemplate();
                }

            });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.tempModel = {};
    $scope.SetSufix = function (args) {
        $scope.tempModel = args.data;
        //var str = $scope.tempModel.UserLotNo;
        //var extention = str.substr(str.indexOf('/')+1);

        //if (!baseService.isUndefinedOrNull($scope.tempModel.Sufix)) {
        //    if ($scope.tempModel.Sufix != extention) {
        //        $scope.tempModel.UserLotNo = $scope.tempModel.UserLotNo + '/' + $scope.tempModel.Sufix;
        //    }
        //}
        if (baseService.isUndefinedOrNull($scope.tempModel.Sufix)) {
            $scope.tempModel.UserLotNo = $scope.tempModel.LotNo;
        }
        else {
            $scope.tempModel.UserLotNo = $scope.tempModel.LotNo + '/' + $scope.tempModel.Sufix;
        }
        var gridObj = $("#GridLC").data("ejGrid");
        gridObj.refreshContent();
        gridObj.refreshTemplate();
    }

    $scope.SaveLotControl = function () {
        try {
            $http({
                method: 'POST',
                url: 'OrderManagements/ProductionOrder/CreateLotControl',
                data: {
                    'data': $scope.lotControlList
                    , 'poId': $scope.model.Id
                },
                dataType: 'JSON'
                , contentType: "application/json charset=utf-8"
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.GetPOLotControlSettingsData();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.POItemList = [];
    $scope.getMaterialMasterbyTypePopUp = function () {
        $scope.POItemList = [];
        $http.get('Productions/ProductionSummary/GetItemsData?entityid=' + $scope.productionSummaryNew.EntityId + '&workCenterMasterId=' + $scope.productionSummaryNew.WorkCenterMasterId + '&productionLevel=' + $scope.productionSummaryNew.ProductionBookingLevel + '&processId=' + $scope.productionSummaryNew.ProcessId + '&ProductionOrderId=' + $scope.productionSummaryNew.ProductionOrderId)
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.POItemList = response.data;
                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
        angular.element(document.querySelector('#POItemPopup')).modal('show');

    };

    $scope.SONo = null;
    $scope.ProductionOrderList = [];
    $scope.GetProductionOrderPopUp = function () {
        $http({
            method: 'POST',
            data: {
                'id': $scope.model.Id
            },
            url: 'OrderManagements/ProductionOrder/GetProductionOredrList'
        }).then(function successCallback(response) {
            $scope.ProductionOrderList = response.data;
            $scope.GetSavedSKUData();
            //$scope.GetAllWorkcenterWisePlanningSummary();
        });

    };

    $scope.ModelNewSPO = {
        SKU1: false, SKU2: false, Both: true, SPT: 0, PlanHour: 24, PlanPercentage: 0, NetUtilizationPercentage: 0, MinQty: 1, LSD: null, NoOfWorkStation:1
    }

    $scope.SetCheckbox = function (name) {
        if (name === 'sku1') {
            $scope.ModelNewSPO.SKU1 = true;
            $scope.ModelNewSPO.SKU2 = false;
            $scope.ModelNewSPO.Both = false;
        }
        if (name === 'sku2') {
            $scope.ModelNewSPO.SKU2 = true;
            $scope.ModelNewSPO.SKU1 = false;
            $scope.ModelNewSPO.Both = false;
        }
        if (name === 'both') {
            $scope.ModelNewSPO.Both = true;
            $scope.ModelNewSPO.SKU1 = false;
            $scope.ModelNewSPO.SKU2 = false;
        }
    }

    $scope.disableinput = false;

    $scope.GetSavedSKUData = function () {
        $http({
            method: 'POST',
            data: {
                'poId': $scope.model.Id
            },
            url: 'OrderManagements/ProductionOrder/GetSavedSKUData'
        }).then(function successCallback(response) {
            $scope.sku1sku2List = response.data;
            if (!baseService.isUndefinedOrNull($scope.sku1sku2List[0].ID)) {
                $scope.disableinput = true;
                $scope.ModelNewSPO.SKU1 = $scope.sku1sku2List[0].SKU1;
                $scope.ModelNewSPO.SKU2 = $scope.sku1sku2List[0].SKU2;
                $scope.ModelNewSPO.Both = $scope.sku1sku2List[0].Both;
            }
            $scope.getModelFilter();

        });
    }


    $scope.sku1sku2List = [];
    $scope.GetSKUData = function () {
        $scope.sku1List = [];
        $scope.sku2List = [];
        $scope.sku1sku2List = [];
        try {

            if ($scope.ModelNewSPO.SKU1 == false && $scope.ModelNewSPO.SKU2 == false && $scope.ModelNewSPO.Both == false) {
                throw "Select SKU level.";
            }
            $scope.$broadcast('show-errors-check-validity');
            if ($scope.SPONewForm.$valid) {

                $http({
                    method: 'POST',
                    data: {
                        'poId': $scope.model.Id, 'SKU1': $scope.ModelNewSPO.SKU1, 'SKU2': $scope.ModelNewSPO.SKU2, 'Both': $scope.ModelNewSPO.Both
                    },
                    url: 'OrderManagements/ProductionOrder/GetSKUData'
                }).then(function successCallback(response) {

                    for (var i = 0; i < response.data.length; i++) {
                        response.data[i].ID = -(Math.floor(Math.random() * 100) + 1);
                        response.data[i].NoOfWorkStation = 1;
                        response.data[i].PlanPercentage = $scope.ModelNewSPO.PlanPercentage;
                        response.data[i].Efficiency = $scope.ModelNewSPO.NetUtilizationPercentage;
                        response.data[i].SPT = $scope.ModelNewSPO.SPT;
                        response.data[i].PlanWorkingHoursPerDay = $scope.ModelNewSPO.PlanHour;
                        response.data[i].FirstDayOutPut = $scope.ModelNewSPO.FirstDayOutPut;
                        response.data[i].DayToReachTheTarget = 0;
                        response.data[i].LSD = $scope.ModelNewSPO.LSD;
                        response.data[i].CommitmentDate = $scope.ModelNewSPO.CommitmentDate;
                        response.data[i].MainRawMaterialInhouseDate = $scope.ModelNewSPO.MainRawMaterialInhouseDate;
                        response.data[i].OtherRawMaterialInhouseDate = $scope.ModelNewSPO.OtherRawMaterialInhouseDate;
                        response.data[i].ProductionPriority = $scope.ModelNewSPO.ProductionPriority;
                        response.data[i].TargetPerHour = 0;
                        response.data[i].TargetPerDay = 0;
                        response.data[i].EfficiencyPercentage = 0;
                        response.data[i].IncrementValue = 0;
                        response.data[i].IncrementType = null;
                        response.data[i].RequiredNoOfLines = 0;
                        response.data[i].RequiredLineDays = 0;
                        response.data[i].MinimumLineDays = 10;
                        response.data[i].Qty = response.data[i].Qty;
                        response.data[i].AdjustableQty = 0;
                        response.data[i].AllocatedLines = 0;
                        response.data[i].SKU1 = $scope.ModelNewSPO.SKU1;
                        response.data[i].SKU2 = $scope.ModelNewSPO.SKU2;
                        response.data[i].Both = $scope.ModelNewSPO.Both;
                        response.data[i].IncrementType = "FIXED";
                        response.data[i].IncrementValue = 100;
                        response.data[i].RunningOrderBlockSize = 1;
                        response.data[i].WCPreferenceType = 'INCLUDE';

                        response.data[i].PlanQty = (response.data[i].Qty + response.data[i].AdjustableQty) * (response.data[i].PlanPercentage + 100) / 100;

                        response.data[i].RequiredLineDays = parseFloat((response.data[i].PlanWorkingHoursPerDay * 60) / (response.data[i].SPT * response.data[i].Qty) / response.data[i].Efficiency).toFixed(2);
                        response.data[i].MaximumAllowedWorkCenter = Math.floor(response.data[i].RequiredLineDays) / response.data[i].MinimumLineDays;
                        if (response.data[i].MaximumAllowedWorkCenter < 1) {
                            response.data[i].MaximumAllowedWorkCenter = 1;
                        }

                        if (response.data[i].NoOfWorkStation > 0 || response.data[i].Efficiency > 0 || response.data[i].SPT > 0) {

                            response.data[i].TargetPerHour = (response.data[i].NoOfWorkStation * 60 / response.data[i].SPT);
                            $scope.TargetQtyAtFullEfficiency = response.data[i].TargetPerHour;
                            if (response.data[i].TargetPerHour > 0) {

                                response.data[i].TargetPerDay = (response.data[i].PlanWorkingHoursPerDay * response.data[i].TargetPerHour);
                                $scope.EfficiencyPercentage = (response.data[i].TargetPerDay);// * response.data[i].Efficiency / 100;


                                //at efficiency level
                                response.data[i].TargetPerHour = response.data[i].TargetPerHour * response.data[i].Efficiency / 100;
                                response.data[i].TargetPerDay = response.data[i].TargetPerDay * response.data[i].Efficiency / 100;



                                response.data[i].RequiredLineDays = (response.data[i].Qty / response.data[i].TargetPerDay).toFixed(2);
                            }

                            if (response.data[i].MinimumLineDays > 0) {

                                response.data[i].RequiredNoOfLines = response.data[i].RequiredLineDays / response.data[i].MinimumLineDays;

                                if (response.data[i].RequiredNoOfLines > 0 && response.data[i].RequiredNoOfLines <= 1)
                                    response.data[i].AllocatedLines = 1;

                                if (response.data[i].RequiredNoOfLines > 1)
                                    response.data[i].AllocatedLines = Math.floor(response.data[i].RequiredNoOfLines);
                            }

                            try {
                                response.data[i].RequiredNoOfLines = response.data[i].RequiredNoOfLines.toFixed(4)
                                response.data[i].RequiredLineDays = response.data[i].RequiredLineDays.toFixed(4)
                            } catch (e) {

                            }
                        }
                        if (response.data[i].FirstDayOutPut > 0 && response.data[i].IncrementValue > 0) {

                            if (response.data[i].IncrementType == "FIXED" || response.data[i].IncrementType == "PERCENTAGE") {
                                var daysrequired = 1;
                                if (response.data[i].FirstDayOutPut < response.data[i].TargetPerHour) {
                                    daysrequired = 1;
                                    var firstdaysoutput = response.data[i].FirstDayOutPut;
                                    while (firstdaysoutput * response.data[i].PlanWorkingHoursPerDay < response.data[i].TargetPerDay) {
                                        daysrequired++;
                                        //if (response.data[i].IncrementType == "FIXED")
                                        firstdaysoutput += response.data[i].IncrementValue;

                                        //compounding method
                                        //if (response.data[i].IncrementType == "PERCENTAGE")
                                        //    firstdaysoutput = firstdaysoutput + (firstdaysoutput * response.data[i].IncrementValue / 100);



                                    }

                                }
                                response.data[i].DayToReachTheTarget = daysrequired.toFixed(2);
                            }

                        }

                    }

                    $scope.sku1sku2List = response.data;
                });
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }

    };

    $scope.calculate = function (obj) {
        for (var i = 0; i < $scope.sku1sku2List.length; i++) {
            $scope.sku1sku2List[i].PlanQty = ($scope.sku1sku2List[i].Qty + $scope.sku1sku2List[i].AdjustableQty) * ($scope.sku1sku2List[i].PlanPercentage + 100) / 100;

            $scope.sku1sku2List[i].RequiredLineDays = parseFloat(($scope.sku1sku2List[i].PlanWorkingHoursPerDay * 60) / ($scope.sku1sku2List[i].SPT * $scope.sku1sku2List[i].Qty) / $scope.sku1sku2List[i].Efficiency).toFixed(2);
            $scope.sku1sku2List[i].MaximumAllowedWorkCenter = Math.floor($scope.sku1sku2List[i].RequiredLineDays) / $scope.sku1sku2List[i].MinimumLineDays;
            if ($scope.sku1sku2List[i].MaximumAllowedWorkCenter < 1) {
                $scope.sku1sku2List[i].MaximumAllowedWorkCenter = 1;
            }

            if ($scope.sku1sku2List[i].NoOfWorkStation > 0 || $scope.sku1sku2List[i].Efficiency > 0 || $scope.sku1sku2List[i].SPT > 0) {

                $scope.sku1sku2List[i].TargetPerHour = ($scope.sku1sku2List[i].NoOfWorkStation * 60 / $scope.sku1sku2List[i].SPT);
                $scope.TargetQtyAtFullEfficiency = $scope.sku1sku2List[i].TargetPerHour;
                if ($scope.sku1sku2List[i].TargetPerHour > 0) {

                    $scope.sku1sku2List[i].TargetPerDay = ($scope.sku1sku2List[i].PlanWorkingHoursPerDay * $scope.sku1sku2List[i].TargetPerHour);
                    $scope.EfficiencyPercentage = ($scope.sku1sku2List[i].TargetPerDay);// * $scope.sku1sku2List[i].Efficiency / 100;


                    //at efficiency level
                    $scope.sku1sku2List[i].TargetPerHour = $scope.sku1sku2List[i].TargetPerHour * $scope.sku1sku2List[i].Efficiency / 100;
                    $scope.sku1sku2List[i].TargetPerDay = $scope.sku1sku2List[i].TargetPerDay * $scope.sku1sku2List[i].Efficiency / 100;



                    $scope.sku1sku2List[i].RequiredLineDays = ($scope.sku1sku2List[i].Qty / $scope.sku1sku2List[i].TargetPerDay).toFixed(2);
                }

                if ($scope.sku1sku2List[i].MinimumLineDays > 0) {

                    $scope.sku1sku2List[i].RequiredNoOfLines = $scope.sku1sku2List[i].RequiredLineDays / $scope.sku1sku2List[i].MinimumLineDays;

                    if ($scope.sku1sku2List[i].RequiredNoOfLines > 0 && $scope.sku1sku2List[i].RequiredNoOfLines <= 1)
                        $scope.sku1sku2List[i].AllocatedLines = 1;

                    if ($scope.sku1sku2List[i].RequiredNoOfLines > 1)
                        $scope.sku1sku2List[i].AllocatedLines = Math.floor($scope.sku1sku2List[i].RequiredNoOfLines);
                }

                try {
                    $scope.sku1sku2List[i].RequiredNoOfLines = $scope.sku1sku2List[i].RequiredNoOfLines.toFixed(4)
                    $scope.sku1sku2List[i].RequiredLineDays = $scope.sku1sku2List[i].RequiredLineDays.toFixed(4)
                } catch (e) {

                }
            }
            if ($scope.sku1sku2List[i].FirstDayOutPut > 0 && $scope.sku1sku2List[i].IncrementValue > 0) {

                if ($scope.sku1sku2List[i].IncrementType == "FIXED" || $scope.sku1sku2List[i].IncrementType == "PERCENTAGE") {
                    var daysrequired = 1;
                    if ($scope.sku1sku2List[i].FirstDayOutPut < $scope.sku1sku2List[i].TargetPerHour) {
                        daysrequired = 1;
                        var firstdaysoutput = $scope.sku1sku2List[i].FirstDayOutPut;
                        while (firstdaysoutput * $scope.sku1sku2List[i].PlanWorkingHoursPerDay < $scope.sku1sku2List[i].TargetPerDay) {
                            daysrequired++;
                            //if ($scope.sku1sku2List[i].IncrementType == "FIXED")
                            firstdaysoutput += $scope.sku1sku2List[i].IncrementValue;

                            //compounding method
                            //if ($scope.sku1sku2List[i].IncrementType == "PERCENTAGE")
                            //    firstdaysoutput = firstdaysoutput + (firstdaysoutput * $scope.sku1sku2List[i].IncrementValue / 100);



                        }

                    }
                    $scope.sku1sku2List[i].DayToReachTheTarget = daysrequired.toFixed(2);
                }

            }
        }
        var gridObj = $("#GridSKU12").data("ejGrid");
        gridObj.refreshContent();
    }

    $scope.CheckMaxWCValue = function (obj) {
        try {
            if (obj.data.AllocatedLines > obj.data.NoOfWorkStation) {
                throw "Alloted Work Center can't greater than Maximum Allowed Work Center.";
            }

        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.tempOj = {}
    $scope.wcgList = [];
    $scope.GetWorkCenterGroup = function (obj) {
        $scope.tempOj = obj.data;
        $http({
            method: 'POST',
            url: 'OrderManagements/ProductionOrder/GetWorkCenterGroup'
        }).then(function successCallback(response) {
            $scope.wcgList = response.data;
            angular.element(document.querySelector('#WCGPopUp')).modal('show');
        });
    }

    $scope.selectWCG = function (data) {
        $scope.tempOj.WorkCenterGroupId = data.data.Id
        $scope.tempOj.WorkCenterGroup = data.data.UserName;
        var gridObj = $("#GridSKU12").data("ejGrid");
        gridObj.refreshContent();
        $scope.CloseWCG();
    };

    $scope.CloseWCG = function () {
        angular.element(document.querySelector('#WCGPopUp')).modal('hide');
    }

    $scope.SaveSPO = function () {
        try {
            $http({
                method: 'POST',
                url: "OrderManagements/ProductionOrder/SaveSPO",
                data: { 'data': $scope.sku1sku2List, 'POId': $scope.model.Id },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.GetSavedSKUData();
                }
            }, function errorCallback(response) {
                ShowResult(response.data.Message, 'failure');
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.workCenterList = [];
    $scope.workcenterfor = '';
    //$scope.workcenterDialog = $("#workCenterPopUp").ejDialog({ target: "#entrycontainer" });
    $scope.GetWorkCenterMaster = function (wcfor) {
        $scope.workcenterfor = wcfor;
        $rootScope.tempList = [];
        $scope.workCenterList = [];

        if (wcfor == 'RUNNING') {
            angular.forEach($scope.runningWorkCenterList, function (a) {
                $rootScope.tempList.push({
                    Id: a.Id
                    , Plant: a.Plant
                    , Entity: a.Entity
                    , WorkCenterMasterId: a.WorkCenterMasterId
                    , ProductionOrderId: $scope.tempOj.ID
                    , Code: a.Code
                    , UserName: a.UserName
                    , Flag: true
                });
            });
        }
        else {
            angular.forEach($scope.productionWorkCenterList, function (a) {
                $rootScope.tempList.push({
                    Id: a.Id
                    , Plant: a.Plant
                    , Entity: a.Entity
                    , WorkCenterMasterId: a.WorkCenterMasterId
                    , ProductionOrderId: $scope.tempOj.ID
                    , Code: a.Code
                    , UserName: a.UserName
                    , Flag: true
                });
            });
        }

        $http({
            method: 'GET',
            url: 'OrderManagements/productionOrderSchedulingParametersType1/GetType2WorkCenterList?entityIds=' + $scope.model.EntityId + "&processid=" + $scope.model.PlanningTypeProcessId + "&wcgId=" + $scope.tempOj.WorkCenterGroupId
        }).then(function successCallback(res) {
            $scope.workCenterList = res.data;

            if (baseService.arrayLength($scope.workCenterList) > 0) {
                for (var i = 0; i < $scope.workCenterList.length; i++) {
                    if (wcfor == 'RUNNING') {
                        for (var j = 0; j < $scope.runningWorkCenterList.length; j++) {
                            if ($scope.runningWorkCenterList[j].WorkCenterMasterId === $scope.workCenterList[i].WorkCenterMasterId) {
                                $scope.workCenterList[i].Flag = true;
                            }
                        }
                    }
                    else {
                        for (var j = 0; j < $scope.productionWorkCenterList.length; j++) {
                            if ($scope.productionWorkCenterList[j].WorkCenterMasterId === $scope.workCenterList[i].WorkCenterMasterId) {
                                $scope.workCenterList[i].Flag = true;
                            }
                        }
                    }

                }
            }
        });


        if (wcfor == 'RUNNING') {
            var eDialog = $("#type2RunworkCenterPopUp").data("ejDialog");
            eDialog.open();
        } else {
            var eDialog = $("#type2workCenterPopUp").data("ejDialog");
            eDialog.open();
        }
    }
    $scope.productionWorkCenterList = [];
    $scope.runningWorkCenterList = [];

    $scope.AddNewType2WorkCenter = function () {
        if ($scope.workcenterfor == 'RUNNING') {
            for (var i = 0; i < $scope.workCenterList.length; i++) {
                var exists = ej.DataManager($scope.runningWorkCenterList).executeLocal(ej.Query().where("Code", "equal", $scope.workCenterList[i].Code));
                if ($scope.workCenterList[i].Flag == true) {
                    if (exists.length == 0) {
                        $scope.runningWorkCenterList.push({
                            Id: null
                            , isResidualApplicable: false
                            , Plant: $scope.workCenterList[i].Plant
                            , Entity: $scope.workCenterList[i].Entity
                            , WorkCenterMasterId: $scope.workCenterList[i].WorkCenterMasterId
                            , ProductionOrderId: $scope.tempOj.ID
                            , Code: $scope.workCenterList[i].Code
                            , UserName: $scope.workCenterList[i].UserName
                        });
                    }
                }
                else {
                    if (exists.length > 0) {
                        exists.pop();
                    }
                }
            }

        }
        else {

            for (var i = 0; i < $scope.workCenterList.length; i++) {
                var exists = ej.DataManager($scope.productionWorkCenterList).executeLocal(ej.Query().where("Code", "equal", $scope.workCenterList[i].Code));
                if ($scope.workCenterList[i].Flag == true) {
                    if (exists.length == 0) {
                        $scope.productionWorkCenterList.push({
                            Id: null
                            , Plant: $scope.workCenterList[i].Plant
                            , Entity: $scope.workCenterList[i].Entity
                            , WorkCenterMasterId: $scope.workCenterList[i].WorkCenterMasterId
                            , ProductionOrderId: $scope.tempOj.ID
                            , Code: $scope.workCenterList[i].Code
                            , UserName: $scope.workCenterList[i].UserName
                        });
                    }
                }
                else {
                    if (exists.length > 0) {
                        exists.pop();
                    }
                }
            }
        }

        $scope.CloseWorkCenterPopUp();
    }


    $scope.productionWorkCenterList = [];
    $scope.runningWorkCenterList = [];

    $scope.GetPrefenceWorkCenter = function (obj) {
        $scope.tempOj = obj.data;
        $http({
            method: 'GET',
            url: $scope.path + 'GetProductionOrderType2WorkCenterList?productionOrderId=' + $scope.tempOj.ID
        }).then(function successCallback(response) {
            $scope.productionWorkCenterList = response.data;
        });
        angular.element(document.querySelector('#PrefenceWorkCenterPopUp')).modal('show');

    }

    $scope.GetSavedPrefenceWorkCenter = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'GetProductionOrderType2WorkCenterList?productionOrderId=' + $scope.tempOj.ID
        }).then(function successCallback(response) {
            $scope.productionWorkCenterList = response.data;
        });

    }

    $scope.ClosePrefenceWorkCenter = function () {
        angular.element(document.querySelector('#PrefenceWorkCenterPopUp')).modal('hide');
    }

    $scope.CloseRunningeWorkCenter = function () {
        angular.element(document.querySelector('#RunningWorkCenterPopUp')).modal('hide');
    }

    $scope.GetRunningWorkCenter = function (obj) {
        $scope.tempOj = obj.data;
        $http({
            method: 'GET',
            url: $scope.path + 'GetRunningOrderType2WorkCenterList?productionOrderId=' + $scope.tempOj.ID
        }).then(function successCallback(response) {
            $scope.runningWorkCenterList = response.data;
        });
        angular.element(document.querySelector('#RunningWorkCenterPopUp')).modal('show');
    }


    $scope.GetSavedRunningWorkCenter = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'GetRunningOrderType2WorkCenterList?productionOrderId=' + $scope.tempOj.ID
        }).then(function successCallback(response) {
            $scope.runningWorkCenterList = response.data;
        });
    }


    $scope.SavePrefenceWorkCenter = function () {
        try {
            if ($scope.productionWorkCenterList.length > 0) {
                $http({
                    method: 'POST',
                    url: "OrderManagements/ProductionOrder/SavePreferenceWorkCenter",
                    data: { 'workcenterlist': $scope.productionWorkCenterList, 'spoId': $scope.tempOj.ID },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.GetSavedPrefenceWorkCenter();
                    }
                }, function errorCallback(response) {
                    ShowResult(response.data.Message, 'failure');
                });

            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.SaveRunningWorkCenter = function () {
        try {
            if ($scope.runningWorkCenterList.length > 0) {
                $http({
                    method: 'POST',
                    url: "OrderManagements/ProductionOrder/SaveType2RunningWorkCenter",
                    data: { 'workcenterlist': $scope.runningWorkCenterList, 'spoId': $scope.tempOj.ID },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.GetSavedRunningWorkCenter();
                    }
                }, function errorCallback(response) {
                    ShowResult(response.data.Message, 'failure');
                });

            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.appointments = [];
    $scope.setDate = new Date();
    $scope.group = {
        resources: ["WorkCenters"]
    };
    $scope.groupdata = [];
    $scope.resourcedata2 = {
        //dataSource: $scope.groupdata,
        dataSource: [
            { text: "Workcenter", id: 3, groupId: 1, color: "#ffaa00" }
        ],
        text: "text", id: "id", groupId: "groupId", color: "color"
    };

    $scope.workweek = ["Saturday", "Friday", "Monday", "Tuesday", "Wednesday", "Thursday"];
    $scope.FreezeDate = null;
    $scope.tempo = "";
    $scope.plancolorchange = function (args) {



        if ($scope.tempo != args.requestType) {
            $scope.tempo = args.requestType;
        }
        try {
            if (args.requestType == "resourcegroupheader") {
                args.element[0].innerText = "Work Centers";
                args.element.css("vertical-align", "middle");
                args.element.css("text-align", "center");
            }

            if (args.requestType == "headercells") {
                try {
                    try {
                        if (args.element[0].innerText.length > 4) {
                            args.element[0].innerText = "0" + args.element[0].innerText.substring(4);
                            args.element.css("color", "#0000ff");
                            args.element.css("vertical-align", "middle");
                            args.element.css("text-align", "center");
                        }

                    } catch (e) { }

                    var Ayear = args.model.currentDate().getFullYear();
                    var AMonth = args.model.currentDate().getMonth() + 1;
                    var ADay = parseInt(args.element[0].innerText);

                    var FDate = new Date($scope.FreezeDate);
                    var Fyear = FDate.getFullYear();
                    var FMonth = FDate.getMonth();
                    var FDay = FDate.getDate();

                    if (Ayear == Fyear & AMonth == FMonth & ADay == FDay) {
                        args.element.css("background", "#FF5733");
                    }
                } catch (e) {

                }

            }

            if (args.requestType == "appointment") {

                args.element.css("background", args.appointment.Color);
                args.element.css("border-color", args.appointment.Color);
                args.element.css("color", args.appointment.Color);
                args.element.css("font-size", "1px");
                args.element.css("height", "19px");

                try {
                    for (var i = 0; i < args.element.length; i++) {
                        args.element[i].innerText = "";
                    }
                } catch (e) {

                }

                if (args.appointment.isBuildUp == true) {
                    args.element.css("border-radius", "100%");
                }

                if (args.appointment.FilterData == 0) {
                    args.element.css("opacity", "0.1");
                }

                if (args.appointment.isStyleChange == true) {
                    args.element.css("border-color", "yellow");
                    args.element.css("border-style", "groove");
                    args.element.css("border-width", "4px");
                }

                if (args.appointment.planningStatus == "FREEZE") {
                    args.element.css("border-bottom", "4px  groove blue");
                }
                else if (args.appointment.planningStatus == "RUNNING") {
                    args.element.css("border-bottom", "4px  groove green");
                }
                if (args.appointment.FailedToCommitmentDate == true) {
                    args.element.css("border-top", "4px  groove red");
                }
            }
        } catch (e) {

        }

    }


    $scope.Simulate = function () {
        try {
            //var DropDownEntityListObj = $("#entityList").data("ejDropDownList");
            //$scope.EntityId = DropDownEntityListObj.getSelectedValue();

            //if (angular.isUndefinedOrNull($scope.EntityId)) {
            //    for (var i = 0; i < DropDownEntityListObj.popupListItems.length; i++) {
            //        if (angular.isUndefinedOrNull($scope.EntityId)) {
            //            EntityId = + DropDownEntityListObj.popupListItems[i].Id;
            //        } else {
            //            EntityId += ',' + DropDownEntityListObj.popupListItems[i].Id;
            //        }
            //    }
            //}
            $http({
                method: 'GET',
                url: "OrderManagements/productionOrderSchedulingParametersType1/ProductionType2PlanSimulation?entityid=" + $scope.model.EntityId + "&processid=" + $scope.model.PlanningTypeProcessId
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult("Simulated successfully", 'success');


                    var args = { "requestType": "filtering" };
                    $scope.filterComplete(args);

                }
            });
        } catch (e) {

        }
    }

    $scope.ModelFilter = null;
    $scope.filtergridonload = function () {
        try {
            $("#GridPlanFilter").children('.e-pager.e-js.e-pager').hide();
            $("#GridPlanFilter").children('.e-gridcontent.e-droppable.e-js').hide();
            $("#GridPlanFilter").children('.e-gridcontent').hide();
        } catch (e) {

        }

    }

    $scope.getModelFilter = function () {
        $http({
            method: 'POST',
            url: "OrderManagements/productionOrderSchedulingParametersType1/LoadType2NewFilterSQL"
        }).then(function successCallback(response) {

            if (response.data.length > 0) {
                try {
                    $scope.ModelFilter = response.data;

                } catch (e) {

                }

            }
            $scope.filtergridonload();
        });

    };
    $scope.filterComplete = function (args) {
        if (args.requestType == "filtering") {
            var gridObj = $("#GridPlanFilterT2").data("ejGrid");
            var filteredRecords = gridObj.getFilteredRecords();
            if (angular.isUndefinedOrNull(filteredRecords) == false) {
                if (filteredRecords.length > 0) {
                    var parameters = [];
                    parameters.push({ "Key": "ProductOrderId", "Value": getString(filteredRecords, "ProductOrderId") });
                    parameters.push({ "Key": "WorkCenterId", "Value": getString(filteredRecords, "WorkCenterId") });
                    //parameters.push({ "Key": "EntityId", "Value": getString(filteredRecords, "EntityId") });
                    parameters.push({ "Key": "ProductMasterId", "Value": getString(filteredRecords, "ProductMasterId") });
                    parameters.push({ "Key": "ProductCategoryId", "Value": getString(filteredRecords, "ProductCategoryId") });
                    parameters.push({ "Key": "MaterialMasterId", "Value": getString(filteredRecords, "MaterialMasterId") });
                    parameters.push({ "Key": "ArticleId", "Value": getString(filteredRecords, "ArticleId") });
                    parameters.push({ "Key": "BuyerId", "Value": getString(filteredRecords, "BuyerId") });
                    parameters.push({ "Key": "CustomerId", "Value": getString(filteredRecords, "CustomerId") });
                    parameters.push({ "Key": "AccountInchargeId", "Value": getString(filteredRecords, "AccountInchargeId") });
                    parameters.push({ "Key": "AccountHolderId", "Value": getString(filteredRecords, "AccountHolderId") });
                    parameters.push({ "Key": "ProductionStatusId", "Value": getString(filteredRecords, "ProductionStatusId") });

                    parameters.push({ "Key": "MasterOrderNo", "Value": getString(filteredRecords, "MasterOrderNo") });
                    parameters.push({ "Key": "BuyerOrderNo", "Value": getString(filteredRecords, "BuyerOrderNo") });
                    parameters.push({ "Key": "BuyerItemNo", "Value": getString(filteredRecords, "BuyerItemNo") });


                    $scope.SimulateVisual(parameters);
                }
                else {
                    $scope.SimulateVisual(null);
                }
            }
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

    $scope.OpenSimulatedData = function () {
        try {


            var args = { "requestType": "filtering" };
            $scope.filterComplete(args);

        } catch (e) {

        }
    }

    $scope.renderDates = {
        start: new Date(),
        end: new Date().setDate(new Date().getDate() + 30)
    }
    $scope.viewtype = ["CustomView"];
    $scope.currentDate = { day: new Date().getDate(), month: new Date().getMonth(), year: new Date().getFullYear() };


    $scope.SimulateVisual = function (ExtraParams) {
        var _data = {};
        var _path = "OrderManagements/productionOrderSchedulingParametersType1/GetNewScheduleData?entityid=" + $scope.model.EntityId + "&processid=" + $scope.model.PlanningTypeProcessId + "&year=" + $scope.currentDate.year + "&month=" + $scope.currentDate.month + "&day=" + $scope.currentDate.day;

        if (angular.isUndefinedOrNull(ExtraParams) == false) {
            _path = "OrderManagements/productionOrderSchedulingParametersType1/GetNewScheduleDataFiltered?entityid=" + $scope.model.EntityId + "&processid=" + $scope.model.PlanningTypeProcessId + "&year=" + $scope.currentDate.year + "&month=" + $scope.currentDate.month + "&day=" + $scope.currentDate.day;

            var _data = {
                "parameters": ExtraParams
            }
        }
        try {
            $http({
                method: 'POST',
                url: _path,
                data: _data
            }).then(function successCallback(res) {

                if (res.data.DATA.length > 0) {
                    $scope.resourcedata2 = {
                        dataSource: res.data.GROUPDATA,
                        text: "text", id: "id", groupId: "groupId", color: "color"
                    };
                    //for (var i = 0; i < res.data.DATA.length; i++) {
                    //    res.data.DATA[i].AllDay = true;
                    //    res.data.DATA[i].Recurrence = false;
                    //}
                    $scope.workweek = res.data.WORKDAYDATA;
                    $scope.appointments = angular.copy(res.data.DATA);

                    try {
                        var gridObj = $("#GridPlanFilterT2").data("ejGrid");
                        //gridObj.clearFiltering();
                        $scope.getModelFilter();
                    } catch (e) {

                    }


                    $scope.FreezeDate = res.data.FREEZEDATE;

                    var schObj = $("#ResourceGroupScheduleT2").data("ejSchedule");

                    schObj.refresh(); // To refresh the Schedule control within the client side event
                    schObj.refreshAppointments();

                }
            });
        } catch (e) {

        }
        //var schObj = $("#ResourceGroupSchedule").data("ejSchedule");
        //var appointments = schObj.getAppointments();


    }


    $scope.WorkAllCenterPlanList = [];
    $scope.WorkCenterPlanList = [];
    $scope.SelectedWorlcenterForSummary = {};
    $scope.GetAllWorkcenterWisePlanningSummary = function () {
        try {

            $http({
                method: 'POST',
                url: "OrderManagements/productionOrderSchedulingParametersType1/GetAllWorkcenterWisePlanningType2Summary?EntityId=" + $scope.model.EntityId

            }).then(function successCallback(response) {
                $scope.WorkAllCenterPlanList = response.data;
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };
        } catch (e) {

            ShowResult(response.data.Message, 'failure');
        }

    }






    // The functions for the priority Update
    $scope.fileData = [];
    $scope.GetSample = function () {
        var reportFormat = "Excel";

        if (angular.isUndefinedOrNull($scope.EntityId)) {
            ShowResult("Please First Select the Entity!");
            throw ("Invalid");
        }

        try {
            window.open('OrderManagements/productionOrderSchedulingParametersType1/GetSampleReports?reportFormat=' + reportFormat + '&Entity=' + $scope.EntityId, '_blank');

        } catch (e) {

        }
    }

    $("#uploadFile").change(function () {
        $scope.fileData = this.files[0];
    });
    $scope.ExcelUploadData = [];
    //IMporting The Data From the Excel File

    $scope.ModelNew = {
        FileName: null
    }


    $scope.ImportData = function () {
        try {
            $scope.ExcelUploadData = [];
            $scope.msg = "";
            $scope.$broadcast('show-errors-check-validity');
            if ($scope.fileData.length == 0) {

                throw ("Please Select A File!!");
            }


            var fileData = new FormData();
            if (!baseService.isUndefinedOrNull($scope.fileData)) {
                $scope.ModelNew.FileName = $scope.fileData.name;
            }

            $http({
                method: 'POST',
                url: 'OrderManagements/productionOrderSchedulingParametersType1/ImportData',
                headers: { 'Content-Type': undefined },
                transformRequest: function (data) {
                    fileData.append("modelNew", angular.toJson(data.modelNew));
                    if (baseService.isUndefinedOrNull($scope.fileData) === false) {
                        fileData.append('file', data.file);

                    }
                    return fileData;
                },
                data: { 'modelNew': $scope.ModelNew, 'file': $scope.fileData }
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, "failure");

                }

                else {
                    try {
                        $scope.ExcelUploadData = response.data;
                    }

                    catch (e) {

                        ShowResult(e, "failure");
                    }

                }
            }, function errorCallback(response) {

            });
            return true;


        } catch (e) {

            ShowResult(e, "failure");
        }
    };

    //Save the File Data
    $scope.saveFileList = function () {

        $http({
            method: 'POST',
            url: 'OrderManagements/productionOrderSchedulingParametersType1/SaveFileList',
            data: { 'data': $scope.ExcelUploadData }
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, "failure");
            }
            else {
                try {
                    ShowResult(response.data.Message, 'success')
                }
                catch (e) {

                    ShowResult(e, "failure");
                }
            }
        });
    }

}