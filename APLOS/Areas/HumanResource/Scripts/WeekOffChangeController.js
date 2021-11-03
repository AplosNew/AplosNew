'use strict';
WeekOffChangeController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'cboService'];
function WeekOffChangeController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, cboService)
{
    $rootScope.title = 'Week-Off Change';
    $scope.Action = 'Save';
    $scope.path = 'humanresource/WeekOffChange/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';

    $scope.daylist = ["", "Saturday", "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday"];

    $scope.modelOriginal = {
        EmpSystemID: null, FixSystemID: null, EffectiveDate: null, AlignWithCC: "1", IndividualWeekOff: "0", FstOffDay: null, FstDayLengthType: "Full Day", SndOffDay: null, SndDayLengthType: "Full Day"
    };
    $scope.model = Object.assign({}, $scope.modelOriginal);

    $scope.selectemployee = [];
    $scope.selectedSinglemployee = {};
    /////EMPLOYEE SEARCH/////
    $scope.searchfield = "EmployeeCode"; $scope.searchtext = "";
    $scope.searchByList = [{ 'name': 'Employee Code', 'value': 'EmployeeCode' },
    { 'name': 'Employee Name', 'value': 'EmployeeName' },
    { 'name': 'Department', 'value': 'Department' },
    { 'name': 'Designation', 'value': 'Designation' },
    { 'name': 'Section', 'value': 'Section' },
    { 'name': 'Sub Section', 'value': 'SubSection' }];

    $scope.getAllEmployee = function ()
    {
        try {
            if (baseService.isUndefinedOrNull($scope.model.EffectiveDate)) {
                throw 'Please Select Effective Date';
            }

            $http({
                method: "POST",
                dataType: 'JSON',
                data: { 'column': $scope.searchfield, 'value': $scope.searchtext, 'effectivedate': $scope.model.EffectiveDate },
                url: $scope.path + 'searchEmployees'

            }).then(function successCallback(response) {
                $scope.selectemployee = response.data;

            });
            angular.element(document.querySelector('#ShiftPopUp')).modal('show');
        } catch (e) {
            ShowResult(e, "failure");

        }
      

    }


    $scope.selectSignleEmployee = function (args)
    {
      
        if (baseService.isUndefinedOrNull(args) == false)
        {
            $scope.selectedSinglemployee = args.data;
            $scope.showAttendanceInfo();
        }
        //var eDialog = $("#dialogEmployeeSelect").data("ejDialog");
        //eDialog.close();
        angular.element(document.querySelector('#ShiftPopUp')).modal('hide');

    }

    $scope.closeShiftPopUp = function () {
        angular.element(document.querySelector('#ShiftPopUp')).modal('hide');
    }

    $scope.SaveEmployee = function ()
    {
        try {

            if ($scope.model.AlignWithCC == false) {
                if (baseService.isUndefinedOrNull($scope.model.FstOffDay)) {
                    throw 'Please select first Off Day Date';
                }

            }
         
            var mod = Object.assign({}, $scope.model);

            mod.EmpSystemID = $scope.selectedSinglemployee.Id;
            if (mod.AlignWithCC == "1")
                mod.AlignWithCC = true;
            else
                mod.AlignWithCC = false;

            if (mod.IndividualWeekOff == "1")
                mod.IndividualWeekOff = true;
            else
                mod.IndividualWeekOff = false;


            $http({
                method: "POST",
                dataType: 'JSON',
                data: { 'employeeWeek': mod },
                url: $scope.path + 'SaveEmployee'

            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.selectedSinglemployee.EffectiveDateDesc = $scope.model.EffectiveDateDesc;
                    $scope.showAttendanceInfo();

                }
                });

        } catch (e) {
            ShowResult(e, "failure");
        }
       
    }

    $scope.attendanceinfo = [];
    $scope.showAttendanceInfo = function ()
    {

        try
        {

            $http({
                method: "POST",
                dataType: 'JSON',
                data: { 'employeeid': $scope.selectedSinglemployee.Id, 'EffectiveDate': $scope.model.EffectiveDate },
                url: $scope.path + 'getAttendanceData'

            }).then(function successCallback(response)
            {
                $scope.attendanceinfo = response.data;
            });
        } catch (e)
        {
            ShowResult(e, 'failure');
        }





    }
    $scope.Clear = function ()
    {
        $scope.model = Object.assign({}, $scope.modelOriginal);
        $scope.selectedSinglemployee = {};
        $scope.attendanceinfo = [];
    }
}