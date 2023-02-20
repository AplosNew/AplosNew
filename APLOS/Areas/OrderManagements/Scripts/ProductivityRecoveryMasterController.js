'use strict';
ProductivityRecoveryMasterController.$inject = ["cboService","commonMessage", "$scope", "$rootScope", "baseService", "$routeParams", "$location", "$http", "$filter"];
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
                    { field: 'ProductCategory', width: 20, headerText: "Product Category", type: "string" },
                    { field: 'ProductSubCategory', width: 20, headerText: "Product Sub Category", type: "string" }
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
        parameters.push({ "Key": "ProductCategory", "Value": getString(fl, "ProductCategory") });
        parameters.push({ "Key": "ProductSubCategory", "Value": getString(fl, "ProductSubCategory") });


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
                    { field: 'ProductCategory', width: 20, headerText: "Product Category", type: "string" },
                    { field: 'ProductSubCategory', width: 20, headerText: "Product Sub Category", type: "string" }
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
        parametersRM.push({ "Key": "ProductCategory", "Value": getString(fl, "ProductCategory") });
        parametersRM.push({ "Key": "ProductSubCategory", "Value": getString(fl, "ProductSubCategory") });


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
        //$scope.filterComplete();
        //$http({

        //    method: 'Get',
        //    url: 'OrderManagements/ProductivityRecoveryMaster/LoadFGArticleDetails?PRMId=' + pid + '&parameters='+ $scope.parameters
        //}).then(function successCallback(response) {
        //    $scope.PRMFGArticleList = response.data;
        //}
        //)
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
                    $scope.Action = 'Save';
                }

            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };
        } catch (ex) {
            ShowResult(ex, 'Info');
        }
    };
    //$scope.EACategoryList = [];
    //$scope.LoadEACategoryDetails = function () {
    //    $http({

    //        method: 'Get',
    //        url: 'OrderManagements/ProductivityRecoveryMaster/LoadEACategoryDetails'
    //    }).then(function successCallback(response) {
    //        $scope.EACategoryList = response.data;
    //    }
    //    )
    //}
    //$scope.LoadEACategoryDetails();

    //$scope.TeamCategoryList = [];
    //$scope.LoadTeamCategoryDetails = function () {
    //    $http({

    //        method: 'Get',
    //        url: 'OrderManagements/ProductivityRecoveryMaster/LoadTeamCategoryDetails'
    //    }).then(function successCallback(response) {
    //        $scope.TeamCategoryList = response.data;
    //    }
    //    )
    //}
    //$scope.LoadTeamCategoryDetails();

    //$scope.GetEACategoryDetails = function (args) {
    //    $http({
    //        method: 'Get',
    //        url: 'OrderManagements/ProductivityRecoveryMaster/LoadEACategoryEditData?CategoryId=' + args.data.Id
    //    }).then(function successCallback(response) {
    //        $scope.EACategory = response.data.category[0];
    //        if (!$rootScope.isCollapsed) {
    //            $rootScope.toggle();
    //        }
    //    }
    //    )
    //}

    //$scope.GetTeamCategoryDetails = function (args) {
    //    $http({
    //        method: 'Get',
    //        url: 'OrderManagements/ProductivityRecoveryMaster/LoadTeamCategoryEditData?TeamCategoryId=' + args.data.Id
    //    }).then(function successCallback(response) {
    //        $scope.TeamCategoryNew = response.data.teamcategory[0];
    //        if (!$rootScope.isCollapsed) {
    //            $rootScope.toggle();
    //        }
    //    }
    //    )
    //}

    //$scope.EACategorySave = function () {
    //    $scope.$broadcast('show-errors-check-validity');
    //    if ($scope.EmployeeActivityCategoryForm.$valid) {
    //        $http({
    //            method: 'POST',
    //            url: $scope.saveUrlEACategory,
    //            data: {
    //                'EACategoryData': $scope.EACategory,
    //            },
    //            dataType: 'JSON'
    //        }).then(function successCallback(response) {
    //            if (response.data.Error === true) {
    //                ShowResult(response.data.Message, 'failure');
    //            }
    //            else {
    //                ShowResult(response.data.Message, 'success');
    //                $scope.LoadEACategoryDetails();
    //                EACategoryFields($scope.GenerateCategroySequenceNo());

    //            }
    //        }), function errorCallBack(response) {
    //            ShowResult(response.data.Message, 'failure');
    //        }
    //    }
    //};

    //$scope.TeamCategorySave = function () {
    //    $scope.$broadcast('show-errors-check-validity');
    //    if ($scope.TeamCategoryForm.$valid) {
    //        $http({
    //            method: 'POST',
    //            url: $scope.saveUrlTeamCategory,
    //            data: {
    //                'TeamCategoryData': $scope.TeamCategoryNew,
    //            },
    //            dataType: 'JSON'
    //        }).then(function successCallback(response) {
    //            if (response.data.Error === true) {
    //                ShowResult(response.data.Message, 'failure');
    //            }
    //            else {
    //                ShowResult(response.data.Message, 'success');
    //                $scope.LoadTeamCategoryDetails();
    //                TeamCategoryFields($scope.GenerateTeamCategroySequenceNo());

    //            }
    //        }), function errorCallBack(response) {
    //            ShowResult(response.data.Message, 'failure');
    //        }
    //    }
    //};

    

    //$scope.EACategoryClear = function () {
    //    EACategoryFields($scope.GenerateCategroySequenceNo());
    //};

    //$scope.TeamCategoryClear = function () {
    //    TeamCategoryFields($scope.GenerateTeamCategroySequenceNo());
    //};

    

    //function EACategoryFields(seq) {
    //    $scope.Action = "Save";
    //    $scope.EACategory = Object.assign({}, $scope.Category);
    //    $scope.EACategory.Sequence = seq;
    //}

    //function TeamCategoryFields(seq) {
    //    $scope.Action = "Save";
    //    $scope.TeamCategoryNew = Object.assign({}, $scope.TeamCategory);
    //    $scope.TeamCategoryNew.Sequence = seq;
    //}

    
    //$scope.EACategoryDelete = function () {
    //    $http({
    //        method: 'POST',
    //        url: 'OrderManagements/ProductivityRecoveryMaster/EACategoryDelete?id=' + $scope.EACategory.Id,
    //        dataType: 'JSON'
    //    }).then(function successCallback(response) {
    //        if (response.data.Error === true) {
    //            ShowResult(response.data.Message, 'failure');
    //        }
    //        else {
    //            ShowResult(response.data.Message, 'success');
    //            $scope.LoadEACategoryDetails();
    //            EACategoryFields($scope.GenerateCategroySequenceNo());
    //        }
    //        function errorCallBack(response) {
    //            ShowResult(response.data.Message, 'failure');
    //        }
    //    });
    //};

    //$scope.TeamCategoryDelete = function () {
    //    $http({
    //        method: 'POST',
    //        url: 'OrderManagements/ProductivityRecoveryMaster/TeamCategoryDelete?id=' + $scope.TeamCategoryNew.Id,
    //        dataType: 'JSON'
    //    }).then(function successCallback(response) {
    //        if (response.data.Error === true) {
    //            ShowResult(response.data.Message, 'failure');
    //        }
    //        else {
    //            ShowResult(response.data.Message, 'success');
    //            $scope.LoadTeamCategoryDetails();
    //            TeamCategoryFields($scope.GenerateTeamCategroySequenceNo());
    //        }
    //        function errorCallBack(response) {
    //            ShowResult(response.data.Message, 'failure');
    //        }
    //    });
    //};

    //$scope.TeamBudgetCodeList = [];
    //$scope.LoadBudgetCodeDetails = function (pid) {
    //    $http({

    //        method: 'Get',
    //        url: 'OrderManagements/ProductivityRecoveryMaster/LoadBudgetCodeDetails?TeamId=' + pid
    //    }).then(function successCallback(response) {
    //        $scope.TeamBudgetCodeList = response.data;
    //    }
    //    )
    //}

    //$scope.TeamEmployeeList = [];
    //$scope.LoadEmployeeDetails = function (pid) {
    //    $http({

    //        method: 'Get',
    //        url: 'OrderManagements/ProductivityRecoveryMaster/LoadEmployeeDetails?TeamId=' + pid
    //    }).then(function successCallback(response) {
    //        $scope.TeamEmployeeList = response.data;
    //    }
    //    )
    //}

   

    //$scope.TeamDefinitionCategoryList = [];
    //$scope.LoadTeamDefinitionCategoryDetails = function (pid) {
    //    $http({

    //        method: 'Get',
    //        url: 'OrderManagements/ProductivityRecoveryMaster/LoadTeamDefinitionCategoryDetails?TeamId=' + pid
    //    }).then(function successCallback(response) {
    //        $scope.TeamDefinitionCategoryList = response.data;
    //    }
    //    )
    //}

    //$scope.refreshTemplateBudgetCode = function (args) {
    //    $("#headchk").ejCheckBox({ "change": CheckBoxSelectAllBudgetCode });
    //};
    //function CheckBoxSelectAllBudgetCode(e) {
    //    var ChkOrUnchk = false;
    //    if (e.model.checkState === "check") {
    //        ChkOrUnchk = true;
    //    }

    //    var filtered = $("#GridBudgetCode").data("ejGrid").getFilteredRecords();
    //    if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
    //        for (var i = 0; i < $scope.TeamBudgetCodeList.length; i++) {
    //            $scope.TeamBudgetCodeList[i].Flag = ChkOrUnchk;
    //        }
    //    }
    //    else {
    //        for (var j = 0; j < filtered.length; j++) {
    //            filtered[j].Flag = ChkOrUnchk;
    //        }
    //    }
    //    var gridObj = $("#GridBudgetCode").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
    //};

    //$scope.refreshTemplateEmployee = function (args) {
    //    $("#Empheadchk").ejCheckBox({ "change": CheckBoxSelectAllEmployee });
    //};
    //function CheckBoxSelectAllEmployee(e) {
    //    var ChkOrUnchk = false;
    //    if (e.model.checkState === "check") {
    //        ChkOrUnchk = true;
    //    }

    //    var filtered = $("#GridEmployee").data("ejGrid").getFilteredRecords();
    //    if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
    //        for (var i = 0; i < $scope.TeamEmployeeList.length; i++) {
    //            $scope.TeamEmployeeList[i].Flag = ChkOrUnchk;
    //        }
    //    }
    //    else {
    //        for (var j = 0; j < filtered.length; j++) {
    //            filtered[j].Flag = ChkOrUnchk;
    //        }
    //    }
    //    var gridObj = $("#GridEmployee").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
    //};


    

    //$scope.refreshTemplateTeamDefinitionCategory = function (args) {
    //    $("#TDCheadchk").ejCheckBox({ "change": CheckBoxSelectAllTeamCategory });
    //};
    //function CheckBoxSelectAllTeamCategory(e) {
    //    var ChkOrUnchk = false;
    //    if (e.model.checkState === "check") {
    //        ChkOrUnchk = true;
    //    }

    //    var filtered = $("#GridTeamDefinitionCategory").data("ejGrid").getFilteredRecords();
    //    if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
    //        for (var i = 0; i < $scope.TeamDefinitionCategoryList.length; i++) {
    //            $scope.TeamDefinitionCategoryList[i].Flag = ChkOrUnchk;
    //        }
    //    }
    //    else {
    //        for (var j = 0; j < filtered.length; j++) {
    //            filtered[j].Flag = ChkOrUnchk;
    //        }
    //    }
    //    var gridObj = $("#GridTeamDefinitionCategory").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
    //};

    //$scope.BudgetCodeSave = function () {
    //    try {

    //        $scope.SaveList = [];
    //        for (var i = 0; i < $scope.TeamBudgetCodeList.length; i++) {
    //            if ($scope.TeamBudgetCodeList[i].Flag == true) {
    //                $scope.TeamBudgetCodeList[i].TeamDefinitionId = $scope.ModelNew.Id;
    //                $scope.SaveList.push($scope.TeamBudgetCodeList[i]);
    //            }
    //        }
    //        $http({
    //            method: 'POST',
    //            url: $scope.saveUrlBudgetCode,
    //            data: {
    //                "DataList": $scope.SaveList,
    //            },
    //            dataType: 'JSON'
    //        }).then(function successCallback(response) {
    //            if (response.data.Error === true) {
    //                ShowResult(response.data.Message, 'failure');
    //            }
    //            else {

    //                ShowResult(response.data.Message, 'success');
    //                $scope.LoadBudgetCodeDetails($scope.ModelNew.Id);
    //                $scope.LoadEmployeeDetails($scope.ModelNew.Id);
    //                $scope.Action = 'Save';
    //            }

    //        }), function errorCallBack(response) {
    //            ShowResult(response.data.Message, 'failure');
    //        };
    //    } catch (ex) {
    //        ShowResult(ex, 'Info');
    //    }
    //};

    //$scope.EmployeeSave = function () {
    //    try {

    //        $scope.SaveList = [];
    //        for (var i = 0; i < $scope.TeamEmployeeList.length; i++) {
    //            if ($scope.TeamEmployeeList[i].Flag == true) {
    //                $scope.TeamEmployeeList[i].TeamDefinitionId = $scope.ModelNew.Id;
    //                $scope.SaveList.push($scope.TeamEmployeeList[i]);
    //            }
    //        }
    //        $http({
    //            method: 'POST',
    //            url: $scope.saveUrlEmployee,
    //            data: {
    //                "DataList": $scope.SaveList,
    //            },
    //            dataType: 'JSON'
    //        }).then(function successCallback(response) {
    //            if (response.data.Error === true) {
    //                ShowResult(response.data.Message, 'failure');
    //            }
    //            else {

    //                ShowResult(response.data.Message, 'success');
    //                $scope.LoadEmployeeDetails($scope.ModelNew.Id);
    //                $scope.Action = 'Save';
    //            }

    //        }), function errorCallBack(response) {
    //            ShowResult(response.data.Message, 'failure');
    //        };
    //    } catch (ex) {
    //        ShowResult(ex, 'Info');
    //    }
    //};

   
    //$scope.TeamDefinitionCategorySave = function () {
    //    try {

    //        $scope.SaveList = [];
    //        for (var i = 0; i < $scope.TeamDefinitionCategoryList.length; i++) {
    //            if ($scope.TeamDefinitionCategoryList[i].Flag == true) {
    //                $scope.TeamDefinitionCategoryList[i].TeamDefinitionId = $scope.ModelNew.Id;
    //                $scope.SaveList.push($scope.TeamDefinitionCategoryList[i]);
    //            }
    //        }
    //        $http({
    //            method: 'POST',
    //            url: $scope.saveUrlTeamDefinitionCategory,
    //            data: {
    //                "DataList": $scope.SaveList,
    //            },
    //            dataType: 'JSON'
    //        }).then(function successCallback(response) {
    //            if (response.data.Error === true) {
    //                ShowResult(response.data.Message, 'failure');
    //            }
    //            else {

    //                ShowResult(response.data.Message, 'success');
    //                $scope.LoadTeamDefinitionCategoryDetails($scope.ModelNew.Id);
    //                $scope.Action = 'Save';
    //            }

    //        }), function errorCallBack(response) {
    //            ShowResult(response.data.Message, 'failure');
    //        };
    //    } catch (ex) {
    //        ShowResult(ex, 'Info');
    //    }
    //};
}