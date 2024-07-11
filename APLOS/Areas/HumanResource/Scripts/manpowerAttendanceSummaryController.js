'use strict';
manpowerAttendanceSummaryController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'toaster', 'cboService', '$window'];
function manpowerAttendanceSummaryController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, toaster, cboService, $window) {

    $scope.path = 'humanresource/PayRegisterBDReport/';

    $scope.dailyComplianceReport = {
        WorkDate: $filter('dateFiltering')(Date.now())
    };

    $scope.reportStatus = {
        status: "dayStatus"
    };
    $scope.WithoutTBS = true;
    $scope.WithoutLA = true;

    $scope.hrStatus = {
        pstatus: 'Default'
    };
    $scope.month = null;
    $scope.year = null;
    $scope.isCompletedMonth = null;
    $scope.salaryProcessId = null;
    $scope.withLine = false;
    $scope.withDesignation = false;

    $scope.unitId = null;
    $scope.departmentId = null;
    $scope.divisionId = null;
    $scope.sectionId = null;
    $scope.subSenctionId = null;
    $scope.payGroupId = null;

    var newdate = new Date();
    $scope.attdnDate = $filter('dateFiltering')(newdate.setDate(newdate.getDate() - 1));

    $scope.monthList = [
        {
            Value: 1,
            Text: 'January'
        },
        {
            Value: 2,
            Text: 'February'
        },
        {
            Value: 3,
            Text: 'March'
        },
        {
            Value: 4,
            Text: 'April'
        },
        {
            Value: 5,
            Text: 'May'
        },
        {
            Value: 6,
            Text: 'June'
        },
        {
            Value: 7,
            Text: 'July'
        },
        {
            Value: 8,
            Text: 'August'
        },
        {
            Value: 9,
            Text: 'September'
        },
        {
            Value: 10,
            Text: 'October'
        },
        {
            Value: 11,
            Text: 'November'
        },
        {
            Value: 12,
            Text: 'December'
        }
    ];

    $scope.yearList = [];
    cboService.getCboLeaveYear(function (result) {
        $scope.yearList = result;
    });
    $scope.unitList = [];
    cboService.getCboUnit(function (result) {
        $scope.unitList = result;
    });

    $scope.divisionList = [];
    cboService.getCboDivisionByCompanyGroup(null, function (result) {
        $scope.divisionList = result;
    });

    $scope.departmentList = [];
    cboService.getCboDepartmentByCompanyGroup(null, function (result) {
        $scope.departmentList = result;
    });

    $scope.subSectionList = [];
    cboService.getCboSubSectionByCompanyGroup(null, function (result) {
        $scope.subSectionList = result;
    });

    $scope.employeeCategoryList = [];
    cboService.getCboEmployeeCategoryGroupByCompanyGroup(null, function (result) {
        $scope.employeeCategoryList = result;
    });

    $scope.designationGroupList = [];
    cboService.getCboDesignationGroupByCompanyGroup(null, function (result) {
        $scope.designationGroupList = result;
    });

    $scope.sectionList = [];
    cboService.getCboSectionByCompanyGroup(null, function (result) {
        $scope.sectionList = result;
    });

    $scope.lineList = [];
    cboService.getCboLineByCompany(null, function (result) {
        $scope.lineList = result;
    });

    $scope.designationList = [];
    cboService.getCboDesignationByCompanyGroup(null, function (result) {
        $scope.designationList = result;
    });

    $scope.payGroupList = [];
    cboService.getPayGroupCbo(function (result) {
        $scope.payGroupList = result;
    });


    $scope.getSalaryProcessIdList = function () {
        $scope.isCompletedMonth = 1;
        cboService.getSalaryProcessIdCboByYearMonth($scope.month, $scope.year, $scope.isCompletedMonth, function (result) {
            $scope.cboSalaryProcessIdList = result;
        });
    };

    //cboService.get(function (result) {
    //    $scope.isCompletedMonth = 1;
    //    $scope.getPlant = function () {
    //        cboService.getSalaryProcessIdCboByYearMonth($scope.month, $scope.year, $scope.isCompletedMonth, function (result) {
    //            $scope.cboSalaryProcessIdList = result;
    //        });
    //    };
    //});

    $scope.GetCustomizedAttendanceSummaryReport = function () {
        try {

            //string divisionId, string unitId, string sectionId, string subSectionId, string departmentId, string payGroupId
            $scope.parameters = 'workDate=' + $scope.attdnDate;
            location.href = 'humanresource/ManpowerAttendanceSummary/GetCustomizedAttendanceSummaryReport?' + $scope.parameters;
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };//GetDailyAttendanceSummary



    $scope.GetmanpowerAttendanceSummaryrReport1 = function () {
        try {

            var DropDownListObj = $("#CWPlant").data("ejDropDownList");
            var PlantId = DropDownListObj.getSelectedValue();

            var DropDownListObj = $("#typeList").data("ejDropDownList");
            var typeList = DropDownListObj.getSelectedValue();

            //string divisionId, string unitId, string sectionId, string subSectionId, string departmentId, string payGroupId
            $scope.parameters = 'workDate=' + $scope.attdnDate + '&withLine=' + $scope.withLine + '&withDesignation=' + $scope.withDesignation + '&PlantId=' + PlantId + '&typeList=' + typeList + '&WithoutTBS=' + $scope.WithoutTBS + '&WithoutLA=' + $scope.WithoutLA;
            location.href = 'humanresource/ManpowerAttendanceSummary/GetmanpowerAttendanceSummaryrReportOld?' + $scope.parameters;
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };//GetDailyAttendanceSummary

    $scope.DataList = [];
    $scope.GetmanpowerAttendanceSummaryData = function () {
        var DropDownListObj = $("#CWPlant").data("ejDropDownList");
        var PlantId = DropDownListObj.getSelectedValue();

        var DropDownListObj = $("#typeList").data("ejDropDownList");
        var typeList = DropDownListObj.getSelectedValue();

        //string divisionId, string unitId, string sectionId, string subSectionId, string departmentId, string payGroupId
        $scope.parameters = 'workDate=' + $scope.attdnDate + '&withLine=' + $scope.withLine + '&withDesignation=' + $scope.withDesignation + '&PlantId=' + PlantId + '&typeList=' + typeList + '&WithoutTBS=' + $scope.WithoutTBS + '&WithoutLA=' + $scope.WithoutLA;

        $http({
            method: 'POST',
            url: 'humanresource/ManpowerAttendanceSummary/GetDailyManpowerAttendanceSummaryData?' + $scope.parameters,
        }).then(function successCallback(response) {
            $scope.DataList = response.data;
        });
    }





    $scope.downloadgriddataUrlPath = 'GridReports/DownloadUsingFullPath';//DownloadUsingPath
    $scope.GetmanpowerAttendanceSummaryrReport = function () {
        try {
          
            var dataList = [];
            var g = $("#GridMAD").data("ejGrid");
            dataList = g.getFilteredRecords();

            if (dataList.length == 0) {
                dataList = $scope.DataList;
            }

            if (dataList.length == 0) {
                throw "First click on View button.";
            }


            $scope.fileName = "ManpowerAttendanceSummaryReport.xlsx";
            $http({
                method: 'POST',
                url: 'humanresource/ManpowerAttendanceSummary/GetSummaryManpowerAttendanceExcelReport',
                data: { 'workDate': $scope.attdnDate, 'withLine' : $scope.withLine , 'withDesignation' : $scope.withDesignation, 'data': dataList },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == false) {
                    $rootScope.report($scope.downloadgriddataUrlPath + "?FullPath=" + response.data.FileName + "&fileName=" + $scope.fileName);//downloadgriddataUrlPath

                }
                else {
                    ShowResult(response.data.Message, 'failure');
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }


    $scope.XGetmanpowerAttendanceSummaryrReport = function () {
        try {
            $scope.parameters = 'workDate=' + $scope.attdnDate;
            location.href = 'humanresource/ManpowerAttendanceSummary/GetDailyAttendanceSummary?' + $scope.parameters;
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.GetAttendancFromAppeReport = function () {
        try {

            //string divisionId, string unitId, string sectionId, string subSectionId, string departmentId, string payGroupId
            $scope.parameters = 'workDate=' + $scope.attdnDate + '&sUnitID=' + $scope.unitId + '&sDivID=' + $scope.sDivID + '&sDepID=' + $scope.sDepID + '&sSecID=' + $scope.sSecID + '&sSubSecID=' + $scope.sSubSecID;
            location.href = 'humanresource/ManpowerAttendanceSummary/GetAttendancFromAppReport?' + $scope.parameters;
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.GetMonthlyCompliedAttendaceReport = function () {
        location.href = 'humanresource/compliedshiftassignment/GetMonthlyCompliedAttendaceReport?month=' + $scope.month + 'year=' + $scope.year + 'reportStatus' + $scope.reportStatus.status;
    };
    //


    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.yearList = [];
    cboService.getCboLeaveYear(function (result) {
        $scope.yearList = result;
    });

    $scope.GetEmployeeJobCardReport = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.EmpJobCardReport.FromDate)) {
                throw "Select From Date.";
            }
            if (baseService.isUndefinedOrNull($scope.EmpJobCardReport.ToDate)) {
                throw "Select To Date.";
            }
            if (baseService.isUndefinedOrNull($scope.EmpJobCardReport.employeeCodeString)) {
                throw "Select Employee Code.";
            }
            location.href = 'humanresource/compliedshiftassignment/GetEmployeeJobCardReport?fromDate=' + $scope.EmpJobCardReport.FromDate + '&toDate=' + $scope.EmpJobCardReport.ToDate + '&emp=' + $scope.EmpIdPass;
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };
    $scope.GetMonthlyShiftReport = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.monthlyShiftReport.YearId)) {
                throw "Select Year.";
            }
            if (baseService.isUndefinedOrNull($scope.monthlyShiftReport.MonthId)) {
                throw "Select Month.";
            }
            location.href = 'humanresource/compliedshiftassignment/GetMonthlyShiftReport?yearId=' + $scope.monthlyShiftReport.YearId + '&monthId=' + $scope.monthlyShiftReport.MonthId;
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.GetDailyAttdnReportMonthy = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.monthlyShiftReport.YearId)) {
                throw "Select Year.";
            }
            if (baseService.isUndefinedOrNull($scope.monthlyShiftReport.MonthId)) {
                throw "Select Month.";
            }
            location.href = 'humanresource/compliedshiftassignment/GetDailyAttdnReportMonthy?yearId=' + $scope.monthlyShiftReport.YearId + '&monthId=' + $scope.monthlyShiftReport.MonthId;
        } catch (e) {
            ShowResult(e, 'failure');
        }
        $scope.monthlyShiftReport.YearId = null;
        $scope.monthlyShiftReport.MonthId = null;
    };
    $scope.selectedemployeeList = [];
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
    };
    $scope.ShortLeavePopUp = function () {
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
    };
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
    $scope.closeEmployeePopUp = function () {
        angular.forEach($scope.employeeList, function (item) {
            if (item.Flag) {
                if (!checkExisting(item.SystemId)) {
                    var ob = {};
                    ob.Id = null;
                    ob.EmployeeId = item.SystemId;
                    ob.EmployeeName = item.EmployeeName;
                    ob.EmployeeCode = item.EmployeeCode;
                    ob.GivenDesignation = item.GivenDesignation;
                    ob.Department = item.Department;
                    ob.Division = item.Division;
                    ob.Section = item.Section;
                    ob.EmployeeCategory = item.EmployeeCategory;
                    $scope.selectedemployeeList.push(ob);
                }
            }

        });

        getStringArrayJoin();
        angular.element(document.querySelector('#employeePopUp')).modal('hide');
    };
    var employeeCodeStringList = [];
    var employeeIdStringList = [];
    $scope.employeeCodeString = null;

    function getStringArrayJoin() {
        angular.forEach($scope.selectedemployeeList, function (item) {
            if (!checkExistingCode(item.EmployeeCode)) {
                employeeCodeStringList.push(item.EmployeeCode);
            }
            if (!checkExistingEmpId(item.SystemId)) {
                employeeIdStringList.push(item.EmployeeId);
            }
        });
        $scope.EmpJobCardReport.employeeCodeString = employeeCodeStringList.join();
        EmpcodePassList(employeeCodeStringList);

        $scope.employeeIdString = employeeIdStringList.join();
        EmpIdPassList(employeeIdStringList);
    }
    function checkExistingCode(code) {
        for (var i = 0; i < employeeCodeStringList.length; i++) {
            var ob = employeeCodeStringList[i];
            if (ob === code) {
                return true;
                break;
            }
        }
        return false;
    }
    function EmpcodePassList(list) {
        $scope.EmpcodePass = "''";
        for (var i = 0; i < list.length; i++) {

            if ($scope.EmpcodePass === "''") {
                $scope.EmpcodePass = "'" + list[i] + "'";
            } else {
                $scope.EmpcodePass += ",'" + list[i] + "'";
            }
        }
        return $scope.EmpcodePass;
    }
    function checkExistingEmpId(id) {
        for (var i = 0; i < employeeIdStringList.length; i++) {
            var ob = employeeIdStringList[i];
            if (ob === id) {
                return true;

            }
        }
        return false;
    }
    function EmpIdPassList(list) {
        $scope.EmpIdPass = "''";
        for (var i = 0; i < list.length; i++) {

            if ($scope.EmpIdPass === "''") {
                $scope.EmpIdPass = "'" + list[i] + "'";
            } else {
                $scope.EmpIdPass += ",'" + list[i] + "'";
            }
        }
        return $scope.EmpIdPass;

    }
    function checkExisting(id) {
        for (var i = 0; i < $scope.selectedemployeeList.length; i++) {
            var ob = $scope.selectedemployeeList[i];
            if (ob.EmployeeId === id) {
                return true;

            }
        }
        return false;
    }
    $scope.Clear = function () {
        ClearFields();
    };
    function ClearFields() {

        $scope.selectedemployeeList = [];
        employeeCodeStringList = [];
        employeeIdStringList = [];
        $scope.employeeIdString = [];
        $scope.employeeCodeString = [];
        $scope.EmpcodePass = [];
        $scope.EmpIdPass = [];

    }

    $scope.PlantIdFromUI = null;
    $scope.PlantList = [];
    $scope.getPlant = function () {
        $http({
            method: 'GET',
            url: "humanresource/payrollReports/GetPlantList",
        }).then(function successCallback(response) {
            $scope.PlantList = response.data;
            var index = 0;
            for (var i = 0; i < $scope.PlantList.length; i++) {
                if ($scope.PlantList[i].PlantId == $window.plantId) {
                    index = i;
                }
            }

            $('#CWPlant').ejDropDownList(
                {
                    dataSource: $scope.PlantList,
                    fields: { text: "PlantName", value: "PlantId" },
                    selectedIndex: index, showCheckBox: true, multiSelectMode: ej.MultiSelectMode.VisualMode
                    , width: 250
                });

        });
    }
    $scope.getPlant();

    $scope.EmployeeCodeTypeList = [];
    $scope.EmployeeCodeTypeCbo = function () {
        $http.get('employees/EmployeeCodeType/GetCbo')
            .then(function (response) {
                $scope.EmployeeCodeTypeList = response.data;

                //var index = 0;
                //for (var i = 0; i < $scope.EmployeeCodeTypeList.length; i++) {
                //    if ($scope.EmployeeCodeTypeList[i].Value == $window.plantId) {
                //        index = i;
                //    }
                //}

                $('#typeList').ejDropDownList(
                    {
                        dataSource: $scope.EmployeeCodeTypeList,
                        fields: { text: "Text", value: "Value" },
                        selectedIndex: 0, showCheckBox: true, multiSelectMode: ej.MultiSelectMode.VisualMode
                        , width: 250
                    });

            });
    }
    $scope.EmployeeCodeTypeCbo();

}