'use strict';
AttendanceSourceConfigController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function AttendanceSourceConfigController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Attendance Source Configuration';
    $scope.Action = 'Save';
    $scope.ModelList = [];
    $scope.path = 'humanresource/AttendanceSourceConfig/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';
    baseService.init($scope.getListUrl);
    $scope.searchBy = "Company"; $scope.search = "";
    $scope.searchByList = [{ value: 'Company', name: "Company" },{ value: 'Plant', name: "Plant" }];


    $scope.Company = null;
    $scope.CompanyList = [];
    $scope.getCompany = function () {
        $http({
            method: 'GET',
            url: 'humanresource/RosterPattern/getCompany'
        }).then(function success(response) {
            $scope.CompanyList = response.data;
        })
    }

    $scope.getCompany();

    $scope.PlantList = [];
    $scope.getPlants = function () {
        $http({
            method: 'GET',
            url: 'HumanResource/RosterPattern/getPlants',
            params: { 'cmp': $scope.ModelNew.CompanyId }
        }).then(function success(response) {
            $scope.PlantList = response.data;
        })
    }

    $scope.getData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetList",
            data: { column: $scope.searchBy, value: $scope.search },
            dataType: 'JSON'
        }).then(function successCallback(response) {          
            $scope.ModelList = response.data;
            ClearFields();           
        });
    }
    $scope.getData();

    $scope.ModelTemp = {
        Id: null,
        CompanyName: null,
        CompanyId: null,
        PlantId: null,
        ManualOTAllowed: false,
        ManualInAllowed: false,
        ManualOutAllowed: false       
    };
    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);
     

    $scope.Get = function (args) {
       
        $scope.ModelNew = Object.assign({}, args.data);
        $scope.getPlants();
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
                    ClearFields();
                    $scope.getData();

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        }
    };
        
    $scope.Clear = function () {
        ClearFields();
        return true;
    };

    function ClearFields() {
        $scope.Action = 'Save';
        $scope.ModelNew = Object.assign({}, $scope.ModelTemp);       
    }
}