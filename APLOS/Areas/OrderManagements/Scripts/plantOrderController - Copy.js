'use strict';
ProductionOrderController.$inject = ["cboService", "commonMessage", "$scope", "$rootScope", "baseService", "$filter", "$window", "$http"];
function ProductionOrderController(cboService, commonMessage, $scope, $rootScope, baseService, $filter, $window, $http) {
    $rootScope.title = "Production Order";
    $scope.Action = 'Save';
    $scope.index = -1;

    $scope.processList = [];
    $scope.operationList = [];
    $scope.modelList = [];
    $scope.productionMaterialList = [];
    $scope.prdProcessSetList = [];
    $scope.productionEntityList = [];
    $scope.productionWorkCenterList = [];
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
    $scope.getData = function () {
        $scope.modelList = [];
        $http({
            method: 'GET',
            data: { 'parameters': null },
            url: $scope.getListUrl + "?column=" + $scope.searchCol + "&value=" + $scope.searchVal
        }).then(function successCallback(response) {
            $scope.modelList = response.data;
        });
    };
    $scope.getData();


    //cboService.getCboProductionEntityByPlant(null, null, $window.plantId, function (result)
    //{
    //    $scope.entityList = result;
    //});
    $scope.getAllEntities = function () {
        $http({
            method: 'POST',
            url: 'OrderManagements/productionOrderSchedulingParametersType1/GetEntity'
        }).then(function successCallback(response) {
            $scope.entityList = response.data;
        });
    }
    $scope.getAllEntities();
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

        if ($scope.MaterialID != e.data.ProductionGrouping + e.data.MaterialMasterId) {
            $scope.isAlternative = $scope.isAlternative * -1;
            $scope.MaterialID = e.data.ProductionGrouping + e.data.MaterialMasterId;
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
        $scope.getProductionBulletinData($scope.model.Id);
        $scope.bulletintab = false;

        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };
    $scope.Load = function (Row) {
        //$scope.index = index;
        //$scope.model = Row.data;
        //$scope.model = Object.assign({}, $scope.model);
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
    $scope.Save = function () {
        try {


            if (baseService.isUndefinedOrNull($scope.recipeMaterialListSelected) || $scope.recipeMaterialListSelected.length <= 0)
                throw 'Please select at least one material';


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
                        //, 'UploadDefault': push
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


                        //var uploadObj = $("#UploadDefault").data("ejUploadbox");
                        //uploadObj.element.find('.e-uploadinput').click();

                        //ClearFields();
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
        $scope.model = { PlantId: $window.plantid };
        $scope.processList = [];
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
    }
    $scope.Clear();

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    // #region Recipe Material and SO

    $scope.recipeMaterialFilterList = [
        {
            'name': 'Master Order No',
            'value': 'MasterOrderNo'
        },
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

    $scope.summaryRows = [{
        title: "Total Qty", summaryColumns: [{ summaryType: ej.Grid.SummaryType.Sum, displayColumn: "Qty", dataMember: "Qty", format: "{0:N0}" }],
        showCaptionSummary: true

    }];
    $scope.serachSoMaterial = function serachSoMaterial() {
        $http({
            method: 'GET',
            url: $scope.path + 'GetSalesOrderListSearch?column=' + $scope.recipeMaterialParameters.searchBy + '&value=' + $scope.recipeMaterialParameters.search + "&productionorderid=" + $scope.model.Id
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



                    if ($scope.recipeMaterialList[i].ProductionGrouping != groupid)
                        throw "Selecting different group materials are not allowed";

                    if ($scope.recipeMaterialList[i].ProductID != productid)
                        throw "Selecting different products are not allowed";

                    if ($scope.recipeMaterialList[i].ArticleId != id)
                        throw "Selecting different articles are not allowed";






                }
            }



            $scope.recipeMaterialListSelected = [];
            for (var i = 0; i < $scope.recipeMaterialList.length; i++) {
                if ($scope.recipeMaterialList[i].Checked == true) {


                    $scope.recipeMaterialListSelected.push($scope.recipeMaterialList[i]);
                }
            }

            $scope.CloseRecipeMaterialPopUp();
        } catch (e) {
            ShowResult(e, 'failure', 'recipeMaterialPopUp');
        }


    };

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
            url: 'Processes/processset/GetProcessSetList?processSetId=' + id
        }).then(function successCallback(response) {
            $scope.prdProcessSetList = response.data;
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


        $scope.popUpProcessUrl = 'Processes/Process/GetProductionProcessList';
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

        });
    };
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
        angular.element(document.querySelector('#popUpId')).modal('show');
        $scope.index = index;
        $scope.getPopUpData();
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

    $scope.workCenterFilterList = [
        {
            'name': 'Code',
            'value': 'Code'
        },
        {
            'name': 'Work Center',
            'value': 'UserName'
        }
    ];

    $scope.workCenterParameters = {
        limit: 10
        , offset: 0
        , order: 'asc'
        , sort: 'UserName'
        , searchBy: 'UserName'
        , pageSize: 10
        , total_count: 0
        , search: null
        , serverPagination: true
    };

    $scope.workCenterPopUp = function () {
        $rootScope.tempList = [];
        $rootScope.workCenterList = [];
        angular.forEach($scope.productionWorkCenterList, function (a) {
            $rootScope.tempList.push({
                Id: a.Id
                , WorkCenterMasterId: a.WorkCenterMasterId
                , ProductionOrderId: a.ProductionOrderId
                , Code: a.Code
                , UserName: a.UserName
                , Flag: true
            });
        });
        baseService.setCurrentPage('workCenterList');
        $scope.workCenterParameters.entityIds = baseService.getColumnValueList($scope.productionEntityList, 'EntityId');
        $scope.getWorkCenterData = function (pageno) {
            baseService.paginationBase($scope.path + 'GetWorkCenterList', pageno, $scope.workCenterParameters)
                .then(function (result) {
                    $scope.workCenterList = result;
                    $scope.workCenterParameters.total_count = result.total;
                    for (var t = 0; t < baseService.arrayLength($scope.workCenterList); t++) {
                        $scope.workCenterList[t].Flag = baseService.valueCheckInList($rootScope.tempList, 'WorkCenterMasterId', $scope.workCenterList[t].WorkCenterMasterId);
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#workCenterPopUp')).modal('show');
        $scope.getWorkCenterData();
    };

    $scope.addWorkCenter = function () {
        if (baseService.arrayLength($rootScope.tempList) > 0) {
            angular.forEach($rootScope.tempList, function (a) {
                if (!baseService.valueCheckInList($scope.productionWorkCenterList, 'WorkCenterMasterId', a.WorkCenterMasterId)) {
                    $scope.productionWorkCenterList.push({
                        Id: null
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
        $scope.CloseWorkCenterPopUp();
    };

    $scope.CloseWorkCenterPopUp = function () {
        angular.element(document.querySelector('#workCenterPopUp')).modal('hide');
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

    // #region  ProductionBulletin     

    // #region checkbox all

    angular.isUndefinedOrNull = function (val) {
        return angular.isUndefined(val) || val === null || val === ""
    }
    function checkChangeOperation(e) {

        var val = e.model.value;
        //item level check
        var row = $filter('filter')($scope.searchdata, { 'OperationVariationId': e.model.value });
        if (!baseService.isUndefinedOrNull(row) && row.length > 0) {
            if (e.model.checkState == "check")
                row[0].Active = true;
            else
                row[0].Active = false;
        }

    }
    function headCheckChangeOperation(e) {
        if (e.model.checkState == "check") {

            // var gridObj = $("#GridOperation").data("ejGrid");
            var filtered = $("#GridOperation").data("ejGrid").getFilteredRecords();
            if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
                for (var i = 0; i < $scope.searchdata.length; i++) {
                    $scope.searchdata[i].Active = true;
                }
            }
            else {
                for (var i = 0; i < $scope.searchdata.length; i++) {
                    for (var j = 0; j < filtered.length; j++) {
                        if ($scope.searchdata[i].OperationVariationId == filtered[j].OperationVariationId)
                            $scope.searchdata[i].Active = true;
                    }

                }
            }

            var checkbox = $("#GridOperation .rowCheckbox").ejCheckBox();
            for (var i = 0; i < checkbox.length; i++) {
                $($("#GridOperation .rowCheckbox")[i]).ejCheckBox({ "change": null });
                $($("#GridOperation .rowCheckbox")[i]).ejCheckBox({ "checked": true });
                $($("#GridOperation .rowCheckbox")[i]).ejCheckBox({ "change": checkChangeOperation });
            }
        }
        else {
            var filtered = $("#GridOperation").data("ejGrid").getFilteredRecords();
            if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
                for (var i = 0; i < $scope.searchdata.length; i++) {
                    $scope.searchdata[i].Active = false;
                }
            }
            else {
                for (var i = 0; i < $scope.searchdata.length; i++) {
                    for (var j = 0; j < filtered.length; j++) {
                        if ($scope.searchdata[i].OperationVariationId == filtered[j].OperationVariationId)
                            $scope.searchdata[i].Active = false;
                    }

                }
            }
            var checkbox = $("#GridOperation .rowCheckbox").ejCheckBox();
            for (var i = 0; i < checkbox.length; i++) {
                $($("#GridOperation .rowCheckbox")[i]).ejCheckBox({ "change": null });
                $($("#GridOperation .rowCheckbox")[i]).ejCheckBox({ "checked": false });
                $($("#GridOperation .rowCheckbox")[i]).ejCheckBox({ "change": checkChangeOperation });
            }
        }
        //header level check
    }
    $scope.dataBoundOperation = function (args) {
        $("#GridOperation .rowCheckbox").ejCheckBox({ "change": checkChange });
        $("#headchk").ejCheckBox({ "change": headCheckChangeOperation });

    }
    $scope.refreshTemplateOperation = function (args) {
        if (args.rowIndex == 0) {
            $("#headchk").ejCheckBox({ "change": headCheckChangeOperation });
        }

        var valobj = $($("#GridOperation .rowCheckbox")[args.rowIndex]).ejCheckBox()[0];
        var val = $($("#GridOperation .rowCheckbox")[args.rowIndex]).ejCheckBox()[0].defaultValue;

        $($("#GridOperation .rowCheckbox")[args.rowIndex]).ejCheckBox({ "change": null });
        var row = $filter('filter')($scope.searchdata, { 'OperationVariationId': val });
        if (!baseService.isUndefinedOrNull(row) && row.length > 0) {
            if (row[0].Active == true)
                $($("#GridOperation .rowCheckbox")[args.rowIndex]).ejCheckBox({ "checked": true });
            else
                $($("#GridOperation .rowCheckbox")[args.rowIndex]).ejCheckBox({ "checked": false });

        }
        $($("#GridOperation .rowCheckbox")[args.rowIndex]).ejCheckBox({ "change": checkChangeOperation });
    }

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
        PlannedHoursPerDay: null
    }
    $scope.bulletinProcessNew = Object.assign({}, $scope.bulletinProcess);

    $scope.bulletinList = [];
    $scope.getBulletinData = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.model.Id)) {
                throw 'First take a product.';
            }
            $scope.bulletinList = [];
            $http.get("OrderManagements/ProductionOrder/GetBulletinDataByProductMaster?productMasterId=" + $scope.model.ProductMasterId)
                .then(
                    function successCallback(response) {
                        if (baseService.arrayLength(response.data) > 0) {
                            $scope.bulletinList = response.data;
                        }
                        $scope.openBullPop();
                    },
                    function errorCallback(response) {
                        ShowResult(response, 'failure');
                    });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.getProductionBulletinData = function (productionOrderId) {
        $scope.processList = [];
        $scope.bulletinTemplateNew = {};
        $http.get("OrderManagements/ProductionOrder/GetProductionBulletinDataByProductionOrder?productionOrderId=" + productionOrderId)
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.bulletinTemplateNew = Object.assign({}, response.data[0]);
                        $scope.getProductionBulletinProcess($scope.bulletinTemplateNew.Id);
                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
    };


    $scope.getProductionBulletinProcess = function (bulletinTemplateId) {
        $scope.processList = [];
        $http.get("OrderManagements/ProductionOrder/GetProductionBulletinProcess?bulletinTemplateId=" + bulletinTemplateId)
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.processList = response.data;
                        $scope.getProductionBulletinOperation($scope.processList[0].Id);
                        $scope.Process = $scope.processList[0].Process;
                        $scope.ProcessId = $scope.processList[0].ProcessId;

                        for (var i = 0; i < $scope.processList.length; i++) {
                            var getrow = $filter("filter")($scope.prdProcessSetList, { "ProcessId": $scope.processList[i].ProcessId });
                            if (getrow.length>0) {
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
        $scope.bulletinTemplateDetailNew.ProductionBulletinTemplateMasterId = $scope.bulletinProcessNew.Id;
        $scope.BulletinTemplateMasterId = $scope.bulletinProcessNew.Id;
        $scope.ProcessId = $scope.bulletinProcessNew.ProcessId;
        $scope.PlannedHoursPerDay = $scope.bulletinProcessNew.PlannedHoursPerDay;
        $scope.getSavedOperationData($scope.bulletinTemplateDetailNew.ProductionBulletinTemplateMasterId);
    }

    $scope.getProductionBulletinOperation = function (ProductionBulletinTemplateId) {
        $scope.operationList = [];
        $http.get("OrderManagements/ProductionOrder/GetProductionBulletinDetailData?ProductionBulletinTemplateId=" + ProductionBulletinTemplateId)
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.operationList = response.data;
                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
    };

    $scope.openBullPop = function () {
        $("#BullPoUp").ejDialog("setTitle", "Bulletin");
        var eDialog = $("#BullPoUp").data("ejDialog");
        eDialog.open();
    }

    $scope.closeBullPop = function () {
        var eDialog = $("#BullPoUp").data("ejDialog");
        eDialog.close();
    }

    $scope.message_BulletinSave = null;
    $scope.GetBulletin = function (obj) {
        $scope.bulletinTemplate = obj.data;
        $scope.bulletinTemplateNew = Object.assign({}, $scope.bulletinTemplate);
        if (!baseService.isUndefinedOrNull($scope.bulletinTemplateNew.Id))
            $scope.message_BulletinSave = 'This Bulletin [ ' + $scope.bulletinTemplateNew.BulletinName + ' ] will be copied and you can change it, Are you sure to save it?';
        angular.element(document.querySelector('#confirmBulletinSavePopUp')).modal('show');
    }

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
                            throw response.data.Message;
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
            $scope.getSavedOperationData($scope.bulletinTemplateDetailNew.BulletinTemplateMasterId);
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
        $scope.bulletinTemplateDetailNew.BulletinTemplateMasterId = $scope.bulletinProcessNew.Id;
        $scope.BulletinTemplateMasterId = $scope.bulletinProcessNew.Id;
        $scope.ProcessId = $scope.bulletinProcessNew.ProcessId;
        $scope.PlannedHoursPerDay = $scope.bulletinProcessNew.PlannedHoursPerDay;
        $scope.getSavedOperationData($scope.bulletinTemplateDetailNew.BulletinTemplateMasterId);
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
            url: 'OrderManagements/ProductionOrder/getoperationdata?processId=' + $scope.ProcessId
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
                    ob.ProductionBulletinTemplateMasterId = $scope.bulletinTemplateDetailNew.ProductionBulletinTemplateMasterId ? undefined || null : $scope.ProcessId;
                    ob.Sequence = null;
                    ob.OperationVariationId = $scope.searchdata[i].OperationVariationId;
                    ob.OperationGroup = null;
                    ob.MachineVarientId = $scope.searchdata[i].MachineVarientId;
                    ob.MaterialMaster = $scope.searchdata[i].MaterialMaster;
                    ob.MachineName = $scope.searchdata[i].Article;
                    ob.SkillId = $scope.searchdata[i].SkillId;
                    ob.FGZoneId = null;
                    ob.FGComponentId = null;
                    ob.Symbol = $scope.searchdata[i].AdditionalSAMSymbol;
                    ob.AdditionalSPT = null;
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
                    $scope.operationList.push(ob);
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
        MakeData();

        $scope.SaveOperation();

        var eDialog = $("#OperationPoUp").data("ejDialog");
        eDialog.close();
    }

    $scope.ViewOperation = function () {
        angular.element(document.querySelector('#OperationPoUp')).modal('show');
    }

    $scope.getSavedOperationData = function (bulletinTemplateMasterId) {
        if (baseService.isUndefinedOrNull(bulletinTemplateMasterId)) {
            $scope.bulletinTemplateMasterId = $scope.BulletinTemplateMasterId;
        } else {
            $scope.bulletinTemplateMasterId = bulletinTemplateMasterId;
        }
        $scope.operationList = [];
        $http({
            method: 'GET',
            url: 'OrderManagements/ProductionOrder/getbulletinoperation?bulletinTemplateMasterId=' + $scope.bulletinTemplateMasterId
        }).then(function successCallback(response) {
            $scope.operationList = response.data;

            for (var i = 0; i < $scope.operationList.length; i++) {
                if (!baseService.isUndefinedOrNull($scope.operationList[i].OperationGroup)) {
                    $scope.SPTSum = $filter("sumByKey")($filter("filter")($scope.operationList, { "OperationGroup": $scope.operationList[i].OperationGroup }), "TotalSPT");
                    $scope.AMSum = $filter("sumByKey")($filter("filter")($scope.operationList, { "OperationGroup": $scope.operationList[i].OperationGroup }), "AllotedManpower");
                    $scope.operationList[i].AvgAllotedTime = ($scope.SPTSum / $scope.AMSum).toFixed(4);
                } else {
                    $scope.operationList[i].AvgAllotedTime = ($scope.operationList[i].TotalSPT / $scope.operationList[i].AllotedManpower).toFixed(4);
                }
            }

            if (baseService.arrayLength($scope.operationList) > 0) {
                $scope.GetProcessCountData();
            }
        });
    }

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
        getOperationVariationUtilityData($scope.operationId, $scope.MachineVarientId, $scope.SkillId);

        angular.element(document.querySelector('#MachinePopUp')).modal('show');
    }

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
            $scope.data.MaterialMaster = materialMasterName;
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
        $scope.data.MachineName = data.StandardName;
        $scope.operationVariationNew.SkillId = data.SkillId;
        $scope.operationVariationNew.SkillName = data.SkillName;
        $scope.operationVariationNew.MachineAllowance = data.MachineAllowance;

        $scope.machine.MachineVarientId = $scope.operationVariationNew.ArticleId;
        $scope.machine.SkillId = $scope.operationVariationNew.SkillId;

        getOperationVariationUtilityData($scope.operationId, $scope.operationVariationNew.ArticleId, $scope.operationVariationNew.SkillId);
        //calculateSAM();
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

    function getOperationUtilityData(operationId) {
        $http.get('machines/operation/getOperationUtilityData?operationId=' + operationId)
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

    $scope.CalculateGroup = function () {
        if (baseService.arrayLength($scope.operationList) > 0) {
            for (var i = 0; i < $scope.operationList.length; i++) {
                if (!baseService.isUndefinedOrNull($scope.operationList[i].OperationGroup)) {
                    $scope.SPTSum = $filter("sumByKey")($filter("filter")($scope.operationList, { "OperationGroup": $scope.operationList[i].OperationGroup }), "TotalSPT");
                    $scope.AMSum = $filter("sumByKey")($filter("filter")($scope.operationList, { "OperationGroup": $scope.operationList[i].OperationGroup }), "AllotedManpower");
                    $scope.operationList[i].AvgAllotedTime = ($scope.SPTSum / $scope.AMSum).toFixed(4);
                } else {
                    $scope.operationList[i].AvgAllotedTime = ($scope.operationList[i].TotalSPT / $scope.operationList[i].AllotedManpower).toFixed(4);
                }

            }
            var gridObj = $("#GridBulOperation").data("ejGrid");
            gridObj.refreshContent(true);
        }
    }

    $window.onresize = function (event) {
        $scope.actionComplete();
    };

    $scope.actionComplete = function (args) {
        try {
            if (args.requestType === "refresh") {
                var gridObj = $("#GridBulOperation").ejGrid("instance");
                var scrollerwidth = $("#bulletin").width();//Obtain the width of the container
                gridObj.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth - 20, height: 300, width: 1060 } });//pass the obtainer width and height to gridmodel options
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
    $scope.GetProcessCountData = function () {
        var totalspt = 0;
        var TotalWoS = 0;
        var totalMP = 0;
        for (var i = 0; i < $scope.operationList.length; i++) {
            totalspt = totalspt + $scope.operationList[i].TotalSPT;
            totalMP = totalMP + $scope.operationList[i].AllotedManpower;
            TotalWoS = TotalWoS + $scope.operationList[i].AllotedWorkstation;
        }
        var ob = {};
        ob.TotalSPT = totalspt.toFixed(4);
        ob.TotalManpower = totalMP;
        ob.TotalWorkStation = $scope.MaxNoOfWS;

        $scope.TotalSPT = totalspt.toFixed(4);
        $scope.TotalManpower = totalMP;
        $scope.TotalWorkStation = TotalWoS;

        //$scope.processCountList.push(ob);
        //ob = {};
    }

    $scope.processPitchList = [];
    $scope.GetProcessPitchCountData = function () {
        var totalspt = 0;
        var totalMP = 0;
        var aatarray = [];
        for (var i = 0; i < $scope.operationList.length; i++) {
            totalspt = totalspt + $scope.operationList[i].TotalSPT;
            totalMP = totalMP + $scope.operationList[i].AllotedManpower;
            aatarray.push(parseFloat($scope.operationList[i].AvgAllotedTime));
        }
        var pitchTime = (totalspt / totalMP).toFixed(4);
        var avgat = Math.max.apply(null, aatarray);
        var ob = {};
        ob.PitchTime = pitchTime;
        ob.MaxAllottedTime = avgat;
        ob.OrganizationEfficiency = (ob.MaxAllottedTime / ob.PitchTime).toFixed(4);
        ob.ProductionEfficiencyPerHour = ((totalMP * 60) / totalspt).toFixed(4);
        ob.ProductionEfficiencyPerDay = (ob.ProductionEfficiencyPerHour * $scope.PlannedHoursPerDay).toFixed(4);
        ob.LineTargetPerHour = (ob.ProductionEfficiencyPerHour * ob.OrganizationEfficiency).toFixed(4);
        ob.LineTargetPerDay = (ob.ProductionEfficiencyPerDay * ob.OrganizationEfficiency).toFixed(4);


        $scope.PitchTime = pitchTime;
        $scope.MaxAllottedTime = avgat;
        $scope.OrganizationEfficiency = (ob.MaxAllottedTime / ob.PitchTime).toFixed(4);
        $scope.ProductionEfficiencyPerHour = ((totalMP * 60) / totalspt).toFixed(4);
        $scope.ProductionEfficiencyPerDay = (ob.ProductionEfficiencyPerHour * $scope.PlannedHoursPerDay).toFixed(4);
        $scope.LineTargetPerHour = (ob.ProductionEfficiencyPerHour * ob.OrganizationEfficiency).toFixed(4);
        $scope.LineTargetPerDay = (ob.ProductionEfficiencyPerDay * ob.OrganizationEfficiency).toFixed(4);

        // $scope.processPitchList.push(ob);
        //ob = {};
        angular.element(document.querySelector('#PitchPopup')).modal('show');
    }

    $scope.closePitchPopUp = function () {
        angular.element(document.querySelector('#PitchPopup')).modal('hide');
    }

    $scope.SaveOperation = function () {
        try {
            if (baseService.arrayLength($scope.operationList) < 0) {
                throw "Select Opearation.";
            }

            for (var i = 0; i < $scope.operationList.length; i++) {
                $scope.operationList[i].ProductionBulletinTemplateMasterId = $scope.bulletinTemplateDetailNew.BulletinTemplateMasterId;
            }
            $http({
                method: 'POST',
                url: $scope.saveOperationUrl,
                data: { 'entities': $scope.operationList, 'productionBulletinTemplateMasterId': $scope.bulletinTemplateDetailNew.BulletinTemplateMasterId },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    throw response.data.Message;
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.operationList = [];
                    $scope.getSavedOperationData($scope.BulletinTemplateMasterId);
                    $scope.GetProcessCountData();
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
                    throw response.data.Message;
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.operationList = [];
                    $scope.getSavedOperationData($scope.BulletinTemplateMasterId);
                    $scope.GetProcessCountData();
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
                $scope.operationList = [];
                $scope.getSavedOperationData($scope.BulletinTemplateMasterId);
            }
        }, function () {
            ShowResult(commonMessage.NetworkError, 'failure');
        }).finally(function () {
        });
    };

    // #endregion Operation       

    // #endregion  ProductionBulletin
}