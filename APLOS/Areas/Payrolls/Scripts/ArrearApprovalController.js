'use strict';
ArrearApprovalController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function ArrearApprovalController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Arrear Approval';
    $scope.Action = 'Save';
    $scope.ModelList = [];
    $scope.path = 'Payrolls/ArrearApproval/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';

    $scope.FromDate = new Date();
    $scope.ToDate = new Date();

    $scope.ArrearProcessInfo = [];
    $scope.SelectedArrearProcessBatchId = null;
    $http({
        method: "GET",
        dataType: 'JSON',
        url: 'humanresource/PayrollReports/GetAllArrearProcessInfo'
    }).then(function successCallback(response) {
        $scope.ArrearProcessInfo = response.data;
    });

    $scope.EmployeeListApproved = [];
    $scope.EmployeeListUnApproved = [];
    $scope.GetEmployeeInformation = function () {


        if (baseService.isUndefinedOrNull($scope.FromDate)) {
            manualValidation('div_FromDate', true, "From Date is required.");
        }
        else if (baseService.isUndefinedOrNull($scope.ToDate)) {
            manualValidation('div_ToDate', true, "To Date is required.");
        }
        else if (new Date($scope.FromDate) > new Date($scope.ToDate)) {
            manualValidation('div_FromDate', true, "From date must be below or equal to To Date");
        }
        else if (new Date($scope.ToDate) < new Date($scope.FromDate)) {
            manualValidation('div_ToDate', true, "To date must be above or equal to From Date.");
        }
        else {
            $scope.searchbyonRoleEmpList = [];
            var parameters = { 'FromDate': $scope.FromDate, 'ToDate': $scope.ToDate };
            $http({
                method: "POST",
                dataType: 'JSON',
                url: $scope.path + 'GetEmpList',
                data: parameters
            }).then(function successCallback(response) {
                for (var i = 0; i < response.data.length; i++) {

                    if (angular.isUndefinedOrNull(response.data[i].DOJ) == false)
                        response.data[i].DOJ = new Date(response.data[i].DOJ);

                    if (angular.isUndefinedOrNull(response.data[i].DOS) == false)
                        response.data[i].DOS = new Date(response.data[i].DOS);

                    if (angular.isUndefinedOrNull(response.data[i].LastSalaryEffectiveDate) == false)
                        response.data[i].LastSalaryEffectiveDate = new Date(response.data[i].LastSalaryEffectiveDate);

                    if (angular.isUndefinedOrNull(response.data[i].LatestSalaryEffectiveDate) == false)
                        response.data[i].LatestSalaryEffectiveDate = new Date(response.data[i].LatestSalaryEffectiveDate);

                }

                $scope.EmployeeListApproved = ej.DataManager(response.data).executeLocal(ej.Query().where("IsApproved", "equal", true));
                $scope.EmployeeListUnApproved = ej.DataManager(response.data).executeLocal(ej.Query().where("IsApproved", "equal", false));
               
            });
        }

    };

    $scope.ProcessAll = function (isApprove) {



    }
}