'use strict';
RequisitionRegisterController.$inject = ['fileReader', 'commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter', 'cboService', '$window', '$controller'];
function RequisitionRegisterController(fileReader, commonMessage, $scope, $rootScope, baseService, $http, $filter, cboService, $window, $controller) {
    $rootScope.title = "Material Ledger / Report";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.products = [];
    $scope.path = 'Materials/RequisitionRegister/';
    $scope.path1 = 'Accounts/InventoryPayable/';
    //$scope.exportgriddataUrl = 'GridReports/ExcelExport';
    $scope.exportgriddataUrl = 'GridReports/ExcelExportJson';

    $scope.downloadgriddataUrl = 'GridReports/Download';
    $controller('baseMaterialAndArticleController', { $scope: $scope, $http: $http });
    $scope.RowColor = "";
    $scope.isAlternative = -1;

    $scope.report.FromDate = null;
    $scope.report.ToDate = null;

    $scope.productNew = {
         Type: null
        ,EmployeeId:null
    };
    $scope.productNew.Type = 'AllData';
    $scope.changeSourceFrom = function (from) {
        debugger;
        if (from === 'AllData') {
            $scope.report.FromDate = null;
            $scope.report.ToDate = null;

            $scope.productNew.Type = 'AllData';
            $scope.EmployeeList = [];
            $scope.productNew.EmployeeId = '';

        }
        if (from === 'EmployeeWise') {
           // $scope.EmpList();
            $scope.report.FromDate = null;
            $scope.report.ToDate = null;
            $scope.productNew.Type = 'EmployeeWise';


        }
    };
    $scope.getRequisitionRegisterReport = function (reportFormat) {
        //});
        try {

            if (baseService.isUndefinedOrNull($scope.report.FromDate)) {
                manualValidation("div_FromDate", true, "From Date is required.");
            }
            else if (baseService.isUndefinedOrNull($scope.report.ToDate)) {
                manualValidation("div_ToDate", true, "To Date is required.");
            }
            else if (new Date($scope.report.FromDate) > new Date($scope.report.ToDate)) {
                manualValidation("div_FromDate", true, "From date must be below or equal to To Date");
            }
            else if (new Date($scope.report.ToDate) < new Date($scope.report.FromDate)) {
                manualValidation("div_ToDate", true, "To date must be above or equal to From Date.");
            }
            if ($scope.productNew.Type === 'EmployeeWise' && baseService.isUndefinedOrNull($scope.productNew.EmployeeId)) {
                ShowResult('Select the employee', 'failure');
                return false;
            }
            
            else {

                var file_src = $scope.path + 'GetRequisitionRegisterReport?reportFormat=' + reportFormat + '&fromDate=' + $scope.report.FromDate + '&toDate=' + $scope.report.ToDate + '&employeeId=' + $scope.productNew.EmployeeId;
                $rootScope.report(file_src);
            }

        } catch (e) {

        }
    }

    $scope.startDate1 = '';
    $scope.endDate1 = '';
    $scope.GetFiscalYear1 = function () {
        $http({
            method: 'GET',
            url: 'Products/Requisition/GetFiscalYear?formattedDate=' + $filter("dateFiltering")(Date.now()),
        }).then(function successCallback(response) {
            $scope.startDate1 = response.data[0].StartDate;
            $scope.endDate1 = response.data[0].EndDate;
            //$scope.EmpList();
        });

    }
    $scope.GetFiscalYear1();
    $scope.EmployeeList = [];
    $scope.EmpList = function () {
        if ($scope.productNew.Type === 'AllData') {

        }
        else {
            //debugger
            $http({
                method: 'GET',
                url: "Products/Requisition/RequisitionByEmpInFixsal?startDate=" + $filter('dateFiltering')($scope.report.FromDate, 'dd-M-yyyy') + '&endDate=' + $filter('dateFiltering')($scope.report.ToDate, 'dd-M-yyyy'),
            }).then(function successCallback(response) {
                $scope.EmployeeList = response.data;


            });
		}
        
    }
}


 

