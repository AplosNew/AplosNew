'use strict';
EmployeeTimeOutApplicableController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'toaster', 'cboService', '$controller', '$window'];
function EmployeeTimeOutApplicableController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, toaster, cboService, $controller, $window) {
    $scope.title = "Employee Time Out Applicable";
    $scope.Action = 'Save';
    $scope.ModelList = [];
    $scope.path = 'Productions/EmployeeTimeOutApplicable/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl);
    //$scope.searchBy = "UserName"; $scope.search = "";
    //$scope.searchByList = [{ value: 'Id', name: "Id" }, { value: 'Code', name: "Code" }, { value: 'ShortName', name: "Short Name" }, { value: 'StandardName', name: "Standard Name" }, { value: 'UserName', name: "User Name" }, { value: 'Description', name: "Description" }, { value: 'Remarks', name: "Remarks" }];


    $scope.companyList = [];
    $scope.entityList = [];
    $scope.plantList = [];
    $scope.processList = [];
    $scope.companyId = [];
    $scope.plantId = [];

    $scope.getData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetList",
            //data: { column: $scope.searchBy, value: $scope.search },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ModelList = response.data;
            ClearFields();
            
        });


        $http({
            method: 'GET',
            url: $scope.path + "GetCompany",
            //data: { column: $scope.searchBy, value: $scope.search },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.companyList = response.data;
        });

    }
    $scope.getData();

    $scope.getPlant = function()
    {
        $http({
            method: 'GET',
            url: $scope.path + "GetPlant",
            data: { cmp: $scope.companyId},
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.plantList = response.data;
        });
    }

    $scope.getEntity = function () {
        $http({
            method: 'GET',
            url: $scope.path + "GetEntity",
            data: { plant: $scope.plantId },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.entityList = response.data;
        });
    }

    $scope.getProcesses = function () {
        $http({
            method: 'GET',
            url: $scope.path + "GetProcess",
            data: { entity: $scope.ModelNew.EntityId },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.processList = response.data;
        });
    }

    $scope.ModelTemp = {
        Id: null,
        EntityId: null,
        ProcessId: null,
        isApplicable: false
    };
    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);

    

    $scope.Get = function (args) {

        $scope.ModelNew = Object.assign({}, args.data);
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
                    ClearFields();
                    $scope.getData();
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
        $scope.Action = 'Save';
        $scope.ModelNew = Object.assign({}, $scope.ModelTemp);
    }
    
}