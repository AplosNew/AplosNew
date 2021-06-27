'use strict';
employeeAttendanceGroupController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$window', 'cboService'];
function employeeAttendanceGroupController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window, cboService) {
    $rootScope.title = "Attendance Group";
    $scope.Action = 'Save';
    $scope.employeeAttendanceGroups = [];
    $scope.path = 'Setups/EmployeeAttendanceGroup/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';
    $scope.deleteUrl = $scope.path + 'delete/';

    $scope.employeeAttendanceGroup = {
        CompanyGroupId: $window.companyGroupId,
        PlantId: $window.plantId,
        AttendanceGroupId: null,
        EmployeeId: null,
        EmployeeCategoryId: null,
        EmployeeCategory: null,
        EmployeeCode: "",
        EmployeeName: ""
    };
    $scope.employeeAttendanceGroupNew = Object.assign({}, $scope.employeeAttendanceGroup);

    $scope.attendanceGroupList = [];
    cboService.getAttendanceGroupCbo(function (result) {
        $scope.attendanceGroupList = result;
    });

    $scope.companyGroupLineList = [];
    cboService.getCboLineByCompanyGroup(null, function (result) {
        $scope.companyGroupLineList = result;
    });

    $scope.companyGroupSubsectionList = [];
    cboService.getCboSubSectionByCompanyGroup(null, function (result) {
        $scope.companyGroupSubsectionList = result;
    });

    //#region Attendance Group

    $scope.getSavedAttendanceGroupData = function () {
        $scope.employeeAttendanceGroups = [];
        $http.get("Setups/EmployeeAttendanceGroup/AttendanceGroupQuery?attendanceGroupId=" + $scope.employeeAttendanceGroupNew.AttendanceGroupId)
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0)
                        $scope.employeeAttendanceGroups = response.data;
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
    };
    //#end region



    $scope.onClick = function (args) {
        var gridObj = $("#Grid").data("ejGrid");
        //getting corresponding record             
        var data = gridObj.getSelectedRecords()[0];
        $scope.valuePassInDelModal(data);

    };
    $scope.command = [{
        type: "details", buttonOptions: {
            text: "Delete",
            width: "100",
            click: $scope.onClick
        }
    }];

    //Deleting Rows from RetentionAllowanceList
    $scope.valuePassInDelModal = function (data) {
        $scope.tempEmpOb = data;
        if (baseService.isUndefinedOrNull($scope.tempEmpOb.Id))
            $scope.message_confirmation = 'Are you sure want to parmenently delete <b> [ ' + data.EmployeeCode + ' - ' + data.EmployeeName + ']</b>';
        else
            $scope.message_confirmation = 'Are you sure want to parmenently delete <b> [ ' + data.EmployeeCode + ' - ' + data.EmployeeName + ']</b>';
        angular.element(document.querySelector('#confirm_PopUp')).modal('show');
    };
    $scope.removeRow = function () {
        if (baseService.isUndefinedOrNull($scope.tempEmpOb.Id) === true) {
            $scope.employeeAttendanceGroups.splice($scope.empIndex, 1);
            //$scope.empIndex = -1;
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
                url: 'Setups/EmployeeAttendanceGroup/Delete',
                dataType: 'JSON',
                data: { 'id': id }
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.employeeAttendanceGroups = [];
                    $scope.searchdata = [];
                    $scope.LoadUnsavedData();
                    $scope.getSavedAttendanceGroupData();
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, 'failure');
            });
            return true;
        } catch (e) {
            ShowResult(e, 'Error');
        }
    };

    //#endregion

    $scope.searchdata = [];
    $scope.LoadUnsavedData = function () {
        $scope.searchdata = [];
        $http.get('employees/approvalconfiguration/GetEmployeeWithoutAttendanceGroupData')
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.searchdata = response.data;
                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
        //}
    };

    // #region checkbox all

    angular.isUndefinedOrNull = function (val) {
        return angular.isUndefined(val) || val === null || val === ""
    }
    function checkChangeemployee(e) {

        var val = e.model.value;
        //item level check
        var row = $filter('filter')($scope.searchdata, { 'EmployeeId': e.model.value });
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
                for (var i = 0; i < $scope.searchdata.length; i++) {
                    $scope.searchdata[i].Active = true;
                }
            }
            else {
                for (var i = 0; i < $scope.searchdata.length; i++) {
                    for (var j = 0; j < filtered.length; j++) {
                        if ($scope.searchdata[i].EmployeeId == filtered[j].EmployeeId)
                            $scope.searchdata[i].Active = true;
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
                for (var i = 0; i < $scope.searchdata.length; i++) {
                    $scope.searchdata[i].Active = false;
                }
            }
            else {
                for (var i = 0; i < $scope.searchdata.length; i++) {
                    for (var j = 0; j < filtered.length; j++) {
                        if ($scope.searchdata[i].EmployeeId == filtered[j].EmployeeId)
                            $scope.searchdata[i].Active = false;
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

    }
    $scope.refreshTemplateemployee = function (args) {
        if (args.rowIndex == 0) {
            $("#headchk").ejCheckBox({ "change": headCheckChangeemployee });
        }

        var valobj = $($("#Gridemployee .rowCheckbox")[args.rowIndex]).ejCheckBox()[0];
        var val = $($("#Gridemployee .rowCheckbox")[args.rowIndex]).ejCheckBox()[0].defaultValue;

        $($("#Gridemployee .rowCheckbox")[args.rowIndex]).ejCheckBox({ "change": null });
        var row = $filter('filter')($scope.searchdata, { 'EmployeeId': val });
        if (!baseService.isUndefinedOrNull(row) && row.length > 0) {
            if (row[0].Active == true)
                $($("#Gridemployee .rowCheckbox")[args.rowIndex]).ejCheckBox({ "checked": true });
            else
                $($("#Gridemployee .rowCheckbox")[args.rowIndex]).ejCheckBox({ "checked": false });

        }
        $($("#Gridemployee .rowCheckbox")[args.rowIndex]).ejCheckBox({ "change": checkChangeemployee });
    }

    // #endregion

    function MakeDataForSave() {

        $scope.employeeAttendanceGroups = [];
        for (var i = 0; i < $scope.searchdata.length; i++) {
            if ($scope.searchdata[i].Active == true) {
                $scope.employeeAttendanceGroups.push($scope.searchdata[i]);
            }
        }

        //getting corresponding record             
        for (var j = 0; j < $scope.employeeAttendanceGroups.length; j++) {
            $scope.employeeAttendanceGroups[j].AttendanceGroupId = $scope.employeeAttendanceGroupNew.AttendanceGroupId;
            $scope.employeeAttendanceGroups[j].CompanyGroupId = $scope.employeeAttendanceGroupNew.CompanyGroupId;
        }
    }

    $scope.Save = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.employeeAttendanceGroupNew.AttendanceGroupId)) {
                throw "Select Attendance Group.";
            }
            MakeDataForSave();

            $scope.$broadcast('show-errors-check-validity');

            if ($scope.employeeAttendanceGroupNewForm.$valid) {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: $scope.employeeAttendanceGroups,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.employeeAttendanceGroups = [];
                        $scope.searchdata = [];
                        $scope.getSavedAttendanceGroupData();
                        $scope.LoadUnsavedData();
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                };
            }
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.Clear = function () {
        ClearFields();
    }
    function ClearFields() {
        $scope.employeeAttendanceGroup = {};
        $scope.employeeAttendanceGroupNew = {};
        $scope.popUpList = [];
        $scope.valueData = [];
    }

    // #region Tablcontrol

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };
    // #endregion
}
