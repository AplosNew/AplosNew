'use strict';
costingItemController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function costingItemController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "Costing Item";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.costingItemList = [];
    $scope.path = 'Costings/costingItem/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl);
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.costingItemList = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.getData();

    $scope.ThisSegment = 'DirectMaterial';
    $scope.costingItem = {
        Id: null,
        Sequence: 0.00,
        Code: null,
        ShortName: null,

        StandardName: null,
        UserName: null,
        CostingCategoryId: null,
        MinimumOfQuantity: null,

        InternalRate: null,
        ExternalRate: null,
        SubProcessId: null,
        ValueLossPercentage: null,


        Description: null,
        Wastage: null,
        ProcessId: null,
        MaterialGroupMasterId: null,
        BudgetMasterId: null,
        ActivityId: null,
        CostingSubCategoryId: null,
        //CostingGroupId: null,
        CostingComponentId: null,
        UnitOfMeasurementId: null,
        Remarks: null,
        isSystemGenerated: false,
        Active: true,
        POIssueDeadLine: null,
        PurchaseGroupId: null,
        ActivityMetarialNo: null,
        BudgetName: null,
        //CostingItemType: null

    };
    
    $scope.searchByList = [
        {
            "name": "Sequence",
            "value": "Sequence"
        },
        {
            "name": "Code",
            "value": "Code"
        },
        {
            "name": "Short Name",
            "value": "ShortName"
        }
        ,
        {
            "name": "Standard Name",
            "value": "StandardName"
        },
        {
            "name": "User Name",
            "value": "UserName"
        },

        {
            "name": "Costing Category",
            "value": "CostingCategory"
        },
        {
            "name": "Costing Sub Category",
            "value": "CostingSubCategory"
        },
        {
            "name": "Component",
            "value": "CostingComponent"
        }
        
    ];

    $scope.costingItemNew = Object.assign({}, $scope.costingItem);

    $scope.BudgetList = [];
    $scope.CostingSubCategoryList = [];

    $scope.MaterialGroupList = [];
    $scope.GetMaterialGroupList = function () {
        $http({
            method: "GET",
            url: $scope.path + "GetMaterialGroupList",
            dataType: "JSON"
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
               
            }
            else {
                $scope.MaterialGroupList = response.data;
            }
        }, function errorCallback(response) {
            
        });
    }
    $scope.GetMaterialGroupList();
    //#region get all combo
    function getCostingGroup() {
        $http({
            method: "GET",
            url: "Costings/costingItem/GetCostingSubCategory",
            dataType: "JSON"
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                $scope.CostingSubCategoryList = response.data;
            }
        }, function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
        });
    };
    $scope.PurchaseGroupList = [];
    function getPurchaseGroup() {
        $http({
            method: "GET",
            url: "Costings/costingItem/GetPurchaseGroups",
            dataType: "JSON"
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                $scope.PurchaseGroupList = response.data;
            }
        }, function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
        });
    };
    getPurchaseGroup();
    getCostingGroup();
    //$scope.ProcessList = [];
    //cboService.getProcessCbo(function (result) {
    //    $scope.ProcessList = result;
    //})


    $scope.ProcessList = [];
    cboService.getProductionProcessCbo(function (response) {
        $scope.ProcessList = response;
    });
    $scope.SubProcessList = [];
    $scope.GetSubProcess = function () {
        $http({
            method: 'GET'
            , url: $scope.path + 'GetSubProcessCbo?processId=' + $scope.costingItemNew.ProcessId
            , dataType: 'JSON'
        }).then(function successCallback(response) {

            $scope.SubProcessList = response.data;
        }), function errorCallBack(response) {
            //ShowResult(response.data.Message, 'failure');
        };
    }
    //#endregion get all combo



    $scope.GetSequence = function () {
        $http.get($scope.getSeqUrl)
            .then(function (response) {
                $scope.costingItemNew.Sequence = response.data;
            });
    };
    $scope.GetSequence();

    $scope.Get = function (id, index) {
        $scope.index = index;

        $scope.costingItem = $scope.costingItemList[index];
        $scope.costingItemNew = Object.assign({}, $scope.costingItem);
        $scope.costingItemNew.BudgetName = $scope.costingItem.BudgetName;
        $scope.getActivity($scope.costingItemNew);
        $scope.ThisSegment = $scope.costingItem.CostingSegment;
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }

        $scope.GetSubProcess();
    };

    $scope.Save = function () {

        $scope.$broadcast('show-errors-check-validity');
        if ($scope.costingItemNewForm.$valid) {

            try {

                if ($scope.ThisSegment == 'DirectMaterial') {
                    if ($scope.costingItemNew.MinimumOfQuantity > 0) { } else { throw 'Please enter minimum order quantity'; }
                    if ($scope.costingItemNew.Wastage > 0) { } else { throw 'Please enter Wastage percentage'; }

                    if (angular.isUndefinedOrNull($scope.costingItemNew.UnitOfMeasurementId)) { throw 'Please enter unit of measurement'; }
                    if (angular.isUndefinedOrNull($scope.costingItemNew.StandardName)) { throw 'Please enter standard name'; }
                    if (angular.isUndefinedOrNull($scope.costingItemNew.ProcessId)) { throw 'Please enter process name'; }
                    if (angular.isUndefinedOrNull($scope.costingItemNew.MaterialGroupMasterId)) { throw 'Please enter material group'; }

                    if ($scope.costingItemNew.POIssueDeadLine > 0) { } else { throw 'Please enter PO Issue Dead Line(days)'; }
                    //if (angular.isUndefinedOrNull($scope.costingItemNew.PurchaseGroupId)) { throw 'Please enter purchase group'; }
                }
                if ($scope.ThisSegment == 'DirectProcess') {
                    if (angular.isUndefinedOrNull($scope.costingItemNew.ProcessId)) { throw 'Please enter process name'; }
                }


                if ($scope.Action === "Save") {
                    $http({
                        method: 'POST'
                        , url: $scope.saveUrl
                        , data: $scope.costingItemNew
                        , dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure');
                        }
                        else {
                            ShowResult(response.data.Message, 'success');
                            $scope.costingItemList.push(response.data.costingItem);
                            $scope.costingItemList = $filter('orderBy')($scope.costingItemList, 'Sequence');
                            $scope.getData();
                            ClearFields(response.data.Sequence);
                        }
                    }), function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    };
                }
                else if ($scope.Action === "Update") {
                    $http({
                        method: 'POST'
                        , url: $scope.updateUrl
                        , data: $scope.costingItemNew
                        , dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure');
                        }
                        else {
                            ShowResult(response.data.Message, 'success');
                            if ($scope.index > -1) {
                                $scope.costingItemList[$scope.index] = $scope.costingItem;
                                $scope.costingItemList = $filter('orderBy')($scope.costingItemList, 'Sequence');
                            }
                            $scope.getData();
                            ClearFields(response.data.Sequence);
                        }
                    }, function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    });
                }
            } catch (e) {
                ShowResult(e, 'failure');
            }
        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.costingItemNew.Id)) {
            $http({
                method: 'POST'
                , url: $scope.deleteUrl + $scope.costingItemNew.Id
                , dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    //$scope.costingItemList.splice($scope.index, 1);
                    $scope.getData();
                    ClearFields(response.data.Sequence);
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };

    $scope.Clear = function () {
        ClearFields($scope.GetSequence());
        return true;
    };

    function ClearFields(seq) {
        var SelectedComponetId = $scope.costingItemNew.CostingComponentId;
        $scope.Action = "Save";
        $scope.costingItem = {};
        $scope.Segment = '';
        $scope.costingItemNew = { CostingComponentId: SelectedComponetId, Sequence: seq, Active: true };
    }

    $scope.CostingCategoryList = [];
    cboService.getCostingCategoryCbo(function (response) {
        $scope.CostingCategoryList = response;
    });

    $scope.CostingComponentList = [];
    cboService.getCostingSubCategoryCbo(function (response) {
        $scope.CostingComponentList = response;
    });

    $scope.UnitOfMeasurementList = [];
    cboService.getUnitOfMeasurementCbo(function (response) {
        $scope.UnitOfMeasurementList = response;

    });

    //#region Budget Activity 

    $scope.addRow = function (data) {

        $scope.costingItemNew.BudgetMasterId = data.BudgetMasterId;
        $scope.costingItemNew.BudgetName = data.BudgetName;
        $scope.getActivity(data);
    };
    $scope.activityList = [];
    $scope.getActivity = function (data) {
        cboService.getBudgetMasterActivityCbo(data.BudgetMasterId, function (result) {
            $scope.costingItemNew.ActivityId = null;
            $scope.activityList = [];
            $scope.activityList = result;
            if ($scope.activityList.length == 1) {
                $scope.costingItemNew.ActivityId = $scope.activityList[0].ActivityId;
            }

        });
    };
    $scope.searchglByList = [
        {
            "name": "Account Group",
            "value": "AccountGroupName"
        },
        {
            "name": "GL Code",
            "value": "GLGeneralInfoCode"
        },
        {
            "name": "GL Name",
            "value": "GLGeneralInfoName"
        },
        {
            "name": "Budget",
            "value": "BudgetName"
        },
        {
            "name": "Activity",
            "value": "ActivityName"
        },
        {
            "name": "RefNo",
            "value": "RefNo"
        }
    ];

    $scope.glListParameters = {
        limit: 10,
        offset: 0,
        order: "asc",
        sort: "GLGeneralInfoCode",
        searchBy: "ActivityName",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.GetCOAICodeList = function () {
        $scope.GLUrl1 = "Accounts/glitem/GetExpenseTypeGLBudgetActivityList";
        $scope.GetCOAICodeListData = function (pageno) {

            baseService.paginationBase($scope.GLUrl1, pageno, $scope.glListParameters)
                .then(function (result) {
                    $scope.cOAICodeList = result.Rows;
                    $scope.glListParameters.total_count = result.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, "failure");
                }).finally(function () {
                });
        };
        angular.element(document.querySelector("#GLPopUp")).modal("show");
        $scope.modalShow = true;
        $scope.GetCOAICodeListData();
    };

    $scope.closeCOAICodeListPopUp = function () {
        angular.element(document.querySelector("#GLPopUp")).modal("hide");
    };

    $scope.setSelected = function (data, index) {
        $scope.addRow(data);
        $scope.closeCOAICodeListPopUp();
    };

    $scope.selectedSegment = function (args) {

        for (var i = 0; i < $scope.CostingComponentList.length; i++) {
            if ($scope.CostingComponentList[i].Value == args) {

                $scope.ThisSegment = $scope.CostingComponentList[i].CostingSegment;
                break;
            }
        }
    }
    //#endregion Budget Activity 

}