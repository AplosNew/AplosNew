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

    $('.datepicker').datepicker({
        startDate: '-1d',
        endDate: '1d',
        datesDisabled: $scope.DisabledDates,
        format: 'dd-M-yyyy',
        todayHighlight: true,
        autoclose: true,
        inline: true,
        changeMonth: true
    });

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


    $scope.GetDblClick = function (args) {
        $scope.ModelNew = Object.assign({}, args.data);
        $scope.GetGoodWorkDetailCenter();
        //if (baseService.arrayLength($scope.checkedByList) == 1) {
        //    $scope.ModelNew.CheckedBy = $scope.checkedByList[0].Value;
        //}
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

}