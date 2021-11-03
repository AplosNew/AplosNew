'use strict';
employeeBankInformationController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function employeeBankInformationController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Employee Bank Information';
    $scope.dataList = [];
    $scope.Action = 'Update';
    $scope.index = -1;
    $scope.employeeBankInformations = [];
    $scope.path = 'employees/EmployeeBankInformation/';
    //$scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateurl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl, null, null, null, 'EmpSystemID', 'EmpSystemID');

    $scope.getEmployeeBankData = function (pageno) {
        $rootScope.parameters.EmpSystemID = $scope.employeeBankInformation.EmpSystemID;
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.employeeBankInformations = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };

    $scope.employeeBankInformation = {
        RowID: null,
        EmployeeCode: null,
        EmployeeName: null,
        DOJ: null,
        GivenDesignation: null,
        Department: null,
        EmpSystemID: null,
        BankSystemID: null,
        BankBranchId: null,
        BankName: null,
        BankBranchName: null,
        BankAccNo: null,
        SalaryPercentage: null,
        IsApproved: false,
        ApprovedBy: null,
        ApprovedDateTime: null,
        DateAdded: null,
        AddedBy:null
    };

    $scope.searchingEmployeeList = [{
        'name': 'Employee Code',
        'value': 'EmployeeCode'
    },
    {
        'name': 'Employee Name',
        'value': 'EmployeeName'
    },
    {
        'name': 'DOJ',
        'value': 'DOJ'
    },
    {
        'name': 'Designation',
        'value': 'Designation'
    },
    {
        'name': 'Department',
        'value': 'Department'
    },
    {
        'name': 'Bank System Id',
        'value': 'BankSystemId'
    },
    {
        'name': 'Bank Name',
        'value': 'BankName'
    },
    {
        'name': 'Bank Account No',
        'value': 'BankAccNo'
    }];

    $scope.EmployeePopUpParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'EmployeeCodeNumeric',
        searchBy: "EmployeeCode",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.GetEmployeeModalData = function (pageno) {
        $scope.employeeBankInformation = {
            RowID: null,
            EmployeeCode: null,
            EmployeeName: null,
            DOJ: null,
            GivenDesignation: null,
            Department: null,
            EmpSystemID: null,
            BankSystemID: null,
            BankBranchId: null,
            BankName: null,
            BankBranchName: null,
            BankAccNo: null,
            SalaryPercentage: null,
            IsApproved: false,
            ApprovedBy: null,
            ApprovedDateTime: null,
            DateAdded: null,
            AddedBy: null
        };
        try {
            baseService.paginationBase('employees/employeebankinformation/GetEmployees/', pageno, $scope.EmployeePopUpParameters)
                .then(function (result) {
                    $scope.dataListEmployee = result.Rows;
                    $scope.EmployeePopUpParameters.total_count = result.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
            angular.element(document.querySelector('#PopUpEmployee')).modal('show');
        } catch (e) {
            ShowResult(e, 'Error');
        }
    };

    $scope.EmployeeBackUpParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'EmployeeCodeNumeric',
        searchBy: "EmployeeCode",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.GetBankBackUp = function (empId) {
        $scope.GetBackUpData = function (pageno) {
            try {
                baseService.paginationBase('employees/employeebankinformation/getEmployeeBankHistory?empSystemId=' + empId, pageno, $scope.EmployeeBackUpParameters)
                    .then(function (result) {
                        $scope.dataList = result.Rows;
                        $scope.EmployeeBackUpParameters.total_count = result.Total;
                    }, function () {
                        ShowResult(commonMessage.NetworkError, 'failure');
                    }).finally(function () {
                    });
            } catch (e) {
                ShowResult(e, 'Error');
            }
        };
        $scope.GetBackUpData();
    };

    $scope.getEmployeeDataOnLabels = function (emp) {
        $scope.employeeBankInformation.RowID = emp.RowID;
        $scope.employeeBankInformation.EmpSystemID = emp.EmpSystemID;
        $scope.employeeBankInformation.EmployeeCode = emp.EmployeeCode;
        $scope.employeeBankInformation.EmployeeName = emp.EmployeeName;
        $scope.employeeBankInformation.DOJ = emp.DOJ;
        $scope.employeeBankInformation.Department = emp.Department;
        $scope.employeeBankInformation.GivenDesignation = emp.GivenDesignation;
        $scope.employeeBankInformation.BankSystemID = emp.BankSystemID;
        $scope.employeeBankInformation.BankBranchId = emp.BankBranchId;
        $scope.employeeBankInformation.BankName = emp.BankName;
        $scope.employeeBankInformation.BankBranchName = emp.BankBranchName;
        $scope.employeeBankInformation.BankAccNo = emp.BankAccNo;
        $scope.employeeBankInformation.SalaryPercentage = emp.SalaryPercentage;
        $scope.employeeBankInformation.IsApproved = emp.IsApproved;

        $scope.employeeBankInformation.AddedBy = emp.AddedBy;
        $scope.employeeBankInformation.DateAdded = emp.DateAdded;

        $scope.GetBankBackUp(emp.EmpSystemID);

        angular.element(document.querySelector('#PopUpEmployee')).modal('hide');
    };

    $scope.Clear = function () {
        ClearFields();
        return true;
    };

    function ClearFields() {
        $scope.Action = 'Update';
        $scope.employeeBankInformation = {};
        $scope.employeeBankInformation.DOJ = null;
        $scope.dataList = [];
    }

    $scope.Update = function () {
        $http({
            method: 'POST',
            url: 'employees/EmployeeBankInformation/edit',
            data: $scope.employeeBankInformation,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.error === true) {
                ShowResult(response.data.Message, 'failure');
            } else {
                ShowResult(response.data.Message, 'success');
            }
        }, function errorCallBack(respose) {
            ShowResult(response.data.Message, 'failure');
        });
    };

    $scope.Delete = function () {
        $http({
            method: 'POST',
            url: 'employees/EmployeeBankInformation/delete?rowId=' + $scope.employeeBankInformation.RowID,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.error === true) {
                ShowResult(response.data.Message, 'failure');
            } else {
                ShowResult(response.data.Message, 'success');
                $scope.Clear();
            }
        }, function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        });
    };
}