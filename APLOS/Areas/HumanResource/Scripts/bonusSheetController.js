'use strict';
bonusSheetController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$http', '$filter', 'cboService', '$window'];
function bonusSheetController(commonMessage, $scope, $rootScope, baseService, $routeParams, $http, $filter, cboService, $window) {
    $rootScope.title = 'Bonus Sheet';
    $scope.Action = 'Download';
    $scope.index = -1;
    $scope.modelList = [];
    $scope.path = 'humanresource/bonussheet/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';

    baseService.init($scope.getListUrl, null, null, null, 'ShiftName', 'ShiftName');
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.modelList = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
   // $scope.getData();

    $scope.searchByList = [
        {
            'name': 'Code',
            'value': 'Code'
        },
        {
            'name': 'Shift Name',
            'value': 'ShiftName'
        },
        {
            'name': 'InTime',
            'value': 'InTime'
        },
        {
            'name': 'OutTime',
            'value': 'OutTime'
        },
        {
            'name': 'IsNight',
            'value': 'IsNight'
        }
    ];

    $scope.model = {
        Id: null
        , PlantId: $window.plantId
        , CompanyGroupId: $window.companyGroupId
        , Code: null
        , ShiftName: null
        , InTime: null
        , OutTime: null
        , IsNight: null
    };

    $scope.PeraModel = {
        PayRollGroupId: null
        , BonusPointId: null
    };

    $scope.modelNew = Object.assign({}, $scope.model);

    $scope.plantList = [];
    $scope.getPlantList = function () {
        cboService.getCboPlantByCompany($scope.modelNew.CompanyId, function (result) {
            $scope.plantList = result;
        });
    };


    $scope.payGroupList = [];
    cboService.getPayGroupCbo(function (result) {
        $scope.payGroupList = result;
    })


   
    $scope.Clear = function () {
        ClearFields();
        return true;
    };


    function ClearFields() {
        $scope.Action = 'Save';
        $scope.model = { PlantId: $scope.modelNew.PlantId, CompanyGroupId: $scope.modelNew.CompanyGroupId };
        $scope.modelNew = { PlantId: $scope.modelNew.PlantId, CompanyGroupId: $scope.modelNew.CompanyGroupId };
    }


    $scope.manualValidationAddRemove = function () {
        if (baseService.isUndefinedOrNull($scope.modelNew.InTime))
            return manualValidation('div_inTime', true, 'In time is required.');
        else manualValidation('div_inTime', false);
        if (baseService.isUndefinedOrNull($scope.modelNew.OutTime))
            return manualValidation('div_inTime', true, 'Out time is required.');
        else manualValidation('div_inTime', false);
        if (!$scope.validateTimeEntry($scope.modelNew.InTime))
            return manualValidation('div_inTime', true, "Invalid Intime " + $scope.modelNew.InTime + " </b> (Check Input Range *00:00 - 23:59* and Format *HH : mm*)");
        else manualValidation('div_inTime', false);
        if (!$scope.validateTimeEntry($scope.modelNew.OutTime))
            return manualValidation('div_outTime', true, "Invalid Outtime " + $scope.modelNew.OutTime + " </b> (Check Input Range *00:00 - 23:59* and Format *HH : mm*)");
        else manualValidation('div_outTime', false);
    };

    $scope.BonusPoint = {
        Id: null
        , Name: null
    };

    $scope.BonusPoint = [];
    $scope.GetBonusPoint = function () {
        $http({
            method: "GET",
            dataType: 'JSON',
            //url: $scope.getSearchListUrl,
            url: 'humanresource/BonusSheet/GetBonusPoint/'
          
        }).then(function successCallback(response) {
            $scope.BonusPoint = response.data.data;

        });
    };

    $scope.GetBonusPoint();

    $scope.BonusSheet = function () {
        $http({
            method: "POST",
            dataType: 'JSON',
            data: { 'PayRollGroupId': $scope.PeraModel.PayRollGroupId, 'BonusPointId': $scope.PeraModel.BonusPointId},
            url: 'humanresource/BonusSheet/GetBonusData'

        }).then(function successCallback(response) {   
        });
    };
}