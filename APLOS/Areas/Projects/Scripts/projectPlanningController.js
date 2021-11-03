'use strict';
ProjectPlanningController.$inject = ['commonMessage', '$window', '$scope', '$rootScope', 'baseService', 'cboService', '$routeParams', '$location', '$http', '$filter', '$controller'];
function ProjectPlanningController(commonMessage, $window, $scope, $rootScope, baseService, cboService, $routeParams, $location, $http, $filter, $controller) {
    $rootScope.title = 'ProjectPlanning';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.projectPlannings = [];
    $scope.budgetMasterSelectedList = [];
    $scope.machineTypeMasterList = [];
    $scope.materialMasterList = [];
    $scope.machineTypeData = [];
    $scope.searchbyMachineTypelist = [];
    $scope.fixedAssetMasterFormList = [];
    $scope.materialMasterFormList = [];
    $scope.path = 'projects/projectPlanning/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    $controller("currencyBaseController", { $scope: $scope, $http: $http });
    $scope.searchByProjectPlanningList = [
        {
            'name': 'Code',
            'value': 'Code'
        },
        {
            'name': 'Description',
            'value': 'Description'
        },
        {
            'name': 'Title',
            'value': 'Title'
        },
        {
            'name': 'Status',
            'value': 'Status'
        }
    ];
    $scope.projectPlanningListParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'Code',
        searchBy: 'Code',
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.getProjectPlanning = function () {
        $scope.GetProjectPlanningListData = function (pageno) {
            baseService.paginationBase($scope.getListUrl, pageno, $scope.projectPlanningListParameters)
                .then(function (data) {
                    $scope.projectPlannings = data.Rows;
                    $scope.projectPlanningListParameters.total_count = data.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#projectPlanningPopUp')).modal('show');
        $scope.modalShow = true;
        $scope.GetProjectPlanningListData();
    };
    $scope.getProjectPlanning();

    $scope.GetProjectPlanningInfo = function (data) {
        data.FromDate = $filter('dateFiltering')(data.FromDate);
        data.ToDate = $filter('dateFiltering')(data.ToDate);
        $scope.projectPlanningNew = data;
        $scope.getExchangeRateAvailable();
        if ($scope.projectPlanningNew.EmployeeId != null) {
            $scope.projectPlanningNew.ResponsiblePersonId = $scope.projectPlanningNew.EmployeeId;
            $scope.projectPlanningNew.ResponsiblePerson = $scope.projectPlanningNew.EmployeeName;
            $scope.projectPlanningNew.ResponsiblePersonBy = 'Employee';
        }
        if ($scope.projectPlanningNew.PositionId != null) {
            $scope.projectPlanningNew.ResponsiblePersonId = $scope.projectPlanningNew.PositionId;
            $scope.projectPlanningNew.ResponsiblePerson = $scope.projectPlanningNew.PositionName;
            $scope.projectPlanningNew.ResponsiblePersonBy = 'Position';
        }
        if ($scope.projectPlanningNew.ManpowerBudgetId != null) {
            $scope.projectPlanningNew.ResponsiblePersonId = $scope.projectPlanningNew.ManpowerBudgetId;
            $scope.projectPlanningNew.ResponsiblePerson = $scope.projectPlanningNew.ManpowerBudgetName
            $scope.projectPlanningNew.ResponsiblePersonBy = 'Budget';
        }
       // $scope.getEntityWithChange()
        $scope.getProjectPlanningDetail();
       // angular.element(document.querySelector('#projectPlanningPopUp')).modal('hide');
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    }
    $scope.projectPlanning = {
        Id: null,
        CompanyId: null,
        PlantId: null,
        EntityId: null,
        Code: null,
        Description: null,
        Title: null,
        PositionId: null,
        ManpowerBudgetId: null,
        ResponsiblePersonBy: 'Employee',
        EmployeeId: null,
        ResponsiblePersonId: null,
        CurrencyId: null,
        ExchangeRate: null,
        FromDate: null,
        ToDate: null,
        Status: null,
        AddedBy: null,
        AddedDate: new Date(),
        AddedFromIP: null,
        UpdatedDate: null
    };
    $scope.ProjectPlanningDetail = {
        Id: null,
        ProjectPlanningId: null,
        BudgetMasterId: null,
        ProjectPlanningCategoryId: null,
        ProjectPlanningSubCategoryId: null,
        LocalAmount: null,
        ImportAmount: null,
        AddedBy: null,
        AddedDate: new Date(),
        AddedFromIP: null,
        UpdatedDate: null
    }
    $scope.ProjectPlaningFixedAsset = {
        Id: null,
        ProjectPlanningId: null,
        ProjectPlanningDetailId: null,
        FixedAssetMasterId: null,
        AssetItemId: null,
        AssetItemName: null,
        Quantity: null,
        AddedBy: null,
        AddedDate: new Date(),
        AddedFromIP: null,
        UpdatedDate: null
    }
    $scope.projectPlanningNew = Object.assign({}, $scope.projectPlanning);

    /***Cbo***************/
    cboService.getEnumCbo('enum/GetEnumForProjectPlanningStatus', function (result) {
        $scope.StatusList = result;
    });
    cboService.getCboPlant(function (result) {
        $scope.PlantList = result;
    });

    $scope.EntityList = [];
    //$scope.getEntityWithChange = function () {
    //    cboService.getCboProductionEntityByPlant(null, null, $scope.projectPlanningNew.PlantId, function (result) {
    //        $scope.EntityList = result;
    //    });
    //}
    $scope.getAllEntities = function () {
        $http({
            method: 'POST',
            url: "OrderManagements/productionOrderSchedulingParametersType1/GetEntity"
        }).then(function successCallback(response) {
            $scope.EntityList = response.data;
        });
    }
    $scope.getAllEntities();

    cboService.getCboProjectPlanningCategory(function (result) {
        $scope.ProjectPlanningCategoryList = result;
    });
    cboService.getCboProjectPlanningSubCategory(function (result) {
        $scope.ProjectPlanningSubCategoryList = result;
    });

    //cboService.getCboTransactionCurrencyByCompany('', function (result) {
    //    $scope.CurrencyList = result;
    //});

    $scope.validateCurrency = function (currency) {
        if (currency != $scope.companyBaseCurrencyId) {
            $scope.showExchangeRateCtrl = false;
        } else {
            $scope.showExchangeRateCtrl = true;
        }
    }

    $scope.CurrencyList = [];
    cboService.getCboTransactionCurrencyByCompany('', function (result) {
        $scope.CurrencyList = [];
        $scope.CurrencyList = result;
        $scope.projectPlanningNew.CurrencyId = $filter("filter")($scope.CurrencyList, { IsBaseCurrency: 1 })[0].CurrencyId;
        $scope.GetCurrencyExchangeRateList();
    });

    $scope.GetCurrencyExchangeRateList = function () {
        if (!baseService.isUndefinedOrNull($scope.projectPlanningNew.CurrencyId)) {
            $http({
                method: "GET",
                url: "currencies/ExchangeRate/GetCompanyCurrencyExchangeRate?fromdate=" + $filter("dateFiltering")(Date.now()) + "&currencyId=" + $scope.projectPlanningNew.CurrencyId
            }).then(function successCallback(response) {
                $scope.currencyExchangeRate = response.data;
                $scope.projectPlanningNew.ExchangeRate = $scope.currencyExchangeRate.ToCurrencyRate;
            });
            $scope.validateCurrency($scope.projectPlanningNew.CurrencyId);
        }
        else {
            $scope.currencyExchangeRate = [];
            $scope.showExchangeRateCtrl = false;
        }
    };

    function getCoaIdByCompany() {
        $http({
            method: 'GET',
            url: 'Projects/ProjectPlanning/GetCoaIdByCompany',
        }).then(function successCallback(response) {
            $scope.CoaData = response.data[0];
        })
    };
    getCoaIdByCompany();
    $scope.uOMList = [];
    cboService.getUoMCbo(function (result) {
        $scope.uOMList = result;
    });
    //$http({
    //    method: 'GET',
    //    url: '/setups/unitofmeasurement/getcbo/',
    //}).then(function successCallback(response) {
    //    $scope.uOMList = response.data;
    //});
    //--------------
    //******************Budget Master**************/
    $scope.budgetMasterTempList = [];
    $scope.selectBudgetMasterChValue = function (event, data) {
        try {
            if (event.currentTarget.checked) {
                if (checkExistTempBudgetMasterList($scope.budgetMasterTempList, data.Id) === false) {
                    $scope.budgetMasterTempList.push(data);
                }
            }
            else {
                for (var i = 0; i < $scope.budgetMasterTempList.length; i++) {
                    if ($scope.budgetMasterTempList[i].Id === data.Id) {
                        $scope.budgetMasterTempList.splice(i, 1);
                        break;
                    }
                }
            }
        } catch (e) {
            event.currentTarget.checked = false;
            ShowResult(e, 'failure');
        }
    }
    function checkExistTempBudgetMasterList(list, Id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].Id === Id) {
                return true;
            }
        }
        return false;
    }
    function getActive(list, id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].Id === id) {
                return true;
            }
        }
        return false;
    }
    $scope.ShowBudgetMasterForm = function () {
        $scope.ProjectPlanningDetail = {
            Id: null,
            ProjectPlanningId: null,
            BudgetMasterId: null,
            ProjectPlanningCategoryId: null,
            ProjectPlanningSubCategoryId: null,
            LocalAmount: null,
            ImportAmount: null,
            AddedBy: null,
            AddedDate: new Date(),
            AddedFromIP: null,
            UpdatedDate: null
        }
        $scope.budgetMasterSelectedList = [];
        angular.element(document.querySelector('#budgetMasterFormPopUp')).modal('show');
    }
    $scope.searchByBudgetMasterList = [
        {
            'name': 'GL',
            'value': 'GL'
        },
        {
            'name': 'Budget Category',
            'value': 'BudgetCategory'
        },
        {
            'name': 'Budget SubCategory',
            'value': 'BudgetSubCategory'
        },
        {
            'name': 'Budget',
            'value': 'BudgetName'
        },
        {
            'name': 'Budget Type',
            'value': 'BudgetType'
        }
        ,
        {
            'name': 'RefNo',
            'value': 'RefNo'
        }
    ];
    $scope.budgetMasterListParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'GLGeneralInfoCode,BudgetName',
        searchBy: 'BudgetName',
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.GetBudgetMasterList = function () {
        $scope.budgetMasterTempList = [];
        if (baseService.isUndefinedOrNull($scope.ProjectPlanningDetail.ProjectPlanningCategoryId)) {
            return ShowResult('Select Project Planning!!', 'failure');
        }
        if (baseService.isUndefinedOrNull($scope.ProjectPlanningDetail.ProjectPlanningSubCategoryId)) {
            return ShowResult('Select Project Planning SubCategory !!', 'failure');
        }
        $scope.GLUrl3 = 'Accounts/budgetmaster/getlist?coaId=' + $scope.CoaData.COAId
        baseService.setCurrentPage('BudgetMasterList');
        $scope.GetBudgetMasterListDatas = function (pageno) {
            baseService.paginationBase($scope.GLUrl3, pageno, $scope.budgetMasterListParameters)
                .then(function (data) {
                    $scope.BudgetMasterList = data.Rows;
                    $scope.budgetMasterListParameters.total_count = data.Total;
                    for (var i = 0; i < $scope.BudgetMasterList.length; i++) {
                        $scope.BudgetMasterList[i].Flag = getActive($scope.budgetMasterTempList, $scope.BudgetMasterList[i].Id);
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#budgetMasterPopUp')).modal('show');
        $scope.modalShow = true;
        $scope.GetBudgetMasterListDatas();
    };
    function checkExist(list, id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].BudgetMasterId === id) {
                return true;
            }
        }
        return false;
    }
    $scope.budgetMasterSelectdCloseListPopUp = function () {
        angular.forEach($scope.budgetMasterTempList, function (item) {
            if (item.Flag && checkExist($scope.budgetMasterSelectedList, item.Id) === false) {
                $scope.budgetMasterSelectedList.push(
                    {
                        Id: null,
                        BudgetMasterId: item.Id,
                        ProjectPlanningId: null,
                        LocalAmount: null,
                        ImportAmount: null,
                        GLGeneralInfoCode: item.GLGeneralInfoCode,
                        GLGeneralInfoName: item.GLGeneralInfoName,
                        BudgetCategory: item.BudgetCategory,
                        BudgetSubCategory: item.BudgetSubCategory,
                        BudgetName: item.BudgetName,
                        RefNo: item.RefNo,
                        ProjectPlanningCategoryId: $scope.ProjectPlanningDetail.ProjectPlanningCategoryId,
                        ProjectPlanningSubCategoryId: $scope.ProjectPlanningDetail.ProjectPlanningSubCategoryId,
                    }
                );
            }
        });
        angular.element(document.querySelector('#budgetMasterPopUp')).modal('hide');
        if ($scope.budgetMasterSelectedList.length > 0) {
            $scope.tableShow = true;
        }
        else {
            $scope.tableShow = false;
        }
    }
    $scope.budgetMasterFormCloseListPopUp = function () {
        angular.element(document.querySelector('#budgetMasterFormPopUp')).modal('hide');
    }
    //-----------------
    //getProjectPlanningDetail***********/
    $scope.ProjectPlanningDetailSavedList = [];
    $scope.getProjectPlanningDetail = function () {
        $http({
            method: 'GET',
            url: 'projects/projectplanning/getprojectplanningdetail?plantId=' + $scope.projectPlanningNew.PlantId + '&projectPlanningId=' + $scope.projectPlanningNew.Id,
        }).then(function successCallback(response) {
            $scope.ProjectPlanningDetailSavedList = response.data;
            //console.log('ProjectPlanningDetailSavedList', $scope.ProjectPlanningDetailSavedList);
        })
    }

    //-----------
    //*************Machine Type Search PopUp************/
    $scope.fixedAssetTypes = [
        {
            Value: 'Machine',
            Text: 'Machine'
        },
        {
            Value: 'Equipment',
            Text: 'Equipment'
        },
        {
            Value: 'Plant',
            Text: 'Plant'
        },
        {
            Value: 'Vahical',
            Text: 'Vahical'
        },
        {
            Value: 'Other',
            Text: 'Other'
        },
    ]
    $scope.searchByMachineTypeList = [
        {
            'name': 'Asset Item',
            'value': 'AssetItemName'
        },
        {
            'name': 'Asset Class',
            'value': 'AssetClassName'
        },
        {
            'name': 'Asset SubClass',
            'value': 'AssetSubClassName'
        },
        {
            'name': 'Fixed Asset Master',
            'value': 'FixedAssetMasterName'
        },
        {
            'name': 'Asset Category',
            'value': 'FixedAssetCategoryName'
        },
        {
            'name': 'Asset SubCategory',
            'value': 'FixedAssetSubCategoryName'
        }
    ]
    $scope.machineTypeListParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'AssetItemName',
        searchBy: 'AssetItemName',
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.getMachineTypeData = function () {
        $scope.machineTypeMasterList = [];
        baseService.setCurrentPage('machineTypeMasterList');
        $scope.loadMachineTypeMasterData = function (pageno) {
            //baseService.paginationBase('/fixedassets/fixedassetmaster/getlistfordynamicpopup', pageno, $scope.machineTypeListParameters)
            baseService.paginationBase('Machines/AssetItem/GetAssetItemWithAssetType?assetType=' + $scope.projectPlanningNew.AssetType, pageno, $scope.machineTypeListParameters)
                .then(function (result) {
                    for (var i = 0; i < result.Rows.length; i++) {
                        result.Rows[i].ProjectPlanningId = $scope.projectPlanningNew.Id;
                        result.Rows[i].ProjectPlanningDetailId = $scope.ProjectPlanningDetailId;
                    }
                    $scope.machineTypeMasterList = result.Rows;
                    console.log('new', $scope.machineTypeMasterList);
                    $scope.machineTypeListParameters.total_count = result.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        }; $scope.loadMachineTypeMasterData();
    };
    $scope.machineTypeMasterSearchPopup = function () {
        $scope.getMachineTypeData();
        angular.element(document.querySelector('#machineTypeMasterModal')).modal('show');
    };
    function checkMachineTypeMasterExist(list, id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].AssetItemId === id) {
                return true;
            }
        }
        return false;
    }
    $scope.ProjectPlaningMachineTypeListForSave = [];
    function ProjectPlaningMachineTypeListSave() {
        angular.forEach($scope.machineTypeMasterList, function (item) {
            if (item.Flag) {
                if (checkMachineTypeMasterExist($scope.ProjectPlaningMachineTypeListForSave, item.Id) === false) {
                    $scope.ProjectPlaningMachineTypeListForSave.push(
                        {
                            Id: null,
                            AssetItemId: item.Id,
                            ProjectPlanningId: item.ProjectPlanningId,
                            ProjectPlanningDetailId: item.ProjectPlanningDetailId,
                            AssetItemName: item.AssetItemName,
                            AssetClassName: item.AssetClassName,
                            AssetSubClassName: item.AssetSubClassName,
                            FixedAssetMasterName: item.FixedAssetMasterName,
                            BaseUOM: item.BaseUom,
                            BaseUOMId: item.BaseUOMId,
                            AlernativeUomLists: buildAssetItemUomDropDown($scope.alterNativeAssetItemUomList, item.Id),
                            PlanningUOMId: selectedAssetItemDDL(buildAssetItemUomDropDown($scope.alterNativeAssetItemUomList, item.Id), item.BaseUOMId),
                            Quantity: null,
                        }
                    );
                }
            }
        })
    }
    function selectedAssetItemDDL(list, id) {
        try {
            var uomId = null;
            for (var i = 0; i < list.length; i++) {
                if (list[i].Value === id) {
                    uomId = list[i].Value;
                    return uomId;
                }
            }
            return uomId;
        } catch (e) {
        }
    }
    $scope.machineTypeMasterModalCloseListPopUp = function () {
        ProjectPlaningMachineTypeListSave();
        console.log('machineTypeMasterList', $scope.machineTypeMasterList);
        //$scope.ProjectPlaningFixedAsset.FixedAssetMasterId = data.Id;
        angular.element(document.querySelector('#machineTypeMasterModal')).modal('hide');
    }
    //------------------
    //*************MachineTypeMasterForm************/
    $scope.getAssetItemUomList = function (materailMasterId) {
        $http({
            method: 'GET',
            url: 'projects/projectplanning/GetAssetItemUomList?materailMasterId=' + $scope.materailMasterId,
        }).then(function successCallback(response) {
            $scope.alterNativeAssetItemUomList = response.data;
        })
    };
    $scope.machineTypeMasterFormSearchPopup = function (Id) {
        $scope.ProjectPlanningDetailId = Id;
        $scope.getMachineTypeMasterSavedList();
        angular.element(document.querySelector('#machineTypeMasterFormModal')).modal('show');
    };
    $scope.getMachineTypeMasterSavedList = function () {
        $scope.getAssetItemUomList();
        $http({
            method: 'GET',
            url: 'projects/projectplanning/getprojectplanningMachineTypeMaster?projectPlanningDetailId=' + $scope.ProjectPlanningDetailId,
        }).then(function successCallback(response) {
            $scope.ProjectPlaningMachineTypeListForSave = response.data.Rows;
            for (var i = 0; i < $scope.ProjectPlaningMachineTypeListForSave.length; i++) {
                $scope.ProjectPlaningMachineTypeListForSave[i].AlernativeUomLists = buildAssetItemUomDropDown($scope.alterNativeAssetItemUomList, $scope.ProjectPlaningMachineTypeListForSave[i].AssetItemId);
            }
        })
    }
    $scope.machineTypeMasterFormModalCloseListPopUp = function () {
        angular.element(document.querySelector('#machineTypeMasterFormModal')).modal('hide');
    }
    //------------------
    //*************MaterialMaster************/
    $scope.getUomList = function (materailMasterId) {
        $http({
            method: 'GET',
            url: 'projects/projectplanning/GetUomList?materailMasterId=' + $scope.materailMasterId,
        }).then(function successCallback(response) {
            $scope.alterNativeUomList = response.data;
        })
    };
    $scope.searchbyMaterailMasterList = [
        {
            'name': 'Code',
            'value': 'Code'
        },
        {
            'name': 'User Name',
            'value': 'UserName'
        },
        {
            'name': 'BaseUom',
            'value': 'BaseUom'
        },
        {
            'name': 'Material Group',
            'value': 'MaterialGroupMaster'
        },
    ]
    $scope.materialMasterListParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'UserName',
        searchBy: 'UserName',
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.getMaterailMasterData = function () {
        $scope.materialMasterList = [];
        baseService.setCurrentPage('materialMasterList');
        $scope.loadMaterialMasterData = function (pageno) {
            baseService.paginationBase('projects/projectplanning/getprojectplanningmaterialmaster?budgetMstId=' + $scope.pDetailBudgetMasterId, pageno, $scope.materialMasterListParameters)
                .then(function (result) {
                    if (result.Rows.length < 1) {
                        throw ShowResult("No asset item is taged with this budget", 'failure', 'materialMasterFormModal');
                    } else {
                        for (var i = 0; i < result.Rows.length; i++) {
                            result.Rows[i].ProjectPlanningId = $scope.projectPlanningNew.Id;
                            result.Rows[i].ProjectPlanningDetailId = $scope.ProjectPlanningDetailId;
                        }
                        $scope.materialMasterList = result.Rows;
                        $scope.materialMasterListParameters.total_count = result.Total;
                        angular.element(document.querySelector('#MaterialMasterModal')).modal('show');
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        }; $scope.loadMaterialMasterData();
    };
    $scope.materialMasterSearchPopup = function () {
        $scope.getMaterailMasterData();
    };
    function checkMaterialMasterExist(list, id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].MaterialMasterId === id) {
                return true;
            }
        }
        return false;
    }
    $scope.ProjectPlaningMaterialListForSave = [];
    function ProjectPlaningMaterialListSave() {
        angular.forEach($scope.materialMasterList, function (item) {
            if (item.Flag) {
                if (checkMaterialMasterExist($scope.ProjectPlaningMaterialListForSave, item.Id) === false) {
                    $scope.ProjectPlaningMaterialListForSave.push(
                        {
                            MaterialMasterId: item.Id,
                            Id: null,
                            MaterialType: item.MaterialType,
                            Nature: item.Nature,
                            IsAsset: item.IsAsset,
                            MaterialGroupMaster: item.MaterialGroupMaster,
                            ProjectPlanningId: item.ProjectPlanningId,
                            FixedAssetMasterId: item.FixedAssetMasterId,
                            FixedAssetMasterName: item.FixedAssetMasterName,
                            Code: item.Code,
                            UserName: item.UserName,
                            GridName: item.GridName,
                            ProductMaster: item.ProductMaster,
                            BaseUom: item.BaseUom,
                            BaseUOMId: item.BaseUOMId,
                            Description: item.Description,
                            MaterialGridId: item.MaterialGridId,
                            AlernativeUomLists: buildUomDropDown($scope.alterNativeUomList, item.Id),
                            PlanningUOMId: selectedDDL(buildUomDropDown($scope.alterNativeUomList, item.Id)),
                            ProjectPlanningDetailId: item.ProjectPlanningDetailId,
                            Quantity: null,
                            MaterialMasterType: 'Asset'
                        }
                    );
                }
            }
        })
    }
    function selectedDDL(list) {
        try {
            var uomId = null;
            for (var i = 0; i < list.length; i++) {
                if (list[i].IsPo) {
                    uomId = list[i].Value;
                    return uomId;
                }
            }
            return uomId;
        } catch (e) {
        }
    }
    $scope.materialMasterModalCloseListPopUp = function () {
        ProjectPlaningMaterialListSave();
        //$scope.ProjectPlaningFixedAsset.FixedAssetMasterId = data.Id;
        angular.element(document.querySelector('#MaterialMasterModal')).modal('hide');
    }
    //*************Other MaterialMaster************/
    $scope.searchbyMaterailMasterOtherList = [
        {
            'name': 'Code',
            'value': 'Code'
        },
        {
            'name': 'User Name',
            'value': 'UserName'
        },
        {
            'name': 'BaseUom',
            'value': 'BaseUom'
        },
        {
            'name': 'Material Group',
            'value': 'MaterialGroupMaster'
        },
    ]
    $scope.materialMasterOtherListParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'UserName',
        searchBy: 'UserName',
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.getMaterailMasterOtherData = function () {
        $scope.materialMasterOtherList = [];
        baseService.setCurrentPage('materialMasterOtherList');
        $scope.loadMaterialMasterOtherData = function (pageno) {
            baseService.paginationBase('projects/projectplanning/GetProjectplanningNonAssetMaterialMaster', pageno, $scope.materialMasterOtherListParameters)
                .then(function (result) {
                    for (var i = 0; i < result.Rows.length; i++) {
                        result.Rows[i].ProjectPlanningId = $scope.projectPlanningNew.Id;
                        result.Rows[i].ProjectPlanningDetailId = $scope.ProjectPlanningDetailId;
                    }
                    $scope.materialMasterOtherList = result.Rows;
                    $scope.materialMasterOtherListParameters.total_count = result.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        }; $scope.loadMaterialMasterOtherData();
    };
    $scope.materialMasterOtherSearchPopup = function () {
        $scope.getMaterailMasterOtherData();
        angular.element(document.querySelector('#MaterialMasterOtherModal')).modal('show');
    };
    $scope.ProjectPlaningMaterialOtherListForSave = [];
    function ProjectPlaningMaterialOtherListSave() {
        angular.forEach($scope.materialMasterOtherList, function (item) {
            if (item.Flag) {
                if (checkMaterialMasterExist($scope.ProjectPlaningMaterialOtherListForSave, item.Id) === false) {
                    $scope.ProjectPlaningMaterialOtherListForSave.push(
                        {
                            MaterialMasterId: item.Id,
                            Id: null,
                            MaterialType: item.MaterialType,
                            Nature: item.Nature,
                            IsAsset: item.IsAsset,
                            MaterialGroupMaster: item.MaterialGroupMaster,
                            ProjectPlanningId: item.ProjectPlanningId,
                            FixedAssetMasterId: item.FixedAssetMasterId,
                            FixedAssetMasterName: item.FixedAssetMasterName,
                            Code: item.Code,
                            UserName: item.UserName,
                            GridName: item.GridName,
                            ProductMaster: item.ProductMaster,
                            BaseUom: item.BaseUom,
                            BaseUOMId: item.BaseUOMId,
                            Description: item.Description,
                            MaterialGridId: item.MaterialGridId,
                            AlernativeUomLists: buildUomDropDown($scope.alterNativeUomList, item.Id),
                            PlanningUOMId: selectedDDL(buildUomDropDown($scope.alterNativeUomList, item.Id)),
                            ProjectPlanningDetailId: item.ProjectPlanningDetailId,
                            Quantity: null,
                            MaterialMasterType: 'AllMaterialMaster'
                        }
                    );
                }
            }
        })
    }
    $scope.materialMasterOtherModalCloseListPopUp = function () {
        ProjectPlaningMaterialOtherListSave();
        //$scope.ProjectPlaningFixedAsset.FixedAssetMasterId = data.Id;
        angular.element(document.querySelector('#MaterialMasterOtherModal')).modal('hide');
    }
    /***UOM************/
    var finalUomDropDownList = [];
    function buildUomDropDown(list, id) {
        finalUomDropDownList = [];
        for (var i = 0; i < list.length; i++) {
            if (list[i].Id === id) {
                if (finalUomDropDownList.length > 0) {
                    if (getIsExistsgUOM(finalUomDropDownList, list[i].UoMID) === false) {
                        finalUomDropDownList.push({
                            Text: list[i].UoM,
                            Value: list[i].UoMID,
                            Id: list[i].Id,
                            IsPo: setPo(list, list[i].Id, list[i].UoMID),
                        });
                    }
                } else {
                    finalUomDropDownList.push({
                        Text: list[i].UoM,
                        Value: list[i].UoMID,
                        Id: list[i].Id,
                        IsPo: setPo(list, list[i].Id, list[i].UoMID),
                    });
                }
            }
        }

        return finalUomDropDownList;
    }
    function setPo(list, id, uomId) {
        try {
            var hasValue = false;
            for (var i = 0; i < list.length; i++) {
                if (list[i].Id === id && list[i].UoMID === uomId && list[i].IsPo) {
                    hasValue = true;
                }
            }
            return hasValue;
        } catch (e) {
        }
    }
    //#region ************* AssetItemUom***********
    function getIsExistsgUOM(list, id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].Value === id) {
                return true;
            }
        }
        return false;
    }
    var finalAssetItemUomDropDownList = [];
    function buildAssetItemUomDropDown(list, id) {
        finalAssetItemUomDropDownList = [];
        for (var i = 0; i < list.length; i++) {
            if (list[i].Id === id) {
                if (finalAssetItemUomDropDownList.length > 0) {
                    if (getIsExistsgUOM(finalAssetItemUomDropDownList, list[i].UoMID) === false) {
                        finalAssetItemUomDropDownList.push({
                            Text: list[i].UoM,
                            Value: list[i].UoMID,
                            Id: list[i].Id,
                        });
                    }
                } else {
                    finalAssetItemUomDropDownList.push({
                        Text: list[i].UoM,
                        Value: list[i].UoMID,
                        Id: list[i].Id,
                    });
                }
            }
        }

        return finalAssetItemUomDropDownList;
    }
    //------------------
    //*******Material MasterForm*****************//
    $scope.materialMasterFormSearchPopup = function (data) {
        $scope.ProjectPlanningDetailId = data.Id;
        $scope.pDetailBudgetMasterId = data.BudgetMasterId;
        $scope.pDetailBudgetId = data.BudgetId;
        $scope.getmaterialMasterSavedList();
        angular.element(document.querySelector('#materialMasterFormModal')).modal('show');
    };
    $scope.getmaterialMasterSavedList = function () {
        $scope.ProjectPlaningMaterialListForSave = [];
        $scope.ProjectPlaningMaterialOtherListForSave = [];
        $scope.getUomList();
        $http({
            method: 'GET',
            url: 'projects/projectplanning/GetProjectplanningMaterialMasterSavedList?projectPlanningDetailId=' + $scope.ProjectPlanningDetailId,
        }).then(function successCallback(response) {
            var obList = response.data.Rows;
            for (var i = 0; i < obList.length; i++) {
                obList[i].AlernativeUomLists = buildUomDropDown($scope.alterNativeUomList, obList[i].MaterialMasterId);
                if (obList[i].MaterialMasterType === 'Asset') {
                    $scope.ProjectPlaningMaterialListForSave.push(obList[i]);
                } else if (obList[i].MaterialMasterType === 'AllMaterialMaster') {
                    $scope.ProjectPlaningMaterialOtherListForSave.push(obList[i]);
                }
            }
        });
    }
    $scope.materialMasterFormModalCloseListPopUp = function () {
        angular.element(document.querySelector('#materialMasterFormModal')).modal('hide');
    }
    //-------------
    /*****ResposiblePerson*****************/
    $scope.excludeList = ['Image', 'Flag']

    $scope.popUpParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: '',
        searchBy: '',
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    function getEmployeeInformationData() {
        $scope.sbEmployeeInformation = [];
        $scope.employeeinformationData = [];
        var name = $scope.projectPlanningNew.ResponsiblePersonBy;
        $scope.popUpTitle = '';
        var popUpUrl = '';
        if (name == 'Position') {
            $scope.popUpTitle = 'Position Profile';
            popUpUrl = 'organizations/Position/GetList';
            $scope.popUpParameters.sort = 'UserName';
            $scope.popUpParameters.searchBy = 'UserName';
            $scope.popUpParameters.EntityId = null;
            $scope.popUpParameters.PlantId = null;
        }
        else if (name == 'Budget') {
            $scope.popUpTitle = 'ManPowerBudget Profile';
            popUpUrl = 'Organizations/ManpowerBudget/GetListByPlant';
            $scope.popUpParameters.sort = 'Code';
            $scope.popUpParameters.searchBy = 'Code';
            $scope.popUpParameters.EntityId = $scope.projectPlanningNew.EntityId;
            $scope.popUpParameters.PlantId = $scope.projectPlanningNew.PlantId;
        }
        else {
            $scope.popUpTitle = 'Employee Profile';
            popUpUrl = 'employees/EmployeeInformation/GetPlantEmployeeList';
            //popUpUrl = 'employees/approvalconfiguration/getallemployeedata';
            
            $scope.popUpParameters.sort = 'EmployeeCode';
            $scope.popUpParameters.searchBy = 'FirstName';
            $scope.popUpParameters.employeeIds = '[' + $scope.projectPlanningNew.EmployeeId + ']';
        }
        baseService.setCurrentPage('dataList');
        $scope.loadEIData = function (pageno) {
            baseService.paginationBase(popUpUrl, pageno, $scope.popUpParameters)
                .then(function (result) {
                    $scope.employeeinformationData = result.Rows;
                    $scope.popUpParameters.total_count = result.Total;
                    if (baseService.arrayLength($scope.sbEmployeeInformation) == 0) {
                        baseService.getDDLSearchColumn(result.Rows, $scope.sbEmployeeInformation);
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure', 'popUpId');
                }).finally(function () {
                });
        };
        $scope.loadEIData();
    }
    $scope.showEmployeeInformationModal = function () {
        getEmployeeInformationData();
        angular.element(document.querySelector('#employeepopup')).modal('show');
    };
    $scope.getEmployee = function (ob) {
        var name = $scope.projectPlanningNew.ResponsiblePersonBy;
        console.log('ob', ob);
        if (name == 'Position') {
            $scope.projectPlanningNew.ResponsiblePerson = ob.UserName;
            $scope.projectPlanningNew.ResponsiblePersonId = ob.Id;
            $scope.projectPlanningNew.PositionId = ob.Id;
        }
        else if (name == 'Budget') {
            $scope.projectPlanningNew.ResponsiblePerson = ob.Position;
            $scope.projectPlanningNew.ResponsiblePersonId = ob.Id;
            $scope.projectPlanningNew.ManpowerBudgetId = ob.Id;
        }
        else {
            $scope.projectPlanningNew.ResponsiblePerson = ob.EmployeeName;
            $scope.projectPlanningNew.ResponsiblePersonId = ob.SystemId;
            $scope.projectPlanningNew.EmployeeId = ob.SystemId;
        }
        angular.element(document.querySelector('#employeepopup')).modal('hide');
    }
    $scope.clearResponsiblePerson = function () {
        $scope.projectPlanningNew.ResponsiblePerson = null;
        $scope.projectPlanningNew.ResponsiblePersonId = null;
    }
    //-----------------
    $scope.showExchangeRateCtrl = true;
    $scope.currencyExchangeName = '';
    $scope.companyBaseCurrencyId = null;
    $scope.getExchangeRateAvailable = function () {
        $http({
            method: 'GET',
            url: 'projects/projectplanning/getcompanycurrencycountrywise',
        }).then(function successCallback(response) {
            $scope.currencyExchangeName = response.data[0].Code;
            $scope.companyBaseCurrencyId = response.data[0].BaseCurrencyId;
        })
    }
    $scope.getExchangeRateAvailable();
    //Deleting Rows from ProjectPlanningList
    $scope.valuePassInProjectPlanningDelModal = function (index, Id) {
        $scope.ProjectPlanningId = Id;
        $scope.pIndex = index;
        if (baseService.isUndefinedOrNull($scope.id))
            $scope.message_confirmation = 'Are you sure want to delete this data....';
        else
            $scope.message_confirmation = 'Are you sure want to delete [ ' + $scope.ProjectPlanningId + ' ]';
        angular.element(document.querySelector('#confirmgenericPopUpForProjectPlanningSaved')).modal('show');
    };

    $scope.DeleteProjectPlanningSavedItem = function () {
        for (var i = 0; i < $scope.projectPlannings.length; i++) {
            if ($scope.projectPlannings[i].Id == $scope.ProjectPlanningId) {
                $http({
                    method: 'POST',
                    url: 'projects/projectplanning/Delete?id=' + $scope.ProjectPlanningId,
                }).then(function successCallback(response) {
                    ShowResult(response.data.Message, 'success');
                    $scope.projectPlannings.splice($scope.pIndex, 1);
                    $scope.getProjectPlanning();
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
            }
        }
        $scope.ProjectPlanningId = null;
        $scope.pIndex = -1;
    };
    //
    //Deleting Rows from BudgetSelectedList
    $scope.valuePassInDelModal = function (index, Id) {
        $scope.FixedAssetMasterId = Id;
        $scope.index = index;
        if (baseService.isUndefinedOrNull($scope.id))
            $scope.message_confirmation = 'Are you sure want to delete this data....';
        else
            $scope.message_confirmation = 'Are you sure want to delete [ ' + $scope.FixedAssetMasterId + ' ]';
        angular.element(document.querySelector('#confirmgenericPopUp')).modal('show');
    };

    $scope.DeleteBudgetItem = function () {
        for (var i = 0; i < $scope.budgetMasterSelectedList.length; i++) {
            if ($scope.budgetMasterSelectedList[i].BudgetMasterId == $scope.FixedAssetMasterId) {
                $scope.budgetMasterSelectedList.splice($scope.index, 1);
            }
        }
        $scope.FixedAssetMasterId = null;
        $scope.index = null;
        if ($scope.budgetMasterSelectedList.length > 0) {
            $scope.tableShow = true;
        }
        else {
            $scope.tableShow = false;
        }
    };
    //
    //Deleting Rows from BudgetSaveList
    $scope.valuePassInBudgetSavedDelModal = function (index, Id) {
        $scope.BudgetMasterId = Id;
        $scope.bIndex = index;
        if (baseService.isUndefinedOrNull($scope.id))
            $scope.message_confirmation = 'Are you sure want to delete this data....';
        else
            $scope.message_confirmation = 'Are you sure want to delete [ ' + BudgetMasterId + ' ]';
        angular.element(document.querySelector('#confirmgenericPopUpForBudgetSaved')).modal('show');
    };

    $scope.DeleteBudgetSavedItem = function () {
        for (var i = 0; i < $scope.ProjectPlanningDetailSavedList.length; i++) {
            if ($scope.ProjectPlanningDetailSavedList[i].Id == $scope.BudgetMasterId) {
                $http({
                    method: 'POST',
                    url: 'projects/projectplanning/DeleteProjectPlanningDetail?id=' + $scope.BudgetMasterId,
                }).then(function successCallback(response) {
                    ShowResult(response.data.Message, 'success');
                    $scope.ProjectPlanningDetailSavedList.splice($scope.bIndex, 1);
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
            }
        }
        $scope.BudgetMasterId = null;
        $scope.bIndex = null;
    };
    //
    //Deleting Rows from MachineFormList
    $scope.valuePassInMachineFormDelModal = function (index, Id) {
        $scope.AssetItemId = Id;
        $scope.mIndex = index;
        if (baseService.isUndefinedOrNull($scope.AssetItemId))
            $scope.message_confirmation = 'Are you sure want to delete this data....';
        else
            $scope.message_confirmation = 'Are you sure want to delete [ ' + $scope.AssetItemId + ' ]';
        angular.element(document.querySelector('#confirmgenericPopUpForMachineForm')).modal('show');
    };

    $scope.DeleteMachineSavedItem = function () {
        if (baseService.isUndefinedOrNull($scope.AssetItemId)) {
            $scope.ProjectPlaningMachineTypeListForSave.splice($scope.mIndex, 1);
        } else {
            for (var i = 0; i < $scope.ProjectPlaningMachineTypeListForSave.length; i++) {
                if ($scope.ProjectPlaningMachineTypeListForSave[i].Id == $scope.AssetItemId) {
                    $http({
                        method: 'GET',
                        url: 'projects/projectplanning/DeleteProjectPlanningMachineType?id=' + $scope.AssetItemId,
                    }).then(function successCallback(response) {
                        ShowResult(response.data.Message, 'success');
                        $scope.ProjectPlaningMachineTypeListForSave.splice($scope.mIndex, 1);
                    }, function () {
                        ShowResult(commonMessage.NetworkError, 'failure');
                    }).finally(function () {
                    })
                }
            }
        }
        $scope.AssetItemId = null;
        $scope.mIndex = null;
    };
    //
    //Deleting Rows from MaterialFormList
    $scope.valuePassInMaterialFormDelModal = function (index, Id, listName) {
        $scope.MaterialMasterId = Id;
        $scope.mTIndex = index;
        $scope.materialMasterSaveListName = listName;
        if (baseService.isUndefinedOrNull($scope.MaterialMasterId))
            $scope.message_confirmation = 'Are you sure want to delete this data....';
        else
            $scope.message_confirmation = 'Are you sure want to delete [ ' + $scope.MaterialMasterId + ' ]';
        angular.element(document.querySelector('#confirmgenericPopUpForMaterialForm')).modal('show');
    };
    $scope.DeleteMaterialSavedItem = function () {
        if (baseService.isUndefinedOrNull($scope.MaterialMasterId)) {
            $scope[$scope.materialMasterSaveListName].splice($scope.mTIndex, 1);
        } else {
            for (var i = 0; i < $scope[$scope.materialMasterSaveListName].length; i++) {
                if ($scope[$scope.materialMasterSaveListName][i].Id == $scope.MaterialMasterId) {
                    $http({
                        method: 'POST',
                        url: 'projects/projectplanning/DeleteProjectPlanningMaterial?id=' + $scope.MaterialMasterId,
                    }).then(function successCallback(response) {
                        ShowResult(response.data.Message, 'success');
                        $scope[$scope.materialMasterSaveListName].splice($scope.mTIndex, 1);
                    }, function () {
                        ShowResult(commonMessage.NetworkError, 'failure');
                    }).finally(function () {
                    })
                }
            }
        }
        $scope.MaterialMasterId = null;
        $scope.mTIndex = null;
    };
    //
    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.projectPlanning = $scope.projectPlannings[$scope.index];
        $scope.projectPlanningNew = Object.assign({}, $scope.projectPlanning);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };
    function checkAmount() {
        var list = $scope.budgetMasterSelectedList.length > 0 ? $scope.budgetMasterSelectedList : $scope.ProjectPlanningDetailSavedList;
        for (var i = 0; i < list.length; i++) {
            var ob = list[i];
            if (ob.Amount < 1) {
                return false;
                break;
            }
        }
        return true;
    }
    function getProjectPlanningById(Id) {
        $http({
            method: 'GET',
            url: 'projects/projectplanning/GetProjectPlanningById?id=' + Id,
        }).then(function successCallback(response) {
            $scope.projectPlanningNew = response.data.Rows[0];
            $scope.projectPlanningNew.FromDate = $filter('dateFiltering')($scope.projectPlanningNew.FromDate);
            $scope.projectPlanningNew.ToDate = $filter('dateFiltering')($scope.projectPlanningNew.ToDate);
            if ($scope.projectPlanningNew.EmployeeId != null) {
                $scope.projectPlanningNew.ResponsiblePersonId = $scope.projectPlanningNew.EmployeeId;
                $scope.projectPlanningNew.ResponsiblePerson = $scope.projectPlanningNew.EmployeeName;
                $scope.projectPlanningNew.ResponsiblePersonBy = 'Employee';
            }
            if ($scope.projectPlanningNew.PositionId != null) {
                $scope.projectPlanningNew.ResponsiblePersonId = $scope.projectPlanningNew.PositionId;
                $scope.projectPlanningNew.ResponsiblePerson = $scope.projectPlanningNew.PositionName;
                $scope.projectPlanningNew.ResponsiblePersonBy = 'Position';
            }
            if ($scope.projectPlanningNew.ManpowerBudgetId != null) {
                $scope.projectPlanningNew.ResponsiblePersonId = $scope.projectPlanningNew.ManpowerBudgetId;
                $scope.projectPlanningNew.ResponsiblePerson = $scope.projectPlanningNew.ManpowerBudgetName;
                $scope.projectPlanningNew.ResponsiblePersonBy = 'Budget';
            }
            $scope.getProjectPlanningDetail();
        })
    }
    $scope.Save = function () {
        angular.copy($scope.projectPlanningNew, $scope.projectPlanning);
        if (new Date($scope.projectPlanningNew.FromDate) > new Date($scope.projectPlanningNew.ToDate)) {
            return ShowResult('From Date must be less then To Date!!', 'failure');
        }
        if (!$scope.showExchangeRateCtrl && !$scope.projectPlanningNew.ExchangeRate > 0) {
            return ShowResult('Exchange rate required!!', 'failure');
        }
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.projectPlanningForm.$valid) {
            if ($scope.Action == 'Save') {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: { 'projectPlanning': $scope.projectPlanning },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        getProjectPlanningById(response.data.ProjectPlanningId);
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            }
            else if ($scope.Action == 'Update') {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: $scope.projectPlanning,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -1) {
                            $scope.projectPlannings[$scope.index] = $scope.projectPlanning;
                            $scope.projectPlannings = $filter('orderBy')($scope.projectPlannings, 'Sequence');
                        }
                    }
                }, function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                });
            }
        }
    }
    $scope.BudgetMasterSave = function () {
        angular.copy($scope.projectPlanningNew, $scope.projectPlanning);
        if ($scope.budgetMasterSelectedList.length < 1) {
            return ShowResult('Select at least one row!!', 'failure');
        }
        if (checkAmount() === false) {
            return ShowResult('Budget Master amount can not be less then 1!!', 'failure');
        }
        if (baseService.isUndefinedOrNull($scope.ProjectPlanningDetail.ProjectPlanningCategoryId)) {
            return ShowResult('Project Planning Category Required!!', 'failure');
        }
        if (baseService.isUndefinedOrNull($scope.ProjectPlanningDetail.ProjectPlanningSubCategoryId)) {
            return ShowResult('Project Planning Sub Category Required !!', 'failure');
        }
        if ($scope.Action == 'Save') {
            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: { 'projectPlanning': $scope.projectPlanning, 'ProjectPlanningDetail': $scope.budgetMasterSelectedList.length > 0 ? $scope.budgetMasterSelectedList : $scope.ProjectPlanningDetailSavedList },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    getProjectPlanningById(response.data.ProjectPlanningId);
                    $scope.budgetMasterFormCloseListPopUp();
                    $scope.budgetMasterSelectedList = [];
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        }
        else if ($scope.Action == 'Update') {
            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: $scope.projectPlanning,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    if ($scope.index > -1) {
                        $scope.projectPlannings[$scope.index] = $scope.projectPlanning;
                        $scope.projectPlannings = $filter('orderBy')($scope.projectPlannings, 'Sequence');
                    }
                }
            }, function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            });
        }
    }
    $scope.FixedAssetMasterSave = function () {
        angular.copy($scope.projectPlanningNew, $scope.projectPlanning);
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.fixedAssetMasterForm.$valid) {
            if ($scope.Action == 'Save') {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: { 'projectPlanning': $scope.projectPlanning, 'ProjectPlanningDetail': $scope.budgetMasterSelectedList.length > 0 ? $scope.budgetMasterSelectedList : $scope.ProjectPlanningDetailSavedList, 'ProjectPlanningFixedAsset': $scope.ProjectPlaningMachineTypeListForSave },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        getProjectPlanningById(response.data.ProjectPlanningId);
                        $scope.machineTypeMasterFormModalCloseListPopUp();
                        $scope.budgetMasterSelectedList = [];
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            }
            else if ($scope.Action == 'Update') {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: $scope.projectPlanning,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -1) {
                            $scope.projectPlannings[$scope.index] = $scope.projectPlanning;
                            $scope.projectPlannings = $filter('orderBy')($scope.projectPlannings, 'Sequence');
                        }
                    }
                }, function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                });
            }
        }
    }
    $scope.ProjectPlaningMaterialListForAssetAndOtherSave = [];
    function setProjectPlanningMaterialMasterForSave() {
        $scope.ProjectPlaningMaterialListForAssetAndOtherSave = [];
        angular.forEach($scope.ProjectPlaningMaterialListForSave, function (item) {
            $scope.ProjectPlaningMaterialListForAssetAndOtherSave.push(item);
        });
        angular.forEach($scope.ProjectPlaningMaterialOtherListForSave, function (item) {
            $scope.ProjectPlaningMaterialListForAssetAndOtherSave.push(item);
        });
    }
    $scope.MaterialMasterSave = function () {
        angular.copy($scope.projectPlanningNew, $scope.projectPlanning);
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.materialMasterForm.$valid) {
            setProjectPlanningMaterialMasterForSave();
            if ($scope.Action == 'Save') {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: { 'projectPlanning': $scope.projectPlanning, 'ProjectPlanningDetail': $scope.budgetMasterSelectedList.length > 0 ? $scope.budgetMasterSelectedList : $scope.ProjectPlanningDetailSavedList, 'projectPlanningMaterial': $scope.ProjectPlaningMaterialListForAssetAndOtherSave },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        getProjectPlanningById(response.data.ProjectPlanningId);
                        $scope.materialMasterFormModalCloseListPopUp();
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            }
            else if ($scope.Action == 'Update') {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: $scope.projectPlanning,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -1) {
                            $scope.projectPlannings[$scope.index] = $scope.projectPlanning;
                            $scope.projectPlannings = $filter('orderBy')($scope.projectPlannings, 'Sequence');
                        }
                    }
                }, function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                });
            }
        }
    }
    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.projectPlanningNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.projectPlanningNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.projectPlannings.splice($scope.index, 1);
                    baseService.paginationRemove();
                    ClearFields();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    }
    $scope.editProjectPlanningDetail = function (index, data) {
        $scope.projectPlanningEditIndex = index;
        $scope.ProjectPlanningDetail.ProjectPlanningCategoryId = data.ProjectPlanningCategoryId;
        $scope.ProjectPlanningDetail.ProjectPlanningSubCategoryId = data.ProjectPlanningSubCategoryId;
        $scope.ProjectPlanningDetailSavedEditTempList = Object.assign({}, data);
        angular.element(document.querySelector('#ProjectPlanningDetailEditPopUp')).modal('show');
    }
    $scope.Clear = function () {
        ClearFields();
        return true;
    }
    $scope.BudgetMasterEditSave = function () {
        $scope.ProjectPlanningDetailSavedEditTempList.ProjectPlanningCategoryId = $scope.ProjectPlanningDetail.ProjectPlanningCategoryId;
        $scope.ProjectPlanningDetailSavedEditTempList.ProjectPlanningSubCategoryId = $scope.ProjectPlanningDetail.ProjectPlanningSubCategoryId;
        $scope.ProjectPlanningDetailSavedList[$scope.projectPlanningEditIndex] = Object.assign({}, $scope.ProjectPlanningDetailSavedEditTempList);
        $scope.MaterialMasterSave();
        angular.element(document.querySelector('#ProjectPlanningDetailEditPopUp')).modal('hide');
    }
    function ClearFields() {
        $scope.Action = 'Save';
        $scope.projectPlanning = {};
        $scope.projectPlanningNew = {};
        $scope.projectPlanningNew.Id = null
        $scope.machineTypeMasterList = [];
        $scope.materialMasterList = [];
        $scope.ProjectPlanningDetailSavedList = [];
        $scope.budgetMasterSelectedList = [];
        $scope.ProjectPlaningMachineTypeListForSave = [];
        $scope.projectPlanningNew.Active = true;

        cboService.getCboTransactionCurrencyByCompany('', function (result) {
            $scope.CurrencyList = [];
            $scope.CurrencyList = result;
            $scope.projectPlanningNew.CurrencyId = $filter("filter")($scope.CurrencyList, { IsBaseCurrency: 1 })[0].CurrencyId;
            $scope.GetCurrencyExchangeRateList();
        });
    }
    //#endregion
    // #region Tab
    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };

    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };
}
