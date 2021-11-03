'use strict';
recipeMaterialController.$inject = ['$controller', '$window', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter'];
function recipeMaterialController($controller, $window, cboService, commonMessage, $scope, $rootScope, baseService, $http, $filter) {
    $rootScope.title = "Recipe Material";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.modelList = [];
    $scope.path = 'Productions/ButtonRecipeMaterial/';
    $scope.getListUrl = $scope.path + 'getrecipelist?masterId=';
    $scope.saveUrl = $scope.path + 'create';
    $scope.deleteUrl = $scope.path + 'delete/';

    $controller('baseMaterialAndArticleController', { $scope: $scope, $http: $http });
    $scope.materialType = ['RawMaterial'];

    $scope.model = {
        Id: null
        , CompanyGroupId: null
        , CompanyId: null
        , EntityId: null
        , ButtonRecipeMasterId: null
    };
    $scope.modelNew = Object.assign({}, $scope.model);

    $scope.getData = function () {
        //$http.get($scope.getListUrl + $scope.modelNew.ButtonRecipeMasterId)
        $http.get('Productions/RecipeMaterial/getrecipelist?masterId=' + $scope.modelNew.ButtonRecipeMasterId)
            .then(function (response) {
                $scope.modelList = response.data;
                console.log('data', $scope.modelList);
            });
    };

    // #region ddl

    $scope.entityList = [];
    //cboService.getCboProductionEntityByPlant(null, null, $window.plantId, function (result) {
    //    $scope.entityList = result;
    //});

    $scope.getAllEntities = function () {
        $http({
            method: 'POST',
            url: "OrderManagements/productionOrderSchedulingParametersType1/GetEntity"
        }).then(function successCallback(response) {
            $scope.entityList = response.data;
        });
    }
    $scope.getAllEntities();

    $scope.recipeMasterList = [];
    $scope.getRecipeCboList = function () {
        $scope.recipeMasterList = [];
        if (!baseService.isUndefinedOrNull($scope.modelNew.EntityId)) {
            cboService.getrecipeCbo($scope.modelNew.EntityId, function (result) {
                $scope.recipeMasterList = result;
            });
        }
    };
    // #endregion ddl

    $scope.Save = function () {
        $http({
            method: 'POST'
            , url: $scope.saveUrl
            , data: $scope.modelList
            , dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.ClearFields();
            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        };
    };

    $scope.Clear = function () {
        $scope.ClearFields();
        $scope.modelNew.EntityId = null;
    };

    $scope.ClearFields = function () {
        $scope.model = {};
        $scope.modelNew = { EntityId: $scope.modelNew.EntityId };
        $scope.modelList = [];
    };

    //#region Material

    $scope.materialMasterbyTypeList = [];
    $scope.searchMaterialMasterList = [
        {
            'Text': 'Material Type',
            'Value': 'MaterialTypeName'
        },
        {
            'Text': 'Material Group',
            'Value': 'MaterialGroupMasterName'
        },
        {
            'Text': 'Code',
            'Value': 'Code'
        },
        {
            'Text': 'Material',
            'Value': 'UserName'
        },
        {
            'Text': 'Product',
            'Value': 'ProductMasterName'
        },
        {
            'Text': 'Id',
            'Value': 'Id'
        }
    ];
    $scope.getMaterialMasterbyTypePopUp = function () {
        $scope.$broadcast('show-errors-check-validity');
        if (!$scope.modelForm.$valid) return;
        $scope.mmPopUpParameters = {
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
        $rootScope.tempList = [];
        angular.forEach($scope.modelList, function (a) {
            $rootScope.tempList.push({
                Id: a.MaterialMasterId
                , MaterialTypeName: a.MaterialTypeName
                , MaterialGroupMasterName: a.MaterialGroupMasterName
                , ProductMasterName: a.ProductMasterName
                , Code: a.Code
                , UserName: a.MaterialMasterName
            });
        });
        //$scope.materialTitle = 'Recipe';
        $scope.materialTitle = ['ProductDefinition'];
        //$scope.materialTitle = 'Material';
        CloseShowResult();
        CloseModalShowResult();
        $scope.searchList = [];
        //$scope.popUpUrl = 'Materials/MaterialMaster/GetMaterialListByMaterialType?materialType=' + JSON.stringify($scope.materialType);
        $scope.popUpUrl = 'Materials/MaterialMaster/MaterialSearchByBusinessProcess?type=' + $scope.materialTitle;
        baseService.setCurrentPage('materialMasterbyTypeList');
        $scope.getMaterialMasterbyTypeData = function (pageno) {
            baseService.paginationBase($scope.popUpUrl, pageno, $scope.mmPopUpParameters)
                .then(function (result) {
                    $scope.materialMasterbyTypeList = result.Rows;
                    $scope.mmPopUpParameters.total_count = result.Total;

                    for (var t = 0; t < baseService.arrayLength($scope.materialMasterbyTypeList); t++) {
                        $scope.materialMasterbyTypeList[t].Flag = baseService.valueCheckInList($rootScope.tempList, 'Id', $scope.materialMasterbyTypeList[t].Id);
                    }
                    angular.element(document.querySelector('#materialMasterbyTypePopup')).modal('show');
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        $scope.getMaterialMasterbyTypeData();
    };
    $scope.closeMaterialMasterbyTypePopUp = function () {
        CloseModalShowResult('materialMasterbyTypePopup');
        angular.element(document.querySelector('#materialMasterbyTypePopup')).modal('hide');
    };

    $scope.selectMaterial = function () {
        if (baseService.arrayLength($rootScope.tempList) > 0) {
            angular.forEach($rootScope.tempList, function (a) {
                if (!baseService.valueCheckInList($scope.modelList, 'MaterialMasterId', a.Id)) {
                    $scope.modelList.push({
                        Id: null
                        , MaterialMasterId: a.Id
                        , ButtonRecipeMasterId: $scope.modelNew.ButtonRecipeMasterId
                        , MaterialTypeName: a.MaterialTypeName
                        , MaterialGroupMasterName: a.MaterialGroupMasterName
                        , ProductMasterName: a.ProductMasterName
                        , Code: a.Code
                        , UserName: a.UserName
                    });
                }
            });
        }
        else
            $scope.modelList = [];
        angular.forEach($scope.modelList, function (a) {
            if (!baseService.valueCheckInList($rootScope.tempList, 'Id', a.MaterialMasterId))
                $scope.modelList.splice(a, 1);
        });
        $scope.closeMaterialMasterbyTypePopUp();
    };

    $scope.removeRowModal = function (name, index, listName, tempId, listId) {
        try {
            $scope.popUpIndex = index;
            $scope.listName = listName;
            $scope.tempId = tempId;
            $scope.listId = listId;
            $scope.message_confirmation = "Are you sure want to permanent delete [" + name + "] ";
            angular.element(document.querySelector('#confirmProcessPopUp')).modal('show');
        }
        catch (e) {
            ShowResult(e, 'Error');
        }
    };
    $scope.removeRow = function () {
        if (!baseService.isUndefinedOrNull($scope[$scope.listName][$scope.popUpIndex].Id)) {
            $http({
                method: 'POST'
                , url: 'Productions/RecipeMaterial/delete/' + $scope[$scope.listName][$scope.popUpIndex].Id
                //, url: $scope.deleteUrl + $scope[$scope.listName][$scope.popUpIndex].Id
                , dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true)
                    ShowResult(response.data.Message, "failure");
                else {
                    ShowResult(response.data.Message, "success");
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, "failure");
            });
        }
        for (var t = 0; t < baseService.arrayLength($rootScope.tempList); t++) {
            if ($rootScope.tempList[t][$scope.tempId] === $scope[$scope.listName][$scope.popUpIndex][$scope.listId])
                $rootScope.tempList.splice(t, 1);
        }
        $scope[$scope.listName].splice($scope.popUpIndex, 1);
        $scope.popUpIndex = -1;
        angular.element(document.querySelector('#confirmProcessPopUp')).modal('hide');
    };

    //#endregion





    // #region Article
    $scope.operationNew = Object.assign({}, $scope.operation);

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

    $scope.tempMaterialMasterId;
    $scope.articlePopUp = function (materialMasterId) {
        $scope.tempMaterialMasterId = materialMasterId;
        $scope.excluedList = ['SkillName', 'MachineAllowance'];
        $scope.articleDataList = [];
        //$scope.articleUrl = $scope.path + 'GetArticleListByMaterialMaster?materialMasterId=' + materialMasterId;

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
    };

    function checkDuplicate(list, materialMasterId, articleId, recipeGlobalMasterId) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].MaterialMasterId === materialMasterId && list[i].ArticleId === articleId && list[i].RecipeGlobalMasterId === recipeGlobalMasterId) {
                throw 'Selected combination already taken.';
            }
        }
    }

    $scope.selectArticle = function (data1) {
        try {

            $scope.operationNew.RecipeGlobalMasterId = $scope.modelNew.ButtonRecipeMasterId;
            $scope.operationNew.MaterialMasterId = $scope.tempMaterialMasterId;
            $scope.operationNew.ArticleId = data1.Id;

            checkDuplicate($scope.modelList, $scope.operationNew.MaterialMasterId, $scope.operationNew.ArticleId, $scope.operationNew.RecipeGlobalMasterId);

            $http({
                method: 'POST',
                url: 'Productions/RecipeMaterial/CreateRecipeMaterial/',
                data: $scope.operationNew,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure', 'articleId');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.getData();
                }
            }), function errorCallback(response) {
                ShowResult(response.data.Message, 'failure', 'articleId');
            }


            $scope.getData();
            $scope.closeArticle();
            $scope.closeMaterial();

        } catch (e) {
            ShowResult(e, 'failure','articleId')
        }
    };
    $scope.closeArticle = function () {
        angular.element(document.querySelector('#articleId')).modal('hide');
    };

    $scope.closeMaterial = function () {
        angular.element(document.querySelector('#materialMasterbyTypePopup')).modal('hide');
    };

    // #endregion Article
}