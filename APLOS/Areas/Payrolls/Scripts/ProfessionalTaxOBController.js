'use strict';
ProfessionalTaxOBController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function ProfessionalTaxOBController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Professional Tax Opening Balance';
    $scope.Action = 'Save';
    $scope.ModelList = [];
    $scope.path = 'Payrolls/ProfessionalTaxOB/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';
    baseService.init($scope.getListUrl);
    $scope.saveBP = $scope.path + 'SaveTaxPolicyPlantWise';
    $scope.employee = [];

    //#region employee Load
    $scope.getPopUpData = function () {
        $scope.employee = [];
        $http({
            method: 'GET',
            url: 'employees/leaveApplication/getemployeelist'
        }).then(function successCallback(response) {
            $scope.employee = response.data;
        });
        angular.element(document.querySelector('#employeeNewPopUp')).modal('show');
    }
    $scope.closeEmployeePopUp = function () {
        angular.element(document.querySelector('#employeePopUp')).modal('hide');
        angular.element(document.querySelector('#employeeNewPopUp')).modal('hide');
    };

    $scope.leaveApplication = {
        EmployeeCode: null,
        EmpSystemID: null,
        EmployeeName: null,
        LegalDesignation: null,
        DOJ: null,
        DOC: null,
    };
    $scope.leaveApplicationNew = Object.assign({}, $scope.leaveApplication);


    $scope.setEmpData = function (obj) {
        $scope.Clear();
        var data = obj.data;
        $scope.leaveApplicationNew.EmployeeCode = data.EmployeeCode;
        $scope.leaveApplicationNew.EmpSystemID = data.SystemID;
        $scope.leaveApplicationNew.EmployeeName = data.EmployeeName;
        $scope.leaveApplicationNew.LegalDesignation = data.LegalDesignation;
        $scope.leaveApplicationNew.DOJ = data.DOJ;
        $scope.leaveApplicationNew.DOC = data.DOC;
        $scope.imageSrc = virtualPath.EmployeePic + data.EmpPicPath;
        $scope.getData($scope.leaveApplicationNew.EmpSystemID);
        angular.element(document.querySelector('#employeeNewPopUp')).modal('hide');        
    };

    $scope.Clear = function () {
        ClearFields();
        return true;
    };
    function ClearFields() {
        $scope.Action = 'Save';
        $scope.leaveApplication = { SectionId: $scope.leaveApplication.SectionId };
        $scope.leaveApplicationNew = { SectionId: $scope.leaveApplicationNew.SectionId };
        $scope.employeeInfo = [];
        $scope.leaveApplications = [];
        $scope.leaveApplicationNew.LeaveDayType = 'FullDay';
        $scope.LeaveBalanceList = [];
        $scope.LeaveTransactionList = [];
        $scope.imageSrc = virtualPath.EmployeePic + '';
    }
    //#endregion 

    //#region Tax Year

    $scope.YearList = [];
    $scope.getData = function () {
        $http({
            method: 'GET',
            url: 'Payrolls/TaxPolicy/GetTaxYear',
        }).then(function successCallback(response) {
            $scope.YearList = response.data;
        });
    }
    $scope.getData();

    //#endregion

    $scope.ProfessionalTaxOB = {
        Id: null,
        EmpSystemId: null,
        TaxYearId: null,
        OpeningTaxableIncomeEarned: null,
        OpeningTaxPaid: null,
    }

    $scope.getData = function (EmpSystemId) {
        $http({
            method: 'POST',
            url: $scope.path + "GetList",
            data: { empId: EmpSystemId },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ProfessionalTaxOB = response.data[0];
        });
    }
    //$scope.getData();

    $scope.Save = function () {
        $scope.ProfessionalTaxOB.EmpSystemId = $scope.leaveApplicationNew.EmpSystemID;
        $scope.ProfessionalTaxOB.Id =null;
        $http({
            method: 'POST',
            url: $scope.saveUrl,
            data: { 'data': $scope.ProfessionalTaxOB },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');                
                $scope.getData($scope.leaveApplicationNew.EmpSystemID);

            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        }
    };

    $scope.Delete = function () {        
            $http({
                method: 'POST',
                url: $scope.path + "Delete",
                data: { 'Id': $scope.ProfessionalTaxOB.Id },
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
