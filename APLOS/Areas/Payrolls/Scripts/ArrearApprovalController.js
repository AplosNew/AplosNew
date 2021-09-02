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

        try {

            var DropDownListYear = $("#ddlYearList").data("ejDropDownList");
            var _selectedBatch = DropDownListYear.getSelectedValue();


            if (baseService.isUndefinedOrNull(_selectedBatch)) {
                throw 'Please select batch';
            }

            var parameters = { 'batchId': $scope.FromDate, 'ToDate': $scope.ToDate };
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

        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.ProcessAll = function (isApprove) {



    }

    $scope.deleteArrear = function (EmpSystemID) {



    }
}