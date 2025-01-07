'use strict';
employeeRegisterController.$inject = ['fileReader', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'cboService', '$window', 'toaster'];
function employeeRegisterController(fileReader, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, cboService, $window, toaster) {
    $rootScope.title = 'Employee Information';
    $scope.index = -1;

    $scope.paidHoursEmployeeAssign = {
        CompanyGroupId: $window.companyGroupId,
        PlantId: $window.plantId,
        PaidHours: null,
        EmployeeId: null,
        EmployeeCategoryId: null,
        EmployeeCategory: null,
        EmployeeCode: "",
        EmployeeName: ""
    };
    $scope.paidHoursEmployeeAssignNew = Object.assign({}, $scope.paidHoursEmployeeAssign);
    // #region ****Scope Ledger Report***

    $scope.emp = {
        Id: null,
        EmployeeCode: null,
        EmployeeId: null,
        EmployeeName: null
    };
    $scope.EmployeeInFoReport = {
        EmployeeCatagory: 'Active',
        ReportFormat: 'Excel',
        CheckBox: false,
        LONGABSENTEEISM: false,
        TBS: false,
        EmployeeCurrentStatus: null
    };
    $scope.EmpRegisterPrint = function () {
        try {
            location.href = 'employees/EmployeeInformation/EmpRegisterInfo?radioValue=' + $scope.EmployeeInFoReport.EmployeeCatagory;

            $scope.Clear();

        } catch (e) {
            ShowResult(e, 'Error');
        }
    };
    //$scope.selectedemployeeList = [];

    $scope.employeeList = [];
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
                },
                {
                    name: 'Given Designation',
                    value: 'GivenDesignation'
                },
                {
                    name: 'Department',
                    value: 'Department'
                }
            ];
            $scope.popUpUrl = '';
            $scope.employeeParameters.sort = '';
            $scope.employeeParameters.searchBy = '';
            $scope.popUpTitle = 'Employee';
            $scope.popUpUrl = 'employees/approvalconfiguration/GetEmployeeDataList';
            $scope.employeeParameters.sort = 'EmployeeCode';
            $scope.employeeParameters.searchBy = 'EmployeeCode';
            $scope.getEmployeeData = function (pageno) {
                baseService.paginationBase($scope.popUpUrl, pageno, $scope.employeeParameters)
                    .then(function (result) {
                        $scope.employeeList = result.Rows;
                        $scope.employeeParameters.total_count = result.Total;
                        getListForm($scope.employeeList);
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
    }
    //#region MultiDropDrown
    function ddlFilter(list, id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].Value === id)
                return true;
        }
        return false;
    }


    function newList(oldMainDDlList, values, name) {
        var list = [];
        for (var i = 0; i < oldMainDDlList.length; i++) {
            if (values.length > 0) {
                for (var ii = 0; ii < values.length; ii++) {
                    if (oldMainDDlList[i][name] === values[ii].Value) {
                        list.push({
                            Id: oldMainDDlList[i].SystemId,
                            Department: oldMainDDlList[i].Department,
                            DepartmentId: oldMainDDlList[i].DepartmentId,
                            Division: oldMainDDlList[i].Division,
                            DivisionId: oldMainDDlList[i].DivisionId,
                            Section: oldMainDDlList[i].Section,
                            SectionId: oldMainDDlList[i].SectionId,
                            EmployeeCategory: oldMainDDlList[i].EmployeeCategory,
                            EmployeeCategoryId: oldMainDDlList[i].EmployeeCategoryId,
                            GivenDesignation: oldMainDDlList[i].GivenDesignation,
                            GivenDesignationId: oldMainDDlList[i].GivenDesignationId
                        });
                    }
                }
            }
            else {
                list.push({
                    Id: oldMainDDlList[i].SystemId,
                    Department: oldMainDDlList[i].Department,
                    DepartmentId: oldMainDDlList[i].DepartmentId,
                    Division: oldMainDDlList[i].Division,
                    DivisionId: oldMainDDlList[i].DivisionId,
                    Section: oldMainDDlList[i].Section,
                    SectionId: oldMainDDlList[i].SectionId,
                    EmployeeCategory: oldMainDDlList[i].EmployeeCategory,
                    EmployeeCategoryId: oldMainDDlList[i].EmployeeCategoryId,
                    GivenDesignation: oldMainDDlList[i].GivenDesignation,
                    GivenDesignationId: oldMainDDlList[i].GivenDesignationId
                });
            }
        }
        return list;
    }
    function ddlFilterByDDL(newlist, value, text) {
        var list = [];
        for (var i = 0; i < newlist.length; i++) {
            if (!ddlFilter(list, newlist[i][value])) {
                list.push({
                    Value: newlist[i][value],
                    Text: newlist[i][text]
                });
            }
        }
        return list.sort(function (a, b) {
            var nameA = a.Text.toUpperCase(); // ignore upper and lowercase
            var nameB = b.Text.toUpperCase(); // ignore upper and lowercase
            if (nameA < nameB) {
                return -1;
            }
            if (nameA > nameB) {
                return 1;
            }
            // names must be equal
            return 0;
        });
    }
    function getListForm(list) {
        $scope.departmentNewList = createCbo(list, 'DepartmentId', 'Department');
        $scope.divisionNewList = createCbo(list, 'DivisionId', 'Division');
        $scope.sectionNewList = createCbo(list, 'SectionId', 'Section');
        $scope.employeeCategoryNewList = createCbo(list, 'EmployeeCategoryId', 'EmployeeCategory');
        $scope.givenDesignationNewList = createCbo(list, 'GivenDesignationId', 'GivenDesignation');
    }
    function createCbo(dblist, value, text) {
        var list = [];
        for (var i = 0; i < dblist.length; i++) {
            if (!ddlFilter(list, dblist[i][value])) {
                list.push({
                    Text: dblist[i][text],
                    Value: dblist[i][value]
                });
            }
        }
        //Sorting with text A-Z
        //return list.sort(function (a, b) {
        //    var nameA = a.Text.toUpperCase(); // ignore upper and lowercase
        //    var nameB = b.Text.toUpperCase(); // ignore upper and lowercase
        //    if (nameA < nameB) {
        //        return -1;
        //    }
        //    if (nameA > nameB) {
        //        return 1;
        //    }

        //    // names must be equal
        //    return 0;
        //});
    }
    $scope.cboCratetor = function (val, name) {
        $scope.newList = [];
        $scope.newList = newList($scope.employeeList, val, name);
        if (name !== 'DepartmentId')
            $scope.departmentNewList = ddlFilterByDDL($scope.newList, 'DepartmentId', 'Department');
        if (name !== 'DivisionId')
            $scope.divisionNewList = ddlFilterByDDL($scope.newList, 'DivisionId', 'Division');
        if (name !== 'SectionId')
            $scope.sectionNewList = ddlFilterByDDL($scope.newList, 'SectionId', 'Section');
        if (name !== 'EmployeeCategoryId')
            $scope.employeeCategoryNewList = ddlFilterByDDL($scope.newList, 'EmployeeCategoryId', 'EmployeeCategory');
        if (name !== 'GivenDesignationId')
            $scope.givenDesignationNewList = ddlFilterByDDL($scope.newList, 'GivenDesignationId', 'GivenDesignation');
    };
    $scope.multiSelectSettings = {
        scrollableHeight: 'auto',
        smartButtonMaxItems: 3,
        scrollable: true,
        showCheckAll: false,
        showUncheckAll: false,
        enableSearch: true,
        dynamicTitle: true,
    };
    $scope.example3customTexts = { buttonDefaultText: 'Department' };
    $scope.example4customTexts = { buttonDefaultText: 'Division' };
    $scope.example5customTexts = { buttonDefaultText: 'Section' };
    $scope.example6customTexts = { buttonDefaultText: 'EmployeeCategory' };
    $scope.example7customTexts = { buttonDefaultText: 'GivenDesignation' };
    $scope.departmentIds = [];
    $scope.multi3events = {
        onItemSelect: function (item) {
            $scope.cboCratetor($scope.departmentIds, 'DepartmentId');
        }, onItemDeselect: function (item) {
            $scope.cboCratetor($scope.departmentIds, 'DepartmentId');
        }
    };
    $scope.divisionIds = [];
    $scope.multi4events = {
        onItemSelect: function (item) {
            $scope.cboCratetor($scope.divisionIds, 'DivisionId');
        }, onItemDeselect: function (item) {
            $scope.cboCratetor($scope.divisionIds, 'DivisionId');
        }
    };
    $scope.sectionIds = [];
    $scope.multi5events = {
        onItemSelect: function (item) {
            $scope.cboCratetor($scope.sectionIds, 'SectionId');
        }, onItemDeselect: function (item) {
            $scope.cboCratetor($scope.sectionIds, 'SectionId');
        }
    };
    $scope.employeeCategoryIds = [];
    $scope.multi6events = {
        onItemSelect: function (item) {
            $scope.cboCratetor($scope.employeeCategoryIds, 'EmployeeCategoryId');
        }, onItemDeselect: function (item) {
            $scope.cboCratetor($scope.employeeCategoryIds, 'EmployeeCategoryId');
        }
    };
    $scope.givenDesignationIds = [];
    $scope.multi7events = {
        onItemSelect: function (item) {
            $scope.cboCratetor($scope.givenDesignationIds, 'GivenDesignationId');
        }, onItemDeselect: function (item) {
            $scope.cboCratetor($scope.givenDesignationIds, 'GivenDesignationId');
        }
    };
    function IdList() {
        $scope.departmentIdstr = createIdList(validListWithStr($scope.departmentNewList, $scope.departmentIds));
        $scope.divisionIdstr = createIdList(validListWithStr($scope.divisionNewList, $scope.divisionIds));
        $scope.sectionIdstr = createIdList(validListWithStr($scope.sectionNewList, $scope.sectionIds));
        $scope.employeeCategoryIdstr = createIdList(validListWithStr($scope.employeeCategoryNewList, $scope.employeeCategoryIds));
        $scope.givenDesignationIdstr = createIdList(validListWithStr($scope.givenDesignationNewList, $scope.givenDesignationIds));
    }
    function createIdList(list) {
        var value = "''";
        for (var i = 0; i < list.length; i++) {

            if (value === "''") {
                value = "'" + list[i].Value + "'";
            } else {
                value += ",'" + list[i].Value + "'";
            }
        }
        return value;

    }
    function validListWithStr(list, values) {
        var tempValues = [];
        for (var i = 0; i < values.length; i++) {
            for (var j = 0; j < list.length; j++) {
                if (values[i].Value === list[j].Value) {
                    tempValues.push(values[i]);
                }
            }
        }
        return tempValues;
    }
    //#end region
    $scope.popUp2 = function (name) {
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
                },
                {
                    name: 'Given Designation',
                    value: 'GivenDesignation'
                },
                {
                    name: 'Department',
                    value: 'Department'
                }
            ];
            $scope.popUpUrl = '';
            $scope.employeeParameters.sort = '';
            $scope.employeeParameters.searchBy = '';
            $scope.popUpTitle = 'Employee';
            $scope.popUpUrl = 'employees/approvalconfiguration/GetEmployeeDataWithfilter?departmentIds=' + $scope.departmentIdstr + '&divisionIds=' + $scope.divisionIdstr + '&sectionIds=' + $scope.sectionIdstr + '&employeeCateogoryIds=' + $scope.employeeCategoryIdstr + '&givenDesignationIds=' + $scope.givenDesignationIdstr + '&employeeCode=' + $scope.paidHoursEmployeeAssignNew.EmployeeCode + '&employeeName=' + $scope.paidHoursEmployeeAssignNew.EmployeeName;
            $scope.employeeParameters.sort = 'EmployeeCode';
            $scope.employeeParameters.searchBy = 'EmployeeCode';
            $scope.getEmployeeData = function (pageno) {
                baseService.paginationBase($scope.popUpUrl, pageno, $scope.employeeParameters)
                    .then(function (result) {
                        $scope.employeeList = result.Rows;
                        $scope.employeeParameters.total_count = result.Total;
                        getListForm($scope.employeeList);
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
    $scope.getSearchData = function () {
        IdList();
        $scope.popUp2();
    };

    $scope.setData = function (data) {
        $scope.emp.Id = data.SystemId;
        $scope.emp.EmployeeCode = data.EmployeeCode;
        $scope.emp.EmployeeId = data.EmployeeId;
        $scope.emp.EmployeeName = data.EmployeeName;
        angular.element(document.querySelector('#employeePopUp')).modal('hide');
    }

    $scope.closeEmployeePopUp = function () {
        angular.element(document.querySelector('#employeePopUp')).modal('hide');
    };
   
    $scope.Clear = function () {
        ClearFields();

    }
    function ClearFields() {

        //$scope.selectedemployeeList = [];
        //$scope.selectedemployeeList = [];
        //employeeCodeStringList = [];
        //employeeIdStringList = [];
        //$scope.employeeIdString = [];
        //$scope.employeeCodeString = [];
    }
}