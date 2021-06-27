'use strict';
IncrementReportSummaryController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function IncrementReportSummaryController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Increment Summary Report';
    $scope.ModelList = [];
    $scope.path = 'Payrolls/IncrementReportSummary/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';
    baseService.init($scope.getListUrl);

    $scope.languageId = null;

    
    $scope.localLanguageList = [];
    cboService.getLanguageIdCbo(function (result) {
        $scope.localLanguageList = result;
    });
    $scope.getIncrementSummaryReport = function() {
        
        try {

            if (!baseService.isUndefinedOrNull($scope.EmpSystemID)) {
                if (!baseService.isUndefinedOrNull($scope.languageId)) {
                    var file_src = $scope.path + "getIncrementReport?EmpSystemId=" + $scope.EmpSystemID + "&languageId=" + $scope.languageId ;
                    $rootScope.report(file_src);
                }
                else {
                    ShowResult('Please Select Language.', 'failure');
                }
            }
            else {
                ShowResult('Please Select Employee.', 'failure');
                //throw 'Please Select Employee.';
            }
            
        } catch (e) {

        }
    }

    $scope.employee = [];
    $scope.getPopUpData = function () {
     
        angular.element(document.querySelector('#employeeNewPopUp')).modal('show');
    }
    $scope.getPopUpDataOnly = function () {
        $scope.employee = [];
        $http({
            method: 'GET',
            url: 'employees/leaveApplication/getemployeelist'
        }).then(function successCallback(response) {
            $scope.employee = response.data;
        });
    }
    $scope.getPopUpDataOnly();
    $scope.SearchEmployee = function () {
        $scope.EmployeeNamee = null;
        $scope.EmpSystemID = null;


        for (var i = 0; i < $scope.employee.length; i++) {
            if ($scope.EmployeeCodee == $scope.employee[i].EmployeeCode) {
                $scope.EmployeeNamee = $scope.employee[i].EmployeeName;
                $scope.EmpSystemID = $scope.employee[i].SystemID;

                break;
            }

        }

    }
    $scope.closeEmployeePopUp = function () {
        angular.element(document.querySelector('#employeeNewPopUp')).modal('hide');
    };

    $scope.EmpSystemID = null;
    $scope.EmployeeCodee = null;
    $scope.EmployeeNamee = null;


    $scope.setEmpData = function (obj) {
       // $scope.Clear();
        var data = obj.data;

        $scope.EmpSystemID = data.SystemID;
        $scope.EmployeeCodee = data.EmployeeCode;
        $scope.EmployeeNamee = data.EmployeeName;
        $scope.closeEmployeePopUp();
    };

    //$scope.GetEnterEmployeeOutInfo = function () {
    //    var parameters = {
    //        'SearchValue': $scope.employeeInfoOut.EmployeeCode
    //    };
    //    $http({
    //        method: "POST",
    //        dataType: 'JSON',
    //        url: 'Attendances/AttendanceEntry/GetEmpInfo',
    //        data: parameters
    //    }).then(function successCallback(response) {
    //        if (response.data.length > 0) {
    //            $scope.employeeInfoOut.EmpSystemID = response.data[0].SystemID;
    //            $scope.employeeInfoOut.EmployeeCode = response.data[0].EmployeeCode;
    //            $scope.employeeInfoOut.EmployeeName = response.data[0].EmployeeName;
    //            $scope.employeeInfoOut.DOJ = response.data[0].DOJ;
    //            $scope.employeeInfoOut.DOC = response.data[0].DOC;
    //            $scope.employeeInfoOut.EmailId = response.data[0].EmailId;
    //            $scope.employeeInfoOut.Code = response.data[0].Code;
    //            $scope.employeeInfoOut.Section = response.data[0].Section;
    //            $scope.employeeInfoOut.SubSection = response.data[0].SubSection;
    //            $scope.employeeInfoOut.Department = response.data[0].Department;
    //            $scope.employeeInfoOut.LegalDesignation = response.data[0].LegalDesignation;
    //            $scope.GetPreDataOut($scope.employeeInfoOut.EmpSystemID, $scope.AttendanceEntryOut.PDate);
    //        }
    //        else {
    //            ShowResult("Please Select Correct Employee Code", 'failure');
    //        }
    //    });
    //};



   
}