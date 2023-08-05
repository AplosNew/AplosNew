'use strict';
ProcessManagementController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'cboService', '$window'];
function ProcessManagementController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, cboService, $window) {
    $scope.title = 'Process Management'
    $scope.path = 'Processes/ProcessManagement/';
    $scope.Action = 'Save';

    // #region TAB CHANGE
    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };

    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.UCtab = 1;
    $scope.SetUCTab = function (newTab) {
        $scope.UCtab = newTab;
    }

    $scope.isSetUC = function (newTab) {
        return $scope.UCtab === newTab;
    }
    // #endregion TAB CHANGE


    $scope.EntityList = [];
    $scope.LoadEntityDetails = function () {
        $http({
            method: 'POST',
            url: 'Processes/ProcessManagement/LoadEntityDetails'
        }).then(function successCallback(response) {
            $scope.EntityList = response.data;
        }
        )
    }
    $scope.LoadEntityDetails();

    $scope.ProcessList = [];
    $scope.LoadProcessList = function () {
        $http({
            method: 'POST',
            url: 'Processes/ProcessManagement/LoadProcessList'
        }).then(function successCallback(response) {
            $scope.ProcessList = response.data;
        }
        )
    }
    $scope.LoadProcessList();

    $scope.SubProcessList = [];
    $scope.LoadSubProcessList = function () {
        $http({
            method: 'POST',
            url: 'Processes/ProcessManagement/LoadSubProcessList'
        }).then(function successCallback(response) {
            $scope.SubProcessList = response.data;
        }
        )
    }
    $scope.LoadSubProcessList();

    $scope.MaterialList = [];
    $scope.LoadMaterialGrid = function () {
        $http({
            method: 'POST',
            url: 'Processes/ProcessManagement/LoadMaterialGrid'
        }).then(function successCallback(response) {
            $scope.MaterialList = response.data;
        }
        )
    }
    $scope.LoadMaterialGrid();

    $scope.UtilityList = [];
    $scope.LoadUtilityGrid = function () {
        $http({
            method: 'POST',
            url: 'Processes/ProcessManagement/LoadUtilityGrid'
        }).then(function successCallback(response) {
            $scope.UtilityList = response.data;
        }
        )
    }
    $scope.LoadUtilityGrid();

    $scope.ResponsiblePersonList = [];
    $scope.LoadResponsiblePopupData = function () {
        $http({
            method: 'POST',
            url: 'Processes/ProcessManagement/LoadResponsiblePopupData'
        }).then(function successCallback(response) {
            $scope.ResponsiblePersonList = response.data;
            angular.element(document.querySelector('#employeePopUps')).modal('show');
        }
        )
       
    }

    $scope.UOMList = [];
    $scope.GetUOM = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'GetUOM',
            dataType:'JSON'
        })
            .then(function successCallback(response) {
                $scope.UOMList = response.data;
            })
    }
    $scope.GetUOM();

    $scope.QualityManagementMasterWorkCenterList = [];
    $scope.GetWorkcenter = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'LoadWorkCenterDetails',
            dataType: 'JSON'
        })
            .then(function successCallback(response) {
                $scope.QualityManagementMasterWorkCenterList = response.data;
            })
    }
    $scope.GetWorkcenter();

    $scope.ProcessManagementTemp = {
        Id: null,
        StandaredName: null,
        UserName:null,
        Process: null,
        SubProcess:null,
        MinSPTInTime: null,
        MaxSPTInTime:null,
        StandardSPTInTime: null,
        ResponsiblePersonCode: null,
        EmployeeName:null
    }
    $scope.ProcessManagementNew = Object.assign({}, $scope.ProcessManagementTemp)

    $scope.SaveUtilityCategory = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'ProcessManagementNew',
            data: { 'data': $scope.UtilityCategoryModelNew },
            dataType: 'JSON'
        })
            .then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');

                }
            })
    }
}