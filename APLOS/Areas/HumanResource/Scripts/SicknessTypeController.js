'use strict';
SicknessTypeController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function SicknessTypeController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Sickness Type';
    $scope.Action = 'Save';
    $scope.ModelList = [];
    $scope.path = 'HumanResource/SicknessType/';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'Save';
    $scope.deleteUrl = $scope.path + 'Delete/';
    baseService.init($scope.getListUrl);
    $scope.downloadgriddataUrl = 'GridReports/Download';

    //=======================================EMPLOYEE POP UP======================================
    $scope.OpeEmployeePopUp = function () {
        angular.element(document.querySelector('#EmployeePop')).modal('show');
        $scope.getEmployee();
    }
    $scope.closeEmployeePopUp = function () {
        angular.element(document.querySelector('#EmployeePop')).modal('hide');

    }
    //=======================================EMPLOYEE POP UP CLOSE======================================
    // ================================================GET MAIN GRID DATA====================================================

    $scope.getData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetList",

            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ModelList = response.data;
            //ClearFields(response.data.Sequence);
            
        });
    }
   // $scope.getData();
    // ================================================GET MAIN GRID DATA CLOSE====================================================

    // ================================================FORM OBJECT DECLARATION & INITIALIZATION====================================
    $scope.ModelTemp = {
        Id: null,
        
    };
    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);
    // ================================================FORM OBJECT DECLARATION & INITIALIZATION=====================================

    //=======================================DOUBLE CLICK ON GRID OPEN FORM============================================
    $scope.Get = function (args) {
        $scope.ModelNew = Object.assign({}, args.data);
        $scope.EmployeeId = args.data.ResponsiblePerson;
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
            $scope.getResponsiblePersonId();
        }
    };
    //=======================================DOUBLE CLICK ON GRID OPEN FORM CLOSE============================================

    //=======================================SAVE============================================
    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');

        $http({
            method: 'POST',
            url: $scope.saveUrl,
            data: {
                'data': $scope.ModelNew,
            },
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
    };
    //=======================================SAVE CLOSE==========================================

    //=======================================DELETE FUNCTION======================================
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
    //=======================================DELETE CLOSE======================================

    //=======================================CLEAR FORM======================================
    $scope.Clear = function () {
        ClearFields();
        return true;
    };
    function ClearFields() {
        $scope.Action = 'Save';
        $scope.ModelTemp = {
            Id: null
            };

        $scope.ModelNew = Object.assign({}, $scope.ModelTemp);
    }
    //=======================================CLEAR FORM======================================
}