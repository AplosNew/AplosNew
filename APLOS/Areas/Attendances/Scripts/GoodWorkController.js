'use strict';
GoodWorkController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', "$controller"];
function GoodWorkController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $controller) {
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
    //***********************************Good Work ********************************************************//

    $scope.ModelTemp = {
        Id: null,
        WorkDate: null,
        EmployeeCategory: null,
        Department: null,
        SubSection: null,
        Section: null,
        Designation: null,
        ShiftId: null,
        Shift: null,
        FromTime: null,
        ToTime: null,
        CalculatedTime: null,
        Remarks: null,
        UserGroup: null
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
        CalculatedTime: null,
        Purpose: null,
        PurposeCategory: null,
        ApprovedById: null,
        ApprovedByName: null,
        Remarks: null
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
    cboService.getCboSectionByCompanyGroup(null, function (result) {
        $scope.SectionList = result;
    });

    cboService.getCboSubSectionByCompanyGroup(null, function (result) {
        $scope.SubSectionList = result;
    });
    cboService.getbyDesignationMasterCbo(function (result) {
        $scope.designationList = result;
    });


    $scope.EmployeeList = [];
    $scope.GoodWorkList = [];
    $scope.getEmploymeeList = function () {
        try {
            if (!baseService.isUndefinedOrNull($scope.ModelNew.FromTime) && !baseService.isUndefinedOrNull($scope.ModelNew.ToTime) && !baseService.isUndefinedOrNull($scope.ModelNew.UserGroup)) {
                $http.get($scope.LoadEmpListUrl + '?empCategory=' + $scope.ModelNew.EmployeeCategory + '&department=' + $scope.ModelNew.Department + '&section=' + $scope.ModelNew.Section
                    + '&subSection=' + $scope.ModelNew.SubSection + '&designation=' + $scope.ModelNew.Designation + '&userGroup=' + $scope.ModelNew.UserGroup)
                    .then(function successCallback(response) {
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
                throw "Select From Time, To Time & User Group";
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
                if (checkItemExist($scope.GoodWorkList, $scope.EmployeeList[i].SystemId) === false) {
                    if ($scope.EmployeeList[i].CheckBoxSelect === true) {
                        $scope.EmployeeList[i].FromTime = $scope.ModelNew.FromTime;
                        $scope.EmployeeList[i].ToTime = $scope.ModelNew.ToTime;
                        $scope.EmployeeList[i].CalculatedTime = $scope.ModelNew.CalculatedTime;
                        $scope.GoodWorkList.push($scope.EmployeeList[i]);
                    }
                }
            }
            angular.element(document.querySelector("#dialogEmployeeInfo")).modal("hide");
        } catch (e) {
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
            if (!baseService.isUndefinedOrNull($scope.ModelNew.FromTime) && !baseService.isUndefinedOrNull($scope.ModelNew.ToTime)) {
                $scope.MinuteUrl = 'Attendances/GoodWork/GetMinute'
                $http({
                    method: 'POST',
                    url: $scope.MinuteUrl,
                    data: { 'data': $scope.ModelNew },
                    dataType: 'JSON'
                }).then(function successCallback(response) {

                    /*data.CalculatedTime = response.data;*/
                    $scope.ModelNew.CalculatedTime = response.data;
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
                    $scope.ModelNewtemp.CalculatedTime = response.data;
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
            $scope.$broadcast('show-errors-check-validity');
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
            url: $scope.path + "GetGoodWorkList",
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ModelList = response.data;
        });
    }
    $scope.getData();


    $scope.GetDblClick = function (args) {
        $scope.ModelNew = Object.assign({}, args.data);
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

}