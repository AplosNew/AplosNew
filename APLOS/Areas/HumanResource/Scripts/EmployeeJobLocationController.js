'use strict';
EmployeeJobLocationController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$window'];
function EmployeeJobLocationController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window) {
    $rootScope.title = 'Employee Job Location';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.EmployeeShiftAssigns = [];
    $scope.path = 'humanresource/EmployeeJobLocation/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';
    baseService.init($scope.getListUrl, null, null, null, 'EmployeeCode', 'EmployeeCode');

    $scope.employeeShiftAssign = {
        SystemID: null,
        EmpSystemID: null,
        FixSystemID: null,
        RosterSystemID: null,
        IsFix: true,
        IsRoster: false,
        EffectiveDate: null,
        RosterStartShiftID: null,
        StartFromDay: null,
        PlantId: null,
        StartFromShift: null
    };

    $scope.employeeWeekOffByDay = {
        SystemID: null,
        EmpSystemID: null,
        FixSystemID: null,
        EffectiveDate: null,
        AlignWithCC: 'True',
        IndividualWeekOff: null,
        FstOffDay: null,
        FstDayLengthType: null,
        SndOffDay: null,
        SndDayLengthType: null,
        FirstHalfRadio: 'Half',
        FirstFullRadio: null,
        SecondHalfRadio: 'Half',
        SecondFullRadio: null
    };

    $scope.ShiftParameters = {
        limit: 10,
        offset: 0,
        order: 'DESC',
        sort: 'EmployeeCode',
        searchBy: "EmployeeCode",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.getData = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.employeeShiftAssign.PlantId)) {
                throw "Select Plant.";
            }
            if (baseService.isUndefinedOrNull($scope.employeeShiftAssign.WorkDate)) {
                throw "Select Work Date.";
            }
            $scope.GLUrl = 'humanresource/employeeshiftassign/getlist?plantId=' + $scope.employeeShiftAssign.PlantId + '&date=' + $scope.employeeShiftAssign.WorkDate,
                $scope.LoadDataList = function (pageno) {
                    baseService.paginationBase($scope.GLUrl, pageno, $scope.ShiftParameters)
                        .then(function (data) {
                            $scope.EmployeeShiftAssigns = data.Rows;
                            //console.log($scope.EmployeeShiftAssigns);
                            $scope.ShiftParameters.total_count = data.Total;
                        }, function () {
                            ShowResult(commonMessage.NetworkError, 'failure');
                        }).finally(function () {
                        });
                };
            $scope.LoadDataList();
        } catch (e) {
            ShowResult(e, 'Error');
        }
    };

    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.PlantId = $scope.employeeShiftAssign.PlantId;
        $scope.employeeShiftAssign.WorkDate = $scope.employeeShiftAssign.WorkDate;
        $scope.WorkDate = $scope.employeeShiftAssign.WorkDate;
        $scope.employeeShiftAssign = $scope.EmployeeShiftAssigns[$scope.index];
        $scope.employeeShiftAssign.PlantId = $scope.PlantId;
        $scope.employeeShiftAssign.WorkDate = $scope.WorkDate;
        if ($scope.employeeShiftAssign.IsFix === false) {
            $scope.employeeShiftAssign.IsRoster = true;
        } else {
            $scope.employeeShiftAssign.IsRoster = false;
        }
        $scope.employeeShiftAssign.EffectiveDate = $scope.employeeShiftAssign.WorkDate;
        $scope.employeeWeekOffByDay.EmpSystemID = $scope.employeeShiftAssign.EmpSystemID;
        $scope.employeeWeekOffByDay.FstOffDay = $scope.employeeShiftAssign.FstOffDay;
        $scope.employeeWeekOffByDay.FstDayLengthType = $scope.employeeShiftAssign.FstDayLengthType;
        $scope.employeeWeekOffByDay.SndDayLengthType = $scope.employeeShiftAssign.SndDayLengthType;
        $scope.employeeWeekOffByDay.SndOffDay = $scope.employeeShiftAssign.SndOffDay;
        $scope.employeeWeekOffByDay.AlignWithCC = $scope.employeeShiftAssign.AlignWithCC;
        $scope.employeeWeekOffByDay.IndividualWeekOff = $scope.employeeShiftAssign.IndividualWeekOff;
        if ($scope.employeeWeekOffByDay.AlignWithCC === false) {
            $scope.employeeShiftAssign.IndividualWeekOff = true;
        } else {
            $scope.employeeShiftAssign.IndividualWeekOff = false;
        }
        $scope.employeeWeekOffByDay.FixSystemID = $scope.employeeShiftAssign.FixSystemID;
        $scope.employeeWeekOffByDay.EffectiveDate = $scope.employeeShiftAssign.WorkDate;
    };

    $scope.PlantList = [];
    cboService.getCboPlantByCompany(null, function (result) {
        $scope.PlantList = result;
    });

    $scope.unitList = [];
    cboService.getCboUnit(function (result) {
        $scope.unitList = result;
    });

    $scope.divisionList = [];
    cboService.getCboDivisionByCompany(null, function (result) {
        $scope.divisionList = result;
    });

    $scope.departmentList = [];
    cboService.getCboDepartmentByCompany(null, function (result) {
        $scope.departmentList = result;
    });

    $scope.subSectionList = [];
    cboService.getCboSubSectionByCompany(null, function (result) {
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
    cboService.getCboSectionByCompany(null, function (result) {
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

    $scope.fixedShitList = [];
    $scope.Shift = function () {
        cboService.getCboShiftDefinationByPlant($scope.employeeShiftAssign.PlantId, function (result) {
            $scope.fixedShitList = result;
        });
    };

    $scope.RoasterShift = function () {
        cboService.getRoasterCboByPlant($scope.employeeShiftAssign.PlantId, function (result) {
            $scope.roasterShitList = result;
        });
    };

    $scope.LoadRoasterShift = function (roasterId) {
        cboService.getRosterWiseShiftCbo($scope.employeeShiftAssign.PlantId, roasterId, function (result) {
            $scope.rosterStartShiftList = result;
        });
    };

    $scope.ShiftPopUp = function () {
        $scope.employeeShiftAssign.EffectiveDate = $scope.employeeShiftAssign.WorkDate;
        angular.element(document.querySelector("#ShiftPopUp")).modal('show');
    };

    $scope.show = function () {
        var x = document.getElementById("shift");
        var y = document.getElementById("shift1");
        var z = document.getElementById("shift2");
        var w = document.getElementById("shift3");
        var v = document.getElementById("shift4");
        var a = document.getElementById("shift5");
        if (x.style.display === "none" && y.style.display === "none" && z.style.display === "none"
            && w.style.display === "none" && v.style.display === "none" && a.style.display === "none") {
            x.style.display = "block";
            y.style.display = "block";
            z.style.display = "block";
            w.style.display = "block";
            v.style.display = "block";
            a.style.display = "block";
        }
    };
    $scope.Hide = function () {
        var x = document.getElementById("shift");
        var y = document.getElementById("shift1");
        var z = document.getElementById("shift2");
        var w = document.getElementById("shift3");
        var v = document.getElementById("shift4");
        var a = document.getElementById("shift5");
        if (x.style.display === "none" && y.style.display === "none" && z.style.display === "none"
            && w.style.display === "none" && v.style.display === "none" && a.style.display === "none") {
            x.style.display = "none";
        } else {
            x.style.display = "none";
            y.style.display = "none";
            z.style.display = "none";
            w.style.display = "none";
            v.style.display = "none";
            a.style.display = "none";
        }
    };

    $scope.ShowShift = function () {
        var x = document.getElementById("week");
        if (x.style.display === "none") {
            x.style.display = "block";
        }
    };
    $scope.HideShift = function () {
        var x = document.getElementById("week");
        if (x.style.display === "none") {
            x.style.display = "none";
        } else {
            x.style.display = "none";
        }
    };

    $scope.Save = function () {
        if ($scope.employeeShiftAssign.IsFix === false) {
            $scope.employeeShiftAssign.IsRoster = true;
        }
        $scope.employeeWeekOffByDay.EffectiveDate = $scope.employeeShiftAssign.EffectiveDate;
        if ($scope.Action === "Save") {
            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: { 'model': $scope.employeeShiftAssign, 'employeeWeekOffByDay': $scope.employeeWeekOffByDay },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure', 'ShiftPopUp');
                }
                else {
                    ShowResult(response.data.Message, 'success', 'ShiftPopUp');
                    $scope.getData();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure', 'ShiftPopUp');
            };
        }
    };
}