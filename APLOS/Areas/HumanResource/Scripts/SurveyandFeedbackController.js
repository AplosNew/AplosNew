'use strict';
SurveyandFeedbackController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function SurveyandFeedbackController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Survey and Feedback';
    $scope.Action = 'Save';
    $scope.ModelList = [];
    $scope.path = 'HumanResource/SurveyandFeedback/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'Save';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl);
    
    // TAB CHANGE
    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };

    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.redirectTab = function () {
        if ($scope.tabForm1.$invalid) {
            $scope.setTab(1);
        }
        else if ($scope.tabForm2.$invalid) {
            $scope.setTab(2);
        }
        else if ($scope.tabForm3.$invalid) {
            $scope.setTab(3);
        }
        else if ($scope.tabForm4.$invalid) {
            $scope.setTab(4);
        }
    };


    $scope.ModalTemp = {

        Id: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        EffectiveDate: null,
        Mentor:null,
        Importance:null,
        Remarks: null,
        IsActive: true,
    };
    $scope.ModalNew = Object.assign({}, $scope.ModalTemp);

    // Save Op
    $scope.Save = function () {
        $http({
            method: 'POST',
            url: $scope.saveUrl,
            data: {
                'data': $scope.ModalNew,               
            },
            dataType: 'JSON',
        }).then(function successCallback(response) {

            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
               
                ClearFields();

            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        }

    };

    // Clear all Fields
    $scope.Clear = function () {
        ClearFields();
        return true;
    };


    function ClearFields() {
        $scope.Action = 'Save';

        $scope.ModalTemp = {
            Id: null,
            ShortName: null,
            StandardName: null,
            UserName: null,
            EffectiveDate: null,
            Importance: null,
            Remarks: null,
            IsActive: true,
        };

        $scope.ModalNew = Object.assign({}, $scope.ModalTemp);
    }

    //=======================================EMPLOYEE POP UP======================================
   /* $scope.OpeEmployeePopUp = function () {
        angular.element(document.querySelector('#EmployeePop')).modal('show');
        $scope.getEmployee();
    }
    $scope.closeEmployeePopUp = function () {
        angular.element(document.querySelector('#EmployeePop')).modal('hide');

    }
    $scope.EmployeeList = [];
    $scope.getEmployee = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'getEmployee',
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.EmployeeList = resp.data;
        });

    }

    $scope.EmployeeId = null;
    $scope.Employee = null;
    $scope.doubleEmploye = function (e) {
        $scope.EmployeeId = e.data.SystemId;
        $scope.ResponsiblePerson = e.data.EmployeeName;
        angular.element(document.querySelector('#EmployeePop')).modal('hide');
        
    }

    $scope.getResponsiblePersonId = function () {
        $http({
            method: 'POST',
            data: { 'ResponsiblePersonId': $scope.EmployeeId, },
            url: $scope.path + 'getResponsiblePersonId',
        }).then(function success(response) {
            $scope.ResponsiblePerson = JSON.stringify(response.data[0].EmployeeName.replace(/\"/g, ""));
            $scope.ResponsiblePerson = $scope.ResponsiblePerson.replace(/\"/g, "");

        });
    }*/
    //=======================================EMPLOYEE POP UP======================================

}