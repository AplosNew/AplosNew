'use strict';
entityConfigController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function entityConfigController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $scope.tableShow = false;
    $scope.Action = 'Save';
    $scope.entityConfigList = [];
    $scope.path = 'Setups/entityConfig/';
    $scope.getUrl = $scope.path + 'get';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';

    $scope.entityConfig = {
        Id: null,
        CompanyId: null,
        StandardName: null,
        UserName: null,
        EntityId: null,
        Value: null,
        Applicable: false,
        IsChangeable: false,
        IsProductionEntity: false,
        NoOfWorkStation: 0,
        FixedCost: 0,
        VariableCost: 0,
        MachineCostPerHour: 0,
        MinFixedCost: 0,
        MaxFixedCost: 0,
        GeneralWorkingHourPerDay: 0,
        CurrencyId: null,
        ConsumptionBooking: null,
        ConsumptionProcessId:null
    };

    $scope.companyList = [];
    cboService.getCompanyGroupCompanyCbo(null, function (result) {
        $scope.companyList = result;
    });

    $scope.currencyList = [];
    $scope.GetCurrency = function () {
        cboService.getCboTransactionCurrencyByCompany($scope.entityConfig.CompanyId, function (result) {
            $scope.currencyList = [];
            $scope.currencyList = result;
            //$scope.entityConfig.CurrencyId = $filter("filter")($scope.currencyList, { IsBaseCurrency: 1 })[0].CurrencyId;
        });
    }

    $scope.PlantList = [];
    $scope.getPlant = function () {
        cboService.getCboPlantByCompany($scope.entityConfig.CompanyId, function (result) {
            $scope.PlantList = result;
        });
    };



    $scope.EntityList = [];
    $scope.getEntityWithChange = function () {
        $scope.EntityList = [];
        cboService.getCboEntityByPlant(null, $scope.entityConfig.CompanyId, $scope.entityConfig.PlantId, function (result) {
            $scope.EntityList = result;
        });
        //cboService.getCboProductionEntityByCompany(null, $scope.entityConfig.CompanyId, function (result) {
        //    $scope.EntityList = result;
        //});
    };

    $scope.processList = [];
    $scope.loadProcessList = function (entityid) {
        cboService.GetEntityProcessCbo(entityid, function (result) {
            $scope.processList = result;
        });
    };


    $scope.ConsumtionBookingList = [];
    cboService.getEnumCbo("enum/GetCboConsumtionBooking", function (result) {
        $scope.ConsumtionBookingList = result;
    });


    $scope.eList = [];
    function getList() {
        $http({
            method: 'GET',
            url: 'setups/entityconfig/getentityconfigparameterlist'
        }).then(function successCallback(response) {
            $scope.eList = response.data;
            for (var i = 0; i < $scope.eList.length; i++) {
                $scope.eList[i].StandardName = $scope.eList[i].Text;
                $scope.eList[i].UserName = null;
                $scope.eList[i].Value = null;
                $scope.eList[i].IsChangeable = false;
                $scope.eList[i].IsProductionEntity = false;
                $scope.eList[i].NoOfWorkStation = 0;
                $scope.eList[i].FixedCost = 0;
                $scope.eList[i].VariableCost = 0;
                $scope.eList[i].MachineCostPerHour = 0;
                $scope.eList[i].MinFixedCost = 0;
                $scope.eList[i].GeneralWorkingHourPerDay = 0;
                $scope.eList[i].CurrencyId = null;
                $scope.eList[i].ConsumptionBooking = null;
                $scope.eList[i].ConsumptionProcessId = null;
            }
        });
    }
    getList();

    $scope.List = [];
    $scope.getSaveList = function () {
        getList();
        $http({
            method: 'GET',
            url: 'setups/entityconfig/GetList',
            params: { 'entityId': $scope.entityConfig.EntityId, 'isProductionEntity': $scope.entityConfig.IsProductionEntity }
        }).then(function successCallback(response) {
            if (baseService.arrayLength(response.data) > 0) {
                $scope.List = response.data;
                for (var t = 0; t < baseService.arrayLength($scope.eList); t++) {
                    for (var i = 0; i < baseService.arrayLength($scope.List); i++) {
                        if (!baseService.isUndefinedOrNull($scope.List[i].Id) && $scope.List[i].StandardName === $scope.eList[t].StandardName) {
                            $scope.eList[t].Id = $scope.List[i].Id;
                            if (!baseService.isUndefinedOrNull($scope.List[i].UserName)) {
                                if ($scope.List[i].Applicable === 1) {
                                    $scope.eList[t].Applicable = true;
                                } else {
                                    $scope.eList[t].Applicable = false;
                                }
                            }
                            $scope.eList[t].UserName = $scope.List[i].UserName;
                            $scope.eList[t].Value = $scope.List[i].Value;
                            $scope.eList[t].IsChangeable = $scope.List[i].IsChangeable;
                            $scope.eList[t].EntityId = $scope.List[i].EntityId;
                            $scope.entityConfig.IsProductionEntity = $scope.List[i].IsProductionEntity;
                            $scope.entityConfig.NoOfWorkStation = $scope.List[i].NoOfWorkStation;
                            $scope.entityConfig.FixedCost = $scope.List[i].FixedCost;
                            $scope.entityConfig.VariableCost = $scope.List[i].VariableCost;

                            $scope.entityConfig.MachineCostPerHour = $scope.List[i].MachineCostPerHour;
                            $scope.entityConfig.MinFixedCost = $scope.List[i].MinFixedCost;
                            $scope.entityConfig.GeneralWorkingHourPerDay = $scope.List[i].GeneralWorkingHourPerDay;
                            $scope.entityConfig.CurrencyId = $scope.List[i].CurrencyId;
                            $scope.entityConfig.ConsumptionBooking = $scope.List[i].ConsumptionBooking;
                            $scope.entityConfig.ConsumptionProcessId = $scope.List[i].ConsumptionProcessId;
                        }
                    }
                }
            }
            else {
                $scope.entityConfig.IsProductionEntity = false;
                $scope.entityConfig.NoOfWorkStation = 0;
                $scope.entityConfig.FixedCost = 0;
                $scope.entityConfig.VariableCost = 0;
                $scope.entityConfig.MachineCostPerHour = 0;
                $scope.entityConfig.MinFixedCost = 0;
                $scope.entityConfig.GeneralWorkingHourPerDay = 0;
                $scope.entityConfig.CurrencyId = null;
                $scope.entityConfig.ConsumptionBooking = null;
                $scope.entityConfig.ConsumptionProcessId = null;
            }
        });
    }

    $scope.ChangeValues = function () {
        if ($scope.entityConfig.IsProductionEntity === false) {
            $scope.entityConfig.MachineCostPerHour = 0;
            $scope.entityConfig.MinFixedCost = 0;
            $scope.entityConfig.GeneralWorkingHourPerDay = 0;
            $scope.entityConfig.ConsumptionBooking = null;
            $scope.entityConfig.ConsumptionProcessId = null;
            $scope.entityConfig.CurrencyId = null;
        }
    }


    $scope.saveDataList = [];
    $scope.Save = function () {
        $scope.saveDataList = [];
        try {
            if (baseService.isUndefinedOrNull($scope.entityConfig.EntityId)) {
                throw "Select Entity.";
            }
            for (var i = 0; i < $scope.eList.length; i++) {
                $scope.eList[i].EntityId = $scope.entityConfig.EntityId;
                $scope.eList[i].IsProductionEntity = $scope.entityConfig.IsProductionEntity;
                $scope.eList[i].NoOfWorkStation = $scope.entityConfig.NoOfWorkStation;
                $scope.eList[i].FixedCost = $scope.entityConfig.FixedCost;
                $scope.eList[i].VariableCost = $scope.entityConfig.VariableCost;

                $scope.eList[i].MachineCostPerHour = $scope.entityConfig.MachineCostPerHour;
                $scope.eList[i].MinFixedCost = $scope.entityConfig.MinFixedCost;
                $scope.eList[i].GeneralWorkingHourPerDay = $scope.entityConfig.GeneralWorkingHourPerDay;
                $scope.eList[i].CurrencyId = $scope.entityConfig.CurrencyId;
                $scope.eList[i].ConsumptionBooking = $scope.entityConfig.ConsumptionBooking;
                $scope.eList[i].ConsumptionProcessId = $scope.entityConfig.ConsumptionProcessId;
                if ($scope.eList[i].Applicable === true) {
                    if (baseService.isUndefinedOrNull($scope.eList[i].UserName)) {
                        throw "User Define Name is required.";
                    }
                    if (baseService.isUndefinedOrNull($scope.eList[i].Value)) {
                        throw "Value is required.";
                    }
                }
            }

            for (var i = 0; i < $scope.eList.length; i++) {
                if ($scope.eList[i].IsProductionEntity) {
                    $scope.saveDataList.push($scope.eList[i]);
                }
            }

            $scope.$broadcast('show-errors-check-validity');
            if ($scope.entityConfigForm.$valid) {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: { 'entities': $scope.saveDataList, 'entityId': $scope.entityConfig.EntityId },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.getSaveList();

                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                };
            }
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.entityConfigNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.entityConfigNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.entityConfigList.splice($scope.index, 1);
                    baseService.paginationRemove();
                    ClearFields();
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

    function ClearFields() {
        $scope.Action = "Save";
        $scope.entityConfig = {};
        $scope.entityConfigNew = {};
    }
}