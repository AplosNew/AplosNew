'use strict';
ProductivityRecoveryMasterController.$inject = ["cboService", "commonMessage", "$scope", "$rootScope", "baseService", "$routeParams", "$location", "$http", "$filter"];
function ProductivityRecoveryMasterController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "ProductivityRecoveryMaster";
    $scope.Action = 'Save';
    $scope.path = 'OrderManagements/ProductivityRecoveryMaster/';
    $scope.saveUrl = $scope.path + 'create';
    $scope.saveUrlBudgetCode = $scope.path + 'createBudgetCode';
    $scope.saveUrlEntity = $scope.path + 'createEntity';
    $scope.saveUrlFGArticle = $scope.path + 'createFGArticle';
    $scope.saveUrlProcess = $scope.path + 'createProcess';
    $scope.saveUrlRMArticle = $scope.path + 'createRMArticle';
    //$scope.saveUrlTeamDefinitionCategory = $scope.path + 'createTeamDefinitionCategory';

    $scope.ModelTemp = {
        Id: null,
        StandardName: null,
        FGProductGroup: null,
        RMGroup: null,
        UserName: null,
        FGProductSubGroup: null,
        RMSubGroup: null,
        Code: null,
        ResponsiblePersonBgtCodeId: null,
        ResponsiblePersonBgtCode: null,
        OrderRecovery: null,
        Remarks: null
    };
    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);

    $scope.selectBudgetCode = function () {
        $scope.getBudgetCode();
        angular.element(document.querySelector('#BudgetCodePopUp')).modal('show');
    }

    $scope.BudgetCodeList = [];
    $scope.getBudgetCode = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'GetBudgetCode',
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.BudgetCodeList = resp.data;
        });
    }

    $scope.doubleBudgetCode = function (e) {
        $scope.ModelNew.ResponsiblePersonBgtCodeId = e.data.ManPowerBudgetId;
        $scope.ModelNew.ResponsiblePersonBgtCode = e.data.Code;
        angular.element(document.querySelector('#BudgetCodePopUp')).modal('hide');
    }

    $scope.closeBudgetCodePopUp = function () {
        angular.element(document.querySelector('#BudgetCodePopUp')).modal('hide');
    }

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.ProductivityRecoveryMasterForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: { 'PRMData': $scope.ModelNew },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.LoadPRMList();
                    PRMClearFields();

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        }
    };

    $scope.PRMList = [];
    $scope.LoadPRMList = function () {
        $http({

            method: 'Get',
            url: 'OrderManagements/ProductivityRecoveryMaster/LoadPRMList'
        }).then(function successCallback(response) {
            $scope.PRMList = response.data;
            var gridObj = $("#GridPRM").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
        }
        )
    }
    $scope.LoadPRMList();

    $scope.GetDetails = function (args) {
        $scope.PRMasterId = args.data.Id;
        $http({
            method: 'Get',
            url: 'OrderManagements/ProductivityRecoveryMaster/LoadPRMEditData?PRMID=' + args.data.Id
        }).then(function successCallback(response) {
            $scope.ModelNew = response.data.prm[0];
            $scope.LoadEntityDetails($scope.PRMasterId);
            //$scope.LoadFGArticleDetails($scope.PRMasterId);
            $scope.LoadProcessDetails($scope.PRMasterId);
            //$scope.LoadRMArticleDetails($scope.PRMasterId);
            $scope.LoadArticleMasterDetails($scope.PRMasterId);
            if (!$rootScope.isCollapsed) {
                $rootScope.toggle();
            }
        }
        )
    }

    $scope.Delete = function () {
        $http({
            method: 'POST',
            url: 'OrderManagements/ProductivityRecoveryMaster/PRMDelete?id=' + $scope.ModelNew.Id,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.LoadPRMList();
                PRMClearFields();
            }
            function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        });
    };

    $scope.Clear = function () {
        PRMClearFields();
    };

    function PRMClearFields() {
        $scope.Action = "Save";
        $scope.ModelNew = Object.assign({}, $scope.ModelTemp);
    }

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;


    };

    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.refreshTemplateEntity = function (args) {
        $("#Eheadchk").ejCheckBox({ "change": CheckBoxSelectAllEntity });
    };
    function CheckBoxSelectAllEntity(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        var filtered = $("#GridEntity").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.PRMEntityList.length; i++) {
                $scope.PRMEntityList[i].Flag = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].Flag = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridEntity").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
    };

    $scope.PRMEntityList = [];
    $scope.LoadEntityDetails = function (pid) {
        $http({

            method: 'Get',
            url: 'OrderManagements/ProductivityRecoveryMaster/LoadEntityDetails?PRMId=' + pid
        }).then(function successCallback(response) {
            $scope.PRMEntityList = response.data;
        }
        )
    }

    $scope.selectResponsiblePerson = function (data) {
        $scope.Newobject = data.data;
        $scope.getEmployee();
        angular.element(document.querySelector('#ResponsiblePersonPopup')).modal('show');
    }

    $scope.EmployeeList = [];
    $scope.getEmployee = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'GetEmployee',
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.EmployeeList = resp.data;
        });
    }

    $scope.doubleEmployee = function (e) {
        $scope.Newobject.ResponsiblePersonId = e.data.SystemId;
        $scope.Newobject.ResponsiblePerson = e.data.EmployeeName;
        angular.element(document.querySelector('#ResponsiblePersonPopup')).modal('hide');
    }

    $scope.closeResponsiblePersonPopUp = function () {
        angular.element(document.querySelector('#ResponsiblePersonPopup')).modal('hide');
    }

    $scope.EntitySave = function () {
        try {

            $scope.SaveList = [];
            for (var i = 0; i < $scope.PRMEntityList.length; i++) {
                if ($scope.PRMEntityList[i].Flag == true) {
                    $scope.PRMEntityList[i].PRMId = $scope.ModelNew.Id;
                    $scope.SaveList.push($scope.PRMEntityList[i]);
                }
            }
            $http({
                method: 'POST',
                url: $scope.saveUrlEntity,
                data: {
                    "DataList": $scope.SaveList,
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {

                    ShowResult(response.data.Message, 'success');
                    $scope.LoadEntityDetails($scope.ModelNew.Id);
                    $scope.LoadProcessDetails($scope.ModelNew.Id);
                    $scope.Action = 'Save';
                }

            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };
        } catch (ex) {
            ShowResult(ex, 'Info');
        }
    };

    $scope.filters = [];
    $scope.getFiltersData = function () {
        try {
            $http({
                method: 'GET',
                url: 'OrderManagements/ProductivityRecoveryMaster/LoadFGArticleFilter',
                dataType: 'JSON'
            }).then(function successCallback(response) {
                $scope.filters = response.data;
                var columnList = [
                    { field: 'MaterialType', width: 20, headerText: "Material Type", type: "string" },
                    { field: 'Material', width: 20, headerText: "Material", type: "string" },
                    { field: 'Product', width: 20, headerText: "Product", type: "string" },
                    { field: 'MaterialCategory', width: 20, headerText: "Material Category", type: "string" },
                    { field: 'MaterialSubCategory', width: 20, headerText: "Material Sub Category", type: "string" }
                ];
                $("#filters").ejGrid({
                    dataSource: $scope.filters,
                    minWidth: 450, minHeight: 400,
                    allowFiltering: true, allowPaging: true, enableTouch: true, responsive: true, allowTextWrap: true, allowScrolling: true,
                    filterSettings: { filterType: "excel" },
                    columns: columnList
                });

                var gridObj = $("#filters").data("ejGrid");
                gridObj.refreshContent(true);
                gridObj.refreshTemplate();
                $("#filters").children('.e-pager.e-js.e-pager').hide();
                $("#filters").children('.e-gridcontent.e-droppable.e-js').hide();
                $("#filters").children('.e-gridcontent').hide();
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.getFiltersData();

    $scope.parameters = [];
    $scope.filterComplete = function () {

        var g = $("#filters").data("ejGrid");
        var fl = g.getFilteredRecords();
        if (fl.length == 0) {
            fl = $scope.filters;
        }


        var parameters = [];
        parameters.push({ "Key": "MaterialType", "Value": getString(fl, "MaterialType") });
        parameters.push({ "Key": "Material", "Value": getString(fl, "Material") });
        parameters.push({ "Key": "Product", "Value": getString(fl, "Product") });
        parameters.push({ "Key": "MaterialCategory", "Value": getString(fl, "MaterialCategory") });
        parameters.push({ "Key": "MaterialSubCategory", "Value": getString(fl, "MaterialSubCategory") });


        $scope.parameters = parameters;
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

    $scope.filtersRM = [];
    $scope.getFiltersRMData = function () {
        try {
            $http({
                method: 'GET',
                url: 'OrderManagements/ProductivityRecoveryMaster/LoadFGArticleFilterRM',
                dataType: 'JSON'
            }).then(function successCallback(response) {
                $scope.filtersRM = response.data;
                var columnList = [
                    { field: 'MaterialType', width: 20, headerText: "Material Type", type: "string" },
                    { field: 'Material', width: 20, headerText: "Material", type: "string" },
                    { field: 'Product', width: 20, headerText: "Product", type: "string" },
                    { field: 'MaterialCategory', width: 20, headerText: "Material Category", type: "string" },
                    { field: 'MaterialSubCategory', width: 20, headerText: "Material Sub Category", type: "string" }
                ];
                $("#filtersRM").ejGrid({
                    dataSource: $scope.filtersRM,
                    minWidth: 450, minHeight: 400,
                    allowFiltering: true, allowPaging: true, enableTouch: true, responsive: true, allowTextWrap: true, allowScrolling: true,
                    filterSettings: { filterType: "excel" },
                    columns: columnList
                });

                var gridObj = $("#filtersRM").data("ejGrid");
                gridObj.refreshContent(true);
                gridObj.refreshTemplate();
                $("#filtersRM").children('.e-pager.e-js.e-pager').hide();
                $("#filtersRM").children('.e-gridcontent.e-droppable.e-js').hide();
                $("#filtersRM").children('.e-gridcontent').hide();
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.getFiltersRMData();

    $scope.parametersRM = [];
    $scope.filterCompleteRM = function () {

        var g = $("#filtersRM").data("ejGrid");
        var fl = g.getFilteredRecords();
        if (fl.length == 0) {
            fl = $scope.filtersRM;
        }


        var parametersRM = [];
        parametersRM.push({ "Key": "MaterialType", "Value": getString(fl, "MaterialType") });
        parametersRM.push({ "Key": "Material", "Value": getString(fl, "Material") });
        parametersRM.push({ "Key": "Product", "Value": getString(fl, "Product") });
        parametersRM.push({ "Key": "MaterialCategory", "Value": getString(fl, "MaterialCategory") });
        parametersRM.push({ "Key": "MaterialSubCategory", "Value": getString(fl, "MaterialSubCategory") });


        $scope.parametersRM = parametersRM;
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

    $scope.refreshTemplateFGArticle = function (args) {
        $("#FGAheadchk").ejCheckBox({ "change": CheckBoxSelectAllFGArticle });
    };
    function CheckBoxSelectAllFGArticle(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        var filtered = $("#GridFGArticle").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.PRMFGArticleList.length; i++) {
                $scope.PRMFGArticleList[i].Flag = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].Flag = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridFGArticle").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
    };

    $scope.PRMFGArticleList = [];
    $scope.LoadFGArticleDetails = function (pid) {
        $scope.PRMFGArticleList = [];
        $scope.filterComplete();
        $http({
            method: 'POST',
            data: {
                'parameters': $scope.parameters, 'PRMId': $scope.PRMasterId
            },
            url: 'OrderManagements/ProductivityRecoveryMaster/LoadFGArticleDetails'
        }).then(function successCallback(response) {
            $scope.PRMFGArticleList = response.data;
        });
    }

    $scope.FGArticleSave = function () {
        try {

            $scope.SaveList = [];
            for (var i = 0; i < $scope.PRMFGArticleList.length; i++) {
                if ($scope.PRMFGArticleList[i].Flag == true) {
                    $scope.PRMFGArticleList[i].PRMId = $scope.ModelNew.Id;
                    $scope.SaveList.push($scope.PRMFGArticleList[i]);
                }
            }
            $http({
                method: 'POST',
                url: $scope.saveUrlFGArticle,
                data: {
                    "DataList": $scope.SaveList,
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {

                    ShowResult(response.data.Message, 'success');
                    $scope.LoadFGArticleDetails($scope.ModelNew.Id);
                    $scope.LoadArticleMasterDetails($scope.ModelNew.Id);
                    $scope.Action = 'Save';
                }

            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };
        } catch (ex) {
            ShowResult(ex, 'Info');
        }
    };

    $scope.refreshTemplateProcess = function (args) {
        $("#Pheadchk").ejCheckBox({ "change": CheckBoxSelectAllProcess });
    };
    function CheckBoxSelectAllProcess(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        var filtered = $("#GridProcess").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.PRMProcessList.length; i++) {
                $scope.PRMProcessList[i].Flag = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].Flag = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridProcess").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
    };

    $scope.PRMProcessList = [];
    $scope.LoadProcessDetails = function (pid) {
        $http({

            method: 'Get',
            url: 'OrderManagements/ProductivityRecoveryMaster/LoadProcessDetails?PRMId=' + pid
        }).then(function successCallback(response) {
            $scope.PRMProcessList = response.data;
        }
        )
    }

    $scope.ProcessSave = function () {
        try {

            $scope.SaveList = [];
            for (var i = 0; i < $scope.PRMProcessList.length; i++) {
                if ($scope.PRMProcessList[i].Flag == true) {
                    $scope.PRMProcessList[i].PRMId = $scope.ModelNew.Id;
                    $scope.SaveList.push($scope.PRMProcessList[i]);
                }
            }
            $http({
                method: 'POST',
                url: $scope.saveUrlProcess,
                data: {
                    "DataList": $scope.SaveList,
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {

                    ShowResult(response.data.Message, 'success');
                    $scope.LoadProcessDetails($scope.ModelNew.Id);
                    $scope.Action = 'Save';
                }

            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };
        } catch (ex) {
            ShowResult(ex, 'Info');
        }
    };

    $scope.refreshTemplateRMArticle = function (args) {
        $("#RMAheadchk").ejCheckBox({ "change": CheckBoxSelectAllRMArticle });
    };
    function CheckBoxSelectAllRMArticle(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        var filtered = $("#GridRMArticle").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.PRMRMArticleList.length; i++) {
                $scope.PRMRMArticleList[i].Flag = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].Flag = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridRMArticle").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
    };

    $scope.PRMRMArticleList = [];
    $scope.LoadRMArticleDetails = function (pid) {
        //$http({

        //    method: 'Get',
        //    url: 'OrderManagements/ProductivityRecoveryMaster/LoadRMArticleDetails?PRMId=' + pid
        $scope.filterCompleteRM();
        $http({
            method: 'POST',
            data: {
                'parametersRM': $scope.parametersRM, 'PRMId': $scope.PRMasterId
            },
            url: 'OrderManagements/ProductivityRecoveryMaster/LoadRMArticleDetails'
        }).then(function successCallback(response) {
            $scope.PRMRMArticleList = response.data;
        }
        )
    }

    $scope.RMArticleSave = function () {
        try {

            $scope.SaveList = [];
            for (var i = 0; i < $scope.PRMRMArticleList.length; i++) {
                if ($scope.PRMRMArticleList[i].Flag == true) {
                    $scope.PRMRMArticleList[i].PRMId = $scope.ModelNew.Id;
                    $scope.SaveList.push($scope.PRMRMArticleList[i]);
                }
            }
            $http({
                method: 'POST',
                url: $scope.saveUrlRMArticle,
                data: {
                    "DataList": $scope.SaveList,
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {

                    ShowResult(response.data.Message, 'success');
                    $scope.LoadRMArticleDetails($scope.ModelNew.Id);
                    $scope.LoadArticleMasterDetails($scope.ModelNew.Id);
                    $scope.Action = 'Save';
                }

            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };
        } catch (ex) {
            ShowResult(ex, 'Info');
        }
    };
    $scope.PRMArticleMasterList = [];
    $scope.LoadArticleMasterDetails = function (pid) {
        $http({

            method: 'Get',
            url: 'OrderManagements/ProductivityRecoveryMaster/LoadArticleMasterDetails?PRMId=' + pid
        }).then(function successCallback(response) {
            $scope.PRMArticleMasterList = response.data;
        })
    }

    $scope.rowDataBound = function rowDataBound(e) {

        if (e.data.FGApplicableId != '' && e.data.RMApplicableId == '') {
            e.row.css("background-color", '#90EE90');
        }
        else if (e.data.FGApplicableId == '' && e.data.RMApplicableId != '') {

            e.row.css("background-color", '#FFFFE0');
        }
        else if (e.data.FGApplicableId != '' && e.data.RMApplicableId != '') {

            e.row.css("background-color", '#FFD580');
        }
        else {
            e.row.css("background-color", '#FFFFFF');

        }
    }

    $scope.rowDataBoundFG = function rowDataBoundFG(e) {

        if (e.data.PRMUserName != '') {
            e.row.css("background-color", '#90EE90');
        }
        else {
            e.row.css("background-color", '#FFFFFF');

        }
    }

    $scope.selectCostingItem = function (data) {
        $scope.Newobject = data.data;
        $scope.getCostingItem();
        angular.element(document.querySelector('#CostingItemPopup')).modal('show');
    }

    $scope.CostingItemList = [];
    $scope.getCostingItem = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'GetCostingItem',
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.CostingItemList = resp.data;
        });
    }

    $scope.doubleCostingItem = function (e) {
        $scope.Newobject.CostingItemId = e.data.Id;
        $scope.Newobject.CostingItem = e.data.UserName;
        angular.element(document.querySelector('#CostingItemPopup')).modal('hide');
    }

    $scope.closeCostingItemPopUp = function () {
        angular.element(document.querySelector('#CostingItemPopup')).modal('hide');
    }
}