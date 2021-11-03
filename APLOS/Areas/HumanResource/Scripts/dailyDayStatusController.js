'use strict';
dailyDayStatusController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'toaster', 'cboService', '$controller', '$window'];
function dailyDayStatusController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, toaster, cboService, $controller, $window) {
    $rootScope.title = 'Daily Day Status';
    $scope.path = 'humanresource/dailydaystatus/';

    $scope.dailyComplianceReport = {
        WorkDate: null
    };
    $scope.AttendanceDayStatusList = [];

    $scope.sDepID = null;
    $scope.sSubSecID = null;
    $scope.attdnDate = null;
    $scope.sSecID = null;
    $scope.sLineID = null;
    $scope.dayStatus = null;
   
    $scope.dayStatus = null;
    $scope.dayStatus = null;
    $scope.shift = null;
    $scope.EmployeeCategory = null;
    $scope.Entity = null;


    $scope.departmentList = [];
    cboService.getCboDepartmentByCompanyGroup(null, function (result) {
        $scope.departmentList = result;
    });

    $scope.subSectionList = [];
    $scope.LoadSubSec = function () {
        cboService.getCboSubSectionByCompanyGroup(null, function (result) {
            $scope.subSectionList = result;
        });
    }
    $scope.LoadSubSec();

    $scope.sectionList = [];
    $scope.LoadSec = function () {
        cboService.getCboSectionByCompanyGroup(null, function (result) {
            $scope.sectionList = result;
        });
    }
    $scope.LoadSec();

    $scope.lineList = [];
    $scope.LoadLine = function () {
        cboService.getCboLineByCompanyGroup(null, function (result) {
            $scope.lineList = result;
        });
    };
    $scope.LoadLine();

    $scope.changeSectionByDept = function () {
        $scope.sectionList = [];
        $scope.subSectionList = [];
        $scope.lineList = [];
       
        if (!baseService.isUndefinedOrNull($scope.sDepID) && $scope.sDepID !== 'All') {
            cboService.getSectionCboByDepartment($scope.sDepID, function (result) {
                $scope.sectionList = result;
            });
        } else {
            $scope.LoadSec();
            $scope.LoadSubSec();
            $scope.LoadLine();
        }
    };

    $scope.changeSubSectionBySection = function () {
        cboService.getSubSectionCboBySection($scope.sSecID, function (result) {
            $scope.subSectionList = result;
        });
    };

    cboService.getAttendanceDayStatus(function (result) {
        $scope.AttendanceDayStatusList = result;
    });

    $scope.changeLineBySubSection = function () {
        cboService.getLineCboBySubSection($scope.sLineID, function (result) {
            $scope.lineList = result;
        });
    };

   
    //$scope.GetdailyDayStatusReport = function () {
    //    try {
          
    //        var date = $filter('dateFiltering')($scope.attdnDate, 'dd-MM-yyyy');
    //        var previousdate = $filter('dateFiltering')($scope.Prev, 'dd-MM-yyyy');

    //        if (new Date(previousdate) > new Date(date)) {
    //            throw 'Previous Date cann\'t be greater.';
    //        }
    //        if (previousdate === date) {
    //            throw 'Previous Date cann\'t be Same.';
    //        }
    //        if (baseService.isUndefinedOrNull($scope.attdnDate)) {
    //            throw "Select Date.";
    //        }  

    //        $scope.Dep = $("#Department option:selected").text();
    //        $scope.Sec = $("#Section option:selected").text();

    //        var url = 'humanresource/dailydaystatus/getdailydaystatusreport?workDate=' + $scope.attdnDate + '&PrevWorkDate=' + $scope.Prev + '&sDepID=' + $scope.sDepID + '&sSecID=' + $scope.sSecID + '&sSubSecID=' + $scope.sSubSecID + '&sLineID=' + $scope.sLineID + '&dayStatus=' + $scope.dayStatus + '&Dep=' + $scope.Dep + '&Sec=' + $scope.Sec + '&EmployeeCategory=' + $scope.EmployeeCategory + '&shift=' + $scope.shift + '&Entity=' + $scope.Entity;
    //        $rootScope.report(url);
    //    }
    //    catch (e)
    //    {
    //        ShowResult(e, 'failure');
    //    }
    //};

    //$scope.downloadgriddataUrl = 'GridReports/Download';
    //$scope.GetdailyDayStatusReport = function () {
    //    try {
    //        var date = $filter('dateFiltering')($scope.attdnDate, 'dd-MM-yyyy');
    //        var previousdate = $filter('dateFiltering')($scope.Prev, 'dd-MM-yyyy');

    //        if (new Date(previousdate) > new Date(date)) {
    //            throw 'Previous Date cann\'t be greater.';
    //        }
    //        if (previousdate === date) {
    //            throw 'Previous Date cann\'t be Same.';
    //        }
    //        if (baseService.isUndefinedOrNull($scope.attdnDate)) {
    //            throw "Select Date.";
    //        }

    //        $scope.Dep = $("#Department option:selected").text();
    //        $scope.Sec = $("#Section option:selected").text();
    //        $http({
    //            method: 'POST',
    //            url: 'humanresource/dailydaystatus/getdailydaystatusreport',
    //            data: { 'workDate': $scope.attdnDate, 'PrevWorkDate': $scope.Prev, 'sDepID': $scope.sDepID, 'sSecID': $scope.sSecID, 'sSubSecID': $scope.sSubSecID, 'sLineID': $scope.sLineID, 'dayStatus': $scope.dayStatus, 'Dep': $scope.Dep, 'Sec': $scope.Sec, 'EmployeeCategory': $scope.EmployeeCategory, 'shift': $scope.shift, 'Entity': $scope.Entity },
    //            dataType: 'JSON'
    //        }).then(function successCallback(response) {
    //            if (response.data.Error === true) {
    //                ShowResult(response.data.Message, 'failure');
    //            }
    //            else {                   
    //                    $rootScope.report($scope.downloadgriddataUrl + "?FileName=" + response.data.FileName);

                    
    //            }
    //        }), function errorCallBack(response) {
    //            ShowResult(response.data.Message, 'failure');
    //        }
    //    } catch (e) {
    //        ShowResult(e, "failure");
    //    }
    //};

    $scope.GetdailyDayStatusReport = function () {
        var reportFormat = "Excel";
        try {
            var date = $filter('dateFiltering')($scope.attdnDate, 'dd-MM-yyyy');
            var previousdate = $filter('dateFiltering')($scope.Prev, 'dd-MM-yyyy');

            if (new Date(previousdate) > new Date(date)) {
                throw 'Previous Date cann\'t be greater.';
            }
            if (previousdate === date) {
                throw 'Previous Date cann\'t be Same.';
            }
            if (baseService.isUndefinedOrNull($scope.attdnDate)) {
                throw "Select Date.";
            }

            var DropDownListObj = $("#AttendanceDayStatusList").data("ejDropDownList");
            var dayStatus = DropDownListObj.getSelectedValue();

            $scope.Dep = $("#Department option:selected").text();
            $scope.Sec = $("#Section option:selected").text();
            var file_src = 'humanresource/dailydaystatus/getdailydaystatusreport?reportFormat=' + reportFormat + '&workDate=' + $scope.attdnDate + '&PrevWorkDate=' + $scope.Prev + '&sDepID=' + $scope.sDepID + '&sSecID=' + $scope.sSecID + '&sSubSecID=' + $scope.sSubSecID + '&sLineID=' + $scope.sLineID + '&dayStatus=' + dayStatus + '&Dep=' + $scope.Dep + '&Sec=' + $scope.Sec + '&EmployeeCategory=' + $scope.EmployeeCategory + '&shift=' + $scope.shift + '&Entity=' + $scope.Entity;
            $rootScope.report(file_src);

        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.Clear = function () {
        ClearFields();
    };
    function ClearFields() {

    }
    $scope.Prev = null;
    $scope.PreviousDateEnabled = false;
    $scope.PreviousDate = function () {
        var attdnDate = new Date($scope.attdnDate);
        $scope.PreviousDateEnabled = false;
        $scope.Prev = $filter('date')(new Date(attdnDate.setDate(attdnDate.getDate() - 1)), 'dd-MMM-yyyy');     
        if (!baseService.isUndefinedOrNull($scope.attdnDate)) {
            $scope.PreviousDateEnabled = true;
        }
    };


    $scope.ShiftList = [];
    $scope.getShift = function () {
        $http({
            method: 'GET',
            url: $scope.path + "GetShift",
        }).then(function successCallback(response) {
            $scope.ShiftList = response.data;
        });
    }
    $scope.getShift();
   // GetEmployeeCategoryList


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



    $scope.EntityList = [];
    $scope.getEntity = function () {
        $http({
            method: 'GET',
            url: $scope.path + "GetEntityList",
        }).then(function successCallback(response) {
            $scope.EntityList = response.data;
        });
    }
    $scope.getEntity();

    $scope.DeptList = [];
    $scope.getDept = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'GetDeptListList?EntityId=' + $scope.Entity,
        }).then(function successCallback(response) {
            $scope.DeptList = response.data;
        });
    }
    //$scope.getDept();

}
