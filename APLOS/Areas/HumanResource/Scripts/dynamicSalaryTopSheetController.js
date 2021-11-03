'use strict';
dynamicSalaryTopSheetController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function dynamicSalaryTopSheetController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $scope.path = 'humanresource/SalaryTopSheet/';
    $scope.employeeCategoryId = null;
    $scope.dailyComplianceReport = {
        WorkDate: null
    };
    $scope.paymentDate = null;
    $scope.languageId = null;
    $scope.paymentMode = null;
    $scope.employeeStatusId = null;
    $scope.groupBy = "DepartmentEmployeeCategory";

    $scope.SalaryTopSheetCategory = 'PayrollGroup';


    $scope.create = function (args) {
        $("#checkBox").ejCheckBox({
            change: function (argss) {
                var obj = $("#ddlEmpStatusList").ejDropDownList("instance");
                if (argss.isChecked) obj.checkAll();
                else obj.uncheckAll();
            },
            text: "Select All",
            cssClass: "ddlSelectAllCheckBox"
        });

        $("#checkBoxEmpCategory").ejCheckBox({
            change: function (argss) {
                var obj = $("#ddlEmpCatgList").ejDropDownList("instance");
                if (argss.isChecked) obj.checkAll();
                else obj.uncheckAll();
            },
            text: "Select All",
            cssClass: "ddlSelectAllCheckBox"
        });
    };

    $scope.createEmpCategory = function (args) {
        $("#checkBoxEmpCategory").ejCheckBox({
            change: function (args) {
                var obj = $("#ddlEmpCatgList").ejDropDownList("instance");
                if (args.isChecked) obj.checkAll();
                else obj.uncheckAll();
            },
            text: "Select All",
            cssClass: "ddlSelectAllCheckBox"
        });
    };
    $scope.employeeCategoryList = [];
    cboService.getCboEmployeeCategoryGroupByCompanyGroup(null, function (result) {

        $scope.employeeCategoryList = result;     

    });

    $scope.employeeStatusMLVList = [];
    cboService.getEmployeeStatusWithMLVCbo(function (result) {
        $scope.employeeStatusMLVList = result;
        //$scope.employeeStatusMLVList = $scope.employeeStatusMLVList.concat([{
        //    Check: false,
        //    Value: "",
        //    Text: "All",
        //    Desc: null
        //}]);
    });

    //$scope.month = null;
    //$scope.year = null;
    $scope.isCompletedMonth = null;
    $scope.salaryProcessId = null;   

    $scope.month = null;
    $scope.year = null;

    $scope.unitId = null;
    $scope.departmentId = null;
    $scope.divisionId = null;
    $scope.sectionId = null;
    $scope.subSenctionId = null;
    $scope.payGroupId = null;

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
        var x = new Date();
        x.setDate(10);
        x.setMonth(x.getMonth() - 1);
        for (var i = 0; i < $scope.yearList.length; i++) {
            if ($scope.yearList[i].Text === x.getFullYear().toString())
            {
                $scope.year = $scope.yearList[i].Text;
                $scope.month = (x.getMonth()+1).toString();

            }
        }
    });
    $scope.GetSalaryTopRegistrar = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.month)) {
                throw "Select Month.";
            }
            if (baseService.isUndefinedOrNull($scope.year)) {
                throw "Select Year.";
            }
            $scope.parameters = 'month=' + $scope.month + '&year=' + $scope.year + '&salaryProcessId=' + $scope.salaryProcessId + '&divisionId=' + $scope.divisionId + '&unitId=' + $scope.unitId + '&sectionId=' + $scope.sectionId + '&subSectionId=' + $scope.subSectionId + '&departmentId=' + $scope.departmentId + '&payGroupId=' + $scope.payGroupId + '&employeeCategoryId=' + $scope.employeeCategoryId + '&paymentDate=' + $scope.paymentDate + '&paymentMode=' + $scope.paymentMode + '&languageId=' + $scope.languageId + '&SalaryTopSheetCategory=' + $scope.SalaryTopSheetCategory;
            //location.href = 'humanresource/SalaryTopSheet/XlsSalaryTopSheet?' + $scope.parameters;
            location.href = 'humanresource/SalaryTopSheet/GetSalaryTopSheet?' + $scope.parameters;
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };
    $scope.GetSalaryTopSheetDetails = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.month)) {
                throw "Select Month.";
            }
            if (baseService.isUndefinedOrNull($scope.year)) {
                throw "Select Year.";
            }

            $scope.parameters = 'month=' + $scope.month + '&year=' + $scope.year + '&salaryProcessId=' + $scope.salaryProcessId + '&divisionId=' + $scope.divisionId + '&unitId=' + $scope.unitId + '&sectionId=' + $scope.sectionId + '&subSectionId=' + $scope.subSectionId + '&departmentId=' + $scope.departmentId + '&payGroupId=' + $scope.payGroupId + '&employeeCategoryId=' + $scope.employeeCategoryId + '&paymentDate=' + $scope.paymentDate + '&paymentMode=' + $scope.paymentMode + '&languageId=' + $scope.languageId + '&SalaryTopSheetCategory=' + $scope.SalaryTopSheetCategory;
            location.href = 'humanresource/SalaryTopSheet/XlsSalaryTopSheet?' + $scope.parameters;
            // location.href = 'humanresource/SalaryTopSheet/GetSalaryTopSheet?' + $scope.parameters;
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.GetSalaryTopSheet = function () {
        try {

            var DropDownListObj = $("#ddlEmpStatusList").data("ejDropDownList");
            var empStatusList = DropDownListObj.getSelectedValue();
            var DropDownEmpCatgListObj = $("#ddlEmpCatgList").data("ejDropDownList");
            var empCatgoryList = DropDownEmpCatgListObj.getSelectedValue();

            if (empStatusList.length == 0)
                throw "Select Employee Status(s)";
            if (empCatgoryList.length == 0)
                throw "Select Employee Category(s)";

            $scope.filteredEmployeeStatus = $scope.employeeStatusMLVList;
            if (baseService.isUndefinedOrNull($scope.month)) {
                throw "Select Month.";
            }
            if (baseService.isUndefinedOrNull($scope.year)) {
                throw "Select Year.";
            }
            if (baseService.isUndefinedOrNull($scope.employeeStatusId) == false) {
                $scope.filteredEmployeeStatus = $filter('filter')($scope.employeeStatusMLVList, { 'Value': $scope.employeeStatusId });
            }

            var wcDocImportance = Array.prototype.map.call($scope.filteredEmployeeStatus, function (item) { return item.Value; });

            $scope.parameters = 'month=' + $scope.month + '&year=' + $scope.year + '&salaryProcessId=' + $scope.salaryProcessId + '&employeeCategoryId=' + empCatgoryList + '&groupBy=' + $scope.groupBy + '&employeeStatusMLVId=' + $scope.employeeStatusMLVId + '&employeeStatus=' + empStatusList;
            location.href = 'humanresource/SalaryTopSheet/XlsSalaryTopLailaSheet?' + $scope.parameters;

        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

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



    $scope.SortingParametersList = [
        //{
        //    parameter: "Department",
        //    type: "Sorting"
        //},
        //{
        //    parameter: "Section",
        //    type: "Sorting"
        //},
        //{
        //    parameter: "SubSection",
        //    type: "Sorting"
        //},
        //{
        //    parameter: "Designation",
        //    type: "Sorting"
        //},
        {
            parameter: "DepartmentEmployeeCategory",
            type: "Sorting"
        }
        //,{
        //    parameter: "DepartmentSubSctionEmployeeCatagory",
        //    type: "Sorting"
        //}
    ];


}