'use strict';
GLControlController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$controller', 'accountService'];
function GLControlController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $controller, accountService) {
    $rootScope.title = 'GL Control';
    $scope.Action = 'Save';
    $scope.ModelList = [];
    $scope.path = 'Accounts/GeneralAccountDeterminate/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'CreateGlControl';
    $scope.saveConsumableUrl = $scope.path + 'CreateComsumable';
    $scope.saveMaterialUrl = $scope.path + 'CreateMaterial';
    $scope.deleteUrl = $scope.path + 'DeleteGlControl/';
    baseService.init($scope.getListUrl);
    $scope.searchBy = "UserName"; $scope.search = "";
    $scope.searchByList = [{ value: 'Id', name: "Id" }, { value: 'Code', name: "Code" }, { value: 'ShortName', name: "Short Name" }, { value: 'StandardName', name: "Standard Name" }, { value: 'UserName', name: "User Name" }];
    $controller('baseMaterialAndArticleController', { $scope: $scope, $http: $http });

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.product = {
        Id: null,
        CompanyGroupId: null,
        CountryId: null,
        CompanyId: null,
        PositionCode: null,
        PlantId: null,
        EntityId: null,
        ProcurementDays: null,
        ProcurementFrequency: null,
        MaterialType: null,
        QualityStdSet: null,
        CostReductionCategory: null,
        MaterialMasterId: null,
        ArticleId: null,
        ArticleCriticality: null,
        FirstCharacteristicsId: null,
        FirstCharacteristicsValueId: null,
        SecondCharacteristicsId: null,
        SecondCharacteristicsValueId: null,
        ThirdCharacteristicsId: null,
        ThirdCharacteristicsValueId: null,
        MinStockLevel: null,
        MaxStockLevel: null,
        CostingPercentage: null,
        ProcurementPercentage: null,
        QualityApprovalReq: null,
        QualityApprovedBy: null,
        PossitionCodeForApproval: null,
        QualityStdSet: null,
        SupplierQualityReportReq: null,
        RequisitionType: null,
        PriceApproval: null,
        POGroupId: null,
        Imported: null,
        ImportedCurrencyId: null,
        ImportedBaseRate: null,
        ImportedTgtLandedRate: null,
        ImportProcurementLedTimeDays: null,
        ImportedMinimumOrderQty: null,
        ImportedArticleLifeDays: null,
        Local: null,
        LocalCurrencyId: null,
        LocalBaseRate: null,
        LocalTgtLandedRate: null,
        LocalProcurementLedTimeDays: null,
        LocalMinimumOrderQty: null,
        LocalArticleLifeDays: null,
        AutoPoGeneration: null,
        POGenerationCriteria: null,
        PoGenerationDay: null,
        LastProcurementRate: null,
        MinimumProcurementRate: null,
        MaximumProcurementRate: null,
        MaterialMasterName: null,
        ArticleName: null,
        ProcurementsPlanDay: null,
        Remarks: null
    };
    $scope.productNew = Object.assign({}, $scope.product);


    $scope.getData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetGlControlList",
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
        $scope.selectExpenseGL(args.data.Id);
        $scope.GetMaterialData(args.data.Id);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.Save = function () {
        var ids = "";
        if (baseService.arrayLength($scope.MaterialDataList) > 0) {
            for (var i = 0; i < $scope.MaterialDataList.length; i++) {
                if (ids == "") {
                    ids = "'','" + $scope.MaterialDataList[i].MaterialMasterId + "'";
                }
                else {
                    ids += ",'" + $scope.MaterialDataList[i].MaterialMasterId + "'";
                }
            }
        }

        $scope.$broadcast('show-errors-check-validity');
        if ($scope.ModelNewForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: { 'data': $scope.ModelNew, 'materialId': ids, 'materialList': $scope.MaterialDataList },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    ClearFields(response.data.Sequence);
                    $scope.getData();
                    $scope.selectIDs();
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
        if (checkItemExist($scope.MaterialDataList, data.Id) === false) {
            $scope.MaterialDataList.push({
                GLControlMasterId: $scope.ModelNew.Id,
                MaterialMasterId: data.Id,
                MaterialMaster: data.UserName
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

    $scope.GetCOAICodeList = function () {
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
            if (list[i].GLGeneralInfoId === data.GLGeneralInfoId && list[i].BudgetId === data.BudgetMasterId && list[i].ActivityId === data.ActivityId) {
                return true;
            }
        }
        return false;
    }

    $scope.setSelected = function (data) {
        if (checkConsumableExist($scope.ExpenseGLList, data) === false) {
            $scope.ExpenseGLList.push({
                GLGeneralInfoName: data.GLGeneralInfoName,
                BudgetName: data.BudgetName,
                ActivityName: data.ActivityName
            });
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

    $scope.SaveConsumable = function () {
        try {
            var ConsumableList = [];
            var ob = {};
            for (var i = 0; i < $scope.ExpenseGLList.length; i++) {
                ob.Id = null;
                ob.GLGeneralInfoId = $scope.ExpenseGLList[i].GLGeneralInfoId;
                ob.BudgetId = $scope.ExpenseGLList[i].BudgetMasterId;
                ob.ActivityId = $scope.ExpenseGLList[i].ActivityId;
                ConsumableList.push(ob);
                ob = {};
            }
            if (ConsumableList.length == 0) {
                throw "Please Select GL Control!";
            }
            $http({
                method: 'POST',
                url: $scope.saveConsumableUrl,
                data: { 'ConsumableList': ConsumableList, 'glControlId': $scope.ModelNew.Id },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.Action = 'Update';
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.tempIndex = [];
    $scope.RemoveExpense = function (data, index) {
        $scope.tempIndex = index;
        $scope.consumableId = data.Id;
        if (baseService.isUndefinedOrNull(data.UserName))
            $scope.message_confirmation = 'Are you sure want to remove this data....';
        else
            $scope.message_confirmation = 'Are you sure want to remove ?';
        angular.element(document.querySelector('#confirmgenericPopUp')).modal('show');
    };
    $scope.RemoveRow = function () {
        if (baseService.isUndefinedOrNull($scope.consumableId)) {
            $scope.ExpenseGLList.splice($scope.tempIndex, 1);
        }
        else {
            $http.get('Accounts/GeneralAccountDeterminate/DeleteConsumerable?Id=' + $scope.consumableId)
                .then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.selectExpenseGL();
                        //$scope.ClearConsumable();
                    }
                    function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    }
                });
        }
    };


    $scope.selectExpenseGL = function (data) {
        $http({
            method: 'POST',
            url: $scope.path + "GetConsumableData",
            data: { 'glControlDetailId': data },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ExpenseGLList = response.data;
        })
    }

    $scope.ClearConsumable = function () {
        $scope.Action = 'Save';
        $scope.ModelNew = Object.assign({}, $scope.ModelTemp);
        $scope.selectExpenseGL = [];
        return true;
    };


    // #endregion --------------------------------- Expense  -----------------------------------//


}