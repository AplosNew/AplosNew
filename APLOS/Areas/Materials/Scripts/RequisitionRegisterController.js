'use strict';
RequisitionRegisterController.$inject = ['fileReader', 'commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter', 'cboService', '$window', '$controller'];
function RequisitionRegisterController(fileReader, commonMessage, $scope, $rootScope, baseService, $http, $filter, cboService, $window, $controller) {
    $rootScope.title = "Requisition Register";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.products = [];
    $scope.path = 'Materials/RequisitionRegister/';
    $scope.path1 = 'Accounts/InventoryPayable/';
    //$scope.exportgriddataUrl = 'GridReports/ExcelExport';
    $scope.exportgriddataUrl = 'GridReports/ExcelExportJson';

    $scope.downloadgriddataUrl = 'GridReports/Download';
    $controller('baseMaterialAndArticleController', { $scope: $scope, $http: $http });

    $scope.report.FromDate = null;
    $scope.report.ToDate = null;

    $scope.report = {
        PartyType: null,
        Type: null,
        EmployeeId: null
    };

    $scope.employeeList = [];
    $scope.employeeIndex = -1;
    $scope.selectedEmployee = null;
    $scope.searchEmployeeByList = [
        {
            'name': 'Employee Code',
            'value': 'EmployeeCode'
        },
        {
            'name': 'First Name',
            'value': 'FirstName'
        },
        {
            'name': 'Middle Name',
            'value': 'MiddleName'
        },
        {
            'name': 'Last Name',
            'value': 'LastName'
        },
        {
            'name': 'Employee Name',
            'value': 'EmployeeName'
        },
        {
            'name': 'Designation',
            'value': 'DesignationName'
        },
        {
            'name': 'Entity',
            'value': 'EntityName'
        },
        {
            'name': 'Department',
            'value': 'Department'
        },
        {
            'name': 'Employment Type',
            'value': 'EmploymentType'
        },
        {
            'name': 'Status',
            'value': 'EmployeeStatus'
        }
    ];

    $scope.employeeParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'EmployeeCode',
        searchBy: 'EmployeeCode',
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.showEmployeeListPopUp = function () {
        baseService.setCurrentPage('employeeList');
        $scope.getEmployeeData = function (pageno) {
            var url = 'employees/EmployeeInformation/GetEmployeeListByPlant';
            baseService.paginationBase(url, pageno, $scope.employeeParameters)
                .then(function (result) {
                    $scope.employeeList = result.Rows;
                    $scope.employeeParameters.total_count = result.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#employeePopUp')).modal('show');
        $scope.getEmployeeData();

    }

    $scope.selectEmployeePopUp = function (index, id) {
        $scope.employeeIndex = index;
        $scope.selectedEmployee = id;

    };
    $scope.closeEmployeePopUp = function () {
        if ($scope.employeeIndex !== -1) {
            var employee = $scope.employeeList[$scope.employeeIndex];
            $scope.report.PartyType = 'Employee';
            $scope.report.EmployeeId = employee.SystemId;
            $scope.EmployeeName = employee.EmployeeName;
        }
        $scope.hideEmployeePopUp();
    };
    $scope.hideEmployeePopUp = function () {
        angular.element(document.querySelector('#employeePopUp')).modal('hide');
        $scope.employeeIndex = -1;
        $scope.selectedEmployee = null;
    };

    $scope.RowColor = "";
    $scope.isAlternative = -1;


    $scope.report.Type = 'AllData';
    $scope.changeSourceFrom = function (from) {
        debugger;
        if (from === 'AllData') {
            $scope.report.FromDate = null;
            $scope.report.ToDate = null;

            $scope.report.Type = 'AllData';
            $scope.EmployeeList = [];
            $scope.report.EmployeeId = '';

        }
        if (from === 'EmployeeWise') {
           // $scope.EmpList();
            $scope.report.FromDate = null;
            $scope.report.ToDate = null;
            $scope.report.Type = 'EmployeeWise';


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

            var file_src = $scope.path + 'GetRequisitionRegisterReport?reportFormat=' + reportFormat + '&status=' + $scope.RequisitionStatus+ '&fromDate=' + $scope.report.FromDate + '&toDate=' + $scope.report.ToDate + '&employeeId=' + $scope.report.EmployeeId;
                $rootScope.report(file_src);

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
        });

    }
    $scope.GetFiscalYear1();
  
}


 

