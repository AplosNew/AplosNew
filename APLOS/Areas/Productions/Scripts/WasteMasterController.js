'use strict';
WasteMasterController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function WasteMasterController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Waste Master';
    $scope.Action = 'Save';
    $scope.ModelList = [];
    $scope.path = 'Productions/WasteMaster/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'create';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl);
    $scope.searchBy = null; $scope.search = null;
    //$scope.searchBy = "UserName"; $scope.search = "";
    //$scope.searchByList = [{ value: 'Id', name: "Id" }, { value: 'Code', name: "Code" }, { value: 'ShortName', name: "Short Name" }, { value: 'StandardName', name: "Standard Name" }, { value: 'UserName', name: "User Name" }, { value: 'Description', name: "Description" }, { value: 'Remarks', name: "Remarks" }];

    $scope.CompanyId = null;
    $scope.CompanyList = [];
    $scope.PlantId = null;
    $scope.PlantList = [];
    $scope.EntityList = [];
    $scope.UOMList = [];
    $scope.BudgetList = [];
    $scope.Budget = null;

    $scope.getData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetList",
            data: { column: $scope.searchBy, value: $scope.search },
            dataType: 'JSON'
        }).then(function successCallback(response) {          
            $scope.ModelList = response.data;
            ClearFields(response.data.Sequence);
            $scope.GetSequence();
        });

        $http({
            method: 'GET',
            url: $scope.path + "getCompany",
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.CompanyList = response.data;
        });

        $http({
            method: 'GET',
            url: $scope.path + "getUOM",
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.UOMList = response.data;
        });
    }
    $scope.getData();

    $scope.getPlant = function () {
        $http({
            method: 'POST',
            url: $scope.path + "getPlants",
            data: { 'cmpId': $scope.CompanyId },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.PlantList = response.data;
        });
    }

    $scope.getEntity = function () {
        $http({
            method: 'POST',
            url: $scope.path + "getEntity",
            data: {'PlantId' : $scope.PlantId},
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.EntityList = response.data;
        });
    }

    $scope.getBudgets = function () {
        $http({
            method: 'POST',
            url: $scope.path + "getBudget",
            data: { 'EId': $scope.ModelNew.EntityId },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.BudgetList = response.data;
        });
    }

    $scope.selectBudget = function () {
        if (angular.isUndefinedOrNull($scope.ModelNew.EntityId)) {
            ShowResult("Please First Select the Entity!!" , 'failure');
            throw ("Invalid!!");
        }
        angular.element(document.querySelector('#Budget')).modal('show');
    }

    $scope.doubleBudget = function (e) {
        $scope.Budget = e.data.Code;
        $scope.ModelNew.BudgetId = e.data.Id;
        angular.element(document.querySelector('#Budget')).modal('hide');
    }

    $scope.ModelTemp = {
        Id: null,
        Sequence: 0,
        EntityId: null,
        UOMId: null,
        BudgetId: null,
        Category: null,
        SubCategory: null,
        ItemName: null,
        StandardRate: null,
        Code: null,
        Remarks:null,
    };
    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);

    $scope.GetSequence = function () {
        cboService.getSequence($scope.getSeqUrl, function (data) {
            $scope.ModelTemp.Sequence = data;
            $scope.ModelNew.Sequence = data;
        });
    };
    $scope.GetSequence();

    $scope.Get = function (args) {

        var AllData = [];
        $http({
            method: 'POST',
            url: $scope.path + "Get",
            data: {'Id':args.data.Id},
            dataType: 'JSON'
        }).then(function successCallback(resp) {
            AllData = resp.data;
        });

        $scope.ModelNew = Object.assign({}, AllData);
        $scope.CompanyId = AllData.CompanyId;
        $scope.PlantId = AllData.PlantId;
        $scope.getPlant();
        $scope.getEntity();
        $scope.getBudgets();
        $scope.Budget = AllData.BudgetCode;

        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');

        if (angular.isUndefinedOrNull($scope.ModelNew.BudgetId)) {
            ShowResult('No Budget Code Selected!!' , 'failure');
            throw ("Invalid");
        }

        if (angular.isUndefinedOrNull($scope.ModelNew.UOMId)) {
            ShowResult('No UOM Selected!!', 'failure');
            throw ("Invalid");
        }

        if ($scope.ModelNewForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: { 'datas': $scope.ModelNew },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    ClearFields(response.data.Sequence);
                    $scope.getData();

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
        $scope.CompanyId = null;
        $scope.PlantId = null;
        $scope.Budget = null;
        $scope.ModelNew = Object.assign({}, $scope.ModelTemp);
        $scope.ModelNew.Sequence = seq;
    }
}