'use strict';
employeeDeleteController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'cboService'];
function employeeDeleteController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, cboService) {
    $rootScope.title = 'Employee Delete';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.maternityLeaveTransactions = [];
    $scope.path = 'employees/EmployeeDelete/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';

    $scope.dataList = [];
    $scope.GetEmployeeDeleteInfo = function () {
        $scope.employeeInfo = {};
        $scope.maternityLeaveTransaction = {};
        $scope.maternityLeaveTransactionNew = {};
        $scope.maternityLeaveTransactionNew.ToDate = null;
        $scope.getDuration = null;
        $scope.dataList = [];
        $http({
            method: 'GET',
            url: 'employees/EmployeeDelete/getemployeeDelete'
        }).then(function successCallback(response) {
            $scope.dataList = response.data;
        });
        angular.element(document.querySelector('#employeeNewPopUp')).modal('show');
    }


    $scope.employeeInfo = {};
    $scope.SetData = function (obj) {
        var emp = obj.data;
        $scope.employeeInfo.EmpSystemID = emp.SystemID;
        if (baseService.isUndefinedOrNull($scope.maternityLeaveTransactionNew.EmpSystemID)) {
            $scope.maternityLeaveTransactionNew.EmpSystemID = $scope.employeeInfo.EmpSystemID;
        }
        $scope.employeeInfo.EmpPic = virtualPath.EmployeePic + emp.EmpPicPath;
        $scope.employeeInfo.EmployeeCode = emp.EmployeeCode;
        $scope.employeeInfo.EmployeeName = emp.EmployeeName;
        $scope.employeeInfo.DOJ = emp.DOJ;
        $scope.employeeInfo.EmailId = emp.EmailId;
        $scope.employeeInfo.Code = emp.Code;
        $scope.employeeInfo.Section = emp.Section;
        $scope.employeeInfo.SubSection = emp.SubSection;
        $scope.employeeInfo.Department = emp.Department;
        $scope.employeeInfo.LegalDesignation = emp.LegalDesignation;
        angular.element(document.querySelector('#employeeNewPopUp')).modal('hide');
        $scope.GetPreData($scope.employeeInfo.EmpSystemID);
    };

    $scope.closeEmployeePopUp = function () {
        angular.element(document.querySelector('#employeeNewPopUp')).modal('hide');
    }

    $scope.GetPreData = function (empId) {
        $http.get('humanresource/maternityleavetransaction/getleavebyempid?empId=' + empId)
            .then(function (response) {
                $scope.maternityLeaveTransactions = response.data;
            });
    };

    $scope.Delete = function () {       
     
        $http.get('employees/EmployeeDelete/Delete?empSystemID=' + $scope.employeeInfo.EmpSystemID)
              
                .then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');

                    $scope.maternityLeaveTransactions.splice($scope.index, 1);
                    baseService.paginationRemove();
                    ClearFields();
                    
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
    };

    $scope.Clear = function () {
        ClearFields();
        ClearExpectedFields();
        return true;
    };
    function ClearFields() {
        
        $scope.employeeInfo = {};
    }
}