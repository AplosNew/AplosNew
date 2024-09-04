'use strict';
GoodWorkDateChangeController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', "$controller"];
function GoodWorkDateChangeController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $controller) {
    $rootScope.title = 'Good Work';
    $scope.ModelList = [];
    $scope.path = 'Attendances/GoodWork/';
    $scope.saveUrl = $scope.path + 'CreateGoodWork';
    $scope.UpdateUrl = $scope.path + 'UpdateGoodWorkDetailEdit';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    //$scope.deleteUrl = $scope.path + 'delete/';
    $scope.deleteChildUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl);
    $scope.LoadEmpListUrl = $scope.path + 'LoadEmployeelist';
    $scope.Action = 'Save';
    $scope.passwordShow = true;
    $controller("employeeBaseController", { $scope: $scope, $http: $http });
    $scope.downloadgriddataUrlPath = 'GridReports/DownloadUsingFullPath';

    //***********************************Good Work ********************************************************//
    $scope.btnDisable = false;
    $scope.ModelTemp = {
        Id: null,
        WorkDate: null,
        EmployeeCategoryId: null,
        EmployeeCategory: null,
        Purpose: null,
        PurposeCategory: null,
        ShiftId: null,
        Shift: null,
        FromTime: null,
        ToTime: null,
        Minute: null,
        Reason: null,
        Remarks: null,
        UserGroup: null,
        CheckedStatus: null,
        WD: true
    };
    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);

    function DateFunc() {
    $('.datepicker').datepicker({
        startDate: '-60d',
        endDate: '1d',
        datesDisabled: $scope.DisabledDates,
        format: 'dd-M-yyyy',
        todayHighlight: true,
        autoclose: true,
        inline: true,
        changeMonth: true
    });
    }
    DateFunc();

    $scope.ModelEmpTemp = {
        Id: null,
        EmpSystemId: null,
        EmployeeCode: null,
        EmployeeName: null,
        FromTime: null,
        ToTime: null,
        Minute: null,
        Purpose: null,
        PurposeCategory: null,
        ApprovedById: null,
        ApprovedByName: null,
        Remark: null
    };
    $scope.ModelEmpNew = Object.assign({}, $scope.ModelEmpTemp);

    ////Load Employee
    $scope.ShiftList = [];
    $scope.selectShift = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.ModelNew.UserGroupId)) {
                throw "Select User Group.";
            }

            $http({
                method: 'GET',
                url: 'Attendances/GoodWork/getShift?setupId=' + $scope.ModelNew.UserGroupId + '&date=' + $scope.ModelNew.WorkDate,
                dataType: 'JSON'
            }).then(function succ(resp) {
                $scope.ShiftList = resp.data;
                angular.element(document.querySelector('#ShiftPop')).modal('show');
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }




    $scope.doubleShift = function (e) {
        $scope.ModelNew.ShiftId = e.data.ShiftId;
        $scope.ModelNew.Shift = e.data.ShiftDefination;
        $scope.getFiltersData();
        angular.element(document.querySelector('#ShiftPop')).modal('hide');
    }

    $scope.closeShiftPopUp = function () {
        angular.element(document.querySelector('#ShiftPop')).modal('hide');
    }


    $scope.EmployeeCategoryList = [];
    $scope.getEmployeeCategory = function () {
        $http({
            method: 'GET',
            url: $scope.path + "GetEmployeeCategoryList",
        }).then(function successCallback(response) {
            $scope.EmployeeCategoryList = response.data;
        });
    }
    $scope.getEmployeeCategory();

    cboService.getCboDepartmentByCompanyGroup(null, function (result) {
        $scope.DepartmentList = result;
    });
    //cboService.getCboSectionByCompanyGroup(null, function (result) {
    //    $scope.SectionList = result;
    //});

    //cboService.getCboSubSectionByCompanyGroup(null, function (result) {
    //    $scope.SubSectionList = result;
    //});

    $scope.sectionList = [];
    $scope.changeSectionByDept = function () {
        cboService.getSectionCboByDepartmentId($scope.ModelNew.DepartmentId, function (result) {
            $scope.sectionList = result;
        });
    }

    $scope.subSectionList = [];
    $scope.changeSubSectionBySection = function () {
        cboService.getSubSectionCboBySectionId($scope.ModelNew.SectionId, function (result) {
            $scope.subSectionList = result;
        });
    }
    cboService.getbyDesignationMasterCbo(function (result) {
        $scope.designationList = result;
    });


    $scope.EmployeeList = [];
    $scope.getEmploymeeList = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.ModelNew.WorkDate)) {
                $scope.ModelNew.WorkDate = $scope.DT;
            }
            if (baseService.isUndefinedOrNull($scope.ModelNew.WorkDate)) {
                throw"Select Work Date.";
            }
            if (baseService.isUndefinedOrNull($scope.ModelNew.UserGroupId)) {
                throw "Select User Group.";
            }
            if (baseService.isUndefinedOrNull($scope.ModelNew.ShiftId)) {
                throw "Select Shift.";
            }
            $scope.filterComplete();
            if (!baseService.isUndefinedOrNull($scope.ModelNew.FromTime)) {
                if (!baseService.isUndefinedOrNull($scope.ModelNew.ToTime)) {
                    if (!baseService.isUndefinedOrNull($scope.ModelNew.WorkDate)) {
                        if ($scope.ModelNew.Minute > 0) {
                            $http({
                                method: 'POST',
                                url: 'Attendances/GoodWork/LoadEmployeelist',
                                data: { 'parameters': $scope.parameters, 'userGroupId': $scope.ModelNew.UserGroupId, 'shiftId': $scope.ModelNew.ShiftId, 'workDate': $scope.ModelNew.WorkDate },
                                dataType: 'JSON'
                            }).then(function successCallback(response) {
                                if (response.data.Error === true) {
                                    ShowResult(response.Message, 'failure');
                                }
                                else {
                                    $scope.EmployeeList = response.data;
                                    angular.element(document.querySelector("#dialogEmployeeInfo")).modal("show");
                                    //var eDialog = $("#dialogEmployeeInfo").data("ejDialog");
                                    //eDialog.open();
                                }
                            },
                                function errorCallBack(response) {
                                    ShowResult(response.Message, 'failure');
                                });
                        }
                        else {
                            throw "Calculated minute should not be negative value!";
                        }
                    }
                    else {
                        throw "Select Work Date!";
                    }
                }
                else {
                    throw "Select To Time!";
                }
            }

            else {
                throw "Select From Time!";
            }
        }
        catch (e) {
            ShowResult(e, "failure");
        }
    };


    $scope.refreshTemplateemployee4 = function (args) {
        $("#headchk4").ejCheckBox({ "change": CheckBoxSelectAllEmolyeeWise });
    };

    function CheckBoxSelectAllEmolyeeWise(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        var filtered = $("#GridEmployeeInfoList").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.EmployeeList.length; i++) {
                $scope.EmployeeList[i].CheckBoxSelect = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].CheckBoxSelect = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridEmployeeInfoList").data("ejGrid");
        gridObj.refreshContent();
    };

    $scope.GetSelectedEmployeeList = function () {
        try {
            for (var i = 0; i < $scope.EmployeeList.length; i++) {
                //if ($scope.EmployeeList[i].DayStatus == 'P' || $scope.EmployeeList[i].DayStatus == 'L' || $scope.EmployeeList[i].DayStatus == 'W' && $scope.EmployeeList[i].OverStay !=null) {
                //    if (checkItemExist($scope.GoodWorkList, $scope.EmployeeList[i].SystemId) === false) {
                //        if ($scope.EmployeeList[i].CheckBoxSelect === true) {
                //            $scope.EmployeeList[i].FromTime = $scope.ModelNew.FromTime;
                //            $scope.EmployeeList[i].ToTime = $scope.ModelNew.ToTime;
                //            $scope.EmployeeList[i].Purpose = $scope.ModelNew.Purpose;
                //            $scope.EmployeeList[i].Minute = $scope.ModelNew.Minute;
                //            $scope.EmployeeList[i].PurposeCategory = $scope.ModelNew.PurposeCategory;
                //            $scope.EmployeeList[i].Remark = $scope.ModelNew.Remarks;
                //            $scope.GoodWorkList.push($scope.EmployeeList[i]);
                //        }
                //    }

                //}
                if (!baseService.isUndefinedOrNull($scope.EmployeeList[i].InTime)) {
                    if (checkItemExist($scope.GoodWorkList, $scope.EmployeeList[i].SystemId) === false) {
                        if ($scope.EmployeeList[i].CheckBoxSelect === true) {
                            $scope.EmployeeList[i].FromTime = $scope.ModelNew.FromTime;
                            $scope.EmployeeList[i].ToTime = $scope.ModelNew.ToTime;
                            $scope.EmployeeList[i].Purpose = $scope.ModelNew.Purpose;
                            $scope.EmployeeList[i].Minute = $scope.ModelNew.Minute;
                            $scope.EmployeeList[i].PurposeCategory = $scope.ModelNew.PurposeCategory;
                            $scope.EmployeeList[i].Remark = $scope.ModelNew.Remarks;
                            $scope.GoodWorkList.push($scope.EmployeeList[i]);
                        }
                    }
                }
                //else if ($scope.EmployeeList[i].DayStatus == 'W' && $scope.EmployeeList[i].OverStay==0) {
                //    throw "Can't Add When DayStatus is Weekend & OverStay is O!";
                //}
                angular.element(document.querySelector("#dialogEmployeeInfo")).modal("hide");
            }
        }
        catch (e) {
            ShowResult(e, "failure");
        }
    };

    function checkItemExist(list, SystemId) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].SystemId === SystemId) {
                return true;
            }
        }
        return false;
    }


    $scope.popUpDataList = [];
    $scope.showByWhomEmployeeListPopUp = function (index) {
        try {
            $scope.tempIndex = index;
            $scope.popUpDataList = [];
            $http({
                method: 'GET',
                url: 'Attendances/GoodWork/GetAllActiveEmpData'
            }).then(function successCallback(response) {
                $scope.popUpDataList = response.data;
            });
            angular.element(document.querySelector('#popUp')).modal('show');
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.SelectEmployee = function (arg) {
        $scope.GoodWorkList[$scope.tempIndex].ApprovedById = arg.data.SystemId;
        $scope.GoodWorkList[$scope.tempIndex].ApprovedByCode = arg.data.EmployeeCode;
        $scope.GoodWorkList[$scope.tempIndex].ApprovedByName = arg.data.EmployeeName;
        $scope.closePopUp();
    }

    $scope.clearEmp = function () {
        $scope.GoodWorkList[$scope.tempIndex].ApprovedById = null;
        $scope.GoodWorkList[$scope.tempIndex].ApprovedByCode = null;
        $scope.GoodWorkList[$scope.tempIndex].ApprovedByName = null;
    }

    $scope.closePopUp = function () {
        angular.element(document.querySelector('#popUp')).modal('hide');
    }

    $scope.getMinute = function () {
        try {
            //$scope.ModelNew.YesterDay = $filter('dateFiltering')(new Date($scope.ModelNew.WorkDate).setDate(new Date($scope.ModelNew.WorkDate).getDate() + 1), 'dd-MM-yyyy');

            if (!baseService.isUndefinedOrNull($scope.ModelNew.FromTime) && !baseService.isUndefinedOrNull($scope.ModelNew.ToTime)) {
                $scope.MinuteUrl = 'Attendances/GoodWork/GetMinute'
                $http({
                    method: 'POST',
                    url: $scope.MinuteUrl,
                    data: { 'data': $scope.ModelNew },
                    dataType: 'JSON'
                }).then(function successCallback(response) {

                    /*data.CalculatedTime = response.data;*/
                    $scope.ModelNew.Minute = response.data;
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }


    $scope.getMinuteEdit = function () {
        try {
            if (!baseService.isUndefinedOrNull($scope.ModelNewtemp.FromTime) && !baseService.isUndefinedOrNull($scope.ModelNewtemp.ToTime)) {
                $scope.MinuteUrl = 'Attendances/GoodWork/GetMinute'
                $http({
                    method: 'POST',
                    url: $scope.MinuteUrl,
                    data: { 'data': $scope.ModelNewtemp },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    $scope.ModelNewtemp.Minute = response.data;
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }


    $scope.removeRow = function (data) {
        $scope.empSystemId = data.SystemId;
        $scope.Id = data.Id;
        if (baseService.isUndefinedOrNull(data.EmployeeName))
            $scope.message_confirmation = 'Are you sure want to delete this data....';
        else
            $scope.message_confirmation = 'Are you sure want to delete [ ' + data.EmployeeName + ' ]';
        angular.element(document.querySelector('#confirmgenericPopUp')).modal('show');
    };

    $scope.DeleteRow = function () {
        if ($scope.Id == "") {
            var tempData = $scope.GoodWorkList;
            for (var i = 0; i < tempData.length; i++) {
                if (tempData[i].SystemId === $scope.empSystemId) {
                    $scope.GoodWorkList.splice(i, 1);
                }
            }
            $scope.Id = null;
            tempData = [];
        }
        else {
            $http({
                method: 'GET',
                url: 'Attendances/GoodWork/DeleteChildUrl?Id=' + $scope.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.GetGoodWorkDetailCenter();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };


    $scope.Save = function () {
        try {
            if ($scope.ModelNew.CheckedStatus == 'Checked' || $scope.ModelNew.ApprovedStatus == 'Approved') {
                throw "Checked or Approved data can't be updated!";
            }

            $scope.$broadcast('show-errors-check-validity');
            if ($scope.ModelNewForm.$valid) {
                $scope.btnDisable = true;
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: { 'data': $scope.ModelNew, 'goodWorkDetail': $scope.GoodWorkList },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        $scope.btnDisable = false;
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.btnDisable = false;
                        $scope.Clear();
                        $scope.getData();
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            }

        }
        catch (e) {
            ShowResult(e, 'failure');
        }
    };


    $scope.Clear = function () {
        $scope.Action = 'Save';
        $scope.GoodWorkList = [];
        $scope.DT = [];
        $scope.ModelNew = Object.assign({}, $scope.ModelTemp);
        $scope.dateControlShow = true;
        $scope.LBLShow = false;
        return true;
        $scope.btnDisable = false;
    };

    $scope.WorkDates = $filter('date')(new Date(), 'dd-MMM-yyyy');

    $scope.getData = function () {
        $http({
            method: 'Get',
            url: $scope.path + "GetGoodWorkList?workDate=" + $scope.WorkDates,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ModelList = response.data;
        });
    }
    $scope.getData();
    $scope.DT = null;
    $scope.dateControlShow = true;
    $scope.LBLShow = false;
    $scope.GetDblClick = function (args) {
        $scope.ModelNew = Object.assign({}, args.data);
        if ($scope.ModelNew.WD) {
            $scope.dateControlShow = true;
            $scope.LBLShow = false;
        }
        else {
            $scope.DT = $scope.ModelNew.WorkDate;
            $scope.ModelNew.WorkDate = $scope.DT;
            $scope.LBLShow = true;
            $scope.dateControlShow = false;
        }
        $scope.GetGoodWorkDetailCenter();
        $scope.GetGoodWorkCheckByCbo();
        $scope.Action = 'Update';
        if (baseService.arrayLength($scope.checkedByList) == 1) {
            $scope.ModelNew.CheckedBy = $scope.checkedByList[0].Value;
        }
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.GoodWorkList = [];
    $scope.GetGoodWorkDetailCenter = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'GetGoodWorkDetailCenter?goodWorkId=' + $scope.ModelNew.Id,
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.GoodWorkList = resp.data;
            for (var i = 0; i < $scope.EmployeeCategoryList.length; i++) {
                if ($scope.EmployeeCategoryList[i].Id == $scope.GoodWorkList[0].EmployeeCategoryId) {
                    $scope.ModelNew.EmployeeCategoryId = $scope.GoodWorkList[0].EmployeeCategoryId;
                    $scope.ModelNew.EmployeeCategory = $scope.GoodWorkList[0].EmployeeCategory;
                    break;
                }

            }
            $scope.ModelNew.DepartmentId = $scope.GoodWorkList[0].DepartmentId;
            $scope.ModelNew.Department = $scope.GoodWorkList[0].Department;
            $scope.ModelNew.SectionId = $scope.GoodWorkList[0].SectionId;
            $scope.ModelNew.Section = $scope.GoodWorkList[0].Section;
            $scope.ModelNew.SubSectionId = $scope.GoodWorkList[0].SubSectionId;
            $scope.ModelNew.SubSection = $scope.GoodWorkList[0].SubSection;
            $scope.ModelNew.DesignationId = $scope.GoodWorkList[0].DesignationId;
            $scope.ModelNew.Designation = $scope.GoodWorkList[0].Designation;
            $scope.ModelNew.UserGroup = $scope.GoodWorkList[0].UserGroup;
            $scope.ModelNew.Purpose = $scope.GoodWorkList[0].Purpose;
            $scope.ModelNew.PurposeCategory = $scope.GoodWorkList[0].PurposeCategory;
            $scope.getFiltersData();
        });
    }

    //Edit
    $scope.editindex = -1;
    $scope.ModelNewtemp = {};
    $scope.selectEdit = function (data, index) {
        angular.copy(data, $scope.ModelNewtemp);
        $scope.editindex = index;
        angular.element(document.querySelector('#EditPopUp')).modal('show');
    }
    $scope.closeEditPopUp = function () {
        angular.copy($scope.ModelNewtemp, $scope.GoodWorkList[$scope.editindex]);
        angular.element(document.querySelector('#EditPopUp')).modal('hide');
    }


    //Edit

    // UserGroup
    $scope.UserGroupList = [];
    $scope.selectUserGroup = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'getUserGroupData',
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.UserGroupList = resp.data;
        });
    }
    $scope.selectUserGroup();

    // UserGroup


    $scope.filters = [];
    $scope.getFiltersData = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.ModelNew.WorkDate)) {
                $scope.ModelNew.WorkDate = $scope.DT;
            }
            $http({
                method: 'GET',
                url: $scope.path + 'getFiltersData?userGroupId=' + $scope.ModelNew.UserGroupId + '&shiftId=' + $scope.ModelNew.ShiftId + '&date=' + $scope.ModelNew.WorkDate,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                $scope.filters = response.data;
                var columnList = [
                    { field: 'EmployeeCategory', width: 110, headerText: "Employee Category", type: "string" },
                    { field: 'Department', width: 100, headerText: "Department", type: "string" },
                    { field: 'Section', width: 100, headerText: "Section", type: "string" },
                    { field: 'SubSection', width: 100, headerText: "Sub-Section", type: "string" },
                    { field: 'LegalDesignation', width: 100, headerText: "Designation", type: "string" },
                    { field: 'UserReportGroup', width: 100, headerText: "User Group", type: "string" }

                ];
                $("#filters").ejGrid({
                    dataSource: $scope.filters,
                    minWidth: 450, minHeight: 400,
                    allowFiltering: true, allowPaging: true, enableTouch: true, responsive: true, allowTextWrap: true, allowScrolling: true,
                    filterSettings: { filterType: "excel" },
                    columns: columnList
                });

                var gridObj = $("#filters").data("ejGrid");
                gridObj.refreshContent(true);
                gridObj.refreshTemplate();
                $("#filters").children('.e-pager.e-js.e-pager').hide();
                $("#filters").children('.e-gridcontent.e-droppable.e-js').hide();
                $("#filters").children('.e-gridcontent').hide();
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }
    /* $scope.getFiltersData();*/

    $scope.parameters = [];
    $scope.filterComplete = function () {
       
        var g = $("#filters").data("ejGrid");
        var fl = g.getFilteredRecords();
        if (fl.length == 0) {
            fl = $scope.filters;
        }
        var parameters = [];
        parameters.push({ "Key": "EmpCategoryId", "Value": getString(fl, "EmpCategoryId") });
        parameters.push({ "Key": "DepartmentId", "Value": getString(fl, "DepartmentId") });
        parameters.push({ "Key": "SectionId", "Value": getString(fl, "SectionId") });
        parameters.push({ "Key": "SubSectionId", "Value": getString(fl, "SubSectionId") });
        parameters.push({ "Key": "DesignationId", "Value": getString(fl, "DesignationId") });
        parameters.push({ "Key": "UserReportGroupId", "Value": getString(fl, "UserReportGroupId") });

        $scope.parameters = parameters;
    }

    var getString = function (data, column) {
        var string = "''";
        var collection = [];

        for (var i = 0; i < data.length; i++) {
            if (collection.includes(data[i][column]) == false) {
                string += ",'" + data[i][column] + "'";
                collection.push(data[i][column]);
            }
        }
        return string;
    }


    $scope.checkedByList = [];
    $scope.GetGoodWorkCheckByCbo = function () {
        $http({
            method: 'GET',
            url: 'Attendances/GoodWork/GetGoodWorkCheckByCbo?setupId=' + $scope.ModelNew.UserGroupId
        }).then(function successCallback(response) {
            $scope.checkedByList = response.data;
            if (baseService.arrayLength($scope.checkedByList) == 1) {
                $scope.ModelNew.CheckedBy = $scope.checkedByList[0].Value;
            }
        });
    };

    $scope.userGroupDataList = [];
    $scope.GetUserGroupList = function () {
        $http({
            method: 'GET',
            url: 'Attendances/GoodWork/GetUserGrData'
        }).then(function successCallback(response) {
            $scope.userGroupDataList = response.data;
        });
    }
    $scope.GetUserGroupList();

    $scope.GoodWorkReport = function () {
        $scope.fileName = "GoodWorkReport.xlsx";

        $http({
            method: 'POST',
            url: $scope.path + "GetGoodWorkReport",
            data: { 'reportFileName': $scope.fileName, 'workDate': $scope.WorkDates },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                $rootScope.report($scope.downloadgriddataUrlPath + "?FullPath=" + response.data.FileName + "&fileName=" + $scope.fileName);
            }
        }, function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
        });
    }

}