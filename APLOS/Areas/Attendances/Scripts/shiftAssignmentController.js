'use strict';
shiftAssignmentController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'cboService'];
function shiftAssignmentController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, cboService) {
    $rootScope.title = 'Shift Assignment';
    $scope.Action = 'Save';

    $scope.path = 'Attendances/ShiftAssignment/';
    $scope.searchShiftUrl = $scope.path + 'SearchShift';
    $scope.saveUrl = $scope.path + 'Save';

    $scope.master = {
        SystemID: null,     
        IsFix: 'true',
        CheckBox: true
    };
    
    //============================================Load Time Call
    $scope.FixedShiftList = [];
    cboService.getCboFixedShift(function (result) {
        $scope.FixedShiftList = result;
    });


    $scope.RosterMasterList = [];
    cboService.getCboRosterMaster(function (result) {
        $scope.RosterMasterList = result;
    });

    //===================================================func
    $scope.RosterShiftList = [];
    $scope.GetRosterShift = function (rosterid) {
        cboService.getCboRosterShift(rosterid, function (result) {
            $scope.RosterShiftList = result;
        });
    }
    
    //-------------------------------------Search
    $scope.SearchedEmployeesList = [];
    $scope.LoadEmployees = function () {
        try {
            var eDialog = $("#dialogShiftInfo").data("ejDialog");
            eDialog.open();

            $http.get($scope.searchShiftUrl)
                .then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        $scope.SearchedEmployeesList = response.data.LeaveInfo;
                        //eDialog.close();
                    }
                },

                    function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    });


        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    function GetEmp_CSV() {
        try {
            var empids = "''";
            for (var i = 0; i < $scope.EmployeeListNew.length; i++) {
                empids += ",'" + $scope.EmployeeListNew[i].EmpSystemId + "'";
            }
            $scope.master.EmpSystemIds = empids;
        } catch (ex) {
            throw ex;
        }
    }
    $scope.Save = function () {
        try {
            //console.log($scope.EmployeeListNew);
            GetEmp_CSV();
            console.log($scope.master);
            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: { 'master': $scope.master, 'checkbox': $scope.master.CheckBox},
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                     
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, 'failure');
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    
    //-------------------------------------Common
    $scope.Clear = function (obj) {
        ClearFields(obj);
        $scope.master.IsChangeAfterIndividualWeekoff = true;
        $scope.master.ChangeAfterDayLength = 7;
        $scope.detail = [];
    };
    function ClearFields(obj) {
        for (var i in obj) {
            obj[i] = null;
        }
        $scope.EmployeeListNew = [];
        $scope.master = {};
        $scope.ChangeRoster();
       
    }

    $scope.EmployeeList = [];
    $scope.GetEmployeeInformation = function () {

        try {

            if (baseService.isUndefinedOrNull($scope.master.EffectiveDate)) {
                throw 'Please Select EffectiveDate';
            }

            var eDialog = $("#dialogShiftInfo").data("ejDialog");
            eDialog.open();

            $scope.EmployeeList = [];
            $http({
                method: 'GET',
                url: 'Attendances/ShiftAssignment/GetEmployeeInformation?EffectiveDate=' + $scope.master.EffectiveDate
            }).then(function successCallback(response) {               
                    $scope.EmployeeList = response.data;
                });

            
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.EmployeeListNew = [];
    $scope.OK= function () {

        try {
            for (var i = 0; i < $scope.EmployeeList.length; i++) {
                if ($scope.EmployeeList[i].CheckBoxSelect == true) {
                    if (checkDoubleEmployee($scope.EmployeeListNew, $scope.EmployeeList[i].EmpSystemId) === false) {
                        $scope.EmployeeListNew.push($scope.EmployeeList[i]);                        
                    }
                }
            }

            var gridObj = $("#GridNew").data("ejGrid");
            gridObj.refreshContent();

            var eDialog = $("#dialogShiftInfo").data("ejDialog");
            eDialog.close();

            //if ($rootScope.isCollapsed) {
            //    $rootScope.toggle();
            //}

        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    function checkDoubleEmployee(list, Id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].EmpSystemId === Id) {
                return true;
            }
        }
        return false;
    }

    $scope.ChangeRoster = function () {       
        $scope.master.IsFix = 'false';
        $scope.master.FixSystemID = null;       
    }

    $scope.ChangeFixed = function () {
        $scope.master.IsFix = 'true';
        $scope.master.RosterSystemID = null;
        $scope.master.RosterStartShiftID = null;
    }
    
    $scope.refreshTemplateemployee = function (args) {
        $("#headchk").ejCheckBox({ "change": CheckBoxSelectAllEmolyeeWise });
    };

    function CheckBoxSelectAllEmolyeeWise(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }
        var filtered = $("#empInfoGrid").data("ejGrid").getFilteredRecords();
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
        var gridObj = $("#empInfoGrid").data("ejGrid");
        gridObj.refreshContent();
    };

    $scope.remove = function (obj) {
        var gridObj = $("#GridNew").data("ejGrid");

        for (var i = 0; i < $scope.EmployeeListNew.length; i++) {
            if ($scope.EmployeeListNew[i].EmpSystemId === obj.data.EmpSystemId) {
                $scope.EmployeeListNew.splice(i, 1);
                break;
            }
        }
        gridObj.refreshContent();   
    };
};