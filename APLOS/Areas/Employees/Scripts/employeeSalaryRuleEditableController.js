'use strict';
EmployeeSalaryRuleEditableController.$inject = ['cboService', 'commonMessage', "$window", '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function EmployeeSalaryRuleEditableController(cboService, commonMessage, $window, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $scope.tableShow = false;
    $scope.Action = 'Save';
    $scope.employeeSalaryRuleEditableList = [];
    $scope.employeeSalaryRuleEditableSelectedList = [];
    $scope.path = 'Employees/EmployeeSalaryRuleEditable/GetList';
    $scope.plantList = [];
    cboService.getCboPlantByCompanyGroup(null, function (result) {
        $scope.plantList = result;
    });
    $scope.employeeSalaryRuleEditableOb = {
        Id: null,
        CompanyGroupId: $window.companyGroupId,
        CompanyId: $window.companyId,
        PlantId: null,
        EmployeeId: null,
        EmployeeName: null,
        BudgetCode: null
    };
    $scope.companyList = [];
    cboService.getCboCompanyByCompanyGroup($scope.employeeSalaryRuleEditableOb.CompanyGroupId, function (result) {
        $scope.companyList = result;
    });
    $scope.plantList = [];
    $scope.getPlantByCompany = function () {
        cboService.getCboPlantByCompany($scope.employeeSalaryRuleEditableOb.CompanyId, function (result) {
            $scope.plantList = result;
        });
    };

    $scope.getEmployeeSalaryRuleEditableData = function () {
        $http.get("Employees/EmployeeSalaryRuleEditable/GetList")
            .then(
                function successCallback(response) {
                    $scope.employeeSalaryRuleEditableSelectedList = response.data.Rows;
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
    };
    $scope.getEmployeeSalaryRuleEditableData();
    //

    $scope.popUpList = [];
    $scope.popUpDataList = [];
    $scope.popUp = function () {
        try {
            $scope.employeeParameters = {
                limit: 10,
                offset: 0,
                order: 'asc',
                sort: '',
                searchBy: '',
                pageSize: 10,
                total_count: 0,
                search: null,
                serverPagination: true
            };
            $scope.searchEmployeeByList = [
                {
                    name: 'Employee Code',
                    value: 'EmployeeCode'
                },
                {
                    name: 'Employee Name',
                    value: 'EmployeeName'
                }, {
                    name: 'Budget Code',
                    value: 'BudgetCode'
                },
                {
                    name: 'Given Designation',
                    value: 'GivenDesignation'
                },
                {
                    name: 'Department',
                    value: 'Department'
                },
                {
                    name: 'Section',
                    value: 'Section'
                }
            ];

            $scope.popUpUrl = '';
            $scope.employeeParameters.sort = '';
            $scope.employeeParameters.searchBy = '';
            $scope.popUpTitle = 'Employee';
            $scope.popUpUrl = 'employees/approvalconfiguration/getemployeedatalist';
            $scope.employeeParameters.sort = 'EmployeeCode';
            $scope.employeeParameters.searchBy = 'EmployeeCode';

            $scope.getEmployeeData = function (pageno) {
                baseService.paginationBase($scope.popUpUrl, pageno, $scope.employeeParameters)
                    .then(function (result) {
                        $scope.employeeList = result.Rows;
                        $scope.employeeParameters.total_count = result.Total;
                    }, function () {
                        ShowResult(commonMessage.NetworkError, 'failure', '#employeePopUp');
                    }).finally(function () {
                    });
            };

            $scope.fieldName = name;
            angular.element(document.querySelector('#employeePopUp')).modal('show');
            $scope.getEmployeeData();
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };
   
    $scope.selectdblClick = function (data) {
        addRow(data);
        angular.element(document.querySelector('#employeePopUp')).modal('hide');
    };
    function addRow(ob) {
        $scope.employeeSalaryRuleEditableOb.EmployeeId = ob.SystemId;
        $scope.employeeSalaryRuleEditableOb.EmployeeName = ob.EmployeeName;
        $scope.employeeSalaryRuleEditableOb.BudgetCode = ob.BudgetCode;
        var ob = Object.assign({}, $scope.employeeSalaryRuleEditableOb);
        if (checkExisting(ob.EmployeeId) === false) {
            $scope.employeeSalaryRuleEditableSelectedList.push(ob);
        } else {
            return ShowResult("This employee already added", 'failure', 'employeePopUp');
        }
    }
    function checkExisting(id) {
        for (var i = 0; i < $scope.employeeSalaryRuleEditableSelectedList.length; i++) {
            var ob = $scope.employeeSalaryRuleEditableSelectedList[i];
            if (ob.EmployeeId === id) {
                return true;
                break;
            }
        }
        return false;
    }
    $scope.valueData = '';
    $scope.selectSingleClick = function (data) {
        $scope.valueData = data;
    };
    $scope.SelectByButton = function () {
        if ($scope.valueData === '') {
            alert('Please at first select row');
            return;
        }
        $scope.selectdblClick($scope.valueData);
        $scope.valueData = '';
        angular.element(document.querySelector('#popUp')).modal('hide');
    };
    $scope.closePopUp = function () {
        angular.element(document.querySelector('#popUp')).modal('hide');
    };
    $scope.clearProfileUpload = function () {
        $scope.approvalConfigurationNew.ProfileUploadRP = null;
        $scope.approvalConfigurationNew.ProfileUploadRPerson = null;
    };
    //Deleting Rows from EmployeeSalaryRuleEditableList
    $scope.valuePassInDelModal = function (index, data) {
        $scope.tempEmployeeSalaryRuleEditableOb = data;
        $scope.glMappingIndex = index;
        if (baseService.isUndefinedOrNull($scope.tempEmployeeSalaryRuleEditableOb.Id))
            $scope.message_confirmation = 'Are you sure want to parmenently delete this data....';
        else
            $scope.message_confirmation = 'Are you sure want to delete [ ' + data.EmployeeName + ' ]';
        angular.element(document.querySelector('#confirmDocumentdelete')).modal('show');
    };

    $scope.removeRow = function () {
        if (baseService.isUndefinedOrNull($scope.tempEmployeeSalaryRuleEditableOb.Id) === true) {
            $scope.employeeSalaryRuleEditableSelectedList.splice($scope.glMappingIndex, 1);
        } else {
            $scope.removeFromDb($scope.tempEmployeeSalaryRuleEditableOb.Id, $scope.glMappingIndex);
        }
        $scope.glMappingIndex = -1;
        $scope.$scope.tempEmployeeSalaryRuleEditableOb.Id = null;
        angular.element(document.querySelector('#confirmDocumentdelete')).modal('hide');
    };
    $scope.removeFromDb = function (id, index) {
        try {
            $http({
                method: 'POST',
                url: 'Employees/EmployeeSalaryRuleEditable/Delete',
                dataType: 'JSON',
                data: { 'id': id }
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.employeeSalaryRuleEditableSelectedList.splice($scope.glMappingIndex, 1);
                    $scope.glMappingIndex = -1;
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, 'failure');
            });
            return true;
        } catch (e) {
            ShowResult(e, 'Error');
        }
    };
    //Save
    $scope.Save = function () {
        try {
            if ($scope.Action === 'Save') {
                $http({
                    method: 'POST',
                    url: 'Employees/EmployeeSalaryRuleEditable/create',
                    data: { 'employeeSalaryRuleEditable': $scope.employeeSalaryRuleEditableSelectedList },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.getEmployeeSalaryRuleEditableData();
                    }
                });
                return true;
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    //
    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };
}