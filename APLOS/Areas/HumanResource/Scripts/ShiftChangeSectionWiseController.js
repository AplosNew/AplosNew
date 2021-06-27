'use strict';
ShiftChangeSectionWiseController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'toaster'];
function ShiftChangeSectionWiseController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, toaster) {
    $rootScope.title = 'Shift Change Section Wise';
    $scope.path = 'HumanResource/ShiftChangeSectionWise/';
    $scope.Action = 'Save';

    //#region Tab
    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };
    // #endregion Tab

    //#region Get Section
    $scope.SectionList = [];
    $scope.getSection = function () {
        $http({
            method: 'GET',
            url: $scope.path + "GetSection",
        }).then(function successCallback(response) {
            $scope.SectionList = response.data;
        });
    }
    $scope.getSection();
    //#endregion

    //#region Get other
    $scope.date = new Date();
    $scope.SectionId = null;
    $scope.EmployeeList = [];
    $scope.LoadEmp = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetEmployee",
            data: { 'section': $scope.SectionId },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.EmployeeList = response.data;            
        });
    }
    $scope.EmpList = [];
    $scope.LoadEmployee = function () {
        $http({
            method: 'POST',
            url: $scope.path + "LoadEmp",
            data: { 'section': $scope.SectionId, 'date': $scope.date },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.EmpList = response.data;            
        });
    }
    //#endregion

}