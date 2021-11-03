'use strict';
materialGroupArticleController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function materialGroupArticleController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "Article";
    $scope.index = -1;
    $scope.path = 'Materials/MaterialGroupMaster/';
    $scope.getListUrl = $scope.path + 'getarticlelist';
    $scope.getCriteriaeUrl = $scope.path + 'getCriterialist';
    $scope.saveUrl = $scope.path + 'create';

    // #region Article
    function getArticleData() {
        baseService.init($scope.getListUrl, null, null, null, 'MaterialGroupMasterName, StandardName', 'StandardName');
        $rootScope.parameters.mGroupId = $scope.articleNew.MaterialGroupMasterId;
        $scope.getData = function (pageno) {
            baseService.pagination(pageno)
                .then(function (result) {
                    $scope.articleList = result.Rows;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        $scope.getData();
    }
    $scope.searchByList = [
        {
            'name': 'Code',
            'value': 'Code'
        },
        {
            'name': 'Short Name',
            'value': 'ShortName'
        },
        {
            'name': 'Standard Name',
            'value': 'StandardName'
        },
        {
            'name': 'Material Group Master',
            'value': 'MaterialGroupMasterName'
        }
    ];
    $scope.article = {
        Id: null
        , MaterialGroupMasterId: null
        , MaterialGroupMasterName: null
        , Code: null
        , ShortName: null
        , StandardName: null
    };
    $scope.articleNew = Object.assign({}, $scope.article);

    $scope.Get = function (index) {
        $scope.index = index;
        $scope.article = $scope.articleList[$scope.index];
        $scope.articleNew = Object.assign({}, $scope.article);
        getAttribute();
        getMaterialProductProcessGroupList();
        if (!$rootScope.isCollapsed) $rootScope.toggle();
    };

    // #endregion

    // #region Material GroupMaster
    $scope.popUpList = [];
    $scope.valueData = '';
    $scope.popUpTitle = 'Material Group (Mst)';
    $scope.popUpParameters = {
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
    $scope.matarialGroupPopUp = function () {
        $scope.popUpUrl = 'Materials/materialgroupmaster/getlistbymaterialtype?materialTypeId=' + '';
        baseService.setCurrentPage('dataList');
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
        $scope.getPopUpData();
    };
    $scope.selectDoubleClick = function (data) {
        $scope.Clear();
        $scope.articleNew.MaterialGroupMasterId = data.Id;
        $scope.articleNew.MaterialGroupMasterName = data.UserName;
        getArticleData();
        getAttribute();
        getMaterialProductProcessGroupList();
        $scope.closePopUp();
    };
    $scope.selectSingleClick = function (data) {
        $scope.valueData = data;
    };
    $scope.selectByButton = function () {
        if (baseService.isUndefinedOrNull($scope.valueData))
            return ShowResult('Please at first select row', 'failure', 'popUpId');
        $scope.selectDoubleClick($scope.valueData);
        $scope.closePopUp();
    };
    $scope.closePopUp = function () {
        $scope.valueData = '';
        angular.element(document.querySelector('#popUpId')).modal('hide');
    };
    // #endregion

    // #region Criteria
    function getMaterialProductProcessGroupList() {
        $http({
            method: 'GET',
            url: $scope.path + 'GetMaterialPrdGroupList?mgMasterId=' + $scope.articleNew.MaterialGroupMasterId + '&articleId=' + $scope.articleNew.Id
        }).then(function successCallback(response) {
            $scope.processGroupList = response.data;
        });
    }
    $rootScope.tempList = [];
    $scope.processCriteriaList = [];
    $scope.criteriaEntryPopUp = function (index) {
        $scope.processCriteriaList = [];
        $scope.criteriaIndex = index;
        $scope.mgPrdProcessGroupId = $scope.processGroupList[$scope.criteriaIndex].Id;
        $scope.mgPrdProcessGroup = $scope.processGroupList[$scope.criteriaIndex].ProdProcessGroupName;
        if (baseService.arrayLength($scope.processGroupList[$scope.criteriaIndex].CriteriaList) === 0)
            getProcessCriteriaList();
        else
            $scope.processCriteriaList = $scope.processGroupList[$scope.criteriaIndex].CriteriaList;
        angular.element(document.querySelector('#criteriaEntryPopUp')).modal('show');
    };
    function getProcessCriteriaList() {
        $http.get($scope.path + 'getProcessCriteriaList?id=' + $scope.mgPrdProcessGroupId)
            .then(function (response) {
                $scope.processCriteriaList = [];
                $scope.processCriteriaList = response.data;
            });
    }
    $scope.criteriaParameters = {
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
    $scope.criteriaPopUp = function () {
        baseService.setCurrentPage('criteriaList');
        $scope.getCriteriaData = function (pageno) {
            $scope.getProcessUrl = $scope.path + 'getcriteriaList?ids=' + baseService.getColumnValueList($scope.processCriteriaList, 'ProcessCriteriaId');
            baseService.paginationBase($scope.getProcessUrl, pageno, $scope.criteriaParameters)
                .then(function (result) {
                    $scope.criteriaList = result.Rows;
                    $scope.criteriaParameters.total_count = result.Total;
                    for (var t = 0; t < baseService.arrayLength($scope.criteriaList); t++) {
                        $scope.criteriaList[t].Flag = baseService.valueCheckInList($rootScope.tempList, 'ProcessCriteriaId', $scope.criteriaList[t].ProcessCriteriaId);
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#criteriaPopUp')).modal('show');
        $scope.getCriteriaData();
    };
    $scope.addProcess = function () {
        if (baseService.arrayLength($rootScope.tempList) > 0) {
            angular.forEach($rootScope.tempList, function (a) {
                if (!baseService.valueCheckInList($scope.processCriteriaList, 'ProcessCriteriaId', a.ProcessCriteriaId)) {
                    $scope.processCriteriaList.push({
                        Id: null
                        , MaterialGroupArticleId: $scope.articleNew.Id
                        , MaterialGroupArticlePrdProcessGroupId: $scope.mgPrdProcessGroupId
                        , ProcessCriteriaId: a.ProcessCriteriaId
                        , Sequence: a.Sequence
                        , Code: a.Code
                        , ShortName: a.ShortName
                        , StandardName: a.StandardName
                        , UserName: a.UserName
                        , Wastage: 0
                        , Rate: 0
                    });
                }
            });
        }
        $scope.closeProcess();
    };
    $scope.removeProcessRowModal = function (ob, index) {
        try {
            $scope.processId = ob.Id;
            $scope.message_confirmation = 'Are you sure want to permanent delete [' + ob.UserName + '].';
            angular.element(document.querySelector('#confirmProcessPopUp')).modal('show');
            $scope.criteriaIndex = index;
        }
        catch (e) {
            ShowResult(e, 'Error');
        }
    };
    $scope.removeProcessRow = function () {
        if (baseService.isUndefinedOrNull($scope.processId))
            $scope.processCriteriaList.splice($scope.criteriaIndex, 1);
        else {
            $http({
                method: 'POST',
                url: $scope.path + 'deleteprocesscriteria?id=' + $scope.processId,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) return ShowResult(response.data.Message, 'failure');
                else {
                    ShowResult(response.data.Message, 'success', 'criteriaEntryPopUp');
                    $scope.processCriteriaList.splice($scope.criteriaIndex, 1);
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure', 'criteriaEntryPopUp');
            };
        }
        $scope.processId = null;
    };
    $scope.closeProcess = function () {
        $rootScope.tempList = [];
        angular.element(document.querySelector('#criteriaPopUp')).modal('hide');
    };
    $scope.closeEntryProcessPopUp = function () {
        $scope.processGroupList[$scope.criteriaIndex].CriteriaList = [];
        $scope.processGroupList[$scope.criteriaIndex].CriteriaList = $scope.processCriteriaList;
        $scope.processCriteriaList = [];
        $scope.mgPrdProcessGroupId = null;
        $scope.criteriaIndex = -1;
        angular.element(document.querySelector('#criteriaEntryPopUp')).modal('hide');
    };
    // #endregion

    // #region Attribute
    function getAttribute() {
        $scope.attributeList = [];
        $http({
            method: 'GET',
            url: $scope.path + 'getAttributeList?groupMasterId=' + $scope.articleNew.MaterialGroupMasterId + '&articleId=' + $scope.articleNew.Id
        }).then(function successCallback(response) {
            $scope.attributeList = response.data;
            if (baseService.arrayLength(response.data) === 0)
                return ShowResult('This material group has no attribute', 'failure');
            for (var i = 0; i < $scope.attributeList.length; i++) {
                $scope.searchFreeField = $scope.attributeList[i].ValueFreeText !== null ? true : false;
                $scope.attributeList[i].FlagDisable = $scope.IsFreeFieldOrNot($scope.attributeList[i].IsFreeField);
            }
        });
    }
    $scope.idNullByFreeText = function (id, index) {
        if ($scope.attributeList[index].MaterialAttributeId === id)
            $scope.attributeList[index].MaterialAttributeValueId = null;
    };
    $scope.IsFreeFieldOrNot = function (IsFreeField) {
        if (IsFreeField) {
            if ($scope.searchFreeField)
                return true;//disabled true
            else
                return false;//disabled false
        }
        else
            return true;//disabled true
    };
    $scope.IsMandatoryButNull = function (isMandatory, ValueFreeText) {
        if (isMandatory) {
            if (baseService.isUndefinedOrNull(ValueFreeText)) return true;
            else return false;
        }
        else return false;
    };
    // #endregion

    // #region value
    $scope.valueindex = -1;
    $scope.searchvalueList = [
        {
            'name': 'Code',
            'value': 'Code'
        },
        {
            'name': 'ShortName',
            'value': 'ShortName'
        },
        {
            'name': 'StanderName',
            'value': 'StanderName'
        },
        {
            'name': 'UserName',
            'value': 'UserName'
        }
    ];
    $scope.valueParameters = {
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
    $scope.valuePoUp = function (data, index) {
        $scope.materialAttributeName = data.MaterialAttributeName;
        $scope.materialAttributeValueUrl = $scope.path + 'GetAttributeValueList';
        baseService.setCurrentPage('valueList');
        $scope.getValueData = function (pageno) {
            $scope.valueParameters.attributeId = data.MaterialAttributeId;
            baseService.paginationBase($scope.materialAttributeValueUrl, pageno, $scope.valueParameters)
                .then(function (result) {
                    $scope.valueList = result.Rows;
                    $scope.valueParameters.total_count = result.Total;
                    $scope.valueindex = index;
                    $scope.searchFreeField = true;
                    angular.element(document.querySelector('#attributeValuePopUp')).modal('show');
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        $scope.getValueData();
    };
    $scope.getAttrValue = function (data) {
        $scope.attributeList[$scope.valueindex].MaterialAttributeValueId = data.MaterialAttributeValueId;
        $scope.attributeList[$scope.valueindex].ValueFreeText = data.UserName;
        $scope.attributeList[$scope.valueindex].FlagDisable = $scope.searchFreeField;
        $scope.valueindex = -1;
        angular.element(document.querySelector('#attributeValuePopUp')).modal('hide');
    };
    $scope.materialAttributeValueClear = function (index) {
        $scope.attributeList[index].MaterialAttributeValueId = null;
        $scope.attributeList[index].ValueFreeText = null;
        $scope.searchFreeField = false;
        var isFree = $scope.attributeList[index].IsFreeField;
        $scope.attributeList[index].FlagDisable = $scope.IsFreeFieldOrNot(isFree);
    };
    $scope.closeValuePopUp = function () {
        angular.element(document.querySelector('#attributeValuePopUp')).modal('hide');
        CloseModalShowResult('attributeValuePopUp');
    };
    // #endregion value

    $scope.Save = function () {
        for (var i = 0; i < $scope.attributeList.length; i++) {
            var _invalid = $scope.IsMandatoryButNull($scope.attributeList[i].IsMandatory, $scope.attributeList[i].ValueFreeText);
            if (_invalid) return ShowResult('');
        }
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.modelForm.$valid) {
            angular.copy($scope.articleNew, $scope.article);
            $http({
                method: 'POST',
                url: $scope.path + 'CreateOrEditArticle',
                data: {
                    article: $scope.article
                    , valueList: $scope.attributeList
                    , processGroupList: $scope.processGroupList
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true)
                    ShowResult(response.data.Message, "failure");
                else {
                    ShowResult(response.data.Message, "success");
                    getArticleData();
                    ClearFields();
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, "failure");
            });
        }
    };
    $scope.Clear = function () {
        $scope.articleNew.MaterialGroupMasterId = null
        $scope.articleNew.MaterialGroupMasterName = null
        $scope.articleList = [];
        ClearFields();
        return true;
    };
    function ClearFields() {
        $scope.article = {};
        $scope.articleNew = {
            MaterialGroupMasterId: $scope.articleNew.MaterialGroupMasterId
            , MaterialGroupMasterName: $scope.articleNew.MaterialGroupMasterName
        };
        $scope.attributeList = [];
        $scope.processGroupList = [];
    }
    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };
}