'use strict';
machineBudgetController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter', 'cboService', '$window'];
function machineBudgetController(commonMessage, $scope, $rootScope, baseService, $http, $filter, cboService, $window) {
    $rootScope.title = "Machine Budget";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.models = [];
    $scope.budgetList = [];
    $scope.processList = [];
    $scope.skillProcessList = [];
    $rootScope.tempList = [];
    $scope.processIndex = -1;
    $scope.path = 'materials/MachineBudget/';
    //$scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };

    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.model = {
        Id: null
        , MaterialMasterId: null
        , Code: null
        , ShortName: null
        , StandardName: null
        , AssetMaster: null
        , UserName: null
        , BaseUom: null
        , SkillId: null
    };
    $scope.modelNew = Object.assign({}, $scope.model);

    $http.get('Machines/StitchCode/GetCbo')
        .then(function (response) {
            $scope.stitchCodeList = response.data;
        });

    $scope.searchModel = {
        Code: null
        , ShortName: null
        , StandardName: null
        , UserName: null
        , BaseUom: null
    };

    function getDetails() {
        $http.get($scope.path + 'getdetaillist?materialMasterId=' + $scope.modelNew.MaterialMasterId)
            .then(function (response) {
                $scope.skillProcessList = response.data;
                getSkillCbo();
            });
    }

    function getArticleList() {
        $http({
            method: 'GET'
            , url: $scope.path + 'GetArticleList?materialMasterId=' + $scope.modelNew.MaterialMasterId
            , contentType: "application/json; charset=utf-8"
        }).then(function successCallback(response) {
            $scope.modelList = response.data;
        });
    }

    // #region add Skill

    $scope.addProcess = function () {
        $scope.modelNew.SkillId = null;
        if (baseService.arrayLength($rootScope.tempList) > 0) {
            angular.forEach($rootScope.tempList, function (a) {
                if (!baseService.valueCheckInList($scope.skillProcessList, 'ProcessId', a.Id)) {
                    $scope.skillProcessList.push({
                        Id: null
                        , MachineId: $scope.modelNew.Id
                        , ProcessId: a.Id
                        , Sequence: a.Sequence
                        , Code: a.Code
                        , ShortName: a.ShortName
                        , StandardName: a.StandardName
                        , UserName: a.UserName
                        //, SkillId: null
                        , Active: a.Active
                    });
                }
            });
        }
        else
            $scope.skillProcessList = [];
        angular.forEach($scope.skillProcessList, function (a) {
            if (!baseService.valueCheckInList($rootScope.tempList, 'Id', a.ProcessId))
                $scope.skillProcessList.splice(a, 1);
        });
        getSkillCbo();
        $scope.CloseProcessPopUp();
    };

    function getSkillCbo() {
        $http({
            method: 'GET'
            , url: 'skills/skill/GetCboByProcess?processIds=' + baseService.getColumnValueList($scope.skillProcessList, 'ProcessId')
        }).then(function successCallback(response) {
            $scope.skillList = response.data;

            if (!baseService.isUndefinedOrNull($scope.modelNew.SkillId) && baseService.arrayLength($scope.skillList) > 0) {
                for (var i = 0; i < $scope.skillList.length; i++) {
                    if ($scope.modelNew.SkillId == $scope.skillList[i].Value) {
                        $scope.modelNew.SkillId = $scope.skillList[i].Value;
                        $scope.Skill = $scope.skillList[i].Text;
                    }
                }
            }
        });
    }

    // #endregion add Skill

    // #region MM
    $scope.Skill = null;
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
        $scope.popUpUrl = $scope.path + 'getmaterialmasterlist';
        baseService.setCurrentPage('popUpDataList');
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
        $scope.modelNew = data;
        $scope.skillProcessList = [];
        $rootScope.tempList = [];
        getDetails();
        getArticleList();
        $scope.closePopUp();
        $scope.Skill = $("#SkillId option:selected").text();
    };
    $scope.closePopUp = function () {
        $scope.valueData = '';
        $scope.materialModel = {};
        angular.element(document.querySelector('#popUpId')).modal('hide');
    };

    // #endregion MM

    //#region Budget
    $scope.Action = 'Save';
    $scope.fileNew = { Id: null, CompanyId: null, ArticleId: $scope.ArticleId, PlantId: null, EntityId: null, ProductionMachineQty: 0, SampleMachineQty: 0, TrainingMachineQty: 0, RentMachineQty: 0, OtherMachineQty: 0 }
    $scope.machineBudget = Object.assign({}, $scope.fileNew);
    $scope.MachineBudgetLevel = null;
    $scope.GetMachineBudgetLevel = function (plantId) {
        $http({
            method: 'GET',
            url: 'Materials/MachineBudget/GetMachineBudgetLevel?plantId=' + plantId
        }).then(function successCallback(response) {
            if (baseService.arrayLength(response.data) > 0) {
                $scope.MachineBudgetLevel = response.data[0].MachineBudgetLevel;
            } else {
                $scope.MachineBudgetLevel = null;
            }
        })
    };


    $scope.GetMachineBudgetByArticle = function (ArticleId) {
        $http({
            method: 'GET',
            url: 'Materials/MachineBudget/GetMachineBudgetByArticle?ArticleId=' + ArticleId
        }).then(function successCallback(response) {
            $scope.budgetList = response.data;
        })
    };

    $scope.LoadGenericData = function () {
        $scope.companyList = [];
        cboService.getCompanyGroupCompanyCbo(null, function (result) {
            $scope.companyList = result;
            if (baseService.arrayLength($scope.companyList) > 0) {
                for (var i = 0; i < $scope.companyList.length; i++) {
                    if ($scope.companyList[i].Value == $window.companyId) {
                        $scope.machineBudget.CompanyId = $scope.companyList[i].Value;
                    }
                }
                cboService.getCboPlantByCompany($scope.machineBudget.CompanyId, function (result) {
                    $scope.PlantList = result;
                });
            }
        });

        $scope.PlantList = [];
        $scope.getPlant = function () {
            cboService.getCboPlantByCompany($scope.machineBudget.CompanyId, function (result) {
                $scope.PlantList = result;
            });
        };

        $scope.EntityList = [];
        $scope.getEntityWithChange = function () {
            $scope.EntityList = [];
            cboService.getCboProductionEntitiesByPlant($scope.machineBudget.PlantId, function (result) {
                $scope.EntityList = result;

            });
        };
    }

    $scope.fileNew = { Id: null, CompanyId: null, ArticleId: $scope.ArticleId, PlantId: null, EntityId: null, ProductionMachineQty: 0, SampleMachineQty: 0, TrainingMachineQty: 0, RentMachineQty: 0, OtherMachineQty: 0 }
    $scope.machineBudget = Object.assign({}, $scope.fileNew);

    $scope.GetBudgetPopUp = function (obj) {
        $scope.ArticleId = obj.Id;
        $scope.fileNew = { Id: null, CompanyId: null, ArticleId: $scope.ArticleId, PlantId: null, EntityId: null, ProductionMachineQty: 0, SampleMachineQty: 0, TrainingMachineQty: 0, RentMachineQty: 0, OtherMachineQty: 0 }
        $scope.machineBudget = Object.assign({}, $scope.fileNew);
        $scope.GetMachineBudgetByArticle($scope.ArticleId);
        $scope.LoadGenericData();
        angular.element(document.querySelector('#budgetpopup')).modal('show');
    };

    $scope.GetMachine = function (obj) {
        $scope.Action = 'Update';
        $scope.fileNew = { Id: null, CompanyId: null, ArticleId: $scope.ArticleId, PlantId: null, EntityId: null, ProductionMachineQty: 0, SampleMachineQty: 0, TrainingMachineQty: 0, RentMachineQty: 0, OtherMachineQty: 0 }
        $scope.fileNew = Object.assign({}, obj);
        $scope.machineBudget = Object.assign({}, $scope.fileNew);
        $scope.GetMachineBudgetLevel($scope.machineBudget.PlantId);
        cboService.getCompanyGroupCompanyCbo(null, function (result) {
            $scope.companyList = result;

            cboService.getCboPlantByCompany($scope.machineBudget.CompanyId, function (result) {
                $scope.PlantList = result;
                if (baseService.arrayLength($scope.PlantList) > 0) {
                    for (var i = 0; i < $scope.PlantList.length; i++) {
                        if ($scope.machineBudget.PlantId == $scope.PlantList[i].Value) {
                            $scope.machineBudget.PlantId = $scope.PlantList[i].Value;
                        }
                    }
                    cboService.getCboProductionEntitiesByPlant($scope.machineBudget.PlantId, function (result) {
                        $scope.EntityList = result;

                        if (baseService.arrayLength($scope.EntityList) > 0) {
                            for (var i = 0; i < $scope.EntityList.length; i++) {
                                if ($scope.machineBudget.EntityId == $scope.EntityList[i].Id) {
                                    $scope.machineBudget.EntityId = $scope.EntityList[i].Id;
                                }
                            }
                        }
                    });

                }

            });
        });
        if (baseService.isUndefinedOrNull($scope.machineBudget.EntityId)) {
            $scope.machineBudget.EntityId = "ALL";
        }
    };

    $scope.ClearMachineBudget = function () {
        $scope.fileNew = { Id: null, CompanyId: null, ArticleId: $scope.ArticleId, PlantId: null, EntityId: null, ProductionMachineQty: 0, SampleMachineQty: 0, TrainingMachineQty: 0, RentMachineQty: 0, OtherMachineQty: 0 }
       // $scope.fileNew = Object.assign({}, obj);
        $scope.machineBudget = Object.assign({}, $scope.fileNew);
        $scope.MachineBudgetLevel = null;
    }

    $scope.SaveBudget = function () {
        $scope.$broadcast('show-errors-check-validity');
        try {
            if ($scope.MachineBudgetLevel==='Entity') {
                if (baseService.isUndefinedOrNull($scope.machineBudget.EntityId)) {
                    throw "Entity is required.";
                }
            }
            if (baseService.isUndefinedOrNull($scope.machineBudget.ProductionMachineQty) || $scope.machineBudget.ProductionMachineQty < 0  || isNaN($scope.machineBudget.ProductionMachineQty)) {
                throw "Production Machine Qty should greater than 0.";
            }
            if (baseService.isUndefinedOrNull($scope.machineBudget.SampleMachineQty) || $scope.machineBudget.SampleMachineQty < 0  || isNaN($scope.machineBudget.SampleMachineQty)) {
                throw "Sample Machine Qty should greater than 0.";
            }
            if (baseService.isUndefinedOrNull($scope.machineBudget.TrainingMachineQty) || $scope.machineBudget.TrainingMachineQty < 0  || isNaN($scope.machineBudget.TrainingMachineQty)) {
                throw "Training Machine Qty should greater than 0.";
            }
            if (baseService.isUndefinedOrNull($scope.machineBudget.RentMachineQty) || $scope.machineBudget.RentMachineQty < 0  || isNaN($scope.machineBudget.RentMachineQty)) {
                throw "Rent Machine Qty should greater than 0.";
            }
            if (baseService.isUndefinedOrNull($scope.machineBudget.OtherMachineQty) || $scope.machineBudget.OtherMachineQty < 0  || isNaN($scope.machineBudget.OtherMachineQty)) {
                throw "Other Machine Qty should greater than 0.";
            }
            if ($scope.machineBudgetForm.$valid) {
                if ($scope.Action == 'Save') {
                    $http({
                        method: 'POST',
                        url: 'Materials/MachineBudget/Create',
                        data: { 'data': $scope.machineBudget },
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure', 'budgetpopup');
                        }
                        else {
                            ShowResult(response.data.Message, 'success', 'budgetpopup');
                            $scope.GetMachineBudgetByArticle($scope.ArticleId);
                            $scope.LoadGenericData();
                            $scope.Action = 'Save';
                            $scope.ClearMachineBudget();
                        }
                    }), function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure', 'budgetpopup');
                    }
                }
                else {
                    $http({
                        method: 'POST',
                        url: 'Materials/MachineBudget/Edit',
                        data: { 'data': $scope.machineBudget },
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure', 'budgetpopup');
                        }
                        else {
                            ShowResult(response.data.Message, 'success', 'budgetpopup');

                            $scope.GetMachineBudgetByArticle($scope.ArticleId);
                            $scope.LoadGenericData();
                            $scope.Action = 'Save';
                            $scope.ClearMachineBudget();
                        }
                    }), function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure', 'budgetpopup');
                    }
                }
            }
        } catch (e) {
            ShowResult(e, 'failure', 'budgetpopup');
        }
    };

    $scope.message_confirmation = null;
    $scope.removeLineItem = function (data) {
        $scope.machineBudget = data;
        if (!baseService.isUndefinedOrNull($scope.machineBudget.Id))
            $scope.message_confirmation = 'Are you sure want to delete permanently';
        angular.element(document.querySelector('#confirmProcessPopUp')).modal('show');
    }

    $scope.DeleteBudget = function () {
        $http({
            method: 'POST',
            url: 'Materials/MachineBudget/Delete?id=' + $scope.machineBudget.Id
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.LoadGenericData();
                $scope.GetMachineBudgetByArticle($scope.ArticleId);
                $scope.ClearMachineBudget();
            }
        }, function () {
            ShowResult(commonMessage.NetworkError, 'failure');
        }).finally(function () {
        });

    };

    $scope.GetMachineBudgetIndexReport = function () {
        var reportFormat = "Excel";
        try {
            var url = 'Materials/MachineBudget/GetMachineBudgetIndexReport?reportFormat=' + reportFormat;

            $rootScope.report(url);
        } catch (e) {

        }
    };

    //#endregion

}