'use strict';
productDefinitionController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function productDefinitionController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "Product";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.path = 'Products/productdefinition/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    $scope.GetFabricList = function () {
        baseService.init($scope.getListUrl, null, null, null, 'UserName', 'UserName');
        $scope.getData = function (pageno) {
            var tempParam = [];
            tempParam.push($scope.searchModel.UserName);
            tempParam.push($scope.searchModel.BaseUoM);
            tempParam.push($scope.searchModel.ProductMasterName);
            tempParam.push($scope.searchModel.SeasonName);
            tempParam.push($scope.searchModel.OurStyleName);
            $rootScope.parameters.tempParam = JSON.stringify(tempParam);
            baseService.pagination(pageno)
                .then(function (result) {
                    $scope.buyerStyles = result.Rows;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        $scope.getData();
        angular.element(document.querySelector('#fabricId')).modal('show');
    };
    $scope.model = {
        Id: null
        , MaterialMasterId: null
        , Code: null
        , ShortName: null
        , StandardName: null
        , UserName: null
        , BaseUoM: null
        , ProductMasterId: null
        , SeasonId: null
        , OurStyleId: null
        , CostAndManufacture: null
        , CostAndManufactureCurrencyId: null
        , DaysToReachTheTarget: null
        , FirstdayOutPut: null
        , IsFixed: 'Fixed'
        , IncrementValue: null
        , ProcessId:null
        , Active: true
    };
    $scope.modelNew = Object.assign({}, $scope.model);
    $scope.searchModel = {
        Code: null
        , ShortName: null
        , StandardName: null
        , UserName: null
        , BaseUoM: null
        , ProductMasterName: null
        , SeasonName: null
        , OurStyleName: null
    };
    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.model = $scope.buyerStyles[$scope.index];
        $scope.modelNew = Object.assign({}, $scope.model);
        getArticleList();
        getEfficencyList();
        $scope.Action = 'Update';
        angular.element(document.querySelector('#fabricId')).modal('hide');
    };
    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.form.$valid) {
            if (parseInt($scope.modelNew.FirstdayOutPut) > parseInt($scope.modelNew.TotalQty))
                return ShowResult('First day output can not greater than total quantity.', 'failure');
            $scope.model = Object.assign({}, $scope.modelNew);
            if ($scope.Action === "Save") {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: {
                        'product': $scope.model
                        , 'articleList': $scope.articleList
                        , 'efficencyList': $scope.efficencyList
                    },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        ClearFields();
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                };
            }
            else if ($scope.Action === "Update") {
                $http({
                    method: 'POST',
                    url: $scope.updateUrl,
                    data: {
                        'product': $scope.model
                        , 'articleList': $scope.articleList
                        , 'efficencyList': $scope.efficencyList
                    },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');

                        ClearFields();
                    }
                }, function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                });
            }
        }
    };
    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.modelNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.modelNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    ClearFields(response.data.Sequence);
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };
    $scope.Clear = function () {
        ClearFields();
        return true;
    };
    function ClearFields(seq) {
        $scope.Action = "Save";
        $scope.model = {};
        $scope.modelNew = { Active: true, IsFixed: 'Fixed' };
        $scope.prdNameList = [];
        $scope.articleList = [];
        $scope.efficencyList = [];
    }

    // #region MM
    $scope.materialModel = {
        materialTypeId: null
        , materialCategoryId: null
        , materialSubCategoryId: null
        , materialGroupMasterId: null
    };
    $scope.popUpList = [];
    $scope.valueData = '';
    $scope.popUpParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'Code',
        searchBy: "UserName",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.popUp = function () {
        $scope.popUpDataList = [];
        $scope.popUpUrl = $scope.path + 'GetMaterialMasterList';
        baseService.setCurrentPage('popUpDataList');
        $scope.getPopUpData = function (pageno) {
            var paramList = [];
            paramList.push($scope.materialModel.materialTypeId);
            paramList.push($scope.materialModel.materialGroupMasterId);
            paramList.push($scope.materialModel.materialCategoryId);
            paramList.push($scope.materialModel.materialSubCategoryId);
            $scope.popUpParameters.paramList = JSON.stringify(paramList);
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
        $scope.getPopUpData();
    };
    $scope.selectDoubleClick = function (data) {
        $scope.articleList = [];
        $scope.prdNameList = [];
        $scope.modelNew = data;
        $scope.modelNew.IsFixed = 'Fixed';
        if (!baseService.isUndefinedOrNull($scope.modelNew.ProductMasterId))
            $scope.prdNameList = getUniqueColumn($scope.productMasters);
        getArticleList();
        getEfficencyList();
        $scope.modelNew.OurStyleId = null;
        $scope.closePopUp();
    };
    $scope.closePopUp = function () {
        $scope.valueData = '';
        $scope.materialModel = {};
        angular.element(document.querySelector('#popUpId')).modal('hide');
    };
    // #endregion MM

    // #region ProductMaster
    $scope.productMasterList = [];
    $scope.productMasters = [];
    $http({
        method: 'GET',
        url: 'Products/productmaster/getcbo/'
    }).then(function successCallback(response) {
        $scope.productMasterList = response.data.Rows;
    });
    $scope.changeOnProductMaster = function (id) {
        if (!baseService.isUndefinedOrNull(id)) {
            $http({
                method: 'GET',
                url: 'Products/productmaster/ProductMasterWithDetails?productMasterId=' + id
            }).then(function successCallback(response) {
                if (response.data.length > 0) {
                    $scope.productMasters = response.data;
                    $scope.prdNameList = getUniqueColumn($scope.productMasters);
                }
                else {
                    productMasterCombinationData(id);
                }
            });
        }
    };
    function productMasterCombinationData(id) {
        if (!baseService.isUndefinedOrNull(id)) {
            $http({
                method: 'GET',
                url: 'Products/productmaster/ProductMasterComminationData?productMasterId=' + id
            }).then(function successCallback(response) {
                $scope.prdNameList = response.data;
            });
        }
    }

    function getUniqueColumn(fromtotable) {
        var _obj = {
            ProductCategoryName: null,
            ProductSubCategoryName: null,
            ProductName: null
        };
        var _step = [];
        //var _stepList = [];
        for (var i_cycle = 0; i_cycle < fromtotable.length; i_cycle++) {
            var hasduplicate = true;
            var _newObj = fromtotable[i_cycle];
            _obj.ProductCategoryName = _newObj.ProductCategoryName;
            _obj.ProductSubCategoryName = _newObj.ProductSubCategoryName;
            _obj.ProductName = _newObj.ProductName;
            hasduplicate = hasColumn(_step, _obj.ProductCategoryName, _obj.ProductSubCategoryName, _obj.ProductName);
            if (hasduplicate === false) {
                _step.push(_obj);
            }
        }
        return _step;
    }
    function hasColumn(list, v1, v2, v3) {
        for (var i = 0; i < list.length; i++) {
            var ob = list[i];
            if (ob.ProductCategoryName === v1) {
                if (ob.ProductSubCategoryName === v2) {
                    if (ob.ProductName === v3) {
                        return true;
                    }
                }
            }
        }
        return false;
    }
    // #endregion

    // #region Ddl

    $scope.styleList = [];
    getOurStyle();
    function getOurStyle() {
        $scope.styleList = [];
        $http({
            method: 'GET',
            url: 'Materials/ourstyle/getcbo/'
        }).then(function successCallback(response) {
            $scope.styleList = response.data;
        });
    }
    $scope.seasonsList = [];
    $http({
        method: 'GET',
        url: 'OrderManagements/seasons/getcbo/'
    }).then(function successCallback(response) {
        $scope.seasonsList = response.data;
    });
    $scope.currencyList = [];
    cboService.getCompanyGroupCurrencyCbo(null, function (response) {
        $scope.currencyList = response;
    });

    // #endregion

    $scope.getMaterialTypeUrl = 'Materials/materialtype/getcbobymaterialmaster';
    $scope.getMaterialCategoryUrl = 'Materials/materialcategory/getcbobymaterialmaster';
    $scope.getMaterialSubCategoryUrl = 'Materials/materialsubcategory/getcbobymaterialmaster';
    $scope.getMaterialGroupMasterUrl = 'Materials/materialgroupmaster/getcbobymaterialmaster';
    // #region DDL
    function getMaterialTypeList() {
        $http.get($scope.getMaterialTypeUrl)
            .then(function (response) {
                $scope.materialTypeList = response.data;
            });
    }
    function getMaterialCategoryList() {
        $http.get($scope.getMaterialCategoryUrl)
            .then(function (response) {
                $scope.materialCategoryList = response.data;
            });
    }
    function getMaterialSubCategoryList() {
        $http.get($scope.getMaterialSubCategoryUrl)
            .then(function (response) {
                $scope.materialSubCategoryList = response.data;
            });
    }
    function getMaterialGroupMasterList() {
        $http.get($scope.getMaterialGroupMasterUrl)
            .then(function (response) {
                $scope.materialGroupMasterList = response.data;
            });
    }
    // #endregion DDL

    // #region Article
    $scope.articleList = [];
    function getArticleList() {
        $scope.articleList = [];
        $http.get('Materials/MaterialMasterArticle/GetArticlListByMaterialMaster?materialMasterId=' + $scope.modelNew.MaterialMasterId)
            .then(function (response) {
                $scope.articleList = response.data;
            });
    }
    // #endregion Article

    // IncrementType, FirstDayOutPut, MinRequiredTargetHourly, StandardTime
    $scope.GetDays = function (incType, incValue, firstDayOutPut, tQty) {
        try {
            var iv = parseInt(CalculateIncrementValue(incType, incValue, firstDayOutPut));//daily iv
            var _days = 1;
            var _cumi_output = parseInt(firstDayOutPut);
            while (_cumi_output < parseInt(tQty)) {
                _days++;
                _cumi_output += iv;
                if (iv <= 0) {
                    _days = 0;
                    break;
                }
            }
            $scope.modelNew.DaysToReachTheTarget = _days;
        } catch (e) {
            throw e;
        }
    };
    function CalculateIncrementValue(isfixed, incValue, firstDayOutPut) {
        try {
            var iv = CheckNullReturnZero(incValue);
            if (isfixed === "Fixed")
                return iv;
            else
                return iv * CheckNullReturnZero(firstDayOutPut) / 100;
        } catch (e) {
            throw e;
        }
    }
    function CheckNullReturnZero(val) {
        if (baseService.isUndefinedOrNull(val)) return 0;
        else return parseInt(val);
    }

    // #region Article Process
    $rootScope.tempList = [];
    $scope.sprocessList = [];
    $scope.processEntryPopUp = function (index) {
        $scope.sprocessList = [];
        $scope.processIndex = index;
        $scope.materialMasterArticleId = $scope.articleList[$scope.processIndex].Id;
        $scope.sprocessList = $scope.articleList[$scope.processIndex].MaterialMasterArticleProcess;
        angular.element(document.querySelector('#processEntryPopUp')).modal('show');
    }
    $scope.processParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'UserName',
        searchBy: "UserName",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.searchProcessByList = [
        {
            'name': 'Sequence',
            'value': 'Sequence'
        },
        {
            'name': 'Code',
            'value': 'Code'
        },
        {
            'name': 'ShortName',
            'value': 'ShortName'
        },
        {
            'name': 'Standard Name',
            'value': 'StandardName'
        },
        {
            'name': 'User Define Name',
            'value': 'UserName'
        }
    ];
    $scope.processPopUp = function () {
        baseService.setCurrentPage('processList');
        $scope.getProcessData = function (pageno) {
            $scope.getProcessUrl = 'Processes/CompanyProcess/GetCompanyProductionProcessList?processIds=' + baseService.getColumnValueList($scope.sprocessList, 'ProcessId');
            baseService.paginationBase($scope.getProcessUrl, pageno, $scope.processParameters)
                .then(function (result) {
                    $scope.processList = result.Rows;
                    $scope.processParameters.total_count = result.Total;
                    for (var t = 0; t < baseService.arrayLength($scope.processList); t++) {
                        $scope.processList[t].Flag = baseService.valueCheckInList($rootScope.tempList, 'Id', $scope.processList[t].Id);
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#processPopUp')).modal('show');
        $scope.getProcessData();
    };
    $scope.addProcess = function () {
        if (baseService.arrayLength($rootScope.tempList) > 0) {
            angular.forEach($rootScope.tempList, function (a) {
                if (!baseService.valueCheckInList($scope.sprocessList, 'ProcessId', a.Id)) {
                    $scope.sprocessList.push({
                        Id: null
                        , ProcessId: a.Id
                        , Sequence: a.Sequence
                        , Code: a.Code
                        , ShortName: a.ShortName
                        , StandardName: a.StandardName
                        , UserName: a.UserName
                        , ProductDefinitionId: $scope.modelNew.Id
                        , MaterialMasterArticleId: $scope.materialMasterArticleId
                    });
                }
            });
        }
        $scope.closeProcess();
    };
    $scope.removeProcessRowModal = function (ob, index) {
        try {
            $scope.processId = ob.Id
            $scope.message_confirmation = 'Are you sure want to permanent delete [' + ob.UserName + '].';
            angular.element(document.querySelector('#confirmProcessPopUp')).modal('show');
            $scope.popUpIndex = index;
        }
        catch (e) {
            ShowResult(e, 'Error');
        }
    }
    $scope.removeProcessRow = function () {
        if (baseService.isUndefinedOrNull($scope.processId))
            $scope.sprocessList.splice($scope.popUpIndex, 1);
        else {
            $http({
                method: 'POST',
                url: $scope.path + 'deletearticleprocess?id=' + $scope.processId,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) return ShowResult(response.data.Message, 'failure');
                else {
                    ShowResult(response.data.Message, 'success', 'processEntryPopUp');
                    $scope.sprocessList.splice($scope.popUpIndex, 1);
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure', 'processEntryPopUp');
            }
        }
        $scope.popUpIndex = -1;
        $scope.processId = null;
    };
    $scope.closeProcess = function () {
        $rootScope.tempList = [];
        angular.element(document.querySelector('#processPopUp')).modal('hide');
    };
    $scope.closeEntryProcessPopUp = function () {
        $scope.articleList[$scope.processIndex].MaterialMasterArticleProcess = [];
        $scope.articleList[$scope.processIndex].MaterialMasterArticleProcess = $scope.sprocessList;
        $scope.sprocessList = [];
        $scope.materialMasterArticleId = null;
        $scope.processIndex = -1;
        angular.element(document.querySelector('#processEntryPopUp')).modal('hide');
    };

    // #endregion Article Process

    $scope.processList = [];
    cboService.getProductionProcessCbo(function (response) {
        $scope.processList = response;
    });

    function getEfficencyList() {
        $http.get($scope.path + 'getefficencylist?masterId=' + $scope.modelNew.Id)
            .then(function (response) {
                $scope.efficencyList = response.data;
            });
    }
    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };
}