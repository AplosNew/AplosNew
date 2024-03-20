'use strict';
GLManagementController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$controller', 'accountService', '$window'];
function GLManagementController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $controller, accountService, $window) {
    $rootScope.title = 'GL Management';
    $scope.Action = 'Save';
    $scope.ModelList = [];
    $scope.path = 'Accounts/GLManagement/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'CreateGlManagementHeader';
    $scope.saveEmpCatUrl = $scope.path + 'CreateGlManagementEmployeeCategory';
    $scope.saveDesignationUrl = $scope.path + 'CreateGlManagementDesignation';
    $scope.savePositionCodeUrl = $scope.path + 'CreateGlManagementPositionCode';
    $scope.saveBudgetCodeUrl = $scope.path + 'CreateGlManagementBudgetCode';
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
        $scope.GetDesignationData(args.data.Id);
        $scope.GetPositionCodeData(args.data.Id);
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
                data: { 'data': $scope.ModelNew },
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
        EmployeeCategoryId: null,
        EmployeeCategory: null
    };
    $scope.EmpCatNew = Object.assign({}, $scope.EmployeeCategory);

    $scope.SaveEmployeeCategory = function () {
        if (baseService.isUndefinedOrNull($scope.ModelNew.Id)) {
            return ShowResult('Please select GL Management!', 'failure');
        }
        for (var i = 0; i < $scope.EmployeeCategoryList.length; i++) {
            if ($scope.EmployeeCategoryList[i].EmployeeCategoryId == $scope.EmpCatNew.EmployeeCategoryId) {
                return ShowResult('Same Employee Category already exists!!!', 'failure');
            }
        }
        $http({
            method: 'POST',
            url: $scope.saveEmpCatUrl,
            data: { 'data': $scope.EmpCatNew, 'GlManagementId': $scope.ModelNew.Id },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.GetEmployeeCategory($scope.ModelNew.Id);
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
    //#region LegalDesignation

    $scope.popUpDataList = [];
    $scope.GetDesignationInformation = function () {
        try { 
            $http({
                method: 'GET',
                url: 'Accounts/GLManagement/GetDesignationInformation'
            }).then(function successCallback(response) {
                $scope.popUpDataList = response.data;
            });
            angular.element(document.querySelector('#LDPopUp')).modal('show');
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.OK = function () {
        try {
            for (var i = 0; i < $scope.popUpDataList.length; i++) {
                if ($scope.popUpDataList[i].CheckBoxSelect == true) {
                    if (checkDoubleDesignationInformation($scope.DesignationList, $scope.popUpDataList[i].DesignationId) === false) {
                        $scope.DesignationList.push($scope.popUpDataList[i]);
                    }
                }
            } 
            angular.element(document.querySelector('#LDPopUp')).modal('hide');
            $scope.SaveDesignation();
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    function checkDoubleDesignationInformation(list, DesignationId) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].DesignationId === DesignationId) {
                return true;
            }
        }
        return false;
    }

    $scope.designation = {
        Id: null,
        DesignationId: null,
        Designation: null
    };
    $scope.designationNew = angular.copy($scope.designation);
     
 
    $scope.SaveDesignation = function () {
        if (baseService.isUndefinedOrNull($scope.ModelNew.Id)) {
            return ShowResult('Please select GL Management!', 'failure');
        }
        $http({
            method: 'POST',
            url: $scope.saveDesignationUrl,
            data: { 'data': $scope.DesignationList, 'GlManagementId': $scope.ModelNew.Id },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.GetDesignationData($scope.ModelNew.Id);
            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        }
    };

    $scope.DesignationList = [];
    $scope.GetDesignationData = function (data) {
        $http({
            method: 'POST',
            url: $scope.path + "GetDesignationData",
            data: { 'glManagementId': data },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.DesignationList = response.data; 
        })
    }
    //#endregion LegalDesignation

    //#region position
    $scope.position = {
        Id: null
        , PositionCodeId: null
        , PositionCode: null
    };
    $scope.positionNew = Object.assign({}, $scope.position);

    $scope.selectPositionCode = function () {
        $scope.getPositionCode();
        angular.element(document.querySelector('#PositionCodePopUp')).modal('show');
    }

    $scope.PositionCodeList = [];
    $scope.getPositionCode = function () {
        $http({
            method: 'Get',
            url: $scope.path + 'GetPositionCode',
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.PositionCodeList = resp.data;
        });
    }

    $scope.closePositionCodePopUp = function () {
        angular.element(document.querySelector('#PositionCodePopUp')).modal('hide');
    }


    $scope.OKPositionCode = function () {
        try {
            for (var i = 0; i < $scope.PositionCodeList.length; i++) {
                if ($scope.PositionCodeList[i].CheckBoxSelect == true) {
                    if (checkDoublePositionCodeInformation($scope.PositionCodeListData, $scope.PositionCodeList[i].PositionCodeId) === false) {
                        $scope.PositionCodeListData.push($scope.PositionCodeList[i]);
                    }
                }
            }
            angular.element(document.querySelector('#PositionCodePopUp')).modal('hide');
            $scope.SavePositionCode();
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    function checkDoublePositionCodeInformation(list, PositionCodeId) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].PositionCodeId === PositionCodeId) {
                return true;
            }
        }
        return false;
    }


    $scope.SavePositionCode = function () {
        if (baseService.isUndefinedOrNull($scope.ModelNew.Id)) {
            return ShowResult('Please select GL Management!', 'failure');
        }
        $http({
            method: 'POST',
            url: $scope.savePositionCodeUrl,
            data: { 'data': $scope.PositionCodeListData, 'GlManagementId': $scope.ModelNew.Id },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.GetPositionCodeData($scope.ModelNew.Id);
            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        }
    };

    $scope.PositionCodeListData = [];
    $scope.GetPositionCodeData = function (data) {
        $http({
            method: 'POST',
            url: $scope.path + "GetPositionCodeData",
            data: { 'glManagementId': data },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.PositionCodeListData = response.data; 
        })
    }
    //#endregion position

    //#region BudgetCode
    $scope.BudgetCode = {
        Id: null,
        BudgetCodeId: null,
        Code: null
    };
    $scope.BudgetCodeNew = angular.copy($scope.BudgetCode);

     
    $scope.BCpopUpTitle = "Manpower Budget";
    $scope.BudgetCodepopUpDataList = [];
    $scope.BudgetCodepopUp = function () {
        $scope.BudgetCodepopUpDataList = [];
        $scope.BudgetCodepopUpList = [];
        $scope.popUpUrl = 'employees/recruitment/getbudgetcodelist';
        //baseService.setCurrentPage('dataList');
        $scope.BudgetCodegetPopUpData = function (pageno) {
            baseService.paginationBase($scope.popUpUrl, pageno, $scope.budgetpopUpParameters)
                .then(function (result) {
                    $scope.BudgetCodepopUpDataList = result.Rows;
                    $scope.budgetpopUpParameters.total_count = result.Total;
                    if (baseService.arrayLength($scope.BudgetCodepopUpList) === 0) {
                        baseService.getDDLSearchColumn(result.Rows, $scope.BudgetCodepopUpList);
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure', 'BudgetCodepopUpId');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#BudgetCodepopUpId')).modal('show');
        $scope.BudgetCodegetPopUpData();
    };

    $scope.selectDoubleClick = function (data) {
        $scope.BudgetCodeNew.BudgetCodeId = data.Id;
        $scope.BudgetCodeNew.Code = data.Code;

        angular.element(document.querySelector('#BudgetCodepopUpId')).modal('hide');
    };

    $scope.clearCode = function () {
        $scope.BudgetCodeNew.BudgetCodeId = null;
        $scope.BudgetCodeNew.Code = null;

    };

    $scope.SaveBudgetCode = function () {
        if (baseService.isUndefinedOrNull($scope.ModelNew.Id)) {
            return ShowResult('Please select GL Management!', 'failure');
        }
        $http({
            method: 'POST',
            url: $scope.saveBudgetCodeUrl,
            data: { 'data': $scope.BudgetCodeNew, 'GlManagementId': $scope.ModelNew.Id },
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

    $scope.BudgetCodeList = [];
    $scope.GetBudgetCodeData = function (data) {
        $http({
            method: 'POST',
            url: $scope.path + "GetBudgetCodeData",
            data: { 'glManagementId': data },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.BudgetCodeList = response.data;
            for (var i = 0; i < $scope.BudgetCodeList.length; i++) {
                $scope.BudgetCodeNew.Code = $scope.BudgetCodeList[i].Code;
            }
        })
    }

    //#endregion BudgetCode

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