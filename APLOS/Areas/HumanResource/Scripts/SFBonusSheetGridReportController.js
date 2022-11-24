'use strict';
SFBonusSheetGridReportController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'toaster', 'cboService'];
function SFBonusSheetGridReportController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, toaster, cboService) {

    $scope.path = 'humanresource/PayRegisterBDReport/';
    $scope.employeeCategoryId = null;
    $scope.dailyComplianceReport = {
        WorkDate: null
    };
    $scope.exportgriddataUrl = 'GridReports/ExcelExport';
    $scope.downloadgriddataUrl = 'GridReports/Download';
    $scope.paymentDate = null;
    $scope.reportHeader = null;
    $scope.isManualFilter = false;
    $scope.isStampDeductApplicable = false;
    $scope.gridShow = false;
    $scope.docGrouping = null;

    $scope.reportStatus = {
        status: "dayStatus"
    };

    $scope.PeraModel = {
        PayRollGroupId: null
        , BonusPointId: null
        , BonuseffectiveDate: null
    };

   
    $scope.BonusPointEffectiveDate = [];

    $scope.GetBonusEffectiveDate = function () {
        $http({
            method: "GET",
            dataType: 'JSON',
            url: 'humanresource/SFBonusSheetReport/GetBonusEffectiveDate/'
        }).then(function successCallback(response) {
            $scope.BonusPointEffectiveDate = response.data.data;
        });
    };
    $scope.GetBonusEffectiveDate();

    $scope.paymentMode = null;
    $scope.hrStatus = {
        pstatus: 'Default'
    };
    $scope.month = null;
    $scope.year = null;
    $scope.isCompletedMonth = null;
    $scope.salaryProcessId = null;

    $scope.unitId = null;
    $scope.departmentId = null;
    $scope.divisionId = null;
    $scope.sectionId = null;
    $scope.subSenctionId = null;
    $scope.payGroupId = null;
    $scope.languageId = null;
    $scope.localLanguageList = [];
    cboService.getLanguageIdCbo(function (result) {
        $scope.localLanguageList = result;
    });
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
   

  
    $scope.GetBonusReportPercentage = function () {
        $scope.bonusType = "Percentage";
        if (baseService.isUndefinedOrNull($scope.payGroupId)) {
            throw "Select PayGroup.";
        }
        if (baseService.isUndefinedOrNull($scope.paymentMode)) {
            throw "Select Payment Mode.";
        }
        if (baseService.isUndefinedOrNull($scope.PeraModel.BonusPointId)) {
            throw "Select Bonus Point.";
        }
        $scope.parameters = 'payGroup=' + $scope.payGroupId + '&paymentMode=' + $scope.paymentMode + '&languageId=' + $scope.languageId + '&bonusPointId=' + $scope.PeraModel.BonusPointId + '&bunusType=' + $scope.bonusType;

        $rootScope.report("HumanResource/SFBonusSheetReport/GetSFBonusSheet?" + $scope.parameters);

        $scope.bonusType = "";

    };

    $scope.GetBonusReportProportional = function () {
        $scope.bonusType = "Proportional";

        if (baseService.isUndefinedOrNull($scope.payGroupId)) {
            return ShowResult('Select PayGroup.', 'failure');
        }
        if (baseService.isUndefinedOrNull($scope.paymentMode)) {
            return ShowResult('Select Payment Mode.', 'failure');
        }
        if (baseService.isUndefinedOrNull($scope.PeraModel.BonusPointId)) {
            return ShowResult('Select Bonus Point.', 'failure');
        }
        $scope.parameters = 'payGroup=' + $scope.payGroupId + '&paymentMode=' + $scope.paymentMode + '&languageId=' + $scope.languageId + '&bonusPointId=' + $scope.PeraModel.BonusPointId + '&bunusType=' + $scope.bonusType;
        $rootScope.report("HumanResource/SFBonusSheetReport/GetSFBonusSheet?" + $scope.parameters);
        $scope.bonusType = "";
    };
    
    $scope.EmployeeList = [];
    $scope.EmployeeListDefault = [];
    $scope.EmployeeListTemp = [];

    $scope.GetEmployeeInformation = function () {       
        var parameters = {
           'effectiveDate': $scope.PeraModel.BonuseffectiveDate
        };
        $http({
            method: "POST",
            dataType: 'JSON',
            url: 'humanresource/SFBonusSheetReport/GetEmpInfo',
            data: parameters
        }).then(function successCallback(response) {
            if (response.data.length > 0) {
                $scope.gridShow = true;

                for (var i = 0; i < response.data.length; i++) {
                    if (angular.isUndefinedOrNull(response.data[i].DOJ) == false) {
                        response.data[i].DOJ = new Date(response.data[i].DOJ);
                    }
                    if (angular.isUndefinedOrNull(response.data[i].DOS) == false) {
                        response.data[i].DOS = new Date(response.data[i].DOS);
                    }
                    //$scope.reportHeader = response.data[i].Remarks;
                }
                $scope.EmployeeListDefault = response.data.filter(d => d.isSelect == true);
                $scope.EmployeeList = $scope.EmployeeListDefault;
                $scope.EmployeeListTemp = $scope.EmployeeListDefault;

                //for (var i = 0; i < response.data.length; i++) {
                //    response.data[i]["DOJ"] = new Date(response.data[i]["DOJ"]);
                //    response.data[i]["DOS"] = new Date(response.data[i]["DOS"]);
                //}

            }
            else {
                $scope.empGrid = false;

                ShowResult("No Data Found", 'failure');

            }
        });


    };
    $scope.GetBonusFromGrid = function (bonusType) {
        try {
            var parameters = [];
            var gridObj = $("#empInfoGrid").ejGrid("instance");
            var filteredRecords = gridObj.getFilteredRecords();
            if ($scope.isManualFilter == true) {
                if (filteredRecords.length == 0) {
                    filteredRecords = $scope.EmployeeListTemp;
                }
            }
            if (angular.isUndefinedOrNull(filteredRecords) === false) {
                if (filteredRecords.length > 0) {
                    parameters = [];
                    parameters.push({ "Key": "EmpSystemId", "Value": getString(filteredRecords, "EmpSystemId") });
                }
            }
            if (parameters.length === 0) {
                parameters.push({ "Key": "", "Value": "" });

            }
          
            $http({
                method: 'POST',
                url: 'humanresource/SFBonusSheetReport/GetSFBonusSheetGrid',
                data: {
                    'parameters': parameters,
                    'cutoffdate': $scope.PeraModel.BonuseffectiveDate,
                    'languageId': $scope.languageId,
                    'paymentMode': $scope.paymentMode,
                    'bonusType': bonusType,
                    'isStampDeductApplicable': $scope.isStampDeductApplicable,        
                    'reportHeader': $scope.reportHeader,
                    'docGrouping': $scope.docGrouping  
                }
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    $rootScope.report($scope.downloadgriddataUrl + "?FileName=" + response.data.FileName);
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
}