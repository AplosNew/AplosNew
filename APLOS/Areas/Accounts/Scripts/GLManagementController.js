'use strict';
GLManagementController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$controller', 'accountService'];
function GLManagementController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $controller, accountService) {
    $rootScope.title = 'GL Management';
    $scope.Action = 'Save';
    $scope.ModelList = [];
    $scope.path = 'Accounts/GLManagement/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'CreateGlManagementHeader';
    $scope.saveEmpCatUrl = $scope.path + 'CreateGlManagementEmployeeCategory';
    $scope.deleteUrl = $scope.path + 'DeleteGlControl/';
    baseService.init($scope.getListUrl);
    $scope.searchBy = "UserName"; $scope.search = "";
    $scope.searchByList = [{ value: 'Id', name: "Id" }, { value: 'Code', name: "Code" }, { value: 'ShortName', name: "Short Name" }, { value: 'StandardName', name: "Standard Name" }, { value: 'UserName', name: "User Name" }];
    $controller('baseMaterialAndArticleController', { $scope: $scope, $http: $http });
    $scope.downloadgriddataUrlPath = 'GridReports/DownloadUsingFullPath';

    $scope.Type = [];
    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.getData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetGlManagementList",
            data: { column: $scope.searchBy, value: $scope.search },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ModelList = response.data;
            ClearFields(response.data.Sequence);
            $scope.GetSequence();
        });
    }
    $scope.getData();

    $scope.ModelTemp = {
        Id: null,
        Sequence: 0,
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        Description: null,
        Remarks: null,
        Active: true
    };
    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);

    $scope.employeeTypeList = [];
    cboService.getCboEmployeeCategoryGroupByCompanyGroup(null, function (result) {
        $scope.employeeTypeList = result;
    });

    $scope.ModelTempMat = {
        Id: null,
        UserName: null,
        StorageLocationId: null,
        StorageSubLocation: null,
        MaterialTypeId: null,
        MaterialGroupMasterId: null,
        MaterialMasterId: null,
        MaterialMasterArticleId: null,
        AccessType: null,
        NoOfBin: null,
        Remarks: null,
        StorageLevel: null,
    };
    $scope.ModelNewMat = Object.assign({}, $scope.ModelTempMat);


    $scope.GetSequence = function () {
        cboService.getSequence($scope.getSeqUrl, function (data) {
            $scope.ModelTemp.Sequence = data;
            $scope.ModelNew.Sequence = data;
        });
    };
    $scope.GetSequence();

    $scope.Get = function (args) {
        $scope.ModelNew = Object.assign({}, args.data);
        $scope.GetEmployeeCategory(args.data.Id);
        //$scope.selectExpenseGL(args.data.Id);
        //$scope.GetInventoryGL(args.data.Id);
        //$scope.GetInventoryCapitalGL(args.data.Id);
        //$scope.GetCapitalGL(args.data.Id);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.Save = function () { 
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.ModelNewForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: {'data': $scope.ModelNew},
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    //ClearFields(response.data.Sequence);
                    //$scope.getData();
                    //$scope.selectIDs();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            } 
        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.ModelNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.ModelNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    ClearFields(response.data.Sequence);
                    $scope.getData();
                    $scope.selectExpenseGL();
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
        $scope.Action = 'Save';
        $scope.ModelNew = Object.assign({}, $scope.ModelTemp);
        $scope.ModelNew.Sequence = seq;
        $scope.MaterialDataList = [];
        $scope.ExpenseGLList = [];
        $scope.InventoryGLList = [];
        $scope.InventoryCapitalGLList = [];
        $scope.CapitalGLList = [];
    }

    $scope.EmployeeCategory = {
        Id: null,
        EmployeeCategoryId: null 
    };
    $scope.EmpCatNew = Object.assign({}, $scope.EmployeeCategory);

    $scope.SaveEmployeeCategory = function () {
        if (baseService.isUndefinedOrNull($scope.ModelNew.Id)) {
            return ShowResult('Please select GL Management!', 'failure');
        }
            $http({
                method: 'POST',
                url: $scope.saveEmpCatUrl,
                data: { 'data': $scope.EmpCatNew, 'GlManagementId': $scope.ModelNew.Id},
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success'); 
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
    };
    $scope.EmployeeCategoryList = [];
    $scope.GetEmployeeCategory = function (data) {
        $http({
            method: 'POST',
            url: $scope.path + "GetMaterialData",
            data: { 'glManagementId': data },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.EmployeeCategoryList = response.data;
            for (var i = 0; i < $scope.EmployeeCategoryList.length; i++) {
                $scope.EmpCatNew.EmployeeCategoryId = $scope.EmployeeCategoryList[i].EmployeeCategoryId;
            }
        })
    }

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
            'value': 'Asset'
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

    $scope.columnExcluedList = ['WithSKU', 'Description', 'Active', 'IsInventory', 'IsExpenseOut', 'IsAsset	', 'AssetMasterName', 'AssetType', 'IsRevenue'];
    $scope.popUpList = [];
    $scope.valueData = '';
    $scope.popUpParameters = {
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
    $scope.popUp = function () {
        $scope.popUpUrl = $scope.path + 'GetMaterialList';
        $scope.getPopUpData = function (pageno) {
            baseService.paginationBase($scope.popUpUrl, pageno, $scope.popUpParameters)
                .then(function (result) {
                    $scope.popUpDataList = result.Rows;
                    $scope.popUpParameters.total_count = result.Total;
                    for (var i = 0; i < $scope.popUpDataList.length; i++) {
                        $scope.ModelNewMat.MaterialTypeId = $scope.popUpDataList[i].MaterialTypeId;
                        $scope.ModelNewMat.MaterialGroupMasterId = $scope.popUpDataList[i].MaterialGroupMasterId;
                        $scope.ModelNewMat.MaterialMasterId = $scope.popUpDataList[i].Id;
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure', 'popUpId');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#popUpId')).modal('show');
        $scope.getPopUpData();
    };

    $scope.closePopUp = function () {
        $scope.valueData = '';
        angular.element(document.querySelector('#popUpId')).modal('hide');
    };


    function checkItemExist(list, Id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].MaterialMasterId === Id) {
                return true;
            }
        }
        return false;
    }

    $scope.selectDoubleClick = function (data) {
        $scope.Type = "Material";
        if (checkItemExist($scope.MaterialDataList, data.Id) === false) {
            $scope.MaterialDataList.push({
                GLControlMasterId: $scope.ModelNew.Id,
                MaterialMasterId: data.Id,
                MaterialMaster: data.UserName,
                Type: $scope.Type
            });
        }
        $scope.closePopUp();
    };

    $scope.selectByButton = function () {
        if (baseService.isUndefinedOrNull($scope.valueData))
            return ShowResult('Please at first select row', 'failure', 'popUpId');
        $scope.selectDoubleClick($scope.valueData);
        $scope.closePopUp();
    };

    // #region ---------------------------------      MATERIAL ALLOCACTION GRID      -----------------------------------//

    $scope.MaterialDataList = [];
    $scope.selectIDs = function (data) {
        $http({
            method: 'POST',
            url: $scope.path + "selectIDs",
            data: {
                'materialType': data.MaterialTypeId,
                'materialGroup': data.MaterialGroupMasterId,
                'materialMasterId': data.Id
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.MaterialDataList = response.data;
            //$scope.selectBinIDs();
        })
    }


    $scope.GetMaterialData = function (data) {
        $http({
            method: 'POST',
            url: $scope.path + "GetMaterialData",
            data: { 'glControlDetailId': data },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.MaterialDataList = response.data;
        })
    }

    $scope.tempIndex = [];
    $scope.RemoveMaterial = function (data, index) {
        $scope.tempIndex = index;
        $scope.materialId = data.MaterialMasterId;
        $scope.materialMasterDataList = data;
        if (baseService.isUndefinedOrNull(data.UserName))
            $scope.message_confirmation = 'Are you sure want to remove this data....';
        else
            $scope.message_confirmation = 'Are you sure want to remove ?';
        angular.element(document.querySelector('#confirmMaterialPopUp')).modal('show');
    };
    $scope.RemoveMaterialRow = function () {
        $scope.MaterialDataList.splice($scope.tempIndex, 1);
        $scope.DeleteMaterial($scope.materialId, $scope.materialMasterDataList);
    };

    $scope.DeleteMaterial = function () {

        $http({
            method: 'POST',
            url: 'Accounts/GeneralAccountDeterminate/UpdateMaterial',
            data: { 'materialId': $scope.materialId, 'materialList': $scope.materialMasterDataList },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.selectIDs();
            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        }

    }

    // #endregion ---------------------------------      MATERIAL ALLOCACTION GRID      -----------------------------------//


    // #region ---------------------------------      Expense     -----------------------------------//

    $scope.report = {
        GLName: null,
        GLGeneralInfoId: null,
        BudgetMasterId: null
    };


    $scope.glList = [];
    $scope.getCompanyGLCboList = function () {
        accountService.getCompanyGLCboList(function (result) {
            $scope.glList = result;
        });
    };
    $scope.getCompanyGLCboList();

    $scope.budgetList = [];
    $scope.getBudgetMasterCboList = function (glId) {
        accountService.getBudgetMasterCboList(glId, function (result) {
            $scope.budgetList = result;
        });
    };

    $scope.activityList = [];
    $scope.getBudgetMasterActivityCbo = function (budgetMasterId) {
        accountService.getBudgetMasterActivityCbo(budgetMasterId, function (result) {
            $scope.activityList = result;
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

    $scope.tabType = "";
    $scope.GetCOAICodeList = function (data) {
        $scope.tabType = data;
        $scope.GLUrl1 = "Accounts/glitem/GetAllGLBudgetActivityList";
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

    $scope.closeCOAICodeListPopUpSelected = function () {
        if ($scope.rowSelected !== null) {
            angular.element(document.querySelector("#GLPopUp")).modal("hide");
        } else {
            angular.element(document.querySelector("#cancelPopUp")).modal("show");
        }
    };

    function checkConsumableExist(list, data) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].GLGeneralInfoId === data.GLGeneralInfoId && list[i].BudgetMasterId === data.BudgetMasterId && list[i].ActivityId === data.ActivityId) {
                return true;
            }
        }
        return false;
    }

    $scope.setSelected = function (data) {

        if ($scope.tabType == 'consumableTab') {
            $scope.Type = "Consumable";

            if (checkConsumableExist($scope.ExpenseGLList, data) === false) {
                $scope.ExpenseGLList.push({
                    Id: null,
                    GLGeneralInfoId: data.GLGeneralInfoId,
                    GLGeneralInfoName: data.GLGeneralInfoName,
                    BudgetMasterId: data.BudgetMasterId,
                    BudgetName: data.BudgetName,
                    ActivityName: data.ActivityName,
                    ActivityId: data.ActivityId,
                    BudgetMasterActivityId: data.BudgetMasterActivityId,
                    Type: $scope.Type
                });
            }
            else {
                ShowResult(data.GLGeneralInfoName + " is already  Exist", "failure");
            }
        }

        else if ($scope.tabType == 'inventoryTab') {
            $scope.Type = "Inventory";
            if (checkConsumableExist($scope.InventoryGLList, data) === false) {
                $scope.InventoryGLList.push({
                    Id: null,
                    GLGeneralInfoId: data.GLGeneralInfoId,
                    GLGeneralInfoName: data.GLGeneralInfoName,
                    BudgetMasterId: data.BudgetMasterId,
                    BudgetName: data.BudgetName,
                    ActivityName: data.ActivityName,
                    ActivityId: data.ActivityId,
                    BudgetMasterActivityId: data.BudgetMasterActivityId,
                    Type: $scope.Type
                });
            }
            else {
                ShowResult(data.GLGeneralInfoName + " is already  Exist", "failure");
            }
        }
        else if ($scope.tabType == 'inventoryCapitalTab') {
            $scope.Type = "InventoryCapital";
            if (checkConsumableExist($scope.InventoryCapitalGLList, data) === false) {
                $scope.InventoryCapitalGLList.push({
                    Id: null,
                    GLGeneralInfoId: data.GLGeneralInfoId,
                    GLGeneralInfoName: data.GLGeneralInfoName,
                    BudgetMasterId: data.BudgetMasterId,
                    BudgetName: data.BudgetName,
                    ActivityName: data.ActivityName,
                    ActivityId: data.ActivityId,
                    BudgetMasterActivityId: data.BudgetMasterActivityId,
                    Type: $scope.Type
                });
            }
            else {
                ShowResult(data.GLGeneralInfoName + " is already  Exist", "failure");
            }
        }
        else {
            $scope.Type = "Capital";
            if (checkConsumableExist($scope.CapitalGLList, data) === false) {
                $scope.CapitalGLList.push({
                    Id: null,
                    GLGeneralInfoId: data.GLGeneralInfoId,
                    GLGeneralInfoName: data.GLGeneralInfoName,
                    BudgetMasterId: data.BudgetMasterId,
                    BudgetName: data.BudgetName,
                    ActivityName: data.ActivityName,
                    ActivityId: data.ActivityId,
                    BudgetMasterActivityId: data.BudgetMasterActivityId,
                    Type: $scope.Type
                });
            }
            else {
                ShowResult(data.GLGeneralInfoName + " is already  Exist", "failure");
            }
        }

        $scope.closeCOAICodeListPopUp();
    };

    $scope.ExpenseGLList = [];
    $scope.selectGLBudget = function (data) {
        $http({
            method: 'POST',
            url: $scope.path + "GetExpenseGLData",
            data: {
                'glId': data.GLGeneralInfoId,
                'budgetId': data.BudgetMasterId,
                'activityId': data.ActivityId
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ExpenseGLList = response.data;
        })
    }

    $scope.tempIndex = [];
    $scope.RemoveIndex = [];
    $scope.RemoveExpense = function (data, index, removeRow) {
        $scope.tempIndex = index;
        $scope.RemoveIndex = removeRow;
        if (data.Id != null) {
            $scope.consumableId = data.Id;
        }
        else {
            $scope.consumableId = "";
        }
        if (baseService.isUndefinedOrNull(data.UserName))
            $scope.message_confirmation = 'Are you sure want to remove this data....';
        else
            $scope.message_confirmation = 'Are you sure want to remove ?';
        angular.element(document.querySelector('#confirmgenericPopUp')).modal('show');
    };

    $scope.RemoveRow = function () {
        if ($scope.RemoveIndex == 'inventoryTabDel') {
            if (baseService.isUndefinedOrNull($scope.consumableId)) {
                $scope.InventoryGLList.splice($scope.tempIndex, 1);
            }
            else {
                $http({
                    method: 'POST',
                    url: 'Accounts/GeneralAccountDeterminate/DeleteConsumerable',
                    data: { 'Id': $scope.consumableId },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
                $scope.InventoryGLList.splice($scope.tempIndex, 1);
            }

        }

        else if ($scope.RemoveIndex == 'consumableTabDel') {
            if (baseService.isUndefinedOrNull($scope.consumableId)) {
                $scope.ExpenseGLList.splice($scope.tempIndex, 1);
            }
            else {
                $http({
                    method: 'POST',
                    url: 'Accounts/GeneralAccountDeterminate/DeleteConsumerable',
                    data: { 'Id': $scope.consumableId },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
                $scope.ExpenseGLList.splice($scope.tempIndex, 1);
            }

        }

        else if ($scope.RemoveIndex == 'inventoryCapitalTabDel') {
            if (baseService.isUndefinedOrNull($scope.consumableId)) {
                $scope.InventoryGLList.splice($scope.tempIndex, 1);
            }
            else {
                $http({
                    method: 'POST',
                    url: 'Accounts/GeneralAccountDeterminate/DeleteConsumerable',
                    data: { 'Id': $scope.consumableId },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
                $scope.InventoryCapitalGLList.splice($scope.tempIndex, 1);
            }

        }

        else {
            if (baseService.isUndefinedOrNull($scope.consumableId)) {
                $scope.CapitalGLList.splice($scope.tempIndex, 1);
            }
            else {
                $http({
                    method: 'POST',
                    url: 'Accounts/GeneralAccountDeterminate/DeleteConsumerable',
                    data: { 'Id': $scope.consumableId },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
                $scope.CapitalGLList.splice($scope.tempIndex, 1);
            }

        }

    }

    $scope.selectExpenseGL = function (data) {
        $scope.TabType = "Consumable";
        $http({
            method: 'POST',
            url: $scope.path + "GetConsumableData",
            data: { 'glControlDetailId': data, 'type': $scope.TabType },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ExpenseGLList = response.data;
        })
    }

    $scope.InventoryGLList = [];
    $scope.selectInventoryGLBudget = function (data) {
        $http({
            method: 'POST',
            url: $scope.path + "GetExpenseGLData",
            data: {
                'glId': data.GLGeneralInfoId,
                'budgetId': data.BudgetMasterId,
                'activityId': data.ActivityId
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.InventoryGLList = response.data;
        })
    }


    $scope.GetInventoryGL = function (data) {
        $scope.TabType = "Inventory";
        $http({
            method: 'POST',
            url: $scope.path + "GetConsumableData",
            data: { 'glControlDetailId': data, 'type': $scope.TabType },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.InventoryGLList = response.data;
        })
    }

    $scope.InventoryCapitalGLList = [];
    $scope.GetInventoryCapitalGL = function (data) {
        $scope.TabType = "InventoryCapital";
        $http({
            method: 'POST',
            url: $scope.path + "GetConsumableData",
            data: { 'glControlDetailId': data, 'type': $scope.TabType },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.InventoryCapitalGLList = response.data;
        })
    }

    $scope.CapitalGLList = [];
    $scope.GetCapitalGL = function (data) {
        $scope.TabType = "Capital";
        $http({
            method: 'POST',
            url: $scope.path + "GetConsumableData",
            data: { 'glControlDetailId': data, 'type': $scope.TabType },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.CapitalGLList = response.data;
        })
    }

    // #endregion --------------------------------- Inventory  -----------------------------------//

    $scope.GLControlReport = function (data, index) {
        $scope.fileName = "GLControlReport.xlsx";

        $http({
            method: 'POST',
            url: $scope.path + "GetGLControlReport",
            data: { 'glControlId': data.Id },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                $rootScope.report($scope.downloadgriddataUrlPath + "?FullPath=" + response.data.FileName + "&fileName=" + $scope.fileName);
            }
        }, function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
        });
    }



}