'use strict';
EmployeeJobLocationController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$window'];
function EmployeeJobLocationController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window) {
    $rootScope.title = 'Employee Job Location';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.path = 'humanresource/EmployeeJobLocation/';
   
    $scope.modal = {
        EmployeeCode: null,
        EmpSystemID: null,
        EmployeeName: null,
        DOJ: null,
        DOC: null,
        DesignationGroup:null,
        LegalDesignation: null,
        SystemID: null,
        JobLcSystemID: null,
        EffectiveDate:null
    }
    $scope.modalNew = Object.assign({}, $scope.modal);


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

    $scope.employee = [];
    $scope.getPopUpData = function () {
        $scope.employee = [];
        $http({
            method: 'GET',
            url: 'employees/leaveApplication/getemployeelist'
        }).then(function successCallback(response) {
            $scope.employee = response.data;
        });
        angular.element(document.querySelector('#employeeNewPopUp')).modal('show');
    }

    $scope.setEmpData = function (obj) {
      //  $scope.Clear();
        var data = obj.data;
        $scope.modalNew.EmployeeCode = data.EmployeeCode;
        $scope.modalNew.EmpSystemID = data.SystemID;
        $scope.modalNew.EmployeeName = data.EmployeeName;
        $scope.modalNew.DOJ = data.DOJ;
       
        $scope.modalNew.DesignationGroup = data.DesignationGroup;
        $scope.modalNew.LegalDesignation = data.LegalDesignation;
        $scope.modalNew.Department = data.Department;
       
        $scope.modalNew.JobLcSystemID = data.JobLcSystemID;
        $scope.imageSrc = virtualPath.EmployeePic + data.EmpPicPath;

        if (baseService.isUndefinedOrNull(data.EffectiveDate))
            $scope.modalNew.EffectiveDate = data.DOJ;
        else
            $scope.modalNew.EffectiveDate = data.EffectiveDate;

        angular.element(document.querySelector('#employeeNewPopUp')).modal('hide');
    };


    $scope.JobLocationList = [];
    $scope.LoadAllJobLocation = function () {
        $scope.JobLocationList = [];
        $scope.Flag = "Load All";
        $scope.PlantId = null;
        $http.get('employees/EmployeeInformation/GetJobLocationCbo?flag=' + $scope.Flag)
            .then(function (response) {
                $scope.JobLocationList = response.data;
            });

        $scope.Flag = "Load Less";
    };

    $scope.Flag = "Load Less";
    $scope.LoadPlantJobLocation = function () {
        $scope.JobLocationList = [];
        $scope.PlantId = null;
        $scope.Flag = "Load Less";
        $http.get('employees/EmployeeInformation/GetJobLocationCbo?flag=' + $scope.Flag)
            .then(function (response) {
                $scope.JobLocationList = response.data;
            });
        $scope.Flag = "Load All";
    };
    $scope.LoadPlantJobLocation();

    $scope.fixedShitList = [];
    $scope.PlantId = null;
    $scope.GetShiftCbo = function () {
        $scope.fixedShitList = [];
        $scope.PlantId = $.grep($scope.JobLocationList, function (item) {
            return item.SystemID === $scope.employeeNew.JobLocationID;
        })[0].PlantID;

        $http.get('employees/EmployeeInformation/GetCboShiftDefinationByPlant?plantId=' + $scope.PlantId)
            .then(function (response) {
                $scope.fixedShitList = response.data;
            });
    };

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.EmployeeJobLocationForm.$valid) {
            $http({
                method: 'POST',
                url: 'HumanResource/EmployeeJobLocation/Create',
                data: { 'data': $scope.modalNew },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                   
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        }
    };
}