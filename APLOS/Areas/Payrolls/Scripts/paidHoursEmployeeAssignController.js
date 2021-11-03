'use strict';
paidHoursEmployeeAssignController.$inject = ['commonMessage', '$controller', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$window', 'cboService'];
function paidHoursEmployeeAssignController(commonMessage, $controller, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window, cboService) {
    $rootScope.title = "Payroll Group Master";
    $scope.Action = 'Save';
    $scope.paidHoursEmployeeAssigns = [];
    $scope.path = 'Payrolls/PaidHoursEmployeeAssign/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';
    $scope.deleteUrl = $scope.path + 'delete/';
    $scope.partyType = 'Customer';
    $controller('partyBaseController', { $scope: $scope, $http: $http });

    $scope.paidHoursEmployeeAssign = {
        CompanyGroupId: $window.companyGroupId,
        PlantId: $window.plantId,
        PaidHours: null,
        EmployeeId: null,
        EmployeeCategoryId: null,
        EmployeeCategory: null,
        EmployeeCode: "",
        EmployeeName:""
    };
    $scope.paidHoursEmployeeAssignNew = Object.assign({}, $scope.paidHoursEmployeeAssign);
    $scope.paidHoursList = [
        {
            Text:'8',
            Value:'8'
        },
        {
            Text: '9',
            Value: '9'
        },
        {
            Text: '10',
            Value: '10'
        },
        {
            Text: '11',
            Value: '11'
        },
        {
            Text: '12',
            Value: '12'
        }
    ];
    
    //#region Employee
    //#region Payroll Group
    $scope.getSavedPayRollGroupData = function () {
        if (!baseService.isUndefinedOrNull($scope.paidHoursEmployeeAssignNew.PaidHours)) {
            $http.get("Payrolls/PaidHoursEmployeeAssign/GetList?paidHours=" + $scope.paidHoursEmployeeAssignNew.PaidHours)
                .then(
                function successCallback(response) {
                    $scope.paidHoursEmployeeAssigns = response.data.Rows;
                    $scope.paidHoursSavedDataCount = response.data.Total;
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
        }
    };
    //#end region
    $scope.employeeList = [];
    $scope.popUp = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.paidHoursEmployeeAssignNew.PaidHours)) {
                throw 'Select hours!';
            }
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
            $scope.popUpUrl = 'employees/approvalconfiguration/GetEmployeeWithoutPaidhoursData';
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
    $scope.popUp2 = function (name) {
        try {
            if (baseService.isUndefinedOrNull($scope.paidHoursEmployeeAssignNew.PaidHours)) {
                throw 'Select paid hours !';
            }
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
            $scope.popUpUrl = 'employees/approvalconfiguration/GetEmployeeDataWithPaidHoursIds?departmentIds=' + $scope.departmentIdstr + '&divisionIds=' + $scope.divisionIdstr + '&sectionIds=' + $scope.sectionIdstr + '&employeeCateogoryIds=' + $scope.employeeCategoryIdstr + '&givenDesignationIds=' + $scope.givenDesignationIdstr + '&employeeCode=' + $scope.paidHoursEmployeeAssignNew.EmployeeCode + '&employeeName=' + $scope.paidHoursEmployeeAssignNew.EmployeeName;
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
    $scope.closeEmployeePopUp = function () {
        angular.forEach($scope.employeeList, function (item) {
            if (item.Flag) {
                if (!checkExisting(item.SystemId)) {
                    var ob = {};
                    ob.Id = null;
                    ob.PaidHours = $scope.paidHoursEmployeeAssignNew.PaidHours;
                    ob.CompanyGroupId = $scope.paidHoursEmployeeAssignNew.CompanyGroupId;
                    ob.PlantId = $scope.paidHoursEmployeeAssignNew.PlantId;
                    ob.EmployeeId = item.SystemId;
                    ob.EmployeeName = item.EmployeeName;
                    ob.EmployeeCode = item.EmployeeCode;
                    ob.GivenDesignation = item.GivenDesignation;
                    ob.Department = item.Department;
                    ob.Division = item.Division;
                    ob.Section = item.Section;
                    ob.EmployeeCategory = item.EmployeeCategory;
                    $scope.paidHoursEmployeeAssigns.push(ob);
                    $scope.paidHoursSavedDataCount++;
                }
            }
        });
        angular.element(document.querySelector('#employeePopUp')).modal('hide');
    };
    function checkExisting(id) {
        for (var i = 0; i < $scope.paidHoursEmployeeAssigns.length; i++) {
            var ob = $scope.paidHoursEmployeeAssigns[i];
            if (ob.EmployeeId === id) {
                return true;
                break;
            }
        }
        return false;
    }

    //#end region

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
    $scope.getSearchData = function () {
        IdList();
        $scope.popUp2();
    };
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

    //#region shipToBillto
    $scope.billShippAddress = function (id, flag) {
        if (!baseService.isUndefinedOrNull(id)) {
            var address = $.grep($scope.partyPlantList, function (item) { return item.Value === id; })[0].Address1;
            var state = $.grep($scope.partyPlantList, function (item) { return item.Value === id; })[0].StateName;
            if (flag === 'billTo') {
                $scope.sampleOrderNew.InvoicingState = state;
                return $scope.sampleOrderNew.InvoicingByAddress = address;
            }
            else if (flag === 'shipTo') {
                $scope.sampleOrderNew.DeliveryState = state;
                return $scope.sampleOrderNew.DeliveryByAddress = address;
            }
        }
        else {
            if (flag === 'billTo') {
                $scope.sampleOrderNew.InvoicingState = null;
                return $scope.sampleOrderNew.InvoicingByAddress = null;
            }
            else if (flag === 'shipTo') {
                $scope.sampleOrderNew.DeliveryState = null;
                return $scope.sampleOrderNew.DeliveryByAddress = null;
            }
        }
    };

    //#end region

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.paidHoursEmployeeAssignNewForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: $scope.paidHoursEmployeeAssigns,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.getSavedPayRollGroupData();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };
        }
    };
    //Deleting Rows from RetentionAllowanceList
    $scope.valuePassInDelModal = function (index, data) {
        $scope.tempEmpOb = data;
        $scope.empIndex = index;
        if (baseService.isUndefinedOrNull($scope.tempEmpOb.Id))
            $scope.message_confirmation = 'Are you sure want to delete <b> [ ' + data.EmployeeCode + ' - ' + data.EmployeeName + ']</b>';
        else
            $scope.message_confirmation = 'Are you sure want to parmenently delete <b> [ ' + data.EmployeeCode + ' - ' + data.EmployeeName +']</b>';
        angular.element(document.querySelector('#confirm_PopUp')).modal('show');
    };
    $scope.removeRow = function () {
        if (baseService.isUndefinedOrNull($scope.tempEmpOb.Id) === true) {
            $scope.paidHoursEmployeeAssigns.splice($scope.empIndex, 1);
            $scope.paidHoursSavedDataCount--;
            $scope.empIndex = -1;
            $scope.tempEmpOb.Id = null;
        } else {
            $scope.removeFromDb($scope.tempEmpOb.Id, $scope.empIndex);
        }

        angular.element(document.querySelector('#confirm_PopUp')).modal('hide');
    };
    $scope.removeFromDb = function (id, index) {
        try {
            $http({
                method: 'POST',
                url: 'Payrolls/PaidHoursEmployeeAssign/Delete',
                dataType: 'JSON',
                data: { 'id': id }
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.paidHoursEmployeeAssigns.splice($scope.empIndex, 1);
                    $scope.paidHoursSavedDataCount--;
                    $scope.empIndex = -1;
                    $scope.tempEmpOb.Id = null;
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, 'failure');
            });
            return true;
        } catch (e) {
            ShowResult(e, 'Error');
        }
    };
    $scope.Clear = function () {
        ClearFields();
    }
    function ClearFields(seq) {
        $scope.paidHoursEmployeeAssign = {};
        $scope.paidHoursEmployeeAssignNew = {};
        $scope.paidHoursEmployeeAssignHeadList = [];
        $scope.popUpList = [];
        $scope.valueData = [];
    }
}
