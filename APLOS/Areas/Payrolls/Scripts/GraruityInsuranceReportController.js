'use strict';
GraruityInsuranceReportController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$window'];
function GraruityInsuranceReportController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window) {
    $rootScope.title = 'Graruity Insurance Report';
    $scope.path = 'Payrolls/GraruityInsuranceReport/';
    $scope.GratuityInsuranceAgreementUrl = $scope.path + 'GetGratuityInsuranceAgreement';  
    $scope.GetDataUrl = $scope.path + 'GetData';
    $scope.GetSummaryDataUrl = $scope.path + 'GetSummaryData';


    $scope.custompara = {
        AgreementId: null,
        FromDate: null,
        ToDate:null
    }

    $scope.GratuityInsuranceAgreementList = [];
    $scope.GetGratuityInsuranceAgreement = function () {
        try {


            $http.get($scope.GratuityInsuranceAgreementUrl)
                .then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.Message, 'failure');
                    }
                    else {
                        $scope.GratuityInsuranceAgreementList = [];                       
                        if (!baseService.isUndefinedOrNull(response.data)) {
                            $scope.GratuityInsuranceAgreementList = response.data;
                        }

                    }
                },

                    function errorCallBack(response) {
                        ShowResult(response.Message, 'failure');
                    });


        } catch (e) {
            ShowResult(e, "failure");
        }
    };
    $scope.GetGratuityInsuranceAgreement();

    $scope.GetData = function () {

        try {
            if (baseService.isUndefinedOrNull($scope.custompara.AgreementId)) {
                throw 'Please Select Agreement.'
            }
            if (baseService.isUndefinedOrNull($scope.custompara.FromDate)) {
                throw 'Please Enter From Date.'
            }
            if (baseService.isUndefinedOrNull($scope.custompara.ToDate)) {
                throw 'Please Enter To Date.'
            }

            location.href = $scope.GetDataUrl + '?AgreementId=' + $scope.custompara.AgreementId + '&FromDate=' + $scope.custompara.FromDate + '&ToDate=' + $scope.custompara.ToDate;

        } catch (e) {
            ShowResult(e, "failure");
        }

        //location.href = 'Attendances/EmployeeProfileUpload/GetSampleFileShift?reportFormat=' + ReportFormat + '&EmployeeIds=' + EmployeeIds;
        
    };
    $scope.GetSummaryData = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.custompara.AgreementId)) {
                throw 'Please Select Agreement.'
            }
            if (baseService.isUndefinedOrNull($scope.custompara.FromDate)) {
                throw 'Please Enter From Date.'
            }
            if (baseService.isUndefinedOrNull($scope.custompara.ToDate)) {
                throw 'Please Enter To Date.'
            }

            location.href = $scope.GetSummaryDataUrl + '?AgreementId=' + $scope.custompara.AgreementId + '&FromDate=' + $scope.custompara.FromDate + '&ToDate=' + $scope.custompara.ToDate;

        } catch (e) {
            ShowResult(e, "failure");
        }

        //location.href = 'Attendances/EmployeeProfileUpload/GetSampleFileShift?reportFormat=' + ReportFormat + '&EmployeeIds=' + EmployeeIds;
    };

}