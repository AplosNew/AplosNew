'use strict';
function LegalSalaryReportController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, toaster, cboService) {
    // #region ****Initial****
    $rootScope.title = 'Legal Salary Report';
    $scope.path = 'HumanResource/legalsalarystructure/';
    // #endregion

    //$scope.legalSalaryGradeList = [];
    //$http.get('HumanResource/LegalSalaryGrade/getcbo')
    //    .then(function (response) {
    //        $scope.legalSalaryGradeList = response.data;
    //    });

    // #region ****Scope Legal Salary Report***
    $scope.legalSalaryReport = {
        //LegalSalaryGradeId: null,
        EffectiveDate: $filter('dateFiltering')(Date.now()),
        GradeLevel: 'Grade',
        CompanyId: null,
        PlantId: null
    };

    //Default Current Date selected
    $('.datepicker').datepicker({
        forceParse: false,
        format: 'dd-M-yyyy', autoclose: true, reset: true, todayHighlight: true, setDate: new Date()
    });
    // #endregion

    $scope.companyList = [];
    cboService.getCompanyGroupCompanyCbo(null, function (result) {
        $scope.companyList = result;
    });
    $scope.companyOnChange = function () {
        $scope.plantList = [];
        cboService.getCboPlantByCompany($scope.legalSalaryReport.CompanyId, function (result) {
            $scope.plantList = result;
        });
    }

    $scope.GradeHideShowFn = function () {
        if ($scope.legalSalaryReport.GradeLevel == 'Grade') {
            $scope.CompanyHideShow = false;
            $scope.legalSalaryReport.CompanyId = null;
            $scope.legalSalaryReport.PlantId = null;
            $scope.selectMessageCompany = '';
            $scope.selectMessagePlant = '';
        }
        else {
            $scope.CompanyHideShow = true;
            $scope.legalSalaryReport.CompanyId = null;
            $scope.legalSalaryReport.PlantId = null;
            $scope.selectMessageCompany = '';
            $scope.selectMessagePlant = '';
        }

    }
    $scope.GradeHideShowFn();

    // #region *****Report*******
    $scope.legalSalaryReport = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.form.$valid) {
            if ($scope.legalSalaryReport.GradeLevel == 'GradeWithDesignation' & $scope.legalSalaryReport.CompanyId == null & $scope.legalSalaryReport.PlantId == null) {
                $scope.selectMessageCompany = 'Company is required';
                $scope.selectMessagePlant = 'Plant is required';
            }
            else if ($scope.legalSalaryReport.GradeLevel == 'GradeWithDesignation' & $scope.legalSalaryReport.CompanyId == null) {
                $scope.selectMessageCompany = 'Company is required';
                $scope.selectMessagePlant = '';
            }
            else if ($scope.legalSalaryReport.GradeLevel == 'GradeWithDesignation' & $scope.legalSalaryReport.PlantId == null) {
                $scope.selectMessageCompany = '';
                $scope.selectMessagePlant = 'Plant is required';
            }
            else {
                location.href = 'HumanResource/legalsalarystructure/legalsalaryreport?effectiveDate=' + $scope.legalSalaryReport.EffectiveDate + '&plantId=' + $scope.legalSalaryReport.PlantId;

                //$http({
                //    method: 'post',
                //    url: 'HumanResource/legalsalarystructure/legalsalaryreport',
                //    //data: $scope.restrictionList,
                //    params: {
                //        'effectiveDate': $scope.legalSalaryReport.EffectiveDate,
                //        'plantId': $scope.legalSalaryReport.PlantId
                //    },
                //    dataType: 'json'
                //}).then(function successCallback(response) {
                //    if (response.data.Error == true) {
                //        ShowResult(response.data.Message, 'failure');
                //    }
                //    else {
                //        var blob = new Blob([response.data], { type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' });
                //        var objectUrl = URL.createObjectURL(blob);
                //        window.open(objectUrl);
                //    }
                //}, function errorCallback(response) {
                //    ShowResult(response.status.Message, 'failure');
                //});


                $scope.selectMessageCompany = '';
                $scope.selectMessagePlant = '';
            }
        }
    };

    // #endregion
}
LegalSalaryReportController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'toaster', 'cboService'];
