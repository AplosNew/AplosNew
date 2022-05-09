'use strict';
ResidenceStatusLocationController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function ResidenceStatusLocationController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Residence Status Loacation';
    $scope.Action = 'Save';
    $scope.ModelList = [];
    $scope.path = 'HumanResource/ResidenceStatusLocation/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'Save';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl);

    // Tab Change
    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.tab2 = 1;
    $scope.setTab2 = function (newTab) {
        $scope.tab2 = newTab;
    };
    $scope.isSet2 = function (tabNum) {
        return $scope.tab2 === tabNum;
    };

    // All List Variables are here for dropdown
    $scope.PlantList = [];
    $scope.LocationList = [];
    $scope.ResidenceGroupIdList = [];
    $scope.ResidenceCategoryList = [];
    $scope.ResidenceSubCategoryList = [];
    $scope.BlockList = [];

    $scope.getPlant = function () {
        $http({
            method: 'POST',
            data: {            
                    'ResidenceGroupId': $scope.selectedData.ResidenceGroupId,
                },
            url: $scope.path + 'getPlant',
        }).then(function success(response) {
            $scope.PlantList = response.data;
        });
    }
   // $scope.getPlant();

    $scope.getLocation = function () {
        $http({
            method: 'POST',
            data: {
                'PlantId': $scope.selectedData.PlantId,
                'ResidenceGroupId': $scope.selectedData.ResidenceGroupId,
            },
            url: $scope.path + 'getLocation',
        }).then(function successCallback(response) {
            $scope.LocationList = [];
            $scope.LocationList = response.data;
        });
    }
    //$scope.getLocation();

    $scope.ResidenceGId = null;
    $scope.getResidenceGroup = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'getResidenceGroup',
        }).then(function success(response) {
            $scope.ResidenceGroupIdList = response.data;
            
        });
    }
    $scope.getResidenceGroup();

    $scope.EmpServiceTypeList = [];
    $scope.getServiceType = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'getServiceType',
            data: {
                'PlantId': $scope.selectedData.PlantId,
                'ResidenceGroupId': $scope.selectedData.ResidenceGroupId,
            },
        }).then(function success(response) {
            $scope.EmpServiceTypeList = response.data;
        });
    }
    //$scope.getResidenceCategory();

    $scope.getResidenceSubCategory = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'getResidenceSubCategory',
            data: {
                'PlantId': $scope.selectedData.PlantId,
                'ResidenceGroupId': $scope.selectedData.ResidenceGroupId,
            },
        }).then(function success(response) {
            $scope.ResidenceSubCategoryList = response.data;
        });
    }
   // $scope.getResidenceSubCategory();

    $scope.getBlock = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'getBlock',
            data: {
                'PlantId': $scope.selectedData.PlantId,
                'ResidenceGroupId': $scope.selectedData.ResidenceGroupId,
            },
        }).then(function success(response) {
            $scope.BlockList = response.data;
        });
    }
   // $scope.getBlock();

    $scope.RoomList = [];

    $scope.getRoom = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'getRoom',
            data: {
                'PlantId': $scope.selectedData.PlantId,
                'ResidenceGroupId': $scope.selectedData.ResidenceGroupId,
            },
        }).then(function success(response) {
            $scope.RoomList = response.data;
        });
    }
    //$scope.getRoom();

    $scope.EmployeeTypeIdList = [];
    $scope.getEmployeeType = function () {
        $http({
            method: 'POST',
            data: {
                'PlantId': $scope.selectedData.PlantId,
                'ResidenceGroupId': $scope.selectedData.ResidenceGroupId,
            },
            url: $scope.path + 'getEmployeeType',
        }).then(function success(response) {
            $scope.EmployeeTypeIdList = response.data;
        });
    }
   // $scope.getEmployeeType();

    $scope.ResidenceNumberList = [];
    $scope.getResidenceNumber = function () {
        $http({
            method: 'POST',
            data: {
                'PlantId': $scope.selectedData.PlantId,
                'ResidenceGroupId': $scope.selectedData.ResidenceGroupId,
            },
            url: $scope.path + 'getResidenceNumber',
        }).then(function success(response) {
            $scope.ResidenceNumberList = response.data;
        });
    }
   // $scope.getResidenceNumber();

    $scope.FloreList = [];
    $scope.getFloor = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'getFloor',
            data: {
                'PlantId': $scope.selectedData.PlantId,
                'ResidenceGroupId': $scope.selectedData.ResidenceGroupId,
            },
        }).then(function success(response) {
            $scope.FloreList = response.data;
        });
    }
    // $scope.getFloor();
    $scope.ResidentTypeList = [];
    $scope.getResidentType = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'getResidentType',
            data: {
                'PlantId': $scope.selectedData.PlantId,
                'ResidenceGroupId': $scope.selectedData.ResidenceGroupId,
            },
        }).then(function success(response) {
            $scope.ResidentTypeList = response.data;
        });
    }

    $scope.AssetNameList = [];
    $scope.getAssetName = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'getAssetName',
            data: {
                'PlantId': $scope.selectedData.PlantId,
                'ResidenceGroupId': $scope.selectedData.ResidenceGroupId,
            },
        }).then(function success(response) {
            $scope.AssetNameList = response.data;
        });
    }

    $scope.selectedData = {
        Id: null,      
        PlantId: null,
        ResidedenceGroupId: null,
        EmployeeCategoryId:null,
        Location: null,
        AssetName:null,
        ResidenceSubCategory: null,
        ResidenceCategory: null,
        Rooms: null,
        Block: null,
        ResidenceType: null,
        Floor: null,
        ResidenceType: null,
        ResidenceNumber: null,
        VacancyStatus: null,
        isActive:0,
    };

    $scope.view = function () {
        $http({
            method: "POST",
            url: $scope.path + "view",

            data: {
                
                'data': $scope.selectedData.VacancyStatus,
                'PlantId': $scope.selectedData.PlantId,
                'ResidenceGroupId': $scope.selectedData.ResidenceGroupId,
                'EmployeeCategoryId': $scope.selectedData.EmployeeCategoryId,
            },
            dataType: 'JSON',
        }).then(function successCallback(response) {
            $scope.ModelList = response.data;           
            var vacancy_status = document.getElementById('VacancyStatus').value;
            

        })
    }

    $scope.openChildGrid = function () {
        angular.element(document.querySelector('#EmpPop')).modal('show');
    }
    $scope.closeChildGrid = function () {
        angular.element(document.querySelector('#EmpPop')).modal('hide');
    }



    $scope.Clear = function () {
        ClearFields();
        return true;
    };

    function ClearFields() {
        $scope.Action = "Save";
        $scope.PlantList = [];
        $scope.LocationList = [];
        $scope.ResidenceGroupIdList = [];
        $scope.ResidenceCategoryList = [];
        $scope.ResidenceSubCategoryList = [];
        $scope.BlockList = [];
        $scope.AssetNameList = [];
        $scope.ResidentTypeList = [];
        $scope.FloreList = [];
        $scope.ResidenceNumberList = [];
        $scope.EmployeeTypeIdList = [];
        $scope.RoomList = [];
        $scope.selectedData = {
            Id: null,
            PlantId: null,
            ResidedenceGroupId: null,
            EmployeeCategoryId: null,
            Location: null,
            AssetName: null,
            ResidenceSubCategory: null,
            ResidenceCategory: null,
            Rooms: null,
            Block: null,
            ResidenceType: null,
            Floor: null,
            ResidenceType: null,
            ResidenceNumber: null,
            VacancyStatus: null,
            isActive: 0,
        };
       
    }

    $scope.EmployeeList = [];
    $scope.getEmployee = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'getEmployee',

        }).then(function success(resp) {
            $scope.EmployeeList = resp.data;
        })
    }
    $scope.getEmployee();
}