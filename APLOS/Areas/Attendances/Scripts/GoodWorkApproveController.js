'use strict';
GoodWorkApproveController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', "$controller"];
function GoodWorkApproveController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $controller) {
    $rootScope.title = 'Good Work Approve';
    $scope.ModelList = [];
    $scope.path = 'Attendances/GoodWork/';
    $scope.saveUrl = $scope.path + 'CreateGoodWorkApproved';
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
    //***********************************Good Work ********************************************************//

    $scope.ModelTemp = {
        Id: null,
        WorkDate: null,
        EmployeeCategoryId: null,
        EmployeeCategory: null,
        DepartmentId: null,
        Department: null,
        SubSectionId: null,
        SubSection: null,
        SectionId: null,
        Section: null,
        DesignationId: null,
        Designation: null,
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
        OverStay: null,
        DayStatus: null
    };
    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);

    //$('.datepicker').datepicker({
    //    startDate: '-1d',
    //    endDate: '1d',
    //    datesDisabled: $scope.DisabledDates,
    //    format: 'dd-M-yyyy',
    //    todayHighlight: true,
    //    autoclose: true,
    //    inline: true,
    //    changeMonth: true
    //});

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

    $scope.selectShift = function () {
        $scope.getsS();
        angular.element(document.querySelector('#ShiftPop')).modal('show');
    }

    $scope.ShiftList = [];
    $scope.getsS = function () {
        $http({
            method: 'GET',
            url: 'employees/route/getShift',
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.ShiftList = resp.data;
        });
    }

    $scope.doubleShift = function (e) {
        $scope.ModelNew.ShiftId = e.data.ShiftId;
        $scope.ModelNew.Shift = e.data.ShiftDefination;
        angular.element(document.querySelector('#ShiftPop')).modal('hide');
    }

    $scope.closeShiftPopUp = function () {
        angular.element(document.querySelector('#ShiftPop')).modal('hide');
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


    $scope.SaveGoodWorkApproved = function () {
        try {
            //for (var i = 0; i < $scope.GoodWorkList.length; i++) {
            //    if ($scope.GoodWorkList[i].Minute > $scope.ModelNew.Minute) {
            //        throw "Minute can not be greater than Calculated Minute!";
            //    }
            //}
            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: { 'data': $scope.ModelNew, 'goodWorkDetail': $scope.GoodWorkList },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.Clear();
                    $scope.getData();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };


    $scope.Clear = function () {
        $scope.Action = 'Save';
        $scope.ModelNew = Object.assign({}, $scope.ModelTemp);
        $scope.GoodWorkList = [];
        return true;
    };


    $scope.getData = function () {
        $http({
            method: 'Get',
            url: $scope.path + "GetGoodWorkApprovedDataList",
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ModelList = response.data;
        });
    }
    $scope.getData();

    $scope.WD = null;
    $scope.GetDblClick = function (args) {
        $scope.ModelNew = Object.assign({}, args.data);
        $scope.ModelNew.WorkDate = $filter('dateFiltering')(new Date($scope.ModelNew.WorkDate), 'dd-MM-yyyy');
        $scope.WD = $filter('dateFiltering')(new Date($scope.ModelNew.WorkDate), 'dd-MM-yyyy');
        $scope.GetGoodWorkDetailCenter();
        $scope.Action = 'Update';
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
            $scope.ModelNew.WorkDate = $scope.WD;
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

    $scope.approvedByList = [];
    $scope.GetSupervisorCboList = function () {
        $http({
            method: 'GET',
            url: 'Attendances/GoodWork/GetGoodWorkApprovedByCbo'
        }).then(function successCallback(response) {
            $scope.approvedByList = response.data;
        });
    }
    $scope.GetSupervisorCboList();

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


    $scope.detailTemp = "#tabGridContents";
    $scope.detailgrid = function detailGridData(e) {

        var filteredData = e.data["Id"];

        $http({
            method: 'POST',
            url: $scope.path + 'GetGoodWorkDetailCenter?goodWorkId=' + filteredData
        }).then(function successCallback(response) {
            $scope.GoodWorkList = response.data;

            var data = ej.DataManager($scope.GoodWorkList).executeLocal(ej.Query().where("GoodWorkId", "equal", parseInt(filteredData), true).take(100));

            e.detailsElement.find("#detailGrid").ejGrid({

                dataSource: data,
                columns: [
                    { field: "Id", headerText: "Id", width: 50 },
                    { field: "EmployeeCode", headerText: "EmployeeCode", width: 50 },
                    { field: "EmployeeName", headerText: "EmployeeName", width: 100 },
                    { field: "FromTime", headerText: "FromTime", width: 50 },
                    { field: "ToTime", headerText: "ToTime", width: 50 },
                    { field: "Minute", headerText: "Minute", width: 50 },
                    { field: "Purpose", headerText: "Purpose", width: 150 },
                    { field: "PurposeCategory", headerText: "PurposeCategory", width: 100 },
                    { field: "OverStay", headerText: "OverStay", width: 50 },
                    { field: "DayStatus", headerText: "DayStatus", width: 40 },
                    { field: "Department", headerText: "Department", width: 150 },
                    { field: "Section", headerText: "Section", width: 100 },
                    { field: "SubSection", headerText: "SubSection", width: 100 },
                    { field: "Remark", headerText: "Remark", width: 150 }

                ]
            });
            e.detailsElement.find(".tabcontrol").ejTab();
        });


    }

    $scope.refreshTemplate = function (args) {
        $("#headschk").ejCheckBox({ "change": CheckBoxSelectAllItemWise });
    };
    function CheckBoxSelectAllItemWise(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        var filtered = $("#GridEdit").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.ModelList.length; i++) {
                $scope.ModelList[i].Flag = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].Flag = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridEdit").data("ejGrid");
        gridObj.refreshContent();
    };

    $scope.UpdateAll = function () {
        try {
            $scope.ToSaveList = [];
            for (var i = 0; i < $scope.ModelList.length; i++) {
                if ($scope.ModelList[i].Flag==true) {
                    $scope.ToSaveList.push($scope.ModelList[i]);
                }
            }

            $http({
                method: 'POST',
                url: 'Attendances/GoodWork/UpdateAll',
                data: { 'data': $scope.ToSaveList},
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.getData();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }



}