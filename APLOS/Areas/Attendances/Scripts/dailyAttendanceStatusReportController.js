'use strict';
dailyAttendanceStatusReportController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'cboService'];
function dailyAttendanceStatusReportController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, cboService) {
    $scope.path = 'humanresource/payrollReports/';
    $scope.employeeCategoryId = null;
    $scope.dailyComplianceReport = {
        WorkDate: null
    };
    $rootScope.title = 'Daily Attendance Status Report';
    $scope.exportgriddataUrl = 'GridReports/ExcelExport';
    $scope.downloadgriddataUrl = 'GridReports/Download';//DownloadUsingPath
    $scope.downloadgriddataUrlPath = 'GridReports/DownloadUsingFullPath';//DownloadUsingPath
    $scope.isActive = true;
    $scope.isSeperated = true;
    $scope.isMaternity = true;
    $scope.paymentDate = null;
    $scope.languageId = null;
    $scope.paymentMode = null;
    $scope.isManualFilter = false;
    var sqlInStatement = "";
    $scope.reportStatus = {
        status: "dayStatus"
    };
    $scope.effectiveDate = $filter('dateFiltering')(Date.now());
    var attdnDate = new Date($scope.effectiveDate);
    $scope.Prev = $filter('date')(new Date(attdnDate.setDate(attdnDate.getDate() - 1)), 'dd-MMM-yyyy');
    $scope.hrStatus = {
        pstatus: 'Default'
    };
    $scope.withStructure = null;
    $scope.sheetType = false;
    $scope.empGrid = false;

    $scope.month = "";
    $scope.year = "";
    $scope.isCompletedMonth = null;
    $scope.salaryProcessId = null;

    $scope.unitId = null;
    $scope.departmentId = null;
    $scope.divisionId = null;
    $scope.sectionId = null;
    $scope.subSenctionId = null;
    $scope.payGroupId = null;
    $scope.empGrid = false;
    $scope.localLanguageList = [];
    cboService.getLanguageIdCbo(function (result) {
        $scope.localLanguageList = result;
    });

    $scope.payGroupList = [];
    $scope.payGroupListSelected = [];

    cboService.getPayRollGroupCbo(function (result) {
        $scope.payGroupList = result;
    });

    $scope.getSalaryProcessIdList = function () {
        $scope.isCompletedMonth = 1;
        cboService.getSalaryProcessIdCboByYearMonth($scope.month, $scope.year, $scope.isCompletedMonth, function (result) {
            $scope.cboSalaryProcessIdList = result;
        });
    };
    //$scope.getSalaryProcessIdList();
    $scope.selectedPaymentMode = $("#paymentMode option:selected").text();
    $scope.selectedEmployeeCategory = $("#employeeCategoryId option:selected").text();
    $scope.payGroupListSelected = [];
    $scope.EmployeeList = [];
    $scope.EmployeeListDefault = [];
    $scope.EmployeeListTemp = [];
    $scope.shift = null;
    $scope.sDepID = null;
    $scope.sSecID = null;
    $scope.sSubSecID = null;
    $scope.initial = [];

    $scope.GetDailyAttendanceStatusReport = function () {
        try {
          
            var parameters = [];
            var gridObj = $("#InfoGrid").data("ejGrid");
            var filteredRecords = gridObj.getFilteredRecords();
            if (filteredRecords.length == 0) {
                filteredRecords = $scope.initial;
            }
            parameters.push({ "Key": "EntityId", "Value": getString(filteredRecords, "EntityId") });
            parameters.push({ "Key": "DepartmentId", "Value": getString(filteredRecords, "DepId") });
            parameters.push({ "Key": "SectionId", "Value": getString(filteredRecords, "SecId") });
            parameters.push({ "Key": "SubSectionId", "Value": getString(filteredRecords, "SubSecId") });

            var enttyList = parameters[0].Value;
            var departmentList = parameters[1].Value;
            var sectionList = parameters[2].Value;
            var subSectionList = parameters[3].Value;
            //console.log(enttyList);
            //console.log(departmentList);
            $scope.fileName = "DailyAttendanceStatus.xls";
            if (angular.isUndefinedOrNull($scope.effectiveDate)) {
                ShowResult("Select Work Date", 'failure');
            }

            if (baseService.isUndefinedOrNull($scope.shift)) {
                throw "Enter shift..";
            }
            $http({
                method: 'POST',
                url: 'Attendances/DailyAttendanceStatusReport/GetDailyAttendanceStatusReport',
                data: { 'workDate': $scope.effectiveDate, 'shift': $scope.shift, 'Entity': enttyList, 'Dept': departmentList, 'Ydate': $scope.Prev, 'Sec': sectionList, 'SSec': subSectionList }
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    //$rootScope.report($scope.downloadgriddataUrlPath + "?FileName=" + response.data.FileName);//downloadgriddataUrlPath
                    $rootScope.report($scope.downloadgriddataUrlPath + "?FullPath=" + response.data.FileName + "&fileName=" + $scope.fileName);//downloadgriddataUrlPath

                }
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };
    var getString = function (data, column) {
        var string = "''";
        var collection = [];
        for (var i = 0; i < data.length; i++) {
            if (collection.includes(data[i][column]) === false) {
                string += ",'" + data[i][column] + "'";
                collection.push(data[i][column]);
            }
        }
        return string;
    };

    //------Multiple Selection(Excel)-------//
    function checkChangeemployee(e) {

        var val = e.model.value;
        //item level check
        var row = $filter('filter')($scope.employeeAttendanceBySingleDateSelection, { 'Id': e.model.value });
        if (!baseService.isUndefinedOrNull(row) && row.length > 0) {
            if (e.model.checkState == "check")
                row[0].Active = true;
            else
                row[0].Active = false;
        }

    }
    function headCheckChangeemployee(e) {
        if (e.model.checkState == "check") {

            // var gridObj = $("#Gridemployee").data("ejGrid");
            var filtered = $("#Gridemployee").data("ejGrid").getFilteredRecords();
            if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
                for (var i = 0; i < $scope.EmployeeList.length; i++) {

                    $scope.EmployeeList[i].isSelect = true;
                }
            }
            else {
                for (var i = 0; i < $scope.EmployeeList.length; i++) {
                    for (var j = 0; j < filtered.length; j++) {
                        if ($scope.EmployeeList[i].EmpSystemId == filtered[j].EmpSystemId)
                            // $scope.EmployeeList[i].isSelect = true;
                            $scope.EmployeeList[i].isToBeSelect = true;
                    }

                }
            }

            var checkbox = $("#Gridemployee .rowCheckbox").ejCheckBox();
            for (var i = 0; i < checkbox.length; i++) {
                $($("#Gridemployee .rowCheckbox")[i]).ejCheckBox({ "change": null });
                $($("#Gridemployee .rowCheckbox")[i]).ejCheckBox({ "checked": true });
                $($("#Gridemployee .rowCheckbox")[i]).ejCheckBox({ "change": checkChangeemployee });
            }
        }
        else {
            var filtered = $("#Gridemployee").data("ejGrid").getFilteredRecords();
            if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
                for (var i = 0; i < $scope.EmployeeList.length; i++) {
                    $scope.EmployeeList[i].isToBeSelect = false;
                }
            }
            else {
                for (var i = 0; i < $scope.EmployeeList.length; i++) {
                    for (var j = 0; j < filtered.length; j++) {
                        if ($scope.EmployeeList[i].Id == filtered[j].Id)
                            $scope.EmployeeList[i].isToBeSelect = false;
                    }

                }
            }
            var checkbox = $("#Gridemployee .rowCheckbox").ejCheckBox();
            for (var i = 0; i < checkbox.length; i++) {
                $($("#Gridemployee .rowCheckbox")[i]).ejCheckBox({ "change": null });
                $($("#Gridemployee .rowCheckbox")[i]).ejCheckBox({ "checked": false });
                $($("#Gridemployee .rowCheckbox")[i]).ejCheckBox({ "change": checkChangeemployee });
            }
        }
        //header level check
    }
    $scope.dataBoundemployee = function (args) {
        $("#Gridemployee .rowCheckbox").ejCheckBox({ "change": checkChange });
        $("#headchk").ejCheckBox({ "change": headCheckChangeemployee });

    };
    $scope.refreshTemplateemployee = function (args) {
        if (args.rowIndex == 0) {
            $("#headchk").ejCheckBox({ "change": headCheckChangeemployee });
        }

        var valobj = $($("#Gridemployee .rowCheckbox")[args.rowIndex]).ejCheckBox()[0];
        var val = $($("#Gridemployee .rowCheckbox")[args.rowIndex]).ejCheckBox()[0].defaultValue;

        $($("#Gridemployee .rowCheckbox")[args.rowIndex]).ejCheckBox({ "change": null });
        var row = $filter('filter')($scope.EmployeeList, { 'EmpSystemId': val });
        if (!baseService.isUndefinedOrNull(row) && row.length > 0) {
            if (row[0].isToBeSelect == true)
                $($("#Gridemployee .rowCheckbox")[args.rowIndex]).ejCheckBox({ "checked": true });
            else
                $($("#Gridemployee .rowCheckbox")[args.rowIndex]).ejCheckBox({ "checked": false });

        }
        $($("#Gridemployee .rowCheckbox")[args.rowIndex]).ejCheckBox({ "change": checkChangeemployee });
    };
    $scope.saveemployeedata = function () {
        $scope.EmployeeListTemp = [];
        var row = $filter('filter')($scope.EmployeeList, { 'isToBeSelect': true });
        if (!baseService.isUndefinedOrNull(row) && row.length > 0) {
            $scope.EmployeeListTemp = row;
            $scope.isManualFilter = true;
        }
        $scope.Back();
    };
    $scope.showEmployeeFilterScreen = function () {
        try {

            var gridObj = $("#Gridemployee").data("ejGrid");
            gridObj.clearFiltering();
            angular.element(document.querySelector('#empfilterPopUp')).modal('show');


        } catch (e) {
            ShowResult(e, 'failure');
        }
    };
    $scope.clearManualFilter = function () {
        $scope.isManualFilter = false;
        $scope.EmployeeListTemp = $scope.EmployeeList;
    };
    $scope.Back = function () {
        angular.element(document.querySelector('#empfilterPopUp')).modal('hide');
    };
    //--------------------------------------//

    //#region -- Entity -- Department -- Shift

    $scope.EntityList = [];
    $scope.getEntity = function () {
        $http({
            method: 'GET',
            url: 'humanresource/dailydaystatus/GetEntityList',
        }).then(function successCallback(response) {
            $scope.EntityList = response.data;
        });
    }
    $scope.getEntity();


    $scope.departmentList = [];
    cboService.getCboDepartmentByCompanyGroup(null, function (result) {
        $scope.departmentList = result;
    });

    $scope.ShiftList = [];
    $scope.getShift = function () {
        $http({
            method: 'GET',
            url: 'humanresource/dailydaystatus/GetShift',
        }).then(function successCallback(response) {
            $scope.ShiftList = response.data;
        });
    }
    $scope.getShift();

    //$scope.changeSectionByDept = function () {
    //    $scope.sectionList = [];
    //    $scope.subSectionList = [];
    //    $scope.lineList = [];

    //    if (!baseService.isUndefinedOrNull($scope.sDepID) && $scope.sDepID !== 'All') {
    //        cboService.getSectionCboByDepartment($scope.sDepID, function (result) {
    //            $scope.sectionList = result;
    //        });
    //    } else {
    //        $scope.LoadSec();
    //        $scope.LoadSubSec();
    //        $scope.LoadLine();
    //    }
    //};

    $scope.sectionList = [];
    $scope.changeSectionByDept = function () {
        var DropDownList = $("#departmentList").data("ejDropDownList");
        var departmentList = DropDownList.getSelectedValue();
        $http({
            method: 'GET',
            url: 'humanresource/dailydaystatus/GetSection?DeptId=' + departmentList,
        }).then(function successCallback(response) {
            $scope.sectionList = response.data;
        });
    }
    //$scope.getEntity();

    //$scope.changeSubSectionBySection = function () {
    //    cboService.getSubSectionCboBySection($scope.sSecID, function (result) {
    //        $scope.subSectionList = result;
    //    });
    //};

    $scope.subSectionList = [];
    $scope.changeSubSectionBySection = function () {
        var DropDownListOb = $("#sectionList").data("ejDropDownList");
        var sectionList = DropDownListOb.getSelectedValue();
        $http({
            method: 'GET',
            url: 'humanresource/dailydaystatus/GetSubSection?SecId=' + sectionList,
        }).then(function successCallback(response) {
            $scope.subSectionList = response.data;
        });
    }

    //#endregion



    //The Grid For the Data
    $http({
        method: 'GET',
        url: 'Attendances/DailyAttendanceStatusReport/GetGrid'
    }).then(function successCallback(response) {
        $scope.initial = response.data;
        var ColumnList = [
            { field: 'Entity', width: 100, headerText: "Entity", type: "string" },
            { field: 'Department', width: 100, headerText: "Department", type: "string" },
            { field: 'Section', width: 100, headerText: "Section", type: "string" },
            { field: 'SubSection', width: 100, headerText: "Sub Section", type: "string" }

        ];
        $("#InfoGrid").ejGrid({
            dataSource: $scope.initial,
            maxWidth: 450, minHeight: 400,
            allowFiltering: true, allowPaging: false, enableTouch: true,
            responsive: true, allowSelection: true, allowTextWrap: true, allowScrolling: true, 
            filterSettings: { filterType: "excel" },
            columns: ColumnList
        });

        var gridObj = $("#InfoGrid").data("ejGrid");
        gridObj.refreshContent(true);
        gridObj.refreshTemplate();
    });
}


/*Select distinct isnull(E.UserName, '') as Entity, dep.UserName as Department, sec.UserName as Section, ssec.UserName as SubSection
from org.Position p
left join mst.ManpowerBudget mpb on mpb.PositionId = p.Id
left join org.Entity e on e.Id = mpb.EntityId
left join org.Section sec on sec.id = p.SectionId
left join org.SubSection ssec on ssec.Id = p.SubSectionId
left join org.Department dep on dep.Id = p.DepartmentId*/