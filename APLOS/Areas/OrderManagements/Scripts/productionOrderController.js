'use strict';
ProductionOrderController.$inject = ["cboService", "commonMessage", "$scope", "$rootScope", "baseService", "$filter", "$window", "$http", "$controller"];
function ProductionOrderController(cboService, commonMessage, $scope, $rootScope, baseService, $filter, $window, $http, $controller) {
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
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    $scope.saveBulletinUrl = $scope.path + 'CreateProductionBulletin';

    $scope.saveProcessUrl = $scope.path + 'CreateProductionBulletinTemplateMaster';
    $scope.saveOperationUrl = $scope.path + 'CreateOperation';
    $scope.saveMachineUrl = $scope.path + 'updatemachine';
    $scope.saveSeqUrl = $scope.path + 'updatesequence';
    $scope.saveOperationMasterUrl = $scope.path + 'UpdateOperationMaster';

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

    //cboService.getCboProductionEntityByPlant(null, null, $window.plantId, function (result)
    //{
    //    $scope.entityList = result;
    //});

    $scope.GetPlanningTypeEntiy = function () {
        $http({
            method: 'GET',
            url: 'OrderManagements/ProductionOrder/GetPlanningTypeEntityCbo?processId=' + $scope.model.PlanningTypeProcessId
        }).then(function successCallback(response) {
            $scope.entityList = response.data;
        });
    }


    $scope.planningTypeProcessList = [];
    $scope.GetPlanningTypeProcess = function () {
        $http({
            method: 'GET',
            url: 'OrderManagements/ProductionOrder/GetPlanningTypeProcessCbo'
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

        $scope.DisableActionButtons = true;
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

        $scope.bulletintab = false;
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
            url: $scope.path + 'GetProductionRecipeMaterialList?productionOrderId=' + $scope.model.Id
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

            getProductionOrderEntityList();

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
            url: $scope.path + 'GetProductionOrderWorkCenterList?productionOrderId=' + $scope.model.Id
        }).then(function successCallback(response) {
            $scope.productionWorkCenterList = response.data;
            $scope.getProductionBulletinData($scope.model.Id);
            $scope.GetSavedWorkCenterListByEntityandFirstProcess();
        });
    }

    $scope.Save_BACKUP = function () {
        try {
            daysSortValidation($scope.prdProcessSetList);
            isJobWorkType($scope.prdProcessSetList);
            //isEntityExistInGrid($scope.prdProcessSetList, $scope.processSetNew.EntityId);

            var isBaseProcess = false;
            for (var i = 0; i < baseService.arrayLength($scope.prdProcessSetList); i++) {
                if ($scope.prdProcessSetList[i].IsBaseProcess) {
                    isBaseProcess = true;
                    break;
                }
                isBaseProcess = false;
            }
            if (!isBaseProcess) throw 'Please select base process';

            angular.copy($scope.model, $scope.model);
            $scope.$broadcast('show-errors-check-validity');
            if ($scope.modelNewForm.$valid) {
                if ($scope.Action === "Save") {
                    $http({
                        method: 'POST'
                        , url: $scope.saveUrl
                        , data: {
                            'master': $scope.model
                            , 'detaillist': $scope.productionMaterialList
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
                else if ($scope.Action === "Update") {
                    $http({
                        method: 'POST'
                        , url: $scope.updateUrl
                        , data: {
                            'master': $scope.model
                            , 'detaillist': $scope.productionMaterialList
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
                            $scope.model.Id = response.data.DATA;
                            $scope.getData();
                            //$scope.load(response.data.DATA);
                            // ClearFields();
                        }
                    }, function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    });
                }
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

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
                        'master': $scope.model
                        , 'detaillist': $scope.recipeMaterialListSelected
                        , 'processSetlist': $scope.prdProcessSetList
                        , 'entitylist': $scope.productionEntityList
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
                        getProductionProcessSetList();
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
        $scope.model = { PlantId: $window.plantId };
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
            'value': 'ArticleName'
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
        $http({
            method: 'GET',
            url: $scope.path + 'GetSalesOrderListSearch?column=' + $scope.recipeMaterialParameters.searchBy + '&value=' + $scope.recipeMaterialParameters.search + "&productionorderid=" + $scope.model.Id + "&EntityId=" + $scope.model.EntityId + "&ProcessId=" + $scope.model.PlanningTypeProcessId
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

    $scope.IsDefaultProcessSet = false;
    $scope.recipeMaterialListSelected = [];
    $scope.addRecipeMaterial = function () {

        try {
            var processSetId = "";
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
                    processSetId = $scope.recipeMaterialList[i].ProcessSetId;
                    $scope.IsDefaultProcessSet = $scope.recipeMaterialList[i].IsDefaultProcessSet;
                }
            }
            if (!baseService.isUndefinedOrNull(processSetId)) {
                getProcessSetList(processSetId);
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

    $scope.valuePassInDelModal = function (data, index) {
        $scope.index = index;
        $scope.message_confirmation = 'Are you sure want to delete [ ' + data.ProcessName + ' ]';
        angular.element(document.querySelector('#confirmDelPopUp')).modal('show');
    };
    $scope.processSetRemoveRow = function () {
        $scope.prdProcessSetList.splice($scope.index, 1);
        $scope.index = -1;
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
    $scope.model = { WCPreferenceType: 'INCLUDE' };
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
    //function getProductionOrderWorkCenterList() {
    //    $http({
    //        method: 'GET',
    //        url: $scope.path + 'GetProductionOrderWorkCenterList?productionOrderId=' + $scope.productionOrderModel.Id
    //    }).then(function successCallback(response) {
    //        $scope.productionWorkCenterList = response.data;
    //    });
    //}


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
            url: 'OrderManagements/ProductionOrder/GetSavedWorkCenterListByEntityandFirstProcess?productionOrderId=' + $scope.model.Id
        }).then(function successCallback(res) {
            $scope.productionFPWorkCenterList = res.data;
            if ($scope.model.IsPreDefineLotApplicable) {
                $scope.GetPOLotControlSettingsData();
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
    //#region Operatoin Thread Consumption
    $scope.businessThreadProcesses = "ThreadConsumption";
    $scope.materialType = null;

    // #region Needle Material Article Search By Business Process

    $scope.materialmasterSearchData = [];
    //$scope.searchList = [];
    //$scope.dataPlate = [];
    $scope.searchbyMaterialMasterDatalist = [
        {
            'name': 'Material Type',
            'value': 'MaterialTypeName'
        },
        {
            'name': 'Material Group',
            'value': 'MaterialGroupMasterName'
        },
        {
            'name': 'Code',
            'value': 'Code'
        },
        {
            'name': 'Product',
            'value': 'ProductMasterName'
        },
        {
            'name': 'User Name',
            'value': 'UserName'
        },
        {
            'name': 'Standard Name',
            'value': 'StandardName'
        },
        {
            'name': 'Short Name',
            'value': 'ShortName'
        },
        {
            'name': 'IsAsset',
            'value': 'IsAsset'
        },
        {
            'name': 'Asset Master',
            'value': 'AssetMasterName'
        },
        {
            'name': 'Budget Code',
            'value': 'AssetBudgetCode'
        },
        {
            'name': 'Activity',
            'value': 'ActivityName'
        },
        {
            'name': 'Id',
            'value': 'Id'
        }
    ];

    $scope.getNeedleMaterialMasterSearchData = function (args) {
        var gridObj = $("#GridBulMacOperation").data("ejGrid");
        $scope.bulletinTemplateDetailNew = gridObj.getSelectedRecords()[0];
        $scope.mmPopUpParameters = {
            limit: 10
            , offset: 0
            , order: 'asc'
            , sort: 'UserName'
            , searchBy: "UserName"
            , pageSize: 10
            , total_count: 0
            , search: null
            , serverPagination: true
        };
        $scope.materialTitle = 'Material';
        CloseShowResult();
        CloseModalShowResult();
        $scope.searchList = [];
        $scope.materialmasterSearchData = [];
        $scope.popUpUrl = 'Materials/MaterialMaster/MaterialSearchByBusinessProcess?type=' + $scope.businessThreadProcesses;
        baseService.setCurrentPage('materialmasterSearchData');
        $scope.loadMMData = function (pageno) {
            baseService.paginationBase($scope.popUpUrl, pageno, $scope.mmPopUpParameters)
                .then(function (result) {
                    $scope.materialmasterSearchData = result.Rows;
                    $scope.mmPopUpParameters.total_count = result.Total;
                    angular.element(document.querySelector('#NeedlematerialMasterbyTypePopup')).modal('show');
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure', 'popUpId');
                }).finally(function () {
                });
        };
        $scope.loadMMData();
    };

    $scope.setNeedleMaterialMasterData = function (ob) {
        $scope.bulletinTemplateDetailNew.NeedleMaterialMasterId = ob.Id;
        $scope.bulletinTemplateDetailNew.NeedleMaterialMaster = ob.UserName;
        $scope.bulletinTemplateDetailNew.NeedleArticleId = null;
        $scope.bulletinTemplateDetailNew.NeedleArticle = null;
        $scope.bulletinTemplateDetailNew.HasAttribute = ob.HasAttribute;

        if ($scope.bulletinTemplateDetailNew.HasAttribute) {
            $scope.materialType = null;
            $scope.getNeedleArticleSearchList(ob.Id);
        } else {
            $scope.closeNeedleMaterialMasterbyTypePopUp();
            return ShowResult('This material has no attribute', 'failure');
        }

        $scope.closeNeedleMaterialMasterbyTypePopUp();

    };

    $scope.closeNeedleMaterialMasterbyTypePopUp = function () {
        CloseModalShowResult('NeedlematerialMasterbyTypePopup');
        angular.element(document.querySelector('#NeedlematerialMasterbyTypePopup')).modal('hide');

    };

    $scope.ClearNeedleMaterialMasterSearchData = function (ob) {
        var gridObj = $("#GridBulMacOperation").data("ejGrid");
        $scope.bulletinTemplateDetailNew = gridObj.getSelectedRecords()[0];
        $scope.bulletinTemplateDetailNew.NeedleMaterialMasterId = null;
        $scope.bulletinTemplateDetailNew.NeedleMaterialMaster = null;
        $scope.bulletinTemplateDetailNew.NeedleArticleId = null;
        $scope.bulletinTemplateDetailNew.NeedleArticle = null;

        gridObj.refreshContent(true);
        gridObj.refreshTemplate();
    };

    $scope.getNeedleArticleSearchList = function (id) {
        try {
            CloseShowResult();
            CloseModalShowResult();
            $scope.articlePopUpParameters = {
                limit: 10
                , offset: 0
                , order: 'asc'
                , sort: 'StandardName'
                , searchBy: "StandardName"
                , pageSize: 10
                , total_count: 0
                , search: null
                , serverPagination: true
            };
            $scope.searchList = [];
            $scope.dataPlate = [];
            baseService.setCurrentPage('dataPlate');
            $scope.articlePopUpParameters.materialMasterId = id;
            $scope.articlePopUpParameters.materialType = JSON.stringify($scope.materialType);
            $scope.loadArticleData = function (pageno) {
                baseService.paginationBase('Materials/MaterialMasterArticle/GetMaterialArticle', pageno, $scope.articlePopUpParameters)
                    .then(function (result) {
                        //if (baseService.arrayLength(result.Rows) === 0) return ShowResult('This material has no article', 'failure');
                        $scope.dataPlate = result.Rows;
                        $scope.articlePopUpParameters.total_count = result.Total;
                        if (baseService.arrayLength($scope.searchList) === 0)
                            baseService.getDDLSearchColumn(result.Rows, $scope.searchList);
                        if ($scope.articlePopUpParameters.total_count == 0) {
                            ShowResult("This material has no article ", 'failure');
                        }
                        else {
                            angular.element(document.querySelector('#NeedlearticleSearchPop')).modal('show');
                        }

                    }, function () {
                        ShowResult(commonMessage.NetworkError, 'failure');
                    }).finally(function () {
                    });
            };
            $scope.loadArticleData();
        } catch (e) {
            ShowResult(e, '');
        }
    };
    $scope.closeMaterialArticlePopUp = function () {
        $scope.searchList = [];
        $scope.dataPlate = [];
        $scope.popUpUrl = '';
        CloseModalShowResult('NeedlearticleSearchPop');
        angular.element(document.querySelector('#NeedlearticleSearchPop')).modal('hide');
    };

    $scope.selectNeedlearticle = function (ob) {
        try {
            $scope.bulletinTemplateDetailNew.NeedleMaterialMasterId = ob.MaterialMasterId;
            $scope.bulletinTemplateDetailNew.NeedleMaterialMaster = ob.MaterialMasterName;
            $scope.bulletinTemplateDetailNew.NeedleArticleId = ob.Id;
            $scope.bulletinTemplateDetailNew.NeedleArticle = ob.StandardName;
            angular.element(document.querySelector('#NeedlearticleSearchPop')).modal('hide');

            var gridObj = $("#GridBulMacOperation").data("ejGrid");
            gridObj.refreshContent();

        } catch (e) {
            ShowResult(e, '', 'NeedlearticleSearchPop');
        }
    };

    // #endregion Needle Material Article Search

    // #region Bobbin Material Article Search By Business Process
    $scope.materialmasterSearchData = [];
    //$scope.searchList = [];
    //$scope.dataPlate = [];

    $scope.getBobbinMaterialMasterSearchData = function (args) {
        var gridObj = $("#GridBulMacOperation").data("ejGrid");
        $scope.bulletinTemplateDetailNew = gridObj.getSelectedRecords()[0];
        $scope.mmPopUpParameters = {
            limit: 10
            , offset: 0
            , order: 'asc'
            , sort: 'UserName'
            , searchBy: "UserName"
            , pageSize: 10
            , total_count: 0
            , search: null
            , serverPagination: true
        };
        $scope.materialTitle = 'Material';
        CloseShowResult();
        CloseModalShowResult();
        $scope.searchList = [];
        $scope.materialmasterSearchData = [];
        $scope.popUpUrl = 'Materials/MaterialMaster/MaterialSearchByBusinessProcess?type=' + $scope.businessThreadProcesses;
        baseService.setCurrentPage('materialmasterSearchData');
        $scope.loadMMData = function (pageno) {
            baseService.paginationBase($scope.popUpUrl, pageno, $scope.mmPopUpParameters)
                .then(function (result) {
                    $scope.materialmasterSearchData = result.Rows;
                    $scope.mmPopUpParameters.total_count = result.Total;
                    angular.element(document.querySelector('#BobbinmaterialMasterbyTypePopup')).modal('show');
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure', 'popUpId');
                }).finally(function () {
                });
        };
        $scope.loadMMData();
    };

    $scope.setBobbinMaterialMasterData = function (ob) {
        $scope.bulletinTemplateDetailNew.BobbinMaterialMasterId = ob.Id;
        $scope.bulletinTemplateDetailNew.BobbinMaterialMaster = ob.UserName;
        $scope.bulletinTemplateDetailNew.BobbinArticleId = null;
        $scope.bulletinTemplateDetailNew.BobbinArticle = null;
        $scope.bulletinTemplateDetailNew.HasAttribute = ob.HasAttribute;

        if ($scope.bulletinTemplateDetailNew.HasAttribute) {
            $scope.materialType = null;
            $scope.getBobbinArticleSearchList(ob.Id);
        } else {
            $scope.closeBobbinMaterialMasterbyTypePopUp();
            return ShowResult('This material has no attribute', 'failure');
        }

        $scope.closeBobbinMaterialMasterbyTypePopUp();

    };

    $scope.closeBobbinMaterialMasterbyTypePopUp = function () {
        CloseModalShowResult('BobbinmaterialMasterbyTypePopup');
        angular.element(document.querySelector('#BobbinmaterialMasterbyTypePopup')).modal('hide');

    };

    $scope.getBobbinArticleSearchList = function (id) {
        try {
            CloseShowResult();
            CloseModalShowResult();
            $scope.articlePopUpParameters = {
                limit: 10
                , offset: 0
                , order: 'asc'
                , sort: 'StandardName'
                , searchBy: "StandardName"
                , pageSize: 10
                , total_count: 0
                , search: null
                , serverPagination: true
            };
            $scope.searchList = [];
            $scope.dataPlate = [];
            baseService.setCurrentPage('dataPlate');
            $scope.articlePopUpParameters.materialMasterId = id;
            $scope.articlePopUpParameters.materialType = JSON.stringify($scope.materialType);
            $scope.loadArticleData = function (pageno) {
                baseService.paginationBase('Materials/MaterialMasterArticle/GetMaterialArticle', pageno, $scope.articlePopUpParameters)
                    .then(function (result) {
                        //if (baseService.arrayLength(result.Rows) === 0) return ShowResult('This material has no article', 'failure');
                        $scope.dataPlate = result.Rows;
                        $scope.articlePopUpParameters.total_count = result.Total;
                        if (baseService.arrayLength($scope.searchList) === 0)
                            baseService.getDDLSearchColumn(result.Rows, $scope.searchList);
                        if ($scope.articlePopUpParameters.total_count == 0) {
                            ShowResult("This material has no article ", 'failure');
                        }
                        else {
                            angular.element(document.querySelector('#BobbinarticleSearchPop')).modal('show');
                        }

                    }, function () {
                        ShowResult(commonMessage.NetworkError, 'failure');
                    }).finally(function () {
                    });
            };
            $scope.loadArticleData();
        } catch (e) {
            ShowResult(e, '');
        }
    };
    $scope.closeMaterialArticlePopUp = function () {
        $scope.searchList = [];
        $scope.dataPlate = [];
        $scope.popUpUrl = '';
        CloseModalShowResult('BobbinarticleSearchPop');
        angular.element(document.querySelector('#BobbinarticleSearchPop')).modal('hide');
    };

    $scope.selectBobbinarticle = function (ob) {
        try {
            $scope.bulletinTemplateDetailNew.BobbinMaterialMasterId = ob.MaterialMasterId;
            $scope.bulletinTemplateDetailNew.BobbinMaterialMaster = ob.MaterialMasterName;
            $scope.bulletinTemplateDetailNew.BobbinArticleId = ob.Id;
            $scope.bulletinTemplateDetailNew.BobbinArticle = ob.StandardName;
            angular.element(document.querySelector('#BobbinarticleSearchPop')).modal('hide');

            var gridObj = $("#GridBulMacOperation").data("ejGrid");
            gridObj.refreshContent();

        } catch (e) {
            ShowResult(e, '', 'BobbinarticleSearchPop');
        }
    };


    $scope.ClearBobbinMaterialMasterSearchData = function (ob) {
        var gridObj = $("#GridBulMacOperation").data("ejGrid");
        $scope.bulletinTemplateDetailNew = gridObj.getSelectedRecords()[0];
        $scope.bulletinTemplateDetailNew.BobbinMaterialMasterId = null;
        $scope.bulletinTemplateDetailNew.BobbinMaterialMaster = null;
        $scope.bulletinTemplateDetailNew.BobbinArticleId = null;
        $scope.bulletinTemplateDetailNew.BobbinArticle = null;
        gridObj.refreshContent(true);
        gridObj.refreshTemplate();
    };

    // #endregion Bobbin Material Article Search

    // #region Looper Material Article Search By Business Process
    $scope.materialmasterSearchData = [];
    //$scope.searchList = [];
    //$scope.dataPlate = [];

    $scope.getLooperMaterialMasterSearchData = function (args) {
        var gridObj = $("#GridBulMacOperation").data("ejGrid");
        $scope.bulletinTemplateDetailNew = gridObj.getSelectedRecords()[0];
        $scope.mmPopUpParameters = {
            limit: 10
            , offset: 0
            , order: 'asc'
            , sort: 'UserName'
            , searchBy: "UserName"
            , pageSize: 10
            , total_count: 0
            , search: null
            , serverPagination: true
        };
        $scope.materialTitle = 'Material';
        CloseShowResult();
        CloseModalShowResult();
        $scope.searchList = [];
        $scope.materialmasterSearchData = [];
        $scope.popUpUrl = 'Materials/MaterialMaster/MaterialSearchByBusinessProcess?type=' + $scope.businessThreadProcesses;
        baseService.setCurrentPage('materialmasterSearchData');
        $scope.loadMMData = function (pageno) {
            baseService.paginationBase($scope.popUpUrl, pageno, $scope.mmPopUpParameters)
                .then(function (result) {
                    $scope.materialmasterSearchData = result.Rows;
                    $scope.mmPopUpParameters.total_count = result.Total;
                    angular.element(document.querySelector('#LoopermaterialMasterbyTypePopup')).modal('show');
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure', 'popUpId');
                }).finally(function () {
                });
        };
        $scope.loadMMData();
    };

    $scope.setLooperMaterialMasterData = function (ob) {
        $scope.bulletinTemplateDetailNew.LooperMaterialMasterId = ob.Id;
        $scope.bulletinTemplateDetailNew.LooperMaterialMaster = ob.UserName;
        $scope.bulletinTemplateDetailNew.LooperArticleId = null;
        $scope.bulletinTemplateDetailNew.LooperArticle = null;
        $scope.bulletinTemplateDetailNew.HasAttribute = ob.HasAttribute;

        if ($scope.bulletinTemplateDetailNew.HasAttribute) {
            $scope.materialType = null;
            $scope.getLooperArticleSearchList(ob.Id);
        } else {
            $scope.closeLooperMaterialMasterbyTypePopUp();
            return ShowResult('This material has no attribute', 'failure');
        }

        $scope.closeLooperMaterialMasterbyTypePopUp();

    };

    $scope.closeLooperMaterialMasterbyTypePopUp = function () {
        CloseModalShowResult('LoopermaterialMasterbyTypePopup');
        angular.element(document.querySelector('#LoopermaterialMasterbyTypePopup')).modal('hide');

    };

    $scope.getLooperArticleSearchList = function (id) {
        try {
            CloseShowResult();
            CloseModalShowResult();
            $scope.articlePopUpParameters = {
                limit: 10
                , offset: 0
                , order: 'asc'
                , sort: 'StandardName'
                , searchBy: "StandardName"
                , pageSize: 10
                , total_count: 0
                , search: null
                , serverPagination: true
            };
            $scope.searchList = [];
            $scope.dataPlate = [];
            baseService.setCurrentPage('dataPlate');
            $scope.articlePopUpParameters.materialMasterId = id;
            $scope.articlePopUpParameters.materialType = JSON.stringify($scope.materialType);
            $scope.loadArticleData = function (pageno) {
                baseService.paginationBase('Materials/MaterialMasterArticle/GetMaterialArticle', pageno, $scope.articlePopUpParameters)
                    .then(function (result) {
                        //if (baseService.arrayLength(result.Rows) === 0) return ShowResult('This material has no article', 'failure');
                        $scope.dataPlate = result.Rows;
                        $scope.articlePopUpParameters.total_count = result.Total;
                        if (baseService.arrayLength($scope.searchList) === 0)
                            baseService.getDDLSearchColumn(result.Rows, $scope.searchList);
                        if ($scope.articlePopUpParameters.total_count == 0) {
                            ShowResult("This material has no article ", 'failure');
                        }
                        else {
                            angular.element(document.querySelector('#LooperarticleSearchPop')).modal('show');
                        }

                    }, function () {
                        ShowResult(commonMessage.NetworkError, 'failure');
                    }).finally(function () {
                    });
            };
            $scope.loadArticleData();
        } catch (e) {
            ShowResult(e, '');
        }
    };
    $scope.closeMaterialArticlePopUp = function () {
        $scope.searchList = [];
        $scope.dataPlate = [];
        $scope.popUpUrl = '';
        CloseModalShowResult('LooperarticleSearchPop');
        angular.element(document.querySelector('#LooperarticleSearchPop')).modal('hide');
    };

    $scope.selectLooperarticle = function (ob) {
        try {
            $scope.bulletinTemplateDetailNew.LooperMaterialMasterId = ob.MaterialMasterId;
            $scope.bulletinTemplateDetailNew.LooperMaterialMaster = ob.MaterialMasterName;
            $scope.bulletinTemplateDetailNew.LooperArticleId = ob.Id;
            $scope.bulletinTemplateDetailNew.LooperArticle = ob.StandardName;
            angular.element(document.querySelector('#LooperarticleSearchPop')).modal('hide');

            var gridObj = $("#GridBulMacOperation").data("ejGrid");
            gridObj.refreshContent();

        } catch (e) {
            ShowResult(e, '', 'LooperarticleSearchPop');
        }
    };

    $scope.ClearLooperMaterialMasterSearchData = function (ob) {
        var gridObj = $("#GridBulMacOperation").data("ejGrid");
        $scope.bulletinTemplateDetailNew = gridObj.getSelectedRecords()[0];
        $scope.bulletinTemplateDetailNew.LooperMaterialMasterId = null;
        $scope.bulletinTemplateDetailNew.LooperMaterialMaster = null;
        $scope.bulletinTemplateDetailNew.LooperArticleId = null;
        $scope.bulletinTemplateDetailNew.LooperArticle = null;
        gridObj.refreshContent(true);
        gridObj.refreshTemplate();
    };

    // #endregion Looper Material Article Search

    $scope.SaveThreadOperation = function () {
        try {
            if (baseService.arrayLength($scope.machineOperationList) < 0) {
                throw "Select Opearation.";
            }

            for (var i = 0; i < $scope.machineOperationList.length; i++) {
                $scope.machineOperationList[i].ProductionBulletinTemplateMasterId = $scope.BulletinTemplateMasterId;
            }
            $http({
                method: 'POST',
                url: $scope.saveOperationUrl,
                data: { 'entities': $scope.machineOperationList, 'productionBulletinTemplateMasterId': $scope.BulletinTemplateMasterId },

                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.getSavedMacnineOperationData($scope.BulletinTemplateMasterId);
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };

        } catch (e) {
            ShowResult(e, 'failure');
        }
    };



    //#endregion Operatoin Thread Consumption

    // #region  ProductionBulletin     

    // #region Cbo

    $scope.OperationVariationList = [];
    cboService.getOperationVariationCbo(function (response) {
        $scope.OperationVariationList = response;
    });

    $scope.OperationTypeList = [];
    cboService.getOperationTypeCbo(function (response) {
        $scope.OperationTypeList = response;
    });

    $scope.OperationConsumptionList = [];
    cboService.getOperationConsumptionCbo(function (response) {
        $scope.OperationConsumptionList = response;
    });

    $scope.OperationCategoryList = [];
    cboService.getOperationCategoryCbo(function (response) {
        $scope.OperationCategoryList = response;
    });

    $scope.MachineVariantList = [];
    cboService.getMachineVariantCbo(function (response) {
        $scope.MachineVariantList = response;
    });

    $scope.FGZoneList = [];
    cboService.getFGZoneCbo(function (response) {
        $scope.FGZoneList = response;
    });

    $scope.FGComponentList = [];
    cboService.getFGComponentCbo(function (response) {
        $scope.FGComponentList = response;
    });

    $scope.gaugeFolderList = [];
    cboService.getGaugeFolderCbo(function (response) {
        $scope.gaugeFolderList = response;
    });

    $scope.attachmentList = [];
    cboService.getAttachmentCbo(function (response) {
        $scope.attachmentList = response;
    });

    $scope.sizeGroupList = [];
    cboService.getSizeGroupCbo(function (response) {
        $scope.sizeGroupList = response;
    });

    $scope.stitchCodeList = [];
    $http.get('Machines/StitchCode/GetCbo')
        .then(function (response) {
            $scope.stitchCodeList = response.data;
        });

    $scope.productMasterList = [];
    cboService.getProductMasterCbo(function (response) {
        $scope.productMasterList = response.Rows;
    });


    $scope.processCboList = [];
    cboService.getProductionProcessCbo(function (response) {
        $scope.processCboList = response;
    });

    $scope.buyerCboList = [];
    cboService.getCboBuyer(function (response) {
        $scope.buyerCboList = response;
    });

    $scope.machineCboList = [];
    function getMachine(processId) {
        cboService.getMachineCbo(processId, function (response) {
            $scope.machineCboList = response;
        });
    }
    // #endregion


    // #region checkbox all

    $scope.refreshTemplateOperation = function (args) {
        $("#headchk").ejCheckBox({ "change": headCheckChangeOperation });
    };

    function headCheckChangeOperation(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        var filtered = $("#GridOperation").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.searchdata.length; i++) {
                $scope.searchdata[i].Active = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].CheckBoxSelect = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridOperation").data("ejGrid");
        gridObj.refreshContent();
    };


    // #endregion

    $scope.bulletinTemplate = {
        Id: null,
        CompanyGroupId: null,
        ProductionOrderId: null,
        BulletinName: null,
        AlternativeName: null,
        ByWhom: null,
        ProductMasterId: null,
        SizeGroupId: null
    }
    $scope.bulletinTemplateNew = Object.assign({}, $scope.bulletinTemplate);

    $scope.bulletinProcess = {
        Id: null,
        ProductionBulletinTemplateId: null,
        ProcessId: null,
        RequiredStdTarget: null,
        MaxNoOfWS: null,
        PlannedHoursPerDay: null,
        BottleNeckPercentage: null
    }
    $scope.bulletinProcessNew = Object.assign({}, $scope.bulletinProcess);

    $scope.sizeGroupList = [];
    cboService.getSizeGroupCbo(function (response) {
        $scope.sizeGroupList = response;
    });

    $scope.productMasterList = [];
    cboService.getProductMasterCbo(function (response) {
        $scope.productMasterList = response.Rows;
    });

    $scope.EditPRB = function () {
        angular.element(document.querySelector('#PRBPoUp')).modal('show');
    }
    $scope.UpdatePRB = function () {
        angular.copy($scope.bulletinTemplateNew, $scope.bulletinTemplate);
        $scope.$broadcast('show-errors-check-validity');
        try {
            if ($scope.PRBForm.$valid) {
                $http({
                    method: 'POST',
                    url: 'OrderManagements/ProductionOrder/EditProductionBulletin',
                    data: { 'data': $scope.bulletinTemplate },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');

                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.bulletinTemplateNew = response.data.Data;
                        $scope.ClosePRB();
                        $scope.getProdBulletinData();
                    }
                }, function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');

                });
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };
    $scope.ClosePRB = function () {
        angular.element(document.querySelector('#PRBPoUp')).modal('hide');
    }

    $scope.getProdBulletinData = function () {

        $http.get("OrderManagements/ProductionOrder/GetProductionBulletinDataByProductionOrder?productionOrderId=" + $scope.model.Id)
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.bulletinTemplateNew = Object.assign({}, response.data[0]);

                        if (!baseService.isUndefinedOrNull($scope.bulletinTemplateNew.PicFileName)) {
                            var str = $scope.bulletinTemplateNew.PicFileName;
                            var extention = str.substr(str.indexOf('.'));
                            $scope.PicFileName = virtualPath.ProductionBulletinImage + '/' + $scope.bulletinTemplateNew.Id + extention;
                        }

                        // $scope.PicFileName = virtualPath.ProductionBulletinImage + $scope.bulletinTemplateNew.PicFileName;
                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
    };

    $scope.bulletinList = [];
    $scope.getBulletinData = function (name) {
        try {
            if (baseService.isUndefinedOrNull($scope.model.Id)) {
                throw 'First take a product.';
            }
            $scope.bulletinList = [];
            if (name == 'BT') {
                //$http.get("OrderManagements/ProductionOrder/GetBulletinDataByProductMaster?productMasterId=" + $scope.model.ProductMasterId)
                $http.get("OrderManagements/ProductionOrder/GetBulletinDataByProductMaster")
                    .then(
                        function successCallback(response) {
                            if (baseService.arrayLength(response.data) > 0) {
                                $scope.bulletinList = response.data;
                            }
                            $scope.openBullPop(name);
                        },
                        function errorCallback(response) {
                            ShowResult(response, 'failure');
                        });
            } else {
                $http.get("OrderManagements/ProductionOrder/GetProductionOrderAndProdBulletinData")
                    .then(
                        function successCallback(response) {
                            if (baseService.arrayLength(response.data) > 0) {
                                $scope.bulletinList = response.data;
                            }
                            $scope.openBullPop(name);
                        },
                        function errorCallback(response) {
                            ShowResult(response, 'failure');
                        });
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.getProductionBulletinData = function (productionOrderId) {
        $scope.processList = [];
        $scope.operationList = [];
        $scope.bulletinTemplateNew = {};
        $http.get("OrderManagements/ProductionOrder/GetProductionBulletinDataByProductionOrder?productionOrderId=" + productionOrderId)
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.bulletinTemplateNew = Object.assign({}, response.data[0]);

                        if (!baseService.isUndefinedOrNull($scope.bulletinTemplateNew.PicFileName)) {
                            var str = $scope.bulletinTemplateNew.PicFileName;
                            var extention = str.substr(str.indexOf('.'));
                            $scope.PicFileName = virtualPath.ProductionBulletinImage + '/' + $scope.bulletinTemplateNew.Id + extention;
                        }

                        //$scope.PicFileName = virtualPath.ProductionBulletinImage + $scope.bulletinTemplateNew.PicFileName;
                        $scope.getProductionBulletinProcess($scope.bulletinTemplateNew.Id);
                    }
                    $scope.DisableActionButtons = false;
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                    $scope.DisableActionButtons = false;
                });
    };

    $scope.machineOperationList = [];
    $scope.getProductionBulletinProcess = function (bulletinTemplateId) {
        $scope.processList = [];
        // $scope.operationList = [];
        $http.get("OrderManagements/ProductionOrder/GetProductionBulletinProcess?bulletinTemplateId=" + bulletinTemplateId)
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.processList = response.data;
                        $scope.BulletinTemplateMasterId = $scope.processList[0].Id;
                        $scope.getProductionBulletinOperation($scope.BulletinTemplateMasterId);
                        $scope.Process = $scope.processList[0].Process;
                        $scope.ProcessId = $scope.processList[0].ProcessId;
                        $scope.PlannedHoursPerDay = $scope.processList[0].PlannedHoursPerDay;
                        $scope.RequiredStdTarget = $scope.processList[0].RequiredStdTarget;

                        for (var i = 0; i < $scope.processList.length; i++) {
                            var getrow = $filter("filter")($scope.prdProcessSetList, { "ProcessId": $scope.processList[i].ProcessId });
                            if (getrow.length > 0) {
                                $scope.processList[i].HasProcess = true;
                                getrow = [];
                            }
                        }

                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
    };

    $scope.onrowdatabound = function (e) {
        if (e.data.HasProcess === 0)
            e.row.css("background-color", "red");
    };

    $scope.bulletinTemplateDetail = {
        Id: null,
        ProductionBulletinTemplateMasterId: null,
        Sequence: null,
        OperationVariationId: null,
        OperationGroup: null,
        MachineVarientId: null,
        FGZoneId: null,
        FGComponentId: null,
        AdditionalSPT: null,
        TotalSPT: null,
        AllotedWorkstation: null,
        AllotedManpower: null,
        AttachmentId: null,
        GaugeFolderId: null,
        OperationConsumptionId: null,
        OperationTypeId: null,
        Frequency: null,
        Remark: null
    }
    $scope.bulletinTemplateDetailNew = Object.assign({}, $scope.bulletinTemplateDetail);

    $scope.getProcess = function (obj) {
        var gridObj = $("#GridProcess").data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        $scope.bulletinProcessNew = data;
        $scope.Process = $scope.bulletinProcessNew.Process;
        $scope.MaxNoOfWS = $scope.bulletinProcessNew.MaxNoOfWS;
        $scope.BulletinTemplateMasterId = $scope.bulletinProcessNew.Id;
        $scope.ProcessId = $scope.bulletinProcessNew.ProcessId;
        $scope.PlannedHoursPerDay = $scope.bulletinProcessNew.PlannedHoursPerDay;
        $scope.getProductionBulletinOperation($scope.BulletinTemplateMasterId);
    }

    $scope.getProductionBulletinOperation = function (bulletinTemplateMasterId) {
        $scope.operationList = [];
        $http.get("OrderManagements/ProductionOrder/GetProductionBulletinDetailData?bulletinTemplateMasterId=" + bulletinTemplateMasterId)
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.operationList = response.data;
                        $scope.CalculateGroup();
                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
    };

    $scope.openBullPop = function (name) {
        if (name == 'BT') {
            $("#BullPoUp").ejDialog("setTitle", "Bulletin");
            var eDialog = $("#BullPoUp").data("ejDialog");
            eDialog.open();
        } else {
            $("#ProdBullPoUp").ejDialog("setTitle", "Production Bulletin");
            var eDialog = $("#ProdBullPoUp").data("ejDialog");
            eDialog.open();
        }
    }

    $scope.closeBullPop = function () {
        var eDialog = $("#BullPoUp").data("ejDialog");
        eDialog.close();
    }

    $scope.closeProdBullPop = function () {
        var eDialog = $("#ProdBullPoUp").data("ejDialog");
        eDialog.close();
    }

    $scope.message_BulletinSave = null;
    //$scope.GetBulletin = function (obj) {
    //    $scope.bulletinTemplate = obj.data;
    //    $scope.bulletinTemplateNew = Object.assign({}, $scope.bulletinTemplate);
    //    if (!baseService.isUndefinedOrNull($scope.bulletinTemplateNew.Id))
    //        $scope.message_BulletinSave = 'This Bulletin [ ' + $scope.bulletinTemplateNew.BulletinName + ' ] will be copied and you can change it, Are you sure to save it?';
    //    angular.element(document.querySelector('#confirmBulletinSavePopUp')).modal('show');
    //}

    $scope.GetBulletin = function (obj) {
        $scope.bulletinTemplate = obj.data;
        $scope.bulletinTemplateNew = Object.assign({}, $scope.bulletinTemplate);
        if (!baseService.isUndefinedOrNull($scope.bulletinTemplateNew.Id))
            $scope.message_BulletinSave = 'This Bulletin [ ' + $scope.bulletinTemplateNew.BulletinName + ' ] will be copied and you can change it, Are you sure to save it?';

        var eDialog = $("#confirmBulletinSavePopUp").data("ejDialog");
        eDialog.open();
        $("#dialogAPI_wrapper").css({ 'position': 'fixed' }).css({ 'top': '200px' });
    };
    $scope.ConfirmBulletinSavePopUpClose = function () {
        var eDialog = $("#confirmBulletinSavePopUp").data("ejDialog");
        eDialog.close();
    };

    $scope.SaveBulletin = function () {
        $scope.bulletinTemplateNew.ProductionOrderId = $scope.model.Id;
        angular.copy($scope.bulletinTemplateNew, $scope.bulletinTemplate);
        try {
            $http({
                method: 'POST',
                url: $scope.saveBulletinUrl,
                data: $scope.bulletinTemplate,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.closeBullPop();
                    $scope.getProductionBulletinData($scope.model.Id);
                    $scope.getProductionBulletinProcess($scope.bulletinTemplateNew.Id);

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');

            };
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.SetProdBulletin = function (obj) {
        $scope.ProductionBulletinTemplateId = obj.data.ProductionBulletinTemplateId;

        if (!baseService.isUndefinedOrNull($scope.ProductionBulletinTemplateId))
            $scope.message_BulletinSave = 'This data will be copied and you can change it, Are you sure to save it?';

        var eDialog = $("#confirmProdBulletinSavePopUp").data("ejDialog");
        eDialog.open();
        $("#dialogAPI_wrapper").css({ 'position': 'fixed' }).css({ 'top': '200px' });
    };
    $scope.ConfirmprodBulletinSavePopUpClose = function () {
        var eDialog = $("#confirmProdBulletinSavePopUp").data("ejDialog");
        eDialog.close();
    };

    $scope.SaveProdBulletin = function () {
        $scope.ProductionOrderId = $scope.model.Id;
        try {
            $http({
                method: 'POST',
                url: 'OrderManagements/ProductionOrder/CopyProductionBulletin',
                data: { 'Id': $scope.ProductionBulletinTemplateId, 'ProductionOrderId': $scope.ProductionOrderId },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.closeProdBullPop();
                    $scope.getProductionBulletinData($scope.model.Id);
                    $scope.getProductionBulletinProcess($scope.bulletinTemplateNew.Id);

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');

            };
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };


    // #region Process

    $scope.processCboList = [];
    cboService.getProductionProcessCbo(function (response) {
        $scope.processCboList = response;
    });

    $scope.ProcessAction = 'Save';

    $scope.SaveProcess = function () {
        angular.copy($scope.bulletinProcessNew, $scope.bulletinProcess);
        $scope.bulletinProcess.ProductionBulletinTemplateId = $scope.bulletinTemplateNew.Id;
        $scope.$broadcast('show-errors-check-validity');
        try {
            if ($scope.ProcessForm.$valid) {
                if ($scope.ProcessAction === 'Save') {
                    $http({
                        method: 'POST',
                        url: $scope.saveProcessUrl,
                        data: $scope.bulletinProcess,
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure', 'ProcessPoUp');
                        }
                        else {
                            ShowResult(response.data.Message, 'success', 'ProcessPoUp');
                            $scope.getBulletinProcessData($scope.bulletinTemplateNew.Id);
                            angular.element(document.querySelector('#ProcessPoUp')).modal('hide');
                        }
                    }), function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure', 'ProcessPoUp');

                    };
                }
                else if ($scope.ProcessAction === 'Update') {
                    $http({
                        method: 'POST',
                        url: $scope.saveProcessUrl,
                        data: $scope.bulletinProcess,
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure', 'ProcessPoUp');
                        }
                        else {
                            ShowResult(response.data.Message, 'success', 'ProcessPoUp');
                            $scope.getBulletinProcessData($scope.bulletinTemplateNew.Id);
                            angular.element(document.querySelector('#ProcessPoUp')).modal('hide');
                        }
                    }, function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure', 'ProcessPoUp');
                    });
                }
            }
        } catch (e) {
            ShowResult(e, 'failure', 'ProcessPoUp');
        }
    };

    $scope.getProcessQtyAndNoWSData = function () {
        $http({
            method: 'GET',
            url: 'ie/bulletintemplate/getprocessqtyandnowsdata?processId=' + $scope.bulletinProcessNew.ProcessId + '&productMasterId=' + $scope.bulletinTemplateNew.ProductMasterId
        }).then(function successCallback(response) {
            if (baseService.arrayLength(response.data) > 0) {
                $scope.bulletinProcessNew.RequiredStdTarget = response.data[0].TargetQty;
                $scope.bulletinProcessNew.MaxNoOfWS = response.data[0].NoOfWorkStation;
            }
        });
    }

    $scope.processList = [];
    $scope.getBulletinProcessData = function (bulletinTemplateId) {
        $scope.processList = [];
        $http({
            method: 'GET',
            url: 'OrderManagements/ProductionOrder/GetProductionBulletinProcess?bulletinTemplateId=' + bulletinTemplateId
        }).then(function successCallback(response) {
            $scope.processList = response.data;

            for (var i = 0; i < $scope.processList.length; i++) {
                var getrow = $filter("filter")($scope.prdProcessSetList, { "ProcessId": $scope.processList[i].ProcessId });
                if (getrow.length > 0) {
                    $scope.processList[i].HasProcess = true;
                    getrow = [];
                }
            }

            $scope.getProductionBulletinOperation($scope.BulletinTemplateMasterId);
        });
    }

    $scope.AddNewProcess = function () {
        $scope.ProcessAction = 'Save';
        $scope.bulletinProcess = {};
        $scope.bulletinProcessNew = {};
        angular.element(document.querySelector('#ProcessPoUp')).modal('show');
    }
    $scope.CloseProcess = function () {
        angular.element(document.querySelector('#ProcessPoUp')).modal('hide');
    }

    $scope.EditProcess = function (obj) {
        var gridObj = $("#GridProcess").data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        $scope.bulletinProcessNew = data;
        $scope.Process = $scope.bulletinProcessNew.Process;
        $scope.ProcessAction = 'Update';
        angular.element(document.querySelector('#ProcessPoUp')).modal('show');
    }

    $scope.getProcess = function (obj) {
        var gridObj = $("#GridProcess").data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        $scope.bulletinProcessNew = data;
        $scope.Process = $scope.bulletinProcessNew.Process;
        $scope.MaxNoOfWS = $scope.bulletinProcessNew.MaxNoOfWS;
        $scope.BulletinTemplateMasterId = $scope.bulletinProcessNew.Id;
        $scope.ProcessId = $scope.bulletinProcessNew.ProcessId;
        $scope.PlannedHoursPerDay = $scope.bulletinProcessNew.PlannedHoursPerDay;

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
        $scope.MCtotalMPt = 0;
        $scope.NonMCtotalMP = 0;

        $scope.getProductionBulletinOperation($scope.BulletinTemplateMasterId);
        $scope.actionComplete();
    }

    $scope.message_confirmation = null;
    $scope.removeProcess = function (obj) {
        var gridObj = $("#GridProcess").data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        $scope.bulletinProcessNew = data;
        if (!baseService.isUndefinedOrNull($scope.bulletinProcessNew.Id))
            $scope.message_confirmation = 'Are you sure want to delete permanently [ ' + $scope.bulletinProcessNew.Process + ' ]';
        angular.element(document.querySelector('#confirmbulletinProcessPopUp')).modal('show');
    }

    $scope.DeleteProcess = function () {
        $http({
            method: 'POST',
            url: 'OrderManagements/ProductionOrder/DeleteProcess?id=' + $scope.bulletinProcessNew.Id
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.processList = [];
                $scope.getBulletinProcessData($scope.bulletinTemplateNew.Id);
            }
        }, function () {
            ShowResult(commonMessage.NetworkError, 'failure');
        }).finally(function () {
        });

    };


    // #endregion

    // #region Operation


    $scope.ShowResultCustom = function (message, type) {
        $("#OperationPoUp").ejDialog("setTitle", "Operation");
        var eDialog = $("#OperationPoUp").data("ejDialog");
        eDialog.open();
        var gridObj = $("#GridOperation").data("ejGrid");
        gridObj.clearFiltering();  // clears all the filtering
    };

    $scope.searchdata = [];
    $scope.GetOperationData = function () {
        $scope.searchdata = [];
        $http({
            method: 'GET',
            url: 'OrderManagements/ProductionOrder/getoperationdata?processId=' + $scope.ProcessId + '&bulletinTemplateId=' + $scope.bulletinTemplateNew.Id
        }).then(function successCallback(response) {
            $scope.searchdata = response.data;
        });
    }


    $scope.AddOperation = function () {
        $scope.searchdata = [];

        if (baseService.isUndefinedOrNull($scope.Process)) {
            return ShowResult('Select Process.', 'failure');
        }
        $scope.GetOperationData();
        $scope.ShowResultCustom();
    }

    $scope.operationList = [];
    function MakeData() {
        for (var i = 0; i < $scope.searchdata.length; i++) {
            if ($scope.searchdata[i].Active == true) {
                if (checkExists($scope.operationList, $scope.searchdata[i].OperationVariationId) === false) {
                    var ob = {};
                    ob.Id = null;
                    ob.ProductionBulletinTemplateMasterId = $scope.BulletinTemplateMasterId ? undefined || null : $scope.ProcessId;
                    ob.Sequence = null;
                    ob.OperationVariationId = $scope.searchdata[i].OperationVariationId;
                    ob.OperationMasterId = $scope.searchdata[i].OperationMasterId;
                    ob.OperationMasterCode = $scope.searchdata[i].OperationMasterCode;
                    ob.OperationGroup = null;
                    ob.MachineVarientId = $scope.searchdata[i].MachineVarientId;
                    ob.MaterialMaster = $scope.searchdata[i].MaterialMaster;
                    ob.MachineName = $scope.searchdata[i].Article;
                    ob.SkillId = $scope.searchdata[i].SkillId;
                    ob.FGZoneId = null;
                    ob.FGComponentId = null;
                    ob.Symbol = $scope.searchdata[i].AdditionalSAMSymbol;
                    ob.AdditionalSPT = null;
                    ob.AvgAllotedTime = null,
                        ob.VASSAMSOURCE = $scope.searchdata[i].VASSAMSOURCE;
                    ob.TotalSPT = $scope.searchdata[i].TotalSAM;
                    ob.OperationSPT = $scope.searchdata[i].TotalSAM;
                    ob.AllotedWorkstation = null;
                    ob.AllotedManpower = null;
                    ob.AttachmentId = null;
                    ob.GaugeFolderId = null;
                    ob.OperationConsumptionId = null;
                    ob.OperationTypeId = $scope.searchdata[i].OperationTypeId;
                    ob.Frequency = $scope.searchdata[i].Frequency;
                    ob.Remark = null;
                    ob.OperationVariation = $scope.searchdata[i].OperationVariation;
                    ob.OperationCode = $scope.searchdata[i].OperationCode;
                    ob.OperationId = $scope.searchdata[i].OperationId;
                    ob.OperationCategoryId = $scope.searchdata[i].OperationCategoryId;
                    ob.QualityLevel = null;
                    ob.SPI = $scope.searchdata[i].SPI;
                    ob.StitchCodeId = $scope.searchdata[i].StitchCodeId;
                    ob.NoOfStitch = 0;
                    ob.OperationLength = $scope.searchdata[i].OperationLength;
                    ob.FabricWidth = 0;

                    $scope.operationList.push(ob);
                } else {
                    throw "This Operation Variation " + $scope.searchdata[i].OperationVariation + " is already taken.";
                }
            }
        }
    }

    function checkExists(list, id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].OperationVariationId === id) {
                return true;
            }
        }
        return false;
    }

    $scope.CloseOperation = function () {
        try {
            MakeData();
            $scope.SaveOperation();
            var eDialog = $("#OperationPoUp").data("ejDialog");
            eDialog.close();
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.ViewOperation = function () {
        angular.element(document.querySelector('#OperationPoUp')).modal('show');
    }


    $scope.getSavedMacnineOperationData = function (bulletinTemplateMasterId) {
        $scope.machineOperationList = [];

        if (baseService.isUndefinedOrNull(bulletinTemplateMasterId)) {
            $scope.bulletinTemplateMasterId = $scope.BulletinTemplateMasterId;
        } else {
            $scope.bulletinTemplateMasterId = bulletinTemplateMasterId;
        }

        if (!baseService.isUndefinedOrNull($scope.bulletinTemplateMasterId)) {
            $http({
                method: 'GET',
                url: 'OrderManagements/ProductionOrder/GetBulletinMachineOperation?bulletinTemplateMasterId=' + $scope.bulletinTemplateMasterId
            }).then(function successCallback(response) {
                $scope.machineOperationList = response.data;
            });
        }

    };

    $scope.getOperationSPTByMachine = function (args) {
        if (!baseService.isUndefinedOrNull(args)) {

            var gridObj = $("#GridOperation").ejGrid("instance");
            var currRow = gridObj.model.currentViewData[this.element.closest("tr").index()];
            var x = args;

            for (var i = 0; i < $scope.machineCboList.length; i++) {
                if ($scope.machineCboList[i].Value === args.selectedValue) {
                    currRow.OperationSPT = $scope.machineCboList[i].OperationSPT;
                }
            }

        }
    }

    // #region  Machine Popup    

    $scope.ActionMachine = 'Save';

    $scope.openMachinePopup = function (args) {
        var gridObj = $("#GridBulOperation").data("ejGrid");
        $scope.data = gridObj.getSelectedRecords()[0];

        $scope.ActionMachine = 'Update';
        $scope.BulletinTemplateDetailId = $scope.data.Id;
        $scope.operationId = $scope.data.OperationId;
        $scope.MachineVarientId = $scope.data.MachineVarientId;
        $scope.SkillId = $scope.data.SkillId;

        $scope.operationVariationNew.ArticleId = $scope.data.MachineVarientId;
        $scope.operationVariationNew.ArticleName = $scope.data.MachineName;
        $scope.operationVariationNew.MaterialName = $scope.data.MaterialMaster;

        $scope.operationVariationNew.SkillId = $scope.data.SkillId;
        $scope.operationVariationNew.SkillName = $scope.data.SkillName;

        $scope.operationVariationNew.BasicProcessTime = $scope.data.BasicProcessTime;
        $scope.operationVariationNew.AssociateProcessTime = $scope.data.AssociateProcessTime;
        $scope.operationVariationNew.PersonalAllowance = $scope.data.PersonalAllowance;
        $scope.operationVariationNew.MachineAllowance = $scope.data.MachineAllowance;

        $scope.operationVariationNew.Frequency = $scope.data.Frequency;
        $scope.operationVariationNew.SPI = $scope.data.SPI;
        $scope.operationVariationNew.IsMachineRequired = $scope.data.IsMachineRequired;
        $scope.operationVariationNew.TotalSAM = $scope.data.TotalSAM;
        $scope.operationVariationNew.AdditionalSAMSymbol = $scope.data.AdditionalSAMSymbol;
        $scope.operationVariationNew.AdditionalSAM = $scope.data.AdditionalSAM;
        $scope.operationVariationNew.SubOperationSAM = $scope.data.SubOperationSAM;
        $scope.data.OperationSPT = $scope.data.TotalSAM;

        //  getOperationVariationUtilityData($scope.operationId, $scope.MachineVarientId, $scope.SkillId);

        angular.element(document.querySelector('#MachinePopUp')).modal('show');
    }

    $scope.closeMachinePopUp = function () {
        angular.element(document.querySelector('#MachinePopUp')).modal('hide');
    }

    // #endregion      

    // #region Material Master

    $scope.materialList = [];
    $scope.materialParameters = {
        limit: 10
        , offset: 0
        , order: 'asc'
        , sort: 'MaterialMasterName'
        , searchBy: "MaterialMasterName"
        , pageSize: 10
        , total_count: 0
        , search: null
        , serverPagination: true
    };

    //$scope.data = {};
    $scope.materialPopUp = function (args) {
        var gridObj = $("#GridBulOperation").data("ejGrid");
        $scope.data = gridObj.getSelectedRecords()[0];
        // getOperationVariationUtilityData($scope.data.OperationId);
        getOperationVariationUtilityData($scope.operationId, $scope.MachineVarientId, $scope.SkillId);
        $scope.materialDataList = [];
        $scope.materialUrl = 'Materials/MaterialMaster/GetCommonMachineListByProcess?processIds=' + baseService.getColumnValueList($scope.ProcessId, 'ProcessId');
        baseService.setCurrentPage('materialDataList');
        $scope.getMaterialData = function (pageno) {
            baseService.paginationBase($scope.materialUrl, pageno, $scope.materialParameters)
                .then(function (result) {
                    $scope.materialDataList = result.Rows;
                    $scope.materialParameters.total_count = result.Total;
                    if (baseService.arrayLength($scope.materialList) === 0) {
                        baseService.getDDLSearchColumn(result.Rows, $scope.materialList);
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure', 'materialId');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#materialId')).modal('show');
        $scope.getMaterialData();
    };
    $scope.closeMaterial = function () {
        $scope.popUpIndex = -1;
        angular.element(document.querySelector('#materialId')).modal('hide');
    };

    // #endregion MM

    // #region Article

    $scope.articleList = [];
    $scope.articleParameters = {
        limit: 10
        , offset: 0
        , order: 'asc'
        , sort: 'StandardName'
        , searchBy: "StandardName"
        , pageSize: 10
        , total_count: 0
        , search: null
        , serverPagination: true
    };

    $scope.articlePopUp = function (materialMasterId, materialMasterName, materialIndex) {
        try {
            var flag = false;

            //var opProcessIds = $.grep($scope.operationList, function (item) { return item.Value === $scope.operationVariationNew.OperationId; })[0].ProsessIds;
            var opProcessIds = $scope.ProcessId;

            var prosessIds = $scope.materialDataList[materialIndex].ProsessIds;
            if (!baseService.isUndefinedOrNull(prosessIds) && !baseService.isUndefinedOrNull(opProcessIds)) {
                var opProcessArray = opProcessIds.split(',');
                var processAray = prosessIds.split(',');
                for (var i = 0; i < baseService.arrayLength(processAray); i++) {
                    if (opProcessArray.indexOf(processAray[i]) !== -1) {
                        flag = true;
                        break;
                    }
                }
            }
            if (!flag) throw 'operation process and machine process not match ';
            $scope.excluedList = ['SkillName', 'MachineAllowance'];
            $scope.articleDataList = [];
            $scope.articleUrl = 'Machines/operation/GetArticleListByMaterialMaster?materialMasterId=' + materialMasterId;
            baseService.setCurrentPage('dataList');
            $scope.getarticleData = function (pageno) {
                baseService.paginationBase($scope.articleUrl, pageno, $scope.articleParameters)
                    .then(function (result) {
                        $scope.articleDataList = result.Rows;
                        $scope.articleParameters.total_count = result.Total;
                        if (baseService.arrayLength($scope.articleList) === 0) {
                            baseService.getDDLSearchColumn(result.Rows, $scope.articleList);
                        }
                    }, function () {
                        ShowResult(commonMessage.NetworkError, 'failure', 'articleId');
                    }).finally(function () {
                    });
            };
            angular.element(document.querySelector('#articleId')).modal('show');
            $scope.getarticleData();
        } catch (e) {
            ShowResult(e, '', 'materialId');
        }

    };

    $scope.selectArticle = function (data) {
        $scope.operationVariationNew.ArticleId = data.Id;
        $scope.operationVariationNew.ArticleName = data.StandardName;
        $scope.operationVariationNew.SkillId = data.SkillId;
        $scope.operationVariationNew.SkillName = data.SkillName;
        $scope.operationVariationNew.MachineAllowance = data.MachineAllowance;

        $scope.machine.MachineVarientId = $scope.operationVariationNew.ArticleId;
        $scope.machine.SkillId = $scope.operationVariationNew.SkillId;

        //getOperationVariationUtilityData($scope.operationId, $scope.operationVariationNew.ArticleId, $scope.operationVariationNew.SkillId);
        calculateSAM();
        $scope.closeArticle();
        $scope.closeMaterial();
    };
    $scope.closeArticle = function () {
        angular.element(document.querySelector('#articleId')).modal('hide');
    };

    // #endregion Article

    // #region Skill

    $scope.skillList = [];
    $scope.skillParameters = {
        limit: 10
        , offset: 0
        , order: 'asc'
        , sort: 'UserName'
        , searchBy: "UserName"
        , pageSize: 10
        , total_count: 0
        , search: null
        , serverPagination: true
    };
    $scope.skillPoUp = function () {
        var opProcessIds = $.grep($scope.operationList, function (item) { return item.Value === $scope.operationVariationNew.OperationId; })[0].ProsessIds;
        var opProcessArray = opProcessIds.split(',');
        $scope.excluedList = [];
        $scope.skillDataList = [];
        $scope.skillUrl = 'Skills/Skill/GetCommonSkillListByProcess?processIds=' + JSON.stringify(opProcessArray);
        baseService.setCurrentPage('dataList');
        $scope.getSkillData = function (pageno) {
            baseService.paginationBase($scope.skillUrl, pageno, $scope.skillParameters)
                .then(function (result) {
                    $scope.skillDataList = result.Rows;
                    $scope.skillParameters.total_count = result.Total;
                    if (baseService.arrayLength($scope.skillList) === 0) {
                        baseService.getDDLSearchColumn(result.Rows, $scope.skillList);
                    }
                    angular.element(document.querySelector('#skillId')).modal('show');
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure', 'skillId');
                }).finally(function () {
                });
        };
        $scope.getSkillData();
    };
    $scope.selectSkill = function (data) {
        $scope.operationVariationNew.ArticleId = null;
        $scope.operationVariationNew.ArticleName = null;
        $scope.operationVariationNew.SkillId = data.SkillId;
        $scope.operationVariationNew.SkillName = data.UserName;
        $scope.operationVariationNew.MachineAllowance = 0;
        $scope.closeSkill();
    };
    $scope.closeSkill = function () {
        $scope.popUpIndex = -1;
        angular.element(document.querySelector('#skillId')).modal('hide');
    };

    // #endregion Skill


    //#region  MaterialSummary

    $scope.threadMatrixList = [];
    $scope.GetMaterialSummary = function () {
        $scope.threadMatrixList = [];

        if (!baseService.isUndefinedOrNull($scope.bulletinTemplateMasterId)) {
            $http({
                method: 'GET',
                url: 'OrderManagements/ProductionOrder/GetThreadMatrixData?bulletinTemplateMasterId=' + $scope.bulletinTemplateMasterId
            }).then(function successCallback(response) {
                $scope.threadMatrixList = response.data;
            });
        }
        angular.element(document.querySelector('#MaterialSummaryPoUp')).modal('show');
    };


    //#endregion

    $scope.operationVariationNew = {
        ArticleId: null
        , ArticleName: null
        , SkillId: null
        , SkillName: null
        , BasicProcessTime: null
        , AssociateProcessTime: null
        , PersonalAllowance: null
        , MachineAllowance: null
        , Frequency: null
        , SPI: null
        , IsMachineRequired: null
        , AdditionalSAMSymbol: '+'
    }

    $scope.getTotalSam = function () {
        var total = eval($scope.operationVariationNew.SubOperationSAM + $scope.operationVariationNew.AdditionalSAMSymbol + $scope.operationVariationNew.AdditionalSAM);
        $scope.operationVariationNew.TotalSAM = total.toFixed(4);
    };

    function getOperationVariationUtilityData(operationId, articleId, skillId) {
        $http.get('machines/OperationVariation/GetUtilityByOperationData?operationId=' + operationId + '&articleId=' + articleId + '&skillId=' + skillId)
            .then(function (response) {
                $scope.operationVariationNew.ArticleId = response.data.ArticleId;
                $scope.operationVariationNew.ArticleName = response.data.ArticleName;
                $scope.operationVariationNew.MaterialName = response.data.MaterialName;

                $scope.operationVariationNew.SkillId = response.data.SkillId;
                $scope.operationVariationNew.SkillName = response.data.SkillName;

                $scope.operationVariationNew.BasicProcessTime = response.data.BasicProcessTime;
                $scope.operationVariationNew.AssociateProcessTime = response.data.AssociateProcessTime;
                $scope.operationVariationNew.PersonalAllowance = response.data.PersonalAllowance;
                $scope.operationVariationNew.MachineAllowance = response.data.MachineAllowance;

                $scope.operationVariationNew.Frequency = response.data.Frequency;
                $scope.operationVariationNew.SPI = response.data.SPI;
                $scope.operationVariationNew.IsMachineRequired = response.data.IsMachineRequired;
                $scope.operationVariationNew.TotalSAM = response.data.TotalSAM;
                $scope.operationVariationNew.AdditionalSAMSymbol = response.data.AdditionalSAMSymbol;
                $scope.operationVariationNew.AdditionalSAM = response.data.AdditionalSAM;
                $scope.operationVariationNew.SubOperationSAM = response.data.SubOperationSAM;
                $scope.data.OperationSPT = $scope.operationVariationNew.TotalSAM;


            });
    }

    function calculateSAM() {
        var firstSam = parseFloat($scope.operationVariationNew.BasicProcessTime) + parseFloat($scope.operationVariationNew.AssociateProcessTime);
        var sam = (firstSam * $scope.operationVariationNew.PersonalAllowance / 100
            + firstSam * $scope.operationVariationNew.MachineAllowance / 100) + firstSam;
        $scope.operationVariationNew.SAM = sam;
        $scope.operationVariationNew.SubOperationSAM = sam.toFixed(4);
        $scope.data.OperationSPT = sam.toFixed(4);
        for (var i = 0; i < $scope.operationList.length; i++) {
            if ($scope.operationList[i].OperationVariationId === $scope.data.OperationVariationId) {
                $scope.operationList[i].OperationSPT = parseFloat($scope.data.OperationSPT);
                $scope.operationList[i].MachineVarientId = $scope.operationVariationNew.ArticleId;
            }
        }

        var gridObj = $("#GridBulOperation").data("ejGrid");
        gridObj.refreshContent(true);

    }

    $scope.MCtotalspt = 0;
    $scope.NonMCtotalspt = 0;
    $scope.TotalMP = 0;
    $scope.MCtotalMP = 0;
    $scope.NonMCtotalMP = 0;

    $scope.CalculateGroup = function () {
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

        var MaxNumber = 0;
        var MaxNumberIndex = -1;
        var totalspt = 0;
        var totalMP = 0;
        var MCtotalspt = 0;
        var NonMCtotalspt = 0;
        var aatarray = [];
        var TotalWoS = 0;

        var TotalMP = 0;
        var MCtotalMP = 0;
        var NonMCtotalMP = 0;

        if (baseService.arrayLength($scope.operationList) > 0) {
            for (var i = 0; i < $scope.operationList.length; i++) {
                if (!baseService.isUndefinedOrNull($scope.operationList[i].OperationGroup)) {
                    $scope.SPTSum = $filter("sumByKey")($filter("filter")($scope.operationList, { "OperationGroup": $scope.operationList[i].OperationGroup }), "TotalSPT");
                    $scope.AMSum = $filter("sumByKey")($filter("filter")($scope.operationList, { "OperationGroup": $scope.operationList[i].OperationGroup }), "AllotedManpower");
                    $scope.operationList[i].AvgAllotedTime = ($scope.SPTSum / $scope.AMSum).toFixed(2);
                } else {
                    if ($scope.operationList[i].AllotedManpower !== 0) {
                        $scope.operationList[i].AvgAllotedTime = ($scope.operationList[i].TotalSPT / $scope.operationList[i].AllotedManpower).toFixed(2);
                    } else {
                        $scope.operationList[i].AvgAllotedTime = 0;
                    }
                }
                $scope.operationList[i].OperationTargetPerHr = Math.round(60 / $scope.operationList[i].TotalSPT);
                $scope.operationList[i].RequiredManPower = ($scope.RequiredStdTarget / (60 / $scope.operationList[i].TotalSPT)).toFixed(2);

                $scope.operationList[i].IsMaxAllottedTime = false;
                if (parseFloat($scope.operationList[i].AvgAllotedTime) > MaxNumber) {
                    MaxNumber = parseFloat($scope.operationList[i].AvgAllotedTime);
                    MaxNumberIndex = i;
                }

                totalspt = totalspt + $scope.operationList[i].TotalSPT;
                totalMP = totalMP + $scope.operationList[i].AllotedManpower;
                TotalWoS = TotalWoS + $scope.operationList[i].AllotedWorkstation;

                if (!baseService.isUndefinedOrNull($scope.operationList[i].MachineVarientId)) {
                    MCtotalspt = MCtotalspt + $scope.operationList[i].TotalSPT;
                    MCtotalMP = MCtotalMP + $scope.operationList[i].AllotedManpower;
                }

                if (baseService.isUndefinedOrNull($scope.operationList[i].MachineVarientId)) {
                    NonMCtotalspt = NonMCtotalspt + $scope.operationList[i].TotalSPT;
                    NonMCtotalMP = NonMCtotalMP + $scope.operationList[i].AllotedManpower;
                }


                aatarray.push(parseFloat($scope.operationList[i].AvgAllotedTime));

            }

            //$scope.operationList[MaxNumberIndex].IsMaxAllottedTime = true;

            var pitchTime = (totalspt / totalMP).toFixed(2);
            var avgat = Math.max.apply(null, aatarray);

            for (var i = 0; i < $scope.operationList.length; i++) {
                if (parseFloat($scope.operationList[i].AvgAllotedTime) == avgat) {
                    $scope.operationList[i].IsMaxAllottedTime = true;
                }
            }

            var ob = {};
            ob.PitchTime = pitchTime;
            ob.MaxAllottedTime = avgat;
            ob.OrganizationEfficiency = (ob.PitchTime / ob.MaxAllottedTime).toFixed(2);
            ob.ProductionEfficiencyPerHour = ((totalMP * 60) / totalspt).toFixed(2);
            ob.ProductionEfficiencyPerDay = (ob.ProductionEfficiencyPerHour * $scope.PlannedHoursPerDay).toFixed(2);
            ob.LineTargetPerHour = (ob.ProductionEfficiencyPerHour * ob.OrganizationEfficiency).toFixed(2);

            $scope.PitchTime = pitchTime;
            $scope.MaxAllottedTime = avgat;
            $scope.OrganizationEfficiency = (ob.PitchTime / ob.MaxAllottedTime).toFixed(2);
            $scope.ProductionEfficiencyPerHour = Math.round(((totalMP * 60) / totalspt).toFixed(2));
            $scope.ProductionEfficiencyPerDay = Math.round((ob.ProductionEfficiencyPerHour * $scope.PlannedHoursPerDay).toFixed(2));
            $scope.LineTargetPerHour = Math.round((ob.ProductionEfficiencyPerHour * ob.OrganizationEfficiency).toFixed(2));

            $scope.TotalSPT = totalspt.toFixed(2);
            $scope.TotalManpower = totalMP.toFixed(2);
            $scope.TotalWorkStation = TotalWoS;

            $scope.MCtotalspt = MCtotalspt.toFixed(2);
            $scope.NonMCtotalspt = NonMCtotalspt.toFixed(2);

            $scope.MCtotalMP = MCtotalMP.toFixed(2);
            $scope.NonMCtotalMP = NonMCtotalMP.toFixed(2);

        }

        var gridObj = $("#GridBulOperation").data("ejGrid");
        gridObj.refreshContent(true);
        gridObj.refreshTemplate();
    };

    $window.onresize = function (event) {
        $scope.actionComplete();
    };

    $scope.actionComplete = function (args) {
        try {
            if (args.requestType === "refresh") {
                var gridObj = $("#GridBulOperation").ejGrid("instance");
                var scrollerwidth = $("#bulletin").width();//Obtain the width of the container
                gridObj.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth - 20, height: 300, width: 1080 } });//pass the obtainer width and height to gridmodel options
                gridObj.windowonresize();
            }
        } catch (e) {
            // $scope.ShowResultCustom(e, 'failure');
        }
    };

    $scope.processCountList = [];
    $scope.TotalSPT = 0;
    $scope.TotalManpower = 0;
    $scope.TotalWorkStation = 0;

    function CheckSequence() {
        var arr = [];
        for (var i = 0; i < $scope.operationList.length; i++) {
            if (!baseService.isUndefinedOrNull($scope.operationList[i].Sequence)) {
                if (checkExistsSS(arr, $scope.operationList[i].Sequence) === false) {
                    arr.push($scope.operationList[i].Sequence);
                }
                else {
                    throw "Sequence " + $scope.operationList[i].Sequence + " is exists for " + $scope.operationList[i].OperationVariation + ".";
                }
            }
        }
    }


    function checkExistsSS(list, id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i] === id) {
                return true;
            }
        }
        return false;
    }

    $scope.SaveOperation = function () {
        try {
            if (baseService.arrayLength($scope.operationList) < 0) {
                throw "Select Opearation.";
            }
            CheckSequence();
            for (var i = 0; i < $scope.operationList.length; i++) {
                $scope.operationList[i].ProductionBulletinTemplateMasterId = $scope.BulletinTemplateMasterId;
            }
            $http({
                method: 'POST',
                url: $scope.saveOperationUrl,
                data: { 'entities': $scope.operationList, 'productionBulletinTemplateMasterId': $scope.BulletinTemplateMasterId },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.getProductionBulletinOperation($scope.BulletinTemplateMasterId);

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };

        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.machine = {
        Id: null,
        MachineVarientId: null,
        SkillId: null
    }

    $scope.UpdateMachine = function () {
        try {

            $scope.machine.Id = $scope.BulletinTemplateDetailId;
            $scope.machine.MachineVarientId = $scope.operationVariationNew.ArticleId;
            $scope.machine.SkillId = $scope.operationVariationNew.SkillId;

            $http({
                method: 'POST',
                url: $scope.saveMachineUrl,
                data: $scope.machine,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');

                    $scope.getProductionBulletinOperation($scope.BulletinTemplateMasterId);
                    $scope.machine = {};
                    angular.element(document.querySelector('#MachinePopUp')).modal('hide');
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };

        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.removeProcessOperation = function (obj) {
        var gridObj = $("#GridBulOperation").data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        $scope.bulletinTemplateDetail = data;
        if (!baseService.isUndefinedOrNull($scope.bulletinTemplateDetail.Id))
            $scope.message_confirmation = 'Are you sure want to delete permanently [' + $scope.bulletinTemplateDetail.OperationVariation + ' ]';
        angular.element(document.querySelector('#confirmProcessOperationPopUp')).modal('show');
    }

    $scope.DeleteProcessOperation = function () {
        $http({
            method: 'POST',
            url: 'OrderManagements/ProductionOrder/DeleteOperation?id=' + $scope.bulletinTemplateDetail.Id
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');

                $scope.getProductionBulletinOperation($scope.BulletinTemplateMasterId);

            }
        }, function () {
            ShowResult(commonMessage.NetworkError, 'failure');
        }).finally(function () {
        });
    };

    $scope.GetSequence = function () {
        var gridObj = $("#GridBulOperation").data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        $scope.bulletinTemplateDetailNew = Object.assign({}, data);
        angular.element(document.querySelector('#SeqPopup')).modal('show');
    }
    $scope.closeSeqPopUp = function () {
        angular.element(document.querySelector('#SeqPopup')).modal('hide');
    }

    $scope.UpdateSequence = function () {
        try {
            $http({
                method: 'POST',
                url: $scope.saveSeqUrl,
                data: $scope.bulletinTemplateDetailNew,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.getProductionBulletinOperation($scope.BulletinTemplateMasterId);
                    angular.element(document.querySelector('#SeqPopup')).modal('hide');
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };

        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    // #endregion Operation       

    $scope.message_PBconfirmation = null;
    $scope.RemoveBulletin = function () {

        if (!baseService.isUndefinedOrNull($scope.model.Id))
            $scope.message_PBconfirmation = 'Are you sure?';
        angular.element(document.querySelector('#confirmPBProcessPopUp')).modal('show');
    }

    $scope.DeleteProductionBulletin = function () {
        $http({
            method: 'POST',
            url: 'OrderManagements/ProductionOrder/DeleteProductionBulletin?ProductionOrderId=' + $scope.model.Id
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');

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
                $scope.MCtotalMP = 0;
                $scope.TotalMP = 0;
                $scope.MCtotalMPt = 0;
                $scope.NonMCtotalMP = 0;
                $scope.getProductionBulletinData($scope.model.Id);
            }
        }, function () {
            ShowResult(commonMessage.NetworkError, 'failure');
        }).finally(function () {
        });

    };

    // #region OperationMaster

    $scope.OperationMasterList = [];
    $scope.Operation = null;
    $scope.showOperationPopUp = function (args) {
        var gridObj = $("#GridBulOperation").data("ejGrid");
        $scope.modeldata = gridObj.getSelectedRecords()[0];

        $scope.Operation = "Operation Master";
        $http.get('employees/EmployeeInformation/GetOperationMaster')
            .then(function (response) {
                $scope.OperationMasterList = [];
                $scope.OperationMasterList = response.data;
            });

        angular.element(document.querySelector('#OperationPopUp')).modal('show');
    };

    $scope.SetOperation = function (args) {
        var gridObj = $("#OperationMasterGrid").data("ejGrid");
        $scope.data = gridObj.getSelectedRecords()[0];
        $scope.modeldata.OperationMasterId = $scope.data.Id;
        $scope.modeldata.OperationMasterCode = $scope.data.Code;
        $scope.UpdateOperationMaster();
        angular.element(document.querySelector('#OperationPopUp')).modal('hide');

    }


    $scope.UpdateOperationMaster = function () {
        try {
            $http({
                method: 'POST',
                url: $scope.saveOperationMasterUrl,
                data: $scope.modeldata,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.getProductionBulletinOperation($scope.BulletinTemplateMasterId);
                    angular.element(document.querySelector('#SeqPopup')).modal('hide');
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };

        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    // #endregion

    // #endregion  ProductionBulletin

    // #region checkbox all for delete multi Operation

    $scope.refreshTemplateDelOperation = function (args) {
        $("#headchk").ejCheckBox({ "change": headCheckChangeOperation });
    };

    function headCheckChangeOperation(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        var filtered = $("#GridBulOperation").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.operationList.length; i++) {
                $scope.operationList[i].DelFlag = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].CheckBoxSelect = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridBulOperation").data("ejGrid");
        gridObj.refreshContent();
    };

    $scope.idList = [];
    $scope.sqlInStatement = null;
    $scope.CountDelItem = 0;
    function MakeMultiDeleteData() {
        $scope.CountDelItem = 0;
        for (var di = 0; di < $scope.operationList.length; di++) {
            if ($scope.operationList[di].DelFlag == true) {
                $scope.idList.push($scope.operationList[di]);
                $scope.CountDelItem++;
            }
        }

        if ($scope.idList.length > 0) {
            var uniqueMasterOrderId = removeDuplicates($scope.idList, 'Id');
            var wcEmpCode = "";
            if (uniqueMasterOrderId.length > 0) {
                wcEmpCode = "IN(";
                wcEmpCode += Array.prototype.map.call(uniqueMasterOrderId, function (item) { return "'" + item.Id + "'"; }).join(",") + ")";
            }
            $scope.sqlInStatement = wcEmpCode;
        }

    }

    function removeDuplicates(myArr, prop) {
        return myArr.filter((obj, pos, arr) => {
            return arr.map(mapObj => mapObj[prop]).indexOf(obj[prop]) === pos;
        });
    }

    $scope.removeMultiOperation = function () {
        MakeMultiDeleteData();
        if (!baseService.isUndefinedOrNull($scope.sqlInStatement))
            $scope.message_multi_confirmation = 'Are you sure want to delete permanently "' + $scope.CountDelItem + '" operations.';
        angular.element(document.querySelector('#confirmMultiOperationPopUp')).modal('show');
    }

    $scope.DeleteMultiOperation = function () {
        $http({
            method: 'POST',
            url: 'OrderManagements/ProductionOrder/DeleteMultiOperation?id=' + $scope.sqlInStatement
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.sqlInStatement = null;
                $scope.getProductionBulletinOperation($scope.BulletinTemplateMasterId);

            }
        }, function () {
            ShowResult(commonMessage.NetworkError, 'failure');
        }).finally(function () {
        });
    };

    // #endregion

    //#region Production Bulletin Picture upload

    $scope.onBeginPBUpload = function (args) {
        try {
            if (angular.isUndefinedOrNull($scope.bulletinTemplateNew.Id))
                throw 'Please select/save the production order first'

            args.data = $scope.bulletinTemplateNew.Id;
        } catch (e) {

            args.cancel = true;
            ShowResult(e, 'Error');
        }

    }
    $scope.uploadPBUrl = "OrderManagements/ProductionOrder/SaveBulletinDefault";
    $scope.fileselect = function (e) {

    }
    $scope.errorPBPicUpload = function (e) {
        if (angular.isUndefinedOrNull($scope.bulletinTemplateNew.Id))
            ShowResult('Please select/save the production order first', 'Error');
        else
            ShowResult("The selected file size is too large. Please select a file less than " + Math.round(e.model.fileSize / (1024 * 1024)) + "MB", 'failure');
    }
    $scope.getFileList = function () {
        $http({
            method: 'POST', url: $scope.path + 'GetFileInfo', dataType: 'JSON',
            data: { Id: $scope.bulletinTemplateNew.Id }
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult('error', 'failure');
            }
            else {
                var str = response.data[0].PicFileName;
                var extention = str.substr(str.indexOf('.'));
                $scope.PicFileName = virtualPath.ProductionBulletinImage + '/' + $scope.bulletinTemplateNew.Id + extention;
            }
        }, function errorCallback(response) {
            ShowResult('Failed', 'failure');
        });
    }
    //#endregion Production Bulletin Picture upload

    //#region MultiOperationCode
    $scope.MultiCodeList = [];
    $scope.AddMultiCode = function () {
        $scope.MultiCodeList = [];
        angular.element(document.querySelector('#AddMultiOperationCodePoUp')).modal('show');
    }
    $scope.CloseMultiCode = function () {
        angular.element(document.querySelector('#AddMultiOperationCodePoUp')).modal('hide');
    }
    $scope.Go = function () {

        var Sequenc = $scope.operationList.length + 1;

        var res = $scope.bulletinTemplateDetailNew.OperationCode.split(" ");
        for (var i = 0; i < res.length; i++) {
            if (checkCodeExists($scope.operationList, res[i]) === false) {
                var obj = {};
                obj.Sequenc = Sequenc + i;
                obj.OperationCode = res[i];
                $scope.MultiCodeList.push(obj);
            }
        }
        res = [];

    }
    function checkCodeExists(list, OperationCode) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].OperationCode === OperationCode) {
                return true;
            }
        }
        return false;
    }


    function checkSeqExists(list, Seq) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].Sequence === Seq) {
                return true;
            }
        }
        return false;
    }


    $scope.UpdateMultiCode = function () {
        try {
            if (baseService.arrayLength($scope.MultiCodeList) == 0) {
                throw "Code is required.";
            }
            for (var i = 0; i < $scope.MultiCodeList.length; i++) {
                if (checkSeqExists($scope.operationList, $scope.MultiCodeList[i].Sequenc)) {
                    throw "This Sequence '" + $scope.MultiCodeList[i].Sequenc + "' is exists";
                }
            }

            $http({
                method: 'POST',
                url: 'OrderManagements/ProductionOrder/InsertMultiOperation',
                data: { 'Code': $scope.bulletinTemplateDetailNew.OperationCode, 'processId': $scope.ProcessId, 'bulletinTemplateMasterId': $scope.BulletinTemplateMasterId, 'MultiCodeList': $scope.MultiCodeList },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.MultiCodeList = [];
                    $scope.CloseMultiCode();
                    $scope.bulletinTemplateDetailNew.OperationCode = null;
                    $scope.getProductionBulletinOperation($scope.BulletinTemplateMasterId);
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure', 'AddMultiOperationCodePoUp');
            };



        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    //#endregion MultiOperationCode

    //#region 

    $scope.GetOperationVaiationCode = function () {
        var gridObj = $("#GridBulOperation").data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        $scope.bulletinTemplateDetailNew = Object.assign({}, data);
        angular.element(document.querySelector('#OperationVaiationCodePopup')).modal('show');
    }

    $scope.closeOperationVaiationCodePopUp = function () {
        angular.element(document.querySelector('#OperationVaiationCodePopup')).modal('hide');
    }
    $scope.UpdateOperationVaiationCodeUrl = 'OrderManagements/ProductionOrder/UpdateOperationVaiationCode';
    $scope.UpdateOperationVaiationCode = function () {
        try {

            if (checkCodeExists($scope.operationList, $scope.bulletinTemplateDetailNew.OperationVaiationCode) === false) {
                $http({
                    method: 'POST',
                    url: $scope.UpdateOperationVaiationCodeUrl,
                    data: { 'bulletinTemplateDetail': $scope.bulletinTemplateDetailNew, 'processId': $scope.ProcessId, 'bulletinTemplateMasterId': $scope.BulletinTemplateMasterId },

                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.getProductionBulletinOperation($scope.BulletinTemplateMasterId);
                        angular.element(document.querySelector('#OperationVaiationCodePopup')).modal('hide');
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                };
            } else {
                throw "This Operation Code " + $scope.bulletinTemplateDetailNew.OperationCode + " is already taken.";
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    //#endregion

    //#region start ProductionBulletinTemplate Reports
    $scope.onClickDetaillPrint = function () {
        var reportFormat = "Excel";
        try {
            //var url=   window.open('IE/bulletintemplate/GetProductionBulletinTemplateReport?reportFormat=' + reportFormat + '&ProductionOrderId=' + $scope.model.Id, '_blank');
            var url = 'IE/bulletintemplate/GetBulletinTamplateProductionDetailReport?reportFormat=' + reportFormat + '&ProductionOrderId=' + $scope.model.Id;

            $rootScope.report(url);
        } catch (e) {

        }
    };

    $scope.onClickSummaryPrint = function () {
        var reportFormat = "Excel";
        try {
            //var url=   window.open('IE/bulletintemplate/GetProductionBulletinTemplateReport?reportFormat=' + reportFormat + '&ProductionOrderId=' + $scope.model.Id, '_blank');
            var url = 'IE/bulletintemplate/GetProductionBulletinTamplateSummaryReport?reportFormat=' + reportFormat + '&ProductionOrderId=' + $scope.model.Id;

            $rootScope.report(url);
        } catch (e) {

        }
    };
    //#endregion end ProductionBulletinTemplate Reports

    //#region ---Report---

    $scope.GetBulletinTamplate2ndIndexReport = function () {
        var reportFormat = "Excel";
        try {
            var url = 'IE/bulletintemplate/GetBulletinTamplate2ndIndexReport?reportFormat=' + reportFormat + '&ProductionId=' + $scope.model.Id;

            $rootScope.report(url);
        } catch (e) {

        }
    };

    //#endregion

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


}