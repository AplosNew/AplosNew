'use strict';
ResidenceMasterController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function ResidenceMasterController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Residence Master';
    $scope.Action = 'Save';
    $scope.ModelList = [];
    $scope.path = 'HumanResource/ResidenceMaster/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'Save';
    $scope.deleteUrl = $scope.path + 'delete/';

    

    // POP CLOSED FOR PLANT
    $scope.closePlantPop = function () {
        angular.element(document.querySelector('#PlantPop')).modal('hide');
    }

    // POP OPEN FOR RESIDENCE GROUP
    $scope.openResidencePop = function () {

        angular.element(document.querySelector('#ResidenceGroupPop')).modal('show');
    }

    // POP CLOSED FOR RESIDENCE GROUP
    $scope.closeResidencePop = function () {
        angular.element(document.querySelector('#ResidenceGroupPop')).modal('hide');
    }

    // POP OPEN FOR Employee Category
    $scope.openEmployeePop = function () {

        angular.element(document.querySelector('#EmpCatPop')).modal('show');
    }

    // POP CLOSED FOR Employee Category
    $scope.closeEmployeePop = function () {
        angular.element(document.querySelector('#EmpCatPop')).modal('hide');
    }
    // --------------------------------------------
    // POP OPEN FOR Employee SERVICE TYPE
    $scope.openEmpServiceType = function () {

        angular.element(document.querySelector('#EmpServiceType')).modal('show');
    }

    // POP CLOSED FOR Employee SERVICE TYPE
    $scope.closeEmpServiceType = function () {
        angular.element(document.querySelector('#EmpServiceType')).modal('hide');
    }

    $scope.PlantList = [];
    $scope.ResidenceGroupList = [];
    $scope.EmployeeCategoryList = [];
    $scope.EmpServiceTypeList = [];

    //Getting the RESIDENCE MASTER Data
    $scope.getData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetResidenceMaster",
            data: {
                'PlantId': $scope.SelectedPlantId,
                'ResidenceGroupId': $scope.ResidenceGroupId,
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ModelList = response.data;


        });
    }
    $scope.getData();



    $scope.getPlant = function () {
        $http({
            method: 'POST',
            url: $scope.path + "getPlant",
            dataType: 'JSON',
        }).then(function successCallback(response) {
            $scope.PlantList = response.data;
        });
    }
    $scope.getPlant();

    $scope.getResidenceGroup = function () {
        $http({
            method: 'POST',
            url: $scope.path + "getResidenceGroup",
            dataType: 'JSON',
        }).then(function successcallback(response) {
            $scope.ResidenceGroupList = response.data;
        })
    }
    $scope.getResidenceGroup();

    $scope.getEmployeeCategory = function () {
        $http({
            method: 'POST',
            url: $scope.path + "getEmployeeCategory",
            dataType: 'JSON',
        }).then(function successcallback(response) {
            $scope.EmployeeCategoryList = response.data;
        })
    }
    $scope.getEmployeeCategory();

    $scope.getEmpServiceType = function () {
        $http({
            method: 'POST',
            url: $scope.path + "getEmpServiceType",
            dataType: 'JSON',
        }).then(function successcallback(response) {
            $scope.EmpServiceTypeList = response.data;
        })
    }
    $scope.getEmpServiceType();

    $scope.SelectedPlantId = null;
    $scope.Plant = null;
    $scope.selectPlant = function (e) {
        $scope.SelectedPlantId = e.data.Id;
        $scope.Plant = e.data.UserName;
        angular.element(document.querySelector('#PlantPop')).modal('hide');
    }

    $scope.ResidenceGroupId = null;
    $scope.Residence = null;
    $scope.selectResidenceGroup = function (e) {
        $scope.ResidenceGroupId = e.data.Id;
        $scope.Residence = e.data.UserName;
        angular.element(document.querySelector('#ResidenceGroupPop')).modal('hide');
    }

    $scope.EmployeeCatId = null;
    $scope.EmployeeUN = null;
    $scope.selectEmployeeCategory = function (e) {
        $scope.EmployeeCatId = e.data.Id;
        $scope.EmployeeUN = e.data.UserName;
        angular.element(document.querySelector('#EmpCatPop')).modal('hide');
    }

    $scope.ServiceType = null;
    $scope.ServiceId = null;
    $scope.selectServiceType = function (e) {
        $scope.ServiceType = e.data.Service;
        $scope.ServiceId = e.data.Id;
        angular.element(document.querySelector('#EmpServiceType')).modal('hide');
    }
    // POP OPEN FOR PLANT
    $scope.openPlantPop = function () {

        angular.element(document.querySelector('#PlantPop')).modal('show');
    }



    // SAVE OPERATION

    $scope.ModalTemp = {

        Location: null,
        Id: null,
        ResidenceCategory: null,
        ResidenceSubCategory: null,
        PlantId: null,
        EmployeeCategoryId: null,
        ResidenceGroupId: null,
        Block: null,
        Floor: null,
        ResidenceNumber: null,
        Rooms: null,
        ResidentType: null,
        Vacancy: null,
        AssetName: null,
        Remarks: null,
        isActive: true,
    };
    $scope.ModalNew = Object.assign({}, $scope.ModalTemp);


    //$scope.Get = function (args) {
    //    $scope.ModalNew = Object.assign({}, args.data);

    //    $scope.Action = 'Update';

    //};

    $scope.Get = function (args) {

        $scope.ModalNew = Object.assign({}, args.data);
        $scope.dependency();
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();

        }
    };

    $scope.Save = function () {
        try {
            $scope.$broadcast('show-errors-check-validity');
            if ($scope.ModalNewForm.$valid) {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: {
                        'data': $scope.ModalNew,
                        'PlantId': $scope.SelectedPlantId,
                        'ResidenceGroupId': $scope.ResidenceGroupId,
                        'Emp': $scope.EmployeeCatId,
                        'ServiceTypeId': $scope.ServiceId,
                    },
                    dataType: 'JSON',
                }).then(function successCallback(response) {

                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.getData();
                        ClearFields();

                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }

    };

    $scope.Clear = function () {
        ClearFields();
        return true;
    };


    function ClearFields() {
        $scope.Action = 'Save';

        $scope.Plant = null;
        $scope.Residence = null;
        $scope.EmployeeUN = null;
        $scope.ModalTemp = {
            Id: null,
            Location: null,
            ResidenceCategory: null,
            ResidenceSubCategory: null,
            Block: null,
            Floor: null,
            ResidenceNumber: null,
            Rooms: null,
            ResidentType: null,
            Vacancy: null,
            AssetName: null,
            Remarks: null,
            isActive: true,
            Rent: null,
        };

        $scope.ModalNew = Object.assign({}, $scope.ModalTemp);
    }

    var app = angular.module('vacancyStatus', [])
    app.controller('vacancyStatuscontroller', function ($scope) {
        $scope.BookList = [{
            VacancyStatusId: '1',
            VacancyStatusName: 'Valid'
        }, {
            VacancyStatusId: '2',
            VacancyStatusName: 'Occupied'
        }, {
            VacancyStatusId: '3',
            VacancyStatusName: 'vacant'
        }, {
            BookId: '4',
            VacancyStatusName: 'All'
        },];

        $scope.GetSelectedValue = function () {
            if ($scope.SelectedBook) {
                $scope.selectedVacancyStatusName = $scope.SelectedVacancyStatus.VacancyStatusName;

            }
            else {
                $scope.selectedVacancyStatusName = 'Please select Vacancy Status';

            }
        }
    });
    //$scope.GetSelectedValue();

    $scope.residenceCategoryList = [];
    $scope.dependency = function () {
        var residentType = document.getElementById("ResidenceType").value;
        if ($scope.ModalNew.ResidentType == "Family") {

            $scope.residenceCategoryList = [
                {
                    'Value': 'Joint',
                    'Text': 'Joint'
                }

            ];
        }
        if ($scope.ModalNew.ResidentType == "Bachelor") {

            $scope.residenceCategoryList = [
                {
                    'Value': 'Male',
                    'Text': 'Male'
                },
                {
                    'Value': 'Female',
                    'Text': 'Female'
                }

            ];
        }
    }

    //  #region  Position Tab

    $scope.ModalTempP = {
        Id: null,
        PlantId: null,
        EntityId: null,
        MPBudgetCodeId: null,
        PositionId: null,
    };
    $scope.ModalNewPosition = Object.assign({}, $scope.ModalTempP);

    // Lists
    $scope.BudgetCodeList = [];
    $scope.EntityList = [];
    $scope.PositionList = [];

    $scope.getEntity = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'getEntity'
        }).then(function successCallback(response) {
            $scope.EntityList = response.data;
        })
    }
    $scope.getEntity();
    $scope.getPosition = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'getPosition',
            data: { 'MPBudgetId': $scope.ModalNewPosition.MPBudgetCodeId },
        }).then(function successCallback(response) {
            $scope.PositionList = response.data;
        })
    }

    $scope.getBudgetCode = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'getBudgetCode',
            data: { 'entityId': $scope.ModalNewPosition.EntityId },
        }).then(function successCallback(response) {
            $scope.BudgetCodeList = response.data;
        })
    }

    $scope.PositionTabgridList = [];
    $scope.getPositionTabGridData = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'getPositionTabGridData',
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.PositionTabgridList = response.data;
        })
    }
    $scope.getPositionTabGridData();
    // #endregion Position Tab
}