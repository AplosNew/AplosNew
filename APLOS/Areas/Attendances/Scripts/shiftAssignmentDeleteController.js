'use strict';
shiftAssignmentDeleteController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'toaster', 'cboService', '$controller', '$window'];
function shiftAssignmentDeleteController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, toaster, cboService, $controller, $window) {
    $scope.path = 'Attendances/ShiftAssignmentDelete/';
    $rootScope.title = 'shift Assignment Delete';
    $controller('employeeBaseController', { $scope: $scope, $http: $http });
    $scope.Action = 'Delete';
    $scope.processUrl = $scope.path + 'Delete';

    $scope.shiftassignmentdeletemodel = {
        FromDate: $filter('dateFiltering')(Date.now())
    };
    
    $scope.refreshTemplateemployee = function (args) {
        $("#headchk").ejCheckBox({ "change": CheckBoxSelectAllEmolyeeWise });
    };

    function CheckBoxSelectAllEmolyeeWise(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }
        var filtered = $("#GridPopUp").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.EmployeePopUpList.length; i++) {
                $scope.EmployeePopUpList[i].CheckBoxSelect = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].CheckBoxSelect = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridPopUp").data("ejGrid");
        gridObj.refreshContent();
    };


    $scope.EmployeeList = [];
    $scope.GetEmployeeInformation = function () {
        if (baseService.isUndefinedOrNull($scope.shiftassignmentdeletemodel.FromDate)) {
            manualValidation('div_FromDate', true, "From Date is required.");
        }        
        else {
            $scope.searchbyonRoleEmpList = [];
            var parameters = { 'fromDate': $scope.shiftassignmentdeletemodel.FromDate};
            $http({
                method: "POST",
                dataType: 'JSON',
                url: 'Attendances/ShiftAssignmentDelete/GetEmpInfo',
                data: parameters
            }).then(function successCallback(response) {
                if (response.data.length > 0) {
                    $scope.EmployeeList = response.data;
                    $scope.GetPopUpEmployee();
                }               
            });
        }
    };
    
    $scope.saveemployeedata = function () {
        var row = $filter('filter')($scope.EmployeePopUpList, { 'CheckBoxSelect': true });
        if (!baseService.isUndefinedOrNull(row) && row.length > 0) {
            $scope.EmployeeList = row;
        }
        $scope.Back();
    }

    $scope.Back = function () {
        angular.element(document.querySelector('#JobCardPopUp')).modal('hide');
    }

   $scope.EmployeePopUpList = [];
    $scope.GetPopUpEmployee = function () {
        if (baseService.isUndefinedOrNull($scope.shiftassignmentdeletemodel.FromDate)) {
            manualValidation('div_FromDate', true, "From Date is required.");
        }
        else {
            var parameters = { 'fromDate': $scope.shiftassignmentdeletemodel.FromDate};
            $http({
                method: "POST",
                dataType: 'JSON',
                url: 'Attendances/ShiftAssignmentDelete/GetEmpInfo',
                data: parameters
            }).then(function successCallback(response) {
                if (response.data.length > 0) {
                    $scope.EmployeePopUpList = response.data;
                }
            });
        }
    };
    
    $scope.showEmployeeFilterScreen = function () {
        try {
            var gridObj = $("#GridPopUp").data("ejGrid");
            gridObj.clearFiltering();
            angular.element(document.querySelector('#JobCardPopUp')).modal('show');

        } catch (e) {
            ShowResult(e, 'failure');
        }
    }
    
    $scope.DeleteShiftData = function () {
        try {
            var gridObj = $("#Grid").ejGrid("instance");
            var filtereddata = gridObj.getFilteredRecords();
            if (filtereddata.length == 0) {
                filtereddata = $scope.EmployeeList;
            }
            $scope.EmployeeListNew = [];
            for (var i = 0; i <filtereddata.length; i++) {
                if ($scope.EmployeeListNew, filtereddata[i].EmpSystemId) {
                    $scope.EmployeeListNew.push(filtereddata[i].EmpSystemId);
                    }            
            }                  
            if (baseService.isUndefinedOrNull($scope.shiftassignmentdeletemodel.FromDate)) {
                manualValidation('div_FromDate', true, "From Date is required.");
            }
            
            $scope.$broadcast('show-errors-check-validity');
                if ($scope.Action === 'Delete') {
                    $http({
                        method: 'POST',
                        url: $scope.processUrl,
                        data: { 'pFromDate': $scope.shiftassignmentdeletemodel.FromDate, 'EmpList': $scope.EmployeeListNew},
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure');
                        }
                        else {
                            ShowResult(response.data.Message, 'success');

                            $scope.GetEmployeeInformation();
                            var gridObj = $("#Grid").data("ejGrid");
                            gridObj.refreshContent(true);
                            gridObj.clearFiltering();
                        }

                    }), function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    };
                }
           
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };
    
}